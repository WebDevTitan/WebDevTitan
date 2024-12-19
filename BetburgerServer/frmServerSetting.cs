using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;

namespace SeastoryServer
{
    public partial class frmServerSetting : Form
    {
        public frmServerSetting()
        {
            InitializeComponent();
        }

        private void frmServerSetting_Load(object sender, EventArgs e)
        {
            this.Text = "Server Setting";

            numspinIPA.Minimum = 0;
            numspinIPA.Maximum = 255;
            numspinIPB.Minimum = 0;
            numspinIPB.Maximum = 255;
            numspinIPC.Minimum = 0;
            numspinIPC.Maximum = 255;
            numspinIPD.Minimum = 0;
            numspinIPD.Maximum = 255;
            cServerSettings.GetInstance().LoadSettings();
            byte[] ipbytes = cServerSettings.GetInstance().DBIP.GetAddressBytes();
            if (ipbytes != null)
            {
                numspinIPA.Value = ipbytes[0];
                numspinIPB.Value = ipbytes[1];
                numspinIPC.Value = ipbytes[2];
                numspinIPD.Value = ipbytes[3];
            }
            numspinListenPort.Minimum = cServerSettings.GAMEPORT_MIN;
            numspinListenPort.Maximum = cServerSettings.GAMEPORT_MAX;
            numspinListenPort.Value = cServerSettings.GetInstance().ListenPort;

            numspinWebAPIPort.Minimum = cServerSettings.GAMEPORT_MIN;
            numspinWebAPIPort.Maximum = cServerSettings.GAMEPORT_MAX;
            numspinWebAPIPort.Value = cServerSettings.GetInstance().WebAPIPort;

            textDBName.Text = cServerSettings.GetInstance().DBName;
            textDBID.Text = cServerSettings.GetInstance().DBID;
            textDBPwd.Text = cServerSettings.GetInstance().DBPwd;
            textDBPort.Text = cServerSettings.GetInstance().DBPort.ToString();
            textNSTokenServer.Text = cServerSettings.GetInstance().NSTServer;
            chkHeartBeat.Checked = cServerSettings.GetInstance().HeartBeat;

            txtPackage1Price.Text = cServerSettings.GetInstance().Package1_Price;
            txtPackage2Price.Text = cServerSettings.GetInstance().Package2_Price;
            txtPackage3Price.Text = cServerSettings.GetInstance().Package3_Price;
            txtPackage4Price.Text = cServerSettings.GetInstance().Package4_Price;
            txtPackage5Price.Text = cServerSettings.GetInstance().Package5_Price;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                string strIP;

                string strTemp;
                strTemp = textDBName.Text;
                if(strTemp.Trim()=="")
                {
                    MessageBox.Show("디비명을 입력하세요.", "입력오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textDBName.Focus();
                    textDBName_Enter(textDBName, new EventArgs());
                    this.DialogResult = DialogResult.None;
                    return;
                }
                strTemp = textDBID.Text;
                if (strTemp.Trim() == "")
                {
                    MessageBox.Show("디비접속아이디를 입력하세요.", "입력오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textDBID.Focus();
                    textDBID_Enter(textDBID, new EventArgs());
                    this.DialogResult = DialogResult.None;
                    return;
                }
                strTemp = textDBPwd.Text;
                //if (strTemp.Length == 0)
                //{
                //    MessageBox.Show("디비접속암호를 입력하세요.", "입력오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    textDBPwd.Focus();
                //    textDBPwd_Enter(textDBPwd, new EventArgs());
                //    this.DialogResult = DialogResult.None;
                //    return;
                //}
                UInt16 nPort = Convert.ToUInt16(textDBPort.Text);

                if(nPort<1433 || nPort>=cServerSettings.GAMEPORT_MAX)
                {
                    MessageBox.Show("디비접속포트를 입력하세요.", "입력오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textDBPort.Focus();
                    textDBPort_Enter(textDBPort, new EventArgs());
                    this.DialogResult = DialogResult.None;
                    return;
                }
                strIP = string.Format("{0}.{1}.{2}.{3}",
                    numspinIPA.Value, numspinIPB.Value, numspinIPC.Value, numspinIPD.Value);
                cServerSettings.GetInstance().DBIP = IPAddress.Parse(strIP);
                cServerSettings.GetInstance().ListenPort = (ushort)numspinListenPort.Value;
                cServerSettings.GetInstance().WebAPIPort = (ushort)numspinWebAPIPort.Value;
                cServerSettings.GetInstance().DBName = textDBName.Text;
                cServerSettings.GetInstance().DBID = textDBID.Text;
                cServerSettings.GetInstance().DBPwd = textDBPwd.Text;
                cServerSettings.GetInstance().DBPort = Convert.ToUInt16(textDBPort.Text);
                cServerSettings.GetInstance().HeartBeat = chkHeartBeat.Checked;
                cServerSettings.GetInstance().NSTServer = textNSTokenServer.Text;
                cServerSettings.GetInstance().Package1_Price = txtPackage1Price.Text;
                cServerSettings.GetInstance().Package2_Price = txtPackage2Price.Text;
                cServerSettings.GetInstance().Package3_Price = txtPackage3Price.Text;
                cServerSettings.GetInstance().Package4_Price = txtPackage4Price.Text;
                cServerSettings.GetInstance().Package5_Price = txtPackage5Price.Text;

                if (!cServerSettings.GetInstance().SaveSetting())
                {
                    MessageBox.Show("설정정보를 보관할수 없습니다.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                }
            }
            catch(Exception)
            {
                MessageBox.Show("설정정보값이 잘못되였습니다.","알림",
                    MessageBoxButtons.OK,MessageBoxIcon.Warning);
                // DialogResult를 None으로 설정하여 대화창이 종료되지 않도록 한다.
                this.DialogResult = DialogResult.None;
            }
        }

        private void textDBName_Enter(object sender, EventArgs e)
        {
            textDBName.Select(0, textDBPwd.TextLength);
        }

        private void textDBID_Enter(object sender, EventArgs e)
        {
            textDBID.Select(0, textDBPwd.TextLength);
        }

        private void textDBPwd_Enter(object sender, EventArgs e)
        {
            textDBPwd.Select(0, textDBPwd.TextLength);
        }

        private void numspinipA_Enter(object sender, EventArgs e)
        {
            numspinIPA.Select(0, numspinIPA.Value.ToString().Length);
        }

        private void numspinipB_Enter(object sender, EventArgs e)
        {
            numspinIPB.Select(0, numspinIPB.Value.ToString().Length);
        }

        private void numspinipC_Enter(object sender, EventArgs e)
        {
            numspinIPC.Select(0, numspinIPC.Value.ToString().Length);
        }

        private void numspinipD_Enter(object sender, EventArgs e)
        {
            numspinIPD.Select(0, numspinIPD.Value.ToString().Length);
        }

        private void textDBPort_Enter(object sender, EventArgs e)
        {
            textDBPort.Select(0, textDBPort.ToString().Length);
        }

        private void numspinListenPort_Enter(object sender, EventArgs e)
        {
            numspinListenPort.Select(0, numspinListenPort.ToString().Length);
        }

        private void numspinWebAPIPort_Enter(object sender, EventArgs e)
        {
            numspinWebAPIPort.Select(0, numspinWebAPIPort.ToString().Length);
        }

        private void chkHeartBeat_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
