namespace Project.Bookie
{
#if (BET365)
    public class Bet365Ctrl : IBookieController
    {
        public HttpClient m_client = null;
        private const double minMarketStake = 10;
        
        public Bet365Ctrl()
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
        public bool Pulse()
        {
            return false;
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

        private FormUrlEncodedContent convertStringIntoFormData(string ns, string ms, MAKE_SLIP_STEP step)
        {
            try
            {
                var keyValues = new List<KeyValuePair<string, string>>();

                keyValues.Add(new KeyValuePair<string, string>());
                keyValues.Add(new KeyValuePair<string, string>("ns", ns));
                LogMng.Instance.onWriteStatus(string.Format("MakeFormData ns {0}", ns));
                if (!string.IsNullOrEmpty(ms))
                {
                    keyValues.Add(new KeyValuePair<string, string>("ms", ms));
                    LogMng.Instance.onWriteStatus(string.Format("MakeFormData ms {0}", ms));
                }

                keyValues.Add(new KeyValuePair<string, string>("betsource", "FlashInPLay"));

                if (step == MAKE_SLIP_STEP.PLACE_BET)
                    keyValues.Add(new KeyValuePair<string, string>("tagType", "WindowsDesktopBrowser"));

                if (step == MAKE_SLIP_STEP.REFRESH_BET)
                    keyValues.Add(new KeyValuePair<string, string>("cr", "1"));

                keyValues.Add(new KeyValuePair<string, string>("bs", "1"));

                return new FormUrlEncodedContent(keyValues);
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public bool login()
        {
            //LogMng.Instance.onWriteStatus("Login [0]");
            //if (checkPrevLogin())
            //{
            //    return true;
            //}
            //LogMng.Instance.onWriteStatus("Login [0.1]");

            int retryCount = 2;
#if OXYLABS
            retryCount = 5;
#endif

            while (--retryCount >= 0)
            {
                try
                {
                    m_client = initHttpClient();
                    HttpResponseMessage response = m_client.GetAsync(string.Format("https://www.{0}", Setting.Instance.domain)).Result;
                    response.EnsureSuccessStatusCode();
                    var responseCookies = Global.cookieContainer.GetCookies(new Uri(string.Format("https://www.{0}", Setting.Instance.domain)));
                    string txtToken = string.Empty;
                    foreach (System.Net.Cookie cookie in responseCookies)
                    {
                        if (cookie.Name == "pstk")
                        {
                            txtToken = cookie.Value;
                            break;
                        }
                    }

                    var loginPost = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string,string>("txtUsername",  Setting.Instance.username),
                        new KeyValuePair<string, string>("txtPassword", Setting.Instance.password),
                        new KeyValuePair<string, string>("txtTKN", txtToken),
                        new KeyValuePair<string, string>("txtFlashVersion", "NOTSET"),
                        new KeyValuePair<string, string>("txtScreenSize", "1916 x 886"),
                        new KeyValuePair<string, string>("txtHPFV", "NOTSET NOTSET"), //1529 I1PIP23 //1527 M07
                        new KeyValuePair<string, string>("txtType", "85"),
                        new KeyValuePair<string, string>("platform","1"),
                        new KeyValuePair<string, string>("IS", "1")
                    });

                    string loginUrl = string.Format("https://members.{0}/Members/lp/default.aspx", Setting.Instance.domain);
                    m_client.DefaultRequestHeaders.Remove("Accept");
                    m_client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/javascript, */*; q=0.01");
                    m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                    m_client.DefaultRequestHeaders.Referrer = new Uri(string.Format("https://www.{0}", Setting.Instance.domain));

                    
                    foreach (var headeritr in Global.BetHeader.Pirxtheaders)
                    {
                        string[] nameSeg = headeritr.Name.Split('_');
                        if (nameSeg.Count() == 2)
                        {
                            string headerName = "PIRXTcSdwp-" + nameSeg[1];
                            m_client.DefaultRequestHeaders.Remove(headerName);
                            m_client.DefaultRequestHeaders.TryAddWithoutValidation(headerName, headeritr.Value);

                            //LogMng.Instance.onWriteStatus(headerName + ":" + headeritr);
                        }
                    }
                    m_client.DefaultRequestHeaders.Remove("PIRXTcSdwp-z");
                    m_client.DefaultRequestHeaders.TryAddWithoutValidation("PIRXTcSdwp-z", "q");

                    m_client.DefaultRequestHeaders.Remove("X-Net-Sync-Term");
                    m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-Net-Sync-Term", Global.BetHeader.NSToken);

                    HttpResponseMessage loginResponse = m_client.PostAsync(loginUrl, loginPost).Result;
                    loginResponse.EnsureSuccessStatusCode();

                    response.EnsureSuccessStatusCode();
                    //LogMng.Instance.onWriteStatus("Login [1]");
                    if (response.StatusCode != HttpStatusCode.OK)
                        return false;
                    //LogMng.Instance.onWriteStatus("Login [2]");
                    string responseMessageSignString = response.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(responseMessageSignString))
                        return false;
                    //LogMng.Instance.onWriteStatus("Login [3]");
                    GroupCollection groups = Regex.Match(responseMessageSignString, "txtPSTK=(?<pstk>\\w*)").Groups;
                    if (groups == null || groups["pstk"] == null)
                        return false;
                    //LogMng.Instance.onWriteStatus("Login [4]");
                    txtToken = groups["pstk"].Value;
                    //WriteCookiesToDisk(Global.cookieContainer);
                    LogMng.Instance.onWriteStatus("Login success from freshLogin!");
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

        public PROCESS_RESULT PlaceBet(BetburgerInfo info)
        {
            return PlaceBet(Utils.ConvertBetburgerPick2OpenBet_365(info));
        }
        public PROCESS_RESULT PlaceBet(OpenBet_Bet365 betinfo)
        {
            PROCESS_RESULT SlipRes = PROCESS_RESULT.ERROR;

            string strBet365Result = string.Empty;

            int nRetry4SmallMarket = 1;

            double origStake = betinfo.stake;
            while (nRetry4SmallMarket > 0)
            {
                nRetry4SmallMarket--;
                string ns = "", ms = "";
                SlipRes = GetNsToken(ref ns, ref ms, ref betinfo, MAKE_SLIP_STEP.INIT, null);
                if (SlipRes == PROCESS_RESULT.ERROR)
                {
                    LogMng.Instance.onWriteStatus(string.Format("[InitBet] Step 1 Failed"));
                    return SlipRes;
                }

                int nRetry = 0;
                while (nRetry++ < 2)
                {
                    strBet365Result = doAddBet(ns, ms);
                    SlipRes = GetNsToken(ref ns, ref ms, ref betinfo, MAKE_SLIP_STEP.ADD_BET, strBet365Result);
                    if (SlipRes == PROCESS_RESULT.ERROR)
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[AddBet] Step 2 Failed"));
                        return SlipRes;
                    }
                    else if (SlipRes == PROCESS_RESULT.NO_LOGIN)
                    {
                        if (!login())
                        {
                            LogMng.Instance.onWriteStatus(string.Format("[AddBet] Step 3 Failed"));
                            return SlipRes;
                        }
                    }
                    else if (SlipRes == PROCESS_RESULT.MOVED)
                    {
                        strBet365Result = doRefreshSlip(ns, ms);
                        SlipRes = GetNsToken(ref ns, ref ms, ref betinfo, MAKE_SLIP_STEP.ADD_BET, strBet365Result);
                    }
                    else
                    {
                        break;
                    }
                }

                if (SlipRes != PROCESS_RESULT.SUCCESS)
                {
                    LogMng.Instance.onWriteStatus(string.Format("[AddBet] Step 4 Failed"));

                    return SlipRes;
                }

                if (Setting.Instance.domain.Contains(".au"))
                {
                    nRetry = 0;
                    while (nRetry++ < 2)
                    {
                        strBet365Result = doConfirmBet(betinfo.betGuid, ns, ms);
                        LogMng.Instance.onWriteStatus("confirmbet result: " + strBet365Result);

                        if (strBet365Result.Contains("\"sr\":0"))
                        {
                            break;
                        }

                        SlipRes = GetNsToken(ref ns, ref ms, ref betinfo, MAKE_SLIP_STEP.PLACE_BET, strBet365Result);
                        if (SlipRes == PROCESS_RESULT.ERROR)
                        {
                            LogMng.Instance.onWriteStatus(string.Format("[ConfirmBet] Step 2 Failed"));
                            return SlipRes;
                        }
                        else if (SlipRes == PROCESS_RESULT.NO_LOGIN)
                        {
                            if (!login())
                            {
                                LogMng.Instance.onWriteStatus(string.Format("[ConfirmBet] Step 3 Failed"));
                                return SlipRes;
                            }
                        }
                        else if (SlipRes == PROCESS_RESULT.MOVED)
                        {
                            strBet365Result = doRefreshSlip(ns, ms);
                            SlipRes = GetNsToken(ref ns, ref ms, ref betinfo, MAKE_SLIP_STEP.PLACE_BET, strBet365Result);
                        }
                    }

                    if (!strBet365Result.Contains("\"sr\":0"))
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[ConfirmBet]! confirmbet failed!"));
                        return PROCESS_RESULT.ERROR;
                    }
                }

                nRetry = 0;
                while (nRetry++ < 4)
                {
                    strBet365Result = doPlaceBet(betinfo.betGuid, ns, ms);
                    SlipRes = GetNsToken(ref ns, ref ms, ref betinfo, MAKE_SLIP_STEP.PLACE_BET, strBet365Result);
                    if (SlipRes == PROCESS_RESULT.PLACE_SUCCESS)
                    {
                        LogMng.Instance.onWriteStatus($"[PlaceBet]! success! stake: {betinfo.stake} origStake: {origStake}");
                        //check if retrying for small markets
                        if (origStake - betinfo.stake >= 1)
                        {
                            origStake -= betinfo.stake;

                            if (origStake < betinfo.stake)
                            {
                                betinfo.stake = origStake;                                
                            }

                            nRetry4SmallMarket = 1;

                            LogMng.Instance.onWriteStatus($"[PlaceBet] Retyring for small stake market cur stake : {betinfo.stake}");
                            break;
                        }                        

                        return SlipRes;
                    }
                    else
                    {
                        LogMng.Instance.onWriteStatus(string.Format("[PlaceBet] failed result: {0}", SlipRes));
                    }
                }
            }
            return SlipRes;
        }
        private string CallBet365(string actionUrl, FormUrlEncodedContent param)
        {
            int retryCount = 2;
#if OXYLABS
            retryCount = 5;
#endif

            while (--retryCount >= 0)
            {
                try
                {
                    m_client.DefaultRequestHeaders.Remove("X-Net-Sync-Term");
                    m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-Net-Sync-Term", Global.BetHeader.NSToken);

                    LogMng.Instance.onWriteStatus($"Nstoken: {Global.BetHeader.NSToken}");

                    HttpResponseMessage actionResponse = m_client.PostAsync(actionUrl, param).Result;
                    actionResponse.EnsureSuccessStatusCode();
                    return actionResponse.Content.ReadAsStringAsync().Result;
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
            return string.Empty;
        }
        private string doPlaceBet(string betGuid, string ns, string ms)
        {
            if (string.IsNullOrEmpty(ns))
                return String.Empty;

            
            foreach (var headeritr in Global.BetHeader.Pirxtheaders)
            {
                string[] nameSeg = headeritr.Name.Split('_');
                if (nameSeg.Count() == 2)
                {
                    string headerName = "PIRXTcSdwp-" + nameSeg[1];
                    m_client.DefaultRequestHeaders.Remove(headerName);
                    m_client.DefaultRequestHeaders.TryAddWithoutValidation(headerName, headeritr.Value);

                    //LogMng.Instance.onWriteStatus(headerName + ":" + headeritr);
                }
            }
            m_client.DefaultRequestHeaders.Remove("PIRXTcSdwp-z");
            m_client.DefaultRequestHeaders.TryAddWithoutValidation("PIRXTcSdwp-z", "q");

            FormUrlEncodedContent postContent = convertStringIntoFormData(ns, ms, MAKE_SLIP_STEP.PLACE_BET);
            string actionUrl = string.Format("https://www.{0}/BetsWebApi/placebet?betGuid={1}", Setting.Instance.domain, betGuid);            
            string result = CallBet365(actionUrl, postContent);

            for (char c = 'a'; c <= 'z'; c++)
            {
                m_client.DefaultRequestHeaders.Remove($"PIRXTcSdwp-{c}");
            }
            return result;
        }
        private string doConfirmBet(string betGuid, string ns, string ms)
        {
            if (string.IsNullOrEmpty(ns))
                return String.Empty;
            FormUrlEncodedContent postContent = convertStringIntoFormData(ns, ms, MAKE_SLIP_STEP.PLACE_BET);
            string actionUrl = string.Format("https://www.{0}/BetsWebAPI/confirmbet?betGuid={1}", Setting.Instance.domain, betGuid);
            return CallBet365(actionUrl, postContent);
        }
        private string doRefreshSlip(string ns, string ms)
        {
            if (string.IsNullOrEmpty(ns))
                return String.Empty;
            FormUrlEncodedContent postContent = convertStringIntoFormData(ns, ms, MAKE_SLIP_STEP.REFRESH_BET);
            string actionUrl = string.Format("https://www.{0}/BetsWebAPI/refreshslip", Setting.Instance.domain);
            return CallBet365(actionUrl, postContent);
        }
        private string doAddBet(string ns, string ms)
        {
            if (string.IsNullOrEmpty(ns))
                return String.Empty;
            FormUrlEncodedContent postContent = convertStringIntoFormData(ns, ms, MAKE_SLIP_STEP.ADD_BET);
            string actionUrl = string.Format("https://www.{0}/BetsWebApi/addbet", Setting.Instance.domain);
            return CallBet365(actionUrl, postContent);
        }
        private PROCESS_RESULT GetNsToken(ref string ns, ref string ms, ref OpenBet_Bet365 infos, MAKE_SLIP_STEP Step, string betSlipString)
        {
            BetSlipJson betSlipJson = null;
            if (!string.IsNullOrEmpty(ms) && infos.betData.Count < 2)
            {
                LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Error MsToken Exists, but betData count is less than 2"));
                return PROCESS_RESULT.ERROR;
            }

            if (Step == MAKE_SLIP_STEP.ADD_BET)
            {
                LogMng.Instance.onWriteStatus(string.Format("[AddBet res] {0}", betSlipString));
                if (!string.IsNullOrEmpty(betSlipString))
                {
                    betSlipJson = JsonConvert.DeserializeObject<BetSlipJson>(betSlipString);
                }
                else
                {
                    LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Error No Slip String (ADD_BET)"));
                    return PROCESS_RESULT.NO_LOGIN;
                } 

                if (string.IsNullOrEmpty(betSlipJson.bg))
                    return PROCESS_RESULT.NO_LOGIN;
                else
                    infos.betGuid = betSlipJson.bg;
            }
            else if (Step == MAKE_SLIP_STEP.PLACE_BET)
            {
                LogMng.Instance.onWriteStatus(string.Format("[PLACE_BET res] {0}", betSlipString));
                if (!string.IsNullOrEmpty(betSlipString))
                {
                    betSlipJson = JsonConvert.DeserializeObject<BetSlipJson>(betSlipString);
                }
                else
                {
                    LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Error No Slip String (PLACE_BET)"));
                    return PROCESS_RESULT.ERROR;
                }

                if (betSlipJson.sr == 0)
                    return PROCESS_RESULT.PLACE_SUCCESS;
            }


            bool bEachWay = false;

            ns = "";
            //ms = "";

            string re = "";
            try
            {
                for (int i = 0; i < infos.betData.Count; i++)
                {
                    if (infos.betData[i].eachway)
                        bEachWay = true;


                    if (Step == MAKE_SLIP_STEP.INIT)
                    {// have to last bet with "id" when add bet
                        if (i == infos.betData.Count - 1)
                            infos.betData[i].sa = $"id={infos.betData[i].fd}-{infos.betData[i].i2}Y";
                        else
                            infos.betData[i].sa = $"sa={calculateSA()}";
                    }
                    else
                    {// everything is sa
                        if (string.IsNullOrEmpty(infos.betData[i].sa) || infos.betData[i].sa.Contains("id="))
                            infos.betData[i].sa = $"sa={calculateSA()}";
                    }


                    if (betSlipJson != null)
                    {
                        if (betSlipJson.sr == 0)
                        {
                            for (int k = 0; k < betSlipJson.bt.Count; k++)
                            {
                                BetSlipItem betSlipItem = betSlipJson.bt[k];
                                if (betSlipItem.pt[0].pi != infos.betData[i].i2)
                                {
                                    continue;
                                }

                                if (betSlipItem.sr == 0)
                                {
                                    if (betSlipItem.su)
                                    {
                                        LogMng.Instance.onWriteStatus(string.Format("Market is suspended"));
                                        return PROCESS_RESULT.SUSPENDED;
                                    }

                                    if (!string.IsNullOrEmpty(betSlipItem.sa))
                                        infos.betData[i].sa = $"sa={betSlipItem.sa}";

                                    if (!string.IsNullOrEmpty(betSlipItem.od) && infos.betData[i].oddStr != betSlipItem.od)
                                    {
                                        LogMng.Instance.onWriteStatus(string.Format("Odd moved {0} -> {1}", infos.betData[i].oddStr, betSlipItem.od));
                                        infos.betData[i].oddStr = betSlipItem.od;
                                    }

                                    if (!string.IsNullOrEmpty(betSlipItem.pt[0].ha) && infos.betData[i].ht != betSlipItem.pt[0].ha)
                                    {
                                        LogMng.Instance.onWriteStatus(string.Format("line moved {0} -> {1}", infos.betData[i].ht, betSlipItem.pt[0].ha));
                                        infos.betData[i].ht = betSlipItem.pt[0].ha;
                                    }

                                    if (!string.IsNullOrEmpty(betSlipItem.oo))
                                        infos.betData[i].oo = betSlipItem.oo;

                                    if (betSlipItem.oc)
                                        infos.betData[i].oc = true;

                                    infos.betData[i].ea = betSlipItem.ea || betSlipItem.ew || betSlipItem.ex;
                                    infos.betData[i].ed = betSlipItem.ed;
                                }
                                else
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Error sr : {0}", betSlipItem.sr));
                                }

                                break;
                            }
                        }
                        else if (betSlipJson.sr == -2)
                        {
                            LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Session is Locked, Retry after 5 sec"));
                            Thread.Sleep(5 * 1000);                            
                        }
                        else if (betSlipJson.sr == 10)
                        {
                            LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Balance is not Enough"));
                            return PROCESS_RESULT.SMALL_BALANCE;
                        }
                        else if (betSlipJson.sr == 11 || betSlipJson.sr == 24)
                        {
                            if (infos.betData.Count == 1)
                            {
                                double maxStake = betSlipJson.bt[0].ms;
                                if (maxStake == 0)
                                {
                                    if (!string.IsNullOrEmpty(betSlipJson.bt[0].re) && Utils.ParseToDouble(betSlipJson.bt[0].re) > 0)
                                    {
                                        //re = betSlipJson.bt[0].re;
                                        infos.stake /= 2;
                                        if (infos.stake > minMarketStake)
                                            infos.stake = minMarketStake;
                                        Thread.Sleep(2 * 1000);
                                    }
                                    else
                                    {
                                        LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Max Stake is 0"));
                                        return PROCESS_RESULT.ZERO_MAX_STAKE;
                                    }
                                }
                                else
                                {
                                    infos.sl = true;
                                    infos.stake = maxStake;
                                }
                            }
                            else
                            {
                                if (betSlipJson.mo.Count > 0)
                                {
                                    double maxStake = betSlipJson.mo[0].ms;
                                    if (maxStake == 0)
                                    {
                                        LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Max Stake is 0"));
                                        return PROCESS_RESULT.ZERO_MAX_STAKE;
                                    }
                                    infos.sl = true;
                                    infos.stake = maxStake;
                                }
                                else
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Error large than Max Stake in Combine bets, but mo result is inccorect"));
                                    return PROCESS_RESULT.ERROR;
                                }
                            }
                        }
                        else if (betSlipJson.sr == 14)
                        {
                            for (int k = 0; k < betSlipJson.bt.Count; k++)
                            {
                                BetSlipItem betSlipItem = betSlipJson.bt[k];
                                if (betSlipItem.pt[0].pi != infos.betData[i].i2)
                                {
                                    continue;
                                }

                                if (!string.IsNullOrEmpty(betSlipItem.sa))
                                    infos.betData[i].sa = $"sa={betSlipItem.sa}";

                                if (betSlipItem.ms == 0)
                                {
                                    re = betSlipItem.re;                                   
                                }
                                else
                                {
                                    if (infos.stake <= betSlipItem.ms)
                                    {
                                        re = betSlipItem.re;
                                    }
                                    else
                                    {
                                        infos.sl = true;
                                        infos.stake = betSlipItem.ms;
                                    }
                                }

                                if (!string.IsNullOrEmpty(betSlipItem.od) && infos.betData[i].oddStr != betSlipItem.od)
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("Odd moved {0} -> {1}", infos.betData[i].oddStr, betSlipItem.od));
                                    //return PROCESS_RESULT.ERROR;
                                    infos.betData[i].oddStr = betSlipItem.od;

                                    re = string.Empty;
                                }

                                if (!string.IsNullOrEmpty(betSlipItem.pt[0].ha) && infos.betData[i].ht != betSlipItem.pt[0].ha)
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("line moved {0} -> {1}", infos.betData[i].ht, betSlipItem.pt[0].ha));
                                    //return PROCESS_RESULT.ERROR;
                                    infos.betData[i].ht = betSlipItem.pt[0].ha;
                                }

                                if (!string.IsNullOrEmpty(betSlipItem.oo))
                                    infos.betData[i].oo = betSlipItem.oo;

                                if (betSlipItem.oc)
                                    infos.betData[i].oc = true;

                                break;
                            }
                        }
                        else if (betSlipJson.sr == 15)
                        {
                            Thread.Sleep(2 * 1000);
                            LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] sr 15 retry after 2 sec"));                            
                        }
                        else
                        {
                            LogMng.Instance.onWriteStatus(string.Format("[GetNsToken] Unknown SR error {0}", betSlipJson.sr));
                            return PROCESS_RESULT.ERROR;
                        }
                    }


                    if (string.IsNullOrEmpty(infos.betData[i].ht))
                        ns += $"pt=N#o={infos.betData[i].oddStr}#f={infos.betData[i].fd}#fp={infos.betData[i].i2}#so=#c={infos.betData[i].cl}#mt=22#{infos.betData[i].sa}#";
                    else
                        ns += $"pt=N#o={infos.betData[i].oddStr}#f={infos.betData[i].fd}#fp={infos.betData[i].i2}#so=#c={infos.betData[i].cl}#ln={infos.betData[i].ht}#mt=22#{infos.betData[i].sa}#";

                    if (!string.IsNullOrEmpty(infos.betData[i].oo))
                        ns += $"oto={infos.betData[i].oo}#";

                    //if (infos.betData[i].oc)
                    //    ns += $"olc=1#";

                    ns += $"|TP=BS{infos.betData[i].fd}-{infos.betData[i].i2}#";

                    infos.betData[i].odd = Utils.FractionToDouble(infos.betData[i].oddStr);
                    infos.stake = Math.Truncate(infos.stake * 100) / 100;

                    if (infos.betData[i].odd == 0)
                        return PROCESS_RESULT.ERROR;

                    if (infos.betData.Count == 1 && betSlipJson != null)
                    {   
                        double tr = infos.stake * infos.betData[i].odd + 0.0001;

                        bool bCheckEachwayLine = true;

#if USOCKS || OXYLABS
    Setting.Instance.bEachWay = true;
    if (Setting.Instance.eachWayOdd < 4)
        Setting.Instance.eachWayOdd = 5.1;
#endif

                        if (Setting.Instance.bEachWay && infos.betData[i].odd < Setting.Instance.eachWayOdd)
                            bCheckEachwayLine = false;

                        ns = $"{ns}ust={infos.stake.ToString("N2")}#st={infos.stake.ToString("N2")}#";
                        if (infos.sl)
                            ns += $"sl={infos.stake.ToString("N2")}#";

                        if (bCheckEachwayLine && infos.betData[i].cl == "2" && infos.betData[i].ea && infos.betData[i].ed != 0)
                        {
                            tr += infos.stake * Utils.FractionToDoubleOfEachway(infos.betData[i].oddStr, infos.betData[i].ed);
                            tr = Math.Truncate(tr * 100) / 100;

                            ns += $"ew=1#";
                        }
                        else
                        {
                            tr = Math.Truncate(tr * 100) / 100;
                        }

                        if (!string.IsNullOrEmpty(re))
                            ns += $"tr={re}#";
                        else
                            ns += $"tr={tr.ToString("N2")}#";
                    }

                    ns += "||";
                }

                
                if (infos.betData.Count > 1 && betSlipJson != null && string.IsNullOrEmpty(ms))
                {
                    if (betSlipJson.dm != null && betSlipJson.dm.ea && bEachWay)
                        ms = $"id={betSlipJson.dm.bt}#bc={betSlipJson.dm.bc}#ust={infos.stake.ToString("N2")}#st={infos.stake.ToString("N2")}#|ew=1#||";
                    else
                        ms = $"id={betSlipJson.dm.bt}#bc={betSlipJson.dm.bc}#ust={infos.stake.ToString("N2")}#st={infos.stake.ToString("N2")}#||";

                    foreach (Dm dm in betSlipJson.mo)
                        ms += $"id={dm.bt}#bc={dm.bc}#||";
                }
            }
            catch (Exception e)
            {
            }

            return PROCESS_RESULT.SUCCESS;
        }
        private string calculateSA()
        {
            Random rnd = new Random();
            int randVal = rnd.Next(1, 15);
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + randVal;
            string aa = unixTimestamp.ToString("X2").ToLower();
            string hexValue = DateTime.Now.Ticks.ToString("X2");
            return aa + "-" + hexValue.Substring(hexValue.Length - 8, 8);
        }        
        public long getTick()
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            long timestamp = (long)t.TotalMilliseconds;
            return timestamp;
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
                string result = string.Empty;
                try
                {
                    m_client.DefaultRequestHeaders.Remove("X-Net-Sync-Term");
                    m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-Net-Sync-Term", Global.BetHeader.NSToken);

                    string balanceUrl = string.Format("https://www.{0}/balancedataapi/pullbalance?rn={1}&y=jsn", Setting.Instance.domain, Utils.getTick());
                    HttpResponseMessage response = m_client.GetAsync(balanceUrl).Result;
                    response.EnsureSuccessStatusCode();
                    result = response.Content.ReadAsStringAsync().Result;
                    //LogMng.Instance.onWriteStatus("GetBalance: " + result);
                    //HtmlDocument doc = new HtmlDocument();
                    //doc.LoadHtml(result);
                    //HtmlNodeCollection divList = doc.DocumentNode.SelectNodes("//div[@data-section='sports-balance-total']/div[@class='balance-column col-1']/div");
                    //string strBalance = divList[1].InnerText.Replace(",", "").Replace(".", "").Replace("$", "").Replace("€", "");
                    //balance = Utils.ParseToDouble(strBalance) / 100;
                    //m_balance = balance;

                    if (result.Equals("error")) return -1;
                    string[] tempArr = result.Split('$');
                    balance = Utils.ParseToDouble(tempArr[1].Replace(",", "."));
                    break;
                    //WriteCookiesToDisk(Global.cookieContainer);                
                }
                catch (Exception e)
                {
                    LogMng.Instance.onWriteStatus(string.Format("Error getBalance : {0}", e.ToString()));
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
        //public CookieContainer ReadCookiesFromDisk()
        //{
        //    CookieContainer cookieJar = null;
        //    try
        //    {
        //        string cookieFilePath = string.Format("Cookies\\{0}", Setting.Instance.username);
        //        using (Stream stream = File.Open(cookieFilePath, FileMode.Open))
        //        {
        //            BinaryFormatter formatter = new BinaryFormatter();
        //            cookieJar = (CookieContainer)formatter.Deserialize(stream);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        cookieJar = new CookieContainer(300, 50, 20480);
        //    }
        //    return cookieJar;
        //}
        //public void WriteCookiesToDisk(CookieContainer cookieJar)
        //{
        //    try
        //    {
        //        Directory.CreateDirectory("Cookies\\");

        //        string cookieFilePath = string.Format("Cookies\\{0}", Setting.Instance.username);
        //        using (Stream stream = File.Create(cookieFilePath))
        //        {
        //            try
        //            {
        //                BinaryFormatter formatter = new BinaryFormatter();
        //                formatter.Serialize(stream, cookieJar);
        //            }
        //            catch (Exception e)
        //            {

        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {

        //    }

        //}
        //private bool checkPrevLogin()
        //{
        //    try
        //    {
        //        CookieContainer cookieJar = ReadCookiesFromDisk();
        //        m_client = initHttpClient(cookieJar);
        //        double balance = getBalance();
        //        //밸런스가 -1이면 로그아웃상태로 판정한다.
        //        if (balance >= 0)
        //        {
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return false;
        //}

    }
#endif
}

