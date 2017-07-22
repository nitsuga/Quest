#pragma warning disable 0169,649
//#define CALLVMDIRECT

using System.Collections.Generic;
using Quest.Common.Messages;
using System;

namespace Quest.Mobile.Service
{
    /// <summary>
    /// VisualsManager needs to be running for this to work.
    /// </summary>

    public class VisualisationService
    {

#if CALLVMDIRECT
        
        private VisualsManager _manager;
#endif


        public GetVisualsCatalogueResponse GetCatalogue(GetVisualsCatalogueRequest request)
        {
#if CALLVMDIRECT
            var args = new NewMessageArgs { Payload = request };
            return (GetVisualsCatalogueResponse)_manager.GetVisualsCatalogueHandler(args);
#else
            return MvcApplication.MsgClientCache.SendAndWait<GetVisualsCatalogueResponse>(request, new TimeSpan(0, 0, 0, 10));
#endif
        }

        public GetVisualsDataResponse GetVisualsData(List<string> visuals)
        {
            GetVisualsDataRequest request = new GetVisualsDataRequest() {Ids = visuals };
#if CALLVMDIRECT
            var args = new NewMessageArgs { Payload = request };
            return (GetVisualsDataResponse)_manager.GetVisualsData(args);
#else
            return MvcApplication.MsgClientCache.SendAndWait<GetVisualsDataResponse>(request, new TimeSpan(0, 0, 0, 10));
#endif
        }

        public QueryVisualResponse Query(string provider, string parameters)
        {
            QueryVisualRequest request = new QueryVisualRequest() { Provider = provider, Query = parameters};
#if CALLVMDIRECT
            var args = new NewMessageArgs { Payload = request };
            return (QueryVisualResponse)_manager.QueryVisual(args);
#else
            return MvcApplication.MsgClientCache.SendAndWait<QueryVisualResponse>(request, new TimeSpan(0, 0, 0, 20));
#endif
        }

    }
}