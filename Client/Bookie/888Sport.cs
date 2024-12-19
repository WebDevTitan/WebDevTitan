namespace Project.Bookie
{
#if (_888SPORT)
    

    public class FingerPrintHash
    {
        public string UserId { get; set; }
        public string Hash { get; set; }

        public FingerPrintHash(string userId, string hash)
        {
            UserId = userId;
            Hash = hash;
        }
    }

    public class BetResultJson
    {
        public List<Errors> betErrors { get; set; } = new List<Errors>();
        public string couponRef { get; set; }
        public string status { get; set; }
    }

    public class Errors
    {
        [JsonProperty("errors")]
        public List<ErrorItem> items { get; set; }
    }

    public class ErrorItem
    {
        public string type { get; set; }
        [JsonProperty("arguments")]
        public List<Argument> arguments { get; set; }
    }

    public class Argument
    {
        public string type { get; set; }
        public string value { get; set; }
    }
    
    public class AuthTokenJson
    {
        public string authtoken { get; set; }
        public string market { get; set; }
        public string playerId { get; set; }
        public string streamingEnabled { get; set; }
        public string offering { get; set; }
        public string country { get; set; }
        public string lang { get; set; }
    }

    class _888SportCtrl : IBookieController
    {
        public HttpClient m_client = null;
             
        Object lockerObj = new object();

        static string FingerPrintHashFileName = "FPHList.dat";

        public List<FingerPrintHash> HashList = new List<FingerPrintHash>();
        private string domain = "888sport.it";
        

        
        public string GetFingerPrintHash()
        {
            string result = ""; // "fc49a49db4ff70108398dd084f545ae8";
            foreach (var hashItr in HashList)
            {
                if (hashItr.UserId == Setting.Instance.username)
                {
                    result = hashItr.Hash;
                    break;
                }
            }
            return result;
        }

        public void Close()
        {

        }

        public bool logout()
        {
            return true;
        }

        public void Feature()
        {

        }
        public bool FetchFingerPrintHash()
        {
            bool bResult = false;
            LogMng.Instance.onWriteStatus("Login account manually for fetching account information");
            Global.SetMonitorVisible(true);
            Global.OpenUrl(string.Format("https://www.{0}/", domain));

            Global.waitResponseEvent.Reset();
            if (Global.waitResponseEvent.Wait(60000))
            {
                for (int i = HashList.Count - 1; i >= 0; i--)
                {
                    if (HashList[i].UserId == Setting.Instance.username)
                        HashList.RemoveAt(i);
                }
                HashList.Add(new FingerPrintHash(Setting.Instance.username, Global.strPlaceBetResult));
                string jsonHashList = JsonConvert.SerializeObject(HashList);
                File.WriteAllText(FingerPrintHashFileName, jsonHashList);
                bResult = true;
                LogMng.Instance.onWriteStatus("Capturing Hash value success");
            }
            else
            {
                bResult = false;
                LogMng.Instance.onWriteStatus("Capturing Hash value failed");
            }
            Global.SetMonitorVisible(false);
            return bResult;
        }

        public void OpenFingerPrintHashFile()
        {
            try
            {
                string jsonHashList = File.ReadAllText(FingerPrintHashFileName);
                HashList = JsonConvert.DeserializeObject<List<FingerPrintHash>>(jsonHashList);
            }
            catch { }
        }
        public bool Pulse()
        {
            return false;
        }

                
        public _888SportCtrl()
        {
            OpenFingerPrintHashFile();
            if (string.IsNullOrEmpty(GetFingerPrintHash()))
            {
                FetchFingerPrintHash();
            }

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
            ServicePointManager.DefaultConnectionLimit = 2;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://www.888sport.it/");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/javascript, */*; q=0.01");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://www.888sport.it");

            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "cross-site");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"Chromium\";v=\"104\", \" Not A;Brand\";v=\"99\", \"Microsoft Edge\";v=\"104\", \"Microsoft Edge WebView2\";v=\"104\"");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "cross-site");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;

            return httpClientEx;
        }

        public bool login()
        {            
            
            bool bLogin = false;
            int nRetry = 0;
            while (nRetry++ < 2)
            {
                try
                {
                    lock (lockerObj)
                    {
                        


                        LogMng.Instance.onWriteStatus($"888sport login Start");
                        Global.OpenUrl("https://www.888sport.it/");

                        int nRetry1 = 0;
                        while (nRetry1 < 60)
                        {
                            Thread.Sleep(500);
                            string result = Global.GetStatusValue("return document.getElementById('leftPanelaCloginButton').outerHTML").ToLower();

                            if (result.Contains("class"))
                            {
                                break;
                            }

                            result = Global.GetStatusValue("return sessionStorage.userIsLoggedIn;").ToLower();
                            if (result.Replace("\"", "") == "true")
                            {
                                break;
                            }

                            nRetry1++;
                        }
                        string result1 = Global.GetStatusValue("return sessionStorage.userIsLoggedIn;").ToLower();
                        if (result1.Replace("\"", "") == "true")
                        {
                            Thread.Sleep(500);
                            //Task.Run(async () => await Global.GetCookie("https://unifiedclient.safe-iplay.com")).Wait();
                            LogMng.Instance.onWriteStatus("Alreay logined");
                            return true;
                        }

                        if (nRetry1 >= 60)       //Page is loading gray page. let's retry
                        {

                            LogMng.Instance.onWriteStatus($"First Page is not displayed");
                            return false;
                        }

                        Global.RunScriptCode("document.getElementById('leftPanelaCloginButton').click();");

                        //LogMng.Instance.onWriteStatus($"login step 2 {eleLoginBtn}");

                        nRetry1 = 0;
                        while (nRetry1 < 60)
                        {
                            Thread.Sleep(500);
                            string result = Global.GetStatusValue("return document.getElementById('rlLoginUsername').outerHTML").ToLower();

                            if (result.Contains("placeholder"))
                            {
                                break;
                            }
                            nRetry1++;
                        }
                        if (nRetry1 >= 60)       //Page is loading gray page. let's retry
                        {
                            LogMng.Instance.onWriteStatus($"Login Page is not displayed");
                            return false;
                        }

                        Global.RunScriptCode($"document.getElementById('rlLoginUsername').value='{Setting.Instance.username}';");

                        Global.RunScriptCode($"document.getElementById('rlLoginPassword').value='{Setting.Instance.password}';");

                        Thread.Sleep(500);

                        Global.RunScriptCode("document.getElementById('rlLoginSubmit').click();");

                        Global.waitResponseEvent.Reset();
                        if (Global.waitResponseEvent.Wait(30000))
                        {
                            //Task.Run(async () => await Global.GetCookie("https://unifiedclient.safe-iplay.com")).Wait();
                            Thread.Sleep(500);
                            LogMng.Instance.onWriteStatus($"Login Success");
                            return true;
                        }
                        else
                        {
                            LogMng.Instance.onWriteStatus("Login No Response");
                        }
                    }
                }
                catch (Exception e)
                {
                    LogMng.Instance.onWriteStatus($"login exception {e.StackTrace} {e.Message}");
                }
            }

            LogMng.Instance.onWriteStatus($"Login Failed");
            return false;
        }

        public void PlaceBet(List<CapsuledBetburgerInfo> info)
        {
            if (info.Count < 1)
                return;

            info[0].result = PlaceBet(info[0].betburgerInfo);
        }
        public PROCESS_RESULT PlaceBet(BetburgerInfo info)
        {

            if (Global.unibetSessionInfo == null)
            {
                LogMng.Instance.onWriteStatus("unibetTicket waiting..");
                return PROCESS_RESULT.ERROR;
            }

            string result = Global.GetStatusValue("return sessionStorage.userIsLoggedIn;").ToLower();
            if (result.Replace("\"", "") != "true")
            {
                return PROCESS_RESULT.NO_LOGIN;
            }


            m_client = initHttpClient();

            try
            {
                Monitor.Enter(Global.locker_unifiedclientHeaders);
                foreach (KeyValuePair<string, string> param in Global.kambicdnHeaders)
                {
                    if (param.Key == "Accept")
                        continue;
                    try
                    {
                        m_client.DefaultRequestHeaders.Remove(param.Key);
                        m_client.DefaultRequestHeaders.TryAddWithoutValidation(param.Key, param.Value);
                    }
                    catch { }
                }
            }
            catch { }
            finally
            {
                Monitor.Exit(Global.locker_unifiedclientHeaders);
            }

            try
            {
                //if (!GetApiKey())
                //    return PROCESS_RESULT.CRITICAL_SITUATION;


                string stakeStr = "";
                try
                {
                    double stakeF = info.stake * 1000;
                    stakeStr = stakeF.ToString();
                }
                catch (Exception e)
                {

                }

                List<OpenBet_Unibet> markets = new List<OpenBet_Unibet> { Utils.ConvertBetburgerPick2OpenBet_Unibet(info) };

                if (!AddBetSlip(markets))
                {
                    return PROCESS_RESULT.SUSPENDED;
                }

                if (!OddCheck(markets))
                {
                    return PROCESS_RESULT.MOVED;
                }
                if (FinalBet(markets, stakeStr))
                    return PROCESS_RESULT.PLACE_SUCCESS;
            }
            catch (Exception e)
            {
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

            HttpClient tmpClient = new HttpClient();
            while (--retryCount >= 0)
            {
                try
                {
                    try
                    {
                        Monitor.Enter(Global.locker_unifiedclientHeaders);
                        foreach (KeyValuePair<string, string> param in Global.unifiedclientHeaders)
                        {
                            try
                            {
                                tmpClient.DefaultRequestHeaders.Remove(param.Key);
                                tmpClient.DefaultRequestHeaders.TryAddWithoutValidation(param.Key, param.Value);
                            }
                            catch { }
                        }
                    }
                    catch { }
                    finally { 
                        Monitor.Exit(Global.locker_unifiedclientHeaders);
                    }


                    //HttpResponseMessage resoponse = tmpClient.GetAsync("https://unifiedclient.safe-iplay.com/api/SessionData/RefreshUserState").Result;
                    HttpResponseMessage resoponse = tmpClient.GetAsync($"https://unifiedclient.safe-iplay.com/api/context/authenticated/?_cb={Utils.getTick()}").Result;
                    resoponse.EnsureSuccessStatusCode(); 

                    string result = resoponse.Content.ReadAsStringAsync().Result;
                    dynamic json = JsonConvert.DeserializeObject<dynamic>(result);
                    balance = Utils.ParseToDouble(json.InitialState.userContext.userBalance.ToString());
                    break;
                }
                catch (Exception e)
                {

                }
            }
            return balance;
        }

        //public bool GetApiKey()
        //{
            

            

        //    //string loginresult = Global.GetStatusValue("return sessionStorage.userIsLoggedIn;").ToLower();
        //    //if (loginresult.Replace("\"", "") == "true")
        //    //{
        //    //    if (unibetSessionInfo != null)
        //    //        return true;
        //    //}

        //    int nRetry = 2;
        //    while (nRetry-- >= 0)
        //    {
        //        try
        //        {
                    
        //            string market = "IT";

        //            //if (string.IsNullOrEmpty(Global.unibetTicket))
        //            //{
        //            //    Global.unibetTicket = Global.GetStatusValue("return _kc.ticket;").Replace("\"", "");
        //            //}

        //            string parameter = "{\"sessionAttributes\":{\"fingerprintHash\":\"" + GetFingerPrintHash() + "\"},\"punterId\":\"\",\"requestStreaming\":true,\"customerSiteIdentifier\":\"31\",\"channel\":\"WEB\",\"ticket\":\"" + Global.unibetTicket + "\",\"market\":\"" + market + "\"}";
        //            StringContent jsonContent = new StringContent(parameter, Encoding.UTF8, "application/json");
                    
        //            string loginApiLink = loginApiLink = "https://mt-auth.kambicdn.org/player/api/v2019/888it/punter/login.json?market=IT&lang=it_IT&channel_id=1&client_id=2&settings=true";
                    

        //            HttpResponseMessage apiKeyResponse = m_client.PostAsync(loginApiLink, jsonContent).Result;
        //            apiKeyResponse.EnsureSuccessStatusCode();
        //            string apikeyStr = apiKeyResponse.Content.ReadAsStringAsync().Result;
        //            Global.unibetSessionInfo = JsonConvert.DeserializeObject<UnibetSessionInfo>(apikeyStr);
        //            return true;
        //        }
        //        catch (Exception e)
        //        {
        //            if (!login())
        //                return false;
        //        }
        //    }
        //    return false;
        //}

        private bool AddBetSlip(List<OpenBet_Unibet> markets)
        {
            long tick = Utils.getTick();

            string marketParam = string.Join("&id=", markets.Select(o => o.marketId));
            string outcomeUrl = $"https://eu-offering.kambicdn.org/offering/v2018/888it/betoffer/outcome.json?lang=it_IT&market=IT&client_id=2&channel_id=1&ncid={tick}&id={marketParam}";

            HttpResponseMessage outcomeResp = m_client.GetAsync(outcomeUrl).Result;
            string resultOutcomeStr = outcomeResp.Content.ReadAsStringAsync().Result;
            if (resultOutcomeStr.Contains("No bet offers found"))
            {
                return false;
            }
            dynamic jsonOutcomeResp = JsonConvert.DeserializeObject<dynamic>(resultOutcomeStr);

            for (int i = 0; i < jsonOutcomeResp.betOffers.Count; i++)
            {
                dynamic betoffer = jsonOutcomeResp.betOffers[i];
                markets[i].eventId = betoffer["eventId"].ToString();
            }
            return true;
        }
        private bool OddCheck(List<OpenBet_Unibet> markets)
        {
            m_client.DefaultRequestHeaders.Remove("Authorization");
            m_client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", string.Format("Bearer {0}", Global.unibetSessionInfo.token));

            string validateUrl = $"https://mt-auth.kambicdn.org/player/api/v2019/888it/coupon/validate.json?lang=it_IT&market=IT&client_id=2&channel_id=1";
            long tick = Utils.getTick();
            

            string strContent = GetOddCheckJsonContent(markets);

            StringContent jsonContent = new StringContent(strContent, Encoding.UTF8, "application/json");
            HttpResponseMessage resultBetting = m_client.PostAsync(validateUrl, jsonContent).Result;
            string resultBettingStr = resultBetting.Content.ReadAsStringAsync().Result;
            int oddTry = 0;
            while (++oddTry < 6 && resultBettingStr.Contains("ODDS_CHANGED"))
            {
                Thread.Sleep(500);
                if (!ChangeMarketOdds(resultBettingStr, markets))
                    return false;
                strContent = GetOddCheckJsonContent(markets);
                jsonContent = new StringContent(strContent, Encoding.UTF8, "application/json");
                resultBetting = m_client.PostAsync(validateUrl, jsonContent).Result;
                resultBettingStr = resultBetting.Content.ReadAsStringAsync().Result;
            }
            if (!resultBettingStr.Contains("SUCCESS"))
            {
                Trace.WriteLine("New Issue for validate - " + resultBettingStr);
                return false;
            }
            return true;
        }
        private bool FinalBet(List<OpenBet_Unibet> markets, string stakeStr)
        {
            int retryCnt = 0;
            while (retryCnt < 2)
            {
                string bettingUrl = "https://mt-auth.kambicdn.org/player/api/v2019/888it/coupon.json?lang=it_IT&market=IT&client_id=2&channel_id=1";
                
                string strContent = GetCouponJsonContent(markets, stakeStr);

                HttpContent jsonContent = new StringContent(strContent, Encoding.UTF8, "application/json");
                HttpResponseMessage resultBetting = m_client.PostAsync(bettingUrl, jsonContent).Result;
                string resultBettingStr = resultBetting.Content.ReadAsStringAsync().Result;
                BetResultJson resultJson = JsonConvert.DeserializeObject<BetResultJson>(resultBettingStr);

                if (resultBettingStr.Contains("SUCCESS") || resultBettingStr.Contains("LIVE_DELAY_PENDING") || resultBettingStr.Contains("APPROVAL_PENDING"))
                {
                    if (resultBettingStr.Contains("LIVE_DELAY_PENDING"))
                    {
                        int delayTime = Utils.parseToInt(Regex.Match(resultBettingStr, @"delayBeforeAcceptingBet[^:]*:(?<VAL>[^,]*)").Groups["VAL"].Value);
                        string refId = Regex.Match(resultBettingStr, @"couponRef[^:]*:(?<VAL>[^,]*)").Groups["VAL"].Value;
                        int waitTime = delayTime > 0 ? delayTime : 9;
                        Thread.Sleep(waitTime * 1000);
                        bettingUrl = string.Format("https://mt-auth.kambicdn.org/player/api/v2019/888it/coupon/history/{0}.json?lang=it_IT&market=IT&client_id=2&channel_id=1&ncid={1}&liveDelayPoll=true", refId, Utils.getTick());
                        
                        resultBetting = m_client.GetAsync(bettingUrl).Result;

                    }

                    return true;
                }

                if (resultJson.betErrors != null)
                {
                    string type = resultJson.betErrors[0].items[0].type;
                    if (type == "STAKE_TOO_HIGH")
                    {
                        stakeStr = resultJson.betErrors[0].items[0].arguments[0].value;
                        retryCnt++;
                        continue;
                    }
                    else if (resultBettingStr.Contains("ODDS_CHANGED"))
                    {
                        Trace.WriteLine($"Odd Changed1 - {resultBettingStr}");
                        if (!ChangeMarketOdds(resultBettingStr, markets))
                            return false;
                        retryCnt++;
                        continue;
                    }
                    else
                    {
                        LogMng.Instance.onWriteStatus(string.Format("FinalBet Failed ({0})", resultJson.betErrors[0].items[0].type));

                        break;
                    }
                }
                else if (resultBettingStr.Contains("NOT_ENOUGH_FUNDS"))
                {
                    LogMng.Instance.onWriteStatus(string.Format("FinalBet Failed (NOT_ENOUGH_FUNDS)"));

                    break;
                }
            }
            return false;
        }
        public string GetOddCheckJsonContent(List<OpenBet_Unibet> markets)
        {
            string ret = "";
            try
            {
                if (markets == null || markets.Count == 0)
                    return ret;

                string strRows = string.Join(",", markets.Select((o, i) => $@"{{""index"":{i},""odds"":{o.odds},""outcomeId"":{o.marketId},""type"":""SIMPLE""}}"));

                ret = $@"{{""couponRows"":[{strRows}],""bets"":[{{""couponRowIndexes"":[{string.Join(",", Enumerable.Range(0, markets.Count))}],""eachWay"":false}}],""isUserLoggedIn"":true}}";
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return ret;
        }
        public string GetCouponJsonContent(List<OpenBet_Unibet> markets, string stake)
        {
            string ret = "";
            try
            {
                if (markets == null || markets.Count == 0)
                    return ret;

                string strRows = string.Join(",", markets.Select((o, i) => $@"{{""index"":{i},""odds"":{o.odds},""outcomeId"":{o.marketId},""type"":""SIMPLE""}}"));
                string strBets = $@"{{""couponRowIndexes"":[{string.Join(",", Enumerable.Range(0, markets.Count))}],""eachWay"":false,""stake"":{stake}}}";
                string strOutcomes = string.Join(",", markets.Select(o => $@"{{""id"":{o.marketId},""outcomeId"":{o.marketId},""betofferId"":{o.offerId},""eventId"":{o.eventId},""approvedOdds"":{o.odds},""isLiveBetoffer"":false,""isPrematchBetoffer"":true,""fromBetBuilder"":false,""oddsApproved"":true,""eachWayApproved"":true,""source"":""Widget"",""betslipLocationSource"":""Home"",""isGameParlayOutcome"":false,""fromPrePack"":false}}"));
                ret = $@"{{""couponRows"":[{strRows}],""bets"":[{strBets}],""allowOddsChange"":""NO"",""channel"":""WEB"",""trackingData"":{{""hasTeaser"":false,""isBetBuilderCombination"":false,""selectedOutcomes"":[{strOutcomes}],""reward"":{{}},""isMultiBuilder"":false,""isPrePackCombination"":false}},""requestId"":""{Guid.NewGuid().ToString().ToLower()}""}}";
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return ret;
        }
        public bool ChangeMarketOdds(string resultBettingStr, List<OpenBet_Unibet> markets)
        {
            try
            {
                dynamic retObj = JsonConvert.DeserializeObject<dynamic>(resultBettingStr);
                if (retObj.status == "FAIL")
                {
                    if (retObj.couponRowErrors != null && retObj.couponRowErrors.Count > 0)
                    {
                        foreach (dynamic rowErr in retObj.couponRowErrors)
                        {
                            int index = Utils.parseToInt(rowErr.couponRowIndex.ToString());
                            dynamic detail = rowErr.errors;
                            if (detail != null || detail.Count > 0)
                            {
                                foreach (dynamic errOne in detail)
                                {
                                    string errorType = errOne.type;
                                    if (errorType == "ODDS_CHANGED" && errOne.arguments != null && errOne.arguments.Count > 0)
                                    {
                                        string changeOdd = errOne.arguments[0].value;
                                        if (!string.IsNullOrEmpty(changeOdd))
                                        {
                                            double origVal = Utils.ParseToDouble(markets[index].odds);
                                            double newVal = Utils.ParseToDouble(changeOdd);
                                            if (origVal > newVal)
                                            {
                                                LogMng.Instance.onWriteStatus($"Odd moved down {markets[index].odds}-{changeOdd}");

                                                bool bAcceptable = false;
                                                if (1000 <= origVal && origVal < 2000)
                                                {//- If the odds are between 1.0 and 2.0 then 0.01 lower odds is acceptable.
                                                    if (origVal - newVal <= 10)
                                                        bAcceptable = true;
                                                }
                                                else if (2000 <= origVal && origVal < 3000)
                                                {//- If the odds are between 2.0 and 3.0 then 0.02 lower odds is acceptable.
                                                    if (origVal - newVal <= 20)
                                                        bAcceptable = true;
                                                }
                                                else if (3000 <= origVal && origVal < 4000)
                                                {//- If the odds are between 3.0 and 4.0 then 0.05 lower odds is acceptable.
                                                    if (origVal - newVal <= 50)
                                                        bAcceptable = true;
                                                }

                                                if (!bAcceptable)
                                                {
                                                    return false;
                                                }
                                                else
                                                {
                                                    LogMng.Instance.onWriteStatus($"Odd down {origVal - newVal} is acceptable");
                                                }

                                            }

                                            LogMng.Instance.onWriteStatus($"Odd is Changed {markets[index].odds}-{changeOdd}");
                                            markets[index].odds = changeOdd;
                                        }
                                        //Trace.WriteLine($"Odds Changed - index-{index}, odd-{changeOdd}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return true;
        }


    }
#endif
}
