namespace Project.Bookie
{
#if (WPLAY)
    class Wplay : IBookieController
    {
        
        public HttpClient m_client = null;
        string domain = "";
        Object lockerObj = new object();
        //private static ProxyServer _proxyServer = null;
        string username = "";
        string sessionToken = "";
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


        public Wplay()
        {
            domain = "wplay.co";
            Global.placeBetHeaderCollection.Clear();
            m_client = initHttpClient();
            if (CDPController.Instance._browserObj == null)
                CDPController.Instance.InitializeBrowser("https://apuestas.wplay.co/es");
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

            /// set cursor on coords, and press mousefl
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
            if (getBalance() > 0)
                return true;

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
                        CDPController.Instance.loginRespBody = string.Empty;
                        CDPController.Instance.NavigateInvoke("https://apuestas.wplay.co/es");
                        Thread.Sleep(15000);

                        long documentId = CDPController.Instance.GetDocumentId().Result;
                        bool isFound = CDPController.Instance.FindAndClickElement(documentId, "input[name='username']").Result;
                        Thread.Sleep(1500);
                        CDPMouseController.Instance.InputText(Setting.Instance.username);

                        isFound = CDPController.Instance.FindAndClickElement(documentId, "input[name='password']").Result;
                        Thread.Sleep(1500);
                        CDPMouseController.Instance.InputText(Setting.Instance.password);

                        isFound = CDPController.Instance.FindAndClickElement(documentId, "input[class='log-in']").Result;
                        Thread.Sleep(2000);

                        int nRetry1 = 0;
                        while (string.IsNullOrEmpty(CDPController.Instance.loginRespBody))
                        {
                            if (!Global.bRun)
                                return false;

                            Thread.Sleep(500);
                            if (nRetry1 > 30)
                                break;

                            nRetry1++;
                        }

                        if (string.IsNullOrEmpty(CDPController.Instance.loginRespBody))
                        {
                            if (getBalance() > 0)
                                return true;
                            else
                                continue;
                        }

                        JObject jResp = JObject.Parse(CDPController.Instance.loginRespBody);
                        username = jResp["username"].ToString();
                        sessionToken = jResp["sessionToken"]["sessionToken"].ToString();
                        bLogin = true;

                        //Global.cookieContainer = new CookieContainer();
                        //Global.cookieContainer = CDPController.Instance.GetCoookies().Result;
                        break;
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

        public void PlaceBet(List<BetburgerInfo> info)
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
                    string addbet_script = Resources.wplay_addbet_query;

                    string payload = $"key=betslip.do_add_bet&bets=%5B%7B%22bet_ref%22%3A%22{info.direct_link}%22%2C%22src_code%22%3A%22EV_SELNS%22%7D%5D&forecast=N&origin=native";
                    addbet_script = addbet_script.Replace("[payload]", payload);

                    CDPController.Instance.AddBetRespBody = "";
                    CDPController.Instance.ExecuteScript(addbet_script);

                    int retryCnt = 0;
                    while (string.IsNullOrEmpty(CDPController.Instance.AddBetRespBody))
                    {
                        retryCnt++;
                        if (retryCnt > 30)
                            break;

                        Thread.Sleep(500);
                    }

                    if (string.IsNullOrEmpty(CDPController.Instance.AddBetRespBody))
                    {
                        LogMng.Instance.onWriteStatus("addbet request error");
                        return PROCESS_RESULT.ERROR;
                    }

                    //var postData = new StringContent(ReqJson, Encoding.UTF8, "application/json");
                    //HttpResponseMessage betResponse = m_client.PostAsync(betUrl, postData).Result;
                    string content = CDPController.Instance.AddBetRespBody;
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


                    string betUrl = $"https://{domain}/en/api/betslipcombo/limits";
                    string ReqJson = limit_req.ToString(Formatting.None).Replace("'", "");
                    string functionString = $"window.fetch('{betUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{ReqJson}', method: 'POST' }}).then(response => response.json());";

                    CDPController.Instance.PlaceBetRespBody = "";
                    CDPController.Instance.ExecuteScript(functionString);
                    
                    retryCnt = 0;
                    while (string.IsNullOrEmpty(CDPController.Instance.PlaceBetRespBody))
                    {
                        retryCnt++;
                        if (retryCnt > 30)
                            break;

                        Thread.Sleep(500);
                    }

                    if (string.IsNullOrEmpty(CDPController.Instance.PlaceBetRespBody))
                    {
                        LogMng.Instance.onWriteStatus("limits request error");
                        return PROCESS_RESULT.ERROR;
                    }
                                        
                    string limitcontent = CDPController.Instance.PlaceBetRespBody;
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

                    CDPController.Instance.PlaceBetRespBody = "";
                    CDPController.Instance.ExecuteScript(functionString);
                    retryCnt = 0;
                    while (string.IsNullOrEmpty(CDPController.Instance.PlaceBetRespBody))
                    {
                        retryCnt++;
                        if (retryCnt > 30)
                            break;

                        Thread.Sleep(500);
                    }

                    if (string.IsNullOrEmpty(CDPController.Instance.PlaceBetRespBody))
                    {
                        LogMng.Instance.onWriteStatus("updatebets request error");
                        return PROCESS_RESULT.ERROR;
                    }

                    content = CDPController.Instance.PlaceBetRespBody;
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

                    CDPController.Instance.PlaceBetRespBody = "";
                    CDPController.Instance.ExecuteScript(functionString);
                    retryCnt = 0;
                    while (string.IsNullOrEmpty(CDPController.Instance.PlaceBetRespBody))
                    {
                        retryCnt++;
                        if (retryCnt > 40)
                            break;

                        Thread.Sleep(500);
                    }

                    if (string.IsNullOrEmpty(CDPController.Instance.PlaceBetRespBody))
                    {
                        LogMng.Instance.onWriteStatus("placebet request error");
                        return PROCESS_RESULT.ERROR;
                    }

                    content = CDPController.Instance.PlaceBetRespBody;
                    dynamic placebets_res = JsonConvert.DeserializeObject<dynamic>(content);
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
                    CDPController.Instance.balanceRespBody = "";

                    string getBalanceURL = "https://apuestas.wplay.co/web_nr?key=login.go_check_login&poll=N&reason=user-request&is_stream_playing=N";
                        
                    string functionString = $"window.fetch('{getBalanceURL}', {{ method: 'GET', headers: {{ 'accept': 'application/json, text/javascript, */*; q=0.01', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7','x-requested-with':'XMLHttpRequest' }}, credentials: 'include', 'referrer': 'https://apuestas.wplay.co/es' , referrerPolicy: 'strict-origin-when-cross-origin'}}).then(response => response.json());";

                    CDPController.Instance.ExecuteScript(functionString);
                    int retryCnt = 0;
                    while (string.IsNullOrEmpty(CDPController.Instance.balanceRespBody))
                    {
                        retryCnt++;
                        if (retryCnt > 20)
                            break;

                        Thread.Sleep(500);
                    }
                   
                    LogMng.Instance.onWriteStatus("getbalance response " + CDPController.Instance.balanceRespBody);

                    JObject jsonContent = JObject.Parse(CDPController.Instance.balanceRespBody);
                    balance = Utils.ParseToDouble(jsonContent["balance"].ToString());
                }
            }
            catch (Exception ex)
            {
                //LogMng.Instance.onWriteStatus("Error in getBalance: " + ex.ToString());
            }
            return balance;
        }
    }
#endif
}
