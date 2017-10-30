#pragma warning disable 0169,649
using System;
using Quest.Lib.ServiceBus;
using System.Threading.Tasks;
using Quest.Common.Messages.Gazetteer;
using Quest.Common.Messages.Gazetteer.Gazetteer;

namespace Quest.WebCore.Services
{
    public class SearchService
    {
        AsyncMessageCache _msgClientCache;

        public SearchService(AsyncMessageCache msgClientCache)
        {
            _msgClientCache = msgClientCache;
        }

        public async Task<SearchResponse> SimpleSearch(string location, string userName)
        {
            // get coords of start and end
            var fromRequest = new SearchRequest()
            {
                includeAggregates = false,
                searchMode = SearchMode.RELAXED,
                take = 1,
                skip = 0,
                searchText = location,
                box = null,
                filters = null,
                displayGroup = SearchResultDisplayGroup.none,
                username = userName,
                indexGroup = null,
            };

            var f = await SemanticSearch(fromRequest);
            return f;
        }

        public async Task<SearchResponse> SemanticSearch(SearchRequest request)
        {
            var result =await _msgClientCache.SendAndWaitAsync<SearchResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        public async Task<IndexResponse> Index(IndexRequest request)
        {
            var result = await _msgClientCache.SendAndWaitAsync<IndexResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        public async Task<SearchResponse> InfoSearch(InfoSearchRequest request)
        {
            var result = await _msgClientCache.SendAndWaitAsync<SearchResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        public async Task<IndexGroupResponse> GetIndexGroups()
        {
            IndexGroupRequest request = new IndexGroupRequest();
            var result = await _msgClientCache.SendAndWaitAsync<IndexGroupResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }
    }
}