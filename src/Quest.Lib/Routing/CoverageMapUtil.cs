using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Quest.Common.Messages;
using Quest.Lib.DataModel;
using Quest.Lib.Trace;
using Quest.Lib.Data;
using Quest.Lib.DependencyInjection;
using Quest.Common.Messages.Routing;

namespace Quest.Lib.Routing
{
    [Injection]
    public class CoverageMapManager
    {
        private CoverageMap _standardCoverage;
        private IGeometry _standardGeometry;
        private IDatabaseFactory _dbFactory;

        public CoverageMapManager(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public IGeometry GetStandardGeometry()
        {
            if (_standardGeometry == null)
                _standardGeometry = GetOperationalAreaInternal();
            return _standardGeometry;
        }

        public CoverageMap GetStandardMap(int tilesize)
        {
            if (_standardCoverage == null)
            {
                _standardGeometry = GetStandardGeometry();
                if (_standardGeometry != null)
                    _standardCoverage = MapFromGeometry("standard", _standardGeometry, tilesize);
            }
            return _standardCoverage;
        }

        public IGeometry GetOperationalAreaInternal()
        {
            Logger.Write("Calculating operational area", TraceEventType.Information, "CoverageMapUtil");
            for (var i = 1; i < 5; i++)
            {
                try
                {
                    return _dbFactory.Execute<QuestContext, IGeometry>((db) =>
                    {
                        // add in programmable ones.
                        var area = db.GetOperationalArea(2000);
                        // convert
                        var reader = new WKTReader();
                        var geoms = reader.Read(area.ToString());
                        Logger.Write($"Operational area is {geoms.Area} sq m", TraceEventType.Information, "CoverageMapUtil");
                        return geoms;
                    });
                }
                catch (Exception ex)
                {
                    Logger.Write($"Failed to calculate operational area: {ex.ToString()}", TraceEventType.Error, "CoverageMapUtil");
                }
            }
            return null;
        }

        public CoverageMap GetOperationalArea(int tilesize)
        {
            try
            {
                var map = GetStandardMap(tilesize);
                if (map != null)
                    return map.CalcCoverage(GetStandardGeometry());
            }
            catch (Exception ex)
            {
                Logger.Write($"Failed to calculate operational area: {ex}", TraceEventType.Error, "CoverageMapUtil");
            }
            return null;
        }

        public CoverageMap MapFromGeometry(string name, IGeometry geom, int tilesize)
        {
            var map = new CoverageMap();

            var e = new Envelope(
                Math.Floor(geom.EnvelopeInternal.MinX / 1000) * 1000,
                Math.Ceiling(geom.EnvelopeInternal.MaxX / 1000) * 1000,
                Math.Floor(geom.EnvelopeInternal.MinY / 1000) * 1000,
                Math.Ceiling(geom.EnvelopeInternal.MaxY / 1000) * 1000
                );

            map.SetExtent(name, e, tilesize);
            return map;
        }
    }

    public static class CoverageMapUtil
    { 
        /// <summary>
        ///     Creates a coverage map from a DBgeometry shape using a line-scan technique
        /// </summary>
        /// <param name="map">target map to update</param>
        /// <param name="geom">the source geometry</param>
        /// <returns></returns>
    public static CoverageMap CalcCoverage(this CoverageMap map, IGeometry geom)
        {
            try
            {
                for (var i = 0; i < map.Rows; i++)
                    for (var j = 0; j < map.Columns; j++)
                    {
                        var easting = j*map.Blocksize + map.OffsetX;
                        var northing = i*map.Blocksize + map.OffsetY;

                        var p = new Point(easting, northing);

                        if (geom.Contains(p))
                            map.Set(easting, northing, 1);
                    }
            }
            catch (Exception)
            {
                // ignored
            }
            return map;
        }

        public static void SetExtent(this CoverageMap c, string name, Envelope limits, int blocksize)
        {
            c.Name = name;
            c.Blocksize = blocksize;
            c.OffsetX = (int) limits.MinX;
            c.OffsetY = (int) limits.MinY;
            c.Rows = (int) ((limits.MaxY - c.OffsetY)/blocksize) + 1;
            c.Columns = (int) ((limits.MaxX - c.OffsetX)/blocksize) + 1;
            c.Data = new byte[c.Rows*c.Columns];
        }

        public static CoverageMap CreateEmptyCopy(CoverageMap c)
        {
            var copy = new CoverageMap();
            copy.Data = new byte[c.Data.Length];
            copy.Columns = c.Columns;
            copy.Rows = c.Rows;
            copy.OffsetX = c.OffsetX;
            copy.OffsetY = c.OffsetY;
            copy.Blocksize = c.Blocksize;
            copy.Name = c.Name;
            return copy;
        }

        public static CoverageMap CloneAsValue(this CoverageMap c, byte value)
        {
            var copy = new CoverageMap();

            if (c != null)
            {
                copy.Data = (byte[]) c.Data.Clone();
                copy.Columns = c.Columns;
                copy.Rows = c.Rows;
                copy.OffsetX = c.OffsetX;
                copy.OffsetY = c.OffsetY;
                copy.Blocksize = c.Blocksize;
                copy.Name = c.Name;
            }

            if (c != null) BufferUtil.CopyAsValue(c.Data, 0, copy.Data, 0, copy.Data.Length, value);

            return copy;
        }

        /// <summary>
        ///     merges source map into newMap setting each +ve value in source as 'value' in the newmap
        /// </summary>
        /// <param name="source"></param>
        /// <param name="newMap"></param>
        /// <param name="value"></param>
        public static void MergeAsValue(CoverageMap source, CoverageMap newMap, byte value)
        {
            BufferUtil.MergeAsValue(source.Data, 0, newMap.Data, 0, newMap.Data.Length, value);
        }

        /// <summary>
        ///     Merge the mergemap into the basemap. take only values from mergemap that are lower than in basemap.•
        /// </summary>
        /// <param name="basemap">The target map to be updated</param>
        /// <param name="mergemap">the map to me merged into thetarget map</param>
        public static void MergeMin(this CoverageMap basemap, CoverageMap mergemap)
        {
            BufferUtil.MergeMin(basemap.Data, mergemap.Data);
        }


        /// <summary>
        ///     take a base map and remove any entries where mapToSubtract > 0
        /// </summary>
        /// <param name="basemap"></param>
        /// <param name="mapTosubtract"></param>
        /// <returns></returns>
        public static CoverageMap DifferenceCoverage(CoverageMap basemap, CoverageMap mapTosubtract)
        {
            if (mapTosubtract == null)
                return null;

            for (var i = 0; i < mapTosubtract.Data.Length; i++)
            {
                int x, y;

                if (mapTosubtract.Data[i] > 0)
                {
                    mapTosubtract.GetCoord(i, out x, out y);
                    basemap.Set(x, y, 0);
                }
            }
            return basemap;
        }


        public static CoverageMap Name(this CoverageMap c, string name)
        {
            c.Name = name;
            return c;
        }

        public static CoverageMap Clone(this CoverageMap c)
        {
            var copy = new CoverageMap();

            if (c != null)
            {
                if (c.Data != null)
                    copy.Data = (byte[]) c.Data.Clone();
                copy.Columns = c.Columns;
                copy.Rows = c.Rows;
                copy.OffsetX = c.OffsetX;
                copy.OffsetY = c.OffsetY;
                copy.Blocksize = c.Blocksize;
                copy.Name = c.Name;
                copy.Percent = c.Percent;
            }
            return copy;
        }

        public static int Hits(this CoverageMap source)
        {
            return BufferUtil.CountNonZero(source.Data);
        }


        public static double Coverage(this CoverageMap source)
        {
            long d = BufferUtil.CountNonZero(source.Data);
            return d/(double) source.Data.Length;
        }

        /// <summary>
        ///     calculate the coverage in a map but only in the mask area
        /// </summary>
        /// <param name="source"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static double Coverage(this CoverageMap source, CoverageMap mask)
        {
            long maxhits = BufferUtil.CountNonZero(mask.Data);
            long hits = 0;

            // scan and find out which map item(s) to updates
            for (var i = 0; i < mask.Rows; i++)
            {
                var northing = i*mask.Blocksize + mask.OffsetY;

                for (var j = 0; j < mask.Columns; j++)
                {
                    var easting = j*mask.Blocksize + mask.OffsetX;

                    // this cell has a value.. update all map items if they are in this range
                    var m = mask.Value(easting, northing);
                    if (m > 0)
                    {
                        var t = source.Value(easting, northing);
                        if (t > 0)
                            hits++;
                    }
                }
            }

            return hits/(double) maxhits;
        }

        /// <summary>
        ///     add the source map into the target
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void Add(this CoverageMap target, CoverageMap source)
        {
            BufferUtil.Add(source.Data, 0, target.Data, 0, target.Data.Length);
        }

        public static bool IsDifferent(CoverageMap source, CoverageMap target)
        {
            return BufferUtil.IsDifferent(source.Data, target.Data);
        }

        /// <summary>
        ///     substract the source map from the target
        /// </summary>
        /// <param name="source">The source to subtract from the base</param>
        /// <param name="target">The base map to substract from</param>
        public static void Subtract(this CoverageMap target, CoverageMap source)
        {
            BufferUtil.Subtract(source.Data, 0, target.Data, 0, target.Data.Length);
        }

        public static void Move(CoverageMap before, CoverageMap after, CoverageMap target)
        {
            BufferUtil.Move(before.Data, after.Data, target.Data);
        }

        /// <summary>
        ///     get index into byte array of a coordinate
        /// </summary>
        /// <param name="c"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int GetIndex(this CoverageMap c, int x, int y)
        {
            var blockx = (x - c.OffsetX)/c.Blocksize;
            var blocky = (y - c.OffsetY)/c.Blocksize;

            if (blockx < 0)
                blockx = 0;

            if (blocky < 0)
                blocky = 0;

            if (blockx >= c.Columns)
                blockx = c.Columns - 1;

            if (blocky >= c.Rows)
                blocky = c.Rows - 1;

            var index = blockx + blocky*c.Columns;

            return index < c.Data.Length ? index : 0;
        }

        public static void GetCoord(this CoverageMap c, int index, out int x, out int y)
        {
            var cIndex = index%c.Columns;
            var rIndex = index/c.Columns;
            x = cIndex*c.Blocksize + c.OffsetX;
            y = rIndex*c.Blocksize + c.OffsetY;
        }

        /// <summary>
        ///     Adds heat to a coverage map
        /// </summary>
        /// <param name="c"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        public static void Add(CoverageMap c, int x, int y, byte value)
        {
            var i = GetIndex(c, x, y);
            var v = c.Data[i];
            if (v < byte.MaxValue && value > 0)
                c.Data[i] += value;
        }

        public static void Set(this CoverageMap c, int x, int y, byte value)
        {
            c.Data[GetIndex(c, x, y)] = value;
        }

        public static int Value(this CoverageMap c, int x, int y)
        {
            return c.Data[GetIndex(c, x, y)];
        }

        public static long Multiply(CoverageMap a, CoverageMap b)
        {
            return BufferUtil.Multiply(a.Data, b.Data);
        }

        public static long Sum(CoverageMap a)
        {
            return BufferUtil.Sum(a.Data);
        }

        public static CoverageMap ClearData(this CoverageMap a)
        {
            BufferUtil.Clear(a.Data);
            return a;
        }


        /// <summary>
        ///     output the coverage map in ArgGrid format
        /// </summary>
        /// <param name="map"></param>
        /// <param name="path"></param>
        public static void ExportArcGrid(CoverageMap map, string path)
        {
            if (path == null || path.Length == 0 || map == null || map.Data == null)
                return;

            try
            {
                using (TextWriter writer = new StreamWriter(Path.Combine(path, map.Name + ".ASC"), false))
                {
                    writer.WriteLine($"ncols           {map.Columns}");
                    writer.WriteLine($"nrows           {map.Rows}");
                    writer.WriteLine($"xllcorner       {map.OffsetX}");
                    writer.WriteLine($"yllcorner       {map.OffsetY}");
                    writer.WriteLine($"cellsize        {map.Blocksize}");
                    writer.WriteLine("NODATA_value    0");

                    for (var i = 0; i < map.Data.Length; i++)
                    {
                        int v = map.Data[i];
                        writer.Write($"{v}  ");
                        if ((i + 1)%18 == 0)
                            writer.WriteLine("\r");
                    }
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}