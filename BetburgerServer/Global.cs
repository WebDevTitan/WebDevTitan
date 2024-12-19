using SeastoryServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BetburgerServer
{
    public delegate string RunScript(string code);
    public delegate void NoParamFunc();
    public delegate void boolParamFunc(bool Param);
    public delegate Task GetCookieFunc(string domain);
    public delegate Task<string> GetPageSourceFunc();
    public class Global
    {
        public static RunScript OpenUrl = null;
        public static RunScript RunScriptCode = null;
        public static RunScript GetStatusValue = null;
        public static NoParamFunc LoadHomeUrl = null;
        public static boolParamFunc SetMonitorVisible = null;
        public static IntPtr ViewerHwnd;
        public static GetCookieFunc GetCookie = null;
        public static NoParamFunc RemoveCookies = null;
        public static NoParamFunc RefreshPage = null;
        public static NoParamFunc RefreshBecauseBet365Notloading = null;
        public static GetPageSourceFunc GetPageSource = null;

        public static CookieContainer cookieContainer = new CookieContainer(300, 50, 20480);

        public static ManualResetEventSlim wait_BetspanLoginEvent = new ManualResetEventSlim();

        public static ManualResetEventSlim wait_TipstrrAuthorizationEvent = new ManualResetEventSlim();
        public static string wait_TipstrrAuthorizationToken = string.Empty;
#if (!FORSALE)
#endif

        public static WTelegram.Client _client;

        public static onWriteStatusEvent onwriteStatus;

        public static string login_url;
    }
}
