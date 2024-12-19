namespace Project.Bookie
{
#if (ELYSGAME)
    class ElysgameCtrl : IBookieController
    {
        private string domain = "https://play.elysgame.it/";
        public class PlacebetJson
        {
            public string action { get; set; } = "WagerService";
            public Bet bet { get; set; } = new Bet();

            public object freeBetToken = null;
        }

        public class Bet
        {
            public long timestampClient { get; set; } = Utils.getTick();
            public string clientTransactionId { get; set; } = "0";
            public long accountId { get; set; }
            public int stake { get; set; }
            public int acceptChangedOdd { get; set; } = 0;
            public int acceptOddToValue { get; set; } = 0;
            public int winAmount { get; set; }

            public bool reserved { get; set; } = false;

            public List<BetData> betDataList { get; set; } = new List<BetData>();

        }

        public class BetData
        {
            public int programCode { get; set; }
            public int eventCode { get; set; }
            public long eventDate { get; set; }
            public int betCode { get; set; }

            public List<int> resultCode { get; set; } = new List<int>();

            public int oddValue { get; set; }
            public List<int> additionalInfo { get; set; } = new List<int>();

        }
        public class LiveEventJson
        {
            public string description { get; set; }
            public Result result { get; set; }

        }

        public class Result
        {
            public int itemCount { get; set; }
            public List<OuterItemListItr> itemList { get; set; }
        }
        public class OuterItemListItr
        {
            public string discipline { get; set; }
            public int disciplineCode { get; set; }
            public List<InnerItemListItr> itemList { get; set; }

        }
        public class InnerItemListItr
        {
            public List<betGroup> betGroupList { get; set; } = new List<betGroup>();
            public EventInfo eventInfo { get; set; }
            public BreadCrumbInfo breadCrumbInfo { get; set; }
        }

        public class DetailedLiveEventJson
        {
            public string description { get; set; }
            public InnerItemListItr result { get; set; }

        }

        public class BreadCrumbInfo
        {
            public string fullUrl { get; set; }
        }
        public class EventInfo
        {
            public long eventCode { get; set; }
            public long programCode { get; set; }
            public long eventData { get; set; }
            public string aliasUrl { get; set; }
        }

        public class betGroup
        {
            public int betId { get; set; }
            public int layoutType { get; set; }
            public string betDescription { get; set; }

            public List<oddGroup> oddGroupList { get; set; } = new List<oddGroup>();

        }

        public class oddGroup
        {
            public string oddGroupDescription { get; set; }
            public string additionalDescription { get; set; }
            public string alternativeDescription { get; set; }
            public List<oddInfo> oddList { get; set; }
        }

        public class oddInfo
        {
            public int betCode { get; set; }
            public int oddValue { get; set; }
            public string oddDescription { get; set; }
            public int resultCode { get; set; }
            public string boxTitle { get; set; }
            public List<int> additionalInfo { get; set; } = new List<int>();
            public bool addInfo { get; set; }
            public bool multiBet { get; set; }
        }

        

        public HttpClient m_client = null;
        
        public string auth_token = "";
        public ElysgameCtrl()
        {
            m_client = initHttpClient();
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

        public bool Pulse()
        {
            return false;
        }
        public void Close()
        {

        }
        public void Feature()
        {

        }

        public int GetPendingbets()
        {
            return 0;
        }
        public bool logout()
        {
            return true;
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
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.142 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");

            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://play.elysgame.it");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://play.elysgame.it/");

            return httpClientEx;
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

        public static void ClickOnPoint(IntPtr wndHandle, Point clientPoint)
        {
            var oldPos = Cursor.Position;

            /// get screen coordinates
            ClientToScreen(wndHandle, ref clientPoint);

            /// set cursor on coords, and press mouse
            Cursor.Position = new System.Drawing.Point((int)clientPoint.X, (int)clientPoint.Y);

            var inputMouseDown = new INPUT();
            inputMouseDown.Type = 0; /// input type mouse
            inputMouseDown.Data.Mouse.Flags = 0x0002; /// left button down

            var inputMouseUp = new INPUT();
            inputMouseUp.Type = 0; /// input type mouse
            inputMouseUp.Data.Mouse.Flags = 0x0004; /// left button up

            var inputs = new INPUT[] { inputMouseDown, inputMouseUp };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

            /// return mouse 
            //Cursor.Position = oldPos;
        }

        public bool login()
        {
            
            //Global.RemoveCookies();
            bool bLogin = false;

            int nRetry = 0;
            while (nRetry++ < 3 && Global.bRun)
            {
                try
                {

                    Global.OpenUrl($"{domain}/login");


                    string result = Global.GetStatusValue("return localStorage.UserData;");
                    
                    if (string.IsNullOrEmpty(auth_token) || result.Contains("null"))
                    {
                        int nRetry1 = 0;
                        while (true)
                        {
                            try
                            {
                                while (nRetry1 < 20)
                                {
                                    Thread.Sleep(500);
                                    string htmlresult = Global.GetStatusValue("return document.getElementById('mat-input-0').outerHTML;");

#if (TROUBLESHOT)
                                    LogMng.Instance.onWriteStatus($"input username status : {htmlresult}");
#endif

                                    if (htmlresult.Contains("class"))
                                    {
                                        break;
                                    }
                                    nRetry1++;
                                }
                                if (nRetry1 >= 20)       //Page is loading gray page. let's retry
                                {
#if (TROUBLESHOT)
                                    LogMng.Instance.onWriteStatus($"Can't open login page");
#endif
                                    //Global.SetMonitorVisible(false);
                                    LogMng.Instance.onWriteStatus($"Login Failed because of can't open login page");
                                    return false;
                                }

                                //check if accept coockie button exists

                                string acceptbutton = Global.GetStatusValue("return document.evaluate(\"//span[text()='Accetta selezionati ']\", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.parentNode.outerHTML;");
                                if (acceptbutton.Contains("class"))
                                {
                                    Global.RunScriptCode("document.evaluate(\"//span[text()='Accetta selezionati ']\", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.parentNode.click();");
                                    continue;
                                }
                            }
                            catch { }
                            break;
                        }
                        Thread.Sleep(2000);

                        //Thread moveThread = new Thread(MouseMove);
                        //moveThread.Start();
                        Rect monitorRect = Global.GetMonitorPos();
                        double top = monitorRect.Top;
                        double left = monitorRect.Left;
                        SetForegroundWindow(Global.ViewerHwnd);

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"window left: {left} top: {top}");
#endif

                        Thread.Sleep(500);
                        double x, y;

                        string posResult = Global.GetStatusValue("return JSON.stringify(document.getElementById('mat-input-0').getBoundingClientRect());");
                        Rect iconRect = Utils.ParseRectFromJson(posResult);
                        x = left + iconRect.X + iconRect.Width / 2;
                        y = top + iconRect.Y + iconRect.Height / 2;
                        //SetCursorPos((int)x, (int)y);


                    LogMng.Instance.onWriteStatus($"Click Pos (username): {x} {y}");


                        int nClickRetry = 0;
                        while (nClickRetry++ < 2)
                        {
                            ClickOnPoint(Global.ViewerHwnd, new Point(x, y));
                            Thread.Sleep(500);                            
                        }

                        //for (int i = 0; i <20;i++)
                        //    SendKeys.SendWait("{backspace}");
                        SendKeys.SendWait(Setting.Instance.username);

                        //Global.RunScriptCode($"document.getElementById('userName').value='{Setting.Instance.username}';");

                        Thread.Sleep(1000);
                        posResult = Global.GetStatusValue("return JSON.stringify(document.getElementById('mat-input-1').getBoundingClientRect());");
                        iconRect = Utils.ParseRectFromJson(posResult);
                        x = left + iconRect.X + iconRect.Width / 2;
                        y = top + iconRect.Y + iconRect.Height / 2;
                        //SetCursorPos((int)x, (int)y);


                    LogMng.Instance.onWriteStatus($"Click Pos (password): {x} {y}");

                        nClickRetry = 0;
                        while (nClickRetry++ < 2)
                        {
                            ClickOnPoint(Global.ViewerHwnd, new Point(x, y));
                            Thread.Sleep(500);                            
                        }
                        
                        //Global.RunScriptCode($"document.getElementById('password').value='{Setting.Instance.password}';");

                        //for (int i = 0; i < 20; i++)
                        //    SendKeys.SendWait("{BACKSPACE}");
                        SendKeys.SendWait(Setting.Instance.password);
                        Thread.Sleep(1000);

                        //moveThread.Abort();


                        Global.strWebResponse1 = "";
                        Global.waitResponseEvent1.Reset();

                        posResult = Global.GetStatusValue("return JSON.stringify(document.getElementsByClassName('mat-focus-indicator mat-raised-button mat-button-base mat-primary')[0].getBoundingClientRect());");
                        iconRect = Utils.ParseRectFromJson(posResult);
                        x = left + iconRect.X + iconRect.Width / 2;
                        y = top + iconRect.Y + iconRect.Height / 2;


                    LogMng.Instance.onWriteStatus($"Click Pos (login button): {x} {y}");

                        ClickOnPoint(Global.ViewerHwnd, new Point(x, y));
                        //Global.RunScriptCode("document.getElementById('btnLoginPrimary').click();");

                        nRetry1 = 0;
                        if (Global.waitResponseEvent1.Wait(5000))
                        {
                            try
                            {
                                dynamic jsonResResp = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse1.Trim());
                                auth_token = jsonResResp.token_type.ToString() + " " + jsonResResp.access_token.ToString();
                            }
                            catch { }
                            result = Global.GetStatusValue("return localStorage.UserData;");
                            if (!result.Contains("null"))
                            {
                                bLogin = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        bLogin = true;
                    }

                }
                catch (Exception ex)
                {
                    LogMng.Instance.onWriteStatus($"Login exception {ex}");
                }
            }

            //LogMng.Instance.onWriteStatus($"Login Result: {bLogin}");
            //Global.SetMonitorVisible(false);
            return bLogin;
        }
        public PROCESS_RESULT PlaceBet(BetburgerInfo info)
        {
            OpenBet_Eurobet openbet = Utils.ConvertBetburgerPick2OpenBet_Eurobet(info);

            if (openbet == null)
            {
                LogMng.Instance.onWriteStatus("Converting OpenBet Error");
                return PROCESS_RESULT.ERROR;
            }

            int retryCount = 2;
//finding matches

            m_client.DefaultRequestHeaders.Remove("Authorization");
            m_client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", auth_token);

            HttpResponseMessage searchResult = m_client.GetAsync("https://odit-apielysgame.odissea-services.net/api/feeds/v2/prematch/search?language=undefined&scheduleTimeFrame=4&layoutModelId=23").Result;
            searchResult.EnsureSuccessStatusCode();

            string searchSportStr = searchResult.Content.ReadAsStringAsync().Result;
            JObject searchObj = JsonConvert.DeserializeObject<JObject>(searchSportStr);

            double mostSimilarity = 0;
            string mostSimilarityEventid = "";
            foreach (JObject categoryObj in searchObj["Sports"][0]["Categories"])
            {
                foreach (JObject tournamentObj in categoryObj["Tournaments"])
                {
                    foreach (JObject eventObj in tournamentObj["Events"])
                    {
                        double ratio1, ratio2;
                        double similarity = Similarity.GetSimilarityRatio(info.homeTeam + " - " + info.awayTeam, eventObj["Name"].ToString(), out ratio1, out ratio2);
                        if (mostSimilarity < similarity)
                        {
                            mostSimilarity = similarity;
                            mostSimilarityEventid = eventObj["Id"].ToString();
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(mostSimilarityEventid))
            {
                LogMng.Instance.onWriteStatus("Eventid is not found");
                return PROCESS_RESULT.ERROR;
            }

            string eventUrl = $"{domain}prematch/detail?eventId={mostSimilarityEventid}";
            Global.OpenUrl(eventUrl);
            while (--retryCount >= 0)
            {                
                try
                {
                    //string liveLink = string.Format("{0}/{1}", info.siteUrl.Replace("https://www.eurobet.it/it/scommesse-live/#!", "https://www.eurobet.it/live-detail-service/sport-schedule/services/event"), "?prematch=0&live=1");
                    string liveLink = "https://www.eurobet.it/live-homepage-service/sport-schedule/services/live-homepage/live?prematch=0&live=1";
                    HttpResponseMessage eventResponseMessage = m_client.GetAsync(liveLink).Result;
                    eventResponseMessage.EnsureSuccessStatusCode();

                    string eventContent = eventResponseMessage.Content.ReadAsStringAsync().Result;
                    LiveEventJson eventJson = JsonConvert.DeserializeObject<LiveEventJson>(eventContent);
                
                    string fullUrl = "";
                    
                    bool found = false;
                    long eventData = 0;

                    foreach (var itemOuterItr in eventJson.result.itemList)
                    {
                        if (found)
                            break;
                        foreach (var itemInnerItr in itemOuterItr.itemList)
                        {
                            if (itemInnerItr.eventInfo.programCode == openbet.programCode && itemInnerItr.eventInfo.eventCode == openbet.eventCode)
                            {
                                eventData = itemInnerItr.eventInfo.eventData;
                                fullUrl = itemInnerItr.breadCrumbInfo.fullUrl;
                                found = true;
                                break;
                            }
                        }
                    }

                    if (found == false)
                        return PROCESS_RESULT.MOVED;

                    
                    liveLink = string.Format("https://www.eurobet.it/live-detail-service/sport-schedule/services/event{0}/?prematch=0&live=1", fullUrl);
                    eventResponseMessage = m_client.GetAsync(liveLink).Result;
                    eventResponseMessage.EnsureSuccessStatusCode();

                    oddInfo selectInfo = null;
                    

                    eventContent = eventResponseMessage.Content.ReadAsStringAsync().Result;
                    DetailedLiveEventJson detailedEventJson = JsonConvert.DeserializeObject<DetailedLiveEventJson>(eventContent);

                    found = false;
                    foreach (var betGroupItr in detailedEventJson.result.betGroupList)
                    {
                        if (found)
                            break;
                        foreach (var oddOne in betGroupItr.oddGroupList)
                        {
                            if (found)
                                break;
                            foreach (oddInfo odd in oddOne.oddList)
                            {
                                if (odd.betCode == openbet.betCode && odd.resultCode == openbet.resultCode)
                                {                                    
                                    selectInfo = odd;
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (selectInfo == null)
                    {
                        LogMng.Instance.onWriteStatus("This line is changed or not existed now...");
                        return PROCESS_RESULT.MOVED;
                    }

                    //if (info.odds != (double)selectInfo.oddValue / 100)
                    //{
                    //    LogMng.Instance.onWriteStatus(string.Format("Odd is changed from {0} To {1}", info.odds, ((double)selectInfo.oddValue / 100).ToString("N2")));
                    //    return PROCESS_RESULT.MOVED;
                    //}

                    if (Setting.Instance.bAllowOddDrop)
                    {
                        if ((double)selectInfo.oddValue / 100 < info.odds)
                        {
                            if ((double)selectInfo.oddValue / 100 < info.odds - info.odds / 100 * Setting.Instance.dAllowOddDropPercent)
                            {
                                LogMng.Instance.onWriteStatus($"Ignore bet because of odd is dropped larger than {Setting.Instance.dAllowOddDropPercent}%: {info.odds} -> {((double)selectInfo.oddValue / 100).ToString("N2")}");
                                return PROCESS_RESULT.MOVED;
                            }
                        }
                    }
                    

                    info.odds = (double)selectInfo.oddValue / 100;
                    if (info.odds > Setting.Instance.maxOddsSports || info.odds < Setting.Instance.minOddsSports)
                    {
                        LogMng.Instance.onWriteStatus(string.Format("Odd is out of range, ignore {0}", info.odds));
                        return PROCESS_RESULT.MOVED;
                    }

                    PlacebetJson placebetJson = new PlacebetJson();
                    placebetJson.bet.acceptChangedOdd = 0;
                    placebetJson.bet.acceptOddToValue = 0;
                    placebetJson.bet.stake = (int)info.stake * 100;
                    placebetJson.bet.winAmount = (int)(info.stake * info.odds * 100);
                    placebetJson.bet.accountId = Utils.parseToInt("1");

                    BetData betData = new BetData();
                    betData.eventCode = openbet.eventCode;
                    betData.programCode = openbet.programCode;
                    betData.eventDate = eventData;
                    betData.betCode = selectInfo.betCode;
                    betData.oddValue = selectInfo.oddValue;
                    betData.resultCode.Add(selectInfo.resultCode);
                    betData.additionalInfo = selectInfo.additionalInfo;
                    placebetJson.bet.betDataList.Add(betData);

                    string placebetPostContent = JsonConvert.SerializeObject(placebetJson);

                    HttpResponseMessage placebetResponse = m_client.PostAsync("https://www.eurobet.it/sport-sale-service/internal-services/bet", new StringContent(placebetPostContent, Encoding.UTF8, "application/json")).Result;
                    placebetResponse.EnsureSuccessStatusCode();

                    string placebetResult = placebetResponse.Content.ReadAsStringAsync().Result;
                    LogMng.Instance.onWriteStatus($"Placebet Result: {placebetResult}");
                    JObject resultObj = JObject.Parse(placebetResult);
                    if (resultObj["description"].ToString().Contains("Success"))
                    {
                        return PROCESS_RESULT.PLACE_SUCCESS;
                    }
                    else if (resultObj["description"].ToString().Contains("Sessione non valida"))
                    {
                        return PROCESS_RESULT.NO_LOGIN;
                    }
                    else
                    {
                        LogMng.Instance.onWriteStatus(string.Format("Place Bet Error {0}", resultObj["description"].ToString()));

                        if (resultObj["description"].ToString().Contains("Giocata bloccata") ||
                            resultObj["description"].ToString().Contains("Scommessa Rifiutata") ||
                            resultObj["description"].ToString().Contains("Raggiunto Limite Ripetizioni Biglietto"))
                        {
                            return PROCESS_RESULT.CRITICAL_SITUATION;
                        }
                    }

                }
                catch (Exception e)
                {
#if OXYLABS
                    if (e.Message.Contains("An error occurred while sending the request"))
                    {
                        Global.ProxySessionID = new Random().Next().ToString();
                        m_client = initHttpClient(false);
                    }
#endif
                }
            }
            LogMng.Instance.onWriteStatus(string.Format("** PLACE BET FAIL"));
            return PROCESS_RESULT.ERROR;
        }
        public double getBalance()
        {
            double balance = -1;
            int retryCount = 2;

            while (--retryCount >= 0)
            {
                try
                {
                    m_client.DefaultRequestHeaders.Remove("Authorization");
                    m_client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", auth_token);

                    HttpResponseMessage balanceResponseMessage = m_client.GetAsync("https://odit-apielysgame.odissea-services.net/api/account/me/wallets").Result;
                    balanceResponseMessage.EnsureSuccessStatusCode();

                    string balanceContent = balanceResponseMessage.Content.ReadAsStringAsync().Result;
                    JObject balanceObj = JObject.Parse(balanceContent);

                    balance = Utils.ParseToDouble(balanceObj["UserWallets"][0]["CashBalance"].ToString());
                    break;
                }
                catch (Exception e)
                {

                }
            }
            return balance;
        }

    }
#endif
}
