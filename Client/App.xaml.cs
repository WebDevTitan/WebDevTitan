using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;

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


            //string text = File.ReadAllText("a.txt");
            //string value = Utils.Between(text, "var CALL_DETAIL_OBJ = '", "'");
            //dynamic origObject1 = JsonConvert.DeserializeObject<dynamic>(value);

            //bool bFound = false;
            //foreach (JObject origObject2 in origObject1["leo"])
            //{
            //    foreach (dynamic origObject3 in origObject2["mmkW"])
            //    {
            //        foreach (var origObject4 in origObject3.Value["spd"])
            //        {
            //            foreach (var origObject5 in origObject4.Value["asl"])
            //            {   
            //                if (origObject5["oi"].ToString() == "23537284187")
            //                {
            //                    string aamsId = origObject2["mi"].ToString();
            //                    string catId = origObject2["ci"].ToString();
            //                    string disId = origObject2["si"].ToString();
            //                    string evnDate = origObject2["ed"].ToString();
            //                    string evnDateTs = origObject2["edt"].ToString();
            //                    string evtId = origObject2["ei"].ToString();
            //                    string evtName = origObject2["en"].ToString();
            //                    string hdrType = origObject3.Value["ht"].ToString();
            //                    string idSlt = origObject3.Value["sslI"].ToString();
            //                    string markId = origObject5["mi"].ToString();
            //                    string markMultipla = origObject5["oc"].ToString();
            //                    string markName = origObject3.Value["mn"].ToString();
            //                    string markTypId = origObject5["mti"].ToString();
            //                    string oddsId = origObject5["oi"].ToString();
            //                    string oddsValue = origObject5["ov"].ToString();
            //                    string onLineCode = origObject2["oc"].ToString();
            //                    string selId = origObject5["si"].ToString();
            //                    string selName = origObject5["sn"].ToString();
            //                    string tId = origObject2["ti"].ToString();
            //                    string tName = origObject2["td"].ToString();
            //                    string vrt = "False";

            //                    bFound = true;
            //                    break;
            //                }
            //            }
            //            if (bFound)
            //                break;
            //        }
            //        if (bFound)
            //            break;
            //    }
            //    if (bFound)
            //        break;
            //}



            try
            {
                RegistryKey rkey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Terminal Server Client", true);
                if (rkey != null)
                {
                    rkey.SetValue("RemoteDesktop_SuppressWhenMinimized", "2", RegistryValueKind.DWord);
                }


                var bootstrapper = new Bootstrapper();
                bootstrapper.Run();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Exception {ex}");
            }
        }
    }
}
