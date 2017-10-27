using System.Collections.Generic;
using Quest.Common.Messages;
using Quest.Common.Messages.Routing;

namespace Quest.Lib.MapMatching.HMMViterbi
{
    internal class SampleRoute // : IEdge<RoutingLocation>
    {
        /// <summary>
        /// Viterbi determines that this is the best fix
        /// </summary>
        internal bool IsBest;

        internal CandidateFix DestinationFix;

        public double RouteDistance;

        public double Duration;

        public Waypoint[] PathPoints;

        internal List<RoadEdgeWithVector> Route;

        internal CandidateFix SourceFix;

        public double SpeedMs;
        public Step Step;

        internal double TransitionProbability;

        internal double TransitionValue;

        public override string ToString()
        {
            //return string.Format("route dist={1:000.#} {2}/{3} {0}", string.Join("->", Route), Distance, DestLink.Coord.X, DestLink.Coord.Y);
            //return $"route {string.Join("->", Route)}";
            return $"{SourceFix}->{DestinationFix}";
        }
    }
}