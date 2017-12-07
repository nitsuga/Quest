using System;

namespace Quest.LAS.Codec
{
    public class CadInboundMessage
    {
        public byte[] MessageText { get; set; }
        public long SequenceNumber { get; set; }
        public DateTime MdtTimestamp { get; set; }
        public DateTime CadTimestamp { get; set; }
        public int RxQueueSize { get; set; }

    }
}
