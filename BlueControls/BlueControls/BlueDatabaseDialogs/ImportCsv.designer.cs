using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueDatabaseDialogs
{
    public sealed partial class ImportCsv {
        //Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.btnImportieren = new BlueControls.Controls.Button();
            this.chkDoppelteTrennzeichen = new BlueControls.Controls.Button();
            this.grpTrennzeichen = new BlueControls.Controls.GroupBox();
            this.txtAndere = new BlueControls.Controls.TextBox();
            this.optTabStopp = new BlueControls.Controls.Button();
            this.optAndere = new BlueControls.Controls.Button();
            this.optSemikolon = new BlueControls.Controls.Button();
            this.optLeerzeichen = new BlueControls.Controls.Button();
            this.optKomma = new BlueControls.Controls.Button();
            this.capEinträge = new BlueControls.Controls.Caption();
            this.chkTrennzeichenAmAnfang = new BlueControls.Controls.Button();
            this.grpSpaltenOptionen = new BlueControls.Controls.GroupBox();
            this.optSpalteNachderReihe = new BlueControls.Controls.Button();
            this.optSpalteZuordnen = new BlueControls.Controls.Button();
            this.btnCancel = new BlueControls.Controls.Button();
            this.grpZeilenOptionen = new BlueControls.Controls.GroupBox();
            this.optZeilenAlle = new BlueControls.Controls.Button();
            this.optZeilenZuorden = new BlueControls.Controls.Button();
            this.pnlStatusBar.SuspendLayout();
            this.grpTrennzeichen.SuspendLayout();
            this.grpSpaltenOptionen.SuspendLayout();
            this.grpZeilenOptionen.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(731, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 302);
            this.pnlStatusBar.Size = new System.Drawing.Size(731, 24);
            // 
            // btnImportieren
            // 
            this.btnImportieren.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnImportieren.ImageCode = "Textfeld|16";
            this.btnImportieren.Location = new System.Drawing.Point(621, 264);
            this.btnImportieren.Name = "btnImportieren";
            this.btnImportieren.Size = new System.Drawing.Size(104, 32);
            this.btnImportieren.TabIndex = 9;
            this.btnImportieren.Text = "Importieren";
            this.btnImportieren.Click += new System.EventHandler(this.Fertig_Click);
            // 
            // chkDoppelteTrennzeichen
            // 
            this.chkDoppelteTrennzeichen.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkDoppelteTrennzeichen.Location = new System.Drawing.Point(8, 40);
            this.chkDoppelteTrennzeichen.Name = "chkDoppelteTrennzeichen";
            this.chkDoppelteTrennzeichen.Size = new System.Drawing.Size(376, 16);
            this.chkDoppelteTrennzeichen.TabIndex = 7;
            this.chkDoppelteTrennzeichen.Text = "Aufeinanderfolgende Trennzeichen als ein Zeichen behandeln";
            // 
            // grpTrennzeichen
            // 
            this.grpTrennzeichen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpTrennzeichen.CausesValidation = false;
            this.grpTrennzeichen.Controls.Add(this.txtAndere);
            this.grpTrennzeichen.Controls.Add(this.optTabStopp);
            this.grpTrennzeichen.Controls.Add(this.optAndere);
            this.grpTrennzeichen.Controls.Add(this.optSemikolon);
            this.grpTrennzeichen.Controls.Add(this.optLeerzeichen);
            this.grpTrennzeichen.Controls.Add(this.optKomma);
            this.grpTrennzeichen.Location = new System.Drawing.Point(16, 88);
            this.grpTrennzeichen.Name = "grpTrennzeichen";
            this.grpTrennzeichen.Size = new System.Drawing.Size(168, 169);
            this.grpTrennzeichen.TabIndex = 13;
            this.grpTrennzeichen.TabStop = false;
            this.grpTrennzeichen.Text = "Trennzeichen";
            // 
            // txtAndere
            // 
            this.txtAndere.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtAndere.Location = new System.Drawing.Point(88, 116);
            this.txtAndere.Name = "txtAndere";
            this.txtAndere.Size = new System.Drawing.Size(64, 24);
            this.txtAndere.TabIndex = 6;
            // 
            // optTabStopp
            // 
            this.optTabStopp.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.optTabStopp.Checked = true;
            this.optTabStopp.Location = new System.Drawing.Point(16, 24);
            this.optTabStopp.Name = "optTabStopp";
            this.optTabStopp.Size = new System.Drawing.Size(80, 16);
            this.optTabStopp.TabIndex = 1;
            this.optTabStopp.Text = "TabStop";
            // 
            // optAndere
            // 
            this.optAndere.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.optAndere.Location = new System.Drawing.Point(16, 120);
            this.optAndere.Name = "optAndere";
            this.optAndere.Size = new System.Drawing.Size(64, 16);
            this.optAndere.TabIndex = 5;
            this.optAndere.Text = "Andere:";
            // 
            // optSemikolon
            // 
            this.optSemikolon.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.optSemikolon.Location = new System.Drawing.Point(16, 72);
            this.optSemikolon.Name = "optSemikolon";
            this.optSemikolon.Size = new System.Drawing.Size(80, 16);
            this.optSemikolon.TabIndex = 2;
            this.optSemikolon.Text = "Semikolon";
            // 
            // optLeerzeichen
            // 
            this.optLeerzeichen.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.optLeerzeichen.Location = new System.Drawing.Point(16, 48);
            this.optLeerzeichen.Name = "optLeerzeichen";
            this.optLeerzeichen.Size = new System.Drawing.Size(88, 16);
            this.optLeerzeichen.TabIndex = 4;
            this.optLeerzeichen.Text = "Leerzeichen";
            // 
            // optKomma
            // 
            this.optKomma.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.optKomma.Location = new System.Drawing.Point(16, 96);
            this.optKomma.Name = "optKomma";
            this.optKomma.Size = new System.Drawing.Size(64, 16);
            this.optKomma.TabIndex = 3;
            this.optKomma.Text = "Komma";
            // 
            // capEinträge
            // 
            this.capEinträge.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.capEinträge.CausesValidation = false;
            this.capEinträge.Location = new System.Drawing.Point(8, 8);
            this.capEinträge.Name = "capEinträge";
            this.capEinträge.Size = new System.Drawing.Size(721, 24);
            this.capEinträge.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // chkTrennzeichenAmAnfang
            // 
            this.chkTrennzeichenAmAnfang.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkTrennzeichenAmAnfang.Location = new System.Drawing.Point(8, 56);
            this.chkTrennzeichenAmAnfang.Name = "chkTrennzeichenAmAnfang";
            this.chkTrennzeichenAmAnfang.Size = new System.Drawing.Size(224, 16);
            this.chkTrennzeichenAmAnfang.TabIndex = 10;
            this.chkTrennzeichenAmAnfang.Text = "Trennzeichen am Anfang entfernen";
            // 
            // grpSpaltenOptionen
            // 
            this.grpSpaltenOptionen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpSpaltenOptionen.CausesValidation = false;
            this.grpSpaltenOptionen.Controls.Add(this.optSpalteNachderReihe);
            this.grpSpaltenOptionen.Controls.Add(this.optSpalteZuordnen);
            this.grpSpaltenOptionen.Location = new System.Drawing.Point(192, 88);
            this.grpSpaltenOptionen.Name = "grpSpaltenOptionen";
            this.grpSpaltenOptionen.Size = new System.Drawing.Size(532, 72);
            this.grpSpaltenOptionen.TabIndex = 12;
            this.grpSpaltenOptionen.TabStop = false;
            this.grpSpaltenOptionen.Text = "Spalten-Optionen";
            // 
            // optSpalteNachderReihe
            // 
            this.optSpalteNachderReihe.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.optSpalteNachderReihe.Location = new System.Drawing.Point(8, 40);
            this.optSpalteNachderReihe.Name = "optSpalteNachderReihe";
            this.optSpalteNachderReihe.Size = new System.Drawing.Size(512, 24);
            this.optSpalteNachderReihe.TabIndex = 1;
            this.optSpalteNachderReihe.Text = "Alle Einträge importieren, <b>interne Spaltenreihenfolge</b> benutzen und nichts " +
    "zuordnen.";
            // 
            // optSpalteZuordnen
            // 
            this.optSpalteZuordnen.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.optSpalteZuordnen.Checked = true;
            this.optSpalteZuordnen.Location = new System.Drawing.Point(8, 16);
            this.optSpalteZuordnen.Name = "optSpalteZuordnen";
            this.optSpalteZuordnen.Size = new System.Drawing.Size(512, 24);
            this.optSpalteZuordnen.TabIndex = 0;
            this.optSpalteZuordnen.Text = "Die <b>erste Zeile</b> enthält den Spaltennamen und zu diesen zuordnen.";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.ImageCode = "Kreuz|16";
            this.btnCancel.Location = new System.Drawing.Point(509, 264);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(104, 32);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Abbrechen";
            this.btnCancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // grpZeilenOptionen
            // 
            this.grpZeilenOptionen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpZeilenOptionen.CausesValidation = false;
            this.grpZeilenOptionen.Controls.Add(this.optZeilenAlle);
            this.grpZeilenOptionen.Controls.Add(this.optZeilenZuorden);
            this.grpZeilenOptionen.Location = new System.Drawing.Point(192, 168);
            this.grpZeilenOptionen.Name = "grpZeilenOptionen";
            this.grpZeilenOptionen.Size = new System.Drawing.Size(532, 88);
            this.grpZeilenOptionen.TabIndex = 0;
            this.grpZeilenOptionen.TabStop = false;
            this.grpZeilenOptionen.Text = "Zeilen-Optionen";
            // 
            // optZeilenAlle
            // 
            this.optZeilenAlle.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.optZeilenAlle.Location = new System.Drawing.Point(8, 56);
            this.optZeilenAlle.Name = "optZeilenAlle";
            this.optZeilenAlle.Size = new System.Drawing.Size(512, 24);
            this.optZeilenAlle.TabIndex = 1;
            this.optZeilenAlle.Text = "<b>Jede Zeile</b> importieren, auch wenn dadurch <b>doppelte</b> Einträge entsteh" +
    "en.";
            // 
            // optZeilenZuorden
            // 
            this.optZeilenZuorden.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.optZeilenZuorden.Checked = true;
            this.optZeilenZuorden.Location = new System.Drawing.Point(8, 24);
            this.optZeilenZuorden.Name = "optZeilenZuorden";
            this.optZeilenZuorden.Size = new System.Drawing.Size(512, 32);
            this.optZeilenZuorden.TabIndex = 0;
            this.optZeilenZuorden.Text = "Die <b>erste Spalte</b> des Imports soll der <b>ersten Spalte</b> der Datenbank z" +
    "ugeordnet werden. <br>Wenn der Eintrag nicht in der Datenbank vorhanden ist, wir" +
    "d eine neue Zeile erstellt.";
            // 
            // ImportCsv
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(731, 326);
            this.Controls.Add(this.grpZeilenOptionen);
            this.Controls.Add(this.grpTrennzeichen);
            this.Controls.Add(this.grpSpaltenOptionen);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.chkTrennzeichenAmAnfang);
            this.Controls.Add(this.btnImportieren);
            this.Controls.Add(this.chkDoppelteTrennzeichen);
            this.Controls.Add(this.capEinträge);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ImportCsv";
            this.ShowInTaskbar = false;
            this.Text = "Einträge importieren:";
            this.Controls.SetChildIndex(this.capEinträge, 0);
            this.Controls.SetChildIndex(this.chkDoppelteTrennzeichen, 0);
            this.Controls.SetChildIndex(this.btnImportieren, 0);
            this.Controls.SetChildIndex(this.chkTrennzeichenAmAnfang, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.grpSpaltenOptionen, 0);
            this.Controls.SetChildIndex(this.grpTrennzeichen, 0);
            this.Controls.SetChildIndex(this.grpZeilenOptionen, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.grpTrennzeichen.ResumeLayout(false);
            this.grpSpaltenOptionen.ResumeLayout(false);
            this.grpZeilenOptionen.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Caption capEinträge;
        private Button optTabStopp;
        private Button optSemikolon;
        private Button optKomma;
        private Button optLeerzeichen;
        private Button optAndere;
        private GroupBox grpTrennzeichen;
        private TextBox txtAndere;
        private Button chkDoppelteTrennzeichen;
        private Button btnImportieren;
        private Button chkTrennzeichenAmAnfang;
        private GroupBox grpSpaltenOptionen;
        private Button btnCancel;
        internal Button optSpalteNachderReihe;
        private Button optSpalteZuordnen;
        internal GroupBox grpZeilenOptionen;
        internal Button optZeilenAlle;
        internal Button optZeilenZuorden;
    }
}
