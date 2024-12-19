namespace Project.Bookie
{
#if (DOMUSBET || BETALAND)
    public class DomusbetCtrl : IBookieController
    {
        public HttpClient m_client = null;
        private string SYSTEMCODE = "";

        private string domuseMarket = "";
        public DomusbetCtrl()
        {
            //parseLog();
            m_client = initHttpClient();

            //Assembly assem = GetType().Assembly;

            //string[] names = assem.GetManifestResourceNames();

            //using (Stream stream = assem.GetManifestResourceStream("Project.Json.DomusMarket.txt"))
            //{
            //    try
            //    {
            //        using (StreamReader reader = new StreamReader(stream))
            //        {
            //            domuseMarket = reader.ReadToEnd();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //    }
            //}

        }

        private void parseLog()
        {
            try
            {

                DirectoryInfo info = new DirectoryInfo("domuslogs\\logs");
                FileInfo[] files = info.GetFiles("*.log").OrderBy(p => p.CreationTime).ToArray();


                List<string> usernames = new List<string>();

                string picktoplacebet = "";
                string foundMatchName = "";
                string jsonData = "";
                string ParseOutcome = "";
                foreach (FileInfo file in files)
                {
                    string[] lines = File.ReadAllLines(file.FullName);
                    foreach (string line in lines)
                    {
                        if (line.Contains("Pick to place to bet"))
                        {
                            picktoplacebet = line;
                            jsonData = "";
                            ParseOutcome = "";
                            foundMatchName = "";
                        }
                        else if (line.Contains("Found Match name"))
                        {
                            foundMatchName = line;
                        }
                        else if (line.Contains("\"ags\""))
                        {
                            jsonData = line;
                        }
                        else if (line.Contains("ParseOutcome: "))
                        {
                            ParseOutcome = line;
                        }
                        else if (line.Contains("Success to place"))
                        {
                            if (picktoplacebet != "" && jsonData != "" && ParseOutcome != "" && foundMatchName != "")
                                parselogParam(file.Name, picktoplacebet.Substring(20), jsonData.Substring(20), ParseOutcome.Substring(20), foundMatchName.Substring(20), "Success ");

                            picktoplacebet = "";
                            jsonData = "";
                            ParseOutcome = "";
                            foundMatchName = "";
                        }
                        else if (line.Contains("Marked as Fail "))
                        {
                            if (picktoplacebet != "" && jsonData != "" && ParseOutcome != "" && foundMatchName != "")
                                parselogParam(file.Name, picktoplacebet.Substring(20), jsonData.Substring(20), ParseOutcome.Substring(20), foundMatchName.Substring(20), "Marked as Fail");
                            picktoplacebet = "";
                            jsonData = "";
                            ParseOutcome = "";
                            foundMatchName = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            int i = 0;
            i++;
        }

        private void parselogParam(string filename, string pickline, string jsondata, string parseoutcome, string foundmatchname, string result)
        {
            string sport = "", outcome = "";
            sport = Utils.Between(pickline, "sport: ", "league: ").Trim();
            outcome = Utils.Between(pickline, "outcome: ", "percent: ").Trim();
            double odd = Utils.ParseToDouble(Utils.Between(pickline, "odds: ", "siteur: ").Trim());
            if (sport == "soccer" && outcome.Contains("EH"))
            {
                File.AppendAllText(@"pickRefine.txt", filename + "  " + pickline + Environment.NewLine);
                File.AppendAllText(@"pickRefine.txt", foundmatchname + Environment.NewLine);
                File.AppendAllText(@"pickRefine.txt", parseoutcome + Environment.NewLine);
                File.AppendAllText(@"pickRefine.txt", result + "   " + odd + Environment.NewLine);
                //File.AppendAllText(@"pickRefine.txt", jsondata + Environment.NewLine);             

                dynamic MarketInfoObj = JsonConvert.DeserializeObject<dynamic>(jsondata);


                string market_cs = "", market_ce = "", market_handicap = "", descBet = "", descDraw = "";
             
                try
                {
                    ParseOutcome(sport, outcome, out market_cs, out market_ce, out market_handicap, out descBet, out descDraw);
                }
                catch
                {
                    
                }
              
                foreach (dynamic marketObj in MarketInfoObj.scs)
                {
                    if (marketObj.cs.ToString() == market_cs)
                    {
                        string line = $"{marketObj.h}   ";                      

                        foreach (dynamic participantObj in marketObj.eqs)
                        {
                            line += $"{participantObj.ce} : {participantObj.q}  ";
                        }
                        File.AppendAllText(@"pickRefine.txt", line + Environment.NewLine);
                    }
                }
            }
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
            if (getBalance() < 0)
                return false;
            return true;
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
            //httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("context", "2:0:en_US");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"Chromium\";v=\"116\", \"Not)A;Brand\";v=\"24\", \"Google Chrome\";v=\"116\"");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");

            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Referer", $"https://www.{Setting.Instance.domain}/");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Origin", $"https://www.{Setting.Instance.domain}");

            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;

            return httpClientEx;
        }

        public bool logout()
        {
            return true;
        }
        public bool login()
        {
            if (Setting.Instance.domain.ToLower().Contains("domus"))
                SYSTEMCODE = "DOMUSBET";
            else if (Setting.Instance.domain.ToLower().Contains("betway"))
                SYSTEMCODE = "BETWAY";
            else if (Setting.Instance.domain.ToLower().Contains("betaland"))
                SYSTEMCODE = "BETALAND";

            bool bLogin = false;
            try
            {
                Global.DomusbetToken = "";
                Global.SetMonitorVisible(true);
                Global.OpenUrl($"https://www.{Setting.Instance.domain}/scommesse-sportive");
                int nRetry1 = 0;
                while (nRetry1 < 20)
                {
                    Thread.Sleep(500);

                    string result = Global.GetStatusValue("return document.getElementById('cg-username').outerHTML;");

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Login button status : {result}");
#endif

                    if (result.Contains("class"))
                    {
                        break;
                    }
                    nRetry1++;
                }
                if (nRetry1 >= 20)       //Page is loading gray page. let's retry
                {
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"Can't open login page");
#endif
                    Task.Run(async () => await Global.GetCookie($"https://www.{Setting.Instance.domain}")).Wait();

                    if (getBalance() >= 0)
                    {                        
                        return true;
                    }
                    return false;
                }


                Global.RunScriptCode($"document.getElementById('cg-username').value='{Setting.Instance.username}';");

                Global.RunScriptCode($"document.getElementById('cg-password').value='{Setting.Instance.password}';");

                Thread.Sleep(500);
                Global.waitResponseEvent.Reset();

                Global.RunScriptCode("document.getElementsByClassName('bottone-login')[0].click();");

                


                if (!Global.waitResponseEvent.Wait(30000))
                {
                    LogMng.Instance.onWriteStatus("no response of initSession");
                    throw new Exception();
                }

                                                
                LogMng.Instance.onWriteStatus($"Token: {Global.DomusbetToken}");
                if (string.IsNullOrEmpty(Global.DomusbetToken))
                {
                    LogMng.Instance.onWriteStatus($"Token Invalid ");
                    throw new Exception();
                }


                //Task.Run(async () => await Global.GetCookie($"https://www.{Setting.Instance.domain}")).Wait();
                Thread.Sleep(2000);
                if (getBalance() < 0)
                {
                    LogMng.Instance.onWriteStatus("getting balance is failed...");
                }
                else
                {
                    Global.OpenUrl($"https://www.{Setting.Instance.domain}/scommesse-sportive");
                    Thread.Sleep(5000);

                    bLogin = true;
                }
            }
            catch (Exception e)
            {

            }

            LogMng.Instance.onWriteStatus($"Login Result: {bLogin}");
            
            return bLogin;
        }

        public double getBalance()
        {
            LogMng.Instance.onWriteStatus(string.Format("GetBalance start"));
            int nRetry = 2;
            double balance = -1;
            while (nRetry >= 0)
            {
                nRetry--;
                try
                {
                    string balanceUrl = $"https://www.{Setting.Instance.domain}/updateBalance";
                    string formDataString = $"systemCode={SYSTEMCODE}&hash=&setSession=false";
                    string functionString = $"window.fetch('{balanceUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/x-www-form-urlencoded' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', body: '{formDataString}', method: 'POST' }}).then(response => response.json());";

                    Global.strAddBetResult = "";
                    Global.waitResponseEvent.Reset();
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"GetBalance Request: {functionString}");
#endif
                    Global.RunScriptCode(functionString);



                    if (!Global.waitResponseEvent.Wait(5000))
                    {
                        LogMng.Instance.onWriteStatus($"GetBalance No Response");
                        return -1;
                    }
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"GetBalance Result: {Global.strAddBetResult}");
#endif

                    string balanceContent = Global.strAddBetResult;
                                        
                    if (string.IsNullOrEmpty(balanceContent))
                    {
                        LogMng.Instance.onWriteStatus("GetBalance Response null");
                    }
                    dynamic balanceObj = JsonConvert.DeserializeObject<dynamic>(balanceContent);
                    if (balanceObj.codiceEsito.ToString() != "0")
                    {
                        LogMng.Instance.onWriteStatus("login failed reason:" + balanceObj.descrizione.ToString());
                        return -1;
                    }
                    balance = Utils.ParseToDouble(balanceObj.data.saldo.ToString()) / 100;
                    break;
                }
                catch (Exception e)
                {
                    LogMng.Instance.onWriteStatus(string.Format("Balance exception: {0} {1}", e.Message, e.StackTrace));
                }
            }

            LogMng.Instance.onWriteStatus(string.Format("Balance: {0}", balance));
            return balance;
        }
        Random rand = new Random();
        public int generateSelectionId()
        {
            return rand.Next(100001, 999999);
        }

        public void PlaceBet(List<CapsuledBetburgerInfo> info)
        {
            if (info.Count < 1)
                return;

            info[0].result = PlaceBet(info[0].betburgerInfo);
        }

        //EH handicap sign should be inverted
        public static void ParseOutcome(string sport, string outcome, out string cs, out string ce, out string handicap, out string descBet, out string descDraw)
        {
            cs = "";
            ce = "";
            handicap = "";
            descBet = "";
            descDraw = "";

            MatchCollection mc = Regex.Matches(outcome, "^(?<side>(1|X|2))[ \\t]?(?<period>((\\[1st half\\])|(\\[regular time\\]))?)");
            if (mc.Count == 1)
            {
                Match m = mc[0];
                if (sport == "soccer")
                {
                    if (string.IsNullOrEmpty(m.Groups["period"].Value))
                    {
                        if (m.Groups["side"].Value == "1")
                        {
                            cs = "3";
                            ce = "1";
                            descBet = "1X2";
                            descDraw = "1";
                        }
                        else if (m.Groups["side"].Value == "X")
                        {
                            cs = "3";
                            ce = "2";
                            descBet = "1X2";
                            descDraw = "X";
                        }
                        else if (m.Groups["side"].Value == "2")
                        {
                            cs = "3";
                            ce = "3";
                            descBet = "1X2";
                            descDraw = "2";
                        }
                    }
                    else if (m.Groups["period"].Value == "[1st half]")
                    {
                        if (m.Groups["side"].Value == "1")
                        {
                            cs = "15529";
                            ce = "1";
                            descBet = "1x2 1°TEMPO";
                            descDraw = "1";
                        }
                        else if (m.Groups["side"].Value == "X")
                        {
                            cs = "15529";
                            ce = "2";
                            descBet = "1x2 1°TEMPO";
                            descDraw = "X";
                        }
                        else if (m.Groups["side"].Value == "2")
                        {
                            cs = "15529";
                            ce = "3";
                            descBet = "1x2 1°TEMPO";
                            descDraw = "2";
                        }
                    }
                }
                else if (sport == "hockey")
                {//2 [regular time]-
                    if (m.Groups["period"].Value == "[regular time]")
                    {
                        if (m.Groups["side"].Value == "1")
                        {
                            cs = "293";
                            ce = "1";
                            descBet = "1X2 (NO OT)";
                            descDraw = "1";
                        }
                        else if (m.Groups["side"].Value == "X")
                        {
                            cs = "293";
                            ce = "2";
                            descBet = "1X2 (NO OT)";
                            descDraw = "X";
                        }
                        else if (m.Groups["side"].Value == "2")
                        {
                            cs = "293";
                            ce = "3";
                            descBet = "1X2 (NO OT)";
                            descDraw = "2";
                        }
                    }
                }
            }
            //AH1(+7.5)  
            //AH2(-3.5) [with OT]-
            //AH2(0.0)/DNB
            //AH2(0.0)/DNB [1st half]-
            //AH1(−17.5) Tempi supplementari 
            mc = Regex.Matches(outcome, "^AH(?<team>\\d)\\((?<handicap>((-?|\\+?)\\d*\\.{0,1}\\d+))\\)[ \\t]?(?<isdnb>(\\/DNB)?)[ \\t]?(?<period>((\\[1st per\\])|(\\[1st half\\])|(\\[with OT\\])|(\\[regular time\\]))?)");
            if (mc.Count == 1)
            {
                Match m = mc[0];

                if (m.Groups["isdnb"].Value != "")
                {
                    if (sport == "soccer")
                    {
                        if (string.IsNullOrEmpty(m.Groups["period"].Value))
                        {//AH2(0.0)/DNB
                            cs = "17875";
                            descBet = "Draw No Bet";
                            if (m.Groups["team"].Value == "1")
                            {                                
                                ce = "1";                                
                                descDraw = "1";
                            }
                            else if (m.Groups["team"].Value == "2")
                            {                                
                                ce = "2";                                
                                descDraw = "2";
                            }
                        }
                        else if (m.Groups["period"].Value == "[1st half]")
                        {
                            cs = "27989";
                            descBet = "DRAW NO BET 1°T";
                            handicap = "1";
                            //AH2(0.0)/DNB [1st half]-
                            if (m.Groups["team"].Value == "1")
                            {                                
                                ce = "1";                                                           
                                descDraw = "1";
                            }
                            else if (m.Groups["team"].Value == "2")
                            {                                
                                ce = "2";                                
                                descDraw = "2";
                            }
                        }
                    }
                }
                else
                {
                    if (sport == "basketball")
                    {
                        if (m.Groups["period"].Value == "[with OT]")
                        {
                            //AH1(+2.5) [with OT]-
                            cs = "26";
                            descBet = $"T/T HANDICAP {m.Groups["handicap"].Value}";
                            handicap = ((int)(Utils.ParseToDouble(m.Groups["handicap"].Value) * 100)).ToString();
                            if (m.Groups["team"].Value == "1")
                            {                                
                                ce = "1";                                
                                descDraw = "1";
                            }
                            else if (m.Groups["team"].Value == "2")
                            {                                
                                ce = "2";                                
                                descDraw = "2";
                            }
                        }
                    }
                    else if (sport == "tennis")
                    {
                        cs = "1127";
                        descBet = $"Handicap {m.Groups["handicap"].Value} Games";
                        handicap = ((int)(Utils.ParseToDouble(m.Groups["handicap"].Value) * 100)).ToString();
                        if (m.Groups["team"].Value == "1")
                        {                            
                            ce = "1";                            
                            descDraw = "1";
                        }
                        else if (m.Groups["team"].Value == "2")
                        {                            
                            ce = "2";                            
                            descDraw = "2";
                        }
                    }
                    else if (sport == "hockey")
                    {
                        if (m.Groups["period"].Value == "[regular time]")
                        {
                            cs = "26";
                            descBet = $"T/T HANDICAP (OT INCL. ) {m.Groups["handicap"].Value}";
                            handicap = ((int)(Utils.ParseToDouble(m.Groups["handicap"].Value) * 100)).ToString();
                            if (m.Groups["team"].Value == "1")
                            {                                
                                ce = "1";                                
                                descDraw = "1";
                            }
                            else if (m.Groups["team"].Value == "2")
                            {                                
                                ce = "2";                                
                                descDraw = "2";
                            }
                        }
                        else if (m.Groups["period"].Value == "[1st per]")
                        {
                            //when handicap is 0, we have to check
                            if (m.Groups["handicap"].Value == "0.5" || m.Groups["handicap"].Value == "-0.5")
                            {
                                cs = "1220";
                                descBet = $"HANDICAP PERIODO {m.Groups["handicap"].Value} 1";
                                if (m.Groups["handicap"].Value == "0.5")
                                    handicap = "65541";
                                else if (m.Groups["handicap"].Value == "-0.5")
                                    handicap = "131067";
                                if (m.Groups["team"].Value == "1")
                                {
                                    ce = "1";
                                    descDraw = "1";
                                }
                                else if (m.Groups["team"].Value == "2")
                                {
                                    ce = "2";
                                    descDraw = "2";
                                }
                            }                            
                        }
                    }
                    else if (sport == "handball")
                    {
                        if (m.Groups["period"].Value == "")
                        {
                            cs = "26";
                            descBet = $"T/T HANDICAP (OT INCL. ) {m.Groups["handicap"].Value}";
                            handicap = ((int)(Utils.ParseToDouble(m.Groups["handicap"].Value) * 100)).ToString();
                            if (m.Groups["team"].Value == "1")
                            {
                                ce = "1";
                                descDraw = "1";
                            }
                            else if (m.Groups["team"].Value == "2")
                            {
                                ce = "2";
                                descDraw = "2";
                            }
                        }
                    }
                }
            }

            //TO(3.5)
            //TO(1.5) for Team1
            //TO(166.5) [with OT]-
            //TO(31.5) [1st half]-
            //TO(0.5) for Team2[1st half] -
            mc = Regex.Matches(outcome, "^(?<side>(TO|TU))\\((?<handicap>((-?|\\+?)\\d*\\.{0,1}\\d+))\\)[ \\t]?(?<team>(for Team\\d)?)[ \\t]?(?<period>((\\[with OT\\])|(\\[1st half\\])|(\\[regular time\\])|(\\[with OT & SO\\]))?)(?<iscard>(- Cards)?)");
            if (mc.Count == 1)
            {
                Match m = mc[0];
                if (sport == "soccer")
                {
                    if (m.Groups["period"].Value == "")
                    {
                        handicap = ((int)(Utils.ParseToDouble(m.Groups["handicap"].Value) * 100)).ToString();
                        if (m.Groups["iscard"].Value == "")
                        {
                            if (m.Groups["team"].Value == "")
                            {
                                cs = "7989";
                                if (m.Groups["side"].Value == "TU")
                                {
                                    ce = "1";
                                    descDraw = "UNDER";
                                }
                                else if (m.Groups["side"].Value == "TO")
                                {
                                    ce = "2";
                                    descDraw = "OVER";
                                }
                                descBet = $"U/O {m.Groups["handicap"].Value}";

                            }
                            else if (m.Groups["team"].Value == "for Team1")
                            {
                                cs = "1749";
                                if (m.Groups["side"].Value == "TU")
                                {
                                    descBet = $"U/O {m.Groups["handicap"].Value} Casa";
                                    ce = "1";
                                    descDraw = "UNDER";
                                }
                                else if (m.Groups["side"].Value == "TO")
                                {
                                    descBet = $"U/O {m.Groups["handicap"].Value} Casa";
                                    ce = "2";
                                    descDraw = "OVER";
                                }
                            }
                            else if (m.Groups["team"].Value == "for Team2")
                            {
                                cs = "1750";
                                if (m.Groups["side"].Value == "TU")
                                {
                                    descBet = $"U/O {m.Groups["handicap"].Value} Ospite";
                                    ce = "1";
                                    descDraw = "UNDER";
                                }
                                else if (m.Groups["side"].Value == "TO")
                                {
                                    descBet = $"U/O {m.Groups["handicap"].Value} Ospite";
                                    ce = "2";
                                    descDraw = "OVER";
                                }
                            }
                        }
                        else
                        {
                            if (m.Groups["team"].Value == "")
                            {
                                cs = "890";
                                if (m.Groups["side"].Value == "TU")
                                {
                                    ce = "1";
                                    descDraw = "UNDER";
                                }
                                else if (m.Groups["side"].Value == "TO")
                                {
                                    ce = "2";
                                    descDraw = "OVER";
                                }
                                descBet = $"U/O CARTELLINI INCONTRO {m.Groups["handicap"].Value}";

                            }
                        }
                    }
                    else if (m.Groups["period"].Value == "[1st half]")
                    {
                        handicap = (65531 + (int)((Utils.ParseToDouble(m.Groups["handicap"].Value) + 0.5) * 10)).ToString();
                        if (m.Groups["team"].Value == "for Team1")
                        {
                            cs = "22291";
                            descBet = $"U/O {m.Groups["handicap"].Value} Casa 1°T";
                            if (m.Groups["side"].Value == "TU")
                            {
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "TO")
                            {
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                        else if (m.Groups["team"].Value == "for Team2")
                        {
                            cs = "22292";
                            descBet = $"U/O {m.Groups["handicap"].Value} Ospite 1°T";
                            if (m.Groups["side"].Value == "TU")
                            {
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "TO")
                            {
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                        else
                        {
                            cs = "9942";
                            descBet = $"U/O {m.Groups["handicap"].Value} 1°T";
                            if (m.Groups["side"].Value == "TU")
                            {
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "TO")
                            {
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                    }
                }
                else if (sport == "basketball")
                {
                    if (m.Groups["team"].Value == "")
                    {
                        if (m.Groups["period"].Value == "[1st half]")
                        {                           
                            handicap = (65531 + (int)((Utils.ParseToDouble(m.Groups["handicap"].Value) + 0.5) * 10)).ToString();
                            cs = "4816";
                            descBet = $"U/O {m.Groups["handicap"].Value} Quarto 1";

                            if (m.Groups["side"].Value == "TU")
                            {
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "TO")
                            {
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                        else if (m.Groups["period"].Value == "[with OT]")
                        {
                            handicap = ((int)(Utils.ParseToDouble(m.Groups["handicap"].Value) * 100)).ToString();
                            cs = "14863";
                            descBet = $"U/O Punti {m.Groups["handicap"].Value}";
                            if (m.Groups["side"].Value == "TU")
                            {                                
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "TO")
                            {                                
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                    }
                    else if (m.Groups["team"].Value != "")
                    {
                        if (m.Groups["period"].Value == "[with OT]")
                        {
                            if (m.Groups["team"].Value == "for Team1")
                            {
                                handicap = ((int)(Utils.ParseToDouble(m.Groups["handicap"].Value) * 100)).ToString();
                                cs = "12501";
                                descBet = $"U/O CASA {m.Groups["handicap"].Value}";
                                if (m.Groups["side"].Value == "TU")
                                {                                    
                                    ce = "1";
                                    descDraw = "UNDER";
                                }
                                else if (m.Groups["side"].Value == "TO")
                                {                                    
                                    ce = "2";
                                    descDraw = "OVER";
                                }
                            }
                            else if (m.Groups["team"].Value == "for Team2")
                            {
                                handicap = ((int)(Utils.ParseToDouble(m.Groups["handicap"].Value) * 100)).ToString();
                                cs = "12502";
                                descBet = $"U/O OSPITE {m.Groups["handicap"].Value}";
                                if (m.Groups["side"].Value == "TU")
                                {                                    
                                    ce = "1";
                                    descDraw = "UNDER";
                                }
                                else if (m.Groups["side"].Value == "TO")
                                {                                    
                                    ce = "2";
                                    descDraw = "OVER";
                                }
                            }
                        }
                    }
                }
                else if (sport == "tennis")
                {
                    if (m.Groups["team"].Value == "")
                    {
                        if (m.Groups["period"].Value == "")
                        {
                            handicap = ((int)(Utils.ParseToDouble(m.Groups["handicap"].Value) * 100)).ToString();
                            cs = "13464";
                            descBet = $"U/O Games {m.Groups["handicap"].Value}";
                            if (m.Groups["side"].Value == "TU")
                            {                                
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "TO")
                            {                                
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                    }
                }
                else if (sport == "hockey")
                {
                    if (m.Groups["team"].Value == "")
                    {
                        if (m.Groups["period"].Value == "[with OT & SO]")
                        {
                            handicap = ((int)(Utils.ParseToDouble(m.Groups["handicap"].Value) * 100)).ToString();
                            cs = "15539";
                            descBet = $"U/O (OT&SO) {m.Groups["handicap"].Value}";

                            if (m.Groups["side"].Value == "TU")
                            {
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "TO")
                            {
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                    }
                    else if (m.Groups["team"].Value != "")
                    {
                        if (m.Groups["period"].Value == "[regular time]")
                        {
                            if (m.Groups["team"].Value == "for Team1")
                            {
                                handicap = ((int)(Utils.ParseToDouble(m.Groups["handicap"].Value) * 100)).ToString();
                                cs = "10757";
                                descBet = $"U/O CASA (NO OT) {m.Groups["handicap"].Value}";
                                if (m.Groups["side"].Value == "TU")
                                {
                                    ce = "1";
                                    descDraw = "UNDER";
                                }
                                else if (m.Groups["side"].Value == "TO")
                                {
                                    ce = "2";
                                    descDraw = "OVER";
                                }
                            }
                            else if (m.Groups["team"].Value == "for Team2")
                            {
                                handicap = ((int)(Utils.ParseToDouble(m.Groups["handicap"].Value) * 100)).ToString();
                                cs = "10758";
                                descBet = $"U/O OSPITE (NO OT) {m.Groups["handicap"].Value}";
                                if (m.Groups["side"].Value == "TU")
                                {                                    
                                    ce = "1";
                                    descDraw = "UNDER";
                                }
                                else if (m.Groups["side"].Value == "TO")
                                {                                    
                                    ce = "2";
                                    descDraw = "OVER";
                                }
                            }
                        }
                    }
                }
                else if (sport == "handball")
                {
                    if (m.Groups["team"].Value == "")
                    {
                        if (m.Groups["period"].Value == "")
                        {
                            handicap = ((int)(Utils.ParseToDouble(m.Groups["handicap"].Value) * 100)).ToString();
                            cs = "13404";
                            descBet = $"UNDER/OVER {m.Groups["handicap"].Value}";
                            if (m.Groups["side"].Value == "TU")
                            {                                
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "TO")
                            {                                
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                    }
                }
            }

            //EH1(-1)
            //EH1(-1) [regular time]-
            mc = Regex.Matches(outcome, "^EH(?<team>\\d)\\((?<handicap>((-?|\\+?)\\d*\\.{0,1}\\d+))\\)[ \\t]?(?<period>(\\[regular time\\])?)");
            if (mc.Count == 1)
            {
                Match m = mc[0];

                if (sport == "soccer")
                {
                    if (m.Groups["period"].Value == "")
                    {
                        handicap = ((int)(Utils.ParseToDouble(m.Groups["handicap"].Value) * 100)).ToString();
                        string descBetAddition = "";
                        int handicapValue = Convert.ToInt32(m.Groups["handicap"].Value);
                        if (handicapValue < 0)
                            descBetAddition = $"{Math.Abs(handicapValue)}:0";
                        else
                            descBetAddition = $"0:{Math.Abs(handicapValue)}";
                        cs = "8";
                        if (m.Groups["team"].Value == "1")
                        {
                            descBet = $"1X2 HANDICAP {descBetAddition}";
                            ce = "1";
                            descDraw = "1";
                        }
                        else if (m.Groups["team"].Value == "2")
                        {
                            descBet = $"1X2 HANDICAP {descBetAddition}";
                            ce = "3";
                            descDraw = "2";
                        }
                    }
                }
                else if (sport == "hockey")
                {
                    if (m.Groups["period"].Value == "[regular time]")
                    {
                        handicap = ((int)(Utils.ParseToDouble(m.Groups["handicap"].Value) * 100)).ToString();
                        string descBetAddition = "";
                        int handicapValue = Convert.ToInt32(m.Groups["handicap"].Value);
                        if (handicapValue < 0)
                            descBetAddition = $"{Math.Abs(handicapValue)}:0";
                        else
                            descBetAddition = $"0:{Math.Abs(handicapValue)}";
                        cs = "8";
                        if (m.Groups["team"].Value == "1")
                        {
                            descBet = $"1X2 HANDICAP {descBetAddition}";
                            ce = "1";
                            descDraw = "1";
                        }
                        else if (m.Groups["team"].Value == "2")
                        {
                            descBet = $"1X2 HANDICAP {descBetAddition}";
                            ce = "3";
                            descDraw = "2";
                        }
                    }
                }
            }

            //Team1 Win [with OT]-
            mc = Regex.Matches(outcome, "^(?<team>Team\\d) Win[ \\t]?(?<period>((\\[with OT\\])|(\\[with OT & SO\\]))?)");
            if (mc.Count == 1)
            {
                Match m = mc[0];
                if (sport == "basketball")
                {
                    if (m.Groups["period"].Value == "[with OT]")
                    {
                        cs = "110";
                        descBet = $"T/T (OT INCL.)";
                        if (m.Groups["team"].Value == "Team1")
                        {
                            ce = "1";
                            descDraw = "1";
                        }
                        else if (m.Groups["team"].Value == "Team2")
                        {
                            ce = "2";
                            descDraw = "2";
                        }
                    }
                }
                else if (sport == "hockey")
                {
                    if (m.Groups["period"].Value == "[with OT & SO]")
                    {
                        cs = "110";
                        descBet = $"T/T (OT INCL.)";
                        if (m.Groups["team"].Value == "Team1")
                        {
                            ce = "1";
                            descDraw = "1";
                        }
                        else if (m.Groups["team"].Value == "Team2")
                        {
                            ce = "2";
                            descDraw = "2";
                        }
                    }
                }
                else if (sport == "tennis")
                {
                    if (m.Groups["period"].Value == "")
                    {
                        cs = "20540";
                        descBet = $"T/T (Escl. Ritiro)";
                        if (m.Groups["team"].Value == "Team1")
                        {
                            ce = "1";
                            descDraw = "1";
                        }
                        else if (m.Groups["team"].Value == "Team2")
                        {
                            ce = "2";
                            descDraw = "2";
                        }
                    }
                }
            }

            //Exact (0) for Team1
            mc = Regex.Matches(outcome, "^Exact \\((?<handicap>((-?|\\+?)\\d*\\.{0,1}\\d+))\\)[ \\t]for[ \\t](?<team>Team\\d)");
            if (mc.Count == 1)
            {
                Match m = mc[0];
                if (sport == "soccer")
                {
                    if (m.Groups["team"].Value == "Team1")
                    {
                        cs = "570";
                        descBet = $"Somma Gol Casa";

                        if (m.Groups["handicap"].Value == "0")
                        {
                            ce = "1";
                            descDraw = "0";
                        }
                        else if (m.Groups["handicap"].Value == "1")
                        {
                            ce = "2";
                            descDraw = "1";
                        }
                        else if (m.Groups["handicap"].Value == "2")
                        {
                            ce = "3";
                            descDraw = "2";
                        }
                        else if (m.Groups["handicap"].Value == ">2")
                        {
                            ce = "4";
                            descDraw = ">2";
                        }
                    }
                    else if (m.Groups["team"].Value == "Team2")
                    {
                        cs = "571";
                        descBet = $"Somma Gol Ospite";

                        if (m.Groups["handicap"].Value == "0")
                        {
                            ce = "1";
                            descDraw = "0";
                        }
                        else if (m.Groups["handicap"].Value == "1")
                        {
                            ce = "2";
                            descDraw = "1";
                        }
                        else if (m.Groups["handicap"].Value == "2")
                        {
                            ce = "3";
                            descDraw = "2";
                        }
                        else if (m.Groups["handicap"].Value == ">2")
                        {
                            ce = "4";
                            descDraw = ">2";
                        }
                    }
                }
            }

            //1 / DNB 1º periodo
            mc = Regex.Matches(outcome, "^(?<team>\\d)[ \\t]?\\/[ \\t]?DNB[ \\t]?(?<period>(\\dº periodo)?)");
            if (mc.Count == 1)
            {
                Match m = mc[0];
                if (sport == "soccer")
                {
                    if (m.Groups["period"].Value == "1º periodo")
                    {
                        cs = "27989";
                        descBet = "DRAW NO BET 1°T";
                        handicap = "1";
                        if (m.Groups["team"].Value == "1")
                        {                            
                            ce = "1";                            
                            descDraw = "1";
                        }
                        else if(m.Groups["team"].Value == "2")
                        {
                            ce = "2";
                            descDraw = "2";
                        }
                    }
                    else if (m.Groups["period"].Value == "2º periodo")
                    {
                        cs = "27989";
                        descBet = "DRAW NO BET 1°T";
                        handicap = "2";
                        if (m.Groups["team"].Value == "1")
                        {
                            ce = "1";
                            descDraw = "1";
                        }
                        else if (m.Groups["team"].Value == "2")
                        {
                            ce = "2";
                            descDraw = "2";
                        }
                    }
                }
            }

            //1(0:2)
            //1(0:2) Tempi supplementari
            mc = Regex.Matches(outcome, "^(?<team>\\d)\\((?<handicap1>((-?|\\+?)\\d*\\.{0,1}\\d+)):(?<handicap2>((-?|\\+?)\\d*\\.{0,1}\\d+))\\)[ \\t]?(?<period>(Tempi supplementari)?)");
            if (mc.Count == 1)
            {
                Match m = mc[0];

                int handicap1 = Utils.parseToInt(m.Groups["handicap1"].Value);
                int handicap2 = Utils.parseToInt(m.Groups["handicap2"].Value);

                if (sport == "soccer")
                {
                    if (m.Groups["period"].Value == "")
                    {
                        cs = "8";
                        descBet = $"1X2 HANDICAP ({handicap1}:{handicap2})";
                        if (handicap1 == 0)
                        {
                            handicap = (handicap2 * 100).ToString();
                        }
                        else if (handicap2 == 0)
                        {
                            handicap = (-handicap1 * 100).ToString();
                        }

                        if (m.Groups["team"].Value == "1")
                        {
                            ce = "1";
                            descDraw = "1";
                        }
                        else if (m.Groups["team"].Value == "2")
                        {
                            ce = "3";
                            descDraw = "2";
                        }
                    }
                }
                else if (sport == "basketball")
                {
                    if (m.Groups["period"].Value == "Tempi supplementari")
                    {
                    }
                }
            }

            //Meno di 30.5 1º periodo
            //Meno di 49.5 Tempi supplementari
            //Meno di 72.5 Tempi supplementari 2ª squadra
            //Meno di 0.5 1º periodo 2ª squadra
            //Più di 55.5 Tempi supplementari
            //Più di 84.5 1º tempo
            mc = Regex.Matches(outcome, "^(?<side>(Meno|Più)) di (?<handicap>((-?|\\+?)\\d*\\.{0,1}\\d+))[ \\t]?(?<period>((\\dº tempo)|(Tempi supplementari)|(\\dº periodo))?)[ \\t]?(?<team>(\\dª squadra)?)");
            if (mc.Count == 1)
            {
                Match m = mc[0];
                if (sport == "soccer")
                {
                    handicap = (65531 + (int)((Utils.ParseToDouble(m.Groups["handicap"].Value) + 0.5) * 10)).ToString();

                    if (m.Groups["period"].Value == "1º periodo")
                    {
                        if (m.Groups["team"].Value == "")
                        {
                            cs = "9942";
                            descBet = $"U/O {m.Groups["handicap"].Value} 1°T";
                            if (m.Groups["side"].Value == "Meno") //Less
                            {
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "Più") //More
                            {
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                        else if (m.Groups["team"].Value == "1ª squadra")
                        {
                            cs = "22291";
                            descBet = $"U/O {m.Groups["handicap"].Value} Casa 1°T";

                            if (m.Groups["side"].Value == "Meno") //Less
                            {
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "Più") //More
                            {
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                        else if (m.Groups["team"].Value == "2ª squadra")
                        {
                            cs = "22292";
                            descBet = $"U/O Ospite {m.Groups["handicap"].Value}";

                            if (m.Groups["side"].Value == "Meno") //Less
                            {
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "Più") //More
                            {
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                    }
                }
                else if (sport == "handball")
                {

                }
                else if (sport == "football")
                {
                    handicap = ((int)(Utils.ParseToDouble(m.Groups["handicap"].Value) * 100)).ToString();
                    
                    if (m.Groups["period"].Value == "Tempi supplementari")
                    {
                        if (m.Groups["team"].Value == "")
                        {
                            cs = "8406";
                            descBet = $"U/O Punti {m.Groups["handicap"].Value}";

                            if (m.Groups["side"].Value == "Meno") //Less
                            {
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "Più") //More
                            {
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                        else if (m.Groups["team"].Value == "1ª squadra")
                        {
                            cs = "8413";
                            descBet = $"U/O Casa {m.Groups["handicap"].Value}";

                            if (m.Groups["side"].Value == "Meno") //Less
                            {
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "Più") //More
                            {
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                        else if (m.Groups["team"].Value == "2ª squadra")
                        {
                            cs = "8414";
                            descBet = $"U/O Ospite {m.Groups["handicap"].Value}";

                            if (m.Groups["side"].Value == "Meno") //Less
                            {
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "Più") //More
                            {
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                    }
                }
                else if (sport == "basketball")
                {
                    handicap = ((int)(Utils.ParseToDouble(m.Groups["handicap"].Value) * 100)).ToString();
                    
                    if (m.Groups["period"].Value == "Tempi supplementari")
                    {
                        if (m.Groups["team"].Value == "")
                        {
                            cs = "14863";
                            descBet = $"U/O Punti {m.Groups["handicap"].Value}";

                            if (m.Groups["side"].Value == "Meno") //Less
                            {
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "Più") //More
                            {
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                        else if (m.Groups["team"].Value == "1ª squadra")
                        {
                            cs = "12501";
                            descBet = $"U/O Casa {m.Groups["handicap"].Value}";

                            if (m.Groups["side"].Value == "Meno") //Less
                            {
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "Più") //More
                            {
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                        else if(m.Groups["team"].Value == "2ª squadra")
                        {
                            cs = "12502";
                            descBet = $"U/O Ospite {m.Groups["handicap"].Value}";

                            if (m.Groups["side"].Value == "Meno") //Less
                            {
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "Più") //More
                            {
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                    }
                    else if (m.Groups["period"].Value == "1º tempo")
                    {
                        if (m.Groups["team"].Value == "")
                        {
                            handicap = (65531 + (int)((Utils.ParseToDouble(m.Groups["handicap"].Value) + 0.5) * 10)).ToString();
                            cs = "4816";
                            descBet = $"U/O {m.Groups["handicap"].Value} Quarto 1";

                            if (m.Groups["side"].Value == "Meno") //Less
                            {
                                ce = "1";
                                descDraw = "UNDER";
                            }
                            else if (m.Groups["side"].Value == "Più") //More
                            {
                                ce = "2";
                                descDraw = "OVER";
                            }
                        }
                    }
                }
            }


            //One scoreless
            if (outcome == "One scoreless")
            {
                if (sport == "soccer")
                {
                }
            }
            //21-2 TS+SO  
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
    
        public PROCESS_RESULT PlaceBet(BetburgerInfo info)
        {
            if (getBalance() < 0)
                login();
            try
            {             

                info.sport = info.sport.ToLower();
                string alias = "";
                string pal = "";
                string avv = "";
                string idAggregata = "";
                string descEvent = "";
                string descSport = "";
                string descTournament = "";
                string sportId = "";
                string tournamentId = "";
                string timestamp = "";
                string maxEventName = "";

                string isLive = "";
                if (info.kind == PickKind.Type_1)
                {
                    string[] directlinkParams = info.direct_link.Split(',');
                    //string directlink_p = directlinkParams[3];
                    string driectlink_bid = Utils.Between(info.direct_link, "&bId=", "&");

                    isLive = "true";
                    if (info.sport == "soccer")
                    {
                        sportId = "1";
                        idAggregata = "-10001";
                    }
                    else if (info.sport == "basketball")
                    {
                        sportId = "2";
                        idAggregata = "-10002";
                    }
                    else if (info.sport == "tennis")
                    {
                        sportId = "5";
                        idAggregata = "-10005";
                    }
                    else if (info.sport == "volleyball")
                    {
                        sportId = "23";
                        idAggregata = "-10023";
                    }
                    else if (info.sport == "hockey")
                    {
                        sportId = "4";
                        idAggregata = "-10004";
                    }
                    else if (info.sport == "football")
                    {
                        sportId = "16";
                        idAggregata = "-10016";
                    }
                    else if (info.sport == "handball")
                    {
                        sportId = "6";
                        idAggregata = "-10006";
                    }
                    else if (info.sport == "table tennis")
                    {
                        sportId = "20";
                        idAggregata = "-10020";
                    }
                    //betburger live pick
                    string eventUrl = $"https://www.{Setting.Instance.domain}/XSportDatastore/getMenuLiveGenerale?systemCode={SYSTEMCODE}&lingua=IT&hash=&sportId={sportId}";
                    HttpResponseMessage searchEventResponse = m_client.PostAsync(eventUrl, null).Result;
                    searchEventResponse.EnsureSuccessStatusCode();
                    string searchEventResponseContent = searchEventResponse.Content.ReadAsStringAsync().Result;

#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus("GetMatch Response");
                    LogMng.Instance.onWriteStatus(searchEventResponseContent);
#endif

                    dynamic jsonEventInfo = JsonConvert.DeserializeObject<dynamic>(searchEventResponseContent);
                    
                    foreach (dynamic spsItr in jsonEventInfo.sps)
                    {
                        if (spsItr.id.ToString() != sportId)
                            continue;
                        foreach (dynamic ctsItr in spsItr.cts)
                        {
                            foreach (dynamic tnsItr in ctsItr.tns)
                            {
                                foreach (dynamic elItr in tnsItr.el)
                                {
                                    if (elItr.bid.ToString() == driectlink_bid)
                                    {
                                        alias = elItr.al.ToString();
                                        pal = elItr.p.ToString();
                                        avv = elItr.a.ToString();
                                        

                                        descEvent = elItr.dsl.IT.ToString();
                                        descSport = spsItr.dsl.IT.ToString();
                                        descTournament = tnsItr.dsl.IT.ToString();
                                        
                                        tournamentId = tnsItr.id.ToString();
                                        timestamp = "";
                                        
                                        break;
                                    }
                                }
                                if (!string.IsNullOrEmpty(pal))
                                    break;
                            }
                            if (!string.IsNullOrEmpty(pal))
                                break;
                        }
                    }

                    
                }
                else
                {
                    isLive = "false";
                    //telegramtip prematch

                    //Searching Events.
                    string eventUrl = $"https://www.{Setting.Instance.domain}/XSportDatastore/getAutocompleteDataBatch?systemCode={SYSTEMCODE}&lingua=IT&hash=";
                    HttpResponseMessage searchEventResponse = m_client.PostAsync(eventUrl, null).Result;
                    searchEventResponse.EnsureSuccessStatusCode();
                    string searchEventResponseContent = searchEventResponse.Content.ReadAsStringAsync().Result;

                    dynamic jsonEventInfo = JsonConvert.DeserializeObject<dynamic>(searchEventResponseContent);
                    string prematchResult = jsonEventInfo.prematch.ToString();
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus("GetMatch Response");
                LogMng.Instance.onWriteStatus(prematchResult);
#endif

                    dynamic eventObjects = JsonConvert.DeserializeObject<dynamic>(prematchResult);

                    double maxSimilarity = 0;

                    int nCounter = 0;
                    foreach (dynamic evt in eventObjects.ms)
                    {
                        nCounter++;
                        string eventName = evt.dsl.IT.ToString();

                        string startTime = evt.ts.ToString(); 

                        string[] eventStarTimes = startTime.Split(' ');

                        string[] infoStarTimes = info.updated.Split(' ');
                        if (eventStarTimes.Length == 2 && infoStarTimes.Length == 3)
                        {
                            if (!eventStarTimes[0].EndsWith(infoStarTimes[0]) || !eventStarTimes[1].StartsWith(infoStarTimes[2]))
                                continue;
                        }

                        double similarity = Similarity.GetSimilarityRatio(eventName, info.eventTitle, out double ratio1, out double ratio2);

                        if (eventName.Contains(info.homeTeam) || eventName.Contains(info.awayTeam))
                            similarity = 50;
                        if (maxSimilarity < similarity)
                        {
                            maxSimilarity = similarity;

                            alias = evt.al.ToString();
                            pal = evt.p.ToString();
                            avv = evt.a.ToString();
                            idAggregata = evt.ag.ToString();

                            descEvent = evt.dsl.IT.ToString();
                            descSport = evt.dsml.IT.ToString();
                            descTournament = evt.dtml.IT.ToString();
                            sportId = evt.s.ToString(); // check with other sports
                            tournamentId = evt.t.ToString();
                            timestamp = evt.ts.ToString();
                            maxEventName = eventName;
                        }
                    }

                    if (maxSimilarity < 50)
                    {
                        LogMng.Instance.onWriteStatus($"Can't find match : {info.eventTitle}");
                        return PROCESS_RESULT.ERROR;
                    }

                    LogMng.Instance.onWriteStatus($"Found Match name: {maxEventName}");
                    //GetMarketsFromEvent
                    if (info.sport == "soccer")
                        idAggregata = "-1";
                    else if (info.sport == "basketball")
                        idAggregata = "-2";
                    else if (info.sport == "tennis")
                        idAggregata = "-5";
                    else if (info.sport == "hockey")
                        idAggregata = "-4";
                    else if (info.sport == "football")
                        idAggregata = "-16";
                    else if (info.sport == "handball")
                        idAggregata = "-6";
                }

                if (string.IsNullOrEmpty(avv))
                {
                    LogMng.Instance.onWriteStatus($"Match not found");
                    return PROCESS_RESULT.ERROR;
                }

              
               

                string market_cs = "", market_ce = "", market_handicap = "", descBet = "", descDraw = "";
                string curOdd = "", curHandicap = "";

                if (info.kind == PickKind.Type_1)
                {
                    market_cs = Utils.Between(info.direct_link, "&marketId=", "&");
                    market_ce = Utils.Between(info.direct_link, "&outcomeId=", "&");
                    market_handicap = Utils.Between(info.direct_link, "&value=", "&");

                    descBet = WebUtility.UrlDecode(Utils.Between(info.direct_link, "&marketName=", "&"));
                    descDraw = WebUtility.UrlDecode(Utils.Between(info.direct_link, "&outcomeName=", "&"));
                }
                else
                {
                    try
                    {
                        ParseOutcome(info.sport, info.outcome, out market_cs, out market_ce, out market_handicap, out descBet, out descDraw);
                    }
                    catch
                    {
                        LogMng.Instance.onWriteStatus($"ParseOutcome exception: {info.sport} outcome: {info.outcome} ");
                    }
                }
                LogMng.Instance.onWriteStatus($"ParseOutcome: {info.sport} outcome: {info.outcome} market_cs: {market_cs} market_ce: {market_ce} market_handicap: {market_handicap} descBet: {descBet} descDraw: {descDraw}");

                if (!GetNewOdd(info, pal, avv, idAggregata, isLive, market_cs, market_ce, market_handicap, out curOdd, out curHandicap))
                {
                    LogMng.Instance.onWriteStatus($"Can't find odd, failed");
                    return PROCESS_RESULT.ERROR;
                }                               

                double newOdd = Utils.ParseToDouble(curOdd) / 100;
                double uptoOdd = info.profit;
                LogMng.Instance.onWriteStatus($"NewOdd: {newOdd} PickOdd: {info.odds} UptoOdd: {uptoOdd}");
                //if (newOdd < uptoOdd)
                //{
                //    LogMng.Instance.onWriteStatus($"Odd is dropped down than the bottom. {newOdd} : {uptoOdd}");
                //    return PROCESS_RESULT.ERROR;
                //}

                if (CheckOddDropCancelBet(newOdd, info))
                {
                    LogMng.Instance.onWriteStatus($"Lower odd, cancelled. ({info.odds}) -> ({newOdd})");
                    return PROCESS_RESULT.MOVED;
                }

                double winMoney = newOdd * info.stake * 100;

                string fatherTrancsactionId = "null", transactionId = "null";

                string functionString = "";
                if (isLive == "true")
                {

                    string FormDataLive = "{\"frontendType\":\"ONLINE\",\"betslip\":{\"isBetBuilder\":false,\"defaultAmount\":100,\"defaultAmountSystem\":200,\"coupons\":{\"1\":{\"selections\":[{\"pal\":*pal*,\"avv\":*avv*,\"banker\":false,\"descEvent\":\"*descEvent*\",\"isLive\":*islive*,\"bets\":[{\"scomSogei\":*scomSogei*,\"handicap\":*handicap*,\"descBet\":\"*descBet*\",\"descDraw\":\"*descDraw*\",\"draw\":*draw*,\"odd\":\"*odd*\",\"legabile\":false,\"combinationType\":0,\"amount\":*amount*,\"printableAmount\":\"*printableAmount*\",\"minPl\":1,\"maxPl\":30,\"minPlError\":false,\"potentialWinning\":*potentialWinning*,\"stakeTaxedAmount\":0,\"stakeTaxedPotWin\":0,\"selectionTaxes\":0,\"updated\":null,\"closed\":null}],\"alias\":\"*alias*\",\"descSport\":\"*descSport*\",\"descTournament\":\"*descTournament*\",\"sportId\":*sportId*,\"tournamentId\":*tournamentId*,\"timestamp\":\"*timestamp*\",\"closed\":null}],\"totalDraws\":1,\"userSystemSelections\":{},\"calculatedSystemData\":{},\"selectedContainer\":\"singlebets\",\"potentialWinning\":*potentialWinning*,\"amountSystem\":0,\"amountMulti\":100,\"amountSingle\":*amountSingle*,\"totalCombs\":0,\"betOnAll\":200,\"betOnAllEnabled\":false,\"multipleBonus\":{\"bonusAmount\":0,\"totalWinWithBonus\":0,\"minEvents\":100,\"eventList\":[],\"appliedPercentage\":0,\"fasce\":{\"1\":0,\"2\":0,\"3\":0,\"4\":0,\"5\":0,\"6\":0,\"7\":0,\"8\":0,\"9\":0,\"10\":0,\"11\":0,\"12\":0,\"13\":0,\"14\":0,\"15\":0,\"16\":0,\"17\":0,\"18\":0,\"19\":0,\"20\":0,\"21\":0,\"22\":0,\"23\":0,\"24\":0,\"25\":0,\"26\":0,\"27\":0,\"28\":0,\"29\":0,\"30\":0},\"bonusElements\":[],\"description\":\"Bonus\",\"applied\":false},\"multipleTaxedBonus\":null,\"oddVariation\":\"NEVER\",\"printableSplitAmount\":0,\"printableAmountMulti\":\"1.00\",\"printableBetOnAll\":\"2.00\",\"minPotentialWinning\":0,\"maxPotentialWinning\":0,\"totalOdd\":\"1.00\",\"totalTaxAmount\":0,\"stakeTaxedAmountSingle\":0,\"stakeTaxedAmountMulti\":0,\"stakeTaxedAmountSystem\":0,\"stakeTaxedPotentialWinning\":0,\"stakeTaxedMaxPotentialWinning\":0,\"stakeTaxedMinPotentialWinning\":0,\"playabilityIssue\":false},\"2\":{\"selections\":[],\"totalDraws\":0,\"userSystemSelections\":{},\"calculatedSystemData\":{},\"selectedContainer\":\"singlebets\",\"potentialWinning\":0,\"amountSystem\":0,\"amountMulti\":100,\"amountSingle\":0,\"totalCombs\":0,\"betOnAll\":200,\"betOnAllEnabled\":false,\"multipleBonus\":null,\"multipleTaxedBonus\":null,\"oddVariation\":\"NEVER\",\"printableSplitAmount\":0,\"printableAmountMulti\":\"1.00\",\"printableBetOnAll\":\"2.00\"},\"3\":{\"selections\":[],\"totalDraws\":0,\"userSystemSelections\":{},\"calculatedSystemData\":{},\"selectedContainer\":\"singlebets\",\"potentialWinning\":0,\"amountSystem\":0,\"amountMulti\":100,\"amountSingle\":0,\"totalCombs\":0,\"betOnAll\":200,\"betOnAllEnabled\":false,\"multipleBonus\":null,\"multipleTaxedBonus\":null,\"oddVariation\":\"NEVER\",\"printableSplitAmount\":0,\"printableAmountMulti\":\"1.00\",\"printableBetOnAll\":\"2.00\"},\"4\":{\"selections\":[],\"totalDraws\":0,\"userSystemSelections\":{},\"calculatedSystemData\":{},\"selectedContainer\":\"singlebets\",\"potentialWinning\":0,\"amountSystem\":0,\"amountMulti\":100,\"amountSingle\":0,\"totalCombs\":0,\"betOnAll\":200,\"betOnAllEnabled\":false,\"multipleBonus\":null,\"multipleTaxedBonus\":null,\"oddVariation\":\"NEVER\",\"printableSplitAmount\":0,\"printableAmountMulti\":\"1.00\",\"printableBetOnAll\":\"2.00\"},\"5\":{\"selections\":[],\"totalDraws\":0,\"userSystemSelections\":{},\"calculatedSystemData\":{},\"selectedContainer\":\"singlebets\",\"potentialWinning\":0,\"amountSystem\":0,\"amountMulti\":100,\"amountSingle\":0,\"totalCombs\":0,\"betOnAll\":200,\"betOnAllEnabled\":false,\"multipleBonus\":null,\"multipleTaxedBonus\":null,\"oddVariation\":\"NEVER\",\"printableSplitAmount\":0,\"printableAmountMulti\":\"1.00\",\"printableBetOnAll\":\"2.00\"}},\"selectedCoupon\":\"1\",\"isMobile\":false,\"saveBetslip\":false,\"isPsv\":false}}";
                    FormDataLive = FormDataLive.Replace("*pal*", pal).Replace("*avv*", avv).Replace("*descEvent*", descEvent);
                    FormDataLive = FormDataLive.Replace("*scomSogei*", market_cs).Replace("*handicap*", curHandicap).Replace("*descBet*", descBet).Replace("*descDraw*", descDraw);
                    FormDataLive = FormDataLive.Replace("*draw*", market_ce).Replace("*odd*", curOdd).Replace("*amount*", ((int)(info.stake * 100)).ToString()).Replace("*printableAmount*", info.stake.ToString());
                    FormDataLive = FormDataLive.Replace("*potentialWinning*", ((int)winMoney).ToString()).Replace("*alias*", alias).Replace("*descSport*", descSport).Replace("*descTournament*", descTournament);
                    FormDataLive = FormDataLive.Replace("*sportId*", sportId).Replace("*tournamentId*", tournamentId).Replace("*timestamp*", timestamp).Replace("*amountSingle*", ((int)(info.stake * 100)).ToString());                    
                    FormDataLive = FormDataLive.Replace("*islive*", isLive);

                    string getlivePurchaseparamUrl = $"https://www.{Setting.Instance.domain}/XSportDatastore/getLivePurchaseParameters?systemCode={SYSTEMCODE}&lingua=IT&hash=&token={Global.DomusbetToken}&shopUserData=";
                    //string purchaseFormDataString = WebUtility.UrlEncode(FormData);
                    string getlivepurchaseFormDataString = FormDataLive.Replace("{", "%7B").Replace("}", "%7D").Replace("[", "%5B").Replace("]", "%5D").Replace("\"", "%22").Replace(" ", "%20");

                    functionString = $"window.fetch('{getlivePurchaseparamUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json; charset=UTF-8;' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', referer: 'https://www.{Setting.Instance.domain}/scommesse-sportive', origin: 'https://www.{Setting.Instance.domain}', body: '{getlivepurchaseFormDataString}', method: 'POST' }}).then(response => response.json());";


                    Global.strWebResponse4 = "";
                    Global.waitResponseEvent4.Reset();
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"GetLivePurchaseParam Request: {functionString}");
#endif
                    Global.RunScriptCode(functionString);



                    if (!Global.waitResponseEvent4.Wait(10000))
                    {
                        LogMng.Instance.onWriteStatus($"GetLivePurchaseParam No Response");
                        return PROCESS_RESULT.ERROR;
                    }
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus($"GetLivePurchaseParam Result: {Global.strWebResponse4}");
#endif

                    try
                    {
                        dynamic jsonGetLiveParamInfo = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse4);

                        fatherTrancsactionId = jsonGetLiveParamInfo.@object.fatherTransactionId.ToString();
                        transactionId = jsonGetLiveParamInfo.@object.transactionId.ToString();
                        int nDelayMilisec = Convert.ToInt32(jsonGetLiveParamInfo.@object.delay.ToString());
                        LogMng.Instance.onWriteStatus($"Wait for Live placing ({nDelayMilisec}ms) transactionId: {transactionId} fatherTrancsactionId: {fatherTrancsactionId}");
                        Thread.Sleep(nDelayMilisec);
                        //if (jsonGetLiveParamInfo.code.ToString() == "0")
                        //{

                        //    return PROCESS_RESULT.ERROR;
                        //}

                        if (!GetNewOdd(info, pal, avv, idAggregata, isLive, market_cs, market_ce, market_handicap, out curOdd, out curHandicap))
                        {
                            LogMng.Instance.onWriteStatus($"Can't find odd, failed (1)");
                            return PROCESS_RESULT.ERROR;
                        }

                        newOdd = Utils.ParseToDouble(curOdd) / 100;                        
                        LogMng.Instance.onWriteStatus($"NewOdd (1): {newOdd} PickOdd: {info.odds} UptoOdd: {uptoOdd}");
                        //if (newOdd < uptoOdd)
                        //{
                        //    LogMng.Instance.onWriteStatus($"Odd is dropped down than the bottom. {newOdd} : {uptoOdd}");
                        //    return PROCESS_RESULT.ERROR;
                        //}

                        if (CheckOddDropCancelBet(newOdd, info))
                        {
                            LogMng.Instance.onWriteStatus($"Lower odd (1), cancelled. ({info.odds}) -> ({newOdd})");
                            return PROCESS_RESULT.MOVED;
                        }

                        winMoney = newOdd * info.stake * 100;
                    }
                    catch { }
                }

                string FormData = "{\"frontendType\":\"ONLINE\",\"betslip\":{\"isBetBuilder\":false,\"defaultAmount\":100,\"defaultAmountSystem\":200,\"coupons\":{\"1\":{\"selections\":[{\"pal\":*pal*,\"avv\":*avv*,\"banker\":false,\"descEvent\":\"*descEvent*\",\"isLive\":*islive*,\"bets\":[{\"scomSogei\":*scomSogei*,\"handicap\":*handicap*,\"descBet\":\"*descBet*\",\"descDraw\":\"*descDraw*\",\"draw\":*draw*,\"odd\":\"*odd*\",\"legabile\":false,\"combinationType\":0,\"amount\":*amount*,\"printableAmount\":\"*printableAmount*\",\"minPl\":1,\"maxPl\":30,\"minPlError\":false,\"potentialWinning\":*potentialWinning*,\"stakeTaxedAmount\":0,\"stakeTaxedPotWin\":0,\"selectionTaxes\":0,\"updated\":null,\"closed\":null}],\"alias\":\"*alias*\",\"descSport\":\"*descSport*\",\"descTournament\":\"*descTournament*\",\"sportId\":*sportId*,\"tournamentId\":*tournamentId*,\"timestamp\":\"*timestamp*\",\"closed\":null}],\"totalDraws\":1,\"userSystemSelections\":{},\"calculatedSystemData\":{},\"selectedContainer\":\"singlebets\",\"potentialWinning\":*potentialWinning*,\"amountSystem\":0,\"amountMulti\":100,\"amountSingle\":*amountSingle*,\"totalCombs\":0,\"betOnAll\":200,\"betOnAllEnabled\":false,\"multipleBonus\":{\"bonusAmount\":0,\"totalWinWithBonus\":0,\"minEvents\":100,\"eventList\":[],\"appliedPercentage\":0,\"fasce\":{\"1\":0,\"2\":0,\"3\":0,\"4\":0,\"5\":0,\"6\":0,\"7\":0,\"8\":0,\"9\":0,\"10\":0,\"11\":0,\"12\":0,\"13\":0,\"14\":0,\"15\":0,\"16\":0,\"17\":0,\"18\":0,\"19\":0,\"20\":0,\"21\":0,\"22\":0,\"23\":0,\"24\":0,\"25\":0,\"26\":0,\"27\":0,\"28\":0,\"29\":0,\"30\":0},\"bonusElements\":[],\"description\":\"Bonus\",\"applied\":false},\"multipleTaxedBonus\":null,\"oddVariation\":\"NEVER\",\"printableSplitAmount\":0,\"printableAmountMulti\":\"1.00\",\"printableBetOnAll\":\"2.00\",\"minPotentialWinning\":0,\"maxPotentialWinning\":0,\"totalOdd\":\"1.00\",\"totalTaxAmount\":0,\"stakeTaxedAmountSingle\":0,\"stakeTaxedAmountMulti\":0,\"stakeTaxedAmountSystem\":0,\"stakeTaxedPotentialWinning\":0,\"stakeTaxedMaxPotentialWinning\":0,\"stakeTaxedMinPotentialWinning\":0,\"playabilityIssue\":false},\"2\":{\"selections\":[],\"totalDraws\":0,\"userSystemSelections\":{},\"calculatedSystemData\":{},\"selectedContainer\":\"singlebets\",\"potentialWinning\":0,\"amountSystem\":0,\"amountMulti\":100,\"amountSingle\":0,\"totalCombs\":0,\"betOnAll\":200,\"betOnAllEnabled\":false,\"multipleBonus\":null,\"multipleTaxedBonus\":null,\"oddVariation\":\"NEVER\",\"printableSplitAmount\":0,\"printableAmountMulti\":\"1.00\",\"printableBetOnAll\":\"2.00\"},\"3\":{\"selections\":[],\"totalDraws\":0,\"userSystemSelections\":{},\"calculatedSystemData\":{},\"selectedContainer\":\"singlebets\",\"potentialWinning\":0,\"amountSystem\":0,\"amountMulti\":100,\"amountSingle\":0,\"totalCombs\":0,\"betOnAll\":200,\"betOnAllEnabled\":false,\"multipleBonus\":null,\"multipleTaxedBonus\":null,\"oddVariation\":\"NEVER\",\"printableSplitAmount\":0,\"printableAmountMulti\":\"1.00\",\"printableBetOnAll\":\"2.00\"},\"4\":{\"selections\":[],\"totalDraws\":0,\"userSystemSelections\":{},\"calculatedSystemData\":{},\"selectedContainer\":\"singlebets\",\"potentialWinning\":0,\"amountSystem\":0,\"amountMulti\":100,\"amountSingle\":0,\"totalCombs\":0,\"betOnAll\":200,\"betOnAllEnabled\":false,\"multipleBonus\":null,\"multipleTaxedBonus\":null,\"oddVariation\":\"NEVER\",\"printableSplitAmount\":0,\"printableAmountMulti\":\"1.00\",\"printableBetOnAll\":\"2.00\"},\"5\":{\"selections\":[],\"totalDraws\":0,\"userSystemSelections\":{},\"calculatedSystemData\":{},\"selectedContainer\":\"singlebets\",\"potentialWinning\":0,\"amountSystem\":0,\"amountMulti\":100,\"amountSingle\":0,\"totalCombs\":0,\"betOnAll\":200,\"betOnAllEnabled\":false,\"multipleBonus\":null,\"multipleTaxedBonus\":null,\"oddVariation\":\"NEVER\",\"printableSplitAmount\":0,\"printableAmountMulti\":\"1.00\",\"printableBetOnAll\":\"2.00\"}},\"selectedCoupon\":\"1\",\"isMobile\":false,\"saveBetslip\":false,\"isPsv\":false},\"selectedFunbonus\":null,\"selectedFreebet\":null,\"pvrAccountId\":null,\"transactionIdAcceptanceForProposal\":null,\"cf\":null,\"provProfile\":null,\"offsetHours\":0,\"frontendTransactionId\":\"*systemcode*-$000000011540194-*milisec*\",\"liveTransactionId\":*liveTransactionId*,\"liveFatherTransactionId\":*liveFatherTransactionId*}";
                FormData = FormData.Replace("*pal*", pal).Replace("*avv*", avv).Replace("*descEvent*", descEvent);
                FormData = FormData.Replace("*scomSogei*", market_cs).Replace("*handicap*", curHandicap).Replace("*descBet*", descBet).Replace("*descDraw*", descDraw);
                FormData = FormData.Replace("*draw*", market_ce).Replace("*odd*", curOdd).Replace("*amount*", ((int)(info.stake * 100)).ToString()).Replace("*printableAmount*", info.stake.ToString());
                FormData = FormData.Replace("*potentialWinning*", ((int)winMoney).ToString()).Replace("*alias*", alias).Replace("*descSport*", descSport).Replace("*descTournament*", descTournament);
                FormData = FormData.Replace("*sportId*", sportId).Replace("*tournamentId*", tournamentId).Replace("*timestamp*", timestamp).Replace("*amountSingle*", ((int)(info.stake * 100)).ToString());
                FormData = FormData.Replace("*milisec*", Utils.getTick().ToString());
                FormData = FormData.Replace("*systemcode*", SYSTEMCODE);
                FormData = FormData.Replace("*islive*", isLive);
                FormData = FormData.Replace("*liveTransactionId*", transactionId).Replace("*liveFatherTransactionId*", fatherTrancsactionId);

                string purchaseUrl = $"https://www.{Setting.Instance.domain}/XSportDatastore/purchase?systemCode={SYSTEMCODE}&lingua=IT&hash=&token={Global.DomusbetToken}&shopUserData=";
                //string purchaseFormDataString = WebUtility.UrlEncode(FormData);
                string purchaseFormDataString = FormData.Replace("{", "%7B").Replace("}", "%7D").Replace("[", "%5B").Replace("]", "%5D").Replace("\"", "%22").Replace(" ", "%20");

                functionString = $"window.fetch('{purchaseUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json; charset=UTF-8;' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', referer: 'https://www.{Setting.Instance.domain}/scommesse-sportive', origin: 'https://www.{Setting.Instance.domain}', body: '{purchaseFormDataString}', method: 'POST' }}).then(response => response.json());";


                Global.strWebResponse1 = "";
                Global.waitResponseEvent1.Reset();
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Purchase Request: {functionString}");
#endif
                Global.RunScriptCode(functionString);



                if (!Global.waitResponseEvent1.Wait(10000))
                {
                    LogMng.Instance.onWriteStatus($"Purchase No Response");
                    return PROCESS_RESULT.ERROR;
                }
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Purchase Result: {Global.strWebResponse1}");
#endif
                string strPlacebetInfo = Global.strWebResponse1;

                dynamic jsonPlacebetInfo = JsonConvert.DeserializeObject<dynamic>(strPlacebetInfo);

                try
                {
                    int resultCode = Utils.parseToInt(jsonPlacebetInfo.returnCode.code.ToString());
                    if (resultCode == 0)
                    {
                        return PROCESS_RESULT.PLACE_SUCCESS;                                                
                    }
                    else
                    {
                        String reason = jsonPlacebetInfo.returnCode.description.ToString();
                        LogMng.Instance.onWriteStatus(string.Format("Placebet failed ({0})", reason));
                        if (reason.Contains("Login Denied"))
                        {
                            return PROCESS_RESULT.NO_LOGIN;
                        }
                        else if (reason.Contains("superato il numero"))
                        {
                            return PROCESS_RESULT.CRITICAL_SITUATION;
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
                LogMng.Instance.onWriteStatus(string.Format("Placebet exception ({0})", e));
                return PROCESS_RESULT.ERROR;
            }
            return PROCESS_RESULT.ERROR;
        }

        private bool GetNewOdd(BetburgerInfo info, string pal, string avv, string idAggregata, string isLive, string market_cs, string market_ce, string market_handicap, out string curOdd, out string curHandicap)
        {
            curOdd = ""; curHandicap = "";
            string MarketsUrl = $"https://www.{Setting.Instance.domain}/XSportDatastore/getEvento?systemCode={SYSTEMCODE}&lingua=IT&hash=&pal={pal}&avv={avv}&idAggregata={idAggregata}&isLive={isLive}";

            //HttpResponseMessage searchMarketResponse = m_client.GetAsync(MarketsUrl).Result;
            //searchMarketResponse.EnsureSuccessStatusCode();
            //string searchMarketResponseContent = searchMarketResponse.Content.ReadAsStringAsync().Result;

            string functionString = $"window.fetch('{MarketsUrl}', {{ headers: {{ accept: '*/*', 'accept-language': 'es,en-US;q=0.9,en;q=0.8,fr;q=0.7', 'content-type': 'application/json; charset=UTF-8;' }}, mode: 'cors', credentials: 'include', referrerPolicy: 'strict-origin-when-cross-origin', referer: 'https://www.{Setting.Instance.domain}/scommesse-sportive', origin: 'https://www.{Setting.Instance.domain}', method: 'GET' }}).then(response => response.json());";

            Global.strWebResponse2ReqUrl = MarketsUrl;
            Global.strWebResponse2 = "";
            Global.waitResponseEvent2.Reset();
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"GetEvento Request: {functionString}");
#endif
            Global.RunScriptCode(functionString);



            if (!Global.waitResponseEvent2.Wait(10000) || string.IsNullOrEmpty(Global.strWebResponse2))
            {
                LogMng.Instance.onWriteStatus($"GetEvento No Response");
                return false;
            }

            dynamic MarketInfoObj = JsonConvert.DeserializeObject<dynamic>(Global.strWebResponse2);

#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"GetEvento Response: {MarketsUrl}");
            LogMng.Instance.onWriteStatus(Global.strWebResponse2);
#endif

                       
            LogMng.Instance.onWriteStatus($"ParseOutcome: {info.sport} outcome: {info.outcome} market_cs: {market_cs} market_ce: {market_ce} market_handicap: {market_handicap}");
            if (string.IsNullOrEmpty(market_cs) || string.IsNullOrEmpty(market_ce))
            {
                LogMng.Instance.onWriteStatus($"outcome can't be parsed sport: {info.sport} outcome: {info.outcome}");
                return false;
            }
            foreach (dynamic marketObj in MarketInfoObj.scs)
            {
                if (marketObj.cs.ToString() == market_cs)
                {
                    foreach (dynamic participantObj in marketObj.eqs)
                    {
                        if (participantObj.ce.ToString() == market_ce)
                        {
                            if (!string.IsNullOrEmpty(market_handicap) && marketObj.h.ToString() != market_handicap)
                                continue;


                            curOdd = participantObj.q.ToString();
                            curHandicap = marketObj.h.ToString();

                            LogMng.Instance.onWriteStatus($"NewOdd: {curOdd} NewHandicap: {curHandicap}");
                            return true;
                        }
                    }
                }
            }
        
            LogMng.Instance.onWriteStatus($"Can't find target Odd sport: {info.sport} outcome: {info.outcome} market_cs: {market_cs} market_ce: {market_ce} market_handicap: {market_handicap}");
            return false;       
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
