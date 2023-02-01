using BlueControls.Enums;

namespace BlueControls.Forms
{
    partial class DialogWithOkAndCancel
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
            this.butOK = new BlueControls.Controls.Button();
            this.butAbbrechen = new BlueControls.Controls.Button();
            this.capText = new BlueControls.Controls.Caption();
            this.SuspendLayout();
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.ImageCode = "Häkchen|16";
            this.butOK.Location = new System.Drawing.Point(72, 7);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(56, 40);
            this.butOK.TabIndex = 2;
            this.butOK.Text = "OK";
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butAbbrechen
            // 
            this.butAbbrechen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butAbbrechen.ImageCode = "Kreuz|16";
            this.butAbbrechen.Location = new System.Drawing.Point(136, 7);
            this.butAbbrechen.Name = "butAbbrechen";
            this.butAbbrechen.Size = new System.Drawing.Size(104, 40);
            this.butAbbrechen.TabIndex = 3;
            this.butAbbrechen.Text = "Abbrechen";
            this.butAbbrechen.Click += new System.EventHandler(this.butAbbrechen_Click);
            // 
            // capText
            // 
            this.capText.CausesValidation = false;
            this.capText.Location = new System.Drawing.Point(8, 8);
            this.capText.Name = "capText";
            this.capText.Size = new System.Drawing.Size(1, 1);
            this.capText.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Steuerelement_Anpassen;
            this.capText.Visible = false;
            // 
            // DialogWithOkAndCancel
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.ToolTip;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(249, 55);
            this.CloseButtonEnabled = false;
            this.Controls.Add(this.capText);
            this.Controls.Add(this.butAbbrechen);
            this.Controls.Add(this.butOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "DialogWithOkAndCancel";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.TopMost = true;
            this.ResumeLayout(false);
        }
        #endregion

        private Controls.Button butOK;
        private Controls.Button butAbbrechen;
        private Controls.Caption capText;
    }
}