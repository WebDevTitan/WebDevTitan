namespace Project.Bookie
{
#if (STOIXIMAN || BETANO)
    class StoiximanCtrl : IBookieController
    {
        
        public HttpClient m_client = null;
        string domain = "";
        Object lockerObj = new object();
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

        public StoiximanCtrl()
        {
            domain = Setting.Instance.domain.ToLower();
            if (!domain.StartsWith("www."))
                domain = "www." + domain;

#if (CHRISTIAN)
            domain = domain.Replace("www.", "br.");
#endif

            Global.placeBetHeaderCollection.Clear();
            m_client = initHttpClient();
            //Global.SetMonitorVisible(false);
        }

        public bool Pulse()
        {
            if (getBalance() < 0)
            {

                return false;
            }
            return true;
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
            domain = Setting.Instance.domain.ToLower();
            if (!domain.StartsWith("www."))
                domain = "www." + domain;

#if (CHRISTIAN)
            domain = domain.Replace("www.", "br.");
#endif

            int nTotalRetry = 0;
            //Global.SetMonitorVisible(true);
            bool bLogin = false;
            while (nTotalRetry++ < 2)
            { 
                if (!Global.bRun)
                    return false;
                try
                {

                    lock (lockerObj)
                    {
                        Global.RemoveCookies();
                        m_client = initHttpClient();



                        Global.OpenUrl($"https://{domain}/");

                        Global.waitResponseEvent.Reset();
                        Thread.Sleep(3000);
                        Global.OpenUrl($"https://{domain}/myaccount/login");

                        //Global.RunScriptCode("document.getElementsByClassName('uk-button uk-button-primary GTM-login')[0].click();");


                        if (Global.waitResponseEvent.Wait(10000))
                        {
                            //LogMng.Instance.onWriteStatus($"login page loaded");
                            ////Global.RunScriptCode($"document.getElementsByName('Username')[0].value='{Setting.Instance.username}';");
                            ////Global.RunScriptCode($"document.getElementsByName('Password')[0].value='{Setting.Instance.password}';");

                            //Thread.Sleep(1000);

                            ////Global.RunScriptCode("document.getElementsByClassName('button button--ripple button button--basic button--secondary')[0].removeAttribute('disabled');");
                            ////Global.RunScriptCode("document.getElementsByClassName('button button--ripple button button--basic button--secondary')[0].click();");


                            //string token = Global.strPlaceBetResult;

                            //var payload = JsonConvert.DeserializeObject<dynamic>(token);

                            //string token1 = payload.token1.ToString();
                            //string token2 = payload.token2.ToString();

                            //string command = "var valIoBlackbox = document.getElementById('ioBlackBox').value;" +
                            //            "var postContent = {'Username':'" + Setting.Instance.username + "','Password':'" + Setting.Instance.password + "','ParentUrl':'https://" + domain + "/','IoBlackBox':valIoBlackbox};" +
                            //            "var xhr = new XMLHttpRequest();" +
                            //            "xhr.open('POST', 'https://" + domain + "/myaccount/login?user=" + Setting.Instance.username + "');" +
                            //            "xhr.withCredentials = true;" +
                            //            "xhr.setRequestHeader('Accept', 'application/json, text/plain, */*');" +
                            //            "xhr.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');" +
                            //            "xhr.setRequestHeader('token1', '" + token1 + "');" +
                            //            "xhr.setRequestHeader('token2', '" + token2 + "');" +
                            //            "xhr.onreadystatechange = function() {" +
                            //            "  if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {" +
                            //            "	  window.location.reload();" +
                            //            "  }" +
                            //            "};" +
                            //            "xhr.send(JSON.stringify(postContent));";
                            //Global.RunScriptCode(command);
                            //Thread.Sleep(3000);
                            //Global.RefreshPage();


                            Rect monitorRect = Global.GetMonitorPos();
                            double top = monitorRect.Top;
                            double left = monitorRect.Left;
                            SetForegroundWindow(Global.ViewerHwnd);

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"window left: {left} top: {top}");
#endif

                            //Global.RunScriptCode("document.getElementById('dropdownLogin').click();");
                            Thread.Sleep(1000);
                            double x, y;
                            string posResult = Global.GetStatusValue("return JSON.stringify(document.getElementsByName('Username')[0].getBoundingClientRect());");
                            Rect iconRect = Utils.ParseRectFromJson(posResult);
                            x = left + iconRect.X + iconRect.Width / 2;
                            y = top + iconRect.Y + iconRect.Height / 2;


#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Click Pos (username): {x} {y}");
#endif

                            ClickOnPoint(Global.ViewerHwnd, new Point(x, y));

                            SendKeys.SendWait(Setting.Instance.username);

                            //Global.RunScriptCode($"document.getElementById('userName').value='{Setting.Instance.username}';");

                            Thread.Sleep(1000);
                            posResult = Global.GetStatusValue("return JSON.stringify(document.getElementsByName('Password')[0].getBoundingClientRect());");
                            iconRect = Utils.ParseRectFromJson(posResult);
                            x = left + iconRect.X + iconRect.Width / 2;
                            y = top + iconRect.Y + iconRect.Height / 2;


#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Click Pos (password): {x} {y}");
#endif

                            ClickOnPoint(Global.ViewerHwnd, new Point(x, y));

                            //Global.RunScriptCode($"document.getElementById('password').value='{Setting.Instance.password}';");
                            SendKeys.SendWait(Setting.Instance.password);
                            Thread.Sleep(1000);

                            //moveThread.Abort();


                            posResult = Global.GetStatusValue("return JSON.stringify(document.querySelectorAll('[data-qa*=\"submit\"]')[0].getBoundingClientRect());");
                            iconRect = Utils.ParseRectFromJson(posResult);
                            x = left + iconRect.X + iconRect.Width / 2;
                            y = top + iconRect.Y + iconRect.Height / 2;

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Click Pos (login button): {x} {y}");
#endif


                            Thread.Sleep(2000);
                            //SetCursorPos((int)x, (int)y);
                            ClickOnPoint(Global.ViewerHwnd, new Point(x, y));


                            int nRetry1 = 0;
                            while (nRetry1 < 3)
                            {
                                if (!Global.bRun)
                                    return false;

                                Thread.Sleep(8000);
                                //Task.Run(async () => await Global.GetCookie($"https://{domain}")).Wait();
                                if (getBalance() >= 0)
                                {
                                    return true;
                                }
                                nRetry1++;
                            }


                        }
                    }
                }
                catch (Exception e)
                {
                    LogMng.Instance.onWriteStatus($"login exception {e.StackTrace} {e.Message}");
                }
            }
            //Global.SetMonitorVisible(false);
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
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
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

        public void PlaceBet(List<CapsuledBetburgerInfo> info)
        {
            if (info.Count < 1)
                return;

            //info[0].result = PlaceBet(info[0].betburgerInfo);
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
                    string ReqJson = $"{{\"selectionIds\":[\"{info.direct_link}\"],\"betslip\":{{\"hash\":\"\",\"slipData\":\"\",\"legs\":[],\"bets\":[],\"betslipTabId\":1,\"betslipTrackId\":\"\"}}}}";
                    string betUrl = $"https://{domain}/en/api/betslip/v3/plain-leg/";
                    //LogMng.Instance.onWriteStatus($"plain-leg request : {ReqJson}");
                                        
                    string functionString = $"window.fetch('{betUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{ReqJson}', method: 'POST' }}).then(response => response.json());";

                    Global.strWebResponse2 = "";
                    Global.waitResponseEvent2.Reset();

                    Global.RunScriptCode(functionString);


                    if (!Global.waitResponseEvent2.Wait(20000) || string.IsNullOrEmpty(Global.strWebResponse2))
                    {
                        LogMng.Instance.onWriteStatus("plain-leg request error");
                        return PROCESS_RESULT.ERROR;
                    }

                    //var postData = new StringContent(ReqJson, Encoding.UTF8, "application/json");
                    //HttpResponseMessage betResponse = m_client.PostAsync(betUrl, postData).Result;
                    string content = Global.strWebResponse2;
                    dynamic addparts_res = JsonConvert.DeserializeObject<dynamic>(content);
                    LogMng.Instance.onWriteStatus($"plain-leg result : {content}");

                    JObject limit_req = new JObject();
                    limit_req["betslip"] = new JObject();
                    limit_req["betslip"]["hash"] = addparts_res.data.hash.ToString();
                    limit_req["betslip"]["slipData"] = addparts_res.data.slipData.ToString();
                    limit_req["betslip"]["legs"] = addparts_res.data.legs;
                    limit_req["betslip"]["bets"] = addparts_res.data.bets;
                    limit_req["betslip"]["betslipTabId"] = addparts_res.data.betSlipTabId.ToString();
                    limit_req["betslip"]["betslipTrackId"] = addparts_res.data.betslipTrackId.ToString();


                    limit_req["tag"] = addparts_res.data.bets[0].tag;
                    limit_req["type"] = addparts_res.data.bets[0].type;


                    betUrl = $"https://{domain}/en/api/betslipcombo/limits";
                    ReqJson = limit_req.ToString(Formatting.None).Replace("'", "");
                    functionString = $"window.fetch('{betUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{ReqJson}', method: 'POST' }}).then(response => response.json());";

                    Global.strPlaceBetResult = "";
                    Global.waitResponseEvent.Reset();

                    Global.RunScriptCode(functionString);

                    //LogMng.Instance.onWriteStatus($"limit request : {functionString}");
                    if (!Global.waitResponseEvent.Wait(20000) || string.IsNullOrEmpty(Global.strPlaceBetResult))
                    {
                        LogMng.Instance.onWriteStatus("limits request error");
                        return PROCESS_RESULT.ERROR;
                    }
                                        
                    string limitcontent = Global.strPlaceBetResult;
                    dynamic limit_res = JsonConvert.DeserializeObject<dynamic>(limitcontent);
                    LogMng.Instance.onWriteStatus($"limit result : {limitcontent}");
                    double maxstake = Utils.ParseToDouble(limit_res.data.max.ToString());

                    if (maxstake < info.stake)
                    {
                        LogMng.Instance.onWriteStatus($"maxbet ({maxstake}) is lower than setting value ({info.stake})");
                        return PROCESS_RESULT.ERROR;
                    }

                    JObject getbetslip_req = new JObject();
                                        
                    getbetslip_req["bets"] = addparts_res.data.bets;
                    getbetslip_req["bets"][0]["amount"] = info.stake;
                    getbetslip_req["betslip"] = new JObject();
                    getbetslip_req["betslip"]["bets"] = addparts_res.data.bets;
                    getbetslip_req["betslip"]["bets"][0]["amount"] = limit_res.data.max;
                    getbetslip_req["betslip"]["hash"] = addparts_res.data.hash.ToString();
                    getbetslip_req["betslip"]["slipData"] = addparts_res.data.slipData.ToString();
                    getbetslip_req["betslip"]["legs"] = addparts_res.data.legs;
                    getbetslip_req["betslip"]["betslipTabId"] = addparts_res.data.betSlipTabId.ToString();
                    getbetslip_req["betslip"]["betslipTrackId"] = addparts_res.data.betslipTrackId.ToString();
                    
           
                    
                    betUrl = $"https://{domain}/en/api/betslip/v3/updatebets";

                    //LogMng.Instance.onWriteStatus($"updatebets request : {getbetslip_req.ToString()}");
                    //HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), betUrl);
                    //request.Content = new StringContent(getbetslip_req.ToString(), Encoding.UTF8, "application/json");
                    //betResponse = m_client.SendAsync(request).Result;
                    //content = betResponse.Content.ReadAsStringAsync().Result;
                    ReqJson = getbetslip_req.ToString(Formatting.None).Replace("'", ""); 
                    functionString = $"window.fetch('{betUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{ReqJson}', method: 'PATCH' }}).then(response => response.json());";

                    Global.strWebResponse3 = "";
                    Global.waitResponseEvent3.Reset();

                    Global.RunScriptCode(functionString);


                    if (!Global.waitResponseEvent3.Wait(20000) || string.IsNullOrEmpty(Global.strWebResponse3))
                    {
                        LogMng.Instance.onWriteStatus("updatebets request error");
                        return PROCESS_RESULT.ERROR;
                    }

                    content = Global.strWebResponse3;
                    //LogMng.Instance.onWriteStatus($"updatebets request : {functionString}");
                    LogMng.Instance.onWriteStatus($"updatebets result : {content}");
                    dynamic updatebets_res = JsonConvert.DeserializeObject<dynamic>(content);
                    

                    JObject placebet_req = new JObject();
                    
                    placebet_req["betslip"] = new JObject();
                    placebet_req["betslip"]["hash"] = updatebets_res.data.hash.ToString();
                    placebet_req["betslip"]["slipData"] = updatebets_res.data.slipData.ToString();
                    placebet_req["betslip"]["legs"] = updatebets_res.data.legs;
                    placebet_req["betslip"]["bets"] = updatebets_res.data.bets;
                    placebet_req["betslip"]["betslipTabId"] = updatebets_res.data.betSlipTabId.ToString();
                    placebet_req["betslip"]["betslipTrackId"] = updatebets_res.data.betslipTrackId;
                    placebet_req["betslip"]["oddschanges"] = "1";

                    double newodd = info.odds;
                    try
                    {
                        newodd = Convert.ToDouble(updatebets_res.data.bets[0].odds.ToString());
                        if (CheckOddDropCancelBet(newodd, info))
                        {

                            LogMng.Instance.onWriteStatus($"Lower odd, cancelled. ({info.odds}) -> ({newodd})");
                            return PROCESS_RESULT.ERROR;
                        }
                    }
                    catch { }
                    //LogMng.Instance.onWriteStatus($"placebet request : {placebet_req.ToString()}");

                    betUrl = $"https://{domain}/en/api/betslip/v3/place";
                    ReqJson = placebet_req.ToString(Formatting.None).Replace("'", "");
                    functionString = $"window.fetch('{betUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{ReqJson}', method: 'POST' }}).then(response => response.json());";

                    Global.strWebResponse4 = "";
                    Global.waitResponseEvent4.Reset();

                    Global.RunScriptCode(functionString);


                    if (!Global.waitResponseEvent4.Wait(20000) || string.IsNullOrEmpty(Global.strWebResponse4))
                    {
                        LogMng.Instance.onWriteStatus("placebet request error");
                        return PROCESS_RESULT.ERROR;
                    }

                    content = Global.strWebResponse4;

                    //postData = new StringContent(placebet_req.ToString(), Encoding.UTF8, "application/json");
                    
                    //betResponse = m_client.PostAsync(betUrl, postData).Result;
                    //content = betResponse.Content.ReadAsStringAsync().Result;
                    dynamic placebets_res = JsonConvert.DeserializeObject<dynamic>(content);
                    //LogMng.Instance.onWriteStatus($"placebet request : {functionString}");
                    LogMng.Instance.onWriteStatus($"placebet result : {content}");
                    
                    if (placebets_res.data.ToString() != "null")
                    {
                        try { 
                            if (placebets_res.errorCode != null)
                            {
                                LogMng.Instance.onWriteStatus(string.Format("Place bet failed. {0} ", placebets_res.errorCode.ToString()));

                                return PROCESS_RESULT.ERROR;
                            }
                        }
                        catch { }
                        return PROCESS_RESULT.PLACE_SUCCESS;
                    }
                    //else if (placebets_res.error.error.ToString() == "998")
                    //{
                    //    bool bRet = login();
                    //    if (!bRet)
                    //    {
                    //        LogMng.Instance.onWriteStatus(string.Format("Bet failed(Need relogin)"));
                    //        return PROCESS_RESULT.NO_LOGIN;
                    //    }
                    //    LogMng.Instance.onWriteStatus(string.Format("Retry after relogin"));
                    //}
                    else
                    {
                        LogMng.Instance.onWriteStatus(string.Format("Place bet failed. {0} ", placebets_res.errors[0].ToString()));

                        return PROCESS_RESULT.ERROR;
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
                    Global.strWebResponse1 = "";
                    Global.waitResponseEvent1.Reset();

                    string getBalanceURL = "";
                    string baseURL = "https://" + domain;
                    if (domain.Contains("betano"))
                    {
                        getBalanceURL = $"https://{domain}/en/myaccount/api/ma/customer/balance";
                    }
                    else
                    {
                        getBalanceURL = $"https://{domain}/api/balance?_={Utils.getTick()}";
                    }
                        
                    string functionString = $"window.fetch('{getBalanceURL}', {{ method: 'GET', headers: {{ 'accept': '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json', }}, credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', referrer: '{baseURL}' }}).then(response => response.json());";

                    Global.RunScriptCode(functionString);

                    if (!Global.waitResponseEvent1.Wait(3000))
                    {
                        LogMng.Instance.onWriteStatus("getbalance Error no response");
                        throw new Exception();
                    }
                    string content = Global.strWebResponse1;
                    LogMng.Instance.onWriteStatus("getbalance response " + content);

                    dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(content);
                    if (domain.Contains("betano"))
                    {                        
                        string RealUserBalance = jsonContent.Result[0].BettingBalance.ToString().Replace("лв.", "").Replace("R$", "").Trim();
#if (CHRISTIAN)
                        RealUserBalance = RealUserBalance.Replace(".", "").Replace(",", ".");
#endif
                        balance = Utils.ParseToDouble(RealUserBalance);
                    }
                    else
                    {                        
                        string RealUserBalance = jsonContent.data.balances[0].bettingBalance.ToString();
                        balance = Utils.ParseToDouble(RealUserBalance);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus("Error in getBalance: " + ex.ToString());
            }
            return balance;
        }
    }
#endif
}
