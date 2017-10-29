using Quest.Common.ServiceBus;
using System;

namespace Quest.Common.Messages
{

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