using BetburgerServer.Constant;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Protocol;
using SeastoryServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BetburgerServer.Controller
{
    public class RebelPickInfo
    {
        public String id;
        public BetburgerInfo pickInfo;        
    }
    public class RebelScraper
    {
        protected onWriteStatusEvent _onWriteStatus;
        private List<RebelPickInfo> curPickList = new List<RebelPickInfo>();

        public RebelScraper(onWriteStatusEvent onWriteStatus)
        {
            _onWriteStatus = onWriteStatus;

            //m_cookieContainer = new CookieContainer();
            //ReadCookiesFromDisk();
            InitHttpClient();
        }

        protected void InitHttpClient()
        {
            
        }

        private bool login()
        {
            Global.OpenUrl("https://vb.rebelbetting.com/login");
            while (GameConstants.bRun)
            {
                try
                {
                    string result = Global.GetStatusValue("return document.getElementById('BetsFooterLink').outerHTML;");
                    if (result.Contains("class"))
                        return true;
                }
                catch { }
                Thread.Sleep(1000);
            }
            return false;
        }

        private void ScrapeProc()
        {
            if (login())
            {
                while (GameConstants.bRun)
                {
                    try
                    {
                        string pageSouirce = Global.RunScriptCode("function getPickIDList() {var collection = document.getElementsByClassName('odds-card'); let i = 0; var result = []; for (i = 0; i < collection.length; i++) { result.push(collection[i].id); } return result; } getPickIDList();");

                        dynamic obj = JsonConvert.DeserializeObject<dynamic>(pageSouirce);

                        bool bIsAdded = false;
                        foreach (string Oddsid in obj)
                        {
                            if (!GameConstants.bRun)
                                break;

                            string id = Oddsid.Replace("OddsID", "");
                            if (string.IsNullOrEmpty(id))
                                continue;
                            bool bIsExist = false;
                            for (int i = 0; i < curPickList.Count; i++)
                            {
                                if (curPickList[i].id == id)
                                {
                                    bIsExist = true;
                                }
                            }
                            if (bIsExist)
                                continue;
                            
                            Global.RunScriptCode($"document.getElementById('{Oddsid}').click();");

                            int nModalWaitRetry = 0;
                            while (nModalWaitRetry++ < 4)
                            {
                                string modalBoxHtml = Global.RunScriptCode($"function getPickInfo(id) {{ var box = document.getElementById('SelectedBetModal'); if (!box.className.includes(id)) return ''; return box.outerHTML; }} getPickInfo('{id}');");
                                if (string.IsNullOrEmpty(modalBoxHtml) || modalBoxHtml == "\"\"")
                                {
                                    Thread.Sleep(500);
                                    continue;
                                }
                                HtmlDocument doc = new HtmlDocument();
                                doc.LoadHtml(modalBoxHtml);
                                string odd = Global.GetStatusValue("return document.getElementById('Odds').value;").Replace("\"", "").Replace("\\t", "").Replace("\\n", "");
                                string value = Global.GetStatusValue("return document.getElementById('Value').innerText;").Replace("\"", "").Replace("\\t", "").Replace("\\n", "");
                                string display = Global.GetStatusValue("return document.getElementById('display').innerText;").Replace("\"", "").Replace("\\t", "").Replace("\\n", "");

                                string participants = Global.GetStatusValue("return document.getElementById('participants').innerText;").Replace("\"", "").Replace("\\t", "").Replace("\\n", "");
                                string oddstype = Global.GetStatusValue("return document.getElementById('oddstype').innerText;").Replace("\"", "").Replace("\\t", "").Replace("\\n", "");
                                string eventname = Global.GetStatusValue("return document.getElementById('eventname').innerText;").Replace("\"", "").Replace("\\t", "").Replace("\\n", "");
                                string sport = Global.GetStatusValue("return document.getElementById('sport').innerText;").Replace("\"", "").Replace("\\t", "").Replace("\\n", "");
                                string start = Global.GetStatusValue("return document.getElementById('start').innerText;").Replace("\"", "").Replace("\\t", "").Replace("\\n", "");
                                string bookmaker = Global.GetStatusValue("return document.getElementById('bookmaker').innerText;").Replace("\"", "").Replace("\\t", "").Replace("\\n", "");
                                string link = Global.GetStatusValue("return document.getElementById('BetOnBookmaker').href;").Replace("\"", "").Replace("\\t", "").Replace("\\n", "");
                                string siteUrl = "";
                                try
                                {
                                    siteUrl = link.Substring(link.IndexOf("#") + 1);
                                }
                                catch { }
                                BetburgerInfo info = new BetburgerInfo();
                                info.arbId = id;
                                info.kind = PickKind.Type_13;
                                info.percent = decimal.Parse(value);
                                info.sport = sport.ToLower();
                                info.league = eventname.ToLower();
                                string[] participant = participants.Split(new string[] { " vs " }, StringSplitOptions.RemoveEmptyEntries);
                                if (participant.Length != 2)
                                {
                                    break;
                                }
                                info.homeTeam = participant[0];
                                info.awayTeam = participant[1];
                                info.bookmaker = bookmaker.ToLower();
                                info.opbookmaker = "Value";
                                info.eventTitle = info.homeTeam + "-" + info.awayTeam;
                                info.odds = double.Parse(odd);

                                
                                info.outcome = getStandardOutcome(display, info.homeTeam, info.awayTeam, info.sport, oddstype);
                                if (string.IsNullOrEmpty(info.outcome))
                                {
                                    _onWriteStatus($"{getLogTitle()} parsing outcome error display: {display} home: {info.homeTeam} away: {info.awayTeam} sport: {info.sport} oddstype: {oddstype}");
                                    break;
                                }

                                ParseBet_Bet365 parsebet = ParseBet_Bet365.ConvertBetburgerPick2ParseBet_365(info, out ParseBet_Bet365 secondaryparsebet);
                                if (parsebet == null)
                                {
                                    _onWriteStatus($"{getLogTitle()} parsing outcome error1 display: {display} home: {info.homeTeam} away: {info.awayTeam} sport: {info.sport} oddstype: {oddstype} info.outcome: {info.outcome}");
                                    break;
                                }


                                info.started = start;
                                info.updated = info.started;
                                info.created = DateTime.Now.ToString();
                                info.siteUrl = siteUrl;

                                RebelPickInfo rebelPickInfo = new RebelPickInfo();
                                rebelPickInfo.pickInfo = info;
                                rebelPickInfo.id = id;

                                curPickList.Add(rebelPickInfo);

                                //Global.RunScriptCode($"document.getElementById('RemoveBet').click();");
                                bIsAdded = true;
                                break;

                            }
                            if (bIsAdded) break;
                        }

                        Global.RunScriptCode($"document.getElementById('CloseSelectedCard').click();");
                        Global.RunScriptCode("document.evaluate(\"//span[contains(text(),'No message from server since')]\", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.click();");

                        if (curPickList.Count > 0)
                        {
                            for (int i = curPickList.Count - 1; i >= 0; i--)
                            {
                                bool bExist = false;
                                foreach (string Oddsid in obj)
                                {
                                    string id = Oddsid.Replace("OddsID", "");
                                    if (id == curPickList[i].id)
                                    {
                                        bExist = true;
                                        break;
                                    }
                                }

                                if (!bExist)
                                {
                                    curPickList.RemoveAt(i);
                                }
                            }
                        }
                        if (curPickList.Count > 0)
                        {
                            List<BetburgerInfo> rebelInfoList = new List<BetburgerInfo>();
                            for (int i = 0; i < curPickList.Count; i++) {
                                rebelInfoList.Add(curPickList[i].pickInfo);                                
                            }
                            

                            _onWriteStatus($"{getLogTitle()} Sending pick: {rebelInfoList.Count}");
                            GameServer.GetInstance().processValuesInfo(rebelInfoList);
                        }
                    }
                    catch (Exception ex)
                    {
                        _onWriteStatus($"RebelScraper Exception: {ex}");
                    }
                    Thread.Sleep(500);
                }
            }
        }

        private string getStandardOutcome(string display, string homeTeam, string awayTeam, string sport, string oddstype)
        {
            string exception = "";
            string result = "";
            try
            {
                MatchCollection mc = Regex.Matches(display, "^AH\\((?<handicap>((-?|\\+?)\\d*\\.{0,1}\\d+))(\\, )?(?<handicap1>((-?|\\+?)\\d*\\.{0,1}\\d+)?)\\) (?<team>([^\\,]*)?)(?<ot>(, overtime included)?)(?<period>((, \\w+ period)|(, \\w+ half)|(, \\w+ set))?)");
                if (mc.Count == 1)
                {
                    Match m = mc[0];
                    string teamNumber = "";
                    string handicap = "";
                    string ot = "";
                    string period = "";

                    if (m.Groups["team"].Value == homeTeam)
                    {
                        teamNumber = "1";
                    }
                    else if (m.Groups["team"].Value == awayTeam)
                    {
                        teamNumber = "2";
                    }
                    else
                    {
                        exception = $"team number exception: {m.Groups["team"].Value}";
                    }

                    if (m.Groups["handicap1"].Value != "")
                    {
                        double dhandicap = Utils.ParseToDouble(m.Groups["handicap"].Value);
                        double dhandicap1 = Utils.ParseToDouble(m.Groups["handicap1"].Value);

                        handicap = ((dhandicap + dhandicap1) / 2).ToString("0.00");
                    }
                    else
                    {
                        handicap = m.Groups["handicap"].Value;
                    }

                    if (m.Groups["ot"].Value != "")
                    {
                        ot = " OT";
                    }

                    if (m.Groups["period"].Value != "")
                    {
                        if (m.Groups["period"].Value.Contains("first half"))
                        {
                            period = " 1st half";
                        }
                        else
                        {
                            exception = $"period param exception: {m.Groups["period"].Value}";
                        }
                    }

                    result = $"AH{teamNumber}({handicap}){ot}{period}";
                }

                mc = Regex.Matches(display, "^(?<side>(O|U))\\((?<handicap>((-?|\\+?)\\d*\\.{0,1}\\d+))\\) (?<team>([^\\,]*)?)(?<ot>(, overtime included)?)");
                if (mc.Count == 1)
                {
                    Match m = mc[0];

                    if (!m.Groups["team"].Value.Contains(homeTeam) || !m.Groups["team"].Value.Contains(awayTeam))
                    {
                        exception = "team name doesn't have homeTeam or awayTeam name";
                    }
                    else
                    {
                        string side = "";
                        string handicap = m.Groups["handicap"].Value;
                        if (m.Groups["side"].Value == "O")
                            side = "Over";
                        else if (m.Groups["side"].Value == "U")
                            side = "Under";
                        string ot = "";
                        if (!string.IsNullOrEmpty(m.Groups["ot"].Value))
                            ot = " OT";

                        result = $"{side} {handicap}{ot}";
                    }
                }

                if (display.StartsWith(homeTeam) || display.StartsWith(awayTeam))
                {
                    string winSide = "";

                    if (display.StartsWith(homeTeam))
                    {
                        winSide = "1";
                    }
                    else if (display.StartsWith(awayTeam))
                    {
                        winSide = "2";
                    }
                                       

                    if (sport == "tennis")
                    {
                        string set = "";
                        if (display.Contains("first set"))
                            set = " 1st set";
                        result = $"{winSide}1-2{set}";
                    }
                    else if (sport == "basketball")
                    {                        
                        result = $"W{winSide}";
                    }
                    else
                    {
                        result = $"{winSide}";
                    }
                }

                if (!string.IsNullOrEmpty(exception))
                {
                    _onWriteStatus($"{getLogTitle()} parsing outcome exception display: {display} home: {homeTeam} away: {awayTeam} sport: {sport} oddstype: {oddstype} description: {exception}");
                    result = "";
                }
            }
            catch (Exception ex){
                _onWriteStatus($"{getLogTitle()} parsing outcome exception1 display: {display} home: {homeTeam} away: {awayTeam} sport: {sport} oddstype: {oddstype} description: {ex}");
                result = "";
            }
            return result;
        }

        public async Task scrape()
        {
            Thread thr = new Thread(ScrapeProc);
            thr.Start();
        }

        private string getLogTitle()
        {
            return "[Rebel]";
        }
    }
}
