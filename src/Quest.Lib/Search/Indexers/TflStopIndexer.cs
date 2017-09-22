using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Tfl.Api.Presentation.Entities;
using Quest.Common.Messages;
using Quest.Lib.Trace;

namespace Quest.Lib.Search.Indexers
{
    internal class TflStopIndexer : ElasticIndexer
    {
        public string URL { get; set; } = "";

        public override void StartIndexing(BuildIndexSettings config)
        {
            Build(config);
        }

        private void Build(BuildIndexSettings config)
        {
            try
            {
                DeleteDataSet<LocationDocument>(config.DefaultIndex, config.Client, IndexBuilder.AddressDocumentType.StopPoint);

                var descriptor = GetBulkRequest(config);

                string[] modes = {"cable-car", "bus", "tube", "coach","cycle", "cycle-hire","dlr", "national-rail","overground","river-bus","river-tour" ,"tflrail", "tram", "walking" };
                foreach(var mode in modes)
                    LoadMode(mode, config, descriptor);

                CommitBultRequest(config, descriptor);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.Read();
            }
        }

        public void LoadMode(string mode, BuildIndexSettings config, BulkRequest descriptor)
        {
            try
            {
                int page = 0;
                int count ;
                do
                {
                    count = 0;
                    page++;
#if false
                    EtlRequest d 
                        = new Tfl.Api.Common.Entities.ETLRequestType();
                    Disruption q = new Tfl.Api.Common.Enums.DisruptionCategory();
#endif
                    //var request = $"https://api.tfl.gov.uk/StopPoint/Mode/{mode}?page={page}&app_id=c6e8ebeb&app_key=2db60e64f5c372c5b9ceb4f41b386e3d";
                    var request = string.Format(URL,mode, page);

                    var response = MakeRequest<StopPointsResponse>(request);
                    if (response != null)
                    {
                        Logger.Write($"{GetType().Name}: processing TfL {mode} page {page}", GetType().Name);

                        count = response.StopPoints.Count;
                        ProcessResponse(mode, response, config, descriptor);
                    }

                } while (count>0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void ProcessResponse(string mode, StopPointsResponse data, BuildIndexSettings config, BulkRequest descriptor)
        {
            if (data == null)
                return;

            config.RecordsTotal += data.StopPoints.Count;

            foreach (var p in data.StopPoints)
            {
                config.RecordsCurrent++;

                // commit any messages and report progress
                CommitCheck(this, config, descriptor);

                if (p.StopType.Contains("Platform"))
                {
                    config.Skipped++;
                    continue;
                }

                string type = mode.Replace("-"," ");

                switch (p.StopType)
                {
                    case "CarPickupSetDownArea":
                        break;
                    case "NaptanAirAccessArea":
                        break;
                    case "NaptanAirEntrance":
                        break;
                    case "NaptanAirportBuilding":
                        break;
                    case "NaptanBusCoachStation":
                        break;
                    case "NaptanBusWayPoint":
                        break;
                    case "NaptanCoachAccessArea":
                        break;
                    case "NaptanCoachBay":
                        break;
                    case "NaptanCoachEntrance":
                        break;
                    case "NaptanCoachServiceCoverage":
                        break;
                    case "NaptanCoachVariableBay":
                        break;
                    case "NaptanFerryAccessArea":
                        break;
                    case "NaptanFerryBerth":
                        break;
                    case "NaptanFerryEntrance":
                        break;
                    case "NaptanFerryPort":
                        break;
                    case "NaptanFlexibleZone":
                        break;
                    case "NaptanHailAndRideSection":
                        break;
                    case "NaptanLiftCableCarAccessArea":
                        break;
                    case "NaptanLiftCableCarEntrance":
                        break;
                    case "NaptanLiftCableCarStop":
                        break;
                    case "NaptanLiftCableCarStopArea":
                        break;
                    case "NaptanMarkedPoint":
                        break;
                    case "NaptanMetroAccessArea":
                        break;
                    case "NaptanMetroEntrance":
                        break;
                    case "NaptanMetroPlatform":
                        break;
                    case "NaptanMetroStation":
                        break;
                    case "NaptanOnstreetBusCoachStopCluster":
                        continue;
                    case "NaptanOnstreetBusCoachStopPair":
                        continue;
                    case "NaptanPrivateBusCoachTram":
                        break;
                    case "NaptanPublicBusCoachTram":
                        break;
                    case "NaptanRailAccessArea":
                        break;
                    case "NaptanRailEntrance":
                        break;
                    case "NaptanRailPlatform":
                        break;
                    case "NaptanRailStation":
                        break;
                    case "NaptanSharedTaxi":
                        break;
                    case "NaptanTaxiRank":
                        break;
                    case "NaptanUnmarkedPoint":
                        break;
                    case "TransportInterchange":
                        continue;
                }

                var description = p.CommonName;

                if (description.Contains(type))
                    description += " " + type;

                if (p.Indicator!=null)
                    description += " " + (p.Indicator ?? "");

                if (p.Lines!=null)
                    description += " " + string.Join(" ", p.Lines.Select(x=>x.Name).ToList());

                var indextext = description;

                //Debug.Print(description);

                var point = new PointGeoShape(new GeoCoordinate(p.Lat, p.Lon));

                // check whether point is in master area if required
                if (!IsPointInRange(config, p.Lon, p.Lat))
                {
                    config.Skipped++;
                    continue;
                }

                var loc = new GeoLocation(p.Lat, p.Lon);

                var terms = GetLocalAreas(loc, config.LocalAreaNames);
                var status = "Active";

                indextext = Join(indextext, terms, false).Decompound(config.DecompoundList);

                var address = new LocationDocument
                {
                    Created = DateTime.Now,
                    Type = IndexBuilder.AddressDocumentType.StopPoint,
                    Source = "TFL",
                    ID = IndexBuilder.AddressDocumentType.StopPoint + p.Id,
                    //BuildingName = "",
                    indextext = indextext.ToUpper(),
                    Description = description.ToUpper(),
                    Location = loc,
                    Point = point,
                /////    Organisation = "",
                 //   Postcode = "",
                //    SubBuilding = "",
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
