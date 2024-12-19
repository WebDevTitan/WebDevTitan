using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bet365LiveAgent.Data.Soccer
{
    [Serializable]
    public class AsianHandicapMarket
    {
        [JsonIgnore]
        public string HomeIT { get; set; } = string.Empty;
        public string HomeHdp { get; set; } = string.Empty;
        [JsonIgnore]
        public string HomeOD { get; set; } = string.Empty;
        public string HomeOdds
        {
            get
            {
                string[] operands = HomeOD.Split(new char[] { '/' });
                if (operands.Length == 2)
                {
                    double op1 = Convert.ToDouble(operands[0]);
                    double op2 = Convert.ToDouble(operands[1]);
                    return string.Format("{0:0.000}", 1 + op1 / op2);
                }
                else
                    return string.Empty;
            }
        }
        public string HomeSU { get; set; } = string.Empty;

        [JsonIgnore]
        public string AwayIT { get; set; } = string.Empty;
        public string AwayHdp { get; set; } = string.Empty;
        [JsonIgnore]
        public string AwayOD { get; set; } = string.Empty;
        public string AwayOdds
        {
            get
            {
                string[] operands = AwayOD.Split(new char[] { '/' });
                if (operands.Length == 2)
                {
                    double op1 = Convert.ToDouble(operands[0]);
                    double op2 = Convert.ToDouble(operands[1]);
                    return string.Format("{0:0.000}", 1 + op1 / op2);
                }
                else
                    return string.Empty;
            }
        }
        public string AwaySU { get; set; } = string.Empty;
    }
    [Serializable]
    public class AsianHandicapMarketData
    {
        public List<AsianHandicapMarket> MarketData { get; set; } = new List<AsianHandicapMarket>();

        public void Update(JObject jObjData)
        {
            string type = jObjData["Type"] == null ? "" : jObjData["Type"].ToString();
            if ("MG".Equals(type))
            {
                foreach (var jMAToken in jObjData["MAData"].ToObject<JObject>())
                {
                    JObject jMAData = jMAToken.Value.ToObject<JObject>();
                    Update(jMAData);
                }
            }
            else if ("MA".Equals(type))
            {
                foreach (var jPAToken in jObjData["PAData"].ToObject<JObject>())
                {
                    JObject jPAData = jPAToken.Value.ToObject<JObject>();
                    if ("1".Equals(jObjData["ID"].ToString()))
                    {
                        AsianHandicapMarket asianHandicapMarket = MarketData.Find(m => !string.IsNullOrWhiteSpace(m.AwayHdp) && decimal.Parse(m.AwayHdp) == (-1) * jPAData["HA"].ToObject<decimal>());
                        if (asianHandicapMarket == null)
                        {
                            asianHandicapMarket = new AsianHandicapMarket();
                            MarketData.Add(asianHandicapMarket);
                        }
                        if (jPAData["IT"] != null)
                            asianHandicapMarket.HomeIT = jPAData["IT"].ToString();
                        if (jPAData["HA"] != null)
                            asianHandicapMarket.HomeHdp = jPAData["HA"].ToString();
                        if (jPAData["OD"] != null)
                            asianHandicapMarket.HomeOD = jPAData["OD"].ToString();
                        if (jPAData["SU"] != null)
                            asianHandicapMarket.HomeSU = jPAData["SU"].ToString();
                    }
                    else if ("2".Equals(jObjData["ID"].ToString()))
                    {
                        AsianHandicapMarket asianHandicapMarket = MarketData.Find(m => !string.IsNullOrWhiteSpace(m.HomeHdp) && decimal.Parse(m.HomeHdp) == (-1) * jPAData["HA"].ToObject<decimal>());
                        if (asianHandicapMarket == null)
                        {
                            asianHandicapMarket = new AsianHandicapMarket();
                            MarketData.Add(asianHandicapMarket);
                        }
                        if (jPAData["IT"] != null)
                            asianHandicapMarket.AwayIT = jPAData["IT"].ToString();
                        if (jPAData["HA"] != null)
                            asianHandicapMarket.AwayHdp = jPAData["HA"].ToString();
                        if (jPAData["OD"] != null)
                            asianHandicapMarket.AwayOD = jPAData["OD"].ToString();
                        if (jPAData["SU"] != null)
                            asianHandicapMarket.AwaySU = jPAData["SU"].ToString();
                    }
                }
            }
        }
    }
}
