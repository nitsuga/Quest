#pragma warning disable 0169,649
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quest.Common.Messages;
using Quest.Lib.DataModel;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;
using Quest.Lib.Trace;
using Autofac;
using Quest.Common.ServiceBus;
using Quest.Lib.Data;
using Quest.Common.Messages.Routing;
using Quest.Common.Messages.Visual;
using Quest.Lib.Resource;

namespace Quest.Lib.Routing
{
    /// <summary>
    ///     This class generates coverage maps.
    ///     1. It continually generates coverage maps for vehicle coverage
    ///     2. It continually produces incident prediction coverages
    ///     3. Calculates vehicle ETA's to incidents (broadcast as ETAResult)
    ///     4. Calculates vehicle ETA's to standby points (i.e. those on th standby point tracker list) (broadcast as
    ///     ETAResult)
    ///     All coverage maps are
    ///     a) broadcast as CoverageMap object
    ///     b) saved in the database in CoverageMapStore
    ///     c) saved as ArcGrid into directory specified in the Coverage.ExportDirectory variable
    /// </summary>
    public class RoutingManager : ServiceBusProcessor
    {
        #region Public Fields

        /// <summary>
        ///     standard coverage maps generated every now and then
        /// </summary>
        public Dictionary<string, CoverageMap> StandardCoverages = new Dictionary<string, CoverageMap>();

        #endregion

        #region Private Fields


        public string defaultengine { get; set; }
        public string roadSpeedCalculator { get; set; }

        private IDatabaseFactory _dbFactory;
        private bool _stopping;
        private readonly RoutingData _routingdata;
        private readonly ILifetimeScope _scope;

        #endregion

        #region public Methods

        public RoutingManager(
            ILifetimeScope scope,
            IDatabaseFactory dbFactory,
            RoutingData routingdata,
            IServiceBusClient serviceBusClient,
            TimedEventQueue eventQueue,
            MessageHandler msgHandler) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _scope = scope;
            _routingdata = routingdata;
            _dbFactory = dbFactory;
        }

        protected override void OnPrepare()
        {
            MsgHandler.AddHandler<RouteRequest>(RouteRequestHandler);
            MsgHandler.AddHandler<RoutingEngineStatusRequest>(RoutingEngineStatusRequestHandler);
        }

        protected override void OnStart()
        {
            Initialise();
        }

        protected override void OnStop()
        {
            _stopping=true;
        }

        /// <summary>
        ///     initilaise the server with n workers
        /// </summary>
        private void Initialise()
        {
            _stopping = false;

            Logger.Write($"Loading default routing engine {defaultengine}", TraceEventType.Information, "Routing Manager");
            var engine = _scope.ResolveNamed<IRouteEngine>(defaultengine);

            Logger.Write($"Waiting routing engine to load", TraceEventType.Information, "Routing Manager");
            WaitForEngineReady(engine);

            _stopping = false;
        }

        private void WaitForEngineReady(IRouteEngine engine)
        {
            // wait for routing engines to start
            while (!engine.IsReady)
            {
                Thread.Sleep(1000);
            }
        }


        /// <summary>
        /// Get the preferred routing engine and wait for it to be ready
        /// </summary>
        /// <param name="preferredEngine"></param>
        /// <param name="wait"></param>
        /// <returns>the routing engine or null if not found</returns>
        IRouteEngine GetRoutingEngine(string preferredEngine, bool wait=true)
        {
            if (preferredEngine==null)
                preferredEngine = defaultengine;

            var engine = _scope.ResolveNamed<IRouteEngine>(preferredEngine);

            if (engine == null || !wait) return null;
            WaitForEngineReady(engine);
            return engine;
        }

        #endregion

        #region Privates

        private RoutingEngineStatus RoutingEngineStatusRequestHandler(NewMessageArgs t)
        {
            return new RoutingEngineStatus() { Ready = _routingdata.IsInitialised };
        }
        
        private RoutingResponse RouteRequestHandler(NewMessageArgs t)
        {
            try
            {
                Logger.Write("Routing Manager: RouteRequestHandler called", TraceEventType.Information, "Routing Manager");
                var request = t.Payload as RouteRequest;

                if (request == null) return new RoutingResponse { Message = "Invalid request received", Success = false };
                var routingEngine = GetRoutingEngine(request.RoutingEngine);

                if (routingEngine == null)
                    return new RoutingResponse
                    {
                        Message = $"Can't find routing engine {request.RoutingEngine}",
                        Success = false
                    };

                if (request.RoadSpeedCalculator == null)
                    return new RoutingResponse { Message = "Null passed for the speed speed calculator", Success = false };

                var result = routingEngine.CalculateQuickestRoute(request);

                return result;
            }
            catch (Exception ex)
            {
                return new RoutingResponse { Message = $"General error : {ex.Message}", Success = false };
            }
        }

        #endregion
    }
}