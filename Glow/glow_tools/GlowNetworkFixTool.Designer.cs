namespace Glow.glow_tools
{
    partial class GlowNetworkFixTool
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
            this.Panel_BG = new System.Windows.Forms.Panel();
            this.NFT_StartBtn = new Glow.TSCustomButton();
            this.NFT_ResultList = new Glow.TSCustomListBox();
            this.NFT_TitleLabel = new Glow.TSCustomLabel();
            this.Panel_BG.SuspendLayout();
            this.SuspendLayout();
            // 
            // Panel_BG
            // 
            this.Panel_BG.Controls.Add(this.NFT_StartBtn);
            this.Panel_BG.Controls.Add(this.NFT_ResultList);
            this.Panel_BG.Controls.Add(this.NFT_TitleLabel);
            this.Panel_BG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panel_BG.Location = new System.Drawing.Point(0, 0);
            this.Panel_BG.Name = "Panel_BG";
            this.Panel_BG.Padding = new System.Windows.Forms.Padding(10);
            this.Panel_BG.Size = new System.Drawing.Size(534, 327);
            this.Panel_BG.TabIndex = 0;
            // 
            // NFT_StartBtn
            // 
            this.NFT_StartBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.NFT_StartBtn.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.NFT_StartBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.NFT_StartBtn.BorderColor = System.Drawing.Color.DodgerBlue;
            this.NFT_StartBtn.BorderRadius = 10;
            this.NFT_StartBtn.BorderSize = 0;
            this.NFT_StartBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.NFT_StartBtn.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.NFT_StartBtn.FlatAppearance.BorderSize = 0;
            this.NFT_StartBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NFT_StartBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.NFT_StartBtn.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.NFT_StartBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.NFT_StartBtn.Location = new System.Drawing.Point(10, 281);
            this.NFT_StartBtn.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.NFT_StartBtn.Name = "NFT_StartBtn";
            this.NFT_StartBtn.Size = new System.Drawing.Size(514, 36);
            this.NFT_StartBtn.TabIndex = 2;
            this.NFT_StartBtn.Text = "Onarımı Başlat";
            this.NFT_StartBtn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.NFT_StartBtn.TextColor = System.Drawing.Color.WhiteSmoke;
            this.NFT_StartBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.NFT_StartBtn.UseVisualStyleBackColor = false;
            this.NFT_StartBtn.Click += new System.EventHandler(this.NFT_StartBtn_Click);
            // 
            // NFT_ResultList
            // 
            this.NFT_ResultList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NFT_ResultList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NFT_ResultList.Cursor = System.Windows.Forms.Cursors.Default;
            this.NFT_ResultList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.NFT_ResultList.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.NFT_ResultList.FormattingEnabled = true;
            this.NFT_ResultList.IntegralHeight = false;
            this.NFT_ResultList.ItemHeight = 23;
            this.NFT_ResultList.Location = new System.Drawing.Point(10, 65);
            this.NFT_ResultList.Margin = new System.Windows.Forms.Padding(3, 0, 3, 20);
            this.NFT_ResultList.Name = "NFT_ResultList";
            this.NFT_ResultList.SelectedBackColor = System.Drawing.Color.DodgerBlue;
            this.NFT_ResultList.SelectedForeColor = System.Drawing.Color.White;
            this.NFT_ResultList.Size = new System.Drawing.Size(514, 196);
            this.NFT_ResultList.TabIndex = 1;
            this.NFT_ResultList.SelectedIndexChanged += new System.EventHandler(this.NFT_ResultList_SelectedIndexChanged);
            // 
            // NFT_TitleLabel
            // 
            this.NFT_TitleLabel.BackColor = System.Drawing.Color.White;
            this.NFT_TitleLabel.BorderRadius = 5;
            this.NFT_TitleLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.NFT_TitleLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 10.75F, System.Drawing.FontStyle.Bold);
            this.NFT_TitleLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.NFT_TitleLabel.Location = new System.Drawing.Point(10, 10);
            this.NFT_TitleLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 20);
            this.NFT_TitleLabel.Name = "NFT_TitleLabel";
            this.NFT_TitleLabel.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.NFT_TitleLabel.Size = new System.Drawing.Size(514, 35);
            this.NFT_TitleLabel.TabIndex = 0;
            this.NFT_TitleLabel.Text = "Ağ Onarım Aracı";
            this.NFT_TitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // GlowNetworkFixTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(534, 327);
            this.Controls.Add(this.Panel_BG);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::Glow.Properties.Resources.GlowLogo;
            this.MaximizeBox = false;
            this.Name = "GlowNetworkFixTool";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GlowNetworkFixTool";
            this.Load += new System.EventHandler(this.GlowNetworkFixTool_Load);
            this.Panel_BG.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel Panel_BG;
        internal TSCustomLabel NFT_TitleLabel;
        private TSCustomButton NFT_StartBtn;
        private TSCustomListBox NFT_ResultList;
    }
}