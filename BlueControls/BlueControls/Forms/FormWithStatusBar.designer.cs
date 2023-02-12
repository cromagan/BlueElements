using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;

namespace BlueControls.Forms {
    public partial class FormWithStatusBar {
        //Das Formular �berschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing) {
            if (disposing) {
            }
            base.Dispose(disposing);
        }
        //Hinweis: Die folgende Prozedur ist f�r den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer m�glich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht m�glich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            this.capStatusBar = new BlueControls.Controls.Caption();
            this.pnlStatusBar = new System.Windows.Forms.Panel();
            this.pnlStatusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.CausesValidation = false;
            this.capStatusBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.capStatusBar.Location = new System.Drawing.Point(0, 0);
            this.capStatusBar.Name = "capStatusBar";
            this.capStatusBar.Size = new System.Drawing.Size(287, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Controls.Add(this.capStatusBar);
            this.pnlStatusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 148);
            this.pnlStatusBar.Name = "pnlStatusBar";
            this.pnlStatusBar.Size = new System.Drawing.Size(287, 24);
            this.pnlStatusBar.TabIndex = 96;
            // 
            // FormWithStatusBar
            // 
            this.ClientSize = new System.Drawing.Size(287, 172);
            this.Controls.Add(this.pnlStatusBar);
            this.Name = "FormWithStatusBar";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "(c) Christian Peter";
            this.pnlStatusBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        protected Caption capStatusBar;
        protected Panel pnlStatusBar;
    }
}