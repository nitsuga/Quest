using Quest.Common.ServiceBus;
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

        public override string ToString()
        {
            return $"Meta: c={CorrelationId} rpy={ReplyTo} key={RoutingKey} src={Source} dst={Destination}";
        }
    }

    /// <summary>
    /// every message on the service bus is one of these containing the message
    /// </summary>
    [Serializable]
    public class MessageQueueItem
    {
        public IServiceBusMessage Message;
        public PublishMetaData Metadata;
    }
}