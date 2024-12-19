namespace Project.Bookie
{
#if (BETPREMIUM)
    class BetpremiumCtrl : IBookieController
    {

        Object lockerObj = new object();
        public HttpClient m_client = null;
        public string accountId = "";
        public string auth_token = "";
        public BetpremiumCtrl()
        {
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

            handler.CookieContainer = Global.cookieContainer;

            HttpClient httpClientEx = new HttpClient(handler);
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.142 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            

            return httpClientEx;
        }
        public bool login()
        {
            //Global.SetMonitorVisible(true);
            bool bLogin = false;
            try
            {
                lock (lockerObj)
                {
                    m_client = initHttpClient();


                    LogMng.Instance.onWriteStatus($"Betpremium login Start");
                    Global.OpenUrl("https://www.betpremium.it/");

                    Thread.Sleep(1000);
                    //LogMng.Instance.onWriteStatus($"login step 1");
                    Global.RunScriptCode("document.getElementsByClassName('LoginClass')[0].getElementsByTagName('a')[0].click();");

                    //LogMng.Instance.onWriteStatus($"login step 2 {eleLoginBtn}");

                    Thread.Sleep(5000);

                    Global.RunScriptCode($"document.getElementById('username').value='{Setting.Instance.username}';");

                    Global.RunScriptCode($"document.getElementById('password').value='{Setting.Instance.password}';");

                    Thread.Sleep(500);

                    Global.RunScriptCode("document.getElementsByClassName('enterButton')[0].click();");

                    Thread.Sleep(5000);


                    
                    Task.Run(async () => await Global.GetCookie("https://www.betpremium.it")).Wait();

                    string userid = "";
                    try
                    {
                        HttpResponseMessage resp = m_client.GetAsync($"https://www.betpremium.it/chkLogin.ashx?get=1&rnd={Utils.Javascript_Math_Random()}").Result;
                        string strContent = resp.Content.ReadAsStringAsync().Result;
                        userid = Utils.Between(strContent, "<user>", "</user>");
                    }
                    catch { }

                    if (string.IsNullOrEmpty(userid))
                    {
                        bLogin = false;
                    }
                    else
                    {
                        bLogin = true;
                    }
                }
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus($"login exception {e.StackTrace} {e.Message}");
            }
            //Global.SetMonitorVisible(false);

            if (bLogin)
                LogMng.Instance.onWriteStatus($"Betpremium login Successed");
            else
                LogMng.Instance.onWriteStatus($"Betpremium login Failed");
            return bLogin;
        }
        public PROCESS_RESULT PlaceBet(BetburgerInfo info)
        {
            string[] strLinkParams = info.direct_link.Split('|');
            if (strLinkParams.Length != 2 || string.IsNullOrEmpty(strLinkParams[0]) || string.IsNullOrEmpty(strLinkParams[1]))
            {
                LogMng.Instance.onWriteStatus($"DirectLink error: {info.direct_link}");
                return PROCESS_RESULT.ERROR;
            }


            m_client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://www.betpremium.it/");
            m_client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://www.betpremium.it/");

            string cid = "", oddStr = "";
            try
            {
                HttpResponseMessage liveScheduleResponseMessage = m_client.GetAsync("https://webapi.microgame.it/v1/Sport/Schedule/live").Result;
                liveScheduleResponseMessage.EnsureSuccessStatusCode();
                string strliveSchedule = liveScheduleResponseMessage.Content.ReadAsStringAsync().Result;
                LogMng.Instance.onWriteStatus($"Live Result: {strliveSchedule}");
                try
                {
                    dynamic liveScheduleJson = JsonConvert.DeserializeObject<dynamic>(strliveSchedule);
                    foreach (var sItr in liveScheduleJson.s)
                    {
                        foreach (var cItr in sItr.c)
                        {
                            foreach (var ccItr in cItr.c)
                            {
                                foreach (var evItr in ccItr.ev)
                                {
                                    if (evItr.id.ToString() == strLinkParams[1])
                                    {
                                        cid = ccItr.id.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }
            catch { }

            if (string.IsNullOrEmpty(cid))
            {
                LogMng.Instance.onWriteStatus("Event is not exist");
                return PROCESS_RESULT.ERROR;
            }

            try
            {
                HttpResponseMessage liveScheduleOddResponseMessage = m_client.GetAsync($"https://webapi.microgame.it/v1/Sport/LiveOdds/{cid}/{strLinkParams[1]}/").Result;
                liveScheduleOddResponseMessage.EnsureSuccessStatusCode();
                string strliveScheduleOdd = liveScheduleOddResponseMessage.Content.ReadAsStringAsync().Result;
                LogMng.Instance.onWriteStatus($"Odd Result: {strliveScheduleOdd}");
                try
                {
                    dynamic liveScheduleOddJson = JsonConvert.DeserializeObject<dynamic>(strliveScheduleOdd);
                    foreach (var eoItr in liveScheduleOddJson.eo)
                    {
                        
                        if (eoItr.id.ToString() == strLinkParams[0])
                        {
                            oddStr = eoItr.o.ToString();
                            break;
                        }
                        
                    }
                }
                catch { }
            }
            catch { }

            if (string.IsNullOrEmpty(oddStr))
            {
                LogMng.Instance.onWriteStatus("Odd is not exist");
                return PROCESS_RESULT.ERROR;
            }

            double NewOdd = Utils.ParseToDouble(oddStr);
            LogMng.Instance.onWriteStatus($"Newodd {NewOdd}");

            int nRetry = 0;
            while (nRetry++ < 2)
            {
                try
                {

                    string userid = "";
                    try
                    {
                        HttpResponseMessage resp = m_client.GetAsync($"https://www.betpremium.it/chkLogin.ashx?get=1&rnd={Utils.Javascript_Math_Random()}").Result;
                        string strContent = resp.Content.ReadAsStringAsync().Result;
                        userid = Utils.Between(strContent, "<user>", "</user>");
                    }
                    catch (Exception ex){
                        LogMng.Instance.onWriteStatus($"check user login Exception {ex.Message} {ex.StackTrace}");
                    }

                    if (string.IsNullOrEmpty(userid))
                    {
                        if (!login())
                        {
                            LogMng.Instance.onWriteStatus($"need to login again");
                            return PROCESS_RESULT.NO_LOGIN;
                        }
                    }

                    double WinValue = info.stake * info.odds;
                    string ReqJson = $"{{\"bets\":[{{\"EventId\":{strLinkParams[1]},\"Odd\":{info.odds.ToString(System.Globalization.CultureInfo.InvariantCulture)},\"OddsId\":{strLinkParams[0]},\"Fixed\":0}}],\"amount\":{info.stake.ToString(System.Globalization.CultureInfo.InvariantCulture)},\"sessionToken\":\"\",\"winning\":{WinValue.ToString(System.Globalization.CultureInfo.InvariantCulture)},\"clientName\":\"CLIENTHTML5PRE\",\"note\":\"[$]client:Mozilla_5_0__Windows_NT_10_0__Win64__x64__AppleWebKit_537_36__KHTML__like_Gecko__Chrome_97_0_4692_99_Safari_537_36|{userid}||{DateTime.Now.ToString("HH:mm:ss:FFF")}|w:3.1|{Utils.Javascript_Math_Random()}\",\"guid\":\"{Guid.NewGuid().ToString()}\",\"tags\":[]}}";
                    LogMng.Instance.onWriteStatus($"bet Req : {ReqJson}");

                    var postData = new StringContent(ReqJson, Encoding.UTF8, "application/json");
                    HttpResponseMessage betResponse = m_client.PostAsync("https://www.betpremium.it/SportsBookAPI.svc/v2/placebet", postData).Result;
                    string strBetResp = betResponse.Content.ReadAsStringAsync().Result;

                    LogMng.Instance.onWriteStatus($"placebet Result: {strBetResp}");

                    dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(strBetResp);
                    string result = jsonContent.ResultCode.ToString();
                    
                    if (result.ToUpper() == "OK")
                    {
                        LogMng.Instance.onWriteStatus("placing success");
                        return PROCESS_RESULT.PLACE_SUCCESS;
                    }
                    else if (result.ToUpper() == "ERROR")
                    {
                        LogMng.Instance.onWriteStatus("Message: " + jsonContent.errors[0].userMessage.ToString());
                    }
                    else if (result == "RequiresConfirmationForLowerWinning")
                    {
                        LogMng.Instance.onWriteStatus("Odd is changed");
                        return PROCESS_RESULT.MOVED;
                    }
                    else if (result == "Proposal")
                    {
                        try
                        {
                            if (jsonContent.Limitations[0].Name.ToString().Trim() == "AmountUserLimitationSSN")
                            {                                
                                return PROCESS_RESULT.CRITICAL_SITUATION;
                            }
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    LogMng.Instance.onWriteStatus($"Placebet Exception {ex.Message} {ex.StackTrace}");
                }
                Thread.Sleep(2000);
            }
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
                 
                    HttpResponseMessage balanceResponseMessage = m_client.GetAsync("https://www.betpremium.it/membership.svc/v1/balancesubaccounts/").Result;
                    balanceResponseMessage.EnsureSuccessStatusCode();

                    string balanceContent = balanceResponseMessage.Content.ReadAsStringAsync().Result;
                    JObject balanceObj = JObject.Parse(balanceContent);

                    balance = Utils.ParseToDouble(balanceObj["balance"].ToString().Replace("€", ""));
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

                
        public bool Pulse()
        {
            try
            {
                string userid = "";
                HttpResponseMessage resp = m_client.GetAsync($"https://www.betpremium.it/chkLogin.ashx?get=1&rnd={Utils.Javascript_Math_Random()}").Result;
                string strContent = resp.Content.ReadAsStringAsync().Result;
                userid = Utils.Between(strContent, "<user>", "</user>");
                if (!string.IsNullOrEmpty(userid))
                {
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
#endif
}
