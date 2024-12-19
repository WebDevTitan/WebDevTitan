using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Json
{
    public class BetSlipJson
    {
        public string bg { get; set; }
        public string cc { get; set; }
        public int sr { get; set; }
        public bool mr { get; set; }
        public bool ir { get; set; }
        public int at { get; set; }
        public string vr { get; set; }
        public bool tx { get; set; } = false;
        public int cs { get; set; }
        public int st { get; set; }
        public List<BetSlipItem> bt { get; set; } = new List<BetSlipItem>();
        public Dm dm { get; set; }

        public string pc { get; set; }

        public List<Dm> mo { get; set; } = new List<Dm>();

        public List<int> bs { get; set; } = new List<int>();
    }

    public class BetSlipItem
    {
        public dynamic ob { get; set; }
        public int cl { get; set; }

        public bool su { get; set; }
        public string sa { get; set; }
        public string tp { get; set; }
        public string fb { get; set; }
        public bool ex { get; set; }
        public bool ew { get; set; }
        public bool ea { get; set; }
        public int ed { get; set; }
        public int mt { get; set; }
        public bool oc { get; set; }
        public bool mr { get; set; }
        public int bt { get; set; }
        public string oo { get; set; }
        public string pf { get; set; }
        public string od { get; set; }
        public long fi { get; set; }
        public string fd { get; set; }
        public List<PartType> pt { get; set; } = new List<PartType>();
        public int sr { get; set; }
        public string re { get; set; }
        public double ms { get; set; }
    }

    public class PartType
    {
        public string pi { get; set; }
        public string bd { get; set; }
        public string md { get; set; }
        public string ha { get; set; }
        public bool hc { get; set; }
    }

    public class Dm
    {
        public int bt { get; set; }
        public string od { get; set; }
        public string bd { get; set; }
        public int bc { get; set; }
        public bool ea { get; set; }
        public bool cb { get; set; }
        public int ma { get; set; }

        public double ms { get; set; }
    }
}
