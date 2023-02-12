﻿
using System.ComponentModel;
using BlueControls.Controls;

namespace BlueControls {
    partial class Befehlsreferenz {
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
            this.lstComands = new BlueControls.Controls.ListBox();
            this.txbComms = new BlueControls.Controls.TextBox();
            this.SuspendLayout();
            // 
            // lstComands
            // 
            this.lstComands.AddAllowed = BlueControls.Enums.AddType.None;
            this.lstComands.Dock = System.Windows.Forms.DockStyle.Left;
            this.lstComands.FilterAllowed = true;
            this.lstComands.Location = new System.Drawing.Point(0, 0);
            this.lstComands.Name = "lstComands";
            this.lstComands.Size = new System.Drawing.Size(272, 450);
            this.lstComands.TabIndex = 3;
            this.lstComands.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.lstComands_ItemClicked);
            // 
            // txbComms
            // 
            this.txbComms.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbComms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txbComms.Location = new System.Drawing.Point(272, 0);
            this.txbComms.Name = "txbComms";
            this.txbComms.Size = new System.Drawing.Size(528, 450);
            this.txbComms.TabIndex = 2;
            this.txbComms.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // frmBefehlsreferenz
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.txbComms);
            this.Controls.Add(this.lstComands);
            this.Name = "Befehlsreferenz";
            this.Text = "Befehlsübersicht";
            this.ResumeLayout(false);

        }

        #endregion

        private ListBox lstComands;
        private TextBox txbComms;
    }
}