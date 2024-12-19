namespace Project.Bookie
{
#if (LOTTOMATICA)
    public class LottomaticaCtrl : IBookieController
    {
        public HttpClient m_client = null;
        private string strIdUtente = "";
        public LottomaticaCtrl()
        {
            m_client = initHttpClient();
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
        public bool Pulse()
        {
            return false;
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


            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html, application/xhtml+xml, application/xml; q=0.9, image/webp, */*; q=0.8");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");

            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("context", "2:0:en_US");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"90\", \"Google Chrome\";v=\"90\"");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;

            return httpClientEx;
        }

        public bool logout()
        {
            return true;
        }
        public bool login()
        {
            bool bLogin = false;
            try
            {
                Global.SetMonitorVisible(true);
                Global.OpenUrl("https://www.lottomatica.it/");
                Thread.Sleep(3000);

                Global.RunScriptCode("document.getElementsByClassName('anonymous--login--button')[0].click();");

                Thread.Sleep(500);
                //IWebElement usernameEle = getElementBy(driver, By.CssSelector("input[ng-model='formData.username']"));
                //Global.RunScriptCode($"document.getElementsByName('loginForm')[0][8].value='{Setting.Instance.username}';");
                Global.RunScriptCode($"document.getElementsByName('login_username')[0].value='{Setting.Instance.username}';");
                Global.RunScriptCode($"document.getElementsByName('login_password')[0].value='{Setting.Instance.password}';");

                Thread.Sleep(500);

                Global.strPlaceBetResult = "";
                Global.waitResponseEvent.Reset();

                Global.RunScriptCode($"document.getElementsByClassName('login__panel--login__form--button--login')[0].submit();");


                if (!Global.waitResponseEvent.Wait(10000))
                {
                    throw new Exception();
                }

                dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(Global.strPlaceBetResult);
                
                if (jsonContent.ResultCode.ToString() != "0")
                {
                    LogMng.Instance.onWriteStatus("Login Error: " + jsonContent.Message.ToString());
                    throw new Exception();
                }

                strIdUtente = jsonContent.IdUtente.ToString();
                Global.OpenUrl("https://www.lottomatica.it/scommesse/live");
                Thread.Sleep(5000);

                
                bLogin = true;
            }
            catch (Exception e)
            {

            }

            LogMng.Instance.onWriteStatus($"Login Result: {bLogin}");
            Global.SetMonitorVisible(false);
            return bLogin;
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

                    string getBalanceURL = "https://gb4theplayer.lottomatica.it/api/ContoDiGioco/getbalanceheader";

                    string formDataString = $"{{\"IdCanale\":1,\"IdUtente\":{strIdUtente},\"IpUtente\":null}}";
                    string functionString = $"window.fetch('{getBalanceURL}', {{ headers: {{ accept: 'application/json', 'accept-language': 'en-US,en;q=0.5', 'content-type': 'application/x-www-form-urlencoded' }}, TE: 'trailers', X-Requested-With: 'XMLHttpRequest', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                    Global.strAddBetResult = "";
                    Global.waitResponseEvent.Reset();

                    Global.RunScriptCode(functionString);

                    if (!Global.waitResponseEvent.Wait(3000))
                    {
                        continue;
                    }

                    dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(Global.strAddBetResult);
                    balance = Utils.ParseToDouble(jsonContent.Saldo.ToString()) / 100;

                    break;
                }
                catch (Exception e)
                {
                    LogMng.Instance.onWriteStatus(string.Format("Lottomatica Balance exception: {0} {1}", e.Message, e.StackTrace));
                }
            }

            LogMng.Instance.onWriteStatus(string.Format("Lottomatica Balance: {0}", balance));
            return balance;
        }
        Random rand = new Random();
        public int generateSelectionId()
        {
            return rand.Next(100001, 999999);
        }

        public PROCESS_RESULT PlaceBet(List<BetburgerInfo> info, out List<PROCESS_RESULT> result)
        {
            List<PROCESS_RESULT> betslipCheckResult = new List<PROCESS_RESULT>();
            result = betslipCheckResult;
            return PROCESS_RESULT.ERROR;
        }

        public PROCESS_RESULT PlaceBet(ref BetburgerInfo info)
        {
            try
            {
                string codSelection = info.direct_link.Split('|')[0].Trim();
                string codMarket = info.direct_link.Split('|')[1].Trim();


                //if (info.stake > Global.balance)
                //{
                //    Global.balance = getBalance();
                //    if (info.stake > Global.balance)
                //        return PROCESS_RESULT.SMALL_BALANCE;
                //}
                

                m_client.DefaultRequestHeaders.Referrer = new Uri("https://www.lottomatica.it/scommesse/live");

                string param = string.Format("{{\"stake\":\"{3}\",\"priceFormat\":\"DECIMAL\",\"betObject\":true,\"selectionStakeList\":[],\"selectionStakeSinglesList\":[\"{3}\"],\"context\":{{\"channelId\":\"2\",\"languageCode\":\"en_US\",\"salesPointGroupId\":\"0\",\"version\":\"1\"}},\"selectionList\":[{{\"priceMap\":{{\"DECIMAL\":{{\"priceFormatId\":\"DECIMAL\",\"price\":\"{0}\"}}}},\"status\":\"TRADEABLE\",\"codSelection\":{1},\"idNode\":\"{1}\",\"nodeType\":\"\",\"descr\":\"\",\"codMarket\":{2}}}],\"cud\":true,\"autoRecalculation\":true,\"alias\":true}}", info.odds, codSelection, codMarket, info.stake);
                StringContent content = new StringContent(param);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string betSlipLink = "https://www.lottomatica.it/ww-igateway/ww/api/v1/betslipvalidator";

                int kk = 0;
                string placeParam = "";
                while (kk < 2)
                {
                    HttpResponseMessage betSlipResponseMessage = m_client.PostAsync(betSlipLink, content).Result;

                    betSlipResponseMessage.EnsureSuccessStatusCode();

                    string betSlipResponseContent = betSlipResponseMessage.Content.ReadAsStringAsync().Result;

                    LogMng.Instance.onWriteStatus(string.Format("Betslipvalidator res: {0}", betSlipResponseContent));

                    JObject origObject = JObject.Parse(betSlipResponseContent);

                    try
                    {
                        bool bBetObjectFound = false;
                        JObject slipObject = new JObject();

                        JObject selectionsToken = new JObject();
                        foreach (var objProp in origObject.SelectToken("bets"))
                        {
                            if (objProp.ToObject<JProperty>().Name != "SIMPLE")
                            {
                                continue;
                            }
                            JObject betItr = new JObject();
                            JToken obj = objProp.ToObject<JProperty>().Value;

                            foreach (var SimpleToken in obj)
                            {
                                if (SimpleToken.ToObject<JProperty>().Name == "totalPrice")
                                {//odd checking
                                    double NewOdd = Utils.ParseToDouble(SimpleToken.ToObject<JProperty>().Value.ToString());

                                    if (info.odds > NewOdd)
                                    {
                                        LogMng.Instance.onWriteStatus(string.Format("Odd is changed from {0} To {1}", info.odds, NewOdd.ToString("N2")));
                                        return PROCESS_RESULT.MOVED;
                                    }
                                }
                                else if (SimpleToken.ToObject<JProperty>().Name == "betObject")
                                {
                                    slipObject.Add("bet", SimpleToken.ToObject<JProperty>().Value);
                                    bBetObjectFound = true;
                                    
                                    //JToken betObject = SimpleToken.ToObject<JProperty>().Value;
                                    //double ApprovedStake = Utils.ParseToDouble(betObject.ToObject(JObject)["winStakeAmount"]);
                                }
                            }
                        }

                        if (!bBetObjectFound)
                        {
                            LogMng.Instance.onWriteStatus(string.Format("Market is not available anymore"));
                            return PROCESS_RESULT.MOVED;
                        }


                        //slipObject.Add("opportunityOptions", opportunitiesToken);

                        JObject gtsContext = new JObject();
                        gtsContext.Add("locale", "en_US");
                        slipObject.Add("gtsContext", gtsContext);

                        JObject channelInfo = new JObject();
                        channelInfo.Add("channelId", "2");
                        slipObject.Add("channelInfo", channelInfo);

                        slipObject.Add("currencyId", "EUR");

                        slipObject.Add("deviceIdType", "WINDOWS_WEB");

                        JArray internalInfos = new JArray();

                        JObject internalInfos_0 = new JObject();
                        internalInfos_0.Add("extraInfoId", "section");
                        internalInfos_0.Add("extraInfoValue", "live");
                        internalInfos.Add(internalInfos_0);

                        JObject internalInfos_1 = new JObject();
                        internalInfos_1.Add("extraInfoId", "sessionBrowserId");
                        internalInfos_1.Add("extraInfoValue", Utils.getTick());
                        internalInfos.Add(internalInfos_1);

                        JObject internalInfos_2 = new JObject();
                        internalInfos_2.Add("extraInfoId", "betSlipIdTime");
                        internalInfos_2.Add("extraInfoValue", DateTime.Now.ToString("HH:mm:ss:fff"));
                        internalInfos.Add(internalInfos_2);

                        JObject internalInfos_3 = new JObject();
                        internalInfos_3.Add("extraInfoId", "lastGTSResponse");
                        internalInfos_3.Add("extraInfoValue", "0::::OK");
                        internalInfos.Add(internalInfos_3);

                        JObject internalInfos_4 = new JObject();
                        internalInfos_4.Add("extraInfoId", "userAgent");
                        internalInfos_4.Add("extraInfoValue", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.72 Safari/537.36");
                        internalInfos.Add(internalInfos_4);

                        JObject internalInfos_5 = new JObject();
                        internalInfos_5.Add("extraInfoId", "stackRequest");
                        internalInfos_5.Add("extraInfoValue", string.Format("start-bet: {0}[2],checkUser: {1},getChannelInfo: {2},buy: {3}", DateTime.Now.AddMilliseconds(-2600).ToString("HH:mm:ss:fff"), DateTime.Now.AddMilliseconds(-2599).ToString("HH:mm:ss:fff"), DateTime.Now.AddMilliseconds(-200).ToString("HH:mm:ss:fff"), DateTime.Now.ToString("HH:mm:ss:fff")));
                        internalInfos.Add(internalInfos_5);

                        JObject internalInfos_6 = new JObject();
                        internalInfos_6.Add("extraInfoId", "vSessionId");
                        internalInfos_6.Add("extraInfoValue", origObject["vSessionId"]);
                        internalInfos.Add(internalInfos_6);

                        JObject internalInfos_7 = new JObject();
                        internalInfos_7.Add("extraInfoId", "isRapid");
                        internalInfos_7.Add("extraInfoValue", false);
                        internalInfos.Add(internalInfos_7);

                        slipObject.Add("internalInfos", internalInfos);

                        slipObject.Add("autoReserve", true);

                        slipObject.Add("autoReferral", true);

                        JArray preferences = new JArray();

                        JObject preferences_0 = new JObject();
                        preferences_0.Add("preferenceName", "IsPriceIncreaseReOfferAutomaticallyAcceptedForPreMatch");
                        preferences_0.Add("preferenceValue", "true");
                        preferences.Add(preferences_0);

                        JObject preferences_1 = new JObject();
                        preferences_1.Add("preferenceName", "IsPriceIncreaseReOfferAutomaticallyAcceptedForInRunning");
                        preferences_1.Add("preferenceValue", "true");
                        preferences.Add(preferences_1);

                        JObject preferences_2 = new JObject();
                        preferences_2.Add("preferenceName", "IsPriceReductionReOfferAutomaticallyAcceptedForPreMatch");
                        preferences_2.Add("preferenceValue", "false");
                        preferences.Add(preferences_2);

                        JObject preferences_3 = new JObject();
                        preferences_3.Add("preferenceName", "IsPriceReductionReOfferAutomaticallyAcceptedForInRunning");
                        preferences_3.Add("preferenceValue", "false");
                        preferences.Add(preferences_3);

                        JObject preferences_4 = new JObject();
                        preferences_4.Add("preferenceName", "StakeReofferAllowedByCustomer");
                        preferences_4.Add("preferenceValue", false);
                        preferences.Add(preferences_4);

                        slipObject.Add("preferences", preferences);

                        JArray extraInfos = new JArray();
                        slipObject.Add("extraInfos", extraInfos);

                        placeParam = slipObject.ToString();
                        break;
                    }
                    catch (Exception ex)
                    {
                        LogMng.Instance.onWriteStatus("Adding slip exception: " + ex);
                    }                    
                }

                //randomSleep(2);

                
                string placebetUrl = "https://www.lottomatica.it/ww-adapter/api/v2/gts/bet/place";

                content = new StringContent(placeParam);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage placeBetResponse = m_client.PostAsync(placebetUrl, content).Result;

                string strPlacebetInfo = placeBetResponse.Content.ReadAsStringAsync().Result;

                LogMng.Instance.onWriteStatus(string.Format("betPlace res: {0}", strPlacebetInfo));

                dynamic jsonPlacebetInfo = JsonConvert.DeserializeObject<dynamic>(strPlacebetInfo);

                try
                {
                    int resultCode = Utils.parseToInt(jsonPlacebetInfo.resultCode.ToString());
                    if (resultCode == 0)
                    {
                        if (jsonPlacebetInfo.placeBetResponse.returnCode.ToString() == "OK")
                            return PROCESS_RESULT.PLACE_SUCCESS;
                        //else if (jsonPlacebetInfo.placeBetResponse.returnCode.ToString() == "BET_TO_BE_AUTHORIZED")
                        //    return PROCESS_RESULT.CRITICAL_SITUATION;

                        LogMng.Instance.onWriteStatus(string.Format("Placebet failed ({0} : {1})", jsonPlacebetInfo.placeBetResponse.returnCode.ToString(), jsonPlacebetInfo.placeBetResponse.betStatus.ToString()));
                        return PROCESS_RESULT.ERROR;
                    }
                    else
                    {
                        String reason = jsonPlacebetInfo.resultDescription.ToString();
                        LogMng.Instance.onWriteStatus(string.Format("Placebet failed ({0})", reason));
                        if (reason.Contains("Login Denied"))
                        {
                            return PROCESS_RESULT.NO_LOGIN;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMng.Instance.onWriteStatus(string.Format("Placebet failed ({0})", strPlacebetInfo));
                    return PROCESS_RESULT.ERROR;
                }

            }
            catch (Exception e)
            {

            }
            return PROCESS_RESULT.ERROR;
        }

        protected void randomSleep(int sec)
        {
            Random _rnd = new Random();
            int rand = _rnd.Next(0, 10);
            Thread.Sleep((sec + rand / 10) * 1000);
        }
    }
#endif
}
