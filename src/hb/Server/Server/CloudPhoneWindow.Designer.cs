namespace Server
{
    partial class CloudPhoneWindow
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CloudPhoneWindow));
            this.Log_List = new System.Windows.Forms.ListBox();
            this.btn_start = new System.Windows.Forms.Button();
            this.btn_stop = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.TrayMenu_About = new System.Windows.Forms.ToolStripMenuItem();
            this.TrayMenu_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.trayMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // Log_List
            // 
            this.Log_List.FormattingEnabled = true;
            this.Log_List.ItemHeight = 12;
            this.Log_List.Location = new System.Drawing.Point(48, 25);
            this.Log_List.Name = "Log_List";
            this.Log_List.Size = new System.Drawing.Size(1391, 232);
            this.Log_List.TabIndex = 0;
            this.Log_List.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.Log_List_DrawItem);
            // 
            // btn_start
            // 
            this.btn_start.Location = new System.Drawing.Point(120, 292);
            this.btn_start.Name = "btn_start";
            this.btn_start.Size = new System.Drawing.Size(129, 55);
            this.btn_start.TabIndex = 1;
            this.btn_start.Text = "Start";
            this.btn_start.UseVisualStyleBackColor = true;
            this.btn_start.Click += new System.EventHandler(this.btn_start_Click);
            // 
            // btn_stop
            // 
            this.btn_stop.Location = new System.Drawing.Point(319, 292);
            this.btn_stop.Name = "btn_stop";
            this.btn_stop.Size = new System.Drawing.Size(127, 55);
            this.btn_stop.TabIndex = 2;
            this.btn_stop.Text = "Stop";
            this.btn_stop.UseVisualStyleBackColor = true;
            this.btn_stop.Click += new System.EventHandler(this.btn_stop_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "notifyIcon1";
            this.notifyIcon.Visible = true;
            this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_DoubleClick);
            // 
            // trayMenuStrip
            // 
            this.trayMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TrayMenu_About,
            this.TrayMenu_Exit});
            this.trayMenuStrip.Name = "trayMenuStrip";
            this.trayMenuStrip.Size = new System.Drawing.Size(117, 48);
            // 
            // TrayMenu_About
            // 
            this.TrayMenu_About.Name = "TrayMenu_About";
            this.TrayMenu_About.Size = new System.Drawing.Size(116, 22);
            this.TrayMenu_About.Text = "About...";
            this.TrayMenu_About.Click += new System.EventHandler(this.TrayMenu_About_Click);
            // 
            // TrayMenu_Exit
            // 
            this.TrayMenu_Exit.Name = "TrayMenu_Exit";
            this.TrayMenu_Exit.Size = new System.Drawing.Size(116, 22);
            this.TrayMenu_Exit.Text = "Exit";
            this.TrayMenu_Exit.Click += new System.EventHandler(this.TrayMenu_Exit_Click);
            // 
            // CloudPhoneWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1451, 367);
            this.Controls.Add(this.btn_stop);
            this.Controls.Add(this.btn_start);
            this.Controls.Add(this.Log_List);
            this.Name = "CloudPhoneWindow";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CloudPhoneWindow_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CloudPhoneWindow_FormClosed);
            this.Load += new System.EventHandler(this.CloudPhoneWindow_Load);
            this.trayMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox Log_List;
        private System.Windows.Forms.Button btn_start;
        private System.Windows.Forms.Button btn_stop;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip trayMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem TrayMenu_About;
        private System.Windows.Forms.ToolStripMenuItem TrayMenu_Exit;
    }
}

