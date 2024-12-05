using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueScript.EventArgs;
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
            this.pnlStatusBar.SuspendLayout();
            this.ribMain.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAktionen.SuspendLayout();
            this.grpInfos.SuspendLayout();
            this.grpAusführen.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new Point(0, 653);
            this.pnlStatusBar.Size = new Size(1297, 24);
            // 
            // eventScriptEditor
            // 
            this.eventScriptEditor.ExecuteScript += new EventHandler<ScriptEventArgs>(this.eventScriptEditor_ExecuteScript);
            // 
            // btnAusführen
            // 
            this.btnAusführen.Click += new EventHandler(this.btnAusführen_Click);
            // 
            // DynamicSymbolScriptEditor
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.ClientSize = new Size(1297, 677);
            this.MinimizeBox = false;
            this.Name = "DynamicSymbolScriptEditor";
            this.Text = "Datenbank-Eigenschaften";
            this.TopMost = true;
            this.WindowState = FormWindowState.Maximized;
            this.Controls.SetChildIndex(this.ribMain, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.eventScriptEditor, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.ribMain.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAktionen.ResumeLayout(false);
            this.grpInfos.ResumeLayout(false);
            this.grpAusführen.ResumeLayout(false);
            this.ResumeLayout(false);

        }

    }
}
