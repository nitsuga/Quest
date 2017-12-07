using System.Collections.Generic;

namespace Quest.LAS.Codec
{
    public interface ICadInterface 
    {
        ICadMessageCodec CadMessageCodec { get; set; }

        int SendOutboundMessage(byte[] messageText, string messageType);
        List<CadInboundMessage> ReadInboundMessages();
        void PurgeTimestamps();

        bool EqMessageReceivedEnabled { get; set; }

        bool DuplicateSequenceNumber { get; set; }
        int OutboundTimestampDelta { get; set; }
        int OutboundSequenceNumber { get; set; }
        int SignalStrengh1 { get; set; }
        int SignalStrengh0 { get; set; }
        void ModifyOutboundSequenceNumber(int modifyByNumber);

        event EqNetworkSwitchDelegate EqNetworkSwitch;
        event EqMessageReceivedDelegate EqMessageReceived;
        event EqMessageCountersDelegate EqMessageCounters;
        event DuplicateSequenceDelegate DuplicateSequenceSent;
    }
}
