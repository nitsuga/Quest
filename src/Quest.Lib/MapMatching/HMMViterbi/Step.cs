using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Quest.Common.Messages.GIS;

namespace Quest.Lib.MapMatching.HMMViterbi
{
    internal class Step
    {
        public List<CandidateFix> CandidateFixes;
        public Fix Fix;

    }

    internal static class StepUtils
    {
        public static void Initialise(this List<CandidateFix> candidateFixes, bool hasRoutes, double viterbi)
        {
            foreach (var candidateFix in candidateFixes)
            {
                candidateFix.HasRoutesToHere = hasRoutes;
                candidateFix.Viterbi = viterbi;
            }
        }

        /// <summary>
        /// Normalise the transition values such that hte sum of emissions equals 1
        /// </summary>
        /// <param name="candidateLinks"></param>
        public static void NormaliseTransition(this List<SampleRoute> candidateLinks)
        {
            // normalise the transition and emission probs..
            var tSum = candidateLinks.Sum(x => x.TransitionProbability);
            foreach (var sampleRoute in candidateLinks)
            {
                Debug.Print($"{sampleRoute.TransitionProbability} =t=> {sampleRoute.TransitionProbability/tSum}");
                sampleRoute.TransitionProbability = sampleRoute.TransitionProbability/tSum;
            }
        }

        /// <summary>
        /// Normalise the emission values such that hte sum of emissions equals 1
        /// </summary>
        /// <param name="candidateFixes"></param>
        public static void NormaliseEmission(this List<CandidateFix> candidateFixes)
        {
            // normalise the emission probs..
            var eSum = candidateFixes.Sum(x => x.EmissionProbability);
            foreach (var c in candidateFixes)
            {
                Debug.Print($"{c.EmissionProbability} =e=> {c.EmissionProbability/eSum}");
                c.EmissionProbability = c.EmissionProbability/eSum;
            }

        }
    }

}