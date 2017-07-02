using System;
using Quest.Common.Messages;

namespace Quest.Common.ServiceBus
{

    public interface IServiceBusClient
    {
        void Initialise(string queueName);

        string QueueName { get; set; }

        event EventHandler<NewMessageArgs> NewMessage;

        void Send(IServiceBusMessage message, string queue);

        void Broadcast(IServiceBusMessage message);

        void Broadcast(IServiceBusMessage message, PublishMetaData metadata);

        void Stop();
    }


    public class NewMessageArgs : EventArgs
    {
        public PublishMetaData Metadata;
        public IServiceBusMessage Payload;
        public IServiceBusClient Client;
    }
}