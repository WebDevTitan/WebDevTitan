namespace Project.Bookie
{
#if (BETWAY_ADDON)

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
            Betway_ADDONCtrl.bWebSocketConnected = false;
        }

        protected override void OnError(ErrorEventArgs e)
        {
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"[WebSocket:OnError] {e.Message}");
#endif
            Betway_ADDONCtrl.bWebSocketConnected = false;
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            Betway_ADDONCtrl.bWebSocketConnected = true;
#if (TROUBLESHOT)
            //LogMng.Instance.onWriteStatus($"[WebSocket:OnMessage] {e.Data}");
#endif

            try
            {
                var responseJson = JsonConvert.DeserializeObject<dynamic>(e.Data);
                if (responseJson.type.ToString() == "webrequest")
                {
                    if (responseJson.url.ToString().ToLower().Contains("/api/account/v3/login"))
                    {
                        Betway_ADDONCtrl.wait_BrowserRequestResult = responseJson.response.ToString();
                        //#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"{responseJson.url.ToString()} Request: {Betway_ADDONCtrl.wait_BrowserRequestResult}");
                        //#endif
                        Betway_ADDONCtrl.wait_BrowserRequestEvent.Set();
                    }                   
                    else if (responseJson.url.ToString().ToLower().Contains("/api/account/v3/info"))
                    {
                        try
                        {
                            JObject newpayload = JsonConvert.DeserializeObject<JObject>(responseJson.response.ToString());
                            if (newpayload["SessionId"] != null && (Betway_ADDONCtrl.payloadObject == null || Betway_ADDONCtrl.payloadObject["SessionId"] == null || Betway_ADDONCtrl.payloadObject["SessionId"] != newpayload["SessionId"]))
                            {
                                Betway_ADDONCtrl.payloadObject = newpayload;
                            }
                        }
                        catch { }
                        Betway_ADDONCtrl.wait_BrowserRequestEvent.Set();
                    }
                }
                else if (responseJson.type.ToString() == "webresponse")
                {
                    if (responseJson.url.ToString().ToLower().Contains("/api/account/v3/login"))
                    {
                        Betway_ADDONCtrl.wait_BrowserResponseResult = responseJson.response.ToString();
                        Betway_ADDONCtrl.wait_BrowserResponseEvent.Set();
                    }
                    else if (responseJson.url.ToString().ToLower().Contains(Betway_ADDONCtrl.wait_RequestedUrl.ToLower()))
                    {
                        Betway_ADDONCtrl.wait_EvalResult = responseJson.response.ToString();
                        Betway_ADDONCtrl.wait_EvalResultEvent.Set();

                        LogMng.Instance.onWriteStatus($"{responseJson.url.ToString()} Response: {Betway_ADDONCtrl.wait_EvalResult}");
                    }
                }
                else if (responseJson.type.ToString() == "scriptresult")
                {
                    Betway_ADDONCtrl.wait_EvalResult = responseJson.response.ToString();
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Script Res: {Betway_ADDONCtrl.wait_EvalResult}");
#endif
                    Betway_ADDONCtrl.wait_EvalResultEvent.Set();
                }
            }
            catch { }
        }

        protected override void OnOpen()
        {
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"[WebSocket:OnOpen]");
#endif
            Betway_ADDONCtrl.bWebSocketConnected = true;
        }
    }

    class Betway_ADDONCtrl : IBookieController
    {
        private Thread pageThread = null;
        public HttpClient m_client = null;
        
        Object lockerObj = new object();
        //private static ProxyServer _proxyServer = null;

        public static JObject payloadObject = null;

        public static bool bWebSocketConnected = false;
        public static ManualResetEventSlim wait_EvalResultEvent = new ManualResetEventSlim();
        public static string wait_EvalResult = string.Empty;
        public static string wait_RequestedUrl = string.Empty;

        
        public static ManualResetEventSlim wait_BrowserRequestEvent = new ManualResetEventSlim();
        public static string wait_BrowserRequestResult = string.Empty;

        public static ManualResetEventSlim wait_BrowserResponseEvent = new ManualResetEventSlim();
        public static string wait_BrowserResponseResult = string.Empty;

        private DateTime refreshLastTime = DateTime.MinValue;
        public void Close()
        {
            if (pageThread != null)
                pageThread.Abort();
        }

        public void Feature()
        {

        }

        public int GetPendingbets()
        {
            return 0;
        }
        public bool logout()
        {
            return true;
        }

#if (CHROME || EDGE)
        public static int titlebarHeight = 75;
#elif (FIREFOX)
        public static int titlebarHeight = 84;
#endif

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
        Random rnd = new Random();
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

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(SystemMetric smIndex);

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
        static double Hypot(double dx, double dy)
        {
            return Math.Sqrt(dx * dx + dy * dy);
        }
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

        public Betway_ADDONCtrl()
        {

            m_client = initHttpClient();
            pageThread = new Thread(PageManagerProc);
            pageThread.Start();
            //Global.SetMonitorVisible(false);
        }

        public bool Pulse()
        {
            if (getBalance() < 0)
                return false;
            return true;
        }

        public bool SendEvalCommand(string type, string message)
        {
            if (string.IsNullOrEmpty(type))
                return true;

#if (TROUBLESHOT)
            //LogMng.Instance.onWriteStatus($"SendEvalCommand {message}");
#endif
            //string request = $"{{\"type\":\"{type}\", \"body\":\"{message}\"}}";
            string request = message;
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
                LogMng.Instance.onWriteStatus($"Page_Evaluate Req  cmd : {command} exp: {expression}");
#endif
                wait_EvalResultEvent.Reset();
                wait_EvalResult = string.Empty;
                SendEvalCommand(command, expression);

                int nWaitSec = 1000;
                if (expression.ToLower().Contains("window.fetch"))
                    nWaitSec = 10000;
                
                if (!wait_EvalResultEvent.Wait(nWaitSec))
                {
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Page_Evaluate No Response");
#endif

                    return string.Empty;
                }
                result = wait_EvalResult;
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Page_Evaluate Res : {result}");
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

        private string RunScript(string param)
        {
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"RunScript: {param}");
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
        public bool login()
        {
            //if it's already login, let's refresh page and catch object.
            if (payloadObject == null)
            {
                string sessionvalue = RunScript("document.getElementsByClassName('refreshBalanceIcon icon-refresh')[0].outerHTML ");
                if (sessionvalue.ToLower().Contains("class"))
                {
                    wait_BrowserRequestEvent.Reset();
                    Page_Goto($"https://{Setting.Instance.domain}/en/sports");
                    wait_BrowserRequestEvent.Wait(10000);
                    LogMng.Instance.onWriteStatus("capturing object finished");
                }
            }
            if (getBalance() >= 0)
                return true;
            try
            {
                Monitor.Enter(locker);

                WaitUntilWebSocketConnect();
                CatchBrowser();
                //ShowWindow(Global.ViewerHwnd, SW_MAXIMIZE);
                //Page_Evaluate("maximize", "");
    


                //Page_Evaluate("domain", Setting.Instance.domain);


                LogMng.Instance.onWriteStatus("Login start");
                Page_Goto($"https://{Setting.Instance.domain}/en/sports");


                int nTotalRetry = 0;
                while (nTotalRetry++ < 3)
                {
                    if (!Global.bRun)
                        return false;

                    int n_waitRetry = 10;
                    while (n_waitRetry-- >= 0)
                    {
                        string style = RunScript("document.querySelectorAll(\"input[placeholder='Username']\")[0].outerHTML");
                        if (style.Contains("input"))
                            break;
                        Thread.Sleep(1000);
                    }
                    if (n_waitRetry <= 0)
                        return false;

                    Thread.Sleep(1000);
                    RunScript($"document.querySelectorAll(\"input[placeholder='Username']\")[0].value='{Setting.Instance.username}';");

                    RunScript($"document.querySelectorAll(\"input[placeholder='Password']\")[0].value='{Setting.Instance.password}';");

                    Thread.Sleep(1000);
                    wait_BrowserRequestEvent.Reset();
                    wait_BrowserResponseEvent.Reset();
                    wait_BrowserRequestResult = "";
                    wait_BrowserResponseResult = "";
                    if (Setting.Instance.domain.ToLower().Contains(".it"))
                        RunScript("cg_login();");
                    else
                        RunScript("document.querySelectorAll(\"input[class='loginSubmit']\")[0].click();");

                    if (!wait_BrowserRequestEvent.Wait(500))
                    {
                        LogMng.Instance.onWriteStatus($"Login No Request");
                        continue;
                    }

                    if (string.IsNullOrEmpty(wait_BrowserRequestResult))
                    {
                        LogMng.Instance.onWriteStatus($"Login No Capture request");
                        continue;
                    }

                    payloadObject = JsonConvert.DeserializeObject<JObject>(wait_BrowserRequestResult);

                    if (!wait_BrowserResponseEvent.Wait(5000))
                    {
                        payloadObject = null;
                        LogMng.Instance.onWriteStatus($"Login No Response");
                        continue;
                    }

                    dynamic loginResponse = JsonConvert.DeserializeObject<dynamic>(wait_BrowserResponseResult);
                    payloadObject["SessionId"] = loginResponse.Login.SessionId.ToString();
                    int nCheckAgain = 0;
                    while (nCheckAgain++ < 3)
                    {
                        Thread.Sleep(5000);

                        if (getBalance() >= 0)
                            return true;
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
                wait_RequestedUrl = $"https://sportsapi.{Setting.Instance.domain}/api/Events/v2/GetEventMarkets";

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

                string functionString = $"window.fetch('{wait_RequestedUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json; charset=UTF-8', 'accept': 'application/json; charset=UTF-8' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";
                                                
                string strWebResponse1 = RunScript(functionString);
                wait_RequestedUrl = "";
                LogMng.Instance.onWriteStatus($"GetEventMarkets Req: {formDataString}");
                LogMng.Instance.onWriteStatus($"GetEventMarkets Res: {strWebResponse1}");
                string OutcomeId = "", MarketId = "", EventId = "";
                dynamic getEventMarket_Response = JsonConvert.DeserializeObject<dynamic>(strWebResponse1);
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
                wait_RequestedUrl = $"https://sportsapi.{Setting.Instance.domain}/api/Betting/v3/BuildBets";

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

                functionString = $"window.fetch('{wait_RequestedUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json; charset=UTF-8', 'accept': 'application/json; charset=UTF-8' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                string strWebResponse2 = RunScript(functionString);
                wait_RequestedUrl = "";
                
                LogMng.Instance.onWriteStatus($"BuildBets Req: {formDataString}");
                LogMng.Instance.onWriteStatus($"BuildBets Res: {strWebResponse2}");
                dynamic buildbets_Response = JsonConvert.DeserializeObject<dynamic>(strWebResponse2);
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

                wait_RequestedUrl = $"https://sportsapi.{Setting.Instance.domain}/api/Betting/v3/InitiateBets";
                                
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
                functionString = $"window.fetch('{wait_RequestedUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json; charset=UTF-8', 'accept': 'application/json; charset=UTF-8' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                string strWebResponse3 = RunScript(functionString);
                wait_RequestedUrl = "";

                LogMng.Instance.onWriteStatus($"InitiateBets Req: {formDataString}");
                LogMng.Instance.onWriteStatus($"InitiateBets Res: {strWebResponse3}");
                dynamic initiatebet_Response = JsonConvert.DeserializeObject<dynamic>(strWebResponse3);
                
                if (initiatebet_Response.Success.ToString().ToLower() != "true")
                {
                    LogMng.Instance.onWriteStatus(initiatebet_Response.MethodResult.ToString());
                    return PROCESS_RESULT.ERROR;
                }

                int nLookupRetry = 30;
                while (nLookupRetry-- > 0)
                {  
                    Thread.Sleep(1000);

                    wait_RequestedUrl = $"https://sportsapi.{Setting.Instance.domain}/api/Betting/v3/LookupBets";

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

                    functionString = $"window.fetch('{wait_RequestedUrl}', {{ headers: {{ accept: 'application/json, text/plain, */*', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                    string strWebResponse4 = RunScript(functionString);
                    wait_RequestedUrl = "";
                    
                    LogMng.Instance.onWriteStatus($"LookupBets Req: {formDataString}");
                    LogMng.Instance.onWriteStatus($"LookupBets Res: {strWebResponse4}");
                    dynamic lookupbets_Response = JsonConvert.DeserializeObject<dynamic>(strWebResponse4);

                    if (lookupbets_Response.BetStatus.ToString() == "3")
                    {
                        return PROCESS_RESULT.PLACE_SUCCESS;
                        
                    }

                    if (lookupbets_Response.BetStatus.ToString() != "2" || lookupbets_Response.Success.ToString().ToLower() == "false")
                    {
                        try
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

                            if (lookupbets_Response.Errors[0].ErrorCode.ToString() == "302")
                            {
                                LogMng.Instance.onWriteStatus($"[Odd error]: {lookupbets_Response.Errors[0].Message.ToString()}");                                
                                return PROCESS_RESULT.ERROR;
                            }
                            else if (lookupbets_Response.Errors[0].ErrorCode.ToString() == "305")
                            {
                                LogMng.Instance.onWriteStatus($"[Stake error]: {lookupbets_Response.Errors[0].Message.ToString()}");
                                return PROCESS_RESULT.CRITICAL_SITUATION;
                            }
                        }
                        catch {
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
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"GetBalance start object set?: {payloadObject != null}");
#endif

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

                wait_RequestedUrl = $"https://sportsapi.{Setting.Instance.domain}/api/Account/v3/Info";
               
                string formDataString = request.ToString().Replace("\r","").Replace("\n", "");
                string functionString = $"window.fetch('{wait_RequestedUrl}', {{ headers: {{ accept: 'application/json, text/plain, */*', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                string result = RunScript(functionString);
                wait_RequestedUrl = "";
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"GetBalance result: {result}");
#endif
                dynamic details = JsonConvert.DeserializeObject<dynamic>(result);

                balance = Utils.ParseToDouble(details.CustomerInfo.Balances.Balance.ToString());
                balance = balance / 100;
                
            }
            catch (Exception e)
            {

            }
            return balance;
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
                Thread.Sleep(5 * 60 * 1000);

                login();                
            }
        }
    }
#endif
}
