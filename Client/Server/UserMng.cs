using System;
using System.Collections.Generic;
using System.Threading;
using Project.Helphers;
using Project.ViewModels;
using Protocol;

namespace Project.Server
{
    public delegate void ProcessNetPacket(Object param1, Object param2);
    public delegate void StopBot();
    public delegate void delConnectedServer();
    public class UserMng
    {

        public onBetburgerServerEvent onArbReceived;

        private UserInfo userInfo = new UserInfo();

        private static UserMng m_instance = null;

        public StopBot stopEvent = null;

        public delConnectedServer connectedServer = null;
        public UserMng()
        {
            m_process[(byte)NETMSG_CODE.NETMSG_ARBINFO] = new ProcessNetPacket(ProcArbInfo);
            m_process[(byte)NETMSG_CODE.NETMSG_VALUEINFO] = new ProcessNetPacket(ProcValueBetsInfo);

            m_process[(byte)NETMSG_CODE.NETMSG_NSTOKEN] = new ProcessNetPacket(ProcReceiveNsToken);
            m_process[(byte)NETMSG_CODE.NETMSG_BETHEADERS] = new ProcessNetPacket(ProcBetHeaders);
            //m_process[(byte)NETMSG_CODE.NETMSG_LOGINRESULT] = new ProcessNetPacket(ProcLoginResult);
            //m_process[(byte)NETMSG_CODE.NETMSG_HEARTBEAT] = new ProcessNetPacket(ProcHeartbeat);
        }

        /// <summary>
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
        /// </summary>
        private Random m_rand = new Random();



        /// <summary>
        /// </summary>
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

        #region

        public void SendToolHeartBeat()
        {
            NetPacket netdata = new NetPacket();
            netdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_HEARTBEAT;
            userInfo.SendData(netdata);
        }

        public void SendClientMessage(string serverLog)
        {
            NetPacket netdata = new NetPacket();
            netdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_CLIENTMESSAGE;
            netdata.Append(serverLog);
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

        public void SendClientBalance(double balance, string info = "")
        {
            NetPacket netdata = new NetPacket();
            netdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_UPLOADBALANCE;
            netdata.Append(balance);
            netdata.Append(info);
            userInfo.SendData(netdata);
        }
        public void SendToolClose()
        {
            NetPacket netdata = new NetPacket();
            netdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_CLOSE;
            userInfo.SendData(netdata);
        }

        public void SendQRRequest(int nMode, string param)
        {
            //nMode 0:Get
            //nMode 1:RefreshToken

            LogMng.Instance.onWriteStatus($"QR Request mode: {nMode} param: {param}");

            NetPacket netdata = new NetPacket();
            netdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_NSTOKEN;
            netdata.Append(nMode);
            netdata.Append(param);
            userInfo.SendData(netdata);
        }
        public void SendLicense(UInt32 version, string license, string bookmaker)
        {
            NetPacket netdata = new NetPacket();
            netdata.MsgCode = (ushort)NETMSG_CODE.NETMSG_LICENSE;            
            string declicense = "";
            string username = "";
            string password = "";
            try
            {
                declicense = license;
                username = Setting.Instance.username;
                password = Setting.Instance.password;
                //declicense = Helpher.Decrypt(license);
            }
            catch { }
             
            netdata.Append(version);
            netdata.Append(declicense);
            netdata.Append(bookmaker);
            netdata.Append(username);
            netdata.Append(password);
            userInfo.SendData(netdata);            
        }

        #endregion

        #region 

        // 
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
                LogMng.Instance.onWriteStatus("[Exception : ProcValueBetsInfo] " + e.ToString());
            }
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
                        LogMng.Instance.onWriteStatus("Your license is incorrect( ErrorCode: 3 ).");
                        break;
                    case 5:
                        LogMng.Instance.onWriteStatus("Your license is deactivated, please contact bot master.");
                        break;
                    case 6:
                        {
                            string bookmaker = (string)netdata.Pop();
                            LogMng.Instance.onWriteStatus($"This license is created for other bookmaker({bookmaker}).");
                        }
                        break;
                    case 7:
                        {
                            string assigned_userid = (string)netdata.Pop();
                            LogMng.Instance.onWriteStatus($"Your license is already assigned for other account({assigned_userid}).");
                        }
                        break;
                    case 8:
                        LogMng.Instance.onWriteStatus("Your license is already expired.");
                        break;
                    case 9:
                        LogMng.Instance.onWriteStatus("Client version is low, Please update bot.");
                        break;
                }

                if (result == 0)
                {
                    if (connectedServer != null)
                    {
                        connectedServer();
                        connectedServer = null;
                    }

                }
                else if (stopEvent != null)
                {
                    stopEvent();
                }
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus("[Exception : ProcLoginResult] " + e.ToString());
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
                //Global.BetHeader.Pirxtheaders = JsonConvert.DeserializeObject<List<BetCryptHeader>>(infoListArray);
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

        Random rnd = new Random();
        public void ProcReceiveNsToken(Object param1, Object param2)
        {
#if (BET365_ADDON)
            //LogMng.Instance.onWriteStatus("[ProcReceiveNsToken] start");
            try
            {
                NetPacket netdata = param1 as NetPacket;
                int nMode = (int)netdata.Pop();
                int nStatusCode = (int)netdata.Pop();
                string strContent = netdata.Pop() as string;

                if (nMode == 0)
                {
                    LogMng.Instance.onWriteStatus($"QR Post Response StatusCode: {(HttpStatusCode)nStatusCode}");
                    LogMng.Instance.onWriteStatus($"QR Post Response: {strContent}");
                                     
                    var qrPostResponse = JsonConvert.DeserializeObject<dynamic>(strContent);
                    if (!string.IsNullOrEmpty(qrPostResponse.refreshToken.ToString()))
                    {
                        Bet365_ADDONCtrl.QRGetRefreshToken = qrPostResponse.refreshToken.ToString();
                        Bet365_ADDONCtrl.timeToSendQRGetRequest = DateTime.Now.AddSeconds(40);                        
                    }
                }
                else if (nMode == 1)
                {
                    LogMng.Instance.onWriteStatus($"QR Get Response StatusCode: {(HttpStatusCode)nStatusCode}");                    
                    LogMng.Instance.onWriteStatus($"QR Get Response : {strContent}");

                    var responseJson = JsonConvert.DeserializeObject<dynamic>(strContent);
                    if (responseJson.status.ToString() == "-1")
                    {
                        Bet365_ADDONCtrl.timeToSendQRGetRequest = DateTime.MaxValue;
                    }
                    else
                    {
                        Bet365_ADDONCtrl.timeToSendQRGetRequest = DateTime.Now.AddSeconds(rnd.Next(30, 60));
                    }
                }               
                
            }
            catch (Exception e)
            {
                LogMng.Instance.onWriteStatus("[Exception : ProcReceiveNsToken] " + e.ToString());
            }
#endif
        }

        public void ProcHeartbeat(Object param1, Object param2)
        {
            LogMng.Instance.onWriteStatus(string.Format("[{0}] Got heart beat from server!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        #endregion
    }
}
