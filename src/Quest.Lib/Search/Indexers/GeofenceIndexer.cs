using System;
using Nest;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Quest.Common.Messages.Gazetteer;

namespace Quest.Lib.Search.Indexers
{
    internal class GeofenceIndexer : ElasticIndexer
    {
        public String Filenames { get; set; }

        public override void StartIndexing(BuildIndexSettings config)
        {
            Build(config);
        }

        private void Build(BuildIndexSettings config)
        {
            var geofences = new PolygonManager();

            var files = Filenames.Split(',');
            foreach (var file in files)
            {
                geofences.BuildFromShapefile(file);

                CreateGeofenceIndex(config);

                var descriptor = GetBulkRequest(ElasticSettings.GeofenceIndex);
                var i = 0;
                foreach (var r in geofences.PolygonIndex.QueryAll())
                {
                    i++;

                    // commit any messages and report progress
                    CommitCheck(this, config, descriptor);

                    var poly = GeomUtils.MakePolygon(r.geom);

                    var address = new GeofenceDocument
                    {
                        ID = Guid.NewGuid().ToString("N"),
                        TriggerId = r.data[1],
                        Description = r.data[2],
                        Notes = r.data[3],
                        Author = r.data[4],
                        Org = r.data[5],
                        Claims = r.data[6],
                        Type = r.data[7],
                        Created = GetDate(r.data[8]),
                        Review = GetDate(r.data[9]),
                        Uprn = r.data[10],
                        Usrn = r.data[11],
                        ValidFrom = GetDate(r.data[12]),
                        ValidTo = GetDate(r.data[13]),
                        PolyGeometry = poly
                    };
                    descriptor.Operations.Add(new BulkIndexOperation<GeofenceDocument>(address));
                }

                CommitBultRequest(config, descriptor);
            }
        }

        DateTime? GetDate(string text)
        {
            DateTime dt;
            var worked = DateTime.TryParse(text, out dt);
            return worked?dt:(DateTime?)null;
        }

        private void CreateGeofenceIndex(BuildIndexSettings config)
        {
            if (config.Client.IndexExists(ElasticSettings.GeofenceIndex).Exists)
                config.Client.DeleteIndex(ElasticSettings.GeofenceIndex);

            var indexState = new IndexState {Settings = new IndexSettings {NumberOfShards = 1, NumberOfReplicas = 1}};
            var result = config.Client.CreateIndex(ElasticSettings.GeofenceIndex, s => s
                .InitializeUsing(indexState)
                .Mappings(ms => ms
                    .Map<GeofenceDocument>(m => m
                        .AutoMap()
                        .Properties(
                            ps =>
                                ps.GeoShape(
                                    g => g.Name("polyGeometry").Precision(1, DistanceUnit.Meters).Tree(GeoTree.Quadtree)))
                    //#CHANGE# changed to DistanceUnit from GeoPrecisionUnit                                       
                    )
                ));
            //catch errors with index creation
            if (!result.IsValid)
                throw new Exception(result.DebugInformation + "\n" + result.ServerError);
        }
    }
}