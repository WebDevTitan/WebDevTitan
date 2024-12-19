using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.Model
{
    public class JsonArb
    {
        public List<JsonArbArb> arbs { get; set; }
        public List<JsonArbBet> bets { get; set; }
        public string[] event_arbs { get; set; }
        public JsonArbExcluded excluded { get; set; }
        public long last_update { get; set; }
        public int limit { get; set; }
        public double max_percent { get; set; }
        public double max_percent_by_filter { get; set; }
        public string project_type { get; set; }
        public string request_domain { get; set; }
        public double request_time { get; set; }
        public long total { get; set; }
        public int total_by_filter { get; set; }
        public double total_time { get; set; }
        public object[] wrong_items { get; set; }
        public bool isLive { get; set; }
        public JsonArbBet getJsonArbBetById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            foreach (JsonArbBet arbBet in bets)
            {
                if (arbBet.id == id)
                    return arbBet;
            }

            return null;
        }

        public void filterJsonArb()
        {
            List<JsonArbArb> delList = new List<JsonArbArb>();
            List<string> idList = new List<string>();
            foreach (JsonArbArb arb in arbs)
            {
                int nDup = 0;
                if (idList.Contains(arb.id))
                {
                    foreach (JsonArbArb arbSrc in arbs)
                    {
                        if (arb.id == arbSrc.id)
                        {
                            nDup++;
                            if (nDup > 1)
                            {
                                delList.Add(arb);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    idList.Add(arb.id);
                    continue;
                }
            }

            foreach (JsonArbArb arb in delList)
            {
                arbs.Remove(arb);
            }
        }

        private bool isExist(JsonArbArb arb)
        {
            int nDup = 0;
            foreach (JsonArbArb arbSrc in arbs)
            {
                if (arb.id == arbSrc.id)
                {
                    nDup++;
                    if (nDup > 1)
                        return true;
                }
            }

            return false;
        }
    }
}
