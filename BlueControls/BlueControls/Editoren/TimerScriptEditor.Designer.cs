using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;

namespace BlueControls.BlueDatabaseDialogs {
    public sealed partial class TimerScriptEditor {
        //Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing) {
            if (disposing) {
            }
            base.Dispose(disposing);
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            this.grpInfos.SuspendLayout();
            this.pnlStatusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // TimerScriptEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Name = "TimerScriptEditor";
            this.Text = "Tabellen-Eigenschaften";
            this.grpInfos.ResumeLayout(false);
            this.pnlStatusBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

    }
}
