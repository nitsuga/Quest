//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Quest.Lib.Simulation.Model
{
    using System;

    public partial class SimulationResult
    {
        public int SimulationResultId { get; set; }
        public Nullable<long> Incidentid { get; set; }
        public Nullable<int> FRDelay { get; set; }
        public Nullable<int> FRResourceId { get; set; }
        public Nullable<int> AmbDelay { get; set; }
        public Nullable<int> TurnAround { get; set; }
        public Nullable<int> AmbResourceId { get; set; }
        public Nullable<int> OnScene { get; set; }
        public int SimulationRunId { get; set; }
        public Nullable<System.DateTime> Closed { get; set; }
        public int HospitalDelay { get; set; }
        public Nullable<System.DateTime> CallStart { get; set; }
        public Nullable<int> Category { get; set; }
        public Nullable<int> HourOrdinal { get; set; }
        public Nullable<bool> Inside { get; set; }
    
        public virtual SimulationIncident SimulationIncident { get; set; }
        public virtual SimulationRun SimulationRun { get; set; }
        public virtual Vehicle Vehicle { get; set; }
        public virtual Vehicle Vehicle1 { get; set; }
    }
}
