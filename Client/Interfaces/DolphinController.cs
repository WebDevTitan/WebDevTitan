using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using Project.Helphers;
using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace Project.Interfaces
{
    class DolphinController

    {
        private static DolphinController _instance = null;
        HttpClient m_httpClient = null;
        CookieContainer m_cookieContainer = null;
        string browser_port = "";
        string browser_socket = "";
        public Browser browser = null;
        public Page page = null;
        public string loginRespBody = string.Empty;
        public string balanceRespBody = string.Empty;
        public string eventRespBody = string.Empty;
        public string AddBetRespBody = string.Empty;
        public string PlaceBetRespBody = string.Empty;
        public bool isLogged = false;

        public string user_id = string.Empty;
        public string jwe_token = string.Empty;
        public string restore_login_message = string.Empty;
        public string user_identify_message = string.Empty;
        public List<string> websocket_request_contents = new List<string>();

        public static DolphinController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DolphinController();
                return _instance;
            }
        }

        public DolphinController()
        {
            getHttpClient();
        }
        private void getHttpClient()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            if (m_cookieContainer == null)
                m_cookieContainer = new CookieContainer();

            handler.CookieContainer = m_cookieContainer;
            m_httpClient = new HttpClient(handler);
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            m_httpClient.DefaultRequestHeaders.ExpectContinue = false;
        }

        public void InitBrowser(string url)
        {
            try
            {

                /*JObject login_payload = new JObject();
                login_payload["username"] = "chriscec028@gmail.com";
                login_payload["password"] = "Dn280809@@";

                HttpResponseMessage login_respMessage = m_httpClient.PostAsync("https://anty-api.com/auth/login", new StringContent(login_payload.ToString(), Encoding.UTF8, "application/json")).Result;

                string login_respBody = login_respMessage.Content.ReadAsStringAsync().Result;
                JObject jResp = JObject.Parse(login_respBody);*/

                //string auth_token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJhdWQiOiIyIiwianRpIjoiNGRmM2JkMTU1MzNkMWJlYjM2ZWNhZGFkZTk2YmU1OGQwM2Q2NzRiMmJkOTU5NTVmMzUzYzE0ZmUxODQxYjI0MzkwNDQyNWM4NmU5OWRiNTEiLCJpYXQiOjE3Mjg2NTY0MTEuODMwMzMsIm5iZiI6MTcyODY1NjQxMS44MzAzMzIsImV4cCI6MTcyODY2MDAxMS44MDgwNTIsInN1YiI6IjM0NzM3NjMiLCJzY29wZXMiOltdfQ.J-f-Fkx1rjU9gk9TMv915WUumRB1onYIwezTI69W1rum5_xqxLWG2SoaDsxui2RWk4GG67Evbif-o_Q5ZL3tNMNkzBJdyHSOH-WPIO2LUIP54_5gyFUApfZvFDLHHUWmD9wGfO6n261-X8IWucBnoEa8BbyQjtgvtUF27T31o7xm3KYUBNHV7HQ9Hqdpab7W4F0hEseCt55L6-L62BqLv2AN9FCMvL_0qn7xiM0Z6heF3mLsUtbRZTXSgv8BbOCwB1knPan1dYnmyYr09fi3V85dBhxSZ7jW5TT5GkH98MpJHcxJob5fjaQIOSgHfNYVHSb3I2aJ2MZk_8gN9ghASKq5Mds6RGnUO5-A55WUyk6UJ0Z-nFEjfRg2NqB13xqSj3C32LY3k05HiS2QcceCRwu5pplKfIBXigUmJ2y6WK2aMD8jwXuq2e2TBb_w_3Hgz7m0f2tV4Hpnl8T_WvIXJ13qzwMX8gcIKf1VWWRghXY9kTucx3nJlE1iR5F1a76wUdUhGUjoZ06F2cyHSxImUMSHhkOUUsPJm3HtuMV8AVbUyLU-Df2xRoAsrpihX6YOXglFsuNRk5ShIDmbAQZxXJpEJHsPHOJrWy1NTBwr4r4UpYGFk90qUl29F8ax3liucnf8oKNV-xIUQGmu0bIz1yR-d7o5wUjskAZOGtzP_88";
                string auth_token = Setting.Instance.dolphinId;
                m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {auth_token}");

                HttpResponseMessage get_profile_respMessage = m_httpClient.GetAsync("https://anty-api.com/browser_profiles").Result;

                string get_profile_respBody = get_profile_respMessage.Content.ReadAsStringAsync().Result;

                string payload = $"{{\"token\": \"{auth_token}\"}}";
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                content.Headers.ContentType.CharSet = string.Empty;

                HttpResponseMessage respMessage = m_httpClient.PostAsync("http://localhost:3001/v1.0/auth/login-with-token", content).Result;

                string respBody = respMessage.Content.ReadAsStringAsync().Result;

                HttpResponseMessage respMessage1 = m_httpClient.GetAsync($"http://localhost:3001/v1.0/browser_profiles/{Setting.Instance.profileId}/start?automation=1").Result;

                respMessage1.EnsureSuccessStatusCode();

                string body = respMessage1.Content.ReadAsStringAsync().Result;
                JObject jBody = JObject.Parse(body);

                browser_port = jBody["automation"]["port"].ToString();
                browser_socket = jBody["automation"]["wsEndpoint"].ToString();

                browser = Puppeteer.ConnectAsync(new ConnectOptions() { BrowserWSEndpoint = $"ws://localhost:{browser_port}{browser_socket}", DefaultViewport = null }).Result;

                LogMng.Instance.onWriteStatus("[Dolphin] Browser is connect now. please change resolution and wait....");
                Thread.Sleep(30000);

                browser.IgnoreHTTPSErrors = true;
                browser.DefaultWaitForTimeout = 0;

                BrowserContext _context = browser.DefaultContext;

                Page[] pages = _context.PagesAsync().Result;
                if (pages.Length > 0)
                    page = pages[0];
                else page = _context.NewPageAsync().Result;

                string innerHeight = ExecuteScript("window.outerHeight");
                string innerWidth = ExecuteScript("window.outerWidth");

                string scriptResult = File.ReadAllText("inject.txt");
                page.EvaluateExpressionOnNewDocumentAsync(scriptResult);
                page.SetViewportAsync(new ViewPortOptions
                {
                    Width = Utils.parseToInt(innerWidth),
                    Height = Utils.parseToInt(innerWidth)
                });

                page.Response += WebResourceResponseReceived;
                page.Request += WebResourceRequested;

                page.GoToAsync(url);

                var client = page.Target.CreateCDPSessionAsync().Result;
                client.SendAsync("Network.enable");
                client.MessageReceived += OnChromeDevProtocolMessage;


            }
            catch (Exception e)
            {
            }
        }
        private void OnChromeDevProtocolMessage(object sender, MessageEventArgs eventArgs)
        {
            if (eventArgs.MessageID == "Network.webSocketCreated")
            {
                Console.WriteLine($"Network.webSocketCreated: {eventArgs.MessageData}");

            }
            else if (eventArgs.MessageID == "Network.webSocketFrameSent")
            {
                Console.WriteLine($"Network.webSocketFrameSent: {eventArgs.MessageData}");
                string payloadData = eventArgs.MessageData.ToString();
                try
                {
                    JObject jPayload = JObject.Parse(payloadData);
                    payloadData = jPayload["response"]["payloadData"].ToString();
                }
                catch { }

                if (payloadData.Contains("restore_login"))
                {
                    try
                    {
                        JObject jPayload = JObject.Parse(payloadData);
                        LogMng.Instance.onWriteStatus($"Restore_Login Message -> {payloadData}");
                        restore_login_message = payloadData;
                        string auth_token = jPayload["params"]["auth_token"].ToString();
                        //jwe_token = jPayload["params"]["jwe_token"].ToString();

                    }
                    catch { }
                }
                else if (payloadData.Contains("login_encrypted"))
                {
                    LogMng.Instance.onWriteStatus($"Restore_Login Message -> {payloadData}");
                    restore_login_message = payloadData;
                }
                else if (payloadData.Contains("store_user_identification_token"))
                {
                    LogMng.Instance.onWriteStatus($"Restore_Login Message -> {payloadData}");
                    user_identify_message = payloadData;
                }
                else if (payloadData.Contains("request_session") || payloadData.Contains("partner.config") || payloadData.Contains("betting"))
                {
                    try
                    {
                        if (websocket_request_contents.Count == 0)
                            websocket_request_contents.Add(payloadData);
                        else
                        {
                            foreach (string message in websocket_request_contents)
                            {
                                if (!message.Contains("request_session"))
                                    websocket_request_contents.Add(payloadData);
                                else if (!message.Contains("partner.config"))
                                    websocket_request_contents.Add(payloadData);
                                else if (!message.Contains("beting"))
                                    websocket_request_contents.Add(payloadData);

                            }
                        }
                    }
                    catch { }
                }
            }
            else if (eventArgs.MessageID == "Network.webSocketFrameReceived")
            {
                Console.WriteLine($"Network.webSocketFrameReceived: {eventArgs.MessageData}");

                try
                {
                    string message = eventArgs.MessageData.ToString();

                    JObject jobject = JObject.Parse(message);
                    string result = jobject["response"]["payloadData"].ToString();
                    if (result.Contains("auth_token"))
                    {
                        try
                        {
                            JObject jPayload = JObject.Parse(result);
                            user_id = jPayload["data"]["user_id"].ToString();
                            string auth_token = jPayload["data"]["auth_token"].ToString();
                            //jwe_token = jPayload["data"]["jwe_token"].ToString();

                        }
                        catch { }
                    }
                    else if (result.Contains("jwe_token"))
                    {
                        try
                        {
                            JObject jPayload = JObject.Parse(result);
                            jwe_token = jPayload["data"]["jwe_token"].ToString();

                        }
                        catch { }
                    }
                    else if (result.Contains("customer_updated"))
                    {
                        try
                        {
                            JObject jPayload = JObject.Parse(result);

                        }
                        catch { }
                    }
                }
                catch (Exception ee)
                {
                }
            }
        }

        async public void WebResourceResponseReceived(object sender, ResponseCreatedEventArgs e)
        {
            try
            {
                int status = (int)e.Response.Status;
                string workingURL = e.Response.Url.ToString();

                if (workingURL.ToLower().Contains("superbet") && workingURL.Contains("/sessions/complete"))
                {
                    string responseBody = await e.Response.TextAsync();
                    JObject jResp = JObject.Parse(responseBody);

                }
                else if (workingURL.Contains("/api/v1/login?clientSourceType=Desktop_new"))
                {
                    string responseBody = await e.Response.TextAsync();
                    JObject jResp = JObject.Parse(responseBody);
                    if (jResp["notice"].ToString() == "Sukces")
                        isLogged = true;
                }
                else if (workingURL.Contains("/api/v3/getPlayerBalance?clientSourceType=Desktop_new"))
                {
                    string responseBody = await e.Response.TextAsync();
                    balanceRespBody = responseBody;
                }
                else if (workingURL.Contains("https://production-superbet-offer-basic.freetls.fastly.net/sb-basic/api/v2/en-BR/struct"))
                {
                    string responseBody = await e.Response.TextAsync();
                    eventRespBody = responseBody;
                }
                else if (workingURL.Contains("https://production-superbet-offer-basic.freetls.fastly.net/sb-basic/api/v2/en-BR/events"))
                {
                    string responseBody = await e.Response.TextAsync();
                    eventRespBody = responseBody;
                }
                else if (workingURL.Contains("legacy-web/betting/submitticket?clientSourceType=Desktop_new"))
                {
                    string responseBody = await e.Response.TextAsync();
                    PlaceBetRespBody = responseBody;
                }
                else if (workingURL.Contains("api.kto.com/auth/login"))
                {
                    string responseBody = await e.Response.TextAsync();
                    loginRespBody = responseBody;
                }
                else if (workingURL.Contains("api/Widget/GetOddsStates"))
                {
                    string responseBody = await e.Response.TextAsync();
                    AddBetRespBody = responseBody;
                }
                else if (workingURL.Contains("api/widget/GetEventDetails"))
                {
                    string responseBody = await e.Response.TextAsync();
                    eventRespBody = responseBody;
                }
                else if (workingURL.Contains("api/WidgetAuth/SignIn"))
                {
                    string responseBody = await e.Response.TextAsync();
                    loginRespBody = responseBody;
                }
                else if (workingURL.Contains("api/widget/placeWidget"))
                {
                    string responseBody = await e.Response.TextAsync();
                    PlaceBetRespBody = responseBody;
                }
                else if (workingURL.Contains("www.recaptcha.net/recaptcha/api2/reload"))
                {
                    string responseBody = await e.Response.TextAsync();
                    PlaceBetRespBody = responseBody;
                }

            }
            catch (Exception ex) { }
        }
        public void WebResourceRequested(object sender, RequestEventArgs e)
        {
            try
            {
                string requestUrl = e.Request.Url.ToString().ToLower();
                //m_handlerWriteStatus(requestUrl);
                if (requestUrl.Contains("recaptcha/enterprise.js?render="))
                {
                }
                else if (requestUrl.Contains("uicountersapi"))
                {
                }
                else if (requestUrl.Contains("refreshslip") || requestUrl.Contains("addbet"))
                {

                    foreach (var header in e.Request.Headers)
                    {
                        if (header.Key == "X-Net-Sync-Term")
                        {
                            // syncTerm = header.Value;
                        }
                    }
                }
                else if (requestUrl.Contains("placebet"))
                {

                }
            }
            catch
            {
            }
        }

        public void CloseBrowser()
        {
            try
            {
                page.CloseAsync();
                browser.CloseAsync();
            }
            catch { }
        }
        public bool NavigateInvoke(string visitUrl)
        {
            try
            {
                if (!visitUrl.StartsWith("https://")) visitUrl = "https://" + visitUrl;
                page.GoToAsync(visitUrl);
            }
            catch (Exception ex)
            {
                int a = 1;
            }
            return true;
        }
        public string ExecuteScript(string jsCode)
        {
            string result = string.Empty;
            try
            {
                //jsCode = jsCode.Replace(";", "");
                jsCode = jsCode.Replace("\n", "");
                JToken jsonResult = page.EvaluateExpressionAsync(jsCode).Result;
                result = jsonResult.ToString();
            }
            catch (Exception ex)
            {
                //m_handlerWriteStatus(ex.ToString());
            }
            return result.Replace("\"", "");
        }

        public ElementHandle GetSelection(string selection)
        {
            try
            {
                ElementHandle element_handle = page.QuerySelectorAsync(selection).Result;
                if (element_handle == null)
                    return null;

                return element_handle;
            }
            catch
            {
                return null;
            }
        }

        public bool FindAndClick(string selection, int clickCnt = 1)
        {
            try
            {
                ElementHandle element_handle = page.QuerySelectorAsync(selection).Result;
                if (element_handle == null)
                    return false;

                ClickOptions options = new ClickOptions();
                options.ClickCount = clickCnt;

                element_handle.ClickAsync(options);
            }
            catch
            {
            }
            return true;
        }

        public BoundingBox GetBoundBox(string selection)
        {
            BoundingBox box = new BoundingBox();
            try
            {
                ElementHandle element_handle = page.QuerySelectorAsync(selection).Result;
                if (element_handle == null)
                    return null;

                box = element_handle.BoundingBoxAsync().Result;
            }
            catch { }
            return box;
        }
        public void InputText(string text)
        {
            try
            {
                page.Keyboard.TypeAsync(text).Wait();
                Thread.Sleep(3000);
            }
            catch { }
        }

        public void ClickOnPoint(BoundingBox box)
        {
            page.Mouse.ClickAsync(box.X, box.Y);
        }
        public void ReloadBrowser()
        {
            page.ReloadAsync();
        }




    }
}
