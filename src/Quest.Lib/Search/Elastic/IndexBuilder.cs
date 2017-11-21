using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using Quest.Lib.Trace;
using Quest.Common.Messages.Gazetteer;

namespace Quest.Lib.Search.Elastic
{
    public static class IndexBuilder
    {
        #region private variables

        #endregion

        /// <summary>
        /// create a history index. Fails if the index already exists and force=false
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="force"></param>
        public static void CreateHistoryIndex(ElasticSettings settings, ElasticClient client, bool force)
        {
            if (client.IndexExists(ElasticSettings.Historyindex).Exists)
                if (force)
                    client.DeleteIndex(ElasticSettings.Historyindex);
                else
                    return;

            var indexState = new IndexState { Settings = new IndexSettings { NumberOfShards = 1, NumberOfReplicas = 1 } };

            var result = client.CreateIndex(ElasticSettings.Historyindex, s => s
                .InitializeUsing(indexState)
                .Mappings(ms => ms
                    .Map<Common.Messages.Gazetteer.SearchRequest>((TypeMappingDescriptor<Common.Messages.Gazetteer.SearchRequest> m) => m
                        .AutoMap()
                        .AllField(a => a.Enabled(false))
                    ))
                );


            //chuck an exception if result not valid - by default there is no exception raised when REST request fails.
            if (!result.IsValid)
                throw new Exception(result.DebugInformation + "\n" + result.ServerError);

        }

        /// <summary>
        /// Create a new document index and return the name of the index
        /// </summary>
        /// <param name="request.Index"></param>
        /// <param name="synonyms"></param>
        /// <param name="config"></param>
        /// <param name="request.Shards"></param>
        /// <param name="request.Replicas"></param>
        /// <returns></returns>
        public static void CreateDocIndex(BuildIndexSettings config, string Index, string[] synonyms, int Shards, int Replicas)
        {
            // https://github.com/elastic/elasticsearch-analysis-phonetic

            Func<CustomAnalyzerDescriptor, ICustomAnalyzer> addressAnalyzer = a => a
                .Filters(new List<string>
                {
                    "lowercase",
                    "address_synonyms",
                    "english_possessive_stemmer",
                    "english_stop",
                    "english_stem",
                    "phonetic_filter"
                })
                .Tokenizer("standard");


            Func<CustomAnalyzerDescriptor, ICustomAnalyzer> strictAnalyzer = a => a
                .Filters(new List<string>
                {
                    "lowercase",
                    "address_synonyms",
                    "english_possessive_stemmer",
                    "english_stop",
                    "english_stem"
                })
                .Tokenizer("standard");

            Func<CustomAnalyzerDescriptor, ICustomAnalyzer> searchStrictAnalyzer = a => a
                .Filters(new List<string>
                {
                    "lowercase",
                    "address_synonyms",
                    "english_possessive_stemmer",
                    "english_stop",
                    "english_stem"
                })
                .Tokenizer("standard");


            Func<CustomAnalyzerDescriptor, ICustomAnalyzer> searchAddressAnalyzer = a => a
                .Filters(new List<string>
                {
                    "lowercase",
                    "address_synonyms",
                    "english_possessive_stemmer",
                    "english_stop",
                    "english_stem",
                    "phonetic_filter"
                })
                .Tokenizer("standard");


            Func<CustomAnalyzerDescriptor, ICustomAnalyzer> testAnalyzer = a => a
                .Filters(new List<string>
                {
                    "engram",
                })
                .Tokenizer("standard");


            var indexState = new IndexState
            {
                Settings = new IndexSettings
                {
                    NumberOfShards = Shards,
                    NumberOfReplicas= Replicas
                }
            };

            try
            {
                config.Client.DeleteIndex(Index);
            }
            catch
            {
                // ignored
            }


            var result = config.Client.CreateIndex(Index, s => s
                .Settings(settings => settings
                .NumberOfShards(Shards)
                .NumberOfReplicas(Replicas)
                .Analysis(an => an
                    .TokenFilters(x => x
                        .Synonym("address_synonyms", asn => asn.Synonyms(synonyms).IgnoreCase())
                        .Phonetic("phonetic_filter", pn => pn.Encoder(PhoneticEncoder.Caverphone2).Replace(false))
                        .Stemmer("english_possessive_stemmer", sf => sf.Language("possessive_english"))
                        .Stemmer("english_stem", sf => sf.Language("light_english"))
                        .Unique("unique", sf => sf.OnlyOnSamePosition(true))
                        .EdgeNGram("engram", sf => sf.MinGram(2).MaxGram(10))
                        .Stop("english_stop", ss => ss.StopWords(new StopWords("_english_")))
                    )
                    .Analyzers(bases => bases
                        .Custom("address_analyser", addressAnalyzer)
                        .Custom("strict_analyser", strictAnalyzer)
                        .Custom("search_strict_analyser", searchStrictAnalyzer)
                        .Custom("search_adress_analyser", searchAddressAnalyzer)
                        .Custom("testAnalyzer", testAnalyzer)
                        
                        )))
                .Mappings(ms => ms
                    .Map<LocationDocument>(m => m
                        .AutoMap()
                        .Properties(p => p
                            .Text(str => str
                                .Name("indextext")
                                .Fields(f => f
                                    .Text(
                                        st =>
                                            st.Name(o => o.indextext.Suffix("address"))
                                                .Analyzer("address_analyser"))
                                    .Text(
                                        st =>
                                            st.Name(o => o.indextext.Suffix("strict"))
                                                .Analyzer("strict_analyser"))
                                )
                            )
                            .GeoPoint(g => g
                                .Name(xg => xg.Location)
                                //.LatLon()
                                //.GeoHash()
                                //.GeoHashPrecision(1)
                            )
                            .GeoShape(g => g
                                .Name(xg => xg.Point)
                                .TreeLevels(10)
                                .PointsOnly()
                                .Precision(1, DistanceUnit.Meters)
                            )
                        )
                    )
                )
                );

            var rr = config.Client.UpdateIndexSettings(Index, x => x.IndexSettings(z => z.NumberOfReplicas(Replicas)));

            if (!result.IsValid)
                throw new Exception(result.DebugInformation + "\n" + result.ServerError);

        }


        /// <summary>
        /// build a new index given a listr of indexers
        /// </summary>
        /// <param name="request.Index"></param>
        /// <param name="indexers"></param>
        /// <param name="statusCallback"></param>
        /// <param name="indexmode"></param>
        /// <param name="fromIndex"></param>
        /// <param name="request.Shards"></param>
        /// <param name="request.Replicas"></param>
        /// <param name="useMaster"></param>
        /// <param name="parameters"></param>
        public static void BuildNewIndex(ElasticSettings settings, IndexRequest request, List<IElasticIndexer> indexers, IDictionary<string, object> parameters)
        {
            // create an index and set it as the defualt index
            if (string.IsNullOrEmpty(request.Index))
            {
                Logger.Write($"No index name found", "IndexBuilder");
                request.Index = ElasticSettings.DefaultDocindex + "_" + DateTime.UtcNow.ToString("yyMMddHHmm");
                Logger.Write($"Using {request.Index} for the index name","IndexBuilder");
            }


            Logger.Write($"Loading synonyms from {settings.SynonymsFile}", "IndexBuilder");
            var syns = LoadSynsFromFile(settings.SynonymsFile);

            BuildIndexSettings config = new BuildIndexSettings(settings, request.Index, parameters);
            config.RestrictToMaster = request.UseMaster;

            switch (request.IndexMode.Trim().ToLower())
            {
                case "replicas":
                    var rr = config.Client.UpdateIndexSettings(request.Index, x => x.IndexSettings(z => z.NumberOfReplicas(request.Replicas)));
                    Logger.Write($"Replicas for index {request.Index} set to {request.Replicas}: valid: {rr.IsValid} error: {rr.ServerError}","IndexBuilder");
                    break;

                case "merge":
                    //config.Settings.Client.ReindexOnServer(fromIndex , request.Index, r => r
                    //     // settings to use when creating to-index
                    //     .CreateIndex(c => c
                    //         .Settings(s => s
                    //             .request.Shards(5)
                    //             .request.Replicas(2)
                    //         )
                    //     ));

                    break;
                case "delete":
                    config.Client.DeleteIndex(request.Index);
                    Logger.Write($"Deleted index {request.Index}", "IndexBuilder");
                    break;

                case "create":
                    // create an index to put records in             
                    Logger.Write($"Creating index {request.Index} ", "IndexBuilder");

                    CreateDocIndex(config, request.Index, syns, request.Shards, request.Replicas);
                    Logger.Write($"Created index {request.Index} ", "IndexBuilder");
                    break;
            }

            if (indexers.Any())
            {
#if parallel
                ExecuteParallel(config, request.Index, indexers, statusCallback, useMaster);

#else
                ExecuteSequential(config, request.Index, indexers, request.UseMaster, parameters);
#endif
            }
        }

        internal static void ExecuteParallel(BuildIndexSettings config, string Index,
            List<IElasticIndexer> indexers, bool useMaster, Dictionary<string, object> parameters)
        {

            // create set of tasks to do indexing
            Task[] alltasks = indexers.Select(x => new Task(() =>
            {
                try
                {
                    x.StartIndexing(config);
                }
                catch (Exception ex)
                {
                    Logger.Write($"{x.GetType().Name} Failed: {ex}", "IndexBuilder");
                }
            }
            )).ToArray();


            // start indexing 
            alltasks.ToList().ForEach(x =>
            {
                Logger.Write($"Started {x.GetType().Name}", "IndexBuilder");

                x.Start();

                x.ContinueWith(y =>
                {
                    Logger.Write($"Completed {x.GetType().Name} Processed:{config.RecordsTotal} Errors: {config.Errors} Skipped: {config.Skipped} Indexed: {config.Indexed}", "IndexBuilder");
                });
            }
                );

            Logger.Write($"Started {alltasks.Length} indexer tasks", "IndexBuilder");

            // wait until complete
            Task.WaitAll(alltasks);

            if (alltasks.Length > 1)
                Logger.Write($"{alltasks.Length} indexer tasks complete", "IndexBuilder");

            //Logger.Write($"Optimising index");
            Optimize(config);
        }

        internal static void ExecuteSequential(
                BuildIndexSettings config, 
                string Index, 
                List<IElasticIndexer> indexers,
                bool useMaster, 
                IDictionary<string, object> parameters)
        {

            foreach (var indexer in indexers)
            {
                try
                {
                    config = new BuildIndexSettings(config.Settings, Index, parameters);
                    config.RestrictToMaster = useMaster;
                    indexer.StartIndexing(config);
                }
                catch (Exception ex)
                {
                    Logger.Write($"{indexer.GetType().Name} Failed: {ex}", "IndexBuilder");
                }
                Logger.Write($"Completed {indexer.GetType().Name} Processed:{config.RecordsTotal} Errors: {config.Errors} Skipped: {config.Skipped} Indexed: {config.Indexed}", "IndexBuilder");
            }
        }

        /// <summary>
        ///     load synonyms from a synonyms file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string[] LoadSynsFromFile(string filename)
        {
            try
            {
                if (string.IsNullOrEmpty(filename))
                {
                    Logger.Write("No Synonyms file","IndexBuilder");
                    return new string[] {};
                }
                var txt = File.ReadAllText(filename);
                var syns = txt.Split('\n');
                return syns;
            }
            catch
            {
                // ignored
            }

            return null;
        }


        /// <summary>
        ///     index any class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="client"></param>
        public static void IndexItem<T>(T obj, ElasticClient client, string index) where T : class
        {
            client.Index(obj, x=>x.Index(index));
            client.Refresh(new RefreshRequest(ElasticSettings.DefaultDocindex));
        }


        public static void Optimize(BuildIndexSettings config)
        {
        }

        public sealed class AddressDocumentType
        {
            public static readonly AddressDocumentType RoadPoint = new AddressDocumentType(1, "RoadPoint");
            public static readonly AddressDocumentType Bus = new AddressDocumentType(2, "BUS");
            public static readonly AddressDocumentType RoadLink = new AddressDocumentType(3, "RoadLink");
            public static readonly AddressDocumentType Junction = new AddressDocumentType(4, "Junction");
            public static readonly AddressDocumentType Address = new AddressDocumentType(1, "Address");
            public static readonly AddressDocumentType Poi = new AddressDocumentType(5, "POI");
            public static readonly AddressDocumentType MarkerPost = new AddressDocumentType(6, "MarkerPost");
            public static readonly AddressDocumentType Bike = new AddressDocumentType(7, "Bike");
            public static readonly AddressDocumentType CodePoint = new AddressDocumentType(8, "CodePoint");
            public static readonly AddressDocumentType Osm = new AddressDocumentType(9, "OSM");
            public static readonly AddressDocumentType Camera = new AddressDocumentType(10, "Camera");
            public static readonly AddressDocumentType StopPoint = new AddressDocumentType(11, "StopPoint");
            public static readonly AddressDocumentType TubeLine = new AddressDocumentType(12, "TubeLine");
            public static readonly AddressDocumentType Overlay = new AddressDocumentType(13, "Overlay");
            public static readonly AddressDocumentType LocalName = new AddressDocumentType(14, "LocalName");
            public static readonly AddressDocumentType Rail = new AddressDocumentType(15, "Rail");
            public static readonly AddressDocumentType Defib = new AddressDocumentType(15, "Defib");

            private readonly string _name;
            private readonly int _value;

            private AddressDocumentType(int value, string name)
            {
                _name = name;
                _value = value;
            }

            public override string ToString()
            {
                return _name;
            }

            public static implicit operator string(AddressDocumentType ad)
            {
                return ad._name;
            }
        }

    }
}