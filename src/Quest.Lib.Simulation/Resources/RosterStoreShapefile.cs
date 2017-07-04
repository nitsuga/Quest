using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Quest.Common.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCrontab;
using NCrontab.Advanced;

namespace Quest.Lib.Simulation.Resources
{
    public class RosterStoreShapefile : IRosterStore
    {
        class CronRoster
        {
            internal int Id;
            internal string Callsign;
            internal string VehicleType;
            internal DateTime ValidFrom;
            internal DateTime ValidTo;
            internal CrontabSchedule Cron;
            internal TimeSpan Duration;
            internal IGeometry Geom;
        }

        private List<CronRoster> _roster = new List<CronRoster>();

        public string Filename { get; set; }

        /// <summary>
        /// return a list of vehicles that are on duty at this time
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public List<VehicleRoster> GetRoster(DateTime validAt)
        {
            List<VehicleRoster> results = new List<VehicleRoster>();

            foreach(var r in _roster)
            {
                var occurrences = r.Cron.GetNextOccurrences(validAt.AddDays(-1), validAt.AddDays(1));
                if (occurrences.Count() > 0)
                {
                    var firstValidPeriod = occurrences.FirstOrDefault(x =>  x <= validAt && x.Add(r.Duration) >= validAt);
                    if (firstValidPeriod != null && firstValidPeriod!=DateTime.MinValue)
                        results.Add(new VehicleRoster
                        {
                            Callsign = r.Callsign,
                            Duration = r.Duration,
                            StartPosition = r.Geom.Coordinate,
                            StartTime = firstValidPeriod,
                            VehicleType = r.VehicleType
                        });
                }
            }
            return results;
        }

        /// <summary>
        /// load the roster for the simulation period
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void LoadRoster(DateTime from, DateTime to)
        {
            IGeometryFactory geomFact = new GeometryFactory();
            var cwd = System.IO.Directory.GetCurrentDirectory();
            var path = System.IO.Path.Combine(cwd, Filename);
            using (var reader = new ShapefileDataReader( path, geomFact))
            {
                while (reader.Read())
                {
                    CronRoster roster = new CronRoster
                    {
                        Id = reader.GetInt32(0),
                        Callsign = reader.GetString(1),
                        VehicleType = reader.GetString(2),
                        Cron = CrontabSchedule.Parse(reader.GetString(3)),
                        ValidFrom = reader.GetDateTime(4),
                        ValidTo = reader.GetDateTime(5),
                        Duration = new TimeSpan(0,reader.GetInt32(6),0),
                        Geom = reader.Geometry
                };

                    if (roster.ValidFrom<to && roster.ValidTo>from)
                        _roster.Add(roster);

                }
            }
        }
    }
}
