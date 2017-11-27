using System;

namespace Quest.Common.Messages.Routing
{
    /// <summary>
    /// override the current location of where a vehicle is
    /// </summary>
    [Serializable]
    public class VehicleOverride
    {
        /// <summary>
        /// callsign of the vehicle
        /// </summary>
        public string Callsign { get; set; }

        
        public int Easting { get; set; }

        
        public int Northing { get; set; }
    }
}