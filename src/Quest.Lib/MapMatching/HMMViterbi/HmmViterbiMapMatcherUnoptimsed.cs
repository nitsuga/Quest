#pragma warning disable 0169,649
#define DUMP_GRAPH_X

using System;
using System.Collections.Generic;
using Quest.Common.Messages;
using Quest.Lib.MapMatching.RouteMatcher;
using Quest.Lib.Routing;
using Quest.Common.Messages.Routing;

namespace Quest.Lib.MapMatching.HMMViterbi
{
    public class HmmViterbiMapMatcherUnOp : IMapMatcher
    {
        private RoutingData _data;

        /// <summary>
        ///     analyse a set of fixes and determine the route using Hmm with Viterbi
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public RouteMatcherResponse AnalyseTrack(RouteMatcherRequest request)
        {
            try
            {
                var parameters = request.GetParameters();

                var steps = parameters.GenerateCandidates();

                if (steps.Length > 0)
                {
                    CalculateRoutes(steps, parameters);

                    var path = steps.GetViterbiPath(parameters, removeUnroutables: false, onlyKeepBestFromPrevious: false);
                    var result = HmmUtil.BuildResponse(steps, path, parameters, request.Name);
                    result.IsSuccess = true;
                    result.Message = "Ok";

                    return result;
                }

                return new RouteMatcherResponse { IsSuccess = false, Message = "No fixes" };
            }
            catch (Exception ex)
            {
                return new RouteMatcherResponse { IsSuccess = false, Message = ex.Message };
            }
        }
        
        /// <summary>
        /// calculate a road route between each candidate at each step t and each candidate in step t+1
        /// Do this for all steps. Each route also includes a calculated transition function.
        /// </summary>
        /// <param name="steps"></param>
        /// <param name="parameters"></param>
        private static void CalculateRoutes(IReadOnlyList<Step> steps, HmmParameters parameters)
        {
            var stepCount = steps.Count;

            // build up a list of MaxCandidates routes from the start to the end
            for (var i = 0; i < stepCount - 1; i++)
            {
                var step = steps[i];
                var nextstep = steps[i + 1];
                step.CandidateFixes.ForEach(
                    c => c.RoutesToNextFix = c.CalculateCandidateRoutes(step, nextstep, parameters, parameters.VehicleType)
                    );
            }
        }
    }
}