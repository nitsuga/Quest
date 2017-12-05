using Quest.Common.Messages.Destination;
using Quest.Common.Messages.GIS;
using Quest.Lib.Coords;
using Quest.Lib.Data;
using Quest.Lib.DataModel;
using Quest.Lib.DependencyInjection;
using Quest.Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quest.Lib.Destinations
{
    [Injection(typeof(IDestinationStore))]
    public class DestinationStore : IDestinationStore
    {
        private IDatabaseFactory _dbFactory;

        public DestinationStore(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;

        }

        public QuestDestination GetDestination(string code)
        {
            return _dbFactory.Execute<QuestContext, QuestDestination>((db) =>
            {
                var x = db.Destinations.FirstOrDefault(y => y.EndDate == null && y.Shortcode == code);
                if (x == null)
                    return null;


                var point = GeomUtils.GetPointFromWkt(x.Wkt);
                var latlng = LatLongConverter.OSRefToWGS84(point.X, point.Y);
                return new QuestDestination
                {
                    Id = x.DestinationId.ToString(),
                    Code = x.Shortcode,
                    IsHospital = x.IsHospital ?? false,
                    IsAandE = x.IsAandE ?? false,
                    IsRoad = x.IsRoad ?? false,
                    IsStandby = x.IsStandby ?? false,
                    IsStation = x.IsStation ?? false,
                    Name = x.Destination,
                    Status = x.Status,
                    Position = new LatLng(latlng.Latitude, latlng.Longitude)
                };

            });
        }


        public List<QuestDestination> GetDestinations(bool hospitals, bool stations, bool standby)
        {
            return _dbFactory.Execute<QuestContext, List<QuestDestination>>((db) =>
            {
                var d = db.Destinations
                    .Where(x => ((hospitals == true && x.IsHospital == true))
                             || ((stations == true && x.IsStation == true))
                             || ((standby == true && x.IsStandby == true))
                              )
                    .Where(x => x.EndDate == null)
                    .ToList()
                    .Select(x =>
                    {
                        var point = GeomUtils.GetPointFromWkt(x.Wkt);
                        var latlng = LatLongConverter.OSRefToWGS84(point.X, point.Y);
                        return new QuestDestination
                        {
                            Id = x.DestinationId.ToString(),
                            Code = x.Shortcode,
                            IsHospital = x.IsHospital ?? false,
                            IsAandE = x.IsAandE ?? false,
                            IsRoad = x.IsRoad ?? false,
                            IsStandby = x.IsStandby ?? false,
                            IsStation = x.IsStation ?? false,
                            Name = x.Destination,
                            Status = x.Status,
                            Position = new LatLng(latlng.Latitude, latlng.Longitude)
                        };

                    }
                    )
                    .ToList();

                return d;
            });
        }

    }
}
