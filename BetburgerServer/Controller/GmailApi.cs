using BetburgerServer.Constant;
using EAGetMail;
using HtmlAgilityPack;
using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using SeastoryServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BetburgerServer.Controller
{
    public class GmailApi
    {
        private string m_uidlfile = "uidl.txt";
        private string m_curpath = "";
        UIDLManager oUIDLManager = null;
        string mailFolder;
        private bool m_bcancel = false;
        FileWriter writer = null;
        ImapClient client = null;
        List<string> idList = new List<string>();

        #region EAGetMail Event Handler
        public void OnConnected(object sender, ref bool cancel)
        {
            //_onWriteStatus("*** OnConnected ***");
            cancel = m_bcancel;
            Application.DoEvents();
        }

        public void OnQuit(object sender, ref bool cancel)
        {
            //_onWriteStatus("***OnQuit***");
            cancel = m_bcancel;
            Application.DoEvents();
        }

        public void OnReceivingDataStream(object sender, MailInfo info, int received, int total, ref bool cancel)
        {
            //pgBar.Minimum = 0;
            //pgBar.Maximum = total;
            //pgBar.Value = received;
            cancel = m_bcancel;
            Application.DoEvents();
        }

        public void OnIdle(object sender, ref bool cancel)
        {
            cancel = m_bcancel;
            Application.DoEvents();
        }

        public void OnAuthorized(object sender, ref bool cancel)
        {
            //_onWriteStatus("***OnAuthorized***");

            cancel = m_bcancel;
            Application.DoEvents();
        }

        public void OnSecuring(object sender, ref bool cancel)
        {
            //_onWriteStatus("***Gmail OnSecuring***");
            cancel = m_bcancel;
            Application.DoEvents();
        }
        #endregion
        public GmailApi(onWriteStatusEvent onWriteStatus)
        {
        }
        public void connectServer()
        {
            try
            {
                client = new ImapClient();
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                //_onWriteStatus("Connecting to Gmail Server ...");
                client.Connect("imap.gmail.com", 993, true);
                client.Authenticate(cServerSettings.GetInstance().BetsmarterUsername, cServerSettings.GetInstance().BetsmarterPassword);
                //_onWriteStatus("Connected to Gmail ...");
                oUIDLManager = new UIDLManager();
                m_curpath = Directory.GetCurrentDirectory();
                string uidlfile = String.Format("{0}\\{1}", m_curpath, m_uidlfile);
                readFile(uidlfile);
                writer = new FileWriter(uidlfile);

                mailFolder = String.Format("{0}\\inbox", m_curpath);
                if (!Directory.Exists(mailFolder))
                    Directory.CreateDirectory(mailFolder);

            }
            catch (Exception e)
            {
                //onWriteStatus(e.ToString());
                //onWriteStatus("Connection Failed!");
            }
        }

        public async Task scrap_thread()
        {
           Task.Run(Scrap);
        }

        private void Scrap()
        {
            connectServer();
            parseEmail();
        }
        public void parseEmail()
        {
            while (GameConstants.bRun)
            {
                Thread.Sleep(10000);
                try
                {
                    if (!client.IsConnected)
                    {
                        client.Disconnect(true);
                        connectServer();
                    }

                    //onWriteLog("Reading Emails..");
                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadOnly);

                    for (int i = inbox.Count - 1; i > inbox.Count - 3; i--)
                    {
                        //onWriteLog("Reading New email..");
                        try
                        {
                            MimeMessage message = inbox.GetMessage(i);

                            string mailName = message.From.ToArray()[0].Name;
                            if (!mailName.Contains("BetSmarter"))
                                continue;

                            string mailContent = message.HtmlBody;

                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(mailContent);

                            //HtmlNode iframeNode = doc.DocumentNode.Descendants("liveFrame").FirstOrDefault(node => node.Id != null && node.Id == "liveFrame");
                            //if (iframeNode == null)
                            //    continue;

                            HtmlNode aNode = doc.DocumentNode.Descendants("a").FirstOrDefault(node => node.Attributes["href"] != null && node.Attributes["href"].Value.Contains("https://betsmarter.app/activateaccount?token="));
                            if (aNode == null)
                                continue;

                            Global.login_url = aNode.Attributes["href"].Value;
                            string messageId = Utils.Between(Global.login_url + "xx", "token=", "xx");

                            if (idList.Contains(messageId))
                                continue;

                            //_onWriteStatus("***********Found New Mail**************");

                            idList.Add(messageId);
                            writer.WriteRow(messageId);
                        }
                        catch
                        {

                        }

                    }
                }
                catch (Exception e)
                {
                    client.Disconnect(true);
                    connectServer();
                }
            }
            

        }

        private void readFile(string path)
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    idList.Add(line);
                }
            }
            catch (Exception e)
            {

            }

        }

    }
}
