namespace Project.Bookie
{
#if (WINAMAX)
    class WinamaxCtrl : IBookieController
    {
        private string domain = "winamax.es";
        public HttpClient m_client = null;
        
        Object lockerObj = new object();
        //private static ProxyServer _proxyServer = null;

        private JObject payloadObject = null;
        
        public int GetPendingbets()
        {
            return 0;
        }

        public void Close()
        {

        }

        public void Feature()
        {

        }

        public bool logout()
        {
            return true;
        }
        public WinamaxCtrl()
        {

            m_client = initHttpClient();
            //Global.SetMonitorVisible(false);
        }

        public bool Pulse()
        {
            if (getBalance() < 0)
                return false;
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
            int nTotalRetry = 0;
            Global.SetMonitorVisible(true);
            bool bLogin = false;
            if (getBalance() > 0)
                return true;
            LogMng.Instance.onWriteStatus("Login Start");
            while (nTotalRetry++ < 2)
            {
                try
                {

                    lock (lockerObj)
                    {
                        Global.RemoveCookies();
                        m_client = initHttpClient();

                        Global.OpenUrl($"https://www.{domain}/account/login.php");

                        string trust_level = "";
                        string challenge_hash = "";
                        string oid = "";

                        int n_waitRetry = 10;
                        while (n_waitRetry-- >= 0)
                        {
                            trust_level = Global.RunScriptCode("document.querySelectorAll('input[name=\"trust_level\"]')[0].value").Replace("\"", "");
                            challenge_hash = Global.RunScriptCode("document.querySelectorAll('input[name=\"challenge_hash\"]')[0].value").Replace("\"", "");
                            oid = Global.RunScriptCode("document.querySelectorAll('input[name=\"oid\"]')[0].value").Replace("\"", "");


                            if (string.IsNullOrEmpty(trust_level) || string.IsNullOrEmpty(challenge_hash) || string.IsNullOrEmpty(oid))
                            {
                                Thread.Sleep(1000);
                                continue;
                            }
                            break;                            
                        }
                        if (n_waitRetry <= 0)
                            continue;

                                             

                        JObject credentials = new JObject();
                        credentials["login"] = Setting.Instance.username;
                        credentials["password"] = Setting.Instance.password;

                        JObject client = new JObject();
                        client["application"] = "winamax";
                        client["oid"] = oid;
                        client["ua"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36";

                        JObject subparams = new JObject();
                        subparams["trust_level"] = trust_level;
                        subparams["challenge_hash"] = challenge_hash;
                        subparams["credentials"] = credentials;
                        subparams["client"] = client;
                        subparams["next_step_ok"] = true;

                        JObject param = new JObject();

                        param["method"] = "Authorize";
                        param["ts"] = (double) Utils.getTick()/1000;
                        param["ns"] = "core/authentication/token/authorize";
                        param["id"] = $"login-form-Authorize-{Utils.getTick()}";
                        param["params"] = subparams;


                        string loginURL = "https://wapi.winamax.es/core/authentication/token/authorize";


                        string formDataString = JsonConvert.SerializeObject(param);
                        HttpResponseMessage respMessage = m_client.PostAsync(loginURL, new StringContent(formDataString, Encoding.UTF8, "application/x-www-form-urlencoded")).Result;

                        string functionString = $"window.fetch('{loginURL}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json; charset=UTF-8', 'accept': 'application/json; charset=UTF-8' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                        Global.strWebResponse2 = "";
                        
                        string strWebResponse2 = Global.RunScriptCode(functionString);

                        
                        LogMng.Instance.onWriteStatus($"validate_betslip Req: {formDataString}");
                        LogMng.Instance.onWriteStatus($"validate_betslip Res: {Global.strWebResponse2}");

                        dynamic placebet_Response = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse2);

                        if (!Global.waitResponseEvent1.Wait(5000))
                        {
                            LogMng.Instance.onWriteStatus($"Login No Response");
                            continue;
                        }

                        if (string.IsNullOrEmpty(Global.strWebResponse1))
                        {
                            LogMng.Instance.onWriteStatus($"Login No Capture request");
                            continue;
                        }

                        dynamic loginresponseObject = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse1);

                        dynamic jsonResResp = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse1);
                        
                        if (jsonResResp.Success.ToString().ToLower() != "true")
                        {
                            LogMng.Instance.onWriteStatus($"Login Failed: {jsonResResp.message.ToString()}");
                            continue;
                        }

                        payloadObject["SessionId"] = jsonResResp.Login.SessionId.ToString();
                        int nRetry1 = 0;
                        while (nRetry1 < 3)
                        {
                            Thread.Sleep(3000);
                            Task.Run(async () => await Global.GetCookie($"https://www.{domain}")).Wait();
                            if (getBalance() >= 0)
                            {
                                Global.RefreshPage();
                                return true;
                            }
                            nRetry1++;
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

            handler.CookieContainer = Global.cookieContainer;

            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
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

            httpClientEx.DefaultRequestHeaders.Add("Host", "wapi.winamax.es");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://wapi.winamax.es/");

            return httpClientEx;
        }

        public void PlaceBet(List<CapsuledBetburgerInfo> info)
        {
            if (info.Count < 1)
                return;

            info[0].result = PlaceBet(ref info[0].betburgerInfo);
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

            if (string.IsNullOrEmpty(info.outcome))
            {
                LogMng.Instance.onWriteStatus("outcome is invalid");
                return PROCESS_RESULT.ERROR;
            }
            int odd = (int)(info.odds * 1000);
            int stake = (int)(info.stake * 100);
            int nRetry = 0;
            while (nRetry++ < 2)
            {
                string getEventsMarketURL = $"hhttps://www.{domain}/betting/validate_betslip.php";


                string formDataString = $"bsm={{\"T\":1,\"A\":{stake},\"B\":[{{\"BID\":{info.direct_link},\"O\":{odd}}}],\"CV\":\"2.61.0-desktop\",\"locale\":\"es\"}}";

                string functionString = $"window.fetch('{getEventsMarketURL}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json; charset=UTF-8', 'accept': 'application/json; charset=UTF-8' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                Global.strWebResponse2 = "";
                Global.waitResponseEvent2.Reset();

                Global.RunScriptCode(functionString);

                if (!Global.waitResponseEvent2.Wait(5000))
                {
                    LogMng.Instance.onWriteStatus($"GetEventMarkets no Response");
                    return PROCESS_RESULT.ERROR;
                }

                LogMng.Instance.onWriteStatus($"validate_betslip Req: {formDataString}");
                LogMng.Instance.onWriteStatus($"validate_betslip Res: {Global.strWebResponse2}");

                dynamic placebet_Response = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse2);
                if (placebet_Response.BSID != null && !string.IsNullOrEmpty(placebet_Response.BSID) && placebet_Response.ESTR != null && placebet_Response.ESTR.ToString() == "OK")
                {
                    return PROCESS_RESULT.PLACE_SUCCESS;
                }
            }
            LogMng.Instance.onWriteStatus(string.Format("** PLACE BET FAILED"));
            return PROCESS_RESULT.ERROR;
        }
        public double getBalance()
        {
            double balance = -1;
            if (payloadObject == null)
                return -1;
            try
            {
                string value = Global.RunScriptCode("document.getElementById('money-block').querySelector('.value').innerText").Replace("€", "");
                
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"GetBalance result: {value}");
#endif
                
                    balance = Utils.ParseToDouble(value);                    
                
            }
            catch (Exception e)
            {

            }
            return balance;
        }
    }
#endif
}
