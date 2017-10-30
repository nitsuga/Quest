using System;
using System.Collections.Generic;
using GeoJSON.Net.Geometry;
using Quest.Common.Messages;
using Quest.Mobile.Models;
using Quest.Lib.ServiceBus;
using System.Threading.Tasks;
using Quest.Common.Messages.Device;
using Quest.Common.Messages.GIS;
using Quest.Common.Messages.Resource;

namespace Quest.WebCore.Services
{

    public class ResourceService
    {
        AsyncMessageCache _messageCache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageCache"></param>
        public ResourceService(AsyncMessageCache messageCache)
        {
            _messageCache = messageCache;
        }

        public ResourceService()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="avail"></param>
        /// <param name="busy"></param>
        /// <returns></returns>
        public async Task<ResourceFeatureCollection> GetResources(bool avail = false, bool busy = false)
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

            var results =await _messageCache.SendAndWaitAsync<MapItemsResponse>(request, new TimeSpan(0, 0, 10));

            var features = new List<ResourceFeature>();
            if (results != null)
            {
                foreach (var res in results.Resources)
                {
                    ResourceFeature feature = GetResourceUpdateFeature(res);
                    if (feature != null)
                        features.Add(feature);

                }
                //foreach (var res in results.Devices)
                //{
                //    ResourceFeature feature = GetResourceUpdateFeature(res);
                //    if (feature != null)
                //        features.Add(feature);

                //}
            }
            var result = new ResourceFeatureCollection();
            result.Features.AddRange(features.ToArray());
            return result;

        }

        public ResourceFeature GetResourceUpdateFeature(ResourceUpdate item)
        {
            if (item.Item != null)
                return GetResourceUpdateFeature(item.Item);
            else
                return GetResourceDeleteFeature(item.Callsign);
        }

        public ResourceFeature GetResourceDeleteFeature(string callsign)
        {
            var feature = new ResourceFeature(new Point(new Position(0, 0)), null)
            {
                ID = callsign,
                FeatureType = "res",
                Action = "d",
            };
            return feature;
        }

        /// <summary>
        ///  //TODO: LD => Check mappings with MP
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public ResourceFeature GetResourceUpdateFeature(ResourceItem res)
        {
            try
            {
                ResourceFeature feature = null;

                    var geometry = new Point(new Position(res.Y, res.X));
                    var properties = new GTResourceFeatureProperties
                    {
                        Speed = res.Resource.Speed,
                        Course = res.Resource.Course,
                        EventId = res.Resource.EventId,
                        Callsign = res.Resource.Callsign,
                        ResourceType = res.Resource.ResourceType,
                        TimeStamp = res.Resource.LastUpdated?.ToString("dd/MM/yyyy HH:mm:ss"),
                        Destination = res.Resource.Destination,
                        ETA = res.Resource.Eta?.ToString("dd/MM/yyyy HH:mm:ss") ?? string.Empty,
                        Fleet = res.Resource.FleetNo?.ToString() ?? string.Empty,
                        Comment = res.Resource.Comment,
                        Skill = res.Resource.Skill,
                        currStatus = res.Resource.Status,
                        StatusCategory = res.Resource.StatusCategory,
                        Area = "",
                        ResourceTypeGroup = res.Resource.ResourceTypeGroup
                    };
                    feature = new ResourceFeature(geometry, properties)
                    {
                        ID = res.ID.ToString(),
                        FeatureType = "res",
                        Action = "u",
                    };

                return feature;
            }

            catch
            { }
            return null;
        }

        public async Task<CancelDeviceResponse> CancelDevice(string callsign, string eventId)
        {
            var request = new CancelDeviceRequest() { Callsign = callsign, EventId = eventId };
            _messageCache.BroadcastMessage(request);
            var results = await _messageCache.SendAndWaitAsync<CancelDeviceResponse>(request, new TimeSpan(0, 0, 10));
            return results;            
        }
    }
}
