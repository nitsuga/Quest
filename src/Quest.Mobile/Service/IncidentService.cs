using GeoJSON.Net.Geometry;
using Quest.Lib.ServiceBus;
using Quest.Common.Messages;
using Quest.Mobile.Models;
using System;
using System.Collections.Generic;

namespace Quest.Mobile.Service
{

    public class IncidentService
    {
        
        MessageCache _messageCache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageCache"></param>
        public IncidentService(MessageCache messageCache)
        {
            _messageCache = messageCache;
        }
        public IncidentService()
        {
        }

        public IncidentFeatureCollection GetIncidents(bool includeCatA = false, bool includeCatB = false)
        {
            MapItemsRequest request = new MapItemsRequest()
            {
                Hospitals = false,
                IncidentsImmediate = includeCatA,
                IncidentsOther = includeCatB,
                ResourcesAvailable = false,
                ResourcesBusy = false,
                Revision = 0,
                Standby = false,
                Stations = false
            };

            var results = _messageCache.SendAndWait<MapItemsResponse>(request, new TimeSpan(0, 0, 10));

            var features = new List<IncidentFeature>();

            if (results != null)
            {
                foreach (var res in results.Events)
                {
                    var feature = GetIncidentUpdateFeature(res);
                    if (feature != null)
                        features.Add(feature);
                }
            }

            var result = new IncidentFeatureCollection();
            result.Features.AddRange(features.ToArray());
            return result;

        }


        /// <summary>
        ///  //TODO: LD => Check mappings with MP
        /// </summary>
        /// <returns></returns>
        public static IncidentFeature GetIncidentUpdateFeature(EventMapItem inc)
        {
            if (inc.LastUpdated == null) return null;
            var geometry = new Point(new Position(inc.Y, inc.X));
            var properties = new IncidentFeatureProperties
            {
                Description = inc.DeterminantDescription,
                Determinant = inc.Determinant,
                Location = inc.Location,
                Priority = inc.Priority,
                Status = inc.Status,
                LastUpdate = inc.LastUpdated.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                IncidentId = inc.EventId,
                AssignedResources = inc.AssignedResources,
                Age = inc.PatientAge ?? "?",
                AZ = inc.AZ ?? "?",
                LocationComment = inc.LocationComment ?? "",
                ProblemDescription = inc.ProblemDescription ?? "",
                Sex = inc.PatientSex ?? "?"
            };

            var feature = new IncidentFeature(geometry, properties)
            {
                ID = inc.EventId.ToString(),
                FeatureType = "inc",
                Action = "u",
            };

            return feature;
        }

    }
}
