using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

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
        /// 소켓의 접속상태를 나타낸다.
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
        /// 소속된 유저
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
        /// UserMng클래스의 함수배열묶음이 대입된다. 수신된 자료를 해당 함수에로 분기시킨다.
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
        /// 서버에 접속요청을 보낸다.
        /// </summary>
        /// <returns>true:성공, false:실패</returns>
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
        /// 서버에 소켓접속이 성공하는 경우 호출된다.
        /// </summary>
        /// <param name="ar">접속한 소켓</param>
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
                UserMng.GetInstance().SendLicense(Global.Version, Setting.Instance.license);

            }
            catch (Exception ex)
            {
                base.Disconnect();
            }
            finally
            {
                // 접속결과를 메인폼에 알려준다.

            }
        }

        /// <summary>
        /// 서버로부터 파켓수신되였을 때 호출된다.
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
        /// 파켓자료를 분석해서 해당 처리함수로 보낸다.
        /// </summary>
        /// <param name="buff">수신된 파켓자료</param>
        /// <param name="param">추가파라메터</param>
        override protected void AnalyzePacket(NetPacket netdata, object param)
        {
            try
            {
                //메세지의 종류에 따라 분류
                if ((int)netdata.MsgCode < m_process.Length && m_process[(int)netdata.MsgCode] != null)
                {
                    this.User.LastRecvPacketID = netdata.SendPacketID;     // 클라이언트에서 보내는 파켓의 식별값을 보관한다.
                    m_process[(int)netdata.MsgCode](netdata, this.User);
                }
                else if ((byte)netdata.MsgCode == 0xFF)
                {   // 하트비트메세지는 되돌려 보낸다.
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
