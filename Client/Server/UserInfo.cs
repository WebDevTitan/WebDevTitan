using System;
using System.Net;
using System.Timers;
using Protocol;

namespace Project.Server
{
    public class UserInfo
    {
        /// <summary>
        /// </summary>
        public string UserID
        {
            get
            {
                return m_strID;
            }
            set
            {
                m_strID = value;
            }
        }
        string m_strID;

        /// <summary>
        /// </summary>
        public string UserPwd
        {
            get
            {
                return m_strPwd;
            }
            set
            {
                m_strPwd = value;
            }
        }
        private string m_strPwd;

        /// <summary>
        /// </summary>
        public ServerConnect Connector
        {
            get
            {
                return m_connect;
            }
        }
        ServerConnect m_connect = null;


        private IPAddress m_serverip;
        /// <summary>
        /// </summary>
        public IPAddress ServerIP
        {
            get
            {
                return m_serverip;
            }
            set
            {
                m_serverip = value;
            }
        }

        private ushort m_port;
        /// <summary>
        /// </summary>
        public ushort Port
        {
            get
            {
                return m_port;
            }
            set
            {
                m_port = value;
            }
        }

        /// <summary>
        /// </summary>
        public DateTime LoginTime
        {
            get
            {
                return m_LoginTime;
            }
            set
            {
                m_LoginTime = value;
            }
        }
        private DateTime m_LoginTime = DateTime.Now;

        /// <summary>
        /// </summary>
        public int PlayingTime
        {
            get
            {
                return m_PlayingTime;
            }
            set
            {
                if (value < 0)
                    value = 0;
                m_PlayingTime = value;
            }
        }
        private int m_PlayingTime = 0;

        /// <summary>
        /// </summary>
        private const int INQUIRY_TIMER = 10000;

        /// <summary>
        /// </summary>
        private Timer m_timer = null;

        /// <summary>
        /// </summary>
        public int LastRecvPacketID
        {
            get
            {
                return m_nLastRecvPacketID;
            }
            set
            {
                m_nLastRecvPacketID = value;
            }
        }
        private int m_nLastRecvPacketID = 0;

        /// <summary>
        /// </summary>
        public int SendPacketID
        {
            set
            {
                if (value < 0)
                    value = 0;
                m_nSendPacketID = value;
            }
            get
            {
                return m_nSendPacketID;
            }
        }
        private int m_nSendPacketID = 0;


        protected USERSTATUS m_status = USERSTATUS.NOLOGIN_STATUS;
        public USERSTATUS Status
        {
            get
            {
                return m_status;
            }
            set
            {
                m_status = value;
            }
        }
        public UserInfo()
        {
            IPAddress ipAddress = null;
            if (!IPAddress.TryParse(Setting.Instance.ServerIP, out ipAddress))
            {
                try
                {
                    ipAddress = Dns.GetHostAddresses(Setting.Instance.ServerIP)[0];
                }
                catch { }
            }
            ServerIP = ipAddress;
            Port = Setting.Instance.ServerPort;
            m_timer = new Timer();
            m_timer.Enabled = false;
            m_timer.Interval = INQUIRY_TIMER;
            m_timer.Elapsed += new ElapsedEventHandler(OnTimerProcess);
            m_timer.Start();

            Status = USERSTATUS.NOLOGIN_STATUS;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnTimerProcess(object sender, ElapsedEventArgs e)
        {
            if (this.Status != USERSTATUS.NOLOGIN_STATUS)
            {
                NetPacket netdata = new NetPacket();

                netdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_HEARTBEAT;
                SendData(netdata);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>True:, False:</returns>
        public bool ConnectToServer()
        {
            m_connect = new ServerConnect(this);
            return m_connect.Connect();
        }

        public void DisconnectToServer()
        {
            if (m_connect != null) ;
            m_connect.Disconnect();
        }

        /// <summary>
        /// </summary>
        /// <param name="netdata"></param>
        public void SendData(NetPacket netdata)
        {
            try
            {
                netdata.SendPacketID = this.SendPacketID;
                netdata.LastRecvPacketID = this.LastRecvPacketID;
                this.SendPacketID++;
                byte[] data = netdata.GetPacketData();
                if (data == null)
                {
                    throw new Exception("error msg code");
                }
                if (m_connect == null)
                {
                    return;
                }
                if (m_connect.State == CONNECTSTATE.STATE_NONE)
                {
                    return;
                }
                m_connect.SendData(data);
            }
            catch (Exception)
            {
                //m_status = USERSTATUS.NOLOGIN_STATUS;
            }
        }
    }
}
