namespace Project.Bookie
{
#if (LEOVEGAS)
    class LeovegasCtrl : IBookieController
    {
        string domain = "https://www.leovegas.it";
        public HttpClient m_client = null;
        public HttpClient m_kambiCdnclient = null;
        ChromeDriver driver = null;

        Object lockerObj = new object();
        //private static ProxyServer _proxyServer = null;
        private string userID = "";
        private string sportsAuthToken = "";

        private string tokenOfKambi = "";        
        public LeovegasCtrl()
        {
            m_client = initHttpClient();
            m_kambiCdnclient = new HttpClient();
            SetAdditionalHeaders(m_kambiCdnclient);
        }

        public bool Pulse()
        {
            return false;           
        }
        private void InitSelenium()
        {

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            //service.LogPath = "chromedriver.log";
            //service.EnableVerboseLogging = true;

            ChromeOptions option = new ChromeOptions
            {
                PageLoadStrategy = PageLoadStrategy.Normal,
            };

            //OpenQA.Selenium.Proxy proxy = new OpenQA.Selenium.Proxy();
            //proxy.Kind = ProxyKind.Manual;
            //proxy.IsAutoDetect = false;
            //proxy.HttpProxy = "http://localhost:18882";
            //proxy.SslProxy = "http://localhost:18882";
            //option.Proxy = proxy;

            driver = new ChromeDriver(service, option, TimeSpan.FromSeconds(180));
        }
        
        private IWebElement getElementBy(IWebDriver driver, By by)
        {
            try
            {
                IWebElement element = driver.FindElement(by);
                return element;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public void ExitSelenium()
        {
            try
            {
                driver.Close();
                driver.Quit();
                Thread.Sleep(5000);
            }
            catch (Exception)
            { }
        }

        public bool login()
        {
            bool bLogin = false;
            try
            {

                lock (lockerObj)
                {
                    m_client = initHttpClient();
                    m_kambiCdnclient = new HttpClient();
                    SetAdditionalHeaders(m_kambiCdnclient);
                    InitSelenium();

                    LogMng.Instance.onWriteStatus($"Leovegas login Start");
                    driver.Navigate().GoToUrl("https://www.leovegas.it/it-it/");

                    string pageSource = driver.PageSource;

                    Thread.Sleep(1000);

                    IWebElement usernameEle = getElementBy(driver, By.CssSelector("input[name='username']"));
                    //LogMng.Instance.onWriteStatus($"login step 3 {usernameEle}");
                    usernameEle.SendKeys(Setting.Instance.username);

                    IWebElement passEle = getElementBy(driver, By.CssSelector("input[name='password']"));
                    //LogMng.Instance.onWriteStatus($"login step 4 {passEle}");

                    passEle.SendKeys(Setting.Instance.password);
                    Thread.Sleep(500);

                    IWebElement eleLoginOKBtn = driver.FindElementByXPath("//button[@data-test-id='navbar-login-form-login-button']");
                    //LogMng.Instance.onWriteStatus($"login step 5 {eleLoginOKBtn}");
                    eleLoginOKBtn.Click();

                    Thread.Sleep(6000);


                    var _cookies = driver.Manage().Cookies.AllCookies;
                    System.Net.CookieContainer _container = new System.Net.CookieContainer();
                    //LogMng.Instance.onWriteStatus($"login step 6 cookies count {_cookies.Count}");
                    foreach (OpenQA.Selenium.Cookie cookie in _cookies)
                    {
                        try
                        {
                            //LogMng.Instance.onWriteStatus($"login step 7 cookie {cookie.Domain} {cookie.Name} {cookie.Value}");

                            if (cookie.Domain.ToLower().Contains("leovegas.it"))
                            {
                                //Global.cookieContainer.Add(new Uri("https://www.leovegas.it"), new System.Net.Cookie(cookie.Name, cookie.Value));
                                Global.cookieContainer.Add(new Uri("https://www.leovegas.it"), new System.Net.Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
                            }
                            else
                            {
                                int i = 0;
                                i++;
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    bLogin = true;
                }                
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus($"login exception {e.StackTrace} {e.Message}");
            }
            ExitSelenium();

            string test = "{\"operationName\":\"SportsAuthTokenMutation\",\"query\":\"mutation SportsAuthTokenMutation {\\n sportsAuthToken\\n}\\n\",\"variables\":{}}";
            var postData = new StringContent(test, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = m_client.PostAsync(domain + "/api?relay", postData).Result;
                response.EnsureSuccessStatusCode();

                string content = response.Content.ReadAsStringAsync().Result;
                LogMng.Instance.onWriteStatus("getSportsAuthToken " + content);
                dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(content);
                sportsAuthToken = jsonContent.data.sportsAuthToken.ToString();
            }
            catch (Exception ex)
            {
            };
            
            
            if (getBalance() < 0)
                return false;


            string fingerprint = Utils.RandomHexString(32);
            string param = string.Format("{{\"punterId\":\"{0}\",\"ticket\":\"{1}\",\"requestStreaming\":true,\"channel\":\"WEB\",\"market\":\"IT\",\"sessionAttributes\":{{\"fingerprintHash\":\"{2}\"}}}}", userID, sportsAuthToken, fingerprint);
            postData = new StringContent(param, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = m_kambiCdnclient.PostAsync(string.Format("https://mt-auth.kambicdn.org/player/api/v2019/leoit/punter/login.json?market=IT&lang=it_IT&channel_id=1&client_id=2&ncid={0}&settings=true", Utils.getTick()), postData).Result;
                response.EnsureSuccessStatusCode();

                string content = response.Content.ReadAsStringAsync().Result;
                dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(content);
                tokenOfKambi = jsonContent.token.ToString();
            }
            catch (Exception ex)
            {

            }
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

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            SetAdditionalHeaders(httpClientEx);
            return httpClientEx;
        }

        public void SetAdditionalHeaders(HttpClient httpClientEx)
        {
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;
            
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "it-IT,it;q=0.9,en-US;q=0.8,en;q=0.7");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.142 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

            
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"90\", \"Google Chrome\";v=\"90\"");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
        }

        public PROCESS_RESULT PlaceBet(BetburgerInfo info)
        {
            OpenBet_Leovegas openbet = Utils.ConvertBetburgerPick2OpenBet_Leovegas(info);

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
                    string odds = "";
                    string outcomeId = "";
                    string betofferId = "";
                    string eventId = "";                                        
                    string id = "";
                    

                    

                    HttpResponseMessage getAllLiveMatches = m_client.GetAsync(string.Format("https://eu-offering.kambicdn.org/offering/v2018/leoit/betoffer/outcome.json?lang=it_IT&market=IT&client_id=2&channel_id=1&ncid={0}&id={1}", Utils.getTick(), openbet.outcomeId)).Result;
                    var strContent = getAllLiveMatches.Content.ReadAsStringAsync().Result;
                    //LogMng.Instance.onWriteStatus("all live sports--");
                    //LogMng.Instance.onWriteStatus(strContent);
                    if (strContent.Contains("error"))
                    {
                        LogMng.Instance.onWriteStatus(string.Format("Market is not available anymore"));
                        return PROCESS_RESULT.ERROR;
                    }
                    JObject origObject = JObject.Parse(strContent);

                    string EventdetailUrl = string.Empty;
                    foreach (var objEvent in origObject["betOffers"])
                    {
                        if (objEvent["id"].ToString() == openbet.betOfferId)
                        {
                            eventId = objEvent["eventId"].ToString();

                            foreach (var objParticipant in objEvent["outcomes"])
                            {
                                if (openbet.outcomeId == objParticipant["id"].ToString())
                                {
                                    odds = objParticipant["odds"].ToString();
                                    outcomeId = openbet.outcomeId;
                                    betofferId = objParticipant["betOfferId"].ToString();
                                    id = objParticipant["id"].ToString();
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(odds))
                    {
                        LogMng.Instance.onWriteStatus(string.Format("Market is not available anymore"));
                        return PROCESS_RESULT.ERROR;
                    }

                    double CurOdd = Utils.ParseToDouble(odds) / 1000;
                    if (CurOdd < info.odds)
                    {
                        LogMng.Instance.onWriteStatus(string.Format("Odd is changed from {0} To {1}", info.odds, odds));
                        return PROCESS_RESULT.MOVED;
                    }
                    string ReqJson = $"{{\"couponRows\":[{{\"index\":0,\"odds\":{odds},\"outcomeId\":{outcomeId},\"type\":\"SIMPLE\"}}],\"bets\":[{{\"couponRowIndexes\":[0],\"eachWay\":false,\"stake\":{info.stake * 1000}}}],\"allowOddsChange\":\"HIGHER\",\"channel\":\"WEB\",\"trackingData\":{{\"hasTeaser\":false,\"isBetBuilderCombination\":false,\"selectedOutcomes\":[{{\"id\":{id},\"outcomeId\":{outcomeId},\"betofferId\":{betofferId},\"eventId\":{eventId},\"approvedOdds\":{odds},\"isLiveBetoffer\":true,\"isPrematchBetoffer\":true,\"fromBetBuilder\":false,\"oddsApproved\":true,\"eachWayApproved\":true,\"source\":\"Widget\",\"isGameParlayOutcome\":false,\"fromPrePack\":false}}],\"reward\":{{}},\"isMultiBuilder\":false,\"isPrePackCombination\":false}},\"requestId\":\"{Guid.NewGuid()}\"}}";
                    //LogMng.Instance.onWriteStatus($"bet Req : {ReqJson}");
                    while (--retryCount >= 0)
                    {
                        try
                        {
                            string betUrl = $"https://mt-auth.kambicdn.org/player/api/v2019/leoit/coupon.json?lang=it_IT&market=IT&client_id=2&channel_id=1&ncid={Utils.getTick()}";

                            int subRetryCount = 6;
                            string strBetResp = string.Empty;

                            while (--subRetryCount > 0)
                            {
                                try
                                {
                                    var postData = new StringContent(ReqJson, Encoding.UTF8, "application/json");

                                    m_client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", string.Format("Bearer {0}", tokenOfKambi));
                                    HttpResponseMessage betResponse = m_client.PostAsync(betUrl, postData).Result;
                                    strBetResp = betResponse.Content.ReadAsStringAsync().Result;
                                    LogMng.Instance.onWriteStatus(string.Format("BetRes: {0}", strBetResp));
                                    break;
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                            //processResponse.betResult = strBetResp;
                            dynamic jsonBetResp = JsonConvert.DeserializeObject<dynamic>(strBetResp);
                            //LogMng.Instance.onWriteStatus("jsonBetResp:" + jsonBetResp.ToString());
                            if (jsonBetResp.status.ToString() == "LIVE_DELAY_PENDING")
                            {
                                return PROCESS_RESULT.PLACE_SUCCESS;                                
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
        public double getBalance()
        {
            double balance = -1;
            try
            {
                string test = "{\"operationName\":\"HistoryTransactionsScreenQuery\",\"query\":\"query HistoryTransactionsScreenQuery(\\n  $type: TransactionType!\\n  $pageSize: Int!\\n  $lastCursor: String\\n) {\\n viewer {\\n...ScreenProtection_viewer\\n user {\\n...LastLogin_user\\n...Transactions_user\\n id\\n    }\\n lobbies {\\n __typename\\n name\\n enabled\\n id\\n    }\\n id\\n  }\\n}\\n\\nfragment ScreenProtection_viewer on Viewer {\\n authenticated\\n}\\n\\nfragment LastLogin_user on User {\\n country {\\n code\\n id\\n  }\\n stats {\\n latestLogin\\n  }\\n}\\n\\nfragment Transactions_user on User {\\n country {\\n code\\n id\\n  }\\n license {\\n name\\n  }\\n...History_user\\n...NetDeposit_user\\n...HistoryTitleAndSelector_user\\n}\\n\\nfragment History_user on User {\\n license {\\n name\\n  }\\n country {\\n code\\n id\\n  }\\n history(type: $type, first: $pageSize, after: $lastCursor) {\\n pageInfo {\\n hasNextPage\\n startCursor\\n endCursor\\n    }\\n edges {\\n cursor\\n node {\\n... on TransactionAccount {\\n id\\n type\\n state\\n status\\n date\\n currency\\n amount\\n fee\\n          event\\n description\\n paymentMethodType\\n regulatoryId\\n operatorUid\\n }\\n...on TransactionCasino {\\n id\\n type\\n state\\n date\\n currency\\n result\\n wager\\n gameName\\n aamsSessionId\\n aamsParticipationId\\n replayLastRoundUrl\\n replayLastRoundHtml\\n brJackpotTransactionType\\n }\\n...on TransactionSport {\\n id\\n type\\n state\\n date\\n currency\\n amount\\n externalType\\n couponId\\n }\\n...on Node {\\n id\\n }\\n __typename\\n    }\\n}\\n  }\\n}\\n\\nfragment NetDeposit_user on User {\\n  currency\\n  stats {\\n    netDeposit\\n  }\\n}\\n\\nfragment HistoryTitleAndSelector_user on User {\\n  country {\\n    code\\n    id\\n  }\\n...LastLogin_user\\n...CurrentBalance_user\\n...CurrentBonus_user\\n...TransactionsSummary_user\\n...HistoryPendingWithdrawal_user\\n}\\n\\nfragment CurrentBalance_user on User {\\n  balance {\\n    amount\\n  }\\n currency\\n}\\n\\nfragment CurrentBonus_user on User {\\n  balance {\\n    totalBonusCasino\\n    totalBonusSport\\n  }\\n country {\\n currency\\n id\\n }\\n}\\n\\nfragment TransactionsSummary_user on User {\\n  country {\\n    currency\\n    code\\n    id\\n  }\\n license {\\n name\\n }\\n userAccountHistory {\\n deposited\\n initialBalance\\n totalBonus\\n wagered\\n wins\\n withdrawn\\n }\\n}\\n\\nfragment HistoryPendingWithdrawal_user on User {\\n  currency\\n  balance {\\n    pendingWithdrawal {\\n      amount\\n    }\\n  }\\n  license {\\n    name\\n  }\\n}\\n\",\"variables\":{ \"type\":\"ACCOUNT\",\"pageSize\":10,\"lastCursor\":null}}";
                var postData = new StringContent(test, Encoding.UTF8, "application/json");
                lock (lockerObj)
                {
                    HttpResponseMessage response = m_client.PostAsync(domain + "/api?relay", postData).Result;
                    response.EnsureSuccessStatusCode();

                    string content = response.Content.ReadAsStringAsync().Result;
                    LogMng.Instance.onWriteStatus("getbalance response " + content);

                    dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(content);
                    //string RealUserBalance = jsonContent[0].ToString();
                    string strbalance = jsonContent.data.viewer.user.balance.amount.ToString();
                    balance = Utils.ParseToDouble(strbalance);

                    byte[] data = Convert.FromBase64String(jsonContent.data.viewer.user.id.ToString());
                    string decodedString = Encoding.UTF8.GetString(data);
                    userID = Utils.Between(decodedString, "User:");
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
