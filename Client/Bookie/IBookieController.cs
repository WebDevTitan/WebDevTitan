using System.Collections.Generic;
using System.Net.Http;
using Protocol;

namespace Project.Bookie
{
    public interface IBookieController
    {
        string getProxyLocation();
        HttpClient initHttpClient(bool bUseNewCookie);
        bool login();
        bool logout();

#if (BET365_ADDON || LOTTOMATICA)
        PROCESS_RESULT PlaceBet(List<BetburgerInfo> infos, out List<PROCESS_RESULT> result);
#endif
        PROCESS_RESULT PlaceBet(ref BetburgerInfo info);

        double getBalance();
        bool Pulse();
        void Close();
        void Feature();
        int GetPendingbets();
    }
}
