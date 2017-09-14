#pragma warning disable 0169,649
using System;
using Quest.Common.Messages;
using Quest.Lib.ServiceBus;

namespace Quest.WebCore.Services
{
    public class SearchService
    {
        MessageCache _msgClientCache;

        public SearchService(MessageCache msgClientCache)
        {
            _msgClientCache = msgClientCache;
        }

        public SearchResponse SimpleSearch(string location, string userName)
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

            var f = SemanticSearch(fromRequest);
            return f;
        }

        public SearchResponse SemanticSearch(SearchRequest request)
        {
            var result = _msgClientCache.SendAndWait<SearchResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        public IndexResponse Index(IndexRequest request)
        {
            var result = _msgClientCache.SendAndWait<IndexResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        public SearchResponse InfoSearch(InfoSearchRequest request)
        {
            var result = _msgClientCache.SendAndWait<SearchResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        public IndexGroupResponse GetIndexGroups()
        {
            IndexGroupRequest request = new IndexGroupRequest();
            var result = _msgClientCache.SendAndWait<IndexGroupResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }
    }
}