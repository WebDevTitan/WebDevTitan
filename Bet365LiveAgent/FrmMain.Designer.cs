namespace Bet365LiveAgent
{
    partial class FrmMain
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.bindSource = new System.Windows.Forms.BindingSource(this.components);
            this.setTimer = new System.Windows.Forms.Timer(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tbcSections = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tblMatch = new System.Windows.Forms.DataGridView();
            this.MatchTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LeagueName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HomeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AwayTeam = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Score = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Statistics = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AsianHandicapOdds = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GoalLineOdds = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dataGridHistory = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MatchLabel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bindingResultSource = new System.Windows.Forms.BindingSource(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpInLog = new System.Windows.Forms.TabPage();
            this.chkBoxInDataAutoScrll = new System.Windows.Forms.CheckBox();
            this.txtInDataLog = new System.Windows.Forms.TextBox();
            this.tpOutLog = new System.Windows.Forms.TabPage();
            this.chkBoxOutDataAutoScrll = new System.Windows.Forms.CheckBox();
            this.txtOutDataLog = new System.Windows.Forms.TextBox();
            this.tpNotifications = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.rdYellowUnder = new System.Windows.Forms.RadioButton();
            this.rdYellowOver = new System.Windows.Forms.RadioButton();
            this.rdCornerUnder = new System.Windows.Forms.RadioButton();
            this.rdCornerOver = new System.Windows.Forms.RadioButton();
            this.rdGoalUnder = new System.Windows.Forms.RadioButton();
            this.rdGoalOver = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.rdNothing = new System.Windows.Forms.RadioButton();
            this.rdCorner = new System.Windows.Forms.RadioButton();
            this.rdCards = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnSet = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.bindSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tbcSections.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tblMatch)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridHistory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingResultSource)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tpInLog.SuspendLayout();
            this.tpOutLog.SuspendLayout();
            this.tpNotifications.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // setTimer
            // 
            this.setTimer.Interval = 3000;
            this.setTimer.Tick += new System.EventHandler(this.setTimer_Tick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tbcSections);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Size = new System.Drawing.Size(1193, 615);
            this.splitContainer1.SplitterDistance = 451;
            this.splitContainer1.TabIndex = 8;
            // 
            // tbcSections
            // 
            this.tbcSections.Controls.Add(this.tabPage1);
            this.tbcSections.Controls.Add(this.tabPage2);
            this.tbcSections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbcSections.Location = new System.Drawing.Point(0, 0);
            this.tbcSections.Name = "tbcSections";
            this.tbcSections.SelectedIndex = 0;
            this.tbcSections.Size = new System.Drawing.Size(1193, 451);
            this.tbcSections.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.tbcSections.TabIndex = 7;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tblMatch);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1185, 425);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Data Log";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tblMatch
            // 
            this.tblMatch.AllowDrop = true;
            this.tblMatch.AllowUserToAddRows = false;
            this.tblMatch.AllowUserToDeleteRows = false;
            this.tblMatch.AutoGenerateColumns = false;
            this.tblMatch.BackgroundColor = System.Drawing.Color.White;
            this.tblMatch.CausesValidation = false;
            this.tblMatch.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Constantia", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.ButtonHighlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.AppWorkspace;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.tblMatch.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.tblMatch.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tblMatch.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.MatchTime,
            this.LeagueName,
            this.HomeName,
            this.AwayTeam,
            this.Score,
            this.Statistics,
            this.AsianHandicapOdds,
            this.GoalLineOdds});
            this.tblMatch.DataSource = this.bindSource;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(91)))), ((int)(((byte)(108)))), ((int)(((byte)(135)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.DimGray;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.tblMatch.DefaultCellStyle = dataGridViewCellStyle2;
            this.tblMatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblMatch.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.tblMatch.EnableHeadersVisualStyles = false;
            this.tblMatch.Location = new System.Drawing.Point(3, 3);
            this.tblMatch.Name = "tblMatch";
            this.tblMatch.ReadOnly = true;
            this.tblMatch.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.ButtonFace;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.tblMatch.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.tblMatch.RowHeadersVisible = false;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(62)))), ((int)(((byte)(68)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.ControlDarkDark;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.tblMatch.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.tblMatch.RowTemplate.Height = 100;
            this.tblMatch.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.tblMatch.Size = new System.Drawing.Size(1179, 419);
            this.tblMatch.TabIndex = 26;
            // 
            // MatchTime
            // 
            this.MatchTime.DataPropertyName = "Time";
            this.MatchTime.HeaderText = "Match Time";
            this.MatchTime.Name = "MatchTime";
            this.MatchTime.ReadOnly = true;
            this.MatchTime.Width = 88;
            // 
            // LeagueName
            // 
            this.LeagueName.DataPropertyName = "LeagueName";
            this.LeagueName.HeaderText = "League";
            this.LeagueName.Name = "LeagueName";
            this.LeagueName.ReadOnly = true;
            this.LeagueName.Width = 120;
            // 
            // HomeName
            // 
            this.HomeName.DataPropertyName = "HomeName";
            this.HomeName.HeaderText = "Home Team";
            this.HomeName.Name = "HomeName";
            this.HomeName.ReadOnly = true;
            this.HomeName.Width = 120;
            // 
            // AwayTeam
            // 
            this.AwayTeam.DataPropertyName = "AwayName";
            this.AwayTeam.HeaderText = "Away Team";
            this.AwayTeam.Name = "AwayTeam";
            this.AwayTeam.ReadOnly = true;
            this.AwayTeam.Width = 120;
            // 
            // Score
            // 
            this.Score.DataPropertyName = "Score";
            this.Score.HeaderText = "Score";
            this.Score.Name = "Score";
            this.Score.ReadOnly = true;
            this.Score.Width = 60;
            // 
            // Statistics
            // 
            this.Statistics.DataPropertyName = "Statistics";
            this.Statistics.HeaderText = "Statistics";
            this.Statistics.Name = "Statistics";
            this.Statistics.ReadOnly = true;
            this.Statistics.Width = 200;
            // 
            // AsianHandicapOdds
            // 
            this.AsianHandicapOdds.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.AsianHandicapOdds.DataPropertyName = "AsianHandicapOdds";
            this.AsianHandicapOdds.HeaderText = "AsianHandicap";
            this.AsianHandicapOdds.Name = "AsianHandicapOdds";
            this.AsianHandicapOdds.ReadOnly = true;
            // 
            // GoalLineOdds
            // 
            this.GoalLineOdds.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.GoalLineOdds.DataPropertyName = "GoalLineOdds";
            this.GoalLineOdds.HeaderText = "GoalLine";
            this.GoalLineOdds.Name = "GoalLineOdds";
            this.GoalLineOdds.ReadOnly = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dataGridHistory);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1185, 425);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "History";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataGridHistory
            // 
            this.dataGridHistory.AllowDrop = true;
            this.dataGridHistory.AllowUserToAddRows = false;
            this.dataGridHistory.AllowUserToDeleteRows = false;
            this.dataGridHistory.AutoGenerateColumns = false;
            this.dataGridHistory.BackgroundColor = System.Drawing.Color.White;
            this.dataGridHistory.CausesValidation = false;
            this.dataGridHistory.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Constantia", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.ButtonHighlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.AppWorkspace;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridHistory.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridHistory.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.MatchLabel,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4});
            this.dataGridHistory.DataSource = this.bindingResultSource;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(91)))), ((int)(((byte)(108)))), ((int)(((byte)(135)))));
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.DimGray;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridHistory.DefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridHistory.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridHistory.EnableHeadersVisualStyles = false;
            this.dataGridHistory.Location = new System.Drawing.Point(3, 3);
            this.dataGridHistory.Name = "dataGridHistory";
            this.dataGridHistory.ReadOnly = true;
            this.dataGridHistory.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.ButtonFace;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridHistory.RowHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dataGridHistory.RowHeadersVisible = false;
            dataGridViewCellStyle8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(62)))), ((int)(((byte)(68)))));
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.ControlDarkDark;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dataGridHistory.RowsDefaultCellStyle = dataGridViewCellStyle8;
            this.dataGridHistory.RowTemplate.Height = 100;
            this.dataGridHistory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridHistory.Size = new System.Drawing.Size(1179, 419);
            this.dataGridHistory.TabIndex = 27;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "IssuedTime";
            this.dataGridViewTextBoxColumn1.HeaderText = "PickedTime";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Width = 88;
            // 
            // MatchLabel
            // 
            this.MatchLabel.DataPropertyName = "MatchLabel";
            this.MatchLabel.HeaderText = "Match Label";
            this.MatchLabel.Name = "MatchLabel";
            this.MatchLabel.ReadOnly = true;
            this.MatchLabel.Width = 300;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "MatchDescription";
            this.dataGridViewTextBoxColumn2.HeaderText = "Match Data";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.Width = 200;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "CommandDescription";
            this.dataGridViewTextBoxColumn3.HeaderText = "Pick Data";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.Width = 300;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.DataPropertyName = "PickDescription";
            this.dataGridViewTextBoxColumn4.HeaderText = "Pick Description";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            this.dataGridViewTextBoxColumn4.Width = 300;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpInLog);
            this.tabControl1.Controls.Add(this.tpOutLog);
            this.tabControl1.Controls.Add(this.tpNotifications);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1057, 160);
            this.tabControl1.TabIndex = 42;
            // 
            // tpInLog
            // 
            this.tpInLog.Controls.Add(this.chkBoxInDataAutoScrll);
            this.tpInLog.Controls.Add(this.txtInDataLog);
            this.tpInLog.Location = new System.Drawing.Point(4, 22);
            this.tpInLog.Name = "tpInLog";
            this.tpInLog.Padding = new System.Windows.Forms.Padding(3);
            this.tpInLog.Size = new System.Drawing.Size(1049, 134);
            this.tpInLog.TabIndex = 0;
            this.tpInLog.Text = "IN - LOG";
            this.tpInLog.UseVisualStyleBackColor = true;
            // 
            // chkBoxInDataAutoScrll
            // 
            this.chkBoxInDataAutoScrll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkBoxInDataAutoScrll.Checked = true;
            this.chkBoxInDataAutoScrll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBoxInDataAutoScrll.Location = new System.Drawing.Point(966, 3);
            this.chkBoxInDataAutoScrll.Name = "chkBoxInDataAutoScrll";
            this.chkBoxInDataAutoScrll.Size = new System.Drawing.Size(77, 17);
            this.chkBoxInDataAutoScrll.TabIndex = 43;
            this.chkBoxInDataAutoScrll.Text = "Auto Scroll";
            this.chkBoxInDataAutoScrll.UseVisualStyleBackColor = true;
            // 
            // txtInDataLog
            // 
            this.txtInDataLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInDataLog.Cursor = System.Windows.Forms.Cursors.Hand;
            this.txtInDataLog.Location = new System.Drawing.Point(3, 21);
            this.txtInDataLog.Multiline = true;
            this.txtInDataLog.Name = "txtInDataLog";
            this.txtInDataLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtInDataLog.Size = new System.Drawing.Size(1043, 110);
            this.txtInDataLog.TabIndex = 42;
            // 
            // tpOutLog
            // 
            this.tpOutLog.Controls.Add(this.chkBoxOutDataAutoScrll);
            this.tpOutLog.Controls.Add(this.txtOutDataLog);
            this.tpOutLog.Location = new System.Drawing.Point(4, 22);
            this.tpOutLog.Name = "tpOutLog";
            this.tpOutLog.Padding = new System.Windows.Forms.Padding(3);
            this.tpOutLog.Size = new System.Drawing.Size(1049, 134);
            this.tpOutLog.TabIndex = 1;
            this.tpOutLog.Text = "OUT - LOG";
            this.tpOutLog.UseVisualStyleBackColor = true;
            // 
            // chkBoxOutDataAutoScrll
            // 
            this.chkBoxOutDataAutoScrll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkBoxOutDataAutoScrll.Checked = true;
            this.chkBoxOutDataAutoScrll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBoxOutDataAutoScrll.Location = new System.Drawing.Point(966, 3);
            this.chkBoxOutDataAutoScrll.Name = "chkBoxOutDataAutoScrll";
            this.chkBoxOutDataAutoScrll.Size = new System.Drawing.Size(77, 17);
            this.chkBoxOutDataAutoScrll.TabIndex = 33;
            this.chkBoxOutDataAutoScrll.Text = "Auto Scroll";
            this.chkBoxOutDataAutoScrll.UseVisualStyleBackColor = true;
            // 
            // txtOutDataLog
            // 
            this.txtOutDataLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutDataLog.Cursor = System.Windows.Forms.Cursors.Hand;
            this.txtOutDataLog.Location = new System.Drawing.Point(3, 21);
            this.txtOutDataLog.Multiline = true;
            this.txtOutDataLog.Name = "txtOutDataLog";
            this.txtOutDataLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtOutDataLog.Size = new System.Drawing.Size(1043, 110);
            this.txtOutDataLog.TabIndex = 32;
            // 
            // tpNotifications
            // 
            this.tpNotifications.Controls.Add(this.groupBox5);
            this.tpNotifications.Controls.Add(this.groupBox4);
            this.tpNotifications.Location = new System.Drawing.Point(4, 22);
            this.tpNotifications.Name = "tpNotifications";
            this.tpNotifications.Padding = new System.Windows.Forms.Padding(3);
            this.tpNotifications.Size = new System.Drawing.Size(1049, 134);
            this.tpNotifications.TabIndex = 2;
            this.tpNotifications.Text = "Notification option";
            this.tpNotifications.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.rdYellowUnder);
            this.groupBox5.Controls.Add(this.rdYellowOver);
            this.groupBox5.Controls.Add(this.rdCornerUnder);
            this.groupBox5.Controls.Add(this.rdCornerOver);
            this.groupBox5.Controls.Add(this.rdGoalUnder);
            this.groupBox5.Controls.Add(this.rdGoalOver);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox5.Location = new System.Drawing.Point(3, 48);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(1043, 46);
            this.groupBox5.TabIndex = 5;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Over/Under";
            // 
            // rdYellowUnder
            // 
            this.rdYellowUnder.AutoSize = true;
            this.rdYellowUnder.Location = new System.Drawing.Point(237, 49);
            this.rdYellowUnder.Name = "rdYellowUnder";
            this.rdYellowUnder.Size = new System.Drawing.Size(118, 17);
            this.rdYellowUnder.TabIndex = 8;
            this.rdYellowUnder.TabStop = true;
            this.rdYellowUnder.Text = "Yellow Cards Under";
            this.rdYellowUnder.UseVisualStyleBackColor = true;
            // 
            // rdYellowOver
            // 
            this.rdYellowOver.AutoSize = true;
            this.rdYellowOver.Location = new System.Drawing.Point(237, 19);
            this.rdYellowOver.Name = "rdYellowOver";
            this.rdYellowOver.Size = new System.Drawing.Size(112, 17);
            this.rdYellowOver.TabIndex = 7;
            this.rdYellowOver.TabStop = true;
            this.rdYellowOver.Text = "Yellow Cards Over";
            this.rdYellowOver.UseVisualStyleBackColor = true;
            // 
            // rdCornerUnder
            // 
            this.rdCornerUnder.AutoSize = true;
            this.rdCornerUnder.Location = new System.Drawing.Point(120, 49);
            this.rdCornerUnder.Name = "rdCornerUnder";
            this.rdCornerUnder.Size = new System.Drawing.Size(88, 17);
            this.rdCornerUnder.TabIndex = 6;
            this.rdCornerUnder.TabStop = true;
            this.rdCornerUnder.Text = "Corner Under";
            this.rdCornerUnder.UseVisualStyleBackColor = true;
            // 
            // rdCornerOver
            // 
            this.rdCornerOver.AutoSize = true;
            this.rdCornerOver.Location = new System.Drawing.Point(120, 19);
            this.rdCornerOver.Name = "rdCornerOver";
            this.rdCornerOver.Size = new System.Drawing.Size(82, 17);
            this.rdCornerOver.TabIndex = 5;
            this.rdCornerOver.TabStop = true;
            this.rdCornerOver.Text = "Corner Over";
            this.rdCornerOver.UseVisualStyleBackColor = true;
            // 
            // rdGoalUnder
            // 
            this.rdGoalUnder.AutoSize = true;
            this.rdGoalUnder.Location = new System.Drawing.Point(21, 49);
            this.rdGoalUnder.Name = "rdGoalUnder";
            this.rdGoalUnder.Size = new System.Drawing.Size(84, 17);
            this.rdGoalUnder.TabIndex = 4;
            this.rdGoalUnder.TabStop = true;
            this.rdGoalUnder.Text = "Goals Under";
            this.rdGoalUnder.UseVisualStyleBackColor = true;
            // 
            // rdGoalOver
            // 
            this.rdGoalOver.AutoSize = true;
            this.rdGoalOver.Location = new System.Drawing.Point(21, 19);
            this.rdGoalOver.Name = "rdGoalOver";
            this.rdGoalOver.Size = new System.Drawing.Size(78, 17);
            this.rdGoalOver.TabIndex = 3;
            this.rdGoalOver.TabStop = true;
            this.rdGoalOver.Text = "Goals Over";
            this.rdGoalOver.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.rdNothing);
            this.groupBox4.Controls.Add(this.rdCorner);
            this.groupBox4.Controls.Add(this.rdCards);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox4.Location = new System.Drawing.Point(3, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(1043, 45);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Corners/Card";
            // 
            // rdNothing
            // 
            this.rdNothing.AutoSize = true;
            this.rdNothing.Location = new System.Drawing.Point(237, 16);
            this.rdNothing.Name = "rdNothing";
            this.rdNothing.Size = new System.Drawing.Size(62, 17);
            this.rdNothing.TabIndex = 2;
            this.rdNothing.Text = "Nothing";
            this.rdNothing.UseVisualStyleBackColor = true;
            // 
            // rdCorner
            // 
            this.rdCorner.AutoSize = true;
            this.rdCorner.Checked = true;
            this.rdCorner.Location = new System.Drawing.Point(21, 16);
            this.rdCorner.Name = "rdCorner";
            this.rdCorner.Size = new System.Drawing.Size(61, 17);
            this.rdCorner.TabIndex = 0;
            this.rdCorner.TabStop = true;
            this.rdCorner.Text = "Corners";
            this.rdCorner.UseVisualStyleBackColor = true;
            // 
            // rdCards
            // 
            this.rdCards.AutoSize = true;
            this.rdCards.Location = new System.Drawing.Point(120, 16);
            this.rdCards.Name = "rdCards";
            this.rdCards.Size = new System.Drawing.Size(86, 17);
            this.rdCards.TabIndex = 1;
            this.rdCards.Text = "Yellow Cards";
            this.rdCards.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.btnTest);
            this.panel1.Controls.Add(this.btnSet);
            this.panel1.Controls.Add(this.btnStart);
            this.panel1.Controls.Add(this.btnStop);
            this.panel1.Controls.Add(this.btnExit);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(1057, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(136, 160);
            this.panel1.TabIndex = 38;
            // 
            // btnTest
            // 
            this.btnTest.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnTest.Location = new System.Drawing.Point(0, 111);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(134, 37);
            this.btnTest.TabIndex = 111;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnSet
            // 
            this.btnSet.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSet.Location = new System.Drawing.Point(0, 74);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(134, 37);
            this.btnSet.TabIndex = 107;
            this.btnSet.Text = "Setting";
            this.btnSet.UseVisualStyleBackColor = true;
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            // 
            // btnStart
            // 
            this.btnStart.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnStart.Location = new System.Drawing.Point(0, 37);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(134, 37);
            this.btnStart.TabIndex = 108;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Visible = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnStop.Location = new System.Drawing.Point(0, 0);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(134, 37);
            this.btnStop.TabIndex = 109;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Visible = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnExit
            // 
            this.btnExit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnExit.Location = new System.Drawing.Point(0, 121);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(134, 37);
            this.btnExit.TabIndex = 110;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1193, 615);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmMain";
            this.Text = "Bet365 Live Notifier ";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmMain_FormClosed);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.bindSource)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tbcSections.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tblMatch)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridHistory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingResultSource)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tpInLog.ResumeLayout(false);
            this.tpInLog.PerformLayout();
            this.tpOutLog.ResumeLayout(false);
            this.tpOutLog.PerformLayout();
            this.tpNotifications.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.BindingSource bindSource;
        private System.Windows.Forms.Timer setTimer;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TabControl tbcSections;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.DataGridView tblMatch;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tpInLog;
		private System.Windows.Forms.CheckBox chkBoxInDataAutoScrll;
		private System.Windows.Forms.TextBox txtInDataLog;
		private System.Windows.Forms.TabPage tpOutLog;
		private System.Windows.Forms.CheckBox chkBoxOutDataAutoScrll;
		private System.Windows.Forms.TextBox txtOutDataLog;
		private System.Windows.Forms.TabPage tpNotifications;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.RadioButton rdYellowUnder;
		private System.Windows.Forms.RadioButton rdYellowOver;
		private System.Windows.Forms.RadioButton rdCornerUnder;
		private System.Windows.Forms.RadioButton rdCornerOver;
		private System.Windows.Forms.RadioButton rdGoalUnder;
		private System.Windows.Forms.RadioButton rdGoalOver;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.RadioButton rdNothing;
		private System.Windows.Forms.RadioButton rdCorner;
		private System.Windows.Forms.RadioButton rdCards;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button btnSet;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.DataGridView dataGridHistory;
        private System.Windows.Forms.BindingSource bindingResultSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn MatchTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn LeagueName;
        private System.Windows.Forms.DataGridViewTextBoxColumn HomeName;
        private System.Windows.Forms.DataGridViewTextBoxColumn AwayTeam;
        private System.Windows.Forms.DataGridViewTextBoxColumn Score;
        private System.Windows.Forms.DataGridViewTextBoxColumn Statistics;
        private System.Windows.Forms.DataGridViewTextBoxColumn AsianHandicapOdds;
        private System.Windows.Forms.DataGridViewTextBoxColumn GoalLineOdds;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn MatchLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.Button btnTest;
    }
}

