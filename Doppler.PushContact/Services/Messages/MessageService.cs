using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services.Messages
{
    public class MessageService : IMessageService
    {
        private readonly MessageServiceSettings _messageServiceSettings;

        public MessageService(IOptions<MessageServiceSettings> messageServiceSettings)
        {
            _messageServiceSettings = messageServiceSettings.Value;
        }

        public async Task<IEnumerable<SendMessageResult>> SendMessage(string title, string body, IEnumerable<string> targetDeviceTokens)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException($"'{nameof(title)}' cannot be null or empty.", nameof(title));
            }

            if (string.IsNullOrEmpty(body))
            {
                throw new ArgumentException($"'{nameof(body)}' cannot be null or empty.", nameof(body));
            }

            var responseBody = await _messageServiceSettings.PushApiUrl
                      .AppendPathSegment("message")
                      .PostJsonAsync(new
                      {
                          notificationTitle = title,
                          notificationBody = body,
                          tokens = targetDeviceTokens
                      })
                     .ReceiveJson<SendMessageResponseContract>();

            return responseBody.Responses.Select(x =>
                {
                    return new SendMessageResult
                    {
                        TargetDeviceToken = x.DeviceToken,
                        IsSuccess = x.IsSuccess,
                        IsValidTargetDeviceToken = !x.IsSuccess && _messageServiceSettings.FatalMessagingErrorCodes.Any(y => y == x.Exception.MessagingErrorCode),
                        NotSuccessErrorDetails = !x.IsSuccess ? $"{nameof(x.Exception.MessagingErrorCode)} {x.Exception.MessagingErrorCode} - {nameof(x.Exception.Message)} {x.Exception.Message}" : null
                    };
                });
        }
    }
}
