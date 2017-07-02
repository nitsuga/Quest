using Quest.Lib.Coords.Ellipsoid;

namespace Quest.Lib.Coords.Datum.nad27
{
    /// <summary>
    ///     Class representing the NAD27 (Western US) datum.
    /// </summary>
    public sealed class NAD27WesternUSDatum : Datum<NAD27WesternUSDatum>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NAD27WesternUSDatum" /> class.
        /// </summary>
        public NAD27WesternUSDatum()
        {
            name = "North American Datum 1927 (NAD27) - Western US";
            Ellipsoid = Clarke1866Ellipsoid.Instance;
            dx = -8.0;
            dy = 159.0;
            dz = 175.0;
            Ds = 0.0;
            rx = 0.0;
            ry = 0.0;
            rz = 0.0;
        }
    }
}