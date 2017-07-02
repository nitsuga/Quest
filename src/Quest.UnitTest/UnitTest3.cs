using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Diagnostics;
using Quest.Lib.ServiceBus.Messages;
using Quest.Lib.Routing.Speeds;
using System.Collections.Generic;
using Quest.Lib.Job;
using Quest.Lib.Search.Elastic;

namespace Quest.UnitTest
{
    [TestClass]
    public class UnitTest3
    {
        private SearchEngine engine = new SearchEngine();

        [TestMethod]
        public void TestAddress()
        {
            var result = engine.SemanticSearch(new Lib.Search.SearchRequest()
            { searchText = "24 HIGH BEECHES", take = 1 });
            string expected = "Dangerous dog on premises";
            string actual = result.Documents[0].l.Description;
        }
    }
}
