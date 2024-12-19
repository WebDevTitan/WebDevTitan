using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bet365LiveAgent.Data.Soccer
{
    [Serializable]
    public class MatchGoalMarket
    {
        [JsonIgnore]
        public string OverIT { get; set; } = string.Empty;
        public string OverHdp { get; set; } = string.Empty;
        [JsonIgnore]
        public string OverOD { get; set; } = string.Empty;
        public string OverOdds
        {
            get
            {
                string[] operands = OverOD.Split(new char[] { '/' });
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
        public string OverSU { get; set; } = string.Empty;

        [JsonIgnore]
        public string UnderIT { get; set; } = string.Empty;
        public string UnderHdp { get; set; } = string.Empty;
        [JsonIgnore]
        public string UnderOD { get; set; } = string.Empty;
        public string UnderOdds
        {
            get
            {
                string[] operands = UnderOD.Split(new char[] { '/' });
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
        public string UnderSU { get; set; } = string.Empty;
    }
    [Serializable]
    public class MatchGoalMarketData
    {
        public List<MatchGoalMarket> MarketData { get; set; } = new List<MatchGoalMarket>();

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
                        MatchGoalMarket goalLineMarket = MarketData.Find(m => !string.IsNullOrWhiteSpace(m.UnderHdp) && decimal.Parse(m.UnderHdp) == jPAData["HA"].ToObject<decimal>());
                        if (goalLineMarket == null)
                        {
                            goalLineMarket = new MatchGoalMarket();
                            MarketData.Add(goalLineMarket);
                        }
                        if (jPAData["IT"] != null)
                            goalLineMarket.OverIT = jPAData["IT"].ToString();
                        if (jPAData["HA"] != null)
                            goalLineMarket.OverHdp = jPAData["HA"].ToString();
                        if (jPAData["OD"] != null)
                            goalLineMarket.OverOD = jPAData["OD"].ToString();
                        if (jPAData["SU"] != null)
                            goalLineMarket.OverSU = jPAData["SU"].ToString();
                    }
                    else if ("2".Equals(jObjData["ID"].ToString()))
                    {
                        MatchGoalMarket goalLineMarket = MarketData.Find(m => !string.IsNullOrWhiteSpace(m.OverHdp) && decimal.Parse(m.OverHdp) == jPAData["HA"].ToObject<decimal>());
                        if (goalLineMarket == null)
                        {
                            goalLineMarket = new MatchGoalMarket();
                            MarketData.Add(goalLineMarket);
                        }
                        if (jPAData["IT"] != null)
                            goalLineMarket.UnderIT = jPAData["IT"].ToString();
                        if (jPAData["HA"] != null)
                            goalLineMarket.UnderHdp = jPAData["HA"].ToString();
                        if (jPAData["OD"] != null)
                            goalLineMarket.UnderOD = jPAData["OD"].ToString();
                        if (jPAData["SU"] != null)
                            goalLineMarket.UnderSU = jPAData["SU"].ToString();
                    }
                }
            }
        }
    }
}
