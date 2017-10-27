#pragma warning disable 0169,649
using System;
using System.Collections.Generic;
using Quest.Common.Messages;
using Quest.Lib.MapMatching.RouteMatcher;
using Quest.Common.Messages.Routing;

namespace Quest.Lib.MapMatching.ParticleFilter
{
    public class ParticleFilterMapMatcher : IMapMatcher
    {
        
        RoadParticleEngine _engine;

        public RouteMatcherResponse AnalyseTrack(RouteMatcherRequest request)
        {

            ParticleParameters parameters = GetParameters(request);

            ParticleStepRequest r = new ParticleStepRequest() { Parameters = parameters };
            
            var stepCount = request.Fixes.Count;
            var rr = new RouteMatcherResponse {
                Name = request.Name,
                IsSuccess = true,
                Message = "",
                Results = new List<RoadLinkEdgeSpeed>()
            };

            // build up a list of MaxCandidates routes from the start to the end
            for (var i = 0; i < stepCount - 1; i++)
            {
                r.PreviousFix = i > 0 ? request.Fixes[i - 1] : null;
                r.NextFix = i < stepCount-1 ? request.Fixes[i + 1] : null;
                r.ThisFix = request.Fixes[i];                
                var result = _engine.AnalyseTrack(r);
                var position  = result.EstimatedVector as RoadParticle;
                r.Particles = result.Particles;
            }

            var rmr = new RouteMatcherResponse { Name = request.Name, IsSuccess = true, Message = "" };

            return rmr;
        }

        private static ParticleParameters GetParameters(RouteMatcherRequest request)
        {
            if (request.Parameters == null)
                throw new ApplicationException("NULL passed for parameters.");

            // extract the parameters from the dynamic
            ParticleParameters parameters = ExpandoUtil.SetFromExpando<ParticleParameters>(request.Parameters);

            if (parameters == null)
                throw new ApplicationException("Incorrect parameters passed, should be of type HMMParameters");

            Enum.TryParse(parameters.Emission, out parameters.EmissionEnum);

            parameters.ParticleRoutingData = request.RoutingData;

            return parameters;
        }

    }
}
