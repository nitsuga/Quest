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
    public class QuestConverter : ServiceBusProcessor
    {
        #region Private Fields
        private ILifetimeScope _scope;
        #endregion

        public QuestConverter(
            ILifetimeScope scope,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _scope = scope;
        }

        protected override void OnPrepare()
        {
            MsgHandler.AddHandler<AdminMessage>(AdminMessageHandler);
            MsgHandler.AddHandler<IncidentCancellation>(AdminMessageHandler);
            MsgHandler.AddHandler<IncidentUpdate>(AdminMessageHandler);
            MsgHandler.AddHandler<SetStatus>(AdminMessageHandler);
            MsgHandler.AddHandler<GeneralMessage>(AdminMessageHandler);
            MsgHandler.AddHandler<CallsignUpdate>(AdminMessageHandler);
            MsgHandler.AddHandler<AdminMessage>(AdminMessageHandler);
            MsgHandler.AddHandler<AdminMessage>(AdminMessageHandler);

        }

        protected override void OnStart()
        {
        }

        private Response AdminMessageHandler(NewMessageArgs arg)
        {
            var msg = arg.Payload as AdminMessage;

            if (msg != null)
            {
            }
            return null;
        }

    }
}
