using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueControls.Classes_Editor;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;

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
            this.RuleItemEditor = new BlueControls.Classes_Editor.RuleItem_Editor();
            this.lbxRuleSelector = new BlueControls.Controls.ListBox();
            this.Caption14 = new BlueControls.Controls.Caption();
            this.Caption13 = new BlueControls.Controls.Caption();
            this.Caption11 = new BlueControls.Controls.Caption();
            this.GlobalTab = new BlueControls.Controls.TabControl();
            this.Tab_Allgemein = new BlueControls.Controls.TabPage();
            this.cbxVerwaisteDaten = new BlueControls.Controls.ComboBox();
            this.capVerwaisteDaten = new BlueControls.Controls.Caption();
            this.btnSpaltenuebersicht = new BlueControls.Controls.Button();
            this.tbxReloadVerzoegerung = new BlueControls.Controls.TextBox();
            this.capJoinTyp = new BlueControls.Controls.Caption();
            this.capReloadVerzoegerung = new BlueControls.Controls.Caption();
            this.cbxAnsicht = new BlueControls.Controls.ComboBox();
            this.cbxJoinTyp = new BlueControls.Controls.ComboBox();
            this.cbxBevorzugtesSkin = new BlueControls.Controls.ComboBox();
            this.capAnsicht = new BlueControls.Controls.Caption();
            this.capBevorzugtesSkin = new BlueControls.Controls.Caption();
            this.Tab_Rechte = new BlueControls.Controls.TabPage();
            this.grpDateiVerschluesselung = new BlueControls.Controls.GroupBox();
            this.capDateiverschluesselungInfo = new BlueControls.Controls.Caption();
            this.btnDateiSchluessel = new BlueControls.Controls.Button();
            this.Tab_Regeln = new BlueControls.Controls.TabPage();
            this.Tab_Binaer = new BlueControls.Controls.TabPage();
            this.lstBinary = new BlueControls.Controls.ListBox();
            this.Tab_Backup = new BlueControls.Controls.TabPage();
            this.ExportEditor = new BlueControls.Classes_Editor.ExportDefinition_Editor();
            this.grpExport = new BlueControls.Controls.GroupBox();
            this.ExportSets = new BlueControls.Controls.ListBox();
            this.Tab_Sortierung = new BlueControls.Controls.TabPage();
            this.Tab_Undo = new BlueControls.Controls.TabPage();
            this.tbxUndoAnzahl = new BlueControls.Controls.TextBox();
            this.capUndoAnzahl = new BlueControls.Controls.Caption();
            this.tblUndo = new BlueControls.Controls.Table();
            this.Tab_Expermimentell = new BlueControls.Controls.TabPage();
            this.btnFremdImport = new BlueControls.Controls.Button();
            this.capExperimentellWarnung = new BlueControls.Controls.Caption();
            this.grpBenutzergruppen.SuspendLayout();
            this.grpKennwort.SuspendLayout();
            this.GlobalTab.SuspendLayout();
            this.Tab_Allgemein.SuspendLayout();
            this.Tab_Rechte.SuspendLayout();
            this.grpDateiVerschluesselung.SuspendLayout();
            this.Tab_Regeln.SuspendLayout();
            this.Tab_Binaer.SuspendLayout();
            this.Tab_Backup.SuspendLayout();
            this.grpExport.SuspendLayout();
            this.Tab_Sortierung.SuspendLayout();
            this.Tab_Undo.SuspendLayout();
            this.Tab_Expermimentell.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpBenutzergruppen
            // 
            this.grpBenutzergruppen.CausesValidation = false;
            this.grpBenutzergruppen.Controls.Add(this.PermissionGroups_NewRow);
            this.grpBenutzergruppen.Controls.Add(this.capNeueZeilenInfo);
            this.grpBenutzergruppen.Controls.Add(this.Caption22);
            this.grpBenutzergruppen.Controls.Add(this.DatenbankAdmin);
            this.grpBenutzergruppen.Controls.Add(this.capNeueZeilen);
            this.grpBenutzergruppen.Location = new System.Drawing.Point(16, 16);
            this.grpBenutzergruppen.Name = "grpBenutzergruppen";
            this.grpBenutzergruppen.Size = new System.Drawing.Size(376, 488);
            this.grpBenutzergruppen.Text = "Benutzergruppen:";
            // 
            // PermissionGroups_NewRow
            // 
            this.PermissionGroups_NewRow.AddAllowed = BlueControls.Enums.enAddType.Text;
            this.PermissionGroups_NewRow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.PermissionGroups_NewRow.CheckBehavior = BlueControls.Enums.enCheckBehavior.MultiSelection;
            this.PermissionGroups_NewRow.FilterAllowed = true;
            this.PermissionGroups_NewRow.Location = new System.Drawing.Point(192, 48);
            this.PermissionGroups_NewRow.Name = "PermissionGroups_NewRow";
            this.PermissionGroups_NewRow.QuickInfo = "";
            this.PermissionGroups_NewRow.RemoveAllowed = true;
            this.PermissionGroups_NewRow.Size = new System.Drawing.Size(176, 360);
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
            this.DatenbankAdmin.Location = new System.Drawing.Point(8, 46);
            this.DatenbankAdmin.Name = "DatenbankAdmin";
            this.DatenbankAdmin.QuickInfo = "";
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
            this.grpKennwort.CausesValidation = false;
            this.grpKennwort.Controls.Add(this.capKennwort);
            this.grpKennwort.Controls.Add(this.txbKennwort);
            this.grpKennwort.Location = new System.Drawing.Point(400, 16);
            this.grpKennwort.Name = "grpKennwort";
            this.grpKennwort.Size = new System.Drawing.Size(232, 96);
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
            this.txbKennwort.Format = BlueBasics.Enums.enDataFormat.Text_Ohne_Kritische_Zeichen;
            this.txbKennwort.Location = new System.Drawing.Point(8, 56);
            this.txbKennwort.Name = "txbKennwort";
            this.txbKennwort.Size = new System.Drawing.Size(216, 22);
            this.txbKennwort.TabIndex = 4;
            // 
            // lbxSortierSpalten
            // 
            this.lbxSortierSpalten.AddAllowed = BlueControls.Enums.enAddType.OnlySuggests;
            this.lbxSortierSpalten.CheckBehavior = BlueControls.Enums.enCheckBehavior.MultiSelection;
            this.lbxSortierSpalten.Location = new System.Drawing.Point(16, 32);
            this.lbxSortierSpalten.MoveAllowed = true;
            this.lbxSortierSpalten.Name = "lbxSortierSpalten";
            this.lbxSortierSpalten.QuickInfo = "";
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
            this.tbxTags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxTags.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxTags.Location = new System.Drawing.Point(615, 24);
            this.tbxTags.MultiLine = true;
            this.tbxTags.Name = "tbxTags";
            this.tbxTags.Size = new System.Drawing.Size(416, 581);
            this.tbxTags.TabIndex = 26;
            this.tbxTags.Verhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbCaption
            // 
            this.txbCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbCaption.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbCaption.Location = new System.Drawing.Point(8, 24);
            this.txbCaption.Name = "txbCaption";
            this.txbCaption.Size = new System.Drawing.Size(600, 24);
            this.txbCaption.TabIndex = 24;
            // 
            // capCaption
            // 
            this.capCaption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.capCaption.CausesValidation = false;
            this.capCaption.Location = new System.Drawing.Point(8, 8);
            this.capCaption.Name = "capCaption";
            this.capCaption.Size = new System.Drawing.Size(137, 16);
            this.capCaption.Text = "Überschrift bzw. Titel:";
            this.capCaption.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capTags
            // 
            this.capTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.capTags.CausesValidation = false;
            this.capTags.Location = new System.Drawing.Point(616, 8);
            this.capTags.Name = "capTags";
            this.capTags.QuickInfo = "Tags / Eigenschaften, die von einem ";
            this.capTags.Size = new System.Drawing.Size(152, 16);
            this.capTags.Text = "Tags:";
            this.capTags.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capInfo
            // 
            this.capInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.capInfo.CausesValidation = false;
            this.capInfo.Location = new System.Drawing.Point(8, 56);
            this.capInfo.Name = "capInfo";
            this.capInfo.Size = new System.Drawing.Size(599, 90);
            this.capInfo.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // RuleItemEditor
            // 
            this.RuleItemEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RuleItemEditor.CausesValidation = false;
            this.RuleItemEditor.IsDialog = false;
            this.RuleItemEditor.Location = new System.Drawing.Point(8, 384);
            this.RuleItemEditor.Name = "RuleItemEditor";
            this.RuleItemEditor.Size = new System.Drawing.Size(1023, 221);
            this.RuleItemEditor.Text = "Regel-Editor";
            // 
            // lbxRuleSelector
            // 
            this.lbxRuleSelector.AddAllowed = BlueControls.Enums.enAddType.UserDef;
            this.lbxRuleSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbxRuleSelector.CheckBehavior = BlueControls.Enums.enCheckBehavior.AlwaysSingleSelection;
            this.lbxRuleSelector.FilterAllowed = true;
            this.lbxRuleSelector.Location = new System.Drawing.Point(8, 8);
            this.lbxRuleSelector.Name = "lbxRuleSelector";
            this.lbxRuleSelector.QuickInfo = "";
            this.lbxRuleSelector.RemoveAllowed = true;
            this.lbxRuleSelector.Size = new System.Drawing.Size(1023, 360);
            this.lbxRuleSelector.TabIndex = 1;
            this.lbxRuleSelector.ContextMenuInit += new System.EventHandler<BlueControls.EventArgs.ContextMenuInitEventArgs>(this.lbxRuleSelector_ContextMenuInit);
            this.lbxRuleSelector.ContextMenuItemClicked += new System.EventHandler<BlueControls.EventArgs.ContextMenuItemClickedEventArgs>(this.lbxRuleSelector_ContextMenuItemClicked);
            this.lbxRuleSelector.ItemCheckedChanged += new System.EventHandler(this.lbxRuleSelector_ItemCheckedChanged);
            this.lbxRuleSelector.AddClicked += new System.EventHandler(this.lbxRuleSelector_AddClicked);
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
            this.GlobalTab.Controls.Add(this.Tab_Rechte);
            this.GlobalTab.Controls.Add(this.Tab_Regeln);
            this.GlobalTab.Controls.Add(this.Tab_Binaer);
            this.GlobalTab.Controls.Add(this.Tab_Backup);
            this.GlobalTab.Controls.Add(this.Tab_Sortierung);
            this.GlobalTab.Controls.Add(this.Tab_Undo);
            this.GlobalTab.Controls.Add(this.Tab_Expermimentell);
            this.GlobalTab.HotTrack = true;
            this.GlobalTab.Location = new System.Drawing.Point(0, 0);
            this.GlobalTab.Name = "GlobalTab";
            this.GlobalTab.Size = new System.Drawing.Size(1047, 645);
            this.GlobalTab.TabIndex = 21;
            // 
            // Tab_Allgemein
            // 
            this.Tab_Allgemein.Controls.Add(this.cbxVerwaisteDaten);
            this.Tab_Allgemein.Controls.Add(this.capVerwaisteDaten);
            this.Tab_Allgemein.Controls.Add(this.tbxTags);
            this.Tab_Allgemein.Controls.Add(this.btnSpaltenuebersicht);
            this.Tab_Allgemein.Controls.Add(this.tbxReloadVerzoegerung);
            this.Tab_Allgemein.Controls.Add(this.capJoinTyp);
            this.Tab_Allgemein.Controls.Add(this.capReloadVerzoegerung);
            this.Tab_Allgemein.Controls.Add(this.cbxAnsicht);
            this.Tab_Allgemein.Controls.Add(this.cbxJoinTyp);
            this.Tab_Allgemein.Controls.Add(this.cbxBevorzugtesSkin);
            this.Tab_Allgemein.Controls.Add(this.capAnsicht);
            this.Tab_Allgemein.Controls.Add(this.capBevorzugtesSkin);
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
            this.Tab_Allgemein.UseVisualStyleBackColor = true;
            // 
            // cbxVerwaisteDaten
            // 
            this.cbxVerwaisteDaten.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbxVerwaisteDaten.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxVerwaisteDaten.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxVerwaisteDaten.Format = BlueBasics.Enums.enDataFormat.Text_Ohne_Kritische_Zeichen;
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
            this.tbxReloadVerzoegerung.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxReloadVerzoegerung.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxReloadVerzoegerung.Format = BlueBasics.Enums.enDataFormat.Ganzzahl;
            this.tbxReloadVerzoegerung.Location = new System.Drawing.Point(168, 510);
            this.tbxReloadVerzoegerung.Name = "tbxReloadVerzoegerung";
            this.tbxReloadVerzoegerung.Size = new System.Drawing.Size(160, 24);
            this.tbxReloadVerzoegerung.Suffix = "Sek.";
            this.tbxReloadVerzoegerung.TabIndex = 34;
            // 
            // capJoinTyp
            // 
            this.capJoinTyp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.capJoinTyp.CausesValidation = false;
            this.capJoinTyp.Location = new System.Drawing.Point(8, 488);
            this.capJoinTyp.Name = "capJoinTyp";
            this.capJoinTyp.Size = new System.Drawing.Size(152, 18);
            this.capJoinTyp.Text = "Dopplte Zeilen Verhalten:";
            // 
            // capReloadVerzoegerung
            // 
            this.capReloadVerzoegerung.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.capReloadVerzoegerung.CausesValidation = false;
            this.capReloadVerzoegerung.Location = new System.Drawing.Point(8, 512);
            this.capReloadVerzoegerung.Name = "capReloadVerzoegerung";
            this.capReloadVerzoegerung.Size = new System.Drawing.Size(136, 18);
            this.capReloadVerzoegerung.Text = "Reload-Verzögerung:";
            // 
            // cbxAnsicht
            // 
            this.cbxAnsicht.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbxAnsicht.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxAnsicht.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxAnsicht.Format = BlueBasics.Enums.enDataFormat.Text_Ohne_Kritische_Zeichen;
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
            this.cbxJoinTyp.Format = BlueBasics.Enums.enDataFormat.Text_Ohne_Kritische_Zeichen;
            this.cbxJoinTyp.Location = new System.Drawing.Point(168, 488);
            this.cbxJoinTyp.Name = "cbxJoinTyp";
            this.cbxJoinTyp.Size = new System.Drawing.Size(160, 24);
            this.cbxJoinTyp.TabIndex = 31;
            // 
            // cbxBevorzugtesSkin
            // 
            this.cbxBevorzugtesSkin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbxBevorzugtesSkin.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxBevorzugtesSkin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxBevorzugtesSkin.Format = BlueBasics.Enums.enDataFormat.Text_Ohne_Kritische_Zeichen;
            this.cbxBevorzugtesSkin.Location = new System.Drawing.Point(168, 534);
            this.cbxBevorzugtesSkin.Name = "cbxBevorzugtesSkin";
            this.cbxBevorzugtesSkin.Size = new System.Drawing.Size(160, 24);
            this.cbxBevorzugtesSkin.TabIndex = 31;
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
            // capBevorzugtesSkin
            // 
            this.capBevorzugtesSkin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.capBevorzugtesSkin.CausesValidation = false;
            this.capBevorzugtesSkin.Location = new System.Drawing.Point(8, 536);
            this.capBevorzugtesSkin.Name = "capBevorzugtesSkin";
            this.capBevorzugtesSkin.Size = new System.Drawing.Size(112, 18);
            this.capBevorzugtesSkin.Text = "Bevorzugtes Skin:";
            // 
            // Tab_Rechte
            // 
            this.Tab_Rechte.Controls.Add(this.grpDateiVerschluesselung);
            this.Tab_Rechte.Controls.Add(this.grpKennwort);
            this.Tab_Rechte.Controls.Add(this.grpBenutzergruppen);
            this.Tab_Rechte.Location = new System.Drawing.Point(4, 25);
            this.Tab_Rechte.Name = "Tab_Rechte";
            this.Tab_Rechte.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_Rechte.Size = new System.Drawing.Size(1039, 616);
            this.Tab_Rechte.TabIndex = 4;
            this.Tab_Rechte.Text = "Rechte";
            this.Tab_Rechte.UseVisualStyleBackColor = true;
            // 
            // grpDateiVerschluesselung
            // 
            this.grpDateiVerschluesselung.CausesValidation = false;
            this.grpDateiVerschluesselung.Controls.Add(this.capDateiverschluesselungInfo);
            this.grpDateiVerschluesselung.Controls.Add(this.btnDateiSchluessel);
            this.grpDateiVerschluesselung.Location = new System.Drawing.Point(400, 120);
            this.grpDateiVerschluesselung.Name = "grpDateiVerschluesselung";
            this.grpDateiVerschluesselung.Size = new System.Drawing.Size(232, 176);
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
            // Tab_Regeln
            // 
            this.Tab_Regeln.Controls.Add(this.RuleItemEditor);
            this.Tab_Regeln.Controls.Add(this.lbxRuleSelector);
            this.Tab_Regeln.Location = new System.Drawing.Point(4, 25);
            this.Tab_Regeln.Name = "Tab_Regeln";
            this.Tab_Regeln.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_Regeln.Size = new System.Drawing.Size(1039, 616);
            this.Tab_Regeln.TabIndex = 3;
            this.Tab_Regeln.Text = "Regeln";
            this.Tab_Regeln.UseVisualStyleBackColor = true;
            // 
            // Tab_Binaer
            // 
            this.Tab_Binaer.Controls.Add(this.lstBinary);
            this.Tab_Binaer.Location = new System.Drawing.Point(4, 25);
            this.Tab_Binaer.Name = "Tab_Binaer";
            this.Tab_Binaer.Size = new System.Drawing.Size(1039, 616);
            this.Tab_Binaer.TabIndex = 7;
            this.Tab_Binaer.Text = "Zusätzliche Binärdaten";
            // 
            // lstBinary
            // 
            this.lstBinary.AddAllowed = BlueControls.Enums.enAddType.Images;
            this.lstBinary.Appearance = BlueControls.Enums.enBlueListBoxAppearance.Gallery;
            this.lstBinary.CheckBehavior = BlueControls.Enums.enCheckBehavior.MultiSelection;
            this.lstBinary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstBinary.FilterAllowed = true;
            this.lstBinary.Location = new System.Drawing.Point(0, 0);
            this.lstBinary.Name = "lstBinary";
            this.lstBinary.QuickInfo = "";
            this.lstBinary.RemoveAllowed = true;
            this.lstBinary.Size = new System.Drawing.Size(1039, 616);
            this.lstBinary.TabIndex = 0;
            this.lstBinary.ContextMenuInit += new System.EventHandler<BlueControls.EventArgs.ContextMenuInitEventArgs>(this.Bilder_ContextMenuInit);
            this.lstBinary.ContextMenuItemClicked += new System.EventHandler<BlueControls.EventArgs.ContextMenuItemClickedEventArgs>(this.Bilder_ContextMenuItemClicked);
            // 
            // Tab_Backup
            // 
            this.Tab_Backup.Controls.Add(this.ExportEditor);
            this.Tab_Backup.Controls.Add(this.grpExport);
            this.Tab_Backup.Location = new System.Drawing.Point(4, 25);
            this.Tab_Backup.Name = "Tab_Backup";
            this.Tab_Backup.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_Backup.Size = new System.Drawing.Size(1039, 616);
            this.Tab_Backup.TabIndex = 5;
            this.Tab_Backup.Text = "Backup & Export";
            this.Tab_Backup.UseVisualStyleBackColor = true;
            // 
            // ExportEditor
            // 
            this.ExportEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportEditor.CausesValidation = false;
            this.ExportEditor.Enabled = false;
            this.ExportEditor.IsDialog = false;
            this.ExportEditor.Location = new System.Drawing.Point(8, 176);
            this.ExportEditor.Name = "ExportEditor";
            this.ExportEditor.Size = new System.Drawing.Size(1031, 437);
            this.ExportEditor.Text = "Export-Editor:";
            // 
            // grpExport
            // 
            this.grpExport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpExport.CausesValidation = false;
            this.grpExport.Controls.Add(this.ExportSets);
            this.grpExport.Location = new System.Drawing.Point(8, 8);
            this.grpExport.Name = "grpExport";
            this.grpExport.Size = new System.Drawing.Size(1031, 168);
            this.grpExport.Text = "Alle Export-Aufgaben:";
            // 
            // ExportSets
            // 
            this.ExportSets.AddAllowed = BlueControls.Enums.enAddType.UserDef;
            this.ExportSets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportSets.FilterAllowed = true;
            this.ExportSets.Location = new System.Drawing.Point(8, 24);
            this.ExportSets.Name = "ExportSets";
            this.ExportSets.QuickInfo = "";
            this.ExportSets.RemoveAllowed = true;
            this.ExportSets.Size = new System.Drawing.Size(1015, 136);
            this.ExportSets.TabIndex = 0;
            this.ExportSets.Text = "ExportAufgaben";
            this.ExportSets.ItemCheckedChanged += new System.EventHandler(this.ExportSets_Item_CheckedChanged);
            this.ExportSets.AddClicked += new System.EventHandler(this.ExportSets_AddClicked);
            this.ExportSets.RemoveClicked += new System.EventHandler<BlueControls.EventArgs.ListOfBasicListItemEventArgs>(this.ExportSets_RemoveClicked);
            // 
            // Tab_Sortierung
            // 
            this.Tab_Sortierung.Controls.Add(this.lbxSortierSpalten);
            this.Tab_Sortierung.Controls.Add(this.capSortierspalten);
            this.Tab_Sortierung.Controls.Add(this.btnSortRichtung);
            this.Tab_Sortierung.Location = new System.Drawing.Point(4, 25);
            this.Tab_Sortierung.Name = "Tab_Sortierung";
            this.Tab_Sortierung.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_Sortierung.Size = new System.Drawing.Size(1039, 616);
            this.Tab_Sortierung.TabIndex = 2;
            this.Tab_Sortierung.Text = "Sortierung";
            this.Tab_Sortierung.UseVisualStyleBackColor = true;
            // 
            // Tab_Undo
            // 
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
            this.btnFremdImport.Click += new System.EventHandler(this.FremdImport_Click);
            // 
            // capExperimentellWarnung
            // 
            this.capExperimentellWarnung.CausesValidation = false;
            this.capExperimentellWarnung.Location = new System.Drawing.Point(16, 16);
            this.capExperimentellWarnung.Name = "capExperimentellWarnung";
            this.capExperimentellWarnung.Size = new System.Drawing.Size(488, 80);
            this.capExperimentellWarnung.Text = resources.GetString("capExperimentellWarnung.Text");
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
            this.Tab_Rechte.ResumeLayout(false);
            this.grpDateiVerschluesselung.ResumeLayout(false);
            this.Tab_Regeln.ResumeLayout(false);
            this.Tab_Binaer.ResumeLayout(false);
            this.Tab_Backup.ResumeLayout(false);
            this.grpExport.ResumeLayout(false);
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
        private ListBox lbxRuleSelector;
        private Caption Caption11;
        private Caption Caption14;
        private Caption Caption13;
        private Caption Caption22;
        private ListBox DatenbankAdmin;
        private TabControl GlobalTab;
        private TabPage Tab_Allgemein;
        private TabPage Tab_Sortierung;
        private TabPage Tab_Regeln;
        private TabPage Tab_Rechte;
        private TabPage Tab_Backup;
        private ComboBox cbxAnsicht;
        private ComboBox cbxBevorzugtesSkin;
        private Caption capAnsicht;
        private Caption capBevorzugtesSkin;
        private GroupBox grpKennwort;
        private RuleItem_Editor RuleItemEditor;
        private GroupBox grpBenutzergruppen;
        private TabPage Tab_Undo;
        private TabPage Tab_Binaer;
        private ListBox lstBinary;
        private ListBox ExportSets;
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
    }
}
