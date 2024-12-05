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
            this.ribMain.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpInfos.SuspendLayout();
            this.grpAusführen.SuspendLayout();
            this.grpAktionen.SuspendLayout();
            this.tbcScriptEigenschaften.SuspendLayout();
            this.tabScriptEditor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.grpCode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSkript)).BeginInit();
            this.grpAusgabeFenster.SuspendLayout();
            this.pnlStatusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // ribMain
            // 
            this.ribMain.Size = new System.Drawing.Size(1297, 110);
            // 
            // tabStart
            // 
            this.tabStart.Size = new System.Drawing.Size(1289, 81);
            // 
            // tbcScriptEigenschaften
            // 
            this.tbcScriptEigenschaften.Size = new System.Drawing.Size(1297, 567);
            // 
            // tabScriptEditor
            // 
            this.tabScriptEditor.Size = new System.Drawing.Size(1289, 538);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Size = new System.Drawing.Size(1283, 532);
            this.splitContainer1.SplitterDistance = 272;
            // 
            // grpCode
            // 
            this.grpCode.Size = new System.Drawing.Size(1283, 272);
            // 
            // txtSkript
            // 
            this.txtSkript.AutoScrollMinSize = new System.Drawing.Size(27, 14);
            this.txtSkript.Size = new System.Drawing.Size(1267, 231);
            // 
            // grpVariablen
            // 
            this.grpVariablen.Size = new System.Drawing.Size(872, 256);
            // 
            // grpAusgabeFenster
            // 
            this.grpAusgabeFenster.Location = new System.Drawing.Point(872, 0);
            this.grpAusgabeFenster.Size = new System.Drawing.Size(411, 256);
            // 
            // txbSkriptInfo
            // 
            this.txbSkriptInfo.AdditionalFormatCheck = BlueBasics.Enums.AdditionalCheck.None;
            this.txbSkriptInfo.Size = new System.Drawing.Size(396, 223);
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(1297, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Size = new System.Drawing.Size(1297, 24);
            // 
            // TimerScriptEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(1297, 677);
            this.Name = "TimerScriptEditor";
            this.Text = "Datenbank-Eigenschaften";
            this.ribMain.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpInfos.ResumeLayout(false);
            this.grpAusführen.ResumeLayout(false);
            this.grpAktionen.ResumeLayout(false);
            this.tbcScriptEigenschaften.ResumeLayout(false);
            this.tabScriptEditor.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.grpCode.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtSkript)).EndInit();
            this.grpAusgabeFenster.ResumeLayout(false);
            this.pnlStatusBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

    }
}
