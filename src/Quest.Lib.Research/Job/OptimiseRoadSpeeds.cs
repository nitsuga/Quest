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

namespace Quest.Lib.Research.Job
{
    /// <summary>
    /// attempts to find the best fitting set of road speeds for each road type
    /// when using the simple LAS estimator which just uses flat speeds for each
    /// road type.
    /// We attempt to optimise the similarity of the route to the actual route taken
    /// rather than the estimated time. But we also output the estimated time using the
    /// LAS road speeds and then compute the estimated time using the road link estimator.
    /// 
    /// The idea is the that route prediction in more accurate with the road type estimator
    /// but the eta is more accurate with the road link estimator. A combination of the two
    /// should produce accurate results (!!)
    /// </summary>
    [Injection("OptimiseRoadSpeeds", typeof(IProcessor))]
    public class OptimiseRoadSpeeds : ServiceBusProcessor
    {
        #region Private Fields
        private ILifetimeScope _scope;
        private RoutingData _data;
        private IDatabaseFactory _dbFactory;
        private DijkstraRoutingEngine _selectedRouteEngine;
        private TrackLoader _trackLoader;

        // list of tracks to analyse
        List<MyTrack> tracks = new List<MyTrack>();
        #endregion

        public OptimiseRoadSpeeds(
           ILifetimeScope scope,
           RoutingData data,
           TrackLoader trackLoader,
           IDatabaseFactory dbFactory,
           DijkstraRoutingEngine selectedRouteEngine,
           IServiceBusClient serviceBusClient,
           MessageHandler msgHandler,
           TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _selectedRouteEngine = selectedRouteEngine;
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
            // start the engine
            Logger.Write("Loading road network", GetType().Name);
            while (_data.IsInitialised == false)
                Thread.Sleep(1000);

            _dbFactory.ExecuteNoTracking<QuestDataContext>((db) =>
            {
                Logger.Write("Getting routes", GetType().Name);

                // get 10000 "good" routes from November 2016. These routes have been excluded from the summary speed tables
                // and so are effectvely unseen
                var routes = db.IncidentRoutes
                    //  .Where(x => x.IncidentId >= 20161101000000 && x.IncidentId < 20161201000000 && x.IsBadGPS == false)
                    .Where(x => x.IncidentId >= 20161001000000 && x.IncidentId < 20161101000000 && x.IsBadGps == false)
                    .Take(100)
                    .ToList();

                foreach (var route in routes)
                {
                    var items = db.RoadSpeedItem
                        .Where(x => x.IncidentRouteId == route.IncidentRouteId)
                        .OrderBy(x => x.DateTime)
                        .ToList();

                    tracks.Add(new MyTrack { Route = items, VehicleId = (int)route.VehicleId, IncidentRouteId = route.IncidentRouteId });
                }
            });

            NelderMeadSimplex optimiser = new NelderMeadSimplex();

            // our starting point is to use the same constants as the LAS routing engine
            SimplexConstant[] constants = new SimplexConstant[] {
                new SimplexConstant(29, 1),
                new SimplexConstant(3,  1),
                new SimplexConstant(24, 1),
                new SimplexConstant(14, 1),
                new SimplexConstant(19, 1),
                new SimplexConstant(35, 1),
                new SimplexConstant(2,  1),
                new SimplexConstant(5,  1),
                new SimplexConstant(5,  1)
            };

            double convergenceTolerance = 0.01;
            int maxEvaluations = 10000;
            int innerInterations = 0;

            var result = optimiser.Regress(constants, convergenceTolerance, maxEvaluations, ObjectiveFunction, innerInterations);

        }

        private double ObjectiveFunction(ObjectiveFunctionParams parms)
        {
            // set up an estimator using the road speeds passed by Mr. Nelder
            OptimsingRoadTypeSpeedCalculator estimator = new OptimsingRoadTypeSpeedCalculator(parms);

            List<double> similarity_results = new List<double>();

            // calculate the routes for each of our tracks using this estimator and measure its
            // similarity to the original route
            foreach (var route in tracks)
            {
                var similarity = AnalyseTrack(route, estimator);

                if (similarity > 0)
                    similarity_results.Add(similarity);
            }

            // return the average similarity
            return similarity_results.Average();
        }

        /// <summary>
        /// calculate the similarity of the route from the estimator vs the actual route
        /// </summary>
        /// <param name="track">actual route data</param>
        /// <param name="edgeCalculator">edge calculator to use</param>
        /// <returns></returns>
        private double AnalyseTrack(MyTrack track, OptimsingRoadTypeSpeedCalculator edgeCalculator)
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
                    RoadSpeedCalculator = edgeCalculator.GetType().Name
                };

                var result = _selectedRouteEngine.CalculateRouteMultiple(request);

                if (result.Items.Count > 0)
                {
                    try
                    {
                        // first route found
                        var engineroute = result.Items[0];

                        // calculate quantile road link matches
                        var original = track.Route.Select(x => x.RoadLinkEdgeId ?? 0).ToArray();
                        var routing = engineroute.Connections.Select(x => x.Edge).ToList().ToArray();

                        var similarity = RouteSimilarity(original, routing);
                        Logger.Write($"Track {track.IncidentRouteId} Similarity={similarity}");

                        return similarity;
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

            return -1; // somethign went wrong
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
            public OptimsingRoadTypeSpeedCalculator(ObjectiveFunctionParams parms)
            {
                Logger.Write($"Constants = {string.Join(",", parms.constants)}");

                Initialise(0, 0, 0, parms.constants);
            }
        }

    }
}
