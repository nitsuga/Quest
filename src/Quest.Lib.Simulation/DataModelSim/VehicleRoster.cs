using System;
using System.Collections.Generic;

namespace Quest.Lib.Simulation.DataModelSim
{
    public partial class VehicleRoster
    {
        public int VehicleRosterid { get; set; }
        public DateTime? Period { get; set; }
        public int? Minutesactual { get; set; }
        public string Callsign { get; set; }
        public int? Easting { get; set; }
        public int? Northing { get; set; }
        public string StationId { get; set; }
        public int? VehicleTypeId { get; set; }

        public VehicleTypes VehicleType { get; set; }
    }
}
