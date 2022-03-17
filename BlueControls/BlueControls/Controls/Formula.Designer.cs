using System.Diagnostics;

namespace BlueControls.Controls
{
    public partial class Formula
    {
        // Inherits Windows.Forms.UserControl
        //Wird vom Windows Form-Designer benötigt.
        //private IContainer components;
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.Tabs = new BlueControls.Controls.TabControl();
            this.grpEditor = new BlueControls.Controls.GroupBox();
            this.capBearbeitungsMethode = new BlueControls.Controls.Caption();
            this.btnReiterNachRechts = new BlueControls.Controls.Button();
            this.btnReiterNachLinks = new BlueControls.Controls.Button();
            this.lbxColumns = new BlueControls.Controls.ListBox();
            this.capUeberschrift = new BlueControls.Controls.Caption();
            this.BlueLine2 = new BlueControls.Controls.Line();
            this.cbxControlType = new BlueControls.Controls.ComboBox();
            this.cbxCaptionPosition = new BlueControls.Controls.ComboBox();
            this.BlueLine1 = new BlueControls.Controls.Line();
            this.grpGroesse = new BlueControls.Controls.GroupBox();
            this.btnGroesseRechts = new BlueControls.Controls.Button();
            this.btnGroesseOben = new BlueControls.Controls.Button();
            this.btnGroesseLinks = new BlueControls.Controls.Button();
            this.btnGroesseUnten = new BlueControls.Controls.Button();
            this.grpPosition = new BlueControls.Controls.GroupBox();
            this.btnPositionRechts = new BlueControls.Controls.Button();
            this.btnPositionOben = new BlueControls.Controls.Button();
            this.btnPositionLinks = new BlueControls.Controls.Button();
            this.btnPositionUnten = new BlueControls.Controls.Button();
            this.btnRename = new BlueControls.Controls.Button();
            this.btnExitEditor = new BlueControls.Controls.Button();
            this.btnRechteFuerAnsicht = new BlueControls.Controls.Button();
            this.btnAnsichtloeschen = new BlueControls.Controls.Button();
            this.btnAnsichtHinzufuegen = new BlueControls.Controls.Button();
            this.ColDiax = new System.Windows.Forms.ColorDialog();
            this.grpEditor.SuspendLayout();
            this.grpGroesse.SuspendLayout();
            this.grpPosition.SuspendLayout();
            this.SuspendLayout();
            // 
            // Tabs
            // 
            this.Tabs.Location = new System.Drawing.Point(8, 232);
            this.Tabs.Name = "Tabs";
            this.Tabs.SelectedIndex = -1;
            this.Tabs.Size = new System.Drawing.Size(128, 112);
            this.Tabs.TabIndex = 2;
            this.Tabs.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Tabs_MouseUp);
            // 
            // grpEditor
            // 
            this.grpEditor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpEditor.CausesValidation = false;
            this.grpEditor.Controls.Add(this.capBearbeitungsMethode);
            this.grpEditor.Controls.Add(this.btnReiterNachRechts);
            this.grpEditor.Controls.Add(this.btnReiterNachLinks);
            this.grpEditor.Controls.Add(this.lbxColumns);
            this.grpEditor.Controls.Add(this.capUeberschrift);
            this.grpEditor.Controls.Add(this.BlueLine2);
            this.grpEditor.Controls.Add(this.cbxControlType);
            this.grpEditor.Controls.Add(this.cbxCaptionPosition);
            this.grpEditor.Controls.Add(this.BlueLine1);
            this.grpEditor.Controls.Add(this.grpGroesse);
            this.grpEditor.Controls.Add(this.grpPosition);
            this.grpEditor.Controls.Add(this.btnRename);
            this.grpEditor.Controls.Add(this.btnExitEditor);
            this.grpEditor.Controls.Add(this.btnRechteFuerAnsicht);
            this.grpEditor.Controls.Add(this.btnAnsichtloeschen);
            this.grpEditor.Controls.Add(this.btnAnsichtHinzufuegen);
            this.grpEditor.Location = new System.Drawing.Point(144, 0);
            this.grpEditor.Name = "grpEditor";
            this.grpEditor.Size = new System.Drawing.Size(176, 347);
            this.grpEditor.Text = "Formular-Editor:";
            this.grpEditor.Visible = false;
            // 
            // capBearbeitungsMethode
            // 
            this.capBearbeitungsMethode.Location = new System.Drawing.Point(8, 272);
            this.capBearbeitungsMethode.Name = "capBearbeitungsMethode";
            this.capBearbeitungsMethode.Size = new System.Drawing.Size(130, 16);
            this.capBearbeitungsMethode.Text = "Bearbeitungs-Methode:";
            // 
            // btnReiterNachRechts
            // 
            this.btnReiterNachRechts.ImageCode = "Pfeil_Rechts|16";
            this.btnReiterNachRechts.Location = new System.Drawing.Point(144, 24);
            this.btnReiterNachRechts.Name = "btnReiterNachRechts";
            this.btnReiterNachRechts.QuickInfo = "Reiter-Anordnung ändern";
            this.btnReiterNachRechts.Size = new System.Drawing.Size(24, 24);
            this.btnReiterNachRechts.TabIndex = 41;
            this.btnReiterNachRechts.Click += new System.EventHandler(this.btnReiterNachRechts_Click);
            // 
            // btnReiterNachLinks
            // 
            this.btnReiterNachLinks.ImageCode = "Pfeil_Links|16";
            this.btnReiterNachLinks.Location = new System.Drawing.Point(120, 24);
            this.btnReiterNachLinks.Name = "Li";
            this.btnReiterNachLinks.QuickInfo = "Reiter-Anordnung ändern";
            this.btnReiterNachLinks.Size = new System.Drawing.Size(24, 24);
            this.btnReiterNachLinks.TabIndex = 40;
            this.btnReiterNachLinks.Click += new System.EventHandler(this.btnReiterNachLinks_Click);
            // 
            // lbxColumns
            // 
            this.lbxColumns.AddAllowed = BlueControls.Enums.AddType.None;
            this.lbxColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbxColumns.CheckBehavior = BlueControls.Enums.CheckBehavior.AlwaysSingleSelection;
            this.lbxColumns.Location = new System.Drawing.Point(8, 328);
            this.lbxColumns.Name = "lbxColumns";
            this.lbxColumns.QuickInfo = "";
            this.lbxColumns.Size = new System.Drawing.Size(160, 11);
            this.lbxColumns.TabIndex = 39;
            this.lbxColumns.ContextMenuInit += new System.EventHandler<BlueControls.EventArgs.ContextMenuInitEventArgs>(this.lbxColumns_ContextMenuInit);
            this.lbxColumns.ContextMenuItemClicked += new System.EventHandler<BlueControls.EventArgs.ContextMenuItemClickedEventArgs>(this.lbxColumns_ContextMenuItemClicked);
            this.lbxColumns.ItemCheckedChanged += new System.EventHandler(this.lbxColumns_ItemCheckedChanged);
            // 
            // capUeberschrift
            // 
            this.capUeberschrift.Location = new System.Drawing.Point(8, 224);
            this.capUeberschrift.Name = "Caption1";
            this.capUeberschrift.Size = new System.Drawing.Size(64, 16);
            this.capUeberschrift.Text = "Überschrift:";
            // 
            // BlueLine2
            // 
            this.BlueLine2.Location = new System.Drawing.Point(8, 320);
            this.BlueLine2.Name = "BlueLine2";
            this.BlueLine2.Size = new System.Drawing.Size(160, 2);
            this.BlueLine2.Text = "BlueLine2";
            // 
            // cbxControlType
            // 
            this.cbxControlType.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxControlType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxControlType.Enabled = false;
            this.cbxControlType.Location = new System.Drawing.Point(8, 288);
            this.cbxControlType.Name = "cbxControlType";
            this.cbxControlType.Size = new System.Drawing.Size(160, 24);
            this.cbxControlType.TabIndex = 37;
            this.cbxControlType.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.cbxControlType_ItemClicked);
            // 
            // cbxCaptionPosition
            // 
            this.cbxCaptionPosition.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxCaptionPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxCaptionPosition.Enabled = false;
            this.cbxCaptionPosition.Location = new System.Drawing.Point(8, 240);
            this.cbxCaptionPosition.Name = "cbxCaptionPosition";
            this.cbxCaptionPosition.Size = new System.Drawing.Size(160, 24);
            this.cbxCaptionPosition.TabIndex = 37;
            this.cbxCaptionPosition.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.cbxCaptionPosition_ItemClicked);
            // 
            // BlueLine1
            // 
            this.BlueLine1.Location = new System.Drawing.Point(8, 88);
            this.BlueLine1.Name = "BlueLine1";
            this.BlueLine1.Size = new System.Drawing.Size(160, 2);
            this.BlueLine1.Text = "BlueLine1";
            // 
            // grpGroesse
            // 
            this.grpGroesse.CausesValidation = false;
            this.grpGroesse.Controls.Add(this.btnGroesseRechts);
            this.grpGroesse.Controls.Add(this.btnGroesseOben);
            this.grpGroesse.Controls.Add(this.btnGroesseLinks);
            this.grpGroesse.Controls.Add(this.btnGroesseUnten);
            this.grpGroesse.Enabled = false;
            this.grpGroesse.Location = new System.Drawing.Point(88, 96);
            this.grpGroesse.Name = "grpGroesse";
            this.grpGroesse.Size = new System.Drawing.Size(80, 120);
            this.grpGroesse.Text = "Ende";
            // 
            // btnGroesseRechts
            // 
            this.btnGroesseRechts.ImageCode = "Pfeil_Rechts|12|||0088FF";
            this.btnGroesseRechts.Location = new System.Drawing.Point(40, 48);
            this.btnGroesseRechts.Name = "btnGroesseRechts";
            this.btnGroesseRechts.Size = new System.Drawing.Size(32, 32);
            this.btnGroesseRechts.TabIndex = 5;
            this.btnGroesseRechts.Click += new System.EventHandler(this.btnGroesseRechts_Click);
            // 
            // btnGroesseOben
            // 
            this.btnGroesseOben.ImageCode = "Pfeil_Oben|16|||0088FF";
            this.btnGroesseOben.Location = new System.Drawing.Point(24, 16);
            this.btnGroesseOben.Name = "btnGroesseOben";
            this.btnGroesseOben.Size = new System.Drawing.Size(32, 32);
            this.btnGroesseOben.TabIndex = 37;
            this.btnGroesseOben.Click += new System.EventHandler(this.btnGroesseOben_Click);
            // 
            // btnGroesseLinks
            // 
            this.btnGroesseLinks.ImageCode = "Pfeil_Links|12|||0088FF";
            this.btnGroesseLinks.Location = new System.Drawing.Point(8, 48);
            this.btnGroesseLinks.Name = "btnGroesseLinks";
            this.btnGroesseLinks.Size = new System.Drawing.Size(32, 32);
            this.btnGroesseLinks.TabIndex = 4;
            this.btnGroesseLinks.Click += new System.EventHandler(this.btnGroesseLinks_Click);
            // 
            // btnGroesseUnten
            // 
            this.btnGroesseUnten.ImageCode = "Pfeil_Unten|16|||0088FF";
            this.btnGroesseUnten.Location = new System.Drawing.Point(24, 80);
            this.btnGroesseUnten.Name = "btnGroesseUnten";
            this.btnGroesseUnten.Size = new System.Drawing.Size(32, 32);
            this.btnGroesseUnten.TabIndex = 36;
            this.btnGroesseUnten.Click += new System.EventHandler(this.btnGroesseUnten_Click);
            // 
            // grpPosition
            // 
            this.grpPosition.CausesValidation = false;
            this.grpPosition.Controls.Add(this.btnPositionRechts);
            this.grpPosition.Controls.Add(this.btnPositionOben);
            this.grpPosition.Controls.Add(this.btnPositionLinks);
            this.grpPosition.Controls.Add(this.btnPositionUnten);
            this.grpPosition.Enabled = false;
            this.grpPosition.Location = new System.Drawing.Point(8, 96);
            this.grpPosition.Name = "grpPosition";
            this.grpPosition.Size = new System.Drawing.Size(80, 120);
            this.grpPosition.Text = "Start";
            // 
            // btnPositionRechts
            // 
            this.btnPositionRechts.ImageCode = "Pfeil_Rechts|12|||FF7700";
            this.btnPositionRechts.Location = new System.Drawing.Point(40, 48);
            this.btnPositionRechts.Name = "btnPositionRechts";
            this.btnPositionRechts.Size = new System.Drawing.Size(32, 32);
            this.btnPositionRechts.TabIndex = 5;
            this.btnPositionRechts.Click += new System.EventHandler(this.btnPositionRechts_Click);
            // 
            // btnPositionOben
            // 
            this.btnPositionOben.ImageCode = "Pfeil_Oben|16|||FF7700";
            this.btnPositionOben.Location = new System.Drawing.Point(24, 16);
            this.btnPositionOben.Name = "btnPositionOben";
            this.btnPositionOben.Size = new System.Drawing.Size(32, 32);
            this.btnPositionOben.TabIndex = 37;
            this.btnPositionOben.Click += new System.EventHandler(this.btnPositionOben_Click);
            // 
            // btnPositionLinks
            // 
            this.btnPositionLinks.ImageCode = "Pfeil_Links|12|||FF7700";
            this.btnPositionLinks.Location = new System.Drawing.Point(8, 48);
            this.btnPositionLinks.Name = "btnPositionLinks";
            this.btnPositionLinks.Size = new System.Drawing.Size(32, 32);
            this.btnPositionLinks.TabIndex = 4;
            this.btnPositionLinks.Click += new System.EventHandler(this.btnPositionLinks_Click);
            // 
            // btnPositionUnten
            // 
            this.btnPositionUnten.ImageCode = "Pfeil_Unten|16|||FF7700";
            this.btnPositionUnten.Location = new System.Drawing.Point(24, 80);
            this.btnPositionUnten.Name = "btnPositionUnten";
            this.btnPositionUnten.Size = new System.Drawing.Size(32, 32);
            this.btnPositionUnten.TabIndex = 36;
            this.btnPositionUnten.Click += new System.EventHandler(this.SUnten_Click);
            // 
            // Rename
            // 
            this.btnRename.ImageCode = "Stift|16";
            this.btnRename.Location = new System.Drawing.Point(80, 24);
            this.btnRename.Name = "Rename";
            this.btnRename.QuickInfo = "Reiter-Beschriftung der<br>aktuellen Ansicht ändern";
            this.btnRename.Size = new System.Drawing.Size(32, 24);
            this.btnRename.TabIndex = 35;
            this.btnRename.Click += new System.EventHandler(this.btnRename_Click);
            // 
            // btnExitEditor
            // 
            this.btnExitEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExitEditor.ImageCode = "Kreuz|26";
            this.btnExitEditor.Location = new System.Drawing.Point(152, 0);
            this.btnExitEditor.Name = "btnExitEditor";
            this.btnExitEditor.QuickInfo = "Formular-Editor beenden";
            this.btnExitEditor.Size = new System.Drawing.Size(24, 24);
            this.btnExitEditor.TabIndex = 34;
            this.btnExitEditor.Click += new System.EventHandler(this.btnExitEditor_Click);
            // 
            // btnRechteFuerAnsicht
            // 
            this.btnRechteFuerAnsicht.ImageCode = "Schild|16";
            this.btnRechteFuerAnsicht.Location = new System.Drawing.Point(8, 56);
            this.btnRechteFuerAnsicht.Name = "btnRechteFuerAnsicht";
            this.btnRechteFuerAnsicht.QuickInfo = "Berechtigungsgruppen für<br>aktuelle Ansicht bearbeiten";
            this.btnRechteFuerAnsicht.Size = new System.Drawing.Size(160, 24);
            this.btnRechteFuerAnsicht.TabIndex = 32;
            this.btnRechteFuerAnsicht.Text = "Berechtigungsgruppen";
            this.btnRechteFuerAnsicht.Click += new System.EventHandler(this.btnRechteFuerAnsicht_Click);
            // 
            // btnAnsichtloeschen
            // 
            this.btnAnsichtloeschen.ImageCode = "MinusZeichen|16";
            this.btnAnsichtloeschen.Location = new System.Drawing.Point(40, 24);
            this.btnAnsichtloeschen.Name = "btnAnsichtloeschen";
            this.btnAnsichtloeschen.QuickInfo = "Aktuelle Ansicht<br>löschen";
            this.btnAnsichtloeschen.Size = new System.Drawing.Size(32, 24);
            this.btnAnsichtloeschen.TabIndex = 6;
            this.btnAnsichtloeschen.Click += new System.EventHandler(this.btnAnsichtloeschen_Click);
            // 
            // btnAnsichtHinzufuegen
            // 
            this.btnAnsichtHinzufuegen.ImageCode = "PlusZeichen|16";
            this.btnAnsichtHinzufuegen.Location = new System.Drawing.Point(8, 24);
            this.btnAnsichtHinzufuegen.Name = "btnAnsichtHinzufuegen";
            this.btnAnsichtHinzufuegen.QuickInfo = "Neue Ansicht<br>erstellen";
            this.btnAnsichtHinzufuegen.Size = new System.Drawing.Size(32, 24);
            this.btnAnsichtHinzufuegen.TabIndex = 2;
            this.btnAnsichtHinzufuegen.Click += new System.EventHandler(this.btnAnsichtHinzufuegen_Click);
            // 
            // Formula
            // 
            this.Controls.Add(this.Tabs);
            this.Controls.Add(this.grpEditor);
            this.MinimumSize = new System.Drawing.Size(320, 350);
            this.Name = "Formula";
            this.Size = new System.Drawing.Size(320, 350);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Tabs_MouseUp);
            this.grpEditor.ResumeLayout(false);
            this.grpGroesse.ResumeLayout(false);
            this.grpPosition.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        private GroupBox grpEditor;
        private Button btnPositionUnten;
        private Button btnExitEditor;
        private Button btnRechteFuerAnsicht;
        private Button btnAnsichtloeschen;
        private Button btnPositionRechts;
        private Button btnPositionLinks;
        private Button btnAnsichtHinzufuegen;
        private Button btnPositionOben;
        private GroupBox grpGroesse;
        private Button btnGroesseRechts;
        private Button btnGroesseOben;
        private Button btnGroesseLinks;
        private Button btnGroesseUnten;
        private GroupBox grpPosition;
        private Line BlueLine1;
        private ListBox lbxColumns;
        private Caption capUeberschrift;
        private Line BlueLine2;
        private ComboBox cbxCaptionPosition;
        private Button btnRename;
        private Button btnReiterNachRechts;
        private Button btnReiterNachLinks;
        private Caption capBearbeitungsMethode;
        private ComboBox cbxControlType;
        private TabControl Tabs;
        private System.Windows.Forms.ColorDialog ColDiax;
    }
}