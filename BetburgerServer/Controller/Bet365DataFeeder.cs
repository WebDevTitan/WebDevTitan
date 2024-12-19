using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.Controller
{
    public class Bet365DataFeeder
    {
        private static JObject ParseStemData(string data)
        {
            JObject jObjResult = new JObject();

            try
            {
                string[] props = data.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < props.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(props[i]))
                        continue;
                    if (props[i].Contains("="))
                    {
                        string[] fields = props[i].Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                        if (fields.Length == 2)
                            jObjResult[fields[0]] = fields[1];
                        else
                            jObjResult[fields[0]] = string.Empty;
                    }
                    else if (i == 0)
                    {
                        jObjResult["Type"] = props[i];
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Exception 4 {0} {1}", ex.Message, ex.StackTrace));
            }

            return jObjResult;
        }
        public static JObject ParseData(string message)
        {
            JObject jObjResult = new JObject();

            try
            {
                char msgType = message.First();
                string msgData = message.Substring(1);
                string[] stemsData = msgData.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                
                
                switch (msgType)
                {
                    case 'F':
                    case 'I':
                        {
                            JObject jObjCL = null, jObjEV = null, jObjMA = null, jObjMG = null;
                            jObjResult["MsgType"] = stemsData[0];
                            jObjResult["Data"] = new JArray();                            
                            for (int i = 0; i < stemsData.Length; i++)
                            {
                                JObject jObjStem = ParseStemData(stemsData[i]);
                                string stemType = jObjStem["Type"] == null ? "-" : jObjStem["Type"].ToString();                                
                                switch (stemType)
                                {
                                    case "CL":
                                        jObjCL = jObjStem;
                                        if (jObjResult["CLData"] == null)
                                        {
                                            jObjResult["CLData"] = new JArray();
                                        }
                                        (jObjResult["CLData"] as JArray).Add(jObjCL);
                                        break;
                                    case "EV":
                                        jObjEV = jObjStem;
                                        if (jObjCL == null)
                                            (jObjResult["Data"] as JArray).Add(jObjStem);
                                        else
                                        {
                                            if (jObjCL["EVData"] == null)
                                                jObjCL["EVData"] = new JArray();
                                            (jObjCL["EVData"] as JArray).Add(jObjStem);
                                        }
                                        break;                                   
                                    case "MG":
                                        jObjMG = jObjStem;
                                        if (jObjEV == null)
                                            (jObjResult["Data"] as JArray).Add(jObjStem);
                                        else
                                        {
                                            if (jObjEV["MGData"] == null)
                                                jObjEV["MGData"] = new JArray();
                                            (jObjEV["MGData"] as JArray).Add(jObjStem);
                                        }
                                        break;
                                    case "MA":
                                        jObjMA = jObjStem;
                                        if (jObjMG == null)
                                            (jObjResult["Data"] as JArray).Add(jObjStem);
                                        else
                                        {
                                            if (jObjMG["MAData"] == null)
                                                jObjMG["MAData"] = new JArray();
                                            (jObjMG["MAData"] as JArray).Add(jObjStem);
                                        }
                                        break;                                    
                                    case "PA":                                        
                                        if (jObjMA == null)
                                            (jObjResult["Data"] as JArray).Add(jObjStem);
                                        else
                                        {
                                            if (jObjMA["PAData"] == null)
                                                jObjMA["PAData"] = new JArray();
                                            (jObjMA["PAData"] as JArray).Add(jObjStem);
                                        }
                                        break;                                    
                                    default:
                                        (jObjResult["Data"] as JArray).Add(jObjStem);
                                        break;
                                }
                            }
                        }
                        break;                  
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception 3 {0} {1}", ex.Message, ex.StackTrace));
            }

            return jObjResult;
        }        

    }
}
