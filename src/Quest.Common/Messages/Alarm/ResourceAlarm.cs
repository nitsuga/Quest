using System;

namespace Quest.Lib.ServiceBus.Messages.Alarm
{
    [Serializable]
    public class ResourceAlarm
    {
        public string Callsign { get; set; }
        public string Message { get; set; }
        public bool IsWarning { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }

        public override string ToString()
        {
            return "ResourceAlarm ";
        }
    }
}