using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.calc
{
    public interface IFormula
    {
        Calc calc { get; set; }
        double revenue_1(double a, double e, double i, double n, double t, double r, double o);
        double revenue_2(double a, double e, double i, double n, double t, double r, double o);
        double revenue_3(double a, double e, double i, double n, double t, double r, double o);
        Calc outcomes_1_2_3(double[] p);
        Calc outcomes_1_2(double[] p);
        Calc outcomes_1(double[] p);
        Calc outcomes_2(double[] p);
    }
}
