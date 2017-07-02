using Quest.Common.Messages;

namespace Quest.Lib.Search.Elastic
{
    public interface ISearchEngine
    {
        void Initialise();
        SearchResponse SemanticSearch(SearchRequest baserequest);
        SearchResponse InfoSearch(InfoSearchRequest baserequest);
        IndexGroupResponse GetIndexGroups();

    }
}