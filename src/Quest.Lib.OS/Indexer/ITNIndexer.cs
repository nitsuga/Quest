using System;
using System.Collections.Generic;
using System.Linq;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Quest.Lib.OS.DataModelOS;
using Quest.Lib.Data;
using Quest.Common.Messages.Gazetteer;
using Quest.Lib.Coords;

namespace Quest.Lib.OS.Indexer
{
    public class ItnIndexer : ElasticIndexer
    {
        private IDatabaseFactory _dbFactory;

        public ItnIndexer(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public override void StartIndexing(BuildIndexSettings config)
        {
            Build(config);
        }

        private void Build(BuildIndexSettings config)
        {
            DeleteDataSet<LocationDocument>(config.DefaultIndex, config.Client, IndexBuilder.AddressDocumentType.RoadLink);

            _dbFactory.Execute<QuestOSContext>((db) =>
            {
                var total = db.Road.Count();

                var descriptor = GetBulkRequest(config);

                config.RecordsTotal = total;

                // set up conversion to/from UTM13
                var csFact = new CoordinateSystemFactory();
                var ctFact = new CoordinateTransformationFactory();
                var from = csFact.CreateFromWkt($@"PROJCS[""OSGB 1936 / British National Grid"",  GEOGCS[""OSGB 1936"",    DATUM[""OSGB 1936"",      SPHEROID[""Airy 1830"", 6377563.396, 299.3249646, AUTHORITY[""EPSG"",""7001""]],      TOWGS84[446.448, -125.157, 542.06, 0.15, 0.247, 0.842, -4.2261596151967575],      AUTHORITY[""EPSG"",""6277""]],    PRIMEM[""Greenwich"", 0.0, AUTHORITY[""EPSG"",""8901""]],    UNIT[""degree"", 0.017453292519943295],    AXIS[""Geodetic longitude"", EAST],    AXIS[""Geodetic latitude"", NORTH],    AUTHORITY[""EPSG"",""4277""]],  PROJECTION[""Transverse Mercator""],  PARAMETER[""central_meridian"", -2.0],  PARAMETER[""latitude_of_origin"", 49.0],  PARAMETER[""scale_factor"", 0.9996012717],  PARAMETER[""false_easting"", 400000.0],  PARAMETER[""false_northing"", -100000.0],  UNIT[""m"", 1.0],  AXIS[""Easting"", EAST],  AXIS[""Northing"", NORTH],  AUTHORITY[""EPSG"",""27700""]]");
                var wgs84 = GeographicCoordinateSystem.WGS84;
                var transformer = ctFact.CreateFromCoordinateSystems(from, wgs84);

                foreach (var r in db.StaticRoadNames.OrderBy(x => x.RoadId))
                {
                    config.RecordsCurrent++;

                    var point = GeomUtils.ConvertToLatLonLoc(r.X, r.Y);

                    // check whether point is in master area if required
                    if (!IsPointInRange(config, point.Longitude, point.Latitude))
                    {
                        config.Skipped++;
                        continue;
                    }

                    var terms = GetLocalAreas(config, point);

                    // commit any messages and report progress
                    CommitCheck(this, config, descriptor);

                    if (r.RoadName == null)
                    {
                        config.Skipped++;
                        continue;
                    }

                    var address = new LocationDocument
                    {
                        Created = DateTime.Now,
                        Type = IndexBuilder.AddressDocumentType.RoadLink,
                        Source = "ITN",
                        ID = IndexBuilder.AddressDocumentType.RoadLink + r.RoadNetworkMemberId,
                        //BuildingName = "",
                        Description = Join(r.RoadName, terms, true),
                        indextext = Join(r.RoadName, terms, false, " ").Decompound(config.DecompoundList),
                        Location = point,
                        Point = PointfromGeoLocation(point),
                        //Organisation = "",
                        //Postcode = "",
                        //      SubBuilding = "",
                        Thoroughfare = new List<string> { r.RoadName.ToUpper() },
                        Locality = new List<string>(),
                        Areas = terms,
                        Status = "Approved",
                        MultiLine = GeomUtils.GetMultiLine(r.Wkt, transformer),
                        Classification = "CT99" // Road
                    };


                    // add to the list of stuff to index
                    address.indextext = address.indextext.Replace("&", " and ");

                    // add item to the list of documents to index
                    AddIndexItem(address, descriptor);
                }

                CommitBultRequest(config, descriptor);

            });
        }
    }
}