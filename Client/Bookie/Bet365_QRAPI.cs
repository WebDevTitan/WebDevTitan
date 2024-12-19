using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using log4net;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Schema;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrbitBot.Controller;
using OrbitBot.Json;
using PlaywrightSharp.Har;
using Project.Helphers;
using Protocol;
using WebSocketSharp;
using static MasterDevs.ChromeDevTools.Protocol.Chrome.ProtocolName;

namespace Project.Bookie
{
#if (BET365_QRAPI)
    class Bet365_qrapiCtrl : IBookieController
    {
        public delegate void onWriteStatusEvent(string status);
        public bool m_isLogged = false;
        public string CSRF = Utils.GetRandomHexNumber(32);
        protected string BASE_URL = "https://Qrsolver.com";
        protected CookieContainer m_cookieContainer;
        protected HttpClient m_httpClient = null;
        protected Thread _mainThread = null;
        protected bool _bWorking = false;
        private int _threadCounter = -1;
        private string _token;
        private string appkey;
        private string sessionToken;
        private double m_balance;
        private string _csrfToken;  
        string strContent = string.Empty;
        public string BF_FP { get; private set; }
        public List<BetburgerInfo> m_oddsChangedBetList = new List<BetburgerInfo>();
        public List<BetburgerInfo> m_triedBetList = new List<BetburgerInfo>();
        private DateTime _lastKeepSessionAlive;
        public string sessionId;
        private List<BetburgerInfo> _betList = new List<BetburgerInfo>();
        public string proxy = "http://jacobspam1946:155fd1@node-de-91.astroproxy.com:10241";



      
        public Bet365_qrapiCtrl()
        {
            m_cookieContainer = new CookieContainer();
            MySQLConnector.CreateInstance();
            MySQLConnector.instance.DoConnect();
            if (MySQLConnector.instance.IsLogged == false) return;
            
            InitHttpClient();
            startListening();                   
        }

        public void startListening()
        {
            try
            {
                string sessionId = ReadRegistry("sessionId");                
                getBalance();
                login();
                logLogin();                
                RetrieveAllSession();                
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"Exception in startListening: {ex.Message}");
            }
        }
        public double getBalance()
        {
            double dbalance = -1;
            HttpResponseMessage response = null;
            string strContent = "";
            try
            {
                response = m_httpClient.GetAsync($"{BASE_URL}/api/placebet/session/{sessionId}/balance/").Result;
                response.EnsureSuccessStatusCode();
                strContent = response.Content.ReadAsStringAsync().Result;
                JObject obj = JsonConvert.DeserializeObject<JObject>(strContent);
                dbalance = Convert.ToDouble(obj["balance"].ToString());
                LogMng.Instance.onWriteStatus($"Current balance: {dbalance}");
                //MySQLConnector.instance.UpdateBalance(dbalance);

                response = m_httpClient.GetAsync($"{BASE_URL}/api/placebet/session/{sessionId}/openbets/").Result;
                strContent = response.Content.ReadAsStringAsync().Result;
                obj = JsonConvert.DeserializeObject<JObject>(strContent);

                double totalStake = Convert.ToDouble(obj["stake"].ToString());
                double totalReturns = Convert.ToDouble(obj["total_returns"].ToString());
                double cashoutReturns = Convert.ToDouble(obj["cashout_returns"].ToString());
                LogMng.Instance.onWriteStatus($"Total Stake: {totalStake}, Total Returns: {totalReturns}, Cashout Returns: {cashoutReturns}");
                //MySQLConnector.instance.UpdateBalance(dbalance + cashoutReturns);
            }
            catch (Exception ex) { }
            m_balance = dbalance;
            return dbalance;
        }

        public void writeStatus(string status)
        {
            MessageBox.Show(status);
        }
        protected virtual void ChangeDefaultHeaders()
        {
            m_httpClient.DefaultRequestHeaders.Clear();
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html, application/xhtml+xml, application/xml; q=0.9, image/webp, */*; q=0.8");
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Setting.Instance.dolphinId);
        }
        public bool login()
        {
            DoLogin();
            Thread.Sleep(3000);
            if (strContent == null) return false;
            else return true;
            
        }
        public void logLogin()
        {
            try
            {
                if (login() == true)
                    LogMng.Instance.onWriteStatus("Login Success!!");
            }
            catch(Exception ex)
            {
                LogMng.Instance.onWriteStatus($"Exception in doLogin: {ex.Message}");
            }
        }
        public string DoLogin()
        {
            try
            {
                dynamic input = new JObject();
                StringContent postData = null;
                HttpResponseMessage response = null;
                string strContent = string.Empty;
                JObject obj = null;
                string url = string.Empty;
            create_session: if (string.IsNullOrEmpty(sessionId))
                {
                    input = new JObject();
                    input["domain"] = Setting.Instance.domain;
                    input["username"] = Setting.Instance.username;
                    input["password"] = Setting.Instance.password;
                    input["proxy"] = proxy;

                    postData = new StringContent(input.ToString(), Encoding.UTF8, "application/json");
                    url = $"{BASE_URL}/api/placebet/create/";
                    
                    LogMng.Instance.onWriteStatus(url);
                    response = m_httpClient.PostAsync(url, postData).Result;
                    response.EnsureSuccessStatusCode();
                    strContent = response.Content.ReadAsStringAsync().Result;
                    obj = JsonConvert.DeserializeObject<JObject>(strContent);
                    LogMng.Instance.onWriteStatus(obj.ToString());
                    if (obj["session_id"] == null) return string.Empty;
                    sessionId = obj["session_id"].ToString();
                }

                _lastKeepSessionAlive = DateTime.Now;
                input = new JObject();
                input["domain"] = Setting.Instance.domain;
                input["username"] = Setting.Instance.username;
                input["password"] = Setting.Instance.password;
                input["proxy"] = proxy;

                postData = new StringContent(input.ToString(), Encoding.UTF8, "application/json");
                url = $"{BASE_URL}/api/placebet/session/{sessionId}/login/";
                LogMng.Instance.onWriteStatus(url);
                response = m_httpClient.PostAsync(url, postData).Result;
                strContent = response.Content.ReadAsStringAsync().Result;
                obj = JsonConvert.DeserializeObject<JObject>(strContent);
                LogMng.Instance.onWriteStatus(obj.ToString());

                if (strContent.Contains("you must create a new session"))
                {

                    sessionId = string.Empty;
                    savesettingInfo();
                    goto create_session;
                }
                if (obj["code"] != null && obj["code"].ToString() == "ERR_NOT_FOUND")
                {
                    sessionId = string.Empty;
                    savesettingInfo();
                    goto create_session;
                }
                response.EnsureSuccessStatusCode();

            }
            catch
            {
            }
            return string.Empty;
        }       
        public void savesettingInfo()
        {
            WriteRegistry("sessionId", sessionId);            
        }
        public void WriteRegistry(string KeyName, string KeyValue)
        {
            try
            {
                Registry.CurrentUser.CreateSubKey("SoftWare").CreateSubKey("Cronos_Bet365_" + Setting.Instance.username).SetValue(KeyName, (object)KeyValue);
            }
            catch
            {

            }
        }
        public string ReadRegistry(string KeyName)
        {
            try
            {
                 return Registry.CurrentUser.CreateSubKey("SoftWare").CreateSubKey("Cronos_Bet365_" + Setting.Instance.username).GetValue(KeyName, (object)"").ToString();
            }
            catch
            {

            }
            return string.Empty;
        }        

        public bool startNewTrade(BetburgerInfo betItem)
        {
            foreach (BetburgerInfo info in m_oddsChangedBetList)
            {
                if (info.direct_link == betItem.direct_link)
                {
                    return false;
                }
            }
            if (getTriedCount(betItem) > 5) return false;
            string betexpression = string.Format("{0}|{1}|{2}|@{3}|{4}%", betItem.tipster, betItem.eventTitle, betItem.outcome, betItem.odds, betItem.percent);
            LogMng.Instance.onWriteStatus(betexpression);
            //double balance = getBalance();
            if (string.IsNullOrEmpty(sessionId))
            {
                DoLogin();
            }
            bool bRet = betRace(betItem);
            //if (bRet == false)
            //{
            //    addTriedBet(betItem);
            //}
            Thread.Sleep(3000);
            return bRet;
        }
        public int getTriedCount(BetburgerInfo newitem)
        {
            int triedCount = 0;
            foreach (BetburgerInfo item in m_triedBetList)
            {
                if (item.direct_link == newitem.direct_link)
                {
                    triedCount += item.retryCount;
                }
            }
            return triedCount;
        }
        //public void addTriedBet(BetburgerInfo newitem)
        //{
        //    foreach (BetburgerInfo item in m_triedBetList)
        //    {
        //        if (item.direct_link == newitem.direct_link)
        //        {
        //            item.retryCount++;
        //            return;
        //        }
        //    }
        //    m_triedBetList.Add(newitem);
        //}

        public bool betRace(BetburgerInfo betItem)
        {
            try
            {
                LogMng.Instance.onWriteStatus("");
                double stake = betItem.stake;
                string[] tmpArray = betItem.direct_link.Split(';')[0].Split('|');
                int retryCounter = 0;
                Random rnd = new Random();
                Thread.Sleep(100 * rnd.Next(10));

            placeBet:
                retryCounter++;
                dynamic input = new JObject();
                stake = Math.Floor(stake);
                //if (stake > Setting.instance.flatStake)
                //{
                //    stake = Setting.instance.flatStake;
                //}
                input["stake"] = stake.ToString().Replace(",", ".");
                JArray selections = new JArray();
                JObject selection = new JObject();
                //selection["sport_id"] = string.IsNullOrEmpty(betItem.sportID) ? getB365SpIdByName(betItem.sport) : int.Parse(betItem.sportID);
                if (betItem.direct_link.Contains("FI="))
                {
                    selection["fi"] = Utils.Between(betItem.direct_link, "FI=", "&");
                }
                else
                {
                    selection["fi"] = tmpArray[2];
                }
                if (betItem.direct_link.Contains("ID="))
                {
                    selection["id"] = Utils.Between(betItem.direct_link, "ID=", "&");
                }
                else
                {
                    selection["id"] = tmpArray[0];
                }

                //if (!string.IsNullOrEmpty(betItem.line))
                //{
                //    selection["line"] = betItem.line;
                //}
                //if (!string.IsNullOrEmpty(betItem.strOD)) selection["odd"] = betItem.strOD;
                //else selection["odd"] = Utils.decimalToFraction(betItem.odds);

                selection["accept_min_odd"] = 1.2;
                selection["accept_max_odd"] = betItem.odds * 2;
                //if (betItem.bEW)
                //    selection["eachway"] = true;

                selections.Add(selection);
                input["selections"] = selections;

                //if (!string.IsNullOrEmpty(betItem.errorMessage))
                //{
                //    LogMng.Instance.onWriteStatus(betItem.errorMessage);
                //}

                LogMng.Instance.onWriteStatus(betItem.direct_link);
                LogMng.Instance.onWriteStatus(input.ToString());

                var postData = new StringContent(input.ToString(), Encoding.UTF8, "application/json");
                HttpResponseMessage responseMessageBet = m_httpClient.PostAsync($"{BASE_URL}/api/placebet/session/{sessionId}/placebet/", postData).Result;
                string strMessageBet = responseMessageBet.Content.ReadAsStringAsync().Result;
                LogMng.Instance.onWriteStatus(strMessageBet);
                if (strMessageBet.Contains("ERR_NOT_FOUND"))
                {
                    sessionId = string.Empty;                    
                    DoLogin();
                    goto placeBet;
                }

                JObject jsonResp = JObject.Parse(strMessageBet);
                string apiStatus = jsonResp["result"].ToString();
                string betStatus = jsonResp["b365_result"].ToString();

                if (apiStatus == "OK")
                {
                    LogMng.Instance.onWriteStatus(string.Format("SUCCESS Stake: {0}", stake));                    
                }
                else if (apiStatus == "CHANGED")
                {
                    double newOdds = Utils.FractionToDouble(jsonResp["selections"][0]["odd"].ToString());
                    LogMng.Instance.onWriteStatus(string.Format("Odds chagned from {0} to {1}", betItem.odds, newOdds));
                    m_oddsChangedBetList.Add(betItem);
                }
                else if (apiStatus == "BET_REFERRAL_REQUIRED")
                {
                    double max_stake = Utils.FractionToDouble(jsonResp["selections"][0]["max_stake"].ToString());
                    stake = Math.Floor(max_stake);
                    goto placeBet;
                }
                else
                {
                    if (retryCounter >= 3) return false;
                    if (apiStatus == "SESSION_LOCKED")
                    {
                        Thread.Sleep(1000);
                        goto placeBet;
                    }
                    else if (apiStatus == "GENERAL_ERROR")
                    {
                        Thread.Sleep(1000);
                        goto placeBet;
                    }
                    else if (apiStatus == "NOT_LOGGED_IN")
                    {
                        DoLogin();
                        goto placeBet;
                    }
                    else
                    {
                        LogMng.Instance.onWriteStatus(string.Format("FAILURE Stake: {0}", stake));
                    }
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus("Exception in placeBet : " + ex.ToString());
            }
            return false;
        }

        protected void InitHttpClient()
        {

            HttpClientHandler handler = new HttpClientHandler();

            handler.CookieContainer = m_cookieContainer;
            m_httpClient = new HttpClient(handler);
            m_httpClient.Timeout = new TimeSpan(0, 0, 100);
            ServicePointManager.DefaultConnectionLimit = 2;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            m_httpClient.DefaultRequestHeaders.ExpectContinue = false;
            ChangeDefaultHeaders();
            m_cookieContainer.Add(new Uri(BASE_URL), new Cookie("CSRF-TOKEN", CSRF));
        }

        public void RetrieveAllSession()
        {
            try
            {
                sessionId = string.Empty;
                var response = m_httpClient.GetAsync($"{BASE_URL}/api/placebet/session/").Result;
                string strContent = response.Content.ReadAsStringAsync().Result;
                LogMng.Instance.onWriteStatus(strContent);
                var obj = JsonConvert.DeserializeObject<JArray>(strContent);
                foreach (var item in obj)
                {
                    string account = item["account"].ToString();
                    string session_id = item["id"].ToString();
                    LogMng.Instance.onWriteStatus($"{account} => {session_id}");
                    if (account.ToLower() == Setting.Instance.username.ToLower())
                    {
                        sessionId = session_id;
                        //break;
                    }
                }
                LogMng.Instance.onWriteStatus($"session id = {sessionId}");                              
            }
            catch (Exception ex)
            {

            }
        }

        public PROCESS_RESULT PlaceBet(ref BetburgerInfo info)
        {
            throw new NotImplementedException();
        }
        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Feature()
        {
            throw new NotImplementedException();
        }        

        public int GetPendingbets()
        {
            throw new NotImplementedException();
        }

        public string getProxyLocation()
        {
            // Implementation here
            return "Your proxy location implementation";
        }

        public HttpClient initHttpClient(bool bUseNewCookie)
        {
            throw new NotImplementedException();
        }       

        public bool logout()
        {
            return true;
        }

        

        public bool Pulse()
        {
            throw new NotImplementedException();
        }
    }

#endif
}
