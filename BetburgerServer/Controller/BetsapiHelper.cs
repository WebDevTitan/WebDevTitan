using BetburgerServer.Constant;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protocol;
using SeastoryServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BetburgerServer.Controller
{
#if (!FORSALE)

    public class FDSelector
    {
        public string fd;
        public double lineOffset;
        public double oddOffset;
    }
    public class HistoryData
    {
        public DateTime timeStamp;
        public string siteUrl;
        public string eventUrl;
        public string extra;    //direct link for kind9(surebet)
    }
    public class PrematchEventData
    {
        public DateTime timeStamp;
        public Dictionary<string, List<dynamic>> listData;

        public PrematchEventData()
        {
            timeStamp = DateTime.MinValue;
            listData = new Dictionary<string, List<dynamic>>();
        }
    }

    public class InplayEventData
    {
        public DateTime timeStamp;
        public List<dynamic> listData;

        public InplayEventData()
        {
            timeStamp = DateTime.MinValue;
            listData = new List<dynamic>();
        }
    }

    public class BetsapiHelper
    {
        private static BetsapiHelper _instance = null;

        public static BetsapiHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BetsapiHelper();
                }

                return _instance;
            }
        }

        private PrematchEventData PrematchEventList = new PrematchEventData();
        private InplayEventData InplayEventList = new InplayEventData();

        private object EventListLocker = new object();

        private Dictionary<string, HistoryData> historyData = new Dictionary<string, HistoryData>();
        


        private string inplayLink = "";
        private string prematchLink = "";
        private string ipViewLink = "";
        private string ipViewLinkRaw = "";
        private string pmViewLinkRaw = "";
        private string pmViewLinkNew = "";

        Dictionary<string, int> allSports = new Dictionary<string, int>();
        public BetsapiHelper()
        {

            initEndPoint();
            initSports();
        }

        private void initSports()
        {            
            allSports["soccer"] = 1;
            allSports["horse racing"] = 2;
            allSports["cricket"] = 3;
            allSports["greyhounds"] = 4;
            allSports["rugby union"] = 8;
            allSports["boxing"] = 9;
            allSports["american football"] = 12;
            allSports["tennis"] = 13;
            allSports["snooker"] = 14;
            allSports["darts"] = 15;
            allSports["baseball"] = 16;
            allSports["ice hockey"] = 17;
            allSports["hockey"] = 17;
            allSports["basket"] = 18;
            allSports["basketball"] = 18;
            allSports["rugby league"] = 19;
            allSports["handball"] = 78;
            allSports["futsal"] = 83;
            allSports["floorball"] = 90;
            allSports["volleyball"] = 91;
            allSports["table tennis"] = 92;
            allSports["tabletennis"] = 92;
            allSports["tennis table"] = 92;
            allSports["tennistable"] = 92;
            allSports["badminton"] = 94;
            allSports["beach volleyball"] = 95;
            allSports["squash"] = 107;
            allSports["water polo"] = 110;
            allSports["waterpolo"] = 110;
            allSports["cs:go"] = 151;
            allSports["dota"] = 151;
            allSports["e-sports"] = 151;
            allSports["e sports"] = 151;
            
        }

        private void initEndPoint()
        {
            inplayLink = "https://api.b365api.com/v1/bet365/inplay?token=" + cServerSettings.GetInstance().BetsapiToken;
            prematchLink = "https://api.b365api.com/v1/bet365/upcoming?token=" + cServerSettings.GetInstance().BetsapiToken + "&sport_id={0}&page={1}";
            ipViewLink = "https://api.b365api.com/v1/bet365/event?token=" + cServerSettings.GetInstance().BetsapiToken + "&FI={0}";
            ipViewLinkRaw = "https://api.b365api.com/v1/bet365/event?token=" + cServerSettings.GetInstance().BetsapiToken + "&FI=%s&raw=Yes";
            pmViewLinkRaw = "https://api.betsapi.com/v3/bet365/prematch?token=" + cServerSettings.GetInstance().BetsapiToken + "&FI={0}&raw=Yes";
            pmViewLinkNew = "https://api.betsapi.com/v3/bet365/prematch?token=" + cServerSettings.GetInstance().BetsapiToken + "&FI=%s";
        }

        private int getSportId(string sportName)
        {
            try
            {
                sportName = sportName.ToLower();
                if (string.IsNullOrEmpty(sportName))
                    return -1;                
                return allSports[sportName];
            }
            catch (Exception ex)
            {

            }
            return -1;
        }

        private double GetHandicapOffset(BetburgerInfo info)
        {
            if (info.sport == "soccer")
                return 0.25;        
            return 2;
        }
        private double GetHandicapLabelValue(string n)
        {
            string[] splits = n.Split(',');
            if (isNumber(n))
                return Utils.ParseToDouble(n);
            if (splits.Length == 2 && isNumber(splits[0].Trim()) && isNumber(splits[1].Trim()))
            {
                return (Utils.ParseToDouble(splits[0].Trim()) + Utils.ParseToDouble(splits[1].Trim())) / 2;
            }
            if (n.ToLower().StartsWith("o ") && isNumber(n.ToLower().Replace("o ", "")))
                return Utils.ParseToDouble(n.ToLower().Replace("o ", ""));
            if (n.ToLower().StartsWith("u ") && isNumber(n.ToLower().Replace("u ", "")))
                return Utils.ParseToDouble(n.ToLower().Replace("u ", ""));
            if (n.ToLower().StartsWith("over ") && isNumber(n.ToLower().Replace("over ", "")))
                return Utils.ParseToDouble(n.ToLower().Replace("over ", ""));
            if (n.ToLower().StartsWith("under ") && isNumber(n.ToLower().Replace("under ", "")))
                return Utils.ParseToDouble(n.ToLower().Replace("under ", ""));
            return -100;
        }

        private string getHandicapSide(string n)
        {
            if (n.ToLower().StartsWith("o ") && isNumber(n.ToLower().Replace("o ", "")))
                return "o";
            if (n.ToLower().StartsWith("u ") && isNumber(n.ToLower().Replace("u ", "")))
                return "u";
            if (n.ToLower().StartsWith("over ") && isNumber(n.ToLower().Replace("over ", "")))
                return "o";
            if (n.ToLower().StartsWith("under ") && isNumber(n.ToLower().Replace("under ", "")))
                return "u";
            return "e";
        }
        private bool IsHandicapLabel(string n)
        {
            string[] splits = n.Split(',');
            try
            {
                if (isNumber(n))
                    return true;
                if (splits.Length == 2 && isNumber(splits[0].Trim()) && isNumber(splits[1].Trim()))
                {
                    return true;
                }
                if (n.ToLower().StartsWith("o ") && isNumber(n.ToLower().Replace("o ", "")))
                    return true;
                if (n.ToLower().StartsWith("u ") && isNumber(n.ToLower().Replace("u ", "")))
                    return true;
                if (n.ToLower().StartsWith("over ") && isNumber(n.ToLower().Replace("over ", "")))
                    return true;
                if (n.ToLower().StartsWith("under ") && isNumber(n.ToLower().Replace("under ", "")))
                    return true;
            }
            catch { }
            return false;
        }
        private bool isNumber(string n)
        {
            double result = 0;

            return double.TryParse(n, out result);
            
		}
        private string getRefinedHandicap(string orig)
        {
            if (string.IsNullOrEmpty(orig))
                return orig;

            string refined = "";
            string[] splits = orig.Split(',');
            if (splits.Length == 2 && isNumber(splits[0].Trim()) && isNumber(splits[1].Trim()))
            {
                refined = ((Utils.ParseToDouble(splits[0].Trim()) + Utils.ParseToDouble(splits[1].Trim())) / 2).ToString();
            }
            else
            {
                splits = orig.Split(' ');
                for (var i = 0; i < splits.Length; i++)
                {
                    if (isNumber(splits[i]))
                        refined += $"{Utils.ParseToDouble(splits[i])} ";
                    else
                        refined += $"{splits[i]} ";
                }
                refined = refined.Trim();
            }
            return refined;
        }
        
        public void UpdateBet365SiteUrl(string betsapiresponse, ref BetburgerInfo info, out string logResult)
        {
            logResult = "";
#if (DEBUG)
            Trace.WriteLine($"---------------------------UpdateBet365SiteUrl -----------------");
            Trace.WriteLine($"sport: {info.sport} | leage: {info.league} | home: {info.homeTeam} | away: {info.awayTeam} | outcome: {info.outcome} | extra: {info.extra} | odd: {info.odds}");
#endif
#if (!SUREBETTEST)

            string[] linkArray = info.direct_link.Split('|');
            
            

            string fd = "";
            if (info.kind == PickKind.Type_9)
            {
                //it doesn't have direct_link
            }
            else
            {
                if (linkArray.Count() != 3)
                {
                    logResult = $"[UpdateBet365SiteUrl] kind: {info.kind} direct_link error {info.direct_link}";
                    return;
                }
                fd = linkArray[0];
            }
                      

            
            if (info.kind == PickKind.Type_7)
            {//Live
                string link_Suffix = "";
                string PA_IT = "";
                string eventid = "";
                try
                {
                    Monitor.Enter(EventListLocker);
                    foreach (var matchArray in InplayEventList.listData)
                    {
                        foreach (var match in matchArray)
                        {
                            try
                            {
                                if (match.type.ToString() != "EV")
                                    continue;

                                if (match.OI.ToString() == linkArray[2])
                                {
                                    link_Suffix = $"IP/EV15{match.C2.ToString()}2C1";
                                    
                                    eventid = Regex.Match(match.ID.ToString(), @"(?<VAL>[0-9]*)[a-zA-Z]").Groups["VAL"].Value;

                                    try
                                    {
                                        info.league = match.CT.ToString();

                                        string NAVal = match.NA.ToString();
                                        string[] teams = NAVal.Split(new string[] { " v " }, StringSplitOptions.RemoveEmptyEntries);
                                        if (teams.Count() == 2)
                                        {
                                            if (!string.IsNullOrEmpty(teams[0].Trim()) && !string.IsNullOrEmpty(teams[1].Trim()))
                                            {
                                                info.homeTeam = teams[0].Trim();
                                                info.awayTeam = teams[1].Trim();
                                            }
                                        }
                                        info.eventTitle = string.Format("{0} - {1}", info.homeTeam, info.awayTeam);
                                    }
                                    catch (Exception ex){ }


                                    break;
                                }
                            }
                            catch (Exception ex){ }
                        }

                        if (!string.IsNullOrEmpty(eventid))
                            break;
                    }
                }
                catch { }
                finally
                {
                    Monitor.Exit(EventListLocker);
                }

                if (historyData.ContainsKey(eventid + "-" + fd))
                {
                    if (DateTime.Now.Subtract(historyData[eventid + "-" + fd].timeStamp).TotalSeconds > 60)
                    {
                        historyData.Remove(eventid + "-" + fd);
                    }
                    else
                    {
                        info.siteUrl = historyData[eventid + "-" + fd].siteUrl;
                        info.eventUrl = historyData[eventid + "-" + fd].eventUrl;
                        info.extra = historyData[eventid + "-" + fd].extra;
                        return;
                    }
                }
                if (string.IsNullOrEmpty(link_Suffix))
                {
                    logResult = $"[UpdateBet365SiteUrl] Type_7 not found event {info}";
                    return;
                }                

                HttpClient httpClient = new HttpClient();
                HttpResponseMessage betsapiInplayEventsListRes = httpClient.GetAsync(string.Format(ipViewLink, eventid)).Result;
                betsapiInplayEventsListRes.EnsureSuccessStatusCode();
                string betsapiResponse = betsapiInplayEventsListRes.Content.ReadAsStringAsync().Result;
                JObject jsonBetsapiResp = JsonConvert.DeserializeObject<JObject>(betsapiResponse);

                string MG_NA = "";
                string MA_NA = "";
                string PA_NA = "";
                string PA_N2 = "";
    
                if (jsonBetsapiResp["success"].ToString() == "1")
                {
                    try
                    {
                        List<string> bodyList = new List<string>();
                        JArray results = jsonBetsapiResp["results"][0].ToObject<JArray>();
                        foreach (var stemData in results)
                        {
                            try
                            {

                                if (stemData["type"].ToString() == "MG")
                                {
                                    MG_NA = "";
                                    if (stemData["NA"] != null && !string.IsNullOrEmpty(stemData["NA"].ToString()))
                                    {
                                        MG_NA = stemData["NA"].ToString();
                                    }
                                }

                                if (stemData["type"].ToString() == "MA")
                                {
                                    MA_NA = "";
                                    if (stemData["NA"] != null && !string.IsNullOrEmpty(stemData["NA"].ToString()))
                                    {
                                        MA_NA = stemData["NA"].ToString();
                                    }
                                }

                                if (stemData["type"].ToString() != "PA")
                                {
                                    continue;
                                }

                                if (stemData["FI"] == null || stemData["ID"] == null)
                                {
                                    continue;
                                }

                                PA_NA = "";
                                if (stemData["NA"] != null && !string.IsNullOrEmpty(stemData["NA"].ToString()))
                                {
                                    PA_NA = stemData["NA"].ToString();
                                }

                                PA_N2 = "";
                                if (stemData["N2"] != null && !string.IsNullOrEmpty(stemData["N2"].ToString()))
                                {
                                    PA_N2 = stemData["N2"].ToString();
                                }

                                if (stemData["FI"].ToString() == linkArray[2] && stemData["ID"].ToString() == linkArray[0])
                                {
                                    PA_IT = stemData["IT"].ToString();
                                    break;
                                }
                            }
                            catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }

                if (string.IsNullOrEmpty(PA_IT))
                {
                    logResult = $"[UpdateBet365SiteUrl] Type_7 not found event {PA_IT} betsapiResponse: {betsapiResponse}";
                    return;
                }

                info.siteUrl = link_Suffix;
                info.eventUrl = PA_IT;
                
                info.extra = MG_NA;
                if (!string.IsNullOrEmpty(MA_NA))
                    info.extra += "|" + MA_NA;
                if (!string.IsNullOrEmpty(PA_N2))
                {
                    info.extra += "|" + PA_N2;
                }
                else
                {
                    if (string.IsNullOrEmpty(PA_NA))
                        info.extra += " " + PA_NA;
                }
                info.extra += "(" + info.outcome + ")";

                HistoryData data = new HistoryData();
                data.timeStamp = DateTime.Now;
                data.siteUrl = info.siteUrl;
                data.eventUrl = info.eventUrl;
                data.extra = info.extra;

                historyData[eventid + "-" + fd] = data;
            }
            else
            {//Prematch
                if (getSportId(info.sport) == -1)
                {
                    logResult = $"[UpdateBet365SiteUrl] prematch sport not found: {info.sport}";
                    return;
                }

                string eventid = "";

                try
                {
                    Monitor.Enter(EventListLocker);
                    foreach (var match in PrematchEventList.listData[info.sport.ToLower()])
                    {
#if (DEBUG)
                        Trace.WriteLine($"checking mhome: {match.home.name.ToString()} maway: {match.away.name.ToString()} ihome: {info.homeTeam} iaway: {info.awayTeam}");
#endif
                        if (Utils.isSameMatch(match.home.name.ToString(), match.away.name.ToString(), info.homeTeam, info.awayTeam))
                        {
                            eventid = match.id.ToString();

                            try
                            {
                                info.league = match.league.name.ToString();
                                info.homeTeam = match.home.name.ToString();
                                info.awayTeam = match.away.name.ToString();
                                info.eventTitle = string.Format("{0} - {1}", info.homeTeam, info.awayTeam);
                            }
                            catch (Exception ex) { }

                            break;
                        }
                    }
                }
                catch (Exception ex) { }
                finally {
                    Monitor.Exit(EventListLocker);
                }

                if (string.IsNullOrEmpty(eventid))
                {
                    SaveNameToFile(info);
                    if (PrematchEventList.listData.ContainsKey(info.sport.ToLower()))
                        logResult = $"[UpdateBet365SiteUrl] not found event: {info.sport} : {info.homeTeam} - {info.awayTeam}";
                    return;
                }

                if (historyData.ContainsKey(eventid + "-" + fd))
                {
                    if (DateTime.Now.Subtract(historyData[eventid + "-" + fd].timeStamp).TotalSeconds > 60)
                    {
                        historyData.Remove(eventid + "-" + fd);
                    }
                    else
                    {
                        info.siteUrl = historyData[eventid + "-" + fd].siteUrl;
                        info.eventUrl = historyData[eventid + "-" + fd].eventUrl;
                        return;
                    }
                }
                else if (historyData.ContainsKey(eventid + "-" + info.outcome))
                {
                    if (DateTime.Now.Subtract(historyData[eventid + "-" + info.outcome].timeStamp).TotalSeconds > 60)
                    {
                        historyData.Remove(eventid + "-" + info.outcome);
                    }
                    else
                    {
                        info.siteUrl = historyData[eventid + "-" + info.outcome].siteUrl;
                        info.eventUrl = historyData[eventid + "-" + info.outcome].eventUrl;
                        if (!string.IsNullOrEmpty(historyData[eventid + "-" + info.outcome].extra))
                            info.direct_link = historyData[eventid + "-" + info.outcome].extra;
                        return;
                    }
                }

              
                

                HttpClient httpClient = new HttpClient();
                HttpResponseMessage betsapiPrematchEventsListRes = httpClient.GetAsync(string.Format("https://api.betsapi.com/v3/bet365/prematch?token=" + cServerSettings.GetInstance().BetsapiToken + "&FI={0}&raw=Yes", eventid)).Result;
                betsapiPrematchEventsListRes.EnsureSuccessStatusCode();
                string betsapiResponse = betsapiPrematchEventsListRes.Content.ReadAsStringAsync().Result;
#else
            if (info.kind == PickKind.Type_9)
            {//
                string betsapiResponse = betsapiresponse;
                string fd = "";
                string eventid = "aaa";
#endif

                Dictionary<string, JObject> pageData = new Dictionary<string, JObject>();
                string MG_ID = "";
                string hierachy_Data = "";
                string link_Suffix = "";

                JObject jsonBetsapiResp = JsonConvert.DeserializeObject<JObject>(betsapiResponse);
                if (jsonBetsapiResp["success"].ToString() == "1")
                {

                    try
                    {
                        List<string> bodyList = new List<string>();
                        JObject results = jsonBetsapiResp["results"][0].ToObject<JObject>();
                        foreach (KeyValuePair<string, JToken> property in results)
                        {
                            if (property.Key == "event_id" || property.Key == "FI")
                                continue;
                            Console.WriteLine(property.Key + " - " + property.Value);

                            if (property.Key != "others")
                            {
                                bodyList.Add(property.Value["body"].ToString());
                            }
                            else
                            {
                                JArray othersArray = property.Value.ToObject<JArray>();
                                foreach (var listdata in othersArray)
                                {
                                    if (listdata["body"] != null)
                                        bodyList.Add(listdata["body"].ToString());
                                }
                            }
                        }


                        if (info.kind == PickKind.Type_9)
                        { // finding market PA_ID from surebet Pick
                            
                            List<FDSelector> fdList = new List<FDSelector>();
                            ParseBet_Bet365 second_parsebet = null, parsebet = null;
                            parsebet = ParseBet_Bet365.ConvertBetburgerPick2ParseBet_365(info, out second_parsebet);
                            while (true)
                            {
                                fd = "";
#if (DEBUG)
                                Trace.WriteLine($"parsebet tabLabel: {parsebet.TabLabel} | marketLabel: {parsebet.MarketLabel} | tableHeader: {parsebet.TableHeader} | RowHeader: {parsebet.RowHeader} | ColHeader: {parsebet.ColHeader} | ParticipantName: {parsebet.ParticipantName} | odd: {parsebet.odd} ");
#endif
                                parsebet.TableHeader = parsebet.TableHeader.Replace("*home*", info.homeTeam).Replace("*away*", info.awayTeam);

                                parsebet.RowHeader = parsebet.RowHeader.Replace("*home*", info.homeTeam).Replace("*away*", info.awayTeam);

                                parsebet.ColHeader = parsebet.ColHeader.Replace("*home*", info.homeTeam).Replace("*away*", info.awayTeam);

                                parsebet.ParticipantName = parsebet.ParticipantName.Replace("*home*", info.homeTeam).Replace("*away*", info.awayTeam);
                                if (!ParseBet_Bet365.IsCorrectParseBet(parsebet))
                                {
                                    //write log to add pattern
#if (DEBUG)
                                    Trace.WriteLine($"Incorrect Parsebet");
#endif
                                    SaveToFile(info, betsapiResponse, "Incorrect Parsebet");
                                }
                                else
                                {
                                    parsebet.ParticipantName = parsebet.ParticipantName.Replace("−", "-");
                                    parsebet.RowHeader = parsebet.RowHeader.Replace("−", "-");

                                    foreach (string body in bodyList)
                                    {
#if (DEBUG)
                                        Trace.WriteLine($"checking body --------- ");
                                        Trace.WriteLine($"{body}");
#endif

                                        JObject stemData = Bet365DataFeeder.ParseData(body);

                                        if (stemData["CLData"] == null)
                                            continue;

                                        JArray CLArray = stemData["CLData"].ToObject<JArray>();
                                        foreach (JObject CLData in CLArray)
                                        {
                                            if (CLData["EVData"] == null)
                                                continue;

                                            JArray EVArray = CLData["EVData"].ToObject<JArray>();
                                            foreach (JObject EVData in EVArray)
                                            {
                                                if (EVData["MGData"] == null)
                                                    continue;

                                                JArray MGArray = EVData["MGData"].ToObject<JArray>();
                                                foreach (JObject MGData in MGArray)
                                                {
                                                    if (MGData["MAData"] == null || MGData["MAData"].Children().Count() < 1)
                                                        continue;

                                                    if (MGData["NA"] == null)
                                                        continue;

#if (DEBUG)
                                                    Trace.WriteLine($"Checking Marketlabel MGData_NA {MGData["NA"].ToString().ToLower().Trim()} | marketlabel: {parsebet.MarketLabel.ToLower().Trim()}");

#endif
                                                    if (MGData["NA"].ToString().ToLower().Trim() != parsebet.MarketLabel.ToLower().Trim())
                                                    {
                                                        if (parsebet.MarketLabel == "Corners")
                                                        {
                                                            if (MGData["NA"].ToString() != "Alternative Corners" && MGData["NA"].ToString() != "Asian Total Corners" && MGData["NA"].ToString() != "Corners 2-Way")
                                                                continue;
                                                        }
                                                        else if (parsebet.MarketLabel.Contains("Goals Over/Under"))
                                                        {
                                                            if (MGData["NA"].ToString() != "Alternative Total Goals")
                                                                continue;
                                                        }
                                                        else if (parsebet.MarketLabel.Contains("Handicap"))
                                                        {//Alternative Corners and Corners can be seprated, but they are same originally
                                                            if ((MGData["NA"].ToString() != "Alternative " + parsebet.MarketLabel) && (MGData["NA"].ToString() != "Alternative " + parsebet.MarketLabel + " Result"))
                                                                continue;
                                                        }
                                                        else
                                                        {
                                                            continue;
                                                        }
                                                    }
                                                    string tableHeader = "";
                                                    List<string> lineArray = new List<string>();
                                                    JArray MAArray = MGData["MAData"].ToObject<JArray>();
                                                    foreach (JObject MAData in MAArray)
                                                    {
                                                        if (MAData["PAData"] == null)
                                                            continue;
                                                        JArray PAArray = MAData["PAData"].ToObject<JArray>();



                                                        string itrMA_NA = "", itrMA_PY = "";

                                                        if (MAData["NA"] != null && !string.IsNullOrEmpty(MAData["NA"].ToString()))
                                                        {
                                                            itrMA_NA = MAData["NA"].ToString().Trim();
                                                        }

                                                        if (MAData["PY"] != null && !string.IsNullOrEmpty(MAData["PY"].ToString()))
                                                        {
                                                            itrMA_PY = MAData["PY"].ToString().Trim();
                                                        }


                                                        //
                                                        //MA_PY =
                                                        //_a :explanation not displayed in table
                                                        //data column
                                                        //_f :no row header data, no col header data
                                                        //_d, _D :row header data, col header data  , capital has SU attribute
                                                        //_e :row header data, col header data with handicap
                                                        //_c :row header (or no), with name

                                                        //de, da, db : header column
                                                        //dk : no header, no column data
                                                        //
                                                        if (itrMA_PY == "da" || itrMA_PY == "db" || itrMA_PY == "de")
                                                        {
                                                            lineArray.Clear();
                                                            tableHeader = itrMA_NA;
#if (DEBUG)
                                                            Trace.WriteLine($"Setting Table Header : {itrMA_NA}");
#endif
                                                        }


                                                        for (int p = 0; p < PAArray.Count; p++)
                                                        {
                                                            try
                                                            {
                                                                JToken PAData = PAArray.ElementAt(p);

                                                                string itrPA_ID = "", itrPA_OD = "", itrPA_NA = "", itrPA_HD = "";

                                                                if (PAData["ID"] != null && !string.IsNullOrEmpty(PAData["ID"].ToString()))
                                                                {
                                                                    itrPA_ID = PAData["ID"].ToString().Trim();
                                                                }

                                                                if (PAData["OD"] != null && !string.IsNullOrEmpty(PAData["OD"].ToString()))
                                                                {
                                                                    itrPA_OD = PAData["OD"].ToString().Trim();
                                                                }

                                                                if (PAData["NA"] != null && !string.IsNullOrEmpty(PAData["NA"].ToString()))
                                                                {
                                                                    itrPA_NA = PAData["NA"].ToString().Trim();
                                                                }

                                                                if (PAData["HD"] != null && !string.IsNullOrEmpty(PAData["HD"].ToString()))
                                                                {
                                                                    itrPA_HD = PAData["HD"].ToString().Trim();
                                                                }

#if (DEBUG)
                                                                Trace.WriteLine($"Checking participant itrMA_NA : {itrMA_NA} | itrPA_ID : {itrPA_ID} | itrPA_OD : {itrPA_OD} | itrPA_NA : {itrPA_NA} | itrPA_HD : {itrPA_HD}");
#endif
                                                                if (itrMA_PY == "da" || itrMA_PY == "db" || itrMA_PY == "de")
                                                                {
#if (DEBUG)
                                                                    Trace.WriteLine($"Adding col header: {lineArray.Count} - {itrPA_NA}");
#endif
                                                                    lineArray.Add(itrPA_NA);
                                                                }
                                                                else
                                                                {
                                                                    bool bIsBinded = true;
                                                                    FDSelector fdResult = new FDSelector();
#if (DEBUG)
                                                                    Trace.WriteLine($"Checking in data cell");
#endif
                                                                    if (!string.IsNullOrEmpty(parsebet.TableHeader))
                                                                    {
                                                                        if (string.IsNullOrEmpty(tableHeader) || tableHeader != parsebet.TableHeader)
                                                                        {
                                                                            bIsBinded = false;
#if (DEBUG)
                                                                            Trace.WriteLine($"Incorrect table Header tableHeader : {tableHeader} | parsebet.TableHeader : {parsebet.TableHeader}");
#endif
                                                                        }
                                                                    }
                                                                    if (!string.IsNullOrEmpty(parsebet.RowHeader))
                                                                    {
                                                                        if (lineArray.Count <= p || getRefinedHandicap(lineArray[p]) != getRefinedHandicap(parsebet.RowHeader))
                                                                        {

                                                                            if (IsHandicapLabel(lineArray[p]) && IsHandicapLabel(parsebet.RowHeader) && (Math.Abs(GetHandicapLabelValue(lineArray[p]) - GetHandicapLabelValue(parsebet.RowHeader)) <= GetHandicapOffset(info)))
                                                                            {
                                                                                fdResult.lineOffset = Math.Abs(GetHandicapLabelValue(lineArray[p]) - GetHandicapLabelValue(parsebet.RowHeader));
                                                                                Trace.WriteLine($"Correct, but handicap difference is lower than offset : {Math.Abs(GetHandicapLabelValue(lineArray[p]) - GetHandicapLabelValue(parsebet.RowHeader))}");
                                                                            }
                                                                            else
                                                                            {
                                                                                bIsBinded = false;
#if (DEBUG)
                                                                                Trace.WriteLine($"Incorrect Row Header p: {p} lineArray[p] : {getRefinedHandicap(lineArray[p])} | parsebet.RowHeader : {getRefinedHandicap(parsebet.RowHeader)}");
#endif
                                                                            }
                                                                        }
                                                                    }
                                                                    if (!string.IsNullOrEmpty(parsebet.ColHeader))
                                                                    {
                                                                        if (!string.IsNullOrEmpty(itrMA_NA) && itrMA_NA != parsebet.ColHeader)
                                                                        {
                                                                            bIsBinded = false;
#if (DEBUG)
                                                                            Trace.WriteLine($"Incorrect Col Header itrMA_NA : {itrMA_NA} | parsebet.ColHeader : {parsebet.ColHeader}");
#endif
                                                                        }
                                                                    }
                                                                    if (!string.IsNullOrEmpty(parsebet.ParticipantName))
                                                                    {
                                                                        if (getRefinedHandicap(itrPA_NA) != getRefinedHandicap(parsebet.ParticipantName) && getRefinedHandicap(itrPA_HD) != getRefinedHandicap(parsebet.ParticipantName))
                                                                        {
                                                                            if (IsHandicapLabel(itrPA_HD) && IsHandicapLabel(parsebet.ParticipantName) && (getHandicapSide(itrPA_HD) == getHandicapSide(parsebet.ParticipantName)) && (Math.Abs(GetHandicapLabelValue(itrPA_HD) - GetHandicapLabelValue(parsebet.ParticipantName)) <= GetHandicapOffset(info)))
                                                                            {
                                                                                fdResult.lineOffset = Math.Abs(GetHandicapLabelValue(itrPA_HD) - GetHandicapLabelValue(parsebet.ParticipantName));
                                                                                Trace.WriteLine($"Correct, but handicap difference is lower than offset : {Math.Abs(GetHandicapLabelValue(itrPA_HD) - GetHandicapLabelValue(parsebet.ParticipantName))}");
                                                                            }
                                                                            else
                                                                            {
                                                                                bIsBinded = false;
                                                                                Trace.WriteLine($"Incorrect Participant itrPA_NA: {getRefinedHandicap(itrPA_NA)} | itrPA_HD: {getRefinedHandicap(itrPA_HD)} | parsebet.ParticipantName: {getRefinedHandicap(parsebet.ParticipantName)}");
                                                                            }
                                                                        }
                                                                    }

                                                                    if (bIsBinded)
                                                                    {
#if (DEBUG)
                                                                        Trace.WriteLine($"Found Participant itrPA_ID: {itrPA_ID} | itrPA_OD: {itrPA_OD} ");
#endif
                                                                        fd = itrPA_ID;
                                                                        fdResult.fd = fd;
                                                                        double odd = Utils.FractionToDouble(itrPA_OD);
                                                                        if (info.odds != odd && Math.Abs(info.odds - odd) > 0.1)
                                                                        {
                                                                            fdResult.oddOffset = Math.Abs(info.odds - odd);
                                                                            
#if (DEBUG)
                                                                            Trace.WriteLine($"Found Participant But Odd is Difference info.odds: {info.odds} | itrPA_OD: {itrPA_OD} | diff: {Math.Abs(info.odds - odd)}");
#endif
                                                                        }
                                                                        fdList.Add(fdResult);

                                                                    }
                                                                    else
                                                                    {
#if (DEBUG)
                                                                        Trace.WriteLine($"Incorrect Participant -");
#endif
                                                                    }
                                                                }
                                                            }
                                                            catch { }
                                                        }

                                                        //if (!string.IsNullOrEmpty(fd))
                                                        //    break;
                                                    }
                                                    //if (!string.IsNullOrEmpty(fd))                                                    
                                                    //    break;                                                  
                                                }
                                                //if (!string.IsNullOrEmpty(fd))
                                                //    break;
                                            }
                                            //if (!string.IsNullOrEmpty(fd))
                                            //    break;
                                        }
                                        //if (!string.IsNullOrEmpty(fd))
                                        //    break;
                                    }
                                }


                                if (second_parsebet != null)
                                {
                                    parsebet = second_parsebet;
                                    second_parsebet = null;
                                    Trace.WriteLine($"[UpdateBet365SiteUrl] finding with second Parsebet");
                                }
                                else
                                {
                                    break;
                                }                                
                            }

                            //getting the best fd
                            fd = "";

                            double minlineOffset = 100;
                            double oddOffset = 0;
                            foreach (var fdItr in fdList)
                            {
                                if (fdItr.lineOffset < minlineOffset)
                                {
                                    fd = fdItr.fd;
                                    minlineOffset = fdItr.lineOffset;
                                    oddOffset = fdItr.oddOffset;
                                }
                            }

                            if (string.IsNullOrEmpty(fd))
                            {
                                SaveToFile(info, betsapiResponse, "Not Found Participant", parsebet);
#if (DEBUG)
                                Trace.WriteLine($"[UpdateBet365SiteUrl]===== Found (failed) in Pick9");
#endif
                            }
                            else
                            {
                                if (minlineOffset > 0.1 || oddOffset > 0.1)
                                    SaveToFile(info, betsapiResponse, $"Offset line : {minlineOffset} odd: {oddOffset}", parsebet);
#if (DEBUG)
                                Trace.WriteLine($"[UpdateBet365SiteUrl]===== Found (successed) in Pick9 fd: {fd} line_offset: {minlineOffset} odd_offset: {oddOffset}");
#endif
                            }
                        }

                        if (!string.IsNullOrEmpty(fd))
                        {
                            foreach (string body in bodyList)
                            {
                                JObject stemData = Bet365DataFeeder.ParseData(body);

                                string siteLink = "";

                                if (stemData["CLData"] == null)
                                    continue;
                                //check if it has PA
                                JArray CLArray = stemData["CLData"].ToObject<JArray>();
                                foreach (JObject CLData in CLArray)
                                {
                                    if (CLData["EVData"] == null)
                                        continue;
                                    JArray EVArray = CLData["EVData"].ToObject<JArray>();
                                    foreach (JObject EVData in EVArray)
                                    {
                                        if (EVData["MGData"] == null)
                                            continue;
                                        JArray MGArray = EVData["MGData"].ToObject<JArray>();
                                        foreach (JObject MGData in MGArray)
                                        {
                                            if (MGData["MAData"] == null)
                                                continue;
                                            JArray MAArray = MGData["MAData"].ToObject<JArray>();
                                            foreach (JObject MAData in MAArray)
                                            {
                                                if (MGData.ContainsKey("SY") && MGData["SY"].ToString() == "cm")
                                                {//menu 
                                                    if (MAData.ContainsKey("LS") && MAData["LS"].ToString() == "1")
                                                    {
                                                        //it's page to open
                                                        siteLink = MAData["PD"].ToString();
                                                        int nIndexofP = siteLink.IndexOf("P^");
                                                        if (nIndexofP > 0)
                                                            siteLink = siteLink.Substring(0, nIndexofP);


                                                    }
                                                }
                                                else
                                                {
                                                    if (MAData["PAData"] == null)
                                                        continue;
                                                    JArray PAArray = MAData["PAData"].ToObject<JArray>();
                                                    foreach (JObject PAData in PAArray)
                                                    {
                                                        if (PAData["ID"] != null && PAData["ID"].ToString() == fd)
                                                        {
                                                            MG_ID = MGData["ID"].ToString();

                                                            if (!string.IsNullOrEmpty(siteLink))
                                                            {
                                                                link_Suffix = siteLink;
                                                                hierachy_Data = CLData["Type"] + ";" + CLData["ID"] + "|"
                                                                    + EVData["Type"] + ";" + EVData["ID"] + "|"
                                                                    + MGData["Type"] + ";" + MGData["ID"] + "|"
                                                                    + MAData["Type"] + ";" + MAData["ID"] + "|"
                                                                    + PAData["Type"] + ";" + PAData["ID"];


                                                            }
                                                            else
                                                            {
                                                                hierachy_Data = MGData["Type"] + ";" + MGData["ID"] + "|"
                                                                    + MAData["Type"] + ";" + MAData["ID"] + "|"
                                                                    + PAData["Type"] + ";" + PAData["ID"];
                                                            }
                                                            break;
                                                        }
                                                    }
                                                }

                                                if (!string.IsNullOrEmpty(hierachy_Data))
                                                    break;
                                            }

                                            if (!string.IsNullOrEmpty(hierachy_Data))
                                                break;
                                        }

                                        if (!string.IsNullOrEmpty(hierachy_Data))
                                            break;
                                    }

                                    if (!string.IsNullOrEmpty(hierachy_Data))
                                        break;
                                }

                                if (!string.IsNullOrEmpty(siteLink))
                                {
                                    pageData[siteLink] = stemData;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }                    
                }

                if (string.IsNullOrEmpty(MG_ID))
                {                    
                    logResult = $"[UpdateBet365SiteUrl] not found MG_ID";
                    return;
                }
                else
                {
                    if (string.IsNullOrEmpty(link_Suffix))
                    {
                        foreach (KeyValuePair<string, JObject> page in pageData)
                        {
                            JObject stemData = page.Value;

                            //check if it has PA
                            JArray CLArray = stemData["CLData"].ToObject<JArray>();
                            foreach (JObject CLData in CLArray)
                            {
                                if (CLData["EVData"] == null)
                                    continue;
                                JArray EVArray = CLData["EVData"].ToObject<JArray>();
                                foreach (JObject EVData in EVArray)
                                {
                                    if (EVData["MGData"] == null)
                                        continue;
                                    JArray MGArray = EVData["MGData"].ToObject<JArray>();
                                    foreach (JObject MGData in MGArray)
                                    {
                                        if (MGData["ID"] == null)
                                            continue;

                                        if (MGData["ID"].ToString() == MG_ID)
                                        {
                                            link_Suffix = page.Key.ToString();

                                            hierachy_Data = CLData["Type"] + ";" + CLData["ID"] + "|"
                                                    + EVData["Type"] + ";" + EVData["ID"] + "|"
                                                    + hierachy_Data;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                info.siteUrl = link_Suffix;
                info.eventUrl = hierachy_Data;

                HistoryData data = new HistoryData();
                data.timeStamp = DateTime.Now;
                data.siteUrl = info.siteUrl;
                data.eventUrl = info.eventUrl;

                data.extra = "";
                if (fd != "")
                {
                    data.extra = string.Format("{0}|{1}|{2}", fd, info.odds, eventid);

                    if (string.IsNullOrEmpty(info.direct_link))
                        info.direct_link = data.extra;
                }

                historyData[eventid + "-" + fd] = data;
                historyData[eventid + "-" + info.outcome] = data;

            }
            
        }

        public void RefreshAllInplayEventList()
        {
            HttpClient httpClient = new HttpClient();
            List<dynamic> newEventList = new List<dynamic>();
                       
            bool bRefreshed = false;
            
            try
            {
                HttpResponseMessage betsapiPrematchEventsListRes = httpClient.GetAsync(inplayLink).Result;
                betsapiPrematchEventsListRes.EnsureSuccessStatusCode();
                string betsapiResponse = betsapiPrematchEventsListRes.Content.ReadAsStringAsync().Result;
                dynamic jsonBetsapiResp = JsonConvert.DeserializeObject<dynamic>(betsapiResponse);
                if (jsonBetsapiResp["success"].ToString() == "1")
                {
                    foreach (var objBetsapiEvent in jsonBetsapiResp["results"])
                    {
                        newEventList.Add(objBetsapiEvent);
                    }                        

                    bRefreshed = true;
                }
            }
            catch { }
            

            if (bRefreshed)
            {
                try
                {
                    Monitor.Enter(EventListLocker);
                    InplayEventList.timeStamp = DateTime.Now;
                    InplayEventList.listData = newEventList;
                }
                catch { }
                finally {
                    Monitor.Exit(EventListLocker);
                }
            }
            
        }
        public void RefreshAllPrematchEventList(List<string> sports)
        {
            HttpClient httpClient = new HttpClient();
            foreach (string sport in sports)
            {
                int nSportID = getSportId(sport);
                if (nSportID == -1)
                    continue;

                List<dynamic> newEventList = new List<dynamic>();

                int nPageId = 0;

                bool bRefreshed = false;
                while (nPageId < 100)
                {
                    nPageId++;
                    try
                    {
                        HttpResponseMessage betsapiPrematchEventsListRes = httpClient.GetAsync(string.Format(prematchLink, nSportID, nPageId)).Result;
                        betsapiPrematchEventsListRes.EnsureSuccessStatusCode();
                        string betsapiResponse = betsapiPrematchEventsListRes.Content.ReadAsStringAsync().Result;
                        dynamic jsonBetsapiResp = JsonConvert.DeserializeObject<dynamic>(betsapiResponse);
                        if (jsonBetsapiResp["success"].ToString() == "1")
                        {   
                            foreach (var objBetsapiEvent in jsonBetsapiResp["results"])
                            {
                                newEventList.Add(objBetsapiEvent);
                            }
                                                        
                            try
                            {
                                int page = Convert.ToInt32(jsonBetsapiResp["pager"]["page"].ToString());
                                int per_page = Convert.ToInt32(jsonBetsapiResp["pager"]["per_page"].ToString());
                                int total = Convert.ToInt32(jsonBetsapiResp["pager"]["total"].ToString());

                                if (page * per_page > total)
                                    break;
                            }
                            catch { }

                            bRefreshed = true;
                        }
                    }
                    catch { }
                }

                if (bRefreshed)
                {
                    try
                    {
                        Monitor.Enter(EventListLocker);
                        PrematchEventList.timeStamp = DateTime.Now;
                        PrematchEventList.listData[sport] = newEventList;
                    }
                    catch { }
                    finally {
                        Monitor.Exit(EventListLocker);
                    }
                }
            }
        }

        List<string> addedNameList = new List<string>();
        private void SaveNameToFile(BetburgerInfo info)
        {
            if (!addedNameList.Contains(info.sport + info.homeTeam + info.awayTeam))
            {
                try
                {
                    Monitor.Enter(EventListLocker);
                    File.AppendAllText(@"missedname.txt", "----------------------------" + Environment.NewLine);

                    File.AppendAllText(@"missedname.txt", $"sport: {info.sport} | home: {info.homeTeam} | away: {info.awayTeam}" + Environment.NewLine);
                    File.AppendAllText(@"missedname.txt", $"list count: {PrematchEventList.listData[info.sport.ToLower()].Count}" + Environment.NewLine);
                    foreach (var match in PrematchEventList.listData[info.sport.ToLower()])
                    {                     
                        File.AppendAllText(@"missedname.txt", $"home: {match.home.name} | away: {match.away.name}" + Environment.NewLine);
                    }
                }
                catch (Exception ex) { }
                finally
                {
                    Monitor.Exit(EventListLocker);
                }
                addedNameList.Add(info.sport + info.homeTeam + info.awayTeam);
            }
        }


        List<string> addedList = new List<string>();
        private void SaveToFile(BetburgerInfo info, string betsapiResponse, string reason, ParseBet_Bet365 parsebet = null)
        {
#if (SUREBETTEST)
            return;
#endif
            if (!addedList.Contains(info.outcome + info.homeTeam + info.awayTeam))
            {
                File.AppendAllText(@"missed.txt", "----------------------------" + Environment.NewLine);
                File.AppendAllText(@"missed.txt", "reason: " + reason + Environment.NewLine);
                File.AppendAllText(@"missed.txt", "sport: " + info.sport + Environment.NewLine);
                File.AppendAllText(@"missed.txt", "league: " + info.league + Environment.NewLine);
                File.AppendAllText(@"missed.txt", "home: " + info.homeTeam + Environment.NewLine);
                File.AppendAllText(@"missed.txt", "away: " + info.awayTeam + Environment.NewLine);                 
                File.AppendAllText(@"missed.txt", "outcome: " + info.outcome + Environment.NewLine);
                File.AppendAllText(@"missed.txt", "extra: " + info.extra + Environment.NewLine);
                File.AppendAllText(@"missed.txt", "odds: " + info.odds + Environment.NewLine);
                if (parsebet == null)
                    File.AppendAllText(@"missed.txt", "parsebet: none" + info.odds + Environment.NewLine);
                else
                    File.AppendAllText(@"missed.txt", $"parsebet: Tab: {parsebet.TabLabel} | Market: {parsebet.MarketLabel} | Table: {parsebet.TableHeader} | Row: {parsebet.RowHeader} | Col: {parsebet.ColHeader} | Part: {parsebet.ParticipantName}" + Environment.NewLine);
                File.AppendAllText(@"missed.txt", betsapiResponse + Environment.NewLine);
                addedList.Add(info.outcome + info.homeTeam + info.awayTeam);
            }
        }
    }
#endif
        }
