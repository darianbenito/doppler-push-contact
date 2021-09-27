using System.Collections.Generic;

namespace Doppler.PushContact.Services.Messages
{
    public class MessageServiceSettings
    {
        public string PushApiUrl { get; set; }

        public List<int> FatalMessagingErrorCodes { get; set; }
    }
}
