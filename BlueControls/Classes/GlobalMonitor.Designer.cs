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
            this.grpLogal = new BlueControls.Controls.GroupBox();
            this.tblLog = new BlueControls.Controls.TableViewWithFilters();
            this.btnLeeren = new BlueControls.Controls.Button();
            this.grpLogal.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpLogal
            // 
            this.grpLogal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpLogal.Controls.Add(this.tblLog);
            this.grpLogal.Controls.Add(this.btnLeeren);
            this.grpLogal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpLogal.Location = new System.Drawing.Point(0, 0);
            this.grpLogal.Name = "grpLogal";
            this.grpLogal.Size = new System.Drawing.Size(800, 450);
            this.grpLogal.TabIndex = 5;
            this.grpLogal.TabStop = false;
            this.grpLogal.Text = "Globaler Log";
            // 
            // tblLog
            // 
            this.tblLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tblLog.Location = new System.Drawing.Point(8, 16);
            this.tblLog.Name = "tblLog";
            this.tblLog.SheetStyle = "Windows 11";
            this.tblLog.Size = new System.Drawing.Size(784, 392);
            this.tblLog.TabIndex = 7;
            // 
            // btnLeeren
            // 
            this.btnLeeren.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLeeren.ImageCode = "Kreuz|16";
            this.btnLeeren.Location = new System.Drawing.Point(752, 416);
            this.btnLeeren.Name = "btnLeeren";
            this.btnLeeren.QuickInfo = "Log leeren";
            this.btnLeeren.Size = new System.Drawing.Size(40, 24);
            this.btnLeeren.TabIndex = 6;
            this.btnLeeren.Click += new System.EventHandler(this.btnLeeren_Click);
            // 
            // GlobalMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.grpLogal);
            this.Name = "GlobalMonitor";
            this.Text = "Monitoring";
            this.grpLogal.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private GroupBox grpLogal;
        private Button btnLeeren;
        private TableViewWithFilters tblLog;
    }
}