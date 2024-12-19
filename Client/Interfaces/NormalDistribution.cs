using System;

namespace Project.Interfaces
{
    public class NormalDistribution
    {
        Random gaussian = new Random();
        bool haveNextNextGaussian = false;
        double nextNextGaussian;

        public double NextGaussian()
        {

            double v1, v2, s;
            do
            {
                v1 = 2 * gaussian.NextDouble() - 1;
                v2 = 2 * gaussian.NextDouble() - 1;

                s = v1 * v1 + v2 * v2;
            } while (s >= 1 || s == 0);

            double multiplier = Math.Sqrt(-2 * Math.Log(s) / s);
            nextNextGaussian = v2 * multiplier;

            return v1 * multiplier;

        }
    }

}
