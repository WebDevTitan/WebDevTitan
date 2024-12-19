using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bet365LiveAgent.Data.Soccer;
using Bet365LiveAgent.Logics;
using File = System.IO.File;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp.Server;

namespace Bet365LiveAgent
{
    public partial class FrmMain : Form
    {
        public FrmMain(LiveAgentNotify notifier)
        {
            InitializeComponent();

            Global.WriteLog = OnWriteLog;
            Global.matchEvent = displayBetburger;
            Global.resultEvent = displayResult;

            Global.LiveResultNotifier = notifier;
        }        


        private void FrmMain_Load(object sender, EventArgs e)
        {
            Config.Instance.LoadConfig();
            RefreshConfigControls();
            
            //System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            //System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            //string version = fvi.FileVersion;

            this.Text = "Bet365 Live Notifier";


            //Bet365AgentManager.SendWebAPI("Joe-SoccerLiver", Bet365AgentManager.ToHexString("soccer|Champions League|Liverpool|Real Madrid"), Bet365AgentManager.ToHexString($"AH1(1.5)"), Bet365AgentManager.ToHexString("3342342"), Bet365AgentManager.ToHexString("test1|test2|test3"));

#if (!TEST)
            try
            {
                Global.socketServer = new WebSocketServer(12323, false);
                Global.socketServer.AddWebSocketService<ClientWebSocket>("/");
                Global.socketServer.Start();
            }
            catch
            {

            }
#endif
        }

        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Bet365ClientManager.Instance.Stop();
            Bet365AgentManager.Instance.Stop();
            RefreshConfig();
            Config.Instance.SaveConfig();
                        
        }        

        private async void btnStart_Click(object sender, EventArgs e)
        {
            Start();            
        }

        public void TestFunc()
        {
            string[] files = Directory.GetFiles("test", "*.log", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                string[] alllines = File.ReadAllLines(file);
                foreach (string line in alllines)
                {
                    int nIndex = line.IndexOf("[Out-Data] ");
                    if (nIndex > 0)
                    {
                        string data = line.Substring(nIndex + 11);
                        try
                        {
                            JArray value = JsonConvert.DeserializeObject<JArray>(data);
                            Bet365AgentManager.Instance.OnBet365ProcessData(value);
                        }
                        catch 
                        {
                            Trace.WriteLine($"Exception of parsing {line}");
                        }
                    }
                }
            }

            Bet365AgentManager.Instance.CheckResult();

        }

        public void Start()
        {
            Global.LogFileName = $"log_{DateTime.Now.ToString("yyyyMMddHHmmss")}.log";
            RefreshConfig();
            RefreshControls(true);
            setTimer.Start();
            Utils.bRun = true;

            if (string.IsNullOrWhiteSpace(Config.Instance.Bet365Domain))
            {
                MessageBoxEx.Show(this, "Please input bet365 domain!", "Warning", MessageBoxButtons.OK);
                return;
            }

            RefreshConfig();
#if (TEST)
            Thread thr = new Thread(TestFunc);
            thr.Start();
#else

            Bet365ClientManager.Instance.Start();
#endif
            Bet365AgentManager.Instance.Start();
        }
        public void Stop()
        {
            RefreshControls(false);
            RefreshConfig();
            Bet365ClientManager.Instance.Stop();
            Bet365AgentManager.Instance.Stop();
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.UseWaitCursor = true;
            if (btnStop.Visible)
            {
                btnStop.PerformClick();
			}
            Application.Exit();
            this.UseWaitCursor = false;
        }

        private void RefreshControls(bool state)
        {
            return;
            btnStart.Enabled = !state;
            btnStart.Visible = !state;
            btnStop.Enabled = state;
            btnStop.Visible = state;
        }

        private void RefreshConfigControls()
        { 
            // now update config controls
         

        }

        private void RefreshConfig()
        {
           

        }

        private void OnWriteLog(LOGLEVEL logLevel, LOGTYPE logType, string strLog)
        {
            this.BeginInvoke(new Action(() =>
            {
                string strLogPrefix = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]";
                if (logLevel == LOGLEVEL.FILE || logLevel == LOGLEVEL.FULL)
                {
                    if (logType == LOGTYPE.INDATA)
                        strLogPrefix = $"{strLogPrefix} [In-Data]";
                    else if(logType == LOGTYPE.OUTDATA)
                        strLogPrefix = $"{strLogPrefix} [Out-Data]";
                    WriteLogToFile($"{strLogPrefix} {strLog}");
                }
                if (logLevel == LOGLEVEL.NOTICE || logLevel == LOGLEVEL.FULL)
                {
                    if (logType == LOGTYPE.INDATA)
                    {
                        if (txtInDataLog.TextLength > 1024 * 1024)
                            txtInDataLog.Text = string.Empty;
                        int selectionStart = txtInDataLog.SelectionStart;
                        int selectionLength = txtInDataLog.SelectionLength;
                        txtInDataLog.AppendText($"{strLogPrefix} {strLog}{Environment.NewLine}");
                        if (!chkBoxInDataAutoScrll.Checked)
                        {
                            txtInDataLog.SelectionStart = selectionStart;
                            txtInDataLog.SelectionLength = selectionLength;
                            txtInDataLog.ScrollToCaret();
                        }
                    }
                    else if (logType == LOGTYPE.OUTDATA)
                    {
                        if (txtOutDataLog.TextLength > 1024 * 1024)
                            txtOutDataLog.Text = string.Empty;
                        int selectionStart = txtOutDataLog.SelectionStart;
                        int selectionLength = txtOutDataLog.SelectionLength;
                        txtOutDataLog.AppendText($"{strLogPrefix} {strLog}{Environment.NewLine}");
                        if (!chkBoxOutDataAutoScrll.Checked)
                        {
                            txtOutDataLog.SelectionStart = selectionStart;
                            txtOutDataLog.SelectionLength = selectionLength;
                            txtOutDataLog.ScrollToCaret();
                        }
                    }
                }
            }));
        }

        private void WriteLogToFile(string strLog)
        {
            try
            {
                Directory.CreateDirectory(Global.LogFilePath);
                FileStream fileStream = File.Open($"{Global.LogFilePath}{Global.LogFileName}", FileMode.Append, FileAccess.Write, FileShare.Read);
                if (fileStream.Length > 20 * 1024 * 1024)
                {                    
                    Global.LogFileName = $"log_{DateTime.Now.ToString("yyyyMMddHHmmss")}.log";
                    fileStream.Close();
                    fileStream = File.Open($"{Global.LogFilePath}{Global.LogFileName}", FileMode.Append, FileAccess.Write, FileShare.Read);
                }
                StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
                if (!string.IsNullOrEmpty(strLog))
                    streamWriter.WriteLine(strLog);
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in WriteLogToFile() : {ex}");
            }
        }

        private void displayResult(List<PickResultData> results)
        {
            this.Invoke(new Action(() => {
                int saveRow = 0;
                if (dataGridHistory.Rows.Count > 0 && dataGridHistory.FirstDisplayedCell != null)
                    saveRow = dataGridHistory.FirstDisplayedCell.RowIndex;

                bindingResultSource.DataSource = results;
                bindingResultSource.ResetBindings(false);

                if (saveRow != 0 && saveRow < dataGridHistory.Rows.Count)
                    dataGridHistory.FirstDisplayedScrollingRowIndex = saveRow;
            }));
        }

        private void displayBetburger(List<SoccerMatchData> matches)
        {
            this.Invoke(new Action(() => {

                int saveRow = 0;
                if (tblMatch.Rows.Count > 0 && tblMatch.FirstDisplayedCell != null)
                    saveRow = tblMatch.FirstDisplayedCell.RowIndex;

                bindSource.DataSource = matches;
                bindSource.ResetBindings(false);

                if (saveRow != 0 && saveRow < tblMatch.Rows.Count)
                    tblMatch.FirstDisplayedScrollingRowIndex = saveRow;
                
            }));
        }
    

        private void btnSet_Click(object sender, EventArgs e)
        {
            FrmSet frmSet = new FrmSet(); // In the load method of this window it refreshes all controls with config values
            frmSet.ShowDialog();
        }

       
        /*
         * TODO: this timer is only used to check if we are inside the starthour - endhour range... we'll do it better
         */
        private void setTimer_Tick(object sender, EventArgs e)
        {

            int cur_hour = DateTime.Now.Hour;
           
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            

            FrmTest test = new FrmTest();
            if (tblMatch.CurrentRow != null)
            {
                SoccerMatchData currentObject = (SoccerMatchData)tblMatch.CurrentRow.DataBoundItem;
                test.SetParameters(currentObject);
            }
            test.Show();
        }

        /*
         * Methods for controlling the webview2 which will be used to load and refresh Bet365 api pstk used in websockets communications
         */


    }
}
