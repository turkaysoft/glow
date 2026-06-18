namespace Glow
{
    partial class GlowAbout
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.PanelTxt = new System.Windows.Forms.Panel();
            this.AboutTable = new System.Windows.Forms.DataGridView();
            this.LabelCopyright = new System.Windows.Forms.Label();
            this.LabelVersion = new System.Windows.Forms.Label();
            this.LabelDeveloper = new System.Windows.Forms.Label();
            this.LabelSoftware = new System.Windows.Forms.Label();
            this.PanelImg = new System.Windows.Forms.Panel();
            this.PanelHeader = new System.Windows.Forms.Panel();
            this.CloseAboutBtn = new System.Windows.Forms.Button();
            this.ImageAbout = new System.Windows.Forms.PictureBox();
            this.About_DonateBtn = new Glow.TSCustomButton();
            this.About_GitHubBtn = new Glow.TSCustomButton();
            this.About_WebsiteBtn = new Glow.TSCustomButton();
            this.PanelTxt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AboutTable)).BeginInit();
            this.PanelImg.SuspendLayout();
            this.PanelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImageAbout)).BeginInit();
            this.SuspendLayout();
            // 
            // PanelTxt
            // 
            this.PanelTxt.BackColor = System.Drawing.Color.White;
            this.PanelTxt.Controls.Add(this.About_DonateBtn);
            this.PanelTxt.Controls.Add(this.About_GitHubBtn);
            this.PanelTxt.Controls.Add(this.About_WebsiteBtn);
            this.PanelTxt.Controls.Add(this.AboutTable);
            this.PanelTxt.Controls.Add(this.LabelCopyright);
            this.PanelTxt.Controls.Add(this.LabelVersion);
            this.PanelTxt.Controls.Add(this.LabelDeveloper);
            this.PanelTxt.Controls.Add(this.LabelSoftware);
            this.PanelTxt.Dock = System.Windows.Forms.DockStyle.Left;
            this.PanelTxt.Location = new System.Drawing.Point(0, 0);
            this.PanelTxt.Name = "PanelTxt";
            this.PanelTxt.Padding = new System.Windows.Forms.Padding(15);
            this.PanelTxt.Size = new System.Drawing.Size(365, 450);
            this.PanelTxt.TabIndex = 0;
            // 
            // AboutTable
            // 
            this.AboutTable.AllowUserToAddRows = false;
            this.AboutTable.AllowUserToDeleteRows = false;
            this.AboutTable.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(237)))));
            this.AboutTable.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.AboutTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.AboutTable.BackgroundColor = System.Drawing.Color.WhiteSmoke;
            this.AboutTable.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.AboutTable.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(87)))), ((int)(((byte)(160)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(87)))), ((int)(((byte)(160)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.AboutTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.AboutTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.AboutTable.Cursor = System.Windows.Forms.Cursors.Default;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(87)))), ((int)(((byte)(160)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.AboutTable.DefaultCellStyle = dataGridViewCellStyle3;
            this.AboutTable.EnableHeadersVisualStyles = false;
            this.AboutTable.GridColor = System.Drawing.Color.Gray;
            this.AboutTable.Location = new System.Drawing.Point(15, 238);
            this.AboutTable.MultiSelect = false;
            this.AboutTable.Name = "AboutTable";
            this.AboutTable.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(87)))), ((int)(((byte)(160)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.AboutTable.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.AboutTable.RowHeadersVisible = false;
            this.AboutTable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.AboutTable.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.AboutTable.Size = new System.Drawing.Size(335, 144);
            this.AboutTable.TabIndex = 6;
            this.AboutTable.SelectionChanged += new System.EventHandler(this.AboutTable_SelectionChanged);
            // 
            // LabelCopyright
            // 
            this.LabelCopyright.BackColor = System.Drawing.Color.Transparent;
            this.LabelCopyright.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.LabelCopyright.Font = new System.Drawing.Font("Segoe UI Semibold", 10.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.LabelCopyright.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(110)))), ((int)(((byte)(110)))));
            this.LabelCopyright.Location = new System.Drawing.Point(15, 410);
            this.LabelCopyright.Margin = new System.Windows.Forms.Padding(0, 25, 0, 0);
            this.LabelCopyright.Name = "LabelCopyright";
            this.LabelCopyright.Size = new System.Drawing.Size(335, 25);
            this.LabelCopyright.TabIndex = 7;
            this.LabelCopyright.Text = "© Copyright Text.";
            this.LabelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LabelVersion
            // 
            this.LabelVersion.AutoSize = true;
            this.LabelVersion.BackColor = System.Drawing.Color.Transparent;
            this.LabelVersion.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.LabelVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.LabelVersion.Location = new System.Drawing.Point(17, 87);
            this.LabelVersion.Margin = new System.Windows.Forms.Padding(0, 5, 0, 25);
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
            this.LabelDeveloper.Size = new System.Drawing.Size(335, 35);
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
            this.LabelSoftware.Size = new System.Drawing.Size(335, 35);
            this.LabelSoftware.TabIndex = 1;
            this.LabelSoftware.Text = "About";
            // 
            // PanelImg
            // 
            this.PanelImg.BackColor = System.Drawing.Color.Transparent;
            this.PanelImg.Controls.Add(this.PanelHeader);
            this.PanelImg.Controls.Add(this.ImageAbout);
            this.PanelImg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelImg.Location = new System.Drawing.Point(365, 0);
            this.PanelImg.Name = "PanelImg";
            this.PanelImg.Size = new System.Drawing.Size(410, 450);
            this.PanelImg.TabIndex = 1;
            // 
            // PanelHeader
            // 
            this.PanelHeader.Controls.Add(this.CloseAboutBtn);
            this.PanelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.PanelHeader.Location = new System.Drawing.Point(0, 0);
            this.PanelHeader.Name = "PanelHeader";
            this.PanelHeader.Size = new System.Drawing.Size(410, 30);
            this.PanelHeader.TabIndex = 0;
            this.PanelHeader.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelHeader_MouseDown);
            this.PanelHeader.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PanelHeader_MouseMove);
            this.PanelHeader.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PanelHeader_MouseUp);
            // 
            // CloseAboutBtn
            // 
            this.CloseAboutBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.CloseAboutBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.CloseAboutBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CloseAboutBtn.Dock = System.Windows.Forms.DockStyle.Right;
            this.CloseAboutBtn.FlatAppearance.BorderSize = 0;
            this.CloseAboutBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CloseAboutBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CloseAboutBtn.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.CloseAboutBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CloseAboutBtn.Location = new System.Drawing.Point(380, 0);
            this.CloseAboutBtn.Name = "CloseAboutBtn";
            this.CloseAboutBtn.Size = new System.Drawing.Size(30, 30);
            this.CloseAboutBtn.TabIndex = 0;
            this.CloseAboutBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.CloseAboutBtn.UseVisualStyleBackColor = false;
            this.CloseAboutBtn.Click += new System.EventHandler(this.CloseAboutBtn_Click);
            // 
            // ImageAbout
            // 
            this.ImageAbout.BackColor = System.Drawing.Color.Transparent;
            this.ImageAbout.BackgroundImage = global::Glow.Properties.Resources.ts_about;
            this.ImageAbout.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ImageAbout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ImageAbout.Location = new System.Drawing.Point(0, 0);
            this.ImageAbout.Name = "ImageAbout";
            this.ImageAbout.Size = new System.Drawing.Size(410, 450);
            this.ImageAbout.TabIndex = 0;
            this.ImageAbout.TabStop = false;
            // 
            // About_DonateBtn
            // 
            this.About_DonateBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.About_DonateBtn.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.About_DonateBtn.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.About_DonateBtn.BorderRadius = 10;
            this.About_DonateBtn.BorderSize = 0;
            this.About_DonateBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.About_DonateBtn.FlatAppearance.BorderSize = 0;
            this.About_DonateBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.About_DonateBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold);
            this.About_DonateBtn.ForeColor = System.Drawing.Color.White;
            this.About_DonateBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.About_DonateBtn.Location = new System.Drawing.Point(15, 174);
            this.About_DonateBtn.Margin = new System.Windows.Forms.Padding(3, 1, 1, 25);
            this.About_DonateBtn.Name = "About_DonateBtn";
            this.About_DonateBtn.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.About_DonateBtn.Size = new System.Drawing.Size(335, 36);
            this.About_DonateBtn.TabIndex = 5;
            this.About_DonateBtn.Text = "Link / Donate";
            this.About_DonateBtn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.About_DonateBtn.TextColor = System.Drawing.Color.White;
            this.About_DonateBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.About_DonateBtn.UseVisualStyleBackColor = false;
            this.About_DonateBtn.Click += new System.EventHandler(this.About_DonateBtn_Click);
            // 
            // About_GitHubBtn
            // 
            this.About_GitHubBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.About_GitHubBtn.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.About_GitHubBtn.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.About_GitHubBtn.BorderRadius = 10;
            this.About_GitHubBtn.BorderSize = 0;
            this.About_GitHubBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.About_GitHubBtn.FlatAppearance.BorderSize = 0;
            this.About_GitHubBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.About_GitHubBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold);
            this.About_GitHubBtn.ForeColor = System.Drawing.Color.White;
            this.About_GitHubBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.About_GitHubBtn.Location = new System.Drawing.Point(184, 136);
            this.About_GitHubBtn.Margin = new System.Windows.Forms.Padding(1, 3, 3, 1);
            this.About_GitHubBtn.Name = "About_GitHubBtn";
            this.About_GitHubBtn.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.About_GitHubBtn.Size = new System.Drawing.Size(166, 36);
            this.About_GitHubBtn.TabIndex = 4;
            this.About_GitHubBtn.Text = "Link / GitHub";
            this.About_GitHubBtn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.About_GitHubBtn.TextColor = System.Drawing.Color.White;
            this.About_GitHubBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.About_GitHubBtn.UseVisualStyleBackColor = false;
            this.About_GitHubBtn.Click += new System.EventHandler(this.About_GitHubBtn_Click);
            // 
            // About_WebsiteBtn
            // 
            this.About_WebsiteBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.About_WebsiteBtn.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.About_WebsiteBtn.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.About_WebsiteBtn.BorderRadius = 10;
            this.About_WebsiteBtn.BorderSize = 0;
            this.About_WebsiteBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.About_WebsiteBtn.FlatAppearance.BorderSize = 0;
            this.About_WebsiteBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.About_WebsiteBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold);
            this.About_WebsiteBtn.ForeColor = System.Drawing.Color.White;
            this.About_WebsiteBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.About_WebsiteBtn.Location = new System.Drawing.Point(15, 136);
            this.About_WebsiteBtn.Margin = new System.Windows.Forms.Padding(3, 3, 1, 1);
            this.About_WebsiteBtn.Name = "About_WebsiteBtn";
            this.About_WebsiteBtn.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.About_WebsiteBtn.Size = new System.Drawing.Size(166, 36);
            this.About_WebsiteBtn.TabIndex = 3;
            this.About_WebsiteBtn.Text = "Link / Website";
            this.About_WebsiteBtn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.About_WebsiteBtn.TextColor = System.Drawing.Color.White;
            this.About_WebsiteBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.About_WebsiteBtn.UseVisualStyleBackColor = false;
            this.About_WebsiteBtn.Click += new System.EventHandler(this.About_WebsiteBtn_Click);
            // 
            // GlowAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(775, 450);
            this.Controls.Add(this.PanelImg);
            this.Controls.Add(this.PanelTxt);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = global::Glow.Properties.Resources.GlowLogo;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GlowAbout";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GlowAbout";
            this.Load += new System.EventHandler(this.GlowAbout_Load);
            this.PanelTxt.ResumeLayout(false);
            this.PanelTxt.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AboutTable)).EndInit();
            this.PanelImg.ResumeLayout(false);
            this.PanelHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ImageAbout)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PanelTxt;
        private System.Windows.Forms.Label LabelVersion;
        private System.Windows.Forms.Label LabelDeveloper;
        private System.Windows.Forms.Label LabelSoftware;
        private System.Windows.Forms.Panel PanelImg;
        private System.Windows.Forms.PictureBox ImageAbout;
        private System.Windows.Forms.Label LabelCopyright;
        private System.Windows.Forms.Button CloseAboutBtn;
        private System.Windows.Forms.Panel PanelHeader;
        private System.Windows.Forms.DataGridView AboutTable;
        private TSCustomButton About_DonateBtn;
        private TSCustomButton About_GitHubBtn;
        private TSCustomButton About_WebsiteBtn;
    }
}