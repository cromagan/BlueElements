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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RowAdderScriptEditor));
            this.txbTestZeile = new BlueControls.Controls.TextBox();
            this.cpZeile = new BlueControls.Controls.Caption();
            this.btnDatenbankKopf = new BlueControls.Controls.Button();
            this.grpScripte = new BlueControls.Controls.GroupBox();
            this.caption1 = new BlueControls.Controls.Caption();
            this.capScriptMenu = new BlueControls.Controls.Caption();
            this.btnScriptAfter = new BlueControls.Controls.Button();
            this.btnScriptMenu = new BlueControls.Controls.Button();
            this.btnScriptBefore = new BlueControls.Controls.Button();
            this.caption2 = new BlueControls.Controls.Caption();
            this.grpInfos.SuspendLayout();
            this.grpAusführen.SuspendLayout();
            this.pnlStatusBar.SuspendLayout();
            this.grpScripte.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpInfos
            // 
            this.grpInfos.Controls.Add(this.btnDatenbankKopf);
            this.grpInfos.Location = new System.Drawing.Point(464, 3);
            this.grpInfos.Size = new System.Drawing.Size(144, 75);
            this.grpInfos.Controls.SetChildIndex(this.btnDatenbankKopf, 0);
            // 
            // grpAusführen
            // 
            this.grpAusführen.Controls.Add(this.txbTestZeile);
            this.grpAusführen.Controls.Add(this.cpZeile);
            this.grpAusführen.Size = new System.Drawing.Size(461, 75);
            this.grpAusführen.Controls.SetChildIndex(this.btnAusführen, 0);
            this.grpAusführen.Controls.SetChildIndex(this.cpZeile, 0);
            this.grpAusführen.Controls.SetChildIndex(this.txbTestZeile, 0);
            // 
            // grpAktionen
            // 
            this.grpAktionen.Location = new System.Drawing.Point(608, 3);
            // 
            // tbcScriptEigenschaften
            // 
            this.tbcScriptEigenschaften.Location = new System.Drawing.Point(290, 110);
            this.tbcScriptEigenschaften.Size = new System.Drawing.Size(494, 427);
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
            this.btnDatenbankKopf.Text = "Tabellen-Kopf";
            this.btnDatenbankKopf.Click += new System.EventHandler(this.btnDatenbankKopf_Click);
            // 
            // grpScripte
            // 
            this.grpScripte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpScripte.Controls.Add(this.caption2);
            this.grpScripte.Controls.Add(this.caption1);
            this.grpScripte.Controls.Add(this.capScriptMenu);
            this.grpScripte.Controls.Add(this.btnScriptAfter);
            this.grpScripte.Controls.Add(this.btnScriptMenu);
            this.grpScripte.Controls.Add(this.btnScriptBefore);
            this.grpScripte.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpScripte.Location = new System.Drawing.Point(0, 110);
            this.grpScripte.Name = "grpScripte";
            this.grpScripte.Size = new System.Drawing.Size(290, 427);
            this.grpScripte.TabIndex = 99;
            this.grpScripte.TabStop = false;
            this.grpScripte.Text = "Scripte";
            // 
            // caption1
            // 
            this.caption1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.caption1.CausesValidation = false;
            this.caption1.Location = new System.Drawing.Point(8, 48);
            this.caption1.Name = "caption1";
            this.caption1.Size = new System.Drawing.Size(274, 50);
            this.caption1.Text = "<b>Diese Skript wird ausgeführt, wenn der User einen Eintrag wählt - BEVOR die Ze" +
    "ile(n) angelegt werden.";
            this.caption1.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capScriptMenu
            // 
            this.capScriptMenu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.capScriptMenu.CausesValidation = false;
            this.capScriptMenu.Location = new System.Drawing.Point(8, 136);
            this.capScriptMenu.Name = "capScriptMenu";
            this.capScriptMenu.Size = new System.Drawing.Size(274, 220);
            this.capScriptMenu.Text = resources.GetString("capScriptMenu.Text");
            this.capScriptMenu.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnScriptAfter
            // 
            this.btnScriptAfter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnScriptAfter.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox;
            this.btnScriptAfter.Location = new System.Drawing.Point(8, 360);
            this.btnScriptAfter.Name = "btnScriptAfter";
            this.btnScriptAfter.Size = new System.Drawing.Size(274, 32);
            this.btnScriptAfter.TabIndex = 2;
            this.btnScriptAfter.Text = "After";
            this.btnScriptAfter.Click += new System.EventHandler(this.btnScriptAfter_Click);
            // 
            // btnScriptMenu
            // 
            this.btnScriptMenu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnScriptMenu.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox;
            this.btnScriptMenu.Checked = true;
            this.btnScriptMenu.Location = new System.Drawing.Point(8, 104);
            this.btnScriptMenu.Name = "btnScriptMenu";
            this.btnScriptMenu.Size = new System.Drawing.Size(274, 32);
            this.btnScriptMenu.TabIndex = 1;
            this.btnScriptMenu.Text = "Menu Generation";
            this.btnScriptMenu.Click += new System.EventHandler(this.btnScriptMenu_Click);
            // 
            // btnScriptBefore
            // 
            this.btnScriptBefore.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnScriptBefore.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox;
            this.btnScriptBefore.Location = new System.Drawing.Point(8, 16);
            this.btnScriptBefore.Name = "btnScriptBefore";
            this.btnScriptBefore.Size = new System.Drawing.Size(274, 32);
            this.btnScriptBefore.TabIndex = 0;
            this.btnScriptBefore.Text = "Before";
            this.btnScriptBefore.Click += new System.EventHandler(this.btnScriptBefore_Click);
            // 
            // caption2
            // 
            this.caption2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.caption2.CausesValidation = false;
            this.caption2.Location = new System.Drawing.Point(8, 392);
            this.caption2.Name = "caption2";
            this.caption2.Size = new System.Drawing.Size(274, 50);
            this.caption2.Text = "<b>Diese Skript wird ausgeführt, wenn der User einen Eintrag wählt - NACHDEM  die" +
    " Zeile(n) angelegt wurden.";
            this.caption2.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // RowAdderScriptEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.grpScripte);
            this.Name = "RowAdderScriptEditor";
            this.Text = "Tabellen-Eigenschaften";
            this.Controls.SetChildIndex(this.ribMain, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.grpScripte, 0);
            this.Controls.SetChildIndex(this.tbcScriptEigenschaften, 0);
            this.grpInfos.ResumeLayout(false);
            this.grpAusführen.ResumeLayout(false);
            this.pnlStatusBar.ResumeLayout(false);
            this.grpScripte.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private TextBox txbTestZeile;
        private Caption cpZeile;
        private Button btnDatenbankKopf;
        private GroupBox grpScripte;
        private Button btnScriptBefore;
        private Button btnScriptAfter;
        private Button btnScriptMenu;
        private Caption capScriptMenu;
        private Caption caption1;
        private Caption caption2;
    }
}
