using Autofac;
using Quest.Common.Messages;
using Quest.Common.ServiceBus;
using Quest.LAS.Codec;
using Quest.LAS.Messages;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;
using System;

namespace Quest.LAS.Processor
{
    /// <summary>
    /// Listen to MDT messages and convert them into raw ExpressQ messages 
    /// </summary>
    public class ExpressQEncoder : ServiceBusProcessor
    {
        #region Private Fields
        private ILifetimeScope _scope;
        private Encoder _encoder;
        #endregion

        public ExpressQEncoder(
            ILifetimeScope scope,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _scope = scope;
            _encoder = new Codec.Encoder();
        }

        protected override void OnPrepare()
        {
            MsgHandler.AddHandler<DeviceMessage>(MessageHandler);
        }

        protected override void OnStart()
        {
        }

        public Response MessageHandler(NewMessageArgs msg)
        {
            try
            {
                var message = msg.Payload as DeviceMessage;
                if (message != null)
                {
                    var eqmsg = _encoder.Encode(message);
                    if (eqmsg != null)
                        ServiceBusClient.Broadcast(eqmsg);
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
    }
}
