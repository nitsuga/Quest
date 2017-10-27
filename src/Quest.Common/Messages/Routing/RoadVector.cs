namespace Quest.Common.Messages.Routing
{
    public struct RoadVector
    {
        /// <summary>
        /// distance along the edge
        /// </summary>
        public double DistanceMeters;

        /// <summary>
        /// time to traverse the edge
        /// </summary>
        public double DurationSecs;

        /// <summary>
        /// speed along the edge
        /// </summary>
        public double SpeedMs;
    }
}