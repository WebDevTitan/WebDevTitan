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
    public class EventLocationData
    {
        public string LE { get; set; } = string.Empty; // GO: Goals, FK: FreeKicks, ST: OnTarget, OF: Offsides
        public string PD { get; set; } = string.Empty;
        public string TE { get; set; } = string.Empty;
        public string XY { get; set; } = string.Empty;

        public void Update(JObject jObjData)
        {
            if (jObjData["LE"] != null)
                LE = jObjData["LE"].ToString();
            if (jObjData["PD"] != null)
                PD = jObjData["PD"].ToString();
            if (jObjData["TE"] != null)
                TE = jObjData["TE"].ToString();
            if (jObjData["XY"] != null)
                XY = jObjData["XY"].ToString();
        }
    }
}
