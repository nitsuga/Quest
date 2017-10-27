using System;
using System.Collections.Generic;
using Quest.Common.Messages;
using Quest.Common.Messages.Routing;
using Quest.Common.Messages.GIS;

namespace Quest.Lib.MapMatching.HMMViterbi
{
    internal class CandidateFix : Fix
    {
        /// <summary>
        /// Viterbi determines that this is the best fix
        /// </summary>
        internal bool IsBest;

        /// <summary>
        /// The road link that has been snapped to.
        /// </summary>
        internal EdgeWithOffset EdgeWithOffset;
        
        /// <summary>
        /// used by viterbi 
        /// </summary>
        internal SampleRoute BestPreviousRoute;

        /// <summary>
        /// distance from fix
        /// </summary>
        internal double Distance;

        /// <summary>
        ///     how likely this roadlink/offset is according to the GPS fix
        /// </summary>
        internal double EmissionProbability;

        /// <summary>
        /// calculated routes to each candidate in the next step
        /// </summary>
        internal List<SampleRoute> RoutesToNextFix;

        /// <summary>
        /// The viterbi value .. a probability for this path
        /// </summary>
        public double Viterbi;

        /// <summary>
        /// has this candidate been successfully routed to by previous candidates? if not, its no good
        /// </summary>
        public bool HasRoutesToHere;

        /// <summary>
        /// degrees difference between this road segment and the GPS fix
        /// </summary>
        public double DegreeOffset;

        public override string ToString()
        {
            var flag = BestPreviousRoute != null ? "*" : "";
            return $"#{Sequence}/{Id} id:{EdgeWithOffset.Edge.RoadLinkEdgeId}/{Math.Round(EdgeWithOffset.Offset,1)} {flag}";
        }
    }
}