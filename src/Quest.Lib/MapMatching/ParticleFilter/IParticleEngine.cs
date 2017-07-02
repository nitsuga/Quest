using System.Collections.Generic;
using Quest.Lib.Routing;

namespace Quest.Lib.MapMatching.ParticleFilter
{
    public interface IParticleEngine
    {
        TrackAnalysis AnalyseTrack(ParticleStepRequest request);
        List<MotionParticle> DeadReckonParticles(RoutingData routingData, List<MotionParticle> particles, double secs);
    }
}