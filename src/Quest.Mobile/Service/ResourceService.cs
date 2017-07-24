using System;
using System.Collections.Generic;
using GeoJSON.Net.Geometry;
using Quest.Common.Messages;
using Quest.Lib.ServiceBus;
using Quest.Mobile.Models;

namespace Quest.Mobile.Service
{

    public class ResourceService
    {
        MessageCache _messageCache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageCache"></param>
        public ResourceService(MessageCache messageCache)
        {
            _messageCache = messageCache;
        }

        public ResourceService()
        {
        }

        public MapItemsResponse GetMapItems(MapItemsRequest request)
        {
            return _messageCache.SendAndWait<MapItemsResponse>(request, new TimeSpan(0, 0, 10));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="avail"></param>
        /// <param name="busy"></param>
        /// <returns></returns>
        public ResourceFeatureCollection GetResources(bool avail = false, bool busy = false)
        {
            MapItemsRequest request = new MapItemsRequest()
            {
                Hospitals = false,
                IncidentsImmediate = false,
                IncidentsOther = false,
                ResourcesAvailable = avail,
                ResourcesBusy = busy,
                Revision = 0,
                Standby = false,
                Stations = false
            };

            var results = _messageCache.SendAndWait<MapItemsResponse>(request, new TimeSpan(0, 0, 10));

            var features = new List<ResourceFeature>();
            if (results != null)
            {
                foreach (var res in results.Resources)
                {
                    ResourceFeature feature = GetResourceUpdateFeature(res, res.ID);
                    if (feature != null)
                        features.Add(feature);

                }
                foreach (var res in results.Devices)
                {
                    ResourceFeature feature = GetResourceUpdateFeature(res, res.ID);
                    if (feature != null)
                        features.Add(feature);

                }
            }
            var result = new ResourceFeatureCollection();
            result.Features.AddRange(features.ToArray());
            return result;

        }


        public ResourceFeature GetResourceUpdateFeature(ResourceDatabaseUpdate item)
        {
            return GetResourceUpdateFeature(item.Item, item.ResourceId.ToString());
        }

        public ResourceFeature GetResourceDeleteFeature(ResourceDatabaseUpdate item)
        {
            var feature = new ResourceFeature(new Point(new Position(item.Item.Y, item.Item.X)), null)
            {
                //ID = item.Item.ID.ToString(),
                //FeatureType = "res",
                //Action = "d",
            };
            return feature;
        }

        /// <summary>
        ///  //TODO: LD => Check mappings with MP
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public ResourceFeature GetResourceUpdateFeature(ResourceItem res, string id)
        {
            try
            {
                ResourceFeature feature = null;

                if (res != null)
                {
                    var geometry = new Point(new Position(res.Y, res.X));
                    var properties = new ResourceFeatureProperties
                    {
                        Speed = res.Speed,
                        Direction = res.Direction,
                        IncSerial = res.Incident,
                        Callsign = res.Callsign,
                        ResourceType = res.VehicleType,
                        TimeStamp = res.lastUpdate?.ToString("dd/MM/yyyy HH:mm:ss"),
                        Destination = res.Destination,
                        ETA = res.Eta?.ToString("dd/MM/yyyy HH:mm:ss") ?? string.Empty,
                        Fleet = res.FleetNo?.ToString() ?? string.Empty,
                        Road = res.Road ?? string.Empty,
                        Comment = res.Comment,
                        Skill = res.Skill,
                        CurrStatus = res.Status,
                        PrevStatus = res.PrevStatus
                        ,
                        StatusCategory = res.StatusCategory
                        ,
                        Area = ""
                        ,
                        ResourceTypeGroup = res.ResourceTypeGroup
                    };
                    feature = new ResourceFeature(geometry, properties)
                    {
                    };
                }
                else
                    feature = new ResourceFeature(new Point(new Position(res.Y, res.X)), null)
                    {
                    };


                return feature;
            }

            catch
            { }
            return null;
        }
    }
}
