#pragma warning disable 0169,649
using System;
using System.Linq;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using Quest.Lib.DataModel;
using Quest.Lib.Trace;

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
    public class SpeedDataHoW : SpeedDataMatrix
    {
        public SpeedDataHoW()
        {
            // load the data and set the flag when done
            Task.Run(() =>
            {
                LoadSpeedDataHoW();
            }).ContinueWith(x =>
            {
                IsReady.Set();
            });
            
        }

        private void LoadSpeedDataHoW(float overrideSpeedMph = 25)
        {
            OverrideSpeedMph = overrideSpeedMph;

            try
            {
                using (var context = new QuestEntities())
                {
                    var summaries = context.RoadSpeedMatrixHoWSummaries.FirstOrDefault();
                    if (summaries != null)
                    {
                        RMax = summaries.MaxRoadType ?? 0;
                        EastingMax = summaries.MaxX ?? 0;
                        NorthingMax = summaries.MaxY ?? 0;
                        EastingMin = summaries.MinX ?? 0;
                        NorthingMin = summaries.MinY ?? 0;
                        VMax = summaries.MaxVehicleType ?? 0;
                        Hourmax = summaries.HourCount ?? 0;
                    }

                    var dimx = 1 + (EastingMax - EastingMin) / Cellsize;
                    var dimy = 1 + (NorthingMax - NorthingMin) / Cellsize;
                    // create and array
                    Data = new float[dimx, dimy, VMax, RMax + 1, Hourmax];

                    foreach (var reader in context.RoadSpeedMatrixHoWs.AsNoTracking())
                    {
                        var x = (reader.GridX - EastingMin) / Cellsize;
                        var y = (reader.GridY - NorthingMin) / Cellsize;

                        var s = (int)reader.AvgSpeed;
                        if (s < 10)
                            s = 10;

                        Data[x, y, reader.VehicleId - 1, reader.RoadTypeId, reader.HourOfWeek] = s;
                    }
                }

                Lowerx = Data.GetLowerBound(0);
                Upperx = Data.GetUpperBound(0);
                Lowery = Data.GetLowerBound(1);
                Uppery = Data.GetUpperBound(1);

                PatchMissingSpeeds();
            }
            catch (Exception ex)
            {
                Logger.Write(ex.ToString(), GetType().Name);
            }
        }

        /// <summary>
        /// Get a road speed from the matrix
        /// </summary>
        /// <param name="roadType">Road type as defined in the database table 'RoadTypes'</param>
        /// <param name="coordBng">Coordinates in British National Grid</param>
        /// <param name="vehicleType">1=AEU 2=FRU</param>
        /// <param name="hourOfWeek">0-163 hour of the week</param>
        /// <returns>the eastimated speed in MPH</returns>
        public float GetRoadSpeedMphHoW(int roadType, Coordinate coordBng, int vehicleType, int hourOfWeek)
        {
            // wait for data to be loaded
            IsReady.WaitOne();

            if (Data == null)
                return OverrideSpeedMph;

            if (vehicleType > Data.GetUpperBound(2))
                vehicleType = Data.GetUpperBound(2) - 1;

            var x = (coordBng.X - EastingMin)/Cellsize;
            var y = (coordBng.Y - NorthingMin)/Cellsize;

            if (y < Data.GetLowerBound(1))
                y = Data.GetLowerBound(1);

            if (x < Data.GetLowerBound(0))
                x = Data.GetLowerBound(0);

            if (y > Data.GetUpperBound(1))
                y = Data.GetUpperBound(1);

            if (x > Data.GetUpperBound(0))
                x = Data.GetUpperBound(0);

            var speed = Data[(int) x, (int) y, vehicleType, roadType, hourOfWeek];
            return speed;
        }

    }
}