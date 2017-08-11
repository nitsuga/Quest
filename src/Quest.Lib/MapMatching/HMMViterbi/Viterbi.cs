using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quest.Common.Messages;
using System.Diagnostics;

namespace Quest.Lib.MapMatching.HMMViterbi
{
    internal static class Viterbi
    {
        internal static void CalculateViterbiValues(this Step[] steps, HmmParameters parameters, bool removeUnroutables, bool onlyKeepBestFromPrevious)
        {
            var stepCount = steps.Length;

            // mark the first set of candidates as routed to so they appear valid routes and have a viterbi of 1
            steps[0].CandidateFixes.Initialise(true, 1);

            // for each time step, starting from step 1 (not 0).. look 
            // back at links to each of the candidates and work out, for each candidate
            // which is the best source route. Do this by calculating the current probability ( Tp * Ep)
            // and add to the previous probability. 
            //
            // If no links exist to this candidate then remove this candidate altogether
            for (var i = 1; i < stepCount; i++)
                steps.CalculateViterbiAtStep(i, parameters, removeUnroutables, onlyKeepBestFromPrevious);
        }

        /// <summary>
        /// calculate the viterbi at the current step, calculating the new
        /// viterbi values passed on from the previous step. Any candidates
        /// that cannot be routed to from any of the candidates on the 
        /// previous layer are removed.
        /// </summary>
        /// <param name="steps"></param>
        /// <param name="index"></param>
        /// <param name="parameters"></param>
        internal static void CalculateViterbiAtStep(this Step[] steps, int index, HmmParameters parameters, bool removeUnroutables, bool onlyKeepBestFromPrevious)
        {
            var step = steps[index];
            var prevStep = steps[index - 1];

            foreach (var candidateFix in step.CandidateFixes.ToList())
            {
                // get a list of candidates to this fix
                var linksToThisCandidate =
                    prevStep.CandidateFixes
                        .Where(x => x.HasRoutesToHere) // only pick previous candidates that are routable
                        .SelectMany(x => x.RoutesToNextFix)
                        .Where(x => x.DestinationFix == candidateFix)
                        .ToList();

                candidateFix.Viterbi = Double.MinValue;
                candidateFix.BestPreviousRoute = null;

                // has this candidate been successfully routed to by previous candidates? if not, its no good
                candidateFix.HasRoutesToHere = linksToThisCandidate.Count > 0;

                // remove unroutable candidates
                if (!candidateFix.HasRoutesToHere && removeUnroutables)
                {
                    step.CandidateFixes.Remove(candidateFix);
                    continue;
                }

                // normalise the transition probabilities..
                if (parameters.NormaliseTransition)
                    linksToThisCandidate.NormaliseTransition();

                // loop through routes to this node and calculate best route
                linksToThisCandidate.CalculateViterbiForCandidate(candidateFix, parameters);
            }

            if (onlyKeepBestFromPrevious)
                prevStep.CandidateFixes = prevStep.CandidateFixes.Where(x => x.IsBest == true).ToList();
        }

        private static void CalculateViterbiForCandidate(this List<SampleRoute> linksToThisCandidate, CandidateFix candidateFix, HmmParameters parameters)
        {
            // loop through routes to this node and calculate best route
            foreach (var route in linksToThisCandidate)
            {
                // additive or multiply viterbi
                var viterbi = parameters.SumProbability ?
                            route.SourceFix.Viterbi + route.DestinationFix.EmissionProbability * route.TransitionProbability
                            :
                            route.SourceFix.Viterbi * route.DestinationFix.EmissionProbability * route.TransitionProbability;

                if (!(candidateFix.Viterbi < viterbi)) continue;
                candidateFix.Viterbi = viterbi;
                candidateFix.BestPreviousRoute = route;
            }
        }

        internal static List<RoadLinkEdgeSpeed> GetViterbiPath(this Step[] steps, HmmParameters parameters, bool removeUnroutables, bool onlyKeepBestFromPrevious)
        {
            steps.CalculateViterbiValues(parameters, removeUnroutables, onlyKeepBestFromPrevious);

            var route = steps.ExtractViterbiPath();

            return route;
        }

        internal static List<RoadLinkEdgeSpeed> ExtractViterbiPath(this Step[] steps)
        {
            var stepCount = steps.Length;

            //------ backtrack through the results, splicing together the routes
            var routes = new List<SampleRoute>();

            // loop back until we get a working route
            CandidateFix bestFix = null;
            for (var i = stepCount - 1; i > 0 && bestFix == null; i--)
            {
                var validCandidates = steps[i].CandidateFixes.Where(x => x.HasRoutesToHere);
                if (validCandidates.Any())
                    bestFix = steps[i].CandidateFixes.Where(x => x.HasRoutesToHere).Aggregate((i1, i2) => i1.Viterbi > i2.Viterbi ? i1 : i2);
            }

            // find the best candidate in the last fix and work back from there
            while (bestFix != null)
            {
                bestFix.IsBest = true;
                if (bestFix.BestPreviousRoute != null)
                {
                    routes.Add(bestFix.BestPreviousRoute);
                    bestFix.BestPreviousRoute.IsBest = true;
                }
                bestFix = bestFix.BestPreviousRoute?.SourceFix;
            }

            var results = routes.Select( major =>
                    new RoadLinkEdgeSpeed
                    {
                        Candidates = major.Step.CandidateFixes.Count,
                        Fix = major.Step.Fix,
                        SourceCoord = major.SourceFix.Position,
                        DestCoord = major.DestinationFix.Position,
                        Sequence = major.Step.Fix.Sequence,
                        Edges = major.Route,
                        PathPoints = major.PathPoints,
                        RouteDistance = major.RouteDistance,
                        SpeedMs = major.SpeedMs,
                        EndTime = major.DestinationFix.Timestamp,
                        StartTime = major.SourceFix.Timestamp
                    }
                ).ToList();

            return results;
        }

        internal static string PrintGraphVis(this Step[] steps)
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine("digraph G {");

            s.AppendLine("graph[rankdir = \"LR\"];");
            s.AppendLine("node[shape = rectangle, color = lemonchiffon, style = filled, fontname = \"Helvetica-Outline\"]");

            var stepCount = steps.Length;
            for (var i = 0; i < stepCount; i++)
            {
                var step = steps[i];
                s.AppendLine($"  subgraph cluster{i} {{");
                foreach (var c in step.CandidateFixes.ToList())
                {
                    s.AppendLine($"    N{step.Fix.Sequence}T{c.Id} [label=\"{step.Fix.Sequence}_{c.Id}\\nE={c.EmissionProbability:E4}\\nV={c.Viterbi:E4}\" {(c.IsBest ? "color=salmon2" : "")} {(!c.HasRoutesToHere ? "style=none" : "")} ];");
                }
                s.AppendLine($"  }}");
            }

            for (var i = 0; i < stepCount; i++)
            {
                var step = steps[i];
                foreach (var c in step.CandidateFixes.ToList())
                {
                    if (c.RoutesToNextFix != null)
                    {
                        foreach (var r in c.RoutesToNextFix)
                        {
                            s.AppendLine($"N{step.Fix.Sequence}T{c.Id} -> N{r.DestinationFix.Sequence}T{r.DestinationFix.Id} [label=\"{r.TransitionProbability:E4} {(int)r.RouteDistance}m\\ndelta={r.TransitionValue}\" {(r.IsBest ? "color=salmon2 penwidth=3" : "")} ];");
                        }
                    }
                }
            }

            s.AppendLine("}");

            Debug.Print(s.ToString());

            return s.ToString();
        }
    }
}