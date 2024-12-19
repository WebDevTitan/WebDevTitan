using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Web.WebView2.Core;

namespace Project.Views
{

    /// <summary>
    /// Interaction logic for PopupDialog.xaml
    /// </summary>
    public partial class PopupDialog : Window
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);


        const uint MF_BYCOMMAND = 0x00000000;
        const uint MF_GRAYED = 0x00000001;

        const uint SC_CLOSE = 0xF060;

        public ManualResetEventSlim waitLoadingEvent = new ManualResetEventSlim();
        public PopupDialog()
        {
            InitializeComponent();


            this.ShowInTaskbar = true;
            this.Width = 1300;
            this.Height = 800;


            Global.GetMonitorPos = GetWindowPos;
            Global.SetMonitorVisible = ShowMonitor;
            Global.LoadHomeUrl = LoadHomeUrl;
            Global.OpenUrl = LoadPage;
            Global.RunScriptCode = RunScript;
            Global.GetStatusValue = GetValue;
            Global.GetCookie = GetCookies;
            Global.RemoveCookies = DeleteCookie;
            Global.RefreshPage = RefreshPage;
            Global.GetPageSource = GetPageSource;
            Global.RefreshBecauseBet365Notloading = RefreshBecauseBet365Notloading;

            Global.ViewerHwnd = new WindowInteropHelper(this).EnsureHandle();

            webview2.DefaultBackgroundColor = System.Drawing.Color.Transparent;
            webview2.DesignModeForegroundColor = System.Drawing.Color.Transparent;
            InitWebView();

        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Disable close button
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            IntPtr hMenu = GetSystemMenu(hwnd, false);
            if (hMenu != IntPtr.Zero)
            {
                EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
            }
        }

        public Rect GetWindowPos()
        {
            Rect result = new Rect();
            this.Dispatcher.Invoke(() =>
            {
                try
                {

                    Point locationFromWindow = webview2.TranslatePoint(new Point(0, 0), this);

                    Point locationFromScreen = webview2.PointToScreen(locationFromWindow);

                    result.X = locationFromScreen.X;
                    result.Y = locationFromScreen.Y;
                    result.Width = webview2.ActualWidth;
                    result.Height = webview2.ActualHeight;
                }
                catch { }
            });
            return result;

        }
        async void InitWebView()
        {
            await webview2.EnsureCoreWebView2Async();

            webview2.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
            webview2.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
            webview2.CoreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;

#if (!BETMGM)
            DeleteCookie();
#endif

#if (UNIBET || BET365_BM || GOLDBET || _888SPORT || LOTTOMATICA || BETMGM || BETWAY)
            webview2.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            webview2.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested1;
#endif
            webview2.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            webview2.CoreWebView2.WebResourceResponseReceived += CoreWebView2_WebResourceResponseReceived;

            //#if (TROUBLESHOT)
            //            webview2.CoreWebView2.OpenDevToolsWindow();
            //#endif

#if (BET365_BM)
#if (CHRISTIAN)
            webview2.Source = new Uri(string.Format("https://www.nj.{0}", Setting.Instance.domain));
#else
                webview2.Source = new Uri(string.Format("https://www.{0}", Setting.Instance.domain));
#endif

#elif (SNAI)
            webview2.Source = new Uri("https://www.snai.it/sport");
#elif (PLANETWIN)
            webview2.Source = new Uri("https://www.planetwin365.it/it/scommesse-live");
#elif (EUROBET)
            webview2.Source = new Uri("https://www.eurobet.it");
#endif

            //this.Dispatcher.Invoke(() =>
            //{
            //    webview2.CoreWebView2.Navigate("https://en.surebet.com/users/sign_in");
            //});

            //Global.ViewerHwnd = webview2.Handle;
        }

        private void CoreWebView2_PermissionRequested(object sender, CoreWebView2PermissionRequestedEventArgs e)
        {
            if (e.PermissionKind == CoreWebView2PermissionKind.Geolocation)
            {
                e.State = CoreWebView2PermissionState.Allow;
            }
        }

        private void CoreWebView2_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            waitLoadingEvent.Set();
        }

        public void DeleteCookie()
        {
            this.Dispatcher.Invoke(async () =>
            {
                try
                {
                    webview2.CoreWebView2.CookieManager.DeleteAllCookies();
                }
                catch (Exception ex)
                {

                }
            }).Wait();
        }
        public void RefreshPage()
        {
#if (TROUBLESHOT)
            //LogMng.Instance.onWriteStatus("[Monitor] RefreshPage Start");                    
#endif

            waitLoadingEvent.Reset();
            //Global.onWriteStatus($"RefreshPage");
            this.Dispatcher.Invoke(() =>
            {
                try
                {
#if (TROUBLESHOT)
                    //         LogMng.Instance.onWriteStatus($"Reload before");
#endif
                    webview2.CoreWebView2.Reload();
#if (TROUBLESHOT)
                    //         LogMng.Instance.onWriteStatus($"Reload after");
#endif
                }
                catch { }
            });
            waitLoadingEvent.Wait(10000);

#if (BET365_BM)
            int nRetryCount = 5;
            while (nRetryCount-- >= 0)
            {
                string result = GetValue("return document.getElementsByClassName('hm-MainHeaderCentreWide ')[0].outerHTML");
                if (result.Contains("class"))
                {
                    break;
                }
                Thread.Sleep(3000);
            }
            Thread.Sleep(3000);
#endif

            RunScript("document.getElementsByClassName('pm-MessageOverlayCloseButton ')[0].click();");
#if (TROUBLESHOT)
            //  LogMng.Instance.onWriteStatus("[Monitor] RefreshPage End");                    
#endif
        }
        private async void CoreWebView2_WebResourceResponseReceived(object sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            if (e.Request.Method == "OPTIONS")
                return;
            var uriString = e.Request.Uri;
            try
            {
#if (BET365_BM)
                if (uriString.ToLower().Contains("/betswebapi/addbet"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strAddBetResult = reader.ReadToEnd();

#if (TROUBLESHOT)
          //          LogMng.Instance.onWriteStatus($"addbet Res: {Global.strAddBetResult}");
#endif
                    
                    Global.waitResponseEvent.Set();
                }
                else if (uriString.ToLower().Contains("/betswebapi/placebet"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strPlaceBetResult = reader.ReadToEnd();

#if (TROUBLESHOT)
           //         LogMng.Instance.onWriteStatus($"*placebet Res: {Global.strPlaceBetResult}");
#endif

                                        
                    Global.waitResponseEvent.Set();
                }
#elif (WINAMAX)
                if (uriString.Contains("authentication/token/authorize"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse1 = reader.ReadToEnd();

                    Global.waitResponseEvent1.Set();
                }
                else if (uriString.Contains("betting/validate_betslip.php"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strPlaceBetResult = reader.ReadToEnd();

                    Global.waitResponseEvent.Set();
                }
#elif (PINNACLE)
                if (e.Request.Method == "POST" && uriString.Contains("guest.api.arcadia.pinnacle.com/0.1/sessions"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    string response = reader.ReadToEnd();
                    dynamic loginResult = JsonConvert.DeserializeObject<dynamic>(response);

                    try
                    {
                        if (loginResult != null && loginResult.trustCode != null && loginResult.token != null)
                        {
                            Global.pinnacleHeaders.Clear();
                            foreach (var header in e.Request.Headers)
                            {
                                if (header.Key.ToLower().StartsWith("x-"))
                                {
                                    LogMng.Instance.onWriteStatus($"pinnacle header captured: {header.Key}:{header.Value}");
                                    Global.pinnacleHeaders.Add(new KeyValuePair<string, string>(header.Key, header.Value));                                    
                                }
                            }

                            Global.pinnacleHeaders.Add(new KeyValuePair<string, string>("x-session", loginResult.token.ToString()));

                            Setting.Instance.usernameBet365 = loginResult.trustCode.ToString();
                            Global.waitResponseEvent1.Set();
                        }                        
                    }
                    catch { }
                    
                }              
#elif (ELYSGAME)
                if (uriString.Contains("odit-apielysgame.odissea-services.net/token"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse1 = reader.ReadToEnd();

                    Global.waitResponseEvent1.Set();
                }
                else if (uriString.Contains("betting/validate_betslip.php"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strPlaceBetResult = reader.ReadToEnd();

                    Global.waitResponseEvent.Set();
                }
#elif (BETMGM)
                if (Global.betMGMrequestQueries.Count < 1 &&
                    uriString.Contains("x-bwin-accessid") &&
                    uriString.Contains("lang") &&
                    uriString.Contains("country") &&
                    uriString.Contains("userCountry") &&
                    uriString.Contains("subdivision"))
                {
                    Uri myUri = new Uri(uriString);

                    NameValueCollection collection = HttpUtility.ParseQueryString(myUri.Query);
                    if (!string.IsNullOrEmpty(collection.Get("x-bwin-accessid")) &&
                        !string.IsNullOrEmpty(collection.Get("lang")) &&
                        !string.IsNullOrEmpty(collection.Get("country")) &&
                        !string.IsNullOrEmpty(collection.Get("userCountry")) &&
                        !string.IsNullOrEmpty(collection.Get("subdivision")))
                    {
                        //LogMng.Instance.onWriteStatus($"set Request queries. {collection.ToString()}");
                        Global.betMGMrequestQueries = collection;
                    }
                }
               
                if (uriString.Contains("en/api/balance/refresh?forceFresh=1") ||
                    uriString.Contains("en/api/clientconfig/partial?configNames=vnBalanceProperties"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strAddBetResult = reader.ReadToEnd();

                    //LogMng.Instance.onWriteStatus($"Response url: {uriString} Res: {Global.strAddBetResult}");

                    Global.waitResponseEvent.Set();
                }
                else if (uriString.Contains("/bettingoffer/fixture-view?"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    string result = reader.ReadToEnd();
                    if (result != "{\"splitFixtures\":[]}")
                    {
                        Global.strWebResponse1 = result;

                        //LogMng.Instance.onWriteStatus($"Response url: {uriString} Res: {Global.strWebResponse1}");

                        Global.waitResponseEvent1.Set();
                    }
                }
                else if(uriString.Contains("bettingoffer/picks?x-bwin-accessid"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse2 = reader.ReadToEnd();

                    Stream stream1 = e.Request.Content;
                    StreamReader reader1 = new StreamReader(stream1);
                    string request = reader1.ReadToEnd();

                    //LogMng.Instance.onWriteStatus($"Response url: {uriString} origrequest: {Global.strPlaceBetResult} request: {request} Res: {Global.strWebResponse2}");

                    if (Global.strPlaceBetResult == request)
                    {
                        Global.waitResponseEvent2.Set();
                    }
                }
                else if(uriString.Contains("api/placebet?forceFresh=1"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse3 = reader.ReadToEnd();

                    Stream stream1 = e.Request.Content;
                    StreamReader reader1 = new StreamReader(stream1);
                    string request = reader1.ReadToEnd();

                    //LogMng.Instance.onWriteStatus($"Response url: {uriString} request: {request} Res: {Global.strWebResponse3}");

                    Global.waitResponseEvent3.Set();
                }                

#elif (PLANETWIN)
                if (uriString.Contains("https://www.planetwin365.it/"))
                {
                    
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    string fromContents = reader.ReadToEnd();

                    if (fromContents.Contains("Scommessa inserita correttamente"))
                    {
                        Global.waitResponseEvent.Set();
                    }
                }
#elif (BETWAY)
                if (uriString.Contains("/api/Account/v3/LogIn"))
                {                    
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream); 
                    Global.strWebResponse1 = reader.ReadToEnd();
                    Global.waitResponseEvent1.Set();                    
                }
                else if(uriString.Contains("/api/Account/v3/Info"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse3 = reader.ReadToEnd();
                    string temp = Global.strWebResponse3;
                    Global.waitResponseEvent3.Set();
                }
                else if (uriString.Contains("/api/Events/v2/GetEventMarkets"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse2 = reader.ReadToEnd();
                    Global.waitResponseEvent2.Set();
                }
                else if (uriString.Contains("/api/Betting/v3/BuildBets"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse1 = reader.ReadToEnd();                    
                    Global.waitResponseEvent1.Set();
                }
                else if (uriString.Contains("/api/Betting/v3/InitiateBets"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse2 = reader.ReadToEnd();
                    Global.waitResponseEvent2.Set();
                }
                else if (uriString.Contains("/api/Betting/v3/LookupBets"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse4 = reader.ReadToEnd();
                    Global.waitResponseEvent4.Set();
                }
#elif (NOVIBET)
                if (uriString.Contains("/ngapi/en/useraccount/login"))
                {                    
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream); 
                    Global.strWebResponse1 = reader.ReadToEnd();
                    Global.waitResponseEvent1.Set();                    
                }
                else if (uriString.Contains("/ngapi/en/useraccount/updateFunds"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse2 = reader.ReadToEnd();
                    Global.waitResponseEvent2.Set();
                }
                else if (uriString.Contains("/ngapi/en/betslip/toggleitem"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse1 = reader.ReadToEnd();
                    Global.waitResponseEvent1.Set();
                }
                else if (uriString.Contains("/ngapi/en/betslip/submit"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse2 = reader.ReadToEnd();
                    Global.waitResponseEvent2.Set();
                }
                else if (uriString.Contains("/ngapi/en/betslip/querydelayed"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse3 = reader.ReadToEnd();
                    Global.waitResponseEvent3.Set();
                }
                else if (uriString.Contains("/ngapi/en/betslip/acceptchanges"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strAddBetResult = reader.ReadToEnd();
                    Global.waitResponseEvent.Set();
                }
                else if (uriString.Contains("/ngapi/en/openbets/updateforchanges"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse4 = reader.ReadToEnd();
                    Global.waitResponseEvent4.Set();
                }
#elif (DOMUSBET || BETALAND)
                if (e.Request.Method == "GET" && uriString.Contains("XSportDatastore/getEvento"))
                {
                    if (uriString.ToLower() == Global.strWebResponse2ReqUrl.ToLower())
                    {
                        Stream stream = await e.Response.GetContentAsync();
                        StreamReader reader = new StreamReader(stream);
                        Global.strWebResponse2 = reader.ReadToEnd();
                        Global.waitResponseEvent2.Set();
                    }
                }
                else if (e.Request.Method == "POST" && uriString.Contains("XPayments/initSession"))
                {

                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    string resultData = reader.ReadToEnd();
                    if (resultData.Contains("\"resultCode\":0"))
                    {

                        Stream stream1 = e.Request.Content;
                        StreamReader reader1 = new StreamReader(stream1);
                        string request = reader1.ReadToEnd();


                        Global.DomusbetToken = Utils.Between(request, "productToken=");
                        //LogMng.Instance.onWriteStatus($"Token updated from login: {Global.DomusbetToken}");
                    }
                    Global.waitResponseEvent.Set();
                }
                else if (e.Request.Method == "POST" && uriString.Contains("updateBalance"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strAddBetResult = reader.ReadToEnd();
                    Global.waitResponseEvent.Set();
                }
                else if (e.Request.Method == "POST" && uriString.Contains("/getLivePurchaseParameters"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse4 = reader.ReadToEnd();
                    Global.waitResponseEvent4.Set();                                        
                }
                else if (e.Request.Method == "POST" && uriString.Contains("/purchase"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse1 = reader.ReadToEnd();
                    Global.waitResponseEvent1.Set();

                    Stream stream1 = e.Request.Content;
                    StreamReader reader1 = new StreamReader(stream1);
                    string request = reader1.ReadToEnd();

                    //LogMng.Instance.onWriteStatus($"Purchase Request captured {uriString}");
                    //LogMng.Instance.onWriteStatus(request);
                    //LogMng.Instance.onWriteStatus("Response-------------");
                    //LogMng.Instance.onWriteStatus(Global.strWebResponse1);
                }

                try
                {
                    Uri myUri = new Uri(uriString);
                    string tokenStr = HttpUtility.ParseQueryString(myUri.Query).Get("token");
                    if (!string.IsNullOrEmpty(tokenStr) && tokenStr != Global.DomusbetToken)
                    {
                        Global.DomusbetToken = tokenStr;
                        //LogMng.Instance.onWriteStatus($"Token updated from url: {Global.DomusbetToken}");
                    }
                }
                catch { }
#elif (EUROBET)
                if (e.Request.Method == "POST" && Global.strRequestUrl == uriString)
                {

                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strPlaceBetResult = reader.ReadToEnd();
                    Global.waitResponseEvent.Set();
                }
#elif (LOTTOMATICA || GOLDBET)
                if (e.Request.Method == "POST" && uriString.Contains("login"))
                {

                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strPlaceBetResult = reader.ReadToEnd();
                    Global.waitResponseEvent.Set();
                }
                else if (e.Request.Method == "POST" && (uriString.Contains("ContoDiGioco/getbalanceheader") || uriString.Contains("getBalance")))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strAddBetResult = reader.ReadToEnd();
                    Global.waitResponseEvent.Set();
                }
                else if (e.Request.Method == "POST" && uriString.Contains("verificaAccesso/lwtOtpDeviceRegister"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strAddBetResult = reader.ReadToEnd();
                }
                else if (Global.strRequestUrl == uriString)
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strPlaceBetResult = reader.ReadToEnd();
                    Global.waitResponseEvent.Set();
                }
#elif (BETFAIR_NEW)
                if (uriString.ToLower().Contains("v1/implybets") && e.Request.Method == "POST")
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse1 = reader.ReadToEnd();
                    Global.waitResponseEvent1.Set();
                }
                else  if (uriString.ToLower().Contains("v1/placebet") && e.Request.Method == "POST")
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse2 = reader.ReadToEnd();
                    Global.waitResponseEvent2.Set();
                }
                else if (uriString.ToLower().Contains("/wallet-service/v3.0/wallets"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strAddBetResult = reader.ReadToEnd();
                    Global.waitResponseEvent.Set();
                }
#elif (BETFAIR_FETCH || BETFAIR)
                if (uriString.ToLower().Contains("action=addselection") || uriString.ToLower().Contains("action=confirm"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    string content = reader.ReadToEnd();
                    Global.strPlaceBetResult = reader.ReadToEnd();
                    Global.waitResponseEvent.Set(); 
                }
                else if (uriString.ToLower().Contains("/wallet-service/v3.0/wallets"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strAddBetResult = reader.ReadToEnd();
                    Global.waitResponseEvent.Set();
                }
#elif (SUPERBET)
                if (e.Request.Method == "POST" && uriString.Contains("/api/v1/login?clientSourceType=Desktop_new"))
                {

                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strPlaceBetResult = reader.ReadToEnd();
                    Global.waitResponseEvent.Set();
                }
                else if (e.Request.Method == "POST" && (uriString.Contains("/api/v3/getPlayerBalance?clientSourceType=Desktop_new") || uriString.Contains("getBalance")))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strPlaceBetResult = reader.ReadToEnd();
                    Global.waitResponseEvent.Set();
                }
                else if (e.Request.Method == "POST" && uriString.Contains("https://production-superbet-offer-basic.freetls.fastly.net/sb-basic/api/v2/en-BR/events"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strPlaceBetResult = reader.ReadToEnd();
                    Global.waitResponseEvent.Set();
                }
                else if (Global.strRequestUrl == uriString)
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strPlaceBetResult = reader.ReadToEnd();
                    Global.waitResponseEvent.Set();
                }  
#elif (STOIXIMAN || BETANO)
                if (uriString.ToLower().Contains("/myaccount/api/tokens"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strPlaceBetResult = reader.ReadToEnd();
                    Global.waitResponseEvent.Set();
                }
                else if (uriString.Contains("myaccount/api/ma/customer/balance") || uriString.Contains("/api/balance?_"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse1 = reader.ReadToEnd();
                    Global.waitResponseEvent1.Set();
                }
                else if (uriString.Contains("api/betslip/v3/plain-leg"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse2 = reader.ReadToEnd();
                    Global.waitResponseEvent2.Set();
                }
                else if (uriString.Contains("api/betslip/v3/updatebets"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse3 = reader.ReadToEnd();
                    Global.waitResponseEvent3.Set();
                }
                else if (uriString.Contains("api/betslip/v3/place"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strWebResponse4 = reader.ReadToEnd();
                    Global.waitResponseEvent4.Set();
                }
                else if (uriString.Contains("api/betslipcombo/limits"))
                {
                    Stream stream = await e.Response.GetContentAsync();
                    StreamReader reader = new StreamReader(stream);
                    Global.strPlaceBetResult = reader.ReadToEnd();
                    Global.waitResponseEvent.Set();
                }
#elif (_888SPORT)
                if (uriString.Contains("punter/login.json"))
                {
                    try
                    {
                        Stream stream = await e.Response.GetContentAsync();
                        StreamReader reader = new StreamReader(stream);

                        string sessionInfo = reader.ReadToEnd();

                        string apikeyStr = HttpUtility.UrlDecode(sessionInfo);
                        Global.unibetSessionInfo = JsonConvert.DeserializeObject<UnibetSessionInfo>(apikeyStr);

                        try
                        {
                            Monitor.Enter(Global.locker_unifiedclientHeaders);
                            Global.kambicdnHeaders.Clear();
                            foreach (var header in e.Request.Headers)
                            {
                                Global.kambicdnHeaders.Add(new KeyValuePair<string, string>(header.Key, header.Value));
                            }
                        }
                        catch { }
                        finally
                        {
                            Monitor.Exit(Global.locker_unifiedclientHeaders);
                        }
                        LogMng.Instance.onWriteStatus("SessionInfo captured");
                    }
                    catch { }
                }                
#elif (BETPLAY)
                if (uriString.Contains("player/api/v2019/betplay/punter/login.json"))
                {
                    try
                    {
                        Stream stream = await e.Response.GetContentAsync();
                        StreamReader reader = new StreamReader(stream);

                        string temp = reader.ReadToEnd();
                        
                        Global.strAddBetResult = temp;
                        Global.waitResponseEvent.Set();
                    }
                    catch { }
                }
#elif (RUSHBET)
                if (uriString.Contains("player/api/v2019/rsico/punter/login.json"))
                {
                    try
                    {
                        Stream stream = await e.Response.GetContentAsync();
                        StreamReader reader = new StreamReader(stream);

                        string temp = reader.ReadToEnd();
                        
                        Global.strAddBetResult = temp;
                        Global.waitResponseEvent.Set();
                    }
                    catch { }
                }
                else if (uriString.Contains("api/service/sessions/portal/auth/login"))
                {
                    try
                    {
                        Stream stream = await e.Response.GetContentAsync();
                        StreamReader reader = new StreamReader(stream);

                        string temp = reader.ReadToEnd();

                        Global.strWebResponse1 = temp;
                        Global.waitResponseEvent1.Set();
                    }
                    catch { }
                }
#endif
            }
            catch (Exception ex)
            {
                //LogMng.Instance.onWriteStatus("CoreWebView2_WebResourceResponseReceived Exception " + ex);
            }
        }

        public async Task<string> GetPageSource()
        {
#if (TROUBLESHOT)
            //      LogMng.Instance.onWriteStatus($"GetPageSource");
#endif

            if (Application.Current.Dispatcher.Thread == Thread.CurrentThread)
            {
#if (TROUBLESHOT)
                //      LogMng.Instance.onWriteStatus($"GetPageSource can't run from ui thread");
#endif
                return "Exception";
            }

            string result = "";
            App.Current.Dispatcher.Invoke(async () =>
            {
                try
                {
#if (TROUBLESHOT)
                    //          LogMng.Instance.onWriteStatus($"ExecuteScriptAsync before: {result}");
#endif
                    result = await webview2.CoreWebView2.ExecuteScriptAsync("document.body.outerHTML");
#if (TROUBLESHOT)
                    //          LogMng.Instance.onWriteStatus($"ExecuteScriptAsync after: {result}");
#endif
                }
                catch { }
            }).Wait();

#if (TROUBLESHOT)
            //   LogMng.Instance.onWriteStatus($"GetPageSource result: {result}");
#endif

            return result;
        }
        public async Task GetCookies(string domain)
        {
            this.Dispatcher.Invoke(async () =>
            {
#if (TROUBLESHOT)
                //       LogMng.Instance.onWriteStatus("GetCookie started");
#endif
                List<CoreWebView2Cookie> cookieList = await webview2.CoreWebView2.CookieManager.GetCookiesAsync(domain);
#if (TROUBLESHOT)
                //       LogMng.Instance.onWriteStatus("GetCookie after");
#endif

                for (int i = 0; i < cookieList.Count; i++)
                {
                    //LogMng.Instance.onWriteStatus($" Capturing coockies: {cookieList[i].Domain} - {cookieList[i].Path} - {cookieList[i].Name} - {cookieList[i].Value}");
                    try
                    {
                        Global.cookieContainer.Add(new System.Net.Cookie(cookieList[i].Name, cookieList[i].Value, cookieList[i].Path, cookieList[i].Domain));
                    }
                    catch (Exception ex) { }
                }
            }).Wait();

        }
        private async void CoreWebView2_WebResourceRequested1(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            var uriString = e.Request.Uri;
            try
            {
#if (GOLDBET || LOTTOMATICA)
                //if (uriString.ToLower().Contains("https://www.goldbet.it/") || uriString.ToLower().Contains("https://www.lottomatica.it/"))
                //{
                //    if (Global.placeBetHeaderCollection.Count <= 0)
                //    {                        
                //        foreach (var header in e.Request.Headers)
                //        {
                //            if (header.Key == "X-NewRelic-ID")
                //            {
                //                LogMng.Instance.onWriteStatus($"X-NewRelic-ID header captured: {header.Value}");
                //                Global.placeBetHeaderCollection.Add(header.Key, header.Value);
                //                break;
                //            }
                //        }
                //    }
                //}
#elif (UNIBET)
                if (uriString.ToLower().Contains("punter/login.json") && (e.Request.Method.ToString() == "POST"))
                {//for fetching fingerprintHash
                    StreamReader reader = new StreamReader(e.Request.Content);
                    string reqBody = HttpUtility.UrlDecode(reader.ReadToEnd());
                    string hash = Utils.Between(reqBody, "fingerprintHash\":\"", "\"");
                    Global.strPlaceBetResult = hash;
                    Global.waitResponseEvent.Set();
                }
#elif (BETWAY)
                if (uriString.ToLower().Contains("/api/account/v3/login") && (e.Request.Method.ToString() == "POST"))
                {
                    StreamReader reader = new StreamReader(e.Request.Content);
                    string reqBody = HttpUtility.UrlDecode(reader.ReadToEnd());
                    
                    Global.strPlaceBetResult = reqBody;
                    Global.waitResponseEvent.Set();
                }
#elif (_888SPORT)
                if (uriString.ToLower().Contains("punter/login.json") && (e.Request.Method.ToString() == "POST"))
                {//for fetching fingerprintHash
                    StreamReader reader = new StreamReader(e.Request.Content);
                    string reqBody = HttpUtility.UrlDecode(reader.ReadToEnd());
                    string hash = Utils.Between(reqBody, "fingerprintHash\":\"", "\"");
                    Global.strPlaceBetResult = hash;

                    
                    Global.waitResponseEvent.Set();
                }
            
                if (uriString.ToLower().Contains("https://unifiedclient.safe-iplay.com/api") && (e.Request.Method.ToString() == "POST" || e.Request.Method.ToString() == "GET"))
                { //for getting balance
                    try
                    {
                        Monitor.Enter(Global.locker_unifiedclientHeaders);
                        Global.unifiedclientHeaders.Clear();
                        foreach (var header in e.Request.Headers)
                        {
                            Global.unifiedclientHeaders.Add(new KeyValuePair<string, string>(header.Key, header.Value));
                        }
                    }
                    catch { }
                    finally { 
                        Monitor.Exit(Global.locker_unifiedclientHeaders);
                    }
                }
#elif (BETMGM)
                
                if (uriString.ToLower().Contains("bettingoffer/counts-batch?"))
                {
                    if (e.Request.Method == "POST")
                    {
                        try
                        {
                            LogMng.Instance.onWriteStatus($"Retreive x headers from url: {uriString}");
                            Monitor.Enter(Global.locker_unifiedclientHeaders);
                            Global.kambicdnHeaders.Clear();
                            foreach (var header in e.Request.Headers)
                            {
                                Global.kambicdnHeaders.Add(new KeyValuePair<string, string>(header.Key, header.Value));
                                LogMng.Instance.onWriteStatus($"Retrieving headers: {header.Key} : {header.Value}");
                            }
                        }
                        catch { }
                        finally
                        {
                            Monitor.Exit(Global.locker_unifiedclientHeaders);
                        }
                    }
                }
#endif

            }
            catch (Exception ex)
            {
            }
        }

        public void ShowMonitor(bool bVisible)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (bVisible)
                    {
                        this.Width = 1300;
                        this.Height = 800;

                        double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                        double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                        double windowWidth = this.Width;
                        double windowHeight = this.Height;
                        this.Left = (screenWidth / 2) - (windowWidth / 2);
                        this.Top = (screenHeight / 2) - (windowHeight / 2);

                        this.ShowInTaskbar = true;

                    }
                    else
                    {
                        this.Width = 0;
                        this.Height = 0;
                        this.ShowInTaskbar = false;
                    }
                });
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus("ShowMonitor Exception " + ex);
            }
        }


        public void LoadHomeUrl()
        {
            try
            {
                waitLoadingEvent.Reset();
#if (BET365_BM)

#if (TROUBLESHOT)
                //LogMng.Instance.onWriteStatus($"LoadHomeUrl https://www.{Setting.Instance.domain}");
                
#endif
                

            this.Dispatcher.Invoke(() =>
            {
#if (CHRISTIAN)
                webview2.CoreWebView2.Navigate($"https://www.nj.{Setting.Instance.domain}");
#else
                webview2.CoreWebView2.Navigate($"https://www.{Setting.Instance.domain}");
#endif
            });
            waitLoadingEvent.Wait(10000);
            RefreshBecauseBet365Notloading();
#elif (SNAI)
            //this.Dispatcher.Invoke(() =>
            //{
            //    webview2.CoreWebView2.Navigate("https://www.snai.it/sport");
            //});
            //waitLoadingEvent.Wait(10000);            
#elif (PLANETWIN)
                this.Dispatcher.Invoke(() =>
                {
                    webview2.CoreWebView2.Navigate("https://www.planetwin365.it/it/scommesse-live");
                });
                waitLoadingEvent.Wait(10000);
#endif
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus("ShowMonitor Exception " + ex);
            }
        }


        public string LoadPage(string url)
        {
            try
            {
                waitLoadingEvent.Reset();

#if (TROUBLESHOT)
                //        LogMng.Instance.onWriteStatus($"LoadPage {url}");

#endif


                this.Dispatcher.Invoke(() =>
                {
                    try
                    {
#if (TROUBLESHOT)
                        //              LogMng.Instance.onWriteStatus($"Navigate before");

#endif
                        webview2.CoreWebView2.Navigate(url);
#if (TROUBLESHOT)
                        //              LogMng.Instance.onWriteStatus($"Navigate after");

#endif
                    }
                    catch { }
                });
#if (!BETFAIR)
                waitLoadingEvent.Wait(10000);
#endif

#if (TROUBLESHOT)
                //        LogMng.Instance.onWriteStatus($"LoadPage {url} finished");

#endif

            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus("LoadPage Exception " + ex);
            }
            return string.Empty;
        }

        public string GetValue(string param)
        {
#if (BET365_BM)
            if (!param.Contains("hm-MainHeaderCentreWide "))
                RefreshBecauseBet365Notloading();
#endif
            ManualResetEventSlim waitResultEvent = new ManualResetEventSlim();
            string result = "";
            try
            {
                var command = string.Format("(function () {{{0}}})();", param);

#if (TROUBLESHOT)
                //        LogMng.Instance.onWriteStatus($"GetValue : {command}");

#endif

                if (Application.Current.Dispatcher.Thread == Thread.CurrentThread)
                {
#if (TROUBLESHOT)
                    //           LogMng.Instance.onWriteStatus($"GetValue can't run from ui thread");
#endif
                    return "Exception";
                }

                //result = webView1.EvalScript(command).ToString();
                waitResultEvent.Reset();
                App.Current.Dispatcher.Invoke(async () =>
                {
                    try
                    {
#if (TROUBLESHOT)
                        //               LogMng.Instance.onWriteStatus($"getValue before");
#endif

                        var tr = await webview2.CoreWebView2.ExecuteScriptAsync(command);

#if (TROUBLESHOT)
                        //               LogMng.Instance.onWriteStatus($"getValue after");
#endif

                        result = tr.ToString();
                        waitResultEvent.Set();
                    }
                    catch (Exception ex)
                    {

                    }
                }).Wait();
            }
            catch (Exception ex)
            {
                result = ex.Message;

#if (TROUBLESHOT)
                //       LogMng.Instance.onWriteStatus("GetValue Exception: " + ex.Message);

#endif

            }
            waitResultEvent.Wait(3000);

#if (TROUBLESHOT)
            //      LogMng.Instance.onWriteStatus($"GetValue Res : {result}");

#endif

            return result;
        }
        public string RunScript(string param)
        {
            if (Application.Current.Dispatcher.Thread == Thread.CurrentThread)
            {
#if (TROUBLESHOT)
                //         LogMng.Instance.onWriteStatus($"runScript can't run from ui thread");
#endif
                return "Exception";
            }

#if (BET365_BM)
            RefreshBecauseBet365Notloading();
#endif
            ManualResetEventSlim waitResultEvent = new ManualResetEventSlim();
            string result = "";
            try
            {
#if (TROUBLESHOT)
                //         LogMng.Instance.onWriteStatus($"ScriptRun : {param}");

#endif

                waitResultEvent.Reset();
                //result = webView1.EvalScript(param).ToString();
                App.Current.Dispatcher.Invoke(async () =>
                {
                    try
                    {
#if (TROUBLESHOT)
                        //               LogMng.Instance.onWriteStatus($"runScript before");
#endif
                        result = await webview2.CoreWebView2.ExecuteScriptAsync(param);
#if (TROUBLESHOT)
                        //               LogMng.Instance.onWriteStatus($"runScript after");
#endif
                    }
                    catch { }
                    waitResultEvent.Set();
                }).Wait();
            }
            catch (Exception ex)
            {

#if (TROUBLESHOT)
                //       LogMng.Instance.onWriteStatus("RunScript Exception: " + ex.Message);

#endif

            }
            waitResultEvent.Wait(3000);

#if (TROUBLESHOT)
            //    LogMng.Instance.onWriteStatus($"ScriptRun Res : {result}");

#endif

            return result.ToLower();
        }


        public void RefreshBecauseBet365Notloading()
        {
            int nRetry = 0;
            while (nRetry++ < 3)
            {
                string result = GetValue("return document.getElementsByClassName('hm-MainHeaderCentreWide ')[0].outerHTML");
                if (result.Contains("class"))
                {
                    break;
                }

#if (TROUBLESHOT)
                //     LogMng.Instance.onWriteStatus("[Monitor] RefreshBecauseBet365Notloading");                    
#endif

                RefreshPage();
                Thread.Sleep(5000);
            }
        }
    }
}
