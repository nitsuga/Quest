using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Nest;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Quest.Common.Messages;

namespace Quest.Lib.OS.Indexer
{
    internal class CodePointIndexer : ElasticIndexer
    {
        public override void StartIndexing(BuildIndexSettings config)
        {
            Build(config);
        }

        private void Build(BuildIndexSettings config)
        {
            var directory = SettingsHelper.GetVariable("File.Codepoint",
                @"G:\Users\Marcus\Documents\codepp_essh_gb\Polygons\Data\Shape\POLYS\Shape_AB_ZE");

            foreach (var filename in Directory.EnumerateFiles(directory, "*.shp"))
            {
                BuildFile(filename, config);
            }
        }

        private void BuildFile(string filename, BuildIndexSettings config)
        {
            Debug.Print("Building codepoint");

            var codes = new PolygonManager();
            codes.BuildFromShapefile(filename);

            var descriptor = GetBulkRequest(config);
            var data = codes.PolygonIndex.QueryAll();
            config.RecordsTotal = data.Count;

            foreach (var p in data)
            {

                config.RecordsCurrent++;

                var pc = p.data[0];

                var poly = GeomUtils.MakePolygonFromBng(p.geom);

                var point = new GeoLocation(poly.Centroid.Y, poly.Centroid.X);

                // check whether point is in master area if required
                if (!IsPointInRange(config, point.Longitude, point.Latitude))
                {
                    config.Skipped++;
                    continue;
                }

                var terms = GetLocalAreas(point, config.LocalAreaNames);

                // commit any messages and report progress
                CommitCheck(this, config, descriptor);

                //if (terms.Length > 0)
                {
                    var description = ((string)pc).ToUpper();
                    var indextext = Join(description, terms, false);

                    // get 1st char of second half
                    var pos = description.IndexOf(' ');
                    if (pos > 0)
                    {
                        indextext = indextext + " " + description.Substring(0, pos + 2);
                    }

                    var address = new LocationDocument
                    {
                        Created = DateTime.Now,
                        Type = IndexBuilder.AddressDocumentType.CodePoint,
                        Source = "OS",
                        ID = "CP " + pc,
                        //BuildingName = "",
                        indextext = indextext,
                        Description = Join(description, terms, true),
                        Location = point,
                        Point = PointfromGeoLocation(point),
                        //Organisation = "",
                        //Postcode = description,
                        Poly = GeomUtils.MakePolygon(poly),
                        //SubBuilding = "",
                        Thoroughfare = null,
                        Locality = new List<string>(),
                        Areas = terms,
                        Status = "Approved"
                    };

                    // add to the list of stuff to index
                    address.indextext = address.indextext.Replace("&", " and ");

                    // add item to the list of documents to index
                    AddIndexItem<LocationDocument>(address, descriptor);
                }
            }

            CommitBultRequest(config, descriptor);
        }
    }
}