namespace Project.Bookie
{
#if (PINNACLE)

    public class PinnacleSelection
    {
        public long matchupId;
        public string marketKey;
        public string designation;
        public double price;
        public double points;
    }
    public class PlacebetItem
    {
        public string oddsFormat;
        public bool acceptBetterPrices;
        public string @class;
        public List<PinnacleSelection> selections;
        public double stake;
        public string originTag;
        public bool acceptBetterPrice;
    }
    class PinnacleCtrl : IBookieController
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
            
            return nResult;
        }
        public bool logout()
        {
            return true;
        }

        public PinnacleCtrl()
        {            
            m_client = initHttpClient();
            //Global.SetMonitorVisible(false);
        }

        public bool Pulse()
        {

            if (getBalance() < 0)
            {

                return false;
            }
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
            //Global.SetMonitorVisible(true);
            bool bLogin = false;
            while (nTotalRetry++ < 2)
            {
                try
                {

                    lock (lockerObj)
                    {
                        Global.RemoveCookies();
                        m_client = initHttpClient();



                        Global.OpenUrl($"https://www.pinnacle.com/es/account/login/");


                        string htmlResult = "";
                        int nWaitRetry = 0;
                        while (nWaitRetry++ < 10)
                        {
                            htmlResult = Global.RunScriptCode("document.querySelectorAll('div[data-test-id=\"Loginform-SubmitButton\"]')[0].outerHTML");

                            if (htmlResult.Contains("class"))
                            {
                                Thread.Sleep(500);
                                break;
                            }
                            Thread.Sleep(500);
                        }

                        if (!htmlResult.Contains("class"))
                        {
                            LogMng.Instance.onWriteStatus("Can't open pinnacle login page");
                            return false;
                        }

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
                        string posResult = Global.GetStatusValue("return JSON.stringify(document.getElementsByName('username')[0].getBoundingClientRect());");
                        Rect iconRect = Utils.ParseRect(posResult);
                        x = left + iconRect.X + iconRect.Width / 2;
                        y = top + iconRect.Y + iconRect.Height / 2;


#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Click Pos (username): {x} {y}");
#endif

                        ClickOnPoint(Global.ViewerHwnd, new Point(x, y));

                        SendKeys.SendWait(Setting.Instance.username);

                        //Global.RunScriptCode($"document.getElementById('userName').value='{Setting.Instance.username}';");

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


                        posResult = Global.GetStatusValue("return JSON.stringify(document.querySelectorAll('div[data-test-id=\"Loginform-SubmitButton\"]')[0].getBoundingClientRect());");
                        iconRect = Utils.ParseRect(posResult);
                        x = left + iconRect.X + iconRect.Width / 2;
                        y = top + iconRect.Y + iconRect.Height / 2;

#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Click Pos (login button): {x} {y}");
#endif


                        Global.strWebResponse1 = "";
                        Global.waitResponseEvent1.Reset();

                        Thread.Sleep(2000);
                        //SetCursorPos((int)x, (int)y);
                        ClickOnPoint(Global.ViewerHwnd, new Point(x, y));


                        if (Global.waitResponseEvent1.Wait(5000))
                        {
                            foreach (var item in Global.pinnacleHeaders)
                            {
                                m_client.DefaultRequestHeaders.Add(item.Key, item.Value);
                            }
                                
                            return true;
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

            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "Keep-Alive");


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

            try
            {
                int retryCount = 3;
                //processResponse.odds = 1;
                string strBody = GetBetSlipBody(info);
                string slipContent = AddbetSlip(strBody);
                //processResponse.slipContent = slipContent;
                if (string.IsNullOrEmpty(slipContent))
                {
                    LogMng.Instance.onWriteStatus($"Addbetslip no response");
                    return PROCESS_RESULT.ERROR;
                }

                JObject jsonSlipObj = JsonConvert.DeserializeObject<JObject>(slipContent);
                if (jsonSlipObj["detail"] != null)
                {
                    LogMng.Instance.onWriteStatus($"AddbetSlip error detail:{jsonSlipObj["detail"].ToString()}");
                    return PROCESS_RESULT.ERROR;
                }
                else
                {
                    if (jsonSlipObj["selections"] != null)
                    {
                        string newOdds = jsonSlipObj["selections"][0]["price"].ToString();
                        double d_newOdds = Utils.ConvertOddFromAmericaToDecimal(newOdds);

                        if (CheckOddDropCancelBet(d_newOdds, info))
                        {
                            LogMng.Instance.onWriteStatus($"Placing bet is cancelled, because of odd dropped({info.odds} -> {d_newOdds})");
                            return PROCESS_RESULT.SUSPENDED;
                        }
                    }

                }
                while (--retryCount > 0)
                {
                    if (string.IsNullOrEmpty(strBody))
                    {
                        LogMng.Instance.onWriteStatus($"BetSlip is Null");
                        return PROCESS_RESULT.ERROR;
                    }
                    double newOdds = 0;
                    string content = DoPlacebet(slipContent, newOdds, info);
                    if (string.IsNullOrEmpty(content))
                    {
                        LogMng.Instance.onWriteStatus($"BetResponse is Null");
                        return PROCESS_RESULT.ERROR;
                    }

                    //processResponse.couponData = content;
                    JObject placebetJson = JsonConvert.DeserializeObject<JObject>(content);
                    if (placebetJson["detail"] != null && placebetJson["detail"].ToString() == "Market has changed")
                    {
                        LogMng.Instance.onWriteStatus("Market is changed, try again now!");
                        newOdds = (double)placebetJson["newPrice"];
                        continue;
                    }
                    else if (placebetJson["price"] != null)
                    {
                        newOdds = (double)placebetJson["price"];
                    }
                    else
                    {
                        LogMng.Instance.onWriteStatus(placebetJson.ToString());
                    }


                    if (placebetJson["requestId"] != null)
                    {
                        return PROCESS_RESULT.PLACE_SUCCESS;
                    }
                    else
                    {
                        LogMng.Instance.onWriteStatus($"This bet is failed to place");
                        return PROCESS_RESULT.ERROR;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus("Exception during Placebet: " + ex.ToString());
            }

            return PROCESS_RESULT.ERROR;
        }

        public string AddbetSlip(string betslipReqBody)
        {
            string addbetResp = string.Empty;
            try
            {
                HttpResponseMessage respMsg = m_client.PostAsync("https://api.arcadia.pinnacle.com/0.1/bets/straight/quote", new StringContent(betslipReqBody, Encoding.UTF8, "application/json")).Result;

                addbetResp = respMsg.Content.ReadAsStringAsync().Result;
            }
            catch { }
            return addbetResp;
        }

        private string GetBetSlipBody(BetburgerInfo betitem)
        {

            string betslipbody = string.Empty;
            try
            {
                string marketKey = betitem.direct_link.Split('/')[0];
                string matchupId = betitem.direct_link.Split('/')[1];
                string price = betitem.direct_link.Split('/')[2];
                string designation = betitem.direct_link.Split('/')[3];
                if (betitem.direct_link == null)
                    betslipbody = $"{{\"oddsFormat\":\"american\",\"selections\":[{{\"matchupId\":{matchupId},\"marketKey\":\"{marketKey}\",\"designation\":\"{designation}\",\"price\":{price}}}]}}";
                else
                    betslipbody = $"{{\"oddsFormat\":\"american\",\"selections\":[{{\"matchupId\":{matchupId},\"marketKey\":\"{marketKey}\",\"designation\":\"{designation}\",\"price\":{price},\"points\": {betitem.direct_link}}}]}}";
            }
            catch
            {
                LogMng.Instance.onWriteStatus($"getbetslipbody exception outcome:{betitem.outcome}");
            }
            return betslipbody;
        }
        public string DoPlacebet(string addbetResp, double newOdds, BetburgerInfo info)
        {
            string placebetRespBody = string.Empty;
            try
            {
                JObject jbetslip = JObject.Parse(addbetResp);
                PinnacleSelection selection = new PinnacleSelection();
                selection.designation = jbetslip["selections"][0]["designation"].ToString();
                selection.marketKey = jbetslip["selections"][0]["marketKey"].ToString();
                selection.matchupId = (long)jbetslip["selections"][0]["matchupId"];
                if (jbetslip["selections"][0]["points"] != null)
                    selection.points = (double)jbetslip["selections"][0]["points"];

                if (newOdds > 0)
                {
                    selection.price = (int)newOdds;
                }
                else
                    selection.price = (int)jbetslip["selections"][0]["price"];

                PlacebetItem placebetItem = new PlacebetItem();
                placebetItem.selections.Add(selection);
                placebetItem.stake = info.stake;

                HttpResponseMessage respMsg = m_client.PostAsync("https://api.arcadia.pinnacle.com/0.1/bets/straight", new StringContent(JsonConvert.SerializeObject(placebetItem), Encoding.UTF8, "application/json")).Result;
                //respMsg.EnsureSuccessStatusCode();

                placebetRespBody = respMsg.Content.ReadAsStringAsync().Result;
            }
            catch { }
            return placebetRespBody;
        }
        public double getBalance()
        {
            double balance = -1;
            try
            {
                HttpResponseMessage respMsg = m_client.GetAsync("https://api.arcadia.pinnacle.com/0.1/wallet/balance").Result;
                respMsg.EnsureSuccessStatusCode();

                string respBody = respMsg.Content.ReadAsStringAsync().Result;
                JObject jBal = JObject.Parse(respBody);
                balance = (double)jBal["amount"];
            }
            catch { }
            return balance;
        }
    }
#endif

}
