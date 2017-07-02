#pragma warning disable 0169,649
using System;
using System.Linq;
using System.Threading;

namespace Quest.Lib.Routing.Speeds
{
    /// <summary>
    /// This class reads speed data from the database and provides a lookup service for speeds.
    /// 
    /// The map matching routines calculate speeds for each route and stores an entry in RoadSpeedItem
    /// for each roadlink traversed by each route.
    ///
    /// The stored procedure CalcRoadSpeeds fills a table RoadSpeedMatrix. It gets its data via 
    /// the view RoadLinksWithSpeed , which uses the RoadSpeedItem table. 
    /// 
    /// RoadSpeedMatrix is used by the view RoadSpeedMatrixSummary which is used by this 
    /// routine to determine the bounds of the data.
    /// 
    /// The data is loaded from RoadSpeedMatrix into a float array representing a speed for 500m x 500m blocks around London
    /// for each vehicle type, each road type and each hour of the day.
    /// </summary>
    public class SpeedDataMatrix
    {
        protected const int Cellsize = 500;
        protected float OverrideSpeedMph;
        protected int EastingMax;
        protected int EastingMin;
        protected int Hourmax;
        protected int Lowerx, Upperx;
        protected int Lowery, Uppery;
        protected int NorthingMax;
        protected int NorthingMin;
        protected int RMax;

        // hold the road speeds as [x,y,vehicle, road, hour]
        protected float[,,,,] Data;

        protected int VMax;
        protected readonly ManualResetEvent IsReady = new ManualResetEvent(false);

        /// <summary>
        ///     go through hour/road type nd vehicletype matrix and compute missing values
        ///     by calculating average of non-zero neibhouring cells
        /// </summary>
        protected void PatchMissingSpeeds()
        {
            var u2 = Data.GetUpperBound(2) + 1;
            var u3 = Data.GetUpperBound(3) + 1;
            var u4 = Data.GetUpperBound(4) + 1;

            // make a new array containing just the patched values.. this gets merged later
            var fixedRoadSpeeds =
                new float[1 + (EastingMax - EastingMin) / Cellsize, 1 + (NorthingMax - NorthingMin) / Cellsize, VMax,
                    RMax + 1, Hourmax];

            // calcate a series of parameters
            var xValues = Enumerable.Range(Lowerx, Upperx);
            var yValues = Enumerable.Range(Lowery, Uppery);

            var allXy = xValues.SelectMany(x => yValues, (x, y) => new { x, y });

            for (var roadType = 0; roadType < u3; roadType++)
            {
                for (var vehicleType = 0; vehicleType < u2; vehicleType++)
                {
                    for (var hour = 0; hour < u4; hour++)
                    {
                        var vtype = vehicleType;
                        var rtype = roadType;
                        var hour1 = hour;
                        allXy.AsParallel()
                            .ForAll(
                                p =>
                                {
                                    fixedRoadSpeeds[p.x, p.y, vtype, rtype, hour1] = CalculateAverageSpeed(p.x, p.y,
                                        vtype, rtype, hour1);
                                }
                            );
                    }
                }
            }

            for (var roadType = 0; roadType < u3; roadType++)
                for (var vehicleType = 0; vehicleType < u2; vehicleType++)
                    for (var hour = 0; hour < u4; hour++)
                    {
                        var vtype = vehicleType;
                        var rtype = roadType;
                        var hour1 = hour;
                        allXy.AsParallel().ForAll(p =>
                        {
                            //double speed2 = Data[p.x, p.y, vtype, rtype, hour1];
                            //if (Math.Abs(speed2) < 0.1)
                            Data[p.x, p.y, vtype, rtype, hour1] =
                                    fixedRoadSpeeds[p.x, p.y, vtype, rtype, hour1];
                        }
                            );
                    }
        }

        private float CalculateAverageSpeed(int x, int y, int vehicleType, int roadType, int hour)
        {
            var count = 0;
            float sum = 0;
            const int maxradius = 5;

            var speed2 = Data[x, y, vehicleType, roadType, hour];
            if (Math.Abs(speed2) > 0.1)
                return speed2;

            // start off small and increase number of squares until you find something
            int radius;
            for (radius = 0, count = 0; radius < maxradius && count == 0; radius++)
            {
                // scan for average speed
                int ya;
                for (ya = y - radius; ya <= y + radius; ya++)
                {
                    // check within limits
                    if (ya < Lowery)
                        continue;

                    // check within limits
                    if (ya > Uppery)
                        continue;

                    int xa;
                    for (xa = x - radius; xa <= x + radius; xa++)
                    {
                        // check within limits
                        if (xa < Lowerx)
                            continue;

                        // check within limits
                        if (xa > Upperx)
                            continue;

                        var speed = Data[xa, ya, vehicleType, roadType, hour];
                        if (!(Math.Abs(speed) > 0.1)) continue;
                        sum += 1 / speed;
                        count++;
                    }
                }

                if (sum > 0)
                    break;
            }

            return count == 0 ? OverrideSpeedMph : 1 / (sum / count);
        }
    }
}