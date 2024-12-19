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
    public class FullTimeMarket
    {
        [JsonIgnore]
        public string HomeIT { get; set; } = string.Empty;
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
                    return string.Format("{0:0.00}", 1 + op1 / op2);
                }
                else
                    return string.Empty;
            }
        }
        public string HomeSU { get; set; } = string.Empty;

        [JsonIgnore]
        public string DrawIT { get; set; } = string.Empty;
        [JsonIgnore]
        public string DrawOD { get; set; } = string.Empty;
        public string DrawOdds
        {
            get
            {
                string[] operands = DrawOD.Split(new char[] { '/' });
                if (operands.Length == 2)
                {
                    double op1 = Convert.ToDouble(operands[0]);
                    double op2 = Convert.ToDouble(operands[1]);
                    return string.Format("{0:0.00}", 1 + op1 / op2);
                }
                else
                    return string.Empty;
            }
        }
        public string DrawSU { get; set; } = string.Empty;

        [JsonIgnore]
        public string AwayIT { get; set; } = string.Empty;
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
                    return string.Format("{0:0.00}", 1 + op1 / op2);
                }
                else
                    return string.Empty;
            }
        }
        public string AwaySU { get; set; } = string.Empty;
    }
    [Serializable]
    public class FullTimeMarketData
    {        
        public FullTimeMarket MarketData { get; set; } = new FullTimeMarket();

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
                    Update(jPAData);
                }
            }
            else if ("PA".Equals(type))
            {
                if (jObjData["OR"] != null)
                {
                    if ("0".Equals(jObjData["OR"].ToString()))
                    {
                        if (jObjData["IT"] != null)
                            MarketData.HomeIT = jObjData["IT"].ToString();
                        if (jObjData["OD"] != null)
                            MarketData.HomeOD = jObjData["OD"].ToString();
                        if (jObjData["SU"] != null)
                            MarketData.HomeSU = jObjData["SU"].ToString();
                    }
                    else if ("1".Equals(jObjData["OR"].ToString()))
                    {
                        if (jObjData["IT"] != null)
                            MarketData.DrawIT = jObjData["IT"].ToString();
                        if (jObjData["OD"] != null)
                            MarketData.DrawOD = jObjData["OD"].ToString();
                        if (jObjData["SU"] != null)
                            MarketData.DrawSU = jObjData["SU"].ToString();
                    }
                    else if ("2".Equals(jObjData["OR"].ToString()))
                    {
                        if (jObjData["IT"] != null)
                            MarketData.AwayIT = jObjData["IT"].ToString();
                        if (jObjData["OD"] != null)
                            MarketData.AwayOD = jObjData["OD"].ToString();
                        if (jObjData["SU"] != null)
                            MarketData.AwaySU = jObjData["SU"].ToString();
                    }
                }
            }
        }
    }
}
