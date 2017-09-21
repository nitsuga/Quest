using System;
using System.Collections.Generic;

namespace Quest.Lib.Research.DataModelResearch
{
    public partial class Activations
    {
        public int ActivationId { get; set; }
        public long IncidentId { get; set; }
        public DateTime? Dispatched { get; set; }
        public DateTime? Arrived { get; set; }
        public string Callsign { get; set; }
        public int? VehicleId { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
    }
}
