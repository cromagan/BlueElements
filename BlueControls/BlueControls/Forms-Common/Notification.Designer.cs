using BlueControls.Enums;
namespace BlueControls.Forms
{
    partial class Notification
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Notification));
            this.capTXT = new BlueControls.Controls.Caption();
            this.timNote = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // capTXT
            // 
            this.capTXT.CausesValidation = false;
            this.capTXT.Location = new System.Drawing.Point(8, 8);
            this.capTXT.Name = "capTXT";
            this.capTXT.Size = new System.Drawing.Size(10, 10);
            this.capTXT.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Steuerelement_Anpassen;
            this.capTXT.Translate = false;
            this.capTXT.Click += new System.EventHandler(this.capTXT_Click);
            // 
            // timNote
            // 
            this.timNote.Interval = 10;
            this.timNote.Tick += new System.EventHandler(this.timNote_Tick);
            // 
            // Notification
            // 
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(86, 65);
            this.Controls.Add(this.capTXT);
            this.Name = "Notification";
            this.ResumeLayout(false);
        }
        #endregion
        private Controls.Caption capTXT;
        private System.Windows.Forms.Timer timNote;
    }
}