using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Project.Helphers;
using Project.Interfaces;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Project.ViewModels
{
    public class SettingViewModel : BindableBase, ISettingViewModel, INotifyPropertyChanged
    {
  
        public SettingViewModel(IEventAggregator eventAggregator)
        {
            this.SaveSettingCommand = new DelegateCommand(OnSaveSettingCommand);

            Setting.Instance.loadSetting();

            Bet365UserId = Setting.Instance.username_bet365;
            Bet365UserPwd = Setting.Instance.password_bet365;
            Bet365Domain = Setting.Instance.domain_bet365;

            LuckiaUserId = Setting.Instance.username_luckia;
            LuckiaUserPwd = Setting.Instance.password_luckia;

            RequestDelay = Setting.Instance.requestDelay;
            BetInterval = Setting.Instance.betInterval;
            Percentage = Setting.Instance.percentage;
            MinOdds = Setting.Instance.minOdds;
            MaxOdds = Setting.Instance.maxOdds;
            Stake = Setting.Instance.stake;
            
                                  
            EnableSoccer = Setting.Instance.bSoccer;
            EnableBasketBall = Setting.Instance.bBasketBall;
            EnableVolleyBall = Setting.Instance.bVolleyBall;
            EnableBaseBall = Setting.Instance.bBaseBall;
            EnableTennis = Setting.Instance.bTennis;
            EnableTableTenis = Setting.Instance.bTableTenis;
            EnableHockey = Setting.Instance.bHockey;
            EnableRugby = Setting.Instance.bRugby;
            EnableESoccer = Setting.Instance.bESoccer;
            EnableHorseRacing = Setting.Instance.bHorseRacing;
            

            SeverIp = Setting.Instance.ServerIP;
            SeverPort = Setting.Instance.ServerPort;
            LicenseKey = Setting.Instance.license;

            StartHour = Setting.Instance.StartHour;
            EndHour = Setting.Instance.EndHour;

            ProxyRegion = Setting.Instance.ProxyRegion;

            HourList = new ObservableCollection<int>();
            for (int i = 0; i < 24; i++)
            {
                HourList.Add(i);
            }
                        
            EndHourList = new ObservableCollection<int>();
            for (int i = 0; i < 24; i++)
            {
                EndHourList.Add(i);
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
        }


        public ICommand SaveSettingCommand { get; private set; }


        private void OnSaveSettingCommand()
        {
                        
            Setting.Instance.username_bet365 = Bet365UserId;
            Setting.Instance.password_bet365 = Bet365UserPwd;
            Setting.Instance.domain_bet365 = Bet365Domain;

            Setting.Instance.username_luckia = LuckiaUserId;
            Setting.Instance.password_luckia = LuckiaUserPwd;

            Setting.Instance.percentage = Percentage;
            Setting.Instance.minOdds = MinOdds;
            Setting.Instance.maxOdds = MaxOdds;
            Setting.Instance.stake = Stake;
            Setting.Instance.requestDelay = RequestDelay;
            Setting.Instance.betInterval = BetInterval;


            Setting.Instance.ServerIP = SeverIp;
            Setting.Instance.ServerPort = SeverPort;
            
            Setting.Instance.bSoccer = EnableSoccer;
            Setting.Instance.bBasketBall = EnableBasketBall;
            Setting.Instance.bVolleyBall = EnableVolleyBall;
            Setting.Instance.bBaseBall = EnableBaseBall;
            Setting.Instance.bTennis = EnableTennis;
            Setting.Instance.bTableTenis = EnableTableTenis;
            Setting.Instance.bHockey = EnableHockey;
            Setting.Instance.bRugby = EnableRugby;
            Setting.Instance.bESoccer = EnableESoccer;
            Setting.Instance.bHorseRacing = EnableHorseRacing;
            
            Setting.Instance.license = LicenseKey;
            
            Setting.Instance.StartHour = StartHour;
            Setting.Instance.EndHour = EndHour;

            Setting.Instance.ProxyRegion = ProxyRegion;

            Setting.Instance.saveSetting();
        }

        #region Properties                
        private string _Bet365UserId;
        public string Bet365UserId
        {
            get { return _Bet365UserId; }
            set { SetProperty(ref _Bet365UserId, value); }
        }

        private string _Bet365UserPwd;
        public string Bet365UserPwd
        {
            get { return _Bet365UserPwd; }
            set { SetProperty(ref _Bet365UserPwd, value); }
        }

        private string _Bet365domain;
        public string Bet365Domain
        {
            get { return _Bet365domain; }
            set { SetProperty(ref _Bet365domain, value); }
        }

        private string _LuckiaUserId;
        public string LuckiaUserId
        {
            get { return _LuckiaUserId; }
            set { SetProperty(ref _LuckiaUserId, value); }
        }

        private string _LuckiaUserPwd;
        public string LuckiaUserPwd
        {
            get { return _LuckiaUserPwd; }
            set { SetProperty(ref _LuckiaUserPwd, value); }
        }

        private int _requestDelay;
        public int RequestDelay
        {
            get { return _requestDelay; }
            set { SetProperty(ref _requestDelay, value); }
        }
                
        private double _percentage;
        public double Percentage
        {
            get { return _percentage; }
            set { SetProperty(ref _percentage, value); }
        }

        
        private double _minOdds;
        public double MinOdds
        {
            get { return _minOdds; }
            set { SetProperty(ref _minOdds, value); }
        }

        private double _maxOdds;
        public double MaxOdds
        {
            get { return _maxOdds; }
            set { SetProperty(ref _maxOdds, value); }
        }

        private double _stake;
        public double Stake
        {
            get { return _stake; }
            set { SetProperty(ref _stake, value); }
        }

        private int _betInterval;
        public int BetInterval
        {
            get { return _betInterval; }
            set { SetProperty(ref _betInterval, value); }
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


        private bool _enableHorseRacing;
        public bool EnableHorseRacing
        {
            get { return _enableHorseRacing; }
            set { SetProperty(ref _enableHorseRacing, value); }
        }

        private string _licenseKey;
        public string LicenseKey
        {
            get { return _licenseKey; }
            set { SetProperty(ref _licenseKey, value); }
        }

        private ObservableCollection<string> _regionList;
        public ObservableCollection<string> RegionList
        {
            get { return _regionList; }
            set { SetProperty(ref _regionList, value); }
        }

        private ObservableCollection<int> _hourList;
        public ObservableCollection<int> HourList
        {
            get { return _hourList; }
            set { SetProperty(ref _hourList, value); }
        }

        private ObservableCollection<int> _endhourList;
        public ObservableCollection<int> EndHourList
        {
            get { return _endhourList; }
            set { SetProperty(ref _endhourList, value); }
        }

        private int _startHour;
        public int StartHour
        {
            get { return _startHour; }
            set { SetProperty(ref _startHour, value); }
        }

        private int _endHour;
        public int EndHour
        {
            get { return _endHour; }
            set { SetProperty(ref _endHour, value); }
        }

        private ObservableCollection<TimeRange> _timeRangeList;
        public ObservableCollection<TimeRange> TimeRangeList
        {
            get { return _timeRangeList; }
            set { SetProperty(ref _timeRangeList, value); }
        }

        private string _proxyRegion;
        public string ProxyRegion
        {
            get { return _proxyRegion; }
            set { SetProperty(ref _proxyRegion, value); }
        }
        #endregion
    }
}