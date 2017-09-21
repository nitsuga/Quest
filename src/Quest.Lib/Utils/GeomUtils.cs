using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using Nest;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Utilities;
//using ProjNet.CoordinateSystems;
//using ProjNet.CoordinateSystems.Transformations;
using Quest.Lib.Coords;
using Quest.Common.Messages;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
//using GeoAPI.Geometries;
//using GeoAPI.Geometries;

namespace Quest.Lib.Utils
{
    public static class GeomUtils
    {
        public static double Distance(float lat1, float lon1, float lat2, float lon2)
        {
            var p = 0.017453292519943295;    // Math.PI / 180
            var a = 0.5 - Math.Cos((lat2 - lat1) * p) / 2 +
                    Math.Cos(lat1 * p) * Math.Cos(lat2 * p) *
                    (1 - Math.Cos((lon2 - lon1) * p)) / 2;

            return 12742.0 * Math.Asin(Math.Sqrt(a)); // 2 * R; R = 6371 km
        }
        
        //public static Coordinate ConvertToCoordinate(this DbGeometry x)
        //{
        //    if (x.YCoordinate != null)
        //    {
        //        if (x.XCoordinate != null)
        //        {
        //            var c = new LatLng((double)x.YCoordinate, (double)x.XCoordinate).WGS84ToOSRef();

        //            return new Coordinate(c.Easting, c.Northing);
        //        }
        //    }
        //    return null;
        //}

        public static Coordinate ConvertToCoordinate(double lat, double lon)
        {
            var c = new LatLng(lat, lon).WGS84ToOSRef();
            return new Coordinate(c.Easting, c.Northing);
        }

        public static Coordinate ConvertToCoordinate(OSRef x)
        {
            var c = new LatLng((double)x.Northing, (double)x.Easting).WGS84ToOSRef();
            return new Coordinate(c.Easting, c.Northing);
        }

        public static string ConvertToGeometryString(OSRef x)
        {
            return string.Format(CultureInfo.InvariantCulture.NumberFormat, "POINT({0} {1})", x.Easting, x.Northing);
        }

        //public static Microsoft.Spatial.Geometry ConvertToDbGeometry(OSRef x)
        //{
        //    return Microsoft.Spatial.Geometry.PointFromText(ConvertToGeometryString(x), 27700);
        //}

        /// <summary>
        /// Create an ellipse in WGS84 coordinate system. 
        /// </summary>
        /// <param name="angle">Angle in degrees</param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="major">Semi-major radius in meters</param>
        /// <param name="minor">semi-minor radius in meters</param>
        /// <returns></returns>
        public static PolygonGeoShape MakeEllipseWsg84(double angle, double lat, double lon, double major, double minor)
        {
            var geomFact = new GeometryFactory();
            var csFact = new CoordinateSystemFactory();
            var ctFact = new CoordinateTransformationFactory();

            IGeographicCoordinateSystem wgs84 = GeographicCoordinateSystem.WGS84;
            var llCoords = new double[] { lon, lat };
            // use 1st longitude to determine UTM Zone
            var long2Utm = ((int)((llCoords[0] + 180) / 6)) + 1;
            ICoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(long2Utm, llCoords[1] > 0);

            // convert coords to UTM
            ICoordinateTransformation trans2Utm = ctFact.CreateFromCoordinateSystems(wgs84, utm);
            ICoordinateTransformation trans2Wgs84 = ctFact.CreateFromCoordinateSystems(utm, wgs84);

            var utmCoords = trans2Utm.MathTransform.Transform(llCoords);

            // create ellipse in local coordinates
            var factory = new GeometricShapeFactory();
            angle = angle * -1;
            factory.Centre = new Coordinate(utmCoords[1], utmCoords[0]);
            factory.Height = major;
            factory.Width = minor;
            factory.Rotation = angle / 180 * Math.PI;            
            var ellipse = factory.CeateEllipse();
            var poly = geomFact.CreatePolygon(new LinearRing(ellipse.Coordinates));

            //convert back to latlong
            var coords = ellipse.Coordinates.Select(x => new double[] { x.X, x.Y }).ToList();
            var coordsWsg84 = trans2Wgs84.MathTransform.TransformList(coords).ToList();

            var nestPoly = MakePolygon(poly);
            return nestPoly;
        }

        public static PolygonGeoShape MakeEllipseBng(double angle, double easting, double northing, double major, double minor)
        {
            var factory = new GeometricShapeFactory();

            angle = angle * -1;

            factory.Centre = new Coordinate(easting, northing);
            factory.Height = major;
            factory.Width = minor;
            factory.Rotation = angle / 180 * Math.PI;

            var ellipse = factory.CeateEllipse();

            //convert to latlong
            var coords = new List<Coordinate>();
            foreach (var p in ellipse.Coordinates)
            {
                var coord = LatLongConverter.OSRefToWGS84(p.X, p.Y);
                var en = new Coordinate { X = coord.Longitude, Y = coord.Latitude };
                coords.Add(en);
            }

            var fact = new GeometryFactory();
            var poly = fact.CreatePolygon(coords.ToArray());

            var nestPoly = MakePolygon(poly);
            return nestPoly;
        }

        public static MultiLineStringGeoShape GetMultiLine(string wkt, ICoordinateTransformation transformer)
        {
            MultiLineStringGeoShape shape = new MultiLineStringGeoShape();
            WKTReader reader = new WKTReader();
            var geom = reader.Read(wkt) as MultiLineString;

            if (geom == null)
                return null;

            return GetMultiLine(geom, transformer);
        }

        public static Point GetPointFromWkt(string wkt)
        {
            MultiLineStringGeoShape shape = new MultiLineStringGeoShape();
            WKTReader reader = new WKTReader();
            var geom = reader.Read(wkt) as Point;
            return geom;
        }

        public static MultiLineStringGeoShape GetMultiLine(IGeometry geom, ICoordinateTransformation transformer)
        {
            try
            {
                var listofpoints = new List<List<GeoCoordinate>>();

                for (var g = 0; g < geom.NumGeometries; g++)
                {
                    var subgeom = geom.GetGeometryN(g);
                    var numpoints = subgeom.NumPoints;
                    var points = new List<GeoCoordinate>();

                    for (var i = 0; i < numpoints; i++)
                    {
                        var coord = subgeom.Coordinates[i];
                        if (transformer != null)
                            coord = transformer.MathTransform.Transform(coord);
                        points.Add(new GeoCoordinate(coord.Y, coord.X));
                    }
                    listofpoints.Add(points);
                }

                MultiLineStringGeoShape result = new MultiLineStringGeoShape();
                result.Coordinates = listofpoints;
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public static PolygonGeoShape MakePolygon(IGeometry geom)
        {
            try
            {
                var p = geom as Polygon;
                if (p == null)
                    return null;

                var numpoints = geom.NumPoints;

                var close = p.Coordinates[0].X != p.Coordinates[numpoints - 1].X ||
                            p.Coordinates[0].Y != p.Coordinates[numpoints - 1].Y
                    ? 1
                    : 0;

                var coords = new double[numpoints + close][];
                var points = new List<GeoCoordinate>();
                var listofpoints = new List<List<GeoCoordinate>>();

                for (var i = 0; i < numpoints; i++)
                {
                    var x = p.Coordinates[i].X;
                    var y = p.Coordinates[i].Y;
                    points.Add(new GeoCoordinate(y, x));
                }

                // add 1st point as last to complete the polygon
                if (close == 1)
                {
                    points.Add(new GeoCoordinate(points[0].Latitude, points[0].Longitude));
                }
                listofpoints.Add(points);

                return new PolygonGeoShape(listofpoints);
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public static PolygonGeoShape GetPolygon(IGeometry geom, ICoordinateTransformation transformer)
        {
            try
            {
                var listofpoints = new List<List<GeoCoordinate>>();

                // only take the first (outer) polygon
                var subgeom = geom.GetGeometryN(0);
                var numpoints = subgeom.NumPoints;
                var points = new List<GeoCoordinate>();

                for (var i = 0; i < numpoints; i++)
                {
                    var coord = subgeom.Coordinates[i];
                    if (transformer != null)
                        coord = transformer.MathTransform.Transform(coord);
                    points.Add(new GeoCoordinate(coord.Y, coord.X));
                }

                var close = points[0].Longitude != points[numpoints - 1].Longitude ||
                            points[0].Latitude != points[numpoints - 1].Latitude
                            ? 1 : 0;

                // add 1st point as last to complete the polygon
                if (close == 1)
                {
                    points.Add(new GeoCoordinate(points[0].Latitude, points[0].Longitude));
                }

                listofpoints.Add(points);

                PolygonGeoShape result = new PolygonGeoShape { Coordinates = listofpoints };
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        //public static List<List<GeoCoordinate>> GetMultiLine(DbGeometry geom)
        //{
        //    try
        //    {
        //        var numpoints = geom.PointCount ?? 0;
        //        var points = new List<GeoCoordinate>();
        //        var listofpoints = new List<List<GeoCoordinate>>();

        //        for (var i = 1; i <= numpoints; i++)
        //        {
        //            var x = geom.PointAt(i).XCoordinate;
        //            var y = geom.PointAt(i).YCoordinate;

        //            // convert from 27700 to WGS84
        //            if (x != null)
        //            {
        //                if (y != null)
        //                {
        //                    var latlng = LatLongConverter.OSRefToWGS84((double) x, (double) y);
        //                    points.Add(new GeoCoordinate(latlng.Latitude, latlng.Longitude));
        //                }
        //            }
        //        }
        //        listofpoints.Add(points);
        //        return listofpoints;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error", ex);
        //    }
        //}

        public static GeoLocation ConvertToLatLonLoc(double easting, double northing)
        {
            var coord = LatLongConverter.OSRefToWGS84(easting, northing);
            return new GeoLocation(coord.Latitude, coord.Longitude);
        }

        public static GeoLocation ConvertFromCoordinate(Coordinate coord)
        {
            return new GeoLocation(coord.Y, coord.X);
        }

        //public static PolygonGeoShape MakePolygon(DbGeometry geom)
        //{
        //    var result = GetPolygon(geom);

        //    var shape = new PolygonGeoShape {Coordinates = result};

        //    return shape;
        //}

        //private static List<List<GeoCoordinate>> GetPolygon(DbGeometry geom)
        //{
        //    try
        //    {
        //        var numpoints = geom.PointCount ?? 0;
        //        var points = new List<GeoCoordinate>();
        //        var listofpoints = new List<List<GeoCoordinate>>();

        //        for (var i = 1; i <= numpoints; i++)
        //        {
        //            var x = geom.PointAt(i).XCoordinate;
        //            var y = geom.PointAt(i).YCoordinate;

        //            // convert from 27700 to WGS84
        //            if (x != null)
        //            {
        //                if (y != null)
        //                {
        //                    var latlng = LatLongConverter.OSRefToWGS84((double) x, (double) y);
        //                    points.Add(new GeoCoordinate(latlng.Latitude, latlng.Longitude));
        //                }
        //            }
        //        }

        //        // add 1st point as last to complete the polygon
        //        var lastPoint = points.Last();
        //        if (lastPoint.Latitude != points[0].Latitude || lastPoint.Longitude != points[0].Longitude)
        //        {
        //            var newlastPoint = new GeoCoordinate(points[0].Latitude, points[0].Longitude);
        //            points.Add(newlastPoint);
        //        }

        //        listofpoints.Add(points);
        //        return listofpoints;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error", ex);
        //    }
        //}

        public static PolygonizeResult Polygonize(SearchResponse results, double range)
        {
            CoordinateTransformationFactory ctFact = new CoordinateTransformationFactory();
            IGeographicCoordinateSystem wgs84 = GeographicCoordinateSystem.WGS84;

            var result = new PolygonizeResult();

            if (results.Count == 0)
                return result;

            var fact = new GeometryFactory();

            // convert points to BNG
            var coords = results.Documents.Select(doc => new Coordinate { X = doc.l.Location.Longitude, Y = doc.l.Location.Latitude }).ToList();

            // also add polygons
            var polys = results.Documents
                .Where(x => x.l.Poly != null)
                .SelectMany(doc => doc.l.Poly.Coordinates)
                .SelectMany(c1 => c1)
                .Select(doc => new Coordinate { X = doc.Longitude, Y = doc.Latitude })
                .ToList();

            coords.AddRange(polys);

            // use 1st longitude to determine UTM Zone
            var long2Utm = ((int)((coords[0].X + 180) / 6)) + 1;
            ICoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(long2Utm, coords[0].Y > 0);

            // convert coords to UTM
            ICoordinateTransformation trans2Utm = ctFact.CreateFromCoordinateSystems(wgs84, utm);
            ICoordinateTransformation trans2Wgs84 = ctFact.CreateFromCoordinateSystems(utm, wgs84);

            var utmCoords = trans2Utm.MathTransform.TransformList(coords);
            var multiPoint = fact.CreateMultiPoint(utmCoords.ToArray());
            var hullGeom = multiPoint.ConvexHull();

            // buffer the geom
            hullGeom = hullGeom.Buffer(range);

            result.centroid = trans2Wgs84.MathTransform.Transform(new[] { hullGeom.Centroid.X, hullGeom.Centroid.Y });

            // now convert back to WGS84
            var wgs84Coords = trans2Wgs84.MathTransform.TransformList(hullGeom.Coordinates);

            var poly = fact.CreatePolygon(wgs84Coords.ToArray());
            result.nts_polygon = poly;
            result.nest_polygon = MakePolygon(poly);

            return result;
        }

        public static IPolygon MakePolygonFromBng(IGeometry geom)
        {
            var factory = new GeometricShapeFactory();

            var fact = new GeometryFactory();
            var coords = new List<Coordinate>();

            // convert points to Lat Lon
            foreach (var coordBng in geom.Coordinates)
            {
                var coord = LatLongConverter.OSRefToWGS84(coordBng.X, coordBng.Y);
                var en = new Coordinate { X = coord.Longitude, Y = coord.Latitude };
                coords.Add(en);
            }

            var close = coords[0].X != coords[coords.Count - 1].X || coords[0].Y != coords[coords.Count - 1].Y;

            if (close)
            {
                coords.Add(coords[0]);
            }

            var poly = fact.CreatePolygon(coords.ToArray());

            return poly;
        }

        public struct PolygonizeResult
        {
            public PolygonGeoShape nest_polygon;
            public IPolygon nts_polygon;
            public GeoLocation centroid;
        }
    }
}