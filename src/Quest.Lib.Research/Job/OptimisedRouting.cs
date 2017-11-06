using Autofac;
using GeoAPI.Geometries;
using Quest.Common.Messages.Routing;
using Quest.Common.ServiceBus;
using Quest.Common.Utils;
using Quest.Lib.Data;
using Quest.Lib.DependencyInjection;
using Quest.Lib.MapMatching;
using Quest.Lib.Optimiser;
using Quest.Lib.Optimiser.NelderMead;
using Quest.Lib.Processor;
using Quest.Lib.Research.DataModelResearch;
using Quest.Lib.Research.Utils;
using Quest.Lib.Routing;
using Quest.Lib.Routing.Speeds;
using Quest.Lib.ServiceBus;
using Quest.Lib.Trace;
using Quest.Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NetTopologySuite.Geometries;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Quest.Lib.Research.Job
{
    /// <summary>
    /// The idea is the that route prediction in more accurate with the road type estimator
    /// but the eta is more accurate with the road link estimator. A combination of the two
    /// should produce accurate results (!!)
    /// </summary>
    [Injection("OptimisedRouting", typeof(IProcessor))]
    public class OptimisedRouting : ServiceBusProcessor
    {
        #region Private Fields
        private ILifetimeScope _scope;
        private RoutingData _data;
        private IDatabaseFactory _dbFactory;
        private DijkstraRoutingEngine _routingEngine;
        private TrackLoader _trackLoader;

        // list of tracks to analyse
        List<MyTrack> tracks = new List<MyTrack>();
        #endregion

        public OptimisedRouting(
           ILifetimeScope scope,
           RoutingData data,
           TrackLoader trackLoader,
           IDatabaseFactory dbFactory,
           DijkstraRoutingEngine selectedRouteEngine,
           IServiceBusClient serviceBusClient,
           MessageHandler msgHandler,
           TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _routingEngine = selectedRouteEngine;
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
            Analyse();
        }

        private void Analyse()
        {
            List<IncidentRoutes> routes = null ;

            _dbFactory.ExecuteNoTracking<QuestDataContext>((db) =>
            {
                Logger.Write("Getting routes", GetType().Name);

                // get 10000 "good" routes from November 2016. These routes have been excluded from the summary speed tables
                // and so are effectvely unseen
                routes = db.IncidentRoutes
                    .AsNoTracking()
                    //  .Where(x => x.IncidentId >= 20161101000000 && x.IncidentId < 20161201000000 && x.IsBadGPS == false)
                    .Where(x => x.IncidentId >= 20161001000000 && x.IncidentId < 20161101000000 && x.IsBadGps == false)
                    .OrderBy(x=>x.IncidentRouteId)
                    .Take(1000)
                    .ToList();
            });

            // start the engine
            Logger.Write("Loading road network", GetType().Name);
            while (_data.IsInitialised == false)
                Thread.Sleep(1000);

            IRoadSpeedCalculator speed_estimator = _scope.ResolveNamed<IRoadSpeedCalculator>("VariableSpeedByEdge");

            // trial 1
            //double[] parms = new double[] { 28.0772284011459, 4.02188509375223, 23.556298782768, 16.8873904946405, 19.54588666518, 35.5268850937522, 3.02188509375223, 15.6093850937522, 7.70938509375223 };

            double[] parms = new double[] { 29.39 , 5.31, 26.83, 15.51, 19.97, 35.47, 5.37, 8.37, 6.84 };


            // set up an estimator using the road speeds passed by Mr. Nelder
            OptimsingRoadTypeSpeedCalculator route_estimator = new OptimsingRoadTypeSpeedCalculator(4.33, parms);

            // calculate the routes for each of our tracks using this estimator and measure its
            // similarity to the original route

            int i = 0;
            routes.AsParallel().ForAll(route =>
            {
                try
                {
                    i++;
                    AnalyseTrack(i, route, route_estimator, speed_estimator);
                }
                catch(Exception ex)
                {
                    Logger.Write(ex);
                }
            });

        }

        /// <summary>
        /// calculate the similarity of the route from the estimator vs the actual route
        /// </summary>
        /// <param name="track">actual route data</param>
        /// <param name="route_estimator">edge calculator to use</param>
        /// <returns></returns>
        private void AnalyseTrack(int step, IncidentRoutes route, OptimsingRoadTypeSpeedCalculator route_estimator, IRoadSpeedCalculator speed_estimator)
        {
            MyTrack track = new MyTrack();
            _dbFactory.ExecuteNoTracking<QuestDataContext>((db) =>
            {
                var items = db.RoadSpeedItem
                .AsNoTracking()
                .Where(x => x.IncidentRouteId == route.IncidentRouteId)
                .OrderBy(x => x.DateTime)
                .ToList();

                track = new MyTrack { Route = items, VehicleId = (int)route.VehicleId, IncidentRouteId = route.IncidentRouteId };
            });
            if (track.Route.Count > 5)
            {
                var first = track.Route.First();
                var last = track.Route.Last();

                var starttime = first.DateTime;
                var endtime = last.DateTime;

                var actualDuration = (endtime - starttime).TotalSeconds;
                var how = starttime.HourOfWeek();

                var startEdge = _data.Dict[first.RoadLinkEdgeId ?? 0];
                var lastEdge = _data.Dict[last.RoadLinkEdgeId ?? 0];

                var startPoint = new EdgeWithOffset { Edge = startEdge, AngleRadians = 0, Coord = startEdge.Geometry.Coordinates[0] };
                var lastPoint = new EdgeWithOffset { Edge = lastEdge, AngleRadians = 0, Coord = lastEdge.Geometry.Coordinates[lastEdge.Geometry.NumPoints - 1] };

                try
                {
                    var request = new RouteRequestMultiple
                    {
                        DistanceMax = 15000,
                        DurationMax = 1800,
                        InstanceMax = 1,
                        StartLocation = startPoint,
                        HourOfWeek = how,
                        SearchType = RouteSearchType.Quickest,
                        EndLocations = new List<EdgeWithOffset> { lastPoint },
                        VehicleType = track.VehicleId == 1 ? "AEU" : "FRU",
                    };

                    var result = _routingEngine.CalculateRouteMultiple(request, route_estimator);

                    if (result.Items.Count > 0)
                    {
                        try
                        {
                            // first route found
                            var engineroute = result.Items[0];

                            // calculate quantile road link matches
                            var original = track.Route.Select(x => x.RoadLinkEdgeId ?? 0).ToArray();
                            var routing = engineroute.Connections.Select(x => x.Edge).ToList().ToList();

                            var similarity = RouteSimilarity(original, routing);
                            var correctedDuration = CalculateEstimateUsingOriginalTrack(engineroute, speed_estimator, how, starttime, track.VehicleId);

                            var sx = startPoint.Coord.X;
                            var ex = lastPoint.Coord.X;
                            var sy = startPoint.Coord.Y;
                            var ey = lastPoint.Coord.Y;

                            var origwkt = MakePathFromRoadSpeedItems(track.Route);
                            var estwkt = MakePathFromRoadSpeedItems(routing);

                            Debug.WriteLine($"{step},{track.IncidentRouteId},{how},{similarity},{actualDuration},{(int)engineroute.Duration},{correctedDuration},{sx},{sy},{ex},{ey},\"{origwkt}\",\"{estwkt}\"");

                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        double CalculateEstimateUsingOriginalTrack(RoutingResult r, IRoadSpeedCalculator edgeCalculator, int how, DateTime startTime, int vehicleType)
        {
            try
            {
                var dow = ((int)startTime.DayOfWeek + 6) % 7;

                var duration = 0.0;
                var distance = 0.0;
                var links = 0;

                RoadVector vector;

                vector.DistanceMeters = 0;
                vector.DurationSecs = 0;

                // loop through each 
                foreach (var rl in r.Connections.Select(x => x.Edge.RoadLinkEdgeId).Distinct().ToList())
                {
                    RoadEdge edge = null;
                    _data.Dict.TryGetValue(rl, out edge);
                    if (edge != null)
                    {
                        vector = edgeCalculator.CalculateEdgeCost(vehicleType == 1 ? "AEU" : "FRU", how, edge);
                    }
                    duration += vector.DurationSecs;
                    distance += vector.DistanceMeters;
                    links++;
                }

                return duration;
            }
            catch (Exception)
            {
                // ignored
            }

            return 0.0;
        }

        private float RouteSimilarity(int[] original, IReadOnlyCollection<RoadEdge> routing)
        {
            var section = routing.ToArray();

            // join with original to find out how many are the same
            var same = section.Select(x => x.RoadLinkEdgeId).Where(original.Contains).Count();

            // save result
            return same / (float)section.Length;
        }

        public string MakePathFromRoadSpeedItems(List<RoadSpeedItem> edges)
        {
            var mls = new MultiLineString(edges.Select(x => GetRoadLinkShape(x.RoadLinkEdgeId)).ToArray());
            return mls.ToText();
        }

        public string MakePathFromRoadSpeedItems(List<RoadEdge> edges)
        {
            var e = edges.Select(x => x.Geometry).ToArray();
            var mls = new MultiLineString(e);
            return mls.ToText();
        }

        private ILineString GetRoadLinkShape(int? roadLinkEdgeId)
        {
            return _data.Dict[roadLinkEdgeId ?? 0].Geometry;
        }

        internal class MyTrack
        {
            public List<RoadSpeedItem> Route;
            public int VehicleId;
            public int IncidentRouteId ;
        }

        internal class OptimsingRoadTypeSpeedCalculator : RoadTypeSpeedCalculator
        {
            public OptimsingRoadTypeSpeedCalculator(double nodeDelay, double[] roadspeeds)
            {
                Initialise(0, 0, nodeDelay, roadspeeds);
            }
        }

    }
}
