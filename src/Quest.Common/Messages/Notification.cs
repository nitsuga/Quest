using System;

namespace Quest.Common.Messages
{
    public class Notification : MessageBase
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
