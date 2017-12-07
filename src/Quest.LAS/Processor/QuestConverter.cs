using Autofac;
using Quest.Common.ServiceBus;
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
        }

        protected override void OnStart()
        {
            Run();
        }

        public void Run()
        {
        }
    }
}
