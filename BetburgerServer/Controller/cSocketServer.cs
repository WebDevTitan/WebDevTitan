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

        // ������
        public cSocketServer(int port)
        {
            m_portListen = port;
        }

        // ���� ���� ����
        public void StartServer()
        {
            // ���� ��ǻ���� IPAddress�� ��´�.
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
                // m_sockListen.Bind(new IPEndPoint(IPAddress.Loopback, m_portListen));	// 127.0.0.1�� �̿��ϱ� ���� �ڵ�
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

        // Ŭ���̾�Ʈ�κ��� accept�� ���� ���
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
        /// �����α׸� �����.
        /// </summary>
        /// <param name="strMsg">�α׹��ڿ�</param>
        public virtual void SendLog(string strMsg)
        {
        }

        /// <summary>
        /// �����α׸� �����.
        /// </summary>
        /// <param name="strMsg">�α׹��ڿ�</param>
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

        // ������ ���۵ǿ������� ���ó����
        public abstract void OnStart();

        // Ŭ���̾�Ʈ�� �������ӵǿ������� ���ó����
        public abstract void OnSocketConnect(Socket sock);

        // Ŭ���̾�Ʈ���ӿ� ���еǿ��� ���
        public abstract void OnError(string msg, Exception e);        
    }
}