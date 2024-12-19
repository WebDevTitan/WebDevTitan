using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.calc
{
    public class Formula15 : IFormula
    {
        public Calc calc { get; set; }
        public double revenue_1(double a, double e, double i, double n, double t, double r, double o) {
            return a * e - o;
        }
        public double revenue_2(double a, double e, double i, double n, double t, double r, double o) {
            return n + (a + 1) * e / 2 - o;
        }
        public double revenue_3(double a, double e, double i, double n, double t, double r, double o) {
            return t * r + i * n - o;
        }
        public Calc outcomes_1_2_3(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = .5 + 1 / (2 * a) + 1 / i - e * (a - 1) / (2 * i * a);
            var t = (2 * a - e * (a - 1)) / (2 * i);
            var r = 1 / (n * a);
            var o = (a - 1) / (2 * n * a);
            var s = t / (n * a);

            calc = new Calc(new double[] { r, o, s }, Math.Round(100 * (r * a - 1), 2));
            return calc;
        }
        public Calc outcomes_1(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = (1 - (a - 1) * (i - 1) / 2) / (e - 1);
            var t = (a - 1) / 2;
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
            var n = (a + i - a * i) / (e - i);
            var t = a - 1 - n;
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[] { r, o, s }, Math.Round(100 * (((a + 1) / 2 + n) / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_3(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = (a - 1) / 2;
            var t = (a - 1) / 2;
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[] { r, o, s }, Math.Round(100 * ((e * n + i * t) / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_1_2(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = (a - 1) / 2;
            var t = (1 - (e - 1) * (a - 1) / 2) / (i - 1);
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[] { r, o, s }, Math.Round(100 * (a / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_2_3(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = (i * (a - 1) - (a + 1) / 2) / (i - e + 1);
            var t = ((a + 1) / 2 - (e - 1) * (a - 1)) / (i - e + 1);
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[] { r, o, s }, Math.Round(100 * ((e * n + i * t) / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_1_3(double[] p)
        {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = (a - i * (a - 1) / 2) / e;
            var t = (a - 1) / 2;
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[] { r, o, s }, Math.Round(100 * ((e * n + i * t) / (1 + n + t) - 1), 2));
            return calc;
        }
    }
}
