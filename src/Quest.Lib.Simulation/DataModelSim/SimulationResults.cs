using System;

namespace Quest.Lib.Simulation.DataModelSim
{
    public partial class SimulationResults
    {
        public int SimulationResultId { get; set; }
        public long? Incidentid { get; set; }
        public int? Frdelay { get; set; }
        public int? FrresourceId { get; set; }
        public int? AmbDelay { get; set; }
        public int? TurnAround { get; set; }
        public int? AmbResourceId { get; set; }
        public int? OnScene { get; set; }
        public int SimulationRunId { get; set; }
        public DateTime? Closed { get; set; }
        public int HospitalDelay { get; set; }
        public DateTime? CallStart { get; set; }
        public int? Category { get; set; }
        public int? HourOrdinal { get; set; }
        public bool? Inside { get; set; }

        public Vehicles AmbResource { get; set; }
        public Vehicles Frresource { get; set; }
        public SimulationIncidents Incident { get; set; }
        public SimulationRun SimulationRun { get; set; }
    }
}
