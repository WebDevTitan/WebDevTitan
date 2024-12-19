using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections;
using Protocol;
using BetburgerServer.Controller;

namespace SeastoryServer
{

    public enum USERTYPE : byte
    {
        ONLINETYPE=0,           // 온라인유저
        AUTOTYPE                // 가상유저
    }

    public enum USERSTATUS : byte
    {
        NOLOGIN_STATUS = 0,     // 미가입상태
        LOGIN_STATUS,           // 가입한 상태
        GAMESTART_STATUS,       // 알방선택한 후 상태
    }

    public enum ROOM : byte
    {
        ROOMFIRST = 0,
        ROOM1,      // 1채널100원방
        ROOM2,      // 2채널100원방
        ROOM3,      // 3채널200원방
        //ROOM4,      // 4채널500원방
        ROOMEND,
    }

    public enum ALERTTYPE : byte
    {
        ALERT_NORMAL = 0,   // 일반통보문
        ALERT_ERROR,        // 오유통보문
    }

    class UserInfo
    {
        public ClientSocket Sock
        {
            get
            {
                return m_sock;
            }
            
            set
            {
                m_sock = value;
            }
            
        }
        protected ClientSocket m_sock;

        private string m_uuID = "";        // uuid of license
        public string UUID
        {
            get
            {
                return m_uuID;
            }
            set
            {
                m_uuID = value;
            }
        }


        protected string m_strMasterName = "";  // 회원 아이디
        public string MasterName
        {
            get
            {
                return m_strMasterName;
            }
            set
            {
                m_strMasterName = value;
            }
        }

        protected string m_strGameID = ""; // 회원 암호
        public string GameID
        {
            get
            {
                return m_strGameID;
            }
            set
            {
                m_strGameID = value;
            }
        }

        protected string m_strLicense = "";
        public string License
        {
            get
            {
                return m_strLicense;
            }
            set
            {
                m_strLicense = value;
            }
        }


        protected DateTime m_expireTime = DateTime.MinValue; // 회원 닉네임
        public DateTime ExpireTime
        {
            get
            {
                return m_expireTime;
            }
            set
            {
                m_expireTime = value;
            }
        }

        /// <summary>
        /// 디비에서 마지막으로 읽은 쪽지수신ID
        /// </summary>
        public long LastAlertID
        {
            get
            {
                return m_nLastAlertID;
            }
            set
            {
                m_nLastAlertID = value;
            }
        }
        private long m_nLastAlertID = 0;

        
        public string Privillage
        {
            get
            {
                return m_strPrivillage;
            }
            set
            {
                m_strPrivillage = value;
            }
        }
        private string m_strPrivillage = "";

        /// <summary>
        /// 관리자가 설정한 VIP회원인가를 나타내는 값(0-VIP회원설정안됨, 그외-관리자가 설정한 VIP회원임)
        /// </summary>
        public string Bookmaker
        {
            get
            {
                return m_strBookmaker;
            }
            set
            {
                m_strBookmaker = value;
            }
        }
        private string m_strBookmaker = "";

        /// <summary>
        /// 유저의 머니
        /// </summary>
        public double Balance
        {
            get
            {
                return m_nBalance;
            }
            set
            {
                lock (this)
                {
                    if (value < 0)
                    {
                        value = 0;
                    }
                    m_nBalance = value;
                }
            }
        }
        protected double m_nBalance = 0;

     
        public bool QR_Scannable
        {
            get
            {
                return m_bQRScannable;
            }
            set
            {
                m_bQRScannable = value;
            }
        }

        protected bool m_bQRScannable = false;
               
        public int QR_ScanCount
        {
            get
            {
                return m_nQRScanCount;
            }
            set
            {
                lock (this)
                {
                    if (value < 0)
                    {
                        value = 0;
                    }
                    m_nQRScanCount = value;
                }
            }
        }
        protected int m_nQRScanCount = 0;

        /// <summary>
        /// 서버로그에 남기는 유저의 닉네임
        /// </summary>
        public string LogName
        {
            get
            {
                return m_strLogName;
            }
            set
            {
                m_strLogName = value;
            }
        }
        protected string m_strLogName = "";

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

        /// <summary>
        /// 유저검색키(해시코드)
        /// </summary>
        public string HashKey
        {
            get
            {
                //if (this.ID == 0)
                //{
                    
                //}
                //else
                //{
                //    return this.Nickname;
                //}

                return this.GetHashCode().ToString();
            }
        }

        public int NetDeadTime
        {
            get
            {
                if (this.Sock == null)
                {
                    return int.MaxValue;
                }
                return this.Sock.ConnectionDeadSeconds;
            }
        }

        protected UInt32 m_nPackageID;
        public UInt32 PackageID
        {
            get
            {
                return m_nPackageID;
            }
            set
            {
                m_nPackageID = value;
            }
        }

        protected USERSTATUS m_nStatus;
        public USERSTATUS Status
        {
            get
            {
                return m_nStatus;
            }
            set
            {
                m_nStatus = value;
            }
        }

        private const int MAXMACHINE = 3;                       // 유저가 점유할수 있는 최대 기계대수
        protected Hashtable m_machines;                         // 점유키: 채널값_기계번호
        public Hashtable Machines
        {
            get
            {
                return m_machines;
            }
        }

        /// <summary>
        /// 로그인한 아이피
        /// </summary>
        public string ClientIP
        {
            get
            {
                return m_strClientIP;
            }
            set
            {
                m_strClientIP = value;
            }
        }
        private string m_strClientIP = "";

        /// <summary>
        /// 클라이언트의 맥주소
        /// </summary>
        public string MacAddress
        {
            set
            {
                m_strMacAddress = value;
            }
            get
            {
                return m_strMacAddress;
            }
        }
        private string m_strMacAddress = "";

        private long m_nLoginhistID = 0;                        // 로그인내역디비레코드아이디

      

        /// <summary>
        /// 유저가 입금할수 있는 은행정보표의 레코드아이디
        /// </summary>
        public int BankInfoID
        {
            get
            {
                return m_nBankInfoID;
            }
            set
            {
                if (value < 0)
                    value = 0;
                m_nBankInfoID = value;
            }
        }
        private int m_nBankInfoID = 0;
        
        /// <summary>
        /// 이전에 보냈던 파켓자료
        /// </summary>
        public ArrayList ArrPrevNetData
        {
            get
            {
                return m_arrPrevNetData;
            }
        }
        public ArrayList m_arrPrevNetData = new ArrayList();

        public UserInfo()
        {
            m_strGameID = "";
            m_strMasterName = "";
            m_strLicense = "";
            m_machines = Hashtable.Synchronized(new Hashtable());
            m_sock = null;
            PackageID = 0;
            m_nStatus = USERSTATUS.NOLOGIN_STATUS;
        }

        public UserInfo(Socket sock)
            : this()
        {
            this.m_sock = new ClientSocket(sock, GameServer.MAXPACKETLENGTH);
            this.Sock.AsyncParam = this;
            this.Sock.FuncAnalyzeData = new ProcessPacket(GameServer.GetInstance().ProcPacketData);
            this.Sock.FuncAsyncReceive = new AsyncCallback(this.OnReceive);
            this.ClientIP = this.m_sock.RemoteEndPoint.ToString().Split(':')[0];
        }

        // Setup the callback for recieved data and loss of conneciton
        public void BeginReceive()
        {
            try
            {
                if (this.Sock != null)
                {
                    this.Sock.BeginReceive();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("cGameClient::BeginReceive Error ", ex.Message);
            }
        }

        /// <summary>
        /// 서버로부터 파켓수신되였을 때 호출된다.
        /// </summary>
        /// <param name="ar"></param>
        public void OnReceive(IAsyncResult ar)
        {
            try
            {
                if (this.Sock != null)
                {
                    this.Sock.OnReceive(ar);
                }
            }
            catch (System.Exception ex)
            {
                string strMsg;
                strMsg = string.Format("{0}", ex.Message);
                GameServer.GetInstance().SendLog(strMsg);
                
            }
        }

        public void Close()
        {
            this.Status = USERSTATUS.NOLOGIN_STATUS;
            ClientSocket tmpSock = this.Sock;
            if (tmpSock != null)
            {
                this.m_sock = null;
                tmpSock.Close();
            }
        }


        /// <summary>
        /// 파라메터로 넘어온 망자료오브젝트를 유저에게 전송한다.
        /// </summary>
        /// <param name="netdata">자료오브젝트</param>
        public void SendData(NetPacket netdata)
        {
            try
            {
                //Trace.WriteLine("SendData 1");
                netdata.SendPacketID = this.SendPacketID;
                netdata.LastRecvPacketID = this.LastRecvPacketID;
                this.SendPacketID++;       // 파켓순서번호를 증가한다.
                byte[] data = netdata.GetPacketData();
                if (data == null)
                {
                    //Trace.WriteLine("SendData 2");
                    throw new Exception("메세지코드가 설정되지 않았습니다.");
                }
                if (m_sock == null)
                {
                    //Trace.WriteLine("SendData 3");
                    return;
                }
                if (data.Length > GameServer.MAXPACKETLENGTH)
                {
                    //Trace.WriteLine("SendData 4");
                    return;
                }
                //Trace.WriteLine("SendData 5");
                this.Sock.SendData(data);
                //Trace.WriteLine("SendData 6");
            }
            catch (SocketException e)
            {
                this.Sock = null;
                string strMsg;
                strMsg = string.Format("파켓자료전송이 실패하였습니다. {0}", e.Message);
                throw new Exception(strMsg);
                //Trace.WriteLine("SendData 7");
            }
            //Trace.WriteLine("SendData 8");
        }
    }
}
