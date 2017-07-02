using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quest.Lib.Search.Elastic;
using Quest.UnitTest.Properties;
using Quest.Lib.Simulation;
using Quest.Lib.MapMatching;
using Quest.Lib.Routing;
using System.Threading;
using System.Diagnostics;
using Quest.Lib.ServiceBus.Messages;
using System.Collections.Generic;
using Quest.Lib.Routing.Speeds;

namespace Quest.UnitTest
{
    [TestClass]
    public class UnitTest1
    {

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            Common.ClassInit(context);
        }

        [TestMethod]
        public void Index_FuzzyTest()
        {
            SearchEngine engine = new SearchEngine();

            String[] syns = new string[Settings.Default.Synonyms.Count];
            Settings.Default.Synonyms.CopyTo(syns, 0);

            //engine.TestIndex(syns);
        }

        [TestMethod]
        public void Index_SpeedImport()
        {
            SpeedImport si = new SpeedImport();
            si.ImportRoadSpeeds();
        }
        


            [TestMethod]
        public void LoadIncidentsFromXReplay()
        {
            XReplayLoader loader = new XReplayLoader();
            loader.Import();
        }

        [TestMethod]
        public void Index_Build()
        {
            SearchEngine engine = new SearchEngine();

            String[] syns = new string[Settings.Default.Synonyms.Count];
            Settings.Default.Synonyms.CopyTo(syns, 0);

            engine.CreateIndex(syns);
            engine.LoadDocuments( true, @"Data\local_area_names.csv", @"Data\master_4326.shp");
            engine.Optimize();
        }

        [TestMethod]
        public void Index_TestIOI()
        {
            SearchEngine engine = new SearchEngine();

            engine.BuildIoIs();

            var result = engine.InfoSearch(new Lib.Search.InfoSearchRequest() { distance = new Lib.Search.DistanceFilter() { lat = 51.5638, lng = -0.2974, distance = "10m" } });
        }


        [TestMethod]
        public void TestMethod2()
        {
            RoutingData data = Common.container.GetExport<RoutingData>().Value;
            data.StartupProgress += Data_StartupProgress;
            while (data.IsInitialised== false)
            {
                Thread.Sleep(1000);
            }
            IRouteEngine selectedRouteEngine = Common.container.GetExport<IRouteEngine>().Value;
            MapMatcherManager.RoadMatcherAllCommandActionWorker(selectedRouteEngine, data, 25, 10, 15, 200, 12);
        }

        [TestMethod]
        public void TestRouting()
        {
            RoutingData data = Common.container.GetExport<RoutingData>().Value;
            data.StartupProgress += Data_StartupProgress;
            while (data.IsInitialised == false)
            {
                Thread.Sleep(1000);
            }
            IRouteEngine selectedRouteEngine = Common.container.GetExport<IRouteEngine>().Value;

            var request = new RouteRequestMultiple()
            {
                DistanceMax = int.MaxValue,
                DurationMax = int.MaxValue,
                InstanceMax = 1,
                StartLocation = new RoutingPoint(520100,151800),
                HourOfWeek = 9,
                MakeRoute = true,
                SearchType = SearchType.Quickest,
                EndLocations = new List<RoutingLocation> { new RoutingLocation(520000, 151999) },
                VehicleType =  "AEU",
                 RoadSpeedCalculator= new VariableSpeedCalculator()
            };

            var result = selectedRouteEngine.CalculateRouteMultiple(request);
        }


        private void Data_StartupProgress(object sender, RouteEngineStatusArgs e)
        {
            Debug.Print(e.message + " "+ e.StartupProgress.ToString()); // 
        }

    }
}
