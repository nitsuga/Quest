using System;
using Quest.Lib.Coords.Datum;

namespace Quest.Lib.Coords
{
    /// <summary>
    ///     Class to represent an Irish National Grid reference.
    ///     <p>
    ///         <b>Irish National Grid</b><br />
    ///         <ul>
    ///             <li>Projection: Transverse Mercator</li>
    ///             <li>Reference ellipsoid: Modified Airy</li>
    ///             <li>Units: metres</li>
    ///             <li>Origin: 53°30'N, 8°W</li>
    ///             <li>False co-ordinates of origin: 200000m east, 250000m north</li>
    ///         </ul>
    ///     </p>
    /// </summary>
    public class IrishRef : CoordinateSystem
    {
        private static readonly double SCALE_FACTOR = 1.000035;

        private static readonly double FALSE_ORIGIN_LATITUDE = 53.5;

        private static readonly double FALSE_ORIGIN_LONGITUDE = -8.0;

        private static readonly double FALSE_ORIGIN_EASTING = 200000.0;

        private static readonly double FALSE_ORIGIN_NORTHING = 250000.0;
        /**
     * The easting in metres relative to the origin of the British National Grid.
     */
        private double easting;

        /**
     * The northing in metres relative to the origin of the British National Grid.
     */
        private double northing;

        /// <summary>
        ///     Create a new Ordnance Survey grid reference using a given easting and
        ///     northing. The easting and northing must be in metres and must be relative
        ///     to the origin of the British National Grid.
        /// </summary>
        /// <param name="easting">
        ///     the easting in metres. Must be greater than or equal to 0.0 and
        ///     less than 800000.0.
        /// </param>
        /// <param name="northing">
        ///     the northing in metres. Must be greater than or equal to 0.0 and
        ///     less than 1400000.0.
        /// </param>
        public IrishRef(double easting, double northing) : base(Ireland1965Datum.Instance)
        {
            Easting = easting;
            Northing = northing;
        }

        /// <summary>
        ///     Take a string formatted as a six-figure OS grid reference (e.g. "TG514131")
        ///     and create a new OSRef object that represents that grid reference. The
        ///     first character must be H, N, S, O or T. The second character can be any
        ///     uppercase character from A through Z excluding I.
        /// </summary>
        /// <param name="gridRef">
        ///     A string representing a six-figure Ordnance Survey grid reference
        ///     in the form XY123456
        /// </param>
        public IrishRef(string gridRef) : base(Ireland1965Datum.Instance)
        {
            // if (ref.matches(""))
            var ch = gridRef[0];
            // Thanks to Nick Holloway for pointing out the radix bug here
            var east = int.Parse(gridRef.Substring(1, 4))*100;
            var north = int.Parse(gridRef.Substring(4, 7))*100;
            if (ch > 73)
                ch--; // Adjust for no I
            double nx = (ch - 65)%5*100000;
            var ny = (4 - Math.Floor((double) (ch - 65)/5))*100000;

            Easting = east + nx;
            Northing = north + ny;
        }

        /// <summary>
        ///     Gets or sets the easting in metres relative to the origin of the Irish Grid.
        /// </summary>
        /// <value>The easting.</value>
        public double Easting
        {
            get { return easting; }
            set
            {
                if (value < 0.0 || value >= 400000.0)
                {
                    throw new ArgumentException("Easting (" + value
                                                + ") is invalid. Must be greather than or equal to 0.0 and "
                                                + "less than 400000.0.");
                }

                easting = value;
            }
        }

        /// <summary>
        ///     Gets or sets the northing in metres relative to the origin of the Irish
        ///     Grid.
        /// </summary>
        /// <value>The northing.</value>
        public double Northing
        {
            get { return northing; }
            set
            {
                if (value < 0.0 || value > 500000.0)
                {
                    throw new ArgumentException("Northing (" + value
                                                + ") is invalid. Must be greather than or equal to 0.0 and less "
                                                + "than or equal to 500000.0.");
                }

                northing = value;
            }
        }

        /// <summary>
        ///     Return a String representation of this Irish grid reference showing the
        ///     easting and northing in metres.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that represents the current Irish grid reference.
        /// </returns>
        public override string ToString()
        {
            return "(" + easting + ", " + northing + ")";
        }

        /// <summary>
        ///     Return a String representation of this Irish grid reference using the
        ///     six-figure notation in the form X123456
        /// </summary>
        /// <returns>
        ///     A string representing this Irish grid reference in six-figure
        ///     notation
        /// </returns>
        public string ToSixFigureString()
        {
            var hundredkmE = (int) Math.Floor(easting/100000);
            var hundredkmN = (int) Math.Floor(northing/100000);

            var charOffset = 4 - hundredkmN;
            var index = 65 + 5*charOffset + hundredkmE;
            if (index >= 73)
                index++;
            var letter = ((char) index).ToString();

            var e = (int) Math.Floor((easting - 100000*hundredkmE)/100);
            var n = (int) Math.Floor((northing - 100000*hundredkmN)/100);
            var es = "" + e;
            if (e < 100)
                es = "0" + es;
            if (e < 10)
                es = "0" + es;
            var ns = "" + n;
            if (n < 100)
                ns = "0" + ns;
            if (n < 10)
                ns = "0" + ns;

            return letter + es + ns;
        }

        /// <summary>
        ///     Convert this Irish grid reference to a latitude/longitude pair using the
        ///     Ireland 1965 datum. Note that, the LatLng object may need to be converted to the
        ///     WGS84 datum depending on the application.
        /// </summary>
        /// <returns>
        ///     A LatLng object representing this Irish grid reference using the
        ///     Ireland 1965 datum
        /// </returns>
        public override LatLng ToLatLng()
        {
            var N0 = FALSE_ORIGIN_NORTHING;
            var E0 = FALSE_ORIGIN_EASTING;
            var phi0 = Util.ToRadians(FALSE_ORIGIN_LATITUDE);
            var lambda0 = Util.ToRadians(FALSE_ORIGIN_LONGITUDE);
            var a = Datum.ReferenceEllipsoid.SemiMajorAxis;
            var b = Datum.ReferenceEllipsoid.SemiMinorAxis;
            var eSquared = Datum.ReferenceEllipsoid.EccentricitySquared;
            var phi = 0.0;
            var lambda = 0.0;
            var E = easting;
            var N = northing;
            var n = (a - b)/(a + b);
            var M = 0.0;
            var phiPrime = (N - N0)/(a*SCALE_FACTOR) + phi0;
            do
            {
                M = b*SCALE_FACTOR
                    *((1 + n + 5.0/4.0*n*n + 5.0/4.0*n*n*n)*(phiPrime - phi0)
                      - (3*n + 3*n*n + 21.0/8.0*n*n*n)
                      *Math.Sin(phiPrime - phi0)*Math.Cos(phiPrime + phi0)
                      + (15.0/8.0*n*n + 15.0/8.0*n*n*n)
                      *Math.Sin(2.0*(phiPrime - phi0))*Math
                          .Cos(2.0*(phiPrime + phi0)) - 35.0/24.0*n*n*n
                      *Math.Sin(3.0*(phiPrime - phi0))*Math
                          .Cos(3.0*(phiPrime + phi0)));
                phiPrime += (N - N0 - M)/(a*SCALE_FACTOR);
            } while (N - N0 - M >= 0.001);
            var v = a*SCALE_FACTOR
                    *Math.Pow(1.0 - eSquared*Util.sinSquared(phiPrime), -0.5);
            var rho = a*SCALE_FACTOR*(1.0 - eSquared)
                      *Math.Pow(1.0 - eSquared*Util.sinSquared(phiPrime), -1.5);
            var etaSquared = v/rho - 1.0;
            var VII = Math.Tan(phiPrime)/(2*rho*v);
            var VIII = Math.Tan(phiPrime)/(24.0*rho*Math.Pow(v, 3.0))
                       *(5.0 + 3.0*Util.tanSquared(phiPrime) + etaSquared - 9.0*Util
                           .tanSquared(phiPrime)*etaSquared);
            var IX = Math.Tan(phiPrime)/(720.0*rho*Math.Pow(v, 5.0))
                     *(61.0 + 90.0*Util.tanSquared(phiPrime) + 45.0*Util
                         .tanSquared(phiPrime)*Util.tanSquared(phiPrime));
            var X = Util.sec(phiPrime)/v;
            var XI = Util.sec(phiPrime)/(6.0*v*v*v)
                     *(v/rho + 2*Util.tanSquared(phiPrime));
            var XII = Util.sec(phiPrime)/(120.0*Math.Pow(v, 5.0))
                      *(5.0 + 28.0*Util.tanSquared(phiPrime) + 24.0*Util
                          .tanSquared(phiPrime)*Util.tanSquared(phiPrime));
            var XIIA = Util.sec(phiPrime)/(5040.0*Math.Pow(v, 7.0))
                       *(61.0 + 662.0*Util.tanSquared(phiPrime)
                         + 1320.0*Util.tanSquared(phiPrime)*Util.tanSquared(phiPrime) + 720.0
                         *Util.tanSquared(phiPrime)*Util.tanSquared(phiPrime)*Util
                             .tanSquared(phiPrime));
            phi = phiPrime - VII*Math.Pow(E - E0, 2.0)
                  + VIII*Math.Pow(E - E0, 4.0) - IX*Math.Pow(E - E0, 6.0);
            lambda = lambda0 + X*(E - E0) - XI*Math.Pow(E - E0, 3.0)
                     + XII*Math.Pow(E - E0, 5.0) - XIIA*Math.Pow(E - E0, 7.0);

            return new LatLng(Util.ToDegrees(phi), Util.ToDegrees(lambda), 0, Datum);
        }
    }
}