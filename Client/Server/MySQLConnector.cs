using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Project;
using Project.Helphers;
//using OpenQA.Selenium.Remote;

namespace OrbitBot.Controller
{
    public class MySQLConnector
    {
        string connectionString = "server=95.179.244.192;port=3306;database=betflash;user=willow;password=KYMis199$109;";

        MySqlConnection connection = null;
        private static MySQLConnector _instance = null;
        public MySQLConnector()
        {
        }

        public static MySQLConnector instance
        {
            get
            {
                return _instance;
            }
        }

        public bool IsLogged { get; internal set; }

        static public void CreateInstance()
        {
            _instance = new MySQLConnector();
        }

        public void DoConnect()
        {
            connection = new MySqlConnection(connectionString);
            try
            {
                // Open the connection
                connection.Open();
                Console.WriteLine("Connection successful!");

                // Create a SQL query
                if(!string.IsNullOrEmpty(Setting.Instance.license))
                {
                    string query = $"SELECT COUNT(*) FROM tbl_account WHERE bookmaker='bet365' AND username='{Setting.Instance.username}'";
                    // Create a command object
                    MySqlCommand command = new MySqlCommand(query, connection);
                    int rowCount = Convert.ToInt32(command.ExecuteScalar());
                    if (rowCount == 0)
                    {
                        query = $"INSERT INTO tbl_account(username, password, bookmaker, country, license, status) VALUES ('{Setting.Instance.username}', '{Setting.Instance.username}', 'bet365', '{Setting.Instance.domain}', '{Setting.Instance.license}', 1);";
                        command = new MySqlCommand(query, connection);
                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    string query = $"SELECT COUNT(*) FROM tbl_account WHERE status=1 AND bookmaker='bet365' AND username='{Setting.Instance.username}' AND license='{Setting.Instance.license}';";
                    // Create a command object
                    MySqlCommand command = new MySqlCommand(query, connection);
                    int rowCount = Convert.ToInt32(command.ExecuteScalar());
                    if (rowCount == 0)
                    {
                        LogMng.Instance.onWriteStatus("It is new account. Please contact support!");
                        query = $"INSERT INTO tbl_account(username, password, bookmaker, country, license) VALUES ('{Setting.Instance.username}', '{Setting.Instance.username}', 'bet365', '{Setting.Instance.domain}', '{Utils.Base64Encode($"{Setting.Instance.username}:{Setting.Instance.password}")}');";
                        command = new MySqlCommand(query, connection);
                        command.ExecuteNonQuery();
                        return;
                    }
                }
                
                IsLogged = true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        public void UpdateBalance(double balance)
        {
            string query = $"UPDATE tbl_account SET current_balance={balance} WHERE username='{Setting.Instance.username}'";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.ExecuteNonQuery();
        }

    }
}
