using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using GeoAPI.Geometries;
using Quest.Common.Messages;
using Quest.Lib.DataModel;
using Quest.Lib.Trace;
using Quest.Lib.Utils;
using Quest.Lib.DependencyInjection;
using Quest.Lib.Data;

namespace Quest.Lib.Routing
{
    [Injection]
    public class EtaCalculator
    {
        private IDatabaseFactory _dbFactory;

        public EtaCalculator(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        /// <summary>
        ///     calculate the time enroute for enroute vehicles
        /// </summary>
        public void CalculateEnrouteTime(IRouteEngine routingEngine, RoutingData routingdata, string speedCalc)
        {
            try
            {
                // get list of resources enroute
                var resources = GetEnrouteResources();

                var results = new EtaResults {TimeNow = DateTime.Now, Results = new List<EtaResult>()};


                // calculate their ETA
                if (resources != null)
                    foreach (var r in resources)
                    {
                        if (r.Latitude != null && r.DestLatitude != null)
                        {
                            var fc = LatLongConverter.WGS84ToOSRef(r.Latitude ?? 0, r.Longitude ?? 0);
                            var tc = LatLongConverter.WGS84ToOSRef(r.DestLatitude ?? 0, r.DestLongitude ?? 0);

                            var result = new EtaResult
                            {
                                Callsign = r.Callsign.Callsign1,
                                Eta = UpdateResourceEta(
                                    routingEngine,
                                    routingdata,
                                    r.ResourceId,
                                    r.ResourceType.ResourceType1,
                                    r.Eta,
                                    fc.Easting, fc.Northing,
                                    tc.Easting, tc.Northing,
                                    speedCalc)
                            };

                            // good result?
                            if (result.Eta != DateTime.MinValue)
                                results.Results.Add(result);
                        }
                    }

                // send to Rabbit
                //if (msgSource != null)
                //    msgSource.BroadcastMessage(results);

                Logger.Write(
                    $"Updated Resource ETA's: {string.Join(",", (from s in results.Results select s.Callsign).ToArray())} ",
                    TraceEventType.Information, "ETA Processor (enroute)");
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

        /// <summary>
        ///     get enroute vehicles for ETA calcs.
        /// </summary>
        /// <returns></returns>
        private DataModel.Resource[] GetEnrouteResources()
        {
            return _dbFactory.ExecuteNoTracking<QuestContext, DataModel.Resource[]>((db) =>
            {
                var vehEnroute = db.Resource.Where(r => r.EndDate == null && r.ResourceStatus.BusyEnroute==true).ToArray();
                return vehEnroute;
            });
        }

        /// <summary>
        ///     calculate and update the resources' ETA
        /// </summary>
        /// <param name="routingEngine"></param>
        /// <param name="resEta"></param>
        /// <param name="resourceId"></param>
        /// <param name="vehicleType"></param>
        private DateTime UpdateResourceEta(IRouteEngine routingEngine, 
            RoutingData routingdata, int resourceId, string vehicleType,
            DateTime? resEta, double positionX, double positionY, 
            double destinationX, double destinationY, string speedCalc)
        {
            var eta = DateTime.MinValue;

            var startPoint = routingdata.GetEdgeFromPoint(new Coordinate(positionX , positionY ));
            var endPoints = new List<EdgeWithOffset> { routingdata.GetEdgeFromPoint(new Coordinate(destinationX, destinationY)) };

            var request = new RouteRequestMultiple
            {
                StartLocation = startPoint,
                EndLocations = endPoints,
                InstanceMax = 1,
                VehicleType = vehicleType,
                HourOfWeek = DateTime.Now.HourOfWeek(),
                DistanceMax = 18000,
                DurationMax = 18000,
                SearchType = RouteSearchType.Quickest,
                RoadSpeedCalculator = speedCalc
            };

            // calculate the route to the incident
            var result = routingEngine.CalculateRouteMultiple(request);

            if (result.Items.Count >= 1)
            {
                var roadName = "";
                // update the time.
                eta = DateTime.Now + new TimeSpan(0, 0, (int)result.Items[0].Duration);

                if (result.Items[0].Connections.Count > 0)
                    roadName = result.Items[0].Connections[0].Edge.RoadName ?? "";


                // update the eta if it has changed more than 30 seconds
                var doUpdate = resEta.HasValue == false ||
                               (Math.Abs(eta.Subtract((DateTime)resEta).TotalSeconds) > 30);

                if (!doUpdate)
                {
                    eta = DateTime.MinValue;
                }
                else
                {
                    // update the resource
                    _dbFactory.Execute<QuestContext>((db) =>
                    {
                        try
                        {
                            var res = db.Resource.First(x => x.ResourceId == resourceId);
                            res.Eta = eta;
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            Logger.Write(ex);
                        }
                    });
                }
            }

            return eta;
        }

    }
}