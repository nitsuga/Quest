using System;
using System.Collections.Generic;

namespace Quest.Lib.Simulation.DataModelSim
{
    public partial class SimulationIncidents
    {
        public SimulationIncidents()
        {
            SimulationResults = new HashSet<SimulationResults>();
        }

        public long IncidentId { get; set; }
        public DateTime CallStart { get; set; }
        public DateTime Ampdstime { get; set; }
        public int? Easting { get; set; }
        public int? Northing { get; set; }
        public string Ampdscode { get; set; }
        public int? Category { get; set; }
        public bool? WasConveyed { get; set; }
        public bool? WasDispatched { get; set; }
        public bool? OutsideLas { get; set; }

        public ICollection<SimulationResults> SimulationResults { get; set; }
    }
}
