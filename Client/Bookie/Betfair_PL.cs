namespace Project.Bookie
{
#if (BETFAIR_PL)
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
    class BetfairCtrl : IBookieController
    {
        Dictionary<string, string> dictEventUrl = new Dictionary<string, string>();
        Object lockerObj = new object();

        public HttpClient m_client = null;
        public string sessionToken = null;

        private IPlaywright playwright;
        private IBrowser browser = null;
        private IPage page = null;

        private ManualResetEventSlim wait_PlacebetResultEvent = new ManualResetEventSlim();
        private string wait_PlacebetResult = string.Empty;
        private ManualResetEventSlim wait_PlacebetExecuteEvent = new ManualResetEventSlim();

        private double balance = -1;
        public string appkey = "";
        public BetfairCtrl()
        {
            m_client = initHttpClient();

            Playwright.InstallAsync().Wait();     
        }

        private void RunBrowser()
        {
            //create browser again and login (sometimes, mouse , keyboard is not working)
            try
            {                
                if (page != null)
                    page.CloseAsync().Wait();

                if (browser != null)
                    browser.CloseAsync().Wait();
            }
            catch { }

            playwright = Playwright.CreateAsync().Result;
            browser = playwright.Firefox.LaunchAsync(false).Result;

            var _context = browser.NewContextAsync(new ViewportSize() { Width = 1000, Height = 600 }).Result;
            _context.GrantPermissionsAsync(new ContextPermission[1] { ContextPermission.Geolocation }).Wait();

            //string content = File.ReadAllText("mouse.js");
            //_context.AddInitScriptAsync(content, path : "mouse.js");

            //_context.AddInitScriptAsync($"function GetNewOdd(marketid, selectionid) {{ var betList = document.querySelectorAll('ul li.ui-bet-row'); for (var i = 0; i < betList.length; i++) {{ var betItr = betList[i]; if (betItr.getAttribute('data-marketid') != marketid || betItr.getAttribute('data-selectionid') != selectionid) continue; if (betItr.classList.contains('ui-disabled')) return 'Suspended'; return betItr.getAttribute('data-last-price'); }} return 'NotInSlip'; }}");
            //_context.AddInitScriptAsync($"function SetStake(marketid, selectionid, stake) {{ var betList = document.querySelectorAll('ul li.ui-bet-row'); for (var i = 0; i < betList.length; i++) {{ var betItr = betList[i]; if (betItr.getAttribute('data-marketid') != marketid || betItr.getAttribute('data-selectionid') != selectionid) continue; if (betItr.classList.contains('ui-disabled')) return 'Suspended'; if (betItr.querySelector('input.stake') == null) return 'NoStakeBox'; betItr.querySelector('input.stake').value = stake; return 'Success'; }} return 'NotInSlip'; }}");

            page = _context.NewPageAsync().Result;
            page.Response += Page_Response;            
        }

        private async void Page_Response(object sender, ResponseEventArgs e)
        {
            if (e.Response.Status != HttpStatusCode.OK)
            {
                return;
            }

            try
            {
                await e.Response.FinishedAsync();

                if (e.Response.Url.ToLower().Contains("action=confirm") || e.Response.Url.ToLower().Contains("action=place"))
                {
                    wait_PlacebetResult = await e.Response.GetTextAsync();
                    wait_PlacebetResultEvent.Set();
                }
                else if (e.Response.Url.ToLower().Contains("/wallets?walletnames="))
                {
                    //LogMng.Instance.onWriteStatus("Balance url is called");
                    string reponse = await e.Response.GetTextAsync();
                    if (reponse.Contains("SUCCESS"))
                    {
                        List<BalanceJson> details = new List<BalanceJson>();
                        details = JsonConvert.DeserializeObject<List<BalanceJson>>(reponse);

                        if (Setting.Instance.domain.Contains(".it"))
                        {
                            BalanceJson mainJson = details.FirstOrDefault(node => node.walletName == "ITA");
                            if (mainJson != null)
                                balance = mainJson.details.availabletobet;
                        }
                        else
                        {
                            BalanceJson mainJson = details.FirstOrDefault(node => node.walletName == "MAIN");
                            if (mainJson != null)
                                balance = mainJson.details.amount;
                        }

                        LogMng.Instance.onWriteStatus($"Balance : {balance}");
                    }
                }
                else if (e.Response.Url.ToLower().Contains($"https://www.{Setting.Instance.domain}/sport/"))
                {
                    string reponse = await e.Response.GetTextAsync();
                    if (reponse.Contains("window.accountId = \"123456\";"))
                    {
                        //LogMng.Instance.onWriteStatus("Not Logined");
                        balance = -1;
                    }
                    else
                    {
                        //LogMng.Instance.onWriteStatus("Already Logined");
                    }
                }
            }
            catch { }

        }
        public void Close()
        {
            try
            {
                page.CloseAsync().Wait();
                browser.CloseAsync().Wait();
            }
            catch { }
                        
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
            //try
            //{
            //    HttpResponseMessage resp = m_client.GetAsync("http://lumtest.com/myip.json").Result;
            //    var strContent = resp.Content.ReadAsStringAsync().Result;
            //    var payload = JsonConvert.DeserializeObject<dynamic>(strContent);
            //    return payload.ip.ToString() + " - " + payload.country.ToString();
            //}
            //catch (Exception ex)
            //{
            //}
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

        private string RunFunction(string function, object param)
        {
            try
            {
                string result = page.EvaluateAsync(function, param).Result.ToString();
                return result;
            }
            catch (Exception  ex)
            { }
            return "error";
        }
        private string RunScript(string param)
        {
            string result = "";
            try
            {
                result = page.EvaluateAsync(param).Result.ToString().ToLower();
            }
            catch (Exception ex)
            { }
            return result;
        }
        public bool InnerLogin(bool bRemovebet = true)
        {

            RunBrowser();
            bool bLogin = false;
            try
            {
                try
                {
                    Monitor.Enter(lockerObj);
                    //m_client = initHttpClient();



                    LogMng.Instance.onWriteStatus($"Betfair login Start");
                    page.GoToAsync($"https://www.{Setting.Instance.domain}/sport/").Wait();


                    Thread.Sleep(500);
                    //LogMng.Instance.onWriteStatus($"login step 1");
                    RunScript("document.getElementById('onetrust-accept-btn-handler').click();");

                    //LogMng.Instance.onWriteStatus($"login step 2 {eleLoginBtn}");

                    Thread.Sleep(1000);

                    RunScript($"document.getElementById('ssc-liu').value='{Setting.Instance.username}';");

                    RunScript($"document.getElementById('ssc-lipw').value='{Setting.Instance.password}';");

                    Thread.Sleep(500);

                    RunScript("document.getElementById('ssc-lis').click();");

                    
                    Thread.Sleep(3000);

                    //for (int p = 0; p < browser.Contexts.Length; p++)
                    //{
                    //    List<NetworkCookie> cookieList = browser.Contexts[p].GetCookiesAsync($"https://www.{Setting.Instance.domain}").Result.ToList();

                    //    for (int i = 0; i < cookieList.Count; i++)
                    //    {
                    //        //LogMng.Instance.onWriteStatus($"{cookieList[i].Domain} - {cookieList[i].Path} - {cookieList[i].Name} - {cookieList[i].Value}");
                    //        try
                    //        {
                    //            Global.cookieContainer.Add(new System.Net.Cookie(cookieList[i].Name, cookieList[i].Value, cookieList[i].Path, cookieList[i].Domain));
                    //        }
                    //        catch (Exception ex) { }
                    //    }
                    //}
                    //Task.Run(async () => await Global.GetCookie()).Wait();
                    if (bRemovebet)
                    {
                        removeBet();
                        Feature();
                    }
                    //click accept button
                    RunScript("document.getElementById('onetrust-accept-btn-handler').click();");
                                                          
                    bLogin = true;
                }
                catch { }
                finally
                {
                    Monitor.Exit(lockerObj);
                }
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("System.Threading.Tasks.Task.Wait"))
                {
                    RunBrowser();
                }
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
            //LogMng.Instance.onWriteStatus(string.Format("Place bet 1"));
            BetResult br = new BetResult();

            try
            {
                OpenBet_Betfair openbet = Utils.ConvertBetburgerPick2OpenBet_Betfair(info);
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 2"));
                if (openbet == null)
                {
                    LogMng.Instance.onWriteStatus("Converting OpenBet Error");
                    return PROCESS_RESULT.ERROR;
                }
                LogMng.Instance.onWriteStatus(string.Format("Place bet 3 eventId: {0}", openbet.eventId));
                string eventUrl = "";
                try
                {
                    Monitor.Enter(lockerObj);
                    if (dictEventUrl.ContainsKey(openbet.eventId))
                    {
                        //LogMng.Instance.onWriteStatus(string.Format("Place bet 4"));
                        eventUrl = dictEventUrl[openbet.eventId];
                    }
                }
                catch { }
                finally
                {
                    Monitor.Exit(lockerObj);
                }
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 5 eventurl: {0}",eventUrl));
                if (string.IsNullOrEmpty(eventUrl))
                {
                    LogMng.Instance.onWriteStatus("Cannot find EventUrl");
                    return PROCESS_RESULT.ERROR;
                }
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 6"));
                page.GoToAsync(eventUrl + "?selectedGroup=all_markets").Wait();
                
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 7"));

                //string loginuserDiv = RunScript("document.getElementsByClassName('ssc-wl ssc-wlco')[0].outerHTML");
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 22 - {0}", loginuserDiv));
                //if (!loginuserDiv.Contains("div"))
                double curBal = getBalance();
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 7.1 check login balance - {0}", curBal));
                if (curBal < 0)
                {
                    LogMng.Instance.onWriteStatus("[Placebet]Login because of logout already");
                    if (!InnerLogin(true))
                        return PROCESS_RESULT.NO_LOGIN;
                    page.GoToAsync(eventUrl + "?selectedGroup=all_markets").Wait();
                }
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 8"));
                int nMarketClickRetry = 0;
                while(nMarketClickRetry++ < 3)
                {
                    //LogMng.Instance.onWriteStatus(string.Format("Place bet 9 - {0}", nMarketClickRetry));
                    int nMarketCount = Utils.parseToInt(RunScript($"document.querySelectorAll('a[data-eventid=\"{openbet.bseId}\"][data-marketid=\"{openbet.bsmId}\"][data-selectionid=\"{openbet.bssId}\"][data-betslip-action=\"add-bet\"]').length"));
                    LogMng.Instance.onWriteStatus(string.Format("Place bet nMarketCount count - {0}", nMarketCount));
                    if (nMarketCount <= 0)
                        Thread.Sleep(500);

                    RunScript($"document.querySelectorAll('a[data-eventid=\"{openbet.bseId}\"][data-marketid=\"{openbet.bsmId}\"][data-selectionid=\"{openbet.bssId}\"][data-betslip-action=\"add-bet\"]')[0].click();");
                    //LogMng.Instance.onWriteStatus(string.Format("Place bet 10 - {0}", nMarketClickRetry));
                    break;
                }
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 11"));
                double newOdd = 0;
                int nGetOddRetry = 0;
                while (nGetOddRetry++ < 3)
                {
                    //LogMng.Instance.onWriteStatus(string.Format("Place bet 12 - {0}", nGetOddRetry));
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
                    //string GetOddresult = RunScript($"function GetNewOdd(marketid, selectionid) {{ var betList = document.querySelectorAll('ul li.ui-bet-row'); for (var i = 0; i < betList.length; i++) {{ var betItr = betList[i]; if (betItr.getAttribute('data-marketid') != marketid || betItr.getAttribute('data-selectionid') != selectionid) continue; if (betItr.classList.contains('ui-disabled')) return 'Suspended'; return betItr.getAttribute('data-last-price'); }} return 'NotInSlip'; }} GetNewOdd('{openbet.bsmId}', '{openbet.bssId}')").Replace("\"", "");
                    string funcGetOddResult = $"jsonParam => {{ var param = JSON.parse(jsonParam); var betList = document.querySelectorAll('ul li.ui-bet-row'); for (var i = 0; i < betList.length; i++) {{ var betItr = betList[i]; if (betItr.getAttribute('data-marketid') != param.marketid || betItr.getAttribute('data-selectionid') != param.selectionid) continue; if (betItr.classList.contains('ui-disabled')) return 'Suspended'; return betItr.getAttribute('data-last-price'); }} return 'NotInSlip'; }}";
                    string GetOddresult = RunFunction(funcGetOddResult, $"{{\"marketid\":\"{openbet.bsmId}\", \"selectionid\":\"{openbet.bssId}\"}}").Replace("\"", "");
                    LogMng.Instance.onWriteStatus($"GetOddresult: {GetOddresult}");
                    newOdd = Utils.ParseToDouble(GetOddresult);
                    if (newOdd != 0)
                    {
                        //LogMng.Instance.onWriteStatus(string.Format("Place bet 13"));
                        break;
                    }
                    Thread.Sleep(1000);
                }
                //Thread.Sleep(5000000);
                if (newOdd == 0)
                {
                    //LogMng.Instance.onWriteStatus(string.Format("Place bet 14"));
                    removeBet();
                    LogMng.Instance.onWriteStatus("Get NewwOdd failed");
                    return PROCESS_RESULT.ERROR;
                }

                if (info.odds == 0)
                    info.odds = newOdd;
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 15"));
                if (CheckOddDropCancelBet(newOdd, info))
                {
                    //LogMng.Instance.onWriteStatus(string.Format("Place bet 16"));
                    removeBet();
                    return PROCESS_RESULT.SUSPENDED;
                }
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 17"));
                //info.odds = newOdd;
                int nSetStakeRetry = 0;
                while (nSetStakeRetry++ < 3)
                {
                    //LogMng.Instance.onWriteStatus(string.Format("Place bet 18 - {0}", nSetStakeRetry));
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
                    //string SetStakeResult = RunScript($"function SetStake(marketid, selectionid, stake) {{ var betList = document.querySelectorAll('ul li.ui-bet-row'); for (var i = 0; i < betList.length; i++) {{ var betItr = betList[i]; if (betItr.getAttribute('data-marketid') != marketid || betItr.getAttribute('data-selectionid') != selectionid) continue; if (betItr.classList.contains('ui-disabled')) return 'Suspended'; if (betItr.querySelector('input.stake') == null) return 'NoStakeBox'; betItr.querySelector('input.stake').value = stake; return 'Success'; }} return 'NotInSlip'; }} SetStake('{openbet.bsmId}', '{openbet.bssId}', '{info.stake}');").Replace("\"", "");
                    string funcSetStakeResult = $"jsonParam => {{ var param = JSON.parse(jsonParam); var betList = document.querySelectorAll('ul li.ui-bet-row'); for (var i = 0; i < betList.length; i++) {{ var betItr = betList[i]; if (betItr.getAttribute('data-marketid') != param.marketid || betItr.getAttribute('data-selectionid') != param.selectionid) continue; if (betItr.classList.contains('ui-disabled')) return 'Suspended'; if (betItr.querySelector('input.stake') == null) return 'NoStakeBox'; betItr.querySelector('input.stake').value = param.stake; return 'Success'; }} return 'NotInSlip'; }}";
                    string SetStakeResult = RunFunction(funcSetStakeResult, $"{{\"marketid\":\"{openbet.bsmId}\", \"selectionid\":\"{openbet.bssId}\", \"stake\":\"{info.stake}\"}}").Replace("\"", "").ToLower();
                    LogMng.Instance.onWriteStatus($"SetStakeResult: {SetStakeResult}");
                    if (SetStakeResult == "success")
                    {
                        //LogMng.Instance.onWriteStatus(string.Format("Place bet 19"));
                        break;
                    }
                    Thread.Sleep(500);
                }

                //LogMng.Instance.onWriteStatus(string.Format("Place bet 20"));
                int nClickBetRetry = 0;
                while (nClickBetRetry++ < 3)
                {
                    //LogMng.Instance.onWriteStatus(string.Format("Place bet 21 - {0}", nClickBetRetry));

                    wait_PlacebetResult = "";
                    wait_PlacebetResultEvent.Reset();

                    string confirmbetBut = RunScript("document.getElementsByClassName('confirm-bets-button ui-betslip-action')[0].outerHTML");
                    //LogMng.Instance.onWriteStatus(string.Format("Place bet 22 - {0}", confirmbetBut));
                    if (confirmbetBut.Contains("button"))
                    {
                        RunScript("document.getElementsByClassName('confirm-bets-button ui-betslip-action')[0].click();");
                    }
                    else
                    {
                        RunScript("document.querySelector('form#betslip-real-form-edit').submit();");
                    }

                    //LogMng.Instance.onWriteStatus(string.Format("Place bet 23"));
                    if (wait_PlacebetResultEvent.Wait(10000))
                    {
                        //LogMng.Instance.onWriteStatus(string.Format("Place bet 24"));
                        if (!wait_PlacebetResult.Contains("\\\"resultCode\\\":\\\"SUCCESS\\\""))
                        {
                            //LogMng.Instance.onWriteStatus(string.Format("Place bet 25 {0}", wait_PlacebetResult));
                            //LogMng.Instance.onWriteStatus($"Place failed Response: {wait_PlacebetResult}");

                            if (wait_PlacebetResult.Contains("\"logged out\""))
                            {
                                //LogMng.Instance.onWriteStatus(string.Format("Place bet 26"));
                                InnerLogin(false);
                                continue;
                            }
                            //if (wait_PlacebetResult.Contains("possibile piazzare le scommesse"))
                            //    return PROCESS_RESULT.CRITICAL_SITUATION;
                            double maxStake = 0, minStake = 0;
                            newOdd = 0;
                            try
                            {
                                //LogMng.Instance.onWriteStatus(string.Format("Place bet 27"));
                                HtmlDocument doc = new HtmlDocument();
                                //HtmlNode.ElementsFlags.Remove("form");
                                doc.LoadHtml(wait_PlacebetResult);
                                //LogMng.Instance.onWriteStatus(string.Format("Place bet 28"));
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
                                                LogMng.Instance.onWriteStatus($"Placebet new maxstake: {maxStake} str: {maxstake}");

                                                if (maxStake <= 0)
                                                    maxStake = 999;
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
                                                LogMng.Instance.onWriteStatus($"Placebet new minstake: {minStake} str: {minstake}");
                                            }
                                            catch { }
                                            //LogMng.Instance.onWriteStatus($"Place bet 28.1");
                                            break;
                                        }
                                    }
                                }
                            }
                            catch { }
                            //LogMng.Instance.onWriteStatus(string.Format("Place bet 29 newOdd  {0}", newOdd));
                            if (newOdd > 0)
                            {
                                if (CheckOddDropCancelBet(newOdd, info))
                                {
                                    //LogMng.Instance.onWriteStatus(string.Format("Place bet 30"));
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
                                    //LogMng.Instance.onWriteStatus(string.Format("Place bet 31"));
                                    removeBet();
                                    return PROCESS_RESULT.SUSPENDED;
                                }
                                if (info.stake > maxStake)
                                {
                                    info.stake = (int)maxStake;

                                    nSetStakeRetry = 0;
                                    while (nSetStakeRetry++ < 3)
                                    {
                                        //string script = $"function SetStake(marketid, selectionid, stake) {{ var betList = document.querySelectorAll('ul li.ui-bet-row'); for (var i = 0; i < betList.length; i++) {{ var betItr = betList[i]; if (betItr.getAttribute('data-marketid') != marketid || betItr.getAttribute('data-selectionid') != selectionid) continue; if (betItr.classList.contains('ui-disabled')) return 'Suspended'; if (betItr.querySelector('input.stake') == null) return 'NoStakeBox'; betItr.querySelector('input.stake').value = stake; return 'Success'; }} return 'NotInSlip'; }} SetStake('{openbet.bsmId}', '{openbet.bssId}', '{info.stake}');";
                                        string funcSetStakeResult = $"jsonParam => {{ var param = JSON.parse(jsonParam); var betList = document.querySelectorAll('ul li.ui-bet-row'); for (var i = 0; i < betList.length; i++) {{ var betItr = betList[i]; if (betItr.getAttribute('data-marketid') != param.marketid || betItr.getAttribute('data-selectionid') != param.selectionid) continue; if (betItr.classList.contains('ui-disabled')) return 'Suspended'; if (betItr.querySelector('input.stake') == null) return 'NoStakeBox'; betItr.querySelector('input.stake').value = param.stake; return 'Success'; }} return 'NotInSlip'; }}";
                                        string SetStakeResult = RunFunction(funcSetStakeResult, $"{{\"marketid\":\"{openbet.bsmId}\", \"selectionid\":\"{openbet.bssId}\", \"stake\":\"{info.stake}\"}}").Replace("\"", "").ToLower();


                                        //LogMng.Instance.onWriteStatus($"SetStakeResult(maxStake) script: {script}");
                                        //string SetStakeResult = RunScript(script).Replace("\"", "");
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
                            //LogMng.Instance.onWriteStatus(string.Format("Place bet 32"));
                            return PROCESS_RESULT.PLACE_SUCCESS;
                        }
                    }
                    
                    LogMng.Instance.onWriteStatus($"Click Placebutton retry {nClickBetRetry}");
                }
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 33"));
                removeBet();
                return PROCESS_RESULT.ERROR;              
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("System.Threading.Tasks.Task.Wait"))
                {
                    RunBrowser();
                }
                //LogMng.Instance.onWriteStatus(string.Format("Place bet 34"));
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
            while( nRetry++ < 3)
            {
                int nRetryButtonCount = 0;
                nRetryButtonCount = Utils.parseToInt(RunScript("document.querySelectorAll('a[data-betslip-action=\"remove-all\"][class=\"remove-all-bets ui-betslip-action\"]').length"));
                if (nRetryButtonCount <= 0)
                    break;
                RunScript("document.querySelectorAll('a[data-betslip-action=\"remove-all\"][class=\"remove-all-bets ui-betslip-action\"]')[0].click();");
                Thread.Sleep(500);
            }
        }

        public double getBalance()
        {
            return balance;    
            //double balance = -1;

            //int nRetry = 0;
            //while (nRetry++ < 2)
            //{
            //    wait_PlacebetResultEvent.Reset();

            //    RunScript("document.getElementsByClassName('ssc-rl ssc-hdn-nojs').click();");
                                
            //    if (wait_PlacebetResultEvent.Wait(5000))
            //    {
                    
            //        if (wait_PlacebetResult.Contains("SUCCESS"))
            //        {
            //            List<BalanceJson> details = new List<BalanceJson>();
            //            details = JsonConvert.DeserializeObject<List<BalanceJson>>(wait_PlacebetResult);

            //            if (Setting.Instance.domain.Contains(".it"))
            //            {
            //                BalanceJson mainJson = details.FirstOrDefault(node => node.walletName == "ITA");
            //                if (mainJson == null)
            //                    return balance;

            //                balance = mainJson.details.availabletobet;
            //            }
            //            else
            //            {
            //                BalanceJson mainJson = details.FirstOrDefault(node => node.walletName == "MAIN");
            //                if (mainJson == null)
            //                    return balance;

            //                balance = mainJson.details.amount;
            //            }

            //            if (balance >= 0)
            //                break;
            //        }
            //    }
            //}    
            //try
            //{
            //    m_client.DefaultRequestHeaders.Clear();
            //    m_client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            //    m_client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36");
            //    m_client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            //    m_client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", $"https://www.{Setting.Instance.domain}/");
            //    m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-Application", "SharedSiteComponent");
            //    if (Setting.Instance.domain.Contains(".it"))
            //        m_client.DefaultRequestHeaders.TryAddWithoutValidation("x-bf-jurisdiction", "italy");
            //    else
            //        m_client.DefaultRequestHeaders.TryAddWithoutValidation("x-bf-jurisdiction", "INTERNATIONAL");
            //    m_client.DefaultRequestHeaders.TryAddWithoutValidation("X-Brand", "betfair");
            //    m_client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", $"https://www.{Setting.Instance.domain}");

            //    string balanceUrl = "";
            //    if (Setting.Instance.domain.Contains(".it"))
            //        balanceUrl = $"https://was.{Setting.Instance.domain}/wallet-service/v3.0/wallets?walletNames=[ITA,SPORTSBOOK_BONUS,BOOST_TOKENS]&alt=json"; 
            //    else
            //        balanceUrl = $"https://was.{Setting.Instance.domain}/wallet-service/v3.0/wallets?walletNames=[MAIN,EXCHANGE_BONUS_CASH]&alt=json";
            //    HttpResponseMessage balanceResponseMessage = m_client.GetAsync(balanceUrl).Result;
            //    balanceResponseMessage.EnsureSuccessStatusCode();

               
            //}
            //catch (Exception e)
            //{

            //}

        }

        public bool Pulse()
        {
            if (getBalance() < 1)
                return false;
            return true;
        }

        private void GetLiveMatchList()
        {
            //LogMng.Instance.onWriteStatus($"GetLiveMatchList 1");
            try
            {
                HttpClient client = new HttpClient();

                HttpResponseMessage responseMessageBetslip = client.GetAsync($"https://www.{Setting.Instance.domain}/sport/inplay").Result;
                responseMessageBetslip.EnsureSuccessStatusCode();
                //LogMng.Instance.onWriteStatus($"GetLiveMatchList 2");
                string responseMessageBetfairString = responseMessageBetslip.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(responseMessageBetfairString))
                {
                    //LogMng.Instance.onWriteStatus($"GetLiveMatchList 3");
                    return;
                }

                HtmlDocument doc = new HtmlDocument();
                HtmlNode.ElementsFlags.Remove("form");
                doc.LoadHtml(responseMessageBetfairString);
                //LogMng.Instance.onWriteStatus($"GetLiveMatchList 4");
                IEnumerable<HtmlNode> nodeForms = doc.DocumentNode.Descendants("div").Where(node => node.Attributes["class"] != null && node.Attributes["class"].Value == "ip-sports-selector");
                if (nodeForms == null || nodeForms.LongCount() < 1)
                {
                    //LogMng.Instance.onWriteStatus($"GetLiveMatchList 5");
                    return;
                }
                //LogMng.Instance.onWriteStatus($"GetLiveMatchList 6");
                HtmlNode nodeForm = nodeForms.ToArray()[0];
                if (nodeForm != null)
                {
                    string text = nodeForm.SelectNodes("script")[0].InnerText;

                    JArray eventList = JsonConvert.DeserializeObject<JArray>(text);
                    //LogMng.Instance.onWriteStatus($"GetLiveMatchList 7");
                    try
                    {
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
                    }
                    catch
                    {

                    }
                    finally
                    {
                        Monitor.Exit(lockerObj);
                    }
                    //LogMng.Instance.onWriteStatus($"GetLiveMatchList 8");
                }
                //string action = nodeForm.GetAttributeValue("action", "");
                LogMng.Instance.onWriteStatus($"live match url refreshed: {dictEventUrl.Count}");
            }
            catch { }
        }
    }
#endif
}
