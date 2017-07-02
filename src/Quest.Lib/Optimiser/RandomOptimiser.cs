using System;

namespace Quest.Lib.Optimiser
{
    /// <summary>
    ///     repeats a simulation run multiple times with the same constants
    /// </summary>
    public sealed class RandomOptimiser : OptimiserBase
    {
        /// <summary>
        ///     Converge using Nelder-Mead algorithm. The method executes the delegate objective function. The function
        ///     should return a real number
        /// </summary>
        /// <param name="simplexConstants">Passed to the objective function</param>
        /// <param name="convergenceTolerance">Not used</param>
        /// <param name="maxEvaluations">sets a cap on the maximum number of iterations</param>
        /// <param name="objectiveFunction">The method to execute to evaluate the current position</param>
        /// <param name="innerInterations">Not used</param>
        /// <returns>a regression result</returns>
        public override RegressionResult Regress(SimplexConstant[] simplexConstants, double convergenceTolerance,
            int maxEvaluations, ObjectiveFunctionDelegate objectiveFunction, int innerInterations)
        {
            // confirm that we are in a position to commence
            if (objectiveFunction == null)
                throw new InvalidOperationException("ObjectiveFunction must be set to a valid ObjectiveFunctionDelegate");

            if (simplexConstants == null)
                throw new InvalidOperationException("SimplexConstants must be initialized");

            double sumperformance = 0;
            var evaluationCount = 0;
            var rand = new Random();

            for (evaluationCount = 0; evaluationCount < maxEvaluations; evaluationCount++)
            {
                var constants = new double[simplexConstants.Length];

                for (var i = 0; i < simplexConstants.Length; i++)
                {
                    // get random number between -1 and 1
                    var randomNumber = rand.NextDouble()*2 - 1;
                    constants[i] = simplexConstants[i].Value + randomNumber*simplexConstants[i].InitialPerturbation;
                }

                sumperformance +=
                    objectiveFunction(new ObjectiveFunctionParams
                    {
                        constants = constants,
                        innerInterations = innerInterations
                    });
            }

            var regressionResult = new RegressionResult(TerminationReason.MaxFunctionEvaluations, null,
                sumperformance/evaluationCount, evaluationCount);
            return regressionResult;
        }
    }
}