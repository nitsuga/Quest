using Quest.LAS.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quest.LAS.Codec
{
    public class EngineeringMessageArgs : EventArgs
    {
        public InboundESMessageTypeEnum InboundEsMessageType { get; set; }
        public DateTime CadTimestamp { get; set; }
        public int SequenceNumber { get; set; }

        public byte[] Data { get; set; }
    }
}
