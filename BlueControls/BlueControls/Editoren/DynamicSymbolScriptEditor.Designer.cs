using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.BlueDatabaseDialogs {
    public sealed partial class DynamicSymbolScriptEditor {
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
            this.grpAusführen.SuspendLayout();
            this.pnlStatusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // DynamicSymbolScriptEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Name = "DynamicSymbolScriptEditor";
            this.Text = "Tabellen-Eigenschaften";
            this.grpAusführen.ResumeLayout(false);
            this.pnlStatusBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

    }
}
