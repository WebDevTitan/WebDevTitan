using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class BetCryptHeader
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public BetCryptHeader(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
    public class BetHeaderInfo
    {
        public List<BetCryptHeader> Pirxtheaders { get; set; }
        public string domain { get; set; }
        public DateTime Tick { get; set; }

        public string NSToken { get; set; }
        public BetHeaderInfo()
        {
            NSToken = "";
            Pirxtheaders = new List<BetCryptHeader>();
            Tick = DateTime.MinValue;
        }
    }
}
