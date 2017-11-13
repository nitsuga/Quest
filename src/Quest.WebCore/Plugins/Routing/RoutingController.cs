using GeoAPI.Geometries;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quest.Common.Messages.Gazetteer;
using Quest.Common.Messages.Gazetteer.Gazetteer;
using Quest.Common.Messages.Resource;
using Quest.Common.Messages.Routing;
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;
using Quest.WebCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quest.WebCore.Plugins.Routing
{
    public class RoutingController : Controller
    {
        private AsyncMessageCache _messageCache;
        private readonly IPluginService _pluginService;
        RoutingPlugin _plugin;

        public RoutingController(
                RoutingPlugin plugin,
                AsyncMessageCache messageCache,
                IPluginService pluginFactory
            )
        {
            _plugin = plugin;
        }

        [HttpGet]
        public RoutingSettings GetSettings()
        {
            return _plugin.GetSettings();
        }

        [HttpGet]
        public async Task<IndexGroupResponse> GetIndexGroups()
        {
            IndexGroupRequest request = new IndexGroupRequest();
            var result = await _messageCache.SendAndWaitAsync<IndexGroupResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        private int GetInt(string text)
        {
            int i;
            int.TryParse(text, out i);
            return i;
        }

        private double GetDouble(string text)
        {
            double i;
            double.TryParse(text, out i);
            return i;
        }

        public async Task<RoutingResponse> Route(RouteRequest request)
        {
            var result = await _messageCache.SendAndWaitAsync<RoutingResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        public async Task<RoutingResponse> Route(string from, string to, string roadSpeedCalculator, string vehicle, int hour, string username)
        {
            if (roadSpeedCalculator == "")
                roadSpeedCalculator = "VariableSpeedCalculator";

            var f = await SimpleSearch(from, username);
            if (f == null || f.Documents.Count == 0)
            {
                throw new System.ApplicationException("Cant find the start location");
            }

            var t = await SimpleSearch(to, username);
            if (t.Documents.Count == 0)
            {
                throw new System.ApplicationException("Cant find the end location");
            }

            var fc = LatLongConverter.WGS84ToOSRef(f.Documents[0].l.Location.Latitude, f.Documents[0].l.Location.Longitude);
            var tc = LatLongConverter.WGS84ToOSRef(t.Documents[0].l.Location.Latitude, t.Documents[0].l.Location.Longitude);

            RouteRequest request = new RouteRequest()
            {
                FromLocation = new Coordinate(fc.Easting, fc.Northing),
                ToLocations = new Coordinate[] { new Coordinate(tc.Easting, tc.Northing) },
                DistanceMax = int.MaxValue,
                DurationMax = int.MaxValue,
                HourOfWeek = hour,
                RoadSpeedCalculator = roadSpeedCalculator,
                SearchType = RouteSearchType.Shortest,
                VehicleType = vehicle
            };

            var route = await Route(request);
            return route;
        }

        public async Task<GetCoverageResponse> GetVehicleCoverage(GetCoverageRequest request)
        {
            var result = await _messageCache.SendAndWaitAsync<GetCoverageResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        public RoutingResponse RouteCompare(int id, string roadSpeedCalculator, string username)
        {
            if (roadSpeedCalculator == "")
                roadSpeedCalculator = "VariableSpeedCalculator";

            // look up the route id and do a route
            //Route(from, to, roadSpeedCalculator, vehicle, hour, username);

            return new RoutingResponse();
        }

        public async Task<SearchResponse> SimpleSearch(string location, string userName)
        {
            var gazrequest = new SearchRequest()
            {
                includeAggregates = false,
                searchMode = SearchMode.RELAXED,
                take = 1,
                skip = 0,
                searchText = location,
                box = null,
                filters = null,
                displayGroup = SearchResultDisplayGroup.none,
                username = User.Identity.Name,
                indexGroup = null
            };

            var searchResult = await _messageCache.SendAndWaitAsync<SearchResponse>(gazrequest, new TimeSpan(0, 0, 10));

            return searchResult;

        }
    }
}