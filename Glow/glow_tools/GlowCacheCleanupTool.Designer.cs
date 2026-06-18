namespace Glow.glow_tools
{
    partial class GlowCacheCleanupTool
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
            this.BG_Panel = new System.Windows.Forms.Panel();
            this.CCTTable = new System.Windows.Forms.DataGridView();
            this.CCT_SelectLabel = new Glow.TSCustomLabel();
            this.CCT_StartBtn = new Glow.TSCustomButton();
            this.BG_Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CCTTable)).BeginInit();
            this.SuspendLayout();
            // 
            // BG_Panel
            // 
            this.BG_Panel.BackColor = System.Drawing.Color.Transparent;
            this.BG_Panel.Controls.Add(this.CCT_SelectLabel);
            this.BG_Panel.Controls.Add(this.CCTTable);
            this.BG_Panel.Controls.Add(this.CCT_StartBtn);
            this.BG_Panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BG_Panel.Location = new System.Drawing.Point(0, 0);
            this.BG_Panel.Name = "BG_Panel";
            this.BG_Panel.Padding = new System.Windows.Forms.Padding(10);
            this.BG_Panel.Size = new System.Drawing.Size(759, 357);
            this.BG_Panel.TabIndex = 0;
            // 
            // CCTTable
            // 
            this.CCTTable.AllowUserToAddRows = false;
            this.CCTTable.AllowUserToDeleteRows = false;
            this.CCTTable.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(237)))));
            this.CCTTable.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.CCTTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CCTTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.CCTTable.BackgroundColor = System.Drawing.Color.White;
            this.CCTTable.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.CCTTable.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(87)))), ((int)(((byte)(160)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(87)))), ((int)(((byte)(160)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.CCTTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.CCTTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.CCTTable.Cursor = System.Windows.Forms.Cursors.Hand;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(87)))), ((int)(((byte)(160)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.CCTTable.DefaultCellStyle = dataGridViewCellStyle3;
            this.CCTTable.EnableHeadersVisualStyles = false;
            this.CCTTable.GridColor = System.Drawing.Color.Gray;
            this.CCTTable.Location = new System.Drawing.Point(10, 10);
            this.CCTTable.Margin = new System.Windows.Forms.Padding(3, 3, 3, 20);
            this.CCTTable.MultiSelect = false;
            this.CCTTable.Name = "CCTTable";
            this.CCTTable.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(87)))), ((int)(((byte)(160)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.CCTTable.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.CCTTable.RowHeadersVisible = false;
            this.CCTTable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.CCTTable.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.CCTTable.Size = new System.Drawing.Size(739, 225);
            this.CCTTable.TabIndex = 0;
            this.CCTTable.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.CCTTable_CellClick);
            // 
            // CCT_SelectLabel
            // 
            this.CCT_SelectLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CCT_SelectLabel.BackColor = System.Drawing.Color.White;
            this.CCT_SelectLabel.BorderRadius = 5;
            this.CCT_SelectLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.CCT_SelectLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CCT_SelectLabel.Location = new System.Drawing.Point(10, 255);
            this.CCT_SelectLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 16);
            this.CCT_SelectLabel.Name = "CCT_SelectLabel";
            this.CCT_SelectLabel.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.CCT_SelectLabel.Size = new System.Drawing.Size(739, 39);
            this.CCT_SelectLabel.TabIndex = 1;
            this.CCT_SelectLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CCT_StartBtn
            // 
            this.CCT_StartBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.CCT_StartBtn.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.CCT_StartBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.CCT_StartBtn.BorderColor = System.Drawing.Color.DodgerBlue;
            this.CCT_StartBtn.BorderRadius = 10;
            this.CCT_StartBtn.BorderSize = 0;
            this.CCT_StartBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CCT_StartBtn.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.CCT_StartBtn.FlatAppearance.BorderSize = 0;
            this.CCT_StartBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CCT_StartBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.CCT_StartBtn.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.CCT_StartBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CCT_StartBtn.Location = new System.Drawing.Point(10, 311);
            this.CCT_StartBtn.Margin = new System.Windows.Forms.Padding(1);
            this.CCT_StartBtn.Name = "CCT_StartBtn";
            this.CCT_StartBtn.Size = new System.Drawing.Size(739, 36);
            this.CCT_StartBtn.TabIndex = 2;
            this.CCT_StartBtn.Text = "TEMİZLE";
            this.CCT_StartBtn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CCT_StartBtn.TextColor = System.Drawing.Color.WhiteSmoke;
            this.CCT_StartBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.CCT_StartBtn.UseVisualStyleBackColor = false;
            this.CCT_StartBtn.Click += new System.EventHandler(this.CCT_StartBtn_Click);
            // 
            // GlowCacheCleanupTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(759, 357);
            this.Controls.Add(this.BG_Panel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::Glow.Properties.Resources.GlowLogo;
            this.MaximizeBox = false;
            this.Name = "GlowCacheCleanupTool";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cache Cleanup Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GlowCacheCleanupTool_FormClosing);
            this.Load += new System.EventHandler(this.GlowCacheCleanupTool_Load);
            this.BG_Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CCTTable)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel BG_Panel;
        private TSCustomButton CCT_StartBtn;
        private System.Windows.Forms.DataGridView CCTTable;
        internal TSCustomLabel CCT_SelectLabel;
    }
}