#pragma warning disable 0169
#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Quest.Lib.DataModel;
using Quest.Lib.Trace;
using Quest.Lib.Data;
using Quest.Lib.OS.DataModelOS;

namespace Quest.Lib.OS.Routing.ITN
{
    public class ItnRoutingData
    {
        
        public static void SaveItnRoadNetwork()
        {
            IEnumerable<RoadLinkEdgeTemp> network = LoadItnRoadNetwork();
            int i = 0;
            using (var db = new QuestContext())
            {
                db.Execute("truncate table RoadLinkEdgeLink");
                db.Execute("truncate table RoadLinkEdge");

                var sql = "";
                foreach (var edge in network)
                {
                    var sql1 =
                        $"INSERT[dbo].[RoadLinkEdge]([RoadLinkEdgeId], [RoadLinkId], [RoadName], [RoadTypeId], [SourceGrade], " +
                        $" [TargetGrade], [Length], [WKT], X, Y) VALUES({edge.RoadLinkEdgeId}, {edge.RoadLinkId}, '{edge.RoadName ?? ""}', {edge.RoadTypeId}, {edge.SourceGrade}, {edge.TargetGrade}, {(int) edge.Length}, '{edge.Geometry.AsText()}, '{edge.Geometry.StartPoint.X}, '{edge.Geometry.StartPoint.Y}');";

                    sql = sql + sql1;

                    foreach (var link in edge.Target.OutEdges)
                    {
                        var sql2 = $"INSERT[dbo].[RoadLinkEdgeLink]([SourceRoadLinkEdge], [TargetRoadLinkEdge]) VALUES({edge.RoadLinkEdgeId}, {link.RoadLinkEdgeId});";

                        sql = sql + sql2;
                        //db.Execute(sql2);
                    }

                    i++;
                    if (i%50 == 0)
                    {
                        db.Execute(sql);
                        sql = "";
                    }
                        //db.SaveChanges();
                }
                db.Execute(sql);
            }
            
        }

        /// <summary>
        ///     Loads routing data into memory. the data willbe available through the Locations array
        /// </summary>
        private static IEnumerable<RoadLinkEdgeTemp> LoadItnRoadNetwork()
        {
            var edges = new List<RoadLinkEdgeTemp>();
            try
            {

                var reader = new WKTReader();
                var linkId=0;
                using (var context = new QuestContext())
                {
                    var locs = LoadNodes();
                    var links = LoadLinks();

                    foreach (var current in links)
                    {
                        int fid = current.FromRoadNodeId ?? 0,
                            tid = current.ToRoadNodeId ?? 0;

                        var geomAny = reader.Read(current.Wkt);
                        var geom = geomAny.GetGeometryN(0) as LineString;

                        if (fid == 0 || tid == 0) continue;

                        RoutingLocation t = null;
                        if (locs.ContainsKey(tid)) t = locs[tid];

                        RoutingLocation f = null;
                        if (locs.ContainsKey(fid)) f = locs[fid];

                        if (f == null || t == null)
                        {
                            continue;
                        }

                        if (geom == null) continue;

                        if (current.RoadName == null)
                            current.RoadName = "";

                        current.RoadName = current.RoadName.Replace("'", "");

                        if (!(current.ToOneWay ?? false))
                        {
                            linkId++;
                            var con1 = new RoadLinkEdgeTemp(linkId, current.RoadLinkId, current.RoadName, current.RoadTypeId, geom,  t, current.FromGrade, current.ToGrade);
                            f.OutEdges.Add(con1);
                            edges.Add(con1);
                        }

                        if (current.FromOneWay ?? false) continue;

                        // deal with bidirectional roads by duplicating the roadlink, but give it a unique ID.
                        linkId++;
                        var con2 = new RoadLinkEdgeTemp(linkId, current.RoadLinkId, current.RoadName, current.RoadTypeId, (LineString)geom.Reverse(),  f, current.ToGrade, current.FromGrade);
                        t.OutEdges.Add(con2);
                        edges.Add(con2);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex.ToString(),"LoadItn");
            }

            return edges;
        }

        private static Dictionary<int, RoutingLocation> LoadNodes()
        {
            Logger.Write("Routing Data is loading the road nodes","LoadItn");
            var locs = new Dictionary<int, RoutingLocation>();
            StaticRoadNode[] nodes;

            using (var context = new QuestOSContext())
            {
                nodes = context.StaticRoadNode.ToArray();
            }

            foreach (var current in nodes)
            {
                var loc = new RoutingLocation(current.X , current.Y )
                {
                    Id = current.RoadNodeId,
                };
                locs.Add(loc.Id, loc);
            }
            Logger.Write("Routing Data has loaded the road nodes", "LoadItn");

            return locs;
        }

        private static StaticRoadLinks[] LoadLinks()
        {
            try
            {
                Logger.Write("Routing Data is loading the road links", "LoadItn");
                using (var context = new QuestOSContext())
                {
                    var links = context.StaticRoadLinks.ToArray();
                    Logger.Write("Routing Data has loaded the road links", "LoadItn");
                    return links;
                }

            }
            catch (Exception ex)
            {
                Logger.Write(ex.ToString(), "LoadItn");
                throw;
            }
        }


    }
}