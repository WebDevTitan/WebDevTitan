using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using System.Net.Http.Headers;
using System.IO;
using System.Reflection;
using WebSocketSharp;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Network;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Page;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Runtime;
using MasterDevs.ChromeDevTools;
using Newtonsoft.Json.Linq;
using MasterDevs.ChromeDevTools.Protocol.Chrome.DOM;
using Newtonsoft.Json;
using WebSocketSharp.Server;

namespace Bet365LiveAgent.Logics
{
    [Serializable]
    public class B365ManifestItems
    {
        public string e;
        public string p;
        public string t;
        public string m;
        public string o;
    }

    [Serializable]
    public class B365ServiceRules
    {
        public B365ManifestItems[] manifest;
    }

    [Serializable]
    public class B365CW
    {
        public string V;
        public string M;
    }

    [Serializable]
    public class B365WebsiteConfig
    {
        public long SERVER_TIME;
        public string BLOB_LOCATION;
        public string SITE_CONFIG_LOCATION;
        public string API_LOCATION;
        public string BETS_WEBAPI_LOCATION;
        public string CLIENT_ERROR_LOCATION;
        public string[] CONNECTION_DETAILS;
        public string[] PRIVATE_CONNECTION_DETAILS;
        public bool IS_TLS_FORCED;
        public bool RS;
        public string HTTPS_HOST;
        public int IDLE_TIMER;
        public string GRCK;
        public string GRCEK;
        public B365ServiceRules SERVICE_RULES;
        public string CWBV;
        public string CWBM;
        public B365CW[] CW;
        public string SSI;
        public string SST;
    }

    class ClientWebSocket : WebSocketBehavior
    {
        public ClientWebSocket()
        {
        }

        public ClientWebSocket(string prefix)
        {

        }

        protected override void OnClose(CloseEventArgs e)
        {
            Trace.WriteLine("[WebSocket:OnClose]");
            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, $"[WebSocket:OnClose] {e.Reason}");

            Bet365ClientManager.bWebSocketConnected = false;
        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            Trace.WriteLine("[WebSocket:OnError]");
            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, $"[WebSocket:OnError] {e.Message}");

            Bet365ClientManager.bWebSocketConnected = false;
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            Bet365ClientManager.bWebSocketConnected = true;


#if (TROUBLESHOT)
            //Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, ($"[WebSocket:OnMessage] {e.Data}");
#endif

            try
            {
                var responseJson = JsonConvert.DeserializeObject<dynamic>(e.Data);

                if (responseJson.type.ToString() == "scriptresult")
                {
                    Bet365ClientManager.wait_EvalResult = responseJson.response.ToString();
                    Bet365ClientManager.wait_EvalResultEvent.Set();
                }
                else if (responseJson.type.ToString() == "websocketdata")
                {
                    if (Bet365AgentManager.Instance.OnBet365DataReceived != null)
                        Bet365AgentManager.Instance.OnBet365DataReceived(responseJson.data.ToString());
                    
                }
            }
            catch { }
        }

        protected override void OnOpen()
        {
            Trace.WriteLine("[WebSocket:OnOpen]");
            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, $"[WebSocket:OnOpen]");

            Bet365ClientManager.bWebSocketConnected = true;
        }
    }
    class Bet365ClientManager
    {
        public static bool bWebSocketConnected = false;
        public static ManualResetEventSlim wait_EvalResultEvent = new ManualResetEventSlim();
        public static string wait_EvalResult = string.Empty;

        private Thread _runner = null;

        private BET365CLIENT_STATUS _status = BET365CLIENT_STATUS.Disconnected;
        public BET365CLIENT_STATUS Status
        {
            get { return _status; }
        }

        private HttpClient _httpClient = null;
        private HttpClient _httpMobileClient = null;
        private CookieContainer _cookieContainer = null;
        private Bet365StreamClient _streamClient = null;
        private Bet365PrivateStreamClient _privateStreamClient = null;
        IChromeProcess chromeProcess = null;
        public IChromeSession chromeSession = null;
        bool isWaitingForPageLoad = false;
        long executionContextId = -1;
        bool isWaitingForLoginResult = false;
        public bool bLogged = false;
        bool isWaitingForAPI = false;
        string RespBody = string.Empty;


        private string _webSocketHost = "premws-pt3.365lpodds.com"; // default value
        public string WebSocketHost
        {
            get { return _webSocketHost; }
            set { _webSocketHost = value; }
        }
        private string _webSocketPort = "443"; // default value
        public string WebSocketPort
        {
            get { return _webSocketPort; }
            set { _webSocketPort = value; }
        }
        private string _privateWebSocketHost = "pshudws.365lpodds.com"; // default value
        public string PrivateWebSocketHost
        {
            get { return _privateWebSocketHost; }
            set { _privateWebSocketHost = value; }
        }
        private string _privateWebSocketPort = "443"; // default value
        public string PrivateWebSocketPort
        {
            get { return _privateWebSocketPort; }
            set { _privateWebSocketPort = value; }
        }
        private string _cookieToken = string.Empty;
        private string _XNST = string.Empty;
        public string CookieToken 
        {
            get { return _cookieToken; }
            set { _cookieToken = value; }
        }
        public string XNSTToken
        {
            get { return _XNST; }
            set { _XNST = value; }
        }

        private int _emptyMessageTick = 0;
        private long _elapsedTimeTick = 0;

        // begin DEPRECATED 202202 as said from previous Developer
        private string _nstToken = null;
        public string NSTToken
        {
            get { return _nstToken; }
        }
        private string _nstTokenLib = null;
        // end DEPRECATED 202202 as said from previous Developer



        public Bet365DataSendHandler OnBet365DataSend = null;
        public Bet365DataSendHandler OnBet365PrivateDataSend = null;

        public Bet365DataReceivedHandler OnBet365DataReceived = null;
        public Bet365DataReceivedHandler OnBet365PrivateDataReceived = null;

        public Bet365RequestDelegate OnBet365Request = null;

        private static Bet365ClientManager _instance = null;
        public static Bet365ClientManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Bet365ClientManager();
                return _instance;
            }
        }

        public Bet365ClientManager()
        {
            OnBet365DataReceived = ReceiveBet365Data;
            ServicePointManager.DefaultConnectionLimit = 2;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            _streamClient = new Bet365StreamClient();
            _streamClient.OnConnected = OnStreamClientConnected;
            _streamClient.OnDisconnected = OnStreamClientDisconnected;
            _privateStreamClient = new Bet365PrivateStreamClient();
            _privateStreamClient.OnConnected = OnPrivateStreamClientConnected;
            _privateStreamClient.OnDisconnected = OnPrivateStreamClientDisconnected;

            OnBet365Request = OnBet365RequestHandler;
        }

        public void Start()
        {
            try
            {
                if (_runner == null)
                {
                    Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, "Bet365ClientManager Started.");
                    _status = BET365CLIENT_STATUS.Connecting;
                    _runner = new Thread(() => Run());
                    _runner.Start();

                    OnBet365DataSend = SendBet365Data;
                    OnBet365PrivateDataSend = SendBet365PrivateData;
                    
                    OnBet365PrivateDataReceived = ReceiveBet365PrivateData;
                }
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, ex.ToString());
            }
        }

        public void Stop()
        {
            try
            {
                if (_runner != null)
                {
                    Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, "Bet365ClientManager Stoped.");
                    _status = BET365CLIENT_STATUS.Disconnected;
                    _runner.Join();
                    _runner = null;

                    _streamClient.Disconnect();
                    _privateStreamClient.Disconnect();

                    OnBet365DataSend = null;
                    OnBet365PrivateDataSend = null;
                    OnBet365DataReceived = null;
                    OnBet365PrivateDataReceived = null;
                }
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, ex.ToString());
            }
        }

        public async void Run()
        {
            try
            {
                RunScript("location.reload();");
                int k = 0;
                while (Utils.bRun)
                {
                    //onWriteStatus("Checking bet365 Socket Connection");
                    CheckSocket(k);
                    k++;
                    Thread.Sleep(2000);
                }
            }
            catch(Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, ex.Message);
            }
        }

        private void SendBet365Data(string strData)
        {
            try
            {
                _streamClient.Send(strData);
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, ex.ToString());
            }
        }

        private void SendBet365PrivateData(string strData)
        {
            try
            {
                _privateStreamClient.Send(strData);
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, ex.ToString());
            }
        }

        private void ReceiveBet365Data(string strData)
        {
            try
            {
                if (Bet365AgentManager.Instance.OnBet365DataReceived != null)
                    Bet365AgentManager.Instance.OnBet365DataReceived(strData);

                if (string.IsNullOrWhiteSpace(strData))
                    _emptyMessageTick++;
                else
                    _emptyMessageTick = 0;

                _elapsedTimeTick = DateTime.Now.Ticks;
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, ex.ToString());
            }
        }

        private void ReceiveBet365PrivateData(string strData)
        {
            try
            {
                if (strData.StartsWith($"{Global.DELTA}S_{_cookieToken}_SPTBK_D23{Global.DELIM_RECORD}"))
                {
                    string encNstToken = strData.TrimStart($"{Global.DELTA}S_{_cookieToken}_SPTBK_D23{Global.DELIM_RECORD}");
                    string commandPacket = $"{Global.DELIM_FIELD}{Global.NONE_ENCODING}command{Global.DELIM_RECORD}nst{Global.DELIM_RECORD}{_nstToken}{Global.DELIM_FIELD}SPTBK";
                    if (Bet365ClientManager.Instance.OnBet365DataSend != null)
                        Bet365ClientManager.Instance.OnBet365DataSend(commandPacket);
                }

                if (Bet365AgentManager.Instance.OnBet365PrivateDataReceived != null)
                    Bet365AgentManager.Instance.OnBet365PrivateDataReceived(strData);
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, ex.ToString());
            }
        }

        private void OnStreamClientConnected()
        {
            if (_status != BET365CLIENT_STATUS.Connected)
            {
                _status = BET365CLIENT_STATUS.Connected;
            }
        }

        private void OnStreamClientDisconnected()
        {
            if (_status != BET365CLIENT_STATUS.Disconnected)
            {
                _status = BET365CLIENT_STATUS.Reconnecting;
            }
        }

        private void OnPrivateStreamClientConnected()
        {
        }

        private void OnPrivateStreamClientDisconnected()
        {
        }

        private void OnBet365RequestHandler(string key, object value)
        {
            if ("Bet365Response".Equals(key))
            {
                string strResponse = value.ToString();
                string strWebSockInfo = "\"CONNECTION_DETAILS\":\\[\"wss://(?<Host1>[^\\,]*),(?<Port1>[^\\,]*),3\",\"https://(?<Host2>[^\\,]*),(?<Port2>[^\\,]*),2\"],";
                string strPrivateWebSockInfo = "\"PRIVATE_CONNECTION_DETAILS\":\\[\"wss://(?<Host1>[^\\,]*),(?<Port1>[^\\,]*),3\",\"https://(?<Host2>[^\\,]*),(?<Port2>[^\\,]*),2\"],";
                Match match = Regex.Match(strResponse, strWebSockInfo);
                _webSocketHost = match.Groups["Host1"].Value;
                _webSocketPort = match.Groups["Port1"].Value;
                match = Regex.Match(strResponse, strPrivateWebSockInfo);
                _privateWebSocketHost = match.Groups["Host1"].Value;
                _privateWebSocketPort = match.Groups["Port1"].Value;
            }
            else if ("NSTToken".Equals(key))
            {
                _nstToken = value.ToString();
            }
        }

        //public async Task InitBrowser()
        //{
        //    string chromePath = "";
        //    if (File.Exists("chromePath.txt"))
        //        chromePath = File.ReadAllText("chromePath.txt");

        //    string user_dir = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
        //    ChromeProcessFactory chromeProcessFactory = new ChromeProcessFactory(new StubbornDirectoryCleaner());
        //    chromeProcessFactory.ChromePath = chromePath;
        //    chromeProcess = chromeProcessFactory.Create(9222, false, "", user_dir);
        //    var sessionInfo = (await chromeProcess.GetSessionInfo()).LastOrDefault();

        //    var chromeSessionFactory = new ChromeSessionFactory();
        //    chromeSession = chromeSessionFactory.Create(sessionInfo.WebSocketDebuggerUrl);
        //    var domEnableResult = chromeSession.SendAsync<MasterDevs.ChromeDevTools.Protocol.Chrome.DOM.EnableCommand>().Result;
        //    Console.WriteLine("DomEnable: " + domEnableResult.Id);

        //    var networkEnableResult = chromeSession.SendAsync<MasterDevs.ChromeDevTools.Protocol.Chrome.Network.EnableCommand>().Result;
        //    Console.WriteLine("NetworkEnable: " + networkEnableResult.Id);

        //    var pageEnableResult = chromeSession.SendAsync<MasterDevs.ChromeDevTools.Protocol.Chrome.Page.EnableCommand>().Result;
        //    Console.WriteLine("PageEnable: " + pageEnableResult.Id);

        //    var runTimeEnableResult = chromeSession.SendAsync<MasterDevs.ChromeDevTools.Protocol.Chrome.Runtime.EnableCommand>().Result;
        //    Console.WriteLine("RunTime: " + runTimeEnableResult.Id);

        //    var navigateResponse = chromeSession.SendAsync(new NavigateCommand
        //    {
        //        Url = $"https://www.{Config.Instance.Bet365Domain}/"
        //    }).Result;

        //    Console.WriteLine("NavigateResponse: " + navigateResponse.Result.FrameId);

        //    chromeSession.Subscribe<FrameStoppedLoadingEvent>(e =>
        //    {


        //        // Console.WriteLine("Frame Loaded" + e.FrameId);
        //    });

        //    chromeSession.Subscribe<RequestWillBeSentEvent>(e =>
        //    {
        //        string requestUrl = e.Request.Url.ToLower();
        //        if (requestUrl.Contains("uicountersapi"))
        //            if (isWaitingForPageLoad) isWaitingForPageLoad = false;
        //    });

        //    chromeSession.Subscribe<WebSocketFrameReceivedEvent>(e =>
        //    {
        //        string response = e.Response.PayloadData;
        //        if (Bet365AgentManager.Instance.OnBet365DataReceived != null)
        //            Bet365AgentManager.Instance.OnBet365DataReceived(response);

        //        Console.WriteLine(response);
        //    });

        //    chromeSession.Subscribe<ExecutionContextCreatedEvent>(e =>
        //    {
        //        Task.Run(async () =>
        //        {
        //            var auxData = e.Context.AuxData as JObject;
        //            var frameId = auxData["frameId"].Value<string>();
        //            if (e.Context.Origin.Contains($"https://www.{Config.Instance.Bet365Domain}") && frameId == navigateResponse.Result.FrameId)
        //            {
        //                executionContextId = e.Context.Id;
        //                var injectResult = (await chromeSession.SendAsync(new AddScriptToEvaluateOnNewDocumentCommand() { Source = File.ReadAllText("inject.js") }));
        //            }
        //        });
        //    });

        //    chromeSession.Subscribe<ResponseReceivedEvent>(e =>
        //    {
        //        Task.Run(async () =>
        //        {
        //            var url = e.Response.Url;
        //            if (url.ToLower().Contains("defaultapi/sports-configuration"))
        //            {
        //                var result = (await chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
        //                string res = result.Body;

        //                if (isWaitingForLoginResult) isWaitingForLoginResult = false;
        //            }
        //            else if (url.ToLower().Contains("recaptcha/enterprise"))
        //                isWaitingForAPI = true;
        //            else if (url.Contains("uicountersapi"))
        //                isWaitingForAPI = true;
        //            else if (url.Contains("recaptcha/enterprise"))
        //                isWaitingForAPI = true;
        //            else if (url.ToLower().Contains("/betswebapi"))
        //            {
        //                if (isWaitingForAPI) isWaitingForAPI = false;

        //                var result = (await chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
        //                RespBody = result.Body;
        //            }
        //        });
        //    });


        //    chromeSession.Subscribe<LoadEventFiredEvent>(loadEventFired =>
        //    {
        //        // we cannot block in event handler, hence the task
        //        Task.Run(async () =>
        //        {
        //            long documentNodeId = (await chromeSession.SendAsync(new GetDocumentCommand())).Result.Root.NodeId;
        //            var injectResult = (await chromeSession.SendAsync(new AddScriptToEvaluateOnNewDocumentCommand() { Source = File.ReadAllText("inject.js") }));
        //            isWaitingForPageLoad = false;
        //        });
        //    });

        //    Thread.Sleep(10000);
        //}
           

        object evalLocker = new object();

        public bool SendEvalCommand(string type, string message)
        {
            if (string.IsNullOrEmpty(type))
                return true;

#if (TROUBLESHOT)
            //LogMng.Instance.onWriteStatus($"SendEvalCommand {message}");
#endif
            string request = $"{{\"type\":\"{type}\", \"body\":\"{message}\"}}";
            if (Global.socketServer != null)
            {
                try
                {
                    Global.socketServer.WebSocketServices.Broadcast(request);
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine("BaccaratA WebSocketServer broadcasting error : " + ex);
                }
            }
            return true;
        }
        private string Page_Evaluate(string command, string expression)
        {
            if (Global.socketServer.WebSocketServices.SessionCount <= 0)
            {
#if (TROUBLESHOT)
                //LogMng.Instance.onWriteStatus("WebSocket is not connected");
#endif
                return string.Empty;
            }

            Monitor.Enter(evalLocker);
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"Page_Evaluate Req  cmd : {command} exp: {expression}");
#endif
            wait_EvalResultEvent.Reset();
            wait_EvalResult = string.Empty;
            SendEvalCommand(command, expression);

            if (!wait_EvalResultEvent.Wait(1000))
            {
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"Page_Evaluate No Response");
#endif
                Monitor.Exit(evalLocker);
                return string.Empty;
            }
#if (TROUBLESHOT)
            LogMng.Instance.onWriteStatus($"Page_Evaluate Res : {wait_EvalResult}");
#endif
            Monitor.Exit(evalLocker);
            return wait_EvalResult;
        }


        private string RunScript(string param)
        {
#if (TROUBLESHOT)
            //LogMng.Instance.onWriteStatus($"RunScript: {param}");
#endif
            string result = "";
            try
            {
                result = Page_Evaluate("script", param);
            }
            catch { }
            return result;
        }

        public void SendRequestToSocket(string request)
        {
            try
            {
                RunScript($"Locator.subscriptionManager._streamDataProcessor.subscribe('{request}')");
                
            }
            catch
            {

            }
        }

        public bool CheckSocket(int firstTime)
        {
            try
            {
                string curUrl = RunScript("ns_navlib_util.WebsiteNavigationManager.CurrentPageData");

                if (curUrl != "#IP#B1")
                {
                    string command = $"var e = {{needsCard:false}}; ns_navlib_util.WebsiteNavigationManager.Instance.navigateTo('#IP#B1', e, false); ";
                    RunScript(command);
                }
              
            }
            catch { }
            return true;

        }
        public async Task<bool> Navigate(string eventUrl)
        {
            var results = await chromeSession.SendAsync(new NavigateCommand
            {
                Url = eventUrl
            });

            return true;
        }
    }
}
