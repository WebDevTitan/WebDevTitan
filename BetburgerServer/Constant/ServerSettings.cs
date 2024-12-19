using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Net;
using System.Net.NetworkInformation;
using System.Xml;

namespace SeastoryServer
{
    public class cServerSettings
    {
        private const string LISTENPORTTAG = "GAME_PORT";
        private const string WEBAPIPORTTAG = "WEBAPI_PORT";
        private const string NSTSERVERTAG = "NSTSERVER";
        private const string DBIPTAG = "DB_IP";
        private const string DBNAMETAG = "DB_NAME";
        private const string DBIDTAG = "DB_LOGINID";
        private const string DBPWDTAG = "DB_LOGINPWD";
        private const string DBPORTTAG = "DB_PORT";
        private const string BBUSERTAG = "BB_USER";
        private const string BBISAPITOKEN = "BB_ISAPITOKEN";
        private const string SUREBETPRFILTERTAG = "SUREBETPRFILTER";
        private const string SUREBETLVFILTERTAG = "SUREBETLVFILTER";
        private const string VALUEBETPRFILTERTAG = "VALUEBETPRFILTER";
        private const string VALUEBETLVFILTERTAG = "VALUEBETLVFILTER";        
        private const string AUTHENKEYTAG = "AUTHENKEY";
        private const string AUTHENSAVETAG = "AUTHENSAVE";
        private const string HEARTBEATTAG = "HEARTBEAT";

        private const string SUREBETPRETAG = "SUREBET_PRE";
        private const string SUREBETLIVETAG = "SUREBET_LIVE";
        private const string VALUEBETPRETAG = "VALUEBET_PRE";
        private const string VALUEBETLIVETAG = "VALUEBET_LIVE";
        private const string SEUBETLIVETAG = "SEUBET_LIVE";     //seubet live   
        private const string SEUBETPRETAG = "SEUBET_PRE";    // seubet pre
        private const string SEUPERCENT = "SEUBET_PERCENT";  // seubet percent

        private const string JUANLIVESOCCER = "JUAN_LIVESOCCER";
        private const string PARKHORSE = "PARK_HORSE";

        private const string BETSPANUSERNAME = "BETSPAN_USERNAME";
        private const string BETSPANPASSWORD = "BETSPAN_PASSWORD";

        private const string SUREBETUSERNAME = "SUREBET_USERNAME";
        private const string SUREBETPASSWORD = "SUREBET_PASSWORD";

        private const string TRADEMATESPORTSUSERNAME = "TRADEMATESPORTS_USERNAME";
        private const string TRADEMATESPORTSPASSWORD = "TRADEMATESPORTS_PASSWORD";
        private const string TRADEMATESPORTSPUNTERID = "TRADEMATESPORTS_PUNTERID";

        private const string BETSMARTERUSERNAME = "BETSMARTER_USERNAME";
        private const string BETSMARTERPASSWORD = "BETSMARTER_PASSWORD";

        private const string BETSAPITOKEN = "BETSAPI_TOKEN";

        private const string m_strXMLFile = "gameconfig.xml";

        public const ushort GAMEPORT_MIN = 1;
        public const ushort GAMEPORT_MAX = 65535;   

        private static cServerSettings m_instance = null;

        private IPAddress m_mssqlip;
        public IPAddress DBIP
        {
            get
            {
                return m_mssqlip;
            }
            set
            {
                m_mssqlip = value;
            }
        }

        private ushort m_listenport;
        /// <summary>
        /// 대기포트
        /// </summary>
        public ushort ListenPort
        {
            get
            {
                return m_listenport;
            }
            set
            {
                if (value >= GAMEPORT_MIN && value <= GAMEPORT_MAX)
                {
                    m_listenport = value;
                }
            }
        }

        private ushort m_WebAPIport;
        /// <summary>
        /// 대기포트
        /// </summary>
        public ushort WebAPIPort
        {
            get
            {
                return m_WebAPIport;
            }
            set
            {
                if (value <= GAMEPORT_MAX)
                {
                    m_WebAPIport = value;
                }
            }
        }

        private string m_dbname;
        /// <summary>
        /// 접근할 디비명
        /// </summary>
        public string DBName
        {
            get
            {
                return m_dbname;
            }
            set
            {
                m_dbname = value;
            }
        }

        private string m_dbid;
        /// <summary>
        /// 디비로그인아이디
        /// </summary>
        public string DBID
        {
            get
            {
                return m_dbid;
            }
            set
            {
                m_dbid = value;
            }
        }

        private string m_dbpwd;
        /// <summary>
        /// 디비로그인암호
        /// </summary>
        public string DBPwd
        {
            get
            {
                return m_dbpwd;
            }
            set
            {
                m_dbpwd = value;
            }
        }

        private ushort m_dbport;
        /// <summary>
        /// 디비접속포트
        /// </summary>
        public ushort DBPort
        {
            get
            {
                return m_dbport;
            }
            set
            {
                if (value >= 1433)
                {
                    m_dbport = value;
                }
            }
        }

        private string m_NSTServer;
        /// <summary>
        /// 디비접속포트
        /// </summary>
        public string NSTServer
        {
            get
            {
                return m_NSTServer;
            }
            set
            {
                m_NSTServer = value;
            }
        }

        public string Package1_Price { get; set; }
        public string Package2_Price { get; set; }
        public string Package3_Price { get; set; }
        public string Package4_Price { get; set; }
        public string Package5_Price { get; set; }

        public bool BBIsAPIToken { get; set; }
        public string BBToken { get; set; }
        public string BBFilterSurebetPr { get; set; }
        public string BBFilterSurebetLv { get; set; }
        public string BBFilterValuebetPr { get; set; }
        public string BBFilterValuebetLv { get; set; }

        public bool EnableSurebet_Pre { get; set; }
        public bool EnableSurebet_Live { get; set; }
        public bool EnableValuebet_Pre { get; set; }
        public bool EnableValuebet_Live { get; set; }

        public bool EnableSeubet_Prematch { get; set; }  //   seubet pre    
        public bool EnableSeubet_Live {  get; set; }    // seubet live
        public string Percent_Price { get; set; }      //seubet percent

        public string Label { get; set; }

        public string JuanLiveSoccerUrl { get; set; }
        public string ParkHorseUrl { get; set; }

        public string BetspanUsername { get; set; }
        public string BetspanPassword { get; set; }

        public string BetsmarterUsername { get; set; }
        public string BetsmarterPassword { get; set; }

        public string SurebetUsername { get; set; }
        public string SurebetPassword { get; set; }

        public string SeubetUsername { get; set; }
        public string SeubetPassword { get; set; }   

        public string TradematesportsUsername { get; set; }
        public string TradematesportsPassword { get; set; }
        public string TradematesportsPunterId { get; set; }

        public string BetsapiToken { get; set; }
        public uint Version { get; set; }
        public int randomUser { get; set; }


        private bool m_bHeartBeat;
        /// <summary>
        /// 클라이언트 하트비트검사
        /// </summary>
        public bool HeartBeat
        {
            get
            {
                return m_bHeartBeat;
            }
            set
            {
                m_bHeartBeat = value;
            }
        }

        public cServerSettings()
        {
            m_listenport = 13000;       
            m_WebAPIport = 16000;       
            m_mssqlip = this.GetNetworkIPAddress();
            m_dbname = "vipseastory";
            m_dbid = "sa";
            m_dbpwd = "sa";
            m_dbport = 1433;
            m_bHeartBeat = true;
            Label = "";
            Version = 0;

            Package1_Price = "1500";
            Package2_Price = "800";
            Package3_Price = "350";
            Package4_Price = "250";
            Package5_Price = "150";
        }

        public static cServerSettings GetInstance()
        {
            if (m_instance == null)
            {
                m_instance = new cServerSettings();
            }
            return m_instance;
        }

        /// <summary>
        /// 네트웍카드에 할당한 아이피주소를 얻는다.
        /// </summary>
        /// <returns>카드에 할당된 IPAddress오브젝트(없는 경우 Loopback주소가 된다.)</returns>
        public IPAddress GetNetworkIPAddress()
        {
            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                if (nics == null || nics.Length < 1)
                {
                    return IPAddress.Loopback;
                }
                NetworkInterface adapter = nics[0]; // 첫번째 장치를 선택한다.
                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                UnicastIPAddressInformationCollection uniCast = adapterProperties.UnicastAddresses;
                if (uniCast != null)
                {
                    if (uniCast.Count > 0)
                    {
                        return uniCast[0].Address;
                    }
                }
                return IPAddress.Loopback;
            }
            catch (Exception)
            {
                return IPAddress.Loopback;
            }
        }

     
        public void LoadSettings()
        {
            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(m_strXMLFile);
                XmlNodeList xmlNodList = xmlDoc.GetElementsByTagName("config");
                XmlNode xmlNode = xmlNodList.Item(0);
                XmlNode node = xmlNode.FirstChild;
                while (node != null)
                {
                    switch (node.Name)
                    {
                        case DBIPTAG:
                            DBIP = IPAddress.Parse(node.InnerText);
                            break;
                        case LISTENPORTTAG:
                            ListenPort = Convert.ToUInt16(node.InnerText);
                            break;
                        case NSTSERVERTAG:
                            NSTServer = node.InnerText;
                            break;
                        case WEBAPIPORTTAG:
                            WebAPIPort = Convert.ToUInt16(node.InnerText);
                            break;
                        case DBNAMETAG:
                            m_dbname = node.InnerText;
                            break;
                        case DBIDTAG:
                            m_dbid = node.InnerText;
                            break;
                        case DBPWDTAG:
                            m_dbpwd = node.InnerText;
                            break;
                        case DBPORTTAG:
                            DBPort = Convert.ToUInt16(node.InnerText);
                            break;
                        case BBUSERTAG:
                            BBToken = node.InnerText;
                            break;
                        case BBISAPITOKEN:
                            BBIsAPIToken = Convert.ToBoolean(node.InnerText);
                            break;
                        case SUREBETPRFILTERTAG:
                            BBFilterSurebetPr = node.InnerText;
                            break;
                        case SUREBETLVFILTERTAG:
                            BBFilterSurebetLv = node.InnerText;
                            break;
                        case VALUEBETPRFILTERTAG:
                            BBFilterValuebetPr = node.InnerText;
                            break;
                        case VALUEBETLVFILTERTAG:
                            BBFilterValuebetLv = node.InnerText;
                            break;
                        case HEARTBEATTAG:
                            m_bHeartBeat = Convert.ToBoolean(node.InnerText);
                            break;
                        case SUREBETPRETAG:
                            EnableSurebet_Pre = Convert.ToBoolean(node.InnerText);
                            break;
                        case SUREBETLIVETAG:
                            EnableSurebet_Live = Convert.ToBoolean(node.InnerText);
                            break;
                        case VALUEBETPRETAG:
                            EnableValuebet_Pre = Convert.ToBoolean(node.InnerText);
                            break;
                        case VALUEBETLIVETAG:
                            EnableValuebet_Live = Convert.ToBoolean(node.InnerText);
                            break;
                        case SEUBETLIVETAG:
                            EnableSeubet_Live = Convert.ToBoolean(node.InnerText);//   seubet liv  
                            break;
                        case SEUBETPRETAG:
                            EnableSeubet_Prematch = Convert.ToBoolean(node.InnerText);//   seubet pre  
                            break;
                        case SEUPERCENT:
                            Percent_Price = node.InnerText;  // seubet percent
                            break;
                        case BETSAPITOKEN:
                            BetsapiToken = node.InnerText;
                            break;
                        case JUANLIVESOCCER:
                            JuanLiveSoccerUrl = node.InnerText;
                            break;
                        case PARKHORSE:
                            ParkHorseUrl = node.InnerText;
                            break;
                        case BETSPANUSERNAME:
                            BetspanUsername = node.InnerText;
                            break;
                        case BETSPANPASSWORD:
                            BetspanPassword = node.InnerText;
                            break;
                        case SUREBETUSERNAME:
                            SurebetUsername = node.InnerText;
                            break;
                        case SUREBETPASSWORD:
                            SurebetPassword = node.InnerText;
                            break;
                        case TRADEMATESPORTSUSERNAME:
                            TradematesportsUsername = node.InnerText;
                            break;
                        case TRADEMATESPORTSPASSWORD:
                            TradematesportsPassword = node.InnerText;
                            break;
                        case TRADEMATESPORTSPUNTERID:
                            TradematesportsPunterId = node.InnerText;
                            break;
                        case BETSMARTERUSERNAME:
                            BetsmarterUsername = node.InnerText;
                            break;
                        case BETSMARTERPASSWORD:
                            BetsmarterPassword = node.InnerText;
                            break;
                        case AUTHENKEYTAG:
                            Label = node.InnerText;
                            break;
                        case AUTHENSAVETAG:
                            Version = Convert.ToUInt32(node.InnerText);
                            break;

                    }
                    node = node.NextSibling;
                }
            }
            catch (Exception)
            {

            }
        }

        // 동작: 설정값들을 설정파일에 보관한다.
        // 결과값: TRUE 성공, FALSE 실패
        public bool SaveSetting()
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml("<config/>");
                XmlElement root = xmlDoc.DocumentElement;
                string strTemp;

                XmlDocumentFragment docFrag = xmlDoc.CreateDocumentFragment();

                strTemp = string.Format("<{0}>{1}</{2}>", LISTENPORTTAG, m_listenport, LISTENPORTTAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", WEBAPIPORTTAG, m_WebAPIport, WEBAPIPORTTAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", NSTSERVERTAG, m_NSTServer, NSTSERVERTAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);
                
                strTemp = string.Format("<{0}>{1}</{2}>", DBIPTAG, m_mssqlip, DBIPTAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", DBNAMETAG, m_dbname, DBNAMETAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", DBIDTAG, m_dbid, DBIDTAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", DBPWDTAG, m_dbpwd, DBPWDTAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", DBPORTTAG, m_dbport, DBPORTTAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", BBUSERTAG, BBToken, BBUSERTAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", BBISAPITOKEN, BBIsAPIToken, BBISAPITOKEN);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);
                
                strTemp = string.Format("<{0}>{1}</{2}>", SUREBETPRFILTERTAG, BBFilterSurebetPr, SUREBETPRFILTERTAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", SUREBETLVFILTERTAG, BBFilterSurebetLv, SUREBETLVFILTERTAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", VALUEBETPRFILTERTAG, BBFilterValuebetPr, VALUEBETPRFILTERTAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", VALUEBETLVFILTERTAG, BBFilterValuebetLv, VALUEBETLVFILTERTAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", AUTHENKEYTAG, Label, AUTHENKEYTAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", AUTHENSAVETAG, Version, AUTHENSAVETAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);
                
                strTemp = string.Format("<{0}>{1}</{2}>", HEARTBEATTAG, m_bHeartBeat, HEARTBEATTAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", SUREBETPRETAG, EnableSurebet_Pre, SUREBETPRETAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", SUREBETLIVETAG, EnableSurebet_Live, SUREBETLIVETAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", VALUEBETPRETAG, EnableValuebet_Pre, VALUEBETPRETAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", VALUEBETLIVETAG, EnableValuebet_Live, VALUEBETLIVETAG);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", SEUBETLIVETAG, EnableSeubet_Live, SEUBETLIVETAG);    //seubet liv
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", SEUBETPRETAG, EnableSeubet_Prematch, SEUBETPRETAG);  // seubet pre
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", SEUPERCENT, Percent_Price, SEUPERCENT);   // seubet percent
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);


                strTemp = string.Format("<{0}>{1}</{2}>", JUANLIVESOCCER, JuanLiveSoccerUrl, JUANLIVESOCCER);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", PARKHORSE, ParkHorseUrl, PARKHORSE);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", BETSAPITOKEN, BetsapiToken, BETSAPITOKEN);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", BETSPANUSERNAME, BetspanUsername, BETSPANUSERNAME);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", BETSPANPASSWORD, BetspanPassword, BETSPANPASSWORD);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", SUREBETUSERNAME, SurebetUsername, SUREBETUSERNAME);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", SUREBETPASSWORD, SurebetPassword, SUREBETPASSWORD);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", TRADEMATESPORTSUSERNAME, TradematesportsUsername, TRADEMATESPORTSUSERNAME);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", TRADEMATESPORTSPASSWORD, TradematesportsPassword, TRADEMATESPORTSPASSWORD);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", TRADEMATESPORTSPUNTERID, TradematesportsPunterId, TRADEMATESPORTSPUNTERID);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", BETSMARTERUSERNAME,BetsmarterUsername, BETSMARTERUSERNAME);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                strTemp = string.Format("<{0}>{1}</{2}>", BETSMARTERPASSWORD, BetsmarterPassword, BETSMARTERPASSWORD);
                docFrag.InnerXml = strTemp;
                root.AppendChild(docFrag);

                xmlDoc.Save(m_strXMLFile);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
