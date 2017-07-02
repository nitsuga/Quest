using System;

namespace Quest.Common.Messages
{
    [Serializable]
    public class XCInbound
    {
        public XCInbound()
        {
        }

        public XCInbound(string data, string channel)
        {
            Data = data;
            Channel = channel;
        }

        public string Data { get; set; }
        public string Channel { get; set; }

        public override string ToString()
        {
            return $"XCInbound {Channel} {Data}";
        }
    }
}