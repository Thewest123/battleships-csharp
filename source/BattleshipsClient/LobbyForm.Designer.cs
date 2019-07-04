namespace BattleshipsClient
{
    partial class LobbyForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LobbyForm));
            this.label1 = new System.Windows.Forms.Label();
            this.lblServerIp = new System.Windows.Forms.Label();
            this.rtbChat = new System.Windows.Forms.RichTextBox();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.listPlayers = new System.Windows.Forms.ListBox();
            this.btnChallenge = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.lblPlayers = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server:";
            // 
            // lblServerIp
            // 
            this.lblServerIp.AutoSize = true;
            this.lblServerIp.Location = new System.Drawing.Point(76, 22);
            this.lblServerIp.Name = "lblServerIp";
            this.lblServerIp.Size = new System.Drawing.Size(91, 13);
            this.lblServerIp.TabIndex = 1;
            this.lblServerIp.Text = "127.0.0.1 : 20123";
            // 
            // rtbChat
            // 
            this.rtbChat.BackColor = System.Drawing.SystemColors.Window;
            this.rtbChat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtbChat.Location = new System.Drawing.Point(32, 52);
            this.rtbChat.Name = "rtbChat";
            this.rtbChat.ReadOnly = true;
            this.rtbChat.Size = new System.Drawing.Size(371, 342);
            this.rtbChat.TabIndex = 2;
            this.rtbChat.Text = "Připojte se k chatu odesláním zprávy!";
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(32, 403);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(291, 20);
            this.txtMessage.TabIndex = 0;
            this.txtMessage.Click += new System.EventHandler(this.txtMessage_Click);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(329, 401);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(74, 23);
            this.btnSend.TabIndex = 4;
            this.btnSend.Text = "Odeslat";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // listPlayers
            // 
            this.listPlayers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listPlayers.FormattingEnabled = true;
            this.listPlayers.Location = new System.Drawing.Point(409, 52);
            this.listPlayers.Name = "listPlayers";
            this.listPlayers.Size = new System.Drawing.Size(170, 340);
            this.listPlayers.TabIndex = 5;
            this.listPlayers.Click += new System.EventHandler(this.listPlayers_Click);
            this.listPlayers.DoubleClick += new System.EventHandler(this.listPlayers_DoubleClick);
            // 
            // btnChallenge
            // 
            this.btnChallenge.Location = new System.Drawing.Point(409, 401);
            this.btnChallenge.Name = "btnChallenge";
            this.btnChallenge.Size = new System.Drawing.Size(170, 23);
            this.btnChallenge.TabIndex = 6;
            this.btnChallenge.Text = "Vyzvat ke hře";
            this.btnChallenge.UseVisualStyleBackColor = true;
            this.btnChallenge.Click += new System.EventHandler(this.btnChallenge_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(409, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Hráči:";
            // 
            // lblPlayers
            // 
            this.lblPlayers.AutoSize = true;
            this.lblPlayers.Location = new System.Drawing.Point(451, 22);
            this.lblPlayers.Name = "lblPlayers";
            this.lblPlayers.Size = new System.Drawing.Size(36, 13);
            this.lblPlayers.TabIndex = 8;
            this.lblPlayers.Text = "3 / 20";
            // 
            // LobbyForm
            // 
            this.AcceptButton = this.btnSend;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(621, 450);
            this.Controls.Add(this.lblPlayers);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnChallenge);
            this.Controls.Add(this.listPlayers);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.rtbChat);
            this.Controls.Add(this.lblServerIp);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "LobbyForm";
            this.Text = "Battleships - Lobby";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.LobbyForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblServerIp;
        private System.Windows.Forms.RichTextBox rtbChat;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.ListBox listPlayers;
        private System.Windows.Forms.Button btnChallenge;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblPlayers;
    }
}