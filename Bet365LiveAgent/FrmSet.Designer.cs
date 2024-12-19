
namespace Bet365LiveAgent
{
    partial class FrmSet
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
            this.txtBet365Domain = new System.Windows.Forms.TextBox();
            this.lbBet365Domain = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.cmbConditionChecker = new System.Windows.Forms.ComboBox();
            this.cmbConditionComparer = new System.Windows.Forms.ComboBox();
            this.txtConditionValue = new System.Windows.Forms.TextBox();
            this.lstConditions = new System.Windows.Forms.ListBox();
            this.btnConditionAdd = new System.Windows.Forms.Button();
            this.btnConditionEdit = new System.Windows.Forms.Button();
            this.btnConditionRemove = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lstCommands = new System.Windows.Forms.ListBox();
            this.btnCommandAdd = new System.Windows.Forms.Button();
            this.btnCommandEdit = new System.Windows.Forms.Button();
            this.btnCommandRemove = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbBetPlusMinus = new System.Windows.Forms.ComboBox();
            this.cmbBetMarket = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cmbCommandOddComparer = new System.Windows.Forms.ComboBox();
            this.txtCommandOddValue = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.btnCommandRevertClone = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCommandHandicap = new System.Windows.Forms.TextBox();
            this.cmbBetOverUnder = new System.Windows.Forms.ComboBox();
            this.cmbBetTeam = new System.Windows.Forms.ComboBox();
            this.lstGroups = new System.Windows.Forms.ListBox();
            this.btnGroupAdd = new System.Windows.Forms.Button();
            this.btnGroupEdit = new System.Windows.Forms.Button();
            this.btnGroupRemove = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtBet365Domain
            // 
            this.txtBet365Domain.Location = new System.Drawing.Point(181, 12);
            this.txtBet365Domain.Name = "txtBet365Domain";
            this.txtBet365Domain.Size = new System.Drawing.Size(99, 20);
            this.txtBet365Domain.TabIndex = 18;
            // 
            // lbBet365Domain
            // 
            this.lbBet365Domain.AutoSize = true;
            this.lbBet365Domain.Location = new System.Drawing.Point(92, 17);
            this.lbBet365Domain.Name = "lbBet365Domain";
            this.lbBet365Domain.Size = new System.Drawing.Size(83, 13);
            this.lbBet365Domain.TabIndex = 17;
            this.lbBet365Domain.Text = "Bet365 Domain:";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(681, 12);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(86, 20);
            this.btnSave.TabIndex = 30;
            this.btnSave.Text = "Change";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // cmbConditionChecker
            // 
            this.cmbConditionChecker.FormattingEnabled = true;
            this.cmbConditionChecker.Location = new System.Drawing.Point(83, 32);
            this.cmbConditionChecker.Name = "cmbConditionChecker";
            this.cmbConditionChecker.Size = new System.Drawing.Size(185, 21);
            this.cmbConditionChecker.TabIndex = 33;
            this.cmbConditionChecker.SelectedIndexChanged += new System.EventHandler(this.cmbConditionChecker_SelectedIndexChanged);
            // 
            // cmbConditionComparer
            // 
            this.cmbConditionComparer.FormattingEnabled = true;
            this.cmbConditionComparer.Location = new System.Drawing.Point(83, 68);
            this.cmbConditionComparer.Name = "cmbConditionComparer";
            this.cmbConditionComparer.Size = new System.Drawing.Size(185, 21);
            this.cmbConditionComparer.TabIndex = 33;
            this.cmbConditionComparer.SelectedIndexChanged += new System.EventHandler(this.cmbConditionComparer_SelectedIndexChanged);
            // 
            // txtConditionValue
            // 
            this.txtConditionValue.Location = new System.Drawing.Point(83, 104);
            this.txtConditionValue.Name = "txtConditionValue";
            this.txtConditionValue.Size = new System.Drawing.Size(185, 20);
            this.txtConditionValue.TabIndex = 32;
            this.txtConditionValue.TextChanged += new System.EventHandler(this.txtConditionValue_TextChanged);
            // 
            // lstConditions
            // 
            this.lstConditions.FormattingEnabled = true;
            this.lstConditions.Location = new System.Drawing.Point(340, 19);
            this.lstConditions.Name = "lstConditions";
            this.lstConditions.Size = new System.Drawing.Size(216, 173);
            this.lstConditions.TabIndex = 35;
            this.lstConditions.SelectedIndexChanged += new System.EventHandler(this.lstConditions_SelectedIndexChanged);
            // 
            // btnConditionAdd
            // 
            this.btnConditionAdd.Location = new System.Drawing.Point(274, 29);
            this.btnConditionAdd.Name = "btnConditionAdd";
            this.btnConditionAdd.Size = new System.Drawing.Size(60, 23);
            this.btnConditionAdd.TabIndex = 36;
            this.btnConditionAdd.Text = "Add";
            this.btnConditionAdd.UseVisualStyleBackColor = true;
            this.btnConditionAdd.Click += new System.EventHandler(this.btnConditionAdd_Click);
            // 
            // btnConditionEdit
            // 
            this.btnConditionEdit.Location = new System.Drawing.Point(274, 58);
            this.btnConditionEdit.Name = "btnConditionEdit";
            this.btnConditionEdit.Size = new System.Drawing.Size(60, 23);
            this.btnConditionEdit.TabIndex = 36;
            this.btnConditionEdit.Text = "Update";
            this.btnConditionEdit.UseVisualStyleBackColor = true;
            this.btnConditionEdit.Click += new System.EventHandler(this.btnConditionEdit_Click);
            // 
            // btnConditionRemove
            // 
            this.btnConditionRemove.Location = new System.Drawing.Point(274, 87);
            this.btnConditionRemove.Name = "btnConditionRemove";
            this.btnConditionRemove.Size = new System.Drawing.Size(60, 23);
            this.btnConditionRemove.TabIndex = 36;
            this.btnConditionRemove.Text = "Remove";
            this.btnConditionRemove.UseVisualStyleBackColor = true;
            this.btnConditionRemove.Click += new System.EventHandler(this.btnConditionRemove_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnGroupRemove);
            this.groupBox1.Controls.Add(this.btnConditionRemove);
            this.groupBox1.Controls.Add(this.btnGroupEdit);
            this.groupBox1.Controls.Add(this.btnConditionEdit);
            this.groupBox1.Controls.Add(this.btnGroupAdd);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.btnConditionAdd);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.lstGroups);
            this.groupBox1.Controls.Add(this.lstConditions);
            this.groupBox1.Controls.Add(this.cmbConditionChecker);
            this.groupBox1.Controls.Add(this.cmbConditionComparer);
            this.groupBox1.Controls.Add(this.txtConditionValue);
            this.groupBox1.Location = new System.Drawing.Point(12, 53);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(896, 205);
            this.groupBox1.TabIndex = 39;
            this.groupBox1.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(32, 106);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 38;
            this.label5.Text = "Value:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 70);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 13);
            this.label4.TabIndex = 38;
            this.label4.Text = "Comparison:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 38;
            this.label3.Text = "Conditions:";
            // 
            // lstCommands
            // 
            this.lstCommands.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstCommands.FormattingEnabled = true;
            this.lstCommands.HorizontalScrollbar = true;
            this.lstCommands.Location = new System.Drawing.Point(12, 351);
            this.lstCommands.Name = "lstCommands";
            this.lstCommands.Size = new System.Drawing.Size(896, 121);
            this.lstCommands.TabIndex = 35;
            this.lstCommands.SelectedIndexChanged += new System.EventHandler(this.lstCommands_SelectedIndexChanged);
            // 
            // btnCommandAdd
            // 
            this.btnCommandAdd.Location = new System.Drawing.Point(249, 322);
            this.btnCommandAdd.Name = "btnCommandAdd";
            this.btnCommandAdd.Size = new System.Drawing.Size(75, 23);
            this.btnCommandAdd.TabIndex = 36;
            this.btnCommandAdd.Text = "Add";
            this.btnCommandAdd.UseVisualStyleBackColor = true;
            this.btnCommandAdd.Click += new System.EventHandler(this.btnCommandAdd_Click);
            // 
            // btnCommandEdit
            // 
            this.btnCommandEdit.Location = new System.Drawing.Point(331, 322);
            this.btnCommandEdit.Name = "btnCommandEdit";
            this.btnCommandEdit.Size = new System.Drawing.Size(75, 23);
            this.btnCommandEdit.TabIndex = 36;
            this.btnCommandEdit.Text = "Update";
            this.btnCommandEdit.UseVisualStyleBackColor = true;
            this.btnCommandEdit.Click += new System.EventHandler(this.btnCommandEdit_Click);
            // 
            // btnCommandRemove
            // 
            this.btnCommandRemove.Location = new System.Drawing.Point(415, 322);
            this.btnCommandRemove.Name = "btnCommandRemove";
            this.btnCommandRemove.Size = new System.Drawing.Size(75, 23);
            this.btnCommandRemove.TabIndex = 36;
            this.btnCommandRemove.Text = "Remove";
            this.btnCommandRemove.UseVisualStyleBackColor = true;
            this.btnCommandRemove.Click += new System.EventHandler(this.btnCommandRemove_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(120, 267);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 13);
            this.label6.TabIndex = 38;
            this.label6.Text = "Market:";
            // 
            // cmbBetPlusMinus
            // 
            this.cmbBetPlusMinus.FormattingEnabled = true;
            this.cmbBetPlusMinus.Location = new System.Drawing.Point(312, 288);
            this.cmbBetPlusMinus.Name = "cmbBetPlusMinus";
            this.cmbBetPlusMinus.Size = new System.Drawing.Size(79, 21);
            this.cmbBetPlusMinus.TabIndex = 33;
            this.cmbBetPlusMinus.SelectedIndexChanged += new System.EventHandler(this.cmbBetSide_SelectedIndexChanged);
            // 
            // cmbBetMarket
            // 
            this.cmbBetMarket.FormattingEnabled = true;
            this.cmbBetMarket.Location = new System.Drawing.Point(123, 288);
            this.cmbBetMarket.Name = "cmbBetMarket";
            this.cmbBetMarket.Size = new System.Drawing.Size(99, 21);
            this.cmbBetMarket.TabIndex = 33;
            this.cmbBetMarket.SelectedIndexChanged += new System.EventHandler(this.cmbBetMarket_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(309, 267);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(31, 13);
            this.label7.TabIndex = 38;
            this.label7.Text = "Side:";
            // 
            // cmbCommandOddComparer
            // 
            this.cmbCommandOddComparer.FormattingEnabled = true;
            this.cmbCommandOddComparer.Location = new System.Drawing.Point(563, 289);
            this.cmbCommandOddComparer.Name = "cmbCommandOddComparer";
            this.cmbCommandOddComparer.Size = new System.Drawing.Size(61, 21);
            this.cmbCommandOddComparer.TabIndex = 33;
            this.cmbCommandOddComparer.SelectedIndexChanged += new System.EventHandler(this.cmbCommandOddComparer_SelectedIndexChanged);
            // 
            // txtCommandOddValue
            // 
            this.txtCommandOddValue.Location = new System.Drawing.Point(646, 289);
            this.txtCommandOddValue.Name = "txtCommandOddValue";
            this.txtCommandOddValue.Size = new System.Drawing.Size(55, 20);
            this.txtCommandOddValue.TabIndex = 32;
            this.txtCommandOddValue.TextChanged += new System.EventHandler(this.txtCommandOddValue_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(643, 268);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 38;
            this.label2.Text = "Value:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(560, 268);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 13);
            this.label8.TabIndex = 38;
            this.label8.Text = "Comparison:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(524, 294);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(30, 13);
            this.label9.TabIndex = 38;
            this.label9.Text = "Odd:";
            // 
            // btnCommandRevertClone
            // 
            this.btnCommandRevertClone.Location = new System.Drawing.Point(496, 322);
            this.btnCommandRevertClone.Name = "btnCommandRevertClone";
            this.btnCommandRevertClone.Size = new System.Drawing.Size(89, 23);
            this.btnCommandRevertClone.TabIndex = 36;
            this.btnCommandRevertClone.Text = "Clone Revert";
            this.btnCommandRevertClone.UseVisualStyleBackColor = true;
            this.btnCommandRevertClone.Click += new System.EventHandler(this.btnCommandRevertClone_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(447, 268);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 38;
            this.label1.Text = "Handicap:";
            // 
            // txtCommandHandicap
            // 
            this.txtCommandHandicap.Location = new System.Drawing.Point(450, 289);
            this.txtCommandHandicap.Name = "txtCommandHandicap";
            this.txtCommandHandicap.Size = new System.Drawing.Size(55, 20);
            this.txtCommandHandicap.TabIndex = 32;
            this.txtCommandHandicap.TextChanged += new System.EventHandler(this.txtCommandHandicap_TextChanged);
            // 
            // cmbBetOverUnder
            // 
            this.cmbBetOverUnder.FormattingEnabled = true;
            this.cmbBetOverUnder.Location = new System.Drawing.Point(312, 288);
            this.cmbBetOverUnder.Name = "cmbBetOverUnder";
            this.cmbBetOverUnder.Size = new System.Drawing.Size(79, 21);
            this.cmbBetOverUnder.TabIndex = 33;
            this.cmbBetOverUnder.SelectedIndexChanged += new System.EventHandler(this.cmbBetOverUnder_SelectedIndexChanged);
            // 
            // cmbBetTeam
            // 
            this.cmbBetTeam.FormattingEnabled = true;
            this.cmbBetTeam.Location = new System.Drawing.Point(228, 288);
            this.cmbBetTeam.Name = "cmbBetTeam";
            this.cmbBetTeam.Size = new System.Drawing.Size(79, 21);
            this.cmbBetTeam.TabIndex = 33;
            this.cmbBetTeam.SelectedIndexChanged += new System.EventHandler(this.cmbBetOverUnder_SelectedIndexChanged);
            // 
            // lstGroups
            // 
            this.lstGroups.FormattingEnabled = true;
            this.lstGroups.Location = new System.Drawing.Point(628, 19);
            this.lstGroups.Name = "lstGroups";
            this.lstGroups.Size = new System.Drawing.Size(257, 173);
            this.lstGroups.TabIndex = 35;
            this.lstGroups.SelectedIndexChanged += new System.EventHandler(this.lstConditions_SelectedIndexChanged);
            // 
            // btnGroupAdd
            // 
            this.btnGroupAdd.Location = new System.Drawing.Point(562, 29);
            this.btnGroupAdd.Name = "btnGroupAdd";
            this.btnGroupAdd.Size = new System.Drawing.Size(60, 23);
            this.btnGroupAdd.TabIndex = 36;
            this.btnGroupAdd.Text = "Add";
            this.btnGroupAdd.UseVisualStyleBackColor = true;
            this.btnGroupAdd.Click += new System.EventHandler(this.btnGroupAdd_Click);
            // 
            // btnGroupEdit
            // 
            this.btnGroupEdit.Location = new System.Drawing.Point(562, 58);
            this.btnGroupEdit.Name = "btnGroupEdit";
            this.btnGroupEdit.Size = new System.Drawing.Size(60, 23);
            this.btnGroupEdit.TabIndex = 36;
            this.btnGroupEdit.Text = "Update";
            this.btnGroupEdit.UseVisualStyleBackColor = true;
            this.btnGroupEdit.Click += new System.EventHandler(this.btnGroupEdit_Click);
            // 
            // btnGroupRemove
            // 
            this.btnGroupRemove.Location = new System.Drawing.Point(562, 87);
            this.btnGroupRemove.Name = "btnGroupRemove";
            this.btnGroupRemove.Size = new System.Drawing.Size(60, 23);
            this.btnGroupRemove.TabIndex = 36;
            this.btnGroupRemove.Text = "Remove";
            this.btnGroupRemove.UseVisualStyleBackColor = true;
            this.btnGroupRemove.Click += new System.EventHandler(this.btnGroupRemove_Click);
            // 
            // FrmSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(920, 483);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCommandRevertClone);
            this.Controls.Add(this.btnCommandRemove);
            this.Controls.Add(this.btnCommandEdit);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtBet365Domain);
            this.Controls.Add(this.btnCommandAdd);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lbBet365Domain);
            this.Controls.Add(this.cmbCommandOddComparer);
            this.Controls.Add(this.txtCommandHandicap);
            this.Controls.Add(this.txtCommandOddValue);
            this.Controls.Add(this.cmbBetTeam);
            this.Controls.Add(this.cmbBetOverUnder);
            this.Controls.Add(this.cmbBetMarket);
            this.Controls.Add(this.cmbBetPlusMinus);
            this.Controls.Add(this.lstCommands);
            this.Name = "FrmSet";
            this.Text = "Setting";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmSet_FormClosing);
            this.Load += new System.EventHandler(this.FrmSet_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtBet365Domain;
        private System.Windows.Forms.Label lbBet365Domain;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.ComboBox cmbConditionChecker;
        private System.Windows.Forms.ComboBox cmbConditionComparer;
        private System.Windows.Forms.TextBox txtConditionValue;
        private System.Windows.Forms.ListBox lstConditions;
        private System.Windows.Forms.Button btnConditionAdd;
        private System.Windows.Forms.Button btnConditionEdit;
        private System.Windows.Forms.Button btnConditionRemove;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox lstCommands;
        private System.Windows.Forms.Button btnCommandAdd;
        private System.Windows.Forms.Button btnCommandEdit;
        private System.Windows.Forms.Button btnCommandRemove;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbBetPlusMinus;
        private System.Windows.Forms.ComboBox cmbBetMarket;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbCommandOddComparer;
        private System.Windows.Forms.TextBox txtCommandOddValue;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnCommandRevertClone;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCommandHandicap;
        private System.Windows.Forms.ComboBox cmbBetOverUnder;
        private System.Windows.Forms.ComboBox cmbBetTeam;
        private System.Windows.Forms.Button btnGroupRemove;
        private System.Windows.Forms.Button btnGroupEdit;
        private System.Windows.Forms.Button btnGroupAdd;
        private System.Windows.Forms.ListBox lstGroups;
    }
}