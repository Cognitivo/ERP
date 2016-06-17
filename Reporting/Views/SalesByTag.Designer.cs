﻿namespace Reporting.Views
{
    partial class SalesByTag
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SalesByTag));
            this.salesByTagBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.salesDB = new Cognitivo.Data.SalesDB();
            this.button1 = new System.Windows.Forms.Button();
            this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.reportViewer1 = new Microsoft.Reporting.WinForms.ReportViewer();
            this.salesByTagTableAdapter = new Cognitivo.Data.SalesDBTableAdapters.SalesByTagTableAdapter();
            ((System.ComponentModel.ISupportInitialize)(this.salesByTagBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.salesDB)).BeginInit();
            this.SuspendLayout();
            // 
            // salesByTagBindingSource
            // 
            this.salesByTagBindingSource.DataMember = "SalesByTag";
            this.salesByTagBindingSource.DataSource = this.salesDB;
            // 
            // salesDB
            // 
            this.salesDB.DataSetName = "SalesDB";
            this.salesDB.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(538, 14);
            this.button1.Margin = new System.Windows.Forms.Padding(7, 5, 7, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(220, 47);
            this.button1.TabIndex = 10;
            this.button1.Text = "Load";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // dateTimePicker2
            // 
            this.dateTimePicker2.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.dateTimePicker2.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.dateTimePicker2.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePicker2.Location = new System.Drawing.Point(276, 14);
            this.dateTimePicker2.Margin = new System.Windows.Forms.Padding(7, 5, 7, 5);
            this.dateTimePicker2.Name = "dateTimePicker2";
            this.dateTimePicker2.Size = new System.Drawing.Size(248, 47);
            this.dateTimePicker2.TabIndex = 9;
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.dateTimePicker1.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePicker1.Location = new System.Drawing.Point(16, 14);
            this.dateTimePicker1.Margin = new System.Windows.Forms.Padding(7, 5, 7, 5);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(246, 47);
            this.dateTimePicker1.TabIndex = 8;
            // 
            // reportViewer1
            // 
            this.reportViewer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.reportViewer1.LocalReport.ReportEmbeddedResource = "Reporting.Reports.SalesByTag.rdlc";
            this.reportViewer1.Location = new System.Drawing.Point(-3, 78);
            this.reportViewer1.Margin = new System.Windows.Forms.Padding(0);
            this.reportViewer1.Name = "reportViewer1";
            this.reportViewer1.Size = new System.Drawing.Size(779, 454);
            this.reportViewer1.TabIndex = 7;
            // 
            // salesByTagTableAdapter
            // 
            this.salesByTagTableAdapter.ClearBeforeFill = true;
            // 
            // SalesByTag
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(774, 529);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dateTimePicker2);
            this.Controls.Add(this.dateTimePicker1);
            this.Controls.Add(this.reportViewer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SalesByTag";
            this.Text = "Sales By Tag";
            this.Load += new System.EventHandler(this.SalesByTag_Load);
            ((System.ComponentModel.ISupportInitialize)(this.salesByTagBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.salesDB)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private Microsoft.Reporting.WinForms.ReportViewer reportViewer1;
        private System.Windows.Forms.BindingSource salesByTagBindingSource;
        private Cognitivo.Data.SalesDB salesDB;
        private Cognitivo.Data.SalesDBTableAdapters.SalesByTagTableAdapter salesByTagTableAdapter;
    }
}