using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Project.Helphers;
using Project.Interfaces;
using Project.Models;

namespace Project.ViewModels
{
    public class SettingViewModel : BindableBase, ISettingViewModel, INotifyPropertyChanged
    {

        public SettingViewModel(IEventAggregator eventAggregator)
        {
            this.SaveSettingCommand = new DelegateCommand(OnSaveSettingCommand);

            Setting.Instance.loadSetting();

            UserId = Setting.Instance.username;
            UserPwd = Setting.Instance.password;
            UserOtp = Setting.Instance.OTP;
            Domain = Setting.Instance.domain;
            Birthday = Setting.Instance.birthday;
            RequestDelay = Setting.Instance.requestDelay;
            IntervalDelay = Setting.Instance.intervalDelay;
            PercentageHorse = Setting.Instance.percentageHorse;
            PercentageToHorse = Setting.Instance.percentageToHorse;
            MinOddsHorse = Setting.Instance.minOddsHorse;
            MaxOddsHorse = Setting.Instance.maxOddsHorse;
            StakeHorse = Setting.Instance.stakeHorse;
            PercentageSports = Setting.Instance.percentageSports;
            PercentageToSports = Setting.Instance.percentageToSports;
            MinOddsSports = Setting.Instance.minOddsSports;
            MaxOddsSports = Setting.Instance.maxOddsSports;
            MaxEventCount = Setting.Instance.maxEventCount;
            FailRetryCount = Setting.Instance.failRetryCount;
            StakeSports = Setting.Instance.stakeSports;
            StakeSportsTo = Setting.Instance.stakeSportsTo;
            EnableStakeSportsTo = Setting.Instance.bStakeSportsTo;
            PercentageStakeMode = Setting.Instance.bStakePercentageMode;
            EnableRangeStake = Setting.Instance.bRangeStake;
            Range1_Start = Setting.Instance.range1_start;
            Range1_End = Setting.Instance.range1_end;
            Range1_Stake = Setting.Instance.range1_stake;
            Range1_StakeTo = Setting.Instance.range1_stakeTo;
            Range2_Start = Setting.Instance.range2_start;
            Range2_End = Setting.Instance.range2_end;
            Range2_Stake = Setting.Instance.range2_stake;
            Range2_StakeTo = Setting.Instance.range2_stakeTo;
            Range3_Start = Setting.Instance.range3_start;
            Range3_End = Setting.Instance.range3_end;
            Range3_Stake = Setting.Instance.range3_stake;
            Range3_StakeTo = Setting.Instance.range3_stakeTo;
            EnableSoccer = Setting.Instance.bSoccer;
            EnableSoccerMoneyline = Setting.Instance.bSoccerMoneyline;
            EnableSoccerHandicap = Setting.Instance.bSoccerHandicap;
            EnableSoccerTotals = Setting.Instance.bSoccerTotals;
            EnableSoccerCorners = Setting.Instance.bSoccerCorners;
            EnableSoccerCards = Setting.Instance.bSoccerCards;
            EnableBasketBall = Setting.Instance.bBasketBall;
            EnableVolleyBall = Setting.Instance.bVolleyBall;
            EnableBaseBall = Setting.Instance.bBaseBall;
            EnableTennis = Setting.Instance.bTennis;
            EnableTableTenis = Setting.Instance.bTableTenis;
            EnableHockey = Setting.Instance.bHockey;
            EnableRugby = Setting.Instance.bRugby;
            EnableESoccer = Setting.Instance.bESoccer;
            EnableHandball = Setting.Instance.bHandball;
            EnableHorseRacing = Setting.Instance.bHorseRacing;
            EnableHorseLessTime = Setting.Instance.bHorseLessTime;
            HorseLessTimeMinute = Setting.Instance.nHorseLessTimeMinute;
            EnableSportsLessTime = Setting.Instance.bSportsLessTime;
            SportsLessTimeMinute = Setting.Instance.nSportsLessTimeMinute;
            EnableEachWay = Setting.Instance.bEachWay;
            EachWayOdd = Setting.Instance.eachWayOdd;
            IgnoreUSARace = Setting.Instance.bIgnoreUSArace;
            IgnoreUKRace = Setting.Instance.bIgnoreUKrace;
            IgnoreAURace = Setting.Instance.bIgnoreAUrace;


            EnableValue1 = Setting.Instance.bValue1;
            EnableValue2 = Setting.Instance.bValue2;
            EnableValue3 = Setting.Instance.bValue3;

            EnableTipster2 = Setting.Instance.bTipster2;
            StakePerTipster2 = Setting.Instance.stakePerTipster2;
            PercentageStakeModeTipster2 = Setting.Instance.percentageStakeModeTipster2;
            MinOddsTipster2 = Setting.Instance.minOddsTipster2;
            MaxOddsTipster2 = Setting.Instance.maxOddsTipster2;


            EnableSoccerLive = Setting.Instance.bSoccerLive;
            StakeSoccerLive = Setting.Instance.stakeSoccerLive;
            PercentageStakeModeSoccerLive = Setting.Instance.percentageStakeModeSoccerLive;
            MinOddsSoccerLive = Setting.Instance.minOddsSoccerLive;
            MaxOddsSoccerLive = Setting.Instance.maxOddsSoccerLive;

            SeverIp = Setting.Instance.ServerIP;
            SeverPort = Setting.Instance.ServerPort;
            LicenseKey = Setting.Instance.license;

            StartHour = Setting.Instance.StartHour;
            EndHour = Setting.Instance.EndHour;


            //ScheduleList = new ObservableCollection<ScheduleItem>();
            //if (Setting.Instance != null && Setting.Instance.ScheduleList != null && Setting.Instance.ScheduleList.Count > 0)
            //{
            //    foreach (var item in Setting.Instance.ScheduleList)
            //        ScheduleList.Add(item);
            //}


            ProxyRegion = Setting.Instance.ProxyRegion;

            VMLoginProfile = Setting.Instance.vmLoginProfile;

            EnableDailyBetCountLimit = Setting.Instance.bDailyBetCountLimit;
            DailyBetCountLimit = Setting.Instance.nDailyBetCountLimit;

            EnableAllowOddDrop = Setting.Instance.bAllowOddDrop;
            AllowOddDropPercent = Setting.Instance.dAllowOddDropPercent;

            EnableAllowOddRise = Setting.Instance.bAllowOddRise;
            AllowOddRisePercent = Setting.Instance.dAllowOddRisePercent;

            EnableMajorLeaguesOnly = Setting.Instance.bAllowMajorLeaguesOnly;

            PlaceDoubleValues = Setting.Instance.bPlaceDouleValues;

            PlaceFastMode = Setting.Instance.bPlaceFastMode;

            SoccerEnableFirstHalfbets = Setting.Instance.bSoccerEnableFirstHalfbets;
            SoccerEnableSecondHalfbets = Setting.Instance.bSoccerEnableSecondHalfbets;

            EnableMaxPendingBets = Setting.Instance.bEnableMaxPendingBets;
            MaxPendingBetsLimit = Setting.Instance.nMaxPendingBetsLimit;

            MaxbetEnableSuperbet = Setting.Instance.bEnableMaxbetSuperbet;
            MaxSuperbetlimit = Setting.Instance.MaxStakeLimit;

            SelectedBrowser = Setting.Instance.browserType;
            ProfileId = Setting.Instance.profileId;

            DolphinId = Setting.Instance.dolphinId;

            DayOfWeekList = new ObservableCollection<DayOfWeek>();
            for (int i = 0; i < 7; i++)
                DayOfWeekList.Add((DayOfWeek)i);

            HourList = new ObservableCollection<int>();
            HourList1 = new ObservableCollection<int>();
            for (int i = 0; i < 24; i++)
            {
                HourList.Add(i);
                HourList1.Add(i);
            }

            MinuteList = new ObservableCollection<int>();
            for (int i = 0; i < 60; i++)
            {
                MinuteList.Add(i);
            }

            RegionList = new ObservableCollection<string>();
            RegionList.Add("BD");
            RegionList.Add("ES");
            RegionList.Add("GB");
            RegionList.Add("PE");
            RegionList.Add("PY");
            RegionList.Add("RA");
            RegionList.Add("GR");
            RegionList.Add("CL");

            OddRangeMarketList = new ObservableCollection<string>();

            foreach (string key in Constants.OddRangeMarketDictionary.Keys)
            {
                OddRangeMarketList.Add(key);
            }

        }


        public void OddRangeMarketCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public ICommand SaveSettingCommand { get; private set; }


        private void OnSaveSettingCommand()
        {
            if (FailRetryCount < 0)
            {
                MessageBox.Show($"Please input correct Fail bet Retry Count Number!");
            }
            if (MaxEventCount > 30 || MaxEventCount < 1)
            {
                MessageBox.Show($"Please input correct Max bets in Event Number!");
            }

            if (PercentageStakeMode)
            {
                if (0.1 > StakeSports || 5 < StakeSports)
                {
                    MessageBox.Show("Percentage Stake mode, you have to set 0.1-5 for stake");
                }
            }

            if (EnableSoccerLive)
            {
                if (PercentageStakeModeSoccerLive)
                {
                    if (0.1 > StakeSoccerLive || 7 < StakeSoccerLive)
                    {
                        MessageBox.Show("Soccer Live Percentage Stake mode, you have to set 0.1-7 for stake");
                    }
                }
                else
                {
                    if (StakeSoccerLive <= 0)
                    {
                        MessageBox.Show("Soccer Live stake is incorrect");
                    }
                }
            }
            if (0 < Constants.PackageID && Constants.PackageID < 6)
            {
                double limitStake = 999999;
                switch (Constants.PackageID)
                {
                    case 2:
                        limitStake = 20;
                        break;
                    case 3:
                        limitStake = 5;
                        break;
                    case 4:
                        limitStake = 3;
                        break;
                    case 5:
                        limitStake = 1.5;
                        break;
                }

                bool bNeedShowMessage = false;
                if (StakeSports > limitStake)
                {
                    bNeedShowMessage = true;
                    StakeSports = limitStake;
                }

                if (StakeHorse > limitStake)
                {
                    bNeedShowMessage = true;
                    StakeHorse = limitStake;
                }
                if (bNeedShowMessage)
                    MessageBox.Show($"With your package, you can't set stake higher than {limitStake}");
            }

            Setting.Instance.username = UserId;
            Setting.Instance.password = UserPwd;
            Setting.Instance.OTP = UserOtp;
            Setting.Instance.domain = Domain;
            Setting.Instance.birthday = Birthday;
            Setting.Instance.percentageSports = PercentageSports;
            Setting.Instance.percentageToSports = PercentageToSports;
            Setting.Instance.minOddsSports = MinOddsSports;
            Setting.Instance.maxOddsSports = MaxOddsSports;
            Setting.Instance.failRetryCount = FailRetryCount;
            Setting.Instance.maxEventCount = MaxEventCount;
            Setting.Instance.stakeSports = StakeSports;
            Setting.Instance.stakeSportsTo = StakeSportsTo;
            Setting.Instance.bStakeSportsTo = EnableStakeSportsTo;
            Setting.Instance.bStakePercentageMode = PercentageStakeMode;
            Setting.Instance.percentageHorse = PercentageHorse;
            Setting.Instance.percentageToHorse = PercentageToHorse;
            Setting.Instance.minOddsHorse = MinOddsHorse;
            Setting.Instance.maxOddsHorse = MaxOddsHorse;
            Setting.Instance.stakeHorse = StakeHorse;
            Setting.Instance.requestDelay = RequestDelay;
            Setting.Instance.intervalDelay = IntervalDelay;
            Setting.Instance.bRangeStake = EnableRangeStake;
            Setting.Instance.range1_start = Range1_Start;
            Setting.Instance.range1_end = Range1_End;
            Setting.Instance.range1_stake = Range1_Stake;
            Setting.Instance.range1_stakeTo = Range1_StakeTo;
            Setting.Instance.range2_start = Range2_Start;
            Setting.Instance.range2_end = Range2_End;
            Setting.Instance.range2_stake = Range2_Stake;
            Setting.Instance.range2_stakeTo = Range2_StakeTo;
            Setting.Instance.range3_start = Range3_Start;
            Setting.Instance.range3_end = Range3_End;
            Setting.Instance.range3_stake = Range3_Stake;
            Setting.Instance.range3_stakeTo = Range3_StakeTo;
            Setting.Instance.ServerIP = SeverIp;
            Setting.Instance.ServerPort = SeverPort;

            Setting.Instance.bSoccer = EnableSoccer;
            Setting.Instance.bSoccerMoneyline = EnableSoccerMoneyline;
            Setting.Instance.bSoccerHandicap = EnableSoccerHandicap;
            Setting.Instance.bSoccerTotals = EnableSoccerTotals;
            Setting.Instance.bSoccerCorners = EnableSoccerCorners;
            Setting.Instance.bSoccerCards = EnableSoccerCards;
            Setting.Instance.bBasketBall = EnableBasketBall;
            Setting.Instance.bVolleyBall = EnableVolleyBall;
            Setting.Instance.bBaseBall = EnableBaseBall;
            Setting.Instance.bTennis = EnableTennis;
            Setting.Instance.bTableTenis = EnableTableTenis;
            Setting.Instance.bHockey = EnableHockey;
            Setting.Instance.bRugby = EnableRugby;
            Setting.Instance.bESoccer = EnableESoccer;
            Setting.Instance.bHandball = EnableHandball;
            Setting.Instance.bHorseRacing = EnableHorseRacing;
            Setting.Instance.bHorseLessTime = EnableHorseLessTime;
            Setting.Instance.nHorseLessTimeMinute = HorseLessTimeMinute;
            Setting.Instance.bSportsLessTime = EnableSportsLessTime;
            Setting.Instance.nSportsLessTimeMinute = SportsLessTimeMinute;
            Setting.Instance.bEachWay = EnableEachWay;
            Setting.Instance.eachWayOdd = EachWayOdd;
            Setting.Instance.license = LicenseKey;
            Setting.Instance.bIgnoreUSArace = IgnoreUSARace;
            Setting.Instance.bIgnoreUKrace = IgnoreUKRace;
            Setting.Instance.bIgnoreAUrace = IgnoreAURace;


            Setting.Instance.bValue1 = EnableValue1;
            Setting.Instance.bValue2 = EnableValue2;
            Setting.Instance.bValue3 = EnableValue3;

            Setting.Instance.bTipster2 = EnableTipster2;
            Setting.Instance.stakePerTipster2 = StakePerTipster2;
            Setting.Instance.percentageStakeModeTipster2 = PercentageStakeModeTipster2;
            Setting.Instance.minOddsTipster2 = MinOddsTipster2;
            Setting.Instance.maxOddsTipster2 = MaxOddsTipster2;

            Setting.Instance.bSoccerLive = EnableSoccerLive;
            Setting.Instance.stakeSoccerLive = StakeSoccerLive;
            Setting.Instance.percentageStakeModeSoccerLive = PercentageStakeModeSoccerLive;
            Setting.Instance.minOddsSoccerLive = MinOddsSoccerLive;
            Setting.Instance.maxOddsSoccerLive = MaxOddsSoccerLive;

            //Setting.Instance.ScheduleList = ScheduleList.ToList();

            Setting.Instance.StartHour = StartHour;
            Setting.Instance.EndHour = EndHour;

            Setting.Instance.ProxyRegion = ProxyRegion;
            Setting.Instance.vmLoginProfile = VMLoginProfile;

            Setting.Instance.bDailyBetCountLimit = EnableDailyBetCountLimit;
            Setting.Instance.nDailyBetCountLimit = DailyBetCountLimit;

            Setting.Instance.bAllowOddDrop = EnableAllowOddDrop;
            Setting.Instance.dAllowOddDropPercent = AllowOddDropPercent;

            Setting.Instance.bAllowOddRise = EnableAllowOddRise;
            Setting.Instance.dAllowOddRisePercent = AllowOddRisePercent;

            Setting.Instance.bAllowMajorLeaguesOnly = EnableMajorLeaguesOnly;

            Setting.Instance.bPlaceDouleValues = PlaceDoubleValues;

            Setting.Instance.bPlaceFastMode = PlaceFastMode;

            Setting.Instance.bSoccerEnableFirstHalfbets = SoccerEnableFirstHalfbets;
            Setting.Instance.bSoccerEnableSecondHalfbets = SoccerEnableSecondHalfbets;

            Setting.Instance.bEnableMaxbetSuperbet = MaxbetEnableSuperbet;
            Setting.Instance.MaxStakeLimit = MaxSuperbetlimit;

            Setting.Instance.bEnableMaxPendingBets = EnableMaxPendingBets;
            Setting.Instance.nMaxPendingBetsLimit = MaxPendingBetsLimit;

            Setting.Instance.browserType = SelectedBrowser;
            Setting.Instance.profileId = ProfileId;

            Setting.Instance.dolphinId = DolphinId;


            if (Setting.Instance.bStakeSportsTo)
            {
                if (Setting.Instance.stakeSportsTo < Setting.Instance.stakeSports)
                    Setting.Instance.stakeSportsTo = Setting.Instance.stakeSports;
            }

            if (Setting.Instance.bRangeStake)
            {
                if (Setting.Instance.range1_end < Setting.Instance.range1_start)
                    Setting.Instance.range1_end = Setting.Instance.range1_start;

                if (Setting.Instance.range1_stakeTo < Setting.Instance.range1_stake)
                    Setting.Instance.range1_stakeTo = Setting.Instance.range1_stake;

                if (Setting.Instance.range2_end < Setting.Instance.range2_start)
                    Setting.Instance.range2_end = Setting.Instance.range2_start;

                if (Setting.Instance.range2_stakeTo < Setting.Instance.range2_stake)
                    Setting.Instance.range2_stakeTo = Setting.Instance.range2_stake;

                if (Setting.Instance.range3_end < Setting.Instance.range3_start)
                    Setting.Instance.range3_end = Setting.Instance.range3_start;

                if (Setting.Instance.range3_stakeTo < Setting.Instance.range3_stake)
                    Setting.Instance.range3_stakeTo = Setting.Instance.range3_stake;
            }

            Setting.Instance.saveSetting();
        }

        #region Properties                
        private string _UserId;
        public string UserId
        {
            get { return _UserId; }
            set { SetProperty(ref _UserId, value); }
        }

        private string _UserPwd;
        public string UserPwd
        {
            get { return _UserPwd; }
            set { SetProperty(ref _UserPwd, value); }
        }

        private string _domain;
        public string Domain
        {
            get { return _domain; }
            set { SetProperty(ref _domain, value); }
        }
        private string _UserOtp;
        public string UserOtp
        {
            get { return _UserOtp; }
            set { SetProperty(ref _UserOtp, value); }
        }

        private string _birthday;
        public string Birthday
        {
            get { return _birthday; }
            set { SetProperty(ref _birthday, value); }
        }

        private int _requestDelay;
        public int RequestDelay
        {
            get { return _requestDelay; }
            set { SetProperty(ref _requestDelay, value); }
        }

        private int _intervalDelay;
        public int IntervalDelay
        {
            get { return _intervalDelay; }
            set { SetProperty(ref _intervalDelay, value); }
        }

        private double _percentageHorse;
        public double PercentageHorse
        {
            get { return _percentageHorse; }
            set { SetProperty(ref _percentageHorse, value); }
        }

        private double _percentageToHorse;
        public double PercentageToHorse
        {
            get { return _percentageToHorse; }
            set { SetProperty(ref _percentageToHorse, value); }
        }

        private double _minOddsHorse;
        public double MinOddsHorse
        {
            get { return _minOddsHorse; }
            set { SetProperty(ref _minOddsHorse, value); }
        }

        private double _maxOddsHorse;
        public double MaxOddsHorse
        {
            get { return _maxOddsHorse; }
            set { SetProperty(ref _maxOddsHorse, value); }
        }

        private double _stakeHorse;
        public double StakeHorse
        {
            get { return _stakeHorse; }
            set { SetProperty(ref _stakeHorse, value); }
        }

        private double _percentageSports;
        public double PercentageSports
        {
            get { return _percentageSports; }
            set { SetProperty(ref _percentageSports, value); }
        }

        private double _percentageToSports;
        public double PercentageToSports
        {
            get { return _percentageToSports; }
            set { SetProperty(ref _percentageToSports, value); }
        }

        private double _minOddsSports;
        public double MinOddsSports
        {
            get { return _minOddsSports; }
            set { SetProperty(ref _minOddsSports, value); }
        }

        private double _maxOddsSports;
        public double MaxOddsSports
        {
            get { return _maxOddsSports; }
            set { SetProperty(ref _maxOddsSports, value); }
        }

        private int _failRetryCount;
        public int FailRetryCount
        {
            get { return _failRetryCount; }
            set { SetProperty(ref _failRetryCount, value); }
        }

        private int _maxEventCount;
        public int MaxEventCount
        {
            get { return _maxEventCount; }
            set { SetProperty(ref _maxEventCount, value); }
        }

        private double _stakeSports;
        public double StakeSports
        {
            get { return _stakeSports; }
            set { SetProperty(ref _stakeSports, value); }
        }

        private double _stakeSportsTo;
        public double StakeSportsTo
        {
            get { return _stakeSportsTo; }
            set { SetProperty(ref _stakeSportsTo, value); }
        }

        private bool _enableStakeSportsTo;
        public bool EnableStakeSportsTo
        {
            get { return _enableStakeSportsTo; }
            set { SetProperty(ref _enableStakeSportsTo, value); }
        }

        private bool _percentageStakeMode;
        public bool PercentageStakeMode
        {
            get { return _percentageStakeMode; }
            set { SetProperty(ref _percentageStakeMode, value); }
        }

        private bool _enableRangeStake;
        public bool EnableRangeStake
        {
            get { return _enableRangeStake; }
            set { SetProperty(ref _enableRangeStake, value); }
        }

        private double _range1_Start;
        public double Range1_Start
        {
            get { return _range1_Start; }
            set { SetProperty(ref _range1_Start, value); }
        }

        private double _range1_End;
        public double Range1_End
        {
            get { return _range1_End; }
            set { SetProperty(ref _range1_End, value); }
        }

        private double _range1_Stake;
        public double Range1_Stake
        {
            get { return _range1_Stake; }
            set { SetProperty(ref _range1_Stake, value); }
        }

        private double _range1_StakeTo;
        public double Range1_StakeTo
        {
            get { return _range1_StakeTo; }
            set { SetProperty(ref _range1_StakeTo, value); }
        }

        private double _range2_Start;
        public double Range2_Start
        {
            get { return _range2_Start; }
            set { SetProperty(ref _range2_Start, value); }
        }

        private double _range2_End;
        public double Range2_End
        {
            get { return _range2_End; }
            set { SetProperty(ref _range2_End, value); }
        }

        private double _range2_Stake;
        public double Range2_Stake
        {
            get { return _range2_Stake; }
            set { SetProperty(ref _range2_Stake, value); }
        }

        private double _range2_StakeTo;
        public double Range2_StakeTo
        {
            get { return _range2_StakeTo; }
            set { SetProperty(ref _range2_StakeTo, value); }
        }

        private double _range3_Start;
        public double Range3_Start
        {
            get { return _range3_Start; }
            set { SetProperty(ref _range3_Start, value); }
        }

        private double _range3_End;
        public double Range3_End
        {
            get { return _range3_End; }
            set { SetProperty(ref _range3_End, value); }
        }

        private double _range3_Stake;
        public double Range3_Stake
        {
            get { return _range3_Stake; }
            set { SetProperty(ref _range3_Stake, value); }
        }

        private double _range3_StakeTo;
        public double Range3_StakeTo
        {
            get { return _range3_StakeTo; }
            set { SetProperty(ref _range3_StakeTo, value); }
        }

        private string _serverIp;
        public string SeverIp
        {
            get { return _serverIp; }
            set { SetProperty(ref _serverIp, value); }
        }

        private ushort _serverPort;
        public ushort SeverPort
        {
            get { return _serverPort; }
            set { SetProperty(ref _serverPort, value); }
        }

        private bool _enableSoccer;
        public bool EnableSoccer
        {
            get { return _enableSoccer; }
            set { SetProperty(ref _enableSoccer, value); }
        }

        private bool _enableSoccerMoneyline;
        public bool EnableSoccerMoneyline
        {
            get { return _enableSoccerMoneyline; }
            set { SetProperty(ref _enableSoccerMoneyline, value); }
        }

        private bool _enableSoccerHandicap;
        public bool EnableSoccerHandicap
        {
            get { return _enableSoccerHandicap; }
            set { SetProperty(ref _enableSoccerHandicap, value); }
        }

        private bool _enableSoccerTotals;
        public bool EnableSoccerTotals
        {
            get { return _enableSoccerTotals; }
            set { SetProperty(ref _enableSoccerTotals, value); }
        }

        private bool _enableSoccerCorners;
        public bool EnableSoccerCorners
        {
            get { return _enableSoccerCorners; }
            set { SetProperty(ref _enableSoccerCorners, value); }
        }

        private bool _enableSoccerCards;
        public bool EnableSoccerCards
        {
            get { return _enableSoccerCards; }
            set { SetProperty(ref _enableSoccerCards, value); }
        }

        private bool _enableBasketBall;
        public bool EnableBasketBall
        {
            get { return _enableBasketBall; }
            set { SetProperty(ref _enableBasketBall, value); }
        }

        private bool _enableVolleyBall;
        public bool EnableVolleyBall
        {
            get { return _enableVolleyBall; }
            set { SetProperty(ref _enableVolleyBall, value); }
        }

        private bool _enableBaseBall;
        public bool EnableBaseBall
        {
            get { return _enableBaseBall; }
            set { SetProperty(ref _enableBaseBall, value); }
        }

        private bool _enableTennis;
        public bool EnableTennis
        {
            get { return _enableTennis; }
            set { SetProperty(ref _enableTennis, value); }
        }

        private bool _enableTableTenis;
        public bool EnableTableTenis
        {
            get { return _enableTableTenis; }
            set { SetProperty(ref _enableTableTenis, value); }
        }

        private bool _enableHockey;
        public bool EnableHockey
        {
            get { return _enableHockey; }
            set { SetProperty(ref _enableHockey, value); }
        }

        private bool _enableRugby;
        public bool EnableRugby
        {
            get { return _enableRugby; }
            set { SetProperty(ref _enableRugby, value); }
        }

        private bool _enableESoccer;
        public bool EnableESoccer
        {
            get { return _enableESoccer; }
            set { SetProperty(ref _enableESoccer, value); }
        }

        private bool _enableHandball;
        public bool EnableHandball
        {
            get { return _enableHandball; }
            set { SetProperty(ref _enableHandball, value); }
        }


        private bool _enableValue1;
        public bool EnableValue1
        {
            get { return _enableValue1; }
            set { SetProperty(ref _enableValue1, value); }
        }

        private bool _enableValue2;
        public bool EnableValue2
        {
            get { return _enableValue2; }
            set { SetProperty(ref _enableValue2, value); }
        }

        private bool _enableValue3;
        public bool EnableValue3
        {
            get { return _enableValue3; }
            set { SetProperty(ref _enableValue3, value); }
        }



        private bool _enableTipster2;
        public bool EnableTipster2
        {
            get { return _enableTipster2; }
            set { SetProperty(ref _enableTipster2, value); }
        }

        private double _stakePerTipster2;
        public double StakePerTipster2
        {
            get { return _stakePerTipster2; }
            set { SetProperty(ref _stakePerTipster2, value); }
        }

        private bool _percentageStakeModeTipster2;
        public bool PercentageStakeModeTipster2
        {
            get { return _percentageStakeModeTipster2; }
            set { SetProperty(ref _percentageStakeModeTipster2, value); }
        }

        private double _minOddsTipster2;
        public double MinOddsTipster2
        {
            get { return _minOddsTipster2; }
            set { SetProperty(ref _minOddsTipster2, value); }
        }

        private double _maxOddsTipster2;
        public double MaxOddsTipster2
        {
            get { return _maxOddsTipster2; }
            set { SetProperty(ref _maxOddsTipster2, value); }
        }

        private bool _enableSoccerLive;
        public bool EnableSoccerLive
        {
            get { return _enableSoccerLive; }
            set { SetProperty(ref _enableSoccerLive, value); }
        }

        private double _stakeSoccerLive;
        public double StakeSoccerLive
        {
            get { return _stakeSoccerLive; }
            set { SetProperty(ref _stakeSoccerLive, value); }
        }

        private bool _percentageStakeModeSoccerLive;
        public bool PercentageStakeModeSoccerLive
        {
            get { return _percentageStakeModeSoccerLive; }
            set { SetProperty(ref _percentageStakeModeSoccerLive, value); }
        }

        private double _minOddsSoccerLive;
        public double MinOddsSoccerLive
        {
            get { return _minOddsSoccerLive; }
            set { SetProperty(ref _minOddsSoccerLive, value); }
        }

        private double _maxOddsSoccerLive;
        public double MaxOddsSoccerLive
        {
            get { return _maxOddsSoccerLive; }
            set { SetProperty(ref _maxOddsSoccerLive, value); }
        }

        private bool _enableHorseRacing;
        public bool EnableHorseRacing
        {
            get { return _enableHorseRacing; }
            set { SetProperty(ref _enableHorseRacing, value); }
        }

        private bool _enableHorseLessTime;
        public bool EnableHorseLessTime
        {
            get { return _enableHorseLessTime; }
            set { SetProperty(ref _enableHorseLessTime, value); }
        }

        private int _horseLessTimeMinute;
        public int HorseLessTimeMinute
        {
            get { return _horseLessTimeMinute; }
            set { SetProperty(ref _horseLessTimeMinute, value); }
        }

        private bool _enableSportsLessTime;
        public bool EnableSportsLessTime
        {
            get { return _enableSportsLessTime; }
            set { SetProperty(ref _enableSportsLessTime, value); }
        }

        private int _sportsLessTimeMinute;
        public int SportsLessTimeMinute
        {
            get { return _sportsLessTimeMinute; }
            set { SetProperty(ref _sportsLessTimeMinute, value); }
        }

        private bool _enableEachWay;
        public bool EnableEachWay
        {
            get { return _enableEachWay; }
            set { SetProperty(ref _enableEachWay, value); }
        }

        private double _eachWayOdd;
        public double EachWayOdd
        {
            get { return _eachWayOdd; }
            set { SetProperty(ref _eachWayOdd, value); }
        }

        private bool _ignoreUSARace;
        public bool IgnoreUSARace
        {
            get { return _ignoreUSARace; }
            set { SetProperty(ref _ignoreUSARace, value); }
        }

        private bool _ignoreUKRace;
        public bool IgnoreUKRace
        {
            get { return _ignoreUKRace; }
            set { SetProperty(ref _ignoreUKRace, value); }
        }

        private bool _ignoreAURace;
        public bool IgnoreAURace
        {
            get { return _ignoreAURace; }
            set { SetProperty(ref _ignoreAURace, value); }
        }


        private string _licenseKey;
        public string LicenseKey
        {
            get { return _licenseKey; }
            set { SetProperty(ref _licenseKey, value); }
        }

        private int _selectedBrowser;
        public int SelectedBrowser
        {
            get { return _selectedBrowser; }
            set { SetProperty(ref _selectedBrowser, value); }
        }

        private int _profileId;
        public int ProfileId
        {
            get { return _profileId; }
            set { SetProperty(ref _profileId, value); }
        }

        private string _dolphinId;
        public string DolphinId
        {
            get { return _dolphinId; }
            set { SetProperty(ref _dolphinId, value); }
        }

        private string _dolphinPass;
        public string DolphinPass
        {
            get { return _dolphinPass; }
            set { SetProperty(ref _dolphinPass, value); }
        }

        private ObservableCollection<string> _regionList;
        public ObservableCollection<string> RegionList
        {
            get { return _regionList; }
            set { SetProperty(ref _regionList, value); }
        }

        //private ObservableCollection<ScheduleItem> _scheduleList;
        //public ObservableCollection<ScheduleItem> ScheduleList
        //{
        //    get { return _scheduleList; }
        //    set { SetProperty(ref _scheduleList, value); }

        //}

        private ObservableCollection<DayOfWeek> _dayOfWeekList;
        public ObservableCollection<DayOfWeek> DayOfWeekList
        {
            get { return _dayOfWeekList; }
            set { SetProperty(ref _dayOfWeekList, value); }

        }
        private ObservableCollection<int> _hourList;
        public ObservableCollection<int> HourList
        {
            get { return _hourList; }
            set { SetProperty(ref _hourList, value); }
        }

        private ObservableCollection<int> _hourList1;
        public ObservableCollection<int> HourList1
        {
            get { return _hourList1; }
            set { SetProperty(ref _hourList1, value); }
        }

        private ObservableCollection<int> _minuteList;
        public ObservableCollection<int> MinuteList
        {
            get { return _minuteList; }
            set { SetProperty(ref _minuteList, value); }
        }

        private DayOfWeek _dayOfWeekVal;
        public DayOfWeek DayOfWeekVal
        {
            get { return _dayOfWeekVal; }
            set { SetProperty(ref _dayOfWeekVal, value); }
        }

        private int _startHour;
        public int StartHour
        {
            get { return _startHour; }
            set { SetProperty(ref _startHour, value); }
        }

        private int _startMinute;
        public int StartMinute
        {
            get { return _startMinute; }
            set { SetProperty(ref _startMinute, value); }
        }

        private int _endHour;
        public int EndHour
        {
            get { return _endHour; }
            set { SetProperty(ref _endHour, value); }
        }

        private int _endminute;
        public int EndMinute
        {
            get { return _endminute; }
            set { SetProperty(ref _endminute, value); }
        }

        private ObservableCollection<TimeRange> _timeRangeList;
        public ObservableCollection<TimeRange> TimeRangeList
        {
            get { return _timeRangeList; }
            set { SetProperty(ref _timeRangeList, value); }
        }

        public string _oddRangeMarketSelectedItem;
        public string OddRangeMarketSelectedItem
        {
            get { return _oddRangeMarketSelectedItem; }
            set
            {
                if (_oddRangeMarketSelectedItem != value)
                {
                    _oddRangeMarketSelectedItem = value;

                    // New item has been selected. Do something here
                }
            }
        }


        private ObservableCollection<string> _oddRangeMarketList;
        public ObservableCollection<string> OddRangeMarketList
        {
            get { return _oddRangeMarketList; }
            set { SetProperty(ref _oddRangeMarketList, value); }
        }

        private string _proxyRegion;
        public string ProxyRegion
        {
            get { return _proxyRegion; }
            set { SetProperty(ref _proxyRegion, value); }
        }

        private string _vmloginProfile;
        public string VMLoginProfile
        {
            get { return _vmloginProfile; }
            set { SetProperty(ref _vmloginProfile, value); }
        }

        private bool _enableDailyBetCountLimit;
        public bool EnableDailyBetCountLimit
        {
            get { return _enableDailyBetCountLimit; }
            set { SetProperty(ref _enableDailyBetCountLimit, value); }
        }

        private int _dailyBetCountLimit;
        public int DailyBetCountLimit
        {
            get { return _dailyBetCountLimit; }
            set { SetProperty(ref _dailyBetCountLimit, value); }
        }

        private bool _enableAllowOddDrop;
        public bool EnableAllowOddDrop
        {
            get { return _enableAllowOddDrop; }
            set { SetProperty(ref _enableAllowOddDrop, value); }
        }

        private double _allowOddDropPercent;
        public double AllowOddDropPercent
        {
            get { return _allowOddDropPercent; }
            set { SetProperty(ref _allowOddDropPercent, value); }
        }

        private bool _enableAllowOddRise;
        public bool EnableAllowOddRise
        {
            get { return _enableAllowOddRise; }
            set { SetProperty(ref _enableAllowOddRise, value); }
        }

        private double _allowOddRisePercent;
        public double AllowOddRisePercent
        {
            get { return _allowOddRisePercent; }
            set { SetProperty(ref _allowOddRisePercent, value); }
        }

        private bool _enableMajorLeaguesOnly;
        public bool EnableMajorLeaguesOnly
        {
            get { return _enableMajorLeaguesOnly; }
            set { SetProperty(ref _enableMajorLeaguesOnly, value); }
        }

        private bool _placeDoubleValues;
        public bool PlaceDoubleValues
        {
            get { return _placeDoubleValues; }
            set { SetProperty(ref _placeDoubleValues, value); }
        }

        private bool _placeFastMode;
        public bool PlaceFastMode
        {
            get { return _placeFastMode; }
            set { SetProperty(ref _placeFastMode, value); }
        }

        private bool _soccerEnableFirstHalfbets;
        public bool SoccerEnableFirstHalfbets
        {
            get { return _soccerEnableFirstHalfbets; }
            set
            {
                SetProperty(ref _soccerEnableFirstHalfbets, value);
                if (value == true)
                {
                    SoccerEnableSecondHalfbets = false;
                }
            }
        }

        private bool _soccerEnableSecondHalfbets;
        public bool SoccerEnableSecondHalfbets
        {
            get { return _soccerEnableSecondHalfbets; }
            set
            {
                SetProperty(ref _soccerEnableSecondHalfbets, value);
                if (value == true)
                {
                    SoccerEnableFirstHalfbets = false;
                }
            }
        }

        private bool _maxbetEnableSuperbet;
        public bool MaxbetEnableSuperbet
        {
            get { return _maxbetEnableSuperbet; }
            set { SetProperty(ref _maxbetEnableSuperbet, value); }

        }

        private double _maxSuperbetlimit;
        public double MaxSuperbetlimit
        {
            get { return _maxSuperbetlimit; }
            set { SetProperty(ref _maxSuperbetlimit, value); }
        }

        private bool _enableMaxPendingBets;
        public bool EnableMaxPendingBets
        {
            get { return _enableMaxPendingBets; }
            set { SetProperty(ref _enableMaxPendingBets, value); }
        }

        private int _maxPendingBetsLimit;
        public int MaxPendingBetsLimit
        {
            get { return _maxPendingBetsLimit; }
            set { SetProperty(ref _maxPendingBetsLimit, value); }
        }
        #endregion
    }
}