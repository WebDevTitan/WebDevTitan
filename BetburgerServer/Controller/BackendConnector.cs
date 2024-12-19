using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeastoryServer;


namespace BetburgerServer
{
    public class OnlineObject
    {
        public string license_uuid;
        public string license;
        public string game_id;
        public string balance;
        public string bookmaker;
        public string ip;
        public string expire_at;
    }
    public class BackendConnector
    {
        
        private SocketIOClient.SocketIO _socket = null;
        

        
        private Thread m_pingThread = null;
        public BackendConnector()
        {
                    
        }
        public void CloseSocket()
        {
            _socket.Dispose();
        }
        private void pingThreadFunc()
        {
            while (true)
            {
                try
                {
                    _socket.EmitAsync("Authorization", "ss");
                    Thread.Sleep(2000);
                }
                catch (Exception)
                {

                }

            }
        }

        public void Stop()
        {
            if (_socket != null)
            {   
                _socket.Dispose();
                _socket = null;
            }
        }
        public async void Start()
        {
            Stop();
#if (BET365)
            _socket = new SocketIOClient.SocketIO("http://127.0.0.1:5000" + "/dashboard", new SocketIOClient.SocketIOOptions
            {
                EIO = SocketIO.Core.EngineIO.V3,
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                ExtraHeaders = new Dictionary<string, string>
                {
                    { "Authorization", "GameServer" },
                    { "ServerName", cServerSettings.GetInstance().Label}
                }

            });

            _socket.OnConnected += Socket_OnConnected;
            _socket.OnError += Socket_OnError;
            _socket.OnDisconnected += Socket_OnDisconnected;
            await _socket.ConnectAsync();
            _socket.OnAny((name, response) =>
            {
                Console.WriteLine(name);
                Console.WriteLine(response);
            });
            _socket.On("hi", response =>
            {
                // Console.WriteLine(response.ToString());
                Console.WriteLine(response.GetValue<string>());
            });

            _socket.On("disconnect", response =>
            {
                // Console.WriteLine(response.ToString());
                Console.WriteLine(response.GetValue<string>());
            });
#endif
        }

        public void SendOnlineUserList(ArrayList users)
        {
            List<OnlineObject> onlineArray = new List<OnlineObject>();
            foreach (UserInfo user in users)
            {
                onlineArray.Add(new OnlineObject() { license_uuid = user.UUID, license = user.License, bookmaker = user.Bookmaker, game_id = user.GameID, expire_at = user.ExpireTime.ToString(), ip = user.Sock.RemoteEndPoint.ToString(), balance = user.Balance>0?user.Balance.ToString():""});                
            };

            string onlineString = JsonConvert.SerializeObject(onlineArray);
            if (_socket != null)
            {
                _socket.EmitAsync("online", onlineString);                
            }
        }

        private void Socket_OnDisconnected(object sender, string e)
        {
            
        }
        private void Socket_OnError(object sender, string e)
        {
            
        }
        private void Socket_OnConnected(object sender, EventArgs e)
        {
            
            var socket = sender as SocketIOClient.SocketIO;
            // m_handlerWriteStatus("Socket.Id:" + socket.Id);
        }
        

    }
}