using System;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using Nest;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Quest.Common.Messages;
using Quest.Lib.Trace;
using Quest.Common.Messages.Gazetteer;

namespace Quest.Lib.Search.Indexers
{
    internal class ShapeFileDefinition
    {
        internal string Filename;
        internal Func<Context, LocationDocument> Processor;
    }

    internal class Context
    {
        internal ICoordinateTransformation Transformer;
        internal PolygonData Data;
        internal int Row;
        public int Skipped { get; set; }
        public int Errors { get; set; }
    }



    internal class SanDiegoIndexer : ElasticIndexer
    {
        private PolygonManager _localAreas;

        private LocationDocument TransitRouteProcessor(Context ctx)
        {
            var wgs84Coord = ctx.Transformer.MathTransform.Transform(ctx.Data.geom.Coordinate);

            // convert to local
            var point = GeomUtils.ConvertFromCoordinate(wgs84Coord);

            var description = $"{ctx.Data.data[8]} {ctx.Data.data[4]} {ctx.Data.data[5]}";

            var areas = _localAreas.Search(ctx.Data.geom.Coordinate);

            if (areas.Count > 0)
            {
                description += ", " + string.Join(" ", areas.Select(x => x.data[1] + " " + x.data[0]).ToList());
            }

            var address = new LocationDocument
            {
                Created = DateTime.UtcNow,
                Type = IndexBuilder.AddressDocumentType.RoadLink,
                Source = "SanDiego_TransRoute",
                ID = $"SD_trr {ctx.Data.data[0]}",
                Description = description,
                indextext = description,
                Location = point,
                Point = PointfromGeoLocation(point),
                Status = "Approved",
                MultiLine = GeomUtils.GetMultiLine(ctx.Data.geom, ctx.Transformer),
            };

            // add to the list of stuff to index
            address.indextext = address.indextext.Replace("&", " and ");

            return address;

        }

        private LocationDocument TransitStopProcessor(Context ctx)
        {
            var wgs84Coord = ctx.Transformer.MathTransform.Transform(ctx.Data.geom.Coordinate);

            // convert to local
            var point = GeomUtils.ConvertFromCoordinate(wgs84Coord);

            var description = $"STOP {ctx.Data.data[4]} {ctx.Data.data[7]}";

            var areas = _localAreas.Search(ctx.Data.geom.Coordinate);

            if (areas.Count > 0)
            {
                description += " " + string.Join(" ", areas.Select(x => x.data[1] + " " + x.data[0]).ToList());
            }

            var address = new LocationDocument
            {
                Created = DateTime.UtcNow,
                Type = IndexBuilder.AddressDocumentType.RoadLink,
                Source = "SanDiego_TransPoint",
                ID = $"SD_trp {ctx.Data.data[0]}",
                Description = description,
                indextext = description,
                Location = point,
                Point = PointfromGeoLocation(point),
                Status = "Approved",
                MultiLine = null, 
            };

            // add to the list of stuff to index
            address.indextext = address.indextext.Replace("&", " and ");

            return address;

        }

        private LocationDocument PlaceProcessor(Context ctx)
        {
            //ctx.Transformer.MathTransform.TransformList(points).ToArray();
            var wgs84Coord = ctx.Transformer.MathTransform.Transform(ctx.Data.geom.Coordinate);

            var point = GeomUtils.ConvertFromCoordinate(wgs84Coord);

            var zip = ctx.Data.data[3];
            if (zip == "0")
                zip = "";

            var description = Join(new[] { ctx.Data.data[0], ctx.Data.data[1], ctx.Data.data[2], zip });

            var address = new LocationDocument
            {
                Created = DateTime.UtcNow,
                Type = IndexBuilder.AddressDocumentType.Address,
                Source = "SanDiego_Place",
                ID = $"SD_plc {ctx.Row}",
                Description = description,
                indextext = description,
                Location = point,
                Point = PointfromGeoLocation(point),
                Status = "Approved",
                //MultiLine = GeomUtils.MakeMultiLine(r.Shape),
            };

            // add to the list of stuff to index
            address.indextext = address.indextext.Replace("&", " and ");

            return address;
        }

        private LocationDocument RoadProcessor(Context ctx)
        {
            var wgs84Coord = ctx.Transformer.MathTransform.Transform(ctx.Data.geom.Coordinate);

            // convert to local
            var point = GeomUtils.ConvertFromCoordinate(wgs84Coord);

            var description = $"{ctx.Data.data[58]}";

            var areas = _localAreas.Search(ctx.Data.geom.Coordinate);

            if (areas.Count > 0)
            {
                description += " " + string.Join( " " , areas.Select(x => x.data[1] + " " + x.data[0]).ToList());
            }

            var address = new LocationDocument
            {
                Created = DateTime.UtcNow,
                Type = IndexBuilder.AddressDocumentType.RoadLink,
                Source = "SanDiego_Road",
                ID = $"SD_rd {ctx.Row}",
                Description = description,
                indextext = description,
                Location = point,
                Point = PointfromGeoLocation(point),
                Status = "Approved",
                MultiLine = GeomUtils.GetMultiLine(ctx.Data.geom, ctx.Transformer),
            };

            // add to the list of stuff to index
            address.indextext = address.indextext.Replace("&", " and ");

            return address;

        }

        private LocationDocument ZipProcessor(Context ctx)
        {
            var wgs84Coord = ctx.Transformer.MathTransform.Transform(ctx.Data.geom.Coordinate);

            // convert to local
            var point = GeomUtils.ConvertFromCoordinate(wgs84Coord);

            var description = $"{ctx.Data.data[1]} {ctx.Data.data[0]}";

            var address = new LocationDocument
            {
                Created = DateTime.UtcNow,
                Type = IndexBuilder.AddressDocumentType.LocalName,
                Source = "SanDiego_Zip",
                ID = $"SD_zip {ctx.Row}",
                Description = description,
                indextext = description,
                Location = point,
                Point = PointfromGeoLocation(point),
                Status = "Approved",
                Poly = GeomUtils.GetPolygon(ctx.Data.geom, ctx.Transformer),
                MultiLine = null,
            };

            // add to the list of stuff to index
            address.indextext = address.indextext.Replace("&", " and ");

            return address;

        }

        private LocationDocument IntersectionProcessor(Context ctx)
        {
            return null;
        }

        private LocationDocument ParksProcessor(Context ctx)
        {
            var wgs84Coord = ctx.Transformer.MathTransform.Transform(ctx.Data.geom.Coordinate);

            var point = GeomUtils.ConvertFromCoordinate(wgs84Coord);

            var description = Join(new[] { ctx.Data.data[0], ctx.Data.data[7] });

            var address = new LocationDocument
            {
                Created = DateTime.UtcNow,
                Type = IndexBuilder.AddressDocumentType.Poi,
                Source = "SanDiego_Park",
                ID = $"SD_prk {ctx.Row}",
                Description = description,
                indextext = description,
                Location = point,
                Point = PointfromGeoLocation(point),
                Status = "Approved",
                //MultiLine = GeomUtils.MakeMultiLine(r.Shape),
            };

            // add to the list of stuff to index
            address.indextext = address.indextext.Replace("&", " and ");

            return address;
        }

        private LocationDocument AddressProcessor(Context ctx)
        {
            var wgs84Coord = ctx.Transformer.MathTransform.Transform(ctx.Data.geom.Coordinate);

            var point = GeomUtils.ConvertFromCoordinate(wgs84Coord);

            var num = ctx.Data.data[0].Split('.');
            var road = ctx.Data.data[3];

            if (ctx.Data.data[4].Length > 0)
                road += " " + ctx.Data.data[4];

            if (ctx.Data.data[5].Length > 0)
                road += " " + ctx.Data.data[5];

            var description = Join(new[] { ctx.Data.data[6] ,num[0], ctx.Data.data[1], ctx.Data.data[2], road, ctx.Data.data[16], ctx.Data.data[7]});

            var address = new LocationDocument
            {
                Created = DateTime.UtcNow,
                Type = IndexBuilder.AddressDocumentType.Address,
                Source = "SanDiego_Address",
                ID = $"SD_add {ctx.Row}",
                Description = description,
                indextext = description,
                Location = point,
                Point = PointfromGeoLocation(point),
                Status = "Approved",
                //MultiLine = GeomUtils.MakeMultiLine(r.Shape),
            };

            // add to the list of stuff to index
            address.indextext = address.indextext.Replace("&", " and ");

            return address;
        }

        private LocationDocument BusinessSitesProcessor(Context ctx)
        {
            var wgs84Coord = ctx.Transformer.MathTransform.Transform(ctx.Data.geom.Coordinate);

            var point = GeomUtils.ConvertFromCoordinate(wgs84Coord);

            var description = Join(new[] { ctx.Data.data[3], ctx.Data.data[10], ctx.Data.data[11], ctx.Data.data[12], ctx.Data.data[13] });
            var index = Join(new[] { ctx.Data.data[3], ctx.Data.data[4], ctx.Data.data[5], ctx.Data.data[6], ctx.Data.data[10], ctx.Data.data[11], ctx.Data.data[12], ctx.Data.data[13] });

            var address = new LocationDocument
            {
                Created = DateTime.UtcNow,
                Type = IndexBuilder.AddressDocumentType.Address,
                Source = "SanDiego_Business",
                ID = $"SD_bus {ctx.Row}",
                Description = description,
                indextext = index,
                Location = point,
                Point = PointfromGeoLocation(point),
                Status = "Approved",
            };

            // add to the list of stuff to index
            address.indextext = address.indextext.Replace("&", " and ");

            return address;
        }

        private LocationDocument RunwaysProcessor(Context ctx)
        {
            var wgs84Coord = ctx.Transformer.MathTransform.Transform(ctx.Data.geom.Coordinate);

            var point = GeomUtils.ConvertFromCoordinate(wgs84Coord);

            var description = Join(new[] { ctx.Data.data[0], ctx.Data.data[1] });
            var index = description;

            var address = new LocationDocument
            {
                Created = DateTime.UtcNow,
                Type = IndexBuilder.AddressDocumentType.Poi,
                Source = "SanDiego_Runway",
                ID = $"SD_air {ctx.Row}",
                Description = description,
                indextext = index,
                Location = point,
                Point = PointfromGeoLocation(point),
                Status = "Approved",
            };

            // add to the list of stuff to index
            address.indextext = address.indextext.Replace("&", " and ");

            return address;
        }

        private LocationDocument RailRoadProcessor(Context ctx)
        {
            var wgs84Coord = ctx.Transformer.MathTransform.Transform(ctx.Data.geom.Coordinate);

            var point = GeomUtils.ConvertFromCoordinate(wgs84Coord);

            var description = Join(new[] { "railroad", ctx.Data.data[2], ctx.Data.data[1] });
            var address = new LocationDocument
            {
                Created = DateTime.UtcNow,
                Type = IndexBuilder.AddressDocumentType.Rail,
                Source = "SanDiego_Rail",
                ID = $"SD_rr {ctx.Row}",
                Description = description,
                indextext = description,
                Location = point,
                Point = PointfromGeoLocation(point),
                Status = "Approved",
                MultiLine = GeomUtils.GetMultiLine(ctx.Data.geom, ctx.Transformer),
            };

            // add to the list of stuff to index
            address.indextext = address.indextext.Replace("&", " and ");

            return address;
        }

        public override void StartIndexing(BuildIndexSettings config)
        {
            Build(config);
        }

        private void Build(BuildIndexSettings config)
        {

            var directory = "";

            if (config.Parameters.ContainsKey("directory"))
                directory = (string) config.Parameters["directory"];

            // set up conversion to/from UTM13
            CoordinateSystemFactory csFact = new CoordinateSystemFactory();
            CoordinateTransformationFactory ctFact = new CoordinateTransformationFactory();
            ICoordinateSystem from = csFact.CreateFromWkt( $@"PROJCS[""NAD_1983_StatePlane_California_VI_FIPS_0406_Feet"",GEOGCS[""GCS_North_American_1983"",DATUM[""D_North_American_1983"",SPHEROID[""GRS_1980"",6378137.0,298.257222101]],PRIMEM[""Greenwich"",0.0],UNIT[""Degree"",0.0174532925199433]],PROJECTION[""Lambert_Conformal_Conic""],PARAMETER[""False_Easting"",6561666.666666666],PARAMETER[""False_Northing"",1640416.666666667],PARAMETER[""Central_Meridian"",-116.25],PARAMETER[""Standard_Parallel_1"",32.78333333333333],PARAMETER[""Standard_Parallel_2"",33.88333333333333],PARAMETER[""Latitude_Of_Origin"",32.16666666666666],UNIT[""Foot_US"",0.3048006096012192]]");
            IGeographicCoordinateSystem wgs84 = GeographicCoordinateSystem.WGS84;
            ICoordinateTransformation trans = ctFact.CreateFromCoordinateSystems(from, wgs84);

            // build local areas
            var localpath = System.IO.Path.Combine(directory, "ZIP_CODES.zip");
            _localAreas = new PolygonManager();
            _localAreas.BuildFromShapefile(localpath);

            config.RecordsTotal = 0;

            // local and process definitions
            var definitions = GetDefinitions();
            var descriptor = GetBulkRequest(config);
            foreach (var def in definitions)
            {
                ProcessDefinition(config, directory, def, trans, descriptor);
            }
        }

        private void ProcessDefinition(BuildIndexSettings config, string directory, ShapeFileDefinition def, ICoordinateTransformation trans, BulkRequest descriptor)
        {
            var path = System.IO.Path.Combine(directory, def.Filename);
            IGeometryFactory geomFact = new GeometryFactory();

            try
            {
                config.Logfrequency = 100;
                Logger.Write($"Processing file {path}", GetType().Name);
                Context ctx = new Context() { Transformer = trans };
                var reader = new ShapefileDataReader(path, geomFact);

                Logger.Write($"Records: {reader.RecordCount} Fields: {reader.FieldCount} ", GetType().Name);

                while (reader.Read())
                {
                    try
                    {
                        List<string> data2 = new List<String>();
                        for (int i = 0; i < reader.FieldCount - 1; i++)
                        {
                            var v = reader.GetValue(i);
                            data2.Add(v?.ToString() ?? "");
                        }

                        //reader.GetValues(data);
                        var geom = reader.Geometry;

                        var polydata = new PolygonData { data = data2.ToArray(), geom = geom };

                        config.RecordsCurrent++;

                        ctx.Data = polydata;
                        ctx.Row++;

                        var address = def.Processor(ctx);

                        if (address == null)
                        {
                            config.Skipped++;
                            ctx.Skipped++;
                            continue;
                        }

                        // commit any messages and report progress
                        CommitCheck(this, config, descriptor);

                        // add item to the list of documents to index
                        AddIndexItem(address, descriptor);
                    }
                    catch (Exception ex)
                    {
                        config.Errors++;
                        ctx.Errors++;
                        Logger.Write($"Failed to process record {path}: {ex.Message}", GetType().Name);
                    }
                }

                CommitBultRequest(config, descriptor);
                Logger.Write($"Completed File {path} Totals: Processed {ctx.Row} Skipped: {ctx.Skipped} Errors: {ctx.Errors} ", GetType().Name);
            }
            catch (Exception ex)
            {
                config.Errors++;
                Logger.Write($"Failed to process file {path}: {ex.Message}", GetType().Name);
            }


        }

        private ShapeFileDefinition[] GetDefinitions()
        {
            ShapeFileDefinition[] definitions = new[]
            {
                new ShapeFileDefinition
                {
                    Filename = @"ADDRESS_APN.shp",
                    Processor = AddressProcessor
                },
                new ShapeFileDefinition
                {
                    Filename = @"PLACES.shp",
                    Processor = PlaceProcessor
                },

                //new ShapeFileDefinition
                //{
                //    Filename = @"ROADS_INTERSECTION.shp",
                //    Processor = IntersectionProcessor
                //},
                new ShapeFileDefinition
                {
                    Filename = @"PARKS.shp",
                    Processor = ParksProcessor
                },
                new ShapeFileDefinition
                {
                    Filename = @"Business_Sites.shp",
                    Processor = BusinessSitesProcessor
                },
                new ShapeFileDefinition
                {
                    Filename = @"RUNWAYS.shp",
                    Processor = RunwaysProcessor
                },
                new ShapeFileDefinition
                {
                    Filename = @"rr.shp",
                    Processor = RailRoadProcessor
                },

                new ShapeFileDefinition
                {
                    Filename = @"ZIP_CODES.shp",
                    Processor = ZipProcessor
                },
                new ShapeFileDefinition
                {
                    Filename = @"ROADS_ALL.shp",
                    Processor = RoadProcessor
                },
                new ShapeFileDefinition
                {
                    Filename = @"TRANSIT_ROUTES_GTFS",
                    Processor = TransitRouteProcessor
                },
                new ShapeFileDefinition
                {
                    Filename = @"TRANSIT_STOPS_GTFS",
                    Processor = TransitStopProcessor
                },
            };

            return definitions;
        }

    }
}