using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueDatabaseDialogs {
    public sealed partial class CreativePadScriptEditor {
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
            this.btnDatenbankKopf = new BlueControls.Controls.Button();
            this.txbTestZeile = new BlueControls.Controls.TextBox();
            this.cpZeile = new BlueControls.Controls.Caption();
            this.cpad = new BlueControls.Controls.CreativePad();
            this.grpAusf�hren.SuspendLayout();
            this.pnlStatusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbcScriptEigenschaften
            // 
            this.tbcScriptEigenschaften.Size = new System.Drawing.Size(327, 451);
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(327, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Size = new System.Drawing.Size(327, 24);
            // 
            // btnDatenbankKopf
            // 
            this.btnDatenbankKopf.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnDatenbankKopf.ImageCode = "Datenbank||||||||||Stift";
            this.btnDatenbankKopf.Location = new System.Drawing.Point(8, 2);
            this.btnDatenbankKopf.Name = "btnDatenbankKopf";
            this.btnDatenbankKopf.Size = new System.Drawing.Size(64, 66);
            this.btnDatenbankKopf.TabIndex = 46;
            this.btnDatenbankKopf.Text = "Datenbank-Kopf";
            this.btnDatenbankKopf.Click += new System.EventHandler(this.btnDatenbankKopf_Click);
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
            // cpad
            // 
            this.cpad.CausesValidation = false;
            this.cpad.Dock = System.Windows.Forms.DockStyle.Right;
            this.cpad.Location = new System.Drawing.Point(327, 110);
            this.cpad.Name = "cpad";
            this.cpad.ShowJointPoint = true;
            this.cpad.Size = new System.Drawing.Size(457, 451);
            this.cpad.TabIndex = 0;
            this.cpad.TabStop = false;
            this.cpad.Text = "connectedCreativePad1";
            // 
            // CreativePadScriptEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.cpad);
            this.Name = "CreativePadScriptEditor";
            this.Text = "Datenbank-Eigenschaften";
            this.Controls.SetChildIndex(this.ribMain, 0);
            this.Controls.SetChildIndex(this.cpad, 0);
            this.Controls.SetChildIndex(this.tbcScriptEigenschaften, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.grpAusf�hren.ResumeLayout(false);
            this.pnlStatusBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private TextBox txbTestZeile;
        private Caption cpZeile;
        private Button btnDatenbankKopf;
        private CreativePad cpad;
    }
}
