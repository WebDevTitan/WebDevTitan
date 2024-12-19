using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.calc
{
    public class Formula17 : IFormula
    {
        public Calc calc { get; set; }
        public double revenue_1(double a, double e, double i, double n, double t, double r, double o) {
            return a * e + i * n - o;
        }
        public double revenue_2(double a, double e, double i, double n, double t, double r, double o) {
            return t * r + (a + 1) * e / 2 - o;
        }
        public double revenue_3(double a, double e, double i, double n, double t, double r, double o) {
            return i * n + t * r - o;
        }
        public Calc outcomes_1_2_3(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = 1 / a + 1 / e + 1 / (a * e) + 2 / i;
            var t = (a + 1) / (2 * e);
            var r = a / i;
            var o = 1 / (1 + t + r);
            var s = t / (1 + t + r);
            var l = r / (1 + t + r);

            calc = new Calc(new double[] { o, s, l }, Math.Round(100 * (s * e + l * i - 1), 2));
            return calc;
        }
        public Calc outcomes_1(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = 1 / (1 + a / e + (a / e + 1 - a) / (i - 1));
            var t = n * a / e;
            var r = 1 - n - t;

            calc = new Calc(new double[] { n, t, r }, Math.Round(100 * (a / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_2(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = 1 / (1 + a / i + (a / i + 1 - a) / (e - 1));
            var r = n * a / i;
            var t = 1 - n - r;

            calc = new Calc(new double[] { n, t, r }, Math.Round(100 * (((a + 1) / 2 + n) / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_3(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var t = 1 / (1 + e / i + (e / i + 1 - e) / (a - 1));
            var r = t * e / i;
            var n = 1 - t - r;

            calc = new Calc(new double[] { n, t, r }, Math.Round(100 * ((e * n + i * t) / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_1_2(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var t = 1 / (2 * e);
            var r = 1 / (2 * i);
            var n = 1 - t - r;

            calc = new Calc(new double[] { n, t, r }, Math.Round(100 * (a / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_2_3(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = 1 / (2 * a);
            var t = 1 / (2 * e);
            var r = 1 - n - t;

            calc = new Calc(new double[] { n, t, r }, Math.Round(100 * ((e * n + i * t) / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_1_3(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = 1 / (2 * a);
            var r = 1 / (2 * i);
            var t = 1 - n - r;

            calc = new Calc(new double[] { n, t, r }, Math.Round(100 * ((e * n + i * t) / (1 + n + t) - 1), 2));
            return calc;
        }
    }
}
