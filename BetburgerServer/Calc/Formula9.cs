using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.calc
{
    public class Formula9 : IFormula
    {
        public Calc calc { get; set; }
        public double revenue_1(double a, double e, double i, double n, double t, double r, double o) {
            return a * e - o;
        }
        public double revenue_2(double a, double e, double i, double n, double t, double r, double o) {
            return i * n + e / 2 + r - o;
        }
        public double revenue_3(double a, double e, double i, double n, double t, double r, double o) {
            return t * r - o;
        }
        public Calc outcomes_1_2_3(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = 1 / a + 1 / i + (2 * a * i - i - 2 * a) / (2 * a * e * i);
            var t = (2 * a * i - i - 2 * a) / (2 * e * i);
            var r = 1 / (n * a);
            var o = t / (n * a);
            var s = 1 / (n * i);

            calc = new Calc(new double[] { r, o, s }, Math.Round(100 * (r * a - 1), 2));
            return calc;
        }
        public Calc outcomes_1(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = 1 / (2 * (e - 1));
            var t = (2 * e - 1) / (2 * (e - 1) * (i - 1));
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[] { r, o, s }, Math.Round(100 * (a / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_2(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = a - 1 - a / i;
            var t = a / i;
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[] { r, o, s }, Math.Round(100 * ((e * n + .5 + t) / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_3(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = 1 / (2 * (e - 1));
            var t = a - 1 - n;
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[] { r, o, s }, Math.Round(100 * (i * t / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_1_2(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var r = 1 / (i - 1);
            var n = (a - .5 - r) / (e + r);
            var t = (1 + n) * r;
            var o = 1 / (1 + n + t);
            var s = n / (1 + n + t);
            var l = t / (1 + n + t);

            calc = new Calc(new double[] { o, s, l }, Math.Round(100 * (a / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_2_3(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = ((i - 1) * (a - 1) - .5) / (e + i - 1);
            var t = a - 1 - n;
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[] { r, o, s }, Math.Round(100 * (i * t / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_1_3(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = 1 / (2 * (e - 1));
            var t = a / i;
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[] { r, o, s }, Math.Round(100 * (i * t / (1 + n + t) - 1), 2));
            return calc;
        }
    }
}
