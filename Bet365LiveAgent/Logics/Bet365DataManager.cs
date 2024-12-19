using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;
using Bet365LiveAgent.Data.Soccer;
using System.Security.Cryptography.X509Certificates;
using System.Web.ModelBinding;

namespace Bet365LiveAgent.Logics
{
    class Bet365DataManager
    {
        private static Bet365DataManager _instance = null;
        public static Bet365DataManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Bet365DataManager();
                return _instance;
            }
        }

        public Bet365DataManager()
        {
            
        }        

        private JObject ParseTopicData(char type, string topic, string message, string[] headers)
        {
            JObject jObjResult = new JObject();

            try
            {                                
                char msgType = message.First();
                string msgData = message.Substring(1);
                string[] stemsData = msgData.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                jObjResult["Topic"] = topic;
                jObjResult["Type"] = type;
                jObjResult["MsgType"] = msgType;
                switch (msgType)
                {
                    case 'F':
                    case 'I':
                        {
                            JObject jObjCS = null, jObjCL = null, jObjEV = null, jObjMG = null, jObjMA = null, jObjCO = null, jObjPA = null, 
                                jObjTG = null, jObjTE = null, jObjES = null, jObjSC = null, jObjSL = null, jObjSG = null, jObjST = null;
                            jObjResult["Data"] = new JObject();
                            for (int i = 0; i < stemsData.Length; i++)
                            {
                                JObject jObjStem = ParseStemData(stemsData[i]);
                                string stemType = jObjStem["Type"] == null ? "-" : jObjStem["Type"].ToString();
                                string stemIT = jObjStem["IT"] == null ? "-" : jObjStem["IT"].ToString();
                                switch (stemType)
                                {
                                    case "IN":
                                    case "CG":
                                        jObjResult["Data"] = jObjStem;
                                        break;
                                    case "CS":
                                        jObjCS = jObjStem;
                                        jObjResult["Data"][stemIT] = jObjStem;
                                        break;
                                    case "CL":
                                        jObjCL = jObjStem;
                                        if (jObjCS == null)
                                            jObjResult["Data"][stemIT] = jObjStem;
                                        else
                                        {
                                            if (jObjCS["CLData"] == null)
                                                jObjCS["CLData"] = new JObject();
                                            jObjCS["CLData"][stemIT] = jObjStem;
                                        }
                                        break;
                                    case "EV":
                                        jObjEV = jObjStem;
                                        if (jObjCL == null)
                                            jObjResult["Data"][stemIT] = jObjStem;
                                        else
                                        {
                                            if (jObjCL["EVData"] == null)
                                                jObjCL["EVData"] = new JObject();
                                            jObjCL["EVData"][stemIT] = jObjStem;
                                        }
                                        break;                                    
                                    case "TG":
                                        jObjTG = jObjStem;
                                        if (jObjEV == null)
                                            jObjResult["Data"][stemIT] = jObjStem;
                                        else
                                        {
                                            if (jObjEV["TGData"] == null)
                                                jObjEV["TGData"] = new JObject();
                                            jObjEV["TGData"][stemIT] = jObjStem;
                                        }
                                        break;
                                    case "TE":
                                        jObjTE = jObjStem;
                                        if (jObjTG == null)
                                            jObjResult["Data"][stemIT] = jObjStem;
                                        else
                                        {
                                            if (jObjTG["TEData"] == null)
                                                jObjTG["TEData"] = new JObject();
                                            jObjTG["TEData"][stemIT] = jObjStem;
                                        }
                                        break;
                                    case "ES":
                                        jObjES = jObjStem;
                                        if (jObjEV == null)
                                            jObjResult["Data"][stemIT] = jObjStem;
                                        else
                                        {
                                            if (jObjEV["ESData"] == null)
                                                jObjEV["ESData"] = new JObject();
                                            jObjEV["ESData"][stemIT] = jObjStem;
                                        }
                                        break;
                                    case "SC":
                                        jObjSC = jObjStem;
                                        if (jObjES == null)
                                            jObjResult["Data"][stemIT] = jObjStem;
                                        else
                                        {
                                            if (jObjES["SCData"] == null)
                                                jObjES["SCData"] = new JObject();
                                            jObjES["SCData"][stemIT] = jObjStem;
                                        }
                                        break;
                                    case "SL":
                                        jObjSL = jObjStem;
                                        if (jObjSC == null)
                                            jObjResult["Data"][stemIT] = jObjStem;
                                        else
                                        {
                                            if (jObjSC["SLData"] == null)
                                                jObjSC["SLData"] = new JObject();
                                            jObjSC["SLData"][stemIT] = jObjStem;
                                        }
                                        break;
                                    case "MG":
                                        jObjMG = jObjStem;
                                        if (jObjEV == null)
                                            jObjResult["Data"][stemIT] = jObjStem;
                                        else
                                        {
                                            if (jObjEV["MGData"] == null)
                                                jObjEV["MGData"] = new JObject();
                                            jObjEV["MGData"][stemIT] = jObjStem;
                                        }
                                        break;
                                    case "MA":
                                        jObjMA = jObjStem;
                                        if (jObjEV == null)
                                            jObjResult["Data"][stemIT] = jObjStem;
                                        else if (jObjMG == null)
                                        {
                                            if (jObjEV["MAData"] == null)
                                                jObjEV["MAData"] = new JObject();
                                            jObjEV["MAData"][stemIT] = jObjStem;
                                        }
                                        else
                                        {
                                            if (jObjMG["MAData"] == null)
                                                jObjMG["MAData"] = new JObject();
                                            jObjMG["MAData"][stemIT] = jObjStem;
                                        }
                                        break;
                                    case "CO":
                                        jObjCO = jObjStem;
                                        if (jObjMA == null)
                                            jObjResult["Data"][stemIT] = jObjStem;
                                        else
                                        {
                                            if (jObjMA["COData"] == null)
                                                jObjMA["COData"] = new JObject();
                                            jObjMA["COData"][stemIT] = jObjStem;
                                        }
                                        break;
                                    case "PA":
                                        jObjPA = jObjStem;
                                        if (jObjMA == null)
                                            jObjResult["Data"][stemIT] = jObjStem;
                                        else
                                        {
                                            if (jObjMA["PAData"] == null)
                                                jObjMA["PAData"] = new JObject();
                                            jObjMA["PAData"][stemIT] = jObjStem;
                                        }
                                        break;
                                    case "SG":
                                        jObjSG = jObjStem;
                                        if (jObjEV == null)
                                            jObjResult["Data"][stemIT] = jObjStem;
                                        else
                                        {
                                            if (jObjEV["SGData"] == null)
                                                jObjEV["SGData"] = new JObject();
                                            jObjEV["SGData"][stemIT] = jObjStem;
                                        }
                                        break;
                                    case "ST":
                                        jObjST = jObjStem;
                                        if (jObjSG == null)
                                            jObjResult["Data"][stemIT] = jObjStem;
                                        else
                                        {
                                            if (jObjSG["STData"] == null)
                                                jObjSG["STData"] = new JObject();
                                            jObjSG["STData"][stemIT] = jObjStem;
                                        }
                                        break;
                                    default:
                                        if (stemType != "CT" && stemType != "ER")
                                        {

                                        }
                                        jObjResult["Data"][stemIT] = jObjStem;
                                        break;
                                }
                            }
                        }
                        break;
                    case 'U':
                    case 'D':
                        {
                            jObjResult["Data"] = new JArray();
                            for (int i = 0; i < stemsData.Length; i++)
                            {
                                JObject jObjStem = ParseStemData(stemsData[i]);
                                ((JArray)jObjResult["Data"]).Add(jObjStem);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, ex.ToString());
            }

            return jObjResult;
        }

        private JObject ParseStemData(string data)
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
                    else if(i == 0)
                    {
                        jObjResult["Type"] = props[i];
                    }
                }
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, ex.ToString());
            }
            
            return jObjResult;
        }

        public JArray ParseBet365Data(string strData)
        {
            JArray jArrResult = new JArray();

            try
            {
                string[] packetsData = strData.Split(new string[] { Global.DELIM_MSG }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < packetsData.Length; i++)
                {
                    char packetType = packetsData[i].First();
                    switch (packetType)
                    {
                        case Global.INITIAL_TOPIC_LOAD:
                        case Global.DELTA:
                            string[] recordsData = packetsData[i].Split(new string[] { Global.DELIM_RECORD }, StringSplitOptions.RemoveEmptyEntries);
                            string[] headersData = recordsData[0].Split(new string[] { Global.DELIM_FIELD }, StringSplitOptions.RemoveEmptyEntries);
                            string strTopic = headersData[0].Substring(1);
                            string strMessage = packetsData[i].Substring(recordsData[0].Length + 1);
                            JObject jObjResult = ParseTopicData(packetType, strTopic, strMessage, headersData);
                            if (jObjResult.Count > 0)
                                jArrResult.Add(jObjResult);
                            break;
                        case Global.CLIENT_ABORT:
                            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, "Connnection abort!");
                            break;
                        case Global.CLIENT_CLOSE:
                            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, "Connnection close!");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, ex.ToString());
            }

            return jArrResult;
        }
    }
}
