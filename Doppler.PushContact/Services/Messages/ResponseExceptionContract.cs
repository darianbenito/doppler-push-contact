namespace Doppler.PushContact.Services.Messages
{
    public class ResponseExceptionContract
    {
        public int MessagingErrorCode { get; set; }

        public string Message { get; set; }
    }
}
