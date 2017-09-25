using Quest.Common.Simulation;
using Quest.Lib.Simulation.DataModelSim;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quest.Lib.Simulation.Resources
{
    public class RosterStoreDb : IRosterStore
    {
        List<VehicleRoster> _roster = new List<VehicleRoster>();

        public List<VehicleRoster> GetRoster(DateTime validAt)
        {
            return _roster
                .Where(x => x.Duration.TotalSeconds > 0)
                .Where(x => x.StartTime.Date == validAt.Date)
                .Where(x => x.StartTime.Hour == validAt.Hour)
                .ToList();
        }

        public void LoadRoster(DateTime from, DateTime to)
        {
            using (var db = new QuestSimContext())
            {
                _roster = db.VehicleViews
                        .ToList()
                        .Select(x => new VehicleRoster
                        {
                            Callsign = x.Callsign,
                            VehicleType = x.VehicleType,
                            StartPosition = new GeoAPI.Geometries.Coordinate(x.Easting ?? 0, x.Northing ?? 0)
                        })
                        .ToList();
            }
        }
    }
}
