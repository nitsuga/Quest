#pragma warning disable 0169,649
using System;
using System.Diagnostics;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Lib.Trace;
using Quest.Common.Messages;
using Quest.Common.ServiceBus;
using Quest.Lib.Utils;
using Quest.Common.Messages.Gazetteer;
using Quest.Common.Messages.Gazetteer.Gazetteer;

namespace Quest.Lib.Search.Elastic
{
    public class SearchManager : ServiceBusProcessor
    {
        #region Public Fields
        #endregion

        #region Private Fields
        private readonly IServiceProvider _container;
        private ISearchEngine _searchEngine;
        #endregion

        #region public Methods

        public SearchManager(
            ISearchEngine searchEngine,
            IServiceProvider container,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _searchEngine = searchEngine;
            _container =container;
        }

        protected override void OnPrepare()
        {
            MsgHandler.AddHandler<SearchRequest>(GazSearchRequestHandler);
            MsgHandler.AddHandler<InfoSearchRequest>(InfoSearchRequestHandler);
            MsgHandler.AddHandler<IndexGroupRequest>(GetIndexGroupsHandler);
        }
         
        protected override void OnStart()
        {
            Initialise();
        }

        protected override void OnStop()
        {
        }

        /// <summary>
        ///     initilaise the server with n workers
        /// </summary>
        private void Initialise()
        {
            Logger.Write("Search Manager Initialising", TraceEventType.Information, "Routing Manager");
        }

        #endregion

        #region Privates

        private SearchResponse GazSearchRequestHandler(NewMessageArgs t)
        {
            Logger.Write($"Executing GazSearchRequestHandler", TraceEventType.Information, GetType().Name);
            SearchRequest request = t.Payload as SearchRequest;
            var results = _searchEngine.SemanticSearch(request);
            Logger.Write($"GazSearchRequestHandler found {results.Documents.Count} documents", TraceEventType.Information, GetType().Name);
            return results;
        }
        private SearchResponse InfoSearchRequestHandler(NewMessageArgs t)
        {
            Logger.Write($"Executing InfoSearchRequestHandler", TraceEventType.Information, GetType().Name);
            InfoSearchRequest request = t.Payload as InfoSearchRequest;
            var results = _searchEngine.InfoSearch(request);
            Logger.Write($"GazSearchRequestHandler found {results.Documents.Count} documents", TraceEventType.Information, GetType().Name);
            return results;
        }

        private IndexGroupResponse GetIndexGroupsHandler(NewMessageArgs t)
        {
            try
            {
                Logger.Write($"Executing GetIndexGroupsHandler", TraceEventType.Information, GetType().Name);
                var result = _searchEngine.GetIndexGroups();
                Logger.Write($"GetIndexGroupsHandler returns {result.Groups.Count} groups", TraceEventType.Information, GetType().Name);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Write($"Executing GetIndexGroupsHandler failed: {ex}", TraceEventType.Information, GetType().Name);
                return null;
            }
        }

        #endregion
    }
}