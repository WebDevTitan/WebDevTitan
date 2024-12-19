using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using BetburgerServer.Constant;

namespace SeastoryServer
{
    
    public class MYSqlMng : mysql
    {
        protected static MYSqlMng m_Instance;
        private bool m_bIsShowErrLog = true;

        /// <summary>

        /// </summary>
        private bool m_bForceConnectionChanged = false;


        /// </summary>
        public bool ShowErrLog
        {
            get
            {
                return m_bIsShowErrLog;
            }
            set
            {
                m_bIsShowErrLog = value;
            }
        }


        /// </summary>
        public bool ReconnectMode = false;

        public MYSqlMng()
        {
            this.IsConnected = false;
            this.DBID = "root";
            this.DBPwd = "";
            this.DBServer = "127.0.0.1";
            this.DBName = "betting";
            this.DBPort = "3306";
        }

        public static MYSqlMng GetInstance()
        {
            if (m_Instance == null)
            {
                m_Instance = new MYSqlMng();
            }
            return m_Instance;
        }

       
        public override bool ConnectDBServer()
        {
            if (IsConnected)
            {
                string strMsg;

                strMsg = string.Format("Connected to db server already.");
                GameServer.GetInstance().SendLog(strMsg, null);
                return true;
            }
            else
            {
                string strTemp;

                strTemp = string.Format("server={0};port={1};uid={2};pwd={3};database={4};Charset=utf8",
                    DBServer, DBPort, DBID, DBPwd, DBName);
                try
                {
                    if (m_dbConnection != null)
                    {
                        m_bForceConnectionChanged = true;
                        m_dbConnection.Close();
                        m_dbConnection.Dispose();
                    }
                    m_dbConnection = new MySqlConnection(strTemp);
                    m_bForceConnectionChanged = true;
                    m_dbConnection.StateChange += new StateChangeEventHandler(OnSqlConnectionStateChange);
                    m_dbConnection.Open();
                }
                catch (Exception ex)
                {
                    strTemp = string.Format("Cannot connect to db server. DB IP:{0} DB Name:{1} DB ID:{2} DB Pass:{3} DB Port:{4} ",
                        this.DBServer, this.DBName, this.DBID, this.DBPwd, this.DBPort);
                    GameServer.GetInstance().SendLog(strTemp, ex);
                    m_dbConnection = null;
                    return false;
                }
                GameServer.GetInstance().SendLog("Connected to db server successfully.", null);

                this.IsConnected = true;
                return true;
            }
        }

        public bool ConnectDBServer(string strDBName)
        {
            if (IsConnected)
            {
                string strMsg;

                strMsg = string.Format("Connected to db server already.");
                GameServer.GetInstance().SendLog(strMsg, null);
                return true;
            }
            else
            {
                string strTemp;

                strTemp = string.Format("server={0};port={1};uid={2};pwd={3};database={4};Charset=utf8",
                    DBServer, DBPort, DBID, DBPwd, DBName);
                try
                {
                    if (m_dbConnection != null)
                    {
                        m_bForceConnectionChanged = true;
                        m_dbConnection.Close();
                        m_dbConnection.Dispose();
                    }
                    m_dbConnection = new MySqlConnection(strTemp);
                    m_bForceConnectionChanged = true;
                    m_dbConnection.StateChange += new StateChangeEventHandler(OnSqlConnectionStateChange);
                    m_dbConnection.Open();
                }
                catch
                {
                    
                    m_dbConnection = null;
                    return false;
                }

                this.IsConnected = true;
                return true;
            }
        }

        public override bool DisconnectDBServer()
        {
            if (!this.IsConnected)
                return true;
            m_bForceConnectionChanged = true;
            m_dbConnection.Close();
            this.IsConnected = false;
            return true;
        }

        private void OnSqlConnectionStateChange(object sender, StateChangeEventArgs e)
        {
            if (this.ReconnectMode)
                return;
            if (sender == null) return;
            if (m_bForceConnectionChanged)
            {
                m_bForceConnectionChanged = false;
                return;
            }

            if (e.OriginalState == ConnectionState.Open && e.CurrentState == ConnectionState.Closed)
            {
                GameServer.GetInstance().SendLog("Disconnected from db server. trying to connect", null);
                DisconnectDBServer();
                if (!ConnectDBServer() || !AttachDatabase())
                {
               
                    DisconnectDBServer();
                    GameServer.GetInstance().Close();
                    return;
                }
            }
        }

        public bool AttachDatabase()
        {
            if (!this.IsConnected)
            {
                GameServer.GetInstance().SendLog("Not connected to db server.", null);
                return false;
            }
            string[] strParamNames;
            object[] paramValues;
            try
            {
                string strSQL;
                string[][] result;
                bool bExistDB = false;

                strSQL = string.Format("USE {0}", DBName);
                if (!this.UpdateQuery(strSQL))
                {
                    GameServer.GetInstance().SendLog("Cannot read db data.", null);
                    return false;
                }
                strSQL = "SHOW DATABASES";
                strParamNames = new string[1];
                strParamNames[0] = "@name";
                paramValues = new string[1];
                paramValues[0] = this.DBName;
                result = this.SelectQuery(strSQL, strParamNames, paramValues);
                if (result.Length > 0 && Utils.isExistDB(result, DBName))
                {
                    bExistDB = true;
                }

                if (bExistDB)
                {
                    strSQL = string.Format("USE {0}", this.DBName);
                    return this.UpdateQuery(strSQL);
                }
                else
                {
                    GameServer.GetInstance().SendLog("Doesn't exist database.", null);
                    return false;
                }
            }
            catch (MySqlException ex)
            {
                if (ex.ErrorCode == 0)
                {
                    if (this.IsConnected)
                    {
                        return AttachDatabase();
                    }
                }
                else
                {
                    string strMsg;
                    strMsg = string.Format("Cannot connect to {0} DB. ", this.DBName);
                    GameServer.GetInstance().SendLog(strMsg, ex);
                    return false;
                }
                return false;
            }
        }

        public bool CreateDB()
        {
            string strSQL;

            try
            {
                GameServer.GetInstance().SendLog("Creating database.", null);
                strSQL = string.Format("CREATE DATABASE {0}", this.DBName);
                this.UpdateQuery(strSQL);
                strSQL = string.Format("USE {0}", this.DBName);
                this.UpdateQuery(strSQL);
            }
            catch (Exception ex)
            {
                GameServer.GetInstance().SendLog("Error occurs while creating database. ", ex);
                return false;
            }
            return true;
        }
     
        public override string[][] SelectQuery(string strParamQuery, string[] strParamNames, object[] paramValues)
        {
            lock (this)
            {
                if (!this.IsConnected)
                {
                    if (!this.ConnectDBServer(this.DBName))
                    {
                        if (this.ShowErrLog)
                        {
                            string strLog;
                            strLog = string.Format("Cannot connect to db, query:{0}", strParamQuery);
                            GameServer.GetInstance().SendLog(strLog);
                        }
                        return new string[0][];
                    }
                }
                string[][] result = base.SelectQuery(strParamQuery, strParamNames, paramValues);
                if (this.IsErr && this.ShowErrLog)
                {
                    GameServer.GetInstance().SendLog(this.ErrMsg, null);
                }
                if (this.ReconnectMode)
                {
                    this.DisconnectDBServer();
                }
                return result;
            }
        }

        public override string[][] SelectQuery(string strQuery)
        {
            try
            {
                lock (this)
                {
                    if (!this.IsConnected)
                    {
                        if (!this.ConnectDBServer(this.DBName))
                        {
                            if (this.ShowErrLog)
                            {
                                string strLog;
                                strLog = string.Format("Cannot connect to db, query:{0}", strQuery);
                                GameServer.GetInstance().SendLog(strLog);
                            }
                            return new string[0][];
                        }
                    }
                    string[][] result = base.SelectQuery(strQuery);
                    if (this.IsErr && this.ShowErrLog)
                    {
                        GameServer.GetInstance().SendLog(this.ErrMsg, null);
                    }
                    if (this.ReconnectMode)
                    {
                        this.DisconnectDBServer();
                    }
                    return result;
                }
            }
            catch(Exception e)
            {
                GameServer.GetInstance().SendLog("SelectQuery Exception : " + e.ToString());
                return new string[0][];
            }
        }

        public override bool UpdateQuery(string strParamQuery, string[] strParamNames, object[] paramValues)
        {
            lock (this)
            {
                if (!this.IsConnected)
                {
                    if (!this.ConnectDBServer(this.DBName))
                    {
                        if (this.ShowErrLog)
                        {
                            string strLog;
                            strLog = string.Format("Cannot connect to db, query:{0}", strParamQuery);
                            GameServer.GetInstance().SendLog(strLog);
                        }
                        return false;
                    }
                }
                bool result = base.UpdateQuery(strParamQuery, strParamNames, paramValues);
                if (this.IsErr && this.ShowErrLog)
                {
                    GameServer.GetInstance().SendLog(this.ErrMsg, null);
                }
                if (this.ReconnectMode)
                {
                    this.DisconnectDBServer();
                }
                return result;
            }
        }

        public override bool UpdateQuery(string strQuery)
        {
            try
            {
                lock (this)
                {
                    if (!this.IsConnected)
                    {
                        if (!this.ConnectDBServer(this.DBName))
                        {
                            if (this.ShowErrLog)
                            {
                                string strLog;
                                strLog = string.Format("Cannot connect to db, query:{0}", strQuery);
                                GameServer.GetInstance().SendLog(strLog);
                            }
                            return false;
                        }
                    }
                    bool result = base.UpdateQuery(strQuery);
                    if (this.IsErr && this.ShowErrLog)
                    {
                        GameServer.GetInstance().SendLog(this.ErrMsg, null);
                    }
                    if (this.ReconnectMode)
                    {
                        this.DisconnectDBServer();
                    }
                    return result;
                }
            }
            catch(Exception e)
            {
                GameServer.GetInstance().SendLog("Update Query Exception : " + e.ToString());
                return false;
            }
        }

        public override bool DeleteQuery(string strParamQuery, string[] strParamNames, object[] paramValues)
        {
            lock (this)
            {
                if (!this.IsConnected)
                {
                    if (!this.ConnectDBServer(this.DBName))
                    {
                        if (this.ShowErrLog)
                        {
                            string strLog;
                            strLog = string.Format("Cannot connect to db, query:{0}", strParamQuery);
                            GameServer.GetInstance().SendLog(strLog);
                        }
                        return false;
                    }
                }
                bool result = base.DeleteQuery(strParamQuery, strParamNames, paramValues);
                if (this.IsErr && this.ShowErrLog)
                {
                    GameServer.GetInstance().SendLog(this.ErrMsg, null);
                }
                if (this.ReconnectMode)
                {
                    this.DisconnectDBServer();
                }
                return result;
            }
        }

    
        public override bool DeleteQuery(string strQuery)
        {
            lock (this)
            {
                if (!this.IsConnected)
                {
                    if (!this.ConnectDBServer(this.DBName))
                    {
                        if (this.ShowErrLog)
                        {
                            string strLog;
                            strLog = string.Format("Cannot connect to db, query:{0}", strQuery);
                            GameServer.GetInstance().SendLog(strLog);
                        }
                        return false;
                    }
                }
                bool result = base.DeleteQuery(strQuery);
                if (this.IsErr && this.ShowErrLog)
                {
                    GameServer.GetInstance().SendLog(this.ErrMsg, null);
                }
                if (this.ReconnectMode)
                {
                    this.DisconnectDBServer();
                }
                return result;
            }
        }

   
        public override long InsertQuery(string strParamQuery, string[] strParamNames, object[] paramValues)
        {
            lock (this)
            {
                if (!this.IsConnected)
                {
                    if (!this.ConnectDBServer(this.DBName))
                    {
                        if (this.ShowErrLog)
                        {
                            string strLog;
                            strLog = string.Format("Cannot connect to db, query:{0}", strParamQuery);
                            GameServer.GetInstance().SendLog(strLog);
                        }
                        return 0;
                    }
                }
                long result = base.InsertQuery(strParamQuery, strParamNames, paramValues);
                if (this.IsErr && this.ShowErrLog)
                {
                    GameServer.GetInstance().SendLog(this.ErrMsg, null);
                }
                if (this.ReconnectMode)
                {
                    this.DisconnectDBServer();
                }
                return result;
            }
        }

        public override long InsertQuery(string strQuery)
        {
            try
            {
                lock (this)
                {
                    if (!this.IsConnected)
                    {
                        if (!this.ConnectDBServer(this.DBName))
                        {
                            if (this.ShowErrLog)
                            {
                                string strLog;
                                strLog = string.Format("Cannot connect to db, query:{0}", strQuery);
                                GameServer.GetInstance().SendLog(strLog);
                            }
                            return 0;
                        }
                    }
                    long result = base.InsertQuery(strQuery);
                    if (this.IsErr && this.ShowErrLog)
                    {
                        GameServer.GetInstance().SendLog(this.ErrMsg, null);
                    }
                    if (this.ReconnectMode)
                    {
                        this.DisconnectDBServer(); 
                    }
                    return result;
                }
            }
            catch(Exception e)
            {
                GameServer.GetInstance().SendLog("InsertQuery Exception : " + e.ToString());
                return 0;
            }
        }
    }
}
