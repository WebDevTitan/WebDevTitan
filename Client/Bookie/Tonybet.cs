namespace Project.Bookie
{
#if (TONYBET)
    class TonybetCtrl : IBookieController
    {
        
        public HttpClient m_client = null;
        string domain = "tonybet.es";
        Object lockerObj = new object();
        //private static ProxyServer _proxyServer = null;
        public string auth_token = string.Empty;

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
            return nLastPendingbetCount;
        }

        public bool logout()
        {
            return true;
        }
        public TonybetCtrl()
        {
            if (CDPController.Instance._browserObj == null)
                CDPController.Instance.InitializeBrowser(domain);

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
            bool isLoggedIn = false;
            try
            {
                CDPController.Instance.NavigateInvoke($"https://{domain}/");
                
                Thread.Sleep(7000);

                CDPController.Instance.ExecuteScript("document.querySelector('button[data-test = modalSuccessOKButton]').click();");
                string responseBody = CDPController.Instance.ExecuteScript("localStorage.customer", true);
                if (!string.IsNullOrEmpty(responseBody))
                {
                    JObject jResp = JObject.Parse(responseBody);
                    CDPController.Instance.auth_token = jResp["token"].ToString();

                    isLoggedIn = true;
                    return isLoggedIn;
                }

                long documentNodeId = CDPController.Instance.GetDocumentId().Result;
                CDPController.Instance.FindAndClickElement(documentNodeId, "button[data-test='login']", 3).Wait();
                Thread.Sleep(1000);

                CDPController.Instance.FindAndClickElement(documentNodeId, "input[data-test='username']", 3).Wait();
                Thread.Sleep(1000);
                CDPMouseController.Instance.InputText(Setting.Instance.username);

                CDPController.Instance.FindAndClickElement(documentNodeId, "input[data-test='password']", 3).Wait();
                Thread.Sleep(1000);
                CDPMouseController.Instance.InputText(Setting.Instance.password);

                CDPController.Instance.isLogged = false;
                CDPController.Instance.auth_token = "";

                CDPController.Instance.FindAndClickElement(documentNodeId, "button[data-test='submitLogin']").Wait();
                Thread.Sleep(5000);

                int rCnt = 0;
                while (!CDPController.Instance.isLogged)
                {
                    rCnt++;
                    Thread.Sleep(1000);
                    if (rCnt > 30)
                        break;
                }

                isLoggedIn = CDPController.Instance.isLogged;
            }
            catch { }
            return isLoggedIn;
        }
        public bool login1()
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

                        HttpResponseMessage respMessage = m_client.GetAsync($"https://platform.{domain}/api/user/is-auth").Result;
                        respMessage.EnsureSuccessStatusCode();

                        string respBody = respMessage.Content.ReadAsStringAsync().Result;
                        JObject jResp = JObject.Parse(respBody);
                        if ((bool)jResp["data"]["isAuth"])
                        {
                            bLogin = true;
                            return bLogin;
                        }

                        HttpResponseMessage responseMessageLogin = m_client.PostAsync($"https://platform.{domain}/api/auth", (HttpContent)new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)new KeyValuePair<string, string>[6]{
                            new KeyValuePair<string, string>("email", Setting.Instance.username),
                            new KeyValuePair<string, string>("phone", ""),
                            new KeyValuePair<string, string>("password", Setting.Instance.password),
                            new KeyValuePair<string, string>("confirmationCode", ""),
                            new KeyValuePair<string, string>("type", "1"),
                            new KeyValuePair<string, string>("prefix", "")
                        })).Result;

                        responseMessageLogin.EnsureSuccessStatusCode();

                        respBody = responseMessageLogin.Content.ReadAsStringAsync().Result;
                        jResp = JObject.Parse(respBody);
                        if (jResp["status"].ToString() == "ok")
                        {
                            auth_token = jResp["data"]["token"].ToString();
                            m_client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {auth_token}");
                            bLogin = true;
                        }
                        else
                            bLogin = false;
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
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");


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

            string eventUrl = info.siteUrl;
            string leagueId = eventUrl.Split('/')[eventUrl.Split('/').Length - 2].Split('-')[0];
            long eventId = GetMatchIdFromLeague(leagueId, info);

            string outcomeType = string.Empty, outcomeId = string.Empty, specifiers = string.Empty, hcp = string.Empty, marketId = string.Empty;
            string directInfo = info.direct_link;
            string[] info_arr = directInfo.Split('&');
            foreach (string str in info_arr)
            {
                string[] sub_arr = str.Split('=');
                if (sub_arr[0] == "outcomeType")
                    outcomeType = sub_arr[1].Trim();
                else if (sub_arr[0] == "outcomeId")
                    outcomeId = sub_arr[1].Trim();
                else if (sub_arr[0] == "marketId")
                    marketId = sub_arr[1].Trim();
                else if (sub_arr[0] == "specifiers")
                    specifiers = WebUtility.UrlDecode(str.Replace("specifiers=", string.Empty).Trim());
            }

            int nRetry = 0;
            while (nRetry++ < 2)
            {
                JObject jBetSlip = new JObject();
                jBetSlip["eventId"] = eventId;
                jBetSlip["eventType"] = 0;
                jBetSlip["outcomeId"] = Utils.parseToInt(outcomeId);
                jBetSlip["marketId"] = Utils.parseToInt(marketId);
                jBetSlip["specifiers"] = specifiers;
                jBetSlip["outcomeType"] = Utils.parseToInt(outcomeType);
                jBetSlip["madeFrom"] = 2;

                m_client.DefaultRequestHeaders.Remove("Authorization");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {CDPController.Instance.auth_token}");

                HttpResponseMessage respMessage = m_client.PostAsync($"https://platform.{domain}/api/v2/coupon/add", new StringContent(jBetSlip.ToString(), Encoding.UTF8, "application/json")).Result;

                string respBody = respMessage.Content.ReadAsStringAsync().Result;
                JObject jResp = JObject.Parse(respBody);
                if(jResp["status"].ToString() == "fail")
                {
                    LogMng.Instance.onWriteStatus(jResp["message"].ToString());
                    Removebet();
                    return PROCESS_RESULT.ERROR;
                }
                try
                {
                    double newOdds = (double)jResp["data"]["bets"][0]["odds"];
                    if (CheckOddDropCancelBet(newOdds, info))
                    {
                        Removebet();
                        LogMng.Instance.onWriteStatus($"Lower odd, cancelled. ({info.odds}) -> ({newOdds})");
                        return PROCESS_RESULT.MOVED;
                    }
                    info.odds = newOdds;
                }
                catch
                {
                    Removebet();
                    return PROCESS_RESULT.ERROR;
                }

                double stake = info.stake;
                int rBetCnt = 0;
                while (rBetCnt < 3)
                {
                    JObject jPlacebet = new JObject();
                    jPlacebet["source"] = "desktop";
                    jPlacebet["lang"] = "pt";
                    jPlacebet["bets"] = new JArray();
                    JArray jArr = new JArray();
                    JObject jBet = new JObject();
                    jBet["error"] = null;
                    jBet["eventId"] = eventId;
                    jBet["marketId"] = Utils.parseToInt(marketId);
                    jBet["outcomeId"] = Utils.parseToInt(outcomeId);
                    jBet["odds"] = info.odds;
                    jBet["outcomeType"] = Utils.parseToInt(outcomeType);
                    jBet["stake"] = stake;
                    jBet["eventType"] = 0;
                    jBet["specifiers"] = specifiers;
                    jBet["madeFrom"] = 2;
                    jBet["marketType"] = 1;
                    jArr.Add(jBet);

                    jPlacebet["bets"] = jArr;

                    CDPController.Instance.couponStatus = string.Empty;
                    HttpResponseMessage placebetRespMessage = m_client.PostAsync($"https://platform.{domain}/api/bet/make-single", new StringContent(jPlacebet.ToString(), Encoding.UTF8, "application/json")).Result;

                    string placebetRespBody = placebetRespMessage.Content.ReadAsStringAsync().Result;
                    JObject jPlacebetBody = JObject.Parse(placebetRespBody);
                    if (jPlacebetBody["status"].ToString() == "ok")
                    {
                        return PROCESS_RESULT.PLACE_SUCCESS;
                    }
                    else
                    {
                        LogMng.Instance.onWriteStatus($"Placebet Result : ****{placebetRespBody}***");
                        Removebet();
                        return PROCESS_RESULT.ERROR;
                        /*int retryCnt = 0;
                        while (string.IsNullOrEmpty(CDPController.Instance.couponStatus))
                        {
                            retryCnt++;
                            Thread.Sleep(500);
                            if (retryCnt == 30)
                                break;
                        }
                        JObject jCoupon = JObject.Parse(CDPController.Instance.couponStatus);
                        var jBetslip = jCoupon["result"]["data"]["data"]["payload"]["bets"][0];
                        if (string.IsNullOrEmpty(jBetslip["rejectCode"].ToString()))
                        {
                            return PROCESS_RESULT.PLACE_SUCCESS;                           
                        }
                        else
                        {
                            int rejectCode = (int)jBetslip["rejectCode"];
                            if (rejectCode == -703)
                            {
                                var jMaxStake = jBetslip["alternativeStake"];
                                if (jMaxStake == null)
                                {
                                    Removebet();
                                    return PROCESS_RESULT.SUSPENDED;
                                }
                                else
                                {
                                    rBetCnt++;
                                    stake = (double)jMaxStake;
                                    info.stake = stake;
                                }
                            }
                            else
                            {
                                return PROCESS_RESULT.PLACE_SUCCESS;
                            }
                        }
                        */
                    }
                }

            }
            LogMng.Instance.onWriteStatus(string.Format("** PLACE BET FAILED"));
            Removebet();
            return PROCESS_RESULT.ERROR;
        }
        public double getBalance()
        {
            double balance = -1;
            try
            {
                m_client.DefaultRequestHeaders.Remove("Authorization");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {CDPController.Instance.auth_token}");

                HttpResponseMessage respMessage = m_client.GetAsync($"https://platform.{domain}/api/v2/user/get-info").Result;
                respMessage.EnsureSuccessStatusCode();

                string respBody = respMessage.Content.ReadAsStringAsync().Result;
                JObject jResp = JObject.Parse(respBody);
                balance = (double)jResp["data"]["user"]["accounts"][0]["amount"];
            }
            catch { }
            return balance;
        }
        public void Removebet()
        {
            try
            {
                HttpResponseMessage respMessage = m_client.GetAsync($"https://platform.{domain}/api/coupon/clear").Result;
                respMessage.EnsureSuccessStatusCode();

            }
            catch { }
        }
        private long  GetMatchIdFromLeague(string leagueId , BetburgerInfo info)
        {
            long eventId = -1;
            try
            {
                HttpResponseMessage respMessage = m_client.GetAsync($"https://platform.tonybet.es/api/event/list?period=0&competitor1Id_neq=&competitor2Id_neq=&status_in%5B%5D=0&limit=150&main=1&relations%5B%5D=odds&relations%5B%5D=league&relations%5B%5D=result&relations%5B%5D=competitors&relations%5B%5D=withMarketsCount&relations%5B%5D=players&relations%5B%5D=sportCategories&relations%5B%5D=broadcasts&relations%5B%5D=statistics&relations%5B%5D=additionalInfo&relations%5B%5D=tips&leagueId_in%5B%5D={leagueId}&oddsExists_eq=1&lang=es").Result;
                
                string respBody = respMessage.Content.ReadAsStringAsync().Result;
                JLeague jResp = JsonConvert.DeserializeObject<JLeague>(respBody);

                List<TonyCompetition> competitions = new List<TonyCompetition>();
                foreach(TonyCompetition jCompet in jResp.data.relations.competitors)
                    competitions.Add(jCompet);

                double min_dis = 100;
                TonyEvent sameEvent = null;
                foreach (TonyEvent jItem in jResp.data.items)
                {
                    try
                    {
                        TonyCompetition homeComp = competitions.Find(c => c.id == jItem.competitor1Id);
                        if (homeComp == null)
                            continue;

                        TonyCompetition awayComp = competitions.Find(c => c.id == jItem.competitor2Id);
                        if (awayComp == null)
                            continue;

                        double h_dis = Utils.getDistance(homeComp.name, info.homeTeam);
                        double a_dis = Utils.getDistance(awayComp.name, info.awayTeam);
                        if(min_dis > (h_dis + a_dis))
                        {
                            min_dis = h_dis + a_dis;
                            sameEvent = jItem;
                        }

                    }
                    catch { }
                }

                eventId = sameEvent.id;
            }
            catch { }
            return eventId;
        }
    }
#endif
}
