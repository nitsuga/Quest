using Quest.LAS.Messages;
using System;

namespace Quest.LAS.Codec
{
    public class IncidentUpdateEventArgs : EventArgs
    {
        public IncidentUpdate IncidentUpdate { get; set; }
        public long SequenceNumber { get; set; }
        public DateTime MessageDateTime { get; set; }
        public bool Completed { get; set; }
    }
}
