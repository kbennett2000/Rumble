namespace Rumble2022
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.cmdListen = new System.Windows.Forms.Button();
            this.cmdStop = new System.Windows.Forms.Button();
            this.cmdMute = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboWaveIn = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboWaveOut = new System.Windows.Forms.ComboBox();
            this.cmdUseDevices = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.cmdSelectIDFile = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTimerInterval = new System.Windows.Forms.TextBox();
            this.lblWavIDFile = new System.Windows.Forms.Label();
            this.cmdSelectConfigLocation = new System.Windows.Forms.Button();
            this.lblConfigFilePath = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtServerNumber = new System.Windows.Forms.TextBox();
            this.txtChannelNumber = new System.Windows.Forms.TextBox();
            this.txtServerName = new System.Windows.Forms.TextBox();
            this.txtChannelName = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.txtCurrentServerURL = new System.Windows.Forms.TextBox();
            this.txtCurrentServerPort = new System.Windows.Forms.TextBox();
            this.txtCurrentServerUserName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(13, 256);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(850, 570);
            this.textBox1.TabIndex = 0;
            // 
            // cmdListen
            // 
            this.cmdListen.Enabled = false;
            this.cmdListen.Location = new System.Drawing.Point(13, 832);
            this.cmdListen.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cmdListen.Name = "cmdListen";
            this.cmdListen.Size = new System.Drawing.Size(88, 27);
            this.cmdListen.TabIndex = 1;
            this.cmdListen.Text = "Listen";
            this.cmdListen.UseVisualStyleBackColor = true;
            this.cmdListen.Click += new System.EventHandler(this.cmdListen_Click);
            // 
            // cmdStop
            // 
            this.cmdStop.Enabled = false;
            this.cmdStop.Location = new System.Drawing.Point(777, 832);
            this.cmdStop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cmdStop.Name = "cmdStop";
            this.cmdStop.Size = new System.Drawing.Size(88, 27);
            this.cmdStop.TabIndex = 2;
            this.cmdStop.Text = "Stop";
            this.cmdStop.UseVisualStyleBackColor = true;
            this.cmdStop.Click += new System.EventHandler(this.cmdStop_Click);
            // 
            // cmdMute
            // 
            this.cmdMute.Location = new System.Drawing.Point(109, 832);
            this.cmdMute.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cmdMute.Name = "cmdMute";
            this.cmdMute.Size = new System.Drawing.Size(88, 27);
            this.cmdMute.TabIndex = 3;
            this.cmdMute.Text = "Mute";
            this.cmdMute.UseVisualStyleBackColor = true;
            this.cmdMute.Click += new System.EventHandler(this.cmdMute_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 15);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "Wave In Device";
            // 
            // comboWaveIn
            // 
            this.comboWaveIn.FormattingEnabled = true;
            this.comboWaveIn.Location = new System.Drawing.Point(19, 35);
            this.comboWaveIn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboWaveIn.Name = "comboWaveIn";
            this.comboWaveIn.Size = new System.Drawing.Size(271, 23);
            this.comboWaveIn.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 67);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Wave Out Device";
            // 
            // comboWaveOut
            // 
            this.comboWaveOut.FormattingEnabled = true;
            this.comboWaveOut.Location = new System.Drawing.Point(19, 87);
            this.comboWaveOut.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboWaveOut.Name = "comboWaveOut";
            this.comboWaveOut.Size = new System.Drawing.Size(271, 23);
            this.comboWaveOut.TabIndex = 7;
            // 
            // cmdUseDevices
            // 
            this.cmdUseDevices.Location = new System.Drawing.Point(310, 87);
            this.cmdUseDevices.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cmdUseDevices.Name = "cmdUseDevices";
            this.cmdUseDevices.Size = new System.Drawing.Size(88, 27);
            this.cmdUseDevices.TabIndex = 8;
            this.cmdUseDevices.Text = "Use These";
            this.cmdUseDevices.UseVisualStyleBackColor = true;
            this.cmdUseDevices.Click += new System.EventHandler(this.cmdUseDevices_Click);
            // 
            // cmdSelectIDFile
            // 
            this.cmdSelectIDFile.Location = new System.Drawing.Point(488, 15);
            this.cmdSelectIDFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cmdSelectIDFile.Name = "cmdSelectIDFile";
            this.cmdSelectIDFile.Size = new System.Drawing.Size(88, 27);
            this.cmdSelectIDFile.TabIndex = 9;
            this.cmdSelectIDFile.Text = "Select ID File";
            this.cmdSelectIDFile.UseVisualStyleBackColor = true;
            this.cmdSelectIDFile.Click += new System.EventHandler(this.cmdSelectIDFile_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(350, 15);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "Timer Interval (sec)";
            // 
            // txtTimerInterval
            // 
            this.txtTimerInterval.Location = new System.Drawing.Point(354, 35);
            this.txtTimerInterval.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtTimerInterval.Name = "txtTimerInterval";
            this.txtTimerInterval.Size = new System.Drawing.Size(116, 23);
            this.txtTimerInterval.TabIndex = 11;
            this.txtTimerInterval.Text = "600";
            // 
            // lblWavIDFile
            // 
            this.lblWavIDFile.AutoSize = true;
            this.lblWavIDFile.Location = new System.Drawing.Point(582, 21);
            this.lblWavIDFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblWavIDFile.Name = "lblWavIDFile";
            this.lblWavIDFile.Size = new System.Drawing.Size(86, 15);
            this.lblWavIDFile.TabIndex = 12;
            this.lblWavIDFile.Text = "no file selected";
            // 
            // cmdSelectConfigLocation
            // 
            this.cmdSelectConfigLocation.Location = new System.Drawing.Point(488, 50);
            this.cmdSelectConfigLocation.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cmdSelectConfigLocation.Name = "cmdSelectConfigLocation";
            this.cmdSelectConfigLocation.Size = new System.Drawing.Size(88, 27);
            this.cmdSelectConfigLocation.TabIndex = 13;
            this.cmdSelectConfigLocation.Text = "Sel Config";
            this.cmdSelectConfigLocation.UseVisualStyleBackColor = true;
            this.cmdSelectConfigLocation.Click += new System.EventHandler(this.cmdSelectConfigLocation_Click);
            // 
            // lblConfigFilePath
            // 
            this.lblConfigFilePath.AutoSize = true;
            this.lblConfigFilePath.Location = new System.Drawing.Point(585, 50);
            this.lblConfigFilePath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblConfigFilePath.Name = "lblConfigFilePath";
            this.lblConfigFilePath.Size = new System.Drawing.Size(94, 15);
            this.lblConfigFilePath.TabIndex = 14;
            this.lblConfigFilePath.Text = "no path selected";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(25, 235);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(99, 15);
            this.label4.TabIndex = 15;
            this.label4.Text = "STATUS UPDATES:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 120);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 15);
            this.label5.TabIndex = 16;
            this.label5.Text = "STATUS:";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(575, 172);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(129, 15);
            this.label6.TabIndex = 17;
            this.label6.Text = "Current Server Number";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(563, 201);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(141, 15);
            this.label7.TabIndex = 18;
            this.label7.Text = "Current Channel Number";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(43, 175);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(117, 15);
            this.label8.TabIndex = 19;
            this.label8.Text = "Current Server Name";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(31, 204);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(129, 15);
            this.label9.TabIndex = 20;
            this.label9.Text = "Current Channel Name";
            // 
            // txtServerNumber
            // 
            this.txtServerNumber.Location = new System.Drawing.Point(710, 169);
            this.txtServerNumber.Name = "txtServerNumber";
            this.txtServerNumber.ReadOnly = true;
            this.txtServerNumber.Size = new System.Drawing.Size(153, 23);
            this.txtServerNumber.TabIndex = 21;
            // 
            // txtChannelNumber
            // 
            this.txtChannelNumber.Location = new System.Drawing.Point(710, 198);
            this.txtChannelNumber.Name = "txtChannelNumber";
            this.txtChannelNumber.ReadOnly = true;
            this.txtChannelNumber.Size = new System.Drawing.Size(153, 23);
            this.txtChannelNumber.TabIndex = 22;
            // 
            // txtServerName
            // 
            this.txtServerName.Location = new System.Drawing.Point(166, 172);
            this.txtServerName.Name = "txtServerName";
            this.txtServerName.ReadOnly = true;
            this.txtServerName.Size = new System.Drawing.Size(389, 23);
            this.txtServerName.TabIndex = 23;
            // 
            // txtChannelName
            // 
            this.txtChannelName.Location = new System.Drawing.Point(166, 201);
            this.txtChannelName.Name = "txtChannelName";
            this.txtChannelName.ReadOnly = true;
            this.txtChannelName.Size = new System.Drawing.Size(389, 23);
            this.txtChannelName.TabIndex = 24;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(54, 146);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(106, 15);
            this.label10.TabIndex = 25;
            this.label10.Text = "Current Server URL";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(632, 141);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(72, 15);
            this.label11.TabIndex = 26;
            this.label11.Text = "Current Port";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(639, 227);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(65, 15);
            this.label12.TabIndex = 27;
            this.label12.Text = "User Name";
            // 
            // txtCurrentServerURL
            // 
            this.txtCurrentServerURL.Location = new System.Drawing.Point(166, 143);
            this.txtCurrentServerURL.Name = "txtCurrentServerURL";
            this.txtCurrentServerURL.ReadOnly = true;
            this.txtCurrentServerURL.Size = new System.Drawing.Size(389, 23);
            this.txtCurrentServerURL.TabIndex = 28;
            // 
            // txtCurrentServerPort
            // 
            this.txtCurrentServerPort.Location = new System.Drawing.Point(710, 138);
            this.txtCurrentServerPort.Name = "txtCurrentServerPort";
            this.txtCurrentServerPort.ReadOnly = true;
            this.txtCurrentServerPort.Size = new System.Drawing.Size(153, 23);
            this.txtCurrentServerPort.TabIndex = 29;
            // 
            // txtCurrentServerUserName
            // 
            this.txtCurrentServerUserName.Location = new System.Drawing.Point(710, 227);
            this.txtCurrentServerUserName.Name = "txtCurrentServerUserName";
            this.txtCurrentServerUserName.ReadOnly = true;
            this.txtCurrentServerUserName.Size = new System.Drawing.Size(153, 23);
            this.txtCurrentServerUserName.TabIndex = 30;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(880, 871);
            this.Controls.Add(this.txtCurrentServerUserName);
            this.Controls.Add(this.txtCurrentServerPort);
            this.Controls.Add(this.txtCurrentServerURL);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txtChannelName);
            this.Controls.Add(this.txtServerName);
            this.Controls.Add(this.txtChannelNumber);
            this.Controls.Add(this.txtServerNumber);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblConfigFilePath);
            this.Controls.Add(this.cmdSelectConfigLocation);
            this.Controls.Add(this.lblWavIDFile);
            this.Controls.Add(this.txtTimerInterval);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmdSelectIDFile);
            this.Controls.Add(this.cmdUseDevices);
            this.Controls.Add(this.comboWaveOut);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboWaveIn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmdMute);
            this.Controls.Add(this.cmdStop);
            this.Controls.Add(this.cmdListen);
            this.Controls.Add(this.textBox1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Rumble";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button cmdListen;
        private System.Windows.Forms.Button cmdStop;
        private System.Windows.Forms.Button cmdMute;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboWaveIn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboWaveOut;
        private System.Windows.Forms.Button cmdUseDevices;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button cmdSelectIDFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtTimerInterval;
        private System.Windows.Forms.Label lblWavIDFile;
        private System.Windows.Forms.Button cmdSelectConfigLocation;
        private System.Windows.Forms.Label lblConfigFilePath;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
        private TextBox txtServerNumber;
        private TextBox txtChannelNumber;
        private TextBox txtServerName;
        private TextBox txtChannelName;
        private Label label10;
        private Label label11;
        private Label label12;
        private TextBox txtCurrentServerURL;
        private TextBox txtCurrentServerPort;
        private TextBox txtCurrentServerUserName;
    }
}

