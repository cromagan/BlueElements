using System.ComponentModel;
using BlueControls.Controls;

namespace BlueControls {
    partial class GlobalMonitor {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.lstLog = new BlueControls.Controls.ListBox();
            this.grpLogal = new BlueControls.Controls.GroupBox();
            this.btnLeeren = new BlueControls.Controls.Button();
            this.btnFilterDel = new BlueControls.Controls.Button();
            this.txbFilter = new BlueControls.Controls.TextBox();
            this.grpLogal.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstLog
            // 
            this.lstLog.AddAllowed = BlueControls.Enums.AddType.None;
            this.lstLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstLog.Location = new System.Drawing.Point(8, 56);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(784, 352);
            this.lstLog.TabIndex = 3;
            // 
            // grpLogal
            // 
            this.grpLogal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpLogal.Controls.Add(this.btnLeeren);
            this.grpLogal.Controls.Add(this.btnFilterDel);
            this.grpLogal.Controls.Add(this.txbFilter);
            this.grpLogal.Controls.Add(this.lstLog);
            this.grpLogal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpLogal.Location = new System.Drawing.Point(0, 0);
            this.grpLogal.Name = "grpLogal";
            this.grpLogal.Size = new System.Drawing.Size(800, 450);
            this.grpLogal.TabIndex = 5;
            this.grpLogal.TabStop = false;
            this.grpLogal.Text = "Globaler Log";
            // 
            // btnLeeren
            // 
            this.btnLeeren.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLeeren.ImageCode = "Kreuz|16";
            this.btnLeeren.Location = new System.Drawing.Point(752, 416);
            this.btnLeeren.Name = "btnLeeren";
            this.btnLeeren.QuickInfo = "Log leeren";
            this.btnLeeren.Size = new System.Drawing.Size(40, 24);
            this.btnLeeren.TabIndex = 6;
            this.btnLeeren.Click += new System.EventHandler(this.btnLeeren_Click);
            // 
            // btnFilterDel
            // 
            this.btnFilterDel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFilterDel.ImageCode = "Trichter|16||1";
            this.btnFilterDel.Location = new System.Drawing.Point(752, 24);
            this.btnFilterDel.Name = "btnFilterDel";
            this.btnFilterDel.QuickInfo = "Filter löschen";
            this.btnFilterDel.Size = new System.Drawing.Size(40, 24);
            this.btnFilterDel.TabIndex = 5;
            this.btnFilterDel.Click += new System.EventHandler(this.btnFilterDel_Click);
            // 
            // txbFilter
            // 
            this.txbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbFilter.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbFilter.Location = new System.Drawing.Point(8, 24);
            this.txbFilter.Name = "txbFilter";
            this.txbFilter.QuickInfo = "Textfilter";
            this.txbFilter.Size = new System.Drawing.Size(736, 24);
            this.txbFilter.SpellCheckingEnabled = true;
            this.txbFilter.TabIndex = 4;
            this.txbFilter.TextChanged += new System.EventHandler(this.txbFilter_TextChanged);
            // 
            // GlobalMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.grpLogal);
            this.Name = "GlobalMonitor";
            this.Text = "Monitoring";
            this.TopMost = true;
            this.grpLogal.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ListBox lstLog;
        private GroupBox grpLogal;
        private Button btnFilterDel;
        private TextBox txbFilter;
        private Button btnLeeren;
    }
}