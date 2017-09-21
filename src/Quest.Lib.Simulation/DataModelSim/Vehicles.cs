using System;
using System.Collections.Generic;

namespace Quest.Lib.Simulation.DataModelSim
{
    public partial class Vehicles
    {
        public Vehicles()
        {
            SimulationResultsAmbResource = new HashSet<SimulationResults>();
            SimulationResultsFrresource = new HashSet<SimulationResults>();
        }

        public int VehicleId { get; set; }
        public int VehicleTypeId { get; set; }
        public int? SkillLevel { get; set; }
        public string Mpcname { get; set; }
        public int? Easting { get; set; }
        public int? Northing { get; set; }
        public string Callsign { get; set; }
        public int? FleetId { get; set; }
        public int? DefaultDestinationId { get; set; }
        public int? GroupId { get; set; }

        public Destinations DefaultDestination { get; set; }
        public ICollection<SimulationResults> SimulationResultsAmbResource { get; set; }
        public ICollection<SimulationResults> SimulationResultsFrresource { get; set; }
    }
}
