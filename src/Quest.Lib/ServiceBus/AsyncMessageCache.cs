﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Quest.Common.Messages;
using Quest.Lib.Trace;
using Quest.Common.ServiceBus;

namespace Quest.Lib.ServiceBus
{
    public class AsyncMessageCache
    {
        public IServiceBusClient MsgSource;

        public AsyncMessageCache(IServiceBusClient msgSource, string queue)
        {
            MsgSource = msgSource;
            MsgSource.Initialise(queue);
        }

        public async Task<T> SendAndWaitAsync<T>(Request obj, TimeSpan timeout, string DestinationQueue = null) where T : class
        {
            T result = null;

            await Task.Run(() => {
                AutoResetEvent foundMessage = new AutoResetEvent(false);
                // create new id for this message
                obj.RequestId = Guid.NewGuid().ToString();

                // hook up to the incoming message notifications
                MsgSource.NewMessage += (who, args) =>
                {
                    // detect our message with the right correlation id and expected type
                    if (args.Metadata.CorrelationId == obj.RequestId && args.Payload.GetType() == typeof(T))
                    {
                        Logger.Write($"{MsgSource.QueueName} {obj.GetType()} got {args.Payload.GetType()} CI={args.Metadata.CorrelationId} looking for {obj.RequestId}", "Web");
                        result = args.Payload as T;
                        foundMessage.Set();
                    }
                };

                PublishMetaData metadata = new PublishMetaData()
                {
                    CorrelationId = obj.RequestId,
                    ReplyTo = MsgSource.QueueName,
                    RoutingKey = "",
                    Source = MsgSource.QueueName,
                    Destination = ""
                };

                // send the message
                MsgSource.Broadcast(obj, metadata);
                Logger.Write($"Broadcase Message Ok", "Web");

                // now wait for the response to arrive (via the anonymous event handler above)
                foundMessage.WaitOne(timeout);
            });

            return result;
        }

        public void BroadcastMessage(IServiceBusMessage request)
        {
            MsgSource.Broadcast(request);
        }
    }
}