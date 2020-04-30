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
            this.Editor = new BlueControls.Controls.GroupBox();
            this.Caption2 = new BlueControls.Controls.Caption();
            this.Re = new BlueControls.Controls.Button();
            this.Li = new BlueControls.Controls.Button();
            this.lbxColumns = new BlueControls.Controls.ListBox();
            this.capUeberschrift = new BlueControls.Controls.Caption();
            this.BlueLine2 = new BlueControls.Controls.Line();
            this.cbxControlType = new BlueControls.Controls.ComboBox();
            this.cbxCaptionPosition = new BlueControls.Controls.ComboBox();
            this.BlueLine1 = new BlueControls.Controls.Line();
            this.x2 = new BlueControls.Controls.GroupBox();
            this.ERechts = new BlueControls.Controls.Button();
            this.EOben = new BlueControls.Controls.Button();
            this.ELinks = new BlueControls.Controls.Button();
            this.EUnten = new BlueControls.Controls.Button();
            this.x1 = new BlueControls.Controls.GroupBox();
            this.SRechts = new BlueControls.Controls.Button();
            this.SOben = new BlueControls.Controls.Button();
            this.SLinks = new BlueControls.Controls.Button();
            this.SUnten = new BlueControls.Controls.Button();
            this.Rename = new BlueControls.Controls.Button();
            this.SpaltBEnde = new BlueControls.Controls.Button();
            this.Rechtex = new BlueControls.Controls.Button();
            this.OrderDelete = new BlueControls.Controls.Button();
            this.OrderAdd = new BlueControls.Controls.Button();
            this.ColDiax = new System.Windows.Forms.ColorDialog();
            this.Editor.SuspendLayout();
            this.x2.SuspendLayout();
            this.x1.SuspendLayout();
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
            // Editor
            // 
            this.Editor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Editor.CausesValidation = false;
            this.Editor.Controls.Add(this.Caption2);
            this.Editor.Controls.Add(this.Re);
            this.Editor.Controls.Add(this.Li);
            this.Editor.Controls.Add(this.lbxColumns);
            this.Editor.Controls.Add(this.capUeberschrift);
            this.Editor.Controls.Add(this.BlueLine2);
            this.Editor.Controls.Add(this.cbxControlType);
            this.Editor.Controls.Add(this.cbxCaptionPosition);
            this.Editor.Controls.Add(this.BlueLine1);
            this.Editor.Controls.Add(this.x2);
            this.Editor.Controls.Add(this.x1);
            this.Editor.Controls.Add(this.Rename);
            this.Editor.Controls.Add(this.SpaltBEnde);
            this.Editor.Controls.Add(this.Rechtex);
            this.Editor.Controls.Add(this.OrderDelete);
            this.Editor.Controls.Add(this.OrderAdd);
            this.Editor.Location = new System.Drawing.Point(144, 0);
            this.Editor.Name = "Editor";
            this.Editor.Size = new System.Drawing.Size(176, 347);
            this.Editor.Text = "Formular-Editor:";
            this.Editor.Visible = false;
            // 
            // Caption2
            // 
            this.Caption2.Location = new System.Drawing.Point(8, 272);
            this.Caption2.Name = "Caption2";
            this.Caption2.Size = new System.Drawing.Size(130, 16);
            this.Caption2.Text = "Bearbeitungs-Methode:";
            // 
            // Re
            // 
            this.Re.ImageCode = "Pfeil_Rechts|16";
            this.Re.Location = new System.Drawing.Point(144, 24);
            this.Re.Name = "Re";
            this.Re.QuickInfo = "Reiter-Anordnung ändern";
            this.Re.Size = new System.Drawing.Size(24, 24);
            this.Re.TabIndex = 41;
            this.Re.Click += new System.EventHandler(this.Re_Click);
            // 
            // Li
            // 
            this.Li.ImageCode = "Pfeil_Links|16";
            this.Li.Location = new System.Drawing.Point(120, 24);
            this.Li.Name = "Li";
            this.Li.QuickInfo = "Reiter-Anordnung ändern";
            this.Li.Size = new System.Drawing.Size(24, 24);
            this.Li.TabIndex = 40;
            this.Li.Click += new System.EventHandler(this.Li_Click);
            // 
            // lbxColumns
            // 
            this.lbxColumns.AddAllowed = BlueControls.Enums.enAddType.None;
            this.lbxColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbxColumns.CheckBehavior = BlueControls.Enums.enCheckBehavior.AlwaysSingleSelection;
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
            this.cbxControlType.Format = BlueBasics.Enums.enDataFormat.Text;
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
            this.cbxCaptionPosition.Format = BlueBasics.Enums.enDataFormat.Text;
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
            // x2
            // 
            this.x2.CausesValidation = false;
            this.x2.Controls.Add(this.ERechts);
            this.x2.Controls.Add(this.EOben);
            this.x2.Controls.Add(this.ELinks);
            this.x2.Controls.Add(this.EUnten);
            this.x2.Enabled = false;
            this.x2.Location = new System.Drawing.Point(88, 96);
            this.x2.Name = "x2";
            this.x2.Size = new System.Drawing.Size(80, 120);
            this.x2.Text = "Ende";
            // 
            // ERechts
            // 
            this.ERechts.ImageCode = "Pfeil_Rechts|12|||0088FF";
            this.ERechts.Location = new System.Drawing.Point(40, 48);
            this.ERechts.Name = "ERechts";
            this.ERechts.Size = new System.Drawing.Size(32, 32);
            this.ERechts.TabIndex = 5;
            this.ERechts.Click += new System.EventHandler(this.ERechts_Click);
            // 
            // EOben
            // 
            this.EOben.ImageCode = "Pfeil_Oben|16|||0088FF";
            this.EOben.Location = new System.Drawing.Point(24, 16);
            this.EOben.Name = "EOben";
            this.EOben.Size = new System.Drawing.Size(32, 32);
            this.EOben.TabIndex = 37;
            this.EOben.Click += new System.EventHandler(this.EOben_Click);
            // 
            // ELinks
            // 
            this.ELinks.ImageCode = "Pfeil_Links|12|||0088FF";
            this.ELinks.Location = new System.Drawing.Point(8, 48);
            this.ELinks.Name = "ELinks";
            this.ELinks.Size = new System.Drawing.Size(32, 32);
            this.ELinks.TabIndex = 4;
            this.ELinks.Click += new System.EventHandler(this.ELinks_Click);
            // 
            // EUnten
            // 
            this.EUnten.ImageCode = "Pfeil_Unten|16|||0088FF";
            this.EUnten.Location = new System.Drawing.Point(24, 80);
            this.EUnten.Name = "EUnten";
            this.EUnten.Size = new System.Drawing.Size(32, 32);
            this.EUnten.TabIndex = 36;
            this.EUnten.Click += new System.EventHandler(this.EUnten_Click);
            // 
            // x1
            // 
            this.x1.CausesValidation = false;
            this.x1.Controls.Add(this.SRechts);
            this.x1.Controls.Add(this.SOben);
            this.x1.Controls.Add(this.SLinks);
            this.x1.Controls.Add(this.SUnten);
            this.x1.Enabled = false;
            this.x1.Location = new System.Drawing.Point(8, 96);
            this.x1.Name = "x1";
            this.x1.Size = new System.Drawing.Size(80, 120);
            this.x1.Text = "Start";
            // 
            // SRechts
            // 
            this.SRechts.ImageCode = "Pfeil_Rechts|12|||FF7700";
            this.SRechts.Location = new System.Drawing.Point(40, 48);
            this.SRechts.Name = "SRechts";
            this.SRechts.Size = new System.Drawing.Size(32, 32);
            this.SRechts.TabIndex = 5;
            this.SRechts.Click += new System.EventHandler(this.SRechts_Click);
            // 
            // SOben
            // 
            this.SOben.ImageCode = "Pfeil_Oben|16|||FF7700";
            this.SOben.Location = new System.Drawing.Point(24, 16);
            this.SOben.Name = "SOben";
            this.SOben.Size = new System.Drawing.Size(32, 32);
            this.SOben.TabIndex = 37;
            this.SOben.Click += new System.EventHandler(this.SOben_Click);
            // 
            // SLinks
            // 
            this.SLinks.ImageCode = "Pfeil_Links|12|||FF7700";
            this.SLinks.Location = new System.Drawing.Point(8, 48);
            this.SLinks.Name = "SLinks";
            this.SLinks.Size = new System.Drawing.Size(32, 32);
            this.SLinks.TabIndex = 4;
            this.SLinks.Click += new System.EventHandler(this.SLinks_Click);
            // 
            // SUnten
            // 
            this.SUnten.ImageCode = "Pfeil_Unten|16|||FF7700";
            this.SUnten.Location = new System.Drawing.Point(24, 80);
            this.SUnten.Name = "SUnten";
            this.SUnten.Size = new System.Drawing.Size(32, 32);
            this.SUnten.TabIndex = 36;
            this.SUnten.Click += new System.EventHandler(this.SUnten_Click);
            // 
            // Rename
            // 
            this.Rename.ImageCode = "Stift|16";
            this.Rename.Location = new System.Drawing.Point(80, 24);
            this.Rename.Name = "Rename";
            this.Rename.QuickInfo = "Reiter-Beschriftung der<br>aktuellen Ansicht ändern";
            this.Rename.Size = new System.Drawing.Size(32, 24);
            this.Rename.TabIndex = 35;
            this.Rename.Click += new System.EventHandler(this.Rename_Click);
            // 
            // SpaltBEnde
            // 
            this.SpaltBEnde.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SpaltBEnde.ImageCode = "Kreuz|26";
            this.SpaltBEnde.Location = new System.Drawing.Point(152, 0);
            this.SpaltBEnde.Name = "SpaltBEnde";
            this.SpaltBEnde.QuickInfo = "Formular-Editor beenden";
            this.SpaltBEnde.Size = new System.Drawing.Size(24, 24);
            this.SpaltBEnde.TabIndex = 34;
            this.SpaltBEnde.Click += new System.EventHandler(this.SpaltBEnde_Click);
            // 
            // Rechtex
            // 
            this.Rechtex.ImageCode = "Schild|16";
            this.Rechtex.Location = new System.Drawing.Point(8, 56);
            this.Rechtex.Name = "Rechtex";
            this.Rechtex.QuickInfo = "Berechtigungsgruppen für<br>aktuelle Ansicht bearbeiten";
            this.Rechtex.Size = new System.Drawing.Size(160, 24);
            this.Rechtex.TabIndex = 32;
            this.Rechtex.Text = "Berechtigungsgruppen";
            this.Rechtex.Click += new System.EventHandler(this.Rechte_Click);
            // 
            // OrderDelete
            // 
            this.OrderDelete.ImageCode = "MinusZeichen|16";
            this.OrderDelete.Location = new System.Drawing.Point(40, 24);
            this.OrderDelete.Name = "OrderDelete";
            this.OrderDelete.QuickInfo = "Aktuelle Ansicht<br>löschen";
            this.OrderDelete.Size = new System.Drawing.Size(32, 24);
            this.OrderDelete.TabIndex = 6;
            this.OrderDelete.Click += new System.EventHandler(this.OrderDelete_Click);
            // 
            // OrderAdd
            // 
            this.OrderAdd.ImageCode = "PlusZeichen|16";
            this.OrderAdd.Location = new System.Drawing.Point(8, 24);
            this.OrderAdd.Name = "OrderAdd";
            this.OrderAdd.QuickInfo = "Neue Ansicht<br>erstellen";
            this.OrderAdd.Size = new System.Drawing.Size(32, 24);
            this.OrderAdd.TabIndex = 2;
            this.OrderAdd.Click += new System.EventHandler(this.OrderAdd_Click);
            // 
            // Formula
            // 
            this.Controls.Add(this.Tabs);
            this.Controls.Add(this.Editor);
            this.MinimumSize = new System.Drawing.Size(320, 350);
            this.Name = "Formula";
            this.Size = new System.Drawing.Size(320, 350);
            this.SizeChanged += new System.EventHandler(this.BlueFormula_SizeChanged);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Tabs_MouseUp);
            this.Editor.ResumeLayout(false);
            this.x2.ResumeLayout(false);
            this.x1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private GroupBox Editor;
        private Button SUnten;
        private Button SpaltBEnde;
        private Button Rechtex;
        private Button OrderDelete;
        private Button SRechts;
        private Button SLinks;
        private Button OrderAdd;
        private Button SOben;
        private GroupBox x2;
        private Button ERechts;
        private Button EOben;
        private Button ELinks;
        private Button EUnten;
        private GroupBox x1;
        private Line BlueLine1;
        private ListBox lbxColumns;
        private Caption capUeberschrift;
        private Line BlueLine2;
        private ComboBox cbxCaptionPosition;
        private Button Rename;
        private Button Re;
        private Button Li;
        private Caption Caption2;
        private ComboBox cbxControlType;
        private TabControl Tabs;
        private System.Windows.Forms.ColorDialog ColDiax;


    }
}