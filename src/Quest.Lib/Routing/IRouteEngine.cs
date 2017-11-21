using Quest.Common.Messages.Routing;

namespace Quest.Lib.Routing
{

    public interface IRouteEngine
    {
        RoutingData Data { get; }

        bool IsReady { get; }

        CoverageMap CalculateCoverage(RouteRequestCoverage request);

        RoutingResponse CalculateRouteMultiple(RouteRequestMultiple request);
        RoutingResponse CalculateQuickestRoute(RouteRequest request);
    }
}