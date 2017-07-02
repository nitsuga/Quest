using Quest.Lib.Coords.Ellipsoid;

namespace Quest.Lib.Coords.Datum.nad27
{
    /// <summary>
    ///     Class representing the NAD27 (Canada NW Territory) datum.
    /// </summary>
    public sealed class NAD27CanadaNWTerritoryDatum : Datum<NAD27CanadaNWTerritoryDatum>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NAD27CanadaNWTerritoryDatum" /> class.
        /// </summary>
        public NAD27CanadaNWTerritoryDatum()
        {
            name = "North American Datum 1927 (NAD27) - Canada NW Territory";
            Ellipsoid = Clarke1866Ellipsoid.Instance;
            dx = 4.0;
            dy = 159.0;
            dz = 188.0;
            Ds = 0.0;
            rx = 0.0;
            ry = 0.0;
            rz = 0.0;
        }
    }
}