using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueScript.EventArgs;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueDatabaseDialogs {
    public sealed partial class CreativePadScriptEditor {
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
            this.btnDatenbankKopf = new Button();
            this.txbTestZeile = new TextBox();
            this.cpZeile = new Caption();
            this.cpad = new CreativePad();
            this.pnlStatusBar.SuspendLayout();
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
            // btnDatenbankKopf
            // 
            this.btnDatenbankKopf.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            this.btnDatenbankKopf.ImageCode = "Datenbank||||||||||Stift";
            this.btnDatenbankKopf.Location = new Point(8, 2);
            this.btnDatenbankKopf.Name = "btnDatenbankKopf";
            this.btnDatenbankKopf.Size = new Size(64, 66);
            this.btnDatenbankKopf.TabIndex = 46;
            this.btnDatenbankKopf.Text = "Datenbank-Kopf";
            this.btnDatenbankKopf.Click += new EventHandler(this.btnDatenbankKopf_Click);
            // 
            // grpAusführen
            // 
            this.grpAusführen.Controls.Add(this.txbTestZeile);
            this.grpAusführen.Controls.Add(this.cpZeile);
            // 
            // txbTestZeile
            // 
            this.txbTestZeile.Cursor = Cursors.IBeam;
            this.txbTestZeile.Location = new Point(73, 24);
            this.txbTestZeile.Name = "txbTestZeile";
            this.txbTestZeile.RaiseChangeDelay = 5;
            this.txbTestZeile.Size = new Size(379, 22);
            this.txbTestZeile.TabIndex = 9;
            // 
            // cpZeile
            // 
            this.cpZeile.CausesValidation = false;
            this.cpZeile.Location = new Point(73, 2);
            this.cpZeile.Name = "cpZeile";
            this.cpZeile.Size = new Size(112, 22);
            this.cpZeile.Text = "Betreffende Zeile:";
            // 
            // btnAusführen
            // 
            this.btnAusführen.Click += new EventHandler(this.btnAusführen_Click);
            // 
            // cpad
            // 
            this.cpad.CausesValidation = false;
            this.cpad.Dock = DockStyle.Right;
            this.cpad.Location = new Point(840, 110);
            this.cpad.Name = "cpad";
            this.cpad.ShiftX = 0F;
            this.cpad.ShiftY = 0F;
            this.cpad.ShowJointPoint = true;
            this.cpad.Size = new Size(457, 543);
            this.cpad.TabIndex = 0;
            this.cpad.TabStop = false;
            this.cpad.Text = "connectedCreativePad1";
            this.cpad.Zoom = 1F;
            // 
            // CreativePadScriptEditor
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.ClientSize = new Size(1297, 677);
            this.Controls.Add(this.cpad);
            this.MinimizeBox = false;
            this.Name = "CreativePadScriptEditor";
            this.Text = "Datenbank-Eigenschaften";
            this.TopMost = true;
            this.WindowState = FormWindowState.Maximized;
            this.Controls.SetChildIndex(this.ribMain, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.cpad, 0);
            this.Controls.SetChildIndex(this.eventScriptEditor, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.ribMain.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAktionen.ResumeLayout(false);
            this.grpInfos.ResumeLayout(false);
            this.grpAusführen.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private TextBox txbTestZeile;
        private Caption cpZeile;
        private Button btnDatenbankKopf;
        private CreativePad cpad;
    }
}
