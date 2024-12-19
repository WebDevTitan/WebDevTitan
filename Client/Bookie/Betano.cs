namespace Project.Bookie
{
#if (BETANO)
    class BetanoCtrl : IBookieController
    {
        
        public HttpClient m_client = null;
        
        Object lockerObj = new object();
        //private static ProxyServer _proxyServer = null;
        
        public void Close()
        {

        }

        public void Feature()
        {

        }

        public bool logout()
        {
            return true;
        }
        public BetanoCtrl()
        {
            Global.placeBetHeaderCollection.Clear();
            m_client = initHttpClient();
            //Global.SetMonitorVisible(false);
        }

        public bool Pulse()
        {
            return false;
           
        }

        public int GetPendingbets()
        {
            return 0;
        }

        public bool login()
        {
            //Global.SetMonitorVisible(true);
            bool bLogin = false;
            try
            {
                lock (lockerObj)
                {
                    Global.RemoveCookies();
                    m_client = initHttpClient();
                    

                    
                    Global.OpenUrl($"https://{Setting.Instance.domain}/");

                    Global.waitResponseEvent.Reset();
                    Thread.Sleep(3000);
                    Global.OpenUrl($"https://{Setting.Instance.domain}/myaccount/login");
                    
                    //Global.RunScriptCode("document.getElementsByClassName('uk-button uk-button-primary GTM-login')[0].click();");


                    if (Global.waitResponseEvent.Wait(10000))
                    {
                        LogMng.Instance.onWriteStatus($"login page loaded");
                        //Global.RunScriptCode($"document.getElementsByName('Username')[0].value='{Setting.Instance.username}';");
                        //Global.RunScriptCode($"document.getElementsByName('Password')[0].value='{Setting.Instance.password}';");

                        Thread.Sleep(1000);

                        //Global.RunScriptCode("document.getElementsByClassName('button button--ripple button button--basic button--secondary')[0].removeAttribute('disabled');");
                        //Global.RunScriptCode("document.getElementsByClassName('button button--ripple button button--basic button--secondary')[0].click();");


                        string token = Global.strPlaceBetResult;

                        var payload = JsonConvert.DeserializeObject<dynamic>(token);

                        string token1 = payload.token1.ToString();
                        string token2 = payload.token2.ToString();

                        string command = "var valIoBlackbox = document.getElementById('ioBlackBox').value;" +
                                    "var postContent = {'Username':'" + Setting.Instance.username + "','Password':'" + Setting.Instance.password + "','ParentUrl':'https://" + Setting.Instance.domain + "/','IoBlackBox':valIoBlackbox};" +
                                    "var xhr = new XMLHttpRequest();" +
                                    "xhr.open('POST', 'https://" + Setting.Instance.domain + "/myaccount/login?user=" + Setting.Instance.username + "');" +
                                    "xhr.withCredentials = true;" +
                                    "xhr.setRequestHeader('Accept', 'application/json, text/plain, */*');" +
                                    "xhr.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');" +
                                    "xhr.setRequestHeader('token1', '" + token1 + "');" +
                                    "xhr.setRequestHeader('token2', '" + token2 + "');" +
                                    "xhr.onreadystatechange = function() {" +
                                    "  if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {" +
                                    "	  window.location.reload();" +
                                    "  }" +
                                    "};" +
                                    "xhr.send(JSON.stringify(postContent));";
                        Global.RunScriptCode(command);
                        Thread.Sleep(3000);
                        Global.RefreshPage();
                        int nRetry1 = 0;
                        while (nRetry1 < 3)
                        {
                            Thread.Sleep(3000);
                            Task.Run(async () => await Global.GetCookie($"https://{Setting.Instance.domain}")).Wait();
                            if (getBalance() >= 0)
                            {
                                return true;
                            }
                            nRetry1++;
                        }
                    }
                }                
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus($"login exception {e.StackTrace} {e.Message}");
            }
            //Global.SetMonitorVisible(false);
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

            //info[0].result = PlaceBet(info[0].betburgerInfo);
        }

        public PROCESS_RESULT PlaceBet(ref BetburgerInfo info)
        {            
            if (getBalance() < 0)
            {
                if (!login())
                    return PROCESS_RESULT.NO_LOGIN;
            }            

#if OXYLABS
            retryCount = 5;
#endif

            try
            {
                lock (lockerObj)
                {
                    //string ReqJson = $"{{\"betRefs\":[\"{info.direct_link}\"],\"GENSLIP\":\"\",\"betslip\":{{\"hash\":\"\",\"slipData\":\"\",\"parts\":[],\"bets\":[]}}}}";
                    string ReqJson = $"{{\"selectionIds\":[\"{info.direct_link}\"],\"betslip\":{{\"hash\":\"\",\"slipData\":\"\",\"legs\":[],\"bets\":[],\"betslipTabId\":1,\"betslipTrackId\":\"\"}}}}";
                    string betUrl = $"https://{Setting.Instance.domain}/api/betslip/v3/plain-leg/";
                    //LogMng.Instance.onWriteStatus($"bet Req : {ReqJson}");

                    var postData = new StringContent(ReqJson, Encoding.UTF8, "application/json");
                    HttpResponseMessage betResponse = m_client.PostAsync(betUrl, postData).Result;
                    string content = betResponse.Content.ReadAsStringAsync().Result;
                    dynamic addparts_res = JsonConvert.DeserializeObject<dynamic>(content);
                    LogMng.Instance.onWriteStatus($"plain-leg result {content}");

                    JObject getbetslip_req = new JObject();

                    getbetslip_req["bets"] = addparts_res.data.bets;
                    getbetslip_req["bets"][0]["amount"] = info.stake;

                    getbetslip_req["betslip"] = new JObject();
                    getbetslip_req["betslip"]["bets"] = addparts_res.data.bets;
                    getbetslip_req["betslip"]["bets"][0]["amount"] = info.stake;                    
                    getbetslip_req["betslip"]["betSlipTabId"] = addparts_res.data.betSlipTabId.ToString();
                    getbetslip_req["betslip"]["betslipTrackId"] = addparts_res.data.betslipTrackId.ToString();
                    getbetslip_req["betslip"]["hash"] = addparts_res.data.hash.ToString();
                    getbetslip_req["betslip"]["legs"] = addparts_res.data.legs;
                    getbetslip_req["betslip"]["slipData"] = addparts_res.data.slipData.ToString();
                    double newodd = info.odds;
                    try
                    {
                        newodd = Convert.ToDouble(addparts_res.data.bets.odds.ToString());
                        if (info.odds < newodd)
                        {
                            
                            LogMng.Instance.onWriteStatus($"Lower odd, cancelled. ({info.odds}) -> ({newodd})");
                            return PROCESS_RESULT.ERROR;
                        }
                    }
                    catch { }
                    string temp = getbetslip_req.ToString();
                    
                    betUrl = $"https://{Setting.Instance.domain}/api/betslip/v3/updatebets";

                    HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), betUrl);
                    request.Content = new StringContent(getbetslip_req.ToString(), Encoding.UTF8, "application/json");
                    betResponse = m_client.SendAsync(request).Result;
                    content = betResponse.Content.ReadAsStringAsync().Result;
                    dynamic updatebets_res = JsonConvert.DeserializeObject<dynamic>(content);
                    LogMng.Instance.onWriteStatus($"updatebets result {content}");

                    JObject placebet_req = new JObject();                    
                    placebet_req["betslip"] = new JObject();
                    placebet_req["betslip"]["bets"] = updatebets_res.data.bets;
                    placebet_req["betslip"]["betSlipTabId"] = updatebets_res.data.betSlipTabId.ToString();
                    placebet_req["betslip"]["betslipTrackId"] = updatebets_res.data.betslipTrackId.ToString();
                    placebet_req["betslip"]["hash"] = updatebets_res.data.hash.ToString();
                    placebet_req["betslip"]["legs"] = updatebets_res.data.legs;
                    placebet_req["betslip"]["oddschanges"] = 1;
                    placebet_req["betslip"]["slipData"] = updatebets_res.data.slipData.ToString();


                    postData = new StringContent(placebet_req.ToString(), Encoding.UTF8, "application/json");
                    betUrl = $"https://{Setting.Instance.domain}/api/betslip/v3/place";
                    betResponse = m_client.PostAsync(betUrl, postData).Result;
                    content = betResponse.Content.ReadAsStringAsync().Result;
                    LogMng.Instance.onWriteStatus($"placebet result {content}");
                    try
                    {
                        dynamic placebets_res = JsonConvert.DeserializeObject<dynamic>(content);

                        //LogMng.Instance.onWriteStatus("jsonBetResp:" + jsonBetResp.ToString());
                        if (placebets_res.data.ToString() != "null")
                        {
                            try
                            {
                                if (placebets_res.errorCode != null)
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("Place bet failed. {0} ", placebets_res.errorCode.ToString()));

                                    return PROCESS_RESULT.ERROR;
                                }
                            }
                            catch { }
                            return PROCESS_RESULT.PLACE_SUCCESS;
                        }
                        //else if (placebets_res.error.error.ToString() == "998")
                        //{
                        //    bool bRet = login();
                        //    if (!bRet)
                        //    {
                        //        LogMng.Instance.onWriteStatus(string.Format("Bet failed(Need relogin)"));
                        //        return PROCESS_RESULT.NO_LOGIN;
                        //    }
                        //    LogMng.Instance.onWriteStatus(string.Format("Retry after relogin"));
                        //}
                        else
                        {
                            LogMng.Instance.onWriteStatus(string.Format("Place bet failed. {0} ", placebets_res.errors[0].ToString()));

                            return PROCESS_RESULT.ERROR;
                        }
                    }
                    catch 
                    {
                        LogMng.Instance.onWriteStatus("Odd is changed");
                        return PROCESS_RESULT.ERROR;
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
        public double getBalance()
        {
            double balance = -1;
            try
            {
                
                var postData = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");
                lock (lockerObj)
                {
                    
                    HttpResponseMessage response = m_client.GetAsync($"https://{Setting.Instance.domain}/myaccount/my-account/api/customer/balance").Result;
                    response.EnsureSuccessStatusCode();

                    string content = response.Content.ReadAsStringAsync().Result;
                    LogMng.Instance.onWriteStatus("getbalance response " + content);

                    dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(content);
                    string RealUserBalance = jsonContent.Result[0].BettingBalance.ToString();
                    string[] splits = RealUserBalance.Split(' ');
                    balance = Utils.ParseToDouble(splits[0]);
                    
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus("Error in getBalance: " + ex.ToString());
            }
            return balance;
        }
    }
#endif
}
