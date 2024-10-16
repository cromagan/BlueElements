using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Enums;
using BlueControls.EventArgs;
using ListBox = BlueControls.Controls.ListBox;

namespace BlueControls.Designer_Support;

partial class QuickPicDesigner {
    /// <summary> 
    /// Erforderliche Designervariable.
    /// </summary>
    private IContainer components = null;

    /// <summary> 
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {
        if (disposing && (components != null)) {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Vom Komponenten-Designer generierter Code

    /// <summary> 
    /// Erforderliche Methode für die Designerunterstützung. 
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent() {
            this.txbZweitsymbol = new TextBox();
            this.grpColor = new GroupBox();
            this.txbChangeGreen = new TextBox();
            this.txbFaerbung = new TextBox();
            this.Label5 = new Label();
            this.Label3 = new Label();
            this.Transpl = new Label();
            this.Label6 = new Label();
            this.Helll = new Label();
            this.satTransparenz = new TrackBar();
            this.Label4 = new Label();
            this.satLum = new TrackBar();
            this.SATL = new Label();
            this.Label2 = new Label();
            this.sldSat = new TrackBar();
            this.picPreview = new PictureBox();
            this.grpEffects = new GroupBox();
            this.chkbXPDisabled = new CheckBox();
            this.chkbMEDisabled = new CheckBox();
            this.chkbGrauStufen = new CheckBox();
            this.chkbDurchgestrichen = new CheckBox();
            this.txbHeight = new TextBox();
            this.txbWidth = new TextBox();
            this.Label1 = new Label();
            this.GroupBox2 = new GroupBox();
            this.grpName = new GroupBox();
            this.lstNames = new ListBox();
            this.txbName = new TextBox();
            this.grpZweitsymbol = new GroupBox();
            this.btnOk = new Button();
            this.grpColor.SuspendLayout();
            ((ISupportInitialize)(this.satTransparenz)).BeginInit();
            ((ISupportInitialize)(this.satLum)).BeginInit();
            ((ISupportInitialize)(this.sldSat)).BeginInit();
            ((ISupportInitialize)(this.picPreview)).BeginInit();
            this.grpEffects.SuspendLayout();
            this.GroupBox2.SuspendLayout();
            this.grpName.SuspendLayout();
            this.grpZweitsymbol.SuspendLayout();
            this.SuspendLayout();
            // 
            // txbZweitsymbol
            // 
            this.txbZweitsymbol.BorderStyle = BorderStyle.FixedSingle;
            this.txbZweitsymbol.Location = new Point(8, 24);
            this.txbZweitsymbol.Name = "txbZweitsymbol";
            this.txbZweitsymbol.Size = new Size(184, 20);
            this.txbZweitsymbol.TabIndex = 17;
            this.txbZweitsymbol.TextChanged += new EventHandler(this.SomethingChanged);
            // 
            // grpColor
            // 
            this.grpColor.Controls.Add(this.txbChangeGreen);
            this.grpColor.Controls.Add(this.txbFaerbung);
            this.grpColor.Controls.Add(this.Label5);
            this.grpColor.Controls.Add(this.Label3);
            this.grpColor.Controls.Add(this.Transpl);
            this.grpColor.Controls.Add(this.Label6);
            this.grpColor.Controls.Add(this.Helll);
            this.grpColor.Controls.Add(this.satTransparenz);
            this.grpColor.Controls.Add(this.Label4);
            this.grpColor.Controls.Add(this.satLum);
            this.grpColor.Controls.Add(this.SATL);
            this.grpColor.Controls.Add(this.Label2);
            this.grpColor.Controls.Add(this.sldSat);
            this.grpColor.Location = new Point(224, 192);
            this.grpColor.Name = "grpColor";
            this.grpColor.Size = new Size(368, 144);
            this.grpColor.TabIndex = 13;
            this.grpColor.TabStop = false;
            this.grpColor.Text = "Color";
            // 
            // txbChangeGreen
            // 
            this.txbChangeGreen.BorderStyle = BorderStyle.FixedSingle;
            this.txbChangeGreen.Location = new Point(200, 112);
            this.txbChangeGreen.Name = "txbChangeGreen";
            this.txbChangeGreen.Size = new Size(48, 20);
            this.txbChangeGreen.TabIndex = 17;
            this.txbChangeGreen.TextChanged += new EventHandler(this.SomethingChanged);
            // 
            // txbFaerbung
            // 
            this.txbFaerbung.BorderStyle = BorderStyle.FixedSingle;
            this.txbFaerbung.Location = new Point(64, 112);
            this.txbFaerbung.Name = "txbFaerbung";
            this.txbFaerbung.Size = new Size(48, 20);
            this.txbFaerbung.TabIndex = 16;
            this.txbFaerbung.TextChanged += new EventHandler(this.SomethingChanged);
            // 
            // Label5
            // 
            this.Label5.Location = new Point(128, 112);
            this.Label5.Name = "Label5";
            this.Label5.Size = new Size(72, 16);
            this.Label5.TabIndex = 13;
            this.Label5.Text = "Grün wird zu:";
            // 
            // Label3
            // 
            this.Label3.Location = new Point(8, 112);
            this.Label3.Name = "Label3";
            this.Label3.Size = new Size(56, 16);
            this.Label3.TabIndex = 11;
            this.Label3.Text = "Färbung:";
            // 
            // Transpl
            // 
            this.Transpl.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            this.Transpl.Location = new Point(320, 72);
            this.Transpl.Name = "Transpl";
            this.Transpl.Size = new Size(40, 16);
            this.Transpl.TabIndex = 10;
            this.Transpl.Text = "0%";
            // 
            // Label6
            // 
            this.Label6.Location = new Point(8, 72);
            this.Label6.Name = "Label6";
            this.Label6.Size = new Size(72, 16);
            this.Label6.TabIndex = 9;
            this.Label6.Text = "Transparenz:";
            // 
            // Helll
            // 
            this.Helll.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            this.Helll.Location = new Point(320, 40);
            this.Helll.Name = "Helll";
            this.Helll.Size = new Size(40, 16);
            this.Helll.TabIndex = 10;
            this.Helll.Text = "100%";
            // 
            // satTransparenz
            // 
            this.satTransparenz.AutoSize = false;
            this.satTransparenz.LargeChange = 1;
            this.satTransparenz.Location = new Point(80, 72);
            this.satTransparenz.Maximum = 99;
            this.satTransparenz.Name = "satTransparenz";
            this.satTransparenz.Size = new Size(240, 32);
            this.satTransparenz.TabIndex = 8;
            this.satTransparenz.TickFrequency = 10;
            this.satTransparenz.ValueChanged += new EventHandler(this.SomethingChanged);
            // 
            // Label4
            // 
            this.Label4.Location = new Point(8, 40);
            this.Label4.Name = "Label4";
            this.Label4.Size = new Size(56, 16);
            this.Label4.TabIndex = 9;
            this.Label4.Text = "Helligkeit:";
            // 
            // satLum
            // 
            this.satLum.AutoSize = false;
            this.satLum.LargeChange = 1;
            this.satLum.Location = new Point(80, 40);
            this.satLum.Maximum = 200;
            this.satLum.Name = "satLum";
            this.satLum.Size = new Size(240, 32);
            this.satLum.TabIndex = 8;
            this.satLum.TickFrequency = 10;
            this.satLum.Value = 100;
            this.satLum.ValueChanged += new EventHandler(this.SomethingChanged);
            // 
            // SATL
            // 
            this.SATL.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            this.SATL.Location = new Point(320, 16);
            this.SATL.Name = "SATL";
            this.SATL.Size = new Size(40, 16);
            this.SATL.TabIndex = 7;
            this.SATL.Text = "100%";
            // 
            // Label2
            // 
            this.Label2.Location = new Point(8, 16);
            this.Label2.Name = "Label2";
            this.Label2.Size = new Size(56, 16);
            this.Label2.TabIndex = 6;
            this.Label2.Text = "Sättigung:";
            // 
            // sldSat
            // 
            this.sldSat.AutoSize = false;
            this.sldSat.LargeChange = 1;
            this.sldSat.Location = new Point(80, 8);
            this.sldSat.Maximum = 200;
            this.sldSat.Name = "sldSat";
            this.sldSat.Size = new Size(240, 32);
            this.sldSat.TabIndex = 5;
            this.sldSat.TickFrequency = 10;
            this.sldSat.Value = 100;
            this.sldSat.ValueChanged += new EventHandler(this.SomethingChanged);
            // 
            // picPreview
            // 
            this.picPreview.Location = new Point(224, 16);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new Size(80, 72);
            this.picPreview.TabIndex = 15;
            this.picPreview.TabStop = false;
            // 
            // grpEffects
            // 
            this.grpEffects.Controls.Add(this.chkbXPDisabled);
            this.grpEffects.Controls.Add(this.chkbMEDisabled);
            this.grpEffects.Controls.Add(this.chkbGrauStufen);
            this.grpEffects.Controls.Add(this.chkbDurchgestrichen);
            this.grpEffects.Location = new Point(224, 96);
            this.grpEffects.Name = "grpEffects";
            this.grpEffects.Size = new Size(160, 88);
            this.grpEffects.TabIndex = 14;
            this.grpEffects.TabStop = false;
            this.grpEffects.Text = "Effekt";
            // 
            // chkbXPDisabled
            // 
            this.chkbXPDisabled.FlatStyle = FlatStyle.Flat;
            this.chkbXPDisabled.Location = new Point(8, 48);
            this.chkbXPDisabled.Name = "chkbXPDisabled";
            this.chkbXPDisabled.Size = new Size(136, 16);
            this.chkbXPDisabled.TabIndex = 2;
            this.chkbXPDisabled.Text = "Windows XP disabled";
            this.chkbXPDisabled.CheckedChanged += new EventHandler(this.SomethingChanged);
            // 
            // chkbMEDisabled
            // 
            this.chkbMEDisabled.FlatStyle = FlatStyle.Flat;
            this.chkbMEDisabled.Location = new Point(8, 32);
            this.chkbMEDisabled.Name = "chkbMEDisabled";
            this.chkbMEDisabled.Size = new Size(136, 16);
            this.chkbMEDisabled.TabIndex = 1;
            this.chkbMEDisabled.Text = "Windows ME disabled";
            this.chkbMEDisabled.CheckedChanged += new EventHandler(this.SomethingChanged);
            // 
            // chkbGrauStufen
            // 
            this.chkbGrauStufen.FlatStyle = FlatStyle.Flat;
            this.chkbGrauStufen.Location = new Point(8, 64);
            this.chkbGrauStufen.Name = "chkbGrauStufen";
            this.chkbGrauStufen.Size = new Size(152, 16);
            this.chkbGrauStufen.TabIndex = 15;
            this.chkbGrauStufen.Text = "Bild in Graustufen anzeigen";
            this.chkbGrauStufen.CheckedChanged += new EventHandler(this.SomethingChanged);
            // 
            // chkbDurchgestrichen
            // 
            this.chkbDurchgestrichen.FlatStyle = FlatStyle.Flat;
            this.chkbDurchgestrichen.Location = new Point(8, 16);
            this.chkbDurchgestrichen.Name = "chkbDurchgestrichen";
            this.chkbDurchgestrichen.Size = new Size(120, 16);
            this.chkbDurchgestrichen.TabIndex = 0;
            this.chkbDurchgestrichen.Text = "Durchgestrichen";
            this.chkbDurchgestrichen.CheckedChanged += new EventHandler(this.SomethingChanged);
            // 
            // txbHeight
            // 
            this.txbHeight.BorderStyle = BorderStyle.FixedSingle;
            this.txbHeight.Location = new Point(88, 16);
            this.txbHeight.Name = "txbHeight";
            this.txbHeight.Size = new Size(24, 20);
            this.txbHeight.TabIndex = 2;
            this.txbHeight.Text = "16";
            this.txbHeight.TextChanged += new EventHandler(this.SomethingChanged);
            // 
            // txbWidth
            // 
            this.txbWidth.BorderStyle = BorderStyle.FixedSingle;
            this.txbWidth.Location = new Point(48, 16);
            this.txbWidth.Name = "txbWidth";
            this.txbWidth.Size = new Size(24, 20);
            this.txbWidth.TabIndex = 0;
            this.txbWidth.Text = "16";
            this.txbWidth.TextChanged += new EventHandler(this.SomethingChanged);
            // 
            // Label1
            // 
            this.Label1.Location = new Point(8, 16);
            this.Label1.Name = "Label1";
            this.Label1.Size = new Size(144, 16);
            this.Label1.TabIndex = 1;
            this.Label1.Text = "Größe            x            Pixel";
            // 
            // GroupBox2
            // 
            this.GroupBox2.Controls.Add(this.txbHeight);
            this.GroupBox2.Controls.Add(this.txbWidth);
            this.GroupBox2.Controls.Add(this.Label1);
            this.GroupBox2.Location = new Point(312, 16);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new Size(160, 48);
            this.GroupBox2.TabIndex = 12;
            this.GroupBox2.TabStop = false;
            this.GroupBox2.Text = "Abmessung";
            // 
            // grpName
            // 
            this.grpName.Controls.Add(this.lstNames);
            this.grpName.Controls.Add(this.txbName);
            this.grpName.Location = new Point(8, 8);
            this.grpName.Name = "grpName";
            this.grpName.Size = new Size(208, 358);
            this.grpName.TabIndex = 11;
            this.grpName.TabStop = false;
            this.grpName.Text = "Name";
            // 
            // lstNames
            // 
            this.lstNames.AddAllowed = AddType.Text;
            this.lstNames.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                     | AnchorStyles.Left) 
                                                    | AnchorStyles.Right)));
            this.lstNames.Location = new Point(8, 16);
            this.lstNames.Name = "lstNames";
            this.lstNames.Size = new Size(192, 303);
            this.lstNames.TabIndex = 0;
            this.lstNames.ItemClicked += new EventHandler<AbstractListItemEventArgs>(this.LstNames_ItemClicked);
            // 
            // txbName
            // 
            this.txbName.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left) 
                                                   | AnchorStyles.Right)));
            this.txbName.BorderStyle = BorderStyle.FixedSingle;
            this.txbName.Location = new Point(8, 334);
            this.txbName.Name = "txbName";
            this.txbName.Size = new Size(192, 20);
            this.txbName.TabIndex = 2;
            this.txbName.Text = "PicNam";
            this.txbName.TextChanged += new EventHandler(this.SomethingChanged);
            // 
            // grpZweitsymbol
            // 
            this.grpZweitsymbol.Controls.Add(this.txbZweitsymbol);
            this.grpZweitsymbol.Location = new Point(392, 96);
            this.grpZweitsymbol.Name = "grpZweitsymbol";
            this.grpZweitsymbol.Size = new Size(200, 88);
            this.grpZweitsymbol.TabIndex = 16;
            this.grpZweitsymbol.TabStop = false;
            this.grpZweitsymbol.Text = "Zweitsymbol";
            // 
            // btnOk
            // 
            this.btnOk.Location = new Point(527, 339);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new Size(64, 24);
            this.btnOk.TabIndex = 10;
            this.btnOk.Text = "OK";
            // 
            // QuickPicDesigner
            // 
            this.Controls.Add(this.grpColor);
            this.Controls.Add(this.picPreview);
            this.Controls.Add(this.grpEffects);
            this.Controls.Add(this.GroupBox2);
            this.Controls.Add(this.grpName);
            this.Controls.Add(this.grpZweitsymbol);
            this.Controls.Add(this.btnOk);
            this.Name = "QuickPicDesigner";
            this.Size = new Size(609, 385);
            this.grpColor.ResumeLayout(false);
            this.grpColor.PerformLayout();
            ((ISupportInitialize)(this.satTransparenz)).EndInit();
            ((ISupportInitialize)(this.satLum)).EndInit();
            ((ISupportInitialize)(this.sldSat)).EndInit();
            ((ISupportInitialize)(this.picPreview)).EndInit();
            this.grpEffects.ResumeLayout(false);
            this.GroupBox2.ResumeLayout(false);
            this.GroupBox2.PerformLayout();
            this.grpName.ResumeLayout(false);
            this.grpName.PerformLayout();
            this.grpZweitsymbol.ResumeLayout(false);
            this.grpZweitsymbol.PerformLayout();
            this.ResumeLayout(false);

    }

  
    #endregion

    private TextBox txbZweitsymbol;
    private GroupBox grpColor;
    private TextBox txbChangeGreen;
    private TextBox txbFaerbung;
    private Label Label5;
    private Label Label3;
    private Label Transpl;
    private Label Label6;
    private Label Helll;
    private TrackBar satTransparenz;
    private Label Label4;
    private TrackBar satLum;
    private Label SATL;
    private Label Label2;
    private TrackBar sldSat;
    private PictureBox picPreview;
    private GroupBox grpEffects;
    private CheckBox chkbXPDisabled;
    private CheckBox chkbMEDisabled;
    private CheckBox chkbGrauStufen;
    private CheckBox chkbDurchgestrichen;
    private TextBox txbHeight;
    private TextBox txbWidth;
    private Label Label1;
    private GroupBox GroupBox2;
    private GroupBox grpName;
    private ListBox lstNames;
    private TextBox txbName;
    private GroupBox grpZweitsymbol;
    internal Button btnOk;
}
