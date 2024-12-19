using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Project.Bookie
{
    public interface IBookieController
    {
        string getProxyLocation();
        HttpClient initHttpClient(bool bUseNewCookie);
        bool login();
        PROCESS_RESULT PlaceBet(BetburgerInfo info);
        double getBalance();
        bool Pulse();
    }
}
