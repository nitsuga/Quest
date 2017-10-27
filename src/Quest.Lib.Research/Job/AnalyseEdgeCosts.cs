using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Quest.Lib.MapMatching;
using Quest.Lib.Research.Utils;
using Quest.Lib.Routing;
using Quest.Lib.Routing.Speeds;
using Quest.Common.Messages;
using Quest.Lib.Trace;
using System.IO;
using Autofac;
using Quest.Lib.ServiceBus;
using Quest.Lib.Processor;
using Quest.Lib.DependencyInjection;
using Quest.Common.ServiceBus;
using Quest.Lib.Utils;
using Quest.Lib.Research.DataModelResearch;
using Quest.Lib.Data;
using System.Data.SqlClient;
using Quest.Common.Utils;
using Quest.Common.Messages.Routing;

namespace Quest.Lib.Research.Job
{
    /// <summary>
    /// Calculate actual vs estimate routing times based on all the edge 
    /// cost calculators we have.
    /// </summary>
    [Injection()]
    public class AnalyseEdgeCosts : ServiceBusProcessor
    {
        #region Private Fields
        private ILifetimeScope _scope;
        private RoutingData _data;
        private IDatabaseFactory _dbFactory;
        private DijkstraRoutingEngine _selectedRouteEngine;
        private TrackLoader _trackLoader;
        #endregion

        public AnalyseEdgeCosts(
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

        public void Analyse()
        {
            List<IncidentRoutes> routes = new List<IncidentRoutes>();

            var edgeCalculators = _scope.Resolve<IEnumerable<IRoadSpeedCalculator>>().ToList();

            // start the engine
            Logger.Write("Loading road network", GetType().Name);
            while (_data.IsInitialised == false)
                Thread.Sleep(1000);

            var filename = @"e:\temp\RoutingResults.csv";

            if (File.Exists(filename))
                File.Delete(filename);

            _dbFactory.ExecuteNoTracking<QuestDataContext>((db) =>
            {
                Logger.Write("Getting routes", GetType().Name);

                // get 10000 "good" routes from November 2016. These routes have been excluded from the summary speed tables
                // and so are effectvely unseen
                routes = db.IncidentRoutes
                    //  .Where(x => x.IncidentId >= 20161101000000 && x.IncidentId < 20161201000000 && x.IsBadGPS == false)
                    .Where(x => x.IncidentId >= 20161001000000 && x.IncidentId < 20161101000000 && x.IsBadGps == false)
                    .Take(100000)
                    .ToList();
            });

            using (var file = new StreamWriter(filename))
            {
                file.WriteLine($"IncidentRouteID, HoW, RoutingMethod, EdgeMethod, EstimatedDuration,EstimatedDistance, Links, ActualDuration, Vehicleid,qA1,qA2,qA3,qA4,qB1,qB2,qB3,qB4,Orig,NewPath,sx,sy,ex,ey,totalAngleDelta,deg45cl,deg60cl,deg90cl,deg45cr,deg60cr,deg90cr,RoadCount");

                var i = 0;
                foreach (var r in routes)
                {
                    i++;
                    try
                    {
                        Logger.Write($"Analysing route {i}", GetType().Name);
                        CalculateEstimates(file, edgeCalculators, r, i);
                    }
                    catch(Exception ex)
                    {
                        Logger.Write(ex.ToString(), GetType().Name);
                    }
                }
            }

        }


        /// <summary>
        /// calculate all the estimates for a given route.
        /// we calculate route using routing engine AND track using map-matcher
        /// we calculate duration for both methods using all edge cost algos
        /// </summary>
        /// <param name="db"></param>
        /// <param name="route"></param>
        /// <param name="file"></param>
        /// <param name="edgeCalculators"></param>
        void CalculateEstimates(StreamWriter file, List<IRoadSpeedCalculator> edgeCalculators, IncidentRoutes route, int index)
        {
            //            Logger.Write($"Loading track", GetType().Name);
            var track = _trackLoader.GetTrack($"db.inc:{route.IncidentRouteId}");

            if (track.Fixes.Count == 0)
                return;

            List<RoadSpeedItem> mapmatchdata=null;

            _dbFactory.Execute<QuestDataContext>((db) =>
            {
                // get map match route as well
                mapmatchdata = db.RoadSpeedItem
                .Where(x => x.IncidentRouteId == route.IncidentRouteId)
                .OrderBy(x => x.DateTime)
                .ToList();
            });

            if (!mapmatchdata.Any())
                return;

            CalculateStartEnd(track, route);

            var first = mapmatchdata.First();
            var last = mapmatchdata.Last();

            var starttime = first.DateTime;
            var endtime = last.DateTime;

            var actualDuration = (endtime - starttime).TotalSeconds;
            var how = starttime.HourOfWeek();

            var startEdge = _data.Dict[first.RoadLinkEdgeId ?? 0];
            var lastEdge = _data.Dict[last.RoadLinkEdgeId ?? 0];

            var startPoint = new EdgeWithOffset { Edge = startEdge, AngleRadians = 0, Coord = startEdge.Geometry.Coordinates[0] };
            var lastPoint = new EdgeWithOffset { Edge = lastEdge, AngleRadians = 0, Coord = lastEdge.Geometry.Coordinates[lastEdge.Geometry.NumPoints - 1] };

            //// try each edge cost in turn,
            foreach (var edgeCalculator in edgeCalculators)
            {
                Logger.Write($"..1", GetType().Name);
                CalculateEstimateUsingOriginalTrack(file, route, edgeCalculator, mapmatchdata, index, startPoint, lastPoint, how, actualDuration, starttime, endtime, route.VehicleId ?? 0);
                Logger.Write($"..2", GetType().Name);
                CalculateEstimateUsingRoutingEngine(file, route, edgeCalculator, mapmatchdata, index, startPoint, lastPoint, how, actualDuration, starttime, endtime, route.VehicleId ?? 0);
            }
        }

        void CalculateStartEnd(Track track, IncidentRoutes r)
        {
            _dbFactory.Execute<QuestDataContext>((db) =>
            {
                var starttime = track.Fixes.First().Timestamp;
                var endtime = track.Fixes.Last().Timestamp;
                var duration = (endtime - starttime).TotalSeconds;

                db.Execute("UpdateIncidentDuration @id={0}, @StartTime={1}, @EndTime={2}, @Duration={3}", r.IncidentRouteId, starttime, endtime, (int)duration);

            });
        }

        void CalculateEstimateUsingOriginalTrack(StreamWriter file, IncidentRoutes r, IRoadSpeedCalculator edgeCalculator, List<RoadSpeedItem> mapmatchdata, int index, EdgeWithOffset startPoint, EdgeWithOffset endPoint, int how, double actualDuration, DateTime startTime, DateTime endTime, int vehicleType)
        {
            try
            {
                var dow = ((int)startTime.DayOfWeek + 6) % 7;

                var duration = 0.0;
                var distance = 0.0;
                var links=0;

                RoadVector vector;

                vector.DistanceMeters = 0;
                vector.DurationSecs = 0;

                // loop through each 
                foreach (var rl in mapmatchdata.Select(x => x.RoadLinkEdgeId).Distinct().ToList())
                {
                    RoadEdge edge = null;
                    _data.Dict.TryGetValue(rl ?? 0, out edge);
                    if (edge != null)
                    {
                        vector = edgeCalculator.CalculateEdgeCost(r.VehicleId == 1 ? "AEU" : "FRU", how, edge);
                    }
                    duration += vector.DurationSecs;
                    distance += vector.DistanceMeters;
                    links++;
                }

                try
                {
                    //var apath = MakePathFromRoadSpeedItems(mapmatchdata);
                    var id = edgeCalculator.GetId();
                    var sx = startPoint.Coord.X;
                    var ex = endPoint.Coord.X;
                    var sy = startPoint.Coord.Y;
                    var ey = endPoint.Coord.Y;
                    file.WriteLine($"{r.IncidentRouteId},{how},{2},{id},{(int)duration},{(int)distance},{links},{(int)actualDuration},{vehicleType},0,0,0,0,0,0,0,0,0,0,0,\"\",\"\",{sx},{sy},{ex},{ey},0,0,0,0" );
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        void CalculateEstimateUsingRoutingEngine(StreamWriter file, IncidentRoutes r, IRoadSpeedCalculator edgeCalculator, List<RoadSpeedItem> mapmatchdata, int index, EdgeWithOffset startPoint, EdgeWithOffset endPoint, int how, double actualDuration, DateTime startTime, DateTime endTime, int vehicleType)
        {
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
                    EndLocations = new List<EdgeWithOffset> { endPoint },
                    VehicleType = vehicleType == 1 ? "AEU" : "FRU",
                    RoadSpeedCalculator = edgeCalculator.GetType().Name
                };

                var result = _selectedRouteEngine.CalculateRouteMultiple(request);

                if (result.Items.Count > 0)
                {
                    try
                    {
                        var engineroute = result.Items[0];

                        var links = engineroute.Connections.Count;
                        var id = edgeCalculator.GetId();

                        var rc = 0;

                        var deg45 = Math.PI / 4;
                        var deg60 = Math.PI / 3;
                        var deg90 = Math.PI / 2;

                        var deg45c_l = 0;
                        var deg60c_l = 0;
                        var deg90c_l = 0;

                        var deg45c_r = 0;
                        var deg60c_r = 0;
                        var deg90c_r = 0;

                        var totalAngleDelta = 0.0;
                        for (int i = 0; i < engineroute.Connections.Count() - 1; i++)
                        {
                            var from = engineroute.Connections[i].Edge;
                            var to = engineroute.Connections[i + 1].Edge;

                            // calculate number of road name changes
                            if (from.RoadName != to.RoadName)
                                rc++;

                            // calculate number of sharp angle changes
                            var fromAngle = Math.Atan2(from.Geometry[0].X - from.Geometry[from.Geometry.Count - 1].X, from.Geometry[0].Y - from.Geometry[from.Geometry.Count - 1].Y);
                            var toAngle = Math.Atan2(    to.Geometry[0].X - to.Geometry[to.Geometry.Count - 1].X, to.Geometry[0].Y - to.Geometry[to.Geometry.Count - 1].Y);

                            if (fromAngle < 0) fromAngle += (2 * Math.PI);
                            if (toAngle < 0) toAngle += (2 * Math.PI);

                            var angledelta = toAngle - fromAngle;

                            if (angledelta >= deg45) deg45c_r++;
                            if (angledelta >= deg60) deg60c_r++;
                            if (angledelta >= deg90) deg90c_r++;

                            if (angledelta <= -deg45) deg45c_l++;
                            if (angledelta <= -deg60) deg60c_l++;
                            if (angledelta <= -deg90) deg90c_l++;

                            totalAngleDelta += Math.Abs(angledelta);
                        }

                        // calculate quantile road link matches
                        var original = mapmatchdata.Select(x=>x.RoadLinkEdgeId??0).ToArray();
                        var routing = engineroute.Connections.Select(x => x.Edge).ToList().ToArray();

                        var qA = QuantileByRoadlink(original, routing);
                        var qB = QuantileByDistance(original, routing);

                        var origPath = "";
                        var newPath = "";

                        if (index < 100)
                        {
                            origPath = MakePathFromRoadSpeedItems(mapmatchdata);
                            newPath = MakePathFromRoadSpeedItems(engineroute.Connections.Select(x=>x.Edge).ToList());
                        }

                        var sx = startPoint.Coord.X;
                        var ex = endPoint.Coord.X;
                        var sy = startPoint.Coord.Y;
                        var ey = endPoint.Coord.Y;

                        file.WriteLine($"{r.IncidentRouteId},{how},{1},{id},{(int)engineroute.Duration},{(int)engineroute.Distance},{links},{(int)actualDuration},{vehicleType},{qA[0]},{qA[1]},{qA[2]},{qA[3]},{qB[0]},{qB[1]},{qB[2]},{qB[3]},\"{origPath}\",\"{newPath}\",{sx},{sy},{ex},{ey},{totalAngleDelta},{deg45c_l},{deg60c_l},{deg90c_l},{deg45c_r},{deg60c_r},{deg90c_r},{rc}");
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

        /// <summary>
        /// Compute similarity of route taken vs actual route
        /// </summary>
        /// <param name="original">list of road link ids in original route</param>
        /// <param name="routing">list of road link ids in route</param>
        /// <returns></returns>
        private float[] QuantileByRoadlink(int[] original, IReadOnlyCollection<RoadEdge> routing)
        {
            var results = new float[] { 0, 0, 0, 0 };
            // how many in each quantile?
            var chunk = routing.Count / 4;
            for (var i = 0; i < 4; i++)
            {
                // get routing quantile section
                var section = routing.Skip(i * chunk).Take(chunk).ToArray();

                // join with original to find out how many are the same
                var same = section.Select(x => x.RoadLinkEdgeId).Where(original.Contains).Count();

                // save result
                results[i] = same / (float)section.Length;
            }
            return results;
        }

        private float[] QuantileByDistance(int[] original, RoadEdge[] routing)
        {
            var results = new float[] { 0, 0, 0, 0 };
            // how many in each quantile?
            var qdistance = routing.Sum(x => x.Length) / 4;
            double over = 0;
            int idx = 0;
            List<RoadEdge> section = new List<RoadEdge>();

            for (var i = 0; i < 4; i++)
            {
                section.Clear();
                int from = idx;
                double l = over;
                if (i != 3)
                    // find index of next quarter from the last idx position
                    // and the bit left over
                    for (; l <= qdistance; l += routing[idx].Length, idx++)
                        section.Add(routing[idx]);
                else
                    for (; idx< routing.Length; idx++)
                        section.Add(routing[idx]);

                //var actualDistance = section.Sum(x => x.Length) - over;
                var actualDistance = l;

                // if we were over the limit then add the previous link into the chain
                if (over > 0)
                    section.Add(routing[from - 1]);

                // because we're dealing with possibly long roads, the actual distance might(will) be longer that the
                // quarter distance, so calculate the remaining length and add it to the next quarter
                over = actualDistance - qdistance;

                // join with original to find out how many are the same
                var same = section.Select(x => x.RoadLinkEdgeId).Where(original.Contains).Count();

                // save result
                results[i] = same / (float)section.Count;
            }
            return results;
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

        ILineString GetRoadLinkShape(int? roadLinkEdgeId)
        {
            return _data.Dict[roadLinkEdgeId??0].Geometry;
        }
    }
}