using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Project
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string content = File.ReadAllText("a.txt");

            dynamic jsonPlacebetInfo = JsonConvert.DeserializeObject<dynamic>(content);
            string prematchResult = jsonPlacebetInfo.prematch.ToString();

            ////string[] linkArray = info.direct_link.Split('|');
            ////if (linkArray.Count() < 3)
            ////    return null;

            //betData.fd = jsonResResp["f"]["FI"].ToString();
            //betData.i2 = jsonResResp["f"]["ID"].ToString();
            //betData.oddStr = jsonResResp["f"]["OD"].ToString();

            //Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "--remote-debugging-port=9222", EnvironmentVariableTarget.Machine);

            RegistryKey rkey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Terminal Server Client", true);
            if (rkey != null)
            {
                rkey.SetValue("RemoteDesktop_SuppressWhenMinimized", "2", RegistryValueKind.DWord);
            }
                                    

            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }
    }
}
