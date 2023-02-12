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
    internal sealed partial class SearchAndReplace
        {
			//Das Formular �berschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
			[DebuggerNonUserCode()]
			protected override void Dispose(bool disposing)
			{
				if (disposing )
				{
				}
				base.Dispose(disposing);
			}
			//Hinweis: Die folgende Prozedur ist f�r den Windows Form-Designer erforderlich.
			//Das Bearbeiten ist mit dem Windows Form-Designer m�glich.  
			//Das Bearbeiten mit dem Code-Editor ist nicht m�glich.
			[DebuggerStepThrough()]
			private void InitializeComponent()
			{
            this.SucheExact = new Button();
            this.NurinAktuellerSpalte = new Button();
            this.ErsetzeMit = new Button();
            this.AktuelleFilterung = new Button();
            this.ers = new Button();
            this.Neu = new TextBox();
            this.Alt = new TextBox();
            this.SucheNach = new Button();
            this.Suchen = new GroupBox();
            this.InhaltEgal = new Button();
            this.BlueFrame1 = new GroupBox();
            this.F�geHinzu = new Button();
            this.ErsetzeKomplett = new Button();
            this.BlueFrame2 = new GroupBox();
            this.AbgeschlosseZellen = new Button();
            this.grpSonderzeichen = new GroupBox();
            this.capSonderzeichen = new Caption();
            this.Suchen.SuspendLayout();
            this.BlueFrame1.SuspendLayout();
            this.BlueFrame2.SuspendLayout();
            this.grpSonderzeichen.SuspendLayout();
            this.SuspendLayout();
            // 
            // SucheExact
            // 
            this.SucheExact.ButtonStyle = ((ButtonStyle)((ButtonStyle.Optionbox | ButtonStyle.Text)));
            this.SucheExact.Location = new Point(8, 40);
            this.SucheExact.Name = "SucheExact";
            this.SucheExact.QuickInfo = "Trifft zu, wenn der eingegebene Text<br><b>exact dem Zelleninhalt</b>entspricht.";
            this.SucheExact.Size = new Size(104, 16);
            this.SucheExact.TabIndex = 11;
            this.SucheExact.Text = "Suche exakt";
            this.SucheExact.CheckedChanged += new EventHandler(this.Something_CheckedChanged);
            // 
            // NurinAktuellerSpalte
            // 
            this.NurinAktuellerSpalte.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.NurinAktuellerSpalte.Checked = true;
            this.NurinAktuellerSpalte.Location = new Point(16, 32);
            this.NurinAktuellerSpalte.Name = "NurinAktuellerSpalte";
            this.NurinAktuellerSpalte.Size = new Size(312, 18);
            this.NurinAktuellerSpalte.TabIndex = 10;
            this.NurinAktuellerSpalte.Text = "Nur in aktueller gew�hlter Spalte ersetzen";
            this.NurinAktuellerSpalte.CheckedChanged += new EventHandler(this.Something_CheckedChanged);
            // 
            // ErsetzeMit
            // 
            this.ErsetzeMit.ButtonStyle = ((ButtonStyle)((ButtonStyle.Optionbox | ButtonStyle.Text)));
            this.ErsetzeMit.Checked = true;
            this.ErsetzeMit.Location = new Point(8, 16);
            this.ErsetzeMit.Name = "ErsetzeMit";
            this.ErsetzeMit.QuickInfo = "Ersetzt die gefundene <b>Textpassage</b><br>mit dem eingegeben Text.";
            this.ErsetzeMit.Size = new Size(128, 16);
            this.ErsetzeMit.TabIndex = 9;
            this.ErsetzeMit.Text = "Ersetze mit";
            this.ErsetzeMit.CheckedChanged += new EventHandler(this.Something_CheckedChanged);
            // 
            // AktuelleFilterung
            // 
            this.AktuelleFilterung.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.AktuelleFilterung.Checked = true;
            this.AktuelleFilterung.Location = new Point(16, 16);
            this.AktuelleFilterung.Name = "AktuelleFilterung";
            this.AktuelleFilterung.Size = new Size(314, 16);
            this.AktuelleFilterung.TabIndex = 7;
            this.AktuelleFilterung.Text = "Aktuelle Filterung ber�cksichtigen";
            this.AktuelleFilterung.CheckedChanged += new EventHandler(this.Something_CheckedChanged);
            // 
            // ers
            // 
            this.ers.Enabled = false;
            this.ers.Location = new Point(496, 288);
            this.ers.Name = "ers";
            this.ers.Size = new Size(80, 32);
            this.ers.TabIndex = 4;
            this.ers.Text = "Ausf�hren";
            this.ers.Click += new EventHandler(this.ers_Click);
            // 
            // Neu
            // 
            this.Neu.Cursor = Cursors.IBeam;
            this.Neu.Location = new Point(144, 16);
            this.Neu.Name = "Neu";
            this.Neu.Size = new Size(416, 48);
            this.Neu.TabIndex = 3;
            this.Neu.TextChanged += new EventHandler(this.Alt_TextChange);
            // 
            // Alt
            // 
            this.Alt.Cursor = Cursors.IBeam;
            this.Alt.Location = new Point(144, 16);
            this.Alt.Name = "Alt";
            this.Alt.Size = new Size(416, 48);
            this.Alt.TabIndex = 2;
            this.Alt.TextChanged += new EventHandler(this.Alt_TextChange);
            // 
            // SucheNach
            // 
            this.SucheNach.ButtonStyle = ((ButtonStyle)((ButtonStyle.Optionbox | ButtonStyle.Text)));
            this.SucheNach.Checked = true;
            this.SucheNach.Location = new Point(8, 16);
            this.SucheNach.Name = "SucheNach";
            this.SucheNach.QuickInfo = "Trifft zu, wenn die eingegebene<br><b>Textpassage</b>enthalten ist.";
            this.SucheNach.Size = new Size(96, 16);
            this.SucheNach.TabIndex = 13;
            this.SucheNach.Text = "Suche nach";
            this.SucheNach.CheckedChanged += new EventHandler(this.Something_CheckedChanged);
            // 
            // Suchen
            // 
            this.Suchen.BackColor = Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.Suchen.CausesValidation = false;
            this.Suchen.Controls.Add(this.InhaltEgal);
            this.Suchen.Controls.Add(this.SucheNach);
            this.Suchen.Controls.Add(this.SucheExact);
            this.Suchen.Controls.Add(this.Alt);
            this.Suchen.Location = new Point(8, 8);
            this.Suchen.Name = "Suchen";
            this.Suchen.Size = new Size(568, 88);
            this.Suchen.TabIndex = 3;
            this.Suchen.TabStop = false;
            this.Suchen.Text = "Suche";
            // 
            // InhaltEgal
            // 
            this.InhaltEgal.ButtonStyle = ((ButtonStyle)((ButtonStyle.Optionbox | ButtonStyle.Text)));
            this.InhaltEgal.Location = new Point(8, 64);
            this.InhaltEgal.Name = "InhaltEgal";
            this.InhaltEgal.QuickInfo = "Trifft immer zu";
            this.InhaltEgal.Size = new Size(96, 16);
            this.InhaltEgal.TabIndex = 14;
            this.InhaltEgal.Text = "Inhalt egal";
            this.InhaltEgal.CheckedChanged += new EventHandler(this.Something_CheckedChanged);
            // 
            // BlueFrame1
            // 
            this.BlueFrame1.BackColor = Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.BlueFrame1.CausesValidation = false;
            this.BlueFrame1.Controls.Add(this.F�geHinzu);
            this.BlueFrame1.Controls.Add(this.ErsetzeKomplett);
            this.BlueFrame1.Controls.Add(this.Neu);
            this.BlueFrame1.Controls.Add(this.ErsetzeMit);
            this.BlueFrame1.Location = new Point(8, 96);
            this.BlueFrame1.Name = "BlueFrame1";
            this.BlueFrame1.Size = new Size(568, 104);
            this.BlueFrame1.TabIndex = 2;
            this.BlueFrame1.TabStop = false;
            this.BlueFrame1.Text = "Ersetzen";
            // 
            // F�geHinzu
            // 
            this.F�geHinzu.ButtonStyle = ((ButtonStyle)((ButtonStyle.Optionbox | ButtonStyle.Text)));
            this.F�geHinzu.Location = new Point(8, 80);
            this.F�geHinzu.Name = "F�geHinzu";
            this.F�geHinzu.QuickInfo = "F�gt den Text als neuen Eintrag hinzu.";
            this.F�geHinzu.Size = new Size(128, 16);
            this.F�geHinzu.TabIndex = 11;
            this.F�geHinzu.Text = "F�ge hinzu";
            // 
            // ErsetzeKomplett
            // 
            this.ErsetzeKomplett.ButtonStyle = ((ButtonStyle)((ButtonStyle.Optionbox | ButtonStyle.Text)));
            this.ErsetzeKomplett.Location = new Point(8, 40);
            this.ErsetzeKomplett.Name = "ErsetzeKomplett";
            this.ErsetzeKomplett.QuickInfo = "Ersetzt den <b>kompletten Zelleninhalt</b><br>mit dem eingegebenen Text.";
            this.ErsetzeKomplett.Size = new Size(128, 32);
            this.ErsetzeKomplett.TabIndex = 10;
            this.ErsetzeKomplett.Text = "Ersetze kompletten Inhalt mit";
            this.ErsetzeKomplett.CheckedChanged += new EventHandler(this.Something_CheckedChanged);
            // 
            // BlueFrame2
            // 
            this.BlueFrame2.BackColor = Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.BlueFrame2.CausesValidation = false;
            this.BlueFrame2.Controls.Add(this.AbgeschlosseZellen);
            this.BlueFrame2.Controls.Add(this.NurinAktuellerSpalte);
            this.BlueFrame2.Controls.Add(this.AktuelleFilterung);
            this.BlueFrame2.Location = new Point(8, 200);
            this.BlueFrame2.Name = "BlueFrame2";
            this.BlueFrame2.Size = new Size(336, 80);
            this.BlueFrame2.TabIndex = 1;
            this.BlueFrame2.TabStop = false;
            this.BlueFrame2.Text = "Optionen";
            // 
            // AbgeschlosseZellen
            // 
            this.AbgeschlosseZellen.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.AbgeschlosseZellen.Checked = true;
            this.AbgeschlosseZellen.Location = new Point(16, 48);
            this.AbgeschlosseZellen.Name = "AbgeschlosseZellen";
            this.AbgeschlosseZellen.Size = new Size(312, 18);
            this.AbgeschlosseZellen.TabIndex = 11;
            this.AbgeschlosseZellen.Text = "Abgeschlosse Zeilen �berspringen";
            this.AbgeschlosseZellen.CheckedChanged += new EventHandler(this.Something_CheckedChanged);
            // 
            // grpSonderzeichen
            // 
            this.grpSonderzeichen.BackColor = Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpSonderzeichen.CausesValidation = false;
            this.grpSonderzeichen.Controls.Add(this.capSonderzeichen);
            this.grpSonderzeichen.Location = new Point(344, 200);
            this.grpSonderzeichen.Name = "grpSonderzeichen";
            this.grpSonderzeichen.Size = new Size(232, 80);
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
            this.capSonderzeichen.Text = ";cr; = Zeilenumbruch<br>;tab; = Tabulator";
            this.capSonderzeichen.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // SearchAndReplace
            // 
            this.ClientSize = new Size(583, 331);
            this.Controls.Add(this.grpSonderzeichen);
            this.Controls.Add(this.BlueFrame2);
            this.Controls.Add(this.BlueFrame1);
            this.Controls.Add(this.Suchen);
            this.Controls.Add(this.ers);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Name = "SearchAndReplace";
            this.ShowInTaskbar = false;
            this.Text = "Suchen und Ersetzen";
            this.TopMost = true;
            this.Suchen.ResumeLayout(false);
            this.BlueFrame1.ResumeLayout(false);
            this.BlueFrame2.ResumeLayout(false);
            this.grpSonderzeichen.ResumeLayout(false);
            this.ResumeLayout(false);

			}
			private TextBox Alt;
			private TextBox Neu;
			private Button ers;
			private Button AktuelleFilterung;
			private Button ErsetzeMit;
			private Button NurinAktuellerSpalte;
			private Button SucheExact;
			private Button SucheNach;
			private GroupBox Suchen;
			private Button InhaltEgal;
			private GroupBox BlueFrame1;
			private Button ErsetzeKomplett;
			private GroupBox BlueFrame2;
			private Button AbgeschlosseZellen;
			private Button F�geHinzu;
        private GroupBox grpSonderzeichen;
        private Caption capSonderzeichen;
    }
	}
