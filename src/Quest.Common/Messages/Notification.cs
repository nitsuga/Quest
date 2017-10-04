using Newtonsoft.Json;

namespace Quest.Common.Messages
{

    public class NotificationResponse : Response
    {
    }

    public class Notification : Request
    {
        public string Method { get; set; }
        public string Address { get; set; }
        public string Subject { get; set; }
        public INotificationMessage Body { get; set; }
    }

    public interface INotificationMessage
    {
    }

    /// <summary>
    /// This message is sent from the Quest server to one or more devices to indicate that
    /// a new message is available
    /// </summary>

    //public class EventNotification : Notification
    //{
    //    public String Callsign;

    //    public String EventId;

    //}

    /// <summary>
    /// Notification that status has changed for this callsign
    /// </summary>

    //public class StatusNotification : Notification
    //{
    //    public String Callsign;
    //    public String NewStatusGroup;
    //}
}
