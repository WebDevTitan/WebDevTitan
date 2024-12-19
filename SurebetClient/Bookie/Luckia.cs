using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project.Helphers;
using Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Bookie
{
    public class LuckiaCtrl : IBookieController
    {
        public HttpClient m_client = null;
        public string accountId = "";
        Object lockerObj = new object();
        public string jwtToken_v2 = "";
        public LuckiaCtrl()
        {
            m_client = initHttpClient();

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
            return true;
        }
        public HttpClient initHttpClient(bool bUseNewCookie = true)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            if (bUseNewCookie)
                Global.cookieContainer_Sub = new CookieContainer(300, 50, 20480);
            
#if USOCKS
            handler.Proxy = new WebProxy(string.Format("http://127.0.0.1:1080"));
            handler.UseProxy = true;
#elif OXYLABS
            handler.Proxy = new WebProxy(string.Format("pr.oxylabs.io:7777"));
            handler.Proxy.Credentials = new NetworkCredential(string.Format("customer-Iniciativasfrainsa-sesstime-30-cc-{0}-sessid-{1}", Setting.Instance.ProxyRegion, Global.ProxySessionID), "Goodluck123!@#");
            handler.UseProxy = true;
#endif
            handler.CookieContainer = Global.cookieContainer_Sub;

            HttpClient httpClientEx = new HttpClient(handler);
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "es-ES,es;q=0.9");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.142 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("X-MOD-SBB-CTYPE", "xhr");

            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"90\", \"Google Chrome\";v=\"90\"");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "Windows");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");

            return httpClientEx;
        }
        public bool login()
        {
            
            bool bLogin = false;
            try
            {
                lock (lockerObj)
                {
                    m_client = initHttpClient();
                    LogMng.Instance.onWriteStatus("Luckia login step 1");

                    Global.OpenUrl_Sub("https://www.luckia.es/usuario/#/login/");

                    string result = "null";
                    int nRetry1 = 0;
                    while (nRetry1 < 10 && Global.bRun)
                    {
                        Thread.Sleep(10000);
                        result = Global.RunScriptCode_Sub("document.getElementById('userLoginId').outerHTML").ToString();

                        LogMng.Instance.onWriteStatus($"Luckia login UserPanel status : {result}");
                        if (result.Contains("class"))
                        {
                            Thread.Sleep(2000);
                            break;
                        }
                        nRetry1++;
                    }
                    
                    if (!result.Contains("class"))
                        return false;
                    LogMng.Instance.onWriteStatus("Luckia login step 2");
                    if (!Global.bRun)
                        return false;

                    string csrtoken = "";
                    Task.Run(async () => await Global.GetCookie_Sub("https://www.luckia.es")).Wait();                    
                    foreach (Cookie cookie in Global.cookieContainer_Sub.GetCookies(new Uri("https://www.luckia.es")))
                    {
                        Global.WriteTroubleShotLog($"Find Cookies {cookie.Domain} - {cookie.Path} - {cookie.Name} - {cookie.Value}");

                        if (cookie.Name == "csrftoken")
                        {
                            csrtoken = cookie.Value;
                            break;
                        }
                    }

                    LogMng.Instance.onWriteStatus($"Luckia login step 3 csrtoken {csrtoken}");

                    string param = $"var t={{accountName: '{Setting.Instance.username_luckia}',password: '{Setting.Instance.password_luckia}',userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.45 Safari/537.36'}};";
                    Global.RunScriptCode_Sub(param);

                    //fetch("https://www.luckia.es/delegate/luckia/user/session?csrftoken=b8aa13f1-1d00-4d1c-89e6-b95479ade2a9", {
                    //method: "POST",
                    //            headers:
                    //    {
                    //        "Content-Type": "application/json",
                    //                Accept: "application/json",
                    //                "X-Requested-With": "XMLHttpRequest"
                    //            },
                    //            credentials: "same-origin",
                    //            body: JSON.stringify(t)
                    //        });
                    string command = $"fetch('https://www.luckia.es/delegate/luckia/user/session?csrftoken={csrtoken}', {{method: 'POST',headers:{{'Content-Type': 'application/json',Accept: 'application/json','X-Requested-With': 'XMLHttpRequest'}},credentials: 'same-origin',body: JSON.stringify(t)}});";
                    Global.RunScriptCode_Sub(command);

                    Global.cookieContainer_Sub = new CookieContainer(300, 50, 20480);

                    Global.OpenUrl_Sub("https://www.luckia.es/usuario/#/login/");
                    Thread.Sleep(3000);                    
                    LogMng.Instance.onWriteStatus($"Luckia login step 6");
                    Global.OpenUrl_Sub("https://www.luckia.es/apuestas/");

                    Task.Run(async () => await Global.GetCookie_Sub("https://www.luckia.es")).Wait();
                    Thread.Sleep(1000);
                    Global.OpenUrl_Sub("https://sports.luckia.es/");

                    Task.Run(async () => await Global.GetCookie_Sub("https://sports.luckia.es")).Wait();
                    LogMng.Instance.onWriteStatus($"Luckia login step 4");
                    foreach (Cookie cookie in Global.cookieContainer_Sub.GetCookies(new Uri("https://sports.luckia.es")))
                    {
                        //Global.WriteTroubleShotLog($"Find Cookies {cookie.Domain} - {cookie.Path} - {cookie.Name} - {cookie.Value}");

                        //LogMng.Instance.onWriteStatus($"Luckia login step 4.1 {cookie.Name} : {cookie.Value}");

                        if (cookie.Name == "jwtToken_v2")
                        {
                            jwtToken_v2 = cookie.Value;
                            LogMng.Instance.onWriteStatus($"Luckia login step 5 jwtToken_v2: {jwtToken_v2}");
                            break;
                        }
                    }
                    bLogin = true;
                }
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus($"login exception {e.StackTrace} {e.Message}");
            }
            return bLogin;
        }
        public PROCESS_RESULT PlaceBet(BetburgerInfo info)
        {
            int retryCount = 2;
#if OXYLABS
            retryCount = 5;
#endif
            string market_id = "", line_id = "";
            string[] directlinkArray = info.direct_link.Split(',');
            if (directlinkArray.Count() == 5)
            {
                market_id = directlinkArray[0];
                line_id = directlinkArray[2];
                
            }
            else if (directlinkArray.Count() == 3)
            {
                market_id = directlinkArray[0];
                line_id = directlinkArray[1];

            }
            else
            {
                LogMng.Instance.onWriteStatus($"Error directlink ({info.direct_link})");
                return PROCESS_RESULT.ERROR;
            }

            Global.RunScriptCode_Sub("UniSlipBlock.clearAll()").ToString();

            HttpClient sbapi_httpClient = new HttpClient();
            sbapi_httpClient.DefaultRequestHeaders.ExpectContinue = false;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            sbapi_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            sbapi_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "deflate, br");
            sbapi_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "es-ES,es;q=0.9");
            sbapi_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.142 Safari/537.36");
            
            
            sbapi_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "Windows");
            sbapi_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            sbapi_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            sbapi_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "cross-site");

            sbapi_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://sports.luckia.es");
            sbapi_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://sports.luckia.es/");


            LogMng.Instance.onWriteStatus($"Luckia PlaceBet step1");
            while (--retryCount >= 0)
            {
                try
                {

                    LogMng.Instance.onWriteStatus($"Luckia PlaceBet query jwtToken_v2: {jwtToken_v2}");

                    string homeTeam = WebUtility.HtmlEncode(info.homeTeam);
                    string SearchLink = $"https://sbapi.sbtech.com/luckia/sportscontent/sportsbook/v1/search?query={homeTeam}&sportIds=1&sportIds=2&sportIds=61&sportIds=20&sportIds=8&sportIds=6&sportIds=26&sportIds=21&sportIds=41&sportIds=39&sportIds=34&sportIds=7&sportIds=33&sportIds=27&sportIds=59&sportIds=37&sportIds=16&sportIds=15&sportIds=64&sportIds=24&sportIds=3&sportIds=25&sportIds=67&sportIds=68&sportIds=12&sportIds=66&sportIds=10&sportIds=43&sportIds=14&sportIds=63&sportIds=38&sportIds=11&sportIds=35&sportIds=13&sportIds=9&sportIds=18&sportIds=60&sportIds=36&sportIds=19&sportIds=31&sportIds=62&sportIds=42&sportIds=28&sportIds=51&sportIds=71&sportIds=70&sportIds=77&sportIds=73&sportIds=75&sportIds=78&sportIds=80&sportIds=83&sportIds=84&sportIds=89&sportIds=92&sportIds=94&sportIds=95&sportIds=108&sportIds=110&sportIds=113&sportIds=115&sportIds=119&sportIds=121&sportIds=123&sportIds=124&sportIds=126&sportIds=127&sportIds=129&sportIds=130&sportIds=131&sportIds=138&sportIds=140&sportIds=142&sportIds=144&sportIds=146&sportIds=147&sportIds=148&sportIds=157&sportIds=159&sportIds=165&sportIds=171&sportIds=176&sportIds=180&sportIds=183&sportIds=189&sportIds=195&sportIds=198&sportIds=202&sportIds=206&sportIds=207&sportIds=208&sportIds=209&sportIds=210&sportIds=211&sportIds=212&sportIds=215&sportIds=217&sportIds=218&sportIds=220&sportIds=223&sportIds=224&sportIds=225&sportIds=229&sportIds=230&sportIds=232&sportIds=233&sportIds=234&sportIds=235&sportIds=237&sportIds=245&sportIds=247&sportIds=248&sportIds=249&sportIds=253&sportIds=256&sportIds=258&sportIds=259&sportIds=260&sportIds=261&sportIds=262&sportIds=266&sportIds=267&sportIds=268&sportIds=269&sportIds=273&sportIds=274&sportIds=285&sportIds=287&sportIds=289";

                    //sbapi_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Access-Control-Request-Headers", "authorization,block-id,content-type,locale");
                    //sbapi_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Access-Control-Request-Method", "GET");

                    //var login_request = new HttpRequestMessage(HttpMethod.Options, SearchLink);
                    //var login_result = sbapi_httpClient.SendAsync(login_request).Result;

                    //sbapi_httpClient.DefaultRequestHeaders.Remove("Access-Control-Request-Headers");
                    //sbapi_httpClient.DefaultRequestHeaders.Remove("Access-Control-Request-Method");

                    sbapi_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"90\", \"Google Chrome\";v=\"90\"");
                    sbapi_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                    sbapi_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("locale", "es");
                    sbapi_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("block-id", "Left_BetSearchReactBlock_18160");

                    sbapi_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + jwtToken_v2);
                    


                    HttpResponseMessage eventResponseMessage = sbapi_httpClient.GetAsync(SearchLink).Result;
                    eventResponseMessage.EnsureSuccessStatusCode();
                    string eventContent = eventResponseMessage.Content.ReadAsStringAsync().Result;
                    //byte[] recvdata = eventResponseMessage.Content.ReadAsByteArrayAsync().Result;
                    //string eventContent = Encoding.UTF8.GetString(recvdata);
                    LogMng.Instance.onWriteStatus($"Luckia PlaceBet query result2: {eventContent}");
                    dynamic eventJson = JsonConvert.DeserializeObject<dynamic>(eventContent);

                    string eventID = "";                    
                    foreach (var itemEvent in eventJson)
                    {
                        string teamName = itemEvent.eventName.ToString().ToLower();
                        if (teamName.Contains(info.homeTeam.ToLower()) && teamName.Contains(info.awayTeam.ToLower()))
                        {
                            eventID = itemEvent.id.ToString();
                            break;
                        }                        
                    }

                    if (string.IsNullOrEmpty(eventID))
                    {
                        LogMng.Instance.onWriteStatus("Error event not found");
                        return PROCESS_RESULT.ERROR;
                    }

                    //m_client.DefaultRequestHeaders.Remove("Content-Type");
                    //m_client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                    m_client.DefaultRequestHeaders.Remove("RequestTarget");
                    m_client.DefaultRequestHeaders.TryAddWithoutValidation("RequestTarget", "AJAXService");

                    string eventDetailsUrl = $"https://sports.luckia.es/methods/masterevents.ashx/getResponsiveMasterEvent?eventID={eventID}&mode=0&isYourBetEnabled=false";
                    HttpResponseMessage eventDetailsResponseMessage = m_client.GetAsync(eventDetailsUrl).Result;
                    eventDetailsResponseMessage.EnsureSuccessStatusCode();
                    string eventDetails = eventDetailsResponseMessage.Content.ReadAsStringAsync().Result;
                    LogMng.Instance.onWriteStatus($"Luckia PlaceBet event detail result: {eventDetails}");
                    dynamic eventDetailsJson = JsonConvert.DeserializeObject<dynamic>(eventDetails);
                    string url = "https://sports.luckia.es/es" + eventDetailsJson[0][30];

                    //eventDetailsUrl = $"https://sports.luckia.es/pagemethods_ros.aspx/UpdateEvents";
                    //string ReqJson = $"requestString={directlinkArray[0]}";
                    //var postData = new StringContent(ReqJson, Encoding.UTF8, "application/x-www-form-urlencoded");
                    //HttpResponseMessage marketDetailsResponseMessage = m_client.PostAsync(eventDetailsUrl, postData).Result;
                    //marketDetailsResponseMessage.EnsureSuccessStatusCode();
                    //string marketDetails = marketDetailsResponseMessage.Content.ReadAsStringAsync().Result;

                    //Open match url
                    //var params = [25729388, 0, 1];
                    //var path = "https://sports.luckia.es/es/sports/f%C3%BAtbol/espa%C3%B1a-la-liga/20220102/atl%C3%A9tico-de-madrid-vs-r-vallecano/";
                    //Application.navigateTo("pre-live-betting", false, params, path, false);

                    LogMng.Instance.onWriteStatus($"Luckia PlaceBet url: {url} marketid: {market_id} lineid: {line_id}");
                    Global.RunScriptCode_Sub($"var params = [{eventID}, 0, 1];var path = '{url}';Application.navigateTo('pre-live-betting', false, params, path, false);").ToString();

                    Thread.Sleep(3000);

                    string betButton = Global.GetStatusValue_Sub(string.Format("return document.querySelector('button[data-params*=\"{0}\"][data-params*=\"{1}\"]').outerHTML", market_id, line_id)).ToLower();
                    if (betButton.Contains("class"))
                    {
                        Global.RunScriptCode_Sub(string.Format("document.querySelector('button[data-params*=\"{0}\"][data-params*=\"{1}\"]').click();", market_id, line_id));
                        string stake_id = "null";
                        int nRetry = 5;
                        while (nRetry-- >= 0)
                        {
                            Thread.Sleep(500);
                            //set stake
                            string StakeInputHtml = Global.GetStatusValue_Sub(string.Format("return document.querySelector('input[data-uat*=\"bet-slip-stakebox\"][id*=\"stake_\"]').id")).ToLower();

                            stake_id = StakeInputHtml.Replace("stake_", "").Replace("\"", "");
                            LogMng.Instance.onWriteStatus($"Luckia PlaceBet StakeInputHtml: {StakeInputHtml} stake_id: {stake_id} info.stake: {info.stake} nRetry: {nRetry}");

                            if (stake_id != "null")
                                break;
                        }

                        if (stake_id == "null")
                        {
                            LogMng.Instance.onWriteStatus("Can't find stake inputbox!");
                            return PROCESS_RESULT.ERROR;
                        }
                        

                        Global.RunScriptCode_Sub($"document.getElementById('stake_{stake_id}').value='{info.stake.ToString("N2")}'").ToString();
                        Thread.Sleep(200);

                        Global.RunScriptCode_Sub($"UniSlipBlock.setStake({stake_id}, document.getElementById('stake_{stake_id}'))").ToString();
                        Thread.Sleep(200);
                        //click bet button
                        Global.RunScriptCode_Sub("UniSlipBlock.placeBets()").ToString();                        
                        return PROCESS_RESULT.PLACE_SUCCESS;
                    }
                }
                catch(Exception ex)
                {
                    LogMng.Instance.onWriteStatus($"Placebet excetpion {ex}");
                }
            }
            LogMng.Instance.onWriteStatus(string.Format("** PLACE BET FAIL"));
            return PROCESS_RESULT.ERROR;
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
                    HttpResponseMessage balanceResponseMessage = m_client.GetAsync(string.Format("https://www.luckia.es/delegate/luckia/user/wallet")).Result;
                    balanceResponseMessage.EnsureSuccessStatusCode();

                    string balanceContent = balanceResponseMessage.Content.ReadAsStringAsync().Result;
                    JObject balanceObj = JObject.Parse(balanceContent);

                    balance = Utils.ParseToDouble(balanceObj["total"].ToString());
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

    }
}
