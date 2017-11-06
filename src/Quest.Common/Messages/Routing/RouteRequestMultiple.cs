using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Routing
{
    public enum RouteSearchType
    {
        Shortest,
        Quickest
    }

    /// <summary>
    /// non-servicebus method for calling the routing engine
    /// </summary>
    
    [Serializable]
    public class RouteRequestMultiple
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
        /// road link endge and offset of start
        /// </summary>
         public EdgeWithOffset StartLocation;

        /// <summary>
        /// road link endge and offset of end locations
        /// </summary>
         public List<EdgeWithOffset> EndLocations;

        /// <summary>
        /// hour of travel
        /// </summary>
         public int HourOfWeek;

        /// <summary>
        /// maximum number of answers
        /// </summary>
         public int InstanceMax;

        /// <summary>
        /// coverage bitmap 
        /// </summary>
         public CoverageMap Map;

        /// <summary>
        /// speed provider to use
        /// </summary>
        public string RoadSpeedCalculator;

        /// <summary>
        /// search type, quickest, shortest
        /// </summary>
         public RouteSearchType SearchType;

        /// <summary>
        /// type of vehicle
        /// </summary>
        public string VehicleType;

    }
}