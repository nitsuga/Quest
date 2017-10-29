using System;

namespace Quest.Common.Messages.TimedEvent
{
    /// <summary>
    /// Post a new timed event request
    /// </summary>
    [Serializable]
    public class TimedEventRequest : MessageBase
    {
        public string Key;
        public DateTime FireTime;
        public MessageBase Message;
    }
}
