using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Tls;

namespace SeastoryServer
{
    public abstract class mysql
    {
        // Fields
        protected const int DBCONN_TIMEOUT = 30;
        private bool m_bIsConnected;
        protected MySqlConnection m_dbConnection;
        private string m_strDBID = "root";
        private string m_strDBName = "betting";
        private string m_strDBPort = "3306";
        private string m_strDBPwd = "";
        private string m_strDBServer = "127.0.0.1";
        private string m_strErrMsg = "";

        // Methods
        protected mysql()
        {
        }

        public virtual bool ConnectDBServer()
        {
            if (!this.IsConnected)
            {
                string connectionString = string.Format("server={0};port={1};uid={2};pwd={3};database={4};Charset=utf8",
                    m_strDBServer, m_strDBPort, m_strDBID, m_strDBPwd, m_strDBName);
                try
                {
                    if (this.m_dbConnection != null)
                    {
                        this.m_dbConnection.Close();
                        this.m_dbConnection.Dispose();
                    }
                    this.m_dbConnection = new MySqlConnection(connectionString);
                    this.m_dbConnection.Open();
                }
                catch (Exception exception)
                {
                    //DB Connection Failed
                    //Make a Error Text
                    this.ErrMsg = "DB Connection Error";
                    this.m_dbConnection = null;
                    return false;
                }
                this.IsConnected = true;
            }
            return true;
        }

        public virtual bool DeleteQuery(string strQuery)
        {
            if (!this.IsConnected)
            {
                return false;
            }
            try
            {
                lock (this)
                {
                    MySqlDataAdapter adapter = new MySqlDataAdapter
                    {
                        DeleteCommand = new MySqlCommand(strQuery, this.m_dbConnection)
                    };
                    adapter.DeleteCommand.ExecuteNonQuery();
                }
                return true;
            }
            catch (MySqlException exception)
            {
                if (exception.ErrorCode == 0)
                {
                    if (this.IsConnected)
                    {
                        return this.DeleteQuery(strQuery);
                    }
                }
                else
                {
                    this.ErrMsg = "SQL Query Error";
                }
                return false;
            }
        }

        public virtual bool DeleteQuery(string strParamQuery, string[] strParamNames, object[] paramValues)
        {
            if (!this.IsConnected)
            {
                return false;
            }
            if (strParamNames.Length != paramValues.Length)
            {
                //

                return false;
            }
            string cmdText = "";
            try
            {
                lock (this)
                {
                    cmdText = string.Format(strParamQuery, (object[])strParamNames);
                    MySqlDataAdapter adapter = new MySqlDataAdapter();
                    MySqlCommand command = new MySqlCommand(cmdText, this.m_dbConnection);
                    for (int i = 0; i < strParamNames.Length; i++)
                    {
                        command.Parameters.AddWithValue(strParamNames[i], paramValues[i]);
                    }
                    adapter.DeleteCommand = command;
                    adapter.DeleteCommand.ExecuteNonQuery();
                }
                return true;
            }
            catch (MySqlException exception)
            {
                if (exception.ErrorCode == 0)
                {
                    if (this.IsConnected)
                    {
                        return this.DeleteQuery(cmdText, strParamNames, paramValues);
                    }
                }
                else
                {
                   //Error Process
                }
                return false;
            }
        }

        public virtual bool DisconnectDBServer()
        {
            if (this.IsConnected)
            {
                this.m_dbConnection.Close();
                this.IsConnected = false;
            }
            return true;
        }

        public virtual long InsertQuery(string strQuery)
        {
            if (!this.IsConnected)
            {
                return 0L;
            }
            try
            {
                long num = 0L;
                lock (this)
                {
                    strQuery = strQuery.Trim();
                    if (strQuery[strQuery.Length - 1] != ';')
                    {
                        strQuery = strQuery + ";";
                    }
                    strQuery = string.Format("{0}", strQuery);
                    string[][] strArray = this.SelectQuery(strQuery);
                    if (strArray.Length > 0)
                    {
                        num = long.Parse(strArray[0][0]);
                    }
                }
                return num;
            }
            catch (MySqlException exception)
            {
                if (exception.ErrorCode == 0)
                {
                    if (this.IsConnected)
                    {
                        return this.InsertQuery(strQuery);
                    }
                }
                else
                {
                    
                }
                return 0L;
            }
        }

        public virtual long InsertQuery(string strParamQuery, string[] strParamNames, object[] paramValues)
        {
            if (!this.IsConnected)
            {
                return 0L;
            }
            if (strParamNames.Length != paramValues.Length)
            {
                //Error Process
                return 0L;
            }
            string str = "";
            try
            {
                long num = 0L;
                lock (this)
                {
                    strParamQuery = strParamQuery.Trim();
                    if (strParamQuery[strParamQuery.Length - 1] != ';')
                    {
                        str = str + ";";
                    }
                    strParamQuery = string.Format(strParamQuery, strParamNames);

                    MySqlCommand command = new MySqlCommand(strParamQuery, m_dbConnection);
                    for (int i = 0; i < paramValues.Length; i++)
                        command.Parameters.AddWithValue(strParamNames[i], paramValues[i]);
                    command.ExecuteNonQuery();
                    num = command.LastInsertedId;
                }
                return num;
            }
            catch (MySqlException exception)
            {
                if (exception.ErrorCode == 0)
                {
                    if (this.IsConnected)
                    {
                        return this.InsertQuery(str, strParamNames, paramValues);
                    }
                }
                else
                {
                    
                }
                return 0L;
            }
        }

        public virtual string[][] SelectQuery(string strQuery)
        {
            if (!this.IsConnected)
            {
                return new string[0][];
            }
            try
            {
                string[][] strArray;
                lock (this)
                {
                    MySqlDataAdapter adapter = new MySqlDataAdapter();
                    DataSet dataSet = new DataSet();
                    adapter.SelectCommand = new MySqlCommand(strQuery, this.m_dbConnection);
                    adapter.Fill(dataSet);
                    int count = dataSet.Tables[0].Rows.Count;
                    int num2 = dataSet.Tables[0].Columns.Count;
                    DataRowCollection rows = dataSet.Tables[0].Rows;
                    strArray = new string[count][];
                    for (int i = 0; i < count; i++)
                    {
                        DataRow row = rows[i];
                        strArray[i] = new string[num2];
                        for (int j = 0; j < num2; j++)
                        {
                            strArray[i][j] = row[j].ToString();
                        }
                    }
                }
                return strArray;
            }
            catch (Exception e)
            {
                return new string[0][];
            }
        }

        public virtual string[][] SelectQuery(string strParamQuery, string[] strParamNames, object[] paramValues)
        {
            if (!this.IsConnected)
            {
                return new string[0][];
            }
            if (strParamNames.Length != paramValues.Length)
            {
               
                return new string[0][];
            }
            string cmdText = "";
            try
            {
                string[][] strArray;
                lock (this)
                {
                    cmdText = string.Format(strParamQuery, (object[])strParamNames);
                    MySqlDataAdapter adapter = new MySqlDataAdapter();
                    MySqlCommand command = new MySqlCommand(cmdText, this.m_dbConnection);
                    DataSet dataSet = new DataSet();
                    for (int i = 0; i < strParamNames.Length; i++)
                    {
                        command.Parameters.AddWithValue(strParamNames[i], paramValues[i]);
                    }
                    adapter.SelectCommand = command;
                    adapter.Fill(dataSet);
                    int count = dataSet.Tables[0].Rows.Count;
                    int num3 = dataSet.Tables[0].Columns.Count;
                    DataRowCollection rows = dataSet.Tables[0].Rows;
                    strArray = new string[count][];
                    for (int j = 0; j < count; j++)
                    {
                        DataRow row = rows[j];
                        strArray[j] = new string[num3];
                        for (int k = 0; k < num3; k++)
                        {
                            strArray[j][k] = row[k].ToString();
                        }
                    }
                }
                return strArray;
            }
            catch (MySqlException exception)
            {
                if (exception.ErrorCode == 0)
                {
                    if (this.IsConnected)
                    {
                        return this.SelectQuery(cmdText, strParamNames, paramValues);
                    }
                }
                else
                {
                    string str2 = "Error in Parameters";
                    this.ErrMsg = str2;
                }
                return new string[0][];
            }
        }

        public virtual Hashtable[] SelectQueryNamed(string strQuery)
        {
            if (!this.IsConnected)
            {
                return new Hashtable[0];
            }
            try
            {
                Hashtable[] hashtableArray;
                lock (this)
                {
                    MySqlDataAdapter adapter = new MySqlDataAdapter();
                    DataSet dataSet = new DataSet();
                    adapter.SelectCommand = new MySqlCommand(strQuery, this.m_dbConnection);
                    adapter.Fill(dataSet);
                    int count = dataSet.Tables[0].Rows.Count;
                    int num2 = dataSet.Tables[0].Columns.Count;
                    DataRowCollection rows = dataSet.Tables[0].Rows;
                    DataColumnCollection columns = dataSet.Tables[0].Columns;
                    hashtableArray = new Hashtable[count];
                    for (int i = 0; i < count; i++)
                    {
                        DataRow row = rows[i];
                        hashtableArray[i] = new Hashtable();
                        for (int j = 0; j < num2; j++)
                        {
                            hashtableArray[i][j] = row[j];
                            string columnName = columns[j].ColumnName;
                            hashtableArray[i][columnName] = row[j];
                        }
                    }
                }
                return hashtableArray;
            }
            catch (MySqlException exception)
            {
                if (exception.ErrorCode == 0)
                {
                    if (this.IsConnected)
                    {
                        return this.SelectQueryNamed(strQuery);
                    }
                }
                else
                {
                   
                }
                return new Hashtable[0];
            }
        }

        public virtual Hashtable[] SelectQueryNamed(string strParamQuery, string[] strParamNames, object[] paramValues)
        {
            if (!this.IsConnected)
            {
                return new Hashtable[0];
            }
            if (strParamNames.Length != paramValues.Length)
            {
                
                return new Hashtable[0];
            }
            string cmdText = "";
            try
            {
                Hashtable[] hashtableArray;
                lock (this)
                {
                    cmdText = string.Format(strParamQuery, (object[])strParamNames);
                    MySqlDataAdapter adapter = new MySqlDataAdapter();
                    MySqlCommand command = new MySqlCommand(cmdText, this.m_dbConnection);
                    DataSet dataSet = new DataSet();
                    for (int i = 0; i < strParamNames.Length; i++)
                    {
                        command.Parameters.AddWithValue(strParamNames[i], paramValues[i]);
                    }
                    adapter.SelectCommand = command;
                    adapter.Fill(dataSet);
                    int count = dataSet.Tables[0].Rows.Count;
                    int num3 = dataSet.Tables[0].Columns.Count;
                    DataRowCollection rows = dataSet.Tables[0].Rows;
                    DataColumnCollection columns = dataSet.Tables[0].Columns;
                    hashtableArray = new Hashtable[count];
                    for (int j = 0; j < count; j++)
                    {
                        DataRow row = rows[j];
                        hashtableArray[j] = new Hashtable();
                        for (int k = 0; k < num3; k++)
                        {
                            hashtableArray[j][k] = row[k];
                            string columnName = columns[k].ColumnName;
                            hashtableArray[j][columnName] = row[k];
                        }
                    }
                }
                return hashtableArray;
            }
            catch (MySqlException exception)
            {
                if (exception.ErrorCode == 0)
                {
                    if (this.IsConnected)
                    {
                        return this.SelectQueryNamed(cmdText, strParamNames, paramValues);
                    }
                }
                else
                {
                    string str3 = "Error in Params";
                    this.ErrMsg = str3;
                }
                return new Hashtable[0];
            }
        }

        public virtual bool UpdateQuery(string strQuery)
        {
            if (!this.IsConnected)
            {
                return false;
            }
            try
            {
                lock (this)
                {
                    MySqlDataAdapter adapter = new MySqlDataAdapter
                    {
                        UpdateCommand = new MySqlCommand(strQuery, this.m_dbConnection)
                    };
                    adapter.UpdateCommand.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception exception)
            {
                //if (exception.ErrorCode == 0)
                //{
                //    if (this.IsConnected)
                //    {
                //        return this.UpdateQuery(strQuery);
                //    }
                //}
                //else
                {
                   
                }
                return false;
            }
        }

        public virtual bool UpdateQuery(string strParamQuery, string[] strParamNames, object[] paramValues)
        {
            if (!this.IsConnected)
            {
                return false;
            }
            if (strParamNames.Length != paramValues.Length)
            {
                
                return false;
            }
            string cmdText = "";
            try
            {
                lock (this)
                {
                    cmdText = string.Format(strParamQuery, (object[])strParamNames);
                    MySqlDataAdapter adapter = new MySqlDataAdapter();
                    MySqlCommand command = new MySqlCommand(cmdText, this.m_dbConnection);
                    for (int i = 0; i < strParamNames.Length; i++)
                    {
                        command.Parameters.AddWithValue(strParamNames[i], paramValues[i]);
                    }
                    adapter.UpdateCommand = command;
                    adapter.UpdateCommand.ExecuteNonQuery();
                }
                return true;
            }
            catch (MySqlException exception)
            {
                if (exception.ErrorCode == 0)
                {
                    if (this.IsConnected)
                    {
                        return this.UpdateQuery(cmdText, strParamNames, paramValues);
                    }
                }
                else
                {
                    
                }
                return false;
            }
        }

        // Properties
        public string DBID
        {
            get
            {
                return this.m_strDBID;
            }
            set
            {
                this.m_strDBID = value;
            }
        }

        public string DBName
        {
            get
            {
                return this.m_strDBName;
            }
            set
            {
                this.m_strDBName = value;
            }
        }

        public string DBPort
        {
            get
            {
                return this.m_strDBPort;
            }
            set
            {
                this.m_strDBPort = value;
            }
        }

        public string DBPwd
        {
            get
            {
                return this.m_strDBPwd;
            }
            set
            {
                this.m_strDBPwd = value;
            }
        }

        public string DBServer
        {
            get
            {
                return this.m_strDBServer;
            }
            set
            {
                this.m_strDBServer = value;
            }
        }

        public string ErrMsg
        {
            get
            {
                string strErrMsg = this.m_strErrMsg;
                this.m_strErrMsg = "";
                return strErrMsg;
            }
            set
            {
                this.m_strErrMsg = value;
            }
        }

        public bool IsConnected
        {
            get
            {
                if (!this.m_bIsConnected)
                {
                    
                }
                else
                {
                    this.ErrMsg = "";
                }
                return this.m_bIsConnected;
            }
            set
            {
                this.m_bIsConnected = value;
            }
        }

        public bool IsErr
        {
            get
            {
                return (this.m_strErrMsg.Length > 0);
            }
        }
    }


}
