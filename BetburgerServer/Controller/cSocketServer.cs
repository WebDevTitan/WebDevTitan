using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace kr.com.choya.net.socket
{

    public abstract class cSocketServer
    {

        protected Socket m_sockListen = null;
        protected int m_portListen;
        public const int MIN_PORT = 4000;
        public const int MAX_PORT = 65500;
        protected bool m_bIsStart = false;

        public int ListenPort
        {
            get
            {
                return m_portListen;
            }
            set
            {
                m_portListen = value;
            }
        }

        // 생성자
        public cSocketServer(int port)
        {
            m_portListen = port;
        }

        // 게임 서버 시작
        public void StartServer()
        {
            // 현재 컴퓨터의 IPAddress를 얻는다.
            IPAddress[] localAddr = null;
            string hostName = "";
            try
            {
                // NOTE: DNS lookups are nice and all but quite time consuming.
                hostName = Dns.GetHostName();
                IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
                localAddr = ipEntry.AddressList;
            }
            catch (Exception ex)
            {
                throw (new Exception("$Error--Failure for getting local addr info."));
            }

            // Verify we got an IP address. Tell the user if we did
            if (localAddr == null || localAddr.Length < 1)
            {
                throw (new Exception("$Error--Failure for getting local addr info."));
            }

            // Create the listener socket in this machines IP address
            m_sockListen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                if(m_portListen<MIN_PORT || m_portListen>MAX_PORT)
                {
                    string strMsg;
                    strMsg = string.Format("Out of port number. Min:{0}, Max:{1}, Input:{2}",
                        MIN_PORT, MAX_PORT, m_portListen);
                    throw (new Exception(strMsg));
                }
                m_sockListen.Bind(new IPEndPoint(IPAddress.Any, m_portListen));
                // m_sockListen.Bind(new IPEndPoint(IPAddress.Loopback, m_portListen));	// 127.0.0.1을 이용하기 위한 코드
                m_sockListen.Listen(400);

                OnStart();
                // Setup a callback to be notified of connection requests
                m_sockListen.BeginAccept(new AsyncCallback(OnConnectRequest), m_sockListen);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // 클라이언트로부터 accept가 있을 경우
        public void OnConnectRequest(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;

            if (m_bIsStart == false)
                return;
            try
            {
                //EventLIstener(SERVER_EVENT.ONCONNECT, listener.EndAccept(ar));
                OnSocketConnect(listener.EndAccept(ar));
            }
            catch (Exception ex)
            {
                SendLog("Occur Error while processing socket connect.", ex);
            }
            try
            {
                listener.BeginAccept(new AsyncCallback(OnConnectRequest), listener);
            }
            catch (Exception ex)
            {
                SendLog("Occur Error while processing socket wait.", ex);
            }
        }

        /// <summary>
        /// 서버로그를 남긴다.
        /// </summary>
        /// <param name="strMsg">로그문자열</param>
        public virtual void SendLog(string strMsg)
        {
        }

        /// <summary>
        /// 서버로그를 남긴다.
        /// </summary>
        /// <param name="strMsg">로그문자열</param>
        public virtual void SendLog(string strMsg, Exception ex)
        {
        }

        public void CloseServer()
        {
            if (m_sockListen != null)
            {            
                m_sockListen.Close();
            }
        }

        // 서버가 시작되였을때의 사건처리부
        public abstract void OnStart();

        // 클라이언트가 소켓접속되였을때의 사건처리부
        public abstract void OnSocketConnect(Socket sock);

        // 클라이언트접속에 실패되였을 경우
        public abstract void OnError(string msg, Exception e);        
    }
}
