using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text.RegularExpressions;
using Nest;
using Quest.Lib.DataModel;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Quest.Common.Messages;
using Quest.Lib.Trace;
using Quest.Lib.OS.Model;

namespace Quest.Lib.OS.Indexer
{
    internal class NlpgIndexer : ElasticIndexer
    {
        /// <summary>
        /// Loads full NLPG from the database
        /// </summary>
        /// <param name="config"></param>
        public override void StartIndexing(BuildIndexSettings config)
        {
            Build(config);
        }

        private void Build(BuildIndexSettings config)
        {
            using (var db = new QuestNLPGEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;

                Logger.Write($"{GetType().Name}: Counting records..", GetType().Name);

                ((IObjectContextAdapter)db).ObjectContext.CommandTimeout = 360;

                // figure out batch sizes
                var startRecord = db.NLPGs.Min(x => x.Id);
                var stopRecord = db.NLPGs.Max(x => x.Id);
                var total = stopRecord - startRecord;

                int estimatedRecordSize = 512;
                // recommended packet size is 10Mb
                int recommendedPacketSize = 10 * 1024 * 1024;
                int recordsPerPacket = recommendedPacketSize / estimatedRecordSize;

                long batchSize = recordsPerPacket;
                int concurrentBatches = 4;

                config.RecordsTotal = db.NLPGs.Select(x=>x.Id).Count();
                //config.Logfrequency = recordsPerPacket;

                BatchIndexer.ProcessBatches(this, config, startRecord, stopRecord, batchSize,
                    concurrentBatches, ProcessBatch);
            }
            config.Client.Refresh(new RefreshRequest(ElasticSettings.DefaultDocindex));
        }

        private void ProcessBatch(BuildIndexSettings config, BatchIndexer.BatchWork work)
        {
            try
            {
                using (var db = new QuestNLPGEntities())
                {
                    db.Configuration.ProxyCreationEnabled = false;

                    ((IObjectContextAdapter) db).ObjectContext.CommandTimeout = 360;

                    var descriptor = GetBulkRequest(config);

                    // main loop here
                    foreach (
                        var r in db.NLPGs.AsNoTracking().Where(x => x.Id >= work.StartIndex && x.Id <= work.StopIndex))
                    {
                        lock (config)
                        {
                            config.RecordsCurrent++;
                        }

                        // dont index street records - these are taken care of by ITN
                        if (r.pao_text == "STREET RECORD")
                        {
                            lock (config)
                            {
                                config.Skipped++;
                            }
                            continue;
                        }

                        var point = GeomUtils.ConvertToLatLonLoc(r.nlpg_x_coordinate ?? 0, r.nlpg_y_coordinate ?? 0);

                        // check whether point is in master area if required
                        if (!IsPointInRange(config, point.Longitude, point.Latitude))
                        {
                            lock (config)
                            {
                                config.Skipped++;
                            }
                            continue;
                        }

                        var terms = GetLocalAreas(config, point);

                        // commit any messages and report progress
                        CommitCheck(this, config, descriptor, true);

                        // add primary number ranges:
                        var buildingNumbers = new List<int>();
                        var startnum = r.pao_start_number ?? 0;
                        if (startnum > 0)
                        {
                            var endnum = r.pao_end_number ?? startnum;
                            if (endnum == 0)
                                endnum = startnum;
                            for (int x = startnum; x <= endnum; x++)
                                buildingNumbers.Add(x);
                        }
                        // add secondary number ranges:
                        startnum = r.sao_start_number ?? 0;
                        if (startnum > 0)
                        {
                            var endnum = r.sao_end_number ?? startnum;
                            if (endnum == 0)
                                endnum = startnum;
                            for (int x = startnum; x <= endnum; x++)
                                buildingNumbers.Add(x);
                        }

                        var indextext = buildingNumbers.Count > 1
                            ? string.Join(" ", buildingNumbers) + " " + r.geo_single_address_label.ToUpper()
                            : r.geo_single_address_label.ToUpper();

                        //add first chars for central london postcode districts that have an alpha suffix eg NW1J, W1X
                        //this means a search of SW1 will return SW1X etc

                        if (!string.IsNullOrEmpty(r.postcode_locator))
                        {
                            var pattern = @"^(?<postcodeshort>[A-Z]{1,2}[0-9])[A-Z].*";

                            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                            var m = regex.Match(r.postcode_locator);
                            if (m.Success)
                            {
                                indextext += " " + m.Groups["postcodeshort"].Value;
                            }
                        }

                        //add 'FLAT 1A' to indextext as well as 'FLAT A, 1' which is default
                        // use sao text and pao_start_number
                        if (r.sao_text != null && r.pao_start_number > 0)
                        {
                            var pattern = @"^FLAT\s(?<flatletter>[A-Z])$";
                            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                            var m = regex.Match(r.sao_text);
                            if (m.Success)
                            {
                                var flattext = r.pao_start_number + m.Groups["flatletter"].Value;
                                indextext += " FLAT " + flattext;
                            }
                        }

                        var locality = new List<string>();
                        if (!string.IsNullOrEmpty(r.town_name))
                            locality.Add(r.town_name.ToUpper());

                        if (!string.IsNullOrEmpty(r.locality_name))
                            locality.Add(r.locality_name.ToUpper());


                            var address = new LocationDocument
                            {
                                Created = DateTime.Now,
                                ID = "NLPG" + r.Id,
                                Type = IndexBuilder.AddressDocumentType.Address,
                                Source = "NLPG",
                                //BuildingName = r.pao_text,
                                Description = r.geo_single_address_label.ToUpper(),
                                indextext = Join(indextext, terms, false).Decompound(config.DecompoundList),
                                Location = point,
                                Point = PointfromGeoLocation(point),
                                //Organisation = "",
                                //Postcode = r.postcode_locator,
                                //SubBuilding = "",
                                Thoroughfare = new List<string> {r.street_description.ToUpper()},
                                Locality = locality,
                                //BuildingNumbers = buildingNumbers,
                                Areas = terms,
                                UPRN = r.uprn ?? 0,
                                USRN = r.usrn ?? 0,
                                Status = LogicalStatus(r.logical_status ?? 0),
                                Classification = r.CLASSIFICATION_CODE
                            };
                            // add to the list of stuff to index
                            address.indextext = address.indextext.Replace("&", " and ");

                            // add item to the list of documents to index
                            AddIndexItem(address, descriptor);
                    }
                    // commit anything else
                    CommitBultRequest(config, descriptor);

                }
            }
            catch (Exception ex)
            {
                Logger.Write($"{GetType().Name}: Batch#{work.Batch}: Failed {ex}", GetType().Name);
            }
        }

        private static string LogicalStatus(int code)
        {
            switch (code)
            {
                case 1:
                    return "Approved";
                case 3:
                    return "Alternative";
                case 5:
                    return "Candidate";
                case 6:
                    return "Provisional";
                case 7:
                    return "Rejected";
                case 9:
                    return "Rejected";
                case 8:
                    return "Historical";
                default:
                    return "Unknown";
            }
        }


    }
}