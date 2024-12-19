namespace Project.Bookie
{
#if (FORTUNA)
    class Fortuna : IBookieController
    {
        public HttpClient m_client = null;
        public string accountId = "";
        public string auth_token = "";
        public Fortuna()
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
            
            if (!Global.bRun)
                return false;

            //
            bool bLogin = false;

            int nRetry = 0;
            while (nRetry++ < 3)
            {
                try
                {
                    string deviceId = Utils.generateGuid();
                    string casino_name = Setting.Instance.domain;
                    if (Setting.Instance.domain.Contains("cz"))
                        casino_name = $"b{Setting.Instance.domain}";

                    string loginUrl = $"https://login.{Setting.Instance.domain}/Login.php?casinoname={casino_name}&realMode=1&rememberMeLogin=1&clientType=casino&clientPlatform=web&clientSkin={casino_name}&deviceId={deviceId}&deviceType=Other&osName=Windows&osVersion=10.0&deviceBrowser=Chrome&languageCode=sk&redirectUrl=https%3A//www.{Setting.Instance.domain}/%23requestId%3D{GetRequestId()}&errorLevel=1&messagesSupported=1";

                    string formDataBoundary = "----WebKitFormBoundary" + Utils.CreateFormDataBoundary();
                    MultipartFormDataContent formContent = new MultipartFormDataContent(formDataBoundary);
                    formContent.Headers.ContentType.CharSet = null;

                    formContent.Headers.ContentType.MediaType = "multipart/form-data";
                    formContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data");

                    formContent.Add(new StringContent(Setting.Instance.username), "username" );
                    formContent.Add(new StringContent(Setting.Instance.password), "password" );
                    formContent.Add(new StringContent("%7B%22Sec-CH-UA-Model%22%3A%22%5C%22%5C%22%22%2C%22Sec-CH-UA-Platform%22%3A%22%5C%22Windows%5C%22%22%2C%22Sec-CH-UA-Platform-Version%22%3A%22%5C%2215.0.0%5C%22%22%2C%22Sec-CH-UA-Full-Version-List%22%3A%22%5C%22Not)A%3BBrand%5C%22%3Bv%3D%5C%2299.0.0.0%5C%22%2C%20%5C%22Google%20Chrome%5C%22%3Bv%3D%5C%22127.0.6533.120%5C%22%2C%20%5C%22Chromium%5C%22%3Bv%3D%5C%22127.0.6533.120%5C%22%22%7D"), "clientHints");
                    formContent.Add(new StringContent("bS%3A0%7CscsVersion%3A2.4.6%7CsdeviceAspectRatio%3A16%2F9%7CsdevicePixelRatio%3A1%7Cbhtml.video.ap4x%3A0%7Cbhtml.video.av1%3A1%7Cbjs.deviceMotion%3A1%7Csjs.webGlRenderer%3AANGLE%20(NVIDIA%2C%20NVIDIA%20GeForce%20GT%20730%20(0x00001287)%20Direct3D11%20vs_5_0%20ps_5_0%2C%20D3D11)%7CsrendererRef%3A01633483327%7CsscreenWidthHeight%3A1920%2F1080%7CsaudioRef%3A781311942%7CbE%3A0"), "deviceAtlasProperties");
                    formContent.Add(new StringContent($"https%3A%2F%2Fwww.{Setting.Instance.domain}"), "eventOrigin");
                    formContent.Add(new StringContent($"https%3A%2F%2Fwww.{Setting.Instance.domain}%2F"), "hr");
                    formContent.Add(new StringContent("Login"), "requestType");
                    formContent.Add(new StringContent("json"), "responseType");
                    
                    HttpResponseMessage respMessage = m_client.PostAsync(loginUrl, formContent).Result;
                    respMessage.EnsureSuccessStatusCode();

                    string respBody = respMessage.Content.ReadAsStringAsync().Result;
                    JObject jResp = JObject.Parse(respBody);

                    if (jResp["username"]["userId"] == null || string.IsNullOrEmpty(jResp["username"]["userId"].ToString()))
                        return bLogin;

                    GetCookies(respMessage);
           
                    string userId = jResp["username"]["userId"].ToString();
                    string session = jResp["rootSessionToken"]["sessionToken"].ToString();

                    string loggedInUrl = $"https://login.{Setting.Instance.domain}/GetLoggedInPlayer.php?casinoname={casino_name}&realMode=1&clientType=sportsbook&clientPlatform=web&clientSkin={casino_name}&deviceId={deviceId}&deviceType=Other&osName=Windows&osVersion=10.0&deviceBrowser=Chrome&languageCode=sk&redirectUrl=https%3A//www.{Setting.Instance.domain}/%23requestId%3D{GetRequestId()}&errorLevel=1&messagesSupported=1";

                    m_client.DefaultRequestHeaders.Referrer = new Uri($"https://login.{Setting.Instance.domain}/pasSetupPage.php?casino={Setting.Instance.domain}");
                    
                    formDataBoundary = "----WebKitFormBoundary" + Utils.CreateFormDataBoundary();
                    formContent = new MultipartFormDataContent(formDataBoundary);
                    formContent.Headers.ContentType.CharSet = null;

                    formContent.Headers.ContentType.MediaType = "multipart/form-data";
                    formContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data");

                    formContent.Add(new StringContent(Setting.Instance.username), "username");
                    formContent.Add(new StringContent(session), "token");
                    formContent.Add(new StringContent(userId), "userId ");
                    formContent.Add(new StringContent($"https%3A%2F%2Fwww.{Setting.Instance.domain}"), "eventOrigin");
                    formContent.Add(new StringContent($"https%3A%2F%2Fwww.{Setting.Instance.domain}%2F"), "hr");
                    formContent.Add(new StringContent("loggedInPlayerHandlerX"), "requestType");
                    formContent.Add(new StringContent("json"), "responseType");

                    respMessage = m_client.PostAsync(loggedInUrl, formContent).Result;
                    respMessage.EnsureSuccessStatusCode();

                    string temparyAuthUrl = $"https://login.{Setting.Instance.domain}/GetTemporaryAuthenticationToken.php?casinoname={casino_name}&serviceType=GamePlay&systemId=66&realMode=1&clientType=sportsbook&clientPlatform=web&clientSkin={casino_name}&deviceId={deviceId}&deviceType=Other&osName=Windows&osVersion=10.0&deviceBrowser=Chrome&languageCode=sk&redirectUrl=https%3A//www.{Setting.Instance.domain}/%23requestId%3D{GetRequestId()}&errorLevel=1&messagesSupported=1";
                    if (Setting.Instance.domain.Contains("cz"))
                        temparyAuthUrl = $"https://login.{Setting.Instance.domain}/GetTemporaryAuthenticationToken.php?casinoname={casino_name}&serviceType=GamePlay&systemId=146&realMode=1&clientType=sportsbook&clientPlatform=web&clientSkin={casino_name}&languageCode=cs&redirectUrl=https%3A//www.{casino_name}/%23requestId%3D{GetRequestId()}&errorLevel=1&messagesSupported=1";

                    formDataBoundary = "----WebKitFormBoundary" + Utils.CreateFormDataBoundary();
                    formContent = new MultipartFormDataContent(formDataBoundary);
                    formContent.Headers.ContentType.CharSet = null;

                    formContent.Headers.ContentType.MediaType = "multipart/form-data";
                    formContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data");

                    formContent.Add(new StringContent(Setting.Instance.username), "username");
                    formContent.Add(new StringContent(session), "token");
                    formContent.Add(new StringContent(userId), "userId ");
                    formContent.Add(new StringContent($"https%3A%2F%2Fwww.{Setting.Instance.domain}"), "eventOrigin");
                    formContent.Add(new StringContent($"https%3A%2F%2Fwww.{Setting.Instance.domain}%2F"), "hr");
                    formContent.Add(new StringContent("GetTemporaryAuthenticationToken"), "requestType");
                    formContent.Add(new StringContent("json"), "responseType");

                    respMessage = m_client.PostAsync(temparyAuthUrl, formContent).Result;
                    respMessage.EnsureSuccessStatusCode();

                    respBody = respMessage.Content.ReadAsStringAsync().Result;
                    jResp = JObject.Parse(respBody);

                    string temp_session = jResp["sessionToken"]["sessionToken"].ToString();

                    m_client.DefaultRequestHeaders.Referrer = new Uri($"https://www.{Setting.Instance.domain}/");

                    string payload = $"{{\"username\":\"{Setting.Instance.username}\",\"password\":\"{temp_session}\"}}";
                    var request_conetnt = new StringContent(payload, Encoding.UTF8, "application/json");
                    request_conetnt.Headers.ContentType.CharSet = string.Empty;

                    respMessage = m_client.PostAsync($"https://api.{Setting.Instance.domain}/_login-desktop", request_conetnt).Result;
                    respMessage.EnsureSuccessStatusCode();

                    //GetCookies(respMessage);

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

            if (!info.isLive)
                return Placebet_prematch(info);
            else
                return Placebet_live(info);
        }

        private PROCESS_RESULT Placebet_prematch(BetburgerInfo info)
        {
            int retryCount = 2;
            while (--retryCount >= 0)
            {
                try
                {
                    string direct_link = info.direct_link;
                    var payload = new StringContent("", Encoding.UTF8, "application/json");
                    payload.Headers.ContentType.CharSet = string.Empty;

                    //add betslip
                    HttpResponseMessage respMessage = m_client.PostAsync($"https://www.{Setting.Instance.domain}/ticket/ajax/M/1/addbet/{direct_link.Split('-')[0].Trim()}/{direct_link.Split('-')[1].Trim()}/{info.odds}", payload).Result;
                    string respBody = respMessage.Content.ReadAsStringAsync().Result;
                    JObject jResp = JObject.Parse(respBody);

                    Thread.Sleep(2000);
                    string betslip_html = jResp["ticketHtml"].ToString();
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(betslip_html);

                    HtmlNode odd_node = doc.DocumentNode.Descendants("strong").FirstOrDefault(node => node.Attributes["class"] != null && node.Attributes["class"].Value == "total-odds");
                    if (odd_node == null)
                    {
                        LogMng.Instance.onWriteStatus("***This line or odds is not existed...***");
                        return PROCESS_RESULT.ERROR;
                    }

                    double newOdds = Utils.ParseToDouble(odd_node.InnerText.Trim());
                    if (CheckOddDropCancelBet(newOdds, info))
                    {
                        ClearTicket();
                        return PROCESS_RESULT.MOVED;
                    }

                    var stake_payload = new StringContent("", Encoding.UTF8, "application/json");
                    stake_payload.Headers.ContentType.CharSet = string.Empty;
                    //Change stake
                    respMessage = m_client.PostAsync($"https://www.{Setting.Instance.domain}/ticket/ajax/M/1/changebetval/{info.stake}", stake_payload).Result;
                    respBody = respMessage.Content.ReadAsStringAsync().Result;

                    Thread.Sleep(2000);
                    //Placebet
                    var placebet_payload = new StringContent("", Encoding.UTF8, "application/json");
                    placebet_payload.Headers.ContentType.CharSet = string.Empty;

                    respMessage = m_client.PostAsync($"https://www.{Setting.Instance.domain}/ticket/ajax/M/1/acceptticket/NONE", placebet_payload).Result;
                    respBody = respMessage.Content.ReadAsStringAsync().Result;

                    if(respMessage.StatusCode == HttpStatusCode.Unauthorized || respMessage.StatusCode != HttpStatusCode.OK)
                    {
                        LogMng.Instance.onWriteStatus("***UnAuthorized***");
                        ClearTicket();

                        Global.cookieContainer = new CookieContainer();
                        m_client = initHttpClient();

                        bool bLogin = login();
                        if (bLogin)
                        {
                            LogMng.Instance.onWriteStatus("Logged In Successfully!");
                            Thread.Sleep(3000);
                        }
                        continue;
                    }
                    jResp = JObject.Parse(respBody);
                    if (jResp["phase"] != null && jResp["ticketId"] != null && jResp["phase"].ToString() == "ACCEPTED")
                    {
                        return PROCESS_RESULT.PLACE_SUCCESS;
                    }
                    else if (jResp["phase"] != null && jResp["phase"].ToString() == "DELAYING")
                    {
                        int retryCnt = 10;
                        while (retryCnt-- > 0)
                        {
                            payload = new StringContent("", Encoding.UTF8, "application/json");
                            payload.Headers.ContentType.CharSet = string.Empty;

                            //Resolve
                            respMessage = m_client.PostAsync($"https://www.{Setting.Instance.domain}/ticket/ajax/M/1/resolveticket/{jResp["transactId"].ToString()}", payload).Result;
                            respBody = respMessage.Content.ReadAsStringAsync().Result;

                            jResp = JObject.Parse(respBody);
                            if (jResp["phase"] != null && jResp["phase"].ToString() == "DELAYING")
                                Thread.Sleep(4000);
                            else if (jResp["phase"] != null && jResp["phase"].ToString() == "ACCEPTED")
                                return PROCESS_RESULT.PLACE_SUCCESS;
                        }
                    }
                    else if (jResp["messages"] != null)
                    {
                        LogMng.Instance.onWriteStatus($"***Placebet Failed : {jResp["messages"][0]["message"].ToString()}***");
                        ClearTicket();
                        return PROCESS_RESULT.ERROR;
                    }
                    else
                    {
                        ClearTicket();
                        return PROCESS_RESULT.ERROR;
                    }


                }
                catch (Exception e)
                {
                    LogMng.Instance.onWriteStatus($"Placebet_prematch +  {e.ToString()}");
                    ClearTicket();
                }
            }
            LogMng.Instance.onWriteStatus(string.Format("** PLACE BET FAIL"));
            return PROCESS_RESULT.ERROR;
        }

        private PROCESS_RESULT Placebet_live(BetburgerInfo info)
        {
            int retryCount = 2;
            while (--retryCount >= 0)
            {
                try
                {

                    string direct_link = info.direct_link;
                    var payload = new StringContent($"{{\"tipId\":{direct_link.Split('|')[0]},\"info\":{direct_link.Split('|')[1]},\"value\":{info.odds}}}", Encoding.UTF8, "application/json");
                    payload.Headers.ContentType.CharSet = string.Empty;

                    //add betslip
                    HttpResponseMessage respMessage = m_client.PostAsync($"https://api.{Setting.Instance.domain}/live3/api/live/ticket/1/addbet", payload).Result;
                    string respBody = respMessage.Content.ReadAsStringAsync().Result;

                    JObject jResp = JObject.Parse(respBody);
                    if (jResp["operation"] == null || jResp["operation"].ToString() != "FINISHED" || jResp["ticket"] == null)
                    {
                        ClearTicket();
                        return PROCESS_RESULT.ERROR;
                    }

                    double newOdds = Utils.ParseToDouble(jResp["ticket"]["totalOdds"].ToString());
                    if (CheckOddDropCancelBet(newOdds, info))
                    {
                        ClearTicket();
                        return PROCESS_RESULT.MOVED;
                    }

                    Thread.Sleep(1200);
                    //Change stake
                    payload = new StringContent($"{{\"value\":{info.stake}}}", Encoding.UTF8, "application/json");
                    payload.Headers.ContentType.CharSet = string.Empty;

                    respMessage = m_client.PostAsync($"https://api.{Setting.Instance.domain}/live3/api/live/ticket/1/changepayval", payload).Result;
                    respBody = respMessage.Content.ReadAsStringAsync().Result;

                    jResp = JObject.Parse(respBody);

                    long selId = (long)jResp["ticket"]["items"][0]["selectedOdds"]["id"];
                    string oddsId  = jResp["ticket"]["items"][0]["selectedOdds"]["oddsId"].ToString();
                    newOdds = (double) jResp["ticket"]["items"][0]["selectedOdds"]["value"];
                    string marketId = jResp["ticket"]["items"][0]["marketId"].ToString();
                    string transactionId = Utils.generateGuid();

                    payload = new StringContent($"{{\"transactionId\":\"{transactionId}\",\"changesHandlingType\":\"IGNORE\",\"payValue\":{info.stake},\"odds\":[{{\"id\":{selId},\"oddsId\":\"{oddsId}\",\"name\":\"NIE\",\"value\":{newOdds},\"result\":null,\"marketId\":\"{marketId}\"}}]}}", Encoding.UTF8, "application/json");
                    payload.Headers.ContentType.CharSet = string.Empty;

                    Thread.Sleep(1500);
                    //Placebet
                    respMessage = m_client.PostAsync($"https://api.{Setting.Instance.domain}/live3/api/live/ticket/1/accept", payload).Result;
                    respBody = respMessage.Content.ReadAsStringAsync().Result;


                    if (respMessage.StatusCode == HttpStatusCode.Unauthorized || respMessage.StatusCode != HttpStatusCode.OK)
                    {
                        LogMng.Instance.onWriteStatus("***UnAuthorized***");
                        ClearTicket();

                        Global.cookieContainer = new CookieContainer();
                        m_client = initHttpClient();

                        bool bLogin = login();
                        if (bLogin)
                        {
                            LogMng.Instance.onWriteStatus("Logged In Successfully!");
                            Thread.Sleep(3000);
                        }
                        continue;
                    }

                    jResp = JObject.Parse(respBody);
                    if (jResp["phase"] != null && jResp["phase"].ToString() == "ACCEPTED")
                    {
                        return PROCESS_RESULT.PLACE_SUCCESS;
                    }
                    else if (jResp["phase"] != null && jResp["phase"].ToString() == "DELAYING")
                    {
                        int retryCnt = 10;
                        while (retryCnt-- > 0)
                        {
                            payload = new StringContent($"{{\"transactionId\":\"{transactionId}\"}}", Encoding.UTF8, "application/json");
                            payload.Headers.ContentType.CharSet = string.Empty;

                            //Resolve
                            respMessage = m_client.PostAsync($"https://api.{Setting.Instance.domain}/live3/api/live/ticket/1/resolve", payload).Result;
                            respBody = respMessage.Content.ReadAsStringAsync().Result;

                            jResp = JObject.Parse(respBody);
                            if (jResp["phase"] != null && jResp["phase"].ToString() == "DELAYING")
                                Thread.Sleep(1000);
                            else if(jResp["phase"] != null && jResp["phase"].ToString() == "ACCEPTED")
                                return PROCESS_RESULT.PLACE_SUCCESS;
                        }
          
                    }
                    else if (jResp["messages"] != null)
                    {
                        LogMng.Instance.onWriteStatus($"***Placebet Failed : {jResp["messages"][0]["text"].ToString()}***");
                        ClearTicket();
                        return PROCESS_RESULT.ERROR;
                    }
                    else
                    {
                        ClearTicket();
                        return PROCESS_RESULT.ERROR;
                    }


                }
                catch (Exception e)
                {
                    LogMng.Instance.onWriteStatus($"Placebet_Live +  {e.ToString()}");
                    ClearTicket();
                }
            }
            LogMng.Instance.onWriteStatus(string.Format("** PLACE BET FAIL"));
            return PROCESS_RESULT.ERROR;
        }


        private void ClearTicket()
        {
            try
            {
                Thread.Sleep(2000);
                var payload = new StringContent("", Encoding.UTF8, "application/json");
                payload.Headers.ContentType.CharSet = string.Empty;

                HttpResponseMessage respMessage = m_client.PostAsync($"https://www.{Setting.Instance.domain}/ticket/ajax/M/1/clearticket", payload).Result;
                string respBody = respMessage.Content.ReadAsStringAsync().Result;
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
                    var payload = new StringContent("");
                    payload.Headers.ContentType.CharSet = string.Empty;

                    HttpResponseMessage balanceResponseMessage = m_client.PostAsync($"https://www.{Setting.Instance.domain}/ajax/user/refreshCreditAndBonusPoints" , payload).Result;
                    balanceResponseMessage.EnsureSuccessStatusCode();

                    string balanceContent = balanceResponseMessage.Content.ReadAsStringAsync().Result;
                    JObject balanceObj = JObject.Parse(balanceContent);

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(balanceObj["creditFormatted"].ToString());

                    IEnumerable<HtmlNode> nodes = doc.DocumentNode.Descendants("span");
                    if (nodes != null && nodes.LongCount() > 0)
                        nodes.ToArray()[0].Remove();

                    string balance_str = doc.DocumentNode.InnerText;
                    balance = Utils.ParseToDouble(balance_str);
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
