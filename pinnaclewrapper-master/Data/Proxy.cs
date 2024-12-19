using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinnacleWrapper.Data
{
    public class Proxy
    {
        public string proxyIP { get; set; }
        public string port { get; set; }
        public string username { get; set; }
        public string password { get; set; }

        public Proxy(string _proxyIP, string _port, string _username, string _password)
        {
            proxyIP = _proxyIP;
            port = _port;
            username = _username;
            password = _password;
        }
    }

}
