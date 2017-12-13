namespace RealtimeSpreadMonitor.Forms
{
    partial class FCM_ReportSummaryForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FCM_ReportSummaryForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lblLatestADMFileDate = new System.Windows.Forms.Label();
            this.fcm_InputSummaryGrid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fcm_InputSummaryGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lblLatestADMFileDate);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.fcm_InputSummaryGrid);
            this.splitContainer1.Size = new System.Drawing.Size(801, 545);
            this.splitContainer1.SplitterDistance = 29;
            this.splitContainer1.TabIndex = 0;
            // 
            // lblLatestADMFileDate
            // 
            this.lblLatestADMFileDate.AutoSize = true;
            this.lblLatestADMFileDate.Location = new System.Drawing.Point(12, 9);
            this.lblLatestADMFileDate.Name = "lblLatestADMFileDate";
            this.lblLatestADMFileDate.Size = new System.Drawing.Size(109, 13);
            this.lblLatestADMFileDate.TabIndex = 0;
            this.lblLatestADMFileDate.Text = "lblLatestADMFileDate";
            // 
            // fcm_InputSummaryGrid
            // 
            this.fcm_InputSummaryGrid.AllowUserToAddRows = false;
            this.fcm_InputSummaryGrid.AllowUserToDeleteRows = false;
            this.fcm_InputSummaryGrid.AllowUserToResizeRows = false;
            this.fcm_InputSummaryGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.fcm_InputSummaryGrid.BackgroundColor = System.Drawing.Color.White;
            this.fcm_InputSummaryGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.fcm_InputSummaryGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.fcm_InputSummaryGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.fcm_InputSummaryGrid.DefaultCellStyle = dataGridViewCellStyle2;
            this.fcm_InputSummaryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fcm_InputSummaryGrid.Location = new System.Drawing.Point(0, 0);
            this.fcm_InputSummaryGrid.Name = "fcm_InputSummaryGrid";
            this.fcm_InputSummaryGrid.ReadOnly = true;
            this.fcm_InputSummaryGrid.RowHeadersVisible = false;
            this.fcm_InputSummaryGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            this.fcm_InputSummaryGrid.Size = new System.Drawing.Size(801, 512);
            this.fcm_InputSummaryGrid.TabIndex = 7;
            // 
            // FCM_ReportSummaryForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(801, 545);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FCM_ReportSummaryForm";
            this.Text = "AdmReportSummaryForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AdmReportSummaryForm_FormClosing);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.AdmReportSummaryForm_DragEnter);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fcm_InputSummaryGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label lblLatestADMFileDate;
        private System.Windows.Forms.DataGridView fcm_InputSummaryGrid;
    }
}