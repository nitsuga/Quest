using System;

namespace Quest.Common.Messages
{
    [Serializable]
    public class XCOutbound
    {
        public XCOutbound()
        {
        }

        public XCOutbound(string command, string channel, int XCOutboundId)
        {
            this.command = command;
            this.channel = channel;
            this.XCOutboundId = XCOutboundId;
        }

        public string command { get; set; }
        public string channel { get; set; }
        public int XCOutboundId { get; set; }

        public override string ToString()
        {
            return $"XCOutbound {command} {channel}";
        }
    }
}