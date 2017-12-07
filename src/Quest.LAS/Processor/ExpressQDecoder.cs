using Autofac;
using Quest.Common.ServiceBus;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quest.LAS.Processor
{
    /// <summary>
    /// Listen to raw ExpressQ messages arriving on the bus and convert them to generic MDT messages
    /// </summary>
    public class ExpressQDecoder : ServiceBusProcessor
    {
        #region Private Fields
        private ILifetimeScope _scope;
        #endregion

        public ExpressQDecoder(
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
