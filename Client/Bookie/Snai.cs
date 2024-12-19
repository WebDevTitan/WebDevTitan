namespace Project.Bookie
{
#if (SNAI)
    public class SnaiCtrl : IBookieController
    {
        public HttpClient m_client = null;
        

        public SnaiCtrl()
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

            
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");            
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html, application/xhtml+xml, application/xml; q=0.9, image/webp, */*; q=0.8");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("context", "2:0:en_US");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"90\", \"Google Chrome\";v=\"90\"");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;

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
                    if (!Global.GetStatusValue("return document.getElementsByClassName('btn-primary accept-btn')[0];").Contains("null").ToLower())
                    {
                        //Accept Cookie button clicking
                        Global.RunScriptCode("document.getElementsByClassName('btn-primary accept-btn')[0].click();");
                        Thread.Sleep(1000);
                    }
                    //check if balance label exists
                    string elementBalance = Global.GetStatusValue("return document.getElementById('saldo_user');").ToLower();
                    if (!elementBalance.Contains("null"))
                        return true;
                    

                    Global.RunScriptCode("document.getElementById('accedi-button').click();");
                    Global.RunScriptCode($"document.getElementById('edit-name').value='{Setting.Instance.username}';");
                    Global.RunScriptCode($"document.getElementById('edit-pass').value='{Setting.Instance.password}';");
                    Global.RunScriptCode("document.getElementById('edit-submit--2').click();");

                    int nRetry = 0;
                    while (nRetry < 3)
                    {
                        Thread.Sleep(5000);
                        nRetry++;
                        elementBalance = Global.GetStatusValue("return document.getElementById('saldo_user');").ToLower();
                        if (!elementBalance.Contains("null"))
                        {

                            //closing betslip
                            Global.RunScriptCode("document.evaluate(\"//a[contains(text(),'Cancella Tutti')]\", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.click()");
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
                    result = Utils.ParseToDouble(Global.GetStatusValue("return document.getElementById('saldo_user').getAttribute('data-saldo');")) / 100;
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
            try
            {
                string codSelection = info.direct_link.Split('|')[3].Trim();
                string codMarket = info.direct_link.Split('|')[4].Trim();

                Global.OpenUrl(info.siteUrl);
                Thread.Sleep(2000);
                //var lineArray =[];
                //var linelist = document.getElementsByClassName('table table-bordered table-condensed table-striped table-hover margin-bottom-10 ng-scope');
                //for (var n = 0; n < linelist.length; n++)
                //{
                //    lineArray[n] = linelist[n].innerText;
                //}
                //return JSON.stringify(lineArray);
                string LineList = Global.GetStatusValue("var lineArray=[];var linelist = document.getElementsByClassName('table table-bordered table-condensed table-striped table-hover margin-bottom-10 ng-scope');for (var n = 0; n < linelist.length; n++){lineArray[n] = linelist[n].innerText;} return JSON.stringify(lineArray);");
                LineList = LineList.Substring(1, LineList.Length - 2);
                string[] LineListObj = JsonConvert.DeserializeObject<string[]>(LineList.Replace("\\\\", "\\").Replace("\\\"", "\""));
                Trace.WriteLine($"-----------------------------------------------");
                Trace.WriteLine($"Comparing line and market {info.siteUrl}");
                for (int i = 0; i < LineListObj.Count(); i++)
                {
                    string LineObj = LineListObj[i];
                    string[] LineArray = LineObj.Split(new string[] { "\\\n\\\n\\\n" }, StringSplitOptions.None);
                    if (LineArray.Count() >= 2)
                    {
                        string Linelabel = LineArray[0];
                        Trace.WriteLine($"Comparing line {LineArray[0].Trim().ToLower()} - {codSelection.Trim().ToLower()}");
                        if (LineArray[0].Trim().ToLower() == codSelection.Trim().ToLower())
                        {
                            for (int j = 1; j < LineArray.Count(); j++) //tr
                            {
                                string[] MarketArray = LineArray[j].Split(new string[] { "\\\t" }, StringSplitOptions.None);
                                for (int k = 0; k < MarketArray.Count(); k++)
                                {
                                    string[] MarketFields = MarketArray[k].Split(new string[] { "\\\n" }, StringSplitOptions.None);
                                    MarketFields = MarketFields.Where(val => !String.IsNullOrWhiteSpace(val.Trim())).ToArray();
                                    if (MarketFields[0].Trim().ToLower() == codMarket.Trim().ToLower())
                                    {                                        
                                        double curOdd = Utils.ParseToDouble(MarketFields[1]);

                                        Trace.WriteLine($"***Found line Cur Odd: {curOdd} Orig Odd: {info.odds}");
                                        //if (info.odds == curOdd)
                                        //{
                                        //    string clickItem = $"document.getElementsByClassName('table table-bordered table-condensed table-striped table-hover margin-bottom-10 ng-scope')[{i}].children[1].children[{j-1}].children[{k}].click();";
                                        //    Global.RunScriptCode(clickItem);
                                        //    Thread.Sleep(1000);

                                        //    string checkStakeInput = Global.GetStatusValue($"return document.getElementsByName('importo')[0]").ToLower();
                                            
                                        //    if (!checkStakeInput.Contains("undefined"))
                                        //    {                                                
                                        //        Global.RunScriptCode($"document.getElementsByName('importo')[0].value='{info.stake}';");
                                        //        Thread.Sleep(500);
                                        //        Global.RunScriptCode("document.evaluate(\"//button[contains(text(),'Scommetti')]\", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.click();");

                                        //        Thread.Sleep(2000);
                                        //        return PROCESS_RESULT.PLACE_SUCCESS;
                                        //    }
                                        //    else
                                        //    {
                                        //        Thread.Sleep(60000);
                                        //    }
                                            
                                        //}
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {

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
