using System.Drawing;

namespace BlueControls.DialogBoxes
{
    partial class InputBoxComboStyle
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
            this.cbxText = new BlueControls.Controls.ComboBox();
            this.SuspendLayout();
            // 
            // cbxText
            // 
            this.cbxText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxText.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxText.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxText.Location = new System.Drawing.Point(8, 32);
            this.cbxText.Name = "cbxText";
            this.cbxText.Size = new System.Drawing.Size(232, 24);
            this.cbxText.TabIndex = 4;
            this.cbxText.Enter += new System.EventHandler(this.cbxText_Enter);
            this.cbxText.ESC += new System.EventHandler(this.cbxText_ESC);
            // 
            // InputComboBox
            // 
            this.ClientSize = new System.Drawing.Size(249, 112);
            this.Controls.Add(this.cbxText);
            this.Name = "InputComboBox";
            this.Shown += new System.EventHandler(this.InputComboBox_Shown);
            this.Controls.SetChildIndex(this.cbxText, 0);
            this.ResumeLayout(false);

        }


        #endregion


        private Controls.ComboBox cbxText;
    }
}