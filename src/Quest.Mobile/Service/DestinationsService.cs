﻿using System;
using System.Collections.Generic;
using GeoJSON.Net.Geometry;
using Quest.Common.Messages;
using Quest.Lib.ServiceBus;
using Quest.Mobile.Models;

namespace Quest.Mobile.Service
{
    /// <summary>
    /// 
    /// </summary>

    public class DestinationService
    {
        
        MessageCache _messageCache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageCache"></param>
        public DestinationService(MessageCache messageCache)
        {
            _messageCache = messageCache;
        }

        public DestinationService()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hosp"></param>
        /// <param name="standby"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        public DestinationFeatureCollection GetDestinations(bool hosp = false, bool standby = false, bool station = false)
        {
            MapItemsRequest request = new MapItemsRequest()
            {
                Hospitals = hosp,
                IncidentsImmediate = false,
                IncidentsOther = false,
                ResourcesAvailable = false,
                ResourcesBusy = false,
                Revision = 0,
                Standby = standby,
                Stations = station
            };

            var results = _messageCache.SendAndWait<MapItemsResponse>(request, new TimeSpan(0, 0, 10));

            var features = new List<DestinationFeature>();

            foreach (var res in results.Destinations)
            {
                var feature = GetDestinationUpdateFeature(res);
                features.Add(feature);

            }

            var result = new DestinationFeatureCollection();
            result.Features.AddRange(features.ToArray());
            return result;

        }


        /// <summary>
        ///  //TODO: LD => Check mappings with MP
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public  DestinationFeature GetDestinationUpdateFeature(QuestDestination item)
        {
            try
            {
                var geometry = new Point(new Position(item.Y, item.X));
                var properties = new DestinationFeatureProperties
                {
                    CoverTier = 0,
                    Destination = item.Name,
                };

                var t = "";
                if (item.IsHospital == true)
                    t += "Hosp ";
                if (item.IsAandE == true)
                    t += "A&E ";
                if (item.IsRoad == true)
                    t += "Road ";
                if (item.IsStation == true)
                    t += "Station ";
                if (item.IsStandby == true)
                    t += "Sbp ";

                properties.DesType = t;

                if (item.IsRoad == true)
                    properties.Status = "RD";
                if (item.IsStandby == true)
                    properties.Status = "SBP";
                if (item.IsStation == true)
                    properties.Status = "STA";
                if (item.IsHospital == true)
                    properties.Status = "HOS";
                if (item.IsAandE == true)
                    properties.Status = "AE";

                var feature = new DestinationFeature(geometry, properties)
                {
                    ID = item.ID,
                };

                return feature;
            }
            catch
            { }
            return null;
        }

        
    }
}
