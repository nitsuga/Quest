using GeoAPI.Geometries;
using System;

namespace Quest.Common.Messages.Routing
{
    /// <summary>
    /// Class used to represent a position on a road link.
    /// </summary>
    [Serializable]
    public class EdgeWithOffset
    {
        /// <summary>
        ///  the angle traveling along the roadlink
        /// </summary>
        public double AngleRadians;

        /// <summary>
        /// the road link
        /// </summary>
        public RoadEdge Edge;

        /// <summary>
        /// coordinate alomg the road link
        /// </summary>
        public Coordinate Coord;

        /// <summary>
        /// offset from the start of the roadlink
        /// </summary>
        public double Offset;

    }
}