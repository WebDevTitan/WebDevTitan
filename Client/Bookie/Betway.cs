namespace Project.Bookie
{
#if (BETWAY)
    class BetwayCtrl : IBookieController
    {
        
        public HttpClient m_client = null;
        
        Object lockerObj = new object();
        //private static ProxyServer _proxyServer = null;

        private JObject payloadObject = null;
        


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
        public BetwayCtrl()
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

                        Global.OpenUrl($"https://{Setting.Instance.domain}/en/sports");

                        int n_waitRetry = 10;
                        while (n_waitRetry-- >= 0)
                        {
                            string style = Global.RunScriptCode("document.querySelectorAll(\"input[placeholder='Username']\")[0].outerHTML");
                            if (style.Contains("input"))
                                break;
                            Thread.Sleep(1000);
                        }
                        if (n_waitRetry <= 0)
                            continue;

                        Thread.Sleep(1000);
                        Global.RunScriptCode($"document.querySelectorAll(\"input[placeholder='Username']\")[0].value='{Setting.Instance.username}';");

                        Global.RunScriptCode($"document.querySelectorAll(\"input[placeholder='Password']\")[0].value='{Setting.Instance.password}';");

                        Thread.Sleep(500);

                        Global.strPlaceBetResult = "";
                        Global.strWebResponse1 = "";
                        Global.waitResponseEvent1.Reset();

                        Global.RunScriptCode("document.querySelectorAll(\"input[class='loginSubmit']\")[0].click();");
                                                                       

                        if (!Global.waitResponseEvent1.Wait(5000))
                        {
                            LogMng.Instance.onWriteStatus($"Login No Response");
                            continue;
                        }

                        if (string.IsNullOrEmpty(Global.strPlaceBetResult))
                        {
                            LogMng.Instance.onWriteStatus($"Login No Capture request");
                            continue;
                        }

                        payloadObject = JsonConvert.DeserializeObject<JObject>(Global.strPlaceBetResult);

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
                            Task.Run(async () => await Global.GetCookie($"https://{Setting.Instance.domain}")).Wait();
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

            httpClientEx.DefaultRequestHeaders.Add("Host", $"{Setting.Instance.domain}");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Referer", $"https://{Setting.Instance.domain}/");

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

            string selectionId = "", marketId = "";
            try
            {
                info.direct_link = info.direct_link.Replace("\\\"", "\"");
                dynamic infoParams = JsonConvert.DeserializeObject<dynamic>(info.direct_link);
                info.outcome = infoParams.marketName.ToString();
                selectionId = infoParams.selectionId.ToString();    //outcomeId
                marketId = infoParams.marketId.ToString();
            }
            catch {
                LogMng.Instance.onWriteStatus($"direct_link error: {info.direct_link}");
                return PROCESS_RESULT.ERROR;
            }

            int nRetry = 0;
            while (nRetry++ < 2)
            {
                string getEventsMarketURL = $"https://sportsapi.{Setting.Instance.domain}/api/Events/v2/GetEventMarkets";

                JObject market_request = new JObject();
                market_request["ApplicationVersion"] = payloadObject["ApplicationVersion"];
                market_request["BrandId"] = payloadObject["BrandId"];
                market_request["BrowserId"] = payloadObject["BrowserId"];
                market_request["BrowserVersion"] = payloadObject["BrowserVersion"];                
                market_request["ClientIntegratorId"] = payloadObject["ClientIntegratorId"];
                market_request["ClientTypeId"] = payloadObject["ClientTypeId"];
                market_request["CorrelationId"] = Guid.NewGuid().ToString();                
                market_request["JourneyId"] = payloadObject["JourneyId"];
                market_request["JurisdictionId"] = payloadObject["JurisdictionId"];
                market_request["LanguageId"] = payloadObject["LanguageId"];
                market_request["OsId"] = payloadObject["OsId"];
                market_request["OsVersion"] = payloadObject["OsVersion"];
                market_request["SessionId"] = payloadObject["SessionId"];
                market_request["TerritoryId"] = payloadObject["TerritoryId"];
                market_request["ViewName"] = payloadObject["ViewName"];
                market_request["VisitId"] = payloadObject["VisitId"];

                JObject objScoreboardRequest = new JObject();
                objScoreboardRequest["IncidentRequest"] = new JObject();
                objScoreboardRequest["ScoreboardType"] = 3;
                
                market_request["MarketIds"] = new JArray(marketId);
                market_request["ScoreboardRequest"] = objScoreboardRequest;


                string formDataString = market_request.ToString().Replace("\r", "").Replace("\n", "");

                string functionString = $"window.fetch('{getEventsMarketURL}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json; charset=UTF-8', 'accept': 'application/json; charset=UTF-8' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                Global.strWebResponse2 = "";
                Global.waitResponseEvent2.Reset();

                Global.RunScriptCode(functionString);

                if (!Global.waitResponseEvent2.Wait(5000))
                {
                    LogMng.Instance.onWriteStatus($"GetEventMarkets no Response");
                    return PROCESS_RESULT.ERROR;
                }

                LogMng.Instance.onWriteStatus($"GetEventMarkets Req: {formDataString}");
                LogMng.Instance.onWriteStatus($"GetEventMarkets Res: {Global.strWebResponse2}");
                string OutcomeId = "", MarketId = "", EventId = "";
                dynamic getEventMarket_Response = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse2);
                foreach (dynamic outcome in getEventMarket_Response.Outcomes)
                {
                    if (outcome.Id.ToString() == selectionId)
                    {
                        OutcomeId = outcome.Id.ToString();
                        MarketId = outcome.MarketId.ToString();
                        EventId = outcome.EventId.ToString();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(EventId) || string.IsNullOrEmpty(MarketId) || string.IsNullOrEmpty(OutcomeId))
                {
                    LogMng.Instance.onWriteStatus("failed because of match doesn't exist");
                    return PROCESS_RESULT.ERROR;
                }
                string buildbetURL = $"https://sportsapi.{Setting.Instance.domain}/api/Betting/v3/BuildBets";

                JObject request = new JObject();
                request["ApplicationVersion"] = payloadObject["ApplicationVersion"];
                request["BrandId"] = payloadObject["BrandId"];
                request["BrowserId"] = payloadObject["BrowserId"];
                request["BrowserVersion"] = payloadObject["BrowserVersion"];
                request["ClientIntegratorId"] = payloadObject["ClientIntegratorId"];
                request["ClientTypeId"] = payloadObject["ClientTypeId"];
                request["CorrelationId"] = Guid.NewGuid().ToString();
                request["IncludeAccountCapabilities"] = "false";
                request["JourneyId"] = payloadObject["JourneyId"];
                request["JurisdictionId"] = payloadObject["JurisdictionId"];
                request["LanguageId"] = payloadObject["LanguageId"];
                request["OsId"] = payloadObject["OsId"];
                request["OsVersion"] = payloadObject["OsVersion"];
                request["SessionId"] = payloadObject["SessionId"];
                request["TerritoryId"] = payloadObject["TerritoryId"];
                request["ViewName"] = payloadObject["ViewName"];
                request["VisitId"] = payloadObject["VisitId"];

                JObject objBuildBetsRequestData = new JObject();
                //JObject objBalanceType = new JObject();
                //objBalanceType["Type"] = "cash";
                //objBalanceType["Value"] = "";

                objBuildBetsRequestData["BalanceTypes"] = new JArray();
                objBuildBetsRequestData["BetSelectionTypeId"] = 1;
                objBuildBetsRequestData["EventId"] = EventId;
                objBuildBetsRequestData["MarketId"] = MarketId;
                objBuildBetsRequestData["OutcomeIds"] = new JArray(OutcomeId);

                request["BuildBetsRequestData"] = new JArray(objBuildBetsRequestData);


                formDataString = request.ToString().Replace("\r", "").Replace("\n", "");

                functionString = $"window.fetch('{buildbetURL}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json; charset=UTF-8', 'accept': 'application/json; charset=UTF-8' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                Global.strWebResponse1 = "";
                Global.waitResponseEvent1.Reset();

                Global.RunScriptCode(functionString);

                if (!Global.waitResponseEvent1.Wait(5000))
                {
                    LogMng.Instance.onWriteStatus($"BuildBets no Response");
                    return PROCESS_RESULT.ERROR;
                }
                LogMng.Instance.onWriteStatus($"BuildBets Req: {formDataString}");
                LogMng.Instance.onWriteStatus($"BuildBets Res: {Global.strWebResponse1}");
                dynamic buildbets_Response = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse1);
                double newodd = Convert.ToDouble(buildbets_Response.Accumulators[0].Selections[0].PriceDecimal.ToString());

                if (CheckOddDropCancelBet(newodd, info))
                {

                    LogMng.Instance.onWriteStatus($"Lower odd, cancelled. ({info.odds}) -> ({newodd})");
                    return PROCESS_RESULT.MOVED;
                }
                dynamic selectionOutcomeDetails = buildbets_Response.OutcomeDetails[0];
                dynamic selectionMarketData = getEventMarket_Response.Markets[0];
                if (selectionMarketData.IsSuspended.ToString().ToLower() == "true")
                {
                    LogMng.Instance.onWriteStatus("market is suspended");
                    return PROCESS_RESULT.ERROR;
                }

                dynamic selectedOutcome = buildbets_Response.Accumulators[0]?.Selections[0]?.SubSelections[0];

                string initiatebetURL = $"https://sportsapi.{Setting.Instance.domain}/api/Betting/v3/InitiateBets";
                                
                JObject initiate_request = new JObject();
                initiate_request["ApplicationVersion"] = payloadObject["ApplicationVersion"];
                initiate_request["BetRequestId"] = Guid.NewGuid().ToString();
                JObject objBetsRequestData = new JObject();

                objBetsRequestData["AcceptPriceChange"] = 1;

                JObject objBetPlacements = new JObject();
                objBetPlacements["BetSelectionTypeId"] = 0;
                objBetPlacements["EachWay"] = false;
                objBetPlacements["NumberOfLines"] = 1;
                objBetPlacements["NumberOfLinesEachWay"] = 0;
                objBetPlacements["PriceDenominator"] = 0;
                objBetPlacements["PriceNumerator"] = 0;

                JObject objSelections = new JObject();
                objSelections["CashOutActive"] = selectionMarketData.CashOutActive;
                objSelections["EachWayActive"] = selectionMarketData.EachWayActive;
                objSelections["EventId"] = selectionMarketData.EventId;
                objSelections["EventName"] = selectionOutcomeDetails.EventName;
                objSelections["EventStartDateMiliseconds"] = 0;
                objSelections["Handicap"] = selectionMarketData.Handicap;
                objSelections["MarketCName"] = selectionMarketData.TypeCName;
                objSelections["MarketId"] = selectionMarketData.Id;
                objSelections["MarketName"] = selectionOutcomeDetails.MarketName;
                objSelections["PriceDecimal"] = selectedOutcome.PriceDecimal;
                double pricedecimal = Convert.ToDouble(selectedOutcome.PriceDecimal.ToString());
                objSelections["PriceDecimalDisplay"] = Math.Truncate(pricedecimal * 100) / 100;
                objSelections["PriceDenominator"] = selectedOutcome.PriceDenominator;
                objSelections["PriceNumerator"] = selectedOutcome.PriceNumerator;
                objSelections["PriceType"] = 1;

                JObject objSubSelections = new JObject();
                objSubSelections["OutcomeId"] = selectionOutcomeDetails.OutcomeId;
                objSubSelections["OutcomeName"] = selectionOutcomeDetails.OutcomeName;

                objSelections["SubSelections"] = new JArray(objSubSelections);

                objBetPlacements["Selections"] = new JArray(objSelections);
                objBetPlacements["StakePerLine"] = (int)(info.stake * 100);
                objBetPlacements["SystemCname"] = "single";
                objBetPlacements["UseFreeBet"] = false;

                objBetsRequestData["BetPlacements"] = new JArray(objBetPlacements);

                initiate_request["BetsRequestData"] = objBetsRequestData;

                initiate_request["BrandId"] = payloadObject["BrandId"];
                initiate_request["BrowserId"] = payloadObject["BrowserId"];
                initiate_request["BrowserVersion"] = payloadObject["BrowserVersion"];
                initiate_request["ClientIntegratorId"] = payloadObject["ClientIntegratorId"];
                initiate_request["ClientTypeId"] = payloadObject["ClientTypeId"];
                initiate_request["CorrelationId"] = Guid.NewGuid().ToString();                
                initiate_request["JourneyId"] = payloadObject["JourneyId"];
                initiate_request["JurisdictionId"] = payloadObject["JurisdictionId"];
                initiate_request["LanguageId"] = payloadObject["LanguageId"];
                initiate_request["OsId"] = payloadObject["OsId"];
                initiate_request["OsVersion"] = payloadObject["OsVersion"];
                initiate_request["SessionId"] = payloadObject["SessionId"];
                initiate_request["TerritoryId"] = payloadObject["TerritoryId"];
                initiate_request["ViewName"] = payloadObject["ViewName"];
                initiate_request["VisitId"] = payloadObject["VisitId"];

                formDataString = initiate_request.ToString().Replace("\r", "").Replace("\n", "");                
                functionString = $"window.fetch('{initiatebetURL}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json; charset=UTF-8', 'accept': 'application/json; charset=UTF-8' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                Global.strWebResponse2 = "";
                Global.waitResponseEvent2.Reset();

                Global.RunScriptCode(functionString);

                if (!Global.waitResponseEvent2.Wait(5000))
                {
                    LogMng.Instance.onWriteStatus($"InitiateBets no Response");
                    return PROCESS_RESULT.ERROR;
                }
                LogMng.Instance.onWriteStatus($"InitiateBets Req: {formDataString}");
                LogMng.Instance.onWriteStatus($"InitiateBets Res: {Global.strWebResponse2}");
                dynamic initiatebet_Response = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse2);
                
                if (initiatebet_Response.Success.ToString().ToLower() != "true")
                {
                    LogMng.Instance.onWriteStatus(initiatebet_Response.MethodResult.ToString());
                    return PROCESS_RESULT.ERROR;
                }

                int nLookupRetry = 30;
                while (nLookupRetry-- > 0)
                {  
                    Thread.Sleep(1000);

                    string lookupbetURL = $"https://sportsapi.{Setting.Instance.domain}/api/Betting/v3/LookupBets";

                    JObject lookup_request = new JObject();
                    lookup_request["ApplicationVersion"] = payloadObject["ApplicationVersion"];
                    lookup_request["BetRequestId"] = initiatebet_Response.BetRequestId.ToString();
                    lookup_request["BrandId"] = payloadObject["BrandId"];
                    lookup_request["BrowserId"] = payloadObject["BrowserId"];
                    lookup_request["BrowserVersion"] = payloadObject["BrowserVersion"];
                    lookup_request["ClientIntegratorId"] = payloadObject["ClientIntegratorId"];
                    lookup_request["ClientTypeId"] = payloadObject["ClientTypeId"];
                    lookup_request["CorrelationId"] = Guid.NewGuid().ToString();
                    lookup_request["IncludeAccountCapabilities"] = "false";
                    lookup_request["JourneyId"] = payloadObject["JourneyId"];
                    lookup_request["JurisdictionId"] = payloadObject["JurisdictionId"];
                    lookup_request["LanguageId"] = payloadObject["LanguageId"];
                    lookup_request["OsId"] = payloadObject["OsId"];
                    lookup_request["OsVersion"] = payloadObject["OsVersion"];
                    lookup_request["OutcomeIds"] = new JArray(selectionOutcomeDetails.OutcomeId);
                    lookup_request["SessionId"] = payloadObject["SessionId"];
                    lookup_request["TerritoryId"] = payloadObject["TerritoryId"];
                    lookup_request["ViewName"] = payloadObject["ViewName"];
                    lookup_request["VisitId"] = payloadObject["VisitId"];


                    formDataString = lookup_request.ToString().Replace("\r", "").Replace("\n", "");

                    functionString = $"window.fetch('{lookupbetURL}', {{ headers: {{ accept: 'application/json, text/plain, */*', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                    Global.strWebResponse4 = "";
                    Global.waitResponseEvent4.Reset();

                    Global.RunScriptCode(functionString);

                    if (!Global.waitResponseEvent4.Wait(5000))
                    {
                        LogMng.Instance.onWriteStatus($"LookupBets no Response");
                        return PROCESS_RESULT.ERROR;
                    }

                    LogMng.Instance.onWriteStatus($"LookupBets Req: {formDataString}");
                    LogMng.Instance.onWriteStatus($"LookupBets Res: {Global.strWebResponse4}");
                    dynamic lookupbets_Response = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse4);

                    if (lookupbets_Response.BetStatus.ToString() == "3")
                    {
                        return PROCESS_RESULT.PLACE_SUCCESS;
                        
                    }

                    if (lookupbets_Response.BetStatus.ToString() != "2" || lookupbets_Response.Success.ToString().ToLower() == "false")
                    {
                        
                        if (lookupbets_Response.ErrorInformation[0].MarketSuspended.ToString().ToLower() == "true" || lookupbets_Response.ErrorInformation[0].OutcomeSuspended.ToString().ToLower() == "true")
                        {
                            LogMng.Instance.onWriteStatus(string.Format("Place Bet Failed - Market or Outcome is suspended"));
                            return PROCESS_RESULT.ERROR;
                        }

                        if (lookupbets_Response.ErrorInformation[0].MarketClosed.ToString().ToLower() == "true" || lookupbets_Response.ErrorInformation[0].EventClosed.ToString().ToLower() == "true")
                        {
                            LogMng.Instance.onWriteStatus(string.Format("Place Bet Failed - Market or Event is closed"));
                            return PROCESS_RESULT.ERROR;
                        }

                        if (lookupbets_Response.Errors[0].ErrorCode.ToString() == 302 && lookupbets_Response.Errors[0].Title.ToString() == "odds")
                        {
                            LogMng.Instance.onWriteStatus(string.Format("Place Bet Failed - Odd is changed"));
                            return PROCESS_RESULT.ERROR;
                        }

                        try
                        {
                            double MaxBet = Convert.ToDouble(lookupbets_Response.Errors[0].BetLimitDetails[0].MaxBet.ToString());
                            info.stake = MaxBet / 100;
                            LogMng.Instance.onWriteStatus($"Maxbet is set, change stakge: {info.stake}");
                            break;
                        }
                        catch { }
                        
                    }                    
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
                JObject request = new JObject();
                request["ApplicationVersion"] = payloadObject["ApplicationVersion"];
                request["BrandId"] = payloadObject["BrandId"];
                request["BrowserId"] = payloadObject["BrowserId"];
                request["BrowserVersion"] = payloadObject["BrowserVersion"];
                request["ClientIntegratorId"] = payloadObject["ClientIntegratorId"];
                request["ClientTypeId"] = payloadObject["ClientTypeId"];
                request["CorrelationId"] = Guid.NewGuid().ToString();
                request["IncludeAccountCapabilities"] = "false";
                request["JourneyId"] = payloadObject["JourneyId"];
                request["JurisdictionId"] = payloadObject["JurisdictionId"];
                request["LanguageId"] = payloadObject["LanguageId"];
                request["OsId"] = payloadObject["OsId"];
                request["OsVersion"] = payloadObject["OsVersion"];
                request["SessionId"] = payloadObject["SessionId"];
                request["TerritoryId"] = payloadObject["TerritoryId"];
                request["ViewName"] = payloadObject["ViewName"];
                request["VisitId"] = payloadObject["VisitId"];

                string getBalanceURL = $"https://sportsapi.{Setting.Instance.domain}/api/Account/v3/Info";
               
                string formDataString = request.ToString().Replace("\r","").Replace("\n", "");
                string functionString = $"window.fetch('{getBalanceURL}', {{ headers: {{ accept: 'application/json, text/plain, */*', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";


                Global.strWebResponse3 = "";
                Global.waitResponseEvent3.Reset();

                //LogMng.Instance.onWriteStatus($"GetBalance request: {functionString}");
                Global.RunScriptCode(functionString);

                if (Global.waitResponseEvent3.Wait(300000))
                {
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"GetBalance result: {Global.strWebResponse3}");
#endif
                    dynamic details = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse3);

                    balance = Utils.ParseToDouble(details.CustomerInfo.Balances.Balance.ToString());
                    balance = balance / 100;
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
