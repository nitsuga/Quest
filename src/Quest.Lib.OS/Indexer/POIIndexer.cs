using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Quest.Common.Messages;
using Nest;
using NetTopologySuite.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System.IO;
using Quest.Lib.Csv;

namespace Quest.Lib.OS.Indexer
{
    internal class PoiIndexer : ElasticIndexer
    {
        public string Filename { get; set; }

        public override void StartIndexing(BuildIndexSettings config)
        {
            if (Filename.EndsWith(".csv", StringComparison.CurrentCultureIgnoreCase))
                BuildFromCsv(config, Filename);
            else if (Filename.EndsWith(".shp", StringComparison.CurrentCultureIgnoreCase))
                BuildFromShapefile(config, Filename);
        }

        private void BuildFromShapefile(BuildIndexSettings config, string poIfilename)
        {
            var descriptor = GetBulkRequest(config);
            IGeometryFactory geomFact = new GeometryFactory();
            using (var reader = new ShapefileDataReader(poIfilename, geomFact))
            {
                while (reader.Read())
                {
                    config.RecordsCurrent++;
                    config.RecordsTotal++;

                    List<string> data = new List<string>();

                    for (int i = 0; i < reader.FieldCount - 1; i++)
                    {
                        var v = reader.GetValue(i);
                        data.Add(v?.ToString() ?? "");
                    }

                    // commit any messages and report progress
                    CommitCheck(this, config, descriptor);

                    var point = new GeoLocation(reader.Geometry.Coordinate.Y, reader.Geometry.Coordinate.X);
                    ProcessRecord(data.ToArray(), config, descriptor, point);
                }
                // commit anything else
                CommitBultRequest(config, descriptor);
            }
        }


        private void BuildFromCsv(BuildIndexSettings config, string poIfilename)
        {
            var descriptor = GetBulkRequest(config);

            // throw away header line
            using (StreamReader reader = File.OpenText(Filename))
            {

                foreach (var data in CsvReader.Read(reader, new CsvOptions { RowsToSkip = 0, Separator = ',' }))
                {
                    config.RecordsCurrent++;
                        config.RecordsTotal++;

                        // commit any messages and report progress
                        CommitCheck(this, config, descriptor);

                        var featureEasting = data[3];
                        var featureNorthing = data[4];
                        var point = GeomUtils.ConvertToLatLonLoc(double.Parse(featureEasting), double.Parse(featureNorthing));

                        ProcessRecord(data.Line, config, descriptor, point);
                    }
                    // commit anything else
                    CommitBultRequest(config, descriptor);
                }
            }
        

        void ProcessRecord(string[] data, BuildIndexSettings config, BulkRequest descriptor, GeoLocation point)
        {
            var uniqueReferenceNumber = data[0];
            var name = data[1];
            var pointxClassificationCode = data[2];
            var featureEasting = data[3];
            var featureNorthing = data[4];
            var addressDetail = data[14];
            var streetName = data[15];
            var locality = data[16];
            var postcode = data[18];

            long.TryParse(data[6], out long uprn);

            if (name.StartsWith("Wi-Fi Hotspot"))
            {
                config.Skipped++;
                return;
            }

            // Remove roads
            if (pointxClassificationCode == "10590732")
            {
                config.Skipped++;
                return;
            }

            // check whether point is in master area if required
            if (!IsPointInRange(config, point.Longitude, point.Latitude))
            {
                config.Skipped++;
                return;
            }

            var terms = GetLocalAreas(config, point);

            // create classification code
            var cc = pointxClassificationCode.PadLeft(8).Replace(" ", "0");

            var parts = new List<string> { name };
            if (addressDetail.Length > 0) parts.Add(addressDetail);
            if (streetName.Length > 0) parts.Add(streetName);
            if (locality.Length > 0) parts.Add(locality);

            var additionalPostcode = "";
            if (!string.IsNullOrEmpty(postcode))
            {
                var pattern = @"^(?<postcodeshort>[A-Z]{1,2}[0-9])[A-Z].*";

                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                var m = regex.Match(postcode);
                if (m.Success)
                {
                    additionalPostcode = m.Groups["postcodeshort"].Value;
                }
            }

            var description = string.Join(", ", parts.ToArray()).ToUpper();

            //description = Remove(description, '(', ')');
            if (terms.Length > 0)
            {
                description = Join(description, terms, true);
            }

            var indexText = description.RemoveBetween('(', ')') + " " + additionalPostcode;
            indexText = indexText.Decompound(config.DecompoundList);

            var address = new LocationDocument
            {
                Created = DateTime.Now,
                Type = IndexBuilder.AddressDocumentType.Poi,
                Source = "POI",
                ID = IndexBuilder.AddressDocumentType.Poi + uniqueReferenceNumber,
                //BuildingName = "",
                Description = description,
                indextext = indexText,
                Location = point,
                Point = PointfromGeoLocation(point),
                //Organisation = "",
                //Postcode = postcode,
                //      SubBuilding = "",
                Thoroughfare = new List<string>(),
                Locality = new List<string>(),
                Areas = terms,
                Status = "Approved",
                Classification = cc,
                UPRN = uprn
            };

            if (streetName.Length > 0)
                address.Thoroughfare.Add(streetName.ToUpper());

            address.Locality.Add(locality);

            // add to the list of stuff to index
            address.indextext = address.indextext.Replace("&", " and ");

            // add item to the list of documents to index
            AddIndexItem(address, descriptor);

        }
    }
}