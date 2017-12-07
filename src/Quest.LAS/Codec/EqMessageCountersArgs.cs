namespace Quest.LAS.Codec
{
    public class EqMessageCountersArgs
    {
        public int EqTxQueueSize { get; set; }
        public int EqRxQueueSize { get; set; }
        public long OutboundSequenceNumber { get; set; }
        public int InboundMessageCount { get; set; }
        public bool EqMessageReceievedEnabled { get; set; }
    }
}
