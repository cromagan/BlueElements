using System;
using System.Diagnostics;
using System.Drawing;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;

namespace BlueControls.BlueDatabaseDialogs
{
    public sealed partial class AdminMenu
    {
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
            this.tbcontrol = new BlueControls.Controls.TabControl();
            this.tabAllgemein = new System.Windows.Forms.TabPage();
            this.btnDatenbankPfad = new BlueControls.Controls.Button();
            this.btnNeueSpalteErstellen = new BlueControls.Controls.Button();
            this.tabAnsichten = new System.Windows.Forms.TabPage();
            this.grpAktuelleSpalte = new BlueControls.Controls.GroupBox();
            this.btnPosEingeben = new BlueControls.Controls.Button();
            this.btnPermanent = new BlueControls.Controls.Button();
            this.btnSpalteDauerhaftloeschen = new BlueControls.Controls.Button();
            this.btnSpalteAusblenden = new BlueControls.Controls.Button();
            this.btnSpalteBearbeiten = new BlueControls.Controls.Button();
            this.btnSpalteNachRechts = new BlueControls.Controls.Button();
            this.btnSpalteNachLinks = new BlueControls.Controls.Button();
            this.grpAktuelleAnsicht = new BlueControls.Controls.GroupBox();
            this.btnSystemspaltenAusblenden = new BlueControls.Controls.Button();
            this.btnSpalteEinblenden = new BlueControls.Controls.Button();
            this.btnAlleSpaltenEinblenden = new BlueControls.Controls.Button();
            this.btnBerechtigungsgruppen = new BlueControls.Controls.Button();
            this.grpAnsichtsVerwaltung = new BlueControls.Controls.GroupBox();
            this.btnAnsichtUmbenennen = new BlueControls.Controls.Button();
            this.capAktuellAngezeigteAnsicht = new BlueControls.Controls.Caption();
            this.cbxInternalColumnArrangementSelector = new BlueControls.Controls.ComboBox();
            this.btnNeueAnsichtErstellen = new BlueControls.Controls.Button();
            this.btnAktuelleAnsichtLoeschen = new BlueControls.Controls.Button();
            this.tbcontrol.SuspendLayout();
            this.tabAllgemein.SuspendLayout();
            this.tabAnsichten.SuspendLayout();
            this.grpAktuelleSpalte.SuspendLayout();
            this.grpAktuelleAnsicht.SuspendLayout();
            this.grpAnsichtsVerwaltung.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbcontrol
            // 
            this.tbcontrol.Controls.Add(this.tabAllgemein);
            this.tbcontrol.Controls.Add(this.tabAnsichten);
            this.tbcontrol.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbcontrol.HotTrack = true;
            this.tbcontrol.Location = new System.Drawing.Point(0, 0);
            this.tbcontrol.Name = "tbcontrol";
            this.tbcontrol.SelectedIndex = 0;
            this.tbcontrol.Size = new System.Drawing.Size(386, 420);
            this.tbcontrol.TabIndex = 0;
            // 
            // tabAllgemein
            // 
            this.tabAllgemein.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabAllgemein.Controls.Add(this.btnDatenbankPfad);
            this.tabAllgemein.Controls.Add(this.btnNeueSpalteErstellen);
            this.tabAllgemein.Location = new System.Drawing.Point(4, 25);
            this.tabAllgemein.Name = "tabAllgemein";
            this.tabAllgemein.Size = new System.Drawing.Size(378, 391);
            this.tabAllgemein.TabIndex = 0;
            this.tabAllgemein.Text = "Allgemein";
            // 
            // btnDatenbankPfad
            // 
            this.btnDatenbankPfad.ImageCode = "Ordner|34";
            this.btnDatenbankPfad.Location = new System.Drawing.Point(16, 16);
            this.btnDatenbankPfad.Name = "btnDatenbankPfad";
            this.btnDatenbankPfad.QuickInfo = "Speicherort der Datenbanken öffnen";
            this.btnDatenbankPfad.Size = new System.Drawing.Size(240, 40);
            this.btnDatenbankPfad.TabIndex = 37;
            this.btnDatenbankPfad.Text = "Datenbanken-Pfad";
            this.btnDatenbankPfad.Click += new System.EventHandler(this.btnDatenbankPfad_Click);
            // 
            // btnNeueSpalteErstellen
            // 
            this.btnNeueSpalteErstellen.ImageCode = "PlusZeichen|24";
            this.btnNeueSpalteErstellen.Location = new System.Drawing.Point(16, 64);
            this.btnNeueSpalteErstellen.Name = "btnNeueSpalteErstellen";
            this.btnNeueSpalteErstellen.Size = new System.Drawing.Size(240, 40);
            this.btnNeueSpalteErstellen.TabIndex = 36;
            this.btnNeueSpalteErstellen.Text = "Neue Spalte";
            this.btnNeueSpalteErstellen.Click += new System.EventHandler(this.btnNeueSpalteErstellen_Click);
            // 
            // tabAnsichten
            // 
            this.tabAnsichten.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabAnsichten.Controls.Add(this.grpAktuelleSpalte);
            this.tabAnsichten.Controls.Add(this.grpAktuelleAnsicht);
            this.tabAnsichten.Controls.Add(this.grpAnsichtsVerwaltung);
            this.tabAnsichten.Location = new System.Drawing.Point(4, 25);
            this.tabAnsichten.Name = "tabAnsichten";
            this.tabAnsichten.Size = new System.Drawing.Size(378, 391);
            this.tabAnsichten.TabIndex = 1;
            this.tabAnsichten.Text = "Ansichten";
            // 
            // grpAktuelleSpalte
            // 
            this.grpAktuelleSpalte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpAktuelleSpalte.CausesValidation = false;
            this.grpAktuelleSpalte.Controls.Add(this.btnPosEingeben);
            this.grpAktuelleSpalte.Controls.Add(this.btnPermanent);
            this.grpAktuelleSpalte.Controls.Add(this.btnSpalteDauerhaftloeschen);
            this.grpAktuelleSpalte.Controls.Add(this.btnSpalteAusblenden);
            this.grpAktuelleSpalte.Controls.Add(this.btnSpalteBearbeiten);
            this.grpAktuelleSpalte.Controls.Add(this.btnSpalteNachRechts);
            this.grpAktuelleSpalte.Controls.Add(this.btnSpalteNachLinks);
            this.grpAktuelleSpalte.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpAktuelleSpalte.Location = new System.Drawing.Point(0, 264);
            this.grpAktuelleSpalte.Name = "grpAktuelleSpalte";
            this.grpAktuelleSpalte.Size = new System.Drawing.Size(378, 120);
            this.grpAktuelleSpalte.TabIndex = 5;
            this.grpAktuelleSpalte.TabStop = false;
            this.grpAktuelleSpalte.Text = "Gewählte Spalte";
            // 
            // btnPosEingeben
            // 
            this.btnPosEingeben.ImageCode = "Summe|16";
            this.btnPosEingeben.Location = new System.Drawing.Point(112, 56);
            this.btnPosEingeben.Name = "btnPosEingeben";
            this.btnPosEingeben.QuickInfo = "Verschiebt die aktuelle Spalte an eine Position.<br>Die Spaltennummerierung wird " +
    "berücksichtigt.";
            this.btnPosEingeben.Size = new System.Drawing.Size(112, 24);
            this.btnPosEingeben.TabIndex = 10;
            this.btnPosEingeben.Text = "Pos. eingeben";
            this.btnPosEingeben.Click += new System.EventHandler(this.btnPosEingeben_Click);
            // 
            // btnPermanent
            // 
            this.btnPermanent.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.btnPermanent.Location = new System.Drawing.Point(112, 32);
            this.btnPermanent.Name = "btnPermanent";
            this.btnPermanent.QuickInfo = "Die aktuelle Spalte wird immer<br>angezeigt und nicht verschoben.";
            this.btnPermanent.Size = new System.Drawing.Size(112, 24);
            this.btnPermanent.TabIndex = 9;
            this.btnPermanent.Text = "permanent";
            this.btnPermanent.CheckedChanged += new System.EventHandler(this.btnPermanent_CheckedChanged);
            // 
            // btnSpalteDauerhaftloeschen
            // 
            this.btnSpalteDauerhaftloeschen.ImageCode = "Papierkorb|16|||FF0000";
            this.btnSpalteDauerhaftloeschen.Location = new System.Drawing.Point(224, 56);
            this.btnSpalteDauerhaftloeschen.Name = "btnSpalteDauerhaftloeschen";
            this.btnSpalteDauerhaftloeschen.Size = new System.Drawing.Size(136, 22);
            this.btnSpalteDauerhaftloeschen.TabIndex = 8;
            this.btnSpalteDauerhaftloeschen.Text = "dauerhaft löschen";
            this.btnSpalteDauerhaftloeschen.Click += new System.EventHandler(this.btnSpalteDauerhaftloeschen_Click);
            // 
            // btnSpalteAusblenden
            // 
            this.btnSpalteAusblenden.ImageCode = "Lupe|16||1";
            this.btnSpalteAusblenden.Location = new System.Drawing.Point(224, 32);
            this.btnSpalteAusblenden.Name = "btnSpalteAusblenden";
            this.btnSpalteAusblenden.Size = new System.Drawing.Size(112, 22);
            this.btnSpalteAusblenden.TabIndex = 7;
            this.btnSpalteAusblenden.Text = "ausblenden";
            this.btnSpalteAusblenden.Click += new System.EventHandler(this.btnSpalteAusblenden_Click);
            // 
            // btnSpalteBearbeiten
            // 
            this.btnSpalteBearbeiten.ImageCode = "Stift|16|||||||||Spalte";
            this.btnSpalteBearbeiten.Location = new System.Drawing.Point(224, 80);
            this.btnSpalteBearbeiten.Name = "btnSpalteBearbeiten";
            this.btnSpalteBearbeiten.QuickInfo = "Eigenschaften der Spalte bearbeiten";
            this.btnSpalteBearbeiten.Size = new System.Drawing.Size(144, 22);
            this.btnSpalteBearbeiten.TabIndex = 6;
            this.btnSpalteBearbeiten.Text = "Eigenschaften bearb.";
            this.btnSpalteBearbeiten.Click += new System.EventHandler(this.btnSpalteBearbeiten_Click);
            // 
            // btnSpalteNachRechts
            // 
            this.btnSpalteNachRechts.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_Big;
            this.btnSpalteNachRechts.ImageCode = "Pfeil_Rechts";
            this.btnSpalteNachRechts.Location = new System.Drawing.Point(56, 32);
            this.btnSpalteNachRechts.Name = "btnSpalteNachRechts";
            this.btnSpalteNachRechts.Size = new System.Drawing.Size(48, 66);
            this.btnSpalteNachRechts.TabIndex = 5;
            this.btnSpalteNachRechts.Text = "nach rechts";
            this.btnSpalteNachRechts.Click += new System.EventHandler(this.btnSpalteNachRechts_Click);
            // 
            // btnSpalteNachLinks
            // 
            this.btnSpalteNachLinks.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_Big;
            this.btnSpalteNachLinks.ImageCode = "Pfeil_Links";
            this.btnSpalteNachLinks.Location = new System.Drawing.Point(8, 32);
            this.btnSpalteNachLinks.Name = "btnSpalteNachLinks";
            this.btnSpalteNachLinks.Size = new System.Drawing.Size(48, 66);
            this.btnSpalteNachLinks.TabIndex = 4;
            this.btnSpalteNachLinks.Text = "nach links";
            this.btnSpalteNachLinks.Click += new System.EventHandler(this.btnSpalteNachLinks_Click);
            // 
            // grpAktuelleAnsicht
            // 
            this.grpAktuelleAnsicht.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpAktuelleAnsicht.CausesValidation = false;
            this.grpAktuelleAnsicht.Controls.Add(this.btnSystemspaltenAusblenden);
            this.grpAktuelleAnsicht.Controls.Add(this.btnSpalteEinblenden);
            this.grpAktuelleAnsicht.Controls.Add(this.btnAlleSpaltenEinblenden);
            this.grpAktuelleAnsicht.Controls.Add(this.btnBerechtigungsgruppen);
            this.grpAktuelleAnsicht.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpAktuelleAnsicht.Location = new System.Drawing.Point(0, 152);
            this.grpAktuelleAnsicht.Name = "grpAktuelleAnsicht";
            this.grpAktuelleAnsicht.Size = new System.Drawing.Size(378, 112);
            this.grpAktuelleAnsicht.TabIndex = 4;
            this.grpAktuelleAnsicht.TabStop = false;
            this.grpAktuelleAnsicht.Text = "Aktuelle Ansicht";
            // 
            // btnSystemspaltenAusblenden
            // 
            this.btnSystemspaltenAusblenden.ImageCode = "Lupe|16||1";
            this.btnSystemspaltenAusblenden.Location = new System.Drawing.Point(80, 46);
            this.btnSystemspaltenAusblenden.Name = "btnSystemspaltenAusblenden";
            this.btnSystemspaltenAusblenden.Size = new System.Drawing.Size(176, 22);
            this.btnSystemspaltenAusblenden.TabIndex = 33;
            this.btnSystemspaltenAusblenden.Text = "Systemspalten ausblenden";
            this.btnSystemspaltenAusblenden.Click += new System.EventHandler(this.btnSystemspaltenAusblenden_Click);
            // 
            // btnSpalteEinblenden
            // 
            this.btnSpalteEinblenden.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_Big;
            this.btnSpalteEinblenden.ImageCode = "Lupe";
            this.btnSpalteEinblenden.Location = new System.Drawing.Point(8, 24);
            this.btnSpalteEinblenden.Name = "btnSpalteEinblenden";
            this.btnSpalteEinblenden.Size = new System.Drawing.Size(64, 66);
            this.btnSpalteEinblenden.TabIndex = 7;
            this.btnSpalteEinblenden.Text = "Spalte einblenden";
            this.btnSpalteEinblenden.Click += new System.EventHandler(this.btnSpalteEinblenden_Click);
            // 
            // btnAlleSpaltenEinblenden
            // 
            this.btnAlleSpaltenEinblenden.ImageCode = "Lupe|16|||FF0000";
            this.btnAlleSpaltenEinblenden.Location = new System.Drawing.Point(80, 24);
            this.btnAlleSpaltenEinblenden.Name = "btnAlleSpaltenEinblenden";
            this.btnAlleSpaltenEinblenden.Size = new System.Drawing.Size(176, 22);
            this.btnAlleSpaltenEinblenden.TabIndex = 3;
            this.btnAlleSpaltenEinblenden.Text = "Alle Spalten einblenden";
            this.btnAlleSpaltenEinblenden.Click += new System.EventHandler(this.btnAlleSpaltenEinblenden_Click);
            // 
            // btnBerechtigungsgruppen
            // 
            this.btnBerechtigungsgruppen.ImageCode = "Schild|16";
            this.btnBerechtigungsgruppen.Location = new System.Drawing.Point(80, 68);
            this.btnBerechtigungsgruppen.Name = "btnBerechtigungsgruppen";
            this.btnBerechtigungsgruppen.Size = new System.Drawing.Size(176, 22);
            this.btnBerechtigungsgruppen.TabIndex = 32;
            this.btnBerechtigungsgruppen.Text = "Berechtigungsgruppen";
            this.btnBerechtigungsgruppen.Click += new System.EventHandler(this.btnBerechtigungsgruppen_Click);
            // 
            // grpAnsichtsVerwaltung
            // 
            this.grpAnsichtsVerwaltung.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpAnsichtsVerwaltung.CausesValidation = false;
            this.grpAnsichtsVerwaltung.Controls.Add(this.btnAnsichtUmbenennen);
            this.grpAnsichtsVerwaltung.Controls.Add(this.capAktuellAngezeigteAnsicht);
            this.grpAnsichtsVerwaltung.Controls.Add(this.cbxInternalColumnArrangementSelector);
            this.grpAnsichtsVerwaltung.Controls.Add(this.btnNeueAnsichtErstellen);
            this.grpAnsichtsVerwaltung.Controls.Add(this.btnAktuelleAnsichtLoeschen);
            this.grpAnsichtsVerwaltung.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpAnsichtsVerwaltung.Location = new System.Drawing.Point(0, 0);
            this.grpAnsichtsVerwaltung.Name = "grpAnsichtsVerwaltung";
            this.grpAnsichtsVerwaltung.Size = new System.Drawing.Size(378, 152);
            this.grpAnsichtsVerwaltung.TabIndex = 3;
            this.grpAnsichtsVerwaltung.TabStop = false;
            this.grpAnsichtsVerwaltung.Text = "Ansichtverwaltung";
            // 
            // btnAnsichtUmbenennen
            // 
            this.btnAnsichtUmbenennen.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_Big;
            this.btnAnsichtUmbenennen.ImageCode = "Stift";
            this.btnAnsichtUmbenennen.Location = new System.Drawing.Point(144, 72);
            this.btnAnsichtUmbenennen.Name = "btnAnsichtUmbenennen";
            this.btnAnsichtUmbenennen.Size = new System.Drawing.Size(64, 72);
            this.btnAnsichtUmbenennen.TabIndex = 35;
            this.btnAnsichtUmbenennen.Text = "Ansicht umbenennen";
            this.btnAnsichtUmbenennen.Click += new System.EventHandler(this.btnAnsichtUmbenennen_Click);
            // 
            // capAktuellAngezeigteAnsicht
            // 
            this.capAktuellAngezeigteAnsicht.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.capAktuellAngezeigteAnsicht.CausesValidation = false;
            this.capAktuellAngezeigteAnsicht.Location = new System.Drawing.Point(16, 24);
            this.capAktuellAngezeigteAnsicht.Name = "capAktuellAngezeigteAnsicht";
            this.capAktuellAngezeigteAnsicht.Size = new System.Drawing.Size(352, 16);
            this.capAktuellAngezeigteAnsicht.Text = "Aktuell angezeigte Ansicht:";
            this.capAktuellAngezeigteAnsicht.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // cbxInternalColumnArrangementSelector
            // 
            this.cbxInternalColumnArrangementSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxInternalColumnArrangementSelector.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxInternalColumnArrangementSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxInternalColumnArrangementSelector.Location = new System.Drawing.Point(16, 46);
            this.cbxInternalColumnArrangementSelector.Name = "cbxInternalColumnArrangementSelector";
            this.cbxInternalColumnArrangementSelector.Size = new System.Drawing.Size(352, 22);
            this.cbxInternalColumnArrangementSelector.TabIndex = 3;
            this.cbxInternalColumnArrangementSelector.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.cbxInternalColumnArrangementSelector_ItemClicked);
            // 
            // btnNeueAnsichtErstellen
            // 
            this.btnNeueAnsichtErstellen.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_Big;
            this.btnNeueAnsichtErstellen.ImageCode = "PlusZeichen";
            this.btnNeueAnsichtErstellen.Location = new System.Drawing.Point(16, 72);
            this.btnNeueAnsichtErstellen.Name = "btnNeueAnsichtErstellen";
            this.btnNeueAnsichtErstellen.Size = new System.Drawing.Size(64, 72);
            this.btnNeueAnsichtErstellen.TabIndex = 2;
            this.btnNeueAnsichtErstellen.Text = "neue Ansicht erstellen";
            this.btnNeueAnsichtErstellen.Click += new System.EventHandler(this.OrderAdd_Click);
            // 
            // btnAktuelleAnsichtLoeschen
            // 
            this.btnAktuelleAnsichtLoeschen.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_Big;
            this.btnAktuelleAnsichtLoeschen.ImageCode = "MinusZeichen";
            this.btnAktuelleAnsichtLoeschen.Location = new System.Drawing.Point(80, 72);
            this.btnAktuelleAnsichtLoeschen.Name = "btnAktuelleAnsichtLoeschen";
            this.btnAktuelleAnsichtLoeschen.Size = new System.Drawing.Size(64, 72);
            this.btnAktuelleAnsichtLoeschen.TabIndex = 6;
            this.btnAktuelleAnsichtLoeschen.Text = "aktuelle Ansicht löschen";
            this.btnAktuelleAnsichtLoeschen.Click += new System.EventHandler(this.btnAktuelleAnsichtLoeschen_Click);
            // 
            // AdminMenu
            // 
            this.ClientSize = new System.Drawing.Size(386, 420);
            this.Controls.Add(this.tbcontrol);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "AdminMenu";
            this.ShowInTaskbar = false;
            this.Text = "Administrator-Menu";
            this.TopMost = true;
            this.tbcontrol.ResumeLayout(false);
            this.tabAllgemein.ResumeLayout(false);
            this.tabAnsichten.ResumeLayout(false);
            this.grpAktuelleSpalte.ResumeLayout(false);
            this.grpAktuelleAnsicht.ResumeLayout(false);
            this.grpAnsichtsVerwaltung.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private TabControl tbcontrol;
        private  System.Windows.Forms.TabPage tabAllgemein;
        private  System.Windows.Forms.TabPage tabAnsichten;
        internal Button btnNeueSpalteErstellen;
        internal GroupBox grpAnsichtsVerwaltung;
        private Button btnAnsichtUmbenennen;
        internal Caption capAktuellAngezeigteAnsicht;
        private ComboBox cbxInternalColumnArrangementSelector;
        private Button btnNeueAnsichtErstellen;
        private Button btnAktuelleAnsichtLoeschen;
        internal GroupBox grpAktuelleAnsicht;
        private Button btnSystemspaltenAusblenden;
        private Button btnSpalteEinblenden;
        private Button btnAlleSpaltenEinblenden;
        private Button btnBerechtigungsgruppen;
        internal GroupBox grpAktuelleSpalte;
        private Button btnPosEingeben;
        private Button btnPermanent;
        private Button btnSpalteDauerhaftloeschen;
        private Button btnSpalteAusblenden;
        private Button btnSpalteBearbeiten;
        private Button btnSpalteNachRechts;
        private Button btnSpalteNachLinks;
        private Button btnDatenbankPfad;
    }
}
