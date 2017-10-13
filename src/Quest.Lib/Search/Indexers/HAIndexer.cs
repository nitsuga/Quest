using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Quest.Common.Messages;
using Quest.Lib.Csv;

namespace Quest.Lib.Search.Indexers
{
    internal class HaIndexer : ElasticIndexer
    {
        public string Filename { get; set; }

        public enum HAdata
        {
            Haid,
            Userformat,
            Bd,
            Tn,
            Area,
            Dd,
            County,
            Beat,
            Easting,
            Northing,
            Region,
            Recordtype,
            Location,
            Oa
        }

        public override void StartIndexing(BuildIndexSettings config)
        {
            Build(config);
        }

        private void Build(BuildIndexSettings config)
        {
            var descriptor = GetBulkRequest(config);

            //speed up by checking london and surrounding counties only
            string[] london = { "MET", "THAM", "ESSX", "HERT", "KENT", "SURY" };

            if (Filename.EndsWith(".zcsv"))
            {
                var uncompressed = Filename + ".csv";
                if (!File.Exists(uncompressed))
                    Compress.Decompress(Filename, uncompressed);
                Filename = uncompressed;
            }

            using (var reader = File.OpenText(Filename))
            {
                foreach (var data in CsvReader.Read(reader, new CsvOptions { RowsToSkip = 1, Separator = ',' }))
                {
                    config.RecordsTotal++;
                    config.RecordsCurrent++;

                    //using an enum to relate column numbers to headings

                    var type = data[(int)HAdata.Recordtype];
                    var county = data[(int)HAdata.County];

                    //if (!london.Contains(county))
                    //{
                    //    config.Skipped++;
                    //    continue;
                    //}

                    if (!(type == "JUNCTION" || type == "INSERTMP" || type == "LINKMP"))
                    {
                        config.Skipped++;
                        continue;
                    }

                    var point = GeomUtils.ConvertToLatLonLoc(double.Parse(data[(int)HAdata.Easting]),
                        double.Parse(data[(int)HAdata.Northing]));


                    if (data[(int)HAdata.Dd] != "M25")
                    {
                        // check whether point is in master area if required
                        if (!IsPointInRange(config, point.Longitude, point.Latitude))
                        {
                            config.Skipped++;
                            continue;
                        }
                    }

                    var terms = GetLocalAreas(config, point);

                    // commit any messages and report progress
                    CommitCheck(this, config, descriptor);

                    var description = "";
                    var addresstype = "";
                    var id = "HWAY" + data[(int)HAdata.Haid];

                    if (type == "JUNCTION") //use road number (DD) and junction name (TN) for description
                    {
                        description = $"{data[(int)HAdata.Dd]} {data[(int)HAdata.Tn]}";
                        addresstype = IndexBuilder.AddressDocumentType.Junction;
                        Debug.Print(id + " " + description);
                    }
                    else if (type == "INSERTMP" || type == "LINKMP")
                    //use road number (DD) and junction name (TN) for description
                    {
                        addresstype = IndexBuilder.AddressDocumentType.MarkerPost;
                        var pattern = @"P(?<postnumber>\d*)\/(?<postdecimal>\d)(?<postletter>[A-Z])";
                        var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                        var m = regex.Match(data[(int)HAdata.Bd]);
                        if (m.Success)
                            description =
                                $"{data[(int)HAdata.Dd]} {m.Groups["postletter"].Value} {m.Groups["postnumber"].Value}.{m.Groups["postdecimal"].Value}";
                        else
                            continue;
                    }
                    // "M25 A 10.2"

                    var address = new LocationDocument
                    {
                        Created = DateTime.Now,
                        Type = addresstype,
                        Source = "HighwaysAgency",
                        ID = id,
                        //BuildingName = "",
                        Description = Join(description, terms, true),
                        indextext = Join(description, terms, false),
                        Location = point,
                        //Organisation = "",
                        //Postcode = "",
                        //SubBuilding = "",
                        Thoroughfare = new List<string>(),
                        Locality = new List<string>(),
                        Areas = terms,
                        Status = "Approved"
                    };
                    //Debug.Print(address.indextext);
                    address.Thoroughfare.Add(data[(int)HAdata.Dd]);

                    // add to the list of stuff to index

                    address.indextext = address.indextext.Replace("&", " and ").Decompound(config.DecompoundList);

                    // add item to the list of documents to index
                    AddIndexItem<LocationDocument>(address, descriptor);
                }

                // commit anything else
                CommitBultRequest(config, descriptor);
            }
        }
    }
}