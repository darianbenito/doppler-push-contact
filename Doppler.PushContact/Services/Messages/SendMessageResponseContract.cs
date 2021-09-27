using System.Collections.Generic;

namespace Doppler.PushContact.Services.Messages
{
    public class SendMessageResponseContract
    {
        public IEnumerable<ResponseDetailContract> Responses { get; set; }

        public int SuccessCount { get; set; }

        public int FailureCount { get; set; }
    }
}
