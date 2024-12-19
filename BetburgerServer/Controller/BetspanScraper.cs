using BetburgerServer.Constant;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Protocol;
using SeastoryServer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace BetburgerServer.Controller
{
#if !(FORSALE)
    public class BetspanScraper
    {
        private onWriteStatusEvent _onWriteStatus;
        private HttpClient httpClient = null;
        private CookieContainer coockieContainer = null;
        protected WebSocket _webSocket = null;
        private string configuration;
        


        List<string> AlreadyProcessedIDList = new List<string>();

        public BetspanScraper(onWriteStatusEvent onWriteStatus)
        {
            _onWriteStatus = onWriteStatus;
            
            ServicePointManager.DefaultConnectionLimit = 2;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            Assembly assem = GetType().Assembly;

            string[] names = assem.GetManifestResourceNames();

            using (Stream stream = assem.GetManifestResourceStream("BetburgerServer.Constant.betspan_param.txt"))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        configuration = reader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void initHttpClient()
        {
            HttpClientHandler handler = new HttpClientHandler();

            coockieContainer = new CookieContainer();
            handler.CookieContainer = coockieContainer;
            httpClient = new HttpClient(handler);

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.51 Safari/537.36");


            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"99\", \"Google Chrome\";v=\"99\"");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "document");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "navigate");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "none");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-User", "?1");

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
        }

        private IWebElement getElementBy(IWebDriver driver, By by)
        {
            try
            {
                IWebElement element = driver.FindElement(by);
                return element;
            }
            catch (Exception es)
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
                    try
                    {
                        IWebElement element = driver.FindElement(by);
                        Thread.Sleep(2000);
                        return element;
                    }
                    catch (Exception)
                    {

                    }

                    IWebElement divElement = driver.FindElement(by);
                    if (divElement.Displayed)
                        return divElement;

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
            Global.OpenUrl("https://betspan.ru/");
            Global.wait_BetspanLoginEvent.Reset();
            if (Global.wait_BetspanLoginEvent.Wait(1000 * 60 * 10))
            {
                _onWriteStatus(getLogTitle() + "login successed");
                return true;
            }

            _onWriteStatus(getLogTitle() + "login failed");
            return false;
            //if (string.IsNullOrEmpty(cServerSettings.GetInstance().BetspanUsername) || string.IsNullOrEmpty(cServerSettings.GetInstance().BetspanPassword))
            //{
            //    _onWriteStatus(getLogTitle() + "username/password incorrect!");
            //    return false;
            //}

            //initHttpClient();
            //ChromeDriver driver = null;
            //try
            //{
            //    ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService();
            //    chromeDriverService.HideCommandPromptWindow = true;
            //    ChromeOptions option = new ChromeOptions();
            //    driver = new ChromeDriver(chromeDriverService, option);

            //    driver.Navigate().GoToUrl("https://betspan.ru/");

            //    IWebElement eLoginId = getElementBy(driver, By.Name("Username"));
            //    if (eLoginId == null)
            //    {
            //        if (driver != null)
            //        {
            //            try
            //            {
            //                driver.Close();
            //            }
            //            catch { }
            //            try
            //            {
            //                driver.Quit();
            //            }
            //            catch { }
            //        }
            //        return true;
            //    }

            //    IWebElement eLoginPassword = getElementBy(driver, By.Name("Password"));
            //    if (eLoginPassword == null)
            //    {
            //        if (driver != null)
            //        {
            //            try
            //            {
            //                driver.Close();
            //            }
            //            catch { }
            //            try
            //            {
            //                driver.Quit();
            //            }
            //            catch { }
            //        }
            //        return false;
            //    }

            //    eLoginId.SendKeys(cServerSettings.GetInstance().BetspanUsername);
            //    eLoginPassword.SendKeys(cServerSettings.GetInstance().BetspanPassword);

            //    IWebElement eSubmit = getElementBy(driver, By.XPath("//button[contains(@data-bind,'do_login')]"));
            //    if (eSubmit == null)
            //    {
            //        if (driver != null)
            //        {
            //            try
            //            {
            //                driver.Close();
            //            }
            //            catch { }
            //            try
            //            {
            //                driver.Quit();
            //            }
            //            catch { }
            //        }
            //        return false;
            //    }

            //    eSubmit.Click();
            //    if (!WaitForTagVisible(driver, By.XPath("//a[contains(@href,'/ru/payment/plans')]")))
            //    {
            //        if (driver != null)
            //        {
            //            try
            //            {
            //                driver.Close();
            //            }
            //            catch { }
            //            try
            //            {
            //                driver.Quit();
            //            }
            //            catch { }
            //        }

            //        return false;
            //    }

            //    foreach (var coockie in driver.Manage().Cookies.AllCookies)
            //    {
            //        coockieContainer.Add(new System.Net.Cookie(coockie.Name, coockie.Value, coockie.Path, coockie.Domain));
            //    }


            //    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            //    string requestParam = (string)js.ExecuteScript("return JSON.stringify(window.server.create_request())");

            //    StringContent jsonContent = new StringContent(requestParam, Encoding.UTF8, "application/json");
            //    HttpResponseMessage resultBetting = httpClient.PostAsync("https://betspan.ru//Surebets_/LoadSettings", jsonContent).Result;
            //    string SettingStr = resultBetting.Content.ReadAsStringAsync().Result;

            //    if (driver != null)
            //    {
            //        try
            //        {
            //            driver.Close();
            //        }
            //        catch { }
            //        try
            //        {
            //            driver.Quit();
            //        }
            //        catch { }
            //    }

            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    _onWriteStatus(getLogTitle() + $"Login exception {ex}");
            //}

            //if (driver != null)
            //{
            //    try
            //    {
            //        driver.Close();
            //    }
            //    catch { }
            //    try
            //    {
            //        driver.Quit();
            //    }
            //    catch { }
            //}

            //return false;
        }

        public void WebSocketConnect()
        {
            try
            {
                List<KeyValuePair<string, string>> webSockCustomHeaders = new List<KeyValuePair<string, string>>();
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Accept-Encoding", "gzip, deflate, br"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Accept-Language", "en-US,en;q=0.9"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Cache-Control", "no-cache"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Connection", "Upgrade"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Host", "betspan.ru"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Origin", "https://betspan.ru"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Pragma", "no-cache"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Sec-WebSocket-Version", "13"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Upgrade", "websocket"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36"));

                webSockCustomHeaders.Add(new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36"));
                

                _webSocket = new WebSocket($"wss://betspan.ru/xsocket/get/?id=");

                Global.GetCookie("https://betspan.ru");
                foreach (System.Net.Cookie coockie in Global.cookieContainer.GetCookies(new Uri("https://betspan.ru")))
                    _webSocket.SetCookie(new WebSocketSharp.Net.Cookie(coockie.Name, coockie.Value, coockie.Path, coockie.Domain));

                //foreach (System.Net.Cookie coockie in coockieContainer.GetCookies(new Uri("https://betspan.ru")))
                //    _webSocket.SetCookie(new WebSocketSharp.Net.Cookie(coockie.Name, coockie.Value, coockie.Path, coockie.Domain));

                _webSocket.EmitOnPing = true;
                _webSocket.CustomHeaders = webSockCustomHeaders;
                _webSocket.Origin = "https://betspan.ru";
                _webSocket.Compression = CompressionMethod.Deflate;
                _webSocket.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls | SslProtocols.Tls12;
                //_webSocket.Log.Level = WebSocketSharp.LogLevel.Error;                
                _webSocket.OnOpen += Socket_OnOpen;
                _webSocket.OnClose += Socket_OnClose;
                _webSocket.OnError += Socket_OnError;
                _webSocket.OnMessage += Socket_OnMessage;
                //_webSocket.SetProxy("http://127.0.0.1:8888", "", "");                
                _webSocket.Connect();
            }
            catch (Exception ex)
            {
                _onWriteStatus(ex.ToString());
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

        protected virtual void Socket_OnOpen(object sender, EventArgs e)
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

        private int sendPrematchTelegramMsg(string text)
        {
            //return 0;
            if (string.IsNullOrEmpty(text))
                return 0;

            try
            {
                //Telegram.Bot.Types.Message msg = Global.tgbotClient.SendTextMessageAsync(new ChatId(-1001703754548), text, Telegram.Bot.Types.Enums.ParseMode.Html).Result;//Value Channel
                return 1;
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        List<BetburgerInfo> GetBetInfo(dynamic jsonResResp)
        {
            List<BetburgerInfo> result = new List<BetburgerInfo>();

            
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    BetburgerInfo info = new BetburgerInfo();

                    info.kind = PickKind.Type_8;


                    info.percent = Convert.ToDecimal(jsonResResp.Profit);
                    info.sport = jsonResResp.Sport;
                    info.started = jsonResResp.StartDate;
                    info.updated = jsonResResp.Updated;
                    info.created = jsonResResp.Imported;
                    info.extra = jsonResResp.LifeTime;


                    info.outcome = jsonResResp.Items[i].Betcode.ToString();
                    info.bookmaker = jsonResResp.Items[i].Book.ToString().ToLower().Replace(".com", "");
                    info.opbookmaker = "value";
                    info.odds = Convert.ToDouble(jsonResResp.Items[i].Factor.ToString());
                    info.awayTeam = jsonResResp.Items[i].Guest.ToString();
                    info.homeTeam = jsonResResp.Items[i].Home.ToString();
                    info.eventTitle = string.Format("{0} - {1}", info.homeTeam, info.awayTeam);
                    info.league = jsonResResp.Items[i].League.ToString();
                    info.isLive = true;

                    string linkdata = jsonResResp.Items[i].LinkData.ToString();
                    

                    if (info.bookmaker == "bet365")
                    {
                        string[] directSplit = linkdata.Split(new char[] { '@', '-' }, StringSplitOptions.RemoveEmptyEntries);
                        if (directSplit.Length == 3)
                            info.direct_link = string.Format("{0}|{1}|{2}", directSplit[1], directSplit[2], directSplit[0]);
                    }
                    else if (info.bookmaker == "pinnaclesports")
                    {
                        info.direct_link = string.Format("{0}|{1}|{2}", jsonResResp.Items[i].LinkData.ToString(), jsonResResp.Items[i].Link.ToString(), jsonResResp.Items[i].Link2.ToString());
                    }

                    _onWriteStatus(getLogTitle() + $"Send pick: {info.bookmaker} {info.league} {info.eventTitle} {info.extra} {info.odds}");
                    result.Add(info);

                    if (info.percent < (decimal)2 || info.percent > (decimal)20)
                        continue;

                    if (info.sport != "Soccer" && info.sport != "Basketball" && info.sport != "Baseball" && info.sport != "Handball")
                        continue;

                    bool bCanSend = false;
                    if (info.bookmaker == "bet365")
                    {
                        if (info.odds >= 1.4 && info.odds <= 3.5)
                        {
                            bCanSend = true;
                        }
                    }
                    else if (info.bookmaker == "pinnaclesports")
                    {
                        bCanSend = true;
                    }

                    if (bCanSend)
                    {
                        if (!AlreadyProcessedIDList.Contains(info.direct_link))
                        {
                            _onWriteStatus(getLogTitle() + $"Send pick in Tg: {info.bookmaker} {info.league} {info.eventTitle} {info.extra} {info.odds}");
                            AlreadyProcessedIDList.Add(info.direct_link);

                            if (info.bookmaker == "BetburgerInfo")
                            {
                                string message = $"{info.bookmaker} {info.sport} ({info.percent}%)" + Environment.NewLine + $"{info.league}" + Environment.NewLine + $"{info.eventTitle}" + Environment.NewLine + $"{info.outcome}" + Environment.NewLine + "Odd: " + info.odds;
                                Task.Run(() => sendPrematchTelegramMsg(message));
                            }                            
                        }
                    }
                    //if (info.bookmaker == "bet365")
                    //{
                    //    string betsapiparseLogResult = "";

                    //    BetsapiHelper.Instance.UpdateBet365SiteUrl("", ref info, out betsapiparseLogResult);

                    //    if (!string.IsNullOrEmpty(betsapiparseLogResult))
                    //    {
                    //        _onWriteStatus($"betspan betsapi parse error: {betsapiparseLogResult}");
                    //    }
                    //}

                    
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Exception6 {ex.StackTrace} {ex.Message}");
                }
            }           
            
            return result;
        }
        protected virtual void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                _onWriteStatus(getLogTitle() + "BS Received info" + e.Data.Length);

                string data = e.Data;

                dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(data);

                List<BetburgerInfo> betburgerInfoList = new List<BetburgerInfo>();

                foreach (dynamic bet in jsonContent.data)
                {
                    if (bet.IsLive.ToString() == "True")
                    {
                        continue;
                    }

                    List<BetburgerInfo> betburgerInfoPair = GetBetInfo(bet);
                    
                    betburgerInfoList.AddRange(betburgerInfoPair);                        
                    
                }

                _onWriteStatus(getLogTitle() + "BS pick count: " + betburgerInfoList.Count);
                if (betburgerInfoList.Count > 0)
                    GameServer.GetInstance().processValuesInfo(betburgerInfoList);
            }
            catch { }
            //if (Bet365ClientManager.Instance.OnBet365DataReceived != null)
            //    Bet365ClientManager.Instance.OnBet365DataReceived(e.Data);
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
                WebSocketConnect();            
        }

        private string getLogTitle()
        {
            return "[Betspan]";
        }
    }
#endif
}
