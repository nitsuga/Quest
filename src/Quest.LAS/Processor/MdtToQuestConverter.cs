using Autofac;
using Quest.Common.Messages;
using Quest.Common.ServiceBus;
using Quest.LAS.Messages;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;

namespace Quest.LAS.Processor
{
    /// <summary>
    /// Convert MDT messages to Quest Device Messages
    /// </summary>
    public class MdtToQuestConverter : ServiceBusProcessor
    {
        #region Private Fields
        private ILifetimeScope _scope;
        #endregion

        public MdtToQuestConverter(
            ILifetimeScope scope,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _scope = scope;
        }

        protected override void OnPrepare()
        {
            MsgHandler.AddHandler<DeviceMessage>(MessageHandler);

        }

        protected override void OnStart()
        {
        }

        private Response MessageHandler(NewMessageArgs arg)
        {
            var msg = arg.Payload as DeviceMessage;

            if (msg != null)
            {
                // hold messages in a cache

            }
            return null;
        }

    }
}
