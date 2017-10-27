using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Quest.Common.Messages;
using Quest.Common.Messages.Gazetteer;

namespace Quest.Lib.Search.Indexers
{
    internal class LocalNameIndexer : ElasticIndexer
    {
        public string Filename { get; set; }

        public override void StartIndexing(BuildIndexSettings config)
        {
            Build(config);
        }

        private void Build(BuildIndexSettings config)
        {
            var descriptor = GetBulkRequest(config);

            config.RecordsTotal = config.LocalAreaNames.PolygonIndex.Count; 
            foreach (var r in config.LocalAreaNames.PolygonIndex.QueryAll())
            {
                config.RecordsCurrent++;

                // commit any messages and report progress
                CommitCheck(this, config, descriptor);

                // find additional items

                var locality = new List<string>();

                var idxItems = config.LocalAreaNames.ContainedWithin(r.geom);

                locality.Add(r.data[0] as string);

                if (idxItems.Count > 0)
                {
                    // idx_items.Reverse();
                    for (var j = 0; j < idxItems.Count; j++)
                        locality.Add(idxItems[j].data[0] as string);
                }
                locality = locality.Distinct().ToList();

                var additionalIndex = string.Join(", ", locality);

                var centre = r.geom.Centroid;
                var poly =  GeomUtils.MakePolygon(r.geom);
                var point = new GeoLocation(centre.Y, centre.X);

                var address = new LocationDocument
                {
                    Created = DateTime.Now,
                    Type = IndexBuilder.AddressDocumentType.LocalName,
                    Source = "OS",
                    ID = IndexBuilder.AddressDocumentType.LocalName + config.RecordsCurrent.ToString(),
                    Roadtype = "",
                    Description = additionalIndex.ToUpper(),
                    indextext = additionalIndex.ToUpper(),
                    Location = point,
                    Point = PointfromGeoLocation(point),
                    Thoroughfare = new List<string>(),
                    Locality = locality,
                    Poly = poly
                };

                // add item to the list of documents to index
                AddIndexItem<LocationDocument>(address, descriptor);

            }

            CommitBultRequest(config, descriptor);
           
        }
    }
}