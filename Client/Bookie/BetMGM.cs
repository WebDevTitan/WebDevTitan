namespace Project.Bookie
{
#if (BETMGM)
    class BetMGMCtrl : IBookieController
    {
        private double balance = -1;
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

        public bool logout()
        {
            return true;
        }
        public BetMGMCtrl()
        {
            domain = Setting.Instance.domain.ToLower();
            if (domain.StartsWith("www."))
                domain = domain.Replace("www.", "");
            if (domain.StartsWith("sports."))
                domain = domain.Replace("sports.", "");

            Global.placeBetHeaderCollection.Clear();
            m_client = initHttpClient();
            //Global.SetMonitorVisible(false);
        }

        public bool Pulse()
        {
            return false;
           
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
            LogMng.Instance.onWriteStatus("Login start , check balance");
            if (getBalance() >= 0)
            {
                LogMng.Instance.onWriteStatus("balance is ok, it's logined already");
                return true;
            }
            LogMng.Instance.onWriteStatus("login from begining");
            Global.betMGMrequestQueries.Clear();

            domain = Setting.Instance.domain.ToLower();
            if (domain.StartsWith("www."))
                domain = domain.Replace("www.", "");
            if (domain.StartsWith("sports."))
                domain = domain.Replace("sports.", "");

            int nTotalRetry = 0;
            //Global.SetMonitorVisible(true);
            bool bLogin = false;
            while (nTotalRetry++ < 2)
            {
                try
                {

                    lock (lockerObj)
                    {
                        //Global.RemoveCookies();
                        m_client = initHttpClient();



                        Global.OpenUrl($"https://sports.{domain}/en/sports");

                        Thread.Sleep(3000);
                        Global.OpenUrl($"https://sports.{domain}/en/labelhost/login");

                        //Global.RunScriptCode("document.getElementsByClassName('uk-button uk-button-primary GTM-login')[0].click();");


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
                        string posResult = Global.GetStatusValue("return JSON.stringify(document.getElementById('userId').getBoundingClientRect());");
                        Rect iconRect = Utils.ParseRectFromJson(posResult);
                        x = left + iconRect.X + iconRect.Width / 2;
                        y = top + iconRect.Y + iconRect.Height / 2;


#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Click Pos (username): {x} {y}");
#endif

                        ClickOnPoint(Global.ViewerHwnd, new Point(x, y));
                        Thread.Sleep(500);
                        SendKeys.SendWait(Setting.Instance.username);

                        //Global.RunScriptCode($"document.getElementById('userName').value='{Setting.Instance.username}';");

                        Thread.Sleep(1000);
                        posResult = Global.GetStatusValue("return JSON.stringify(document.getElementsByName('password')[0].getBoundingClientRect());");
                        iconRect = Utils.ParseRectFromJson(posResult);
                        x = left + iconRect.X + iconRect.Width / 2;
                        y = top + iconRect.Y + iconRect.Height / 2;


#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Click Pos (password): {x} {y}");
#endif

                        ClickOnPoint(Global.ViewerHwnd, new Point(x, y));
                        Thread.Sleep(500);
                        //Global.RunScriptCode($"document.getElementById('password').value='{Setting.Instance.password}';");
                        SendKeys.SendWait(Setting.Instance.password);
                        Thread.Sleep(1000);

                        //moveThread.Abort();


                        posResult = Global.GetStatusValue("return JSON.stringify(document.getElementsByClassName('login btn btn-primary')[0].getBoundingClientRect());");
                        iconRect = Utils.ParseRectFromJson(posResult);
                        x = left + iconRect.X + iconRect.Width / 2;
                        y = top + iconRect.Y + iconRect.Height / 2;

#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Click Pos (login button): {x} {y}");
#endif

                        //SetCursorPos((int)x, (int)y);
                        ClickOnPoint(Global.ViewerHwnd, new Point(x, y));

                        Thread.Sleep(5000);

                        int nRetry1 = 0;
                        while (nRetry1++ < 100)
                        {
                            string curPagelink = Global.GetStatusValue("return location.href;");
                            LogMng.Instance.onWriteStatus($"check if 2nd verification needed: {curPagelink}");
                            if (curPagelink.Contains("mobileportal/twofa") ||
                                curPagelink.Contains("mobileportal/strongauthv2"))
                            {
                                LogMng.Instance.onWriteStatus("waiting 2nd verification");
                                Thread.Sleep(3000);
                            }
                            else
                            {
                                break;
                            }
                        }

                        nRetry1 = 0;
                        while (nRetry1++ < 5)
                        {
                            Thread.Sleep(1000);
                            //Task.Run(async () => await Global.GetCookie($"https://sports.{domain}")).Wait();
                            if (getBalance() >= 0)
                            {
                                LogMng.Instance.onWriteStatus("Login Success");
                                return true;
                            }
                        }


                        
                    }
                }
                catch (Exception e)
                {
                    LogMng.Instance.onWriteStatus($"login exception {e.StackTrace} {e.Message}");
                }
            }
            LogMng.Instance.onWriteStatus("Login Failed");
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

            info[0].result = PlaceBet(info[0].betburgerInfo);
        }

        public PROCESS_RESULT PlaceBet(BetburgerInfo info)
        {            
            if (getBalance() < 0)
            {
                if (!login())
                    return PROCESS_RESULT.NO_LOGIN;
            }

            string gameId = info.direct_link.Split('|')[0].Trim();
            string marketId = info.direct_link.Split('|')[1].Trim();

            if (string.IsNullOrEmpty(gameId) || string.IsNullOrEmpty(marketId) || string.IsNullOrEmpty(info.siteUrl))
            {
                LogMng.Instance.onWriteStatus($"parameters error gameId: {gameId} marketId: {marketId} siteUrl: {info.siteUrl}");
                return PROCESS_RESULT.ERROR;
            }

            string[] siteUrlparams = info.siteUrl.Split('-');
            string fixtureId = siteUrlparams[siteUrlparams.Length - 1];

            Global.strWebResponse1 = "";
            Global.waitResponseEvent1.Reset();

            LogMng.Instance.onWriteStatus($"parameters gameId: {gameId} marketId: {marketId} fixtureId: {fixtureId} siteUrl: {info.siteUrl}");

            string pageUrl = $"https://sports.{domain}/en/sports/events/{info.siteUrl}";
            Global.OpenUrl(pageUrl);


            if (!Global.waitResponseEvent1.Wait(5000))
            {
                LogMng.Instance.onWriteStatus($"Get Fixture-view No Response");
                return PROCESS_RESULT.ERROR;
            }
            LogMng.Instance.onWriteStatus($"OpenPage Response: {Global.strWebResponse1.Trim()}");

            string Source = "";
            try
            {
                dynamic jsonResResp = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse1.Trim());
                Source = jsonResResp.fixture.source.ToString();
            }
            catch { }

            //LogMng.Instance.onWriteStatus($"Curpick Source: {Source}");
            string betURL = $"https://sports.{domain}/cds-api/bettingoffer/picks?x-bwin-accessid={Global.betMGMrequestQueries.Get("x-bwin-accessid")}&lang={Global.betMGMrequestQueries.Get("lang")}&country={Global.betMGMrequestQueries.Get("country")}&userCountry={Global.betMGMrequestQueries.Get("userCountry")}&subdivision={Global.betMGMrequestQueries.Get("subdivision")}";

            string formDataString = "";
            if (Source == "V1")
            {
                formDataString = $"{{\"picks\":[{{\"fixtureId\":\"{fixtureId}\",\"gameId\":{gameId},\"offerSource\":\"{Source}\",\"useLiveFallback\":false}}]}}";
            }
            else if (Source == "V2")
            {
                formDataString = $"{{\"picks\":[{{\"fixtureId\":\"{fixtureId}\",\"optionMarketId\":{gameId},\"offerSource\":\"{Source}\"}}]}}";
            }
            else
            {
                LogMng.Instance.onWriteStatus($"Source format is incorrect: {Source}");
                return PROCESS_RESULT.ERROR;
            }

            
            string functionString = $"window.fetch('{betURL}', {{ headers: {{ accept: 'application/json, text/plain, */*', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

            Global.strWebResponse2 = "";            
            Global.waitResponseEvent2.Reset();
//#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"Get Picks Request: {functionString}");
//#endif
            Global.RunScriptCode(functionString);


            if (!Global.waitResponseEvent2.Wait(5000))
            {
                LogMng.Instance.onWriteStatus($"Get Picks No Response");
                return PROCESS_RESULT.ERROR;
            }

            string strWebResponse2 = Global.strWebResponse2;

            //#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"Get Picks Result: {strWebResponse2}");
            //#endif
            
            if (string.IsNullOrEmpty(strWebResponse2))
            {
                LogMng.Instance.onWriteStatus("[placeBet] Could not get Picks info");
                return PROCESS_RESULT.ERROR;
            }
            
            int nRetry = 0;
            while (nRetry++ < 2)
            {
                bool bFoundMarket = false;
                JObject placePlaceParam = new JObject();

                #region OLD_2023_2
                //try
                //{
                //    dynamic jsonResResp = JsonConvert.DeserializeObject<dynamic>(strWebResponse2.Trim());


                //    placePlaceParam.Add("type", "Single");
                //    placePlaceParam.Add("odds", "Higher");

                //    if (Source == "V1")
                //    {
                //        JObject bets = new JObject();
                //        placePlaceParam.Add("bets", new JArray(bets));

                //        bets.Add("sportId", jsonResResp.fixturePage.fixtures[0].sport.id);
                //        bets.Add("sportName", jsonResResp.fixturePage.fixtures[0].sport.name);

                //        bets.Add("regionId", jsonResResp.fixturePage.fixtures[0].region.id);
                //        bets.Add("regionName", jsonResResp.fixturePage.fixtures[0].region.name);

                //        bets.Add("leagueId", jsonResResp.fixturePage.fixtures[0].competition.parentLeagueId);
                //        bets.Add("leagueName", jsonResResp.fixturePage.fixtures[0].competition.name);

                //        bets.Add("eventId", jsonResResp.fixturePage.fixtures[0].id);
                //        bets.Add("eventName", jsonResResp.fixturePage.fixtures[0].name);

                //        bets.Add("eventStartsAt", jsonResResp.fixturePage.fixtures[0].startDate);

                //        foreach (var game in jsonResResp.fixturePage.fixtures[0].games)
                //        {
                //            if (game.id.ToString() == gameId)
                //            {
                //                bets.Add("marketId", game.id);
                //                bets.Add("marketName", game.name);

                //                foreach (var result in game.results)
                //                {
                //                    if (result.id.ToString() == marketId)
                //                    {
                //                        bFoundMarket = true;
                //                        bets.Add("optionId", result.id);
                //                        bets.Add("optionName", result.name);

                //                        JObject oddsInUserFormat = new JObject();
                //                        oddsInUserFormat.Add("oddsFormat", "american");
                //                        oddsInUserFormat.Add("usOdds", result.americanOdds);

                //                        bets.Add("oddsInUserFormat", oddsInUserFormat);

                //                        bets.Add("oddsFormat", "american");
                //                        bets.Add("stake", info.stake);
                //                        bets.Add("index", Utils.GetUIDofSlip());
                //                        bets.Add("isBanker", false);
                //                        bets.Add("stakeTaxation", null);
                //                        break;
                //                    }
                //                }
                //                break;
                //            }
                //        }
                //        placePlaceParam.Add("freeformBets", new JArray());
                //    }
                //    else if (Source == "V2")
                //    {
                //        JObject bets = new JObject();
                //        placePlaceParam.Add("freeformBets", new JArray(bets));

                //        bets.Add("sportId", jsonResResp.fixturePage.fixtures[0].sport.id);
                //        bets.Add("sportName", jsonResResp.fixturePage.fixtures[0].sport.name);

                //        bets.Add("regionId", jsonResResp.fixturePage.fixtures[0].region.id);
                //        bets.Add("regionName", jsonResResp.fixturePage.fixtures[0].region.name);

                //        bets.Add("leagueId", jsonResResp.fixturePage.fixtures[0].competition.id);
                //        bets.Add("leagueName", jsonResResp.fixturePage.fixtures[0].competition.name);

                //        bets.Add("meetingId", jsonResResp.fixturePage.fixtures[0].competition.id);
                //        bets.Add("meetingName", jsonResResp.fixturePage.fixtures[0].competition.name);

                //        bets.Add("eventId", jsonResResp.fixturePage.fixtures[0].id);
                //        bets.Add("eventName", jsonResResp.fixturePage.fixtures[0].name);

                //        bets.Add("fixtureStartsAt", jsonResResp.fixturePage.fixtures[0].startDate);
                //        bets.Add("fixtureType", jsonResResp.fixturePage.fixtures[0].fixtureType);
                //        bets.Add("fixtureName", jsonResResp.fixturePage.fixtures[0].name);
                //        bets.Add("fixtureId", jsonResResp.fixturePage.fixtures[0].sourceId);


                //        foreach (var optionMarket in jsonResResp.fixturePage.fixtures[0].optionMarkets)
                //        {
                //            if (optionMarket.id.ToString() == gameId)
                //            {
                //                bets.Add("marketId", optionMarket.id);
                //                bets.Add("marketName", optionMarket.name);

                //                foreach (var option in optionMarket.options)
                //                {
                //                    if (option.id.ToString() == marketId)
                //                    {
                //                        bFoundMarket = true;
                //                        bets.Add("optionId", option.id);
                //                        bets.Add("optionName", option.name);
                //                        bets.Add("optionPriceId", option.price.id);

                //                        JObject oddsInUserFormat = new JObject();
                //                        oddsInUserFormat.Add("oddsFormat", "american");
                //                        oddsInUserFormat.Add("usOdds", option.price.americanOdds);

                //                        bets.Add("oddsInUserFormat", oddsInUserFormat);

                //                        bets.Add("oddsFormat", "american");
                //                        bets.Add("stake", info.stake);
                //                        bets.Add("index", Utils.GetUIDofSlip());
                //                        bets.Add("isBanker", false);
                //                        bets.Add("stakeTaxation", null);
                //                        break;
                //                    }
                //                }
                //                break;
                //            }
                //        }
                //        placePlaceParam.Add("bets", new JArray());
                //    }


                //    placePlaceParam.Add("raceBets", new JArray());
                //    placePlaceParam.Add("participantPicks", new JArray());
                //    placePlaceParam.Add("stake", null);
                //    placePlaceParam.Add("stakeTaxation", null);
                //    placePlaceParam.Add("systemType", null);
                //    placePlaceParam.Add("doubleBetPrevention", "Disabled");

                //    placePlaceParam.Add("walletType", null);

                //    placePlaceParam.Add("appNotification", false);
                //    placePlaceParam.Add("smsNotification", false);
                //    placePlaceParam.Add("emailNotification", false);
                //    placePlaceParam.Add("requestId", Guid.NewGuid().ToString());
                //    placePlaceParam.Add("trackBetContext", new JArray());

                //}
                //catch { }
                #endregion

                try
                {
                    dynamic jsonResResp = JsonConvert.DeserializeObject<dynamic>(strWebResponse2.Trim());
                    
                    JObject placeBetRequest = new JObject();

                    
                    JObject betSlips = new JObject();
                        


                    betSlips.Add("betSlipType", "Single");
                    betSlips.Add("index", 1);

                    JObject stake = new JObject();
                    betSlips.Add("stake", stake);
                    stake.Add("amount", info.stake);
                    stake.Add("currency", "USD");

                    betSlips.Add("stakeTaxation", null);

                    JObject additionalInformation = new JObject();
                    betSlips.Add("additionalInformation", additionalInformation);
                    additionalInformation.Add("informationItems", new JArray());

                    betSlips.Add("edsPromoTokens", new JArray());
                    betSlips.Add("oddsAcceptanceMode", "Higher");
                    betSlips.Add("promoTokens", new JArray());
                    betSlips.Add("overAskOfferDetails", null);
                    betSlips.Add("systemSlipDetails", null);

                    JObject bets = new JObject();
                    betSlips.Add("bets", new JArray(bets));

                    bets.Add("index", 1);
                    bets.Add("isBanker", false);

                    if (Source == "V1")
                    {
                        bets.Add("betType", "Result");
                    }
                    else if (Source == "V2")
                    {
                        bets.Add("betType", "Option");
                    }
                                              

                    JArray betDetails = new JArray();
                    bets.Add("betDetails", betDetails);

                    JObject betDetailSport = new JObject();
                    betDetailSport.Add("id", jsonResResp.fixturePage.fixtures[0].sport.id);
                    betDetailSport.Add("betDetailType", "Sport");
                    betDetailSport.Add("textValue", jsonResResp.fixturePage.fixtures[0].sport.name);
                    betDetails.Add(betDetailSport);

                    JObject betDetailRegion = new JObject();
                    betDetailRegion.Add("id", jsonResResp.fixturePage.fixtures[0].region.id);
                    betDetailRegion.Add("betDetailType", "Region");
                    betDetailRegion.Add("textValue", jsonResResp.fixturePage.fixtures[0].region.name);
                    betDetails.Add(betDetailRegion);

                    JObject betDetailCompetition = new JObject();
                    betDetailCompetition.Add("id", jsonResResp.fixturePage.fixtures[0].competition.id);
                    betDetailCompetition.Add("betDetailType", "Competition");
                    betDetailCompetition.Add("textValue", jsonResResp.fixturePage.fixtures[0].competition.name);
                    betDetails.Add(betDetailCompetition);

                    JObject betDetailFixture = new JObject();
                    betDetailFixture.Add("id", jsonResResp.fixturePage.fixtures[0].sourceId);
                    betDetailFixture.Add("betDetailType", "Fixture");
                    JObject Fixture_textValue = new JObject();
                    betDetailFixture.Add("textValue", Fixture_textValue);
                    if (jsonResResp.fixturePage.fixtures[0].name.@short == null)
                    {
                        Fixture_textValue.Add("value", jsonResResp.fixturePage.fixtures[0].name.value);
                        Fixture_textValue.Add("sign", jsonResResp.fixturePage.fixtures[0].name.sign);
                    }
                    else
                    {
                        Fixture_textValue.Add("value", jsonResResp.fixturePage.fixtures[0].name.@short);
                        Fixture_textValue.Add("sign", jsonResResp.fixturePage.fixtures[0].name.shortSign);
                    }
                    betDetails.Add(betDetailFixture);

                    if (Source == "V1")
                    {
                        foreach (var game in jsonResResp.fixturePage.fixtures[0].games)
                        {
                            if (game.id.ToString() == gameId)
                            {
                                JObject betDetailMarket = new JObject();
                                betDetailMarket.Add("id", game.id);
                                betDetailMarket.Add("betDetailType", "Market");
                                betDetailMarket.Add("textValue", game.name);
                                betDetails.Add(betDetailMarket);


                                foreach (var result in game.results)
                                {
                                    if (result.id.ToString() == marketId)
                                    {
                                        bFoundMarket = true;

                                        JObject betDetailOption = new JObject();
                                        betDetailOption.Add("id", result.id);
                                        betDetailOption.Add("betDetailType", "Option");
                                        betDetailOption.Add("textValue", result.name);
                                        betDetails.Add(betDetailOption);

                                        JObject odds = new JObject();
                                        odds.Add("oddsFormat", "American");
                                        odds.Add("american", result.americanOdds);
                                        bets.Add("odds", odds);

                                        bets.Add("oddsFormat", "American");

                                        JObject additionalInformation1 = new JObject();
                                        JArray informationItems = new JArray();
                                        additionalInformation1.Add("informationItems", informationItems);

                                        JObject informationItem1 = new JObject();
                                        informationItem1.Add("key", "fixtureStartTime");
                                        informationItem1.Add("value", jsonResResp.fixturePage.fixtures[0].startDate);
                                        informationItem1.Add("valueType", "DateTime");
                                        informationItems.Add(informationItem1);

                                        bets.Add("additionalInformation", additionalInformation1);

                                        JObject picks = new JObject();
                                        picks.Add("id", result.id);
                                        bets.Add("picks", new JArray(picks));
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                    else if (Source == "V2")
                    {
                        foreach (var optionMarket in jsonResResp.fixturePage.fixtures[0].optionMarkets)
                        {
                            if (optionMarket.id.ToString() == gameId)
                            {
                                JObject betDetailMarket = new JObject();
                                betDetailMarket.Add("id", optionMarket.id);
                                betDetailMarket.Add("betDetailType", "Market");
                                betDetailMarket.Add("textValue", optionMarket.name);
                                betDetails.Add(betDetailMarket);


                                foreach (var option in optionMarket.options)
                                {
                                    if (option.id.ToString() == marketId)
                                    {
                                        bFoundMarket = true;

                                        JObject betDetailOption = new JObject();
                                        betDetailOption.Add("id", option.id);
                                        betDetailOption.Add("betDetailType", "Option");
                                        betDetailOption.Add("textValue", option.name);
                                        betDetails.Add(betDetailOption);

                                        JObject odds = new JObject();
                                        odds.Add("oddsFormat", "American");
                                        odds.Add("american", option.price.americanOdds);
                                        bets.Add("odds", odds);

                                        bets.Add("oddsFormat", "American");

                                        JObject additionalInformation1 = new JObject();
                                        JArray informationItems = new JArray();
                                        additionalInformation1.Add("informationItems", informationItems);

                                        JObject informationItem1 = new JObject();
                                        informationItem1.Add("key", "fixtureStartTime");
                                        informationItem1.Add("value", jsonResResp.fixturePage.fixtures[0].startDate);
                                        informationItem1.Add("valueType", "DateTime");
                                        informationItems.Add(informationItem1);

                                        JObject informationItem2 = new JObject();
                                        informationItem2.Add("key", "fixtureType");
                                        informationItem2.Add("value", jsonResResp.fixturePage.fixtures[0].fixtureType);
                                        informationItem2.Add("valueType", "Enum");
                                        informationItems.Add(informationItem2);
                                        bets.Add("additionalInformation", additionalInformation1);

                                        JObject picks = new JObject();
                                        picks.Add("id", option.price.id);
                                        bets.Add("picks", new JArray(picks));
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                    placeBetRequest.Add("betSlips", new JArray(betSlips));                        
                    


                    placeBetRequest.Add("requestId", Guid.NewGuid().ToString());
                    placeBetRequest.Add("doubleBetPreventionMode", "Disabled");

                    placePlaceParam.Add("placeBetRequest", placeBetRequest);
                    placePlaceParam.Add("betContextualDetails", new JArray());
                }
                catch { }

                if (!bFoundMarket)
                {
                    LogMng.Instance.onWriteStatus("Not Found market in Picks result");
                    return PROCESS_RESULT.ERROR;
                }

                if (domain.Contains("az."))
                {
                    betURL = $"https://sports.{domain}/en/sports/api/placebet/place?forceFresh=1";
                }
                else
                {
                    betURL = $"https://sports.{domain}/en/sports/api/placebet?forceFresh=1";
                }
                string externalHeaders = "";
                try
                {
                    Monitor.Enter(Global.locker_unifiedclientHeaders);
                    foreach (KeyValuePair<string, string> param in Global.kambicdnHeaders)
                    {
                        if (param.Key.ToLower().StartsWith("x-"))
                        {
                            //LogMng.Instance.onWriteStatus($"add additional headers: {param.Key} : {param.Value}");
                            externalHeaders += $", '{param.Key}': '{param.Value}'";
                        }
                    }
                }
                catch { }
                finally
                {
                    Monitor.Exit(Global.locker_unifiedclientHeaders);
                }

                formDataString = placePlaceParam.ToString(Formatting.None).Replace("\'", "\\\'");
                functionString = $"window.fetch('{betURL}', {{ headers: {{ 'accept': 'application/json, text/plain, */*', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/json', 'sports-api-version': 'SportsAPIv1' {externalHeaders}}}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                Global.strWebResponse3 = "";
                Global.waitResponseEvent3.Reset();
                //#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Placebet Request: {functionString}");
                //#endif
                Global.RunScriptCode(functionString);


                if (!Global.waitResponseEvent3.Wait(20000))
                {
                    LogMng.Instance.onWriteStatus($"Get Placebet No Response");
                    return PROCESS_RESULT.ERROR;
                }


                //#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Placebet Result: {Global.strWebResponse3}");
                //#endif

                if (string.IsNullOrEmpty(Global.strWebResponse3))
                {
                    LogMng.Instance.onWriteStatus("[placeBet] Could not place bet , no response");
                    return PROCESS_RESULT.ERROR;
                }


                try
                {
                    dynamic pickResponse = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse3.Trim());

                    if (pickResponse != null && pickResponse.betslips != null && pickResponse.betslips[0].status == "Succeeded")
                    {
                        LogMng.Instance.onWriteStatus($"[placeBet] Successfully placed bet {pickResponse.betslips[0].betNumber}");
                        return PROCESS_RESULT.PLACE_SUCCESS;
                    }
                    else if (pickResponse != null && pickResponse.betslips != null && pickResponse.betslips[0].status == "Failed")
                    {
                        if (pickResponse.betslips[0].errors != null && pickResponse.betslips[0].errors[0].type == "MaximumWinOfBetExceeded")
                        {
                            info.stake = Utils.FractionToDouble(pickResponse.betslips[0].errors[0].newStakeHint.ToString());
                            LogMng.Instance.onWriteStatus($"Retry with max stake: {info.stake}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMng.Instance.onWriteStatus($"apiPlaceBet exception {ex}");
                }
            }

            return PROCESS_RESULT.ERROR;
        }
        public double getBalance()
        {
            double balance = -1;
            if (Setting.Instance.domain.Contains("az."))
            {
                try
                {

                    string getBalanceURL = $"https://sports.{Setting.Instance.domain}/en/api/clientconfig/partial?configNames=vnBalanceProperties&forceFresh=1";
                    string baseURL = $"https://sports.{Setting.Instance.domain}/en/sports/";
                    string functionString = $"window.fetch('{getBalanceURL}', {{ method: 'GET', headers: {{ 'accept': 'application/json, text/plain, */*', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/json', }}, credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', referrer: '{baseURL}' }}).then(response => response.json());";

                    Global.strAddBetResult = "";
                    Global.waitResponseEvent.Reset();

                    LogMng.Instance.onWriteStatus($"GetBalance request: {functionString}");
                    Global.RunScriptCode(functionString);

                    if (Global.waitResponseEvent.Wait(3000))
                    {
                        LogMng.Instance.onWriteStatus($"GetBalance result: {Global.strAddBetResult}");
                        dynamic details = JsonConvert.DeserializeObject<dynamic>(Global.strAddBetResult);

                        balance = Utils.ParseToDouble(details.vnBalanceProperties.balanceProperties.accountBalance.ToString());
                    }
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                try
                {

                    string getBalanceURL = $"https://sports.{Setting.Instance.domain}/en/api/balance/refresh?forceFresh=1";
                    string baseURL = $"https://sports.{Setting.Instance.domain}/en/sports/";
                    string functionString = $"window.fetch('{getBalanceURL}', {{ method: 'GET', headers: {{ 'accept': 'application/json, text/plain, */*', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/json', }}, credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', referrer: '{baseURL}' }}).then(response => response.json());";

                    Global.strAddBetResult = "";
                    Global.waitResponseEvent.Reset();

                    //LogMng.Instance.onWriteStatus($"GetBalance request: {functionString}");
                    Global.RunScriptCode(functionString);

                    if (Global.waitResponseEvent.Wait(3000))
                    {
                        //LogMng.Instance.onWriteStatus($"GetBalance result: {Global.strAddBetResult}");
                        dynamic details = JsonConvert.DeserializeObject<dynamic>(Global.strAddBetResult);

                        balance = Utils.ParseToDouble(details.vnBalance.accountBalance.ToString());
                    }
                }
                catch (Exception e)
                {

                }
            }
            Global.balance = balance;
            LogMng.Instance.onWriteStatus($"GetBalance Result: {balance}");
            return balance;
        }
    }
    
#endif
}
