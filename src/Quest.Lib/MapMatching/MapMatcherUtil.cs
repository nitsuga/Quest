using System;
using System.Diagnostics;
using System.Threading;
using Quest.Lib.MapMatching.RouteMatcher;
using Quest.Lib.Routing;
using Quest.Lib.Trace;
using Autofac;
using Quest.Common.Messages.GIS;

namespace Quest.Lib.MapMatching
{
    public class MapMatcherUtil
    {

        /// <summary>
        /// Analyse a single track using a specified map matcher and paramers
        /// </summary>
        /// <param name="container"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static MapMatcherMatchSingleResponse MapMatcherMatchSingle(ILifetimeScope scope, MapMatcherMatchSingleRequest request)
        {
            try
            {

                if (request == null)
                    return new MapMatcherMatchSingleResponse { Success = false, Message = "Invalid payload" };

                var routingDataExport = scope.Resolve<RoutingData>();
                if (routingDataExport==null)
                    return new MapMatcherMatchSingleResponse { Success = false, Message = "Routing data is not loaded" };

                // wait for routing data to be loaded
                var routingData = routingDataExport;
                while (routingData.IsInitialised == false)
                {
                    Thread.Sleep(1000);
                }

                var selectedRouteEngine = request.RoutingEngine!=null? scope.ResolveNamed<IRouteEngine>(request.RoutingEngine):null;
                var matcher = scope.ResolveNamed<IMapMatcher>(request.MapMatcher);

                if (request.Fixes.Count < 2)
                    return new MapMatcherMatchSingleResponse{ Success = false, Message = "Not enough fixes" };

                var analyseRequest = new RouteMatcherRequest
                {
                    Name = request.Name,
                    RoadSpeedCalculator = "ConstantSpeedCalculator",
                    RoutingData = routingData,
                    Fixes = request.Fixes,
                    RoutingEngine = selectedRouteEngine,
                    Parameters = request.Parameters
                };

                var response = matcher.AnalyseTrack(analyseRequest);
                //Debug.Print(response.GraphVis);
                return new MapMatcherMatchSingleResponse { Result = response,  Success = response.IsSuccess , Message = response.Message  };
                    
            }
            catch (Exception ex)
            {
                var msg = $"MapMatcherMatchSingle Error: {ex}";
                Logger.Write(msg, TraceEventType.Verbose, "Map Matcher");
                return new MapMatcherMatchSingleResponse { Success = false, Message = ex.Message };
            }
        }


    }
}