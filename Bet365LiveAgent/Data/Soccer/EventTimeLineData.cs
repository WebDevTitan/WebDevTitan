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
    public class EventTimeLineData
    {        
        public string IC { get; set; } = string.Empty; // 1: Score, 2: Goal, 4: Yellow Card, 5: Red Card, 7: Corner
        public string LA { get; set; } = string.Empty;
        public string TM { get; set; } = string.Empty;

        public void Update(JObject jObjData)
        {
            if (jObjData["IC"] != null)
                IC = jObjData["IC"].ToString();
            if (jObjData["LA"] != null)
                LA = jObjData["LA"].ToString();
            if (jObjData["TM"] != null)
                TM = jObjData["TM"].ToString();
        }
    }
}
