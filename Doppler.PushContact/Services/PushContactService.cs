using Doppler.PushContact.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Doppler.PushContact.Services
{
    public class PushContactService : IPushContactService
    {
        private readonly IMongoClient _mongoClient;
        private readonly IOptions<PushContactMongoContextSettings> _pushContactMongoContextSettings;
        private readonly IDeviceTokenValidator _deviceTokenValidator;
        private readonly ILogger<PushContactService> _logger;

        public PushContactService(
            IMongoClient mongoClient,
            IOptions<PushContactMongoContextSettings> pushContactMongoContextSettings,
            IDeviceTokenValidator deviceTokenValidator,
            ILogger<PushContactService> logger)
        {

            _mongoClient = mongoClient;
            _pushContactMongoContextSettings = pushContactMongoContextSettings;
            _deviceTokenValidator = deviceTokenValidator;
            _logger = logger;
        }

        public async Task AddAsync(PushContactModel pushContactModel)
        {
            if (pushContactModel == null)
            {
                throw new ArgumentNullException(nameof(pushContactModel));
            }

            if (!await _deviceTokenValidator.IsValidAsync(pushContactModel.DeviceToken))
            {
                throw new ArgumentException($"{nameof(pushContactModel.DeviceToken)} is not valid");
            }

            var now = DateTime.UtcNow;
            var key = ObjectId.GenerateNewId(now).ToString();

            var pushContactDocument = new BsonDocument {
                { PushContactDocumentProps.IdPropName, key },
                { PushContactDocumentProps.DomainPropName, pushContactModel.Domain },
                { PushContactDocumentProps.DeviceTokenPropName, pushContactModel.DeviceToken },
                { PushContactDocumentProps.EmailPropName, string.IsNullOrEmpty(pushContactModel.Email) ? BsonNull.Value : pushContactModel.Email },
                { PushContactDocumentProps.DeletedPropName, false },
                { PushContactDocumentProps.ModifiedPropName, now }
            };

            try
            {
                await PushContacts.InsertOneAsync(pushContactDocument);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @$"Error inserting {nameof(pushContactModel)}
with following {nameof(pushContactModel.DeviceToken)}: {pushContactModel.DeviceToken}");

                throw;
            }
        }

        public async Task UpdateEmailAsync(string deviceToken, string email)
        {
            if (deviceToken == null)
            {
                throw new ArgumentNullException(nameof(deviceToken));
            }

            if (email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }

            var filter = Builders<BsonDocument>.Filter.Eq(PushContactDocumentProps.DeviceTokenPropName, deviceToken)
                & Builders<BsonDocument>.Filter.Eq(PushContactDocumentProps.DeletedPropName, false);

            var updateDefinition = Builders<BsonDocument>.Update
                .Set(PushContactDocumentProps.EmailPropName, email)
                .Set(PushContactDocumentProps.ModifiedPropName, DateTime.UtcNow);

            try
            {
                await PushContacts.UpdateOneAsync(filter, updateDefinition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @$"Error updating {nameof(PushContactModel)}
with {nameof(deviceToken)} {deviceToken}. {PushContactDocumentProps.EmailPropName} can not be updated with following value: {email}");

                throw;
            }
        }

        public async Task<IEnumerable<PushContactModel>> GetAsync(PushContactFilter pushContactFilter)
        {
            if (pushContactFilter == null)
            {
                throw new ArgumentNullException(nameof(pushContactFilter));
            }

            if (string.IsNullOrEmpty(pushContactFilter.Domain))
            {
                throw new ArgumentException(
                    $"'{nameof(pushContactFilter.Domain)}' cannot be null or empty", nameof(pushContactFilter.Domain));
            }

            if (pushContactFilter.ModifiedFrom > pushContactFilter.ModifiedTo)
            {
                throw new ArgumentException(
                    $"'{nameof(pushContactFilter.ModifiedFrom)}' cannot be greater than '{nameof(pushContactFilter.ModifiedTo)}'");
            }

            var FilterBuilder = Builders<BsonDocument>.Filter;

            var filter = FilterBuilder.Eq(PushContactDocumentProps.DomainPropName, pushContactFilter.Domain);

            if (pushContactFilter.Email != null)
            {
                filter &= FilterBuilder.Eq(PushContactDocumentProps.EmailPropName, pushContactFilter.Email);
            }

            if (pushContactFilter.ModifiedFrom != null)
            {
                filter &= FilterBuilder.Gte(PushContactDocumentProps.ModifiedPropName, pushContactFilter.ModifiedFrom);
            }

            if (pushContactFilter.ModifiedTo != null)
            {
                filter &= FilterBuilder.Lte(PushContactDocumentProps.ModifiedPropName, pushContactFilter.ModifiedTo);
            }

            filter &= !FilterBuilder.Eq(PushContactDocumentProps.DeletedPropName, true);

            try
            {
                var pushContactsFiltered = await (await PushContacts.FindAsync<BsonDocument>(filter)).ToListAsync();

                return pushContactsFiltered
                    .Select(x =>
                    {
                        return new PushContactModel
                        {
                            Domain = x.GetValue(PushContactDocumentProps.DomainPropName, null)?.AsString,
                            DeviceToken = x.GetValue(PushContactDocumentProps.DeviceTokenPropName, null)?.AsString,
                            Email = x.GetValue(PushContactDocumentProps.EmailPropName, null)?.AsString
                        };
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting {nameof(PushContactModel)}s");

                throw;
            }
        }

        public async Task<long> DeleteByDeviceTokenAsync(IEnumerable<string> deviceTokens)
        {
            if (deviceTokens == null || !deviceTokens.Any())
            {
                throw new ArgumentException(
                    $"'{nameof(deviceTokens)}' cannot be null or empty", nameof(deviceTokens));
            }

            var filter = Builders<BsonDocument>.Filter.AnyIn(PushContactDocumentProps.DeviceTokenPropName, deviceTokens);

            var update = new BsonDocument("$set", new BsonDocument
                {
                    { PushContactDocumentProps.DeletedPropName, true },
                    { PushContactDocumentProps.ModifiedPropName, DateTime.UtcNow }
                });

            try
            {
                var result = await PushContacts.UpdateManyAsync(filter, update);

                return result.IsModifiedCountAvailable ? result.ModifiedCount : default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting {nameof(PushContactModel)}s");

                throw;
            }
        }

        public async Task AddHistoryEventsAsync(IEnumerable<PushContactHistoryEvent> pushContactHistoryEvents)
        {
            if (pushContactHistoryEvents == null || !pushContactHistoryEvents.Any())
            {
                throw new ArgumentException(
                    $"'{nameof(pushContactHistoryEvents)}' cannot be null or empty", nameof(pushContactHistoryEvents));
            }

            var now = DateTime.UtcNow;

            var updateRequest = pushContactHistoryEvents
                .Select(x =>
                {
                    var historyEvent = new BsonDocument {
                        { PushContactDocumentProps.HistoryEvents_SentSuccessPropName, x.SentSuccess },
                        { PushContactDocumentProps.HistoryEvents_EventDatePropName, x.EventDate },
                        { PushContactDocumentProps.HistoryEvents_InsertedDatePropName, now },
                        { PushContactDocumentProps.HistoryEvents_DetailsPropName, x.Details }
                    };

                    var filter = Builders<BsonDocument>.Filter.Eq(PushContactDocumentProps.DeviceTokenPropName, x.DeviceToken);

                    var update = Builders<BsonDocument>.Update.Push(PushContactDocumentProps.HistoryEventsPropName, historyEvent);

                    return new UpdateOneModel<BsonDocument>(filter, update);
                });

            try
            {
                await PushContacts.BulkWriteAsync(updateRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding {nameof(PushContactHistoryEvent)}s");

                throw;
            }
        }

        public async Task<IEnumerable<string>> GetAllDeviceTokensByDomainAsync(string domain)
        {
            var FilterBuilder = Builders<BsonDocument>.Filter;

            var filter = FilterBuilder.Eq(PushContactDocumentProps.DomainPropName, domain)
                        & !FilterBuilder.Eq(PushContactDocumentProps.DeletedPropName, true);

            var options = new FindOptions<BsonDocument>
            {
                Projection = Builders<BsonDocument>.Projection
                            .Include(PushContactDocumentProps.DeviceTokenPropName)
                            .Exclude(PushContactDocumentProps.IdPropName)
            };

            try
            {
                var pushContactsFiltered = await (await PushContacts.FindAsync(filter, options)).ToListAsync();

                return pushContactsFiltered
                    .Select(x => x.GetValue(PushContactDocumentProps.DeviceTokenPropName, null)?.AsString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting {nameof(PushContactModel)}s by {nameof(domain)} {domain}");

                throw;
            }
        }

        private IMongoCollection<BsonDocument> PushContacts
        {
            get
            {
                var database = _mongoClient.GetDatabase(_pushContactMongoContextSettings.Value.DatabaseName);
                return database.GetCollection<BsonDocument>(_pushContactMongoContextSettings.Value.PushContactsCollectionName);
            }
        }
    }
}
