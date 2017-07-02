using System;

namespace Quest.Lib.Optimiser
{
    /// <summary>
    ///     repeats a simulation run multiple times with the same constants
    /// </summary>
    public sealed class NullOptimiser : OptimiserBase
    {
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

            for (evaluationCount = 0; evaluationCount < maxEvaluations; evaluationCount++)
            {
                var constants = new double[simplexConstants.Length];

                for (var i = 0; i < simplexConstants.Length; i++)
                    constants[i] = simplexConstants[i].Value;

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