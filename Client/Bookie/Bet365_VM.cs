//using PuppeteerSharp;
//using PuppeteerSharp.Input;
namespace Project.Bookie
{
#if (BET365_VM)
    public enum TaskType
    {
        None,
        Login,
        Addbet,
        Placebet,
        Openbet,
        GetBalance,
        RefreshPage,
    }

    public class TaskParam
    {
        public TaskType type;
        public string f;
        public string fp;
        public string o;
        public string st;
        public string tr;
    }
    public class Bet365_VMCtrl : IBookieController
    {
        //communication with page Thread.
        private Thread pageThread = null;

        private object taskLocker = new object();
        private List<TaskParam> taskList = new List<TaskParam>();


        private ManualResetEventSlim waitGetBalanceResultEvent = new ManualResetEventSlim();
        private double getBalanceResult = -1;

        private ManualResetEventSlim wait_LoginResultEvent = new ManualResetEventSlim();
        private bool wait_LoginResult = false;

        private ManualResetEventSlim wait_AddbetResultEvent = new ManualResetEventSlim();
        private string wait_AddbetResult = string.Empty;
        private ManualResetEventSlim wait_AddbetExecuteEvent = new ManualResetEventSlim();

        private ManualResetEventSlim wait_PlacebetResultEvent = new ManualResetEventSlim();
        private string wait_PlacebetResult = string.Empty;
        private ManualResetEventSlim wait_PlacebetExecuteEvent = new ManualResetEventSlim();


        TaskParam paramForRoute = new TaskParam();


        private const double minMarketStake = 10;
         

        Dictionary<string, int> Bet365IconNumber = new Dictionary<string, int>
        { 
            //{"American Football", 12},
            //{"Baseball", 16},
            {"Basketball", 18},
            {"Esports", 151},
            //{"Golf", 7},
            //{"Greyhounds", 4},
            //{"Horse Racing", 2},
            {"Ice Hockey", 17},
            {"Soccer", 1},
            //{"Table Tennis", 92},
            {"Tennis", 13},
            //{"Volleyball", 91},
            //{"Handball", 78}
        };

        Browser _browser;
        BrowserContext _context;
        Page _page;

        int pageWidth = 0, pageHeight = 0;
        private void RunBrowser()
        {
            LogMng.Instance.onWriteStatus("Starting InitBrowser");
            int retryCount = 2;
            while (--retryCount > 0)
            {
                try
                {
                    Utils.CloseBrowser();
                    // Sessions folder, History, Cookies files
                    LogMng.Instance.onWriteStatus("VMLogin Running");
                    string puppeteerUrl = Utils.LaunchBrowser();
                    if (puppeteerUrl == "Browser profile not found.")
                    {
                        LogMng.Instance.onWriteStatus("Please input VMLogin profile correctly!");
                        return;
                    }
                    Thread.Sleep(5000);
                    LogMng.Instance.onWriteStatus(puppeteerUrl);
                    var browserFetcher = new BrowserFetcher();
                    //await browserFetcher.DownloadAsync();
                    if (puppeteerUrl.Contains("ws"))
                    {
                        _browser = Puppeteer.ConnectAsync(
                        new ConnectOptions
                        {
                            BrowserWSEndpoint = puppeteerUrl,
                            DefaultViewport = null,
                        }).Result;
                    }
                    else
                    {
                        _browser = Puppeteer.ConnectAsync(
                        new ConnectOptions
                        {
                            BrowserURL = puppeteerUrl,
                            DefaultViewport = null,
                        }).Result;
                    }
                    //_context = await _browser.CreateIncognitoBrowserContextAsync();
                    //await _context.DefaultContext.CloseAsync();
                    _context = _browser.DefaultContext;
                    //await _context.OverridePermissionsAsync("https://www.bet365.es&quot;, new OverridePermission[] { OverridePermission.Geolocation });
                    Page[] pages = _context.PagesAsync().Result;
                    if (pages.Length > 0)
                        _page = pages[0];
                    else 
                        _page = _context.NewPageAsync().Result;
                    //await _page.SetUserAgentAsync();
                    Dictionary<string, string> extraHeaders = new Dictionary<string, string>();
                    string INJECTED_JS_CODE = File.ReadAllText("inject.js") + "\n"
                                                                      + "\nObject.defineProperty(navigator,'language',{get:function(){return 'es-ES';}});Object.defineProperty(navigator,'languages',{get:function(){return['es','es-ES'];}});" + "\n";

                    _page.EvaluateExpressionOnNewDocumentAsync(INJECTED_JS_CODE).Wait();


                    _page.SetRequestInterceptionAsync(true);
                    //_page.Response += Page_Response;
                    //_page.Request += Router;

                    if (Setting.Instance.domain.Contains("es"))
                        extraHeaders.Add("Accept-Language", "es-ES,es;q=0.9");

                    _page.SetExtraHttpHeadersAsync(extraHeaders).Wait();
                    //await _page.SetCacheEnabledAsync(false);
                    //await _page.GoToAsync("http://lumtest.com/myip.json&quot;);
                    _page.GoToAsync($"https://www.{Setting.Instance.domain}/#/HO/").Wait();

                    pageWidth = Convert.ToInt32(RunScript("window.innerWidth"));
                    pageHeight = Convert.ToInt32(RunScript("window.innerHeight"));
                    Thread.Sleep(5000);                    
                }
                catch (Exception ex)
                {
                    //btnLunch_Click(null, null);
                    LogMng.Instance.onWriteStatus(ex.ToString());
                }
            }
            LogMng.Instance.onWriteStatus("Finished InitBrowser");            

        }

        

        Random rnd = new Random();
        public Bet365_VMCtrl()
        {
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"Bet365_VMCtrl Start");
#endif
            paramForRoute.type = TaskType.None;

            RunBrowser();


            lock (taskLocker)
            {
                taskList.Clear();
            }
            pageThread = new Thread(PageManagerProc);
            pageThread.Start();
        }

        public void Close()
        {
            try
            {
                Utils.CloseBrowser();
            }
            catch { }

            if (pageThread != null)
                pageThread.Abort();

        }

        private void Router(object sender, RequestEventArgs e)
        {
            var postdata = e.Request.PostData as string;
            var headers = e.Request.Headers;
            var method = e.Request.Method;
            var url = e.Request.Url;
            try
            {
                if (method == HttpMethod.Post)
                {
                    if (url.Contains("/members/lp/default.aspx"))
                    {
                        string ecUsername = WebUtility.UrlEncode(Setting.Instance.username);
                        string ecPassword = WebUtility.UrlEncode(Setting.Instance.password);
                        postdata = Utils.ReplaceStr(postdata, ecUsername, "&txtUsername=", "&");
                        postdata = Utils.ReplaceStr(postdata, ecPassword, "&txtPassword=", "&");
                    }
                    //else if (url.Contains("addbet"))
                    //{
                    //    if (paramForRoute.type == TaskType.Addbet)
                    //    {
                    //        string ns = Utils.Between(postdata, "ns=", "&");
                    //        ns = WebUtility.UrlDecode(ns);
                    //        ns = Utils.ReplaceStr(ns, paramForRoute.f, "#f=", "#");
                    //        ns = Utils.ReplaceStr(ns, paramForRoute.fp, "#fp=", "#");
                    //        ns = Utils.ReplaceStr(ns, paramForRoute.o, "#o=", "#");
                    //        ns = Utils.ReplaceStr(ns, $"BS{paramForRoute.f}-{paramForRoute.fp}", "TP=", "#");
                    //        postdata = Utils.ReplaceStr(postdata, WebUtility.UrlEncode(ns), "ns=", "&");

                    //        wait_AddbetExecuteEvent.Set();
                    //    }
                    //}
                    //else if (url.Contains("placebet"))
                    //{
                    //    if (paramForRoute.type == TaskType.Placebet)
                    //    {
                    //        postdata = Utils.ReplaceStr(postdata, paramForRoute.f, "#f=", "#");
                    //        postdata = Utils.ReplaceStr(postdata, paramForRoute.fp, "#fp=", "#");
                    //        postdata = Utils.ReplaceStr(postdata, paramForRoute.o, "#o=", "#");
                    //        postdata = Utils.ReplaceStr(postdata, $"BS{paramForRoute.f}-{paramForRoute.fp}", "TP=", "#");

                    //        postdata = Utils.ReplaceStr(postdata, paramForRoute.st, "#ust=", "#");
                    //        postdata = Utils.ReplaceStr(postdata, paramForRoute.st, "#st=", "#");
                    //        postdata = Utils.ReplaceStr(postdata, paramForRoute.tr, "#tr=", "#");

                    //        wait_PlacebetExecuteEvent.Set();
                    //    }
                    //}
                }

            }
            catch { }

            e.Request.ContinueAsync(new Payload()
            {
                Method = method,
                PostData = postdata,
                Headers = headers,
                Url = url
            });            
        }

        private void Page_Response(object sender, ResponseCreatedEventArgs e)
        {
            if (e.Response.Status != HttpStatusCode.OK)
            {
                return;
            }

            try
            {
            //    await e.Response.FinishedAsync();

                if (e.Response.Url.ToLower().Contains("/betswebapi/addbet"))
                {
                    wait_AddbetResult = e.Response.TextAsync().Result;
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"addbet Res: {wait_AddbetResult}");
#endif
                    wait_AddbetResultEvent.Set();
                }
                else if (e.Response.Url.ToLower().Contains("/betswebapi/placebet"))
                {
                    wait_PlacebetResult = e.Response.TextAsync().Result;
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"placebet Res: {wait_PlacebetResult}");
#endif
                    wait_PlacebetResultEvent.Set();
                }
            }
            catch { }

        }

        public string getProxyLocation()
        {
            try
            {
                //page.GoToAsync("http://lumtest.com/myip.json").Wait();
                try
                {
                    _page.GoToAsync("http://checkip.dyndns.org/").Wait();


                    string content = _page.GetContentAsync().Result.Replace("Current IP Address:", "");
                    return content;
                }
                catch { }
            }
            catch (Exception ex)
            {
            }
            return "UNKNOWN";
        }

        public HttpClient initHttpClient(bool bUseNewCookie = true)
        {
            return null;
        }


        public int GetMyBetsCount()
        {
            int result = 0;
            try
            {
                result = Utils.parseToInt(RunScript("document.getElementsByClassName('hm-HeaderMenuItemMyBets_MyBetsCount ')[0].innerText"));
            }
            catch (Exception ex)
            {
                //LogMng.Instance.onWriteStatus($"GetMyBetsCount Exception {ex}");
            }
            return result;
        }



        public void RefreshBecauseBet365Notloading()
        {
            int nRetry = 0;
            while (nRetry++ < 3)
            {
                Thread.Sleep(3000);
                try
                {
                    var visible = PageIsVisible("div.bl-Preloader_Spinner");
                    if (!visible)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                }

                try
                {
                    _page.ReloadAsync(60 * 1000).Wait();
                }
                catch (Exception ex)
                {
                    LogMng.Instance.onWriteStatus($"RefreshBecauseBet365Notloading Exception {ex}");
                }
                Thread.Sleep(5000);
            }
        }

        private bool PageIsVisible(string param)
        {
            //bool bResult = false;
            //try
            //{
            //    bResult = _page.IsVisibleAsync(param).Result;
            //}
            //catch { }
            //return bResult;

            
            WaitForFunctionOptions options = new WaitForFunctionOptions();
            options.Timeout = 1;
            try
            {
                _page.WaitForExpressionAsync(param, options).Wait();
                return true;
            }
            catch { }

            return false;
        }
        private void PageClick(string param, int timeout = 500, int nRetry = 3)
        {
            while (nRetry-- > 0)
            {
                try
                {
                    ClickOptions options = new ClickOptions();
                    options.Delay = timeout;
                    options.ClickCount = 1;
                    options.Button = MouseButton.Left;
                    _page.ClickAsync($"{param}", options).Wait();
                    return;
                }
                catch { }
            }
        }
        private string RunScript(string param)
        {
            string result = "";
            try
            {
                result = _page.EvaluateExpressionAsync(param).Result.ToString().ToLower();
            }
            catch { }
            return result;
        }
        public bool login()
        {
#if (!SCRIPT)
            try
            {
                if (IsPageLoginStatus())
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"CheckLogin Exception: {ex}");
                if (ex.ToString().Contains("System.Threading.Tasks.Task.Wait"))
                {
                    RunBrowser();
                }
            }

            try
            {
                ///
                _page.GoToAsync($"https://www.{Setting.Instance.domain}").Wait();


#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus("Login start");
#endif
                int nTotalRetry = 0;
                while (nTotalRetry++ < 3)
                {
                    try
                    {//return Locator.user.isLoggedIn;

                        RefreshBecauseBet365Notloading();


                        if (IsPageLoginStatus())
                        {
                            return true;
                        }


                        string result = RunScript("document.getElementsByClassName('hm-MainHeaderRHSLoggedOutWide_Login ')[0].outerHTML");

#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Login button status : {result}");
#endif

                        if (!result.Contains("class"))
                        {
                            _page.GoToAsync($"https://www.{Setting.Instance.domain}").Wait();

                            //check if page is loaded all
                            int nRetry1 = 0;
                            while (nRetry1 < 30)
                            {
                                Thread.Sleep(500);
                                result = RunScript("document.getElementsByClassName('hm-MainHeaderRHSLoggedOutWide_Login ')[0].outerHTML");

#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus($"Login button status : {result}");
#endif

                                if (result.Contains("class"))
                                {
                                    break;
                                }
                                nRetry1++;
                            }
                            if (nRetry1 >= 30)       //Page is loading gray page. let's retry
                            {
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus($"Let's retry because of loading gray page");
#endif

                                continue;
                            }
                        }

                        int nButtonRetry = 0;
                        while (nButtonRetry++ < 3)
                        {
                            PageClick("div.hm-MainHeaderRHSLoggedOutWide_Login");
                            Thread.Sleep(500);
                            string button_result = RunScript("document.getElementsByClassName('lms-StandardLogin_Username ')[0].outerHTML");

#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"UserId label status : {result}");
#endif

                            if (button_result.Contains("class"))
                                break;
                        }
                        if (nButtonRetry == 3)
                        {
                            LogMng.Instance.onWriteStatus("Clicking Login button not working. retry from scratch again.");
                            continue;
                        }
                        Thread.Sleep(500);


                        TypeOptions options = new TypeOptions();
                        options.Delay = 100;
                        
                        PageClick("input.lms-StandardLogin_Username");
                        _page.Keyboard.TypeAsync(Setting.Instance.username, options).Wait();
                        Thread.Sleep(500);

                        PageClick($"input.lms-StandardLogin_Password");
                        _page.Keyboard.TypeAsync(Setting.Instance.password, options).Wait();
                        Thread.Sleep(500);

                        try
                        {
                            PageClick("div.lms-StandardLogin_LoginButton");
                        }
                        catch { }
                        try
                        {
                            PageClick("div.lms-LoginButton");
                        }
                        catch { }

                        //                        string region = "";
                        //                        if (Setting.Instance.domain.ToLower().Contains(".es"))
                        //                        {
                        //                            region = "&txtLCNOVR=ES";
                        //                        }
                        //                        if (Setting.Instance.domain.ToLower().Contains(".gr"))
                        //                        {
                        //                            region = "&txtLCNOVR=GR";
                        //                        }
                        //                        string command = "function getCookie(cname) {" +
                        //                                        "  var name = cname + '=';" +
                        //                                        "  var decodedCookie = decodeURIComponent(document.cookie);" +
                        //                                        "  var ca = decodedCookie.split(';');" +
                        //                                        "  for(var i = 0; i <ca.length; i++) {" +
                        //                                        "	var c = ca[i];" +
                        //                                        "	while (c.charAt(0) == ' ') {" +
                        //                                        "	  c = c.substring(1);" +
                        //                                        "	}" +
                        //                                        "	if (c.indexOf(name) == 0) {" +
                        //                                        "	  return c.substring(name.length, c.length);" +
                        //                                        "	}" +
                        //                                        "  }" +
                        //                                        "  return '';" +
                        //                                        "};" +
                        //                                        "var postContent = 'txtUsername=" + Setting.Instance.username + "&txtPassword=" + Setting.Instance.password + region + "&txtTKN=' + getCookie('pstk') + '&txtType=85&platform=1&AuthenticationMethod=0&txtScreenSize=1000%20x%20600&IS=11';" +
                        //                                        "var xhr = new XMLHttpRequest();" +
                        //#if (CHRISTIAN)
                        //                                                            "xhr.open('POST', 'https://members.nj." + Setting.Instance.domain + "/members/lp/default.aspx', true);" +
                        //#else
                        //                                                            "xhr.open('POST', 'https://members." + Setting.Instance.domain + "/members/lp/default.aspx', true);" +
                        //#endif
                        //                                                            "xhr.withCredentials = true;" +
                        //                                        "xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');" +
                        //                                        "xhr.onreadystatechange = function() {" +
                        //                                        "  if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {" +
                        //                                        "	  window.location.reload();" +
                        //                                        "  }" +
                        //                                        "};" +
                        //                                        "xhr.send(postContent);";
                        //                        RunScript(command);

                        //string loginScript = $"doLogin365('{Setting.Instance.username}', '{Setting.Instance.password}', '{Setting.Instance.domain}')";
                        //RunScript(loginScript);

                        int nRetry = 0;
                        while (nRetry < 3)
                        {
                            Thread.Sleep(5000);
                            nRetry++;

                            if (IsPageLoginStatus())
                            {
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus("Login Successed");
#endif
                                return true;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"login exception: {ex}");
#endif
                        if (ex.ToString().Contains("(NS_ERROR_NOT_AVAILABLE) [nsITextInputProcessor.keydown]"))
                        {
                            RunBrowser();

                        }
                    }
                }
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus("Login Failed");
#endif
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("ThrowIfExceptional(Boolean includeTaskCanceledExceptions)"))
                {
                    RunBrowser();

                }

                LogMng.Instance.onWriteStatus($"Exception : {ex.Message} {ex.StackTrace}");
            }
            return false;
#else
            wait_LoginResult = false;
            wait_LoginResultEvent.Reset();

            TaskParam task = new TaskParam();
            task.type = TaskType.Login;
            lock (taskLocker)
            {
                taskList.Add(task);
            }
            if (wait_LoginResultEvent.Wait(100000))
            {
                return wait_LoginResult;
            }
            else
            {
                LogMng.Instance.onWriteStatus("login No Result Event");
            }
            return false;
#endif
        }

        private bool IsPageLoginStatus()
        {
            string result = RunScript("Locator.user.isLoggedIn");
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"LonginStatus(1): {result}");
#endif
            if (result == "true")
            {
                return true;
            }
            return false;
        }
        public void page_login()
        {
            try
            {
                if (IsPageLoginStatus())
                {
                    wait_LoginResult = true;
                    wait_LoginResultEvent.Set();
                    return;
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"CheckLogin Exception: {ex}");
                if (ex.ToString().Contains("System.Threading.Tasks.Task.Wait"))
                {
                    //Closed browser
                    RunBrowser();
                }
            }
            return;

            try
            {
                _page.GoToAsync($"https://www.{Setting.Instance.domain}").Wait();


#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus("Login start");
#endif
                int nTotalRetry = 0;
                while (nTotalRetry++ < 2)
                {
                    try
                    {//return Locator.user.isLoggedIn;

                        RefreshBecauseBet365Notloading();


                        if (IsPageLoginStatus())
                        {
                            wait_LoginResult = true;
                            wait_LoginResultEvent.Set();
                            return;
                        }


                        string result = RunScript("document.getElementsByClassName('hm-MainHeaderRHSLoggedOutWide_Login ')[0].outerHTML");

#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Login button status : {result}");
#endif

                        if (!result.Contains("class"))
                        {
                            _page.GoToAsync($"https://www.{Setting.Instance.domain}").Wait();

                            //check if page is loaded all
                            int nRetry1 = 0;
                            while (nRetry1 < 30)
                            {
                                Thread.Sleep(500);
                                result = RunScript("document.getElementsByClassName('hm-MainHeaderRHSLoggedOutWide_Login ')[0].outerHTML");

#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus($"Login button status : {result}");
#endif

                                if (result.Contains("class"))
                                {
                                    break;
                                }
                                nRetry1++;
                            }
                            if (nRetry1 >= 30)       //Page is loading gray page. let's retry
                            {
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus($"Let's retry because of loading gray page");
#endif

                                continue;
                            }
                        }


                        Thread.Sleep(500);

                        PageClick("div.hm-MainHeaderRHSLoggedOutWide_Login");
                        Thread.Sleep(500);

                        PageClick("input.lms-StandardLogin_Username");
                        _page.Keyboard.TypeAsync(Setting.Instance.username).Wait();
                        //page.FillAsync("input.lms-StandardLogin_Username", Setting.Instance.username).Wait();

                        Thread.Sleep(500);

                        PageClick($"input.lms-StandardLogin_Password");
                        _page.Keyboard.TypeAsync(Setting.Instance.password).Wait();
                        //page.FillAsync("input.lms-StandardLogin_Password", Setting.Instance.password).Wait();
                        Thread.Sleep(500);

                        try
                        {
                            PageClick("div.lms-StandardLogin_LoginButton");
                        }
                        catch { }
                        try
                        {
                            PageClick("div.lms-LoginButton");
                        }
                        catch { }

                        int nRetry = 0;
                        while (nRetry < 3)
                        {
                            Thread.Sleep(5000);
                            nRetry++;

                            if (IsPageLoginStatus())
                            {
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus("Login Successed");
#endif


                                wait_LoginResult = true;
                                wait_LoginResultEvent.Set();
                                return;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"login exception: {ex}");
#endif

                    }
                }
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus("Login Failed");
#endif
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"Exception : {ex.Message} {ex.StackTrace}");
            }

            wait_LoginResult = false;
            wait_LoginResultEvent.Set();
        }

        public PROCESS_RESULT PlaceBetInBrowser(BetburgerInfo info)
        {
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"Placebet action start");
#endif
            OpenBet_Bet365 openbet = Utils.ConvertBetburgerPick2OpenBet_365(info);

            try
            {
                int nTotalRetry = 0;
                while (nTotalRetry < 2)
                {
                    nTotalRetry++;

                    if (!login())
                    {
                        LogMng.Instance.onWriteStatus("Placebet failed because of login failure.");
                        return PROCESS_RESULT.NO_LOGIN;
                    }


#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"[Placebet] EventUrl Modified: {info.eventUrl}");
#endif


                    _page.GoToAsync($"https://www.{Setting.Instance.domain}" + info.eventUrl).Wait();
                    _page.WaitForSelectorAsync(".ipe-EventViewView");
                    int nRetry = 0;
                    while (nRetry++ < 30)
                    {
                        string IsElementExist = RunScript($"scrollBetIntoView('{openbet.betData[0].i2}')");
                        if (IsElementExist == "true")
                        {
                            break;
                        }
                        Thread.Sleep(100);
                    }
                    if (nRetry >= 30)
                    {
                        LogMng.Instance.onWriteStatus("Can't find OddElement");
                        continue;
                    }
                    //scrolling to element
                    nRetry = 0;
                    while (nRetry++ < 50)
                    {
                        string IsElementVisible = RunScript($"isScrolledIntoView()");
                        if (IsElementVisible == "true")
                            break;
                        _page.Mouse.MoveAsync(200, 200);
                        _page.Mouse.WheelAsync(0, 200);
                        Thread.Sleep(50);
                    }

                    if (nRetry >= 50)
                    {
                        LogMng.Instance.onWriteStatus("Can't Scroll to OddElement");
                        continue;
                    }
                    //clicking odd element
                    string oddElementLocation = RunScript("getLocation('{openbet.betData[0].i2}')");
                    Rect iconRect = Utils.ParseRectFromJson(oddElementLocation);
                    if (iconRect.X <= 0 || iconRect.Y <= 0 || iconRect.Width <= 0 || iconRect.Height <= 0)
                        continue;
                    
                    nRetry = 0;
                    while (nRetry++ < 3)
                    {
                        _page.Mouse.ClickAsync((decimal)(iconRect.X + iconRect.Width / 2), (decimal)(iconRect.Y + iconRect.Height / 2));

                        string betslipStatus = RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets.length");
                        if (betslipStatus == "1")
                        {
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus("Placebet Betslip appeared");
#endif
                            break;
                        }
                        Thread.Sleep(500);
                    }
                    if (nRetry >= 3)
                    {
                        LogMng.Instance.onWriteStatus("addbet failed");
                        continue;
                    }
                    
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus("Placebet inputing stake");
#endif

                    string stakeposition = RunScript("JSON.stringify(BetSlipLocator.betSlipManager.betslip.activeModule.slip.footer.stakeBox._active_element.getBoundingClientRect())");
                    iconRect = Utils.ParseRectFromJson(stakeposition);
                    if (iconRect.X <= 0 || iconRect.Y <= 0 || iconRect.Width <= 0 || iconRect.Height <= 0)
                        continue;

                    _page.Mouse.ClickAsync((decimal)(iconRect.X + iconRect.Width / 2), (decimal)(iconRect.Y + iconRect.Height / 2));
                    _page.Keyboard.TypeAsync(info.stake.ToString());

                    //RunScript(string.Format("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.stakeBox.setStake(\"{0}\")", info.stake));
                    Thread.Sleep(500);

                    if (Setting.Instance.bEachWay && info.sport == "Horse Racing" && info.odds >= Setting.Instance.eachWayOdd)
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus("Placebet ticking e/w");
#endif

                        RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.eachwayChecked()");
                        Thread.Sleep(500);
                    }


                    wait_PlacebetResult = "";
                    wait_PlacebetResultEvent.Reset();

                    int nRetryPlacebet = 0;
                    while (nRetryPlacebet < 3)
                    {
                        nRetryPlacebet++;
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus("Placebet clicking placebet button..");
#endif

                        string PlacebetPosition = RunScript("JSON.stringify(BetSlipLocator.betSlipManager.betslip.activeModule.slip.footer.betWrapper.placeBetButton._active_element.getBoundingClientRect())");
                        iconRect = Utils.ParseRectFromJson(PlacebetPosition);
                        if (iconRect.X <= 0 || iconRect.Y <= 0 || iconRect.Width <= 0 || iconRect.Height <= 0)
                            continue;

                        _page.Mouse.ClickAsync((decimal)(iconRect.X + iconRect.Width / 2), (decimal)(iconRect.Y + iconRect.Height / 2));

                        //Thread.Sleep(100);
                        //_page.EvaluateExpressionAsync("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.betPlacement.placeBetButton.slipdelegate.acceptOnlyButtonValidate()").Wait();

                        //Thread.Sleep(100);
                        //_page.EvaluateExpressionAsync("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.betPlacement.placeBetButton.slipdelegate.placeBetButtonValidateAndPlaceBet()").Wait();


                        if (wait_PlacebetResultEvent.Wait(20000))
                        {
                            BetSlipJson betSlipJson = null;
                            if (!string.IsNullOrEmpty(wait_PlacebetResult))
                            {
                                betSlipJson = JsonConvert.DeserializeObject<BetSlipJson>(wait_PlacebetResult);
                                if (betSlipJson.sr == 0)
                                {
                                    return PROCESS_RESULT.PLACE_SUCCESS;
                                }
                            }

                            Thread.Sleep(1000);
                            string betslipState = RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.currentState");

#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"Placebet betslip status {betslipState} retry {nRetryPlacebet}");
#endif

                        }

#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus("Placebet wait timeout ..");
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus("PlaceBetInBrowser exception: " + ex);
            }
            return PROCESS_RESULT.ERROR;
        }

        public void PlaceBet(List<CapsuledBetburgerInfo> infoList)
        {
            if (infoList.Count <= 0)
            {
                LogMng.Instance.onWriteStatus("Infolist is insufficient");
                return;
            }


            List<OpenBet_Bet365> openbetList = new List<OpenBet_Bet365>();
            for (int i = 0; i < infoList.Count; i++)
            {
                OpenBet_Bet365 openbet = Utils.ConvertBetburgerPick2OpenBet_365(infoList[i].betburgerInfo);
                if (openbet == null)
                {
                    if (infoList[i].betburgerInfo.kind != PickKind.Type_4 && infoList[i].betburgerInfo.kind != PickKind.Type_5)
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Directlink error: {infoList[i].betburgerInfo.eventTitle} direct_link: {infoList[i].betburgerInfo.direct_link} siteurl: {infoList[i].betburgerInfo.siteUrl}");
#endif
                        Uri uriResult;
                        if (Uri.TryCreate(infoList[i].betburgerInfo.siteUrl, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                        {
                            infoList[i].result = PlaceBetInBrowser(infoList[i].betburgerInfo);
                            continue;
                        }
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Siteurl invalid error: {infoList[i].betburgerInfo.eventTitle} siteurl: {infoList[i].betburgerInfo.siteUrl}");
#endif
                        infoList[i].result = PROCESS_RESULT.ERROR;
                    }
                }
            }

            bool bAlreadyProcessed = true;
            foreach (var info in infoList)
            {
                if (info.result == PROCESS_RESULT.SUCCESS)
                {
                    bAlreadyProcessed = false;
                    break;
                }
            }

            if (bAlreadyProcessed)
                return;

            infoList[0].result = PlaceBetInBrowser(infoList[0].betburgerInfo);

            //try
            //{
            //    PlaceScriptBet(infoList);
            //}
            //catch (Exception ex)
            //{
            //    LogMng.Instance.onWriteStatus($"Placebet exception {ex}");
            //}

            foreach (var info in infoList)
            {
                if (Global.PackageID == 1 && info.result == PROCESS_RESULT.PLACE_SUCCESS)
                {
                    try
                    {
                        Global.balance = getBalance();
                        int nMyBetCount = GetMyBetsCount();


                        PlacedBetInfo betinfo = new PlacedBetInfo();
                        betinfo.bookmaker = info.betburgerInfo.extra;
                        betinfo.username = Setting.Instance.username;
                        betinfo.odds = info.betburgerInfo.odds;
                        betinfo.stake = info.betburgerInfo.stake;
                        betinfo.balance = Global.balance;
                        betinfo.percent = info.betburgerInfo.percent;
                        betinfo.sport = info.betburgerInfo.sport;
                        betinfo.outcome = info.betburgerInfo.outcome;
                        betinfo.eventTitle = info.betburgerInfo.eventTitle;
                        betinfo.homeTeam = info.betburgerInfo.homeTeam;
                        betinfo.awayTeam = info.betburgerInfo.awayTeam;
                        betinfo.bookmaker = info.betburgerInfo.extra;
                        betinfo.pendingBets = nMyBetCount;
                        UserMng.GetInstance().SendSuccessBetReport(betinfo);

                    }
                    catch { }
                }
            }
        }


        public double getBalance()
        {
            getBalanceResult = -1;
            waitGetBalanceResultEvent.Reset();

            TaskParam task = new TaskParam();
            task.type = TaskType.GetBalance;
            lock (taskLocker)
            {
                taskList.Add(task);
            }
            if (waitGetBalanceResultEvent.Wait(10000))
            {
                return getBalanceResult;
            }
            else
            {
                LogMng.Instance.onWriteStatus("getBalance No Result Event");
            }
            return -1;
        }

        public void page_getBalance()
        {
            int nRetry = 0;
            double result = -1;

            while (nRetry++ < 2)
            {
                PageClick("div.hm-MainHeaderMembersWide_MembersMenuIcon");
                Thread.Sleep(500);
                PageClick("div.um-BalanceRefreshButton_Icon");
                Thread.Sleep(1000);
                try
                {
                    result = Utils.ParseToDouble(RunScript("Locator.user.getBalance().totalBalance"));
                }
                catch
                {

                }

                if (result > 0)
                    break;
                Thread.Sleep(100);
            }

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"getBalance: {result}");
#endif
            PageClick("div.hm-MembersMenuModuleContainer_DarkWash");
            getBalanceResult = result;
            waitGetBalanceResultEvent.Set();
        }

        public void Feature()
        {
            TaskParam task = new TaskParam();
            task.type = TaskType.RefreshPage;
            lock (taskLocker)
            {
                taskList.Add(task);
            }
        }
        public bool Pulse()
        {
            TaskParam task = new TaskParam();
            task.type = TaskType.Openbet;
            lock (taskLocker)
            {
                taskList.Add(task);
            }
            return true;
        }

        private void PageManagerProc()
        {
            while (true)
            {
                if (!Global.bRun)
                {
                    if (taskList.Count == 1 && taskList[0].type == TaskType.Openbet)
                    {
                        //when openbet is requested, it should be run even though it's stopped status.
                    }
                    else
                    {
                        Thread.Sleep(200);
                        continue;
                    }
                }

                if (PageIsVisible("div.alm-ActivityLimitStayButton"))
                {//closing for last login time(balance) popup
                    PageClick("div.alm-ActivityLimitStayButton", 100, 1);
                }

                if (PageIsVisible("div.llm-LastLoginModule_Button"))
                {//closing for last login time(balance) popup
                    PageClick("div.llm-LastLoginModule_Button", 100, 1);
                }

                if (PageIsVisible("div.pm-MessageOverlayCloseButton"))
                {//closing for reading message popup
                    PageClick("div.pm-MessageOverlayCloseButton", 100, 1);
                }

                if (PageIsVisible("div.lqb-QuickBetHeader_DoneButton"))
                {//closing for placebet betslip result box
                    PageClick("div.lqb-QuickBetHeader_DoneButton", 100, 1);
                }

                if (PageIsVisible("div.alm-InactivityAlertRemainButton"))
                {//closing for inactivity alert popup
                    PageClick("div.alm-InactivityAlertRemainButton", 100, 1);
                }

                if (PageIsVisible("div.pm-FreeBetsPushGraphicCloseButton"))
                {//closing for freebet alert popup
                    PageClick("div.pm-FreeBetsPushGraphicCloseButton", 100, 1);
                }

                if (PageIsVisible("button#KeepCurrentLimitsButton"))
                {//closing for deposit limit popup
                    PageClick("button#KeepCurrentLimitsButton", 100, 1);
                }

                if (PageIsVisible("button#btn-keep-current-setting"))
                {//closing for reality check setting
                    PageClick("button#btn-keep-current-setting", 100, 1);
                }

                try
                {
                    if (PageIsVisible("div.hm-MembersMenuModuleContainer_DarkWash"))
                    {//when account context menu opens, click mouse to close it
                        try
                        {
                            _page.Mouse.ClickAsync(70, 58);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                catch { }

                paramForRoute.type = TaskType.None;

                int nTaskListcount = 0;
                lock (taskLocker)
                {
                    nTaskListcount = taskList.Count;
                }
                if (nTaskListcount < 1)
                {
                    int nActKind = rnd.Next(1, 10);

                    if (nActKind < 3)
                    {
                        try
                        {
                            int x = rnd.Next(1, pageWidth);
                            int y = rnd.Next(1, pageHeight);
                            _page.Mouse.MoveAsync(x, y).Wait();
                        }
                        catch { }
                    }
                    else if (nActKind < 4)
                    {
                        try
                        {
                            _page.Keyboard.PressAsync("Home");
                        }
                        catch { }
                    }
                    else if (nActKind < 5)
                    {
                        try
                        {
                            _page.Keyboard.PressAsync("ArrowUp");
                        }
                        catch { }
                    }
                    else if (nActKind < 6)
                    {
                        try
                        {
                            _page.Keyboard.PressAsync("ArrowDown");
                        }
                        catch { }
                    }
                    Thread.Sleep(100 * rnd.Next(2, 4));
                    continue;
                }
                TaskParam task = null;
                lock (taskLocker)
                {
                    task = taskList[0];
                    taskList.RemoveAt(0);
                }
                switch (task.type)
                {
                    case TaskType.None:
                        {
                            Thread.Sleep(500);
                        }
                        break;
                    case TaskType.Login:
                        {
                            page_login();
                        }
                        break;
                    case TaskType.RefreshPage:
                        {
                            try
                            {
                                _page.ReloadAsync(60 * 1000).Wait();
                            }
                            catch (Exception ex)
                            {
                                LogMng.Instance.onWriteStatus($"RefreshPage Exception {ex}");
                            }
                        }
                        break;
                    case TaskType.GetBalance:
                        {
                            page_getBalance();
                        }
                        break;
                    case TaskType.Addbet:
                        {
                            wait_AddbetExecuteEvent.Reset();
                            paramForRoute = task;
                            int nRetry = 2;
                            while (nRetry-- >= 0)
                            {
                                try
                                {
                                    //opening betting page(1st tab)                                    
                                    IEnumerable<ElementHandle> elements = _page.QuerySelectorAllAsync("div.hm-MainHeaderCentreWide_Link").Result;
                                    if (elements.Count() == 3)
                                    {
                                        if (!elements.ElementAt(0).GetPropertyAsync("class").Result.ToString().Contains("hm-HeaderMenuItem_LinkSelected"))
                                        {
                                            elements.ElementAt(0).ClickAsync().Wait();
                                        }
                                    }
                                }
                                catch { }
                                //RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.deleteBet()"); //close betslip
                                PageClick($"div.lbs-NormalBetItem_Remove");

                                int randVal = rnd.Next(1, Bet365IconNumber.Count);
                                PageClick($"div.cis-ClassificationIconSmall-{Bet365IconNumber.ElementAt(randVal).Value}");

                                int nRRetry = 2;
                                while (nRRetry-- >= 0)
                                {
                                    Thread.Sleep(300);
                                    if (PageIsVisible($"div.ff-ParticipantFixtureOdd"))
                                    {
                                        PageClick($"div.ff-ParticipantFixtureOdd");
                                        break;
                                    }
                                    //else if (PageIsVisible($"div.pbb-AddBoostToBetslip"))
                                    //{
                                    //    PageClick($"div.pbb-AddBoostToBetslip");
                                    //    break;
                                    //}
                                }


                                if (wait_AddbetExecuteEvent.Wait(1000))
                                {
                                    break;
                                }
                            }

                            if (!wait_AddbetExecuteEvent.IsSet)
                            {
                                wait_AddbetResult = string.Empty;
                                wait_AddbetResultEvent.Set();
                            }
                        }
                        break;
                    case TaskType.Placebet:
                        {
                            string betslipbox = RunScript("BetSlipLocator.betSlipManager.betslip.uid").ToLower();
                            if (betslipbox == "null")
                            {//no betslip box
                                wait_PlacebetResult = string.Empty;
                                wait_PlacebetResultEvent.Set();
                                break;
                            }

                            wait_PlacebetExecuteEvent.Reset();
                            paramForRoute = task;

                            int nRetry = 2;
                            while (nRetry-- >= 0)
                            {
                                //click stakebox
                                PageClick($"div.lqb-StakeBox_StakeInput");
                                try
                                {
                                    TypeOptions options = new TypeOptions();
                                    options.Delay = 100;
                                    _page.Keyboard.TypeAsync("2", options).Wait();
                                }
                                catch { }


                                PageClick($"div.lqb-PlaceBetButton");


                                if (wait_PlacebetExecuteEvent.Wait(1000))
                                {
                                    break;
                                }
                            }

                            if (!wait_PlacebetExecuteEvent.IsSet)
                            {
                                wait_PlacebetResult = string.Empty;
                                wait_PlacebetResultEvent.Set();
                            }
                        }
                        break;
                    case TaskType.Openbet:
                        {
                            page_login();

                            if (IsPageLoginStatus())
                            {
                                PageClick($"div.hm-HeaderMenuItemMyBets");
                                Thread.Sleep(2000);
                                var command = string.Format("(function () {{ {0} }})();", Global.GetOpenBetListCommandLine);

                                String result = Utils.ParseOpenBet(RunScript(command));
                                LogMng.Instance.onWriteStatus(result);
                                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    System.Windows.MessageBox.Show(result);
                                }));
                            }
                        }
                        break;
                }
            }
        }
        public void PlaceScriptBet(List<CapsuledBetburgerInfo> infoList)
        {
            string strBet365Result = string.Empty;

            PROCESS_RESULT SlipRes = PROCESS_RESULT.ERROR;

            if (!login())
            {
                foreach (var info in infoList)
                    info.result = PROCESS_RESULT.NO_LOGIN;
                LogMng.Instance.onWriteStatus("Placebet failed because of login failure.");
                return;
            }

            int nRetry4SmallMarket = 3;

            List<double> origStakeList = new List<double>();

            while (nRetry4SmallMarket > 0)
            {
                nRetry4SmallMarket--;

                string bet_guid = "", bet_cc = "", bet_pc = "";
                string ns = "", ms = "";
                SlipRes = GetNsToken(ref ns, ref ms, infoList, MAKE_SLIP_STEP.INIT, null, ref bet_guid, ref bet_cc, ref bet_pc);
                if (SlipRes == PROCESS_RESULT.ERROR)
                {
                    LogMng.Instance.onWriteStatus(string.Format("[InitBet] Step 1 Failed"));
                    return;
                }

                int nRetry = 0;
                while (nRetry++ < 2)
                {
#if (SCRIPT)
                    strBet365Result = doAddBetTask(betinfo.betData[0].fd, betinfo.betData[0].i2, betinfo.betData[0].oddStr);
#else
                    strBet365Result = doAddBet(ns, ms);
#endif

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"doAddBet Result: {strBet365Result}");
#endif
                    SlipRes = GetNsToken(ref ns, ref ms, infoList, MAKE_SLIP_STEP.ADD_BET, strBet365Result, ref bet_guid, ref bet_cc, ref bet_pc);

                    if (SlipRes == PROCESS_RESULT.ERROR || SlipRes == PROCESS_RESULT.RE_FIXED)
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[AddBet] Failed"));

                        foreach (var info in infoList)
                            info.result = SlipRes;
                        return;
                    }
                    else if (SlipRes == PROCESS_RESULT.NO_LOGIN)
                    {

                        LogMng.Instance.onWriteStatus(string.Format("[AddBet] Login again..."));
                        if (!login())
                        {
                            LogMng.Instance.onWriteStatus(string.Format("[AddBet] Step 3 Failed"));
                            return;
                        }
                    }
                    else if (SlipRes == PROCESS_RESULT.MOVED)
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[AddBet] Retry because of changed odd(line)"));
                        SlipRes = GetNsToken(ref ns, ref ms, infoList, MAKE_SLIP_STEP.INIT, null, ref bet_guid, ref bet_cc, ref bet_pc);
                        if (SlipRes == PROCESS_RESULT.ERROR)
                        {
                            LogMng.Instance.onWriteStatus(string.Format("[InitBet] Step 2 Failed"));
                            return;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (SlipRes != PROCESS_RESULT.SUCCESS)
                {
                    LogMng.Instance.onWriteStatus(string.Format("[AddBet] Step 4 Failed"));

                    return;
                }

                //if (Setting.Instance.domain.Contains(".au"))
                //{
                //    nRetry = 0;
                //    while (nRetry++ < 2)
                //    {
                //        strBet365Result = doConfirmBet(betinfo.betGuid, ns, ms);
                //        LogMng.Instance.onWriteStatus("confirmbet result: " + strBet365Result);

                //        if (strBet365Result.Contains("\"sr\":0"))
                //        {
                //            break;
                //        }

                //        SlipRes = GetNsToken(ref ns, ref ms, ref betinfo, MAKE_SLIP_STEP.PLACE_BET, strBet365Result);
                //        if (SlipRes == PROCESS_RESULT.ERROR)
                //        {
                //            LogMng.Instance.onWriteStatus(string.Format("[ConfirmBet] Step 2 Failed"));
                //            return SlipRes;
                //        }
                //        else if (SlipRes == PROCESS_RESULT.NO_LOGIN)
                //        {
                //            if (!login())
                //            {
                //                LogMng.Instance.onWriteStatus(string.Format("[ConfirmBet] Step 3 Failed"));
                //                return SlipRes;
                //            }
                //        }
                //        else if (SlipRes == PROCESS_RESULT.MOVED)
                //        {
                //            strBet365Result = doRefreshSlip(ns, ms);
                //            SlipRes = GetNsToken(ref ns, ref ms, ref betinfo, MAKE_SLIP_STEP.PLACE_BET, strBet365Result);
                //        }
                //    }

                //    if (!strBet365Result.Contains("\"sr\":0"))
                //    {
                //        LogMng.Instance.onWriteStatus(string.Format("[ConfirmBet]! confirmbet failed!"));
                //        return PROCESS_RESULT.ERROR;
                //    }
                //}



#if (!SCRIPT)

                strBet365Result = doPlaceBet(bet_guid, bet_cc, bet_pc, ns, ms);
#else
                double tr = betinfo.stake * betinfo.betData[0].odd + 0.0001;
                tr = Math.Truncate(tr * 100) / 100;
                Thread.Sleep(1000);
                strBet365Result = doPlaceBetTask(betinfo.betData[0].fd, betinfo.betData[0].i2, betinfo.betData[0].oddStr, betinfo.stake.ToString("N2"), tr.ToString("N2"));
#endif

#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"doPlaceBet Result: {strBet365Result}");
#endif
                SlipRes = GetNsToken(ref ns, ref ms, infoList, MAKE_SLIP_STEP.PLACE_BET, strBet365Result, ref bet_guid, ref bet_cc, ref bet_pc);


                if (SlipRes == PROCESS_RESULT.PLACE_SUCCESS)
                {
                    LogMng.Instance.onWriteStatus($"[PlaceBet]! success! ");
                    //check if retrying for small markets
                    //if (infoList.Count == 1 && (origStake - betinfo.stake >= 1))
                    //{
                    //    origStake -= betinfo.stake;
                    //    infoList[0].stake = origStake;
                    //    if (origStake < betinfo.stake)
                    //    {
                    //        betinfo.stake = origStake;
                    //    }

                    //    nRetry4SmallMarket = 1;

                    //    LogMng.Instance.onWriteStatus($"[PlaceBet] Retrying for small stake market cur stake : {betinfo.stake}");
                    //    Thread.Sleep(5000);
                    //    continue;
                    //}

                    return;
                }
                else if (SlipRes == PROCESS_RESULT.NO_LOGIN)
                {
                    try
                    {
                        _page.DeleteCookieAsync().Wait();
                    }
                    catch { }
                    LogMng.Instance.onWriteStatus(string.Format("[PlaceBet] Login again..."));
                    if (!login())
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[PlaceBet] Step 3 Failed"));
                        return;
                    }
                }
                else if (SlipRes == PROCESS_RESULT.MOVED)
                {
                    if (nRetry4SmallMarket <= 0)
                        break;

                    //                    ns = ""; ms = "";
                    //                    SlipRes = GetNsToken(ref ns, ref ms, infoList, MAKE_SLIP_STEP.INIT, null, ref bet_guid, ref bet_cc, ref bet_pc);
                    //#if (!SCRIPT)
                    //                    strBet365Result = doAddBet(ns, ms);
                    //#else
                    //                    strBet365Result = doAddBetTask(betinfo.betData[0].fd, betinfo.betData[0].i2, betinfo.betData[0].oddStr);
                    //#endif
                    //#if (TROUBLESHOT)
                    //                    LogMng.Instance.onWriteStatus($"doAddBet in Placebet Result: {strBet365Result}");
                    //#endif
                    //                    SlipRes = GetNsToken(ref ns, ref ms, infoList, MAKE_SLIP_STEP.ADD_BET, strBet365Result, ref bet_guid, ref bet_cc, ref bet_pc);

                }
                else
                {
                    LogMng.Instance.onWriteStatus(string.Format("[PlaceBet] failed result: {0}", SlipRes));
                }

            }
        }

        private string doAddBet(string ns, string ms)
        {
            if (string.IsNullOrEmpty(ns))
                return String.Empty;

            //#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"doAddBet ns: {ns} ms: {ms}");
            //#endif

            try
            {
                wait_AddbetResult = "";
                wait_AddbetResultEvent.Reset();
                //string command = $"var s = {{normals: '{ns}',casts: '',multiples: '{ms}',completeHandler: function(t) {{}},errorHandler: function() {{}}}};ns_betslipstandardlib_util.APIHelper.AddBet(s);";

                string command = $"ns_betslipcorelib_util.BetsWebApi.MakeApiReqiest('addbet', {{normals: '{ns}',casts: '',multiples: '{ms}',completeHandler: function(t) {{}},errorHandler: function() {{}}}});";
                RunScript(command);
                wait_AddbetResultEvent.Wait(30000);
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"doAddBet Res: {wait_AddbetResult}");
#endif
                return wait_AddbetResult;
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"doAddBet Exception {ex}");
            }

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"doAddBet Res:(empty)");
#endif
            return string.Empty;
        }
        private string doPlaceBet(string betGuid, string bet_cc, string bet_pc, string ns, string ms)
        {
            if (string.IsNullOrEmpty(ns))
                return String.Empty;

            //#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"doPlaceBet gid: {betGuid} cc: {bet_cc} pc: {bet_pc} ns: {ns} ms: {ms}");
            //#endif

            betGuid += "&c=" + bet_cc + "&p=" + bet_pc;

            try
            {
                wait_PlacebetResult = "";
                wait_PlacebetResultEvent.Reset();
                //string command = $"var s = {{betGuid: '{betGuid}',participantCorrelation: '{bet_pc}',betRequestCorrelation: '{bet_cc}',normals: '{ns}',casts: '',multiples: '{ms}',completeHandler: function(t) {{}},errorHandler: function() {{}}}};ns_betslipstandardlib_util.APIHelper.PlaceBet(s);";

                string command = $"ns_betslipcorelib_util.BetsWebApi.MakeApiReqiest('placebet?betGuid={betGuid}', {{normals: '{ns}',completeHandler: function(t) {{}},errorHandler: function() {{}}}});";
                if (!string.IsNullOrEmpty(ms))
                {
                    command = $"ns_betslipcorelib_util.BetsWebApi.MakeApiReqiest('placebet?betGuid={betGuid}', {{normals: '{ns}', multiples: '{ms}',completeHandler: function(t) {{}},errorHandler: function() {{}}}})";
                }

                RunScript(command);
                wait_PlacebetResultEvent.Wait(30000);

#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"doPlaceBet Res: {wait_PlacebetResult}");
#endif
                return wait_PlacebetResult;
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"doPlaceBet Exception {ex}");
            }

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"doPlaceBet Res:(empty)");
#endif
            return string.Empty;
        }
        private string doPlaceBetTask(string f, string fp, string o, string st, string tr)
        {
            if (string.IsNullOrEmpty(f) || string.IsNullOrEmpty(fp) || string.IsNullOrEmpty(o) || string.IsNullOrEmpty(st) || string.IsNullOrEmpty(tr))
                return string.Empty;

            wait_PlacebetResult = string.Empty;
            wait_PlacebetResultEvent.Reset();

            LogMng.Instance.onWriteStatus($"doPlaceBet f: {f} fp: {fp} o: {o} st: {st} tr: {tr}");

            TaskParam task = new TaskParam();
            task.type = TaskType.Placebet;
            task.f = f;
            task.fp = fp;
            task.o = o;
            task.st = st;
            task.tr = tr;
            lock (taskLocker)
            {
                taskList.Add(task);
            }
            if (wait_PlacebetResultEvent.Wait(30000))
            {
                return wait_PlacebetResult;
            }
            else
            {
                LogMng.Instance.onWriteStatus("placebet No Result Event");
            }
            return string.Empty;
        }

        private string doAddBetTask(string f, string fp, string o)
        {
            if (string.IsNullOrEmpty(f) || string.IsNullOrEmpty(fp) || string.IsNullOrEmpty(o))
                return string.Empty;

            wait_AddbetResult = string.Empty;
            wait_AddbetResultEvent.Reset();

            LogMng.Instance.onWriteStatus($"doAddBet f: {f} fp: {fp} o: {o}");

            TaskParam task = new TaskParam();
            task.type = TaskType.Addbet;
            task.f = f;
            task.fp = fp;
            task.o = o;
            lock (taskLocker)
            {
                taskList.Add(task);
            }
            if (wait_AddbetResultEvent.Wait(30000))
            {
                return wait_AddbetResult;
            }
            else
            {
                LogMng.Instance.onWriteStatus("addbet No Result Event");
            }
            return string.Empty;
        }

        private PROCESS_RESULT GetNsToken(ref string ns, ref string ms, List<CapsuledBetburgerInfo> infos, MAKE_SLIP_STEP Step, string betSlipString, ref string guid, ref string cc, ref string pc)
        {
            BetSlipJson betSlipJson = null;
            guid = "";
            cc = "";
            pc = "";

            try
            {
                if (Step == MAKE_SLIP_STEP.ADD_BET)
                {
                    LogMng.Instance.onWriteStatus(string.Format("[AddBet Res] {0}", betSlipString));

                    if (!string.IsNullOrEmpty(betSlipString))
                    {
                        betSlipJson = JsonConvert.DeserializeObject<BetSlipJson>(betSlipString);
                    }
                    else
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[ADD_BET|GetNsToken] Not Login (No Slip String)"));

                        return PROCESS_RESULT.CRITICAL_SITUATION;
                    }

                    if (string.IsNullOrEmpty(betSlipJson.bg))
                    {
                        if (betSlipJson.sr == 8)
                        {
                            LogMng.Instance.onWriteStatus(string.Format("[ADD_BET|GetNsToken] Not Login (No bg, sr:8)"));
                            return PROCESS_RESULT.NO_LOGIN;
                        }
                        else if (betSlipJson.sr == -1)
                        {
                            return PROCESS_RESULT.RE_FIXED;  //wait 20 min
                        }
                        LogMng.Instance.onWriteStatus(string.Format("[ADD_BET|GetNsToken] Not Login (Empty bg) res: {0}", betSlipString));
                        return PROCESS_RESULT.CRITICAL_SITUATION;
                    }
                    else
                    {
                        guid = betSlipJson.bg;
                    }

                    cc = WebUtility.UrlEncode(betSlipJson.cc);
                    pc = betSlipJson.pc;
                    if (string.IsNullOrEmpty(cc))
                    {
                        LogMng.Instance.onWriteStatus("cc is incorrect");
                        return PROCESS_RESULT.ERROR;
                    }
                }
                else if (Step == MAKE_SLIP_STEP.PLACE_BET)
                {
                    LogMng.Instance.onWriteStatus(string.Format("[PLACE_BET Res] {0}", betSlipString));

                    if (!string.IsNullOrEmpty(betSlipString))
                    {
                        betSlipJson = JsonConvert.DeserializeObject<BetSlipJson>(betSlipString);
                    }
                    else
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[PLACE_BET|GetNsToken] Error No Slip String (PLACE_BET)"));
                        return PROCESS_RESULT.CRITICAL_SITUATION;
                    }

                    if (betSlipJson.sr == 0)
                    {
                        foreach (var bt in betSlipJson.bt)
                        {
                            for (int i = 0; i < infos.Count; i++)
                            {
                                OpenBet_Bet365 openBet = Utils.ConvertBetburgerPick2OpenBet_365(infos[i].betburgerInfo);
                                if (openBet == null)
                                    continue;

                                if (openBet.betData[0].fd == bt.fi.ToString() && openBet.betData[0].i2 == bt.pt[0].pi)
                                {
                                    infos[i].result = PROCESS_RESULT.PLACE_SUCCESS;
                                    infos[i].betburgerInfo.raw_id = bt.tr;
                                    break;
                                }
                            }
                        }
                        return PROCESS_RESULT.PLACE_SUCCESS;
                    }
                    else if (betSlipJson.sr == 15)
                    {
                        return PROCESS_RESULT.CRITICAL_SITUATION;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"[GetNsToken] Exception Step({Step}) betSlipString({betSlipString})");
                return PROCESS_RESULT.ERROR;
            }

            bool bEachWay = false;

            ns = "";
            //ms = "";

            List<OpenBet_Bet365> openBetList = new List<OpenBet_Bet365>();
            string re = "";
            try
            {
                for (int i = 0; i < infos.Count; i++)
                {
                    if (infos[i].result != PROCESS_RESULT.SUCCESS)
                        continue;

                    OpenBet_Bet365 openbet = Utils.ConvertBetburgerPick2OpenBet_365(infos[i].betburgerInfo);


                    if (openbet.betData[0].eachway && Setting.Instance.bEachWay)
                        bEachWay = true;

                    openbet.betData[0].sa = $"sa={calculateSA()}";

                    if (betSlipJson != null)
                    {
                        if (betSlipJson.sr == 0)
                        {
                            for (int k = 0; k < betSlipJson.bt.Count; k++)
                            {
                                BetSlipItem betSlipItem = betSlipJson.bt[k];
                                if (betSlipItem.pt[0].pi != openbet.betData[0].i2)
                                {
                                    continue;
                                }

                                if (betSlipItem.sr == 0)
                                {
                                    if (betSlipItem.su)
                                    {
                                        LogMng.Instance.onWriteStatus(string.Format("Market is suspended"));
                                        return PROCESS_RESULT.SUSPENDED;
                                    }

                                    if (!string.IsNullOrEmpty(betSlipItem.sa))
                                        openbet.betData[0].sa = $"sa={betSlipItem.sa}";

                                    if (!string.IsNullOrEmpty(betSlipItem.od) && openbet.betData[0].oddStr != betSlipItem.od)
                                    {
                                        LogMng.Instance.onWriteStatus(string.Format("Odd moved {0} -> {1}", openbet.betData[0].oddStr, betSlipItem.od));

                                        openbet.betData[0].oddStr = betSlipItem.od;

                                        infos[i].betburgerInfo.direct_link = string.Format("{0}|{1}|{2}", openbet.betData[0].i2, openbet.betData[0].oddStr, openbet.betData[0].fd);
                                    }

                                    if (!string.IsNullOrEmpty(betSlipItem.pt[0].ha) && openbet.betData[0].ht != betSlipItem.pt[0].ha)
                                    {
                                        LogMng.Instance.onWriteStatus(string.Format("line moved {0} -> {1}", openbet.betData[0].ht, betSlipItem.pt[0].ha));
                                        openbet.betData[0].ht = betSlipItem.pt[0].ha;

                                        infos[i].betburgerInfo.outcome = Utils.ReplaceStr(infos[i].betburgerInfo.outcome, betSlipItem.pt[0].ha, "(", ")");
                                    }

                                    if (!string.IsNullOrEmpty(betSlipItem.oo))
                                        openbet.betData[0].oo = betSlipItem.oo;

                                    if (betSlipItem.oc)
                                        openbet.betData[0].oc = true;

                                    openbet.betData[0].ea = betSlipItem.ea || betSlipItem.ew || betSlipItem.ex;
                                    openbet.betData[0].ed = betSlipItem.ed;
                                }
                                else
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Error sr : {0}", betSlipItem.sr));
                                }

                                break;
                            }
                        }
                        else if (betSlipJson.sr == -2)
                        {
                            LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Session is Locked, Retry after 5 sec"));
                            Thread.Sleep(5 * 1000);
                        }
                        else if (betSlipJson.sr == 10)
                        {
                            LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Balance is not Enough"));
                            return PROCESS_RESULT.SMALL_BALANCE;
                        }
                        else if (betSlipJson.sr == 11 || betSlipJson.sr == 24)
                        {
                            if (openbet.betData.Count == 1)
                            {
                                double maxStake = betSlipJson.bt[0].ms;
                                if (maxStake == 0)
                                {
                                    if (!string.IsNullOrEmpty(betSlipJson.bt[0].re) && Utils.ParseToDouble(betSlipJson.bt[0].re) > 0)
                                    {
                                        //re = betSlipJson.bt[0].re;
                                        openbet.stake /= 2;
                                        if (openbet.stake > minMarketStake)
                                            openbet.stake = minMarketStake;
                                        Thread.Sleep(2 * 1000);
                                    }
                                    else
                                    {
                                        LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Max Stake is 0"));
                                        return PROCESS_RESULT.ZERO_MAX_STAKE;
                                    }
                                }
                                else
                                {
                                    openbet.sl = true;
                                    openbet.stake = maxStake;
                                }
                            }
                            else
                            {
                                if (betSlipJson.mo.Count > 0)
                                {
                                    double maxStake = betSlipJson.mo[0].ms;
                                    if (maxStake == 0)
                                    {
                                        LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Max Stake is 0"));
                                        return PROCESS_RESULT.ZERO_MAX_STAKE;
                                    }
                                    openbet.sl = true;
                                    openbet.stake = maxStake;
                                }
                                else
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Error large than Max Stake in Combine bets, but mo result is inccorect"));
                                    return PROCESS_RESULT.ERROR;
                                }
                            }
                        }
                        else if (betSlipJson.sr == 14)
                        {
                            for (int k = 0; k < betSlipJson.bt.Count; k++)
                            {
                                BetSlipItem betSlipItem = betSlipJson.bt[k];
                                if (betSlipItem.pt[0].pi != openbet.betData[0].i2)
                                {
                                    continue;
                                }

                                if (!string.IsNullOrEmpty(betSlipItem.sa))
                                    openbet.betData[0].sa = $"sa={betSlipItem.sa}";

                                if (betSlipItem.ms == 0)
                                {
                                    re = betSlipItem.re;
                                }
                                else
                                {
                                    if (openbet.stake <= betSlipItem.ms)
                                    {
                                        re = betSlipItem.re;
                                    }
                                    else
                                    {
                                        openbet.sl = true;
                                        openbet.stake = betSlipItem.ms;
                                    }
                                }

                                bool bOddChanged = false;
                                if (!string.IsNullOrEmpty(betSlipItem.od) && openbet.betData[0].oddStr != betSlipItem.od)
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("Odd moved {0} -> {1}", openbet.betData[0].oddStr, betSlipItem.od));
                                    bOddChanged = true;
                                    openbet.betData[0].oddStr = betSlipItem.od;

                                    infos[i].betburgerInfo.direct_link = string.Format("{0}|{1}|{2}", openbet.betData[0].i2, openbet.betData[0].oddStr, openbet.betData[0].fd);

                                    re = string.Empty;
                                }

                                if (!string.IsNullOrEmpty(betSlipItem.pt[0].ha) && openbet.betData[0].ht != betSlipItem.pt[0].ha)
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("line moved {0} -> {1}", openbet.betData[0].ht, betSlipItem.pt[0].ha));
                                    bOddChanged = true;
                                    openbet.betData[0].ht = betSlipItem.pt[0].ha;

                                    infos[i].betburgerInfo.outcome = Utils.ReplaceStr(infos[i].betburgerInfo.outcome, betSlipItem.pt[0].ha, "(", ")");
                                }

                                if (!string.IsNullOrEmpty(betSlipItem.oo))
                                    openbet.betData[0].oo = betSlipItem.oo;

                                if (betSlipItem.oc)
                                    openbet.betData[0].oc = true;

                                if (bOddChanged)
                                    return PROCESS_RESULT.MOVED;
                                break;
                            }
                        }
                        else if (betSlipJson.sr == 8)
                        {
                            LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Not Login (sr:8)"));
                            return PROCESS_RESULT.NO_LOGIN;
                        }
                        else if (betSlipJson.sr == 15)
                        {

                            LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Retry later"));
                            return PROCESS_RESULT.ERROR;
                        }
                        else
                        {
                            LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Unknown SR error {0}", betSlipJson.sr));
                            return PROCESS_RESULT.ERROR;
                        }
                    }

                    openBetList.Add(openbet);
                }
            }
            catch { }
            try
            {
                for (int i = 0; i < openBetList.Count; i++)
                {

                    if (Step == MAKE_SLIP_STEP.INIT)
                    {// have to last bet with "id" when add bet
                        if (i == openBetList.Count - 1)
                            openBetList[i].betData[0].sa = $"id={openBetList[i].betData[0].fd}-{openBetList[i].betData[0].i2}Y";
                    }



                    if (string.IsNullOrEmpty(openBetList[i].betData[0].ht))
                        ns += $"pt=N#o={openBetList[i].betData[0].oddStr}#f={openBetList[i].betData[0].fd}#fp={openBetList[i].betData[0].i2}#so=#c={openBetList[i].betData[0].cl}#mt=22#{openBetList[i].betData[0].sa}#";
                    else
                        ns += $"pt=N#o={openBetList[i].betData[0].oddStr}#f={openBetList[i].betData[0].fd}#fp={openBetList[i].betData[0].i2}#so=#c={openBetList[i].betData[0].cl}#ln={openBetList[i].betData[0].ht}#mt=22#{openBetList[i].betData[0].sa}#";

                    if (!string.IsNullOrEmpty(openBetList[i].betData[0].oo))
                        ns += $"oto={openBetList[i].betData[0].oo}#";

                    ns += $"|TP=BS{openBetList[i].betData[0].fd}-{openBetList[i].betData[0].i2}#";

                    if (openBetList[i].betData[0].oc)
                        ns += $"olc=1#";

                    if (Step != MAKE_SLIP_STEP.INIT)
                    {
                        openBetList[i].betData[0].odd = Utils.FractionToDouble(openBetList[i].betData[0].oddStr);
                        openBetList[i].stake = Math.Truncate(openBetList[i].stake * 100) / 100;

                        if (openBetList[i].betData[0].odd == 0)
                            return PROCESS_RESULT.ERROR;

                        if ((openBetList[i].betData.Count == 1 && betSlipJson != null) || (openBetList[i].betData.Count > 1 && !openBetList[i].doubleBet))
                        {//only 1 bet or multiple bets(not double bet)
                            double tr = openBetList[i].stake * openBetList[i].betData[0].odd + 0.0001;

                            bool bCheckEachwayLine = true;

#if USOCKS || OXYLABS
Setting.Instance.bEachWay = true;
if (Setting.Instance.eachWayOdd < 4)
    Setting.Instance.eachWayOdd = 5.1;
#endif
                            if (!Setting.Instance.bEachWay)
                            {
                                bCheckEachwayLine = false;
                            }
                            else
                            {
                                if (openBetList[i].betData[0].odd < Setting.Instance.eachWayOdd)
                                    bCheckEachwayLine = false;
                            }

                            ns = $"{ns}ust={openBetList[i].stake.ToString("N2")}#st={openBetList[i].stake.ToString("N2")}#";
                            if (openBetList[i].sl)
                                ns += $"sl={openBetList[i].stake.ToString("N2")}#";

                            if (bCheckEachwayLine && openBetList[i].betData[0].cl == "2" && openBetList[i].betData[0].ea && openBetList[i].betData[0].ed != 0)
                            {
                                tr += openBetList[i].stake * Utils.FractionToDoubleOfEachway(openBetList[i].betData[0].oddStr, openBetList[i].betData[0].ed);
                                tr = Math.Truncate(tr * 100) / 100;

                                ns += $"ew=1#";
                            }
                            else
                            {
                                tr = Math.Truncate(tr * 100) / 100;
                            }

                            if (!string.IsNullOrEmpty(re))
                                ns += $"tr={re}#";
                            else
                                ns += $"tr={tr.ToString("N2")}#";
                        }
                    }
                    ns += "||";
                }

                //preparing ms
                if (betSlipJson != null)
                {
                    if (betSlipJson.dm != null)
                    {//dm parameter is for double bet

                        ms = $"id={betSlipJson.dm.bt}#bc={betSlipJson.dm.bc}#";
                        //if (infos[0].betburgerInfo.doubleBet)
                        //{
                        //    ms += $"ust={infos[0].betburgerInfo.stake.ToString("N2")}#st={infos[0].betburgerInfo.stake.ToString("N2")}#";
                        //    if (betSlipJson.dm.ea && bEachWay)
                        //        ms += $"|ew=1#";
                        //}                        
                        ms += "||";
                    }

                    //mo parameter should be added in ms even though it's individual multiple bets
                    if (betSlipJson.mo != null)
                    {
                        foreach (Dm dm in betSlipJson.mo)
                            ms += $"id={dm.bt}#bc={dm.bc}#||";
                    }
                }
            }
            catch (Exception e)
            {
            }

            return PROCESS_RESULT.SUCCESS;
        }
        private string calculateSA()
        {

            int randVal = rnd.Next(1, 15);
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + randVal;
            string aa = unixTimestamp.ToString("X2").ToLower();
            string hexValue = DateTime.Now.Ticks.ToString("X2");
            return aa + "-" + hexValue.Substring(hexValue.Length - 8, 8);
        }
        public long getTick()
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            long timestamp = (long)t.TotalMilliseconds;
            return timestamp;
        }

    }
#endif
}
