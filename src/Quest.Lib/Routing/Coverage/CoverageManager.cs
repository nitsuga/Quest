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
using Trigger.NET;
using Trigger.NET.Cron;
using Trigger.NET.FluentAPI;

namespace Quest.Lib.Routing.Coverage
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
    public class CoverageManager : ServiceBusProcessor
    {
        #region Public Fields

        public int resRange { get; set; } = 450;
        public string defaultengine { get; set; }
        public int tilesize { get; set; } = 250;
        public bool doEta { get; set; } = false;
        public string roadSpeedCalculator { get; set; }
        public int enrFrequencySeconds { get; set; } = 10;
        public int resFrequencySeconds { get; set; } = 10;
        public int incFrequencySeconds { get; set; } = 60;
        
        #endregion

        #region Private Fields
        /// <summary>
        ///     standard coverage maps generated every now and then
        /// </summary>
        private Dictionary<string, CoverageMap> _coverages = new Dictionary<string, CoverageMap>();

        private const string ResourceHoles = "Resource Holes";
        private const string CombinedCoverage = "Combined Coverage";
        private const string IncidentCoverage = "Expected Incidents";
        //private const string StandbyCoverage = "Standby Coverage";
        //private const string StandbyCompliance = "Standby Compliance";

        private bool _stopping;
        private CoverageMap _operationalArea;
        private readonly RoutingData _routingdata;
        private readonly ILifetimeScope _scope;
        private Scheduler _scheduler = new Scheduler();

        /// <summary>
        ///     list of vehicle coverage trackers
        /// </summary>
        private readonly List<VehicleCoverageTracker> _vehicleCoverageTrackerList = new List<VehicleCoverageTracker>();
        private IDatabaseFactory _dbFactory;
        private EtaCalculator _etaCalculator;
        private CoverageMapManager _coverageMapManager;
        private IResourceStore _resStore;
        private IRouteEngine _routingEngine;
        private RoutingData _routingData;

        #endregion

        #region public Methods

        public CoverageManager(
            ILifetimeScope scope,
            IDatabaseFactory dbFactory,
            RoutingData data,
            EtaCalculator etaCalculator,
            RoutingData routingdata,
            IResourceStore resStore,
            IServiceBusClient serviceBusClient,
            TimedEventQueue eventQueue,
            CoverageMapManager coverageMap,
            MessageHandler msgHandler) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _routingData = data;
            _scope = scope;
            _routingdata = routingdata;
            _dbFactory = dbFactory;
            _coverageMapManager = coverageMap;
            _etaCalculator = etaCalculator;
            _resStore = resStore;
        }

        protected override void OnPrepare()
        {
            Initialise();

            MsgHandler.AddHandler<CustomCoverageRequest>(CustomCoverageRequestHandler);
            MsgHandler.AddHandler<GetCoverageRequest>(GetCoverageHandler);
        }

        protected override void OnStart()
        {
            Task.Run( () => Run());
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

            Logger.Write("Coverage Manager Initialising", TraceEventType.Information, "Routing Manager");

            Logger.Write($"Loading default routing engine {defaultengine}", TraceEventType.Information, "Routing Manager");
            _routingEngine = _scope.ResolveNamed<IRouteEngine>(defaultengine);

            _operationalArea = _coverageMapManager.GetOperationalArea(tilesize);
        }

        private void Run()
        {
            //var jobId = _scheduler.AddJob<UpdateAvailability>(cfg => cfg.UseCron(me.Cron).WithParameter(me));

            Logger.Write($"Waiting routing engine to load", TraceEventType.Information, "Routing Manager");
            WaitForEngineReady(_routingEngine);

            // build list and start tracker engines
            MakeTrackerList();

            if (_operationalArea!=null)
            {
                // start standard coverage calculator
                var t1 = new Task(CalculateStandardCoverages );
                t1.Start();
            }
            else
                Logger.Write($"Coverage Tracking turned off", TraceEventType.Information, "Routing Manager");

            if (doEta)
            {
                var t2 = new Task(CalculateEta);
                t2.Start();
            }
            else
                Logger.Write($"ETA Tracking turned off", TraceEventType.Information, "Routing Manager");

            Logger.Write("Routing Manager Started", TraceEventType.Information, "Routing Manager");

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

        private void CalculateEta()
        {
            try
            {
                var counter = 0;

                Logger.Write("Routing Manager: CalculateEta task started", TraceEventType.Information, "Routing Manager");

                do
                {
                    counter++;
                    Thread.Sleep(1000);

                    try
                    {
                        if (!_stopping && counter % enrFrequencySeconds == 0)
                        {
                            var routingEngine = GetRoutingEngine(defaultengine);
                            _etaCalculator.CalculateEnrouteTime(routingEngine, _routingdata, roadSpeedCalculator);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Write(ex);
                    }

                } while (!_stopping);
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

        /// <summary>
        ///     method that calculates coverages on a continuous loop
        /// </summary>
        private void CalculateStandardCoverages()
        {
            try
            {
                var counter = 0;
                CoverageMap incCoverage = null;

                Logger.Write("Routing Manager: CalculateStandardCoverages task started", TraceEventType.Information, "Routing Manager");


                // build list and start tracker engines
                MakeTrackerList();

                // make sure WeakReference have default maps set up
                AddCoverage(_coverages, new CoverageMap { Code = "INC", Name = IncidentCoverage});
                AddCoverage(_coverages, new CoverageMap { Code = "COV", Name = CombinedCoverage});
                AddCoverage(_coverages, new CoverageMap { Code = "HOL", Name = ResourceHoles});
                //AddCoverage(_coverages, new CoverageMap { Code = "INC", Name = StandbyCoverage});
                //AddCoverage(_coverages, new CoverageMap { Code = "INC", Name = StandbyCompliance});

                do
                {
                    counter++;
                    Thread.Sleep(1000);

                    try
                    {
                        if (resFrequencySeconds>0 && !_stopping && counter% resFrequencySeconds == 0)
                        {
                            CalculateResCoverages(defaultengine, _coverages, _vehicleCoverageTrackerList, incCoverage);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Write(ex);
                    }

                    try
                    {
                        if (incFrequencySeconds>0 && !_stopping && ((counter % incFrequencySeconds) == 0 || incCoverage == null))
                        {
                            incCoverage = CalculateIncCoverages(_coverages);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Write(ex);
                    }


                } while (!_stopping);
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
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

        /// <summary>
        ///     calculate a list of custom coverages based on the standard list but using a vehicle position overrides list
        /// </summary>
        /// <param name="overrides"></param>
        /// <param name="engine"></param>
        /// <returns></returns>
        public List<TrialCoverageResponse> CalculateCustomCoverages(List<VehicleOverride> overrides, string engine)
        {
            List<TrialCoverageResponse> results = null;

            try
            {
                results = CalculateTrialCoverages(_vehicleCoverageTrackerList, overrides, _coverages, engine);
            }
            catch (Exception ex)
            {
                Logger.Write($"CalculateCustomCoverages: failed {ex}",
                    TraceEventType.Information, "Routing Manager");
                Logger.Write(ex);
            }
            return results;
        }

        #endregion

        #region Privates

        private GetCoverageResponse GetCoverageHandler(NewMessageArgs t)
        {
            var request = t.Payload as GetCoverageRequest;
            if (request != null)
            {
                CoverageMap data;
                _coverages.TryGetValue(request.Code, out data);

                if (data != null)
                {                        
                    var heatmap=GetHeatmap(data);
                    var r = new GetCoverageResponse
                    {
                        Map = heatmap
                    };

                    Logger.Write("Routing Manager: GetCoverageHandler returning GetCoverageResponse", TraceEventType.Information, "Routing Manager");

                    return r;
                }

                return null;
            }

            var result = new GetCoverageResponse { Message = "Unable to unpack the request", Success = false };
            return result;
        }

        /// <summary>
        /// Get a standard heatmap from an internal coverage object
        /// </summary>
        /// <param name="results"></param>
        /// <param name="vehicleType"></param>
        /// <returns></returns>
        private Heatmap GetHeatmap(CoverageMap results)
        {
            var offset = LatLongConverter.OSRefToWGS84(results.OffsetX, results.OffsetY);

            var blocksize = LatLongConverter.OSRefToWGS84(results.OffsetX + results.Blocksize, results.OffsetY + results.Blocksize);

            var heatmap = new Heatmap
            {
                lon = offset.Longitude,
                lat = offset.Latitude,
                map = results.Data,
                cols = results.Columns,
                rows = results.Rows,
                lonBlocksize = blocksize.Longitude - offset.Longitude,
                latBlocksize = blocksize.Latitude - offset.Latitude,
                Name = results.Name,
                Code = results.Code
            };

            return heatmap;

        }

        private Response CustomCoverageRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as CustomCoverageRequest;
            if (request != null)
            {
                var result = new CustomCoverageResponse { id = request.id };

                var overrides = string.Join(",", request.overrides.Select(x => x.Callsign).ToArray());

                var incCoverage = _coverages[IncidentCoverage];

                if (incCoverage == null)
                    Logger.Write(
                        "CustomCoverage: failed as incident coverage has not been calculated yet ", TraceEventType.Information,
                        "Routing Manager");
                else
                {
                    result.results = CalculateCustomCoverages(request.overrides, request.RoutingEngine);
                    Logger.Write($"CustomCoverage: CalculateCustomCoverages overrides = {overrides} ",
                        TraceEventType.Information, "Routing Manager");
                }
                return result;
            }
            return null;
        }

        /// <summary>
        ///     Builds a list of objects that each individual manage calculating the coverage of a set of vehicle types
        /// </summary>
        private void MakeTrackerList()
        {
            Logger.Write("Making coverage tracking list", TraceEventType.Information, "Routing Manager");
            _dbFactory.Execute<QuestContext>((db) =>
            {
                // add in programmable ones.
                var maps = db.CoverageMapDefinition.Where( c=> c.VehicleCodes.Length > 0 && c.IsEnabled);

                foreach (var definition in maps)
                    _vehicleCoverageTrackerList.Add(new VehicleCoverageTracker(definition, _routingData, _coverageMapManager, _resStore, _routingEngine, _dbFactory));
            });
            Logger.Write($"Coverage tracking list has {_vehicleCoverageTrackerList.Count} items", TraceEventType.Information, "Routing Manager");

        }

        private CoverageMap CalculateIncCoverages(Dictionary<string, CoverageMap> target)
        {
            Logger.Write("Routing Manager: calculating incident coverage", TraceEventType.Information, "Routing Manager");
            var incCoverage = CalculateStandardIncidentCoverage(IncidentCoverage, "INC", tilesize);

            AddCoverage(target, incCoverage);

            HeatmapUpdate heatmap = new HeatmapUpdate { Item = GetHeatmap(incCoverage), ValidFrom = DateTime.UtcNow };
            base.ServiceBusClient.Broadcast(heatmap);

            return incCoverage;
        }

        private void CalculateResCoverages(string engine, Dictionary<string, CoverageMap> target, List<VehicleCoverageTracker> vehiclemaps, CoverageMap incCoverage)
        {
            var routingEngine = GetRoutingEngine(engine);

            Logger.Write("Routing Manager: calculating resource coverage", 
                TraceEventType.Information, "Routing Manager");
            CoverageMap combinedmap = null;
            var vehicleCoverage = new List<CoverageMap>();

            // calculate coverage for each defined resource type
            foreach (var vt in vehiclemaps)
            {
                if (!_stopping)
                {
                    var map = vt.GetCoverage();

                    if (map != null)
                    {
                        map.Percent = Math.Round(map.Coverage(_operationalArea) * 100, 1);

                        AddCoverage(target, map);
                        vehicleCoverage.Add(vt.CombinedMap);

                        HeatmapUpdate heatmap = new HeatmapUpdate { Item = GetHeatmap(map), ValidFrom = DateTime.UtcNow };
                        base.ServiceBusClient.Broadcast(heatmap);
                    }
                }
            }

            if (!_stopping)
            {
                //calculate a combined coverage
                combinedmap = CalculateCombinedResourceCoverage(vehicleCoverage, CombinedCoverage, "COV");

                if (combinedmap != null)
                {
                    //combinedmap.Percent = CalcPercent(CoverageFactor, CoverageMapUtil.Coverage(combinedmap));
                    combinedmap.Percent = Math.Round(combinedmap.Coverage(_operationalArea) * 100, 1);


                    Logger.Write($"Calculated combined coverage={combinedmap.Percent}",
                        TraceEventType.Information, "Routing Manager");

                    AddCoverage(target, combinedmap);

                    HeatmapUpdate heatmap = new HeatmapUpdate { Item = GetHeatmap(combinedmap), ValidFrom = DateTime.UtcNow };
                    base.ServiceBusClient.Broadcast(heatmap);

                }
            }

            if (!_stopping && incCoverage != null && combinedmap != null)
            {
                Logger.Write("Calculating resource holes",
                    TraceEventType.Information, "Routing Manager");
                // subtract all resource coverages from incident coverage to see where the holes are
                var holes = CalculateResourceHoles(incCoverage, combinedmap, ResourceHoles);

                if (holes != null)
                {
                    holes.Percent = Math.Round(holes.Coverage(_operationalArea) * 100, 1);

                    AddCoverage(target, holes);

                    HeatmapUpdate heatmap = new HeatmapUpdate { Item = GetHeatmap(holes), ValidFrom = DateTime.UtcNow };
                    base.ServiceBusClient.Broadcast(heatmap);

                }
            }
        }

        private List<TrialCoverageResponse> CalculateTrialCoverages(List<VehicleCoverageTracker> vehiclemaps, List<VehicleOverride> overrides, Dictionary<string, CoverageMap> standardCoverages, string engine)
        {
            var routingEngine = GetRoutingEngine(engine);

            Logger.Write("Routing Manager: calculating trial coverage", 
                TraceEventType.Information, "Routing Manager");
            var results = new List<TrialCoverageResponse>();
            var vehicleCoverage = new List<CoverageMap>();

            if (standardCoverages.Count == 0)
                return results;

            // calculate coverage for each defined resource type
            foreach (var vt in vehiclemaps)
            {
                var result = vt.GetTrialCoverage(overrides);
                if (result != null)
                {
                    vehicleCoverage.Add(result.Map);
                    results.Add(result);
                }
                else
                    vehicleCoverage.Add(vt.CombinedMap);
            }

            //calculate a combined coverage
            var combinedmap = CalculateCombinedResourceCoverage(vehicleCoverage, CombinedCoverage, "COV");

            // get originals
            var incCoverage = standardCoverages[IncidentCoverage];
            var holesBefore = standardCoverages[ResourceHoles];

            // subtract all resource coverages from incident coverage to see where the holes are
            var holesAfter = CalculateResourceHoles(incCoverage, combinedmap, ResourceHoles);

            var holesTrial = new TrialCoverageResponse
            {
                Map = holesAfter,
                LowIsBad = false,
                Before = (1 - holesBefore.Coverage())*100.0,
                After = (1 - holesAfter.Coverage())*100.0
            };


            holesTrial.UpdateDelta();

            results.Add(holesTrial);

            return results;
        }

        private static CoverageMap CalculateResourceHoles(CoverageMap incident, CoverageMap vehicleCoverage, string name)
        {
            Logger.Write("Routing Manager: calculating resource holes", TraceEventType.Information, "Routing Manager");
            var source = incident.Clone();
            source.Name = name;
            source = source.DifferenceCoverage(vehicleCoverage);
            return source;
        }

        /// <summary>
        ///     merge all vehicle coverages into a single one
        /// </summary>
        /// <param name="name"></param>
        /// <param name="vehicleCoverage"></param>
        private static CoverageMap CalculateCombinedResourceCoverage(List<CoverageMap> vehicleCoverage, string name, string code)
        {
            try
            {
                Logger.Write("Routing Manager: calculating combined coverage", TraceEventType.Information, "Routing Manager");
                if (vehicleCoverage == null || vehicleCoverage.Count == 0 || vehicleCoverage[0] == null)
                    return null;

                // merge all maps
                var map = new CoverageMap(name, code)
                {
                    Blocksize = vehicleCoverage[0].Blocksize,
                    Columns = vehicleCoverage[0].Columns,
                    OffsetX = vehicleCoverage[0].OffsetX,
                    OffsetY = vehicleCoverage[0].OffsetY,
                    Rows = vehicleCoverage[0].Rows,
                    Data = new byte[vehicleCoverage[0].Data.Length]
                };

                for (var i = 0; i < map.Data.Length; i++)
                {
                    var sum = 0;
                    foreach (var m in vehicleCoverage)
                        if (m != null)
                            sum += m.Data[i];

                    map.Data[i] = (byte) (sum > 255 ? 255 : sum);
                }

                return map;
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }

            return null;
        }

        /// <summary>
        ///     calculate incident density
        /// </summary>
        /// <param name="name"></param>
        private CoverageMap CalculateStandardIncidentCoverage(string name, string code, int tilesize)
        {
            //TODO: Calculate density
            var map = _coverageMapManager.GetStandardMap(tilesize).Clone().Name(name).Code(code);
            map.ClearData();
            map.Percent = Math.Round(map.Coverage(_operationalArea) * 100, 1);
            return map;
        }

        /// <summary>
        ///     add a coverage into our list
        /// </summary>
        /// <param name="target"></param>
        /// <param name="map"></param>
        private void AddCoverage(Dictionary<string, CoverageMap> target, CoverageMap map)
        {
            if (map == null)
                return;

            var clone = map.Clone();

            // add to list
            if (!target.ContainsKey(map.Code))
                target.Add(map.Code, clone);
            else
            {
                target[map.Code] = clone;
            }
        }

        #endregion
    }
    
}