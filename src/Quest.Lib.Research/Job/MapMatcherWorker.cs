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
    public class MapMatcherWorker : ServiceBusProcessor
    {
        #region Private Fields
        private ILifetimeScope _scope;
        private MapMatcherUtil _mapMatcherUtil;
        #endregion


        public MapMatcherWorker(
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

            var engine = _scope.ResolveNamed<IRouteEngine>((string)parms.RoutingEngine);
            var matcher = _scope.ResolveNamed<IMapMatcher>((string)parms.MapMatcher);

            int taskid;
            int runid;
            int startrouteid;
            int endrouteid;

            int.TryParse(Configuration["taskid"], out taskid);
            int.TryParse(Configuration["runid"], out runid);
            int.TryParse(Configuration["startrouteid"], out startrouteid);
            int.TryParse(Configuration["endrouteid"], out endrouteid);

            _mapMatcherUtil.RoadMatcherBatchWorker(taskid, startrouteid, endrouteid, runid, matcher, engine, parms);
        }
    }
}

