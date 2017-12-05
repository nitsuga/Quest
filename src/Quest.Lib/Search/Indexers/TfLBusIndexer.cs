using System;
using System.Collections.Generic;
using System.Linq;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using System.IO;
using Quest.Lib.Csv;
using Quest.Common.Messages.Gazetteer;
using Quest.Lib.Coords;

namespace Quest.Lib.Search.Indexers
{
    internal class TfLBusIndexer : ElasticIndexer
    {
        public string Filename { get; set; } = "";

        public override void StartIndexing(BuildIndexSettings config)
        {

            BuildBusStops(Filename, config);
        }

        private void BuildBusStops(object filename, BuildIndexSettings config)
        {
            // throw away header line
            using (StreamReader reader = File.OpenText(Filename))
            {
                var descriptor = GetBulkRequest(config);

                foreach (var data in CsvReader.Read(reader, new CsvOptions { RowsToSkip = 0, Separator = ',' }))
                {
                    config.RecordsCurrent++;
                    config.RecordsTotal++;

                    if (data == null)
                        continue;

                    var bus = data[0];
                    var run = data[1];
                    var seq = data[2];
                    var code = data[4];
                    var stopname = data[6].Replace("#", "");
                    var easting = data[7];
                    var northing = data[8];

                    double e, n;

                    double.TryParse(easting, out e);
                    double.TryParse(northing, out n);

                    var point = GeomUtils.ConvertToLatLonLoc(e, n);

                    // commit any messages and report progress
                    CommitCheck(this, config, descriptor);

                    // check whether point is in master area if required
                    if (!IsPointInRange(config, point.Longitude, point.Latitude))
                    {
                        config.Skipped++;
                        continue;
                    }

                    var terms = GetLocalAreas(point, config.LocalAreaNames);

                    //if (terms.Length > 0)
                    {
                        var description = "bus stop " + bus + " " + stopname;
                        description = description.ToUpper();

                        var address = new LocationDocument
                        {
                            Created = DateTime.Now,
                            Type = IndexBuilder.AddressDocumentType.Bus,
                            Source = "TFL",
                            ID = IndexBuilder.AddressDocumentType.Bus + " " + bus + " " + run + "/" + seq,
                            //BuildingName = "",
                            indextext = Join(description, terms, false).Decompound(config.DecompoundList) + " " + code,
                            Description = Join(description, terms, true),
                            Location = point,
                            Point = PointfromGeoLocation(point),
                            // Organisation = "",
                            // Postcode = "",
                            //      SubBuilding = "",
                            Thoroughfare = stopname.Split('/').ToList(),
                            Locality = new List<string>(),
                            Areas = terms,
                            Status = "Approved"
                        };

                        // add to the list of stuff to index
                        address.indextext = address.indextext.Replace("&", " and ");

                        // add item to the list of documents to index
                        AddIndexItem(address, descriptor);
                    }
                }

                CommitBultRequest(config, descriptor);

            }
        }
    }
}
