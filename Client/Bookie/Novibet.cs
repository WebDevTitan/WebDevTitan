namespace Project.Bookie
{
#if (NOVIBET)
    class NovibetCtrl : IBookieController
    {
        
        public HttpClient m_client = null;
        string domain = "www.novibet.gr";
        Object lockerObj = new object();
        //private static ProxyServer _proxyServer = null;
        
        public void Close()
        {

        }

        public void Feature()
        {

        }
        int nLastPendingbetCount = 0;
        DateTime LastPendingbetGetTime = DateTime.MinValue;
        public int GetPendingbets()
        {
            if (DateTime.Now.Subtract(LastPendingbetGetTime).TotalMinutes > 1)
            {
                LastPendingbetGetTime = DateTime.Now;
                try
                {

                    string getOpenbetsURL = $"https://{domain}/ngapi/en/openbets/updateforchanges";
                    string baseURL = $"https://{domain}/stoixima";
                    string functionString = $"window.fetch('{getOpenbetsURL}', {{ method: 'GET', headers: {{ 'accept': 'application/json, text/plain, */*', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/json', }}, credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', referrer: '{baseURL}' }}).then(response => response.json());";

                    Global.strWebResponse4 = "";
                    Global.waitResponseEvent4.Reset();

                    //LogMng.Instance.onWriteStatus($"GetBalance request: {functionString}");
                    Global.RunScriptCode(functionString);

                    if (Global.waitResponseEvent4.Wait(3000))
                    {
                        LogMng.Instance.onWriteStatus($"Openbets result: {Global.strWebResponse4}");
                        dynamic details = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse4);

                        nLastPendingbetCount = details.openBets.totalCount;
                    }
                }
                catch (Exception e)
                {

                }
            }
            return nLastPendingbetCount;
        }

        public bool logout()
        {
            return true;
        }
        public NovibetCtrl()
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
            while (nTotalRetry++ < 2)
            {
                try
                {

                    lock (lockerObj)
                    {
                        Global.RemoveCookies();
                        m_client = initHttpClient();


                        Global.OpenUrl($"https://{domain}");

                        //Global.RunScriptCode("document.getElementsByClassName('uk-button uk-button-primary GTM-login')[0].click();");

                        string betURL = $"https://{domain}/ngapi/en/useraccount/login";

                        string formDataString = "";
                       
                        formDataString = $"{{\"username\":\"{Setting.Instance.username}\",\"password\":\"{Setting.Instance.password}\"}}";
                     

                        string functionString = $"window.fetch('{betURL}', {{ headers: {{ accept: 'application/json, text/plain, */*', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                        Global.strWebResponse1 = "";
                        Global.waitResponseEvent1.Reset();
                        
                        Global.RunScriptCode(functionString);

                        if (!Global.waitResponseEvent1.Wait(5000))
                        {
                            LogMng.Instance.onWriteStatus($"Login No Response");
                            continue;
                        }

                        dynamic jsonResResp = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse1);
                        
                        if (jsonResResp.success.ToString() != "True")
                        {
                            LogMng.Instance.onWriteStatus($"Login Failed: {jsonResResp.message.ToString()}");
                            return false;
                        }

                        int nRetry1 = 0;
                        while (nRetry1 < 3)
                        {
                            Thread.Sleep(3000);
                            Task.Run(async () => await Global.GetCookie($"https://{domain}")).Wait();
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

            info[0].result = PlaceBet(info[0].betburgerInfo);
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
        public PROCESS_RESULT PlaceBet(BetburgerInfo info)
        {
            Thread.Sleep(1000 * Setting.Instance.requestDelay);
            if (getBalance() < 0)
            {
                if (!login())
                    return PROCESS_RESULT.NO_LOGIN;
            }

            string[] paramIDs = info.direct_link.Split('|');

            int nRetry = 0;
            while (nRetry++ < 2)
            {
                string betURL = $"https://{domain}/ngapi/en/betslip/toggleitem";

                string formDataString = "";

                formDataString = $"{{\"compositeId\":\"e{paramIDs[1]}-{paramIDs[0]}\",\"price\":{info.odds}}}";


                string functionString = $"window.fetch('{betURL}', {{ headers: {{ accept: 'application/json, text/plain, */*', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                Global.strWebResponse1 = "";
                Global.waitResponseEvent1.Reset();

                Global.RunScriptCode(functionString);

                if (!Global.waitResponseEvent1.Wait(5000))
                {
                    LogMng.Instance.onWriteStatus($"toggleitem no Response");
                    return PROCESS_RESULT.ERROR;
                }

                dynamic jsonResResp = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse1);
                double newodd = Convert.ToDouble(jsonResResp.selections.items[0].price.ToString());

                if (CheckOddDropCancelBet(newodd, info))
                {

                    LogMng.Instance.onWriteStatus($"Lower odd, cancelled. ({info.odds}) -> ({newodd})");
                    return PROCESS_RESULT.MOVED;
                }
                //if (newodd != info.odds)
                //{
                //Global.strAddBetResult = "";
                //betURL = $"https://{domain}/ngapi/en/betslip/acceptchanges";
                //formDataString = "{}";
                //functionString = $"window.fetch('{betURL}', {{ headers: {{ accept: 'application/json, text/plain, */*', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                //Global.RunScriptCode(functionString);
                //if (!Global.waitResponseEvent.Wait(5000))
                //{
                //    LogMng.Instance.onWriteStatus($"acceptchanges no Response");
                //    return PROCESS_RESULT.ERROR;
                //}
                //LogMng.Instance.onWriteStatus($"acceptchanges Res: {Global.strAddBetResult}");
                //}
                betURL = $"https://{domain}/ngapi/en/betslip/submit";

                formDataString = $"[{{\"id\":\"{paramIDs[0]}\",\"amount\":{info.stake}}}]";
                LogMng.Instance.onWriteStatus($"submit Req: {formDataString}");
                functionString = $"window.fetch('{betURL}', {{ headers: {{ accept: 'application/json, text/plain, */*', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                Global.strWebResponse2 = "";
                Global.waitResponseEvent2.Reset();

                Global.RunScriptCode(functionString);

                if (!Global.waitResponseEvent2.Wait(5000))
                {
                    LogMng.Instance.onWriteStatus($"submit no Response");
                    return PROCESS_RESULT.ERROR;
                }


                jsonResResp = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse2);
                LogMng.Instance.onWriteStatus($"submit Res: {Global.strWebResponse2}");
                if (jsonResResp.mode.ToString() == "3")
                {
                    LogMng.Instance.onWriteStatus(jsonResResp.message.ToString());
                    return PROCESS_RESULT.PLACE_SUCCESS;
                }

                string expectedDelay = jsonResResp.expectedDelay.ToString();

                double seconds = TimeSpan.Parse(expectedDelay).TotalSeconds;

                if (seconds <= 0 || seconds > 10)
                {
                    LogMng.Instance.onWriteStatus($"submit has special delay: {expectedDelay}");
                    return PROCESS_RESULT.ERROR;
                }
                Thread.Sleep((int)seconds * 1000);

                betURL = $"https://{domain}/ngapi/en/betslip/querydelayed";

                formDataString = "{}";

                functionString = $"window.fetch('{betURL}', {{ headers: {{ accept: 'application/json, text/plain, */*', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                Global.strWebResponse3 = "";
                Global.waitResponseEvent3.Reset();

                Global.RunScriptCode(functionString);

                if (!Global.waitResponseEvent3.Wait(5000))
                {
                    LogMng.Instance.onWriteStatus($"querydelayed no Response");
                    return PROCESS_RESULT.ERROR;
                }

                LogMng.Instance.onWriteStatus($"querydelayed Res: {Global.strWebResponse3}");
                jsonResResp = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse3);

                if (jsonResResp.mode.ToString() == "3")
                {
                    info.eventTitle = jsonResResp.selections.items[0].id.ToString();
                    LogMng.Instance.onWriteStatus(jsonResResp.message.ToString());
                    return PROCESS_RESULT.PLACE_SUCCESS;
                }

                if (jsonResResp.errorType.ToString() == "2")
                {
                    if (jsonResResp.selections.items[0].rejection != null)
                    {
                        newodd = Convert.ToDouble(jsonResResp.selections.items[0].rejection.currentBetPrice.ToString());
                        if (CheckOddDropCancelBet(newodd, info))
                        {
                            LogMng.Instance.onWriteStatus($"Odd is changed {info.odds} -> {newodd}");
                            return PROCESS_RESULT.MOVED;
                        }

                        info.odds = newodd;
                    }
                }                
            }
            LogMng.Instance.onWriteStatus(string.Format("** PLACE BET FAILED"));
            return PROCESS_RESULT.ERROR;
        }
        public double getBalance()
        {
            double balance = -1;
            try
            {

                string getBalanceURL = $"https://{domain}/ngapi/en/useraccount/updateFunds";
                string baseURL = $"https://{domain}/en/live-betting";
                string functionString = $"window.fetch('{getBalanceURL}', {{ method: 'GET', headers: {{ 'accept': 'application/json, text/plain, */*', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/json', }}, credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', referrer: '{baseURL}' }}).then(response => response.json());";

                Global.strWebResponse2 = "";
                Global.waitResponseEvent2.Reset();

                //LogMng.Instance.onWriteStatus($"GetBalance request: {functionString}");
                Global.RunScriptCode(functionString);

                if (Global.waitResponseEvent2.Wait(3000))
                {
                    LogMng.Instance.onWriteStatus($"GetBalance result: {Global.strWebResponse2}");
                    dynamic details = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse2);

                    balance = Utils.ParseToDouble(details.funds.ToString());
                }
            }
            catch (Exception e)
            {

            }
            return balance;
        }
    }
#endif
}
