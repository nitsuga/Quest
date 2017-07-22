using Quest.Lib.Search.Elastic;

namespace Quest.Lib.Search
{
    public interface IElasticIndexer
    {
        void StartIndexing(BuildIndexSettings config);
    }
}
