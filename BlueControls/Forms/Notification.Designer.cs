using System.ComponentModel;
using System.Windows.Forms;
using BlueControls.Controls;

namespace BlueControls.Forms {
    partial class Notification {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing) {
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
            this.components = new System.ComponentModel.Container();
            this.capText = new BlueControls.Controls.Caption();
            this.timNote = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // capText
            // 
            this.capText.CausesValidation = false;
            this.capText.Location = new System.Drawing.Point(8, 8);
            this.capText.Name = "capText";
            this.capText.Size = new System.Drawing.Size(10, 10);
            this.capText.Translate = false;
            // 
            // timNote
            // 
            this.timNote.Interval = 10;
            this.timNote.Tick += new System.EventHandler(this.Timer_Tick);
            // 
            // Notification
            // 
            this.ClientSize = new System.Drawing.Size(86, 65);
            this.Controls.Add(this.capText);
            this.Name = "Notification";
            this.ResumeLayout(false);
        }
        #endregion

        private Caption capText;
        private Timer timNote;
    }
}