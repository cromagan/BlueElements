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
    internal sealed partial class SearchAndReplaceInCells
        {
			//Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
			[DebuggerNonUserCode()]
			protected override void Dispose(bool disposing)
			{
				if (disposing )
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
            this.optSucheExact = new BlueControls.Controls.Button();
            this.chkNurinAktuellerSpalte = new BlueControls.Controls.Button();
            this.optErsetzeMit = new BlueControls.Controls.Button();
            this.chkAktuelleFilterung = new BlueControls.Controls.Button();
            this.btnAusfuehren = new BlueControls.Controls.Button();
            this.txbNeu = new BlueControls.Controls.TextBox();
            this.txbAlt = new BlueControls.Controls.TextBox();
            this.optSucheNach = new BlueControls.Controls.Button();
            this.grpSuche = new BlueControls.Controls.GroupBox();
            this.optInhaltEgal = new BlueControls.Controls.Button();
            this.grpErsetzen = new BlueControls.Controls.GroupBox();
            this.optFügeHinzu = new BlueControls.Controls.Button();
            this.optErsetzeKomplett = new BlueControls.Controls.Button();
            this.grpOptionen = new BlueControls.Controls.GroupBox();
            this.chkAbgeschlosseZellen = new BlueControls.Controls.Button();
            this.grpSonderzeichen = new BlueControls.Controls.GroupBox();
            this.capSonderzeichen = new BlueControls.Controls.Caption();
            this.grpSuche.SuspendLayout();
            this.grpErsetzen.SuspendLayout();
            this.grpOptionen.SuspendLayout();
            this.grpSonderzeichen.SuspendLayout();
            this.SuspendLayout();
            // 
            // optSucheExact
            // 
            this.optSucheExact.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.optSucheExact.Location = new System.Drawing.Point(8, 40);
            this.optSucheExact.Name = "optSucheExact";
            this.optSucheExact.QuickInfo = "Trifft zu, wenn der eingegebene Text<br><b>exact dem Zelleninhalt</b>entspricht.";
            this.optSucheExact.Size = new System.Drawing.Size(104, 16);
            this.optSucheExact.TabIndex = 11;
            this.optSucheExact.Text = "Suche exakt";
            this.optSucheExact.CheckedChanged += new System.EventHandler(this.Something_CheckedChanged);
            // 
            // chkNurinAktuellerSpalte
            // 
            this.chkNurinAktuellerSpalte.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkNurinAktuellerSpalte.Checked = true;
            this.chkNurinAktuellerSpalte.Location = new System.Drawing.Point(16, 32);
            this.chkNurinAktuellerSpalte.Name = "chkNurinAktuellerSpalte";
            this.chkNurinAktuellerSpalte.Size = new System.Drawing.Size(312, 18);
            this.chkNurinAktuellerSpalte.TabIndex = 10;
            this.chkNurinAktuellerSpalte.Text = "Nur in aktueller gewählter Spalte ersetzen";
            this.chkNurinAktuellerSpalte.CheckedChanged += new System.EventHandler(this.Something_CheckedChanged);
            // 
            // optErsetzeMit
            // 
            this.optErsetzeMit.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.optErsetzeMit.Checked = true;
            this.optErsetzeMit.Location = new System.Drawing.Point(8, 16);
            this.optErsetzeMit.Name = "optErsetzeMit";
            this.optErsetzeMit.QuickInfo = "Ersetzt die gefundene <b>Textpassage</b><br>mit dem eingegeben Text.";
            this.optErsetzeMit.Size = new System.Drawing.Size(128, 16);
            this.optErsetzeMit.TabIndex = 9;
            this.optErsetzeMit.Text = "Ersetze mit";
            this.optErsetzeMit.CheckedChanged += new System.EventHandler(this.Something_CheckedChanged);
            // 
            // chkAktuelleFilterung
            // 
            this.chkAktuelleFilterung.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAktuelleFilterung.Checked = true;
            this.chkAktuelleFilterung.Location = new System.Drawing.Point(16, 16);
            this.chkAktuelleFilterung.Name = "chkAktuelleFilterung";
            this.chkAktuelleFilterung.Size = new System.Drawing.Size(314, 16);
            this.chkAktuelleFilterung.TabIndex = 7;
            this.chkAktuelleFilterung.Text = "Aktuelle Filterung (und Pin) berücksichtigen";
            this.chkAktuelleFilterung.CheckedChanged += new System.EventHandler(this.Something_CheckedChanged);
            // 
            // btnAusfuehren
            // 
            this.btnAusfuehren.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAusfuehren.Enabled = false;
            this.btnAusfuehren.ImageCode = "Häkchen|24";
            this.btnAusfuehren.Location = new System.Drawing.Point(458, 284);
            this.btnAusfuehren.Name = "btnAusfuehren";
            this.btnAusfuehren.Size = new System.Drawing.Size(120, 32);
            this.btnAusfuehren.TabIndex = 4;
            this.btnAusfuehren.Text = "Ausführen";
            this.btnAusfuehren.Click += new System.EventHandler(this.ers_Click);
            // 
            // txbNeu
            // 
            this.txbNeu.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbNeu.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbNeu.Location = new System.Drawing.Point(144, 16);
            this.txbNeu.Name = "txbNeu";
            this.txbNeu.Size = new System.Drawing.Size(419, 80);
            this.txbNeu.TabIndex = 3;
            this.txbNeu.TextChanged += new System.EventHandler(this.AltNeu_TextChanged);
            // 
            // txbAlt
            // 
            this.txbAlt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbAlt.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbAlt.Location = new System.Drawing.Point(144, 16);
            this.txbAlt.Name = "txbAlt";
            this.txbAlt.Size = new System.Drawing.Size(419, 64);
            this.txbAlt.TabIndex = 2;
            this.txbAlt.TextChanged += new System.EventHandler(this.AltNeu_TextChanged);
            // 
            // optSucheNach
            // 
            this.optSucheNach.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.optSucheNach.Checked = true;
            this.optSucheNach.Location = new System.Drawing.Point(8, 16);
            this.optSucheNach.Name = "optSucheNach";
            this.optSucheNach.QuickInfo = "Trifft zu, wenn die eingegebene<br><b>Textpassage</b>enthalten ist.";
            this.optSucheNach.Size = new System.Drawing.Size(96, 16);
            this.optSucheNach.TabIndex = 13;
            this.optSucheNach.Text = "Suche nach";
            this.optSucheNach.CheckedChanged += new System.EventHandler(this.Something_CheckedChanged);
            // 
            // grpSuche
            // 
            this.grpSuche.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSuche.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpSuche.CausesValidation = false;
            this.grpSuche.Controls.Add(this.optInhaltEgal);
            this.grpSuche.Controls.Add(this.optSucheNach);
            this.grpSuche.Controls.Add(this.optSucheExact);
            this.grpSuche.Controls.Add(this.txbAlt);
            this.grpSuche.Location = new System.Drawing.Point(8, 8);
            this.grpSuche.Name = "grpSuche";
            this.grpSuche.Size = new System.Drawing.Size(571, 88);
            this.grpSuche.TabIndex = 3;
            this.grpSuche.TabStop = false;
            this.grpSuche.Text = "Suche";
            // 
            // optInhaltEgal
            // 
            this.optInhaltEgal.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.optInhaltEgal.Location = new System.Drawing.Point(8, 64);
            this.optInhaltEgal.Name = "optInhaltEgal";
            this.optInhaltEgal.QuickInfo = "Trifft immer zu";
            this.optInhaltEgal.Size = new System.Drawing.Size(96, 16);
            this.optInhaltEgal.TabIndex = 14;
            this.optInhaltEgal.Text = "Inhalt egal";
            this.optInhaltEgal.CheckedChanged += new System.EventHandler(this.Something_CheckedChanged);
            // 
            // grpErsetzen
            // 
            this.grpErsetzen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpErsetzen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpErsetzen.CausesValidation = false;
            this.grpErsetzen.Controls.Add(this.optFügeHinzu);
            this.grpErsetzen.Controls.Add(this.optErsetzeKomplett);
            this.grpErsetzen.Controls.Add(this.txbNeu);
            this.grpErsetzen.Controls.Add(this.optErsetzeMit);
            this.grpErsetzen.Location = new System.Drawing.Point(8, 96);
            this.grpErsetzen.Name = "grpErsetzen";
            this.grpErsetzen.Size = new System.Drawing.Size(571, 104);
            this.grpErsetzen.TabIndex = 2;
            this.grpErsetzen.TabStop = false;
            this.grpErsetzen.Text = "Ersetzen";
            // 
            // optFügeHinzu
            // 
            this.optFügeHinzu.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.optFügeHinzu.Location = new System.Drawing.Point(8, 80);
            this.optFügeHinzu.Name = "optFügeHinzu";
            this.optFügeHinzu.QuickInfo = "Fügt den Text als neuen Eintrag hinzu.";
            this.optFügeHinzu.Size = new System.Drawing.Size(128, 16);
            this.optFügeHinzu.TabIndex = 11;
            this.optFügeHinzu.Text = "Füge hinzu";
            // 
            // optErsetzeKomplett
            // 
            this.optErsetzeKomplett.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.optErsetzeKomplett.Location = new System.Drawing.Point(8, 40);
            this.optErsetzeKomplett.Name = "optErsetzeKomplett";
            this.optErsetzeKomplett.QuickInfo = "Ersetzt den <b>kompletten Zelleninhalt</b><br>mit dem eingegebenen Text.";
            this.optErsetzeKomplett.Size = new System.Drawing.Size(128, 32);
            this.optErsetzeKomplett.TabIndex = 10;
            this.optErsetzeKomplett.Text = "Ersetze kompletten Inhalt mit";
            this.optErsetzeKomplett.CheckedChanged += new System.EventHandler(this.Something_CheckedChanged);
            // 
            // grpOptionen
            // 
            this.grpOptionen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.grpOptionen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpOptionen.CausesValidation = false;
            this.grpOptionen.Controls.Add(this.chkAbgeschlosseZellen);
            this.grpOptionen.Controls.Add(this.chkNurinAktuellerSpalte);
            this.grpOptionen.Controls.Add(this.chkAktuelleFilterung);
            this.grpOptionen.Location = new System.Drawing.Point(8, 200);
            this.grpOptionen.Name = "grpOptionen";
            this.grpOptionen.Size = new System.Drawing.Size(336, 77);
            this.grpOptionen.TabIndex = 1;
            this.grpOptionen.TabStop = false;
            this.grpOptionen.Text = "Optionen";
            // 
            // chkAbgeschlosseZellen
            // 
            this.chkAbgeschlosseZellen.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAbgeschlosseZellen.Location = new System.Drawing.Point(16, 48);
            this.chkAbgeschlosseZellen.Name = "chkAbgeschlosseZellen";
            this.chkAbgeschlosseZellen.Size = new System.Drawing.Size(312, 18);
            this.chkAbgeschlosseZellen.TabIndex = 11;
            this.chkAbgeschlosseZellen.Text = "Abgeschlosse Zeilen überspringen";
            this.chkAbgeschlosseZellen.CheckedChanged += new System.EventHandler(this.Something_CheckedChanged);
            // 
            // grpSonderzeichen
            // 
            this.grpSonderzeichen.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSonderzeichen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpSonderzeichen.CausesValidation = false;
            this.grpSonderzeichen.Controls.Add(this.capSonderzeichen);
            this.grpSonderzeichen.Location = new System.Drawing.Point(344, 200);
            this.grpSonderzeichen.Name = "grpSonderzeichen";
            this.grpSonderzeichen.Size = new System.Drawing.Size(235, 77);
            this.grpSonderzeichen.TabIndex = 0;
            this.grpSonderzeichen.TabStop = false;
            this.grpSonderzeichen.Text = "Sonderzeichen";
            // 
            // capSonderzeichen
            // 
            this.capSonderzeichen.CausesValidation = false;
            this.capSonderzeichen.Location = new System.Drawing.Point(8, 16);
            this.capSonderzeichen.Name = "capSonderzeichen";
            this.capSonderzeichen.Size = new System.Drawing.Size(128, 32);
            this.capSonderzeichen.Text = "\\r = Zeilenumbruch<br>\\t = Tabulator";
            this.capSonderzeichen.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // SearchAndReplace
            // 
            this.ClientSize = new System.Drawing.Size(585, 319);
            this.Controls.Add(this.grpSonderzeichen);
            this.Controls.Add(this.grpOptionen);
            this.Controls.Add(this.grpErsetzen);
            this.Controls.Add(this.grpSuche);
            this.Controls.Add(this.btnAusfuehren);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SearchAndReplace";
            this.ShowInTaskbar = false;
            this.Text = "Suchen und Ersetzen";
            this.TopMost = true;
            this.grpSuche.ResumeLayout(false);
            this.grpErsetzen.ResumeLayout(false);
            this.grpOptionen.ResumeLayout(false);
            this.grpSonderzeichen.ResumeLayout(false);
            this.ResumeLayout(false);

			}
			private TextBox txbAlt;
			private TextBox txbNeu;
			private Button btnAusfuehren;
			private Button chkAktuelleFilterung;
			private Button optErsetzeMit;
			private Button chkNurinAktuellerSpalte;
			private Button optSucheExact;
			private Button optSucheNach;
			private GroupBox grpSuche;
			private Button optInhaltEgal;
			private GroupBox grpErsetzen;
			private Button optErsetzeKomplett;
			private GroupBox grpOptionen;
			private Button chkAbgeschlosseZellen;
			private Button optFügeHinzu;
        private GroupBox grpSonderzeichen;
        private Caption capSonderzeichen;
    }
	}
