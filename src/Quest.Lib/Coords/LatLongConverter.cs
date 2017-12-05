using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using Quest.Lib.Coords;
using Quest.Common.Messages.GIS;

namespace Quest.Lib.Coords
{
    public static class LatLongConverter
    {
        public static double DegToRadians(double v)
        {
            return 2*Math.PI*v/360.0;
        }

        //public static double Distance(this LatLng p1, LatLng p2)
        //{
        //    return
        //        Math.Acos(Math.Sin(DegToRadians(p1.Latitude))*Math.Sin(DegToRadians(p2.Latitude)) +
        //                  Math.Cos(DegToRadians(p1.Latitude))*Math.Cos(DegToRadians(p2.Latitude))*
        //                  Math.Cos(DegToRadians(p2.Longitude - p1.Longitude)))*6371000.0;
        //}

        public static double ToRadians(this double angle)
        {
            return angle * Math.PI / 180;
        }

        public static LatLng OSRefToWGS84(double x, double y)
        {
            try
            {
                return OSRefToWGS84(new OSRef(x, y));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OSRefToWGS84 error " + ex.Message);
                return null;
            }
        }

        public static LatLng OSRefToWGS84(this OSRef position)
        {
            var ll = position.ToLatLng();
            ll.ToWGS84();
            return new LatLng(ll.Latitude, ll.Longitude);
        }

        public static LatLng OSRefToWGS84(this Coordinate position)
        {
            var r = new OSRef(position.X, position.Y);
            return OSRefToWGS84(r);
        }

        public static OSRef WGS84ToOSRef(double latitude, double longitude)
        {
            try
            {
                return WGS84ToOSRef(new LatLng(latitude, longitude));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"failed to obtain os ref from lat long with exception details: {ex.Message}");
                //We have some invalid data return nothing
                return null;
            }
        }

        public static OSRef WGS84ToOSRef(this LatLng position)
        {
            var copy = new LL(position.Latitude, position.Longitude);
            copy.ToOSGB36();
            return copy.ToOSRef();
        }
    }
}