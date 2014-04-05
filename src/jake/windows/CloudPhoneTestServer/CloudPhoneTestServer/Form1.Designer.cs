namespace CloudPhoneTestServer
{
    partial class CloudPhoneForm
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
            this.btn_serverOn = new System.Windows.Forms.Button();
            this.listLog = new System.Windows.Forms.ListBox();
            this.btn_serverOff = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusBar = new System.Windows.Forms.ToolStripStatusLabel();
            this.camViewer = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.camViewer)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_serverOn
            // 
            this.btn_serverOn.Font = new System.Drawing.Font("Malgun Gothic", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btn_serverOn.Location = new System.Drawing.Point(438, 128);
            this.btn_serverOn.Name = "btn_serverOn";
            this.btn_serverOn.Size = new System.Drawing.Size(102, 86);
            this.btn_serverOn.TabIndex = 0;
            this.btn_serverOn.Text = "On";
            this.btn_serverOn.UseVisualStyleBackColor = true;
            this.btn_serverOn.Click += new System.EventHandler(this.btn_serverOn_Click);
            // 
            // listLog
            // 
            this.listLog.Cursor = System.Windows.Forms.Cursors.Default;
            this.listLog.Font = new System.Drawing.Font("Malgun Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.listLog.FormattingEnabled = true;
            this.listLog.ItemHeight = 15;
            this.listLog.Location = new System.Drawing.Point(12, 466);
            this.listLog.Name = "listLog";
            this.listLog.ScrollAlwaysVisible = true;
            this.listLog.Size = new System.Drawing.Size(724, 274);
            this.listLog.TabIndex = 1;
            this.listLog.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listLog_DrawItem);
            // 
            // btn_serverOff
            // 
            this.btn_serverOff.Font = new System.Drawing.Font("Malgun Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btn_serverOff.Location = new System.Drawing.Point(563, 128);
            this.btn_serverOff.Name = "btn_serverOff";
            this.btn_serverOff.Size = new System.Drawing.Size(102, 86);
            this.btn_serverOff.TabIndex = 2;
            this.btn_serverOff.Text = "Off";
            this.btn_serverOff.UseVisualStyleBackColor = true;
            this.btn_serverOff.Click += new System.EventHandler(this.btn_serverOff_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Malgun Gothic", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(12, 438);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 25);
            this.label1.TabIndex = 3;
            this.label1.Text = "Log정보";
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusBar});
            this.statusStrip.Location = new System.Drawing.Point(0, 745);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(750, 22);
            this.statusStrip.TabIndex = 4;
            // 
            // statusBar
            // 
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(0, 17);
            // 
            // camViewer
            // 
            this.camViewer.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.camViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.camViewer.Location = new System.Drawing.Point(36, 43);
            this.camViewer.Name = "camViewer";
            this.camViewer.Size = new System.Drawing.Size(325, 376);
            this.camViewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.camViewer.TabIndex = 5;
            this.camViewer.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Malgun Gothic", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.Location = new System.Drawing.Point(12, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 25);
            this.label2.TabIndex = 6;
            this.label2.Text = "CamViewer";
            // 
            // CloudPhoneForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 767);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.camViewer);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_serverOff);
            this.Controls.Add(this.listLog);
            this.Controls.Add(this.btn_serverOn);
            this.Name = "CloudPhoneForm";
            this.Text = "CloudPhoneTestServer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CloudPhoneForm_FormClosed);
            this.Load += new System.EventHandler(this.CloudPhoneForm_Load);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.camViewer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_serverOn;
        private System.Windows.Forms.ListBox listLog;
        private System.Windows.Forms.Button btn_serverOff;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusBar;
        private System.Windows.Forms.PictureBox camViewer;
        private System.Windows.Forms.Label label2;
    }
}

