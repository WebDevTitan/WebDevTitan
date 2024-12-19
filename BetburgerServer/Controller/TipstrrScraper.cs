using BetburgerServer.Constant;
using Newtonsoft.Json;
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
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using WebSocketSharp;

namespace BetburgerServer.Controller
{
#if !(FORSALE)
    public class TipstrrScraper
    {
        private onWriteStatusEvent _onWriteStatus;
        private HttpClient httpClient = null;
        private CookieContainer coockieContainer = null;
        protected WebSocket _webSocket = null;
        private string configuration;

        
        List<string> AlreadyProcessedIDList = new List<string>();

        public TipstrrScraper(onWriteStatusEvent onWriteStatus)
        {
            _onWriteStatus = onWriteStatus;
            
            ServicePointManager.DefaultConnectionLimit = 2;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        private void initHttpClient()
        {
            HttpClientHandler handler = new HttpClientHandler();

            coockieContainer = new CookieContainer();
            handler.CookieContainer = coockieContainer;
            httpClient = new HttpClient(handler);

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");            
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.51 Safari/537.36");


            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"99\", \"Google Chrome\";v=\"99\"");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            
        }

        private IWebElement getElementBy(IWebDriver driver, By by)
        {
            try
            {
                IWebElement element = driver.FindElement(by);
                return element;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private bool WaitForTagVisible(ChromeDriver driver, By by, int waitTime = 20)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitTime));
                wait.PollingInterval = TimeSpan.FromSeconds(1);
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(NoSuchFrameException));
                wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException), typeof(StaleElementReferenceException));
                wait.IgnoreExceptionTypes(typeof(ArgumentOutOfRangeException));
                wait.Until(d =>
                {
                    int nRetry = 0;
                    while (nRetry++ < 5)
                    {
                        try
                        {
                            IWebElement element = driver.FindElement(by);
                            Thread.Sleep(2000);
                            return element;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    try
                    {
                        IWebElement divElement = driver.FindElement(by);
                        if (divElement.Displayed)
                            return divElement;
                    }
                    catch (Exception ex )
                    {

                    }
                    throw new NoSuchElementException();
                });

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        private bool login()
        {
            if (string.IsNullOrEmpty(cServerSettings.GetInstance().SurebetUsername) || string.IsNullOrEmpty(cServerSettings.GetInstance().SurebetPassword))
            {
                _onWriteStatus(getLogTitle() + "username/password incorrect!");
                return false;
            }

            Global.wait_TipstrrAuthorizationToken = "";
            Global.wait_TipstrrAuthorizationEvent.Reset();

            initHttpClient();

            try
            {
                Global.OpenUrl("https://tipstrr.com/login");

                if (Global.wait_TipstrrAuthorizationEvent.Wait(30000))
                {
                    return true;
                }
            }
            catch { }
            return false;
        }

        public void Run()
        {

        }
        public void GetPicks()
        {
            while (true)
            {
                try
                {
                    List<BetburgerInfo> betburgerInfoList = new List<BetburgerInfo>();

                    httpClient.DefaultRequestHeaders.Remove("Authorization");
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Global.wait_TipstrrAuthorizationToken);

                    HttpResponseMessage resultBetting = httpClient.GetAsync("https://tipstrr.com/api/authed/dashboard").Result;
                    string dashboardStr = resultBetting.Content.ReadAsStringAsync().Result;
                    if (dashboardStr.Contains("Request failed with status code 401"))
                    {
                        Trace.WriteLine("401 Error found");
                        Global.RefreshPage();
                        Thread.Sleep(5000);
                        continue;
                    }
                    dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(dashboardStr);
                    _onWriteStatus(getLogTitle() + $"tipstrr received count: {jsonContent.Count}");
                    foreach (dynamic data in jsonContent)
                    {
                        if (data.type != 1)
                        {
                            continue;
                        }

                        //if (AlreadyProcessedIDList.Contains($"{data.portfolioReference}-{data.reference}"))
                        //{
                        //    continue;
                        //}
                        //AlreadyProcessedIDList.Add($"{data.portfolioReference}-{data.reference}");
                                             

                        string eachPickUrl = $"https://tipstrr.com/api/authed/portfolio/{data.portfolioReference}/tips/{data.reference}";
                        HttpResponseMessage eachPickMsg = httpClient.GetAsync(eachPickUrl).Result;
                        string pickStr = eachPickMsg.Content.ReadAsStringAsync().Result;
                        try
                        {
                            dynamic pickjsonContent = JsonConvert.DeserializeObject<dynamic>(pickStr);
                            if (pickjsonContent.bookmakerId != 6)
                                continue;
                        
                            List<BetburgerInfo> betburgerInfoPair = GetBetInfo($"{data.portfolioReference}-{data.reference}", pickjsonContent);
                            //if ((betburgerInfoPair.Count == 2) && (betburgerInfoPair[0].bookmaker == "bet365" || betburgerInfoPair[1].bookmaker == "bet365"))
                            //{
                                betburgerInfoList.AddRange(betburgerInfoPair);
                            //}                            
                        }
                        catch { }
                    }
                    _onWriteStatus(getLogTitle() + "BS pick count: " + betburgerInfoList.Count);
                    if (betburgerInfoList.Count > 0)
                        GameServer.GetInstance().processValuesInfo(betburgerInfoList);

                }
                catch { }
                Thread.Sleep(60000);
            }
        }
        public void Disconnect()
        {
            try
            {
                if (_webSocket != null)
                {
                    _webSocket.Close();
                    _webSocket = null;
                }
            }
            catch (Exception ex)
            {
                _onWriteStatus(getLogTitle() + ex.ToString());
            }
        }


        private int sendTipsterHorseTelegramMsg(string text)
        {
            //return 0;
            if (string.IsNullOrEmpty(text))
                return 0;

            try
            {
                //Telegram.Bot.Types.Message msg = Global.tgbotClient.SendTextMessageAsync(new ChatId(-1001666085196), text, Telegram.Bot.Types.Enums.ParseMode.Html).Result;//Value Channel
                return 1;
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        List<BetburgerInfo> GetBetInfo(string id, dynamic jsonResResp)
        {
            List<BetburgerInfo> result = new List<BetburgerInfo>();

            foreach (dynamic tipBetItem in jsonResResp.tipBetItem)
            {
                HttpResponseMessage matchDetails = httpClient.GetAsync($"https://tipstrr.com/api/fixture/{tipBetItem.fixtureReference}").Result;
                string matchDetailStr = matchDetails.Content.ReadAsStringAsync().Result;

                dynamic matchContent = JsonConvert.DeserializeObject<dynamic>(matchDetailStr);

            
                try
                {
                    BetburgerInfo info = new BetburgerInfo();
                    

                    info.arbId = id;
                    info.kind = PickKind.Type_2;

                    
                    info.opbookmaker = "Tipster";

                    info.percent = 1;

                    foreach (dynamic liveoddItr in tipBetItem.liveOdds)
                    {
                        if (liveoddItr.bookmakerId == 6)
                        {
                            info.bookmaker = "Bet365";
                            info.odds = liveoddItr.odds;
                            info.siteUrl = liveoddItr.deepLink;
                            break;
                        }                            
                    }
                    if (string.IsNullOrEmpty(info.bookmaker))
                        continue;

                    info.eventTitle = matchContent.name;
                        

                    info.league = matchContent.league.group + " " + matchContent.league.name;
                    if (matchContent.league.sportId == 1)
                    {
                        info.sport = "Soccer";
                        info.awayTeam = matchContent.awayTeam.name;
                        info.homeTeam = matchContent.homeTeam.name;
                    }
                    else if (matchContent.league.sportId == 2)
                    {
                        info.sport = "Horse Racing";                                                        
                        info.homeTeam = tipBetItem.betText;
                    }
                    else if (matchContent.league.sportId == 19)
                    {
                        info.sport = "Gol";
                        info.homeTeam = tipBetItem.betText;
                    }
                    else
                    {
                        info.sport = $"Unknown({matchContent.league.sportId.ToString()})";
                        info.homeTeam = tipBetItem.betText;
                        try
                        {
                            info.awayTeam = matchContent.homeTeam.name + '-' + matchContent.homeTeam.name;
                        }
                        catch { }
                    }

                    info.started = matchContent.startTime;
                    info.updated = matchContent.startTime;
                    info.created = DateTime.Now.ToString();
                    info.outcome = tipBetItem.marketText + $"({tipBetItem.betText})";

                    if (!AlreadyProcessedIDList.Contains(info.arbId))
                    {
                        AlreadyProcessedIDList.Add(info.arbId);

                        _onWriteStatus(getLogTitle() + $"Send pick in Tg: {info.bookmaker} {info.sport} {info.league} {info.eventTitle} {info.outcome} {info.odds}");

                        string message = $"{info.bookmaker} {info.sport}" + Environment.NewLine + $"{info.league}" + Environment.NewLine + $"{info.eventTitle}" + Environment.NewLine + $"{info.outcome}" + Environment.NewLine + "Odd: " + info.odds;

                        Task.Run(() => sendTipsterHorseTelegramMsg(message));
                    }

                        
                    result.Add(info);                    
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Exception6 {ex.StackTrace} {ex.Message}");
                }
                
            }
            return result;
        }
       

        public void Send(string strData)
        {
            try
            {
                if (_webSocket.ReadyState == WebSocketState.Open)
                    _webSocket.Send(strData);
            }
            catch (Exception ex)
            {
                _onWriteStatus(getLogTitle() + ex.ToString());
            }
        }

        public async Task scrape()
        {
            
            if (login())
            {
                Thread thr = new Thread(GetPicks);
                thr.Start();
            }
        }

        private string getLogTitle()
        {
            return "[Tipstrr]";
        }
    }
#endif
}
