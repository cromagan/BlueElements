﻿using System.ComponentModel;
using BlueControls.Controls;

namespace BlueControls.Forms
{
    partial class InputBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
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
            this.txbText = new BlueControls.Controls.TextBox();
            this.SuspendLayout();
            // 
            // txbText
            // 
            this.txbText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbText.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbText.Location = new System.Drawing.Point(8, 16);
            this.txbText.Name = "txbText";
            this.txbText.Size = new System.Drawing.Size(232, 24);
            this.txbText.TabIndex = 4;
            this.txbText.Enter += new System.EventHandler(this.txbText_Enter);
            this.txbText.Esc += new System.EventHandler(this.txbText_ESC);
            // 
            // InputBox
            // 
            this.ClientSize = new System.Drawing.Size(249, 91);
            this.Controls.Add(this.txbText);
            this.Name = "InputBox";
            this.Shown += new System.EventHandler(this.InputBox_Shown);
            this.Controls.SetChildIndex(this.txbText, 0);
            this.ResumeLayout(false);
        }
        #endregion

        private TextBox txbText;
    }
}