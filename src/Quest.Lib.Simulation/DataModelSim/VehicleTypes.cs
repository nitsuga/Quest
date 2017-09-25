using System;
using System.Collections.Generic;

namespace Quest.Lib.Simulation.DataModelSim
{
    public partial class VehicleTypes
    {
        public VehicleTypes()
        {
            VehicleRoster = new HashSet<VehicleRoster>();
            Vehicles = new HashSet<Vehicles>();
        }

        public int VehicleTypeId { get; set; }
        public string Name { get; set; }

        public ICollection<VehicleRoster> VehicleRoster { get; set; }
        public ICollection<Vehicles> Vehicles { get; set; }
    }
}
