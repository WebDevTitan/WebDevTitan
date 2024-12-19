using log4net;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Project.Interfaces;
using Project.Models;
using Project.Views;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Project.Json;
using Project.Server;
using System.Threading;
using Project.Helphers;
using Project.Bookie;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Windows.Interop;
using FlaUI.UIA3;
using System.Diagnostics;
using FlaUI.Core.AutomationElements;

namespace Project.ViewModels
{
    #region Delegate
    public delegate void onWriteStatusEvent(string status);        
    public delegate void onBetburgerServerEvent(List<BetburgerInfo> betburgerInfoList);
    
    #endregion

    class MainWindowViewModel : BindableBase, ICloseable
    {       

        #region fields
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #endregion


        #region Bet
        public double totalStake = 0;

        IBookieController bookieControllerMain = null;
        IBookieController bookieControllerSub = null;

        PopupDialog popupDialog_Main = null;
        PopupDialog popupDialog_Sub = null;

        private UserInfo user;

        public Thread threadReconnect = null;
        public Thread threadBet = null;

        private object _totalBetburgerInfoLocker = new object();
        private List<BetburgerInfo> _betburgerInfo = new List<BetburgerInfo>();
        private List<BetburgerInfo> _totalBetburgerInfo = new List<BetburgerInfo>();
        private List<PlacedBetInfo> _placedBetInfo = new List<PlacedBetInfo>();
        private List<FailedBetburgerInfo> _failedBetburgerInfo = new List<FailedBetburgerInfo>();
        #endregion



        public event EventHandler<EventArgs> RequestClose;
        public event EventHandler<EventArgs> RequestMinimize;
        public event EventHandler<EventArgs> RequestRestore;

        public MainWindowViewModel(IUnityContainer container, IEventAggregator eventAggregator)
        {
            LogText = "";
            IsStopped = true;
            IsStarted = false;



            ServerSureBetList = new ObservableCollection<BetburgerInfo>();
            FinishedBetList = new ObservableCollection<PlacedBetInfo>();

            LogMng.Instance.onWriteStatus = SetLog;
            LogMng.Instance.onWriteStatus("App Started");
            
            BetViewModel = (BetViewModel)container.Resolve<IBetViewModel>();
            SettingViewModel = (SettingViewModel)container.Resolve<ISettingViewModel>();


            MenuCommand = new DelegateCommand<string>(async (index) => await OnMenuCommand(index));
            WindowCommand = new DelegateCommand<string>((index) => OnWindowCommand(index));

            StartCommand = new DelegateCommand(OnStartCommand);
            StopCommand = new DelegateCommand(OnStopCommand);
            RefreshBalanceCommand = new DelegateCommand(OnRefreshBalanceCommand);

            UserMng.GetInstance().stopEvent += OnStopCommand;

            Version = string.Format("version: {0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());

            OnMenuCommand("BET");


            popupDialog_Main = new PopupDialog(BOOKMAKER.BET365);
            popupDialog_Main.Show();

            popupDialog_Sub = new PopupDialog(BOOKMAKER.LUCKIA);
            popupDialog_Sub.Show();


            AccountId = Setting.Instance.username_bet365 + " - " + Setting.Instance.username_luckia;

            //string aamsId = "";
            //string catId = "";
            //string disId = "";
            //string evnDate = "";
            //string evnDateTs = "";
            //string evtId = "";
            //string evtName = "";
            //string hdrType = "";
            //string markId = "";
            //string markName = "";
            //string markTypId = "";
            //string oddsId = "";
            //string oddsValue = "";
            //string onLineCode = "";
            //string prvIdEvt = "";
            //string selId = "";
            //string selName = "";
            //string tId = "";
            //string tName = "";
            //string vrt = "";


            //string betSlipResponseContent = File.ReadAllText("1.txt");

            //JObject origObject = JObject.Parse(betSlipResponseContent);

            //string EventdetailUrl = string.Empty;
            //foreach (var objEvent in origObject["leo"])
            //{
            //    if (objEvent["enm"].ToString() == "Carla Suarez Navarro - Sloane Stephens" && objEvent["tdsc"].ToString() == "Grand Slam Roland Garros")
            //    {                    
            //        aamsId = objEvent["aid"].ToString();
            //        catId = objEvent["cid"].ToString();
            //        disId = objEvent["sid"].ToString();
            //        evnDate = objEvent["edt"].ToString();
            //        evnDateTs = objEvent["edts"].ToString();
            //        evtId = objEvent["eid"].ToString();
            //        evtName = objEvent["enm"].ToString();
            //        onLineCode = objEvent["ocd"].ToString();
            //        prvIdEvt = objEvent["eprId"].ToString();
            //        tId = objEvent["tid"].ToString();
            //        tName = objEvent["tdsc"].ToString();
            //        vrt = objEvent["vrt"].ToString().ToLower();

            //        EventdetailUrl = string.Format("https://goldbet.it/getDetailsEventLive/{0}/{1}", objEvent["sid"].ToString(), objEvent["eid"].ToString());
            //        break;
            //    }
            //}

            //if (string.IsNullOrEmpty(EventdetailUrl))
            //    return;

            //bool bFound = false;
            //betSlipResponseContent = File.ReadAllText("2.txt");
            //origObject = JObject.Parse(betSlipResponseContent);
            //foreach (var objEvent in origObject["mktWbD"])
            //{
            //    JToken matchObj = objEvent.ToObject<JProperty>().Value;
            //    if (matchObj["mn"].ToString() == "T/T Set")
            //    {
            //        foreach (var marketEvent in matchObj["ms"])
            //        {
            //            JToken marketObj = marketEvent.ToObject<JProperty>().Value;
            //            foreach (var piEvent in marketObj["asl"])
            //            {
            //                if (piEvent["si"].ToString() == "1529372476" && piEvent["mi"].ToString() == "583070778")
            //                {
            //                    hdrType = matchObj["ht"].ToString();
            //                    markId = piEvent["mi"].ToString();
            //                    markName = matchObj["mn"].ToString();
            //                    markTypId = piEvent["mti"].ToString();
            //                    oddsId = piEvent["oi"].ToString();
            //                    oddsValue = piEvent["ov"].ToString();
            //                    selId = piEvent["si"].ToString();
            //                    selName = piEvent["sn"].ToString();
            //                    bFound = true;
            //                    break;
            //                }
            //            }
            //            if (bFound)
            //                break;
            //        }
            //        break;
            //    }
            //}

            //string ReqJson = $"{{\"single_stake\":{{\"combsInfo\":[{{\"combType\":\"1\",\"combNum\":\"1\",\"stake\":2}}],\"sumCombsXType\":1}},\"total_stake\":2,\"fixed\":[],\"events\":[{{\"isLive\":\"true\",\"selName\":\"{selName}\",\"selId\":\"{selId}\",\"oddsId\":\"{oddsId}\",\"oddsValue\":\"{oddsValue}\",\"markName\":\"{markName}({selName})\",\"markId\":\"{markId}\",\"markTypId\":\"{markTypId}\",\"hdrType\":\"{hdrType}\",\"prvIdEvt\":\"{prvIdEvt}\",\"aamsId\":\"{aamsId}\",\"evtName\":\"{evtName}\",\"evnDate\":\"{evnDate}\",\"evnDateTs\":\"{evnDateTs}\",\"evtId\":\"{evtId}\",\"catId\":\"{catId}\",\"disId\":\"{disId}\",\"tId\":\"{tId}\",\"tName\":\"{tName}\",\"onLineCode\":\"{onLineCode}\",\"vrt\":{vrt}}}],\"creationTime\":{Utils.getTick()},\"virtual\":false,\"allowStakeReduction\":true,\"allowOddChanges\":true}}";
        }


        private bool canStart()
        {   
            if (string.IsNullOrEmpty(Setting.Instance.ServerIP))
            {                
                MessageBox.Show("Please enter the server ip!", "Error");
                return false;
            }

            if (Setting.Instance.ServerPort < 1377)
            {
                MessageBox.Show("Please enter the server port!");
                return false;
            }

            if (string.IsNullOrEmpty(Setting.Instance.license))
            {
                MessageBox.Show("Please enter the license key!");
                return false;
            }

            if (string.IsNullOrEmpty(Setting.Instance.username_bet365) || string.IsNullOrEmpty(Setting.Instance.username_luckia))
            {
                MessageBox.Show("Please enter the username!");
                return false;
            }

            if (string.IsNullOrEmpty(Setting.Instance.password_bet365) || string.IsNullOrEmpty(Setting.Instance.password_luckia))
            {
                MessageBox.Show("Please enter the password!");
                return false;
            }
            
          

            if (!Setting.Instance.bSoccer && 
                !Setting.Instance.bBasketBall &&
                !Setting.Instance.bVolleyBall && 
                !Setting.Instance.bBaseBall && 
                !Setting.Instance.bTennis && 
                !Setting.Instance.bTableTenis && 
                !Setting.Instance.bHockey &&
                !Setting.Instance.bRugby &&
                !Setting.Instance.bESoccer &&
                !Setting.Instance.bHorseRacing)
            {
                MessageBox.Show("Please check at least one sport!");
                return false;
            }

            
            if (Setting.Instance.stake <= 0)
            {
                MessageBox.Show("Please input correct stake for sports!", "Error");
                return false;
            }

            if (Setting.Instance.percentage == 0)
            {
                MessageBox.Show("Please enter the percentage for sports!");
                return false;
            }
#if (ARB_LIMIT)
            if (Setting.Instance.percentageToSports <= Setting.Instance.percentageSports)
            {
                MessageBox.Show("Please enter the percent maximum value correctly!");
                return false;
            }
#endif

            if (Setting.Instance.minOdds >= Setting.Instance.maxOdds)
            {
                MessageBox.Show("Please enter the correct odds rangefor sports!");
                return false;
            }
            

            return true;
        }

        private void ReconnectThread(object type)
        {            
            while (Global.bRun)
            {
                if (!Global.bServerConnect)
                {
                    if (user != null)
                    {
                        user.DisconnectToServer();
                        user = null;
                    }
                    user = new UserInfo();
                    user.ConnectToServer();
                }

                Thread.Sleep(5000);

                if (Global.bServerConnect && user.Connector.ConnectionDeadSeconds > 240)
                {
                    LogMng.Instance.onWriteStatus("Socket Connection Lost from Server!");
                    Global.bServerConnect = false;
                }
            }
        }

        private bool checkPercent(BetburgerInfo info)
        {
            double percent = 0, percentageTo = 0;
            
            percent = Setting.Instance.percentage;
                        

            if (info.percent < (decimal)percent)
            {
                //LogMng.Instance.onWriteStatus(string.Format("Skip this event cause of Percent limit ({0} < {1})", info.percent, percent));
                return false;
            }

            return true;
        }

        private bool checkSports(BetburgerInfo info)
        {
            if (info.sport == "Soccer")
            {
                if (!Setting.Instance.bSoccer)
                    return false;
            }
            else if (info.sport == "Basketball")
            {
                if (!Setting.Instance.bBasketBall)
                    return false;
            }
            else if (info.sport == "Volleyball")
            {
                if (!Setting.Instance.bVolleyBall)
                    return false;
            }
            else if (info.sport == "Baseball")
            {
                if (!Setting.Instance.bBaseBall)
                    return false;
            }
            else if (info.sport == "Tennis")
            {
                if (!Setting.Instance.bTennis)
                    return false;
            }
            else if (info.sport == "Table Tennis")
            {
                if (!Setting.Instance.bTableTenis)
                    return false;
            }
            else if (info.sport == "Hockey")
            {
                if (!Setting.Instance.bHockey)
                    return false;
            }
            else if (info.sport == "Rugby")
            {
                if (!Setting.Instance.bRugby)
                    return false;
            }
            else if (info.sport == "E-Sports")
            {
                if (!Setting.Instance.bESoccer)
                    return false;
            }
            else if (info.sport == "Horse Racing")
            {
                if (!Setting.Instance.bHorseRacing)
                    return false;
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool checkOdds(BetburgerInfo info)
        {
            double minodd = 0, maxodd = 0;
            
            minodd = Setting.Instance.minOdds;
            maxodd = Setting.Instance.maxOdds;
            

            if (info.odds > maxodd || info.odds < minodd)
                return false;

            return true;
        }

        private bool checkSameLine(BetburgerInfo info)
        {
            if (_totalBetburgerInfo.Count == 0)
                return false;

            if (info.sport == "Horse Racing")
            {
                IEnumerable<BetburgerInfo> sameHorseInforms = _totalBetburgerInfo.Where(node => node.eventTitle == info.eventTitle && node.homeTeam == info.homeTeam && node.outcome == info.outcome);
                if (sameHorseInforms != null && sameHorseInforms.LongCount() > 0)
                    return true;
            }
            else
            {
                IEnumerable<BetburgerInfo> sameInforms = _totalBetburgerInfo.Where(node => node.eventTitle == info.eventTitle);
                //IEnumerable<BettingInfo> sameInforms = _totalBetburgerInfo;
                if (sameInforms != null && sameInforms.LongCount() > 0)
                {
                    foreach (var node in sameInforms)
                    {
                        try
                        {
                            string node_outcome = node.outcome;
                            int nFirstIndex = node_outcome.Length;
                            if (nFirstIndex < 1)
                                continue;
                            int nSearchIndex = node_outcome.IndexOf(' ');
                            if (nSearchIndex > 0 && nSearchIndex < nFirstIndex)
                                nFirstIndex = nSearchIndex;

                            nSearchIndex = node_outcome.IndexOf('(');
                            if (nSearchIndex > 0 && nSearchIndex < nFirstIndex)
                                nFirstIndex = nSearchIndex;

                            nSearchIndex = node_outcome.IndexOf('-');
                            if (nSearchIndex > 0 && nSearchIndex < nFirstIndex)
                                nFirstIndex = nSearchIndex;

                            for (int i = 0; i < 10; i++)
                            {
                                nSearchIndex = node_outcome.IndexOf(i.ToString());
                                if (nSearchIndex > 0 && nSearchIndex < nFirstIndex)
                                    nFirstIndex = nSearchIndex;
                            }

                            string subOutCome = node_outcome.Substring(0, nFirstIndex).ToLower();

                            //LogMng.Instance.onWriteStatus(string.Format("checkSameLine Checking: cur outcome: {0} sub outcome: {1}", info.outcome, subOutCome));
                            if (info.outcome.ToLower().Contains(subOutCome))
                                return true;
                        }
                        catch (Exception ex)
                        {
                            LogMng.Instance.onWriteStatus("checkSameLine Exception: " + ex);
                        }
                    }                    
                }
            }

            return false;
        }

        public bool checkSameEventInDirectlink(BetburgerInfo info)
        {
#if (BET365_BM)
            int nSameEvent = 0;
            try
            {
                Monitor.Enter(_totalBetburgerInfoLocker);
                if (_totalBetburgerInfo.Count == 0)
                    return false;

                try
                {
                    OpenBet_Bet365 newBet = Utils.ConvertBetburgerPick2OpenBet_365(info);
                    if (newBet != null)
                    {
                        for (int i = 0; i < _totalBetburgerInfo.Count; i++)
                        {
                            OpenBet_Bet365 oldBet = Utils.ConvertBetburgerPick2OpenBet_365(_totalBetburgerInfo[i]);

                            if (oldBet != null)
                            {
                                if (newBet.betData[0].fd == oldBet.betData[0].fd && newBet.betData[0].i2 == oldBet.betData[0].i2)
                                    return true;

                                if (newBet.betData[0].fd == oldBet.betData[0].fd)
                                    nSameEvent++;


                                if (info.sport == "Horse Racing")
                                {
                                    if (nSameEvent >= 2)
                                        return true;
                                }
                                else
                                {
                                    if (nSameEvent >= 1)
                                        return true;
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        return checkSameEvent(info) || checkSameLine(info);
                    }
                }
                catch { }
            }
            catch { }
            finally
            {
                Monitor.Exit(_totalBetburgerInfoLocker);
            }
#endif
            return false;
        }
        private bool checkSameEvent(BetburgerInfo info)
        {
            if (_totalBetburgerInfo.Count == 0)
                return false;

            if (info.sport == "Horse Racing")
            {
                List<string> betedHorse = new List<string>();
                IEnumerable<BetburgerInfo> sameInforms = _totalBetburgerInfo.Where(node => node.eventTitle == info.eventTitle);
                if (sameInforms != null)
                {
                    foreach (var itr in sameInforms)
                    {
                        if (!betedHorse.Contains(itr.homeTeam))
                        {
                            betedHorse.Add(itr.homeTeam);
                        }
                    }
                    if (betedHorse.Count >= 2)
                        return true;
                }
            }
            else
            {
                IEnumerable<BetburgerInfo> sameInforms = _totalBetburgerInfo.Where(node => node.eventTitle == info.eventTitle);
                if (sameInforms != null && sameInforms.Count() >= 3)
                    return true;
            }
            return false;
        }

        private bool checkBetInfo(PlacedBetInfo info)
        {
            try
            {
                if (_placedBetInfo.Count == 0)
                    return true;

                PlacedBetInfo existInfo = _placedBetInfo.FirstOrDefault(node => node.eventTitle == info.eventTitle && node.homeTeam == info.homeTeam && node.awayTeam == info.awayTeam && node.outcome == info.outcome && node.odds == info.odds);
                if (existInfo == null)
                    return true;

                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void addBetList(BetburgerInfo info)
        {
            PlacedBetInfo _info = new PlacedBetInfo();
            
            _info.timeStamp = DateTime.Now.ToString("MM-dd-yyyy HH:mm");
            _info.bookmaker = info.bookmaker;
            _info.username = Setting.Instance.username_bet365;            
            _info.eventTitle = info.eventTitle;
            _info.odds = info.odds;
            _info.percent = info.percent;
            _info.outcome = info.outcome;
            _info.homeTeam = info.homeTeam;
            _info.awayTeam = info.awayTeam;
            

            _info.sport = info.sport;
            _info.stake = info.stake;
            
            totalStake += info.stake;
            

            lock (_placedBetInfo)
            {
                if (!checkBetInfo(_info))
                    return;

                _placedBetInfo.Add(_info);

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    FinishedBetList.Add(_info);
                    Balance = Global.balanceMain.ToString("N2") + " - " + Global.balanceSub.ToString("N2");
                    TotalBet = string.Format("{0} ({1})", FinishedBetList.Count, totalStake);
                }));
                
            }            
        }

        private async Task<PROCESS_RESULT> placeBet(BetburgerInfo info)
        {            
            LogMng.Instance.onWriteStatus(string.Format("Trying to place to bet {0} {1} {2} {3} {4} outcome {5} odd {6}", info.bookmaker, info.sport, info.eventTitle, info.homeTeam, info.awayTeam, info.outcome, info.odds));


            PROCESS_RESULT result = PROCESS_RESULT.ERROR;
            if (info.bookmaker == "Bet365")
                result = bookieControllerMain.PlaceBet(info);
            else if (info.bookmaker == "10bet")
                result = bookieControllerSub.PlaceBet(info);

            if (result == PROCESS_RESULT.PLACE_SUCCESS)
            {

#if (GOLDBET || SNAI)
                Global.balance = bookieController.getBalance();
#elif (BET365_BM)

#else
                Global.balance -= info.stake;
#endif
                if (info.bookmaker == "Bet365")
                    NextFakeAddbet = DateTime.Now.AddMinutes(15);

                addBetList(info);
                LogMng.Instance.onWriteStatus(string.Format("Success to place to bet {0} {1} {2}", info.eventTitle, info.homeTeam, info.awayTeam));
            }
            else if (result == PROCESS_RESULT.CRITICAL_SITUATION)
            {
                LogMng.Instance.onWriteStatus(string.Format("************************************"));
#if (EUROBET || LOTTOMATICA || GOLDBET || LEOVEGAS || SNAI)
                LogMng.Instance.onWriteStatus(string.Format("*BOT STOPPED BECAUSE CRITICAL ALERT*"));
#elif (UNIBET)
                LogMng.Instance.onWriteStatus(string.Format("*BOT STOPPED BECAUSE LOGIN FAILED*"));
#endif
                LogMng.Instance.onWriteStatus(string.Format("************************************"));
                OnStopCommand();
            }
            else if (result == PROCESS_RESULT.NO_LOGIN)
            {
                LogMng.Instance.onWriteStatus(string.Format("Failed because of logout {0} {1} {2}", info.eventTitle, info.homeTeam, info.awayTeam));
                bookieControllerMain.login();                
            }
            else
            {
                LogMng.Instance.onWriteStatus(string.Format("Fail to place to bet {0} {1} {2}", info.eventTitle, info.homeTeam, info.awayTeam));
            }
            return result;            
        }

        DateTime NextFakeAddbet = DateTime.Now.AddMinutes(15);
        private void betTask()
        {
            while (Global.bRun)
            {
                BetburgerInfo info = null;
                if (_betburgerInfo.Count >= 2)
                {
                    List<Task> TaskList = new List<Task>();
                    for (int i = 0; i < 2; i++)
                    {
                        info = _betburgerInfo[0];
                        _betburgerInfo.RemoveAt(0);

                        TaskList.Add(placeBet(info));                        
                    }
                    Task.WaitAll(TaskList.ToArray());
                    if (Setting.Instance.betInterval >= 20)
                        Thread.Sleep(Setting.Instance.betInterval * 1000);
                    else
                        Thread.Sleep(20 * 1000);
                    //bool bSuccessed = false;
                    //foreach (var task in TaskList)
                    //{
                    //    PROCESS_RESULT result = task.
                    //}
                    //if (bSuccessed)
                    //{ 
                    //    Thread.Sleep(120000);
                    //}
                    //else
                    //{
                    //    Thread.Sleep(10000);
                    //}

                }
                else
                {
                    if (NextFakeAddbet < DateTime.Now)
                    {
                        NextFakeAddbet = DateTime.Now.AddMinutes(15);

                        string result = Global.GetStatusValue_Main("return Locator.user.isLoggedIn;").ToLower();

                        if (result == "true")
                        {
                            var command = "var xhr = new XMLHttpRequest();" +
                                "xhr.open('GET', 'https://www." + Setting.Instance.domain_bet365 + "/sessionactivityapi/setlastactiontime', true);" +
                                "xhr.withCredentials = true; " +
                                "xhr.onreadystatechange = function() { " +
                                "if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {}" +
                                "}" +
                                "xhr.send(); ";
                            Global.RunScriptCode_Main(command);
                            Thread.Sleep(500);
                            Global.RefreshPage_Main();
                            Thread.Sleep(3000);
                            Global.RefreshBecauseBet365Notloading_Main();
                        }
                    }

                }
            }
        }


        private bool alreadyExistBetburgerInfo(BetburgerInfo info)
        {
            foreach (BetburgerInfo _info in _totalBetburgerInfo)
            {
                if (info.sport == "Horse Racing")
                {
                    if (_info.eventTitle == info.eventTitle && _info.homeTeam == info.homeTeam)
                        return true;
                }
                else
                {
                    if (_info.eventTitle == info.eventTitle && _info.outcome == info.outcome)
                        return true;
                }
            }

            return false;
        }

        
        private void displayBetburger(List<BetburgerInfo> bbInfo)
        {
            try
            {
                //LogMng.Instance.onWriteStatus(string.Format("Found {0} new arbs...", bbInfo.Count));

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                  
                    ServerSureBetList.Clear();
                    foreach (BetburgerInfo info in bbInfo)
                    {

                        if (info.bookmaker != "Bet365" && info.bookmaker != "10bet")
                            continue;
                   
                        ServerSureBetList.Add(info);
                    }
                }));
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus(string.Format("displayBetburger Exception {0} ", ex));
            }
        }

        private void ArbReceived(List<BetburgerInfo> rawbetburgerInfoList)
        {
            List<BetburgerInfo> betburgerInfoList = new List<BetburgerInfo>();

            if ((rawbetburgerInfoList.Count <= 0) || (rawbetburgerInfoList.Count % 2 != 0))
                return;

            for (int i = 0; i < rawbetburgerInfoList.Count / 2; i++)
            {
                //check TO, TU included market
                //if (rawbetburgerInfoList[i * 2].outcome.Contains("TO") && rawbetburgerInfoList[i * 2 + 1].outcome.Contains("TU"))
                //{   

                //}
                //else if(rawbetburgerInfoList[i * 2].outcome.Contains("TU") && rawbetburgerInfoList[i * 2 + 1].outcome.Contains("TO"))
                //{

                //}
                //else
                //{
                //    continue;
                //}
                //rawbetburgerInfoList[i * 2].outcome = rawbetburgerInfoList[i * 2].outcome.Replace("+", "");
                //rawbetburgerInfoList[i * 2 + 1].outcome = rawbetburgerInfoList[i * 2 + 1].outcome.Replace("+", "");
                betburgerInfoList.Add(rawbetburgerInfoList[i * 2]);
                betburgerInfoList.Add(rawbetburgerInfoList[i * 2 + 1]);
            }

            displayBetburger(betburgerInfoList);

            try
            {
                if (!Global.bRun)
                    return;

                if (betburgerInfoList == null)
                    betburgerInfoList = new List<BetburgerInfo>();

               
                if (Setting.Instance.StartHour != Setting.Instance.EndHour)
                {
                    int curhour = DateTime.Now.ToUniversalTime().Hour;

                    bool bIsInRestRange = false;
                    if (Setting.Instance.StartHour > Setting.Instance.EndHour)
                    {                        
                        if (Setting.Instance.StartHour <= curhour || curhour < Setting.Instance.EndHour)
                            bIsInRestRange = true;
                    }
                    else
                    {
                        if (Setting.Instance.StartHour <= curhour && curhour < Setting.Instance.EndHour)
                            bIsInRestRange = true;
                    }
                    if (bIsInRestRange)
                        return;                
                }

                while (betburgerInfoList.Count > 0)
                {
                    BetburgerInfo info_Main = null, info_Sub = null;

                    if (betburgerInfoList[0].bookmaker == "Bet365" && betburgerInfoList[1].bookmaker == "10bet")
                    {
                        info_Main = betburgerInfoList[0];
                        info_Sub = betburgerInfoList[1];

                        betburgerInfoList.RemoveRange(0, 2);
                    }
                    else if (betburgerInfoList[1].bookmaker == "Bet365" && betburgerInfoList[0].bookmaker == "10bet")
                    {
                        info_Main = betburgerInfoList[1];
                        info_Sub = betburgerInfoList[0];

                        betburgerInfoList.RemoveRange(0, 2);
                    }
                    else
                    {
                        //LogMng.Instance.onWriteStatus(string.Format("Ignore_Bookie mistmatch different {0}:{1}", betburgerInfoList[0].bookmaker, betburgerInfoList[1].bookmaker));
                        betburgerInfoList.RemoveRange(0, 2);
                        continue;
                    }

                    if (info_Main.sport != info_Sub.sport)
                    {
                        //LogMng.Instance.onWriteStatus(string.Format("Ignore_Sports different {0}:{1}", info_Main.sport, info_Sub.sport));
                        continue;
                    }
                    //check percent every bookie
                    if (!checkSports(info_Main))
                    {
                        //LogMng.Instance.onWriteStatus(string.Format("Ignore_Sports mismatched Processed {0} - {1}:{2} [bookmaker : {3}] {4}", info_Main.sport, info_Main.homeTeam, info_Main.awayTeam, info_Main.extra, info_Main.sport));
                        continue;
                    }

                    if (info_Main.percent != info_Sub.percent)
                    {
                        //LogMng.Instance.onWriteStatus(string.Format("Ignore_Percent different {0}:{1}", info_Main.percent, info_Sub.percent));
                        continue;
                    }

                    if (!checkPercent(info_Main))
                    {
                        //LogMng.Instance.onWriteStatus(string.Format("Ignore_Arb mismatched Processed {0} - {1}:{2} [bookmaker : {3}] {4}:{5}", info_Main.sport, info_Main.homeTeam, info_Main.awayTeam, info_Main.extra, info_Main.percent, Setting.Instance.percentage));
                        continue;
                    }

                    //check odds range
                    if (!checkOdds(info_Main) && !checkOdds(info_Sub))
                    {
                        //LogMng.Instance.onWriteStatus(string.Format("Ignore_Odd mismatched Processed {0} - {1}:{2} [bookmaker : {3}] {4}:{5}-{6}", info_Main.sport, info_Main.homeTeam, info_Main.awayTeam, info_Main.extra, info_Main.odds, Setting.Instance.minOdds, Setting.Instance.maxOdds));
                        continue;
                    }

                    if (checkSameEventInDirectlink(info_Main))
                    {
                        //LogMng.Instance.onWriteStatus(string.Format("Ignore_SameEvent mismatched Processed {0} - {1}:{2} [bookmaker : {3}] {4}:{5}-{6}", info_Main.sport, info_Main.homeTeam, info_Main.awayTeam, info_Main.extra, info_Main.odds, Setting.Instance.minOdds, Setting.Instance.maxOdds));
                        continue;
                    }

                    try
                    {
                        Monitor.Enter(_totalBetburgerInfoLocker);
                        if (alreadyExistBetburgerInfo(info_Main))
                        {
                            //LogMng.Instance.onWriteStatus(string.Format("Ignore_Already Processed {0} - {1}:{2} [bookmaker : {3}] ", info_Main.sport, info_Main.homeTeam, info_Main.awayTeam, info_Main.extra));
                            continue;
                        }

                        _totalBetburgerInfo.Add(info_Main);
                    }
                    catch { }
                    finally
                    {
                        Monitor.Exit(_totalBetburgerInfoLocker);
                    }

                    info_Main.stake = info_Sub.odds / (info_Main.odds + info_Sub.odds) * Setting.Instance.stake;
                    info_Main.stake = Math.Truncate(info_Main.stake * 100) / 100;

                    info_Sub.stake = Setting.Instance.stake - info_Main.stake;

                    LogMng.Instance.onWriteStatus(string.Format("Addbet to stack {0} - {1}:{2} [{3} : {4}] ", info_Main.sport, info_Main.homeTeam, info_Main.awayTeam, info_Main.bookmaker, info_Sub.bookmaker));
                    _betburgerInfo.Add(info_Main);
                    _betburgerInfo.Add(info_Sub);
                }
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus("[Exception : ArbReceived] " + e.ToString());
            }
        }

#if (EUROBET || LOTTOMATICA || GOLDBET || LEOVEGAS || SNAI || PLANETWIN || BETFAIR)
        private Thread EurobetReconnectThread = null;
        private void EurobetReconnectRun()
        {
            Thread.Sleep(60 * 1000);

            while (true)
            {
                int nSleepMinute = 10;
#if (EUROBET)
    nSleepMinute = 8 * 60;
#elif (LOTTOMATICA || SNAI)
    nSleepMinute = 200 * 60;
#elif (GOLDBET || LEOVEGAS)
    nSleepMinute = 19 * 60;
#elif (PLANETWIN)
    nSleepMinute = 95 * 60;
#endif
                Thread.Sleep(nSleepMinute * 1000);

                if (bookieController == null)
                    continue;

                if (!bookieController.Pulse())
                {
                    bool logResult = bookieController.login();
                    if (logResult)
                    {
                        Global.balance = bookieController.getBalance();


                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            Balance = Global.balance.ToString("N2");
                        }));
                    }
                }
            }
        }
#endif
        private async void OnStartCommand()
        {
            //var app = FlaUI.Core.Application.Attach(Process.GetCurrentProcess());
            //using (var automation = new UIA3Automation())
            //{
            //    FlaUI.Core.AutomationElements.Window[] Window = app.GetAllTopLevelWindows(automation);
            //    foreach (var window in Window)
            //    {
            //        if (window.Title.Contains("Monitor"))
            //        {
            //            AutomationElement[] childElements = window.FindAllDescendants();
            //            foreach (var item in childElements)
            //            {
            //                Trace.WriteLine(item.Name);
            //                if (item.Name == "Iniciar sesión")
            //                {
            //                    System.Drawing.Point point = item.GetClickablePoint();
            //                    FlaUI.Core.Input.Mouse.LeftClick(point);                            
            //                }
            //            }
            //        }
            //        Console.WriteLine(window.Title);
            //    }
            //}

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                AccountId = Setting.Instance.username_bet365 + " - " + Setting.Instance.username_luckia;
            }));

            if (Global.bRun)
                return;

            if (!canStart())
                return;

            LogMng.Instance.onWriteStatus("Bot Started");

            IsStarted = true;
            IsStopped = false;
            Global.bRun = true;

            if (UserMng.GetInstance().onArbReceived == null)
                UserMng.GetInstance().onArbReceived = ArbReceived;

            if (UserMng.GetInstance().connectedServer == null)
                UserMng.GetInstance().connectedServer = OnConnectedServer; 

            _betburgerInfo.Clear();
            _failedBetburgerInfo.Clear();

            threadReconnect = new Thread(new ParameterizedThreadStart(ReconnectThread));
            threadReconnect.IsBackground = true;
            threadReconnect.Start(0);

        }

        public void OnConnectedServer()
        {
            LogMng.Instance.onWriteStatus("Connected Bot Server Successfully!");
            bookieControllerMain = new Bet365_BMCtrl();
            bookieControllerSub = new LuckiaCtrl();


            LogMng.Instance.onWriteStatus("Bot Start and login");

            bool logResult = false;
            logResult = bookieControllerMain.login() && bookieControllerSub.login();
            
            if (logResult)
            {
                Global.balanceMain = bookieControllerMain.getBalance();
                Global.balanceSub = bookieControllerSub.getBalance();

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Balance = Global.balanceMain.ToString("N2") + " - " + Global.balanceSub.ToString("N2");
                }));

                LogMng.Instance.onWriteStatus(string.Format("Balance: {0} - {1}", Global.balanceMain, Global.balanceSub));
            }
            else
            {
                LogMng.Instance.onWriteStatus(string.Format("Login failed!"));
                OnStopCommand();
                return;
            }

            threadBet = new Thread(betTask);
            threadBet.IsBackground = true;
            threadBet.Start();

        }

      
        private void OnRefreshBalanceCommand()
        {
            Global.balanceMain = bookieControllerMain.getBalance();
            Global.balanceSub = bookieControllerSub.getBalance();

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Balance = Global.balanceMain.ToString("N2") + " - " + Global.balanceSub.ToString("N2");
            }));

          
        }
        private void OnStopCommand()
        {
            if (!Global.bRun)
                return;

            Global.bRun = false;

            IsStopped  = true;
            IsStarted = false;

            UserMng.GetInstance().SendToolClose();
            UserMng.GetInstance().SetUserInfoState(USERSTATUS.NOLOGIN_STATUS);

            try
            {
                if (threadReconnect != null && threadReconnect.IsAlive)
                    threadReconnect.Abort();
            }
            catch { }

            try
            {
                user.DisconnectToServer();
            }
            catch { }
            user = null;

            try
            {
                if (threadBet != null && threadBet.IsAlive)
                    threadBet.Abort();
            }
            catch { }

#if (EUROBET || LOTTOMATICA || GOLDBET || LEOVEGAS || SNAI || PLANETWIN)
            if (EurobetReconnectThread != null)
            {
                EurobetReconnectThread.Abort();
                EurobetReconnectThread = null;
            }
#endif
            LogMng.Instance.onWriteStatus("Bot Stopped");
        }

        private void SetServerBetList(List<BetburgerInfo> list)
        {

        }

        private void AddFinishedBetList(BetburgerInfo param)
        {

        }

        string LogFilePath = $"logs\\";
        string LogFileName = $"log_{DateTime.Now.ToString("yyyyMMddHHmmss")}.log";
        private void SetLog(string log)
        {
            Global.WriteTroubleShotLog(log);
            try 
            {
                Directory.CreateDirectory(LogFilePath);

                bool bNeedtoCreateNew = false;
                using (FileStream fileStream = File.Open($"{LogFilePath}{LogFileName}", FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    if (fileStream.Length > 5 * 1024 * 1024)
                    {
                        bNeedtoCreateNew = true;
                    }
                    else
                    {
                        using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                        {
                            streamWriter.WriteLine(string.Format("{0} {1}", DateTime.Now.ToString(), log));
                        }
                    }
                }

                if (bNeedtoCreateNew)
                {
                    LogFileName = $"log_{DateTime.Now.ToString("yyyyMMddHHmmss")}.log";
                    using (FileStream fileStream = File.Open($"{LogFilePath}{LogFileName}", FileMode.Append, FileAccess.Write, FileShare.Read))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                        {
                            streamWriter.WriteLine(string.Format("{0} {1}", DateTime.Now.ToString(), log));
                        }
                    }
                }
            
                if (LogText.Length > 30000)
                    LogText = "";

                if (!string.IsNullOrEmpty(LogText))
                    LogText += "\r\n" + log;
                else
                    LogText = log;

            }
            catch { }
        }

        private void OnWindowCommand(string index)
        {
            log.Info(string.Format("OnWindowCommand - {0}", index));

            if (index == "MINIMIZE")
            {
                this.RequestMinimize.Invoke(this, null);
            }
            else if (index == "RESTORE")
            {
                this.RequestRestore.Invoke(this, null);
            }
            else
            {
                OnStopCommand();
                this.RequestClose.Invoke(this, null);

                if (popupDialog_Main != null)
                    popupDialog_Main.Close();

                if (popupDialog_Sub != null)
                    popupDialog_Sub.Close();

            }
        }

        private async Task OnMenuCommand(string index)
        {
            log.Info(string.Format("OnMenuCommand - {0}", index));

            this.CurrentTab = index;
        }

#region properties
        public ICommand StartCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand RefreshBalanceCommand { get; private set; }
        public ICommand WindowCommand { get; private set; }
        public ICommand MenuCommand { get; private set; }

        public SettingViewModel SettingViewModel { get; private set; }
        public BetViewModel BetViewModel { get; private set; }


        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _version;
        public string Version
        {
            get { return _version; }
            set { SetProperty(ref _version, value); }
        }

        private string _currentTab;
        public string CurrentTab
        {
            get { return _currentTab; }
            set { SetProperty(ref _currentTab, value); }
        }

        private string _logText;
        public string LogText
        {
            get { return _logText; }
            set { SetProperty(ref _logText, value); }
        }

        private string _accountId;
        public string AccountId
        {
            get { return _accountId; }
            set { SetProperty(ref _accountId, value); }
        }

        private string _balance;
        public string Balance
        {
            get { return _balance; }
            set { SetProperty(ref _balance, value); }
        }

        private ObservableCollection<BetburgerInfo> _serverSureBetList;
        public ObservableCollection<BetburgerInfo> ServerSureBetList
        {
            get { return _serverSureBetList; }
            set { SetProperty(ref _serverSureBetList, value); }
        }

        private ObservableCollection<PlacedBetInfo> _finishedBetList;
        public ObservableCollection<PlacedBetInfo> FinishedBetList
        {
            get { return _finishedBetList; }
            set { SetProperty(ref _finishedBetList, value); }
        }

        private string _totalBet;
        public string TotalBet
        {
            get { return _totalBet; }
            set { SetProperty(ref _totalBet, value); }
        }

        private bool _isStarted;
        public bool IsStarted
        {
            get { return _isStarted; }
            set { SetProperty(ref _isStarted, value); }
        }

        private bool _isStopped;
        public bool IsStopped
        {
            get { return _isStopped; }
            set { SetProperty(ref _isStopped, value); }
        }

#endregion
    }
}
