using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeoAPI.Geometries;
using Nest;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.IO;
using Quest.Lib.Trace;
using Newtonsoft.Json;
using System.IO;
using CsvHelper;

namespace Quest.Lib.Utils
{
    public class PolygonManager
    {
        public Quadtree<PolygonData> PolygonIndex;

        public List<PolygonData> Search(Coordinate coord)
        {
            return Search(coord.X, coord.Y);
        }
        public List<PolygonData> Search(GeoLocation coord)
        {
            return Search(coord.Longitude, coord.Latitude);
        }

        public List<PolygonData> Search(PointGeoShape coord)
        {
            return Search(coord.Coordinates.Longitude, coord.Coordinates.Latitude);
        }

        /// <summary>
        ///     find a list of polygons that contain the given point
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public List<PolygonData> Search(double X, double Y)
        {
            var coord = new Coordinate(X, Y);
            var p = new Point(coord);
            p.SRID = 4326;
            var envelope = new Envelope(coord);
            var items = PolygonIndex.Query(envelope);
            var all2 = items.Where(x => x.geom.Contains(p)).ToList();
            return all2;
        }

        /// <summary>
        ///     returns a list of parent containers
        /// </summary>
        /// <param name="shape"></param>
        /// <returns></returns>
        public List<PolygonData> ContainedWithin(IGeometry shape)
        {
            var envelope = shape.EnvelopeInternal;
            var items = PolygonIndex.Query(envelope);
            var all2 = items.Where(x => x.geom.Contains(shape)).ToList();
            return all2;
        }

        public void BuildFromShapefile(string filename)
        {
            Debug.Print("Building polygon index");
            IGeometryFactory geomFact = new GeometryFactory();

            PolygonIndex = new Quadtree<PolygonData>();
            var dir = System.IO.Directory.GetCurrentDirectory();

            using (var reader = new ShapefileDataReader(filename, geomFact))
            {
                while (reader.Read())
                {
                    List<String> data2 = new List<String>();
                    for (int i = 0; i < reader.FieldCount - 1; i++)
                    {
                        var v = reader.GetValue(i);
                        data2.Add(v?.ToString() ?? "");
                    }

                    //reader.GetValues(data);
                    var geom = reader.Geometry;

                    var polydata = new PolygonData { data = data2.ToArray(), geom = geom };
                    geom.SRID = 4326;

                    // add to the index
                    PolygonIndex.Insert(geom.EnvelopeInternal, polydata);
                }
            }
        }

     
        public void BuildFromJson(string filename, int geomColumn, int srid = 4326)
        {
            Logger.Write($"Building polygon index from {filename} srid={srid}", GetType().Name);

            IGeometryFactory geomFact = new GeometryFactory();

            JsonSerializer ser = new JsonSerializer();

            var fileReader = new WKTReader(geomFact);

            try
            {
                var allText = System.IO.File.ReadAllText(filename);
                var data = JsonConvert.DeserializeObject<String[][]>(allText);
                PolygonIndex = new Quadtree<PolygonData>();

                foreach (var entry in data)
                {
                    try
                    {
                        var geom = fileReader.Read(entry[geomColumn]);
                        var polydata = new PolygonData { data = entry, geom = geom };
                        geom.SRID = srid;
                        // add to the index
                        PolygonIndex.Insert(geom.EnvelopeInternal, polydata);
                    }
                    catch (Exception ex)
                    {
                        Logger.Write($"Polygon index failed: {ex}", GetType().Name);
                    }
                }

                Logger.Write("Polygon index built", GetType().Name);
            }
            catch (Exception ex)
            {
                Logger.Write($"Polygon index failed: {ex}", GetType().Name);
            }
        }
    }
}