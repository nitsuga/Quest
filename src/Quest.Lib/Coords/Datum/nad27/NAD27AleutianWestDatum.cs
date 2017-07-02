using Quest.Lib.Coords.Ellipsoid;

namespace Quest.Lib.Coords.Datum.nad27
{
    /// <summary>
    ///     Class representing the NAD27 (Aleutian West) datum.
    /// </summary>
    public sealed class NAD27AleutianWestDatum : Datum<NAD27AleutianWestDatum>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NAD27AleutianWestDatum" /> class.
        /// </summary>
        public NAD27AleutianWestDatum()
        {
            name = "North American Datum 1927 (NAD27) - Aleutian West";
            Ellipsoid = Clarke1866Ellipsoid.Instance;
            dx = 2.0;
            dy = 204.0;
            dz = 105.0;
            Ds = 0.0;
            rx = 0.0;
            ry = 0.0;
            rz = 0.0;
        }
    }
}