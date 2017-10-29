using Newtonsoft.Json;

namespace Quest.Common.Messages.Notification
{

    public class Notification : Request
    {
        public string Method { get; set; }
        public string Address { get; set; }
        public string Subject { get; set; }
        public INotificationMessage Body { get; set; }
    }
}
