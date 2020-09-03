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
            this.btnVorherigeVersion = new BlueControls.Controls.Button();
            this.btnDatenbankKopf = new BlueControls.Controls.Button();
            this.btnSpaltenUebersicht = new BlueControls.Controls.Button();
            this.btnClipboardImport = new BlueControls.Controls.Button();
            this.grpTabellenAnsicht = new BlueControls.Controls.GroupBox();
            this.btnScripting = new BlueControls.Controls.Button();
            this.grpBearbeitung = new BlueControls.Controls.GroupBox();
            this.btnAdminMenu = new BlueControls.Controls.Button();
            this.btnDatenüberprüfung = new BlueControls.Controls.Button();
            this.btnZeileLöschen = new BlueControls.Controls.Button();
            this.grpAllgemein.SuspendLayout();
            this.grpTabellenAnsicht.SuspendLayout();
            this.grpBearbeitung.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpAllgemein
            // 
            this.grpAllgemein.CausesValidation = false;
            this.grpAllgemein.Controls.Add(this.btnLayouts);
            this.grpAllgemein.Controls.Add(this.btnVorherigeVersion);
            this.grpAllgemein.Controls.Add(this.btnDatenbankKopf);
            this.grpAllgemein.Controls.Add(this.btnSpaltenUebersicht);
            this.grpAllgemein.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAllgemein.Location = new System.Drawing.Point(0, 0);
            this.grpAllgemein.Name = "grpAllgemein";
            this.grpAllgemein.Size = new System.Drawing.Size(272, 81);
            this.grpAllgemein.TabIndex = 4;
            this.grpAllgemein.TabStop = false;
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
            // btnVorherigeVersion
            // 
            this.btnVorherigeVersion.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_RibbonBar;
            this.btnVorherigeVersion.ImageCode = "Uhr";
            this.btnVorherigeVersion.Location = new System.Drawing.Point(200, 0);
            this.btnVorherigeVersion.Name = "btnVorherigeVersion";
            this.btnVorherigeVersion.Size = new System.Drawing.Size(64, 72);
            this.btnVorherigeVersion.TabIndex = 42;
            this.btnVorherigeVersion.Text = "Vorherige Version";
            this.btnVorherigeVersion.Click += new System.EventHandler(this.btnVorherigeVersion_Click);
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
            // btnClipboardImport
            // 
            this.btnClipboardImport.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_RibbonBar;
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
            this.grpTabellenAnsicht.Controls.Add(this.btnScripting);
            this.grpTabellenAnsicht.Controls.Add(this.btnClipboardImport);
            this.grpTabellenAnsicht.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpTabellenAnsicht.Location = new System.Drawing.Point(272, 0);
            this.grpTabellenAnsicht.Name = "grpTabellenAnsicht";
            this.grpTabellenAnsicht.Size = new System.Drawing.Size(136, 81);
            this.grpTabellenAnsicht.TabIndex = 3;
            this.grpTabellenAnsicht.TabStop = false;
            this.grpTabellenAnsicht.Text = "Import";
            // 
            // btnScripting
            // 
            this.btnScripting.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_RibbonBar;
            this.btnScripting.ImageCode = "Formel||||||||||Pfeil_Links";
            this.btnScripting.Location = new System.Drawing.Point(64, 0);
            this.btnScripting.Name = "btnScripting";
            this.btnScripting.Size = new System.Drawing.Size(56, 72);
            this.btnScripting.TabIndex = 43;
            this.btnScripting.Text = "Import-Skript";
            this.btnScripting.Click += new System.EventHandler(this.btnScripting_Click);
            // 
            // grpBearbeitung
            // 
            this.grpBearbeitung.CausesValidation = false;
            this.grpBearbeitung.Controls.Add(this.btnZeileLöschen);
            this.grpBearbeitung.Controls.Add(this.btnDatenüberprüfung);
            this.grpBearbeitung.Controls.Add(this.btnAdminMenu);
            this.grpBearbeitung.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpBearbeitung.Location = new System.Drawing.Point(408, 0);
            this.grpBearbeitung.Name = "grpBearbeitung";
            this.grpBearbeitung.Size = new System.Drawing.Size(224, 81);
            this.grpBearbeitung.TabIndex = 5;
            this.grpBearbeitung.TabStop = false;
            this.grpBearbeitung.Text = "Bearbeitung";
            // 
            // btnAdminMenu
            // 
            this.btnAdminMenu.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_RibbonBar;
            this.btnAdminMenu.ImageCode = "Zahnrad||||FF0000";
            this.btnAdminMenu.Location = new System.Drawing.Point(8, 0);
            this.btnAdminMenu.Name = "btnAdminMenu";
            this.btnAdminMenu.Size = new System.Drawing.Size(56, 72);
            this.btnAdminMenu.TabIndex = 40;
            this.btnAdminMenu.Text = "Admin-Menu";
            this.btnAdminMenu.Click += new System.EventHandler(this.btnAdminMenu_Click);
            // 
            // btnDatenüberprüfung
            // 
            this.btnDatenüberprüfung.ImageCode = "Zeile||||||||||Häkchen";
            this.btnDatenüberprüfung.Location = new System.Drawing.Point(72, 0);
            this.btnDatenüberprüfung.Name = "btnDatenüberprüfung";
            this.btnDatenüberprüfung.QuickInfo = "Aktuell angezeigte Zeilen<br>automatisch überprüfen.";
            this.btnDatenüberprüfung.Size = new System.Drawing.Size(64, 72);
            this.btnDatenüberprüfung.TabIndex = 41;
            this.btnDatenüberprüfung.Text = "Datenüber-prüfung";
            this.btnDatenüberprüfung.Click += new System.EventHandler(this.btnDatenüberprüfung_Click);
            // 
            // btnZeileLöschen
            // 
            this.btnZeileLöschen.ImageCode = "Zeile||||||||||Kreuz";
            this.btnZeileLöschen.Location = new System.Drawing.Point(144, 0);
            this.btnZeileLöschen.Name = "btnZeileLöschen";
            this.btnZeileLöschen.QuickInfo = "Angezeigte Zeilen löschen";
            this.btnZeileLöschen.Size = new System.Drawing.Size(72, 72);
            this.btnZeileLöschen.TabIndex = 42;
            this.btnZeileLöschen.Text = "Zeilen löschen";
            this.btnZeileLöschen.Click += new System.EventHandler(this.btnZeileLöschen_Click);
            // 
            // tabAdministration
            // 
            this.Controls.Add(this.grpBearbeitung);
            this.Controls.Add(this.grpTabellenAnsicht);
            this.Controls.Add(this.grpAllgemein);
            this.Enabled = false;
            this.Name = "tabAdministration";
            this.Size = new System.Drawing.Size(1400, 81);
            this.grpAllgemein.ResumeLayout(false);
            this.grpTabellenAnsicht.ResumeLayout(false);
            this.grpBearbeitung.ResumeLayout(false);
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
        private Button btnScripting;
        private GroupBox grpBearbeitung;
        private Button btnAdminMenu;
        private Button btnDatenüberprüfung;
        private Button btnZeileLöschen;
    }
}
