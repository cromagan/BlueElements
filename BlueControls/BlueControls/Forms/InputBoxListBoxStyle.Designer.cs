﻿using System.ComponentModel;
using BlueControls.Controls;
using BlueControls.Enums;

namespace BlueControls.Forms
{
    partial class InputBoxListBoxStyle
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
            this.txbText = new BlueControls.Controls.ListBox();
            this.SuspendLayout();
            // 
            // txbText
            // 
            this.txbText.AddAllowed = AddType.Text;
            this.txbText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbText.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbText.Location = new System.Drawing.Point(8, 16);
            this.txbText.Name = "txbText";
            this.txbText.Size = new System.Drawing.Size(232, 264);
            this.txbText.TabIndex = 4;
            // 
            // InputBoxListBoxStyle
            // 
            this.ClientSize = new System.Drawing.Size(249, 295);
            this.Controls.Add(this.txbText);
            this.Name = "InputBoxListBoxStyle";
            this.Shown += new System.EventHandler(this.InputBox_Shown);
            this.Controls.SetChildIndex(this.txbText, 0);
            this.ResumeLayout(false);
        }
        #endregion

        private ListBox txbText;
    }
}