using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Runtime.Serialization;
using System.Diagnostics;
using MessageBroker.Objects;
using System.Data.Entity.Spatial;
using GeoAPI.Geometries;

namespace Quest.Lib.Routing
{

    public static class CoverageMapUtil 
    {
        /// <summary>
        /// Creates a coverage map from a DBgeometry shape using a line-scan technique
        /// </summary>
        /// <param name="map">target map to update</param>
        /// <param name="geom">the source geometry</param>
        /// <returns></returns>
        public static CoverageMap CalcGeometryCoverage(this CoverageMap map, DbGeometry geom)
        {
            NetTopologySuite.IO.WKTReader reader = new NetTopologySuite.IO.WKTReader();
            IGeometry geoms = reader.Read(geom.ToString());

            for (int i = 0; i < map.Rows; i++)
                for (int j = 0; j < map.Columns; j++)
                {
                    int easting = (j * map.Blocksize) + map.OffsetX;
                    int northing = (i * map.Blocksize) + map.OffsetY;

                    NetTopologySuite.Geometries.Point p = new NetTopologySuite.Geometries.Point(easting, northing);

                    if (geoms.Contains(p))
                        map.Set(easting, northing, 1);
                }
            return map;
        }

        public static void SetExtent(this CoverageMap c, String Name, RTree.Rectangle limits, int blocksize)
        {
            c.Name = Name;
            c.Blocksize = blocksize;
            c.OffsetX = (int)limits.min[0];
            c.OffsetY = (int)limits.min[1];
            c.Rows =    (int)((limits.max[1] - c.OffsetY) / blocksize) + 1;
            c.Columns = (int)((limits.max[0] - c.OffsetX) / blocksize) + 1;
            c.Data = new byte[c.Rows * c.Columns];
        }

        public static CoverageMap CreateEmptyCopy(CoverageMap c)
        {
            CoverageMap copy = new CoverageMap();
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
            CoverageMap copy = new CoverageMap();

            if (c != null)
            {
                copy.Data = (byte[])c.Data.Clone();
                copy.Columns = c.Columns;
                copy.Rows = c.Rows;
                copy.OffsetX = c.OffsetX;
                copy.OffsetY = c.OffsetY;
                copy.Blocksize = c.Blocksize;
                copy.Name = c.Name;
            }

            BufferUtil.CopyAsValue(c.Data, 0, copy.Data, 0, copy.Data.Count(), value);

            return copy;
        }

        /// <summary>
        /// merges source map into newMap setting each +ve value in source as 'value' in the newmap
        /// </summary>
        /// <param name="source"></param>
        /// <param name="newMap"></param>
        /// <param name="value"></param>
        public static void MergeAsValue(CoverageMap source, CoverageMap newMap, byte value)
        {
            BufferUtil.MergeAsValue(source.Data, 0, newMap.Data, 0, newMap.Data.Count(), value);
        }


        public static CoverageMap DifferenceCoverage(CoverageMap basemap, CoverageMap mapTosubtract)
        {
            for (int i = 0; i < mapTosubtract.Data.Count(); i++)
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


        public static CoverageMap Clone(this CoverageMap c)
        {
            CoverageMap copy = new CoverageMap();

            if (c != null)
            {
                if (c.Data!=null)
                    copy.Data = (byte[])c.Data.Clone();
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


        public static double Coverage(CoverageMap source)
        {
            long d = BufferUtil.CountNonZero(source.Data);
            return (double)d / (double)source.Data.Length;
        }

        /// <summary>
        /// calculate the coverage in a map but only in the mask area
        /// </summary>
        /// <param name="source"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static double Coverage(this CoverageMap source, CoverageMap mask)
        {
            long maxhits = BufferUtil.CountNonZero(mask.Data);
            long hits = 0;

            // scan and find out which map item(s) to updates
            for (int i = 0; i < mask.Rows; i++)
            {
                int northing = (i * mask.Blocksize) + mask.OffsetY;

                for (int j = 0; j < mask.Columns; j++)
                {
                    int easting = (j * mask.Blocksize) + mask.OffsetX;

                    // this cell has a value.. update all map items if they are in this range
                    int m = mask.Value(easting, northing);
                    if (m > 0)
                    {
                        int t = source.Value(easting, northing);
                        if (t > 0)
                            hits++;
                    }
                }
            }

            return (double)hits / (double)maxhits;
        }


        /// <summary>
        /// add the source map into the target 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void Add(this CoverageMap target, CoverageMap source)
        {
            BufferUtil.Add(source.Data, 0, target.Data, 0, target.Data.Length);
        }

        public static bool IsDifferent(CoverageMap source, CoverageMap target)
        {
            return BufferUtil.IsDifferent(source.Data, target.Data );
        }



        /// <summary>
        /// substract the source map from the target
        /// </summary>
        /// <param name="source">The source to subtract from the base</param>
        /// <param name="target">The base map to substract from</param>
        public static void Subtract(this CoverageMap target, CoverageMap source)
        {
            BufferUtil.Subtract(source.Data, 0, target.Data, 0, target.Data.Count());
        }

        public static void Move(CoverageMap before, CoverageMap after, CoverageMap target)
        {
            BufferUtil.Move(before.Data, after.Data, target.Data);
        }

        /// <summary>
        /// merges location hits to the target bitmap
        /// </summary>
        /// <param name="c"></param>
        /// <param name="locs"></param>
        public static void ProcessLocations(CoverageMap c, Dictionary<int, RoutingLocation> locs)
        {
            // create a data array or 0's and 1's and then merge it with the coverage map.
            byte[] data = new byte[c.Data.Length];
            
            foreach (RoutingLocation l in locs.Values)
                if (l.Processed)
                    data[GetIndex(c, l.Point.X, l.Point.Y)] = 1;

            BufferUtil.Add(data, 0, c.Data, 0, data.Length);
        }

        /// <summary>
        /// get index into byte array of a coordinate
        /// </summary>
        /// <param name="c"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int GetIndex(this CoverageMap c, int x, int y)
        {
            int blockx = (x - c.OffsetX) / c.Blocksize;
            int blocky = (y - c.OffsetY) / c.Blocksize;

            if (blockx<0) 
                blockx=0;

            if (blocky<0) 
                blocky=0;
            
            if (blockx>=c.Columns) 
                blockx=c.Columns-1;

            if (blocky>=c.Rows) 
                blocky=c.Rows-1;

            int index= blockx + (blocky * c.Columns);

            return index < c.Data.Length ? index : 0;
        }

        public static void GetCoord(this CoverageMap c, int index, out int x, out int y)
        {
            int c_index = index % c.Columns;
            int r_index = index / c.Columns;
            x = c_index * c.Blocksize + c.OffsetX;
            y = r_index * c.Blocksize + c.OffsetY;
        }

        /// <summary>
        /// Adds heat to a coverage map 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        public static void Add(CoverageMap c, int x, int y, byte value)
        {
            int i = GetIndex(c, x, y);
            byte v = c.Data[i];
            if ( v< byte.MaxValue && value > 0) 
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

        public static CoverageMap Clear(this CoverageMap a)
        {
            BufferUtil.Clear(a.Data);
            return a;
        }


        /// <summary>
        /// output the coverage map in ArgGrid format
        /// </summary>
        /// <param name="map"></param>
        public static void ExportArcGrid(CoverageMap map, string path)
        {
            if (path == null || path.Length == 0 || map == null || map.Data == null)
                return;

            try
            {
                using (System.IO.TextWriter writer = new System.IO.StreamWriter(System.IO.Path.Combine(path, map.Name + ".ASC"), false))
                {
                    writer.WriteLine(String.Format("ncols           {0}", map.Columns));
                    writer.WriteLine(String.Format("nrows           {0}", map.Rows));
                    writer.WriteLine(String.Format("xllcorner       {0}", map.OffsetX));
                    writer.WriteLine(String.Format("yllcorner       {0}", map.OffsetY));
                    writer.WriteLine(String.Format("cellsize        {0}", map.Blocksize));
                    writer.WriteLine(String.Format("NODATA_value    0"));

                    for (int i = 0; i < map.Data.Length; i++)
                    {
                        int v = map.Data[i];
                        writer.Write(String.Format("{0}  ", v));
                        if ((i + 1) % 18 == 0)
                            writer.WriteLine("\r");
                    }
                }
            }
            catch 
            {

            }

        }

    }
}
