using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueDatabaseDialogs {
    public sealed partial class RowAdderScriptEditor  {
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
            this.txbTestZeile = new BlueControls.Controls.TextBox();
            this.cpZeile = new BlueControls.Controls.Caption();
            this.btnDatenbankKopf = new BlueControls.Controls.Button();
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
            // grpInfos
            // 
            this.grpInfos.Controls.Add(this.btnDatenbankKopf);
            this.grpInfos.Location = new System.Drawing.Point(464, 3);
            this.grpInfos.Size = new System.Drawing.Size(144, 75);
            this.grpInfos.Controls.SetChildIndex(this.btnDatenbankKopf, 0);
            this.grpInfos.Controls.SetChildIndex(this.btnBefehlsUebersicht, 0);
            // 
            // grpAusführen
            // 
            this.grpAusführen.Controls.Add(this.txbTestZeile);
            this.grpAusführen.Controls.Add(this.cpZeile);
            this.grpAusführen.Size = new System.Drawing.Size(461, 75);
            this.grpAusführen.Controls.SetChildIndex(this.cpZeile, 0);
            this.grpAusführen.Controls.SetChildIndex(this.txbTestZeile, 0);
            this.grpAusführen.Controls.SetChildIndex(this.btnAusführen, 0);
            // 
            // grpAktionen
            // 
            this.grpAktionen.Location = new System.Drawing.Point(608, 3);
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
            // txbTestZeile
            // 
            this.txbTestZeile.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbTestZeile.Location = new System.Drawing.Point(73, 24);
            this.txbTestZeile.Name = "txbTestZeile";
            this.txbTestZeile.RaiseChangeDelay = 5;
            this.txbTestZeile.Size = new System.Drawing.Size(379, 22);
            this.txbTestZeile.TabIndex = 9;
            // 
            // cpZeile
            // 
            this.cpZeile.CausesValidation = false;
            this.cpZeile.Location = new System.Drawing.Point(73, 2);
            this.cpZeile.Name = "cpZeile";
            this.cpZeile.Size = new System.Drawing.Size(112, 22);
            this.cpZeile.Text = "Betreffende Zeile:";
            // 
            // btnDatenbankKopf
            // 
            this.btnDatenbankKopf.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnDatenbankKopf.ImageCode = "Datenbank||||||||||Stift";
            this.btnDatenbankKopf.Location = new System.Drawing.Point(72, 2);
            this.btnDatenbankKopf.Name = "btnDatenbankKopf";
            this.btnDatenbankKopf.Size = new System.Drawing.Size(64, 66);
            this.btnDatenbankKopf.TabIndex = 46;
            this.btnDatenbankKopf.Text = "Datenbank-Kopf";
            this.btnDatenbankKopf.Click += new System.EventHandler(this.btnDatenbankKopf_Click);
            // 
            // RowAdderScriptEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(1297, 677);
            this.Name = "RowAdderScriptEditor";
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

        private TextBox txbTestZeile;
        private Caption cpZeile;
        private Button btnDatenbankKopf;
    }
}
