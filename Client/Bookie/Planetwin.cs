namespace Project.Bookie
{
#if (PLANETWIN)
    public class PlanetwinCtrl : IBookieController
    {
        public HttpClient m_client = null;
        

        public PlanetwinCtrl()
        {
            m_client = initHttpClient();


        }

        public bool Pulse()
        {
            //Task<HttpResponseMessage> response = null, response1 = null;
            //string content = "";
            //try
            //{
            //    string tick = "0";//Utils.getTick().ToString();
            //    //response = m_client.GetAsync($"https://live.planetwin365.it/api/web/v1/overview/7/1/0").Result;
            //    response = m_client.GetAsync($"https://live.planetwin365.it/api/web/v1/overview/7/1/{tick}");
            //    response1 = m_client.GetAsync($"https://live.planetwin365.it/api/web/v1/menu/7/1/{tick}");
            //    content = response.Result.Content.ReadAsStringAsync().Result;
            //    content = response1.Result.Content.ReadAsStringAsync().Result;
            //}
            //catch { }

            //if (string.IsNullOrEmpty(content))
            //    return true;
            //JObject origObject = JObject.Parse(content);

            //foreach (var objProp in origObject.SelectToken("Events"))
            //{
            //    if (objProp.ToObject<JProperty>().Name != "SIMPLE")
            //    {
            //        continue;
            //    }
            //    JObject betItr = new JObject();
            //    JToken obj = objProp.ToObject<JProperty>().Value;

            //    foreach (var SimpleToken in obj)
            //    {
            //    }
            //}
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

            
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en,en-US;q=0.9");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36 Edg/94.0.992.50");
            
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"Chromium\";v=\"94\", \"Microsoft Edge\";v=\"94\", \"; Not A Brand\";v=\"99\"");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-site");

            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://www.planetwin365.it");
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;
            
            httpClientEx.DefaultRequestHeaders.Referrer = new Uri("https://www.planetwin365.it/");

            return httpClientEx;
        }

        
        public bool login()
        {
            int nTotalRetry = 0;
            while (nTotalRetry++ < 2)
            {
                try
                {
                    Global.LoadHomeUrl();
                    Thread.Sleep(2000);
                    
                    //check if balance label exists
                    string elementBalance = Global.GetStatusValue("return document.getElementById('l_SiteHeader_personalMenu_lblSaldoTotal');").ToLower();
                    if (!elementBalance.Contains("null"))
                        return true;
                    

                    Global.RunScriptCode($"document.getElementById('l_SiteHeader_cLoginRedesign_ctrlLogin_Username').value='{Setting.Instance.username}';");
                    Global.RunScriptCode($"document.getElementById('l_SiteHeader_cLoginRedesign_ctrlLogin_Password').value='{Setting.Instance.password}';");
                    Global.RunScriptCode("document.getElementById('l_SiteHeader_cLoginRedesign_ctrlLogin_lnkBtnLogin').click();");

                    int nRetry = 0;
                    while (nRetry < 3)
                    {
                        Thread.Sleep(5000);
                        nRetry++;
                        elementBalance = Global.GetStatusValue("return document.getElementById('l_SiteHeader_personalMenu_lblSaldoTotal');").ToLower();
                        if (!elementBalance.Contains("null"))
                        {
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {

                }
            }
            return false;
        }

        public double getBalance()
        {
            int nRetry = 0;
            double result = -1;
            while (nRetry++ < 3)
            {
                try
                {
                    result = Utils.ParseToDouble(Global.GetStatusValue("return document.getElementById('l_SiteHeader_personalMenu_lblSaldoTotal').innerText;").Replace(":", "").Replace("€", "")).ToLower();
                }
                catch
                {

                }

                if (result > 0)
                    break;
                Thread.Sleep(1000);
            }

            Global.WriteTroubleShotLog($"getBalance: {result}");
            return result;
        }
        Random rand = new Random();
        public int generateSelectionId()
        {
            return rand.Next(100001, 999999);
        }
        public PROCESS_RESULT PlaceBet(BetburgerInfo info)
        {
            //LogMng.Instance.onWriteStatus("Placebet step 1");
            try
            {
                Global.RunScriptCode("__doPostBack('l$cCoupon$repCoupon$ctl01$repCouponDetails$ctl00$LinkButton1','');");
                Thread.Sleep(500);
            }
            catch { };
            //LogMng.Instance.onWriteStatus("Placebet step 2");
            string siteUrl = "";
            //if (Setting.Instance.domain.Contains(".it"))
            //{
                //LogMng.Instance.onWriteStatus($"Placebet step 2.5 info.siteUrl: {info.siteUrl}");
                string eventid = info.siteUrl.Substring(info.siteUrl.LastIndexOf("/") + 1);
                //LogMng.Instance.onWriteStatus($"Placebet step 2.5 eventid: {eventid}");
                siteUrl = string.Format("https://www.planetwin365.it/it/scommesse-live/#/event-view/{0}/{1}", info.sport.Replace(" ", ""), eventid);
            //}
            //else
            //{
            //    siteUrl = info.siteUrl;
            //}
            int nRetry = 0;
            //LogMng.Instance.onWriteStatus("Placebet step 3");
            while (nRetry++ < 1)
            {
                try
                {
                    Global.OpenUrl(siteUrl);
                    Thread.Sleep(1000);

                    //add bet
                    string command = $"AddCoupon(\"l_cCoupon_btnAddCoupon\", \"l_cCoupon_txtIDQuota\", {info.direct_link});";
                    Global.RunScriptCode(command);
                    Thread.Sleep(500);
                    int nCheckRetry = 0;
                    while (nCheckRetry++ < 3)
                    {
                        string elementStake = Global.GetStatusValue("return document.getElementById('l_cCoupon_txtImporto').outerHTML").ToLower();
                        if (!elementStake.Contains("null"))
                            break;
                        Thread.Sleep(500);
                    }
                    if (nCheckRetry >= 3)
                        continue;
                    //LogMng.Instance.onWriteStatus("Placebet step 4");
                    Global.RunScriptCode($"document.getElementById('l_cCoupon_txtImporto').value='{info.stake}';");
                    Global.RunScriptCode($"document.getElementById('l_cCoupon_cbAccetaCambioQuote').checked=false;");
                    
                    Thread.Sleep(1000);

                    Global.RunScriptCode("__doPostBack('l$cCoupon$lnkAvanti','');");
                    Thread.Sleep(500);

                    nCheckRetry = 0;
                    while (nCheckRetry++ < 3)
                    {
                        string elementStake = Global.GetStatusValue("return document.getElementById('l_cCoupon_lnkConferma').outerHTML").ToLower();
                        if (!elementStake.Contains("null"))
                            break;
                        Thread.Sleep(500);
                    }
                    if (nCheckRetry >= 3)
                        continue;
                    //LogMng.Instance.onWriteStatus("Placebet step 5");
                    Global.waitResponseEvent.Reset();
                    Global.RunScriptCode("__doPostBack('l$cCoupon$lnkConferma','');");
                    if (Global.waitResponseEvent.Wait(15000))
                    {
                        return PROCESS_RESULT.PLACE_SUCCESS;
                    }
                    //LogMng.Instance.onWriteStatus("Placebet step 6");
                }
                catch (Exception e)
                {

                }
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
