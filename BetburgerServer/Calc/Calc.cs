using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.calc
{
    public class Calc
    {
        public double[] stakes { get; set; }
        public double percent { get; set; }

        public Calc(double[] _stakes, double _percent)
        {
            stakes = _stakes;
            percent = _percent;
        }
    }
}
