namespace Doppler.PushContact.Services.Messages
{
    public class SendMessageResult
    {
        public string TargetDeviceToken { get; set; }

        public bool IsSuccess { get; set; }

        public bool IsValidTargetDeviceToken { get; set; }

        public string NotSuccessErrorDetails { get; set; }
    }
}
