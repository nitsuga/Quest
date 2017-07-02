using GeoAPI.Geometries;

namespace Quest.Common.Simulation
{
    /// <summary>
    /// represents a vehicle that exists in the system, used initially to load vehicles into the system
    /// </summary>
    public class SimVehicle
    {
        public int VehicleId;
        public string VehicleType;
        public Coordinate Position;
    }

}
