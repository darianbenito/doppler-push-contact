using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services.Messages
{
    public interface IMessageService
    {
        Task<IEnumerable<SendMessageResult>> SendMessage(string title, string body, IEnumerable<string> targetDeviceTokens);
    }
}
