#pragma warning disable 0169,649
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Quest.Lib.ServiceBus;
using Quest.Lib.Processor;
using Quest.Lib.Trace;
using Quest.Common.Messages;
using Autofac;
using Quest.Common.ServiceBus;
using Quest.Lib.Utils;

namespace Quest.Lib.Search.Elastic
{
    public class IndexerManager : ServiceBusProcessor
    {
        #region Private Fields
        private ILifetimeScope _scope;
        private ISearchEngine _searchEngine;
        private ElasticSettings _settings;
        #endregion

        public IndexerManager(
            ElasticSettings settings,
            ISearchEngine searchEngine,
            ILifetimeScope scope,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _searchEngine = searchEngine;
            _scope = scope;
            _settings = settings;
        }
        protected override void OnPrepare()
        {
            MsgHandler.AddHandler<IndexRequest>(IndexRequestHandler);
        }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        private IndexResponse IndexRequestHandler(NewMessageArgs t)
        {
            IndexRequest request = t.Payload as IndexRequest;
            if (request!=null)
            {
                return StartIndexing(request);
            }
            return null;
        }

        /// <summary>
        /// start indexing using the list of indexers
        /// </summary>
        /// <param name="data"></param>
        /// <param name="statusCallback"></param>
        public IndexResponse StartIndexing(IndexRequest request)
        {           
            // build list of indexers from names provided
            List<IElasticIndexer> indexers = GetIndexersFromString(request.Indexers);

            Dictionary<string, object> parameters = new  Dictionary<string, object>();
            // execute index process
            IndexBuilder.BuildNewIndex(_settings, request, indexers, parameters);

            Logger.Write($"Indexing complete", TraceEventType.Error, GetType().Name);

            return new IndexResponse();
        }

        List<IElasticIndexer> GetIndexersFromString(string text)
        {
            text = text.Replace("\n", "").Replace("\r", "");

            List<IElasticIndexer> indexers = new List<IElasticIndexer>();
            foreach (string indexer in text.Split('|'))
            {
                if (indexer.Length > 0)
                {
                    try
                    {
                        var export = _scope.ResolveNamed<IElasticIndexer>(indexer);
                        if (export != null)
                        {
                            var indexerModule = export;
                            indexers.Add(indexerModule);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Write($"Could not locate module {indexer} in MEF: {ex.Message}", TraceEventType.Error, GetType().Name);
                    }
                }
            }

            return indexers;
        }
    }
}