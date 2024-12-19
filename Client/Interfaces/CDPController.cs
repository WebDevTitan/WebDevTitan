using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChromeDevTools.Protocol.Chrome.Emulation;
using MasterDevs.ChromeDevTools;
using MasterDevs.ChromeDevTools.Protocol.Chrome.DOM;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Network;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Page;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Runtime;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Target;
using Newtonsoft.Json.Linq;
using Project.Helphers;
using Project.Json;
using Project.Json.SuperTour;
using static Project.Interfaces.CDPMouseController;
using Cookie = MasterDevs.ChromeDevTools.Protocol.Chrome.Network.Cookie;

namespace Project.Interfaces
{
    class CDPController
    {
        private static CDPController _instance = null;

        long documentNodeId = 1;

        public IChromeProcess _browserObj = null;

        public IChromeSession _chromeSession = null;

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

        public List<SuperTour> tours = new List<SuperTour>();

        public List<SuperSport> sports = new List<SuperSport>();

        public string couponStatus = string.Empty;

        //PlayPix
        public string user_id = string.Empty;
        public string jwe_token = string.Empty;
        public string restore_login_message = string.Empty;
        public string user_identify_message = string.Empty;
        public List<string> websocket_request_contents = null;

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
            CDPMouseController.CreateInstance(_chromeSession);
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

                        string scriptResult = File.ReadAllText("inject.txt");
                        var injectResult = _chromeSession.SendAsync(new AddScriptToEvaluateOnNewDocumentCommand() { Source = scriptResult }).Result;

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
                                    //Console.WriteLine(resp_url);
                                    //Superbet
                                    if (resp_url.ToLower().Contains("superbet") && resp_url.Contains("/sessions/complete"))
                                    {
                                        var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                        string responseBody = result.Body;
                                        JObject jResp = JObject.Parse(responseBody);

                                    }
                                    else if (resp_url.Contains("/api/v1/login?clientSourceType=Desktop_new"))
                                    {
                                        int retryCnt = 0;
                                        while (retryCnt < 4)
                                        {
                                            var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                            if (result == null)
                                            {
                                                retryCnt++;
                                                Thread.Sleep(500);
                                                continue;
                                            }
                                            string responseBody = result.Body;
                                            JObject jResp = JObject.Parse(responseBody);
                                            if (jResp["notice"].ToString() == "Sukces")
                                            {
                                                isLogged = true;
                                            }
                                            break;
                                        }

                                    }
                                    else if (resp_url.Contains("/api/v3/getPlayerBalance?clientSourceType=Desktop_new"))
                                    {
                                        int retryCnt = 0;
                                        while (retryCnt < 4)
                                        {
                                            var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                            if (result == null)
                                            {
                                                retryCnt++;
                                                Thread.Sleep(500);
                                                continue;
                                            }

                                            if (result.Base64Encoded)
                                                balanceRespBody = Utils.DecodeBase64(result.Body);
                                            else
                                                balanceRespBody = result.Body;

                                            break;
                                        }


                                    }
                                    else if (resp_url.Contains("https://production-superbet-offer-basic.freetls.fastly.net/sb-basic/api/v2/en-BR/struct"))
                                    {
                                        int retryCnt = 0;
                                        while (retryCnt < 4)
                                        {
                                            var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                            if (result == null)
                                            {
                                                retryCnt++;
                                                Thread.Sleep(500);
                                                continue;
                                            }
                                            eventRespBody = result.Body;
                                            break;
                                        }


                                    }
                                    else if (resp_url.Contains("https://production-superbet-offer-basic.freetls.fastly.net/sb-basic/api/v2/en-BR/events"))
                                    {
                                        int retryCnt = 0;
                                        while (retryCnt < 4)
                                        {
                                            var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                            if (result == null)
                                            {
                                                retryCnt++;
                                                Thread.Sleep(500);
                                                continue;
                                            }
                                            eventRespBody = result.Body;
                                            break;
                                        }

                                    }
                                    else if (resp_url.Contains("legacy-web/betting/submitticket?clientSourceType=Desktop_new"))
                                    {
                                        int retryCnt = 0;
                                        while (retryCnt < 4)
                                        {
                                            var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                            if (result == null)
                                            {
                                                retryCnt++;
                                                Thread.Sleep(500);
                                                continue;
                                            }
                                            PlaceBetRespBody = result.Body;
                                            break;
                                        }
                                    }
                                    //Bet365
                                    if (resp_url.ToLower().Contains("defaultapi/sports-configuration"))
                                    {
                                        var result = (await _chromeSession.SendAsync(new MasterDevs.ChromeDevTools.Protocol.Chrome.Network.GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                        string responseBody = result.Body;

                                        WaitingForLogin = false;
                                        if (responseBody.ToLower().Contains(Setting.Instance.username.ToLower()))
                                            LOGGED = true;
                                        else
                                            LOGGED = false;
                                    }
                                    else if (resp_url.ToLower().Contains("betswebapi") && !resp_url.ToLower().Contains("polltempreceipt"))
                                    {
                                        var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                        string responseBody = result.Body;
                                        PlaceBetRespBody = responseBody;
                                        WaitingForAPI = false;
                                    }
                                    //Betano
                                    else if (resp_url.Contains("/myaccount/api/tokens"))
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                            if (result == null)
                                            {
                                                Thread.Sleep(500);
                                                continue;
                                            }
                                            balanceRespBody = result.Body;
                                            break;
                                        }

                                    }
                                    else if (resp_url.Contains("myaccount/api/ma/customer/balance") || resp_url.Contains("/api/balance?_"))
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                            if (result == null)
                                            {
                                                Thread.Sleep(500);
                                                continue;
                                            }
                                            balanceRespBody = result.Body;
                                            break;
                                        }

                                    }
                                    else if (resp_url.Contains("api/betslip/v3/plain-leg"))
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                            if (result == null)
                                            {
                                                Thread.Sleep(500);
                                                continue;
                                            }
                                            AddBetRespBody = result.Body;
                                            break;
                                        }

                                    }
                                    else if (resp_url.Contains("api/betslip/v3/updatebets"))
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                            if (result == null)
                                            {
                                                Thread.Sleep(500);
                                                continue;
                                            }
                                            PlaceBetRespBody = result.Body;
                                            break;
                                        }

                                    }
                                    else if (resp_url.Contains("api/betslip/v3/place"))
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                            if (result == null)
                                            {
                                                Thread.Sleep(500);
                                                continue;
                                            }
                                            PlaceBetRespBody = result.Body;
                                            break;
                                        }

                                    }
                                    else if (resp_url.Contains("api/betslipcombo/limits"))
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                            if (result == null)
                                            {
                                                Thread.Sleep(500);
                                                continue;
                                            }
                                            PlaceBetRespBody = result.Body;
                                            break;
                                        }

                                    }
                                    //Betplay
                                    else if (resp_url.Contains("/sessions/complete"))
                                    {
                                        var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                        string responseBody = result.Body;
                                        JObject jResp = JObject.Parse(responseBody);
                                        if (responseBody.ToLower().Contains(Setting.Instance.username.ToLower()))
                                        {
                                            kambiToken = jResp["kambiToken"].ToString();
                                            isLogged = true;
                                        }
                                        else
                                        {
                                            isLogged = false;
                                        }
                                    }
                                    else if (resp_url.ToLower().Contains("/api/v2019/betplay/punter/login.json"))
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                            if (result == null)
                                            {
                                                Thread.Sleep(500);
                                                continue;
                                            }
                                            loginRespBody = result.Body;

                                            JObject jResp = JObject.Parse(loginRespBody);
                                            auth_token = jResp["token"].ToString();
                                            LogMng.Instance.onWriteStatus("Auth Token : " + auth_token);
                                            isLogged = true;
                                            break;
                                        }
                                    }
                                    //Tonybet
                                    else if (resp_url.ToLower().Contains("tonybet") && resp_url.Contains("/api/auth"))
                                    {
                                        int retryCnt = 3;
                                        while (retryCnt-- > 0)
                                        {
                                            var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                            if (result == null)
                                            {
                                                Thread.Sleep(500);
                                                continue;
                                            }
                                            string responseBody = result.Body;

                                            JObject jResp = JObject.Parse(responseBody);
                                            auth_token = jResp["data"]["token"].ToString();
                                            isLogged = true;

                                            break;
                                        }

                                    }
                                    //Bwin
                                    else if (resp_url.Contains("/api/login"))
                                    {
                                        int retryCnt = 3;
                                        while (retryCnt-- > 0)
                                        {
                                            try
                                            {
                                                var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                                if (result == null)
                                                {
                                                    Thread.Sleep(500);
                                                    continue;
                                                }
                                                string responseBody = result.Body;
                                                JObject jResp = JObject.Parse(responseBody);
                                                if (jResp["isCompleted"].ToString() == "True")
                                                    isLogged = true;

                                                break;
                                            }
                                            catch { }

                                        }
                                    }
                                    else if (resp_url.Contains("/api/balance"))
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                            if (result == null)
                                            {
                                                Thread.Sleep(500);
                                                continue;
                                            }
                                            balanceRespBody = result.Body;
                                            break;
                                        }
                                    }
                                    else if (resp_url.Contains("cds-api/bettingoffer/picks?x-bwin-accessid"))
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                            if (result == null)
                                            {
                                                Thread.Sleep(500);
                                                continue;
                                            }
                                            AddBetRespBody = result.Body;
                                            break;
                                        }
                                    }
                                    else if (resp_url.Contains("sports/api/placebet/place"))
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                            if (result == null)
                                            {
                                                Thread.Sleep(500);
                                                continue;
                                            }
                                            PlaceBetRespBody = result.Body;
                                            break;
                                        }
                                    }
                                    else if (resp_url.Contains("/api/placebet/querystatus?requestId="))
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                            if (result == null)
                                            {
                                                Thread.Sleep(500);
                                                continue;
                                            }
                                            PlaceBetRespBody = result.Body;
                                            break;
                                        }
                                    }
                                    //Estrelabet
                                    else if (resp_url.Contains("ajax/login"))
                                    {
                                        int retryCnt = 3;
                                        while (retryCnt-- > 0)
                                        {
                                            try
                                            {
                                                var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                                if (result == null)
                                                {
                                                    Thread.Sleep(500);
                                                    continue;
                                                }
                                                string responseBody = result.Body;
                                                if (responseBody.Contains("real_user"))
                                                    isLogged = true;

                                                break;
                                            }
                                            catch { }

                                        }
                                    }
                                    else if (resp_url.Contains("/ajax/profile/getData"))
                                    {
                                        int retryCnt = 3;
                                        while (retryCnt-- > 0)
                                        {
                                            try
                                            {
                                                var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                                if (result == null)
                                                {
                                                    Thread.Sleep(500);
                                                    continue;
                                                }
                                                balanceRespBody = result.Body;

                                                break;
                                            }
                                            catch { }
                                        }
                                    }
                                    else if (resp_url.Contains("api/widget/GetEventDetails"))
                                    {
                                        int retryCnt = 3;
                                        while (retryCnt-- > 0)
                                        {
                                            try
                                            {
                                                var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                                if (result == null)
                                                {
                                                    Thread.Sleep(500);
                                                    continue;
                                                }
                                                eventRespBody = result.Body;
                                                break;
                                            }
                                            catch { }
                                        }
                                    }
                                    //Winamax
                                    else if (resp_url.Contains("authentication/token/authorize"))
                                    {
                                        int retryCnt = 3;
                                        while (retryCnt-- > 0)
                                        {
                                            try
                                            {
                                                var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                                if (result == null)
                                                {
                                                    Thread.Sleep(500);
                                                    continue;
                                                }
                                                loginRespBody = result.Body;
                                                break;
                                            }
                                            catch { }
                                        }
                                    }
                                    else if (resp_url.Contains("account/dashboard.php"))
                                    {
                                        int retryCnt = 3;
                                        while (retryCnt-- > 0)
                                        {
                                            try
                                            {
                                                var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                                if (result == null)
                                                {
                                                    Thread.Sleep(500);
                                                    continue;
                                                }
                                                balanceRespBody = result.Body;
                                                break;
                                            }
                                            catch { }
                                        }
                                    }
                                    else if (resp_url.Contains("/betting/validate_betslip.php"))
                                    {
                                        int retryCnt = 3;
                                        while (retryCnt-- > 0)
                                        {
                                            try
                                            {
                                                var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                                if (result == null)
                                                {
                                                    Thread.Sleep(500);
                                                    continue;
                                                }
                                                PlaceBetRespBody = result.Body;
                                                break;
                                            }
                                            catch { }
                                        }
                                    }
                                    //KTO
                                    else if (resp_url.Contains("api.kto.com/auth/login"))
                                    {
                                        int retryCnt = 3;
                                        while (retryCnt-- > 0)
                                        {
                                            try
                                            {
                                                var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                                if (result == null)
                                                {
                                                    Thread.Sleep(500);
                                                    continue;
                                                }
                                                loginRespBody = result.Body;
                                                break;
                                            }
                                            catch { }
                                        }
                                    }
                                    else if (resp_url.Contains("api/Widget/GetOddsStates"))
                                    {
                                        int retryCnt = 3;
                                        while (retryCnt-- > 0)
                                        {
                                            try
                                            {
                                                var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                                if (result == null)
                                                {
                                                    Thread.Sleep(500);
                                                    continue;
                                                }
                                                AddBetRespBody = result.Body;
                                                break;
                                            }
                                            catch { }
                                        }
                                    }
                                    else if (resp_url.Contains("api/WidgetAuth/SignIn"))
                                    {
                                        int retryCnt = 3;
                                        while (retryCnt-- > 0)
                                        {
                                            try
                                            {
                                                var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                                if (result == null)
                                                {
                                                    Thread.Sleep(500);
                                                    continue;
                                                }
                                                eventRespBody = result.Body;
                                                break;
                                            }
                                            catch { }
                                        }
                                    }
                                    else if (resp_url.Contains("api/widget/placeWidget"))
                                    {
                                        int retryCnt = 3;
                                        while (retryCnt-- > 0)
                                        {
                                            try
                                            {
                                                var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                                if (result == null)
                                                {
                                                    Thread.Sleep(500);
                                                    continue;
                                                }
                                                PlaceBetRespBody = result.Body;
                                                break;
                                            }
                                            catch { }
                                        }
                                    }
                                    //WPLAY
                                    else if (resp_url.Contains("https://login.wplay.co/GetTemporaryAuthenticationToken.php?casinoname=wplayco"))
                                    {
                                        int retryCnt = 3;
                                        while (retryCnt-- > 0)
                                        {
                                            try
                                            {
                                                var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                                if (result == null)
                                                {
                                                    Thread.Sleep(500);
                                                    continue;
                                                }
                                                loginRespBody = result.Body;
                                                break;
                                            }
                                            catch { }
                                        }
                                    }
                                    else if (resp_url.Contains("https://apuestas.wplay.co/web_nr?key=login.go_check_login&poll=N&reason=user-request&is_stream_playing=N"))
                                    {
                                        int retryCnt = 3;
                                        while (retryCnt-- > 0)
                                        {
                                            try
                                            {
                                                var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                                if (result == null)
                                                {
                                                    Thread.Sleep(500);
                                                    continue;
                                                }
                                                balanceRespBody = result.Body;
                                                break;
                                            }
                                            catch { }
                                        }
                                    }
                                    else if (resp_url.Contains("apuestas.wplay.co/web_nr"))
                                    {
                                        int retryCnt = 3;
                                        while (retryCnt-- > 0)
                                        {
                                            try
                                            {
                                                var result = (await _chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                                                if (result == null)
                                                {
                                                    Thread.Sleep(500);
                                                    continue;
                                                }
                                                if (result.Body == "[null]")
                                                    break;


                                                AddBetRespBody = result.Body;
                                                break;
                                            }
                                            catch { }
                                        }
                                    }
                                    var headers = e.Response.Headers;
                                    foreach (var header in headers)
                                    {
                                        if (header.Key != "Set-Cookie")
                                            continue;

                                        string cookie_val = header.Value;
                                        if (cookie_val.Contains("_GTM"))
                                        {

                                        }
                                    }


                                }
                                catch { }
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

                        _chromeSession.Subscribe<WebSocketFrameSentEvent>(e =>
                        {
                            string payloadData = e.Response.PayloadData;
                            Console.WriteLine(payloadData);
                            if (payloadData.Contains("restore_login"))
                            {
                                try
                                {
                                    JObject jPayload = JObject.Parse(payloadData);
                                    LogMng.Instance.onWriteStatus($"Restore_Login Message -> {payloadData}");
                                    restore_login_message = payloadData;
                                    string auth_token = jPayload["params"]["auth_token"].ToString();
                                    //jwe_token = jPayload["params"]["jwe_token"].ToString();

                                }
                                catch { }
                            }
                            else if (payloadData.Contains("login_encrypted"))
                            {
                                LogMng.Instance.onWriteStatus($"Restore_Login Message -> {payloadData}");
                                restore_login_message = payloadData;
                            }
                            else if (payloadData.Contains("store_user_identification_token"))
                            {
                                LogMng.Instance.onWriteStatus($"Restore_Login Message -> {payloadData}");
                                user_identify_message = payloadData;
                            }
                            else if (payloadData.Contains("request_session") || payloadData.Contains("partner.config") || payloadData.Contains("betting"))
                            {
                                try
                                {
                                    if (websocket_request_contents.Count == 0)
                                        websocket_request_contents.Add(payloadData);
                                    else
                                    {
                                        foreach (string message in websocket_request_contents)
                                        {
                                            if (!message.Contains("request_session"))
                                                websocket_request_contents.Add(payloadData);
                                            else if (!message.Contains("partner.config"))
                                                websocket_request_contents.Add(payloadData);
                                            else if (!message.Contains("beting"))
                                                websocket_request_contents.Add(payloadData);

                                        }
                                    }
                                }
                                catch { }
                            }
                        });
                        _chromeSession.Subscribe<WebSocketFrameReceivedEvent>(e =>
                        {
                            string payloadData = e.Response.PayloadData;
                            Console.WriteLine(payloadData);
                            if (payloadData.Contains("auth_token"))
                            {
                                try
                                {
                                    JObject jPayload = JObject.Parse(payloadData);
                                    user_id = jPayload["data"]["user_id"].ToString();
                                    string auth_token = jPayload["data"]["auth_token"].ToString();
                                    //jwe_token = jPayload["data"]["jwe_token"].ToString();

                                }
                                catch { }
                            }
                            else if (payloadData.Contains("jwe_token"))
                            {
                                try
                                {
                                    JObject jPayload = JObject.Parse(payloadData);
                                    jwe_token = jPayload["data"]["jwe_token"].ToString();

                                }
                                catch { }
                            }
                            else if (payloadData.Contains("customer_updated"))
                            {
                                try
                                {
                                    JObject jPayload = JObject.Parse(payloadData);

                                }
                                catch { }
                            }
                        });
                    }
                }
            }
        }
        public CDPController()
        {
            httpClient = getHttpClient();
            websocket_request_contents = new List<string>();
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
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozill" +
                "a/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://betplay.com.co/");

            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "cross-site");
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;

            return httpClientEx;
        }

        public async Task<bool> DoUILogin(string username, string password, string birthday)
        {
            try
            {
                int retry = 3;
                documentNodeId = (await _chromeSession.SendAsync(new GetDocumentCommand())).Result.Root.NodeId;
                NavigateInvoke("https://www.winamax.es/account/login.php");
                Thread.Sleep(10000);

                while (retry-- > 0)
                {
                    try
                    {
                        loginRespBody = string.Empty;
                        documentNodeId = (await _chromeSession.SendAsync(new GetDocumentCommand())).Result.Root.NodeId;

                        Point cur_point = await GetLocationForElement(documentNodeId, "iframe[id='iframe-login']");
                        Point new_point = new Point() { X = cur_point.X + 10, Y = cur_point.Y + 43 };
                        await CLickOnPoint(cur_point.X + 10, cur_point.Y + 43, ClickType.TripleClick);

                        Thread.Sleep(2000);
                        CDPMouseController.Instance.PressKeyboard("Tab");
                        Thread.Sleep(2000);
                        CDPMouseController.Instance.InputText(Setting.Instance.username);
                        Thread.Sleep(2000);

                        CDPMouseController.Instance.PressKeyboard("Tab");
                        Thread.Sleep(2000);
                        CDPMouseController.Instance.InputText(Setting.Instance.password);
                        Thread.Sleep(2000);

                        CDPMouseController.Instance.PressKeyboard("Tab");
                        Thread.Sleep(2000);
                        CDPMouseController.Instance.PressKeyboard("Enter");

                        int rCnt = 0;
                        while (string.IsNullOrEmpty(loginRespBody))
                        {
                            rCnt++;
                            Thread.Sleep(500);
                            if (rCnt > 30)
                                break;
                        }

                        JObject jResp = JObject.Parse(loginRespBody);
                        if (jResp["result"] != null && jResp["result"]["missing_credentials"] != null)
                        {
                            if (jResp["result"]["missing_credentials"][0].ToString() == "birthdate")
                            {
                                bool isSuccess = await SetBirthday(birthday);
                            }
                        }
                        else if (loginRespBody.Contains("WrongCredentials"))
                        {
                            ReloadBrowser();
                            Thread.Sleep(5000);
                        }
                        else
                        {
                            isLogged = true;
                            break;
                        }

                        if (isLogged)
                            break;
                    }
                    catch
                    {
                        Thread.Sleep(3000);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus("[CDPController]-DoUILogin " + ex.ToString());
            }

            return isLogged;
        }

        public async Task<bool> SetBirthday(string birthday)
        {
            bool isSuccess = false;
            try
            {
                string[] arr = birthday.Split('/');

                documentNodeId = (await _chromeSession.SendAsync(new GetDocumentCommand())).Result.Root.NodeId;

                Point cur_point = await GetLocationForElement(documentNodeId, "iframe[id='iframe-login']");
                await CLickOnPoint(cur_point.X + 10, cur_point.Y + 10, ClickType.TripleClick);
                Thread.Sleep(1000);

                CDPMouseController.Instance.PressKeyboard("Tab");
                Thread.Sleep(1000);
                CDPMouseController.Instance.InputText(arr[0]);
                Thread.Sleep(1000);
                CDPMouseController.Instance.InputText(arr[1]);
                Thread.Sleep(1000);
                CDPMouseController.Instance.InputText(arr[2]);
                Thread.Sleep(1000);

                CDPMouseController.Instance.PressKeyboard("Tab");
                Thread.Sleep(2000);
                loginRespBody = string.Empty;

                CDPMouseController.Instance.PressKeyboard("Enter");


                int rCnt = 0;
                while (string.IsNullOrEmpty(loginRespBody))
                {
                    rCnt++;
                    Thread.Sleep(500);
                    if (rCnt > 30)
                        break;
                }

                JObject jResp = JObject.Parse(loginRespBody);
                if (jResp["result"] != null && jResp["result"]["missing_credentials"] != null)
                {

                }
                else if (loginRespBody.Contains("WrongCredentials"))
                {
                    ReloadBrowser();
                    Thread.Sleep(5000);
                }
                else
                {
                    isLogged = true;
                    isSuccess = true;
                }

            }
            catch { }
            return isSuccess;
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
                    if (cookie.Name == "_GTM")
                    {
                        Global.GTM = cookie.Value;
                        LogMng.Instance.onWriteStatus(Global.GTM);
                    }

                    System.Net.Cookie http_cookie = new System.Net.Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain);
                    container.Add(http_cookie);
                }
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus("GetCoookies + " + e.ToString());
            }
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
        public async Task<bool> ClickOnPoint(string scriptResult, ClickType clickType = ClickType.click, int interval = 1)
        {
            try
            {
                JObject posObject = JObject.Parse(scriptResult);
                decimal x = decimal.Parse(posObject.SelectToken("x").ToString());
                decimal y = decimal.Parse(posObject.SelectToken("y").ToString());
                decimal width = Utils.ParseToDecimal(posObject.SelectToken("width").ToString());
                decimal height = Utils.ParseToDecimal(posObject.SelectToken("height").ToString());
                if (x == 0 && y == 0)
                    LogMng.Instance.onWriteStatus("Wrong position detected!");
                else
                {
                    Point point = new Point()
                    {
                        X = (int)x,
                        Y = (int)y
                    };
                    CDPMouseController.Instance.MouseMovement(point, MoveMethod.SQRT);
                    Thread.Sleep(Utils.GetRandValue(500, 1000));
                    int cnt = 1;
                    if (clickType == ClickType.doubleClick)
                        cnt = 2;
                    else if (clickType == ClickType.TripleClick)
                        cnt = 3;

                    CDPMouseController.Instance.MouseClick(point, cnt);

                    return true;
                }
            }
            catch (Exception e)
            {
                //m_handlerWriteStatus("ClickOnPoint " + e.ToString());
            }
            return false;

        }

        public async Task<bool> CLickOnPoint(int x, int y, ClickType clickType = ClickType.click)
        {
            try
            {
                if (x == 0 && y == 0)
                {
                    LogMng.Instance.onWriteStatus("Wrong position detected!");
                    return false;
                }
                else
                {
                    Point point = new Point()
                    {
                        X = x,
                        Y = y
                    };
                    await CDPMouseController.Instance.MouseMovement(point, MoveMethod.SQRT);
                    Thread.Sleep(Utils.GetRandValue(500, 1000));
                    CDPMouseController.Instance.MouseClick(point);
                    return true;
                }
            }
            catch { }

            return true;
        }
        public async Task<bool> CLickElementOn(long documentId, Point point, MoveMethod moveMethod = MoveMethod.BEZIER)
        {
            bool isFound = false;
            try
            {
                await CDPMouseController.Instance.MouseMovement(point, moveMethod);
                CDPMouseController.Instance.MouseClick(point);
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

                await CDPMouseController.Instance.MouseMovement(cur_point, moveMethod);
                CDPMouseController.Instance.MouseClick(cur_point, ClickCnt);
                isFound = true;

            }
            catch { }
            return isFound;
        }
        public async Task<bool> FindAndClickElement(long documentId, long bodyId)
        {
            bool isFound = false;
            try
            {
                Point cur_point = await GetLocationForElement(documentId, bodyId);
                if (cur_point.X == 0 && cur_point.Y == 0)
                    return isFound;

                await CDPMouseController.Instance.MouseMovement(cur_point);
                CDPMouseController.Instance.MouseClick(cur_point, 1);
                isFound = true;
            }
            catch { }
            return isFound;
        }
        public async Task<bool> FindElement(long documentId, string selector, bool isFrame = false)
        {
            bool isFound = false;
            try
            {
                if (!isFrame)
                    documentNodeId = (await _chromeSession.SendAsync(new GetDocumentCommand())).Result.Root.NodeId;

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
        public async Task<bool> FindElement(long documentId, string selector)
        {
            bool isFound = false;
            try
            {
                documentId = await GetDocumentId();

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

        public async Task<bool> ScrollInToView(string selector)
        {
            bool isFound = false;
            try
            {
                long documentId = await GetDocumentId();

                long bodyNodeId = (await _chromeSession.SendAsync(new QuerySelectorCommand
                {
                    NodeId = documentId,
                    Selector = selector
                })).Result.NodeId;

                if (bodyNodeId != 0)
                    isFound = true;

                var result = await _chromeSession.SendAsync(new ScrollIntoViewIfNeededCommand
                {
                    NodeId = bodyNodeId
                });

            }
            catch { }
            return isFound;
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

        public async Task ClickCaptcha()
        {
            try
            {
                documentNodeId = await GetDocumentId();
                string result = ExecuteScript("JSON.stringify(document.getElementsByTagName('iframe')[0].getBoundingClientRect())", true);
                JObject jResp = JObject.Parse(result);

                int x = (int)jResp["x"] + 22;
                int y = (int)jResp["y"] + 23;

                await CLickOnPoint(x, y, ClickType.click);
                Thread.Sleep(8000);
            }
            catch { }
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

    }
}
