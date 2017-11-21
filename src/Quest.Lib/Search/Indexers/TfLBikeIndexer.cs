using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Tfl.Api.Presentation.Entities;
using Quest.Common.Messages.Gazetteer;

namespace Quest.Lib.Search.Indexers
{
    internal class TfLBikeIndexer : ElasticIndexer
    {
        // "https://api.tfl.gov.uk/BikePoint?app_id=c6e8ebeb&app_key=2db60e64f5c372c5b9ceb4f41b386e3d";
        public string URL { get; set; } = ""; 

        public override void StartIndexing(BuildIndexSettings config)
        {
            Build(config);
        }

        internal void Build(BuildIndexSettings config)
        {                    
            var locationsResponse = MakeRequest<Place[]>(URL);
            ProcessResponse(locationsResponse, config.LocalAreaNames, config);
        }

        public static void ProcessResponse(Place[] places, PolygonManager localAreas, BuildIndexSettings config)
        {
            if (places == null)
                return;

            var docs = new List<LocationDocument>();

            config.RecordsTotal = places.Length;

            foreach (var p in places)
            {
                config.RecordsCurrent++;
                var point = new PointGeoShape(new GeoCoordinate(p.Lat, p.Lon));
                var loc = new GeoLocation(p.Lat, p.Lon);

                var terms = GetLocalAreas(loc, localAreas);

                var description = "BIKE POINT " + p.CommonName.ToUpper();

                var address = new LocationDocument
                {
                    Created = DateTime.Now,
                    Type = IndexBuilder.AddressDocumentType.Bike,
                    Source = "TFL",
                    ID = IndexBuilder.AddressDocumentType.Bike + p.Id,
                    //BuildingName = "",
                    indextext = Join(description, terms, false).Decompound(config.DecompoundList),
                    Description = description,
                    Location = loc,
                    Point = point,
                    //Organisation = "",
                    //Postcode = "",
              //      SubBuilding = "",
                    Thoroughfare = p.CommonName.Split(',').ToList(),
                    Locality = new List<string>(),
                    Areas = terms,
                    Status = "Approved"
                };

                if (Math.Abs(p.Lat) < 0.1 && Math.Abs(p.Lon) < 0.1)
                {
                    config.Skipped++;
                }
                else
                    docs.Add(address);
            }

            IndexItems(docs.ToArray(), config);
        }
    }
}
