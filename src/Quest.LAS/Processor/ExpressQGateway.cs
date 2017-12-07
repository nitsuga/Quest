using Autofac;
using Quest.Common.ServiceBus;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;

namespace Quest.LAS.Processor
{
    /// <summary>
    /// Read and write files in ExpressQ directories
    /// Emits CadInboundMessage
    /// Listens for 
    /// </summary>
    public class ExpressQGateway : ServiceBusProcessor
    {
        #region Private Fields
        private ILifetimeScope _scope;
        #endregion

        public ExpressQGateway(
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
