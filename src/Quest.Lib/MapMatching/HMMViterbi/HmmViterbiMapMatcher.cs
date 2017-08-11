#pragma warning disable 0169
#define DUMP_GRAPH_X

using System;
using System.Collections.Generic;
using System.Linq;
using Quest.Common.Messages;
using Quest.Lib.MapMatching.RouteMatcher;
using Quest.Lib.Routing;

namespace Quest.Lib.MapMatching.HMMViterbi
{
    public class HmmViterbiMapMatcher : IMapMatcher
    {
        
        private RoutingData _data;

        /// <summary>
        ///     analyse a set of fixes and determine the route using Hmm with Viterbi.
        ///     This routing differs from the unoptimised version because it works
        ///     iteratively through the list of fixes, only keeping the best route
        ///     so far
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public RouteMatcherResponse AnalyseTrack(RouteMatcherRequest request)
        {
            try
            {
                var parameters = request.GetParameters();

                var steps = parameters.GenerateCandidates();

                var stepCount = steps.Count();

                // mark the first set of candidates as routed to so they appear valid routes and have a viterbi of 1
                steps[0].CandidateFixes.Initialise(true, 1);

                // build up a list of MaxCandidates routes from the start to the end
                for (var i = 0; i < stepCount - 1; i++)
                {
                    // calculate routes to the next step
                    CalculateRoutes(steps, i, parameters);

                    // calculate the viterbi 
                    steps.CalculateViterbiAtStep(i+1, parameters, removeUnroutables: true, onlyKeepBestFromPrevious: true);
                }

                var path = steps.ExtractViterbiPath();

                var result = HmmUtil.BuildResponse(steps, path, parameters, request.Name);

                return result;
            }
            catch (Exception ex)
            {
                return new RouteMatcherResponse { IsSuccess = false, Message = ex.Message };
            }
        }

        /// <summary>
        /// calculate a road route between each candidate at step stepIndex and each candidate in step stepIndex+1
        /// Each route also includes a calculated transition function.
        /// </summary>
        /// <param name="steps"></param>
        /// <param name="stepIndex"></param>
        /// <param name="parameters"></param>
        private static void CalculateRoutes(IReadOnlyList<Step> steps, int stepIndex, HmmParameters parameters)
        {
            var step = steps[stepIndex];
            var nextstep = steps[stepIndex + 1];
            step.CandidateFixes.ForEach(
                c => c.RoutesToNextFix = c.CalculateCandidateRoutes(step, nextstep, parameters, parameters.VehicleType)
                );
        }
    }
}