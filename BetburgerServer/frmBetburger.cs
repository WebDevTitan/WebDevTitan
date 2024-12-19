using BetburgerServer.Constant;
using SeastoryServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BetburgerServer
{
    public partial class frmBetburger : Form
    {
        public frmBetburger()
        {
            InitializeComponent();

#if (ODDSJAM)
            groupBox3.Text = "Oddsjam";
            groupBox1.Visible = false;
            groupBox2.Visible = false;
            groupBox4.Visible = false;
            groupBox5.Visible = false;
            groupBox6.Visible = false;
            groupBox7.Visible = false;
            groupBox8.Visible = false;
#else
#if (FORSALE)
            groupBox2.Visible = false;
            groupBox3.Visible = false;
            groupBox4.Visible = false;
            groupBox5.Visible = false;
            groupBox6.Visible = false;
            groupBox7.Visible = false;
            groupBox8.Visible = false;
#endif
#endif
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            uint nVersion = 0;
            try
            {
                nVersion = Convert.ToUInt32(txtVersion.Text);
            }
            catch { }
            if (nVersion < 1)
            {
                MessageBox.Show("Please enter the Server Version!");
                txtVersion.Focus();
                return;
            }

            //if (string.IsNullOrEmpty(txtToken.Text))
            //{
            //    MessageBox.Show("Please enter the betburger API token!");
            //    txtToken.Focus();
            //    return;
            //}

            //if (string.IsNullOrEmpty(txtFilterSurebetPr.Text) && string.IsNullOrEmpty(txtFilterSurebetLv.Text)
            //    && string.IsNullOrEmpty(txtFilterValuebetPr.Text) && string.IsNullOrEmpty(txtFilterValuebetLv.Text))
            //{
            //    MessageBox.Show("Please enter the betburger filter ids!");
            //    txtFilterSurebetPr.Focus();
            //    return;
            //}

            cServerSettings.GetInstance().Version = nVersion; 
            cServerSettings.GetInstance().BBIsAPIToken = chkBetburgerAPI.Checked;
            cServerSettings.GetInstance().BBToken = txtToken.Text;
            cServerSettings.GetInstance().BBFilterSurebetPr = txtFilterSurebetPr.Text;
            cServerSettings.GetInstance().BBFilterSurebetLv = txtFilterSurebetLv.Text;
            cServerSettings.GetInstance().BBFilterValuebetPr = txtFilterValuebetPr.Text;
            cServerSettings.GetInstance().BBFilterValuebetLv = txtFilterValuebetLv.Text;
            cServerSettings.GetInstance().Label = txtLabel.Text;

            cServerSettings.GetInstance().EnableSurebet_Pre = chkSureBet_Pre.Checked;
            cServerSettings.GetInstance().EnableSurebet_Live = chkSureBet_Live.Checked;
            cServerSettings.GetInstance().EnableValuebet_Pre = chkValueBet_Pre.Checked;
            cServerSettings.GetInstance().EnableValuebet_Live = chkValueBet_Live.Checked;
            cServerSettings.GetInstance().EnableSeubet_Live = Seubet_Live_check.Checked;  //seubet live
            cServerSettings.GetInstance().EnableSeubet_Prematch = Seubet_Pre_check.Checked; // seubet pre
            cServerSettings.GetInstance().Percent_Price = percentControl.Text;// seubet percent

            cServerSettings.GetInstance().JuanLiveSoccerUrl = txtJuanLiveSoccerURL.Text;
            cServerSettings.GetInstance().ParkHorseUrl = txtParkHorseURL.Text;

            cServerSettings.GetInstance().BetsapiToken = txtBetsapiToken.Text;

            cServerSettings.GetInstance().BetspanUsername = txtBetspanUsername.Text;
            cServerSettings.GetInstance().BetspanPassword = txtBetspanPassword.Text;

            cServerSettings.GetInstance().SurebetUsername = txtSurebetUsername.Text;
            cServerSettings.GetInstance().SurebetPassword = txtSurebetPassword.Text;

            cServerSettings.GetInstance().BetsmarterUsername = txtBetsmarterUser.Text;
            cServerSettings.GetInstance().BetsmarterPassword = txtBetsmarterPass.Text;

            cServerSettings.GetInstance().TradematesportsUsername = txtTradematesportsUsername.Text;
            cServerSettings.GetInstance().TradematesportsPassword = txtTradematesportsPassword.Text;

            cServerSettings.GetInstance().SaveSetting();

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void frmBetburger_Load(object sender, EventArgs e)
        {
            this.Text = "Betburger Setting";

            cServerSettings.GetInstance().LoadSettings();
            chkBetburgerAPI.Checked = cServerSettings.GetInstance().BBIsAPIToken;
            txtToken.Text = cServerSettings.GetInstance().BBToken;
            txtFilterSurebetPr.Text = cServerSettings.GetInstance().BBFilterSurebetPr;
            txtFilterSurebetLv.Text = cServerSettings.GetInstance().BBFilterSurebetLv;
            txtFilterValuebetPr.Text = cServerSettings.GetInstance().BBFilterValuebetPr;
            txtFilterValuebetLv.Text = cServerSettings.GetInstance().BBFilterValuebetLv;
            txtLabel.Text = cServerSettings.GetInstance().Label;
            txtVersion.Text = cServerSettings.GetInstance().Version.ToString();


            chkSureBet_Pre.Checked = cServerSettings.GetInstance().EnableSurebet_Pre;
            chkSureBet_Live.Checked = cServerSettings.GetInstance().EnableSurebet_Live;
            chkValueBet_Pre.Checked = cServerSettings.GetInstance().EnableValuebet_Pre;
            chkValueBet_Live.Checked = cServerSettings.GetInstance().EnableValuebet_Live;
            Seubet_Pre_check.Checked = cServerSettings.GetInstance().EnableSeubet_Prematch;//seubet  pre
            Seubet_Live_check.Checked = cServerSettings.GetInstance().EnableSeubet_Live;//seubet   live
            percentControl.Text = cServerSettings.GetInstance().Percent_Price;    // seubet live

            txtJuanLiveSoccerURL.Text = cServerSettings.GetInstance().JuanLiveSoccerUrl;
            txtParkHorseURL.Text = cServerSettings.GetInstance().ParkHorseUrl;

            txtBetsapiToken.Text = cServerSettings.GetInstance().BetsapiToken;

            txtBetspanUsername.Text = cServerSettings.GetInstance().BetspanUsername;
            txtBetspanPassword.Text = cServerSettings.GetInstance().BetspanPassword;

            txtSurebetUsername.Text = cServerSettings.GetInstance().SurebetUsername;
            txtSurebetPassword.Text = cServerSettings.GetInstance().SurebetPassword;

            txtTradematesportsUsername.Text = cServerSettings.GetInstance().TradematesportsUsername;
            txtTradematesportsPassword.Text = cServerSettings.GetInstance().TradematesportsPassword;

            txtBetsmarterUser.Text = cServerSettings.GetInstance().BetsmarterUsername;
            txtBetsmarterPass.Text = cServerSettings.GetInstance().BetsmarterPassword;

            txtTgVerificationCode.Text = Constants.Phone;
        }

        private async Task DoLogin(string loginInfo)
        {
            if(Global._client == null)
                Global._client = new WTelegram.Client(Utils.ParseToInt(Constants.ApiID) , Constants.ApiHash);

            string what = await Global._client.Login(loginInfo);
            if (what != null)
            {
                MessageBox.Show($"A {what} is required...");               
                return;
            }
            MessageBox.Show($"We are now connected as {Global._client.User}");
        }
        private async void btnTgVerificationCode_Click(object sender, EventArgs e)
        {
            await DoLogin(txtTgVerificationCode.Text);            
        }

        private void txtBetsmarterUser_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtSurebetUsername_TextChanged(object sender, EventArgs e)
        {

        }

        private void chkSureBet_Pre_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void txtJuanLiveSoccerURL_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtTgVerificationCode_TextChanged(object sender, EventArgs e)
        {

        }

        private void chkBetburgerAPI_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkSureBet_Live_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Seubet_Live_check_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
