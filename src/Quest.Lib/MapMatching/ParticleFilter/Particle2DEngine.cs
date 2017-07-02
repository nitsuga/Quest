#pragma warning disable 0169,649
using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using Quest.Common.Messages;
using Quest.Lib.Constants;
using Quest.Lib.Maths;
using Quest.Lib.Routing;

namespace Quest.Lib.MapMatching.ParticleFilter
{
    internal class Particle2DEngine : ParticleParticle
    {
        protected override List<MotionParticle> DeadReckon(RoutingData routingData, List<MotionParticle> particles,
            double secs)
        {
            var result = new List<MotionParticle>();
            foreach (var p in particles)
            {
                MoveDeadReckoning(p.Vector, secs);
                result.Add(p);
            }
            return result;
        }

        protected override List<MotionParticle> UpdateWeights(ParticleStepRequest request, List<MotionParticle> particles)
        {
            foreach (var particle in particles)
            {
                var distance = particle.Vector.DistanceFrom(request.ThisFix);
                particle.Weight = Distributions.RayleighDistribution(6, distance);
            }

            return particles.NormalizedWeights();
        }

        protected override List<MotionParticle> MoveAllParticles(ParticleStepRequest request,
            List<MotionParticle> particles)
        {
            var result = new List<MotionParticle>();
            foreach (var p in particles)
                if (MoveUsing(request, p.Vector))
                    result.Add(p);
            return result;
        }

        protected override List<MotionParticle> GenerateParticles(ParticleStepRequest request)
        {
            var particles = new List<MotionParticle>();

            if (request != null)
            {
                double standardWeight = 1/request.Parameters.NumberOfParticles;

                var t = request.PreviousFix.Timestamp - request.ThisFix.Timestamp;

                var secs = t.TotalSeconds + double.Epsilon;
                var meters = request.ThisFix.DistanceFrom(request.PreviousFix);
                var mph = meters/secs*Constant.ms2mph;
                var dx = request.ThisFix.Position.X - request.PreviousFix.Position.X + double.Epsilon;
                var dy = request.ThisFix.Position.Y - request.PreviousFix.Position.Y;
                var direction = Math.Atan2(dx, dy)*180/Math.PI;

                for (var i = 0; i < request.Parameters.NumberOfParticles; i++)
                {
                    if (Math.Abs(request.ThisFix.Speed) < 0.1)
                        request.ThisFix.Speed = 10;

                    // generate a particle
                    var p = new MotionParticle
                    {
                        Vector = new MotionVector
                        {
                            Position = request.ThisFix.Position.CreateRandomPointAround(request.Parameters.RoadGeometryRange),
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


#if false
        private static void MoveDelta(MapMatcherRequest request, MotionVector reference, Fix thisFix, Fix prevFix)
        {
            var t = thisFix.Timestamp - prevFix.Timestamp;
            var d = reference.DistanceFrom(thisFix);
            double ms = d / t.TotalSeconds;

            // rotate (in meters)
            double dx = Math.Sin(reference.Direction * Constants.deg2rad) * d;
            double dy = Math.Cos(reference.Direction * Constants.deg2rad) * d;

            // randomise its position according to the given distributions
            reference.Position = new Coordinate(reference.Position.X + dx, reference.Position.Y + dy);
            reference.Speed = reference.Speed;
            reference.Direction = reference.Direction;
        }


#endif

        /// <summary>
        ///     calculate new position of a motion vector based on a timespan, using stored direction and speed
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="secs"></param>
        /// <returns></returns>
        private void MoveDeadReckoning(MotionVector reference, double secs)
        {
            var meters = reference.Speed*secs*Constant.mph2ms;
            // rotate (in meters)
            var dx = Math.Sin(reference.Direction*Constant.deg2rad)*meters;
            var dy = Math.Cos(reference.Direction*Constant.deg2rad)*meters;

            // randomise its position according to the given distributions
            reference.Position = new Coordinate(reference.Position.X + dx, reference.Position.Y + dy);
            reference.Direction = reference.Direction;
        }

        /// <summary>
        ///     move using the same angle and speed
        /// </summary>
        /// <param name="request"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        private bool MoveUsing(ParticleStepRequest request, MotionVector reference)
        {
            var t = request.ThisFix.Timestamp - request.PreviousFix.Timestamp;
            var d = reference.DistanceFrom(request.ThisFix);
            var ms = d/t.TotalSeconds;

            var dx = request.ThisFix.Position.X - request.PreviousFix.Position.X + double.Epsilon;
            var dy = request.ThisFix.Position.Y - request.PreviousFix.Position.Y;

            // randomise its position according to the given distributions
            reference.Position = new Coordinate(reference.Position.X + dx, reference.Position.Y + dy);
            reference.Speed = ms*Constant.ms2mph;
            reference.Direction = Math.Atan2(dx, dy)*180/Math.PI;
            return true;
        }

        /// <summary>
        ///     generate an updated particle, based on the current and last observation
        /// </summary>
        /// <param name="source"></param>
        /// <param name="prevFix"></param>
        /// <param name="thisFix"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override void Perturb(ParticleStepRequest request, MotionParticle source)
        {
            var t = request.ThisFix.Timestamp - request.PreviousFix.Timestamp;
            var secs = t.TotalSeconds + double.Epsilon;
            var meters = request.ThisFix.DistanceFrom(request.PreviousFix);
            var mph = meters/secs*Constant.ms2mph;
            var dx = request.ThisFix.Position.X - request.PreviousFix.Position.X + double.Epsilon;
            var dy = request.ThisFix.Position.Y - request.PreviousFix.Position.Y;
            var direction = Math.Atan2(dx, dy)*180/Math.PI;

            source.Vector.Position = request.ThisFix.Position.CreateRandomPointAround(request.Parameters.RoadGeometryRange);
            source.Vector.Direction = (direction + RandomProportional.NextDouble(-10, 10))%360.0;
            source.Vector.Speed = mph + RandomProportional.NextDouble(-5, 5);
        }
    }
}