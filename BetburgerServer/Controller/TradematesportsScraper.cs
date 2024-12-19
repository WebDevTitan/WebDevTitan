using BetburgerServer.Constant;
using BetburgerServer.Model;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Protocol;
using SeastoryServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace BetburgerServer.Controller
{
    public class TrademateCtrl
    {
        private WebSocket _webSocket = null;
        public bool m_isLogged = false;
                
        protected onWriteStatusEvent m_handlerWriteStatus;        
        protected string m_domain = "https://app.tradematesports.com/tradefeed/";
        //protected CookieContainer m_cookieContainer;
        protected HttpClient m_httpClient = null;
        public List<JsonTrade> _TradeList = new List<JsonTrade>();
        public List<string> _PlacedTradeList = new List<string>();
        
        public TrademateCtrl(onWriteStatusEvent onWriteStatus)
        {
            m_handlerWriteStatus = onWriteStatus;
                        
            //m_cookieContainer = new CookieContainer();
            //ReadCookiesFromDisk();
            InitHttpClient();
        }

        private void Check_Heartbeat()
        {
            while (true)
            {
                if (_webSocket != null && _webSocket.ReadyState == WebSocketState.Open)
                {
                    _webSocket.Send("2");
                    Thread.Sleep(12000);
                }
                else
                    Thread.Sleep(2000);
            }
        }

        public void startListening()
        {
            while (GameConstants.bRun)
            {
                try
                {
                   
                    if (string.IsNullOrEmpty(GameConstants.TradematesportsPunterId))
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    
                    m_handlerWriteStatus($"Connecting trademate websocket");
                
                    HttpResponseMessage pollingResponseMessage = m_httpClient.GetAsync(string.Format("https://ws.tradematesports.com/socket.io/?EIO=3&transport=polling&t={0}", Utils.get_yeast())).Result;
                    pollingResponseMessage.EnsureSuccessStatusCode();

                    IEnumerable<string> cookies1;
                    string sessionId = string.Empty;
                    if (pollingResponseMessage.Headers.TryGetValues("set-cookie", out cookies1))
                    {
                        foreach (var cookie in cookies1)
                        {
                            if (cookie.Contains("io"))
                            {
                                sessionId = cookie.Replace("io=", string.Empty);
                                break;
                            }
                        }
                    }

                    HttpResponseMessage socketResponseMessage = m_httpClient.GetAsync(string.Format("https://ws.tradematesports.com/socket.io/?EIO=3&transport=polling&t={1}&sid={0}", sessionId, Utils.get_yeast())).Result;
                    socketResponseMessage.EnsureSuccessStatusCode();


                    string addSocketContent = string.Format("107:42[\"addSocket\",{{\"socketId\":\"{0}\",\"userId\":\"{1}\"}}]", sessionId, GameConstants.TradematesportsPunterId);

                    HttpResponseMessage addSocketResponseMessage = m_httpClient.PostAsync(string.Format("https://ws.tradematesports.com/socket.io/?EIO=3&transport=polling&t={1}&sid={0}", sessionId, Utils.get_yeast()), new StringContent(addSocketContent, Encoding.UTF8, "text/plain")).Result;
                    addSocketResponseMessage.EnsureSuccessStatusCode();

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    _webSocket = new WebSocket(string.Format("wss://ws.tradematesports.com/socket.io/?EIO=3&transport=websocket&sid={0}", sessionId));

                    //_webSocket.SetProxy("http://127.0.0.1:8888", "", "");
                    _webSocket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    _webSocket.Origin = "https://app.tradematesports.com";
                    _webSocket.OnOpen += Socket_OnOpen;
                    _webSocket.OnMessage += Socket_OnMessage;
                    _webSocket.OnClose += Socket_OnClose;
                    _webSocket.OnError += Socket_OnError;
                    _webSocket.Compression = CompressionMethod.None;
                    _webSocket.EmitOnPing = true;
                    _webSocket.Connect();
                    Task.Run(Check_Heartbeat);
                    while (true)
                    {
                        if (_webSocket != null && _webSocket.ReadyState != WebSocketState.Open)
                        {
                            Thread.Sleep(1000);
                            break;
                        }
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    m_handlerWriteStatus("[Trademate]Exception in startListening: " + ex.ToString());
                    Thread.Sleep(5000);
                }
                
            }
        }

        private void Socket_OnOpen(object sender, EventArgs e)
        {
            _webSocket.Send("2probe");
            m_handlerWriteStatus("[Trademate]Socket_OnOpen");
            /*string[] lines = File.ReadAllLines("messages.txt");
            foreach(string line in lines)
            {
                _webSocket.Send(line);
            }*/
        }

        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {

            try
            {
                if (e.Data.ToString() == "3probe")
                {
                    _webSocket.Send("5");
                    return;
                }
                //if (e.Data.ToString() == "3")
                //{
                //    _webSocket.Send("2");
                //    return;
                //}
                string strBody = e.Data.ToString().Substring(2);
                //m_handlerWriteStatus(e.Data.ToString());
                dynamic jsonBody = JsonConvert.DeserializeObject<dynamic>(strBody);
                string msgCode = jsonBody[0].ToString();
                string msgData = jsonBody[1].ToString();
                jsonBody = JsonConvert.DeserializeObject<dynamic>(msgData);
                if (msgCode == "addTrades")
                {
                    System.IO.File.AppendAllText(@"tm_picks.txt", DateTime.Now.ToString() + " addTrades" + Environment.NewLine);
                    lock (_TradeList)
                    {
                        _TradeList.Clear();
                        foreach (var item in ((JObject)jsonBody).Values())
                        {
                            JsonTrade tradeItem = JsonConvert.DeserializeObject<JsonTrade>(item.ToString());
                            _TradeList.Add(tradeItem);

                            
                            System.IO.File.AppendAllText(@"tm_picks.txt", item.ToString() + Environment.NewLine);
                            //Trace.WriteLine($"AddTrades {tradeItem.id} {tradeItem.leagueName} {tradeItem.homeTeam} {tradeItem.awayTeam} {tradeItem.startTime}");
                        }
                        //m_showTrade(_TradeList);
                    }
                }
                else if (msgCode == "addTrade")
                {
                    JsonTrade tradeItem = JsonConvert.DeserializeObject<JsonTrade>(msgData);
                    _TradeList.Add(tradeItem);

                    System.IO.File.AppendAllText(@"tm_picks.txt", DateTime.Now.ToString() + " addTrade" + Environment.NewLine);
                    System.IO.File.AppendAllText(@"tm_picks.txt", msgData + Environment.NewLine);

                    //Trace.WriteLine($"AddTrade {tradeItem.id} {tradeItem.leagueName} {tradeItem.homeTeam} {tradeItem.awayTeam} {tradeItem.startTime}");
                    //m_showTrade(_TradeList);
                }
                else if (msgCode == "changeTrade")
                {
                    System.IO.File.AppendAllText(@"tm_picks.txt", DateTime.Now.ToString() + " changeTrade" + Environment.NewLine);
                    System.IO.File.AppendAllText(@"tm_picks.txt", msgData + Environment.NewLine);

                    JsonTrade tradeItem = JsonConvert.DeserializeObject<JsonTrade>(msgData);
                    for (int i = 0; i < _TradeList.Count; i++)
                    {
                        if (_TradeList[i].id == tradeItem.id)
                        {
                            _TradeList[i] = tradeItem;

                            
                            //Trace.WriteLine($"ChangeTrade {tradeItem.id} ");
                        }
                    }
                    //m_showTrade(_TradeList);
                }
                else if (msgCode == "removeTrade")
                {
                    System.IO.File.AppendAllText(@"tm_picks.txt", DateTime.Now.ToString() + " removeTrade" + Environment.NewLine);
                    System.IO.File.AppendAllText(@"tm_picks.txt", msgData + Environment.NewLine);

                    int removalIndex = -1;
                    for (int i = 0; i < _TradeList.Count; i++)
                    {
                        if (_TradeList[i].id == msgData)
                        {
                            removalIndex = i;

                            Trace.WriteLine($"RemoveTrade {msgData}");
                        }
                    }
                    _TradeList.RemoveAt(removalIndex);
                    //m_showTrade(_TradeList);
                }
            }
            catch (Exception ex)
            {
                //m_handlerWriteStatus("Exception in socket message: " + ex.ToString());
            }
        }

        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            m_handlerWriteStatus("[Trademate]Socket_OnClose");
            m_handlerWriteStatus(e.Reason);
        }

        private void Socket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            m_handlerWriteStatus("[Trademate]Socket_OnError");
            m_handlerWriteStatus(e.Message.ToString());
        }

        public void CloseWebSocket()
        {

            if (_webSocket.ReadyState == WebSocketState.Open)
                _webSocket.Close();
        }

        public bool doLogin()
        {
            if (string.IsNullOrEmpty(cServerSettings.GetInstance().TradematesportsUsername))
                return false;
            try
            {
                dynamic jsonPayload = new JObject();
                StringContent jsonContent = null;
                HttpResponseMessage verifyResponse = null;
                if (!string.IsNullOrEmpty(cServerSettings.GetInstance().TradematesportsPunterId))
                {
                    string userTradeUrl = "https://api.tradematesports.com/v2/usertrades_by_page";
                    jsonPayload.open = true;
                    jsonPayload.page = 0;
                    jsonPayload.settled = true;
                    jsonPayload.userId = cServerSettings.GetInstance().TradematesportsPunterId;
                    jsonContent = new StringContent(jsonPayload.ToString(), Encoding.UTF8, "application/json");
                    verifyResponse = m_httpClient.PostAsync(userTradeUrl, jsonContent).Result;
                    if (verifyResponse.IsSuccessStatusCode)
                        return true;
                }


                string verifyEndpoint = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key=AIzaSyAD2wRm5EmnoCeiwsc-4UEb_xTcqIND3x8";
                jsonPayload = new JObject();
                jsonPayload.email = cServerSettings.GetInstance().TradematesportsUsername;
                jsonPayload.password = cServerSettings.GetInstance().TradematesportsPassword;
                jsonPayload.returnSecureToken = true;
                jsonContent = new StringContent(jsonPayload.ToString(), Encoding.UTF8, "application/json");
                verifyResponse = m_httpClient.PostAsync(verifyEndpoint, jsonContent).Result;
                verifyResponse.EnsureSuccessStatusCode();
                string strVerifyContent = verifyResponse.Content.ReadAsStringAsync().Result;
                dynamic jsonVerifyContent = JsonConvert.DeserializeObject<dynamic>(strVerifyContent);

                verifyEndpoint = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/getAccountInfo?key=AIzaSyAD2wRm5EmnoCeiwsc-4UEb_xTcqIND3x8";
                jsonContent = new StringContent("{\"idToken\":\"" + jsonVerifyContent.idToken.ToString() + "\"}",
                    Encoding.UTF8, "application/json");
                verifyResponse = m_httpClient.PostAsync(verifyEndpoint, jsonContent).Result;
                verifyResponse.EnsureSuccessStatusCode();
                strVerifyContent = verifyResponse.Content.ReadAsStringAsync().Result;
                jsonVerifyContent = JsonConvert.DeserializeObject<dynamic>(strVerifyContent);
                cServerSettings.GetInstance().TradematesportsPunterId = jsonVerifyContent.users[0].localId;
                cServerSettings.GetInstance().SaveSetting();
                try { m_httpClient.DefaultRequestHeaders.Remove(":authority"); } catch { }
                m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation(":authority", "api.tradematesports.com");
                verifyEndpoint = "https://api.tradematesports.com/v2/usertrades_by_page";
                jsonContent = new StringContent("{\"userId\":\"" + jsonVerifyContent.users[0].localId + "\",\"page\":0,\"open\":true,\"settled\":true}",
                    Encoding.UTF8, "application/json");
                verifyResponse = m_httpClient.PostAsync(verifyEndpoint, jsonContent).Result;
                verifyResponse.EnsureSuccessStatusCode();
                strVerifyContent = verifyResponse.Content.ReadAsStringAsync().Result;
                jsonVerifyContent = JsonConvert.DeserializeObject<dynamic>(strVerifyContent);
                //WriteCookiesToDisk(m_cookieContainer);
                return true;
            }
            catch (Exception ex)
            {
                m_handlerWriteStatus("[Trademate]Error in doLogin: " + ex.ToString());
            }
            return false;
        }

        public void removeTrade(JsonTrade tradeItem)
        {
            try
            {
                string msgCode = $"[\"removeTrade\", \"{tradeItem.id}\"]";
                _webSocket.Send(msgCode);
                msgCode = $"42[\"removeTrade\", \"{tradeItem.id}\"]";
                _webSocket.Send(msgCode);
            }
            catch
            {
            }
        }

        public void registerTrade(JsonTrade tradeItem)
        {
            try
            {
                if (_PlacedTradeList.Contains(tradeItem.id)) return;
                if (string.IsNullOrEmpty(GameConstants.TradematesportsPunterId))
                {
                    return;
                }
                //m_handlerWriteStatus(string.Format("Trade:{0} is registering", tradeItem.id));
                try { m_httpClient.DefaultRequestHeaders.Remove(":authority"); } catch { }
                m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation(":authority", "api.tradematesports.com");
                string verifyEndpoint = "https://api.tradematesports.com/v2/register_usertrade";
                dynamic postContent = new JObject();
                postContent.data = new JObject();
                postContent.data.match = new JObject();
                postContent.data.match.startTime = Int64.Parse(tradeItem.startTime);
                postContent.data.match.competition = tradeItem.leagueName;
                postContent.data.match.awayTeam = tradeItem.awayTeam;
                postContent.data.match.awayTeamId = Int64.Parse(tradeItem.awayTeamId);
                postContent.data.match.homeTeam = tradeItem.homeTeam;
                postContent.data.match.homeTeamId = Int64.Parse(tradeItem.homeTeamId);
                postContent.data.match._id = tradeItem.eventId;


                postContent.data.bookmaker = int.Parse(tradeItem.bookmaker);
                postContent.data.sharpBookmaker = 3000107;
                postContent.data.createdAt = (Int64)Math.Floor(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000);
                postContent.data.wager = Math.Round(3000 * tradeItem.kelly / 10);
                postContent.data.currency = "EUR";
                postContent.data.templateId = Int64.Parse(tradeItem.templateId);
                postContent.data.edge = tradeItem.edge;
                postContent.data.closing = tradeItem.edge;
                postContent.data.odds = tradeItem.odds;
                postContent.data.oddsType = tradeItem.oddsType;
                postContent.data.oddsTypeCondition = tradeItem.oddsTypeCondition;
                postContent.data.output = tradeItem.output;
                postContent.data.result = "-";
                postContent.data.status = 1;
                postContent.data.sport = tradeItem.sportId;
                postContent.data.user = GameConstants.TradematesportsPunterId;
                postContent.data.typeId = tradeItem.typeId;
                postContent.data.tradeId = tradeItem.id;
                postContent.data.eventPartId = tradeItem.eventPartId;
                postContent.data.participant = tradeItem.participant;
                postContent.data.deleted = false;
                postContent.data.customTrade = false;
                postContent.data.changeHistory = new JObject();
                postContent.data.timeToKickoff = null;

                StringContent jsonContent = new StringContent(postContent.ToString(), Encoding.UTF8, "application/json");
                HttpResponseMessage verifyResponse = m_httpClient.PostAsync(verifyEndpoint, jsonContent).Result;
                verifyResponse.EnsureSuccessStatusCode();
                string strVerifyContent = verifyResponse.Content.ReadAsStringAsync().Result;
                dynamic jsonVerifyContent = JsonConvert.DeserializeObject<dynamic>(strVerifyContent);
                //WriteCookiesToDisk(m_cookieContainer);
                _PlacedTradeList.Add(tradeItem.id);
                //m_handlerWriteStatus(string.Format("Trade:{0} is registered", tradeItem.id));
            }
            catch (Exception ex)
            {
                m_handlerWriteStatus("[Trademate]Error in registerTrade: " + ex.ToString());
            }
        }

        protected void InitHttpClient()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = Global.cookieContainer;
            //handler.CookieContainer = m_cookieContainer;
            m_httpClient = new HttpClient(handler);
            m_httpClient.Timeout = new TimeSpan(0, 0, 100);
            ServicePointManager.DefaultConnectionLimit = 2;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            m_httpClient.DefaultRequestHeaders.ExpectContinue = false;
            ChangeDefaultHeaders();
        }

        protected virtual void ChangeDefaultHeaders()
        {
            m_httpClient.DefaultRequestHeaders.Clear();
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36 Edg/118.0.2088.76");
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://app.tradematesports.com/");
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://app.tradematesports.com");
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"Chromium\";v=\"118\", \"Microsoft Edge\";v=\"118\", \"Not = A ? Brand\";v=\"99\", \"Microsoft Edge WebView2\";v=\"118\"");
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");

            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-site");
            
        }

        public void WriteCookiesToDisk(CookieContainer cookieJar)
        {
            using (Stream stream = System.IO.File.Create("tradematesports-cookie.bin"))
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, cookieJar);
                }
                catch (Exception e)
                {
                    m_handlerWriteStatus("[Trademate]Problem writing cookies to disk: " + e.GetType());
                }
            }
        }

        //public void ReadCookiesFromDisk()
        //{
        //    try
        //    {
        //        using (Stream stream = System.IO.File.Open("tradematesports-cookie.bin", FileMode.Open))
        //        {
        //            BinaryFormatter formatter = new BinaryFormatter();

        //            this.m_cookieContainer = (CookieContainer)formatter.Deserialize(stream);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        this.m_cookieContainer = new CookieContainer();
        //    }
        //}
    }
    class TradematesportsScraper
    {
        private onWriteStatusEvent _onWriteStatus;
        private TrademateCtrl m_betCtrl = null;
        public TradematesportsScraper(onWriteStatusEvent onWriteStatus)
        {
            _onWriteStatus = onWriteStatus;

            ServicePointManager.DefaultConnectionLimit = 2;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            string curPath = Directory.GetCurrentDirectory();
            string BookieData = System.IO.File.ReadAllText(curPath + "\\Constant\\BookieData.json");
            GameConstants.Tradematesports_BookieData = JsonConvert.DeserializeObject<JObject>(BookieData);
            string SportData = System.IO.File.ReadAllText(curPath + "\\Constant\\SportData.json");
            GameConstants.Tradematesports_SportData = JsonConvert.DeserializeObject<JObject>(SportData);
        }

        private void prepareBetControl()
        {
            
            m_betCtrl = new TrademateCtrl(_onWriteStatus);
            //bool bRet = m_betCtrl.doLogin();
            //if (bRet)
            //{
            //    _onWriteStatus("Tradematesports Login Success!");
            //}
            //else
            //{
            //    _onWriteStatus("Tradematesports Login Failure!");
            //}
            Task.Run(() => {
                m_betCtrl.startListening();
            });
        }

        private void ScrapeProc()
        {
            prepareBetControl();
            while (GameConstants.bRun)
            {
                lock (m_betCtrl._TradeList)
                {
                    List<JsonTrade> Bet365FeedList = new List<JsonTrade>();

                    Trace.WriteLine($"[trademate] raw count: {m_betCtrl._TradeList.Count}");
                    for (int i = 0; i < m_betCtrl._TradeList.Count; i++)
                    {
                        JsonTrade tradeItem = m_betCtrl._TradeList[i];

                        bool bIsShowedItem = false;
                        try
                        {
                            Monitor.Enter(GameConstants.lockertrademateIDLists);
                            
                            if (GameConstants.trademateIDLists.Contains(tradeItem.id))
                                bIsShowedItem = true;
                        }
                        catch { }
                        finally
                        {
                            Monitor.Exit(GameConstants.lockertrademateIDLists);
                        }

                        if (!bIsShowedItem)
                            continue;

                        if (tradeItem.bookmakerName != "Bet365")
                        {
                            m_betCtrl.registerTrade(tradeItem);                            
                            continue;
                        }

                      
                        GameConstants.TrademateSports_displayOddType(tradeItem);
                        
                        try
                        {
                            tradeItem.period = GameConstants.Tradematesports_eventTypeIds[tradeItem.eventPartId];
                        }
                        catch
                        {
                            tradeItem.period = "Whole Match";
                        }

                        /*
                        JsonTrade existTrade = m_reg_trades.Find(t => t.outcomeId == tradeItem.outcomeId && !string.IsNullOrEmpty(tradeItem.selectionId));
                        if (existTrade == null)
                            tradeItem.selectionId = GetSelectionId(tradeItem , pre_game.leaguename);
                        else
                            tradeItem.selectionId = existTrade.selectionId;

                        if (string.IsNullOrEmpty(tradeItem.selectionId) || string.IsNullOrEmpty(tradeItem.bet365Link))
                            continue;
                        */
                        tradeItem.b3Market = GameConstants.TrademateSports_GetMarketName(tradeItem, tradeItem.leagueName);
                        
                        m_betCtrl.registerTrade(tradeItem);
                        //m_betCtrl.removeTrade(tradeItem);
                        Bet365FeedList.Add(tradeItem);
                    }

                    if (Bet365FeedList.Count > 0)
                    {
                        //remove all low edge pick in same market and matches

                        List<BetburgerInfo> trademateInfoList = new List<BetburgerInfo>();
                        foreach (JsonTrade trade in Bet365FeedList)
                        {
                            bool bLowerEdgeInSameMarket = false;
                            foreach (JsonTrade tradeanother in Bet365FeedList)
                            {
                                if (trade == tradeanother)
                                    continue;
                                if (trade.sportId == tradeanother.sportId &&
                                    trade.leagueName == tradeanother.leagueName &&
                                    trade.countryName == tradeanother.countryName &&
                                    trade.homeTeam == tradeanother.homeTeam &&
                                    trade.awayTeam == tradeanother.awayTeam &&
                                    trade.market == tradeanother.market &&
                                    trade.runnerText == tradeanother.runnerText &&
                                    trade.period == tradeanother.period)
                                {
                                    if (trade.edge < tradeanother.edge)
                                    {
                                        bLowerEdgeInSameMarket = true;
                                        break;
                                    }
                                }
                            }
                            if (bLowerEdgeInSameMarket)
                                continue;
                            BetburgerInfo info = new BetburgerInfo();
                            info.arbId = trade.id;
                            info.kind = PickKind.Type_6;                            
                            info.percent = (decimal)trade.edge;
                            info.sport = trade.sportName;
                            info.league = trade.leagueName;
                            info.homeTeam = trade.homeTeam.Replace("(W)", "Women");
                            info.awayTeam = trade.awayTeam.Replace("(W)", "Women");
                            info.bookmaker = trade.bookmakerName;
                            info.eventTitle = trade.homeTeam + "-" + trade.awayTeam;
                            info.odds = trade.odds;
                            info.outcome = trade.outcomeText;
                            info.direct_link = trade.market + "|" + trade.period + "|" + trade.runnerText + "|" + trade.oddsTypeCondition + "|" + trade.marketText;
                            info.started = Utils.UnixTimeStampToDateTime(double.Parse(trade.startTime)).ToString("MM/dd/yyyy HH:mm:ss");
                            info.updated = info.started;
                            info.created = DateTime.Now.ToString();
                            info.siteUrl = trade.participant + "|" + trade.homeTeamId + "|" + trade.awayTeamId + "|" + trade.output + "|" + trade.oddsType + "|" + trade.oddsTypeCondition + "|" + trade.typeId;
                                                       
                            info.opbookmaker = "Valuebet";
                            
                            trademateInfoList.Add(info);
                            
                        }
                        _onWriteStatus($"[Trademate] Sending pick: {trademateInfoList.Count/2}");
                        GameServer.GetInstance().processValuesInfo(trademateInfoList);
                    }
                }
                Thread.Sleep(1000);
            }
        }
          
       

        public async Task scrape()
        {
            Thread thr = new Thread(ScrapeProc);
            thr.Start();
        }

        private string getLogTitle()
        {
            return "[Oddsjam]";
        }
    }
}
