#pragma warning disable 0169,649

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.IO;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Operation.Distance;
using Quest.Common.Messages;
using Quest.Lib.DataModel;
using Quest.Lib.Trace;
using Quest.Lib.Utils;

namespace Quest.Lib.Routing
{
    /// <summary>
    ///     Manages the loading of the road network
    /// </summary>
    [Serializable]
    public class RoutingData
    {
        /// <summary>
        ///     The <see cref="IsInitialised" /> property's name.
        /// </summary>
        public const string IsInitialisedPropertyName = "IsInitialised";

        private bool _isInitialised;

        /// <summary>
        ///     Index of road shapes
        /// </summary>
        public Quadtree<RoadEdge> ConnectionIndex = new Quadtree<RoadEdge>();

        public Dictionary<int, RoadEdge> Dict = new Dictionary<int, RoadEdge>();

        private readonly WKTReader _reader = new WKTReader();

        public Envelope Bounds;

        public RoutingData()
        {
            Bounds = new Envelope();

            // start a background thread to initialise the module
            var w = new Thread(StartdataLoader)
            {
                IsBackground = true,
                Name = "routing dataLoader"
            };
            w.Start();
        }

        /// <summary>
        ///     Sets and gets the IsInitialised property.
        ///     Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public bool IsInitialised
        {
            get { return _isInitialised; }

            set
            {
                // ReSharper disable once RedundantCheckBeforeAssignment
                if (_isInitialised == value)
                {
                    return;
                }

                _isInitialised = value;
            }
        }

        public void LoadRoadNetwork()
        {
            // repeat attempts at getting network data
            for (int i = 0; i < 5; i++)
            {
                var count = LoadRoadNetworkInternal();
                if (count != 0)
                {
                    IsInitialised = true;
                    return;
                }
            }
        }

        private int LoadRoadNetworkInternal()
        {
            string connection = "";
            try
            {
                Logger.Write($"Loading road network...", TraceEventType.Information, "Routing Data");
                using (var db = new QuestEntities())
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    db.Database.CommandTimeout = 60;
                    connection = db.Database.Connection.ConnectionString;

                    foreach (var current in db.RoadLinkEdges.AsNoTracking())
                    {
                        var geomAny = _reader.Read(current.WKT);
                        var geom = geomAny.GetGeometryN(0) as LineString;
                        var con = new RoadEdge(current.RoadLinkEdgeId, current.RoadLinkId, current.RoadName, current.RoadTypeId, geom, current.SourceGrade, current.TargetGrade);

                        // add into the quadtree
                        ConnectionIndex.Insert(con.Envelope, con);
                        Bounds.ExpandToInclude(con.Envelope);
                        Dict.Add(current.RoadLinkEdgeId,con);
                    }

                    // patch up outlinks
                    foreach (var link in db.RoadLinkEdgeLinks.AsNoTracking())
                    {
                        var src = Dict[link.SourceRoadLinkEdge];
                        var dst = Dict[link.TargetRoadLinkEdge];
                        src.OutEdges.Add(dst);
                    }

                    Logger.Write($"Loading road network complete - {Dict.Count} road links", TraceEventType.Information, "Routing Data");
                    return Dict.Count;
                }
            }
                // ReSharper disable once UnusedVariable
            catch(Exception ex)
            {
                Logger.Write($"Routing Data failed to load: {ex.Message}. Connection is {connection}", TraceEventType.Error, "Routing Data");
            }
            return 0;
        }

        private void StartdataLoader()
        {
            Logger.Write("Routing Data Initialising", TraceEventType.Information, "Routing Manager");
            LoadRoadNetwork();
            Logger.Write("Routing Data Initialised", TraceEventType.Information, "Routing Manager");
            IsInitialised = true;
        }

        public List<EdgeWithOffset> GetEdgesFromPoints(IEnumerable<Coordinate> coords, int epsg = 27700)
        {
            List<EdgeWithOffset> result = new List<EdgeWithOffset>();
            foreach (var c in coords)
                result.Add(GetEdgeFromPoint(c, epsg));
            return result;
        }

        /// <summary>
        ///     gets nearest edge to a point and sets that locations tag
        /// </summary>
        /// <returns></returns>
        public EdgeWithOffset GetEdgeFromPoint(Coordinate coord, int epsg = 27700)
        {
            if (epsg==4326)
            {
                // convert original coordinates
                var fc = LatLongConverter.WGS84ToOSRef(coord.Y, coord.X);
                coord.X = fc.Easting;
                coord.Y = fc.Northing;
            }

            var point = new Point(coord);

            IList<RoadEdge> nearbyRoadsCourse = null;

            var range = 10;
            do
            {
                var envelope = point.Buffer(range).EnvelopeInternal;

                // get a block of nearest roads
                nearbyRoadsCourse = ConnectionIndex.Query(envelope);
                range += 10;

            } while ((nearbyRoadsCourse == null || nearbyRoadsCourse.Count == 0) && range <= 500);

            if (nearbyRoadsCourse != null && nearbyRoadsCourse.Count == 0)
                return null; // no roads within the envelope

            // extract "MaxCandidates" nearest roads based on range to geometry from the fix and within "RoadGeometryRange"
            if (nearbyRoadsCourse == null)
                return null;
            
            var nearestRoadinOrder = nearbyRoadsCourse.OrderBy(x => DistanceOp.Distance(x.Geometry, point)).ToList();

            var nearestRoad = nearestRoadinOrder.FirstOrDefault();

            return GetCoordinateOnEdge(coord, nearestRoad);

        }

        /// <summary>
        /// compute a EdgeWithOffset object that estimates the offset along the road link given a coordinate
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        internal static EdgeWithOffset GetCoordinateOnEdge(Coordinate coord, RoadEdge edge)
        {
            var liL = new LengthIndexedLine(edge.Geometry);
            
            var offset = liL.Project(coord);

            var value = new EdgeWithOffset()
            {
                Edge = edge,
                Coord = liL.ExtractPoint(offset),
                Offset = liL.Project(coord)
            };

            var line = liL.ExtractLine(offset, offset + 1);

            // oops looks like the offset+1 goes beyond the end of the line.. take a step back instead..
            if (line.Length==0)
            {
                line = liL.ExtractLine(offset-1, offset);
            }

            value.AngleRadians = Math.Atan2(line.Coordinates[1].X - line.Coordinates[0].X, line.Coordinates[1].Y - line.Coordinates[0].Y) ;
            if (value.AngleRadians < 0)
                value.AngleRadians += (2*Math.PI);
            return value;
        }

    }
}