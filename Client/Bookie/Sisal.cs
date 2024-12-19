namespace Project.Bookie
{
#if (SISAL)
    class MatchLinkData
    {
        string MatchLabel;
        string LinkUrl;
    }

    class UrllWeight
    {
        public string url;
        public int weight;

        public UrllWeight(string _url, int _weight)
        {
            url = _url;
            weight = _weight;
        }
    }
    class SisalCtrl : IBookieController
    {
        Dictionary<string, List<MatchLinkData>> dictEventUrl = new Dictionary<string, List<MatchLinkData>>();
        Object lockerObj = new object();

        public HttpClient m_client = null;
        public string sessionToken = null;

        private IPlaywright playwright;
        private IBrowser browser = null;
        private IPage page = null;

        private ManualResetEventSlim wait_PlacebetResultEvent = new ManualResetEventSlim();
        private string wait_PlacebetResult = string.Empty;
        private ManualResetEventSlim wait_PlacebetExecuteEvent = new ManualResetEventSlim();

        private double balance = -1;
        public string appkey = "";

        private string domain = "sisal.it";
        public SisalCtrl()
        {
            m_client = initHttpClient();

            Playwright.InstallAsync().Wait();
            
        }

        private void RunBrowser()
        {
            //create browser again and login (sometimes, mouse , keyboard is not working)
            try
            {
                if (page != null)
                    page.CloseAsync().Wait();

                if (browser != null)
                    browser.CloseAsync().Wait();
            }
            catch { }

            playwright = Playwright.CreateAsync().Result;
            browser = playwright.Firefox.LaunchAsync(false).Result;

            var _context = browser.NewContextAsync(new ViewportSize() { Width = 1400, Height = 800 }).Result;
            _context.GrantPermissionsAsync(new ContextPermission[1] { ContextPermission.Geolocation }).Wait();

            //string content = File.ReadAllText("mouse.js");
            //_context.AddInitScriptAsync(content, path : "mouse.js");

            //_context.AddInitScriptAsync($"function GetNewOdd(marketid, selectionid) {{ var betList = document.querySelectorAll('ul li.ui-bet-row'); for (var i = 0; i < betList.length; i++) {{ var betItr = betList[i]; if (betItr.getAttribute('data-marketid') != marketid || betItr.getAttribute('data-selectionid') != selectionid) continue; if (betItr.classList.contains('ui-disabled')) return 'Suspended'; return betItr.getAttribute('data-last-price'); }} return 'NotInSlip'; }}");
            //_context.AddInitScriptAsync($"function SetStake(marketid, selectionid, stake) {{ var betList = document.querySelectorAll('ul li.ui-bet-row'); for (var i = 0; i < betList.length; i++) {{ var betItr = betList[i]; if (betItr.getAttribute('data-marketid') != marketid || betItr.getAttribute('data-selectionid') != selectionid) continue; if (betItr.classList.contains('ui-disabled')) return 'Suspended'; if (betItr.querySelector('input.stake') == null) return 'NoStakeBox'; betItr.querySelector('input.stake').value = stake; return 'Success'; }} return 'NotInSlip'; }}");

            page = _context.NewPageAsync().Result;
            page.Response += Page_Response;
        }

        private async void Page_Response(object sender, ResponseEventArgs e)
        {
            try
            {
                await e.Response.FinishedAsync();

                if (e.Response.Url.ToLower().Contains("api/biglietto-common/vendita-biglietto-sport"))
                {//place bet result
                    wait_PlacebetResult = await e.Response.GetTextAsync();
                    wait_PlacebetResultEvent.Set();
                }
                else if (e.Response.Url.ToLower().Contains("account/movements/getbalance"))
                {
                    //wait_PlacebetResult = await e.Response.GetTextAsync();
                    //wait_PlacebetResultEvent.Set();
                    string result = await e.Response.GetTextAsync();
                    dynamic details = JsonConvert.DeserializeObject<dynamic>(result);
                    balance = (double)details.balance.availableBalance / 100;
                    LogMng.Instance.onWriteStatus($"Balance : {balance}");

                }
            }
            catch { }

        }
        public void Close()
        {
            try
            {
                page.CloseAsync().Wait();
                browser.CloseAsync().Wait();
            }
            catch { }

        }

        public bool logout()
        {
            return true;
        }

        public void Feature()
        {
            
        }
        public string getProxyLocation()
        {
            //try
            //{
            //    HttpResponseMessage resp = m_client.GetAsync("http://lumtest.com/myip.json").Result;
            //    var strContent = resp.Content.ReadAsStringAsync().Result;
            //    var payload = JsonConvert.DeserializeObject<dynamic>(strContent);
            //    return payload.ip.ToString() + " - " + payload.country.ToString();
            //}
            //catch (Exception ex)
            //{
            //}
            return "UNKNOWN";
        }
        public HttpClient initHttpClient(bool bUseNewCookie = true)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            if (bUseNewCookie)
                Global.cookieContainer = new CookieContainer(300, 50, 20480);

            handler.CookieContainer = Global.cookieContainer;

            HttpClient httpClientEx = new HttpClient(handler);
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.142 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");


            return httpClientEx;
        }

        private bool PageClick(string param, int timeout = 500, int nRetry = 3)
        {
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"[PageClick] {param}");
#endif
            while (nRetry-- > 0)
            {
                try
                {
                    IElementHandle element = page.QuerySelectorAsync(param).Result;
                    if (element != null && element.IsVisibleAsync().Result)
                    {

                        Rect rect = page.QuerySelectorAsync(param).Result.GetBoundingBoxAsync().Result;
                        
                        page.Mouse.ClickAsync((int)(rect.X + rect.Width / 2), (int)(rect.Y + rect.Height / 2));
#if (TROUBLESHOT)
                        //page.ClickAsync($"{param}", timeout: timeout).Wait();
                        LogMng.Instance.onWriteStatus($"[PageClick] {param} clicked");
#endif
                        return true;
                    }
                }
                catch { }
                Thread.Sleep(500);
            }
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"[PageClick] {param} failed");
#endif
            return false;
        }
        private string RunFunction(string function, object param)
        {
            try
            {
                string result = page.EvaluateAsync(function, param).Result.ToString();
                return result;
            }
            catch (Exception ex)
            { }
            return "error";
        }
        private string RunScript(string param)
        {
            string result = "";
            try
            {
                result = page.EvaluateAsync(param).Result.ToString().ToLower();
            }
            catch (Exception ex)
            { }
            return result;
        }
        public bool InnerLogin(bool bRemovebet = true)
        {

            RunBrowser();
            bool bLogin = false;
            int nRetry = 0;
            while (nRetry++ < 2 && bLogin == false)
            {
                try
                {
                    try
                    {
                        Monitor.Enter(lockerObj);
                        //m_client = initHttpClient();



                        LogMng.Instance.onWriteStatus($"sisal login Start");
                        page.GoToAsync($"https://areaprivata.{domain}/loginJwt/?endcallbackurl=https://www.{domain}/&amp;cancelcallbackurl=https://www.{domain}/").Wait();

                        int nAcceptRetry = 0;
                        while (nAcceptRetry++ < 10)
                        {

                            Thread.Sleep(1000);
                            string acceptCookie = RunScript("document.querySelectorAll(\"button[id='onetrust-accept-btn-handler']\").length");
                            LogMng.Instance.onWriteStatus($"accept button res: {acceptCookie}");
                            if (acceptCookie == "1")
                            {
                                //click accept button

                                PageClick("button[id='onetrust-accept-btn-handler']");
                                break;
                            }
                        }


                        PageClick("input[name='usernameEtc']");
                        page.Keyboard.TypeAsync(Setting.Instance.username).Wait();

                        PageClick("input[name='password']");
                        page.Keyboard.TypeAsync(Setting.Instance.password).Wait();

                        Thread.Sleep(500);

                        PageClick("button#buttonAuth");



                        int nBalanceCheck = 0;
                        while (nBalanceCheck++ < 20)
                        {
                            PageClick("a[href*='.sisal.it/SSOLogin/?auth-token=']");
                            Thread.Sleep(1000);
                            if (getBalance() > 0)
                                break;
                        }

                        if (nBalanceCheck >= 20)
                            continue;

                        if (bRemovebet)
                        {
                            removeBet();
                        }


                        bLogin = true;                        
                    }
                    catch { }
                    finally
                    {
                        Monitor.Exit(lockerObj);
                    }
                }
                catch (Exception e)
                {
                    if (e.ToString().Contains("System.Threading.Tasks.Task.Wait"))
                    {
                        RunBrowser();
                    }
                    LogMng.Instance.onWriteStatus($"login exception {e.StackTrace} {e.Message}");
                }
            }
            //Global.SetMonitorVisible(false);

            if (bLogin)
                LogMng.Instance.onWriteStatus($"sisal login Successed");
            else
                LogMng.Instance.onWriteStatus($"sisal login Failed");
            return bLogin;
        }
        public bool login()
        {
            return InnerLogin();
        }


        public void PlaceBet(List<CapsuledBetburgerInfo> info)
        {
            if (info.Count < 1)
                return;

            info[0].result = PlaceBet(info[0].betburgerInfo);
        }

        Dictionary<string, string> sportsBinding = new Dictionary<string, string>()
        {
            { "soccer","calcio"} ,
            { "tennis","tennis"} ,
            { "basketball","basket"} ,
            { "volleyball","volley"} ,
            { "table tennis","tennis-tavolo"} ,
            { "hockey","hockey-su-ghiaccio"} ,
        };

        
        public PROCESS_RESULT PlaceBet(BetburgerInfo info)
        {
            LogMng.Instance.onWriteStatus(string.Format("Place bet 1"));
            BetResult br = new BetResult();
            balance = -1;
            try
            {
                OpenBet_Sisal openbet = Utils.ConvertBetburgerPick2OpenBet_Sisal(info);
                LogMng.Instance.onWriteStatus(string.Format("Place bet 2"));
                if (openbet == null)
                {
                    LogMng.Instance.onWriteStatus("Converting OpenBet Error");
                    return PROCESS_RESULT.ERROR;
                }

                string ItalianSportsName = sportsBinding[info.sport.ToLower()];
                string sportsurl = $"https://www.{domain}/scommesse-live-i18n/sport/{ItalianSportsName}";
                page.GoToAsync(sportsurl).Wait();

                string matchesUrl = "";
                int nRetryMatchesUrl = 0;
                while (nRetryMatchesUrl++ < 6)
                {
                    matchesUrl = RunScript("JSON.stringify(Array.from(document.querySelectorAll('a[class*=\"regulator_description\"]')).map( a => ({href: a.href, text: a.outerText})))");
                    if (matchesUrl != "[]")
                        break;
                    Thread.Sleep(500);
                }

                if (string.IsNullOrEmpty(matchesUrl))
                {
                    LogMng.Instance.onWriteStatus("Opening sports page error");
                    return PROCESS_RESULT.ERROR;
                }
                
                //expand all matches
                RunScript("document.querySelectorAll(\"div[class*='grid_mg-pointer']\").forEach(function(divobj){ if (divobj.parentElement.getAttribute('data-collapsed')=='1') divobj.click()})");

                List<string> keywords = new List<string>();
                if (!string.IsNullOrEmpty(openbet.sublink))
                {
                    try {
                        string teamspart = openbet.sublink.Substring(openbet.sublink.ToLower().LastIndexOf("/") + 1);
                        keywords.AddRange(teamspart.Split('-').ToList());
                    }
                    catch { }                    
                }
                if (!string.IsNullOrEmpty(info.homeTeam))
                {
                    keywords.AddRange(info.homeTeam.ToLower().Split(new string[] { ",", "-", " " }, StringSplitOptions.None));
                }
                if (!string.IsNullOrEmpty(info.awayTeam))
                {
                    keywords.AddRange(info.awayTeam.ToLower().Split(new string[] { ",", "-", " " }, StringSplitOptions.None));
                }

                for (int i = keywords.Count - 1; i >= 0; i--)
                {
                    if (string.IsNullOrEmpty(keywords[i].Trim()))
                        keywords.RemoveAt(i);
                }

                List<UrllWeight> weightList = new List<UrllWeight>();
                string eventUrl = "";
                JArray eventList = JsonConvert.DeserializeObject<JArray>(matchesUrl);
                foreach (var stemData in eventList)
                {
                    try
                    {
                        if (stemData["href"].ToString() == null)
                        {
                            continue;
                        }
                        string url = stemData["href"].ToString();
                        url = url.Substring(url.LastIndexOf("/") + 1);

                        int weight = 0;
                        foreach (string keyword in keywords)
                        {
                            if (string.IsNullOrEmpty(keyword))
                                continue;
                            if (url.Contains(keyword))
                            {
                                weight++;
                            }
                        }

                        weightList.Add(new UrllWeight(stemData["href"].ToString(), weight));                        
                    }
                    catch { }
                }


                if (weightList.Count < 1)
                    return PROCESS_RESULT.ERROR;
                var ordered = weightList.OrderByDescending(x => x.weight).ToList();
                eventUrl = ordered[0].url;
                
                LogMng.Instance.onWriteStatus(string.Format("Place bet second check - sublink: {0}", eventUrl));

                if (string.IsNullOrEmpty(eventUrl))
                {
                    LogMng.Instance.onWriteStatus("Cannot find EventUrl");
                    return PROCESS_RESULT.ERROR;
                }
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 6"));
                page.GoToAsync(eventUrl).Wait();

                //LogMng.Instance.onWriteStatus(string.Format("Place bet 7"));

                //string loginuserDiv = RunScript("document.getElementsByClassName('ssc-wl ssc-wlco')[0].outerHTML");
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 22 - {0}", loginuserDiv));
                //if (!loginuserDiv.Contains("div"))
                double curBal = getBalance();
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 7.1 check login balance - {0}", curBal));
                if (curBal < 0)
                {
                    LogMng.Instance.onWriteStatus("[Placebet]Login because of logout already");
                    if (!InnerLogin(true))
                        return PROCESS_RESULT.NO_LOGIN;
                    page.GoToAsync(eventUrl).Wait();
                }

                RunScript("document.querySelector(\"button[class*='ticketReceiptSuccess_closeButton']\").click()");
                int nTotalMarketsRetry = 0;
                while (nTotalMarketsRetry++ < 5)
                {
                    string curStatus = RunScript("Array.from(document.querySelectorAll('button')).find(el => el.textContent === 'TUTTE').getAttribute('class')");
                    if (curStatus.Contains("btn-warning"))
                        break;
                    //go to all markets
                    RunScript("Array.from(document.querySelectorAll('button')).find(el => el.textContent === 'TUTTE').click()");
                    Thread.Sleep(200);
                }
                //expand all matches
                RunScript("document.querySelectorAll(\"div[class*='grid_mg-header__2xvyj']\").forEach(function(divobj){ if (divobj.parentElement.getAttribute('data-collapsed')=='1') divobj.click()})");
                Thread.Sleep(500);

                string selector = $"div[data-qa*='{openbet.eventIds[2]}_{openbet.eventIds[3]}_{openbet.eventIds[4]}_{openbet.eventIds[1]}_{openbet.eventIds[0]}']";
                string selectorSpan = $"span[data-qa*='{openbet.eventIds[2]}_{openbet.eventIds[3]}_{openbet.eventIds[4]}_{openbet.eventIds[1]}_{openbet.eventIds[0]}']";

                string nMarketSelector = selector + "[class*='selectionButton_']"; 
                List<IElementHandle> elements = page.QuerySelectorAllAsync(nMarketSelector).Result.ToList();
                if (elements.Count != 1)
                {
                    LogMng.Instance.onWriteStatus($"{elements.Count} markets found, we have to ignore");
                    return PROCESS_RESULT.ERROR;
                }
                
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 8"));
              
                RunScript($"document.querySelectorAll(\"{nMarketSelector}\")[0].click()");

                LogMng.Instance.onWriteStatus($"checking betslip new odd");

                int nMarketClickRetry = 0;
                while (nMarketClickRetry++ < 5)
                {
                    Thread.Sleep(300);
                    List<IElementHandle> slipElements = page.QuerySelectorAllAsync(selectorSpan).Result.ToList();
                    if (slipElements.Count == 1)
                    {                       
                                                
                        double newOdd = 0;
                                                
                        string GetOddresult = slipElements[0].GetInnerTextAsync().Result;    
                        LogMng.Instance.onWriteStatus($"GetOddresult in slip: {GetOddresult}");
                        newOdd = Utils.ParseToDouble(GetOddresult);
                            
                        if (newOdd == 0)
                        {
                            //LogMng.Instance.onWriteStatus(string.Format("Place bet 14"));
                            continue;
                        }

                        if (info.odds == 0)
                            info.odds = newOdd;
                        //LogMng.Instance.onWriteStatus(string.Format("Place bet 15"));
                        if (CheckOddDropCancelBet(newOdd, info))
                        {
                            //LogMng.Instance.onWriteStatus(string.Format("Place bet 16"));
                            removeBet();
                            return PROCESS_RESULT.SUSPENDED;
                        }

                        break;
                    }
                }
                
                if (nMarketClickRetry >= 5)
                {
                    removeBet();
                    LogMng.Instance.onWriteStatus("Get NewwOdd failed");
                    return PROCESS_RESULT.ERROR;
                }
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 17"));
                //info.odds = newOdd;
                int nSetStakeRetry = 0;
                while (nSetStakeRetry++ < 3)
                {
                    PageClick("input[data-qa='biglietto_puntata']");
                    for (int i = 0; i < 10; i++)
                    {
                        page.Keyboard.PressAsync("Backspace").Wait();
                        page.Keyboard.PressAsync("Delete").Wait();
                    }
                    page.Keyboard.TypeAsync(info.stake.ToString()).Wait();


                    string SetStakeResult = RunScript("document.querySelector(\"input[data-qa='biglietto_puntata']\").value");
                    LogMng.Instance.onWriteStatus($"SetStakeResult: {SetStakeResult}");
                    if (SetStakeResult == info.stake.ToString())
                    {
                        //LogMng.Instance.onWriteStatus(string.Format("Place bet 19"));
                        break;
                    }
                    Thread.Sleep(300);
                }

                
                string OddpanelOpening = RunScript("document.querySelectorAll('label[for=\"option_0\"]').length");
                LogMng.Instance.onWriteStatus(string.Format("Place bet OddpanelOpening: {0}", OddpanelOpening));
                if (OddpanelOpening != "1")
                {
                    RunScript("document.querySelectorAll('i[class*=\"ticketBetSetting_arrowIcon\"]')[0].click()");
                    Thread.Sleep(300);
                }

                RunScript("document.querySelectorAll('label[for=\"option_0\"]')[0].click()");                    
                Thread.Sleep(300);
                

                Thread.Sleep(500);
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 20"));
                int nClickBetRetry = 0;
                while (nClickBetRetry++ < 3)
                {
                    //LogMng.Instance.onWriteStatus(string.Format("Place bet 21 - {0}", nClickBetRetry));

                    wait_PlacebetResult = "";
                    wait_PlacebetResultEvent.Reset();

                    PageClick("button[data-qa='biglietto_scommetti']");


                    //LogMng.Instance.onWriteStatus(string.Format("Place bet 23"));
                    if (wait_PlacebetResultEvent.Wait(10000))
                    {
                        LogMng.Instance.onWriteStatus(string.Format("Place bet response: {0}", wait_PlacebetResult));
                        //LogMng.Instance.onWriteStatus(string.Format("Place bet 24"));
                        if (!wait_PlacebetResult.Contains("\"messaggio\":\"Operazione eseguita\""))                        
                        {
                            
                            //LogMng.Instance.onWriteStatus($"Place failed Response: {wait_PlacebetResult}");

                            //check if logout

                            //check if odd is changed
                            //LogMng.Instance.onWriteStatus(string.Format("Place bet 29 newOdd  {0}", newOdd));
                            //if (newOdd > 0)
                            //{
                            //    if (CheckOddDropCancelBet(newOdd, info))
                            //    {
                            //        //LogMng.Instance.onWriteStatus(string.Format("Place bet 30"));
                            //        removeBet();
                            //        return PROCESS_RESULT.SUSPENDED;
                            //    }
                            //    //info.odds = newOdd;
                            //}

                        }
                        else
                        {
                            //LogMng.Instance.onWriteStatus(string.Format("Place bet 32"));
                            RunScript("document.querySelector(\"button[class*='ticketReceiptSuccess_closeButton']\").click()");
                            Thread.Sleep(500);
                            return PROCESS_RESULT.PLACE_SUCCESS;
                        }
                    }

                    LogMng.Instance.onWriteStatus($"Click Placebutton retry {nClickBetRetry}");
                }
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 33"));
                removeBet();
                return PROCESS_RESULT.ERROR;
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("System.Threading.Tasks.Task.Wait"))
                {
                    RunBrowser();
                }
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 34"));
                removeBet();
                return PROCESS_RESULT.ERROR;
            }
        }

        bool CheckOddDropCancelBet(double newOdd, BetburgerInfo info)
        {
            LogMng.Instance.onWriteStatus($"Odd checking odd in pick: {info.odds} new odd: {newOdd}");
            if (newOdd > Setting.Instance.maxOddsSports || newOdd < Setting.Instance.minOddsSports)
            {
                LogMng.Instance.onWriteStatus($"Ignore bet because of odd is out of range, new odd: {newOdd}");
                return true;
            }

            if (Setting.Instance.bAllowOddDrop)
            {
                if (newOdd < info.odds)
                {
                    if (newOdd < info.odds - info.odds / 100 * Setting.Instance.dAllowOddDropPercent)
                    {
                        LogMng.Instance.onWriteStatus($"Ignore bet because of odd is dropped larger than {Setting.Instance.dAllowOddDropPercent}%: {info.odds} -> {newOdd}");
                        return true;
                    }
                }
            }
            else
            {
                if (newOdd < info.odds)
                {
                    LogMng.Instance.onWriteStatus($"Ignore bet because of odd is dropped {info.odds} -> {newOdd}");
                    return true;
                }
            }
            return false;
        }

        public void removeBet()
        {
            int nRetry = 0;
            while (nRetry++ < 3)
            {
                int nRetryButtonCount = 0;
                nRetryButtonCount = Utils.parseToInt(RunScript("document.querySelectorAll('button[data-qa=\"biglietto_svuota\"]').length"));
                if (nRetryButtonCount <= 0)
                    break;
                RunScript("document.querySelectorAll('button[data-qa=\"biglietto_svuota\"]')[0].click();");
                Thread.Sleep(500);
            }
        }

        public double getBalance()
        {
            return balance;

            //int nRetry = 0;
            //while (nRetry++ < 2)
            //{
            //    wait_PlacebetResultEvent.Reset();
            //    wait_PlacebetResult = "";

            //    string useragent = RunScript("navigator.userAgent");
            //    string command = "var xhr = new XMLHttpRequest();" +
            //                        "xhr.open('GET', 'https://areaprivata.sisal.it/api/movimenti-conto-ms/ms/account/movements/getBalance?ipAddress=&channel=62&requestId=" + Guid.NewGuid().ToString() + "&infoChannel=" + WebUtility.UrlEncode(useragent) + "', true);" +
            //                        "xhr.send(null);";
            //    RunScript(command);

            //    if (wait_PlacebetResultEvent.Wait(3000))
            //    {
            //        try
            //        {
            //            if (wait_PlacebetResult.Contains("SUCCESS"))
            //            {
            //                //LogMng.Instance.onWriteStatus("Balance url is called");

            //                dynamic details = JsonConvert.DeserializeObject<dynamic>(wait_PlacebetResult);
            //                double balance = details.balance.availableBalance / 100;

            //                LogMng.Instance.onWriteStatus($"Balance : {balance}");

            //                if (balance >= 0)
            //                    break;
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            LogMng.Instance.onWriteStatus($"GetBalance error; {ex}");
            //        }
            //    }
            //}
            //return -1;
        }


        Dictionary<string, int> sportsNumberBinding = new Dictionary<string, int>()
        {
            { "soccer", 1} ,
            { "tennis", 3} ,
            { "basketball", 2} ,
            { "volleyball", 5} ,
            { "table tennis", 60} ,
            { "hockey", 6} ,
        };

        private void GetLiveMatchList()
        {
            LogMng.Instance.onWriteStatus($"GetLiveMatchList 1");
            foreach (var sportsBinder in sportsNumberBinding)
            {
                List<MatchLinkData> newLinkArray = new List<MatchLinkData>();
                
                try
                {
                    HttpClient client = new HttpClient();

                    HttpResponseMessage responseMessageBetslip = client.GetAsync($"https://betting.sisal.it/api/lettura-palinsesto-sport/palinsesto/live/live-ora/{sportsBinder.Value}").Result;
                    responseMessageBetslip.EnsureSuccessStatusCode();
                    
                    string responseMessageBetfairString = responseMessageBetslip.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(responseMessageBetfairString))
                    {
                        return;
                    }

                    HtmlDocument doc = new HtmlDocument();
                    HtmlNode.ElementsFlags.Remove("form");
                    doc.LoadHtml(responseMessageBetfairString);
                    LogMng.Instance.onWriteStatus($"GetLiveMatchList 4");
                    IEnumerable<HtmlNode> nodeForms = doc.DocumentNode.Descendants("a").Where(node => node.Attributes["class"] != null && node.Attributes["class"].Value.Contains("regulator_description_"));
                    if (nodeForms == null || nodeForms.LongCount() < 1)
                    {
                        LogMng.Instance.onWriteStatus($"GetLiveMatchList 5");
                        return;
                    }
                    LogMng.Instance.onWriteStatus($"GetLiveMatchList 6");
                    List<HtmlNode> nodeForm = nodeForms.ToList();
                    if (nodeForm != null)
                    {       
                        foreach (var stemData in nodeForm)
                        {
                            try
                            {
                                   
                            }
                            catch { }
                        }                        
                    }
                    //string action = nodeForm.GetAttributeValue("action", "");
                    LogMng.Instance.onWriteStatus($"live match url refreshed: {newLinkArray.Count}");
                }
                catch { }

                try
                {
                    Monitor.Enter(lockerObj);
                    dictEventUrl[sportsBinder.Key] = newLinkArray;
                }
                catch{}
                finally
                {
                    Monitor.Exit(lockerObj);
                }
            }
        }
        public bool Pulse()
        {
            return false;            
        }
        
    }
#endif
}
