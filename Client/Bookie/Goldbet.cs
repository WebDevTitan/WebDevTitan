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
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project.Helphers;
using Protocol;
using WebSocketSharp;
using static MasterDevs.ChromeDevTools.Protocol.Chrome.ProtocolName;

namespace Project.Bookie
{
#if (GOLDBET)
    class GoldbetCtrl : IBookieController
    {
        string domain = "www.goldbet.it";

        public HttpClient m_client = null;
        
        Object lockerObj = new object();
        //private static ProxyServer _proxyServer = null;
        private string strIdUtente = "";   
        public GoldbetCtrl()
        {
            
            m_client = initHttpClient();
            Global.SetMonitorVisible(true);
#if (LOTTOMATICA)
            domain = "www.lottomatica.it";
#endif
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
            //SetTraceHeaders();
            //try
            //{
            //    lock (lockerObj)
            //    {
            //        HttpResponseMessage tokenResponseMessage = m_client.GetAsync("https://" + domain + "/getOverviewLive/?idDiscipline=0&idTab=0&isFromUser=false").Result;
            //        tokenResponseMessage.EnsureSuccessStatusCode();
            //    }
            //}
            //catch
            //{ }
            //int nHostIDCount = 0;
            //foreach (System.Net.Cookie cookie in Global.cookieContainer.GetCookies(new Uri("https://" + domain)))
            //{
            //    if (cookie.Name == "HOSTID")
            //    {
            //        nHostIDCount++;
            //    }
            //}

            //if (nHostIDCount > 1)
            //{
            //    LogMng.Instance.onWriteStatus($"HostID Count: {nHostIDCount} need to relogin");
            //    return false;
            //}
            //return true;
        }
      

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }
        Random rand = new Random();
        public void MouseMove()
        {
            while (true)
            {
                SetForegroundWindow(Global.ViewerHwnd);


                Thread.Sleep(100);


                int x = rand.Next(0, (int)SystemParameters.WorkArea.Width);
                int y = rand.Next(0, (int)SystemParameters.WorkArea.Height);

                //SetCursorPos(x, y);

                //int lParam = y << 16 | x;
                //PostMessage(Global.ViewerHwnd, 0x0200, (IntPtr)0, (IntPtr)lParam);  //WM_MOUSEMOVE
                //PostMessage(Global.ViewerHwnd, 0x0084, (IntPtr)0, (IntPtr)lParam);  //WM_NCHITTEST
                //PostMessage(Global.ViewerHwnd, 0x0020, (IntPtr)Global.ViewerHwnd, (IntPtr)0x02000001);  //WM_SETCURSOR
            }
        }

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

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

        public static void ClickOnPoint(IntPtr wndHandle, Point clientPoint)
        {
            var oldPos = Cursor.Position;

            /// get screen coordinates
            ClientToScreen(wndHandle, ref clientPoint);

            /// set cursor on coords, and press mouse
            Cursor.Position = new System.Drawing.Point((int)clientPoint.X, (int)clientPoint.Y);

            var inputMouseDown = new INPUT();
            inputMouseDown.Type = 0; /// input type mouse
            inputMouseDown.Data.Mouse.Flags = 0x0002; /// left button down

            var inputMouseUp = new INPUT();
            inputMouseUp.Type = 0; /// input type mouse
            inputMouseUp.Data.Mouse.Flags = 0x0004; /// left button up

            var inputs = new INPUT[] { inputMouseDown, inputMouseUp };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

            /// return mouse 
            //Cursor.Position = oldPos;
        }
        public bool login()
        {
            Global.RemoveCookies();
            Global.SetMonitorVisible(true);
            Global.OpenUrl($"https://{domain}/scommesse/sport/");

            // Wait for the page to fully render by checking for a specific element
            bool pageLoaded = false;
            int retryCount = 0;
            while (!pageLoaded && retryCount < 30)
            {
                Thread.Sleep(1000); // Wait for 1 second
                retryCount++;
                try
                {
                    // Check if a specific element is present on the page
                    string script = "return document.querySelector('.anonymous--login--button') !== null;";
                    dynamic result = Global.RunScriptCode(script);
                    pageLoaded = result == true;
                }
                catch
                {
                    
                }
            }

            if (!pageLoaded)
            {
                LogMng.Instance.onWriteStatus("Page load timeout.");
                return false;
            }

            bool bLogin = false;
            try
            {
                Global.RunScriptCode($"document.getElementById('onetrust-accept-btn-handler').click()");
                Global.RunScriptCode("document.querySelector('.anonymous--login--button').click();");

                Thread.Sleep(1000);
                Global.RunScriptCode($"document.getElementById('login_username').value = '{Setting.Instance.username}';");
                Global.RunScriptCode($"document.getElementById('login_password').value = '{Setting.Instance.password}';");

                Thread.Sleep(500);

                Global.strPlaceBetResult = "";
                Global.waitResponseEvent.Reset();

                Global.RunScriptCode("document.querySelector('.login__panel--login__form--button--login').click();");
                Thread.Sleep(3000);

                //Global.RunScriptCode($"document.getElementById('mat-input-0').value = '{Setting.Instance.OTP}';");

                if (!Global.waitResponseEvent.Wait(10000))
                {
                    LogMng.Instance.onWriteStatus("Login response timeout.");
                    throw new Exception("Login response timeout.");
                }

                dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(Global.strPlaceBetResult);

                if (jsonContent.authenticated.ToString().ToLower() != "true")
                {
                    LogMng.Instance.onWriteStatus("Login Error: " + jsonContent.authenticated.ToString());
                    throw new Exception("Authentication failed.");
                }

                Global.strAddBetResult = "";

                Global.OpenUrl($"https://{domain}/scommesse/live");
                Thread.Sleep(500);

                bLogin = true;
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus($"Exception: {e.Message}");
                LogMng.Instance.onWriteStatus($"Stack Trace: {e.StackTrace}");
            }

            LogMng.Instance.onWriteStatus($"Login Result: {bLogin}");
            return bLogin;
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

        private string previousLocalEventResult = "";

        string ChangeOddFromEventJsonString(string eventsJson, string oddsId, string oddsValue)
        {
            string result = eventsJson;
            string origOddsId = Utils.Between("\"oddsId\":\"", "\"");
            string origoddsValue = Utils.Between("\"oddsValue\":\"", "\"");

            if (string.IsNullOrEmpty(origOddsId) || origOddsId.Length > 20)
                return result;
            if (string.IsNullOrEmpty(origoddsValue) || origoddsValue.Length > 20)
                return result;

            result = result.Replace("\"oddsId\":\"" + origOddsId + "\"", "\"oddsId\":\"" + oddsId + "\"");
            result = result.Replace("\"oddsId\":\"" + origoddsValue + "\"", "\"oddsId\":\"" + oddsValue + "\"");
            return result;
        }

        PROCESS_RESULT GetPrematchEventJsonString(BetburgerInfo info, out string selIDs, out string eventsJson, out double curOdds)
        {
            selIDs = string.Empty;
            eventsJson = string.Empty;
            curOdds = 0;

            Global.strRequestUrl = "https://" + domain + "/scommesse/" + info.siteUrl;
            
            Global.strPlaceBetResult = "";
            Global.waitResponseEvent.Reset();

            Global.OpenUrl(Global.strRequestUrl);

            if (!Global.waitResponseEvent.Wait(3000))
            {
                if (string.IsNullOrEmpty(previousLocalEventResult))
                {
                    LogMng.Instance.onWriteStatus("GetAllLivesMatches Error");
                    return PROCESS_RESULT.ERROR;
                }
            }
                        
            string strEventContent = Utils.Between(Global.strPlaceBetResult, "var CALL_DETAIL_OBJ = '", "'");
            

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus("event Data Json--");
            LogMng.Instance.onWriteStatus(strEventContent);
#endif


            OpenBet_Goldbet openbet = Utils.ConvertBetburgerPick2OpenBet_Goldbet(info);

            JObject origObject1 = JObject.Parse(strEventContent);

            bool bFound = false;
            foreach (JObject origObject2 in origObject1["leo"])
            {
                foreach (dynamic origObject3 in origObject2["mmkW"])
                {
                    foreach (var origObject4 in origObject3.Value["spd"])
                    {
                        foreach (var origObject5 in origObject4.Value["asl"])
                        {
                            if (origObject5["oi"].ToString() == openbet.market_oi)
                            {
                                string aamsId = origObject2["mi"].ToString();
                                string catId = origObject2["ci"].ToString();
                                string disId = origObject2["si"].ToString();
                                string evnDate = origObject2["ed"].ToString();
                                string evnDateTs = origObject2["edt"].ToString();
                                string evtId = origObject2["ei"].ToString();
                                string evtName = origObject2["en"].ToString();
                                string hdrType = origObject3.Value["ht"].ToString();
                                string idSlt = origObject3.Value["sslI"].ToString();
                                string markId = origObject5["mi"].ToString();
                                string markMultipla = origObject5["oc"].ToString();
                                string markName = origObject3.Value["mn"].ToString();
                                string markTypId = origObject5["mti"].ToString();
                                string oddsId = origObject5["oi"].ToString();
                                string oddsValue = origObject5["ov"].ToString().Replace(",", ".");
                                string onLineCode = origObject2["oc"].ToString();
                                string selId = origObject5["si"].ToString();
                                string selName = origObject5["sn"].ToString();
                                string tId = origObject2["ti"].ToString();
                                string tName = origObject2["td"].ToString();
                                string vrt = "false";

                                double NewOdd = Utils.ParseToDouble(oddsValue);
                                curOdds = NewOdd;
                                LogMng.Instance.onWriteStatus($"SelNewOdd : {NewOdd}");

                                if (CheckOddDropCancelBet(NewOdd, info))
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("Odd is changed from {0} To {1}", info.odds, NewOdd.ToString("N2")));
                                    return PROCESS_RESULT.MOVED;
                                }

                                selIDs = selId;
                                LogMng.Instance.onWriteStatus($"SelId: {selIDs} ready for bet");

                                eventsJson = $"{{\"isLive\":false,\"onLineCode\":\"{onLineCode}\",\"selName\":\"{selName}\",\"selId\":\"{selId}\",\"oddsId\":\"{oddsId}\",\"oddsValue\":\"{oddsValue}\",\"markName\":\"{markName}\",\"markId\":\"{markId}\",\"markTypId\":\"{markTypId}\",\"idSlt\":\"{idSlt}\",\"markMultipla\":\"{markMultipla}\",\"hdrType\":\"{hdrType}\",\"catId\":\"{catId}\",\"disId\":\"{disId}\",\"tId\":\"{tId}\",\"tName\":\"{tName}\",\"evnDateTs\":\"{evnDateTs}\",\"evtId\":\"{evtId}\",\"aamsId\":\"{aamsId}\",\"evtName\":\"{evtName}\",\"evnDate\":\"{evnDate}\",\"vrt\":{vrt}}}";

                                bFound = true;
                                return PROCESS_RESULT.SUCCESS;
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


            return PROCESS_RESULT.ERROR;
        }

        PROCESS_RESULT GetLiveEventJsonString(BetburgerInfo info, out string selIDs, out string eventsJson, out double curOdds)
        {
            selIDs = string.Empty;
            eventsJson = string.Empty;
            curOdds = 0;


            Global.strRequestUrl = "https://" + domain + "/scommesse/getOverviewLive/?idDiscipline=0&idTab=0&menu=menu&isFromUser=false";
            string baseURL = "https://" + domain;
            string functionString = $"window.fetch('{Global.strRequestUrl}', {{ method: 'GET', headers: {{ 'accept': '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json', }}, credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', referrer: '{baseURL}' }}).then(response => response.json());";


            Global.strPlaceBetResult = "";
            Global.waitResponseEvent.Reset();

            Global.RunScriptCode(functionString);

            if (!Global.waitResponseEvent.Wait(3000))
            {
                if (string.IsNullOrEmpty(previousLocalEventResult))
                {
                    LogMng.Instance.onWriteStatus("GetAllLivesMatches Error");
                    return PROCESS_RESULT.ERROR;
                }
            }

            var strEventContent = Global.strPlaceBetResult;
            JObject origObject = JObject.Parse(strEventContent);
            if (origObject["leo"] == null)
            {
                origObject = JObject.Parse(previousLocalEventResult);
                if (origObject["leo"] == null)
                {
                    LogMng.Instance.onWriteStatus("GetAllLivesMatches Error(1)");
                    return PROCESS_RESULT.ERROR;
                }
            }
            else
            {
                previousLocalEventResult = strEventContent;
            }

            string aamsId = "";
            string catId = "";
            string disId = "";
            string evnDate = "";
            string evnDateTs = "";
            string evtId = "";
            string evtName = "";
            string hdrType = "";
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

            //LogMng.Instance.onWriteStatus($"Placed bet prepare eventTitle:{info.eventTitle} league:{info.league} odd:{info.odds} stake:{info.stake} mn:{openbet.market_mn} si:{openbet.market_si} mi:{openbet.market_mi}");

            //SetTraceHeaders();
            
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus("all live sports--");
                    LogMng.Instance.onWriteStatus(strEventContent);
#endif
            
           
            OpenBet_Goldbet openbet = Utils.ConvertBetburgerPick2OpenBet_Goldbet(info);

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

            Global.strRequestUrl = EventdetailUrl;
            baseURL = "https://" + domain;
            functionString = $"window.fetch('{Global.strRequestUrl}', {{ method: 'GET', headers: {{ 'accept': '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json', }}, credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', referrer: '{baseURL}' }}).then(response => response.json());";

            Global.strPlaceBetResult = "";
            Global.waitResponseEvent.Reset();

            Global.RunScriptCode(functionString);

            if (!Global.waitResponseEvent.Wait(3000))
            {
                LogMng.Instance.onWriteStatus("GetAllLivesMatches Error");
                return PROCESS_RESULT.ERROR;                
            }

            string strContent = Global.strPlaceBetResult;
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus("target match--");
                    LogMng.Instance.onWriteStatus(strContent);
#endif
            JObject origObject4 = JObject.Parse(strContent);
            foreach (var objEvent in origObject4["mktWbD"])
            {
                JToken matchObj = objEvent.ToObject<JProperty>().Value;

                foreach (var marketEvent in matchObj["ms"])
                {
                    JToken marketObj = marketEvent.ToObject<JProperty>().Value;
                    foreach (var piEvent in marketObj["asl"])
                    {
                        if (piEvent["si"].ToString() == openbet.market_si && piEvent["mi"].ToString() == openbet.market_mi)
                        {
                            hdrType = matchObj["ht"].ToString();
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
                return PROCESS_RESULT.SUSPENDED;                
            }

            double NewOdd = Utils.ParseToDouble(oddsValue);
            curOdds = NewOdd;
            LogMng.Instance.onWriteStatus($"SelNewOdd : {NewOdd}");

            if (CheckOddDropCancelBet(NewOdd, info))
            {                
                LogMng.Instance.onWriteStatus(string.Format("Odd is changed from {0} To {1}", info.odds, NewOdd.ToString("N2")));
                return PROCESS_RESULT.MOVED;
            }

            selIDs = selId;
            LogMng.Instance.onWriteStatus($"SelId: {selIDs} ready for bet");

            eventsJson = $"{{\"isLive\":\"true\",\"selName\":\"{selName}\",\"selId\":\"{selId}\",\"oddsId\":\"{oddsId}\",\"oddsValue\":\"{oddsValue}\",\"markName\":\"{markName}({sNL})\",\"markId\":\"{markId}\",\"markTypId\":\"{markTypId}\",\"hdrType\":\"{hdrType}\",\"prvIdEvt\":\"{prvIdEvt}\",\"aamsId\":\"{aamsId}\",\"evtName\":\"{evtName}\",\"evnDate\":\"{evnDate}\",\"evnDateTs\":\"{evnDateTs}\",\"evtId\":\"{evtId}\",\"catId\":\"{catId}\",\"disId\":\"{disId}\",\"tId\":\"{tId}\",\"tName\":\"{tName}\",\"onLineCode\":\"{onLineCode}\",\"vrt\":{vrt}}}";
                
            return PROCESS_RESULT.SUCCESS;
        }

        public PROCESS_RESULT PlaceBet(List<BetburgerInfo> infos, out List<PROCESS_RESULT> itrResult)
        {
            if (infos.Count < 1)
            {
                itrResult = new List<PROCESS_RESULT>();
                return PROCESS_RESULT.ERROR;
            }

            string[] selIDs = new string[infos.Count];
            double[] selOdds = new double[infos.Count];
            string[] eventsJsonItr = new string[infos.Count];

            itrResult = new List<PROCESS_RESULT>();
            

            try
            {
                lock (lockerObj)
                {
                    
                    double MaxWin = infos[0].stake;
                    int betCount = 0;

                    for (int i = 0; i < infos.Count; i++)
                    {
                        OpenBet_Goldbet openbet = Utils.ConvertBetburgerPick2OpenBet_Goldbet(infos[i]);
                        if (openbet.isLive)
                            itrResult.Add(GetLiveEventJsonString(infos[i], out selIDs[i], out eventsJsonItr[i], out selOdds[i]));
                        else
                            itrResult.Add(GetPrematchEventJsonString(infos[i], out selIDs[i], out eventsJsonItr[i], out selOdds[i]));
                    }
                    
                    int retryCount = 2;

                    while (--retryCount >= 0)
                    {

                        string eventsJson = "";
                        for (int k = 0; k < infos.Count; k++)
                        {
                            if (itrResult[k] == PROCESS_RESULT.SUCCESS)
                            {
                                if (!string.IsNullOrEmpty(eventsJson))
                                    eventsJson += ",";
                                eventsJson += eventsJsonItr[k];
                                MaxWin *= selOdds[k];
                                betCount++;
                            }
                        }

                        int combType = 1;
                        if (betCount > 1)
                            combType = 2;

                        if (betCount <= 0)
                        {
                            LogMng.Instance.onWriteStatus("Nothing to Place bet in info list");
                            return PROCESS_RESULT.ERROR;
                        }


                        string maxPG = MaxWin.ToString("N2").Replace(",", ".");

                        string ReqJson = $"{{\"groupCombs\":{{\"combsInfo\":[{{\"combType\":\"{combType}\",\"combNum\":\"1\",\"stake\":{infos[0].stake}}}],\"sumCombsXType\":1}},\"totalStake\":{infos[0].stake},\"fixed\":[],\"events\":[{eventsJson}],\"creationTime\":{Utils.getTick()},\"virtual\":false,\"allowStakeReduction\":false,\"allowOddChanges\":false,\"bonusWager\":false,\"maxPag\":\"{maxPG}\"}}";
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"insertBet Req : {ReqJson}");
#endif
                        try
                        {
                            string betUrl = "https://" + domain + "/scommesse/insertBet";

                            int subRetryCount = 6;
                            string strBetResp = string.Empty;

                            while (--subRetryCount > 0)
                            {
                                try
                                {
                                    Global.strRequestUrl = betUrl;
                                    string functionString = $"window.fetch('{Global.strRequestUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{ReqJson}', method: 'POST' }}).then(response => response.json());";

                                    Global.strPlaceBetResult = "";
                                    Global.waitResponseEvent.Reset();

                                    Global.RunScriptCode(functionString);


                                    if (!Global.waitResponseEvent.Wait(20000) || string.IsNullOrEmpty(Global.strPlaceBetResult))
                                    {
                                        LogMng.Instance.onWriteStatus("insertBet No Response");
                                        continue;
                                    }

                                    strBetResp = Global.strPlaceBetResult;

#if (TROUBLESHOT)
                                    LogMng.Instance.onWriteStatus("insertBet Res:" + strBetResp);
#endif

                                    break;
                                }
                                catch (Exception ex)
                                {
                                    LogMng.Instance.onWriteStatus("insertBet exception:" + ex);
                                }
                            }
                            
                            dynamic jsonBetResp = JsonConvert.DeserializeObject<dynamic>(strBetResp);

                            if (jsonBetResp.success.ToString() == "True")
                            {
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus(string.Format("Placed bet in Step 1"));
#endif
                                string confirmBetUrl = "https://" + domain + "/scommesse/pendingBet/" + jsonBetResp.data.couponCode.ToString();
                                string strConfirmBetResp = string.Empty;
                                subRetryCount = 3;
                                while (--subRetryCount > 0)
                                {
                                    try
                                    {
                                        Global.strRequestUrl = confirmBetUrl;
                                        string functionString = $"window.fetch('{Global.strRequestUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '', method: 'POST' }}).then(response => response.json());";

                                        Global.strPlaceBetResult = "";
                                        Global.waitResponseEvent.Reset();

                                        Global.RunScriptCode(functionString);


                                        if (!Global.waitResponseEvent.Wait(20000) || string.IsNullOrEmpty(Global.strPlaceBetResult))
                                        {
                                            continue;
                                        }

                                        strConfirmBetResp = Global.strPlaceBetResult;

//#if (TROUBLESHOT)
                                        LogMng.Instance.onWriteStatus("pendingBet Res:" + strConfirmBetResp);
//#endif

                                        break;
                                    }
                                    catch (Exception ex)
                                    {
                                        LogMng.Instance.onWriteStatus("pendingBet exception:" + ex);
                                    }
                                }
                                dynamic jsonConfirmBetResp = JsonConvert.DeserializeObject<dynamic>(strConfirmBetResp);
                                LogMng.Instance.onWriteStatus($"Placing failed Reason: {jsonConfirmBetResp.statusDesc}");
                                if (jsonConfirmBetResp.statusDesc.ToString() == "Placed" || jsonConfirmBetResp.statusDesc.ToString() == "P")
                                {
                                    for (int i = 0; i < itrResult.Count; i++)
                                        itrResult[i] = PROCESS_RESULT.PLACE_SUCCESS;
                                    return PROCESS_RESULT.PLACE_SUCCESS;
                                }
                                //else if (jsonConfirmBetResp.statusDesc.ToString() == "Refused")
                                //{
                                //    for (int i = 0; i < itrResult.Count; i++)
                                //        itrResult[i] = PROCESS_RESULT.CRITICAL_SITUATION;
                                //    return PROCESS_RESULT.CRITICAL_SITUATION;
                                //}


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
                            else if (jsonBetResp.error.error.ToString() == "3")
                            {
                                LogMng.Instance.onWriteStatus(string.Format("Place bet failed code(3). {0} ", jsonBetResp.error.ToString()));

                                try
                                {
                                    foreach (var expired in jsonBetResp.error.expiredSelections)
                                    {                                        
                                        for (int k = 0; k < infos.Count; k++)
                                        {
                                            //LogMng.Instance.onWriteStatus($"checking 3 selections index: {k} SelID: {selIDs[k]} Value: {expired.selectionId.ToString()}");

                                            if (selIDs[k] == expired.selectionId.ToString())
                                            {
                                                itrResult[k] = PROCESS_RESULT.SUSPENDED;
                                                LogMng.Instance.onWriteStatus($"Market is suspended({selIDs[k]})");
                                                break;
                                            }
                                        }
                                    }
                                }
                                catch { }                                
                            }
                            else if (jsonBetResp.error.error.ToString() == "4")
                            {
                                LogMng.Instance.onWriteStatus(string.Format("Place bet failed code(4). {0} ", jsonBetResp.error.ToString()));

                                try
                                {
                                    foreach (var changed in jsonBetResp.error.changedSelections)
                                    {
                                        for (int k = 0; k < infos.Count; k++)
                                        {                                            
                                            //LogMng.Instance.onWriteStatus($"checking 4 selections index: {k} SelID: {selIDs[k]} Value: {changed.selectionId.ToString()}");

                                            if (selIDs[k] == changed.selectionId.ToString())
                                            {
                                                double NewOdd = Utils.ParseToDouble(changed.oddValue.ToString());
                                                LogMng.Instance.onWriteStatus($"Odd changed in placebet({selIDs[k]}): {infos[k]} -> {NewOdd}");
                                                selOdds[k] = NewOdd;
                                                eventsJsonItr[k] = ChangeOddFromEventJsonString(eventsJsonItr[k], changed.oddId.ToString(), changed.oddValue.ToString());

                                                if (CheckOddDropCancelBet(NewOdd, infos[k]))
                                                {
                                                    itrResult[k] = PROCESS_RESULT.MOVED;
                                                    LogMng.Instance.onWriteStatus($"Ignore this bet because of Odd is dropped a lot {selIDs[k]}");                                                    
                                                }                                                
                                                break;
                                            }
                                        }
                                    }
                                }
                                catch { }                                
                            }
                            else
                            {
                                LogMng.Instance.onWriteStatus(string.Format("Place bet failed. {0} ", jsonBetResp.error.ToString()));

                                return PROCESS_RESULT.ERROR;
                            }

                        }
                        catch (Exception ex)
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
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"Place bet exception {ex.StackTrace} {ex.Message}");
            }

            return PROCESS_RESULT.ERROR;
        }

        //Not using.
        public PROCESS_RESULT PlaceBet(ref BetburgerInfo info)
        {
            OpenBet_Goldbet openbet = Utils.ConvertBetburgerPick2OpenBet_Goldbet(info);

            if (openbet == null)
            {
                LogMng.Instance.onWriteStatus("Converting OpenBet Error");
                return PROCESS_RESULT.ERROR;
            }

            int retryCount = 2;
#if OXYLABS
            retryCount = 5;
#endif

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
                    string hdrType = "";
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

                    //LogMng.Instance.onWriteStatus($"Placed bet prepare eventTitle:{info.eventTitle} league:{info.league} odd:{info.odds} stake:{info.stake} mn:{openbet.market_mn} si:{openbet.market_si} mi:{openbet.market_mi}");

                    //SetTraceHeaders();
                    Global.strRequestUrl = "https://" + domain + "/scommesse/getOverviewLive/?idDiscipline=0&idTab=0&menu=menu&isFromUser=false";
                    string baseURL = "https://" + domain;
                    string functionString = $"window.fetch('{Global.strRequestUrl}', {{ method: 'GET', headers: {{ 'accept': '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json', }}, credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', referrer: '{baseURL}' }}).then(response => response.json());";

                    
                    Global.strPlaceBetResult = "";
                    Global.waitResponseEvent.Reset();

                    Global.RunScriptCode(functionString);

                    if (!Global.waitResponseEvent.Wait(3000))
                    {
                        LogMng.Instance.onWriteStatus("GetAllLivesMatches Error");
                        return PROCESS_RESULT.ERROR;
                    }
                    var strContent = Global.strPlaceBetResult;
                    //LogMng.Instance.onWriteStatus("all live sports--");
                    //LogMng.Instance.onWriteStatus(strContent);
                    JObject origObject1 = JObject.Parse(strContent);
                    double maxSimilarity = 0;
                    string EventdetailUrl = string.Empty;
                    
                    foreach (var objEvent in origObject1["leo"])
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

                    Global.strRequestUrl = EventdetailUrl;
                    functionString = $"window.fetch('{Global.strRequestUrl}', {{ method: 'GET', headers: {{ 'accept': '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json', }}, credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', referrer: '{baseURL}' }}).then(response => response.json());";

                    Global.strPlaceBetResult = "";
                    Global.waitResponseEvent.Reset();

                    Global.RunScriptCode(functionString);

                    if (!Global.waitResponseEvent.Wait(3000))
                    {
                        LogMng.Instance.onWriteStatus("GetAllLivesMatches Error");
                        return PROCESS_RESULT.ERROR;
                    }
                                        
                    strContent = Global.strPlaceBetResult;
                    //LogMng.Instance.onWriteStatus("target match--");
                    //LogMng.Instance.onWriteStatus(strContent);
                    JObject origObject2 = JObject.Parse(strContent);
                    foreach (var objEvent in origObject2["mktWbD"])
                    {
                        JToken matchObj = objEvent.ToObject<JProperty>().Value;

                        foreach (var marketEvent in matchObj["ms"])
                        {
                            JToken marketObj = marketEvent.ToObject<JProperty>().Value;
                            foreach (var piEvent in marketObj["asl"])
                            {
                                if (piEvent["si"].ToString() == openbet.market_si && piEvent["mi"].ToString() == openbet.market_mi)
                                {
                                    hdrType = matchObj["ht"].ToString();
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

                    double NewOdd = Utils.ParseToDouble(oddsValue);
                    LogMng.Instance.onWriteStatus($"NewOdd : {NewOdd}");
                    if (CheckOddDropCancelBet(NewOdd, info))
                    {
                        LogMng.Instance.onWriteStatus(string.Format("Odd is changed from {0} To {1}", info.odds, NewOdd.ToString("N2")));
                        return PROCESS_RESULT.MOVED;
                    }

                    double MaxWin = info.stake * NewOdd;
                    string maxPG = MaxWin.ToString().Replace(",", ".");
                    //string ReqJson = $"{{\"single_stake\":{{\"combsInfo\":[{{\"combType\":\"1\",\"combNum\":\"1\",\"stake\":{info.stake}}}],\"sumCombsXType\":1}},\"total_stake\":{info.stake},\"fixed\":[],\"events\":[{{\"isLive\":\"true\",\"selName\":\"{selName}\",\"selId\":\"{selId}\",\"oddsId\":\"{oddsId}\",\"oddsValue\":\"{oddsValue}\",\"markName\":\"{markName}({sNL})\",\"markId\":\"{markId}\",\"markTypId\":\"{markTypId}\",\"hdrType\":\"{hdrType}\",\"prvIdEvt\":\"{prvIdEvt}\",\"aamsId\":\"{aamsId}\",\"evtName\":\"{evtName}\",\"evnDate\":\"{evnDate}\",\"evnDateTs\":\"{evnDateTs}\",\"evtId\":\"{evtId}\",\"catId\":\"{catId}\",\"disId\":\"{disId}\",\"tId\":\"{tId}\",\"tName\":\"{tName}\",\"onLineCode\":\"{onLineCode}\",\"vrt\":{vrt}}}],\"creationTime\":{Utils.getTick()},\"virtual\":false,\"allowStakeReduction\":false,\"allowOddChanges\":false}}";
                    string ReqJson = $"{{\"groupCombs\":{{\"combsInfo\":[{{\"combType\":\"1\",\"combNum\":\"1\",\"stake\":{info.stake}}}],\"sumCombsXType\":1}},\"totalStake\":{info.stake},\"fixed\":[],\"events\":[{{\"isLive\":\"true\",\"selName\":\"{selName}\",\"selId\":\"{selId}\",\"oddsId\":\"{oddsId}\",\"oddsValue\":\"{oddsValue}\",\"markName\":\"{markName}({sNL})\",\"markId\":\"{markId}\",\"markTypId\":\"{markTypId}\",\"hdrType\":\"{hdrType}\",\"prvIdEvt\":\"{prvIdEvt}\",\"aamsId\":\"{aamsId}\",\"evtName\":\"{evtName}\",\"evnDate\":\"{evnDate}\",\"evnDateTs\":\"{evnDateTs}\",\"evtId\":\"{evtId}\",\"catId\":\"{catId}\",\"disId\":\"{disId}\",\"tId\":\"{tId}\",\"tName\":\"{tName}\",\"onLineCode\":\"{onLineCode}\",\"vrt\":{vrt}}}],\"creationTime\":{Utils.getTick()},\"virtual\":false,\"allowStakeReduction\":false,\"allowOddChanges\":false,\"bonusWager\":false,\"maxPag\":\"{maxPG}\"}}";
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"insertBet Req : {ReqJson}");
#endif
                    while (--retryCount >= 0)
                    {
                        try
                        {
                            string betUrl = "https://" + domain + "/scommesse/insertBet";

                            int subRetryCount = 6;
                            string strBetResp = string.Empty;

                            while (--subRetryCount > 0)
                            {
                                try
                                {
                                    Global.strRequestUrl = betUrl;
                                    functionString = $"window.fetch('{Global.strRequestUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{ReqJson}', method: 'POST' }}).then(response => response.json());";

                                    Global.strPlaceBetResult = "";
                                    Global.waitResponseEvent.Reset();

                                    Global.RunScriptCode(functionString);


                                    if (!Global.waitResponseEvent.Wait(20000) || string.IsNullOrEmpty(Global.strPlaceBetResult))
                                    {
                                        continue;
                                    }

                                    strBetResp = Global.strPlaceBetResult;

#if (TROUBLESHOT)
                                    LogMng.Instance.onWriteStatus("insertBet Res:" + strBetResp);
#endif

                                    break;
                                }
                                catch (Exception ex)
                                {
                                    LogMng.Instance.onWriteStatus("insertBet exception:" + ex);
                                }
                            }
                            
                            dynamic jsonBetResp = JsonConvert.DeserializeObject<dynamic>(strBetResp);

                            if (jsonBetResp.success.ToString() == "True")
                            {
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus(string.Format("Placed bet in Step 1"));
#endif
                                string confirmBetUrl = "https://" + domain + "/scommesse/pendingBet/" + jsonBetResp.data.couponCode.ToString();
                                string strConfirmBetResp = string.Empty;
                                subRetryCount = 6;
                                while (--subRetryCount > 0)
                                {
                                    try
                                    {
                                        Global.strRequestUrl = confirmBetUrl;
                                        functionString = $"window.fetch('{Global.strRequestUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '', method: 'POST' }}).then(response => response.json());";

                                        Global.strPlaceBetResult = "";
                                        Global.waitResponseEvent.Reset();

                                        Global.RunScriptCode(functionString);


                                        if (!Global.waitResponseEvent.Wait(20000) || string.IsNullOrEmpty(Global.strPlaceBetResult))
                                        {
                                            continue;
                                        }

                                        strConfirmBetResp = Global.strPlaceBetResult;

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

                                if (jsonConfirmBetResp.statusDesc.ToString() == "Placed" || jsonConfirmBetResp.statusDesc.ToString() == "P")
                                    return PROCESS_RESULT.PLACE_SUCCESS;
                                
                                LogMng.Instance.onWriteStatus($"Placing failed Reason: {jsonConfirmBetResp.statusDesc}");
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
#if OXYLABS
                        if (e.Message.Contains("An error occurred while sending the request"))
                        {
                            Global.ProxySessionID = new Random().Next().ToString();
                            m_client = initHttpClient(false);
                        }
#endif
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"Place bet exception {ex.StackTrace} {ex.Message}");
            }
            
            LogMng.Instance.onWriteStatus(string.Format("** PLACE BET FAIL"));
            return PROCESS_RESULT.ERROR;
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
            int nRetry = 2;
            double balance = 0;
            while (nRetry >= 0)
            {
                nRetry--;
                try
                {

                    string formDataString = "";
                    string getBalanceURL = "https://" + domain + "/scommesse/getBalance/";
//#if (LOTTOMATICA)
                    formDataString = "page=sport";
//#endif

                    string functionString = $"window.fetch('{getBalanceURL}', {{ headers: {{ accept: 'application/json', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/x-www-form-urlencoded' }}, TE: 'trailers', 'X-Requested-With': 'XMLHttpRequest', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                    Global.strAddBetResult = "";
                    Global.waitResponseEvent.Reset();

                    Global.RunScriptCode(functionString);

                    if (!Global.waitResponseEvent.Wait(3000))
                    {
                        continue;
                    }
                    LogMng.Instance.onWriteStatus($"getBalance response: {Global.strAddBetResult}");
                    dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(Global.strAddBetResult);

                    string RealUserBalance = jsonContent[0].ToString();
                    balance = Utils.ParseToDouble(RealUserBalance);

                    break;
                }
                catch (Exception e)
                {
                    LogMng.Instance.onWriteStatus(string.Format("GetBalance exception: {0} {1}", e.Message, e.StackTrace));
                }
            }

            LogMng.Instance.onWriteStatus(string.Format("GetBalance: {0}", balance));
            return balance;
        
        }
    }
#endif
}
//#if (GOLDBET)
//                int nRetry = 0;
//                while (Global.bRun)
//                {
//                    nRetry++;
//                    if (nRetry > 600)
//                    {
//                        LogMng.Instance.onWriteStatus($"Goldbet 2fa is not verifed");
//                        return false;
//                    }
//                    if (!string.IsNullOrEmpty(Global.strAddBetResult))
//                    {
//                        try
//                        {
//                            dynamic otpResponse = JsonConvert.DeserializeObject<dynamic>(Global.strAddBetResult);

//                            if (otpResponse.ResultCode.ToString() == "0")
//                                break;
//                        }
//                        catch { }
//                    }
//                    Thread.Sleep(500);
//                }
//#endif

