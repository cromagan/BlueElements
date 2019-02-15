using System;
using System.Diagnostics;
using System.Drawing;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;

namespace BlueControls.BlueDatabaseDialogs
    {


        internal sealed partial class SearchAndReplace : Form
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
            this.SucheExact = new  Button();
            this.NurinAktuellerSpalte = new  Button();
            this.ErsetzeMit = new  Button();
            this.AktuelleFilterung = new  Button();
            this.Caption3 = new  Caption();
            this.ers = new  Button();
            this.Neu = new  TextBox();
            this.Alt = new  TextBox();
            this.SucheNach = new  Button();
            this.Suchen = new  GroupBox();
            this.InhaltEgal = new  Button();
            this.BlueFrame1 = new  GroupBox();
            this.FügeHinzu = new  Button();
            this.ErsetzeKomplett = new  Button();
            this.BlueFrame2 = new  GroupBox();
            this.AbgeschlosseZellen = new  Button();
            this.BlueFrame3 = new  GroupBox();
            this.Suchen.SuspendLayout();
            this.BlueFrame1.SuspendLayout();
            this.BlueFrame2.SuspendLayout();
            this.BlueFrame3.SuspendLayout();
            this.SuspendLayout();
            // 
            // SucheExact
            // 
            this.SucheExact.ButtonStyle = enButtonStyle.Optionbox_Text;
            this.SucheExact.Location = new Point(24, 40);
            this.SucheExact.Name = "SucheExact";
            this.SucheExact.QuickInfo = "Trifft zu, wenn der eingegebene Text<br><b>exact dem Zelleninhalt</b>entspricht.";
            this.SucheExact.Size = new Size(104, 16);
            this.SucheExact.TabIndex = 11;
            this.SucheExact.Text = "Suche exakt";
            this.SucheExact.CheckedChanged += new  EventHandler(this.Something_CheckedChanged);
            // 
            // NurinAktuellerSpalte
            // 
            this.NurinAktuellerSpalte.ButtonStyle = enButtonStyle.Checkbox_Text;
            this.NurinAktuellerSpalte.Checked = true;
            this.NurinAktuellerSpalte.Location = new Point(16, 32);
            this.NurinAktuellerSpalte.Name = "NurinAktuellerSpalte";
            this.NurinAktuellerSpalte.Size = new Size(312, 18);
            this.NurinAktuellerSpalte.TabIndex = 10;
            this.NurinAktuellerSpalte.Text = "Nur in aktueller gewählter Spalte ersetzen";
            this.NurinAktuellerSpalte.CheckedChanged += new  EventHandler(this.Something_CheckedChanged);
            // 
            // ErsetzeMit
            // 
            this.ErsetzeMit.ButtonStyle = enButtonStyle.Optionbox_Text;
            this.ErsetzeMit.Checked = true;
            this.ErsetzeMit.Location = new Point(8, 16);
            this.ErsetzeMit.Name = "ErsetzeMit";
            this.ErsetzeMit.QuickInfo = "Ersetzt die gefundene <b>Textpassage</b><br>mit dem eingegeben Text.";
            this.ErsetzeMit.Size = new Size(128, 16);
            this.ErsetzeMit.TabIndex = 9;
            this.ErsetzeMit.Text = "Ersetze mit";
            this.ErsetzeMit.CheckedChanged += new  EventHandler(this.Something_CheckedChanged);
            // 
            // AktuelleFilterung
            // 
            this.AktuelleFilterung.ButtonStyle = enButtonStyle.Checkbox_Text;
            this.AktuelleFilterung.Checked = true;
            this.AktuelleFilterung.Location = new Point(16, 16);
            this.AktuelleFilterung.Name = "AktuelleFilterung";
            this.AktuelleFilterung.Size = new Size(314, 16);
            this.AktuelleFilterung.TabIndex = 7;
            this.AktuelleFilterung.Text = "Aktuelle Filterung berücksichtigen";
            this.AktuelleFilterung.CheckedChanged += new  EventHandler(this.Something_CheckedChanged);
            // 
            // Caption3
            // 
            this.Caption3.CausesValidation = false;
            this.Caption3.Location = new Point(8, 16);
            this.Caption3.Name = "Caption3";
            this.Caption3.Size = new Size(176, 48);
            this.Caption3.Text = "<b><u>Sonder-Werte:</b></u><br>;cr; = Zeilenumbruch";
            this.Caption3.TextAnzeigeVerhalten = enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // ers
            // 
            this.ers.Enabled = false;
            this.ers.Location = new Point(496, 304);
            this.ers.Name = "ers";
            this.ers.Size = new Size(80, 32);
            this.ers.TabIndex = 4;
            this.ers.Text = "Ausführen";
            this.ers.Click += new EventHandler(this.ers_Click);
            // 
            // Neu
            // 
            this.Neu.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Neu.Location = new Point(144, 8);
            this.Neu.Name = "Neu";
            this.Neu.Size = new Size(416, 40);
            this.Neu.TabIndex = 3;
            this.Neu.TextChanged += new EventHandler(this.Alt_TextChange);
            // 
            // Alt
            // 
            this.Alt.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Alt.Location = new Point(136, 16);
            this.Alt.Name = "Alt";
            this.Alt.Size = new Size(424, 40);
            this.Alt.TabIndex = 2;
            this.Alt.TextChanged += new EventHandler(this.Alt_TextChange);
            // 
            // SucheNach
            // 
            this.SucheNach.ButtonStyle = enButtonStyle.Optionbox_Text;
            this.SucheNach.Checked = true;
            this.SucheNach.Location = new Point(24, 16);
            this.SucheNach.Name = "SucheNach";
            this.SucheNach.QuickInfo = "Trifft zu, wenn die eingegebene<br><b>Textpassage</b>enthalten ist.";
            this.SucheNach.Size = new Size(96, 16);
            this.SucheNach.TabIndex = 13;
            this.SucheNach.Text = "Suche nach";
            this.SucheNach.CheckedChanged += new  EventHandler(this.Something_CheckedChanged);
            // 
            // Suchen
            // 
            this.Suchen.CausesValidation = false;
            this.Suchen.Controls.Add(this.InhaltEgal);
            this.Suchen.Controls.Add(this.SucheNach);
            this.Suchen.Controls.Add(this.SucheExact);
            this.Suchen.Controls.Add(this.Alt);
            this.Suchen.Location = new Point(8, 8);
            this.Suchen.Name = "Suchen";
            this.Suchen.Size = new Size(568, 96);
            this.Suchen.Text = "Suche";
            // 
            // InhaltEgal
            // 
            this.InhaltEgal.ButtonStyle = enButtonStyle.Optionbox_Text;
            this.InhaltEgal.Location = new Point(24, 72);
            this.InhaltEgal.Name = "InhaltEgal";
            this.InhaltEgal.QuickInfo = "Trifft immer zu";
            this.InhaltEgal.Size = new Size(96, 16);
            this.InhaltEgal.TabIndex = 14;
            this.InhaltEgal.Text = "Inhalt egal";
            this.InhaltEgal.CheckedChanged += new  EventHandler(this.Something_CheckedChanged);
            // 
            // BlueFrame1
            // 
            this.BlueFrame1.CausesValidation = false;
            this.BlueFrame1.Controls.Add(this.FügeHinzu);
            this.BlueFrame1.Controls.Add(this.ErsetzeKomplett);
            this.BlueFrame1.Controls.Add(this.Neu);
            this.BlueFrame1.Controls.Add(this.ErsetzeMit);
            this.BlueFrame1.Location = new Point(8, 104);
            this.BlueFrame1.Name = "BlueFrame1";
            this.BlueFrame1.Size = new Size(568, 112);
            this.BlueFrame1.Text = "Ersetzen";
            // 
            // FügeHinzu
            // 
            this.FügeHinzu.ButtonStyle = enButtonStyle.Optionbox_Text;
            this.FügeHinzu.Location = new Point(8, 88);
            this.FügeHinzu.Name = "FügeHinzu";
            this.FügeHinzu.QuickInfo = "Fügt den Text als neuen Eintrag hinzu.";
            this.FügeHinzu.Size = new Size(128, 16);
            this.FügeHinzu.TabIndex = 11;
            this.FügeHinzu.Text = "Füge hinzu";
            this.FügeHinzu.CheckedChanged += Something_CheckedChanged;
            // 
            // ErsetzeKomplett
            // 
            this.ErsetzeKomplett.ButtonStyle = enButtonStyle.Optionbox_Text;
            this.ErsetzeKomplett.Location = new Point(8, 40);
            this.ErsetzeKomplett.Name = "ErsetzeKomplett";
            this.ErsetzeKomplett.QuickInfo = "Ersetzt den <b>kompletten Zelleninhalt</b><br>mit dem eingegebenen Text.";
            this.ErsetzeKomplett.Size = new Size(128, 32);
            this.ErsetzeKomplett.TabIndex = 10;
            this.ErsetzeKomplett.Text = "Ersetze kompletten Inhalt mit";
            this.ErsetzeKomplett.CheckedChanged += new  EventHandler(this.Something_CheckedChanged);
            // 
            // BlueFrame2
            // 
            this.BlueFrame2.CausesValidation = false;
            this.BlueFrame2.Controls.Add(this.AbgeschlosseZellen);
            this.BlueFrame2.Controls.Add(this.NurinAktuellerSpalte);
            this.BlueFrame2.Controls.Add(this.AktuelleFilterung);
            this.BlueFrame2.Location = new Point(8, 216);
            this.BlueFrame2.Name = "BlueFrame2";
            this.BlueFrame2.Size = new Size(336, 80);
            this.BlueFrame2.Text = "Optionen";
            // 
            // AbgeschlosseZellen
            // 
            this.AbgeschlosseZellen.ButtonStyle = enButtonStyle.Checkbox_Text;
            this.AbgeschlosseZellen.Checked = true;
            this.AbgeschlosseZellen.Location = new Point(16, 48);
            this.AbgeschlosseZellen.Name = "AbgeschlosseZellen";
            this.AbgeschlosseZellen.Size = new Size(312, 18);
            this.AbgeschlosseZellen.TabIndex = 11;
            this.AbgeschlosseZellen.Text = "Abgeschlosse Zeilen überspringen";
            this.AbgeschlosseZellen.CheckedChanged += new  EventHandler(this.Something_CheckedChanged);
            // 
            // BlueFrame3
            // 
            this.BlueFrame3.CausesValidation = false;
            this.BlueFrame3.Controls.Add(this.Caption3);
            this.BlueFrame3.Location = new Point(344, 216);
            this.BlueFrame3.Name = "BlueFrame3";
            this.BlueFrame3.Size = new Size(232, 80);
            this.BlueFrame3.Text = "BlueFrame3";
            // 
            // SearchAndReplace
            // 
            this.ClientSize = new Size(587, 343);
            this.Controls.Add(this.BlueFrame3);
            this.Controls.Add(this.BlueFrame2);
            this.Controls.Add(this.BlueFrame1);
            this.Controls.Add(this.Suchen);
            this.Controls.Add(this.ers);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SearchAndReplace";
            this.ShowInTaskbar = false;
            this.Text = "Suchen und Ersetzen";
            this.TopMost = true;
            this.Suchen.ResumeLayout(false);
            this.BlueFrame1.ResumeLayout(false);
            this.BlueFrame2.ResumeLayout(false);
            this.BlueFrame3.ResumeLayout(false);
            this.ResumeLayout(false);

			}
			private TextBox Alt;
			private TextBox Neu;
			private Button ers;
			private Caption Caption3;
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
			private GroupBox BlueFrame3;
			private Button FügeHinzu;



		}
	}
