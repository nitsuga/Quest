using Quest.Lib.Coords.Ellipsoid;

namespace Quest.Lib.Coords.Datum
{
    /// <summary>
    ///     Class representing the Ireland 1965 datum.
    /// </summary>
    public sealed class Ireland1965Datum : Datum<Ireland1965Datum>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Ireland1965Datum" /> class.
        /// </summary>
        public Ireland1965Datum()
        {
            name = "Ireland 1965";
            Ellipsoid = ModifiedAiryEllipsoid.Instance;
            dx = 482.53;
            dy = -130.596;
            dz = 564.557;
            Ds = 8.15;
            rx = -1.042;
            ry = -0.214;
            rz = -0.631;
        }
    }
}