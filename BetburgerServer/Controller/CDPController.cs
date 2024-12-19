using ChromeDevTools.Protocol.Chrome.Emulation;
using ChromeDevTools.Protocol.Chrome.Input;
using MasterDevs.ChromeDevTools;
using MasterDevs.ChromeDevTools.Protocol.Chrome.DOM;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Input;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Network;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Page;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Runtime;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Target;
using Newtonsoft.Json.Linq;
using Protocol;
using SeastoryServer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cookie = MasterDevs.ChromeDevTools.Protocol.Chrome.Network.Cookie;

namespace BetburgerServer.Controller
{
    public enum MoveMethod
    {
        SQRT,
        BEZIER
    }
    class CDPController
    {
        private static CDPController _instance = null;

        long documentNodeId = 1;

        public IChromeProcess _browserObj = null;

        IChromeSession _chromeSession = null;

        ChromeSessionFactory _chromeSessionFactory = null;

        UserAgentMetadata _userAgentMetadata = null;

        object _lockerSession = new object();

        HttpClient httpClient = null;

        public string evo_sessionId = string.Empty;

        public string evo_userId = string.Empty;

        List<string> _args = new List<string>()
            {
                //"--headless --disable-gpu",
                "--no-first-run","--disable-default-apps","--no-default-browser-check","--disable-breakpad",
                "--disable-crash-reporter","--no-crash-upload","--deny-permission-prompts",
                "--autoplay-policy=no-user-gesture-required","--disable-prompt-on-repost",
                "--disable-search-geolocation-disclosure","--password-store=basic","--use-mock-keychain",
                "--force-color-profile=srgb","--disable-blink-features=AutomationControlled","--disable-infobars",
                "--disable-session-crashed-bubble","--disable-renderer-backgrounding",
                "--disable-backgrounding-occluded-windows","--disable-background-timer-throttling",
                "--disable-ipc-flooding-protection","--disable-hang-monitor","--disable-background-networking",
                "--metrics-recording-only","--disable-sync","--disable-client-side-phishing-detection",
                "--disable-component-update","--disable-features=TranslateUI,enable-webrtc-hide-local-ips-with-mdns,OptimizationGuideModelDownloading,OptimizationHintsFetching",
                /*"--disable-web-security","--start-maximized"*/
            };

        public bool isPageLoaded = false;

        public bool isLogged = false;

        public string kambiToken = string.Empty;

        public string auth_token = string.Empty;

        public bool WaitingForAPI = false;

        public string loginRespBody = string.Empty;

        public string eventRespBody = string.Empty;

        public string balanceRespBody = string.Empty;

        public string AddBetRespBody = string.Empty;

        public string updateBetRespBody = string.Empty;

        public string PlaceBetRespBody = string.Empty;

        public string maxStakeRespBody = string.Empty;

        public string device_id = string.Empty;

        public string anonymous_id = string.Empty;

        public string couponStatus = string.Empty;


        //Bet365 
        public bool WaitingForLogin = false;
        public bool LOGGED = false;
        public static CDPController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CDPController();
                return _instance;
            }
        }
        public void InitializeBrowser(string url)
        {

            _args.Add("--window-size=1500,900");

            //#if(BETPLAY || RUSHBET)
            //            _args.Add("--window-size=800,805");
            //#else
            //            _args.Add("--window-size=1500,900");
            //#endif

            string _chromePath = "";
            if (File.Exists("chromePath.txt"))
                _chromePath = File.ReadAllText("chromePath.txt");

            //m_handlerWriteStatus(_chromePath);
            string user_dir = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            user_dir = user_dir + "\\Chrome_data\\";

            var chromeProcessFactory = new ChromeProcessFactory(new StubbornDirectoryCleaner(), _chromePath);
            _browserObj = chromeProcessFactory.Create(
                new ChromeBrowserSettings() { UseRandomPort = true, Args = _args.ToArray(), DataDir = user_dir });

            InitializeChromeSession(url);
        }

        protected void InitializeChromeSession(string url)
        {
            if (_browserObj is null)
            {
                return;
            }

            var sessionInfo = _browserObj.GetSessionInfo().Result.LastOrDefault(c => c.Type == "page");
            _chromeSessionFactory = new ChromeSessionFactory();

            _chromeSession = _chromeSessionFactory.Create(sessionInfo.WebSocketDebuggerUrl) as ChromeSession;

            var resultUserAgentBrands = _chromeSession.SendAsync(new EvaluateCommand() { Expression = "JSON.stringify(window.navigator.userAgentData.brands)" }).Result;

            if (resultUserAgentBrands.Result.Result.Value == null)
            {
                //Пустая страница почему-то
                //NavigateInvoke("chrome://new-tab-page");
                Thread.Sleep(2000);
                resultUserAgentBrands = _chromeSession.SendAsync(new EvaluateCommand() { Expression = "JSON.stringify(window.navigator.userAgentData.brands)" }).Result;
            }

            _userAgentMetadata = new UserAgentMetadata()
            {
                Platform = "Windows",
                PlatformVersion = "",
                Architecture = "",
                Model = "",
                Mobile = false
            };

            InitSession("about:blank");
            _chromeSession.SendAsync(new NavigateCommand
            {
                Url = url
            }).Wait();
        }

        private void InitSession(string url)
        {
            lock (_lockerSession)
            {
                var targetInfo = _chromeSession.SendAsync(new CreateTargetCommand() { Url = url }).Result;

                var allSessions = _browserObj.GetSessionInfo().Result;
                foreach (var session in allSessions)
                {
                    // Close all other sessions
                    if (session.Id != targetInfo.Result.TargetId)
                    {
                        _chromeSession.SendAsync(new CloseTargetCommand() { TargetId = session.Id }).Wait();
                    }
                    else
                    {
                        _chromeSession.Dispose();

                        _chromeSession = _chromeSessionFactory.Create(session.WebSocketDebuggerUrl) as ChromeSession;

                        var domEnableResult = _chromeSession.SendAsync<MasterDevs.ChromeDevTools.Protocol.Chrome.DOM.EnableCommand>().Result;
                        var networkEnableResult = _chromeSession.SendAsync<MasterDevs.ChromeDevTools.Protocol.Chrome.Network.EnableCommand>().Result;
                        var pageEnableResult = _chromeSession.SendAsync<MasterDevs.ChromeDevTools.Protocol.Chrome.Page.EnableCommand>().Result;

                        _chromeSession.Subscribe<RequestWillBeSentEvent>(sendedRequest =>
                        {
                            try
                            {
                                string requestUrl = sendedRequest.Request.Url.ToLower();

                            }
                            catch { }
                        });

                        var targets = _chromeSession.SendAsync(new SetDiscoverTargetsCommand() { Discover = true }).Result;

                        //finish page load
                        _chromeSession.Subscribe<LoadEventFiredEvent>(loadEvent =>
                        {
                            // we cannot block in event handler, hence the task
                            Task.Run(async () =>
                            {
                                Console.WriteLine("LoadEventFiredEvent: " + loadEvent.Timestamp);
                                Console.WriteLine("Page Loaded");
                                isPageLoaded = true;
                                documentNodeId = (await _chromeSession.SendAsync(new GetDocumentCommand())).Result.Root.NodeId;
                            });
                        });

                        _chromeSession.Subscribe<RequestWillBeSentEvent>(e =>
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    string requestUrl = e.Request.Url.ToLower();
                                    if (requestUrl.Contains("mt-auth-api.kambicdn.com"))
                                    {
                                        string auth = e.Request.Headers["Authorization"];
                                        if (!string.IsNullOrEmpty(auth))
                                        {
                                            auth_token = auth.Replace("Bearer", string.Empty).Trim();
                                        }
                                    }

                                }
                                catch { }
                            });

                        });

                        _chromeSession.Subscribe<ResponseReceivedEvent>(e =>
                        {
                            Task.Run(async () =>
                            {
                                try
                                {
                                    var resp_url = e.Response.Url;
                                    Console.WriteLine(resp_url);
                                    //Superbet
                                    if (resp_url.ToLower().Contains("/arbs/query"))
                                    {
                                        var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                        string responseBody = result.Body;

                                        List<BetburgerInfo> infos = new List<BetburgerInfo>();
                                        JArray jResp = JArray.Parse(responseBody);
                                        foreach (var jVar in jResp)
                                        {
                                            double rawProfit = (double)jVar["rawProfit"];
                                            double per = 100 - rawProfit * 100;

                                            string event_name = jVar["match"]["name"].ToString();
                                            string sport = jVar["match"]["sport"]["name"].ToString();
                                            string matchTime = jVar["match"]["time"]["timestamp"].ToString();

                                            foreach (var jBet in jVar["bets"])
                                            {
                                                if (jBet["bookie"].ToString() == "winamax")
                                                {
                                                    string eventId = jBet["event"]["id"].ToString();
                                                    string selectionId = jBet["raw"]["id"].ToString();

                                                    BetburgerInfo info = new BetburgerInfo();
                                                    info.kind = PickKind.Type_8;

                                                    info.percent = (decimal)per;
                                                    info.sport = sport;
                                                    info.eventTitle = event_name;
                                                    info.odds = (double)jBet["odds"];
                                                    info.bookmaker = "winamax";
                                                    info.outcome = jBet["raw"]["label"].ToString();
                                                    info.isLive = false;
                                                    info.started = matchTime;
                                                    info.direct_link = selectionId;
                                                    info.eventUrl = $"https://www.winamax.es/apuestas-deportivas/match/{eventId}";

                                                    Global.onwriteStatus(getLogTitle() + $"Send pick: {info.bookmaker} {info.eventTitle} {info.outcome} {info.odds}");
                                                    infos.Add(info);
                                                }
                                            }

                                            Global.onwriteStatus(getLogTitle() + "BetSmarter pick count: " + infos.Count);
                                            if (infos.Count > 0)
                                            {
                                                infos = infos.OrderByDescending(k => k.percent).ToList();
                                                GameServer.GetInstance().processValuesInfo(infos);
                                            }
                                        }
                                    }
                                }
                                catch(Exception exx)
                                {
                                    
                                }
                            });
                        });
                        _chromeSession.Subscribe<FrameStartedLoadingEvent>(frameStarted =>
                        {

                        });

                        _chromeSession.Subscribe<FrameResizedEvent>(e =>
                        {
                            Task.Run(async () =>
                            {
                                Console.WriteLine("FrameResizedEvent: ");
                                Console.WriteLine("Page Loaded");
                            });
                        });
                        //can be FrameStoppedLoadingEvent or LoadEventFiredEvent
                        _chromeSession.Subscribe<FrameStoppedLoadingEvent>(frameStopped =>
                        {

                        });

                        _chromeSession.Subscribe<NavigatedWithinDocumentEvent>(navigatedWithinDocument =>
                        {

                        });

                        _chromeSession.Subscribe<FrameNavigatedEvent>(frameNavigated =>
                        {
                            try
                            {

                            }
                            catch (Exception e)
                            {
                            }
                        });
                        _chromeSession.Subscribe<ExecutionContextCreatedEvent>(executionContext =>
                        {
                            try
                            {
                                Task.Run(async () =>
                                {
                                    var auxData = executionContext.Context.AuxData as JObject;
                                    var frameId = auxData["frameId"].Value<string>();

                                });
                            }
                            catch (Exception e)
                            {
                            }
                        });
                        _chromeSession.Subscribe<ExecutionContextDestroyedEvent>(contextDestroyed =>
                        {
                            try
                            {

                            }
                            catch (Exception e)
                            {
                            }
                        });

                        _chromeSession.Subscribe<FrameDetachedEvent>(frameDetached =>
                        {

                        });

                        _chromeSession.Subscribe<WebSocketFrameReceivedEvent>(e =>
                        {
                            string payloadData = e.Response.PayloadData;
                            if (payloadData.Contains("coupon-status"))
                            {
                                couponStatus = payloadData;
                            }
                        });
                    }
                }
            }
        }
        public CDPController()
        {
            httpClient = getHttpClient();
        }
        private HttpClient getHttpClient()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            handler.CookieContainer = new CookieContainer();

            HttpClient httpClientEx = new HttpClient(handler);
            ServicePointManager.DefaultConnectionLimit = 2;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://betplay.com.co/");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://betplay.com.co/");

            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "cross-site");
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;

            return httpClientEx;
        }

        #region Browser Functions
        public bool NavigateInvoke(string visitUrl)
        {
            try
            {
                if (!visitUrl.StartsWith("https://")) visitUrl = "https://" + visitUrl;
                ExecuteScript(string.Format("location.href==='{0}'?0:location.href='{0}'", visitUrl));
            }
            catch (Exception ex)
            {
                int a = 1;
            }
            return true;
        }
        public async Task<long> GetDocumentId()
        {
            documentNodeId = (await _chromeSession.SendAsync(new GetDocumentCommand())).Result.Root.NodeId;
            return documentNodeId;
        }
        public string ExecuteScript(string jsCode, bool requiredResult = false, bool awaitPromise = false)
        {
            string result = string.Empty;
            try
            {
                if (!requiredResult)
                    _chromeSession.SendAsync(new EvaluateCommand() { Expression = jsCode }).Wait();
                else
                {
                    var script = _chromeSession.SendAsync(new EvaluateCommand() { Expression = jsCode, AwaitPromise = awaitPromise }).Result.Result;
                    if (script.Result.Value == null)
                        return result;

                    result = script.Result.Value.ToString();
                }
            }
            catch { }
            return result;
        }
        public async Task<CookieContainer> GetCoookies()
        {
            CookieContainer container = new CookieContainer();
            try
            {
                GetAllCookiesCommandResponse resp = (await _chromeSession.SendAsync(new GetAllCookiesCommand())).Result;
                foreach (Cookie cookie in resp.Cookies)
                {
                    System.Net.Cookie http_cookie = new System.Net.Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain);
                    container.Add(http_cookie);
                }
            }
            catch { }
            return container;
        }
        public async Task ClearCookies()
        {
            try
            {
                await _chromeSession.SendAsync(new ClearBrowserCookiesCommand());
                Thread.Sleep(5000);
                await _chromeSession.SendAsync(new ReloadCommand());

            }
            catch { }
        }
        public void ReloadBrowser()
        {
            try
            {
                _chromeSession.SendAsync(new ReloadCommand()).Wait();
                Thread.Sleep(2000);
            }
            catch { }
        }

        public async Task<bool> FindElement(long documentId, string selector)
        {
            bool isFound = false;
            try
            {
                long bodyNodeId = (await _chromeSession.SendAsync(new QuerySelectorCommand
                {
                    NodeId = documentId,
                    Selector = selector
                })).Result.NodeId;

                if (bodyNodeId != 0)
                    isFound = true;
            }
            catch { }
            return isFound;
        }

        public async Task<bool> FindAndClickElement(long documentId, string selector, int ClickCnt = 1, MoveMethod moveMethod = MoveMethod.BEZIER)
        {
            bool isFound = false;
            try
            {
                documentId = await GetDocumentId();

                Point cur_point = await GetLocationForElement(documentId, selector);
                if (cur_point.X == 0 && cur_point.Y == 0)
                    return isFound;

                MouseClick(cur_point, ClickCnt);
                isFound = true;

            }
            catch { }
            return isFound;
        }
        public void MouseClick(Point point, int clickCnt = 1)
        {
            try
            {
                long button = (long)MouseButton.Left;
                _chromeSession.SendAsync(new DispatchMouseEventCommand { Type = "mousePressed", Button = "left", ClickCount = clickCnt, Buttons = button, X = point.X, Y = point.Y });
                Thread.Sleep(600);
                _chromeSession.SendAsync(new DispatchMouseEventCommand { Type = "mouseReleased", Button = "left", ClickCount = clickCnt, Buttons = button, X = point.X, Y = point.Y });
            }
            catch { }
        }
        public bool InputText(string text)
        {
            try
            {
                _chromeSession.SendAsync(new ImeSetCompositionCommand { Text = text, SelectionStart = 0, SelectionEnd = (long)text.Length });
                Thread.Sleep(800);
            }
            catch { }
            return true;
        }
        public async Task<Point> GetLocationForElement(long documentId, string selecter)
        {
            Point point = new Point();
            try
            {
                long bodyNodeId = (await _chromeSession.SendAsync(new QuerySelectorCommand
                {
                    NodeId = documentId,
                    Selector = selecter
                })).Result.NodeId;

                if (bodyNodeId == 0)
                    return new Point(0, 0);

                var height = (await _chromeSession.SendAsync(new GetBoxModelCommand { NodeId = bodyNodeId })).Result;
                point.X = (int)height.Model.Content[0];
                point.Y = (int)height.Model.Content[1];
            }
            catch { }
            return point;
        }
        public async Task<Point> GetLocationForElement(long documentId, long bodyNodeId)
        {
            Point point = new Point();
            try
            {
                var height = (await _chromeSession.SendAsync(new GetBoxModelCommand { NodeId = bodyNodeId })).Result;
                point.X = (int)height.Model.Content[0];
                point.Y = (int)height.Model.Content[1];
            }
            catch { }
            return point;
        }

        public void Close_Browser()
        {
            try
            {
                _chromeSession.SendAsync<CloseCommand>().Wait();

            }
            catch { }
            try
            {
                if (_chromeSession != null)
                {
                    _chromeSession.Dispose();
                    _chromeSession = null;
                }


                if (_browserObj != null)
                {
                    _browserObj.Dispose();
                    _browserObj = null;
                }

            }
            catch (Exception e) { }
        }
        #endregion

        public bool DoLoginToBetsmarter()
        {
            bool bLogin = false;
            try
            {
                NavigateInvoke("https://betsmarter.app/");
                Thread.Sleep(10000);

                string innerText = ExecuteScript("document.querySelector(\"a[href = '/login']\").innerText", true);
                if (!innerText.Equals("Login"))
                    return true;

                NavigateInvoke("https://betsmarter.app/login");
                Thread.Sleep(10000);

                bool isClicked = FindAndClickElement(1, "input[type='email']", 3).Result;
                Thread.Sleep(2000);

                InputText(cServerSettings.GetInstance().BetsmarterUsername);
                Thread.Sleep(1000);

                Global.login_url = string.Empty;
                isClicked = FindAndClickElement(1, "button[type='submit']", 1).Result;

                int retryCnt = 0;
                while (string.IsNullOrEmpty(Global.login_url))
                {
                    if (retryCnt > 30)
                        break;

                    retryCnt++;
                    Thread.Sleep(400);
                }

               if (!string.IsNullOrEmpty(Global.login_url))
                    bLogin = true;
            }
            catch { }
            return bLogin;
        }
        private string getLogTitle()
        {
            return "[BetSmarter]";
        }
    }

}
