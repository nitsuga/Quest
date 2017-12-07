using Quest.LAS.Messages;
using System;

namespace Quest.LAS.Codec
{
    public class StatusUpdateEventArgs : EventArgs
    {
        public CadStatusOrigin StatusOrigin { get; set; }
        public int StatusValue { get; set; }
        public long SequenceNumber { get; set; }
        public DateTime MessageDateTime { get; set; }
        public bool IsCallsignUpdate { get; set; }
    }
}
