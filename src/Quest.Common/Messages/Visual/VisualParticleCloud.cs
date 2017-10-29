using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Visual
{
    /// <summary>
    ///     a cloud of particles at a given time
    /// </summary>

    public class VisualParticleCloud
    {
        public DateTime Timestamp { get; set; }

        public List<VisualParticle> Particles { get; set; }

        /// <summary>
        ///     a single particle
        /// </summary>
        
        public class VisualParticle : VisualMotionVector
        {
            public double Probability { get; set; }
        }
    }


}