using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Quest.Lib.Simulation.Probability
{

    /// <summary>
    /// The ProbabilityEngine class maintains and allows actions on a single Continuous Distribution Function.
    /// The class is instantiated by deserialisation of XML stored in the database. Use the 
    /// Utils.DeserializeXml class to acheive this.
    /// </summary>
    [Serializable]
    public class ProbabilityEngine
    {
        /// <summary>
        /// maintain a random number generator
        /// </summary>
        static Random _rnd = new Random( DateTime.Now.Millisecond );

        /// <summary>
        /// holds the distribution
        /// </summary>
        public double[] ncdf;

        /// <summary>
        /// hold the min,max and bucket size
        /// </summary>
        public double min;
        public double max;
        public double outlierThreshold=0.001;
        public double bucketsize;
        public double mean;
        public double stddev;
        public int numbuckets = 30;
        public int count = 0;
        public List<double> samples = new List<double>();

        public ProbabilityEngine()
        {
        }


        /// <summary>
        /// make a random number from the given normalised continuous probability distribution
        /// </summary>
        /// <param name="ncdf"></param>
        /// <returns></returns>
        public double GetNextRand()
        {
            double d = _rnd.NextDouble();

            for (int i = 0; i < ncdf.Length; i++)
                if (d < ncdf[i])
                {

                    return min+(d*(max-min));
                }
            return 0;
        }

        /// <summary>
        /// prepare
        /// </summary>
        public void BeginSampling()
        {
            ncdf=null;
            min=0.0;
            max=0.0;
            outlierThreshold=0.001;
            bucketsize=0;
            mean=0.0;
            stddev=0.0;
            numbuckets = 30;
            count = 0;
            samples = new List<double>();
        }

        /// <summary>
        /// add a sample to the sample collection
        /// </summary>
        /// <param name="sample"></param>
        public void AddSample(double sample)
        {
            samples.Add(sample);
        }

        /// <summary>
        /// make a continuous probability distribution from samples
        /// </summary>
        /// <param name="samples"></param>
        /// <returns></returns>
        public void EndSampling()
        {
            if (samples.Count() == 0)
                return;

            // first pass... calculate the distributio according to the samples we have
            CalculateNCDF();

            // remove and samples that occur rarely
            RemoveLowFrequencySamples(outlierThreshold);

            //calculate average
            CalcStats();

            // normalise the results.
            Normalise();
        }

        /// <summary>
        /// calculate key metrics
        /// </summary>
        private void CalcStats()
        {
            mean = 0;
            stddev=0;
            count = samples.Count;

            if (count == 0)
                return;

            mean = samples.Sum() / count;

            if (count == 1)
                return;


            double diffs = samples.Sum(s => { return (s - mean) * (s - mean); });

            stddev = Math.Sqrt( diffs / (count - 1));
        }

        /// <summary>
        /// calculate the normalised continuous distribution
        /// </summary>
        private void CalculateNCDF()
        {
            if (samples.Count() == 0)
                return;

            ncdf = new double[numbuckets];

            min = samples.Min();
            max = samples.Max();
            bucketsize = (max - min) / numbuckets;

            if (bucketsize == 0)
                bucketsize = 1;

            // transfer to buckets 
            samples.ForEach(
                v =>
                {
                    int b = (int)((v - min) / bucketsize);
                    if (b < numbuckets)
                        ncdf[b]++;
                }
            );

       }

        /// <summary>
        /// normalise the distribution to between 0..1
        /// </summary>
        private void Normalise()
        {
            double maxcdf = ncdf.Sum();
            double cum = 0;

            for (int i = 0; i < ncdf.Length; i++)
            {
                cum += ncdf[i];
                ncdf[i] = cum / maxcdf;
            }

        }

        /// <summary>
        /// removes samples that occur in frequency buckets that have threshold% let occurrences that the maximum bucket.
        /// </summary>
        /// <param name="threshold"></param>
        private void RemoveLowFrequencySamples(double threshold)
        {
            // calculate the cutoff frequency.. usually min will end up as 0. so we
            // get rid of any values in the frequency range between min and the cutoff
            double maxfreq = ncdf.Max();
            double minfreq = ncdf.Min();
            double cutoff = ((maxfreq-minfreq)*threshold)+minfreq;

            //remove values from samples with low frequency by transfering to new list
            List<double> newsamples = new List<double>();

            for (int i=0; i<ncdf.Length;i++)
            {
                if (ncdf[i] > cutoff)
                {
                    double smin = min + (i * bucketsize);
                    double smax = smin + bucketsize;
                    // find values 

                    samples.ForEach(s =>
                    {
                        if (s >= smin && s < smax)
                            newsamples.Add(s);
                    }
                    );
                }
            }

            samples = newsamples;

            // recalculate NCDF
            CalculateNCDF();
        }

        /// <summary>
        /// convert a string containing a list of numbers into an array of floats
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static double[] ConvertStringList(string source, char separator)
        {
            int index = 0;
            string[] parts = source.Split(separator);
            double[] values = new double[parts.Length];
            // transfer to double array int index = 0; 
            foreach (String p in parts)
                double.TryParse(p, out values[index++]);
            return values;
        }
    }
}
