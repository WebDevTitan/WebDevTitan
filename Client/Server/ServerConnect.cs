using System;
using System.Threading;

namespace Project.Server
{
    public enum CONNECTSTATE : byte
    {
        STATE_NONE = 0,
        STATE_CONNECT,
        STATE_RECONNECT,
    }
    public class ServerConnect : ServerSock
    {
        private CONNECTSTATE m_state = CONNECTSTATE.STATE_NONE;
        /// <summary>
        /// </summary>
        public CONNECTSTATE State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value;
            }
        }

        private UserInfo m_user = null;
        /// <summary>
        /// </summary>
        public UserInfo User
        {
            get
            {
                return m_user;
            }
            set
            {
                m_user = value;
            }
        }

        /// <summary>
        /// UserMng
        /// </summary>
        private ProcessNetPacket[] m_process = null;

        public ServerConnect(UserInfo user)
            : base(user.ServerIP, user.Port)
        {
            try
            {
                this.User = user;
                this.m_process = UserMng.GetInstance().Process;

            }
            catch (Exception)
            { }
        }


        /// <summary>
        /// </summary>
        /// <returns>true, false</returns>
        public bool Connect()
        {
            try
            {
                LogMng.Instance.onWriteStatus("Server connecting...");

                UserMng.GetInstance().SetUserInfo(User);
                return this.ConnectToServer();
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// called when socket connection is successful to server
        /// </summary>
        /// <param name="ar">connected socket</param>
        override public void OnConnect(IAsyncResult ar)
        {
            try
            {
                base.OnConnect(ar);
                m_state = CONNECTSTATE.STATE_CONNECT;
                User.Status = Protocol.USERSTATUS.LOGIN_STATUS;

                LogMng.Instance.onWriteStatus("Server connected!");
                Global.bServerConnect = true;               

                // send license key
                UserMng.GetInstance().SendLicense(Global.Version, Setting.Instance.license, Global.Bookmaker);              

            }
            catch (Exception ex)
            {
                base.Disconnect();
            }
            finally
            {

            }
        }

        /// <summary>
        /// </summary>
        /// <param name="ar"></param>
        override public void OnReceive(IAsyncResult ar)
        {
            try
            {
                base.OnReceive(ar);
            }
            catch (Exception ex)
            {
                this.Disconnect();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="buff">received packet data</param>
        /// <param name="param">extra param</param>
        override protected void AnalyzePacket(NetPacket netdata, object param)
        {
            try
            {
                if ((int)netdata.MsgCode < m_process.Length && m_process[(int)netdata.MsgCode] != null)
                {
                    this.User.LastRecvPacketID = netdata.SendPacketID;
                    m_process[(int)netdata.MsgCode](netdata, this.User);
                }
                else if ((byte)netdata.MsgCode == 0xFF)
                {
                    //m_user.SendData(netdata);
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus("Server connection lost, will try to reconnect...");
                LogMng.Instance.onWriteStatus("[Exception : AnalyzePacket] " + ex.ToString());
                Thread.Sleep(70000);    //waiting means server remove current connection in server side.
                                        //if it reconnect imediately, server misjudge it's duplicated license connecting.

                base.Disconnect();
            }
        }

        public override void OnDisconnect(IAsyncResult ar)
        {
            LogMng.Instance.onWriteStatus("OnDisconnect");
            Global.bServerConnect = false;
        }
    }
}
