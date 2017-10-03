#pragma warning disable 0169,649
//#define CALLVMDIRECT

using System.Collections.Generic;
using Quest.Common.Messages;
using System;
using Quest.Lib.ServiceBus;
using System.Threading.Tasks;

namespace Quest.WebCore.Services
{
    /// <summary>
    /// VisualsManager needs to be running for this to work.
    /// </summary>


    public class VisualisationService
    {
        AsyncMessageCache _msgClientCache;

        public VisualisationService(AsyncMessageCache msgClientCache)
        {
            _msgClientCache = msgClientCache;
        }

#if CALLVMDIRECT
        
        private VisualsManager _manager;
#endif


        public async Task<GetVisualsCatalogueResponse> GetCatalogue(GetVisualsCatalogueRequest request)
        {
#if CALLVMDIRECT
            var args = new NewMessageArgs { Payload = request };
            return (GetVisualsCatalogueResponse)_manager.GetVisualsCatalogueHandler(args);
#else
            return await _msgClientCache.SendAndWaitAsync<GetVisualsCatalogueResponse>(request, new TimeSpan(0, 0, 0, 10));
#endif
        }

        public async Task<GetVisualsDataResponse> GetVisualsData(List<string> visuals)
        {
            GetVisualsDataRequest request = new GetVisualsDataRequest() {Ids = visuals };
#if CALLVMDIRECT
            var args = new NewMessageArgs { Payload = request };
            return (GetVisualsDataResponse)_manager.GetVisualsData(args);
#else
            return await _msgClientCache.SendAndWaitAsync<GetVisualsDataResponse>(request, new TimeSpan(0, 0, 0, 10));
#endif
        }

        public async Task<QueryVisualResponse> Query(string provider, string parameters)
        {
            QueryVisualRequest request = new QueryVisualRequest() { Provider = provider, Query = parameters};
#if CALLVMDIRECT
            var args = new NewMessageArgs { Payload = request };
            return (QueryVisualResponse)_manager.QueryVisual(args);
#else
            return await _msgClientCache.SendAndWaitAsync<QueryVisualResponse>(request, new TimeSpan(0, 0, 0, 10));
#endif
        }

    }
}