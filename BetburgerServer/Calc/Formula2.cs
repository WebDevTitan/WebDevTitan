using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.calc
{
    public class Formula2 : IFormula
    {
        public Calc calc {get; set;}
        public double revenue_1(double a, double e, double i, double n, double t, double r, double o) {
                return a * e - o;
        }
        public double revenue_2(double a, double e, double i, double n, double t, double r, double o) {
            return i * n - o;
        }
        public double revenue_3(double a, double e, double i, double n, double t, double r, double o) {
            return t * r - o;
        }
        public Calc outcomes_1_2_3(double[] p) {
            var n = 1 / p[0] + 1 / p[1] + 1 / p[2];
            var t = 1 / (n * p[0]);
            var r = 1 / (n * p[1]);
            var o = 1 / (n * p[2]);
            
            calc = new Calc(new double[] {t, r, o}, (double)Math.Round(100 * (t * p[0] - 1), 2));
            return calc;
        }
        public Calc outcomes_1(double[] p) {
            var n = p[2] / (p[1] * p[2] - p[1] - p[2]);
            var t = (1 + n) / (p[2] - 1);
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);
            
            calc = new Calc(new double[]{r, o, s}, (double)Math.Round(100 * (p[0] / (1 + n + t) - 1)));
            return calc;
        }
        public Calc outcomes_2(double[] p) {
            var n = p[0] - 1 - p[0] / p[2];
            var t = p[0] / p[2];
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[]{r, o, s}, (double)Math.Round(100 * (p[1] * n / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_3(double[] p) {
            var n = p[0] / p[1];
            var t = (p[1] - 1) * n - 1;
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);
            
            calc = new Calc(new double[]{r, o, s}, (double)Math.Round(100 * (p[2] * t / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_1_2(double[] p) {
            var n = p[0] / p[1];
            var t = (1 + n) / (p[2] - 1);
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);
            
            calc = new Calc(new double[]{r, o, s}, (double)Math.Round(100 * (p[0] / (1 + n + t) - 1), 2));
            return calc;    
        }
        public Calc outcomes_2_3(double[] p) {
            var n = p[2] * (p[0] - 1) / (p[1] + p[2]);
            var t = p[1] * (p[0] - 1) / (p[1] + p[2]);
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[]{r, o, s}, (double)Math.Round(100 * (p[1] * n / (1 + n + t) - 1), 2));
            return calc;
        }
        public Calc outcomes_1_3(double[] p) {
            var n = (1 + p[0] / p[2]) / (p[1] - 1);
            var t = p[0] / p[2];
            var r = 1 / (1 + n + t);
            var o = n / (1 + n + t);
            var s = t / (1 + n + t);

            calc = new Calc(new double[]{r, o, s}, (double)Math.Round(100 * (p[2] * t / (1 + n + t) - 1), 2));
            return calc;
        }
    }
}
