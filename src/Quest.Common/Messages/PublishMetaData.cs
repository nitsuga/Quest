using System;

namespace Quest.Common.Messages
{
    /// <summary>
    /// Additional information for each message
    /// </summary>
    [Serializable]
    public class PublishMetaData
    {
        public string CorrelationId;
        public string ReplyTo;
        public string RoutingKey;
        public string Source;
        public string Destination;
        public string MsgType;

        public override string ToString()
        {
            return $"Meta: type={MsgType} c={CorrelationId} rpy={ReplyTo} key={RoutingKey} src={Source} dst={Destination}";
        }
    }
}