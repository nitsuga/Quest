using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Quest.Lib.Trace;
using Quest.Lib.OS.DataModelOS;
using Quest.Lib.Data;
using Quest.Common.Messages.Gazetteer;

namespace Quest.Lib.OS.Indexer
{
    internal class PafIndexer : ElasticIndexer
    {
        private IDatabaseFactory _dbFactory;

        public PafIndexer(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public override void StartIndexing(BuildIndexSettings config)
        {
            Build(config);
        }

        private void Build(BuildIndexSettings config)
        {
            _dbFactory.Execute<QuestOSContext>((db) =>
            {

                Logger.Write($"{GetType().Name}: Counting records..", GetType().Name);

                // figure out batch sizes
                var startRecord = db.Paf.Min(x => x.Id);
                var stopRecord = db.Paf.Max(x => x.Id);

                int estimatedRecordSize = 512;
                // recommended packet size is 10Mb
                int recommendedPacketSize = 10 * 1024 * 1024;
                int recordsPerPacket = recommendedPacketSize / estimatedRecordSize;

                long batchSize = recordsPerPacket;
                int concurrentBatches = 4;

                config.RecordsTotal = db.Paf.Select(x => x.Id).Count();
                //config.Logfrequency = recordsPerPacket;

                BatchIndexer.ProcessBatches(this, config, startRecord, stopRecord, batchSize,
                    concurrentBatches, ProcessBatch);
            });
        }

        private void ProcessBatch(BuildIndexSettings config, BatchIndexer.BatchWork work)
        {
            _dbFactory.Execute<QuestOSContext>((db) =>
            {
                var total = db.Paf.Count();
                config.RecordsTotal = total;

                var pattern = @"^(?<postcodeshort>[A-Z]{1,2}[0-9])[A-Z].*";
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);

                var descriptor = GetBulkRequest(config);
                foreach (var r in db.Paf.Where(x => x.Id >= work.StartIndex && x.Id <= work.StopIndex).ToList())
                {
                    config.RecordsCurrent++;

                    // commit any messages and report progress
                    CommitCheck(this, config, descriptor, true);

                    try
                    {
                        var point = GeomUtils.ConvertToLatLonLoc(r.XCoordinate, r.YCoordinate);

                        // check whether point is in master area if required
                        if (!IsPointInRange(config, point.Longitude, point.Latitude))
                        {
                            config.Skipped++;
                            continue;
                        }

                        var terms = GetLocalAreas(config, point);

                        var range = ExtractRange(r.BuildingName);
                        var indexText = r.Fulladdress;
                        if (range != null)
                        {
                            for (var x = range[0]; x <= range[1]; x++)
                                indexText += " " + x;
                        }


                        if (!string.IsNullOrEmpty(r.Postcode))
                        {
                            var m = regex.Match(r.Postcode);
                            if (m.Success)
                            {
                                indexText += " " + m.Groups["postcodeshort"].Value;
                            }
                        }

                        r.DependentLocality = r.DependentLocality ?? "";
                        r.BuildingName = r.BuildingName ?? "";
                        r.Fulladdress = r.Fulladdress ?? "";
                        r.OrganisationName = r.OrganisationName ?? "";
                        r.SubBuildingName = r.SubBuildingName ?? "";
                        r.Thoroughfare = r.Thoroughfare ?? "";
                        r.DependentThoroughfare = r.DependentThoroughfare ?? "";

                        var address = new LocationDocument
                        {
                            Created = DateTime.Now,
                            ID = "PAF" + r.Id,
                            Type = IndexBuilder.AddressDocumentType.Address,
                            Source = "PAF",
                            //BuildingName = r.BUILDING_NUMBER == 0 ? r.BUILDING_NAME : r.BUILDING_NUMBER.ToString().ToUpper(),
                            Description = r.Fulladdress.ToUpper(),
                            indextext = Join(indexText, terms, false).Decompound(config.DecompoundList),
                            Location = point,
                            Point = PointfromGeoLocation(point),
                            //Organisation = r.ORGANISATION_NAME ?? "".ToUpper(),
                            //Postcode = r.POSTCODE ?? "".ToUpper(),
                            //SubBuilding = r.SUB_BUILDING_NAME ?? "".ToUpper(),
                            Thoroughfare = new List<string>(),
                            Locality = new List<string>(),
                            //BuildingNumbers = range,
                            Areas = terms,
                            UPRN = r.Uprn,
                            USRN = r.Usrn,
                            Classification = ""
                        };

                        address.Thoroughfare.Add(r.Thoroughfare.ToUpper());
                        if (r.DependentThoroughfare.Length > 0)
                            address.Thoroughfare.Add(r.DependentThoroughfare.ToUpper());

                        address.Locality.Add(r.PostTown);
                        if (r.DependentLocality.Length > 0) address.Locality.Add(r.DependentLocality.ToUpper());

                        //TODO: include status in PAF load
                        address.Status = "Active"; // LogicalStatus(r.LOGICAL_STATUS??0);

                        // add to the list of stuff to index
                        address.indextext = address.indextext.Replace("&", " and ");

                        // add item to the list of documents to index
                        AddIndexItem(address, descriptor);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                // commit anything else
                CommitBultRequest(config, descriptor);

            });
        }
    }
}