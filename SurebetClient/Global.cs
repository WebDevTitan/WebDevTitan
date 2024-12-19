using Microsoft.Web.WebView2.Core;
using Protocol;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Project
{
    public enum BOOKMAKER
    {
        BET365,
        LUCKIA,
    }

    public delegate string RunScript(string code);
    public delegate void NoParamFunc();
    public delegate void boolParamFunc(bool Param);
    public delegate Rect GetMonitorPosFunc();
    public delegate Task GetCookieFunc(string domain);
    public delegate Task<string> GetPageSourceFunc();

    public class Global
    {
        private static Global s_instance = null;
        public static Global Instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = new Global();

                return s_instance;
            }
        }

        public static bool bRun = false;
        public static bool bServerConnect = false;        
                
        public static CookieContainer cookieContainer_Main = new CookieContainer(300, 50, 20480);
        public static CookieContainer cookieContainer_Sub = new CookieContainer(300, 50, 20480);
        public static double balanceMain;
        public static double balanceSub;
        public static int accountStatus;
        
        public static string ProxySessionID = "";

        public static UInt32 Version = 135;
        public static UInt32 PackageID = 0;
        

        public static BetHeaderInfo BetHeader = new BetHeaderInfo();
        public static NameValueCollection placeBetHeaderCollection = new NameValueCollection();

        
        public static RunScript OpenUrl_Main = null, OpenUrl_Sub = null;
        public static RunScript RunScriptCode_Main = null, RunScriptCode_Sub = null;
        public static RunScript GetStatusValue_Main = null, GetStatusValue_Sub = null;
        public static NoParamFunc LoadHomeUrl_Main = null, LoadHomeUrl_Sub = null;
        public static boolParamFunc SetMonitorVisible_Main = null, SetMonitorVisible_Sub = null;
        public static GetMonitorPosFunc GetMonitorPos_Main = null, GetMonitorPos_Sub = null;
        public static IntPtr ViewerHwnd_Main, ViewerHwnd_Sub;
        public static GetCookieFunc GetCookie_Main = null, GetCookie_Sub = null;
        public static GetPageSourceFunc GetPageSource_Main = null, GetPageSource_Sub = null;
        public static NoParamFunc RefreshPage_Main = null;
        public static NoParamFunc RefreshBecauseBet365Notloading_Main = null;

        public static ManualResetEventSlim waitResponseEvent = new ManualResetEventSlim();
        public static string strAddBetResult = "";
        public static string strPlaceBetResult = "";

        public const string GetOpenBetListCommandLine = "var OpenBetList = [];var e, n;if (n = Locator.treeLookup.getReference('OPENBETS')){for (e = 0; e < n.getChildren().length; e++){var OpenBetItr = n.getChildren()[e].data;data = [];var m;for (m = 0; m < n.getChildren()[e].getChildren().length; m++){data.push(n.getChildren()[e].getChildren()[m].data);}OpenBetItr['data'] = data;OpenBetList.push(OpenBetItr);}}return JSON.stringify(OpenBetList);";

        public static void WriteTroubleShotLog(string log)
        {
#if (TROUBLESHOT)
            Trace.WriteLine(log);
#endif
        }
        public Global()
        {

        }
    }
}
