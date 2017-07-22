using Quest.Common.Simulation;
using Quest.Lib.DataModel;
using Quest.Lib.Routing;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Quest.Lib.Simulation.Destinations
{
    public class DestinationStoreDb : IDestinationStore
    {
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

            using (var db = new QuestEntities())
            {
                var d = db.DestinationViews
                    .ToList()
                    .Select(x =>
                    {
                        var pos = new GeoAPI.Geometries.Coordinate(x.X ?? 0, x.Y ?? 0);
                        var edge = _data.GetEdgeFromPoint(pos);
                        return new SimDestination
                        {
                            DestinationId = x.DestinationID,
                            IsHospital = x.IsHospital ?? false,
                            IsAandE = x.IsAandE ?? false,
                            IsRoad = x.IsRoad ?? false,
                            IsStandby = x.IsStandby ?? false,
                            IsStation = x.IsStation ?? false,
                            Name = x.Destination,
                            Position = pos,
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
