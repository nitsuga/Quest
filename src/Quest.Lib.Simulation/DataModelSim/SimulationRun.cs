using System;
using System.Collections.Generic;

namespace Quest.Lib.Simulation.DataModelSim
{
    public partial class SimulationRun
    {
        public SimulationRun()
        {
            SimulationAssignments = new HashSet<SimulationAssignments>();
            SimulationOrcon = new HashSet<SimulationOrcon>();
            SimulationResults = new HashSet<SimulationResults>();
            SimulationStats = new HashSet<SimulationStats>();
        }

        public int SimulationRunId { get; set; }
        public string Notes { get; set; }
        public string Constants { get; set; }
        public float? Performance { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Stopped { get; set; }

        public ICollection<SimulationAssignments> SimulationAssignments { get; set; }
        public ICollection<SimulationOrcon> SimulationOrcon { get; set; }
        public ICollection<SimulationResults> SimulationResults { get; set; }
        public ICollection<SimulationStats> SimulationStats { get; set; }
    }
}
