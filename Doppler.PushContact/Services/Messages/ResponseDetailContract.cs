namespace Doppler.PushContact.Services.Messages
{
    public class ResponseDetailContract
    {
        public string MessageId { get; set; }

        public bool IsSuccess { get; set; }

        public ResponseExceptionContract Exception { get; set; }

        public string DeviceToken { get; set; }
    }
}
