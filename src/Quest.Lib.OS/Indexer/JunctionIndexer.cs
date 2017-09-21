using System;
using System.Collections.Generic;
using System.Linq;
using Quest.Lib.OS.Model;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Quest.Common.Messages;

namespace Quest.Lib.OS.Indexer
{
    internal class JunctionIndexer : ElasticIndexer
    {

        public override void StartIndexing(BuildIndexSettings config)
        {
            Build(config);
        }

        private void Build(BuildIndexSettings config)
        {
            DeleteDataSet<LocationDocument>(config.DefaultIndex, config.Client, IndexBuilder.AddressDocumentType.Junction);

            using (var db = new QuestOSEntities())
            {
                var descriptor = GetBulkRequest(config);
                var total = db.Junctions.Count();
                config.RecordsTotal = total;

                foreach (var r in db.Junctions)
                {
                    config.RecordsCurrent++;

                    var point = GeomUtils.ConvertToLatLonLoc(r.X ?? 0,
                        r.Y ?? 0);

                    // check whether point is in master area if required
                    if (!IsPointInRange(config, point.Longitude, point.Latitude))
                    {
                        config.Skipped++;
                        continue;
                    }

                    var terms = GetLocalAreas(config, point);

                    // commit any messages and report progress
                    CommitCheck(this, config, descriptor);

                    var description = r.R1 + " / " + r.R2;

                    var address = new LocationDocument
                    {
                        Created = DateTime.Now,
                        Type = IndexBuilder.AddressDocumentType.Junction,
                        Source = "Extent",
                        ID = IndexBuilder.AddressDocumentType.Junction + r.JunctionID,
                        indextext = Join(description, terms, false),
                        Description = Join(description, terms, true),
                        Location = point,
                        Point = PointfromGeoLocation(point),
                        Thoroughfare = new List<string>(),
                        Locality = new List<string>(),
                        Areas = terms,
                        Status = "Approved"
                    };

                    address.Thoroughfare.Add(r.R1.ToUpper());
                    address.Thoroughfare.Add(r.R2.ToUpper());

                    // add to the list of stuff to index
                    address.indextext = address.indextext.Replace("&", " and ");

                    // add item to the list of documents to index
                    AddIndexItem<LocationDocument>(address, descriptor);
                }

                // commit anything else
                CommitBultRequest(config, descriptor);
            }
        }

    }
}