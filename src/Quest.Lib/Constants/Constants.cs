using System;

namespace Quest.Lib.Constants
{
    public static class Constant
    {
        // radius of the earth
        public const double EarthRadius = 6378137;
        public const double deg2rad = 2*Math.PI/360.0;
        public const double rad2deg = 1/deg2rad;
        public const double mph2ms = 0.44704;
        public const double ms2mph = 1/0.44704;
    }
}