using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Classes_Editor;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using Button = BlueControls.Controls.Button;
using Form = BlueControls.Forms.Form;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;
using TabControl = BlueControls.Controls.TabControl;
using TabPage = System.Windows.Forms.TabPage;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueDatabaseDialogs
{
    public sealed partial class DatabaseHeadEditor : Form
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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(DatabaseHeadEditor));
            this.grpBenutzergruppen = new GroupBox();
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
            this.txbStandardFormulaFile = new TextBox();
            this.capStandardFormulaFile = new Caption();
            this.txbAdditionalFiles = new TextBox();
            this.capAdditional = new Caption();
            this.txbZeilenQuickInfo = new TextBox();
            this.capZeilenQuickInfo = new Caption();
            this.txbGlobalScale = new TextBox();
            this.caption1 = new Caption();
            this.btnSpaltenuebersicht = new Button();
            this.tabVariablen = new TabPage();
            this.variableEditor = new VariableEditor();
            this.tabScripts = new TabPage();
            this.eventScriptEditor = new EventScript_Editor();
            this.grpVerfügbareSkripte = new GroupBox();
            this.lstEventScripts = new ListBox();
            this.tabRechte = new TabPage();
            this.tabSortierung = new TabPage();
            this.tabBackup = new TabPage();
            this.ExportEditor = new ExportDefinition_Editor();
            this.grpExport = new GroupBox();
            this.lbxExportSets = new ListBox();
            this.tabUndo = new TabPage();
            this.btnClipboard = new Button();
            this.tbxUndoAnzahl = new TextBox();
            this.capUndoAnzahl = new Caption();
            this.tblUndo = new Table();
            this.capBinInfo = new Caption();
            this.btnSave = new Button();
            this.grpBenutzergruppen.SuspendLayout();
            this.grpKennwort.SuspendLayout();
            this.GlobalTab.SuspendLayout();
            this.tabAllgemein.SuspendLayout();
            this.tabVariablen.SuspendLayout();
            this.tabScripts.SuspendLayout();
            this.grpVerfügbareSkripte.SuspendLayout();
            this.tabRechte.SuspendLayout();
            this.tabSortierung.SuspendLayout();
            this.tabBackup.SuspendLayout();
            this.grpExport.SuspendLayout();
            this.tabUndo.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpBenutzergruppen
            // 
            this.grpBenutzergruppen.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpBenutzergruppen.CausesValidation = false;
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
            // PermissionGroups_NewRow
            // 
            this.PermissionGroups_NewRow.AddAllowed = AddType.Text;
            this.PermissionGroups_NewRow.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                                   | AnchorStyles.Left)));
            this.PermissionGroups_NewRow.CheckBehavior = CheckBehavior.MultiSelection;
            this.PermissionGroups_NewRow.FilterAllowed = true;
            this.PermissionGroups_NewRow.Location = new Point(192, 48);
            this.PermissionGroups_NewRow.Name = "PermissionGroups_NewRow";
            this.PermissionGroups_NewRow.RemoveAllowed = true;
            this.PermissionGroups_NewRow.Size = new Size(176, 352);
            this.PermissionGroups_NewRow.TabIndex = 4;
            // 
            // capNeueZeilenInfo
            // 
            this.capNeueZeilenInfo.CausesValidation = false;
            this.capNeueZeilenInfo.Location = new Point(192, 408);
            this.capNeueZeilenInfo.Name = "capNeueZeilenInfo";
            this.capNeueZeilenInfo.Size = new Size(176, 72);
            this.capNeueZeilenInfo.Text = "<i>Administratoren dürfen immer neue Zeilen anlegen, wenn die erste Spalte eine B" +
    "earbeitung zulässt";
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
            this.DatenbankAdmin.CheckBehavior = CheckBehavior.MultiSelection;
            this.DatenbankAdmin.FilterAllowed = true;
            this.DatenbankAdmin.Location = new Point(8, 46);
            this.DatenbankAdmin.Name = "DatenbankAdmin";
            this.DatenbankAdmin.RemoveAllowed = true;
            this.DatenbankAdmin.Size = new Size(176, 434);
            this.DatenbankAdmin.TabIndex = 4;
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
            this.lbxSortierSpalten.CheckBehavior = CheckBehavior.MultiSelection;
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
            this.btnOk.Location = new Point(973, 651);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new Size(72, 24);
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
            this.txbTags.Location = new Point(568, 24);
            this.txbTags.MultiLine = true;
            this.txbTags.Name = "txbTags";
            this.txbTags.Size = new Size(463, 581);
            this.txbTags.TabIndex = 26;
            this.txbTags.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbCaption
            // 
            this.txbCaption.Cursor = Cursors.IBeam;
            this.txbCaption.Location = new Point(8, 24);
            this.txbCaption.Name = "txbCaption";
            this.txbCaption.Size = new Size(552, 24);
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
            this.capTags.Location = new Point(568, 8);
            this.capTags.Name = "capTags";
            this.capTags.QuickInfo = "Tags / Eigenschaften, die von einem ";
            this.capTags.Size = new Size(152, 16);
            this.capTags.Text = "Tags:";
            this.capTags.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capInfo
            // 
            this.capInfo.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                   | AnchorStyles.Left)));
            this.capInfo.CausesValidation = false;
            this.capInfo.Location = new Point(8, 56);
            this.capInfo.Name = "capInfo";
            this.capInfo.Size = new Size(552, 90);
            this.capInfo.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // GlobalTab
            // 
            this.GlobalTab.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                      | AnchorStyles.Left) 
                                                     | AnchorStyles.Right)));
            this.GlobalTab.Controls.Add(this.tabAllgemein);
            this.GlobalTab.Controls.Add(this.tabVariablen);
            this.GlobalTab.Controls.Add(this.tabScripts);
            this.GlobalTab.Controls.Add(this.tabRechte);
            this.GlobalTab.Controls.Add(this.tabSortierung);
            this.GlobalTab.Controls.Add(this.tabBackup);
            this.GlobalTab.Controls.Add(this.tabUndo);
            this.GlobalTab.HotTrack = true;
            this.GlobalTab.Location = new Point(0, 0);
            this.GlobalTab.Name = "GlobalTab";
            this.GlobalTab.RowKey = ((long)(-1));
            this.GlobalTab.SelectedIndex = 0;
            this.GlobalTab.Size = new Size(1047, 645);
            this.GlobalTab.TabDefault = null;
            this.GlobalTab.TabDefaultOrder = null;
            this.GlobalTab.TabIndex = 21;
            this.GlobalTab.Selecting += new TabControlCancelEventHandler(this.GlobalTab_Selecting);
            // 
            // tabAllgemein
            // 
            this.tabAllgemein.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabAllgemein.Controls.Add(this.txbStandardFormulaFile);
            this.tabAllgemein.Controls.Add(this.capStandardFormulaFile);
            this.tabAllgemein.Controls.Add(this.txbAdditionalFiles);
            this.tabAllgemein.Controls.Add(this.capAdditional);
            this.tabAllgemein.Controls.Add(this.txbZeilenQuickInfo);
            this.tabAllgemein.Controls.Add(this.capZeilenQuickInfo);
            this.tabAllgemein.Controls.Add(this.txbGlobalScale);
            this.tabAllgemein.Controls.Add(this.caption1);
            this.tabAllgemein.Controls.Add(this.txbTags);
            this.tabAllgemein.Controls.Add(this.btnSpaltenuebersicht);
            this.tabAllgemein.Controls.Add(this.capInfo);
            this.tabAllgemein.Controls.Add(this.capTags);
            this.tabAllgemein.Controls.Add(this.txbCaption);
            this.tabAllgemein.Controls.Add(this.capCaption);
            this.tabAllgemein.Location = new Point(4, 25);
            this.tabAllgemein.Name = "tabAllgemein";
            this.tabAllgemein.Padding = new Padding(3);
            this.tabAllgemein.Size = new Size(1039, 616);
            this.tabAllgemein.TabIndex = 1;
            this.tabAllgemein.Text = "Allgemein";
            // 
            // txbStandardFormulaFile
            // 
            this.txbStandardFormulaFile.Cursor = Cursors.IBeam;
            this.txbStandardFormulaFile.Location = new Point(8, 424);
            this.txbStandardFormulaFile.Name = "txbStandardFormulaFile";
            this.txbStandardFormulaFile.Size = new Size(440, 24);
            this.txbStandardFormulaFile.TabIndex = 47;
            this.txbStandardFormulaFile.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capStandardFormulaFile
            // 
            this.capStandardFormulaFile.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.capStandardFormulaFile.CausesValidation = false;
            this.capStandardFormulaFile.Location = new Point(8, 408);
            this.capStandardFormulaFile.Name = "capStandardFormulaFile";
            this.capStandardFormulaFile.Size = new Size(152, 18);
            this.capStandardFormulaFile.Text = "Standard-Formular-Datei:";
            // 
            // txbAdditionalFiles
            // 
            this.txbAdditionalFiles.Cursor = Cursors.IBeam;
            this.txbAdditionalFiles.Location = new Point(8, 376);
            this.txbAdditionalFiles.Name = "txbAdditionalFiles";
            this.txbAdditionalFiles.Size = new Size(552, 24);
            this.txbAdditionalFiles.TabIndex = 45;
            this.txbAdditionalFiles.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capAdditional
            // 
            this.capAdditional.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.capAdditional.CausesValidation = false;
            this.capAdditional.Location = new Point(8, 360);
            this.capAdditional.Name = "capAdditional";
            this.capAdditional.Size = new Size(152, 18);
            this.capAdditional.Text = "Zugehörige-Dateien-Pfad:";
            // 
            // txbZeilenQuickInfo
            // 
            this.txbZeilenQuickInfo.Cursor = Cursors.IBeam;
            this.txbZeilenQuickInfo.Location = new Point(8, 176);
            this.txbZeilenQuickInfo.MultiLine = true;
            this.txbZeilenQuickInfo.Name = "txbZeilenQuickInfo";
            this.txbZeilenQuickInfo.Size = new Size(552, 168);
            this.txbZeilenQuickInfo.TabIndex = 43;
            this.txbZeilenQuickInfo.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capZeilenQuickInfo
            // 
            this.capZeilenQuickInfo.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.capZeilenQuickInfo.CausesValidation = false;
            this.capZeilenQuickInfo.Location = new Point(8, 160);
            this.capZeilenQuickInfo.Name = "capZeilenQuickInfo";
            this.capZeilenQuickInfo.Size = new Size(152, 18);
            this.capZeilenQuickInfo.Text = "Zeilen-Quick-Info: ";
            // 
            // txbGlobalScale
            // 
            this.txbGlobalScale.Cursor = Cursors.IBeam;
            this.txbGlobalScale.Location = new Point(168, 512);
            this.txbGlobalScale.Name = "txbGlobalScale";
            this.txbGlobalScale.Size = new Size(160, 24);
            this.txbGlobalScale.TabIndex = 39;
            // 
            // caption1
            // 
            this.caption1.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.caption1.CausesValidation = false;
            this.caption1.Location = new Point(8, 512);
            this.caption1.Name = "caption1";
            this.caption1.Size = new Size(144, 18);
            this.caption1.Text = "Bevorzugtes Skalierung:";
            // 
            // btnSpaltenuebersicht
            // 
            this.btnSpaltenuebersicht.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnSpaltenuebersicht.ImageCode = "Spalte|16";
            this.btnSpaltenuebersicht.Location = new Point(360, 538);
            this.btnSpaltenuebersicht.Name = "btnSpaltenuebersicht";
            this.btnSpaltenuebersicht.Size = new Size(200, 32);
            this.btnSpaltenuebersicht.TabIndex = 35;
            this.btnSpaltenuebersicht.Text = "Spaltenübersicht";
            this.btnSpaltenuebersicht.Click += new EventHandler(this.btnSpaltenuebersicht_Click);
            // 
            // tabVariablen
            // 
            this.tabVariablen.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabVariablen.Controls.Add(this.variableEditor);
            this.tabVariablen.Location = new Point(4, 25);
            this.tabVariablen.Name = "tabVariablen";
            this.tabVariablen.Padding = new Padding(3);
            this.tabVariablen.Size = new Size(1039, 616);
            this.tabVariablen.TabIndex = 3;
            this.tabVariablen.Text = "Variablen";
            // 
            // variableEditor
            // 
            this.variableEditor.Dock = DockStyle.Fill;
            this.variableEditor.Editabe = true;
            this.variableEditor.Location = new Point(3, 3);
            this.variableEditor.Name = "variableEditor";
            this.variableEditor.Size = new Size(1033, 610);
            this.variableEditor.TabIndex = 0;
            // 
            // tabScripts
            // 
            this.tabScripts.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabScripts.Controls.Add(this.eventScriptEditor);
            this.tabScripts.Controls.Add(this.grpVerfügbareSkripte);
            this.tabScripts.Location = new Point(4, 25);
            this.tabScripts.Name = "tabScripts";
            this.tabScripts.Padding = new Padding(3);
            this.tabScripts.Size = new Size(1039, 616);
            this.tabScripts.TabIndex = 7;
            this.tabScripts.Text = "Skripte";
            // 
            // eventScriptEditor
            // 
            this.eventScriptEditor.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.eventScriptEditor.Database = null;
            this.eventScriptEditor.Dock = DockStyle.Fill;
            this.eventScriptEditor.Location = new Point(240, 3);
            this.eventScriptEditor.Name = "eventScriptEditor";
            this.eventScriptEditor.Size = new Size(796, 610);
            this.eventScriptEditor.TabIndex = 3;
            this.eventScriptEditor.TabStop = false;
            this.eventScriptEditor.Text = "Skript-Editor";
            // 
            // grpVerfügbareSkripte
            // 
            this.grpVerfügbareSkripte.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpVerfügbareSkripte.CausesValidation = false;
            this.grpVerfügbareSkripte.Controls.Add(this.lstEventScripts);
            this.grpVerfügbareSkripte.Dock = DockStyle.Left;
            this.grpVerfügbareSkripte.Location = new Point(3, 3);
            this.grpVerfügbareSkripte.Name = "grpVerfügbareSkripte";
            this.grpVerfügbareSkripte.Size = new Size(237, 610);
            this.grpVerfügbareSkripte.TabIndex = 2;
            this.grpVerfügbareSkripte.TabStop = false;
            this.grpVerfügbareSkripte.Text = "Verfügbare Skripte:";
            // 
            // lstEventScripts
            // 
            this.lstEventScripts.AddAllowed = AddType.UserDef;
            this.lstEventScripts.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                            | AnchorStyles.Left) 
                                                           | AnchorStyles.Right)));
            this.lstEventScripts.FilterAllowed = true;
            this.lstEventScripts.Location = new Point(8, 24);
            this.lstEventScripts.Name = "lstEventScripts";
            this.lstEventScripts.RemoveAllowed = true;
            this.lstEventScripts.Size = new Size(222, 578);
            this.lstEventScripts.TabIndex = 0;
            this.lstEventScripts.AddClicked += new EventHandler(this.lstEventScripts_AddClicked);
            this.lstEventScripts.ItemCheckedChanged += new EventHandler(this.lstEventScripts_ItemCheckedChanged);
            // 
            // tabRechte
            // 
            this.tabRechte.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabRechte.Controls.Add(this.grpKennwort);
            this.tabRechte.Controls.Add(this.grpBenutzergruppen);
            this.tabRechte.Location = new Point(4, 25);
            this.tabRechte.Name = "tabRechte";
            this.tabRechte.Padding = new Padding(3);
            this.tabRechte.Size = new Size(1039, 616);
            this.tabRechte.TabIndex = 4;
            this.tabRechte.Text = "Rechte";
            // 
            // tabSortierung
            // 
            this.tabSortierung.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabSortierung.Controls.Add(this.lbxSortierSpalten);
            this.tabSortierung.Controls.Add(this.capSortierspalten);
            this.tabSortierung.Controls.Add(this.btnSortRichtung);
            this.tabSortierung.Location = new Point(4, 25);
            this.tabSortierung.Name = "tabSortierung";
            this.tabSortierung.Padding = new Padding(3);
            this.tabSortierung.Size = new Size(1039, 616);
            this.tabSortierung.TabIndex = 2;
            this.tabSortierung.Text = "Sortierung";
            // 
            // tabBackup
            // 
            this.tabBackup.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabBackup.Controls.Add(this.ExportEditor);
            this.tabBackup.Controls.Add(this.grpExport);
            this.tabBackup.Location = new Point(4, 25);
            this.tabBackup.Name = "tabBackup";
            this.tabBackup.Padding = new Padding(3);
            this.tabBackup.Size = new Size(1039, 616);
            this.tabBackup.TabIndex = 5;
            this.tabBackup.Text = "Backup & Export";
            // 
            // ExportEditor
            // 
            this.ExportEditor.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                         | AnchorStyles.Left) 
                                                        | AnchorStyles.Right)));
            this.ExportEditor.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ExportEditor.CausesValidation = false;
            this.ExportEditor.Enabled = false;
            this.ExportEditor.Location = new Point(8, 176);
            this.ExportEditor.Name = "ExportEditor";
            this.ExportEditor.Size = new Size(1031, 437);
            this.ExportEditor.TabIndex = 0;
            this.ExportEditor.TabStop = false;
            this.ExportEditor.Text = "Export-Editor:";
            this.ExportEditor.Changed += new EventHandler(this.ExportEditor_Changed);
            // 
            // grpExport
            // 
            this.grpExport.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                     | AnchorStyles.Right)));
            this.grpExport.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpExport.CausesValidation = false;
            this.grpExport.Controls.Add(this.lbxExportSets);
            this.grpExport.Location = new Point(8, 8);
            this.grpExport.Name = "grpExport";
            this.grpExport.Size = new Size(1031, 168);
            this.grpExport.TabIndex = 1;
            this.grpExport.TabStop = false;
            this.grpExport.Text = "Alle Export-Aufgaben:";
            // 
            // lbxExportSets
            // 
            this.lbxExportSets.AddAllowed = AddType.UserDef;
            this.lbxExportSets.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                          | AnchorStyles.Left) 
                                                         | AnchorStyles.Right)));
            this.lbxExportSets.FilterAllowed = true;
            this.lbxExportSets.Location = new Point(8, 24);
            this.lbxExportSets.Name = "lbxExportSets";
            this.lbxExportSets.RemoveAllowed = true;
            this.lbxExportSets.Size = new Size(1016, 136);
            this.lbxExportSets.TabIndex = 0;
            this.lbxExportSets.Text = "ExportAufgaben";
            this.lbxExportSets.AddClicked += new EventHandler(this.lbxExportSets_AddClicked);
            this.lbxExportSets.ItemCheckedChanged += new EventHandler(this.lbxExportSets_ItemCheckedChanged);
            // 
            // tabUndo
            // 
            this.tabUndo.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabUndo.Controls.Add(this.btnClipboard);
            this.tabUndo.Controls.Add(this.tbxUndoAnzahl);
            this.tabUndo.Controls.Add(this.capUndoAnzahl);
            this.tabUndo.Controls.Add(this.tblUndo);
            this.tabUndo.Location = new Point(4, 25);
            this.tabUndo.Name = "tabUndo";
            this.tabUndo.Size = new Size(1039, 616);
            this.tabUndo.TabIndex = 6;
            this.tabUndo.Text = "Undo";
            // 
            // btnClipboard
            // 
            this.btnClipboard.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnClipboard.Location = new Point(864, 8);
            this.btnClipboard.Name = "btnClipboard";
            this.btnClipboard.Size = new Size(168, 32);
            this.btnClipboard.TabIndex = 3;
            this.btnClipboard.Text = "Clipboard";
            this.btnClipboard.Click += new EventHandler(this.btnClipboard_Click);
            // 
            // tbxUndoAnzahl
            // 
            this.tbxUndoAnzahl.Cursor = Cursors.IBeam;
            this.tbxUndoAnzahl.Location = new Point(176, 8);
            this.tbxUndoAnzahl.Name = "tbxUndoAnzahl";
            this.tbxUndoAnzahl.Size = new Size(88, 24);
            this.tbxUndoAnzahl.TabIndex = 1;
            // 
            // capUndoAnzahl
            // 
            this.capUndoAnzahl.CausesValidation = false;
            this.capUndoAnzahl.Location = new Point(0, 8);
            this.capUndoAnzahl.Name = "capUndoAnzahl";
            this.capUndoAnzahl.Size = new Size(168, 40);
            this.capUndoAnzahl.Text = "Anzahl gespeicherter Undos:<br>(Standard: 300)";
            // 
            // tblUndo
            // 
            this.tblUndo.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                    | AnchorStyles.Left) 
                                                   | AnchorStyles.Right)));
            this.tblUndo.DropMessages = false;
            this.tblUndo.Location = new Point(0, 56);
            this.tblUndo.Name = "tblUndo";
            this.tblUndo.ShowWaitScreen = true;
            this.tblUndo.Size = new Size(1039, 562);
            this.tblUndo.TabIndex = 0;
            this.tblUndo.Text = "UndoTab";
            this.tblUndo.ContextMenuInit += new EventHandler<ContextMenuInitEventArgs>(this.tblUndo_ContextMenuInit);
            this.tblUndo.ContextMenuItemClicked += new EventHandler<ContextMenuItemClickedEventArgs>(this.tblUndo_ContextMenuItemClicked);
            // 
            // capBinInfo
            // 
            this.capBinInfo.CausesValidation = false;
            this.capBinInfo.Dock = DockStyle.Top;
            this.capBinInfo.Location = new Point(0, 0);
            this.capBinInfo.Name = "capBinInfo";
            this.capBinInfo.Size = new Size(1039, 24);
            this.capBinInfo.Text = "Die hier aufgeführten Binärdaten können - falls es ein Bild ist - mit DB_Dateinam" +
    "e mit Suffix angesprochen werden.";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnSave.ImageCode = "Diskette|16";
            this.btnSave.Location = new Point(840, 651);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new Size(112, 24);
            this.btnSave.TabIndex = 22;
            this.btnSave.Text = "Speichern";
            this.btnSave.Click += new EventHandler(this.btnSave_Click);
            // 
            // DatabaseHeadEditor
            // 
            this.ClientSize = new Size(1050, 677);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.GlobalTab);
            this.Controls.Add(this.btnOk);
            this.Icon = ((Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "DatabaseHeadEditor";
            this.ShowInTaskbar = false;
            this.Text = "Datenbank-Eigenschaften";
            this.grpBenutzergruppen.ResumeLayout(false);
            this.grpKennwort.ResumeLayout(false);
            this.GlobalTab.ResumeLayout(false);
            this.tabAllgemein.ResumeLayout(false);
            this.tabVariablen.ResumeLayout(false);
            this.tabScripts.ResumeLayout(false);
            this.grpVerfügbareSkripte.ResumeLayout(false);
            this.tabRechte.ResumeLayout(false);
            this.tabSortierung.ResumeLayout(false);
            this.tabBackup.ResumeLayout(false);
            this.grpExport.ResumeLayout(false);
            this.tabUndo.ResumeLayout(false);
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
        private  TabPage tabAllgemein;
        private  TabPage tabSortierung;
        private  TabPage tabVariablen;
        private  TabPage tabRechte;
        private  TabPage tabBackup;
        private GroupBox grpKennwort;
        private GroupBox grpBenutzergruppen;
        private  TabPage tabUndo;
        private ListBox lbxExportSets;
        private Caption capKennwort;
        private GroupBox grpExport;
        private ExportDefinition_Editor ExportEditor;
        private Button btnSpaltenuebersicht;
        private Table tblUndo;
        private Caption capNeueZeilenInfo;
        private Caption capUndoAnzahl;
        private TextBox tbxUndoAnzahl;
        private TextBox txbGlobalScale;
        private Caption caption1;
        private TabControl GlobalTab;
        private TextBox txbZeilenQuickInfo;
        private Caption capZeilenQuickInfo;
        private Caption capBinInfo;
        private TextBox txbAdditionalFiles;
        private Caption capAdditional;
        private Button btnSave;
        private Button btnClipboard;
        private TextBox txbStandardFormulaFile;
        private Caption capStandardFormulaFile;
        private TabPage tabScripts;
        private GroupBox grpVerfügbareSkripte;
        private ListBox lstEventScripts;
        private EventScript_Editor eventScriptEditor;
        private VariableEditor variableEditor;
    }
}
