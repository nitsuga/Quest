using System;

namespace Quest.Lib.Simulation.DataModelSim
{
    public partial class SimulationAssignments
    {
        public int SimulationAssignments1 { get; set; }
        public int? SimulationRunId { get; set; }
        public DateTime? Tstamp { get; set; }
        public string Callsign { get; set; }
        public string Incident { get; set; }
        public string Action { get; set; }

        public SimulationRun SimulationRun { get; set; }
    }
}
