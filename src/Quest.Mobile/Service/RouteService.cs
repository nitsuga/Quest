using Microsoft.VisualBasic.FileIO;
using Quest.Common.Messages;
using System;
using System.ComponentModel.Composition;
using System.IO;
using GeoAPI.Geometries;
using Quest.Lib.Utils;
using Quest.Mobile.Job;
using Quest.Mobile.Models;

namespace Quest.Mobile.Service
{
    public class RouteService : JobService<RouteJob, RouteInfo>
    {
        SearchService _searchService;

        public RouteService(SearchService searchService)
        {
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

        private RoutingResponse Route(RouteRequest request)
        {
            var routingQueue = "RoutingManager_0_0";
            var result = MvcApplication.MsgClientCache.SendAndWait<RoutingResponse>(request, new TimeSpan(0, 0, 10), routingQueue);
            return result;
        }

        public RoutingResponse Route(string from, string to, string roadSpeedCalculator, string vehicle, int hour, string username)
        {
            if (roadSpeedCalculator == "")
                roadSpeedCalculator = "VariableSpeedCalculator";

            var f = _searchService.SimpleSearch(from, username);
            if (f == null || f.Documents.Count == 0)
            {
                throw new ApplicationException("Cant find the start location");
            }

            var t = _searchService.SimpleSearch(to, username);
            if (t.Documents.Count == 0)
            {
                throw new ApplicationException("Cant find the end location");
            }

            var fc = LatLongConverter.WGS84ToOSRef(f.Documents[0].l.Location.Latitude, f.Documents[0].l.Location.Longitude);
            var tc = LatLongConverter.WGS84ToOSRef(t.Documents[0].l.Location.Latitude, t.Documents[0].l.Location.Longitude);

            RouteRequest request = new RouteRequest()
            {
                FromLocation = new Coordinate(fc.Easting, fc.Northing),
                ToLocation = new Coordinate(tc.Easting, tc.Northing),
                DistanceMax = int.MaxValue,
                DurationMax = int.MaxValue,
                HourOfWeek = hour,
                RoadSpeedCalculator = roadSpeedCalculator,
                SearchType = RouteSearchType.Shortest,
                VehicleType = vehicle
            };

            var route = Route(request);
            return route;
        }

        public GetCoverageResponse GetVehicleCoverage(GetCoverageRequest request)
        {
            var result = MvcApplication.MsgClientCache.SendAndWait<GetCoverageResponse>(request, new TimeSpan(0, 0, 10));
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

        /// <summary>
        /// process a batch of searches
        /// </summary>
        /// <returns>a job id</returns>
        public RouteJob BatchRoutes(string routes, string roadSpeedCalculator)
        {
            // create new job record and add to dictionary
            var newjob = new RouteJob();

            using (var reader = new StringReader(routes))
            {
                using (var parser = new TextFieldParser(reader))
                {
                    parser.SetDelimiters(new string[] { "," });
                    while (!parser.EndOfData)
                    {
                        try
                        {
                            var data = parser.ReadFields();
                            var ri = new RouteInfo { RoadSpeedCalculator = roadSpeedCalculator };
                            if (data != null && data.Length >= 7)
                                ri.ActualTimeSecs = GetDouble(data[6]);

                            if (data != null && data.Length >= 6)
                            {
                                ri.FromX = GetInt(data[0]);
                                ri.FromY = GetInt(data[1]);
                                ri.ToX = GetInt(data[2]);
                                ri.ToY = GetInt(data[3]);
                                ri.Hour = GetInt(data[5]);
                                ri.Vehicle = data[4];
                                ri.Request = new RouteRequest()
                                {
                                    RoadSpeedCalculator = roadSpeedCalculator,
                                    DistanceMax = int.MaxValue,
                                    DurationMax = int.MaxValue,
                                    FromLocation = new Coordinate(ri.FromX, ri.FromY),
                                    HourOfWeek = ri.Hour,
                                    SearchType = RouteSearchType.Quickest,
                                    ToLocation = new Coordinate(ri.ToX, ri.ToY),
                                    VehicleType = ri.Vehicle
                                };
                                newjob.items.Add(ri);
                            }

                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }

            AddJob(newjob);

            newjob.Run((w) =>
            {
                // this gets executed for each work item
                w.Result = MvcApplication.MsgClientCache.SendAndWait<RoutingResponse>(w.Request, new TimeSpan(0, 0, 10));
                if (w.Result?.Items.Count > 0)
                    w.EstTimeSecs = w.Result.Items[0].Duration;
            });

            return newjob;
        }
    }
}