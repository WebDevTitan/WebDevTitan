﻿namespace Project.Bookie
{
#if (BETPLAY)
    

    public class FingerPrintHash
    {
        public string UserId { get; set; }
        public string Hash { get; set; }

        public FingerPrintHash(string userId, string hash)
        {
            UserId = userId;
            Hash = hash;
        }
    }

    public class BetResultJson
    {
        public List<Errors> betErrors { get; set; } = new List<Errors>();
        public string couponRef { get; set; }
        public string status { get; set; }
    }

    public class Errors
    {
        [JsonProperty("errors")]
        public List<ErrorItem> items { get; set; }
    }

    public class ErrorItem
    {
        public string type { get; set; }
        [JsonProperty("arguments")]
        public List<Argument> arguments { get; set; }
    }

    public class Argument
    {
        public string type { get; set; }
        public string value { get; set; }
    }
    
    public class AuthTokenJson
    {
        public string authtoken { get; set; }
        public string market { get; set; }
        public string playerId { get; set; }
        public string streamingEnabled { get; set; }
        public string offering { get; set; }
        public string country { get; set; }
        public string lang { get; set; }
    }

    class BetplayCtrl : IBookieController
    {
        public HttpClient m_client = null;
             
        Object lockerObj = new object();

        static string FingerPrintHashFileName = "FPHList.dat";

        public List<FingerPrintHash> HashList = new List<FingerPrintHash>();
        private string domain = "betplay.com.co";
        UnibetSessionInfo unibetSessionInfo = null;


        public string GetFingerPrintHash()
        {
            string result = ""; // "fc49a49db4ff70108398dd084f545ae8";
            foreach (var hashItr in HashList)
            {
                if (hashItr.UserId == Setting.Instance.username)
                {
                    result = hashItr.Hash;
                    break;
                }
            }
            return result;
        }

        public bool logout()
        {
            return true;
        }

        public void Feature()
        {

        }
        public void Close()
        {
        }

        public int GetPendingbets()
        {
            return 0;
        }
        public bool FetchFingerPrintHash()
        {
            bool bResult = false;
            LogMng.Instance.onWriteStatus("Login account manually for fetching account information");
            
            Global.waitResponseEvent.Reset();
            if (Global.waitResponseEvent.Wait(60000))
            {
                for (int i = HashList.Count - 1; i >= 0; i--)
                {
                    if (HashList[i].UserId == Setting.Instance.username)
                        HashList.RemoveAt(i);
                }
                HashList.Add(new FingerPrintHash(Setting.Instance.username, Global.strPlaceBetResult));
                string jsonHashList = JsonConvert.SerializeObject(HashList);
                File.WriteAllText(FingerPrintHashFileName, jsonHashList);
                bResult = true;
                LogMng.Instance.onWriteStatus("Capturing Hash value success");
            }
            else
            {
                bResult = false;
                LogMng.Instance.onWriteStatus("Capturing Hash value failed");
            }            
            return bResult;
        }

        public void OpenFingerPrintHashFile()
        {
            try
            {
                string jsonHashList = File.ReadAllText(FingerPrintHashFileName);
                HashList = JsonConvert.DeserializeObject<List<FingerPrintHash>>(jsonHashList);
            }
            catch { }
        }
        public bool Pulse()
        {
            return false;
        }

        public BetplayCtrl()
        {                      
            m_client = initHttpClient();

            //if (string.IsNullOrEmpty(GetFingerPrintHash()))
            //{
            //    FetchFingerPrintHash();
            //}
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
            ServicePointManager.DefaultConnectionLimit = 2;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Referer", $"https://{domain}/");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Origin", $"https://{domain}");

            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "cross-site");
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;

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
           

            if (!Global.bRun)
                return false;
            try
            {
                   
                Global.OpenUrl($"https://{domain}");
                Global.strAddBetResult = "";
                Global.waitResponseEvent.Reset();

                string result = Global.GetStatusValue("return localStorage.profile;");
                if (result == "null")
                {


                    int nRetry1 = 0;
                    while (nRetry1 < 10)
                    {
                        Thread.Sleep(500);
                        result = Global.GetStatusValue("return document.getElementById('dropdownLogin').outerHTML;");

#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Login button status : {result}");
#endif

                        if (result.Contains("class"))
                        {
                            break;
                        }
                        nRetry1++;
                    }
                    if (nRetry1 >= 30)       //Page is loading gray page. let's retry
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Can't open login page");
#endif
                        return false;
                    }

                    //Thread moveThread = new Thread(MouseMove);
                    //moveThread.Start();
                    Rect monitorRect = Global.GetMonitorPos();
                    double top = monitorRect.Top;
                    double left = monitorRect.Left;
                    SetForegroundWindow(Global.ViewerHwnd);

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"window left: {left} top: {top}");
#endif

                    Thread.Sleep(2000);
                    double x, y;
                    string posResult = Global.GetStatusValue("return JSON.stringify(document.getElementById('dropdownLogin').getBoundingClientRect());");
                    Rect iconRect = Utils.ParseRect(posResult);
                    x = left + iconRect.X + iconRect.Width / 2;
                    y = top + iconRect.Y + iconRect.Height / 2;

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Click Pos (login dropdown): {x} {y}");
#endif

                    ClickOnPoint(Global.ViewerHwnd, new Point(x, y));


                    //Global.RunScriptCode("document.getElementById('dropdownLogin').click();");
                    Thread.Sleep(1000);

                    posResult = Global.GetStatusValue("return JSON.stringify(document.getElementById('userName').getBoundingClientRect());");
                    iconRect = Utils.ParseRect(posResult);
                    x = left + iconRect.X + iconRect.Width / 2;
                    y = top + iconRect.Y + iconRect.Height / 2;
                    //SetCursorPos((int)x, (int)y);

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Click Pos (username): {x} {y}");
#endif

                    ClickOnPoint(Global.ViewerHwnd, new Point(x, y));

                    SendKeys.SendWait(Setting.Instance.username);

                    //Global.RunScriptCode($"document.getElementById('userName').value='{Setting.Instance.username}';");

                    Thread.Sleep(1000);
                    posResult = Global.GetStatusValue("return JSON.stringify(document.getElementById('password').getBoundingClientRect());");
                    iconRect = Utils.ParseRect(posResult);
                    x = left + iconRect.X + iconRect.Width / 2;
                    y = top + iconRect.Y + iconRect.Height / 2;
                    //SetCursorPos((int)x, (int)y);

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Click Pos (password): {x} {y}");
#endif

                    ClickOnPoint(Global.ViewerHwnd, new Point(x, y));

                    //Global.RunScriptCode($"document.getElementById('password').value='{Setting.Instance.password}';");
                    SendKeys.SendWait(Setting.Instance.password);
                    Thread.Sleep(1000);

                    //moveThread.Abort();


                    posResult = Global.GetStatusValue("return JSON.stringify(document.getElementById('btnLoginPrimary').getBoundingClientRect());");
                    iconRect = Utils.ParseRect(posResult);
                    x = left + iconRect.X + iconRect.Width / 2;
                    y = top + iconRect.Y + iconRect.Height / 2;

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Click Pos (login button): {x} {y}");
#endif


                    Thread.Sleep(2000);
                    string test = Global.GetStatusValue("return document.getElementById('btnLoginPrimary').outerHTML;");
                    LogMng.Instance.onWriteStatus($"class {test}");

                    //SetCursorPos((int)x, (int)y);
                    ClickOnPoint(Global.ViewerHwnd, new Point(x, y));
                    //Global.RunScriptCode("document.getElementById('btnLoginPrimary').click();");


                    //string command = "var xhr = new XMLHttpRequest();" +
                    //                "xhr.open('POST', 'https://" + domain + "/reverse-proxy/accounts/sessions/complete', true);" +

                    //                "xhr.withCredentials = true;" +
                    //                "xhr.setRequestHeader('Accept', 'application/json, text/plain, */*');" +
                    //                "xhr.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');" +
                    //                "xhr.onreadystatechange = function() {" +
                    //                "  if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {" +
                    //                "	  window.location.reload();" +
                    //                "  }" +
                    //                "};" +
                    //                "xhr.send(JSON.stringify({ 'userName': '" + Setting.Instance.username + "', 'password': '" + Setting.Instance.password + "'}));";
                    //Global.RunScriptCode(command);

                    nRetry1 = 0;
                    while (nRetry1 < 20)
                    {
                        Thread.Sleep(500);

                        result = Global.GetStatusValue("return localStorage.profile;").ToLower();
                        if (result != "null")
                        {
                            break;
                        }
                    }
                }
                LogMng.Instance.onWriteStatus($"Betplay Login Successed, get token");

                Global.OpenUrl($"https://{domain}/apuestas#home");
                

                if (Global.waitResponseEvent.Wait(120000))
                {
                    unibetSessionInfo = JsonConvert.DeserializeObject<UnibetSessionInfo>(Global.strAddBetResult);
                    LogMng.Instance.onWriteStatus("Capturing Hash value success");
                    return true;
                }
                else
                {                                
                    LogMng.Instance.onWriteStatus("Capturing Hash value failed");
                    return false;
                }
            }
            catch(Exception ex)
            {
                LogMng.Instance.onWriteStatus($"Login exception {ex}");
            }
          

            LogMng.Instance.onWriteStatus($"Login Failed");
            return false;
        }

        public PROCESS_RESULT PlaceBet(ref BetburgerInfo info)
        {
            string result = Global.GetStatusValue("return localStorage.profile;").ToLower();
            if (result == "null")
            {
                if (!login())
                {
                    return PROCESS_RESULT.NO_LOGIN;
                }
            }

            try
            {
                //if (!GetApiKey())
                //    return PROCESS_RESULT.CRITICAL_SITUATION;


                string stakeStr = "";
                try
                {
                    double stakeF = info.stake * 1000;
                    stakeStr = stakeF.ToString();
                }
                catch (Exception e)
                {

                }

                List<OpenBet_Unibet> markets = new List<OpenBet_Unibet> { Utils.ConvertBetburgerPick2OpenBet_Unibet(info) };

                if (!AddBetSlip(markets))
                {
                    return PROCESS_RESULT.SUSPENDED;
                    
                }

                if (!OddCheck(markets))
                {
                    return PROCESS_RESULT.MOVED;
                }
                if (FinalBet(markets, stakeStr))
                {
                    return PROCESS_RESULT.PLACE_SUCCESS;                    
                }
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus(string.Format("** PLACE BET Exception {0}", e));
            }
            LogMng.Instance.onWriteStatus(string.Format("** PLACE BET FAIL"));
            return PROCESS_RESULT.ERROR;
        }
        public void PlaceBet(List<CapsuledBetburgerInfo> info)
        {
            info[0].result = PlaceBet(ref info[0].betburgerInfo);
        }
        public double getBalance()
        {
            double balance = -1;
            int retryCount = 2;

            
            while (--retryCount >= 0)
            {
                try
                {
                    string result = Global.GetStatusValue("return localStorage.balance;").Replace("\\\"", "\"").Replace("\"{", "{").Replace("}\"", "}");
                    dynamic json = JsonConvert.DeserializeObject<dynamic>(result);
                    balance = Utils.ParseToDouble(json.totalBalance.ToString());
                    break;
                }
                catch (Exception e)
                {

                }
            }
            return balance;
        }

        //public bool GetApiKey()
        //{
        //    int nRetry = 2;
        //    while (nRetry-- >= 0)
        //    {
        //        try
        //        {

        //            string market = "CO";

        //            string unibetTicket = "";
        //            unibetTicket = RunScript("localStorage.kambiToken").Replace("\"", "");


        //            string profile = RunScript("localStorage.profile");
        //            dynamic json = JsonConvert.DeserializeObject<dynamic>(profile);
        //            string punterId = json.accountcode.ToString();

        //            //string temp = "{\"sessionAttributes\":{\"fingerprintHash\":\"" + GetFingerPrintHash() + "\"},\"punterId\":\"" + punterId + "\",\"requestStreaming\":true,\"customerSiteIdentifier\":\"\",\"channel\":\"WEB\",\"ticket\":\"" + unibetTicket + "\",\"market\":\"" + market + "\"}";
        //            string temp = "{\"sessionAttributes\":{},\"punterId\":\"" + punterId + "\",\"requestStreaming\":true,\"customerSiteIdentifier\":\"\",\"channel\":\"WEB\",\"ticket\":\"" + unibetTicket + "\",\"market\":\"" + market + "\"}";
        //            StringContent jsonContent = new StringContent(temp, Encoding.UTF8, "application/json");
        //            string loginApiLink = "https://mt-auth-api.kambicdn.com/player/api/v2019/betplay/punter/login.json?market=CO&lang=es_ES&channel_id=1&client_id=2&settings=true";


        //            HttpResponseMessage apiKeyResponse = m_client.PostAsync(loginApiLink, jsonContent).Result;
        //            apiKeyResponse.EnsureSuccessStatusCode();
        //            string apikeyStr = apiKeyResponse.Content.ReadAsStringAsync().Result;
        //            unibetSessionInfo = JsonConvert.DeserializeObject<UnibetSessionInfo>(apikeyStr);
        //            break;
        //        }
        //        catch (Exception e)
        //        {
        //            if (!login())
        //                return false;
        //        }
        //    }
        //    return true;
        //}

        private bool AddBetSlip(List<OpenBet_Unibet> markets)
        {
            long tick = Utils.getTick();

            string marketParam = string.Join("&id=", markets.Select(o => o.marketId));
            string outcomeUrl = $"https://na-offering-api.kambicdn.net/offering/v2018/betplay/betoffer/outcome.json?lang=es_ES&market=CO&client_id=2&channel_id=1&ncid={tick}&id={marketParam}";

            HttpResponseMessage outcomeResp = m_client.GetAsync(outcomeUrl).Result;
            string resultOutcomeStr = outcomeResp.Content.ReadAsStringAsync().Result;
            if (resultOutcomeStr.Contains("No bet offers found"))
            {
                return false;
            }
            dynamic jsonOutcomeResp = JsonConvert.DeserializeObject<dynamic>(resultOutcomeStr);

            for (int i = 0; i < jsonOutcomeResp.betOffers.Count; i++)
            {
                dynamic betoffer = jsonOutcomeResp.betOffers[i];
                markets[i].eventId = betoffer["eventId"].ToString();
            }
            return true;
        }
        private bool OddCheck(List<OpenBet_Unibet> markets)
        {
            m_client.DefaultRequestHeaders.Clear();
            m_client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/javascript, */*; q=0.01");
            m_client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            m_client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
            m_client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", string.Format("https://{0}/", domain));
            //m_client.DefaultRequestHeaders.TryAddWithoutValidation("Host", "mt-auth-api.kambicdn.com");

            m_client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", string.Format("Bearer {0}", unibetSessionInfo.token));

            string validateUrl = $"https://cf-mt-auth-api.kambicdn.com/player/api/v2019/betplay/coupon/validate.json?lang=es_ES&market=CO&client_id=2&channel_id=1";
            long tick = Utils.getTick();
            

            string strContent = GetOddCheckJsonContent(markets);

            StringContent jsonContent = new StringContent(strContent, Encoding.UTF8, "application/json");
            HttpResponseMessage resultBetting = m_client.PostAsync(validateUrl, jsonContent).Result;
            string resultBettingStr = resultBetting.Content.ReadAsStringAsync().Result;
            int oddTry = 0;
            while (++oddTry < 6 && resultBettingStr.Contains("ODDS_CHANGED"))
            {
                Thread.Sleep(500);
                if (!ChangeMarketOdds(resultBettingStr, markets))
                    return false;
                strContent = GetOddCheckJsonContent(markets);
                jsonContent = new StringContent(strContent, Encoding.UTF8, "application/json");
                resultBetting = m_client.PostAsync(validateUrl, jsonContent).Result;
                resultBettingStr = resultBetting.Content.ReadAsStringAsync().Result;
            }
            if (!resultBettingStr.Contains("SUCCESS"))
            {
                Trace.WriteLine("New Issue for validate - " + resultBettingStr);
                return false;
            }
            return true;
        }
        private bool FinalBet(List<OpenBet_Unibet> markets, string stakeStr)
        {
            int retryCnt = 0;
            while (retryCnt < 2)
            {
                string bettingUrl = "https://cf-mt-auth-api.kambicdn.com/player/api/v2019/betplay/coupon.json?lang=es_ES&market=CO&client_id=2&channel_id=1";
                
                string strContent = GetCouponJsonContent(markets, stakeStr);

                HttpContent jsonContent = new StringContent(strContent, Encoding.UTF8, "application/json");
                HttpResponseMessage resultBetting = m_client.PostAsync(bettingUrl, jsonContent).Result;
                string resultBettingStr = resultBetting.Content.ReadAsStringAsync().Result;
                BetResultJson resultJson = JsonConvert.DeserializeObject<BetResultJson>(resultBettingStr);

                if (resultBettingStr.Contains("SUCCESS") || resultBettingStr.Contains("LIVE_DELAY_PENDING"))
                {
                    if (resultBettingStr.Contains("LIVE_DELAY_PENDING"))
                    {
                        int delayTime = Utils.parseToInt(Regex.Match(resultBettingStr, @"delayBeforeAcceptingBet[^:]*:(?<VAL>[^,]*)").Groups["VAL"].Value);
                        string refId = Regex.Match(resultBettingStr, @"couponRef[^:]*:(?<VAL>[^,]*)").Groups["VAL"].Value;
                        int waitTime = delayTime > 0 ? delayTime : 9;
                        Thread.Sleep(waitTime * 1000);
                        bettingUrl = string.Format("https://cf-mt-auth-api.kambicdn.com/player/api/v2019/betplay/coupon/history/{0}.json?lang=es_ES&market=CO&client_id=2&channel_id=1&ncid={1}&liveDelayPoll=true", refId, Utils.getTick());
                        
                        resultBetting = m_client.GetAsync(bettingUrl).Result;

                    }

                    return true;
                }

                if (resultJson.betErrors != null)
                {
                    string type = resultJson.betErrors[0].items[0].type;
                    if (type == "STAKE_TOO_HIGH")
                    {
                        stakeStr = resultJson.betErrors[0].items[0].arguments[0].value;
                        retryCnt++;
                        continue;
                    }
                    else if (resultBettingStr.Contains("ODDS_CHANGED"))
                    {
                        Trace.WriteLine($"Odd Changed1 - {resultBettingStr}");
                        if (!ChangeMarketOdds(resultBettingStr, markets))
                            return false;
                        retryCnt++;
                        continue;
                    }
                    else
                    {
                        LogMng.Instance.onWriteStatus(string.Format("FinalBet Failed ({0})", resultJson.betErrors[0].items[0].type));

                        break;
                    }
                }
                else if (resultBettingStr.Contains("NOT_ENOUGH_FUNDS"))
                {
                    LogMng.Instance.onWriteStatus(string.Format("FinalBet Failed (NOT_ENOUGH_FUNDS)"));

                    break;
                }
            }
            return false;
        }
        public string GetOddCheckJsonContent(List<OpenBet_Unibet> markets)
        {
            string ret = "";
            try
            {
                if (markets == null || markets.Count == 0)
                    return ret;

                string strRows = string.Join(",", markets.Select((o, i) => $@"{{""index"":{i},""odds"":{o.odds},""outcomeId"":{o.marketId},""type"":""SIMPLE""}}"));

                ret = $@"{{""couponRows"":[{strRows}],""bets"":[{{""couponRowIndexes"":[{string.Join(",", Enumerable.Range(0, markets.Count))}],""eachWay"":false}}],""isUserLoggedIn"":true}}";
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return ret;
        }
        public string GetCouponJsonContent(List<OpenBet_Unibet> markets, string stake)
        {
            string ret = "";
            try
            {
                if (markets == null || markets.Count == 0)
                    return ret;

                string strRows = string.Join(",", markets.Select((o, i) => $@"{{""index"":{i},""odds"":{o.odds},""outcomeId"":{o.marketId},""type"":""SIMPLE""}}"));
                string strBets = $@"{{""couponRowIndexes"":[{string.Join(",", Enumerable.Range(0, markets.Count))}],""eachWay"":false,""stake"":{stake}}}";
                string strOutcomes = string.Join(",", markets.Select(o => $@"{{""id"":{o.marketId},""outcomeId"":{o.marketId},""betofferId"":{o.offerId},""eventId"":{o.eventId},""approvedOdds"":{o.odds},""isLiveBetoffer"":false,""isPrematchBetoffer"":true,""fromBetBuilder"":false,""oddsApproved"":true,""eachWayApproved"":true,""source"":""Widget"",""betslipLocationSource"":""Home"",""isGameParlayOutcome"":false,""fromPrePack"":false}}"));
                ret = $@"{{""couponRows"":[{strRows}],""bets"":[{strBets}],""allowOddsChange"":""NO"",""channel"":""WEB"",""trackingData"":{{""hasTeaser"":false,""isBetBuilderCombination"":false,""selectedOutcomes"":[{strOutcomes}],""reward"":{{}},""isMultiBuilder"":false,""isPrePackCombination"":false}},""requestId"":""{Guid.NewGuid().ToString().ToLower()}""}}";
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return ret;
        }
        public bool ChangeMarketOdds(string resultBettingStr, List<OpenBet_Unibet> markets)
        {
            try
            {
                dynamic retObj = JsonConvert.DeserializeObject<dynamic>(resultBettingStr);
                if (retObj.status == "FAIL")
                {
                    if (retObj.couponRowErrors != null && retObj.couponRowErrors.Count > 0)
                    {
                        foreach (dynamic rowErr in retObj.couponRowErrors)
                        {
                            int index = Utils.parseToInt(rowErr.couponRowIndex.ToString());
                            dynamic detail = rowErr.errors;
                            if (detail != null || detail.Count > 0)
                            {
                                foreach (dynamic errOne in detail)
                                {
                                    string errorType = errOne.type;
                                    if (errorType == "ODDS_CHANGED" && errOne.arguments != null && errOne.arguments.Count > 0)
                                    {
                                        string changeOdd = errOne.arguments[0].value;
                                        if (!string.IsNullOrEmpty(changeOdd))
                                        {
                                            double origVal = Utils.ParseToDouble(markets[index].odds);
                                            double newVal = Utils.ParseToDouble(changeOdd);
                                            if (origVal > newVal)
                                            {
                                                LogMng.Instance.onWriteStatus($"Odd moved down {markets[index].odds}-{changeOdd}");

                                                bool bAcceptable = false;
                                                if (1000 <= origVal && origVal < 2000)
                                                {//- If the odds are between 1.0 and 2.0 then 0.01 lower odds is acceptable.
                                                    if (origVal - newVal <= 10)
                                                        bAcceptable = true;
                                                }
                                                else if (2000 <= origVal && origVal < 3000)
                                                {//- If the odds are between 2.0 and 3.0 then 0.02 lower odds is acceptable.
                                                    if (origVal - newVal <= 20)
                                                        bAcceptable = true;
                                                }
                                                else if (3000 <= origVal && origVal < 4000)
                                                {//- If the odds are between 3.0 and 4.0 then 0.05 lower odds is acceptable.
                                                    if (origVal - newVal <= 50)
                                                        bAcceptable = true;
                                                }

                                                if (!bAcceptable)
                                                {
                                                    return false;
                                                }
                                                else
                                                {
                                                    LogMng.Instance.onWriteStatus($"Odd down {origVal - newVal} is acceptable");
                                                }

                                            }

                                            LogMng.Instance.onWriteStatus($"Odd is Changed {markets[index].odds}-{changeOdd}");
                                            markets[index].odds = changeOdd;
                                        }
                                        //Trace.WriteLine($"Odds Changed - index-{index}, odd-{changeOdd}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return true;
        }


    }
#endif
}