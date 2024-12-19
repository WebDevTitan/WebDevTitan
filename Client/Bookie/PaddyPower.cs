namespace Project.Bookie
{
#if (PADDYPOWER || BETFAIR_NEW)

    public class BalanceDetail
    {
        public double amount { get; set; }
        public double availabletobet { get; set; }
    }
    public class BalanceJson
    {
        public string status { get; set; }
        public string walletName { get; set; }

        public BalanceDetail details { get; set; }
    }
    class PaddyPowerCtrl : IBookieController
    {
        Dictionary<string, string> configUrls = new Dictionary<string, string>()
        {
            {"baseURL", "https://www.{0}" },
            {"implybetURL", "https://sib.{0}/www/sports/fixedodds/transactional/v1/implyBets" },
            {"placebetURL", "https://spb.{0}/www/sports/fixedodds/transactional/v1/placeBet" },
            {"homePageURL", "https://www.{0}/sport" },
            {"loginURL", "https://identitysso.{0}/view/login" },
            {"balanceURL", "https://was.{0}/wallet-service/v3.0/wallets" },
            {"myActivityURL", "https://myactivity.{0}/" },
            {"betHistoryURL", "https://myactivity.{0}/activity/sportsbook" },
            {"accountSummaryURL", "https://myaccount.{0}/summary/accountsummary" }
        };

        public HttpClient m_client = null;
        public string appKey = "";

        Object lockerObj = new object();
        public PaddyPowerCtrl()
        {
            m_client = initHttpClient();
        }

        public bool logout()
        {
            return false;
        }

        public void Feature()
        {
            
        }

        public void Close()
        {

        }

        public int GetPendingbets()
        {
            return 1;
        }

        public bool Pulse()
        {
            Global.RefreshPage();

            if (getBalance() < 1)
                return false;
            return true;
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

            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;

            return httpClientEx;
        }

        public void ExitSelenium()
        {
            try
            {
             
                Thread.Sleep(5000);
            }
            catch (Exception)
            { }
        }

        public bool login()
        {
            logout();
            return InnerLogin();
        }
        public bool InnerLogin(bool bRemovebet = true)
        {
            Global.SetMonitorVisible(true);
            bool bLogin = false;
            try
            {
                try
                {
                    Monitor.Enter(lockerObj);


                    LogMng.Instance.onWriteStatus($"Betfair login Start");
                    Global.OpenUrl($"https://www.{Setting.Instance.domain}/sport/");


                    Thread.Sleep(3000);
                    //LogMng.Instance.onWriteStatus($"login step 1");
                    Global.RunScriptCode("document.getElementById('onetrust-accept-btn-handler').click();");

                    //LogMng.Instance.onWriteStatus($"login step 2 {eleLoginBtn}");

                    Thread.Sleep(1000);

                    Global.RunScriptCode($"document.getElementById('ssc-liu').value='{Setting.Instance.username}';");

                    Global.RunScriptCode($"document.getElementById('ssc-lipw').value='{Setting.Instance.password}';");

                    Thread.Sleep(500);

                    Global.RunScriptCode("document.getElementById('ssc-lis').click();");
                    
                    
                    //for another page

                    Global.RunScriptCode($"document.getElementById('username').value='{Setting.Instance.username}';");

                    Global.RunScriptCode($"document.getElementById('password').value='{Setting.Instance.password}';");

                    Thread.Sleep(500);

                    Global.RunScriptCode("document.getElementById('login').click();");


                    int nRetryCheck = 0;
                    while (nRetryCheck++ < 3)
                    {
                        Thread.Sleep(4000);

                        if (getBalance() < 0)
                        {
                            bLogin = false;
                        }
                        else
                        {
                            Task.Run(async () => await Global.GetCookie($"https://www.{Setting.Instance.domain}")).Wait();
                            if (bRemovebet)
                            {
                                removeBet();

                            }

                            string pageSource = Global.GetPageSource().Result;
                            appKey = Utils.Between(pageSource, "appkey\\\":\\\"", "\\\"");
                            bLogin = true;
                            break;
                        }
                    }
                }
                catch { }
                finally
                {
                    Monitor.Exit(lockerObj);
                }
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus($"login exception {e.StackTrace} {e.Message}");
            }
            //Global.SetMonitorVisible(false);

            if (bLogin)
                LogMng.Instance.onWriteStatus($"betfair login Successed");
            else
                LogMng.Instance.onWriteStatus($"betfair login Failed");
            return bLogin;
        }

        private string getSecurityToken()
        {
            try
            {
                foreach (Cookie cookieData in Global.cookieContainer.GetCookies(new Uri($"https://www.{Setting.Instance.domain}")))
                {
                    if (cookieData.Name == "xsrftoken")
                    {
                        return cookieData.Value;
                    }
                }
                LogMng.Instance.onWriteStatus("[getSecurityToken] Security token not found");
                return null;
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus("getSecurityToken exception");
            }
            return null;
        }

        public void removeBet()
        {
            int nRetry = 0;

            string homePageURL = string.Format(configUrls["homePageURL"], Setting.Instance.domain);
            Dictionary<string, string> queryString = new Dictionary<string, string>(){
                {"action", "removeAll" },
                {"modules", "betslip" },
                {"bsContext", "REAL" },
                {"xsrftoken", getSecurityToken()},
                {"isAjax", "true" },
                {"alt", "json" },
                {"ts", Utils.getTick().ToString() },
            };


            List<string> qs = new List<string>();
            foreach (KeyValuePair<string, string> pair in queryString)
            {
                qs.Add($"{pair.Key}={pair.Value}");
            }
            string removeEndpointURL = $"{homePageURL}/?{string.Join("&", qs)}";

            string functionString = $"window.fetch('{removeEndpointURL}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', method: 'GET' }}).then(response => response.json());";

            //int nRetryButtonCount = 0;
            //nRetryButtonCount = Utils.parseToInt(Global.GetStatusValue("return document.querySelectorAll('a[data-betslip-action=\"remove-all\"][class=\"remove-all-bets ui-betslip-action\"]').length;"));
            //if (nRetryButtonCount <= 0)
            //    break;
            Global.RunScriptCode(functionString);

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"RemoveBet Request: {functionString}");
#endif

        }

        public PROCESS_RESULT PlaceBet(ref BetburgerInfo info)
        {
            OpenBet_Betfair openbet = Utils.ConvertBetburgerPick2OpenBet_Betfair(info);

            

            int retryCount = 2;
#if OXYLABS
            retryCount = 5;
#endif

            while (--retryCount >= 0)
            {
                if (string.IsNullOrEmpty(appKey))
                {
                    login();
                    continue;
                }
                try
                {
                    
                    string formDataString = $"{{\"betLegs\":[{{\"legType\":\"SIMPLE_SELECTION\",\"betRunners\":[{{\"runner\":{{\"marketId\":\"{openbet.bsmId}\",\"selectionId\":{openbet.bssId}}}}}]}}]}}";

                    string implybetURL = $"{string.Format(configUrls["implybetURL"], Setting.Instance.domain)}?_ak={appKey}";
                    string baseURL = $"{string.Format(configUrls["baseURL"], Setting.Instance.domain)}";                    
                    string functionString = $"window.fetch('{implybetURL}', {{ headers: {{ accept: 'application/json, text/plain, */*', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST', referrer: '{baseURL}' }}).then(response => response.json());";

                    Global.strWebResponse1 = "";
                    Global.waitResponseEvent1.Reset();

                    Global.RunScriptCode(functionString);

                    if (!Global.waitResponseEvent1.Wait(10000) || string.IsNullOrEmpty(Global.strWebResponse1))
                    {
                        LogMng.Instance.onWriteStatus("implybet no Result");
                        return PROCESS_RESULT.ERROR;
                    }
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Implybet Res: {Global.strWebResponse1}");
#endif

                    dynamic paramJson = JsonConvert.DeserializeObject(Global.strWebResponse1);
                    if (paramJson == null)
                        return PROCESS_RESULT.SUSPENDED;

                    bool isLivebet = paramJson.runnerOdds[0].inplay;
                    if (isLivebet)
                    {
                        if (!Setting.Instance.bValue1)
                        {
                            LogMng.Instance.onWriteStatus("This is live bet, but live option is unchecked");
                            return PROCESS_RESULT.ERROR;
                        }
                    }
                    else
                    {
                        if (!Setting.Instance.bValue2)
                        {
                            LogMng.Instance.onWriteStatus("This is prematch bet, but prematch option is unchecked");
                            return PROCESS_RESULT.ERROR;
                        }
                    }

                    string betReference = paramJson.betCombinations[0].betReference;
                    double newodd = paramJson.winRunnerOdds[0].odds.trueOdds.decimalOdds.decimalOdds;
                    if (CheckOddDropCancelBet(newodd, info))
                    {

                        LogMng.Instance.onWriteStatus($"Lower odd, cancelled. ({info.odds}) -> ({newodd})");
                        return PROCESS_RESULT.MOVED;
                    }
                    info.odds = newodd;
                    LogMng.Instance.onWriteStatus($"new odd: {info.odds} ");
                    string bEachway = "false";

                    
                    double maxStake = paramJson.betCombinations[0].betMaxStake;
                    if (info.stake <= 0)
                    {
                        LogMng.Instance.onWriteStatus($"Maximum allowed stake is 0, ignore bet..");
                        return PROCESS_RESULT.ERROR;
                    }
                    if (info.stake > maxStake)
                    {
                        LogMng.Instance.onWriteStatus($"Stake({info.stake}) is larger than MaxStake({maxStake}), stake is changed with max Stake");
                        info.stake = maxStake;
                    }
                    int nPlaceRetry = 0;
                    while (nPlaceRetry++ < 2)
                    {
                        string postString = $"{{\"betDefinitions\":[{{\"betNo\":0,\"betType\":\"SINGLE\",\"legs\":[{{\"betRunners\":[{{\"runner\":{{\"marketId\":\"{paramJson.winRunnerOdds[0].runner.marketId}\",\"selectionId\":{paramJson.winRunnerOdds[0].runner.selectionId}}}}}],\"bsp\":false,\"guaranteedPrice\":false,\"isBanker\":false,\"legType\":\"SIMPLE_SELECTION\",\"options\":[],\"winExpectedOdds\":{{\"decimalOdds\":{{\"decimalOdds\":{paramJson.winRunnerOdds[0].odds.trueOdds.decimalOdds.decimalOdds}}},\"fractionalOdds\":{{\"denominator\":{paramJson.winRunnerOdds[0].odds.trueOdds.fractionalOdds.denominator},\"numerator\":{paramJson.winRunnerOdds[0].odds.trueOdds.fractionalOdds.numerator}}}}}}}],\"stakePerLine\":{info.stake},\"takeEachway\":{bEachway},\"operatorOverrides\":[]}}],\"customerRef\":\"{getCustomerRef()}\",\"acceptLowerOdds\":true,\"useAvailableBonus\":false,\"dryRun\":false}}";

                        string placebetURL = $"{string.Format(configUrls["placebetURL"], Setting.Instance.domain)}?_ak={appKey}";

                        functionString = $"window.fetch('{placebetURL}', {{ headers: {{ accept: 'application/json, text/plain, */*', 'accept-language': 'en-US,en;q=0.9', 'content-type': 'application/json' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{postString}', method: 'POST', referrer: '{baseURL}' }}).then(response => response.json());";

                        Global.strWebResponse2 = "";
                        Global.waitResponseEvent2.Reset();

                        Global.RunScriptCode(functionString);

                        if (!Global.waitResponseEvent2.Wait(10000) || string.IsNullOrEmpty(Global.strWebResponse2))
                        {
                            LogMng.Instance.onWriteStatus("Placebet no Result");
                            return PROCESS_RESULT.ERROR;

                        }
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"PlaceBet Res: {Global.strWebResponse2}");
#endif

                        JObject resultJson = JsonConvert.DeserializeObject<JObject>(Global.strWebResponse2);

                        if (resultJson["respCode"] != null)
                        {
                            if (resultJson["respCode"].ToString() == "SUCCESS")
                            {
                                LogMng.Instance.onWriteStatus(string.Format("** PLACE BET SUCCESS"));

                                return PROCESS_RESULT.PLACE_SUCCESS;
                            }
                            else if (resultJson["respCode"].ToString() == "BET_PLACEMENT_FAILURE")
                            {
                                if (resultJson["result"][0]["resultCode"].ToString() == "STAKE_ABOVE_MAXIMUM_ALLOWED")
                                {
                                    info.stake = Utils.ParseToDouble(resultJson["result"][0]["betMaxStake"].ToString());
                                    LogMng.Instance.onWriteStatus($"Max Stake is lower , new stake is {info.stake}");
                                    continue;
                                }
                                else if (resultJson["result"][0]["resultCode"].ToString() == "INVALID_BET_DEFINITION")
                                {                                    
                                    LogMng.Instance.onWriteStatus($"Bet market is suspended");
                                    return PROCESS_RESULT.ERROR;
                                }
                            }
                        }
                        else
                        {
                            if (resultJson["faultstring"] != null && resultJson["faultstring"].ToString() == "DSC-0015")
                                return PROCESS_RESULT.NO_LOGIN;
                        }
                        LogMng.Instance.onWriteStatus($"Unknown Placebet Res: {Global.strWebResponse2}");
                    }                    
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
            LogMng.Instance.onWriteStatus(string.Format("** PLACE BET FAIL"));
            return PROCESS_RESULT.ERROR;
        }

        bool CheckOddDropCancelBet(double newOdd, BetburgerInfo info)
        {
            LogMng.Instance.onWriteStatus($"Odd checking odd in pick: {info.odds} new odd: {newOdd}");
            if (newOdd > Setting.Instance.maxOddsSports || newOdd < Setting.Instance.minOddsSports)
            {
                LogMng.Instance.onWriteStatus($"Ignore bet because of odd is out of range, new odd: {newOdd}");
                return true;
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
            else
            {
                if (newOdd < info.odds)
                {
                    LogMng.Instance.onWriteStatus($"Ignore bet because of odd is dropped {info.odds} -> {newOdd}");
                    return true;
                }
            }
            return false;
        }

        public double getBalance()
        {
            double balance = -1;
            try
            {
                Dictionary<string, string> queryString = new Dictionary<string, string>(){
                    {"walletNames", "[MAIN,SPORTSBOOK_BONUS,BOOST_TOKENS]" },
                    {"alt", "json" }
                };
                if (Setting.Instance.domain.Contains(".it"))
                {
                    queryString = new Dictionary<string, string>(){
                    {"walletNames", "[ITA,SPORTSBOOK_BONUS,BOOST_TOKENS]" },
                    {"alt", "json" }
                };
                }
                List<string> qs = new List<string>();
                foreach (KeyValuePair<string, string> pair in queryString)
                {
                    qs.Add($"{pair.Key}={pair.Value}");
                }

                string getBalanceURL = $"{string.Format(configUrls["balanceURL"], Setting.Instance.domain)}?{string.Join("&", qs)}";
                string baseURL = $"{string.Format(configUrls["baseURL"], Setting.Instance.domain)}";
                string functionString = $"window.fetch('{getBalanceURL}', {{ method: 'GET', headers: {{ 'accept': '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json', }}, credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', referrer: '{baseURL}' }}).then(response => response.json());";

                Global.strAddBetResult = "";
                Global.waitResponseEvent.Reset();

                Global.RunScriptCode(functionString);

                if (Global.waitResponseEvent.Wait(3000))
                {
                    List<BalanceJson> details = new List<BalanceJson>();
                    details = JsonConvert.DeserializeObject<List<BalanceJson>>(Global.strAddBetResult);

                    if (Setting.Instance.domain.Contains(".it"))
                    {
                        BalanceJson mainJson = details.FirstOrDefault(node => node.walletName == "ITA");
                        if (mainJson == null)
                            return balance;

                        balance = mainJson.details.availabletobet;
                    }
                    else
                    {
                        BalanceJson mainJson = details.FirstOrDefault(node => node.walletName == "MAIN");
                        if (mainJson == null)
                            return balance;

                        balance = mainJson.details.amount;
                    }
                }
            }
            catch (Exception e)
            {

            }
            Global.balance = balance;
            return balance;
        }

        public string RandomString(int length)
        {
            Random r = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[r.Next(s.Length)]).ToArray());
        }
        public string getCustomerRef()
        {
            return string.Format("{0}0{1}", RandomString(12), Utils.getTick().ToString());
        }
    }    
#endif
}
