
namespace BetburgerServer
{
    partial class ViewerForm
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
            this.webview2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.txtURL = new System.Windows.Forms.TextBox();
            this.btnGo = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.webview2)).BeginInit();
            this.SuspendLayout();
            // 
            // webview2
            // 
            this.webview2.AllowExternalDrop = true;
            this.webview2.CreationProperties = null;
            this.webview2.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webview2.Location = new System.Drawing.Point(-1, 28);
            this.webview2.Name = "webview2";
            this.webview2.Size = new System.Drawing.Size(1033, 643);
            this.webview2.TabIndex = 0;
            this.webview2.ZoomFactor = 1D;
            // 
            // txtURL
            // 
            this.txtURL.Location = new System.Drawing.Point(12, 5);
            this.txtURL.Name = "txtURL";
            this.txtURL.Size = new System.Drawing.Size(733, 20);
            this.txtURL.TabIndex = 1;
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point(751, 3);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(75, 23);
            this.btnGo.TabIndex = 2;
            this.btnGo.Text = "Go";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(862, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "BetSpanReady";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1034, 671);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.txtURL);
            this.Controls.Add(this.webview2);
            this.Name = "ViewerForm";
            this.Text = "Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ViewerForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.webview2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webview2;
        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.Button button1;
    }
}