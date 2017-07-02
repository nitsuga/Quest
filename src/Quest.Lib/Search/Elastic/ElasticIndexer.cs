using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Nest;
using Newtonsoft.Json;
using Quest.Lib.Utils;
using Quest.Common.Messages;
using Quest.Lib.Trace;

namespace Quest.Lib.Search.Elastic
{
    /// <summary>
    /// Base class for anything that wants to index data in elastic
    /// </summary>
    public abstract class ElasticIndexer: IElasticIndexer
    {
        public abstract void StartIndexing(BuildIndexSettings config);

        public static BulkRequest GetBulkRequest(BuildIndexSettings config)
        {
            var bulk = new BulkRequest(config.DefaultIndex)
            {
                Operations = new List<IBulkOperation>()
            };

            return bulk;
        }

        public static BulkRequest GetBulkRequest(string index)
        {
            var bulk = new BulkRequest(index)
            { 
                Operations = new List<IBulkOperation>()
            };

            return bulk;
        }

        public static bool CommitCheck(object owner, BuildIndexSettings config, BulkRequest request, bool silent = false)
        {
            if (config.RecordsCurrent > 0 && config.StartedIndexing == DateTime.MinValue)
                config.StartedIndexing = DateTime.UtcNow;

            if (((IBulkRequest) request).Operations.Count >= config.Logfrequency)
                return CommitBultRequest(config, request);

            if ((config.RecordsCurrent % config.Logfrequency) == 0)
            {
                config.RecordsPerSecond = 0;
                // calc ETA
                if (config.RecordsCurrent > 0 && config.RecordsTotal > 0)
                {
                    var seconds = (DateTime.UtcNow - config.StartedIndexing).TotalSeconds;
                    if (Math.Abs(seconds) < 0.1)
                        config.RecordsPerSecond = 0;
                    else
                        config.RecordsPerSecond = (int)(config.RecordsCurrent/seconds);
                    var recordsRemaining = config.RecordsTotal - config.RecordsCurrent;
                    if (recordsRemaining > 0 && config.RecordsPerSecond>0)
                    {
                        double remainingTime = (double)recordsRemaining / (double)config.RecordsPerSecond;
                        if (!double.IsPositiveInfinity(remainingTime))
                            config.EstimateCompleteIndexing = DateTime.UtcNow.AddSeconds(remainingTime);
                    }
                }

                if (!silent)
                    OutputProgressLogMessage(owner, config);
            }

            return false;
        }

        internal static void OutputProgressLogMessage(object owner, BuildIndexSettings config)
        {
            //Logger.Write($"{indexer.GetType().Name}: Indexed: {config.Indexed}/{config.RecordsTotal} Skipped: {config.Skipped} Errors: {config.Errors}  ({(int)(config.RecordsCurrent * 100 / total)}%)");

            if (config.EstimateCompleteIndexing != DateTime.MinValue)
                Logger.Write($"{owner.GetType().Name}: #{config.RecordsCurrent}/{config.RecordsTotal} Skipped: {config.Skipped} Errors: {config.Errors}  ({(int)(config.RecordsCurrent * 100 / config.RecordsTotal)}%) ETA {config.EstimateCompleteIndexing} r/s={config.RecordsPerSecond}", "ElasticIndexer");
            else
                Logger.Write($"{owner.GetType().Name}: #{config.RecordsCurrent} Indexed: {config.Indexed}/{config.RecordsTotal}", "ElasticIndexer");
        }

        public static bool CommitBultRequest(BuildIndexSettings config, BulkRequest request)
        {
            var isValid = true;

            if (request.Operations == null)
                return false;

            if (((IBulkRequest)request).Operations.Count > 0)
            {
                lock (config)
                {
                    var result = config.Client.Bulk(request);
                    isValid = result.IsValid;
                    var errorCount = result.ItemsWithErrors.Count();
                    config.Errors += errorCount;
                    config.Indexed += result.Items.Count();
                    if (errorCount > 0)
                    {
                        Debug.Print("errors");
                    }

                }
            }
            request.Operations.Clear();
            return isValid;
        }


        /// <summary>
        ///     index an array of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="config"></param>
        public static void IndexItems<T>(T[] obj, BuildIndexSettings config) where T : class
        {
            var bulk = GetBulkRequest(config);

            foreach (var i in obj)
                bulk.Operations.Add(new BulkIndexOperation<T>(i));

            CommitBultRequest(config, bulk);
        }

        public static void AddIndexItem<T>(T obj, BulkRequest descriptor) where T : class
        {
            if (obj!=null)
                descriptor.Operations.Add(new BulkIndexOperation<T>(obj));
        }


        /// <summary>
        /// save a bulk request and create a new one
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public static PointGeoShape PointfromGeoLocation(GeoLocation loc)
        {
            return new PointGeoShape(new GeoCoordinate(loc.Latitude, loc.Longitude));
        }

        public static string Join(string description, string[] terms, bool firstonly, string separator = ", ")
        {
            if (terms.Length == 0)
                return description;
            if (firstonly)
                return description.ToUpper() + separator + terms[0];
            return description.ToUpper() + separator + string.Join(separator, terms);
        }

        internal static string Join(string[] terms, string separator = ", ")
        {
            var newterms = terms.Select(x => x.Trim().ToUpper()).Where(x => x.Length > 0).ToList();
            return string.Join(separator, newterms);
        }

        public static void DeleteDataSet<T>(string index, ElasticClient client, IndexBuilder.AddressDocumentType doctype) 
            where T: QuestDocument
        {
            ISearchResponse<T> deletedObjects;
            do
            {
                deletedObjects = client
                    .Search<T>(i => i
                    .Index(index)
                    .AllTypes()
                    .Query(f => f.Term(e => e.Field("type").Value(doctype.ToString())))
                    .Take(10000)
                    );

                if (deletedObjects.Hits.Any())
                {

                    var bulk = new BulkRequest(index)
                    {
                        Operations = new List<IBulkOperation>()
                    };

                    var items= deletedObjects.Hits.Select(x => new BulkDeleteOperation<T>(new Id(x.Id))).ToList();

                    foreach (var bulkDeleteOperation in items)
                    {
                        bulk.Operations.Add(bulkDeleteOperation);
                    }

                    var result = client.Bulk(bulk);
                }
            } while (deletedObjects.Hits.Any());
        }

        #region geofunctions

        public bool IsPointInRange(BuildIndexSettings config, double longitude, double latitude)
        {
            if (!config.RestrictToMaster)
                return true;                // yes, in range as we're not checking the master area

            return config.MasterArea.Search(longitude, latitude).Any();
        }

        public static string[] GetLocalAreas(BuildIndexSettings config, GeoLocation point)
        {
            return GetLocalAreas(point, config.LocalAreaNames);
        }

        public static string[] GetLocalAreas(GeoLocation point, PolygonManager localAreas)
        {
            var result = new string[] {};
            // find additional items
            var idxItems = localAreas.Search(point.Longitude, point.Latitude);
            idxItems.Reverse();
            if (idxItems.Count > 0)
                result = idxItems.Distinct().Select(x => ((string)x.data[0]).ToUpper().Trim()).ToArray();

            return result;
        }

        #endregion

        internal static string MakeRequest(string requestUrl)
        {
            try
            {
                var request = WebRequest.Create(requestUrl) as HttpWebRequest;

                if (request == null)
                    return null; 

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response == null)
                        return null;

                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception($"Server error (HTTP {response.StatusCode}: {response.StatusDescription}).");

                    using (var stream = response.GetResponseStream())
                    {
                        if (stream == null)
                            return null;

                        using (var readStream = new StreamReader(stream, Encoding.UTF8))
                            {
                                var text = readStream.ReadToEnd();
                                return text;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        internal static T MakeRequest<T>(string requestUrl) where T : class
        {
            T result;
            var txt = MakeRequest(requestUrl);

            try
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None
                };

                result = JsonConvert.DeserializeObject<T>(txt, settings);
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        /// <summary>
        ///     extract a range e.g. 12-25 or 34A-34B
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<int> ExtractRange(string text)
        {
            if (text == null)
                return null;

            const string numberMapping = "0987654321";
            var range = new List<int>();

            var parts = text.Split(' ');

            foreach (var p in parts)
            {
                var bit = p.Split('-');
                if (bit.Length == 2)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        var final = "";
                        foreach (var c in bit[i])
                            if (numberMapping.Contains(c))
                                final += c;

                        if (final.Length > 0)
                            range.Add(int.Parse(final));
                    }

                    if (range.Count == 2)
                        return range.ToList();
                }
            }
            return null;
        }


    }
}