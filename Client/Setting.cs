using System;
using System.IO;
using Newtonsoft.Json;

namespace Project
{
    [Serializable]
    public class ScheduleItem
    {
        public DayOfWeek WeekDay;
        public int StartHour;
        public int StartMinute;
        public int EndHour;
        public int EndMinute;

        public ScheduleItem(DayOfWeek _weekday, int _startHour, int _startMinute, int _endHour, int _endMinute)
        {
            WeekDay = _weekday;
            StartHour = _startHour;
            StartMinute = _startMinute;
            EndHour = _endHour;
            EndMinute = _endMinute;
        }
    }
    public class Setting
    {
        string confName = "setting.conf";
        private static Setting _instance = null;

        public static Setting Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Setting();
                }

                return _instance;
            }
        }

        public void saveSetting()
        {
            string jsonSetting = JsonConvert.SerializeObject(_instance);
            File.WriteAllText(confName, jsonSetting);
        }

        public void loadSetting()
        {

            if (!File.Exists(confName))
                return;

            string jsonSetting = File.ReadAllText(confName);
            _instance = JsonConvert.DeserializeObject<Setting>(jsonSetting);

            if (string.IsNullOrEmpty(_instance.username))
                _instance.username = _instance.usernameBet365;
            if (string.IsNullOrEmpty(_instance.password))
                _instance.password = _instance.passwordBet365;
            if (string.IsNullOrEmpty(_instance.domain))
                _instance.domain = _instance.domainBet365;

            if (_instance.percentageSports == 0)
                _instance.percentageSports = 2;
            if (_instance.percentageToSports == 0)
                _instance.percentageToSports = 501;
            if (_instance.minOddsSports == 0)
                _instance.minOddsSports = 1.6;
            if (_instance.maxOddsSports == 0)
                _instance.maxOddsSports = 5;
            if (_instance.stakeSports == 0)
                _instance.stakeSports = 1;
            if (_instance.maxEventCount < 1 || _instance.maxEventCount > 30)
                _instance.maxEventCount = 30;

            if (_instance.percentageHorse == 0)
                _instance.percentageHorse = 0.2;
            if (_instance.percentageToHorse == 0)
                _instance.percentageToHorse = 501;
            if (_instance.minOddsHorse == 0)
                _instance.minOddsHorse = 3;
            if (_instance.maxOddsHorse == 0)
                _instance.maxOddsHorse = 501;
            if (_instance.stakeHorse == 0)
                _instance.stakeHorse = 1;

            if (_instance.minOddsTipster2 == 0)
                _instance.minOddsTipster2 = 1.6;
            if (_instance.maxOddsTipster2 == 0)
                _instance.maxOddsTipster2 = 100;

            if (_instance.minOddsSoccerLive == 0)
                _instance.minOddsSoccerLive = 1.6;
            if (_instance.maxOddsSoccerLive == 0)
                _instance.maxOddsSoccerLive = 3;

#if (UNIBET || STOIXIMAN || BETANO || NOVIBET || SUPERBET)
            if (intervalDelay < 4)
                _instance.intervalDelay = 4;
#endif

            if (lastGetStakeTime == null)
                lastGetStakeTime = DateTime.MinValue;

            //if (ScheduleList == null)
            //    ScheduleList = new List<ScheduleItem>();
            StartHour = 10;
            EndHour = 23;
        }

        public string username;
        public string password;
        public string OTP;
        public string domain;
        public string birthday;

        public double percentageHorse;        //arbitrage percentage
        public double percentageToHorse;        //arbitrage percentage To
        public double minOddsHorse;
        public double maxOddsHorse;
        public double stakeHorse;

        public double percentageSports;        //arbitrage percentage
        public double percentageToSports;        //arbitrage percentage To
        public double minOddsSports;
        public double maxOddsSports;
        public double stakeSports;
        public double stakeSportsTo;
        public bool bStakeSportsTo;

        public bool bStakePercentageMode;

        public int failRetryCount;
        public int maxEventCount;
        public int requestDelay;
        public int intervalDelay;

        public string usernameBet365;
        public string passwordBet365;
        public string domainBet365;
        public double perBet365;        //arbitrage percentage
        public double perBet365To;        //arbitrage percentage

        public double stakeBet365;

        public bool bRangeStake;
        public double range1_start;
        public double range1_end;
        public double range1_stake;
        public double range1_stakeTo;
        public double range2_start;
        public double range2_end;
        public double range2_stake;
        public double range2_stakeTo;
        public double range3_start;
        public double range3_end;
        public double range3_stake;
        public double range3_stakeTo;

        public string ServerIP;
        public ushort ServerPort;
        public string license;

        //public List<ScheduleItem> ScheduleList;

        public int StartHour;
        public int EndHour;

        public string ProxyRegion;
        public string proxy { get; set; }
        public string sessionId { get; internal set; }

        public bool bSoccer;

        public bool bSoccerMoneyline;
        public bool bSoccerHandicap;
        public bool bSoccerTotals;
        public bool bSoccerCorners;
        public bool bSoccerCards;

        public bool bBasketBall;
        public bool bVolleyBall;
        public bool bBaseBall;
        public bool bTennis;
        public bool bTableTenis;
        public bool bHockey;
        public bool bRugby;
        public bool bESoccer;
        public bool bHandball;


        public bool bValue1;        //betburger
        public bool bValue2;        //surebet
        public bool bValue3;        //betspan

        public bool bTipster2;
        public double stakePerTipster2;
        public bool percentageStakeModeTipster2;
        public double minOddsTipster2;
        public double maxOddsTipster2;

        public bool bSoccerLive;
        public double stakeSoccerLive;
        public bool percentageStakeModeSoccerLive;
        public double minOddsSoccerLive;
        public double maxOddsSoccerLive;

        public bool bSportsLessTime;
        public int nSportsLessTimeMinute;

        public bool bHorseRacing;

        public bool bHorseLessTime;
        public int nHorseLessTimeMinute;

        public bool bEachWay;
        public double eachWayOdd;

        public bool bIgnoreUSArace;
        public bool bIgnoreUKrace;
        public bool bIgnoreAUrace;

        public string vmLoginProfile;

        public bool bDailyBetCountLimit;
        public int nDailyBetCountLimit;

        public bool bAllowOddDrop;
        public double dAllowOddDropPercent;

        public bool bAllowOddRise;
        public double dAllowOddRisePercent;

        public bool bAllowMajorLeaguesOnly;

        public bool bPlaceDouleValues;

        public bool bPlaceFastMode;

        public bool bSoccerEnableFirstHalfbets;
        public bool bSoccerEnableSecondHalfbets;

        public bool bEnableMaxbetSuperbet;
        public double MaxStakeLimit;

        public bool bEnableMaxPendingBets;
        public int nMaxPendingBetsLimit;

        public double percentageGetStake;
        public DateTime lastGetStakeTime;

        //Encode/Decode
        public string key { get; set; }
        public string salt { get; set; }

        //3rd Browser
        public int browserType { get; set; } = 0;
        public int profileId { get; set; }
        public string dolphinId { get; set; }
        public string dolphinPass { get; set; }
    }
}
