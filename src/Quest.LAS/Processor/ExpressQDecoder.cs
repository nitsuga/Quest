using Autofac;
using Quest.Common.Messages;
using Quest.Common.ServiceBus;
using Quest.LAS.Codec;
using Quest.LAS.Messages;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;
using System;
using System.Collections.Generic;


namespace Quest.LAS.Processor
{
    /// <summary>
    /// Listen to raw ExpressQ messages arriving on the bus and convert them to generic MDT messages
    /// CadOutboundMessage --> DeviceMessage
    /// </summary>
    public class ExpressQDecoder : ServiceBusProcessor
    {
        #region Private Fields
        private ILifetimeScope _scope;
        private Decoder _decoder;
        #endregion

        public ExpressQDecoder(
            ILifetimeScope scope,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _scope = scope;
            _decoder = new Codec.Decoder();
        }

        protected override void OnPrepare()
        {
            MsgHandler.AddHandler<CadOutboundRawMessage>(CadMessageHandler);
        }

        protected override void OnStart()
        {
        }

        public Response CadMessageHandler(NewMessageArgs msg)
        {
            try
            {
                var message = msg.Payload as CadOutboundRawMessage;
                if (message != null)
                {
                    var eqmsg = _decoder.DecodeCadMessage(message);
                    if (eqmsg!=null)
                        ServiceBusClient.Broadcast(eqmsg);
                }
            }
            catch(Exception ex)
            {

            }
            return null;
        }
    }
}
