using System;
using System.Collections.Generic;
using Quest.Common.Messages;
using Quest.Lib.MapMatching.RouteMatcher;
using Quest.Lib.Maths;
using Quest.Lib.Routing;
using Quest.Common.Messages.GIS;

namespace Quest.Lib.MapMatching.HMMViterbi
{
    [Serializable]
    public class HmmParameters : IMapMatchParameters
    {
        /// <summary>
        /// Speed calculator to use when routing
        /// </summary>
        public string RoadSpeedCalculator;

        /// <summary>
        /// Road network data
        /// </summary>
        public RoutingData HmmRoutingData;

        /// <summary>
        /// routing engine to use (if any)
        /// </summary>
        public IRouteEngine HmmRoutingEngine;

        /// <summary>
        /// Vehicle type to assume when routing
        /// </summary>
        public string VehicleType;

        /// <summary>
        ///  The observations to create a route from
        /// </summary>
        public List<Fix> Fixes;

        /// <summary>
        /// probability function for emission, gets converted to EmissionEnum
        /// </summary>
        public string Emission;

        /// <summary>
        /// probability enum function for emission
        /// </summary>
        public Distributions.DistType EmissionEnum;

        /// <summary>
        /// probability parameters (1)
        /// </summary>
        public double EmissionP1;

        /// <summary>
        /// probability parameters (2)
        /// </summary>
        public double EmissionP2;

        /// <summary>
        /// probability function for transition, gets converted to TransitionEnum
        /// </summary>
        public string Transition;

        /// <summary>
        /// probability enum function for transition
        /// </summary>
        public Distributions.DistType TransitionEnum;

        /// <summary>
        /// probability parameters (1)
        /// </summary>
        public double TransitionP1;

        /// <summary>
        /// probability parameters (2)
        /// </summary>
        public double TransitionP2;

        /// <summary>
        /// maximum number of routes to consider at each step
        /// </summary>
        public int MaxCandidates;

        /// <summary>
        /// Maximum distance a roadlink can be from a fix
        /// </summary>
        public double RoadGeometryRange;

        /// <summary>
        /// The envelope to use to capture nearby road links, must be larger than RoadGeometryRange
        /// </summary>
        public int RoadEndpointEnvelope;

        /// <summary>
        /// generate a GraphVis output at the end of processing
        /// </summary>
        public bool GenerateGraphVis;

        /// <summary>
        /// do we normalise emission values at each step?
        /// </summary>
        public bool NormaliseEmission;

        /// <summary>
        /// do we normalise transition values at each step?
        /// </summary>
        public bool NormaliseTransition;

        /// <summary>
        /// do we multiply (false) or add (true) subsequent viterbi values?
        /// if true:   new viterbi = old viterbi + (emission * transition)
        /// if false:  new viterbi = old viterbi x (emission * transition)
        /// </summary>
        public bool SumProbability;

        /// <summary>
        /// candidate road links must be with DirectionTolerance degrees of the reported GPS fix direction
        /// Set to 180 or above to ignore candidate road link filtering based on direction of travel
        /// </summary>
        public double DirectionTolerance;

    }
}