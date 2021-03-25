using BlueControls.Classes_Editor;
using BlueControls.Controls;
using BlueControls.Forms;
using System.Diagnostics;

namespace BlueControls.BlueDatabaseDialogs
{

    internal sealed partial class DatabaseHeadEditor : Form
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
            this.components = new System.ComponentModel.Container();
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
            this.btnSperreAufheben = new BlueControls.Controls.Button();
            this.tbxTags = new BlueControls.Controls.TextBox();
            this.txbCaption = new BlueControls.Controls.TextBox();
            this.capCaption = new BlueControls.Controls.Caption();
            this.capTags = new BlueControls.Controls.Caption();
            this.capInfo = new BlueControls.Controls.Caption();
            this.Caption14 = new BlueControls.Controls.Caption();
            this.Caption13 = new BlueControls.Controls.Caption();
            this.Caption11 = new BlueControls.Controls.Caption();
            this.GlobalTab = new BlueControls.Controls.TabControl();
            this.Tab_Allgemein = new BlueControls.Controls.TabPage();
            this.txbZeilenQuickInfo = new BlueControls.Controls.TextBox();
            this.capZeilenQuickInfo = new BlueControls.Controls.Caption();
            this.txbFilterImagePath = new BlueControls.Controls.TextBox();
            this.capFilterImagePath = new BlueControls.Controls.Caption();
            this.txbGlobalScale = new BlueControls.Controls.TextBox();
            this.caption1 = new BlueControls.Controls.Caption();
            this.cbxVerwaisteDaten = new BlueControls.Controls.ComboBox();
            this.capVerwaisteDaten = new BlueControls.Controls.Caption();
            this.btnSpaltenuebersicht = new BlueControls.Controls.Button();
            this.tbxReloadVerzoegerung = new BlueControls.Controls.TextBox();
            this.capJoinTyp = new BlueControls.Controls.Caption();
            this.capReloadVerzoegerung = new BlueControls.Controls.Caption();
            this.cbxAnsicht = new BlueControls.Controls.ComboBox();
            this.cbxJoinTyp = new BlueControls.Controls.ComboBox();
            this.capAnsicht = new BlueControls.Controls.Caption();
            this.Tab_Regeln = new BlueControls.Controls.TabPage();
            this.tabCSckript = new BlueControls.Controls.TabControl();
            this.tabScriptAnzeige = new BlueControls.Controls.TabPage();
            this.btnExtern = new BlueControls.Controls.Button();
            this.txtSkript = new BlueControls.Controls.TextBox();
            this.tabBefehle = new BlueControls.Controls.TabPage();
            this.txbComms = new BlueControls.Controls.TextBox();
            this.tabVariablen = new BlueControls.Controls.TabPage();
            this.tableVariablen = new BlueControls.Controls.Table();
            this.filterVariablen = new BlueControls.BlueDatabaseDialogs.Filterleiste();
            this.grpTextAllgemein = new BlueControls.Controls.GroupBox();
            this.txbTestZeile = new BlueControls.Controls.TextBox();
            this.capTestZeile = new BlueControls.Controls.Caption();
            this.txbSkriptInfo = new BlueControls.Controls.TextBox();
            this.btnTest = new BlueControls.Controls.Button();
            this.Tab_Backup = new BlueControls.Controls.TabPage();
            this.ExportEditor = new BlueControls.Classes_Editor.ExportDefinition_Editor();
            this.grpExport = new BlueControls.Controls.GroupBox();
            this.lbxExportSets = new BlueControls.Controls.ListBox();
            this.Tab_Rechte = new BlueControls.Controls.TabPage();
            this.grpDateiVerschluesselung = new BlueControls.Controls.GroupBox();
            this.capDateiverschluesselungInfo = new BlueControls.Controls.Caption();
            this.btnDateiSchluessel = new BlueControls.Controls.Button();
            this.Tab_Sortierung = new BlueControls.Controls.TabPage();
            this.Tab_Undo = new BlueControls.Controls.TabPage();
            this.tbxUndoAnzahl = new BlueControls.Controls.TextBox();
            this.capUndoAnzahl = new BlueControls.Controls.Caption();
            this.tblUndo = new BlueControls.Controls.Table();
            this.Tab_Expermimentell = new BlueControls.Controls.TabPage();
            this.btnFremdImport = new BlueControls.Controls.Button();
            this.capExperimentellWarnung = new BlueControls.Controls.Caption();
            this.capBinInfo = new BlueControls.Controls.Caption();
            this.ExternTimer = new System.Windows.Forms.Timer(this.components);
            this.capAdditional = new BlueControls.Controls.Caption();
            this.txbAdditionalFiles = new BlueControls.Controls.TextBox();
            this.grpBenutzergruppen.SuspendLayout();
            this.grpKennwort.SuspendLayout();
            this.GlobalTab.SuspendLayout();
            this.Tab_Allgemein.SuspendLayout();
            this.Tab_Regeln.SuspendLayout();
            this.tabCSckript.SuspendLayout();
            this.tabScriptAnzeige.SuspendLayout();
            this.tabBefehle.SuspendLayout();
            this.tabVariablen.SuspendLayout();
            this.grpTextAllgemein.SuspendLayout();
            this.Tab_Backup.SuspendLayout();
            this.grpExport.SuspendLayout();
            this.Tab_Rechte.SuspendLayout();
            this.grpDateiVerschluesselung.SuspendLayout();
            this.Tab_Sortierung.SuspendLayout();
            this.Tab_Undo.SuspendLayout();
            this.Tab_Expermimentell.SuspendLayout();
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
            this.PermissionGroups_NewRow.AddAllowed = BlueControls.Enums.enAddType.Text;
            this.PermissionGroups_NewRow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.PermissionGroups_NewRow.CheckBehavior = BlueControls.Enums.enCheckBehavior.MultiSelection;
            this.PermissionGroups_NewRow.FilterAllowed = true;
            this.PermissionGroups_NewRow.LastFilePath = null;
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
            this.capNeueZeilenInfo.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption22
            // 
            this.Caption22.CausesValidation = false;
            this.Caption22.Location = new System.Drawing.Point(8, 24);
            this.Caption22.Name = "Caption22";
            this.Caption22.Size = new System.Drawing.Size(176, 22);
            this.Caption22.Text = "Datenbank-Administratoren:";
            this.Caption22.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // DatenbankAdmin
            // 
            this.DatenbankAdmin.AddAllowed = BlueControls.Enums.enAddType.Text;
            this.DatenbankAdmin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.DatenbankAdmin.CheckBehavior = BlueControls.Enums.enCheckBehavior.MultiSelection;
            this.DatenbankAdmin.FilterAllowed = true;
            this.DatenbankAdmin.LastFilePath = null;
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
            this.capNeueZeilen.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
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
            this.capKennwort.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
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
            this.lbxSortierSpalten.AddAllowed = BlueControls.Enums.enAddType.OnlySuggests;
            this.lbxSortierSpalten.CheckBehavior = BlueControls.Enums.enCheckBehavior.MultiSelection;
            this.lbxSortierSpalten.LastFilePath = null;
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
            this.capSortierspalten.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnSortRichtung
            // 
            this.btnSortRichtung.ButtonStyle = BlueControls.Enums.enButtonStyle.Pic1_or_Pic2;
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
            // btnSperreAufheben
            // 
            this.btnSperreAufheben.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSperreAufheben.ImageCode = "Häkchen|16";
            this.btnSperreAufheben.Location = new System.Drawing.Point(360, 498);
            this.btnSperreAufheben.Name = "btnSperreAufheben";
            this.btnSperreAufheben.Size = new System.Drawing.Size(200, 32);
            this.btnSperreAufheben.TabIndex = 28;
            this.btnSperreAufheben.Text = "Datenbank-Sperre aufheben";
            this.btnSperreAufheben.Click += new System.EventHandler(this.btnSperreAufheben_Click);
            // 
            // tbxTags
            // 
            this.tbxTags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxTags.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxTags.Location = new System.Drawing.Point(568, 24);
            this.tbxTags.MultiLine = true;
            this.tbxTags.Name = "tbxTags";
            this.tbxTags.Size = new System.Drawing.Size(463, 581);
            this.tbxTags.TabIndex = 26;
            this.tbxTags.Verhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
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
            this.capCaption.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capTags
            // 
            this.capTags.CausesValidation = false;
            this.capTags.Location = new System.Drawing.Point(568, 8);
            this.capTags.Name = "capTags";
            this.capTags.QuickInfo = "Tags / Eigenschaften, die von einem ";
            this.capTags.Size = new System.Drawing.Size(152, 16);
            this.capTags.Text = "Tags:";
            this.capTags.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capInfo
            // 
            this.capInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.capInfo.CausesValidation = false;
            this.capInfo.Location = new System.Drawing.Point(8, 56);
            this.capInfo.Name = "capInfo";
            this.capInfo.Size = new System.Drawing.Size(552, 90);
            this.capInfo.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption14
            // 
            this.Caption14.CausesValidation = false;
            this.Caption14.Location = new System.Drawing.Point(448, 24);
            this.Caption14.Name = "Caption14";
            this.Caption14.Size = new System.Drawing.Size(79, 18);
            this.Caption14.Text = "(in der Spalte)";
            // 
            // Caption13
            // 
            this.Caption13.CausesValidation = false;
            this.Caption13.Location = new System.Drawing.Point(264, 24);
            this.Caption13.Name = "Caption13";
            this.Caption13.Size = new System.Drawing.Size(42, 18);
            this.Caption13.Text = "Dann...";
            // 
            // Caption11
            // 
            this.Caption11.CausesValidation = false;
            this.Caption11.Location = new System.Drawing.Point(8, 16);
            this.Caption11.Name = "Caption11";
            this.Caption11.Size = new System.Drawing.Size(232, 16);
            this.Caption11.Text = "Dann führe all diese Aktionen aus:";
            this.Caption11.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // GlobalTab
            // 
            this.GlobalTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GlobalTab.Controls.Add(this.Tab_Allgemein);
            this.GlobalTab.Controls.Add(this.Tab_Regeln);
            this.GlobalTab.Controls.Add(this.Tab_Backup);
            this.GlobalTab.Controls.Add(this.Tab_Rechte);
            this.GlobalTab.Controls.Add(this.Tab_Sortierung);
            this.GlobalTab.Controls.Add(this.Tab_Undo);
            this.GlobalTab.Controls.Add(this.Tab_Expermimentell);
            this.GlobalTab.HotTrack = true;
            this.GlobalTab.Location = new System.Drawing.Point(0, 0);
            this.GlobalTab.Name = "GlobalTab";
            this.GlobalTab.SelectedIndex = 0;
            this.GlobalTab.Size = new System.Drawing.Size(1047, 645);
            this.GlobalTab.TabIndex = 21;
            // 
            // Tab_Allgemein
            // 
            this.Tab_Allgemein.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Tab_Allgemein.Controls.Add(this.txbAdditionalFiles);
            this.Tab_Allgemein.Controls.Add(this.capAdditional);
            this.Tab_Allgemein.Controls.Add(this.txbZeilenQuickInfo);
            this.Tab_Allgemein.Controls.Add(this.capZeilenQuickInfo);
            this.Tab_Allgemein.Controls.Add(this.txbFilterImagePath);
            this.Tab_Allgemein.Controls.Add(this.capFilterImagePath);
            this.Tab_Allgemein.Controls.Add(this.txbGlobalScale);
            this.Tab_Allgemein.Controls.Add(this.caption1);
            this.Tab_Allgemein.Controls.Add(this.cbxVerwaisteDaten);
            this.Tab_Allgemein.Controls.Add(this.capVerwaisteDaten);
            this.Tab_Allgemein.Controls.Add(this.tbxTags);
            this.Tab_Allgemein.Controls.Add(this.btnSpaltenuebersicht);
            this.Tab_Allgemein.Controls.Add(this.tbxReloadVerzoegerung);
            this.Tab_Allgemein.Controls.Add(this.capJoinTyp);
            this.Tab_Allgemein.Controls.Add(this.capReloadVerzoegerung);
            this.Tab_Allgemein.Controls.Add(this.cbxAnsicht);
            this.Tab_Allgemein.Controls.Add(this.cbxJoinTyp);
            this.Tab_Allgemein.Controls.Add(this.capAnsicht);
            this.Tab_Allgemein.Controls.Add(this.capInfo);
            this.Tab_Allgemein.Controls.Add(this.capTags);
            this.Tab_Allgemein.Controls.Add(this.txbCaption);
            this.Tab_Allgemein.Controls.Add(this.btnSperreAufheben);
            this.Tab_Allgemein.Controls.Add(this.capCaption);
            this.Tab_Allgemein.Location = new System.Drawing.Point(4, 25);
            this.Tab_Allgemein.Name = "Tab_Allgemein";
            this.Tab_Allgemein.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_Allgemein.Size = new System.Drawing.Size(1039, 616);
            this.Tab_Allgemein.TabIndex = 1;
            this.Tab_Allgemein.Text = "Allgemein";
            // 
            // txbZeilenQuickInfo
            // 
            this.txbZeilenQuickInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txbZeilenQuickInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbZeilenQuickInfo.Location = new System.Drawing.Point(8, 176);
            this.txbZeilenQuickInfo.MultiLine = true;
            this.txbZeilenQuickInfo.Name = "txbZeilenQuickInfo";
            this.txbZeilenQuickInfo.Size = new System.Drawing.Size(552, 168);
            this.txbZeilenQuickInfo.TabIndex = 43;
            this.txbZeilenQuickInfo.Verhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
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
            // txbFilterImagePath
            // 
            this.txbFilterImagePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txbFilterImagePath.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbFilterImagePath.Location = new System.Drawing.Point(8, 368);
            this.txbFilterImagePath.Name = "txbFilterImagePath";
            this.txbFilterImagePath.Size = new System.Drawing.Size(552, 24);
            this.txbFilterImagePath.TabIndex = 41;
            this.txbFilterImagePath.Verhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capFilterImagePath
            // 
            this.capFilterImagePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.capFilterImagePath.CausesValidation = false;
            this.capFilterImagePath.Location = new System.Drawing.Point(8, 352);
            this.capFilterImagePath.Name = "capFilterImagePath";
            this.capFilterImagePath.Size = new System.Drawing.Size(152, 18);
            this.capFilterImagePath.Text = "Filter-Bild-Datei-Pfad:";
            // 
            // txbGlobalScale
            // 
            this.txbGlobalScale.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbGlobalScale.Format = BlueBasics.Enums.enDataFormat.Gleitkommazahl;
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
            // cbxVerwaisteDaten
            // 
            this.cbxVerwaisteDaten.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbxVerwaisteDaten.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxVerwaisteDaten.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxVerwaisteDaten.Location = new System.Drawing.Point(168, 582);
            this.cbxVerwaisteDaten.Name = "cbxVerwaisteDaten";
            this.cbxVerwaisteDaten.Size = new System.Drawing.Size(160, 24);
            this.cbxVerwaisteDaten.TabIndex = 37;
            // 
            // capVerwaisteDaten
            // 
            this.capVerwaisteDaten.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.capVerwaisteDaten.CausesValidation = false;
            this.capVerwaisteDaten.Location = new System.Drawing.Point(8, 584);
            this.capVerwaisteDaten.Name = "capVerwaisteDaten";
            this.capVerwaisteDaten.Size = new System.Drawing.Size(120, 18);
            this.capVerwaisteDaten.Text = "Verwaiste Daten:";
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
            // tbxReloadVerzoegerung
            // 
            this.tbxReloadVerzoegerung.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxReloadVerzoegerung.Format = BlueBasics.Enums.enDataFormat.Ganzzahl;
            this.tbxReloadVerzoegerung.Location = new System.Drawing.Point(168, 488);
            this.tbxReloadVerzoegerung.Name = "tbxReloadVerzoegerung";
            this.tbxReloadVerzoegerung.Size = new System.Drawing.Size(160, 24);
            this.tbxReloadVerzoegerung.Suffix = "Sek.";
            this.tbxReloadVerzoegerung.TabIndex = 34;
            // 
            // capJoinTyp
            // 
            this.capJoinTyp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.capJoinTyp.CausesValidation = false;
            this.capJoinTyp.Location = new System.Drawing.Point(8, 466);
            this.capJoinTyp.Name = "capJoinTyp";
            this.capJoinTyp.Size = new System.Drawing.Size(152, 18);
            this.capJoinTyp.Text = "Dopplte Zeilen Verhalten:";
            // 
            // capReloadVerzoegerung
            // 
            this.capReloadVerzoegerung.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.capReloadVerzoegerung.CausesValidation = false;
            this.capReloadVerzoegerung.Location = new System.Drawing.Point(8, 490);
            this.capReloadVerzoegerung.Name = "capReloadVerzoegerung";
            this.capReloadVerzoegerung.Size = new System.Drawing.Size(136, 18);
            this.capReloadVerzoegerung.Text = "Reload-Verzögerung:";
            // 
            // cbxAnsicht
            // 
            this.cbxAnsicht.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbxAnsicht.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxAnsicht.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxAnsicht.Location = new System.Drawing.Point(168, 558);
            this.cbxAnsicht.Name = "cbxAnsicht";
            this.cbxAnsicht.Size = new System.Drawing.Size(160, 24);
            this.cbxAnsicht.TabIndex = 32;
            // 
            // cbxJoinTyp
            // 
            this.cbxJoinTyp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbxJoinTyp.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxJoinTyp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxJoinTyp.Location = new System.Drawing.Point(168, 466);
            this.cbxJoinTyp.Name = "cbxJoinTyp";
            this.cbxJoinTyp.Size = new System.Drawing.Size(160, 24);
            this.cbxJoinTyp.TabIndex = 31;
            // 
            // capAnsicht
            // 
            this.capAnsicht.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.capAnsicht.CausesValidation = false;
            this.capAnsicht.Location = new System.Drawing.Point(8, 560);
            this.capAnsicht.Name = "capAnsicht";
            this.capAnsicht.Size = new System.Drawing.Size(120, 18);
            this.capAnsicht.Text = "Bevorzugte Ansicht:";
            // 
            // Tab_Regeln
            // 
            this.Tab_Regeln.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Tab_Regeln.Controls.Add(this.tabCSckript);
            this.Tab_Regeln.Controls.Add(this.grpTextAllgemein);
            this.Tab_Regeln.Location = new System.Drawing.Point(4, 25);
            this.Tab_Regeln.Name = "Tab_Regeln";
            this.Tab_Regeln.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_Regeln.Size = new System.Drawing.Size(1039, 616);
            this.Tab_Regeln.TabIndex = 3;
            this.Tab_Regeln.Text = "Skript";
            // 
            // tabCSckript
            // 
            this.tabCSckript.Controls.Add(this.tabScriptAnzeige);
            this.tabCSckript.Controls.Add(this.tabBefehle);
            this.tabCSckript.Controls.Add(this.tabVariablen);
            this.tabCSckript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCSckript.HotTrack = true;
            this.tabCSckript.Location = new System.Drawing.Point(3, 104);
            this.tabCSckript.Name = "tabCSckript";
            this.tabCSckript.SelectedIndex = 0;
            this.tabCSckript.Size = new System.Drawing.Size(1033, 509);
            this.tabCSckript.TabIndex = 0;
            this.tabCSckript.SelectedIndexChanged += new System.EventHandler(this.tabCSckript_SelectedIndexChanged);
            // 
            // tabScriptAnzeige
            // 
            this.tabScriptAnzeige.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabScriptAnzeige.Controls.Add(this.btnExtern);
            this.tabScriptAnzeige.Controls.Add(this.txtSkript);
            this.tabScriptAnzeige.Location = new System.Drawing.Point(4, 25);
            this.tabScriptAnzeige.Name = "tabScriptAnzeige";
            this.tabScriptAnzeige.Size = new System.Drawing.Size(1025, 480);
            this.tabScriptAnzeige.TabIndex = 0;
            this.tabScriptAnzeige.Text = "Skript-Text";
            // 
            // btnExtern
            // 
            this.btnExtern.Location = new System.Drawing.Point(888, 0);
            this.btnExtern.Name = "btnExtern";
            this.btnExtern.Size = new System.Drawing.Size(112, 40);
            this.btnExtern.TabIndex = 1;
            this.btnExtern.Text = "Extern öffnen";
            this.btnExtern.Click += new System.EventHandler(this.btnExtern_Click);
            // 
            // txtSkript
            // 
            this.txtSkript.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtSkript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSkript.Location = new System.Drawing.Point(0, 0);
            this.txtSkript.MultiLine = true;
            this.txtSkript.Name = "txtSkript";
            this.txtSkript.Size = new System.Drawing.Size(1025, 480);
            this.txtSkript.TabIndex = 0;
            // 
            // tabBefehle
            // 
            this.tabBefehle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabBefehle.Controls.Add(this.txbComms);
            this.tabBefehle.Location = new System.Drawing.Point(4, 25);
            this.tabBefehle.Name = "tabBefehle";
            this.tabBefehle.Size = new System.Drawing.Size(1025, 480);
            this.tabBefehle.TabIndex = 2;
            this.tabBefehle.Text = "Befehle";
            // 
            // txbComms
            // 
            this.txbComms.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbComms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txbComms.Location = new System.Drawing.Point(0, 0);
            this.txbComms.Name = "txbComms";
            this.txbComms.Size = new System.Drawing.Size(1025, 480);
            this.txbComms.TabIndex = 2;
            this.txbComms.Verhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // tabVariablen
            // 
            this.tabVariablen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabVariablen.Controls.Add(this.tableVariablen);
            this.tabVariablen.Controls.Add(this.filterVariablen);
            this.tabVariablen.Location = new System.Drawing.Point(4, 25);
            this.tabVariablen.Name = "tabVariablen";
            this.tabVariablen.Size = new System.Drawing.Size(1025, 480);
            this.tabVariablen.TabIndex = 1;
            this.tabVariablen.Text = "Variablen";
            // 
            // tableVariablen
            // 
            this.tableVariablen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableVariablen.Location = new System.Drawing.Point(0, 40);
            this.tableVariablen.Name = "tableVariablen";
            this.tableVariablen.Size = new System.Drawing.Size(1025, 440);
            this.tableVariablen.TabIndex = 2;
            this.tableVariablen.Text = "tabVariablen";
            // 
            // filterVariablen
            // 
            this.filterVariablen.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.filterVariablen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.filterVariablen.Dock = System.Windows.Forms.DockStyle.Top;
            this.filterVariablen.Location = new System.Drawing.Point(0, 0);
            this.filterVariablen.Name = "filterVariablen";
            this.filterVariablen.Size = new System.Drawing.Size(1025, 40);
            this.filterVariablen.TabIndex = 1;
            this.filterVariablen.TabStop = false;
            // 
            // grpTextAllgemein
            // 
            this.grpTextAllgemein.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpTextAllgemein.Controls.Add(this.txbTestZeile);
            this.grpTextAllgemein.Controls.Add(this.capTestZeile);
            this.grpTextAllgemein.Controls.Add(this.txbSkriptInfo);
            this.grpTextAllgemein.Controls.Add(this.btnTest);
            this.grpTextAllgemein.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpTextAllgemein.Location = new System.Drawing.Point(3, 3);
            this.grpTextAllgemein.Name = "grpTextAllgemein";
            this.grpTextAllgemein.Size = new System.Drawing.Size(1033, 101);
            this.grpTextAllgemein.TabIndex = 1;
            this.grpTextAllgemein.TabStop = false;
            this.grpTextAllgemein.Text = "Allgemein";
            // 
            // txbTestZeile
            // 
            this.txbTestZeile.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbTestZeile.Location = new System.Drawing.Point(8, 64);
            this.txbTestZeile.Name = "txbTestZeile";
            this.txbTestZeile.Size = new System.Drawing.Size(144, 24);
            this.txbTestZeile.TabIndex = 2;
            // 
            // capTestZeile
            // 
            this.capTestZeile.CausesValidation = false;
            this.capTestZeile.Location = new System.Drawing.Point(8, 48);
            this.capTestZeile.Name = "capTestZeile";
            this.capTestZeile.Size = new System.Drawing.Size(72, 16);
            this.capTestZeile.Text = "Test-Zeile:";
            // 
            // txbSkriptInfo
            // 
            this.txbSkriptInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbSkriptInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbSkriptInfo.Location = new System.Drawing.Point(160, 16);
            this.txbSkriptInfo.Name = "txbSkriptInfo";
            this.txbSkriptInfo.Size = new System.Drawing.Size(864, 80);
            this.txbSkriptInfo.TabIndex = 1;
            this.txbSkriptInfo.Verhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(8, 16);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(136, 24);
            this.btnTest.TabIndex = 0;
            this.btnTest.Text = "Testen";
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
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
            this.lbxExportSets.AddAllowed = BlueControls.Enums.enAddType.UserDef;
            this.lbxExportSets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbxExportSets.FilterAllowed = true;
            this.lbxExportSets.LastFilePath = null;
            this.lbxExportSets.Location = new System.Drawing.Point(8, 24);
            this.lbxExportSets.Name = "lbxExportSets";
            this.lbxExportSets.RemoveAllowed = true;
            this.lbxExportSets.Size = new System.Drawing.Size(2046, 304);
            this.lbxExportSets.TabIndex = 0;
            this.lbxExportSets.Text = "ExportAufgaben";
            this.lbxExportSets.ItemCheckedChanged += new System.EventHandler(this.lbxExportSets_ItemCheckedChanged);
            this.lbxExportSets.AddClicked += new System.EventHandler(this.ExportSets_AddClicked);
            this.lbxExportSets.RemoveClicked += new System.EventHandler<BlueControls.EventArgs.ListOfBasicListItemEventArgs>(this.lbxExportSets_RemoveClicked);
            // 
            // Tab_Rechte
            // 
            this.Tab_Rechte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Tab_Rechte.Controls.Add(this.grpDateiVerschluesselung);
            this.Tab_Rechte.Controls.Add(this.grpKennwort);
            this.Tab_Rechte.Controls.Add(this.grpBenutzergruppen);
            this.Tab_Rechte.Location = new System.Drawing.Point(4, 25);
            this.Tab_Rechte.Name = "Tab_Rechte";
            this.Tab_Rechte.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_Rechte.Size = new System.Drawing.Size(1039, 616);
            this.Tab_Rechte.TabIndex = 4;
            this.Tab_Rechte.Text = "Rechte";
            // 
            // grpDateiVerschluesselung
            // 
            this.grpDateiVerschluesselung.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpDateiVerschluesselung.CausesValidation = false;
            this.grpDateiVerschluesselung.Controls.Add(this.capDateiverschluesselungInfo);
            this.grpDateiVerschluesselung.Controls.Add(this.btnDateiSchluessel);
            this.grpDateiVerschluesselung.Location = new System.Drawing.Point(400, 120);
            this.grpDateiVerschluesselung.Name = "grpDateiVerschluesselung";
            this.grpDateiVerschluesselung.Size = new System.Drawing.Size(232, 176);
            this.grpDateiVerschluesselung.TabIndex = 0;
            this.grpDateiVerschluesselung.TabStop = false;
            this.grpDateiVerschluesselung.Text = "Datei-Verschlüsselung:";
            // 
            // capDateiverschluesselungInfo
            // 
            this.capDateiverschluesselungInfo.CausesValidation = false;
            this.capDateiverschluesselungInfo.Location = new System.Drawing.Point(8, 24);
            this.capDateiverschluesselungInfo.Name = "capDateiverschluesselungInfo";
            this.capDateiverschluesselungInfo.Size = new System.Drawing.Size(216, 72);
            this.capDateiverschluesselungInfo.Text = "Dazugehörige Dateien im Dateisystem können mit diesen Kopf verschlüsselt oder wie" +
    "der entschlüsselt werden.";
            this.capDateiverschluesselungInfo.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnDateiSchluessel
            // 
            this.btnDateiSchluessel.Location = new System.Drawing.Point(8, 112);
            this.btnDateiSchluessel.Name = "btnDateiSchluessel";
            this.btnDateiSchluessel.Size = new System.Drawing.Size(216, 48);
            this.btnDateiSchluessel.TabIndex = 1;
            this.btnDateiSchluessel.Text = "Dateien ";
            this.btnDateiSchluessel.Click += new System.EventHandler(this.DateienSchlüssel_Click);
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
            // Tab_Undo
            // 
            this.Tab_Undo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Tab_Undo.Controls.Add(this.tbxUndoAnzahl);
            this.Tab_Undo.Controls.Add(this.capUndoAnzahl);
            this.Tab_Undo.Controls.Add(this.tblUndo);
            this.Tab_Undo.Location = new System.Drawing.Point(4, 25);
            this.Tab_Undo.Name = "Tab_Undo";
            this.Tab_Undo.Size = new System.Drawing.Size(1039, 616);
            this.Tab_Undo.TabIndex = 6;
            this.Tab_Undo.Text = "Undo";
            // 
            // tbxUndoAnzahl
            // 
            this.tbxUndoAnzahl.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxUndoAnzahl.Format = BlueBasics.Enums.enDataFormat.Ganzzahl;
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
            this.tblUndo.Location = new System.Drawing.Point(0, 56);
            this.tblUndo.Name = "tblUndo";
            this.tblUndo.Size = new System.Drawing.Size(1039, 562);
            this.tblUndo.TabIndex = 0;
            this.tblUndo.Text = "UndoTab";
            // 
            // Tab_Expermimentell
            // 
            this.Tab_Expermimentell.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Tab_Expermimentell.Controls.Add(this.btnFremdImport);
            this.Tab_Expermimentell.Controls.Add(this.capExperimentellWarnung);
            this.Tab_Expermimentell.Location = new System.Drawing.Point(4, 25);
            this.Tab_Expermimentell.Name = "Tab_Expermimentell";
            this.Tab_Expermimentell.Size = new System.Drawing.Size(1039, 616);
            this.Tab_Expermimentell.TabIndex = 8;
            this.Tab_Expermimentell.Text = "Experimentell";
            // 
            // btnFremdImport
            // 
            this.btnFremdImport.Location = new System.Drawing.Point(16, 112);
            this.btnFremdImport.Name = "btnFremdImport";
            this.btnFremdImport.Size = new System.Drawing.Size(256, 48);
            this.btnFremdImport.TabIndex = 1;
            this.btnFremdImport.Text = "Import aus fremder Datenbank";
            this.btnFremdImport.Click += new System.EventHandler(this.btnFremdImport_Click);
            // 
            // capExperimentellWarnung
            // 
            this.capExperimentellWarnung.CausesValidation = false;
            this.capExperimentellWarnung.Location = new System.Drawing.Point(16, 16);
            this.capExperimentellWarnung.Name = "capExperimentellWarnung";
            this.capExperimentellWarnung.Size = new System.Drawing.Size(488, 80);
            this.capExperimentellWarnung.Text = resources.GetString("capExperimentellWarnung.Text");
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
            // ExternTimer
            // 
            this.ExternTimer.Interval = 1000;
            this.ExternTimer.Tick += new System.EventHandler(this.ExternTimer_Tick);
            // 
            // capAdditional
            // 
            this.capAdditional.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.capAdditional.CausesValidation = false;
            this.capAdditional.Location = new System.Drawing.Point(8, 400);
            this.capAdditional.Name = "capAdditional";
            this.capAdditional.Size = new System.Drawing.Size(152, 18);
            this.capAdditional.Text = "Zugehörige-Dateien-Pfad:";
            // 
            // txbAdditionalFiles
            // 
            this.txbAdditionalFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txbAdditionalFiles.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbAdditionalFiles.Location = new System.Drawing.Point(8, 416);
            this.txbAdditionalFiles.Name = "txbAdditionalFiles";
            this.txbAdditionalFiles.Size = new System.Drawing.Size(552, 24);
            this.txbAdditionalFiles.TabIndex = 45;
            this.txbAdditionalFiles.Verhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // DatabaseHeadEditor
            // 
            this.ClientSize = new System.Drawing.Size(1050, 677);
            this.Controls.Add(this.GlobalTab);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DatabaseHeadEditor";
            this.ShowInTaskbar = false;
            this.Text = "Datenbank-Eigenschaften";
            this.grpBenutzergruppen.ResumeLayout(false);
            this.grpKennwort.ResumeLayout(false);
            this.GlobalTab.ResumeLayout(false);
            this.Tab_Allgemein.ResumeLayout(false);
            this.Tab_Regeln.ResumeLayout(false);
            this.tabCSckript.ResumeLayout(false);
            this.tabScriptAnzeige.ResumeLayout(false);
            this.tabBefehle.ResumeLayout(false);
            this.tabVariablen.ResumeLayout(false);
            this.grpTextAllgemein.ResumeLayout(false);
            this.Tab_Backup.ResumeLayout(false);
            this.grpExport.ResumeLayout(false);
            this.Tab_Rechte.ResumeLayout(false);
            this.grpDateiVerschluesselung.ResumeLayout(false);
            this.Tab_Sortierung.ResumeLayout(false);
            this.Tab_Undo.ResumeLayout(false);
            this.Tab_Expermimentell.ResumeLayout(false);
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
        private TextBox tbxTags;
        private ListBox lbxSortierSpalten;
        private Button btnSperreAufheben;
        private Caption Caption11;
        private Caption Caption14;
        private Caption Caption13;
        private Caption Caption22;
        private ListBox DatenbankAdmin;
        private TabPage Tab_Allgemein;
        private TabPage Tab_Sortierung;
        private TabPage Tab_Regeln;
        private TabPage Tab_Rechte;
        private TabPage Tab_Backup;
        private ComboBox cbxAnsicht;
        private Caption capAnsicht;
        private GroupBox grpKennwort;
        private GroupBox grpBenutzergruppen;
        private TabPage Tab_Undo;
        private ListBox lbxExportSets;
        private Caption capKennwort;
        private TextBox tbxReloadVerzoegerung;
        private Caption capReloadVerzoegerung;
        private GroupBox grpExport;
        private ExportDefinition_Editor ExportEditor;
        private Button btnSpaltenuebersicht;
        private Button btnDateiSchluessel;
        private TabPage Tab_Expermimentell;
        private Caption capExperimentellWarnung;
        private Button btnFremdImport;
        private Table tblUndo;
        private GroupBox grpDateiVerschluesselung;
        private Caption capDateiverschluesselungInfo;
        private Caption capNeueZeilenInfo;
        private Caption capJoinTyp;
        private ComboBox cbxJoinTyp;
        private ComboBox cbxVerwaisteDaten;
        private Caption capVerwaisteDaten;
        private Caption capUndoAnzahl;
        private TextBox tbxUndoAnzahl;
        private TextBox txbGlobalScale;
        private Caption caption1;
        private TabControl GlobalTab;
        private TextBox txbFilterImagePath;
        private Caption capFilterImagePath;
        private TextBox txbZeilenQuickInfo;
        private Caption capZeilenQuickInfo;
        private Caption capBinInfo;
        private TabControl tabCSckript;
        private TabPage tabScriptAnzeige;
        private TextBox txtSkript;
        private TabPage tabVariablen;
        private TabPage tabBefehle;
        private GroupBox grpTextAllgemein;
        private TextBox txbSkriptInfo;
        private Button btnTest;
        private Button btnExtern;
        private System.Windows.Forms.Timer ExternTimer;
        private System.ComponentModel.IContainer components;
        private TextBox txbComms;
        private TextBox txbTestZeile;
        private Caption capTestZeile;
        private Table tableVariablen;
        private Filterleiste filterVariablen;
        private TextBox txbAdditionalFiles;
        private Caption capAdditional;
    }
}
