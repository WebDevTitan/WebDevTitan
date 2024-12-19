using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using HtmlAgilityPack;
using Newtonsoft.Json;

using BetburgerServer.Model;
using BetburgerServer.Constant;
using SeastoryServer;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Protocol;
using System.IO;
using BetburgerServer.Controller;
using BetburgerServer;


namespace ArbRegServer
{
    public enum JUAN_BOOKMAKER
    {
        BETFAIR,
        BETWAY,
        WINAMAX
    }
    public class BetburgerScraper
    {
        private HttpClient httpClient = null;
        private onWriteStatusEvent _onWriteStatus;
        private onJsonDirectoriesEvent _onJsonDirData;
        private onJsonFeedsEvent _onJsonArbData;
        private onJsonFeedsEvent _onJsonValueData;

        public List<string> paramList = new List<string>();
        List<JsonDirBetCombination> combines = new List<JsonDirBetCombination>();

        List<string> AlreadyProcessedIDList = new List<string>();

        public BetburgerScraper(onWriteStatusEvent onWriteStatus, onJsonDirectoriesEvent onJsonDirData, onJsonFeedsEvent onJsonArbData, onJsonFeedsEvent onJsonValueData)
        {
            _onWriteStatus = onWriteStatus;
            _onJsonDirData = onJsonDirData;
            _onJsonArbData = onJsonArbData;
            _onJsonValueData = onJsonValueData;

            if (httpClient == null)
                initHttpClient();
        }

        public BetburgerScraper()
        {
            if (httpClient == null)
                initHttpClient();
        }

        private void initHttpClient()
        {
            ServicePointManager.DefaultConnectionLimit = 2;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = GameConstants.container;
            httpClient = new HttpClient(handler);

            
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json, text/plain, */*");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("origin", "https://www.betburger.com");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("referer", "https://www.betburger.com/");
        }

        private string GetBBParam(string strParam, int index)
        {
            string[] strParams = strParam.Split('|');

            List<string> filtedParam = new List<string>();
            foreach (var param in strParams)
            {
                if (string.IsNullOrEmpty(param.Trim()))
                    continue;
                filtedParam.Add(param.Trim());
            }
            if (filtedParam.Count > 1)
                return filtedParam[index % 2];
            else if (filtedParam.Count == 1)
                return filtedParam[0];
            return "";
        }

        int nBBIndex = 0;
        public async Task scrape()
        {
            //_onWriteStatus("Scraping Started");
            try
            {
                if (cServerSettings.GetInstance().EnableSurebet_Pre || cServerSettings.GetInstance().EnableValuebet_Pre)
                {
                    //prematch
                    HttpResponseMessage responseMessageDirectories = httpClient.GetAsync(string.Format("https://api-{0}.betburger.com/api/v1/directories?access_token={1}&locale=en", "pr", cServerSettings.GetInstance().BBToken)).Result;
                    if (!responseMessageDirectories.IsSuccessStatusCode)
                    {
                        _onWriteStatus(getLogTitle() + "Cannot get directories!");
                        return;
                    }

                    string responseMessageDirectoriesString = responseMessageDirectories.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(responseMessageDirectoriesString))
                    {
                        _onWriteStatus(getLogTitle() + "Cannot get correct directories!");
                        return;
                    }

                    JsonDir directories = JsonConvert.DeserializeObject<JsonDir>(responseMessageDirectoriesString);
                    if (directories == null)
                    {
                        _onWriteStatus(getLogTitle() + "Cannot get correct directories!");
                        return;
                    }

                    List<JsonBookmaker> bookmakers = new List<JsonBookmaker>();
                    //send the directories 
                    _onJsonDirData(directories, bookmakers);
                }

                if (cServerSettings.GetInstance().EnableSurebet_Live || cServerSettings.GetInstance().EnableValuebet_Live)
                {
                    //Live Scrap --------------

                    HttpResponseMessage responseMessageDirectories = httpClient.GetAsync(string.Format("https://api-{0}.betburger.com/api/v1/directories?access_token={1}&locale=en", "lv", cServerSettings.GetInstance().BBToken)).Result;
                    if (!responseMessageDirectories.IsSuccessStatusCode)
                    {
                        _onWriteStatus(getLogTitle() + "Cannot get directories!");
                        return;
                    }

                    string responseMessageDirectoriesString = responseMessageDirectories.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(responseMessageDirectoriesString))
                    {
                        _onWriteStatus(getLogTitle() + "Cannot get correct directories!");
                        return;
                    }

                    JsonDir directories = JsonConvert.DeserializeObject<JsonDir>(responseMessageDirectoriesString);
                    if (directories == null)
                    {
                        _onWriteStatus(getLogTitle() + "Cannot get correct directories!");
                        return;
                    }

                    List<JsonBookmaker> bookmakers = new List<JsonBookmaker>();
                    //send the directories 
                    _onJsonDirData(directories, bookmakers);
                }                
                while (GameConstants.bRun)
                {
#if (BET365 || BETFAIR)
#if (BETFAIR)
                    foreach (JUAN_BOOKMAKER JuanBookie in Enum.GetValues(typeof(JUAN_BOOKMAKER)))
                    {

                        if (JuanBookie == JUAN_BOOKMAKER.BETFAIR)
                            cServerSettings.GetInstance().JuanLiveSoccerUrl = "https://4vpwk7dkw3.execute-api.eu-west-1.amazonaws.com/prod/betminator/model/live-soccer?token=KMdexWsDL8tYXYQ5JpEikVGYTkkkZNVC&bookie=betfair";                            
                        else if (JuanBookie == JUAN_BOOKMAKER.BETWAY)
                            cServerSettings.GetInstance().JuanLiveSoccerUrl = "https://4vpwk7dkw3.execute-api.eu-west-1.amazonaws.com/prod/betminator/model/live-soccer?token=KMdexWsDL8tYXYQ5JpEikVGYTkkkZNVC&bookie=betwaylive";
                        else if (JuanBookie == JUAN_BOOKMAKER.WINAMAX)
                            cServerSettings.GetInstance().JuanLiveSoccerUrl = "https://4vpwk7dkw3.execute-api.eu-west-1.amazonaws.com/prod/betminator/model/live-soccer?token=KMdexWsDL8tYXYQ5JpEikVGYTkkkZNVC&bookie=winalive";
#endif
                        if (!string.IsNullOrEmpty(cServerSettings.GetInstance().JuanLiveSoccerUrl))
                        {
                            try
                            {
                                _onWriteStatus(getLogTitle() + "Updating Joan Live Soccer picks...");

                                HttpClient httpClient = new HttpClient();
                                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.190 Safari/537.36");
                                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
                                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");



                                HttpResponseMessage responseMessageLogin = httpClient.GetAsync(cServerSettings.GetInstance().JuanLiveSoccerUrl).Result;
                                responseMessageLogin.EnsureSuccessStatusCode();
                                string responseMessage = responseMessageLogin.Content.ReadAsStringAsync().Result;
                                //string responseMessage = cServerSettings.GetInstance().JuanLiveSoccerUrl;
                                if (responseMessage != "{\"picks\":[]}")
                                {

                                    string bookmaker = Utils.Between(cServerSettings.GetInstance().JuanLiveSoccerUrl, "bookie=");
                                    _onWriteStatus($"-------------------{bookmaker}-----------------");
                                    _onWriteStatus(responseMessage);


                                    List<BetburgerInfo> picklist = new List<BetburgerInfo>();
                                    try
                                    {
                                        dynamic jsonResResp = JsonConvert.DeserializeObject<dynamic>(responseMessage);
                                        foreach (var objEvent in jsonResResp["picks"])
                                        {
                                            try
                                            {
                                                string[] teams = new string[2] { "---", "---" };
                                                try
                                                {
                                                    teams = objEvent["eventName"].ToString().Split(new string[] { " - ", " ⇄ " }, StringSplitOptions.None);
                                                }
                                                catch { }

                                                BetburgerInfo info = new BetburgerInfo();
                                                
                                                info.arbId = objEvent["id"].ToString();
                                                info.kind = PickKind.Type_7;

                                                info.opbookmaker = "Live";

                                                info.created = DateTime.Now.ToString();
                                                info.updated = DateTime.Now.ToString();
                                                info.started = DateTime.Now.ToString();
                                                info.homeTeam = teams[0].Trim();
                                                info.awayTeam = teams[1].Trim();

                                                info.eventTitle = "---";
                                                try
                                                {
                                                    info.eventTitle = objEvent["eventName"].ToString();
                                                }
                                                catch { }

                                                info.league = "---";
                                                try
                                                {
                                                    info.league = objEvent["leagueName"].ToString();
                                                }
                                                catch { }
                                                info.sport = "Soccer";
#if (BET365)
                                            info.bookmaker = "Bet365";
                                            string directlink = WebUtility.UrlDecode(objEvent["betslip"].ToString());

                                            string ID = Utils.Between(directlink, "ID=", "&");
                                            string OD = Utils.Between(directlink, "OD=", "&");
                                            string FI = Utils.Between(directlink, "FI=", "&");
                                            string HA = Utils.Between(directlink, "HA=", ";");

                                            info.direct_link = string.Format("{0}|{1}|{2}", ID, OD, FI);
                                            

                                            info.outcome = "---";
                                            if (HA.Length <= 6 && HA.Length > 0)
                                            {
                                                info.outcome = HA;    //handicap
                                            }

                                            try
                                            {
                                                info.odds = Utils.FractionToDouble(objEvent["currentOdds"].ToString());
                                            }
                                            catch { }

                                            //string betsapiparseLogResult = "";

                                            //BetsapiHelper.Instance.UpdateBet365SiteUrl("", ref info, out betsapiparseLogResult);
                                            //if (!string.IsNullOrEmpty(betsapiparseLogResult))
                                            //{
                                            //    _onWriteStatus($"Joan Live Pick betsapi parse error: {betsapiparseLogResult}");
                                            //}

                                            //if (info.bookmaker.ToLower() == "bet365" && !string.IsNullOrEmpty(info.extra) && (1.2 <= info.odds && info.odds <= 3))
                                            //{

                                            //    if (!AlreadyProcessedIDList.Contains(info.arbId))
                                            //    {
                                            //        _onWriteStatus(getLogTitle() + $"Send pick in Tg: {info.bookmaker} {info.league} {info.eventTitle} {info.extra} {info.odds}");
                                            //        AlreadyProcessedIDList.Add(info.arbId);
                                            //        string message = $"LIVE {info.bookmaker}" + Environment.NewLine + $"{info.league}" + Environment.NewLine + $"{info.eventTitle}" + Environment.NewLine + $"{info.extra}" + Environment.NewLine + "Odd: " + info.odds;
                                            //        Task.Run(() => sendLiveTelegramMsg(message));
                                            //    }
                                            //}
#elif (BETFAIR)
                                                if (JuanBookie == JUAN_BOOKMAKER.BETFAIR)
                                                    info.bookmaker = "bfsportsbook";
                                                else if (JuanBookie == JUAN_BOOKMAKER.BETWAY)
                                                    info.bookmaker = "betway";
                                                else if (JuanBookie == JUAN_BOOKMAKER.WINAMAX)
                                                    info.bookmaker = "winamax";

                                                info.direct_link = objEvent["betslip"].ToString();
                                                try
                                                {
                                                    info.odds = Utils.FractionToDouble(objEvent["currentOdds"].ToString());
                                                }
                                                catch { }
#endif

                                                picklist.Add(info);

                                            }
                                            catch (Exception ex)
                                            {
                                                Trace.WriteLine($"Exception2 {ex.Message} {ex.StackTrace} {responseMessage}");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _onWriteStatus($"Exception3 {ex.Message} {ex.StackTrace}");
                                    }

                                    _onWriteStatus($"Joan Live Pick count: {picklist.Count}");
                                    GameServer.GetInstance().processValuesInfo(picklist);


                                    //HttpClient httpClient1 = new HttpClient();
                                    //HttpResponseMessage betsapiLiveRes = httpClient1.GetAsync($"https://api.b365api.com/v1/bet365/inplay?token={cServerSettings.GetInstance().BetsapiToken}").Result;
                                    //betsapiLiveRes.EnsureSuccessStatusCode();
                                    //string betsapiResponse = betsapiLiveRes.Content.ReadAsStringAsync().Result;
                                    //dynamic jsonBetsapiResp = JsonConvert.DeserializeObject<dynamic>(betsapiResponse);
                                    //if (jsonBetsapiResp["success"].ToString() == "1")
                                    //{

                                    //    List<BetburgerInfo> picklist = new List<BetburgerInfo>();
                                    //    try
                                    //    {
                                    //        dynamic jsonResResp = JsonConvert.DeserializeObject<dynamic>(responseMessage);
                                    //        foreach (var objEvent in jsonResResp["picks"])
                                    //        {
                                    //            try
                                    //            {

                                    //                string directlink = objEvent["betslip"].ToString();
                                    //                string[] linkArray = directlink.Split('|');
                                    //                if (linkArray.Count() < 3)
                                    //                {
                                    //                    continue;
                                    //                }

                                    //                string[] teams = new string[2] { "---", "---" };
                                    //                try
                                    //                {
                                    //                    teams = objEvent["eventName"].ToString().Split(new string[] { " - " }, StringSplitOptions.None);
                                    //                }
                                    //                catch { }

                                    //                BetburgerInfo info = new BetburgerInfo();
                                    //                BetburgerInfo info1 = new BetburgerInfo();
                                    //                info.arbId = objEvent["id"].ToString();
                                    //                info.kind = PickKind.Type_7;
                                    //                info.bookmaker = "Bet365";
                                    //                info1.kind = PickKind.Type_7;
                                    //                info1.bookmaker = "Live";

                                    //                info.created = DateTime.Now.ToString();
                                    //                info.updated = DateTime.Now.ToString();
                                    //                info.started = DateTime.Now.ToString();
                                    //                info.homeTeam = teams[0].Trim();
                                    //                info.awayTeam = teams[1].Trim();

                                    //                info.eventTitle = "---";
                                    //                try
                                    //                {
                                    //                    info.eventTitle = objEvent["eventName"].ToString();
                                    //                }
                                    //                catch { }

                                    //                info.league = "---";
                                    //                try
                                    //                {
                                    //                    info.league = objEvent["leagueName"].ToString();
                                    //                }
                                    //                catch { }
                                    //                info.direct_link = string.Format("{0}|{1}|{2}", linkArray[0], linkArray[1], linkArray[2]);
                                    //                info.sport = "Soccer";

                                    //                info.outcome = "---";
                                    //                if (linkArray.Count() >= 4)
                                    //                {
                                    //                    info.outcome = "hd(" + linkArray[3] + ")";
                                    //                }

                                    //                try
                                    //                {
                                    //                    info.odds = Utils.FractionToDouble(linkArray[1]);
                                    //                }
                                    //                catch { }

                                    //                string sportId = "1";

                                    //                foreach (var objBetsapiEvent in jsonBetsapiResp["results"][0])
                                    //                {
                                    //                    try
                                    //                    {
                                    //                        if (objBetsapiEvent["type"].ToString() == "EV" && objBetsapiEvent["OI"].ToString() == linkArray[2])
                                    //                        {
                                    //                            info.eventUrl = $"/#/IP/EV15{objBetsapiEvent["C2"].ToString()}2C{sportId}";
                                    //                            break;
                                    //                        }
                                    //                    }
                                    //                    catch { }
                                    //                }

                                    //                picklist.Add(info);
                                    //                picklist.Add(info1);

                                    //            }
                                    //            catch (Exception ex)
                                    //            {
                                    //                _onWriteStatus($"Exception2 {ex.Message} {ex.StackTrace}");
                                    //            }
                                    //        }
                                    //    }
                                    //    catch (Exception ex)
                                    //    {
                                    //        _onWriteStatus($"Exception3 {ex.Message} {ex.StackTrace}");
                                    //    }

                                    //    GameServer.GetInstance().processValuesInfo(picklist);
                                    //}
                                }
                            }
                            catch (Exception ex)
                            {
                                _onWriteStatus($"Exception4 {ex.Message} {ex.StackTrace}");
                            }
                        }

#if (BETFAIR)
                    }
#endif

                    if (!string.IsNullOrEmpty(cServerSettings.GetInstance().ParkHorseUrl))
                    {
                        try
                        {
                            _onWriteStatus(getLogTitle() + "Updating Park Horse picks...");

                            HttpClient httpClient = new HttpClient();
                            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.190 Safari/537.36");
                            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
                            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");



                            HttpResponseMessage responseMessageLogin = httpClient.GetAsync(cServerSettings.GetInstance().ParkHorseUrl).Result;
                            responseMessageLogin.EnsureSuccessStatusCode();
                            string responseMessage = responseMessageLogin.Content.ReadAsStringAsync().Result;

                            List<BetburgerInfo> picklist = new List<BetburgerInfo>();
                            try
                            {
                                dynamic jsonResResp = JsonConvert.DeserializeObject<dynamic>(responseMessage);
                                foreach (var objEvent in jsonResResp["horselist"])
                                {
                                    try
                                    {
                                        BetburgerInfo info = new BetburgerInfo();
                                        
                                        info.arbId = objEvent["date"].ToString() + "_" +  objEvent["horseId"].ToString();

                                        info.kind = PickKind.Type_2;
                                                                               
                                        
                                        info.created = DateTime.Now.ToString();                                        
                                        info.updated = DateTime.Now.ToString();

                                        info.started = objEvent["date"].ToString();
                                        
                                        info.homeTeam = objEvent["runner"].ToString();
                                        info.awayTeam = "";
                                                                       

                                        info.league = "---";
                                        try
                                        {
                                            info.league = objEvent["league"].ToString();
                                        }
                                        catch { }

                                        info.sport = "Horse Racing";

                                        dynamic data = JsonConvert.DeserializeObject<dynamic>(objEvent["value"].ToString());

                                        try
                                        {
                                            info.bookmaker = data[0]["Bookie"].ToString();
                                        }
                                        catch {
                                            continue;
                                        }
                                        info.opbookmaker = "Unknown";
                                        info.eventTitle = info.league + "-" + info.homeTeam;
                                        info.outcome = "win";
                                        string EV = data[0]["EV"].ToString();
                                        double EV_Value = double.Parse(EV);
                                        if (EV_Value < 100)
                                            continue;
                                        string Term = data[0]["Term"].ToString();
                                        string Place = data[0]["Place"].ToString();
                                        info.odds = Convert.ToDouble(data[0]["Odd"].ToString());

                                        info.direct_link = string.Format("{0}|{1}|{2}", EV, Term, Place);

                                        picklist.Add(info);
                                        
                                        if (EV_Value >= 100)
                                        {
                                            if (!AlreadyProcessedIDList.Contains(info.arbId))
                                            {
                                                //_onWriteStatus(getLogTitle() + $"Send pick(Horse) in Tg: {info.bookmaker} {info.league} {info.homeTeam} {info.started} {info.odds}");
                                                AlreadyProcessedIDList.Add(info.arbId);
                                                string message = $"{info.bookmaker} {info.sport} ({EV})" + Environment.NewLine + $"{info.league}" + Environment.NewLine + $"{info.homeTeam}" + Environment.NewLine + $"{info.started}" + Environment.NewLine + "Odd: " + info.odds;

                                                Task.Run(() => sendHorseTelegramMsg(message));
                                            }
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        Trace.WriteLine($"Exception2 {ex.Message} {ex.StackTrace} {responseMessage}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _onWriteStatus($"Exception3 {ex.Message} {ex.StackTrace}");
                            }

                            if (picklist.Count > 0)
                            {
                                _onWriteStatus($"Park Horse Pick count: {picklist.Count}");
                                GameServer.GetInstance().processValuesInfo(picklist);
                            }
                        }
                        catch { }
                    }

#endif


                    List<JsonArb> arb_list = new List<JsonArb>();

                    if (cServerSettings.GetInstance().EnableSurebet_Pre)
                    {
                        _onWriteStatus(getLogTitle() + "Updating the Surebet Prematch Feeds...");
                        JsonArb preArb = new JsonArb();
                        getSurebetInfoBySubscription(ref preArb, "arbs", false);
                        preArb.isLive = false;
                        if (preArb.arbs != null && preArb.arbs.Count > 0)
                            arb_list.Add(preArb);
                    }

                    if (cServerSettings.GetInstance().EnableSurebet_Live)
                    {
                        _onWriteStatus(getLogTitle() + "Updating the Surebet Live Feeds...");
                        JsonArb liveArb = new JsonArb();
                        getSurebetInfoBySubscription(ref liveArb, "arbs", true);
                        liveArb.isLive = true;
                        if (liveArb.arbs != null && liveArb.arbs.Count > 0)
                            arb_list.Add(liveArb);
                    }
                    
                    //Trace.WriteLine($"surebet count: {arb_list.Count} combine count: {combines.Count}");
                    if (arb_list.Count > 0)
                        _onJsonArbData(arb_list, combines);

                    arb_list.Clear();
                    if (cServerSettings.GetInstance().EnableValuebet_Live)
                    {
                        _onWriteStatus(getLogTitle() + "Updating the Valuebet Live Feeds...");
                        JsonArb preArb = new JsonArb();
                        getSurebetInfoBySubscription(ref preArb, "valuebets", true);
                        //getValuebetInfo(ref preArb, true);
                        preArb.isLive = true;
                        if (preArb.arbs != null && preArb.arbs.Count > 0)
                            arb_list.Add(preArb);
                    }

                    if (cServerSettings.GetInstance().EnableValuebet_Pre)
                    {
                        _onWriteStatus(getLogTitle() + "Updating the Valuebet Prematch Feeds...");
                        JsonArb preArb = new JsonArb();
                        getSurebetInfoBySubscription(ref preArb, "valuebets", false);
                        //getValuebetInfo(ref preArb, false);
                        preArb.isLive = false;
                        if (preArb.arbs != null && preArb.arbs.Count > 0)
                            arb_list.Add(preArb);
                    }
                    //Trace.WriteLine($"aa valuebet count: {arb_list.Count} combine count: {combines.Count}");
                    if (arb_list.Count > 0)
                        _onJsonValueData(arb_list, combines);

                    if (cServerSettings.GetInstance().BBIsAPIToken)
                    {
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                    
                }
            }
            catch (Exception e)
            {

            }
        }

        private int sendHorseTelegramMsg(string text)
        {
            #if (!FORSALE)
            //return 0;
            if (string.IsNullOrEmpty(text))
                return 0;

            try
            {
                return 1;
            }
            catch (Exception ex)
            {

            }
#endif
            return 0;
        }
        private void getSurebetInfoBySubscription(ref JsonArb arbInfo, string mode, bool bIsLive)
        {
            try
            {
                IList<KeyValuePair<string, string>> contents = new List<KeyValuePair<string, string>>();

                contents.Add(new KeyValuePair<string, string>("access_token", cServerSettings.GetInstance().BBToken));
                //contents.Add(new KeyValuePair<string, string>("auto_update", "true"));
                //contents.Add(new KeyValuePair<string, string>("notification_sound", "false"));
                //contents.Add(new KeyValuePair<string, string>("notification_popup", "false"));
                contents.Add(new KeyValuePair<string, string>("show_event_arbs", "true"));

                contents.Add(new KeyValuePair<string, string>("grouped", "true"));

                contents.Add(new KeyValuePair<string, string>("per_page", "30"));
                //contents.Add(new KeyValuePair<string, string>("sort_by", "percent"));
                contents.Add(new KeyValuePair<string, string>("sort_by", "percent"));

                //for (int i = 1; i < 9; i++)
                //    contents.Add(new KeyValuePair<string, string>("event_arb_types[]", $"{i}"));

                string filterStr = "";
                if (mode == "valuebets")
                {
                    if (bIsLive)
                        filterStr = cServerSettings.GetInstance().BBFilterValuebetLv;
                    else
                        filterStr = cServerSettings.GetInstance().BBFilterValuebetPr;
                }
                else if (mode == "arbs")
                {
                    if (bIsLive)
                        filterStr = cServerSettings.GetInstance().BBFilterSurebetLv;
                    else
                        filterStr = cServerSettings.GetInstance().BBFilterSurebetPr;
                }
                string[] filters = filterStr.Split(',');
                
                int nCount = 0;
                foreach (var filter in filters)
                {
                    if (string.IsNullOrEmpty(filter.Trim()))
                        continue;
                    contents.Add(new KeyValuePair<string, string>("search_filter[]", filter.Trim()));
                    nCount++;
                }

                if (nCount <= 0)
                    return;

                string type = "";
                if (bIsLive)
                {
                    contents.Add(new KeyValuePair<string, string>("is_live", $"true"));
                    type = "lv";
                }
                else
                {
                    contents.Add(new KeyValuePair<string, string>("is_live", $"false"));
                    type = "pr";
                }

                HttpResponseMessage responseMessageArbs = null;


                if (cServerSettings.GetInstance().BBIsAPIToken)
                {
                    responseMessageArbs = httpClient.PostAsync($"https://rest-api-{type}.betburger.com/api/v1/{mode}/bot_pro_search?access_token={cServerSettings.GetInstance().BBToken}&locale=en", new FormUrlEncodedContent(contents)).Result;
                }
                else
                {
                    httpClient.DefaultRequestHeaders.Referrer = new Uri("https://rest-api-lv.betburger.com/webjars/swagger-ui/index.html?configUrl=/v3/api-docs/swagger-config");
                    responseMessageArbs = httpClient.PostAsync($"https://rest-api-{type}.betburger.com/api/v1/{mode}/bot_search", new FormUrlEncodedContent(contents)).Result;
                }
                responseMessageArbs.EnsureSuccessStatusCode();
                
                string responseMessageArbsString = responseMessageArbs.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(responseMessageArbsString))
                {
                    _onWriteStatus(getLogTitle() + "Cannot get Arbs!");
                    return;
                }

                arbInfo = JsonConvert.DeserializeObject<JsonArb>(responseMessageArbsString);
                if (arbInfo == null)
                {
                    _onWriteStatus(getLogTitle() + "Cannot get correct Arbs!");
                    return;
                }

                string parms = string.Empty;

                List<string> addedinList = new List<string>();
                foreach (JsonArbBet bets in arbInfo.bets)
                {
                    if (!paramList.Contains(bets.bc_id) && !addedinList.Contains(bets.bc_id))
                    {
                        addedinList.Add(bets.bc_id);
                        parms += bets.bc_id;
                        parms += ",";
                    }
                }

                if (!string.IsNullOrEmpty(parms))
                {
                    parms = parms.Substring(0, parms.Length - 1);

                    string combinations_Url = string.Format("https://api-pr.betburger.com/api/v1/bet_combinations/" + parms + "?access_token=", cServerSettings.GetInstance().BBToken + "&locale=en");
                    HttpResponseMessage combinationsRespMessage = httpClient.GetAsync(combinations_Url).Result;
                    combinationsRespMessage.EnsureSuccessStatusCode();

                    string combinationsResp_Str = combinationsRespMessage.Content.ReadAsStringAsync().Result;
                    JObject obj = JsonConvert.DeserializeObject<JObject>(combinationsResp_Str);
                    List<JsonDirBetCombination> resultcombines = JsonConvert.DeserializeObject<List<JsonDirBetCombination>>(obj["bet_combinations"].ToString());

                    foreach (var itr in resultcombines)
                    {
                        if (!paramList.Contains(itr.id))
                            paramList.Add(itr.id);
                    }
                    combines.AddRange(resultcombines);
                }
            }
            catch (Exception e)
            {

            }
        }

        private void getValuebetInfo(ref JsonArb arbInfo, bool bIsLive)
        {
            try
            {
                IList<KeyValuePair<string, string>> contents = new List<KeyValuePair<string, string>>();
                contents.Add(new KeyValuePair<string, string>("access_token", cServerSettings.GetInstance().BBToken));
                contents.Add(new KeyValuePair<string, string>("show_event_arbs", "true"));
                //contents.Add(new KeyValuePair<string, string>("grouped", "true"));
                contents.Add(new KeyValuePair<string, string>("per_page", "50"));
                contents.Add(new KeyValuePair<string, string>("sort_by", "true"));

                string filterStr = "";
                if (bIsLive)
                    filterStr = cServerSettings.GetInstance().BBFilterValuebetLv;
                else
                    filterStr = cServerSettings.GetInstance().BBFilterValuebetPr;
                string[] filters = filterStr.Split(',');

                int nCount = 0;
                foreach (var filter in filters)
                {
                    if (string.IsNullOrEmpty(filter.Trim()))
                        continue;
                    contents.Add(new KeyValuePair<string, string>("search_filter[]", filter.Trim()));
                    nCount++;
                }

                if (nCount <= 0)
                    return;

                string type = "";
                if (bIsLive)
                    type = "lv";
                else
                    type = "pr";
                
                HttpResponseMessage responseMessageArbs = httpClient.PostAsync($"https://rest-api-{type}.betburger.com/api/v1/valuebets/bot_pro_search", (HttpContent)new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)contents)).Result;
                responseMessageArbs.EnsureSuccessStatusCode();

                string responseMessageArbsString = responseMessageArbs.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(responseMessageArbsString))
                {
                    _onWriteStatus(getLogTitle() + "Cannot get Arbs!");
                    return;
                }

                arbInfo = JsonConvert.DeserializeObject<JsonArb>(responseMessageArbsString);
                if (arbInfo == null)
                {
                    _onWriteStatus(getLogTitle() + "Cannot get correct Arbs!");
                    return;
                }
               // _onWriteStatus($"arb count {arbInfo.bets.Count}");

                string parms = string.Empty;

                List<string> addedinList = new List<string>();
                foreach (JsonArbBet bets in arbInfo.bets)
                {
                    if (!paramList.Contains(bets.bc_id) && !addedinList.Contains(bets.bc_id))
                    {
                        addedinList.Add(bets.bc_id);
                        parms += bets.bc_id;
                        parms += ",";
                    }
                }

                if (!string.IsNullOrEmpty(parms))
                {
                    parms = parms.Substring(0, parms.Length - 1);

                    string combinations_Url = string.Format("https://api-pr.betburger.com/api/v1/bet_combinations/" + parms + "?access_token=", cServerSettings.GetInstance().BBToken);
                    HttpResponseMessage combinationsRespMessage = httpClient.GetAsync(combinations_Url).Result;
                    combinationsRespMessage.EnsureSuccessStatusCode();

                    string combinationsResp_Str = combinationsRespMessage.Content.ReadAsStringAsync().Result;
                    JObject obj = JsonConvert.DeserializeObject<JObject>(combinationsResp_Str);
                    List<JsonDirBetCombination> resultcombines = JsonConvert.DeserializeObject<List<JsonDirBetCombination>>(obj["bet_combinations"].ToString());

                    foreach (var itr in resultcombines)
                    {
                        if (!paramList.Contains(itr.id))
                            paramList.Add(itr.id);
                    }
                    combines.AddRange(resultcombines);
                }
            }
            catch (Exception e)
            {

            }
        }

        private string getLogTitle()
        {
            return "[Betburger]";
        }
    }
}
