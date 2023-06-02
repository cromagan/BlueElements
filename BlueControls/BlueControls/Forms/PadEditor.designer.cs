using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueDatabase.Enums;
using Button = BlueControls.Controls.Button;
using ColorDialog = System.Windows.Forms.ColorDialog;
using ComboBox = BlueControls.Controls.ComboBox;
using GroupBox = BlueControls.Controls.GroupBox;
using TabControl = BlueControls.Controls.TabControl;
using TabPage = System.Windows.Forms.TabPage;
using TextBox = BlueControls.Controls.TextBox;

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
            this.capRasterFangen = new Caption();
            this.capRasterAnzeige = new Caption();
            this.txbRasterFangen = new TextBox();
            this.txbRasterAnzeige = new TextBox();
            this.ckbRaster = new Button();
            this.grpKomponenteHinzufügen = new GroupBox();
            this.btnAddPhsyik = new Button();
            this.btnAddSymbol = new Button();
            this.btnAddUnterStufe = new Button();
            this.btnAddText = new Button();
            this.btnAddImage = new Button();
            this.btnAddDimension = new Button();
            this.btnAddLine = new Button();
            this.tabHintergrund = new TabPage();
            this.grpDesign = new GroupBox();
            this.btnKeinHintergrund = new Button();
            this.btnHintergrundFarbe = new Button();
            this.btnArbeitsbreichSetup = new Button();
            this.cbxSchriftGröße = new ComboBox();
            this.capSchriftgröße = new Caption();
            this.PadDesign = new ComboBox();
            this.capDesign = new Caption();
            this.ColorDia = new ColorDialog();
            this.tabRightSide = new TabControl();
            this.tabElementEigenschaften = new TabPage();
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
            this.Pad.Size = new Size(512, 225);
            this.Pad.ClickedItemChanging += new EventHandler(this.Pad_ClickedItemChanging);
            this.Pad.ClickedItemChanged += new EventHandler(this.Pad_ClickedItemChanged);
            this.Pad.GotNewItemCollection += new EventHandler(this.Pad_GotNewItemCollection);
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
            this.grpAssistent.Size = new Size(200, 81);
            this.grpAssistent.Controls.SetChildIndex(this.btnVorschauModus, 0);
            this.grpAssistent.Controls.SetChildIndex(this.ckbRaster, 0);
            this.grpAssistent.Controls.SetChildIndex(this.txbRasterAnzeige, 0);
            this.grpAssistent.Controls.SetChildIndex(this.txbRasterFangen, 0);
            this.grpAssistent.Controls.SetChildIndex(this.capRasterAnzeige, 0);
            this.grpAssistent.Controls.SetChildIndex(this.capRasterFangen, 0);
            // 
            // capRasterFangen
            // 
            this.capRasterFangen.CausesValidation = false;
            this.capRasterFangen.Location = new Point(72, 46);
            this.capRasterFangen.Name = "capRasterFangen";
            this.capRasterFangen.Size = new Size(56, 22);
            this.capRasterFangen.Text = "Fangen:";
            // 
            // capRasterAnzeige
            // 
            this.capRasterAnzeige.CausesValidation = false;
            this.capRasterAnzeige.Location = new Point(72, 24);
            this.capRasterAnzeige.Name = "capRasterAnzeige";
            this.capRasterAnzeige.Size = new Size(56, 22);
            this.capRasterAnzeige.Text = "Anzeige:";
            // 
            // txbRasterFangen
            // 
            this.txbRasterFangen.AdditionalFormatCheck = BlueBasics.Enums.AdditionalCheck.Float;
            this.txbRasterFangen.AllowedChars = "0123456789,";
            this.txbRasterFangen.Cursor = Cursors.IBeam;
            this.txbRasterFangen.Location = new Point(128, 46);
            this.txbRasterFangen.Name = "txbRasterFangen";
            this.txbRasterFangen.Regex = "(^-?([1-9]\\d*)|^0)([.]\\d*[1-9])?$";
            this.txbRasterFangen.Size = new Size(64, 22);
            this.txbRasterFangen.Suffix = "mm";
            this.txbRasterFangen.TabIndex = 6;
            this.txbRasterFangen.Text = "10";
            this.txbRasterFangen.TextChanged += new EventHandler(this.txbRasterFangen_TextChanged);
            // 
            // txbRasterAnzeige
            // 
            this.txbRasterAnzeige.AdditionalFormatCheck = BlueBasics.Enums.AdditionalCheck.Float;
            this.txbRasterAnzeige.AllowedChars = "0123456789,";
            this.txbRasterAnzeige.Cursor = Cursors.IBeam;
            this.txbRasterAnzeige.Location = new Point(128, 24);
            this.txbRasterAnzeige.Name = "txbRasterAnzeige";
            this.txbRasterAnzeige.Regex = "(^-?([1-9]\\d*)|^0)([.]\\d*[1-9])?$";
            this.txbRasterAnzeige.Size = new Size(64, 22);
            this.txbRasterAnzeige.Suffix = "mm";
            this.txbRasterAnzeige.TabIndex = 5;
            this.txbRasterAnzeige.Text = "10";
            this.txbRasterAnzeige.TextChanged += new EventHandler(this.txbRasterAnzeige_TextChanged);
            // 
            // ckbRaster
            // 
            this.ckbRaster.ButtonStyle = ((ButtonStyle)(((ButtonStyle.Checkbox | ButtonStyle.Button_Big) 
                                                         | ButtonStyle.Borderless)));
            this.ckbRaster.ImageCode = "Raster|18";
            this.ckbRaster.Location = new Point(72, 2);
            this.ckbRaster.Name = "ckbRaster";
            this.ckbRaster.Size = new Size(120, 22);
            this.ckbRaster.TabIndex = 4;
            this.ckbRaster.Text = "Raster";
            this.ckbRaster.CheckedChanged += new EventHandler(this.ckbRaster_CheckedChanged);
            // 
            // grpKomponenteHinzufügen
            // 
            this.grpKomponenteHinzufügen.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpKomponenteHinzufügen.CausesValidation = false;
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddPhsyik);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddSymbol);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddUnterStufe);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddText);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddImage);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddDimension);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddLine);
            this.grpKomponenteHinzufügen.Dock = DockStyle.Left;
            this.grpKomponenteHinzufügen.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            this.grpKomponenteHinzufügen.Location = new Point(496, 0);
            this.grpKomponenteHinzufügen.Name = "grpKomponenteHinzufügen";
            this.grpKomponenteHinzufügen.Size = new Size(272, 81);
            this.grpKomponenteHinzufügen.TabIndex = 0;
            this.grpKomponenteHinzufügen.TabStop = false;
            this.grpKomponenteHinzufügen.Text = "Komponente hinzufügen";
            // 
            // btnAddPhsyik
            // 
            this.btnAddPhsyik.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnAddPhsyik.ImageCode = "Formel|16";
            this.btnAddPhsyik.Location = new Point(176, 24);
            this.btnAddPhsyik.Name = "btnAddPhsyik";
            this.btnAddPhsyik.Size = new Size(88, 22);
            this.btnAddPhsyik.TabIndex = 10;
            this.btnAddPhsyik.Text = "Physik";
            this.btnAddPhsyik.Click += new EventHandler(this.btnAddPhsyik_Click);
            // 
            // btnAddSymbol
            // 
            this.btnAddSymbol.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnAddSymbol.ImageCode = "Stern|16|||||137|0";
            this.btnAddSymbol.Location = new Point(8, 46);
            this.btnAddSymbol.Name = "btnAddSymbol";
            this.btnAddSymbol.Size = new Size(80, 22);
            this.btnAddSymbol.TabIndex = 9;
            this.btnAddSymbol.Text = "Symbol";
            this.btnAddSymbol.Click += new EventHandler(this.btnAddSymbol_Click);
            // 
            // btnAddUnterStufe
            // 
            this.btnAddUnterStufe.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnAddUnterStufe.ImageCode = "Datei|16";
            this.btnAddUnterStufe.Location = new Point(176, 2);
            this.btnAddUnterStufe.Name = "btnAddUnterStufe";
            this.btnAddUnterStufe.Size = new Size(88, 22);
            this.btnAddUnterStufe.TabIndex = 8;
            this.btnAddUnterStufe.Text = "Unterstufe";
            this.btnAddUnterStufe.Click += new EventHandler(this.btnAddUnterStufe_Click);
            // 
            // btnAddText
            // 
            this.btnAddText.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnAddText.ImageCode = "Textfeld|16";
            this.btnAddText.Location = new Point(8, 2);
            this.btnAddText.Name = "btnAddText";
            this.btnAddText.Size = new Size(80, 22);
            this.btnAddText.TabIndex = 4;
            this.btnAddText.Text = "Text";
            this.btnAddText.Click += new EventHandler(this.btnAddText_Click);
            // 
            // btnAddImage
            // 
            this.btnAddImage.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnAddImage.ImageCode = "Bild|16";
            this.btnAddImage.Location = new Point(8, 24);
            this.btnAddImage.Name = "btnAddImage";
            this.btnAddImage.Size = new Size(80, 22);
            this.btnAddImage.TabIndex = 2;
            this.btnAddImage.Text = "Bild";
            this.btnAddImage.Click += new EventHandler(this.btnAddImage_Click);
            // 
            // btnAddDimension
            // 
            this.btnAddDimension.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnAddDimension.ImageCode = "Bemaßung|16";
            this.btnAddDimension.Location = new Point(96, 2);
            this.btnAddDimension.Name = "btnAddDimension";
            this.btnAddDimension.Size = new Size(72, 22);
            this.btnAddDimension.TabIndex = 6;
            this.btnAddDimension.Text = "Maß";
            this.btnAddDimension.Click += new EventHandler(this.btnAddDimension_Click);
            // 
            // btnAddLine
            // 
            this.btnAddLine.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnAddLine.ImageCode = "Linie|16";
            this.btnAddLine.Location = new Point(96, 24);
            this.btnAddLine.Name = "btnAddLine";
            this.btnAddLine.Size = new Size(72, 22);
            this.btnAddLine.TabIndex = 7;
            this.btnAddLine.Text = "Linie";
            this.btnAddLine.Click += new EventHandler(this.btnAddLine_Click);
            // 
            // tabHintergrund
            // 
            this.tabHintergrund.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabHintergrund.Controls.Add(this.grpDesign);
            this.tabHintergrund.Location = new Point(4, 25);
            this.tabHintergrund.Name = "tabHintergrund";
            this.tabHintergrund.Size = new Size(876, 81);
            this.tabHintergrund.TabIndex = 3;
            this.tabHintergrund.Text = "Hintergrund";
            // 
            // grpDesign
            // 
            this.grpDesign.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpDesign.CausesValidation = false;
            this.grpDesign.Controls.Add(this.btnKeinHintergrund);
            this.grpDesign.Controls.Add(this.btnHintergrundFarbe);
            this.grpDesign.Controls.Add(this.btnArbeitsbreichSetup);
            this.grpDesign.Controls.Add(this.cbxSchriftGröße);
            this.grpDesign.Controls.Add(this.capSchriftgröße);
            this.grpDesign.Controls.Add(this.PadDesign);
            this.grpDesign.Controls.Add(this.capDesign);
            this.grpDesign.Dock = DockStyle.Left;
            this.grpDesign.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            this.grpDesign.Location = new Point(0, 0);
            this.grpDesign.Name = "grpDesign";
            this.grpDesign.Size = new Size(648, 81);
            this.grpDesign.TabIndex = 1;
            this.grpDesign.TabStop = false;
            this.grpDesign.Text = "Design";
            // 
            // btnKeinHintergrund
            // 
            this.btnKeinHintergrund.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnKeinHintergrund.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnKeinHintergrund.Location = new Point(464, 2);
            this.btnKeinHintergrund.Name = "btnKeinHintergrund";
            this.btnKeinHintergrund.Size = new Size(112, 22);
            this.btnKeinHintergrund.TabIndex = 16;
            this.btnKeinHintergrund.Text = "kein Hintergrund";
            this.btnKeinHintergrund.Click += new EventHandler(this.btnKeinHintergrund_Click);
            // 
            // btnHintergrundFarbe
            // 
            this.btnHintergrundFarbe.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnHintergrundFarbe.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnHintergrundFarbe.ImageCode = "Farben";
            this.btnHintergrundFarbe.Location = new Point(376, 2);
            this.btnHintergrundFarbe.Name = "btnHintergrundFarbe";
            this.btnHintergrundFarbe.Size = new Size(80, 66);
            this.btnHintergrundFarbe.TabIndex = 15;
            this.btnHintergrundFarbe.Text = "Hintergrund-Farbe";
            this.btnHintergrundFarbe.Click += new EventHandler(this.btnHintergrundFarbe_Click);
            // 
            // btnArbeitsbreichSetup
            // 
            this.btnArbeitsbreichSetup.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnArbeitsbreichSetup.ImageCode = "SeiteEinrichten";
            this.btnArbeitsbreichSetup.Location = new Point(8, 2);
            this.btnArbeitsbreichSetup.Name = "btnArbeitsbreichSetup";
            this.btnArbeitsbreichSetup.Size = new Size(96, 66);
            this.btnArbeitsbreichSetup.TabIndex = 13;
            this.btnArbeitsbreichSetup.Text = "Arbeitsbereich einreichten";
            this.btnArbeitsbreichSetup.Click += new EventHandler(this.btnArbeitsbreichSetup_Click);
            // 
            // cbxSchriftGröße
            // 
            this.cbxSchriftGröße.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                           | AnchorStyles.Right)));
            this.cbxSchriftGröße.Cursor = Cursors.IBeam;
            this.cbxSchriftGröße.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxSchriftGröße.Location = new Point(208, 46);
            this.cbxSchriftGröße.Name = "cbxSchriftGröße";
            this.cbxSchriftGröße.Regex = null;
            this.cbxSchriftGröße.Size = new Size(168, 22);
            this.cbxSchriftGröße.TabIndex = 3;
            this.cbxSchriftGröße.ItemClicked += new EventHandler<BasicListItemEventArgs>(this.cbxSchriftGröße_ItemClicked);
            // 
            // capSchriftgröße
            // 
            this.capSchriftgröße.CausesValidation = false;
            this.capSchriftgröße.Location = new Point(112, 46);
            this.capSchriftgröße.Name = "capSchriftgröße";
            this.capSchriftgröße.Size = new Size(88, 22);
            this.capSchriftgröße.Text = "Schrift-Größe:";
            // 
            // PadDesign
            // 
            this.PadDesign.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                     | AnchorStyles.Right)));
            this.PadDesign.Cursor = Cursors.IBeam;
            this.PadDesign.DropDownStyle = ComboBoxStyle.DropDownList;
            this.PadDesign.Location = new Point(112, 24);
            this.PadDesign.Name = "PadDesign";
            this.PadDesign.Regex = null;
            this.PadDesign.Size = new Size(264, 22);
            this.PadDesign.TabIndex = 1;
            this.PadDesign.ItemClicked += new EventHandler<BasicListItemEventArgs>(this.PadDesign_ItemClicked);
            // 
            // capDesign
            // 
            this.capDesign.CausesValidation = false;
            this.capDesign.Location = new Point(112, 2);
            this.capDesign.Name = "capDesign";
            this.capDesign.Size = new Size(77, 22);
            this.capDesign.Text = "Design:";
            this.capDesign.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            // 
            // tabRightSide
            // 
            this.tabRightSide.Controls.Add(this.tabElementEigenschaften);
            this.tabRightSide.Dock = DockStyle.Right;
            this.tabRightSide.HotTrack = true;
            this.tabRightSide.Location = new Point(512, 136);
            this.tabRightSide.Name = "tabRightSide";
            this.tabRightSide.SelectedIndex = 0;
            this.tabRightSide.Size = new Size(372, 225);
            this.tabRightSide.TabDefault = null;
            this.tabRightSide.TabDefaultOrder = null;
            this.tabRightSide.TabIndex = 4;
            // 
            // tabElementEigenschaften
            // 
            this.tabElementEigenschaften.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabElementEigenschaften.Location = new Point(4, 25);
            this.tabElementEigenschaften.Name = "tabElementEigenschaften";
            this.tabElementEigenschaften.Size = new Size(364, 196);
            this.tabElementEigenschaften.TabIndex = 0;
            this.tabElementEigenschaften.Text = "Element-Eigenschaften";
            // 
            // PadEditor
            // 
            this.ClientSize = new Size(884, 361);
            this.Controls.Add(this.tabRightSide);
            this.Name = "PadEditor";
            this.Controls.SetChildIndex(this.Ribbon, 0);
            this.Controls.SetChildIndex(this.tabSeiten, 0);
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
        private ColorDialog ColorDia;
        private Button btnKeinHintergrund;
        protected TabPage tabHintergrund;
        private TabPage tabElementEigenschaften;
        protected TabControl tabRightSide;
    }
}