namespace Project.Bookie
{
#if (BET365_BM)
    public class Bet365_BMCtrl : IBookieController
    {
        Thread monitorthread = null;

        public HttpClient m_client = null;
        private const double minMarketStake = 10;
        public Bet365_BMCtrl()
        {            
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"Bet365_BMCtrl Start");
#endif
            m_client = initHttpClient();


            monitorthread = new Thread(MonitorProc);
            monitorthread.Start();
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
            ServicePointManager.DefaultConnectionLimit = 2;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "es-ES,es;q=0.9");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            //httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;


            return httpClientEx;
        }

        public void Feature()
        {
            Global.RefreshPage();
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

        public void Close()
        {
            if (monitorthread != null)
                monitorthread.Abort();
        }

        Random rand = new Random();

        private bool PageIsVisible(string classVal)
        {
            try
            {                
                string result = Global.GetStatusValue($"return document.getElementsByClassName('{classVal}')[0].outerHTML").ToLower();

#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"PageIsVisible : {result}");
#endif

                if (result.Contains("class"))
                {
                    return true;
                }
            }
            catch { }
            return false;
        }
        private void PageClick(string classVal, int timeout = 500, int nRetry = 3)
        {
            while (nRetry-- > 0)
            {
                try
                {
                    double x, y;

                    Rect monitorRect = Global.GetMonitorPos();
                    double top = monitorRect.Top;
                    double left = monitorRect.Left;

                    string posResult = Global.GetStatusValue($"return JSON.stringify(document.getElementsByClassName('{classVal}')[0].getBoundingClientRect());");
                    Rect iconRect = Utils.ParseRectFromJson(posResult);
                    x = left + iconRect.X + iconRect.Width / 2;
                    y = top + iconRect.Y + iconRect.Height / 2;

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Click Pos (login dropdown): {x} {y}");
#endif

                    ClickOnPoint(Global.ViewerHwnd, new Point(x, y));

                    return;
                }
                catch { }
            }
        }
        public void MonitorProc()
        {
            while (true)
            {
                if (PageIsVisible("llm-LastLoginModule_Button "))
                {//closing for last login time(balance) popup
                    SetForegroundWindow(Global.ViewerHwnd);
                    PageClick("llm-LastLoginModule_Button ", 100, 1);
                    Thread.Sleep(500);
                }
                
                if (PageIsVisible("pm-MessageOverlayCloseButton "))
                {//closing for reading message popup
                    SetForegroundWindow(Global.ViewerHwnd);
                    PageClick("pm-MessageOverlayCloseButton ", 100, 1);
                    Thread.Sleep(500);
                }

                if (PageIsVisible("lqb-QuickBetHeader_DoneButton "))
                {//closing for placebet betslip result box
                    SetForegroundWindow(Global.ViewerHwnd);
                    PageClick("lqb-QuickBetHeader_DoneButton ", 100, 1);
                    Thread.Sleep(500);
                }

                if (PageIsVisible("alm-InactivityAlertRemainButton "))
                {//closing for inactivity alert popup
                    SetForegroundWindow(Global.ViewerHwnd);
                    PageClick("alm-InactivityAlertRemainButton ", 100, 1);
                    Thread.Sleep(500);
                }

                if (PageIsVisible("pm-FreeBetsPushGraphicCloseButton "))
                {//closing for freebet alert popup
                    SetForegroundWindow(Global.ViewerHwnd);
                    PageClick("pm-FreeBetsPushGraphicCloseButton ", 100, 1);
                    Thread.Sleep(500);
                }

                if (PageIsVisible("KeepCurrentLimitsButton "))
                {//closing for deposit limit popup
                    SetForegroundWindow(Global.ViewerHwnd);
                    PageClick("KeepCurrentLimitsButton ", 100, 1);
                    Thread.Sleep(500);
                }

                Thread.Sleep(500);
            }            
        }

        public int GetMyBetsCount()
        {
            int result = 0;
            result = Utils.parseToInt(Global.GetStatusValue("return document.getElementsByClassName('hm-HeaderMenuItemMyBets_MyBetsCount ')[0].innerText;"));
            return result;
        }

        public bool GetBalanceBeforeBet()
        {
            if (!login())
                return false;
            try
            {
                SetForegroundWindow(Global.ViewerHwnd);
                Thread.Sleep(100);

                double x, y;
                Rect monitorRect = Global.GetMonitorPos();
                double top = monitorRect.Top;
                double left = monitorRect.Left;
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Window Pos: {monitorRect.Left} {monitorRect.Top} {monitorRect.Width} {monitorRect.Height}");
#endif
                Global.RefreshPage();

                string posResult = Global.GetStatusValue("return JSON.stringify(document.getElementsByClassName('hm-MainHeaderMembersWide_MembersMenuIcon ')[0].getBoundingClientRect());").ToLower();
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"MemberIcon Pos: {posResult}");
#endif
                Rect iconRect = Utils.ParseRectFromJson(posResult);
                x = left + iconRect.X + iconRect.Width / 2;
                y = top + iconRect.Y + iconRect.Height / 2;

#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Click Pos: {x} {y}");
#endif
                SetCursorPos((int)x, (int)y);
                Thread.Sleep(500);
                Global.RunScriptCode("document.getElementsByClassName('hm-MainHeaderMembersWide_MembersMenuIcon ')[0].click();");

                Thread.Sleep(500);
                posResult = Global.GetStatusValue("return JSON.stringify(document.getElementsByClassName('um-BalanceRefreshButton ')[0].getBoundingClientRect());").ToLower();
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"RefreshIcon Pos: {posResult}");
#endif
                Rect refreshRect = Utils.ParseRectFromJson(posResult);
                x = left + refreshRect.X + refreshRect.Width / 2;
                y = top + refreshRect.Y + refreshRect.Height / 2;

#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Click Pos: {x} {y}");
#endif
                SetCursorPos((int)x, (int)y);
                Thread.Sleep(500);
                Global.RunScriptCode("document.getElementsByClassName('hm-BalanceRefreshButton ')[0].click();");

                Thread.Sleep(500);

                Global.balance = getBalance();
                return true;
            }
            catch (Exception ex){
                LogMng.Instance.onWriteStatus($"GetBalanceBeforeBet Exception {ex.Message} {ex.StackTrace}");
            }
            
            return false;
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
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus("Login start");
#endif
            int nTotalRetry = 0;
            while (nTotalRetry++ < 2 && Global.bRun)
            {
                try
                {
                    string result = Global.GetStatusValue("return Locator.user.isLoggedIn;").ToLower();
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"LonginStatus(1): {result}");
#endif
                    
                    if (result == "true")
                    {
                        Global.RefreshPage();
                        result = Global.GetStatusValue("return Locator.user.isLoggedIn;").ToLower();
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"LonginStatus(2): {result}");
#endif
                        if (result == "true")
                        {
                            LogMng.Instance.onWriteStatus("Already logined");
                            return true;
                        }
                    }
                    Global.RemoveCookies();
                    Global.RefreshPage();
                    
                    result = Global.GetStatusValue("return document.getElementsByClassName('hm-MainHeaderRHSLoggedOutWide_Login ')[0].outerHTML").ToLower();

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Login button status : {result}");
#endif
                    
                    if (!result.Contains("class"))
                    {
                        Global.LoadHomeUrl();

                        //check if page is loaded all
                        int nRetry1 = 0;
                        while (nRetry1 < 30)
                        {
                            Thread.Sleep(500);
                            result = Global.GetStatusValue("return document.getElementsByClassName('hm-MainHeaderRHSLoggedOutWide_Login ')[0].outerHTML").ToLower();

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


                   
                    Rect monitorRect = Global.GetMonitorPos();
                    double top = monitorRect.Top;
                    double left = monitorRect.Left;
                    SetForegroundWindow(Global.ViewerHwnd);

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"window left: {left} top: {top}");
#endif

                    Thread.Sleep(2000);
                    double x, y;
                    string posResult = Global.GetStatusValue("return JSON.stringify(document.getElementsByClassName('hm-MainHeaderRHSLoggedOutWide_Login ')[0].getBoundingClientRect());");
                    Rect iconRect = Utils.ParseRectFromJson(posResult);
                    x = left + iconRect.X + iconRect.Width / 2;
                    y = top + iconRect.Y + iconRect.Height / 2;

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Click Pos (login dropdown): {x} {y}");
#endif

                    ClickOnPoint(Global.ViewerHwnd, new Point(x, y));


                    
                    Thread.Sleep(1000);

                    posResult = Global.GetStatusValue("return JSON.stringify(document.getElementsByClassName('lms-StandardLogin_Username ')[0].getBoundingClientRect());");
                    iconRect = Utils.ParseRectFromJson(posResult);
                    x = left + iconRect.X + iconRect.Width / 2;
                    y = top + iconRect.Y + iconRect.Height / 2;                    

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Click Pos (username): {x} {y}");
#endif

                    ClickOnPoint(Global.ViewerHwnd, new Point(x, y));

                    SendKeys.SendWait(Setting.Instance.username);

                    
                    Thread.Sleep(1000);
                    posResult = Global.GetStatusValue("return JSON.stringify(document.getElementsByClassName('lms-StandardLogin_Password ')[0].getBoundingClientRect());");
                    iconRect = Utils.ParseRectFromJson(posResult);
                    x = left + iconRect.X + iconRect.Width / 2;
                    y = top + iconRect.Y + iconRect.Height / 2;                    

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Click Pos (password): {x} {y}");
#endif

                    ClickOnPoint(Global.ViewerHwnd, new Point(x, y));
                    
                    SendKeys.SendWait(Setting.Instance.password);
                    Thread.Sleep(1000);

                   
                    posResult = Global.GetStatusValue("return JSON.stringify(document.getElementsByClassName('lms-LoginButton_Text ')[0].getBoundingClientRect());");
                    iconRect = Utils.ParseRectFromJson(posResult);
                    x = left + iconRect.X + iconRect.Width / 2;
                    y = top + iconRect.Y + iconRect.Height / 2;

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Click Pos (login button): {x} {y}");
#endif


                    Thread.Sleep(1000);
   
                    ClickOnPoint(Global.ViewerHwnd, new Point(x, y));

                    //moveThread.Abort();

                    //                    Global.RunScriptCode("document.getElementsByClassName('hm-MainHeaderRHSLoggedOutWide_Login ')[0].click();");
                    //                    Thread.Sleep(500);
                    //#if (TROUBLESHOT)
                    //                    LogMng.Instance.onWriteStatus($"Longin Send Req");
                    //#endif
                    //                    string region = "";
                    //                    if (Setting.Instance.domain.ToLower().Contains(".es"))
                    //                    {
                    //                        region = "&txtLCNOVR=ES";
                    //                    }
                    //                    string command = "function getCookie(cname) {" +
                    //                                    "  var name = cname + '=';" +
                    //                                    "  var decodedCookie = decodeURIComponent(document.cookie);" +
                    //                                    "  var ca = decodedCookie.split(';');" +
                    //                                    "  for(var i = 0; i <ca.length; i++) {" +
                    //                                    "	var c = ca[i];" +
                    //                                    "	while (c.charAt(0) == ' ') {" +
                    //                                    "	  c = c.substring(1);" +
                    //                                    "	}" +
                    //                                    "	if (c.indexOf(name) == 0) {" +
                    //                                    "	  return c.substring(name.length, c.length);" +
                    //                                    "	}" +
                    //                                    "  }" +
                    //                                    "  return '';" +
                    //                                    "};" +
                    //                                    "var postContent = 'txtUsername=" + Setting.Instance.username + "&txtPassword=" + Setting.Instance.password + region + "&txtTKN=' + getCookie('pstk') + '&txtType=85&platform=1&AuthenticationMethod=0&txtScreenSize=1920%20x%201080&IS=11';" +
                    //                                    "var xhr = new XMLHttpRequest();" +
                    //#if (CHRISTIAN)
                    //                                    "xhr.open('POST', 'https://members.nj." + Setting.Instance.domain + "/members/lp/default.aspx', true);" +
                    //#else
                    //                                    "xhr.open('POST', 'https://members." + Setting.Instance.domain + "/members/lp/default.aspx', true);" +
                    //#endif
                    //                                    "xhr.withCredentials = true;" +
                    //                                    "xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');" +
                    //                                    "xhr.onreadystatechange = function() {" +
                    //                                    "  if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {" +
                    //                                    "	  window.location.reload();" +
                    //                                    "  }" +
                    //                                    "};" +
                    //                                    "xhr.send(postContent);";
                    //                    Global.RunScriptCode(command);

                    int nRetry = 0;
                    while (nRetry < 20)
                    {
                        Thread.Sleep(500);
                        nRetry++;
                        result = Global.GetStatusValue("return Locator.user.isLoggedIn;").ToLower();

                        if (result == "true")
                        {
                            LogMng.Instance.onWriteStatus("Login Successed");
                            return true;
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

            LogMng.Instance.onWriteStatus("Login Failed");
            return false;            
        }

        private void ClickMouseEvent()
        {
            Global.RunScriptCode("document.getElementsByClassName('hm-MainHeaderLHSNarrow_InPlayLabel ')[0].click();");
        }


        public void page_getBalance()
        {
            int nRetry = 0;
            double result = -1;

            while (nRetry++ < 2)
            {
                PageClick("hm-MainHeaderMembersWide_MembersMenuIcon ");
                Thread.Sleep(500);
                PageClick("um-BalanceRefreshButton_Icon ");
                Thread.Sleep(1000);
                try
                {
                    result = Utils.ParseToDouble(Global.GetStatusValue("return Locator.user.getBalance().totalBalance;"));
                }
                catch
                {

                }

                if (result > 0)
                {
                    Global.balance = result;
                    break;
                }
                Thread.Sleep(100);
            }

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"getBalance: {result}");
#endif

        }

        public PROCESS_RESULT PlaceBetInBrowser(BetburgerInfo info)
        {
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"Placebet action start");
#endif

            
            try
            {
                int nTotalRetry = 0;
                while (nTotalRetry < 2)
                {
                    nTotalRetry++;

                    string result = Global.GetStatusValue("return Locator.user.isLoggedIn;").ToLower();

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"[Placebet] LonginStatus: {result}");
#endif
                    if (result != "true")
                    {
                        if (!login())
                        {                            
                            return PROCESS_RESULT.NO_LOGIN;
                        }
                    }
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"[Placebet] SiteUrl: {info.siteUrl}");
#endif
                    
                    if (info.siteUrl.Contains("#"))
                    {
                        info.siteUrl = info.siteUrl.Substring(0, info.siteUrl.IndexOf("#"));
                    }

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"[Placebet] SiteUrl Modified: {info.siteUrl}");
#endif
                    

                    Global.OpenUrl(info.siteUrl);
                    //Thread.Sleep(600000);

                    int nRetry = 0;
                    while (nRetry < 30)
                    {
                        nRetry++;
                        string betslipStatus = Global.GetStatusValue("return BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.rememberedStake;").ToLower();
                        if (betslipStatus != "null")
                        {
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus("Placebet Betslip appeared");
#endif

                            break;
                        }
                        Thread.Sleep(500);
                    }
                    if (nRetry >= 30)
                    {
                        LogMng.Instance.onWriteStatus("addbet failed");
                        continue;
                    }
                    Thread.Sleep(1500);

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus("Placebet inputing stake");
#endif

                    
                    Global.RunScriptCode(string.Format("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.stakeBox.setStake(\"{0}\")", info.stake));
                    Thread.Sleep(500);

                    if (Setting.Instance.bEachWay && info.sport == "Horse Racing" && info.odds >= Setting.Instance.eachWayOdd)
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus("Placebet ticking e/w");
#endif

                        Global.RunScriptCode(string.Format("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.eachwayChecked()"));
                        Thread.Sleep(500);
                    }

                    Global.strPlaceBetResult = "";
                    Global.waitResponseEvent.Reset();

                    int nRetryPlacebet = 0;
                    while (nRetryPlacebet < 3)
                    {
                        nRetryPlacebet++;
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus("Placebet clicking placebet button..");
#endif

                        Thread.Sleep(100);
                        Global.RunScriptCode("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.betPlacement.placeBetButton.slipdelegate.acceptOnlyButtonValidate()");

                        Thread.Sleep(100);
                        Global.RunScriptCode("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.betPlacement.placeBetButton.slipdelegate.placeBetButtonValidateAndPlaceBet()");


                        if (Global.waitResponseEvent.Wait(20000))
                        {
                            BetSlipJson betSlipJson = null;
                            if (!string.IsNullOrEmpty(Global.strPlaceBetResult))
                            {
                                betSlipJson = JsonConvert.DeserializeObject<BetSlipJson>(Global.strPlaceBetResult);
                                if (betSlipJson.sr == 0)
                                {                                    
                                    return PROCESS_RESULT.PLACE_SUCCESS;
                                }
                            }

                            Thread.Sleep(1000);
                            string betslipState = Global.GetStatusValue("return BetSlipLocator.betSlipManager.betslip.activeModule.slip.currentState;").ToLower();

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
//            if (GetBalanceBeforeBet())
//            {
//                if (Global.balance < info.stake)
//                {
//#if (TROUBLESHOT)
//                    LogMng.Instance.onWriteStatus($"PlaceBet failed because of insufficient balance.");
//#endif
//                    return PROCESS_RESULT.SMALL_BALANCE;
//                }
//            }
//            else
//            {
//#if (TROUBLESHOT)
//                LogMng.Instance.onWriteStatus($"PlaceBet failed because of failing getBalance");
//#endif

//                return PROCESS_RESULT.ERROR;
//            }

            OpenBet_Bet365 openbet = Utils.ConvertBetburgerPick2OpenBet_365(infoList[0].betburgerInfo);            
            if (openbet == null)
            {
                if (infoList[0].betburgerInfo.kind == PickKind.Type_4)
                {
                    openbet = Utils.parseBet365BsString2OpenBet(infoList[0].betburgerInfo.eventTitle);

                    openbet.stake = Setting.Instance.stakePerTipster2;
                }
                else if (infoList[0].betburgerInfo.kind == PickKind.Type_5)
                {
                    try
                    {
                        openbet = JsonConvert.DeserializeObject<OpenBet_Bet365>(infoList[0].betburgerInfo.eventTitle);
                        openbet.stake = Setting.Instance.stakePerTipster2;
                        if (openbet.betData.Count > 0)
                        {
                            infoList[0].betburgerInfo.eventTitle = "Bet365 Tipster pick";
                            infoList[0].betburgerInfo.odds = openbet.betData[0].odd;
                            infoList[0].betburgerInfo.stake = openbet.stake;
                        }
                    }
                    catch (Exception ex)
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Deserialze Openbet failed {infoList[0].betburgerInfo.eventTitle} {ex}");
#endif
                    }

                    openbet.stake = Setting.Instance.stakePerTipster2;
                }
                else
                {
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Directlink error: {infoList[0].betburgerInfo.eventTitle} direct_link: {infoList[0].betburgerInfo.direct_link} siteurl: {infoList[0].betburgerInfo.siteUrl}");
#endif
                    Uri uriResult;
                    if (Uri.TryCreate(infoList[0].betburgerInfo.siteUrl, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                    {
                        infoList[0].result = PlaceBetInBrowser(infoList[0].betburgerInfo);
                        return;
                    }
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Siteurl invalid error: {infoList[0].betburgerInfo.eventTitle} siteurl: {infoList[0].betburgerInfo.siteUrl}");
#endif
                    infoList[0].result = PROCESS_RESULT.ERROR;
                    return;
                }
            }
            
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"Directlink {infoList[0].betburgerInfo.direct_link} Siteurl {infoList[0].betburgerInfo.siteUrl}");
#endif
            

            try
            {
                infoList[0].result = PlaceBet(openbet, infoList[0].betburgerInfo);
            }
            catch(Exception ex)
            {
                LogMng.Instance.onWriteStatus($"Placebet exception {ex}");
            }
            
            if (Global.PackageID == 1 && infoList[0].result == PROCESS_RESULT.PLACE_SUCCESS)
            {

                try
                {

                    page_getBalance();

                     
                    int nMyBetCount = GetMyBetsCount();

                    PlacedBetInfo betinfo = new PlacedBetInfo();
                    betinfo.bookmaker = infoList[0].betburgerInfo.extra;
                    betinfo.username = Setting.Instance.username;
                    betinfo.odds = infoList[0].betburgerInfo.odds;
                    betinfo.stake = infoList[0].betburgerInfo.stake;
                    betinfo.balance = Global.balance;
                    betinfo.percent = infoList[0].betburgerInfo.percent;
                    betinfo.sport = infoList[0].betburgerInfo.sport;
                    betinfo.outcome = infoList[0].betburgerInfo.outcome;
                    betinfo.eventTitle = infoList[0].betburgerInfo.eventTitle;
                    betinfo.homeTeam = infoList[0].betburgerInfo.homeTeam;
                    betinfo.awayTeam = infoList[0].betburgerInfo.awayTeam;
                    betinfo.bookmaker = infoList[0].betburgerInfo.extra;
                    betinfo.pendingBets = nMyBetCount;
                    UserMng.GetInstance().SendSuccessBetReport(betinfo);
                }
                catch { }
            }
        }
        
        public bool SendPlaceBetInBrowser(BetburgerInfo info)
        {
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus("Placebet action start");
#endif

            int nTotalRetry = 0;
            while (nTotalRetry++ < 3)
            {
                string result = Global.GetStatusValue("return Locator.user.isLoggedIn;").ToLower();

#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"[Placebet] LonginStatus: {result}");
#endif
                                
                if (result != "true")
                {
                    if (!login())
                        return false;
                }

                string betslipStatus = Global.GetStatusValue("return BetSlipLocator.betSlipManager.betslip.uid;").ToLower();
                if (betslipStatus == "null")
                {
                    Global.RunScriptCode("location.reload();");
                }

                int nRetry = 0;
                while (nRetry++ < 20)
                {
                    betslipStatus = Global.GetStatusValue("return BetSlipLocator.betSlipManager.betslip.uid;").ToLower();
                    if (betslipStatus != "null")
                    {
                        Global.RunScriptCode("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.deleteBet();"); //close betslip

#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus("Placebet Betslip Awaked from Gray Screen");
#endif
                        
                        break;
                    }
                    Thread.Sleep(500);
                }
                if (nRetry >= 20)
                {
                    LogMng.Instance.onWriteStatus("awaking from grayscreen failed");
                    continue;
                }
                
                betslipStatus = Global.GetStatusValue("return BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.rememberedStake;").ToLower();
                if (betslipStatus != "null")
                {
                    LogMng.Instance.onWriteStatus("already exist betslip");
                    Global.RunScriptCode("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.deleteBet();"); //close betslip
                    Thread.Sleep(500);
                }
                Global.waitResponseEvent.Reset();
                Global.RunScriptCode("document.getElementsByClassName('hip-OddsOnly gl-Participant_General ')[0].click();"); //click any one bet button
                Global.RunScriptCode("document.getElementsByClassName('ait-ParticipantOdds gl-Market_General-cn1 ')[0].click();"); //click any one bet button

                if (Global.waitResponseEvent.Wait(1000))
                {
                    return true;
                }           
            }
            return false;
        }
        public double getBalance()
        {
            int nRetry = 0;
            double result = -1;
            while (nRetry++ < 6)
            {
                try
                {
                    result = Utils.ParseToDouble(Global.GetStatusValue("return Locator.user.getBalance().totalBalance;"));
                }
                catch
                {

                }

                if (result > 0)
                    break;
                Thread.Sleep(1000);
            }

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"getBalance: {result}");
#endif

            return result;
        }
        public bool Pulse()
        {
            return true;
        }

        public PROCESS_RESULT PlaceBet(OpenBet_Bet365 betinfo, BetburgerInfo info)
        {
            PROCESS_RESULT SlipRes = PROCESS_RESULT.ERROR;
            
            string strBet365Result = string.Empty;

            int nRetry4SmallMarket = 3;

            double origStake = betinfo.stake;
            while (nRetry4SmallMarket > 0)
            {
                nRetry4SmallMarket--;

                string result = Global.GetStatusValue("return Locator.user.isLoggedIn;").ToLower();

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"[Placebet] LonginStatus: {result}");
#endif
                if (result != "true")
                {
                    if (!login())
                    {
                        LogMng.Instance.onWriteStatus($"[Placebet] Login failed");
                        return PROCESS_RESULT.NO_LOGIN;
                    }
                }

                string ns = "", ms = "";
                SlipRes = GetNsToken(ref ns, ref ms, ref betinfo, MAKE_SLIP_STEP.INIT, null);
                if (SlipRes == PROCESS_RESULT.ERROR)
                {
                    LogMng.Instance.onWriteStatus(string.Format("[InitBet] Step 1 Failed"));
                    return SlipRes;
                }

                int nRetry = 0;
                while (nRetry++ < 2)
                {
                    strBet365Result = doAddBet(ns, ms);
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"doAddBet Result: {strBet365Result}");
#endif
                    SlipRes = GetNsToken(ref ns, ref ms, ref betinfo, MAKE_SLIP_STEP.ADD_BET, strBet365Result);
                    info.direct_link = string.Format("{0}|{1}|{2}", betinfo.betData[0].i2, betinfo.betData[0].oddStr, betinfo.betData[0].fd);
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"new direct link(Addbet) : {info.direct_link} ns: {ns}");
#endif
                    if (SlipRes == PROCESS_RESULT.ERROR || SlipRes == PROCESS_RESULT.RE_FIXED)
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[AddBet] Failed"));
                        return SlipRes;
                    }
                    else if (SlipRes == PROCESS_RESULT.NO_LOGIN)
                    {
                        
                        LogMng.Instance.onWriteStatus(string.Format("[AddBet] Login again..."));
                        if (!login())
                        {
                            LogMng.Instance.onWriteStatus(string.Format("[AddBet] Step 3 Failed"));
                            return SlipRes;
                        }
                    }
                    else if (SlipRes == PROCESS_RESULT.MOVED)
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[AddBet] Retry because of changed odd(line)"));
                        SlipRes = GetNsToken(ref ns, ref ms, ref betinfo, MAKE_SLIP_STEP.INIT, null);
                        if (SlipRes == PROCESS_RESULT.ERROR)
                        {
                            LogMng.Instance.onWriteStatus(string.Format("[InitBet] Step 2 Failed"));
                            return SlipRes;
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

                    return SlipRes;
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

           
                strBet365Result = doPlaceBet(betinfo.betGuid, betinfo.cc, betinfo.pc, ns, ms);
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"doPlaceBet Result: {strBet365Result}");
#endif
                SlipRes = GetNsToken(ref ns, ref ms, ref betinfo, MAKE_SLIP_STEP.PLACE_BET, strBet365Result);
                info.direct_link = string.Format("{0}|{1}|{2}", betinfo.betData[0].i2, betinfo.betData[0].oddStr, betinfo.betData[0].fd);
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"new direct link(placebet) : {info.direct_link}");
#endif

                if (SlipRes == PROCESS_RESULT.PLACE_SUCCESS)
                {
                    LogMng.Instance.onWriteStatus($"[PlaceBet]! success! stake: {betinfo.stake} origStake: {origStake}");
                    //check if retrying for small markets
                    if (origStake - betinfo.stake >= 1)
                    {
                        origStake -= betinfo.stake;
                        info.stake = origStake;
                        if (origStake < betinfo.stake)
                        {
                            betinfo.stake = origStake;                        
                        }

                        nRetry4SmallMarket = 1;
                        
                        LogMng.Instance.onWriteStatus($"[PlaceBet] Retrying for small stake market cur stake : {betinfo.stake}");
                        Thread.Sleep(5000);
                        continue;
                    }
                    info.raw_id = betinfo.tr;
                    return SlipRes;
                }
                else if (SlipRes == PROCESS_RESULT.NO_LOGIN)
                {
                    
                    LogMng.Instance.onWriteStatus(string.Format("[PlaceBet] Login again..."));
                    if (!login())
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[PlaceBet] Step 3 Failed"));
                        return SlipRes;
                    }
                    return SlipRes;
                }
                else if (SlipRes == PROCESS_RESULT.MOVED)
                {
                    if (nRetry4SmallMarket <= 0)
                        break;

                    ns = ""; ms = "";
                    SlipRes = GetNsToken(ref ns, ref ms, ref betinfo, MAKE_SLIP_STEP.INIT, null);

                    strBet365Result = doAddBet(ns, ms);
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"doAddBet in Placebet Result: {strBet365Result}");
#endif
                    SlipRes = GetNsToken(ref ns, ref ms, ref betinfo, MAKE_SLIP_STEP.ADD_BET, strBet365Result);
                    info.direct_link = string.Format("{0}|{1}|{2}", betinfo.betData[0].i2, betinfo.betData[0].oddStr, betinfo.betData[0].fd);
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"new direct link(placebet/addbet) : {info.direct_link}");
#endif
                }
                else
                {
                    LogMng.Instance.onWriteStatus(string.Format("[PlaceBet] failed result: {0}", SlipRes));
                }
                
            }
            return SlipRes;
        }
        private string CallBet365(string actionUrl, FormUrlEncodedContent param)
        {
            int retryCount = 2;
#if OXYLABS
            retryCount = 5;
#endif

            while (--retryCount >= 0)
            {
                try
                {                    
                    HttpResponseMessage actionResponse = m_client.PostAsync(actionUrl, param).Result;
                    actionResponse.EnsureSuccessStatusCode();
                    string result = actionResponse.Content.ReadAsStringAsync().Result;

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"CallBet365 {actionUrl} [res] {result}");
#endif

                    return result;
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
            return string.Empty;
        }
        private string doPlaceBet(string betGuid, string bet_cc, string bet_pc, string ns, string ms)
        {
            if (string.IsNullOrEmpty(ns))
                return String.Empty;

//#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"doPlaceBet gid: {betGuid} cc: {bet_cc} pc: {bet_pc} ns: {ns} ms: {ms}");
//#endif

            //var s = {
            //    betRequestCorrelation: ""
            //    participantCorrelation: ""
            //    betGuid: ""
            //    normals: "pt=N#o=7/11#f=107654392#fp=2095221927#so=#c=1#mt=13#id=107654392-2095221927Y#|TP=BS107654392-2095221927#||",
            //    casts: "",
            //    multiples: "",
            //    completeHandler: function(t) {

            //    },
            //    errorHandler: function() {
            //    }
            //};
            //ns_betslipstandardlib_util.APIHelper.PlaceBet(s);


            betGuid += "&c=" + bet_cc + "&p=" + bet_pc;

            ClickMouseEvent();
            try
            {
                Global.strPlaceBetResult = "";
                Global.waitResponseEvent.Reset();
                //string command = $"var s = {{betGuid: '{betGuid}',participantCorrelation: '{bet_pc}',betRequestCorrelation: '{bet_cc}',normals: '{ns}',casts: '',multiples: '{ms}',completeHandler: function(t) {{}},errorHandler: function() {{}}}};ns_betslipstandardlib_util.APIHelper.PlaceBet(s);";
                                
                string command = $"ns_betslipcorelib_util.BetsWebApi.MakeApiReqiest('placebet?betGuid={betGuid}', {{normals: '{ns}',completeHandler: function(t) {{}},errorHandler: function() {{}}}});";
                if (!string.IsNullOrEmpty(ms))
                {
                    command = $"ns_betslipcorelib_util.BetsWebApi.MakeApiReqiest('placebet?betGuid={betGuid}', {{normals: '{ns}', multiples: '{ms}',completeHandler: function(t) {{}},errorHandler: function() {{}}}})";
                }

                Global.RunScriptCode(command);
                Global.waitResponseEvent.Wait(30000);

#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"doPlaceBet Res: {Global.strPlaceBetResult}");
#endif
                return Global.strPlaceBetResult;
            }
            catch (Exception ex){
                LogMng.Instance.onWriteStatus($"doPlaceBet Exception {ex}");
            }

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"doPlaceBet Res:(empty)");
#endif
            return string.Empty;
            //foreach (var name in Global.placeBetHeaderCollection.AllKeys)
            //{
            //    try
            //    {
            //        m_client.DefaultRequestHeaders.Remove(name);
            //    }
            //    catch { }
            //    m_client.DefaultRequestHeaders.TryAddWithoutValidation(name, Global.placeBetHeaderCollection[name]);
            //}

            //FormUrlEncodedContent postContent = convertStringIntoFormData(ns, ms, MAKE_SLIP_STEP.PLACE_BET);
            //string actionUrl = string.Format("https://www.{0}/BetsWebApi/placebet?betGuid={1}", Setting.Instance.domain, betGuid);
            //string result = CallBet365(actionUrl, postContent);

            ////for (char c = 'a'; c <= 'z'; c++)
            ////{
            ////    m_client.DefaultRequestHeaders.Remove($"PIRXTcSdwp-{c}");
            ////}
            //return result;
        }
        private string doConfirmBet(string betGuid, string ns, string ms)
        {
            if (string.IsNullOrEmpty(ns))
                return String.Empty;
            FormUrlEncodedContent postContent = convertStringIntoFormData(ns, ms, MAKE_SLIP_STEP.PLACE_BET);
            string actionUrl = string.Format("https://www.{0}/BetsWebAPI/confirmbet?betGuid={1}", Setting.Instance.domain, betGuid);
            return CallBet365(actionUrl, postContent);
        }
        private string doRefreshSlip(string ns, string ms)
        {
            if (string.IsNullOrEmpty(ns))
                return String.Empty;

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"doRefreshSlip ns: {ns} ms: {ms}");
#endif

            //FormUrlEncodedContent postContent = convertStringIntoFormData(ns, ms, MAKE_SLIP_STEP.REFRESH_BET);
            //string actionUrl = string.Format("https://www.{0}/BetsWebAPI/refreshslip", Setting.Instance.domain);
            //return CallBet365(actionUrl, postContent);

            //var s = {
            //    normals: "pt=N#o=7/11#f=107654392#fp=2095221927#so=#c=1#mt=13#id=107654392-2095221927Y#|TP=BS107654392-2095221927#||",
            //    casts: "",
            //    multiples: "",
            //    completeHandler: function(t) {

            //    },
            //    errorHandler: function() {
            //    }
            //};
            //ns_betslipstandardlib_util.APIHelper.AddBet(s);

            try
            {
                Global.strAddBetResult = "";
                Global.waitResponseEvent.Reset();
                string command = $"var s = {{normals: '{ns}',casts: '',multiples: '{ms}',completeHandler: function(t) {{}},errorHandler: function() {{}}}};ns_betslipstandardlib_util.APIHelper.RefreshSlip(s);";
                Global.RunScriptCode(command);
                Global.waitResponseEvent.Wait(10000);
                return Global.strAddBetResult;
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"doAddBet Exception {ex}");
            }
            return string.Empty;
        }
        private string doAddBet(string ns, string ms)
        {
            if (string.IsNullOrEmpty(ns))
                return String.Empty;

//#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"doAddBet ns: {ns} ms: {ms}");
//#endif



            //var s = {
            //    normals: "pt=N#o=7/11#f=107654392#fp=2095221927#so=#c=1#mt=13#id=107654392-2095221927Y#|TP=BS107654392-2095221927#||",
            //    casts: "",
            //    multiples: "",
            //    completeHandler: function(t) {

            //    },
            //    errorHandler: function() {
            //    }
            //};
            //ns_betslipstandardlib_util.APIHelper.AddBet(s);
            
            //ns_betslipcorelib_util.BetsWebApi.MakeApiReqiest(\"addbet\", {normals: \"" + ns + "\"})
            ClickMouseEvent();
            try
            {
                Global.strAddBetResult = "";
                Global.waitResponseEvent.Reset();
                //string command = $"var s = {{normals: '{ns}',casts: '',multiples: '{ms}',completeHandler: function(t) {{}},errorHandler: function() {{}}}};ns_betslipstandardlib_util.APIHelper.AddBet(s);";
                
                string command = $"ns_betslipcorelib_util.BetsWebApi.MakeApiReqiest('addbet', {{normals: '{ns}',casts: '',multiples: '{ms}',completeHandler: function(t) {{}},errorHandler: function() {{}}}});";
                Global.RunScriptCode(command);
                Global.waitResponseEvent.Wait(30000);
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"doAddBet Res: {Global.strAddBetResult}");
#endif
                return Global.strAddBetResult;
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"doAddBet Exception {ex}");
            }

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"doAddBet Res:(empty)");
#endif
            return string.Empty;

            //foreach (var name in Global.placeBetHeaderCollection.AllKeys)
            //{
            //    try
            //    {
            //        m_client.DefaultRequestHeaders.Remove(name);
            //    }
            //    catch { }
            //    m_client.DefaultRequestHeaders.TryAddWithoutValidation(name, Global.placeBetHeaderCollection[name]);
            //}

            //FormUrlEncodedContent postContent = convertStringIntoFormData(ns, ms, MAKE_SLIP_STEP.ADD_BET);
            //string actionUrl = string.Format("https://www.{0}/BetsWebApi/addbet", Setting.Instance.domain);
            //return CallBet365(actionUrl, postContent);
        }

        private FormUrlEncodedContent convertStringIntoFormData(string ns, string ms, MAKE_SLIP_STEP step)
        {
            try
            {
                var keyValues = new List<KeyValuePair<string, string>>();

                keyValues.Add(new KeyValuePair<string, string>());
                keyValues.Add(new KeyValuePair<string, string>("ns", ns));
                //LogMng.Instance.onWriteStatus(string.Format("MakeFormData ns {0}", ns));
                if (!string.IsNullOrEmpty(ms))
                {
                    keyValues.Add(new KeyValuePair<string, string>("ms", ms));
                    //LogMng.Instance.onWriteStatus(string.Format("MakeFormData ms {0}", ms));
                }

                keyValues.Add(new KeyValuePair<string, string>("betsource", "FlashInPLay"));

                if (step == MAKE_SLIP_STEP.PLACE_BET)
                    keyValues.Add(new KeyValuePair<string, string>("tagType", "WindowsDesktopBrowser"));

                if (step == MAKE_SLIP_STEP.REFRESH_BET)
                    keyValues.Add(new KeyValuePair<string, string>("cr", "1"));

                keyValues.Add(new KeyValuePair<string, string>("bs", "1"));

                return new FormUrlEncodedContent(keyValues);
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        private PROCESS_RESULT GetNsToken(ref string ns, ref string ms, ref OpenBet_Bet365 infos, MAKE_SLIP_STEP Step, string betSlipString)
        {
            BetSlipJson betSlipJson = null;
            try
            {                
                if (!string.IsNullOrEmpty(ms) && infos.betData.Count < 2)
                {
                    LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Error MsToken Exists, but betData count is less than 2"));
                    return PROCESS_RESULT.ERROR;
                }

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
                        infos.betGuid = betSlipJson.bg;
                    }

                    infos.cc = WebUtility.UrlEncode(betSlipJson.cc);
                    infos.pc = betSlipJson.pc;
                    if (string.IsNullOrEmpty(infos.cc))
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
                        infos.tr = betSlipJson.bt[0].tr;
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

            string re = "";
            try
            {
                for (int i = 0; i < infos.betData.Count; i++)
                {
                    if (infos.betData[i].eachway && Setting.Instance.bEachWay)
                        bEachWay = true;


                    if (Step == MAKE_SLIP_STEP.INIT)
                    {// have to last bet with "id" when add bet
                        if (i == infos.betData.Count - 1)
                            infos.betData[i].sa = $"id={infos.betData[i].fd}-{infos.betData[i].i2}Y";
                        else
                            infos.betData[i].sa = $"sa={calculateSA()}";
                    }
                    else
                    {// everything is sa
                        if (string.IsNullOrEmpty(infos.betData[i].sa) || infos.betData[i].sa.Contains("id="))
                            infos.betData[i].sa = $"sa={calculateSA()}";
                    }


                    if (betSlipJson != null)
                    {
                        if (betSlipJson.sr == 0)
                        {
                            for (int k = 0; k < betSlipJson.bt.Count; k++)
                            {
                                BetSlipItem betSlipItem = betSlipJson.bt[k];
                                if (betSlipItem.pt[0].pi != infos.betData[i].i2)
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
                                        infos.betData[i].sa = $"sa={betSlipItem.sa}";

                                    if (!string.IsNullOrEmpty(betSlipItem.od) && infos.betData[i].oddStr != betSlipItem.od)
                                    {
                                        LogMng.Instance.onWriteStatus(string.Format("Odd moved {0} -> {1}", infos.betData[i].oddStr, betSlipItem.od));
                                        infos.betData[i].oddStr = betSlipItem.od;
                                    }

                                    if (!string.IsNullOrEmpty(betSlipItem.pt[0].ha) && infos.betData[i].ht != betSlipItem.pt[0].ha)
                                    {
                                        LogMng.Instance.onWriteStatus(string.Format("line moved {0} -> {1}", infos.betData[i].ht, betSlipItem.pt[0].ha));
                                        infos.betData[i].ht = betSlipItem.pt[0].ha;
                                    }

                                    if (!string.IsNullOrEmpty(betSlipItem.oo))
                                        infos.betData[i].oo = betSlipItem.oo;

                                    if (betSlipItem.oc)
                                        infos.betData[i].oc = true;

                                    infos.betData[i].ea = betSlipItem.ea || betSlipItem.ew || betSlipItem.ex;
                                    infos.betData[i].ed = betSlipItem.ed;
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
                            if (infos.betData.Count == 1)
                            {
                                double maxStake = betSlipJson.bt[0].ms;
                                if (maxStake == 0)
                                {
                                    if (!string.IsNullOrEmpty(betSlipJson.bt[0].re) && Utils.ParseToDouble(betSlipJson.bt[0].re) > 0)
                                    {
                                        //re = betSlipJson.bt[0].re;
                                        infos.stake /= 2;
                                        if (infos.stake > minMarketStake)
                                            infos.stake = minMarketStake;
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
                                    infos.sl = true;
                                    infos.stake = maxStake;
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
                                    infos.sl = true;
                                    infos.stake = maxStake;
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
                                if (betSlipItem.pt[0].pi != infos.betData[i].i2)
                                {
                                    continue;
                                }

                                if (!string.IsNullOrEmpty(betSlipItem.sa))
                                    infos.betData[i].sa = $"sa={betSlipItem.sa}";

                                if (betSlipItem.ms == 0)
                                {
                                    re = betSlipItem.re;
                                }
                                else
                                {
                                    if (infos.stake <= betSlipItem.ms)
                                    {
                                        re = betSlipItem.re;
                                    }
                                    else
                                    {
                                        infos.sl = true;
                                        infos.stake = betSlipItem.ms;
                                    }
                                }

                                bool bOddChanged = false;
                                if (!string.IsNullOrEmpty(betSlipItem.od) && infos.betData[i].oddStr != betSlipItem.od)
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("Odd moved {0} -> {1}", infos.betData[i].oddStr, betSlipItem.od));
                                    bOddChanged = true;
                                    infos.betData[i].oddStr = betSlipItem.od;

                                    re = string.Empty;
                                }

                                if (!string.IsNullOrEmpty(betSlipItem.pt[0].ha) && infos.betData[i].ht != betSlipItem.pt[0].ha)
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("line moved {0} -> {1}", infos.betData[i].ht, betSlipItem.pt[0].ha));
                                    bOddChanged = true;
                                    infos.betData[i].ht = betSlipItem.pt[0].ha;
                                }

                                if (!string.IsNullOrEmpty(betSlipItem.oo))
                                    infos.betData[i].oo = betSlipItem.oo;

                                if (betSlipItem.oc)
                                    infos.betData[i].oc = true;

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


                    if (string.IsNullOrEmpty(infos.betData[i].ht))
                        ns += $"pt=N#o={infos.betData[i].oddStr}#f={infos.betData[i].fd}#fp={infos.betData[i].i2}#so=#c={infos.betData[i].cl}#mt=22#{infos.betData[i].sa}#";
                    else
                        ns += $"pt=N#o={infos.betData[i].oddStr}#f={infos.betData[i].fd}#fp={infos.betData[i].i2}#so=#c={infos.betData[i].cl}#ln={infos.betData[i].ht}#mt=22#{infos.betData[i].sa}#";

                    if (!string.IsNullOrEmpty(infos.betData[i].oo))
                        ns += $"oto={infos.betData[i].oo}#";

                    //if (infos.betData[i].oc)
                    //    ns += $"olc=1#";

                    ns += $"|TP=BS{infos.betData[i].fd}-{infos.betData[i].i2}#";

                    infos.betData[i].odd = Utils.FractionToDouble(infos.betData[i].oddStr);
                    infos.stake = Math.Truncate(infos.stake * 100) / 100;

                    if (infos.betData[i].odd == 0)
                        return PROCESS_RESULT.ERROR;

                    if (infos.betData.Count == 1 && betSlipJson != null)
                    {
                        double tr = infos.stake * infos.betData[i].odd + 0.0001;

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
                            if (infos.betData[i].odd < Setting.Instance.eachWayOdd)
                                bCheckEachwayLine = false;
                        }

                        ns = $"{ns}ust={infos.stake.ToString("N2")}#st={infos.stake.ToString("N2")}#";
                        if (infos.sl)
                            ns += $"sl={infos.stake.ToString("N2")}#";

                        if (bCheckEachwayLine && infos.betData[i].cl == "2" && infos.betData[i].ea && infos.betData[i].ed != 0)
                        {
                            tr += infos.stake * Utils.FractionToDoubleOfEachway(infos.betData[i].oddStr, infos.betData[i].ed);
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

                    ns += "||";
                }


                if (infos.betData.Count > 1 && betSlipJson != null && string.IsNullOrEmpty(ms))
                {
                    if (betSlipJson.dm != null && betSlipJson.dm.ea && bEachWay)
                        ms = $"id={betSlipJson.dm.bt}#bc={betSlipJson.dm.bc}#ust={infos.stake.ToString("N2")}#st={infos.stake.ToString("N2")}#|ew=1#||";
                    else
                        ms = $"id={betSlipJson.dm.bt}#bc={betSlipJson.dm.bc}#ust={infos.stake.ToString("N2")}#st={infos.stake.ToString("N2")}#||";

                    foreach (Dm dm in betSlipJson.mo)
                        ms += $"id={dm.bt}#bc={dm.bc}#||";
                }
            }
            catch (Exception e)
            {
            }

            return PROCESS_RESULT.SUCCESS;
        }
        private string calculateSA()
        {
            Random rnd = new Random();
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
