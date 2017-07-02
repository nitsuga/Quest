using Quest.Lib.Coords.Ellipsoid;

namespace Quest.Lib.Coords.Datum.nad27
{
    /// <summary>
    ///     Class representing the NAD27 (Alaska) datum.
    /// </summary>
    public sealed class NAD27AlaskaDatum : Datum<NAD27AlaskaDatum>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NAD27AlaskaDatum" /> class.
        /// </summary>
        public NAD27AlaskaDatum()
        {
            name = "North American Datum 1927 (NAD27) - Alaska";
            Ellipsoid = Clarke1866Ellipsoid.Instance;
            dx = -5.0;
            dy = 135.0;
            dz = 172.0;
            Ds = 0.0;
            rx = 0.0;
            ry = 0.0;
            rz = 0.0;
        }
    }
}