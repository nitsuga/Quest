using System;
using Quest.Common.Messages;

namespace Quest.Common.ServiceBus
{

    public interface IServiceBusClient
    {
        void Initialise(string queueName);

        string QueueName { get; set; }

        event EventHandler<NewMessageArgs> NewMessage;

        void Broadcast(IServiceBusMessage message);

        void Broadcast(IServiceBusMessage message, PublishMetaData metadata);

        /// <summary>
        /// Sends a message to a destination queue and qaits for a response
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="destqueue"></param>
        /// <param name="srcqueue"></param>
        /// <returns></returns>
        T SendMessageAndWait<T>(IServiceBusMessage message, string destqueue, string srcqueue = null) where T : class;

        void Stop();
    }


    public class NewMessageArgs : EventArgs
    {
        public PublishMetaData Metadata;
        public IServiceBusMessage Payload;
        public IServiceBusClient Client;
    }
}