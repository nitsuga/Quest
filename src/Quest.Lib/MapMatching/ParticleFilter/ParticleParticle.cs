using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Quest.Lib.Routing;

namespace Quest.Lib.MapMatching.ParticleFilter
{
    internal abstract class ParticleParticle : IParticleEngine
    {
        public List<MotionParticle> DeadReckonParticles(RoutingData routingData, List<MotionParticle> particles,
            double secs)
        {
            return DeadReckon(routingData, particles, secs);
        }

        public TrackAnalysis AnalyseTrack(ParticleStepRequest request)
        {
            var result = new TrackAnalysis
            {
                Particles = request.Particles,
                EstimatedVector = new MotionParticle()
            };

            // 1. initialise or move
            if (request.PreviousFix == null)
            {
                result.Particles = GenerateParticles(request);
                result.EstimatedVector = result.Particles.CalcEstimatedVector();
                return result;
            }

            Debug.Print($"before move: particles={result.Particles.Count}");

            // 2. predict (resample if necessary, move and perturb)
            result.Particles = Predict(result.Particles, 0.9, request);

            // 3. measurement update: adjust weight of prior based on new sensor information
            // if we have just generated a uniform distribution then the weights are inversly 
            // proportional to the distance
            result.Particles = UpdateWeights(request, result.Particles);

            // this step calculates the final estimate from the cloud of particles
            result.EstimatedVector = result.Particles.CalcEstimatedVector();

            return result;
        }

        /// <summary>
        ///     generate random particles during step 1
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected abstract List<MotionParticle> GenerateParticles(ParticleStepRequest request);

        protected abstract List<MotionParticle> MoveAllParticles(ParticleStepRequest request, List<MotionParticle> particles);

        protected abstract List<MotionParticle> DeadReckon(RoutingData routingData, List<MotionParticle> particles, double secs);

        protected abstract void Perturb(ParticleStepRequest request, MotionParticle source);

        protected abstract List<MotionParticle> UpdateWeights(ParticleStepRequest request, List<MotionParticle> particles);


        /// <summary>
        ///     resample a set of particles according to the weight distribution
        /// </summary>
        /// <param name="particles"></param>
        /// <param name="sampleCount"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected List<MotionParticle> Resample(List<MotionParticle> particles, int sampleCount,
            ParticleStepRequest request)
        {
            if (particles.Count == 0)
            {
                GenerateParticles(request);
                return request.Particles;
            }

            // take the top (80)n% best particles
            particles = particles.OrderByDescending(x => x.Weight)
                    .ToList()
                    .Take((int) (particles.Count*request.Parameters.ResampleCutoff))
                    .ToList();

            // create CDF of weights and sample from the CDF so that higher weighted 
            // particles are selected more often. This results in possible duplicates 
            // of higher weighted particles and possible removal of lower weighted particles.
            var cumulativeWeights = new double[particles.Count];

            var cumSumIdx = 0;
            double cumSum = 0;
            foreach (var p in particles)
            {
                cumSum += p.Weight;
                cumulativeWeights[cumSumIdx++] = cumSum;
            }

            var maxCumWeight = cumulativeWeights[particles.Count - 1];

            var filteredParticles = new List<MotionParticle>();

            // make sampleCount particles
            for (var i = 0; i < sampleCount; i++)
            {
                var randWeight = RandomProportional.NextDouble(1)*maxCumWeight;

                // find index of particle template
                var particleIdx = 0;
                while (cumulativeWeights[particleIdx] < randWeight)
                    particleIdx++;

                // add particle to the new list with a uniform weight

                #region add particle to the new list with a uniform weight

                var p = particles[particleIdx];

                if (filteredParticles.Contains(p))
                {
                    // if the particle list already has this particle then copy it
                    // and make it slightly different
                    var clone = p.Clone();
                    p = (MotionParticle) clone;
                    Perturb(request, p);
                }

                #endregion

                filteredParticles.Add(p);
            }

            Debug.Print($"resample: filteredParticles={filteredParticles.Count}");

            // draw particles from distribution to get particle weights normalised to 0..1
            filteredParticles.NormalizedWeights();

            return filteredParticles;
        }

        protected List<MotionParticle> Predict(List<MotionParticle> particles, double effectiveCountMinRatio,
            ParticleStepRequest request)
        {
            List<MotionParticle> newParticles;

            // calculate effective particle count to determine if we need to resample
            var effectiveCountRatio = EffectiveParticleCount(particles) / particles.Count;

            if (effectiveCountRatio > float.Epsilon && effectiveCountRatio < effectiveCountMinRatio)
                newParticles = Resample(particles, particles.Count, request).ToList();
            else
                newParticles = particles.Select(x => x).ToList();

            // apply state transition to the estimated state using the particles motion model
            request.Particles = MoveAllParticles(request, request.Particles);

            Debug.Print($"after move: particles={request.Particles.Count}");

            // apply state transition error
            foreach (var p in request.Particles)
                Perturb(request, p);

            return newParticles;
        }

        /// <summary>
        ///     return sum(x)/sum(x^2)
        /// </summary>
        /// <param name="particles"></param>
        /// <returns></returns>
        private double EffectiveParticleCount(IEnumerable<MotionParticle> particles)
        {
            var motionParticles = particles as MotionParticle[] ?? particles.ToArray();
            var sumSqr = motionParticles.Sum(x => x.Weight*x.Weight) + float.Epsilon;
            var sum = motionParticles.Sum(x => x.Weight) + float.Epsilon;
            var effectiveCountRatio = sum/sumSqr;

            Debug.Print($"predict: effectiveCountRatio={effectiveCountRatio}");

            return effectiveCountRatio;
        }
    }
}