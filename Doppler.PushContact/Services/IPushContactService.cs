using Doppler.PushContact.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services
{
    public interface IPushContactService
    {
        Task AddAsync(PushContactModel pushContactModel);

        Task UpdateEmailAsync(string deviceToken, string email);

        Task<IEnumerable<PushContactModel>> GetAsync(PushContactFilter pushContactFilter);

        Task<long> DeleteByDeviceTokenAsync(IEnumerable<string> deviceTokens);

        Task AddHistoryEventsAsync(IEnumerable<PushContactHistoryEvent> pushContactHistoryEvents);

        Task<IEnumerable<string>> GetAllDeviceTokensByDomainAsync(string domain);

        Task<IEnumerable<PushContactHistoryEvent>> GetHistoryEventsAsync(string domain, Guid messageId);
    }
}
