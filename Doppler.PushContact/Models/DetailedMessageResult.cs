using System;

namespace Doppler.PushContact.Models
{
    public class DetailedMessageResult : MessageResult
    {
        public int Sent { get; }

        public int Delivered { get; }

        public int NotDelivered => Sent - Delivered;

        public DetailedMessageResult(Guid messageId, int sent, int delivered) : base(messageId)
        {
            Sent = sent;
            Delivered = delivered;
        }
    }
}
