//using MasterDevs.ChromeDevTools;
//using MasterDevs.ChromeDevTools.Protocol.Chrome.DOM;
//using MasterDevs.ChromeDevTools.Protocol.Chrome.Input;
//using MasterDevs.ChromeDevTools.Protocol.Chrome.Network;
//using MasterDevs.ChromeDevTools.Protocol.Chrome.Page;
//using MasterDevs.ChromeDevTools.Protocol.Chrome.Runtime;
namespace Project.Bookie
{
    public enum TaskType
    {
        None,
        Login,
        Addbet,
        Placebet,
        Openbet,
        GetBalance,
        RefreshPage,
    }

    public class TaskParam
    {
        public TaskType type;
        public string f;
        public string fp;
        public string o;
        public string st;
        public string tr;
    }
#if (BET365_CHROMEDEV)
    public class BET365_CHROMEDEVCtrl : IBookieController
    {
        //communication with page Thread.
        string domain = "";
        Object lockerObj = new object();
        long documentNodeId = -1;
        private string stake_pos = string.Empty;
        private decimal _X_stake = 0;
        private decimal _Y_stake = 0;
        private string placebetBtn_pos = string.Empty;
        public BET365_CHROMEDEVCtrl()
        {
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"Bet365_BMCtrl Start");
#endif

            domain = $"https://www.{Setting.Instance.domain}/";
            if (CDPController.Instance._browserObj == null)
                CDPController.Instance.InitializeBrowser(domain);
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
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.142 Safari/537.36");


            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"90\", \"Google Chrome\";v=\"90\"");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");

            httpClientEx.DefaultRequestHeaders.Add("Host", $"{domain}");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Referer", $"https://{domain}/");

            return httpClientEx;
        }

        public bool login()
        {

            domain = $"https://www.{Setting.Instance.domain}/"; 
            if (CDPController.Instance._browserObj != null)
            {
                if (getBalance() > 0)
                    return true;
            }

            int nTotalRetry = 0;
            bool bLogin = false;
            while (nTotalRetry++ < 2)
            {
                if (!Global.bRun)
                    return false;
                try
                {
                    CDPController.Instance.ReloadBrowser();
                    Thread.Sleep(10000);
                    if (CheckIfLogged()) return false;

                    CDPController.Instance.WaitingForLogin = true;

                    documentNodeId = CDPController.Instance.GetDocumentId().Result;
                    bool isFound = CDPController.Instance.FindElement(documentNodeId, "div[class='hm-MainHeaderRHSLoggedOutMed_Login ']").Result;
                    if (isFound)
                    {
                        CDPController.Instance.FindAndClickElement(documentNodeId, "div[class='hm-MainHeaderRHSLoggedOutMed_Login ']", 1, MoveMethod.SQRT).Wait();
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        isFound = CDPController.Instance.FindElement(documentNodeId, "div[class='hm-MainHeaderRHSLoggedOutWide_Login ']").Result;
                        if (isFound)
                        {
                            CDPController.Instance.FindAndClickElement(documentNodeId, "div[class='hm-MainHeaderRHSLoggedOutWide_Login ']", 1, MoveMethod.SQRT).Wait();
                            Thread.Sleep(1000);
                        }
                    }

                    //Input Username
                    isFound = CDPController.Instance.FindElement(documentNodeId, "input[class='lms-StandardLogin_Username ']").Result;
                    if (isFound)
                    {
                        CDPController.Instance.FindAndClickElement(documentNodeId, "input[class='lms-StandardLogin_Username ']", 3, MoveMethod.SQRT).Wait();
                        CDPMouseController.Instance.InputText(Setting.Instance.username);
                        Thread.Sleep(1000);
                    }

                    //Input Password
                    isFound = CDPController.Instance.FindElement(documentNodeId, "input[class='lms-StandardLogin_Password ']").Result;
                    if (isFound)
                    {
                        CDPController.Instance.FindAndClickElement(documentNodeId, "input[class='lms-StandardLogin_Password ']", 3, MoveMethod.SQRT).Wait();
                        CDPMouseController.Instance.InputText(Setting.Instance.password);
                        Thread.Sleep(1000);
                    }

                    //Click Login Button
                    isFound = CDPController.Instance.FindElement(documentNodeId, "div[class='lms-LoginButton ']").Result;
                    if (isFound)
                    {
                        CDPController.Instance.FindAndClickElement(documentNodeId, "div[class='lms-LoginButton ']", 2, MoveMethod.SQRT).Wait();
                        Thread.Sleep(1000);
                    }

                    int wCnt = 0;
                    while (CDPController.Instance.WaitingForLogin && !CheckIfLogged())
                    {
                        wCnt++;
                        Thread.Sleep(200);
                        if (wCnt >= 100)
                            break;
                    }

                    Thread.Sleep(5000);
                    if (!CheckIfLogged())
                    {
                        LogMng.Instance.onWriteStatus("Login Failed!");
                        bLogin = false;
                        continue;
                    }

                    //쿠키버튼을 클릭
                    isFound = CDPController.Instance.FindElement(documentNodeId, "div[class='ccm-CookieConsentPopup_Accept ']").Result;
                    if (isFound)
                    {
                        CDPController.Instance.FindAndClickElement(documentNodeId, "div[class='ccm-CookieConsentPopup_Accept ']", 1, MoveMethod.SQRT).Wait();
                        Thread.Sleep(1000);
                    }
                    LogMng.Instance.onWriteStatus("Logged In Successfully!");
                    bLogin = true;

                    break;
                }
                catch (Exception e)
                {
                    LogMng.Instance.onWriteStatus($"login exception {e.StackTrace} {e.Message}");
                }
            }
            return bLogin;
        }
        public bool CheckIfLogged()
        {
            try
            {
                string jsResult = CDPController.Instance.ExecuteScript("flashvars.USER_NAME", true);
                if (string.IsNullOrEmpty(jsResult)) return false;
                if (jsResult.ToLower().Contains(Setting.Instance.username.ToLower()))
                    return true;
            }
            catch {}
            return false;
        }
        public bool logout()
        {
            return true;
        }
        public string getProxyLocation()
        {
            try
            {
                HttpClient m_client = new HttpClient();
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
        public double getBalance()
        {
            double balance = -1;
            int retryCnt = 10;
            while (retryCnt-- > 0)
            {
                try
                {
                    lock (lockerObj)
                    {
                        string strBalance = CDPController.Instance.ExecuteScript("Locator.user._balance.totalBalance", true);
                        if (string.IsNullOrEmpty(strBalance) || strBalance == "null" || strBalance == "undefined") return -1;
                        strBalance = strBalance.Replace("\"", "").Replace("'", "");
                        strBalance = strBalance.Substring(0, strBalance.Length - 1);
                        balance = Utils.ParseToDouble(strBalance.Replace(",", "").Replace(",", "."));
                        if (balance > -1)
                            break;

                        Thread.Sleep(500);
                    }
                }
                catch { }
            }
            return balance;
        }
        public PROCESS_RESULT PlaceBet(List<BetburgerInfo> infos, out List<PROCESS_RESULT> result)
        {
            result = null;
            return PROCESS_RESULT.ERROR;
        }
        public PROCESS_RESULT PlaceBet(ref BetburgerInfo info)
        {
            Thread.Sleep(1000 * Setting.Instance.requestDelay);

            if (getBalance() < 0)
            {
                if (!login())
                    return PROCESS_RESULT.NO_LOGIN;
            }

#if OXYLABS
            retryCount = 5;
#endif

            try
            {
                lock (lockerObj)
                {
                    Removebet();
                    string fi = string.Empty, fd = string.Empty, odds = string.Empty;
                    string direct_link = Utils.Between("xx" + info.direct_link, "xx", ";origin_feed_bk_id");
                    if (!direct_link.Contains("ID="))
                    {
                        string[] arr = direct_link.Split('|');
                        fi = arr[2];

                        odds = info.odds.ToString();
                        fd = arr[0];
                    }
                    else
                    {
                        string[] arr = direct_link.Split('&');
                        foreach(string str in arr)
                        {
                            if (str.Contains("ID="))
                                fd = str.Replace("ID=", string.Empty).Trim();
                            else if (str.Contains("FI="))
                                fi = str.Replace("FI=", string.Empty).Trim();
                            else if (str.Contains("OD="))
                                odds = str.Replace("OD=", string.Empty).Trim();
                        }
                    }

                    double balance = getBalance();
                    LogMng.Instance.onWriteStatus("Current Balance : " + balance.ToString());

                    bool bLogin = CheckIfLogged();
                    if (!bLogin)
                    {
                        login();
                        Thread.Sleep(500);
                    }
                    else
                        LogMng.Instance.onWriteStatus("Logged In Already!");

                    DoClickDlgBox();

                    //Go To Live page
                    string page_url = CDPController.Instance.ExecuteScript("ns_navlib_util.WebsiteNavigationManager.CurrentPageData", true);
                    if (!page_url.Contains("IP"))
                        CDPController.Instance.NavigateInvoke($"{domain}#/IP/B1");

                    //Addbet
                    string addbetRespBody = DoAddBetUI(fi, fd, odds);
                    dynamic jsonSlipResponse = new JObject();
                    if (string.IsNullOrEmpty(addbetRespBody))
                    {
                        try
                        {
                            int betLen1 = Utils.parseToInt(CDPController.Instance.ExecuteScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets.length", true));
                            if (betLen1 == 0)
                            {
                                Removebet();
                                return PROCESS_RESULT.ERROR;
                            }

                            if (!IsBetSlipOpened())
                            {
                                CDPController.Instance.ReloadBrowser();          
                                return PROCESS_RESULT.ERROR;
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        int betLen = Utils.parseToInt(CDPController.Instance.ExecuteScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets.length", true));
                        if(betLen > 1)
                        {
                            Removebet();
                            return PROCESS_RESULT.ERROR;
                        }

                        if (!IsBetSlipOpened())
                        {
                            CDPController.Instance.ReloadBrowser();
                            return PROCESS_RESULT.ERROR;
                        }

                        jsonSlipResponse = JsonConvert.DeserializeObject<dynamic>(addbetRespBody);
                        try
                        {

                            string strSU = jsonSlipResponse.bt[0].su.ToString();
                            if (strSU.ToLower() == "true")
                            {
                                Removebet();
                                return PROCESS_RESULT.SUSPENDED;
                            }

                            string strMD = jsonSlipResponse.bt[0].pt[0].md.ToString();
                            if (strMD.ToLower() == "")
                            {
                                Removebet();
                                return PROCESS_RESULT.SUSPENDED;
                            }
                        }
                        catch { }

                        try
                        {
                            string strOdds = jsonSlipResponse.bt[0].od.ToString();
                            double newOdds = GetBetslipOdds();
                            if (CheckOddDropCancelBet(newOdds, info))
                            {
                                Removebet();
                                return PROCESS_RESULT.ERROR;
                            }
                        }
                        catch { }

                        string betRespCode = jsonSlipResponse.sr.ToString();
                        if (betRespCode == "-1" || betRespCode == "15")
                        {
                            Removebet();
                            LogMng.Instance.onWriteStatus(string.Format("[{0}] Reason: addbet response code => {1}", DateTime.Now, betRespCode));
                            return PROCESS_RESULT.SUSPENDED;
                        }

                        BetSlipJson betSlipJson = JsonConvert.DeserializeObject<BetSlipJson>(addbetRespBody);
                        foreach (BetSlipItem betSlipItem in betSlipJson.bt)
                        {
                            if (betSlipItem.pt[0].pi == fd)
                            {
                                if (betSlipItem.su)
                                {
                                    LogMng.Instance.onWriteStatus(string.Format("The line is not existed {0} {1}", info.eventTitle, info.outcome));
                                    return PROCESS_RESULT.SUSPENDED;
                                }
                            }
                        }
                    }


                    int betLen2 = Utils.parseToInt(CDPController.Instance.ExecuteScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets.length", true));
                    if(betLen2 > 1)
                        return PROCESS_RESULT.ERROR;
                    
                    //Placebet
                    int retryCount = 3;
                    while (--retryCount > 0)
                    {
                        LogMng.Instance.onWriteStatus("Trying to place bet!");
                        double stake = info.stake;
                        string placebetRespBody = DoPlacebetUI(stake).Result;

                        dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(placebetRespBody);
                        string betRespCode = jsonContent.sr.ToString();
                        try
                        {
                            if (jsonContent["bt"][0]["rs"] != null)
                            {
                                string strLimitStake = jsonContent["bt"][0]["rs"].ToString();
                                double maxLimitStake = Utils.ParseToDouble(strLimitStake);
                                if (maxLimitStake > 0)
                                {
                                    stake = Math.Floor(maxLimitStake);
                                }
                            }

                            if (jsonContent["bt"][0]["ms"] != null)
                            {
                                string strLimitStake = jsonContent["bt"][0]["ms"].ToString();
                                double maxLimitStake = Utils.ParseToDouble(strLimitStake);
                                if (maxLimitStake > 0)
                                {
                                    stake = Math.Floor(maxLimitStake);
                                }
                            }
                        }
                        catch { }

                        double cur_balance = getBalance();
                        if (betRespCode == "0" || cur_balance < balance)
                        {
                            CDPController.Instance.ReloadBrowser();
                            return PROCESS_RESULT.PLACE_SUCCESS;
                        }
                        else if (betRespCode == "9")
                        {
                            retryCount--;
                            continue;
                        }
                        else if (betRespCode == "8")
                        {
                            Thread.Sleep(2500);
                            retryCount--;
                            continue;
                        }
                        else if (betRespCode == "10")
                            return PROCESS_RESULT.SMALL_BALANCE; 
                        else if (betRespCode == "11")
                        {
                            //Maxstake
                            string strBetMaxAmount = jsonContent["bt"][0]["rs"].ToString();
                            if (string.IsNullOrEmpty(strBetMaxAmount))
                                return PROCESS_RESULT.ZERO_MAX_STAKE;

                            double maxLimitStake = Utils.ParseToDouble(strBetMaxAmount);
                            if (maxLimitStake == 0)
                                return PROCESS_RESULT.ZERO_MAX_STAKE;

                            stake = maxLimitStake;
                            retryCount--;
                        }
                        else if (betRespCode == "14")
                        {
                            Thread.Sleep(1500);
                            retryCount--;
                            continue;
                        }
    
                        else
                            return PROCESS_RESULT.ERROR;
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
        private void Removebet()
        {
            LogMng.Instance.onWriteStatus("CloseBetSlip");
            //RunScript("BetSlipLocator.betSlipManager.deleteAllBets();");
            //Thread.Sleep(500);
            //return;
            //Removing all betslip stubs
            int nRetryCount = 0;
            while (nRetryCount++ < 3)
            {
                string betslipCount = CDPController.Instance.ExecuteScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets.length", true);
                int nBetsInSlip = 0;

                if (int.TryParse(betslipCount, out nBetsInSlip))
                {
                    if (nBetsInSlip > 0)
                    {


                        string removeButtonLocation = CDPController.Instance.ExecuteScript("JSON.stringify(BetSlipLocator.betSlipManager.betslip.activeModule.slip.bets[0].removeButton.getActiveElement().getBoundingClientRect())", true).ToLower();
                        Rectangle ResultRemoveBoxRect = Utils.ParseRectFromJson(removeButtonLocation);
                        if (ResultRemoveBoxRect.X > 0 && ResultRemoveBoxRect.Y > 0 && ResultRemoveBoxRect.Width > 0 && ResultRemoveBoxRect.Height > 0)
                        {
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"Stub Bets in Slip count: {ResultRemoveBoxRect}");
#endif

                            bool isClicked = CDPController.Instance.CLickOnPoint(ResultRemoveBoxRect.X , ResultRemoveBoxRect.Y).Result;
                            Thread.Sleep(500);
                        }
                        else
                        {
                            //betslip is minimized, we have to restore it first by clicking header 
                            string slipheaderLocation = CDPController.Instance.ExecuteScript("JSON.stringify(BetSlipLocator.betSlipManager.betslip.activeModule.slip.header.getActiveElement().getBoundingClientRect())", true).ToLower();
                            Rectangle slipheaderRect = Utils.ParseRectFromJson(slipheaderLocation);
                            if (slipheaderRect.X > 0 && slipheaderRect.Y > 0 && slipheaderRect.Width > 0 && slipheaderRect.Height > 0)
                            {
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus($"Stub Bets in Slip 1 count: {slipheaderRect}");
#endif

                                bool isClicked = CDPController.Instance.CLickOnPoint(slipheaderRect.X, slipheaderRect.Y).Result;
                                Thread.Sleep(500);
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    Thread.Sleep(300);
                }
            }

            string singlebetslipValue = CDPController.Instance.ExecuteScript("JSON.stringify(BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.indicationArea.getActiveElement().childNodes[1].getBoundingClientRect())", true).ToLower();
            Rectangle ResultSinglebetRemoveBoxRect = Utils.ParseRectFromJson(singlebetslipValue);
            if (ResultSinglebetRemoveBoxRect.X > 0 && ResultSinglebetRemoveBoxRect.Y > 0 && ResultSinglebetRemoveBoxRect.Width > 0 && ResultSinglebetRemoveBoxRect.Height > 0)
            {
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Stub Bets in Slip 2 count: {ResultSinglebetRemoveBoxRect}");
#endif
                bool isClicked = CDPController.Instance.CLickOnPoint(ResultSinglebetRemoveBoxRect.X, ResultSinglebetRemoveBoxRect.Y).Result;
                Thread.Sleep(500);
            }
            ///

            //closing multi betslips (it doesn't closed by function)  find RemoveAll button and click
            /*
            function getRemoveButtonPos()
            {
                var buttonClass = BetSlipLocator.betSlipManager.betslip.activeModule.slip.getActiveElement().children[1].children[0].className;
                if (buttonClass.includes('RemoveButton') || buttonClass.includes('ErrorMessage_Remove'))
                    return BetSlipLocator.betSlipManager.betslip.activeModule.slip.getActiveElement().children[1].children[0].getBoundingClientRect();
                return 'exception';
            }
            getRemoveButtonPos();
            */
            string removeAllButtonLocation = CDPController.Instance.ExecuteScript("function getRemoveButtonPos(){var buttonClass = BetSlipLocator.betSlipManager.betslip.activeModule.slip.getActiveElement().children[1].children[0].className;if (buttonClass.includes('RemoveButton') || buttonClass.includes('ErrorMessage_Remove'))return JSON.stringify(BetSlipLocator.betSlipManager.betslip.activeModule.slip.getActiveElement().children[1].children[0].getBoundingClientRect());return 'exception';}getRemoveButtonPos();", true).ToLower();
            Rectangle ResultBoxRect = Utils.ParseRectFromJson(removeAllButtonLocation);
            if (ResultBoxRect.X > 0 && ResultBoxRect.Y > 0 && ResultBoxRect.Width > 0 && ResultBoxRect.Height > 0)
            {
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Stub Bets in Slip 3 count: {ResultSinglebetRemoveBoxRect}");
#endif
                bool isClicked = CDPController.Instance.CLickOnPoint(ResultBoxRect.X, ResultBoxRect.Y).Result;
                Thread.Sleep(500);
            }

        }

        private bool IsBetSlipOpened()
        {
            string slipVisible = CDPController.Instance.ExecuteScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip._visible" , true);
            LogMng.Instance.onWriteStatus($"BetSlip Visible ： {slipVisible}");
            if (slipVisible.ToLower() == "true")
                return true;
            
            return false;
        }
        private double GetBetslipOdds()
        {
            double newOdds = 0;
            int kk = 0;
            while (kk < 4)
            {
                try
                {
                    string odds_str = CDPController.Instance.ExecuteScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.header.standardContent.oddsValue._text", true);
                    if (string.IsNullOrEmpty(odds_str))
                    {
                        kk++;
                        Thread.Sleep(500);
                        continue;
                    }
                    newOdds = Utils.ParseToDouble(odds_str);
                    break;
                }
                catch { }
            }
            return newOdds;
        }

        bool CheckOddDropCancelBet(double newOdd, BetburgerInfo info)
        {

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
            if (Setting.Instance.bAllowOddRise)
            {
                if (newOdd > info.odds)
                {
                    if (newOdd > info.odds + info.odds / 100 * Setting.Instance.dAllowOddRisePercent)
                    {
                        LogMng.Instance.onWriteStatus($"Ignore bet because of odd is rise up larger than {Setting.Instance.dAllowOddRisePercent}%: {info.odds} -> {newOdd}");
                        return true;
                    }
                }
            }
            return false;
        }

        public bool Pulse()
        {
            if (CDPController.Instance._browserObj != null)
            {
                CDPController.Instance.ExecuteScript("document.getElementsByClassName('sb-btn sb-btn--block sb-btn--large sb-btn--success')[0].click()");
                Thread.Sleep(2000);
                CDPController.Instance.NavigateInvoke("https://superbet.com/en-br/profile/");
            }
            return true;
        }
       
        public bool Removebet1()
        {
            string removeScript = Guard.Algorithms.DecryptData(Global.RemoveScript);
            CDPController.Instance.ExecuteScript(removeScript);
            return true;
        }
        public bool DoClickDlgBox()
        {
            string scriptResult = string.Empty;
            try
            {
                string jsCode = Guard.Algorithms.DecryptData(Global.popupScript);
                scriptResult = CDPController.Instance.ExecuteScript(jsCode, true);
                if (!string.IsNullOrEmpty(scriptResult) && !scriptResult.Contains("nodialog") && !scriptResult.Contains("exception"))
                {
                    //m_handlerWriteStatus("LastLoginModule_Button Clicked");
                    Thread.Sleep(900);
                }

            }
            catch { }
            return true;
        }

        public void Close()
        {

                        
        }
        public void Feature()
        {

        }
        public int GetPendingbets()
        {
            int nResult = 0;
            try
            {
                HttpClient m_client = new HttpClient();
                string betUrl = $"https://{domain}/en/api/bets/open?_={Utils.getTick()}";
                HttpResponseMessage openbetResponse = m_client.GetAsync(betUrl).Result;
                string content = openbetResponse.Content.ReadAsStringAsync().Result;
                dynamic addbet_res = JsonConvert.DeserializeObject<dynamic>(content);
                nResult = addbet_res.data.Count;
            }
            catch { }
            return nResult;
        }
        public string DoAddBetUI(string f , string fp , string o)
        {
            CDPController.Instance.PlaceBetRespBody = string.Empty;
            CDPController.Instance.WaitingForAPI = true;
            try
            {
                string function = "function doaddbet(pt, o, f, fp, so, c, mt) { for (let i = 0; i < Locator.user._eRegister.oddsChanged.length; i++) { try { let item = Locator.user._eRegister.oddsChanged[i]; if (item == null || item.scope == null || item.scope.twinEmphasizedHandlerType == null) continue; if (item.scope.twinEmphasizedHandlerType.includes(f) && item.scope.twinEmphasizedHandlerType.includes(fp)) { var t = item.scope.getBetItem(); t.updateItem(); BetSlipLocator.betSlipManager.addBet(t, item.scope); return true; } } catch (err) {} } var randomItem = null; for (let i = 0; i < Locator.user._eRegister.oddsChanged.length; i++) { let item = Locator.user._eRegister.oddsChanged[i]; if (item == null || item.scope == null || item.scope.twinEmphasizedHandlerType == null) continue; var targetPartDiv = item.scope._active_element; if (!targetPartDiv.className.includes('Suspended')) { var rect = targetPartDiv.getBoundingClientRect(); var isVisible = (rect.top >= 0) && (rect.bottom <= window.innerHeight - 50); if (document.elementFromPoint(rect.left + 1, rect.top + 1) != null && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.top + 1) != targetPartDiv) { isVisible = false; } if (document.elementFromPoint(rect.left + 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.left + 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.left + 1, rect.bottom - 1) != targetPartDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.top + 1) != null && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.top + 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.top + 1) != targetPartDiv) { isVisible = false; } if (document.elementFromPoint(rect.right - 1, rect.bottom - 1) != null && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('wcl-PageContainer ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ovm-OverviewScroller ') && !document.elementFromPoint(rect.right - 1, rect.bottom - 1).className.includes('ipe-EventViewDetailScroller ') && document.elementFromPoint(rect.right - 1, rect.bottom - 1) != targetPartDiv) { isVisible = false; } if (isVisible) { randomItem = item; break; } } } if (randomItem == null) { console.log('Not Found Element'); return false; } var t = randomItem.scope.getBetItem(); t.partType = pt; t.odds = o; t.fixtureID = f; t.participantID = fp; t.classificationID = c; t.betsource = mt; uid = f + '-' + fp; t.updateItem(); BetSlipLocator.betSlipManager.addBet(t, randomItem.scope); return true; } ";
                function += $"doaddbet('N','{o}','{f}','{fp}','','1','11')";
                
                //Thread.Sleep(1000);
                CDPController.Instance.PlaceBetRespBody = string.Empty;
                CDPController.Instance.ExecuteScript(function);
                Thread.Sleep(new Random().Next(1000, 1500));
                int retryCnt = 0;
                while (CDPController.Instance.WaitingForAPI)
                {
                    retryCnt++;
                    Thread.Sleep(100);
                    if (retryCnt >= 80)
                        break;
                }
                dynamic jsonSlipResponse = JsonConvert.DeserializeObject<dynamic>(CDPController.Instance.PlaceBetRespBody);
            }
            catch (Exception) { }

            return CDPController.Instance.PlaceBetRespBody;
        }
        private async Task<string> DoPlacebetUI(double stake)
        {
            CDPController.Instance.PlaceBetRespBody = string.Empty;
            CDPController.Instance.WaitingForAPI = true;
            JObject betPosition = null;
            int retryCnt = 0;
            stake_pos = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(stake_pos))
                {
                    int k = 3;
                    while (k-- > 0)
                    {
                        stake_pos = CDPController.Instance.ExecuteScript("JSON.stringify(BetSlipLocator.betSlipManager.betslip.activeModule.slip.footer.stakeBox._active_element.getBoundingClientRect())", true);
                        if (!string.IsNullOrEmpty(stake_pos))
                            break;

                        Thread.Sleep(1000);
                    }
                }

                string result = string.Empty;
                if (!string.IsNullOrEmpty(stake_pos))
                    betPosition = JsonConvert.DeserializeObject<JObject>(stake_pos);

                decimal x = 0, y = 0;
                if (!string.IsNullOrEmpty(stake_pos))
                {
                    betPosition = JsonConvert.DeserializeObject<JObject>(stake_pos);
                    x = decimal.Parse(betPosition.SelectToken("x").ToString());
                    y = decimal.Parse(betPosition.SelectToken("y").ToString());
                    decimal width = decimal.Parse(betPosition.SelectToken("width").ToString());
                    decimal height = decimal.Parse(betPosition.SelectToken("height").ToString());

                    _X_stake = x + width / 4 + Utils.GetRandValue(0, 5, true);
                    _Y_stake = y + height / 4 + Utils.GetRandValue(0, 5, true);
                    LogMng.Instance.onWriteStatus(string.Format("Stake pos: ({0}, {1})", _X_stake, _Y_stake));
                }

                if (!IsBetSlipOpened())
                    return string.Empty;

                await CDPController.Instance.ClickOnPoint(stake_pos, ClickType.TripleClick, 1);
                Thread.Sleep(800);
                // Send Stake
                LogMng.Instance.onWriteStatus("Stake :" + stake.ToString("N2"));
                CDPController.Instance.ExecuteScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.footer.stakeBox.inputBlur();", false);
                CDPController.Instance.ExecuteScript($"BetSlipLocator.betSlipManager.betslip.activeModule.slip.footer.stakeBox.setStake(\"{stake.ToString("N2")}\");", false);

                string placebetScript = Guard.Algorithms.DecryptData(Global.PlacebetScript);
                while (retryCnt < 8)
                {
                    decimal placebet_x = 0, placebet_y = 0;
                    CDPController.Instance.WaitingForAPI = true;
                    int kk = 3;
                    if (string.IsNullOrEmpty(placebetBtn_pos))
                    {
                        while (kk-- > 0)
                        {
                            placebetBtn_pos = CDPController.Instance.ExecuteScript(placebetScript, true);
                            if (!string.IsNullOrEmpty(placebetBtn_pos))
                                break;
                            else
                                Thread.Sleep(700);
                        }
                    }

                    LogMng.Instance.onWriteStatus("Placebet Button pos => " + placebetBtn_pos);
                    if (!string.IsNullOrEmpty(placebetBtn_pos) && !placebetBtn_pos.Contains("Placebet btn is disable now"))
                    {
                        if (!IsBetSlipOpened())
                            return string.Empty;

                        placebet_x = x + 250;
                        placebet_y = y;
                        CDPController.Instance.CLickOnPoint((int)placebet_x, (int)placebet_y, ClickType.click).Wait();
                        Thread.Sleep(2000);
                    
                    }

                    int rCnt = 0;
                    while (CDPController.Instance.WaitingForAPI)
                    {
                        rCnt++;
                        Thread.Sleep(100);
                        if (rCnt >= 150)
                            break;
                    }
                    // If stake is above max stake, odds or line has been changed!>
                    //m_handlerWriteStatus(RespBody);
                    dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(CDPController.Instance.PlaceBetRespBody);
                    string betRespCode = jsonContent.sr.ToString();
                    if (betRespCode == "11")
                    {
                        //max stake
                        retryCnt++;
                        if (!IsBetSlipOpened())
                            return string.Empty;

                        CDPController.Instance.CLickOnPoint((int)placebet_x, (int)placebet_y, ClickType.click).Wait();
                        Thread.Sleep(300);
                    }
                    else if (betRespCode == "0")
                    {
                        Thread.Sleep(2000);
                        // Remove placed bets
                        CDPController.Instance.ExecuteScript("BetSlipLocator.betSlipManager.betslip.activeModule.slip.receiptContent.receiptContentDoneButtonClicked();", false);
                        break;
                    }
                    else if (betRespCode == "14")
                    {
                        Thread.Sleep(3000);
                        retryCnt++;
                    }
                    else if (betRespCode == "24")
                    {
                        retryCnt++;
                        Thread.Sleep(1000);
                    }
                    else if (betRespCode == "86")
                    {
                        retryCnt++;
                    }
                    else
                    {
                        break;
                    }
                }


            }
            catch (Exception) { }
            return CDPController.Instance.PlaceBetRespBody;
        }


    }
#endif
}
