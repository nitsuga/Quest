using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quest.Lib.Search.Elastic;
using Quest.Common.Messages;

namespace Quest.UnitTest
{
    [TestClass]
    public class DataDrivenTests
    { 

        private TestContext m_testContext;
        public TestContext TestContext
        {

            get { return m_testContext; }
            set { m_testContext = value; }
        }
        public TestContext testContext { get; set; }

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Common.ClassInit(context);
        }


        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\testdata.csv", "testdata#csv", DataAccessMethod.Sequential), DeploymentItem("testdata.csv"), TestMethod]
        public void TestAddress()
        {
            ElasticSettings settings = new ElasticSettings();
            SearchEngine engine = new SearchEngine(settings);
            var searchtext = TestContext.DataRow["searchtext"].ToString();
            //string searchtext = "footscray rugby club";
            if (searchtext.Substring(0,1)!="#")  //comments in test data
            {
                //run a search using the data in the csv file
                var result = engine.SemanticSearch(new SearchRequest { searchText = searchtext, take = 700, searchMode = SearchMode.EXACT, displayGroup=SearchResultDisplayGroup.description });
                //take the best match
                string actual;
                if (result != null && result.Documents.Count > 0)
                {
                    SearchHit best;
                    var docs=result.Documents.Where(w => w.s != 10).Select(s => s).ToList(); //ignore score of 10 for area
                
                    if (docs.Count > 0)
                    {
                        best = docs.OrderByDescending(x => x.s).First();
                        actual = best.l.Description;
                    }
                    else
                        actual = "";
                  
                    //ignore commas and double spaces
                    actual = actual.Replace(",", "").Replace("  ", " ");
                }
                else
                {
                    actual = " ";
                }

                //get the expected result from the csv file
                var expected = TestContext.DataRow["expectedresult"].ToString();
                expected = expected.Replace(",", "");

                var matched = actual.Contains(expected);
                Debug.Print($"{matched}: <{actual}> <{expected}>");
                if (!matched) Console.WriteLine("<{3}> : {0}: expected <{1}> actual <{2}>", matched, expected, actual, searchtext);
                //check that they match
                // try  {
                Assert.AreEqual(true, matched, $"Expected <{expected}> Actual <{actual}>");
               // } catch { }
            
            }
        }

        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\testdatafuzzy.csv", "testdatafuzzy#csv", DataAccessMethod.Sequential), DeploymentItem("testdatafuzzy.csv"), TestMethod]
        public void TestFuzzyAddress()
        {
            ElasticSettings settings = new ElasticSettings();
            SearchEngine engine = new SearchEngine(settings);
            var searchtext = TestContext.DataRow["searchtext"].ToString();
            if (searchtext.Substring(0, 1) != "#")  //comments in test data
            {
                //run a search using the data in the csv file
                var result = engine.SemanticSearch(new SearchRequest { searchText = searchtext, take = 20, searchMode = SearchMode.FUZZY, displayGroup = SearchResultDisplayGroup.description });

                var actual="";
                var matched = false;

                //get the expected result from the csv file
                var expected = TestContext.DataRow["expectedresult"].ToString();
                expected = expected.Replace(",", "");

                if (result != null && result.Documents.Count > 0)
                {
                    foreach (var doc in result.Documents)
                    {
                        actual = doc.l.Description;

                        //ignore commas and double spaces
                        actual = actual.Replace(",", "").Replace("  ", " ");
                        if (actual.Contains(expected)) matched = true;
                    }
                }

                Debug.Print($"{matched}: <{actual}> <{expected}>");
                if (!matched) Console.WriteLine($"{matched}: expected <{expected}> actual <{actual}>");
                //check that they match
                // try  {
                Assert.AreEqual(true, matched, $"Expected <{expected}> Actual <{actual}>");
                // } catch { }

            }
        }

    }
}
