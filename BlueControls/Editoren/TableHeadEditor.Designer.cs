using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;
using TabControl = BlueControls.Controls.TabControl;
using TabPage = System.Windows.Forms.TabPage;
using TextBox = BlueControls.Controls.TextBox;
using Caption = BlueControls.Controls.Caption;

namespace BlueControls.BlueTableDialogs {
    public sealed partial class TableHeadEditor {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(TableHeadEditor));
            grpBenutzergruppen = new GroupBox();
            btnDummyAdmin = new Button();
            PermissionGroups_NewRow = new ListBox();
            capNeueZeilenInfo = new Caption();
            capTableAdmins = new Caption();
            lbxTableAdmin = new ListBox();
            capNeueZeilen = new Caption();
            grpKennwort = new GroupBox();
            capKennwort = new Caption();
            txbKennwort = new TextBox();
            btnOk = new Button();
            txbTags = new TextBox();
            txbCaption = new TextBox();
            capCaption = new Caption();
            capTags = new Caption();
            capInfo = new Caption();
            GlobalTab = new TabControl();
            tabAllgemein = new TabPage();
            btnLoadAll = new Button();
            btnMasterMe = new Button();
            btnUnMaster = new Button();
            btnTabellenAnsicht = new Button();
            btnSkripte = new Button();
            btnSpaltenAnordnungen = new Button();
            txbZeilenQuickInfo = new TextBox();
            butSystemspaltenErstellen = new Button();
            btnOptimize = new Button();
            txbStandardFormulaFile = new TextBox();
            capStandardFormulaFile = new Caption();
            txbAssetFolder = new TextBox();
            capAdditional = new Caption();
            capZeilenQuickInfo = new Caption();
            btnSpaltenuebersicht = new Button();
            tabRechte = new TabPage();
            tabSortierung = new TabPage();
            rowSortDefinitionEditor = new BlueControls.Forms.RowSortDefinitionEditor();
            tabUniqueValues = new TabPage();
            capUniqueInfo = new Caption();
            uniqueValueDefinitionEditor = new BlueControls.Forms.UniqueValueDefinitionEditor();
            lstUniqueValues = new ListBox();
            tabVariablen = new TabPage();
            variableEditor = new VariableEditor();
            tabDictionary = new TabPage();
            txbDictionary = new TextBox();
            capDictionary = new Caption();
            tabUndo = new TabPage();
            tblUndo = new TableViewWithFilters();
            grpUndoActions = new GroupBox();
            pnlStatusBar.SuspendLayout();
            grpBenutzergruppen.SuspendLayout();
            grpKennwort.SuspendLayout();
            GlobalTab.SuspendLayout();
            tabAllgemein.SuspendLayout();
            tabRechte.SuspendLayout();
            tabSortierung.SuspendLayout();
            tabUniqueValues.SuspendLayout();
            tabVariablen.SuspendLayout();
            tabDictionary.SuspendLayout();
            tabUndo.SuspendLayout();
            SuspendLayout();
            // 
            // capStatusBar
            // 
            capStatusBar.Size = new Size(1189, 24);
            // 
            // pnlStatusBar
            // 
            pnlStatusBar.Location = new Point(0, 732);
            pnlStatusBar.Size = new Size(1189, 24);
            // 
            // grpBenutzergruppen
            // 
            grpBenutzergruppen.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpBenutzergruppen.BackColor = Color.FromArgb(255, 255, 255);
            grpBenutzergruppen.CausesValidation = false;
            grpBenutzergruppen.Controls.Add(btnDummyAdmin);
            grpBenutzergruppen.Controls.Add(PermissionGroups_NewRow);
            grpBenutzergruppen.Controls.Add(capNeueZeilenInfo);
            grpBenutzergruppen.Controls.Add(capTableAdmins);
            grpBenutzergruppen.Controls.Add(lbxTableAdmin);
            grpBenutzergruppen.Controls.Add(capNeueZeilen);
            grpBenutzergruppen.Location = new Point(8, 8);
            grpBenutzergruppen.Name = "grpBenutzergruppen";
            grpBenutzergruppen.Size = new Size(408, 656);
            grpBenutzergruppen.TabIndex = 2;
            grpBenutzergruppen.TabStop = false;
            grpBenutzergruppen.Text = "Benutzergruppen:";
            // 
            // btnDummyAdmin
            // 
            btnDummyAdmin.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnDummyAdmin.ButtonStyle = ButtonStyle.Checkbox_Text;
            btnDummyAdmin.Checked = true;
            btnDummyAdmin.Enabled = false;
            btnDummyAdmin.Location = new Point(192, 48);
            btnDummyAdmin.Name = "btnDummyAdmin";
            btnDummyAdmin.Size = new Size(208, 16);
            btnDummyAdmin.TabIndex = 17;
            btnDummyAdmin.Text = "#Administrator";
            // 
            // PermissionGroups_NewRow
            // 
            PermissionGroups_NewRow.AddAllowed = AddType.Text;
            PermissionGroups_NewRow.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PermissionGroups_NewRow.Appearance = ListBoxAppearance.Listbox_Boxes;
            PermissionGroups_NewRow.CheckBehavior = CheckBehavior.MultiSelection;
            PermissionGroups_NewRow.FilterText = null;
            PermissionGroups_NewRow.Location = new Point(192, 64);
            PermissionGroups_NewRow.Name = "PermissionGroups_NewRow";
            PermissionGroups_NewRow.RemoveAllowed = true;
            PermissionGroups_NewRow.Size = new Size(208, 544);
            PermissionGroups_NewRow.TabIndex = 4;
            PermissionGroups_NewRow.Translate = false;
            // 
            // capNeueZeilenInfo
            // 
            capNeueZeilenInfo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            capNeueZeilenInfo.CausesValidation = false;
            capNeueZeilenInfo.Location = new Point(192, 608);
            capNeueZeilenInfo.Name = "capNeueZeilenInfo";
            capNeueZeilenInfo.Size = new Size(208, 40);
            capNeueZeilenInfo.Text = "<i>Die erste Spalte muss eine Bearbeitung zulassen";
            // 
            // capTableAdmins
            // 
            capTableAdmins.CausesValidation = false;
            capTableAdmins.Location = new Point(8, 24);
            capTableAdmins.Name = "capTableAdmins";
            capTableAdmins.Size = new Size(176, 22);
            capTableAdmins.Text = "Tabellen-Administratoren:";
            // 
            // lbxTableAdmin
            // 
            lbxTableAdmin.AddAllowed = AddType.Text;
            lbxTableAdmin.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lbxTableAdmin.Appearance = ListBoxAppearance.Listbox_Boxes;
            lbxTableAdmin.CheckBehavior = CheckBehavior.MultiSelection;
            lbxTableAdmin.FilterText = null;
            lbxTableAdmin.Location = new Point(8, 46);
            lbxTableAdmin.Name = "lbxTableAdmin";
            lbxTableAdmin.Size = new Size(176, 602);
            lbxTableAdmin.TabIndex = 4;
            lbxTableAdmin.Translate = false;
            // 
            // capNeueZeilen
            // 
            capNeueZeilen.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            capNeueZeilen.CausesValidation = false;
            capNeueZeilen.Location = new Point(192, 24);
            capNeueZeilen.Name = "capNeueZeilen";
            capNeueZeilen.Size = new Size(208, 24);
            capNeueZeilen.Text = "Neue Zeilen anlegen:";
            // 
            // grpKennwort
            // 
            grpKennwort.BackColor = Color.FromArgb(255, 255, 255);
            grpKennwort.CausesValidation = false;
            grpKennwort.Controls.Add(capKennwort);
            grpKennwort.Controls.Add(txbKennwort);
            grpKennwort.Location = new Point(424, 8);
            grpKennwort.Name = "grpKennwort";
            grpKennwort.Size = new Size(232, 96);
            grpKennwort.TabIndex = 1;
            grpKennwort.TabStop = false;
            grpKennwort.Text = "Kennwort:";
            // 
            // capKennwort
            // 
            capKennwort.CausesValidation = false;
            capKennwort.Location = new Point(8, 24);
            capKennwort.Name = "capKennwort";
            capKennwort.Size = new Size(216, 22);
            capKennwort.Text = "Zum Öffnen der Tabelle:";
            // 
            // txbKennwort
            // 
            txbKennwort.Cursor = Cursors.IBeam;
            txbKennwort.Location = new Point(8, 56);
            txbKennwort.Name = "txbKennwort";
            txbKennwort.Size = new Size(216, 22);
            txbKennwort.TabIndex = 4;
            // 
            // btnOk
            // 
            btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOk.ImageCode = "Häkchen|24";
            btnOk.Location = new Point(1089, 715);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(88, 32);
            btnOk.TabIndex = 11;
            btnOk.Text = "OK";
            btnOk.Click += OkBut_Click;
            // 
            // txbTags
            // 
            txbTags.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txbTags.Cursor = Cursors.IBeam;
            txbTags.Location = new Point(632, 24);
            txbTags.MultiLine = true;
            txbTags.Name = "txbTags";
            txbTags.Size = new Size(536, 643);
            txbTags.TabIndex = 26;
            txbTags.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbCaption
            // 
            txbCaption.Cursor = Cursors.IBeam;
            txbCaption.Location = new Point(8, 24);
            txbCaption.Name = "txbCaption";
            txbCaption.Size = new Size(616, 24);
            txbCaption.TabIndex = 24;
            // 
            // capCaption
            // 
            capCaption.CausesValidation = false;
            capCaption.Location = new Point(8, 8);
            capCaption.Name = "capCaption";
            capCaption.Size = new Size(137, 16);
            capCaption.Text = "Überschrift bzw. Titel:";
            // 
            // capTags
            // 
            capTags.CausesValidation = false;
            capTags.Location = new Point(632, 8);
            capTags.Name = "capTags";
            capTags.QuickInfo = "Tags / Eigenschaften, die von einem";
            capTags.Size = new Size(152, 16);
            capTags.Text = "Tags:";
            // 
            // capInfo
            // 
            capInfo.CausesValidation = false;
            capInfo.Location = new Point(8, 56);
            capInfo.Name = "capInfo";
            capInfo.Size = new Size(616, 144);
            // 
            // GlobalTab
            // 
            GlobalTab.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            GlobalTab.Controls.Add(tabAllgemein);
            GlobalTab.Controls.Add(tabRechte);
            GlobalTab.Controls.Add(tabSortierung);
            GlobalTab.Controls.Add(tabUniqueValues);
            GlobalTab.Controls.Add(tabVariablen);
            GlobalTab.Controls.Add(tabDictionary);
            GlobalTab.Controls.Add(tabUndo);
            GlobalTab.HotTrack = true;
            GlobalTab.Location = new Point(0, 0);
            GlobalTab.Name = "GlobalTab";
            GlobalTab.SelectedIndex = 0;
            GlobalTab.Size = new Size(1186, 707);
            GlobalTab.TabDefault = null;
            GlobalTab.TabDefaultOrder = null;
            GlobalTab.TabIndex = 21;
            GlobalTab.SelectedIndexChanged += GlobalTab_SelectedIndexChanged;
            // 
            // tabAllgemein
            // 
            tabAllgemein.BackColor = Color.FromArgb(255, 255, 255);
            tabAllgemein.Controls.Add(btnLoadAll);
            tabAllgemein.Controls.Add(btnMasterMe);
            tabAllgemein.Controls.Add(btnUnMaster);
            tabAllgemein.Controls.Add(btnTabellenAnsicht);
            tabAllgemein.Controls.Add(btnSkripte);
            tabAllgemein.Controls.Add(btnSpaltenAnordnungen);
            tabAllgemein.Controls.Add(txbZeilenQuickInfo);
            tabAllgemein.Controls.Add(butSystemspaltenErstellen);
            tabAllgemein.Controls.Add(btnOptimize);
            tabAllgemein.Controls.Add(txbStandardFormulaFile);
            tabAllgemein.Controls.Add(capStandardFormulaFile);
            tabAllgemein.Controls.Add(txbAssetFolder);
            tabAllgemein.Controls.Add(capAdditional);
            tabAllgemein.Controls.Add(capZeilenQuickInfo);
            tabAllgemein.Controls.Add(txbTags);
            tabAllgemein.Controls.Add(btnSpaltenuebersicht);
            tabAllgemein.Controls.Add(capInfo);
            tabAllgemein.Controls.Add(capTags);
            tabAllgemein.Controls.Add(txbCaption);
            tabAllgemein.Controls.Add(capCaption);
            tabAllgemein.Location = new Point(4, 25);
            tabAllgemein.Name = "tabAllgemein";
            tabAllgemein.Padding = new Padding(3);
            tabAllgemein.Size = new Size(1178, 678);
            tabAllgemein.TabIndex = 1;
            tabAllgemein.Text = "Allgemein";
            // 
            // btnLoadAll
            // 
            btnLoadAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnLoadAll.ImageCode = "Ordner|16";
            btnLoadAll.Location = new Point(424, 632);
            btnLoadAll.Name = "btnLoadAll";
            btnLoadAll.Size = new Size(200, 32);
            btnLoadAll.TabIndex = 56;
            btnLoadAll.Text = "Alle Daten laden";
            btnLoadAll.Click += btnLoadAll_Click;
            // 
            // btnMasterMe
            // 
            btnMasterMe.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnMasterMe.ImageCode = "Stern|16|";
            btnMasterMe.Location = new Point(424, 552);
            btnMasterMe.Name = "btnMasterMe";
            btnMasterMe.Size = new Size(200, 32);
            btnMasterMe.TabIndex = 55;
            btnMasterMe.Text = "Master me!";
            btnMasterMe.Click += btnMasterMe_Click;
            // 
            // btnUnMaster
            // 
            btnUnMaster.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUnMaster.ImageCode = "Stern|16|||||||||Kreuz";
            btnUnMaster.Location = new Point(424, 592);
            btnUnMaster.Name = "btnUnMaster";
            btnUnMaster.Size = new Size(200, 32);
            btnUnMaster.TabIndex = 54;
            btnUnMaster.Text = "Unmaster me!";
            btnUnMaster.Click += btnUnMaster_Click;
            // 
            // btnTabellenAnsicht
            // 
            btnTabellenAnsicht.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnTabellenAnsicht.ImageCode = "Tabelle|16";
            btnTabellenAnsicht.Location = new Point(216, 552);
            btnTabellenAnsicht.Name = "btnTabellenAnsicht";
            btnTabellenAnsicht.Size = new Size(200, 32);
            btnTabellenAnsicht.TabIndex = 53;
            btnTabellenAnsicht.Text = "Tabellenansicht";
            btnTabellenAnsicht.Click += btnTabellenAnsicht_Click;
            // 
            // btnSkripte
            // 
            btnSkripte.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSkripte.ImageCode = "Skript|16";
            btnSkripte.Location = new Point(216, 632);
            btnSkripte.Name = "btnSkripte";
            btnSkripte.Size = new Size(200, 32);
            btnSkripte.TabIndex = 52;
            btnSkripte.Text = "Skripte";
            btnSkripte.Click += btnSkripte_Click;
            // 
            // btnSpaltenAnordnungen
            // 
            btnSpaltenAnordnungen.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSpaltenAnordnungen.ImageCode = "Spalte|16";
            btnSpaltenAnordnungen.Location = new Point(216, 592);
            btnSpaltenAnordnungen.Name = "btnSpaltenAnordnungen";
            btnSpaltenAnordnungen.Size = new Size(200, 32);
            btnSpaltenAnordnungen.TabIndex = 51;
            btnSpaltenAnordnungen.Text = "Spaltenanordnung";
            btnSpaltenAnordnungen.Click += btnSpaltenAnordnungen_Click;
            // 
            // txbZeilenQuickInfo
            // 
            txbZeilenQuickInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            txbZeilenQuickInfo.Cursor = Cursors.IBeam;
            txbZeilenQuickInfo.Location = new Point(8, 216);
            txbZeilenQuickInfo.MultiLine = true;
            txbZeilenQuickInfo.Name = "txbZeilenQuickInfo";
            txbZeilenQuickInfo.Size = new Size(616, 190);
            txbZeilenQuickInfo.TabIndex = 43;
            txbZeilenQuickInfo.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // butSystemspaltenErstellen
            // 
            butSystemspaltenErstellen.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            butSystemspaltenErstellen.ImageCode = "Spalte|16";
            butSystemspaltenErstellen.Location = new Point(8, 632);
            butSystemspaltenErstellen.Name = "butSystemspaltenErstellen";
            butSystemspaltenErstellen.Size = new Size(200, 32);
            butSystemspaltenErstellen.TabIndex = 49;
            butSystemspaltenErstellen.Text = "Alle Systemspalten erstellen";
            butSystemspaltenErstellen.Click += butSystemspaltenErstellen_Click;
            // 
            // btnOptimize
            // 
            btnOptimize.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnOptimize.ImageCode = "Blitz|16";
            btnOptimize.Location = new Point(8, 592);
            btnOptimize.Name = "btnOptimize";
            btnOptimize.QuickInfo = "Stellt alle Spalten um, \r\ndass die Daten";
            btnOptimize.Size = new Size(200, 32);
            btnOptimize.TabIndex = 48;
            btnOptimize.Text = "Tabelle optimieren";
            btnOptimize.Click += btnOptimize_Click;
            // 
            // txbStandardFormulaFile
            // 
            txbStandardFormulaFile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txbStandardFormulaFile.Cursor = Cursors.IBeam;
            txbStandardFormulaFile.Location = new Point(8, 486);
            txbStandardFormulaFile.Name = "txbStandardFormulaFile";
            txbStandardFormulaFile.Size = new Size(616, 24);
            txbStandardFormulaFile.TabIndex = 47;
            txbStandardFormulaFile.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capStandardFormulaFile
            // 
            capStandardFormulaFile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            capStandardFormulaFile.CausesValidation = false;
            capStandardFormulaFile.Location = new Point(8, 470);
            capStandardFormulaFile.Name = "capStandardFormulaFile";
            capStandardFormulaFile.Size = new Size(152, 18);
            capStandardFormulaFile.Text = "Standard-Formular-Datei:";
            // 
            // txbAssetFolder
            // 
            txbAssetFolder.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txbAssetFolder.Cursor = Cursors.IBeam;
            txbAssetFolder.Location = new Point(8, 438);
            txbAssetFolder.Name = "txbAssetFolder";
            txbAssetFolder.Size = new Size(616, 24);
            txbAssetFolder.TabIndex = 45;
            txbAssetFolder.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capAdditional
            // 
            capAdditional.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            capAdditional.CausesValidation = false;
            capAdditional.Location = new Point(8, 422);
            capAdditional.Name = "capAdditional";
            capAdditional.Size = new Size(152, 18);
            capAdditional.Text = "Zugehörige-Dateien-Pfad:";
            // 
            // capZeilenQuickInfo
            // 
            capZeilenQuickInfo.CausesValidation = false;
            capZeilenQuickInfo.Location = new Point(8, 200);
            capZeilenQuickInfo.Name = "capZeilenQuickInfo";
            capZeilenQuickInfo.Size = new Size(152, 18);
            capZeilenQuickInfo.Text = "Zeilen-Quick-Info: ";
            // 
            // btnSpaltenuebersicht
            // 
            btnSpaltenuebersicht.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSpaltenuebersicht.ImageCode = "Spalte|16";
            btnSpaltenuebersicht.Location = new Point(8, 552);
            btnSpaltenuebersicht.Name = "btnSpaltenuebersicht";
            btnSpaltenuebersicht.Size = new Size(200, 32);
            btnSpaltenuebersicht.TabIndex = 35;
            btnSpaltenuebersicht.Text = "Spaltenübersicht";
            btnSpaltenuebersicht.Click += btnSpaltenuebersicht_Click;
            // 
            // tabRechte
            // 
            tabRechte.BackColor = Color.FromArgb(255, 255, 255);
            tabRechte.Controls.Add(grpKennwort);
            tabRechte.Controls.Add(grpBenutzergruppen);
            tabRechte.Location = new Point(4, 25);
            tabRechte.Name = "tabRechte";
            tabRechte.Padding = new Padding(3);
            tabRechte.Size = new Size(1178, 678);
            tabRechte.TabIndex = 4;
            tabRechte.Text = "Rechte";
            // 
            // tabSortierung
            // 
            tabSortierung.BackColor = Color.FromArgb(255, 255, 255);
            tabSortierung.Controls.Add(rowSortDefinitionEditor);
            tabSortierung.Location = new Point(4, 25);
            tabSortierung.Name = "tabSortierung";
            tabSortierung.Padding = new Padding(3);
            tabSortierung.Size = new Size(1178, 678);
            tabSortierung.TabIndex = 2;
            tabSortierung.Text = "Sortierung";
            // 
            // rowSortDefinitionEditor
            // 
            rowSortDefinitionEditor.Editable = false;
            rowSortDefinitionEditor.Location = new Point(8, 8);
            rowSortDefinitionEditor.Name = "rowSortDefinitionEditor";
            rowSortDefinitionEditor.Size = new Size(392, 664);
            rowSortDefinitionEditor.TabIndex = 0;
            // 
            // tabUniqueValues
            // 
            tabUniqueValues.BackColor = Color.FromArgb(255, 255, 255);
            tabUniqueValues.Controls.Add(capUniqueInfo);
            tabUniqueValues.Controls.Add(uniqueValueDefinitionEditor);
            tabUniqueValues.Controls.Add(lstUniqueValues);
            tabUniqueValues.Location = new Point(4, 25);
            tabUniqueValues.Name = "tabUniqueValues";
            tabUniqueValues.Padding = new Padding(3);
            tabUniqueValues.Size = new Size(1178, 678);
            tabUniqueValues.TabIndex = 8;
            tabUniqueValues.Text = "Einzigartig";
            // 
            // capUniqueInfo
            // 
            capUniqueInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            capUniqueInfo.CausesValidation = false;
            capUniqueInfo.Location = new Point(8, 8);
            capUniqueInfo.Name = "capUniqueInfo";
            capUniqueInfo.Size = new Size(696, 72);
            capUniqueInfo.Text = resources.GetString("capUniqueInfo.Text");
            // 
            // uniqueValueDefinitionEditor
            // 
            uniqueValueDefinitionEditor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            uniqueValueDefinitionEditor.Editable = false;
            uniqueValueDefinitionEditor.Location = new Point(720, 8);
            uniqueValueDefinitionEditor.Name = "uniqueValueDefinitionEditor";
            uniqueValueDefinitionEditor.Size = new Size(452, 664);
            uniqueValueDefinitionEditor.TabIndex = 1;
            // 
            // lstUniqueValues
            // 
            lstUniqueValues.AddAllowed = AddType.UserDef;
            lstUniqueValues.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstUniqueValues.FilterText = null;
            lstUniqueValues.Location = new Point(8, 80);
            lstUniqueValues.Name = "lstUniqueValues";
            lstUniqueValues.RemoveAllowed = true;
            lstUniqueValues.Size = new Size(696, 592);
            lstUniqueValues.TabIndex = 0;
            lstUniqueValues.AddClicked += lstUniqueValues_AddClicked;
            lstUniqueValues.ItemCheckedChanged += lstUniqueValues_ItemCheckedChanged;
            lstUniqueValues.RemoveClicked += lstUniqueValues_RemoveClicked;
            // 
            // tabVariablen
            // 
            tabVariablen.BackColor = Color.FromArgb(255, 255, 255);
            tabVariablen.Controls.Add(variableEditor);
            tabVariablen.Location = new Point(4, 25);
            tabVariablen.Name = "tabVariablen";
            tabVariablen.Padding = new Padding(3);
            tabVariablen.Size = new Size(1178, 678);
            tabVariablen.TabIndex = 7;
            tabVariablen.Text = "Variablen";
            // 
            // variableEditor
            // 
            variableEditor.Dock = DockStyle.Fill;
            variableEditor.Editable = true;
            variableEditor.Location = new Point(3, 3);
            variableEditor.Name = "variableEditor";
            variableEditor.Size = new Size(1172, 672);
            variableEditor.TabIndex = 1;
            // 
            // tabDictionary
            // 
            tabDictionary.BackColor = Color.FromArgb(255, 255, 255);
            tabDictionary.Controls.Add(txbDictionary);
            tabDictionary.Controls.Add(capDictionary);
            tabDictionary.Location = new Point(4, 25);
            tabDictionary.Name = "tabDictionary";
            tabDictionary.Padding = new Padding(3);
            tabDictionary.Size = new Size(1178, 678);
            tabDictionary.TabIndex = 9;
            tabDictionary.Text = "Wörterbuch";
            // 
            // txbDictionary
            // 
            txbDictionary.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txbDictionary.Location = new Point(8, 32);
            txbDictionary.Name = "txbDictionary";
            txbDictionary.Size = new Size(1162, 638);
            txbDictionary.TabIndex = 1;
            // 
            // capDictionary
            // 
            capDictionary.CausesValidation = false;
            capDictionary.Location = new Point(8, 8);
            capDictionary.Name = "capDictionary";
            capDictionary.Size = new Size(300, 20);
            capDictionary.Text = "Zusätzliche Wörter für die Rechtschreibprüfung:";
            // 
            // tabUndo
            // 
            tabUndo.BackColor = Color.FromArgb(255, 255, 255);
            tabUndo.Controls.Add(tblUndo);
            tabUndo.Controls.Add(grpUndoActions);
            tabUndo.Location = new Point(4, 25);
            tabUndo.Name = "tabUndo";
            tabUndo.Size = new Size(1178, 678);
            tabUndo.TabIndex = 6;
            tabUndo.Text = "Undo";
            // 
            // tblUndo
            // 
            tblUndo.Dock = DockStyle.Fill;
            tblUndo.Location = new Point(0, 0);
            tblUndo.Name = "tblUndo";
            tblUndo.Size = new Size(1178, 608);
            tblUndo.TabIndex = 0;
            tblUndo.Text = "UndoTab";
            // 
            // grpUndoActions
            // 
            grpUndoActions.BackColor = Color.FromArgb(255, 255, 255);
            grpUndoActions.Dock = DockStyle.Bottom;
            grpUndoActions.Location = new Point(0, 608);
            grpUndoActions.Name = "grpUndoActions";
            grpUndoActions.Size = new Size(1178, 70);
            grpUndoActions.TabIndex = 4;
            grpUndoActions.TabStop = false;
            grpUndoActions.Text = "Aktionen";
            // 
            // TableHeadEditor
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1189, 756);
            Controls.Add(btnOk);
            Controls.Add(GlobalTab);
            GlobalMenuHeight = 0;
            MinimizeBox = false;
            Name = "TableHeadEditor";
            ShowInTaskbar = false;
            Text = "Tabellen-Eigenschaften";
            Controls.SetChildIndex(GlobalTab, 0);
            Controls.SetChildIndex(pnlStatusBar, 0);
            Controls.SetChildIndex(btnOk, 0);
            pnlStatusBar.ResumeLayout(false);
            grpBenutzergruppen.ResumeLayout(false);
            grpKennwort.ResumeLayout(false);
            GlobalTab.ResumeLayout(false);
            tabAllgemein.ResumeLayout(false);
            tabRechte.ResumeLayout(false);
            tabSortierung.ResumeLayout(false);
            tabUniqueValues.ResumeLayout(false);
            tabVariablen.ResumeLayout(false);
            tabDictionary.ResumeLayout(false);
            tabUndo.ResumeLayout(false);
            ResumeLayout(false);

        }
        private Caption capInfo;
        private Button btnOk;
        private TextBox txbKennwort;
        private Caption capNeueZeilen;
        private ListBox PermissionGroups_NewRow;
        private Caption capTags;
        private Caption capCaption;
        private TextBox txbCaption;
        private TextBox txbTags;
        private Caption capTableAdmins;
        private ListBox lbxTableAdmin;
        private TabPage tabAllgemein;
        private TabPage tabSortierung;
        private TabPage tabRechte;
        private GroupBox grpKennwort;
        private GroupBox grpBenutzergruppen;
        private TabPage tabUndo;
        private Caption capKennwort;
        private Button btnSpaltenuebersicht;
        private TableViewWithFilters tblUndo;
        private Caption capNeueZeilenInfo;
        private TabControl GlobalTab;
        private TextBox txbZeilenQuickInfo;
        private Caption capZeilenQuickInfo;
        private TextBox txbAssetFolder;
        private Caption capAdditional;
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
        private Button btnUnMaster;
        private Button btnMasterMe;
        private Button btnLoadAll;
        private Forms.RowSortDefinitionEditor rowSortDefinitionEditor;
        private TabPage tabUniqueValues;
        private ListBox lstUniqueValues;
        private Forms.UniqueValueDefinitionEditor uniqueValueDefinitionEditor;
        private Caption capUniqueInfo;
        private TabPage tabDictionary;
        private TextBox txbDictionary;
        private Caption capDictionary;
    }
}
