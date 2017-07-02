using System;
using System.Diagnostics;
using Quest.Common.Messages;
using Quest.Common.ServiceBus;

namespace Quest.Lib.ServiceBus
{
    internal class LocalServiceBusEngine
    {
        private static LocalServiceBusEngine _instance;

        private LocalServiceBusEngine()
        {
        }

        public event EventHandler<NewMessageArgs> NewMessage;

        public static LocalServiceBusEngine Bus()
        {
            return _instance ?? (_instance = new LocalServiceBusEngine());
        }

        public void Broadcast(IServiceBusClient sender, IServiceBusMessage message, PublishMetaData metadata)
        {
            Debug.Assert(sender != null);
            Debug.Assert(message != null);
            Debug.Assert(metadata != null);

            if (NewMessage == null) return;
            var delegates = NewMessage.GetInvocationList();
            foreach (var target in delegates)
            {
                var serviceBusClient = target.Target as IServiceBusClient;
                if (serviceBusClient != null && serviceBusClient.QueueName != sender.QueueName)
                    target.DynamicInvoke(target, new NewMessageArgs {Payload = message, Client = sender, Metadata = metadata });
            }
        }
    }
}