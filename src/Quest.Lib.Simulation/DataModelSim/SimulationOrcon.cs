using System;
using System.Collections.Generic;

namespace Quest.Lib.Simulation.DataModelSim
{
    public partial class SimulationOrcon
    {
        public int SimulationOrconId { get; set; }
        public int? SimulationRunId { get; set; }
        public int? HourOrdinal { get; set; }
        public int? CatAoutside { get; set; }
        public int? CatAinside { get; set; }
        public int? CatBoutside { get; set; }
        public int? CatBinside { get; set; }
        public float? Performance { get; set; }
        public float? PerfA { get; set; }
        public float? PerfB { get; set; }

        public SimulationRun SimulationRun { get; set; }
    }
}
