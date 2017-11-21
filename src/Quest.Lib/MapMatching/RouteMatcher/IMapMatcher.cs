using Quest.Common.Messages.Routing;

namespace Quest.Lib.MapMatching.RouteMatcher
{
    public interface IMapMatcher
    {
        RouteMatcherResponse AnalyseTrack(RouteMatcherRequest request);
    }
}
