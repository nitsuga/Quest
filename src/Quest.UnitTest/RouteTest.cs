using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading;
using GeoAPI.Geometries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quest.Lib.Job;
using Quest.Lib.MapMatching;
using Quest.Lib.MapMatching.HMMViterbi;
using Quest.Lib.Routing;
using Quest.Lib.Routing.Speeds;
using Quest.Lib.ServiceBus.Messages;

namespace Quest.UnitTest
{
    [TestClass]
    public class RouteTest
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Common.ClassInit(context);
        }

        [TestMethod]
        public void AnalyseEdgeCosts()
        {
            var export = Common.Container.GetExport<AnalyseEdgeCosts>();
            if (export != null)
            {
                var analyseEdgeCosts = export.Value;
                analyseEdgeCosts.Analyse();
            }
        }

        [TestMethod]
        public void AnalyseNearestRoads()
        {
            MapMatcherManager.AnalyseNearestRoads();
        }

        [TestMethod]
        public void AnalyseTrackSpeeds()
        {
            MapMatcherManager.AnalyseTrackSpeeds();
        }


        /// <summary>
        /// Trial emmission and transition settings on a cohort of routes and detect
        /// output of high speeds. 
        /// </summary>
        [TestMethod]

        public void RouteTest_Matching_MinSpeed()
        {
            int[] incRoutes = new int[]
            {
                683422, 1066255, 968952, 732038, 1216348, 1146839, 1193202, 1177811, 789144, 913767, 950051, 747381, 980668,
                1125969, 707470, 1018942, 1104343, 1218054, 1133202, 896503
            };

            foreach (var tid in incRoutes)
            {
                // get a sample track to process
                var track = MapMatcherManager.GetIncidentRouteTrack(tid);

                track = track.RemoveCloseFixes(12, 5);

                for (var transitionBeta = 0.01; transitionBeta < 4; transitionBeta += 0.5)
                {
                    try
                    {
                        dynamic parameters = new ExpandoObject();

                        parameters.Emission = "GpsEmission";
                        parameters.EmissionP1 = 0;
                        parameters.EmissionP2 = 0;

                        parameters.Transition = "Exponential";
                        parameters.TransitionP1 = transitionBeta;

                        parameters.MaxRoutes = 15;
                        parameters.RoadGeometryRange = 50;
                        parameters.RoadEndpointEnvelope = 50;

                        HmmParameters p = ExpandoUtil.SetFromExpando<HmmParameters>(parameters);

                        var request = new MapMatcherMatchSingleRequest()
                        {
                            RoutingEngine = "Dijkstra",
                            MapMatcher = "HmmViterbiMapMatcher",
                            RoutingData = "Standard",
                            Fixes = track.Fixes,
                            Parameters = parameters
                        };

                        var response = MapMatcherManager.MapMatcherMatchSingle(Common.Container, request);

                        if (response != null)
                        {
                            var distance = response.Result.Results.Sum(x => (x.Distance));
                            //var distance2 = response.Result.Route.Length;
                            var totalspeed = response.Result.Results.Sum(x => (x.SpeedMs));
                            var maxspeed = response.Result.Results.Max(x => (x.SpeedMs));
                            Debug.Print($"{tid}\t{transitionBeta}\t{totalspeed}\t{maxspeed}\t{distance}");
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

        }

        /// <summary>
        /// Use this to test various emmssion and transitions on a single route
        /// </summary>
        [TestMethod]
        public void RouteTest_Matching_Parameters()
        {
            // get a sample track to process
            Track track = MapMatcherManager.GetIncidentRouteTrack(68342212);

            track = track.RemoveCloseFixes(12, 5);

            // filter out the samples you're interested in
            var filteredFixes = track.Fixes.Skip(2).Take(7).ToList();

            for (var emissionAlpha = 0.0; emissionAlpha < 2; emissionAlpha += 0.1)
            {
                for (var transitionBeta = 0.0; transitionBeta < 2; transitionBeta += 0.1)
                {
                    try
                    {
                        dynamic parameters = new ExpandoObject();

                        parameters.Emission = "Exponential";
                        parameters.EmissionP1 = emissionAlpha;
                        parameters.EmissionP2 = 0;

                        parameters.Transition = "Exponential";
                        parameters.TransitionP1 = transitionBeta;

                        parameters.MaxRoutes = 25;
                        parameters.RoadGeometryRange = 100;
                        parameters.RoadEndpointEnvelope = 100;
                        
                        HmmParameters p = ExpandoUtil.SetFromExpando<HmmParameters>(parameters);

                        var request = new MapMatcherMatchSingleRequest()
                        {
                            RoutingEngine = "Dijkstra",
                            //MapMatcher = "ParticleFilterMapMatcher",
                            MapMatcher = "HmmViterbiMapMatcher",
                            RoutingData = "Standard",
                            Fixes = filteredFixes,
                            Parameters = parameters
                        };

                        var response = MapMatcherManager.MapMatcherMatchSingle(Common.Container, request);

                        if (response != null)
                        {
                            var distance = response.Result.Results.Sum(x => (x.Distance));
                            //var distance2 = response.Result.Route.Length;
                            var totalspeed = response.Result.Results.Sum(x => (x.SpeedMs));
                            var maxspeed = response.Result.Results.Max(x => (x.SpeedMs));
                            Debug.Print($"{emissionAlpha}\t{transitionBeta}\t{(int)totalspeed}\t{(int)maxspeed}\t{distance} ");
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                }

            }
        }

        /// <summary>
        /// Use this to test a specific route
        /// </summary>
        [TestMethod]
        public void RouteTest_Matching_Single()
        {
            // get a sample track to process
            Track track = MapMatcherManager.GetIncidentRouteTrack(683422);

            track = track.RemoveCloseFixes(12, 5);


            // filter out the samples you're interested in
            var filteredFixes = track.Fixes.Skip(2).Take(7).ToList();


            try
            {
                dynamic parameters = new ExpandoObject();

                parameters.Emission = "Exponential";
                parameters.EmissionP1 = 1;
                parameters.EmissionP2 = 0;

                parameters.Transition = "Exponential";
                parameters.TransitionP1 = 1;

                parameters.MaxRoutes = 5;
                parameters.RoadGeometryRange = 50;
                parameters.RoadEndpointEnvelope = 50;

                parameters.GenerateGraphVis = true;

                HmmParameters p = ExpandoUtil.SetFromExpando<HmmParameters>(parameters);

                var request = new MapMatcherMatchSingleRequest()
                {
                    RoutingEngine = "Dijkstra",
                    //MapMatcher = "ParticleFilterMapMatcher",
                    MapMatcher = "HmmViterbiMapMatcher",
                    RoutingData = "Standard",
                    Fixes = filteredFixes,
                    Parameters = parameters
                };

                var response = MapMatcherManager.MapMatcherMatchSingle(Common.Container, request);

                if (response != null)
                {
                    var distance = response.Result.Results.Sum(x => (x.Distance));
                    //var distance2 = response.Result.Route.Length;
                    var totalspeed = response.Result.Results.Sum(x => (x.SpeedMs));
                    var maxspeed = response.Result.Results.Max(x => (x.SpeedMs));

                    Debug.Print($"{parameters.EmissionP1}\t{parameters.TransitionP1}\t{(int) totalspeed}\t{(int) maxspeed}\t{distance} ");

                    Debug.Print($"{response.Result.GraphVis} ");
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void Data_StartupProgress(object sender, RouteEngineStatusArgs e)
        {
            Debug.Print(e.Message + " " + e.StartupProgress); // 
        }

        [TestMethod]
        public void RouteTest_SingleRoute()
        {
            var speedData = Common.Container.GetExportedValues<SpeedMatrixLoader>();

            var data = Common.Container.GetExportedValue<RoutingData>();
            while (data.IsInitialised == false)
            {
                Thread.Sleep(1000);
            }

            

            var speedCalc = Common.Container.GetExportedValue<IRoadSpeedCalculator>("VariableSpeedCalculator");

            var export = Common.Container.GetExport<IRouteEngine>("Dijkstra");
            if (export != null)
            {
                var selectedRouteEngine = export.Value;

                var request = new RouteRequestMultiple
                {
                    DistanceMax = int.MaxValue,
                    DurationMax = int.MaxValue,
                    InstanceMax = 1,
                    StartLocation = data.GetEdgeFromPoint(new Coordinate(525758, 192409)),
                    HourOfWeek = 9,
                    SearchType = SearchType.Quickest,
                    EndLocations = new List<EdgeWithOffset> { data.GetEdgeFromPoint(new Coordinate(524535, 197018)) },
                    VehicleType = "AEU",
                    RoadSpeedCalculator = speedCalc
                };

                var calculateRouteMultiple = selectedRouteEngine.CalculateRouteMultiple(request);
                if (calculateRouteMultiple == null) throw new ArgumentNullException(nameof(calculateRouteMultiple));
            }
        }

    }
}
