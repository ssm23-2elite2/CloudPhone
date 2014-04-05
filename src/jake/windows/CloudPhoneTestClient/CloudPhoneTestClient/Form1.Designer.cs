namespace CloudPhoneTestClient
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
            this.btn_connectionOn = new System.Windows.Forms.Button();
            this.btn_connectionOff = new System.Windows.Forms.Button();
            this.btn_open = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_connectionOn
            // 
            this.btn_connectionOn.Font = new System.Drawing.Font("Malgun Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btn_connectionOn.Location = new System.Drawing.Point(13, 12);
            this.btn_connectionOn.Name = "btn_connectionOn";
            this.btn_connectionOn.Size = new System.Drawing.Size(90, 36);
            this.btn_connectionOn.TabIndex = 0;
            this.btn_connectionOn.Text = "On";
            this.btn_connectionOn.UseVisualStyleBackColor = true;
            this.btn_connectionOn.Click += new System.EventHandler(this.btn_connectionOn_Click);
            // 
            // btn_connectionOff
            // 
            this.btn_connectionOff.Font = new System.Drawing.Font("Malgun Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btn_connectionOff.Location = new System.Drawing.Point(109, 12);
            this.btn_connectionOff.Name = "btn_connectionOff";
            this.btn_connectionOff.Size = new System.Drawing.Size(90, 36);
            this.btn_connectionOff.TabIndex = 1;
            this.btn_connectionOff.Text = "Off";
            this.btn_connectionOff.UseVisualStyleBackColor = true;
            this.btn_connectionOff.Click += new System.EventHandler(this.btn_connectionOff_Click);
            // 
            // btn_open
            // 
            this.btn_open.Font = new System.Drawing.Font("Malgun Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btn_open.Location = new System.Drawing.Point(206, 12);
            this.btn_open.Name = "btn_open";
            this.btn_open.Size = new System.Drawing.Size(102, 36);
            this.btn_open.TabIndex = 4;
            this.btn_open.Text = "Open";
            this.btn_open.UseVisualStyleBackColor = true;
            this.btn_open.Click += new System.EventHandler(this.btn_openClick);
            // 
            // CloudPhoneForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(325, 58);
            this.Controls.Add(this.btn_open);
            this.Controls.Add(this.btn_connectionOff);
            this.Controls.Add(this.btn_connectionOn);
            this.Name = "CloudPhoneForm";
            this.Text = "CloudPhoneTestClient";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CloudPhoneForm_FormClosed);
            this.Load += new System.EventHandler(this.CloudPhoneForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_connectionOn;
        private System.Windows.Forms.Button btn_connectionOff;
        private System.Windows.Forms.Button btn_open;
    }
}

