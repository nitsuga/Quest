using System;

namespace Quest.Lib.Maths
{
    public static class Distributions
    {
        public enum DistType
        {
            Normal,
            LogNormal,
            Exponential,
            LogExponential,
            Gamma,
            GpsEmission,
            Beta
        }

        public static double GpsEmissionDistribution(double x)
        {
            double[] pros = new[]
            {
                0.0691935194286681, 0.068613559186299, 0.0665882191862215, 0.0651441310994315, 0.0630270824641464,
                0.0616424112396037, 0.0589066522343969, 0.0579908575532394, 0.0565428944536942, 0.0564783109077734,
                0.0577299400277193, 0.0581962332292677, 0.0552757652827274, 0.049919205984053, 0.0419948048995661,
                0.0337449027436382, 0.0253684168377054, 0.0166328464164528, 0.0100246579978326, 0.00634468755126319,
                0.00381817923483998, 0.00262855031897813, 0.00174633908169948, 0.00123483739800644, 0.000942919770444244,
                0.00072850239798706, 0.000490834948998374, 0.000472751556140539, 0.000431418086751202, 0.000381042920932948,
                0.000426251403077535,0.00036037618623828, 0.000286750943888524, 0.000357792844401446, 0.000303542665827942
                , 0.000251875829091271, 0.000343584464298862, 0.00032420940052261, 0.000238959119907103,0.000372001224504031,
                0.00032420940052261, 0.000321626058685776, 0.000273834234704356, 0.00024800081633602, 0.000238959119907103,
                0.000206667346946684, 0.000171792232149431,0.000183417270415182, 0.000236375778070269, 0.00020408400510985,
                0.00019633397959935, 0.00019633397959935, 0.000222167397967685, 0.000178250586741515, 0.000253167500009688,
                0.000269959221949106,0.000246709145417604, 0.000192458966844099, 0.000195042308680933, 0.000140792130107428
            };

            var index = (int)x*2;
            if (index >= 0 && index < pros.Length)
                return pros[index];
            return 0;
        }


        public static double CalcDistribution(this double x, DistType distribution, double p1, double p2=0)
        {
            switch (distribution)
            {
                case DistType.Exponential:
                    return ExponentialDistribution(p1, x);
                case DistType.Gamma:
                    return GammaDistribution(p1, p2, x);
                case DistType.LogExponential:
                    return LogExponentialDistribution(p1, x);
                case DistType.LogNormal:
                    return LognormalDistribution(p1, x);
                case DistType.Normal:
                    return NormalDistribution(p1, x);
                case DistType.GpsEmission:
                    return GpsEmissionDistribution(x);
                case DistType.Beta:
                    return BetaDistribution(p1,p2,x);
            }
            return 0;
        }

        public static double NormalDistribution(double sigma, double x)
        {
            return 1.0/(Math.Sqrt(2.0*Math.PI)*sigma)*Math.Exp(-0.5*Math.Pow(x/sigma, 2));
        }

        public static double LognormalDistribution(double sigma, double x)
        {
            return 1.0/(x*Math.Sqrt(2.0*Math.PI)*sigma)*Math.Exp(-0.5*Math.Pow(Math.Log(x)/sigma, 2));
        }

        public static double ExponentialDistribution(double beta, double x)
        {
            //return 1.0 / beta * Math.Exp(-x / beta);
            return Math.Exp(-x*beta)*beta;
        }

        public static double GammaDistribution(double alpha, double beta, double x)
        {
            var dist = Accord.Statistics.Distributions.Univariate.GammaDistribution.FromBayesian(alpha, beta);
            return dist.ProbabilityDensityFunction(x);
        }

        public static double BetaDistribution(double alpha, double beta, double x)
        {
            var dist = new Accord.Statistics.Distributions.Univariate.BetaDistribution(alpha, beta);
            return dist.ProbabilityDensityFunction(x);
        }


        /**
         * Use this function instead of Math.log(exponentialDistribution(beta, x)) to avoid an
         * arithmetic underflow for very small probabilities.
         */

        public static double LogExponentialDistribution(double beta, double x)
        {
            return Math.Log(1.0/beta) - x/beta;
        }

        public static double RayleighDistribution(double theta, double x)
        {
            var mu2 = theta*theta;
            var x2 = x*x;
            return x/mu2*Math.Exp(-x2/(2*mu2));
        }
    }
}