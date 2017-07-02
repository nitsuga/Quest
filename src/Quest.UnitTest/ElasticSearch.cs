using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quest.Lib.Search.Elastic;
using Quest.Common.Messages;

namespace Quest.UnitTest
{
    [TestClass]
    public class ElasticSearch
    {
        [TestMethod]
        public void Semantic_Standard()
        {
            ElasticSettings settings = new ElasticSettings();
            SearchEngine engine = new SearchEngine(settings);
            SearchRequest baserequest = new SearchRequest()
            {
                searchText="sidcup high street",
                searchMode= SearchMode.EXACT,
                includeAggregates = false,
                skip=0,
                take=700
            };

            string[] addresses = new string[]
            {
                "QUEEN VICTORIA MEMORIAL",
                "QUEEN VICTORIA MEMORIAL, CITY OF WESTMINSTER",
                "TRAFALGAR SQUARE",
                "TRAFALGAR SQUARE, STRAND",
                "Eltham Crematorium, Eltham",
                "ELTHAM CREMATORIUM, ELTHAM CEMETERY, CROWN WOODS WAY, ELTHAM, SE9 2AZ",
                "Footscray Rugby Club",
                "FOOTSCRAY RUGBY SPORTS",
                "Danson Lake",
                "DANSON LAKE, DANSON PARK, DANSON ROAD, BEXLEYHEATH, DA6 8HL",
                "Battersea Dogs Home", "BATTERSEA DOGS HOME 4 BATTERSEA PARK ROAD",
                "Bromley Common, Bromley", "BROMLEY COMMON, BROMLEY COMMON",
                "Marble Arch", "MARBLE ARCH"
            };

            for(int i=1; i<10;i++)
                foreach (var a in addresses)
                {
                    baserequest.searchText = a;
                    engine.SemanticSearch(baserequest);
                }
        }
    }
}
