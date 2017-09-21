#define DubiousCode
using System;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using Quest.Common.Messages;
using Quest.Lib.Constants;
using Quest.Lib.DataModel;
using Quest.Lib.Trace;
using System.Threading;

namespace Quest.Lib.Routing.Speeds
{
    /// <summary>
    /// Calculate a road speed by looking in RoadlinksWithSpeed to see if there is a specific speed for the given roadlink.
    /// 
    /// If there are no 
    /// </summary>
    public class VariableSpeedByEdge : IRoadSpeedCalculator
    {
        private RoutingData _routingdata;

        /// <summary>
        /// dictionary of road link ids with associated speeds for how and vehicle type
        /// </summary>
        private Dictionary<int, double[,]> _speeds = new Dictionary<int, double[,]>();

        private readonly long[] _usageCounts = new long[10];

        private SpeedDataHoW _speeddata;
        private ConstantSpeedCalculator _constspeeddata;

        public VariableSpeedByEdge(SpeedDataHoW speeddata, ConstantSpeedCalculator constspeeddata, RoutingData routingdata)
        {
            _speeddata = speeddata;
            _constspeeddata = constspeeddata;
            _routingdata = routingdata;

            Initialise();
        }

        public int GetId()
        {
            return 5;
        }

        public void Initialise()
        {
            // 64 bit - preload all the data    
            if (IntPtr.Size != 8)
            {
                Logger.Write("Need 64bit arhitecture for this estimator", GetType().Name, System.Diagnostics.TraceEventType.Error);
            }

            using (var context = new QuestContext())
            {
                Logger.Write("Loading road speeds", GetType().Name);
                // count roadlinks
                var speeds =
                    context.RoadSpeed
                        .ToArray()
                        .GroupBy(x => x.RoadLinkEdgeId ?? 0);

                Logger.Write("Waiting for road network to load", GetType().Name);

                while (!_routingdata.IsInitialised)
                    Thread.Sleep(1);

                Logger.Write("Estimating missing speeds", GetType().Name);

                foreach (var k in speeds)
                {
                    var spds = k.ToArray();
                    var data = MakeSpeedArray(k.Key, spds);
                    _speeds.Add(k.Key, data);
                }

                Logger.Write("Estimation of missing speeds completed", GetType().Name);

            }

        }
        

        private double[,] MakeSpeedArray(int roadLinkEdgeId, RoadSpeed[] data)
        {
            double[,] a = new double[168, 2];
            foreach (var d in data)
                a[d.HourOfWeek, d.VehicleId-1] = d.SpeedAvg;
            for (int v = 0; v < 2; v++)
                for (int how = 0; how < 168; how++)
                    if (a[how, v] == 0)
                        a[how, v] = CalcEstimateSpeed(roadLinkEdgeId, how, v, data);
            return a;
        }

        private double CalcEstimateSpeed(int roadLinkEdgeId, int how, int v, RoadSpeed[] speeds)
        {
            double speed = -1;

            // no data, return estimate
            if (speeds == null || speeds.Length == 0)
            {
                _usageCounts[1]++;
                // get coordinate from routing data
                var edge = _routingdata.Dict[roadLinkEdgeId];
                speed = _speeddata.GetRoadSpeedMphHoW(edge.RoadTypeId, edge.Envelope.Centre, v, how);
                goto complete;
            }

            _usageCounts[2]++;

            // find the right vehicle/hour
            var f = speeds.Where(x => x.HourOfWeek == how && x.VehicleId == v+1).ToArray();
            if (f.Any())
            {
                speed = f.Average(x => x.SpeedAvg);
                goto complete;
            }

            //TODO: Lawrence change this block to use both RoadSpeedMatrixHoWSummaries and RoadSpeedMatrixDoWSummaries instead.
#if true
            _usageCounts[3]++;

            // maybe just the hour?
            // find the right hour, not the right vehicle
            var g = speeds.Where(x => x.HourOfWeek == how);
            if (g.Any())
            {
                speed = g.Average(x => x.SpeedAvg);
                goto complete;
            }

            _usageCounts[4]++;

            // not the hour or the vehicle, try hour of day for right part of the week
            // nothing for this hour of week, try the Monday to Friday
            var hoD = how % 24;
            var h = speeds.Where(x => x.HourOfWeek % 24 == hoD && x.HourOfWeek < 24 * 5);
            if (h.Any())
            {
                speed = h.Average(x => x.SpeedAvg);
                goto complete;
            }

            _usageCounts[5]++;

            var i = speeds.Where(x => x.HourOfWeek % 24 == hoD && x.HourOfWeek >= 24 * 5);
            if (i.Any())
            {
                speed = i.Average(x => x.SpeedAvg);
            }
            else
            {
                speed = speeds.Average(x => x.SpeedAvg);
            }
#endif
        complete:
            return speed;
        }

        public void PrintUsageCounts()
        {
            Console.WriteLine($"Edge Usage Counts");
            foreach (var v in _usageCounts)
            {
                Console.WriteLine($"{v}");
            }
        }

        public RoadVector CalculateEdgeCost(string vehicletype, int hourOfWeek, RoadEdge edge)
        {
            var vid = vehicletype == "AEU" ? 0 : 1;

            double[,] speeds = null;
            _speeds.TryGetValue(edge.RoadLinkEdgeId, out speeds);

            // no data, return estimate
            if (speeds == null || speeds.Length == 0)
                return _constspeeddata.CalculateEdgeCost(vehicletype, hourOfWeek, edge);

            var speed = speeds[hourOfWeek, vid];

            if (speed == 0)
                return _constspeeddata.CalculateEdgeCost(vehicletype, hourOfWeek, edge);

            var speedMs = (speed*Constant.mph2ms);

            return new RoadVector
            {
                DistanceMeters = edge.Length,
                DurationSecs = edge.Length / speedMs,
                SpeedMs = speedMs
            };
        }
    }
}