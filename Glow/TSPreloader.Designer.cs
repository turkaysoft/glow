namespace Glow
{
    partial class TSPreloader
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
            this.PanelTxt = new System.Windows.Forms.Panel();
            this.LabelVersion = new System.Windows.Forms.Label();
            this.LabelDeveloper = new System.Windows.Forms.Label();
            this.LabelSoftware = new System.Windows.Forms.Label();
            this.LabelLoader = new System.Windows.Forms.Label();
            this.PanelLoaderBG = new Glow.TSCustomPanel();
            this.PanelLoaderFE = new Glow.TSCustomPanel();
            this.LabelCopyright = new System.Windows.Forms.Label();
            this.PanelImg = new System.Windows.Forms.Panel();
            this.ImageWelcome = new System.Windows.Forms.PictureBox();
            this.PanelTxt.SuspendLayout();
            this.PanelLoaderBG.SuspendLayout();
            this.PanelImg.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImageWelcome)).BeginInit();
            this.SuspendLayout();
            // 
            // PanelTxt
            // 
            this.PanelTxt.BackColor = System.Drawing.Color.White;
            this.PanelTxt.Controls.Add(this.LabelVersion);
            this.PanelTxt.Controls.Add(this.LabelDeveloper);
            this.PanelTxt.Controls.Add(this.LabelSoftware);
            this.PanelTxt.Controls.Add(this.LabelLoader);
            this.PanelTxt.Controls.Add(this.PanelLoaderBG);
            this.PanelTxt.Controls.Add(this.LabelCopyright);
            this.PanelTxt.Dock = System.Windows.Forms.DockStyle.Left;
            this.PanelTxt.Location = new System.Drawing.Point(0, 0);
            this.PanelTxt.Name = "PanelTxt";
            this.PanelTxt.Padding = new System.Windows.Forms.Padding(15);
            this.PanelTxt.Size = new System.Drawing.Size(365, 450);
            this.PanelTxt.TabIndex = 0;
            // 
            // LabelVersion
            // 
            this.LabelVersion.AutoSize = true;
            this.LabelVersion.BackColor = System.Drawing.Color.Transparent;
            this.LabelVersion.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.LabelVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.LabelVersion.Location = new System.Drawing.Point(17, 87);
            this.LabelVersion.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.LabelVersion.Name = "LabelVersion";
            this.LabelVersion.Size = new System.Drawing.Size(62, 21);
            this.LabelVersion.TabIndex = 2;
            this.LabelVersion.Text = "vXX.XX";
            this.LabelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelDeveloper
            // 
            this.LabelDeveloper.BackColor = System.Drawing.Color.Transparent;
            this.LabelDeveloper.Font = new System.Drawing.Font("Segoe UI Black", 17F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.LabelDeveloper.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.LabelDeveloper.Location = new System.Drawing.Point(15, 9);
            this.LabelDeveloper.Margin = new System.Windows.Forms.Padding(0);
            this.LabelDeveloper.Name = "LabelDeveloper";
            this.LabelDeveloper.Size = new System.Drawing.Size(326, 35);
            this.LabelDeveloper.TabIndex = 0;
            this.LabelDeveloper.Text = "Türkaysoft";
            this.LabelDeveloper.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // LabelSoftware
            // 
            this.LabelSoftware.BackColor = System.Drawing.Color.Transparent;
            this.LabelSoftware.Font = new System.Drawing.Font("Segoe UI Black", 17F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.LabelSoftware.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.LabelSoftware.Location = new System.Drawing.Point(15, 42);
            this.LabelSoftware.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.LabelSoftware.Name = "LabelSoftware";
            this.LabelSoftware.Size = new System.Drawing.Size(326, 35);
            this.LabelSoftware.TabIndex = 1;
            this.LabelSoftware.Text = "Preloader";
            // 
            // LabelLoader
            // 
            this.LabelLoader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LabelLoader.AutoSize = true;
            this.LabelLoader.BackColor = System.Drawing.Color.Transparent;
            this.LabelLoader.Font = new System.Drawing.Font("Segoe UI Semibold", 11.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.LabelLoader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.LabelLoader.Location = new System.Drawing.Point(17, 336);
            this.LabelLoader.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.LabelLoader.Name = "LabelLoader";
            this.LabelLoader.Size = new System.Drawing.Size(58, 21);
            this.LabelLoader.TabIndex = 3;
            this.LabelLoader.Text = "Load...";
            // 
            // PanelLoaderBG
            // 
            this.PanelLoaderBG.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelLoaderBG.BackColor = System.Drawing.Color.Transparent;
            this.PanelLoaderBG.BorderColor = System.Drawing.Color.DodgerBlue;
            this.PanelLoaderBG.BorderRadius = 3;
            this.PanelLoaderBG.BorderSize = 0;
            this.PanelLoaderBG.Controls.Add(this.PanelLoaderFE);
            this.PanelLoaderBG.Location = new System.Drawing.Point(21, 372);
            this.PanelLoaderBG.Margin = new System.Windows.Forms.Padding(3, 15, 3, 32);
            this.PanelLoaderBG.Name = "PanelLoaderBG";
            this.PanelLoaderBG.Size = new System.Drawing.Size(320, 6);
            this.PanelLoaderBG.TabIndex = 4;
            // 
            // PanelLoaderFE
            // 
            this.PanelLoaderFE.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.PanelLoaderFE.BorderColor = System.Drawing.Color.DodgerBlue;
            this.PanelLoaderFE.BorderRadius = 3;
            this.PanelLoaderFE.BorderSize = 0;
            this.PanelLoaderFE.Dock = System.Windows.Forms.DockStyle.Left;
            this.PanelLoaderFE.Location = new System.Drawing.Point(0, 0);
            this.PanelLoaderFE.Name = "PanelLoaderFE";
            this.PanelLoaderFE.Size = new System.Drawing.Size(50, 6);
            this.PanelLoaderFE.TabIndex = 0;
            // 
            // LabelCopyright
            // 
            this.LabelCopyright.BackColor = System.Drawing.Color.Transparent;
            this.LabelCopyright.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.LabelCopyright.Font = new System.Drawing.Font("Segoe UI Semibold", 10.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.LabelCopyright.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(110)))), ((int)(((byte)(110)))));
            this.LabelCopyright.Location = new System.Drawing.Point(15, 410);
            this.LabelCopyright.Margin = new System.Windows.Forms.Padding(0);
            this.LabelCopyright.Name = "LabelCopyright";
            this.LabelCopyright.Size = new System.Drawing.Size(335, 25);
            this.LabelCopyright.TabIndex = 5;
            this.LabelCopyright.Text = "© Copyright Text.";
            this.LabelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PanelImg
            // 
            this.PanelImg.BackColor = System.Drawing.Color.Transparent;
            this.PanelImg.Controls.Add(this.ImageWelcome);
            this.PanelImg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelImg.Location = new System.Drawing.Point(365, 0);
            this.PanelImg.Name = "PanelImg";
            this.PanelImg.Size = new System.Drawing.Size(410, 450);
            this.PanelImg.TabIndex = 1;
            // 
            // ImageWelcome
            // 
            this.ImageWelcome.BackColor = System.Drawing.Color.Transparent;
            this.ImageWelcome.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ImageWelcome.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ImageWelcome.Location = new System.Drawing.Point(0, 0);
            this.ImageWelcome.Name = "ImageWelcome";
            this.ImageWelcome.Size = new System.Drawing.Size(410, 450);
            this.ImageWelcome.TabIndex = 0;
            this.ImageWelcome.TabStop = false;
            // 
            // TSPreloader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(775, 450);
            this.Controls.Add(this.PanelImg);
            this.Controls.Add(this.PanelTxt);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = global::Glow.Properties.Resources.GlowLogo;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TSPreloader";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TSPreloader";
            this.Load += new System.EventHandler(this.TSPreloader_Load);
            this.PanelTxt.ResumeLayout(false);
            this.PanelTxt.PerformLayout();
            this.PanelLoaderBG.ResumeLayout(false);
            this.PanelImg.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ImageWelcome)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PanelTxt;
        private System.Windows.Forms.Label LabelVersion;
        private System.Windows.Forms.Label LabelDeveloper;
        private System.Windows.Forms.Label LabelSoftware;
        private System.Windows.Forms.Label LabelLoader;
        private TSCustomPanel PanelLoaderBG;
        private TSCustomPanel PanelLoaderFE;
        private System.Windows.Forms.Label LabelCopyright;
        private System.Windows.Forms.Panel PanelImg;
        private System.Windows.Forms.PictureBox ImageWelcome;
    }
}