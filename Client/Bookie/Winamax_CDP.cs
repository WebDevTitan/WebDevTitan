namespace Project.Bookie
{
#if (WINAMAX)
    class Winamax_CDP : IBookieController
    {
        
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

        public Winamax_CDP()
        {
            domain = "winamax.es";
            if (!domain.StartsWith("www."))
                domain = "www." + domain;

            Global.placeBetHeaderCollection.Clear();
            m_client = initHttpClient();
            if (CDPController.Instance._browserObj == null)
                CDPController.Instance.InitializeBrowser($"https://{domain}/");

            //Global.SetMonitorVisible(false);
        }

        public bool Pulse()
        {
            if (getBalance() < 0)
                return true;
            else
                Logout();

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
            try
            {
                domain = "winamax.es";
                if (!domain.StartsWith("www."))
                    domain = "www." + domain;

                Global.cookieContainer = CDPController.Instance.GetCoookies().Result;
                if (!string.IsNullOrEmpty(Global.GTM))
                    m_client = initHttpClient(false);
                else
                {
                    Global.GTM = string.Empty;
                    m_client = initHttpClient(true);
                }
                if (getBalance() > 0)
                    return true;

                bLogin = CDPController.Instance.DoUILogin(Setting.Instance.username, Setting.Instance.password, Setting.Instance.birthday).Result;
                Thread.Sleep(5000);
                CDPController.Instance.ExecuteScript("document.getElementById('submitevent').click()");
                //Global.SetMonitorVisible(false);
                if (bLogin)
                {
                    Global.cookieContainer = CDPController.Instance.GetCoookies().Result;
                    m_client = initHttpClient(false);
                }
            }
            catch(Exception e)
            {
                LogMng.Instance.onWriteStatus("[Winamax]-login " + e.ToString());
            }
   

            return bLogin;
        }

        public void ClickRemoveButton()
        {
            try
            {
                CDPController.Instance.ExecuteScript("document.querySelector('svg[class=sc-dVUfCs kJWggx]').click()");

                /*bool isFound = CDPController.Instance.FindAndClickElement(1, "svg[class='sc-dVUfCs kJWggx']").Result;
                if (isFound)
                {
                    LogMng.Instance.onWriteStatus("****Clicked Remove Button****");
                }*/
            }
            catch { }
        }
        public void Logout()
        {
            try
            {
                LogMng.Instance.onWriteStatus("***LOGOUT WINAMAX***");
                CDPController.Instance.NavigateInvoke("https://www.winamax.es/?LOGOUT");
                Global.GTM = string.Empty;
            }
            catch { }
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

            if (string.IsNullOrEmpty(info.direct_link))
                return PROCESS_RESULT.SUSPENDED;
            
            //Go to Event Url
            CDPController.Instance.ExecuteScript("document.getElementById('submitevent').click()");
            CDPController.Instance.NavigateInvoke(info.eventUrl);
            Thread.Sleep(5000);

            while (nRetry++ < 4)
            {
                //LogMng.Instance.onWriteStatus("***Remove all bets***");
                //ClickRemoveButton();

                //Expand all markets
                LogMng.Instance.onWriteStatus("***Enable all markets***");
                string script1 = Properties.Resources.winamax_script1;
                CDPController.Instance.ExecuteScript(script1);
                Thread.Sleep(800);

                LogMng.Instance.onWriteStatus("***Expand all markets***");
                string script2 = Properties.Resources.winamax_scrip2;
                CDPController.Instance.ExecuteScript(script2);
                Thread.Sleep(800);


                /*bool isFound = CDPController.Instance.FindElement(1, $"div[data-testid=odd-button-{info.direct_link}]").Result;
                if(!isFound)
                {
                    LogMng.Instance.onWriteStatus("Not Found Odds..");
                    Thread.Sleep(3000);
                    continue; 
                }*/

                /*isFound = CDPController.Instance.ScrollInToView($"div[data-testid=odd-button-{info.direct_link}]").Result;
                Thread.Sleep(3000);
                
                //Add To Betslip
                isFound = CDPController.Instance.FindAndClickElement(1, $"div[data-testid=odd-button-{info.direct_link}]").Result;
                Thread.Sleep(700);

                //Add Stake
                isFound = CDPController.Instance.FindAndClickElement(1, "input[type=text]" , 3).Result;
                Thread.Sleep(600);

                CDPMouseController.Instance.InputText(info.stake.ToString());
                Thread.Sleep(3000);

                //Click Placebet Button*/
                CDPController.Instance.PlaceBetRespBody = string.Empty;
                //CDPController.Instance.ExecuteScript("document.querySelector('button[data-testid = basket-submit-button]').click()");
                //isFound = CDPController.Instance.FindAndClickElement(1, "button[data-testid='basket-submit-button']" , 3).Result;
                //string formDataString = $"bsm={{'T':1,'A':{stake},'B':[{{'BID':{info.direct_link},'O':{odd}}}],'CV':'2.104.0-desktop','locale':'es'}}";
                string formDataString = $"bsm={{\\\"T\\\":1,\\\"A\\\":{stake},\\\"B\\\":[{{\\\"BID\\\":{info.direct_link},\\\"O\\\":{odd}}}],\\\"CV\\\":\\\"2.108.2-desktop\\\",\\\"locale\\\":\\\"es\\\"}}";
                string getEventsMarketURL = $"https://www.winamax.es/betting/validate_betslip.php";

                string placebet_script = Properties.Resources.winamax_placebet_query;
                placebet_script = placebet_script.Replace("[x-token]", Global.GTM);
                placebet_script = placebet_script.Replace("[body]", formDataString);

                CDPController.Instance.ExecuteScript(placebet_script);
                int retryCnt = 0;
                while (string.IsNullOrEmpty(CDPController.Instance.PlaceBetRespBody))
                {
                    retryCnt++;
                    if (retryCnt > 30)
                        break;

                    Thread.Sleep(500);
                }
                //m_client.DefaultRequestHeaders.Remove("X-Token");
                //m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-Token", Global.GTM);

                /*HttpResponseMessage respMessage = m_client.PostAsync(getEventsMarketURL, (HttpContent)new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)new KeyValuePair<string, string>[1]{
                        new KeyValuePair<string, string>("bsm", formDataString)
                })).Result;

                string respBody = respMessage.Content.ReadAsStringAsync().Result;*/
                LogMng.Instance.onWriteStatus($"validate_betslip Req: {formDataString}");
                LogMng.Instance.onWriteStatus($"validate_betslip Res: {CDPController.Instance.PlaceBetRespBody}");

                JObject placebet_Response = JObject.Parse(CDPController.Instance.PlaceBetRespBody);
                if (placebet_Response["BSID"] != null && !string.IsNullOrEmpty(placebet_Response["BSID"].ToString()) && placebet_Response["ESTR"] != null && placebet_Response["ESTR"].ToString() == "OK")
                {
                    return PROCESS_RESULT.PLACE_SUCCESS;
                }
                else if(CDPController.Instance.PlaceBetRespBody.Contains("Odds changed"))
                {
                    odd = (int) placebet_Response["EDETAILS"][info.direct_link]["EINFO"]["O"];
                    LogMng.Instance.onWriteStatus($"Odds is changed . New Odds : {odd.ToString()}");
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
                /*string balance_script = Properties.Resources.winamax_balance_query;
                balance_script = balance_script.Replace("[balance_url]", "https://www.winamax.es/account/dashboard.php");

                CDPController.Instance.balanceRespBody = string.Empty;

                CDPController.Instance.ExecuteScript(balance_script);
                int retryCnt = 0;
                while (string.IsNullOrEmpty(CDPController.Instance.balanceRespBody))
                {
                    retryCnt++;
                    if (retryCnt > 30)
                        break;

                    Thread.Sleep(500);
                }
                //m_client.DefaultRequestHeaders.Remove("X-Token");

                //HttpResponseMessage respMessage = m_client.GetAsync("https://www.winamax.es/account/dashboard.php").Result;
                //respMessage.EnsureSuccessStatusCode();

                //string respBody = respMessage.Content.ReadAsStringAsync().Result;
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(CDPController.Instance.balanceRespBody);

                HtmlNode money_block = doc.DocumentNode.Descendants("div").FirstOrDefault(n => n.Id != null && n.Id == "money-block");
                if (money_block == null)
                    return balance;

                HtmlNode label = money_block.Descendants("div").FirstOrDefault(n => n.Attributes["class"] != null && n.Attributes["class"].Value == "value");
                if (label == null)
                    return balance;*/

                string balance_str = CDPController.Instance.ExecuteScript("document.getElementById('money-block').innerText" , true);

                string balance_text = WebUtility.HtmlDecode(balance_str.Trim());
                if (string.IsNullOrEmpty(balance_text))
                    return balance;

                LogMng.Instance.onWriteStatus(balance_text);
                balance = Utils.ParseToDouble(balance_text.Replace("\n" , string.Empty).Replace("SALDO" , string.Empty).Replace("€", string.Empty).Trim());
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
