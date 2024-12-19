using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;
using Bet365LiveAgent.Data.Soccer;
using System.Threading;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Bet365LiveAgent.Logics
{
    public delegate void onWriteStatusEvent(string status);
    class Bet365AgentManager
    {
        private BET365AGENT_STATUS _status = BET365AGENT_STATUS.Stoped;

        private WebSocketServer _webSocketServer = null;

        private int _matchRequestCnt = 0;

        #region SportsMatches
        private List<SoccerMatchData> _soccermatches = null;
        public List<SoccerMatchData> SoccerMatches
        {
            get
            {
                return _soccermatches;
            }
        }        
        #endregion

        private object _lockMatches = new object();

        public ProcessBet365DataDelegate OnBet365ProcessData = null;
        public Bet365DataReceivedHandler OnBet365DataReceived = null;
        public Bet365DataReceivedHandler OnBet365PrivateDataReceived = null;

        private static Bet365AgentManager _instance = null;


        public void CheckResult()
        {

        }

        private static void WriteBetHistory(PickResultData pickData)
        {          
            try
            {
                string text = JsonConvert.SerializeObject(pickData);
                System.IO.File.AppendAllText(@"pickHistory.txt", text + Environment.NewLine);
                
            }
            catch { }
        }
             

        public static Bet365AgentManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Bet365AgentManager();
                return _instance;
            }
        }

        public Bet365AgentManager()
        {
            
        }

        public void Start()
        {
            try
            {
                if (_webSocketServer == null || !_webSocketServer.IsListening)
                {
                    /* original commented * /
                    Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "Bet365AgentManager Started.");
                    _status = BET365AGENT_STATUS.Started;
                    _webSocketServer = new WebSocketServer(Config.Instance.ServerListenPort, false);
                    _webSocketServer.AddWebSocketService<WebSocketClient>("/zap");
                    _webSocketServer.Log.Level = LogLevel.Error;
                    _webSocketServer.Log.File = $"{Global.LogFilePath}WebSocketServer-{Global.LogFileName}";
                    _webSocketServer.Start();
                    /* */
                    _soccermatches = new List<SoccerMatchData>();
                    OnBet365ProcessData = ProcessBet365Data;
                    OnBet365DataReceived = ReceiveBet365Data;
                    // 20220210
                    OnBet365PrivateDataReceived = ReceiveBet365Data;
                    // 20220210

                    _matchRequestCnt = 0;
                }
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, ex.ToString());
            }
        }

        public void Stop()
        {
            try
            {
                if (_webSocketServer != null && _webSocketServer.IsListening)
                {
                    Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "Bet365AgentManager Stoped.");
                    _status = BET365AGENT_STATUS.Stoped;
                    _webSocketServer.Stop();
                }
                _webSocketServer = null;
                _soccermatches = null;
                OnBet365DataReceived = null;

                _matchRequestCnt = 0;
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, ex.StackTrace);
            }
        }

        private void BroadCastMessage(string strMessage)
        {
            /*try
            {
                if (string.IsNullOrWhiteSpace(strMessage))
                    return;

                if (_webSocketServer != null && _webSocketServer.IsListening)
                {
#if DEBUG
                    Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, strMessage);
#else
                    Global.WriteLog(LOGLEVEL.FILE, LOGTYPE.OUTDATA, strMessage);
#endif
                    _webSocketServer.WebSocketServices["/zap"].Sessions.Broadcast(strMessage);
                }
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, ex.ToString());
            }*/
        }

        private void ReceiveBet365Data(string strData)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strData))
                    return;
#if DEBUG
                //Trace.WriteLine(strData);
#endif
                JArray jArrData = Bet365DataManager.Instance.ParseBet365Data(strData);
                if  (jArrData.Count > 0)
                {

                    Global.WriteLog(LOGLEVEL.FILE, LOGTYPE.OUTDATA, JsonConvert.SerializeObject(jArrData));
#if DEBUG
                    Trace.WriteLine(JsonConvert.SerializeObject(jArrData));
#endif

                    ProcessBet365Data(jArrData);
                }
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, ex.ToString());
            }
        }
        
        private void ProcessBet365Data(JArray jArrData)
        {
            try
            {                
                lock (_lockMatches)
                {
                    List<SoccerMatchData> updatedSoccerMatches = new List<SoccerMatchData>();
                    foreach (JObject jObjData in jArrData)
                    {
                        string topic = jObjData["Topic"].ToString();
                        char type = jObjData["Type"].ToObject<char>();
                        char msgType = jObjData["MsgType"].ToObject<char>();
                        if (type == Global.INITIAL_TOPIC_LOAD)
                        {
                            if (topic.Equals($"OVInPlay_{Global.LANG_ID}_{Global.ZONE_ID}"))
                            {
                                _matchRequestCnt = 0;
                                JObject jObjCLsData = jObjData["Data"].ToObject<JObject>();
                                foreach (var jCLToken in jObjCLsData)
                                {
                                    JObject jObjCLData = jCLToken.Value.ToObject<JObject>();
                                    if (jObjCLData["NA"] != null && "Soccer".Equals(jObjCLData["NA"].ToString()) )
                                    {
                                        _soccermatches.Clear();
                                        JObject jObjEVsData = jObjCLData["EVData"].ToObject<JObject>();
                                        int nReqCnt = 1;
                                        foreach (var jEVToken in jObjEVsData)
                                        {
                                            JObject jObjEVData = jEVToken.Value.ToObject<JObject>();
                                            SoccerMatchData soccerMatchData = new SoccerMatchData();
                                            soccerMatchData.Update(jObjEVData);
                                            if (string.IsNullOrWhiteSpace(soccerMatchData.FixtureID))
                                            {
                                                Task.Delay(nReqCnt * 2000).ContinueWith(t => RequestMatchLiveData(soccerMatchData.ID, soccerMatchData.EventID, "1"));
                                                nReqCnt++;
                                                _soccermatches.Add(soccerMatchData);
                                                BroadCastMessage($"I{JsonConvert.SerializeObject(soccerMatchData)}");
                                            }
                                        }
                                    }
                                }
                            }
                            else if (topic.EndsWith($"C1A_{Global.LANG_ID}_{Global.ZONE}") || topic.EndsWith($"M1_{Global.LANG_ID}"))
                            {
                                // Soccer
                                JObject jObjEVsData = jObjData["Data"].ToObject<JObject>();
                                foreach (var jEVToken in jObjEVsData)
                                {
                                    JObject jObjEVData = jEVToken.Value.ToObject<JObject>();
                                    string strEventID = $"{jObjEVData["C1"]}{jObjEVData["T1"]}{jObjEVData["C2"]}{jObjEVData["T2"]}";
                                    SoccerMatchData soccerMatchData = _soccermatches.Find(m => m.EventID == strEventID);
                                    if (soccerMatchData == null)
                                    {
                                        soccerMatchData = new SoccerMatchData();
                                        soccerMatchData.Update(jObjEVData);
                                        _soccermatches.Add(soccerMatchData);

                                        string message = JsonConvert.SerializeObject(soccerMatchData);
                                        BroadCastMessage($"I{message}");
                                    }
                                    else
                                    {
                                        soccerMatchData.Update(jObjEVData);
                                    }

                                    SoccerMatchData updatedSoccerMatch = updatedSoccerMatches.Find(m => m.EventID == strEventID);
                                    if (updatedSoccerMatch == null)
                                        updatedSoccerMatches.Add(soccerMatchData);                                    
                                }
                            }
                        }
                        else if (type == Global.DELTA)
                        {
                            SoccerMatchData soccerMatchData = null;
                            if (msgType == 'I')
                            {
                                string[] stemTopics = topic.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                                if (stemTopics.Length > 0)
                                {
                                    JObject jObjStemsData = jObjData["Data"].ToObject<JObject>();
                                    foreach (var jStemToken in jObjStemsData)
                                    {
                                        JObject jObjStemData = jStemToken.Value.ToObject<JObject>();
                                        string strType = jObjStemData["Type"] == null ? "" : jObjStemData["Type"].ToString();
                                        if (strType == "EV")
                                        {
                                            if (stemTopics[0].EndsWith($"C1_{Global.LANG_ID}_{Global.ZONE_ID}"))
                                            {
                                                SoccerMatchData soccerMatchDataToAdd = new SoccerMatchData();
                                                soccerMatchDataToAdd.Update(jObjStemData);
                                                if (string.IsNullOrWhiteSpace(soccerMatchDataToAdd.FixtureID))
                                                {
                                                    Task.Delay(1500).ContinueWith(t => RequestMatchLiveData(soccerMatchDataToAdd.ID, soccerMatchDataToAdd.EventID, "1"));
                                                }
                                            }
                                        }
                                        else if (strType == "PA")
                                        {
                                            if (stemTopics[0].StartsWith("6V") && stemTopics[0].EndsWith($"-1777_{Global.LANG_ID}_{Global.ZONE}")) // Changed to ZONE
                                            {
                                                string strFixtureID = stemTopics[0].TrimStart("6V").TrimEnd($"-1777_{Global.LANG_ID}_{Global.ZONE}"); // Changed to ZONE
                                                soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                                if (soccerMatchData != null)
                                                {
                                                    soccerMatchData.FullTime.Update(jObjStemData);
                                                }
                                            }
                                            else if (stemTopics[0].StartsWith("6V") && stemTopics[0].EndsWith($"C1-G12-1_{Global.LANG_ID}_{Global.ZONE}")) // Changed to ZONE
                                            {
                                                string strFixtureID = stemTopics[0].TrimStart("6V").TrimEnd($"C1-G12-1_{Global.LANG_ID}_{Global.ZONE}"); // Changed to ZONE
                                                soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                                if (soccerMatchData != null)
                                                {
                                                    AsianHandicapMarket asianHandicapMarket = soccerMatchData.AsianHandicap.MarketData.Find(m => !string.IsNullOrWhiteSpace(m.AwayHdp) && decimal.Parse(m.AwayHdp) == (-1) * jObjStemData["HA"].ToObject<decimal>());
                                                    if (asianHandicapMarket == null)
                                                    {
                                                        asianHandicapMarket = new AsianHandicapMarket();
                                                        soccerMatchData.AsianHandicap.MarketData.Add(asianHandicapMarket);
                                                    }
                                                    if (jObjStemData["IT"] != null)
                                                        asianHandicapMarket.HomeIT = jObjStemData["IT"].ToString();
                                                    if (jObjStemData["HA"] != null)
                                                        asianHandicapMarket.HomeHdp = jObjStemData["HA"].ToString();
                                                    if (jObjStemData["OD"] != null)
                                                        asianHandicapMarket.HomeOD = jObjStemData["OD"].ToString();
                                                    if (jObjStemData["SU"] != null)
                                                        asianHandicapMarket.HomeSU = jObjStemData["SU"].ToString();
                                                }
                                            }
                                            else if (stemTopics[0].StartsWith("6V") && stemTopics[0].EndsWith($"C1-G12-2_{Global.LANG_ID}_{Global.ZONE}")) // Changed to ZONE
                                            {
                                                string strFixtureID = stemTopics[0].TrimStart("6V").TrimEnd($"C1-G12-2_{Global.LANG_ID}_{Global.ZONE}"); // Changed to ZONE
                                                soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                                if (soccerMatchData != null)
                                                {
                                                    AsianHandicapMarket asianHandicapMarket = soccerMatchData.AsianHandicap.MarketData.Find(m => !string.IsNullOrWhiteSpace(m.HomeHdp) && decimal.Parse(m.HomeHdp) == (-1) * jObjStemData["HA"].ToObject<decimal>());
                                                    if (asianHandicapMarket == null)
                                                    {
                                                        asianHandicapMarket = new AsianHandicapMarket();
                                                        soccerMatchData.AsianHandicap.MarketData.Add(asianHandicapMarket);
                                                    }
                                                    if (jObjStemData["IT"] != null)
                                                        asianHandicapMarket.AwayIT = jObjStemData["IT"].ToString();
                                                    if (jObjStemData["HA"] != null)
                                                        asianHandicapMarket.AwayHdp = jObjStemData["HA"].ToString();
                                                    if (jObjStemData["OD"] != null)
                                                        asianHandicapMarket.AwayOD = jObjStemData["OD"].ToString();
                                                    if (jObjStemData["SU"] != null)
                                                        asianHandicapMarket.AwaySU = jObjStemData["SU"].ToString();
                                                }
                                            }
                                            else if (stemTopics[0].StartsWith("6V") && stemTopics[0].EndsWith($"C1-G15-2_{Global.LANG_ID}_{Global.ZONE}")) // Changed to ZONE
                                            {
                                                string strFixtureID = stemTopics[0].TrimStart("6V").TrimEnd($"C1-G15-2_{Global.LANG_ID}_{Global.ZONE}"); // Changed to ZONE
                                                soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                                if (soccerMatchData != null)
                                                {
                                                    GoalLineMarket goalLineMarket = soccerMatchData.GoalLine.MarketData.Find(m => !string.IsNullOrWhiteSpace(m.UnderHdp) && decimal.Parse(m.UnderHdp) == jObjStemData["HA"].ToObject<decimal>());
                                                    if (goalLineMarket == null)
                                                    {
                                                        goalLineMarket = new GoalLineMarket();
                                                        soccerMatchData.GoalLine.MarketData.Add(goalLineMarket);
                                                    }
                                                    if (jObjStemData["IT"] != null)
                                                        goalLineMarket.OverIT = jObjStemData["IT"].ToString();
                                                    if (jObjStemData["HA"] != null)
                                                        goalLineMarket.OverHdp = jObjStemData["HA"].ToString();
                                                    if (jObjStemData["OD"] != null)
                                                        goalLineMarket.OverOD = jObjStemData["OD"].ToString();
                                                    if (jObjStemData["SU"] != null)
                                                        goalLineMarket.OverSU = jObjStemData["SU"].ToString();
                                                }
                                            }
                                            else if (stemTopics[0].StartsWith("6V") && stemTopics[0].EndsWith($"C1-G15-3_{Global.LANG_ID}_{Global.ZONE}")) // Changed to ZONE
                                            {
                                                string strFixtureID = stemTopics[0].TrimStart("6V").TrimEnd($"C1-G15-3_{Global.LANG_ID}_{Global.ZONE}"); // Changed to ZONE
                                                soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                                if (soccerMatchData != null)
                                                {
                                                    GoalLineMarket goalLineMarket = soccerMatchData.GoalLine.MarketData.Find(m => !string.IsNullOrWhiteSpace(m.OverHdp) && decimal.Parse(m.OverHdp) == jObjStemData["HA"].ToObject<decimal>());
                                                    if (goalLineMarket == null)
                                                    {
                                                        goalLineMarket = new GoalLineMarket();
                                                        soccerMatchData.GoalLine.MarketData.Add(goalLineMarket);
                                                    }
                                                    if (jObjStemData["IT"] != null)
                                                        goalLineMarket.UnderIT = jObjStemData["IT"].ToString();
                                                    if (jObjStemData["HA"] != null)
                                                        goalLineMarket.UnderHdp = jObjStemData["HA"].ToString();
                                                    if (jObjStemData["OD"] != null)
                                                        goalLineMarket.UnderOD = jObjStemData["OD"].ToString();
                                                    if (jObjStemData["SU"] != null)
                                                        goalLineMarket.UnderSU = jObjStemData["SU"].ToString();
                                                }
                                            }
                                        }
                                        else if (strType == "ST")
                                        {
                                            string strID = jObjStemData["ID"] == null ? string.Empty : jObjStemData["ID"].ToString();
                                            if (stemTopics[0].EndsWith($"C1U1_{Global.LANG_ID}") && topic.EndsWith($"C1U{strID}_{Global.LANG_ID}")) // Removed to ZONE
                                            {
                                                string strFI = stemTopics[0].TrimStart("ML").TrimEnd($"C1U1_{Global.LANG_ID}"); // Removed to ZONE
                                                soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFI);
                                                if (soccerMatchData != null)
                                                {
                                                    Data.Soccer.EventTimeLineData eventTimeLineData = jObjStemData.ToObject<Data.Soccer.EventTimeLineData>();
                                                    if (eventTimeLineData != null)
                                                        soccerMatchData.EventTimeLines.Add(eventTimeLineData);
                                                }
                                            }
                                            else if (stemTopics[0].EndsWith($"C1U100_{Global.LANG_ID}") && topic.EndsWith($"C1U{strID}_{Global.LANG_ID}")) // Removed to ZONE
                                            {
                                                string strFI = stemTopics[0].TrimStart("ML").TrimEnd($"C1U100_{Global.LANG_ID}"); // Removed to ZONE
                                                soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFI);
                                                if (soccerMatchData != null)
                                                {
                                                    Data.Soccer.EventLocationData eventLocationData = jObjStemData.ToObject<Data.Soccer.EventLocationData>();
                                                    if (eventLocationData != null)
                                                        soccerMatchData.EventLocations.Add(eventLocationData);
                                                }
                                            }
                                        }
                                    }
                                }                                
                            }
                            else if (msgType == 'U')
                            {
                                JArray jArrDeltaData = jObjData["Data"].ToObject<JArray>();
                                #region Soccer
                                if (topic.EndsWith($"C1A_{Global.LANG_ID}_{Global.ZONE_ID}"))
                                {
                                    string strFixtureID = topic.TrimStart("OV").TrimEnd($"C1A_{Global.LANG_ID}_{Global.ZONE_ID}");
                                    soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                    if (soccerMatchData != null)
                                    {
                                        foreach (var jDeltaToken in jArrDeltaData)
                                        {
                                            soccerMatchData.Update(jDeltaToken.ToObject<JObject>());
                                        }
                                    }
                                }
                                else if (topic.EndsWith($"M1_{Global.LANG_ID}"))
                                {
                                    string strEventID = topic.TrimEnd($"M1_{Global.LANG_ID}");
                                    soccerMatchData = _soccermatches.Find(m => m.EventID == strEventID);
                                    if (soccerMatchData != null)
                                    {
                                        foreach (var jDeltaToken in jArrDeltaData)
                                        {
                                            soccerMatchData.Update(jDeltaToken.ToObject<JObject>());
                                        }
                                    }
                                }
                                else if (topic.EndsWith($"C1T1_{Global.LANG_ID}"))
                                {
                                    string strFixtureID = topic.TrimStart("ML").TrimEnd($"C1T1_{Global.LANG_ID}");
                                    soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                    if (soccerMatchData != null)
                                    {
                                        foreach (var jDeltaToken in jArrDeltaData)
                                        {
                                            soccerMatchData.HomeTeam.Update(jDeltaToken.ToObject<JObject>());
                                        }
                                    }
                                }
                                else if (topic.EndsWith($"C1T2_{Global.LANG_ID}"))
                                {
                                    string strFixtureID = topic.TrimStart("ML").TrimEnd($"C1T2_{Global.LANG_ID}");
                                    soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                    if (soccerMatchData != null)
                                    {
                                        foreach (var jDeltaToken in jArrDeltaData)
                                        {
                                            soccerMatchData.AwayTeam.Update(jDeltaToken.ToObject<JObject>());
                                        }
                                    }
                                }
                                else if (topic.EndsWith($"C1ES1-0_{Global.LANG_ID}")) // Removed ZONE_ID
                                {
                                    string strFixtureID = topic.TrimStart("ML").TrimEnd($"C1ES1-0_{Global.LANG_ID}"); // Removed ZONE_ID
                                    soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                    if (soccerMatchData != null)
                                    {
                                        foreach (var jDeltaToken in jArrDeltaData)
                                        {
                                            if (jDeltaToken["D1"] != null)
                                                soccerMatchData.HomeTeam.Score = jDeltaToken["D1"].ToString();
                                        }
                                    }
                                }
                                else if (topic.EndsWith($"C1ES1-1_{Global.LANG_ID}")) // Removed ZONE_ID
                                {
                                    string strFixtureID = topic.TrimStart("ML").TrimEnd($"C1ES1-1_{Global.LANG_ID}"); // Removed ZONE_ID
                                    soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                    if (soccerMatchData != null)
                                    {
                                        foreach (var jDeltaToken in jArrDeltaData)
                                        {
                                            if (jDeltaToken["D1"] != null)
                                                soccerMatchData.AwayTeam.Score = jDeltaToken["D1"].ToString();
                                        }
                                    }
                                }
                                else if (topic.EndsWith($"C1ES2-0_{Global.LANG_ID}")) // Removed ZONE_ID
                                {
                                    string strFixtureID = topic.TrimStart("ML").TrimEnd($"C1ES2-0_{Global.LANG_ID}"); // Removed ZONE_ID
                                    soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                    if (soccerMatchData != null)
                                    {
                                        foreach (var jDeltaToken in jArrDeltaData)
                                        {
                                            if (jDeltaToken["D1"] != null)
                                                soccerMatchData.HomeTeam.Corner = jDeltaToken["D1"].ToString();
                                        }
                                    }
                                }
                                else if (topic.EndsWith($"C1ES2-1_{Global.LANG_ID}")) // Removed ZONE_ID
                                {
                                    string strFixtureID = topic.TrimStart("ML").TrimEnd($"C1ES2-1_{Global.LANG_ID}"); // Removed ZONE_ID
                                    soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                    if (soccerMatchData != null)
                                    {
                                        foreach (var jDeltaToken in jArrDeltaData)
                                        {
                                            if (jDeltaToken["D1"] != null)
                                                soccerMatchData.AwayTeam.Corner = jDeltaToken["D1"].ToString();
                                        }
                                    }
                                }
                                else if (topic.EndsWith($"C1ES3-0_{Global.LANG_ID}")) // Removed ZONE_ID
                                {
                                    string strFixtureID = topic.TrimStart("ML").TrimEnd($"C1ES3-0_{Global.LANG_ID}"); // Removed ZONE_ID
                                    soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                    if (soccerMatchData != null)
                                    {
                                        foreach (var jDeltaToken in jArrDeltaData)
                                        {
                                            if (jDeltaToken["D1"] != null)
                                                soccerMatchData.HomeTeam.YellowCard = jDeltaToken["D1"].ToString();
                                        }
                                    }
                                }
                                else if (topic.EndsWith($"C1ES3-1_{Global.LANG_ID}")) // Removed ZONE_ID
                                {
                                    string strFixtureID = topic.TrimStart("ML").TrimEnd($"C1ES3-1_{Global.LANG_ID}"); // Removed ZONE_ID
                                    soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                    if (soccerMatchData != null)
                                    {
                                        foreach (var jDeltaToken in jArrDeltaData)
                                        {
                                            if (jDeltaToken["D1"] != null)
                                                soccerMatchData.AwayTeam.YellowCard = jDeltaToken["D1"].ToString();
                                        }
                                    }
                                }
                                else if (topic.EndsWith($"C1ES4-0_{Global.LANG_ID}")) // Removed ZONE_ID
                                {
                                    string strFixtureID = topic.TrimStart("ML").TrimEnd($"C1ES4-0_{Global.LANG_ID}"); // Removed ZONE_ID
                                    soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                    if (soccerMatchData != null)
                                    {
                                        foreach (var jDeltaToken in jArrDeltaData)
                                        {
                                            if (jDeltaToken["D1"] != null)
                                                soccerMatchData.HomeTeam.RedCard = jDeltaToken["D1"].ToString();
                                        }
                                    }
                                }
                                else if (topic.EndsWith($"C1ES4-1_{Global.LANG_ID}")) // Removed ZONE_ID
                                {
                                    string strFixtureID = topic.TrimStart("ML").TrimEnd($"C1ES4-1_{Global.LANG_ID}"); // Removed ZONE_ID
                                    soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                    if (soccerMatchData != null)
                                    {
                                        foreach (var jDeltaToken in jArrDeltaData)
                                        {
                                            if (jDeltaToken["D1"] != null)
                                                soccerMatchData.AwayTeam.RedCard = jDeltaToken["D1"].ToString();
                                        }
                                    }                                    
                                }                                
                                else if (topic.EndsWith($"C1ES8-0_{Global.LANG_ID}")) // Removed ZONE_ID
                                {
                                    string strFixtureID = topic.TrimStart("ML").TrimEnd($"C1ES8-0_{Global.LANG_ID}"); // Removed ZONE_ID
                                    soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                    if (soccerMatchData != null)
                                    {
                                        foreach (var jDeltaToken in jArrDeltaData)
                                        {
                                            if (jDeltaToken["D1"] != null)
                                                soccerMatchData.HomeTeam.Penalty = jDeltaToken["D1"].ToString();
                                        }
                                    }
                                }
                                else if (topic.EndsWith($"C1ES8-1_{Global.LANG_ID}")) // Removed ZONE_ID
                                {
                                    string strFixtureID = topic.TrimStart("ML").TrimEnd($"C1ES8-1_{Global.LANG_ID}"); // Removed ZONE_ID
                                    soccerMatchData = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                    if (soccerMatchData != null)
                                    {
                                        foreach (var jDeltaToken in jArrDeltaData)
                                        {
                                            if (jDeltaToken["D1"] != null)
                                                soccerMatchData.AwayTeam.Penalty = jDeltaToken["D1"].ToString();
                                        }
                                    }
                                }
                                else if (topic.StartsWith("6VP") && topic.EndsWith($"_{Global.LANG_ID}_{Global.ZONE}")) // Changed to ZONE
                                {
                                    string[] stemTopics = topic.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                                    string strC3 = stemTopics[0].TrimStart("6VP");
                                    SoccerMatchData soccerMatchDataForPA = _soccermatches.Find(m => m.C3 == strC3);
                                    if (soccerMatchDataForPA != null)
                                    {
                                        foreach (var jDeltaToken in jArrDeltaData)
                                        {
                                            if (soccerMatchDataForPA.FullTime.MarketData.HomeIT == topic)
                                            {
                                                if (jDeltaToken["OD"] != null)
                                                    soccerMatchDataForPA.FullTime.MarketData.HomeOD = jDeltaToken["OD"].ToString();
                                                if (jDeltaToken["SU"] != null)
                                                    soccerMatchDataForPA.FullTime.MarketData.HomeSU = jDeltaToken["SU"].ToString();
                                            }
                                            else if (soccerMatchDataForPA.FullTime.MarketData.AwayIT == topic)
                                            {
                                                if (jDeltaToken["OD"] != null)
                                                    soccerMatchDataForPA.FullTime.MarketData.AwayOD = jDeltaToken["OD"].ToString();
                                                if (jDeltaToken["SU"] != null)
                                                    soccerMatchDataForPA.FullTime.MarketData.AwaySU = jDeltaToken["SU"].ToString();
                                            }
                                            else if (soccerMatchDataForPA.FullTime.MarketData.DrawIT == topic)
                                            {
                                                if (jDeltaToken["OD"] != null)
                                                    soccerMatchDataForPA.FullTime.MarketData.DrawOD = jDeltaToken["OD"].ToString();
                                                if (jDeltaToken["SU"] != null)
                                                    soccerMatchDataForPA.FullTime.MarketData.DrawSU = jDeltaToken["SU"].ToString();
                                            }
                                            else if (soccerMatchDataForPA.AsianHandicap.MarketData.Any(ma => ma.HomeIT == topic || ma.AwayIT == topic))
                                            {
                                                AsianHandicapMarket asianHandicapMarket = soccerMatchDataForPA.AsianHandicap.MarketData.Find(ma => ma.HomeIT == topic);
                                                if (asianHandicapMarket != null)
                                                {
                                                    if (jDeltaToken["HA"] != null)
                                                        asianHandicapMarket.HomeHdp = jDeltaToken["HA"].ToString();
                                                    if (jDeltaToken["OD"] != null)
                                                        asianHandicapMarket.HomeOD = jDeltaToken["OD"].ToString();
                                                    if (jDeltaToken["SU"] != null)
                                                        asianHandicapMarket.HomeSU = jDeltaToken["SU"].ToString();
                                                }
                                                else
                                                {
                                                    asianHandicapMarket = soccerMatchDataForPA.AsianHandicap.MarketData.Find(ma => ma.AwayIT == topic);
                                                    if (asianHandicapMarket != null)
                                                    {
                                                        if (jDeltaToken["HA"] != null)
                                                            asianHandicapMarket.AwayHdp = jDeltaToken["HA"].ToString();
                                                        if (jDeltaToken["OD"] != null)
                                                            asianHandicapMarket.AwayOD = jDeltaToken["OD"].ToString();
                                                        if (jDeltaToken["SU"] != null)
                                                            asianHandicapMarket.AwaySU = jDeltaToken["SU"].ToString();
                                                    }
                                                }
                                            }
                                            else if(soccerMatchDataForPA.matchGoal.MarketData.Any(ma => ma.OverIT == topic || ma.UnderIT == topic))
                                            {
                                                MatchGoalMarket goalLineMarket = soccerMatchDataForPA.matchGoal.MarketData.Find(ma => ma.OverIT == topic);
                                                if (goalLineMarket != null)
                                                {
                                                    if (jDeltaToken["HA"] != null)
                                                        goalLineMarket.OverHdp = jDeltaToken["HA"].ToString();
                                                    if (jDeltaToken["OD"] != null)
                                                        goalLineMarket.OverOD = jDeltaToken["OD"].ToString();
                                                    if (jDeltaToken["SU"] != null)
                                                        goalLineMarket.OverSU = jDeltaToken["SU"].ToString();
                                                }
                                                else
                                                {
                                                    goalLineMarket = soccerMatchDataForPA.matchGoal.MarketData.Find(ma => ma.UnderIT == topic);
                                                    if (goalLineMarket != null)
                                                    {
                                                        if (jDeltaToken["HA"] != null)
                                                            goalLineMarket.UnderHdp = jDeltaToken["HA"].ToString();
                                                        if (jDeltaToken["OD"] != null)
                                                            goalLineMarket.UnderOD = jDeltaToken["OD"].ToString();
                                                        if (jDeltaToken["SU"] != null)
                                                            goalLineMarket.UnderSU = jDeltaToken["SU"].ToString();
                                                    }
                                                }
                                            }
                                            else if (soccerMatchDataForPA.GoalLine.MarketData.Any(ma => ma.OverIT == topic || ma.UnderIT == topic))
                                            {
                                                GoalLineMarket goalLineMarket = soccerMatchDataForPA.GoalLine.MarketData.Find(ma => ma.OverIT == topic);
                                                if (goalLineMarket != null)
                                                {
                                                    if (jDeltaToken["HA"] != null)
                                                        goalLineMarket.OverHdp = jDeltaToken["HA"].ToString();
                                                    if (jDeltaToken["OD"] != null)
                                                        goalLineMarket.OverOD = jDeltaToken["OD"].ToString();
                                                    if (jDeltaToken["SU"] != null)
                                                        goalLineMarket.OverSU = jDeltaToken["SU"].ToString();
                                                }
                                                else
                                                {
                                                    goalLineMarket = soccerMatchDataForPA.GoalLine.MarketData.Find(ma => ma.UnderIT == topic);
                                                    if (goalLineMarket != null)
                                                    {
                                                        if (jDeltaToken["HA"] != null)
                                                            goalLineMarket.UnderHdp = jDeltaToken["HA"].ToString();
                                                        if (jDeltaToken["OD"] != null)
                                                            goalLineMarket.UnderOD = jDeltaToken["OD"].ToString();
                                                        if (jDeltaToken["SU"] != null)
                                                            goalLineMarket.UnderSU = jDeltaToken["SU"].ToString();
                                                    }
                                                }                                                
                                            }
                                        }
                                        soccerMatchData = soccerMatchDataForPA;
                                    }
                                }
                                #endregion                                
                            }
                            else if (msgType == 'D')
                            {
                                string[] stemTopics = topic.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                                if (stemTopics.Length > 1)
                                {
                                    if ($"OVInPlay_{Global.LANG_ID}_{Global.ZONE_ID}".Equals(stemTopics[0]))
                                    {
                                        if ($"OV_1_{Global.LANG_ID}_{Global.ZONE_ID}".Equals(stemTopics[1]) &&
                                            stemTopics.Length == 4 && stemTopics[3].EndsWith($"C1A_{Global.LANG_ID}_{Global.ZONE_ID}"))
                                        {
                                            string strFixtureID = stemTopics[3].TrimStart("OV").TrimEnd($"C1A_{Global.LANG_ID}_{Global.ZONE_ID}");
                                            SoccerMatchData soccerMatchDataToDel = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                            if (soccerMatchDataToDel != null)
                                            {
                                                Global.WriteLog(LOGLEVEL.FILE, LOGTYPE.INDATA, $"Removed Match Data: EventID={soccerMatchDataToDel.EventID}, FixtureID={soccerMatchDataToDel.FixtureID}, Name='{soccerMatchDataToDel.HomeName} vs {soccerMatchDataToDel.AwayName}'");
                                                _soccermatches.Remove(soccerMatchDataToDel);
                                                BroadCastMessage($"D{soccerMatchDataToDel.EventID}");
                                            }
                                        }
                                    }
                                    else if (stemTopics.Length == 2 && stemTopics[1].StartsWith("6V") && stemTopics[1].EndsWith($"_{Global.LANG_ID}_{Global.ZONE}"))
                                    {
                                        string strFixtureID = stemTopics[1].TrimStart("6V").TrimEnd($"C1G1777_{Global.LANG_ID}_{Global.ZONE}");                                        
                                        SoccerMatchData soccerMatchDataForPA = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                        if (soccerMatchDataForPA != null)
                                        {
                                            soccerMatchDataForPA.FullTime.MarketData = new FullTimeMarket();
                                        }
                                        else
                                        {
                                            strFixtureID = stemTopics[1].TrimStart("6V").TrimEnd($"C1-G429_{Global.LANG_ID}_{Global.ZONE}");
                                            soccerMatchDataForPA = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                            if (soccerMatchDataForPA != null)
                                            {
                                                soccerMatchDataForPA.matchGoal.MarketData.Clear();
                                            }
                                            else
                                            {
                                                strFixtureID = stemTopics[1].TrimStart("6V").TrimEnd($"C1-G15_{Global.LANG_ID}_{Global.ZONE}");
                                                soccerMatchDataForPA = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                                if (soccerMatchDataForPA != null)
                                                {
                                                    soccerMatchDataForPA.GoalLine.MarketData.Clear();
                                                }
                                            }
                                        }
                                        soccerMatchData = soccerMatchDataForPA;
                                    }
                                    else if (stemTopics.Length == 4 && stemTopics[3].StartsWith("6VP") && stemTopics[3].EndsWith($"_{Global.LANG_ID}_{Global.ZONE}"))
                                    {
                                        string[] tempTopics = stemTopics[3].Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                                        string strC3 = tempTopics[0].TrimStart("6VP");
                                        if (stemTopics[0].EndsWith($"C1A_{Global.LANG_ID}_{Global.ZONE}"))
                                        {
                                            SoccerMatchData soccerMatchDataForPA = _soccermatches.Find(m => m.C3 == strC3
                                            && (m.FullTime.MarketData.HomeIT == stemTopics[3] || m.FullTime.MarketData.DrawIT == stemTopics[3] || m.FullTime.MarketData.AwayIT == stemTopics[3]));
                                            if (soccerMatchDataForPA != null)
                                            {
                                                soccerMatchDataForPA.FullTime.MarketData = new FullTimeMarket();
                                            }
                                            else
                                            {
                                                soccerMatchDataForPA = _soccermatches.Find(m => m.C3 == strC3
                                                       && (m.matchGoal.MarketData.Any(ma => ma.OverIT == stemTopics[3] || ma.UnderIT == stemTopics[3])));
                                                if (soccerMatchDataForPA != null)
                                                {
                                                    MatchGoalMarket goalLineMarket = soccerMatchDataForPA.matchGoal.MarketData.Find(ma => ma.OverIT == stemTopics[3] || ma.UnderIT == stemTopics[3]);
                                                    soccerMatchDataForPA.matchGoal.MarketData.Remove(goalLineMarket);
                                                }

                                                soccerMatchDataForPA = _soccermatches.Find(m => m.C3 == strC3
                                                    && (m.AsianHandicap.MarketData.Any(ma => ma.HomeIT == stemTopics[3] || ma.AwayIT == stemTopics[3])));
                                                if (soccerMatchDataForPA != null)
                                                {
                                                    AsianHandicapMarket asianHandicapMarket = soccerMatchDataForPA.AsianHandicap.MarketData.Find(ma => ma.HomeIT == stemTopics[3] || ma.AwayIT == stemTopics[3]);
                                                    soccerMatchDataForPA.AsianHandicap.MarketData.Remove(asianHandicapMarket);
                                                }
                                                else
                                                {
                                                    soccerMatchDataForPA = _soccermatches.Find(m => m.C3 == strC3
                                                        && (m.GoalLine.MarketData.Any(ma => ma.OverIT == stemTopics[3] || ma.UnderIT == stemTopics[3])));
                                                    if (soccerMatchDataForPA != null)
                                                    {
                                                        GoalLineMarket goalLineMarket = soccerMatchDataForPA.GoalLine.MarketData.Find(ma => ma.OverIT == stemTopics[3] || ma.UnderIT == stemTopics[3]);
                                                        soccerMatchDataForPA.GoalLine.MarketData.Remove(goalLineMarket);
                                                    }

                                                    soccerMatchDataForPA = _soccermatches.Find(m => m.C3 == strC3
                                                       && (m.matchGoal.MarketData.Any(ma => ma.OverIT == stemTopics[3] || ma.UnderIT == stemTopics[3])));
                                                    if (soccerMatchDataForPA != null)
                                                    {
                                                        MatchGoalMarket goalLineMarket = soccerMatchDataForPA.matchGoal.MarketData.Find(ma => ma.OverIT == stemTopics[3] || ma.UnderIT == stemTopics[3]);
                                                        soccerMatchDataForPA.matchGoal.MarketData.Remove(goalLineMarket);
                                                    }
                                                }
                                            }
                                            soccerMatchData = soccerMatchDataForPA;
                                        }
                                    }
                                }
                                else
                                {
                                    if (topic.EndsWith($"C1A_{Global.LANG_ID}_{Global.ZONE}"))
                                    {
                                        string strFixtureID = topic.TrimStart("6V").TrimEnd($"C1A_{Global.LANG_ID}_{Global.ZONE}");
                                        SoccerMatchData soccerMatchDataToDel = _soccermatches.Find(m => m.FixtureID == strFixtureID);
                                        if (soccerMatchDataToDel != null)
                                        {
                                            Global.WriteLog(LOGLEVEL.FILE, LOGTYPE.INDATA, $"Removed Match Data (By FixtureID): EventID={soccerMatchDataToDel.EventID}, FixtureID={soccerMatchDataToDel.FixtureID}, Name='{soccerMatchDataToDel.HomeName} vs {soccerMatchDataToDel.AwayName}'");
                                            _soccermatches.Remove(soccerMatchDataToDel);
                                            BroadCastMessage($"D{soccerMatchDataToDel.EventID}");
                                        }
                                    }
                                    else if (topic.EndsWith($"M1_{Global.LANG_ID}"))
                                    {
                                        string strEventID = topic.TrimEnd($"M1_{Global.LANG_ID}");
                                        SoccerMatchData soccerMatchDataToDel = _soccermatches.Find(m => m.EventID == strEventID);
                                        if (soccerMatchDataToDel != null)
                                        {
                                            Global.WriteLog(LOGLEVEL.FILE, LOGTYPE.INDATA, $"Removed Match Data (By EventID): EventID={soccerMatchDataToDel.EventID}, FixtureID={soccerMatchDataToDel.FixtureID}, Name='{soccerMatchDataToDel.HomeName} vs {soccerMatchDataToDel.AwayName}'");
                                            _soccermatches.Remove(soccerMatchDataToDel);
                                            BroadCastMessage($"D{soccerMatchDataToDel.EventID}");
                                        }
                                    }
                                }
                            }

                            if (soccerMatchData != null)
                            {
                                SoccerMatchData updatedSoccerMatch = updatedSoccerMatches.Find(m => m.FixtureID == soccerMatchData.FixtureID);
                                if (updatedSoccerMatch == null)
                                    updatedSoccerMatches.Add(soccerMatchData);                                
                            }
                        }
                    }
                    if (updatedSoccerMatches.Count > 0)
                    {
                        _soccermatches.ForEach(n => { n.HomeScore = n.Score.Split('-')[0].Trim();n.AwayScore = n.Score.Split('-')[1].Trim(); n.Time = n.CalcMatchTime(n.DC, n.TU, n.TM, n.TS, n.TT, n.TD); }) ;
                        List<SoccerMatchData> newUpdatedMatches = new List<SoccerMatchData>();
                        foreach (SoccerMatchData matchData in _soccermatches)
                        {
                            if (matchData.LeagueName.Contains("Esoccer"))
                                continue;

                            //List<EventTimeLineData> updateTimeLineData = timeLineData.OrderBy(n => n.TM).ToList();
                            
                            double matchMin = Utils.ParseToDouble(matchData.Time.Split(':')[0].Trim());
                            //if (Config.Instance.targetTime < matchMin)
                            //    continue;
                                                       

                            int hOff = 0, hOn = 0, aOff = 0, aOn = 0;
                            int.TryParse(matchData.HomeTeam.OffTarget, out hOff);
                            int.TryParse(matchData.HomeTeam.OnTarget, out hOn);
                            int.TryParse(matchData.AwayTeam.OffTarget, out aOff);
                            int.TryParse(matchData.AwayTeam.OnTarget, out aOn);
                            matchData.Statistics =  $"射正      {matchData.HomeTeam.OnTarget}:{matchData.AwayTeam.OnTarget}" + Environment.NewLine;
                            matchData.Statistics += $"射偏      {matchData.HomeTeam.OffTarget}:{matchData.AwayTeam.OffTarget}" + Environment.NewLine;
                            matchData.Statistics += $"射门      {hOn + hOff}:{aOn + aOff}" + Environment.NewLine;
                            matchData.Statistics += $"危险进攻  {matchData.HomeTeam.DangerAttack}:{matchData.AwayTeam.DangerAttack}" + Environment.NewLine;
                            matchData.Statistics += $"进攻      {matchData.HomeTeam.Attack}:{matchData.AwayTeam.Attack}" + Environment.NewLine;

                            
                            matchData.AsianHandicapOdds = "";
                            matchData.GoalLineOdds = "";
                            if (matchData.AsianHandicap.MarketData.Count > 0)
                            {
                                for (int i = 0; i < matchData.AsianHandicap.MarketData.Count; i++)
                                {
                                    matchData.AsianHandicapOdds += "(" + matchData.AsianHandicap.MarketData[i].HomeHdp + ") " + matchData.AsianHandicap.MarketData[i].HomeOdds + "    ";
                                    matchData.AsianHandicapOdds += matchData.AsianHandicap.MarketData[i].AwayOdds + " (" + matchData.AsianHandicap.MarketData[i].AwayHdp + ") " + Environment.NewLine;
                                }
                            }

                            if (matchData.GoalLine.MarketData.Count > 0)
                            {
                                for (int i = 0; i < matchData.GoalLine.MarketData.Count; i++)
                                {
                                    matchData.GoalLineOdds += matchData.GoalLine.MarketData[i].OverOdds + "    ";
                                    matchData.GoalLineOdds += "(" + matchData.GoalLine.MarketData[i].UnderHdp + ")    " + matchData.GoalLine.MarketData[i].UnderOdds + Environment.NewLine;
                                }
                            }
                            newUpdatedMatches.Add(matchData);

                            try
                            {
                                if (isMatchCondition(matchData, null))
                                {
                                    //string strMsg = $"{title} \n\n{strLeague}\n{strEventName}\n{strGoal}\n{strCorner}\n{strYellowCards}\n{strNull}\n{overConrer}\n{underConrer}\n{overYellow}\n{underYellow}\n{overGoals}\n{underGoals}";
                                    //if (!Config.Instance.message_history.Contains(strMsg))
                                    //{
                                    //    Config.Instance.message_history.Add(strMsg);
                                    //    sendTelegramMsg(strMsg);
                                    //    Global.WriteLog(LOGLEVEL.FILE, LOGTYPE.INDATA, strMsg);
                                    //}

                                }
                            }
                            catch (Exception ex)
                            {
                                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "PickFindingException: " + ex.ToString());
                            }
                        }
//#if (!TEST)
                        Global.matchEvent(newUpdatedMatches);
//#endif
                    }
                }
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, ex.ToString());
            }
        }

        private void WriteLog(string log)
        {

        }
        
        private void RequestMatchLiveData(string ID, string eventID, string type)
        {
      
#if (TEST)
            return;
#endif
            int nIndex = ID.IndexOf($"C{type}A_{Global.LANG_ID}");
            string fixtureID = ID;
            if (nIndex >= 0)
                fixtureID = ID.Substring(0, nIndex);

            string strReqMsg = $"6V{fixtureID}C{type}A_{Global.LANG_ID}_{Global.ZONE}";
            Bet365ClientManager.Instance.SendRequestToSocket(strReqMsg);

            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, $"Sent MatchLiveData Request 1 >>> {strReqMsg}");

            string strReqMsg1 = $"{eventID}M{type}_{Global.LANG_ID}";
            Bet365ClientManager.Instance.SendRequestToSocket(strReqMsg1);

            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, $"Sent MatchLiveData Request 2 >>> {strReqMsg1}");
        }

        private void RequestMatchLeagueTableData(string tableId)
        {
            string[] tableIds = tableId.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string strReqMsg = string.Empty;
            for (var i = 0; i < tableIds.Length; i++)
            {
                if (!string.IsNullOrEmpty(tableIds[i]))
                    strReqMsg = $"{strReqMsg}LT{tableIds[i]}_{Global.LANG_ID}_{Global.ZONE_ID},";
            }
            if (string.IsNullOrEmpty(strReqMsg))
                return;
            strReqMsg = $"{Global.CLIENT_SUBSCRIBE}{Global.NONE_ENCODING}{strReqMsg.TrimEnd(",")}{Global.DELIM_RECORD}";
            if (Bet365ClientManager.Instance.OnBet365DataSend != null)
                Bet365ClientManager.Instance.OnBet365DataSend(strReqMsg);

            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, $"Sent MatchLeagueTableData Request >>> {strReqMsg}");
        }

      
        public static bool isMatchCondition(SoccerMatchData matchData, onWriteStatusEvent writeLog)
        {
            writeLog?.Invoke($"[MatchCondition] {matchData.Time} {matchData.LeagueName}  {matchData.HomeName}({matchData.HomeScore}) - {matchData.AwayName}({matchData.AwayScore})");
            writeLog?.Invoke($"[MatchCondition] {matchData.Statistics})");
            writeLog?.Invoke($"[MatchCondition] {matchData.AsianHandicapOdds})");
            writeLog?.Invoke($"[MatchCondition] {matchData.GoalLineOdds})");

            //matchData
            //		EventID	"15860999092"	string
            //MarketData
            //AwayHdp "+0.25" string
            //AwayIT  "6VP132719807-126718090_1_1"    string
            //AwayOD  "37/20" string
            //AwayOdds    "2.850" string

            //page link
            //EV15860999092C1
            //addbet betslip
            //"pt=N#o=37/20#f=132719807#fp=126718090#so=#c=1#ln=+0.25#mt=1#id=132719807-126718090Y#oto=2#|TP=BS132719807-126718090x2x3#||"
            bool isMatched = true;
            foreach (COMMAND command in Config.Instance.Commands)
            {
                try
                {
                    isMatched = true;
                    writeLog?.Invoke($"---------------------------------------");
                    writeLog?.Invoke($"[checking_command] {command}");
                    //check if already published
                    bool bAlreadyPublished = false;
                    foreach (var origPick in Global.resultHistory)
                    {
                        if (origPick.matchData.EventID == matchData.EventID && origPick.matchData.FixtureID == matchData.FixtureID && origPick.command.BetMarket == command.BetMarket)
                        {
                            bAlreadyPublished = true;
                            break;
                        }
                    }
                    if (bAlreadyPublished)
                    {
                        writeLog?.Invoke($"Already published");
                        continue;
                    }



                    foreach (CONDITION condition in command.Conditions)
                    {
                        writeLog?.Invoke($"[checking_condition] {condition}");
                        switch (condition.Checker)
                        {
                            case CONDITION_CHECKER.MATCH_RECENT_TIME:
                                {
                                    try
                                    {
                                        double matchMin = Utils.ParseToDouble(matchData.Time.Split(':')[0].Trim());
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(matchMin, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {matchMin} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.MATCH_TIME: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.MATCH_TIME:
                                {
                                    try
                                    {
                                        double matchMin = Utils.ParseToDouble(matchData.Time.Split(':')[0].Trim());
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(matchMin, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {matchMin} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.MATCH_TIME: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.SCORE_DETAIL:
                                {
                                    try 
                                    { 
                                        if (condition.Comparer == COMPARISON.EQUAL)
                                        {
                                            string matchScore = matchData.Score;
                                            if (matchScore != condition.Value)
                                            {
                                                isMatched = false;
                                                writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {matchScore} {condition.Value}");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.SCORE_DETAIL: " + ex.ToString());
                                    }
                                }
                                break;

                            case CONDITION_CHECKER.SCORE_SUM:
                                {
                                    try
                                    {
                                        string[] scores = matchData.Score.Split('-');
                                        if (scores.Length == 2)
                                        {
                                            double ScoreSum = Utils.ParseToDouble(scores[0]) + Utils.ParseToDouble(scores[1]);
                                            double value = Utils.ParseToDouble(condition.Value);
                                            if (!Utils.CheckCondition(ScoreSum, condition.Comparer, value))
                                            {
                                                isMatched = false;
                                                writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {ScoreSum} {condition.Comparer.GetDescription()} {value}");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.SCORE_SUM: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.HOME_GOALS:
                                {
                                    try
                                    {
                                        double homeScore = Utils.ParseToDouble(matchData.HomeScore);
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(homeScore, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {homeScore} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.HOME_GOALS: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.HOME_ONTARGET:
                                {
                                    try
                                    {
                                        double homeOnTarget = Utils.ParseToDouble(matchData.HomeTeam.OnTarget);
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(homeOnTarget, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {homeOnTarget} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.HOME_ONTARGET: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.HOME_SHOTS:
                                {
                                    try
                                    {
                                        double homeOffTarget = Utils.ParseToDouble(matchData.HomeTeam.OffTarget);
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(homeOffTarget, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {homeOffTarget} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.HOME_SHOTS: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.HOME_SHOTS_PLUS_ONTARGET:
                                {
                                    try
                                    {
                                        double homeShotsPlusOnTarget = Utils.ParseToDouble(matchData.HomeTeam.OffTarget) + Utils.ParseToDouble(matchData.HomeTeam.OnTarget);
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(homeShotsPlusOnTarget, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {homeShotsPlusOnTarget} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.HOME_SHOTS_PLUS_ONTARGET: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.HOME_DANGEROUSATTACKS:
                                {
                                    try
                                    {
                                        double homeDangerousAttack = Utils.ParseToDouble(matchData.HomeTeam.DangerAttack);
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(homeDangerousAttack, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {homeDangerousAttack} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.HOME_DANGEROUSATTACKS: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.HOME_ATTACKS:
                                {
                                    try
                                    {
                                        double homeAttack = Utils.ParseToDouble(matchData.HomeTeam.Attack);
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(homeAttack, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {homeAttack} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.HOME_ATTACKS: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.HOME_GOALS_RATIO:
                                {
                                    try
                                    {
                                        double homeScore = Utils.ParseToDouble(matchData.HomeScore) / (Utils.ParseToDouble(matchData.HomeScore) + Utils.ParseToDouble(matchData.AwayScore));
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(homeScore, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {homeScore} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.HOME_GOALS_RATIO: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.HOME_ONTARGET_RATIO:
                                {
                                    try
                                    {
                                        double homeOnTarget = Utils.ParseToDouble(matchData.HomeTeam.OnTarget) / (Utils.ParseToDouble(matchData.HomeTeam.OnTarget) + Utils.ParseToDouble(matchData.AwayTeam.OnTarget));
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(homeOnTarget, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {homeOnTarget} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.HOME_ONTARGET_RATIO: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.HOME_SHOTS_RATIO:
                                {
                                    try
                                    {
                                        double homeOffTarget = Utils.ParseToDouble(matchData.HomeTeam.OffTarget) / (Utils.ParseToDouble(matchData.HomeTeam.OffTarget) + Utils.ParseToDouble(matchData.AwayTeam.OffTarget));
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(homeOffTarget, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {homeOffTarget} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.HOME_ONTARGET_RATIO: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.HOME_SHOTS_PLUS_ONTARGET_RATIO:
                                {
                                    try
                                    {
                                        double homeShotsPlusOnTarget = (Utils.ParseToDouble(matchData.HomeTeam.OffTarget) + Utils.ParseToDouble(matchData.HomeTeam.OnTarget)) / (Utils.ParseToDouble(matchData.HomeTeam.OffTarget) + Utils.ParseToDouble(matchData.HomeTeam.OnTarget) + Utils.ParseToDouble(matchData.AwayTeam.OffTarget) + Utils.ParseToDouble(matchData.AwayTeam.OnTarget));
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(homeShotsPlusOnTarget, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {homeShotsPlusOnTarget} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.HOME_SHOTS_PLUS_ONTARGET_RATIO: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.HOME_DANGEROUSATTACKS_RATIO:
                                {
                                    try
                                    {
                                        double homeDangerousAttack = Utils.ParseToDouble(matchData.HomeTeam.DangerAttack) / (Utils.ParseToDouble(matchData.HomeTeam.DangerAttack) + Utils.ParseToDouble(matchData.AwayTeam.DangerAttack));
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(homeDangerousAttack, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {homeDangerousAttack} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.HOME_DANGEROUSATTACKS_RATIO: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.HOME_ATTACKS_RATIO:
                                {
                                    try
                                    {
                                        double homeAttack = Utils.ParseToDouble(matchData.HomeTeam.Attack) / (Utils.ParseToDouble(matchData.HomeTeam.Attack) + Utils.ParseToDouble(matchData.AwayTeam.Attack));
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(homeAttack, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {homeAttack} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.HOME_ATTACKS_RATIO: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.AWAY_GOALS:
                                {
                                    try
                                    {
                                        double awayScore = Utils.ParseToDouble(matchData.AwayScore);
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(awayScore, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {awayScore} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.AWAY_GOALS: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.AWAY_ONTARGET:
                                {
                                    try
                                    {
                                        double awayOnTarget = Utils.ParseToDouble(matchData.AwayTeam.OnTarget);
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(awayOnTarget, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {awayOnTarget} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.AWAY_ONTARGET: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.AWAY_SHOTS:
                                {
                                    try
                                    {
                                        double awayOffTarget = Utils.ParseToDouble(matchData.AwayTeam.OffTarget);
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(awayOffTarget, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {awayOffTarget} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.AWAY_SHOTS: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.AWAY_SHOTS_PLUS_ONTARGET:
                                {
                                    try
                                    {
                                        double awayShotsPlusOnTarget = Utils.ParseToDouble(matchData.AwayTeam.OffTarget) + Utils.ParseToDouble(matchData.AwayTeam.OnTarget);
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(awayShotsPlusOnTarget, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {awayShotsPlusOnTarget} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.AWAY_SHOTS_PLUS_ONTARGET: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.AWAY_DANGEROUSATTACKS:
                                {
                                    try
                                    {
                                        double awayDangerousAttack = Utils.ParseToDouble(matchData.AwayTeam.DangerAttack);
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(awayDangerousAttack, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {awayDangerousAttack} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.AWAY_DANGEROUSATTACKS: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.AWAY_ATTACKS:
                                {
                                    try
                                    {
                                        double awayAttack = Utils.ParseToDouble(matchData.AwayTeam.Attack);
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(awayAttack, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {awayAttack} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.AWAY_ATTACKS: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.AWAY_GOALS_RATIO:
                                {
                                    try
                                    {
                                        double awayScore = Utils.ParseToDouble(matchData.AwayScore) / (Utils.ParseToDouble(matchData.HomeScore) + Utils.ParseToDouble(matchData.AwayScore));
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(awayScore, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {awayScore} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.AWAY_GOALS_RATIO: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.AWAY_ONTARGET_RATIO:
                                {
                                    try
                                    {
                                        double awayOnTarget = Utils.ParseToDouble(matchData.AwayTeam.OnTarget) / (Utils.ParseToDouble(matchData.HomeTeam.OnTarget) + Utils.ParseToDouble(matchData.AwayTeam.OnTarget));
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(awayOnTarget, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {awayOnTarget} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.AWAY_ONTARGET_RATIO: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.AWAY_SHOTS_RATIO:
                                {
                                    try
                                    {
                                        double awayOffTarget = Utils.ParseToDouble(matchData.AwayTeam.OffTarget) / (Utils.ParseToDouble(matchData.HomeTeam.OffTarget) + Utils.ParseToDouble(matchData.AwayTeam.OffTarget));
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(awayOffTarget, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {awayOffTarget} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.AWAY_SHOTS_RATIO: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.AWAY_SHOTS_PLUS_ONTARGET_RATIO:
                                {
                                    try
                                    {
                                        double awayShotsPlusOnTarget = (Utils.ParseToDouble(matchData.AwayTeam.OffTarget) + Utils.ParseToDouble(matchData.AwayTeam.OnTarget)) / (Utils.ParseToDouble(matchData.HomeTeam.OffTarget) + Utils.ParseToDouble(matchData.HomeTeam.OnTarget) + Utils.ParseToDouble(matchData.AwayTeam.OffTarget) + Utils.ParseToDouble(matchData.AwayTeam.OnTarget));
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(awayShotsPlusOnTarget, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {awayShotsPlusOnTarget} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.AWAY_SHOTS_PLUS_ONTARGET_RATIO: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.AWAY_DANGEROUSATTACKS_RATIO:
                                {
                                    try
                                    {
                                        double awayDangerousAttack = Utils.ParseToDouble(matchData.AwayTeam.DangerAttack) / (Utils.ParseToDouble(matchData.HomeTeam.DangerAttack) + Utils.ParseToDouble(matchData.AwayTeam.DangerAttack));
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(awayDangerousAttack, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {awayDangerousAttack} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.AWAY_DANGEROUSATTACKS_RATIO: " + ex.ToString());
                                    }
                                }
                                break;
                            case CONDITION_CHECKER.AWAY_ATTACKS_RATIO:
                                {
                                    try
                                    {
                                        double awayAttack = Utils.ParseToDouble(matchData.AwayTeam.Attack) / (Utils.ParseToDouble(matchData.HomeTeam.Attack) + Utils.ParseToDouble(matchData.AwayTeam.Attack));
                                        double value = Utils.ParseToDouble(condition.Value);
                                        if (!Utils.CheckCondition(awayAttack, condition.Comparer, value))
                                        {
                                            isMatched = false;
                                            writeLog?.Invoke($"[checking_condition] wrong {condition.Checker.GetDescription()} : {awayAttack} {condition.Comparer.GetDescription()} {value}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "[Exception]CONDITION_CHECKER.AWAY_ATTACKS_RATIO: " + ex.ToString());
                                    }
                                }
                                break;
                        }

                        if (!isMatched)
                        {
                            writeLog?.Invoke($"[checking_condition] condition mismatched");
                            break;
                        }
                    }

                    if (isMatched)
                    {
                        writeLog?.Invoke($"[checking_bet option]");

                       

                        try
                        {
                            double handicap = Utils.ParseToDouble(command.Handicap);
                            if (command.BetPlusMinus == BET_PLUSMINUS.PLUS)
                                handicap = Math.Abs(handicap);
                            else
                                handicap = 0 - Math.Abs(handicap);
                            double OddValue = Utils.ParseToDouble(command.OddValue);

                            switch (command.BetMarket)
                            {
                                case BET_MARKET.ASIAN_HANDICAP:
                                    {
                                        foreach (AsianHandicapMarket market in matchData.AsianHandicap.MarketData)
                                        {
                                            double marketHomeHandicap = Utils.ParseToDouble(market.HomeHdp);
                                            double marketAwayHandicap = Utils.ParseToDouble(market.AwayHdp);


                                            writeLog?.Invoke($"[checking_AH_bet option] Home HD:{market.HomeHdp} ODD:{market.HomeOdds} Away HD:{market.AwayHdp} ODD:{market.AwayOdds}  condition HD: {handicap}");

                                            if (command.BetTeam == BET_TEAM.HOME_TEAM && handicap == marketHomeHandicap)
                                            {
                                                double HomeOdd = Utils.ParseToDouble(market.HomeOdds);
                                                if (Utils.CheckCondition(HomeOdd, command.OddComparer, OddValue))
                                                {
                                                    writeLog?.Invoke($"=========[checking_AH_bet option] option is matched**************************");
                                                    string directlink = "";
                                                    MatchCollection mc = Regex.Matches(market.HomeIT, "^\\d+[a-zA-Z]+(?<f>\\d+)-(?<fp>\\d+)_");
                                                    if (mc.Count == 1)
                                                    {
                                                        Match m = mc[0];
                                                        string f = m.Groups["f"].Value;
                                                        string fp = m.Groups["fp"].Value;

                                                        directlink = $"{fp}|{market.HomeOD}|{f}";
                                                    }

                                                    if (!string.IsNullOrEmpty(directlink))
                                                    {
                                                        if (writeLog == null)
                                                        {
                                                            AddPickHistory(matchData, command, $"亚洲让分盘 主队盘口 {handicap} 当前赔率 {HomeOdd} {command.OddComparer.GetDescription()} 设置赔率 {OddValue} Handicap {market.HomeHdp} EventID {matchData.EventID} DirectLink {directlink}");
                                                            //SendWebAPI("Joe-SoccerLiver", ToHexString("soccer|" + matchData.LeagueName + "|" + matchData.HomeName + "|" + matchData.AwayName), ToHexString($"AH1({market.HomeHdp})"), ToHexString(matchData.EventID), ToHexString(directlink));
                                                            Global.LiveResultNotifier("Joe-SoccerLiver", "soccer|" + matchData.LeagueName + "|" + matchData.HomeName + "|" + matchData.AwayName + "|" + matchData.Score, $"AH1({market.HomeHdp})", matchData.EventID, directlink);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Global.WriteLog(LOGLEVEL.FILE, LOGTYPE.INDATA, $"Error GetDirectlink1 {market.HomeIT}");
                                                    }
                                                    break;
                                                }
                                                else
                                                {
                                                    writeLog?.Invoke($"[checking_AH_bet option] option is mismatched");
                                                }
                                            }

                                            if (command.BetTeam == BET_TEAM.AWAY_TEAM && handicap == marketAwayHandicap)
                                            {
                                                double AwayOdd = Utils.ParseToDouble(market.AwayOdds);
                                                if (Utils.CheckCondition(AwayOdd, command.OddComparer, OddValue))
                                                {
                                                    writeLog?.Invoke($"=========[checking_AH_bet option] option is matched**************************");
                                                    string directlink = "";
                                                    MatchCollection mc = Regex.Matches(market.AwayIT, "^\\d+[a-zA-Z]+(?<f>\\d+)-(?<fp>\\d+)_");
                                                    if (mc.Count == 1)
                                                    {
                                                        Match m = mc[0];
                                                        string f = m.Groups["f"].Value;
                                                        string fp = m.Groups["fp"].Value;

                                                        directlink = $"{fp}|{market.HomeOD}|{f}";
                                                    }

                                                    if (!string.IsNullOrEmpty(directlink))
                                                    {
                                                        if (writeLog == null)
                                                        {
                                                            AddPickHistory(matchData, command, $"亚洲让分盘 客队盘口 {handicap} 当前赔率 {AwayOdd} {command.OddComparer.GetDescription()} 设置赔率 {OddValue} Handicap {market.AwayHdp} EventID {matchData.EventID} DirectLink {directlink}");
                                                            //SendWebAPI("Joe-SoccerLiver", ToHexString("soccer|" + matchData.LeagueName + "|" + matchData.HomeName + "|" + matchData.AwayName), ToHexString($"AH2({market.AwayHdp})"), ToHexString(matchData.EventID), ToHexString(directlink));
                                                            Global.LiveResultNotifier("Joe-SoccerLiver", "soccer|" + matchData.LeagueName + "|" + matchData.HomeName + "|" + matchData.AwayName + "|" + matchData.Score, $"AH2({market.AwayHdp})", matchData.EventID, directlink);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Global.WriteLog(LOGLEVEL.FILE, LOGTYPE.INDATA, $"Error GetDirectlink2 {market.AwayIT}");
                                                    }
                                                    break;
                                                }
                                                else
                                                {
                                                    writeLog?.Invoke($"[checking_AH_bet option] option is mismatched");
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case BET_MARKET.GOAL_LINE:
                                    {
                                        double beautyHandicap = 0;

                                        double beautyOdd = 10000;
                                        string beautyOd = "";

                                        string beautyIT = "";

                                        foreach (GoalLineMarket market in matchData.GoalLine.MarketData)
                                        {
                                            double marketOverHandicap = Utils.ParseToDouble(market.OverHdp);
                                            double marketOverOdd = Utils.ParseToDouble(market.OverOdds);
                                            double marketUnderHandicap = Utils.ParseToDouble(market.UnderHdp);
                                            double marketUnderOdd = Utils.ParseToDouble(market.UnderOdds);

                                            writeLog?.Invoke($"[checking_GL_bet option] Over HD:{market.OverHdp} ODD:{market.OverOdds} Under HD:{market.UnderHdp} ODD:{market.UnderOdds}  condition HD: {handicap}");

                                            if (command.BetOverUnder == BET_OVERUNDER.OVER)
                                            {
                                                if (marketOverHandicap >= handicap)
                                                {
                                                    if (Utils.CheckCondition(marketOverOdd, command.OddComparer, OddValue))
                                                    {
                                                        writeLog?.Invoke($"=========[checking_GL_bet option] option is matched");
                                                        if (marketOverOdd < beautyOdd)
                                                        {
                                                            beautyHandicap = marketOverHandicap;
                                                            beautyOdd = marketOverOdd;
                                                            beautyIT = market.OverIT;
                                                            beautyOd = market.OverOD;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        writeLog?.Invoke($"[checking_GL_bet option] option is mismatched");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (marketUnderHandicap <= handicap)
                                                {
                                                    if (Utils.CheckCondition(marketUnderOdd, command.OddComparer, OddValue))
                                                    {
                                                        writeLog?.Invoke($"=========[checking_GL_bet option] option is matched**************************");
                                                        if (marketUnderOdd < beautyOdd)
                                                        {
                                                            beautyHandicap = marketUnderHandicap;
                                                            beautyOdd = marketUnderOdd;
                                                            beautyIT = market.UnderIT;
                                                            beautyOd = market.UnderOD;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        writeLog?.Invoke($"[checking_GL_bet option] option is mismatched");
                                                    }
                                                }
                                            }

                                        }

                                        if (!string.IsNullOrEmpty(beautyIT))
                                        {
                                            writeLog?.Invoke($"[checking_GL_bet option] best option beautyHD {beautyHandicap} Odd {beautyOd}");

                                            string directlink = "";
                                            MatchCollection mc = Regex.Matches(beautyIT, "^\\d+[a-zA-Z]+(?<f>\\d+)-(?<fp>\\d+)_");
                                            if (mc.Count == 1)
                                            {
                                                Match m = mc[0];
                                                string f = m.Groups["f"].Value;
                                                string fp = m.Groups["fp"].Value;

                                                directlink = $"{fp}|{beautyOd}|{f}";
                                            }

                                            if (!string.IsNullOrEmpty(directlink))
                                            {
                                                if (writeLog == null)
                                                {
                                                    if (writeLog == null)
                                                    {
                                                        AddPickHistory(matchData, command, $"大小盘  {command.BetOverUnder.GetDescription()} 设置盘口 {handicap} 当前盘口 {beautyHandicap}  当前赔率 {beautyOdd} {command.OddComparer.GetDescription()} 设置赔率 {OddValue} Handicap {beautyHandicap} EventID {matchData.EventID} DirectLink {directlink}");

                                                        //SendWebAPI("Joe-SoccerLiver", ToHexString("soccer|" + matchData.LeagueName + "|" + matchData.HomeName + "|" + matchData.AwayName), ToHexString($"{command.BetOverUnder.GetDescription()}{beautyHandicap}"), ToHexString(matchData.EventID), ToHexString(directlink));
                                                        Global.LiveResultNotifier("Joe-SoccerLiver", "soccer|" + matchData.LeagueName + "|" + matchData.HomeName + "|" + matchData.AwayName + "|" + matchData.Score, $"{command.BetOverUnder.GetDescription()}{beautyHandicap}", matchData.EventID, directlink);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Global.WriteLog(LOGLEVEL.FILE, LOGTYPE.INDATA, $"Error GetDirectlink3 {beautyIT}");
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            writeLog?.Invoke($"[checking_bet option] exception: {ex}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, "isMatchCondition: " + ex.ToString());
                }
            }                
            
            return isMatched;
        }

        public static string ToHexString(string param)
        {
            return ByteArrayToString(StringToByte(param));
        }

        public static string ToLiteralString(string param)
        {
            return ByteToString(StringToByteArray(param));
        }
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        private static string ByteToString(byte[] strByte)
        {
            string str = Encoding.UTF8.GetString(strByte);
            return str;
        }

        private static byte[] StringToByte(string str)
        {
            byte[] StrByte = Encoding.UTF8.GetBytes(str);
            return StrByte;
        }

        private static void AddPickHistory(SoccerMatchData matchData, COMMAND command, string description)
        {
            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, description);
            PickResultData resultData = new PickResultData(matchData, command, description);
            resultData.IssuedTime = DateTime.Now;
            Global.resultHistory.Add(resultData);
            WriteBetHistory(resultData);

            Global.resultEvent(Global.resultHistory);
        }
        public static void SendWebAPI(string name, string param1, string param2 = "", string param3 = "", string param4 = "")
        {
            
#if (DEBUG)
            return;
#endif
            Thread postAPI = new Thread(() =>
            {
                try
                {
                    //var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://hca07s88ab8.sn.mynetname.net:80/API/TipNotify");
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://192.168.88.200:16002/API/TipNotify");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";



                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        string json = $"{{\"name\":\"{name}\"," +
                                          $"\"param1\":\"{param1}\"," +
                                          $"\"param2\":\"{param2}\"," +
                                          $"\"param3\":\"{param3}\"," +
                                          $"\"param4\":\"{param4}\"}}";

                        streamWriter.Write(json);
                    }

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                { }

                //try
                //{
                //    var request = (HttpWebRequest)WebRequest.Create("http://192.168.88.5:16002/API/TipNotify");

                //    var postData = "thing1=" + Uri.EscapeDataString("hello");
                //    postData += "&thing2=" + Uri.EscapeDataString("world");
                //    var data = Encoding.ASCII.GetBytes(postData);

                //    request.Accept = "*/*";
                //    request.Expect = null;
                //    request.UserAgent = "PostmanRuntime/7.31.1";
                //    request.Method = "POST";
                //    request.ContentType = "application/x-www-form-urlencoded";
                //    request.ContentLength = data.Length;

                //    using (var stream = request.GetRequestStream())
                //    {
                //        stream.Write(data, 0, data.Length);
                //    }

                //    var response = (HttpWebResponse)request.GetResponse();

                //    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                //}
                //catch { }
            });
            postAPI.Start();
        }
        private void sendTelegramMsg(string text)
        {
            try
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, string.Format("[sendTelegramMsg] {0}", text));
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, string.Format("[sendTelegramMsg] Exception => {0}", ex.ToString()));
            }
        }
    }
}


//Locator.subscriptionManager._streamDataProcessor._serverConnection._currentTransportMethod.socketDataCallback