using Quest.Lib.Coords.Ellipsoid;

namespace Quest.Lib.Coords.Datum.nad27
{
    /// <summary>
    ///     Class representing the NAD27 (Canal Zone) datum.
    /// </summary>
    public sealed class NAD27CanalZoneDatum : Datum<NAD27CanalZoneDatum>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NAD27CanalZoneDatum" /> class.
        /// </summary>
        public NAD27CanalZoneDatum()
        {
            name = "North American Datum 1927 (NAD27) - Canal Zone";
            Ellipsoid = Clarke1866Ellipsoid.Instance;
            dx = 0.0;
            dy = 125.0;
            dz = 201.0;
            Ds = 0.0;
            rx = 0.0;
            ry = 0.0;
            rz = 0.0;
        }
    }
}