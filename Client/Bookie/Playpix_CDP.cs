namespace Project.Bookie
{
#if (PLAYPIX)
    class Playpix_CDP : IBookieController
    {
        
        public HttpClient m_client = null;
        string domain = "";
        Object lockerObj = new object();
        private WebSocket _webSocket = null;

        private string sport_resp = string.Empty;
        private string event_resp = string.Empty;
        private bool isLoggedIn = false;
        private string betslip_resp = string.Empty;
        private string placebet_resp = string.Empty;
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

        public Playpix_CDP()
        {
            domain = "playpix.com";
            if (!domain.StartsWith("www."))
                domain = "www." + domain;

            Global.placeBetHeaderCollection.Clear();
            m_client = initHttpClient();
            if (Setting.Instance.browserType == 0)
            {
                if (CDPController.Instance._browserObj == null)
                    CDPController.Instance.InitializeBrowser($"https://{domain}/en");
            }
            else
            {
                if (DolphinController.Instance.browser == null)
                    DolphinController.Instance.InitBrowser($"https://{domain}/en");
            }
            //Global.SetMonitorVisible(false);
        }

        public void StartListening()
        {
            try 
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                _webSocket = new WebSocket("wss://eu-swarm-newm.hogoxiyfctcdpjbu.com");

                _webSocket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                _webSocket.Origin = "https://www.playpix.com";
                _webSocket.OnOpen += Socket_OnOpen;
                _webSocket.OnMessage += Socket_OnMessage;
                _webSocket.OnClose += Socket_OnClose;
                _webSocket.OnError += Socket_OnError;
                _webSocket.Compression = CompressionMethod.None;
                _webSocket.EmitOnPing = true;
                _webSocket.Connect();
            }
            catch { }
        }
        private void Socket_OnOpen(object sender, EventArgs e)
        {
            LogMng.Instance.onWriteStatus("Socket_OnOpen");
            if (Setting.Instance.browserType == 0)
            {
                foreach (string line in CDPController.Instance.websocket_request_contents)
                {
                    _webSocket.Send(line);
                }

                Thread.Sleep(700);
                if (!string.IsNullOrEmpty(CDPController.Instance.user_identify_message))
                    _webSocket.Send(CDPController.Instance.user_identify_message);

                Thread.Sleep(700);
                if (!string.IsNullOrEmpty(CDPController.Instance.restore_login_message))
                    _webSocket.Send(CDPController.Instance.restore_login_message);
            }
            else
            {
                foreach (string line in DolphinController.Instance.websocket_request_contents)
                    _webSocket.Send(line);
                

                Thread.Sleep(700);
                if (!string.IsNullOrEmpty(DolphinController.Instance.user_identify_message))
                    _webSocket.Send(DolphinController.Instance.user_identify_message);

                Thread.Sleep(700);
                if (!string.IsNullOrEmpty(DolphinController.Instance.restore_login_message))
                    _webSocket.Send(DolphinController.Instance.restore_login_message);
            }
                

        }
        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {

            try
            {
                string strBody = e.Data.ToString().Substring(2);
                if (strBody.Contains("auth_token") || strBody.Contains("user_id"))
                {
                    LogMng.Instance.onWriteStatus("Logged in into Socket!");
                    isLoggedIn = true;
                }
                else if (strBody.Contains("c9bee840a414e09380d3051903eac49048252bfb"))
                    sport_resp = strBody;
                else if (strBody.Contains("804c2dcf88cd9d7145a883d1a2e6e1c34507c472"))
                    event_resp = strBody;
                else if (strBody.Contains("08b0df00c1cab143a465736247629ae5746451da"))
                    betslip_resp = strBody;
                else if (strBody.Contains("a02d3882a48db6158ca3d1044fe95ff69238f661"))
                    betslip_resp = strBody;
                else if (strBody.Contains("8493ba482943f7cf4ed107202cfc05aa51a9482b"))
                    betslip_resp = strBody;

                //LogMng.Instance.onWriteStatus(strBody);


            }
            catch (Exception ex)
            {
                //m_handlerWriteStatus("Exception in socket message: " + ex.ToString());
            }
        }

        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            LogMng.Instance.onWriteStatus("***Socket_OnClose***");
            //m_handlerWriteStatus("Socket_OnClose");
            //m_handlerWriteStatus(e.Reason);
        }

        private void Socket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            LogMng.Instance.onWriteStatus("***Socket_OnError***");
            //m_handlerWriteStatus("Socket_OnError");
            //m_handlerWriteStatus(e.Message.ToString());
        }

        public void CloseWebSocket()
        {

            if (_webSocket.ReadyState == WebSocketState.Open)
                _webSocket.Close();
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
            bool bLogin = false;
            if (Setting.Instance.browserType == 0)
                bLogin = login_cdp();
            else
                bLogin = login_puputter();

            return bLogin;
        }

        public bool login_cdp()
        {
            domain = "playpix.com";
            if (!domain.StartsWith("www."))
                domain = "www." + domain;

            Thread.Sleep(5000);

            if (getBalance() > 0)
            {
                CDPController.Instance.ReloadBrowser();
                Thread.Sleep(7000);

                StartListening();
                return true;
            }

            int nTotalRetry = 0;
            bool bLogin = false;
            while (nTotalRetry++ < 2)
            {
                if (!Global.bRun)
                    return false;
                try
                {

                    lock (lockerObj)
                    {
                        m_client = initHttpClient();

                        CDPController.Instance.NavigateInvoke($"https://{domain}/en");

                        Thread.Sleep(10000);
                        long documentId = CDPController.Instance.GetDocumentId().Result;
                        bool isFound = CDPController.Instance.FindAndClickElement(documentId, "button[class='btn s-small sign-in ']").Result;
                        Thread.Sleep(5000);

                        isFound = CDPController.Instance.FindAndClickElement(documentId, "input[name='username']", 3, MoveMethod.SQRT).Result;
                        Thread.Sleep(1500);
                        CDPMouseController.Instance.InputText(Setting.Instance.username);

                        isFound = CDPController.Instance.FindAndClickElement(documentId, "input[name='password']", 3, MoveMethod.SQRT).Result;
                        Thread.Sleep(1500);
                        CDPMouseController.Instance.InputText(Setting.Instance.password);

                        CDPController.Instance.user_id = string.Empty;
                        isFound = CDPController.Instance.FindAndClickElement(documentId, "button[type='submit']", 1, MoveMethod.SQRT).Result;
                        Thread.Sleep(2000);

                        int nRetry1 = 0;
                        while (string.IsNullOrEmpty(CDPController.Instance.user_id))
                        {
                            Thread.Sleep(300);
                            if (nRetry1 > 50)
                                break;

                            nRetry1++;
                        }

                        CDPController.Instance.ReloadBrowser();
                        Thread.Sleep(7000);
                        StartListening();
                        if (!string.IsNullOrEmpty(CDPController.Instance.user_id))
                        {
                            bLogin = true;
                            break;
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

        public bool login_puputter()
        {
            domain = "playpix.com";
            if (!domain.StartsWith("www."))
                domain = "www." + domain;

            Thread.Sleep(5000);

            if (getBalance() > 0)
            {
                DolphinController.Instance.ReloadBrowser();
                Thread.Sleep(7000);

                StartListening();
                return true;
            }

            int nTotalRetry = 0;
            bool bLogin = false;
            while (nTotalRetry++ < 2)
            {
                if (!Global.bRun)
                    return false;
                try
                {

                    lock (lockerObj)
                    {
                        m_client = initHttpClient();

                        DolphinController.Instance.NavigateInvoke($"https://{domain}/en");

                        Thread.Sleep(30000);
                        bool isFound = DolphinController.Instance.FindAndClick("button[class='btn s-small sign-in ']");
                        Thread.Sleep(5000);

                        //string doEventJScript = "function doEvent(obj, event ) { var event = new Event( event, { target: obj, bubbles: true} ); return obj ? obj.dispatchEvent(event) : false; }";
                        //DolphinController.Instance.ExecuteScript(doEventJScript);

                        isFound = DolphinController.Instance.FindAndClick("input[name='username']", 3);
                        Thread.Sleep(1500);

                        DolphinController.Instance.InputText(Setting.Instance.username);
                        //string input_name_script = $"var el = document.querySelector(input[name='username']);el.value = '{Setting.Instance.username}';doEvent(el, 'input');";
                        //DolphinController.Instance.ExecuteScript(input_name_script);

                        isFound = DolphinController.Instance.FindAndClick("input[name='password']", 3);
                        Thread.Sleep(1500);

                        DolphinController.Instance.InputText(Setting.Instance.password);
                        //string input_pass_script = $"var el = document.querySelector(input[name='password']);el.value = '{Setting.Instance.password}';doEvent(el, 'input');";
                        //DolphinController.Instance.ExecuteScript(input_name_script);

                        DolphinController.Instance.user_id = string.Empty;
                        isFound = DolphinController.Instance.FindAndClick("button[type='submit']", 1);
                        Thread.Sleep(2000);

                        int nRetry1 = 0;
                        while (string.IsNullOrEmpty(DolphinController.Instance.user_id))
                        {
                            Thread.Sleep(300);
                            if (nRetry1 > 50)
                                break;

                            nRetry1++;
                        }

                        DolphinController.Instance.ReloadBrowser();
                        Thread.Sleep(7000);
                        StartListening();
                        if (!string.IsNullOrEmpty(DolphinController.Instance.user_id))
                        {
                            bLogin = true;
                            break;
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

            //httpClientEx.DefaultRequestHeaders.Add("Host", $"{domain}");
            //httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Referer", $"https://{domain}/");

            return httpClientEx;
        }

        public void PlaceBet(List<CapsuledBetburgerInfo> info)
        {
            if (info.Count < 1)
                return;

            PlaceBet(ref info[0].betburgerInfo);
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
            try
            {
                Thread.Sleep(1000 * Setting.Instance.requestDelay);
                if (getBalance() < 0)
                {
                    if (!login())
                        return PROCESS_RESULT.NO_LOGIN;
                }

                if (string.IsNullOrEmpty(info.outcome))
                {
                    LogMng.Instance.onWriteStatus("outcome is invalid");
                    return PROCESS_RESULT.ERROR;
                }

                string competId = string.Empty, gameId = string.Empty, sportId = string.Empty, regionId = string.Empty, sport_name = string.Empty, region_name = string.Empty;
                foreach (string str in info.siteUrl.Split('&'))
                {
                    if (str.Split('=')[0].Trim() == "competition")
                        competId = str.Split('=')[1].Trim();
                    else if (str.Split('=')[0].Trim() == "sport")
                        sportId = str.Split('=')[1].Trim();
                    else if (str.Split('=')[0].Trim() == "game")
                        gameId = str.Split('=')[1].Trim();
                    else if (str.Split('=')[0].Trim() == "region")
                        regionId = str.Split('=')[1].Trim();
                }

                if (_webSocket.ReadyState == WebSocketState.Closed || _webSocket == null)
                {
                    LogMng.Instance.onWriteStatus("Connecting to Websocket ...");
                    StartListening();
                    Thread.Sleep(5000);
                }

                string marketId = "";
                string selectionId = "";

                double newOdds = 0;

                if (!info.isLive)
                {
                    selectionId = info.direct_link;
                    sport_resp = string.Empty;
                    _webSocket.Send(Properties.Resources.playpix_sport_message);

                    int retryCnt2 = 0;
                    while (string.IsNullOrEmpty(sport_resp))
                    {
                        retryCnt2++;
                        if (retryCnt2 > 30)
                            break;

                        Thread.Sleep(300);
                    }

                    sport_resp = "{\"" + sport_resp;
                    JObject jSportResp = JObject.Parse(sport_resp);
                    foreach (dynamic jSportData in jSportResp["data"]["data"]["sport"])
                    {
                        foreach (dynamic jSport in jSportData)
                        {
                            string sport_id = jSport["id"].ToString();
                            if (sportId != sport_id)
                                continue;

                            sport_name = jSport["alias"].ToString();
                            foreach (var jRegionData in jSport["region"])
                            {
                                foreach (dynamic jRegion in jRegionData)
                                {
                                    if (regionId != jRegion["id"].ToString())
                                        continue;

                                    region_name = jRegion["alias"].ToString();
                                    break;
                                }
                            }
                        }

                    }

                    if (string.IsNullOrEmpty(region_name))
                    {
                        LogMng.Instance.onWriteStatus("***Can't find Region***");
                        return PROCESS_RESULT.ERROR;
                    }

                    string event_message = Properties.Resources.playpix_event_message;
                    event_message = event_message.Replace("[gameId]", gameId).Replace("[sportId]", sport_name).Replace("[compId]", competId).Replace("[region]", region_name);

                    event_resp = string.Empty;
                    _webSocket.Send(event_message);

                    int rCnt = 0;
                    while (string.IsNullOrEmpty(event_resp))
                    {
                        rCnt++;
                        if (rCnt > 30)
                            break;

                        Thread.Sleep(300);
                    }

                    if (string.IsNullOrEmpty(event_resp))
                    {
                        LogMng.Instance.onWriteStatus("This event is not existed now..");
                        return PROCESS_RESULT.ERROR;
                    }

                    event_resp = "{\"" + event_resp;
                    JObject jEventResp = JObject.Parse(event_resp);
                    bool isFound = false;
       
                    foreach (var jSportData in jEventResp["data"]["data"]["sport"])
                    {
                        if (isFound)
                            break;

                        foreach (var jSport in jSportData)
                        {
                            if (isFound)
                                break;

                            foreach (var jRegionData in jSport["region"])
                            {
                                if (isFound)
                                    break;

                                foreach (var jRegion in jRegionData)
                                {
                                    if (isFound)
                                        break;

                                    foreach (var jCompetData in jRegion["competition"])
                                    {
                                        if (isFound)
                                            break;

                                        foreach (var jCompet in jCompetData)
                                        {
                                            if (isFound)
                                                break;

                                            foreach (var jGameData in jCompet["game"])
                                            {
                                                if (isFound)
                                                    break;

                                                foreach (var jGame in jGameData)
                                                {
                                                    if (isFound)
                                                        break;

                                                    foreach (var jMarketData in jGame["market"])
                                                    {
                                                        if (isFound)
                                                            break;

                                                        foreach (var jMarket in jMarketData)
                                                        {
                                                            if (isFound)
                                                                break;

                                                            foreach (var jEventData in jMarket["event"])
                                                            {
                                                                if (isFound)
                                                                    break;

                                                                foreach (var jEvent in jEventData)
                                                                {
                                                                    if (isFound)
                                                                        break;

                                                                    string selection_id = jEvent["id"].ToString();
                                                                    if (selection_id == info.direct_link)
                                                                    {
                                                                        marketId = jMarket["id"].ToString();
                                                                        newOdds = (double)jEvent["price"];
                                                                        isFound = true;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!isFound)
                    {
                        LogMng.Instance.onWriteStatus("***Not found odds or line is changed***");
                        return PROCESS_RESULT.MOVED;
                    }

                    if (CheckOddDropCancelBet(newOdds, info))
                        return PROCESS_RESULT.MOVED;
                }
                else
                {
                    try
                    {
                        newOdds = info.odds;
                        string direct_link = WebUtility.UrlDecode(info.direct_link);
                        string[] arr = direct_link.Split('&');
                        foreach (string str in arr)
                        {
                            if (str.Contains("eventId="))
                                gameId = str.Replace("eventId=", string.Empty).Trim();
                            else if (str.Contains("outcomeId="))
                                selectionId = str.Replace("outcomeId=", string.Empty).Trim();
                            else if (str.Contains("marketId="))
                                marketId = str.Replace("marketId=", string.Empty).Trim();
                        }
                    }
                    catch { }
                  
                }
             

                string betslip_message = Properties.Resources.playpix_betslip_message.Replace("[gameId]", gameId).Replace("[marketId]", marketId).Replace("[selectionId]", selectionId);
                betslip_resp = string.Empty;

                _webSocket.Send(betslip_message);
                int retryCnt1 = 0;
                while (string.IsNullOrEmpty(betslip_resp))
                {
                    retryCnt1++;
                    if (retryCnt1 > 30)
                        break;

                    Thread.Sleep(300);
                }

                if (string.IsNullOrEmpty(betslip_resp))
                    return PROCESS_RESULT.ERROR;

                betslip_resp = "{\"" + betslip_resp;

                try
                {
                    JObject jBetslip = JObject.Parse(betslip_resp);
                    newOdds = (double) jBetslip["data"]["data"]["game"][gameId]["market"][marketId]["event"][selectionId]["price"];
                    if (CheckOddDropCancelBet(newOdds, info))
                        return PROCESS_RESULT.MOVED;

                    info.odds = newOdds;
                }
                catch 
                {
                    return PROCESS_RESULT.ERROR;
                }

                /*betslip_resp = string.Empty;
                string getmax_message = Properties.Resources.playpix_getmax_message.Replace("[selId]", info.direct_link);
                _webSocket.Send(getmax_message);

                int retryCnt = 0;
                while (string.IsNullOrEmpty(betslip_resp))
                {
                    retryCnt++;
                    if (retryCnt > 30)
                        break;

                    Thread.Sleep(300);
                }

                if(string.IsNullOrEmpty(betslip_resp))
                    return PROCESS_RESULT.ERROR;

                JObject jMax = JObject.Parse(betslip_resp);
                double max_stake = (double) jMax["data"]["details"]["amount"];*/

                int nRetry = 0;
                while (nRetry++ < 2)
                {
                    betslip_resp = string.Empty;
                    string placebet_message = Properties.Resources.playpix_placebet_message.Replace("[selId]", selectionId).Replace("[odds]", newOdds.ToString()).Replace("[stake]", info.stake.ToString());
                    betslip_resp = string.Empty;
                    _webSocket.Send(placebet_message);

                    retryCnt1 = 0;
                    while (string.IsNullOrEmpty(betslip_resp))
                    {
                        retryCnt1++;
                        if (retryCnt1 > 30)
                            break;

                        Thread.Sleep(300);
                    }

                    if (string.IsNullOrEmpty(betslip_resp))
                    {
                        Thread.Sleep(3000);
                        continue;
                    }

                    betslip_resp = "{\"" + betslip_resp;
                    LogMng.Instance.onWriteStatus("****Placebet Result***" + betslip_resp);
                    JObject placebet_Response = JObject.Parse(betslip_resp);
                    if (placebet_Response["data"]["StatusCode"] != null && placebet_Response["data"]["StatusCode"].ToString() == "0")
                    {
                        return PROCESS_RESULT.PLACE_SUCCESS;
                    }
                }
            }
            catch { }
            
            LogMng.Instance.onWriteStatus(string.Format("** PLACE BET FAILED"));
            return PROCESS_RESULT.ERROR;
        }
        public double getBalance()
        {
            double balance = -1;
            try
            {
                string balance_str = "";
                if (Setting.Instance.browserType == 0)
                {
                    balance_str = CDPController.Instance.ExecuteScript("document.getElementsByClassName('balanceAmount')[0].innerText", true);
                    if (string.IsNullOrEmpty(balance_str))
                        return balance;
                }
                else
                {
                    balance_str = DolphinController.Instance.ExecuteScript("document.getElementsByClassName('balanceAmount')[0].innerText");
                    if (string.IsNullOrEmpty(balance_str))
                        return balance;
                }


                balance = Utils.ParseToDouble(balance_str.Replace("R$", string.Empty).Replace(",", ".").Trim());         
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
