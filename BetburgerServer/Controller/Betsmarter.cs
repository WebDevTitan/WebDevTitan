using BetburgerServer.Constant;
using Newtonsoft.Json.Linq;
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

namespace BetburgerServer.Controller
{
    public class Betsmarter
    {
        private onWriteStatusEvent _onWriteStatus;
        private HttpClient httpClient = null;
        private CookieContainer coockieContainer = null;
        private string jwt = "";
        bool isLoggedIn = false;
        List<BetburgerInfo> saved_infos = new List<BetburgerInfo>();
        public Betsmarter(onWriteStatusEvent onWriteStatus)
        {
            _onWriteStatus = onWriteStatus;
            initHttpClient();
        }

        private void initHttpClient()
        {
            HttpClientHandler handler = new HttpClientHandler();

            coockieContainer = new CookieContainer();
            handler.CookieContainer = coockieContainer;
            httpClient = new HttpClient(handler);

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.51 Safari/537.36");

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"99\", \"Google Chrome\";v=\"99\"");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "document");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "navigate");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "none");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-User", "?1");

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://betsmarter.app");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
        }

        private string getLogTitle()
        {
            return "[BetSmarter]";
        }
        public async Task scrap_thread()
        {
            Task.Run(Scrap);
        }
        public void Scrap()
        {
            isLoggedIn = CDPController.Instance.DoLoginToBetsmarter();
            if (isLoggedIn)
                _onWriteStatus("*** Logged Into Betsmarter ***");
            
            while (GameConstants.bRun)
            {
                try
                {      
                    if(!isLoggedIn)
                        isLoggedIn = CDPController.Instance.DoLoginToBetsmarter();

                    if (!string.IsNullOrEmpty(Global.login_url))
                        CDPController.Instance.NavigateInvoke(Global.login_url);

                    jwt = CDPController.Instance.ExecuteScript("localStorage.jwt", true);
                    if (string.IsNullOrEmpty(jwt))
                        continue;

                    string payload = "{\"channels\":[\"pinnacle-winamax\"],\"minPercentage\":0,\"maxPercentage\":100}";
                    httpClient.DefaultRequestHeaders.Remove("Authorization");
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", jwt);

                    HttpResponseMessage respMessage = httpClient.PostAsync("https://api.betsmarter.app/arbs/query", new StringContent(payload, Encoding.UTF8, "application/json")).Result;
                    respMessage.EnsureSuccessStatusCode();

                    List<BetburgerInfo> infos = new List<BetburgerInfo>();
                    string respBody = respMessage.Content.ReadAsStringAsync().Result;
                    JArray jResp = JArray.Parse(respBody);
                    foreach (var jVar in jResp)
                    {
                        double rawProfit = (double) jVar["rawProfit"];
                        double per = 100 - rawProfit * 100;

                        string event_name = jVar["match"]["name"].ToString();
                        string sport = jVar["match"]["sport"]["name"].ToString();
                        string matchTime = jVar["match"]["time"]["timestamp"].ToString();

                        foreach(var jBet in jVar["bets"])
                        {
                            if (jBet["bookie"].ToString() == "winamax" && jVar["channel"].ToString() == "pinnacle-winamax")
                            {
                                string eventId = jBet["event"]["id"].ToString();
                                string selectionId = jBet["raw"]["id"].ToString();

                                BetburgerInfo info = new BetburgerInfo();
                                info.kind = PickKind.Type_8;

                                info.percent = (decimal) per;
                                info.sport = sport;
                                info.eventTitle = event_name;
                                info.odds = (double)jBet["odds"];
                                info.bookmaker = "winamax";
                                info.outcome = jBet["raw"]["label"].ToString();
                                info.isLive = false;
                                info.started = matchTime;
                                info.direct_link = selectionId;
                                info.eventUrl = $"https://www.winamax.es/apuestas-deportivas/match/{eventId}";

                                BetburgerInfo exist_info = saved_infos.Find(i => i.direct_link == info.direct_link);
                                if(exist_info == null)
                                {
                                    saved_infos.Add(info);
                                    string message = $"Percent : {(per + 1).ToString("N2")} \r\n Sport : {sport} \r\n {event_name} \r\n {info.eventUrl} \r\n {info.outcome} \r\n {info.odds}";
                                    TelegramCtrl.Instance.sendMessage(message);
                                }
                             

                                _onWriteStatus(getLogTitle() + $"Send pick: {info.bookmaker} {info.eventTitle} {info.outcome} {info.odds}");
                                infos.Add(info);
                            }
                        }

                        _onWriteStatus(getLogTitle() + "BetSmarter pick count: " + infos.Count);
                        if (infos.Count > 0)
                        {
                            infos = infos.OrderByDescending(k => k.percent).ToList();
                            GameServer.GetInstance().processValuesInfo(infos);
                        }

                    }

                }
                catch(Exception e)
                {
                    _onWriteStatus("Betsmarter " + e.ToString());
                    isLoggedIn = false;
                }
                Thread.Sleep(5000);
            }


              
        }
    }
}
