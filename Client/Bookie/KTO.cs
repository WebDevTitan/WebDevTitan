namespace Project.Bookie
{
#if (KTO)
    class KTO : IBookieController
    {
        public HttpClient m_client = null;
        public string accountId = "";
        public string auth_token = "";
        public string sport_token = "";
        public string api_token = "";
        public KTO()
        {
            if (Setting.Instance.browserType == 0)
            {
                if (CDPController.Instance._browserObj == null)
                    CDPController.Instance.InitializeBrowser("https://www.kto.com/pt");
            }
            else
            {
                if (DolphinController.Instance.browser == null)
                    DolphinController.Instance.InitBrowser("https://www.kto.com/pt");
            }

            m_client = initHttpClient();
        }

        public int GetPendingbets()
        {
            return 0;
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

            handler.CookieContainer = Global.cookieContainer;
            handler.UseCookies = true;

            HttpClient httpClientEx = new HttpClient(handler);
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");

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
            bool bLogin = false;
            if (Setting.Instance.browserType == 0)
                bLogin = login_cdp();
            else
                bLogin = login_puputter();

            return bLogin;
        }
        public bool login_cdp()
        {
            if (!Global.bRun)
                return false;
            //
            bool bLogin = false;

            auth_token = CDPController.Instance.ExecuteScript("localStorage['@kto:access_token']", true);
            if (!string.IsNullOrEmpty(auth_token))
            {
                LogMng.Instance.onWriteStatus("Logged In Already!");
                bLogin = true;
                return bLogin;
            }
            int nRetry = 0;
            while (nRetry++ < 3)
            {
                try
                {
                    CDPController.Instance.NavigateInvoke("https://www.kto.com/pt/login/");
                    Thread.Sleep(10000);

                    bool isFound = CDPController.Instance.FindAndClickElement(1, "input[id='username']", 3).Result;
                    if (isFound)
                    {
                        CDPMouseController.Instance.InputText(Setting.Instance.username);
                        Thread.Sleep(1000);
                    }

                    isFound = CDPController.Instance.FindAndClickElement(1, "input[id='password']", 3).Result;
                    if (isFound)
                    {
                        CDPMouseController.Instance.InputText(Setting.Instance.password);
                        Thread.Sleep(1000);
                    }

                    CDPController.Instance.loginRespBody = string.Empty;
                    isFound = CDPController.Instance.FindAndClickElement(1, "button[id='login']", 1).Result;

                    int retryCnt = 0;
                    while (string.IsNullOrEmpty(CDPController.Instance.loginRespBody))
                    {
                        if (retryCnt > 30)
                            break;

                        retryCnt++;
                        Thread.Sleep(500);
                    }


                    JObject jResp = JObject.Parse(CDPController.Instance.loginRespBody);
                    auth_token = jResp["data"]["access_token"].ToString();

                    bLogin = true;
                    break;

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
        public bool login_puputter()
        {
            if (!Global.bRun)
                return false;
            //
            bool bLogin = false;
            Thread.Sleep(25000);

            auth_token = DolphinController.Instance.ExecuteScript("localStorage['@kto:access_token']");
            if (!string.IsNullOrEmpty(auth_token))
            {
                LogMng.Instance.onWriteStatus("Logged In Already!");
                bLogin = true;
                return bLogin;
            }
            DolphinController.Instance.NavigateInvoke("https://www.kto.com/pt/login/");
            Thread.Sleep(25000);

            int nRetry = 0;
            while (nRetry++ < 3)
            {
                try
                {
                    string doEventJScript = "function doEvent(obj, event ) { var event = new Event( event, { target: obj, bubbles: true} ); return obj ? obj.dispatchEvent(event) : false; }";
                    DolphinController.Instance.ExecuteScript(doEventJScript);

                    string input_name_script = $"var el = document.getElementById('username');el.value = '{Setting.Instance.username}';doEvent(el, 'input');";
                    DolphinController.Instance.ExecuteScript(input_name_script);
                    bool isFound = DolphinController.Instance.FindAndClick("input[id='username']", 3);

                    string input_password_script = $"var el = document.getElementById('password');el.value ='{Setting.Instance.password}';doEvent(el, 'input');";
                    DolphinController.Instance.ExecuteScript(input_password_script);
                    isFound = DolphinController.Instance.FindAndClick("input[id='password']", 3);

                    /*string username = Setting.Instance.username;
                    bool isFound = DolphinController.Instance.FindAndClick("input[id='username']" , 3);
                    if (isFound)
                    {
                        DolphinController.Instance.InputText(Setting.Instance.username);
                        Thread.Sleep(1000);
                    }

                    string password = Setting.Instance.password;
                    isFound = DolphinController.Instance.FindAndClick("input[id='password']",3);
                    if (isFound)
                    {
                        DolphinController.Instance.InputText(Setting.Instance.password);
                        Thread.Sleep(1000);
                    }*/

                    DolphinController.Instance.loginRespBody = string.Empty;
                    Thread.Sleep(3000);

                    isFound = DolphinController.Instance.FindAndClick("button[id='login']" , 1);
                    Thread.Sleep(800);
                    isFound = DolphinController.Instance.FindAndClick("button[id='login']", 1);
                    Thread.Sleep(800);
                    isFound = DolphinController.Instance.FindAndClick("button[id='login']", 1);

                    int retryCnt = 0;
                    while (string.IsNullOrEmpty(DolphinController.Instance.loginRespBody))
                    {
                        if (retryCnt > 30)
                            break;

                        retryCnt++;
                        Thread.Sleep(500);
                    }

                    if (string.IsNullOrEmpty(DolphinController.Instance.loginRespBody))
                        continue;

                    JObject jResp = JObject.Parse(DolphinController.Instance.loginRespBody);
                    auth_token = jResp["data"]["access_token"].ToString();

                    bLogin = true;
                    break;

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

        public PROCESS_RESULT PlaceBet(ref BetburgerInfo info)
        {
            Thread.Sleep(1000 * Setting.Instance.requestDelay);

            if (getBalance() < 0)
            {
                if (!login())
                    return PROCESS_RESULT.NO_LOGIN;
            }

            GetSportToken();

            int retryCount = 2;
            while (--retryCount >= 0)
            {
                try
                {
                    string eventId = info.siteUrl.Split('/')[info.siteUrl.Split('/').Length - 1];
                    string outcomeId = string.Empty, marketId = string.Empty, sportId = string.Empty;
                    string sv = string.Empty , market_name = string.Empty , outcome_name = string.Empty;
                    long marketTypeId = 0;
                    long outcomeTypeId = 0;

                    foreach (string str in info.direct_link.Split('&'))
                    {
                        if (str.Contains("outcomeId"))
                            outcomeId = str.Replace("outcomeId=", string.Empty).Trim();
                        else if (str.Contains("marketId"))
                            marketId = str.Replace("marketId=", string.Empty).Trim();
                        else if (str.Contains("sportId"))
                            sportId = str.Replace("sportId=", string.Empty).Trim();
                    }

                    //Get Event Detail
                    m_client.DefaultRequestHeaders.Remove("X-Requested-With");
                    m_client.DefaultRequestHeaders.Remove("Authorization");
                    string eventRespBody = string.Empty;

                    if (Setting.Instance.browserType == 0)
                    {
                        CDPController.Instance.eventRespBody = string.Empty;
                        //string event_query = Properties.Resources.kto_event_query.Replace("[request_url]", $"https://sb2frontend-altenar2.biahosted.com/api/widget/GetEventDetails?culture=en-GB&timezoneOffset=-480&integration=kto.com&deviceType=1&numFormat=en-GB&eventId={eventId}");
                        //CDPController.Instance.ExecuteScript(event_query);
                        CDPController.Instance.ExecuteScript($"GetRequest('{$"https://sb2frontend-altenar2.biahosted.com/api/widget/GetEventDetails?culture=en-GB&timezoneOffset=-480&integration=kto.com&deviceType=1&numFormat=en-GB&eventId={eventId}"}')");

                        int retry_cnt = 0;
                        while (string.IsNullOrEmpty(CDPController.Instance.eventRespBody))
                        {
                            retry_cnt++;
                            if (retry_cnt > 20)
                                break;

                            Thread.Sleep(500);
                        }

                        eventRespBody = CDPController.Instance.eventRespBody;
                    }
                    else
                    {
                        DolphinController.Instance.eventRespBody = string.Empty;
                        //string event_query = Properties.Resources.kto_event_query.Replace("[request_url]", $"https://sb2frontend-altenar2.biahosted.com/api/widget/GetEventDetails?culture=en-GB&timezoneOffset=-480&integration=kto.com&deviceType=1&numFormat=en-GB&eventId={eventId}");
                        //CDPController.Instance.ExecuteScript(event_query);
                        DolphinController.Instance.ExecuteScript($"GetRequest('{$"https://sb2frontend-altenar2.biahosted.com/api/widget/GetEventDetails?culture=en-GB&timezoneOffset=-480&integration=kto.com&deviceType=1&numFormat=en-GB&eventId={eventId}"}')");

                        int retry_cnt = 0;
                        while (string.IsNullOrEmpty(DolphinController.Instance.eventRespBody))
                        {
                            retry_cnt++;
                            if (retry_cnt > 20)
                                break;

                            Thread.Sleep(500);
                        }

                        eventRespBody = DolphinController.Instance.eventRespBody;
                    }
                    

                    //HttpResponseMessage respMessage = m_client.GetAsync($"https://sb2frontend-altenar2.biahosted.com/api/widget/GetEventDetails?culture=en-GB&timezoneOffset=-480&integration=kto.com&deviceType=1&numFormat=en-GB&eventId={eventId}").Result;

                    //string respBody = respMessage.Content.ReadAsStringAsync().Result;
                    JObject jResp = JObject.Parse(eventRespBody);

                    foreach (var jMarket in jResp["markets"])
                    {
                        if (jMarket["id"].ToString() == marketId)
                        {
                            marketTypeId = (long)jMarket["typeId"];
                            market_name = jMarket["name"].ToString();
                            if (jMarket["sv"] != null)
                                sv = jMarket["sv"].ToString();
                        }
                    }

                    foreach (var jOdd in jResp["odds"])
                    {
                        if (jOdd["id"].ToString() == outcomeId)
                        {
                            outcomeTypeId = (long) jOdd["typeId"];
                            outcome_name = jOdd["name"].ToString();
                        }
                    }

                    if(marketTypeId == 0 || outcomeTypeId == 0)
                    {
                        LogMng.Instance.onWriteStatus("***Not Found Line or odds!***");
                        return PROCESS_RESULT.MOVED;
                    }

                    string champ_name = jResp["champ"]["name"].ToString();
                    string sport_name = jResp["sport"]["name"].ToString();
                    string event_name = jResp["name"].ToString();
                    string cat_name = jResp["category"]["name"].ToString();

                    string payload = "";
                    if (!string.IsNullOrEmpty(sv))
                    {
                        payload = $"{{\"culture\":\"en-GB\",\"timezoneOffset\":-480,\"integration\":\"kto.com\",\"deviceType\":1,\"numFormat\":\"en-GB\",\"countryCode\":\"BR\",\"odds\":[{{\"oddId\":{outcomeId},\"price\":{info.odds},\"eventId\":{eventId},\"marketTypeId\":{marketTypeId},\"spov\":\"{sv}\",\"selectionTypeId\":{outcomeTypeId},\"sportTypeId\":{sportId},\"isBoost\":false}}]}}";
                    }
                    else
                        payload = $"{{\"culture\":\"en-GB\",\"timezoneOffset\":-480,\"integration\":\"kto.com\",\"deviceType\":1,\"numFormat\":\"en-GB\",\"countryCode\":\"BR\",\"odds\":[{{\"oddId\":{outcomeId},\"price\":{info.odds},\"eventId\":{eventId},\"marketTypeId\":{marketTypeId},\"selectionTypeId\":{outcomeTypeId},\"sportTypeId\":{sportId},\"isBoost\":false}}]}}";

                    string addbetRespBody = string.Empty;
                    if (Setting.Instance.browserType == 0)
                    {
                        CDPController.Instance.AddBetRespBody = string.Empty;
                        CDPController.Instance.ExecuteScript($"PostRequest('{payload}' , 'https://sb2frontend-altenar2.biahosted.com/api/Widget/GetOddsStates')");

                        int retry_cnt = 0;
                        while (string.IsNullOrEmpty(CDPController.Instance.AddBetRespBody))
                        {
                            retry_cnt++;
                            if (retry_cnt > 20)
                                break;

                            Thread.Sleep(500);
                        }

                        addbetRespBody = CDPController.Instance.AddBetRespBody;
                    }
                    else 
                    {
                        DolphinController.Instance.AddBetRespBody = string.Empty;
                        DolphinController.Instance.ExecuteScript($"PostRequest('{payload}' , 'https://sb2frontend-altenar2.biahosted.com/api/Widget/GetOddsStates')");

                        int retry_cnt = 0;
                        while (string.IsNullOrEmpty(DolphinController.Instance.AddBetRespBody))
                        {
                            retry_cnt++;
                            if (retry_cnt > 20)
                                break;

                            Thread.Sleep(500);
                        }

                        addbetRespBody = DolphinController.Instance.AddBetRespBody;
                    }
                       

                    if (string.IsNullOrEmpty(addbetRespBody))
                    {
                        LogMng.Instance.onWriteStatus("****Betslip Failed!***");
                        return PROCESS_RESULT.ERROR;
                    }

                    JObject jBetslip = JObject.Parse(addbetRespBody);
                    double newOdds = (double)jBetslip["oddStates"][0]["price"];
                    if (CheckOddDropCancelBet(newOdds, info))
                    {
                        return PROCESS_RESULT.MOVED;
                    }

                    int retryCnt = 3;
                    while(retryCnt-- > 0)
                    {
                        string requestId = string.Empty;
                        if (Setting.Instance.browserType == 0)
                            requestId = CDPController.Instance.ExecuteScript("getRandToken(21)", true);
                        else
                            requestId = DolphinController.Instance.ExecuteScript("getRandToken(21)");

                        KTO_Placebet jPlacebet = new KTO_Placebet();
                        jPlacebet.stakes.Add(info.stake);
                        jPlacebet.requestId = requestId;

                        BetMarket betMarket = new BetMarket();
                        jPlacebet.betMarkets.Add(betMarket);

                        betMarket.catName = cat_name;
                        betMarket.champName = champ_name;
                        betMarket.dbId = 10;
                        betMarket.eventName = event_name;
                        betMarket.id = Utils.parseToLong(eventId);
                        betMarket.sportName = sport_name;
                        betMarket.sportTypeId = Utils.parseToInt(sportId);

                        KTO_Odd kto_odds = new KTO_Odd();

                        kto_odds.id = Utils.parseToLong(outcomeId);
                        kto_odds.marketId = Utils.parseToLong(marketId);
                        kto_odds.marketName = market_name;
                        kto_odds.marketTypeId = (int)marketTypeId;
                        kto_odds.price = newOdds;
                        kto_odds.selectionName = outcome_name;
                        kto_odds.selectionTypeId = (int)outcomeTypeId;
                        kto_odds.sPOV = sv;

                        betMarket.odds.Add(kto_odds);

                        string placebet_payload = JsonConvert.SerializeObject(jPlacebet);
                        string placebetRespBody = string.Empty;

                        if (Setting.Instance.browserType == 0)
                        {
                            CDPController.Instance.PlaceBetRespBody = string.Empty;
                            CDPController.Instance.ExecuteScript($"PostRequest('{placebet_payload}' , 'https://sb2bets-altenar2.biahosted.com/api/widget/placeWidget' , '{api_token}')");

                            int retry_cnt = 0;
                            while (string.IsNullOrEmpty(CDPController.Instance.PlaceBetRespBody))
                            {
                                retry_cnt++;
                                if (retry_cnt > 20)
                                    break;

                                Thread.Sleep(500);
                            }
                            placebetRespBody = CDPController.Instance.PlaceBetRespBody;
                        }
                        else
                        {
                            DolphinController.Instance.PlaceBetRespBody = string.Empty;
                            DolphinController.Instance.ExecuteScript($"PostRequest('{placebet_payload}' , 'https://sb2bets-altenar2.biahosted.com/api/widget/placeWidget' , '{api_token}')");

                            int retry_cnt = 0;
                            while (string.IsNullOrEmpty(DolphinController.Instance.PlaceBetRespBody))
                            {
                                retry_cnt++;
                                if (retry_cnt > 20)
                                    break;

                                Thread.Sleep(500);
                            }
                            placebetRespBody = DolphinController.Instance.PlaceBetRespBody;
                        }


                        if (string.IsNullOrEmpty(placebetRespBody))
                        {
                            LogMng.Instance.onWriteStatus("****Placebet Failed!***");
                            return PROCESS_RESULT.ERROR;
                        }

                        JObject jPlacebetResp = JObject.Parse(placebetRespBody);
                        if (placebetRespBody.Contains("error"))
                        {
                            if (jPlacebetResp["error"]["errorType"].ToString() == "1") 
                            {
                                double maxStake = Utils.ParseToDouble(jPlacebetResp["error"]["totalStake"].ToString());

                                if (!Setting.Instance.bEnableMaxbetSuperbet)
                                {
                                    info.stake = maxStake;
                                    LogMng.Instance.onWriteStatus($"RePlace bet now!");
                                    continue;
                                }
                                else
                                {
                                    maxStake = Utils.ParseToDouble(Utils.Between(jResp["notice"].ToString(), "Your bet was over the maximum amount you can place", "BRL").Trim());
                                    if (maxStake < Setting.Instance.MaxStakeLimit)
                                    {
                                        LogMng.Instance.onWriteStatus($"Max stake : {maxStake} Stake : {info.stake}");
                                        if (maxStake < info.stake)
                                            TelegramCtrl.Instance.sendMessage($"Possible limitation on the {Setting.Instance.username} account on the KTO website");

                                        LogMng.Instance.onWriteStatus($"Max Stake is lower than Configured Value ({Setting.Instance.MaxStakeLimit})");
                                        return PROCESS_RESULT.ZERO_MAX_STAKE;
                                    }

                                    info.stake = maxStake;
                                    LogMng.Instance.onWriteStatus($"RePlace bet now!");
                                    continue;
                                }
                            }
                            
                        }
                        if (jPlacebetResp["bets"] != null && !string.IsNullOrEmpty(jPlacebetResp["bets"][0]["createdDate"].ToString()))
                        {
                            return PROCESS_RESULT.PLACE_SUCCESS;
                        }
                        else
                        {
                            LogMng.Instance.onWriteStatus(placebetRespBody);
                            return PROCESS_RESULT.ERROR;
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }
            LogMng.Instance.onWriteStatus(string.Format("** PLACE BET FAIL"));
            return PROCESS_RESULT.ERROR;
        }

        private void GetSportToken()
        {
            try
            {
                m_client.DefaultRequestHeaders.Remove("X-Requested-With");
                m_client.DefaultRequestHeaders.Remove("Authorization");

                m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "KTOWeb_1.230.1");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {auth_token}");

                HttpResponseMessage respMessage = m_client.GetAsync("https://api.kto.com/sportsbook/v2/altenar/token/v2").Result;
                respMessage.EnsureSuccessStatusCode();

                string respBody = respMessage.Content.ReadAsStringAsync().Result;
                JObject jResp = JObject.Parse(respBody);

                sport_token = jResp["token"].ToString();

                string payload = $"{{\"culture\":\"pt-BR\",\"timezoneOffset\":-480,\"integration\":\"kto.com\",\"deviceType\":1,\"numFormat\":\"en-GB\",\"token\":\"{sport_token}\",\"walletCode\":\"209702\"}}";

                string eventRespBody = string.Empty;
                if (Setting.Instance.browserType == 0)
                {
                    CDPController.Instance.eventRespBody = string.Empty;
                    CDPController.Instance.ExecuteScript($"PostRequest('{payload}' , 'https://sb2auth-altenar2.biahosted.com/api/WidgetAuth/SignIn')");
                    int retryCnt = 0;
                    while (string.IsNullOrEmpty(CDPController.Instance.eventRespBody))
                    {
                        retryCnt++;
                        if (retryCnt > 20)
                            break;

                        Thread.Sleep(400);
                    }
                    eventRespBody = CDPController.Instance.eventRespBody;
                }
                else
                {
                    DolphinController.Instance.loginRespBody = string.Empty;
                    DolphinController.Instance.ExecuteScript($"PostRequest('{payload}' , 'https://sb2auth-altenar2.biahosted.com/api/WidgetAuth/SignIn')");
                    int retryCnt = 0;
                    while (string.IsNullOrEmpty(DolphinController.Instance.loginRespBody))
                    {
                        retryCnt++;
                        if (retryCnt > 20)
                            break;

                        Thread.Sleep(400);
                    }
                    eventRespBody = DolphinController.Instance.loginRespBody;
                }
                   

                JObject jAccess = JObject.Parse(eventRespBody);
                api_token = jAccess["accessToken"].ToString();
            }
            catch { }
        }
        public double getBalance()
        {
            double balance = -1;
            int retryCount = 2;
#if OXYLABS
            retryCount = 5;
#endif
            while (--retryCount >= 0)
            {
                try
                {
                    m_client.DefaultRequestHeaders.Remove("X-Requested-With");
                    m_client.DefaultRequestHeaders.Remove("Authorization");

                    m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "KTOWeb_1.230.1");
                    m_client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {auth_token}");

                    HttpResponseMessage respMessage = m_client.GetAsync("https://api.kto.com/wallet").Result;
                    respMessage.EnsureSuccessStatusCode();

                    string respBody = respMessage.Content.ReadAsStringAsync().Result;
                    JObject jResp = JObject.Parse(respBody);

                    balance = (double)jResp["data"]["amount"];
                    break;
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
            return balance;
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
        private string GetRequestId()
        {
            Random rand = new Random();
            rand.NextDouble();

            string requestId = Utils.getTick().ToString() + "x" + Math.Round(rand.NextDouble() * 1000000);
            return requestId;
        }

        private void GetEventDetail(string marketId  , string outcomeId, ref string sv , ref long marketTypeId , ref long selectionTypeId)
        {
            try
            {
               
              
            }
            catch { }
        }

        private void GetCookies(HttpResponseMessage message)
        {

            try
            {
                message.Headers.TryGetValues("Set-Cookie", out var cookiesHeader);
                List<Cookie> cookies = cookiesHeader.Select(cookieString => CreateCookie(cookieString)).ToList();
                foreach (Cookie cookie in cookies)
                {
                    if (cookie == null)
                        continue;

                    Global.cookieContainer.Add(new Uri($"https://login.{Setting.Instance.domain}/"), new Cookie(cookie.Name, cookie.Value));
                    Global.cookieContainer.Add(new Uri($"https://api.{Setting.Instance.domain}/"), new Cookie(cookie.Name, cookie.Value));
                    Global.cookieContainer.Add(new Uri($"https://www.{Setting.Instance.domain}/"), new Cookie(cookie.Name, cookie.Value));
                }
            }
            catch { }
        }

        private Cookie CreateCookie(string cookieString)
        {
            try
            {
                var properties = cookieString.Split(';');
                var name = properties[0].Split('=')[0];
                var value = properties[0].Split('=')[1];

                var path = properties[2].Replace("path=", "");
                var cookie = new Cookie(name, value, path);
                return cookie;
            }
            catch
            {
                return null;
            }

        }

    }
#endif
}
