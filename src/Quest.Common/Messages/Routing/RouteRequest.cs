using System;
using GeoAPI.Geometries;

namespace Quest.Common.Messages.Routing
{
    [Serializable]    
    public class RouteRequest : Request, IRouteRequest
    {
        /// <summary>
        /// Maximum distance in meters
        /// </summary>
        public double DistanceMax = int.MaxValue;

        /// <summary>
        /// maximum duration in seconds
        /// </summary>
        public double DurationMax = int.MaxValue;

        /// <summary>
        /// Coordinate of start location
        /// </summary>
        public Coordinate FromLocation;

        /// <summary>
        /// Coordinates of end locations
        /// </summary>
        public Coordinate[] ToLocations;

        /// <summary>
        /// hour of travel
        /// </summary>
        public int HourOfWeek;

        /// <summary>
        /// speed provider to use
        /// </summary>
        public string RoadSpeedCalculator;

        /// <summary>
        /// search type, quickest, shortest
        /// </summary>
        public RouteSearchType SearchType;

        /// <summary>
        /// type of vehicle - AEU or FRU
        /// </summary>
        public string VehicleType;

        /// <summary>
        /// routing engine to use
        /// </summary>
        public string RoutingEngine;
    }
}