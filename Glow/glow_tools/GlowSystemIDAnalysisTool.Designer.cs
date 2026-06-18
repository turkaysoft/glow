namespace Glow.glow_tools
{
    partial class GlowSystemIDAnalysisTool
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
            this.Label_Info = new Glow.TSCustomLabel();
            this.DGV_MainTable = new System.Windows.Forms.DataGridView();
            this.Btn_Compare = new Glow.TSCustomButton();
            this.Btn_Save = new Glow.TSCustomButton();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_MainTable)).BeginInit();
            this.SuspendLayout();
            // 
            // Label_Info
            // 
            this.Label_Info.BackColor = System.Drawing.Color.White;
            this.Label_Info.BorderRadius = 5;
            this.Label_Info.Dock = System.Windows.Forms.DockStyle.Top;
            this.Label_Info.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold);
            this.Label_Info.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Label_Info.Location = new System.Drawing.Point(10, 10);
            this.Label_Info.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.Label_Info.Name = "Label_Info";
            this.Label_Info.Padding = new System.Windows.Forms.Padding(10);
            this.Label_Info.Size = new System.Drawing.Size(864, 140);
            this.Label_Info.TabIndex = 0;
            this.Label_Info.Text = "N/A Text";
            this.Label_Info.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DGV_MainTable
            // 
            this.DGV_MainTable.AllowUserToAddRows = false;
            this.DGV_MainTable.AllowUserToDeleteRows = false;
            this.DGV_MainTable.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(237)))));
            this.DGV_MainTable.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.DGV_MainTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DGV_MainTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DGV_MainTable.BackgroundColor = System.Drawing.Color.White;
            this.DGV_MainTable.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DGV_MainTable.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(87)))), ((int)(((byte)(160)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(87)))), ((int)(((byte)(160)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.DGV_MainTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.DGV_MainTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV_MainTable.Cursor = System.Windows.Forms.Cursors.Default;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(87)))), ((int)(((byte)(160)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.DGV_MainTable.DefaultCellStyle = dataGridViewCellStyle3;
            this.DGV_MainTable.EnableHeadersVisualStyles = false;
            this.DGV_MainTable.GridColor = System.Drawing.Color.Gray;
            this.DGV_MainTable.Location = new System.Drawing.Point(10, 158);
            this.DGV_MainTable.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.DGV_MainTable.MultiSelect = false;
            this.DGV_MainTable.Name = "DGV_MainTable";
            this.DGV_MainTable.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(87)))), ((int)(((byte)(160)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DGV_MainTable.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.DGV_MainTable.RowHeadersVisible = false;
            this.DGV_MainTable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.DGV_MainTable.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DGV_MainTable.Size = new System.Drawing.Size(864, 250);
            this.DGV_MainTable.TabIndex = 1;
            this.DGV_MainTable.SelectionChanged += new System.EventHandler(this.DGV_MainTable_SelectionChanged);
            // 
            // Btn_Compare
            // 
            this.Btn_Compare.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Btn_Compare.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.Btn_Compare.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.Btn_Compare.BorderColor = System.Drawing.Color.DodgerBlue;
            this.Btn_Compare.BorderRadius = 10;
            this.Btn_Compare.BorderSize = 0;
            this.Btn_Compare.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Btn_Compare.Enabled = false;
            this.Btn_Compare.FlatAppearance.BorderSize = 0;
            this.Btn_Compare.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_Compare.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold);
            this.Btn_Compare.ForeColor = System.Drawing.Color.White;
            this.Btn_Compare.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Btn_Compare.Location = new System.Drawing.Point(10, 462);
            this.Btn_Compare.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.Btn_Compare.Name = "Btn_Compare";
            this.Btn_Compare.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.Btn_Compare.Size = new System.Drawing.Size(864, 36);
            this.Btn_Compare.TabIndex = 3;
            this.Btn_Compare.Text = "Karşılaştır";
            this.Btn_Compare.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Btn_Compare.TextColor = System.Drawing.Color.White;
            this.Btn_Compare.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Btn_Compare.UseVisualStyleBackColor = false;
            this.Btn_Compare.Click += new System.EventHandler(this.Btn_Compare_Click);
            // 
            // Btn_Save
            // 
            this.Btn_Save.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Btn_Save.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.Btn_Save.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(95)))), ((int)(((byte)(146)))));
            this.Btn_Save.BorderColor = System.Drawing.Color.DodgerBlue;
            this.Btn_Save.BorderRadius = 10;
            this.Btn_Save.BorderSize = 0;
            this.Btn_Save.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Btn_Save.FlatAppearance.BorderSize = 0;
            this.Btn_Save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_Save.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold);
            this.Btn_Save.ForeColor = System.Drawing.Color.White;
            this.Btn_Save.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Btn_Save.Location = new System.Drawing.Point(10, 418);
            this.Btn_Save.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.Btn_Save.Name = "Btn_Save";
            this.Btn_Save.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.Btn_Save.Size = new System.Drawing.Size(864, 36);
            this.Btn_Save.TabIndex = 2;
            this.Btn_Save.Text = "Rapor Oluştur";
            this.Btn_Save.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Btn_Save.TextColor = System.Drawing.Color.White;
            this.Btn_Save.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Btn_Save.UseVisualStyleBackColor = false;
            this.Btn_Save.Click += new System.EventHandler(this.Btn_Save_Click);
            // 
            // GlowSystemIDAnalysisTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(884, 511);
            this.Controls.Add(this.Btn_Compare);
            this.Controls.Add(this.DGV_MainTable);
            this.Controls.Add(this.Btn_Save);
            this.Controls.Add(this.Label_Info);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::Glow.Properties.Resources.GlowLogo;
            this.MaximizeBox = false;
            this.Name = "GlowSystemIDAnalysisTool";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GlowSystemIDGenerator";
            this.Load += new System.EventHandler(this.GlowSystemIDAnalysisTool_Load);
            ((System.ComponentModel.ISupportInitialize)(this.DGV_MainTable)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TSCustomButton Btn_Save;
        internal TSCustomLabel Label_Info;
        private System.Windows.Forms.DataGridView DGV_MainTable;
        private TSCustomButton Btn_Compare;
    }
}