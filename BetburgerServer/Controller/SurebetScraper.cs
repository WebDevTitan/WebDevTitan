using BetburgerServer.Constant;
using HtmlAgilityPack;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Protocol;
using SeastoryServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace BetburgerServer.Controller
{
#if !(FORSALE)
    public class SurebetScraper
    {
        private onWriteStatusEvent _onWriteStatus;
        private HttpClient httpClient = null;
        protected WebSocket _webSocket = null;

        
        List<string> AlreadyProcessedIDList = new List<string>();

        public SurebetScraper(onWriteStatusEvent onWriteStatus)
        {
            _onWriteStatus = onWriteStatus;

            ServicePointManager.DefaultConnectionLimit = 2;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        private void initHttpClient()
        {
            HttpClientHandler handler = new HttpClientHandler();
            Global.cookieContainer = new CookieContainer(300, 50, 20480);
            handler.CookieContainer = Global.cookieContainer;
            httpClient = new HttpClient(handler);

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/javascript, */*; q=0.01");
            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.51 Safari/537.36");


            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"99\", \"Google Chrome\";v=\"99\"");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-requested-with", "XMLHttpRequest");
        }

        private bool IsLogined()
        {
            int nretry = 0;
            while (nretry++ < 4)
            {
                string result = Global.GetStatusValue("return document.querySelectorAll('[width=\"23\"]')[0].outerHTML;");

                if (result.Contains("title"))
                {

                    return true;
                }
                Thread.Sleep(500);
            }
            return false;
        }
        private bool login()
        {

            if (string.IsNullOrEmpty(cServerSettings.GetInstance().SurebetUsername) || string.IsNullOrEmpty(cServerSettings.GetInstance().SurebetPassword))
            {
                _onWriteStatus(getLogTitle() + "username/password incorrect!");
                return false;
            }

            initHttpClient();
            
            try
            {            
                Global.OpenUrl("https://en.surebet.com/users/sign_in");

                int nretry = 0;
                while (nretry++ < 20)
                {
                    string result = Global.GetStatusValue("return document.querySelectorAll('[alt=\"logo\"]')[0].outerHTML;");

                    if (result.Contains("img"))
                    {
                        break;
                    }
                    Thread.Sleep(500);
                }
                //wait for until cloudfare is passed



                if (!IsLogined())
                {

                    nretry = 0;
                    while (nretry++ < 3)
                    {
                        string result = Global.GetStatusValue("return document.getElementById('user_email').outerHTML;");

                        if (result.Contains("class"))
                        {
                            break;
                        }
                        Thread.Sleep(500);
                    }
                    if (nretry >= 3)
                        return false;

                    Global.RunScriptCode($"document.getElementById('user_email').value='{cServerSettings.GetInstance().SurebetUsername}';");
                    Global.RunScriptCode($"document.getElementById('user_password').value='{cServerSettings.GetInstance().SurebetPassword}';");
                    Thread.Sleep(500);

                    Global.RunScriptCode("document.getElementById('sign_in_user').click();");
                    Thread.Sleep(500);

                    if (!IsLogined())
                        return false;
                }
                         
                Global.GetCookie($"https://surebet.com");

                return true;
            }
            catch (Exception ex)
            {
                _onWriteStatus(getLogTitle() + $"Login exception {ex}");
            }


            return false;
        }



        public static long getTick()
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            long timestamp = (long)t.TotalMilliseconds;
            return timestamp;
        }

        //private int sendPrematchTelegramMsg(string text)
        //{
        //    //return 0;
        //    if (string.IsNullOrEmpty(text))
        //        return 0;

        //    try
        //    {
        //        Telegram.Bot.Types.Message msg = Global.tgbotClient.SendTextMessageAsync(new ChatId(-1001598027641), text, Telegram.Bot.Types.Enums.ParseMode.Html).Result;//Value Channel
        //        return msg.MessageId;
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return 0;
        //}

        private string ConvertSport(string origSport)
        {
            switch (origSport.ToLower())
            {
                case "football":
                    return "soccer";
                case "ice hockey":
                    return "hockey";
            }
            return origSport.ToLower();
        }
        private void ScrapeProc()
        {
            if (login())
            {
                string signature = "";
                while (GameConstants.bRun)
                {
                    try
                    {
                        List<BetburgerInfo> surebetInfoList = new List<BetburgerInfo>();

                        HttpResponseMessage responseMessage = httpClient.GetAsync(string.Format("https://en.surebet.com/surebets?format=json&signature={0}&_={1}", signature, getTick())).Result;
                        responseMessage.EnsureSuccessStatusCode();

                        string response = responseMessage.Content.ReadAsStringAsync().Result;

                        dynamic jsonresponse = JsonConvert.DeserializeObject<dynamic>(response);
                        string has_data = jsonresponse.has_data.ToString().ToLower();
                        if (has_data == "false")
                        {
                            Thread.Sleep(1500);
                            continue;
                        }
                        signature = jsonresponse.signature.ToString();

                        
                        foreach (dynamic pickItr in jsonresponse.table)
                        {
                            try
                            {
                                HtmlDocument doc = new HtmlDocument();
                                //HtmlNode.ElementsFlags.Remove("form");
                                doc.LoadHtml(pickItr.html.ToString());

                                IEnumerable<HtmlNode> nodeForms = doc.DocumentNode.Descendants("tbody");
                                if (nodeForms.LongCount() > 0)
                                {
                                    HtmlNode tbodyNode = nodeForms.ToList().ElementAt(0);
                                    string age = tbodyNode.SelectSingleNode(".//span[@class='age']").InnerText;

                                    List<HtmlNode> trNodes = tbodyNode.Descendants("tr").ToList();

                                    _onWriteStatus(getLogTitle() + $"tr node count: {trNodes.Count()}");

                                    foreach (HtmlNode trNode in trNodes)
                                    {
                                        if (trNode.SelectSingleNode(".//td[@class='booker']//a") == null)
                                            continue;

                                        BetburgerInfo info = new BetburgerInfo();
                                        info.percent = pickItr.profit;
                                        info.bookmaker = trNode.SelectSingleNode(".//td[@class='booker']//a").InnerText;
                                        info.sport = ConvertSport(trNode.SelectSingleNode(".//td[@class='booker']//span").InnerText);
                                        info.started = trNode.SelectSingleNode(".//td[@class='time']").Attributes["data-utc"].Value;
                                        info.eventTitle = trNode.SelectSingleNode(".//td[contains(@class,'event')]//a").InnerText;
                                        string[] teams = info.eventTitle.Split('–');
                                        if (teams.Count() == 2)
                                        {
                                            info.homeTeam = teams[0].Trim();
                                            info.awayTeam = teams[1].Trim();
                                        }

                                        info.league = trNode.SelectSingleNode(".//td[contains(@class,'event')]//span").InnerText;
                                        info.extra = trNode.SelectSingleNode(".//td[@class='coeff']//abbr").Attributes["title"].Value;
                                        info.outcome = trNode.SelectSingleNode(".//td[@class='coeff']//abbr").InnerText.Replace("−", "-");
                                        
                                        info.odds = Utils.ParseToDouble(trNode.SelectSingleNode(".//td[contains(@class,'value')]//a").InnerText);
                                        info.kind = PickKind.Type_9;
                                        info.arbId = pickItr.id;

                                        //if (info.bookmaker.ToLower() == "bet365")
                                        //{
                                        //    string betsapiparseLogResult = "";
                                        //    BetsapiHelper.Instance.UpdateBet365SiteUrl("", ref info, out betsapiparseLogResult);

                                        //    if (!string.IsNullOrEmpty(betsapiparseLogResult))
                                        //    {
                                        //        _onWriteStatus($"surebet betsapi parse error: {betsapiparseLogResult}");
                                        //    }
                                        //}
                                        //if (pickItr.profit >= 2 && (info.odds >= 1.2 && info.odds <= 5))
                                        //{
                                        //    if (info.bookmaker.ToLower() == "bet365")
                                        //    {

                                        //        if (!AlreadyProcessedIDList.Contains(info.arbId))
                                        //        {
                                        //            _onWriteStatus(getLogTitle() + $"Send pick in Tg: {info.bookmaker} {info.league} {info.eventTitle} {info.extra} {info.odds}");
                                        //            AlreadyProcessedIDList.Add(info.arbId);
                                        //            string message = $"Prematch_2 {info.bookmaker} {info.sport} ({pickItr.profit}%)" + Environment.NewLine + $"{info.league}" + Environment.NewLine + $"{info.eventTitle}" + Environment.NewLine + $"{info.extra}" + Environment.NewLine + "Odd: " + info.odds;
                                        //            //Task.Run(() => sendPrematchTelegramMsg(message));
                                        //        }
                                        //    }
                                        //}
                                        if (info.bookmaker.ToLower() == "bet365")
                                            surebetInfoList.Add(info);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        if (surebetInfoList.Count > 0)
                            GameServer.GetInstance().processValuesInfo(surebetInfoList);                        
                    }
                    catch (Exception ex)
                    {
                        _onWriteStatus(getLogTitle() + $"Exception: {ex}");
                    }

                    Thread.Sleep(1500);
                }
            }
        }
        public async Task scrape()
        {
            Thread thr = new Thread(ScrapeProc);
            thr.Start();
        }

        
        private string getLogTitle()
        {
            return "[Surebet]";
        }
    }
#endif
}
