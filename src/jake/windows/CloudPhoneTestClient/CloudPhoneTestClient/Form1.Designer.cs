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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_open = new System.Windows.Forms.Button();
            this.camViewer = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.camViewer)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_connectionOn
            // 
            this.btn_connectionOn.Font = new System.Drawing.Font("Malgun Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btn_connectionOn.Location = new System.Drawing.Point(105, 559);
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
            this.btn_connectionOff.Location = new System.Drawing.Point(201, 559);
            this.btn_connectionOff.Name = "btn_connectionOff";
            this.btn_connectionOff.Size = new System.Drawing.Size(90, 36);
            this.btn_connectionOff.TabIndex = 1;
            this.btn_connectionOff.Text = "Off";
            this.btn_connectionOff.UseVisualStyleBackColor = true;
            this.btn_connectionOff.Click += new System.EventHandler(this.btn_connectionOff_Click);
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Malgun Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.textBox1.Location = new System.Drawing.Point(165, 513);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(235, 29);
            this.textBox1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Malgun Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(101, 513);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 21);
            this.label1.TabIndex = 3;
            this.label1.Text = "아이피";
            // 
            // btn_open
            // 
            this.btn_open.Font = new System.Drawing.Font("Malgun Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btn_open.Location = new System.Drawing.Point(298, 559);
            this.btn_open.Name = "btn_open";
            this.btn_open.Size = new System.Drawing.Size(102, 36);
            this.btn_open.TabIndex = 4;
            this.btn_open.Text = "Open";
            this.btn_open.UseVisualStyleBackColor = true;
            this.btn_open.Click += new System.EventHandler(this.btn_openClick);
            // 
            // camViewer
            // 
            this.camViewer.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.camViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.camViewer.Location = new System.Drawing.Point(12, 12);
            this.camViewer.Name = "camViewer";
            this.camViewer.Size = new System.Drawing.Size(496, 476);
            this.camViewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.camViewer.TabIndex = 5;
            this.camViewer.TabStop = false;
            // 
            // CloudPhoneForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 622);
            this.Controls.Add(this.camViewer);
            this.Controls.Add(this.btn_open);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btn_connectionOff);
            this.Controls.Add(this.btn_connectionOn);
            this.Name = "CloudPhoneForm";
            this.Text = "CloudPhoneTestClient";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CloudPhoneForm_FormClosed);
            this.Load += new System.EventHandler(this.CloudPhoneForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.camViewer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_connectionOn;
        private System.Windows.Forms.Button btn_connectionOff;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_open;
        private System.Windows.Forms.PictureBox camViewer;
    }
}

