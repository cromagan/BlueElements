using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueTableDialogs
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
            this.optSucheExact = new Button();
            this.chkNurinAktuellerSpalte = new Button();
            this.optErsetzeMit = new Button();
            this.chkAktuelleFilterung = new Button();
            this.btnAusfuehren = new Button();
            this.txbNeu = new TextBox();
            this.txbAlt = new TextBox();
            this.optSucheNach = new Button();
            this.grpSuche = new GroupBox();
            this.optInhaltEgal = new Button();
            this.grpErsetzen = new GroupBox();
            this.optFügeHinzu = new Button();
            this.optErsetzeKomplett = new Button();
            this.grpOptionen = new GroupBox();
            this.chkAbgeschlosseZellen = new Button();
            this.grpSonderzeichen = new GroupBox();
            this.capSonderzeichen = new Caption();
            this.grpSuche.SuspendLayout();
            this.grpErsetzen.SuspendLayout();
            this.grpOptionen.SuspendLayout();
            this.grpSonderzeichen.SuspendLayout();
            this.SuspendLayout();
            // 
            // optSucheExact
            // 
            this.optSucheExact.ButtonStyle = ButtonStyle.Optionbox_Text;
            this.optSucheExact.Location = new Point(8, 40);
            this.optSucheExact.Name = "optSucheExact";
            this.optSucheExact.QuickInfo = "Trifft zu, wenn der eingegebene Text<br><b>exact dem Zelleninhalt</b>entspricht.";
            this.optSucheExact.Size = new Size(104, 16);
            this.optSucheExact.TabIndex = 11;
            this.optSucheExact.Text = "Suche exakt";
            this.optSucheExact.CheckedChanged += new EventHandler(this.Something_CheckedChanged);
            // 
            // chkNurinAktuellerSpalte
            // 
            this.chkNurinAktuellerSpalte.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.chkNurinAktuellerSpalte.Checked = true;
            this.chkNurinAktuellerSpalte.Location = new Point(16, 32);
            this.chkNurinAktuellerSpalte.Name = "chkNurinAktuellerSpalte";
            this.chkNurinAktuellerSpalte.Size = new Size(312, 18);
            this.chkNurinAktuellerSpalte.TabIndex = 10;
            this.chkNurinAktuellerSpalte.Text = "Nur in aktueller gewählter Spalte ersetzen";
            this.chkNurinAktuellerSpalte.CheckedChanged += new EventHandler(this.Something_CheckedChanged);
            // 
            // optErsetzeMit
            // 
            this.optErsetzeMit.ButtonStyle = ButtonStyle.Optionbox_Text;
            this.optErsetzeMit.Checked = true;
            this.optErsetzeMit.Location = new Point(8, 16);
            this.optErsetzeMit.Name = "optErsetzeMit";
            this.optErsetzeMit.QuickInfo = "Ersetzt die gefundene <b>Textpassage</b><br>mit dem eingegeben Text.";
            this.optErsetzeMit.Size = new Size(128, 16);
            this.optErsetzeMit.TabIndex = 9;
            this.optErsetzeMit.Text = "Ersetze mit";
            this.optErsetzeMit.CheckedChanged += new EventHandler(this.Something_CheckedChanged);
            // 
            // chkAktuelleFilterung
            // 
            this.chkAktuelleFilterung.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.chkAktuelleFilterung.Checked = true;
            this.chkAktuelleFilterung.Location = new Point(16, 16);
            this.chkAktuelleFilterung.Name = "chkAktuelleFilterung";
            this.chkAktuelleFilterung.Size = new Size(314, 16);
            this.chkAktuelleFilterung.TabIndex = 7;
            this.chkAktuelleFilterung.Text = "Aktuelle Filterung (und Pin) berücksichtigen";
            this.chkAktuelleFilterung.CheckedChanged += new EventHandler(this.Something_CheckedChanged);
            // 
            // btnAusfuehren
            // 
            this.btnAusfuehren.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnAusfuehren.Enabled = false;
            this.btnAusfuehren.ImageCode = "Häkchen|24";
            this.btnAusfuehren.Location = new Point(458, 284);
            this.btnAusfuehren.Name = "btnAusfuehren";
            this.btnAusfuehren.Size = new Size(120, 32);
            this.btnAusfuehren.TabIndex = 4;
            this.btnAusfuehren.Text = "Ausführen";
            this.btnAusfuehren.Click += new EventHandler(this.ers_Click);
            // 
            // txbNeu
            // 
            this.txbNeu.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                   | AnchorStyles.Left) 
                                                  | AnchorStyles.Right)));
            this.txbNeu.Cursor = Cursors.IBeam;
            this.txbNeu.Location = new Point(144, 16);
            this.txbNeu.Name = "txbNeu";
            this.txbNeu.Size = new Size(419, 80);
            this.txbNeu.TabIndex = 3;
            this.txbNeu.TextChanged += new EventHandler(this.AltNeu_TextChanged);
            // 
            // txbAlt
            // 
            this.txbAlt.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                   | AnchorStyles.Left) 
                                                  | AnchorStyles.Right)));
            this.txbAlt.Cursor = Cursors.IBeam;
            this.txbAlt.Location = new Point(144, 16);
            this.txbAlt.Name = "txbAlt";
            this.txbAlt.Size = new Size(419, 64);
            this.txbAlt.TabIndex = 2;
            this.txbAlt.TextChanged += new EventHandler(this.AltNeu_TextChanged);
            // 
            // optSucheNach
            // 
            this.optSucheNach.ButtonStyle = ButtonStyle.Optionbox_Text;
            this.optSucheNach.Checked = true;
            this.optSucheNach.Location = new Point(8, 16);
            this.optSucheNach.Name = "optSucheNach";
            this.optSucheNach.QuickInfo = "Trifft zu, wenn die eingegebene<br><b>Textpassage</b>enthalten ist.";
            this.optSucheNach.Size = new Size(96, 16);
            this.optSucheNach.TabIndex = 13;
            this.optSucheNach.Text = "Suche nach";
            this.optSucheNach.CheckedChanged += new EventHandler(this.Something_CheckedChanged);
            // 
            // grpSuche
            // 
            this.grpSuche.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                    | AnchorStyles.Right)));
            this.grpSuche.BackColor = Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpSuche.CausesValidation = false;
            this.grpSuche.Controls.Add(this.optInhaltEgal);
            this.grpSuche.Controls.Add(this.optSucheNach);
            this.grpSuche.Controls.Add(this.optSucheExact);
            this.grpSuche.Controls.Add(this.txbAlt);
            this.grpSuche.Location = new Point(8, 8);
            this.grpSuche.Name = "grpSuche";
            this.grpSuche.Size = new Size(571, 88);
            this.grpSuche.TabIndex = 3;
            this.grpSuche.TabStop = false;
            this.grpSuche.Text = "Suche";
            // 
            // optInhaltEgal
            // 
            this.optInhaltEgal.ButtonStyle = ButtonStyle.Optionbox_Text;
            this.optInhaltEgal.Location = new Point(8, 64);
            this.optInhaltEgal.Name = "optInhaltEgal";
            this.optInhaltEgal.QuickInfo = "Trifft immer zu";
            this.optInhaltEgal.Size = new Size(96, 16);
            this.optInhaltEgal.TabIndex = 14;
            this.optInhaltEgal.Text = "Inhalt egal";
            this.optInhaltEgal.CheckedChanged += new EventHandler(this.Something_CheckedChanged);
            // 
            // grpErsetzen
            // 
            this.grpErsetzen.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                       | AnchorStyles.Right)));
            this.grpErsetzen.BackColor = Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpErsetzen.CausesValidation = false;
            this.grpErsetzen.Controls.Add(this.optFügeHinzu);
            this.grpErsetzen.Controls.Add(this.optErsetzeKomplett);
            this.grpErsetzen.Controls.Add(this.txbNeu);
            this.grpErsetzen.Controls.Add(this.optErsetzeMit);
            this.grpErsetzen.Location = new Point(8, 96);
            this.grpErsetzen.Name = "grpErsetzen";
            this.grpErsetzen.Size = new Size(571, 104);
            this.grpErsetzen.TabIndex = 2;
            this.grpErsetzen.TabStop = false;
            this.grpErsetzen.Text = "Ersetzen";
            // 
            // optFügeHinzu
            // 
            this.optFügeHinzu.ButtonStyle = ButtonStyle.Optionbox_Text;
            this.optFügeHinzu.Location = new Point(8, 80);
            this.optFügeHinzu.Name = "optFügeHinzu";
            this.optFügeHinzu.QuickInfo = "Fügt den Text als neuen Eintrag hinzu.";
            this.optFügeHinzu.Size = new Size(128, 16);
            this.optFügeHinzu.TabIndex = 11;
            this.optFügeHinzu.Text = "Füge hinzu";
            // 
            // optErsetzeKomplett
            // 
            this.optErsetzeKomplett.ButtonStyle = ButtonStyle.Optionbox_Text;
            this.optErsetzeKomplett.Location = new Point(8, 40);
            this.optErsetzeKomplett.Name = "optErsetzeKomplett";
            this.optErsetzeKomplett.QuickInfo = "Ersetzt den <b>kompletten Zelleninhalt</b><br>mit dem eingegebenen Text.";
            this.optErsetzeKomplett.Size = new Size(128, 32);
            this.optErsetzeKomplett.TabIndex = 10;
            this.optErsetzeKomplett.Text = "Ersetze kompletten Inhalt mit";
            this.optErsetzeKomplett.CheckedChanged += new EventHandler(this.Something_CheckedChanged);
            // 
            // grpOptionen
            // 
            this.grpOptionen.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                       | AnchorStyles.Left)));
            this.grpOptionen.BackColor = Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpOptionen.CausesValidation = false;
            this.grpOptionen.Controls.Add(this.chkAbgeschlosseZellen);
            this.grpOptionen.Controls.Add(this.chkNurinAktuellerSpalte);
            this.grpOptionen.Controls.Add(this.chkAktuelleFilterung);
            this.grpOptionen.Location = new Point(8, 200);
            this.grpOptionen.Name = "grpOptionen";
            this.grpOptionen.Size = new Size(336, 77);
            this.grpOptionen.TabIndex = 1;
            this.grpOptionen.TabStop = false;
            this.grpOptionen.Text = "Optionen";
            // 
            // chkAbgeschlosseZellen
            // 
            this.chkAbgeschlosseZellen.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.chkAbgeschlosseZellen.Location = new Point(16, 48);
            this.chkAbgeschlosseZellen.Name = "chkAbgeschlosseZellen";
            this.chkAbgeschlosseZellen.Size = new Size(312, 18);
            this.chkAbgeschlosseZellen.TabIndex = 11;
            this.chkAbgeschlosseZellen.Text = "Abgeschlosse Zeilen überspringen";
            this.chkAbgeschlosseZellen.CheckedChanged += new EventHandler(this.Something_CheckedChanged);
            // 
            // grpSonderzeichen
            // 
            this.grpSonderzeichen.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                             | AnchorStyles.Left) 
                                                            | AnchorStyles.Right)));
            this.grpSonderzeichen.BackColor = Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpSonderzeichen.CausesValidation = false;
            this.grpSonderzeichen.Controls.Add(this.capSonderzeichen);
            this.grpSonderzeichen.Location = new Point(344, 200);
            this.grpSonderzeichen.Name = "grpSonderzeichen";
            this.grpSonderzeichen.Size = new Size(235, 77);
            this.grpSonderzeichen.TabIndex = 0;
            this.grpSonderzeichen.TabStop = false;
            this.grpSonderzeichen.Text = "Sonderzeichen";
            // 
            // capSonderzeichen
            // 
            this.capSonderzeichen.CausesValidation = false;
            this.capSonderzeichen.Location = new Point(8, 16);
            this.capSonderzeichen.Name = "capSonderzeichen";
            this.capSonderzeichen.Size = new Size(128, 32);
            this.capSonderzeichen.Text = "\\r = Zeilenumbruch<br>\\t = Tabulator";
            // 
            // SearchAndReplace
            // 
            this.ClientSize = new Size(585, 319);
            this.Controls.Add(this.grpSonderzeichen);
            this.Controls.Add(this.grpOptionen);
            this.Controls.Add(this.grpErsetzen);
            this.Controls.Add(this.grpSuche);
            this.Controls.Add(this.btnAusfuehren);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
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
