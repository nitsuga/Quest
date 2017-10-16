using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Quest.Lib.MapMatching;
using Quest.Lib.Research.Utils;
using Quest.Lib.Routing;
using Quest.Common.Messages;
using NetTopologySuite.Operation.Distance;
using System.Dynamic;
using Quest.Lib.Processor;
using Autofac;
using Quest.Lib.Search.Elastic;
using Quest.Lib.ServiceBus;
using Quest.Lib.Trace;
using Quest.Common.ServiceBus;
using Quest.Lib.Utils;
using Quest.Lib.Research.DataModelResearch;
using Quest.Lib.Data;
using Quest.Common.Utils;

namespace Quest.Lib.Research.Job
{
    /// <summary>
    /// Calculate actual vs estimate routing times based on all the edge 
    /// cost calculators we have.
    /// </summary>
    public class AnalyseEmissionAndTransition : ServiceBusProcessor
    {
        #region Private Fields
        private ILifetimeScope _scope;
        private ISearchEngine _searchEngine;
        private RoutingData _data;
        private DijkstraRoutingEngine _selectedRouteEngine;
        #endregion

        private IDatabaseFactory _dbFactory;
        private TrackLoader _trackLoader;

        public AnalyseEmissionAndTransition(
            ISearchEngine searchEngine,
            ILifetimeScope scope,
            IDatabaseFactory dbFactory,
            RoutingData data,
            TrackLoader trackLoader,
            DijkstraRoutingEngine selectedRouteEngine,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _selectedRouteEngine = selectedRouteEngine;
            _searchEngine = searchEngine;
            _scope = scope;
            _data = data;
            _dbFactory = dbFactory;
            _trackLoader = trackLoader;
        }

        protected override void OnPrepare()
        {
        }
        protected override void OnStart()
        {
            AtsParms settings = new AtsParms() { MinSeconds = 10 };
            //Analyse(settings);
        }

        public void Analyse(AtsParms settings)
        {
            var incs = GetIncidentRoutes();

            // wait for routing engine to fire up
            while (_selectedRouteEngine.IsReady == false)
            {
                Thread.Sleep(1000);
            }

            Logger.Write($"Analysing {incs.Count} incidents", GetType().Name);

            string filename = @"e:\temp\Transition.csv";

            if (File.Exists(filename))
                File.Delete(filename);

            using (var file = new StreamWriter(filename))
            {
                file.WriteLine($"Incident,Callsign,FixId,dx,dr,dy,dydx0,dydx1,count");
                var counter = 0;
                foreach (var inc in incs)
                {
                    try
                    {
                        AnalyseTransition(file, inc, settings);
                        counter++;
                        if (counter%1 == 0)
                            Logger.Write($"Analysed {counter} incident routes", GetType().Name);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }

        public void AnalyseSpeeds(AtsParms settings)
        {
            // wait for routing engine to fire up
            while (_selectedRouteEngine.IsReady == false)
            {
                Thread.Sleep(1000);
            }

            var incs = GetIncidents();

            Logger.Write($"Analysing {incs.Count} incidents", GetType().Name);

            var counter = 0;
            foreach (var inc in incs)
            {
                try
                {
                    var t = _trackLoader.GetTracks($"db.inc:{inc}");

                    foreach (var track in t)
                    {
                        track.MarkSuspectFixes(settings.MinSeconds, settings.MinDistance);

                        AnalyseTrackSpeeds(t, settings);
                    }
                    counter++;

                    if (counter % 1 == 0)
                        Logger.Write($"Analysed {counter} incidents", GetType().Name);

                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// get a list of 1000 incidents
        /// </summary>
        /// <returns></returns>
        private List<long> GetIncidents()
        {
            List<long> incidents;
            return _dbFactory.Execute<QuestDataContext, List<long>>((db) =>
            {
                incidents = db.IncidentRoutes
                    .Where(x => x.IsBadGps == false)
                    .Select(x => (long)x.IncidentId)
                    .Distinct()
                    .Take(10000)
                    .ToList();
                return incidents;
            });
        }

        private List<long> GetIncidentRoutes()
        {
            return _dbFactory.Execute<QuestDataContext, List<long>>((db) =>
            {
                List<long> incidents;
                incidents = db.IncidentRoutes
                    .Where(x => x.IsBadGps == false)
                    .Select(x => (long)x.IncidentRouteId)
                    .Distinct()
                    .Take(10000)
                    .ToList();
                return incidents;
            });
        }

        private void AnalyseTransition(StreamWriter file, long routeid, AtsParms parms)
        {
            dynamic parameters = new ExpandoObject();

            parameters.Emission = "GpsEmission";
            parameters.EmissionP1 = 0;
            parameters.EmissionP2 = 0;

            parameters.Transition = "exponential";
            parameters.TransitionP1 = 0.0182;
            parameters.TransitionP2 = 0;

            parameters.MaxRoutes = 15;
            parameters.RoadGeometryRange = 50;
            parameters.RoadEndpointEnvelope = 50;
            parameters.DirectionTolerance = 120;
            parameters.Skip = 4;
            parameters.Take = 9999;
            parameters.SumProbability = false;
            parameters.NormaliseTransition = false;
            parameters.NormaliseEmission = false;
            parameters.GenerateGraphVis = false;
            parameters.MinDistance = 25;
            parameters.MaxSpeed = 80;
            parameters.MaxCandidates = 100;

            var track = _trackLoader.GetTrack($"db.inc://{routeid}");

            var request = new MapMatcherMatchSingleRequest()
            {
                RoutingEngine = "Dijkstra",
                MapMatcher = "HmmViterbiMapMatcher",
                RoutingData = "Standard",
                Fixes = track.Fixes,
                Parameters = parameters
            };

            var result = MapMatching.MapMatcherUtil.MapMatcherMatchSingle(_scope, request);

            if (!result.Success)
                return;

            var vectors = result.Result.Results;
            for ( int i=0; i< vectors.Count-1; i++)
            {
                var thisfix = vectors[i];
                var nextfix = vectors[i+1];

                var dx = thisfix.SourceCoord.Distance(thisfix.DestCoord);
                var dr = thisfix.RouteDistance;
                var dy = thisfix.Fix.Position.Distance(nextfix.Fix.Position);
                var dydx0 = thisfix.Fix.Position.Distance(thisfix.SourceCoord);
                var dydx1 = thisfix.Fix.Position.Distance(thisfix.DestCoord);
                var rc = thisfix.Candidates;

                Debug.WriteLine($"{track.Incident},\"{track.Callsign}\",{i},{dx},{dr},{dy}");
                file.WriteLine($"{track.Incident},\"{track.Callsign}\",{i},{dx},{dr},{dy},{dydx0},{dydx1},{rc}");
            }
        }

        private void AnalyseTransitionEmissionOld(StreamWriter file, List<Track> tracks, AtsParms parms)
        {
            var calc = "ConstantSpeedCalculator";

            foreach (var track in tracks)
            {
                // only good fixes please
                track.Fixes = track.Fixes.Where(x => x.Corrupt == null).ToList();

                for (int i = 0; i < track.Fixes.Count - 1; i++)
                {
                    try
                    {
                        var thisfix = track.Fixes[i];
                        var nextfix = track.Fixes[i + 1];

                        var how = thisfix.Timestamp.HourOfWeek();

                        var startPoint = _data.GetEdgeFromPoint(thisfix.Position);
                        var endPoint = _data.GetEdgeFromPoint(nextfix.Position);
                        var endPoints = new List<EdgeWithOffset>
                        {
                            endPoint
                        };

                        var request = new RouteRequestMultiple
                        {
                            DistanceMax = int.MaxValue,
                            DurationMax = int.MaxValue,
                            InstanceMax = 1,
                            StartLocation = startPoint,
                            HourOfWeek = how,
                            SearchType = RouteSearchType.Quickest,
                            EndLocations = endPoints,
                            VehicleType = track.VehicleType == 1 ? "AEU" : "FRU",
                            RoadSpeedCalculator = calc
                        };

                        var roadEndpointEnvelope = 50;

                        var point = new NetTopologySuite.Geometries.Point(thisfix.Position);

                        // range envelope
                        var envelope = point.Buffer(roadEndpointEnvelope).EnvelopeInternal;

                        // get a block of nearest roads
                        var nearbyRoadsCourse = _data.ConnectionIndex.Query(envelope);

                        // extract "MaxCandidates" nearest roads based on range to geometry from the fix and within "RoadGeometryRange"
                        var nearestRoads =
                            nearbyRoadsCourse.Select(
                                x => new { RoadLink = x, distance = DistanceOp.Distance(x.Geometry, point) })
                                .Where(x => x.distance < roadEndpointEnvelope)
                                .OrderBy(x => x.distance);

                        if (!nearestRoads.Any()) continue;
                        var roadCount = nearestRoads.Count();
                        var nearestRoad = nearestRoads.First().distance;

                        var result = _selectedRouteEngine.CalculateRouteMultiple(request);
                        if (!result.Items.Any()) continue;

                        var euclidean = startPoint.Coord.Distance(endPoint.Coord);
                        var roadDistance = result.Items.First().Distance;
                        file.WriteLine(
                            $"{track.Incident},\"{track.Callsign}\",{thisfix.Id},{euclidean},{roadDistance},{roadCount},{nearestRoad}");
                    }
                    catch (Exception ex)
                    {
                        Debug.Print($"{ex}");
                    }

                }
            }
        }

        private void AnalyseTrackSpeeds(List<Track> tracks, AtsParms parms)
        {
            _dbFactory.Execute<QuestDataContext>((db) =>
            {
                foreach (var track in tracks)
                {
                    var bulk = new StringBuilder();

                    if (track == null) throw new ArgumentNullException(nameof(track));


                    // set all failed records to null
                    foreach (var f1 in track.Fixes.Where(x => x.Corrupt != null))
                    {
                        var sql = $"update Avls set estimatedspeed=null where rawAvlsid={f1.Id};";
                        bulk.Append(sql);
                    }
                    var sqlc = bulk.ToString();
                    if (sqlc.Length > 0)
                        db.Execute(sqlc);

                    track.CalculateEstimateSpeeds();

                    bulk.Clear();
                    foreach (var f1 in track.Fixes.Where(x => x.Corrupt == null))
                    {
                        try
                        {
                            var spd = f1.EstimatedSpeedMph != null ? f1.EstimatedSpeedMph.ToString() : "null";
                            var sql = $"update Avls set estimatedspeed={spd} where rawAvlsid={f1.Id};";
                            bulk.Append(sql);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }


                    if (bulk.ToString().Length > 0)
                        db.Execute(bulk.ToString());

                }
            });
        }

        public class AtsParms
        {
            public int MinSeconds;
            public int MinDistance;
        }

    }

}