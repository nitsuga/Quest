using System;
using GeoAPI.Geometries;

namespace Quest.Common.Messages
{

    [Serializable]
    public class RouteRequestCoverage : Request, IRouteRequest
    {

        public int TileSize = 250;

        public double DistanceMax;

        public double DurationMax;

        public int HourOfWeek;

        public string Name;

        public string RoadSpeedCalculator;

        public RouteSearchType SearchType;

        public Coordinate[] StartPoints;

        public string VehicleType;

        public int epsg=27700;
    }
}