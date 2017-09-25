using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Quest.Common.Simulation;
using Quest.Lib.DataModel;
using Quest.Lib.Research.DataModelResearch;
using Quest.Lib.Routing;
using Quest.Lib.Simulation.DataModelSim;
using Quest.Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Quest.Lib.Simulation.Destinations
{
    [Obsolete]
    public class DestinationStoreDb : IDestinationStore
    {

        public string Filename { get; set; }
        private RoutingData _data;

        List<SimDestination> _allDestinations;

        public DestinationStoreDb(RoutingData data)
        {
            _data = data;
            _allDestinations = GetAllDestinations();
        }


        private List<SimDestination> GetAllDestinations()
        {
            while (!_data.IsInitialised)
                Thread.Sleep(1);

            using (var db = new QuestSimContext())
            {
                var d = db.Destinations
                    .ToList()
                    .Select(x =>
                    {
                        var pos = new GeoAPI.Geometries.Coordinate(0,0);
                        var edge = _data.GetEdgeFromPoint(pos);
                        return new SimDestination
                        {
                            ID = x.DestinationId.ToString(),
                            IsHospital = x.IsHospital,
                            IsRoad = x.IsRoad ,
                            IsStandby = x.IsStandby ,
                            IsStation = x.IsStation ,
                            Name = x.Destination,
                            X = 0,
                            Y = 0,
                            RoadPosition = edge
                        };
                    })
                    .ToList();

                //foreach (var res in d)
                //{
                //    var latlng = LatLongConverter.OSRefToWGS84(res.Position);
                //    res.Position = new GeoAPI.Geometries.Coordinate(latlng.Longitude, latlng.Latitude);
                //}
                return d;
            }
        }

        public List<SimDestination> GetDestinations(bool hospitals, bool stations, bool standby)
        {
            var d = _allDestinations
                .Where(x => ((hospitals == true && x.IsHospital == true))
                         || ((stations == true && x.IsStation == true))
                         || ((standby == true && x.IsStandby == true))
                          )
                .ToList();
            return d;
        }

    }
}
