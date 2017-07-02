using Quest.Lib.Coords.Ellipsoid;

namespace Quest.Lib.Coords.Datum
{
    /// <summary>
    ///     Class representing the Ordnance Survey of Great Britain 1936 (OSGB36) datum.
    /// </summary>
    public sealed class OSGB36Datum : Datum<OSGB36Datum>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OSGB36Datum" /> class.
        /// </summary>
        public OSGB36Datum()
        {
            name = "Ordnance Survey of Great Britain 1936 (OSGB36)";
            Ellipsoid = Airy1830Ellipsoid.Instance;
            dx = 446.448;
            dy = -125.157;
            dz = 542.06;
            Ds = -20.4894;
            rx = 0.1502;
            ry = 0.2470;
            rz = 0.8421;
        }
    }
}