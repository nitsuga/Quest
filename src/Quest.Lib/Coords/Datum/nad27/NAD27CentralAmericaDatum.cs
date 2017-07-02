using Quest.Lib.Coords.Ellipsoid;

namespace Quest.Lib.Coords.Datum.nad27
{
    /// <summary>
    ///     Class representing the NAD27 (Central America) datum.
    /// </summary>
    public sealed class NAD27CentralAmericaDatum : Datum<NAD27CentralAmericaDatum>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NAD27CentralAmericaDatum" /> class.
        /// </summary>
        public NAD27CentralAmericaDatum()
        {
            name = "North American Datum 1927 (NAD27) - Central America";
            Ellipsoid = Clarke1866Ellipsoid.Instance;
            dx = 0.0;
            dy = 125.0;
            dz = 194.0;
            Ds = 0.0;
            rx = 0.0;
            ry = 0.0;
            rz = 0.0;
        }
    }
}