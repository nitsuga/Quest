using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using NetTopologySuite.Operation.Distance;
using Quest.Lib.MapMatching.RouteMatcher;
using Quest.Lib.Maths;
using Quest.Lib.Routing;
using Quest.Lib.Utils;
using Quest.Common.Utils;
using Quest.Common.Messages.Routing;
using Quest.Common.Messages.GIS;
using Quest.Common.Messages.Visual;
using Quest.Lib.Coords;

namespace Quest.Lib.MapMatching.HMMViterbi
{
    internal static class HmmUtil
    {
        public static RouteMatcherResponse BuildResponse(Step[] steps, List<RoadLinkEdgeSpeed> path, HmmParameters parameters, string name)
        {
            var response = new RouteMatcherResponse
            {

                GraphVis = parameters.GenerateGraphVis ? steps.PrintGraphVis() : "",
                Results = path,
                Fixes = new Visual
                {
                    Id = new VisualId
                    {
                        Source = "HMM_MM",
                        Id = $"{name}",
                        Name = $"HMM {name} Fixes",
                        VisualType = "Fixes"
                    },
                    Timeline = parameters.Fixes.Select(x => new TimelineData(
                        x.Timestamp.Ticks,
                        x.Timestamp,
                        null,
                        "",
                        ""
                        )).ToList(),
                    Geometry =
                        new FeatureCollection(
                            steps.Select(x => x.GetFixAsFeatureCollection("fix", "normal")).ToList())
                }
            };


            var particles = steps.SelectMany((x, y) => x.CandidateFixes).OrderBy(x => x.Sequence).ToList();
            var geoms = particles.Select(x => x.GetCandidateFixAsFeatureCollection()).ToList();


            response.Particles = new Visual
            {
                Id = new VisualId
                {
                    Source = "HMM_MM",
                    Name = $"{name}",
                    Id = $"HMM {name} Particles",
                    VisualType = "Particles"
                },
                //Timeline = particles.Select(x => new TimelineData(
                //    x.Timestamp.Ticks,
                //    x.Timestamp,
                //    null,
                //    "",
                //    ""
                //    )).GroupBy(x => x.ToString()).Select(x -).ToList(),
                Geometry = new FeatureCollection(geoms)
            };

            var rt = response.Results
                .GroupBy(x => x.Sequence)
                .OrderBy(x => x.Key)
                .Select(x => new TimelineData(
                    x.First().StartTime.Ticks,
                    x.First().StartTime,
                    x.First().EndTime,
                    ((int)x.First().SpeedMs).ToString(CultureInfo.InvariantCulture) + " m/s",
                    "")).ToList();

            response.Route = new Visual
            {
                Id = new VisualId
                {
                    Source = "HMM_MM",
                    Name = $"{name}",
                    Id = $"HMM {name} Route",
                    VisualType = "Route"
                },
                Timeline = rt,
                Geometry =
                    new FeatureCollection(
                        response.Results.Select(HmmUtil.GetRoadLinkEdgeSpeedAsFeatureCollection).ToList())
            };

            response.Name = name;

            return response;
        }

        /// <summary>
        /// compute candidate road position "hidden/unobserved" fixes (CandidateFix) for all observed 
        /// GPS fixes. All candidates lie on the road segments with an offset and a calculated
        /// emission probability 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static Step[] GenerateCandidates(this HmmParameters parameters)
        {
            // 
            var steps = parameters
                .Fixes
                .Select(x => new Step
                {
                    CandidateFixes = x.GetCandidateFixes(parameters),
                    Fix = x
                })

                // only use fixes that have some candidates
                .Where(x=>x.CandidateFixes.Count>0 && x.Fix.Speed>0)

                .ToArray();

            return steps;
        }

        /// <summary>
        /// extract parameters into our concrete HmmParameters class
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static HmmParameters GetParameters(this RouteMatcherRequest request)
        {
            if (request.Parameters == null)
                throw new ApplicationException("NULL passed for parameters.");

            // extract the parameters from the dynamic
            HmmParameters parameters = ExpandoUtil.SetFromExpando<HmmParameters>(request.Parameters);

            if (parameters == null)
                throw new ApplicationException("Incorrect parameters passed, should be of type HMMParameters");

            Enum.TryParse(parameters.Emission, out parameters.EmissionEnum);
            Enum.TryParse(parameters.Transition, out parameters.TransitionEnum);

            parameters.Fixes = request.Fixes;
            parameters.RoadSpeedCalculator = request.RoadSpeedCalculator;
            parameters.HmmRoutingData = request.RoutingData;
            parameters.HmmRoutingEngine = request.RoutingEngine;
            if (parameters.VehicleType == null)
                parameters.VehicleType = "AEU";

            if (parameters.MaxCandidates == 0)
                throw new ApplicationException("MaxCandidates should be > 0");

            return parameters;
        }

        /// <summary>
        ///     work out routes from the sourceFix to the targets
        /// </summary>
        /// <param name="sourceCandidate">route from this coordinate</param>
        /// <param name="step"></param>
        /// <param name="targetFixes">route to these coordinates</param>
        /// <param name="parameters"></param>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        internal static List<SampleRoute> CalculateCandidateRoutes(
            this CandidateFix sourceCandidate,
            Step step,
            Step nextstep,
            HmmParameters parameters,
            string vehicle
            )
        {
            var candidateRoutes = new List<SampleRoute>();

            // make sure each record is unique
            var endLocations = nextstep.CandidateFixes.Select(x=>x.EdgeWithOffset).ToList();

            var maxDistance = nextstep.CandidateFixes.Max(x => x.EdgeWithOffset.Coord.Distance(sourceCandidate.Position));

            var routeRequest = new RouteRequestMultiple
            {
                RoadSpeedCalculator = parameters.RoadSpeedCalculator,
                StartLocation = sourceCandidate.EdgeWithOffset,
                EndLocations = endLocations,
                DistanceMax = 5000,
                DurationMax = 600,
                InstanceMax = endLocations.Count,
                VehicleType = vehicle,
                SearchType = RouteSearchType.Quickest,
                HourOfWeek = step.Fix.Timestamp.HourOfWeek(),
                Map = null
            };

            var routingResult = parameters.HmmRoutingEngine.CalculateRouteMultiple(routeRequest);

            // create a network with the resultant routes with associated probabilities
            foreach (var route in routingResult.Items)
            {
                // find the original target in the list of routes returned
                var targetCandidate = nextstep.CandidateFixes.FirstOrDefault(x => Equals(x.EdgeWithOffset, route.EndEdge));

                if (route.Connections.Last() == null)
                    route.Connections.RemoveAt(route.Connections.Count - 1);

                if (targetCandidate != null)
                {
                    var fixDistance = step.Fix.DistanceFrom(nextstep.Fix);
                    //var roadDistance = sourceCandidate.DistanceFrom(targetCandidate);
                    var roadDistance = route.Distance;

#if !Koller
                    var pdfValue = Math.Abs(roadDistance - fixDistance);
#else
                    var routeDistance = Math.Abs(route.Distance - roadDistance);
                    var pdfValue = (route.Distance / roadDistance) - 1;
                    Debug.Print($"{sourceCandidate}->{targetCandidate.Id} min={roadDistance}   route distance {routeDistance} =T=> {transitionProbability}");
#endif

                    var duration = (targetCandidate.Timestamp - sourceCandidate.Timestamp).TotalSeconds;

                    var transitionProbability = pdfValue.CalcDistribution(parameters.TransitionEnum, parameters.TransitionP1, parameters.TransitionP2);
                    var speed = route.Distance / duration;


                    //-------------------------------------------------------------
                    //  Sanity check 50m/s which is over 100mph
                    //-------------------------------------------------------------
                    //if (speed > 150)
                    //    continue;

                    if (Math.Abs(transitionProbability) < Double.Epsilon)
                        continue;

                    var candidateRoute = new SampleRoute
                    {
                        TransitionValue = pdfValue,
                        Step = step,
                        Route = route.Connections,
                        TransitionProbability = transitionProbability,
                        SourceFix = sourceCandidate,
                        DestinationFix = targetCandidate,
                        RouteDistance = route.Distance,
                        Duration = duration,
                        SpeedMs = speed,
                        PathPoints = route.PathPoints
                    };

                    candidateRoutes.Add(candidateRoute);

                }
                else
                {
                    Debug.Print("Something went wrong with the route.. it doesn't have a correct tag");
                }
            }

            return candidateRoutes;
        }

        internal static Feature GetFixAsFeatureCollection(this Step step, string type, string status)
        {
            var fix = step.Fix;
            var coord = $"{Math.Round(fix.Position.X, 1)},{Math.Round(fix.Position.Y, 1)}";
            var dict = new Dictionary<string, object>
            {
                {"type", type},
                {"status", status},
                {"step", fix.Sequence},
                {"timestamp", fix.Timestamp},
                {"coord", coord} ,
                {"candidates", step.CandidateFixes.Count} ,
                {"speed", Math.Round(fix.Speed,2)},
                {"direction", Math.Round(fix.Direction,2)},
            };
            var ll = LatLongConverter.OSRefToWGS84(fix.Position.X, fix.Position.Y);
            var pos = new Position(ll.Latitude, ll.Longitude);
            var point = new Point(pos);
            var feature = new Feature(point, dict, coord);
            return feature;
        }

        internal static Feature GetCandidateFixAsFeatureCollection(this CandidateFix fix)
        {
            var links = (fix.RoutesToNextFix == null)
                ? "none"
                : String.Join(",", fix.RoutesToNextFix.Select(x => x.DestinationFix.ToString()).ToArray()).Replace("#", " ");

            var coord = $"{Math.Round(fix.Position.X, 1)},{Math.Round(fix.Position.Y, 1)}";
            var dict = new Dictionary<string, object>
            {
                {"type", "particle"},
                {"status", fix.HasRoutesToHere?"normal":"noroute"},
                {"id", fix.Id},
                {"step", fix.Sequence},
                {"timestamp", fix.Timestamp},
                {"distance", fix.Distance},
                {"roadlinkid", fix.EdgeWithOffset.Edge.RoadLinkEdgeId},
                {"offset", Math.Round( fix.EdgeWithOffset.Offset,1)},
                {"virterbi", Math.Round( fix.Viterbi,6)},
                {"emission", Math.Round( fix.EmissionProbability,6)},
                {"linksto", links},
                {"coord", coord} ,
                {"direction", fix.Direction} ,
            };
            var ll = LatLongConverter.OSRefToWGS84(fix.Position.X, fix.Position.Y);
            var pos = new Position(ll.Latitude, ll.Longitude);
            var point = new Point(pos);
            var feature = new Feature(point, dict, coord);
            return feature;
        }

        internal static Feature GetRoadLinkEdgeSpeedAsFeatureCollection(RoadLinkEdgeSpeed rles)
        {
            var dict = new Dictionary<string, object>
            {
                {"type", "route"},
                {"step", rles.Sequence},
                {"length", Math.Round(rles.RouteDistance,2)},
                {"speed", Math.Round(rles.SpeedMs,2)},
                {"start", rles.StartTime},
                {"end", rles.EndTime},
            };

            var lines = new LineString(rles.PathPoints.Select(
                x =>
                {
                    var ll = LatLongConverter.OSRefToWGS84(x.X, x.Y);
                    return new Position(ll.Latitude, ll.Longitude);
                }));

            var feature = new Feature(lines, dict);
            return feature;
        }


        /// <summary>
        ///     get a list of 'take' roads within 'range' meters
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="fix">position of the fix</param>
        /// <returns></returns>
        internal static List<CandidateFix> GetCandidateFixes(this Fix fix, HmmParameters parameters)
        {
            var point = new NetTopologySuite.Geometries.Point(fix.Position);

            // range envelope
            var envelope = point.Buffer(parameters.RoadEndpointEnvelope).EnvelopeInternal;

            // get a block of nearest roads
            var nearbyRoadsCourse = parameters.HmmRoutingData.ConnectionIndex.Query(envelope).ToArray();
            if (nearbyRoadsCourse.Length == 0)
                return null; // no roads within the envelope

            // extract "MaxCandidates" nearest roads based on range to geometry from the fix and within "RoadGeometryRange"
            var roads = nearbyRoadsCourse
                .Select(x => new { RoadLink = x, distance = DistanceOp.Distance(x.Geometry, point) })

                // candiates must be within range of the fix
                .Where(x => x.distance < parameters.RoadGeometryRange)
                .OrderBy(x => x.distance)

                // only take the best MaxCandidates routes
                .Take(parameters.MaxCandidates);

            var road2 = roads.Select(z => new { z.RoadLink, Distance = z.distance, eo = RoutingData.GetCoordinateOnEdge(fix.Position, z.RoadLink) })
                .Select(x => new CandidateFix
                {
                    DegreeOffset = DegreesDifference(Math.Floor(x.eo.AngleRadians * 180 / Math.PI), fix.Direction),
                    EdgeWithOffset = x.eo,
                    Sequence = fix.Sequence,
                    Timestamp = fix.Timestamp,
                    Position = x.eo.Coord,
                    Distance = x.Distance,
                    Viterbi = 0,
                    Direction = Math.Floor(x.eo.AngleRadians * 180 / Math.PI),
                    EmissionProbability = x.Distance.CalcDistribution(parameters.EmissionEnum, parameters.EmissionP1, parameters.EmissionP2)
                }).ToList();

                // only include those links with a good emission value
            var nearestRoads = road2.Where(x=>x.EmissionProbability>0)

                // filter out candidates not traveling in the right direction
                .Where(x => Math.Abs(x.DegreeOffset) < parameters.DirectionTolerance)

                .OrderByDescending(x=>x.EmissionProbability)
                .ToList();

            // normalise the emissions if required
            if (parameters.NormaliseEmission)
                nearestRoads.NormaliseEmission();


            for (var i = 0; i < nearestRoads.Count; i++)
                nearestRoads[i].Id = i;

            nearestRoads.ForEach(x => Debug.Print($"{x.Distance} =E=> {x.EmissionProbability}"));
            return nearestRoads;
        }

        /// <summary>
        /// calculate the difference between two angles.
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
        internal static double DegreesDifference(double a1, double a2)
        {
            return Math.Min((a1 - a2 + 360) % 360, (a2 - a1 + 360) % 360);
        }

        internal static bool DegreesWithinRange(double angle1, double angle2, double tolerance)
        {
            if (tolerance <= 0)
                return true;
            else
                return DegreesDifference(angle1, angle2)< tolerance;
        }
    }
}