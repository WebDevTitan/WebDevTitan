using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.calc
{
    public class Formula1 : IFormula
    {
        public Calc calc { get; set; }
        public double revenue_1(double a, double e, double i, double n, double t, double r, double o) {
                return a * e - o;
        }
        public double revenue_2(double a, double e, double i, double n, double t, double r, double o) {
            return i * n - o;
        }
        public double revenue_3(double a, double e, double i, double n, double t, double r, double o)
        {
            return 0;
        }

        public Calc outcomes_1_2_3(double[] p)
        {
            return null;
        }
        public Calc outcomes_1_2(double[] p) {
            var i = 1 / p[0] + 1 / p[1];
            var n = 1 / (i * p[0]);
            var t = 1 / (i * p[1]);

            calc = new Calc(new double[]{n, t}, (double)Math.Round(100 * (n * p[0] - 1), 2));
            return calc;
        }
        public Calc outcomes_1(double[] p) {
            var i = 1 / p[0] + 1 / p[1];
            var n = 1 / (i * p[0]);
            var t = 1 / (i * p[1]);
            var o = t / (n * p[0]);
            var r = 1 - t / (n * p[0]);

            calc = new Calc(new double[]{r, o}, (double)Math.Round(100 * (r * p[0] - 1), 2));
            return calc;
        }
        public Calc outcomes_2(double[] p) {
            var i = 1 / p[0] + 1 / p[1];
            var n = 1 / (i * p[0]);
            var t = 1 / (i * p[1]);
            var r = n / (n * p[0]);
            var o = 1 - n / (n * p[0]);
            
            calc = new Calc(new double[]{r, o}, (double)Math.Round(100 * (o * p[1] - 1), 2));
            return calc;
        }
    }
}
