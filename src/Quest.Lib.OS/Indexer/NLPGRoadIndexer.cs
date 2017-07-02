#if false
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Quest.Lib.DataModel;
using Quest.Lib.Search.Elastic;

namespace Quest.Lib.Search.Indexers
{
    internal class NlpgRoadIndexer : ElasticIndexer
    {
        internal override void StartIndexing(BuildIndexSettings config)
        {
            Build(config);
        }

        private void Build(BuildIndexSettings config)
        {
            using (var db = new QuestEntities())
            {
                var i = 0;
                ((IObjectContextAdapter) db).ObjectContext.CommandTimeout = 360;
                var total = db.vw_NLPG_ROAD.Count();

                db.Configuration.ProxyCreationEnabled = false;
                var descriptor = GetBulkRequest(config);

                foreach (var r in db.vw_NLPG_ROAD.AsNoTracking()) //#change NLPGroads
                {
                    var point = Elastic.Utils.ConvertToLatLonLoc(r.STREET_START_X, r.STREET_START_Y);

                    // check whether point is in master area if required
                    if (!IsPointInRange(config, point.Longitude, point.Latitude))
                    {
                        config.Skipped++;
                        continue;
                    }


                    var dlist = new List<string>();

                    if (r.LOCALITY_NAME != null)
                        dlist.Add(r.LOCALITY_NAME);

                    if (r.TOWN_NAME != null)
                        dlist.Add(r.TOWN_NAME);

                    if (r.ADMINSTRATIVE_AREA != null)
                    {
                        dlist.Add(r.ADMINSTRATIVE_AREA.Replace("LONDON BOROUGH OF ", ""));
                    }

                    i++;
                    if (i%config.Logfrequency == 0)
                    {
                        config.StatusCallback?.Invoke($"{this.GetType().Name}: {i}/{total}");

                        CommitBultRequest(config, descriptor);

                    }


                    var name = r.STREET_DESCRIPTION + ", " + string.Join(", ", dlist);

                    dlist.AddRange(GetLocalAreas(config, point));
                    var areas = dlist.Distinct().ToList();
                    var areanames = string.Join(" ", areas);

                    var address = new AddressDocument
                    {
                        Created = DateTime.Now,
                        Type = IndexBuilder.AddressDocumentType.RoadPoint,
                        Source = "NLPG",
                        ID = "nrd " + r.USRN,
                        BuildingName = "",
                        Description = name,
                        indextext = r.STREET_DESCRIPTION + " " + areanames,
                        Location = point,
                        Point = PointfromGeoLocation(point),
                        Organisation = "",
                        Postcode = "",
                        SubBuilding = "",
                        Thoroughfare = new List<string> {r.STREET_DESCRIPTION.ToUpper()},
                        Locality = new List<string> {r.LOCALITY_NAME ?? "".ToUpper()},
                        Areas = areas.ToArray(),
                        Status = "Approved",
                        USRN = r.USRN,
                        Classification = "CT99" // Road
                    };


                    // add to the list of stuff to index
                    address.indextext = address.indextext.Replace("&", " and ");

                    // add item to the list of documents to index
                    AddIndexItem(address, descriptor);
                }

                // commit anything else
                CommitBultRequest(config, descriptor);
            }

        }


    }
}
#endif
