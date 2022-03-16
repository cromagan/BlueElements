using BlueControls.Controls;
using System.ComponentModel;

namespace BlueControls.BlueDatabaseDialogs
{
    partial class TabAdministration
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
            this.btnVorherigeVersion = new BlueControls.Controls.Button();
            this.btnDatenbankKopf = new BlueControls.Controls.Button();
            this.btnSpaltenUebersicht = new BlueControls.Controls.Button();
            this.btnClipboardImport = new BlueControls.Controls.Button();
            this.grpTabellenAnsicht = new BlueControls.Controls.GroupBox();
            this.grpZeilen = new BlueControls.Controls.GroupBox();
            this.btnPowerBearbeitung = new BlueControls.Controls.Button();
            this.btnZeileLöschen = new BlueControls.Controls.Button();
            this.btnDatenüberprüfung = new BlueControls.Controls.Button();
            this.btnSpaltenanordnung = new BlueControls.Controls.Button();
            this.grpBearbeiten = new BlueControls.Controls.GroupBox();
            this.grpAllgemein.SuspendLayout();
            this.grpTabellenAnsicht.SuspendLayout();
            this.grpZeilen.SuspendLayout();
            this.grpBearbeiten.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpAllgemein
            // 
            this.grpAllgemein.CausesValidation = false;
            this.grpAllgemein.Controls.Add(this.btnVorherigeVersion);
            this.grpAllgemein.Controls.Add(this.btnDatenbankKopf);
            this.grpAllgemein.Controls.Add(this.btnSpaltenUebersicht);
            this.grpAllgemein.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAllgemein.Location = new System.Drawing.Point(0, 0);
            this.grpAllgemein.Name = "grpAllgemein";
            this.grpAllgemein.Size = new System.Drawing.Size(208, 81);
            this.grpAllgemein.TabIndex = 4;
            this.grpAllgemein.TabStop = false;
            this.grpAllgemein.Text = "Allgemein";
            // 
            // btnLayouts
            // 
            this.btnLayouts.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Button_Big | BlueControls.Enums.enButtonStyle.Borderless)));
            this.btnLayouts.ImageCode = "Layout||||||||||Stift";
            this.btnLayouts.Location = new System.Drawing.Point(8, 0);
            this.btnLayouts.Name = "btnLayouts";
            this.btnLayouts.Size = new System.Drawing.Size(56, 72);
            this.btnLayouts.TabIndex = 41;
            this.btnLayouts.Text = "Layouts bearbeiten";
            this.btnLayouts.Click += new System.EventHandler(this.btnLayouts_Click);
            // 
            // btnVorherigeVersion
            // 
            this.btnVorherigeVersion.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Button_Big | BlueControls.Enums.enButtonStyle.Borderless)));
            this.btnVorherigeVersion.ImageCode = "Uhr";
            this.btnVorherigeVersion.Location = new System.Drawing.Point(136, 0);
            this.btnVorherigeVersion.Name = "btnVorherigeVersion";
            this.btnVorherigeVersion.Size = new System.Drawing.Size(64, 72);
            this.btnVorherigeVersion.TabIndex = 42;
            this.btnVorherigeVersion.Text = "Vorherige Version";
            this.btnVorherigeVersion.Click += new System.EventHandler(this.btnVorherigeVersion_Click);
            // 
            // btnDatenbankKopf
            // 
            this.btnDatenbankKopf.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Button_Big | BlueControls.Enums.enButtonStyle.Borderless)));
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
            this.btnSpaltenUebersicht.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Button_Big | BlueControls.Enums.enButtonStyle.Borderless)));
            this.btnSpaltenUebersicht.ImageCode = "Spalte||||||||||Information";
            this.btnSpaltenUebersicht.Location = new System.Drawing.Point(72, 0);
            this.btnSpaltenUebersicht.Name = "btnSpaltenUebersicht";
            this.btnSpaltenUebersicht.Size = new System.Drawing.Size(64, 72);
            this.btnSpaltenUebersicht.TabIndex = 36;
            this.btnSpaltenUebersicht.Text = "Spalten-Übersicht";
            this.btnSpaltenUebersicht.Click += new System.EventHandler(this.btnSpaltenUebersicht_Click);
            // 
            // btnClipboardImport
            // 
            this.btnClipboardImport.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Button_Big | BlueControls.Enums.enButtonStyle.Borderless)));
            this.btnClipboardImport.ImageCode = "Tabelle||||||||||Pfeil_Links";
            this.btnClipboardImport.Location = new System.Drawing.Point(8, 0);
            this.btnClipboardImport.Name = "btnClipboardImport";
            this.btnClipboardImport.Size = new System.Drawing.Size(56, 72);
            this.btnClipboardImport.TabIndex = 39;
            this.btnClipboardImport.Text = "Clipboard-Import";
            this.btnClipboardImport.Click += new System.EventHandler(this.btnClipboardImport_Click);
            // 
            // grpTabellenAnsicht
            // 
            this.grpTabellenAnsicht.CausesValidation = false;
            this.grpTabellenAnsicht.Controls.Add(this.btnClipboardImport);
            this.grpTabellenAnsicht.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpTabellenAnsicht.Location = new System.Drawing.Point(472, 0);
            this.grpTabellenAnsicht.Name = "grpTabellenAnsicht";
            this.grpTabellenAnsicht.Size = new System.Drawing.Size(72, 81);
            this.grpTabellenAnsicht.TabIndex = 3;
            this.grpTabellenAnsicht.TabStop = false;
            this.grpTabellenAnsicht.Text = "Import";
            // 
            // grpZeilen
            // 
            this.grpZeilen.CausesValidation = false;
            this.grpZeilen.Controls.Add(this.btnZeileLöschen);
            this.grpZeilen.Controls.Add(this.btnDatenüberprüfung);
            this.grpZeilen.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpZeilen.Location = new System.Drawing.Point(544, 0);
            this.grpZeilen.Name = "grpZeilen";
            this.grpZeilen.Size = new System.Drawing.Size(152, 81);
            this.grpZeilen.TabIndex = 5;
            this.grpZeilen.TabStop = false;
            this.grpZeilen.Text = "Zeilen";
            // 
            // btnPowerBearbeitung
            // 
            this.btnPowerBearbeitung.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Button_Big | BlueControls.Enums.enButtonStyle.Borderless)));
            this.btnPowerBearbeitung.ImageCode = "Stift||||FF0000||||||Uhr";
            this.btnPowerBearbeitung.Location = new System.Drawing.Point(128, 0);
            this.btnPowerBearbeitung.Name = "btnPowerBearbeitung";
            this.btnPowerBearbeitung.QuickInfo = "Aktuell angezeigte Zeilen<br>automatisch überprüfen.";
            this.btnPowerBearbeitung.Size = new System.Drawing.Size(72, 72);
            this.btnPowerBearbeitung.TabIndex = 43;
            this.btnPowerBearbeitung.Text = "Power-Bearbeitung";
            this.btnPowerBearbeitung.Click += new System.EventHandler(this.btnPowerBearbeitung_Click);
            // 
            // btnZeileLöschen
            // 
            this.btnZeileLöschen.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Button_Big | BlueControls.Enums.enButtonStyle.Borderless)));
            this.btnZeileLöschen.ImageCode = "Zeile||||||||||Kreuz";
            this.btnZeileLöschen.Location = new System.Drawing.Point(72, 0);
            this.btnZeileLöschen.Name = "btnZeileLöschen";
            this.btnZeileLöschen.QuickInfo = "Angezeigte Zeilen löschen";
            this.btnZeileLöschen.Size = new System.Drawing.Size(72, 72);
            this.btnZeileLöschen.TabIndex = 42;
            this.btnZeileLöschen.Text = "Zeilen löschen";
            this.btnZeileLöschen.Click += new System.EventHandler(this.btnZeileLöschen_Click);
            // 
            // btnDatenüberprüfung
            // 
            this.btnDatenüberprüfung.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Button_Big | BlueControls.Enums.enButtonStyle.Borderless)));
            this.btnDatenüberprüfung.ImageCode = "Zeile||||||||||Häkchen";
            this.btnDatenüberprüfung.Location = new System.Drawing.Point(8, 0);
            this.btnDatenüberprüfung.Name = "btnDatenüberprüfung";
            this.btnDatenüberprüfung.QuickInfo = "Aktuell angezeigte Zeilen<br>automatisch überprüfen.";
            this.btnDatenüberprüfung.Size = new System.Drawing.Size(64, 72);
            this.btnDatenüberprüfung.TabIndex = 41;
            this.btnDatenüberprüfung.Text = "Datenüber-prüfung";
            this.btnDatenüberprüfung.Click += new System.EventHandler(this.btnDatenüberprüfung_Click);
            // 
            // btnSpaltenanordnung
            // 
            this.btnSpaltenanordnung.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Button_Big | BlueControls.Enums.enButtonStyle.Borderless)));
            this.btnSpaltenanordnung.ImageCode = "Spalte||||||||||Stift";
            this.btnSpaltenanordnung.Location = new System.Drawing.Point(64, 0);
            this.btnSpaltenanordnung.Name = "btnSpaltenanordnung";
            this.btnSpaltenanordnung.Size = new System.Drawing.Size(64, 72);
            this.btnSpaltenanordnung.TabIndex = 43;
            this.btnSpaltenanordnung.Text = "Spalten-anordung";
            this.btnSpaltenanordnung.Click += new System.EventHandler(this.btnSpaltenanordnung_Click);
            // 
            // grpBearbeiten
            // 
            this.grpBearbeiten.CausesValidation = false;
            this.grpBearbeiten.Controls.Add(this.btnPowerBearbeitung);
            this.grpBearbeiten.Controls.Add(this.btnSpaltenanordnung);
            this.grpBearbeiten.Controls.Add(this.btnLayouts);
            this.grpBearbeiten.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpBearbeiten.Location = new System.Drawing.Point(208, 0);
            this.grpBearbeiten.Name = "grpBearbeiten";
            this.grpBearbeiten.Size = new System.Drawing.Size(264, 81);
            this.grpBearbeiten.TabIndex = 6;
            this.grpBearbeiten.TabStop = false;
            this.grpBearbeiten.Text = "Bearbeiten";
            // 
            // TabAdministration
            // 
            this.Controls.Add(this.grpZeilen);
            this.Controls.Add(this.grpTabellenAnsicht);
            this.Controls.Add(this.grpBearbeiten);
            this.Controls.Add(this.grpAllgemein);
            this.Enabled = false;
            this.Name = "TabAdministration";
            this.Size = new System.Drawing.Size(1400, 81);
            this.grpAllgemein.ResumeLayout(false);
            this.grpTabellenAnsicht.ResumeLayout(false);
            this.grpZeilen.ResumeLayout(false);
            this.grpBearbeiten.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        internal GroupBox grpAllgemein;
        internal Button btnSpaltenUebersicht;
        private Button btnDatenbankKopf;
        private Button btnLayouts;
        private Button btnClipboardImport;
        private Button btnVorherigeVersion;
        private GroupBox grpTabellenAnsicht;
        private GroupBox grpZeilen;
        private Button btnDatenüberprüfung;
        private Button btnZeileLöschen;
        private Button btnPowerBearbeitung;
        internal Button btnSpaltenanordnung;
        internal GroupBox grpBearbeiten;
    }
}
