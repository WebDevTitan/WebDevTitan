namespace Project.Bookie
{
#if (BET365_ADDON)

    public class WindowHandleInfo
    {
        private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        private IntPtr _MainHandle;

        public WindowHandleInfo(IntPtr handle)
        {
            this._MainHandle = handle;
        }

        public List<IntPtr> GetAllChildHandles()
        {
            List<IntPtr> childHandles = new List<IntPtr>();

            GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
            IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                EnumChildWindows(this._MainHandle, childProc, pointerChildHandlesList);
            }
            finally
            {
                gcChildhandlesList.Free();
            }

            return childHandles;
        }

        private bool EnumWindow(IntPtr hWnd, IntPtr lParam)
        {
            GCHandle gcChildhandlesList = GCHandle.FromIntPtr(lParam);

            if (gcChildhandlesList == null || gcChildhandlesList.Target == null)
            {
                return false;
            }

            List<IntPtr> childHandles = gcChildhandlesList.Target as List<IntPtr>;
            childHandles.Add(hWnd);

            return true;
        }
    }
    class ClientWebSocket : WebSocketBehavior
    {        
        public ClientWebSocket()
        {
           
        }

        private void QRGetProc()
        {

        }

        public ClientWebSocket(string prefix)
        {
            
        }     

        protected override void OnClose(CloseEventArgs e)
        {
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"[WebSocket:OnClose] {e.Reason}");
#endif
            Bet365_ADDONCtrl.bWebSocketConnected = false;
        }

        protected override void OnError(ErrorEventArgs e)
        {
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"[WebSocket:OnError] {e.Message}");
#endif
            Bet365_ADDONCtrl.bWebSocketConnected = false;
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            Bet365_ADDONCtrl.bWebSocketConnected = true;
#if (TROUBLESHOT)
            //LogMng.Instance.onWriteStatus($"[WebSocket:OnMessage] {e.Data}");
#endif

            try
            {
                var responseJson = JsonConvert.DeserializeObject<dynamic>(e.Data);
                if (responseJson.type.ToString() == "domainresult" ||
                    responseJson.type.ToString() == "maximizeresult" ||
                    responseJson.type.ToString() == "openpageresult")
                {
                    Bet365_ADDONCtrl.wait_EvalResultEvent.Set();
                }
                else if (responseJson.type.ToString() == "scriptresult")
                {                    
                    Bet365_ADDONCtrl.wait_EvalResult = responseJson.response.ToString();
#if (TROUBLESHOT)
                    //LogMng.Instance.onWriteStatus($"eval Res: {Bet365_ADDONCtrl.wait_EvalResult}");
#endif
                    Bet365_ADDONCtrl.wait_EvalResultEvent.Set();
                }
                else if (responseJson.type.ToString() == "titlebarresult")
                {
                    try
                    {
                        Bet365_ADDONCtrl.titlebarHeight = Convert.ToInt32(responseJson.response.ToString());
#if (TROUBLESHOT)
                        //LogMng.Instance.onWriteStatus($"titlebar height: {Bet365_ADDONCtrl.titlebarHeight}");
#endif
                    }
                    catch (Exception ex){
#if (TROUBLESHOT)
                        //LogMng.Instance.onWriteStatus($"titlebar height error: {ex}");
#endif
                    }
                    Bet365_ADDONCtrl.wait_EvalResultEvent.Set();
                }
                else if (responseJson.type.ToString() == "webresponse")
                {
                    if (responseJson.resurl.ToString().ToLower().Contains("/betswebapi/addbet"))
                    {
                        Bet365_ADDONCtrl.wait_AddbetResult = responseJson.response.ToString();
//#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"addbet Res: {Bet365_ADDONCtrl.wait_AddbetResult}");
//#endif
                        Bet365_ADDONCtrl.wait_AddbetResultEvent.Set();
                    }
                    else if (responseJson.resurl.ToString().ToLower().Contains("/betswebapi/placebet"))
                    {
                        Bet365_ADDONCtrl.wait_PlacebetResult = responseJson.response.ToString();
//#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"placebet Res: {Bet365_ADDONCtrl.wait_PlacebetResult}");
//#endif
                        Bet365_ADDONCtrl.wait_PlacebetResultEvent.Set();
                    }
                }
                else if (responseJson.type.ToString() == "webrequest")
                {
                    if (responseJson.requrl.ToString().ToLower().Contains("/betswebapi/addbet"))
                    {
                        Bet365_ADDONCtrl.wait_AddbetRequest = WebUtility.UrlDecode(responseJson.request.ToString());
                        //#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"addbet Req: {Bet365_ADDONCtrl.wait_AddbetRequest}");
                        //#endif
                        Bet365_ADDONCtrl.wait_AddbetRequestEvent.Set();
                    }
                    else if (responseJson.requrl.ToString().ToLower().Contains("/betswebapi/placebet"))
                    {
                        Bet365_ADDONCtrl.wait_PlacebetRequest = WebUtility.UrlDecode(responseJson.request.ToString());
                        //#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"placebet Req: {Bet365_ADDONCtrl.wait_PlacebetRequest}");
                        //#endif
                        Bet365_ADDONCtrl.wait_PlacebetRequestEvent.Set();
                    }
                }
                else if (responseJson.type.ToString().ToLower().Contains("qrpost"))
                {
                    
                    JObject betData = new JObject();
                    
                    betData["xcft"] = responseJson.xcft.ToString();
                    betData["lat"] = responseJson.lat.ToString().Replace(",", ".");
                    betData["lon"] = responseJson.lon.ToString().Replace(",", ".");
                    betData["countryCode"] = Bet365_ADDONCtrl.AccountCountryCode;
                    betData["bet365Username"] = Setting.Instance.username;

                    ScanQrCode(0, betData);
                    //UserMng.GetInstance().SendQRRequest(0, betData);
                }
            }
            catch { }
        }

        private void ScanQrCode(int nMode , JObject betData)
        {
            try
            {
                if (nMode == 0)
                {
                    LogMng.Instance.onWriteStatus($"QR Post xcft: {betData["xcft"]} lat: {betData["lat"]} lon: {betData["lon"]} countryCode: {betData["countryCode"]}");

                    betData["apiKey"] = "2e27fbbb49bd4b07b0ddfab8371a674a";
                    betData["region"] = "";
                    if (betData["countryCode"].ToString() == "ES")
                    {                                     //Guille           
                        betData["host"] = "www.bet365.es";
                        betData["proxy"] = $"http://nikukurti:HjX6pcx84Uzbcm4mgT@es.smartproxy.com:10001";

                        //SendLog($"QR Post Spain proxy IP: {SpainProxyIP}");
                    }
                    else if (betData["countryCode"].ToString() == "BG")
                    {               //NAS X
                        betData["host"] = "www.bet365.com";
                        betData["proxy"] = "http://beta88:CtyuM3xX7mz8aTdxz6@bg.smartproxy.com:38026";
                    }
                    else if (betData["countryCode"].ToString() == "IT")
                    {
                        betData["host"] = "www.bet365.it";
                        betData["proxy"] = "http://marvit5:Nirvana1701@s4.airproxy.io:21210";
                    }
                    else if (betData["countryCode"].ToString() == "BD")
                    {   //JC_Spain
                        betData["host"] = "www.3256871.com";
                        betData["proxy"] = "http://adminip:J521Osnam$@103.150.136.88:41609";
                    }

                    string placebetPostContent = JsonConvert.SerializeObject(betData);

                    LogMng.Instance.onWriteStatus($"QR Post Request: {placebetPostContent}");
                    using (HttpClient newHttpClient = new HttpClient())
                    {
                        HttpResponseMessage qrBet365Response = newHttpClient.PostAsync("https://qrresolver.bettingco.ru/api/QrResolver/GetQrBet365", new StringContent(placebetPostContent, Encoding.UTF8, "application/json")).Result;
                        HttpStatusCode StatusCode = qrBet365Response.StatusCode;
                        LogMng.Instance.onWriteStatus($"QR Post Response StatusCode: {qrBet365Response.StatusCode}");
                        try
                        {
                            string strContent = qrBet365Response.Content.ReadAsStringAsync().Result;
                            LogMng.Instance.onWriteStatus($"QR Post Response: {strContent}");
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        protected override void OnOpen()
        {
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"[WebSocket:OnOpen]");
#endif
            Bet365_ADDONCtrl.bWebSocketConnected = true;
        }
    }
    public class Bet365_ADDONCtrl : IBookieController
    {
        //communication with page Thread.
        private Thread pageThread = null;

        private object taskLocker = new object();

        private DateTime refreshLastTime = DateTime.MinValue;

#if (CHROME || EDGE)
        public static int titlebarHeight = 75;
#elif (FIREFOX)
        public static int titlebarHeight = 84;
#endif
        public static string QRGetRefreshToken = "";
        public static DateTime timeToSendQRGetRequest = DateTime.MaxValue;

        public static string AccountCountryCode = "";

        public static bool bWebSocketConnected = false;
        public static ManualResetEventSlim wait_EvalResultEvent = new ManualResetEventSlim();
        public static string wait_EvalResult = string.Empty;

        public static ManualResetEventSlim wait_AddbetResultEvent = new ManualResetEventSlim();
        public static string wait_AddbetResult = string.Empty;

        public static ManualResetEventSlim wait_PlacebetResultEvent = new ManualResetEventSlim();
        public static string wait_PlacebetResult = string.Empty;


        public static ManualResetEventSlim wait_AddbetRequestEvent = new ManualResetEventSlim();
        public static string wait_AddbetRequest = string.Empty;

        public static ManualResetEventSlim wait_PlacebetRequestEvent = new ManualResetEventSlim();
        public static string wait_PlacebetRequest = string.Empty;

        Dictionary<string, int> Bet365InnerSportNumber = new Dictionary<string, int>
        {
            {"american football", 12},
            {"rugby", 19},
            {"baseball", 16},
            {"basketball", 18},
            {"esports", 151},
            {"e-sports", 151},
            {"horse racing", 2},
            {"hockey", 17},
            {"soccer", 1},
            {"table tennis", 92},
            {"tennis", 13},
            {"volleyball", 91},
            {"handball", 78}
        };

        public object locker = new object();
        double last_x = 0, last_y = 0;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOZORDER = 0x0004;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public int GetPendingbets()
        {
            return 0;
        }
        private void SetWindowTopMost()
        {
            double height = SystemParameters.FullPrimaryScreenHeight;
            double width = SystemParameters.FullPrimaryScreenWidth;

            RECT rct = new RECT();
            GetWindowRect(Global.ViewerHwnd, ref rct);

            //LogMng.Instance.onWriteStatus($"SetWindowTopMost hwnd: {Global.ViewerHwnd} rect: {rct.Left},{rct.Top} - {rct.Right},{rct.Bottom}");

            if (rct.Left < 0 || rct.Right > width || rct.Bottom > height)
                SetWindowPos(Global.ViewerHwnd, IntPtr.Zero, -8, -8, 0, 0, SWP_NOSIZE | SWP_NOZORDER);

            SetForegroundWindow(Global.ViewerHwnd);
        }

        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);


#pragma warning disable 649
        //internal struct INPUT229157
        //{
        //    public UInt32 Type;
        //    public MOUSEKEYBDHARDWAREINPUT Data;
        //}

        //[StructLayout(LayoutKind.Explicit)]
        //internal struct MOUSEKEYBDHARDWAREINPUT
        //{
        //    [FieldOffset(0)]
        //    public MOUSEINPUT Mouse;
        //}

        //internal struct MOUSEINPUT
        //{
        //    public Int32 X;
        //    public Int32 Y;
        //    public UInt32 MouseData;
        //    public UInt32 Flags;
        //    public UInt32 Time;
        //    public IntPtr ExtraInfo;
        //}


        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            public SendInputEventType type;
            public MouseKeybdhardwareInputUnion mkhi;
        }
        [StructLayout(LayoutKind.Explicit)]
        internal struct MouseKeybdhardwareInputUnion
        {
            [FieldOffset(0)]
            public MouseInputData mi;

            [FieldOffset(0)]
            public KEYBDINPUT ki;

            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }
        internal struct MouseInputData
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public MouseEventFlags dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        [Flags]
        internal enum MouseEventFlags : uint
        {
            MOUSEEVENTF_MOVE = 0x0001,
            MOUSEEVENTF_LEFTDOWN = 0x0002,
            MOUSEEVENTF_LEFTUP = 0x0004,
            MOUSEEVENTF_RIGHTDOWN = 0x0008,
            MOUSEEVENTF_RIGHTUP = 0x0010,
            MOUSEEVENTF_MIDDLEDOWN = 0x0020,
            MOUSEEVENTF_MIDDLEUP = 0x0040,
            MOUSEEVENTF_XDOWN = 0x0080,
            MOUSEEVENTF_XUP = 0x0100,
            MOUSEEVENTF_WHEEL = 0x0800,
            MOUSEEVENTF_VIRTUALDESK = 0x4000,
            MOUSEEVENTF_ABSOLUTE = 0x8000
        }
        internal enum SendInputEventType : int
        {
            InputMouse,
            InputKeyboard,
            InputHardware
        }

#pragma warning restore 649

        private int scrollx = 0, scrolly = 0;
        public void Scroll(uint ScrollHeight = 0xFFFFFE98) //-360
        {
            Point clientPoint = new System.Windows.Point(scrollx, scrolly);
#if !(DEBUG)
            SetWindowTopMost();
            Thread.Sleep(300);
#endif
            

            RECT rct = new RECT();
            GetWindowRect(Global.ViewerHwnd, ref rct);

            //LogMng.Instance.onWriteStatus($"Inner Scroll hwnd: {Global.ViewerHwnd} rect: {rct.Left},{rct.Top} - {rct.Right},{rct.Bottom}");
            //try
            //{
            //    int height = Convert.ToInt32(RunScript("window.innerHeight"));
            //    titlebarHeight = rct.Bottom - rct.Top - height;
            //}
            //catch { }
            clientPoint.Y += titlebarHeight;

            clientPoint.X += rct.Left;
            clientPoint.Y += rct.Top;
            /// get screen coordinates
            //ClientToScreen(Global.ViewerHwnd, ref clientPoint);

            /// set cursor on coords, and press mouse
            //System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)clientPoint.X, (int)clientPoint.Y);

            var inputMouseWheel = new INPUT();
            inputMouseWheel.type = SendInputEventType.InputMouse;

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"page_Scroll {clientPoint.X}, {clientPoint.Y}");
#endif
            MoveMouse((int)clientPoint.X, (int)clientPoint.Y);

            inputMouseWheel.mkhi.mi.dx = CalculateAbsoluteCoordinateX((int)clientPoint.X);
            inputMouseWheel.mkhi.mi.dy = CalculateAbsoluteCoordinateY((int)clientPoint.Y);
            inputMouseWheel.mkhi.mi.mouseData = ScrollHeight;
            inputMouseWheel.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_WHEEL | MouseEventFlags.MOUSEEVENTF_ABSOLUTE;
            inputMouseWheel.mkhi.mi.time = 0;


            var inputs = new INPUT[] { inputMouseWheel };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

            /// return mouse 
            //Cursor.Position = oldPos;
        }
        [DllImport("USER32.DLL")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn,
            IntPtr lParam);
        static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
        {
            var handles = new List<IntPtr>();

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
                EnumThreadWindows(thread.Id,
                    (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);

            return handles;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        public static string GetWindowTitle(IntPtr hWnd)
        {
            var length = GetWindowTextLength(hWnd) + 1;
            var title = new StringBuilder(length);
            GetWindowText(hWnd, title, length);
            return title.ToString();
        }

        private bool EnterEventPageFromLivePage(BetburgerInfo info)
        {
            //from Live Soccer Lobby enter
            string targetPageData = "";

            string EventID = "";
            if (info.eventUrl.IndexOf("#") > 0)
            {
                targetPageData = info.eventUrl.Substring(info.eventUrl.IndexOf("#") + 1);
                targetPageData = targetPageData.Replace("/", "#");
            }

            OpenBet_Bet365 openbet = Utils.ConvertBetburgerPick2OpenBet_365(info);
            if (openbet != null)
                EventID = openbet.betData[0].fd;

            string origUrl = RunScript("ns_navlib_util.WebsiteNavigationManager.CurrentPageData");
            if (!string.IsNullOrEmpty(targetPageData))
            {                
                if (origUrl == targetPageData)
                {
                    LogMng.Instance.onWriteStatus("Already opend target Match Page");
                    return true;
                }
                if (Setting.Instance.bPlaceFastMode)
                {
                    Page_Navigate(targetPageData);
                    WaitSpinnerShowing();
                    LogMng.Instance.onWriteStatus("Enter match success in fast mode");
                    return true;
                }
            }
            if (origUrl != "#IP#B1")
            {
                Page_Navigate("#IP#B1");
                WaitSpinnerShowing();
            }

            string function = "function IsVisibleElement(targetDiv) { var rect = targetDiv.getBoundingClientRect(); var isVisible = (rect.top >= 0) && (rect.bottom <= window.innerHeight - 10); if (document.elementFromPoint(rect.left + 1, rect.top + 1) != null && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.top + 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.left + 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.bottom - 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.top + 1) != null && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.top + 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.bottom - 1) != targetDiv) { isVisible = false; } if (isVisible) { return true; } return false; } function GetFindResultList(sport) { var resultlist = []; var inplayKey = ''; for (var key in Locator.treeLookup._table) { if (key.startsWith('OVInPlay_')) { inplayKey = key; break; } } if (inplayKey == '') return 'NoInplayKey'; var Sports_List = Locator.treeLookup.getReference(inplayKey)._actualChildren; try { for (let i = 0; i < Sports_List.length; i++) { let CL_item = Sports_List[i]; if (CL_item == null || CL_item.nodeName !== 'CL' || CL_item.data == null) continue; if (CL_item.data.ID != sport) continue; for (let j = 0; j < CL_item._actualChildren.length; j++) { let CT_item = CL_item._actualChildren[j]; if (CT_item == null || CT_item.nodeName !== 'CT' || CT_item.data == null) continue; for (let m = 0; m < CT_item._actualChildren.length; m++) { let EV_item = CT_item._actualChildren[m]; if (EV_item == null || EV_item.nodeName !== 'EV' || EV_item.data == null) continue; try { var matchData = {}; matchData.data = EV_item.data; matchData.pageData = '#IP#EV' + EV_item.getLegacyInPlayNavigationID(); for (let k = 0; k < EV_item._delegateList.length; k++) { if (EV_item._delegateList[k] != null && EV_item._delegateList[k].classificationId != null && EV_item._delegateList[k].classificationId == '1') { matchData.position = JSON.stringify(EV_item._delegateList[k]._active_element.getBoundingClientRect()); matchData.isVisible = IsVisibleElement(EV_item._delegateList[k]._active_element); break; } } resultlist.push(matchData); } catch (e) { console.log('exception1: ' + e); } } } } if (resultlist.length <= 0) { return 'NoSearchEvent'; } return resultlist; } catch (e) { console.log('exception2: ' + e); } return resultlist; } GetFindResultList('1');";

            int nRetry = 0;
            while (nRetry++ < 20)
            {
                string matchListResult = RunScript(function);

#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"matchList: {matchListResult}");
#endif
                Rect searchFirstResultRect = new Rect(0, 0, 0, 0);
                try
                {
                    double highsimilarity = 0;
                    dynamic highValue = null;
                    dynamic resultlist = JsonConvert.DeserializeObject<dynamic>(matchListResult);
                    foreach (var resultitr in resultlist)
                    {
                        try
                        {
                            double ratio1, ratio2;
                            string searchName = info.eventTitle;
                            double similarity = Similarity.GetSimilarityRatio(resultitr.data.NA.ToString(), searchName, out ratio1, out ratio2);
//#if (TROUBLESHOT)
                        //LogMng.Instance.onWriteStatus($"PageData: {resultitr.pageData.ToString()} searchItr: {resultitr.data.NA.ToString()} similarity: {similarity}");
                            //#endif
                            if (!string.IsNullOrEmpty(targetPageData) && targetPageData == resultitr.pageData.ToString())
                            {
                                highsimilarity = 100;
                                highValue = resultitr;
                                //LogMng.Instance.onWriteStatus($"same as pageData, it's ok");
                                break;
                            }

                            if (!string.IsNullOrEmpty(EventID) && EventID == resultitr.data.C3.ToString())
                            {
                                highsimilarity = 100;
                                highValue = resultitr;
                                //LogMng.Instance.onWriteStatus($"same as EventID, it's ok");
                                break;
                            }

                            if (highsimilarity < similarity)
                            {
                                highsimilarity = similarity;
                                highValue = resultitr;
                            }
                        }
                        catch { }
                    }

                    if (highsimilarity < 60)
                    {
                        //LogMng.Instance.onWriteStatus("No similar name match");
                        return false;
                    }
                    //LogMng.Instance.onWriteStatus($"Entering eventPage {highValue.pageData.ToString()}");
                    if (Setting.Instance.bPlaceFastMode || nRetry > 3)
                    {
                        Page_Navigate(highValue.pageData.ToString());
                        WaitSpinnerShowing();
                        LogMng.Instance.onWriteStatus("Enter match success in fast mode");
                        return true;
                    }
                    string isVisible = highValue.isVisible.ToString().ToLower();
                    if (highValue.isVisible.ToString().ToLower() == "false")
                    {
                        if (IsScrollDownmost())
                        {
                            LogMng.Instance.onWriteStatus($"Scroll is in Downmost");
                            break;
                        }
                        Scroll();
                        Thread.Sleep(500);
                        continue;
                    }
                    Rect iconRect = Utils.ParseRectFromJson(highValue.position.ToString());
                    if (iconRect.Y <= 0 || iconRect.Width <= 0 || iconRect.Height <= 0)
                    {                       
                        LogMng.Instance.onWriteStatus($"Match item is visible, but position is wrong");
                        Scroll();
                        Thread.Sleep(500);
                        continue;
                        //return false;
                    }
                    
                    origUrl = RunScript("ns_navlib_util.WebsiteNavigationManager.CurrentPageData");                    
                    Page_MouseClick(iconRect);
                    WaitSpinnerShowing();
                    Thread.Sleep(500);
                    if (RunScript("ns_navlib_util.WebsiteNavigationManager.CurrentPageData") != origUrl)
                    {
                        LogMng.Instance.onWriteStatus("EnterEventPageFromLivePage successed");
                        return true;
                    }
                }
                catch { }
                {

                }
            }
            LogMng.Instance.onWriteStatus("EnterEventPageFromLivePage failed");
            return false;
        }
        private void CloseTemaviewerPopup()
        {
            foreach (Process pList in Process.GetProcesses())
            {

                if (pList.ProcessName.ToLower() == "teamviewer")
                {
                    foreach (var handle in EnumerateProcessWindowHandles(pList.Id))
                    {
                        StringBuilder className = new StringBuilder(100);
                        int nret = GetClassName(handle, className, className.Capacity);
#if (TROUBLESHOT)
                        //LogMng.Instance.onWriteStatus($"className: {className.ToString()}");
#endif
                        if (className.ToString().Contains("#32770"))
                        {
                            string title = GetWindowTitle(handle);

                            var allChildWindows = new WindowHandleInfo(handle).GetAllChildHandles();

#if (TROUBLESHOT)
             //               LogMng.Instance.onWriteStatus($"title: {title} child count: {allChildWindows.Count}");
#endif

                            RECT rct = new RECT();
                            GetWindowRect(allChildWindows[3], ref rct);
#if (TROUBLESHOT)
             //               LogMng.Instance.onWriteStatus($"teamviewer accpet pos: {rct.Left},{rct.Top} - {rct.Right},{rct.Bottom}");
#endif
                            Page_MouseClick(new Rect(rct.Left, rct.Top, rct.Right - rct.Left, rct.Bottom - rct.Top), false);
                            //foreach (var chdhandle in allChildWindows)
                            //{
                            //    StringBuilder chclassName = new StringBuilder(100);
                            //    int chnret = GetClassName(chdhandle, chclassName, chclassName.Capacity);

                            //    string chTitle = GetWindowTitle(chdhandle);

                            //    LogMng.Instance.onWriteStatus($"child className: {chclassName} title: {chTitle}");
                            //}
                        }
                    }

                }
            }
        }
        private bool CatchBrowser()
        {
            int nRetry = 0;
            IntPtr hWnd = IntPtr.Zero;
            foreach (Process pList in Process.GetProcesses())
            {
#if (CHROME)
                if (pList.ProcessName.ToLower() == "chrome")
                {
                    foreach (var handle in EnumerateProcessWindowHandles(pList.Id))
                    {
                        StringBuilder className = new StringBuilder(100);
                        int nret = GetClassName(handle, className, className.Capacity);
                        if (className.ToString() == "Chrome_WidgetWin_1")
                        {
                            string title = GetWindowTitle(handle);
                            if (title.Contains("Google Chrome") && !title.Contains("Developer Tools"))
                            {
                                Global.ViewerHwnd = handle;
#if (TROUBLESHOT)
                                //LogMng.Instance.onWriteStatus($"Chrome handle found: {Global.ViewerHwnd}");
#else
                                //LogMng.Instance.onWriteStatus($"Chrome is connected");
#endif
                                return true;
                            }
                        }                        
                    }

                }
#elif (EDGE)
                if (pList.ProcessName.ToLower() == "msedge")
                {
                    foreach (var handle in EnumerateProcessWindowHandles(pList.Id))
                    {
                        StringBuilder className = new StringBuilder(100);
                        int nret = GetClassName(handle, className, className.Capacity);
                        if (className.ToString() == "Chrome_WidgetWin_1")
                        {
                            string title = GetWindowTitle(handle);
                            if (title.Contains("Microsoft​ Edge") && !title.Contains("DevTools"))
                            {
                                Global.ViewerHwnd = handle;
#if (TROUBLESHOT)
                                //LogMng.Instance.onWriteStatus($"Firefox handle found: {Global.ViewerHwnd}");
#else
                                //LogMng.Instance.onWriteStatus($"Firefox is connected");
#endif
                                return true;
                            }
                        }
                    }

                }
#elif (FIREFOX)
                if (pList.ProcessName.ToLower() == "firefox")
                {
                    foreach (var handle in EnumerateProcessWindowHandles(pList.Id))
                    {
                        StringBuilder className = new StringBuilder(100);
                        int nret = GetClassName(handle, className, className.Capacity);
                        if (className.ToString() == "MozillaWindowClass")
                        {
                            string title = GetWindowTitle(handle);
                            if ((title.Contains("Mozilla Firefox") || title.Contains("Stealthfox")) && !title.Contains("Developer Tools"))
                            {
                                Global.ViewerHwnd = handle;
#if (TROUBLESHOT)
                                //LogMng.Instance.onWriteStatus($"Firefox handle found: {Global.ViewerHwnd}");
#else
                                //LogMng.Instance.onWriteStatus($"Firefox is connected");
#endif
                                return true;
                            }
                        }
                    }

                }
#endif
            }

#if (CHROME)
            LogMng.Instance.onWriteStatus($"Chrome is not running! Please check again.");
#elif (EDGE)
            LogMng.Instance.onWriteStatus($"Edge is not running! Please check again.");
#elif (FIREFOX)
            LogMng.Instance.onWriteStatus($"Firefox is not running! Please check again.");
#endif
            return false;
        }


        Random rnd = new Random();
        public Bet365_ADDONCtrl()
        {
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"Bet365_BMCtrl Start");
#endif

            CatchBrowser();

            //CloseTemaviewerPopup();

            pageThread = new Thread(PageManagerProc);
            pageThread.Start();
        }


        public void Close()
        {

            if (pageThread != null)
                pageThread.Abort();

        }

        public bool SendEvalCommand(string type, string message)
        {
            if (string.IsNullOrEmpty(type))
                return true;

#if (TROUBLESHOT)
            //LogMng.Instance.onWriteStatus($"SendEvalCommand {message}");
#endif
            string request = $"{{\"type\":\"{type}\", \"body\":\"{message}\"}}";
            if (Global.socketServer != null)
            {
                try
                {
                    Global.socketServer.WebSocketServices.Broadcast(request);
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine("BaccaratA WebSocketServer broadcasting error : " + ex);
                }
            }
            return true;
        }

        private int GetBrowserInnerHeight()
        {
            int result = 500;
            RECT rct = new RECT();
            GetWindowRect(Global.ViewerHwnd, ref rct);


            result = rct.Bottom - rct.Top - titlebarHeight - 50;


            return result;
        }
        private bool Page_IsVisible(string selector)
        {
            bool bResult = false;
            try
            {
                string expression = $"$('{selector}').getBoundingClientRect().top >= 0) && ($('{selector}').getBoundingClientRect().bottom <= window.innerHeight)";
                bResult = Convert.ToBoolean(RunScript(expression));
            }
            catch { }
            return bResult;
        }

        private Rect Page_GetBoundingRect(string selector)
        {
            Rect bResult = new Rect(0, 0, 0, 0);
            try
            {
                string expression = $"JSON.stringify($('{selector}').getBoundingClientRect())";
                bResult = Utils.ParseRect(RunScript(expression));
            }
            catch { }
            return bResult;
        }

        private bool PageClick(string param, int timeout = 500, int nRetry = 3)
        {
#if (TROUBLESHOT)
            //LogMng.Instance.onWriteStatus($"[PageClick] {param}");
#endif
            while (nRetry-- > 0)
            {
                try
                {
                    if (Page_IsVisible(param))
                    {
                        Rect rect = Page_GetBoundingRect(param);

                        Page_MouseClick(rect);
#if (TROUBLESHOT)
                        //page.ClickAsync($"{param}", timeout: timeout).Wait();
            //            LogMng.Instance.onWriteStatus($"[PageClick] {param} clicked");
#endif
                        return true;
                    }
                }
                catch { }
                Thread.Sleep(500);
            }
#if (TROUBLESHOT)
            //LogMng.Instance.onWriteStatus($"[PageClick] {param} failed");
#endif
            return false;
        }

        object evalLocker = new object();
        private string Page_Evaluate(string command, string expression)
        {
            string result = "";
            try
            {
                Monitor.Enter(evalLocker);
                if (Global.socketServer.WebSocketServices.SessionCount <= 0)
                {
#if (TROUBLESHOT)
                //LogMng.Instance.onWriteStatus("WebSocket is not connected");
#endif
                    return string.Empty;
                }
                
#if (TROUBLESHOT)
            //LogMng.Instance.onWriteStatus($"Page_Evaluate Req  cmd : {command} exp: {expression}");
#endif
                wait_EvalResultEvent.Reset();
                wait_EvalResult = string.Empty;
                SendEvalCommand(command, expression);

                if (!wait_EvalResultEvent.Wait(1000))
                {
#if (TROUBLESHOT)
                //LogMng.Instance.onWriteStatus($"Page_Evaluate No Response");
#endif
             
                    return string.Empty;
                }
                result = wait_EvalResult;
#if (TROUBLESHOT)
            //LogMng.Instance.onWriteStatus($"Page_Evaluate Res : {result}");
#endif
            }
            catch
            { }
            finally
            {
                
                Monitor.Exit(evalLocker);
            }
            
            return result;
        }

        private void WaitSpinnerShowing()
        {
            int nWaitCount = 0;
            while (nWaitCount++ < 30)
            {
                string command = "ns_navlib_util.WebsiteNavigationManager.Instance.spinner === null";
                string result = RunScript(command).ToLower();
                if (result.Contains("true"))
                    return;
                Thread.Sleep(300);
            }
        }

        private void Page_Navigate(string subUrl)
        {
            string command = $"var e = {{needsCard:false}}; ns_navlib_util.WebsiteNavigationManager.Instance.navigateTo('{subUrl}', e, false); ";
            RunScript(command);
        }

        private void Page_Goto(string url, bool bNoNeedWait = false)
        {
            if (!bNoNeedWait)
            {
                if (DateTime.Now.Subtract(refreshLastTime).TotalSeconds < 10)
                    return;
            }
            //Page_Evaluate("openpage", url);
            LogMng.Instance.onWriteStatus($"Page_Goto - {url}");
            string command = $"window.open('{url}','_self')";
            RunScript(command);
            refreshLastTime = DateTime.Now;
            Thread.Sleep(1000);
        }

        private void Page_Reload()
        {
            if (DateTime.Now.Subtract(refreshLastTime).TotalSeconds < 10)
                return;
         
            LogMng.Instance.onWriteStatus($"Page_Reload");
            RunScript("location.reload()");
            

            //string url = $"https://www.{Setting.Instance.domain}";
            //string command = $"window.open('{url}','_self')";
            //RunScript(command);

            refreshLastTime = DateTime.Now;
            Thread.Sleep(1000);
        }

        enum SystemMetric
        {
            SM_CXSCREEN = 0,
            SM_CYSCREEN = 1,
        }

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(SystemMetric smIndex);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern short VkKeyScan(char ch);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);
        int CalculateAbsoluteCoordinateX(int x)
        {
            return (x * 65536) / GetSystemMetrics(SystemMetric.SM_CXSCREEN);
        }

        int CalculateAbsoluteCoordinateY(int y)
        {
            return (y * 65536) / GetSystemMetrics(SystemMetric.SM_CYSCREEN);
        }

        private void Page_MouseClick(Rect rect, bool bRelation = true)
        {
            int x = (int)rect.X + (int)(rect.Width / 4) + rnd.Next(0, (int)(rect.Width / 2));
            int y = (int)rect.Y + (int)(rect.Height / 4) + rnd.Next(0, (int)(rect.Height / 2));



            SetWindowTopMost();
            Thread.Sleep(300);


            RECT rct = new RECT();
            GetWindowRect(Global.ViewerHwnd, ref rct);

            //if (y > rct.Bottom - rct.Top - 20)
            //{
            //    return;
            //}
            //try
            //{
            //    int height = Convert.ToInt32(RunScript("window.innerHeight"));
            //    titlebarHeight = rct.Bottom - rct.Top - height;
            //}
            //catch { }
            rct.Left += 8;
            rct.Top += 8;
            INPUT mouseInput = new INPUT();
            mouseInput.type = SendInputEventType.InputMouse;

#if (TROUBLESHOT)
            //LogMng.Instance.onWriteStatus($"Page_MouseClick (view: {x},{y}) (window: {rct.Left}, {rct.Top}) {rct.Left + x}, {rct.Top + titlebarHeight + y}");
#endif
            if (bRelation)
            {
                MoveMouse((rct.Left + x), (rct.Top + titlebarHeight + y));

                mouseInput.mkhi.mi.dx = CalculateAbsoluteCoordinateX(rct.Left + x);
                mouseInput.mkhi.mi.dy = CalculateAbsoluteCoordinateY(rct.Top + titlebarHeight + y);
            }
            else
            {
                mouseInput.mkhi.mi.dx = CalculateAbsoluteCoordinateX(x);
                mouseInput.mkhi.mi.dy = CalculateAbsoluteCoordinateY(y);
            }
            mouseInput.mkhi.mi.mouseData = 0;


            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_MOVE | MouseEventFlags.MOUSEEVENTF_ABSOLUTE;
            var inputs = new INPUT[] { mouseInput };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN | MouseEventFlags.MOUSEEVENTF_MOVE | MouseEventFlags.MOUSEEVENTF_ABSOLUTE;
            inputs = new INPUT[] { mouseInput };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTUP | MouseEventFlags.MOUSEEVENTF_MOVE | MouseEventFlags.MOUSEEVENTF_ABSOLUTE;
            inputs = new INPUT[] { mouseInput };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private void MouseMoveInScreen(int x, int y)
        {
#if !(DEBUG)
            SetWindowTopMost();
            Thread.Sleep(10);
#endif


            INPUT mouseInput = new INPUT();
            mouseInput.type = SendInputEventType.InputMouse;
            mouseInput.mkhi.mi.dx = CalculateAbsoluteCoordinateX(x);
            mouseInput.mkhi.mi.dy = CalculateAbsoluteCoordinateY(y);
            mouseInput.mkhi.mi.mouseData = 0;
            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_MOVE | MouseEventFlags.MOUSEEVENTF_ABSOLUTE;

            var inputs = new INPUT[] { mouseInput };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private void Page_KeyboardType(string sText)
        {
            for (int i = 0; i < 30; i++)
            {
                INPUT[] input = new INPUT[1];
                input[0].type = SendInputEventType.InputKeyboard;
                input[0].mkhi.ki.wVk = 0x2e;    //del
                input[0].mkhi.ki.dwFlags = 0x01;
                SendInput(1u, input, Marshal.SizeOf((object)default(INPUT)));

                input[0].type = SendInputEventType.InputKeyboard;
                input[0].mkhi.ki.wVk = 0x08;    //backspace
                input[0].mkhi.ki.dwFlags = 0x01;
                SendInput(1u, input, Marshal.SizeOf((object)default(INPUT)));
                //Thread.Sleep(rnd.Next(10, 12));
            }

            char[] cText = sText.ToCharArray();
            foreach (char c in cText)
            {
                INPUT[] input = new INPUT[2];
                if (c >= 0 && c < 256)//az AZ
                {
                    short num = VkKeyScan(c);//Get virtual key code value
                    if (num != -1)
                    {
                        bool shift = (num >> 8 & 1) != 0;//num >>8 means the high byte is in the state, if it is 1, press Shift, otherwise, if Shift is not pressed, that is, when the uppercase key CapsLk is not turned on, Whether you need to press Shift.
                        if ((GetKeyState(20) & 1) != 0 && ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')))//Win32API.GetKeyState(20) Get CapsLk uppercase key state
                        {
                            shift = !shift;
                        }
                        if (shift)
                        {
                            input[0].type = SendInputEventType.InputKeyboard;//simulate keyboard
                            input[0].mkhi.ki.wVk = 16;//Shift key
                            input[0].mkhi.ki.dwFlags = 0;//Press
                            SendInput(1u, input, Marshal.SizeOf((object)default(INPUT)));
                        }
                        input[0].type = SendInputEventType.InputKeyboard;
                        input[0].mkhi.ki.wVk = (ushort)(num & 0xFF);
                        input[1].type = SendInputEventType.InputKeyboard;
                        input[1].mkhi.ki.wVk = (ushort)(num & 0xFF);
                        input[1].mkhi.ki.dwFlags = 2;
                        SendInput(2u, input, Marshal.SizeOf((object)default(INPUT)));
                        if (shift)
                        {
                            input[0].type = SendInputEventType.InputKeyboard;
                            input[0].mkhi.ki.wVk = 16;
                            input[0].mkhi.ki.dwFlags = 2;//lift
                            SendInput(1u, input, Marshal.SizeOf((object)default(INPUT)));
                        }
                        Thread.Sleep(rnd.Next(40, 80));
                        if (!Setting.Instance.bPlaceFastMode)
                            Thread.Sleep(rnd.Next(50, 70));

                        continue;
                    }
                }
                input[0].type = SendInputEventType.InputKeyboard;
                input[0].mkhi.ki.wVk = 0;//When dwFlags is KEYEVENTF_UNICODE that is 4, wVk must be 0
                input[0].mkhi.ki.wScan = (ushort)c;
                input[0].mkhi.ki.dwFlags = 4;//Enter UNICODE characters
                input[0].mkhi.ki.time = 0;
                input[0].mkhi.ki.dwExtraInfo = IntPtr.Zero;
                input[1].type = SendInputEventType.InputKeyboard;
                input[1].mkhi.ki.wVk = 0;
                input[1].mkhi.ki.wScan = (ushort)c;
                input[1].mkhi.ki.dwFlags = 6;
                input[1].mkhi.ki.time = 0;
                input[1].mkhi.ki.dwExtraInfo = IntPtr.Zero;
                SendInput(2u, input, Marshal.SizeOf((object)default(INPUT)));
                Thread.Sleep(rnd.Next(40, 80));
                if (!Setting.Instance.bPlaceFastMode)
                     Thread.Sleep(rnd.Next(50, 70));

}

        }
        private string Page_PageSource()
        {
            string result = "";
            return result;
        }
        public string getProxyLocation()
        {
            return "";
            //try
            //{
            //    //Page_Goto("http://lumtest.com/myip.json");
            //    try
            //    {
            //        Page_Goto("http://checkip.dyndns.org/", timeout: 3000);


            //        string content = Page_PageSource().Replace("Current IP Address:", "");
            //        return content;
            //    }
            //    catch { }
            //}
            //catch (Exception ex)
            //{
            //}
            //return "UNKNOWN";
        }

        public HttpClient initHttpClient(bool bUseNewCookie = true)
        {
            return null;
        }

        
        public bool IsPageNotLoadedInSubPage()
        {
            //Message showing
            //Sorry, this page is no longer available. Betting has closed or has been suspended.

            string IsLoginModuleOpen = RunScript("function IsSuspendlabelShowing() { try { return ns_gen5_ui.Application.currentApplication.getElementChildren()[0].innerHTML.includes('sph-BettingSuspendedScreen_Message'); } catch {} return ''; } IsSuspendlabelShowing();").ToLower();
            if (IsLoginModuleOpen == "true")
            {
                return true;
            }
            return false;
        }
        public bool IsSearchIconVisible()
        {
            string findSearchButtonFunc = "function FindSearchButton() { try { for (let i = 0; i < ns_gen5_ui.Application.currentApplication._eRegister.widthChanged.length; i++) { try { let item = ns_gen5_ui.Application.currentApplication._eRegister.widthChanged[i]; if (item.scope == null || item.scope == undefined || item.scope._eRegister == null || item.scope._eRegister == undefined || item.scope._eRegister.widthStateChanged == null || item.scope._eRegister.widthStateChanged == undefined) continue; let subItem = item.scope._eRegister.widthStateChanged; if (subItem.length == 0) continue; if (!subItem[0].scope._element.className == null) continue; if (!subItem[0].scope._element.className.includes('hm-HeaderModule')) continue; var childNodes = subItem[0].scope._element.childNodes; var mainHeaderNode = null; childNodes.forEach(element => { if (element.className != null && element.className.includes('hm-MainHeaderWide')) { mainHeaderNode = element; } }); var searchHeaderNode = null; mainHeaderNode.childNodes.forEach(element => { if (element.className != null && (element.className.includes('hm-MainHeaderRHSLoggedInWide hm-MainHeaderRHSLoggedIn') || element.className.includes('hm-MainHeaderRHSLoggedOutWide'))) { searchHeaderNode = element; } }); var searchBtn = null; searchHeaderNode.childNodes.forEach(element => { if (element.className != null && (element.className.includes('hm-SiteSearchIconLoggedIn') || element.className.includes('hm-SiteSearchIconLoggedOut'))) { searchBtn = element; } }); return JSON.stringify(searchBtn.getBoundingClientRect()); } catch (exx) { return exx; } } } catch (ex) { return ex; } return 'noButton'; } FindSearchButton();";
            string SearchButtonResult = RunScript(findSearchButtonFunc).ToLower();
            Rect SearchButtonRect = Utils.ParseRect(SearchButtonResult);
            if (SearchButtonRect.X <= 0 || SearchButtonRect.Y <= 0 || SearchButtonRect.Width <= 0 || SearchButtonRect.Height <= 0)
            {
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"IsSearchIconVisible false");
#endif
                return false;
            }
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"IsSearchIconVisible true");
#endif
            return true;
        }
        public bool IsLoadedOddsCorrectly()
        {
            //function IsLoadedOddsCorrectly()
            //{
            //    try
            //    {
            //        if (Locator.user._eRegister.oddsChanged == null)
            //            return false;
            //        var eventElemet = null;
            //        for (let i = 0; i < Locator.user._eRegister.oddsChanged.length; i++)
            //        {
            //            let item = Locator.user._eRegister.oddsChanged[i];
            //            if (item == null || item.scope == null) continue;
            //            if (item.scope._active_element.offsetWidth != 0)
            //            {
            //                return true;
            //            }
            //        }
            //    }
            //    catch { }
            //    return false;
            //}
            string result = RunScript("function IsLoadedOddsCorrectly() { try { if (Locator.user._eRegister.oddsChanged == null) return false; var eventElemet = null; for (let i = 0; i < Locator.user._eRegister.oddsChanged.length; i++) { let item = Locator.user._eRegister.oddsChanged[i]; if (item == null || item.scope == null) continue; if (item.scope._active_element.offsetWidth != 0) { return true; } } } catch { } return false; } IsLoadedOddsCorrectly();").ToLower();

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"IsLoadedOddsCorrectly check : {result}");
#endif
            if (result == "false")
                return false;
            return true;
        }

        public bool IsLoadingSpinnerAppeared()
        {
            string result = RunScript("function IsSpinnerShowed() { try { var childArray = ns_gen5_ui.Application.currentApplication.getElementChildren(); for (let i = 0; i < childArray.length; i++) { let item = childArray[i]; if (item.className == 'bl-Preloader') { return true; } } } catch (ex) { return ex; } return false; } IsSpinnerShowed();").ToLower();

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"IsSpinnerShowed check : {result}");
#endif
            if (result == "true")
                return true;
            return false;
        }

        public void RefreshBecauseBet365Notloading()
        {
            int nRetry = 0;
            while (nRetry++ < 30)
            {
                int nSpinnercount = 0;
                Thread.Sleep(300);
                try
                {
                    //string spinnercount = RunScript("document.getElementsByClassName('bl-Preloader_Spinner').length");
                    //nSpinnercount = Convert.ToInt32(spinnercount);
                    string isLoaded = RunScript("function IsConnected() { try { return ns_gen5_ui.Application.currentApplication.connected; } catch {} return ''; } IsConnected();").ToLower();
                    if (isLoaded != "true" || IsLoadingSpinnerAppeared())
                        nSpinnercount++;
                }
                catch (Exception ex)
                {
                }

                if (nSpinnercount > 0)
                {
                    if (nRetry == 15)
                    {
                        LogMng.Instance.onWriteStatus("[RefreshBecauseBet365Notloading] reload page from Spinner");
                        Page_Reload();
                    }
                    Thread.Sleep(300);
                    continue;
                }

                if (IsLoadedOddsCorrectly() /*&& IsSearchIconVisible()*/)
                {
                    LogMng.Instance.onWriteStatus("[RefreshBecauseBet365Notloading] loading finished");
                    return;
                }

                if (nRetry % 14 == 0)
                {
                    LogMng.Instance.onWriteStatus("[RefreshBecauseBet365Notloading] reload page from timeout");
                    Page_Reload();
                }
            }
        }



        private string RunScript(string param)
        {
#if (TROUBLESHOT)
            //LogMng.Instance.onWriteStatus($"RunScript: {param}");
#endif
            string result = "";
            try
            {
                result = Page_Evaluate("script", param);
            }
            catch { }
            return result;
        }

        private void WaitUntilWebSocketConnect()
        {
            int nRetry = 0;
            while (nRetry++ < 100)
            {
                if (Global.socketServer.WebSocketServices.SessionCount > 0)
                    break;
                //if (bWebSocketConnected)
                //    break;
                Thread.Sleep(500);
            }
        }
        private void GetTitlebarHeight()
        {
#if (CHROME)
            //Page_Evaluate("titlebar", "");
#else
//            try
//            {
//                string offsetx = RunScript("mozInnerScreenY");

//#if (TROUBLESHOT)
//                LogMng.Instance.onWriteStatus($"mozInnerScreenY res: {offsetx}");
//#endif
//                Bet365_ADDONCtrl.titlebarHeight = Convert.ToInt32(offsetx);
//            }
//            catch { }
#endif
        }

        public bool logout()
        {
            //string functionString = "window.localStorage.clear();window.sessionStorage.clear();";
            //RunScript(functionString);

            int nButtonRetry = 0;
            while (nButtonRetry++ < 3)
            {
                Rect logoutRect = GetDomRectFromClass("um-GeneralTab_LogoutOption ");

                if (logoutRect.X > 0 && logoutRect.Y > 0 && logoutRect.Width > 0 && logoutRect.Height > 0)
                {
                    Page_MouseClick(logoutRect);
                    Thread.Sleep(1000);
                    return true;
                }
                else
                {
                    Rect memberIconRect = GetDomRectFromClass("hm-MainHeaderMembersWide_MembersMenuIcon ");

                    if (memberIconRect.X <= 0 || memberIconRect.Y <= 0 || memberIconRect.Width <= 0 || memberIconRect.Height <= 0)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    else
                    {
                        Page_MouseClick(memberIconRect);
                        Thread.Sleep(1000);
                    }
                }
            }

            return false;
        }

        Rect GetDomRectFromClass(string className)
        {
            //function getMemberIconRect(classLabel)
            //{
            //    var domArray = [];
            //    domArray.push(ns_gen5_ui.Application.currentApplication.getElementChildren());

            //    console.log('init count: ' + domArray.length);
            //    var counter = 0;
            //    while (domArray.length > 0)
            //    {
            //        counter++;
            //        console.log('recursive counter: ' + counter);

            //        try
            //        {
            //            var curIterator = domArray.shift();

            //            for (var i = 0; i < curIterator.length; i++)
            //            {
            //                try
            //                {
            //                    if (curIterator[i].className.includes(classLabel))
            //                    {
            //                        console.log(curIterator[i]);
            //                        return curIterator[i].getBoundingClientRect();
            //                    }
            //                    domArray.push(curIterator[i].childNodes);
            //                }
            //                catch (ex1)
            //                {
            //                    console.log('ex1: ' + ex1);
            //                }
            //            }
            //        }
            //        catch (ex)
            //        {
            //            console.log('ex: ' + ex);
            //        }
            //    }
            //    return '';
            //}
            //getMemberIconRect('hm-MainHeaderMembersWide_MembersMenuIcon ');

            Rect loginbtnRect = Utils.ParseRect(RunScript($"function getMemberIconRect(classLabel) {{ var domArray = []; domArray.push(ns_gen5_ui.Application.currentApplication.getElementChildren()); var counter = 0; while (domArray.length > 0) {{ counter++; try {{ var curIterator = domArray.shift(); for (var i = 0; i < curIterator.length; i++) {{ try {{ if (curIterator[i].className.includes(classLabel)) {{ return curIterator[i].getBoundingClientRect(); }} domArray.push(curIterator[i].childNodes); }} catch (ex1) {{ }} }} }} catch (ex) {{ }} }} return ''; }} getMemberIconRect('{className}');"));

            return loginbtnRect;
        }
        public bool login()
        {         
            try
            {
                Monitor.Enter(locker);

                WaitUntilWebSocketConnect();
                CatchBrowser();
                //ShowWindow(Global.ViewerHwnd, SW_MAXIMIZE);
                //Page_Evaluate("maximize", "");
                GetTitlebarHeight();

                try
                {
                    if (IsPageLoginStatus())
                    {
                        AccountCountryCode = RunScript("Locator.user.countryCode");
                        //CloseBetSlip();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    LogMng.Instance.onWriteStatus($"CheckLogin Exception: {ex}");
                }


                //Page_Evaluate("domain", Setting.Instance.domain);


                LogMng.Instance.onWriteStatus("Login start");
                Page_Goto($"https://www.{Setting.Instance.domain}");

                int nTotalRetry = 0;
                while (nTotalRetry++ < 3)
                {
                    if (!Global.bRun)
                        return false;
                    try
                    {//return Locator.user.isLoggedIn;

                        RefreshBecauseBet365Notloading();

                        if (IsPageLoginStatus())
                        {
                            AccountCountryCode = RunScript("Locator.user.countryCode");
                            return true;
                        }

                        //
                        //function getLoginButtonClass()
                        //{
                        //    var loginDom = null;
                        //    try
                        //    {
                        //        loginDom = ns_gen5_ui.Application.currentApplication.getElementChildren()[0].childNodes[0].childNodes[2].childNodes[0].childNodes[0].childNodes[1].childNodes[3].childNodes[2];
                        //    }
                        //    catch { }
                        //    try
                        //    {
                        //        loginDom = ns_gen5_ui.Application.currentApplication.getElementChildren()[0].childNodes[0].childNodes[2].childNodes[0].childNodes[0].childNodes[5].childNodes[3].childNodes[2];
                        //    }
                        //    catch { }

                        //    if (loginDom != null)
                        //        return loginDom.outerHTML;
                        //    return "";
                        //}
                        //getLoginButtonClass();

                        Rect loginbtnRect = GetDomRectFromClass("hm-MainHeaderRHSLoggedOutWide_Login ");
                       

#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Login button status : {loginbtnRect.X}, {loginbtnRect.Y} - {loginbtnRect.Width}, {loginbtnRect.Height}");
#endif

                        if (loginbtnRect.X <= 0 || loginbtnRect.Y <= 0 || loginbtnRect.Width <= 0 || loginbtnRect.Height <= 0)
                        {
                            LogMng.Instance.onWriteStatus("No Login button, Page_Goto again");
                            Page_Goto($"https://www.{Setting.Instance.domain}");

                            //check if page is loaded all
                            int nRetry1 = 0;
                            while (nRetry1 < 30)
                            {
                                if (!Global.bRun)
                                    return false;

                                Thread.Sleep(500);
                                loginbtnRect = GetDomRectFromClass("hm-MainHeaderRHSLoggedOutWide_Login ");

#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus($"Login button status_1 : {loginbtnRect.X}, {loginbtnRect.Y} - {loginbtnRect.Width}, {loginbtnRect.Height}");
#endif

                                if (loginbtnRect.X > 0 && loginbtnRect.Y > 0 && loginbtnRect.Width > 0 && loginbtnRect.Height > 0)
                                {
                                    break;
                                }
                                nRetry1++;
                            }
                            if (nRetry1 >= 30)       //Page is loading gray page. let's retry
                            {
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus($"Let's retry because of loading gray page");
#endif

                                continue;
                            }
                        }

                        CloseAcceptCookie();

                        int nButtonRetry = 0;
                        while (nButtonRetry++ < 3)
                        {
                            loginbtnRect = GetDomRectFromClass("hm-MainHeaderRHSLoggedOutWide_Login ");

                            if (loginbtnRect.X <= 0 || loginbtnRect.Y <= 0 || loginbtnRect.Width <= 0 || loginbtnRect.Height <= 0)
                            {
                                Thread.Sleep(1000);
                                continue;
                            }
                            else
                            {
                                Page_MouseClick(loginbtnRect);
                                Thread.Sleep(500);
                            }


                            Rect chkinputUsernameRect = Utils.ParseRect(RunScript("function getLoginElementRect(param) {    try {        var nodeList = ns_gen5_util.Singleton.getInstance(ns_loginlib_utils.AlternativeAuthentication).standardLogin.loginPopup.getModule().getElementChildren()[0].childNodes[2].childNodes[0].childNodes;        for (let i = 0; i < nodeList.length; i++) {            try{                if (nodeList[i].className == null) continue;                if (nodeList[i].className.includes('InputsContainer')) {                    if (param == 'username') {                        if (!nodeList[i].className.includes('password')) return nodeList[i].getBoundingClientRect();                    } else if (param == 'password') {                        if (nodeList[i].className.includes('password')) return nodeList[i].getBoundingClientRect();                    }                } else if (nodeList[i].className.includes('LoginButton')) {                    if (param == 'login') {                        return nodeList[i].getBoundingClientRect();                    }                }            }            catch {}        }    } catch (ex)    {         return ex;    }    return 'noelement';}getLoginElementRect('username');"));

                            if (chkinputUsernameRect.X > 0 && chkinputUsernameRect.Y > 0 && chkinputUsernameRect.Width > 0 && chkinputUsernameRect.Height > 0)
                            {
                                break;
                            }
                        }
                        if (nButtonRetry == 3)
                        {

                            LogMng.Instance.onWriteStatus("Clicking Login button not working. retry from scratch again.");
                            continue;
                        }
                        Thread.Sleep(500);

                        string IsLoginModuleOpen = RunScript("function isloginModuleOpen(){return ns_gen5_util.Singleton.getInstance(ns_loginlib_utils.AlternativeAuthentication).standardLogin.loginDisplaying;} isloginModuleOpen();").ToLower();
                        if (IsLoginModuleOpen == "false")
                        {//if login dialog is not appeared
                            continue;
                        }

                        Rect inputUsernameRect = Utils.ParseRect(RunScript("function getLoginElementRect(param) {    try {        var nodeList = ns_gen5_util.Singleton.getInstance(ns_loginlib_utils.AlternativeAuthentication).standardLogin.loginPopup.getModule().getElementChildren()[0].childNodes[2].childNodes[0].childNodes;        for (let i = 0; i < nodeList.length; i++) {            try{                if (nodeList[i].className == null) continue;                if (nodeList[i].className.includes('InputsContainer')) {                    if (param == 'username') {                        if (!nodeList[i].className.includes('password')) return nodeList[i].getBoundingClientRect();                    } else if (param == 'password') {                        if (nodeList[i].className.includes('password')) return nodeList[i].getBoundingClientRect();                    }                } else if (nodeList[i].className.includes('LoginButton')) {                    if (param == 'login') {                        return nodeList[i].getBoundingClientRect();                    }                }            }            catch {}        }    } catch (ex)    {         return ex;    }    return 'noelement';}getLoginElementRect('username');"));

                        if (inputUsernameRect.X <= 0 || inputUsernameRect.Y <= 0 || inputUsernameRect.Width <= 0 || inputUsernameRect.Height <= 0)
                        {
                            continue;
                        }
                        else
                        {
                            Page_MouseClick(inputUsernameRect);
                            Thread.Sleep(200);
                            
                        }
                        //PageClick("input.lms-StandardLogin_Username");                                                
                        Page_KeyboardType(Setting.Instance.username);
                        Thread.Sleep(500);

                        Rect inputPasswordRect = Utils.ParseRect(RunScript("function getLoginElementRect(param) {    try {        var nodeList = ns_gen5_util.Singleton.getInstance(ns_loginlib_utils.AlternativeAuthentication).standardLogin.loginPopup.getModule().getElementChildren()[0].childNodes[2].childNodes[0].childNodes;        for (let i = 0; i < nodeList.length; i++) {            try{                if (nodeList[i].className == null) continue;                if (nodeList[i].className.includes('InputsContainer')) {                    if (param == 'username') {                        if (!nodeList[i].className.includes('password')) return nodeList[i].getBoundingClientRect();                    } else if (param == 'password') {                        if (nodeList[i].className.includes('password')) return nodeList[i].getBoundingClientRect();                    }                } else if (nodeList[i].className.includes('LoginButton')) {                    if (param == 'login') {                        return nodeList[i].getBoundingClientRect();                    }                }            }            catch {}        }    } catch (ex)    {         return ex;    }    return 'noelement';}getLoginElementRect('password');"));

                        if (inputPasswordRect.X <= 0 || inputPasswordRect.Y <= 0 || inputPasswordRect.Width <= 0 || inputPasswordRect.Height <= 0)
                        {
                            continue;
                        }
                        else
                        {
                            Page_MouseClick(inputPasswordRect);
                            Thread.Sleep(200);

                        }
                        //PageClick("input.lms-StandardLogin_Password");                        
                        Page_KeyboardType(Setting.Instance.password);
                        Thread.Sleep(500);


                        Rect buttonLoginRect = Utils.ParseRect(RunScript("function getLoginElementRect(param) {    try {        var nodeList = ns_gen5_util.Singleton.getInstance(ns_loginlib_utils.AlternativeAuthentication).standardLogin.loginPopup.getModule().getElementChildren()[0].childNodes[2].childNodes[0].childNodes;        for (let i = 0; i < nodeList.length; i++) {            try{                if (nodeList[i].className == null) continue;                if (nodeList[i].className.includes('InputsContainer')) {                    if (param == 'username') {                        if (!nodeList[i].className.includes('password')) return nodeList[i].getBoundingClientRect();                    } else if (param == 'password') {                        if (nodeList[i].className.includes('password')) return nodeList[i].getBoundingClientRect();                    }                } else if (nodeList[i].className.includes('LoginButton')) {                    if (param == 'login') {                        return nodeList[i].getBoundingClientRect();                    }                }            }            catch {}        }    } catch (ex)    {         return ex;    }    return 'noelement';}getLoginElementRect('login');"));

                        if (buttonLoginRect.X <= 0 || buttonLoginRect.Y <= 0 || buttonLoginRect.Width <= 0 || buttonLoginRect.Height <= 0)
                        {
                            continue;
                        }
                        else
                        {
                            Page_MouseClick(buttonLoginRect);
                            Thread.Sleep(200);

                        }
                        
                        //try
                        //{
                        //    PageClick("div.lms-LoginButton");                            
                        //}
                        //catch { }


                        int nRetry = 0;
                        while (nRetry < 3)
                        {
                            if (!Global.bRun)
                                return false;

                            Thread.Sleep(5000);
                            nRetry++;

                            if (IsPageLoginStatus())
                            {
                                AccountCountryCode = RunScript("Locator.user.countryCode");
                                LogMng.Instance.onWriteStatus($"Login Successed [{Setting.Instance.username}]");
                                int nClosePopupRetry = 0;
                                while (nClosePopupRetry++ < 4)
                                {
                                    if (!ClosePopupMessage())
                                        break;
                                    Thread.Sleep(1000);
                                }
                                CloseBetSlip();

                                Page_Navigate("#MB#");
                                WaitSpinnerShowing();                                
                                //Rect logoutRect = GetDomRectFromClass("hm-HeaderMenuItemMyBets ");
                                //if (logoutRect.X > 0 && logoutRect.Y > 0 && logoutRect.Width > 0 && logoutRect.Height > 0)
                                //{
                                //    Page_MouseClick(logoutRect);
                                //}

                                return true;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"login exception: {ex}");
#endif
                    }
                }
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus("Login Failed");
#endif
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"Exception : {ex.Message} {ex.StackTrace}");
            }
            finally
            {
                Monitor.Exit(locker);
            }
            UserMng.GetInstance().stopEvent();
            return false;
        }

        private bool IsPageLoginStatus()
        {
            string result = RunScript("function IsLoggedIn(){return Locator.user.isLoggedIn;} IsLoggedIn();").ToLower();
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"LonginStatus(1): {result}");
#endif
            if (result == "true")
            {
                return true;
            }
            return false;
        }
        
        private bool CloseAcceptCookie()
        {
            //function getAcceptCookieRect()
            //{
            //    var nodeList = ns_gen5_ui.Application.currentApplication.getElementChildren();
            //    try
            //    {
            //        for (let i = 0; i < nodeList.length; i++)
            //        {
            //            if (nodeList[i].className.includes("ccm-CookieConsentPopup"))
            //            {
            //                return nodeList[i].childNodes[0].childNodes[1].childNodes[1].getBoundingClientRect();
            //            }
            //        }
            //    }
            //    catch { }

            //    return 'nobutton';
            //}
            //getAcceptCookieRect();

            string function1 = $"function getAcceptCookieRect() {{ var nodeList = ns_gen5_ui.Application.currentApplication.getElementChildren(); try {{ for (let i = 0; i < nodeList.length; i++) {{ if (nodeList[i].className.includes('ccm-CookieConsentPopup')) {{ return nodeList[i].childNodes[0].childNodes[1].childNodes[1].getBoundingClientRect(); }} }} }} catch {{ }} return 'nobutton'; }} getAcceptCookieRect();";

            string getCookieAcceptButResult = RunScript(function1).ToLower();

            if (getCookieAcceptButResult == "nobutton")
            {
                return false;
            }
            else
            {
                Rect acceptbuttonRect = Utils.ParseRect(getCookieAcceptButResult);

                if (acceptbuttonRect.X <= 0 || acceptbuttonRect.Y <= 0 || acceptbuttonRect.Width <= 0 || acceptbuttonRect.Height <= 0)
                {

                }
                else
                {
                    Page_MouseClick(acceptbuttonRect);
                    Thread.Sleep(200);
                    return true;
                }
            }
            return false;
        }
        //true： closed any popup message
        private bool ClosePopupMessage()
        {
            try
            {
                Monitor.Enter(locker);
             
                //clicking iframe button
                string function1 = $"function getIframeUrl(){{try{{var lastChild = ns_webconsolelib_util.ModalManager.Root._element.childNodes[ns_webconsolelib_util.ModalManager.Root._element.childNodes.length - 1];if (lastChild.childNodes.length > 0 && lastChild.childNodes[0].tagName.toLowerCase().includes('iframe')){{return lastChild.childNodes[0].src;}}}}catch {{ }}return 'noiframe';}} getIframeUrl();";

                string iframeUrl = RunScript(function1).ToLower();

                if (!string.IsNullOrEmpty(iframeUrl) && !iframeUrl.Contains("noiframe") && Utils.IsValidUrl(iframeUrl))
                {
                    Rect closebuttonRect = new Rect(0, 0, 0, 0);
                    string origUrl = RunScript("function getUrl(){return document.URL;} getUrl();");
                    //#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"There's iFrame, let's go iframe: {iframeUrl} origUrl: {origUrl}");
                    //#endif
                    if (Uri.IsWellFormedUriString(iframeUrl, UriKind.Absolute))
                    {
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus("Open iframeURL");
#endif
                        Page_Goto(iframeUrl);
                        Thread.Sleep(5000);
                        int nRetry = 0;
                        while (nRetry++ < 5)
                        {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Clicking RemindMeLater Retry {nRetry}");
#endif

                            string function2 = "function closeButtonRect() { var buttons = [['id', 'RemindMeLater'],['class' , 'nui-Button']]; for (let k = 0; k < buttons.length; k++) { try{ if (buttons[k][0] == 'id') { return JSON.stringify(document.getElementById(buttons[k][1]).getBoundingClientRect()); } else if(buttons[k][0] == 'class') { return JSON.stringify(document.getElementsByClassName(buttons[k][1])[0].getBoundingClientRect()); } } catch{} } return 'noButton'; } closeButtonRect();";
                            string closeBox = RunScript(function2).ToLower();
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Clicking RemindMeLater result: {closeBox}");
#endif
                            closebuttonRect = Utils.ParseRect(closeBox);

                            if (closebuttonRect.X <= 0 || closebuttonRect.Y <= 0 || closebuttonRect.Width <= 0 || closebuttonRect.Height <= 0)
                            {
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"Clicking RemindMeLater no button box");
#endif
                            }
                            else
                            {
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"get closing button success");
#endif
                                break;
                            }
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Clicking RemindMeLater retry again");
#endif
                            Thread.Sleep(600);
                        }
                    }

#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"let's go to original Url : {origUrl}");
#endif
                    Page_Goto(origUrl, true);
                    Thread.Sleep(3000);

                    if (closebuttonRect.X <= 0 || closebuttonRect.Y <= 0 || closebuttonRect.Width <= 0 || closebuttonRect.Height <= 0)
                    {
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"no closing button pos");
#endif
                    }
                    else
                    {
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"clicking close button in orig page");
#endif
                        Page_MouseClick(closebuttonRect);
                        Thread.Sleep(1000);
                    }
                }
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"no iFrame let's continue");
#endif

                function1 = "function GetDialogRect() { var buttons = [ ['div', 'pm-FreeBetsPushGraphicCloseButton'], ['div', 'gco-RealityCheckAcknowledgeButton'], ['div', 'bil-BetslipPushBetDialog_OkayButton'], ['div', 'alm-ActivityLimitStayButton'], ['div', 'llm-LastLoginModule_Button'], ['div', 'pm-MessageOverlayCloseButton'], ['div', 'lqb-QuickBetHeader_DoneButton'], ['div', 'alm-InactivityAlertRemainButton'], ['div', 'pm-FreeBetsPushGraphicCloseButton'], ['button', 'KeepCurrentLimitsButton'], ['button', 'btn-keep-current-setting'], ['div', 'bss-ReceiptContent_Done'], ['div', 'bsm-LocationErrorMessage_Close'], ['div', 'bs-PlaceBetErrorMessage_Remove'] ]; var domArray = []; domArray.push(ns_gen5_ui.Application.currentApplication.getElementChildren()); while (domArray.length > 0) { try { var curIterator = domArray.shift(); for (var i = 0; i < curIterator.length; i++) { try { for (let k = 0; k < buttons.length; k++) { if (curIterator[i].tagName.toLowerCase().includes(buttons[k][0]) && curIterator[i].className.includes(buttons[k][1])) { return curIterator[i].getBoundingClientRect(); } } domArray.push(curIterator[i].childNodes); } catch (ex1) { } } } catch (ex) { } } return 'nodialog'; } GetDialogRect();";

                string getDialogClosebutResult = RunScript(function1).ToLower();

                if (getDialogClosebutResult == "exception" || getDialogClosebutResult == "nodialog")
                {
                    return false;
                }
                else
                {
                    Rect closebuttonRect = Utils.ParseRect(getDialogClosebutResult);

                    if (closebuttonRect.X <= 0 || closebuttonRect.Y <= 0 || closebuttonRect.Width <= 0 || closebuttonRect.Height <= 0)
                    {

                    }
                    else
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"clicking close button in popup dialog");
#endif
                        Page_MouseClick(closebuttonRect);
                        Thread.Sleep(200);
                        return true;
                    }
                }



            }
            catch
            { }
            finally
            {
                
                Monitor.Exit(locker);
            }
            return false;
        }
        private void CloseBetSlipInner(string fi)
        {
            int nRetry = 0;
            while (nRetry++ < 3)
            {
                string betslipCount = RunScript("function getSlipCount(){ return BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets.length;} getSlipCount();");
                int nBetsInSlip = 0;

                if (int.TryParse(betslipCount, out nBetsInSlip))
                {
                    if (nBetsInSlip > 0)
                    {
                        string checkExpandStatus = RunScript("function getSlipClass(){return BetSlipLocator.betSlipManager.betslip.activeModule.slip._element.className;} getSlipClass();");
                        if (!checkExpandStatus.Contains("_Expanded"))
                        {
                            Rect expandRect = GetDomRectFromClass("bss-OtherMultiplesButton "); // for double bets

                            if (expandRect.X > 0 && expandRect.Y > 0 && expandRect.Width > 0 && expandRect.Height > 0)
                            {

                            }
                            else
                            {
                                expandRect = GetDomRectFromClass("bss-DefaultContent_Close "); //for more bets
                            }

                            if (expandRect.X > 0 && expandRect.Y > 0 && expandRect.Width > 0 && expandRect.Height > 0)
                            {
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"clicking close button in betslip Inner");
#endif
                                Page_MouseClick(expandRect);
                                Thread.Sleep(500);
                            }
                            else
                            {
                                LogMng.Instance.onWriteStatus($"No expand button of betslip");                                
                                //return;
                            }
                        }

                        string getGetBetSlipCloseRect = "function GetBetSlipCloseRect(fi) { try { for (let i = 0; i < BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets.length; i++) { try { let item = BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets[i]; if (item == null || item.model == null || item.model.bet == null || item.model.bet.data == null || item.model.bet.data.tp == null) continue; if (item.model.bet.data.tp.includes(fi)) return item.removeButton._active_element.getBoundingClientRect(); } catch {} } } catch { return 'exception'; } return 'noresult'; } ";
                        getGetBetSlipCloseRect += $"GetBetSlipCloseRect('{fi}');";

                        string betslipcloseRectRes = RunScript(getGetBetSlipCloseRect);
                        LogMng.Instance.onWriteStatus($"betslipcloseRectRes : {betslipcloseRectRes}");
                        Rect HierachyDOMRect = Utils.ParseRect(betslipcloseRectRes);
                        if (HierachyDOMRect.X > 0 && HierachyDOMRect.Y > 0 && HierachyDOMRect.Width > 0 && HierachyDOMRect.Height > 0)
                        {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"clicking close button in betslip Inner 1");
#endif
                            Page_MouseClick(HierachyDOMRect);
                            Thread.Sleep(500);
                        }
                    }
                }
            }
        }
        private void CloseBetSlip()
        {
            if (!IsPageLoginStatus())
                return;

            RunScript("function removebetstring(){window.sessionStorage.setItem('betstring', ''); return 'success';} removebetstring();");
            //Removing all betslip stubs
            int nRetryCount = 0;
            while (nRetryCount++ < 3)
            {
                string betslipCount = RunScript("function getSlipCount(){return BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets.length;} getSlipCount();");
                int nBetsInSlip = 0;

                if (int.TryParse(betslipCount, out nBetsInSlip))
                {
                    if (nBetsInSlip > 0)
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Stub Bets in Slip count: {nBetsInSlip}");
#endif
                        
                        string removeButtonLocation = RunScript("function getRemoveButtonPos(){try{return BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets[0].removeButton.getActiveElement().getBoundingClientRect();}catch{} return '';} getRemoveButtonPos();").ToLower();
                        Rect ResultRemoveBoxRect = Utils.ParseRect(removeButtonLocation);
                        if (ResultRemoveBoxRect.X > 0 && ResultRemoveBoxRect.Y > 0 && ResultRemoveBoxRect.Width > 0 && ResultRemoveBoxRect.Height > 0)
                        {
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"clicking close button in betslip ");
#endif
                            Page_MouseClick(ResultRemoveBoxRect);
                            Thread.Sleep(500);
                        }
                        else
                        {
                            //betslip is minimized, we have to restore it first by clicking header 
                            string slipheaderLocation = RunScript("function getRestoreButtonPos(){try{return BetSlipLocator.betSlipManager.betslip.activeModule.slip.header.getActiveElement().getBoundingClientRect();}catch{} return '';} getRestoreButtonPos();").ToLower();
                            Rect slipheaderRect = Utils.ParseRect(slipheaderLocation);
                            if (slipheaderRect.X > 0 && slipheaderRect.Y > 0 && slipheaderRect.Width > 0 && slipheaderRect.Height > 0)
                            {
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus($"clicking close button in betslip 1");
#endif
                                Page_MouseClick(slipheaderRect);
                                Thread.Sleep(500);
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    Thread.Sleep(300);
                }
            }

            string singlebetslipValue = RunScript("function getSinglebetSlipPos(){try{return BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.indicationArea.getActiveElement().childNodes[1].getBoundingClientRect();}catch{} return '';} getSinglebetSlipPos();").ToLower();
            Rect ResultSinglebetRemoveBoxRect = Utils.ParseRect(singlebetslipValue);
            if (ResultSinglebetRemoveBoxRect.X > 0 && ResultSinglebetRemoveBoxRect.Y > 0 && ResultSinglebetRemoveBoxRect.Width > 0 && ResultSinglebetRemoveBoxRect.Height > 0)
            {
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"clicking close button in betslip 2");
#endif
                Page_MouseClick(ResultSinglebetRemoveBoxRect);
                Thread.Sleep(500);
            }
            ///

            //closing multi betslips (it doesn't closed by function)  find RemoveAll button and click
            
            
//            string removeAllButtonLocation = RunScript("function getRemoveButtonPos(){var buttonClass = BetSlipLocator.betSlipManager.betslip.activeModule.slip.getActiveElement().children[1].children[0].className;if (buttonClass.includes('RemoveButton') || buttonClass.includes('ErrorMessage_Remove'))return BetSlipLocator.betSlipManager.betslip.activeModule.slip.getActiveElement().children[1].children[0].getBoundingClientRect();return 'exception';}getRemoveButtonPos();").ToLower();
//            Rect ResultBoxRect = Utils.ParseRectFromJson(removeAllButtonLocation);
//            if (ResultBoxRect.X > 0 && ResultBoxRect.Y > 0 && ResultBoxRect.Width > 0 && ResultBoxRect.Height > 0)
//            {
//#if (TROUBLESHOT)
//                LogMng.Instance.onWriteStatus($"clicking close button in betslip 3");
//#endif
//                Page_MouseClick(ResultBoxRect);
//                Thread.Sleep(500);                
//            }

            //RunScript("function closeBetSlip() { BetSlipLocator.betSlipManager.betslip.activeModule.slip.controlBar.delegate.controlBarRemoveAllClicked(); return 'success'; } closeBetSlip();");
        }

        private bool IsThisPageIsNoLongerAvailable()
        {
            Rect NoLongerAvailableDivRect = GetDomRectFromClass("sph-BettingSuspendedScreen ");


#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"NoLongerAvailableDivRect status : {NoLongerAvailableDivRect.X}, {NoLongerAvailableDivRect.Y} - {NoLongerAvailableDivRect.Width}, {NoLongerAvailableDivRect.Height}");
#endif

            if (NoLongerAvailableDivRect.X > 0 || NoLongerAvailableDivRect.Y > 0 || NoLongerAvailableDivRect.Width > 0 || NoLongerAvailableDivRect.Height > 0)
            {
                return true;
            }
            return false;
        }
        private bool IsScrollTopmost()
        {
            //function IsScrollTopmost(classLabel)
            //{
            //    var domArray = [];
            //    domArray.push(ns_gen5_ui.Application.currentApplication.getElementChildren());

            //    console.log('init count: ' + domArray.length);
            //    var counter = 0;
            //    while (domArray.length > 0)
            //    {
            //        counter++;
            //        console.log('recursive counter: ' + counter);

            //        try
            //        {
            //            var curIterator = domArray.shift();

            //            for (var i = 0; i < curIterator.length; i++)
            //            {
            //                try
            //                {
            //                    if (curIterator[i].className.includes(classLabel))
            //                    {
            //                        console.log(curIterator[i].scrollTop);
            //                        if (curIterator[i].scrollTop < 50)
            //                            return 'true';
            //                        else
            //                            return 'false';
            //                    }
            //                    domArray.push(curIterator[i].childNodes);
            //                }
            //                catch (ex1)
            //                {
            //                    console.log('ex1: ' + ex1);
            //                }
            //            }
            //        }
            //        catch (ex)
            //        {
            //            console.log('ex: ' + ex);
            //        }
            //    }
            //    return '';
            //}
            //IsScrollTopmost('ipe-EventViewDetailScroller');

            string status = RunScript("function IsScrollTopmost(classLabel) { var domArray = []; domArray.push(ns_gen5_ui.Application.currentApplication.getElementChildren()); var counter = 0; while (domArray.length > 0) { counter++; try { var curIterator = domArray.shift(); for (var i = 0; i < curIterator.length; i++) { try { if (curIterator[i].className.includes(classLabel)) { if (curIterator[i].scrollTop < 50) return 'true'; else return 'false'; } domArray.push(curIterator[i].childNodes); } catch (ex1) { } } } catch (ex) { } } return ''; } IsScrollTopmost('ipe-EventViewDetailScroller');");
            if (status == "true")
                return true;
            return false;
        }

        private bool IsScrollDownmost()
        {
            //function IsScrollDownmost(classLabel)
            //{
            //    var domArray = [];
            //    domArray.push(ns_gen5_ui.Application.currentApplication.getElementChildren());

            //    var counter = 0;
            //    while (domArray.length > 0)
            //    {
            //        counter++;

            //        try
            //        {
            //            var curIterator = domArray.shift();

            //            for (var i = 0; i < curIterator.length; i++)
            //            {
            //                try
            //                {
            //                    if (curIterator[i].className.includes(classLabel))
            //                    {
            //                        if (curIterator[i].scrollTop + curIterator[i].clientHeight + 100 >= curIterator[i].scrollHeight)
            //                            return 'true';
            //                        else
            //                            return 'false';
            //                    }
            //                    domArray.push(curIterator[i].childNodes);
            //                }
            //                catch (ex1)
            //                {
            //                }
            //            }
            //        }
            //        catch (ex)
            //        {
            //        }
            //    }
            //    return '';
            //}
            //IsScrollDownmost('ipe-EventViewDetailScroller');

            string status = RunScript("function IsScrollDownmost(classLabel) { var domArray = []; domArray.push(ns_gen5_ui.Application.currentApplication.getElementChildren()); var counter = 0; while (domArray.length > 0) { counter++; try { var curIterator = domArray.shift(); for (var i = 0; i < curIterator.length; i++) { try { if (curIterator[i].className.includes(classLabel)) { if (curIterator[i].scrollTop + curIterator[i].clientHeight + 100 >= curIterator[i].scrollHeight) return 'true'; else return 'false'; } domArray.push(curIterator[i].childNodes); } catch (ex1) { } } } catch (ex) { } } return ''; } IsScrollDownmost('wcl-PageContainer-scrollable ');");
            if (status == "true")
                return true;
            return false;
        }

        private string UpdateHandicap(string handicaplabel, double offset)
        {
            //handicap format
            //1.5
            //O 1.5
            //21+

            //1.5
            MatchCollection mc = Regex.Matches(handicaplabel, "^(?<value>(-?|\\+?)[\\d.]+)$");
            if (mc.Count == 1)
            {
                if (mc[0].Groups["value"].Value != "")
                {
                    double handicap = Utils.ParseToDouble(mc[0].Groups["value"].Value);
                    return $"{handicap + offset}";
                }
            }

            //O 1.5
            mc = Regex.Matches(handicaplabel, "^(?<side>O|U) (?<value>(-?|\\+?)[\\d.]+)$");
            if (mc.Count == 1)
            {
                if (mc[0].Groups["value"].Value != "" && mc[0].Groups["side"].Value != "")
                {
                    double handicap = Utils.ParseToDouble(mc[0].Groups["value"].Value);
                    return $"{mc[0].Groups["side"].Value} {handicap + offset}";
                }
            }

            //21+
            mc = Regex.Matches(handicaplabel, "^(?<value>(-?|\\+?)[\\d.]+)\\+$");
            if (mc.Count == 1)
            {
                if (mc[0].Groups["value"].Value != "")
                {
                    double handicap = Utils.ParseToDouble(mc[0].Groups["value"].Value);
                    return $"{handicap + offset}+";
                }
            }
            return string.Empty;
        }

        private string GetCurrentPageEventTitle()
        {
            //Getting match name
    #region GetMatchName
            //function get_allElementsFromClass(classLabel)
            //{
            //    var resultArray = [];
            //    var domArray = [];
            //    domArray.push(ns_gen5_ui.Application.currentApplication.getElementChildren());
            //    while (domArray.length > 0)
            //    {
            //        try
            //        {
            //            var curIterator = domArray.shift();
            //            for (var i = 0; i < curIterator.length; i++)
            //            {
            //                try
            //                {
            //                    if (curIterator[i].className.includes(classLabel))
            //                    {
            //                        resultArray.push(curIterator[i]);
            //                    }
            //                    domArray.push(curIterator[i].childNodes);
            //                }
            //                catch (ex1) { }
            //            }
            //        }
            //        catch (ex) { }
            //    }
            //    return resultArray;
            //}


            //function get_MatchName()
            //{
            //    var list = get_allElementsFromClass('sph-EventHeader_Label ');
            //    if (list.length > 0)
            //    {
            //        for (var i = 0; i < list.length; i++)
            //        {
            //            if (list[i].outerText != '')
            //                return list[i].outerText;
            //        }
            //    }

            //    list = get_allElementsFromClass('ipe-EventHeader_Fixture ');
            //    if (list.length > 0)
            //    {
            //        for (var i = 0; i < list.length; i++)
            //        {
            //            if (list[i].outerText != '')
            //                return list[i].outerText;
            //        }
            //    }

            //    return '';
            //}

            //get_MatchName()
    #endregion
            string getMatchNameFunc = "function get_allElementsFromClass(classLabel) { var resultArray = []; var domArray = []; domArray.push(ns_gen5_ui.Application.currentApplication.getElementChildren()); while (domArray.length > 0) { try { var curIterator = domArray.shift(); for (var i = 0; i < curIterator.length; i++) { try { if (curIterator[i].className.includes(classLabel)) { resultArray.push(curIterator[i]); } domArray.push(curIterator[i].childNodes); } catch (ex1) {} } } catch (ex) {} } return resultArray; } function get_MatchName() { var list = get_allElementsFromClass('sph-EventWrapper_Label '); if (list.length > 0) { for (var i = 0; i < list.length; i++) { if (list[i].outerText != '') return list[i].outerText; } } list = get_allElementsFromClass('sph-EventHeader_Label '); if (list.length > 0) { for (var i = 0; i < list.length; i++) { if (list[i].outerText != '') return list[i].outerText; } } list = get_allElementsFromClass('ipe-EventHeader_Fixture '); if (list.length > 0) { for (var i = 0; i < list.length; i++) { if (list[i].outerText != '') return list[i].outerText; } } return ''; } get_MatchName();";
            string matchName = RunScript(getMatchNameFunc);
            string[] teams = matchName.Split(new string[] { " v ", " @ ", " vs " }, StringSplitOptions.RemoveEmptyEntries);
            if (teams.Length == 2)
                return $"{teams[0]} - {teams[1]}";
            return "Invalid Event";
        }

        private bool GoMarketAndAddToSlip_ParseBet_Manually(BetburgerInfo info, out string fp)
        {
            fp = "";
            ParseBet_Bet365 secondaryparsebet = null;
            ParseBet_Bet365 parsebet = ParseBet_Bet365.ConvertBetburgerPick2ParseBet_365(info, out secondaryparsebet);
            if (parsebet == null)
            {
                if (info.kind == PickKind.Type_6)
                {
                    string[] splits = info.direct_link.Split('|');
                    if (splits.Length == 5)
                    {
                        string trade_market = splits[0];
                        string trade_period = splits[1];
                        string trade_runnerText = splits[2];
                        string trade_oddsTypeCondition = splits[3];
                        string trade_marketText = splits[4];

                        LogMng.Instance.onWriteStatus($"Converting ParseBet failed sport: {info.sport} {info.homeTeam} vs {info.awayTeam} market: {trade_market} period: {trade_period} runnerText: {trade_runnerText} oddsTypeCondition: {trade_oddsTypeCondition} marketText: {trade_marketText}");
                    }
                }
                else
                {
                    LogMng.Instance.onWriteStatus($"Converting ParseBet failed: directlink: {info.outcome}");
                }
                return false;
            }

            string[] teams = null;

            string matchName = GetCurrentPageEventTitle();
            teams = matchName.Split(new string[] { " - "}, StringSplitOptions.RemoveEmptyEntries);

            if (teams == null || teams.Length != 2)
            {
                LogMng.Instance.onWriteStatus($"GoMarketAndAddToSlip_ParseBet Team names incorrect : {matchName}");
                return false;
            }

            parsebet.TableHeader = parsebet.TableHeader.Replace("*home*", teams[0]).Replace("*away*", teams[1]);
            parsebet.RowHeader = parsebet.RowHeader.Replace("*home*", teams[0]).Replace("*away*", teams[1]);
            parsebet.ColHeader = parsebet.ColHeader.Replace("*home*", teams[0]).Replace("*away*", teams[1]);
            parsebet.ParticipantName = parsebet.ParticipantName.Replace("*home*", teams[0]).Replace("*away*", teams[1]);

            parsebet.ParticipantName = parsebet.ParticipantName.Replace("−", "-");
            parsebet.RowHeader = parsebet.RowHeader.Replace("−", "-");

            if (secondaryparsebet != null)
            {
                secondaryparsebet.TableHeader = secondaryparsebet.TableHeader.Replace("*home*", teams[0]).Replace("*away*", teams[1]);
                secondaryparsebet.RowHeader = secondaryparsebet.RowHeader.Replace("*home*", teams[0]).Replace("*away*", teams[1]);
                secondaryparsebet.ColHeader = secondaryparsebet.ColHeader.Replace("*home*", teams[0]).Replace("*away*", teams[1]);
                secondaryparsebet.ParticipantName = secondaryparsebet.ParticipantName.Replace("*home*", teams[0]).Replace("*away*", teams[1]);

                secondaryparsebet.ParticipantName = secondaryparsebet.ParticipantName.Replace("−", "-");
                secondaryparsebet.RowHeader = secondaryparsebet.RowHeader.Replace("−", "-");
            }

            int nMaxLimit = 3;
            int nRetry1 = 0;
            while (nRetry1++ < nMaxLimit)
            {

                string function1 = "function getHandicapSide(n) { if (n.toLowerCase().startsWith('o ') && isNumber(n.toLowerCase().replace('o ', ''))) return 'o'; if (n.toLowerCase().startsWith('u ') && isNumber(n.toLowerCase().replace('u ', ''))) return 'u'; if (n.toLowerCase().startsWith('over ') && isNumber(n.toLowerCase().replace('over ', ''))) return 'over'; if (n.toLowerCase().startsWith('under ') && isNumber(n.toLowerCase().replace('under ', ''))) return 'under'; return ''; } function GetHandicapLabelValue(n) { let splits = n.split(','); try { if (isNumber(n)) return parseFloat(n); if (splits.length == 2 && isNumber(splits[0].trim()) && isNumber(splits[1].trim())) { return (parseFloat(splits[0].trim()) + parseFloat(splits[1].trim())) / 2; } if (n.toLowerCase().StartsWith('o ') && isNumber(n.toLowerCase().Replace('o ', ''))) return parseFloat(n.toLowerCase().Replace('o ', '')); if (n.toLowerCase().StartsWith('u ') && isNumber(n.toLowerCase().Replace('u ', ''))) return parseFloat(n.toLowerCase().Replace('u ', '')); if (n.toLowerCase().StartsWith('over ') && isNumber(n.toLowerCase().Replace('over ', ''))) return parseFloat(n.toLowerCase().Replace('over ', '')); if (n.toLowerCase().StartsWith('under ') && isNumber(n.toLowerCase().Replace('under ', ''))) return parseFloat(n.toLowerCase().Replace('under ', '')); } catch {} return -100; } function IsHandicapLabel(n) { let splits = n.split(','); try { if (isNumber(n)) return true; if (splits.length == 2 && isNumber(splits[0].trim()) && isNumber(splits[1].trim())) { return true; } if (n.toLowerCase().startsWith('o ') && isNumber(n.toLowerCase().replace('o ', ''))) return true; if (n.toLowerCase().startsWith('u ') && isNumber(n.toLowerCase().replace('u ', ''))) return true; if (n.toLowerCase().startsWith('over ') && isNumber(n.toLowerCase().replace('over ', ''))) return true; if (n.toLowerCase().startsWith('under ') && isNumber(n.toLowerCase().replace('under ', ''))) return true; } catch {} return false; } function getRefinedHandicap(orig) { var refined = ''; if (orig == '') return refined; let splits = orig.split(','); if (splits.length == 2 && isNumber(splits[0].trim()) && isNumber(splits[1].trim())) { refined = (parseFloat(splits[0].trim()) + parseFloat(splits[1].trim())) / 2; } else { splits = orig.split(' '); for (var i = 0; i < splits.length; i++) { if (isNumber(splits[i])) refined += parseFloat(splits[i]) + ' '; else refined += splits[i] + ' '; } refined = refined.trim(); } return refined; } function IsVisibleElement(targetDiv) { var rect = targetDiv.getBoundingClientRect(); var isVisible = (rect.top >= 0) && (rect.bottom <= window.innerHeight - 10); if (document.elementFromPoint(rect.left + 1, rect.top + 1) != null && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.top + 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.left + 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.bottom - 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.top + 1) != null && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.top + 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.bottom - 1) != targetDiv) { isVisible = false; } if (isVisible) { return true; } return false; } function ClickParsebetMarket(tabLabel, MarketLabel, TableHeader, RowHeader, ColHeader, ParticipantName, info_odds, HandiOffset) { console.log('1'); var EV_Item = Locator.treeLookup.getReference(ns_navlib_util.WebsiteNavigationManager.CurrentPageData)._actualChildren[0]; var tabMatched = false; var fdList = []; for (let i = 0; i < EV_Item._actualChildren.length; i++) { var MG_Item = EV_Item._actualChildren[i]; try { if (MG_Item.data.SY != 'cm') { if (MG_Item.data.NA != MarketLabel) { if (MarketLabel == 'Corners') { if (MG_Item.data.NA != 'Alternative Corners' && MG_Item.data.NA != 'Asian Total Corners' && MG_Item.data.NA != 'Corners 2-Way') continue; } else if (MarketLabel.includes('Goals Over/Under')) { if (MG_Item.data.NA != 'Alternative Total Goals') continue; } else if (MarketLabel.includes('Handicap')) { if ((MG_Item.data.NA != 'Alternative ' + MarketLabel) && (MG_Item.data.NA != 'Alternative ' + MarketLabel + ' Result')) continue; } else { continue; } } if (MG_Item._delegateList != null) { for (let i = 0; i < MG_Item._delegateList.length; i++) { if (MG_Item._delegateList[i] == null) continue; if (MG_Item._delegateList[i]._open == null) continue; if (MG_Item._delegateList[i]._open == false) { MG_Item._delegateList[i].setOpen(true); return 'expanded_market'; } } } } var curtableHeader = ''; var lineArray = []; for (let j = 0; j < MG_Item._actualChildren.length; j++) { var MA_Item = MG_Item._actualChildren[j]; if (MG_Item.data.SY == 'cm') { if (MA_Item.data.NA == tabLabel) { tabMatched = true; if (MA_Item.data.LS == null || MA_Item.data.LS != '1') { var ea = { needsCard: false }; ns_navlib_util.WebsiteNavigationManager.Instance.navigateTo(MA_Item.data.PD, ea, false); return 'tab_moved'; } } continue; } var itrMA_NA = '', itrMA_PY = ''; if (MA_Item.data.NA != null && MA_Item.data.NA != '') { itrMA_NA = MA_Item.data.NA.trim(); } if (MA_Item.data.PY != null && MA_Item.data.PY != '') { itrMA_PY = MA_Item.data.PY.trim(); } if (itrMA_PY == 'da' || itrMA_PY == 'db' || itrMA_PY == 'de') { lineArray = []; curtableHeader = itrMA_NA; } for (let p = 0; p < MA_Item._actualChildren.length; p++) { var PA_Item = MA_Item._actualChildren[p]; try { var itrPA_ID = '', itrPA_OD = '', itrPA_NA = '', itrPA_HD = ''; if (PA_Item.data.ID != null && PA_Item.data.ID != '') { itrPA_ID = PA_Item.data.ID.trim(); } if (PA_Item.data.OD != null && PA_Item.data.OD != '') { itrPA_OD = PA_Item.data.OD.trim(); for (let z = 0; z < MA_Item._actualChildren.length; z++) { if (MA_Item._actualChildren[z] != null) { itrPA_OD = PA_Item._delegateList[z]._oddsText._text.trim(); break; } } } if (PA_Item.data.NA != null && PA_Item.data.NA != '') { itrPA_NA = PA_Item.data.NA.trim(); } if (PA_Item.data.HD != null && PA_Item.data.HD != '') { itrPA_HD = PA_Item.data.HD.trim(); } if (itrMA_PY == 'da' || itrMA_PY == 'db' || itrMA_PY == 'de') { lineArray.push(itrPA_NA); } else { var bIsBinded = true; var fdResult = {}; fdResult.ID = itrPA_ID; fdResult.lineOffset = 0; if (TableHeader != '') { if (curtableHeader == '' || curtableHeader != TableHeader) { bIsBinded = false; } } if (RowHeader != '') { if (lineArray.length <= p) { bIsBinded = false; } else if (getRefinedHandicap(lineArray[p]) != getRefinedHandicap(RowHeader)) { if (IsHandicapLabel(lineArray[p]) && IsHandicapLabel(RowHeader) && (Math.abs(GetHandicapLabelValue(lineArray[p]) - GetHandicapLabelValue(RowHeader)) <= HandiOffset)) { fdResult.lineOffset = Math.abs(GetHandicapLabelValue(lineArray[p]) - GetHandicapLabelValue(RowHeader)); } else { bIsBinded = false; } } } if (ColHeader != '') { if (itrMA_NA != '' && itrMA_NA != ColHeader) { bIsBinded = false; } } if (ParticipantName != '') { if (getRefinedHandicap(itrPA_NA) != getRefinedHandicap(ParticipantName) && getRefinedHandicap(itrPA_HD) != getRefinedHandicap(ParticipantName)) { if (IsHandicapLabel(itrPA_HD) && IsHandicapLabel(ParticipantName) && (getHandicapSide(itrPA_HD) == getHandicapSide(ParticipantName)) && (Math.abs(GetHandicapLabelValue(itrPA_HD) - GetHandicapLabelValue(ParticipantName)) <= HandiOffset)) { fdResult.lineOffset = Math.abs(GetHandicapLabelValue(itrPA_HD) - GetHandicapLabelValue(ParticipantName)); } else { bIsBinded = false; } } } if (bIsBinded) { for (let i = 0; i < PA_Item._delegateList.length; i++) { if (PA_Item._delegateList[i] == null) continue; if (PA_Item._delegateList[i].twinEmphasizedHandlerType != null && PA_Item._delegateList[i].twinEmphasizedHandlerType != '') { fdResult.PA_ActiveElement = PA_Item._delegateList[i]; if (PA_Item._delegateList[i]._oddsText != null && PA_Item._delegateList[i]._oddsText._text != null && PA_Item._delegateList[i]._oddsText._text != '') itrPA_OD = PA_Item._delegateList[i]._oddsText._text; break; } } var odd = parseFloat(itrPA_OD); fdResult.oddOffset = Math.abs(info_odds - odd); if (info_odds != odd && Math.abs(info_odds - odd) > 0.1) {} fdList.push(fdResult); } else {} } } catch (e) { console.log('PA exception: ' + e); } } } } catch (e) { console.log('MA exception: ' + e); } } if (tabMatched == false) return 'tab_not_found'; var PA_ActiveElement = null; var PA_ID = ''; var minlineOffset = 100; var oddOffset = 0; for (var i = 0; i < fdList.length; i++) { if (fdList[i].lineOffset < minlineOffset) { PA_ID = fdList[i].ID; PA_ActiveElement = fdList[i].PA_ActiveElement; minlineOffset = fdList[i].lineOffset; oddOffset = fdList[i].oddOffset; } } if (PA_ActiveElement != null) { if (IsVisibleElement(PA_ActiveElement._active_element)) return 'success_' + PA_ID + '_' + JSON.stringify(PA_ActiveElement._active_element.getBoundingClientRect()); return 'scroll'; } return 'failed'; }";
                //function1 += $"ClickParsebetMarket('{parsebet.TabLabel}', '{parsebet.MarketLabel}', '{parsebet.TableHeader}', '{parsebet.RowHeader}', '{parsebet.ColHeader}', '{parsebet.ParticipantName}', {parsebet.odd}, 2);";
                function1 += $"ClickParsebetMarket('{parsebet.TabLabel}', '{parsebet.MarketLabel}', '{parsebet.TableHeader}', '{parsebet.RowHeader}', '{parsebet.ColHeader}', '{parsebet.ParticipantName}', {parsebet.odd}, 0);";
                string OddMarketResult = RunScript(function1).ToLower();
                //#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"ClickParsebetMarket Result: {OddMarketResult}");
                //#endif


//#if (TROUBLESHOT)

                string[] splits1 = info.direct_link.Split('|');
                if (splits1.Length == 5)
                {
                    string trade_market = splits1[0];
                    string trade_period = splits1[1];
                    string trade_runnerText = splits1[2];
                    string trade_oddsTypeCondition = splits1[3];
                    string trade_marketText = splits1[4];
                    LogMng.Instance.onWriteStatus($"ParseBet Param sport: {info.sport} {info.homeTeam} vs {info.awayTeam} market: {trade_market} period: {trade_period} runnerText: {trade_runnerText} oddsTypeCondition: {trade_oddsTypeCondition} marketText: {trade_marketText} odd: {info.odds}");
                }
                LogMng.Instance.onWriteStatus($"Parsebet Tab: {parsebet.TabLabel} Market: {parsebet.MarketLabel} Table: {parsebet.TableHeader} Row: {parsebet.RowHeader} Col: {parsebet.ColHeader} Part: {parsebet.ParticipantName}");
                if (!OddMarketResult.StartsWith("success"))
                {//Log All markets
                    string logfunction = "function ClickParsebetMarket() { var result = ''; var curtabLabel = '', curMarketLabel = '', RowHeader = '', ColHeader = '', ParticipantName = ''; var EV_Item = Locator.treeLookup.getReference(ns_navlib_util.WebsiteNavigationManager.CurrentPageData)._actualChildren[0]; var tabMatched = false; var fdList = []; for (let i = 0; i < EV_Item._actualChildren.length; i++) { var MG_Item = EV_Item._actualChildren[i]; try { if (MG_Item.data.SY != 'cm') { curMarketLabel = MG_Item.data.NA; } var curtableHeader = ''; var lineArray = []; for (let j = 0; j < MG_Item._actualChildren.length; j++) { var MA_Item = MG_Item._actualChildren[j]; if (MG_Item.data.SY == 'cm') { if (MA_Item.data.LS !== null && MA_Item.data.LS == '1') curtabLabel = MA_Item.data.NA; continue; } var itrMA_NA = '', itrMA_PY = ''; if (MA_Item.data.NA != null && MA_Item.data.NA != '') { itrMA_NA = MA_Item.data.NA.trim(); } if (MA_Item.data.PY != null && MA_Item.data.PY != '') { itrMA_PY = MA_Item.data.PY.trim(); } if (itrMA_PY == 'da' || itrMA_PY == 'db' || itrMA_PY == 'de') { lineArray = []; curtableHeader = itrMA_NA; } for (let p = 0; p < MA_Item._actualChildren.length; p++) { var PA_Item = MA_Item._actualChildren[p]; try { var itrPA_ID = '', itrPA_OD = '', itrPA_NA = '', itrPA_HD = ''; if (PA_Item.data.ID != null && PA_Item.data.ID != '') { itrPA_ID = PA_Item.data.ID.trim(); } if (PA_Item.data.OD != null && PA_Item.data.OD != '') { itrPA_OD = PA_Item.data.OD.trim(); for (let z = 0; z < MA_Item._actualChildren.length; z++) { if (MA_Item._actualChildren[z] != null) { itrPA_OD = PA_Item._delegateList[z]._oddsText._text.trim(); break; } } } if (PA_Item.data.NA != null && PA_Item.data.NA != '') { itrPA_NA = PA_Item.data.NA.trim(); } if (PA_Item.data.HD != null && PA_Item.data.HD != '') { itrPA_HD = PA_Item.data.HD.trim(); } if (itrMA_PY == 'da' || itrMA_PY == 'db' || itrMA_PY == 'de') { lineArray.push(itrPA_NA); } else { var odd = parseFloat(itrPA_OD); if (lineArray.length > p) { RowHeader = lineArray[p]; } ColHeader = itrMA_NA; ParticipantName = itrPA_NA + '-' + itrPA_HD; var oneMarket = 'Tab: ' + curtabLabel + ' Market: ' + curMarketLabel + ' Table: ' + curtableHeader + ' Row:' + RowHeader + ' Col: ' + ColHeader + ' Part: ' + ParticipantName + ' ODD: ' + odd + '|'; result += oneMarket; RowHeader = ''; ColHeader = ''; ParticipantName = ''; itrPA_OD = ''; } } catch (e) { console.log('PA exception: ' + e); } } } } catch (e) { console.log('MA exception: ' + e); } } return result; } ClickParsebetMarket();";
                    
                    string logFunctionResult = RunScript(logfunction).ToLower();
                    //#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Page All Markets Result: {logFunctionResult}");
                }
//#endif
                 
                if (OddMarketResult.StartsWith("success"))
                {
                    string[] splits = OddMarketResult.Split('_');
                    if (splits.Length != 3)
                        return false;

                    fp = splits[1];

                    Rect iconRect = Utils.ParseRect(splits[2]);
                    if (iconRect.X <= 0 || iconRect.Y <= 0 || iconRect.Width <= 0 || iconRect.Height <= 0)
                    {
                        return false;
                    }
                    Page_MouseClick(iconRect);
                    return true;
                }
                else if (OddMarketResult == "scroll")
                {
                    nMaxLimit = 30;
                    if (IsScrollDownmost())
                    {
                        LogMng.Instance.onWriteStatus($"Scroll is in Downmost 1");
                        break;
                    }
                    Scroll();
                    Thread.Sleep(500);
                    continue;
                }
                else if (OddMarketResult == "expanded_market" || OddMarketResult == "tab_moved")
                {
                    Thread.Sleep(500);
                    WaitSpinnerShowing();
                    continue;
                }
                else if (OddMarketResult == "tab_not_found")
                {
                    break;
                }
                else if (OddMarketResult == "failed")
                {
                    if (secondaryparsebet != null)
                        parsebet = secondaryparsebet;
                }
                Thread.Sleep(500);
            }
            return false;
        }
        private bool GoMarketAndAddToSlip_LiveSoccerWithDirectLink(BetburgerInfo info, out string fp)
        {
            fp = "";
            string score = info.arbId;
            string MG_ID = "";
            string MA_ID = "";
            string HA = "";
            string isFastMode = "false";
            if (Setting.Instance.bPlaceFastMode)
                isFastMode = "true";
        
            MatchCollection mc = Regex.Matches(info.outcome, "^AH(?<team>\\d)\\((?<handicap>((-?|\\+?)\\d*\\.{0,1}\\d+))\\)");
            if (mc.Count == 1)
            {
                Match m = mc[0];

                MG_ID = "12";
                MA_ID = m.Groups["team"].Value;
                HA = m.Groups["handicap"].Value;
            }                
            
            mc = Regex.Matches(info.outcome, "^(?<side>(Over|Under)) (?<handicap>((-?|\\+?)\\d*\\.{0,1}\\d+))(?<iscorner>( - corners)?)(?<ot>(( OT)|( OT+SO))?)(?<period>(( \\w+ period)|( \\w+ half))?)(?<team>( \\w+ team)?)");
            if (mc.Count == 1)
            {
                Match m = mc[0];

                MG_ID = "15";
                if (m.Groups["side"].Value == "Over")
                {
                    MA_ID = "1";
                    HA = m.Groups["handicap"].Value;
                }
                else
                {
                    MA_ID = "2";
                    HA = m.Groups["handicap"].Value;
                }
            }
            LogMng.Instance.onWriteStatus($"getMarketData params  score: {score} MG_ID: {MG_ID} MA_ID: {MA_ID} HA: {HA} isFastMode: {isFastMode}");

            //string function = "function IsVisibleElement(targetDiv) { var rect = targetDiv.getBoundingClientRect(); var isVisible = (rect.top >= 0) && (rect.bottom <= window.innerHeight - 10); if (document.elementFromPoint(rect.left + 1, rect.top + 1) != null && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.top + 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.left + 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.bottom - 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.top + 1) != null && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.top + 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.bottom - 1) != targetDiv) { isVisible = false; } if (isVisible) { return true; } return false; } function getMarketData(curScore, MG_ID, MA_ID, HA, scriptclick) { try { var targetStem = null; var curPageData = ns_navlib_util.WebsiteNavigationManager.CurrentPageData.split('#')[2].slice(2); var pageData = ns_inplaycorelib_navigation_model.FixtureModel.getModelByLegacyInPlayNavigationID(curPageData); var IT = '6V' + pageData.stem.data.IT.slice(2); var EV_Item = null; for (let i = 0; i < 100; i++) { var newIT = IT.slice(0, -1) + i; EV_Item = Locator.treeLookup.getReference(newIT); if (EV_Item != null) break; } for (let i = 0; i < EV_Item._actualChildren.length; i++) { var MG_Item = EV_Item._actualChildren[i]; try { if (MG_Item.data.ID != MG_ID) continue; if (!MG_Item.data.NA.includes(curScore)) return 'Score changed already'; if (MG_ID == '15') { if (MG_Item._actualChildren.length != 3) return 'GoalLine dataformat incorrect'; let lineIndex = -1; for (let j = 0; j < MG_Item._actualChildren[0]._actualChildren.length; j++) { var PA_Item = MG_Item._actualChildren[0]._actualChildren[j]; if (PA_Item.data.NA.trim() == HA) { lineIndex = j; break; } } if (lineIndex < 0) return 'Can not find line'; for (let k = 1; k < MG_Item._actualChildren.length; k++) { var MA_Item = MG_Item._actualChildren[k]; if (MA_Item.data.ID == MA_ID) { targetStem = MA_Item._actualChildren[lineIndex]; break; } } } else if (MG_ID == '12') { if (MG_Item._actualChildren.length != 2) return 'AsianHandicap dataformat incorrect'; for (let k = 0; k < MG_Item._actualChildren.length; k++) { var MA_Item = MG_Item._actualChildren[k]; if (MA_Item.data.ID != MA_ID) continue; for (let j = 0; j < MA_Item._actualChildren.length; j++) { if (MA_Item._actualChildren[j].data.HA == HA) { targetStem = MA_Item._actualChildren[j]; break; } } break; } } break; } catch (e) { console.log(e); } } if (targetStem == null) return 'can not find target Stem'; var PA_ActiveElement = null; for (let i = 0; i < targetStem._delegateList.length; i++) { try{ if (targetStem._delegateList[i].twinEmphasizedHandlerType != null && targetStem._delegateList[i].twinEmphasizedHandlerType != '') { PA_ActiveElement = targetStem._delegateList[i]; break; } } catch{} } if (scriptclick == 'true') { PA_ActiveElement.betItemSelected(); return 'success_' + targetStem.data.ID + '_' + JSON.stringify(PA_ActiveElement._active_element.getBoundingClientRect()); } if (!IsVisibleElement(PA_ActiveElement._active_element)) return 'invisible_' + targetStem.data.ID + '_' + JSON.stringify(PA_ActiveElement._active_element.getBoundingClientRect()); return 'success_' + targetStem.data.ID + '_' + JSON.stringify(PA_ActiveElement._active_element.getBoundingClientRect()); } catch (e) { return 'exception_' + e; } }";
            string function = "function IsVisibleElement(targetDiv) { var rect = targetDiv.getBoundingClientRect(); var isVisible = (rect.top >= 0) && (rect.bottom <= window.innerHeight - 10); if (document.elementFromPoint(rect.left + 1, rect.top + 1) != null && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.top + 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.left + 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.bottom - 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.top + 1) != null && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.top + 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.bottom - 1) != targetDiv) { isVisible = false; } if (isVisible) { return true; } return false; } function getMarketData(curScore, MG_ID, MA_ID, HA, scriptclick) { try { var targetStem = null; var curPageData = ns_navlib_util.WebsiteNavigationManager.CurrentPageData.split('#')[2].slice(2); var pageData = ns_inplaycorelib_navigation_model.FixtureModel.getModelByLegacyInPlayNavigationID(curPageData); var IT = '6V' + pageData.stem.data.IT.slice(2); var EV_Item = null; for (let i = 0; i < 100; i++) { var newIT = IT.slice(0, -1) + i; EV_Item = Locator.treeLookup.getReference(newIT); if (EV_Item != null) break; } for (let i = 0; i < EV_Item._actualChildren.length; i++) { var MG_Item = EV_Item._actualChildren[i]; try { if (MG_Item.data.ID != MG_ID) continue; if (MG_ID == '15') { if (MG_Item._actualChildren.length != 3) return 'GoalLine dataformat incorrect'; let lineIndex = -1; for (let j = 0; j < MG_Item._actualChildren[0]._actualChildren.length; j++) { var PA_Item = MG_Item._actualChildren[0]._actualChildren[j]; if (PA_Item.data.NA.trim() == HA) { lineIndex = j; break; } } if (lineIndex < 0) return 'Can not find line'; for (let k = 1; k < MG_Item._actualChildren.length; k++) { var MA_Item = MG_Item._actualChildren[k]; if (MA_Item.data.ID == MA_ID) { targetStem = MA_Item._actualChildren[lineIndex]; break; } } } else if (MG_ID == '12') { if (MG_Item._actualChildren.length != 2) return 'AsianHandicap dataformat incorrect'; for (let k = 0; k < MG_Item._actualChildren.length; k++) { var MA_Item = MG_Item._actualChildren[k]; if (MA_Item.data.ID != MA_ID) continue; for (let j = 0; j < MA_Item._actualChildren.length; j++) { if (MA_Item._actualChildren[j].data.HA == HA) { targetStem = MA_Item._actualChildren[j]; break; } } break; } } break; } catch (e) { console.log(e); } } if (targetStem == null) return 'can not find target Stem'; var PA_ActiveElement = null; for (let i = 0; i < targetStem._delegateList.length; i++) { try { if (targetStem._delegateList[i].twinEmphasizedHandlerType != null && targetStem._delegateList[i].twinEmphasizedHandlerType != '') { PA_ActiveElement = targetStem._delegateList[i]; break; } } catch {} } if (scriptclick == 'true') { PA_ActiveElement.betItemSelected(); return 'success_' + targetStem.data.ID + '_' + JSON.stringify(PA_ActiveElement._active_element.getBoundingClientRect()); } if (!IsVisibleElement(PA_ActiveElement._active_element)) return 'invisible_' + targetStem.data.ID + '_' + JSON.stringify(PA_ActiveElement._active_element.getBoundingClientRect()); return 'success_' + targetStem.data.ID + '_' + JSON.stringify(PA_ActiveElement._active_element.getBoundingClientRect()); } catch (e) { return 'exception_' + e; } }";
            function += $"getMarketData('{score}', '{MG_ID}', '{MA_ID}', '{HA}', '{isFastMode}');";

            int nMaxLimit = 3;
            int nRetry1 = 0;
            while (nRetry1++ < nMaxLimit)
            {                                
                string OddMarketResult = RunScript(function).ToLower();
                //#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"getMarketData Result: {OddMarketResult}");
                //#endif

                if (OddMarketResult.StartsWith("success"))
                {
                    string[] splits = OddMarketResult.Split('_');
                    if (splits.Length != 3)
                        return false;

                    fp = splits[1];

                    if (!Setting.Instance.bPlaceFastMode)
                    {
                        Rect iconRect = Utils.ParseRect(splits[2]);
                        if (iconRect.X <= 0 || iconRect.Y <= 0 || iconRect.Width <= 0 || iconRect.Height <= 0)
                        {
                            return false;
                        }
                        Page_MouseClick(iconRect);
                    }
                    return true;
                }
                else if (OddMarketResult.StartsWith("invisible"))
                {
                    nMaxLimit = 10;
                    if (IsScrollDownmost())
                    {
                        LogMng.Instance.onWriteStatus($"Scroll is in Downmost 1");
                        break;
                    }
                    Scroll();                    
                }
                Thread.Sleep(500);
            }
            return false;
        }

        private void MoveToTargetSportstabInSearchResult(BetburgerInfo info)
        {
            try
            {
                string searchGotoTargetSportstabFunc = "function MoveTargetSportTab(sportID) { var resultlist = []; var CL_List = Locator.treeLookup.getReference(ns_navlib_util.WebsiteNavigationManager.CurrentPageData)._actualChildren; try { for (let i = 0; i < CL_List.length; i++) { try { let CL_item = CL_List[i]; if (CL_item == null || CL_item.nodeName !== 'CL') continue; if (CL_item.data.ID != sportID) continue; for (let j = 0; j < CL_item._delegateList.length; j++) { let delegate_item = CL_item._delegateList[j]; if (delegate_item != null) { if(delegate_item._styleList['ssm-SearchClassificationRibbonItem'] == true) { if (delegate_item._styleList['ssm-SearchClassificationRibbonItem_Selected'] == null) { delegate_item.clickHandler(); return 'Opend targetTab success'; } } else { return 'Opend targetTab already'; } } } } catch (e) { console.log('exception1: ' + e); } } if (resultlist.length <= 0) { return 'NoSearchEvent'; } return resultlist; } catch (e) { console.log('exception2: ' + e); } return 'NoSearchEvent1'; } ";
                int sportID = Bet365InnerSportNumber[info.sport.ToLower()];
                searchGotoTargetSportstabFunc += $"MoveTargetSportTab('{sportID}');";
                string searchGotoTargetSportstabResult = RunScript(searchGotoTargetSportstabFunc);
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"searchGotoTargetTab({info.sport}): {searchGotoTargetSportstabResult}");
#endif
                Thread.Sleep(500);
            }
            catch { }
        }

        private bool AddHorseToBetSlip_Manually(BetburgerInfo info, out string fp)
        {
            fp = "";
            LogMng.Instance.onWriteStatus($"find horse  {info.homeTeam}");

            int nRetry = 0;
            while (nRetry++ < 3)
            {
                string searchTextBoxFunc = "ns_sitesearchlib_ui_header.SearchHeader.Instance.searchInput._active_element.getBoundingClientRect()";
                string searchTextBoxResult = RunScript(searchTextBoxFunc).ToLower();
                Rect searchTextBoxRect = Utils.ParseRect(searchTextBoxResult);
                if (searchTextBoxRect.X <= 0 || searchTextBoxRect.Y <= 0 || searchTextBoxRect.Width <= 0 || searchTextBoxRect.Height <= 0)
                {

                    string findSearchButtonFunc = "function FindSearchButton() { try { for (let i = 0; i < ns_gen5_ui.Application.currentApplication._eRegister.widthChanged.length; i++) { try { let item = ns_gen5_ui.Application.currentApplication._eRegister.widthChanged[i]; if (item.scope == null || item.scope == undefined || item.scope._eRegister == null || item.scope._eRegister == undefined || item.scope._eRegister.widthStateChanged == null || item.scope._eRegister.widthStateChanged == undefined) continue; let subItem = item.scope._eRegister.widthStateChanged; if (subItem.length == 0) continue; if (!subItem[0].scope._element.className == null) continue; if (!subItem[0].scope._element.className.includes('hm-HeaderModule')) continue; var childNodes = subItem[0].scope._element.childNodes; var mainHeaderNode = null; childNodes.forEach(element => { if (element.className != null && element.className.includes('hm-MainHeaderWide')) { mainHeaderNode = element; } }); var searchHeaderNode = null; mainHeaderNode.childNodes.forEach(element => { if (element.className != null && element.className.includes('hm-MainHeaderRHSLoggedInWide hm-MainHeaderRHSLoggedIn')) { searchHeaderNode = element; } }); var searchBtn = null; searchHeaderNode.childNodes.forEach(element => { if (element.className != null && element.className.includes('hm-SiteSearchIconLoggedIn')) { searchBtn = element; } }); return JSON.stringify(searchBtn.getBoundingClientRect()); } catch (exx) { return exx; } } } catch (ex) { return ex; } return 'noButton'; } FindSearchButton();";
                    string SearchButtonResult = RunScript(findSearchButtonFunc).ToLower();
                    Rect SearchButtonRect = Utils.ParseRect(SearchButtonResult);
                    if (SearchButtonRect.X <= 0 || SearchButtonRect.Y <= 0 || SearchButtonRect.Width <= 0 || SearchButtonRect.Height <= 0)
                    {

                        LogMng.Instance.onWriteStatus("Can't find Search Button element");
                        Thread.Sleep(500);

                        continue;
                    }
                    Page_MouseClick(SearchButtonRect);
                    WaitSpinnerShowing();
                }


                bool bSearchFocused = false;
                int nFindTextBoxRetry = 0;
                while (nFindTextBoxRetry++ < 3)
                {
                    Thread.Sleep(300);
                    string isInputFocused = RunScript("document.activeElement == ns_sitesearchlib_ui_header.SearchHeader.Instance.searchInput._active_element");
                    if (isInputFocused.ToLower() == "true" && nFindTextBoxRetry >= 2)
                    {
                        bSearchFocused = true;
                        break;
                    }
                    ////////////////////////////////////////////////////////////////////////////
                    searchTextBoxFunc = "ns_sitesearchlib_ui_header.SearchHeader.Instance.searchInput._active_element.getBoundingClientRect()";
                    searchTextBoxResult = RunScript(searchTextBoxFunc).ToLower();
                    searchTextBoxRect = Utils.ParseRect(searchTextBoxResult);
                    if (searchTextBoxRect.X <= 0 || searchTextBoxRect.Y <= 0 || searchTextBoxRect.Width <= 0 || searchTextBoxRect.Height <= 0)
                    {
#if (TROUBLESHOT)
                                        LogMng.Instance.onWriteStatus("Can't find Search Textbox element");
#endif
                        Thread.Sleep(500);
                        continue;
                    }
                    Page_MouseClick(searchTextBoxRect);
                    Thread.Sleep(500);
                    WaitSpinnerShowing();
                }

                if (!bSearchFocused)
                {
                    LogMng.Instance.onWriteStatus("Searching bar is not focused");
                    continue;
                }

                Page_KeyboardType(info.homeTeam);
                Thread.Sleep(500);
                WaitSpinnerShowing();
                break;
            }

            MoveToTargetSportstabInSearchResult(info);

            nRetry = 0;
            int nMaxRetryCount = 3;
            while (nRetry++ < nMaxRetryCount)
            {
                string searchFirstResultFunc = "function IsVisibleElement(targetDiv) { var rect = targetDiv.getBoundingClientRect(); var isVisible = (rect.top >= 0) && (rect.bottom <= window.innerHeight - 10); if (isVisible) { return true; } return false; } function GetFindHorseResultList() { var resultlist = []; var CL_List = Locator.treeLookup.getReference(ns_navlib_util.WebsiteNavigationManager.CurrentPageData)._actualChildren; try { for (let i = 0; i < CL_List.length; i++) { try { let CL_item = CL_List[i]; if (CL_item == null || CL_item.nodeName !== 'CL') continue; if (CL_item.data.ID != '2') continue; for (let j = 0; j < CL_item._actualChildren.length; j++) { let EV_item = CL_item._actualChildren[j]; if (EV_item == null || EV_item.nodeName !== 'EV') continue; if (EV_item.data.DO != '1') continue; for (let k = 0; k < EV_item._actualChildren.length; k++) { let MG_item = EV_item._actualChildren[k]; if (MG_item == null || MG_item.nodeName !== 'MG') continue; for (let l = 0; l < MG_item._actualChildren.length; l++) { let MA_item = MG_item._actualChildren[l]; if (MA_item == null || MA_item.nodeName !== 'MA') continue; for (let m = 0; m < MA_item._actualChildren.length; m++) { let PA_item = MA_item._actualChildren[m]; if (PA_item == null || PA_item.nodeName !== 'PA' || PA_item.data == null) continue; if (PA_item.data.FI == null || PA_item.data.ID == null || PA_item.data.NA == null || PA_item.data.OD == null || PA_item.data.MA == null) continue; try { var resultdata = new Object(); for (let n = PA_item._delegateList.length - 1; n >= 0; n--) { resultdata['RECT'] = PA_item._delegateList[n]._active_element.getBoundingClientRect(); if (resultdata['RECT'].width != 0) { if (!IsVisibleElement(PA_item._delegateList[n]._active_element)) resultdata['RECT'] = 'scroll'; break; } } resultdata['CL_NA'] = CL_item.data.NA; resultdata['EV_NA'] = EV_item.data.NA; resultdata['MG_BC'] = MG_item.data.BC; resultdata['CC'] = PA_item.data.CC; resultdata['NA'] = PA_item.data.NA; resultdata['N2'] = PA_item.data.N2; resultdata['ID'] = PA_item.data.ID; resultdata['FI'] = PA_item.data.FI; resultdata['MA'] = PA_item.data.MA; resultdata['OD'] = PA_item.data.OD; resultlist.push(resultdata); } catch {} } } } } } catch (e) { console.log('exception1: ' + e); } } if (resultlist.length <= 0) { return 'NoSearchEvent'; } return resultlist; } catch (e) { console.log('exception2: ' + e); } return 'NoSearchEvent1'; } GetFindHorseResultList();";
                string searchFirstResultResult = RunScript(searchFirstResultFunc);
                if (string.IsNullOrEmpty(searchFirstResultResult))
                {
                    Thread.Sleep(500);
                    continue;
                }

#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"searchList: {searchFirstResultResult}");
#endif
                Rect searchFirstResultRect = new Rect(0, 0, 0, 0);
                try
                {
                    dynamic resultlist = JsonConvert.DeserializeObject<dynamic>(searchFirstResultResult);
                    foreach (var resultitr in resultlist)
                    {
                        if (resultitr.CL_NA.ToString().ToLower() != "horse racing")
                            continue;

                        if (resultitr.EV_NA.ToString().ToLower() != "bets")
                            continue;

                        if (resultitr.N2.ToString().ToLower() != "win and each way")
                            continue;

                        if (!resultitr.CC.ToString().ToLower().Contains(info.league.ToLower()))
                            continue;

                        if (resultitr.RECT.ToString() == "scroll")
                        {
                            nMaxRetryCount = 10;
                            if (IsScrollDownmost())
                            {
                                LogMng.Instance.onWriteStatus($"Scroll is in Downmost");
                                return false;
                            }
                            Scroll();
                            Thread.Sleep(500);
                            continue;
                        }
                        Rect iconRect = Utils.ParseRectFromJson(resultitr.RECT.ToString());
                        if (iconRect.X <= 0 || iconRect.Y <= 0 || iconRect.Width <= 0 || iconRect.Height <= 0)
                        {
                            return false;
                        }
                        Page_MouseClick(iconRect);


                        fp = resultitr.ID.ToString();

                        return true;
                    }

                }
                catch { }
                {

                }
            }
            return false;
        }

        private bool RunAddLinkToBetSlip(BetburgerInfo info, out string fp)
        {
            fp = "";
            if (!info.siteUrl.Contains("bet365.com"))
                return false;

            string addbetUrl = info.siteUrl.Replace("bet365.com", Setting.Instance.domain);

            MatchCollection mc = Regex.Matches(addbetUrl, "&bs=(?<f>(\\d+))-(?<fp>(\\d+))");
            if (mc.Count == 1 && !string.IsNullOrEmpty(mc[0].Groups["f"].Value) && !string.IsNullOrEmpty(mc[0].Groups["fp"].Value))
            {
                fp = mc[0].Groups["fp"].Value;
                LogMng.Instance.onWriteStatus($"addbet by link fp: {fp} link: {addbetUrl}");

                Page_Navigate(addbetUrl);
                WaitSpinnerShowing();
                return true;
            }
            return false;
        }
        private bool AddHorseToBetSlip(BetburgerInfo info, out string fp)
        {
            fp = "";

            LogMng.Instance.onWriteStatus($"find horse by fast mode {info.homeTeam}");

            string origLangId = RunScript("function getLanguageId() { try { var origId = Locator.user.languageId; Locator.user.languageId = '1'; return origId;} catch {} return '1'; } getLanguageId();");
            string navigationUrl = $"#AX#K^{info.homeTeam}#";
            Page_Navigate(navigationUrl);
            WaitSpinnerShowing();

            string setLangCmd = "function setLanguageId(langId) { try { Locator.user.languageId = langId; } catch {} } ";
            setLangCmd += $"setLanguageId('{origLangId}');";

            MoveToTargetSportstabInSearchResult(info);

            int nRetry = 0;
            int nMaxRetryCount = 3;
            while (nRetry++ < nMaxRetryCount)
            {
                string searchFirstResultFunc = "function GetFindHorseResultList() { var resultlist = []; var CL_List = Locator.treeLookup.getReference(ns_navlib_util.WebsiteNavigationManager.CurrentPageData)._actualChildren; try { for (let i = 0; i < CL_List.length; i++) { try { let CL_item = CL_List[i]; if (CL_item == null || CL_item.nodeName !== 'CL') continue; if (CL_item.data.ID != '2') continue; for (let j = 0; j < CL_item._actualChildren.length; j++) { let EV_item = CL_item._actualChildren[j]; if (EV_item == null || EV_item.nodeName !== 'EV') continue; if (EV_item.data.DO != '1') continue; for (let k = 0; k < EV_item._actualChildren.length; k++) { let MG_item = EV_item._actualChildren[k]; if (MG_item == null || MG_item.nodeName !== 'MG') continue; for (let l = 0; l < MG_item._actualChildren.length; l++) { let MA_item = MG_item._actualChildren[l]; if (MA_item == null || MA_item.nodeName !== 'MA') continue; for (let m = 0; m < MA_item._actualChildren.length; m++) { let PA_item = MA_item._actualChildren[m]; if (PA_item == null || PA_item.nodeName !== 'PA' || PA_item.data == null) continue; if (PA_item.data.FI == null || PA_item.data.ID == null || PA_item.data.NA == null || PA_item.data.OD == null || PA_item.data.MA == null) continue; try { var resultdata = new Object(); for (let n = PA_item._delegateList.length-1; n >= 0; n--) { resultdata['RECT'] = PA_item._delegateList[n]._active_element.getBoundingClientRect(); if (resultdata['RECT'].width != 0) break; } resultdata['CL_NA'] = CL_item.data.NA; resultdata['EV_NA'] = EV_item.data.NA; resultdata['MG_BC'] = MG_item.data.BC; resultdata['CC'] = PA_item.data.CC; resultdata['NA'] = PA_item.data.NA; resultdata['N2'] = PA_item.data.N2; resultdata['ID'] = PA_item.data.ID; resultdata['FI'] = PA_item.data.FI; resultdata['MA'] = PA_item.data.MA; resultdata['OD'] = PA_item.data.OD; resultlist.push(resultdata); } catch {} } } } } } catch (e) { console.log('exception1: ' + e); } } if (resultlist.length <= 0) { return 'NoSearchEvent'; } return resultlist; } catch (e) { console.log('exception2: ' + e); } return 'NoSearchEvent1'; } GetFindHorseResultList();";
                string searchFirstResultResult = RunScript(searchFirstResultFunc);
                if (string.IsNullOrEmpty(searchFirstResultResult))
                {
                    Thread.Sleep(500);
                    continue;
                }
               
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"searchList: {searchFirstResultResult}");
#endif
                Rect searchFirstResultRect = new Rect(0, 0, 0, 0);
                try
                {
                    dynamic resultlist = JsonConvert.DeserializeObject<dynamic>(searchFirstResultResult);
                    foreach (var resultitr in resultlist)
                    {
                        if (resultitr.CL_NA.ToString().ToLower() != "horse racing")
                            continue;

                        if (resultitr.EV_NA.ToString().ToLower() != "bets")
                            continue;

                        if (resultitr.N2.ToString().ToLower() != "win and each way")
                            continue;

                        if (!resultitr.CC.ToString().ToLower().Contains(info.league.ToLower()))
                            continue;

                        if (resultitr.RECT.ToString() == "scroll")
                        {
                            nMaxRetryCount = 10;
                            if (IsScrollDownmost())
                            {
                                LogMng.Instance.onWriteStatus($"Scroll is in Downmost");
                                return false;
                            }
                            Scroll();
                            Thread.Sleep(500);
                            continue;
                        }
                        Rect iconRect = Utils.ParseRectFromJson(resultitr.RECT.ToString());
                        if (iconRect.X <= 0 || iconRect.Y <= 0 || iconRect.Width <= 0 || iconRect.Height <= 0)
                        {
                            return false;
                        }
                        Page_MouseClick(iconRect);


                        fp = resultitr.ID.ToString();

                        return true;
                    }

                }
                catch { }
                {

                }
            }
            return false;
        }
        
        private bool GoMarketAndAddToSlip_ParseBet(BetburgerInfo info, out string fp)
        {
            fp = "";
            ParseBet_Bet365 secondaryparsebet = null;
            ParseBet_Bet365 parsebet = ParseBet_Bet365.ConvertBetburgerPick2ParseBet_365(info, out secondaryparsebet);
            if (parsebet == null)
            {
                LogMng.Instance.onWriteStatus($"Converting ParseBet failed: directlink: {info.outcome}");
                return false;
            }

            string[] teams = null;
            
            string matchName = GetCurrentPageEventTitle();
            teams = matchName.Split(new string[] { " - "}, StringSplitOptions.RemoveEmptyEntries);
             
            if (teams == null || teams.Length != 2)
            {
                LogMng.Instance.onWriteStatus($"GoMarketAndAddToSlip_ParseBet Team names incorrect : {matchName}");
                return false;
            }
            
            parsebet.TableHeader = parsebet.TableHeader.Replace("*home*", teams[0]).Replace("*away*", teams[1]);            
            parsebet.RowHeader = parsebet.RowHeader.Replace("*home*", teams[0]).Replace("*away*", teams[1]);
            parsebet.ColHeader = parsebet.ColHeader.Replace("*home*", teams[0]).Replace("*away*", teams[1]);           
            parsebet.ParticipantName = parsebet.ParticipantName.Replace("*home*", teams[0]).Replace("*away*", teams[1]);

            parsebet.ParticipantName = parsebet.ParticipantName.Replace("−", "-");
            parsebet.RowHeader = parsebet.RowHeader.Replace("−", "-");


            int nRetry1 = 0;
            while (nRetry1++ < 3)
            {

                string function1 = "function getHandicapSide(n) { if (n.toLowerCase().startsWith('o ') && isNumber(n.toLowerCase().replace('o ', ''))) return 'o'; if (n.toLowerCase().startsWith('u ') && isNumber(n.toLowerCase().replace('u ', ''))) return 'u'; if (n.toLowerCase().startsWith('over ') && isNumber(n.toLowerCase().replace('over ', ''))) return 'over'; if (n.toLowerCase().startsWith('under ') && isNumber(n.toLowerCase().replace('under ', ''))) return 'under'; return ''; } function GetHandicapLabelValue(n) { let splits = n.split(','); try { if (isNumber(n)) return parseFloat(n); if (splits.length == 2 && isNumber(splits[0].trim()) && isNumber(splits[1].trim())) { return (parseFloat(splits[0].trim()) + parseFloat(splits[1].trim())) / 2; } if (n.toLowerCase().StartsWith('o ') && isNumber(n.toLowerCase().Replace('o ', ''))) return parseFloat(n.toLowerCase().Replace('o ', '')); if (n.toLowerCase().StartsWith('u ') && isNumber(n.toLowerCase().Replace('u ', ''))) return parseFloat(n.toLowerCase().Replace('u ', '')); if (n.toLowerCase().StartsWith('over ') && isNumber(n.toLowerCase().Replace('over ', ''))) return parseFloat(n.toLowerCase().Replace('over ', '')); if (n.toLowerCase().StartsWith('under ') && isNumber(n.toLowerCase().Replace('under ', ''))) return parseFloat(n.toLowerCase().Replace('under ', '')); } catch {} return -100; } function IsHandicapLabel(n) { let splits = n.split(','); try { if (isNumber(n)) return true; if (splits.length == 2 && isNumber(splits[0].trim()) && isNumber(splits[1].trim())) { return true; } if (n.toLowerCase().startsWith('o ') && isNumber(n.toLowerCase().replace('o ', ''))) return true; if (n.toLowerCase().startsWith('u ') && isNumber(n.toLowerCase().replace('u ', ''))) return true; if (n.toLowerCase().startsWith('over ') && isNumber(n.toLowerCase().replace('over ', ''))) return true; if (n.toLowerCase().startsWith('under ') && isNumber(n.toLowerCase().replace('under ', ''))) return true; } catch {} return false; } function getRefinedHandicap(orig) { var refined = ''; if (orig == '') return refined; let splits = orig.split(','); if (splits.length == 2 && isNumber(splits[0].trim()) && isNumber(splits[1].trim())) { refined = (parseFloat(splits[0].trim()) + parseFloat(splits[1].trim())) / 2; } else { splits = orig.split(' '); for (var i = 0; i < splits.length; i++) { if (isNumber(splits[i])) refined += parseFloat(splits[i]) + ' '; else refined += splits[i] + ' '; } refined = refined.trim(); } return refined; } function IsVisibleElement(targetDiv) { var rect = targetDiv.getBoundingClientRect(); var isVisible = (rect.top >= 0) && (rect.bottom <= window.innerHeight - 10); if (document.elementFromPoint(rect.left + 1, rect.top + 1) != null && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.top + 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.left + 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.bottom - 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.top + 1) != null && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.top + 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.bottom - 1) != targetDiv) { isVisible = false; } if (isVisible) { return true; } return false; } function ClickParsebetMarket(tabLabel, MarketLabel, TableHeader, RowHeader, ColHeader, ParticipantName, info_odds, HandiOffset) { console.log('1'); var EV_Item = Locator.treeLookup.getReference(ns_navlib_util.WebsiteNavigationManager.CurrentPageData)._actualChildren[0]; var tabMatched = false; var fdList = []; for (let i = 0; i < EV_Item._actualChildren.length; i++) { var MG_Item = EV_Item._actualChildren[i]; try { if (MG_Item.data.SY != 'cm') { if (MG_Item.data.NA != MarketLabel) { if (MarketLabel == 'Corners') { if (MG_Item.data.NA != 'Alternative Corners' && MG_Item.data.NA != 'Asian Total Corners' && MG_Item.data.NA != 'Corners 2-Way') continue; } else if (MarketLabel.includes('Goals Over/Under')) { if (MG_Item.data.NA != 'Alternative Total Goals') continue; } else if (MarketLabel.includes('Handicap')) { if ((MG_Item.data.NA != 'Alternative ' + MarketLabel) && (MG_Item.data.NA != 'Alternative ' + MarketLabel + ' Result')) continue; } else { continue; } } if (MG_Item._delegateList != null) { for (let i = 0; i < MG_Item._delegateList.length; i++) { if (MG_Item._delegateList[i] == null) continue; if (MG_Item._delegateList[i]._open == null) continue; if (MG_Item._delegateList[i]._open == false) { MG_Item._delegateList[i].setOpen(true); return 'expanded_market'; } } } } var curtableHeader = ''; var lineArray = []; for (let j = 0; j < MG_Item._actualChildren.length; j++) { var MA_Item = MG_Item._actualChildren[j]; if (MG_Item.data.SY == 'cm') { if (MA_Item.data.NA == tabLabel) { tabMatched = true; if (MA_Item.data.LS == null || MA_Item.data.LS != '1') { var ea = { needsCard: false }; ns_navlib_util.WebsiteNavigationManager.Instance.navigateTo(MA_Item.data.PD, ea, false); return 'tab_moved'; } } continue; } var itrMA_NA = '', itrMA_PY = ''; if (MA_Item.data.NA != null && MA_Item.data.NA != '') { itrMA_NA = MA_Item.data.NA.trim(); } if (MA_Item.data.PY != null && MA_Item.data.PY != '') { itrMA_PY = MA_Item.data.PY.trim(); } if (itrMA_PY == 'da' || itrMA_PY == 'db' || itrMA_PY == 'de') { lineArray = []; curtableHeader = itrMA_NA; } for (let p = 0; p < MA_Item._actualChildren.length; p++) { var PA_Item = MA_Item._actualChildren[p]; try { var itrPA_ID = '', itrPA_OD = '', itrPA_NA = '', itrPA_HD = ''; if (PA_Item.data.ID != null && PA_Item.data.ID != '') { itrPA_ID = PA_Item.data.ID.trim(); } if (PA_Item.data.OD != null && PA_Item.data.OD != '') { itrPA_OD = PA_Item.data.OD.trim(); for (let z = 0; z < MA_Item._actualChildren.length; z++) { if (MA_Item._actualChildren[z] != null) { itrPA_OD = PA_Item._delegateList[z]._oddsText._text.trim(); break; } } } if (PA_Item.data.NA != null && PA_Item.data.NA != '') { itrPA_NA = PA_Item.data.NA.trim(); } if (PA_Item.data.HD != null && PA_Item.data.HD != '') { itrPA_HD = PA_Item.data.HD.trim(); } if (itrMA_PY == 'da' || itrMA_PY == 'db' || itrMA_PY == 'de') { lineArray.push(itrPA_NA); } else { var bIsBinded = true; var fdResult = {}; fdResult.ID = itrPA_ID; fdResult.lineOffset = 0; if (TableHeader != '') { if (curtableHeader == '' || curtableHeader != TableHeader) { bIsBinded = false; } } if (RowHeader != '') { if (lineArray.length <= p) { bIsBinded = false; } else if (getRefinedHandicap(lineArray[p]) != getRefinedHandicap(RowHeader)) { if (IsHandicapLabel(lineArray[p]) && IsHandicapLabel(RowHeader) && (Math.abs(GetHandicapLabelValue(lineArray[p]) - GetHandicapLabelValue(RowHeader)) <= HandiOffset)) { fdResult.lineOffset = Math.abs(GetHandicapLabelValue(lineArray[p]) - GetHandicapLabelValue(RowHeader)); } else { bIsBinded = false; } } } if (ColHeader != '') { if (itrMA_NA != '' && itrMA_NA != ColHeader) { bIsBinded = false; } } if (ParticipantName != '') { if (getRefinedHandicap(itrPA_NA) != getRefinedHandicap(ParticipantName) && getRefinedHandicap(itrPA_HD) != getRefinedHandicap(ParticipantName)) { if (IsHandicapLabel(itrPA_HD) && IsHandicapLabel(ParticipantName) && (getHandicapSide(itrPA_HD) == getHandicapSide(ParticipantName)) && (Math.abs(GetHandicapLabelValue(itrPA_HD) - GetHandicapLabelValue(ParticipantName)) <= HandiOffset)) { fdResult.lineOffset = Math.abs(GetHandicapLabelValue(itrPA_HD) - GetHandicapLabelValue(ParticipantName)); } else { bIsBinded = false; } } } if (bIsBinded) { for (let i = 0; i < PA_Item._delegateList.length; i++) { if (PA_Item._delegateList[i] == null) continue; if (PA_Item._delegateList[i].twinEmphasizedHandlerType != null && PA_Item._delegateList[i].twinEmphasizedHandlerType != '') { fdResult.PA_ActiveElement = PA_Item._delegateList[i]; if (PA_Item._delegateList[i]._oddsText != null && PA_Item._delegateList[i]._oddsText._text != null && PA_Item._delegateList[i]._oddsText._text != '') itrPA_OD = PA_Item._delegateList[i]._oddsText._text; break; } } var odd = parseFloat(itrPA_OD); fdResult.oddOffset = Math.abs(info_odds - odd); if (info_odds != odd && Math.abs(info_odds - odd) > 0.1) {} fdList.push(fdResult); } else {} } } catch (e) { console.log('PA exception: ' + e); } } } } catch (e) { console.log('MA exception: ' + e); } } if (tabMatched == false) return 'tab_not_found'; var PA_ActiveElement = null; var PA_ID = ''; var minlineOffset = 100; var oddOffset = 0; for (var i = 0; i < fdList.length; i++) { if (fdList[i].lineOffset < minlineOffset) { PA_ID = fdList[i].ID; PA_ActiveElement = fdList[i].PA_ActiveElement; minlineOffset = fdList[i].lineOffset; oddOffset = fdList[i].oddOffset; } } if (PA_ActiveElement != null) { if (IsVisibleElement(PA_ActiveElement._active_element)) return 'success_' + PA_ID + '_' + JSON.stringify(PA_ActiveElement._active_element.getBoundingClientRect()); return 'scroll'; } return 'failed'; }";
                function1 += $"ClickParsebetMarket('{parsebet.TabLabel}', '{parsebet.MarketLabel}', '{parsebet.TableHeader}', '{parsebet.RowHeader}', '{parsebet.ColHeader}', '{parsebet.ParticipantName}', {parsebet.odd}, 0);";
                string OddMarketResult = RunScript(function1).ToLower();
                //#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"ClickParsebetMarket Result: {OddMarketResult}");
                //#endif

                if (OddMarketResult.StartsWith("success"))
                {
                    fp = OddMarketResult.Replace("success_", "");
                    return true;
                }
                else if (OddMarketResult == "expanded_market" || OddMarketResult == "tab_moved")
                {
                    Thread.Sleep(500);
                    WaitSpinnerShowing();
                    continue;
                }
                else if (OddMarketResult == "tab_not_found")
                {
                    break;
                }
                Thread.Sleep(500);
            }
            return false;
        }
        private bool GoMarketAndAddToSlip_OpenBet_Manually(BetburgerInfo info, out string fp)
        {//Prematch
            fp = "";
            OpenBet_Bet365 openbet = Utils.ConvertBetburgerPick2OpenBet_365(info);
            if (openbet == null)
            {
                LogMng.Instance.onWriteStatus($"Converting OpenBet failed: directlink: {info.direct_link}");
                return false;
            }

            int nMaxLimit = 10;
            int nRetry1 = 0;
            while (nRetry1++ < nMaxLimit)
            {
                string function1 = "function IsVisibleElement(targetDiv) { var rect = targetDiv.getBoundingClientRect(); var isVisible = (rect.top >= 0) && (rect.bottom <= window.innerHeight - 10); if (document.elementFromPoint(rect.left + 1, rect.top + 1) != null && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.top + 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.left + 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.bottom - 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.top + 1) != null && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.top + 1) != targetDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.bottom - 1) != targetDiv) { isVisible = false; } if (isVisible) { return true; } return false; } function ClickOpenbetMarket(fi) { var PA_ActiveElement = null; var stemArray = []; stemArray.push(Locator.treeLookup.getReference(ns_navlib_util.WebsiteNavigationManager.CurrentPageData)); var marketExpanded = false; var nexttabPD = ''; while (stemArray.length > 0) { try { var curIterator = stemArray.shift(); if (curIterator._actualChildren != null) { for (let k = 0; k < curIterator._actualChildren.length; k++) stemArray.push(curIterator._actualChildren[k]); } try { if (curIterator.nodeName == null) continue; if (curIterator.nodeName == 'MG') { if (curIterator.data != null && curIterator.data.SY != null && curIterator.data.SY == 'cm') { for (let i = 0; i < curIterator._actualChildren.length - 1; i++) { if (curIterator._actualChildren[i].data != null && curIterator._actualChildren[i].data.LS != null && curIterator._actualChildren[i].data.LS == '1') { for (let j = i + 1; j < curIterator._actualChildren.length; j++) { if (curIterator._actualChildren[j].data.FF != '') continue; if (curIterator._actualChildren[j].data.FG != '') continue; nexttabPD = curIterator._actualChildren[j].data.PD; break; } break; } } } else { if (curIterator._actualChildren != null && curIterator._actualChildren.length == 0 && curIterator._delegateList != null) { for (let i = 0; i < curIterator._delegateList.length; i++) { if (curIterator._delegateList[i]._open == null) continue; if (curIterator._delegateList[i]._open == false) { marketExpanded = true; curIterator._delegateList[i].setOpen(true); } } } } } else if (curIterator.nodeName == 'PA') { if (curIterator.data != null && curIterator.data.ID != null && curIterator.data.ID == fi) { for (let i = 0; i < curIterator._delegateList.length; i++) { if (curIterator._delegateList[i].twinEmphasizedHandlerType != null && curIterator._delegateList[i].twinEmphasizedHandlerType != '') { PA_ActiveElement = curIterator._delegateList[i]; break; } } } } } catch (ex1) { console.log('ex1: ' + ex1); } } catch (ex) { console.log('ex: ' + ex); } } if (marketExpanded) return 'expanded'; if (PA_ActiveElement != null) { if (IsVisibleElement(PA_ActiveElement._active_element)) return 'success_' + JSON.stringify(PA_ActiveElement._active_element.getBoundingClientRect()); return 'scroll'; } if (nexttabPD != '') { var e = { needsCard: false }; ns_navlib_util.WebsiteNavigationManager.Instance.navigateTo(nexttabPD, e, false); return 'moved'; } for (let i = 0; i < Locator.user._eRegister.oddsChanged.length; i++) { try { let item = Locator.user._eRegister.oddsChanged[i]; if (item.scope == null || item.scope == undefined || item.scope.stem == undefined || item.scope.stem.data == undefined) continue; if (item.scope.stem.data.ID == fi) { if (IsVisibleElement(item.scope._active_element)) return 'success_' + JSON.stringify(item.scope._active_element.getBoundingClientRect()); return 'scroll'; } } catch (exx) {} } return 'failed'; } ";
                function1 += $"ClickOpenbetMarket('{openbet.betData[0].i2}');";

                string OddMarketResult = RunScript(function1).ToLower();

                //#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"clickMarket Result: {OddMarketResult}");
                //#endif

                if (OddMarketResult.StartsWith("success_"))
                {
                    string rectStr = OddMarketResult.Replace("success_", "");
                    Rect iconRect = Utils.ParseRect(rectStr);
                    if (iconRect.X <= 0 || iconRect.Y <= 0 || iconRect.Width <= 0 || iconRect.Height <= 0)
                    {
                        return false;
                    }
                    Page_MouseClick(iconRect);

                    fp = openbet.betData[0].i2;
                    return true;
                }
                else if (OddMarketResult == "scroll")
                {
                    nMaxLimit = 40;
                    if (IsScrollDownmost())
                    {
                        LogMng.Instance.onWriteStatus($"Scroll is in Downmost 1");
                        break;
                    }
                    Scroll();
                    Thread.Sleep(500);
                    continue;
                }
                else if (OddMarketResult == "expanded" || OddMarketResult == "moved")
                {
                    Thread.Sleep(500);
                    WaitSpinnerShowing();
                    continue;
                }
                Thread.Sleep(500);
            }
            return false;
        }
        private bool GoMarketAndAddToSlip_OpenBet(BetburgerInfo info, out string fp)
        {
            fp = "";
            OpenBet_Bet365 openbet = Utils.ConvertBetburgerPick2OpenBet_365(info);
            if (openbet == null)
            {
                LogMng.Instance.onWriteStatus($"Converting OpenBet failed: directlink: {info.direct_link}");
                return false;
            }
                        
            int nRetry1 = 0;
            while (nRetry1++ < 10)
            {
                string function1 = "function ClickOpenbetMarket(fi) { var PA_ActiveElement = null; var stemArray = []; stemArray.push(Locator.treeLookup.getReference(ns_navlib_util.WebsiteNavigationManager.CurrentPageData)); var marketExpanded = false; var nexttabPD = ''; while (stemArray.length > 0) { try { var curIterator = stemArray.shift(); if (curIterator._actualChildren != null) { for (let k = 0; k < curIterator._actualChildren.length; k++) stemArray.push(curIterator._actualChildren[k]); } try { if (curIterator.nodeName == null) continue; if (curIterator.nodeName == 'MG') { if (curIterator.data != null && curIterator.data.SY != null && curIterator.data.SY == 'cm') { for (let i = 0; i < curIterator._actualChildren.length - 1; i++) { if (curIterator._actualChildren[i].data != null && curIterator._actualChildren[i].data.LS != null && curIterator._actualChildren[i].data.LS == '1') { for (let j = i + 1; j < curIterator._actualChildren.length; j++) { if (curIterator._actualChildren[j].data.FF != '') continue; if (curIterator._actualChildren[j].data.FG != '') continue; nexttabPD = curIterator._actualChildren[j].data.PD; break; } break; } } } else { if (curIterator._actualChildren != null && curIterator._actualChildren.length == 0 && curIterator._delegateList != null) { for (let i = 0; i < curIterator._delegateList.length; i++) { if (curIterator._delegateList[i]._open == null) continue; if (curIterator._delegateList[i]._open == false) { marketExpanded = true; curIterator._delegateList[i].setOpen(true); } } } } } else if (curIterator.nodeName == 'PA') { if (curIterator.data != null && curIterator.data.ID != null && curIterator.data.ID == fi) { for (let i = 0; i < curIterator._delegateList.length; i++) { if (curIterator._delegateList[i].twinEmphasizedHandlerType != null && curIterator._delegateList[i].twinEmphasizedHandlerType != '') { PA_ActiveElement = curIterator._delegateList[i]; break; } } } } } catch (ex1) { console.log('ex1: ' + ex1); } } catch (ex) { console.log('ex: ' + ex); } } if (marketExpanded) return 'expanded'; if (PA_ActiveElement != null) { PA_ActiveElement.betItemSelected(); return 'success'; } if (nexttabPD != '') { var e = { needsCard: false }; ns_navlib_util.WebsiteNavigationManager.Instance.navigateTo(nexttabPD, e, false); return 'moved'; } for (let i = 0; i < Locator.user._eRegister.oddsChanged.length; i++) { try { let item = Locator.user._eRegister.oddsChanged[i]; if (item.scope == null || item.scope == undefined || item.scope.stem == undefined || item.scope.stem.data == undefined) continue; if (item.scope.stem.data.ID == fi) { item.scope.betItemSelected(); return 'success'; } } catch (exx) { } } return 'failed'; } ";
                function1 += $"ClickOpenbetMarket('{openbet.betData[0].i2}');";

                string OddMarketResult = RunScript(function1).ToLower();

                //#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"clickMarket Result: {OddMarketResult}");
                //#endif

                if (OddMarketResult == "success")
                {
                    fp = openbet.betData[0].i2;
                    return true;
                }
                else if (OddMarketResult == "expanded" || OddMarketResult == "moved")
                {
                    Thread.Sleep(500);
                    WaitSpinnerShowing();
                    continue;
                }
                Thread.Sleep(500);
            }            
            return false;
        }
        
        private bool GoLinkFromSubKeywords(BetburgerInfo info)
        {
            bool result = false;
            LogMng.Instance.onWriteStatus("Going Event by Sub Keywords");

            string[] keywords = info.homeTeam.Split(' ');
            for (int i = 0; i < keywords.Length; i++)
            {
                if (keywords[i].ToUpper() == "FC")
                    continue;

                string navigationUrl = $"#AX#K^{keywords[i]}#";
                Page_Navigate(navigationUrl);
                WaitSpinnerShowing();

                MoveToTargetSportstabInSearchResult(info);

                string searchFirstResultFunc = "function GetFindResultList() { var resultlist = []; var CL_List = Locator.treeLookup.getReference(ns_navlib_util.WebsiteNavigationManager.CurrentPageData)._actualChildren; try { for (let i = 0; i < CL_List.length; i++) { try { let CL_item = CL_List[i]; if (CL_item == null || CL_item.nodeName !== 'CL') continue; for (let j = 0; j < CL_item._actualChildren.length; j++) { let EV_item = CL_item._actualChildren[j]; if (EV_item == null || EV_item.nodeName !== 'EV') continue; for (let k = 0; k < EV_item._actualChildren.length; k++) { let MG_item = EV_item._actualChildren[k]; if (MG_item == null || MG_item.nodeName !== 'MG') continue; for (let l = 0; l < MG_item._actualChildren.length; l++) { let MA_item = MG_item._actualChildren[l]; if (MA_item == null || MA_item.nodeName !== 'MA') continue; for (let m = 0; m < MA_item._actualChildren.length; m++) { let PA_item = MA_item._actualChildren[m]; if (PA_item == null || PA_item.nodeName !== 'PA' || PA_item.data == null) continue; if (PA_item.data.BC == null || PA_item.data.NA == null || PA_item.data.PD == null) continue; var resultdata = new Object(); resultdata['CL_NA'] = CL_item.data.NA; resultdata['EV_NA'] = EV_item.data.NA; resultdata['NA'] = PA_item.data.NA; resultdata['PD'] = PA_item.data.PD; resultdata['BC'] = PA_item.data.BC; resultlist.push(resultdata); } } } } } catch (e) { console.log('exception1: ' + e); } } if (resultlist.length <= 0) { return 'NoSearchEvent'; } return resultlist; } catch (e) { console.log('exception2: ' + e); } return 'NoSearchEvent1'; } GetFindResultList();";
                string searchFirstResultResult = RunScript(searchFirstResultFunc);


                string Event_PD = "";
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"searchList: {searchFirstResultResult}");
#endif
                Rect searchFirstResultRect = new Rect(0, 0, 0, 0);
                try
                {
                    string searchName = info.awayTeam;
                    double ratio1, ratio2;

                    double highsimilarity = 0;
                    dynamic resultlist = JsonConvert.DeserializeObject<dynamic>(searchFirstResultResult);
                    foreach (var resultitr in resultlist)
                    {

                        DateTime info_started = DateTime.ParseExact(info.started, "MM/dd/yyyy HH:mm:ss", null);

                        if (info.sport.ToLower() != resultitr.CL_NA.ToString().ToLower())
                            continue;

                        if (string.IsNullOrEmpty(resultitr.BC.ToString()))
                            continue;

                        DateTime tmpDate = DateTime.ParseExact(resultitr.BC.ToString(), "yyyyMMddHHmmss", null);
                        tmpDate = tmpDate.AddHours(-1);

                        if (tmpDate.Minute != info_started.Minute)
                            continue;
                        if (Math.Abs(tmpDate.Subtract(info_started).TotalMinutes) > 100)
                            continue;

                        //if (tmpDate.Year != info_started.Year ||
                        //    tmpDate.Month != info_started.Month ||
                        //    tmpDate.Day != info_started.Day ||
                        //    tmpDate.Hour != info_started.Hour ||
                        //    tmpDate.Minute != info_started.Minute)
                        //    continue;

                        string[] awayName = resultitr.NA.ToString().Split(new String[] { " v ", " vs " }, StringSplitOptions.RemoveEmptyEntries);
                        if (awayName.Length != 2)
                        {
                            LogMng.Instance.onWriteStatus($"awayName is strange : {resultitr.NA}");
                            continue;
                        }

                        try
                        {
                            double similarity = Similarity.GetSimilarityRatio(awayName[1], searchName, out ratio1, out ratio2);
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"searchItr: {resultitr.NA.ToString()} similarity: {similarity}");
#endif
                            if (highsimilarity < similarity)
                            {
                                highsimilarity = similarity;
                                Event_PD = resultitr.PD.ToString();
                            }
                        }
                        catch { }
                    }

                    if (highsimilarity < 50)
                        Event_PD = "";
                }
                catch { }
                {

                }

                if (string.IsNullOrEmpty(Event_PD))
                {
                    continue;
                }

                Page_Navigate(Event_PD);
                result = true;
                break;
            }
            return result;
        }            

        private bool OpenBetEventPageBySearch_Manually(BetburgerInfo info)
        {
            bool bOpenPageDirectly = false;
#if (OPENDIRECT)
            bOpenPageDirectly = true;
#endif
            try
            {
                int nTotalRetry = 0;
                while (nTotalRetry++ < 2)
                {
                    string searchName = $"{info.homeTeam} - {info.awayTeam}";
                    string curEventTitle = GetCurrentPageEventTitle();
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"retrying : {nTotalRetry} curEventTitle : {curEventTitle}");
#endif

                    double ratio1, ratio2;

                    double eventSimilarity = Similarity.GetSimilarityRatio(curEventTitle, searchName, out ratio1, out ratio2);
                    if (eventSimilarity >= 90)
                    {
                        LogMng.Instance.onWriteStatus($"Event page is already opened");

                        if (IsThisPageIsNoLongerAvailable())
                        {
                            LogMng.Instance.onWriteStatus($"Event page is no longer available");
                            return false;
                        }

                        //scroll to up                        
                        Scroll(10000);
                        Thread.Sleep(500);
                    }
                    else
                    {
                        if (Setting.Instance.domain.Contains(".it") || bOpenPageDirectly || /*DateTime.Now.Subtract(refreshLastTime).TotalMinutes > 5 ||*/ nTotalRetry == 2)
                        {
                            if (info.kind == PickKind.Type_1)
                            {                                
                                string siteUrl = info.siteUrl.Replace("/", "#");
                                LogMng.Instance.onWriteStatus($"Go Page by siteUrl directly: {siteUrl}");
                                Page_Navigate(siteUrl);                                
                            }
                            else
                            {
                                LogMng.Instance.onWriteStatus($"can't go event page using team name");
                                return false;
                            }
                        }
                        else
                        {
                            LogMng.Instance.onWriteStatus($"go event page by manual find {info.homeTeam} - {info.awayTeam}");

                            string searchTextBoxFunc = "ns_sitesearchlib_ui_header.SearchHeader.Instance.searchInput._active_element.getBoundingClientRect()";
                            string searchTextBoxResult = RunScript(searchTextBoxFunc).ToLower();
                            Rect searchTextBoxRect = Utils.ParseRect(searchTextBoxResult);
                            if (searchTextBoxRect.X <= 0 || searchTextBoxRect.Y <= 0 || searchTextBoxRect.Width <= 0 || searchTextBoxRect.Height <= 0)
                            {

                                string findSearchButtonFunc = "function FindSearchButton() { try { for (let i = 0; i < ns_gen5_ui.Application.currentApplication._eRegister.widthChanged.length; i++) { try { let item = ns_gen5_ui.Application.currentApplication._eRegister.widthChanged[i]; if (item.scope == null || item.scope == undefined || item.scope._eRegister == null || item.scope._eRegister == undefined || item.scope._eRegister.widthStateChanged == null || item.scope._eRegister.widthStateChanged == undefined) continue; let subItem = item.scope._eRegister.widthStateChanged; if (subItem.length == 0) continue; if (!subItem[0].scope._element.className == null) continue; if (!subItem[0].scope._element.className.includes('hm-HeaderModule')) continue; var childNodes = subItem[0].scope._element.childNodes; var mainHeaderNode = null; childNodes.forEach(element => { if (element.className != null && element.className.includes('hm-MainHeaderWide')) { mainHeaderNode = element; } }); var searchHeaderNode = null; mainHeaderNode.childNodes.forEach(element => { if (element.className != null && element.className.includes('hm-MainHeaderRHSLoggedInWide hm-MainHeaderRHSLoggedIn')) { searchHeaderNode = element; } }); var searchBtn = null; searchHeaderNode.childNodes.forEach(element => { if (element.className != null && element.className.includes('hm-SiteSearchIconLoggedIn')) { searchBtn = element; } }); return JSON.stringify(searchBtn.getBoundingClientRect()); } catch (exx) { return exx; } } } catch (ex) { return ex; } return 'noButton'; } FindSearchButton();";
                                string SearchButtonResult = RunScript(findSearchButtonFunc).ToLower();
                                Rect SearchButtonRect = Utils.ParseRect(SearchButtonResult);
                                if (SearchButtonRect.X <= 0 || SearchButtonRect.Y <= 0 || SearchButtonRect.Width <= 0 || SearchButtonRect.Height <= 0)
                                {

                                    LogMng.Instance.onWriteStatus("Can't find Search Button element");
                                    Thread.Sleep(500);

                                    continue;
                                }
                                Page_MouseClick(SearchButtonRect);
                                WaitSpinnerShowing();
                            }

                            bool bSearchFocused = false;
                            int nFindTextBoxRetry = 0;
                            while (nFindTextBoxRetry++ < 3)
                            {
                                Thread.Sleep(300);
                                string isInputFocused = RunScript("document.activeElement == ns_sitesearchlib_ui_header.SearchHeader.Instance.searchInput._active_element");
                                if (isInputFocused.ToLower() == "true")
                                {
                                    bSearchFocused = true;
                                    break;
                                }
                                ////////////////////////////////////////////////////////////////////////////
                                searchTextBoxFunc = "ns_sitesearchlib_ui_header.SearchHeader.Instance.searchInput._active_element.getBoundingClientRect()";
                                searchTextBoxResult = RunScript(searchTextBoxFunc).ToLower();
                                searchTextBoxRect = Utils.ParseRect(searchTextBoxResult);
                                if (searchTextBoxRect.X <= 0 || searchTextBoxRect.Y <= 0 || searchTextBoxRect.Width <= 0 || searchTextBoxRect.Height <= 0)
                                {
#if (TROUBLESHOT)
                                    LogMng.Instance.onWriteStatus("Can't find Search Textbox element");
#endif
                                    Thread.Sleep(500);
                                    continue;
                                }
                                Page_MouseClick(searchTextBoxRect);
                                Thread.Sleep(500);
                                WaitSpinnerShowing();
                            }

                            if (!bSearchFocused)
                            {
                                LogMng.Instance.onWriteStatus("Searching bar is not focused");
                                continue;
                            }
                                

                            Page_KeyboardType(info.homeTeam);
                            Thread.Sleep(500);
                            WaitSpinnerShowing();

                            int nWaitRetryFindResult = 0;
                            while (nWaitRetryFindResult++ < 7)
                            {
                                MoveToTargetSportstabInSearchResult(info);

                                string searchFirstResultFunc = "function GetFindResultList() { var resultlist = []; var CL_List = Locator.treeLookup.getReference(ns_navlib_util.WebsiteNavigationManager.CurrentPageData)._actualChildren; try { for (let i = 0; i < CL_List.length; i++) { try { let CL_item = CL_List[i]; if (CL_item == null || CL_item.nodeName !== 'CL') continue; for (let j = 0; j < CL_item._actualChildren.length; j++) { let EV_item = CL_item._actualChildren[j]; if (EV_item == null || EV_item.nodeName !== 'EV') continue; for (let k = 0; k < EV_item._actualChildren.length; k++) { let MG_item = EV_item._actualChildren[k]; if (MG_item == null || MG_item.nodeName !== 'MG') continue; for (let l = 0; l < MG_item._actualChildren.length; l++) { let MA_item = MG_item._actualChildren[l]; if (MA_item == null || MA_item.nodeName !== 'MA') continue; for (let m = 0; m < MA_item._actualChildren.length; m++) { let PA_item = MA_item._actualChildren[m]; if (PA_item == null || PA_item.nodeName !== 'PA' || PA_item.data == null) continue; if (PA_item.data.BC == null || PA_item.data.NA == null || PA_item.data.PD == null) continue; var resultdata = new Object(); for (let n = 0; n < PA_item._delegateList.length; n++) { if (PA_item._delegateList[n] !== undefined) { resultdata['RECT'] = PA_item._delegateList[n]._active_element.getBoundingClientRect(); break; } } resultdata['CL_NA'] = CL_item.data.NA; resultdata['EV_NA'] = EV_item.data.NA; resultdata['NA'] = PA_item.data.NA; resultdata['PD'] = PA_item.data.PD; resultdata['BC'] = PA_item.data.BC; resultlist.push(resultdata); } } } } } catch (e) { console.log('exception1: ' + e); } } if (resultlist.length <= 0) { return 'NoSearchEvent'; } return resultlist; } catch (e) { console.log('exception2: ' + e); } return 'NoSearchEvent1'; } GetFindResultList();";
                                string searchFirstResultResult = RunScript(searchFirstResultFunc);
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus($"searchList: {searchFirstResultResult}");
#endif
                                Rect searchFirstResultRect = new Rect(0, 0, 0, 0);
                                if (!string.IsNullOrEmpty(searchFirstResultResult))
                                {
                                    try
                                    {
                                        double highsimilarity = 0;
                                        dynamic resultlist = JsonConvert.DeserializeObject<dynamic>(searchFirstResultResult);
                                        foreach (var resultitr in resultlist)
                                        {
                                            //DateTime info_started = DateTime.ParseExact(info.started, "MM/dd/yyyy HH:mm:ss", null);

                                            //if (info.sport.ToLower() != resultitr.CL_NA.ToString().ToLower())
                                            //    continue;

                                            //if (string.IsNullOrEmpty(resultitr.BC.ToString()))
                                            //    continue;
                                            
                                            //DateTime tmpDate = DateTime.ParseExact(resultitr.BC.ToString(), "yyyyMMddHHmmss", null);
                                            //if (info.kind == PickKind.Type_12)
                                            //{
                                            //    DateTime utctmpDate = tmpDate.ToUniversalTime();
                                            //    if (utctmpDate != info_started)
                                            //        continue;
                                            //}
                                            //else
                                            //{
                                            //    tmpDate = tmpDate.AddHours(-1);

                                            //    LogMng.Instance.onWriteStatus($"Time checking: Itr time: {tmpDate} infoTime: {info_started}");

                                            //    if (tmpDate.Minute != info_started.Minute)
                                            //        continue;
                                            //    if (Math.Abs(tmpDate.Subtract(info_started).TotalMinutes) > 100)
                                            //        continue;
                                            //}
                                          
                                            try
                                            {
                                                double similarity = Similarity.GetSimilarityRatio(resultitr.NA.ToString(), searchName, out ratio1, out ratio2);
#if (TROUBLESHOT)
                                        LogMng.Instance.onWriteStatus($"searchItr: {resultitr.NA.ToString()} similarity: {similarity}");
#endif
                                                if (highsimilarity < similarity)
                                                {
                                                    highsimilarity = similarity;
                                                    searchFirstResultRect = Utils.ParseRectFromJson(resultitr.RECT.ToString());
                                                }

                                                if (!string.IsNullOrEmpty(info.siteUrl))
                                                {
                                                    string[] eventUrlSplitList = info.siteUrl.Split(new string[] { "#", "/" }, StringSplitOptions.RemoveEmptyEntries);
                                                    string[] PDList = resultitr.PD.ToString().Split(new string[] { "#", "/" }, StringSplitOptions.RemoveEmptyEntries);

                                                    string EventName1 = "", EventName2 = "";
                                                    foreach (string itr in eventUrlSplitList)
                                                    {
                                                        if (itr.StartsWith("E"))
                                                        {
                                                            EventName1 = itr;
                                                            break;
                                                        }
                                                    }

                                                    foreach (string itr in PDList)
                                                    {
                                                        if (itr.StartsWith("E"))
                                                        {
                                                            EventName2 = itr;
                                                            break;
                                                        }
                                                    }
                                                    if (!string.IsNullOrEmpty(EventName1) && EventName1 == EventName2)
                                                    {
                                                        highsimilarity = 100;
                                                        searchFirstResultRect = Utils.ParseRectFromJson(resultitr.RECT.ToString());
                                                    }
                                                }
                                            }
                                            catch { }
                                        }

                                        if (highsimilarity < 10)
                                            searchFirstResultRect = new Rect(0, 0, 0, 0);

                                    }
                                    catch { }
                                    {

                                    }
                                }

                                int nBrowserheight = Convert.ToInt32(RunScript("window.innerHeight"));
                                if (searchFirstResultRect.X <= 0 || searchFirstResultRect.Y <= 0 || searchFirstResultRect.Width <= 0 || searchFirstResultRect.Height <= 0 || searchFirstResultRect.Y >= nBrowserheight)
                                {
                                    //LogMng.Instance.onWriteStatus($"Can't find event in Result list({nWaitRetryFindResult}) {searchFirstResultResult}");
                                }
                                else
                                {
                                    string origUrl = RunScript("ns_navlib_util.WebsiteNavigationManager.CurrentPageData");
                                    Page_MouseClick(searchFirstResultRect);
                                    Thread.Sleep(500);
                                    if (RunScript("ns_navlib_util.WebsiteNavigationManager.CurrentPageData") != origUrl)
                                        break;
                                }
                            }

                            if (nWaitRetryFindResult >= 7)
                            {
                                if (!string.IsNullOrEmpty(info.siteUrl) && (info.siteUrl.Contains("/") || info.siteUrl.Contains("#")))
                                {
                                    info.siteUrl = info.siteUrl.Replace("https://www.bet365.com/#", "");
                                    string siteUrl = info.siteUrl.Replace("/", "#");
                                    Page_Navigate(siteUrl);
                                    RefreshBecauseBet365Notloading();
                                    Page_Reload();
                                    LogMng.Instance.onWriteStatus($"Go Page by siteUrl directly: {siteUrl}");                                    
                                }
                                else
                                {
                                    if (!GoLinkFromSubKeywords(info))
                                    {

                                        LogMng.Instance.onWriteStatus("Can't find Search Result element finally, ignore bet");

                                        string searchCloseButtonFunc = "ns_sitesearchlib_ui_header.SearchHeader.Instance.closeButton._active_element.getBoundingClientRect()";
                                        string searchCloseButtonResult = RunScript(searchCloseButtonFunc).ToLower();
                                        Rect searchCloseButtonRect = Utils.ParseRect(searchCloseButtonResult);
                                        if (searchCloseButtonRect.X <= 0 || searchCloseButtonRect.Y <= 0 || searchCloseButtonRect.Width <= 0 || searchCloseButtonRect.Height <= 0)
                                        {
#if (TROUBLESHOT)
                                            LogMng.Instance.onWriteStatus("Can't find Search close button");
#endif
                                            Thread.Sleep(500);
                                            continue;
                                        }
                                        Page_MouseClick(searchCloseButtonRect);
                                        if (nTotalRetry == 1)
                                        {
                                            Page_Reload();
                                            Thread.Sleep(500);
                                            WaitSpinnerShowing();
                                        }
                                        continue;
                                    }
                                }
                            }
                                                        
                            WaitSpinnerShowing();
                        }
                    }


                    RefreshBecauseBet365Notloading();

                    if (IsPageNotLoadedInSubPage())
                    {
                        LogMng.Instance.onWriteStatus("Refresh page because of not loaded in Sub Page");
                        Page_Reload();
                        Thread.Sleep(1500);
                        RefreshBecauseBet365Notloading();
                        continue;
                    }

                    if (IsThisPageIsNoLongerAvailable())
                    {
                        LogMng.Instance.onWriteStatus($"Event page is no longer available");
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus("OpenBetEventPageBySearch_Manually exception: " + ex);
            }
            return false;
        }
        private bool OpenBetEventPageBySearch(BetburgerInfo info)
        {         
            bool bOpenPageDirectly = false;
#if (OPENDIRECT)
            bOpenPageDirectly = true;
#endif
            try
            {
                int nTotalRetry = 0;
                while (nTotalRetry++ < 2)
                {
                    string searchName = $"{info.homeTeam} - {info.awayTeam}";
                    string curEventTitle = GetCurrentPageEventTitle();
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"retrying : {nTotalRetry} curEventTitle : {curEventTitle}");
#endif
                    double ratio1, ratio2;

                    double eventSimilarity = Similarity.GetSimilarityRatio(curEventTitle, searchName, out ratio1, out ratio2);
                    if (eventSimilarity >= 90)
                    {
                        LogMng.Instance.onWriteStatus($"Event page is already opened");

                        if (IsThisPageIsNoLongerAvailable())
                        {
                            LogMng.Instance.onWriteStatus($"Event page is no longer available");
                            return false;
                        }

                        //scroll to up                        
                        Scroll(10000);
                        Thread.Sleep(500);                        
                    }
                    else
                    {
                        if (Setting.Instance.domain.Contains(".it") || bOpenPageDirectly || /*DateTime.Now.Subtract(refreshLastTime).TotalMinutes > 5 ||*/ nTotalRetry == 2)
                        {
                            if (info.kind == PickKind.Type_1)
                            {
                                string siteUrl = info.siteUrl.Replace("/", "#");
                                LogMng.Instance.onWriteStatus($"Go Page by siteUrl directly: {siteUrl}");
                                Page_Navigate(siteUrl);
                            }
                            else
                            {
                                LogMng.Instance.onWriteStatus($"can't go event page using team name");
                                return false;
                            }
                        }
                        else
                        {
                            LogMng.Instance.onWriteStatus($"go event page by fast mode {info.homeTeam} - {info.awayTeam}");
                            
                            string origLangId = RunScript("function getLanguageId() { try { var origId = Locator.user.languageId; Locator.user.languageId = '1'; return origId;} catch {} return '1'; } getLanguageId();");
                            string navigationUrl = $"#AX#K^{info.homeTeam}#";
                            Page_Navigate(navigationUrl);
                            WaitSpinnerShowing();

                            MoveToTargetSportstabInSearchResult(info);

                            string searchFirstResultFunc = "function GetFindResultList() { var resultlist = []; var CL_List = Locator.treeLookup.getReference(ns_navlib_util.WebsiteNavigationManager.CurrentPageData)._actualChildren; try { for (let i = 0; i < CL_List.length; i++) { try { let CL_item = CL_List[i]; if (CL_item == null || CL_item.nodeName !== 'CL') continue; for (let j = 0; j < CL_item._actualChildren.length; j++) { let EV_item = CL_item._actualChildren[j]; if (EV_item == null || EV_item.nodeName !== 'EV') continue; for (let k = 0; k < EV_item._actualChildren.length; k++) { let MG_item = EV_item._actualChildren[k]; if (MG_item == null || MG_item.nodeName !== 'MG') continue; for (let l = 0; l < MG_item._actualChildren.length; l++) { let MA_item = MG_item._actualChildren[l]; if (MA_item == null || MA_item.nodeName !== 'MA') continue; for (let m = 0; m < MA_item._actualChildren.length; m++) { let PA_item = MA_item._actualChildren[m]; if (PA_item == null || PA_item.nodeName !== 'PA' || PA_item.data == null) continue; if (PA_item.data.BC == null || PA_item.data.NA == null || PA_item.data.PD == null) continue; var resultdata = new Object(); resultdata['CL_NA'] = CL_item.data.NA; resultdata['EV_NA'] = EV_item.data.NA; resultdata['NA'] = PA_item.data.NA; resultdata['PD'] = PA_item.data.PD; resultdata['BC'] = PA_item.data.BC; resultlist.push(resultdata); } } } } } catch (e) { console.log('exception1: ' + e); } } if (resultlist.length <= 0) { return 'NoSearchEvent'; } return resultlist; } catch (e) { console.log('exception2: ' + e); } return 'NoSearchEvent1'; } GetFindResultList();";
                            string searchFirstResultResult = RunScript(searchFirstResultFunc);

                            string setLangCmd = "function setLanguageId(langId) { try { Locator.user.languageId = langId; } catch {} } ";
                            setLangCmd += $"setLanguageId('{origLangId}');";

                            string Event_PD = "";
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"searchList: {searchFirstResultResult}");
#endif
                            Rect searchFirstResultRect = new Rect(0, 0, 0, 0);
                            try
                            {
                                double highsimilarity = 0;
                                dynamic resultlist = JsonConvert.DeserializeObject<dynamic>(searchFirstResultResult);
                                foreach (var resultitr in resultlist)
                                {
                                    try
                                    {                                        
                                        double similarity = Similarity.GetSimilarityRatio(resultitr.NA.ToString(), searchName, out ratio1, out ratio2);
#if (TROUBLESHOT)
                                        LogMng.Instance.onWriteStatus($"searchItr: {resultitr.NA.ToString()} similarity: {similarity}");
#endif
                                        if (highsimilarity < similarity)
                                        {
                                            highsimilarity = similarity;
                                            Event_PD = resultitr.PD.ToString();     
                                        }
                                    }
                                    catch { }
                                }

                                if (highsimilarity < 50)
                                    Event_PD = "";
                            }
                            catch { }
                            {

                            }

                            if (string.IsNullOrEmpty(Event_PD))
                            {
                                LogMng.Instance.onWriteStatus("Can't find Search Result element finally, ignore bet");                                                             
                                return false;
                            }

                            Page_Navigate(Event_PD);
                            WaitSpinnerShowing();
                        }
                    }

                    
                    RefreshBecauseBet365Notloading();

                    if (IsPageNotLoadedInSubPage())
                    {
                        LogMng.Instance.onWriteStatus("Refresh page because of not loaded in Sub Page");
                        Page_Reload();
                        Thread.Sleep(1500);
                        RefreshBecauseBet365Notloading();
                        continue;
                    }

                    if (IsThisPageIsNoLongerAvailable())
                    {
                        LogMng.Instance.onWriteStatus($"Event page is no longer available");
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus("OpenBetEventPageBySearch exception: " + ex);                
            }
            return false;
        }

        private bool OpenBetEventByEventId_Manually(BetburgerInfo info)
        {
            string param = "#IP#EV" + info.siteUrl + "C1";
            Page_Navigate(param);
            WaitSpinnerShowing();
            return true;
        }
        private bool OpenBetEventByEventId(BetburgerInfo info)
        {
            string param = "#IP#EV" + info.siteUrl + "C1";
            Page_Navigate(param);
            WaitSpinnerShowing();

            string curUrl = RunScript("ns_navlib_util.WebsiteNavigationManager.CurrentPageData");
            LogMng.Instance.onWriteStatus($"OpenBetEventByEventId check Correct Moved EventID: {info.siteUrl} curUrl: {curUrl}");
            if (curUrl.Contains(info.siteUrl))
                return true;
            return false;
        }
        private bool OpenBetEvent(BetburgerInfo info)
        {
            if (info.kind == PickKind.Type_10 || info.kind == PickKind.Type_3 || info.kind == PickKind.Type_7 || info.kind == PickKind.Type_5)
            {
                return EnterEventPageFromLivePage(info);
            }
            //else if (info.kind == PickKind.Type_3)
            //{
            //    if (Setting.Instance.bPlaceFastMode)
            //        return OpenBetEventByEventId(info);
            //    else
            //        return OpenBetEventByEventId_Manually(info);

            //}
            else
            {
                if (Setting.Instance.bPlaceFastMode)
                    return OpenBetEventPageBySearch(info);
                else
                    return OpenBetEventPageBySearch_Manually(info);

            }
            return false;
        }

        private bool FetchBetslip(List<BetburgerInfo> infos, ref List<PROCESS_RESULT> resultList)
        {
            LogMng.Instance.onWriteStatus($"FetchBetslip checking");

            for (int i = 0; i < infos.Count; i++)
            {
                resultList[i] = PROCESS_RESULT.ERROR;
            }

            string betslipListResultFunc = "function GetBetSlipList() { var resultlist = []; try { for (let i = 0; i < BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets.length; i++) { try { let item = BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets[i]; if (item == null || item.model == null || item.model.bet == null || item.model.bet.data == null) continue; resultlist.push(item.model.bet.data); } catch { } } if (resultlist.length <= 0) { return 'NoSlipData'; } return resultlist; } catch { } return 'slipException'; } GetBetSlipList();";
            int nRetry = 0;
            while (nRetry++ < 3)
            {
                string betslipListResultResult = RunScript(betslipListResultFunc).ToLower();

                LogMng.Instance.onWriteStatus($"GetBetSlipList: {betslipListResultResult}");

                dynamic betsliplist = null;

                if (betslipListResultResult == "noslipdata")
                {
                    Thread.Sleep(500);
                    continue;
                }

                try
                {
                    betsliplist = JsonConvert.DeserializeObject<dynamic>(betslipListResultResult);
                }
                catch { }

                if (betsliplist == null || betsliplist.Count != infos.Count)
                {
                    Thread.Sleep(500);
                    continue;
                }
                //if (betsliplist.Count != 2)
                //{
                //    Thread.Sleep(500);
                //    continue;
                //}

                for (int i = 0; i < infos.Count; i++)
                {
                    
                    if (infos[i].formula != "")
                    {
                        foreach (var betslipItr in betsliplist)
                        {
                            try
                            {
                                if (betslipItr.fi == null)
                                    continue;

                                if (betslipItr.pt == null || betslipItr.pt[0] == null || betslipItr.pt[0].pi == null)
                                    continue;

                                if (betslipItr.pt[0].pi.ToString() == infos[i].formula)
                                {
                                    LogMng.Instance.onWriteStatus($"bet is added to betslip: {infos[i].formula}");

                                    if (infos[i].kind == PickKind.Type_6 && betslipItr.pt[0].ha != null)
                                    {
                                        string[] splits = infos[i].direct_link.Split('|');
                                        if (splits.Length == 5)
                                        {
                                            string trade_market = splits[0];
                                            string trade_period = splits[1];
                                            string trade_runnerText = splits[2];
                                            string trade_oddsTypeCondition = splits[3];
                                            string trade_marketText = splits[4];


                                            LogMng.Instance.onWriteStatus($"Value2 checking handicap pick: {trade_oddsTypeCondition} slip: {betslipItr.pt[0].ha.ToString()}");
                                            double line = 0;
                                            double.TryParse(trade_oddsTypeCondition, out line);
                                            if (line != 0)
                                            {
                                                double lineInSlip = 0;
                                                double.TryParse(betslipItr.pt[0].ha.ToString(), out lineInSlip);
                                                if (line != lineInSlip)
                                                {
                                                    LogMng.Instance.onWriteStatus($"Value2 line is different, ignore bet");
                                                    resultList[i] = PROCESS_RESULT.SUSPENDED;
                                                    break;
                                                }
                                            }    
                                        }
                                    }

                                    double newodd = Utils.FractionToDouble(betslipItr.od.ToString());

                                    if (CheckOddDropCancelBet(newodd, infos[i]))
                                        resultList[i] = PROCESS_RESULT.MOVED;
                                    else
                                        resultList[i] = PROCESS_RESULT.SUCCESS;

                                    if (betslipItr.su != null && betslipItr.su.ToString() == "true")
                                        resultList[i] = PROCESS_RESULT.SUSPENDED;

                                    break;
                                }
                            }
                            catch { }
                        }
                    }                    
                }
                return true;
                
            }
            return false;
        }
        private bool AddToBetSlip(BetburgerInfo info, out string fp)
        {
            fp = "";

            if (info.kind == PickKind.Type_2)
            {
                return RunAddLinkToBetSlip(info, out fp);
                //if (Setting.Instance.bPlaceFastMode)
                //    return AddHorseToBetSlip(info, out fp);
                //else
                //    return AddHorseToBetSlip_Manually(info, out fp);
            }
            else
            {
                if (!OpenBetEvent(info))
                {
                    LogMng.Instance.onWriteStatus($"GoOpenBetEventPage {info.homeTeam} - {info.awayTeam} failed");
                    return false;
                }

                int nRetryCount = 0;
                while (nRetryCount++ < 3)
                {
                    wait_AddbetRequestEvent.Reset();
                    if (info.kind == PickKind.Type_10 || info.kind == PickKind.Type_3)
                    {
                        if (!GoMarketAndAddToSlip_LiveSoccerWithDirectLink(info, out fp))
                        {
                            LogMng.Instance.onWriteStatus($"GoMarketAndAddToSlip_LiveSoccerWithDirectLink {info.outcome} - {info.direct_link} failed");
                            return false;
                        }
                    }
                    else if (info.kind == PickKind.Type_9 || info.kind == PickKind.Type_6 || info.kind == PickKind.Type_12 || info.kind == PickKind.Type_13)
                    {
                        if (Setting.Instance.bPlaceFastMode)
                        {
                            if (!GoMarketAndAddToSlip_ParseBet(info, out fp))
                            {
                                LogMng.Instance.onWriteStatus($"GoMarketAndAddToSlip_ParseBet {info.homeTeam} - {info.awayTeam} failed");
                                return false;
                            }
                        }
                        else
                        {
                            if (!GoMarketAndAddToSlip_ParseBet_Manually(info, out fp))
                            {
                                LogMng.Instance.onWriteStatus($"GoMarketAndAddToSlip_ParseBet {info.homeTeam} - {info.awayTeam} failed");
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (Setting.Instance.bPlaceFastMode)
                        {
                            if (!GoMarketAndAddToSlip_OpenBet(info, out fp))
                            {
                                LogMng.Instance.onWriteStatus($"GoMarketAndAddToSlip_OpenBet {info.homeTeam} - {info.awayTeam} failed");
                                return false;
                            }
                        }
                        else
                        {
                            if (!GoMarketAndAddToSlip_OpenBet_Manually(info, out fp))
                            {
                                LogMng.Instance.onWriteStatus($"GoMarketAndAddToSlip_OpenBet {info.homeTeam} - {info.awayTeam} failed");
                                return false;
                            }
                        }
                    }

                    if (wait_AddbetRequestEvent.Wait(2000))
                    {
                        LogMng.Instance.onWriteStatus($"AddToSlip {info.homeTeam} - {info.awayTeam} successed fp: {fp}");
                        return true;
                    }
                    LogMng.Instance.onWriteStatus($"Addbet No request, try again.. {nRetryCount}");
                }
                fp = "";
                return false;
            }
        }

        private bool IsBetSlipOpened()
        {
            string slipVisible = RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip._visible");
            //LogMng.Instance.onWriteStatus($"BetSlip Visible ： {slipVisible}");
            if (slipVisible.ToLower() == "true")
            {
                return true;
            }
            return false;
        }


        private bool InputStakeAndClickBet(double stake, bool bEWCheck, out string placeResult)
        {
            placeResult = "";
            int nTotalRetry = 0;
            Rect iconRectS = new Rect(0, 0, 0, 0);

            double origHandicap = 0;

            string betslipListResultFunc = "function GetBetSlipList() { var resultlist = []; try { for (let i = 0; i < BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets.length; i++) { try { let item = BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets[i]; if (item == null || item.model == null || item.model.bet == null || item.model.bet.data == null) continue; resultlist.push(item.model.bet.data); } catch { } } if (resultlist.length <= 0) { return 'NoSlipData'; } return resultlist; } catch { } return 'slipException'; } GetBetSlipList();";
            string betslipListResultResult = RunScript(betslipListResultFunc).ToLower();

            if (!IsBetSlipOpened())
                return false;

            if (AccountCountryCode.ToLower() == "it")
            {
                ClosePopupMessage();
            }
                
            try
            {
                dynamic betsliplist = JsonConvert.DeserializeObject<dynamic>(betslipListResultResult);


                if (betsliplist[0].fi != null && betsliplist[0].pt != null && betsliplist[0].pt[0] != null && betsliplist[0].pt[0].ha != null)
                {
                    origHandicap = Utils.ParseToDouble(betsliplist[0].pt[0].ha.ToString());

                }

            }
            catch { }
            LogMng.Instance.onWriteStatus($"orig Handicap: {origHandicap}");

            while (nTotalRetry++ < 10)
            {
                if (!IsBetSlipOpened())
                    return false;
                try
                {
                    bool NeedToInputStakeNewly = true;
                    string RemeberStake = RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.footer.stakeBox.rememberStakeButtonNonTouch.selected");
                    LogMng.Instance.onWriteStatus($"Remember Stake ： {RemeberStake}");
                    if (RemeberStake.ToLower() == "true")
                    {
                        string stakeInput = RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.footer.stakeBox.stakeEntered");
                        LogMng.Instance.onWriteStatus($"Remembered Stake ： {stakeInput}");
                        double inputedInStake = Utils.ParseToDouble(stakeInput);
                        if (inputedInStake != 0)
                            NeedToInputStakeNewly = false;
                    }

                    if (NeedToInputStakeNewly)
                    {
                        string stakeposition = RunScript("function GetStakeRect() { var targetPartDiv = BetSlipLocator.betSlipManager.betslip.activeModule.slip.footer.stakeBox.stakeValueInputElement; var rect = targetPartDiv.getBoundingClientRect(); var isVisible = (rect.top >= 0) && (rect.bottom <= window.innerHeight - 30); if (document.elementFromPoint(rect.left + 1, rect.top + 1) != null && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.top + 1) != targetPartDiv) { isVisible = false; } if (document.elementFromPoint(rect.left + 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.bottom - 1) != targetPartDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.top + 1) != null && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.top + 1) != targetPartDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.bottom - 1) != targetPartDiv) { isVisible = false; } if (isVisible) { return JSON.stringify(rect); } return 'Inactive'; } GetStakeRect();");

                        Rect iconRect = Utils.ParseRect(stakeposition);
                        if (iconRect.X <= 0 || iconRect.Y <= 0 || iconRect.Width <= 0 || iconRect.Height <= 0)
                        {
                            stakeposition = RunScript("function GetStakeRect1() { var targetPartDiv = BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.stakeBox.stakeValueInputElement; var rect = targetPartDiv.getBoundingClientRect(); var isVisible = (rect.top >= 0) && (rect.bottom <= window.innerHeight - 30); if (document.elementFromPoint(rect.left + 1, rect.top + 1) != null && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.top + 1) != targetPartDiv) { isVisible = false; } if (document.elementFromPoint(rect.left + 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.bottom - 1) != targetPartDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.top + 1) != null && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.top + 1) != targetPartDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.bottom - 1) != targetPartDiv) { isVisible = false; } if (isVisible) { return JSON.stringify(rect); } return 'Inactive'; } GetStakeRect1();");
                            iconRect = Utils.ParseRect(stakeposition);
                            if (iconRect.X <= 0 || iconRect.Y <= 0 || iconRect.Width <= 0 || iconRect.Height <= 0)
                            {
                                LogMng.Instance.onWriteStatus("No stakebox");

                                Thread.Sleep(300);
                                continue;
                            }
                        }
                        //LogMng.Instance.onWriteStatus($" stakebox pos {iconRect}");
                        if (iconRectS == iconRect)
                        {

                        }
                        else
                        {
                            iconRectS = iconRect;

                            Thread.Sleep(300);
                            continue;
                        }


                        if (iconRect.Width == 1)
                            iconRect.Width = 40;

                        int nRetry = 0;
                        while (nRetry++ < 3)
                        {
                            if (!IsBetSlipOpened())
                                return false;

                            Thread.Sleep(100);
                            Page_MouseClick(iconRect);
                            Thread.Sleep(200);
                            Page_KeyboardType(stake.ToString());

                            string stakeInput = RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.footer.stakeBox.stakeEntered");

                            LogMng.Instance.onWriteStatus($"Stake Write： {stakeInput}");

                            if (string.IsNullOrEmpty(stakeInput))
                            {
                                stakeInput = RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.stakeBox.stakeEntered");

                                LogMng.Instance.onWriteStatus($"Stake Write_1： {stakeInput}");

                            }

                            if (!string.IsNullOrEmpty(stakeInput))
                            {
                                try
                                {
                                    double inputedInStake = Utils.ParseToDouble(stakeInput);
                                    LogMng.Instance.onWriteStatus($"Stake Input cur: {inputedInStake} to: {stake}");
                                    if (inputedInStake != stake)
                                    {
                                        continue;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogMng.Instance.onWriteStatus($"Stake Input Exception: {stakeInput}");
                                }
                                break;
                            }
                            Thread.Sleep(500);
                        }

                        if (nRetry >= 3)
                        {
                            LogMng.Instance.onWriteStatus("Stake Input Failed ");
                            return false;
                        }
                    }
                    if (bEWCheck)
                    {
#if (TROUBLESHOT)
                                        LogMng.Instance.onWriteStatus("Placebet ticking e/w");
#endif

                        RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.model.bets[0].eachwayChecked();");
                     
                        Thread.Sleep(100);
                    }

                    wait_PlacebetResult = "";
                    bool bIgnoreClicking = false;
                    int nRetryPlacebet = 0;
                    while (nRetryPlacebet++ < 8)
                    {
                        if (!IsBetSlipOpened())
                            return false;
                        try
                        {
                            dynamic betsliplist = JsonConvert.DeserializeObject<dynamic>(betslipListResultResult);


                            if (betsliplist[0].fi != null && betsliplist[0].pt != null && betsliplist[0].pt[0] != null && betsliplist[0].pt[0].ha != null)
                            {
                                double curHandicap = Utils.ParseToDouble(betsliplist[0].pt[0].ha.ToString());
                                LogMng.Instance.onWriteStatus($"curHandicap {curHandicap}");

                                if (curHandicap != origHandicap)
                                {
                                    LogMng.Instance.onWriteStatus($"Handicap is changed {origHandicap} -> {curHandicap}");
                                    return false;
                                }
                            }

                        }
                        catch { }

                        if (!bIgnoreClicking)
                        {
                            string PlacebetPosition = RunScript("JSON.stringify(BetSlipLocator.betSlipManager.betslip.activeModule.slip.footer.betWrapper._active_element.getBoundingClientRect())");
                            Rect iconRect = Utils.ParseRect(PlacebetPosition);
                            if (iconRect.X <= 0 || iconRect.Y <= 0 || iconRect.Width <= 0 || iconRect.Height <= 0)
                            {
                                PlacebetPosition = RunScript("JSON.stringify(BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.betPlacement._active_element.getBoundingClientRect())");
                                iconRect = Utils.ParseRect(PlacebetPosition);
                                if (iconRect.X <= 0 || iconRect.Y <= 0 || iconRect.Width <= 0 || iconRect.Height <= 0)
                                {

                                    LogMng.Instance.onWriteStatus("No placebet button ");
                                    return false;
                                }
                            }
                            wait_PlacebetResultEvent.Reset();

                            LogMng.Instance.onWriteStatus($"Placebet clicking placebet button.. {nRetryPlacebet}");

                            int nClickRetry = 0;

                            while (nClickRetry++ < 5)
                            {
                                if (!IsBetSlipOpened())
                                    return false;

                                try
                                {
                                    dynamic betsliplist = JsonConvert.DeserializeObject<dynamic>(betslipListResultResult);


                                    if (betsliplist[0].fi != null && betsliplist[0].pt != null && betsliplist[0].pt[0] != null && betsliplist[0].pt[0].ha != null)
                                    {
                                        double curHandicap = Utils.ParseToDouble(betsliplist[0].pt[0].ha.ToString());
                                        LogMng.Instance.onWriteStatus($"curHandicap {curHandicap}");

                                        if (curHandicap != origHandicap)
                                        {
                                            LogMng.Instance.onWriteStatus($"Handicap is changed {origHandicap} -> {curHandicap}");
                                            return false;
                                        }
                                    }

                                }
                                catch { }

                                wait_PlacebetRequestEvent.Reset();

                                LogMng.Instance.onWriteStatus($"Placebet clicking placebet button for request.. {nClickRetry}");
                                Page_MouseClick(iconRect);
                                if (wait_PlacebetRequestEvent.Wait(500))
                                {
                                    break;
                                }
                            }
                            if (nClickRetry == 5)
                            {
                                LogMng.Instance.onWriteStatus($"Betslip frozen, give up placing");
                                return false;
                            }
                        }
                        //Thread.Sleep(100);
                        //_page.EvaluateExpressionAsync("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.betPlacement.placeBetButton.slipdelegate.acceptOnlyButtonValidate()").Wait();

                        //Thread.Sleep(100);
                        //_page.EvaluateExpressionAsync("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.betPlacement.placeBetButton.slipdelegate.placeBetButtonValidateAndPlaceBet()").Wait();


                        if (bIgnoreClicking || wait_PlacebetResultEvent.Wait(20000))
                        {
                            BetSlipJson betSlipJson = null;
                            if (!string.IsNullOrEmpty(wait_PlacebetResult))
                            {
                                betSlipJson = JsonConvert.DeserializeObject<BetSlipJson>(wait_PlacebetResult);

                                //LogMng.Instance.onWriteStatus($"Placebet Result: {wait_PlacebetResult}");
                                if (betSlipJson.sr == 0 || betSlipJson.sr == 24)
                                {
                                    placeResult = wait_PlacebetResult;
                                    if (AccountCountryCode.ToLower() == "it")
                                    {
                                        int nCloseReceiptSlipRetry = 0;
                                        while (nCloseReceiptSlipRetry++ < 3)
                                        {
                                            if (!IsBetSlipOpened())
                                                return false;

                                            Thread.Sleep(1000);
                                            string appeared = RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.receiptMode").ToLower();
                                            if (appeared == "true")
                                            {
                                                string closeButtonLocation = RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.receiptContent.getActiveElement().lastChild.getBoundingClientRect()").ToLower();
                                                Rect ResultBoxRect = Utils.ParseRect(closeButtonLocation);
                                                if (ResultBoxRect.X > 0 && ResultBoxRect.Y > 0 && ResultBoxRect.Width > 0 && ResultBoxRect.Height > 0)
                                                {
                                                    Page_MouseClick(ResultBoxRect);
                                                    Thread.Sleep(500);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    //try
                                    //{
                                    //    info.odds = Math.Round(Utils.GetOddFromAddbetReply(wait_PlacebetResult), 2, MidpointRounding.ToEven);
                                    //}
                                    //catch { }
                                    Thread.Sleep(500);
                                    //RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.receiptContent.delegate.receiptContentDoneClicked();");
                                    LogMng.Instance.onWriteStatus("Placebet success");
                                    return true;
                                }
                                else if (betSlipJson.sr == 14)
                                {
                                    try
                                    {
                                        double curHandicap = Utils.ParseToDouble(betSlipJson.bt[0].pt[0].ha);
                                        LogMng.Instance.onWriteStatus($"curHandicap {curHandicap}");

                                        if (curHandicap != origHandicap)
                                        {
                                            LogMng.Instance.onWriteStatus($"Handicap is changed {origHandicap} -> {curHandicap}");
                                            return false;
                                        }

                                    }
                                    catch { }
                                    //double newodd = Utils.GetOddFromAddbetReply(wait_PlacebetResult);
                                    //if (CheckOddDropCancelBet(newodd, info))
                                    //    return PROCESS_RESULT.SUSPENDED;
                                }
                                else if (betSlipJson.sr == 70 || betSlipJson.sr == 86)
                                {
                                    int nRetryCount = 0;
                                    while (Global.bRun && nRetryCount++ < 20)
                                    {
                                        Thread.Sleep(2000);
                                        string CheckIfQRExist = RunScript("function getDomElement(tagName, classLabel, outerText = '') { var domArray = []; domArray.push(ns_gen5_ui.Application.currentApplication.getElementChildren()); var counter = 0; while (domArray.length > 0) { try { var curIterator = domArray.shift(); for (var i = 0; i < curIterator.length; i++) { try { if (curIterator[i].tagName.includes(tagName) && curIterator[i].className.includes(classLabel)) { var checkouterText = true; if (outerText != '' && !curIterator[i].outerText.includes(outerText)) { checkouterText = false; } if (checkouterText) { return curIterator[i]; } } domArray.push(curIterator[i].childNodes); } catch (ex1) {} } } catch (ex) {} } return 'noElement'; } getDomElement('DIV', 'atm-AuthenticatorModule')");
                                        LogMng.Instance.onWriteStatus($"Checking QR Popups: {CheckIfQRExist}");
                                        if (CheckIfQRExist.ToLower() == "noelement")
                                        {
                                            bIgnoreClicking = true;
                                            break;
                                        }
                                    }
                                }
                                else
                                {

                                }
                            }

                            Thread.Sleep(1000);
                        }

#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus("Placebet wait timeout ..");
#endif
                    }


                }
                catch (Exception ex)
                {
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Clicking Placebet Exception {ex}");
#endif
                }
                break;
            }
            return false;
        }

        private bool WaitUntilAddBetResponseArrive(string fp)
        {
            if (!wait_AddbetResultEvent.Wait(20000))
            {
                LogMng.Instance.onWriteStatus("Addbet has no response (20 sec), ignore bet");
                return false;
            }

            if (wait_AddbetResult.Contains(fp))
                return true;
            return false;
        }
        private bool AddBetSlipUsingScript(string oddStr, string fd, string i2, string cl)
        {
            MoveToBettablePage();
            //orig checking for only visible element
            string function = "function doaddbet(o, f, fp, c) { for (let i = 0; i < Locator.user._eRegister.oddsChanged.length; i++) { try { let item = Locator.user._eRegister.oddsChanged[i]; if (item == null || item.scope == null || item.scope.twinEmphasizedHandlerType == null) continue; if (item.scope.twinEmphasizedHandlerType.includes(f) && item.scope.twinEmphasizedHandlerType.includes(fp)) { var t = item.scope.getBetItem(); t.updateItem(); BetSlipLocator.betSlipManager.addBet(t, item.scope); return 'market_exist_click'; } } catch (err) {} } var randomItem = null; for (let i = 0; i < Locator.user._eRegister.oddsChanged.length; i++) { let item = Locator.user._eRegister.oddsChanged[i]; if (item == null || item.scope == null || item.scope.twinEmphasizedHandlerType == null) continue; var targetPartDiv = item.scope._active_element; if (!targetPartDiv.className.includes('Suspended')) { var rect = targetPartDiv.getBoundingClientRect(); var isVisible = (rect.top >= 0) && (rect.bottom <= window.innerHeight - 50); if (document.elementFromPoint(rect.left + 1, rect.top + 1) != null && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.top + 1) != targetPartDiv) { isVisible = false; } if (document.elementFromPoint(rect.left + 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.bottom - 1) != targetPartDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.top + 1) != null && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.top + 1) != targetPartDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.bottom - 1) != targetPartDiv) { isVisible = false; } if (isVisible) { randomItem = item; break; } } } if (randomItem == null) { return 'not_found'; } var t = randomItem.scope.getBetItem(); t.odds = o; t.fixtureID = f; t.participantID = fp; t.classificationID = c; t.updateItem(); BetSlipLocator.betSlipManager.addBet(t, randomItem.scope); return 'clicked_any_market'; }";            
            function += $"doaddbet('{oddStr}','{fd}','{i2}','{cl}')";
            string result = RunScript(function);
            LogMng.Instance.onWriteStatus($"AddBetSlip result: {result}");

            if (result == "not_found")
                return false;

            return true;
        }

        bool CheckOddDropCancelBet(double newOdd, BetburgerInfo info)
        {
            if (info.kind == PickKind.Type_7)
            {
                LogMng.Instance.onWriteStatus($"Odd checking odd in pick: {info.odds} new odd: {newOdd}");
                if (newOdd > Setting.Instance.maxOddsSoccerLive || newOdd < Setting.Instance.minOddsSoccerLive)
                {
                    LogMng.Instance.onWriteStatus($"Ignore bet because of odd is out of range, new odd: {newOdd}");
                    return true;
                }
            }
            else if (info.kind == PickKind.Type_1 || info.kind == PickKind.Type_8 || info.kind == PickKind.Type_9)
            {
                LogMng.Instance.onWriteStatus($"Odd checking odd in pick: {info.odds} new odd: {newOdd}");
                if (newOdd > Setting.Instance.maxOddsSports || newOdd < Setting.Instance.minOddsSports)
                {
                    LogMng.Instance.onWriteStatus($"Ignore bet because of odd is out of range, new odd: {newOdd}");
                    return true;
                }
            }

            if (Setting.Instance.bAllowOddDrop)
            {
                LogMng.Instance.onWriteStatus($"Odd checking odd in pick: {info.odds} new odd: {newOdd}");
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

        int mouseSpeed = 40;
        void MoveMouse(int x, int y, int rx = 5, int ry = 5)
        {

            last_x = System.Windows.Forms.Cursor.Position.X;
            last_y = System.Windows.Forms.Cursor.Position.Y;

            x += rnd.Next(rx);
            y += rnd.Next(ry);

            double randomSpeed = Math.Max((rnd.Next(mouseSpeed) / 2.0 + mouseSpeed) / 10.0, 0.1);

            WindMouse(last_x, last_y, x, y, 9.0, 3.0, 10.0 / randomSpeed,
                15.0 / randomSpeed, 10.0 * randomSpeed, 10.0 * randomSpeed);
        }

        void WindMouse(double xs, double ys, double xe, double ye,
            double gravity, double wind, double minWait, double maxWait,
            double maxStep, double targetArea)
        {

            double dist, windX = 0, windY = 0, veloX = 0, veloY = 0, randomDist, veloMag, step;
            int oldX, oldY, newX = (int)Math.Round(xs), newY = (int)Math.Round(ys);

            double waitDiff = maxWait - minWait;
            double sqrt2 = Math.Sqrt(2.0);
            double sqrt3 = Math.Sqrt(3.0);
            double sqrt5 = Math.Sqrt(5.0);

            dist = Hypot(xe - xs, ye - ys);

            while (dist > 1.0)
            {

                if (!Global.bRun)
                    return;
                wind = Math.Min(wind, dist);

                if (dist >= targetArea)
                {
                    int w = rnd.Next((int)Math.Round(wind) * 2 + 1);
                    windX = windX / sqrt3 + (w - wind) / sqrt5;
                    windY = windY / sqrt3 + (w - wind) / sqrt5;
                }
                else
                {
                    windX = windX / sqrt2;
                    windY = windY / sqrt2;
                    if (maxStep < 3)
                        maxStep = rnd.Next(3) + 3.0;
                    else
                        maxStep = maxStep / sqrt5;
                }

                veloX += windX;
                veloY += windY;
                veloX = veloX + gravity * (xe - xs) / dist;
                veloY = veloY + gravity * (ye - ys) / dist;

                if (Hypot(veloX, veloY) > maxStep)
                {
                    randomDist = maxStep / 2.0 + rnd.Next((int)Math.Round(maxStep) / 2);
                    veloMag = Hypot(veloX, veloY);
                    veloX = (veloX / veloMag) * randomDist;
                    veloY = (veloY / veloMag) * randomDist;
                }

                oldX = (int)Math.Round(xs);
                oldY = (int)Math.Round(ys);
                xs += veloX;
                ys += veloY;
                dist = Hypot(xe - xs, ye - ys);
                newX = (int)Math.Round(xs);
                newY = (int)Math.Round(ys);

                if (oldX != newX || oldY != newY)
                    MouseMoveInScreen(newX, newY);

                step = Hypot(xs - oldX, ys - oldY);
                int wait = (int)Math.Round(waitDiff * (step / maxStep) + minWait);
                Thread.Sleep(wait);
            }

            int endX = (int)Math.Round(xe);
            int endY = (int)Math.Round(ye);
            if (endX != newX || endY != newY)
                MouseMoveInScreen(endX, endY);
        }

        static double Hypot(double dx, double dy)
        {
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public PROCESS_RESULT PlaceBet(ref BetburgerInfo info)
        {
            if (info.kind == PickKind.Type_5)
            {
                OpenBet_Bet365 openbet = Utils.ConvertBetburgerPick2OpenBet_365(info);
                if (openbet == null)
                {
                    return PROCESS_RESULT.ERROR;
                }
                
                CloseBetSlip();

                if (!login())
                {
                    LogMng.Instance.onWriteStatus("Placebet failed because of login failure.");
                    return PROCESS_RESULT.NO_LOGIN;
                }
            
                string placebetResult = "";
                
                if (InputStakeAndClickBet(info.stake, false, out placebetResult))
                {                   
                    return PROCESS_RESULT.PLACE_SUCCESS;
                }
                
                return PROCESS_RESULT.SUSPENDED;
            }
            return PROCESS_RESULT.ERROR;
        }

        public PROCESS_RESULT PlaceBet(List<BetburgerInfo> infos, out List<PROCESS_RESULT> result)
        {
            Thread.Sleep(1000 * Setting.Instance.requestDelay);
            LogMng.Instance.onWriteStatus($"Bets count: {infos.Count} ---starting---");
#if (TROUBLESHOT)

#endif
            //Getting Cursor Position for Scrolling           
            try
            {
                RECT rect = new RECT();

                GetWindowRect(Global.ViewerHwnd, ref rect);

                LogMng.Instance.onWriteStatus($"GetScrollPos hwnd: {Global.ViewerHwnd} rect: {rect.Left},{rect.Top} - {rect.Right},{rect.Bottom}");

                scrollx = (rect.Right - rect.Left) / 2 - 25 + rnd.Next(1, 50);
                scrolly = (rect.Bottom - rect.Top) / 2 - 25 + rnd.Next(1, 50);
            }
            catch (Exception ex)
            {
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Get scroll Cursor Position Exception {ex}");
#endif
            }

            List<PROCESS_RESULT> betslipCheckResult = new List<PROCESS_RESULT>();
            for (int i = 0; i < infos.Count; i++)
            {
                betslipCheckResult.Add(PROCESS_RESULT.ERROR);
            }

            try
            {
                Monitor.Enter(locker);
                CloseBetSlip();
                result = null;

                if (!login())
                {
                    LogMng.Instance.onWriteStatus("Placebet failed because of login failure.");
                    return PROCESS_RESULT.NO_LOGIN;
                }
                               

                for (int i = 0; i < infos.Count; i++)
                {
                    string fp = "";
                    wait_AddbetResult = "";
                    wait_AddbetResultEvent.Reset();
                    if (Setting.Instance.bPlaceFastMode && infos[i].kind == PickKind.Type_5)
                    {
                        OpenBet_Bet365 openbet = Utils.ConvertBetburgerPick2OpenBet_365(infos[i]);
                        if (openbet != null)
                        {
                            if (AddBetSlipUsingScript(openbet.betData[0].oddStr, openbet.betData[0].fd, openbet.betData[0].i2, openbet.betData[0].cl))
                                fp = openbet.betData[0].i2;
                        }
                    }
                    else
                    {
                        if (AddToBetSlip(infos[i], out fp) == false)
                        {
                            //check if it's usable for Italian accounts
                            //OpenBet_Bet365 openbet = Utils.ConvertBetburgerPick2OpenBet_365(infos[i]);
                            //if (openbet != null)
                            //{                                
                            //    if (AddBetSlipUsingScript(openbet.betData[0].oddStr, openbet.betData[0].fd, openbet.betData[0].i2, openbet.betData[0].cl))
                            //        fp = openbet.betData[0].i2;
                            //}
                        }
                    }

                    infos[i].formula = fp;
                    if (!string.IsNullOrEmpty(fp))
                    {
                        if (infos[i].kind == PickKind.Type_2 && infos[i].siteUrl.ToLower().Contains("https://"))
                        {
                            Thread.Sleep(100);
                        }
                        else
                        {
                            WaitUntilAddBetResponseArrive(fp);
                        }
                    }
                }
                Thread.Sleep(500);
                try
                {
                    //checking betslip and prepare betslipCheckResult
                    FetchBetslip(infos, ref betslipCheckResult);
                }
                catch { }

                int nApprovedCount = 0;

                for (int i = 0; i < betslipCheckResult.Count; i++)
                {
                    if (betslipCheckResult[i] == PROCESS_RESULT.SUCCESS)
                    {
                        nApprovedCount++;
                    }
                    else if (betslipCheckResult[i] == PROCESS_RESULT.ERROR || betslipCheckResult[i] == PROCESS_RESULT.MOVED || betslipCheckResult[i] == PROCESS_RESULT.SUSPENDED)
                    {
                        OpenBet_Bet365 openbet = Utils.ConvertBetburgerPick2OpenBet_365(infos[i]);
                        if (openbet != null)
                        {
                            LogMng.Instance.onWriteStatus($"Removing bet {openbet.betData[0].i2} from Slip because of reason: {betslipCheckResult[i]}");
                            CloseBetSlipInner(openbet.betData[0].i2);
                        }
                        else
                        {
                            LogMng.Instance.onWriteStatus($"Removing bet from Slip because of odd dropping(suspended) but OpenBet parsing error");
                        }
                    }
                }


                if (nApprovedCount == infos.Count)
                //if (nApprovedCount != 0) //it will allow 1 bet also
                {
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus("Placebet inputing stake");
#endif
                    string placebetResult = "";
                    bool bEWBet = false;
                    if (infos[0].sport == "Horse Racing" && Setting.Instance.bEachWay && infos[0].odds >= Setting.Instance.eachWayOdd)
                    {
                        bEWBet = true;
                    }
                    if (InputStakeAndClickBet(infos[0].stake, bEWBet, out placebetResult))
                    {
                        //BetSlipJson betSlipJson = JsonConvert.DeserializeObject<BetSlipJson>(placebetResult);
                        //foreach (BetSlipItem bt in betSlipJson.bt)
                        //{
                        //    LogMng.Instance.onWriteStatus($"Placebet result parsing bt.fi: {bt.pt[0].pi}");
                        //    for (int i = 0; i < infos.Count; i++)
                        //    {
                        //        OpenBet_Bet365 openbet = Utils.ConvertBetburgerPick2OpenBet_365(infos[i]);
                        //        if (openbet == null)
                        //        {
                        //            LogMng.Instance.onWriteStatus($"Placebet result checking i: {i} openbet: null");
                        //        }
                        //        else
                        //        {
                        //            LogMng.Instance.onWriteStatus($"Placebet result checking i: {i} openbet: {openbet.betData[0].i2}");
                        //        }

                        //        if (openbet != null && openbet.betData[0].i2 == bt.pt[0].pi)
                        //        {
                        //            betslipCheckResult[i] = PROCESS_RESULT.PLACE_SUCCESS;
                        //        }
                        //    }
                        //}
                        result = betslipCheckResult;
                        if (Setting.Instance.bTipster2 || Setting.Instance.bSoccerLive)
                        {
                            Page_Navigate("#IP#B1");
                        }
                        return PROCESS_RESULT.PLACE_SUCCESS;
                    }

                    for (int i = 0; i < betslipCheckResult.Count; i++)
                    {
                        betslipCheckResult[i] = PROCESS_RESULT.SUSPENDED;
                    }
                }
            }
            catch { }
            finally
            {
                Monitor.Exit(locker);
            }
            result = betslipCheckResult;
            return PROCESS_RESULT.ERROR;
        }


        public string getPendingBalance()
        {
            Global.balance = getBalance();
            try
            {

                Monitor.Enter(locker);
                string curPendingCount = RunScript("function getMemberIconRect(classLabel) { var domArray = []; domArray.push(ns_gen5_ui.Application.currentApplication.getElementChildren()); while (domArray.length > 0) { try { var curIterator = domArray.shift(); for (var i = 0; i < curIterator.length; i++) { try { if (curIterator[i].className.includes(classLabel)) { return curIterator[i].outerText; } domArray.push(curIterator[i].childNodes); } catch (ex1) { } } } catch (ex) { } } return '0'; } getMemberIconRect('hm-HeaderMenuItemMyBets_MyBetsCount ');");
                if (curPendingCount != "0")
                {
                    //Rect logoutRect = GetDomRectFromClass("hm-HeaderMenuItemMyBets ");

                    //if (logoutRect.X > 0 && logoutRect.Y > 0 && logoutRect.Width > 0 && logoutRect.Height > 0)
                    //{
                    //    Page_MouseClick(logoutRect);                        
                    //}
                    //else
                    //{
                    //    return $"Current Balance: {Global.balance} Pending: Error [No Openbets button]";
                    //}

                    int nRetry = 0;
                    while (nRetry++ < 2)
                    {
                        var command = string.Format("(function () {{ {0} }})();", Global.GetOpenBetListCommandLine);
                        string json = RunScript(command).ToLower();

                        int nPendingbetCount = 0;
                        double lPendingbetBalance = 0;

                        try
                        {
                            List<OpenBet_Bet365> curBetList = Utils.ParseBet365OpenBets(json);

                            foreach (OpenBet_Bet365 bet in curBetList)
                            {
                                nPendingbetCount++;
                                lPendingbetBalance += bet.stake;

                                foreach (BetData_Bet365 betData in bet.betData)
                                {
                                    if (betData.eachway)
                                    {
                                        lPendingbetBalance += bet.stake;
                                        break;
                                    }
                                }

                            }
                        }
                        catch { }
                        if (nPendingbetCount == 0)
                        {
                            Thread.Sleep(500);
                            continue;
                        }
                        Global.TotalBalance = Global.balance + lPendingbetBalance;
                        return $"Current Balance: {Global.balance} Pending: {lPendingbetBalance}({nPendingbetCount} bets) Total: {Global.balance + lPendingbetBalance}";
                    }
                }
            }
            catch { }
            finally {
                Monitor.Exit(locker);
            }
            return $"Current Balance: {Global.balance} Pending: No pending";
        }
        public double getBalance()
        {
            int nRetry = 0;
            double result = -1;
            try
            {
                Monitor.Enter(locker);


                

                while (nRetry++ < 2)
                {
                    //PageClick("div.hm-MainHeaderMembersWide_MembersMenuIcon");
                    //Thread.Sleep(500);
                    //PageClick("div.um-BalanceRefreshButton_Icon");
                    //Thread.Sleep(1000);
                    try
                    {
                        result = Utils.ParseToDouble(RunScript("function getUserBalance() { try { return Locator.user.getBalance().totalBalance; } catch {} return ''; } getUserBalance();"));
                    }
                    catch
                    {

                    }
                                        
                }
            }
            catch { }
            finally
            {
                Monitor.Exit(locker);
            }

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"getBalance: {result}");
#endif

            return result;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_MAXIMIZE = 3;

        private DateTime lastSendingBalance = DateTime.MinValue;

        int nCloseBetSlipCheckCount = 0;
        public void Feature()
        {

            if (DateTime.Now.Subtract(lastSendingBalance).TotalMinutes < 120)
            {
                //    if (nCloseBetSlipCheckCount++ > 5)
                //    {
                //        try
                //        {
                //            Monitor.Enter(locker);
                //            CloseBetSlip();

                //        }
                //        catch { }
                //        finally
                //        {     
                //            Monitor.Exit(locker);
                //        }
                //        nCloseBetSlipCheckCount = 0;
                //    }
                return;

            }

            string curDetailBalance = getPendingBalance();
            UserMng.GetInstance().SendClientMessage(curDetailBalance);
            lastSendingBalance = DateTime.Now;
        }

        private void MoveToBettablePage()
        {
            Scroll(10000);
            Thread.Sleep(500);
            
            string function = "function CanImmediateBet() { var randomItem = null; for (let i = 0; i < Locator.user._eRegister.oddsChanged.length; i++) { let item = Locator.user._eRegister.oddsChanged[i]; if (item == null || item.scope == null || item.scope.twinEmphasizedHandlerType == null) continue; var targetPartDiv = item.scope._active_element; if (!targetPartDiv.className.includes('Suspended')) { var rect = targetPartDiv.getBoundingClientRect(); var isVisible = (rect.top >= 0) && (rect.bottom <= window.innerHeight - 50); if (document.elementFromPoint(rect.left + 1, rect.top + 1) != null && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.top + 1) != targetPartDiv) { isVisible = false; } if (document.elementFromPoint(rect.left + 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.bottom - 1) != targetPartDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.top + 1) != null && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.top + 1) != targetPartDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.bottom - 1) != targetPartDiv) { isVisible = false; } if (isVisible) { randomItem = item; break; } } } if (randomItem === null) { return false; } return true; } CanImmediateBet();";
            string result = RunScript(function);
            if (result.ToLower() == "false")
            {
                Page_Navigate("#HO#");
                WaitSpinnerShowing();
            }
        }
        public bool Pulse()
        {
//            if (IsPageLoginStatus())
//            {


//                PageClick($"div.hm-HeaderMenuItemMyBets");
//                Thread.Sleep(2000);
//                var command = string.Format("(function () {{ {0} }})();", Global.GetOpenBetListCommandLine);

//                String result = Utils.ParseOpenBet(RunScript(command));
//#if (TROUBLESHOT)
//                LogMng.Instance.onWriteStatus(result);
//#endif
//                Application.Current.Dispatcher.Invoke(new Action(() =>
//                {
//                    MessageBox.Show(result);
//                }));
//            }
            return true;
        }

        private void PageManagerProc()
        {
            while (true)
            {
                if (!Global.bRun || Global.IsRestStatus)
                {
                
                    Thread.Sleep(200);
                    continue;                
                }

                if (login())
                    Global.balance = getBalance();
//#if (!DEBUG)
                ClosePopupMessage();
//#endif
                //CloseTemaviewerPopup();

                Thread.Sleep(2000);

                //if (!string.IsNullOrEmpty(QRGetRefreshToken))
                //{
                //    if (timeToSendQRGetRequest <= DateTime.Now)
                //    {
                //        timeToSendQRGetRequest = DateTime.MaxValue;
                //        UserMng.GetInstance().SendQRRequest(1, QRGetRefreshToken);
                //    }
                //}
            }
        }
               
    }
#endif
}

//modal popup queue
//ns_webconsolelib_util.ModalManager.ModalQueue

//running query
//Locator.validationManager.callLater
//Locator.validationManager.callNewContext

//Locator.treeLookup.getReference('#AX#K^real#') //searching with real keyword
//Locator.treeLookup.getReference('InPlay_1_3')

//ns_navlib_util.WebsiteNavigationManager.CurrentPageData

//Locator.validationManager.callLaterOrig = Locator.validationManager.callLater;
//Locator.validationManager.callLater = function(e) {
//    console.trace();
//    return Locator.validationManager.callLaterOrig(e);
//};

//"lid=" + Locator.user.languageId + "&zid=" + Locator.user.zoneId + "&pd=" + escape(t) + "&cid=" + Locator.user.countryId)

//OVInPlay_1_3
