using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Operation.Distance;
using Quest.Common.Messages;
using Quest.Lib.Constants;
using Quest.Lib.Maths;
using Quest.Lib.Routing;
using Quest.Common.Messages.Routing;
using Quest.Common.Messages.GIS;

namespace Quest.Lib.MapMatching.ParticleFilter
{
    internal class RoadParticleEngine : ParticleParticle
    {
        /// <summary>
        ///     move this particle a bit
        /// </summary>
        /// <param name="source"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override void Perturb(ParticleStepRequest request, MotionParticle source)
        {
        }

        protected override List<MotionParticle> UpdateWeights(ParticleStepRequest request, List<MotionParticle> particles)
        {
            foreach (var particle in particles)
            {
                var distance = particle.Vector.DistanceFrom(request.ThisFix);

                // This is the emission probability based on the distance between the fix and the "actual" position
                // An alternate is to use the Rayleigh distribution but they are both similar
                // https://www.agi.com/downloads/resources/white-papers/Long-Term-Prediction-of-GPS-Accuracy-Understanding-the-Fundamentals.pdf

                //particle.Weight = Distributions.GammaDistribution(3.14733, 0.462432, distance);
                particle.Weight = distance.CalcDistribution(request.Parameters.EmissionEnum, request.Parameters.EmissionP1, request.Parameters.EmissionP2);
                //particle.Weight = Distributions.RayleighDistribution(3, distance);
            }

            return particles.NormalizedWeights();
        }

        protected override List<MotionParticle> MoveAllParticles(ParticleStepRequest request,
            List<MotionParticle> particles)
        {
            return MoveAllParticlesRandomWalk2(request, particles);
//            return MoveAllParticlesNearestRoads(request, particles);
        }

        /// <summary>
        /// this is the theoretically correct algorithm for moving the particle but it doesn't work very well 
        /// as speeds are very variable in traffic, so it is hard to predict the distance a vehicle will travel.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="particles"></param>
        /// <returns></returns>
        protected List<MotionParticle> MoveAllParticlesRandomWalk1(ParticleStepRequest request, List<MotionParticle> particles)
        {
            List<MotionParticle> result = new List<MotionParticle>();

            // calculate time since the last fix and then move each particle using its motion vector

            var t = request.ThisFix.Timestamp - request.PreviousFix.Timestamp;
            double secs = t.TotalSeconds + double.Epsilon;

            foreach (var p in particles)
            {
                var meters = Constant.mph2ms*p.Vector.Speed*secs;
                var distance = meters + RandomProportional.NextDouble(-request.Parameters.RoadGeometryRange, request.Parameters.RoadGeometryRange);
                if (MoveByDistance(request.Parameters.ParticleRoutingData, p, distance, p.Vector.Speed))
                    result.Add(p);
            }
            return result;
        }

        /// <summary>
        /// This method cheats a little by eastimating distance travelled by looking at the new fix.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="particles"></param>
        /// <returns></returns>
        protected List<MotionParticle> MoveAllParticlesRandomWalk2(ParticleStepRequest request, List<MotionParticle> particles)
        {
            List<MotionParticle> result = new List<MotionParticle>();

            var t = request.ThisFix.Timestamp - request.PreviousFix.Timestamp;
            double secs = t.TotalSeconds + double.Epsilon;
            double meters = request.ThisFix.Position.Distance(request.PreviousFix.Position);
            // calc distance to the fix from this particle
            double ms = meters / secs;
            double mph = ms * Constant.ms2mph;

            Debug.Print("Move Particles {0} {1}s {2}m {3}mph", request.ThisFix.Timestamp, secs, meters, mph);

            foreach (var p in particles)
            {
                var distance = meters + RandomProportional.NextDouble(-request.Parameters.RoadGeometryRange, request.Parameters.RoadGeometryRange);
                if (MoveByDistance(request.Parameters.ParticleRoutingData, p, distance, mph))
                    result.Add(p);
            }
            return result;
        }


        protected override List<MotionParticle> DeadReckon(RoutingData routingData, List<MotionParticle> particles,
            double distance)
        {
            var result = new List<MotionParticle>();

            foreach (var p in particles)
            {
                if (MoveByDistance(routingData, p, distance, p.Vector.Speed))
                    result.Add(p);
            }
            return result;
        }

        /// <summary>
        ///     Move a particle along its track by "distance" meters. It will choose a random path if a junction is encountered
        /// </summary>
        /// <param name="routingData"></param>
        /// <param name="reference"></param>
        /// <param name="distance"></param>
        /// <param name="mph"></param>
        /// <returns>return true if ok, false if a dead-end is reached</returns>
        private bool MoveByDistance(RoutingData routingData, MotionParticle reference, double distance, double mph)
        {
            var p = (RoadParticle) reference;
            var currentEdge = p.Edge;

            // walk the network randomly for a distance of "meters"
            while (distance > currentEdge.Geometry.Length - p.Distance)
            {
                var remainingDistance = currentEdge.Geometry.Length - p.Distance;
                p.Distance = 0;
                distance -= remainingDistance;

                // get edges from the last vertex
#if false
                var nextEdges = routingData.Graph.OutEdges(currentEdge.Target)
                    .Where(x => x.RoadLinkEdgeId != currentEdge.RoadLinkEdgeId) // only get links that are not the same
                    .ToArray();
#else
                var nextEdges = currentEdge.OutEdges
                .Where(x => x.RoadLinkEdgeId != currentEdge.RoadLinkEdgeId) // only get links that are not the same
                .ToArray();
#endif
                if (!nextEdges.Any())
                    return false;

                var i = (int) RandomProportional.NextDouble(0, nextEdges.Length - double.Epsilon);
                currentEdge = nextEdges[i];
            }

            // calculate our final coordinate
            p.Distance += distance;
            var liL = new LengthIndexedLine(currentEdge.Geometry);
            var pos = liL.ExtractPoint(p.Distance);

            // get a small step back so we can calculate our new direction
            var lastpos = p.Distance < 2 ? liL.ExtractPoint(0) : liL.ExtractPoint(p.Distance - 2);
            var dx = pos.X - lastpos.X;
            var dy = pos.Y - lastpos.Y;
            var direction = Math.Atan2(dx + double.Epsilon, dy + double.Epsilon)*180/Math.PI;

            // remember the edge for later
            p.Edge = currentEdge;

            // set the new vector
            reference.Vector.Position = pos;
            reference.Vector.Speed = mph;
            reference.Vector.Direction = direction;

            return true;
        }


        /// <summary>
        ///     get a list of 10 roads within 100m
        /// </summary>
        /// <param name="request"></param>
        /// <param name="c"></param>
        /// <param name="range"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        private static RoadEdge[] GetNearestRoadLines(ParticleStepRequest request, Coordinate c, double range, int take)
        {
            var point = new Point(c);

            var e = point.Buffer(range).EnvelopeInternal;

            // get a block of nearest roads
            var nearbyRoads = request.Parameters.ParticleRoutingData.ConnectionIndex.Query(e);
            if (nearbyRoads.Count == 0)
                return null;

            var nearestRoads = nearbyRoads.OrderBy(x => DistanceOp.Distance(x.Geometry, point)).Take(take).ToArray();

            return nearestRoads;
        }

        /// <summary>
        ///     generate particles along the road segment.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override List<MotionParticle> GenerateParticles(ParticleStepRequest request)
        {
            var particles = new List<MotionParticle>();

            if (request != null)
            {
                // ReSharper disable once PossibleLossOfFraction
                double standardWeight = 1/request.Parameters.NumberOfParticles;

                // estimate speed from the next fix
                var t = request.NextFix.Timestamp - request.ThisFix.Timestamp;
                var secs = t.TotalSeconds + double.Epsilon;
                var meters = request.ThisFix.DistanceFrom(request.NextFix);
                var mph = meters/secs*Constant.ms2mph;
                var dx = request.NextFix.Position.X - request.ThisFix.Position.X + double.Epsilon;
                var dy = request.NextFix.Position.Y - request.ThisFix.Position.Y;
                var direction = Math.Atan2(dx, dy)*180/Math.PI;


                // find out how far along the GPS point is 
                // get the nearest road edge and create an index 
                var edges = GetNearestRoadLines(request, request.ThisFix.Position, 100, 10);

                for (var i = 0; i < request.Parameters.NumberOfParticles; i++)
                {
                    // pick a random road
                    var roadIndex = RandomProportional.NextInt(0, edges.Length);
                    var edge = edges[roadIndex];

                    var liL = new LengthIndexedLine(edge.Geometry);
                    var offsetOfFix = liL.IndexOf(request.ThisFix.Position);

                    double TOLERANCE=0.1;
                    if (Math.Abs(request.ThisFix.Speed) < TOLERANCE)
                        request.ThisFix.Speed = 10;

                    var distance = offsetOfFix +
                                   RandomProportional.NextDouble(-request.Parameters.RoadGeometryRange, request.Parameters.RoadGeometryRange);
                    if (distance > liL.EndIndex)
                        distance = liL.EndIndex;

                    if (distance < 0)
                        distance = 0;

                    // extract a point along the road segment along a distance
                    var newcoord = liL.ExtractPoint(distance);
                    var location = new Coordinate(newcoord.X, newcoord.Y);

                    // generate a particle
                    var p = new RoadParticle
                    {
                        Edge = edge,
                        Distance = distance,
                        Vector = new MotionVector
                        {
                            Position = location,
                            Direction =
                                direction +
                                RandomProportional.NextDouble(-request.Parameters.ParticleDirectionVariance,
                                    request.Parameters.ParticleDirectionVariance),
                            
                            Speed =
                                mph +
                                RandomProportional.NextDouble(-request.Parameters.ParticleSpeedVariance, request.Parameters.ParticleSpeedVariance)
                        },
                        Weight = standardWeight
                    };


                    if (p.Vector.Speed < 0)
                        p.Vector.Speed = 0;

                    p.Vector.Direction = p.Vector.Direction%360.0;

                    // even weight to all particle
                    p.Weight = 1d/request.Parameters.NumberOfParticles;

                    particles.Add(p);
                }
            }
            return particles;
        }
    }
}