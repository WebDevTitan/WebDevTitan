using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bet365LiveAgent.Data.Soccer
{
    [Serializable]
    public class SoccerMatchData
    {
        public string ID { get; set; } = string.Empty;        
        private string C1 { get; set; } = string.Empty;
        private string C2 { get; set; } = string.Empty;
        [JsonIgnore]
        public string C3 { get; set; } = string.Empty;        
        private string T1 { get; set; } = string.Empty;        
        private string T2 { get; set; } = string.Empty;
        [JsonIgnore]
        private string T3 { get; set; } = string.Empty;
        public string EventID
        {
            get
            {
                return $"{C1}{T1}{C2}{T2}";
            }
        }
        public string FixtureID { get; set; } = string.Empty;
        public string Type { get; set; } = "Soccer";
        public string LeagueName { get; set; } = string.Empty;
        public string HomeName { get; set; } = string.Empty;
        public string AwayName { get; set; } = string.Empty;        
        public string FS { get; set; } = string.Empty;        
        public string Info1 { get; set; } = string.Empty;        
        public string Info2 { get; set; } = string.Empty;
        public string MD { get; set; } = string.Empty;
        public string ML { get; set; } = string.Empty;
        public string XT { get; set; } = string.Empty;        
        public string DC { get; set; } = string.Empty;        
        public string TD { get; set; } = string.Empty;        
        public string TT { get; set; } = string.Empty;        
        public string TU { get; set; } = string.Empty;        
        public string TM { get; set; } = string.Empty;        
        public string TS { get; set; } = string.Empty;        
        public string TA { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        [JsonIgnore] 
        
        public string Score { get; set; } = string.Empty;      
        public string HomeScore { get; set; }
        public string AwayScore { get; set; }

        
        public string VC { get; set; } = string.Empty;        
        public string Animation
        {
            get
            {
                string value = VC;
                if (value.IndexOf("_") > -1)
                    value = value.Substring(value.IndexOf("_") + 1);
                switch (value)
                {
                    case "1014":
                        value = "Kick Off";
                        break;
                    case "1015":
                        value = "Half Time";
                        break;
                    case "1016":
                        value = "Second Half";
                        break;
                    case "1017":
                        value = "Full Time";
                        break;
                    case "1026":
                        value = "Injury Time";
                        if (!string.IsNullOrWhiteSpace(TA))
                            value = $"{value} - {TA} Mins";
                        break;
                    case "11000":
                        value = $"{HomeName} - Dangerous Attack";
                        break;
                    case "11001":
                        value = $"{HomeName} - Attack";
                        break;
                    case "11002":
                        value = $"{HomeName} - In Possession";
                        break;
                    case "11003":
                        value = $"{HomeName} - Goal";
                        break;
                    case "11004":
                        value = $"{HomeName} - Corner";
                        break;
                    case "11005":
                        value = $"{HomeName} - Yellow Card";
                        break;
                    case "11006":
                        value = $"{HomeName} - Red Card";
                        break;
                    case "11007":
                        value = $"{HomeName} - Goal Kick";
                        break;
                    case "11008":
                        value = $"{HomeName} - Penalty";
                        break;
                    case "11009":
                        value = $"{HomeName} - Dangerous Free Kick";
                        break;
                    case "11010":
                        value = $"{HomeName} - Free Kick";
                        break;
                    case "11011":
                        value = $"{HomeName} - Shot On Target";
                        break;
                    case "11012":
                        value = $"{HomeName} - Shot Off Target";
                        break;
                    case "11013":
                        value = $"{HomeName} - Substitution";
                        break;
                    case "11024":
                        value = $"{HomeName} - Throw In";
                        break;
                    case "11025":
                        value = $"{HomeName} - Injury";
                        break;
                    case "11234":
                        value = $"{HomeName} - Offside";
                        break;
                    case "21000":
                        value = $"{AwayName} - Dangerous Attack";
                        break;
                    case "21001":
                        value = $"{AwayName} - Attack";
                        break;
                    case "21002":
                        value = $"{AwayName} - In Possession";
                        break;
                    case "21003":
                        value = $"{AwayName} - Goal";
                        break;
                    case "21004":
                        value = $"{AwayName} - Corner";
                        break;
                    case "21005":
                        value = $"{AwayName} - Yellow Card";
                        break;
                    case "21006":
                        value = $"{AwayName} - Red Card";
                        break;
                    case "21007":
                        value = $"{AwayName} - Goal Kick";
                        break;
                    case "21008":
                        value = $"{AwayName} - Penalty";
                        break;
                    case "21009":
                        value = $"{AwayName} - Dangerous Free Kick";
                        break;
                    case "21010":
                        value = $"{AwayName} - Free Kick";
                        break;
                    case "21011":
                        value = $"{AwayName} - Shot On Target";
                        break;
                    case "21012":
                        value = $"{AwayName} - Shot Off Target";
                        break;
                    case "21013":
                        value = $"{AwayName} - Substitution";
                        break;
                    case "21024":
                        value = $"{AwayName} - Throw In";
                        break;
                    case "21025":
                        value = $"{AwayName} - Injury";
                        break;
                    case "21234":
                        value = $"{AwayName} - Offside";
                        break;
                    default:
                        value = string.Empty;
                        break;
                }
                return value;
            }
        }        
        public string XY { get; set; } = string.Empty;        
        public string PG { get; set; } = string.Empty;

        public string VI { get; set; } = string.Empty;        
        public string MS { get; set; } = string.Empty;

        public string Statistics { get; set; }
        public string AsianHandicapOdds { get; set; }
        
        public string GoalLineOdds { get; set; }
        

        public FullTimeMarketData FullTime { get; set; } = new FullTimeMarketData();
        public AsianHandicapMarketData AsianHandicap { get; set; } = new AsianHandicapMarketData();
        public MatchGoalMarketData matchGoal { get; set; } = new MatchGoalMarketData();
        public GoalLineMarketData GoalLine { get; set; } = new GoalLineMarketData();
        
        public SoccerTeamData HomeTeam { get; set; } = new SoccerTeamData();        
        public SoccerTeamData AwayTeam { get; set; } = new SoccerTeamData();

        public List<EventTimeLineData> EventTimeLines { get; set; } = new List<EventTimeLineData>();        
        public List<EventLocationData> EventLocations { get; set; } = new List<EventLocationData>();

        public void Update(JObject jObjData)
        {
            if (jObjData["ID"] != null)
                ID = jObjData["ID"].ToString();
            if (jObjData["C1"] != null)
                C1 = jObjData["C1"].ToString();
            if (jObjData["C2"] != null)
                C2 = jObjData["C2"].ToString();
            if (jObjData["C3"] != null)
                C3 = jObjData["C3"].ToString();
            if (jObjData["T1"] != null)
                T1 = jObjData["T1"].ToString();
            if (jObjData["T2"] != null)
                T2 = jObjData["T2"].ToString();
            if (jObjData["T3"] != null)
                T3 = jObjData["T3"].ToString();
            if (jObjData["FI"] != null)
                FixtureID = jObjData["FI"].ToString();
            if (jObjData["CT"] != null)
                LeagueName = jObjData["CT"].ToString();
            if (jObjData["FS"] != null)
                FS = jObjData["FS"].ToString();
            if (jObjData["S5"] != null)
                Info1 = jObjData["S5"].ToString();
            if (jObjData["S6"] != null)
                Info2 = jObjData["S6"].ToString();
            if (jObjData["MD"] != null)
                MD = jObjData["MD"].ToString();
            if (jObjData["ML"] != null)
                ML = jObjData["ML"].ToString();
            if (jObjData["XT"] != null)
                XT = jObjData["XT"].ToString();
            if (jObjData["DC"] != null)
                DC = jObjData["DC"].ToString();
            if (jObjData["TD"] != null)
                TD = jObjData["TD"].ToString();
            if (jObjData["TT"] != null)
                TT = jObjData["TT"].ToString();
            if (jObjData["TU"] != null)
                TU = jObjData["TU"].ToString();
            if (jObjData["TM"] != null)
                TM = jObjData["TM"].ToString();
            if (jObjData["TS"] != null)
                TS = jObjData["TS"].ToString();
            if (jObjData["TA"] != null)
                TA = jObjData["TA"].ToString();
            if (jObjData["SS"] != null)
                Score = jObjData["SS"].ToString();
            if (jObjData["VC"] != null)
                VC = jObjData["VC"].ToString();
            if (jObjData["XY"] != null)
                XY = jObjData["XY"].ToString();
            if (jObjData["PG"] != null)
                PG = jObjData["PG"].ToString();
            if (jObjData["VI"] != null)
                VI = jObjData["VI"].ToString();
            if (jObjData["MS"] != null)
                MS = jObjData["MS"].ToString();
            if (jObjData["TGData"] != null)
            {
                foreach (var jTGToken in jObjData["TGData"].ToObject<JObject>())
                {
                    JObject jTGData = jTGToken.Value.ToObject<JObject>();
                    if (jTGData["TEData"] != null)
                    {
                        JToken jTkHomeTeam = jTGData["TEData"][$"6V{FixtureID}C1T1_{Global.LANG_ID}_{Global.ZONE}"] ?? jTGData["TEData"][$"ML{FixtureID}C1T1_{Global.LANG_ID}"];
                        if (jTkHomeTeam != null)
                        {
                            HomeTeam.Update(jTkHomeTeam.ToObject<JObject>());
                            HomeName = HomeTeam.Name;
                        }
                        JToken jTkAwayTeam = jTGData["TEData"][$"6V{FixtureID}C1T2_{Global.LANG_ID}_{Global.ZONE}"] ?? jTGData["TEData"][$"ML{FixtureID}C1T2_{Global.LANG_ID}"];
                        if (jTkAwayTeam != null)
                        {
                            AwayTeam.Update(jTkAwayTeam.ToObject<JObject>());
                            AwayName = AwayTeam.Name;
                        }
                    }
                }
            }
            if (jObjData["ESData"] != null)
            {
                foreach (var jESToken in jObjData["ESData"].ToObject<JObject>())
                {
                    JObject jESData = jESToken.Value.ToObject<JObject>();
                    if (jESData["SCData"] != null)
                    {
                        JToken jTkSCData1 = jESData["SCData"][$"6V{FixtureID}C1ES1_{Global.LANG_ID}_{Global.ZONE}"] ?? jESData["SCData"][$"ML{FixtureID}C1ES1_{Global.LANG_ID}"];
                        if (jTkSCData1 != null)
                        {
                            JToken jTkHomeScore = jTkSCData1["SLData"][$"6V{FixtureID}C1ES1-0_{Global.LANG_ID}_{Global.ZONE}"] ?? jTkSCData1["SLData"][$"ML{FixtureID}C1ES1-0_{Global.LANG_ID}"];
                            if (jTkHomeScore != null && jTkHomeScore["D1"] != null)
                                HomeTeam.Score = jTkHomeScore["D1"].ToString();
                            JToken jTkAwayScore = jTkSCData1["SLData"][$"6V{FixtureID}C1ES1-1_{Global.LANG_ID}_{Global.ZONE}"] ?? jTkSCData1["SLData"][$"ML{FixtureID}C1ES1-1_{Global.LANG_ID}"];
                            if (jTkAwayScore != null && jTkAwayScore["D1"] != null)
                                AwayTeam.Score = jTkAwayScore["D1"].ToString();
                        }

                        JToken jTkSCData2 = jESData["SCData"][$"6V{FixtureID}C1ES2_{Global.LANG_ID}_{Global.ZONE}"] ?? jESData["SCData"][$"ML{FixtureID}C1ES2_{Global.LANG_ID}"];
                        if (jTkSCData2 != null)
                        {                            
                            JToken jTkHomeCorner = jTkSCData2["SLData"][$"6V{FixtureID}C1ES2-0_{Global.LANG_ID}_{Global.ZONE}"] ?? jTkSCData2["SLData"][$"ML{FixtureID}C1ES2-0_{Global.LANG_ID}"];
                            if (jTkHomeCorner != null && jTkHomeCorner["D1"] != null)
                                HomeTeam.Corner = jTkHomeCorner["D1"].ToString();
                            JToken jTkAwayCorner = jTkSCData2["SLData"][$"6V{FixtureID}C1ES2-1_{Global.LANG_ID}_{Global.ZONE}"] ?? jTkSCData2["SLData"][$"ML{FixtureID}C1ES2-1_{Global.LANG_ID}"];
                            if (jTkAwayCorner != null && jTkAwayCorner["D1"] != null)
                                AwayTeam.Corner = jTkAwayCorner["D1"].ToString();
                        }

                        JToken jTkSCData3 = jESData["SCData"][$"6V{FixtureID}C1ES3_{Global.LANG_ID}_{Global.ZONE}"] ?? jESData["SCData"][$"ML{FixtureID}C1ES3_{Global.LANG_ID}"];
                        if (jTkSCData3 != null)
                        {
                            JToken jTkHomeYellowCard = jTkSCData3["SLData"][$"6V{FixtureID}C1ES3-0_{Global.LANG_ID}_{Global.ZONE}"] ?? jTkSCData3["SLData"][$"ML{FixtureID}C1ES3-0_{Global.LANG_ID}"];
                            if (jTkHomeYellowCard != null && jTkHomeYellowCard["D1"] != null)
                                HomeTeam.YellowCard = jTkHomeYellowCard["D1"].ToString();
                            JToken jTkAwayYellowCard = jTkSCData3["SLData"][$"6V{FixtureID}C1ES3-1_{Global.LANG_ID}_{Global.ZONE}"] ?? jTkSCData3["SLData"][$"ML{FixtureID}C1ES3-1_{Global.LANG_ID}"];
                            if (jTkAwayYellowCard != null && jTkAwayYellowCard["D1"] != null)
                                AwayTeam.YellowCard = jTkAwayYellowCard["D1"].ToString();
                        }

                        JToken jTkSCData4 = jESData["SCData"][$"6V{FixtureID}C1ES4_{Global.LANG_ID}_{Global.ZONE}"] ?? jESData["SCData"][$"ML{FixtureID}C1ES4_{Global.LANG_ID}"];
                        if (jTkSCData4 != null)
                        {                            
                            JToken jTkHomeRedCard = jTkSCData4["SLData"][$"6V{FixtureID}C1ES4-0_{Global.LANG_ID}_{Global.ZONE}"] ?? jTkSCData4["SLData"][$"ML{FixtureID}C1ES4-0_{Global.LANG_ID}"];
                            if (jTkHomeRedCard != null && jTkHomeRedCard["D1"] != null)
                                HomeTeam.RedCard = jTkHomeRedCard["D1"].ToString();
                            JToken jTkAwayRedCard = jTkSCData4["SLData"][$"6V{FixtureID}C1ES4-1_{Global.LANG_ID}_{Global.ZONE}"] ?? jTkSCData4["SLData"][$"ML{FixtureID}C1ES4-1_{Global.LANG_ID}"];
                            if (jTkAwayRedCard != null && jTkAwayRedCard["D1"] != null)
                                AwayTeam.RedCard = jTkAwayRedCard["D1"].ToString();
                        }

                        JToken jTkSCData8 = jESData["SCData"][$"6V{FixtureID}C1ES8_{Global.LANG_ID}_{Global.ZONE}"] ?? jESData["SCData"][$"ML{FixtureID}C1ES8_{Global.LANG_ID}"];
                        if (jTkSCData8 != null)
                        {                            
                            JToken jTkHomePenalty = jTkSCData8["SLData"][$"6V{FixtureID}C1ES8-0_{Global.LANG_ID}_{Global.ZONE}"] ?? jTkSCData8["SLData"][$"ML{FixtureID}C1ES8-0_{Global.LANG_ID}"];
                            if (jTkHomePenalty != null && jTkHomePenalty["D1"] != null)
                                HomeTeam.Penalty = jTkHomePenalty["D1"].ToString();
                            JToken jTkAwayPenalty = jTkSCData8["SLData"][$"6V{FixtureID}C1ES8-1_{Global.LANG_ID}_{Global.ZONE}"] ?? jTkSCData8["SLData"][$"ML{FixtureID}C1ES8-1_{Global.LANG_ID}"];
                            if (jTkAwayPenalty != null && jTkAwayPenalty["D1"] != null)
                                AwayTeam.Penalty = jTkAwayPenalty["D1"].ToString();
                        }
                    }
                }
            }
            if (jObjData["MGData"] != null)
            {
                foreach (var jMGToken in jObjData["MGData"].ToObject<JObject>())
                {
                    JObject jMGData = jMGToken.Value.ToObject<JObject>();
                    if (jMGData["MAData"] != null)
                    {
                        string strMarkgetID = jMGData["ID"] != null ? jMGData["ID"].ToString() : string.Empty;
                        if ("1777".Equals(strMarkgetID))
                            FullTime.Update(jMGData);

                        if ("12".Equals(strMarkgetID))
                            AsianHandicap.Update(jMGData);

                        if ("15".Equals(strMarkgetID))
                            GoalLine.Update(jMGData);

                        if (strMarkgetID.Equals("421"))
                            matchGoal.Update(jMGData);

                    }
                }
            }
            if (jObjData["SGData"] != null)
            {
                if (jObjData["SGData"][$"ML{FixtureID}C1U1_{Global.LANG_ID}"] != null)
                {
                    if (jObjData["SGData"][$"ML{FixtureID}C1U1_{Global.LANG_ID}"]["STData"] != null)
                    {
                        EventTimeLines.Clear();
                        foreach (var jSTToken in jObjData["SGData"][$"ML{FixtureID}C1U1_{Global.LANG_ID}"]["STData"].ToObject<JObject>())
                        {
                            EventTimeLineData eventTimeLineData = jSTToken.Value.ToObject<EventTimeLineData>();
                            EventTimeLines.Add(eventTimeLineData);
                        }
                    }
                }
                if (jObjData["SGData"][$"ML{FixtureID}C1U100_{Global.LANG_ID}"] != null)
                {
                    if (jObjData["SGData"][$"ML{FixtureID}C1U100_{Global.LANG_ID}"]["STData"] != null)
                    {
                        EventLocations.Clear();
                        foreach (var jSTToken in jObjData["SGData"][$"ML{FixtureID}C1U100_{Global.LANG_ID}"]["STData"].ToObject<JObject>())
                        {
                            EventLocationData eventLocationData = jSTToken.Value.ToObject<EventLocationData>();
                            EventLocations.Add(eventLocationData);
                        }
                    }
                }
            }
        }

        public string CalcMatchTime(string DC, string TU, string TM, string TS, string TT, string TD)
        {
            string defaultClockValue = "00:00";
            bool singleDigitMins = false;
            DateTime savedDateTime;
            if (!DateTime.TryParseExact($"{TU} GMT+0100", "yyyyMMddHHmmss 'GMT'K",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out savedDateTime))
                savedDateTime = DateTime.Now;

            int minsSaved, secsSaved;
            if (!int.TryParse(TM, out minsSaved) || !int.TryParse(TS, out secsSaved))
                return defaultClockValue;

            bool timerRunning = string.IsNullOrWhiteSpace(TU) ? false : "1".Equals(DC) && "1".Equals(TT);
            bool countDown = "1".Equals(TD);

            DateTime serverTime = DateTime.Now;
            TimeSpan e = serverTime - savedDateTime;
            int serverMSpassed = 60000 * minsSaved + 1000 * secsSaved;
            if (serverMSpassed < 0)
                serverMSpassed = -1;

            int t = timerRunning ? countDown ? serverMSpassed - (int)e.TotalMilliseconds : serverMSpassed + (int)e.TotalMilliseconds : serverMSpassed;
            int i = t / 60000 >> 0;
            var n = (int)(0.001 * (t - 60000 * i)) >> 0;
            string r = 10 > i && i > -1 && !singleDigitMins ? "0" + i : 0 > i ? "00" : i.ToString();
            r += ":";
            if (10 > n && n >= 0)
                r += "0";
            else if (0 > n)
                r += "00";
            r += n.ToString();
            if (0 >= t)
                r = defaultClockValue;
            return r;

            /*
            //DateTime dtTU = DateTime.ParseExact($"{TU} GMT+0100", "yyyyMMddHHmmss 'GMT'K", CultureInfo.InvariantCulture);            
            DateTime dtTU = DateTime.ParseExact($"{TU} GMT+0000", "yyyyMMddHHmmss 'GMT'K", CultureInfo.InvariantCulture);
            TimeSpan timeDiff = DateTime.UtcNow.Subtract(dtTU.ToUniversalTime());
            string value = string.Empty;
            //if (timeDiff.TotalMinutes > 0)
            //{
            //    if (TT == "1")
            //    {
                  
            //        value = $"{timeDiff.Minutes}:{timeDiff.Seconds}";
            //    }
            //    else
            //        value = $"{TM}:00";
            //}

            int _minsSaved = 0;
            int _secsSaved = 0;
            _minsSaved = Convert.ToInt32(TM);
            _secsSaved = Convert.ToInt32(TS);

            bool _timerRunning;
            if (DC == "1" && TT == "1")
                _timerRunning = true;
            else
                _timerRunning = false;

            bool _countdown;
            if (TD == "1")
                _countdown = true;
            else
                _countdown = false;

            if (_timerRunning)
            {
                timeDiff = timeDiff.Add(new TimeSpan(0, _minsSaved, _secsSaved));
                value = $"{timeDiff.Minutes + timeDiff.Hours * 60}:{timeDiff.Seconds}";
            }
            else
            {
                value = $"{_minsSaved}:{_secsSaved}";
            }
            return value;*/
        }
    }
}
