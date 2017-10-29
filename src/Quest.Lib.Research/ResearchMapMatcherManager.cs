using System;
using System.Collections.Generic;
using Quest.Lib.MapMatching;
using Quest.Lib.Research.Utils;
using Quest.Lib.Utils;
using Quest.Common.Messages;
using Autofac;
using Quest.Lib.DependencyInjection;
using Quest.Common.Messages.GIS;
using Quest.Common.Messages.Visual;

namespace Quest.Lib.Research
{
    [Injection]
    public class ResearchMapMatcherManager
    {
        private TrackLoader _trackLoader;

        public ResearchMapMatcherManager(
            ILifetimeScope scope,
            TrackLoader trackLoader
            )
        {
            _trackLoader = trackLoader;
        }

        public MapMatcherMatchSingleResponse QueryVisual(ILifetimeScope scope, QueryVisualRequest request)
        {
            try
            {
                dynamic parms = ExpandoUtils.MakeExpandoFromString(request.Query);
                var expandoParms = (IDictionary<string, object>) parms;

                Track track = _trackLoader.GetTrack((string)parms.Track, (int)parms.Skip);

                bool isGood = track.CleanTrack((int)parms.MinSeconds, (int)parms.MinDistance, (int)parms.MaxSpeed, (int)parms.Take);

                if (!isGood)
                    return new MapMatcherMatchSingleResponse {
                        Message = track.ErrorMessage,
                        Success = false,
                    };

                MapMatcherMatchSingleRequest mmrequest = new MapMatcherMatchSingleRequest()
                {
                    RoutingEngine = expandoParms.ContainsKey("RoutingEngine") ? parms.RoutingEngine : null,
                    MapMatcher = parms.MapMatcher,
                    RoutingData = parms.RoutingData,
                    Fixes = track.Fixes,
                    Parameters = parms,
                    Name = (string)parms.Track
                };

                var response = MapMatcherUtil.MapMatcherMatchSingle(scope, mmrequest);

                return response;
            }
            catch (Exception ex)
            {
                return new MapMatcherMatchSingleResponse { Message = ex.Message, Success = false };
            }
        }
    }
}