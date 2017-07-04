using Autofac;
using Quest.Common.Messages;
using Quest.Common.ServiceBus;
using Quest.Lib.MapMatching.RouteMatcher;
using Quest.Lib.Processor;
using Quest.Lib.Routing;
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Lib.Research.Job
{
    public class MapMatcherAll : ServiceBusProcessor
    {
        #region Private Fields
        private ILifetimeScope _scope;
        private MapMatcherUtil _mapMatcherUtil;
        #endregion


        public MapMatcherAll(
            ILifetimeScope scope,
            MapMatcherUtil mapMatcherUtil,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _scope = scope;
            _mapMatcherUtil = mapMatcherUtil;
        }

        protected override void OnPrepare()
        {
        }

        bool started = false;

        protected override void OnStart()
        {
            if (!started)
                new TaskFactory().StartNew(() =>
                {
                    DoMapMatch();
                });
            started = true;
        }

        private void DoMapMatch()
        {
            var args = Configuration["args"];

            dynamic parms = ExpandoUtils.MakeExpandoFromString(args);

            var request = new MapMatcherMatchAllRequest
            {
                MapMatcher = parms.MapMatcher,
                RoutingEngine = parms.RoutingEngine,
                RoutingData = parms.RoutingData,
                Workers = parms.Workers,
                Parameters = args
            };

            _mapMatcherUtil.MapMatcherMatchAll(request);
        }
    }
}

