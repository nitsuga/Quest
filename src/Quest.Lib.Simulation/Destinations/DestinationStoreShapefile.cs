using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Quest.Common.Simulation;
using System.Collections.Generic;
using System.Linq;
using Quest.Lib.Simulation.Destinations;
using Quest.Lib.Routing;
using Quest.Lib.Utils;
using System.Threading;

namespace Quest.Lib.Simulation.Resources
{
    public class DestinationStoreShapefile : IDestinationStore
    {

        public string Filename { get; set; }

        private RoutingData _data;
        private List<SimDestination> _allDestinations;

        public DestinationStoreShapefile(RoutingData data)
        {
            _data = data;
            _allDestinations = LoadDestinations();
        }

        private List<SimDestination> LoadDestinations()
        {
            while (!_data.IsInitialised)
                Thread.Sleep(1);

            List<SimDestination> list = new List<SimDestination>();

            IGeometryFactory geomFact = new GeometryFactory();
            var cwd = System.IO.Directory.GetCurrentDirectory();
            var path = System.IO.Path.Combine(cwd, Filename);
            using (var reader = new ShapefileDataReader(path, geomFact))
            {
                while (reader.Read())
                {
                    var geom = reader.Geometry;
                    Point p = geom as Point;
                    var en = LatLongConverter.WGS84ToOSRef(p.Y, p.X);

                    var edge = _data.GetEdgeFromPoint(new Coordinate(en.Easting, en.Northing));
                    list.Add(new SimDestination
                    {
                        ID = reader.GetInt32(0).ToString(),
                        Name = reader.GetString(1),
                        IsHospital = reader.GetBoolean(2),
                        IsStandby = reader.GetBoolean(3),
                        IsStation = reader.GetBoolean(4),
                        IsRoad = reader.GetBoolean(5),
                        X = en.Easting,
                        Y = en.Northing,
                        RoadPosition = edge
                    });
                }
            }
            return list;
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
