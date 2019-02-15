using BlueControls.Controls;
using System.ComponentModel;


namespace BlueControls.BlueDatabaseDialogs
{
    partial class tabAdministration
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.grpAllgemein = new BlueControls.Controls.GroupBox();
            this.btnLayouts = new BlueControls.Controls.Button();
            this.btnDatenbankKopf = new BlueControls.Controls.Button();
            this.btnSpaltenUebersicht = new BlueControls.Controls.Button();
            this.btnVorherigeVersion = new BlueControls.Controls.Button();
            this.btnClipboardImport = new BlueControls.Controls.Button();
            this.btnNeueSpalteErstellen = new BlueControls.Controls.Button();
            this.grpAktuelleSpalte = new BlueControls.Controls.GroupBox();
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
            this.grpTabellenAnsicht = new BlueControls.Controls.GroupBox();
            this.btnPermanent = new BlueControls.Controls.Button();
            this.grpAllgemein.SuspendLayout();
            this.grpAktuelleSpalte.SuspendLayout();
            this.grpAktuelleAnsicht.SuspendLayout();
            this.grpAnsichtsVerwaltung.SuspendLayout();
            this.grpTabellenAnsicht.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpAllgemein
            // 
            this.grpAllgemein.CausesValidation = false;
            this.grpAllgemein.Controls.Add(this.btnLayouts);
            this.grpAllgemein.Controls.Add(this.btnDatenbankKopf);
            this.grpAllgemein.Controls.Add(this.btnSpaltenUebersicht);
            this.grpAllgemein.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAllgemein.Location = new System.Drawing.Point(0, 0);
            this.grpAllgemein.Name = "grpAllgemein";
            this.grpAllgemein.Size = new System.Drawing.Size(200, 81);
            this.grpAllgemein.Text = "Allgemein";
            // 
            // btnLayouts
            // 
            this.btnLayouts.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_RibbonBar;
            this.btnLayouts.ImageCode = "Layout";
            this.btnLayouts.Location = new System.Drawing.Point(72, 0);
            this.btnLayouts.Name = "btnLayouts";
            this.btnLayouts.Size = new System.Drawing.Size(56, 72);
            this.btnLayouts.TabIndex = 41;
            this.btnLayouts.Text = "Layouts";
            this.btnLayouts.Click += new System.EventHandler(this.btnLayouts_Click);
            // 
            // btnDatenbankKopf
            // 
            this.btnDatenbankKopf.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_RibbonBar;
            this.btnDatenbankKopf.ImageCode = "Datenbank";
            this.btnDatenbankKopf.Location = new System.Drawing.Point(8, 0);
            this.btnDatenbankKopf.Name = "btnDatenbankKopf";
            this.btnDatenbankKopf.Size = new System.Drawing.Size(64, 72);
            this.btnDatenbankKopf.TabIndex = 37;
            this.btnDatenbankKopf.Text = "Datenbank-Kopf";
            this.btnDatenbankKopf.Click += new System.EventHandler(this.btnDatenbankKopf_Click);
            // 
            // btnSpaltenUebersicht
            // 
            this.btnSpaltenUebersicht.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_RibbonBar;
            this.btnSpaltenUebersicht.ImageCode = "Spalte";
            this.btnSpaltenUebersicht.Location = new System.Drawing.Point(128, 0);
            this.btnSpaltenUebersicht.Name = "btnSpaltenUebersicht";
            this.btnSpaltenUebersicht.Size = new System.Drawing.Size(64, 72);
            this.btnSpaltenUebersicht.TabIndex = 36;
            this.btnSpaltenUebersicht.Text = "Spalten-Übersicht";
            this.btnSpaltenUebersicht.Click += new System.EventHandler(this.btnSpaltenUebersicht_Click);
            // 
            // btnVorherigeVersion
            // 
            this.btnVorherigeVersion.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_RibbonBar;
            this.btnVorherigeVersion.ImageCode = "Uhr";
            this.btnVorherigeVersion.Location = new System.Drawing.Point(112, 0);
            this.btnVorherigeVersion.Name = "btnVorherigeVersion";
            this.btnVorherigeVersion.Size = new System.Drawing.Size(56, 72);
            this.btnVorherigeVersion.TabIndex = 42;
            this.btnVorherigeVersion.Text = "Vorherige Version";
            this.btnVorherigeVersion.Click += new System.EventHandler(this.btnVorherigeVersion_Click);
            // 
            // btnClipboardImport
            // 
            this.btnClipboardImport.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_RibbonBar;
            this.btnClipboardImport.ImageCode = "Tabelle||||||||||Pfeil_Links";
            this.btnClipboardImport.Location = new System.Drawing.Point(56, 0);
            this.btnClipboardImport.Name = "btnClipboardImport";
            this.btnClipboardImport.Size = new System.Drawing.Size(56, 72);
            this.btnClipboardImport.TabIndex = 39;
            this.btnClipboardImport.Text = "Clipboard-Import";
            this.btnClipboardImport.Click += new System.EventHandler(this.btnClipboardImport_Click);
            // 
            // btnNeueSpalteErstellen
            // 
            this.btnNeueSpalteErstellen.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_RibbonBar;
            this.btnNeueSpalteErstellen.ImageCode = "PlusZeichen";
            this.btnNeueSpalteErstellen.Location = new System.Drawing.Point(8, 0);
            this.btnNeueSpalteErstellen.Name = "btnNeueSpalteErstellen";
            this.btnNeueSpalteErstellen.Size = new System.Drawing.Size(48, 72);
            this.btnNeueSpalteErstellen.TabIndex = 35;
            this.btnNeueSpalteErstellen.Text = "Neue Spalte";
            this.btnNeueSpalteErstellen.Click += new System.EventHandler(this.btnNeueSpalteErstellen_Click);
            // 
            // grpAktuelleSpalte
            // 
            this.grpAktuelleSpalte.CausesValidation = false;
            this.grpAktuelleSpalte.Controls.Add(this.btnPermanent);
            this.grpAktuelleSpalte.Controls.Add(this.btnSpalteDauerhaftloeschen);
            this.grpAktuelleSpalte.Controls.Add(this.btnSpalteAusblenden);
            this.grpAktuelleSpalte.Controls.Add(this.btnSpalteBearbeiten);
            this.grpAktuelleSpalte.Controls.Add(this.btnSpalteNachRechts);
            this.grpAktuelleSpalte.Controls.Add(this.btnSpalteNachLinks);
            this.grpAktuelleSpalte.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAktuelleSpalte.Location = new System.Drawing.Point(952, 0);
            this.grpAktuelleSpalte.Name = "grpAktuelleSpalte";
            this.grpAktuelleSpalte.Size = new System.Drawing.Size(432, 81);
            this.grpAktuelleSpalte.Text = "Gewählte Spalte";
            // 
            // btnSpalteDauerhaftloeschen
            // 
            this.btnSpalteDauerhaftloeschen.ImageCode = "Papierkorb|16|||FF0000";
            this.btnSpalteDauerhaftloeschen.Location = new System.Drawing.Point(112, 46);
            this.btnSpalteDauerhaftloeschen.Name = "btnSpalteDauerhaftloeschen";
            this.btnSpalteDauerhaftloeschen.Size = new System.Drawing.Size(136, 22);
            this.btnSpalteDauerhaftloeschen.TabIndex = 8;
            this.btnSpalteDauerhaftloeschen.Text = "dauerhaft löschen";
            this.btnSpalteDauerhaftloeschen.Click += new System.EventHandler(this.btnSpalteDauerhaftloeschen_Click);
            // 
            // btnSpalteAusblenden
            // 
            this.btnSpalteAusblenden.ImageCode = "Lupe|16||1";
            this.btnSpalteAusblenden.Location = new System.Drawing.Point(112, 24);
            this.btnSpalteAusblenden.Name = "btnSpalteAusblenden";
            this.btnSpalteAusblenden.Size = new System.Drawing.Size(112, 22);
            this.btnSpalteAusblenden.TabIndex = 7;
            this.btnSpalteAusblenden.Text = "ausblenden";
            this.btnSpalteAusblenden.Click += new System.EventHandler(this.btnSpalteAusblenden_Click);
            // 
            // btnSpalteBearbeiten
            // 
            this.btnSpalteBearbeiten.ImageCode = "Spalte|16|||||||||Stift";
            this.btnSpalteBearbeiten.Location = new System.Drawing.Point(112, 2);
            this.btnSpalteBearbeiten.Name = "btnSpalteBearbeiten";
            this.btnSpalteBearbeiten.Size = new System.Drawing.Size(176, 22);
            this.btnSpalteBearbeiten.TabIndex = 6;
            this.btnSpalteBearbeiten.Text = "Eigenschaften bearbeiten";
            this.btnSpalteBearbeiten.Click += new System.EventHandler(this.btnSpalteBearbeiten_Click);
            // 
            // btnSpalteNachRechts
            // 
            this.btnSpalteNachRechts.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_RibbonBar;
            this.btnSpalteNachRechts.ImageCode = "Pfeil_Rechts";
            this.btnSpalteNachRechts.Location = new System.Drawing.Point(56, 2);
            this.btnSpalteNachRechts.Name = "btnSpalteNachRechts";
            this.btnSpalteNachRechts.Size = new System.Drawing.Size(48, 66);
            this.btnSpalteNachRechts.TabIndex = 5;
            this.btnSpalteNachRechts.Text = "nach rechts";
            this.btnSpalteNachRechts.Click += new System.EventHandler(this.btnSpalteNachRechts_Click);
            // 
            // btnSpalteNachLinks
            // 
            this.btnSpalteNachLinks.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_RibbonBar;
            this.btnSpalteNachLinks.ImageCode = "Pfeil_Links";
            this.btnSpalteNachLinks.Location = new System.Drawing.Point(8, 2);
            this.btnSpalteNachLinks.Name = "btnSpalteNachLinks";
            this.btnSpalteNachLinks.Size = new System.Drawing.Size(48, 66);
            this.btnSpalteNachLinks.TabIndex = 4;
            this.btnSpalteNachLinks.Text = "nach links";
            this.btnSpalteNachLinks.Click += new System.EventHandler(this.btnSpalteNachLinks_Click);
            // 
            // grpAktuelleAnsicht
            // 
            this.grpAktuelleAnsicht.CausesValidation = false;
            this.grpAktuelleAnsicht.Controls.Add(this.btnSystemspaltenAusblenden);
            this.grpAktuelleAnsicht.Controls.Add(this.btnSpalteEinblenden);
            this.grpAktuelleAnsicht.Controls.Add(this.btnAlleSpaltenEinblenden);
            this.grpAktuelleAnsicht.Controls.Add(this.btnBerechtigungsgruppen);
            this.grpAktuelleAnsicht.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAktuelleAnsicht.Location = new System.Drawing.Point(680, 0);
            this.grpAktuelleAnsicht.Name = "grpAktuelleAnsicht";
            this.grpAktuelleAnsicht.Size = new System.Drawing.Size(272, 81);
            this.grpAktuelleAnsicht.Text = "Aktuelle Ansicht";
            // 
            // btnSystemspaltenAusblenden
            // 
            this.btnSystemspaltenAusblenden.ImageCode = "Lupe|16||1";
            this.btnSystemspaltenAusblenden.Location = new System.Drawing.Point(88, 24);
            this.btnSystemspaltenAusblenden.Name = "btnSystemspaltenAusblenden";
            this.btnSystemspaltenAusblenden.Size = new System.Drawing.Size(176, 22);
            this.btnSystemspaltenAusblenden.TabIndex = 33;
            this.btnSystemspaltenAusblenden.Text = "Systemspalten ausblenden";
            this.btnSystemspaltenAusblenden.Click += new System.EventHandler(this.Systemspalten_Click);
            // 
            // btnSpalteEinblenden
            // 
            this.btnSpalteEinblenden.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_RibbonBar;
            this.btnSpalteEinblenden.ImageCode = "Lupe";
            this.btnSpalteEinblenden.Location = new System.Drawing.Point(8, 2);
            this.btnSpalteEinblenden.Name = "btnSpalteEinblenden";
            this.btnSpalteEinblenden.Size = new System.Drawing.Size(72, 66);
            this.btnSpalteEinblenden.TabIndex = 7;
            this.btnSpalteEinblenden.Text = "Spalte einblenden";
            this.btnSpalteEinblenden.Click += new System.EventHandler(this.SpalteEinblenden_Click);
            // 
            // btnAlleSpaltenEinblenden
            // 
            this.btnAlleSpaltenEinblenden.ImageCode = "Lupe|16|||FF0000";
            this.btnAlleSpaltenEinblenden.Location = new System.Drawing.Point(88, 2);
            this.btnAlleSpaltenEinblenden.Name = "btnAlleSpaltenEinblenden";
            this.btnAlleSpaltenEinblenden.Size = new System.Drawing.Size(176, 22);
            this.btnAlleSpaltenEinblenden.TabIndex = 3;
            this.btnAlleSpaltenEinblenden.Text = "Alle Spalten einblenden";
            this.btnAlleSpaltenEinblenden.Click += new System.EventHandler(this.OrderReset_Click);
            // 
            // btnBerechtigungsgruppen
            // 
            this.btnBerechtigungsgruppen.ImageCode = "Schild|16";
            this.btnBerechtigungsgruppen.Location = new System.Drawing.Point(88, 46);
            this.btnBerechtigungsgruppen.Name = "btnBerechtigungsgruppen";
            this.btnBerechtigungsgruppen.Size = new System.Drawing.Size(176, 22);
            this.btnBerechtigungsgruppen.TabIndex = 32;
            this.btnBerechtigungsgruppen.Text = "Berechtigungsgruppen";
            this.btnBerechtigungsgruppen.Click += new System.EventHandler(this.Rechtex_Click);
            // 
            // grpAnsichtsVerwaltung
            // 
            this.grpAnsichtsVerwaltung.CausesValidation = false;
            this.grpAnsichtsVerwaltung.Controls.Add(this.btnAnsichtUmbenennen);
            this.grpAnsichtsVerwaltung.Controls.Add(this.capAktuellAngezeigteAnsicht);
            this.grpAnsichtsVerwaltung.Controls.Add(this.cbxInternalColumnArrangementSelector);
            this.grpAnsichtsVerwaltung.Controls.Add(this.btnNeueAnsichtErstellen);
            this.grpAnsichtsVerwaltung.Controls.Add(this.btnAktuelleAnsichtLoeschen);
            this.grpAnsichtsVerwaltung.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAnsichtsVerwaltung.Location = new System.Drawing.Point(376, 0);
            this.grpAnsichtsVerwaltung.Name = "grpAnsichtsVerwaltung";
            this.grpAnsichtsVerwaltung.Size = new System.Drawing.Size(304, 81);
            this.grpAnsichtsVerwaltung.Text = "Ansichtverwaltung";
            // 
            // btnAnsichtUmbenennen
            // 
            this.btnAnsichtUmbenennen.ImageCode = "Stift|16";
            this.btnAnsichtUmbenennen.Location = new System.Drawing.Point(136, 46);
            this.btnAnsichtUmbenennen.Name = "btnAnsichtUmbenennen";
            this.btnAnsichtUmbenennen.Size = new System.Drawing.Size(160, 22);
            this.btnAnsichtUmbenennen.TabIndex = 35;
            this.btnAnsichtUmbenennen.Text = "Ansicht umbenennen";
            this.btnAnsichtUmbenennen.Click += new System.EventHandler(this.Rename_Click);
            // 
            // capAktuellAngezeigteAnsicht
            // 
            this.capAktuellAngezeigteAnsicht.CausesValidation = false;
            this.capAktuellAngezeigteAnsicht.Location = new System.Drawing.Point(16, 2);
            this.capAktuellAngezeigteAnsicht.Name = "capAktuellAngezeigteAnsicht";
            this.capAktuellAngezeigteAnsicht.Size = new System.Drawing.Size(112, 44);
            this.capAktuellAngezeigteAnsicht.Text = "Aktuell angezeigte Ansicht:";
            this.capAktuellAngezeigteAnsicht.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // cbxInternalColumnArrangementSelector
            // 
            this.cbxInternalColumnArrangementSelector.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxInternalColumnArrangementSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxInternalColumnArrangementSelector.Format = BlueBasics.Enums.enDataFormat.Text_Ohne_Kritische_Zeichen;
            this.cbxInternalColumnArrangementSelector.Location = new System.Drawing.Point(16, 46);
            this.cbxInternalColumnArrangementSelector.Name = "cbxInternalColumnArrangementSelector";
            this.cbxInternalColumnArrangementSelector.Size = new System.Drawing.Size(112, 22);
            this.cbxInternalColumnArrangementSelector.TabIndex = 3;
            this.cbxInternalColumnArrangementSelector.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.cbxInternalColumnArrangementSelector_ItemClicked);
            // 
            // btnNeueAnsichtErstellen
            // 
            this.btnNeueAnsichtErstellen.ImageCode = "PlusZeichen|16";
            this.btnNeueAnsichtErstellen.Location = new System.Drawing.Point(136, 2);
            this.btnNeueAnsichtErstellen.Name = "btnNeueAnsichtErstellen";
            this.btnNeueAnsichtErstellen.Size = new System.Drawing.Size(160, 22);
            this.btnNeueAnsichtErstellen.TabIndex = 2;
            this.btnNeueAnsichtErstellen.Text = "neue Ansicht erstellen";
            this.btnNeueAnsichtErstellen.Click += new System.EventHandler(this.OrderAdd_Click);
            // 
            // btnAktuelleAnsichtLoeschen
            // 
            this.btnAktuelleAnsichtLoeschen.ImageCode = "MinusZeichen|16";
            this.btnAktuelleAnsichtLoeschen.Location = new System.Drawing.Point(136, 24);
            this.btnAktuelleAnsichtLoeschen.Name = "btnAktuelleAnsichtLoeschen";
            this.btnAktuelleAnsichtLoeschen.Size = new System.Drawing.Size(160, 22);
            this.btnAktuelleAnsichtLoeschen.TabIndex = 6;
            this.btnAktuelleAnsichtLoeschen.Text = "aktuelle Ansicht löschen";
            this.btnAktuelleAnsichtLoeschen.Click += new System.EventHandler(this.OrderDelete_Click);
            // 
            // grpTabellenAnsicht
            // 
            this.grpTabellenAnsicht.CausesValidation = false;
            this.grpTabellenAnsicht.Controls.Add(this.btnVorherigeVersion);
            this.grpTabellenAnsicht.Controls.Add(this.btnClipboardImport);
            this.grpTabellenAnsicht.Controls.Add(this.btnNeueSpalteErstellen);
            this.grpTabellenAnsicht.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpTabellenAnsicht.Location = new System.Drawing.Point(200, 0);
            this.grpTabellenAnsicht.Name = "grpTabellenAnsicht";
            this.grpTabellenAnsicht.Size = new System.Drawing.Size(176, 81);
            this.grpTabellenAnsicht.Text = "Tabellen-Ansicht";
            // 
            // btnPermanent
            // 
            this.btnPermanent.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnPermanent.Location = new System.Drawing.Point(296, 0);
            this.btnPermanent.Name = "btnPermanent";
            this.btnPermanent.Size = new System.Drawing.Size(96, 24);
            this.btnPermanent.TabIndex = 9;
            this.btnPermanent.Text = "permanent";
            this.btnPermanent.CheckedChanged += new System.EventHandler(this.btnPermanent_CheckedChanged);
            // 
            // tabAdministration
            // 
            this.Controls.Add(this.grpAktuelleSpalte);
            this.Controls.Add(this.grpAktuelleAnsicht);
            this.Controls.Add(this.grpAnsichtsVerwaltung);
            this.Controls.Add(this.grpTabellenAnsicht);
            this.Controls.Add(this.grpAllgemein);
            this.Enabled = false;
            this.Name = "tabAdministration";
            this.Size = new System.Drawing.Size(1400, 81);
            this.grpAllgemein.ResumeLayout(false);
            this.grpAktuelleSpalte.ResumeLayout(false);
            this.grpAktuelleAnsicht.ResumeLayout(false);
            this.grpAnsichtsVerwaltung.ResumeLayout(false);
            this.grpTabellenAnsicht.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        #endregion

        internal GroupBox grpAllgemein;
        internal Button btnSpaltenUebersicht;
        internal Button btnNeueSpalteErstellen;
        internal GroupBox grpAktuelleSpalte;
        private Button btnSpalteDauerhaftloeschen;
        private Button btnSpalteAusblenden;
        private Button btnSpalteBearbeiten;
        private Button btnSpalteNachRechts;
        private Button btnSpalteNachLinks;
        internal GroupBox grpAktuelleAnsicht;
        private Button btnSystemspaltenAusblenden;
        private Button btnSpalteEinblenden;
        private Button btnAlleSpaltenEinblenden;
        private Button btnBerechtigungsgruppen;
        internal GroupBox grpAnsichtsVerwaltung;
        private Button btnAnsichtUmbenennen;
        internal Caption capAktuellAngezeigteAnsicht;
        private ComboBox cbxInternalColumnArrangementSelector;
        private Button btnNeueAnsichtErstellen;
        private Button btnAktuelleAnsichtLoeschen;
        private Button btnDatenbankKopf;
        private Button btnLayouts;
        private Button btnClipboardImport;
        private Button btnVorherigeVersion;
        private GroupBox grpTabellenAnsicht;
        private Button btnPermanent;
    }
}
