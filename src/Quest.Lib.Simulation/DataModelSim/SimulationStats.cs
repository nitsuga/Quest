using System;
using System.Collections.Generic;

namespace Quest.Lib.Simulation.DataModelSim
{
    public partial class SimulationStats
    {
        public int SimulationStatsId { get; set; }
        public int? SimulationRunId { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Status { get; set; }
        public int? Quantity { get; set; }
        public int? VehicleTypeId { get; set; }

        public SimulationRun SimulationRun { get; set; }
    }
}
