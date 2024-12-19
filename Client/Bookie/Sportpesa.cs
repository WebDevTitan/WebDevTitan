namespace Project.Bookie
{
#if (SPORTPESA || DOMUSBET || REPLATZ || CHANCEBET || BETALAND)
    class SportpesaCtrl : IBookieController
    {

        Object lockerObj = new object();
        public HttpClient m_client = null;
        public string accountId = "";
        public string auth_token = "";

#if (SPORTPESA)
        private string domain = "sportpesa.it";
#elif (BETALAND)
        private string domain = "betaland.it";
#elif (DOMUSBET)
        private string domain = "domusbet.it";
#elif (CHANCEBET)
        private string domain = "chancebet.it";
#elif (REPLATZ)
        private string domain = "replatz.it";
#endif
        public SportpesaCtrl()
        {
            m_client = initHttpClient();
        }

        public int GetPendingbets()
        {
            return 0;
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

        public void Close()
        {

        }

        public void Feature()
        {

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

        public bool logout()
        {
            return false;
        }
        public bool login()
        {
            //Global.SetMonitorVisible(true);
            bool bLogin = false;
            Global.RemoveCookies();
            try
            {
                lock (lockerObj)
                {
                    m_client = initHttpClient();


                    LogMng.Instance.onWriteStatus($"login Start");
                    Global.OpenUrl($"https://www.{domain}/");

                    Thread.Sleep(1000);
                    //LogMng.Instance.onWriteStatus($"login step 1");
#if (REPLATZ || DOMUSBET || BETALAND)

#else
                    Global.RunScriptCode("document.querySelector(\"a[rel]\").click();");
#endif
                    //LogMng.Instance.onWriteStatus($"login step 2 {eleLoginBtn}");

                    int nRetry1 = 0;
                    while (nRetry1 < 20)
                    {
                        Thread.Sleep(500);
#if (DOMUSBET || BETALAND)
                        string result = Global.GetStatusValue("return document.getElementById('cg-username').outerHTML;");
#else
                        string result = Global.GetStatusValue("return document.getElementById('username').outerHTML;");
#endif
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Login button status : {result}");
#endif

                        if (result.Contains("class"))
                        {
                            break;
                        }
                        nRetry1++;
                    }
                    if (nRetry1 >= 30)       //Page is loading gray page. let's retry
                    {
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Can't open login page");
#endif
                        return false;
                    }

#if (DOMUSBET || BETALAND)
                    Global.RunScriptCode($"document.getElementById('cg-username').value='{Setting.Instance.username}';");

                    Global.RunScriptCode($"document.getElementById('cg-password').value='{Setting.Instance.password}';");
#else
                    Global.RunScriptCode($"document.getElementById('username').value='{Setting.Instance.username}';");

                    Global.RunScriptCode($"document.getElementById('password').value='{Setting.Instance.password}';");
#endif
                    Thread.Sleep(500);

#if (REPLATZ)
                    Global.RunScriptCode("document.getElementsByClassName('login-action_login')[0].click();");
#elif (DOMUSBET || BETALAND)
                    Global.RunScriptCode("document.getElementsByClassName('bottone-login')[0].click();");
#else
                    Global.RunScriptCode("document.getElementsByClassName('enterButton')[0].click();");
#endif
                    Thread.Sleep(5000);


                    
                    Task.Run(async () => await Global.GetCookie($"https://www.{domain}")).Wait();

#if (DOMUSBET || BETALAND)
                    if (getBalance() >= 0)
                        bLogin = true;
                    else
                        bLogin = false;
#else
                    string userid = "";
                    try
                    {
                        HttpResponseMessage resp = m_client.GetAsync($"https://www.{domain}/chkLogin.ashx?get=1&rnd={Utils.Javascript_Math_Random()}").Result;
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
#endif
                }
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus($"login exception {e.StackTrace} {e.Message}");
            }
            //Global.SetMonitorVisible(false);

            if (bLogin)
                LogMng.Instance.onWriteStatus($"login Successed");
            else
                LogMng.Instance.onWriteStatus($"login Failed");
            return bLogin;
        }

        public void PlaceBet(List<CapsuledBetburgerInfo> info)
        {
            if (info.Count < 1)
                return;

            info[0].result = PlaceBet(info[0].betburgerInfo);
        }
        public PROCESS_RESULT PlaceBet(BetburgerInfo info)
        {
            string[] strLinkParams = info.direct_link.Split('|');
            if (strLinkParams.Length != 3 || string.IsNullOrEmpty(strLinkParams[0]) || string.IsNullOrEmpty(strLinkParams[1]) || string.IsNullOrEmpty(strLinkParams[2]))
            {
                LogMng.Instance.onWriteStatus($"DirectLink error: {info.direct_link}");
                return PROCESS_RESULT.ERROR;
            }


            m_client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", $"https://www.{domain}/");
            m_client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", $"https://www.{domain}/");

            string cid = "", oddStr = "";
            //try
            //{
            //    HttpResponseMessage liveScheduleResponseMessage = m_client.GetAsync("https://webapi.microgame.it/v1/Sport/Schedule/live").Result;
            //    liveScheduleResponseMessage.EnsureSuccessStatusCode();
            //    string strliveSchedule = liveScheduleResponseMessage.Content.ReadAsStringAsync().Result;
            //    LogMng.Instance.onWriteStatus($"Live Result: {strliveSchedule}");
            //    try
            //    {
            //        dynamic liveScheduleJson = JsonConvert.DeserializeObject<dynamic>(strliveSchedule);
            //        foreach (var sItr in liveScheduleJson.s)
            //        {
            //            foreach (var cItr in sItr.c)
            //            {
            //                foreach (var ccItr in cItr.c)
            //                {
            //                    foreach (var evItr in ccItr.ev)
            //                    {
            //                        if (evItr.id.ToString() == strLinkParams[1])
            //                        {
            //                            cid = ccItr.id.ToString();
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    catch { }
            //}
            //catch { }

            //if (string.IsNullOrEmpty(cid))
            //{
            //    LogMng.Instance.onWriteStatus("Event is not exist");
            //    return PROCESS_RESULT.ERROR;
            //}

            try
            {
                HttpResponseMessage liveScheduleOddResponseMessage = m_client.GetAsync($"https://webapi.microgame.it/v2/Sport/LiveOdds/{strLinkParams[1]}/{strLinkParams[2]}/all/all").Result;
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
            if (CheckOddDropCancelBet(NewOdd, info))
            {
                LogMng.Instance.onWriteStatus(string.Format("Odd is changed from {0} To {1}", info.odds, NewOdd.ToString("N2")));
                return PROCESS_RESULT.MOVED;
            }
            
            int nRetry = 0;
            while (nRetry++ < 2)
            {
                try
                {

                    string userid = "";
                    try
                    {
                        HttpResponseMessage resp = m_client.GetAsync($"https://www.{domain}/chkLogin.ashx?get=1&rnd={Utils.Javascript_Math_Random()}").Result;
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
                    string ReqJson = $"{{\"bets\":[{{\"EventId\":{strLinkParams[2]},\"Odd\":{info.odds.ToString(System.Globalization.CultureInfo.InvariantCulture)},\"OddsId\":{strLinkParams[0]},\"Fixed\":0}}],\"amount\":{info.stake.ToString(System.Globalization.CultureInfo.InvariantCulture)},\"sessionToken\":\"\",\"winning\":{WinValue.ToString(System.Globalization.CultureInfo.InvariantCulture)},\"clientName\":\"CLIENTHTML5PRE\",\"note\":\"[$]client:Mozilla_5_0__Windows_NT_10_0__Win64__x64__AppleWebKit_537_36__KHTML__like_Gecko__Chrome_97_0_4692_99_Safari_537_36|{userid}||{DateTime.Now.ToString("HH:mm:ss:FFF")}|w:3.1|{Utils.Javascript_Math_Random()}\",\"guid\":\"{Guid.NewGuid().ToString()}\",\"tags\":[]}}";
                    LogMng.Instance.onWriteStatus($"bet Req : {ReqJson}");

                    var postData = new StringContent(ReqJson, Encoding.UTF8, "application/json");
                    HttpResponseMessage betResponse = m_client.PostAsync($"https://www.{domain}/SportsBookAPI.svc/v2/placebet", postData).Result;
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
                        if (jsonContent.errors[0].userMessage.ToString().Contains("Limitazione per SSN su importo"))
                        {
                            return PROCESS_RESULT.CRITICAL_SITUATION;
                        }
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

            while (--retryCount >= 0)
            {
                try
                {

#if (DOMUSBET || BETALAND)
                    string balanceUrl = $"https://www.{domain}/updateBalance";
                    var postData = new StringContent("systemCode=DOMUSBET&hash=&setSession=false", Encoding.UTF8, "application/x-www-form-urlencoded");
                    
                    HttpResponseMessage balanceResponseMessage = m_client.PostAsync(balanceUrl, postData).Result;
                    balanceResponseMessage.EnsureSuccessStatusCode();

                    string balanceContent = balanceResponseMessage.Content.ReadAsStringAsync().Result;
                    dynamic balanceObj = JsonConvert.DeserializeObject<dynamic>(balanceContent);

                    balance = Utils.ParseToDouble(balanceObj.data.saldo) / 100;
                    break;
#else
                    string balanceUrl = $"https://www.{domain}/membership.svc/v1/balancesubaccounts/";
#if (CHANCEBET || REPLATZ)
                    balanceUrl = $"https://www.{domain}/membership.svc/v1/balance/";
#endif
                    HttpResponseMessage balanceResponseMessage = m_client.GetAsync(balanceUrl).Result;
                    balanceResponseMessage.EnsureSuccessStatusCode();

                    string balanceContent = balanceResponseMessage.Content.ReadAsStringAsync().Result;
                    JObject balanceObj = JObject.Parse(balanceContent);

                    balance = Utils.ParseToDouble(balanceObj["balance"].ToString().Replace("€", ""));
                    break;
#endif
                }
                catch (Exception e)
                {

                }
            }
            return balance;
        }

        bool CheckOddDropCancelBet(double newOdd, BetburgerInfo info)
        {
            if (info.kind == PickKind.Type_1)
            {
                LogMng.Instance.onWriteStatus($"Odd checking odd in pick: {info.odds} new odd: {newOdd}");
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
            return false;
        }

        public bool Pulse()
        {
            try
            {
                string userid = "";
                HttpResponseMessage resp = m_client.GetAsync($"https://www.{domain}/chkLogin.ashx?get=1&rnd={Utils.Javascript_Math_Random()}").Result;
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
