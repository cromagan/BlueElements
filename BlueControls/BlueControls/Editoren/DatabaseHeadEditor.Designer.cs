using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;
using TabControl = BlueControls.Controls.TabControl;
using TabPage = System.Windows.Forms.TabPage;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueDatabaseDialogs {
    public sealed partial class DatabaseHeadEditor {
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
            this.grpBenutzergruppen = new BlueControls.Controls.GroupBox();
            this.btnDummyAdmin = new BlueControls.Controls.Button();
            this.PermissionGroups_NewRow = new BlueControls.Controls.ListBox();
            this.capNeueZeilenInfo = new BlueControls.Controls.Caption();
            this.Caption22 = new BlueControls.Controls.Caption();
            this.DatenbankAdmin = new BlueControls.Controls.ListBox();
            this.capNeueZeilen = new BlueControls.Controls.Caption();
            this.grpKennwort = new BlueControls.Controls.GroupBox();
            this.capKennwort = new BlueControls.Controls.Caption();
            this.txbKennwort = new BlueControls.Controls.TextBox();
            this.lbxSortierSpalten = new BlueControls.Controls.ListBox();
            this.capSortierspalten = new BlueControls.Controls.Caption();
            this.btnSortRichtung = new BlueControls.Controls.Button();
            this.btnOk = new BlueControls.Controls.Button();
            this.txbTags = new BlueControls.Controls.TextBox();
            this.txbCaption = new BlueControls.Controls.TextBox();
            this.capCaption = new BlueControls.Controls.Caption();
            this.capTags = new BlueControls.Controls.Caption();
            this.capInfo = new BlueControls.Controls.Caption();
            this.GlobalTab = new BlueControls.Controls.TabControl();
            this.tabAllgemein = new System.Windows.Forms.TabPage();
            this.btnTabellenAnsicht = new BlueControls.Controls.Button();
            this.btnSkripte = new BlueControls.Controls.Button();
            this.btnSpaltenAnordnungen = new BlueControls.Controls.Button();
            this.txbZeilenQuickInfo = new BlueControls.Controls.TextBox();
            this.butSystemspaltenErstellen = new BlueControls.Controls.Button();
            this.btnOptimize = new BlueControls.Controls.Button();
            this.txbStandardFormulaFile = new BlueControls.Controls.TextBox();
            this.capStandardFormulaFile = new BlueControls.Controls.Caption();
            this.txbAdditionalFiles = new BlueControls.Controls.TextBox();
            this.capAdditional = new BlueControls.Controls.Caption();
            this.capZeilenQuickInfo = new BlueControls.Controls.Caption();
            this.txbGlobalScale = new BlueControls.Controls.TextBox();
            this.capGlobalScale = new BlueControls.Controls.Caption();
            this.btnSpaltenuebersicht = new BlueControls.Controls.Button();
            this.tabRechte = new System.Windows.Forms.TabPage();
            this.tabSortierung = new System.Windows.Forms.TabPage();
            this.tabVariablen = new System.Windows.Forms.TabPage();
            this.variableEditor = new BlueControls.VariableEditor();
            this.tabUndo = new System.Windows.Forms.TabPage();
            this.tblUndo = new BlueControls.Controls.Table();
            this.grpUndoActions = new BlueControls.Controls.GroupBox();
            this.btnClipboard = new BlueControls.Controls.Button();
            this.pnlStatusBar.SuspendLayout();
            this.grpBenutzergruppen.SuspendLayout();
            this.grpKennwort.SuspendLayout();
            this.GlobalTab.SuspendLayout();
            this.tabAllgemein.SuspendLayout();
            this.tabRechte.SuspendLayout();
            this.tabSortierung.SuspendLayout();
            this.tabVariablen.SuspendLayout();
            this.tabUndo.SuspendLayout();
            this.grpUndoActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(1244, 24);
            this.capStatusBar.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Text_Abschneiden;
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 732);
            this.pnlStatusBar.Size = new System.Drawing.Size(1244, 24);
            // 
            // grpBenutzergruppen
            // 
            this.grpBenutzergruppen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpBenutzergruppen.CausesValidation = false;
            this.grpBenutzergruppen.Controls.Add(this.btnDummyAdmin);
            this.grpBenutzergruppen.Controls.Add(this.PermissionGroups_NewRow);
            this.grpBenutzergruppen.Controls.Add(this.capNeueZeilenInfo);
            this.grpBenutzergruppen.Controls.Add(this.Caption22);
            this.grpBenutzergruppen.Controls.Add(this.DatenbankAdmin);
            this.grpBenutzergruppen.Controls.Add(this.capNeueZeilen);
            this.grpBenutzergruppen.Location = new System.Drawing.Point(16, 16);
            this.grpBenutzergruppen.Name = "grpBenutzergruppen";
            this.grpBenutzergruppen.Size = new System.Drawing.Size(376, 488);
            this.grpBenutzergruppen.TabIndex = 2;
            this.grpBenutzergruppen.TabStop = false;
            this.grpBenutzergruppen.Text = "Benutzergruppen:";
            // 
            // btnDummyAdmin
            // 
            this.btnDummyAdmin.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnDummyAdmin.Checked = true;
            this.btnDummyAdmin.Enabled = false;
            this.btnDummyAdmin.Location = new System.Drawing.Point(192, 48);
            this.btnDummyAdmin.Name = "btnDummyAdmin";
            this.btnDummyAdmin.Size = new System.Drawing.Size(176, 16);
            this.btnDummyAdmin.TabIndex = 17;
            this.btnDummyAdmin.Text = "#Administrator";
            // 
            // PermissionGroups_NewRow
            // 
            this.PermissionGroups_NewRow.AddAllowed = BlueControls.Enums.AddType.Text;
            this.PermissionGroups_NewRow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.PermissionGroups_NewRow.Appearance = BlueControls.Enums.ListBoxAppearance.Listbox_Boxes;
            this.PermissionGroups_NewRow.CheckBehavior = BlueControls.Enums.CheckBehavior.MultiSelection;
            this.PermissionGroups_NewRow.FilterText = null;
            this.PermissionGroups_NewRow.Location = new System.Drawing.Point(192, 64);
            this.PermissionGroups_NewRow.Name = "PermissionGroups_NewRow";
            this.PermissionGroups_NewRow.RemoveAllowed = true;
            this.PermissionGroups_NewRow.Size = new System.Drawing.Size(176, 376);
            this.PermissionGroups_NewRow.TabIndex = 4;
            this.PermissionGroups_NewRow.Translate = false;
            // 
            // capNeueZeilenInfo
            // 
            this.capNeueZeilenInfo.CausesValidation = false;
            this.capNeueZeilenInfo.Location = new System.Drawing.Point(192, 440);
            this.capNeueZeilenInfo.Name = "capNeueZeilenInfo";
            this.capNeueZeilenInfo.Size = new System.Drawing.Size(176, 40);
            this.capNeueZeilenInfo.Text = "<i>Die erste Spalte muss eine Bearbeitung zulassen";
            this.capNeueZeilenInfo.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption22
            // 
            this.Caption22.CausesValidation = false;
            this.Caption22.Location = new System.Drawing.Point(8, 24);
            this.Caption22.Name = "Caption22";
            this.Caption22.Size = new System.Drawing.Size(176, 22);
            this.Caption22.Text = "Datenbank-Administratoren:";
            this.Caption22.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // DatenbankAdmin
            // 
            this.DatenbankAdmin.AddAllowed = BlueControls.Enums.AddType.Text;
            this.DatenbankAdmin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.DatenbankAdmin.Appearance = BlueControls.Enums.ListBoxAppearance.Listbox_Boxes;
            this.DatenbankAdmin.CheckBehavior = BlueControls.Enums.CheckBehavior.MultiSelection;
            this.DatenbankAdmin.FilterText = null;
            this.DatenbankAdmin.Location = new System.Drawing.Point(8, 46);
            this.DatenbankAdmin.Name = "DatenbankAdmin";
            this.DatenbankAdmin.Size = new System.Drawing.Size(176, 434);
            this.DatenbankAdmin.TabIndex = 4;
            this.DatenbankAdmin.Translate = false;
            // 
            // capNeueZeilen
            // 
            this.capNeueZeilen.CausesValidation = false;
            this.capNeueZeilen.Location = new System.Drawing.Point(192, 24);
            this.capNeueZeilen.Name = "capNeueZeilen";
            this.capNeueZeilen.Size = new System.Drawing.Size(176, 24);
            this.capNeueZeilen.Text = "Neue Zeilen anlegen:";
            this.capNeueZeilen.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // grpKennwort
            // 
            this.grpKennwort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpKennwort.CausesValidation = false;
            this.grpKennwort.Controls.Add(this.capKennwort);
            this.grpKennwort.Controls.Add(this.txbKennwort);
            this.grpKennwort.Location = new System.Drawing.Point(400, 16);
            this.grpKennwort.Name = "grpKennwort";
            this.grpKennwort.Size = new System.Drawing.Size(232, 96);
            this.grpKennwort.TabIndex = 1;
            this.grpKennwort.TabStop = false;
            this.grpKennwort.Text = "Kennwort:";
            // 
            // capKennwort
            // 
            this.capKennwort.CausesValidation = false;
            this.capKennwort.Location = new System.Drawing.Point(8, 24);
            this.capKennwort.Name = "capKennwort";
            this.capKennwort.Size = new System.Drawing.Size(216, 22);
            this.capKennwort.Text = "Zum Öffnen der Datenbank:";
            this.capKennwort.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbKennwort
            // 
            this.txbKennwort.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbKennwort.Location = new System.Drawing.Point(8, 56);
            this.txbKennwort.Name = "txbKennwort";
            this.txbKennwort.Size = new System.Drawing.Size(216, 22);
            this.txbKennwort.TabIndex = 4;
            // 
            // lbxSortierSpalten
            // 
            this.lbxSortierSpalten.AddAllowed = BlueControls.Enums.AddType.OnlySuggests;
            this.lbxSortierSpalten.AutoSort = false;
            this.lbxSortierSpalten.CheckBehavior = BlueControls.Enums.CheckBehavior.AllSelected;
            this.lbxSortierSpalten.FilterText = null;
            this.lbxSortierSpalten.Location = new System.Drawing.Point(16, 32);
            this.lbxSortierSpalten.MoveAllowed = true;
            this.lbxSortierSpalten.Name = "lbxSortierSpalten";
            this.lbxSortierSpalten.RemoveAllowed = true;
            this.lbxSortierSpalten.Size = new System.Drawing.Size(256, 440);
            this.lbxSortierSpalten.TabIndex = 5;
            // 
            // capSortierspalten
            // 
            this.capSortierspalten.CausesValidation = false;
            this.capSortierspalten.Location = new System.Drawing.Point(16, 8);
            this.capSortierspalten.Name = "capSortierspalten";
            this.capSortierspalten.Size = new System.Drawing.Size(160, 24);
            this.capSortierspalten.Text = "Sortier-Spalten:";
            this.capSortierspalten.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnSortRichtung
            // 
            this.btnSortRichtung.ButtonStyle = BlueControls.Enums.ButtonStyle.Yes_or_No;
            this.btnSortRichtung.Location = new System.Drawing.Point(288, 32);
            this.btnSortRichtung.Name = "btnSortRichtung";
            this.btnSortRichtung.Size = new System.Drawing.Size(184, 40);
            this.btnSortRichtung.TabIndex = 0;
            this.btnSortRichtung.Text = "Umgekehrte Sortierung";
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.ImageCode = "Häkchen|24";
            this.btnOk.Location = new System.Drawing.Point(1144, 715);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(88, 32);
            this.btnOk.TabIndex = 11;
            this.btnOk.Text = "OK";
            this.btnOk.Click += new System.EventHandler(this.OkBut_Click);
            // 
            // txbTags
            // 
            this.txbTags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbTags.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbTags.Location = new System.Drawing.Point(624, 24);
            this.txbTags.MultiLine = true;
            this.txbTags.Name = "txbTags";
            this.txbTags.Size = new System.Drawing.Size(601, 639);
            this.txbTags.TabIndex = 26;
            this.txbTags.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbCaption
            // 
            this.txbCaption.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbCaption.Location = new System.Drawing.Point(8, 24);
            this.txbCaption.Name = "txbCaption";
            this.txbCaption.Size = new System.Drawing.Size(608, 24);
            this.txbCaption.TabIndex = 24;
            // 
            // capCaption
            // 
            this.capCaption.CausesValidation = false;
            this.capCaption.Location = new System.Drawing.Point(8, 8);
            this.capCaption.Name = "capCaption";
            this.capCaption.Size = new System.Drawing.Size(137, 16);
            this.capCaption.Text = "Überschrift bzw. Titel:";
            this.capCaption.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capTags
            // 
            this.capTags.CausesValidation = false;
            this.capTags.Location = new System.Drawing.Point(624, 8);
            this.capTags.Name = "capTags";
            this.capTags.QuickInfo = "Tags / Eigenschaften, die von einem ";
            this.capTags.Size = new System.Drawing.Size(152, 16);
            this.capTags.Text = "Tags:";
            this.capTags.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capInfo
            // 
            this.capInfo.CausesValidation = false;
            this.capInfo.Location = new System.Drawing.Point(8, 56);
            this.capInfo.Name = "capInfo";
            this.capInfo.Size = new System.Drawing.Size(608, 144);
            this.capInfo.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // GlobalTab
            // 
            this.GlobalTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GlobalTab.Controls.Add(this.tabAllgemein);
            this.GlobalTab.Controls.Add(this.tabRechte);
            this.GlobalTab.Controls.Add(this.tabSortierung);
            this.GlobalTab.Controls.Add(this.tabVariablen);
            this.GlobalTab.Controls.Add(this.tabUndo);
            this.GlobalTab.HotTrack = true;
            this.GlobalTab.Location = new System.Drawing.Point(0, 0);
            this.GlobalTab.Name = "GlobalTab";
            this.GlobalTab.SelectedIndex = 0;
            this.GlobalTab.Size = new System.Drawing.Size(1241, 707);
            this.GlobalTab.TabDefault = null;
            this.GlobalTab.TabDefaultOrder = null;
            this.GlobalTab.TabIndex = 21;
            this.GlobalTab.SelectedIndexChanged += new System.EventHandler(this.GlobalTab_SelectedIndexChanged);
            // 
            // tabAllgemein
            // 
            this.tabAllgemein.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabAllgemein.Controls.Add(this.btnTabellenAnsicht);
            this.tabAllgemein.Controls.Add(this.btnSkripte);
            this.tabAllgemein.Controls.Add(this.btnSpaltenAnordnungen);
            this.tabAllgemein.Controls.Add(this.txbZeilenQuickInfo);
            this.tabAllgemein.Controls.Add(this.butSystemspaltenErstellen);
            this.tabAllgemein.Controls.Add(this.btnOptimize);
            this.tabAllgemein.Controls.Add(this.txbStandardFormulaFile);
            this.tabAllgemein.Controls.Add(this.capStandardFormulaFile);
            this.tabAllgemein.Controls.Add(this.txbAdditionalFiles);
            this.tabAllgemein.Controls.Add(this.capAdditional);
            this.tabAllgemein.Controls.Add(this.capZeilenQuickInfo);
            this.tabAllgemein.Controls.Add(this.txbGlobalScale);
            this.tabAllgemein.Controls.Add(this.capGlobalScale);
            this.tabAllgemein.Controls.Add(this.txbTags);
            this.tabAllgemein.Controls.Add(this.btnSpaltenuebersicht);
            this.tabAllgemein.Controls.Add(this.capInfo);
            this.tabAllgemein.Controls.Add(this.capTags);
            this.tabAllgemein.Controls.Add(this.txbCaption);
            this.tabAllgemein.Controls.Add(this.capCaption);
            this.tabAllgemein.Location = new System.Drawing.Point(4, 29);
            this.tabAllgemein.Name = "tabAllgemein";
            this.tabAllgemein.Padding = new System.Windows.Forms.Padding(3);
            this.tabAllgemein.Size = new System.Drawing.Size(1233, 674);
            this.tabAllgemein.TabIndex = 1;
            this.tabAllgemein.Text = "Allgemein";
            // 
            // btnTabellenAnsicht
            // 
            this.btnTabellenAnsicht.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnTabellenAnsicht.ImageCode = "Tabelle|16";
            this.btnTabellenAnsicht.Location = new System.Drawing.Point(216, 544);
            this.btnTabellenAnsicht.Name = "btnTabellenAnsicht";
            this.btnTabellenAnsicht.Size = new System.Drawing.Size(200, 32);
            this.btnTabellenAnsicht.TabIndex = 53;
            this.btnTabellenAnsicht.Text = "Tabellenansicht";
            this.btnTabellenAnsicht.Click += new System.EventHandler(this.btnTabellenAnsicht_Click);
            // 
            // btnSkripte
            // 
            this.btnSkripte.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSkripte.ImageCode = "Skript|16";
            this.btnSkripte.Location = new System.Drawing.Point(216, 624);
            this.btnSkripte.Name = "btnSkripte";
            this.btnSkripte.Size = new System.Drawing.Size(200, 32);
            this.btnSkripte.TabIndex = 52;
            this.btnSkripte.Text = "Skripte";
            this.btnSkripte.Click += new System.EventHandler(this.btnSkripte_Click);
            // 
            // btnSpaltenAnordnungen
            // 
            this.btnSpaltenAnordnungen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSpaltenAnordnungen.ImageCode = "Spalte|16";
            this.btnSpaltenAnordnungen.Location = new System.Drawing.Point(216, 584);
            this.btnSpaltenAnordnungen.Name = "btnSpaltenAnordnungen";
            this.btnSpaltenAnordnungen.Size = new System.Drawing.Size(200, 32);
            this.btnSpaltenAnordnungen.TabIndex = 51;
            this.btnSpaltenAnordnungen.Text = "Spaltenanordnung";
            this.btnSpaltenAnordnungen.Click += new System.EventHandler(this.btnSpaltenAnordnungen_Click);
            // 
            // txbZeilenQuickInfo
            // 
            this.txbZeilenQuickInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txbZeilenQuickInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbZeilenQuickInfo.Location = new System.Drawing.Point(8, 216);
            this.txbZeilenQuickInfo.MultiLine = true;
            this.txbZeilenQuickInfo.Name = "txbZeilenQuickInfo";
            this.txbZeilenQuickInfo.Size = new System.Drawing.Size(608, 186);
            this.txbZeilenQuickInfo.TabIndex = 43;
            this.txbZeilenQuickInfo.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // butSystemspaltenErstellen
            // 
            this.butSystemspaltenErstellen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butSystemspaltenErstellen.ImageCode = "Spalte|16";
            this.butSystemspaltenErstellen.Location = new System.Drawing.Point(8, 624);
            this.butSystemspaltenErstellen.Name = "butSystemspaltenErstellen";
            this.butSystemspaltenErstellen.Size = new System.Drawing.Size(200, 32);
            this.butSystemspaltenErstellen.TabIndex = 49;
            this.butSystemspaltenErstellen.Text = "Alle Systemspalten erstellen";
            this.butSystemspaltenErstellen.Click += new System.EventHandler(this.butSystemspaltenErstellen_Click);
            // 
            // btnOptimize
            // 
            this.btnOptimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOptimize.ImageCode = "Blitz|16";
            this.btnOptimize.Location = new System.Drawing.Point(8, 584);
            this.btnOptimize.Name = "btnOptimize";
            this.btnOptimize.QuickInfo = "Stellt alle Spalten um, \r\ndass die Daten";
            this.btnOptimize.Size = new System.Drawing.Size(200, 32);
            this.btnOptimize.TabIndex = 48;
            this.btnOptimize.Text = "Datenbank optimieren";
            this.btnOptimize.Click += new System.EventHandler(this.btnOptimize_Click);
            // 
            // txbStandardFormulaFile
            // 
            this.txbStandardFormulaFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txbStandardFormulaFile.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbStandardFormulaFile.Location = new System.Drawing.Point(8, 482);
            this.txbStandardFormulaFile.Name = "txbStandardFormulaFile";
            this.txbStandardFormulaFile.Size = new System.Drawing.Size(608, 24);
            this.txbStandardFormulaFile.TabIndex = 47;
            this.txbStandardFormulaFile.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capStandardFormulaFile
            // 
            this.capStandardFormulaFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.capStandardFormulaFile.CausesValidation = false;
            this.capStandardFormulaFile.Location = new System.Drawing.Point(8, 466);
            this.capStandardFormulaFile.Name = "capStandardFormulaFile";
            this.capStandardFormulaFile.Size = new System.Drawing.Size(152, 18);
            this.capStandardFormulaFile.Text = "Standard-Formular-Datei:";
            // 
            // txbAdditionalFiles
            // 
            this.txbAdditionalFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txbAdditionalFiles.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbAdditionalFiles.Location = new System.Drawing.Point(8, 434);
            this.txbAdditionalFiles.Name = "txbAdditionalFiles";
            this.txbAdditionalFiles.Size = new System.Drawing.Size(608, 24);
            this.txbAdditionalFiles.TabIndex = 45;
            this.txbAdditionalFiles.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capAdditional
            // 
            this.capAdditional.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.capAdditional.CausesValidation = false;
            this.capAdditional.Location = new System.Drawing.Point(8, 418);
            this.capAdditional.Name = "capAdditional";
            this.capAdditional.Size = new System.Drawing.Size(152, 18);
            this.capAdditional.Text = "Zugehörige-Dateien-Pfad:";
            // 
            // capZeilenQuickInfo
            // 
            this.capZeilenQuickInfo.CausesValidation = false;
            this.capZeilenQuickInfo.Location = new System.Drawing.Point(8, 200);
            this.capZeilenQuickInfo.Name = "capZeilenQuickInfo";
            this.capZeilenQuickInfo.Size = new System.Drawing.Size(152, 18);
            this.capZeilenQuickInfo.Text = "Zeilen-Quick-Info: ";
            // 
            // txbGlobalScale
            // 
            this.txbGlobalScale.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txbGlobalScale.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbGlobalScale.Location = new System.Drawing.Point(160, 514);
            this.txbGlobalScale.Name = "txbGlobalScale";
            this.txbGlobalScale.Size = new System.Drawing.Size(160, 24);
            this.txbGlobalScale.TabIndex = 39;
            // 
            // capGlobalScale
            // 
            this.capGlobalScale.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.capGlobalScale.CausesValidation = false;
            this.capGlobalScale.Location = new System.Drawing.Point(8, 514);
            this.capGlobalScale.Name = "capGlobalScale";
            this.capGlobalScale.Size = new System.Drawing.Size(144, 18);
            this.capGlobalScale.Text = "Bevorzugtes Skalierung:";
            // 
            // btnSpaltenuebersicht
            // 
            this.btnSpaltenuebersicht.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSpaltenuebersicht.ImageCode = "Spalte|16";
            this.btnSpaltenuebersicht.Location = new System.Drawing.Point(8, 544);
            this.btnSpaltenuebersicht.Name = "btnSpaltenuebersicht";
            this.btnSpaltenuebersicht.Size = new System.Drawing.Size(200, 32);
            this.btnSpaltenuebersicht.TabIndex = 35;
            this.btnSpaltenuebersicht.Text = "Spaltenübersicht";
            this.btnSpaltenuebersicht.Click += new System.EventHandler(this.btnSpaltenuebersicht_Click);
            // 
            // tabRechte
            // 
            this.tabRechte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabRechte.Controls.Add(this.grpKennwort);
            this.tabRechte.Controls.Add(this.grpBenutzergruppen);
            this.tabRechte.Location = new System.Drawing.Point(4, 29);
            this.tabRechte.Name = "tabRechte";
            this.tabRechte.Padding = new System.Windows.Forms.Padding(3);
            this.tabRechte.Size = new System.Drawing.Size(1233, 674);
            this.tabRechte.TabIndex = 4;
            this.tabRechte.Text = "Rechte";
            // 
            // tabSortierung
            // 
            this.tabSortierung.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabSortierung.Controls.Add(this.lbxSortierSpalten);
            this.tabSortierung.Controls.Add(this.capSortierspalten);
            this.tabSortierung.Controls.Add(this.btnSortRichtung);
            this.tabSortierung.Location = new System.Drawing.Point(4, 29);
            this.tabSortierung.Name = "tabSortierung";
            this.tabSortierung.Padding = new System.Windows.Forms.Padding(3);
            this.tabSortierung.Size = new System.Drawing.Size(1233, 674);
            this.tabSortierung.TabIndex = 2;
            this.tabSortierung.Text = "Sortierung";
            // 
            // tabVariablen
            // 
            this.tabVariablen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabVariablen.Controls.Add(this.variableEditor);
            this.tabVariablen.Location = new System.Drawing.Point(4, 29);
            this.tabVariablen.Name = "tabVariablen";
            this.tabVariablen.Padding = new System.Windows.Forms.Padding(3);
            this.tabVariablen.Size = new System.Drawing.Size(1233, 674);
            this.tabVariablen.TabIndex = 7;
            this.tabVariablen.Text = "Variablen";
            // 
            // variableEditor
            // 
            this.variableEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.variableEditor.Editabe = true;
            this.variableEditor.Location = new System.Drawing.Point(3, 3);
            this.variableEditor.Name = "variableEditor";
            this.variableEditor.Size = new System.Drawing.Size(1227, 668);
            this.variableEditor.TabIndex = 1;
            // 
            // tabUndo
            // 
            this.tabUndo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabUndo.Controls.Add(this.tblUndo);
            this.tabUndo.Controls.Add(this.grpUndoActions);
            this.tabUndo.Location = new System.Drawing.Point(4, 29);
            this.tabUndo.Name = "tabUndo";
            this.tabUndo.Size = new System.Drawing.Size(1233, 674);
            this.tabUndo.TabIndex = 6;
            this.tabUndo.Text = "Undo";
            // 
            // tblUndo
            // 
            this.tblUndo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblUndo.Location = new System.Drawing.Point(0, 0);
            this.tblUndo.Name = "tblUndo";
            this.tblUndo.Size = new System.Drawing.Size(1233, 604);
            this.tblUndo.TabIndex = 0;
            this.tblUndo.Text = "UndoTab";
            this.tblUndo.ContextMenuInit += new System.EventHandler<BlueControls.EventArgs.ContextMenuInitEventArgs>(this.tblUndo_ContextMenuInit);
            this.tblUndo.ContextMenuItemClicked += new System.EventHandler<BlueControls.EventArgs.ContextMenuItemClickedEventArgs>(this.tblUndo_ContextMenuItemClicked);
            // 
            // grpUndoActions
            // 
            this.grpUndoActions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpUndoActions.Controls.Add(this.btnClipboard);
            this.grpUndoActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.grpUndoActions.Location = new System.Drawing.Point(0, 604);
            this.grpUndoActions.Name = "grpUndoActions";
            this.grpUndoActions.Size = new System.Drawing.Size(1233, 70);
            this.grpUndoActions.TabIndex = 4;
            this.grpUndoActions.TabStop = false;
            this.grpUndoActions.Text = "Aktionen";
            // 
            // btnClipboard
            // 
            this.btnClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClipboard.ImageCode = "Clipboard|24";
            this.btnClipboard.Location = new System.Drawing.Point(16, 24);
            this.btnClipboard.Name = "btnClipboard";
            this.btnClipboard.Size = new System.Drawing.Size(176, 40);
            this.btnClipboard.TabIndex = 3;
            this.btnClipboard.Text = "In die Zwischenablage";
            this.btnClipboard.Click += new System.EventHandler(this.btnClipboard_Click);
            // 
            // DatabaseHeadEditor
            // 
            this.ClientSize = new System.Drawing.Size(1244, 756);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.GlobalTab);
            this.MinimizeBox = false;
            this.Name = "DatabaseHeadEditor";
            this.ShowInTaskbar = false;
            this.Text = "Datenbank-Eigenschaften";
            this.Controls.SetChildIndex(this.GlobalTab, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.btnOk, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.grpBenutzergruppen.ResumeLayout(false);
            this.grpKennwort.ResumeLayout(false);
            this.GlobalTab.ResumeLayout(false);
            this.tabAllgemein.ResumeLayout(false);
            this.tabRechte.ResumeLayout(false);
            this.tabSortierung.ResumeLayout(false);
            this.tabVariablen.ResumeLayout(false);
            this.tabUndo.ResumeLayout(false);
            this.grpUndoActions.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Caption capInfo;
        private Button btnOk;
        private Button btnSortRichtung;
        private Caption capSortierspalten;
        private TextBox txbKennwort;
        private Caption capNeueZeilen;
        private ListBox PermissionGroups_NewRow;
        private Caption capTags;
        private Caption capCaption;
        private TextBox txbCaption;
        private TextBox txbTags;
        private ListBox lbxSortierSpalten;
        private Caption Caption22;
        private ListBox DatenbankAdmin;
        private TabPage tabAllgemein;
        private TabPage tabSortierung;
        private TabPage tabRechte;
        private GroupBox grpKennwort;
        private GroupBox grpBenutzergruppen;
        private TabPage tabUndo;
        private Caption capKennwort;
        private Button btnSpaltenuebersicht;
        private Table tblUndo;
        private Caption capNeueZeilenInfo;
        private TextBox txbGlobalScale;
        private Caption capGlobalScale;
        private TabControl GlobalTab;
        private TextBox txbZeilenQuickInfo;
        private Caption capZeilenQuickInfo;
        private TextBox txbAdditionalFiles;
        private Caption capAdditional;
        private Button btnClipboard;
        private TextBox txbStandardFormulaFile;
        private Caption capStandardFormulaFile;
        private Button btnOptimize;
        private Button butSystemspaltenErstellen;
        private GroupBox grpUndoActions;
        private TabPage tabVariablen;
        private VariableEditor variableEditor;
        private Button btnDummyAdmin;
        private Button btnSpaltenAnordnungen;
        private Button btnSkripte;
        private Button btnTabellenAnsicht;
    }
}
