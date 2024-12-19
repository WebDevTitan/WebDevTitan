using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Gma.System.MouseKeyHook;
using log4net;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Project.Bookie;
using Project.Helphers;
using Project.Interfaces;
using Project.Models;
using Project.Server;
using Project.Views;
using Protocol;

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

        IBookieController bookieController = null;


        private bool bBrowserVisible = true;
        PopupDialog popupDialog = null;
        Random random = new Random();

        private UserInfo user;

        public Thread threadReconnect = null;
        public Thread threadBet = null;

        private object _runthreadLocker = new object();

        private List<BetburgerInfo> curInfo = new List<BetburgerInfo>();
        private List<List<BetburgerInfo>> _placePickInfo = new List<List<BetburgerInfo>>();
        private List<BetburgerInfo> _placedoublePickInfo = new List<BetburgerInfo>();

        private List<FailedBetburgerInfo> _failedBetburgerInfo = new List<FailedBetburgerInfo>();
        #endregion


        public bool SleepStatus = false;

        public event EventHandler<EventArgs> RequestClose;
        public event EventHandler<EventArgs> RequestMinimize;
        public event EventHandler<EventArgs> RequestRestore;

        private List<BetResult> BetHistory = new List<BetResult>();
        private IKeyboardMouseEvents m_GlobalHook;
        private void WriteBetHistory(BetResult result)
        {

            Monitor.Enter(_runthreadLocker);
            BetHistory.Add(result);
            Monitor.Exit(_runthreadLocker);
            try
            {
                string text = JsonConvert.SerializeObject(BetHistory);
                File.WriteAllText("bethistory.txt", text);
            }
            catch { }
        }

        private void LoadBetHistory()
        {

            Monitor.Enter(_runthreadLocker);
            BetHistory.Clear();
            try
            {
                string text = File.ReadAllText("bethistory.txt");
                BetHistory = JsonConvert.DeserializeObject<List<BetResult>>(text);

                for (int i = BetHistory.Count - 1; i >= 0; i--)
                {
                    if (DateTime.Now.Subtract(BetHistory[i].date).TotalDays > 2)
                        BetHistory.RemoveAt(i);
                }
            }
            catch { }
            Monitor.Exit(_runthreadLocker);

        }

        public MainWindowViewModel(IUnityContainer container, IEventAggregator eventAggregator)
        {
            LoadBetHistory();
            GuardStart();
            LogText = "";
            IsStopped = true;
            IsStarted = false;
            RefreshableBalance = true;

            ServerSureBetList = new ObservableCollection<BetburgerInfo>();
            FinishedBetList = new ObservableCollection<BetburgerInfo>();

            LogMng.Instance.onWriteStatus = SetLog;

            BetViewModel = (BetViewModel)container.Resolve<IBetViewModel>();
            SettingViewModel = (SettingViewModel)container.Resolve<ISettingViewModel>();


            MenuCommand = new DelegateCommand<string>(async (index) => await OnMenuCommand(index));
            WindowCommand = new DelegateCommand<string>((index) => OnWindowCommand(index));

            StartCommand = new DelegateCommand(OnStartCommand);
            StopCommand = new DelegateCommand(OnStopCommand);
            ClearBetHistory = new DelegateCommand(OnClearBetHistoryCommand);
            RefreshBalanceCommand = new DelegateCommand(OnRefreshBalanceCommand);
            GetStakeCommand = new DelegateCommand(OnGetStakeCommand);

            UserMng.GetInstance().stopEvent = OnStopCommand;

            Version = string.Format("version: {0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());

            OnMenuCommand("BET");

            //m_GlobalHook = Hook.GlobalEvents();

            //m_GlobalHook.KeyDown += M_GlobalHook_KeyDown;


#if (BET365_ADDON || BETWAY_ADDON)
            try
            {
                Global.socketServer = new WebSocketServer(12323, false);
                Global.socketServer.AddWebSocketService<ClientWebSocket>("/");
                Global.socketServer.Start();
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus(ex.Message);

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MessageBox.Show("Please check port 12323 is already in use");
                }));                
            }
#endif

#if (BET365_BM || SNAI || UNIBET || SKYBET || PLANETWIN || GOLDBET || LOTTOMATICA || PINNACLE || STOIXIMAN || BETANO|| BETPREMIUM || _888SPORT || SPORTPESA || REPLATZ || CHANCEBET || DOMUSBET || BETALAND|| BETFAIR_FETCH || BETFAIR || ELYSGAME || EUROBET || FANDUEL || BETMGM || NOVIBET || BETWAY || BETFAIR_NEW || RUSHBET)
            popupDialog = new PopupDialog();
            popupDialog.Show();


#if (LOTTOMATICA || GOLDBET)
            Global.SetMonitorVisible(false);
#endif

#endif
            AccountId = Setting.Instance.username;


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


        }


        private bool canStart()
        {
            if (string.IsNullOrEmpty(Setting.Instance.ServerIP))
            {
                MessageBox.Show("Please enter the server ip(domain)!", "Error");
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

            if (string.IsNullOrEmpty(Setting.Instance.username))
            {
                MessageBox.Show("Please enter the username!");
                return false;
            }

            if (string.IsNullOrEmpty(Setting.Instance.password))
            {
                MessageBox.Show("Please enter the password!");
                return false;
            }

            if (Setting.Instance.maxEventCount < 1 || Setting.Instance.maxEventCount > 30)
            {
                MessageBox.Show("Please input correct Max bets in event number!", "Error");
                return false;
            }

#if (BET365_VM || BET365_BM || BET365_PL || BET365_CHROMEDEV || BET365_PUPPETEER || BET365_ADDON || SUPERBET || BETANO || BETANO_CDP || STOIXIMAN || STOIXIMAN_CDP || BETFAIR || BETFAIR_NEW || BWIN || SPORTINGBET || RUSHBET || BETPLAY || PLAYPIX || FORTUNA || KTO || WPLAY || SEUBET || BET365_QRAPI)


            if (Setting.Instance.bTipster2)
            {
                if (Setting.Instance.stakePerTipster2 <= 0)
                {
                    MessageBox.Show("Please input correct stake for Tipster2", "Error");
                    return false;
                }
            }

            if (Setting.Instance.bSoccerLive)
            {
                if (Setting.Instance.percentageStakeModeSoccerLive)
                {
                    if (Setting.Instance.stakeSoccerLive <= 0 && Setting.Instance.stakeSoccerLive > 7)
                    {
                        MessageBox.Show("Please input correct stake for Soccer Live (0.1 - 7)", "Error");
                        return false;
                    }
                }
                else
                {
                    if (Setting.Instance.stakeSoccerLive <= 0)
                    {
                        MessageBox.Show("Please input correct stake for Soccer Live ", "Error");
                        return false;
                    }
                }
            }
#else
            Setting.Instance.bValue1 = true;
#endif
            if (Setting.Instance.bValue1 || Setting.Instance.bValue2 || Setting.Instance.bValue3)
            {
                if (Setting.Instance.bSportsLessTime)
                {
                    if (Setting.Instance.nSportsLessTimeMinute <= 0)
                    {
                        MessageBox.Show("Please input correct less minute value before match start!", "Error");
                        return false;
                    }
                }

                if (Setting.Instance.stakeSports <= 0)
                {
                    MessageBox.Show("Please input correct stake for sports!", "Error");
                    return false;
                }

                if (Setting.Instance.percentageSports == 0)
                {
                    MessageBox.Show("Please enter the percentage for sports!");
                    return false;
                }

                if (Setting.Instance.percentageToSports <= Setting.Instance.percentageSports)
                {
                    MessageBox.Show("Please enter correct arb percentage range!");
                    return false;
                }

                if (Setting.Instance.maxOddsSports <= Setting.Instance.minOddsSports)
                {
                    MessageBox.Show("Please enter correct odd range!");
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
                    !Setting.Instance.bHandball)
                {
                    MessageBox.Show("Please check at least one sport!");
                    return false;
                }
            }
            else
            {
                if (!Setting.Instance.bTipster2 && !Setting.Instance.bSoccerLive && !Setting.Instance.bHorseRacing)
                {
                    MessageBox.Show("Please select bot mode(value or tip source)");
                    return false;
                }
            }

            if (Setting.Instance.bHorseRacing)
            {
                if (Setting.Instance.bHorseLessTime)
                {
                    if (Setting.Instance.nHorseLessTimeMinute <= 0)
                    {
                        MessageBox.Show("Please input correct less minute value before Racing event!", "Error");
                        return false;
                    }
                }
                if (Setting.Instance.stakeHorse <= 0)
                {
                    MessageBox.Show("Please input correct stake for horse!", "Error");
                    return false;
                }

                if (Setting.Instance.percentageHorse == 0)
                {
                    MessageBox.Show("Please enter the percent for horse!");
                    return false;
                }
#if (ARB_LIMIT)
                if (Setting.Instance.percentageToHorse <= Setting.Instance.percentageHorse)
                {
                    MessageBox.Show("Please enter the percent maximum value correctly!");
                    return false;
                }
#endif

                if (Setting.Instance.minOddsHorse >= Setting.Instance.maxOddsHorse)
                {
                    MessageBox.Show("Please enter the correct odds range for horse!");
                    return false;
                }
            }


            if (Setting.Instance.bEachWay && Setting.Instance.eachWayOdd == 0)
                Setting.Instance.eachWayOdd = 5.1;


            return true;
        }

        private void ReconnectThread(object type)
        {
            while (Global.bRun)
            {
                try
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
                catch { }
            }
        }

        private bool checkRemainTime(BetburgerInfo info)
        {
            if (info.sport == "Horse Racing")
            {
                if (Setting.Instance.bHorseLessTime && Setting.Instance.nHorseLessTimeMinute > 0)
                {
                    try
                    {
                        //LogMng.Instance.onWriteStatus(string.Format("LessTime checking {0} {1} {2} {3}", info.eventTitle, info.homeTeam, info.awayTeam, info.started));
                        DateTime curTime = DateTime.Now.ToUniversalTime();
                        DateTime startTime = DateTime.Now.AddDays(1);
                        try
                        {
                            //betburger format
                            startTime = DateTime.ParseExact(info.started, "MM-dd-yyyy HH:mm", null).AddHours(1);
                        }
                        catch
                        {
                            try
                            {
                                //betspan format
                                startTime = DateTime.ParseExact(info.started, "MM/dd/yyyy HH:mm:ss", null);
                            }
                            catch
                            {
                                //LogMng.Instance.onWriteStatus("Parsing date exception try second");
                                startTime = Convert.ToDateTime(info.started).AddHours(1);  //time is based UTC(-1)
                            }
                        }

                        int nTotalMinutes = (int)startTime.Subtract(curTime).TotalMinutes;
                        int nSettingMinutes = Setting.Instance.nHorseLessTimeMinute;

                        //LogMng.Instance.onWriteStatus(string.Format("Cur {0} Start {1} total {2} seting {3}", curTime, startTime, nTotalMinutes, nSettingMinutes));
                        if (nTotalMinutes > nSettingMinutes)
                            return false;
                    }
                    catch { }
                }
            }
            else
            {
                if (Setting.Instance.bSportsLessTime && Setting.Instance.nSportsLessTimeMinute > 0)
                {
                    try
                    {
                        //LogMng.Instance.onWriteStatus(string.Format("LessTime checking {0} {1} {2} {3}", info.eventTitle, info.homeTeam, info.awayTeam, info.started));
                        DateTime curTime = DateTime.Now.ToUniversalTime();
                        DateTime startTime = DateTime.Now.AddDays(1);
                        try
                        {
                            //betburger format
                            startTime = DateTime.ParseExact(info.started, "MM-dd-yyyy HH:mm", null).AddHours(1);
                        }
                        catch
                        {
                            try
                            {
                                //betspan format
                                startTime = DateTime.ParseExact(info.started, "MM/dd/yyyy HH:mm:ss", null);
                            }
                            catch
                            {
                                //LogMng.Instance.onWriteStatus("Parsing date exception try second");
                                startTime = Convert.ToDateTime(info.started).AddHours(1);  //time is based UTC(-1)
                            }
                        }


                        int nTotalMinutes = (int)startTime.Subtract(curTime).TotalMinutes;
                        int nSettingMinutes = Setting.Instance.nSportsLessTimeMinute;

                        //LogMng.Instance.onWriteStatus(string.Format("Cur {0} Start {1} total {2} seting {3}", curTime, startTime, nTotalMinutes, nSettingMinutes));

                        if (nTotalMinutes > nSettingMinutes)
                            return false;
                    }
                    catch { }
                }
            }
            return true;
        }
        private bool checkPercent(BetburgerInfo info)
        {
            double percent = 0, percentageTo = 0;
            if (info.sport == "Horse Racing")
            {
                percent = Setting.Instance.percentageHorse;
                percentageTo = Setting.Instance.percentageToHorse;
            }
            else
            {
                percent = Setting.Instance.percentageSports;
                percentageTo = Setting.Instance.percentageToSports;
            }

            if (info.percent < (decimal)percent)
            {
                //LogMng.Instance.onWriteStatus(string.Format("Skip this event cause of Percent limit ({0} < {1})", info.percent, percent));
                return false;
            }

#if (ARB_LIMIT)
            if (info.percent > (decimal)percentageTo)
            {
                //LogMng.Instance.onWriteStatus(string.Format("Skip this event cause of Percent limit ({0} > {1})", info.percent, percentageTo));
                return false;
            }
#endif

            return true;
        }

        private bool CheckIsInRestTimeRange()
        {
            if (Setting.Instance.StartHour != Setting.Instance.EndHour)
            {
                int curhour = DateTime.Now.Hour;

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
                    return true;
            }
            return false;
        }
        private bool checkSports(BetburgerInfo info, bool bdefaultReturn = false)
        {
#if (WINAMAX)
                return true;
#endif

            if (info.sport.ToLower() == "soccer")
            {
                if (!Setting.Instance.bSoccer)
                    return false;
            }
            else if (info.sport.ToLower() == "basketball")
            {
                if (!Setting.Instance.bBasketBall)
                    return false;
            }
            else if (info.sport.ToLower() == "volleyball")
            {
                if (!Setting.Instance.bVolleyBall)
                    return false;
            }
            else if (info.sport.ToLower() == "baseball")
            {
                if (!Setting.Instance.bBaseBall)
                    return false;
            }
            else if (info.sport.ToLower() == "tennis")
            {
                if (!Setting.Instance.bTennis)
                    return false;
            }
            else if (info.sport.ToLower() == "table tennis")
            {
                if (!Setting.Instance.bTableTenis)
                    return false;
            }
            else if (info.sport.ToLower() == "hockey")
            {
                if (!Setting.Instance.bHockey)
                    return false;
            }
            else if (info.sport.ToLower() == "rugby")
            {
                if (!Setting.Instance.bRugby)
                    return false;
            }
            else if (info.sport.ToLower() == "e-sports" || info.sport == "E-Soccer")
            {
                if (!Setting.Instance.bESoccer)
                    return false;
                else if (info.outcome.Contains("European"))
                    return false;
            }
            else if (info.sport.ToLower() == "handball")
            {
                if (!Setting.Instance.bHandball)
                    return false;
            }
            else if (info.sport.ToLower() == "horse racing")
            {
                if (!Setting.Instance.bHorseRacing)
                    return false;
            }
            else
            {
                return bdefaultReturn;
            }

            return true;
        }

        private bool checkOdds(BetburgerInfo info)
        {
            double minodd = 0, maxodd = 0;
            if (info.sport == "Horse Racing")
            {
                minodd = Setting.Instance.minOddsHorse;
                maxodd = Setting.Instance.maxOddsHorse;
            }
            else
            {
                minodd = Setting.Instance.minOddsSports;
                maxodd = Setting.Instance.maxOddsSports;
            }

            if (info.odds > maxodd || info.odds < minodd)
                return false;

            return true;
        }

        private bool checkSameLine(BetburgerInfo info)
        {
            try
            {
                Monitor.Enter(_runthreadLocker);
                if (BetHistory.Count == 0)
                    return false;

                if (info.sport == "Horse Racing")
                {
                    IEnumerable<BetburgerInfo> sameHorseInforms = BetHistory.Where(node => node.eventTitle == info.eventTitle && node.homeTeam == info.homeTeam && node.outcome == info.outcome);
                    if (sameHorseInforms != null && sameHorseInforms.LongCount() > 0)
                        return true;
                }
                else
                {
                    IEnumerable<BetburgerInfo> sameInforms = BetHistory.Where(node => node.eventTitle == info.eventTitle);
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
            }
            catch { }
            finally
            {
                Monitor.Exit(_runthreadLocker);
            }
            return false;
        }

        private void AddBetHistory(BetburgerInfo info)
        {
            info.date = DateTime.Now;

#if (SUPERBET)
            try
            {
                int removed_cnt = FinishedBetList.ToList().RemoveAll(b => DateTime.Now.Subtract(b.date).TotalHours > 24);
                if(removed_cnt > 0)
                    LogMng.Instance.onWriteStatus($"Removed {removed_cnt} bets with out of date");
            }
            catch { }
#endif

            if (info.sport == "Horse Racing")
            {
                if (Setting.Instance.bEachWay && info.odds >= Setting.Instance.eachWayOdd)
                    totalStake += info.stake * 2;
            }
            else
            {
                totalStake += info.stake;
            }

            UserMng.GetInstance().SendSuccessBetReport(new PlacedBetInfo(info, Global.balance));
            WriteBetHistory(new BetResult(info));

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    FinishedBetList.Add(info);
                    Balance = Global.balance.ToString("N2");
                    TotalBet = string.Format("{0} ({1})", FinishedBetList.Count, totalStake);
                }
                catch { }
            }));


        }

        private bool IsSameMarketPlacedAlready(BetburgerInfo info)
        {
            //checking for same event bet(for betburger pick only)
            if (info.kind != PickKind.Type_1)
                return false;

            try
            {
                for (int i = 0; i < BetHistory.Count; i++)
                {
                    if (info.eventTitle == BetHistory[i].eventTitle && info.homeTeam == BetHistory[i].homeTeam && info.awayTeam == BetHistory[i].awayTeam)
                    {
                        string infoOutcome = info.outcome;
                        string placedOutcome = BetHistory[i].outcome;

                        if (infoOutcome.Contains("("))
                        {
                            infoOutcome = infoOutcome.Substring(0, infoOutcome.IndexOf("("));
                        }

                        if (placedOutcome.Contains("("))
                        {
                            placedOutcome = placedOutcome.Substring(0, placedOutcome.IndexOf("("));
                        }

                        LogMng.Instance.onWriteStatus($"Checking outcome cur {infoOutcome} orig {placedOutcome}");
                        infoOutcome = infoOutcome.Trim().ToLower();
                        placedOutcome = placedOutcome.Trim().ToLower();
                        if (infoOutcome == placedOutcome)
                        {
                            LogMng.Instance.onWriteStatus($"Checking outcome Skip bet because of same");
                            return true;
                        }

                        if (infoOutcome.Contains(placedOutcome) || placedOutcome.Contains(infoOutcome))
                        {
                            LogMng.Instance.onWriteStatus($"Checking outcome Skip bet because of similar kind");
                            return true;
                        }
                    }
                }

            }
            catch { }

            return false;
        }
        private bool CheckMaxBetsInEvent(BetburgerInfo info)
        {
            //checking bets count in event
            try
            {
                int nSameEventCount = 0;
                for (int i = 0; i < BetHistory.Count; i++)
                {
                    if (info.eventTitle == BetHistory[i].eventTitle)
                    {
                        nSameEventCount++;
                    }
                }

                if (nSameEventCount >= Setting.Instance.maxEventCount)
                {

                    LogMng.Instance.onWriteStatus(string.Format("maxEventCount Placed bet {0} {1} {2} {3} {4} outcome {5} odd {6} direct {7} curCount {8}", info.bookmaker, info.sport, info.eventTitle, info.homeTeam, info.awayTeam, info.outcome, info.odds, info.direct_link, nSameEventCount));
                    return true;
                }
            }
            catch { }
            return false;
        }

        private bool AlreadyPlacedWithSameDirectLinkOutCome(BetburgerInfo info)
        {
            //checking same bet is placed using directlink
            try
            {
                Monitor.Enter(_runthreadLocker);
#if (BET365_ADDON)
                OpenBet_Bet365 openbet = Utils.ConvertBetburgerPick2OpenBet_365(info);
                if (openbet != null)
                {
                    for (int i = 0; i < BetHistory.Count; i++)
                    {
                        OpenBet_Bet365 openhistorybet = Utils.ConvertBetburgerPick2OpenBet_365(BetHistory[i]);

                        if (openbet != null && openhistorybet != null)
                        {
                            if (openbet.betData[0].fd == openhistorybet.betData[0].fd && openbet.betData[0].i2 == openhistorybet.betData[0].i2)
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
#endif
                for (int i = 0; i < BetHistory.Count; i++)
                {
                    if ((info.kind == PickKind.Type_2) || (info.bookmaker.ToLower() != "bfsportsbook" && info.bookmaker.ToLower() != "betway" && info.kind == PickKind.Type_7))
                    {
                        if (info.arbId == BetHistory[i].arbId)
                        {
                            return true;
                        }

                        if (info.kind == PickKind.Type_2)
                        {
                            if (BetHistory[i].eventTitle == info.eventTitle)
                            {
                                if (BetHistory[i].outcome == info.outcome)
                                    return true;
                            }
                        }
                    }
                    else
                    {
                        //if (info.outcome == BetHistory[i].outcome && info.direct_link == BetHistory[i].direct_link && info.homeTeam == BetHistory[i].homeTeam && info.awayTeam == BetHistory[i].awayTeam && info.league == BetHistory[i].league)
                        //{
                        //    return true;
                        //}

                        if (info.outcome == BetHistory[i].outcome && info.homeTeam == BetHistory[i].homeTeam && info.awayTeam == BetHistory[i].awayTeam && info.league == BetHistory[i].league)
                        {
                            return true;
                        }

                        if (info.kind != PickKind.Type_6 && !string.IsNullOrEmpty(info.outcome) && info.eventTitle == BetHistory[i].eventTitle && info.outcome == BetHistory[i].outcome)
                        {
                            return true;
                        }
                    }

                }
#if (BET365_ADDON)
                }
#endif
            }
            catch (Exception ex)
            {

            }
            finally
            {
                Monitor.Exit(_runthreadLocker);
            }
            return false;
        }

        private void betTask()
        {
            while (Global.bRun)
            {
                if (CheckIsInRestTimeRange())
                {
                    if (!Global.IsRestStatus)
                    {
                        Global.IsRestStatus = true;
                        LogMng.Instance.onWriteStatus("Rest started...");

                        bookieController.logout();
                    }
                }
                else
                {
                    if (Global.IsRestStatus)
                    {
                        Global.IsRestStatus = false;
                        LogMng.Instance.onWriteStatus("Working started...");
                    }
                }

                if (Global.IsRestStatus)
                {
                    Monitor.Enter(_runthreadLocker);
                    _placePickInfo.Clear();
                    Monitor.Exit(_runthreadLocker);

                    Thread.Sleep(1000);
                    continue;
                }


                Monitor.Enter(_runthreadLocker);
                curInfo.Clear();
                if (_placePickInfo.Count > 0)
                {
                    curInfo.AddRange(_placePickInfo[0]);
                    _placePickInfo.RemoveAt(0);
                }
                Monitor.Exit(_runthreadLocker);

                try
                {
                    if (curInfo.Count < 1)
                    {
                        //get one-time pick failed history
                        BetburgerInfo failedInfo = GetRetryOneTimePickFromFailedHistory();
                        if (failedInfo != null)
                        {

                            curInfo.Add(failedInfo);
                        }
                        else
                        {
                            Thread.Sleep(50);
#if (BET365_ADDON)
                            if (bookieController != null)
                                bookieController.Feature();
#endif

                            continue;
                        }
                    }
                    Console.WriteLine("bookieController", bookieController);
                    if (bookieController != null)
                    {
                        if (CheckMaxBetsInEvent(curInfo[0]))
                            continue;
                        if (AlreadyPlacedWithSameDirectLinkOutCome(curInfo[0]))
                            continue;

                        LogMng.Instance.onWriteStatus("【Placebet Thread】 There's task--------------------------");

#if (PRE)
                        Setting.Instance.bValue1 = false;
#endif

#if (CHRISTIAN)
                        if (curInfo[0].isLive)          
                        {
                            if (!Setting.Instance.bValue1)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (!Setting.Instance.bValue2)
                            {
                                continue;
                            }
                        }
#endif

                        if (curInfo[0].isLive)
                        {
                            LogMng.Instance.onWriteStatus("This Game Status : LIVE");
                            LogMng.Instance.onWriteStatus($"Setting Status : {Setting.Instance.bValue1.ToString()}");
                        }
                        else
                        {
                            LogMng.Instance.onWriteStatus($"Setting Status : {Setting.Instance.bValue1.ToString()}");
                            LogMng.Instance.onWriteStatus("This Game Status : PREMATCH");
                        }

                        LogMng.Instance.onWriteStatus(string.Format("Pick to place to bet kind: {10} sport: {0} league: {1} home: {2} away: {3} directlink: {4} outcome: {5} percent: {6} odds: {7} siteur: {8} eventurl: {9}"
                            , curInfo[0].sport, curInfo[0].league, curInfo[0].homeTeam, curInfo[0].awayTeam, curInfo[0].direct_link, curInfo[0].outcome, curInfo[0].percent, curInfo[0].odds, curInfo[0].siteUrl, curInfo[0].eventUrl, curInfo[0].kind));


                        List<PROCESS_RESULT> perResult = null;
#if (BET365_ADDON || LOTTOMATICA)
                        PROCESS_RESULT result = PROCESS_RESULT.ERROR;
                        
                        result = bookieController.PlaceBet(curInfo, out perResult);
                        
#else
                        Console.WriteLine("place bet part");
                        BetburgerInfo param = curInfo[0];
                        PROCESS_RESULT result = bookieController.PlaceBet(ref param);
                        curInfo[0] = param;
#endif

                        if (result == PROCESS_RESULT.PLACE_SUCCESS)
                        {
                            //Successed to place bet at least 1 bet                           
                            for (int i = 0; i < curInfo.Count; i++)
                            {

                                LogMng.Instance.onWriteStatus(string.Format("Success to place to bet {0} {1} {2} {3} {4}", curInfo[i].stake, curInfo[i].eventTitle, curInfo[i].homeTeam, curInfo[i].awayTeam, curInfo[i].direct_link));
                                RemoveFailedHistory(curInfo[i]);
                                AddBetHistory(curInfo[i]);
                            }

                            Global.balance = bookieController.getBalance();
                            UserMng.GetInstance().SendClientBalance(Global.balance);
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                Balance = Global.balance.ToString("N2");
                            }));
                        }
                        else
                        {
#if (BET365_ADDON)
                            if (perResult != null && perResult.Count == curInfo.Count)
                            {
                                for (int i = 0; i < perResult.Count; i++)
                                {
                                    //if (perResult[i] == PROCESS_RESULT.SUSPENDED || perResult[i] == PROCESS_RESULT.MOVED)
                                    //{
                                    //    LogMng.Instance.onWriteStatus(string.Format("Marked as Fail retry later {0} {1} {2} {3} {4} -> result : {5}", curInfo[i].league, curInfo[i].homeTeam, curInfo[i].awayTeam, curInfo[i].direct_link, curInfo[i].outcome, perResult[i]));
                                    //    AddFailedHistory(curInfo[i]);
                                    //}
                                    //else
                                    //{
                                        LogMng.Instance.onWriteStatus(string.Format("Failed , not retry anymore {0} {1} {2} {3} {4} -> result : {5}", curInfo[i].league, curInfo[i].homeTeam, curInfo[i].awayTeam, curInfo[i].direct_link, curInfo[i].outcome, perResult[i]));
                                        AddFailedHistory(curInfo[i]);
                                    //}
                                }
                            }
#elif (LOTTOMATICA)
                            if (perResult != null && perResult.Count == curInfo.Count)
                            {
                                for (int i = 0; i < perResult.Count; i++)
                                {
                                    if (perResult[i] != PROCESS_RESULT.SUCCESS)
                                    {
                                        LogMng.Instance.onWriteStatus(string.Format("Failed({5}), not retry anymore {0} {1} {2} {3} {4}", curInfo[i].league, curInfo[i].homeTeam, curInfo[i].awayTeam, curInfo[i].direct_link, curInfo[i].outcome, perResult[i]));
                                        AddFailedHistory(curInfo[i], true);
                                    }
                                    else
                                    {
                                        LogMng.Instance.onWriteStatus(string.Format("Cancelled({5}) , retry later {0} {1} {2} {3} {4}", curInfo[i].league, curInfo[i].homeTeam, curInfo[i].awayTeam, curInfo[i].direct_link, curInfo[i].outcome, perResult[i]));
                                        AddFailedHistory(curInfo[i]);
                                    }
                                }
                            }
#elif (SUPERBET)
                            if (result == PROCESS_RESULT.ZERO_MAX_STAKE)
                            {
                                LogMng.Instance.onWriteStatus($"This account is limited now. The bot will be stopped now...");
                                OnStopCommand();
                            }
#else
                            {
                                LogMng.Instance.onWriteStatus(string.Format("Marked as Fail {0} {1} {2} {3} {4} -> result : {5}", curInfo[0].league, curInfo[0].homeTeam, curInfo[0].awayTeam, curInfo[0].direct_link, curInfo[0].outcome, result));
                                AddFailedHistory(curInfo[0]);
                            }

#endif


                            if (result == PROCESS_RESULT.CRITICAL_SITUATION)
                            {
#if (!BET365_VM && !BET365_BM && !BET365_PL && !BET365_CHROMEDEV && !BET365_PUPPETEER && !BET365_ADDON)
                                LogMng.Instance.onWriteStatus(string.Format("************************************"));
#endif

#if (EUROBET || LOTTOMATICA || GOLDBET || LEOVEGAS || SNAI || BETPREMIUM || SPORTPESA || REPLATZ || DOMUSBET || CHANCEBET || BETFAIR_PL || BETFAIR_FETCH || BETFAIR || BETALAND || BETFAIR_NEW)
                                LogMng.Instance.onWriteStatus(string.Format("*BOT STOPPED BECAUSE CRITICAL ALERT*"));
#elif (UNIBET || _888SPORT || BETPLAY || RUSHBET)
                LogMng.Instance.onWriteStatus(string.Format("*BOT STOPPED BECAUSE LOGIN FAILED*"));
#endif

#if (!BET365_VM && !BET365_BM && !ET365_PL && !BET365_CHROMEDEV && !BET365_PUPPETEER && !BET365_ADDON)
                                LogMng.Instance.onWriteStatus(string.Format("************************************"));
                                OnStopCommand();
#endif
                            }
                            else if (result == PROCESS_RESULT.NO_LOGIN)
                            {
                                LogMng.Instance.onWriteStatus(string.Format("Failed because of logout"));
                                bookieController.login();
                            }
                        }
                    }
#if (UNIBET || _888SPORT || BETPLAY || BETANO || BETANO_CDP || STOIXIMAN || STOIXIMAN_CDP || NOVIBET || RUSHBET || SUPERBET)
                    Thread.Sleep(1000 * Setting.Instance.intervalDelay);

#elif (BET365_ADDON)
                    Thread.Sleep(1000 + new Random().Next(1, 2) * 1000);
#else
                    //Thread.Sleep(6000 + new Random().Next(1, 3) * 1000);
                    Thread.Sleep(100);
#endif
                    Monitor.Enter(_runthreadLocker);
                    curInfo.Clear();
                    Monitor.Exit(_runthreadLocker);
                }
                catch (Exception ex)
                {
                    LogMng.Instance.onWriteStatus($"betTask Exception: {ex}");
                }
            }
            LogMng.Instance.onWriteStatus($"betTask------- finisehd");
        }

        private BetburgerInfo GetRetryOneTimePickFromFailedHistory()
        {
            BetburgerInfo result = null;
            DateTime theFirstTime = DateTime.MaxValue;
            try
            {
                Monitor.Enter(_runthreadLocker);

                foreach (var failedInfo in _failedBetburgerInfo)
                {
                    if (failedInfo.betburgerInfo.kind == PickKind.Type_3 || failedInfo.betburgerInfo.kind == PickKind.Type_5 || failedInfo.betburgerInfo.kind == PickKind.Type_10)
                    {
                        if (failedInfo.FailedCount > Setting.Instance.failRetryCount)
                            continue;

                        if (DateTime.Now >= failedInfo.NextRetryTime)
                        {
                            if (theFirstTime > failedInfo.NextRetryTime)
                            {
                                result = failedInfo.betburgerInfo;
                                theFirstTime = failedInfo.NextRetryTime;
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                Monitor.Exit(_runthreadLocker);
            }
            return result;
        }

        private bool IsFailedWaiting(BetburgerInfo info)
        {
            try
            {
                Monitor.Enter(_runthreadLocker);

                foreach (var failedInfo in _failedBetburgerInfo)
                {
                    if (failedInfo.betburgerInfo.eventTitle == info.eventTitle && failedInfo.betburgerInfo.direct_link == info.direct_link && failedInfo.betburgerInfo.league == info.league && failedInfo.betburgerInfo.outcome == info.outcome)
                    {
                        if (failedInfo.FailedCount > Setting.Instance.failRetryCount)
                            return true;

                        if (DateTime.Now < failedInfo.NextRetryTime)
                            return true;
                    }
                }
            }
            catch { }
            finally
            {
                Monitor.Exit(_runthreadLocker);
            }
            return false;
        }

        private void RemoveFailedHistory(BetburgerInfo info)
        {
            try
            {

                Monitor.Enter(_runthreadLocker);

                for (int i = _failedBetburgerInfo.Count - 1; i >= 0; i--)
                {
                    if (_failedBetburgerInfo[i].betburgerInfo.eventTitle == info.eventTitle && _failedBetburgerInfo[i].betburgerInfo.direct_link == info.direct_link && _failedBetburgerInfo[i].betburgerInfo.league == info.league && _failedBetburgerInfo[i].betburgerInfo.outcome == info.outcome)
                    {
                        _failedBetburgerInfo.RemoveAt(i);
                        break;
                    }
                }
            }
            catch { }
            finally
            {
                Monitor.Exit(_runthreadLocker);
            }
        }
        private void AddFailedHistory(BetburgerInfo info, bool bCompletelyRemoved = false)
        {
            try
            {
                bool bAlreadyExist = false;

                Monitor.Enter(_runthreadLocker);

                foreach (var failedInfo in _failedBetburgerInfo)
                {
                    if (failedInfo.betburgerInfo.eventTitle == info.eventTitle && failedInfo.betburgerInfo.direct_link == info.direct_link && failedInfo.betburgerInfo.league == info.league && failedInfo.betburgerInfo.outcome == info.outcome)
                    {
                        bAlreadyExist = true;
                        failedInfo.FailedCount++;

                        if (info.kind == PickKind.Type_3 || info.kind == PickKind.Type_5 || info.kind == PickKind.Type_10)
                        {
                            failedInfo.NextRetryTime = DateTime.Now.AddSeconds(5);
                        }
                        else
                        {
#if (LOTTOMATICA || GOLDBET)
                            failedInfo.NextRetryTime = DateTime.Now.AddSeconds(5);
#else
                            failedInfo.NextRetryTime = DateTime.Now.AddSeconds(60 * failedInfo.FailedCount);
#endif
                        }



                        if (bCompletelyRemoved)
                        {
                            failedInfo.FailedCount = 100;
                            failedInfo.NextRetryTime = DateTime.MaxValue;
                        }
                        break;
                    }
                }

                if (!bAlreadyExist)
                {
                    FailedBetburgerInfo failedInfo = new FailedBetburgerInfo(info);
                    failedInfo.FailedCount = 1;
                    if (info.kind == PickKind.Type_3 || info.kind == PickKind.Type_5 || info.kind == PickKind.Type_10)
                    {
                        failedInfo.NextRetryTime = DateTime.Now.AddSeconds(5);
                    }
                    else
                    {
#if (LOTTOMATICA || GOLDBET)
                        failedInfo.NextRetryTime = DateTime.Now.AddSeconds(3);
#else
                        failedInfo.NextRetryTime = DateTime.Now.AddSeconds(180); //3 min
#endif


                    }


                    if (bCompletelyRemoved)
                    {
                        failedInfo.FailedCount = 100;
                        failedInfo.NextRetryTime = DateTime.MaxValue;
                    }
                    _failedBetburgerInfo.Add(failedInfo);
                }
            }
            catch { }
            finally
            {
                Monitor.Exit(_runthreadLocker);
            }
        }

        private void displayBetburger(List<BetburgerInfo> bbInfo)
        {
            try
            {
                LogMng.Instance.onWriteStatus(string.Format("Found {0} new arbs...", bbInfo.Count));

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {

                    ServerSureBetList.Clear();
                    foreach (BetburgerInfo info in bbInfo)
                    {
#if (BET365_VM || BET365_BM || BET365_PL || BET365_CHROMEDEV || BET365_PUPPETEER || BET365_ADDON)
                        if (info.bookmaker.ToLower() != "bet365")
                        continue;

                        //if (info.color == "yellow" || info.color == "red" || info.color == "green")
                        //    continue;
#elif (UNIBET || _888SPORT || BETPLAY || RUSHBET)
                    if (info.bookmaker.ToLower() != "unibet")
                        continue;

                    //only allow grey and blue
                    //if (info.color == "yellow" || info.color == "red" || info.color == "green")
                    //    continue;
#elif (PADDYPOWER)
                    if (info.bookmaker.ToLower() != "paddyPower")
                        continue;
#elif (SISAL)
                    if (info.bookmaker.ToLower() != "sisal")
                        continue;
#elif (ELYSGAME || DOMUSBET || BETALAND)
                    if (info.bookmaker.ToLower() != "telegramgtip" && info.bookmaker.ToLower() != "betaland")
                        continue;
#elif (EUROBET)
                    if (info.bookmaker.ToLower() != "eurobet")
                        continue;
#elif (SKYBET)
                    if (info.bookmaker.ToLower() != "skybet")
                            continue;
#elif (BWIN || SPORTINGBET)
                        if (info.bookmaker.ToLower() != "bwin")
                            continue;
//#elif (LOTTOMATICA)
//                    if (info.bookmaker.ToLower() != "lottomatica")
//                        continue; 
#elif (GOLDBET || LOTTOMATICA)
                    if (info.bookmaker.ToLower() != "goldbetshop")
                        continue;
#elif (LEOVEGAS)
                    if (info.bookmaker.ToLower() != "unibet")
                        continue;
#elif (SNAI)
                    if (info.bookmaker.ToLower() != "snai")
                        continue;
#elif (BETMGM)
                    if (info.bookmaker.ToLower() != "betmgm")
                        continue;
#elif (NOVIBET)
                    if (info.bookmaker.ToLower() != "novibet")
                        continue;
#elif (WINAMAX)
                    if (info.bookmaker.ToLower() != "winamax")
                        continue;
#elif (BETWAY || BETWAY_ADDON)
                    if (info.bookmaker.ToLower() != "betway")
                        continue;
#elif (SUPERBET)
                        if (info.bookmaker.ToLower() != "superbet")
                            continue;
#elif (STOIXIMAN || BETANO || STOIXIMAN_CDP || BETANO_CDP)
                    if (info.bookmaker.ToLower() != "stoiximan")
                        continue;
#elif (PINNACLE)
                    if (info.bookmaker.ToLower() != "pinnacle" && info.bookmaker.ToLower() != "pinnaclesports")
                        continue;
#elif (BETPREMIUM || SPORTPESA || DOMUSBET || CHANCEBET || REPLATZ || BETALAND)
                        if (info.bookmaker.ToLower() != "sportpesait")
                        continue;
#elif (BETFAIR_FETCH || BETFAIR || BETFAIR_PL || BETFAIR_NEW)
                        if (info.bookmaker.ToLower() != "bfsportsbook")
                        continue;
#elif (PLANETWIN)
                    if (info.bookmaker.ToLower() != "planetWin365")
                        continue;
#elif (TONYBET)
                        if (info.bookmaker.ToLower() != "tonybet")
                            continue;
#elif (PLAYPIX)
                        if (info.bookmaker.ToLower() != "playpix")
                            continue;
#elif (FORTUNA)
                        if (info.bookmaker.ToLower() != "fortuna")
                            continue;
#elif (KTO)
                        if (info.bookmaker.ToLower() != "kto" && info.bookmaker != "Goldenpalace")
                            continue;
#elif (WPLAY)
                        if (info.bookmaker.ToLower() != "wplay")
                            continue;
#elif (SEUBET)
                        if (info.bookmaker.ToLower() != "seubet")
                            continue;
#elif (BET365_QRAPI)
                        if (info.bookmaker.ToLower() != "bet365_qrapi")
                            continue;
#endif


                        Console.WriteLine(info.eventid);
                        ServerSureBetList.Add(info);
                    }
                }));
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus(string.Format("displayBetburger Exception {0} ", ex));
            }
        }

        private void SavePickToFile(BetburgerInfo info)
        {
            File.AppendAllText(@"missed.txt", "----------------------------" + Environment.NewLine);
            File.AppendAllText(@"missed.txt", "sport: " + info.sport + Environment.NewLine);
            File.AppendAllText(@"missed.txt", "leage: " + info.league + "   home: " + info.homeTeam + "   away: " + info.awayTeam + Environment.NewLine);
            File.AppendAllText(@"missed.txt", "extra: " + info.extra + Environment.NewLine);
            File.AppendAllText(@"missed.txt", "outcome: " + info.outcome + Environment.NewLine);
            File.AppendAllText(@"missed.txt", "odds: " + info.odds + Environment.NewLine);

        }

        List<BetburgerInfo> SaveList = new List<BetburgerInfo>();
        private void ArbReceived(List<BetburgerInfo> rawbetburgerInfoList)
        {
            List<BetburgerInfo> betburgerInfoList = new List<BetburgerInfo>();

            foreach (var itr in rawbetburgerInfoList)
            {
                //if (1 < Global.PackageID && Global.PackageID < 6)OnConnectedServer   //2,3,4: Horse
                //{
                //    if (itr.sport == "Horse Racing")
                //        betburgerInfoList.Add(itr);
                //}
                //else if (Global.PackageID == 1)
                {
                    betburgerInfoList.Add(itr);
                }
            }

            displayBetburger(betburgerInfoList);

            if (!Global.bRun)
                return;

            try
            {

                Monitor.Enter(_runthreadLocker);


                if (Setting.Instance.bDailyBetCountLimit)
                {
                    int nTotdayCount = BetHistory.Select(a => a.date.Date == DateTime.Now.Date).Count();
                    if (nTotdayCount >= Setting.Instance.nDailyBetCountLimit)
                    {
                        //LogMng.Instance.onWriteStatus("Ignore daily bet count...");
                        return;
                    }
                }

                if (Global.IsRestStatus)
                    return;


                for (int i = 0; i < betburgerInfoList.Count; i++)
                {
                    BetburgerInfo info = betburgerInfoList[i];

                    if (info.sport.ToLower() == "basket")
                        info.sport = "basketball";

#if (TROUBLESHOT)
                    //LogMng.Instance.onWriteStatus(string.Format("New pick kind {8} {0} - {1}:{2} [bookmaker : {3}] arb {4} odd {5} direct {6} arbid {7} period: {9} outcome: {10}", info.sport, info.homeTeam, info.awayTeam, info.bookmaker, info.percent, info.odds, info.direct_link, info.arbId, info.kind, info.period, info.outcome));
#endif
#if (BET365_VM || BET365_BM || BET365_PL || BET365_CHROMEDEV || BET365_PUPPETEER || BET365_ADDON)
                    
                    if (info.bookmaker.ToLower() != "bet365")
                        continue;

                    //if (info.color == "yellow" || info.color == "red" || info.color == "green")
                    //    continue;
#elif (UNIBET || _888SPORT || BETPLAY || RUSHBET)
                    if (info.bookmaker.ToLower() != "unibet")
                        continue;

                    //only allow grey and blue
                    //if (info.color == "yellow" || info.color == "red" || info.color == "green")
                    //    continue;
#elif (PADDYPOWER)
                    if (info.bookmaker.ToLower() != "paddyPower")
                        continue;
#elif (SISAL)
                    if (info.bookmaker.ToLower() != "sisal")
                        continue;
#elif (ELYSGAME || DOMUSBET || BETALAND)
                    if (info.bookmaker.ToLower() != "telegramgtip" && info.bookmaker.ToLower() != "betaland")
                        continue;
#elif (EUROBET)
                    if (info.bookmaker.ToLower() != "eurobet")
                        continue;
#elif (BWIN || SPORTINGBET)
                    if (info.bookmaker.ToLower() != "bwin")
                        continue; 
#elif (SKYBET)
                    if (info.bookmaker.ToLower() != "skybet")
                        continue; 
//#elif (LOTTOMATICA)
//                    if (info.bookmaker.ToLower() != "lottomatica")
//                        continue; 
#elif (GOLDBET || LOTTOMATICA)
                    if (info.bookmaker.ToLower() != "goldbetshop")
                        continue;
                    OpenBet_Goldbet openbet = Utils.ConvertBetburgerPick2OpenBet_Goldbet(info);
                    //if (openbet.isLive)
                    //    continue;
#elif (LEOVEGAS)
                    if (info.bookmaker.ToLower() != "unibet")
                        continue;
#elif (BETPREMIUM || SPORTPESA || DOMUSBET || CHANCEBET || REPLATZ || BETALAND)
                    if (info.bookmaker.ToLower() != "sportpesait")
                        continue;
#elif (BETFAIR_FETCH || BETFAIR || BETFAIR_PL || BETFAIR_NEW)
                    if (info.bookmaker.ToLower() != "bfsportsbook")
                    {
                        continue;
                    }
                    
#elif (SNAI)
                    if (info.bookmaker.ToLower() != "snai")
                        continue;
#elif (BETMGM)
                    if (info.bookmaker.ToLower() != "betmgm")
                        continue;
#elif (NOVIBET)
                    if (info.bookmaker.ToLower() != "novibet")
                        continue;
#elif (WINAMAX)
                    if (info.bookmaker.ToLower() != "winamax")
                        continue;
#elif (BETWAY || BETWAY_ADDON)
                    if (info.bookmaker.ToLower() != "betway")
                        continue;
#elif (SUPERBET)
                    if (info.bookmaker.ToLower() != "superbet")
                        continue;
#elif (STOIXIMAN || STOIXIMAN_CDP || BETANO || BETANO_CDP)
                    if (info.bookmaker.ToLower() != "stoiximan")
                        continue;
#elif (PINNACLE)
                    if (info.bookmaker.ToLower() != "pinnacle" && info.bookmaker.ToLower() != "pinnaclesports")
                        continue;
#elif (PLANETWIN)
                    if (info.bookmaker.ToLower() != "planetWin365")
                        continue;
#elif (TONYBET)
                    if (info.bookmaker.ToLower() != "tonybet")
                        continue;
#elif (PLAYPIX)
                    if (info.bookmaker.ToLower() != "vbet")
                        continue;
#elif (FORTUNA)
                    if (info.bookmaker.ToLower() != "fortuna")
                        continue;
#elif (KTO)
                    if (info.bookmaker.ToLower() != "kto" && info.bookmaker != "Goldenpalace")
                        continue;
#elif (WPLAY)
                    if (info.bookmaker.ToLower() != "wplay")
                        continue;
#elif (SEUBET)
                    if (info.bookmaker.ToLower() != "seubet")
                        continue;
#elif (BET365_QRAPI)
                    if (info.bookmaker.ToLower() != "bet365_qrapi")
                        continue;
#endif
#if (CHRISTIAN)
                    if (info.isLive)
                    {
                        if (!Setting.Instance.bValue1)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (!Setting.Instance.bValue2)
                        {
                            continue;
                        }
                    }
#endif
                    if (info.kind == PickKind.Type_1)
                    {
                        if (i % 2 == 0)
                            info.opbookmaker = betburgerInfoList[i + 1].bookmaker;
                        else
                            info.opbookmaker = betburgerInfoList[i - 1].bookmaker;
                    }

                    if (Setting.Instance.bAllowMajorLeaguesOnly)
                    {
                        if (info.sport.ToLower() == "soccer")
                        {
                            bool bIsMajorLeague = false;
                            foreach (string league in Constants.MajorLeagues)
                            {
                                if (league.ToLower().Trim() == info.league.ToLower().Trim())
                                {
                                    bIsMajorLeague = true;
                                    break;
                                }
                            }
                            if (!bIsMajorLeague)
                            {

                                //LogMng.Instance.onWriteStatus(string.Format("Ignore_Major Leages"));

                                continue;
                            }
                        }
                    }
                    if (info.kind == PickKind.Type_1 || info.kind == PickKind.Type_6 || info.kind == PickKind.Type_8 || info.kind == PickKind.Type_9)
                    {
                        if (_placePickInfo.Count > 0)
                            return;

                        if (!checkRemainTime(info))
                        {
#if (TROUBLESHOT)
                            //LogMng.Instance.onWriteStatus(string.Format("Ignore_Remaintime mismatched Processed {0} - {1}", info.started, DateTime.Now));
#endif
                            continue;
                        }

                        if (info.sport == "Horse Racing")
                        {

                            //if (!info.outcome.Contains("yes"))
                            //    continue;

                            if (Setting.Instance.bIgnoreUSArace)
                            {
                                if (info.awayTeam.Contains("North America"))
                                    continue;
                            }

                            if (Setting.Instance.bIgnoreUKrace)
                            {
                                if (info.awayTeam.Contains("UK "))
                                    continue;
                            }

                            if (Setting.Instance.bIgnoreAUrace)
                            {
                                if (info.awayTeam.Contains("Australia"))
                                    continue;
                            }


                            info.eventTitle = info.awayTeam + " " + info.started;

                            info.stake = Setting.Instance.stakeHorse;
                        }
                        else
                        {
#if (BET365_VM || BET365_BM || BET365_PL || BET365_CHROMEDEV || BET365_PUPPETEER || BET365_ADDON)
                            if (info.kind == PickKind.Type_1 && !Setting.Instance.bValue1)
                            {
                                continue;
                            }

                            if (info.kind == PickKind.Type_8 && !Setting.Instance.bValue3)
                            {
                                continue;
                            }

                            if (info.kind == PickKind.Type_9 && !Setting.Instance.bValue2)
                            {
                                continue;
                            }

                            if (info.kind == PickKind.Type_6 && !Setting.Instance.bValue2)
                            {
                                continue;
                            }
#endif
                            if (info.league.Contains("NCAAB"))
                                continue;

                            info.stake = Setting.Instance.stakeSports;
                            if (Setting.Instance.bStakePercentageMode)
                            {
                                if (Global.balance < 0)
                                {
                                    LogMng.Instance.onWriteStatus($"Percentage Stake mode {Setting.Instance.stakeSports}%, but Current Balance is incorrect");
                                    continue;
                                }

                                if (Setting.Instance.stakeSports < 0.1 || Setting.Instance.stakeSports > 7)
                                {
                                    LogMng.Instance.onWriteStatus("Stake setting is incorrect, please check! (0.1 - 7)");
                                    continue;
                                }

                                info.stake = Global.balance / 100 * Setting.Instance.stakeSports;
                                info.stake = Math.Round(info.stake);
                            }
                            else
                            {
                                if (Setting.Instance.bStakeSportsTo)
                                {
                                    //#if (BET365_VM || BET365_BM || BET365_PL || BET365_CHROMEDEV || BET365_PUPPETEER || BET365_ADDON)
                                    //                                info.stake = random.NextDouble() * (Setting.Instance.stakeSportsTo - Setting.Instance.stakeSports) + Setting.Instance.stakeSports;   
                                    //#else
                                    info.stake = random.Next((int)Setting.Instance.stakeSports, (int)Setting.Instance.stakeSportsTo);

#if (STOIXIMAN)
                                    info.stake = Setting.Instance.stakeSports;
#endif
                                    //#endif
                                }

                                if (Setting.Instance.bRangeStake)
                                {
                                    info.stake = 0;
                                    if (Setting.Instance.range1_start <= info.odds && info.odds <= Setting.Instance.range1_end)
                                    {
                                        //#if (BET365_VM || BET365_BM || BET365_PL || BET365_CHROMEDEV || BET365_PUPPETEER || BET365_ADDON)
                                        //                                info.stake = random.NextDouble() * (Setting.Instance.range1_stakeTo - Setting.Instance.range1_stake) + Setting.Instance.range1_stake;
                                        //#else
                                        info.stake = random.Next((int)Setting.Instance.range1_stake, (int)Setting.Instance.range1_stakeTo);
                                        //#endif
                                    }
                                    else if (Setting.Instance.range2_start <= info.odds && info.odds <= Setting.Instance.range2_end)
                                    {
                                        //#if (BET365_VM || BET365_BM || BET365_PL || BET365_CHROMEDEV || BET365_PUPPETEER || BET365_ADDON)
                                        //                                info.stake = random.NextDouble() * (Setting.Instance.range2_stakeTo - Setting.Instance.range2_stake) + Setting.Instance.range2_stake;
                                        //#else
                                        info.stake = random.Next((int)Setting.Instance.range2_stake, (int)Setting.Instance.range2_stakeTo);
                                        //#endif
                                    }
                                    else if (Setting.Instance.range3_start <= info.odds && info.odds <= Setting.Instance.range3_end)
                                    {
                                        //#if (BET365_VM || BET365_BM || BET365_PL || BET365_CHROMEDEV || BET365_PUPPETEER || BET365_ADDON)
                                        //                                info.stake = random.NextDouble() * (Setting.Instance.range3_stakeTo - Setting.Instance.range3_stake) + Setting.Instance.range3_stake;
                                        //#else
                                        info.stake = random.Next((int)Setting.Instance.range3_stake, (int)Setting.Instance.range3_stakeTo);
                                        //#endif
                                    }

                                    if (info.stake == 0)
                                        info.stake = Setting.Instance.range1_stake;
                                    if (info.stake == 0)
                                        info.stake = Setting.Instance.range2_stake;
                                    if (info.stake == 0)
                                        info.stake = Setting.Instance.range3_stake;
                                }
                            }
                        }

                        info.stake = Math.Round(info.stake, 2, MidpointRounding.ToEven);
                        if (info.stake == 0)
                        {
#if (TROUBLESHOT)
                            //LogMng.Instance.onWriteStatus(string.Format("Ignore_Stake mismatched Processed {0} - {1}:{2} [bookmaker : {3}] {4}", info.sport, info.homeTeam, info.awayTeam, info.extra, info.sport));
#endif
                            continue;
                        }

                        //switch (Global.PackageID)
                        //{
                        //    case 1: //all sports and no limitation                            
                        //        break;
                        //    case 2:
                        //        if (info.stake > 20)
                        //            info.stake = 20;
                        //        break;
                        //    case 3:
                        //        if (info.stake > 5)
                        //            info.stake = 5;
                        //        break;
                        //    case 4:
                        //        if (info.stake > 3)
                        //            info.stake = 3;
                        //        break;
                        //    case 5:
                        //        if (info.stake > 1.5)
                        //            info.stake = 1.5;
                        //        break;
                        //    default:
                        //        continue;
                        //}


                        //check percent every bookie
#if (DOMUSBET || BETALAND)
                        if (!checkSports(info, true))
#else
                        if (!checkSports(info))
#endif
                        {
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus(string.Format("Ignore_Sports mismatched Processed {0} - {1}:{2} [bookmaker : {3}] {4}", info.sport, info.homeTeam, info.awayTeam, info.extra, info.sport));
#endif
                            continue;
                        }
#if (STOIXIMAN || STOIXIMAN_CDP || BETPLAY || NOVIBET || BETANO || BETANO_CDP || SUPERBET || WPLAY || SEUBET || BET365_QRAPI)
                        if (Setting.Instance.bEnableMaxPendingBets)
                        {
                            int nCurrentPendingbets = 0;
                            if (bookieController != null)
                            {
                                nCurrentPendingbets = bookieController.GetPendingbets();
                            }
                            if (nCurrentPendingbets != 0 && Setting.Instance.nMaxPendingBetsLimit <= nCurrentPendingbets)
                            {
                                LogMng.Instance.onWriteStatus($"ignore bet because of max pending bet limit: {Setting.Instance.nMaxPendingBetsLimit} current: {nCurrentPendingbets}");
                                continue;
                            }
                        }

                        if (info.sport.ToLower() == "soccer")
                        {
                            if (Setting.Instance.bSoccerEnableFirstHalfbets)
                            {
                                if (info.period == "1 time")
                                {

                                }
                                else
                                {
                                    LogMng.Instance.onWriteStatus("ignore bet because of 1 time.");
                                    continue;
                                }
                            }
                            if (Setting.Instance.bSoccerEnableSecondHalfbets)
                            {
                                if (info.period == "2 time")
                                {

                                }
                                else
                                {
                                    LogMng.Instance.onWriteStatus("ignore bet because of 2 time.");
                                    continue;
                                }
                            }

                            if (info.outcome.ToLower() == "team1 win" ||
                                info.outcome.ToLower() == "team2 win" ||
                                info.outcome.ToLower() == "1" ||
                                info.outcome.ToLower() == "2" ||
                                info.outcome.ToLower() == "12" ||
                                info.outcome.ToLower() == "1x" ||
                                info.outcome.ToLower() == "x2")
                            {
                                /*if (!Setting.Instance.bSoccerMoneyline)
                                {
                                    continue;
                                }              */
                            }
                            /*else if (info.outcome.ToLower().Contains("corners"))
                            {
                                if (!Setting.Instance.bSoccerCorners) 
                                {
                                    continue;
                                }
                            }
                            else if (info.outcome.ToLower().Contains("cards"))
                            {
                                if (!Setting.Instance.bSoccerCards) 
                                {
                                    continue;
                                }
                            }*/
                            else
                            {
                                if ((Setting.Instance.bSoccerMoneyline &&
                                    Setting.Instance.bSoccerHandicap &&
                                    Setting.Instance.bSoccerTotals &&
                                    Setting.Instance.bSoccerCorners &&
                                    Setting.Instance.bSoccerCards) ||
                                    (!Setting.Instance.bSoccerMoneyline &&
                                    !Setting.Instance.bSoccerHandicap &&
                                    !Setting.Instance.bSoccerTotals &&
                                    !Setting.Instance.bSoccerCorners &&
                                    !Setting.Instance.bSoccerCards))
                                {

                                }
                                else
                                {
                                    if (info.outcome.ToLower().StartsWith("ah") || info.outcome.ToLower().StartsWith("eh") ||
                                        info.outcome.ToLower().StartsWith("asian handicap") || info.outcome.ToLower().StartsWith("european handicap"))
                                    {
                                        if (!Setting.Instance.bSoccerHandicap)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (info.outcome.ToLower().StartsWith("to(") || info.outcome.ToLower().StartsWith("tu(") || info.outcome.ToLower().StartsWith("total"))
                                    {
                                        if (!Setting.Instance.bSoccerTotals)
                                        {
                                            continue;
                                        }
                                    }
                                    continue;
                                }
                            }
                        }
#endif
                        if (info.kind != PickKind.Type_6) //tradematesports is doesn't need to check percentage
                        {
                            if (!checkPercent(info))
                            {
#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus(string.Format("Ignore_Arb mismatched Processed {0} - {1}:{2} [directlink : {3}] {4}:{5}", info.sport, info.homeTeam, info.awayTeam, info.direct_link, info.percent, Setting.Instance.percentageSports));
#endif
                                continue;
                            }
                        }

                        //check odds range
                        if (!checkOdds(info))
                        {
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus(string.Format("Ignore_Odd mismatched Processed {0} - {1}:{2} [bookmaker : {3}] {4}:{5}-{6}", info.sport, info.homeTeam, info.awayTeam, info.extra, info.odds, Setting.Instance.minOddsSports, Setting.Instance.maxOddsSports));
#endif
                            continue;
                        }

#if (GOLDBET || LOTTOMATICA)
                        if (info.outcome.ToLower().Contains("u") || info.outcome.ToLower().Contains("one scoreless"))
                        {
                            continue;
                        }
#endif
                    }
                    else if (info.kind == PickKind.Type_2)
                    {
                        if (info.sport.ToLower() != "horse racing")
                            continue;
#if (!HORSE)
                        continue;
#endif
                        info.eventTitle = info.league + " " + info.started;
                        //if (_placePickInfo.Count > 0)
                        //    return;
#if (VIP)
                        if (!Setting.Instance.bHorseRacing)
                        {
                            continue;
                        }

                        info.stake = Setting.Instance.stakeHorse;
#else
                        continue;
#endif
                    }
                    else if (info.kind == PickKind.Type_6 || info.kind == PickKind.Type_13)
                    {
                        if (_placePickInfo.Count > 0)
                            return;
#if (VIP)
                        if (!Setting.Instance.bValue2)
                        {
                            continue;
                        }


                        if (!checkOdds(info))
                        {
#if (TROUBLESHOT)
                            //LogMng.Instance.onWriteStatus(string.Format("Ignore_Odd mismatched Processed {0} - {1}:{2} [bookmaker : {3}] {4}:{5}-{6}", info.sport, info.homeTeam, info.awayTeam, info.extra, info.odds, Setting.Instance.minOddsSports, Setting.Instance.maxOddsSports));
#endif
                            continue;
                        }

                        if (!checkSports(info, true))
                        {
#if (TROUBLESHOT)
                            //LogMng.Instance.onWriteStatus(string.Format("Ignore_Sports mismatched Processed {0} - {1}:{2} [bookmaker : {3}] {4}", info.sport, info.homeTeam, info.awayTeam, info.extra, info.sport));
#endif
                            continue;
                        }

                        if (!checkPercent(info))
                        {
#if (TROUBLESHOT)
                            //LogMng.Instance.onWriteStatus(string.Format("Ignore_Arb mismatched Processed {0} - {1}:{2} [bookmaker : {3}] {4}:{5}", info.sport, info.homeTeam, info.awayTeam, info.extra, info.percent, Setting.Instance.percentageSports));
#endif
                            continue;
                        }

                        if (Setting.Instance.bSportsLessTime && Setting.Instance.nSportsLessTimeMinute > 0)
                        {
                            try
                            {
                                DateTime curTime = DateTime.Now.ToUniversalTime();
                                DateTime startTime = DateTime.Now.AddDays(1);
                                try
                                {
                                    startTime = DateTime.ParseExact(info.started, "dd.MM.yyyy HH:mm", null);
                                }
                                catch
                                {
                                    //LogMng.Instance.onWriteStatus("Parsing date exception try second");
                                    startTime = Convert.ToDateTime(info.started);  //time is based UTC(-1)
                                }


                                int nTotalMinutes = (int)startTime.Subtract(curTime).TotalMinutes;
                                int nSettingMinutes = Setting.Instance.nSportsLessTimeMinute;

#if (TROUBLESHOT)
                                LogMng.Instance.onWriteStatus(string.Format("Cur {0} Start {1} total {2} seting {3}", curTime, startTime, nTotalMinutes, nSettingMinutes));
#endif
                                if (nTotalMinutes > nSettingMinutes)
                                    continue;
                            }
                            catch { }
                        }

                        info.stake = Setting.Instance.stakeSports;
                        if (Setting.Instance.bStakeSportsTo)
                        {
                            info.stake = random.NextDouble() * (Setting.Instance.stakeSportsTo - Setting.Instance.stakeSports) + Setting.Instance.stakeSports;
                        }



                        if (Setting.Instance.bRangeStake)
                        {
                            info.stake = 0;
                            if (Setting.Instance.range1_start <= info.odds && info.odds <= Setting.Instance.range1_end)
                                info.stake = random.NextDouble() * (Setting.Instance.range1_stakeTo - Setting.Instance.range1_stake) + Setting.Instance.range1_stake;
                            else if (Setting.Instance.range2_start <= info.odds && info.odds <= Setting.Instance.range2_end)
                                info.stake = random.NextDouble() * (Setting.Instance.range2_stakeTo - Setting.Instance.range2_stake) + Setting.Instance.range2_stake;
                            else if (Setting.Instance.range3_start <= info.odds && info.odds <= Setting.Instance.range3_end)
                                info.stake = random.NextDouble() * (Setting.Instance.range3_stakeTo - Setting.Instance.range3_stake) + Setting.Instance.range3_stake;

                            if (info.stake == 0)
                                info.stake = Setting.Instance.range1_stake;
                            if (info.stake == 0)
                                info.stake = Setting.Instance.range2_stake;
                            if (info.stake == 0)
                                info.stake = Setting.Instance.range3_stake;
                        }

                        info.stake = Math.Round(info.stake, 2, MidpointRounding.ToEven);
                        if (info.stake == 0)
                            continue;
#else
                        continue;
#endif
                    }
                    else if (info.kind == PickKind.Type_12)
                    {
                        if (info.sport.Contains("soccer"))
                            info.sport = "soccer";
#if (VIP)
                        if (!Setting.Instance.bTipster2)
                        {
                            continue;
                        }

                        if (Setting.Instance.percentageStakeModeTipster2)
                        {
                            if (Global.balance < 0)
                            {
                                //LogMng.Instance.onWriteStatus($"[Ignorebet]Percentage Stake mode {Setting.Instance.stakePerTipster2}%, but Current Balance is incorrect");
                                continue;
                            }
                            info.stake = Global.TotalBalance / 100 * Setting.Instance.stakePerTipster2;
                        }
                        else
                        {
                            info.stake = Setting.Instance.stakePerTipster2;
                        }

                        info.stake = Math.Round(info.stake);
#else
                        continue;
#endif
                    }
                    else if (info.kind == PickKind.Type_3 || info.kind == PickKind.Type_4 || info.kind == PickKind.Type_5 || info.kind == PickKind.Type_7)
                    {
                        if (info.kind == PickKind.Type_7)
                        {//skip type_7 because same picks comes frequetly.
                            if (_placePickInfo.Count > 0)
                                return;
                        }
#if (VIP)
                        //LogMng.Instance.onWriteStatus(string.Format("New pick kind {8} {0} - {1}:{2} [bookmaker : {3}] arb {4} odd {5} direct {6} arbid {7} ", info.sport, info.homeTeam, info.awayTeam, info.bookmaker, info.percent, info.odds, info.direct_link, info.arbId, info.kind));
                        //LogMng.Instance.onWriteStatus($"Check pick-- Extra: {info.outcome.ToLower()}");
                        //if (info.outcome.ToLower().Contains("corner"))
                        //{
                        //    LogMng.Instance.onWriteStatus("Ignore this pick because of corner");
                        //    continue;
                        //}

                        if (!Setting.Instance.bSoccerLive)
                        {
                            continue;
                        }

                        if (info.bookmaker != "bfsportsbook")
                        {
                            if (info.odds > Setting.Instance.maxOddsSoccerLive || info.odds < Setting.Instance.minOddsSoccerLive)
                            {
                                //LogMng.Instance.onWriteStatus("[Ignorebet]odd is out of range, ");
                                continue;
                            }
                        }

                        if (info.bookmaker.ToLower() == "bet365")
                        {
                            if (info.outcome.ToLower().Contains("corner"))
                                continue;
                        }

                        if (Global.TotalBalance <= 0)
                        {
                            Global.TotalBalance = Global.balance;
                            if (Global.TotalBalance <= 0)
                            {
                                //LogMng.Instance.onWriteStatus($"[Ignorebet]Percentage Stake mode {Setting.Instance.stakeSoccerLive}%, but Current Balance is incorrect");
                                continue;
                            }
                        }
                        if (Setting.Instance.percentageStakeModeSoccerLive)
                        {
                            info.stake = Global.TotalBalance / 100 * Setting.Instance.stakeSoccerLive;
                        }
                        else
                        {
                            info.stake = Setting.Instance.stakeSoccerLive;
                        }

                        info.stake = Math.Round(info.stake);


#else
                        continue;
#endif
                    }
                    else if (info.kind == PickKind.Type_11)
                    {


                        info.stake = Setting.Instance.stakeSports;
                        if (Setting.Instance.bStakePercentageMode)
                        {
                            if (Global.balance < 0)
                            {
                                LogMng.Instance.onWriteStatus($"Percentage Stake mode {Setting.Instance.stakeSports}%, but Current Balance is incorrect");
                                continue;
                            }

                            if (Setting.Instance.stakeSports < 0.1 || Setting.Instance.stakeSports > 7)
                            {
                                LogMng.Instance.onWriteStatus("Stake setting is incorrect, please check! (0.1 - 7)");
                                continue;
                            }

                            info.stake = Global.balance / 100 * Setting.Instance.stakeSports;
                            info.stake = Math.Round(info.stake);
                        }
                        else
                        {
                            if (Setting.Instance.bStakeSportsTo)
                            {
                                info.stake = random.Next((int)Setting.Instance.stakeSports, (int)Setting.Instance.stakeSportsTo);

                            }

                            if (Setting.Instance.bRangeStake)
                            {
                                info.stake = 0;
                                if (Setting.Instance.range1_start <= info.odds && info.odds <= Setting.Instance.range1_end)
                                {
                                    //#if (BET365_VM || BET365_BM || BET365_PL || BET365_CHROMEDEV || BET365_PUPPETEER || BET365_ADDON)
                                    //                                info.stake = random.NextDouble() * (Setting.Instance.range1_stakeTo - Setting.Instance.range1_stake) + Setting.Instance.range1_stake;
                                    //#else
                                    info.stake = random.Next((int)Setting.Instance.range1_stake, (int)Setting.Instance.range1_stakeTo);
                                    //#endif
                                }
                                else if (Setting.Instance.range2_start <= info.odds && info.odds <= Setting.Instance.range2_end)
                                {
                                    //#if (BET365_VM || BET365_BM || BET365_PL || BET365_CHROMEDEV || BET365_PUPPETEER || BET365_ADDON)
                                    //                                info.stake = random.NextDouble() * (Setting.Instance.range2_stakeTo - Setting.Instance.range2_stake) + Setting.Instance.range2_stake;
                                    //#else
                                    info.stake = random.Next((int)Setting.Instance.range2_stake, (int)Setting.Instance.range2_stakeTo);
                                    //#endif
                                }
                                else if (Setting.Instance.range3_start <= info.odds && info.odds <= Setting.Instance.range3_end)
                                {
                                    //#if (BET365_VM || BET365_BM || BET365_PL || BET365_CHROMEDEV || BET365_PUPPETEER || BET365_ADDON)
                                    //                                info.stake = random.NextDouble() * (Setting.Instance.range3_stakeTo - Setting.Instance.range3_stake) + Setting.Instance.range3_stake;
                                    //#else
                                    info.stake = random.Next((int)Setting.Instance.range3_stake, (int)Setting.Instance.range3_stakeTo);
                                    //#endif
                                }

                                if (info.stake == 0)
                                    info.stake = Setting.Instance.range1_stake;
                                if (info.stake == 0)
                                    info.stake = Setting.Instance.range2_stake;
                                if (info.stake == 0)
                                    info.stake = Setting.Instance.range3_stake;
                            }
                        }


                        info.stake = Math.Round(info.stake, 2, MidpointRounding.ToEven);
                        if (info.stake == 0)
                        {
#if (TROUBLESHOT)
                            //LogMng.Instance.onWriteStatus(string.Format("Ignore_Stake mismatched Processed {0} - {1}:{2} [bookmaker : {3}] {4}", info.sport, info.homeTeam, info.awayTeam, info.extra, info.sport));
#endif
                            continue;
                        }



                        //check percent every bookie
                        if (!checkSports(info))
                        {
#if (TROUBLESHOT)
                            //LogMng.Instance.onWriteStatus(string.Format("Ignore_Sports mismatched Processed {0} - {1}:{2} [bookmaker : {3}] {4}", info.sport, info.homeTeam, info.awayTeam, info.extra, info.sport));
#endif
                            continue;
                        }
                    } 
                    else if (info.kind == PickKind.Type_14)
                    {

                    }
                    else
                    {
                        continue;
                    }

#if (BET365_ADDON)
                    if (info.stake < 1)
                        info.stake = 1;

                    if (info.stake > 99)
                        info.stake = info.stake / 10 * 10;
                    else if (info.stake > 999)
                        info.stake = info.stake / 100 * 100;
#endif

                    if (info.kind != PickKind.Type_5 && info.kind != PickKind.Type_10 && info.kind != PickKind.Type_11)
                    {
                        if (AlreadyPlacedWithSameDirectLinkOutCome(info))
                        {
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"{info.eventTitle} Ignoring because of AlreadyPlacedWithSameDirectLink!");
#endif
                            continue;
                        }

                        if (CheckMaxBetsInEvent(info))
                        {
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"{info.eventTitle} Ignoring because of CheckMaxbetsInEvent!");
#endif
                            continue;
                        }
                        //because of doulbe bets, we have to check failed bet retrying here, for only prematch which picks are comming with not once mode.
                        if (IsFailedWaiting(info))
                        {
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"{info.eventTitle} ignoring because of IsFailedWaiting!");
#endif
                            continue;
                        }
                    }

#if (BET365_ADDON)
                    //to ignore no direct_link bets(failed parsing in betsapi)
                    if (info.kind != PickKind.Type_10 && info.kind != PickKind.Type_2 && info.kind != PickKind.Type_6 && info.kind != PickKind.Type_12 && info.kind != PickKind.Type_13)
                    {
                        OpenBet_Bet365 openbet = Utils.ConvertBetburgerPick2OpenBet_365(info);
                        if (openbet == null)
                        {

#if (TROUBLESHOT)
                                            LogMng.Instance.onWriteStatus($"Ignoring because of directlink is wrong");
#endif
                            continue;
                        }
                    }
#endif
#if (TROUBLESHOT)
                    LogMng.Instance.onWriteStatus("NewPick refined *******");
#endif


#if (BET365_ADDON || LOTTOMATICA)
                    if (Setting.Instance.bPlaceDouleValues)
                    {
                    
                        if (_placedoublePickInfo.Count == 1)
                        {
                            if (_placedoublePickInfo[0].eventTitle == info.eventTitle)
                                continue;
                        }

                        bool bAlreadyAdded = false;
                        foreach (var candidateInfoList in _placePickInfo)
                        {
                            foreach (var candidateInfo in candidateInfoList)
                            {
                                if (candidateInfo.eventTitle == info.eventTitle && candidateInfo.direct_link == info.direct_link && candidateInfo.league == info.league && candidateInfo.outcome == info.outcome)
                                {
                                    bAlreadyAdded = true;
                                    break;
                                }
                            }
                        }
                        foreach (var candidateInfo in _placedoublePickInfo)
                        {
                            if (candidateInfo.eventTitle == info.eventTitle && candidateInfo.direct_link == info.direct_link && candidateInfo.league == info.league && candidateInfo.outcome == info.outcome)
                            {
                                bAlreadyAdded = true;
                                break;
                            }
                        }

                        foreach (var candidateInfo in curInfo)
                        {
                            if (candidateInfo.eventTitle == info.eventTitle && candidateInfo.direct_link == info.direct_link && candidateInfo.league == info.league && candidateInfo.outcome == info.outcome)
                            {
                                bAlreadyAdded = true;
                                break;
                            }
                        }
                        if (bAlreadyAdded)
                            continue;

                        _placedoublePickInfo.Add(info);
#if (TROUBLESHOT)
                        LogMng.Instance.onWriteStatus($"Pushed in Stack {info.kind} {info.sport} - {info.homeTeam}:{info.awayTeam} [bookmaker : {info.extra}] {info.eventTitle} {info.outcome} {info.direct_link} {info.siteUrl} {info.eventUrl} tempStackSize: {_placedoublePickInfo.Count}");
#endif

                        if (_placedoublePickInfo.Count >= 2)
                        {
#if (TROUBLESHOT)
                            LogMng.Instance.onWriteStatus($"Pass to PlaceBet Thread TempStackCount: {_placedoublePickInfo.Count}");
#endif
                                                        
                            _placePickInfo.Add(_placedoublePickInfo);
                            _placedoublePickInfo = new List<BetburgerInfo>();

                            break;
                        }                        
                    }
                    else
                    {
                        bool bAlreadyAdded = false;
                        foreach (var candidateInfoList in _placePickInfo)
                        {
                            foreach (var candidateInfo in candidateInfoList)
                            {
                                if (candidateInfo.eventTitle == info.eventTitle && candidateInfo.direct_link == info.direct_link && candidateInfo.league == info.league && candidateInfo.outcome == info.outcome)
                                {
                                    bAlreadyAdded = true;
                                    break;
                                }
                            }
                        }
                        foreach (var candidateInfo in curInfo)
                        {
                            if (candidateInfo.eventTitle == info.eventTitle && candidateInfo.direct_link == info.direct_link && candidateInfo.league == info.league && candidateInfo.outcome == info.outcome)
                            {
                                bAlreadyAdded = true;
                                break;
                            }
                        }
                        if (bAlreadyAdded)
                            continue;
                        _placePickInfo.Add(new List<BetburgerInfo>{ info });
                        
                    }
#else
                    bool bAlreadyAdded = false;
                    foreach (var candidateInfoList in _placePickInfo)
                    {
                        foreach (var candidateInfo in candidateInfoList)
                        {
                            if (candidateInfo.eventTitle == info.eventTitle && candidateInfo.direct_link == info.direct_link && candidateInfo.league == info.league && candidateInfo.outcome == info.outcome)
                            {
                                bAlreadyAdded = true;
                                break;
                            }
                        }
                    }
                    foreach (var candidateInfo in curInfo)
                    {
                        if (candidateInfo.eventTitle == info.eventTitle && candidateInfo.direct_link == info.direct_link && candidateInfo.league == info.league && candidateInfo.outcome == info.outcome)
                        {
                            bAlreadyAdded = true;
                            break;
                        }
                    }
                    if (bAlreadyAdded)
                    {
                        LogMng.Instance.onWriteStatus($"{info.eventTitle} Already Added...");
                        continue;
                    }

                    _placePickInfo.Add(new List<BetburgerInfo> { info });
                    break;
#endif
                }
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus("[Exception : ArbReceived] " + e.ToString());
            }
            finally
            {
                Monitor.Exit(_runthreadLocker);
            }
        }
        private Thread EurobetReconnectThread = null;
        private Thread RefreshThread = null;
#if (ELYSGAME || EUROBET || LOTTOMATICA || LEOVEGAS || SNAI || PLANETWIN || BETPREMIUM || SPORTPESA || REPLATZ || CHANCEBET || DOMUSBET || BETALAND || BETFAIR_FETCH || BETFAIR || BETFAIR_PL || SISAL || BWIN || SPORTINGBET || NOVIBET || BETWAY || WINAMAX || GOLDBET || STOIXIMAN || BETANO || STOIXIMAN_CDP || BETANO_CDP || PINNACLE || BETFAIR_NEW || SUPERBET || PLAYPIX || KTO || WPLAY || SEUBET || BET365_QRAPI)

        private void RefreshFunc()
        {
            Thread.Sleep(60 * 1000);
            while (true)
            {
                int nSleepMinute = 10 * 60;
                Thread.Sleep(nSleepMinute * 1000);

                bool bIsInBetting = false;
                Monitor.Enter(_runthreadLocker);
                if (_placePickInfo.Count > 0 || curInfo.Count > 0)
                    bIsInBetting = true;
                Monitor.Exit(_runthreadLocker);

                if (bIsInBetting)
                    continue;

                if (CDPController.Instance._browserObj != null)
                    CDPController.Instance.ReloadBrowser();
            }
        }
        private void EurobetReconnectRun()
        {
            int nTotalCounter = 0;

            Thread.Sleep(60 * 1000);

            while (true)
            {
                nTotalCounter++;
                int nSleepMinute = 5 * 60;
#if (EUROBET)
    nSleepMinute = 1 * 60;
#elif (BETFAIR_FETCH || BETFAIR || BETFAIR_PL || BETFAIR_NEW)
                nSleepMinute = 4 * 60;
#elif (NOVIBET)
    nSleepMinute = 2 * 60;
#elif (SNAI || ELYSGAME)
    nSleepMinute = 200 * 60;
#elif (LEOVEGAS || LOTTOMATICA || SISAL || GOLDBET || STOIXIMAN || BETANO || BETANO_CDP || STOIXIMAN_CDP)
                nSleepMinute = 10 * 60;
#elif (PLANETWIN)
    nSleepMinute = 95 * 60;
#elif (SUPERBET)
    nSleepMinute = 15 * 60;
#elif (BETPREMIUM || SPORTPESA || REPLATZ || CHANCEBET)
                nSleepMinute = 10 * 60;
#elif (DOMUSBET || BETALAND || PINNACLE)
                nSleepMinute = 4 * 60;
#elif (PLAYPIX)
                nSleepMinute = 3 * 60;
#elif (WINAMAX)
                nSleepMinute = 30 * 60;
#endif
                Thread.Sleep(nSleepMinute * 1000);

                if (bookieController == null)
                    continue;

                bool bIsInBetting = false;
                Monitor.Enter(_runthreadLocker);
                if (_placePickInfo.Count > 0 || curInfo.Count > 0)
                    bIsInBetting = true;
                Monitor.Exit(_runthreadLocker);

                if (bIsInBetting)
                    continue;

#if (BETFAIR || BETFAIR_PL || BETFAIR_FETCH)
                bookieController.Feature();
                if (nTotalCounter < 19)
                    continue;
                nTotalCounter = 0;
#endif        
                if (!bookieController.Pulse())
                {
                    bool logResult = bookieController.login();
                    if (logResult)
                    {
                        Global.balance = bookieController.getBalance();
                        UserMng.GetInstance().SendClientBalance(Global.balance);

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
#if (BET365_ADDON)
            try
            {
                if (Global.socketServer.WebSocketServices.SessionCount <= 0)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
#if (CHROME)
                        MessageBox.Show("Load extension in Chrome");
#elif (EDGE)
                        MessageBox.Show("Load extension in Edge");
#elif (FIREFOX)
                        MessageBox.Show("Load extension in Firefox");
#endif
                    }));
                    return;
                }
                
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus(ex.Message);                
            }
#endif
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
                AccountId = Setting.Instance.username;
            }));

            if (Global.bRun)
                return;

            if (!canStart())
                return;

            LogMng.Instance.onWriteStatus("Bot Started");

            IsStarted = true;
            IsStopped = false;
            Global.bRun = true;

            //bookieController = new WinamaxCtrl();
            //bookieController.login();

            if (UserMng.GetInstance().onArbReceived == null)
                UserMng.GetInstance().onArbReceived = ArbReceived;

            if (UserMng.GetInstance().connectedServer == null)
                UserMng.GetInstance().connectedServer = OnConnectedServer;
            //OnConnectedServer();           


            _failedBetburgerInfo.Clear();

            threadReconnect = new Thread(new ParameterizedThreadStart( ReconnectThread));
            threadReconnect.IsBackground = true;
            threadReconnect.Start(0);
            OnConnectedServer();

        }


        bool bIsRunningConnectedServer = false;
        public void OnConnectedServer()
        {

            if (bIsRunningConnectedServer == true)
                return;

            bIsRunningConnectedServer = true;
            LogMng.Instance.onWriteStatus("Connected Bot Server Successfully!");
#if (BET365)
            //bookieController = new Bet365Ctrl();
#elif (BET365_VM)
            if (bookieController == null)
                bookieController = new Bet365_VMCtrl(); 
#elif (BET365_BM)
            if (bookieController == null)
                bookieController = new Bet365_BMCtrl(); 
#elif (BET365_PL)
            if (bookieController == null)
                bookieController = new BET365_PLCtrl();
#elif (BET365_ADDON)
            if (bookieController == null)
                bookieController = new Bet365_ADDONCtrl();
            
#elif (BET365_PUPPETEER)
            if (bookieController == null)
                bookieController = new BET365_PUPPETEERCtrl();
#elif (BET365_CHROMEDEV)
            if (bookieController == null)
                bookieController = new BET365_CHROMEDEVCtrl();
#elif (UNIBET)
            if (bookieController == null)
                bookieController = new UnibetCtrl();
#elif (_888SPORT)
            if (bookieController == null)
                bookieController = new _888SportCtrl();
#elif (BETPLAY)
            if (bookieController == null)
                bookieController = new BetplayCDPCtrl();
#elif (RUSHBET)
            if (bookieController == null)
                bookieController = new RushbetCtrl();
#elif (PADDYPOWER)
            if (bookieController == null)
                bookieController = new PaddyPowerCtrl();
#elif (LEOVEGAS)
            if (bookieController == null)
                bookieController = new LeovegasCtrl();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
#elif (ELYSGAME)
            if (bookieController == null)
                bookieController = new ElysgameCtrl();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
#elif (EUROBET)
            if (bookieController == null)
                bookieController = new EurobetCtrl();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
#elif (SISAL)
            if (bookieController == null)
                bookieController = new SisalCtrl();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
        }

#elif (SKYBET)
            if (bookieController == null)
                bookieController = new SkybetCtrl();
#elif (FANDUEL)
            if (bookieController == null)
                bookieController = new FanduelCtrl();
#elif (PLANETWIN)
            if (bookieController == null)
                bookieController = new PlanetwinCtrl();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
#elif (BETFAIR_FETCH || BETFAIR || BETFAIR_PL)
            if (bookieController == null)
                bookieController = new BetfairCtrl();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
#elif (BETFAIR_NEW)
            if (bookieController == null)
                bookieController = new PaddyPowerCtrl();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
#elif (BWIN || SPORTINGBET)
            if (bookieController == null)
                bookieController = new BwinCtrl();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
#elif (SNAI)
            if (bookieController == null)
                bookieController = new SnaiCtrl();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
//#elif (LOTTOMATICA)
//            if (bookieController == null)
//                bookieController = new LottomaticaCtrl();
//            if (EurobetReconnectThread == null)
//            {
//                EurobetReconnectThread = new Thread(EurobetReconnectRun);
//                EurobetReconnectThread.Start();
//            }
#elif (BETMGM)
            if (bookieController == null)
                bookieController = new BetMGMCtrl();
#elif (NOVIBET)
            if (bookieController == null)
                bookieController = new NovibetCtrl();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
#elif (BETWAY_ADDON)
            if (bookieController == null)
                bookieController = new Betway_ADDONCtrl();
#elif (BETWAY)
            if (bookieController == null)
                bookieController = new BetwayCtrl();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
#elif (WINAMAX)
            if (bookieController == null)
                bookieController = new Winamax_CDP();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
#elif (PINNACLE)
            if (bookieController == null)
                bookieController = new PinnacleCtrl();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
#elif (SUPERBET)
            if (bookieController == null)
                bookieController = new SuperbetCtrl();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
            if(RefreshThread == null)
            {
                RefreshThread = new Thread(RefreshFunc);
                RefreshThread.Start();
            }
#elif (STOIXIMAN_CDP || BETANO_CDP)
            if (bookieController == null)
                bookieController = new Stoiximan_CDP();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
            if(RefreshThread == null)
            {
                RefreshThread = new Thread(RefreshFunc);
                RefreshThread.Start();
            }
#elif (STOIXIMAN || BETANO)
            if (bookieController == null)
                bookieController = new StoiximanCtrl();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
            //#elif (BETANO)
            //            if (bookieController == null)
            //                bookieController = new BetanoCtrl();
#elif (BETPREMIUM)
            if (bookieController == null)
                bookieController = new BetpremiumCtrl();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
#elif (DOMUSBET || BETALAND)
            if (bookieController == null)
                bookieController = new DomusbetCtrl();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
#elif (SPORTPESA || CHANCEBET || REPLATZ)
            if (bookieController == null)
                bookieController = new SportpesaCtrl();
            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
#elif (TONYBET)
            if (bookieController == null)
                bookieController = new TonybetCtrl();

#elif (GOLDBET || LOTTOMATICA)
            if (bookieController == null)
                bookieController = new GoldbetCtrl();

            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
#elif (PLAYPIX)
            if (bookieController == null)
                bookieController = new Playpix_CDP();

            if (EurobetReconnectThread == null)
            {
                EurobetReconnectThread = new Thread(EurobetReconnectRun);
                EurobetReconnectThread.Start();
            }
#elif (FORTUNA)
            if (bookieController == null)
                bookieController = new Fortuna();

            if (EurobetReconnectThread == null)
            {
                //EurobetReconnectThread = new Thread(EurobetReconnectRun);
                //EurobetReconnectThread.Start();
            }
#elif (KTO)
            if (bookieController == null)
                bookieController = new KTO();

            if (EurobetReconnectThread == null)
            {
                //EurobetReconnectThread = new Thread(EurobetReconnectRun);
                //EurobetReconnectThread.Start();
            }
#elif (WPLAY)
            if (bookieController == null)
                bookieController = new Wplay();

            if (EurobetReconnectThread == null)
            {
                //EurobetReconnectThread = new Thread(EurobetReconnectRun);
                //EurobetReconnectThread.Start();
            }
#elif (SEUBET)
            if (bookieController == null)
                bookieController = new SeubetCtrl();
#elif (BET365_QRAPI)
            if(bookieController == null)
            bookieController = new Bet365_qrapiCtrl();
#endif

#if (TROUBLESHOT)
            string region = bookieController.getProxyLocation();
            LogMng.Instance.onWriteStatus(string.Format("Region of Bot - {0}", region));
#endif
            LogMng.Instance.onWriteStatus("Bot Start and login");

            bool logResult = bookieController.login();
            if (logResult)
            {
                Global.balance = bookieController.getBalance();
                UserMng.GetInstance().SendClientBalance(Global.balance);

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Balance = Global.balance.ToString("N2");
                }));

                LogMng.Instance.onWriteStatus(string.Format("Balance: {0}", Global.balance));

                //Winamax Testing
                /*BetburgerInfo info = new BetburgerInfo();
                info.odds = 1.64;
                info.outcome = "123w4234";
                info.direct_link = "1119268015";
                info.stake = 1;

                bookieController.PlaceBet(ref info);*/
            }
            else
            {
                LogMng.Instance.onWriteStatus(string.Format("Login failed!"));
                OnStopCommand();
                bIsRunningConnectedServer = false;
                return;
            }

            if (threadBet != null)
            {
                threadBet.Abort();
                threadBet = null;
            }

            threadBet = new Thread(betTask);
            threadBet.IsBackground = true;
            threadBet.Start();

            bIsRunningConnectedServer = false;
        }

        public string GetOpenBetList()
        {
            string result = "";
            try
            {
                Global.OpenUrl(string.Format("https://www.{0}/#/MB/", Setting.Instance.domain));
                Thread.Sleep(3000);
                var command = string.Format("(function () {{ {0} }})();", Global.GetOpenBetListCommandLine);
                result = Global.RunScriptCode(command);
            }
            catch (Exception ex)
            {
                result = ex.Message;
                Trace.WriteLine("GetValue Exception: " + ex.Message);
            }
            return result.ToLower();
        }

        private void OnGetStakeCommand()
        {
            return;
            if (DateTime.Now.Subtract(Setting.Instance.lastGetStakeTime).TotalHours < 20)
            {
                if (MessageBox.Show($"You got stake from balance less than 20 hours ({Setting.Instance.lastGetStakeTime.ToString("g")}), will you get balance again anyway?", "Question", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return;
            }

            //            GetStakeDialog dialog = new GetStakeDialog();
            //            if (dialog.ShowDialog().Value)
            //            {
            //                Setting.Instance.stakeSoccerLive = 0;
            //                SettingViewModel.StakeSoccerLive = 0;

            //                Setting.Instance.percentageGetStake = dialog.Result_Percentage;                

            //                Task.Run(() =>
            //                {                    
            //                    if (bookieController == null)
            //                    {
            //#if (BET365)
            //            //bookieController = new Bet365Ctrl();
            //#elif (BET365_VM)
            //            bookieController = new Bet365_VMCtrl(); 
            //#elif (BET365_BM)
            //            bookieController = new Bet365_BMCtrl(); 
            //#elif (BET365_PL)
            //            bookieController = new BET365_PLCtrl(); 
            //#elif (BET365_ADDON)
            //                        bookieController = new Bet365_ADDONCtrl();
            //#elif (BET365_PUPPETEER)
            //            bookieController = new BET365_PUPPETEERCtrl();
            //#elif (BET365_CHROMEDEV)
            //            bookieController = new BET365_CHROMEDEVCtrl();
            //#endif
            //                    }


            //                    if (bookieController.login())
            //                    {
            //                        Global.balance = bookieController.getBalance();
            //                        Balance = Global.balance.ToString("N2");


            //                    }
            //                });
            //                Setting.Instance.lastGetStakeTime = DateTime.Now;
            //                Setting.Instance.saveSetting();
            //            }
        }

        private void OnClearBetHistoryCommand()
        {
            FinishedBetList.Clear();
        }
        private void OnRefreshBalanceCommand()
        {
            return;
            if (string.IsNullOrEmpty(Setting.Instance.username) || string.IsNullOrEmpty(Setting.Instance.password))
                return;

            //BetburgerInfo info = new BetburgerInfo();
            //info.homeTeam = "aaa";
            //info.awayTeam = "bbb";
            //info.bookmaker = "Bet365";
            //info.sport = "Soccer";
            //info.started = "02-23-2022 12:00";
            //info.percent = 5;
            //info.odds = 1.53;
            //info.league = "ccc";
            //info.eventTitle = "aaa - bbb";
            //info.direct_link = "666541595|8/15|115493015";
            //info.stake = 0.1;
            //_betburgerInfo.Add(info);

            Task.Run(() =>
            {
                RefreshableBalance = false;
                if (bookieController == null)
                {
#if (BET365)
            //bookieController = new Bet365Ctrl();
#elif (BET365_VM)
            bookieController = new Bet365_VMCtrl(); 
#elif (BET365_BM)
            bookieController = new Bet365_BMCtrl(); 
#elif (BET365_PL)
            bookieController = new BET365_PLCtrl(); 
#elif (BET365_ADDON)
            bookieController = new Bet365_ADDONCtrl(); 
#elif (BET365_PUPPETEER)
            bookieController = new BET365_PUPPETEERCtrl();
#elif (BET365_CHROMEDEV)
            bookieController = new BET365_CHROMEDEVCtrl();
#elif (UNIBET)
            bookieController = new UnibetCtrl();
#elif (_888SPORT)
            bookieController = new _888SportCtrl();
#elif (BETPLAY)
            bookieController = new BetplayCtrl();
#elif (RUSHBET)
            bookieController = new RushbetCtrl();
#elif (PADDYPOWER || BETFAIR_NEW)
            bookieController = new PaddyPowerCtrl();
#elif (LEOVEGAS)
            bookieController = new LeovegasCtrl();
#elif (ELYSGAME)
            bookieController = new ElysgameCtrl();
#elif (EUROBET)
            bookieController = new EurobetCtrl();
#elif (SISAL)
            bookieController = new SisalCtrl();
#elif (SKYBET)
            bookieController = new SkybetCtrl();
#elif (FANDUEL)
            bookieController = new FanduelCtrl();
#elif (PLANETWIN)
                    bookieController = new PlanetwinCtrl();
#elif (BWIN || SPORTINGBET)
                    bookieController = new BwinCtrl();
#elif (SNAI)
            bookieController = new SnaiCtrl();
//#elif (LOTTOMATICA)
//            bookieController = new LottomaticaCtrl();
#elif (BETMGM)
            bookieController = new BetMGMCtrl();
#elif (NOVIBET)
            bookieController = new NovibetCtrl();
#elif (WINAMAX)
            bookieController = new WinamaxCtrl();
#elif (BETWAY)
            bookieController = new BetwayCtrl();
#elif (BETWAY_ADDON)
            bookieController = new Betway_ADDONCtrl();
#elif (PINNACLE)
            bookieController = new PinnacleCtrl();
#elif (SUPERBET)
            bookieController = new SuperbetCtrl();
#elif (STOIXIMAN || BETANO)
            bookieController = new StoiximanCtrl();
//#elif (BETANO)
//            bookieController = new BetanoCtrl();
#elif (BETFAIR_FETCH || BETFAIR || BETFAIR_PL)
                    bookieController = new BetfairCtrl();                    
#elif (BETPREMIUM)
            bookieController = new BetpremiumCtrl();            
#elif (DOMUSBET || BETALAND)
            bookieController = new DomusbetCtrl();            
#elif (SPORTPESA || CHANCEBET || REPLATZ)
                    bookieController = new SportpesaCtrl();            
#elif (GOLDBET || LOTTOMATICA)
                    bookieController = new GoldbetCtrl();      
#elif (PLAYPIX)
                    bookieController = new Playpix_CDP();
#elif (WPLAY)
                    bookieController = new Wplay();
#endif
                }


                if (!bookieController.login())
                {
                    LogMng.Instance.onWriteStatus(string.Format("[RefreshBalance] Login Failed"));
                    RefreshableBalance = true;
                    return;
                }

                Global.balance = bookieController.getBalance();
                Balance = Global.balance.ToString("N2");


#if (BET365_PL || BET365_VM)
                bookieController.Pulse();
#endif
                RefreshableBalance = true;
            }
            );
        }

        public void M_GlobalHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            return;
            if (e.KeyData == System.Windows.Forms.Keys.F3)
            {
                OnStartCommand();
            }
            else if (e.KeyData == System.Windows.Forms.Keys.F4)
            {
                OnStopCommand();
            }
        }

        private void OnStopCommand()
        {

            if (!Global.bRun)
                return;

            Global.bRun = false;

            IsStopped = true;
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
            catch ( Exception ex)
            {
                LogMng.Instance.onWriteStatus($"Exception in stopCommand:{ex.Message}");
            }
            user = null;

            try
            {
                if (threadBet != null && threadBet.IsAlive)
                    threadBet.Abort();
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"Exception in stopCommand:{ex.Message}");
            }

#if (ELYSGAME || EUROBET || LOTTOMATICA || GOLDBET || LEOVEGAS || SNAI || PLANETWIN || BETPREMIUM || SPORTPESA || REPLATZ || CHANCEBET || DOMUSBET || BETFAIR_FETCH || BETFAIR || BETFAIR_PL || SISAL || BWIN || SPORTINGBET || NOVIBET || BETWAY || WINAMAX || WPLAY || BETALAND || BETFAIR_NEW)
            if (EurobetReconnectThread != null)
            {
                EurobetReconnectThread.Abort();
            }
#endif

            try
            {
                if (EurobetReconnectThread != null)
                    EurobetReconnectThread.Abort();

                if (RefreshThread != null)
                    RefreshThread.Abort();
            }
            catch { }

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
            try
            {
                if (index == "BROWSER")
                {
                    //if (bBrowserVisible)
                    //{
                    //    Global.SetMonitorVisible(false);
                    //    bBrowserVisible = false;
                    //}
                    //else
                    //{
                    //    Global.SetMonitorVisible(true);
                    //    bBrowserVisible = true;
                    //}
                }
                else if (index == "MINIMIZE")
                {
                    this.RequestMinimize.Invoke(this, null);
                }
                else if (index == "RESTORE")
                {
                    this.RequestRestore.Invoke(this, null);
                }
                else
                {
                    //m_GlobalHook.KeyDown -= M_GlobalHook_KeyDown;

                    OnStopCommand();
                    try
                    {
                        if (bookieController != null)
                            bookieController.Close();
                    }
                    catch { }
                    this.RequestClose.Invoke(this, null);

                    if (popupDialog != null)
                        popupDialog.Close();


                    if (Global.socketServer != null)
                    {
                        try
                        {
                            Global.socketServer.Stop();
                        }
                        catch { }
                        Global.socketServer = null;
                    }
                }
            }
            catch { }
        }

        private async Task OnMenuCommand(string index)
        {
            log.Info(string.Format("OnMenuCommand - {0}", index));

            this.CurrentTab = index;
        }
        private void GuardStart()
        {
            Setting.Instance.key = "0e0mh583kfclx8m4obp0kzgdpc56gwjb";
            Setting.Instance.salt = "t49f66ynufrq2abx";
            Guard.Start_Session();
        }

        #region properties
        public ICommand StartCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand RefreshBalanceCommand { get; private set; }
        public ICommand ClearBetHistory { get; private set; }
        public ICommand WindowCommand { get; private set; }
        public ICommand GetStakeCommand { get; private set; }
        public ICommand MenuCommand { get; private set; }
        public ICommand TimeScheduleCommand { get; private set; }

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

        private ObservableCollection<BetburgerInfo> _finishedBetList;
        public ObservableCollection<BetburgerInfo> FinishedBetList
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

        private bool _refreshableBalance;
        public bool RefreshableBalance
        {
            get { return _refreshableBalance; }
            set { SetProperty(ref _refreshableBalance, value); }
        }
        #endregion
    }
}
