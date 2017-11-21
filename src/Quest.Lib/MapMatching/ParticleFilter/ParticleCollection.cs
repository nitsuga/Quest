#define XCUDIFY

#if CUDIFY
using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;
#endif

using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using Quest.Common.Messages.GIS;

namespace Quest.Lib.MapMatching.ParticleFilter
{
    public static class ParticleCollectionUtils
    {
#if CUDIFY
        static GPGPU gpu;
        static CudafyModule km;

        [Cudafy]
        public static void devCalcEstimatedVector(GThread thread, int N, double[] Weight, double[] Direction, double[] Speed, double[] X, double[] Y)
        {
            int idx = thread.threadIdx.x + thread.blockIdx.x * thread.blockDim.x;
            if (idx < N)
            {
                Direction[idx] = Direction[idx] * Weight[idx];
                Speed[idx] = Speed[idx] * Weight[idx];                
                X[idx] = X[idx] * Weight[idx];
                Y[idx] = Y[idx] * Weight[idx];
            }
        }
#endif


#if CUDIFY
        public static MotionVector CalcEstimatedVector(this List<MotionParticle> particles)
        {
            int N = particles.Count();

            double[] w = particles.Select(px => px.Weight).ToArray();
            double[] d = particles.Select(px => px.Vector.Direction).ToArray();
            double[] s = particles.Select(px => px.Vector.Speed).ToArray();
            double[] x = particles.Select(px => px.Vector.Position.X).ToArray();
            double[] y = particles.Select(px => px.Vector.Position.Y).ToArray();

            double[] dev_w = gpu.CopyToDevice(w);
            double[] dev_d = gpu.CopyToDevice(d);
            double[] dev_s = gpu.CopyToDevice(s);
            double[] dev_x = gpu.CopyToDevice(x);
            double[] dev_y = gpu.CopyToDevice(y);

            gpu.Launch(N, 1).devCalcEstimatedVector(N, dev_w, dev_d, dev_s, dev_x, dev_y);

            gpu.CopyFromDevice(dev_x, x);
            gpu.CopyFromDevice(dev_y, y);
            gpu.CopyFromDevice(dev_d, d);
            gpu.CopyFromDevice(dev_s, s);

            MotionVector result = new MotionVector()
            {
                Direction = d.Sum(),
                Speed = s.Sum(),
                Position = new Coordinate(x.Sum(), y.Sum())
            };

            // free the memory allocated on the GPU
            gpu.Free(dev_w);
            gpu.Free(dev_d);
            gpu.Free(dev_s);
            gpu.Free(dev_x);
            gpu.Free(dev_y);

            return result;
        }
#else
        public static MotionParticle CalcEstimatedVector(this List<MotionParticle> particles)
        {
            var p = new MotionParticle
            {
                Weight = 1,
                Vector = new MotionVector
                {
                    Direction = particles.Sum(x => x.Weight * x.Vector.Direction),
                    Speed = particles.Sum(x => x.Weight * x.Vector.Speed),
                    Position =
                    new Coordinate(particles.Sum(x => x.Weight * x.Vector.Position.X),
                        particles.Sum(x => x.Weight * x.Vector.Position.Y))
                }
            };

            return p;
        }
#endif

        public static List<MotionParticle> NormalizedWeights(this List<MotionParticle> particles)
        {
            var weightSum = particles.Sum(x => x.Weight) + float.Epsilon;
            foreach (var p in particles)
                p.Weight = p.Weight/weightSum;


            var r = particles.OrderBy(x => x.Weight).ToList();
            return r;
        }
    }
}