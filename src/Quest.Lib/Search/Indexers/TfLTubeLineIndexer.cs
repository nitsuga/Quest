using System;
using System.Collections.Generic;
using System.Linq;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Tfl.Api.Presentation.Entities;
using Quest.Common.Messages.Gazetteer;

namespace Quest.Lib.Search.Indexers
{
    internal class TfLTubeLineIndexer : ElasticIndexer
    {
        public string URL { get; set; } = "";

        public override void StartIndexing(BuildIndexSettings config)
        {
            Build(config);
        }

        internal void Build(BuildIndexSettings config)
        {
            var response = MakeRequest<Line[]>(URL);
            ProcessResponse(response, config.LocalAreaNames, config);
        }

        public void ProcessResponse(Line[] places, PolygonManager localAreas, BuildIndexSettings config)
        {
            if (places == null)
                return;

            var descriptor = GetBulkRequest(config);
            config.RecordsTotal = places.Length;

            foreach (var p in places)
            {
                //var point = new PointGeoShape(new GeoCoordinate(p.Lat, p.Lon));
                //var loc = new GeoLocation(p.Lat, p.Lon);
                //var terms = IndexBuilder.GetLocalAreas(loc, localAreas);

                config.RecordsCurrent++;

                // commit any messages and report progress
                CommitCheck(this, config, descriptor);

                var status = "UNKNOWN";
                if (p.LineStatus.Count > 0)
                {
                    var firstOrDefault = p.LineStatus.FirstOrDefault();
                    if (firstOrDefault != null)
                        status = firstOrDefault.StatusSeverityDescription;
                }

                var description = "TUBE STATUS " + p.Name.ToUpper() + " "+ status;
                
                var address = new LocationDocument
                {
                    Created = DateTime.Now,
                    Type = IndexBuilder.AddressDocumentType.TubeLine,
                    Source = "TFL",
                    ID = IndexBuilder.AddressDocumentType.TubeLine + p.Id,
                    //BuildingName = "",
                    indextext = description, // IndexBuilder.Join(description, terms, false),
                    Description = description,
                    Location = null, //loc,
                    Point = null,//point,
                   // Organisation = "",
                    //Postcode = "",
                    //SubBuilding = "",
                    Thoroughfare = null, // p.CommonName.Split(',').ToList(),
                    Locality = new List<string>(),
                    Areas = null, //terms,
                    Status = status
                };

                AddIndexItem(address, descriptor);
            }

            CommitBultRequest(config, descriptor);
        }
    }
}
