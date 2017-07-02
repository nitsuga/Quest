using Quest.Common.Messages;

namespace Quest.Lib.MapMatching.RouteMatcher
{
    public interface IMapMatcher
    {
        RouteMatcherResponse AnalyseTrack(RouteMatcherRequest request);
    }
}
