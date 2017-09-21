using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Quest.Lib.OS.Model;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Quest.Common.Messages;
using Quest.Lib.Trace;

namespace Quest.Lib.OS.Indexer
{
    internal class PafIndexer : ElasticIndexer
    {
        public override void StartIndexing(BuildIndexSettings config)
        {
            Build(config);
        }

        private void Build(BuildIndexSettings config)
        {
            using (var db = new QuestOSEntities())
            {

                Logger.Write($"{GetType().Name}: Counting records..", GetType().Name);

                // figure out batch sizes
                var startRecord = db.PAFs.Min(x => x.Id);
                var stopRecord = db.PAFs.Max(x => x.Id);

                int estimatedRecordSize = 512;
                // recommended packet size is 10Mb
                int recommendedPacketSize = 10 * 1024 * 1024;
                int recordsPerPacket = recommendedPacketSize / estimatedRecordSize;

                long batchSize = recordsPerPacket;
                int concurrentBatches = 4;

                config.RecordsTotal = db.PAFs.Select(x => x.Id).Count();
                //config.Logfrequency = recordsPerPacket;

                BatchIndexer.ProcessBatches(this, config, startRecord, stopRecord, batchSize,
                    concurrentBatches, ProcessBatch);
            }
        }

        private void ProcessBatch(BuildIndexSettings config, BatchIndexer.BatchWork work)
        {
            using (var db = new QuestOSEntities())
            {
                var total = db.PAFs.Count();
                config.RecordsTotal = total;

                var pattern = @"^(?<postcodeshort>[A-Z]{1,2}[0-9])[A-Z].*";
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);

                var descriptor = GetBulkRequest(config);
                foreach (var r in db.PAFs.Where(x => x.Id >= work.StartIndex && x.Id <= work.StopIndex).ToList())
                {
                    config.RecordsCurrent++;

                    // commit any messages and report progress
                    CommitCheck(this, config, descriptor, true);

                    try
                    {
                        var point = GeomUtils.ConvertToLatLonLoc(r.X_COORDINATE, r.Y_COORDINATE);

                        // check whether point is in master area if required
                        if (!IsPointInRange(config, point.Longitude, point.Latitude))
                        {
                            config.Skipped++;
                            continue;
                        }

                        var terms = GetLocalAreas(config, point);

                        var range = ExtractRange(r.BUILDING_NAME);
                        var indexText = r.FULLADDRESS;
                        if (range != null)
                        {
                            for (var x = range[0]; x <= range[1]; x++)
                                indexText += " " + x;
                        }


                        if (!string.IsNullOrEmpty(r.POSTCODE))
                        {
                            var m = regex.Match(r.POSTCODE);
                            if (m.Success)
                            {
                                indexText += " " + m.Groups["postcodeshort"].Value;
                            }
                        }

                        r.DEPENDENT_LOCALITY = r.DEPENDENT_LOCALITY ?? "";
                        r.BUILDING_NAME = r.BUILDING_NAME ?? "";
                        r.FULLADDRESS = r.FULLADDRESS ?? "";
                        r.ORGANISATION_NAME = r.ORGANISATION_NAME ?? "";
                        r.SUB_BUILDING_NAME = r.SUB_BUILDING_NAME ?? "";
                        r.THOROUGHFARE = r.THOROUGHFARE ?? "";
                        r.DEPENDENT_THOROUGHFARE = r.DEPENDENT_THOROUGHFARE ?? "";

                        var address = new LocationDocument
                        {
                            Created = DateTime.Now,
                            ID = "PAF" + r.Id,
                            Type = IndexBuilder.AddressDocumentType.Address,
                            Source = "PAF",
                            //BuildingName = r.BUILDING_NUMBER == 0 ? r.BUILDING_NAME : r.BUILDING_NUMBER.ToString().ToUpper(),
                            Description = r.FULLADDRESS.ToUpper(),
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
                            UPRN = r.UPRN,
                            USRN = r.USRN,
                            Classification = ""
                        };

                        address.Thoroughfare.Add(r.THOROUGHFARE.ToUpper());
                        if (r.DEPENDENT_THOROUGHFARE.Length > 0)
                            address.Thoroughfare.Add(r.DEPENDENT_THOROUGHFARE.ToUpper());

                        address.Locality.Add(r.POST_TOWN);
                        if (r.DEPENDENT_LOCALITY.Length > 0) address.Locality.Add(r.DEPENDENT_LOCALITY.ToUpper());

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

            }

        }


    }
}