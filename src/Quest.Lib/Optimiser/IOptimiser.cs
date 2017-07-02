namespace Quest.Lib.Optimiser
{
    /// <summary>
    ///     abstract class used as a template for optimisers
    /// </summary>
    public abstract class OptimiserBase
    {
        /// <summary>
        ///     this method should be implemented to evaluate the current n-dimensional surface
        /// </summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        public delegate double ObjectiveFunctionDelegate(ObjectiveFunctionParams parms);

        /// <summary>
        ///     an implementation of the Nelder-Mead optimisation algorithm. Use the Regress function
        ///     to perform the search.
        /// </summary>
        /// <param name="simplexConstants">vector to pass to your objective function</param>
        /// <param name="convergenceTolerance">
        ///     Tolerance level. If the return from the
        ///     delegate function differs from the last iteration by less than the tolerance
        ///     then the search is ended
        /// </param>
        /// <param name="maxEvaluations">sets a cap on the maximum number of iterations</param>
        /// <param name="objectiveFunction">The method to execute to evaluate the current position</param>
        /// <param name="innerInterations">the number of smoothing inner iterations</param>
        /// <returns>a regression result</returns>
        public abstract RegressionResult Regress(SimplexConstant[] simplexConstants, double convergenceTolerance,
            int maxEvaluations, ObjectiveFunctionDelegate objectiveFunction, int innerInterations);
    }

    public class ObjectiveFunctionParams
    {
        public double[] constants;
        public int innerInterations;
    }
}