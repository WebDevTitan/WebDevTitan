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
        ONLINETYPE=0,           // �¶�������
        AUTOTYPE                // ��������
    }

    public enum USERSTATUS : byte
    {
        NOLOGIN_STATUS = 0,     // �̰��Ի���
        LOGIN_STATUS,           // ������ ����
        GAMESTART_STATUS,       // �˹漱���� �� ����
    }

    public enum ROOM : byte
    {
        ROOMFIRST = 0,
        ROOM1,      // 1ä��100����
        ROOM2,      // 2ä��100����
        ROOM3,      // 3ä��200����
        //ROOM4,      // 4ä��500����
        ROOMEND,
    }

    public enum ALERTTYPE : byte
    {
        ALERT_NORMAL = 0,   // �Ϲ��뺸��
        ALERT_ERROR,        // �����뺸��
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


        protected string m_strMasterName = "";  // ȸ�� ���̵�
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

        protected string m_strGameID = ""; // ȸ�� ��ȣ
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


        protected DateTime m_expireTime = DateTime.MinValue; // ȸ�� �г���
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
        /// ��񿡼� ���������� ���� ��������ID
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
        /// �����ڰ� ������ VIPȸ���ΰ��� ��Ÿ���� ��(0-VIPȸ�������ȵ�, �׿�-�����ڰ� ������ VIPȸ����)
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
        /// ������ �Ӵ�
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
        /// �����α׿� ����� ������ �г���
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
        /// ���������� ������ ���Ͻĺ���
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
        /// ������ ������ ������ȣ
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
        /// �����˻�Ű(�ؽ��ڵ�)
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

        private const int MAXMACHINE = 3;                       // ������ �����Ҽ� �ִ� �ִ� �����
        protected Hashtable m_machines;                         // ����Ű: ä�ΰ�_����ȣ
        public Hashtable Machines
        {
            get
            {
                return m_machines;
            }
        }

        /// <summary>
        /// �α����� ������
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
        /// Ŭ���̾�Ʈ�� ���ּ�
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

        private long m_nLoginhistID = 0;                        // �α��γ�������ڵ���̵�

      

        /// <summary>
        /// ������ �Ա��Ҽ� �ִ� ��������ǥ�� ���ڵ���̵�
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
        /// ������ ���´� �����ڷ�
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
        /// �����κ��� ���ϼ��ŵǿ��� �� ȣ��ȴ�.
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
        /// �Ķ���ͷ� �Ѿ�� ���ڷ������Ʈ�� �������� �����Ѵ�.
        /// </summary>
        /// <param name="netdata">�ڷ������Ʈ</param>
        public void SendData(NetPacket netdata)
        {
            try
            {
                //Trace.WriteLine("SendData 1");
                netdata.SendPacketID = this.SendPacketID;
                netdata.LastRecvPacketID = this.LastRecvPacketID;
                this.SendPacketID++;       // ���ϼ�����ȣ�� �����Ѵ�.
                byte[] data = netdata.GetPacketData();
                if (data == null)
                {
                    //Trace.WriteLine("SendData 2");
                    throw new Exception("�޼����ڵ尡 �������� �ʾҽ��ϴ�.");
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
                strMsg = string.Format("�����ڷ������� �����Ͽ����ϴ�. {0}", e.Message);
                throw new Exception(strMsg);
                //Trace.WriteLine("SendData 7");
            }
            //Trace.WriteLine("SendData 8");
        }
    }
}