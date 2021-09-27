using System;

namespace Doppler.PushContact.Models
{
    public class PushContactHistoryEvent
    {
        public Guid MessageId { get; set; }

        public string DeviceToken { get; set; }

        public bool SentSuccess { get; set; }

        public DateTime EventDate { get; set; }

        public string Details { get; set; }
    }
}
