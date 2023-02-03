using BlueControls.Classes_Editor;
using BlueControls.Controls;
using BlueControls.Forms;
using System.Diagnostics;
using BlueControls.Enums;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseHeadEditor));
            this.grpBenutzergruppen = new BlueControls.Controls.GroupBox();
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
            this.txbStandardFormulaFile = new BlueControls.Controls.TextBox();
            this.capStandardFormulaFile = new BlueControls.Controls.Caption();
            this.txbAdditionalFiles = new BlueControls.Controls.TextBox();
            this.capAdditional = new BlueControls.Controls.Caption();
            this.txbZeilenQuickInfo = new BlueControls.Controls.TextBox();
            this.capZeilenQuickInfo = new BlueControls.Controls.Caption();
            this.txbGlobalScale = new BlueControls.Controls.TextBox();
            this.caption1 = new BlueControls.Controls.Caption();
            this.btnSpaltenuebersicht = new BlueControls.Controls.Button();
            this.tabEvents = new System.Windows.Forms.TabPage();
            this.tabScripts = new System.Windows.Forms.TabPage();
            this.eventScriptEditor = new BlueControls.Classes_Editor.EventScript_Editor();
            this.grpVerfügbareSkripte = new BlueControls.Controls.GroupBox();
            this.lstEventScripts = new BlueControls.Controls.ListBox();
            this.tabRechte = new System.Windows.Forms.TabPage();
            this.tabSortierung = new System.Windows.Forms.TabPage();
            this.tabBackup = new System.Windows.Forms.TabPage();
            this.ExportEditor = new BlueControls.Classes_Editor.ExportDefinition_Editor();
            this.grpExport = new BlueControls.Controls.GroupBox();
            this.lbxExportSets = new BlueControls.Controls.ListBox();
            this.tabUndo = new System.Windows.Forms.TabPage();
            this.btnClipboard = new BlueControls.Controls.Button();
            this.tbxUndoAnzahl = new BlueControls.Controls.TextBox();
            this.capUndoAnzahl = new BlueControls.Controls.Caption();
            this.tblUndo = new BlueControls.Controls.Table();
            this.capBinInfo = new BlueControls.Controls.Caption();
            this.btnSave = new BlueControls.Controls.Button();
            this.table1 = new BlueControls.Controls.Table();
            this.grpBenutzergruppen.SuspendLayout();
            this.grpKennwort.SuspendLayout();
            this.GlobalTab.SuspendLayout();
            this.tabAllgemein.SuspendLayout();
            this.tabEvents.SuspendLayout();
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
            this.grpBenutzergruppen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpBenutzergruppen.CausesValidation = false;
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
            // PermissionGroups_NewRow
            // 
            this.PermissionGroups_NewRow.AddAllowed = BlueControls.Enums.AddType.Text;
            this.PermissionGroups_NewRow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.PermissionGroups_NewRow.CheckBehavior = BlueControls.Enums.CheckBehavior.MultiSelection;
            this.PermissionGroups_NewRow.FilterAllowed = true;
            this.PermissionGroups_NewRow.Location = new System.Drawing.Point(192, 48);
            this.PermissionGroups_NewRow.Name = "PermissionGroups_NewRow";
            this.PermissionGroups_NewRow.RemoveAllowed = true;
            this.PermissionGroups_NewRow.Size = new System.Drawing.Size(176, 352);
            this.PermissionGroups_NewRow.TabIndex = 4;
            // 
            // capNeueZeilenInfo
            // 
            this.capNeueZeilenInfo.CausesValidation = false;
            this.capNeueZeilenInfo.Location = new System.Drawing.Point(192, 408);
            this.capNeueZeilenInfo.Name = "capNeueZeilenInfo";
            this.capNeueZeilenInfo.Size = new System.Drawing.Size(176, 72);
            this.capNeueZeilenInfo.Text = "<i>Administratoren dürfen immer neue Zeilen anlegen, wenn die erste Spalte eine B" +
    "earbeitung zulässt";
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
            this.DatenbankAdmin.CheckBehavior = BlueControls.Enums.CheckBehavior.MultiSelection;
            this.DatenbankAdmin.FilterAllowed = true;
            this.DatenbankAdmin.Location = new System.Drawing.Point(8, 46);
            this.DatenbankAdmin.Name = "DatenbankAdmin";
            this.DatenbankAdmin.RemoveAllowed = true;
            this.DatenbankAdmin.Size = new System.Drawing.Size(176, 434);
            this.DatenbankAdmin.TabIndex = 4;
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
            this.lbxSortierSpalten.CheckBehavior = BlueControls.Enums.CheckBehavior.MultiSelection;
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
            this.btnOk.Location = new System.Drawing.Point(973, 651);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(72, 24);
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
            this.txbTags.Location = new System.Drawing.Point(568, 24);
            this.txbTags.MultiLine = true;
            this.txbTags.Name = "txbTags";
            this.txbTags.Size = new System.Drawing.Size(463, 581);
            this.txbTags.TabIndex = 26;
            this.txbTags.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbCaption
            // 
            this.txbCaption.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbCaption.Location = new System.Drawing.Point(8, 24);
            this.txbCaption.Name = "txbCaption";
            this.txbCaption.Size = new System.Drawing.Size(552, 24);
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
            this.capTags.Location = new System.Drawing.Point(568, 8);
            this.capTags.Name = "capTags";
            this.capTags.QuickInfo = "Tags / Eigenschaften, die von einem ";
            this.capTags.Size = new System.Drawing.Size(152, 16);
            this.capTags.Text = "Tags:";
            this.capTags.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capInfo
            // 
            this.capInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.capInfo.CausesValidation = false;
            this.capInfo.Location = new System.Drawing.Point(8, 56);
            this.capInfo.Name = "capInfo";
            this.capInfo.Size = new System.Drawing.Size(552, 90);
            this.capInfo.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // GlobalTab
            // 
            this.GlobalTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GlobalTab.Controls.Add(this.tabAllgemein);
            this.GlobalTab.Controls.Add(this.tabEvents);
            this.GlobalTab.Controls.Add(this.tabScripts);
            this.GlobalTab.Controls.Add(this.tabRechte);
            this.GlobalTab.Controls.Add(this.tabSortierung);
            this.GlobalTab.Controls.Add(this.tabBackup);
            this.GlobalTab.Controls.Add(this.tabUndo);
            this.GlobalTab.HotTrack = true;
            this.GlobalTab.Location = new System.Drawing.Point(0, 0);
            this.GlobalTab.Name = "GlobalTab";
            this.GlobalTab.RowKey = ((long)(-1));
            this.GlobalTab.SelectedIndex = 0;
            this.GlobalTab.Size = new System.Drawing.Size(1047, 645);
            this.GlobalTab.TabDefault = null;
            this.GlobalTab.TabDefaultOrder = null;
            this.GlobalTab.TabIndex = 21;
            this.GlobalTab.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.GlobalTab_Selecting);
            // 
            // tabAllgemein
            // 
            this.tabAllgemein.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
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
            this.tabAllgemein.Location = new System.Drawing.Point(4, 25);
            this.tabAllgemein.Name = "tabAllgemein";
            this.tabAllgemein.Padding = new System.Windows.Forms.Padding(3);
            this.tabAllgemein.Size = new System.Drawing.Size(1039, 616);
            this.tabAllgemein.TabIndex = 1;
            this.tabAllgemein.Text = "Allgemein";
            // 
            // txbStandardFormulaFile
            // 
            this.txbStandardFormulaFile.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbStandardFormulaFile.Location = new System.Drawing.Point(8, 424);
            this.txbStandardFormulaFile.Name = "txbStandardFormulaFile";
            this.txbStandardFormulaFile.Size = new System.Drawing.Size(440, 24);
            this.txbStandardFormulaFile.TabIndex = 47;
            this.txbStandardFormulaFile.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capStandardFormulaFile
            // 
            this.capStandardFormulaFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.capStandardFormulaFile.CausesValidation = false;
            this.capStandardFormulaFile.Location = new System.Drawing.Point(8, 408);
            this.capStandardFormulaFile.Name = "capStandardFormulaFile";
            this.capStandardFormulaFile.Size = new System.Drawing.Size(152, 18);
            this.capStandardFormulaFile.Text = "Standard-Formular-Datei:";
            // 
            // txbAdditionalFiles
            // 
            this.txbAdditionalFiles.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbAdditionalFiles.Location = new System.Drawing.Point(8, 376);
            this.txbAdditionalFiles.Name = "txbAdditionalFiles";
            this.txbAdditionalFiles.Size = new System.Drawing.Size(552, 24);
            this.txbAdditionalFiles.TabIndex = 45;
            this.txbAdditionalFiles.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capAdditional
            // 
            this.capAdditional.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.capAdditional.CausesValidation = false;
            this.capAdditional.Location = new System.Drawing.Point(8, 360);
            this.capAdditional.Name = "capAdditional";
            this.capAdditional.Size = new System.Drawing.Size(152, 18);
            this.capAdditional.Text = "Zugehörige-Dateien-Pfad:";
            // 
            // txbZeilenQuickInfo
            // 
            this.txbZeilenQuickInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbZeilenQuickInfo.Location = new System.Drawing.Point(8, 176);
            this.txbZeilenQuickInfo.MultiLine = true;
            this.txbZeilenQuickInfo.Name = "txbZeilenQuickInfo";
            this.txbZeilenQuickInfo.Size = new System.Drawing.Size(552, 168);
            this.txbZeilenQuickInfo.TabIndex = 43;
            this.txbZeilenQuickInfo.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capZeilenQuickInfo
            // 
            this.capZeilenQuickInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.capZeilenQuickInfo.CausesValidation = false;
            this.capZeilenQuickInfo.Location = new System.Drawing.Point(8, 160);
            this.capZeilenQuickInfo.Name = "capZeilenQuickInfo";
            this.capZeilenQuickInfo.Size = new System.Drawing.Size(152, 18);
            this.capZeilenQuickInfo.Text = "Zeilen-Quick-Info: ";
            // 
            // txbGlobalScale
            // 
            this.txbGlobalScale.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbGlobalScale.Location = new System.Drawing.Point(168, 512);
            this.txbGlobalScale.Name = "txbGlobalScale";
            this.txbGlobalScale.Size = new System.Drawing.Size(160, 24);
            this.txbGlobalScale.TabIndex = 39;
            // 
            // caption1
            // 
            this.caption1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.caption1.CausesValidation = false;
            this.caption1.Location = new System.Drawing.Point(8, 512);
            this.caption1.Name = "caption1";
            this.caption1.Size = new System.Drawing.Size(144, 18);
            this.caption1.Text = "Bevorzugtes Skalierung:";
            // 
            // btnSpaltenuebersicht
            // 
            this.btnSpaltenuebersicht.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSpaltenuebersicht.ImageCode = "Spalte|16";
            this.btnSpaltenuebersicht.Location = new System.Drawing.Point(360, 538);
            this.btnSpaltenuebersicht.Name = "btnSpaltenuebersicht";
            this.btnSpaltenuebersicht.Size = new System.Drawing.Size(200, 32);
            this.btnSpaltenuebersicht.TabIndex = 35;
            this.btnSpaltenuebersicht.Text = "Spaltenübersicht";
            this.btnSpaltenuebersicht.Click += new System.EventHandler(this.btnSpaltenuebersicht_Click);
            // 
            // tabEvents
            // 
            this.tabEvents.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabEvents.Controls.Add(this.table1);
            this.tabEvents.Location = new System.Drawing.Point(4, 25);
            this.tabEvents.Name = "tabEvents";
            this.tabEvents.Padding = new System.Windows.Forms.Padding(3);
            this.tabEvents.Size = new System.Drawing.Size(1039, 616);
            this.tabEvents.TabIndex = 3;
            this.tabEvents.Text = "Event-Zuordnungen";
            // 
            // tabScripts
            // 
            this.tabScripts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabScripts.Controls.Add(this.eventScriptEditor);
            this.tabScripts.Controls.Add(this.grpVerfügbareSkripte);
            this.tabScripts.Location = new System.Drawing.Point(4, 25);
            this.tabScripts.Name = "tabScripts";
            this.tabScripts.Padding = new System.Windows.Forms.Padding(3);
            this.tabScripts.Size = new System.Drawing.Size(1039, 616);
            this.tabScripts.TabIndex = 7;
            this.tabScripts.Text = "Skripte";
            // 
            // eventScriptEditor
            // 
            this.eventScriptEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.eventScriptEditor.Database = null;
            this.eventScriptEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.eventScriptEditor.Location = new System.Drawing.Point(3, 128);
            this.eventScriptEditor.Name = "eventScriptEditor";
            this.eventScriptEditor.Size = new System.Drawing.Size(1033, 485);
            this.eventScriptEditor.TabIndex = 3;
            this.eventScriptEditor.TabStop = false;
            this.eventScriptEditor.Changed += new System.EventHandler(this.eventScript_Editor1_Changed);
            // 
            // grpVerfügbareSkripte
            // 
            this.grpVerfügbareSkripte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpVerfügbareSkripte.CausesValidation = false;
            this.grpVerfügbareSkripte.Controls.Add(this.lstEventScripts);
            this.grpVerfügbareSkripte.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpVerfügbareSkripte.Location = new System.Drawing.Point(3, 3);
            this.grpVerfügbareSkripte.Name = "grpVerfügbareSkripte";
            this.grpVerfügbareSkripte.Size = new System.Drawing.Size(1033, 125);
            this.grpVerfügbareSkripte.TabIndex = 2;
            this.grpVerfügbareSkripte.TabStop = false;
            this.grpVerfügbareSkripte.Text = "Verfügbare Skripte:";
            // 
            // lstEventScripts
            // 
            this.lstEventScripts.AddAllowed = BlueControls.Enums.AddType.UserDef;
            this.lstEventScripts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstEventScripts.FilterAllowed = true;
            this.lstEventScripts.Location = new System.Drawing.Point(8, 24);
            this.lstEventScripts.Name = "lstEventScripts";
            this.lstEventScripts.RemoveAllowed = true;
            this.lstEventScripts.Size = new System.Drawing.Size(1018, 93);
            this.lstEventScripts.TabIndex = 0;
            this.lstEventScripts.AddClicked += new System.EventHandler(this.lstEventScripts_AddClicked);
            this.lstEventScripts.ItemCheckedChanged += new System.EventHandler(this.lstEventScripts_ItemCheckedChanged);
            // 
            // tabRechte
            // 
            this.tabRechte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabRechte.Controls.Add(this.grpKennwort);
            this.tabRechte.Controls.Add(this.grpBenutzergruppen);
            this.tabRechte.Location = new System.Drawing.Point(4, 25);
            this.tabRechte.Name = "tabRechte";
            this.tabRechte.Padding = new System.Windows.Forms.Padding(3);
            this.tabRechte.Size = new System.Drawing.Size(1039, 616);
            this.tabRechte.TabIndex = 4;
            this.tabRechte.Text = "Rechte";
            // 
            // tabSortierung
            // 
            this.tabSortierung.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabSortierung.Controls.Add(this.lbxSortierSpalten);
            this.tabSortierung.Controls.Add(this.capSortierspalten);
            this.tabSortierung.Controls.Add(this.btnSortRichtung);
            this.tabSortierung.Location = new System.Drawing.Point(4, 25);
            this.tabSortierung.Name = "tabSortierung";
            this.tabSortierung.Padding = new System.Windows.Forms.Padding(3);
            this.tabSortierung.Size = new System.Drawing.Size(1039, 616);
            this.tabSortierung.TabIndex = 2;
            this.tabSortierung.Text = "Sortierung";
            // 
            // tabBackup
            // 
            this.tabBackup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabBackup.Controls.Add(this.ExportEditor);
            this.tabBackup.Controls.Add(this.grpExport);
            this.tabBackup.Location = new System.Drawing.Point(4, 25);
            this.tabBackup.Name = "tabBackup";
            this.tabBackup.Padding = new System.Windows.Forms.Padding(3);
            this.tabBackup.Size = new System.Drawing.Size(1039, 616);
            this.tabBackup.TabIndex = 5;
            this.tabBackup.Text = "Backup & Export";
            // 
            // ExportEditor
            // 
            this.ExportEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ExportEditor.CausesValidation = false;
            this.ExportEditor.Enabled = false;
            this.ExportEditor.Location = new System.Drawing.Point(8, 176);
            this.ExportEditor.Name = "ExportEditor";
            this.ExportEditor.Size = new System.Drawing.Size(1031, 437);
            this.ExportEditor.TabIndex = 0;
            this.ExportEditor.TabStop = false;
            this.ExportEditor.Text = "Export-Editor:";
            this.ExportEditor.Changed += new System.EventHandler(this.ExportEditor_Changed);
            // 
            // grpExport
            // 
            this.grpExport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpExport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpExport.CausesValidation = false;
            this.grpExport.Controls.Add(this.lbxExportSets);
            this.grpExport.Location = new System.Drawing.Point(8, 8);
            this.grpExport.Name = "grpExport";
            this.grpExport.Size = new System.Drawing.Size(1031, 168);
            this.grpExport.TabIndex = 1;
            this.grpExport.TabStop = false;
            this.grpExport.Text = "Alle Export-Aufgaben:";
            // 
            // lbxExportSets
            // 
            this.lbxExportSets.AddAllowed = BlueControls.Enums.AddType.UserDef;
            this.lbxExportSets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbxExportSets.FilterAllowed = true;
            this.lbxExportSets.Location = new System.Drawing.Point(8, 24);
            this.lbxExportSets.Name = "lbxExportSets";
            this.lbxExportSets.RemoveAllowed = true;
            this.lbxExportSets.Size = new System.Drawing.Size(1016, 136);
            this.lbxExportSets.TabIndex = 0;
            this.lbxExportSets.Text = "ExportAufgaben";
            this.lbxExportSets.AddClicked += new System.EventHandler(this.lbxExportSets_AddClicked);
            this.lbxExportSets.ItemCheckedChanged += new System.EventHandler(this.lbxExportSets_ItemCheckedChanged);
            this.lbxExportSets.RemoveClicked += new System.EventHandler<BlueControls.EventArgs.ListOfBasicListItemEventArgs>(this.lbxExportSets_RemoveClicked);
            // 
            // tabUndo
            // 
            this.tabUndo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabUndo.Controls.Add(this.btnClipboard);
            this.tabUndo.Controls.Add(this.tbxUndoAnzahl);
            this.tabUndo.Controls.Add(this.capUndoAnzahl);
            this.tabUndo.Controls.Add(this.tblUndo);
            this.tabUndo.Location = new System.Drawing.Point(4, 25);
            this.tabUndo.Name = "tabUndo";
            this.tabUndo.Size = new System.Drawing.Size(1039, 616);
            this.tabUndo.TabIndex = 6;
            this.tabUndo.Text = "Undo";
            // 
            // btnClipboard
            // 
            this.btnClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClipboard.Location = new System.Drawing.Point(864, 8);
            this.btnClipboard.Name = "btnClipboard";
            this.btnClipboard.Size = new System.Drawing.Size(168, 32);
            this.btnClipboard.TabIndex = 3;
            this.btnClipboard.Text = "Clipboard";
            this.btnClipboard.Click += new System.EventHandler(this.btnClipboard_Click);
            // 
            // tbxUndoAnzahl
            // 
            this.tbxUndoAnzahl.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxUndoAnzahl.Location = new System.Drawing.Point(176, 8);
            this.tbxUndoAnzahl.Name = "tbxUndoAnzahl";
            this.tbxUndoAnzahl.Size = new System.Drawing.Size(88, 24);
            this.tbxUndoAnzahl.TabIndex = 1;
            // 
            // capUndoAnzahl
            // 
            this.capUndoAnzahl.CausesValidation = false;
            this.capUndoAnzahl.Location = new System.Drawing.Point(0, 8);
            this.capUndoAnzahl.Name = "capUndoAnzahl";
            this.capUndoAnzahl.Size = new System.Drawing.Size(168, 40);
            this.capUndoAnzahl.Text = "Anzahl gespeicherter Undos:<br>(Standard: 300)";
            // 
            // tblUndo
            // 
            this.tblUndo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tblUndo.DropMessages = false;
            this.tblUndo.Location = new System.Drawing.Point(0, 56);
            this.tblUndo.Name = "tblUndo";
            this.tblUndo.ShowWaitScreen = true;
            this.tblUndo.Size = new System.Drawing.Size(1039, 562);
            this.tblUndo.TabIndex = 0;
            this.tblUndo.Text = "UndoTab";
            this.tblUndo.ContextMenuInit += new System.EventHandler<BlueControls.EventArgs.ContextMenuInitEventArgs>(this.tblUndo_ContextMenuInit);
            this.tblUndo.ContextMenuItemClicked += new System.EventHandler<BlueControls.EventArgs.ContextMenuItemClickedEventArgs>(this.tblUndo_ContextMenuItemClicked);
            // 
            // capBinInfo
            // 
            this.capBinInfo.CausesValidation = false;
            this.capBinInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.capBinInfo.Location = new System.Drawing.Point(0, 0);
            this.capBinInfo.Name = "capBinInfo";
            this.capBinInfo.Size = new System.Drawing.Size(1039, 24);
            this.capBinInfo.Text = "Die hier aufgeführten Binärdaten können - falls es ein Bild ist - mit DB_Dateinam" +
    "e mit Suffix angesprochen werden.";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.ImageCode = "Diskette|16";
            this.btnSave.Location = new System.Drawing.Point(840, 651);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(112, 24);
            this.btnSave.TabIndex = 22;
            this.btnSave.Text = "Speichern";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // table1
            // 
            this.table1.DropMessages = false;
            this.table1.Location = new System.Drawing.Point(16, 16);
            this.table1.Name = "table1";
            this.table1.ShowWaitScreen = true;
            this.table1.Size = new System.Drawing.Size(728, 464);
            this.table1.TabIndex = 0;
            this.table1.Text = "tblEventZuordnungen";
            // 
            // DatabaseHeadEditor
            // 
            this.ClientSize = new System.Drawing.Size(1050, 677);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.GlobalTab);
            this.Controls.Add(this.btnOk);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "DatabaseHeadEditor";
            this.ShowInTaskbar = false;
            this.Text = "Datenbank-Eigenschaften";
            this.grpBenutzergruppen.ResumeLayout(false);
            this.grpKennwort.ResumeLayout(false);
            this.GlobalTab.ResumeLayout(false);
            this.tabAllgemein.ResumeLayout(false);
            this.tabEvents.ResumeLayout(false);
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
        private  System.Windows.Forms.TabPage tabAllgemein;
        private  System.Windows.Forms.TabPage tabSortierung;
        private  System.Windows.Forms.TabPage tabEvents;
        private  System.Windows.Forms.TabPage tabRechte;
        private  System.Windows.Forms.TabPage tabBackup;
        private GroupBox grpKennwort;
        private GroupBox grpBenutzergruppen;
        private  System.Windows.Forms.TabPage tabUndo;
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
        private System.Windows.Forms.TabPage tabScripts;
        private GroupBox grpVerfügbareSkripte;
        private ListBox lstEventScripts;
        private EventScript_Editor eventScriptEditor;
        private Table table1;
    }
}
