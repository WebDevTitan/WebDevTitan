namespace Project.Bookie
{
#if (EUROBET)
    class EurobetCtrl : IBookieController
    {
        public class PlacebetJson
        {
            public string action { get; set; } = "WagerService";
            public Bet bet { get; set; } = new Bet();

            public object freeBetToken = null;
        }

        public class Bet
        {
            public long timestampClient { get; set; } = Utils.getTick();
            public string clientTransactionId { get; set; } = "0";
            public long accountId { get; set; }
            public int stake { get; set; }
            public int acceptChangedOdd { get; set; } = 0;
            public int acceptOddToValue { get; set; } = 0;
            public int winAmount { get; set; }

            public bool reserved { get; set; } = false;

            public List<BetData> betDataList { get; set; } = new List<BetData>();

        }

        public class BetData
        {
            public int programCode { get; set; }
            public int eventCode { get; set; }
            public long eventDate { get; set; }
            public int betCode { get; set; }

            public List<int> resultCode { get; set; } = new List<int>();

            public int oddValue { get; set; }
            public List<int> additionalInfo { get; set; } = new List<int>();

        }
        public class LiveEventJson
        {
            public string description { get; set; }
            public Result result { get; set; }

        }

        public class Result
        {
            public int itemCount { get; set; }
            public List<OuterItemListItr> itemList { get; set; }
        }
        public class OuterItemListItr
        {
            public string discipline { get; set; }
            public int disciplineCode { get; set; }
            public List<InnerItemListItr> itemList { get; set; }

        }
        public class InnerItemListItr
        {
            public List<betGroup> betGroupList { get; set; } = new List<betGroup>();
            public EventInfo eventInfo { get; set; }
            public BreadCrumbInfo breadCrumbInfo { get; set; }
        }

        public class DetailedLiveEventJson
        {
            public string description { get; set; }
            public InnerItemListItr result { get; set; }

        }

        public class BreadCrumbInfo
        {
            public string fullUrl { get; set; }
        }
        public class EventInfo
        {
            public long eventCode { get; set; }
            public long programCode { get; set; }
            public long eventData { get; set; }
            public string aliasUrl { get; set; }
        }

        public class betGroup
        {
            public int betId { get; set; }
            public int layoutType { get; set; }
            public string betDescription { get; set; }

            public List<oddGroup> oddGroupList { get; set; } = new List<oddGroup>();

        }

        public class oddGroup
        {
            public string oddGroupDescription { get; set; }
            public string additionalDescription { get; set; }
            public string alternativeDescription { get; set; }
            public List<oddInfo> oddList { get; set; }
        }

        public class oddInfo
        {
            public int betCode { get; set; }
            public int oddValue { get; set; }
            public string oddDescription { get; set; }
            public int resultCode { get; set; }
            public string boxTitle { get; set; }
            public List<int> additionalInfo { get; set; } = new List<int>();
            public bool addInfo { get; set; }
            public bool multiBet { get; set; }
        }

        

        public HttpClient m_client = null;
        public string accountId = "";
        public string auth_token = "";
        public EurobetCtrl()
        {
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
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.142 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            if (!string.IsNullOrEmpty(auth_token))
                httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("X-EB-Auth-Token", auth_token);

            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("X-EB-Accept-Language", "IT");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("X-EB-MarketId", "5");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("X-EB-PlatformId", "1");

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

            //
            bool bLogin = false;

            int nRetry = 0;
            while (nRetry++ < 3)
            {
                try
                {
                    Global.RemoveCookies();
                    Global.OpenUrl($"https://www.eurobet.it/it/");


                    string result = Global.GetStatusValue("return localStorage.Eurobet;").Replace("\\\"", "\"").Replace("\"{", "{").Replace("}\"", "}").Replace("\\\\", "").ToLower();
                    if (!result.Contains("\"islogged\":true"))
                    {
                        int nRetry1 = 0;
                        while (nRetry1 < 10)
                        {
                            Thread.Sleep(500);
                            string htmlresult = Global.GetStatusValue("return document.getElementsByClassName('header__login--desktop')[0].outerHTML;");

#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"input username status : {htmlresult}");
#endif

                            if (htmlresult.Contains("login"))
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
                            //Global.SetMonitorVisible(false);
                            LogMng.Instance.onWriteStatus($"Login Failed because of can't open login page");
                            continue;
                        }
                        Thread.Sleep(2000);
                        Global.RunScriptCode("document.getElementsByClassName('header__login--desktop')[0].click();");
                        Thread.Sleep(2000);

                        Rect monitorRect = Global.GetMonitorPos();
                        double top = monitorRect.Top;
                        double left = monitorRect.Left;
                        SetForegroundWindow(Global.ViewerHwnd);

#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"window left: {left} top: {top}");
#endif
                                                
                        Thread.Sleep(1000);
                        double x, y;
                        string posResult = Global.GetStatusValue("return JSON.stringify(document.getElementsByName('username')[0].getBoundingClientRect());");
                        Rect iconRect = Utils.ParseRect(posResult);
                        x = left + iconRect.X + iconRect.Width / 2;
                        y = top + iconRect.Y + iconRect.Height / 2;


#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Click Pos (username): {x} {y}");
#endif

                        ClickOnPoint(Global.ViewerHwnd, new Point(x, y));

                        SendKeys.SendWait(Setting.Instance.username);
                                                
                        Thread.Sleep(1000);
                        posResult = Global.GetStatusValue("return JSON.stringify(document.getElementsByName('password')[0].getBoundingClientRect());");
                        iconRect = Utils.ParseRect(posResult);
                        x = left + iconRect.X + iconRect.Width / 2;
                        y = top + iconRect.Y + iconRect.Height / 2;


#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Click Pos (password): {x} {y}");
#endif

                        ClickOnPoint(Global.ViewerHwnd, new Point(x, y));

                        //Global.RunScriptCode($"document.getElementById('password').value='{Setting.Instance.password}';");
                        SendKeys.SendWait(Setting.Instance.password);
                        Thread.Sleep(1000);

                        //moveThread.Abort();


                        posResult = Global.GetStatusValue("return JSON.stringify(document.getElementsByName('submit')[0].getBoundingClientRect());");
                        iconRect = Utils.ParseRect(posResult);
                        x = left + iconRect.X + iconRect.Width / 2;
                        y = top + iconRect.Y + iconRect.Height / 2;

#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Click Pos (login button): {x} {y}");
#endif


                        Thread.Sleep(2000);
                        //SetCursorPos((int)x, (int)y);
                        ClickOnPoint(Global.ViewerHwnd, new Point(x, y));

                        //string functionString = $"function setNativeValue(element, value) {{ const valueSetter = Object.getOwnPropertyDescriptor(element, 'value').set; const prototype = Object.getPrototypeOf(element); const prototypeValueSetter = Object.getOwnPropertyDescriptor(prototype, 'value').set; if (valueSetter && valueSetter !== prototypeValueSetter) {{ prototypeValueSetter.call(element, value); }} else {{ valueSetter.call(element, value); }} }} var inputid = document.getElementsByName('username')[0]; setNativeValue(inputid, '*idvalue*'); inputid.dispatchEvent(new Event('input', {{ bubbles: true }})); var inputpwd = document.getElementsByName('password')[0]; setNativeValue(inputpwd, '*pwdvalue*'); inputpwd.dispatchEvent(new Event('input', {{ bubbles: true }}));";
                        //functionString = functionString.Replace("*idvalue*", Setting.Instance.username);
                        //functionString = functionString.Replace("*pwdvalue*", Setting.Instance.password);
                        //Global.RunScriptCode(functionString);
                        //Thread.Sleep(2000);
                        //Global.RunScriptCode("document.getElementsByClassName('btnsecond')[0].click();");


                        nRetry1 = 0;
                        while (nRetry1++ < 5)
                        {
                            Thread.Sleep(500);

                            result = Global.GetStatusValue("return localStorage.Eurobet;").Replace("\\\"", "\"").Replace("\"{", "{").Replace("}\"", "}").Replace("\\\\", "").ToLower();
                            if (result.Contains("\"islogged\":true"))
                            {
                                bLogin = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        bLogin = true;
                    }

                    if (bLogin)
                    {
                        var userdata = JsonConvert.DeserializeObject<dynamic>(result);
                        accountId = userdata.userdata.autenticatedprops.accountid.ToString();
                        auth_token = userdata.userdata.autenticatedprops.sessionid.ToString();
                        Task.Run(async () => await Global.GetCookie("https://www.eurobet.it")).Wait();
                        break;
                    }
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
            OpenBet_Eurobet openbet = Utils.ConvertBetburgerPick2OpenBet_Eurobet(info);

            if (openbet == null)
            {
                LogMng.Instance.onWriteStatus("Converting OpenBet Error");
                return PROCESS_RESULT.ERROR;
            }

            int retryCount = 2;
#if OXYLABS
            retryCount = 5;
#endif
                    

            while (--retryCount >= 0)
            {                
                try
                {


                    string liveLink = $"https://www.eurobet.it/live-detail-service/sport-schedule/services/event/{info.siteUrl}/?prematch=0&live=1";
                    HttpResponseMessage eventResponseMessage = m_client.GetAsync(liveLink).Result;
                    eventResponseMessage.EnsureSuccessStatusCode();

                    oddInfo selectInfo = null;
                    

                    string eventContent = eventResponseMessage.Content.ReadAsStringAsync().Result;
                    DetailedLiveEventJson detailedEventJson = JsonConvert.DeserializeObject<DetailedLiveEventJson>(eventContent);

                    
                    
                    bool found = false;                    
                    foreach (var betGroupItr in detailedEventJson.result.betGroupList)
                    {
                        if (found)
                            break;
                        foreach (var oddOne in betGroupItr.oddGroupList)
                        {
                            if (found)
                                break;
                            foreach (oddInfo odd in oddOne.oddList)
                            {
                                if (odd.betCode == openbet.betCode && odd.resultCode == openbet.resultCode)
                                {                                    
                                    selectInfo = odd;
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (selectInfo == null)
                    {
                        LogMng.Instance.onWriteStatus("This line is changed or not existed now...");
                        return PROCESS_RESULT.MOVED;
                    }

                    //if (info.odds != (double)selectInfo.oddValue / 100)
                    //{
                    //    LogMng.Instance.onWriteStatus(string.Format("Odd is changed from {0} To {1}", info.odds, ((double)selectInfo.oddValue / 100).ToString("N2")));
                    //    return PROCESS_RESULT.MOVED;
                    //}

                    if (Setting.Instance.bAllowOddDrop)
                    {
                        if ((double)selectInfo.oddValue / 100 < info.odds)
                        {
                            if ((double)selectInfo.oddValue / 100 < info.odds - info.odds / 100 * Setting.Instance.dAllowOddDropPercent)
                            {
                                LogMng.Instance.onWriteStatus($"Ignore bet because of odd is dropped larger than {Setting.Instance.dAllowOddDropPercent}%: {info.odds} -> {((double)selectInfo.oddValue / 100).ToString("N2")}");
                                return PROCESS_RESULT.MOVED;
                            }
                        }
                    }
                    

                    info.odds = (double)selectInfo.oddValue / 100;
                    if (info.odds > Setting.Instance.maxOddsSports || info.odds < Setting.Instance.minOddsSports)
                    {
                        LogMng.Instance.onWriteStatus(string.Format("Odd is out of range, ignore {0}", info.odds));
                        return PROCESS_RESULT.MOVED;
                    }

                    PlacebetJson placebetJson = new PlacebetJson();
                    placebetJson.bet.acceptChangedOdd = 0;
                    placebetJson.bet.acceptOddToValue = 0;
                    placebetJson.bet.stake = (int)info.stake * 100;
                    placebetJson.bet.winAmount = (int)(info.stake * info.odds * 100);
                    placebetJson.bet.accountId = Utils.parseToInt(accountId);

                    BetData betData = new BetData();
                    betData.eventCode = openbet.eventCode;
                    betData.programCode = openbet.programCode;
                    betData.eventDate = detailedEventJson.result.eventInfo.eventData;
                    betData.betCode = selectInfo.betCode;
                    betData.oddValue = selectInfo.oddValue;
                    betData.resultCode.Add(selectInfo.resultCode);
                    betData.additionalInfo = selectInfo.additionalInfo;
                    placebetJson.bet.betDataList.Add(betData);

                    string placebetPostContent = JsonConvert.SerializeObject(placebetJson);

                    int subRetryCount = 6;
                    string strBetResp = string.Empty;

                    //while (--subRetryCount > 0)
                    //{
                    //    try
                    //    {
                    //        Global.strRequestUrl = "https://www.eurobet.it/sport-sale-service/internal-services/bet";
                    //        string functionString = $"window.fetch('{Global.strRequestUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{placebetPostContent}', method: 'POST', x-eb-marketid: 5, x-eb-platformid: 1, x-eb-auth-token: {auth_token}}}).then(response => response.json());";

                    //        Global.strPlaceBetResult = "";
                    //        Global.waitResponseEvent.Reset();

                    //        Global.RunScriptCode(functionString);

                    //        if (!Global.waitResponseEvent.Wait(20000) || string.IsNullOrEmpty(Global.strPlaceBetResult))
                    //        {
                    //            Global.strRequestUrl = "";
                    //            continue;
                    //        }
                    //        Global.strRequestUrl = "";
                    //        strBetResp = Global.strPlaceBetResult;

                    //        break;
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        LogMng.Instance.onWriteStatus("place exception:" + ex);
                    //    }
                    //}

                    m_client.DefaultRequestHeaders.Remove("X-EB-Username");
                    m_client.DefaultRequestHeaders.Remove("X-EB-Password");
                    m_client.DefaultRequestHeaders.Remove("X-EB-Auth-Token");

                    m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-EB-Auth-Token", auth_token);

                    HttpResponseMessage balanceResponseMessage = m_client.PostAsync("https://www.eurobet.it/sport-sale-service/internal-services/bet", new StringContent(placebetPostContent, Encoding.UTF8, "application/json")).Result;
                    balanceResponseMessage.EnsureSuccessStatusCode();

                    strBetResp = balanceResponseMessage.Content.ReadAsStringAsync().Result;
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Placebet Result: {strBetResp}");
#endif
                    JObject resultObj = JObject.Parse(strBetResp);
                    if (resultObj["description"].ToString().Contains("Success"))
                    {
                        return PROCESS_RESULT.PLACE_SUCCESS;
                    }
                    else if (resultObj["description"].ToString().Contains("Sessione non valida"))
                    {
                        return PROCESS_RESULT.NO_LOGIN;
                    }
                    else
                    {
                        LogMng.Instance.onWriteStatus(string.Format("Place Bet Error {0}", resultObj["description"].ToString()));

                        if (resultObj["description"].ToString().Contains("Giocata bloccata") ||
                            resultObj["description"].ToString().Contains("Scommessa Rifiutata") ||
                            resultObj["description"].ToString().Contains("Raggiunto Limite Ripetizioni Biglietto"))
                        {
                            return PROCESS_RESULT.CRITICAL_SITUATION;
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
                    m_client.DefaultRequestHeaders.Remove("X-EB-Username");
                    m_client.DefaultRequestHeaders.Remove("X-EB-Password");
                    m_client.DefaultRequestHeaders.Remove("X-EB-Auth-Token");

                    m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-EB-Auth-Token", auth_token);

                    HttpResponseMessage balanceResponseMessage = m_client.GetAsync(string.Format("https://infoservice.eurobet.it/balance-service/account-info/balance/{0}", accountId)).Result;
                    balanceResponseMessage.EnsureSuccessStatusCode();

                    string balanceContent = balanceResponseMessage.Content.ReadAsStringAsync().Result;
                    JObject balanceObj = JObject.Parse(balanceContent);

                    balance = Utils.ParseToDouble(balanceObj["result"]["balance"].ToString()) / 100;
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

    }
#endif
}
