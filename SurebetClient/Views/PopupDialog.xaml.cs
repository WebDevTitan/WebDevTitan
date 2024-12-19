using Microsoft.Web.WebView2.Core;
using Project.Helphers;
using Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Project.Views
{
 
    /// <summary>
    /// Interaction logic for PopupDialog.xaml
    /// </summary>
    public partial class PopupDialog : Window
    {
        BOOKMAKER bookie;
        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);


        const uint MF_BYCOMMAND = 0x00000000;
        const uint MF_GRAYED = 0x00000001;

        const uint SC_CLOSE = 0xF060;

        public ManualResetEventSlim waitLoadingEvent = new ManualResetEventSlim();
        public PopupDialog(BOOKMAKER bookmaker)
        {
            InitializeComponent();
            bookie = bookmaker;

            this.ShowInTaskbar = true;
            this.Width = 1300;
            this.Height = 800;

            if (bookmaker == BOOKMAKER.BET365)
            {
                Global.GetMonitorPos_Main = GetWindowPos;
                Global.SetMonitorVisible_Main = ShowMonitor;
                Global.LoadHomeUrl_Main = LoadHomeUrl;
                Global.OpenUrl_Main = LoadPage;
                Global.RunScriptCode_Main = RunScript;
                Global.GetStatusValue_Main = GetValue;
                Global.GetCookie_Main = GetCookies;
                Global.GetPageSource_Main = GetPageSource;
                Global.RefreshPage_Main = RefreshPage;
                Global.RefreshBecauseBet365Notloading_Main = RefreshBecauseBet365Notloading;
                Global.ViewerHwnd_Main = new WindowInteropHelper(this).EnsureHandle();
            }
            else
            {
                Global.GetMonitorPos_Sub = GetWindowPos;
                Global.SetMonitorVisible_Sub = ShowMonitor;
                Global.LoadHomeUrl_Sub = LoadHomeUrl;
                Global.OpenUrl_Sub = LoadPage;
                Global.RunScriptCode_Sub = RunScript;
                Global.GetStatusValue_Sub = GetValue;
                Global.GetCookie_Sub = GetCookies;
                Global.GetPageSource_Sub = GetPageSource;

                Global.ViewerHwnd_Sub = new WindowInteropHelper(this).EnsureHandle();
            }

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

            result.Width = this.ActualWidth;
            result.Height = this.ActualHeight;
            return result;
            
        }
        async void InitWebView()
        {
            await webview2.EnsureCoreWebView2Async();

            

            DeleteCookie();

            if (bookie == BOOKMAKER.BET365)
            {
                webview2.CoreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;
                webview2.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
                webview2.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested1;
            }
            webview2.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            webview2.CoreWebView2.WebResourceResponseReceived += CoreWebView2_WebResourceResponseReceived;

            if (bookie == BOOKMAKER.BET365)
                webview2.Source = new Uri(string.Format("https://www.bet365.es/"));
            else
                webview2.Source = new Uri("https://www.luckia.es/");

            //Global.ViewerHwnd = webview2.Handle;
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

        public void RefreshPage()
        {
            waitLoadingEvent.Reset();
            //Global.onWriteStatus($"RefreshPage");
            this.Dispatcher.Invoke(() =>
            {            
                webview2.CoreWebView2.Reload();
            });
            waitLoadingEvent.Wait(10000);
            //Global.onWriteStatus($"RefreshPage fininshed");
        }
        private async void CoreWebView2_WebResourceResponseReceived(object sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            var uriString = e.Request.Uri;            
            try
            {
                if (bookie == BOOKMAKER.BET365)
                {
                    if (uriString.ToLower().Contains("/betswebapi/addbet"))
                    {
                        Stream stream = await e.Response.GetContentAsync();
                        StreamReader reader = new StreamReader(stream);
                        Global.strAddBetResult = reader.ReadToEnd();

                        Global.waitResponseEvent.Set();
                    }
                    else if (uriString.ToLower().Contains("/betswebapi/placebet"))
                    {


                        Stream stream = await e.Response.GetContentAsync();
                        StreamReader reader = new StreamReader(stream);
                        Global.strPlaceBetResult = reader.ReadToEnd();

                        Global.WriteTroubleShotLog($"*received {uriString.ToLower()}");
                        Global.WriteTroubleShotLog($"*received {Global.strPlaceBetResult}");

                        Global.waitResponseEvent.Set();
                    }
                }
                else
                {
                    if (uriString.ToLower().Contains("delegate/luckia/user"))
                    {
                        CoreWebView2HttpResponseHeaders headers = e.Response.Headers;
                        foreach (var value in headers)
                        {
                            string name = value.Key;
                            string valueStr = value.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //LogMng.Instance.onWriteStatus("CoreWebView2_WebResourceResponseReceived Exception " + ex);
            }
        }

        public async Task<string> GetPageSource()
        {
            string result = "";
            this.Dispatcher.Invoke(async () =>
            {
                result = await webview2.CoreWebView2.ExecuteScriptAsync("document.body.outerHTML");

            }).Wait();
            return result;
        }
        public async Task GetCookies(string domain)
        {            
            this.Dispatcher.Invoke(async () =>
            {
                
                List<CoreWebView2Cookie> cookieList = await webview2.CoreWebView2.CookieManager.GetCookiesAsync(domain);
                for (int i = 0; i < cookieList.Count; i++)
                {
                    try
                    {
                        Global.WriteTroubleShotLog($"GetCookies {cookieList[i].Domain} - {cookieList[i].Path} - {cookieList[i].Name} - {cookieList[i].Value}");

                        if (bookie == BOOKMAKER.BET365)
                            Global.cookieContainer_Main.Add(new Uri(domain), new System.Net.Cookie(cookieList[i].Name, cookieList[i].Value));
                        else
                            Global.cookieContainer_Sub.Add(new Uri(domain), new System.Net.Cookie(cookieList[i].Name, cookieList[i].Value));
                    }
                    catch (Exception ex)
                    {
                    }
                
                }
                
            }).Wait();
            
        }
        private async void CoreWebView2_WebResourceRequested1(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            var uriString = e.Request.Uri;
            try
            {
                //Global.WriteTroubleShotLog($"ProcessRequest {uriString}");

                if (bookie == BOOKMAKER.BET365)
                {
                    if (uriString.ToLower().Contains("/betswebapi/addbet") || uriString.ToLower().Contains("/betswebapi/placebet"))
                    {
                        StreamReader reader = new StreamReader(e.Request.Content);
                        string reqBody = HttpUtility.UrlDecode(reader.ReadToEnd());

                        Global.WriteTroubleShotLog($"*requested {uriString}");
                        Global.WriteTroubleShotLog($"*requested {reqBody}");
                    }
                }
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

                if (bookie == BOOKMAKER.BET365)
                {
                    Global.WriteTroubleShotLog($"LoadHomeUrl https://www.bet365.es/");

                    this.Dispatcher.Invoke(() =>
                    {
                        webview2.CoreWebView2.Navigate($"https://www.bet365.es/");
                    });
                    waitLoadingEvent.Wait(10000);
                    RefreshBecauseBet365Notloading();
                }
                else
                {

                    this.Dispatcher.Invoke(() =>
                    {
                        webview2.CoreWebView2.Navigate("https://www.luckia.es/");
                    });
                    waitLoadingEvent.Wait(10000);
                }
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
                Global.WriteTroubleShotLog($"LoadPage {url}");

                this.Dispatcher.Invoke(() =>
                {
                    webview2.CoreWebView2.Navigate(url);
                });
                waitLoadingEvent.Wait(10000);
                Global.WriteTroubleShotLog($"LoadPage {url} finished");
            }catch(Exception ex)
            {
                LogMng.Instance.onWriteStatus("LoadPage Exception " + ex);
            }
            return string.Empty;
        }
                
        public string GetValue(string param)
        {
            if (bookie == BOOKMAKER.BET365)
            {
                if (!param.Contains("hm-MainHeaderCentreWide "))
                    RefreshBecauseBet365Notloading();
            }
            ManualResetEventSlim waitResultEvent = new ManualResetEventSlim();
            string result = "";
            try
            {              
                var command = string.Format("(function () {{{0}}})();", param);

                Global.WriteTroubleShotLog($"GetValue : {command}");
                //result = webView1.EvalScript(command).ToString();
                waitResultEvent.Reset();
                this.Dispatcher.Invoke(async () =>
                {
                    try
                    {
                        var tr = await webview2.CoreWebView2.ExecuteScriptAsync(command);
                        result = tr.ToString();
                        waitResultEvent.Set();
                    }
                    catch(Exception ex)
                    {

                    }
                }).Wait();
            }
            catch (Exception ex)
            {
                result = ex.Message;
                Global.WriteTroubleShotLog("GetValue Exception: " + ex.Message);
            }
            waitResultEvent.Wait(3000);
            Global.WriteTroubleShotLog($"GetValue Res : {result}");
            return result;
        }
        public string RunScript(string param)
        {
            if (bookie == BOOKMAKER.BET365)
                RefreshBecauseBet365Notloading();

            ManualResetEventSlim waitResultEvent = new ManualResetEventSlim();
            string result = "";
            try
            {
                Global.WriteTroubleShotLog($"ScriptRun : {param}");
                waitResultEvent.Reset();
                //result = webView1.EvalScript(param).ToString();
                this.Dispatcher.Invoke(async () =>
                {
                    result = await webview2.CoreWebView2.ExecuteScriptAsync(param);
                    waitResultEvent.Set();
                }).Wait();
            }
            catch (Exception ex){
                Global.WriteTroubleShotLog("RunScript Exception: " + ex.Message);
            }
            waitResultEvent.Wait(3000);
            Global.WriteTroubleShotLog($"ScriptRun Res : {result}");
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
                RefreshPage();
                Thread.Sleep(5000);
            }            
        }
    }
}
