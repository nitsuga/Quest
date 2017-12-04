namespace Quest.Common.Messages.Resource
{
    public class DestinationHistory
    {
        public string DestinationCode;

        public string Callsign;

        public string Message;

        public StatusCode Status;

        public enum StatusCode
        {
            Normal,
            Warning,
        }
    }

}
