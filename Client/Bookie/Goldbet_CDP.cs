using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlaywrightSharp;
using Project.Helphers;
using Project.Interfaces;
using Protocol;
using PuppeteerSharp;
using Telegram.Bot.Types;
using WebSocketSharp;
using static MasterDevs.ChromeDevTools.Protocol.Chrome.ProtocolName;
using static Project.Interfaces.CDPMouseController;

namespace Project.Bookie
{
#if (GOLDBET)
    class GoldbetCtrl : IBookieController
    {
        string domain = "";
        public HttpClient m_client = null;
        private string strIdUtente = "";
        Object lockerObj = new object();

        public GoldbetCtrl()
        {
            startListening();
        }

        public void startListening()
        {
            try
            {
                LogMng.Instance.onWriteStatus("Goldbet client is start working!");
                domain = "goldbet.it";
                if (!domain.StartsWith("www."))
                    domain = "www." + domain;
                m_client = initHttpClient();
                Global.placeBetHeaderCollection.Clear();
                if (CDPController.Instance._browserObj == null)
                    CDPController.Instance.InitializeBrowser($"https://{domain}");

                //this.login();
                //this.getBalance();
                //this.PlaceBet(ref new BetburgerInfo());
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"Exception in startListening: {ex.Message}");
            }
        }

        public bool login()
        {
            CDPController.Instance.loginRespBody = "";
            CDPController.Instance.isLogged = false;
            bool isLoggedIn = false;
            long documentId = CDPController.Instance.GetDocumentId().Result;
            try
            {

                lock (lockerObj)
                {
                    m_client = initHttpClient();

                    CDPController.Instance.NavigateInvoke($"https://{domain}/scommesse/sport");
                    Thread.Sleep(5000);

                    if (CDPController.Instance.FindElement(documentId, "button[class='anonymous--login--button']").Result)
                    {

                        bool isFound = CDPController.Instance.FindAndClickElement(documentId, "button[class='anonymous--login--button']").Result;
                        Thread.Sleep(3000);

                        isFound = CDPController.Instance.FindAndClickElement(documentId, "input[name='login_username']", 3, MoveMethod.SQRT).Result;
                        Thread.Sleep(1500);
                        CDPMouseController.Instance.InputText(Setting.Instance.username);

                        isFound = CDPController.Instance.FindAndClickElement(documentId, "input[name='login_password']", 3, MoveMethod.SQRT).Result;
                        Thread.Sleep(1500);
                        CDPMouseController.Instance.InputText(Setting.Instance.password);

                        CDPController.Instance.user_id = string.Empty;
                        isFound = CDPController.Instance.FindAndClickElement(documentId, "button[type='submit']", 1, MoveMethod.SQRT).Result;
                    }

                    Thread.Sleep(5000);
                    bool r = CDPController.Instance.FindElement(documentId, "button[class='mat-focus-indicator btn-link mat-raised-button mat-button-base']").Result;
                    int rCnt = 0;
                    while (CDPController.Instance.FindElement(documentId, "button[class='anonymous--login--button']").Result || CDPController.Instance.FindElement(documentId, "button[class='mat-focus-indicator btn-link mat-raised-button mat-button-base']").Result)
                    {
                        rCnt++;
                        Thread.Sleep(1000);
                        if (rCnt > 30)
                            break;
                    }
                    Thread.Sleep(4000);
                    if (!CDPController.Instance.FindElement(documentId, "button[class='anonymous--login--button']").Result && !CDPController.Instance.FindElement(documentId, "button[class='mat-focus-indicator btn-link mat-raised-button mat-button-base']").Result)
                        isLoggedIn = true;

                }
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus($"login exception {e.StackTrace} {e.Message}");
            }

            LogMng.Instance.onWriteStatus($"Login Result: {isLoggedIn}");
            return isLoggedIn;
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
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "it-IT,it;q=0.9,en-US;q=0.8,en;q=0.7");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.142 Safari/537.36");



            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"90\", \"Google Chrome\";v=\"90\"");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "document");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "navigate");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "none");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-User", "?1");

            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");

            httpClientEx.DefaultRequestHeaders.Add("Host", domain);

            return httpClientEx;
        }

        public double getBalance()
        {
            int nRetry = 2;
            double balance = 0;
            while (nRetry >= 0)
            {
                nRetry--;
                try
                {                   
                    balance = Convert.ToDouble(CDPController.Instance.balanceRespBody) / 100;
                }
                catch (Exception e)
                {
                    LogMng.Instance.onWriteStatus(string.Format("GetBalance exception: {0} {1}", e.Message, e.StackTrace));
                }
            }

            LogMng.Instance.onWriteStatus(string.Format("GetBalance: {0}", balance));
            return balance;        
        }

        public PROCESS_RESULT PlaceBet(ref BetburgerInfo info)
        {
            if (getBalance() < 0)
            {
                if (!login())
                {
                    return PROCESS_RESULT.NO_LOGIN;
                }
            }


            throw new NotImplementedException();
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
               

        public bool logout()
        {
            throw new NotImplementedException();
        }    
               

        public bool Pulse()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Feature()
        {
            throw new NotImplementedException();
        }

        public int GetPendingbets()
        {
            throw new NotImplementedException();
        }
    }
#endif
}
