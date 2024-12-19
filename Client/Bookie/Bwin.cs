namespace Project.Bookie
{
#if (BWIN || SPORTINGBET)
    public class BwinCtrl : IBookieController
    {
        public HttpClient m_client = null;

        public string country_code = "";

        string bwin_access_id = "";

        string xsrfToken = "";

        string href = "";

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
        public BwinCtrl()
        {
            m_client = initHttpClient();
            if (CDPController.Instance._browserObj == null)
                CDPController.Instance.InitializeBrowser($"https://sports.{Setting.Instance.domain}/");

        }

        public void Close()
        {

        }

        public void Feature()
        {

        }

        public bool Pulse()
        {
            return false;
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

            if (Global.cookieContainer == null)
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


            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html, application/xhtml+xml, application/xml; q=0.9, image/webp, */*; q=0.8");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");

            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("context", "2:0:en_US");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"90\", \"Google Chrome\";v=\"90\"");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;

            return httpClientEx;
        }

        public bool logout()
        {
            return true;
        }
        public bool login()
        {
            bool bLogin = false;
            try
            {
                country_code = CDPController.Instance.ExecuteScript("clientConfig.msConnection.culture", true);

                CDPController.Instance.NavigateInvoke($"https://sports.{Setting.Instance.domain}/{country_code}/sports");
                Thread.Sleep(10000);

                long documentId = CDPController.Instance.GetDocumentId().Result;

                bool isFound = CDPController.Instance.FindAndClickElement(documentId, "vn-menu-item-text-content[data-testid='signinsports']").Result;
                if (!isFound)
                {
                    bLogin = true;
                    return bLogin;
                }
                Thread.Sleep(3000);

               isFound = CDPController.Instance.FindAndClickElement(documentId, "input[id='userId']" , 3).Result;
                if (isFound)
                    CDPMouseController.Instance.InputText(Setting.Instance.username);

                Thread.Sleep(1000);

                isFound = CDPController.Instance.FindAndClickElement(documentId, "input[name='password']" , 3).Result;
                if (isFound)
                    CDPMouseController.Instance.InputText(Setting.Instance.password);

                Thread.Sleep(5000);
                CDPController.Instance.isLogged = false;
                isFound = CDPController.Instance.FindAndClickElement(documentId, "button[class='login w-100 btn btn-primary']").Result;
                Thread.Sleep(2000);
                isFound = CDPController.Instance.FindAndClickElement(documentId, "button[class='login w-100 btn btn-primary']").Result;

                int retryCnt = 0;
                while (!CDPController.Instance.isLogged)
                {
                    retryCnt++;
                    if (retryCnt > 30)
                        break;

                    Thread.Sleep(500);
                }

                bLogin = CDPController.Instance.isLogged;
                if (bLogin)
                {
                    Global.cookieContainer = CDPController.Instance.GetCoookies().Result;
                    m_client = initHttpClient();
                }
            }
            catch (Exception e)
            {

            }

            LogMng.Instance.onWriteStatus($"Login Result: {bLogin}");
            return bLogin;
        }

        public double getBalance()
        {
            int nRetry = 2;
            double balance = -1;
            while (nRetry >= 0)
            {
                nRetry--;
                try
                {
                   if(string.IsNullOrEmpty(country_code))
                        country_code = CDPController.Instance.ExecuteScript("clientConfig.msConnection.culture", true);
                    
                    string balance_url = $"https://sports.{Setting.Instance.domain}/{country_code}/api/balance";
                    if(string.IsNullOrEmpty(href))
                        href = CDPController.Instance.ExecuteScript("window.location.href", true);

                    string balance_query = Resources.ResourceManager.GetString("bwin_balance_query");

                    balance_query = balance_query.Replace("[balance_url]", balance_url);
                    balance_query = balance_query.Replace("[x-bwin-browser-url]", href);

                    //HttpResponseMessage respMessage = m_client.GetAsync(balance_url).Result;

                    //string balanceResp = respMessage.Content.ReadAsStringAsync().Result;
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

                    JObject jBal = JObject.Parse(CDPController.Instance.balanceRespBody);
                    balance = (double)jBal["balance"]["accountBalance"];

                    break;
                }
                catch (Exception e)
                {
                    //LogMng.Instance.onWriteStatus(string.Format("Balance exception: {0} {1}", e.Message, e.StackTrace));
                }
            }

            LogMng.Instance.onWriteStatus(string.Format("Balance: {0}", balance));
            return balance;
        }
        
        public int generateSelectionId()
        {
            return rand.Next(100001, 999999);
        }

        public void PlaceBet(List<CapsuledBetburgerInfo> info)
        {
            if (info.Count < 1)
                return;

            info[0].result = PlaceBet(ref info[0].betburgerInfo);
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

                bwin_access_id = CDPController.Instance.ExecuteScript("clientConfig.msConnection.publicAccessId", true);

                if (string.IsNullOrEmpty(bwin_access_id))
                    bwin_access_id = CDPController.Instance.ExecuteScript("clientConfig.msConnection.pushAccessId", true);

                xsrfToken = CDPController.Instance.ExecuteScript("clientConfig.vnUser.xsrfToken", true);

                if(string.IsNullOrEmpty(country_code))
                    country_code = CDPController.Instance.ExecuteScript("clientConfig.msConnection.culture", true);
                
                string country = "";
                if (country_code == "pt-br" || country_code == "en")
                    country = "BR";

                string betslip_url = $"https://sports.{Setting.Instance.domain}/cds-api/bettingoffer/picks?x-bwin-accessid={bwin_access_id}&lang={country_code}&country={country}&userCountry={country}";
                
                string codSelection = info.direct_link.Split('|')[0].Trim();
                string codMarket = info.direct_link.Split('|')[1].Trim();
                string eventId = info.siteUrl.Split('-')[info.siteUrl.Split('-').Length - 1];
                string betslipReq = "";

                if (!info.direct_link.Contains("-"))
                    betslipReq = $"{{\"picks\":[],\"tv1Picks\":[],\"tv2Picks\":[{{\"fixtureId\":\"{eventId}\",\"optionMarketId\":{codSelection},\"isClassicBetBuilder\":false,\"optionId\":{codMarket}}}]}}";
                else
                    betslipReq = $"{{\"picks\":[],\"tv1Picks\":[{{\"fixtureId\":\"{eventId}\",\"gameId\":{codSelection},\"resultId\":{codMarket},\"useLiveFallback\":false}}],\"tv2Picks\":[]}}";
                
                if(string.IsNullOrEmpty(href))
                    href = CDPController.Instance.ExecuteScript("window.location.href", true);

                //m_client.DefaultRequestHeaders.Clear();

                //m_client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                //m_client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
                //m_client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
                //m_client.DefaultRequestHeaders.TryAddWithoutValidation("x-bwin-browser-url", href);
                //m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-XSRF-TOKEN", xsrfToken);
                //m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-From-Product", "sports");
                //m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-App-Context", "default");
                //m_client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://sports.bwin.com");

                //HttpResponseMessage betslipRespMessage = m_client.PostAsync(betslip_url, new StringContent(betslipReq, Encoding.UTF8, "application/json")).Result;


                //string betslipResp = betslipRespMessage.Content.ReadAsStringAsync().Result;

                string betslip_query = Resources.ResourceManager.GetString("bwin_betslip_query");
                betslip_query = betslip_query.Replace("[betslip_url]", betslip_url);
                betslip_query = betslip_query.Replace("[x-bwin-browser-url]", href);
                betslip_query = betslip_query.Replace("[X-XSRF-TOKEN]", xsrfToken);
                betslip_query = betslip_query.Replace("[body]", betslipReq);

                CDPController.Instance.AddBetRespBody = "";
                CDPController.Instance.ExecuteScript(betslip_query);
                int retryCnt = 0;
                while (string.IsNullOrEmpty(CDPController.Instance.AddBetRespBody))
                {
                    retryCnt++;
                    if (retryCnt > 30)
                        break;

                    Thread.Sleep(400);
                }

                if (string.IsNullOrEmpty(CDPController.Instance.AddBetRespBody)) 
                {
                    LogMng.Instance.onWriteStatus("Betslip failed. Try again.");
                    return PROCESS_RESULT.ERROR;
                }


                JObject jBetslipResp = JObject.Parse(CDPController.Instance.AddBetRespBody);
                dynamic jSelection = null;
                dynamic jMarketName = null;
                long pickId = 0;
                double newOdds = 0;

                try
                {
                    if(jBetslipResp["fixturePage"]["fixtures"][0]["optionMarkets"].Count() == 0)
                    {
                        foreach (var jMarket in jBetslipResp["fixturePage"]["fixtures"][0]["games"][0]["results"])
                        {
                            long marketId = (long)jMarket["id"];
                            if (marketId != Utils.parseToLong(codMarket))
                                continue;

                            jMarketName = jBetslipResp["fixturePage"]["fixtures"][0]["games"][0]["name"];
                            jSelection = jMarket["name"];
                            pickId = (long)jMarket["id"];
                            newOdds = (double)jMarket["odds"];

                            break;

                        }
                    }
                    else
                    {
                        foreach (var jMarket in jBetslipResp["fixturePage"]["fixtures"][0]["optionMarkets"][0]["options"])
                        {
                            long marketId = (long)jMarket["id"];
                            if (marketId != Utils.parseToLong(codMarket))
                                continue;

                            jSelection = jMarket["name"];
                            pickId = (long)jMarket["price"]["id"];
                            newOdds = (double)jMarket["price"]["odds"];

                            break;
                        }
                    }
                 
                }
                catch 
                {
                    LogMng.Instance.onWriteStatus("This line is not existed now...");
                    return PROCESS_RESULT.ERROR;
                }

                if(pickId == 0)
                {
                    LogMng.Instance.onWriteStatus("This line is not existed now...");
                    return PROCESS_RESULT.ERROR;
                }
                else if(pickId == -1)
                    pickId = Utils.parseToLong(codMarket);
                

                LogMng.Instance.onWriteStatus($"Pick Id : {pickId.ToString()}");
                if (CheckOddDropCancelBet(newOdds, info))
                {

                    LogMng.Instance.onWriteStatus($"Lower odd, cancelled. ({info.odds}) -> ({newOdds})");
                    return PROCESS_RESULT.ERROR;
                }

                var jSport = jBetslipResp["fixturePage"]["fixtures"][0]["sport"];
                var jCompetition = jBetslipResp["fixturePage"]["fixtures"][0]["competition"];
                var jRegion= jBetslipResp["fixturePage"]["fixtures"][0]["region"];
                var jEventName = jBetslipResp["fixturePage"]["fixtures"][0]["name"];
                if(jMarketName == null)
                    jMarketName = jBetslipResp["fixturePage"]["fixtures"][0]["optionMarkets"][0]["name"];

P:              BwinPlacebet placebetReq = new BwinPlacebet(codMarket);

                string guid = Utils.generateGuid();
                placebetReq.placeBetRequest.requestId = guid;
                placebetReq.betContextualDetails[0].requestId = guid;
                placebetReq.placeBetRequest.betSlips[0].stake.amount = info.stake;

                if (info.direct_link.Contains("-"))
                {
                    placebetReq.placeBetRequest.betSlips[0].bets[0].betModel = "Result";
                    placebetReq.placeBetRequest.betSlips[0].bets[0].additionalInformation.informationItems.RemoveAll(m => m.key == "fixtureType");
                }

                DateTime startTime = (DateTime) jBetslipResp["fixturePage"]["fixtures"][0]["startDate"];
                LogMng.Instance.onWriteStatus($"Start Time : {startTime.ToString("yyyy-MM-ddTHH:mm:ss.000Z")}");
                placebetReq.placeBetRequest.betSlips[0].bets[0].odds.european = info.odds;
                placebetReq.placeBetRequest.betSlips[0].bets[0].additionalInformation.informationItems[0].value = startTime.ToString("yyyy-MM-ddTHH:mm:ss.000Z");
                placebetReq.placeBetRequest.betSlips[0].bets[0].picks.Add(new Pick() { id = pickId.ToString() });

                placebetReq.placeBetRequest.betSlips[0].bets[0].betDetails = new List<BetDetail>()
                {
                    new BetDetail(jSport["id"].ToString() , jSport["type"].ToString() , jSport["name"]["value"].ToString() , jSport["name"]["sign"].ToString()),
                    new BetDetail(jRegion["id"].ToString() , jRegion["type"].ToString() , jRegion["name"]["value"].ToString() , jRegion["name"]["sign"].ToString()),
                    new BetDetail(jCompetition["id"].ToString() , jCompetition["type"].ToString() , jCompetition["name"]["value"].ToString() , jCompetition["name"]["sign"].ToString()),
                    new BetDetail(eventId , "Fixture" , jEventName["value"].ToString() , jEventName["sign"].ToString()),
                    new BetDetail(codSelection , "Market" , jMarketName["value"].ToString() , jMarketName["sign"].ToString()),
                    new BetDetail(codMarket , "Option" , jSelection["value"].ToString() , jSelection["sign"].ToString())
                };

                string placebetReqBody = JsonConvert.SerializeObject(placebetReq , Formatting.Indented);
                placebetReqBody = placebetReqBody.Replace("\r\n", string.Empty);

                string placebet_url = $"https://sports.{Setting.Instance.domain}/pt-br/sports/api/placebet/place";

                string placebet_query = Resources.ResourceManager.GetString("bwin_betslip_query");
                placebet_query = placebet_query.Replace("[betslip_url]", placebet_url);
                placebet_query = placebet_query.Replace("[x-bwin-browser-url]", href);
                placebet_query = placebet_query.Replace("[x-xsrf-token]", xsrfToken);
                placebet_query = placebet_query.Replace("[body]", placebetReqBody);

                CDPController.Instance.PlaceBetRespBody = "";
                CDPController.Instance.ExecuteScript(placebet_query);

                int rCnt = 0;
                while(string.IsNullOrEmpty(CDPController.Instance.PlaceBetRespBody))
                {
                    rCnt++;
                    if (rCnt > 30)
                        break;

                    Thread.Sleep(500);
                }


R:              JObject jPlacebet = JObject.Parse(CDPController.Instance.PlaceBetRespBody);
                if (jPlacebet["status"].ToString() == "Success")
                {

                    LogMng.Instance.onWriteStatus(string.Format("** PLACE BET SUCCESS"));
                    return PROCESS_RESULT.PLACE_SUCCESS;
                }
                else if(jPlacebet["status"].ToString() == "Pending")
                {
                    LogMng.Instance.onWriteStatus(string.Format("** It is pending now...Just wait few secs"));
                    Thread.Sleep(10000);

                    String confirmUrl = $"https://sports.{Setting.Instance.domain}/pt-br/sports/api/placebet/querystatus?requestId={guid}";

                    string confirm_query = Resources.ResourceManager.GetString("bwin_balance_query");

                    confirm_query = confirm_query.Replace("[balance_url]", confirmUrl);
                    confirm_query = confirm_query.Replace("[x-bwin-browser-url]", href);

                    CDPController.Instance.PlaceBetRespBody = "";
                    CDPController.Instance.ExecuteScript(confirm_query);

                    retryCnt = 0;
                    while (string.IsNullOrEmpty(CDPController.Instance.PlaceBetRespBody))
                    {
                        retryCnt++;
                        if (retryCnt > 30)
                            break;

                        Thread.Sleep(400);
                    }

                    goto R;
                }
                else if(jPlacebet["betslips"][0]["failed"] != null)
                {
                    foreach(var jError in jPlacebet["betslips"][0]["failed"]["errors"])
                    {
                        if(jError["type"].ToString() == "MaximumStakeForSingleBetsExceeded")
                        {
                            LogMng.Instance.onWriteStatus("***MaximumStakeForSingleBetsExceeded***");
                            info.stake = (double) jError["newStakeHint"];
                            if(info.stake == 0)
                                return PROCESS_RESULT.ZERO_MAX_STAKE;

                            goto P;
                        }
                    }
                }
                else
                {
                    LogMng.Instance.onWriteStatus(string.Format("** PLACE FAILED"));
                    return PROCESS_RESULT.ERROR;
                }
            }
            catch (Exception e)
            {

            }
            return PROCESS_RESULT.ERROR;
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

        public int GetPendingbets()
        {
            throw new NotImplementedException();
        }
    }
#endif
}
