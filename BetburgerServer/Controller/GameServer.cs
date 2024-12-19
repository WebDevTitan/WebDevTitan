//#define REELITEM
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
using BetburgerServer.Model;
using kr.com.choya.net.socket;
using System.Net.Http;
using BetburgerServer.Constant;
using System.Net;
using Protocol;
using ArbRegServer;
using System.Diagnostics;
using BetburgerServer.Controller;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Web;
using BetburgerServer;
using BetComparerServer.Controller;
using System.Windows.Forms;

namespace SeastoryServer
{
    public delegate void onWriteStatusEvent(string status);
    public delegate void onJsonFeedsEvent(List<JsonArb> arbs, List<JsonDirBetCombination> combines);

    public delegate void onJsonDirectoriesEvent(JsonDir dir, List<JsonBookmaker> booker);
    public delegate string onRunJSGetNSTokenEvent(string Param);

    class   GameServer: cSocketServer
    {      

        private static GameServer m_Instance = null;
        /// <summary>
       
        /// </summary>
        public byte[] PrimaryEncDecKey
        {
            get
            {
                byte[] arrEncDecKey = new byte[] { 0x42, 0x7D, 0xE2, 0xA5 };
                return arrEncDecKey;
            }
        }
        public const int MAXPACKETLENGTH = 1024 * 10;       

        private Hashtable m_hashClients;            
        private ArrayList m_arrAutoUsers;           
        public event LogEvent LogEventListener;
        public event ServerEvent ServerEventListener;

        public event onRunJSGetNSTokenEvent getNSToken = null; 
        public event onWriteStatusEvent onWriteStatus;
        public event onJsonDirectoriesEvent onJsonDir;
        public event onJsonFeedsEvent onJsonArb;
        public event onJsonFeedsEvent onJsonValue;
                
        private List<JsonArb> _arbInfo = null;
        private JsonDir _dirInfo = null;
        private List<JsonBookmaker> _bookers = null;
        private List<BetburgerInfo> _betburgerInfo = null;
        private List<BetburgerInfo> _totalBetburgerInfo = null;
        private List<BetburgerInfo> _allowedInfoList = null;

        DateTime lastUpdateNSToken = DateTime.MinValue;
        

        private string[] _bookmakers = {
                                           "Seubet", "Bet365", "Bwin", "SportsBetting", "MyBet", "Interwetten", "888Sport", "WilliamHill", "Betsson", "Cashpoint", "PlanetWin365", "TonyBet", "10bet", "Admiral", "BFSportsbook", "Betway", "Tipico", "TheGreek", "Betvictor", "Marathon", "RedBet", "1xbet", "Novibet", "PaddyPower", "Unibet" 
                                       };

        private string[] _bookmakersAgainst = {
                                                  "Betfair", "Sbobet", "Pinnacle", "188bet", "BetAtHome", "BetOnline", "Betvictor", "Jetbull", "Matchbook", "Betdaq", "Smarkets", "Dafabet","Pinnacle"
                                              };

        /// <summary>
        /// </summary>
        public bool IsStart
        {
            get
            {
                return m_bIsStart;
            }
        }

        public delegate void ProcessNetdata(Object param1, Object param2);
        private Hashtable m_process = new Hashtable();

        private delegate void MonitoringTimer();
        private Thread m_thread = null;
        private Thread m_threadHeart = null;

        private string SpainProxyIP = "";

        private JObject betburger_outcomes = null;
        public GameServer(int nPort)
            : base(nPort)
        {
            m_hashClients = Hashtable.Synchronized(new Hashtable());
            m_arrAutoUsers = ArrayList.Synchronized(new ArrayList());
            m_bIsStart = false;

            
            onWriteStatus += SendLog;
            onJsonDir += jsonDirData;
            onJsonArb += jsonArbData;
            onJsonValue += jsonValueData;

            m_process.Add(NETMSG_CODE.NETMSG_CLIENTMESSAGE, new ProcessNetdata(ProcClientMessage));
            m_process.Add(NETMSG_CODE.NETMSG_LICENSE, new ProcessNetdata(ProcLicenseCheck));
            m_process.Add(NETMSG_CODE.NETMSG_SAVEBET, new ProcessNetdata(ProcSaveBet));
            m_process.Add(NETMSG_CODE.NETMSG_UPLOADBALANCE, new ProcessNetdata(ProcUpdateBalance)); 
            m_process.Add(NETMSG_CODE.NETMSG_HEARTBEAT, new ProcessNetdata(ProcHeartBeat));
            m_process.Add(NETMSG_CODE.NETMSG_CLOSE, new ProcessNetdata(ProcGameClose));
            m_process.Add(NETMSG_CODE.NETMSG_CONTROL, new ProcessNetdata(ProcExitServer));
            m_process.Add(NETMSG_CODE.NETMSG_NSTOKEN, new ProcessNetdata(ProcNsToken)); 

            _totalBetburgerInfo = new List<BetburgerInfo>();
            _allowedInfoList = new List<BetburgerInfo>();




            Assembly assem = GetType().Assembly;

            string[] names = assem.GetManifestResourceNames();

            using (Stream stream = assem.GetManifestResourceStream("BetburgerServer.Constant.outcome.csv"))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string betburgerOutcome = reader.ReadToEnd();
                        betburger_outcomes = JsonConvert.DeserializeObject<JObject>(betburgerOutcome);
                    }
                }
                catch (Exception ex)
                {
                }
            }

            
        }

        public static GameServer GetInstance()
        {
            if (m_Instance == null)
            {
                m_Instance = new GameServer(0);
            }
            return m_Instance;
        }

        public int GetUserCount()
        {
            int nCount = 0;
            //Trace.WriteLine("GetUserCount Start");
            try
            {
                lock (m_hashClients)
                {
                    foreach (UserInfo user in m_hashClients.Values)
                    {
                        if (user.Status == USERSTATUS.NOLOGIN_STATUS)
                            continue;
                        nCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                //Trace.WriteLine($"GetUserCount Exception {ex}");
            }
            //Trace.WriteLine("GetUserCount End");
            return nCount;// m_hashClients.Count;
        }

        public ArrayList GetUserList()
        {
            //Trace.WriteLine("GetUserList Start");
            ArrayList users = new ArrayList();
            try
            {
                lock (m_hashClients)
                {
                    foreach (UserInfo user in m_hashClients.Values)
                    {
                        if (user.Status == USERSTATUS.LOGIN_STATUS)
                            users.Add(user);
                    }
                }
            }
            catch (Exception ex)
            {
                //Trace.WriteLine($"GetUserList Exception {ex}");
            }
            //Trace.WriteLine("GetUserList End");
            return users;
        }

        new public void StartServer()
        {
#if (BET365)
            //GameConstants.NSToken = GetNstkToken();
#endif
            
            try
            {
                if (!this.IsStart)
                {
                    base.StartServer();
                }

                SendLog($"Starting service. (Port:{ListenPort})");

                ServerEventListener(SERVER_EVENT.ONSERVICESTART, this);
            }
            catch (Exception ex)
            {
                string strMsg;

                strMsg = "Cannot enter the socket status.";
                SendLog(strMsg, ex);
                Close();
            }
        }

        // 소켓대기상태에 들어갈 때 호출
        public override void OnStart()
        {
            m_bIsStart = true;
            GameConstants.bRun = true;

            if (m_thread == null)
            {
                m_thread = new Thread(new ThreadStart(MonitoringArb));
                m_thread.Start();
            }

            if (m_threadHeart == null)
            {
                m_threadHeart = new Thread(new ThreadStart(MonitoringHeart));
                m_threadHeart.Start();
            }

            MYSqlMng.GetInstance().ReconnectMode = true;
        }

        // Monitor Body
        public void MonitoringArb()
        {
            try
            {
                var tasks = new List<Task>();



#if !(FORSALE) && !(BETFAIR)
                //RebelScraper task0 = new RebelScraper(onWriteStatus);
                //tasks.Add(task0.scrape());

                //SurebetScraper task0 = new SurebetScraper(onWriteStatus);
                //tasks.Add(task0.scrape());

                //if (!string.IsNullOrEmpty(cServerSettings.GetInstance().BetspanUsername) && !string.IsNullOrEmpty(cServerSettings.GetInstance().BetspanPassword))
                //{
                //    BetspanScraper task1 = new BetspanScraper(onWriteStatus);
                //    tasks.Add(task1.scrape());
                //}


                //if (!string.IsNullOrEmpty(cServerSettings.GetInstance().TradematesportsUsername) && !string.IsNullOrEmpty(cServerSettings.GetInstance().TradematesportsUsername))
                //{
                //    TradematesportsScraper task3 = new TradematesportsScraper(onWriteStatus);
                //    tasks.Add(task3.scrape());
                //}

                //if (!string.IsNullOrEmpty(cServerSettings.GetInstance().SurebetUsername) && !string.IsNullOrEmpty(cServerSettings.GetInstance().SurebetPassword))
                //{
                //    TipstrrScraper task4 = new TipstrrScraper(onWriteStatus);
                //    tasks.Add(task4.scrape());
                //}
#endif

                //TelegramScraper task2 = new TelegramScraper(onWriteStatus);
                //tasks.Add(task2.scrape());

                if (!string.IsNullOrEmpty(cServerSettings.GetInstance().BetspanUsername) && !string.IsNullOrEmpty(cServerSettings.GetInstance().BetspanPassword))
                {
                    BetspanScraper task1 = new BetspanScraper(onWriteStatus);
                    tasks.Add(task1.scrape());
                }

                if ((cServerSettings.GetInstance().EnableSeubet_Live == true || cServerSettings.GetInstance().EnableSeubet_Prematch == true) && !string.IsNullOrEmpty(cServerSettings.GetInstance().Percent_Price))
                {
                    SeubetScraper task5 = new SeubetScraper(onWriteStatus);
                    tasks.Add(task5.scrape());
                } else
                {
                    MessageBox.Show("Please check Seubet status.And input percent.");
                }
              

                if (!string.IsNullOrEmpty(cServerSettings.GetInstance().BetsmarterUsername) && !string.IsNullOrEmpty(cServerSettings.GetInstance().BetsmarterPassword))
                {
                    GmailApi api = new GmailApi(onWriteStatus);
                    tasks.Add(api.scrap_thread());

                    CDPController.Instance.InitializeBrowser("https://betsmarter.app/");

                    Betsmarter betsmart = new Betsmarter(onWriteStatus);
                    tasks.Add(betsmart.scrap_thread());
                }

                BetburgerScraper task = new BetburgerScraper(onWriteStatus, onJsonDir, onJsonArb, onJsonValue);
                tasks.Add(task.scrape());

                Task.WaitAll(tasks.ToArray());
            }
            catch(Exception e)
            {
                Global.onwriteStatus($"MonitoringArb + {e.ToString()}");
            }
          
        }

        public string GetNstkToken()
        {
            string result = "";
            using (var client = new WebClient())
            {

                try
                {
                    string urlReq = "";
#if (FORSALE)
                    urlReq = $"http://{cServerSettings.GetInstance().NSTServer}:8000/api/getToken/";  //powerserver
#else
                    urlReq = "http://176.223.142.38:7070/api/getToken/"; 
#endif
                    result = client.DownloadString(urlReq);
                }
                catch (Exception ex){ }
            }
            return result;
            //string ret = string.Empty;
            //try
            //{
            //    string jsScript = GameConstants.B365SimpleEncryptJS + GameConstants.NSTTokenJS.Replace("/***nstTokenLib***/", GameConstants.tokenScript);
            //    var jsResult = GameConstants.browser.EvaluateScriptAsync(jsScript).Result;
            //    return jsResult.Result == null ? string.Empty : jsResult.Result.ToString();
            //}
            //catch (Exception ex)
            //{

            //}
            //return ret;
        }

        void MonitoringHeart()
        {
            while (GameConstants.bRun)
            {
                try
                {
                    //SendAllHeartbeats();
                    SendHeartbeat();
                    Thread.Sleep(10000);

#if (BET365)
                //try
                //{
                //    if (DateTime.Now.Subtract(lastUpdateNSToken).TotalSeconds > 300)
                //    {                        
                //        HttpClient client = new HttpClient();
                //        HttpResponseMessage response = client.GetAsync("http://node-de-25.astroproxy.com:10095/api/changeIP?apiToken=ba90a36dc2f16419").Result;

                //        try
                //        {
                //            string strContent = response.Content.ReadAsStringAsync().Result;
                //            SendLog($"GET Spain Proxy IP: {strContent}");

                //            dynamic proxyResult = JsonConvert.DeserializeObject<dynamic>(strContent);
                //            SpainProxyIP = proxyResult.IP.ToString();
                //            lastUpdateNSToken = DateTime.Now;
                //        }
                //        catch
                //        {
                //            lastUpdateNSToken = DateTime.Now.Subtract(TimeSpan.FromSeconds(30));
                //        }
                //    }

                //}
                //catch (Exception ex)
                //{
                //    onWriteStatus("Exception in getting NSToken..." + ex);
                //}
#endif
                }
                catch (Exception e)
                {
                    Global.onwriteStatus($"MonitoringHeart + {e.ToString()}");
                }
    
            }
        }

        public void SendAllBetHeaders(List<BetCryptHeader> BetHeader)
        {
            NetPacket newnetdata = new NetPacket();
            newnetdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_BETHEADERS;            
            string value = JsonConvert.SerializeObject(BetHeader);
            newnetdata.Append(value);

            //Trace.WriteLine("SendAllBetHeaders");
            SendAllUser(newnetdata);
        }

        public void SendNSToken(string NSToken)
        {
            if (!string.IsNullOrEmpty(NSToken))
            {
                NetPacket netdata = new NetPacket();
                netdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_NSTOKEN;
                netdata.Append(NSToken);

                //Trace.WriteLine("SendNSToken");
                SendAllUser(netdata);                                
            }
            else
            {
                onWriteStatus("Send NSToken... NULL token");
            }
        }

        private void SendAllHeartbeats()
        {
            try
            {
                NetPacket netdata = new NetPacket();
                netdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_HEARTBEAT;
                //Trace.WriteLine("Send All Heartbeats");
                SendAllUser(netdata);
            }
            catch (Exception e)
            {

            }
        }

        // 클라이언트에서 소켓접속이 이루어 졌을 때 호출
        public override void OnSocketConnect(Socket sock)
        {
            try
            {
                string strMsg;
                strMsg = string.Format("Request connection from [{0}].", sock.RemoteEndPoint.ToString());
                SendLog(strMsg);

                UserInfo client = new UserInfo(sock);

                // check duplicated bet
                if (!checkDuplicatedUser(client.ClientIP))
                {
                    client.Status = USERSTATUS.NOLOGIN_STATUS;
                    AddUser(client);
                    client.BeginReceive();
                }
            }
            catch (Exception)
            {
                //if (sock != null)
                //{
                //    sock.Close();
                //}
            }
        }

        private bool checkDuplicatedUser(string ip)
        {
            return false;
            try
            {
                lock (m_hashClients)
                {
                    List<UserInfo> removeUsers = new List<UserInfo>();
                    bool is_double = false;
                    foreach (UserInfo client in m_hashClients.Values)
                    {
                        if (client.ClientIP.Equals(ip))
                        {
                            is_double = true;
                            break;
                        }
                    }
                    return is_double;
                }
            }
            catch (Exception ex)
            {
                string strMsg;
                strMsg = string.Format("Error occured while checkDuplicatedUser [{0}]", ex);
                SendLog(strMsg, ex);
            }
        }

        /// <summary>
        /// 수신된 파켓을 분석하여 해당 처리를 진행한다.
        /// </summary>
        /// <param name="buff">수신된 파켓자료</param>
        /// <param name="param">추가파라메터(여기서는 수신된 유저이다.)</param>
        public void ProcPacketData(NetPacket netPacket, object param)
        {
            if (!(param is UserInfo))
            {
                return;
            }

            UserInfo client = (UserInfo)param;
            if (netPacket == null)
            {
                // 소켓이 끊긴 경우
                if (m_bIsStart == true)
                {
                    
                    ProcGameClose(null, client);
                    
                }
                return;
            }
            try
            {
                //if (netPacket.MsgCode != (ushort)NETMSG_CODE.NETMSG_HEARTBEAT && netPacket.MsgCode != 1992 && ((client.Status == USERSTATUS.NOLOGIN_STATUS && netPacket.MsgCode != (ushort)NETMSG_CODE.NETMSG_CLIENTLOGIN &&
                //            netPacket.MsgCode > (ushort)NETMSG_CODE.NETMSG_CLIENTLOGIN) ||
                //            (client.Status != USERSTATUS.NOLOGIN_STATUS &&
                //            (netPacket.MsgCode <= (ushort)NETMSG_CODE.NETMSG_CLIENTLOGIN || netPacket.MsgCode == (ushort)NETMSG_CODE.NETMSG_CLIENTLOGIN))))
                //{
                //    // 아직 인증되지 않은 유저인데 가입메세지가 아닌 자료가 왔거나
                //    if (client.Sock != null)
                //    {
                //        string strMsg;

                //        strMsg = "Not authenticated data sent!";
                //        SendLog(strMsg);
                //    }
                //    ProcGameClose(null, client);
                //}
                //else 
                if (m_process.Contains((NETMSG_CODE)netPacket.MsgCode))
                {
                    client.LastRecvPacketID = netPacket.SendPacketID;     // 클라이언트에서 보내는 파켓의 식별값을 보관한다.
                    int nLastRecvPacketID = netPacket.LastRecvPacketID;       // 클라이언트가 마지막으로 받은 파켓식별값

                    ProcessNetdata process = (ProcessNetdata)m_process[(NETMSG_CODE)netPacket.MsgCode];
                    process(netPacket, client);
                }
            }
            catch (Exception ex)
            {
                string strMsg;

                strMsg = string.Format("{0}-수신된 데이터를 읽을 수 없습니다.", client.License);
                SendLog(strMsg, ex);
                ProcGameClose(null, client);
            }
        }

        public override void OnError(string msg, Exception e)
        {

        }

        public void Close()
        {
            if (m_bIsStart)
            {
                m_bIsStart = false;
                GameConstants.bRun = false;
                base.CloseServer();
                //Trace.WriteLine("Close Start");
                try 
                { 
                    lock (m_hashClients)
                    {
                        Hashtable hashClients = new Hashtable();
                        foreach (UserInfo user in m_hashClients.Values)
                        {
                            try
                            {
                                hashClients.Add(user.License, user);
                            }
                            catch (Exception)
                            { }
                        }
                        foreach (UserInfo user in hashClients.Values)
                        {
                            try
                            {
                                // 유저에게 게임종료메세지를 호출하고 배열에서 삭제한다.
                                ProcGameClose(null, user);
                            }
                            catch (Exception)
                            { }
                        }
                        hashClients.Clear();
                        m_hashClients.Clear();
                    }
                
                }
                catch (Exception ex)
                {
                    //Trace.WriteLine($"Close Exception {ex}");
                }
                //Trace.WriteLine("Close End");
                SendLog("Stop service.");
                ServerEventListener(SERVER_EVENT.ONSERVICESTOP, this);
            }
            if (m_thread != null)
            {
                m_thread.Abort();
            }
            m_thread = null;

            if (m_threadHeart != null)
            {
                m_threadHeart.Abort();
            }
            m_threadHeart = null;
        }


        // 동작: 파라메터로 들어온 유저를 등록된 정보에서 제거한다.
        // 결과: TRUE 성공, FALSE 찾을수 없음
        public void RemoveClient(UserInfo client)
        {
            //Trace.WriteLine("RemoveClient start");
            try
            {
                if (m_hashClients.Count > 0)
                {
                    lock (m_hashClients)
                    {
                        // 등록된 유저인가를 판정한다.
                        if (m_hashClients.ContainsKey(client.HashKey))
                        {
                            string strMsg;
                            m_hashClients.Remove(client.HashKey);
                            if (client.LogName != "")
                            {
                                strMsg = string.Format("Remove user [{0}].", client.LogName);
                                SendLog(strMsg);
                            }
                        }
                    }
                }

                ServerEventListener(SERVER_EVENT.ONUSERLOGOUT, client);
            }
            catch (Exception e)
            {
                //Trace.WriteLine($"RemoveClient Exception {e}");
                string strMsg;
                strMsg = string.Format("Error on client process: {0} ", e.Message);
                LogEventListener(LOG_EVENT.ONERRORMSG, strMsg);
            }
            finally
            {
                client.Close();
            }
            //Trace.WriteLine("RemoveClient End");
        }

        // 새로운 유저를 등록한다.
        private void AddUser(UserInfo client)
        {
            //Trace.WriteLine("AddUser start aa");
            try
            {
                lock (m_hashClients)
                {
                    m_hashClients.Add(client.HashKey, client);
                    // 유저가입상태감시창의 정보를 갱신해준다.
                    
                }
            }
            catch (Exception ex)
            {
                //Trace.WriteLine($"AddUser exception {ex}");
            }
            //Trace.WriteLine("AddUser end");
        }

        // 파켓자료를 전체 유저들에게 보내준다.
        public void SendAllUser(NetPacket netdata)
        {
            //Trace.WriteLine("SendAllUser Start");
            try
            {
                lock (m_hashClients)
                {
                    //Trace.WriteLine("SendAllUser Start In");
                    foreach (UserInfo user in m_hashClients.Values)
                    {                 
                        if (user.ExpireTime < DateTime.Now)
                        {
                            continue;
                        }
                        try
                        {
                            if (user.Status == USERSTATUS.NOLOGIN_STATUS)
                                continue;

                            
                            //onWriteStatus($"SendAllUser SendData Before {user.Nickname}");
                            user.SendData(netdata);
                            //onWriteStatus($"SendAllUser SendData After {user.Nickname}");
                        }
                        catch (Exception e)
                        {
                            onWriteStatus($"SendAllUser Exception {user.License}");
                            //RemoveClient(user);
                        }
                    }
                }
            }            
            catch (Exception ex)
            {
                onWriteStatus($"SendAllUser Exception {ex}");
            }
            //Trace.WriteLine("SendAllUser End");
        }

        public void SendRandomUser(NetPacket netdata)
        {
            //Trace.WriteLine("SendRandomUser start");
            try 
            { 
                lock (m_hashClients)
                {
                    int totalCnt = m_hashClients.Values.Count;
                    int randomCnt = (totalCnt / 100) * cServerSettings.GetInstance().randomUser;
                    int i = 0;
                    foreach (UserInfo user in m_hashClients.Values)
                    {
                        try
                        {
                            if (user.Status == USERSTATUS.NOLOGIN_STATUS)
                                continue;

                            if (randomCnt == i && randomCnt > 0)
                                break;

                            //Trace.WriteLine("SendRandomUser SendData Before");
                            user.SendData(netdata);
                            //Trace.WriteLine("SendRandomUser SendData After");
                            i++;
                        }
                        catch (Exception e)
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Trace.WriteLine($"SendRandomUser exception {ex}");
            }
            //Trace.WriteLine("SendRandomUser end");
        }

        private bool CheckDoubleLicense(string license)
        {
            //Trace.WriteLine("CheckDoubleLicense Start");
            try
            {
                lock (m_hashClients)
                {
                    foreach (UserInfo user in m_hashClients.Values)
                    {
                        if (user.Status == USERSTATUS.NOLOGIN_STATUS)
                            continue;

                        if (user.License == license)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Trace.WriteLine($"CheckDoubleLicense Exception {ex}");
            }
            //Trace.WriteLine("CheckDoubleLicense End");
            return false;
        }

        private void UpdateUserById(UserInfo client, string uuid, string license_creator, string license_privillage, string license, string gameid, DateTime expire, string bookmaker, bool QRScannable = false)
        {
            //Trace.WriteLine("UpdateUserById Start");
            try
            {
                lock (m_hashClients)
                {
                    foreach (UserInfo user in m_hashClients.Values)
                    {
                        if (user.Status != USERSTATUS.NOLOGIN_STATUS)
                            continue;

                        if (user.HashKey == client.HashKey)
                        {
                            user.UUID = uuid;
                            user.Privillage = license_privillage;
                            user.MasterName = license_creator;
                            user.License = license;
                            user.Status = USERSTATUS.LOGIN_STATUS;                            
                            user.ExpireTime = expire;
                            user.GameID = gameid;
                            user.Bookmaker = bookmaker;
                            user.QR_Scannable = QRScannable;

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Trace.WriteLine($"UpdateUserById Exception {ex}");
            }
            //Trace.WriteLine("UpdateUserById End");
        }

        private void ProcHeartBeat(Object param1, Object param2)
        {
            try
            {
                UserInfo client = (UserInfo)param2;
                NetPacket netdata = new NetPacket();
                netdata.MsgCode = (int)NETMSG_CODE.NETMSG_HEARTBEAT;
                //Trace.WriteLine("ProcHeartBeat SendData Before");
                client.SendData(netdata);
                //Trace.WriteLine("ProcHeartBeat SendData After");
            }
            catch(Exception e)
            {

            }
        }

        // 유저하트비트검사
        public void SendHeartbeat()
        {
            //Trace.WriteLine("SendHeartbeat Start aa");

            if (!cServerSettings.GetInstance().HeartBeat)
            {
                //Trace.WriteLine("SendHeartbeat End 1");
                return;
            }

            try
            {
                lock (m_hashClients)
                {
                    Hashtable hashLogout = new Hashtable();       // 끊어진 유저들을 검사하여 보관

                    foreach (UserInfo client in m_hashClients.Values)
                    {
                        
                        if (client.NetDeadTime > 60)
                        {
                            // 망통신하지 않은지 60초가 지났다면 선로가 끊어진것으로 판정한다.
                            hashLogout.Add(client.HashKey, client);
                        }
                        
                    }
                    foreach (UserInfo client in hashLogout.Values)
                    {
                        string strMsg;
                        if (client.LogName != "")
                        {
                            strMsg = string.Format("Kicked out cause of heartbeat checking user [{0}]", client.LogName);
                        }
                        else if (client.Sock != null)
                        {
                            strMsg = string.Format("Kicked out cause of heartbeat checking from {0}, sock = null", client.ClientIP);
                            //SendLog(strMsg);
                        }
                        strMsg = string.Format("Kicked out cause of heartbeat checking from {0}", client.ClientIP);

                        SendLog(strMsg);

                        ProcGameClose(null, client);
                    }
                }
            }
            catch (Exception ex)
            {
                //Trace.WriteLine($"SendHeartbeat Exception {ex}");
            }
            //Trace.WriteLine("SendHeartbeat End");
        }



        // 게임종료요청처리
        public void ProcGameClose(Object param1, Object param2)
        {
            if (param2 == null)
                return;
            UserInfo client = (UserInfo)param2;

            if (client.Status == USERSTATUS.NOLOGIN_STATUS)
            {
                RemoveClient(client);
                return;
            }

            RemoveClient(client);
        }

      
        public void ProcExitServer(Object param1, Object param2)
        {
            try
            {
                NetPacket netdata = (NetPacket)param1;
                object objType = netdata.GetData(0);
                int type = (int)objType;
                if (type == 19)
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }
        
        public void ProcNsToken(Object param1, Object param2)
        {

            NetPacket netdata = (NetPacket)param1;
            UserInfo client = (UserInfo)param2;

            SendLog($"QR Scanning requested from license : {client.License} account: {client.GameID}");

            if (!client.QR_Scannable)
            {
                SendLog($"QR Scanning is disabled in this license: {client.License} account: {client.GameID}");
                return;
            }
            try
            {

                int nMode = (int)netdata.Pop();
                System.Net.HttpStatusCode StatusCode = System.Net.HttpStatusCode.Unused;
                string param = netdata.Pop() as string;

                string strContent = "";
                if (nMode == 0)
                {
                    JObject betData = JsonConvert.DeserializeObject<JObject>(param);

                    SendLog($"QR Post xcft: {betData["xcft"]} lat: {betData["lat"]} lon: {betData["lon"]} countryCode: {betData["countryCode"]}");

                    betData["apiKey"] = "2e27fbbb49bd4b07b0ddfab8371a674a";
                    betData["region"] = "";
                    if (betData["countryCode"].ToString() == "ES")
                    {                                     //Guille           
                        betData["host"] = "www.bet365.es";
                        betData["proxy"] = $"http://guillegarridocontreras:na5nBvzqV5@185.121.14.180:59100";

                        //SendLog($"QR Post Spain proxy IP: {SpainProxyIP}");
                    }
                    else if (betData["countryCode"].ToString() == "BG")
                    {               //NAS X
                        betData["host"] = "www.bet365.com";
                        betData["proxy"] = "http://beta88:CtyuM3xX7mz8aTdxz6@bg.smartproxy.com:38026";
                    }
                    else if (betData["countryCode"].ToString() == "IT")
                    {
                        betData["host"] = "www.bet365.it";
                        betData["proxy"] = "http://marvit5:Nirvana1701@s4.airproxy.io:21210";
                    }
                    else if (betData["countryCode"].ToString() == "BD")
                    {   //JC_Spain
                        betData["host"] = "www.3256871.com";
                        betData["proxy"] = "http://adminip:J521Osnam$@103.150.136.88:41609";
                    }

                    string placebetPostContent = JsonConvert.SerializeObject(betData);

                    SendLog($"QR Post Request: {placebetPostContent}");
                    using (HttpClient newHttpClient = new HttpClient())
                    {
                        HttpResponseMessage qrBet365Response = newHttpClient.PostAsync("https://qrresolver.bettingco.ru/api/QrResolver/GetQrBet365", new StringContent(placebetPostContent, Encoding.UTF8, "application/json")).Result;
                        StatusCode = qrBet365Response.StatusCode;
                        SendLog($"QR Post Response StatusCode: {qrBet365Response.StatusCode}");
                        try
                        {
                            strContent = qrBet365Response.Content.ReadAsStringAsync().Result;
                            SendLog($"QR Post Response: {strContent}");
                        }
                        catch { }
                    }
                }
                //else if(nMode == 1)
                //{
                //    using (HttpClient newHttpClient = new HttpClient())
                //    {
                //        HttpResponseMessage resp = newHttpClient.GetAsync($"https://qrresolver.bettingco.ru/api/QrResolver/RefreshSession?refreshToken={param}&forcedResolve=true").Result;
                //        StatusCode = resp.StatusCode;
                //        SendLog($"QR Get Response StatusCode: {resp.StatusCode}");

                //        try
                //        {
                //            strContent = resp.Content.ReadAsStringAsync().Result;
                //            SendLog($"QR Get Response : {strContent}");
                //        }
                //        catch { }
                //    }                  
                //}

                NetPacket resnetdata = new NetPacket();
                resnetdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_NSTOKEN;
                resnetdata.Append(nMode);
                resnetdata.Append((int)StatusCode);
                resnetdata.Append(strContent);
                client.SendData(resnetdata);

                DBManager.Increase_QRScanCount(client.UUID);

                SendLog($"QR Scanning is finished, increasing count in license: {client.License} account: {client.GameID}");
            }
            catch (Exception e)
            {
               
            }
        }

        /// <summary>
        /// 서버로그를 남긴다.
        /// </summary>
        /// <param name="strMsg">로그문자열</param>
        public override void SendLog(string strMsg)
        {
            LogEventListener(LOG_EVENT.ONNOTICEMSG, strMsg);
        }

        /// <summary>
        /// 서버로그를 남긴다.
        /// </summary>
        /// <param name="strMsg">로그문자열</param>
        public override void SendLog(string strMsg, Exception ex)
        {
            if (ex != null)
            {
                strMsg = string.Format("{0} Exception kind: {1} Detail explaintation: {2}", strMsg, ex.Message, ex.StackTrace);
            }
            SendLog(strMsg);
        }

        /// <summary>
        /// 게임서버의 상태를 변경하는 이벤트를 발생한다.
        /// </summary>
        /// <param name="evt">변경시킬 사건종류</param>
        /// <param name="param">추가파라메터</param>
        public void SendServerEvent(SERVER_EVENT evt, object param)
        {
            ServerEventListener(evt, param);
        }

        private void jsonDirData(JsonDir dir, List<JsonBookmaker> booker)
        {
            _dirInfo = dir;
            _bookers = booker;
            
        }

        private void jsonValueData(List<JsonArb> arbs, List<JsonDirBetCombination> combines)
        {
            Debug.WriteLine(string.Format("jsonValueData-------------"));
            try
            {
                if (GameConstants.bRunning)
                    return;

                _arbInfo = arbs;
                foreach (JsonArb arb in _arbInfo)
                {
                    if (_arbInfo != null && arb.arbs != null)
                    {
                        _betburgerInfo = new List<BetburgerInfo>();
                        //arb.filterJsonArb();
                        //SendLog($"Updating Valuebet list, 1st step {arb.arbs.Count} items...");

                        List<BetburgerInfo> infoList = new List<BetburgerInfo>();
                        foreach (JsonArbArb arbarb in arb.arbs)
                        {

                            

                            string formula_id = _dirInfo.getCalcFormula(arbarb.arb_formula_id);
                            if (string.IsNullOrEmpty(formula_id))
                                continue;


                            string formula_key = "formula_" + formula_id;

                            JsonArbBet arbBet1 = arb.getJsonArbBetById(arbarb.bet1_id);
                            JsonArbBet arbBet2 = arb.getJsonArbBetById(arbarb.bet2_id);

                            BetburgerInfo info1 = getBetburgerInfoByBetId(arbarb, arbBet1, combines, formula_key);
                            BetburgerInfo info2 = getBetburgerInfoByBetId(arbarb, arbBet2, combines, formula_key);
                            if (info1 == null || info2 == null)
                                continue;
                            //SendLog($"{_dirInfo.getSportById(arbarb.sport_id)} {info1.percent} {info1.bookmaker} {info2.percent} {info2.bookmaker} {arbarb.league}");
                            info1.isLive = arb.isLive;
                            info2.isLive = arb.isLive;

                            if (info1.bookmaker == info2.bookmaker)
                                continue;

                            if (info1 != null && info2 != null)
                            {
                                _betburgerInfo.Add(info1);
                                _betburgerInfo.Add(info2);
                            }
                        }
                        GameConstants.bRunning = true;
                        List<BetburgerInfo> betburgerInfoList = updateBetburgerInfo(_betburgerInfo);

                        SendLog(string.Format("Updating Valuebet list, Found {0} items...", betburgerInfoList.Count));
                        // process betburger info list
                        processValuesInfo(betburgerInfoList);
                    }
                    else
                    {
                        SendLog(string.Format("Updating Valuebet list, Found {0} items...", -1));

                        // process betburger info list
                        processValuesInfo(new List<BetburgerInfo>());
                    }
                }


                GameConstants.bRunning = false;
            }
            catch (Exception e)
            {

            }
        }
        private void jsonArbData(List<JsonArb> arbs , List<JsonDirBetCombination> combines)
        {
            //Trace.Write("jsonArbData 0");
            try
            {
                if (GameConstants.bRunning)
                    return;

                //Trace.Write("jsonArbData 1");
                _arbInfo = arbs;
                foreach(JsonArb arb in _arbInfo) 
                {
                    if (_arbInfo != null && arb.arbs != null)
                    {
                        _betburgerInfo = new List<BetburgerInfo>();
                        //arb.filterJsonArb();
                        //Trace.Write("jsonArbData 2");
                        List<BetburgerInfo> infoList = new List<BetburgerInfo>();
                        foreach (JsonArbArb arbarb in arb.arbs)
                        {
                            string formula_id = _dirInfo.getCalcFormula(arbarb.arb_formula_id);
                            if (string.IsNullOrEmpty(formula_id))
                                continue;


                            string formula_key = "formula_" + formula_id;
                            
                            JsonArbBet arbBet1 = arb.getJsonArbBetById(arbarb.bet1_id);
                            JsonArbBet arbBet2 = arb.getJsonArbBetById(arbarb.bet2_id);

                            BetburgerInfo info1 = getBetburgerInfoByBetId(arbarb, arbBet1, combines, formula_key);
                            BetburgerInfo info2 = getBetburgerInfoByBetId(arbarb, arbBet2, combines, formula_key);
                            if (info1 == null || info2 == null)
                                continue;

                            info1.isLive = arb.isLive;
                            info2.isLive = arb.isLive;

                            if (info1.bookmaker == info2.bookmaker)
                                continue;

                            if (info1 != null && info2 != null)
                            {
                                _betburgerInfo.Add(info1);
                                _betburgerInfo.Add(info2);
                            }
                        }
                        //Trace.Write("jsonArbData 3");
                        GameConstants.bRunning = true;
                        List<BetburgerInfo> betburgerInfoList = updateBetburgerInfo(_betburgerInfo);
                        //Trace.Write($"jsonArbData 4 count: {betburgerInfoList.Count}");
                        //SendLog(string.Format("Updating arbitrage list, Found {0} items...", betburgerInfoList.Count));
                        // process betburger info list
                        processArbInfo(betburgerInfoList);
                        //Trace.Write($"jsonArbData 5");
                    }
                    else
                    {
                        //SendLog(string.Format("Updating arbitrage list, Found {0} items...", -1));

                        // process betburger info list
                        processArbInfo(new List<BetburgerInfo>());
                    }
                }

                //Trace.Write($"jsonArbData 6");
                GameConstants.bRunning = false;
            }
            catch (Exception e)
            {
                //Trace.Write($"jsonArbData 7");
                GameConstants.bRunning = false;
            }
        }
        
        private List<BetburgerInfo> updateBetburgerInfo(List<BetburgerInfo> _betburgerInfo)
        {
            List<BetburgerInfo> betburgerInfo = new List<BetburgerInfo>();

            for (int i = 0; i < _betburgerInfo.Count - 1; i += 2)
            {
                BetburgerInfo info1 = _betburgerInfo[i];
                BetburgerInfo info2 = _betburgerInfo[i + 1];

                if (!updateBetburgerInfo(ref info1))
                    continue;

                updateBetburgerInfo(ref info2);

                betburgerInfo.Add(info1);
                betburgerInfo.Add(info2);

                if (betburgerInfo.Count >= 40)
                    break;
            }

            return betburgerInfo;
        }

        private bool updateBetburgerInfo(ref BetburgerInfo info)
        {
            try
            {
                switch (info.bookmaker)
                {
                    //case "Bet365":
                    //    return updateBetburgerInfoDirectLink(ref info);
                    case "Snai":
                    case "PlanetWin365":
                        return updateBetburgerInfoDirectLink(ref info);
                    //case "Admiral":
                    //case "Betsson":
                    //case "Betway":
                    //case "MyBet":
                    //case "TheGreek":
                    //case "Tipico":
                    //case "TonyBet":
                    //case "10bet":
                    //case "Cashpoint":
                    //case "1xbet":
                    //case "BFSportsbook":
                    //case "PaddyPower":
                    //case "Ladbrokes":
                    //case "888Sport":
                    //case "Unibet":
                    //case "Novibet":                    
                    //case "Tipsport":
                    //    return updateBetburgerInfoDirectLink(ref info);
                    //case "Interwetten":
                    //    return updateBetburgerInfoDirectLinkInterwetten(ref info);
                    //case "Bwin":
                    //    return updateBetburgerInfoBwin(ref info);
                    //case "Marathon":
                    //    return updateBetburgerInfoMarathon(ref info);
                    //case "PlanetWin365":
                    //    return updateBetburgerInfoDirectLinkPlanet365(ref info);
                    //case "RedBet":
                    //case "Betvictor":
                    //    return true;
                    //case "Dafabet":
                    //case "Sbobet":
                    //case "188bet":
                    //case "Matchbook":
                    //    return true;
                    //case "Pinnacle":
                    //    return true;
                    //case "Betfair":
                    //    return true;
                    //case "WilliamHill":
                    //    return true;
                    //case "Skybet":
                    //    updateBetburgerInfoDirectLink(ref info);
                    //    return true;
                    //case "Fortuna":
                    //    //updateBetburgerInfoDirectLink(ref info);
                    //    return true;
                    //case "NikeSk":
                    //    updateBetburgerInfoDirectLink(ref info);
                    //    return true;
                    //case "Eurobet":
                    //    updateBetburgerInfoDirectLink(ref info);
                    //    return true;
                    default:
                        return true;
                }
            }
            catch (Exception e)
            {

            }

            return false;
        }

        private bool updateBetburgerInfoDirectLinkWilliamhill(ref BetburgerInfo info)
        {
            try
            {
                HttpClient httpClient = getHttpClient();

                if (string.IsNullOrEmpty(info.eventUrl))
                    return false;

                httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.betburger.com/arbs");
                HttpResponseMessage responseMessageMain = httpClient.GetAsync(info.eventUrl).Result;
                responseMessageMain.EnsureSuccessStatusCode();

                string mainReferer = responseMessageMain.RequestMessage.RequestUri.AbsoluteUri;
                if (string.IsNullOrEmpty(mainReferer) || mainReferer.Equals(info.eventUrl))
                    return false;

                string responseMessageMainString = responseMessageMain.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(responseMessageMainString))
                    return false;

                if (!responseMessageMainString.Contains("direct_link"))
                {
                    responseMessageMain = httpClient.GetAsync(info.eventUrl).Result;
                    responseMessageMain.EnsureSuccessStatusCode();

                    responseMessageMainString = responseMessageMain.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(responseMessageMainString) || !responseMessageMainString.Contains("direct_link"))
                        return false;
                }

                GroupCollection groups = Regex.Match(responseMessageMainString, "direct_link = '(?<siteUrl>.*)';").Groups;
                if (groups == null || groups["siteUrl"] == null)
                    return false;

                string siteUrl = groups["siteUrl"].Value;
                siteUrl = WebUtility.HtmlDecode(siteUrl);

                groups = Regex.Match(responseMessageMainString, "document\\.body\\.innerHTML = '(?<extra>.*)';").Groups;
                if (groups == null || groups["extra"] == null)
                    return false;

                string extra = groups["extra"].Value;

                info.siteUrl = siteUrl;
                info.extra = extra;
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private bool updateBetburgerInfoDirectLink(ref BetburgerInfo info)
        {
            try
            {
                //Trace.WriteLine($"[updateBetburgerInfoDirectLink]eventUrl: {info.eventUrl}");

                HttpClient httpClient = getHttpClient();

                httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.betburger.com/arbs");
                HttpResponseMessage responsemessageMain = httpClient.GetAsync(info.eventUrl).Result;
                responsemessageMain.EnsureSuccessStatusCode();

                
                string mainReferer = responsemessageMain.RequestMessage.RequestUri.AbsoluteUri;

                //Trace.WriteLine($"[updateBetburgerInfoDirectLink] step1: {mainReferer}");
                if (string.IsNullOrEmpty(mainReferer))
                    return false;

                string responseMessageMainString = responsemessageMain.Content.ReadAsStringAsync().Result;

                //Trace.WriteLine($"[updateBetburgerInfoDirectLink] step1: {responseMessageMainString}");
                if (string.IsNullOrEmpty(responseMessageMainString))
                    return false;

                GroupCollection groups = Regex.Match(responseMessageMainString, "direct_link = '(?<siteUrl>.*)';").Groups;
                if (groups == null || groups["siteUrl"] == null)
                    return false;

                string siteUrl = groups["siteUrl"].Value;
                if (string.IsNullOrEmpty(siteUrl))
                    return false;

                info.siteUrl = siteUrl;
                info.eventUrl = "";
                //Trace.WriteLine($"direct url: {info.siteUrl}");
                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"[updateBetburgerInfoDirectLink] exception: {e.Message} {e.StackTrace}");
                return false;
            }
        }

        private bool updateBetburgerInfoDirectLinkBet365(ref BetburgerInfo info)
        {
            try
            {
                HttpClient httpClient = getHttpClient();

                if (string.IsNullOrEmpty(info.eventUrl))
                    return false;

                httpClient.DefaultRequestHeaders.Remove("Origin");
                httpClient.DefaultRequestHeaders.Remove("Accept");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                httpClient.DefaultRequestHeaders.Remove("Connection");

                httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.betburger.com/arbs");
                HttpResponseMessage responseMessageMain = httpClient.GetAsync(info.eventUrl).Result;
                responseMessageMain.EnsureSuccessStatusCode();

                string mainReferer = responseMessageMain.RequestMessage.RequestUri.AbsoluteUri;
                if (string.IsNullOrEmpty(mainReferer) || mainReferer.Equals(info.eventUrl))
                    return false;

                string responseMessageMainString = responseMessageMain.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(responseMessageMainString))
                    return false;

                GroupCollection groups = Regex.Match(responseMessageMainString, "'<iframe height=\"100%\" src=\"(?<bet365Url>.*)\" width=\"100%\">").Groups;
                if (groups == null || groups["bet365Url"] == null)
                    return false;

                string bet365Url = groups["bet365Url"].Value;
                if (string.IsNullOrEmpty(bet365Url))
                    return false;

                bet365Url = WebUtility.HtmlDecode(bet365Url);
                info.siteUrl = bet365Url;

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private bool updateBetburgerInfoDirectLinkInterwetten(ref BetburgerInfo info)
        {
            try
            {
                HttpClient httpClient = getHttpClient();

                httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.betburger.com/arbs");
                HttpResponseMessage responsemessageMain = httpClient.GetAsync(info.eventUrl).Result;
                responsemessageMain.EnsureSuccessStatusCode();

                string mainReferer = responsemessageMain.RequestMessage.RequestUri.AbsoluteUri;
                if (string.IsNullOrEmpty(mainReferer))
                    return false;

                string responseMessageMainString = responsemessageMain.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(responseMessageMainString))
                    return false;

                GroupCollection groups = Regex.Match(responseMessageMainString, "direct_link = '(?<siteUrl>.*)';").Groups;
                if (groups == null || groups["siteUrl"] == null)
                    return false;

                string siteUrl = groups["siteUrl"].Value;
                if (string.IsNullOrEmpty(siteUrl))
                    return false;

                groups = Regex.Match(responseMessageMainString, "innerHTML = '<form action=\"(?<interwettenDOM>.*)\" method=\"POST\"").Groups;
                if (groups == null || groups["interwettenDOM"] == null)
                    return false;

                string interwettenDOM = groups["interwettenDOM"].Value;
                if (string.IsNullOrEmpty(interwettenDOM))
                    return false;

                interwettenDOM = WebUtility.HtmlDecode(interwettenDOM);
                interwettenDOM = WebUtility.UrlDecode(interwettenDOM);

                info.siteUrl = siteUrl;
                info.extra = interwettenDOM;

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private bool updateBetburgerInfoDirectLinkPlanet365(ref BetburgerInfo info)
        {
            try
            {
                HttpClient httpClient = getHttpClient();

                httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.betburger.com/arbs");
                HttpResponseMessage responsemessageMain = httpClient.GetAsync(info.eventUrl).Result;
                responsemessageMain.EnsureSuccessStatusCode();

                string mainReferer = responsemessageMain.RequestMessage.RequestUri.AbsoluteUri;
                if (string.IsNullOrEmpty(mainReferer))
                    return false;

                string responseMessageMainString = responsemessageMain.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(responseMessageMainString))
                    return false;

                GroupCollection groups = Regex.Match(responseMessageMainString, "direct_link = '(?<siteUrl>.*)';").Groups;
                if (groups == null || groups["siteUrl"] == null)
                    return false;

                string siteUrl = groups["siteUrl"].Value;
                if (string.IsNullOrEmpty(siteUrl))
                    return false;

                groups = Regex.Match(responseMessageMainString, "innerHTML = '(?<extra>.*)';").Groups;
                if (groups == null || groups["extra"] == null)
                    return false;

                string extra = groups["extra"].Value;
                if (string.IsNullOrEmpty(extra))
                    return false;

                extra = WebUtility.HtmlDecode(extra);
                extra = WebUtility.UrlDecode(extra);

                info.siteUrl = siteUrl;
                info.extra = extra;

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private bool updateBetburgerInfoDirectLinkPinnacle(ref BetburgerInfo info)
        {
            try
            {
                HttpClient httpClient = getHttpClient();

                httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.betburger.com/arbs");
                HttpResponseMessage responsemessageMain = httpClient.GetAsync(info.eventUrl + "&domain=pinnacle.com").Result;
                responsemessageMain.EnsureSuccessStatusCode();

                string mainReferer = responsemessageMain.RequestMessage.RequestUri.AbsoluteUri;
                if (string.IsNullOrEmpty(mainReferer))
                    return false;

                string responseMessageMainString = responsemessageMain.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(responseMessageMainString))
                    return false;

                GroupCollection groups = Regex.Match(responseMessageMainString, "'<form action=\"(?<pinnacleUrl>.*)\" method=\"POST\"").Groups;
                if (groups == null || groups["pinnacleUrl"] == null)
                    return false;

                string pinnacleUrl = groups["pinnacleUrl"].Value;
                if (string.IsNullOrEmpty(pinnacleUrl))
                    return false;

                info.siteUrl = pinnacleUrl;
                info.extra = string.Empty;

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private bool updateBetburgerInfoMarathon(ref BetburgerInfo info)
        {
            try
            {
                HttpClient httpClient = getHttpClient();

                httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.betburger.com/arbs");
                HttpResponseMessage responseMessageMain = httpClient.GetAsync(info.eventUrl).Result;
                responseMessageMain.EnsureSuccessStatusCode();

                string mainReferer = responseMessageMain.RequestMessage.RequestUri.AbsoluteUri;
                if (string.IsNullOrEmpty(mainReferer) || mainReferer.Equals(info.eventUrl))
                    return false;

                mainReferer = mainReferer.Replace("locale=en", "locale=de") + "www.marathonbet.co.uk";

                responseMessageMain = httpClient.GetAsync(mainReferer).Result;
                responseMessageMain.EnsureSuccessStatusCode();

                string responseMessageMainString = responseMessageMain.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(responseMessageMainString))
                    return false;

                GroupCollection groups = Regex.Match(responseMessageMainString, "direct_link = '(?<marathon>.*)';").Groups;
                if (groups == null || groups["marathon"] == null)
                    return false;

                string marathon = groups["marathon"].Value;
                if (string.IsNullOrEmpty(marathon))
                    return false;

                marathon = Regex.Replace(marathon, "uk/\\w\\w/addchoice", "uk/en/addchoice");

                info.siteUrl = marathon;

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private bool updateBetburgerInfoBwin(ref BetburgerInfo info)
        {
            try
            {
                HttpClient httpClient = getHttpClient();

                httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.betburger.com/arbs");
                HttpResponseMessage responseMessageMain = httpClient.GetAsync(info.eventUrl).Result;
                responseMessageMain.EnsureSuccessStatusCode();

                string mainReferer = responseMessageMain.RequestMessage.RequestUri.AbsoluteUri;
                if (string.IsNullOrEmpty(mainReferer) || mainReferer.Equals(info.eventUrl))
                    return false;

                string responseMessageMainString = responseMessageMain.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(responseMessageMainString))
                    return false;

                GroupCollection groups = Regex.Match(responseMessageMainString, "'<form action=\"(?<siteUrl>.*)\" method=\"POST\"").Groups;
                if (groups == null || groups["siteUrl"] == null)
                    return false;

                string siteUrl = groups["siteUrl"].Value;
                if (string.IsNullOrEmpty(siteUrl))
                    return false;

                siteUrl = WebUtility.HtmlDecode(siteUrl);

                groups = Regex.Match(responseMessageMainString, "<input id=\"ff3fix\" name=\"ff3fix\" type=\"hidden\" value=\"(?<ff3fix>\\d*)\" />").Groups;
                if (groups == null || groups["ff3fix"] == null)
                    return false;

                string ff3fix = groups["ff3fix"].Value;
                if (ff3fix == null)
                    return false;

                info.siteUrl = siteUrl;
                info.extra = ff3fix;

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private HttpClient getHttpClient()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = GameConstants.container;
            HttpClient httpClient = new HttpClient(handler);
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.110 Safari/537.36");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");

            return httpClient;
        }


        private BetburgerInfo getBetburgerInfoByBetId(JsonArbArb arbarb, JsonArbBet arbBet, List<JsonDirBetCombination> combines, string formula = null, double profit = 0)
        {
            if (arbarb == null || arbBet == null)
                return null;

            BetburgerInfo info = new BetburgerInfo();

            //if (arbarb.is_live)
            //    info.percent = (decimal)arbarb.middle_value;
            //else
                info.percent = (decimal)arbarb.percent;
//#if (FORSALE)
            
//#endif
            info.ROI = arbarb.roi;

            if (_dirInfo == null || _dirInfo.sports == null || _dirInfo.sports.Count < 1)
            {
                onWriteStatus("There is no directories infomation.");
                return null;
            }

            info.formula = formula;
            info.sport = _dirInfo.getSportById(arbarb.sport_id);
            info.bookmaker = _dirInfo.getBookmakerById(arbBet.bookmaker_id);
            info.color = arbarb.getColor();
            
            info.created = arbBet.getCreateat();
            info.updated = arbBet.getUpdatedat();
            
            info.updated_sec = arbBet.updated_at;
            info.league = arbBet.league;
            info.homeTeam = arbBet.home;
            info.awayTeam = arbBet.away;
            info.eventTitle = arbBet.getEventTitle();
            //info.eventUrl = arbBet.getEventUrl(arbarb.is_live);

            //string outcome = string.Empty, variation = string.Empty, value = null, market_id = string.Empty;
            //_dirInfo.getOutcomeById(arbBet.bc_id, ref outcome, ref variation, ref value, ref market_id , combines);
            //info.outcome = outcome;

            info.outcome = betburger_outcomes[arbBet.market_and_bet_type].ToString().Replace("%s", arbBet.market_and_bet_type_param);
            info.odds = arbBet.koef;
            info.commission = arbBet.commission;
            info.period = _dirInfo.getPeriodTitle(arbBet.period_id, arbarb.sport_id);
            info.arbId = arbBet.id;
            info.profit = profit;
            info.direct_link = arbBet.direct_link;
            info.started = arbBet.getStartat();
            if (info.bookmaker == "Bet365")
            {
                try
                {
                    info.direct_link = HttpUtility.UrlDecode(info.direct_link);
                    /*string[] directlinks = info.direct_link.Split(';');
                    string[] siteUrls = arbBet.bookmaker_event_direct_link.Split(';');
                    //Trace.WriteLine($"before direct link: {directlinks[0]} siteUrl: {siteUrls[0]}");
                    string[] linkArray = directlinks[0].Split('|');
                    if (linkArray.Count() == 3)
                    {
                        string fd = linkArray[2];
                        string i2 = linkArray[0];
                        string oddStr = linkArray[1];

                        //if (string.IsNullOrEmpty(fd) && !string.IsNullOrEmpty(arbBet.bookmaker_event_direct_link))
                        //{
                        //    fd = arbBet.bookmaker_event_direct_link;
                        //}

                        info.direct_link = string.Format("{0}|{1}|{2}", i2, oddStr, fd);
                        info.siteUrl = siteUrls[0];

                    }
                    else if (linkArray.Count() == 1)
                    {
                        string fd = "";
                        string i2 = "";
                        string oddStr = "";

                        if (string.IsNullOrEmpty(fd) && !string.IsNullOrEmpty(arbBet.bookmaker_event_direct_link))
                        {
                            fd = arbBet.bookmaker_event_direct_link;
                        }

                        if (string.IsNullOrEmpty(i2) && !string.IsNullOrEmpty(arbBet.bookmaker_league_id))
                        {
                            i2 = arbBet.bookmaker_league_id;
                        }

                        info.direct_link = string.Format("{0}|{1}|{2}", i2, oddStr, fd);
                    }*/

                    //Trace.WriteLine($"after direct link: {info.direct_link}");
                }
                catch { }


                if (GameConstants.betsapiEvents != null)
                {
                    ////
                    ///Checking with betsapi
                    try
                    {
                        string[] linkArray = info.direct_link.Split('|');
                        if (linkArray.Count() == 3 && info.sport != "Horse Racing")
                        {
                            string fd = linkArray[2];
                            string i2 = linkArray[0];
                            string oddStr = linkArray[1];

                            info.started = DateTime.Now.AddDays(100).ToString("MM-dd-yyyy HH:mm");
                            info.extra = $"Not matched game(not assigned with fd)";

                            bool bFound = false;
                            foreach (var sports in GameConstants.betsapiEvents)
                            {
                                foreach (var events in sports)
                                {
                                    foreach (var eventer in events)
                                    {

                                        try
                                        {
                                            if (eventer.isLive.ToString().ToLower() == "false")
                                            {
                                                long l_fd = Convert.ToInt64(fd);
                                                long l_eventerid = Convert.ToInt64(eventer.id.ToString());
                                                
                                                for (long l_iterator = l_fd - 2; l_iterator <= l_fd + 2; l_iterator++)
                                                {

                                                    if (l_iterator == l_eventerid)
                                                    {
                                                        info.started = arbBet.getStartat(eventer.time.ToString());
                                                        bFound = true;

                                                        if (info.league != eventer.league.name.ToString())
                                                        {
                                                            info.started = DateTime.Now.AddDays(100).ToString("MM-dd-yyyy HH:mm");
                                                            info.extra = $"Not matched game(assigned with fd) {info.league} vs {eventer.league.name.ToString()}";
                                                        }
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        catch { }

                                    }
                                    if (bFound)
                                        break;
                                }
                                if (bFound)
                                    break;
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }

            else if (info.bookmaker == "SportpesaIT" || info.bookmaker == "Sisal" || info.bookmaker == "Novibet")
            {
                info.direct_link = string.Format("{0}|{1}", arbBet.direct_link, arbBet.bookmaker_event_direct_link);
            }
            else if (info.bookmaker == "Betfair")
            {
                info.direct_link = string.Format("{0}|{1}", arbBet.direct_link, arbBet.bookmaker_event_direct_link);
            }
            else /*if (info.bookmaker == "BetMGM" || info.bookmaker == "Goldbetshop")*/
            {
                info.siteUrl = arbBet.bookmaker_event_direct_link;
            }
            info.raw_id = arbBet.raw_id;

            Debug.WriteLine(string.Format("{0} {1} {2}", info.eventTitle, info.percent, info.color));
            return info;
        }


        // Broadcast arb list
        private void processArbInfo(List<BetburgerInfo> infoList)
        {
            try
            {
                NetPacket data = new NetPacket();
                data.MsgCode = (ushort)NETMSG_CODE.NETMSG_ARBINFO;
                byte[] arbs = Utils.ObjectToByteArray(infoList);
                byte[] arbsZip = Utils.Compress(arbs);
                data.Append(arbsZip);

                onWriteStatus(string.Format("Broadcast {0} betburger surebet info list!", infoList.Count));
                //Trace.WriteLine(string.Format("Broadcast {0} betburger surebet info list!", infoList.Count));
                SendAllUser(data);

                foreach (BetburgerInfo info in infoList)
                {
                    if (!sentArbIDList.Contains(info.arbId))
                    {
                        sentArbIDList.Add(info.arbId);
                        DBManager.Insert_Pick_History(info);
                    }
                }
            }
            catch(Exception e)
            {

            }
        }

        public List<string> sentArbIDList = new List<string>();

        //Broadcast valuebets list
        public void processValuesInfo(List<BetburgerInfo> infoList)
        {
            try
            {
                NetPacket data = new NetPacket();
                data.MsgCode = (ushort)NETMSG_CODE.NETMSG_VALUEINFO;
                byte[] arbs = Utils.ObjectToByteArray(infoList);
                byte[] arbsZip = Utils.Compress(arbs);
                data.Append(arbsZip);

                
                //Trace.WriteLine(string.Format("Broadcast {0} betburger value info list!", infoList.Count));
                SendAllUser(data);

                foreach (BetburgerInfo info in infoList)
                {
                    if (!sentArbIDList.Contains(info.arbId))
                    {
                        sentArbIDList.Add(info.arbId);
                        DBManager.Insert_Pick_History(info);
                    }
                }
                onWriteStatus(string.Format("Broadcast {0} betburger value info list Finished", infoList.Count));
            }
            catch (Exception e)
            {

            }
        }

        public void ProcClientMessage(Object param1, Object param2)
        {
            try
            {
                NetPacket netdata = (NetPacket)param1;
                UserInfo client = (UserInfo)param2;

                string clientMessage = netdata.Pop() as string;

                onWriteStatus($"Client_Message id: {client.GameID} license: {client.License} Message: {clientMessage}");
            }
            catch (Exception e)
            {
                Trace.WriteLine("[Exception : ProcSaveBet] " + e.ToString());
            }
        }

        public void ProcUpdateBalance(Object param1, Object param2)
        {
            try
            {
                NetPacket netdata = (NetPacket)param1;
                UserInfo client = (UserInfo)param2;

                double balance = (double)netdata.Pop();                
                string info = netdata.Pop() as string;

                onWriteStatus($"Client_Balance id: {client.GameID} license: {client.License} Balance: {balance} info: {info}");
                client.Balance = balance;
                DBManager.Insert_Balance_History(client.UUID, client.GameID, balance, info);

                ServerEventListener(SERVER_EVENT.ONUSERLOGIN, client);
            }
            catch (Exception e)
            {
                Trace.WriteLine("[Exception : ProcSaveBet] " + e.ToString());
            }
        }

        public void ProcSaveBet(Object param1, Object param2)
        {
            try
            {
                NetPacket netdata = (NetPacket)param1;
                UserInfo client = (UserInfo)param2;

                byte[] infoListArray = netdata.Pop() as byte[];
                PlacedBetInfo betInfo = Converter.GetInstance().ByteArrayToObject(infoListArray) as PlacedBetInfo;

                DBManager.Insert_Bet_History(client.UUID, client.GameID, betInfo.arbID, betInfo.balance, betInfo.outcome, betInfo.sport, betInfo.homeTeam, betInfo.awayTeam, betInfo.league, (double)betInfo.percent, betInfo.odds, betInfo.stake, betInfo.reserve, betInfo.reserve1, betInfo.pendingBets);
                
            }
            catch (Exception e)
            {
                Trace.WriteLine("[Exception : ProcSaveBet] " + e.ToString());
            }

        }
        // Set License Key
        public void ProcLicenseCheck(Object param1, Object param2)
        {
            //Trace.WriteLine("ProcLicenseCheck: 0");
            NetPacket netdata = (NetPacket)param1;
            UserInfo client = (UserInfo)param2;
            try
            {

                UInt32 nVersion = (UInt32)netdata.Pop();                
                string client_license = netdata.Pop() as string;
                string client_bookmaker = netdata.Pop() as string;
                string client_gameid = netdata.Pop() as string;
                string client_gamepwd = "";
                try
                {
                    client_gamepwd = netdata.Pop() as string;
                }
                catch{}
                                
                if (string.IsNullOrEmpty(client_license))
                {
                    onWriteStatus("License key errors!");
                    return;
                }
                
                //Trace.WriteLine("ProcLicenseCheck: 1");
                if (CheckDoubleLicense(client_license))
                {
                    onWriteStatus(string.Format("Doubled License key {0}!", client_license));

                    NetPacket newnetdata = new NetPacket();
                    newnetdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_LOGINRESULT;
                    newnetdata.Append(1);

                    //Trace.WriteLine("ProcLicenseCheck 1 SendData Before");
                    client.SendData(newnetdata);
                    //Trace.WriteLine("ProcLicenseCheck 1 SendData After");

                    RemoveClient(client);
                    return;
                }
                //Trace.WriteLine("ProcLicenseCheck: 2");                
                string[][] result = DBManager.Select_license(client_license);
                if (result == null || result.Length < 1)
                {
                    onWriteStatus("Can't find this license : " + client_license);

                    NetPacket newnetdata = new NetPacket();
                    newnetdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_LOGINRESULT;
                    newnetdata.Append(2);
                    //Trace.WriteLine("ProcLicenseCheck 2 SendData Before");
                    client.SendData(newnetdata);
                    //Trace.WriteLine("ProcLicenseCheck 2 SendData After");

                    RemoveClient(client);
                    return;
                }
                //Trace.WriteLine("ProcLicenseCheck: 3");
                string license_uuid = result[0][0];
                string create_user_id = result[0][1];
                string site_id = result[0][2];
                string account_username = result[0][4];
                DateTime finished_at = DateTime.Parse(result[0][7]);
                bool IsActivated = bool.Parse(result[0][8]);
                bool IsDeleted = bool.Parse(result[0][9]);
                //added in new version
                //bool IsQRScanActivated = bool.Parse(result[0][10]);
                //int nQRScanCount = int.Parse(result[0][11]);

                //string Privillage = result[0][13];
                //string CreatorUsername = result[0][14];
                //string bookmaker = result[0][28];

                bool IsQRScanActivated = false;
                int nQRScanCount = 0;
                string Privillage = result[0][14];
                string CreatorUsername = result[0][12];
                string bookmaker = result[0][28];
                //string bookmaker = result[0][26];

                foreach (string str in result[0])
                {
                    try
                    {
                        onWriteStatus(str);
                    }
                    catch { }
                }

                if (string.IsNullOrEmpty(license_uuid))
                {
                    onWriteStatus("Can't get user id from license! " + client_license);

                    NetPacket newnetdata = new NetPacket();
                    newnetdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_LOGINRESULT;
                    newnetdata.Append(3);

                    //Trace.WriteLine("ProcLicenseCheck 3 SendData Before");
                    client.SendData(newnetdata);
                    //Trace.WriteLine("ProcLicenseCheck 3 SendData After");

                    if (client.Status == USERSTATUS.NOLOGIN_STATUS)
                    {
                        RemoveClient(client);
                        return;
                    }

                    RemoveClient(client);
                    return;
                }
                if (IsDeleted)
                {
                    onWriteStatus(string.Format("{0} is deleted already", client_license));
                    NetPacket newnetdata = new NetPacket();
                    newnetdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_LOGINRESULT;
                    newnetdata.Append(4);
                    
                    client.SendData(newnetdata);                   

                    RemoveClient(client);
                    return;
                }
                if (!IsActivated)
                {
                    onWriteStatus(string.Format("{0} is deactivated already", client_license));

                    NetPacket newnetdata = new NetPacket();
                    newnetdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_LOGINRESULT;
                    newnetdata.Append(5);
                                        
                    client.SendData(newnetdata);                   

                    RemoveClient(client);
                    return;
                }
                if (client_bookmaker.Trim().ToLower() != bookmaker.Trim().ToLower())
                {
                    onWriteStatus($"{client_license} bookmaker dismatch (bot:{client_bookmaker} <> license:{bookmaker})");

                    NetPacket newnetdata = new NetPacket();
                    newnetdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_LOGINRESULT;
                    newnetdata.Append(6);
                    newnetdata.Append(bookmaker);
                    client.SendData(newnetdata);

                    RemoveClient(client);
                    return;
                }
                if (!string.IsNullOrEmpty(account_username) && account_username != client_gameid)
                {
                    onWriteStatus($"{client_license} This license is already assigned to({account_username}) but bot_gameid: {client_gameid}");

                    NetPacket newnetdata = new NetPacket();
                    newnetdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_LOGINRESULT;
                    newnetdata.Append(7);
                    newnetdata.Append(account_username);
                    client.SendData(newnetdata);

                    RemoveClient(client);
                    return;
                }
                //Trace.WriteLine("ProcLicenseCheck: 5");
                
                if (finished_at < DateTime.Now)
                {
                    onWriteStatus($"{client_license} is Out of Date! finished at {finished_at}");

                    NetPacket newnetdata = new NetPacket();
                    newnetdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_LOGINRESULT;
                    newnetdata.Append(8);                    
                    //Trace.WriteLine("ProcLicenseCheck 4 SendData Before");
                    client.SendData(newnetdata);
                    //Trace.WriteLine("ProcLicenseCheck 4 SendData After");

                    RemoveClient(client);
                    return;
                }
                
                if (nVersion < cServerSettings.GetInstance().Version)
                {
                    onWriteStatus(string.Format("{1} Version is low! client Version: {0}!", nVersion, client_license));

                    NetPacket newnetdata = new NetPacket();
                    newnetdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_LOGINRESULT;
                    newnetdata.Append(9);

                    //Trace.WriteLine("ProcLicenseCheck 5 SendData Before");
                    client.SendData(newnetdata);
                    //Trace.WriteLine("ProcLicenseCheck 6 SendData After");

                    RemoveClient(client);
                    return;
                }
                
                //set user id from license key
                UpdateUserById(client, license_uuid, CreatorUsername, Privillage, client_license, client_gameid, finished_at, bookmaker, IsQRScanActivated);
                ServerEventListener(SERVER_EVENT.ONUSERLOGIN, client);
                if (string.IsNullOrEmpty(account_username))
                {
                    //assign account to the license
                    DBManager.Update_license_account(license_uuid, client_gameid);                    
                }
                //
                //Trace.WriteLine(string.Format("ProcLicenseCheck: 8 {0}", gameid));
                onWriteStatus($"Bot Login license {client_license} by {Privillage}({CreatorUsername}) game_id: {client_gameid}  game_pwd: {client_gamepwd} bookmaker: {client_bookmaker} finished_at: {finished_at} version: {nVersion} qr_able: {IsQRScanActivated} qr_count: {nQRScanCount}");
                //Trace.WriteLine("ProcLicenseCheck: 9");

                //insert login history
                DBManager.Insert_login_history(license_uuid, site_id, client_gameid);
                                                

                NetPacket resnetdata = new NetPacket();
                resnetdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_LOGINRESULT;
                resnetdata.Append(0);                
                client.SendData(resnetdata);


            }
            catch (Exception e)
            {
                onWriteStatus(string.Format("License Parsing error! old version ex: {0}", e));

                try
                {
                    NetPacket newnetdata = new NetPacket();
                    newnetdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_LOGINRESULT;
                    newnetdata.Append(6);

                    //Trace.WriteLine("ProcLicenseCheck 8 SendData Before");
                    client.SendData(newnetdata);
                    //Trace.WriteLine("ProcLicenseCheck 8 SendData After");

                    RemoveClient(client);
                }
                catch { }
                return;
            }
        }
    }
}
