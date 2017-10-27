using System;
using System.IO;
using Nest;
using Newtonsoft.Json;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Quest.Common.Messages;
using Quest.Lib.Trace;
using Quest.Common.Messages.Gazetteer;

namespace Quest.Lib.Search.Indexers
{
    /// <summary>
    /// www.heartsafe.org.uk/AED-Locations
    /// </summary>
    internal class DefibIndexer : ElasticIndexer
    {
        public string Filename { get; set; }

        public override void StartIndexing(BuildIndexSettings config)
        {
            Build(config);
        }

        private void Build(BuildIndexSettings config)
        {
            // radius of 50 meters around each defib
            double radius = 50;

            DeleteDataSet<LocationDocument>(config.DefaultIndex,config.Client, IndexBuilder.AddressDocumentType.Defib);

            var descriptor = GetBulkRequest(config);

            string text = File.ReadAllText(Filename);
            using (var reader = new StringReader(text))
            {
                JsonSerializer ser = new JsonSerializer();
                var result = ser.Deserialize(reader, typeof(Defibs));

                if (result == null)
                {
                    Logger.Write($"[1001] could not deserialise {Filename}", GetType().Name);
                    return;
                }

                Defibs defibs = result as Defibs;

                if (defibs == null)
                {
                    Logger.Write($"[1002] {Filename} does not contain JSON in defibs format", GetType().Name);
                    return;
                }

                if (defibs.places == null)
                {
                    Logger.Write($"[1002] {Filename} does not contain any defibs", GetType().Name);
                    return;
                }


                foreach (var defib in defibs.places)
                {
                    config.RecordsTotal++;
                    config.RecordsCurrent++;

                    if (defib.posn == null)
                    {
                        config.Errors++;
                        continue;
                    }

                    if (defib.posn.Length != 2)
                    {
                        config.Errors++;
                        continue;
                    }

                    var point = new GeoCoordinate(defib.posn[0], defib.posn[1]);
                    var location = new GeoLocation(defib.posn[0], defib.posn[1]);
                    
                    var terms = GetLocalAreas(config, point);

                    // check whether point is in master area if required
                    if (!IsPointInRange(config, point.Longitude, point.Latitude))
                    {
                        config.Skipped++;
                        continue;
                    }

                    // commit any messages and report progress
                    CommitCheck(this, config, descriptor);

                    var description = "Defib " + defib.address;

                    var address = new LocationDocument
                    {
                        Created = DateTime.Now,
                        Type = IndexBuilder.AddressDocumentType.Defib,
                        Source = "heartsafe",
                        ID = IndexBuilder.AddressDocumentType.Defib+ " "+defib.id,
                        Description = description,
                        indextext = Join(description, terms, false),
                        Location = location,
                        Point = PointfromGeoLocation(point),
                        //Areas = terms,
                        Status = "Approved",
                        Url = "http://www.heartsafe.org.uk",
                        Content = defib.desc,
                        // this will trigger this defib of searching in this footpront
                        InfoGeofence = GeomUtils.MakeEllipseWsg84(0, defib.posn[0], defib.posn[1], radius, radius),
                        InfoClasses = "info"
                    };

                    address.indextext = address.indextext.Replace("&", " and ");

                    // add item to the list of documents to index
                    AddIndexItem<LocationDocument>(address, descriptor);
                }

                // commit anything else
                CommitBultRequest(config, descriptor);

            }
        }
    }

#pragma warning disable 0649
    [Serializable]
    internal class Defibs
    {
        public Defib[] places;
    }

    [Serializable]
    internal class Defib
    {
        public string name;
        public string address;
        public string desc;
        public string icon;
        public double[] posn;
        public int zoom;
        public string id;
    }
#pragma warning restore 0649
}