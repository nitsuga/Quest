using System;

namespace Quest.Lib.MapMatching
{
    /// <summary>
    ///     Static class to get random double value
    /// </summary>
    public static class RandomProportional
    {
        private static readonly Random Random = new Random();

        /// <summary>
        ///     Getting random double value from 0 to MaxValue
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static double NextDouble(double maxValue)
        {
            return Random.NextDouble()*maxValue;
        }

        /// <summary>
        ///     Getting random double value from MinValue to MaxValue
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static double NextDouble(double minValue, double maxValue)
        {
            var randomValue = Random.NextDouble();
            return minValue + randomValue*(maxValue - minValue);
        }

        public static int NextInt(int minValue, int maxValue)
        {
            var randomValue = Random.NextDouble();
            return (int) (minValue + randomValue*(maxValue - minValue));
        }

        public static T NextRandom<T>(this T[] array)
        {
            var randomValue = Random.NextDouble();
            var index = (int) (randomValue*array.Length);
            return array[index];
        }


        public static double Exponential(double beta)
        {
            var u = Random.NextDouble();
            var r = Math.Log(1 - u)/-beta;
            return r;
        }
    }
}