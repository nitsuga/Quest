using System;

namespace Quest.LAS.Messages
{
    public class SetStatus : ICadMessage
    {
        public int ExternalStatusId { get; set; }        
        public CadStatusOrigin StatusOrigin { get; set; }
        public int StatusValue { get; set; }
        public long SequenceNumber { get; set; }
        public DateTime MessageDateTime { get; set; }
        public bool IsCallsignUpdate { get; set; }
    }
}
