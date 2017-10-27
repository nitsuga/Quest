using GeoAPI.Geometries;
using Quest.Common.Messages;
using Quest.Common.Messages.Routing;
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;
using System;
using System.Threading.Tasks;

namespace Quest.WebCore.Services
{
    public class RouteService 
    {
        SearchService _searchService;
        AsyncMessageCache _msgClientCache;

        public RouteService(SearchService searchService, AsyncMessageCache msgClientCache)
        {
            _msgClientCache = msgClientCache;
            _searchService = searchService;
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
            var result = await _msgClientCache.SendAndWaitAsync<RoutingResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        public async Task<RoutingResponse> Route(string from, string to, string roadSpeedCalculator, string vehicle, int hour, string username)
        {
            if (roadSpeedCalculator == "")
                roadSpeedCalculator = "VariableSpeedCalculator";

            var f = await _searchService.SimpleSearch(from, username);
            if (f == null || f.Documents.Count == 0)
            {
                throw new ApplicationException("Cant find the start location");
            }

            var t = await _searchService.SimpleSearch(to, username);
            if (t.Documents.Count == 0)
            {
                throw new ApplicationException("Cant find the end location");
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
            var result = await _msgClientCache.SendAndWaitAsync<GetCoverageResponse>(request, new TimeSpan(0, 0, 10));
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
    }
}