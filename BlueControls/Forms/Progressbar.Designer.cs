// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using System.ComponentModel;

namespace BlueControls.Forms
{
    partial class Progressbar
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
            this.capText = new BlueControls.Controls.Caption();
            this.btnAction = new BlueControls.Controls.Button();
            this.SuspendLayout();
            // 
            // capText
            // 
            this.capText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.capText.CausesValidation = false;
            this.capText.Location = new System.Drawing.Point(8, 8);
            this.capText.Name = "capText";
            this.capText.Size = new System.Drawing.Size(72, 48);
            // 
            // btnAction
            // 
            this.btnAction.ButtonStyle = BlueControls.Enums.ButtonStyle.Button;
            this.btnAction.Location = new System.Drawing.Point(8, 60);
            this.btnAction.Name = "btnAction";
            this.btnAction.Size = new System.Drawing.Size(100, 28);
            this.btnAction.Translate = false;
            this.btnAction.Visible = false;
            this.btnAction.Click += new System.EventHandler(this.btnAction_Click);
            // 
            // Progressbar
            // 
            this.ClientSize = new System.Drawing.Size(86, 65);
            this.Controls.Add(this.btnAction);
            this.Controls.Add(this.capText);
            this.Name = "Progressbar";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
        }
        #endregion

        private Caption capText;
        private BlueControls.Controls.Button btnAction;
    }
}