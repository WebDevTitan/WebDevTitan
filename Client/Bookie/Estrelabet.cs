namespace Project
{
#if (ESTRELABET)
    class Estrelabet : IBookieController
    {
        public HttpClient m_client = null;
        string domain = "";
        Object lockerObj = new object();
        string device_id = "";
        string anonymous_id = "";
        //private static ProxyServer _proxyServer = null;

        public void Close()
        {

        }

        public void Feature()
        {

        }

        public int GetPendingbets()
        {
            int nResult = 0;
            try
            {
                string betUrl = $"https://{domain}/en/api/bets/open?_={Utils.getTick()}";
                HttpResponseMessage openbetResponse = m_client.GetAsync(betUrl).Result;
                string content = openbetResponse.Content.ReadAsStringAsync().Result;
                dynamic addbet_res = JsonConvert.DeserializeObject<dynamic>(content);
                nResult = addbet_res.data.Count;
            }
            catch { }
            return nResult;
        }
        public bool logout()
        {
            return true;
        }

        public Estrelabet()
        {
            domain = Setting.Instance.domain.ToLower();
      
            m_client = initHttpClient();
            //Global.SetMonitorVisible(false);
        }

        public bool Pulse()
        {
            if(CDPController.Instance._browserObj != null)
            {
                CDPController.Instance.ExecuteScript("document.getElementsByClassName('sb-btn sb-btn--block sb-btn--large sb-btn--success')[0].click()");
                Thread.Sleep(2000);
                string cur_url = CDPController.Instance.ExecuteScript("location.href", true);
                if (cur_url.Contains("pt-br/401"))
                    CDPController.Instance.NavigateInvoke("https://superbet.com");
                else
                    CDPController.Instance.NavigateInvoke("https://superbet.com/pt-br/profile/withdrawal");
            }
            return true;
        }
        public PROCESS_RESULT PlaceBet(List<BetburgerInfo> infos, out List<PROCESS_RESULT> result)
        {
            result = null;
            return PROCESS_RESULT.ERROR;
        }





        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }
        Random rand = new Random();
        public void MouseMove()
        {
            while (true)
            {
                SetForegroundWindow(Global.ViewerHwnd);

                Thread.Sleep(100);

                int x = rand.Next(0, (int)SystemParameters.WorkArea.Width);
                int y = rand.Next(0, (int)SystemParameters.WorkArea.Height);

                //SetCursorPos(x, y);

                //int lParam = y << 16 | x;
                //PostMessage(Global.ViewerHwnd, 0x0200, (IntPtr)0, (IntPtr)lParam);  //WM_MOUSEMOVE
                //PostMessage(Global.ViewerHwnd, 0x0084, (IntPtr)0, (IntPtr)lParam);  //WM_NCHITTEST
                //PostMessage(Global.ViewerHwnd, 0x0020, (IntPtr)Global.ViewerHwnd, (IntPtr)0x02000001);  //WM_SETCURSOR
            }
        }

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

#pragma warning disable 649
        internal struct INPUT
        {
            public UInt32 Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }

        internal struct MOUSEINPUT
        {
            public Int32 X;
            public Int32 Y;
            public UInt32 MouseData;
            public UInt32 Flags;
            public UInt32 Time;
            public IntPtr ExtraInfo;
        }

#pragma warning restore 649

        public bool login()
        {

            domain = Setting.Instance.domain.ToLower();
            if (CDPController.Instance._browserObj != null)
            {
                if (getBalance() > 0)
                    return true;
            }

            int nTotalRetry = 0;
            bool bLogin = false;
            while (nTotalRetry++ < 3)
            {
                if (!Global.bRun)
                    return false;
                try
                {
                    lock (lockerObj)
                    {
                        m_client = initHttpClient();

                        if (CDPController.Instance._browserObj == null)
                            CDPController.Instance.InitializeBrowser("https://www.estrelabet.com/");

                        CDPController.Instance.NavigateInvoke("https://www.estrelabet.com/");
                        Thread.Sleep(30000);
                        long documentId = CDPController.Instance.GetDocumentId().Result;


                        //Click Session Close button
                        //bool isFound = CDPController.Instance.FindAndClickElement(documentId, "button[class='capitalize btn btn--primary btn--block']").Result;


                        //Click Session Continue Button
                        //CDPController.Instance.ExecuteScript("document.getElementsByClassName('sb-btn sb-btn--block sb-btn--large sb-btn--success')[0].click()");

                        bool isFound = CDPController.Instance.FindAndClickElement(documentId, "button[class='site-btn site-btn__teritary site-btn--xs header-login-btn']").Result;
                        if (!isFound)
                        {
                            isFound = CDPController.Instance.FindAndClickElement(documentId, "div[class='login-banner mb-3 e2e-login-banner']").Result;
                        }
                        //CDPController.Instance.ExecuteScript("document.getElementsByClassName('sds-button sds-focus e2e-login sds-button--md sds-button--primary-color')[0].click()");
                        //Thread.Sleep(4000);
                        //CDPController.Instance.ExecuteScript("document.getElementsByClassName('sb-btn e2e-login capitalize sb-btn--medium sb-btn--ghost')[0].click();");
                        Thread.Sleep(2000);

                        //Input Username
                        isFound = CDPController.Instance.FindAndClickElement(documentId, "input[id='email']" , 3).Result;
                        if (isFound)
                            CDPMouseController.Instance.InputText(Setting.Instance.username);

                        //Input Password
                        isFound = CDPController.Instance.FindAndClickElement(documentId, "input[id='txtPassword']", 3).Result;
                        if (isFound)
                            CDPMouseController.Instance.InputText(Setting.Instance.password);

                        isFound = CDPController.Instance.FindAndClickElement(documentId, "button[id='loginButton']").Result;

                        int retryCnt = 0;
                        while (!CDPController.Instance.isLogged)
                        {
                            retryCnt++;
                            Thread.Sleep(500);
                            if (retryCnt > 30)
                                break;
                        }

                        bLogin = CDPController.Instance.isLogged;
                        if (!bLogin)
                            continue;

                        break;
                    }
                }
                catch (Exception e)
                {
                    LogMng.Instance.onWriteStatus($"login exception {e.StackTrace} {e.Message}");
                }
            }
            return bLogin;
        }
        public string getProxyLocation()
        {
            try
            {
                HttpResponseMessage resp = m_client.GetAsync("http://lumtest.com/myip.json").Result;
                var strContent = resp.Content.ReadAsStringAsync().Result;
                var payload = JsonConvert.DeserializeObject<dynamic>(strContent);
                return payload.ip.ToString() + " - " + payload.country.ToString();
            }
            catch (Exception ex)
            {
            }
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

#if USOCKS
            handler.Proxy = new WebProxy(string.Format("http://127.0.0.1:1080"));
            handler.UseProxy = true;
#elif OXYLABS
            handler.Proxy = new WebProxy(string.Format("pr.oxylabs.io:7777"));
         
            handler.Proxy.Credentials = new NetworkCredential(string.Format("customer-Iniciativasfrainsa-sesstime-30-cc-{0}-sessid-{1}", Setting.Instance.ProxyRegion, Global.ProxySessionID), "Goodluck123!@#");
            handler.UseProxy = true;
#endif
            handler.CookieContainer = Global.cookieContainer;

            HttpClient httpClientEx = new HttpClient(handler);
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;

            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.142 Safari/537.36");


            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"90\", \"Google Chrome\";v=\"90\"");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");

            httpClientEx.DefaultRequestHeaders.Add("Host", $"{domain}");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Referer", $"https://{domain}/");

            return httpClientEx;
        }

        bool CheckOddDropCancelBet(double newOdd, BetburgerInfo info)
        {

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
            if (Setting.Instance.bAllowOddRise)
            {
                if (newOdd > info.odds)
                {
                    if (newOdd > info.odds + info.odds / 100 * Setting.Instance.dAllowOddRisePercent)
                    {
                        LogMng.Instance.onWriteStatus($"Ignore bet because of odd is rise up larger than {Setting.Instance.dAllowOddRisePercent}%: {info.odds} -> {newOdd}");
                        return true;
                    }
                }
            }
            return false;
        }
        public PROCESS_RESULT PlaceBet(ref BetburgerInfo info)
        {
            Thread.Sleep(1000 * Setting.Instance.requestDelay);

            if (getBalance() < 0)
            {
                if (!login())
                    return PROCESS_RESULT.NO_LOGIN;
            }

#if OXYLABS
            retryCount = 5;
#endif

            try
            {
                lock (lockerObj)
                {
                    CDPController.Instance.ExecuteScript("document.getElementsByClassName('sb-btn sb-btn--block sb-btn--large sb-btn--success')[0].click()");

                    string eventId = Utils.Between(info.siteUrl, "e=", "&t=");
                    if (string.IsNullOrEmpty(eventId))
                        eventId = info.siteUrl.Split('-')[info.siteUrl.Split('-').Length - 1];

                    string outcomeId = "";
                    string sbValue = "";
                    string[] arr = info.direct_link.Split('&');
                    foreach(string str in arr)
                    {
                        if (str.Contains("outcomeId="))
                            outcomeId = str.Replace("outcomeId=", string.Empty).Trim();
                        else if(str.Contains("specialBetValue="))
                            sbValue = str.Replace("specialBetValue=", string.Empty).Trim();
                    }

                    //string outcomeId = Utils.Between(info.direct_link, "outcomeId=", "&marketId");
                    //string sbValue = Utils.Between(info.direct_link, "specialBetValue=", "&");

                    if (CDPController.Instance.sports.Count == 0 || CDPController.Instance.tours.Count == 0)
                        GetStruct();

                    if (string.IsNullOrEmpty(device_id) || string.IsNullOrEmpty(anonymous_id))
                        GetTokens();

                    SuperbetTicket ticket = new SuperbetTicket();
                    //SuperbetTicket ticket = FindEvent(eventId, outcomeId , sbValue);
                    if (ticket.items.Count == 0)
                    {
                        LogMng.Instance.onWriteStatus( "This Line is not existed now...");
                        return PROCESS_RESULT.ERROR;
                    }
                    double newOdds = Utils.ParseToDouble(ticket.items[0].value);
                    if (CheckOddDropCancelBet(newOdds, info))
                    {
                        LogMng.Instance.onWriteStatus(string.Format("Odd is changed from {0} To {1}", info.odds, newOdds.ToString("N2")));
                        return PROCESS_RESULT.MOVED;
                    }
                    ticket.total = info.stake;
                    int kk = 0;
                    while(kk < 3)
                    {
                        ticket.requestDetails.deviceId = device_id;
                        ticket.requestDetails.ldAnonymousUserKey = anonymous_id;

                        string betUrl = "https://api.web.production.betler.superbet.com/legacy-web/betting/submitticket?clientSourceType=Desktop_new";
                        string functionString = $"window.fetch('{betUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{JsonConvert.SerializeObject(ticket)}', method: 'POST' }}).then(response => response.json());";

                        CDPController.Instance.PlaceBetRespBody = string.Empty;
                        CDPController.Instance.ExecuteScript(functionString);

                        int retryCnt = 0;
                        while (string.IsNullOrEmpty(CDPController.Instance.PlaceBetRespBody))
                        {
                            retryCnt++;
                            if (retryCnt > 30)
                                break;

                            Thread.Sleep(500);
                        }

                        if (string.IsNullOrEmpty(CDPController.Instance.PlaceBetRespBody))
                        {
                            kk++;
                            continue;
                        }

                        JObject jResp = JObject.Parse(CDPController.Instance.PlaceBetRespBody);
                        if ((bool)jResp["error"])
                        {
                            if (jResp["errorCode"].ToString() == "liabilityLimitsExceeded")
                            {
                                LogMng.Instance.onWriteStatus($"Place bet Failed {jResp["notice"].ToString()}");
                                if (!Setting.Instance.bEnableMaxbetSuperbet)
                                {
                                    double maxStake = Utils.ParseToDouble(Utils.Between(jResp["notice"].ToString(), "Your bet was over the maximum amount you can place", "BRL").Trim());
                                    ticket.total = maxStake;
                                    ticket.ticketUuid = Utils.generateGuid();
                                    info.stake = maxStake;

                                    LogMng.Instance.onWriteStatus($"RePlace bet now!");
                                    kk++;
                                    continue;
                                }
                                else
                                {
                                    double maxStake = Utils.ParseToDouble(Utils.Between(jResp["notice"].ToString(), "Your bet was over the maximum amount you can place", "BRL").Trim());
                                    if(maxStake < Setting.Instance.MaxStakeLimit)
                                    {
                                        LogMng.Instance.onWriteStatus($"Max Stake is lower than Configured Value ({Setting.Instance.MaxStakeLimit})");
                                        return PROCESS_RESULT.ZERO_MAX_STAKE;
                                    }

                                    ticket.total = maxStake;
                                    ticket.ticketUuid = Utils.generateGuid();
                                    info.stake = maxStake;

                                    LogMng.Instance.onWriteStatus($"RePlace bet now!");
                                    kk++;
                                    continue;

                                }

                            }
                            else
                            {
                                LogMng.Instance.onWriteStatus($"Place bet Failed {jResp["errorCode"].ToString()}");
                                if(jResp["errorCode"].ToString() == "sessionNotValid")
                                {
                                    LogMng.Instance.onWriteStatus("Login Session is expired.");
                                    Thread.Sleep(2000);
                                    CDPController.Instance.ReloadBrowser();
                                }
                                return PROCESS_RESULT.ERROR;
                            }
                         
                        }
                        else
                        {
                            return PROCESS_RESULT.PLACE_SUCCESS;
                        }
                    }
                 
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"Place bet exception {ex.StackTrace} {ex.Message}");
            }

            LogMng.Instance.onWriteStatus(string.Format("** PLACE BET FAIL"));
            return PROCESS_RESULT.ERROR;
        }
        public double getBalance()
        {
            double balance = -1;
            try
            {
                lock (lockerObj)
                {
                    string balance_query = Resources.ResourceManager.GetString("estralebet_balance_query");
                    balance_query = balance_query.Replace("[balance_url]", "https://service.estrelabet.com//ajax/profile/getData");

                    CDPController.Instance.balanceRespBody = string.Empty;
                    CDPController.Instance.ExecuteScript(balance_query);
                    int retryCnt = 0;
                    while (string.IsNullOrEmpty(CDPController.Instance.balanceRespBody))
                    {
                        retryCnt++;
                        if (retryCnt > 20)
                            break;

                        Thread.Sleep(400);
                    }

                    JObject jResp = JObject.Parse(CDPController.Instance.balanceRespBody);
                    balance = (double)jResp["balanceDetails"]["cash"];
                }
            }
            catch (Exception ex)
            {
                //LogMng.Instance.onWriteStatus("Error in getBalance: " + ex.ToString());
            }
            return balance;
        }
        public EstraTicket FindEvent(string eventId, string outcomeId , string sbValue)
        {
            EstraTicket ticket = new EstraTicket();
            try
            {
                string fetchScript = Properties.Resources.estralebet_eventfind_query;
                fetchScript = fetchScript.Replace("[eventUrl]", $"https://sb2frontend-altenar2.biahosted.com/api/widget/GetEventDetails?culture=pt-BR&timezoneOffset=-480&integration=estrelabet&deviceType=1&numFormat=en-GB&countryCode=BR&eventId={eventId}");

                CDPController.Instance.eventRespBody = string.Empty;
                int retryCnt = 0;
                CDPController.Instance.ExecuteScript(fetchScript);
                while (string.IsNullOrEmpty(CDPController.Instance.eventRespBody))
                {
                    retryCnt++;
                    if (retryCnt > 20)
                        break;

                    Thread.Sleep(400);
                }

                JObject jResp = JObject.Parse(CDPController.Instance.eventRespBody);
                foreach (var jData in jResp["data"])
                {
                    TicketItem ticketItem = new TicketItem();

                    ticketItem.matchId = Utils.parseToLong(eventId);
                    ticketItem.matchName = jData["matchName"].ToString();
                    ticketItem.matchDate = jData["matchDate"].ToString();
                    ticketItem.matchDateUtc = jData["utcDate"].ToString();
                    ticketItem.tournamentId = jData["tournamentId"].ToString();
                    ticketItem.tournamentName = CDPController.Instance.tours.Find(t => t.id == ticketItem.tournamentId).tourname;
                    ticketItem.teamnameone = ticketItem.matchName.Split('·')[0].Trim();
                    ticketItem.teamnametwo = ticketItem.matchName.Split('·')[1].Trim();
                    ticketItem.teamId1 = jData["homeTeamId"].ToString();
                    ticketItem.teamId2 = jData["awayTeamId"].ToString();
                    ticketItem.sportId = (int)jData["sportId"];
                    ticketItem.sportName = CDPController.Instance.sports.Find(s => s.id == ticketItem.sportId.ToString()).name;
                    ticketItem.eventId = Utils.parseToLong(eventId);
                    ticketItem.eventCode = (long)jData["matchCode"];
                    ticketItem.eventUuid = jData["uuid"].ToString();
                    if (jData["metadata"] != null && jData["metadata"]["brId"] != null)
                        ticketItem.betRadarId = jData["metadata"]["brId"].ToString();
                    else if (jData["betradarId"] != null)
                        ticketItem.betRadarId = jData["betradarId"].ToString();

                    foreach (var jOdd in jData["odds"])
                    {
                        if (jOdd["outcomeId"].ToString() == outcomeId)
                        {
                            ticketItem.oddTypeId = (long)jOdd["outcomeId"];
                            if(jOdd["specialBetValue"] != null)
                            {
                                if (Utils.ParseToDouble(sbValue.Trim()) != Utils.ParseToDouble(jOdd["specialBetValue"].ToString()))
                                    continue;

                                ticketItem.sbValue = jOdd["specialBetValue"].ToString();
                            }

                            ticketItem.value = jOdd["price"].ToString();
                            ticketItem.oddFullName = jOdd["name"].ToString();
                            ticketItem.betGroupId = (long)jOdd["marketId"];
                            ticketItem.oddDescription = jOdd["info"].ToString();
                            ticketItem.oddId = (long)jOdd["outcomeId"];
                            ticketItem.oddUuid = jOdd["uuid"].ToString();
                            ticketItem.uuid = jOdd["uuid"].ToString();
                            ticketItem.marketUuid = jOdd["marketUuid"].ToString();
                            ticketItem.oddName = jOdd["name"].ToString();
                            ticketItem.marketName = jOdd["marketName"].ToString();

                            break;
                        }
                    }
                }

            }
            catch { }
            return ticket;
        }
        public void GetTokens()
        {
            try
            {
                device_id = CDPController.Instance.ExecuteScript("localStorage.device_id", true);
                anonymous_id = CDPController.Instance.ExecuteScript("localStorage.ldAnonymousUserKey", true);
            }
            catch { }
        }
        public void GetStruct()
        {
            try
            {
                string fetchScript = File.ReadAllText("GetFetch.txt");
                fetchScript = fetchScript.Replace("[url]", "https://production-superbet-offer-basic.freetls.fastly.net/sb-basic/api/v2/en-BR/struct");

                CDPController.Instance.eventRespBody = string.Empty;
                int retryCnt = 0;
                CDPController.Instance.ExecuteScript(fetchScript);

                while (string.IsNullOrEmpty(CDPController.Instance.eventRespBody))
                {
                    retryCnt++;
                    if (retryCnt > 20)
                        break;

                    Thread.Sleep(400);
                }

                JObject jResp = JObject.Parse(CDPController.Instance.eventRespBody);
                foreach (var jTour in jResp["data"]["tournaments"])
                {
                    SuperTour tour = new SuperTour();
                    tour.id = jTour["id"].ToString();
                    tour.tourname = jTour["localNames"]["en-BR"].ToString();

                    CDPController.Instance.tours.Add(tour);
                }

                foreach (var jSport in jResp["data"]["sports"])
                {
                    SuperSport sport = new SuperSport();

                    sport.id = jSport["id"].ToString();
                    sport.name = jSport["localNames"]["en-BR"].ToString();

                    CDPController.Instance.sports.Add(sport);
                }

            }
            catch { }
        }

    }
#endif

}
