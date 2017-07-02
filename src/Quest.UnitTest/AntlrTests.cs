using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Antlr4.Runtime;
using System.Diagnostics;
using Quest.Antlr;

namespace ALTNR.TEST
{
    [TestClass]
    public class AntlrTests
    {
        [TestMethod]
        public void AntlrTests_NestedNearSearchOperation()
        {
            //var expr = $"\"tesco\" near \"orpington high street\" near \"chip shop\"";
            var expr = $"\"tesco\" near \"epsom high street\" near \"kebab shop\"";
            var result = GazetteerSearchHelper.Evaluate(expr);
            
            foreach(var r in result.Documents)
            {
                Debug.WriteLine(r.l.Description);
            }

            Assert.IsNotNull(result.Documents);
        }

        [TestMethod]
        public void AntlrTests_SimpleSearchOperation()
        {
            //var expr = "\"Orlando\"";
            //var expr = "Orlando near epsom";
            //var expr = "123.45,567.678 near \"epsom\"";
            //var expr = "123.45";
            //var expr = "123.45,567.678";
            //var expr = "123.45,567.678 near epsom";
            //var expr = "123.45,567.678 near somple";

            //var expr = $"123.45,567.678 near \"somple\"";
            //var expr = $"show roads in \"chelsfield\"";
            var expr = $"\"tesco\" near \"orpington high street\" near \"chip shop\"";

            //var expr = $"\"tesco\" near \"orpington high street\" near \"high beeches\" / \"windsor drive\"";
            //var expr = $"\"high beeches\" / \"windsor drive\"";
            //var expr = $"tesco near orpington high street near chip shop";

            var result = GazetteerSearchHelper.Evaluate(expr);
           
        }
    }
}
