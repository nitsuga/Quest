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

        private const string ResourceHoles = "Resource Holes";
        private const string CombinedCoverage = "Combined Coverage";
        private const string IncidentCoverage = "Expected Incidents";
        private const string StandbyCoverage = "Standby Coverage";
        private const string StandbyCompliance = "Standby Compliance";

        private bool _stopping;
        //private CoverageMap _sbpTotalCoverage = null;
        private CoverageMap _operationalArea;
        private readonly RoutingData _routingdata;
        private readonly ILifetimeScope _scope;

        public string defaultengine { get; set; }
        public bool doCoverage { get; set; } = false;
        public int tilesize { get; set; } = 250;
        public bool doEta { get; set; } = false;
        public string roadSpeedCalculator { get; set; }
        public int enrFrequencySeconds { get; set; } = 10;
        public int resFrequencySeconds { get; set; } = 10;
        public int incFrequencySeconds { get; set; } = 60;
        public string coverageExportDirectory { get; set; }

        /// <summary>
        ///     list of vehicle coverage trackers
        /// </summary>
        private readonly List<VehicleCoverageTracker<RoutingManager>> _vehicleCoverageTrackerList = new List<VehicleCoverageTracker<RoutingManager>>();

        #endregion

        #region public Methods

        public RoutingManager(
            ILifetimeScope scope,
            RoutingData routingdata,
            IServiceBusClient serviceBusClient,
            TimedEventQueue eventQueue,
            MessageHandler msgHandler) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _scope = scope;
            _routingdata = routingdata;
        }

        protected override void OnPrepare()
        {
            MsgHandler.AddHandler<CustomCoverageRequest>(CustomCoverageRequestHandler);
            MsgHandler.AddHandler<GetCoverageRequest>(GetCoverageHandler);
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

            Logger.Write("Routing Manager Initialising", TraceEventType.Information, "Routing Manager");

            Logger.Write($"Loading default routing engine {defaultengine}", TraceEventType.Information, "Routing Manager");
            var engine = _scope.ResolveNamed<IRouteEngine>(defaultengine);

            Logger.Write($"Waiting routing engine to load", TraceEventType.Information, "Routing Manager");
            WaitForEngineReady(engine);

            // build list and start tracker engines
            MakeTrackerList();

            _operationalArea = CoverageMapUtil.GetOperationalArea(tilesize);

            if (doCoverage && _operationalArea!=null)
            {
                // start standard coverage calculator
                var t1 = new Task(CalculateStandardCoverages );
                t1.Start();
            }

            if (doEta)
            {
                var t2 = new Task(CalculateEta);
                t2.Start();
            }

            Logger.Write("Routing Manager Started", TraceEventType.Information, "Routing Manager");

            _stopping = false;
        }

        private void WaitForEngineReady(IRouteEngine engine)
        {
            // wait for routing engines to start
            while (!engine.IsReady)
            {
                Thread.Sleep(1000);
//                ServiceBusClient.Broadcast(new RoutingEngineStatus { Ready = engine.IsReady });
            }
//            ServiceBusClient.Broadcast(new RoutingEngineStatus { Ready = engine.IsReady });
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
                            EtaCalculator.CalculateEnrouteTime(routingEngine, _routingdata, roadSpeedCalculator);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteError(ex);
                    }

                } while (!_stopping);
            }
            catch (Exception ex)
            {
                WriteError(ex);
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
                AddCoverage(StandardCoverages, new CoverageMap {Name = IncidentCoverage});
                AddCoverage(StandardCoverages, new CoverageMap {Name = CombinedCoverage});
                AddCoverage(StandardCoverages, new CoverageMap {Name = ResourceHoles});
                AddCoverage(StandardCoverages, new CoverageMap {Name = StandbyCoverage});
                AddCoverage(StandardCoverages, new CoverageMap {Name = StandbyCompliance});

                foreach (var definition in _vehicleCoverageTrackerList)
                    AddCoverage(StandardCoverages, new CoverageMap {Name = definition.Name});

                do
                {
                    counter++;
                    Thread.Sleep(1000);

                    try
                    {
                        if (resFrequencySeconds>0 && !_stopping && counter% resFrequencySeconds == 0)
                        {
                            CalculateResCoverages(defaultengine, StandardCoverages, _vehicleCoverageTrackerList, incCoverage);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteError(ex);
                    }

                    try
                    {
                        if (incFrequencySeconds>0 && !_stopping && ((counter % incFrequencySeconds) == 0 || incCoverage == null))
                        {
                            incCoverage = CalculateIncCoverages(StandardCoverages);
                            // also check for coverage stats
                            //mapcoverageutil.CalculateCoverageStats();
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteError(ex);
                    }

                    using (var db = new QuestEntities())
                    {
                        db.CleanCoverage();
                    }

                } while (!_stopping);
            }
            catch (Exception ex)
            {
                WriteError(ex);
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
                results = CalculateTrialCoverages(_vehicleCoverageTrackerList, overrides, StandardCoverages, engine);
            }
            catch (Exception ex)
            {
                Logger.Write($"CalculateCustomCoverages: failed {ex}",
                    TraceEventType.Information, "Routing Manager");
                WriteError(ex);
            }
            return results;
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

        private GetCoverageResponse GetCoverageHandler(NewMessageArgs t)
        {
            var request = t.Payload as GetCoverageRequest;
            if (request != null)
            {
                using (var db = new QuestEntities())
                {
                    var r = new GetCoverageResponse();
                    var results = db.GetVehicleCoverage(Convert.ToInt16(request.vehtype)).First();
                    if (results != null)
                    {
                        var offset = LatLongConverter.OSRefToWGS84(results.OffsetX, results.OffsetY);
                        var blocksize = LatLongConverter.OSRefToWGS84(results.OffsetX + results.Blocksize,
                            results.OffsetY + results.Blocksize);
                        var heatmap = new Heatmap
                        {
                            lon = offset.Longitude,
                            lat = offset.Latitude,
                            map = results.Data,
                            cols = results.Columns,
                            rows = results.Rows,
                            lonBlocksize = blocksize.Longitude - offset.Longitude,
                            latBlocksize = blocksize.Latitude - offset.Latitude,
                            vehtype = request.vehtype
                        };

                        r.Map = heatmap;
                    }

                    Logger.Write("Routing Manager: GetCoverageHandler returning GetCoverageResponse",
                        TraceEventType.Information, "Routing Manager");
                    return r;
                }
            }
            var result = new GetCoverageResponse { Message = "Unable to unpack the request", Success = false };
            return result;
        }

        private Response CustomCoverageRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as CustomCoverageRequest;
            if (request != null)
            {
                var result = new CustomCoverageResponse { id = request.id };

                var overrides = string.Join(",", request.overrides.Select(x => x.Callsign).ToArray());

                var incCoverage = StandardCoverages[IncidentCoverage];

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
            using (var db = new QuestEntities())
            {
                // add in programmable ones.
                var maps = from c in db.CoverageMapDefinitions where c.VehicleCodes.Length > 0 select c;

                foreach (var m in maps)
                    _vehicleCoverageTrackerList.Add(new VehicleCoverageTracker<RoutingManager>
                    {
                        Name = m.Name,
                        CombinedMap = null,
                        Definition = m,
                        Manager = this
                    });
            }
            Logger.Write($"Coverage tracking list has {_vehicleCoverageTrackerList.Count} items", TraceEventType.Information, "Routing Manager");

        }

        private CoverageMap CalculateIncCoverages(Dictionary<string, CoverageMap> target)
        {
            Logger.Write("Routing Manager: calculating incident coverage", TraceEventType.Information, "Routing Manager");
            var incCoverage = CalculateStandardIncidentCoverage(IncidentCoverage, tilesize);

            AddCoverage(target, incCoverage);
            return incCoverage;
        }

        private void CalculateResCoverages(string engine, Dictionary<string, CoverageMap> target,
            List<VehicleCoverageTracker<RoutingManager>> vehiclemaps, CoverageMap incCoverage)
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
                    var map = vt.GetCoverage(routingEngine);

                    // map.Percent = CalcPercent(CoverageFactor, CoverageMapUtil.Coverage(map));
                    map.Percent = Math.Round(map.Coverage(_operationalArea)*100, 1);

                    AddCoverage(target, map);
                    vehicleCoverage.Add(vt.CombinedMap);
                }
            }

            if (!_stopping)
            {
                //calculate a combined coverage
                combinedmap = CalculateCombinedResourceCoverage(vehicleCoverage, CombinedCoverage);

                //combinedmap.Percent = CalcPercent(CoverageFactor, CoverageMapUtil.Coverage(combinedmap));
                combinedmap.Percent = Math.Round(combinedmap.Coverage(_operationalArea)*100, 1);


                Logger.Write($"Calculated combined coverage={combinedmap.Percent}",
                    TraceEventType.Information, "Routing Manager");

                AddCoverage(target, combinedmap);
            }

            if (!_stopping && incCoverage != null && combinedmap != null)
            {
                    Logger.Write("Calculating resource holes", 
                        TraceEventType.Information, "Routing Manager");
                    // subtract all resource coverages from incident coverage to see where the holes are
                    var holes = CalculateResourceHoles(incCoverage, combinedmap, ResourceHoles);

                    holes.Percent = Math.Round(holes.Coverage(_operationalArea)*100, 1);

                    //holes.Percent = CalcPercent(CoverageFactor, CoverageMapUtil.Coverage(holes));

                    AddCoverage(target, holes);
            }
        }

        private List<TrialCoverageResponse> CalculateTrialCoverages(List<VehicleCoverageTracker<RoutingManager>> vehiclemaps,
            List<VehicleOverride> overrides, Dictionary<string, CoverageMap> standardCoverages, string engine)
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
                var result = vt.GetTrialCoverage(overrides, routingEngine);
                if (result != null)
                {
                    vehicleCoverage.Add(result.Map);
                    results.Add(result);
                }
                else
                    vehicleCoverage.Add(vt.CombinedMap);
            }

            //calculate a combined coverage
            var combinedmap = CalculateCombinedResourceCoverage(vehicleCoverage, CombinedCoverage);

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
            source = CoverageMapUtil.DifferenceCoverage(source, vehicleCoverage);
            return source;
        }

        /// <summary>
        ///     merge all vehicle coverages into a single one
        /// </summary>
        /// <param name="name"></param>
        /// <param name="vehicleCoverage"></param>
        private static CoverageMap CalculateCombinedResourceCoverage(List<CoverageMap> vehicleCoverage, string name)
        {
            try
            {
                Logger.Write("Routing Manager: calculating combined coverage", TraceEventType.Information, "Routing Manager");
                if (vehicleCoverage == null || vehicleCoverage.Count == 0 || vehicleCoverage[0] == null)
                    return null;

                // merge all maps
                var map = new CoverageMap(name)
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
                WriteError(ex);
            }

            return null;
        }

        /// <summary>
        ///     calculate incident density
        /// </summary>
        /// <param name="name"></param>
        private CoverageMap CalculateStandardIncidentCoverage(string name, int tilesize)
        {
            var map = CoverageMapUtil.GetStandardMap(tilesize).Clone().Name(name);
            map.ClearData();

            try
            {
                using (var db = new QuestEntities())
                {
                    var results = db.GetIncidentDensity().ToList();

                    foreach (var v in results)
                    {
                        if (v.CellX == null)
                            v.CellX = 0;

                        if (v.CellY == null)
                            v.CellY = 0;

                        var i = (int) v.CellX + (int) (v.CellY*map.Columns);
                        if (i >= 0 && i < map.Data.Length)
                        {
                            var i1 = v.Quantity & 0xff;
                            if (i1 != null) map.Data[i] = (byte) i1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteError(ex);
            }

            map.Percent = Math.Round(map.Coverage(_operationalArea)*100, 1);

            return map;
        }

        private static void WriteError(Exception ex)
        {
            Logger.Write(ex.ToString(), TraceEventType.Error,
                "Routing Manager");
            EventLog.WriteEntry("Application", ex.ToString());
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
            if (!target.ContainsKey(map.Name))
                target.Add(map.Name, clone);
            else
            {
                target[map.Name] = clone;
            }

            ExportArcGrid(map);

            ExportToDatabase(map);
        }

        /// <summary>
        ///     output the coverage map in ArgGrid format
        /// </summary>
        /// <param name="map"></param>
        private static void ExportToDatabase(CoverageMap map)
        {
            try
            {
                using (var db = new QuestEntities())
                {
                    try
                    {
                        var newrecord = new CoverageMapStore
                        {
                            Name = map.Name,
                            Blocksize = map.Blocksize,
                            Columns = map.Columns,
                            Rows = map.Rows,
                            Data = map.Data,
                            OffsetX = map.OffsetX,
                            OffsetY = map.OffsetY,
                            tstamp = DateTime.Now,
                            Percent = map.Percent
                        };

                        db.CoverageMapStores.Add(newrecord);

                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        WriteError(ex);
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        private void ExportArcGrid(CoverageMap map)
        {
            if (coverageExportDirectory.Length == 0 | map.Data == null)
                return;

            try
            {
                using (TextWriter writer = new StreamWriter(Path.Combine(coverageExportDirectory, map.Name + ".ASC"), false))
                {
                    writer.WriteLine("ncols           {0}", map.Columns);
                    writer.WriteLine("nrows           {0}", map.Rows);
                    writer.WriteLine("xllcorner       {0}", map.OffsetX);
                    writer.WriteLine("yllcorner       {0}", map.OffsetY);
                    writer.WriteLine("cellsize        {0}", map.Blocksize);
                    writer.WriteLine("NODATA_value    0");

                    for (var i = 0; i < map.Data.Length; i++)
                    {
                        int v = map.Data[i];
                        writer.Write("{0}  ", v);
                        if ((i + 1)%18 == 0)
                            writer.WriteLine("\r");
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        #endregion
    }
}