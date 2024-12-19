using Newtonsoft.Json;
using Project.Helphers;
using Project.ViewModels;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project.ViewModels.MainWindowViewModel;

namespace Project.Server
{
    public delegate void ProcessNetPacket(Object param1, Object param2);
    public delegate void StopBot();
    public delegate void delConnectedServer();
    public class UserMng
    {
        // 메인폼에 알려주기 위한 핸들러

        public onBetburgerServerEvent onArbReceived;

        private UserInfo userInfo = new UserInfo();

        private static UserMng m_instance = null;

        public event StopBot stopEvent = null;

        public delConnectedServer connectedServer = null;
        public UserMng()
        {
            m_process[(byte)NETMSG_CODE.NETMSG_ARBINFO] = new ProcessNetPacket(ProcArbInfo);
            m_process[(byte)NETMSG_CODE.NETMSG_VALUEINFO] = new ProcessNetPacket(ProcValueBetsInfo);
            m_process[(byte)NETMSG_CODE.NETMSG_EVENTCHECK] = new ProcessNetPacket(ProcEventCheck);
            m_process[(byte)NETMSG_CODE.NETMSG_OPENBET] = new ProcessNetPacket(ProcOpenBets);            
            m_process[(byte)NETMSG_CODE.NETMSG_NSTOKEN] = new ProcessNetPacket(ProcReceiveNsToken);
            m_process[(byte)NETMSG_CODE.NETMSG_BETHEADERS] = new ProcessNetPacket(ProcBetHeaders); 
            m_process[(byte)NETMSG_CODE.NETMSG_LOGINRESULT] = new ProcessNetPacket(ProcLoginResult);
            //m_process[(byte)NETMSG_CODE.NETMSG_HEARTBEAT] = new ProcessNetPacket(ProcHeartbeat);
        }

        /// <summary>
        /// 서버로부터 수신된 자료를 처리하기 위한 함수배열
        /// </summary>
        public ProcessNetPacket[] Process
        {
            get
            {
                return m_process;
            }
        }
        private ProcessNetPacket[] m_process = new ProcessNetPacket[255];

        /// <summary>
        /// 유연수발생을 위한 변수
        /// </summary>
        private Random m_rand = new Random();



        /// <summary>
        /// 유일한 실체를 얻는다.
        /// </summary>
        /// <returns>실체값</returns>
        public static UserMng GetInstance()
        {
            if (m_instance == null)
                m_instance = new UserMng();

            return m_instance;
        }

        public void SetUserInfo(UserInfo _userInfo)
        {
            userInfo = _userInfo;
        }

        public void SetUserInfoState(USERSTATUS status)
        {
            userInfo.Status = status;
        }

        #region 요청메시지

        // 서버에 하트비트를 보낸다.
        public void SendToolHeartBeat()
        {
            NetPacket netdata = new NetPacket();
            netdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_HEARTBEAT;
            userInfo.SendData(netdata);
        }

        // 서버에 베팅가능성을 요청한다.

        public void SendToolEventCheck(BetburgerInfo info1, BetburgerInfo info2)
        {
            NetPacket netdata = new NetPacket();
            netdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_EVENTCHECK;
            netdata.Append(Converter.GetInstance().ObjectToByteArray(info1));
            netdata.Append(Converter.GetInstance().ObjectToByteArray(info2));
            userInfo.SendData(netdata);
        }

        // Post Betting data
        public void SendSuccessBetReport(PlacedBetInfo info)
        {
            NetPacket netdata = new NetPacket();
            netdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_SAVEBET;
            netdata.Append(Converter.GetInstance().ObjectToByteArray(info));
            userInfo.SendData(netdata);
        }


        // 클라이언트의 종료를 예고한다.
        public void SendToolClose()
        {
            NetPacket netdata = new NetPacket();
            netdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_CLOSE;
            userInfo.SendData(netdata);
        }

        public void SendLicense(UInt32 version, string license)
        {
            NetPacket netdata = new NetPacket();
            netdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_LICENSE;

            string declicense = "";
            try
            {
                declicense = license;
                //declicense = Helpher.Decrypt(license);
            }
            catch { }

            netdata.Append(version);            
            netdata.Append(declicense);
            netdata.Append(Setting.Instance.username_bet365);            

            userInfo.SendData(netdata);
        }

        #endregion

        #region 응답처리메시지

        // 서버와의 접속이 성공하였을때 처리.
        public void ProcArbInfo(Object param1, Object param2)
        {
            //if (Setting.Instance.betType == BBB.Betting.BetType.Valuebets)
            //    return;

            //if (BBPCookies.bRun)
            //    return;

            //LogMng.Instance.onWriteStatus("Updating server feeds...");
                        
            try
            {
                NetPacket netdata = param1 as NetPacket;
                byte[] infoListArray = netdata.Pop() as byte[];
                byte[] infoListArrayUnzip = Utils.Decompress(infoListArray);
                List<BetburgerInfo> infoList = Converter.GetInstance().ByteArrayToObject(infoListArrayUnzip) as List<BetburgerInfo>;

                onArbReceived(infoList);
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus("[Exception : ProcArbInfo] " + e.ToString());
            }
        }

        public void ProcValueBetsInfo(Object param1, Object param2)
        {
            //if (Setting.Instance.betType == BBB.Betting.BetType.Betsure)
            //    return;

            //if (BBPCookies.bRun)
            //    return;

            LogMng.Instance.onWriteStatus("Updating Values feeds...");

            try
            {
                NetPacket netdata = param1 as NetPacket;
                byte[] infoListArray = netdata.Pop() as byte[];
                byte[] infoListArrayUnzip = Utils.Decompress(infoListArray);
                List<BetburgerInfo> infoList = Converter.GetInstance().ByteArrayToObject(infoListArrayUnzip) as List<BetburgerInfo>;

                onArbReceived(infoList);
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus("[Exception : ProcArbInfo] " + e.ToString());
            }
        }

        public void ProcOpenBets(Object param1, Object param2)
        {
            LogMng.Instance.onWriteStatus("ProcOpenBets");
        }

        public void ProcLoginResult(Object param1, Object param2)
        {
            try
            {
                NetPacket netdata = param1 as NetPacket;
                int result = (int)netdata.Pop();
                switch (result)
                {
                    case 1:
                        LogMng.Instance.onWriteStatus("Your license is in use already.");
                        break;
                    case 2:
                        LogMng.Instance.onWriteStatus("Your license is incorrect( ErrorCode: 1 ).");
                        break;
                    case 3:
                        LogMng.Instance.onWriteStatus("Your license is incorrect( ErrorCode: 2 ).");
                        break;
                    case 4:
                        LogMng.Instance.onWriteStatus("Your license is out of date.");
                        break;
                    case 5:
                        LogMng.Instance.onWriteStatus("Your license is not registered for this package.");
                        break;
                    case 6:
                        LogMng.Instance.onWriteStatus("Client version is low, Please update bot.");
                        break;
                }

                if (result == 0)
                {
                    Global.PackageID = (UInt32)netdata.Pop();
                    Constants.PackageID = Global.PackageID;
                    if (0 < Global.PackageID && Global.PackageID < 6)
                    {
                        if (connectedServer != null)
                        {
                            connectedServer();
                            connectedServer = null;
                        }
                    }

                }
                else if (stopEvent != null)
                {
                    stopEvent();
                }
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus("[Exception : ProcArbInfo] " + e.ToString());
            }
        }

        
        public void ProcBetHeaders(Object param1, Object param2)
        {
            //LogMng.Instance.onWriteStatus("[ProcBetHeaders] start");
            try
            {
                NetPacket netdata = param1 as NetPacket;
                string infoListArray = netdata.Pop() as string;
                //LogMng.Instance.onWriteStatus($"[ProcBetHeaders] data {infoListArray}");
                Global.BetHeader.Pirxtheaders = JsonConvert.DeserializeObject<List<BetCryptHeader>>(infoListArray);
                //LogMng.Instance.onWriteStatus($"[ProcBetHeaders] count {Global.BetHeader.Pirxtheaders.Count}");                 
                if (connectedServer != null)
                {
                    connectedServer();
                    connectedServer = null;
                }

            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus("[Exception : ProcBetHeaders] " + e.ToString());
            }
        }

        public void ProcReceiveNsToken(Object param1, Object param2)
        {
            
            //LogMng.Instance.onWriteStatus("[ProcReceiveNsToken] start");
            try
            {
                NetPacket netdata = param1 as NetPacket;
                string nsToken = netdata.Pop() as string;
                Global.BetHeader.NSToken = nsToken;

                
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus("[Exception : ProcReceiveNsToken] " + e.ToString());
            }
        }
        
        public void ProcEventCheck(Object param1, Object param2)
        {
            LogMng.Instance.onWriteStatus("ProcEventCheck");
        }

        public void ProcHeartbeat(Object param1, Object param2)
        {
            LogMng.Instance.onWriteStatus(string.Format("[{0}] Got heart beat from server!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        }

#endregion
    }
}
