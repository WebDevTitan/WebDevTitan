namespace Project.Bookie
{
#if (FANDUEL)

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
    public class FanduelCtrl : IBookieController
    {
        Dictionary<string, string> dictEventUrl = new Dictionary<string, string>();
        Object lockerObj = new object();

        public HttpClient m_client = null;
        public string sessionToken = null;

        public string appkey = "";

        private IPlaywright playwright;
        private IBrowser browser = null;
        private IPage page = null;

        public FanduelCtrl()
        {
            m_client = initHttpClient();
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
            GetLiveMatchList();
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
        public bool InnerLogin(bool bRemovebet = true)
        {
            Global.RemoveCookies();
            Global.SetMonitorVisible(true);
            bool bLogin = false;
            try
            {
                lock (lockerObj)
                {
                    m_client = initHttpClient();



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

                    Thread.Sleep(3000);


                    Task.Run(async () => await Global.GetCookie($"https://www.{Setting.Instance.domain}")).Wait();
                    if (bRemovebet)
                    {
                        removeBet();
                        Feature();
                    }
                    bLogin = true;
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
        public bool login()
        {
            return InnerLogin();
        }


        public void PlaceBet(List<CapsuledBetburgerInfo> info)
        {
            if (info.Count < 1)
                return;

            info[0].result = PlaceBet(info[0].betburgerInfo);
        }

        public PROCESS_RESULT PlaceBet(BetburgerInfo info)
        {
            BetResult br = new BetResult();

            try
            {
                OpenBet_Betfair openbet = Utils.ConvertBetburgerPick2OpenBet_Betfair(info);

                if (openbet == null)
                {
                    LogMng.Instance.onWriteStatus("Converting OpenBet Error");
                    return PROCESS_RESULT.ERROR;
                }

                string eventUrl = "";
                Monitor.Enter(lockerObj);
                if (dictEventUrl.ContainsKey(openbet.eventId))
                {
                    eventUrl = dictEventUrl[openbet.eventId];
                }
                Monitor.Exit(lockerObj);

                if (string.IsNullOrEmpty(eventUrl))
                {
                    LogMng.Instance.onWriteStatus("Cannot find EventUrl");
                    return PROCESS_RESULT.ERROR;
                }

                Global.OpenUrl(eventUrl + "?selectedGroup=all_markets");

                string pageSource = Global.GetPageSource().Result;
                if (pageSource.Contains("\"logged out\""))
                {
                    LogMng.Instance.onWriteStatus("[Placebet]Login because of logout already");
                    InnerLogin(true);
                }

                int nMarketClickRetry = 0;
                while (nMarketClickRetry++ < 3)
                {
                    int nMarketCount = Utils.parseToInt(Global.GetStatusValue($"return document.querySelectorAll('a[data-eventid=\"{openbet.bseId}\"][data-marketid=\"{openbet.bsmId}\"][data-selectionid=\"{openbet.bssId}\"][data-betslip-action=\"add-bet\"]').length;"));
                    if (nMarketCount <= 0)
                        Thread.Sleep(500);

                    Global.RunScriptCode($"document.querySelectorAll('a[data-eventid=\"{openbet.bseId}\"][data-marketid=\"{openbet.bsmId}\"][data-selectionid=\"{openbet.bssId}\"][data-betslip-action=\"add-bet\"]')[0].click();");
                    break;
                }

                double newOdd = 0;
                int nGetOddRetry = 0;
                while (nGetOddRetry++ < 3)
                {
                    //function GetNewOdd(marketid, selectionid)
                    //{
                    //    var betList = document.querySelectorAll('ul li.ui-bet-row');
                    //    for (var i = 0; i < betList.length; i++)
                    //    {
                    //        var betItr = betList[i];
                    //        if (betItr.getAttribute('data-marketid') != marketid || betItr.getAttribute('data-selectionid') != selectionid)
                    //            continue;

                    //        if (betItr.classList.contains('ui-disabled'))
                    //            return 'Suspended';

                    //        return betItr.getAttribute('data-last-price');
                    //    }
                    //    return 'NotInSlip';
                    //}
                    string GetOddresult = Global.RunScriptCode($"function GetNewOdd(marketid, selectionid) {{ var betList = document.querySelectorAll('ul li.ui-bet-row'); for (var i = 0; i < betList.length; i++) {{ var betItr = betList[i]; if (betItr.getAttribute('data-marketid') != marketid || betItr.getAttribute('data-selectionid') != selectionid) continue; if (betItr.classList.contains('ui-disabled')) return 'Suspended'; return betItr.getAttribute('data-last-price'); }} return 'NotInSlip'; }} GetNewOdd('{openbet.bsmId}', '{openbet.bssId}');").Replace("\"", "");
                    LogMng.Instance.onWriteStatus($"GetOddresult: {GetOddresult}");
                    newOdd = Utils.ParseToDouble(GetOddresult);
                    if (newOdd != 0)
                        break;
                    Thread.Sleep(1000);
                }
                //Thread.Sleep(5000000);
                if (newOdd == 0)
                {
                    removeBet();
                    LogMng.Instance.onWriteStatus("Get NewwOdd failed");
                    return PROCESS_RESULT.ERROR;
                }

                if (info.odds == 0)
                    info.odds = newOdd;

                if (CheckOddDropCancelBet(newOdd, info))
                {
                    removeBet();
                    return PROCESS_RESULT.SUSPENDED;
                }

                //info.odds = newOdd;
                int nSetStakeRetry = 0;
                while (nSetStakeRetry++ < 3)
                {
                    //function SetStake(marketid, selectionid, stake)
                    //{
                    //    var betList = document.querySelectorAll('ul li.ui-bet-row');
                    //    for (var i = 0; i < betList.length; i++)
                    //    {
                    //        var betItr = betList[i];
                    //        if (betItr.getAttribute('data-marketid') != marketid || betItr.getAttribute('data-selectionid') != selectionid)
                    //            continue;

                    //        if (betItr.classList.contains('ui-disabled'))
                    //            return 'Suspended';

                    //        if (betItr.querySelector('input.stake') == null)
                    //            return 'NoStakeBox';

                    //        betItr.querySelector('input.stake').value = stake;
                    //        return 'Success';
                    //    }
                    //    return 'NotInSlip';
                    //}
                    string SetStakeResult = Global.RunScriptCode($"function SetStake(marketid, selectionid, stake) {{ var betList = document.querySelectorAll('ul li.ui-bet-row'); for (var i = 0; i < betList.length; i++) {{ var betItr = betList[i]; if (betItr.getAttribute('data-marketid') != marketid || betItr.getAttribute('data-selectionid') != selectionid) continue; if (betItr.classList.contains('ui-disabled')) return 'Suspended'; if (betItr.querySelector('input.stake') == null) return 'NoStakeBox'; betItr.querySelector('input.stake').value = stake; return 'Success'; }} return 'NotInSlip'; }} SetStake('{openbet.bsmId}', '{openbet.bssId}', '{info.stake}');").Replace("\"", "");
                    LogMng.Instance.onWriteStatus($"SetStakeResult: {SetStakeResult}");
                    if (SetStakeResult == "success")
                        break;
                    Thread.Sleep(500);
                }


                int nClickBetRetry = 0;
                while (nClickBetRetry++ < 3)
                {
                    Global.strPlaceBetResult = "";
                    Global.waitResponseEvent.Reset();
                    Global.RunScriptCode("document.querySelector('form#betslip-real-form-edit').submit();");
                    if (Global.waitResponseEvent.Wait(10000))
                    {
                        if (!Global.strPlaceBetResult.Contains("\\\"resultCode\\\":\\\"SUCCESS\\\""))
                        {
                            LogMng.Instance.onWriteStatus($"Place failed Response: {Global.strPlaceBetResult}");

                            if (Global.strPlaceBetResult.Contains("\"logged out\""))
                            {
                                InnerLogin(false);
                                continue;
                            }
                            //if (Global.strPlaceBetResult.Contains("possibile piazzare le scommesse"))
                            //    return PROCESS_RESULT.CRITICAL_SITUATION;
                            double maxStake = 0, minStake = 0;
                            newOdd = 0;
                            try
                            {

                                HtmlDocument doc = new HtmlDocument();
                                //HtmlNode.ElementsFlags.Remove("form");
                                doc.LoadHtml(Global.strPlaceBetResult);

                                IEnumerable<HtmlNode> nodeForms = doc.DocumentNode.Descendants("li").Where(node => node.Attributes["data-marketId"] != null && node.Attributes["data-marketId"].Value == openbet.bsmId && node.Attributes["data-selectionId"] != null && node.Attributes["data-selectionId"].Value == openbet.bssId);
                                if (nodeForms.LongCount() > 0)
                                {
                                    string oddStr = nodeForms.ToList().ElementAt(0).Attributes["data-last-price"].Value;
                                    LogMng.Instance.onWriteStatus($"Placebet new odd: {oddStr}");
                                    newOdd = Utils.ParseToDouble(oddStr);

                                    foreach (HtmlNode linode in nodeForms)
                                    {
                                        HtmlNode maxNode = linode.SelectSingleNode(".//a[@class='set-max-stake']");
                                        if (maxNode != null)
                                        {
                                            try
                                            {
                                                LogMng.Instance.onWriteStatus($"Placebet new maxstake string: {maxNode.InnerText}");
                                                string maxstake = maxNode.InnerText.Replace("max:", "").Replace("€", "").Trim();
                                                maxStake = Utils.ParseToDouble(maxstake);
                                                LogMng.Instance.onWriteStatus($"Placebet new maxstake: {maxStake}");
                                            }
                                            catch { }
                                        }

                                        HtmlNode minNode = linode.SelectSingleNode(".//a[@class='set-min-stake']");
                                        if (minNode != null)
                                        {
                                            try
                                            {
                                                LogMng.Instance.onWriteStatus($"Placebet new minstake string: {minNode.InnerText}");
                                                string minstake = minNode.InnerText.Replace("min:", "").Replace("€", "").Trim();
                                                minStake = Utils.ParseToDouble(minstake);
                                                LogMng.Instance.onWriteStatus($"Placebet new minstake: {minStake}");
                                            }
                                            catch { }
                                            break;
                                        }
                                    }
                                }
                            }
                            catch { }

                            if (newOdd > 0)
                            {
                                if (CheckOddDropCancelBet(newOdd, info))
                                {
                                    removeBet();
                                    return PROCESS_RESULT.SUSPENDED;
                                }
                                //info.odds = newOdd;
                            }

                            LogMng.Instance.onWriteStatus($"maxstake: {maxStake} minstake: {minStake} info.stake: {info.stake}");

                            if (maxStake > 0 && minStake > 0)
                            {
                                if (maxStake < minStake)
                                {
                                    removeBet();
                                    return PROCESS_RESULT.SUSPENDED;
                                }
                                if (info.stake > maxStake)
                                {
                                    info.stake = (int)maxStake;

                                    nSetStakeRetry = 0;
                                    while (nSetStakeRetry++ < 3)
                                    {
                                        string script = $"function SetStake(marketid, selectionid, stake) {{ var betList = document.querySelectorAll('ul li.ui-bet-row'); for (var i = 0; i < betList.length; i++) {{ var betItr = betList[i]; if (betItr.getAttribute('data-marketid') != marketid || betItr.getAttribute('data-selectionid') != selectionid) continue; if (betItr.classList.contains('ui-disabled')) return 'Suspended'; if (betItr.querySelector('input.stake') == null) return 'NoStakeBox'; betItr.querySelector('input.stake').value = stake; return 'Success'; }} return 'NotInSlip'; }} SetStake('{openbet.bsmId}', '{openbet.bssId}', '{info.stake}');";
                                        //LogMng.Instance.onWriteStatus($"SetStakeResult(maxStake) script: {script}");
                                        string SetStakeResult = Global.RunScriptCode(script).Replace("\"", "");
                                        LogMng.Instance.onWriteStatus($"SetStakeResult(maxStake): {SetStakeResult}");
                                        if (SetStakeResult == "success")
                                            break;
                                        Thread.Sleep(500);
                                    }
                                }
                            }
                        }
                        else
                        {
                            return PROCESS_RESULT.PLACE_SUCCESS;
                        }
                    }
                    LogMng.Instance.onWriteStatus($"Click Placebutton retry {nClickBetRetry}");
                }
                removeBet();
                return PROCESS_RESULT.ERROR;
            }
            catch (Exception e)
            {
                removeBet();
                return PROCESS_RESULT.ERROR;
            }
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
        public void removeBet()
        {
            int nRetry = 0;
            while (nRetry++ < 3)
            {
                int nRetryButtonCount = 0;
                nRetryButtonCount = Utils.parseToInt(Global.GetStatusValue("return document.querySelectorAll('a[data-betslip-action=\"remove-all\"][class=\"remove-all-bets ui-betslip-action\"]').length;"));
                if (nRetryButtonCount <= 0)
                    break;
                Global.RunScriptCode("document.querySelectorAll('a[data-betslip-action=\"remove-all\"][class=\"remove-all-bets ui-betslip-action\"]')[0].click();");
                Thread.Sleep(500);
            }
        }

        public void removeBet(string content)
        {
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(content);

                IEnumerable<HtmlNode> betNodes = doc.DocumentNode.Descendants("a").Where(node => node.Attributes["class"] != null && node.Attributes["class"].Value == "remove-all-bets ui-betslip-action");
                if (betNodes == null || betNodes.LongCount() == 0)
                    return;

                string removeLink = string.Format("https://www.{0}{1}&lastId=1049&isAjax=true&ts={2}&alt=json", Setting.Instance.domain, betNodes.ToArray()[0].Attributes["href"].Value, Utils.getTick());
                HttpResponseMessage response = m_client.GetAsync(removeLink).Result;
                response.EnsureSuccessStatusCode();

            }
            catch (Exception e)
            {

            }

        }
        public double getBalance()
        {
            double balance = -1;
            try
            {
                m_client.DefaultRequestHeaders.Clear();
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", $"https://www.{Setting.Instance.domain}/");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-Application", "SharedSiteComponent");
                if (Setting.Instance.domain.Contains(".it"))
                    m_client.DefaultRequestHeaders.TryAddWithoutValidation("x-bf-jurisdiction", "italy");
                else
                    m_client.DefaultRequestHeaders.TryAddWithoutValidation("x-bf-jurisdiction", "INTERNATIONAL");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-Brand", "betfair");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", $"https://www.{Setting.Instance.domain}");

                string balanceUrl = "";
                if (Setting.Instance.domain.Contains(".it"))
                    balanceUrl = $"https://was.{Setting.Instance.domain}/wallet-service/v3.0/wallets?walletNames=[ITA,SPORTSBOOK_BONUS,BOOST_TOKENS]&alt=json";
                else
                    balanceUrl = $"https://was.{Setting.Instance.domain}/wallet-service/v3.0/wallets?walletNames=[MAIN,EXCHANGE_BONUS_CASH]&alt=json";
                HttpResponseMessage balanceResponseMessage = m_client.GetAsync(balanceUrl).Result;
                balanceResponseMessage.EnsureSuccessStatusCode();

                string content = balanceResponseMessage.Content.ReadAsStringAsync().Result;
                List<BalanceJson> details = new List<BalanceJson>();
                details = JsonConvert.DeserializeObject<List<BalanceJson>>(content);

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
            catch (Exception e)
            {

            }

            return balance;
        }

        public bool Pulse()
        {
            if (getBalance() < 1)
                return false;
            return true;
        }

        private void GetLiveMatchList()
        {
            try
            {
                HttpClient client = new HttpClient();
                m_client.DefaultRequestHeaders.Remove("Accept");
                m_client.DefaultRequestHeaders.Remove("X-Requested-With");
                m_client.DefaultRequestHeaders.Remove("X-Prototype-Version");
                m_client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html, application/xhtml+xml, application/xml; q=0.9, image/webp, */*; q=0.8");

                HttpResponseMessage responseMessageBetslip = client.GetAsync("https://www.betfair.it/sport/inplay").Result;
                responseMessageBetslip.EnsureSuccessStatusCode();

                string responseMessageBetfairString = responseMessageBetslip.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(responseMessageBetfairString))
                    return;

                HtmlDocument doc = new HtmlDocument();
                HtmlNode.ElementsFlags.Remove("form");
                doc.LoadHtml(responseMessageBetfairString);

                IEnumerable<HtmlNode> nodeForms = doc.DocumentNode.Descendants("div").Where(node => node.Attributes["class"] != null && node.Attributes["class"].Value == "ip-sports-selector");
                if (nodeForms == null || nodeForms.LongCount() < 1)
                {
                    return;
                }

                HtmlNode nodeForm = nodeForms.ToArray()[0];
                if (nodeForm != null)
                {
                    string text = nodeForm.SelectNodes("script")[0].InnerText;

                    JArray eventList = JsonConvert.DeserializeObject<JArray>(text);

                    Monitor.Enter(lockerObj);
                    dictEventUrl.Clear();
                    foreach (var stemData in eventList)
                    {
                        try
                        {

                            if (stemData["url"].ToString() == null)
                            {
                                continue;
                            }
                            string url = stemData["url"].ToString();
                            string eventid = url.Substring(url.LastIndexOf("/") + 1);
                            dictEventUrl[eventid] = url;
                        }
                        catch { }
                    }
                    Monitor.Exit(lockerObj);
                }
                //string action = nodeForm.GetAttributeValue("action", "");
                LogMng.Instance.onWriteStatus($"live match url refreshed: {dictEventUrl.Count}");
            }
            catch { }
        }
    }

#endif
}
