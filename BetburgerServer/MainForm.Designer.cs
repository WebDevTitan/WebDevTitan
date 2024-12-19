namespace SeastoryServer
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.mnuAction = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuActionLogin = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuActionStart = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuActionStop = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuActionClose = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSetting = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSettingServ = new System.Windows.Forms.ToolStripMenuItem();
            this.betburgerCredientalsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblConnectCnt = new System.Windows.Forms.Label();
            this.chkViewMode = new System.Windows.Forms.CheckBox();
            this.listLog = new System.Windows.Forms.ListBox();
            this.tblExit = new System.Windows.Forms.Button();
            this.tblUsers = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.License = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Gameid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Expire = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.timerCursorMover = new System.Windows.Forms.Timer(this.components);
            this.button2 = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tblUsers)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAction,
            this.mnuSetting});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
            // 
            // mnuAction
            // 
            this.mnuAction.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuActionLogin,
            this.mnuActionStart,
            this.mnuActionStop,
            this.mnuActionClose});
            this.mnuAction.Name = "mnuAction";
            resources.ApplyResources(this.mnuAction, "mnuAction");
            // 
            // mnuActionLogin
            // 
            this.mnuActionLogin.Name = "mnuActionLogin";
            resources.ApplyResources(this.mnuActionLogin, "mnuActionLogin");
            this.mnuActionLogin.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            this.mnuActionLogin.Click += new System.EventHandler(this.mnuActionLogin_Click);
            // 
            // mnuActionStart
            // 
            this.mnuActionStart.Name = "mnuActionStart";
            resources.ApplyResources(this.mnuActionStart, "mnuActionStart");
            this.mnuActionStart.Click += new System.EventHandler(this.mnuActionStart_Click);
            // 
            // mnuActionStop
            // 
            this.mnuActionStop.Name = "mnuActionStop";
            resources.ApplyResources(this.mnuActionStop, "mnuActionStop");
            this.mnuActionStop.Click += new System.EventHandler(this.mnuActionStop_Click);
            // 
            // mnuActionClose
            // 
            this.mnuActionClose.Name = "mnuActionClose";
            resources.ApplyResources(this.mnuActionClose, "mnuActionClose");
            this.mnuActionClose.Click += new System.EventHandler(this.mnuActionClose_Click);
            // 
            // mnuSetting
            // 
            this.mnuSetting.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSettingServ,
            this.betburgerCredientalsToolStripMenuItem});
            this.mnuSetting.Name = "mnuSetting";
            resources.ApplyResources(this.mnuSetting, "mnuSetting");
            // 
            // mnuSettingServ
            // 
            this.mnuSettingServ.Name = "mnuSettingServ";
            resources.ApplyResources(this.mnuSettingServ, "mnuSettingServ");
            this.mnuSettingServ.Click += new System.EventHandler(this.mnuSettingServ_Click);
            // 
            // betburgerCredientalsToolStripMenuItem
            // 
            this.betburgerCredientalsToolStripMenuItem.Name = "betburgerCredientalsToolStripMenuItem";
            resources.ApplyResources(this.betburgerCredientalsToolStripMenuItem, "betburgerCredientalsToolStripMenuItem");
            this.betburgerCredientalsToolStripMenuItem.Click += new System.EventHandler(this.betburgerCredientalsToolStripMenuItem_Click);
            // 
            // lblConnectCnt
            // 
            resources.ApplyResources(this.lblConnectCnt, "lblConnectCnt");
            this.lblConnectCnt.Name = "lblConnectCnt";
            // 
            // chkViewMode
            // 
            resources.ApplyResources(this.chkViewMode, "chkViewMode");
            this.chkViewMode.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkViewMode.Name = "chkViewMode";
            this.chkViewMode.UseVisualStyleBackColor = true;
            // 
            // listLog
            // 
            resources.ApplyResources(this.listLog, "listLog");
            this.listLog.Cursor = System.Windows.Forms.Cursors.Hand;
            this.listLog.FormattingEnabled = true;
            this.listLog.Name = "listLog";
            // 
            // tblExit
            // 
            resources.ApplyResources(this.tblExit, "tblExit");
            this.tblExit.Name = "tblExit";
            this.tblExit.UseVisualStyleBackColor = true;
            this.tblExit.Click += new System.EventHandler(this.tblExit_Click);
            // 
            // tblUsers
            // 
            this.tblUsers.AllowUserToAddRows = false;
            this.tblUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tblUsers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.License,
            this.Gameid,
            this.Expire,
            this.Column2});
            resources.ApplyResources(this.tblUsers, "tblUsers");
            this.tblUsers.MultiSelect = false;
            this.tblUsers.Name = "tblUsers";
            this.tblUsers.RowHeadersVisible = false;
            this.tblUsers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column1.FillWeight = 80F;
            resources.ApplyResources(this.Column1, "Column1");
            this.Column1.Name = "Column1";
            // 
            // License
            // 
            this.License.FillWeight = 80F;
            resources.ApplyResources(this.License, "License");
            this.License.Name = "License";
            // 
            // Gameid
            // 
            resources.ApplyResources(this.Gameid, "Gameid");
            this.Gameid.Name = "Gameid";
            // 
            // Expire
            // 
            resources.ApplyResources(this.Expire, "Expire");
            this.Expire.Name = "Expire";
            // 
            // Column2
            // 
            resources.ApplyResources(this.Column2, "Column2");
            this.Column2.Name = "Column2";
            // 
            // timerCursorMover
            // 
            this.timerCursorMover.Enabled = true;
            this.timerCursorMover.Interval = 10000;
            this.timerCursorMover.Tick += new System.EventHandler(this.timerCursorMover_Tick);
            // 
            // button2
            // 
            resources.ApplyResources(this.button2, "button2");
            this.button2.Name = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // frmMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button2);
            this.Controls.Add(this.tblExit);
            this.Controls.Add(this.tblUsers);
            this.Controls.Add(this.listLog);
            this.Controls.Add(this.chkViewMode);
            this.Controls.Add(this.lblConnectCnt);
            this.Controls.Add(this.menuStrip1);
            this.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_Closing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tblUsers)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuAction;
        private System.Windows.Forms.ToolStripMenuItem mnuActionLogin;
        private System.Windows.Forms.ToolStripMenuItem mnuActionStart;
        private System.Windows.Forms.ToolStripMenuItem mnuActionStop;
        private System.Windows.Forms.ToolStripMenuItem mnuActionClose;
        private System.Windows.Forms.ToolStripMenuItem mnuSetting;
        private System.Windows.Forms.Label lblConnectCnt;
        private System.Windows.Forms.CheckBox chkViewMode;
        private System.Windows.Forms.ToolStripMenuItem mnuSettingServ;
        private System.Windows.Forms.ListBox listLog;
        private System.Windows.Forms.ToolStripMenuItem betburgerCredientalsToolStripMenuItem;
        private System.Windows.Forms.Button tblExit;
        private System.Windows.Forms.DataGridView tblUsers;
        private System.Windows.Forms.Timer timerCursorMover;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn License;
        private System.Windows.Forms.DataGridViewTextBoxColumn Gameid;
        private System.Windows.Forms.DataGridViewTextBoxColumn Expire;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.Button button2;
    }
}

