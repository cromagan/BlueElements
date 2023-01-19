using BlueControls.Classes_Editor;
using BlueControls.Controls;
using BlueControls.Forms;
using System.Diagnostics;

namespace BlueControls.BlueDatabaseDialogs
{
    public sealed partial class DatabaseHeadEditor : Form
    {
        //Das Formular �berschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
        //Hinweis: Die folgende Prozedur ist f�r den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer m�glich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht m�glich.
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
            this.Tab_Allgemein = new System.Windows.Forms.TabPage();
            this.txbStandardFormulaFile = new BlueControls.Controls.TextBox();
            this.capStandardFormulaFile = new BlueControls.Controls.Caption();
            this.txbAdditionalFiles = new BlueControls.Controls.TextBox();
            this.capAdditional = new BlueControls.Controls.Caption();
            this.txbZeilenQuickInfo = new BlueControls.Controls.TextBox();
            this.capZeilenQuickInfo = new BlueControls.Controls.Caption();
            this.txbGlobalScale = new BlueControls.Controls.TextBox();
            this.caption1 = new BlueControls.Controls.Caption();
            this.btnSpaltenuebersicht = new BlueControls.Controls.Button();
            this.Tab_Regeln = new System.Windows.Forms.TabPage();
            this.scriptEditor = new BlueControls.ScriptEditorDatabase();
            this.Tab_Rechte = new System.Windows.Forms.TabPage();
            this.Tab_Sortierung = new System.Windows.Forms.TabPage();
            this.Tab_Backup = new System.Windows.Forms.TabPage();
            this.ExportEditor = new BlueControls.Classes_Editor.ExportDefinition_Editor();
            this.grpExport = new BlueControls.Controls.GroupBox();
            this.lbxExportSets = new BlueControls.Controls.ListBox();
            this.Tab_Undo = new System.Windows.Forms.TabPage();
            this.btnClipboard = new BlueControls.Controls.Button();
            this.tbxUndoAnzahl = new BlueControls.Controls.TextBox();
            this.capUndoAnzahl = new BlueControls.Controls.Caption();
            this.tblUndo = new BlueControls.Controls.Table();
            this.capBinInfo = new BlueControls.Controls.Caption();
            this.btnSave = new BlueControls.Controls.Button();
            this.grpBenutzergruppen.SuspendLayout();
            this.grpKennwort.SuspendLayout();
            this.GlobalTab.SuspendLayout();
            this.Tab_Allgemein.SuspendLayout();
            this.Tab_Regeln.SuspendLayout();
            this.Tab_Rechte.SuspendLayout();
            this.Tab_Sortierung.SuspendLayout();
            this.Tab_Backup.SuspendLayout();
            this.grpExport.SuspendLayout();
            this.Tab_Undo.SuspendLayout();
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
            this.capNeueZeilenInfo.Text = "<i>Administratoren d�rfen immer neue Zeilen anlegen, wenn die erste Spalte eine B" +
    "earbeitung zul�sst";
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
            this.capKennwort.Text = "Zum �ffnen der Datenbank:";
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
            this.btnSortRichtung.ButtonStyle = Enums.ButtonStyle.Pic1_or_Pic2;
            this.btnSortRichtung.ImageCode = "AZ|16|8";
            this.btnSortRichtung.ImageCode_Checked = "ZA|16|8";
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
            this.capCaption.Text = "�berschrift bzw. Titel:";
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
            this.GlobalTab.Controls.Add(this.Tab_Allgemein);
            this.GlobalTab.Controls.Add(this.Tab_Regeln);
            this.GlobalTab.Controls.Add(this.Tab_Rechte);
            this.GlobalTab.Controls.Add(this.Tab_Sortierung);
            this.GlobalTab.Controls.Add(this.Tab_Backup);
            this.GlobalTab.Controls.Add(this.Tab_Undo);
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
            // Tab_Allgemein
            // 
            this.Tab_Allgemein.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Tab_Allgemein.Controls.Add(this.txbStandardFormulaFile);
            this.Tab_Allgemein.Controls.Add(this.capStandardFormulaFile);
            this.Tab_Allgemein.Controls.Add(this.txbAdditionalFiles);
            this.Tab_Allgemein.Controls.Add(this.capAdditional);
            this.Tab_Allgemein.Controls.Add(this.txbZeilenQuickInfo);
            this.Tab_Allgemein.Controls.Add(this.capZeilenQuickInfo);
            this.Tab_Allgemein.Controls.Add(this.txbGlobalScale);
            this.Tab_Allgemein.Controls.Add(this.caption1);
            this.Tab_Allgemein.Controls.Add(this.txbTags);
            this.Tab_Allgemein.Controls.Add(this.btnSpaltenuebersicht);
            this.Tab_Allgemein.Controls.Add(this.capInfo);
            this.Tab_Allgemein.Controls.Add(this.capTags);
            this.Tab_Allgemein.Controls.Add(this.txbCaption);
            this.Tab_Allgemein.Controls.Add(this.capCaption);
            this.Tab_Allgemein.Location = new System.Drawing.Point(4, 25);
            this.Tab_Allgemein.Name = "Tab_Allgemein";
            this.Tab_Allgemein.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_Allgemein.Size = new System.Drawing.Size(1039, 616);
            this.Tab_Allgemein.TabIndex = 1;
            this.Tab_Allgemein.Text = "Allgemein";
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
            this.capAdditional.Text = "Zugeh�rige-Dateien-Pfad:";
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
            this.btnSpaltenuebersicht.Text = "Spalten�bersicht";
            this.btnSpaltenuebersicht.Click += new System.EventHandler(this.btnSpaltenuebersicht_Click);
            // 
            // Tab_Regeln
            // 
            this.Tab_Regeln.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Tab_Regeln.Controls.Add(this.scriptEditor);
            this.Tab_Regeln.Location = new System.Drawing.Point(4, 25);
            this.Tab_Regeln.Name = "Tab_Regeln";
            this.Tab_Regeln.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_Regeln.Size = new System.Drawing.Size(1039, 616);
            this.Tab_Regeln.TabIndex = 3;
            this.Tab_Regeln.Text = "Skript";
            // 
            // scriptEditor
            // 
            this.scriptEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.scriptEditor.Database = null;
            this.scriptEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scriptEditor.Location = new System.Drawing.Point(3, 3);
            this.scriptEditor.Name = "scriptEditor";
            this.scriptEditor.Size = new System.Drawing.Size(1033, 610);
            this.scriptEditor.TabIndex = 0;
            this.scriptEditor.TabStop = false;
            // 
            // Tab_Rechte
            // 
            this.Tab_Rechte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Tab_Rechte.Controls.Add(this.grpKennwort);
            this.Tab_Rechte.Controls.Add(this.grpBenutzergruppen);
            this.Tab_Rechte.Location = new System.Drawing.Point(4, 25);
            this.Tab_Rechte.Name = "Tab_Rechte";
            this.Tab_Rechte.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_Rechte.Size = new System.Drawing.Size(1039, 616);
            this.Tab_Rechte.TabIndex = 4;
            this.Tab_Rechte.Text = "Rechte";
            // 
            // Tab_Sortierung
            // 
            this.Tab_Sortierung.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Tab_Sortierung.Controls.Add(this.lbxSortierSpalten);
            this.Tab_Sortierung.Controls.Add(this.capSortierspalten);
            this.Tab_Sortierung.Controls.Add(this.btnSortRichtung);
            this.Tab_Sortierung.Location = new System.Drawing.Point(4, 25);
            this.Tab_Sortierung.Name = "Tab_Sortierung";
            this.Tab_Sortierung.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_Sortierung.Size = new System.Drawing.Size(1039, 616);
            this.Tab_Sortierung.TabIndex = 2;
            this.Tab_Sortierung.Text = "Sortierung";
            // 
            // Tab_Backup
            // 
            this.Tab_Backup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Tab_Backup.Controls.Add(this.ExportEditor);
            this.Tab_Backup.Controls.Add(this.grpExport);
            this.Tab_Backup.Location = new System.Drawing.Point(4, 25);
            this.Tab_Backup.Name = "Tab_Backup";
            this.Tab_Backup.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_Backup.Size = new System.Drawing.Size(1039, 616);
            this.Tab_Backup.TabIndex = 5;
            this.Tab_Backup.Text = "Backup & Export";
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
            this.lbxExportSets.AddClicked += new System.EventHandler(this.ExportSets_AddClicked);
            this.lbxExportSets.ItemCheckedChanged += new System.EventHandler(this.lbxExportSets_ItemCheckedChanged);
            this.lbxExportSets.RemoveClicked += new System.EventHandler<BlueControls.EventArgs.ListOfBasicListItemEventArgs>(this.lbxExportSets_RemoveClicked);
            // 
            // Tab_Undo
            // 
            this.Tab_Undo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Tab_Undo.Controls.Add(this.btnClipboard);
            this.Tab_Undo.Controls.Add(this.tbxUndoAnzahl);
            this.Tab_Undo.Controls.Add(this.capUndoAnzahl);
            this.Tab_Undo.Controls.Add(this.tblUndo);
            this.Tab_Undo.Location = new System.Drawing.Point(4, 25);
            this.Tab_Undo.Name = "Tab_Undo";
            this.Tab_Undo.Size = new System.Drawing.Size(1039, 616);
            this.Tab_Undo.TabIndex = 6;
            this.Tab_Undo.Text = "Undo";
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
            this.capBinInfo.Text = "Die hier aufgef�hrten Bin�rdaten k�nnen - falls es ein Bild ist - mit DB_Dateinam" +
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
            this.Tab_Allgemein.ResumeLayout(false);
            this.Tab_Regeln.ResumeLayout(false);
            this.Tab_Rechte.ResumeLayout(false);
            this.Tab_Sortierung.ResumeLayout(false);
            this.Tab_Backup.ResumeLayout(false);
            this.grpExport.ResumeLayout(false);
            this.Tab_Undo.ResumeLayout(false);
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
        private  System.Windows.Forms.TabPage Tab_Allgemein;
        private  System.Windows.Forms.TabPage Tab_Sortierung;
        private  System.Windows.Forms.TabPage Tab_Regeln;
        private  System.Windows.Forms.TabPage Tab_Rechte;
        private  System.Windows.Forms.TabPage Tab_Backup;
        private GroupBox grpKennwort;
        private GroupBox grpBenutzergruppen;
        private  System.Windows.Forms.TabPage Tab_Undo;
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
        private ScriptEditorDatabase scriptEditor;
        private Button btnClipboard;
        private TextBox txbStandardFormulaFile;
        private Caption capStandardFormulaFile;
    }
}
