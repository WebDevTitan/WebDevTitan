using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.calc
{
    public class Formula3 : IFormula
    {
        public Calc calc {get; set;}
        public double revenue_1(double a, double e, double i, double n, double t, double r, double o) {
            return a * e - o;
        }
        public double revenue_2(double a, double e, double i, double n, double t, double r, double o) {
            return i * n + e - o;
        }
        public double revenue_3(double a, double e, double i, double n, double t, double r, double o) {
            return t * r - o;
        }
        public Calc outcomes_1_2_3(double[] p) {
            
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = 1 / a + 1 / i + (a - 1) / (a * e);
            var t = 1 / (n * a);
            var r = (a - 1) / (n * a * e);
            var o = 1 / (n * i);

            calc = new Calc(new double[]{t, r, o}, Math.Round(100 * (t * a - 1), 2));
            return calc;
        }
        public Calc outcomes_1(double[] p) {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = 1 / (e * i - e - i);
            var t = n * (e - 1);
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[] {r, o, s}, Math.Round(100 * (a / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_2(double[] p) {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = a - 1 - a / i;
            var t = a / i;
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[]{r, o, s}, Math.Round(100 * ((e * n + 1) / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_3(double[] p) {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = (a - 1) / e;
            var t = (e - 1) * (a - 1) / e;
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[]{r, o, s}, Math.Round(100 * (i * t / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_1_2(double[] p) {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = (a - 1) / e;
            var t = (e + a - 1) / (e * (i - 1));
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[]{r, o, s}, Math.Round(100 * (a / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_2_3(double[] p) {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = (i * (a - 1) - 1) / (e + i);
            var t = a - 1 - n;
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[]{r, o, s}, Math.Round(100 * (i * t / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_1_3(double[] p) {
            var a = p[0];
            var e = p[1];
            var i = p[2];
            var n = a / (i * (e - 1));
            var t = a / i;
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[]{r, o, s}, Math.Round(100 * (i * t / (1 + n + t) - 1), 2));
            return calc;
        }
    }
}
