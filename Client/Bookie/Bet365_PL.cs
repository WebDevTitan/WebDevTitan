namespace Project.Bookie
{
#if (BET365_PL)
    public enum TaskType
    {
        None,
        Login,
        Addbet,
        Placebet,
        Openbet,
        GetBalance,
        RefreshPage,        
    }

    public class TaskParam
    {
        public TaskType type;
        public string f;  
        public string fp; 
        public string o;
        public string st;
        public string tr;
    }
    public class BET365_PLCtrl : IBookieController
    {
        //communication with page Thread.
        private Thread pageThread = null;

        private object taskLocker = new object();
        private List<TaskParam> taskList = new List<TaskParam>();
        
        
        private ManualResetEventSlim waitGetBalanceResultEvent = new ManualResetEventSlim();
        private double getBalanceResult = -1;

        private ManualResetEventSlim wait_LoginResultEvent = new ManualResetEventSlim();
        private bool wait_LoginResult = false;

        private ManualResetEventSlim wait_AddbetResultEvent = new ManualResetEventSlim();
        private string wait_AddbetResult = string.Empty;
        private ManualResetEventSlim wait_AddbetExecuteEvent = new ManualResetEventSlim();

        private ManualResetEventSlim wait_PlacebetResultEvent = new ManualResetEventSlim();
        private string wait_PlacebetResult = string.Empty;
        private ManualResetEventSlim wait_PlacebetExecuteEvent = new ManualResetEventSlim();


        TaskParam paramForRoute = new TaskParam();

        private bool bPlacingbet = false;
        private const double minMarketStake = 10;

        private IPlaywright playwright;
        private IBrowser browser = null;
        private IPage page = null;

        double last_x = 0, last_y = 0;
        Dictionary<string, int> Bet365IconNumber = new Dictionary<string, int> 
        { 
            //{"American Football", 12},
            //{"Baseball", 16},
            {"Basketball", 18},
            {"Esports", 151},
            //{"Golf", 7},
            //{"Greyhounds", 4},
            //{"Horse Racing", 2},
            {"Ice Hockey", 17},
            {"Soccer", 1},
            //{"Table Tennis", 92},
            {"Tennis", 13},
            //{"Volleyball", 91},
            //{"Handball", 78}
        };

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

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

        private double titlebarHeight = 0;
        private void SetWindowTopMost()
        {
            double height = SystemParameters.FullPrimaryScreenHeight;
            double width = SystemParameters.FullPrimaryScreenWidth;

            RECT rct = new RECT();
            GetWindowRect(Global.ViewerHwnd, ref rct);

            if (rct.Left < 0 || rct.Right > width || rct.Bottom > height)
                SetWindowPos(Global.ViewerHwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER);

            titlebarHeight = rct.Bottom - rct.Top - 600;
            SetForegroundWindow(Global.ViewerHwnd);
        }

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

        public void Scroll(Point clientPoint, uint ScrollHeight = 0xFFFFFE98) //-360
        {
            SetWindowTopMost();
            Thread.Sleep(300);
            clientPoint.Y += titlebarHeight;
            RECT rct = new RECT();
            GetWindowRect(Global.ViewerHwnd, ref rct);
            clientPoint.X += rct.Left;
            clientPoint.Y += rct.Top;
            /// get screen coordinates
            //ClientToScreen(Global.ViewerHwnd, ref clientPoint);

            /// set cursor on coords, and press mouse
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)clientPoint.X, (int)clientPoint.Y);

            //var inputMouseDown = new INPUT();
            //inputMouseDown.Type = 0; /// input type mouse
            //inputMouseDown.Data.Mouse.Flags = 0x0002; /// left button down

            //var inputMouseUp = new INPUT();
            //inputMouseUp.Type = 0; /// input type mouse
            //inputMouseUp.Data.Mouse.Flags = 0x0004; /// left button up


            var inputMouseWheel = new INPUT();
            inputMouseWheel.Type = 0; /// input type mouse
            inputMouseWheel.Data.Mouse.Flags = 0x0800; /// MOUSEEVENTF_WHEEL
            inputMouseWheel.Data.Mouse.Time = 0;
            inputMouseWheel.Data.Mouse.MouseData = ScrollHeight;
            inputMouseWheel.Data.Mouse.X = (int)clientPoint.X;
            inputMouseWheel.Data.Mouse.Y = (int)clientPoint.Y;


            var inputs = new INPUT[] { inputMouseWheel };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

            /// return mouse 
            //Cursor.Position = oldPos;
        }

        private void RunBrowser()
        {
            //create browser again and login (sometimes, mouse , keyboard is not working)
            try
            {
                if (page != null)
                    page.CloseAsync().Wait();

                if (browser != null)
                    browser.CloseAsync().Wait();
            }
            catch { }

            browser = playwright.Firefox.LaunchAsync(false).Result;

            var _context = browser.NewContextAsync(new ViewportSize() { Width = 1000, Height = 600 }).Result;
            _context.GrantPermissionsAsync(new ContextPermission[1] { ContextPermission.Geolocation }).Wait();
                        
            //string content = File.ReadAllText("mouse.js");
            //_context.AddInitScriptAsync(content, path : "mouse.js");

            page = _context.NewPageAsync().Result;
            
            page.Response += Page_Response;
            page.RouteAsync("**/*", Router);

            int nRetry = 0;
            while (nRetry++ < 10)
            {
                IntPtr hwnd = FindWindow("MozillaWindowClass", null);
                Global.ViewerHwnd = hwnd;
                if (hwnd != IntPtr.Zero)
                {
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"BrowserWnd catched {Global.ViewerHwnd}");
#endif
                    break;
                }
                Thread.Sleep(500);
            }
        }

       
        Random rnd = new Random();
        public BET365_PLCtrl()
        {
#if (TROUBLESHOT)
        LogMng.Instance.onWriteStatus($"Bet365_BMCtrl Start");
#endif
            paramForRoute.type = TaskType.None;

            Playwright.InstallAsync().Wait();
            playwright = Playwright.CreateAsync().Result;

            RunBrowser();


            lock (taskLocker)
            {
                taskList.Clear();
            }
            pageThread = new Thread(PageManagerProc);
            pageThread.Start();
        }

        public void Close()
        {

            try
            {
                page.CloseAsync().Wait();
                browser.CloseAsync().Wait();                
            }
            catch { }

            if (pageThread != null)
                pageThread.Abort();
                        
        }

        private async void Router(Route route, IRequest request)
        {
            var postdata = route.Request.PostData;
            var headers = route.Request.Headers;
            var method = route.Request.Method;
            var url = route.Request.Url;
            try
            {
                if (method == HttpMethod.Post)
                {
                    if (url.Contains("/members/lp/default.aspx"))
                    {
                        string ecUsername = WebUtility.UrlEncode(Setting.Instance.username);
                        string ecPassword = WebUtility.UrlEncode(Setting.Instance.password);
                        postdata = Utils.ReplaceStr(postdata, ecUsername, "&txtUsername=", "&");
                        postdata = Utils.ReplaceStr(postdata, ecPassword, "&txtPassword=", "&");                        
                    }
                    else if (url.Contains("addbet"))
                    {

                        //string ns = Utils.Between(postdata, "ns=", "&");
                        //ns = WebUtility.UrlDecode(ns);
                        //ns = Utils.ReplaceStr(ns, paramForRoute.f, "#f=", "#");
                        //ns = Utils.ReplaceStr(ns, paramForRoute.fp, "#fp=", "#");
                        //ns = Utils.ReplaceStr(ns, paramForRoute.o, "#o=", "#");
                        //ns = Utils.ReplaceStr(ns, $"BS{paramForRoute.f}-{paramForRoute.fp}", "TP=", "#");
                        //postdata = Utils.ReplaceStr(postdata, WebUtility.UrlEncode(ns), "ns=", "&");
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus("Addbet Sent");
#endif
                        wait_AddbetExecuteEvent.Set();
                        
                    }
                    else if (url.Contains("placebet"))
                    {

                        //postdata = Utils.ReplaceStr(postdata, paramForRoute.f, "#f=", "#");
                        //postdata = Utils.ReplaceStr(postdata, paramForRoute.fp, "#fp=", "#");
                        //postdata = Utils.ReplaceStr(postdata, paramForRoute.o, "#o=", "#");
                        //postdata = Utils.ReplaceStr(postdata, $"BS{paramForRoute.f}-{paramForRoute.fp}", "TP=", "#");

                        //postdata = Utils.ReplaceStr(postdata, paramForRoute.st, "#ust=", "#");
                        //postdata = Utils.ReplaceStr(postdata, paramForRoute.st, "#st=", "#");
                        //postdata = Utils.ReplaceStr(postdata, paramForRoute.tr, "#tr=", "#");
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus("Placebet Sent");
#endif
                        wait_PlacebetExecuteEvent.Set();
                        
                    }
                }
                
            }
            catch { }

            route.ContinueAsync(method, postdata, headers, url);
        }

        
        private async void Page_Response(object sender, ResponseEventArgs e)
        {
            if (e.Response.Status != HttpStatusCode.OK)
            {
                return;
            }

            try
            {
                await e.Response.FinishedAsync();

                if (e.Response.Url.ToLower().Contains("/betswebapi/addbet"))
                {
                    wait_AddbetResult = await e.Response.GetTextAsync();
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"addbet Res: {wait_AddbetResult}");
#endif
                    wait_AddbetResultEvent.Set();
                }
                else if (e.Response.Url.ToLower().Contains("/betswebapi/placebet"))
                {
                    wait_PlacebetResult = await e.Response.GetTextAsync();
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"placebet Res: {wait_PlacebetResult}");
#endif
                    wait_PlacebetResultEvent.Set();
                }
            }
            catch { }

        }

        public string getProxyLocation()
        {
            try
            {
                //page.GoToAsync("http://lumtest.com/myip.json").Wait();
                try
                {
                    page.GoToAsync("http://checkip.dyndns.org/", timeout:3000).Wait();
                
                
                    string content = page.GetTextContentAsync("xpath=//body").Result.Replace("Current IP Address:", "");
                    return content;
                }
                catch { }
            }
            catch (Exception ex)
            {
            }
            return "UNKNOWN";
        }

        public HttpClient initHttpClient(bool bUseNewCookie = true)
        {
            return null;
        }

        
        public int GetMyBetsCount()
        {
            int result = 0;
            try
            {
                result = Utils.parseToInt(RunScript("document.getElementsByClassName('hm-HeaderMenuItemMyBets_MyBetsCount ')[0].innerText"));
            }
            catch (Exception ex)
            {
                //LogMng.Instance.onWriteStatus($"GetMyBetsCount Exception {ex}");
            }
            return result;
        }



        public void RefreshBecauseBet365Notloading()
        {
            int nRetry = 0;
            while (nRetry++ < 3)
            {
                Thread.Sleep(3000);
                try
                {
                    var visible = PageIsVisible("div.bl-Preloader_Spinner");
                    if (!visible)
                    {
                        break;
                    }
                }
                catch (Exception ex){
                }

                try
                { 
                    page.ReloadAsync( 60 * 1000 ).Wait();
                }
                catch (Exception ex)
                {
                    LogMng.Instance.onWriteStatus($"RefreshBecauseBet365Notloading Exception {ex}");
                }
                Thread.Sleep(5000);
            }
        }

        private bool PageIsVisible(string param)
        {
            bool bResult = false;
            try
            {
                bResult = page.IsVisibleAsync(param).Result;
            }
            catch { }
            return bResult;
        }
        private bool PageClick(string param, int timeout = 500, int nRetry = 3)
        {
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"[PageClick] {param}");
#endif
            while (nRetry-- > 0)
            {
                try
                {
                    if (page.QuerySelectorAsync(param).Result.IsVisibleAsync().Result)
                    {

                        PlaywrightSharp.Rect rect = page.QuerySelectorAsync(param).Result.GetBoundingBoxAsync().Result;
                        MoveMouse((int)(rect.X + rect.Width / 2), (int)(rect.Y + rect.Height / 2));
                        page.Mouse.ClickAsync((int)(rect.X + rect.Width / 2), (int)(rect.Y + rect.Height / 2));
#if (TROUBLESHOT)
                        //page.ClickAsync($"{param}", timeout: timeout).Wait();
                        LogMng.Instance.onWriteStatus($"[PageClick] {param} clicked");
#endif
                        return true;
                    }
                }
                catch { }
                Thread.Sleep(500);
            }
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"[PageClick] {param} failed");
#endif
            return false;
        }
        private string RunFunction(string function, string param)
        {
            string result = page.EvaluateAsync(function, param).Result.ToString();
            return result;  
        }
        private string RunScript(string param)
        {
            string result = "";
            try
            {
                result = page.EvaluateAsync(param).Result.ToString().ToLower();
            }
            catch { }
            return result;
        }
        public bool login()
        {
#if (!SCRIPT)
            try
            {
                if (IsPageLoginStatus())
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"CheckLogin Exception: {ex}");
                if (ex.ToString().Contains("System.Threading.Tasks.Task.Wait"))
                {
                    RunBrowser();
                }
            }
                    
            
            try
            {
                ///
                page.GoToAsync($"https://www.{Setting.Instance.domain}").Wait();
                               
                

#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus("Login start");
#endif
                int nTotalRetry = 0;
                while (nTotalRetry++ < 3)
                {
                    try
                    {//return Locator.user.isLoggedIn;

                        RefreshBecauseBet365Notloading();


                        if (IsPageLoginStatus())
                        {
                            return true;
                        }
                        

                        string result = RunScript("document.getElementsByClassName('hm-MainHeaderRHSLoggedOutWide_Login ')[0].outerHTML");

#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Login button status : {result}");
#endif

                        if (!result.Contains("class"))
                        {
                            page.GoToAsync($"https://www.{Setting.Instance.domain}").Wait();

                            //check if page is loaded all
                            int nRetry1 = 0;
                            while (nRetry1 < 30)
                            {
                                Thread.Sleep(500);
                                result = RunScript("document.getElementsByClassName('hm-MainHeaderRHSLoggedOutWide_Login ')[0].outerHTML");

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
                                LogMng.Instance.onWriteStatus($"Let's retry because of loading gray page");
#endif

                                continue;
                            }
                        }

                        
                        int nButtonRetry = 0;
                        while (nButtonRetry++ < 3)
                        {
                            PageClick("div.hm-MainHeaderRHSLoggedOutWide_Login");
                            Thread.Sleep(500);
                            string button_result = RunScript("document.getElementsByClassName('lms-StandardLogin_Username ')[0].outerHTML");

#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"UserId label status : {result}");
#endif

                            if (button_result.Contains("class"))
                                break;
                        }
                        if (nButtonRetry == 3)
                        {

                            LogMng.Instance.onWriteStatus("Clicking Login button not working. retry from scratch again.");
                            continue;
                        }
                        Thread.Sleep(500);


                        PageClick("input.lms-StandardLogin_Username");
                        
                        //PageClick("input.lms-StandardLogin_Username");
                        page.Keyboard.TypeAsync(Setting.Instance.username, 100).Wait();
                        Thread.Sleep(500);

                        PageClick("input.lms-StandardLogin_Password");
                        //PageClick($"input.lms-StandardLogin_Password");
                        page.Keyboard.TypeAsync(Setting.Instance.password, 100).Wait();
                        Thread.Sleep(500);

                        //try
                        //{
                        //    PageClick("div.lms-StandardLogin_LoginButton");
                            
                        //    //PageClick("div.lms-StandardLogin_LoginButton");
                        //}
                        //catch { }
                        try
                        {
                            PageClick("div.lms-LoginButton");
                            
                            //PageClick("div.lms-LoginButton");
                        }
                        catch { }


                        int nRetry = 0;
                        while (nRetry < 3)
                        {
                            Thread.Sleep(5000);
                            nRetry++;

                            if (IsPageLoginStatus())
                            {
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus("Login Successed");
#endif
                                return true;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"login exception: {ex}");
#endif
                        if (ex.ToString().Contains("(NS_ERROR_NOT_AVAILABLE) [nsITextInputProcessor.keydown]"))
                        {
                            RunBrowser();
                            
                        }
                    }
                }
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus("Login Failed");
#endif
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("ThrowIfExceptional(Boolean includeTaskCanceledExceptions)"))
                {
                    RunBrowser();

                }

                LogMng.Instance.onWriteStatus($"Exception : {ex.Message} {ex.StackTrace}");
            }
            return false;
#else
            wait_LoginResult = false;
            wait_LoginResultEvent.Reset();

            TaskParam task = new TaskParam();
            task.type = TaskType.Login;
            lock (taskLocker)
            {
                taskList.Add(task);
            }
            if (wait_LoginResultEvent.Wait(100000))
            {
                return wait_LoginResult;
            }
            else
            {
                LogMng.Instance.onWriteStatus("login No Result Event");
            }
            return false;
#endif
        }

        private bool IsPageLoginStatus()
        {
            string result = RunScript("Locator.user.isLoggedIn");
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"LonginStatus(1): {result}");
#endif
            if (result == "true")
            {
                return true;
            }
            return false;
        }
        public void page_login()
        {
            try
            {
                if (IsPageLoginStatus())
                {
                    wait_LoginResult = true;
                    wait_LoginResultEvent.Set();
                    return;
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"CheckLogin Exception: {ex}");
                if (ex.ToString().Contains("System.Threading.Tasks.Task.Wait"))
                {
                    //Closed browser
                    RunBrowser();
                }
            }


            try
            {
                page.GoToAsync($"https://www.{Setting.Instance.domain}").Wait();


#if (TROUBLESHOT)
        LogMng.Instance.onWriteStatus("Login start");
#endif
                int nTotalRetry = 0;
                while (nTotalRetry++ < 2)
                {
                    try
                    {//return Locator.user.isLoggedIn;

                        RefreshBecauseBet365Notloading();


                        if (IsPageLoginStatus())
                        {
                            wait_LoginResult = true;
                            wait_LoginResultEvent.Set();
                            return;
                        }


                        string result = RunScript("document.getElementsByClassName('hm-MainHeaderRHSLoggedOutWide_Login ')[0].outerHTML");

#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Login button status : {result}");
#endif

                        if (!result.Contains("class"))
                        {
                            page.GoToAsync($"https://www.{Setting.Instance.domain}").Wait();

                            //check if page is loaded all
                            int nRetry1 = 0;
                            while (nRetry1 < 30)
                            {
                                Thread.Sleep(500);
                                result = RunScript("document.getElementsByClassName('hm-MainHeaderRHSLoggedOutWide_Login ')[0].outerHTML");

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
                                LogMng.Instance.onWriteStatus($"Let's retry because of loading gray page");
#endif

                                continue;
                            }
                        }


                        Thread.Sleep(500);

                        PageClick("div.hm-MainHeaderRHSLoggedOutWide_Login");
                        Thread.Sleep(500);

                        PageClick("input.lms-StandardLogin_Username");
                        page.Keyboard.TypeAsync(Setting.Instance.username).Wait();
                        //page.FillAsync("input.lms-StandardLogin_Username", Setting.Instance.username).Wait();

                        Thread.Sleep(500);

                        PageClick($"input.lms-StandardLogin_Password");
                        page.Keyboard.TypeAsync(Setting.Instance.password).Wait();
                        //page.FillAsync("input.lms-StandardLogin_Password", Setting.Instance.password).Wait();
                        Thread.Sleep(500);

                        try
                        {
                            PageClick("div.lms-StandardLogin_LoginButton");
                        }
                        catch { }
                        try
                        {
                            PageClick("div.lms-LoginButton");
                        }
                        catch { }

                        int nRetry = 0;
                        while (nRetry < 3)
                        {
                            Thread.Sleep(5000);
                            nRetry++;
                            
                            if (IsPageLoginStatus())
                            {
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus("Login Successed");
#endif
                                                             

                                wait_LoginResult = true;
                                wait_LoginResultEvent.Set();
                                return;
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
            catch (Exception ex){
                LogMng.Instance.onWriteStatus($"Exception : {ex.Message} {ex.StackTrace}");
            }

            wait_LoginResult = false;
            wait_LoginResultEvent.Set();
        }

        public PROCESS_RESULT PlaceBetInScript(BetburgerInfo info)
        {
            PROCESS_RESULT SlipRes = PROCESS_RESULT.ERROR;

            if (!login())
                return PROCESS_RESULT.NO_LOGIN;

            int nRetry4SmallMarket = 3;
            string strBet365Result = string.Empty;

            double origStake = info.stake;
            while (nRetry4SmallMarket > 0)
            {
                nRetry4SmallMarket--;
                string ns = "", ms = "";
                string guid = "", cc = "", pc = "";
                SlipRes = GetNsToken(ref ns, ref ms, info, MAKE_SLIP_STEP.INIT, null, ref guid, ref cc, ref pc);
                if (SlipRes == PROCESS_RESULT.ERROR)
                {
                    LogMng.Instance.onWriteStatus(string.Format("[InitBet] Step 1 Failed"));
                    return SlipRes;
                }

                int nRetry = 0;
                while (nRetry++ < 2)
                {
#if (SCRIPT)
                    strBet365Result = doAddBetTask(betinfo.betData[0].fd, betinfo.betData[0].i2, betinfo.betData[0].oddStr);
#else
                    strBet365Result = doAddBet(ns, ms);
#endif
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"doAddBet Result: {strBet365Result}");
#endif
                    SlipRes = GetNsToken(ref ns, ref ms, info, MAKE_SLIP_STEP.ADD_BET, strBet365Result, ref guid, ref cc, ref pc);
       
                    if (SlipRes == PROCESS_RESULT.ERROR || SlipRes == PROCESS_RESULT.RE_FIXED)
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[AddBet] Failed"));
                        return SlipRes;
                    }
                    else if (SlipRes == PROCESS_RESULT.NO_LOGIN)
                    {

                        LogMng.Instance.onWriteStatus(string.Format("[AddBet] Login again..."));
                        if (!login())
                        {
                            LogMng.Instance.onWriteStatus(string.Format("[AddBet] Step 3 Failed"));
                            return SlipRes;
                        }
                    }
                    else if (SlipRes == PROCESS_RESULT.MOVED)
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[AddBet] Retry because of changed odd(line)"));
                        SlipRes = GetNsToken(ref ns, ref ms, info, MAKE_SLIP_STEP.INIT, null, ref guid, ref cc, ref pc);
                        if (SlipRes == PROCESS_RESULT.ERROR)
                        {
                            LogMng.Instance.onWriteStatus(string.Format("[InitBet] Step 2 Failed"));
                            return SlipRes;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (SlipRes != PROCESS_RESULT.SUCCESS)
                {
                    LogMng.Instance.onWriteStatus(string.Format("[AddBet] Step 4 Failed"));

                    return SlipRes;
                }

                //if (Setting.Instance.domain.Contains(".au"))
                //{
                //    nRetry = 0;
                //    while (nRetry++ < 2)
                //    {
                //        strBet365Result = doConfirmBet(betinfo.betGuid, ns, ms);
                //        LogMng.Instance.onWriteStatus("confirmbet result: " + strBet365Result);

                //        if (strBet365Result.Contains("\"sr\":0"))
                //        {
                //            break;
                //        }

                //        SlipRes = GetNsToken(ref ns, ref ms, ref betinfo, MAKE_SLIP_STEP.PLACE_BET, strBet365Result);
                //        if (SlipRes == PROCESS_RESULT.ERROR)
                //        {
                //            LogMng.Instance.onWriteStatus(string.Format("[ConfirmBet] Step 2 Failed"));
                //            return SlipRes;
                //        }
                //        else if (SlipRes == PROCESS_RESULT.NO_LOGIN)
                //        {
                //            if (!login())
                //            {
                //                LogMng.Instance.onWriteStatus(string.Format("[ConfirmBet] Step 3 Failed"));
                //                return SlipRes;
                //            }
                //        }
                //        else if (SlipRes == PROCESS_RESULT.MOVED)
                //        {
                //            strBet365Result = doRefreshSlip(ns, ms);
                //            SlipRes = GetNsToken(ref ns, ref ms, ref betinfo, MAKE_SLIP_STEP.PLACE_BET, strBet365Result);
                //        }
                //    }

                //    if (!strBet365Result.Contains("\"sr\":0"))
                //    {
                //        LogMng.Instance.onWriteStatus(string.Format("[ConfirmBet]! confirmbet failed!"));
                //        return PROCESS_RESULT.ERROR;
                //    }
                //}


#if (!SCRIPT)

                strBet365Result = doPlaceBet(guid, cc, pc, ns, ms);
#else
                Thread.Sleep(1000);
                strBet365Result = doPlaceBetTask(betinfo.betData[0].fd, betinfo.betData[0].i2, betinfo.betData[0].oddStr, betinfo.stake.ToString("N2"), tr.ToString("N2"));
#endif

#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"doPlaceBet Result: {strBet365Result}");
#endif
                SlipRes = GetNsToken(ref ns, ref ms, info, MAKE_SLIP_STEP.PLACE_BET, strBet365Result, ref guid, ref cc, ref pc);

                

                if (SlipRes == PROCESS_RESULT.PLACE_SUCCESS)
                {
                    LogMng.Instance.onWriteStatus($"[PlaceBet]! success! stake: {info.stake} origStake: {origStake}");
                    //check if retrying for small markets
                    if (origStake - info.stake >= 1)
                    {
                        origStake -= info.stake;
                        info.stake = origStake;
                      

                        nRetry4SmallMarket = 1;

                        LogMng.Instance.onWriteStatus($"[PlaceBet] Retrying for small stake market cur stake : {info.stake}");
                        Thread.Sleep(5000);
                        continue;
                    }
                    return SlipRes;
                }
                else if (SlipRes == PROCESS_RESULT.NO_LOGIN)
                {
                    try
                    {
                        page.Context.ClearCookiesAsync().Wait();
                    }
                    catch { }
                    LogMng.Instance.onWriteStatus(string.Format("[PlaceBet] Login again..."));
                    if (!login())
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[PlaceBet] Step 3 Failed"));
                        return SlipRes;
                    }
                    return SlipRes;
                }
                else if (SlipRes == PROCESS_RESULT.MOVED)
                {
                    if (nRetry4SmallMarket <= 0)
                        break;

                    ns = ""; ms = "";
                    SlipRes = GetNsToken(ref ns, ref ms, info, MAKE_SLIP_STEP.INIT, null, ref guid, ref cc, ref pc);
#if (!SCRIPT)
                    strBet365Result = doAddBet(ns, ms);
#else
                    strBet365Result = doAddBetTask(betinfo.betData[0].fd, betinfo.betData[0].i2, betinfo.betData[0].oddStr);
#endif
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"doAddBet in Placebet Result: {strBet365Result}");
#endif
                    SlipRes = GetNsToken(ref ns, ref ms, info, MAKE_SLIP_STEP.ADD_BET, strBet365Result, ref guid, ref cc, ref pc);

                }
                else
                {
                    LogMng.Instance.onWriteStatus(string.Format("[PlaceBet] failed result: {0}", SlipRes));
                }

            }
            return SlipRes;
        }

        private string doAddBet(string ns, string ms)
        {
            if (string.IsNullOrEmpty(ns))
                return String.Empty;

            //#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"doAddBet ns: {ns} ms: {ms}");
            //#endif

            try
            {
                wait_AddbetResult = "";
                wait_AddbetResultEvent.Reset();
                //string command = $"var s = {{normals: '{ns}',casts: '',multiples: '{ms}',completeHandler: function(t) {{}},errorHandler: function() {{}}}};ns_betslipstandardlib_util.APIHelper.AddBet(s);";

                string command = $"ns_betslipcorelib_util.BetsWebApi.MakeApiReqiest('addbet', {{normals: '{ns}',casts: '',multiples: '{ms}',completeHandler: function(t) {{}},errorHandler: function() {{}}}});";
                RunScript(command);
                wait_AddbetResultEvent.Wait(30000);
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"doAddBet Res: {wait_AddbetResult}");
#endif
                return wait_AddbetResult;
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"doAddBet Exception {ex}");
            }

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"doAddBet Res:(empty)");
#endif
            return string.Empty;
        }
        private string doPlaceBet(string betGuid, string bet_cc, string bet_pc, string ns, string ms)
        {
            if (string.IsNullOrEmpty(ns))
                return String.Empty;

            //#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"doPlaceBet gid: {betGuid} cc: {bet_cc} pc: {bet_pc} ns: {ns} ms: {ms}");
            //#endif

            betGuid += "&c=" + bet_cc + "&p=" + bet_pc;

            try
            {
                wait_PlacebetResult = "";
                wait_PlacebetResultEvent.Reset();
                //string command = $"var s = {{betGuid: '{betGuid}',participantCorrelation: '{bet_pc}',betRequestCorrelation: '{bet_cc}',normals: '{ns}',casts: '',multiples: '{ms}',completeHandler: function(t) {{}},errorHandler: function() {{}}}};ns_betslipstandardlib_util.APIHelper.PlaceBet(s);";

                string command = $"ns_betslipcorelib_util.BetsWebApi.MakeApiReqiest('placebet?betGuid={betGuid}', {{normals: '{ns}',completeHandler: function(t) {{}},errorHandler: function() {{}}}});";
                if (!string.IsNullOrEmpty(ms))
                {
                    command = $"ns_betslipcorelib_util.BetsWebApi.MakeApiReqiest('placebet?betGuid={betGuid}', {{normals: '{ns}', multiples: '{ms}',completeHandler: function(t) {{}},errorHandler: function() {{}}}})";
                }

                RunScript(command);
                wait_PlacebetResultEvent.Wait(30000);

#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"doPlaceBet Res: {wait_PlacebetResult}");
#endif
                return wait_PlacebetResult;
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"doPlaceBet Exception {ex}");
            }

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"doPlaceBet Res:(empty)");
#endif
            return string.Empty;
        }

        public PROCESS_RESULT PlaceBetInBrowser(BetburgerInfo info)
        {
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"Placebet action start");
#endif
            OpenBet_Bet365 openbet = Utils.ConvertBetburgerPick2OpenBet_365(info);

            if (info.kind == PickKind.Type_6)
                info.eventUrl = "/" + info.siteUrl;
            try
            {
                int nTotalRetry = 0;
                while (nTotalRetry < 2)
                {
                    nTotalRetry++;

                    if (!login())
                    {
                        LogMng.Instance.onWriteStatus("Placebet failed because of login failure.");
                        return PROCESS_RESULT.NO_LOGIN;
                    }


#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"[Placebet] EventUrl Modified: {info.eventUrl}");
#endif

                    ////find match
                    //PageClick("div.hm-SiteSearchIconLoggedIn_Icon");
                    ////Thread.Sleep(1000);

                    ////if (page.WaitForSelectorAsync("div.sml-ClearButton", WaitForState.Visible, 3000).Result == null)
                    ////{
                    ////    LogMng.Instance.onWriteStatus("Event input label is not visible");
                    ////    continue;
                    ////}
                    //PageClick("div.sml-ClearButton");
                    //Thread.Sleep(100);
                    //PageClick("div.sml-SearchTextInput");
                    //Thread.Sleep(100);
                    //page.Keyboard.TypeAsync(info.homeTeam, 100 );
                    //Thread.Sleep(1000);
                    //PageClick($"div.ssm-SiteSearchLabelOnlyParticipant:has-text('{info.awayTeam}')");

                    page.GoToAsync($"https://www.{Setting.Instance.domain}" + info.eventUrl).Wait();
                    Thread.Sleep(1000);

                    last_x = 0;
                    last_y = 0;

                    int scrollx = 0, scrolly = 0;

                    try
                    {
                        PlaywrightSharp.Rect rect = new PlaywrightSharp.Rect(0, 0, 0, 0);
                        if (info.kind == PickKind.Type_6) //prematch
                            rect = page.QuerySelectorAsync("div.wcl-PageContainer").Result.GetBoundingBoxAsync().Result;
                        else
                            rect = page.QuerySelectorAsync("div.ipe-EventViewDetail").Result.GetBoundingBoxAsync().Result;

                        if (rect.Width == 0 || rect.Height == 0)
                        {
                            throw new SystemException("rect doesn't exist");
                        }

                        scrollx = (int)(rect.X + rect.Width / 3 + rnd.Next(1, (int)(rect.Width / 3)));
                        scrolly = (int)(rect.Y + rect.Height / 3 + rnd.Next(1, (int)(rect.Height / 3)));

                        MoveMouse(scrollx, scrolly);
                        Scroll(new Point(scrollx, scrolly), 0xFFFFFF88); //-120
                        Thread.Sleep(300);
                        Scroll(new Point(scrollx, scrolly), 0x78);   //120

                    }
                    catch
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus("Event Url is incorrect");
#endif
                        return PROCESS_RESULT.ERROR;
                    }
                    Thread.Sleep(500);
                    string OddMarketResult = "no_market";
                    System.Windows.Rect OddMarketRect = new System.Windows.Rect(0, 0, 0, 0);

                    int nRetry = 0;
                    while (nRetry++ < 40)
                    {
                        string function1 = "fi => { try{for (let i = 0; i < Locator.user._eRegister.oddsChanged.length; i++){let item = Locator.user._eRegister.oddsChanged[i];if (item == null || item.scope == null || item.scope.twinEmphasizedHandlerType == null)continue;if (item.scope.twinEmphasizedHandlerType.endsWith(fi)){if (!item.scope._active_element.className.includes('Suspended')){var rect = item.scope._active_element.getBoundingClientRect();var isVisible = (rect.top >= 0) && (rect.bottom <= window.innerHeight);if (isVisible)return JSON.stringify(rect);return 'invisible';}return 'suspend';}}}catch(err){return 'exception';}return 'no_market';}";
                        OddMarketResult = RunFunction(function1, openbet.betData[0].i2).ToLower();


                        ////////////////////////////////////////////////////////////////////////////
                        //try
                        //{
                        //    for (let i = 0; i < Locator.user._eRegister.oddsChanged.length; i++)
                        //    {
                        //        let item = Locator.user._eRegister.oddsChanged[i];
                        //        if (item == null || item.scope == null || item.scope.twinEmphasizedHandlerType == null)
                        //            continue;
                        //        if (item.scope.twinEmphasizedHandlerType.endsWith(FI))
                        //        {
                        //            if (!item.scope._active_element.className.includes("Suspended"))
                        //            {
                        //                var rect = item.scope._active_element.getBoundingClientRect();
                        //                var isVisible = (rect.top >= 0) && (rect.bottom <= window.innerHeight);
                        //                if (isVisible)
                        //                    return JSON.stringify(rect);
                        //                return "invisible";  
                        //            }
                        //            return "suspend";
                        //        }
                        //    }
                        //}
                        //catch (err)
                        //{
                        //    return "exception";
                        //}
                        //return "no_market";
                        ////////////////////////////////////////////////////////////////////////////

#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Finding OddMarketPos Result: {OddMarketResult}");
#endif
                        if (OddMarketResult == "no_market")
                        {
                            return PROCESS_RESULT.SUSPENDED;
                        }
                        else if (OddMarketResult == "suspend")
                        {
                            return PROCESS_RESULT.SUSPENDED;
                        }
                        else if (OddMarketResult == "exception")
                        {
                            return PROCESS_RESULT.SUSPENDED;
                        }
                        else if (OddMarketResult == "invisible")
                        {

                        }
                        else
                        {
                            OddMarketRect = Utils.ParseRectFromJson(OddMarketResult);
                            break;
                        }

                        Scroll(new Point(scrollx, scrolly));
                        Thread.Sleep(300);
                        //page.Mouse.WheelAsync(0, 300);
                    }

                    if (OddMarketRect.X <= 0 || OddMarketRect.Y <= 0 || OddMarketRect.Width <= 0 || OddMarketRect.Height <= 0)
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus("Can't find OddElement");
#endif
                        Thread.Sleep(500);
                        continue;
                    }


                    //clicking odd element

                    try
                    {

                        nRetry = 0;
                        while (nRetry++ < 3)
                        {
                            wait_AddbetResult = "";
                            wait_AddbetResultEvent.Reset();

                            ////////////////////////////////////////////////////////////////////////////
                            //try
                            //{
                            //    for (let i = 0; i < Locator.user._eRegister.oddsChanged.length; i++)
                            //    {
                            //        let item = Locator.user._eRegister.oddsChanged[i];
                            //        if (item == null || item.scope == null || item.scope.twinEmphasizedHandlerType == null)
                            //            continue;
                            //        if (item.scope.twinEmphasizedHandlerType.endsWith(FI))
                            //        {
                            //            if (!item.scope._active_element.className.includes('Suspended'))
                            //            {
                            //                return JSON.stringify(item.scope._active_element.getBoundingClientRect());
                            //            }
                            //            break;
                            //        }
                            //    }
                            //}
                            //catch (err)
                            //{
                            //    return err.message;
                            //}
                            //return '';
                            ////////////////////////////////////////////////////////////////////////////
                            ///

                            string getOddMarketPos = "fi => { try{for (let i = 0; i < Locator.user._eRegister.oddsChanged.length; i++){let item = Locator.user._eRegister.oddsChanged[i];if (item == null || item.scope == null || item.scope.twinEmphasizedHandlerType == null)continue;if (item.scope.twinEmphasizedHandlerType.endsWith(fi)){if (!item.scope._active_element.className.includes('Suspended')){return JSON.stringify(item.scope._active_element.getBoundingClientRect());}break;}}}catch (err){return err.message;}return '';}";
                            string oddElementLocation = "";
                            oddElementLocation = RunFunction(getOddMarketPos, openbet.betData[0].i2).ToLower();
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"Before Clicking OddMarket Pos {oddElementLocation}");
#endif
                            OddMarketRect = Utils.ParseRectFromJson(oddElementLocation);

                            if (OddMarketRect.X <= 0 || OddMarketRect.Y <= 0 || OddMarketRect.Width <= 0 || OddMarketRect.Height <= 0)
                            {
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus("Can't find OddElement in clicking module");
#endif
                                Thread.Sleep(500);
                                continue;
                            }

                            wait_AddbetExecuteEvent.Reset();
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"moving to addbet button {nRetry}");
#endif

                            MoveMouse((int)(OddMarketRect.X + OddMarketRect.Width / 2), (int)(OddMarketRect.Y + OddMarketRect.Height / 2));
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"Before clicking addbet button {nRetry}");
#endif
                            Thread.Sleep(200);
                            page.Mouse.ClickAsync((decimal)(OddMarketRect.X + OddMarketRect.Width / 2), (decimal)(OddMarketRect.Y + OddMarketRect.Height / 2));

                            if (!wait_AddbetExecuteEvent.Wait(2000))
                            {
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus("Addbet is not requested, retry");
#endif
                                continue;
                            }

                            if (!wait_AddbetResultEvent.Wait(30000))
                            {
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus("Addbet is has no response, retry");
#endif
                                return PROCESS_RESULT.RE_FIXED;
                            }

                            int nBetslipcheckRetry = 0;
                            while (nBetslipcheckRetry++ < 30)
                            {
                                string betslipStatus = RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets.length");
                                if (betslipStatus == "1")
                                {
#if (TROUBLESHOT)
                                    LogMng.Instance.onWriteStatus("Placebet Betslip appeared");
#endif
                                    break;
                                }
                                Thread.Sleep(100);
                            }
                            Thread.Sleep(500);
                            break;
                        }
                        if (nRetry >= 3)
                        {
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus("addbet failed");
#endif
                            continue;
                        }

                    }
                    catch { }


#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus("Placebet inputing stake");
#endif

                    string stakeposition = RunScript("JSON.stringify(BetSlipLocator.betSlipManager.betslip.activeModule.slip.footer.stakeBox._active_element.getBoundingClientRect())");
                    System.Windows.Rect iconRect = Utils.ParseRectFromJson(stakeposition);
                    if (iconRect.X <= 0 || iconRect.Y <= 0 || iconRect.Width <= 0 || iconRect.Height <= 0)
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus("No stakebox ");
#endif
                        continue;
                    }

                    nRetry = 0;
                    while (nRetry++ < 3)
                    {
                        MoveMouse((int)(iconRect.X + iconRect.Width / 2), (int)(iconRect.Y + iconRect.Height / 2));
                        Thread.Sleep(200);
                        page.Mouse.ClickAsync((decimal)(iconRect.X + iconRect.Width / 2), (decimal)(iconRect.Y + iconRect.Height / 2));
                        Thread.Sleep(200);
                        page.Keyboard.TypeAsync(info.stake.ToString());

                        string stakeInput = RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.footer.stakeBox.stakeEntered");
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"Stake Write： {stakeInput}");
#endif
                        if (stakeInput != "")
                        {

                            break;
                        }
                        Thread.Sleep(500);
                    }
                    if (nRetry >= 3)
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus("Stake Input Failed ");
#endif
                        continue;
                    }
                    if (Setting.Instance.bEachWay && info.sport == "Horse Racing" && info.odds >= Setting.Instance.eachWayOdd)
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus("Placebet ticking e/w");
#endif

                        RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.eachwayChecked()");
                        Thread.Sleep(500);
                    }


                    wait_PlacebetResult = "";
                    

                    int nRetryPlacebet = 0;
                    while (nRetryPlacebet < 3)
                    {
                        nRetryPlacebet++;

                        string PlacebetPosition = RunScript("JSON.stringify(BetSlipLocator.betSlipManager.betslip.activeModule.slip.footer.betWrapper._active_element.getBoundingClientRect())");
                        iconRect = Utils.ParseRectFromJson(PlacebetPosition);
                        if (iconRect.X <= 0 || iconRect.Y <= 0 || iconRect.Width <= 0 || iconRect.Height <= 0)
                        {
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus("No placebet button ");
#endif
                            return PROCESS_RESULT.SUSPENDED;
                        }
                        wait_PlacebetResultEvent.Reset();
                        wait_PlacebetExecuteEvent.Reset();

#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Placebet clicking placebet button.. {nRetryPlacebet}");
#endif

                        MoveMouse((int)(iconRect.X + iconRect.Width / 2), (int)(iconRect.Y + iconRect.Height / 2));
                        Thread.Sleep(200);
                        page.Mouse.ClickAsync((decimal)(iconRect.X + iconRect.Width / 2), (decimal)(iconRect.Y + iconRect.Height / 2));

                        if (!wait_PlacebetExecuteEvent.Wait(2000))
                            continue;

                        //Thread.Sleep(100);
                        //_page.EvaluateExpressionAsync("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.betPlacement.placeBetButton.slipdelegate.acceptOnlyButtonValidate()").Wait();

                        //Thread.Sleep(100);
                        //_page.EvaluateExpressionAsync("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.betPlacement.placeBetButton.slipdelegate.placeBetButtonValidateAndPlaceBet()").Wait();


                        if (wait_PlacebetResultEvent.Wait(10000))
                        {
                            BetSlipJson betSlipJson = null;
                            if (!string.IsNullOrEmpty(wait_PlacebetResult))
                            {
                                betSlipJson = JsonConvert.DeserializeObject<BetSlipJson>(wait_PlacebetResult);
                                if (betSlipJson.sr == 0)
                                {
                                    return PROCESS_RESULT.PLACE_SUCCESS;
                                }
                            }

                            Thread.Sleep(1000);
                            string betslipState = RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.currentState");

#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"Placebet betslip status {betslipState} retry {nRetryPlacebet}");
#endif

                        }

#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus("Placebet wait timeout ..");
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus("PlaceBetInBrowser exception: " + ex);
            }
            return PROCESS_RESULT.ERROR;
        }

        int mouseSpeed = 7;
        void MoveMouse(int x, int y, int rx = 5, int ry = 5)
        {

            x += rnd.Next(rx);
            y += rnd.Next(ry);

            double randomSpeed = Math.Max((rnd.Next(mouseSpeed) / 2.0 + mouseSpeed) / 10.0, 0.1);

            WindMouse(last_x, last_y, x, y, 9.0, 3.0, 10.0 / randomSpeed,
                15.0 / randomSpeed, 10.0 * randomSpeed, 10.0 * randomSpeed);
            last_x = x;
            last_y = y;
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
                    page.Mouse.MoveAsync(newX, newY);

                step = Hypot(xs - oldX, ys - oldY);
                int wait = (int)Math.Round(waitDiff * (step / maxStep) + minWait);
                Thread.Sleep(wait);
            }

            int endX = (int)Math.Round(xe);
            int endY = (int)Math.Round(ye);
            if (endX != newX || endY != newY)
                page.Mouse.MoveAsync(endX, endY);
        }

        static double Hypot(double dx, double dy)
        {
            return Math.Sqrt(dx * dx + dy * dy);
        }
        public void PlaceBet(List<CapsuledBetburgerInfo> infoList)
        {
            if (infoList.Count <= 0)
            {
                LogMng.Instance.onWriteStatus("Infolist is insufficient");
                return;
            }
 

            List<OpenBet_Bet365> openbetList = new List<OpenBet_Bet365>();
            for (int i = 0; i <infoList.Count; i++)
            {
                OpenBet_Bet365 openbet = Utils.ConvertBetburgerPick2OpenBet_365(infoList[i].betburgerInfo);
                if (openbet == null)
                {
                    if (infoList[i].betburgerInfo.kind != PickKind.Type_4 && infoList[i].betburgerInfo.kind != PickKind.Type_5)
                    {                     
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Directlink error: {infoList[i].betburgerInfo.eventTitle} direct_link: {infoList[i].betburgerInfo.direct_link} siteurl: {infoList[i].betburgerInfo.siteUrl}");
#endif
                        Uri uriResult;
                        if (Uri.TryCreate(infoList[i].betburgerInfo.siteUrl, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                        {
                            infoList[i].result = PlaceBetInBrowser(infoList[i].betburgerInfo);
                            continue;
                        }
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Siteurl invalid error: {infoList[i].betburgerInfo.eventTitle} siteurl: {infoList[i].betburgerInfo.siteUrl}");
#endif
                        infoList[i].result = PROCESS_RESULT.ERROR;
                    }
                }
            }

            bool bAlreadyProcessed = true;
            foreach (var info in infoList)
            {
                if (info.result == PROCESS_RESULT.SUCCESS)
                {
                    bAlreadyProcessed = false;
                    break;
                }
            }

            if (bAlreadyProcessed)
                return;


            bPlacingbet = true;
            infoList[0].result = PlaceBetInBrowser(infoList[0].betburgerInfo);
            bPlacingbet = false;
            //try
            //{
            //    PlaceScriptBet(infoList);
            //}
            //catch (Exception ex)
            //{
            //    LogMng.Instance.onWriteStatus($"Placebet exception {ex}");
            //}

            foreach (var info in infoList)
            {
                if (Global.PackageID == 1 && info.result == PROCESS_RESULT.PLACE_SUCCESS)
                {
                    try
                    {
                        Global.balance = getBalance();
                        int nMyBetCount = GetMyBetsCount();

                        
                        PlacedBetInfo betinfo = new PlacedBetInfo();
                        betinfo.bookmaker = info.betburgerInfo.extra;
                        betinfo.username = Setting.Instance.username;
                        betinfo.odds = info.betburgerInfo.odds;
                        betinfo.stake = info.betburgerInfo.stake;
                        betinfo.balance = Global.balance;
                        betinfo.percent = info.betburgerInfo.percent;
                        betinfo.sport = info.betburgerInfo.sport;
                        betinfo.outcome = info.betburgerInfo.outcome;
                        betinfo.eventTitle = info.betburgerInfo.eventTitle;
                        betinfo.homeTeam = info.betburgerInfo.homeTeam;
                        betinfo.awayTeam = info.betburgerInfo.awayTeam;
                        betinfo.bookmaker = info.betburgerInfo.extra;
                        betinfo.pendingBets = nMyBetCount;
                        UserMng.GetInstance().SendSuccessBetReport(betinfo);
                        
                    }
                    catch { }
                }
            }
        }

        public double getBalance()
        {
            getBalanceResult = -1;
            waitGetBalanceResultEvent.Reset();
            
            TaskParam task = new TaskParam();
            task.type = TaskType.GetBalance;
            lock (taskLocker)
            {
                taskList.Add(task);
            }
            if (waitGetBalanceResultEvent.Wait(10000))
            {                
                return getBalanceResult;
            }
            else
            {
                LogMng.Instance.onWriteStatus("getBalance No Result Event");
            }
            return -1;
        }

        public void page_getBalance()
        {    
            int nRetry = 0;
            double result = -1;          

            while (nRetry++ < 2)
            {
                PageClick("div.hm-MainHeaderMembersWide_MembersMenuIcon");
                Thread.Sleep(500);
                PageClick("div.um-BalanceRefreshButton_Icon");
                Thread.Sleep(1000);
                try
                {
                    result = Utils.ParseToDouble(RunScript("Locator.user.getBalance().totalBalance"));
                }
                catch
                {

                }

                if (result > 0)
                    break;
                Thread.Sleep(100);
            }

#if (TROUBLESHOT)
        LogMng.Instance.onWriteStatus($"getBalance: {result}");
#endif

            getBalanceResult = result;
            waitGetBalanceResultEvent.Set();
        }

        public void Feature()
        {
            //TaskParam task = new TaskParam();
            //task.type = TaskType.RefreshPage;
            //lock (taskLocker)
            //{
            //    taskList.Add(task);
            //}
        }
        public bool Pulse()
        {
            TaskParam task = new TaskParam();
            task.type = TaskType.Openbet;
            lock (taskLocker)
            {
                taskList.Add(task);
            }
            return true;
        }

        private void PageManagerProc()
        {
            while (true)
            {
                if (!Global.bRun)
                {
                    if (taskList.Count == 1 && taskList[0].type == TaskType.Openbet)
                    {
                        //when openbet is requested, it should be run even though it's stopped status.
                    }
                    else
                    {
                        Thread.Sleep(200);
                        continue;
                    }
                }

                if (PageIsVisible("div.alm-ActivityLimitStayButton"))
                {//closing for last login time(balance) popup
                    PageClick("div.alm-ActivityLimitStayButton", 100, 1);
                }

                if (PageIsVisible("div.llm-LastLoginModule_Button"))
                {//closing for last login time(balance) popup
                    PageClick("div.llm-LastLoginModule_Button", 100, 1);
                }
                
                if (PageIsVisible("div.pm-MessageOverlayCloseButton"))
                {//closing for reading message popup
                    PageClick("div.pm-MessageOverlayCloseButton", 100, 1);
                }

                if (PageIsVisible("div.lqb-QuickBetHeader_DoneButton"))
                {//closing for placebet betslip result box
                    PageClick("div.lqb-QuickBetHeader_DoneButton", 100, 1);
                }

                if (PageIsVisible("div.alm-InactivityAlertRemainButton"))
                {//closing for inactivity alert popup
                    PageClick("div.alm-InactivityAlertRemainButton", 100, 1);
                }

                if (PageIsVisible("div.pm-FreeBetsPushGraphicCloseButton"))
                {//closing for freebet alert popup
                    PageClick("div.pm-FreeBetsPushGraphicCloseButton", 100, 1);
                }

                if (PageIsVisible("button#KeepCurrentLimitsButton"))
                {//closing for deposit limit popup
                    PageClick("button#KeepCurrentLimitsButton", 100, 1);
                }

                if (PageIsVisible("button#btn-keep-current-setting"))
                {//closing for reality check setting
                    PageClick("button#btn-keep-current-setting", 100, 1);
                }

                if (PageIsVisible("div.bss-ReceiptContent_Done"))
                {//closing for bet success message
                    PageClick("div.bss-ReceiptContent_Done", 100, 1);
                }

                if (PageIsVisible("div.bsm-LocationErrorMessage_Close"))
                {//closing for ubilocation error message
                    PageClick("div.bsm-LocationErrorMessage_Close", 100, 1);
                }

                if (PageIsVisible("div.bs-PlaceBetErrorMessage_Remove"))
                {//closing for ubilocation error message
                    PageClick("div.bs-PlaceBetErrorMessage_Remove", 100, 1);
                }


                try
                {
                    if (PageIsVisible("div.hm-MembersMenuModuleContainer_DarkWash"))
                    {//when account context menu opens, click mouse to close it
                        try {
                            MoveMouse(70, 58);
                            Thread.Sleep(200);
                            page.Mouse.ClickAsync(70, 58);
                        }
                        catch (Exception ex)
                        {
                            
                        }
                    }
                }
                catch { }

                if (bPlacingbet)
                {
                    Thread.Sleep(200);
                    continue;
                }

                //Removing all betslip stubs
                string betslipStatus = RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets.length");
                int nBetsInSlip = 0;

                if (int.TryParse(betslipStatus, out nBetsInSlip))
                {

                    if (nBetsInSlip > 0)
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Stub Bets in Slip count: {nBetsInSlip}");
#endif
                        RunScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets[0].deleteBet()");
                    }
                }

                paramForRoute.type = TaskType.None;

                int nTaskListcount = 0;
                lock (taskLocker)
                {
                    nTaskListcount = taskList.Count;
                }
                if (nTaskListcount < 1)
                {
                    int nActKind = rnd.Next(1, 10);

                    if (nActKind < 3)
                    {
                        try
                        {
                            int x = rnd.Next(1, page.ViewportSize.Width);
                            int y = rnd.Next(1, page.ViewportSize.Height);
                            MoveMouse(x, y);
                        }
                        catch { }
                    }
                    else if (nActKind < 4)
                    {
                        try { 
                        page.Keyboard.PressAsync("Home");
                        }
                        catch { }
                    }
                    else if (nActKind < 5)
                    {
                        try { 
                        page.Keyboard.PressAsync("ArrowUp");
                        }
                        catch { }
                    }
                    else if (nActKind < 6)
                    {
                        try { 
                        page.Keyboard.PressAsync("ArrowDown");
                        }
                        catch { }
                    }
                    Thread.Sleep(100 * rnd.Next(2, 4));
                    continue;
                }
                TaskParam task = null;
                lock (taskLocker)
                {
                    task = taskList[0];
                    taskList.RemoveAt(0);
                }
                switch (task.type)
                {
                    case TaskType.None:
                        {
                            Thread.Sleep(500);
                        }
                        break;                    
                    //case TaskType.Login:
                    //    {
                    //        page_login();
                    //    }
                    //    break;
                    //case TaskType.RefreshPage:
                    //    {
                    //        try
                    //        {
                    //            page.ReloadAsync(60 * 1000).Wait();
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            LogMng.Instance.onWriteStatus($"RefreshPage Exception {ex}");
                    //        }
                    //    }
                    //    break;
                    case TaskType.GetBalance:
                        {
                            page_getBalance();
                        }
                        break;                    
                    case TaskType.Openbet:
                        {
                            page_login();

                            if (IsPageLoginStatus())
                            {
                                PageClick($"div.hm-HeaderMenuItemMyBets");
                                Thread.Sleep(2000);
                                var command = string.Format("(function () {{ {0} }})();", Global.GetOpenBetListCommandLine);

                                String result = Utils.ParseOpenBet(RunScript(command));
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus(result);
#endif
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    MessageBox.Show(result);
                                }));
                            }
                        }
                        break;
                }
            }
        }

        private PROCESS_RESULT GetNsToken(ref string ns, ref string ms, BetburgerInfo info, MAKE_SLIP_STEP Step, string betSlipString, ref string guid, ref string cc, ref string pc)
        {
            BetSlipJson betSlipJson = null;
            guid = "";
            cc = "";
            pc = "";

            try
            {
                if (Step == MAKE_SLIP_STEP.ADD_BET)
                {
                    LogMng.Instance.onWriteStatus(string.Format("[AddBet Res] {0}", betSlipString));

                    if (!string.IsNullOrEmpty(betSlipString))
                    {
                        betSlipJson = JsonConvert.DeserializeObject<BetSlipJson>(betSlipString);
                    }
                    else
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[ADD_BET|GetNsToken] Not Login (No Slip String)"));

                        return PROCESS_RESULT.CRITICAL_SITUATION;
                    }

                    if (string.IsNullOrEmpty(betSlipJson.bg))
                    {
                        if (betSlipJson.sr == 8)
                        {
                            LogMng.Instance.onWriteStatus(string.Format("[ADD_BET|GetNsToken] Not Login (No bg, sr:8)"));
                            return PROCESS_RESULT.NO_LOGIN;
                        }
                        else if (betSlipJson.sr == -1)
                        {
                            return PROCESS_RESULT.RE_FIXED;  //wait 20 min
                        }
                        LogMng.Instance.onWriteStatus(string.Format("[ADD_BET|GetNsToken] Not Login (Empty bg) res: {0}", betSlipString));
                        return PROCESS_RESULT.CRITICAL_SITUATION;
                    }
                    else
                    {
                        guid = betSlipJson.bg;
                    }

                    cc = WebUtility.UrlEncode(betSlipJson.cc);
                    pc = betSlipJson.pc;
                    if (string.IsNullOrEmpty(cc))
                    {
                        LogMng.Instance.onWriteStatus("cc is incorrect");
                        return PROCESS_RESULT.ERROR;
                    }
                }
                else if (Step == MAKE_SLIP_STEP.PLACE_BET)
                {
                    LogMng.Instance.onWriteStatus(string.Format("[PLACE_BET Res] {0}", betSlipString));

                    if (!string.IsNullOrEmpty(betSlipString))
                    {
                        betSlipJson = JsonConvert.DeserializeObject<BetSlipJson>(betSlipString);
                    }
                    else
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[PLACE_BET|GetNsToken] Error No Slip String (PLACE_BET)"));
                        return PROCESS_RESULT.CRITICAL_SITUATION;
                    }

                    if (betSlipJson.sr == 0)
                    {
                        return PROCESS_RESULT.PLACE_SUCCESS;                        
                    }
                    else if (betSlipJson.sr == 15)
                    {
                        return PROCESS_RESULT.CRITICAL_SITUATION;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"[GetNsToken] Exception Step({Step}) betSlipString({betSlipString})");
                return PROCESS_RESULT.ERROR;
            }

            bool bEachWay = false;

            ns = "";
            //ms = "";

            List<OpenBet_Bet365> openBetList = new List<OpenBet_Bet365>();
            string re = "";
            try
            {
                OpenBet_Bet365 openbet = Utils.ConvertBetburgerPick2OpenBet_365(info);


                if (openbet.betData[0].eachway && Setting.Instance.bEachWay)
                    bEachWay = true;

                openbet.betData[0].sa = $"sa={calculateSA()}";

                if (betSlipJson != null)
                {
                    if (betSlipJson.sr == 0)
                    {
                        for (int k = 0; k < betSlipJson.bt.Count; k++)
                        {
                            BetSlipItem betSlipItem = betSlipJson.bt[k];
                            if (betSlipItem.pt[0].pi != openbet.betData[0].i2)
                            {
                                continue;
                            }

                            if (betSlipItem.sr == 0)
                            {
                                if (betSlipItem.su)
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("Market is suspended"));
                                    return PROCESS_RESULT.SUSPENDED;
                                }

                                if (!string.IsNullOrEmpty(betSlipItem.sa))
                                    openbet.betData[0].sa = $"sa={betSlipItem.sa}";

                                if (!string.IsNullOrEmpty(betSlipItem.od) && openbet.betData[0].oddStr != betSlipItem.od)
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("Odd moved {0} -> {1}", openbet.betData[0].oddStr, betSlipItem.od));

                                    openbet.betData[0].oddStr = betSlipItem.od;

                                    info.direct_link = string.Format("{0}|{1}|{2}", openbet.betData[0].i2, openbet.betData[0].oddStr, openbet.betData[0].fd);
                                }

                                if (!string.IsNullOrEmpty(betSlipItem.pt[0].ha) && openbet.betData[0].ht != betSlipItem.pt[0].ha)
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("line moved {0} -> {1}", openbet.betData[0].ht, betSlipItem.pt[0].ha));
                                    openbet.betData[0].ht = betSlipItem.pt[0].ha;

                                    info.outcome = Utils.ReplaceStr(info.outcome, betSlipItem.pt[0].ha, "(", ")");
                                }

                                if (!string.IsNullOrEmpty(betSlipItem.oo))
                                    openbet.betData[0].oo = betSlipItem.oo;

                                if (betSlipItem.oc)
                                    openbet.betData[0].oc = true;

                                openbet.betData[0].ea = betSlipItem.ea || betSlipItem.ew || betSlipItem.ex;
                                openbet.betData[0].ed = betSlipItem.ed;
                            }
                            else
                            {
                                LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Error sr : {0}", betSlipItem.sr));
                            }

                            break;
                        }
                    }
                    else if (betSlipJson.sr == -2)
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Session is Locked, Retry after 5 sec"));
                        Thread.Sleep(5 * 1000);
                    }
                    else if (betSlipJson.sr == 10)
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Balance is not Enough"));
                        return PROCESS_RESULT.SMALL_BALANCE;
                    }
                    else if (betSlipJson.sr == 11 || betSlipJson.sr == 24)
                    {
                        if (openbet.betData.Count == 1)
                        {
                            double maxStake = betSlipJson.bt[0].ms;
                            if (maxStake == 0)
                            {
                                if (!string.IsNullOrEmpty(betSlipJson.bt[0].re) && Utils.ParseToDouble(betSlipJson.bt[0].re) > 0)
                                {
                                    //re = betSlipJson.bt[0].re;
                                    openbet.stake /= 2;
                                    if (openbet.stake > minMarketStake)
                                        openbet.stake = minMarketStake;
                                    Thread.Sleep(2 * 1000);
                                }
                                else
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Max Stake is 0"));
                                    return PROCESS_RESULT.ZERO_MAX_STAKE;
                                }
                            }
                            else
                            {
                                openbet.sl = true;
                                openbet.stake = maxStake;
                            }
                        }
                        else
                        {
                            if (betSlipJson.mo.Count > 0)
                            {
                                double maxStake = betSlipJson.mo[0].ms;
                                if (maxStake == 0)
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Max Stake is 0"));
                                    return PROCESS_RESULT.ZERO_MAX_STAKE;
                                }
                                openbet.sl = true;
                                openbet.stake = maxStake;
                            }
                            else
                            {
                                LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Error large than Max Stake in Combine bets, but mo result is inccorect"));
                                return PROCESS_RESULT.ERROR;
                            }
                        }
                    }
                    else if (betSlipJson.sr == 14)
                    {
                        for (int k = 0; k < betSlipJson.bt.Count; k++)
                        {
                            BetSlipItem betSlipItem = betSlipJson.bt[k];
                            if (betSlipItem.pt[0].pi != openbet.betData[0].i2)
                            {
                                continue;
                            }

                            if (!string.IsNullOrEmpty(betSlipItem.sa))
                                openbet.betData[0].sa = $"sa={betSlipItem.sa}";

                            if (betSlipItem.ms == 0)
                            {
                                re = betSlipItem.re;
                            }
                            else
                            {
                                if (openbet.stake <= betSlipItem.ms)
                                {
                                    re = betSlipItem.re;
                                }
                                else
                                {
                                    openbet.sl = true;
                                    openbet.stake = betSlipItem.ms;
                                }
                            }

                            bool bOddChanged = false;
                            if (!string.IsNullOrEmpty(betSlipItem.od) && openbet.betData[0].oddStr != betSlipItem.od)
                            {
                                LogMng.Instance.onWriteStatus(string.Format("Odd moved {0} -> {1}", openbet.betData[0].oddStr, betSlipItem.od));
                                bOddChanged = true;
                                openbet.betData[0].oddStr = betSlipItem.od;

                                info.direct_link = string.Format("{0}|{1}|{2}", openbet.betData[0].i2, openbet.betData[0].oddStr, openbet.betData[0].fd);

                                re = string.Empty;
                            }

                            if (!string.IsNullOrEmpty(betSlipItem.pt[0].ha) && openbet.betData[0].ht != betSlipItem.pt[0].ha)
                            {
                                LogMng.Instance.onWriteStatus(string.Format("line moved {0} -> {1}", openbet.betData[0].ht, betSlipItem.pt[0].ha));
                                bOddChanged = true;
                                openbet.betData[0].ht = betSlipItem.pt[0].ha;

                                info.outcome = Utils.ReplaceStr(info.outcome, betSlipItem.pt[0].ha, "(", ")");
                            }

                            if (!string.IsNullOrEmpty(betSlipItem.oo))
                                openbet.betData[0].oo = betSlipItem.oo;

                            if (betSlipItem.oc)
                                openbet.betData[0].oc = true;

                            if (bOddChanged)
                                return PROCESS_RESULT.MOVED;
                            break;
                        }
                    }
                    else if (betSlipJson.sr == 8)
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Not Login (sr:8)"));
                        return PROCESS_RESULT.NO_LOGIN;
                    }
                    else if (betSlipJson.sr == 15)
                    {

                        LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Retry later"));
                        return PROCESS_RESULT.ERROR;
                    }
                    else
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Unknown SR error {0}", betSlipJson.sr));
                        return PROCESS_RESULT.ERROR;
                    }
                }

            }
            catch { }
            try
            {
                for (int i = 0; i < openBetList.Count; i++)
                {

                    if (Step == MAKE_SLIP_STEP.INIT)
                    {// have to last bet with "id" when add bet
                        if (i == openBetList.Count - 1)
                            openBetList[i].betData[0].sa = $"id={openBetList[i].betData[0].fd}-{openBetList[i].betData[0].i2}Y";
                    }



                    if (string.IsNullOrEmpty(openBetList[i].betData[0].ht))
                        ns += $"pt=N#o={openBetList[i].betData[0].oddStr}#f={openBetList[i].betData[0].fd}#fp={openBetList[i].betData[0].i2}#so=#c={openBetList[i].betData[0].cl}#mt=22#{openBetList[i].betData[0].sa}#";
                    else
                        ns += $"pt=N#o={openBetList[i].betData[0].oddStr}#f={openBetList[i].betData[0].fd}#fp={openBetList[i].betData[0].i2}#so=#c={openBetList[i].betData[0].cl}#ln={openBetList[i].betData[0].ht}#mt=22#{openBetList[i].betData[0].sa}#";

                    if (!string.IsNullOrEmpty(openBetList[i].betData[0].oo))
                        ns += $"oto={openBetList[i].betData[0].oo}#";

                    ns += $"|TP=BS{openBetList[i].betData[0].fd}-{openBetList[i].betData[0].i2}#";

                    if (openBetList[i].betData[0].oc)
                        ns += $"olc=1#";

                    if (Step != MAKE_SLIP_STEP.INIT)
                    {
                        openBetList[i].betData[0].odd = Utils.FractionToDouble(openBetList[i].betData[0].oddStr);
                        openBetList[i].stake = Math.Truncate(openBetList[i].stake * 100) / 100;

                        if (openBetList[i].betData[0].odd == 0)
                            return PROCESS_RESULT.ERROR;

                        if ((openBetList[i].betData.Count == 1 && betSlipJson != null) || (openBetList[i].betData.Count > 1 && !openBetList[i].doubleBet))
                        {//only 1 bet or multiple bets(not double bet)
                            double tr = openBetList[i].stake * openBetList[i].betData[0].odd + 0.0001;

                            bool bCheckEachwayLine = true;

#if USOCKS || OXYLABS
Setting.Instance.bEachWay = true;
if (Setting.Instance.eachWayOdd < 4)
    Setting.Instance.eachWayOdd = 5.1;
#endif
                            if (!Setting.Instance.bEachWay)
                            {
                                bCheckEachwayLine = false;
                            }
                            else
                            {
                                if (openBetList[i].betData[0].odd < Setting.Instance.eachWayOdd)
                                    bCheckEachwayLine = false;
                            }

                            ns = $"{ns}ust={openBetList[i].stake.ToString("N2")}#st={openBetList[i].stake.ToString("N2")}#";
                            if (openBetList[i].sl)
                                ns += $"sl={openBetList[i].stake.ToString("N2")}#";

                            if (bCheckEachwayLine && openBetList[i].betData[0].cl == "2" && openBetList[i].betData[0].ea && openBetList[i].betData[0].ed != 0)
                            {
                                tr += openBetList[i].stake * Utils.FractionToDoubleOfEachway(openBetList[i].betData[0].oddStr, openBetList[i].betData[0].ed);
                                tr = Math.Truncate(tr * 100) / 100;

                                ns += $"ew=1#";
                            }
                            else
                            {
                                tr = Math.Truncate(tr * 100) / 100;
                            }

                            if (!string.IsNullOrEmpty(re))
                                ns += $"tr={re}#";
                            else
                                ns += $"tr={tr.ToString("N2")}#";
                        }
                    }
                    ns += "||";
                }

                //preparing ms
                if (betSlipJson != null)
                {
                    if (betSlipJson.dm != null)
                    {//dm parameter is for double bet

                        ms = $"id={betSlipJson.dm.bt}#bc={betSlipJson.dm.bc}#";
                        //if (infos[0].betburgerInfo.doubleBet)
                        //{
                        //    ms += $"ust={infos[0].betburgerInfo.stake.ToString("N2")}#st={infos[0].betburgerInfo.stake.ToString("N2")}#";
                        //    if (betSlipJson.dm.ea && bEachWay)
                        //        ms += $"|ew=1#";
                        //}                        
                        ms += "||";
                    }

                    //mo parameter should be added in ms even though it's individual multiple bets
                    if (betSlipJson.mo != null)
                    {
                        foreach (Dm dm in betSlipJson.mo)
                            ms += $"id={dm.bt}#bc={dm.bc}#||";
                    }
                }
            }
            catch (Exception e)
            {
            }

            return PROCESS_RESULT.SUCCESS;
        }
        private string calculateSA()
        {

            int randVal = rnd.Next(1, 15);
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + randVal;
            string aa = unixTimestamp.ToString("X2").ToLower();
            string hexValue = DateTime.Now.Ticks.ToString("X2");
            return aa + "-" + hexValue.Substring(hexValue.Length - 8, 8);
        }
        public long getTick()
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            long timestamp = (long)t.TotalMilliseconds;
            return timestamp;
        }

    }
#endif
}
