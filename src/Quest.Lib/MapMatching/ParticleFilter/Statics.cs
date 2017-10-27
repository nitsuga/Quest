using System;
using GeoAPI.Geometries;
using Quest.Common.Messages;
using Quest.Common.Messages.GIS;

namespace Quest.Lib.MapMatching.ParticleFilter
{
    internal static class Statics
    {
        internal static MotionVector Diffuse(this MotionVector mv, ParticleStepRequest request)
        {
            mv.Position = CreateRandomPointAround(mv.Position, request.Parameters.RoadGeometryRange);
            mv.Direction = mv.Direction + RandomProportional.NextDouble(-request.Parameters.ParticleDirectionVariance, request.Parameters.ParticleDirectionVariance);
            mv.Speed = mv.Speed + RandomProportional.NextDouble(-request.Parameters.ParticleSpeedVariance, request.Parameters.ParticleSpeedVariance);
            return mv;
        }

        internal static Coordinate CreateRandomPointAround(this Coordinate reference, double rangeMeters)
        {
            var angle = RandomProportional.NextDouble(2*Math.PI);
            var range = RandomProportional.NextDouble(rangeMeters);

            // rotate (in meters)
            var dx = Math.Sin(angle)*range;
            var dy = Math.Cos(angle)*range;

            // randomise its position according to the given distributions
            return new Coordinate(reference.X + dx, reference.Y + dy);
        }
    }
}