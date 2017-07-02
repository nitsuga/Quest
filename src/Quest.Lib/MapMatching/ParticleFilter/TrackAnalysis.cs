using System.Collections.Generic;

namespace Quest.Lib.MapMatching.ParticleFilter
{
    public class TrackAnalysis
    {
        /// <summary>
        ///     Estimate of the current vector
        /// </summary>
        public MotionParticle EstimatedVector;

        /// <summary>
        ///     List of particles from the last step
        /// </summary>
        public List<MotionParticle> Particles;
    }
}