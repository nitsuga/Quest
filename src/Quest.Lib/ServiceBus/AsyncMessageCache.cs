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
            Logger.Write($"ctor::AsyncMessageCache {queue}", "Web");
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

                // create a message handler which we can subscribe and then (importantly) unsubscribe to
                // after the message has been received.
                EventHandler<NewMessageArgs> handler = null;

                handler = (sender, args) =>
                {
                    MsgSourceHandler(sender, args, foundMessage, obj, ref result);
                };
                MsgSource.NewMessage += handler;

                PublishMetaData metadata = new PublishMetaData()
                {
                    CorrelationId = obj.RequestId,
                    ReplyTo = MsgSource.QueueName,
                    RoutingKey = "",
                    Source = MsgSource.QueueName,
                    Destination = ""  ,
                    MsgType = obj.GetType().Name
                };

                // send the message
                MsgSource.Broadcast(obj, metadata);

                // now wait for the response to arrive (via the anonymous event handler above)
                foundMessage.WaitOne(timeout);

                MsgSource.NewMessage -= handler; // Unsubscribe

            });

            return result;
        }

        private void MsgSourceHandler<T>(object who, NewMessageArgs args, AutoResetEvent foundMessage, Request obj, ref T result) where T : class
        {
            // detect our message with the right correlation id and expected type
            if (args.Metadata.CorrelationId == obj.RequestId && args.Payload.GetType() == typeof(T))
            {
                Logger.Write($"{MsgSource.QueueName} {obj.GetType()} got {args.Payload.GetType()} CI='{args.Metadata.CorrelationId}' looking for '{obj.RequestId}'", "Web");
                result = args.Payload as T;
                foundMessage.Set();
            }
            else
            {
                if (args.Metadata.CorrelationId != obj.RequestId)
                    Logger.Write($"Wrong correlation {MsgSource.QueueName} got '{args.Metadata.CorrelationId}' but was looking for '{obj.RequestId}'", "Web");
                else
                    Logger.Write($"Wrong type {MsgSource.QueueName} got {args.Payload.GetType()} but was looking for '{typeof(T)}'", "Web");
            }


        }

        public void BroadcastMessage(IServiceBusMessage request)
        {
            MsgSource.Broadcast(request);
        }
    }
}