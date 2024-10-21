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
            this.grpBenutzergruppen = new GroupBox();
            this.btnDummyAdmin = new Button();
            this.PermissionGroups_NewRow = new ListBox();
            this.capNeueZeilenInfo = new Caption();
            this.Caption22 = new Caption();
            this.DatenbankAdmin = new ListBox();
            this.capNeueZeilen = new Caption();
            this.grpKennwort = new GroupBox();
            this.capKennwort = new Caption();
            this.txbKennwort = new TextBox();
            this.lbxSortierSpalten = new ListBox();
            this.capSortierspalten = new Caption();
            this.btnSortRichtung = new Button();
            this.btnOk = new Button();
            this.txbTags = new TextBox();
            this.txbCaption = new TextBox();
            this.capCaption = new Caption();
            this.capTags = new Caption();
            this.capInfo = new Caption();
            this.GlobalTab = new TabControl();
            this.tabAllgemein = new TabPage();
            this.btnTabellenAnsicht = new Button();
            this.btnSkripte = new Button();
            this.btnSpaltenAnordnungen = new Button();
            this.txbZeilenQuickInfo = new TextBox();
            this.butSystemspaltenErstellen = new Button();
            this.btnOptimize = new Button();
            this.txbStandardFormulaFile = new TextBox();
            this.capStandardFormulaFile = new Caption();
            this.txbAdditionalFiles = new TextBox();
            this.capAdditional = new Caption();
            this.capZeilenQuickInfo = new Caption();
            this.btnSpaltenuebersicht = new Button();
            this.tabRechte = new TabPage();
            this.tabSortierung = new TabPage();
            this.tabVariablen = new TabPage();
            this.variableEditor = new VariableEditor();
            this.tabUndo = new TabPage();
            this.tblUndo = new Table();
            this.grpUndoActions = new GroupBox();
            this.btnClipboard = new Button();
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
            this.capStatusBar.Size = new Size(1244, 24);
            this.capStatusBar.TextAnzeigeVerhalten = SteuerelementVerhalten.Text_Abschneiden;
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new Point(0, 732);
            this.pnlStatusBar.Size = new Size(1244, 24);
            // 
            // grpBenutzergruppen
            // 
            this.grpBenutzergruppen.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpBenutzergruppen.CausesValidation = false;
            this.grpBenutzergruppen.Controls.Add(this.btnDummyAdmin);
            this.grpBenutzergruppen.Controls.Add(this.PermissionGroups_NewRow);
            this.grpBenutzergruppen.Controls.Add(this.capNeueZeilenInfo);
            this.grpBenutzergruppen.Controls.Add(this.Caption22);
            this.grpBenutzergruppen.Controls.Add(this.DatenbankAdmin);
            this.grpBenutzergruppen.Controls.Add(this.capNeueZeilen);
            this.grpBenutzergruppen.Location = new Point(16, 16);
            this.grpBenutzergruppen.Name = "grpBenutzergruppen";
            this.grpBenutzergruppen.Size = new Size(376, 488);
            this.grpBenutzergruppen.TabIndex = 2;
            this.grpBenutzergruppen.TabStop = false;
            this.grpBenutzergruppen.Text = "Benutzergruppen:";
            // 
            // btnDummyAdmin
            // 
            this.btnDummyAdmin.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.btnDummyAdmin.Checked = true;
            this.btnDummyAdmin.Enabled = false;
            this.btnDummyAdmin.Location = new Point(192, 48);
            this.btnDummyAdmin.Name = "btnDummyAdmin";
            this.btnDummyAdmin.Size = new Size(176, 16);
            this.btnDummyAdmin.TabIndex = 17;
            this.btnDummyAdmin.Text = "#Administrator";
            // 
            // PermissionGroups_NewRow
            // 
            this.PermissionGroups_NewRow.AddAllowed = AddType.Text;
            this.PermissionGroups_NewRow.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                                   | AnchorStyles.Left)));
            this.PermissionGroups_NewRow.Appearance = ListBoxAppearance.Listbox_Boxes;
            this.PermissionGroups_NewRow.CheckBehavior = CheckBehavior.MultiSelection;
            this.PermissionGroups_NewRow.FilterText = null;
            this.PermissionGroups_NewRow.Location = new Point(192, 64);
            this.PermissionGroups_NewRow.Name = "PermissionGroups_NewRow";
            this.PermissionGroups_NewRow.RemoveAllowed = true;
            this.PermissionGroups_NewRow.Size = new Size(176, 376);
            this.PermissionGroups_NewRow.TabIndex = 4;
            this.PermissionGroups_NewRow.Translate = false;
            // 
            // capNeueZeilenInfo
            // 
            this.capNeueZeilenInfo.CausesValidation = false;
            this.capNeueZeilenInfo.Location = new Point(192, 440);
            this.capNeueZeilenInfo.Name = "capNeueZeilenInfo";
            this.capNeueZeilenInfo.Size = new Size(176, 40);
            this.capNeueZeilenInfo.Text = "<i>Die erste Spalte muss eine Bearbeitung zulassen";
            this.capNeueZeilenInfo.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption22
            // 
            this.Caption22.CausesValidation = false;
            this.Caption22.Location = new Point(8, 24);
            this.Caption22.Name = "Caption22";
            this.Caption22.Size = new Size(176, 22);
            this.Caption22.Text = "Datenbank-Administratoren:";
            this.Caption22.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // DatenbankAdmin
            // 
            this.DatenbankAdmin.AddAllowed = AddType.Text;
            this.DatenbankAdmin.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                          | AnchorStyles.Left)));
            this.DatenbankAdmin.Appearance = ListBoxAppearance.Listbox_Boxes;
            this.DatenbankAdmin.CheckBehavior = CheckBehavior.MultiSelection;
            this.DatenbankAdmin.FilterText = null;
            this.DatenbankAdmin.Location = new Point(8, 46);
            this.DatenbankAdmin.Name = "DatenbankAdmin";
            this.DatenbankAdmin.Size = new Size(176, 434);
            this.DatenbankAdmin.TabIndex = 4;
            this.DatenbankAdmin.Translate = false;
            // 
            // capNeueZeilen
            // 
            this.capNeueZeilen.CausesValidation = false;
            this.capNeueZeilen.Location = new Point(192, 24);
            this.capNeueZeilen.Name = "capNeueZeilen";
            this.capNeueZeilen.Size = new Size(176, 24);
            this.capNeueZeilen.Text = "Neue Zeilen anlegen:";
            this.capNeueZeilen.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // grpKennwort
            // 
            this.grpKennwort.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpKennwort.CausesValidation = false;
            this.grpKennwort.Controls.Add(this.capKennwort);
            this.grpKennwort.Controls.Add(this.txbKennwort);
            this.grpKennwort.Location = new Point(400, 16);
            this.grpKennwort.Name = "grpKennwort";
            this.grpKennwort.Size = new Size(232, 96);
            this.grpKennwort.TabIndex = 1;
            this.grpKennwort.TabStop = false;
            this.grpKennwort.Text = "Kennwort:";
            // 
            // capKennwort
            // 
            this.capKennwort.CausesValidation = false;
            this.capKennwort.Location = new Point(8, 24);
            this.capKennwort.Name = "capKennwort";
            this.capKennwort.Size = new Size(216, 22);
            this.capKennwort.Text = "Zum Öffnen der Datenbank:";
            this.capKennwort.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbKennwort
            // 
            this.txbKennwort.Cursor = Cursors.IBeam;
            this.txbKennwort.Location = new Point(8, 56);
            this.txbKennwort.Name = "txbKennwort";
            this.txbKennwort.Size = new Size(216, 22);
            this.txbKennwort.TabIndex = 4;
            // 
            // lbxSortierSpalten
            // 
            this.lbxSortierSpalten.AddAllowed = AddType.OnlySuggests;
            this.lbxSortierSpalten.AutoSort = false;
            this.lbxSortierSpalten.CheckBehavior = CheckBehavior.AllSelected;
            this.lbxSortierSpalten.FilterText = null;
            this.lbxSortierSpalten.Location = new Point(16, 32);
            this.lbxSortierSpalten.MoveAllowed = true;
            this.lbxSortierSpalten.Name = "lbxSortierSpalten";
            this.lbxSortierSpalten.RemoveAllowed = true;
            this.lbxSortierSpalten.Size = new Size(256, 440);
            this.lbxSortierSpalten.TabIndex = 5;
            // 
            // capSortierspalten
            // 
            this.capSortierspalten.CausesValidation = false;
            this.capSortierspalten.Location = new Point(16, 8);
            this.capSortierspalten.Name = "capSortierspalten";
            this.capSortierspalten.Size = new Size(160, 24);
            this.capSortierspalten.Text = "Sortier-Spalten:";
            this.capSortierspalten.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnSortRichtung
            // 
            this.btnSortRichtung.ButtonStyle = ButtonStyle.Yes_or_No;
            this.btnSortRichtung.Location = new Point(288, 32);
            this.btnSortRichtung.Name = "btnSortRichtung";
            this.btnSortRichtung.Size = new Size(184, 40);
            this.btnSortRichtung.TabIndex = 0;
            this.btnSortRichtung.Text = "Umgekehrte Sortierung";
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnOk.ImageCode = "Häkchen|24";
            this.btnOk.Location = new Point(1144, 715);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new Size(88, 32);
            this.btnOk.TabIndex = 11;
            this.btnOk.Text = "OK";
            this.btnOk.Click += new EventHandler(this.OkBut_Click);
            // 
            // txbTags
            // 
            this.txbTags.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                    | AnchorStyles.Left) 
                                                   | AnchorStyles.Right)));
            this.txbTags.Cursor = Cursors.IBeam;
            this.txbTags.Location = new Point(624, 24);
            this.txbTags.MultiLine = true;
            this.txbTags.Name = "txbTags";
            this.txbTags.Size = new Size(601, 639);
            this.txbTags.TabIndex = 26;
            this.txbTags.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbCaption
            // 
            this.txbCaption.Cursor = Cursors.IBeam;
            this.txbCaption.Location = new Point(8, 24);
            this.txbCaption.Name = "txbCaption";
            this.txbCaption.Size = new Size(608, 24);
            this.txbCaption.TabIndex = 24;
            // 
            // capCaption
            // 
            this.capCaption.CausesValidation = false;
            this.capCaption.Location = new Point(8, 8);
            this.capCaption.Name = "capCaption";
            this.capCaption.Size = new Size(137, 16);
            this.capCaption.Text = "Überschrift bzw. Titel:";
            this.capCaption.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capTags
            // 
            this.capTags.CausesValidation = false;
            this.capTags.Location = new Point(624, 8);
            this.capTags.Name = "capTags";
            this.capTags.QuickInfo = "Tags / Eigenschaften, die von einem ";
            this.capTags.Size = new Size(152, 16);
            this.capTags.Text = "Tags:";
            this.capTags.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capInfo
            // 
            this.capInfo.CausesValidation = false;
            this.capInfo.Location = new Point(8, 56);
            this.capInfo.Name = "capInfo";
            this.capInfo.Size = new Size(608, 144);
            this.capInfo.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // GlobalTab
            // 
            this.GlobalTab.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                      | AnchorStyles.Left) 
                                                     | AnchorStyles.Right)));
            this.GlobalTab.Controls.Add(this.tabAllgemein);
            this.GlobalTab.Controls.Add(this.tabRechte);
            this.GlobalTab.Controls.Add(this.tabSortierung);
            this.GlobalTab.Controls.Add(this.tabVariablen);
            this.GlobalTab.Controls.Add(this.tabUndo);
            this.GlobalTab.HotTrack = true;
            this.GlobalTab.Location = new Point(0, 0);
            this.GlobalTab.Name = "GlobalTab";
            this.GlobalTab.SelectedIndex = 0;
            this.GlobalTab.Size = new Size(1241, 707);
            this.GlobalTab.TabDefault = null;
            this.GlobalTab.TabDefaultOrder = null;
            this.GlobalTab.TabIndex = 21;
            this.GlobalTab.SelectedIndexChanged += new EventHandler(this.GlobalTab_SelectedIndexChanged);
            // 
            // tabAllgemein
            // 
            this.tabAllgemein.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
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
            this.tabAllgemein.Controls.Add(this.txbTags);
            this.tabAllgemein.Controls.Add(this.btnSpaltenuebersicht);
            this.tabAllgemein.Controls.Add(this.capInfo);
            this.tabAllgemein.Controls.Add(this.capTags);
            this.tabAllgemein.Controls.Add(this.txbCaption);
            this.tabAllgemein.Controls.Add(this.capCaption);
            this.tabAllgemein.Location = new Point(4, 29);
            this.tabAllgemein.Name = "tabAllgemein";
            this.tabAllgemein.Padding = new Padding(3);
            this.tabAllgemein.Size = new Size(1233, 674);
            this.tabAllgemein.TabIndex = 1;
            this.tabAllgemein.Text = "Allgemein";
            // 
            // btnTabellenAnsicht
            // 
            this.btnTabellenAnsicht.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnTabellenAnsicht.ImageCode = "Tabelle|16";
            this.btnTabellenAnsicht.Location = new Point(216, 544);
            this.btnTabellenAnsicht.Name = "btnTabellenAnsicht";
            this.btnTabellenAnsicht.Size = new Size(200, 32);
            this.btnTabellenAnsicht.TabIndex = 53;
            this.btnTabellenAnsicht.Text = "Tabellenansicht";
            this.btnTabellenAnsicht.Click += new EventHandler(this.btnTabellenAnsicht_Click);
            // 
            // btnSkripte
            // 
            this.btnSkripte.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnSkripte.ImageCode = "Skript|16";
            this.btnSkripte.Location = new Point(216, 624);
            this.btnSkripte.Name = "btnSkripte";
            this.btnSkripte.Size = new Size(200, 32);
            this.btnSkripte.TabIndex = 52;
            this.btnSkripte.Text = "Skripte";
            this.btnSkripte.Click += new EventHandler(this.btnSkripte_Click);
            // 
            // btnSpaltenAnordnungen
            // 
            this.btnSpaltenAnordnungen.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnSpaltenAnordnungen.ImageCode = "Spalte|16";
            this.btnSpaltenAnordnungen.Location = new Point(216, 584);
            this.btnSpaltenAnordnungen.Name = "btnSpaltenAnordnungen";
            this.btnSpaltenAnordnungen.Size = new Size(200, 32);
            this.btnSpaltenAnordnungen.TabIndex = 51;
            this.btnSpaltenAnordnungen.Text = "Spaltenanordnung";
            this.btnSpaltenAnordnungen.Click += new EventHandler(this.btnSpaltenAnordnungen_Click);
            // 
            // txbZeilenQuickInfo
            // 
            this.txbZeilenQuickInfo.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                              | AnchorStyles.Left)));
            this.txbZeilenQuickInfo.Cursor = Cursors.IBeam;
            this.txbZeilenQuickInfo.Location = new Point(8, 216);
            this.txbZeilenQuickInfo.MultiLine = true;
            this.txbZeilenQuickInfo.Name = "txbZeilenQuickInfo";
            this.txbZeilenQuickInfo.Size = new Size(608, 186);
            this.txbZeilenQuickInfo.TabIndex = 43;
            this.txbZeilenQuickInfo.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // butSystemspaltenErstellen
            // 
            this.butSystemspaltenErstellen.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.butSystemspaltenErstellen.ImageCode = "Spalte|16";
            this.butSystemspaltenErstellen.Location = new Point(8, 624);
            this.butSystemspaltenErstellen.Name = "butSystemspaltenErstellen";
            this.butSystemspaltenErstellen.Size = new Size(200, 32);
            this.butSystemspaltenErstellen.TabIndex = 49;
            this.butSystemspaltenErstellen.Text = "Alle Systemspalten erstellen";
            this.butSystemspaltenErstellen.Click += new EventHandler(this.butSystemspaltenErstellen_Click);
            // 
            // btnOptimize
            // 
            this.btnOptimize.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnOptimize.ImageCode = "Blitz|16";
            this.btnOptimize.Location = new Point(8, 584);
            this.btnOptimize.Name = "btnOptimize";
            this.btnOptimize.QuickInfo = "Stellt alle Spalten um, \r\ndass die Daten";
            this.btnOptimize.Size = new Size(200, 32);
            this.btnOptimize.TabIndex = 48;
            this.btnOptimize.Text = "Datenbank optimieren";
            this.btnOptimize.Click += new EventHandler(this.btnOptimize_Click);
            // 
            // txbStandardFormulaFile
            // 
            this.txbStandardFormulaFile.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.txbStandardFormulaFile.Cursor = Cursors.IBeam;
            this.txbStandardFormulaFile.Location = new Point(8, 482);
            this.txbStandardFormulaFile.Name = "txbStandardFormulaFile";
            this.txbStandardFormulaFile.Size = new Size(608, 24);
            this.txbStandardFormulaFile.TabIndex = 47;
            this.txbStandardFormulaFile.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capStandardFormulaFile
            // 
            this.capStandardFormulaFile.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.capStandardFormulaFile.CausesValidation = false;
            this.capStandardFormulaFile.Location = new Point(8, 466);
            this.capStandardFormulaFile.Name = "capStandardFormulaFile";
            this.capStandardFormulaFile.Size = new Size(152, 18);
            this.capStandardFormulaFile.Text = "Standard-Formular-Datei:";
            // 
            // txbAdditionalFiles
            // 
            this.txbAdditionalFiles.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.txbAdditionalFiles.Cursor = Cursors.IBeam;
            this.txbAdditionalFiles.Location = new Point(8, 434);
            this.txbAdditionalFiles.Name = "txbAdditionalFiles";
            this.txbAdditionalFiles.Size = new Size(608, 24);
            this.txbAdditionalFiles.TabIndex = 45;
            this.txbAdditionalFiles.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capAdditional
            // 
            this.capAdditional.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.capAdditional.CausesValidation = false;
            this.capAdditional.Location = new Point(8, 418);
            this.capAdditional.Name = "capAdditional";
            this.capAdditional.Size = new Size(152, 18);
            this.capAdditional.Text = "Zugehörige-Dateien-Pfad:";
            // 
            // capZeilenQuickInfo
            // 
            this.capZeilenQuickInfo.CausesValidation = false;
            this.capZeilenQuickInfo.Location = new Point(8, 200);
            this.capZeilenQuickInfo.Name = "capZeilenQuickInfo";
            this.capZeilenQuickInfo.Size = new Size(152, 18);
            this.capZeilenQuickInfo.Text = "Zeilen-Quick-Info: ";
            // 
            // btnSpaltenuebersicht
            // 
            this.btnSpaltenuebersicht.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnSpaltenuebersicht.ImageCode = "Spalte|16";
            this.btnSpaltenuebersicht.Location = new Point(8, 544);
            this.btnSpaltenuebersicht.Name = "btnSpaltenuebersicht";
            this.btnSpaltenuebersicht.Size = new Size(200, 32);
            this.btnSpaltenuebersicht.TabIndex = 35;
            this.btnSpaltenuebersicht.Text = "Spaltenübersicht";
            this.btnSpaltenuebersicht.Click += new EventHandler(this.btnSpaltenuebersicht_Click);
            // 
            // tabRechte
            // 
            this.tabRechte.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabRechte.Controls.Add(this.grpKennwort);
            this.tabRechte.Controls.Add(this.grpBenutzergruppen);
            this.tabRechte.Location = new Point(4, 29);
            this.tabRechte.Name = "tabRechte";
            this.tabRechte.Padding = new Padding(3);
            this.tabRechte.Size = new Size(1233, 674);
            this.tabRechte.TabIndex = 4;
            this.tabRechte.Text = "Rechte";
            // 
            // tabSortierung
            // 
            this.tabSortierung.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabSortierung.Controls.Add(this.lbxSortierSpalten);
            this.tabSortierung.Controls.Add(this.capSortierspalten);
            this.tabSortierung.Controls.Add(this.btnSortRichtung);
            this.tabSortierung.Location = new Point(4, 29);
            this.tabSortierung.Name = "tabSortierung";
            this.tabSortierung.Padding = new Padding(3);
            this.tabSortierung.Size = new Size(1233, 674);
            this.tabSortierung.TabIndex = 2;
            this.tabSortierung.Text = "Sortierung";
            // 
            // tabVariablen
            // 
            this.tabVariablen.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabVariablen.Controls.Add(this.variableEditor);
            this.tabVariablen.Location = new Point(4, 29);
            this.tabVariablen.Name = "tabVariablen";
            this.tabVariablen.Padding = new Padding(3);
            this.tabVariablen.Size = new Size(1233, 674);
            this.tabVariablen.TabIndex = 7;
            this.tabVariablen.Text = "Variablen";
            // 
            // variableEditor
            // 
            this.variableEditor.Dock = DockStyle.Fill;
            this.variableEditor.Editabe = true;
            this.variableEditor.Location = new Point(3, 3);
            this.variableEditor.Name = "variableEditor";
            this.variableEditor.Size = new Size(1227, 668);
            this.variableEditor.TabIndex = 1;
            // 
            // tabUndo
            // 
            this.tabUndo.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabUndo.Controls.Add(this.tblUndo);
            this.tabUndo.Controls.Add(this.grpUndoActions);
            this.tabUndo.Location = new Point(4, 29);
            this.tabUndo.Name = "tabUndo";
            this.tabUndo.Size = new Size(1233, 674);
            this.tabUndo.TabIndex = 6;
            this.tabUndo.Text = "Undo";
            // 
            // tblUndo
            // 
            this.tblUndo.Dock = DockStyle.Fill;
            this.tblUndo.Location = new Point(0, 0);
            this.tblUndo.Name = "tblUndo";
            this.tblUndo.Size = new Size(1233, 604);
            this.tblUndo.TabIndex = 0;
            this.tblUndo.Text = "UndoTab";
            this.tblUndo.ContextMenuInit += new EventHandler<ContextMenuInitEventArgs>(this.tblUndo_ContextMenuInit);
            this.tblUndo.ContextMenuItemClicked += new EventHandler<ContextMenuItemClickedEventArgs>(this.tblUndo_ContextMenuItemClicked);
            // 
            // grpUndoActions
            // 
            this.grpUndoActions.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpUndoActions.Controls.Add(this.btnClipboard);
            this.grpUndoActions.Dock = DockStyle.Bottom;
            this.grpUndoActions.Location = new Point(0, 604);
            this.grpUndoActions.Name = "grpUndoActions";
            this.grpUndoActions.Size = new Size(1233, 70);
            this.grpUndoActions.TabIndex = 4;
            this.grpUndoActions.TabStop = false;
            this.grpUndoActions.Text = "Aktionen";
            // 
            // btnClipboard
            // 
            this.btnClipboard.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnClipboard.ImageCode = "Clipboard|24";
            this.btnClipboard.Location = new Point(16, 24);
            this.btnClipboard.Name = "btnClipboard";
            this.btnClipboard.Size = new Size(176, 40);
            this.btnClipboard.TabIndex = 3;
            this.btnClipboard.Text = "In die Zwischenablage";
            this.btnClipboard.Click += new EventHandler(this.btnClipboard_Click);
            // 
            // DatabaseHeadEditor
            // 
            this.ClientSize = new Size(1244, 756);
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
