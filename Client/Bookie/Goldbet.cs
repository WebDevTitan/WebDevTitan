using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Emulation;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlaywrightSharp;
using PlaywrightSharp.Har;
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
        Object lockerObj = new object();
        //private static ProxyServer _proxyServer = null;
        private string strIdUtente = "";   
        public GoldbetCtrl()
        {
            domain = "goldbet.it"; 
            if (!domain.StartsWith("www."))
                domain = "www." + domain;
            m_client = initHttpClient();            
            if (CDPController.Instance._browserObj == null)
                CDPController.Instance.InitializeBrowser($"https://{domain}");
            

#if (LOTTOMATICA)
            domain = "www.lottomatica.it";
#endif
        }
             
        

        public bool login()
        {
            CDPController.Instance.loginRespBody = "";
            CDPController.Instance.isLogged = false;
            bool isLoggedIn = true;
            //long documentId = CDPController.Instance.GetDocumentId().Result;
            //try
            //{

            //    lock (lockerObj)
            //    {
            //        m_client = initHttpClient();

            //        CDPController.Instance.NavigateInvoke($"https://{domain}/scommesse/sport");

            //        Thread.Sleep(15000);

            //        if (CDPController.Instance.FindElement(documentId, "button[class='anonymous--login--button']").Result)
            //        {

            //            bool isFound = CDPController.Instance.FindAndClickElement(documentId, "button[class='anonymous--login--button']").Result;
            //            Thread.Sleep(3000);

            //            isFound = CDPController.Instance.FindAndClickElement(documentId, "input[name='login_username']", 3, MoveMethod.SQRT).Result;
            //            Thread.Sleep(1500);
            //            CDPMouseController.Instance.InputText(Setting.Instance.username);

            //            isFound = CDPController.Instance.FindAndClickElement(documentId, "input[name='login_password']", 3, MoveMethod.SQRT).Result;
            //            Thread.Sleep(1500);
            //            CDPMouseController.Instance.InputText(Setting.Instance.password);

            //            CDPController.Instance.user_id = string.Empty;
            //            isFound = CDPController.Instance.FindAndClickElement(documentId, "button[type='submit']", 1, MoveMethod.SQRT).Result;


            //            Thread.Sleep(5000);
            //            bool r = CDPController.Instance.FindElement(documentId, "button[class='mat-focus-indicator btn-link mat-raised-button mat-button-base']").Result;
            //            int rCnt = 0;
            //            while (CDPController.Instance.FindElement(documentId, "button[class='anonymous--login--button']").Result || CDPController.Instance.FindElement(documentId, "button[class='mat-focus-indicator btn-link mat-raised-button mat-button-base']").Result)
            //            {
            //                rCnt++;
            //                Thread.Sleep(1000);
            //                if (rCnt > 30)
            //                    break;
            //            }
            //        }
            //        Thread.Sleep(4000);
            //        if (!CDPController.Instance.FindElement(documentId, "button[class='anonymous--login--button']").Result && !CDPController.Instance.FindElement(documentId, "button[class='mat-focus-indicator btn-link mat-raised-button mat-button-base']").Result)
            //            isLoggedIn = true;

            //    }
            //}
            //catch (Exception e)
            //{
            //    LogMng.Instance.onWriteStatus($"login exception {e.StackTrace} {e.Message}");
            //}

            LogMng.Instance.onWriteStatus("Login Result: Just to let you know, login is unneed.");
            return isLoggedIn;
        }

        public string getProxyLocation()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage resp= httpClient.GetAsync("http://lumtest.com/myip.json").Result;
                var strContent = resp.Content.ReadAsStringAsync().Result;
                dynamic json = JObject.Parse(strContent);                                
                return json["geo"]["region_name"].ToString() + " - " + json["country"].ToString();
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"getProxyLocation exception {ex.StackTrace} {ex.Message}");
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

        private string[] parts;
        private string authToken = "";
        private string Idau = "";
        private string get_Balance = "";
        public PROCESS_RESULT PlaceBet(ref BetburgerInfo info)
        {
            OpenBet_Goldbet openbet = Utils.ConvertBetburgerPick2OpenBet_Goldbet(info);
            
            if (openbet == null)
            {
                LogMng.Instance.onWriteStatus("Converting OpenBet Error");
                return PROCESS_RESULT.ERROR;
            }
            try
            {
                lock (lockerObj)
                {
                    string aamsId = "";
                    string catId = "";
                    string disId = "";
                    string evnDate = "";
                    string evnDateTs = "";
                    string evtId = "";
                    string evtName = "";                    
                    string markId = "";
                    string markName = "";
                    string markTypId = "";
                    string oddsId = "";
                    string oddsValue = "";
                    string onLineCode = "";
                    string prvIdEvt = "";
                    string selId = "";
                    string selName = "";
                    string tId = "";
                    string tName = "";
                    string vrt = "";
                    string sNL = "";
                    string idSlt = "";                    
                    double NewOdd = 0;
                    double curOdds = 0;
                    string selIDs = "";
                    string sportName = "";
                    string categoryDescription = "";
                    //string userName = Setting.Instance.username;
                    //string password = Setting.Instance.password;
                    //string hashedPassword = Hashpassword(password);
                    //string authTokenAndIdau = GetAuthAndIdUtente(hashedPassword, userName);
                    //string[] parts = authTokenAndIdau.Split('/');
                    //string authToken = parts[0];
                    //string Idau = parts[1];
                    //string get_Balance = parts[2];




                    if (info.isLive)
                    {


                        try
                        {
                            string getLiveOddURL = "https://" + domain + "/scommesse/getOverviewLive/?idDiscipline=0&idTab=0&menu=menu&isFromUser=false";
                            JObject getLiveOddsJOB = new JObject
                            {
                                ["headers"] = new JObject
                                {
                                    ["accept"] = "application/json, text/plain, */*",
                                    ["accept-language"] = "en-US,en;q=0.9",
                                    ["content-type"] = "application/json",
                                    ["priority"] = "u=1, i",
                                    ["sec-ch-ua"] = "\"Google Chrome\";v=\"131\", \"Chromium\";v=\"131\", \"Not_A Brand\";v=\"24\"",
                                    ["sec-ch-ua-mobile"] = "?0",
                                    ["sec-ch-ua-platform"] = "\"Windows\"",
                                    ["sec-fetch-dest"] = "empty",
                                    ["sec-fetch-mode"] = "cors",
                                    ["sec-fetch-site"] = "same-origin",
                                    ["x-acceptconsent"] = "true",
                                    ["x-brand"] = "1",
                                    ["x-idcanale"] = "1"
                                },
                                ["referrer"] = "https://" + domain + "/scommesse/sport/",
                                ["referrerPolicy"] = "strict-origin-when-cross-origin",
                                ["body"] = JValue.CreateNull(),
                                ["method"] = "GET",
                                ["mode"] = "cors",
                                ["credentials"] = "include"
                            };

                            string responseData = "";
                            string functionString = $"var link = ''; fetch(\"{getLiveOddURL}\", {getLiveOddsJOB}).then(res=>res.json()).then(json=>{{link = json}});";
                            CDPController.Instance.ExecuteScript(functionString);
                            Thread.Sleep(5000);
                            int count = 0;
                            while (count < 20)
                            {
                                responseData = CDPController.Instance.ExecuteScript("JSON.stringify(link)", true, true);
                                if (!string.IsNullOrEmpty(responseData))
                                    break;
                                Thread.Sleep(1000);
                                count++;
                            }

                            if (string.IsNullOrEmpty(responseData))
                            {
                                LogMng.Instance.onWriteStatus("getliveOdds request error");
                                return PROCESS_RESULT.ERROR;
                            }

                            var strEventContent = responseData;


#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus("all live sports--");
                            LogMng.Instance.onWriteStatus(strEventContent);
#endif

                            double maxSimilarity = 0;
                            string EventdetailUrl = string.Empty;

                            JObject origObject3 = JObject.Parse(strEventContent);
                            foreach (var objEvent in origObject3["leo"])
                            {
                                double similarity = Similarity.GetSimilarityRatio(objEvent["enm"].ToString(), info.eventTitle, out double ratio1, out double ratio2);

                                if (similarity > maxSimilarity)
                                {
                                    maxSimilarity = similarity;

                                    aamsId = objEvent["aid"].ToString();
                                    catId = objEvent["cid"].ToString();
                                    disId = objEvent["sid"].ToString();
                                    evnDate = objEvent["edt"].ToString();
                                    evnDateTs = objEvent["edts"].ToString();
                                    evtId = objEvent["eid"].ToString();
                                    evtName = objEvent["enm"].ToString();
                                    onLineCode = objEvent["ocd"].ToString();
                                    prvIdEvt = objEvent["eprId"].ToString();
                                    tId = objEvent["tid"].ToString();
                                    tName = objEvent["tdsc"].ToString();
                                    vrt = objEvent["vrt"].ToString().ToLower();
                                    EventdetailUrl = string.Format("https://" + domain + "/scommesse/getDetailsEventLive/{0}/{1}", objEvent["sid"].ToString(), objEvent["eid"].ToString());
                                }
                            }

                            if (!string.IsNullOrEmpty(tName))
                            {
                                double similarity1 = Similarity.GetSimilarityRatio(tName, info.league, out double ratio11, out double ratio21);
                                if (similarity1 < 50)
                                {
                                    EventdetailUrl = "";
                                }
                            }

                            if (string.IsNullOrEmpty(EventdetailUrl))
                            {
                                LogMng.Instance.onWriteStatus("Didn't find target event");
                                return PROCESS_RESULT.ERROR;
                            }

                            bool bFound = false;

                            CDPController.Instance.strRequestUrl = EventdetailUrl;
                            string baseURL = "https://" + domain;
                            JObject requestObject = new JObject
                            {
                                ["method"] = "GET",
                                ["headers"] = new JObject
                                {
                                    ["accept"] = "*/*",
                                    ["accept-language"] = "es,en-US;q=0.9,en;q=0.8,fr;q=0.7",
                                    ["content-type"] = "application/json"
                                },
                                ["credentials"] = "include",
                                ["referrerPolicy"] = "strict-origin-when-cross-origin",
                                ["referrer"] = baseURL
                            };
                            string responseDatacheck = "";
                            string functionstring1 = $"var link = ''; fetch(\"{CDPController.Instance.strRequestUrl}\", {requestObject}).then(res=>res.json()).then(json=>{{link = json}});";
                            CDPController.Instance.ExecuteScript(functionstring1);
                            Thread.Sleep(4000);
                            int count1 = 0;
                            while (count < 20)
                            {
                                responseDatacheck = CDPController.Instance.ExecuteScript("JSON.stringify(link)", true, true);
                                if (!string.IsNullOrEmpty(responseDatacheck))
                                    break;
                                Thread.Sleep(1000);
                                count1++;
                            }
                            if (string.IsNullOrEmpty(responseDatacheck))
                            {
                                LogMng.Instance.onWriteStatus("GetAllLivesMatches Error");
                                return PROCESS_RESULT.ERROR;
                            }


                            string strContent = responseDatacheck;
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus("target match--");
                            LogMng.Instance.onWriteStatus(strContent);
#endif
                            JObject origObject4 = JObject.Parse(strContent);
                            foreach (var objEvent in origObject4["mktWbG"])
                            {
                                JToken matchObj = objEvent.ToObject<JProperty>().Value;

                                foreach (var marketEvent in matchObj["ms"])
                                {
                                    JToken marketObj = marketEvent.ToObject<JProperty>().Value;
                                    foreach (var piEvent in marketObj["asl"])
                                    {
                                        if (piEvent["oi"].ToString() == info.direct_link)
                                        {                                            
                                            markId = piEvent["mi"].ToString();
                                            markName = matchObj["mn"].ToString();
                                            markTypId = piEvent["mti"].ToString();
                                            oddsId = piEvent["oi"].ToString();
                                            oddsValue = piEvent["ov"].ToString().Replace(",", ".");
                                            selId = piEvent["si"].ToString();
                                            selName = piEvent["sn"].ToString();
                                            sNL = piEvent["sNL"].ToString();
                                            bFound = true;
                                            break;
                                        }
                                    }
                                    if (bFound)
                                        break;
                                }
                                if (bFound)
                                    break;
                            }

                            if (!bFound)
                            {
                                LogMng.Instance.onWriteStatus("Didn't find target market(pi)");
                                return PROCESS_RESULT.ERROR;
                            }

                            NewOdd = Utils.ParseToDouble(oddsValue);
                            curOdds = NewOdd;
                            LogMng.Instance.onWriteStatus($"SelNewOdd : {NewOdd}");
                            selIDs = selId;
                            LogMng.Instance.onWriteStatus($"SelId: {selIDs} ready for bet");

                            if (CheckOddDropCancelBet(NewOdd, info))
                            {
                                LogMng.Instance.onWriteStatus(string.Format("Odd is changed from {0} To {1}", info.odds, NewOdd.ToString("N2")));
                                return PROCESS_RESULT.ERROR;
                            }

                            string strinsert_Result = string.Empty;
                            int retry = 5;
                            while (--retry > 0)
                            {
                                string responseData_insertBet = "";

                                string bodyArray = $"{{\"TagType\":12,\"Username\":\"{Setting.Instance.username}\",\"AuthToken\":\"{authToken}\",\"UserId\":{Idau},\"IdUtente\":{Idau},\"IdUser\":{Idau},\"Payload\":{{\"ReservationMaker\":3,\"allowOddChanges\":true,\"allowStakeReduction\":true,\"creationTime\":{Utils.getTick()},\"events\":[{{\"categoryDescription\":\"{categoryDescription}\",\"selId\":{selId},\"evtName\":\"{evtName}\",\"evnDate\":\"{evnDate}\",\"tName\":\"{tName}\",\"markName\":\"{markName}\",\"markId\":{markId},\"selName\":\"{selName}\",\"oddsValue\":{oddsValue},\"markMultipla\":0,\"isLive\":false,\"oddsId\":{oddsId},\"evtId\":{evtId},\"disId\":{disId},\"catId\":{catId},\"tId\":{tId},\"aamsId\":\"{aamsId}\",\"onLineCode\":{onLineCode},\"firstTeam\":\"{info.homeTeam}\",\"secondTeam\":\"{info.awayTeam}\",\"idSelectionType\":0,\"markTypId\":0,\"aamsGamePlay\":0,\"sportName\":\"{sportName}\",\"vrt\":false,\"spreadId\":\"0\",\"stake\":{Setting.Instance.stakeSports},\"isRigiocoVillaggio\":false}}],\"fixed\":[],\"groupCombs\":{{\"combsInfo\":[{{\"combNum\":1,\"combType\":1,\"stake\":{Setting.Instance.stakeSports}}}],\"sumCombsXType\":1}},\"totalStake\":{Setting.Instance.stakeSports},\"virtual\":false,\"bonusWager\":false}},\"idCanale\":1}}";

                                string insertBetUrl = "https://" + domain + "/api/sport/book/insertBet";
                                var insertBetObj = new JObject
                                {
                                    ["headers"] = new JObject
                                    {
                                        ["accept"] = "application/json, text/plain, */*",
                                        ["accept-language"] = "en-US,en;q=0.9",
                                        ["content-type"] = "application/json",
                                        ["priority"] = "u=1, i",
                                        ["sec-ch-ua"] = "\"Google Chrome\";v=\"131\", \"Chromium\";v=\"131\", \"Not_A Brand\";v=\"24\"",
                                        ["sec-ch-ua-mobile"] = "?0",
                                        ["sec-ch-ua-platform"] = "\"Windows\"",
                                        ["sec-fetch-dest"] = "empty",
                                        ["sec-fetch-mode"] = "cors",
                                        ["sec-fetch-site"] = "same-origin",
                                        ["x-acceptconsent"] = "true",
                                        ["x-auth-iduser"] = Idau,
                                        ["x-auth-token"] = authToken,
                                        ["x-auth-username"] = Setting.Instance.username,
                                        ["x-brand"] = "1",
                                        ["x-idcanale"] = "1"
                                    },
                                    ["referrer"] = "https://" + domain + "/scommesse/" + info.siteUrl,
                                    ["referrer"] = string.Format("https://{0}/scommesse/{1}", domain, info.siteUrl),
                                    ["referrerPolicy"] = "strict-origin-when-cross-origin",
                                    ["body"] = bodyArray,
                                    ["method"] = "POST",
                                    ["mode"] = "cors",
                                    ["credentials"] = "include"
                                };
                                string functionstring_InsertBet = $"var link = ''; fetch(\"{insertBetUrl}\", {insertBetObj}).then(res=>res.json()).then(json=>{{link = json}});";
                                CDPController.Instance.ExecuteScript(functionstring_InsertBet);
                                Thread.Sleep(4000);
                                int count2 = 0;
                                while (count2 < 20)
                                {
                                    responseData_insertBet = CDPController.Instance.ExecuteScript("JSON.stringify(link)", true, true);
                                    if (!string.IsNullOrEmpty(responseData_insertBet))
                                        break;
                                    Thread.Sleep(1000);
                                    count2++;
                                }

                                if (string.IsNullOrEmpty(responseData_insertBet))
                                {
                                    LogMng.Instance.onWriteStatus("insertbet no response");
                                    return PROCESS_RESULT.ERROR;
                                }

                                strinsert_Result = responseData_insertBet;
                                break;

                            }
                            dynamic jsonBetResp = JsonConvert.DeserializeObject<dynamic>(strinsert_Result);

                            if (jsonBetResp.success.ToString() == "True")
                            {
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus(string.Format("Placed bet in Step 1"));
#endif
                                string confirmBetUrl = "https://" + domain + "/api/sport/book/pendingBet";
                                string strConfirmBetResp = string.Empty;
                                retry = 3;
                                while (--retry > 0)
                                {
                                    try
                                    {
                                        JObject requestObjectPending = new JObject
                                        {
                                            ["headers"] = new JObject
                                            {
                                                ["accept"] = "application/json, text/plain, */*",
                                                ["accept-language"] = "en-US,en;q=0.9",
                                                ["content-type"] = "application/json",
                                                ["priority"] = "u=1, i",
                                                ["sec-ch-ua"] = "\"Google Chrome\";v=\"131\", \"Chromium\";v=\"131\", \"Not_A Brand\";v=\"24\"",
                                                ["sec-ch-ua-mobile"] = "?0",
                                                ["sec-ch-ua-platform"] = "\"Windows\"",
                                                ["sec-fetch-dest"] = "empty",
                                                ["sec-fetch-mode"] = "cors",
                                                ["sec-fetch-site"] = "same-origin",
                                                ["x-acceptconsent"] = "true",
                                                ["x-auth-iduser"] = Idau,
                                                ["x-auth-token"] = $"{authToken}",
                                                ["x-auth-username"] = "silvilore",
                                                ["x-brand"] = "1",
                                                ["x-idcanale"] = "1"
                                            },
                                            ["referrer"] = "https://" + domain + "/scommesse/" + info.siteUrl,
                                            ["referrer"] = string.Format("https://{0}/scommesse/{1}", domain, info.siteUrl),
                                            ["referrerPolicy"] = "strict-origin-when-cross-origin",
                                            ["body"] = $"{{\"AuthToken\":\"{authToken}\",\"UserId\":{Idau},\"IdUtente\":{Idau},\"IdUser\":{Idau},\"idCanale\":1,\"CouponCode\":\"{jsonBetResp.data.couponCode}\"}}",
                                            ["method"] = "POST",
                                            ["mode"] = "cors",
                                            ["credentials"] = "include"
                                        };
                                        string responsedata = "";
                                        string functionstring_pending = $"var link = ''; fetch(\"{confirmBetUrl}\", {requestObjectPending}).then(res=>res.json()).then(json=>{{link = json}});";
                                        CDPController.Instance.ExecuteScript(functionstring_pending);
                                        Thread.Sleep(4000);
                                        int count3 = 0;
                                        while (count3 < 20)
                                        {
                                            responsedata = CDPController.Instance.ExecuteScript("JSON.stringify(link)", true, true);
                                            if (!string.IsNullOrEmpty(responsedata))
                                                break;
                                            Thread.Sleep(1000);
                                            count3++;
                                        }

                                        if (string.IsNullOrEmpty(responsedata))
                                        {
                                            continue;
                                        }

                                        strConfirmBetResp = responsedata;
#if (TROUBLESHOT)
                                        LogMng.Instance.onWriteStatus("pendingBet Res:" + strConfirmBetResp);
#endif

                                        break;
                                    }
                                    catch (Exception ex)
                                    {
                                        LogMng.Instance.onWriteStatus("pendingBet exception:" + ex);
                                    }
                                }
                                dynamic jsonConfirmBetResp = JsonConvert.DeserializeObject<dynamic>(strConfirmBetResp);
                                LogMng.Instance.onWriteStatus($"Placing failed Reason: {jsonConfirmBetResp.statusCode}");
                                if (jsonConfirmBetResp.statusCode.ToString() == "Placed" || jsonConfirmBetResp.statusCode.ToString() == "P")
                                {
                                    return PROCESS_RESULT.PLACE_SUCCESS;
                                }

                                return PROCESS_RESULT.ERROR;
                            }
                            else if (jsonBetResp.error.error.ToString() == "998")
                            {
                                bool bRet = login();
                                if (!bRet)
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("Bet failed(Need relogin)"));
                                    return PROCESS_RESULT.NO_LOGIN;
                                }
                                LogMng.Instance.onWriteStatus(string.Format("Retry after relogin"));
                            }
                            else
                            {
                                LogMng.Instance.onWriteStatus(string.Format("Place bet failed. {0} ", jsonBetResp.error.ToString()));

                                return PROCESS_RESULT.ERROR;
                            }


                        }
                        catch (Exception ex)
                        {
                            LogMng.Instance.onWriteStatus($"getLiveEventJsonString: {ex.Message}");
                        }
                    }
                    else
                    {

                        CDPController.Instance.strRequestUrl = "https://" + domain + "/scommesse/" + info.siteUrl;
                        string tid = Utils.Between(info.siteUrl, "tid=", "&");
                        string eventid = Utils.Between(info.siteUrl, "eid=", "&");
                        string createUrl = "https://" + domain + "/api/sport/pregame/getDetailsEvent/" + tid + "/" + eventid + "/0";
                        string responseData = "";                        
                        Thread.Sleep(4000);

                        JObject jsonObject = new JObject
                        {
                            ["headers"] = new JObject
                            {
                                ["accept"] = "application/json, text/plain, */*",
                                ["accept-language"] = "en-US,en;q=0.9",
                                ["content-type"] = "application/json",
                                ["priority"] = "u=1, i",
                                ["sec-ch-ua"] = "\"Google Chrome\";v=\"131\", \"Chromium\";v=\"131\", \"Not_A Brand\";v=\"24\"",
                                ["sec-ch-ua-mobile"] = "?0",
                                ["sec-ch-ua-platform"] = "\"Windows\"",
                                ["sec-fetch-dest"] = "empty",
                                ["sec-fetch-mode"] = "cors",
                                ["sec-fetch-site"] = "same-origin",
                                ["x-acceptconsent"] = "true",                                
                                ["x-brand"] = "1",
                                ["x-idcanale"] = "1"
                            },
                            ["referrer"] = "https://" + domain + info.siteUrl,
                            ["referrerPolicy"] = "strict-origin-when-cross-origin",
                            ["body"] = null,
                            ["method"] = "GET",
                            ["mode"] = "cors",
                            ["credentials"] = "include"
                        };
                        string functionString2 = $"var link = ''; fetch(\"{createUrl}\", {jsonObject}).then(res=>res.json()).then(json=>{{link = json}});";
                        CDPController.Instance.ExecuteScript(functionString2);
                        Thread.Sleep(5000);
                        int count = 0;
                        while (count < 20)
                        {
                            responseData = CDPController.Instance.ExecuteScript("JSON.stringify(link)", true, true);
                            if (!string.IsNullOrEmpty(responseData))
                                break;
                            Thread.Sleep(1000);
                            count++;
                        }

                        if (string.IsNullOrEmpty(responseData))
                        {
                            LogMng.Instance.onWriteStatus("GetAllLivesMatches Error");
                            return PROCESS_RESULT.ERROR;
                        }

#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus("event Data Json--");
                        LogMng.Instance.onWriteStatus(responseData);
#endif


                        JObject origObject1 = JObject.Parse(responseData);

                        bool bFound = false;
                        foreach (JObject origObject2 in origObject1["leo"])
                        {
                            foreach (dynamic origObject3 in origObject2["mmkW"])
                            {
                                foreach (var origObject4 in origObject3.Value["spd"])
                                {
                                    foreach (var origObject5 in origObject4.Value["asl"])
                                    {
                                        if (origObject5["oi"].ToString() == info.direct_link)
                                        {
                                            aamsId = origObject2["mi"].ToString();
                                            catId = origObject2["ci"].ToString();
                                            disId = origObject2["si"].ToString();
                                            evnDate = origObject2["ed"].ToString();                                            
                                            evtId = origObject2["ei"].ToString();
                                            evtName = origObject2["en"].ToString();                                            
                                            idSlt = origObject3.Value["sslI"].ToString();
                                            markId = origObject5["mi"].ToString();                                            
                                            markName = origObject3.Value["mn"].ToString();
                                            markTypId = origObject5["mti"].ToString();
                                            oddsId = origObject5["oi"].ToString();
                                            oddsValue = origObject5["ov"].ToString().Replace(",", ".");
                                            onLineCode = origObject2["oc"].ToString();
                                            selId = origObject5["si"].ToString();
                                            selName = origObject5["sn"].ToString();
                                            tId = origObject2["ti"].ToString();
                                            tName = origObject2["td"].ToString();
                                            sportName = origObject2["sn"].ToString();
                                            categoryDescription = origObject2["cd"].ToString();
                                            vrt = "false";
                                            NewOdd = Utils.ParseToDouble(oddsValue);
                                            curOdds = NewOdd;
                                            LogMng.Instance.onWriteStatus($"SelNewOdd : {NewOdd}");

                                            if (CheckOddDropCancelBet(NewOdd, info))
                                            {
                                                LogMng.Instance.onWriteStatus(string.Format("Odd is changed from {0} To {1}", info.odds, NewOdd.ToString("N2")));
                                                return PROCESS_RESULT.ERROR;
                                            }
                                            
                                            
                                            selIDs = selId;
                                            LogMng.Instance.onWriteStatus($"SelId: {selIDs} ready for bet");
                                            bFound = true;
                                            break;
                                        }
                                    }
                                    if (bFound)
                                        break;
                                }
                                if (bFound)
                                    break;
                            }
                            if (bFound)
                                break;
                        }
                        string strinsert_Result = string.Empty;
                        int retry = 5;
                        while (--retry > 0)
                        {

                            string responseData_insertBet = "";

                            string bodyArray = $"{{\"TagType\":12,\"Username\":\"{Setting.Instance.username}\",\"AuthToken\":\"{authToken}\",\"UserId\":{Idau},\"IdUtente\":{Idau},\"IdUser\":{Idau},\"Payload\":{{\"ReservationMaker\":3,\"allowOddChanges\":true,\"allowStakeReduction\":true,\"creationTime\":{Utils.getTick()},\"events\":[{{\"categoryDescription\":\"{categoryDescription}\",\"selId\":{selId},\"evtName\":\"{evtName}\",\"evnDate\":\"{evnDate}\",\"tName\":\"{tName}\",\"markName\":\"{markName}\",\"markId\":{markId},\"selName\":\"{selName}\",\"oddsValue\":{oddsValue},\"markMultipla\":0,\"isLive\":false,\"oddsId\":{oddsId},\"evtId\":{evtId},\"disId\":{disId},\"catId\":{catId},\"tId\":{tId},\"aamsId\":\"{aamsId}\",\"onLineCode\":{onLineCode},\"firstTeam\":\"{info.homeTeam}\",\"secondTeam\":\"{info.awayTeam}\",\"idSelectionType\":0,\"markTypId\":0,\"aamsGamePlay\":0,\"sportName\":\"{sportName}\",\"vrt\":false,\"spreadId\":\"0\",\"stake\":{Setting.Instance.stakeSports},\"isRigiocoVillaggio\":false}}],\"fixed\":[],\"groupCombs\":{{\"combsInfo\":[{{\"combNum\":1,\"combType\":1,\"stake\":{Setting.Instance.stakeSports}}}],\"sumCombsXType\":1}},\"totalStake\":{Setting.Instance.stakeSports},\"virtual\":false,\"bonusWager\":false}},\"idCanale\":1}}";

                            string insertBetUrl = "https://" + domain + "/api/sport/book/insertBet"; 
                            var insertBetObj = new JObject
                            {
                                ["headers"] = new JObject
                                {
                                    ["accept"] = "application/json, text/plain, */*",
                                    ["accept-language"] = "en-US,en;q=0.9",
                                    ["content-type"] = "application/json",
                                    ["priority"] = "u=1, i",
                                    ["sec-ch-ua"] = "\"Google Chrome\";v=\"131\", \"Chromium\";v=\"131\", \"Not_A Brand\";v=\"24\"",
                                    ["sec-ch-ua-mobile"] = "?0",
                                    ["sec-ch-ua-platform"] = "\"Windows\"",
                                    ["sec-fetch-dest"] = "empty",
                                    ["sec-fetch-mode"] = "cors",
                                    ["sec-fetch-site"] = "same-origin",
                                    ["x-acceptconsent"] = "true",
                                    ["x-auth-iduser"] = Idau,
                                    ["x-auth-token"] = authToken,
                                    ["x-auth-username"] = Setting.Instance.username,
                                    ["x-brand"] = "1",
                                    ["x-idcanale"] = "1"
                                },
                                ["referrer"] = "https://" + domain + "/scommesse/" + info.siteUrl,
                                ["referrer"] = string.Format("https://{0}/scommesse/{1}", domain, info.siteUrl),
                                ["referrerPolicy"] = "strict-origin-when-cross-origin",
                                ["body"] = bodyArray,
                                ["method"] = "POST",
                                ["mode"] = "cors",
                                ["credentials"] = "include"
                            };
                            string functionstring_InsertBet = $"var link = ''; fetch(\"{insertBetUrl}\", {insertBetObj}).then(res=>res.json()).then(json=>{{link = json}});";
                            CDPController.Instance.ExecuteScript(functionstring_InsertBet);
                            Thread.Sleep(4000);
                            int count1 = 0;
                            while (count1 < 20)
                            {
                                responseData_insertBet = CDPController.Instance.ExecuteScript("JSON.stringify(link)", true, true);
                                if (!string.IsNullOrEmpty(responseData_insertBet))
                                    break;
                                Thread.Sleep(1000);
                                count1++;
                            }

                            if (string.IsNullOrEmpty(responseData_insertBet))
                            {
                                LogMng.Instance.onWriteStatus("insertbet no response");
                                return PROCESS_RESULT.ERROR;
                            }

                            strinsert_Result = responseData_insertBet;
                            break;

                        }
                        dynamic jsonBetResp = JsonConvert.DeserializeObject<dynamic>(strinsert_Result);

                        if (jsonBetResp.success.ToString() == "True")
                        {
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus(string.Format("Placed bet in Step 1"));
#endif
                            string confirmBetUrl = "https://" + domain + "/api/sport/book/pendingBet";
                            string strConfirmBetResp = string.Empty;
                            retry = 3;
                            while (--retry > 0)
                            {
                                try
                                {
                                    JObject requestObjectPending = new JObject
                                    {
                                        ["headers"] = new JObject
                                        {
                                            ["accept"] = "application/json, text/plain, */*",
                                            ["accept-language"] = "en-US,en;q=0.9",
                                            ["content-type"] = "application/json",
                                            ["priority"] = "u=1, i",
                                            ["sec-ch-ua"] = "\"Google Chrome\";v=\"131\", \"Chromium\";v=\"131\", \"Not_A Brand\";v=\"24\"",
                                            ["sec-ch-ua-mobile"] = "?0",
                                            ["sec-ch-ua-platform"] = "\"Windows\"",
                                            ["sec-fetch-dest"] = "empty",
                                            ["sec-fetch-mode"] = "cors",
                                            ["sec-fetch-site"] = "same-origin",
                                            ["x-acceptconsent"] = "true",
                                            ["x-auth-iduser"] = Idau,
                                            ["x-auth-token"] = $"{authToken}",
                                            ["x-auth-username"] = "silvilore",
                                            ["x-brand"] = "1",
                                            ["x-idcanale"] = "1"
                                        },
                                        ["referrer"] = "https://" + domain + "/scommesse/" + info.siteUrl,
                                        ["referrer"] = string.Format("https://{0}/scommesse/{1}", domain, info.siteUrl),
                                        ["referrerPolicy"] = "strict-origin-when-cross-origin",
                                        ["body"] = $"{{\"AuthToken\":\"{authToken}\",\"UserId\":{Idau},\"IdUtente\":{Idau},\"IdUser\":{Idau},\"idCanale\":1,\"CouponCode\":\"{jsonBetResp.data.couponCode}\"}}",
                                        ["method"] = "POST",
                                        ["mode"] = "cors",
                                        ["credentials"] = "include"
                                    };
                                    string responsedata = "";
                                    string functionstring_pending = $"var link = ''; fetch(\"{confirmBetUrl}\", {requestObjectPending}).then(res=>res.json()).then(json=>{{link = json}});";
                                    CDPController.Instance.ExecuteScript(functionstring_pending);
                                    Thread.Sleep(4000);
                                    int count1 = 0;
                                    while (count1 < 20)
                                    {
                                        responsedata = CDPController.Instance.ExecuteScript("JSON.stringify(link)", true, true);
                                        if (!string.IsNullOrEmpty(responsedata))
                                            break;
                                        Thread.Sleep(1000);
                                        count1++;
                                    }

                                    if (string.IsNullOrEmpty(responsedata))
                                    {
                                        continue;
                                    }

                                    strConfirmBetResp = responsedata;
#if (TROUBLESHOT)
                                    LogMng.Instance.onWriteStatus("pendingBet Res:" + strConfirmBetResp);
#endif

                                    break;
                                }
                                catch (Exception ex)
                                {
                                    LogMng.Instance.onWriteStatus("pendingBet exception:" + ex);
                                }
                            }
                            dynamic jsonConfirmBetResp = JsonConvert.DeserializeObject<dynamic>(strConfirmBetResp);
                            LogMng.Instance.onWriteStatus($"Placing failed Reason: {jsonConfirmBetResp.statusCode}");
                            if (jsonConfirmBetResp.statusCode.ToString() == "Placed" || jsonConfirmBetResp.statusCode.ToString() == "P")
                            {
                                return PROCESS_RESULT.PLACE_SUCCESS;
                            }

                            return PROCESS_RESULT.ERROR;
                        }
                        else if (jsonBetResp.error.error.ToString() == "998")
                        {
                            bool bRet = login();
                            if (!bRet)
                            {
                                LogMng.Instance.onWriteStatus(string.Format("Bet failed(Need relogin)"));
                                return PROCESS_RESULT.NO_LOGIN;
                            }
                            LogMng.Instance.onWriteStatus(string.Format("Retry after relogin"));
                        }
                        else if (jsonBetResp.error.error.ToString() == "30")
                        {
                            LogMng.Instance.onWriteStatus(string.Format("Bet failed(+++++YOUR STAKE IS LOW+++++)"));
                            return PROCESS_RESULT.ERROR;
                        }
                        else
                        {
                            LogMng.Instance.onWriteStatus(string.Format("Place bet failed. {0} ", jsonBetResp.error.ToString()));

                            return PROCESS_RESULT.ERROR;
                        }
                    }
                    
                        
                    
                }
                            
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"Place bet exception {ex.StackTrace} {ex.Message}");
            }

            return PROCESS_RESULT.ERROR;
        }      

        public string Hashpassword(string password)
        {
                       

            using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    StringBuilder builder = new StringBuilder();
                    foreach (byte b in bytes)
                    {
                        builder.Append(b.ToString("x2")); // Converts to a hex string
                    }
                    return builder.ToString();
                }
            
        }

        public string GetAuthAndIdUtente(string hashedPassword, string userName)
        {
            string strLogin_Result = string.Empty;
            string auth_token = string.Empty;
            string IdUtenteInfo = string.Empty;
            string strAuthandIdUT = string.Empty;
            string balance_Current = string.Empty;
            string logInUrl = "https://" + domain + "/api/pam/account/login";
            string bodyArray = $"{{\"username\":\"{userName}\",\"password\":\"{hashedPassword}\",\"claims\":\"/tj/IXi8Q2xAY5ptpSKp8Aeepvo=\",\"blackboxIovation\":\"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;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\",\"idCanale\":1,\"ipUtente\":null,\"vertical\":1,\"CodiceTransazione\":\"0ad1590b-c81b-480e-924b-41a977570020\"}}";
            JObject getIoginJOB = new JObject
            {
                ["headers"] = new JObject
                {
                    ["accept"] = "application/json, text/plain, */*",
                    ["accept-language"] = "en-US,en;q=0.9",
                    ["content-type"] = "application/json",
                    ["priority"] = "u=1, i",
                    ["sec-ch-ua"] = "\"Google Chrome\";v=\"131\", \"Chromium\";v=\"131\", \"Not_A Brand\";v=\"24\"",
                    ["sec-ch-ua-mobile"] = "?0",
                    ["sec-ch-ua-platform"] = "\"Windows\"",
                    ["sec-fetch-dest"] = "empty",
                    ["sec-fetch-mode"] = "cors",
                    ["sec-fetch-site"] = "same-origin",
                    ["x-acceptconsent"] = "true",
                    ["x-brand"] = "1",
                    ["x-idcanale"] = "1"
                },
                ["referrer"] = "https://" + domain + "/scommesse/sport/",
                ["referrerPolicy"] = "strict-origin-when-cross-origin",
                ["body"] = bodyArray,
                ["method"] = "POST",
                ["mode"] = "cors",
                ["credentials"] = "include"
            };

            string responseData = "";
            string functionString = $"var link = ''; fetch(\"{logInUrl}\", {getIoginJOB}).then(res=>res.json()).then(json=>{{link = json}});";
            CDPController.Instance.ExecuteScript(functionString);
            Thread.Sleep(5000);
            int count = 0;
            while (count < 20)
            {
                responseData = CDPController.Instance.ExecuteScript("JSON.stringify(link)", true, true);
                if (!string.IsNullOrEmpty(responseData))
                    break;
                Thread.Sleep(1000);
                count++;
            }

            if (string.IsNullOrEmpty(responseData))
            {
                LogMng.Instance.onWriteStatus("getAuthKEY request error");              
            }

            strLogin_Result = responseData;
            dynamic jsonLotinResp = JsonConvert.DeserializeObject<dynamic>(responseData);
            auth_token = jsonLotinResp.AuthToken.ToString();
            IdUtenteInfo = (string)jsonLotinResp.IdUtente;
            balance_Current = (string)jsonLotinResp.Saldo;
            strAuthandIdUT = auth_token + "/" + IdUtenteInfo + "/" + balance_Current;
            return strAuthandIdUT;

        }
             


        bool CheckOddDropCancelBet(double newOdd, BetburgerInfo info)
        {
            if (info.kind == PickKind.Type_1)
            {
                //LogMng.Instance.onWriteStatus($"Odd checking odd in pick: {info.odds} new odd: {newOdd}");
                if (newOdd > Setting.Instance.maxOddsSports || newOdd < Setting.Instance.minOddsSports)
                {
                    LogMng.Instance.onWriteStatus($"Ignore bet because of odd is out of range, new odd: {newOdd}");
                    return true;
                }
            }

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
                        LogMng.Instance.onWriteStatus($"Ignore bet because of odd is raised larger than {Setting.Instance.dAllowOddRisePercent}%: {info.odds} -> {newOdd}");
                        return true;
                    }
                }
            }
            return false;
        }


        public double getBalance()
        {
            Thread.Sleep(10000);
            int nRetry = 2;            
            double balance = 0;
            string userName = Setting.Instance.username;
            string password = Setting.Instance.password;
            string hashedPassword = Hashpassword(password);
            string authTokenAndIdau = GetAuthAndIdUtente(hashedPassword, userName);
            parts = authTokenAndIdau.Split('/');
            authToken = parts[0];
            Idau = parts[1];
            get_Balance = parts[2];
            while (nRetry >= 0)
            {
                nRetry--;
                try
                {                    
                    balance = Convert.ToDouble(get_Balance) / 100;                    
                }
                catch (Exception e)
                {
                    LogMng.Instance.onWriteStatus("You without Login.");
                }
                break;
            }

            LogMng.Instance.onWriteStatus(string.Format("GetBalance: {0}", balance));
            return balance;
        
        }
        

        public void Close()
        {

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

        public bool Pulse()
        {
            return false;
        }

        public PROCESS_RESULT PlaceBet(List<BetburgerInfo> infos, out List<PROCESS_RESULT> result)
        {
            throw new NotImplementedException();
        }
    }
#endif
}

