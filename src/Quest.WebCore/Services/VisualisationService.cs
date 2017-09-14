#pragma warning disable 0169,649
//#define CALLVMDIRECT

using System.Collections.Generic;
using Quest.Common.Messages;
using System;
using Quest.Lib.ServiceBus;

namespace Quest.WebCore.Services
{
    /// <summary>
    /// VisualsManager needs to be running for this to work.
    /// </summary>


    public class VisualisationService
    {
        MessageCache _msgClientCache;

        public VisualisationService(MessageCache msgClientCache)
        {
            _msgClientCache = msgClientCache;
        }

#if CALLVMDIRECT
        
        private VisualsManager _manager;
#endif


        public GetVisualsCatalogueResponse GetCatalogue(GetVisualsCatalogueRequest request)
        {
#if CALLVMDIRECT
            var args = new NewMessageArgs { Payload = request };
            return (GetVisualsCatalogueResponse)_manager.GetVisualsCatalogueHandler(args);
#else
            return _msgClientCache.SendAndWait<GetVisualsCatalogueResponse>(request, new TimeSpan(0, 0, 0, 10));
#endif
        }

        public GetVisualsDataResponse GetVisualsData(List<string> visuals)
        {
            GetVisualsDataRequest request = new GetVisualsDataRequest() {Ids = visuals };
#if CALLVMDIRECT
            var args = new NewMessageArgs { Payload = request };
            return (GetVisualsDataResponse)_manager.GetVisualsData(args);
#else
            return _msgClientCache.SendAndWait<GetVisualsDataResponse>(request, new TimeSpan(0, 0, 0, 10));
#endif
        }

        public QueryVisualResponse Query(string provider, string parameters)
        {
            QueryVisualRequest request = new QueryVisualRequest() { Provider = provider, Query = parameters};
#if CALLVMDIRECT
            var args = new NewMessageArgs { Payload = request };
            return (QueryVisualResponse)_manager.QueryVisual(args);
#else
            return _msgClientCache.SendAndWait<QueryVisualResponse>(request, new TimeSpan(0, 0, 0, 10));
#endif
        }

    }
}