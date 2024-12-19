namespace Project.Bookie
{
#if (BETFAIR_FETCH)

    enum PlaceBetStatus
    {
        canBet, // = 'CAN_BET'
        oddsDropped, // = 'NO_BET_ODDS_DROPPED'
        oddsChangedWhilePacing, // = 'ODDS_CHANGED_WHILE_PLACING'
        placementFailure, // = 'BET_PLACEMENT_FAILURE'
        maxStakeReached, // = 'MAX_STAKE_REACHED'
        minStakeNeeded, // = 'MIN_STAKE_NEEDED'
        marketNotAvailable, // = 'MARKET_NOT_AVAILABLE'
        marketSuspended, // = 'MARKET_SUSPENDED'
        unexpectedError, // = 'UNEXPECTED_ERROR'
        betPlaced, // = 'BET_PLACED'
    }

    class PlaceBetData
    {
        public PlaceBetStatus status;

        public double minStake;
        public double maxStake;
        public double currentOdds;

        public Dictionary<string, KeyValuePair<string, string>> formData = new Dictionary<string, KeyValuePair<string, string>>();
    };
    


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
        Dictionary<string, string> configUrls = new Dictionary<string, string>()
        {
            {"baseURL", "https://www.{0}" },
            {"homePageURL", "https://www.{0}/sport" },
            {"loginURL", "https://identitysso.{0}/view/login" },
            {"balanceURL", "https://was.{0}/wallet-service/v3.0/wallets" },
            {"myActivityURL", "https://myactivity.{0}/" },
            {"betHistoryURL", "https://myactivity.{0}/activity/sportsbook" },
            {"accountSummaryURL", "https://myaccount.{0}/summary/accountsummary" }
        };
    

        Dictionary<string, string> dictEventUrl = new Dictionary<string, string>();
        Object lockerObj = new object();

        public HttpClient m_client = null;
        public string sessionToken = null;

        public string appkey = "";
        public BetfairCtrl()
        {
            m_client = initHttpClient();
        }

        public void Close()
        {
            
        }


        public bool logout()
        {
            Global.RemoveCookies();

            string functionString = "window.localStorage.clear();window.sessionStorage.clear();";
            Global.RunScriptCode(functionString);
            return true;
        }

        public int GetPendingbets()
        {
            return 0;
        }
        private bool islogin()
        {
            if (getBalance() < 0)
                return false;
            return true;
        }
        public void Feature()
        {

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

        private string getSecurityToken()
        {
            try
            {
                foreach (Cookie cookieData in Global.cookieContainer.GetCookies(new Uri($"https://www.{Setting.Instance.domain}")))
                {
                    if (cookieData.Name == "xsrftoken")
                    {
                        return cookieData.Value;
                    }
                }
                LogMng.Instance.onWriteStatus("[getSecurityToken] Security token not found");
                return null;
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus("getSecurityToken exception");
            }
            return null;
        }
        public bool InnerLogin(bool bRemovebet = true)
        {            
            Global.SetMonitorVisible(true);
            bool bLogin = false;
            try
            {
                try
                {
                    Monitor.Enter(lockerObj);
           

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

                    int nRetryCheck = 0;
                    while (nRetryCheck++ < 3)
                    {
                        Thread.Sleep(4000);

                        if (getBalance() < 0)
                        {
                            bLogin = false;
                        }
                        else
                        {
                            Task.Run(async () => await Global.GetCookie($"https://www.{Setting.Instance.domain}")).Wait();
                            if (bRemovebet)
                            {
                                removeBet();

                            }
                            bLogin = true;
                            break;
                        }
                    }
                }
                catch { }
                finally
                {
                    Monitor.Exit(lockerObj);
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
            logout();
            return InnerLogin();
        }


        public void PlaceBet(List<CapsuledBetburgerInfo> info)
        {
            if (info.Count < 1)
                return;

            //info[0].result = PlaceBet(info[0].betburgerInfo);
        }

        private PlaceBetData getPlaceBetData(dynamic addbetResponse, OpenBet_Betfair openbet, BetburgerInfo info)
        {
            PlaceBetData placeBetData = new PlaceBetData();
            
            string betRawForm = "";
            dynamic instructions = addbetResponse.page.config.instructions;
            try
            {
                foreach (var instruction in instructions)
                {
                    if (instruction.action != "add-bet")
                    {
                        continue;
                    }
                    betRawForm = instruction.arguments.bet;
                }

                if (string.IsNullOrEmpty(betRawForm))
                {
                    placeBetData.status = PlaceBetStatus.marketNotAvailable;
                    return placeBetData;
                }

                placeBetData.status = PlaceBetStatus.unexpectedError;

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(betRawForm);


                bool isSuspended = true;
                Dictionary<string, KeyValuePair<string, string>> formInputs = new Dictionary<string, KeyValuePair<string, string>>();
                IEnumerable<HtmlNode> inputTypes = doc.DocumentNode.Descendants("input");
                if (inputTypes.LongCount() > 0)
                {
                    foreach (HtmlNode input in inputTypes)
                    {
                        string inputName = "";
                        if (input.Attributes["name"] != null)
                            inputName = input.Attributes["name"].Value;                        
                        
                        string inputValue = "";
                        if (input.Attributes["value"] != null)
                            inputValue = input.Attributes["value"].Value;
                        
                        if (string.IsNullOrWhiteSpace(inputName))
                        {
                            continue;
                        }
                        string inputParsedName = inputName.Split('-').Reverse().ElementAt(0);
                        if (inputParsedName == "minStake")
                        {
                            placeBetData.minStake = Utils.ParseToDouble(inputValue);
                        }
                        else if (inputParsedName == "maxStake")
                        {
                            placeBetData.maxStake = Utils.ParseToDouble(inputValue);
                        }
                        else if (inputParsedName == "odd")
                        {
                            placeBetData.currentOdds = Utils.ParseToDouble(inputValue);
                        }
                        else if (inputParsedName == "suspended")
                        {
                            isSuspended = Utils.parseToBool(inputValue);
                        }
                        else if (inputParsedName == "stake")
                        {
                            inputValue = info.stake.ToString("0.00").Replace(",", ".");
                        }
                        formInputs[inputParsedName] = new KeyValuePair<string, string>(inputName, inputValue);
                    }
                }
                if (formInputs.Count > 0)
                    placeBetData.formData = formInputs;

                if (isSuspended)
                {
                    placeBetData.status = PlaceBetStatus.marketSuspended;
                    return placeBetData;
                }

                if (CheckOddDropCancelBet(placeBetData.currentOdds, info))
                {
                    placeBetData.status = PlaceBetStatus.oddsDropped;
                    return placeBetData;
                }

                if (info.stake > placeBetData.maxStake)
                {
                    placeBetData.status = PlaceBetStatus.maxStakeReached;
                    return placeBetData;
                }
                if (placeBetData.minStake > info.stake)
                {
                    placeBetData.status = PlaceBetStatus.minStakeNeeded;
                    return placeBetData;
                }
            }
            catch (Exception ex)
            {
                placeBetData.status = PlaceBetStatus.unexpectedError;
                return placeBetData;
            }

            placeBetData.status = PlaceBetStatus.canBet;
            return placeBetData;
        }
        private dynamic addBet(OpenBet_Betfair openbet)
        {
            string homePageURL = string.Format(configUrls["homePageURL"], Setting.Instance.domain);

            string bsUUID = Utils.RandomHexString(40).ToLower();


            Dictionary<string, string> queryString = new Dictionary<string, string>(){
                {"gaMod", "minimarketview" },
                {"eventId", openbet.eventId },
                {"gaTab", "UG9wdWxhcg==" },
                {"gaZone", "Main" },
                {"bseId", openbet.bseId },
                {"isSP", "false" },
                {"bsContext", "REAL" },
                {"bssId", openbet.bssId },
                {"bsmSt", Utils.getTick().ToString() },
                {"gaPageView", "event_inplay" },
                {"action", "addSelection" },
                {"bsUUID", bsUUID }, 
                {"bsmId", openbet.bsmId },
                {"modules", "betslip" },
                {"xsrftoken", getSecurityToken()},
                {"bsGroup", openbet.bseId },
                {"isAjax", "true" },
                {"lastId", "1081" },
                {"ts", Utils.getTick().ToString() },
                {"alt", "json" }
            };


            string addSelectionURL = $"{homePageURL}/football/german-bundesliga-2/hamburger-sv-v-schalke-04/{openbet.eventId}?";
            
            foreach (KeyValuePair<string, string> pair in queryString)
            {
                addSelectionURL += $"{pair.Key}={pair.Value}&";
            }
            addSelectionURL = addSelectionURL.Substring(0, addSelectionURL.Length - 1);
            
            string functionString = $"window.fetch('{addSelectionURL}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', method: 'GET' }}).then(response => response.json());";

            Global.strPlaceBetResult = "";
            Global.waitResponseEvent.Reset();

            Global.RunScriptCode(functionString);
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"Addbet Request: {functionString}");
#endif
            if (Global.waitResponseEvent.Wait(10000))
            {
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Addbet Result: {Global.strPlaceBetResult.Trim()}");
#endif
                dynamic addbetResponse = JsonConvert.DeserializeObject<dynamic>(Global.strPlaceBetResult.Trim());
                return addbetResponse;                
            }
            return null;
        }

        private dynamic getPlaceBetResult(dynamic placebetResponse)
        {
            dynamic instructions = placebetResponse.page.config.instructions;
            dynamic placedBetData = null;

            foreach (dynamic instruction in instructions)
            {
                if (!(instruction?.action == "call-subscribeBets" && instruction?.type == "betslip"))
                {
                    continue;
                }
                try
                {
                    string json = instruction.arguments?.funcArgs;
                    string temp = json.Replace("\\\"", "\"");
                    placedBetData = JsonConvert.DeserializeObject<dynamic>(temp);
                    break;
                }
                catch (Exception ex)
                {
                    LogMng.Instance.onWriteStatus($"[placeBet] {ex} ${instruction}");
                    return null;
                }
            }
            if (placedBetData == null)
            {
                LogMng.Instance.onWriteStatus($"Placebet failed with null");
                return null;
            }
            dynamic singleBetResult = placedBetData.impliedSingleBets[0];
            if (singleBetResult == null)
            {
                return null;
            }
            if (!(singleBetResult.receiptId != null && singleBetResult.resultCode == "SUCCESS"))
            {
                LogMng.Instance.onWriteStatus($"[placeBet] Could not place bet unexpected status, {singleBetResult}");
                return null;
            }
            return singleBetResult;
        }
        private PROCESS_RESULT apiPlaceBet(OpenBet_Betfair openbet, Dictionary<string, KeyValuePair<string, string>> formData)
        {
            string homePageURL = string.Format(configUrls["homePageURL"], Setting.Instance.domain);

            string xsrftoken = getSecurityToken();
            Dictionary<string, string> queryString = new Dictionary<string, string>(){
                    {"action", "confirm" },
                    {"modules", "betslip" },
                    {"bsContext", "REAL" },
                    {"xsrftoken", getSecurityToken()},
                    {"lastId", "1065" },
                    {"isAjax", "true" }
                };

            List<string> qs = new List<string>();
            foreach (KeyValuePair<string, string> pair in queryString)
            {
                qs.Add($"{pair.Key}={pair.Value}");
            }
            string betURL = $"{homePageURL}/football/french-cup/rennes-v-lorient/{openbet.eventId}?{string.Join("&", qs)}";

            List<string> formBody = new List<string>();
            foreach (var data in formData) 
            {
                formBody.Add($"{WebUtility.UrlEncode(data.Value.Key)}={WebUtility.UrlEncode(data.Value.Value + "")}");
            }
            string formDataString = string.Join("&", formBody);
            string functionString = $"window.fetch('{betURL}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/x-www-form-urlencoded' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

            Global.strPlaceBetResult = "";
            Global.waitResponseEvent.Reset();
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"Placebet Request: {functionString}");
#endif
            Global.RunScriptCode(functionString);



            if (Global.waitResponseEvent.Wait(20000))
            {
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Placebet Result: {Global.strPlaceBetResult}");
#endif

                if (string.IsNullOrEmpty(Global.strPlaceBetResult))
                {
                    LogMng.Instance.onWriteStatus("[placeBet] Could not place bet , no response");
                    return PROCESS_RESULT.ERROR;
                }

                try
                {
                    string temp = Global.strPlaceBetResult.Trim();
                    dynamic placebetResponse = JsonConvert.DeserializeObject<dynamic>(Global.strPlaceBetResult.Trim());
                    dynamic placeBetResult = getPlaceBetResult(placebetResponse);

                    if (placeBetResult != null)
                    {
                        LogMng.Instance.onWriteStatus($"[placeBet] Successfully placed bet { placeBetResult.receiptId}");
                        return PROCESS_RESULT.PLACE_SUCCESS;
                    }
                    dynamic instructions = placebetResponse.page.config.instructions;
                    if (instructions == null)
                    {
                        LogMng.Instance.onWriteStatus($"[placeBet] Instructions not found, could not bet, response {placebetResponse}");
                        return PROCESS_RESULT.ERROR;
                    }

                    // eslint-disable-next-line
                    dynamic analyticsEvents = null;
                    string domContent = null;
                    foreach (dynamic instruction in instructions) 
                    {
                        if (instruction?.action == "send-event" && instruction?.type == "analytics")
                        {
                            analyticsEvents = instruction.arguments;
                        }
                        if (instruction?.action == "set-content" && instruction?.type == "dom")
                        {
                            domContent = instruction?.arguments?.html;
                        }
                    }
                    if (analyticsEvents == null)
                    {
                        LogMng.Instance.onWriteStatus($"[handlePlaceBetResponse] analytics data not found dom error");
                        return PROCESS_RESULT.ERROR;
                    }
                    dynamic errorLabel = analyticsEvents.eventData.label;
                    if (errorLabel == null)
                    {
                        LogMng.Instance.onWriteStatus($"[handlePlaceBetResponse] Could not get analytics label {analyticsEvents}");
                    }

                    if (errorLabel == "REQUESTED_PRICE_NOT_AVAILABLE")
                    {
                        LogMng.Instance.onWriteStatus("Odd is changed while placing bet");                        
                    }
                    else if (errorLabel == "BET_PLACEMENT_FAILURE")
                    {
                        LogMng.Instance.onWriteStatus("Placement failure");                        
                    }
                    else
                    {
                        LogMng.Instance.onWriteStatus($"[handlePlaceBetResponse] Unexpected label {errorLabel}, {analyticsEvents}");
                    }                    
                }
                catch (Exception ex)
                {
                    LogMng.Instance.onWriteStatus($"apiPlaceBet exception {ex}");
                }                
            }

            return PROCESS_RESULT.ERROR;
        }
        public PROCESS_RESULT PlaceBet(ref BetburgerInfo info)
        {
            removeBet();
            LogMng.Instance.onWriteStatus(string.Format("Place bet start isLive: {0}", info.isLive));
            BetResult br = new BetResult();

            try
            {
                OpenBet_Betfair openbet = Utils.ConvertBetburgerPick2OpenBet_Betfair(info);
                LogMng.Instance.onWriteStatus(string.Format("Place bet converting openbet success"));
                if (openbet == null)
                {
                    LogMng.Instance.onWriteStatus("Converting OpenBet Error");
                    return PROCESS_RESULT.ERROR;
                }

                dynamic addBetResponse = addBet(openbet);
                PlaceBetData betFormData = getPlaceBetData(addBetResponse, openbet, info);
                if (betFormData.status == PlaceBetStatus.unexpectedError ||
                    betFormData.status == PlaceBetStatus.marketSuspended ||
                    betFormData.status == PlaceBetStatus.minStakeNeeded ||
                    betFormData.status == PlaceBetStatus.marketNotAvailable ||
                    betFormData.status == PlaceBetStatus.oddsDropped)
                {
                    LogMng.Instance.onWriteStatus($"[placeBet] Could not get data for bet {betFormData.status}:");
                    return PROCESS_RESULT.ERROR;
                }

                if (betFormData.status == PlaceBetStatus.maxStakeReached)
                {
                    LogMng.Instance.onWriteStatus($"[placeBet] Max stake reached, old stake:{info.stake} new: { betFormData.maxStake}");
                    
                    betFormData.formData["stake"] = new KeyValuePair<string, string>(betFormData.formData["stake"].Key, betFormData.maxStake.ToString("0.00").Replace(",", "."));

                    if (Setting.Instance.domain.ToLower().Contains(".it"))
                    {
                        if (betFormData.maxStake < 2)
                        {
                            LogMng.Instance.onWriteStatus("Italian account can't place bet lower than 2 euro");
                            return PROCESS_RESULT.ERROR;
                        }
                        LogMng.Instance.onWriteStatus($"[placeBet] Max stake reached, old stake:{info.stake} new: { betFormData.maxStake} italian stake: {((int)Math.Floor(betFormData.maxStake)).ToString("0.00").Replace(",", ".")}");
                        betFormData.formData["stake"] = new KeyValuePair<string, string>(betFormData.formData["stake"].Key, ((int)Math.Floor(betFormData.maxStake)).ToString("0.00").Replace(",", "."));
                        
                    }
                    info.stake = betFormData.maxStake;
                }
                betFormData.formData["timestamp"] = new KeyValuePair<string, string>("timestamp", Utils.getTick().ToString());

                return apiPlaceBet(openbet, betFormData.formData);                
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus($"Place bet exception {e}");
                
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
                    
            string homePageURL = string.Format(configUrls["homePageURL"], Setting.Instance.domain);
            Dictionary<string, string> queryString = new Dictionary<string, string>(){
                {"action", "removeAll" },
                {"modules", "betslip" },
                {"bsContext", "REAL" },
                {"xsrftoken", getSecurityToken()},
                {"isAjax", "true" },
                {"alt", "json" },
                {"ts", Utils.getTick().ToString() },
            };


            List<string> qs = new List<string>();
            foreach (KeyValuePair<string, string> pair in queryString)
            {
                qs.Add($"{pair.Key}={pair.Value}");
            }
            string removeEndpointURL = $"{homePageURL}/?{string.Join("&", qs)}";

            string functionString = $"window.fetch('{removeEndpointURL}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', method: 'GET' }}).then(response => response.json());";

            //int nRetryButtonCount = 0;
            //nRetryButtonCount = Utils.parseToInt(Global.GetStatusValue("return document.querySelectorAll('a[data-betslip-action=\"remove-all\"][class=\"remove-all-bets ui-betslip-action\"]').length;"));
            //if (nRetryButtonCount <= 0)
            //    break;
            Global.RunScriptCode(functionString);

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"RemoveBet Request: {functionString}");
#endif
                        
        }

       
        public double getBalance()
        {
            double balance = -1;
            try
            {
                Dictionary<string, string> queryString = new Dictionary<string, string>(){
                    {"walletNames", "[MAIN,SPORTSBOOK_BONUS,BOOST_TOKENS]" },                    
                    {"alt", "json" }
                };
                if (Setting.Instance.domain.Contains(".it"))
                {
                    queryString = new Dictionary<string, string>(){
                    {"walletNames", "[ITA,SPORTSBOOK_BONUS,BOOST_TOKENS]" },
                    {"alt", "json" }
                };
                }
                List<string> qs = new List<string>();
                foreach (KeyValuePair<string, string> pair in queryString)
                {
                    qs.Add($"{pair.Key}={pair.Value}");
                }

                string getBalanceURL = $"{string.Format(configUrls["balanceURL"], Setting.Instance.domain)}?{string.Join("&", qs)}";
                string baseURL = $"{string.Format(configUrls["baseURL"], Setting.Instance.domain)}";
                string functionString = $"window.fetch('{getBalanceURL}', {{ method: 'GET', headers: {{ 'accept': '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json', }}, credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', referrer: '{baseURL}' }}).then(response => response.json());";

                Global.strAddBetResult = "";
                Global.waitResponseEvent.Reset();

                Global.RunScriptCode(functionString);

                if (Global.waitResponseEvent.Wait(3000))
                {
                    List<BalanceJson> details = new List<BalanceJson>();
                    details = JsonConvert.DeserializeObject<List<BalanceJson>>(Global.strAddBetResult);

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
            }
            catch (Exception e)
            {

            }
            Global.balance = balance;
            return balance;
        }

        public bool Pulse()
        {
            Global.RefreshPage();

            if (getBalance() < 1)                
                return false;
            return true;
        }

    }
#endif
}
