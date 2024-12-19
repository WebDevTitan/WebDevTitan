using BetburgerServer.Constant;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BetburgerServer
{
    public partial class ViewerForm : Form
    {
        Thread thrScrape = null;
        public ViewerForm()
        {
            InitializeComponent();
            InitWebView();

            Global.OpenUrl = OpenPage;
            Global.RunScriptCode = RunScript;
            Global.GetStatusValue = GetValue;
            Global.GetCookie = GetCookies;
            Global.RefreshPage = refreshPage;
       

        }

        async void InitWebView()
        {            
            await webview2.EnsureCoreWebView2Async();
            webview2.CoreWebView2.Settings.IsWebMessageEnabled = true;

            webview2.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            webview2.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested1;

            webview2.CoreWebView2.Settings.IsWebMessageEnabled = true;
            webview2.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            webview2.CoreWebView2.WebResourceResponseReceived += CoreWebView2_WebResourceResponseReceived;
            
             
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
           
            try
            {
                Trace.WriteLine($"WebResponse: {e.Source}");
                Trace.WriteLine(e.WebMessageAsJson);

            }
            catch { }
        }

        private async void CoreWebView2_WebResourceResponseReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            if (e.Request.Method == "OPTIONS")
                return;
            var uriString = e.Request.Uri;

           
            if (uriString.Contains("api/authed/dashboard"))
            {
                foreach (var header in e.Request.Headers)
                {
                    if (header.Key == "Authorization" && Global.wait_TipstrrAuthorizationToken != header.Value) 
                    {
                        Trace.WriteLine("Authorization header is updated");
                        Global.wait_TipstrrAuthorizationToken = header.Value;
                        Global.wait_TipstrrAuthorizationEvent.Set();
                        break;
                    }
                }
            }
            else if (uriString.Contains("relyingparty/getAccountInfo"))
            {
                Stream stream = await e.Response.GetContentAsync();
                StreamReader reader = new StreamReader(stream);
                string data = reader.ReadToEnd();
                dynamic jsonVerifyContent = JsonConvert.DeserializeObject<dynamic>(data);
                if (GameConstants.TradematesportsPunterId != jsonVerifyContent.users[0].localId.ToString())
                {
                    GameConstants.TradematesportsPunterId = jsonVerifyContent.users[0].localId.ToString();
                    GetCookies(string.Empty).Wait();
                }
            }
            //else if (uriString.Contains("https://ws.tradematesports.com/socket.io"))
            //{
            //    try
            //    {
            //        Stream stream = await e.Response.GetContentAsync();
            //        StreamReader reader = new StreamReader(stream);
            //        string data = reader.ReadToEnd();
            //        int nStartIndex = data.IndexOf("{");
            //        if (nStartIndex > 0)
            //        {
            //            string substring = data.Substring(nStartIndex);
            //            Trace.WriteLine(substring);
            //            dynamic jsonResResp = JsonConvert.DeserializeObject<dynamic>(substring);
            //            string sid = jsonResResp.sid.ToString();
            //            try
            //            {
            //                Monitor.Enter(GameConstants.lockertrademateIDLists);

            //                if (GameConstants.trademateSID != sid)
            //                    GameConstants.trademateSID = sid;
            //            }
            //            catch { }
            //            finally
            //            {
            //                Monitor.Exit(GameConstants.lockertrademateIDLists);
            //            }

            //        }
            //        //foreach (var header in e.Request.Headers)
            //        //{
            //        //    if (header.Key.ToLower() == "cookie")
            //        //    {
            //        //        string[] coockies = header.Value.Split(';');
            //        //        foreach (string cookie in coockies)
            //        //        {
            //        //            if (cookie.Contains("io="))
            //        //                if (GameConstants.trademateSID != cookie.Replace("io=", string.Empty).Trim())
            //        //                    GameConstants.trademateSID = cookie.Replace("io=", string.Empty).Trim();
            //        //        }
            //        //    }
            //        //}
            //    }
            //    catch { }
            //}
        }
         
        private async void CoreWebView2_WebResourceRequested1(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            var uriString = e.Request.Uri;

            try
            {
                
                StreamReader reader = new StreamReader(e.Request.Content);
                string reqBody = reader.ReadToEnd();
                                        
                Trace.WriteLine($"WebRequest: {uriString}");
                Trace.WriteLine(reqBody);
                
            }
            catch { }
            
            try
            {                
                foreach (var header in e.Request.Headers)
                {
                    if (header.Key == "Authorization")
                    {
                        Global.wait_TipstrrAuthorizationToken = header.Value;
                        Global.wait_TipstrrAuthorizationEvent.Set();
                        break;
                    }
                }
                               
            }
            catch { }
        }
        public string GetValue(string param)
        {
            ManualResetEventSlim waitResultEvent = new ManualResetEventSlim();
            string result = "";
            try
            {
                var command = string.Format("(function () {{{0}}})();", param);


                //result = webView1.EvalScript(command).ToString();
                waitResultEvent.Reset();
                webview2.Invoke((MethodInvoker)async delegate {                
                    try
                    {
                        var tr = await webview2.CoreWebView2.ExecuteScriptAsync(command);
                        result = tr.ToString();
                        waitResultEvent.Set();
                    }
                    catch (Exception ex)
                    {

                    }
                });
            }
            catch (Exception ex)
            {
                result = ex.Message;

            }
            waitResultEvent.Wait(3000);

            return result;
        }

        public string RunScript(string param)
        {
            ManualResetEventSlim waitResultEvent = new ManualResetEventSlim();
            string result = "";
            try
            {


                waitResultEvent.Reset();

                webview2.Invoke((MethodInvoker)async delegate {
                    result = await webview2.CoreWebView2.ExecuteScriptAsync(param);
                    waitResultEvent.Set();
                });
                
            }
            catch (Exception ex)
            {
            }
            waitResultEvent.Wait(3000);

            return result;
        }
        public async Task GetCookies(string domain)
        {
            ManualResetEventSlim waitResultEvent = new ManualResetEventSlim();
            try
            {     
                waitResultEvent.Reset();

                webview2.Invoke((MethodInvoker)async delegate {
                    // Running on the UI thread

                    List<CoreWebView2Cookie> cookieList = await webview2.CoreWebView2.CookieManager.GetCookiesAsync(domain);
                    for (int i = 0; i < cookieList.Count; i++)
                    {
                        //LogMng.Instance.onWriteStatus($"{cookieList[i].Domain} - {cookieList[i].Path} - {cookieList[i].Name} - {cookieList[i].Value}");
                        try
                        {
                            Global.cookieContainer.Add(new System.Net.Cookie(cookieList[i].Name, cookieList[i].Value, cookieList[i].Path, cookieList[i].Domain));
                        }
                        catch (Exception ex) { }
                    }
                    waitResultEvent.Set();
                });
            }
            catch (Exception ex)
            {
            }
            waitResultEvent.Wait(3000);

        }
       
        public void refreshPage()
        {
            webview2.Invoke((MethodInvoker)async delegate {
                webview2.CoreWebView2.Reload();
            });
        }

        public string OpenPage(string domain)
        {
            webview2.Invoke((MethodInvoker)async delegate{            
                webview2.CoreWebView2.Navigate(domain);
            });

            return "";
        }

        public void StartTrademateSports()
        {
            
            OpenPage("https://app.tradematesports.com/tradefeed");
            
            if (thrScrape != null)
            {
                thrScrape.Abort();
                thrScrape = null;
            }
            
            thrScrape = new Thread(ScrapeFunc);
            thrScrape.Start();
            
        }

        private void ScrapeFunc()
        {
            Trace.WriteLine($"[trademate] ScrapeFunc start");

            while (true)
            {
                
                string pageIDList = RunScript("function GetAllItems() { var resultlist = []; var allItems = document.querySelectorAll('tr[id]'); for (let i = 0; i < allItems.length; i++) { var item = allItems[i]; try { var resultdata = new Object(); resultdata['id'] = item.id; resultlist.push(resultdata); } catch {} } return resultlist; } GetAllItems();");
                try
                {
                    Monitor.Enter(GameConstants.lockertrademateIDLists);
                    GameConstants.trademateIDLists.Clear();
                    //Trace.WriteLine($"[trademate] site item result: {pageIDList}");
                    dynamic resultlist = JsonConvert.DeserializeObject<dynamic>(pageIDList);
                    if (resultlist != null)
                    {
                        foreach (var resultitr in resultlist)
                        {
                            GameConstants.trademateIDLists.Add(resultitr.id.ToString());
                        }
                    }
                }
                catch (Exception ex){
                    Trace.WriteLine($"[trademate] Exception {ex}");
                }
                finally
                {
                    Monitor.Exit(GameConstants.lockertrademateIDLists);
                }
                Thread.Sleep(500);
            }
        }
        private void ViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (thrScrape != null)
            {
                thrScrape.Abort();
                thrScrape = null;
            }
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            OpenPage(txtURL.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Global.wait_BetspanLoginEvent.Set();
        }
    }
}
