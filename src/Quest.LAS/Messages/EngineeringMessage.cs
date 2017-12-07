using Quest.LAS.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quest.LAS.Messages
{
    public class EngineeringMessage : ICadMessage
    {
        public InboundESMessageTypeEnum InboundEsMessageType { get; set; }
        public DateTime CadTimestamp { get; set; }
        public int SequenceNumber { get; set; }

        public byte[] Data { get; set; }
    }
}
