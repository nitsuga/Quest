using System;

namespace Quest.Common.Messages
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

    [Serializable]
    public class TimedEventTimeChange : MessageBase
    {
        public DateTime Time;
    }
}
