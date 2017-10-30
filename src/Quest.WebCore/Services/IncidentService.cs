using GeoJSON.Net.Geometry;
using Quest.Lib.ServiceBus;
using Quest.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quest.Common.Messages.GIS;

namespace Quest.WebCore.Services
{

    public class IncidentService
    {

        AsyncMessageCache _messageCache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageCache"></param>
        public IncidentService(AsyncMessageCache messageCache)
        {
            _messageCache = messageCache;
        }
        public IncidentService()
        {
        }

        public async Task<IncidentFeatureCollection> GetIncidents(bool includeCatA = false, bool includeCatB = false)
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

            var results =await _messageCache.SendAndWaitAsync<MapItemsResponse>(request, new TimeSpan(0, 0, 10));

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
        public static IncidentFeature GetIncidentUpdateFeature(IncidentItem inc)
        {
            var geometry = new Point(new Position(inc.Y, inc.X));
            var properties = new GTIncidentFeatureProperties
            {
                Description = inc.Incident.DeterminantDescription,
                Determinant = inc.Incident.Determinant,
                Location = inc.Incident.Location,
                Priority = inc.Incident.Priority,
                Status = inc.Incident.Status,
                IncidentId = inc.Incident.Serial,
                AssignedResources = inc.Incident.AssignedResources,
                Age = inc.Incident.PatientAge ?? "?",
                LocationComment = inc.Incident.LocationComment ?? "",
                ProblemDescription = inc.Incident.ProblemDescription ?? "",
                Sex = inc.Incident.PatientSex ?? "?"
            };

            var feature = new IncidentFeature(geometry, properties)
            {
                ID = inc.ID.ToString(),
                FeatureType = "inc",
                Action = "u",
            };

            return feature;
        }

    }
}
