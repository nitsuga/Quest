using System;

namespace Quest.Common.Messages.TimedEvent
{

    [Serializable]
    public class TimedEventTimeChange : MessageBase
    {
        public DateTime Time;
    }
}
