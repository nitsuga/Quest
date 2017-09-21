using System;
using System.Collections.Generic;
using System.Linq;
using Quest.Lib.Research.Model;
using Quest.Lib.Research.Utils;
using Quest.Lib.Routing;
using Quest.Lib.Trace;
using System.IO;
using Autofac;
using Quest.Lib.Processor;
using Quest.Lib.DependencyInjection;
using Quest.Lib.Utils;
using Quest.Lib.Google;
using Quest.Lib.Google.DistanceMatrix;
using System.Diagnostics;
using System.Threading;
using Quest.Lib.Routing.Speeds;
using Quest.Common.Messages;
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;

namespace Quest.Lib.Research.Job
{
    /// <summary>
    /// Compare routing times with google.
    /// </summary>
    [Injection()]
    public class CompareWithGoogle : SimpleProcessor
    {
        private ILifetimeScope _scope;
        private RoutingData _data;
        private VariableSpeedByEdge _edgeCalculator;
        private DijkstraRoutingEngine _selectedRouteEngine;

        public CompareWithGoogle(
            ILifetimeScope scope,
            RoutingData data,
            DijkstraRoutingEngine selectedRouteEngine,
            TimedEventQueue eventQueue) : base(eventQueue)
        {
            _selectedRouteEngine = selectedRouteEngine;
            _data = data;
            _scope = scope;
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
            // start the engine
            Logger.Write("Loading road network", GetType().Name);
            while (_data.IsInitialised == false)
                Thread.Sleep(1000);
            var filename = @"GoogleRoutingResults1.csv";

            if (File.Exists(filename))
                File.Delete(filename);

            List<IncidentRouteView> routes = new List<IncidentRouteView>();
            using (var db = new QuestResearchEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.Database.CommandTimeout = int.MaxValue;

                Logger.Write("Getting routes", GetType().Name);
                routes = db.IncidentRouteViews
                    
                    //.Where(x => x.IncidentId >= 20161001000000 && x.IncidentId < 20161130004872 && x.IsBadGPS == false)
                    .Where(x => x.IncidentRouteID >= 1405137 && x.IsBadGPS == false)
                    .Where(x => x.ActualDuration != null && x.IsBadGPS == false)
                    .Take(10000)
                    .ToList();
            }

            using (var file = new StreamWriter(filename))
            {
                file.WriteLine($"IncidentRouteID, HoW, DoW, ActualDuration, Vehicleid, EstimatedDuration,EstimatedDurationTraffic,EstimatedDistance");
                var i = 0;
                foreach (var r in routes)
                {
                    i++;
                    try
                    {
                        Logger.Write($"Analysing route {i}", GetType().Name);
                        CalculateEstimates(file, r, i);
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
        void CalculateEstimates(StreamWriter file, IncidentRouteView route, int index)
        {
            //            Logger.Write($"Loading track", GetType().Name);
            var track = Tracks.GetTrack($"db.inc:{route.IncidentRouteID}");

            if (track.Fixes.Count == 0)
                return;

            var starttime = track.Fixes.First().Timestamp;
            var endtime = track.Fixes.Last().Timestamp;
            var start = LatLongConverter.OSRefToWGS84(track.Fixes.First().Position);
            var end = LatLongConverter.OSRefToWGS84(track.Fixes.Last().Position);
            var dow = ((int)starttime.DayOfWeek + 6) % 7;
            var how = starttime.Hour + dow * 24;
            var actualDuration = (endtime - starttime).TotalSeconds;
            var startpos = track.Fixes.First().Position;
            var endpos = track.Fixes.Last().Position;

            string APIKEY = null;
            var baseURL = "https://maps.googleapis.com/maps/api/distancematrix/json";


            // make the time in the future
            while (starttime < DateTime.Now)
                starttime = starttime.AddDays(7);
            
            DistanceApi gmap = new DistanceApi(baseURL, new WebClientFactory());

            Result estimate = gmap.Calculate(start,end,starttime, key: APIKEY);


            var startPoint = _data.GetEdgeFromPoint(startpos);
            var endPoints = new List<EdgeWithOffset>
                            {
                                _data.GetEdgeFromPoint(endpos)
                            };

            var request = new RouteRequestMultiple
            {
                DistanceMax = 15000,
                DurationMax = 1800,
                InstanceMax = 1,
                StartLocation = startPoint,
                HourOfWeek = how,
                SearchType = RouteSearchType.Quickest,
                EndLocations = endPoints,
                VehicleType = track.VehicleType == 1 ? "AEU" : "FRU",
                RoadSpeedCalculator = _edgeCalculator.GetType().Name
            };

            var result = _selectedRouteEngine.CalculateRouteMultiple(request);
            var engineroute = result.Items[0];

            var links = engineroute.Connections.Count;
            var id = _edgeCalculator.GetId();

            var routing = engineroute.Connections.Select(x => x.Edge).ToList().ToArray();

            var csvLine = $"{route.IncidentRouteID},{how},{dow},{(int)actualDuration},{track.VehicleType},{estimate.Rows[0].Elements[0].Duration.Value},{estimate.Rows[0].Elements[0].DurationInTraffic.Value},{estimate.Rows[0].Elements[0].Distance.Value}";
            Debug.Print(csvLine);
            file.WriteLine(csvLine);
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