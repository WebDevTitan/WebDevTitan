using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitBot.Json
{
    [Serializable]
    public class Account
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public Account(string _username, string _password)
        {
            Username = _username;
            Password = _password;
        }
    }
}