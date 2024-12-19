namespace SeastoryServer
{
    partial class frmServerSetting
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
            this.chkHeartBeat = new System.Windows.Forms.CheckBox();
            this.textDBPort = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textDBPwd = new System.Windows.Forms.TextBox();
            this.textDBID = new System.Windows.Forms.TextBox();
            this.textDBName = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.numspinListenPort = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numspinIPD = new System.Windows.Forms.NumericUpDown();
            this.numspinIPC = new System.Windows.Forms.NumericUpDown();
            this.numspinIPB = new System.Windows.Forms.NumericUpDown();
            this.numspinIPA = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.numspinWebAPIPort = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.textNSTokenServer = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtPackage1Price = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtPackage2Price = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtPackage3Price = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtPackage4Price = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtPackage5Price = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numspinListenPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numspinIPD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numspinIPC)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numspinIPB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numspinIPA)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numspinWebAPIPort)).BeginInit();
            this.SuspendLayout();
            // 
            // chkHeartBeat
            // 
            this.chkHeartBeat.AutoSize = true;
            this.chkHeartBeat.Location = new System.Drawing.Point(256, 9);
            this.chkHeartBeat.Name = "chkHeartBeat";
            this.chkHeartBeat.Size = new System.Drawing.Size(76, 17);
            this.chkHeartBeat.TabIndex = 10;
            this.chkHeartBeat.Text = "Heart beat";
            this.chkHeartBeat.UseVisualStyleBackColor = true;
            this.chkHeartBeat.CheckedChanged += new System.EventHandler(this.chkHeartBeat_CheckedChanged);
            // 
            // textDBPort
            // 
            this.textDBPort.Location = new System.Drawing.Point(140, 160);
            this.textDBPort.MaxLength = 10;
            this.textDBPort.Name = "textDBPort";
            this.textDBPort.Size = new System.Drawing.Size(100, 20);
            this.textDBPort.TabIndex = 8;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(81, 164);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(47, 13);
            this.label8.TabIndex = 56;
            this.label8.Text = "DB Port:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textDBPwd
            // 
            this.textDBPwd.Location = new System.Drawing.Point(139, 136);
            this.textDBPwd.MaxLength = 20;
            this.textDBPwd.Name = "textDBPwd";
            this.textDBPwd.PasswordChar = '*';
            this.textDBPwd.Size = new System.Drawing.Size(100, 20);
            this.textDBPwd.TabIndex = 7;
            // 
            // textDBID
            // 
            this.textDBID.Location = new System.Drawing.Point(140, 112);
            this.textDBID.MaxLength = 20;
            this.textDBID.Name = "textDBID";
            this.textDBID.Size = new System.Drawing.Size(100, 20);
            this.textDBID.TabIndex = 6;
            // 
            // textDBName
            // 
            this.textDBName.Location = new System.Drawing.Point(140, 88);
            this.textDBName.MaxLength = 20;
            this.textDBName.Name = "textDBName";
            this.textDBName.Size = new System.Drawing.Size(100, 20);
            this.textDBName.TabIndex = 5;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(81, 139);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(51, 13);
            this.label7.TabIndex = 54;
            this.label7.Text = "DB Pass:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(81, 115);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 13);
            this.label5.TabIndex = 53;
            this.label5.Text = "DB ID:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(82, 91);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 52;
            this.label2.Text = "DB Name:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnCancel
            // 
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(180, 381);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Cancel(&C)";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(58, 381);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 11;
            this.btnOK.Text = "OK(&O)";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // numspinListenPort
            // 
            this.numspinListenPort.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.numspinListenPort.InterceptArrowKeys = false;
            this.numspinListenPort.Location = new System.Drawing.Point(164, 185);
            this.numspinListenPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numspinListenPort.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numspinListenPort.Name = "numspinListenPort";
            this.numspinListenPort.Size = new System.Drawing.Size(66, 20);
            this.numspinListenPort.TabIndex = 9;
            this.numspinListenPort.Tag = "";
            this.numspinListenPort.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numspinListenPort.Enter += new System.EventHandler(this.numspinListenPort_Enter);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(96, 189);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 50;
            this.label3.Text = "Server Port:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numspinIPD
            // 
            this.numspinIPD.Cursor = System.Windows.Forms.Cursors.Hand;
            this.numspinIPD.Location = new System.Drawing.Point(240, 55);
            this.numspinIPD.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numspinIPD.Name = "numspinIPD";
            this.numspinIPD.Size = new System.Drawing.Size(44, 20);
            this.numspinIPD.TabIndex = 4;
            this.numspinIPD.Enter += new System.EventHandler(this.numspinipD_Enter);
            // 
            // numspinIPC
            // 
            this.numspinIPC.Cursor = System.Windows.Forms.Cursors.Hand;
            this.numspinIPC.Location = new System.Drawing.Point(195, 55);
            this.numspinIPC.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numspinIPC.Name = "numspinIPC";
            this.numspinIPC.Size = new System.Drawing.Size(44, 20);
            this.numspinIPC.TabIndex = 3;
            this.numspinIPC.Enter += new System.EventHandler(this.numspinipC_Enter);
            // 
            // numspinIPB
            // 
            this.numspinIPB.Cursor = System.Windows.Forms.Cursors.Hand;
            this.numspinIPB.Location = new System.Drawing.Point(151, 55);
            this.numspinIPB.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numspinIPB.Name = "numspinIPB";
            this.numspinIPB.Size = new System.Drawing.Size(44, 20);
            this.numspinIPB.TabIndex = 2;
            this.numspinIPB.Enter += new System.EventHandler(this.numspinipB_Enter);
            // 
            // numspinIPA
            // 
            this.numspinIPA.Cursor = System.Windows.Forms.Cursors.Hand;
            this.numspinIPA.Location = new System.Drawing.Point(106, 55);
            this.numspinIPA.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numspinIPA.Name = "numspinIPA";
            this.numspinIPA.Size = new System.Drawing.Size(44, 20);
            this.numspinIPA.TabIndex = 1;
            this.numspinIPA.Enter += new System.EventHandler(this.numspinipA_Enter);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(23, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(270, 29);
            this.label6.TabIndex = 49;
            this.label6.Text = "Please enter the database ip address.";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(35, 57);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 48;
            this.label4.Text = "DB IP:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(86, 211);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 50;
            this.label1.Text = "WebAPI Port:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numspinWebAPIPort
            // 
            this.numspinWebAPIPort.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.numspinWebAPIPort.InterceptArrowKeys = false;
            this.numspinWebAPIPort.Location = new System.Drawing.Point(164, 209);
            this.numspinWebAPIPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numspinWebAPIPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numspinWebAPIPort.Name = "numspinWebAPIPort";
            this.numspinWebAPIPort.Size = new System.Drawing.Size(66, 20);
            this.numspinWebAPIPort.TabIndex = 9;
            this.numspinWebAPIPort.Tag = "";
            this.numspinWebAPIPort.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            this.numspinWebAPIPort.Enter += new System.EventHandler(this.numspinWebAPIPort_Enter);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(44, 235);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(90, 13);
            this.label9.TabIndex = 56;
            this.label9.Text = "NSToken Server:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textNSTokenServer
            // 
            this.textNSTokenServer.Location = new System.Drawing.Point(140, 232);
            this.textNSTokenServer.MaxLength = 50;
            this.textNSTokenServer.Name = "textNSTokenServer";
            this.textNSTokenServer.Size = new System.Drawing.Size(100, 20);
            this.textNSTokenServer.TabIndex = 8;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(68, 262);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(86, 13);
            this.label10.TabIndex = 56;
            this.label10.Text = "Package1 Price:";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPackage1Price
            // 
            this.txtPackage1Price.Location = new System.Drawing.Point(164, 259);
            this.txtPackage1Price.MaxLength = 50;
            this.txtPackage1Price.Name = "txtPackage1Price";
            this.txtPackage1Price.Size = new System.Drawing.Size(55, 20);
            this.txtPackage1Price.TabIndex = 8;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(68, 284);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(86, 13);
            this.label11.TabIndex = 56;
            this.label11.Text = "Package2 Price:";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPackage2Price
            // 
            this.txtPackage2Price.Location = new System.Drawing.Point(164, 281);
            this.txtPackage2Price.MaxLength = 50;
            this.txtPackage2Price.Name = "txtPackage2Price";
            this.txtPackage2Price.Size = new System.Drawing.Size(55, 20);
            this.txtPackage2Price.TabIndex = 8;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(68, 306);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(86, 13);
            this.label12.TabIndex = 56;
            this.label12.Text = "Package3 Price:";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPackage3Price
            // 
            this.txtPackage3Price.Location = new System.Drawing.Point(164, 303);
            this.txtPackage3Price.MaxLength = 50;
            this.txtPackage3Price.Name = "txtPackage3Price";
            this.txtPackage3Price.Size = new System.Drawing.Size(55, 20);
            this.txtPackage3Price.TabIndex = 8;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(68, 328);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(86, 13);
            this.label13.TabIndex = 56;
            this.label13.Text = "Package4 Price:";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPackage4Price
            // 
            this.txtPackage4Price.Location = new System.Drawing.Point(164, 325);
            this.txtPackage4Price.MaxLength = 50;
            this.txtPackage4Price.Name = "txtPackage4Price";
            this.txtPackage4Price.Size = new System.Drawing.Size(55, 20);
            this.txtPackage4Price.TabIndex = 8;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(68, 350);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(86, 13);
            this.label14.TabIndex = 56;
            this.label14.Text = "Package5 Price:";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPackage5Price
            // 
            this.txtPackage5Price.Location = new System.Drawing.Point(164, 347);
            this.txtPackage5Price.MaxLength = 50;
            this.txtPackage5Price.Name = "txtPackage5Price";
            this.txtPackage5Price.Size = new System.Drawing.Size(55, 20);
            this.txtPackage5Price.TabIndex = 8;
            // 
            // frmServerSetting
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(328, 416);
            this.Controls.Add(this.chkHeartBeat);
            this.Controls.Add(this.txtPackage5Price);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.txtPackage4Price);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.txtPackage3Price);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.txtPackage2Price);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtPackage1Price);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.textNSTokenServer);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.textDBPort);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.textDBPwd);
            this.Controls.Add(this.textDBID);
            this.Controls.Add(this.textDBName);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.numspinWebAPIPort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numspinListenPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numspinIPD);
            this.Controls.Add(this.numspinIPC);
            this.Controls.Add(this.numspinIPB);
            this.Controls.Add(this.numspinIPA);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label4);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmServerSetting";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "frmServerSetting";
            this.Load += new System.EventHandler(this.frmServerSetting_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numspinListenPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numspinIPD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numspinIPC)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numspinIPB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numspinIPA)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numspinWebAPIPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkHeartBeat;
        private System.Windows.Forms.TextBox textDBPort;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textDBPwd;
        private System.Windows.Forms.TextBox textDBID;
        private System.Windows.Forms.TextBox textDBName;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.NumericUpDown numspinListenPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numspinIPD;
        private System.Windows.Forms.NumericUpDown numspinIPC;
        private System.Windows.Forms.NumericUpDown numspinIPB;
        private System.Windows.Forms.NumericUpDown numspinIPA;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numspinWebAPIPort;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textNSTokenServer;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtPackage1Price;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtPackage2Price;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtPackage3Price;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtPackage4Price;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtPackage5Price;
    }
}