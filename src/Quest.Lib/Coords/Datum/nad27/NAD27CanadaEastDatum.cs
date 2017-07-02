using Quest.Lib.Coords.Ellipsoid;

namespace Quest.Lib.Coords.Datum.nad27
{
    /// <summary>
    ///     Class representing the NAD27 (Canada East) datum.
    /// </summary>
    public sealed class NAD27CanadaEastDatum : Datum<NAD27CanadaEastDatum>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NAD27CanadaEastDatum" /> class.
        /// </summary>
        public NAD27CanadaEastDatum()
        {
            name = "North American Datum 1927 (NAD27) - Canada East";
            Ellipsoid = Clarke1866Ellipsoid.Instance;
            dx = -22.0;
            dy = 160.0;
            dz = 190.0;
            Ds = 0.0;
            rx = 0.0;
            ry = 0.0;
            rz = 0.0;
        }
    }
}