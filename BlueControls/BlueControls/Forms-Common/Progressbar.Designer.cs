using BlueControls.Enums;

namespace BlueControls.Forms
{
    partial class Progressbar
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Progressbar));
            this.capTXT = new BlueControls.Controls.Caption();
            this.SuspendLayout();
            // 
            // capTXT
            // 
            this.capTXT.CausesValidation = false;
            this.capTXT.Location = new System.Drawing.Point(8, 8);
            this.capTXT.Name = "capTXT";
            this.capTXT.Size = new System.Drawing.Size(1, 1);
            this.capTXT.Translate = false;
            this.capTXT.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Steuerelement_Anpassen;
            // 
            // Progressbar
            // 
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(86, 65);
            this.Controls.Add(this.capTXT);
            this.Name = "Progressbar";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.Caption capTXT;
    }
}