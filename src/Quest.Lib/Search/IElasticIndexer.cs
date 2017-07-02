using Quest.Lib.Search.Elastic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Lib.Search
{
    public interface IElasticIndexer
    {
        void StartIndexing(BuildIndexSettings config);
    }
}
