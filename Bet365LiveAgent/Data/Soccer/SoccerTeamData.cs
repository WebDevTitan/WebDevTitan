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
    public class SoccerTeamData
    {
        public string Order { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string KC { get; set; } = string.Empty;
        public string KI { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Score { get; set; } = string.Empty;
        public string OnTarget { get; set; } = string.Empty;
        public string OffTarget { get; set; } = string.Empty;
        public string Attack { get; set; } = string.Empty;
        public string DangerAttack { get; set; } = string.Empty;
        public string Possession { get; set; } = string.Empty;
        public string Corner { get; set; } = string.Empty;
        public string YellowCard { get; set; } = string.Empty;
        public string RedCard { get; set; } = string.Empty;
        public string Penalty { get; set; } = string.Empty;

        public void Update(JObject jObjData)
        {
            if (jObjData["ID"] != null)
                Order = jObjData["ID"].ToString();
            if (jObjData["NA"] != null)
                Name = jObjData["NA"].ToString();
            if (jObjData["KC"] != null)
                KC = jObjData["KC"].ToString();
            if (jObjData["KI"] != null)
                KI = jObjData["KI"].ToString();
            if (jObjData["TC"] != null)
                Color = jObjData["TC"].ToString();
            if (jObjData["S1"] != null)            
                OnTarget = jObjData["S1"].ToString();
            if (jObjData["S2"] != null)
                OffTarget = jObjData["S2"].ToString();
            if (jObjData["S3"] != null)
                Attack = jObjData["S3"].ToString();
            if (jObjData["S4"] != null)
                DangerAttack = jObjData["S4"].ToString();
            if (jObjData["S7"] != null)
                Possession = jObjData["S7"].ToString();
        }
    }
}
