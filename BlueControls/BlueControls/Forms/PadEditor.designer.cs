using System.Diagnostics;
using BlueControls.Controls;

namespace BlueControls.Forms {
    public partial class PadEditor {
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
            this.capRasterFangen = new BlueControls.Controls.Caption();
            this.capRasterAnzeige = new BlueControls.Controls.Caption();
            this.txbRasterFangen = new BlueControls.Controls.TextBox();
            this.txbRasterAnzeige = new BlueControls.Controls.TextBox();
            this.ckbRaster = new BlueControls.Controls.Button();
            this.grpKomponenteHinzufügen = new BlueControls.Controls.GroupBox();
            this.btnAddPhsyik = new BlueControls.Controls.Button();
            this.btnAddSymbol = new BlueControls.Controls.Button();
            this.btnAddUnterStufe = new BlueControls.Controls.Button();
            this.btnAddText = new BlueControls.Controls.Button();
            this.btnAddImage = new BlueControls.Controls.Button();
            this.btnAddDimension = new BlueControls.Controls.Button();
            this.btnAddLine = new BlueControls.Controls.Button();
            this.tabHintergrund = new System.Windows.Forms.TabPage();
            this.grpDesign = new BlueControls.Controls.GroupBox();
            this.btnKeinHintergrund = new BlueControls.Controls.Button();
            this.btnHintergrundFarbe = new BlueControls.Controls.Button();
            this.btnArbeitsbreichSetup = new BlueControls.Controls.Button();
            this.cbxSchriftGröße = new BlueControls.Controls.ComboBox();
            this.capSchriftgröße = new BlueControls.Controls.Caption();
            this.PadDesign = new BlueControls.Controls.ComboBox();
            this.capDesign = new BlueControls.Controls.Caption();
            this.ColorDia = new System.Windows.Forms.ColorDialog();
            this.tabRightSide = new BlueControls.Controls.TabControl();
            this.tabElementEigenschaften = new System.Windows.Forms.TabPage();
            this.Ribbon.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAssistent.SuspendLayout();
            this.grpKomponenteHinzufügen.SuspendLayout();
            this.tabHintergrund.SuspendLayout();
            this.grpDesign.SuspendLayout();
            this.tabRightSide.SuspendLayout();
            this.SuspendLayout();
            // 
            // Pad
            // 
            this.Pad.Size = new System.Drawing.Size(512, 251);
            this.Pad.ClickedItemChanged += new System.EventHandler(this.Pad_ClickedItemChanged);
            this.Pad.GotNewItemCollection += new System.EventHandler(this.Pad_GotNewItemCollection);
            // 
            // Ribbon
            // 
            this.Ribbon.Controls.Add(this.tabHintergrund);
            this.Ribbon.TabDefaultOrder = new string[] {
        "Start",
        "Hintergrund",
        "Export"};
            this.Ribbon.Controls.SetChildIndex(this.tabHintergrund, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabExport, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabStart, 0);
            // 
            // tabStart
            // 
            this.tabStart.Controls.Add(this.grpKomponenteHinzufügen);
            this.tabStart.Controls.SetChildIndex(this.grpAssistent, 0);
            this.tabStart.Controls.SetChildIndex(this.grpKomponenteHinzufügen, 0);
            // 
            // grpAssistent
            // 
            this.grpAssistent.Controls.Add(this.capRasterFangen);
            this.grpAssistent.Controls.Add(this.capRasterAnzeige);
            this.grpAssistent.Controls.Add(this.txbRasterFangen);
            this.grpAssistent.Controls.Add(this.txbRasterAnzeige);
            this.grpAssistent.Controls.Add(this.ckbRaster);
            this.grpAssistent.Size = new System.Drawing.Size(200, 81);
            this.grpAssistent.Controls.SetChildIndex(this.ckbRaster, 0);
            this.grpAssistent.Controls.SetChildIndex(this.txbRasterAnzeige, 0);
            this.grpAssistent.Controls.SetChildIndex(this.txbRasterFangen, 0);
            this.grpAssistent.Controls.SetChildIndex(this.capRasterAnzeige, 0);
            this.grpAssistent.Controls.SetChildIndex(this.capRasterFangen, 0);
            // 
            // capRasterFangen
            // 
            this.capRasterFangen.CausesValidation = false;
            this.capRasterFangen.Location = new System.Drawing.Point(72, 46);
            this.capRasterFangen.Name = "capRasterFangen";
            this.capRasterFangen.Size = new System.Drawing.Size(56, 22);
            this.capRasterFangen.Text = "Fangen:";
            // 
            // capRasterAnzeige
            // 
            this.capRasterAnzeige.CausesValidation = false;
            this.capRasterAnzeige.Location = new System.Drawing.Point(72, 24);
            this.capRasterAnzeige.Name = "capRasterAnzeige";
            this.capRasterAnzeige.Size = new System.Drawing.Size(56, 22);
            this.capRasterAnzeige.Text = "Anzeige:";
            // 
            // txbRasterFangen
            // 
            this.txbRasterFangen.AdditionalCheck = BlueDatabase.Enums.AdditionalCheck.Float;
            this.txbRasterFangen.AllowedChars = "0123456789,";
            this.txbRasterFangen.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbRasterFangen.Location = new System.Drawing.Point(128, 46);
            this.txbRasterFangen.Name = "txbRasterFangen";
            this.txbRasterFangen.Regex = "(^-?([1-9]\\d*)|^0)([.]\\d*[1-9])?$";
            this.txbRasterFangen.Size = new System.Drawing.Size(64, 22);
            this.txbRasterFangen.Suffix = "mm";
            this.txbRasterFangen.TabIndex = 6;
            this.txbRasterFangen.Text = "10";
            this.txbRasterFangen.TextChanged += new System.EventHandler(this.txbRasterFangen_TextChanged);
            // 
            // txbRasterAnzeige
            // 
            this.txbRasterAnzeige.AdditionalCheck = BlueDatabase.Enums.AdditionalCheck.Float;
            this.txbRasterAnzeige.AllowedChars = "0123456789,";
            this.txbRasterAnzeige.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbRasterAnzeige.Location = new System.Drawing.Point(128, 24);
            this.txbRasterAnzeige.Name = "txbRasterAnzeige";
            this.txbRasterAnzeige.Regex = "(^-?([1-9]\\d*)|^0)([.]\\d*[1-9])?$";
            this.txbRasterAnzeige.Size = new System.Drawing.Size(64, 22);
            this.txbRasterAnzeige.Suffix = "mm";
            this.txbRasterAnzeige.TabIndex = 5;
            this.txbRasterAnzeige.Text = "10";
            this.txbRasterAnzeige.TextChanged += new System.EventHandler(this.txbRasterAnzeige_TextChanged);
            // 
            // ckbRaster
            // 
            this.ckbRaster.ButtonStyle = ((BlueControls.Enums.ButtonStyle)(((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Button_Big) 
            | BlueControls.Enums.ButtonStyle.Borderless)));
            this.ckbRaster.ImageCode = "Raster|18";
            this.ckbRaster.Location = new System.Drawing.Point(72, 2);
            this.ckbRaster.Name = "ckbRaster";
            this.ckbRaster.Size = new System.Drawing.Size(120, 22);
            this.ckbRaster.TabIndex = 4;
            this.ckbRaster.Text = "Raster";
            this.ckbRaster.CheckedChanged += new System.EventHandler(this.ckbRaster_CheckedChanged);
            // 
            // grpKomponenteHinzufügen
            // 
            this.grpKomponenteHinzufügen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpKomponenteHinzufügen.CausesValidation = false;
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddPhsyik);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddSymbol);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddUnterStufe);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddText);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddImage);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddDimension);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddLine);
            this.grpKomponenteHinzufügen.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpKomponenteHinzufügen.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpKomponenteHinzufügen.Location = new System.Drawing.Point(496, 0);
            this.grpKomponenteHinzufügen.Name = "grpKomponenteHinzufügen";
            this.grpKomponenteHinzufügen.Size = new System.Drawing.Size(272, 81);
            this.grpKomponenteHinzufügen.TabIndex = 0;
            this.grpKomponenteHinzufügen.TabStop = false;
            this.grpKomponenteHinzufügen.Text = "Komponente hinzufügen";
            // 
            // btnAddPhsyik
            // 
            this.btnAddPhsyik.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnAddPhsyik.ImageCode = "Formel|16";
            this.btnAddPhsyik.Location = new System.Drawing.Point(176, 24);
            this.btnAddPhsyik.Name = "btnAddPhsyik";
            this.btnAddPhsyik.Size = new System.Drawing.Size(88, 22);
            this.btnAddPhsyik.TabIndex = 10;
            this.btnAddPhsyik.Text = "Physik";
            this.btnAddPhsyik.Click += new System.EventHandler(this.btnAddPhsyik_Click);
            // 
            // btnAddSymbol
            // 
            this.btnAddSymbol.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnAddSymbol.ImageCode = "Stern|16|||||137|0";
            this.btnAddSymbol.Location = new System.Drawing.Point(8, 46);
            this.btnAddSymbol.Name = "btnAddSymbol";
            this.btnAddSymbol.Size = new System.Drawing.Size(80, 22);
            this.btnAddSymbol.TabIndex = 9;
            this.btnAddSymbol.Text = "Symbol";
            this.btnAddSymbol.Click += new System.EventHandler(this.btnAddSymbol_Click);
            // 
            // btnAddUnterStufe
            // 
            this.btnAddUnterStufe.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnAddUnterStufe.ImageCode = "Datei|16";
            this.btnAddUnterStufe.Location = new System.Drawing.Point(176, 2);
            this.btnAddUnterStufe.Name = "btnAddUnterStufe";
            this.btnAddUnterStufe.Size = new System.Drawing.Size(88, 22);
            this.btnAddUnterStufe.TabIndex = 8;
            this.btnAddUnterStufe.Text = "Unterstufe";
            this.btnAddUnterStufe.Click += new System.EventHandler(this.btnAddUnterStufe_Click);
            // 
            // btnAddText
            // 
            this.btnAddText.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnAddText.ImageCode = "Textfeld|16";
            this.btnAddText.Location = new System.Drawing.Point(8, 2);
            this.btnAddText.Name = "btnAddText";
            this.btnAddText.Size = new System.Drawing.Size(80, 22);
            this.btnAddText.TabIndex = 4;
            this.btnAddText.Text = "Text";
            this.btnAddText.Click += new System.EventHandler(this.btnAddText_Click);
            // 
            // btnAddImage
            // 
            this.btnAddImage.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnAddImage.ImageCode = "Bild|16";
            this.btnAddImage.Location = new System.Drawing.Point(8, 24);
            this.btnAddImage.Name = "btnAddImage";
            this.btnAddImage.Size = new System.Drawing.Size(80, 22);
            this.btnAddImage.TabIndex = 2;
            this.btnAddImage.Text = "Bild";
            this.btnAddImage.Click += new System.EventHandler(this.btnAddImage_Click);
            // 
            // btnAddDimension
            // 
            this.btnAddDimension.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnAddDimension.ImageCode = "Bemaßung|16";
            this.btnAddDimension.Location = new System.Drawing.Point(96, 2);
            this.btnAddDimension.Name = "btnAddDimension";
            this.btnAddDimension.Size = new System.Drawing.Size(72, 22);
            this.btnAddDimension.TabIndex = 6;
            this.btnAddDimension.Text = "Maß";
            this.btnAddDimension.Click += new System.EventHandler(this.btnAddDimension_Click);
            // 
            // btnAddLine
            // 
            this.btnAddLine.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnAddLine.ImageCode = "Linie|16";
            this.btnAddLine.Location = new System.Drawing.Point(96, 24);
            this.btnAddLine.Name = "btnAddLine";
            this.btnAddLine.Size = new System.Drawing.Size(72, 22);
            this.btnAddLine.TabIndex = 7;
            this.btnAddLine.Text = "Linie";
            this.btnAddLine.Click += new System.EventHandler(this.btnAddLine_Click);
            // 
            // tabHintergrund
            // 
            this.tabHintergrund.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabHintergrund.Controls.Add(this.grpDesign);
            this.tabHintergrund.Location = new System.Drawing.Point(4, 25);
            this.tabHintergrund.Name = "tabHintergrund";
            this.tabHintergrund.Size = new System.Drawing.Size(876, 81);
            this.tabHintergrund.TabIndex = 3;
            this.tabHintergrund.Text = "Hintergrund";
            // 
            // grpDesign
            // 
            this.grpDesign.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpDesign.CausesValidation = false;
            this.grpDesign.Controls.Add(this.btnKeinHintergrund);
            this.grpDesign.Controls.Add(this.btnHintergrundFarbe);
            this.grpDesign.Controls.Add(this.btnArbeitsbreichSetup);
            this.grpDesign.Controls.Add(this.cbxSchriftGröße);
            this.grpDesign.Controls.Add(this.capSchriftgröße);
            this.grpDesign.Controls.Add(this.PadDesign);
            this.grpDesign.Controls.Add(this.capDesign);
            this.grpDesign.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpDesign.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpDesign.Location = new System.Drawing.Point(0, 0);
            this.grpDesign.Name = "grpDesign";
            this.grpDesign.Size = new System.Drawing.Size(648, 81);
            this.grpDesign.TabIndex = 1;
            this.grpDesign.TabStop = false;
            this.grpDesign.Text = "Design";
            // 
            // btnKeinHintergrund
            // 
            this.btnKeinHintergrund.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnKeinHintergrund.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnKeinHintergrund.Location = new System.Drawing.Point(464, 2);
            this.btnKeinHintergrund.Name = "btnKeinHintergrund";
            this.btnKeinHintergrund.Size = new System.Drawing.Size(112, 22);
            this.btnKeinHintergrund.TabIndex = 16;
            this.btnKeinHintergrund.Text = "kein Hintergrund";
            this.btnKeinHintergrund.Click += new System.EventHandler(this.btnKeinHintergrund_Click);
            // 
            // btnHintergrundFarbe
            // 
            this.btnHintergrundFarbe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHintergrundFarbe.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnHintergrundFarbe.ImageCode = "Farben";
            this.btnHintergrundFarbe.Location = new System.Drawing.Point(376, 2);
            this.btnHintergrundFarbe.Name = "btnHintergrundFarbe";
            this.btnHintergrundFarbe.Size = new System.Drawing.Size(80, 66);
            this.btnHintergrundFarbe.TabIndex = 15;
            this.btnHintergrundFarbe.Text = "Hintergrund-Farbe";
            this.btnHintergrundFarbe.Click += new System.EventHandler(this.btnHintergrundFarbe_Click);
            // 
            // btnArbeitsbreichSetup
            // 
            this.btnArbeitsbreichSetup.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnArbeitsbreichSetup.ImageCode = "SeiteEinrichten";
            this.btnArbeitsbreichSetup.Location = new System.Drawing.Point(8, 2);
            this.btnArbeitsbreichSetup.Name = "btnArbeitsbreichSetup";
            this.btnArbeitsbreichSetup.Size = new System.Drawing.Size(96, 66);
            this.btnArbeitsbreichSetup.TabIndex = 13;
            this.btnArbeitsbreichSetup.Text = "Arbeitsbereich einreichten";
            this.btnArbeitsbreichSetup.Click += new System.EventHandler(this.btnArbeitsbreichSetup_Click);
            // 
            // cbxSchriftGröße
            // 
            this.cbxSchriftGröße.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxSchriftGröße.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxSchriftGröße.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxSchriftGröße.Location = new System.Drawing.Point(208, 46);
            this.cbxSchriftGröße.Name = "cbxSchriftGröße";
            this.cbxSchriftGröße.Regex = null;
            this.cbxSchriftGröße.Size = new System.Drawing.Size(168, 22);
            this.cbxSchriftGröße.TabIndex = 3;
            this.cbxSchriftGröße.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.cbxSchriftGröße_ItemClicked);
            // 
            // capSchriftgröße
            // 
            this.capSchriftgröße.CausesValidation = false;
            this.capSchriftgröße.Location = new System.Drawing.Point(112, 46);
            this.capSchriftgröße.Name = "capSchriftgröße";
            this.capSchriftgröße.Size = new System.Drawing.Size(88, 22);
            this.capSchriftgröße.Text = "Schrift-Größe:";
            // 
            // PadDesign
            // 
            this.PadDesign.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PadDesign.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.PadDesign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PadDesign.Location = new System.Drawing.Point(112, 24);
            this.PadDesign.Name = "PadDesign";
            this.PadDesign.Regex = null;
            this.PadDesign.Size = new System.Drawing.Size(264, 22);
            this.PadDesign.TabIndex = 1;
            this.PadDesign.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.PadDesign_ItemClicked);
            // 
            // capDesign
            // 
            this.capDesign.CausesValidation = false;
            this.capDesign.Location = new System.Drawing.Point(112, 2);
            this.capDesign.Name = "capDesign";
            this.capDesign.Size = new System.Drawing.Size(77, 22);
            this.capDesign.Text = "Design:";
            this.capDesign.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            // 
            // tabRightSide
            // 
            this.tabRightSide.Controls.Add(this.tabElementEigenschaften);
            this.tabRightSide.Dock = System.Windows.Forms.DockStyle.Right;
            this.tabRightSide.HotTrack = true;
            this.tabRightSide.Location = new System.Drawing.Point(512, 110);
            this.tabRightSide.Name = "tabRightSide";
            this.tabRightSide.SelectedIndex = 0;
            this.tabRightSide.Size = new System.Drawing.Size(372, 251);
            this.tabRightSide.TabDefault = null;
            this.tabRightSide.TabDefaultOrder = null;
            this.tabRightSide.TabIndex = 4;
            // 
            // tabElementEigenschaften
            // 
            this.tabElementEigenschaften.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabElementEigenschaften.Location = new System.Drawing.Point(4, 25);
            this.tabElementEigenschaften.Name = "tabElementEigenschaften";
            this.tabElementEigenschaften.Size = new System.Drawing.Size(364, 222);
            this.tabElementEigenschaften.TabIndex = 0;
            this.tabElementEigenschaften.Text = "Element-Eigenschaften";
            // 
            // PadEditor
            // 
            this.ClientSize = new System.Drawing.Size(884, 361);
            this.Controls.Add(this.tabRightSide);
            this.Name = "PadEditor";
            this.Controls.SetChildIndex(this.Ribbon, 0);
            this.Controls.SetChildIndex(this.tabRightSide, 0);
            this.Controls.SetChildIndex(this.Pad, 0);
            this.Ribbon.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAssistent.ResumeLayout(false);
            this.grpKomponenteHinzufügen.ResumeLayout(false);
            this.tabHintergrund.ResumeLayout(false);
            this.grpDesign.ResumeLayout(false);
            this.tabRightSide.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Button btnAddText;
        private Button btnAddImage;
        private Button btnAddDimension;
        private Button btnAddLine;
        protected GroupBox grpKomponenteHinzufügen;
        protected GroupBox grpDesign;
        private Caption capDesign;
        private ComboBox PadDesign;
        protected Button ckbRaster;
        private Caption capRasterFangen;
        private Caption capRasterAnzeige;
        private TextBox txbRasterFangen;
        private TextBox txbRasterAnzeige;
        internal ComboBox cbxSchriftGröße;
        internal Caption capSchriftgröße;
        protected internal Button btnArbeitsbreichSetup;
        private Button btnAddUnterStufe;
        private Button btnAddSymbol;
        private Button btnAddPhsyik;
        private Button btnHintergrundFarbe;
        private System.Windows.Forms.ColorDialog ColorDia;
        private Button btnKeinHintergrund;
        protected System.Windows.Forms.TabPage tabHintergrund;
        private System.Windows.Forms.TabPage tabElementEigenschaften;
        protected TabControl tabRightSide;
    }
}