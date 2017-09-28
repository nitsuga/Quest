using System.Collections.Generic;

namespace Quest.Lib.Simulation.DataModelSim
{
    public partial class Destinations
    {
        public Destinations()
        {
            Vehicles = new HashSet<Vehicles>();
        }

        public int DestinationId { get; set; }
        public string Destination { get; set; }
        public bool IsHospital { get; set; }
        public bool IsStandby { get; set; }
        public bool IsStation { get; set; }
        public bool IsRoad { get; set; }
        public bool? IsPolice { get; set; }
        public double E { get; set; }
        public double N { get; set; }
        public int? GroupId { get; set; }

        public ICollection<Vehicles> Vehicles { get; set; }
    }
}
