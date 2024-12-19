using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Timers;
using BetburgerServer;
using BetburgerServer.Constant;
using Protocol;
using System.Net.Http;
using System.Net;
using System.Collections;
using System.Text.RegularExpressions;
using BetburgerServer.Controller;
using Newtonsoft.Json;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HtmlAgilityPack;
using ArbRegServer;
using BetComparerServer.Controller;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace SeastoryServer
{
    public enum SERVER_EVENT : int
    {
        ONGATEWAYDISCONNECT = 0,    // 霸捞飘傀捞辑滚家南瞒窜
        ONGATEWAYCONNECT,           // 霸捞飘傀捞辑滚客 家南楷搬己傍
        ONGATEWAYLOGIN,             // 霸捞飘傀捞辑滚肺弊牢己傍
        ONSERVICESTART,
        ONSERVICESTOP,
        ONSERVERMODE,               // 辑滚悼累规侥搬沥
        ONUSERLOGOUT,               // 蜡历啊涝呕硼
        ONUSERLOGIN,                // 蜡历啊涝
        ONUSERCHANGE,               // 蜡历舅规急琶 棺 扁拌痢蜡惑怕函版
        ONCHATMSG,                  // 蜡历狼 盲泼肺弊荤扒
    }

    public enum LOG_EVENT : int
    {
        ONERRORMSG = 0,
        ONDEBUGMSG,
        ONNOTICEMSG
    }

    
    public delegate void ServerEvent(SERVER_EVENT evt, Object param);
    public delegate void LogEvent(LOG_EVENT evt, Object param);

    public partial class frmMain : Form
    {
        public delegate void ServerEventHandler(SERVER_EVENT evt, Object param);
        public delegate void LogEventHandler(LOG_EVENT evt, Object param);
        private const int MAXLOGLINE = 500;
        private FileStream m_fileStream = null;
        private string m_strFileName = "";               
        public event onWriteStatusEvent onWriteStatus;
        private Thread threadEventUpdater1 = null;
        private Thread threadEventUpdater2 = null;
        private volatile bool shouldStop = false;       

        public List<PickSource> pickSourceList = new List<PickSource>();
        public DateTime pickSourceUpdatedTime = DateTime.MinValue;

        ViewerForm viewerForm = null;
        BackendConnector backendConnector = new BackendConnector();
         
#if (JOE)
        Bet365LiveAgent.FrmMain frmLiveAgent = null;
#endif
#if (TGACCOUNTMANAGE)
        TGAccountManager TGManager = new TGAccountManager();
#endif
    //frmCefSharp sharpDlg = null;
    Thread thrEventListRefresh = null;
        private enum STATUS : byte
        {
            NOLOGIN_STATUS = 0,
            LOGIN_STATUS,
            SERVICE_STATUS,
        }

        private STATUS m_nStatus;

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
        public frmMain()
        {
#if (SUREBETTEST)
            string[] testlines = System.IO.File.ReadAllLines("missed.txt");
            for (int i = 0; i < testlines.Length / 11; i++)
            {
                //if (i >= 800)
                //    break;

                BetburgerInfo info = new BetburgerInfo();
                info.kind = PickKind.Type_9;
                info.sport = ConvertSport(Utils.Between(testlines[i * 11 + 2], "sport: ").ToLower());
                info.league = Utils.Between(testlines[i * 11 + 3], "league: ");                
                info.homeTeam = Utils.Between(testlines[i * 11 + 4], "home: ");
                info.awayTeam = Utils.Between(testlines[i * 11 + 5], "away: ");
                info.outcome = Utils.Between(testlines[i * 11 + 6], "outcome: ");
                info.extra = Utils.Between(testlines[i * 11 + 7], "extra: ");
                info.odds = Utils.ParseToDouble(Utils.Between(testlines[i * 11 + 8], "odds: "));                
                string betsapiparseLogResult = "";
                Trace.WriteLine($"Iterator : {i}  ----");
                BetsapiHelper.Instance.UpdateBet365SiteUrl(testlines[i * 11 + 10], ref info, out betsapiparseLogResult);
                                
            }
#endif

            InitializeComponent();

            ServicePointManager.DefaultConnectionLimit = 2;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            Global.onwriteStatus += SendLog;
     

#if (JOE)
            frmLiveAgent = new Bet365LiveAgent.FrmMain(ParseAPIRequest);
            frmLiveAgent.Show();
#endif
        }

        private HttpClient getHttpClient()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            CookieContainer cookieContainer = new CookieContainer();

            handler.CookieContainer = cookieContainer;

            HttpClient httpClientEx = new HttpClient(handler);
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html, application/xhtml+xml, application/xml; q=0.9, image/webp, *; q=0.8");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9,ko;q=0.8,ko-KR;q=0.7");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;

            return httpClientEx;
        }

        public void SendLog(string strMsg)
        {
            OnLogEventHandler(LOG_EVENT.ONNOTICEMSG, strMsg);
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            viewerForm = new ViewerForm();
            viewerForm.Show();

            backendConnector.Start();

            //BetsapiHelper helper = new BetsapiHelper(SendLog);
            //helper.RefreshAllPrematchEventList(new List<string> {"soccer", "basketball" });

            Trace.WriteLine("frmMain_Load");

            m_nStatus = STATUS.NOLOGIN_STATUS;

            chkViewMode.Checked = true;
            UpdateMenu();


            //var me = GameConstants.botClient.GetMeAsync().Result;
            //Update[] updates = GameConstants.botClient.GetUpdatesAsync(0, timeout: 20).Result;
            //foreach (var update in updates)
            //{
            //}

            Routes.GET["POST"].Add(new Route()
            {
                UrlRegex = "^/API/TipNotify",
                Method = "POST",
                Callable = APIGetTip
            });

            Routes.GET["POST"].Add(new Route()
            {
                UrlRegex = "^/API/pirxtToken",
                Method = "POST",
                Callable = PirxtToken
            });

            //Routes.GET["POST"].Add(new Route()
            //{
            //    UrlRegex = "^/API/xnsrToken",
            //    Method = "POST",
            //    Callable = XnsrToken
            //});

            cServerSettings.GetInstance().LoadSettings();
            GameServer.GetInstance().LogEventListener += new LogEvent(this.OnLogEvent);
            GameServer.GetInstance().ServerEventListener += new ServerEvent(this.OnServerEvent);

            Text = string.Format("Server V.{0}  {1}", cServerSettings.GetInstance().Version, cServerSettings.GetInstance().Label);
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void mnuActionLogin_Click(object sender, EventArgs e)
        {
            
            if (string.IsNullOrEmpty(cServerSettings.GetInstance().Label))
            {
                MessageBox.Show("Please set server Label");
                return;
            }
            if (!string.IsNullOrEmpty(cServerSettings.GetInstance().TradematesportsUsername) && !string.IsNullOrEmpty(cServerSettings.GetInstance().TradematesportsPassword))
                viewerForm.StartTrademateSports();

            mnuActionLogin.Enabled = false;
            Thread thread;
            thread = new Thread(new ThreadStart(LoginDB));
            thread.Start();
        }

        private void LoginDB()
        {
            this.UseWaitCursor = true;
            
            if (!DBManager.Connect_DB())            
            {
                this.Invoke(new Action(() =>
                {                    
                    mnuActionLogin.Enabled = true;
                    this.UseWaitCursor = false;
                    return;
                }));
            }

            this.UseWaitCursor = false;
            OnServerEvent(SERVER_EVENT.ONSERVICESTOP, this);
        }

        public void OutputLog(string strMsg, bool bNewLine)
        {
            try
            {
                if (listLog.InvokeRequired)
                {
                    listLog.Invoke(Global.onwriteStatus , strMsg);
                }
                else
                {
                    bool bViewMode = chkViewMode.Checked;
                    int nLastRow = listLog.Items.Count;

                    if (nLastRow >= MAXLOGLINE)
                    {
                        listLog.Items.Clear();
                        //for (int i = 1; i < nLastRow; i++)
                        //{
                        //    object item = listLog.Items[i];
                        //    listLog.Items[i - 1] = item;
                        //}
                        //listLog.Items.RemoveAt(nLastRow - 1);
                    }
                    listLog.Items.Add(strMsg);
                    if (bViewMode && nLastRow > 0)
                        listLog.TopIndex = listLog.Items.Count - 1;
                }              
            }
            catch (Exception e)
            {
                Global.onwriteStatus($"OutputLog + {e.ToString()}");
            }
        }

        public void OutputLog(string strMsg)
        {
            //Trace.WriteLine(strMsg);
            OutputLog(strMsg, true);
        }

        private void UpdateUserCount()
        {
            this.lblConnectCnt.Text = string.Format("Current Users: {0}", GameServer.GetInstance().GetUserCount());
            updateUsers(GameServer.GetInstance().GetUserList());
        }

        private void UpdateMenu()
        {
            bool bMnuActionLogin = true;
            bool bMnuActionStartSrv = false;
            bool bMnuActionStopSrv = false;

            switch (m_nStatus)
            {
                case STATUS.NOLOGIN_STATUS:
                default:
                    bMnuActionStartSrv = false;
                    bMnuActionStopSrv = false;
                    break;
                case STATUS.LOGIN_STATUS:
                    bMnuActionLogin = false;
                    bMnuActionStartSrv = true;
                    bMnuActionStopSrv = false;
                    break;
                case STATUS.SERVICE_STATUS:
                    bMnuActionLogin = false;
                    bMnuActionStartSrv = false;
                    bMnuActionStopSrv = true;
                    break;
            }
            mnuActionLogin.Enabled = bMnuActionLogin;
            mnuActionStart.Enabled = true;
            mnuActionStart.Enabled = bMnuActionStartSrv;
            mnuActionStop.Enabled = bMnuActionStopSrv;
        }

        public void OnServerEvent(SERVER_EVENT evt, Object param)
        {
            if (this.Visible)
            {
                this.BeginInvoke(new ServerEventHandler(OnServerEventHandler), evt, param);
            }
        }

        public void OnLogEvent(LOG_EVENT evt, Object param)
        {
            if (this.Visible)
            {
                this.BeginInvoke(new LogEventHandler(OnLogEventHandler), evt, param);
            }
        }

        public void OnServerEventHandler(SERVER_EVENT evt, Object param)
        {
            switch (evt)
            {
                case SERVER_EVENT.ONGATEWAYDISCONNECT:  // 霸捞飘傀捞立加瞒窜
                    GameServer.GetInstance().Close();
                    m_nStatus = STATUS.NOLOGIN_STATUS;
                    UpdateMenu();
                    break;
                case SERVER_EVENT.ONGATEWAYCONNECT:
                    break;
                case SERVER_EVENT.ONGATEWAYLOGIN:
                    break;
                case SERVER_EVENT.ONCHATMSG:
                    break;
                case SERVER_EVENT.ONSERVICESTART:       // 辑厚胶矫累
                    m_nStatus = STATUS.SERVICE_STATUS;
                    UpdateMenu();
                    break;
                case SERVER_EVENT.ONSERVICESTOP:        // 辑厚胶吝瘤
                    if (MYSqlMng.GetInstance().ReconnectMode == false && !MYSqlMng.GetInstance().IsConnected)
                    {
                        m_nStatus = STATUS.NOLOGIN_STATUS;
                    }
                    else
                    {
                        m_nStatus = STATUS.LOGIN_STATUS;
                    }
                    UpdateMenu();
                    UpdateUserCount();
                    break;
                case SERVER_EVENT.ONSERVERMODE:         // 林辑滚悼累荤扒
                    break;
                case SERVER_EVENT.ONUSERLOGIN:          // 货蜡历 殿废
                    {
                        UserInfo client = (UserInfo)param;
                        UpdateUserCount();
                    }
                    break;
                case SERVER_EVENT.ONUSERLOGOUT:         // 蜡历啊涝呕硼
                    {
                        UserInfo client = (UserInfo)param;
                        UpdateUserCount();
                    }
                    break;
                case SERVER_EVENT.ONUSERCHANGE:         // 蜡历舅规急琶 & 扁拌痢蜡函版
                    break;
            }
        }

        public void OnLogEventHandler(LOG_EVENT evt, Object param)
        {
            string strMsg;
            string strTime, strMsgType;

            DateTime now = DateTime.Now;

            strTime = string.Format("{0} ->", now.ToString("yyyy-MM-dd HH:mm:ss"));
            strMsgType = " ";
            switch (evt)
            {
                case LOG_EVENT.ONERRORMSG:
                    strMsgType = " === 坷幅惯积 === ";
                    break;

                case LOG_EVENT.ONDEBUGMSG:
                    strMsgType = "### DEBUG ### ";
                    break;

                case LOG_EVENT.ONNOTICEMSG:
                    break;
            }
            strMsg = string.Format("{0}{1}{2}", strTime, strMsgType, param.ToString());
            OutputLog(strMsg);
            LogToFile(strMsg);
        }

        private void mnuActionClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void frmMain_Closing(object sender, FormClosingEventArgs e)
        {
            string strMsg = "You want to exit this program?";

            DialogResult result = MessageBox.Show(strMsg, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                e.Cancel = true;
            }
            else if (GameServer.GetInstance().IsStart)
            {
                e.Cancel = false;
                GameServer.GetInstance().Close();
                if (thrEventListRefresh != null)
                {
                    thrEventListRefresh.Abort();
                    thrEventListRefresh = null;
                }

            }

            backendConnector.Stop();
        }

        private void frmMainTextlog_Enter(object sender, EventArgs e)
        {
            chkViewMode.Focus();
        }

        private void mnuActionStart_Click(object sender, EventArgs e)
        {

#if (!FORSALE)

/*            try
            {
                Global.tgbotClient = new TelegramBotClient("6063125415:AAG136iCV5CZchGEOChC_nYNxyPePSO-3uE");    //@bet365liveprovider_bot
            }
            catch (Exception ex)
            {
                OnLogEvent(LOG_EVENT.ONNOTICEMSG, ex);
            }
            var me = Global.tgbotClient.GetMeAsync().Result;
            Update[] updates = Global.tgbotClient.GetUpdatesAsync(0, timeout: 20).Result;
            foreach (var update in updates)
            {

            }*/
#endif
#if (BET365 && !FORSALE)
            string myExternalIP = Utils.GetMyIPAddress();
            MessageBox.Show($"My External IP {myExternalIP}", "Check server ip", MessageBoxButtons.OK);
            if (!WebAPIServer.GetInstance().start(IPAddress.Parse(myExternalIP), cServerSettings.GetInstance().WebAPIPort, 100, string.Empty))
            {
                MessageBox.Show("Couldn't start WebAPI server. Make sure port " + cServerSettings.GetInstance().WebAPIPort + " is not being used by other process", "Error", MessageBoxButtons.OK);
                return;
            }

#endif
            try
            {
                GameServer.GetInstance().ListenPort = cServerSettings.GetInstance().ListenPort;
                GameServer.GetInstance().StartServer();
            }
            catch (Exception ex){
                OnLogEvent(LOG_EVENT.ONNOTICEMSG, ex);
            }
#if (JOE)
            frmLiveAgent.Start();


            //if (thrEventListRefresh == null)
            //{
            //    thrEventListRefresh = new Thread(EventListRefreshFunc);
            //    thrEventListRefresh.Start();
            //}
#endif

            

#if (TGACCOUNTMANAGE)
            TGManager.Start();
#endif
            //timerCursorMover.Enabled = true;

        }

        private void ParseAPIRequest(string name, string param1, string param2, string param3, string param4)
        {
            string tipString = $"Tip fetched name: {name} param1: {param1} param2: {param2} param3: {param3} param4: {param4}";
            OnLogEvent(LOG_EVENT.ONNOTICEMSG, tipString);
            Trace.WriteLine(tipString);

            Trace.WriteLine("APIGetTip 3");

            //PickSource pickSource = pickSourceList.Find(o => o.name == name);
            //if (pickSource != null)
            //{
            //    BetburgerInfo info = new BetburgerInfo();
                
            //    info.kind = PickKind.Type_2;
            //    info.bookmaker = "Bet365";
            //    info.opbookmaker = "Unkown";

            //    while (true)
            //    {
            //        OnLogEvent(LOG_EVENT.ONNOTICEMSG, $"picksource name: {pickSource.name} directlink: {pickSource.ndirectlink} isbroadcast: {pickSource.isbroadcast}");
            //        if (pickSource.ndirectlink == 0)
            //        {
            //            dynamic jsonResResp = JsonConvert.DeserializeObject<dynamic>(param1);

            //            info.kind = PickKind.Type_2;
            //            info.created = jsonResResp.created_time.ToString();
            //            info.updated = jsonResResp.created_time.ToString();
            //            info.started = jsonResResp.start_time.ToString();
            //            info.homeTeam = jsonResResp.master.ToString();
            //            info.awayTeam = jsonResResp.slave.ToString();
            //            info.eventTitle = jsonResResp.event_name.ToString();
            //            info.league = jsonResResp.event_name.ToString();
            //            info.direct_link = string.Format("{0}|{1}|{2}", jsonResResp.i2.ToString(), jsonResResp.odd.ToString(), jsonResResp.fd.ToString());
            //            try
            //            {
            //                info.odds = Convert.ToDouble(jsonResResp.odd.ToString());
            //            }
            //            catch { }
            //            info.sport = "Horse Racing";
            //            info.outcome = "comp";
            //            info.opbookmaker = "Comparison";
            //        }
            //        else if (pickSource.ndirectlink == 1)
            //        {
            //            //soccer live 
            //            string[] descriptions = param1.Split('|');

            //            if (descriptions.Length != 4)
            //                break;
            //            //percent
            //            //sport
            //            //eventTitle
            //            //outcome
            //            //odds
            //            //updated
            //            info.kind = PickKind.Type_3;
            //            info.created = DateTime.Now.ToString();
            //            info.updated = DateTime.Now.ToString();
            //            info.started = DateTime.Now.ToString();

            //            info.sport = descriptions[0].Trim();
            //            info.league = descriptions[1].Trim();
            //            info.homeTeam = descriptions[2].Trim();
            //            info.awayTeam = descriptions[3].Trim();
            //            info.eventTitle = $"{info.homeTeam} - {info.awayTeam}";

            //            info.siteUrl = param3;
            //            info.direct_link = param4;
            //            info.outcome = param2;
            //            try
            //            {
            //                string[] directparams = info.direct_link.Split('|');
            //                info.odds = Utils.FractionToDouble(directparams[1]);
            //            }
            //            catch { }

            //            info.opbookmaker = "SoccerLive";
            //        }
            //        else if (pickSource.ndirectlink == 2)
            //        {
            //            string command = param1;
            //            string bs = param2;

            //            if (command.ToLower() != "placebet")
            //            {
            //                break;
            //            }

            //            info.kind = PickKind.Type_4;
            //            info.created = DateTime.Now.ToString();
            //            info.updated = DateTime.Now.ToString();
            //            info.started = DateTime.Now.ToString();
            //            info.homeTeam = "";
            //            info.awayTeam = "";
            //            info.eventTitle = bs;
            //            info.league = "";
            //            info.siteUrl = "";
            //            info.direct_link = "";
            //            info.outcome = "Tips";
            //            info.sport = "Tips";
            //            info.opbookmaker = "Tipster2";
            //        }
            //        else if (pickSource.ndirectlink == 3)
            //        {
            //            Task.Run(() => sendTelegramMsg(tipString));

            //            info.kind = PickKind.Type_5;
            //            info.created = DateTime.Now.ToString();
            //            info.updated = DateTime.Now.ToString();
            //            info.started = DateTime.Now.ToString();
            //            info.homeTeam = "";
            //            info.awayTeam = "";
            //            info.eventTitle = param1;
            //            info.league = "";
            //            info.siteUrl = "";
            //            info.direct_link = "";
            //            info.outcome = "Tips";
            //            info.sport = "Tips";
            //            info.opbookmaker = "Tipster2";

            //            dynamic jsonResResp = JsonConvert.DeserializeObject<dynamic>(param1);
            //            try
            //            {
            //                info.odds = Utils.ParseToDouble(jsonResResp.betData[0].odd.ToString());
            //            }
            //            catch { }

            //            //if (pickSource.name == "Joe-CopybotOpenbet" && rand.Next(1, 2) == 1)
            //            //{
            //            //    SendWebAPIToKYM(param1);
            //            //}
            //        }
            //        else if (pickSource.ndirectlink == 4)
            //        {//Joan - GestIOValue
            //            dynamic jsonResResp = JsonConvert.DeserializeObject<dynamic>(param1);

            //            info.kind = PickKind.Type_6;
            //            info.created = jsonResResp.created_time.ToString();
            //            info.updated = jsonResResp.created_time.ToString();
            //            info.started = jsonResResp.start_time.ToString();
            //            info.homeTeam = jsonResResp.home.ToString();
            //            info.awayTeam = jsonResResp.away.ToString();
            //            info.eventTitle = string.Format("{0} - {1}", info.homeTeam, info.awayTeam);
            //            info.league = "";
            //            info.siteUrl = jsonResResp.bet365link.ToString();
            //            info.direct_link = string.Format("{0}|{1}|{2}", jsonResResp.i2.ToString(), jsonResResp.odd.ToString(), jsonResResp.fd.ToString());
            //            info.sport = jsonResResp.sport.ToString();
            //            try
            //            {
            //                info.percent = Convert.ToDecimal(jsonResResp.percentage.ToString());
            //            }
            //            catch
            //            {
            //                info.percent = 0;
            //            }
            //            info.outcome = jsonResResp.outcome.ToString();
            //            try
            //            {
            //                info.odds = Utils.FractionToDouble(jsonResResp.odd.ToString());
            //            }
            //            catch { }

            //            info.opbookmaker = "Value";
            //        }
                                        
            //        if (!pickSource.isbroadcast)
            //            break;

            //        List<BetburgerInfo> list = new List<BetburgerInfo>() { info };
            //        GameServer.GetInstance().processValuesInfo(list);

            //        break;
            //    }

            //    OnLogEvent(LOG_EVENT.ONNOTICEMSG, $"Pick processed name {name} {pickSource.description}");
            //}
            //else
            //{
            //    Task.Run(() => sendTelegramMsg(tipString));

            //    OnLogEvent(LOG_EVENT.ONNOTICEMSG, $"Pick Ignored name {name}");
            //}
        }
        public HttpResponse APIGetTip(HttpRequest httpRequest)
        {
            Trace.WriteLine("APIGetTip 0");
            //if (DateTime.Now.Subtract(pickSourceUpdatedTime).TotalSeconds > 60)
            //{
            //    Trace.WriteLine("APIGetTip 1");
            //    pickSourceList.Clear();

            //    try
            //    {
            //        string query = string.Format("select * from picksource");
            //        string[][] result = MYSqlMng.GetInstance().SelectQuery(query);
            //        if (result != null && result.Length > 0)
            //        {
            //            for (int i = 0; i < result.Length; i++)
            //            {
            //                PickSource pickSource = new PickSource(result[i][1], result[i][2], result[i][3], result[i][4]);
            //                pickSourceList.Add(pickSource);
            //            }
            //        }
            //    }
            //    catch { }
            //    pickSourceUpdatedTime = DateTime.Now;
            //}
            HttpResponse response = new HttpResponse();
            response.setContent("OK");
            response.StatusCode = "200";
#if (!FORSALE)
            Trace.WriteLine("APIGetTip 2");
            try
            {
                TipInfo request = JsonConvert.DeserializeObject<TipInfo>(httpRequest.Content);

                string param1 = Utils.ToLiteralString(request.param1);
                string param2 = Utils.ToLiteralString(request.param2);
                string param3 = Utils.ToLiteralString(request.param3);
                string param4 = Utils.ToLiteralString(request.param4);

                ParseAPIRequest(request.name, param1, param2, param3, param4);

            }
            catch (Exception ex)
            {

            }
#endif
            return response;
        }

        private void SendWebAPIToKYM(string command)
        {
            Thread postAPI = new Thread(() =>
            {
                try
                {
                    //var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://89.40.6.53:7002/interface/dataFromApp2");
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://37.187.91.64:5002/interface/dataFromApp3");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";



                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        //string json = $"{{\"tipster\":\"lgs\"," +
                        //                  $"\"url\":\"{command}\"," +
                        //                  $"\"payload\":\"{bs}\"," +
                        //                  $"\"isRequest\":true," +
                        //                  $"\"userTerm\":\"\"," +
                        //                  $"\"pirA\":\"\"," +
                        //                  $"\"pirB\":\"\"," +
                        //                  $"\"pirC\":\"\"," +
                        //                  $"\"pirD\":\"\"," +
                        //                  $"\"pirF\":\"\"," +
                        //                  $"\"pirZ\":\"\"}}";

                        streamWriter.Write(command);
                    }

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    OnLogEvent(LOG_EVENT.ONNOTICEMSG, $"To KYM exception {ex}");
                }
            });
            postAPI.Start();
        }

        public HttpResponse PirxtToken(HttpRequest httpRequest)
        {
            HttpResponse response = new HttpResponse();
            response.setContent("OK");
            response.StatusCode = "200";

            string pirxtToken = httpRequest.Content;
            OnLogEvent(LOG_EVENT.ONNOTICEMSG, string.Format("PirxtToken: {0}", pirxtToken));

#if (!FORSALE)

            //try
            //{
            //    Trace.WriteLine(string.Format("Received Token data: {0}", httpRequest.Content));

            //    string pirxtToken = httpRequest.Content;
            //    JObject obj = JsonConvert.DeserializeObject<JObject>(httpRequest.Content);

            //    BetHeaderInfo newHeader = new BetHeaderInfo();
            //    newHeader.Tick = DateTime.Now;
            //    newHeader.Pirxtheaders.Clear();

            //    foreach (var x in obj)
            //    {
            //        if (x.Key == "Token")
            //        {
            //            newHeader.NSToken = x.Value.ToString();
            //            OnLogEvent(LOG_EVENT.ONNOTICEMSG, string.Format("XnsrToken {0}", newHeader.NSToken));
            //        }
            //        else
            //        {
            //            newHeader.Pirxtheaders.Add(new BetCryptHeader(x.Key, x.Value.ToString()));
            //        }
            //    }

            //    GameServer.GetInstance().SendNSToken(newHeader.NSToken);
            //    GameServer.GetInstance().SendAllBetHeaders(newHeader.Pirxtheaders);
            //    OnLogEvent(LOG_EVENT.ONNOTICEMSG, string.Format("PirxtToken {0}", pirxtToken.Substring(100)));

            //    try
            //    {
            //        Monitor.Enter(GameConstants.betheaderLocker);

            //        if (GameConstants.BetHeaders.Count > 0)
            //        {
            //            for (int i = GameConstants.BetHeaders.Count - 1; i >= 0; i--)
            //            {
            //                if (DateTime.Now.Subtract(GameConstants.BetHeaders[i].Tick).TotalMinutes > 10)
            //                    GameConstants.BetHeaders.RemoveAt(i);
            //            }
            //        }
            //        GameConstants.BetHeaders.Add(newHeader);
            //    }
            //    catch { }
            //    finally
            //    {
            //        Monitor.Exit(GameConstants.betheaderLocker);
            //    }
            //}
            //catch { }
#endif
            return response;
        }
        public HttpResponse XnsrToken(HttpRequest httpRequest)
        {
            HttpResponse response = new HttpResponse();
            response.setContent("OK");
            response.StatusCode = "200";
#if (!FORSALE)

            try
            {
                string pirxtToken = httpRequest.Content;
                //JObject obj = JsonConvert.DeserializeObject<JObject>(httpRequest.Content);
                //GameConstants.NSToken = obj["Token"].ToString();
                //GameServer.GetInstance().SendNSToken();

                //OnLogEvent(LOG_EVENT.ONNOTICEMSG, string.Format("XnsrToken {0}", GameConstants.NSToken));
            }
            catch { }
#endif
            return response;
        }

        private int sendTelegramMsg(string text)
        {
            #if (!FORSALE)
            if (string.IsNullOrEmpty(text))
                return 0;

            try
            {
                //Telegram.Bot.Types.Message msg = GameConstants.botClient.SendTextMessageAsync(new ChatId(644451800), text, Telegram.Bot.Types.Enums.ParseMode.Html).Result;//GBot Notifier
                //return msg.MessageId;
            }
            catch (Exception ex)
            {

            }
#endif
            return 0;
        }


        private void mnuActionStop_Click(object sender, EventArgs e)
        {
            WebAPIServer.GetInstance().stop();
            GameServer.GetInstance().Close();
            
        
#if (TGACCOUNTMANAGE)
            TGManager.Stop();
#endif
            timerCursorMover.Enabled = false;

#if (JOE)
            frmLiveAgent.Stop();
#endif
        }

        private void mnuSettingServ_Click(object sender, EventArgs e)
        {
            frmServerSetting frmSetting = new frmServerSetting();
            frmSetting.ShowDialog(this);
        }

        private void LogToFile(string strMsg)
        {
            try
            {
                if (m_strFileName.Length == 0 || m_fileStream == null)
                {
                    DateTime now = DateTime.Now;
                    m_strFileName = string.Format("GameServer_{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}.log",
                        now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                }
                lock (this)
                {
                    string strFilePath = Application.StartupPath;
                    strFilePath = string.Format("{0}\\log\\", Application.StartupPath);
                    Directory.CreateDirectory(strFilePath);
                    strFilePath += m_strFileName;
                    m_fileStream = System.IO.File.Open(strFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                    if (m_fileStream.Length > 6291456)
                    {
                        DateTime now = DateTime.Now;
                        m_strFileName = string.Format("GameServer_{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}.log",
                            now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                    }
                    StreamWriter writer = new StreamWriter(m_fileStream, Encoding.UTF8);
                    writer.WriteLine(strMsg);
                    writer.Close();
                }
            }
            catch (Exception e)
            {
                string strErr = string.Format("颇老肺弊巢扁扁 角菩: {0}", e.Message);
                OutputLog(strErr);
            }
            finally
            {
                if (m_fileStream != null)
                {
                    m_fileStream.Close();
                }
            }
        }

        private void betburgerCredientalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmBetburger frm = new frmBetburger();
            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                cServerSettings.GetInstance().SaveSetting();

                Text = string.Format("Server V.{0}  {1}", cServerSettings.GetInstance().Version, cServerSettings.GetInstance().Label);
            }
        }


        private void updateUsers(ArrayList users)
        {
            try
            {               

                tblUsers.Invoke(new Action(() =>
                {
                    tblUsers.Rows.Clear();
                    foreach (UserInfo user in users)
                    {
                        string ip = "";
                        try
                        {
                            ip = user.Sock.RemoteEndPoint.ToString();
                        }
                        catch { }

                        if (user.Privillage.ToLower() == "admin")
                            continue;
                        if (!string.IsNullOrEmpty(user.UUID))
                            addUser(user);
                    }
                }));

                backendConnector.SendOnlineUserList(users);
            }
            catch { }
        }

        private void addUser(UserInfo user)
        {
            int nIndex = tblUsers.Rows.Add();
            if (nIndex < 0)
                return;
            string clientip = "UNKNOWN";
            try
            {
                clientip = user.Sock.RemoteEndPoint.ToString();
            }
            catch { }

            tblUsers.Rows[nIndex].SetValues(user.Bookmaker, user.License, user.GameID, user.ExpireTime.ToString("MM-dd-yyyy HH:mm"), clientip);
            tblUsers.Rows[nIndex].Tag = user;
        }

        private void tblExit_Click(object sender, EventArgs e)
        {
            if (tblUsers.SelectedRows == null || tblUsers.SelectedRows.Count < 1)
            {
                MessageBox.Show("Select user to Disconnect.");
                return;
            }

            UserInfo user = tblUsers.SelectedRows[0].Tag as UserInfo;

            DialogResult result = MessageBox.Show(string.Format("Are you confirm to disconnect [{0}] ?", user.GameID), "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                
                GameServer.GetInstance().RemoveClient(user);
            }
        }

        Random rand = new Random();


        private void EventListRefreshFunc()
        {
#if (BET365)
            //SetCursorPos(rand.Next(1, 5) * 100, rand.Next(1, 5) * 100);
            int nCounter = 60;
            while (true)
            {
                nCounter++;
#if (!FORSALE)
                BetsapiHelper.Instance.RefreshAllInplayEventList();
                

                if (nCounter > 60)
                {
                    nCounter = 0;
                    
                    //update per 60 min
                    List<string> sports = new List<string>();
                    sports.Add("soccer");
                    sports.Add("basketball");
                    sports.Add("volleyball");
                    sports.Add("baseball");
                    sports.Add("tennis");
                    sports.Add("tabletennis");
                    sports.Add("hockey");
                    sports.Add("rugby league");
                    sports.Add("rugby union");
                    sports.Add("e-sports");
                    sports.Add("handball");
                    sports.Add("horse racing");

                    BetsapiHelper.Instance.RefreshAllPrematchEventList(sports);
                    
                }
#endif
                Thread.Sleep(1 * 60 * 1000);
            }
#endif
            }
            private void timerCursorMover_Tick(object sender, EventArgs e)
        {
#if (BET365)
            //SetCursorPos(rand.Next(1, 5) * 100, rand.Next(1, 5) * 100);
#endif
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            try
            {
                
                GameServer.GetInstance().ListenPort = cServerSettings.GetInstance().ListenPort;
                GameServer.GetInstance().StartServer();
            }
            catch (Exception ex)
            {
                OnLogEvent(LOG_EVENT.ONNOTICEMSG, ex);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TelegramCtrl task = new TelegramCtrl();
            task.sendMessage("sersdfds");
            //TelegramCtrl.Instance.sendMessage("sdfsdfds");
            //GameConstants.bRun = true;
            //var tasks = new List<Task>();

            //if (!string.IsNullOrEmpty(cServerSettings.GetInstance().BetsmarterUsername) && !string.IsNullOrEmpty(cServerSettings.GetInstance().BetsmarterPassword))
            //{
            //    GmailApi api = new GmailApi(Global.onwriteStatus);
            //    tasks.Add(api.scrap_thread());

            //    CDPController.Instance.InitializeBrowser("https://betsmarter.app/");

            //    Betsmarter betsmart = new Betsmarter(Global.onwriteStatus);
            //    tasks.Add(betsmart.scrap_thread());
            //}
        }

        private void test_gmail()
        {
           
        }
        
    }
}
