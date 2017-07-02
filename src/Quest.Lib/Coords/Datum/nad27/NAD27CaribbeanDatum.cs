using Quest.Lib.Coords.Ellipsoid;

namespace Quest.Lib.Coords.Datum.nad27
{
    /// <summary>
    ///     Class representing the NAD27 (Caribbean) datum.
    /// </summary>
    public sealed class NAD27CaribbeanDatum : Datum<NAD27CaribbeanDatum>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NAD27CaribbeanDatum" /> class.
        /// </summary>
        public NAD27CaribbeanDatum()
        {
            name = "North American Datum 1927 (NAD27) - Caribbean";
            Ellipsoid = Clarke1866Ellipsoid.Instance;
            dx = -3.0;
            dy = 142.0;
            dz = 183.0;
            Ds = 0.0;
            rx = 0.0;
            ry = 0.0;
            rz = 0.0;
        }
    }
}