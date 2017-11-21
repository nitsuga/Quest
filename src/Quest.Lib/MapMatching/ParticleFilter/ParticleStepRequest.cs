using System.Collections.Generic;
using Quest.Common.Messages.GIS;

namespace Quest.Lib.MapMatching.ParticleFilter
{
    public class ParticleStepRequest
    {
        public ParticleParameters Parameters;

        /// <summary>
        ///     The previous fix
        /// </summary>
        public Fix NextFix;

        /// <summary>
        ///     current weighted estimates of positions
        /// </summary>
        public List<MotionParticle> Particles;

        /// <summary>
        ///     The previous fix
        /// </summary>
        public Fix PreviousFix;

        /// <summary>
        ///     parse up to this fix
        /// </summary>
        public Fix ThisFix;

    }
}