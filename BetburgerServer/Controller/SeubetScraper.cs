using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using BetburgerServer.Constant;
using System.Reflection;
using SeastoryServer;
using Newtonsoft.Json;
using Protocol;
using System.Diagnostics;
using System.Threading;
using BetburgerServer.Controller;
using Telegram.Bot;
using Seubet.Controller;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Debugger;
using Project.Interfaces;
using System.Timers;
using Timer = System.Timers.Timer;
using BetburgerServer;
using SocketIOClient.Transport.Http;
using MySqlX.XDevAPI.Common;
using Google.Protobuf.WellKnownTypes;

namespace BetComparerServer.Controller

{
    public class SeubetScraper
    {

        public string ApiBaseUrl = "https://api.pinnacle.com/v1/";
        public string username = "MG1807553";
        public string password = "Pe040590@#";
        public int sportId = 29;
        public string leagueIdsArray;
        JArray oddsArray = new JArray();
        JArray oddsPin = new JArray();
        JArray oddsSeu = new JArray();
        private static JArray differenceOdd = new JArray();
        private onWriteStatusEvent _onWriteStatus;
        private HttpClient httpClient = null;
        private CookieContainer coockieContainer = null;
        protected WebSocket _webSocket = null;
        private string configuration;
        List<string> AlreadyProcessedIDList = new List<string>();
        public TelegramCtrler telegramCtrl = null;
        private Timer checkConditionTimer;
        private Thread _pinScanThread;
        private static SurebetScraper c_Instance = null;


        private void initHttpClient()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(ApiBaseUrl);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));
        }
        public SeubetScraper(onWriteStatusEvent onWriteStatus)
        {
            _onWriteStatus = onWriteStatus;
            initHttpClient();
            telegramCtrl = new TelegramCtrler(onWriteStatus, "704271441:AAG-Z5oqj36ERnEPyKz2ODr5yyQZhy6DlZw");

            ServicePointManager.DefaultConnectionLimit = 2;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }
        //get odds and market Id of Pinnacle

        public void WebSocketConnect()
        {
            try
            {

                _webSocket = new WebSocket("wss://eu-swarm-springre.deimosphobos.net/");

                _webSocket.Origin = "https://www.seubet.com/";
                _webSocket.OnOpen += openSeubetSoc;
                _webSocket.OnMessage += getRid;
                try
                {
                    _webSocket.Connect();
                }
                catch (WebSocketException wsEx)
                {
                    _onWriteStatus($"WebSocketException: {wsEx.Message}");
                    Task.Delay(5000).ContinueWith(_ => WebSocketConnect()); // Retry after 5 seconds
                }
                catch (Exception ex)
                {
                    //_onWriteStatus($"Exception in Connect: {ex.Message}");
                    Task.Delay(5000).ContinueWith(_ => WebSocketConnect()); // Retry after 5 seconds
                }
                try
                {
                    if (_pinScanThread != null) _pinScanThread.Abort();
                }
                catch
                {

                }
                _pinScanThread = new Thread(threadScanFunc);
                _pinScanThread.Start();

                _webSocket.OnOpen += Socket_OnOpen;
                _webSocket.OnClose += Socket_OnClose;
                _webSocket.OnError += Socket_OnError;
                _webSocket.OnMessage += Socket_OnMessage;
            }
            catch (Exception ex)
            {

            }
        }

        public void threadScanFunc()
        {
            while (true)
            {
                try
                {
                    getPinOddsArray();
                    getFinalPinOdds();
                    matchOdds();
                }
                catch
                {
                }
                Thread.Sleep(2000);
            }
        }             
      


        public void getFinalPinOdds()
        {
                HttpResponseMessage response = httpClient.GetAsync($"fixtures?sportId=29").Result;
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    JObject fixturesJson = JObject.Parse(responseBody);
                    JArray pinOddsArray = new JArray();
                    JObject transformedData = new JObject();

                    foreach (var league in fixturesJson["league"])
                    {
                        foreach (var eventObj in league["events"])
                        {
                            var eventId = eventObj["id"];
                            var moneylines = oddsArray
                                .Where(o => (string)o["event_id"] == eventId.ToString())
                                .Select(o => o["moneyline"])
                                 .FirstOrDefault();

                            if (moneylines != null)
                            {
                                DateTimeOffset dateTimeOffset = DateTimeOffset.Parse(eventObj["starts"].ToString());
                                transformedData["league"] = league["name"];
                                //transformedData["event"] = eventId;
                                transformedData["start"] = dateTimeOffset.ToUnixTimeSeconds();
                                //transformedData["starts"] = eventObj["starts"];
                                transformedData["home"] = eventObj["home"];
                                transformedData["away"] = eventObj["away"];
                                transformedData["moneyline"] = moneylines;

                                pinOddsArray.Add(transformedData);
                            }
                        }
                    }
                    if (pinOddsArray.Count > 0)
                    {
                        oddsPin = pinOddsArray;
                    } else { _onWriteStatus($"There is no live market."); }
                        
                } 
            
            
                
        }
        public void getPinOddsArray()
        {
            if (cServerSettings.GetInstance().EnableSeubet_Live == true && cServerSettings.GetInstance().EnableSeubet_Prematch == false)
            {
                HttpResponseMessage response = httpClient.GetAsync($"odds?sportId=29&oddsFormat=Decimal&isLive=true").Result;
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    JObject oddsJson = JObject.Parse(responseBody);
                    JObject transformedData = new JObject();

                    foreach (var league in oddsJson["leagues"])
                    {
                        foreach (var eventObj in league["events"])
                        {
                            var firstPeriod = eventObj["periods"].FirstOrDefault();
                            if (firstPeriod == null)
                                continue;

                            var moneyline = firstPeriod["moneyline"];
                            if (moneyline == null)
                                continue;

                            transformedData["event_id"] = eventObj["id"];
                            transformedData["moneyline"] = new JObject
                            {
                                ["home"] = moneyline["home"],
                                ["away"] = moneyline["away"],
                                ["draw"] = moneyline["draw"]
                            };
                            oddsArray.Add(transformedData);
                        }
                    }
                }
                else
                {
                    _onWriteStatus($"There is no live market."); 
                }
            }
            else
            {
                HttpResponseMessage response = httpClient.GetAsync($"odds?sportId=29&oddsFormat=Decimal").Result;
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    JObject oddsJson = JObject.Parse(responseBody);
                    JObject transformedData = new JObject();

                    foreach (var league in oddsJson["leagues"])
                    {
                        foreach (var eventObj in league["events"])
                        {
                            var firstPeriod = eventObj["periods"].FirstOrDefault();
                            if (firstPeriod == null)
                                continue;

                            var moneyline = firstPeriod["moneyline"];
                            if (moneyline == null)
                                continue;

                            transformedData["event_id"] = eventObj["id"];
                            transformedData["moneyline"] = new JObject
                            {
                                ["home"] = moneyline["home"],
                                ["away"] = moneyline["away"],
                                ["draw"] = moneyline["draw"]
                            };
                            oddsArray.Add(transformedData);
                        }
                    }
                }
            }
                

        }

        private void getRid(object sender, MessageEventArgs e)
        {
            try
            {
                JObject jsonData = JObject.Parse(e.Data);
                if ((string)jsonData["rid"] != "0" && e.Data.Contains("GameListSubscribeCmd"))
                {
                    getSeuOdds(jsonData);
                }
            }
            catch
            {

            }
        }

        private void openSeubetSoc(object sender, EventArgs e)
        {
            JObject jLoginPayload = new JObject();
            jLoginPayload["command"] = "request_session";
            jLoginPayload["params"] = new JObject();
            jLoginPayload["params"]["afec"] = "LzdN_7hSSYEK1_vayOupPXCls7zslsNIkqNA";
            jLoginPayload["params"]["is_wrap_app"] = false;
            jLoginPayload["params"]["language"] = "eng";
            jLoginPayload["params"]["site_id"] = "18749911";
            jLoginPayload["params"]["source"] = "42";
            jLoginPayload["rid"] = GenerateRid("request_session");
            _webSocket.Send(jLoginPayload.ToString());

            jLoginPayload = new JObject();
            jLoginPayload["command"] = "get";
            jLoginPayload["params"] = new JObject();
            jLoginPayload["params"]["source"] = "betting";
            jLoginPayload["params"]["what"] = new JObject();
            jLoginPayload["params"]["what"]["sport"] = new JArray { "name", "alias" };
            jLoginPayload["params"]["what"]["region"] = new JArray { "name" };
            jLoginPayload["params"]["what"]["competition"] = new JArray { "id", "name", "order" };
            jLoginPayload["params"]["what"]["game"] = new JArray { new JArray { "id", "team1_name", "team2_name", "start_ts" } };
            jLoginPayload["params"]["what"]["market"] = new JArray { "name", "order" };
            jLoginPayload["params"]["what"]["event"] = new JArray { "id", "name", "price", "type_1" };
            jLoginPayload["params"]["where"] = new JObject();
            jLoginPayload["params"]["where"]["market"] = new JObject();
            jLoginPayload["params"]["where"]["market"]["type"] = "P1XP2";
            jLoginPayload["params"]["where"]["sport"] = new JObject();
            jLoginPayload["params"]["where"]["sport"]["alias"] = "Soccer";
            jLoginPayload["params"]["where"]["sport"]["type"] = new JObject();
            jLoginPayload["params"]["where"]["sport"]["type"]["@in"] = new JArray { 0, 2, 5 };
            jLoginPayload["params"]["subscribe"] = true;
            jLoginPayload["rid"] = GenerateRid("GameListSubscribeCmd");
            _webSocket.Send(jLoginPayload.ToString());
        }
        public string GenerateRid(string command)
        {
            // Function to generate a random 15-digit number
            string GenerateRandom15DigitNumber()
            {
                Random random = new Random();
                string result = string.Empty;
                for (int i = 0; i < 15; i++)
                {
                    result += random.Next(0, 10).ToString();
                }
                return result;
            }

            // Generate the rid
            string rand = GenerateRandom15DigitNumber();
            return $"{command}{rand}";
        }
        //get Seubet odds
        public void getSeuOdds(JObject jsonData)
        {
            JArray oddsArray = new JArray();
            JObject transformedData = new JObject();

            foreach (var sport in jsonData["data"]["data"]["sport"])
            {
                foreach (var region in sport.First["region"])
                {
                    foreach (var competition in region.First["competition"])
                    {
                        foreach (var game in competition.First["game"])
                        {
                            var gameData = game.First;
                            var leagueName = region.First["name"] + " - " + competition.First["name"];

                            transformedData["league"] = leagueName;
                            transformedData["start"] = (int)gameData["start_ts"] + 28800;
                            transformedData["home"] = gameData["team1_name"];
                            transformedData["away"] = gameData["team2_name"];

                            JObject moneyline = new JObject();
                            JObject eventId = new JObject();
                            foreach (var market in gameData["market"])
                            {
                                foreach (var eventItem in market.First["event"])
                                {
                                    switch (eventItem.First["type_1"].ToString().ToLower())
                                    {
                                        case "w1":
                                            moneyline["home"] = eventItem.First["price"];
                                            eventId["home"] = eventItem.First["id"];
                                            break;
                                        case "w2":
                                            moneyline["away"] = eventItem.First["price"];
                                            eventId["away"] = eventItem.First["id"];
                                            break;
                                        case "x":
                                            moneyline["draw"] = eventItem.First["price"];
                                            eventId["draw"] = eventItem.First["id"];
                                            break;
                                    }
                                }
                            }
                            transformedData["moneyline"] = moneyline;
                            transformedData["event"] = eventId;
                            if (moneyline.Count != 0)
                            {
                                oddsArray.Add(transformedData);
                            }
                        }
                    }
                }
            }

            oddsSeu = oddsArray;
        }

        //match market and compare odds between pinnacle and seubet
        public void matchOdds()
        {
            int cntPin = oddsPin.Count;
            int cntSeu = oddsSeu.Count;
            JArray matches = new JArray();

            for (int i = 0; i < cntPin; i++)
            {
                for (int j = 0; j < cntSeu; j++)
                {
                    var pinObj = (JObject)oddsPin[i];
                    var seuObj = (JObject)oddsSeu[j];
                    //if()
                    if (pinObj == null || seuObj == null) continue;
                    double leagueProximity = JaroWinklerDistance.proximity(pinObj["league"].ToString(), seuObj["league"].ToString());
                    double startProximity = JaroWinklerDistance.proximity(pinObj["start"].ToString(), seuObj["start"].ToString());
                    double homeProximity = JaroWinklerDistance.proximity(pinObj["home"].ToString(), seuObj["home"].ToString());
                    double awayProximity = JaroWinklerDistance.proximity(pinObj["away"].ToString(), seuObj["away"].ToString());

                    if ((homeProximity + awayProximity) / 2 >= 0.85 && leagueProximity > 0.6)
                    {
                        foreach (var item in seuObj["moneyline"].Children<JProperty>())
                        {
                            var key = item.Name;                            
                            var valueSeu = seuObj["moneyline"][key].Type != JTokenType.Null ? (double?)seuObj["moneyline"][key] : null;
                            var valuePin = pinObj["moneyline"][key].Type != JTokenType.Null ? (double?)pinObj["moneyline"][key] : null;
                            var percentValue = (valueSeu - valuePin) / valuePin * 100;                            
                            if (valueSeu.HasValue && valuePin.HasValue && valueSeu > valuePin && percentValue > Convert.ToDouble(cServerSettings.GetInstance().Percent_Price))
                            {                                
                                JObject result = new JObject();
                                result["sport"] = "Soccer";
                                result["bookmaker"] = "Seubet";
                                result["league"] = seuObj["league"];
                                result["marketId"] = seuObj["eventId"];
                                result["homeTeam"] = seuObj["home"];
                                result["awayTeam"] = seuObj["away"];
                                result["startTime"] = seuObj["start"];
                                result["eventId"] = seuObj["event"][key];
                                result["market"] = "moneyline";
                                result["runner"] = key;
                                result["price"] = valueSeu.Value;
                                result["fairPrice"] = valuePin.Value;
                                result["percent"] = percentValue;
                                matches.Add(result);
                            }
                        }
                    }
                }
            }

            differenceOdd = new JArray(matches);

            if (differenceOdd.Count > 0)
            {
                List<BetburgerInfo> betburgerInfoPair = new List<BetburgerInfo>();
                betburgerInfoPair = GetBetInfo(differenceOdd);
                _onWriteStatus(getLogTitle() + "SB pick count: " + betburgerInfoPair.Count);
                if (betburgerInfoPair.Count > 0)
                    GameServer.GetInstance().processValuesInfo(betburgerInfoPair);
            }
            else
            {
                _onWriteStatus(getLogTitle() + "There is no matched market.");
            }

            //Main();
            //if (differenceOdd.Count > 0)
            //{

            //    List<BetburgerInfo> betburgerInfoPair = GetBetInfo(differenceOdd);

            //    _onWriteStatus(getLogTitle() + "SB pick count: " + betburgerInfoPair.Count);

            //    // Send in batches of 10
            //    int batchSize = 10;
            //    for (int i = 0; i < betburgerInfoPair.Count; i += batchSize)
            //    {
            //        var batch = betburgerInfoPair.Skip(i).Take(batchSize).ToList();
            //        if (batch.Count > 0)
            //        {
            //            GameServer.GetInstance().processValuesInfo(batch);
            //        }
            //    }
            //}

        }
            

        

        public void Main()
        {
            SetTimer();
        }

        private void SetTimer()
        {
            checkConditionTimer = new Timer(5000);

            checkConditionTimer.Elapsed += CheckConditionAndTeleConnect;
            checkConditionTimer.AutoReset = true;
            checkConditionTimer.Enabled = true;
        }

        private void CheckConditionAndTeleConnect(object sourse, EventArgs e)
        {
            if (ConditionIsMet())
            {
                TeleConnect();
            }
        }

        private bool ConditionIsMet()
        {
            return differenceOdd.Count > 0;
        }
        internal void TeleConnect()
        {
            int cntdifferenceOdd = differenceOdd.Count();
            if (differenceOdd.Count > 0)
            {
                for (int i = 0; i < cntdifferenceOdd; i++)
                {
                    telegramCtrl.sendMessage(differenceOdd[i].ToString());
                }
            }
        }

        //set prematch telegram message
        private int sendPrematchTelegramMsg(string text)
        {
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
            return 0;
        }

        //make seuBet List that is betting

        List<BetburgerInfo> GetBetInfo(JArray differenceOdd)
        {

            List<BetburgerInfo> result = new List<BetburgerInfo>();

            for (int i = 0; i < differenceOdd.Count; i++)
            {
                try
                {
                    BetburgerInfo info = new BetburgerInfo();
                    info.kind = PickKind.Type_14;
                    var differenceObj = differenceOdd[i];
                    info.bookmaker = differenceObj["bookmaker"].ToString();
                    info.sport = differenceObj["sport"].ToString();
                    info.league = differenceObj["league"].ToString();
                    info.homeTeam = differenceObj["homeTeam"].ToString();
                    info.awayTeam = differenceObj["awayTeam"].ToString();
                    info.eventTitle = string.Format("{0} - {1}", info.homeTeam, info.awayTeam);
                    info.odds = Convert.ToDouble(differenceObj["price"].ToString());
                    info.eventid = (long)differenceObj["eventId"];
                    info.outcome = "value";
                    info.isLive = true;
                    info.percent = Convert.ToDecimal(differenceObj["percent"].ToString());
                    //info.arbId = differenceObj["eventId"].ToString();

                    //_onWriteStatus(getLogTitle() + $"Send pick: {info.bookmaker} {info.league} {info.eventTitle} {info.odds}");
                    result.Add(info);
                    bool bCanSend = false;
                    if (info.bookmaker == "Seubet")
                    {
                        bCanSend = true;
                    }
                    if (bCanSend)
                    {

                        if (info.bookmaker == "Seubet")
                        {
                            string message = $"{info.bookmaker} {info.sport}" + Environment.NewLine + $"{info.league}" + Environment.NewLine + $"{info.eventTitle}" + Environment.NewLine + $"{info.outcome}" + Environment.NewLine + "Odd: " + info.odds;
                            Task.Run(() => sendPrematchTelegramMsg(message));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Exception {ex.StackTrace} {ex.Message}");
                }
            }
            return result;
        }
        private void Socket_OnOpen(object sender, EventArgs e)
        {
            _onWriteStatus(getLogTitle() + $"Websocket connected");

            _webSocket.Send(configuration);
        }

        protected virtual void Socket_OnClose(object sender, CloseEventArgs e)
        {
            _onWriteStatus(getLogTitle() + $"Websocket closed : {e.Reason}");

            WebSocketConnect();

        }
        protected virtual void Socket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            _onWriteStatus(getLogTitle() + $"Websocket error : {e.Message}");

            WebSocketConnect();
        }
        protected virtual void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {


            }
            catch (Exception ex)
            {

            }

        }
        internal async Task scrape()
        {
            WebSocketConnect();
        }
       
        public void CloseWebSocket()
        {
            if (_webSocket.ReadyState == WebSocketState.Open)
                _webSocket.Close();
        }
        private string getLogTitle()
        {
            return "[Seubet]";
        }

        
        
        internal void stopThread()
        {
            if (_pinScanThread != null)
                _pinScanThread.Abort();
        }
    }

}
