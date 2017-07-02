#define DubiousCode
using System;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using Quest.Common.Messages;
using Quest.Lib.Constants;
using Quest.Lib.DataModel;

namespace Quest.Lib.Routing.Speeds
{
    /// <summary>
    /// Calculate a road speed by looking in RoadlinksWithSpeed to see if there is a specific speed for the given roadlink.
    /// 
    /// If there are no 
    /// </summary>
    public class VariableSpeedByEdge : IRoadSpeedCalculator
    {
        private Dictionary<int, RoadSpeed[]> _allspeeds;

        private readonly long[] _usageCounts  = new long[10];

        
        private SpeedDataHoW _speeddata;

        public VariableSpeedByEdge()
        {
            Initialise();
        }

        public VariableSpeedByEdge(SpeedDataHoW speeddata)
        {
            _speeddata = speeddata;
            Initialise();
        }

        public RoadVector CalculateEdgeCost(string vehicletype, int hourOfWeek, RoadEdge edge)
        {
            var vid = vehicletype == "AEU" ? 1 : 2;
            return CalculateEdgeCost(hourOfWeek, edge.RoadLinkEdgeId, edge.RoadTypeId, edge.Geometry.Coordinates.First(),
                vid, edge.Geometry.Length);
        }

        public int GetId()
        {
            return 5;
        }

        public void Initialise()
        {
            // 64 bit - preload all the data    
            if (_allspeeds == null)
            {
                if (IntPtr.Size == 8)
                {
                    using (var context = new QuestEntities())
                    {
                        context.Configuration.ProxyCreationEnabled = false;
                        _allspeeds =
                            context.RoadSpeeds.AsNoTracking()
                                .ToArray()
                                .GroupBy(x => x.RoadLinkEdgeId??0)
                                .ToDictionary(x => x.Key, y => y.ToArray());
                    }
                }
                else
                {
                    _allspeeds = new Dictionary<int, RoadSpeed[]>();
                }
            }
        }

        /// <summary>
        /// Get speeds from the database
        /// </summary>
        /// <param name="roadLinkEdgeId"></param>
        /// <returns></returns>
        private RoadSpeed[] GetSpeeds(int roadLinkEdgeId)
        {
            lock (_allspeeds)
            {
                RoadSpeed[] s;
                _allspeeds.TryGetValue(roadLinkEdgeId, out s);
                if (s != null)
                    return s;
                if (IntPtr.Size == 8)
                {
                    // if we have already loaded all the data dont try and find it again
                    return null;
                }

                using (var context = new QuestEntities())
                {
                    context.Configuration.ProxyCreationEnabled = false;
                    var speeds = context.RoadSpeeds.AsNoTracking().Where(x => x.RoadLinkEdgeId == roadLinkEdgeId).ToArray();

                    // add to the cache
                    _allspeeds.Add(roadLinkEdgeId, speeds);
                    return speeds;
                }
            }
        }

        public void PrintUsageCounts()
        {
            Console.WriteLine($"Edge Usage Counts");
            foreach (var v in _usageCounts)
            {
                Console.WriteLine($"{v}");
            }
        }

        private RoadVector CalculateEdgeCost(int hourOfWeek, int roadLinkEdgeId, int roadTypeId, Coordinate coord, int vid,
            double length)
        {
            double speed = -1;

            _usageCounts[0]++;

            var speeds = GetSpeeds(roadLinkEdgeId);

            // no data, return estimate
            if (speeds == null || speeds.Length == 0)
            {
                _usageCounts[1]++;
                speed = _speeddata.GetRoadSpeedMphHoW(roadTypeId, coord, vid, hourOfWeek);
                goto complete;

            }

            _usageCounts[2]++;

            // find the right vehicle/hour
            var f = speeds.Where(x => x.HourOfWeek == hourOfWeek && x.VehicleId == vid).ToArray();
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
            var g = speeds.Where(x => x.HourOfWeek == hourOfWeek);
            if (g.Any())
            {   
                speed = g.Average(x => x.SpeedAvg);
                goto complete;
            }

            _usageCounts[4]++;

            // not the hour or the vehicle, try hour of day for right part of the week
            // nothing for this hour of week, try the Monday to Friday
            var hoD = hourOfWeek%24;
            var h = speeds.Where(x => x.HourOfWeek%24 == hoD && x.HourOfWeek < 24*5);
            if (h.Any())
            {
                speed = h.Average(x => x.SpeedAvg);
                goto complete;
            }

            _usageCounts[5]++;

            var i = speeds.Where(x => x.HourOfWeek%24 == hoD && x.HourOfWeek >= 24*5);
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

            var speedMs = (speed*Constant.mph2ms);

            return new RoadVector
            {
                DistanceMeters = length,
                DurationSecs = length/ speedMs,
                SpeedMs = speedMs
            };
        }
    }
}