using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading;
using WebSocketSharp;

namespace Bet365LiveAgent.Logics
{
    class Bet365StreamClient
    {
        protected string _prefix = null;
        protected WebSocket _webSocket = null;
        protected string _webSocketHost = null;
        protected string _webSocketPort = null;
        protected string _webSocketOrigin = null;

        public Bet365ClientHandler OnConnected;
        public Bet365ClientHandler OnDisconnected;

        public Bet365StreamClient()
        {
            _prefix = string.Empty;
            _webSocketHost = string.Empty;
            _webSocketPort = string.Empty;
            _webSocketOrigin = string.Empty;
        }

        public void Connect(string Host = "", string Port = "", string Origin = "")
        {
            try
            {
                _webSocketHost = Host;
                _webSocketPort = Port;
                _webSocketOrigin = Origin;

                string _randomkey = Utils.GenerateRandomNumberString(16);
                string _randomkeybase64 = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(_randomkey));

                List<KeyValuePair<string, string>> webSockCustomHeaders = new List<KeyValuePair<string, string>>();
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Accept-Encoding", "gzip, deflate, br"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Accept-Language", "en-US,es-ES;q=0.8,es;q=0.5,en;q=0.3")); // "en -US,en;q=0.9"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Cache-Control", "no-cache"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Connection", "keep-alive, Upgrade")); // "Upgrade"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Host", _webSocketHost));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Origin", "https://" + _webSocketOrigin));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Pragma", "no-cache"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Sec-WebSocket-Version", "13"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Sec-WebSocket-key", _randomkeybase64));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Sec-WebSocket-Protocol", "zap-protocol-v1"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("Upgrade", "websocket"));
                webSockCustomHeaders.Add(new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36"));
                _webSocket = new WebSocket($"wss://{_webSocketHost}/zap/?uid={_randomkey}", "zap-protocol-v1");
                //if (!string.IsNullOrWhiteSpace(Config.Instance.ProxyUrl))
                //    _webSocket.SetProxy(Config.Instance.ProxyUrl, Config.Instance.ProxyUser, Config.Instance.ProxyPassword);
                _webSocket.EmitOnPing = true;
                _webSocket.CustomHeaders = webSockCustomHeaders;
                _webSocket.Origin = _webSocketOrigin;
                _webSocket.Compression = CompressionMethod.Deflate;
                _webSocket.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls | SslProtocols.Tls12 | SslProtocols.Tls12;
                _webSocket.Log.Level = WebSocketSharp.LogLevel.Error;
                _webSocket.Log.File = $"{Global.LogFilePath}{Global.LogFileName}";
                _webSocket.OnOpen += Socket_OnOpen;
                _webSocket.OnClose += Socket_OnClose;
                _webSocket.OnError += Socket_OnError;
                _webSocket.OnMessage += Socket_OnHandshake;
                _webSocket.Connect();
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, ex.ToString());
            }
        }

        public void Disconnect()
        {
            try
            {
                _webSocketHost = string.Empty;
                _webSocketPort = string.Empty;
                _webSocketOrigin = string.Empty;
                if (_webSocket != null)
                {
                    _webSocket.Close();
                    _webSocket = null;
                }
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, ex.ToString());
            }
        }

        protected virtual void Socket_OnOpen(object sender, EventArgs e)
        {
            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, $"{_prefix}Websocket connected");

            if (OnConnected != null)
                OnConnected();

            string cookieToken = Bet365ClientManager.Instance.CookieToken;
            string nstToken = Bet365ClientManager.Instance.NSTToken;
            string handshakePacket = $"{Global.HANDSHAKE_PROTOCOL}{Global.HANDSHAKE_VERSION}{Global.HANDSHAKE_CONNECTION_TYPE}{Global.HANDSHAKE_CAPABILITIES_FLAG}__time,S_{cookieToken},D_{nstToken}{(char)0}";
            _webSocket.Send(handshakePacket);

            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, $"{_prefix}Websocket sent handshake : {handshakePacket}");
        }

        protected virtual void Socket_OnClose(object sender, CloseEventArgs e)
        {
            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, $"{_prefix}Websocket closed : {e.Reason}");

            if (OnDisconnected != null)
                OnDisconnected();
        }

        protected virtual void Socket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, $"{_prefix}Websocket error : {e.Message}");
        }

        protected virtual void Socket_OnHandshake(object sender, MessageEventArgs e)
        {
            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, $"{_prefix}Websocket received handshake : {e.Data}");

            _webSocket.OnMessage -= Socket_OnHandshake;
            _webSocket.OnMessage += Socket_OnMessage;

            string configPacket = $"{Global.CLIENT_SUBSCRIBE}{Global.NONE_ENCODING}CONFIG_{Global.LANG_ID}_{Global.ZONE_ID},OVInPlay_{Global.LANG_ID}_{Global.ZONE_ID}{Global.DELIM_RECORD}";
            _webSocket.Send(configPacket);
        }

        protected virtual void Socket_OnMessage(object sender, MessageEventArgs e)
        {
#if DEBUG
            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, e.Data);
#else
            Global.WriteLog(LOGLEVEL.FILE, LOGTYPE.INDATA, e.Data);
#endif

            if (Bet365ClientManager.Instance.OnBet365DataReceived != null)
                Bet365ClientManager.Instance.OnBet365DataReceived(e.Data);
        }

        public void Send(string strData)
        {
            try
            {
                if (_webSocket.ReadyState == WebSocketState.Open)
                    _webSocket.Send(strData);
            }
            catch (Exception ex)
            {
                Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, ex.ToString());
            }
        }
    }

    class Bet365PrivateStreamClient : Bet365StreamClient
    {
        public Bet365PrivateStreamClient() : base()
        {            
            _prefix = "[Private] ";
        }

        protected override void Socket_OnHandshake(object sender, MessageEventArgs e)
        {
            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, $"{_prefix}Websocket received handshake : {e.Data}");

            _webSocket.OnMessage -= Socket_OnHandshake;
            _webSocket.OnMessage += Socket_OnMessage;
        }

        protected override void Socket_OnMessage(object sender, MessageEventArgs e)
        {
#if DEBUG
            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.INDATA, e.Data);
#else
            Global.WriteLog(LOGLEVEL.FILE, LOGTYPE.INDATA, e.Data);
#endif

            if (Bet365ClientManager.Instance.OnBet365PrivateDataReceived != null)
                Bet365ClientManager.Instance.OnBet365PrivateDataReceived(e.Data);
        }
    }
}
