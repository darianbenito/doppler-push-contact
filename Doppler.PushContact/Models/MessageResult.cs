using System;

namespace Doppler.PushContact.Models
{
    public class MessageResult
    {
        public Guid MessageId { get; }

        public MessageResult(Guid messageId)
        {
            MessageId = messageId;
        }
    }
}
