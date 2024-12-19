using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Protocol;
using WebSocketSharp;

namespace Project.Bookie
{
#if (SEUBET)
    class SetbetCtrl : IBookieController
    {
        private WebSocket _webSocket = null;
        string m_userId = null;
        double m_balance = -1;
        public SetbetCtrl()
        {
            startListening();
        }
        public void startListening()
        {
            try
            {
                LogMng.Instance.onWriteStatus("Seubet Socket is start listening!");

                // Since QUIC is not supported directly, keep the TLS settings
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Initialize WebSocket
                _webSocket = new WebSocket("wss://eu-swarm-springre.deimosphobos.net/");
                _webSocket.Origin = "https://seubet.com/";
                _webSocket.OnOpen += Socket_OnOpen;
                _webSocket.OnMessage += Socket_OnMessage;
                _webSocket.OnClose += Socket_OnClose;
                _webSocket.OnError += Socket_OnError;

                try
                {
                    _webSocket.Connect();

                }
                catch (WebSocketException wsEx)
                {
                    LogMng.Instance.onWriteStatus($"WebSocketException: {wsEx.Message}");
                    //Task.Delay(5000).ContinueWith(_ => startListening()); // Retry after 5 seconds
                }
                catch (Exception ex)
                {
                    LogMng.Instance.onWriteStatus($"Exception in Connect: {ex.Message}");
                    //Task.Delay(5000).ContinueWith(_ => startListening()); // Retry after 5 seconds
                }
                // Monitor the connection in a background task
                //Task.Run(() => MonitorConnection());
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"Exception in startListening: {ex.Message}");
            }
        }
        private void Socket_OnOpen(object sender, EventArgs e)
        {
            LogMng.Instance.onWriteStatus("Socket_OnOpen");
            sendRequestSession();
        }
        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                JObject jsonData = JObject.Parse(e.Data);
                Console.WriteLine(e.Data);
                if (jsonData["data"]?["authentication_status"] != null && jsonData["data"]["authentication_status"].ToString() == "0")
                {
                    if (jsonData["data"]?["user_id"] != null)
                    {
                        m_userId = jsonData["data"]["user_id"].ToString();
                        LogMng.Instance.onWriteStatus($"User ID: {m_userId}");
                    }
                    LogMng.Instance.onWriteStatus("Login Success");

                    //sendPlaceBetMessage();
                }

                if (e.Data.Contains("balance"))
                {
                    var profileData = jsonData["data"]?["data"]?["profile"];
                    if (profileData != null)
                    {
                        foreach (var profile in profileData)
                        {
                            string balance = profile.First?["balance"]?.ToString();
                            if (balance != null)
                            {
                                m_balance = Convert.ToDouble(balance);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus("Exception in socket message: " + ex.ToString());
                LogMng.Instance.onWriteStatus($"Data received: {e.Data}");
            }
        }
        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            LogMng.Instance.onWriteStatus("WebSocket closed");

            // Log close status and reason
            LogMng.Instance.onWriteStatus($"Close Status: {e.Code}");
            LogMng.Instance.onWriteStatus($"Close Reason: {e.Reason}");

            // Determine the cause of the closure
            switch (e.Code)
            {
                case 1000:
                    LogMng.Instance.onWriteStatus("Normal closure");
                    break;
                case 1001:
                    LogMng.Instance.onWriteStatus("Going away");
                    break;
                case 1002:
                    LogMng.Instance.onWriteStatus("Protocol error");
                    break;
                case 1003:
                    LogMng.Instance.onWriteStatus("Unsupported data");
                    break;
                case 1005:
                    LogMng.Instance.onWriteStatus("No status received");
                    break;
                case 1006:
                    LogMng.Instance.onWriteStatus("Abnormal closure");
                    break;
                case 1007:
                    LogMng.Instance.onWriteStatus("Invalid frame payload data");
                    break;
                case 1008:
                    LogMng.Instance.onWriteStatus("Policy violation");
                    break;
                case 1009:
                    LogMng.Instance.onWriteStatus("Message too big");
                    break;
                case 1010:
                    LogMng.Instance.onWriteStatus("Missing extension");
                    break;
                case 1011:
                    LogMng.Instance.onWriteStatus("Internal server error");
                    break;
                case 1012:
                    LogMng.Instance.onWriteStatus("Service restart");
                    break;
                case 1013:
                    LogMng.Instance.onWriteStatus("Try again later");
                    break;
                case 1014:
                    LogMng.Instance.onWriteStatus("Bad gateway");
                    break;
                case 1015:
                    LogMng.Instance.onWriteStatus("TLS handshake failure");
                    break;
                default:
                    LogMng.Instance.onWriteStatus("Unknown reason");
                    break;
            }
            Task.Delay(5000).ContinueWith(_ => startListening()); // Retry after 5 seconds
        }
        private void Socket_OnError(object sender, ErrorEventArgs e)
        {
            LogMng.Instance.onWriteStatus("Socket_OnError");
            LogMng.Instance.onWriteStatus(e.Message.ToString());
            Task.Delay(5000).ContinueWith(_ => startListening()); // Retry after 5 seconds
        }
        private async Task MonitorConnection()
        {
            while (true)
            {
                if (_webSocket != null && _webSocket.ReadyState != WebSocketState.Open)
                {
                    LogMng.Instance.onWriteStatus("WebSocket connection lost. Attempting to reconnect...");
                    await Task.Delay(1000);
                    startListening();
                    break;
                }
                await Task.Delay(100);
            }
        }
        private void sendRequestSession()
        {
            JObject jSessionPayload = new JObject();
            jSessionPayload["command"] = "request_session";
            jSessionPayload["params"] = new JObject();
            jSessionPayload["params"]["afec"] = "LzdN_7hSSYEK1_vayOupPXCls7zslsNIkqNA";
            jSessionPayload["params"]["is_wrap_app"] = false;
            jSessionPayload["params"]["language"] = "eng";
            jSessionPayload["params"]["site_id"] = "18749911";
            jSessionPayload["params"]["source"] = "42";
            jSessionPayload["rid"] = GenerateRid("request_session");
            _webSocket.Send(jSessionPayload.ToString());
        }
        private void doLogin()
        {
            JObject jLoginPayload = new JObject();
            jLoginPayload["command"] = "login";
            jLoginPayload["params"] = new JObject();
            jLoginPayload["params"]["confirmation_code"] = null;
            jLoginPayload["params"]["encrypted_token"] = true;
            jLoginPayload["params"]["username"] = Setting.Instance.username;
            jLoginPayload["params"]["password"] = Setting.Instance.password;
            jLoginPayload["rid"] = GenerateRid("command");
            _webSocket.Send(jLoginPayload.ToString());
        }
        private void sendRequestProfile()
        {
            JObject jProfilePayload = new JObject();
            jProfilePayload["command"] = "get";
            jProfilePayload["params"] = new JObject();
            jProfilePayload["params"]["source"] = "user";
            jProfilePayload["params"]["subscribe"] = true;
            jProfilePayload["params"]["what"] = new JObject();
            jProfilePayload["params"]["what"]["profile"] = new JArray();
            jProfilePayload["rid"] = GenerateRid("SubscribeCmd");
            _webSocket.Send(jProfilePayload.ToString());
        }
        private void sendPlaceBetMessage()
        {
            JObject jPlaceBetPayload = new JObject();
            jPlaceBetPayload["command"] = "do_bet";
            jPlaceBetPayload["params"] = new JObject();
            jPlaceBetPayload["params"]["amount"] = 1;
            JArray betsArray = new JArray();
            JObject bet = new JObject();
            bet["event_id"] = 5161092096;
            bet["price"] = 1.74;
            betsArray.Add(bet);
            jPlaceBetPayload["params"]["bets"] = betsArray;
            jPlaceBetPayload["params"]["is_bonus_money"] = false;
            jPlaceBetPayload["params"]["mode"] = 2;
            jPlaceBetPayload["params"]["odd_type"] = 0;
            jPlaceBetPayload["params"]["source"] = "42";
            jPlaceBetPayload["params"]["type"] = 1;
            jPlaceBetPayload["rid"] = GenerateRid("command");
            _webSocket.Send(jPlaceBetPayload.ToString());
            LogMng.Instance.onWriteStatus("Bet Success");

        }
        public string GenerateRid(string command)
        {
            // Function to generate a random 15-digit number
            string GenerateRandom15DigitNumber()
            {
                Random random = new Random();
                string result = string.Empty;
                for (int i = 0; i < 15; i++)
                {
                    result += random.Next(0, 10).ToString();
                }
                return result;
            }

            // Generate the rid
            string rand = GenerateRandom15DigitNumber();
            return $"{command}{rand}";
        }

        public bool login()
        {
            doLogin();
            Thread.Sleep(3000);
            if (m_userId == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public double getBalance()
        {
            sendRequestProfile();
            Thread.Sleep(2000);
            return m_balance; // Example return value
        }
        public PROCESS_RESULT PlaceBet(ref BetburgerInfo info)
        {
            LogMng.Instance.onWriteStatus("Place Bet Succeed");
            return new PROCESS_RESULT(); // Example return value
        }
        public string getProxyLocation()
        {
            // Implementation here
            return "Your proxy location implementation";
        }
        public HttpClient initHttpClient(bool bUseNewCookie)
        {
            // Implementation here
            return new HttpClient(); // Example return value
        }
        public bool logout()
        {
            // Implementation here
            return true; // Example return value
        }
        public bool Pulse()
        {
            // Implementation here
            return false; // Example return value
        }
        public void Close()
        {
            // Implementation here
        }
        public void Feature()
        {
            // Implementation here
        }
        public int GetPendingbets()
        {
            // Implementation here
            return 0; // Example return value
        }
    }
#endif
}
