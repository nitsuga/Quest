using Autofac;
using Quest.Common.ServiceBus;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;

namespace Quest.LAS.Processor
{
    /// <summary>
    /// Listen to MDT messages and convert them into raw ExpressQ messages 
    /// </summary>
    public class ExpressQEncoder : ServiceBusProcessor
    {
        #region Private Fields
        private ILifetimeScope _scope;
        #endregion

        public ExpressQEncoder(
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
