namespace Project.Bookie
{
#if (UNIBET)

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
    public class Balance
    {
        public double bonus { get; set; }
        public double cash { get; set; }
        public double total { get; set; }
        public double credit { get; set; }
        public double grantedCredit { get; set; }
    }
    public class BalanceJson
    {
        [JsonProperty("balance")]
        public Balance balance { get; set; }
    }
    public class JsonLoginInput
    {
        public string brand { get; set; }
        public string captchaResponse { get; set; }
        public string captchaType { get; set; }
        public string channel { get; set; }
        public string client { get; set; }
        public string clientVersion { get; set; }
        public string loginId { get; set; }
        public string loginSecret { get; set; }
        public string platform { get; set; }

        public JsonLoginInput()
        {
            brand = "unibet";
            captchaResponse = "";
            captchaType = "INVISIBLE";
            channel = "WEB";
            client = "polopoly";
            clientVersion = "desktop";
            platform = "desktop";
        }
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

    class UnibetCtrl : IBookieController
    {
        public HttpClient m_client = null;
        public string domain = string.Empty;
        public UnibetSessionInfo apiResultInfo = null;
        
        public Process chromeProcess = null;

        public string market = string.Empty;
        string authToken = string.Empty;
        string customer_id = string.Empty;
        static string FingerPrintHashFileName = "FPHList.dat";

        public List<FingerPrintHash> HashList = new List<FingerPrintHash>();

        bool bIsHashCaptured = false;
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

        public void Feature()
        {

        }

        public bool logout()
        {
            return true;
        }
        public void Close()
        {

        }
        public bool Pulse()
        {
            return false;
        }

        public bool FetchFingerPrintHash()
        {
            bool bResult = false;
            Global.SetMonitorVisible(true);
            LogMng.Instance.onWriteStatus("Please login account in bot browser for fecthing account info.");
            Global.OpenUrl(string.Format("https://www.{0}/", Setting.Instance.domain));
                                   
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

        public UnibetCtrl()
        {
            OpenFingerPrintHashFile();
            if (string.IsNullOrEmpty(GetFingerPrintHash()))
            {
                FetchFingerPrintHash();
            }

            m_client = initHttpClient();

            domain = Setting.Instance.domain;
            if (domain.Contains("unibet-27"))
                domain = $"pl.{Setting.Instance.domain}";
            else if (!domain.Contains("de"))
                domain = $"www.{Setting.Instance.domain}";
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
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html, application/xhtml+xml, application/xml; q=0.9, image/webp, */*; q=0.8");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;

            return httpClientEx;
        }
                
        public bool login()
        {
            if (string.IsNullOrEmpty(GetFingerPrintHash()))
            {
                LogMng.Instance.onWriteStatus("Login unibet manually to capture hash value");
                return false;
            }
            int retryCount = 2;
#if OXYLABS
            retryCount = 5;
#endif

            while (--retryCount >= 0)
            {
                try
                {
                    m_client.DefaultRequestHeaders.Remove("Host");
                    HttpResponseMessage response = m_client.GetAsync(string.Format("https://www.{0}/betting", Setting.Instance.domain)).Result;
                    response.EnsureSuccessStatusCode();

                    string content = response.Content.ReadAsStringAsync().Result;
                    if (content.Contains(Setting.Instance.username))
                        return true;

                    JsonLoginInput input = new JsonLoginInput();
                    input.loginId = Setting.Instance.username;
                    input.loginSecret = Setting.Instance.password;
                    var postData = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
                    string loginLink = string.Format("https://{0}/login-api/methods/password", domain);
                    if (loginLink.Contains("uk") || loginLink.Contains("se"))
                        loginLink = string.Format("https://{0}/login-api/v2/methods/password", domain);

                    HttpResponseMessage loginResponse = m_client.PostAsync(loginLink, postData).Result;
                    loginResponse.EnsureSuccessStatusCode();

                    IEnumerable<string> cookieStrings = loginResponse.Headers.GetValues("Set-Cookie");
                    foreach (string cookieString in cookieStrings)
                    {
                        string[] cookieStringArray = cookieString.Split(new char[] { ';' });
                        if (cookieStringArray == null || cookieStringArray.Length < 1)
                            continue;

                        string[] cookieStringSubArray = cookieStringArray[0].Split(new char[] { '=' }, 2);
                        if (cookieStringSubArray == null || cookieStringArray.Length < 2)
                            continue;

                        if (cookieStringSubArray[0] == "SessionTimeout")
                            continue;

                        Global.cookieContainer.Add(new Uri(string.Format("https://{0}/", domain)), new Cookie(cookieStringSubArray[0], cookieStringSubArray[1]));
                    }
                    string strLoginInfo = loginResponse.Content.ReadAsStringAsync().Result;
                    if (strLoginInfo.Contains("INVALID_CREDENTIALS"))
                        return false;

                    JObject loginObj = JObject.Parse(strLoginInfo);
                    market = loginObj["locale"].ToString();
                    customer_id = loginObj["customerId"].ToString();
                    return true;
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
            try
            {
                if (!GetApiKey())
                    return PROCESS_RESULT.CRITICAL_SITUATION;

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
            while (--retryCount >= 0)
            {
                try
                {
                    string balanceUrl = string.Format("https://{0}/wallitt/mainbalance", domain);
                    HttpResponseMessage resoponse = m_client.GetAsync(balanceUrl).Result;
                    resoponse.EnsureSuccessStatusCode();

                    string result = resoponse.Content.ReadAsStringAsync().Result;
                    BalanceJson json = JsonConvert.DeserializeObject<BalanceJson>(result);
                    balance = json.balance.total;
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

        public bool GetApiKey()
        {
            int nRetry = 2;
            while (nRetry-- >= 0)
            {
                try
                {
                    m_client.DefaultRequestHeaders.Remove("Accept");
                    m_client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/javascript, */*; q=0.01");
                    string tokenApi = string.Empty;
                    if (domain.Contains("uk"))
                        tokenApi = string.Format("https://{0}/kambi-rest-api/gameLauncher2.json?_={1}&useRealMoney=true&brand=unibet&jurisdiction=UK&locale=en_GB&currency=GBP&deviceGroup=desktop&clientId=polopoly_desktop&deviceOs=&marketLocale=en_GB&loadHTML5client=true&enablePoolBetting=false", domain, Utils.getTick().ToString());
                    else if (domain.Contains("pl"))
                        tokenApi = string.Format("https://{0}/kambi-rest-api/gameLauncher2.json?_={1}&useRealMoney=true&brand=unibet&jurisdiction=MT&locale=pl_PL&currency=PLN&deviceGroup=desktop&clientId=polopoly_desktop&deviceOs=&marketLocale=pl_PL&loadHTML5client=true&enablePoolBetting=false", domain, Utils.getTick().ToString()); 
                    else if (domain.Contains("it"))
                        tokenApi = string.Format("https://{0}/kambi-rest-api/gameLauncher2.json?_={1}&useRealMoney=true&brand=unibet&jurisdiction=IT&locale=it_IT&currency=EUR&deviceGroup=desktop&clientId=polopoly_desktop&deviceOs=&marketLocale=it_IT&loadHTML5client=true&enablePoolBetting=false", domain, Utils.getTick().ToString());
                    else if (domain.Contains("se"))
                        tokenApi = string.Format("https://{0}/kambi-rest-api/gameLauncher2.json?_={1}&useRealMoney=true&brand=unibet&jurisdiction=SE&locale=sv_SE&currency=SEK&deviceGroup=desktop&clientId=polopoly_desktop&deviceOs=&marketLocale=sv_SE&loadHTML5client=true&enablePoolBetting=false", domain, Utils.getTick().ToString());
                    else
                        tokenApi = string.Format("https://{0}/kambi-rest-api/gameLauncher2.json?_={1}&useRealMoney=true&brand=unibet&jurisdiction=MT&locale=en_GB&currency=EUR&deviceGroup=desktop&clientId=polopoly_desktop&deviceOs=&marketLocale=en_GB&loadHTML5client=true&enablePoolBetting=false", domain, Utils.getTick().ToString());

                    HttpResponseMessage tokenApiResult = m_client.GetAsync(tokenApi).Result;
                    tokenApiResult.EnsureSuccessStatusCode();
                    AuthTokenJson token = JsonConvert.DeserializeObject<AuthTokenJson>(tokenApiResult.Content.ReadAsStringAsync().Result);

                    string market = "ZZ";
                    if (domain.Contains("pl"))
                        market = "PL";
                    else if (domain.Contains("uk"))
                        market = "GB";
                    else if (domain.Contains("de"))
                        market = "DE";
                    else if (domain.Contains("se"))
                        market = "SE";
                    else if (domain.Contains("it"))
                        market = "IT";

                    StringContent jsonContent = new StringContent("{\"sessionAttributes\":{\"fingerprintHash\":\"" + GetFingerPrintHash() + "\"},\"punterId\":\"" + token.playerId + "\",\"requestStreaming\":true,\"channel\":\"WEB\",\"ticket\":\"" + token.authtoken + "\",\"market\":\"" + market + "\"}", Encoding.UTF8, "application/json");
                    string loginApiLink = string.Empty;
                    if (domain.Contains("uk"))
                        loginApiLink = "https://al-auth.kambicdn.org/player/api/v2019/ubuk/punter/login.json?market=GB&lang=en_GB&channel_id=1&client_id=2&settings=true";
                    else if (domain.Contains("se"))
                        loginApiLink = "https://mt-auth.kambicdn.org/player/api/v2019/ubse/punter/login.json?market=SE&lang=sv_SE&channel_id=1&client_id=2&ncid=" + Utils.getTick().ToString() + "&settings=true";
                    else if (domain.Contains("pl"))
                        loginApiLink = "https://mt-auth.kambicdn.org/player/api/v2/ub/punter/login.json?settings=true&lang=en_GB";
                    else if (domain.Contains("it"))
                        loginApiLink = "https://mt-auth.kambicdn.org/player/api/v2019/ubit/punter/login.json?market=IT&lang=it_IT&channel_id=1&client_id=2&settings=true";
                    else
                        loginApiLink = "https://mt-auth.kambicdn.org/player/api/v2/ub/punter/login.json?market=PL&lang=pl_PL&channel_id=1&client_id=2&ncid=" + Utils.getTick().ToString() + "&settings=true";

                    HttpResponseMessage apiKeyResponse = m_client.PostAsync(loginApiLink, jsonContent).Result;
                    apiKeyResponse.EnsureSuccessStatusCode();
                    string apikeyStr = apiKeyResponse.Content.ReadAsStringAsync().Result;
                    apiResultInfo = JsonConvert.DeserializeObject<UnibetSessionInfo>(apikeyStr);
                    break;
                }
                catch (Exception e)
                {
                    if (!login())
                        return false;
                }
            }
            return true;
        }
        private bool AddBetSlip(List<OpenBet_Unibet> markets)
        {
            long tick = Utils.getTick();

            string marketParam = string.Join("&id=", markets.Select(o => o.marketId));
            string outcomeUrl = $"https://eu-offering.kambicdn.org/offering/v2018/ub/betoffer/outcome.json?lang=en_GB&market=GB&client_id=2&channel_id=1&ncid={tick}&id={marketParam}";
            if (domain.Contains("uk"))
                outcomeUrl = $"https://eu-offering.kambicdn.org/offering/v2018/ubuk/betoffer/outcome.json?lang=en_GB&market=GB&client_id=2&channel_id=1&ncid={tick}&id={marketParam}";
            else if (domain.Contains("se"))
                outcomeUrl = $"https://eu-offering.kambicdn.org/offering/v2018/ubse/betoffer/outcome.json?lang=sv_SE&market=SE&client_id=2&channel_id=1&ncid={tick}&id={marketParam}";
            else if (domain.Contains("pl"))
                outcomeUrl = $"https://eu-offering.kambicdn.org/offering/v2018/ub/betoffer/outcome.json?lang=pl_PL&market=PL&client_id=2&channel_id=1&ncid={tick}&id={marketParam}";
            else if (domain.Contains("it"))
                outcomeUrl = $"https://eu-offering.kambicdn.org/offering/v2018/ubit/betoffer/outcome.json?lang=it_IT&market=IT&client_id=2&channel_id=1&ncid={tick}&id={marketParam}";

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
            m_client.DefaultRequestHeaders.Clear();
            m_client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/javascript, */*; q=0.01");
            m_client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9,ko;q=0.8");
            m_client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
            m_client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", string.Format("https://{0}/betting", domain));
            if (domain.Contains("uk"))
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("Host", "al-auth.kambicdn.org");
            else
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("Host", "mt-auth.kambicdn.org");

            m_client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", string.Format("Bearer {0}", apiResultInfo.token));

            string validateUrl = string.Empty;
            long tick = Utils.getTick();
            if (domain.Contains("uk"))
                validateUrl = $"https://al-auth.kambicdn.org/player/api/v2019/ubuk/coupon/validate.json?lang=en_GB&market=GB&client_id=2&channel_id=1&ncid={tick}";
            else if (domain.Contains("se"))
                validateUrl = $"https://mt-auth.kambicdn.org/player/api/v2019/ubse/coupon/validate.json?lang=sv_SE&market=SE&client_id=2&channel_id=1&ncid={tick}";
            else if (domain.Contains("pl"))
                validateUrl = $"https://mt-auth.kambicdn.org/player/api/v2019/ub/coupon/validate.json?lang=pl_PL&market=PL&client_id=2&channel_id=1&ncid={tick}";
            else if (domain.Contains("it"))
                validateUrl = $"https://mt-auth.kambicdn.org/player/api/v2019/ubit/coupon/validate.json?lang=it_IT&market=IT&client_id=2&channel_id=1";
            else
                validateUrl = $"https://mt-auth.kambicdn.org/player/api/v2019/ub/coupon/validate.json?lang=en_GB&market=ZZ&client_id=2&channel_id=1&ncid={tick}";

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
                string bettingUrl = string.Empty;
                if (domain.Contains("uk"))
                    bettingUrl = "https://al-auth.kambicdn.org/player/api/v2019/ubuk/coupon.json?lang=en_GB&market=GB&client_id=2&channel_id=1&ncid=" + Utils.getTick();
                else if (domain.Contains("se"))
                    bettingUrl = "https://mt-auth.kambicdn.org/player/api/v2019/ubse/coupon.json?lang=sv_SE&market=SE&client_id=2&channel_id=1&ncid=" + Utils.getTick();
                else if (domain.Contains("pl"))
                    bettingUrl = "https://mt-auth.kambicdn.org/player/api/v2019/ub/coupon.json?lang=pl_PL&market=PL&client_id=2&channel_id=1&ncid=" + Utils.getTick();
                else if (domain.Contains("it"))
                    bettingUrl = "https://mt-auth.kambicdn.org/player/api/v2019/ubit/coupon.json?lang=it_IT&market=IT&client_id=2&channel_id=1";
                else
                    bettingUrl = "https://mt-auth.kambicdn.org/player/api/v2019/ub/coupon.json?lang=en_GB&market=ZZ&client_id=2&channel_id=1&ncid=" + Utils.getTick();

                string strContent = GetCouponJsonContent(markets, stakeStr);

                HttpContent jsonContent = new StringContent(strContent, Encoding.UTF8, "application/json");
                HttpResponseMessage resultBetting = m_client.PostAsync(bettingUrl, jsonContent).Result;
                string resultBettingStr = resultBetting.Content.ReadAsStringAsync().Result;
                BetResultJson resultJson = JsonConvert.DeserializeObject<BetResultJson>(resultBettingStr);

                if (resultBettingStr.Contains("SUCCESS") || resultBettingStr.Contains("LIVE_DELAY_PENDING"))
                {
                    if (resultBettingStr.Contains("LIVE_DELAY_PENDING"))
                    {
                        int delayTime = Utils.parseToInt(Regex.Match(resultBettingStr, @"delayBeforeAcceptingBet[^:]*:(?<VAL>[^,]*)").Groups["VAL"].Value);
                        string refId = Regex.Match(resultBettingStr, @"couponRef[^:]*:(?<VAL>[^,]*)").Groups["VAL"].Value;
                        int waitTime = delayTime > 0 ? delayTime : 9;
                        Thread.Sleep(waitTime * 1000);
                        //if (domain.Contains("uk"))
                        //    bettingUrl = string.Format("https://al-auth.kambicdn.org/player/api/v2019/ubuk/coupon/history/{0}.json?lang=en_GB&market=GB&client_id=2&channel_id=1&ncid={1}&liveDelayPoll=true", refId, Utils.getTick());
                        //else if (domain.Contains("se"))
                        //    bettingUrl = string.Format("https://mt-auth.kambicdn.org/player/api/v2019/ubse/coupon/history/{0}.json?lang=sv_SE&market=SE&client_id=2&channel_id=1&ncid={1}&liveDelayPoll=true", refId, Utils.getTick());
                        //else if (domain.Contains("pl"))
                        //    bettingUrl = string.Format("https://mt-auth.kambicdn.org/player/api/v2019/ub/coupon/history/{0}.json?lang=en_GB&market=PL&client_id=2&channel_id=1&ncid={1}&liveDelayPoll=true", refId, Utils.getTick());
                        //else if (domain.Contains("it"))
                        //    bettingUrl = string.Format("https://mt-auth.kambicdn.org/player/api/v2019/ubit/coupon/history/{0}.json?lang=it_IT&market=IT&client_id=2&channel_id=1&ncid={1}&liveDelayPoll=true", refId, Utils.getTick());
                        //else
                        //    bettingUrl = string.Format("https://mt-auth.kambicdn.org/player/api/v2019/ub/coupon/history/{0}.json?lang=en_GB&market=ZZ&client_id=2&channel_id=1&ncid={1}&liveDelayPoll=true", refId, Utils.getTick());

                        //resultBetting = m_client.GetAsync(bettingUrl).Result;

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
                                                {//- If the odds are between 1.0 and 2.0 then 0.02 lower odds is acceptable.
                                                    if (origVal - newVal <= 20)
                                                        bAcceptable = true;
                                                }
                                                else if (2000 <= origVal && origVal < 3000)
                                                {//- If the odds are between 2.0 and 3.0 then 0.05 lower odds is acceptable.
                                                    if (origVal - newVal <= 50)
                                                        bAcceptable = true;
                                                }
                                                else if (3000 <= origVal && origVal < 4000)
                                                {//- If the odds are between 3.0 and 4.0 then 0.1 lower odds is acceptable.
                                                    if (origVal - newVal <= 100)
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
