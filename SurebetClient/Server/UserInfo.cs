using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Project.Server
{
    public class UserInfo
    {
        /// <summary>
        /// 가입아이디
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
        /// 가입암호 가상유저인 경우 0xFFFFFFFF
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
        /// 서버접속처리오브젝트변수
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
        /// 접속할 서버의 주소
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
        /// 접속할 서버의 포트번호
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
        /// 유저가 로그인한 시간
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
        /// 유저의 플레이시간
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
        /// 타이머주기
        /// </summary>
        private const int INQUIRY_TIMER = 10000;

        /// <summary>
        /// 타이머발생기
        /// </summary>
        private Timer m_timer = null;

        /// <summary>
        /// 마지막으로 수신한 파켓식별값
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
        /// 보내는 파켓의 순서번호
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
            IPAddress.TryParse(Setting.Instance.ServerIP, out ipAddress);
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
        /// 유저가 가입성공한 경우 주기적으로 호출
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnTimerProcess(object sender, ElapsedEventArgs e)
        {
            if (this.Status != USERSTATUS.NOLOGIN_STATUS)
            {
                NetPacket netdata = new NetPacket();

                netdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_HEARTBEAT;
                SendData(netdata);  // 서버에 하트비트신호보내기
            }
        }

        /// <summary>
        /// 서버에 접속시도한다.
        /// </summary>
        /// <returns>True:성공, False:실패</returns>
        public bool ConnectToServer()
        {
            m_connect = new ServerConnect(this);
            return m_connect.Connect();
        }

        public void DisconnectToServer()
        {
            if (m_connect != null);
                m_connect.Disconnect();            
        }

        /// <summary>
        /// 서버에 파켓자료를 보낸다.
        /// </summary>
        /// <param name="netdata">구조화된 자료오브젝트</param>
        public void SendData(NetPacket netdata)
        {
            try
            {
                netdata.SendPacketID = this.SendPacketID;
                netdata.LastRecvPacketID = this.LastRecvPacketID;
                this.SendPacketID++;       // 파켓순서번호를 증가한다.
                byte[] data = netdata.GetPacketData();
                if (data == null)
                {
                    throw new Exception("메세지코드가 설정되지 않았습니다.");
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
