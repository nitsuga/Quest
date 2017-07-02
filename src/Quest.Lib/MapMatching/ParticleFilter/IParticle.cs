using System;

namespace Quest.Lib.MapMatching.ParticleFilter
{
    public interface IParticle : ICloneable
    {
        double Weight { get; set; }
    }
}