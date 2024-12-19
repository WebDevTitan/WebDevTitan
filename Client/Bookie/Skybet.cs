namespace Project.Bookie
{
#if (SKYBET)
    class SkybetCtrl : IBookieController
    {
        public HttpClient m_client = null;
        public string tinysessid = "";

        public SkybetCtrl()
        {
            m_client = initHttpClient();
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
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html, application/xhtml+xml, application/xml; q=0.9, image/webp, */*; q=0.8");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;

            return httpClientEx;
        }


        public bool login()
        {
            //Global.SetMonitorVisible(true);
            bool bLogin = false;
            try
            {
                
                Global.OpenUrl("https://www.skybet.com/secure/identity/m/login/mskybet");
                Thread.Sleep(3000);

                string command = $"var t={{PasswordShowhide: false,pin: '{Setting.Instance.password}',username: '{Setting.Instance.username}'}};";
                Global.RunScriptCode(command);

                Thread.Sleep(1000);

                //fetch("https://www.skybet.com/secure/identity/m/login/mskybet", {
                //    method: "POST",
                //            headers:
                //    {
                //        "Content-Type": "application/json",
                //                Accept: "application/json",
                //                "X-Requested-With": "XMLHttpRequest"
                //            },
                //            credentials: "same-origin",
                //            body: JSON.stringify(t)
                //        });

                command = $"fetch('https://www.skybet.com/secure/identity/m/login/mskybet', {{method: 'POST',headers:{{'Content-Type': 'application/json',Accept: 'application/json','X-Requested-With': 'XMLHttpRequest'}},credentials: 'same-origin',body: JSON.stringify(t)}});";
                Global.RunScriptCode(command);

                Global.OpenUrl("https://www.skybet.com");

                Thread.Sleep(5000);

                string balanceElement = Global.GetStatusValue("return document.getElementById('js-balance-visibility').outerHTML").ToLower();
                if (!balanceElement.Contains("class"))
                {
                    return false;
                }

                Task.Run(async () => await Global.GetCookie("https://www.skybet.com")).Wait();
                Task.Run(async () => await Global.GetCookie("https://m.skybet.com")).Wait();
                Thread.Sleep(2000);
                foreach (Cookie cookie in Global.cookieContainer.GetCookies(new Uri("https://m.skybet.com")))
                {
                    try
                    {
                        if (cookie.Name == "TINYSESSID")
                        {
                            tinysessid = cookie.Value;
                            break;
                        }                        
                    }catch{}
                }
                bLogin = true;
            }
            catch (Exception e)
            {

            }
            //Global.SetMonitorVisible(false);
            return bLogin;
        }

        public bool doLogin()
        {
            try
            {
                HttpResponseMessage resopnseMessage = m_client.GetAsync("https://www.skybet.com/").Result;
                resopnseMessage.EnsureSuccessStatusCode();

                HttpResponseMessage configResponseMessage = m_client.GetAsync("https://static.skybetservices.com/bannering/www.skybet.com/login/mskybet/config.json").Result;
                configResponseMessage.EnsureSuccessStatusCode();

                HttpResponseMessage response = m_client.GetAsync("https://www.skybet.com/secure/identity/m/login-state/mskybet?urlconsumer=https://m.skybet.com&dl=1&ssoTransferToken=").Result;
                response.EnsureSuccessStatusCode();

                response = m_client.GetAsync("https://www.skybet.com/secure/identity/m/login/mskybet?urlconsumer=https://m.skybet.com&dl=1").Result;
                response.EnsureSuccessStatusCode();

                string content = response.Content.ReadAsStringAsync().Result;
                if (content.ToLower().Contains(Setting.Instance.username.ToLower()))
                    return true;

                m_client.DefaultRequestHeaders.Remove("Accept");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                m_client.DefaultRequestHeaders.Referrer = new Uri("https://www.skybet.com/secure/identity/m/login/mskybet?urlconsumer=https://m.skybet.com&dl=1");

                StringContent authPostContent = new StringContent(string.Format("{{\"username\":\"{0}\",\"pin\":\"{1}\" ,\"rememberUsername\":true}}", Setting.Instance.username, Setting.Instance.password), Encoding.UTF8, "application/json");
                authPostContent.Headers.ContentType.CharSet = string.Empty;

                HttpResponseMessage identMessageresponse = m_client.PostAsync("https://www.skybet.com/secure/identity/m/login/mskybet", authPostContent).Result;
                identMessageresponse.EnsureSuccessStatusCode();

                string identContent = identMessageresponse.Content.ReadAsStringAsync().Result;
                JObject jsonObj = JObject.Parse(identContent);

                string token = jsonObj["user_data"]["token"].ToString();

                HttpResponseMessage responseLogin = m_client.PostAsync("https://m.skybet.com/auth/login", (HttpContent)new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)new KeyValuePair<string, string>[2]{
                    new KeyValuePair<string, string>("currentPath", "/"),
                    new KeyValuePair<string, string>("token",token)
                })).Result;

                responseLogin.EnsureSuccessStatusCode();

                string loginContent = responseLogin.Content.ReadAsStringAsync().Result;


                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public double getBalance()
        {
            double balance = 0;
            try
            {
                HttpResponseMessage balanceResponseMessage = m_client.PostAsync("https://m.skybet.com/session/refresh-balance", new StringContent("")).Result;

                balanceResponseMessage.EnsureSuccessStatusCode();

                string balanceResponseContent = balanceResponseMessage.Content.ReadAsStringAsync().Result;
                dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(balanceResponseContent);
                balance = Utils.ParseToDouble(jsonContent.cash.ToString());


            }
            catch (Exception e)
            {

            }
            return balance;
        }
        Random rand = new Random();
        public int generateSelectionId()
        {
            return rand.Next(100001, 999999);
        }
        public PROCESS_RESULT PlaceBet(BetburgerInfo info)
        {
            try
            {
                string originMarketName = info.direct_link.Split('|')[0].Trim();
                string originOdds = info.direct_link.Split('|')[1].Trim();

                m_client.DefaultRequestHeaders.Remove("Accept");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                m_client.DefaultRequestHeaders.Referrer = new Uri("https://m.skybet.com/");

                m_client.DefaultRequestHeaders.Remove("Sec-Fetch-Dest");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
                m_client.DefaultRequestHeaders.Remove("Sec-Fetch-Mode");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
                m_client.DefaultRequestHeaders.Remove("Sec-Fetch-Site");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-site");


                m_client.DefaultRequestHeaders.Remove("X-Consumer-Id");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-Consumer-Id", "skybetweb");

                m_client.DefaultRequestHeaders.Remove("x-tinysessid");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("x-tinysessid", tinysessid);

                string param = string.Format("{{\"selections\":{{\"{0}\":{{\"selectionId\":{0},\"type\":\"ST\",\"outcomeIds\":[{1}]}}}}}}", generateSelectionId(), originMarketName);
                StringContent content = new StringContent(param);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string betSlipLink = "https://placement.api.skybet.com/v1/get-opportunities";

                int kk = 0;
                string placeParam = "";
                while (kk < 2)
                {
                    HttpResponseMessage betSlipResponseMessage = m_client.PostAsync(betSlipLink, content).Result;

                    betSlipResponseMessage.EnsureSuccessStatusCode();

                    string betSlipResponseContent = betSlipResponseMessage.Content.ReadAsStringAsync().Result;
                    JObject origObject = JObject.Parse(betSlipResponseContent);

                    try
                    {
                        JObject slipObject = new JObject();

                        JObject selectionsToken = new JObject();
                        foreach (var objProp in origObject.SelectToken("selections"))
                        {
                            JObject betItr = new JObject();
                            JToken obj = objProp.ToObject<JProperty>().Value;
                            betItr.Add("selectionId", obj.SelectToken("selectionId"));
                            betItr.Add("outcomeIds", obj.SelectToken("outcomeIds"));
                            betItr.Add("type", obj.SelectToken("type"));
                            betItr.Add("availablePriceTypes", obj.SelectToken("availablePriceTypes"));
                            betItr.Add("priceType", obj.SelectToken("availablePriceTypes").Children().ToList()[0].ToObject<JValue>().ToString());
                            betItr.Add("excludeFromMultiples", false);

                            selectionsToken.Add(objProp.ToObject<JProperty>().Name, betItr);
                        }
                        slipObject.Add("selections", selectionsToken);

                        JObject opportunitiesToken = new JObject();
                        foreach (var objProp in origObject.SelectToken("opportunities"))
                        {
                            JObject betItr = new JObject();
                            JToken obj = objProp.ToObject<JProperty>().Value;
                            betItr.Add("opportunityId", obj.SelectToken("opportunityId"));

                            double siteMaxstake = obj.SelectToken("maxStake").Value<double>();
                            double siteMinstake = obj.SelectToken("minStake").Value<double>();
                            double sitestakeIncrement = obj.SelectToken("stakeIncrement").Value<double>();
                            string oddString = obj.SelectToken("price").SelectToken("num").Value<string>() + "/" + obj.SelectToken("price").SelectToken("den").Value<string>();
                            double odd = Utils.FractionToDouble(oddString);


                            if (info.odds != odd)
                            {
                                LogMng.Instance.onWriteStatus("Placebet Failed (odd is changed..)");
                                return PROCESS_RESULT.MOVED;
                            }

                            if (siteMaxstake < info.stake)
                            {
                                LogMng.Instance.onWriteStatus("Placebet Failed (Maxstake is too low..)");
                                return PROCESS_RESULT.MOVED;
                            }
                            if (info.stake < siteMinstake)
                            {
                                LogMng.Instance.onWriteStatus("Placebet Failed (MinStake is too high..)");
                                return PROCESS_RESULT.MOVED;
                            }

                            //info.stake = maxstake;
                            betItr.Add("stakePerLine", info.stake);
                            opportunitiesToken.Add(objProp.ToObject<JProperty>().Name, betItr);
                        }
                        slipObject.Add("opportunityOptions", opportunitiesToken);

                        JObject outcomesToken = new JObject();
                        foreach (var objProp in origObject.SelectToken("outcomes"))
                        {
                            JObject betItr = new JObject();
                            JToken obj = objProp.ToObject<JProperty>().Value;
                            betItr.Add("outcomeId", obj.SelectToken("outcomeId"));
                            betItr.Add("price", obj.SelectToken("price"));

                            outcomesToken.Add(objProp.ToObject<JProperty>().Name, betItr);
                        }
                        slipObject.Add("outcomeOptions", outcomesToken);

                        JObject marketsToken = new JObject();
                        foreach (var objProp in origObject.SelectToken("markets"))
                        {
                            JObject betItr = new JObject();
                            JToken obj = objProp.ToObject<JProperty>().Value;
                            betItr.Add("marketId", obj.SelectToken("marketId"));

                            marketsToken.Add(objProp.ToObject<JProperty>().Name, betItr);
                        }
                        slipObject.Add("marketOptions", marketsToken);

                        slipObject.Add("applyBOGIfAvailable", true);

                        placeParam = slipObject.ToString();
                        break;
                    }
                    catch (Exception ex)
                    {
                        LogMng.Instance.onWriteStatus("Adding slip exception: " + ex);
                    }
                }

                randomSleep(2);

                double beforeBalance = getBalance();
                string placebetUrl = "https://placement.api.skybet.com/v1/place-bets";

                content = new StringContent(placeParam);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage placeBetResponse = m_client.PostAsync(placebetUrl, content).Result;

                string strPlacebetInfo = placeBetResponse.Content.ReadAsStringAsync().Result;

                dynamic jsonPlacebetInfo = JsonConvert.DeserializeObject<dynamic>(strPlacebetInfo);

                try
                {
                    double afterBalance = Utils.ParseToDouble(jsonPlacebetInfo.accountBalance.balance.ToString());

                    if (afterBalance < beforeBalance)
                    {                        
                        return PROCESS_RESULT.PLACE_SUCCESS;
                    }
                }
                catch (Exception ex)
                {
                    LogMng.Instance.onWriteStatus(string.Format("Placebet failed ({0}) {1}", strPlacebetInfo, ex));
                    return PROCESS_RESULT.ERROR;
                }

            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus("Placebet exception: " + e);
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
