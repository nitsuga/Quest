using Quest.Lib.MapMatching.RouteMatcher;
using Quest.Lib.Routing;
using Quest.Lib.Maths;

namespace Quest.Lib.MapMatching.ParticleFilter
{
    
    public class ParticleParameters : IMapMatchParameters
    {
        /// <summary>
        ///     number of particles to initially generate
        /// </summary>
        public int NumberOfParticles;

        public double ParticleDirectionVariance;

        /// <summary>
        ///     initial max distance of particles
        /// </summary>
        public double RoadGeometryRange;

        public double ParticleSpeedVariance;

        /// <summary>
        ///     when resampling we only take the best 90% of samples by weight
        /// </summary>
        public double ResampleCutoff = 0.5;

        public RoutingData ParticleRoutingData;

        public string Emission;

        public Distributions.DistType EmissionEnum;

        public double EmissionP1;

        public double EmissionP2;

    }
}