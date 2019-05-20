﻿using System.Drawing;

namespace BlueControls.DialogBoxes
{
    partial class EditBoxRow
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
            this.formToEdit = new BlueControls.Controls.Formula();
            this.SuspendLayout();
            // 
            // butOK
            // 
            this.butOK.ButtonStyle = BlueControls.Enums.enButtonStyle.Button;
            this.butOK.Location = new System.Drawing.Point(72, 213);
            // 
            // formToEdit
            // 
            this.formToEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.formToEdit.Location = new System.Drawing.Point(8, 8);
            this.formToEdit.MinimumSize = new System.Drawing.Size(320, 350);
            this.formToEdit.Name = "formToEdit";
            this.formToEdit.Size = new System.Drawing.Size(592, 648);
            this.formToEdit.TabIndex = 4;
            // 
            // EditBoxRow
            // 
            this.ClientSize = new System.Drawing.Size(612, 663);
            this.Controls.Add(this.formToEdit);
            this.Name = "EditBoxRow";
            this.Controls.SetChildIndex(this.butOK, 0);
            this.Controls.SetChildIndex(this.formToEdit, 0);
            this.ResumeLayout(false);

        }


        #endregion


        private Controls.Formula formToEdit;
    }
}