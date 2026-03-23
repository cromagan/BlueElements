using System.ComponentModel;
using System.Windows.Forms;
using BlueControls.Controls;

namespace BlueControls.Forms {
    partial class Notification {
        private IContainer components = null;
        protected override void Dispose(bool disposing) {
            if (disposing) {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.capText = new BlueControls.Controls.Caption();
            this.timNote = new System.Windows.Forms.Timer(this.components);
            this.btnClose = new BlueControls.Controls.Button();
            this.btnAction = new BlueControls.Controls.Button();
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
            // btnClose
            // 
            this.btnClose.ButtonStyle = BlueControls.Enums.ButtonStyle.Borderless;
            this.btnClose.ImageCode = "Kreuz|16";
            this.btnClose.Location = new System.Drawing.Point(100, 4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(24, 24);
            this.btnClose.Translate = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnAction
            // 
            this.btnAction.ButtonStyle = BlueControls.Enums.ButtonStyle.Button;
            this.btnAction.Location = new System.Drawing.Point(8, 100);
            this.btnAction.Name = "btnAction";
            this.btnAction.Size = new System.Drawing.Size(100, 28);
            this.btnAction.Translate = false;
            this.btnAction.Visible = false;
            this.btnAction.Click += new System.EventHandler(this.btnAction_Click);
            // 
            // Notification
            // 
            this.ClientSize = new System.Drawing.Size(86, 65);
            this.Controls.Add(this.btnAction);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.capText);
            this.Name = "Notification";
            this.ResumeLayout(false);
        }
        #endregion

        private Caption capText;
        private Timer timNote;
        private BlueControls.Controls.Button btnClose;
        private BlueControls.Controls.Button btnAction;
    }
}
