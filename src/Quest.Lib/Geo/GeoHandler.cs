﻿#define NO_APPLE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Quest.Common.Messages;
using Quest.Lib.DataModel;
using Quest.Lib.Utils;
using Quest.Lib.Incident;
using Quest.Lib.Resource;
using Quest.Lib.Data;
using Quest.Common.ServiceBus;
using Quest.Lib.Search.Elastic;
using Quest.Common.Utils;
using Quest.Common.Messages.Device;
using Quest.Common.Messages.GIS;
using Quest.Common.Messages.CAD;
using Quest.Common.Messages.Resource;
using Quest.Common.Messages.Destination;
using Quest.Common.Messages.Incident;

namespace Quest.Lib.Geo
{
    public class GeoHandler
    {
        private ElasticSettings _elastic;
        private BuildIndexSettings _config;
        private IResourceStore _resStore;
        private IIncidentStore _incStore;
        private ResourceHandler _resHandler;
        private IDatabaseFactory _dbFactory;


    public GeoHandler(IDatabaseFactory dbFactory, ElasticSettings elastic,
            IResourceStore resStore,
            ResourceHandler resHandler,
            IIncidentStore incStore)
        {
            _resHandler = resHandler;
            _resStore = resStore;
            _incStore = incStore;
            _elastic = elastic;
            _dbFactory = dbFactory;

        }

        public MapItemsResponse GetMapItems(MapItemsRequest request)
        {
            var response = new MapItemsResponse
            {
                RequestId = request.RequestId,
                Resources = new List<QuestResource>(),
                Destinations = new List<QuestDestination>(),
                Incidents = new List<QuestIncident>(),
                Success = true,
                Message = "successful"
            };

            response.Destinations = GetDestinations(request.Hospitals, request.Stations, request.Standby);

            if (request.ResourcesAvailable || request.ResourcesBusy)
            {
                // work out which ones were on display at the original revision
                var resources = _resStore.GetResources(request.Revision, request.ResourceGroups, request.ResourcesAvailable, request.ResourcesBusy);
                // get resources
                response.Resources.AddRange(resources);
            }

            if (request.IncidentsImmediate || request.IncidentsOther)
            {
                var incidents = _incStore.GetIncidents(request.Revision, request.IncidentsImmediate, request.IncidentsOther);
                response.Incidents.AddRange(incidents);
            }

            // calculate the maximum revision being returned.
            long c1 = 0, c2 = 0, c3 = 0;
            if (response.Resources.Count > 0)
                c1 = response.Resources.Max(x => x.Revision);

            if (response.Incidents.Count > 0)
                c2 = response.Incidents.Max(x => x.Revision);

            response.Revision = Math.Max(Math.Max(c1, c2), c3);

            if (response.Revision == 0)
                response.Revision = response.CurrRevision;

            Debug.Print($"Map rev in={request.Revision} rev out={response.Revision} rev cur = {response.CurrRevision} count inc={response.Incidents.Count} res={response.Resources.Count}");

            return response;
        }


        private List<QuestDestination> GetDestinations(bool hospitals, bool stations, bool standby)
        {
            return _dbFactory.Execute<QuestContext, List<QuestDestination>>((db) =>
            {
                var d = db.Destinations
                    .Where(x => ((hospitals == true && x.IsHospital == true))
                             || ((stations == true && x.IsStation == true))
                             || ((standby == true && x.IsStandby == true))
                              )
                    .Where(x=>x.EndDate ==null)
                    .ToList()
                    .Select(x =>
                    {
                        var point = GeomUtils.GetPointFromWkt(x.Wkt);
                        var latlng = LatLongConverter.OSRefToWGS84(point.X, point.Y);
                        return new QuestDestination
                        {
                            Id = x.DestinationId.ToString(),
                            IsHospital = x.IsHospital ?? false,
                            IsAandE = x.IsAandE ?? false,
                            IsRoad = x.IsRoad ?? false,
                            IsStandby = x.IsStandby ?? false,
                            IsStation = x.IsStation ?? false,
                            Name = x.Destination,
                            Status=x.Status,
                            Position = new LatLongCoord(latlng.Longitude, latlng.Latitude)
                        };

                    }
                    )
                    .ToList();

                return d;
            });
        }

        private string GetStatusDescription(DataModel.ResourceStatus status)
        {
            return GetStatusDescription(status.Available ?? false, status.Busy ?? false, status.BusyEnroute ?? false, status.Rest ?? false);
        }

        private string GetStatusDescription(DataModel.Resource status)
        {
            return GetStatusDescription(status.ResourceStatus.Available ?? false, status.ResourceStatus.Busy ?? false, status.ResourceStatus.BusyEnroute ?? false, status.ResourceStatus.Rest ?? false);
        }

        private string GetStatusDescription(bool available, bool busy, bool enroute, bool rest)
        {
            if (available == true)
                return "Available";
            if (enroute == true)
                return "Enroute";
            if (busy == true)
                return "Busy";
            if (rest == true)
                return "Rest";
            return "Offroad";
        }

    }
}