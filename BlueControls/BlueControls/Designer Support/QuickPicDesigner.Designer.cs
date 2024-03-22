namespace BlueControls.Designer_Support;

partial class QuickPicDesigner {
    /// <summary> 
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

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
            this.txbZweitsymbol = new System.Windows.Forms.TextBox();
            this.grpColor = new System.Windows.Forms.GroupBox();
            this.txbChangeGreen = new System.Windows.Forms.TextBox();
            this.txbFaerbung = new System.Windows.Forms.TextBox();
            this.Label5 = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.Transpl = new System.Windows.Forms.Label();
            this.Label6 = new System.Windows.Forms.Label();
            this.Helll = new System.Windows.Forms.Label();
            this.satTransparenz = new System.Windows.Forms.TrackBar();
            this.Label4 = new System.Windows.Forms.Label();
            this.satLum = new System.Windows.Forms.TrackBar();
            this.SATL = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.sldSat = new System.Windows.Forms.TrackBar();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.grpEffects = new System.Windows.Forms.GroupBox();
            this.chkbXPDisabled = new System.Windows.Forms.CheckBox();
            this.chkbMEDisabled = new System.Windows.Forms.CheckBox();
            this.chkbGrauStufen = new System.Windows.Forms.CheckBox();
            this.chkbDurchgestrichen = new System.Windows.Forms.CheckBox();
            this.txbHeight = new System.Windows.Forms.TextBox();
            this.txbWidth = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.GroupBox2 = new System.Windows.Forms.GroupBox();
            this.grpName = new System.Windows.Forms.GroupBox();
            this.lstNames = new BlueControls.Controls.ListBox();
            this.txbName = new System.Windows.Forms.TextBox();
            this.grpZweitsymbol = new System.Windows.Forms.GroupBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.grpColor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.satTransparenz)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.satLum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sldSat)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.grpEffects.SuspendLayout();
            this.GroupBox2.SuspendLayout();
            this.grpName.SuspendLayout();
            this.grpZweitsymbol.SuspendLayout();
            this.SuspendLayout();
            // 
            // txbZweitsymbol
            // 
            this.txbZweitsymbol.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txbZweitsymbol.Location = new System.Drawing.Point(8, 24);
            this.txbZweitsymbol.Name = "txbZweitsymbol";
            this.txbZweitsymbol.Size = new System.Drawing.Size(184, 20);
            this.txbZweitsymbol.TabIndex = 17;
            this.txbZweitsymbol.TextChanged += new System.EventHandler(this.SomethingChanged);
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
            this.grpColor.Location = new System.Drawing.Point(224, 192);
            this.grpColor.Name = "grpColor";
            this.grpColor.Size = new System.Drawing.Size(368, 144);
            this.grpColor.TabIndex = 13;
            this.grpColor.TabStop = false;
            this.grpColor.Text = "Color";
            // 
            // txbChangeGreen
            // 
            this.txbChangeGreen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txbChangeGreen.Location = new System.Drawing.Point(200, 112);
            this.txbChangeGreen.Name = "txbChangeGreen";
            this.txbChangeGreen.Size = new System.Drawing.Size(48, 20);
            this.txbChangeGreen.TabIndex = 17;
            this.txbChangeGreen.TextChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // txbFaerbung
            // 
            this.txbFaerbung.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txbFaerbung.Location = new System.Drawing.Point(64, 112);
            this.txbFaerbung.Name = "txbFaerbung";
            this.txbFaerbung.Size = new System.Drawing.Size(48, 20);
            this.txbFaerbung.TabIndex = 16;
            this.txbFaerbung.TextChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // Label5
            // 
            this.Label5.Location = new System.Drawing.Point(128, 112);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(72, 16);
            this.Label5.TabIndex = 13;
            this.Label5.Text = "Grün wird zu:";
            // 
            // Label3
            // 
            this.Label3.Location = new System.Drawing.Point(8, 112);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(56, 16);
            this.Label3.TabIndex = 11;
            this.Label3.Text = "Färbung:";
            // 
            // Transpl
            // 
            this.Transpl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Transpl.Location = new System.Drawing.Point(320, 72);
            this.Transpl.Name = "Transpl";
            this.Transpl.Size = new System.Drawing.Size(40, 16);
            this.Transpl.TabIndex = 10;
            this.Transpl.Text = "0%";
            // 
            // Label6
            // 
            this.Label6.Location = new System.Drawing.Point(8, 72);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(72, 16);
            this.Label6.TabIndex = 9;
            this.Label6.Text = "Transparenz:";
            // 
            // Helll
            // 
            this.Helll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Helll.Location = new System.Drawing.Point(320, 40);
            this.Helll.Name = "Helll";
            this.Helll.Size = new System.Drawing.Size(40, 16);
            this.Helll.TabIndex = 10;
            this.Helll.Text = "100%";
            // 
            // satTransparenz
            // 
            this.satTransparenz.AutoSize = false;
            this.satTransparenz.LargeChange = 1;
            this.satTransparenz.Location = new System.Drawing.Point(80, 72);
            this.satTransparenz.Maximum = 99;
            this.satTransparenz.Name = "satTransparenz";
            this.satTransparenz.Size = new System.Drawing.Size(240, 32);
            this.satTransparenz.TabIndex = 8;
            this.satTransparenz.TickFrequency = 10;
            this.satTransparenz.ValueChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // Label4
            // 
            this.Label4.Location = new System.Drawing.Point(8, 40);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(56, 16);
            this.Label4.TabIndex = 9;
            this.Label4.Text = "Helligkeit:";
            // 
            // satLum
            // 
            this.satLum.AutoSize = false;
            this.satLum.LargeChange = 1;
            this.satLum.Location = new System.Drawing.Point(80, 40);
            this.satLum.Maximum = 200;
            this.satLum.Name = "satLum";
            this.satLum.Size = new System.Drawing.Size(240, 32);
            this.satLum.TabIndex = 8;
            this.satLum.TickFrequency = 10;
            this.satLum.Value = 100;
            this.satLum.ValueChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // SATL
            // 
            this.SATL.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SATL.Location = new System.Drawing.Point(320, 16);
            this.SATL.Name = "SATL";
            this.SATL.Size = new System.Drawing.Size(40, 16);
            this.SATL.TabIndex = 7;
            this.SATL.Text = "100%";
            // 
            // Label2
            // 
            this.Label2.Location = new System.Drawing.Point(8, 16);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(56, 16);
            this.Label2.TabIndex = 6;
            this.Label2.Text = "Sättigung:";
            // 
            // sldSat
            // 
            this.sldSat.AutoSize = false;
            this.sldSat.LargeChange = 1;
            this.sldSat.Location = new System.Drawing.Point(80, 8);
            this.sldSat.Maximum = 200;
            this.sldSat.Name = "sldSat";
            this.sldSat.Size = new System.Drawing.Size(240, 32);
            this.sldSat.TabIndex = 5;
            this.sldSat.TickFrequency = 10;
            this.sldSat.Value = 100;
            this.sldSat.ValueChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // picPreview
            // 
            this.picPreview.Location = new System.Drawing.Point(224, 16);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(80, 72);
            this.picPreview.TabIndex = 15;
            this.picPreview.TabStop = false;
            // 
            // grpEffects
            // 
            this.grpEffects.Controls.Add(this.chkbXPDisabled);
            this.grpEffects.Controls.Add(this.chkbMEDisabled);
            this.grpEffects.Controls.Add(this.chkbGrauStufen);
            this.grpEffects.Controls.Add(this.chkbDurchgestrichen);
            this.grpEffects.Location = new System.Drawing.Point(224, 96);
            this.grpEffects.Name = "grpEffects";
            this.grpEffects.Size = new System.Drawing.Size(160, 88);
            this.grpEffects.TabIndex = 14;
            this.grpEffects.TabStop = false;
            this.grpEffects.Text = "Effekt";
            // 
            // chkbXPDisabled
            // 
            this.chkbXPDisabled.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkbXPDisabled.Location = new System.Drawing.Point(8, 48);
            this.chkbXPDisabled.Name = "chkbXPDisabled";
            this.chkbXPDisabled.Size = new System.Drawing.Size(136, 16);
            this.chkbXPDisabled.TabIndex = 2;
            this.chkbXPDisabled.Text = "Windows XP disabled";
            this.chkbXPDisabled.CheckedChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // chkbMEDisabled
            // 
            this.chkbMEDisabled.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkbMEDisabled.Location = new System.Drawing.Point(8, 32);
            this.chkbMEDisabled.Name = "chkbMEDisabled";
            this.chkbMEDisabled.Size = new System.Drawing.Size(136, 16);
            this.chkbMEDisabled.TabIndex = 1;
            this.chkbMEDisabled.Text = "Windows ME disabled";
            this.chkbMEDisabled.CheckedChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // chkbGrauStufen
            // 
            this.chkbGrauStufen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkbGrauStufen.Location = new System.Drawing.Point(8, 64);
            this.chkbGrauStufen.Name = "chkbGrauStufen";
            this.chkbGrauStufen.Size = new System.Drawing.Size(152, 16);
            this.chkbGrauStufen.TabIndex = 15;
            this.chkbGrauStufen.Text = "Bild in Graustufen anzeigen";
            this.chkbGrauStufen.CheckedChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // chkbDurchgestrichen
            // 
            this.chkbDurchgestrichen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkbDurchgestrichen.Location = new System.Drawing.Point(8, 16);
            this.chkbDurchgestrichen.Name = "chkbDurchgestrichen";
            this.chkbDurchgestrichen.Size = new System.Drawing.Size(120, 16);
            this.chkbDurchgestrichen.TabIndex = 0;
            this.chkbDurchgestrichen.Text = "Durchgestrichen";
            this.chkbDurchgestrichen.CheckedChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // txbHeight
            // 
            this.txbHeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txbHeight.Location = new System.Drawing.Point(88, 16);
            this.txbHeight.Name = "txbHeight";
            this.txbHeight.Size = new System.Drawing.Size(24, 20);
            this.txbHeight.TabIndex = 2;
            this.txbHeight.Text = "16";
            this.txbHeight.TextChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // txbWidth
            // 
            this.txbWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txbWidth.Location = new System.Drawing.Point(48, 16);
            this.txbWidth.Name = "txbWidth";
            this.txbWidth.Size = new System.Drawing.Size(24, 20);
            this.txbWidth.TabIndex = 0;
            this.txbWidth.Text = "16";
            this.txbWidth.TextChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // Label1
            // 
            this.Label1.Location = new System.Drawing.Point(8, 16);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(144, 16);
            this.Label1.TabIndex = 1;
            this.Label1.Text = "Größe            x            Pixel";
            // 
            // GroupBox2
            // 
            this.GroupBox2.Controls.Add(this.txbHeight);
            this.GroupBox2.Controls.Add(this.txbWidth);
            this.GroupBox2.Controls.Add(this.Label1);
            this.GroupBox2.Location = new System.Drawing.Point(312, 16);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new System.Drawing.Size(160, 48);
            this.GroupBox2.TabIndex = 12;
            this.GroupBox2.TabStop = false;
            this.GroupBox2.Text = "Abmessung";
            // 
            // grpName
            // 
            this.grpName.Controls.Add(this.lstNames);
            this.grpName.Controls.Add(this.txbName);
            this.grpName.Location = new System.Drawing.Point(8, 8);
            this.grpName.Name = "grpName";
            this.grpName.Size = new System.Drawing.Size(208, 358);
            this.grpName.TabIndex = 11;
            this.grpName.TabStop = false;
            this.grpName.Text = "Name";
            // 
            // lstNames
            // 
            this.lstNames.AddAllowed = BlueControls.Enums.AddType.Text;
            this.lstNames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstNames.Location = new System.Drawing.Point(8, 16);
            this.lstNames.Name = "lstNames";
            this.lstNames.Size = new System.Drawing.Size(192, 303);
            this.lstNames.TabIndex = 0;
            this.lstNames.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.LstNames_ItemClicked);
            // 
            // txbName
            // 
            this.txbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txbName.Location = new System.Drawing.Point(8, 334);
            this.txbName.Name = "txbName";
            this.txbName.Size = new System.Drawing.Size(192, 20);
            this.txbName.TabIndex = 2;
            this.txbName.Text = "PicNam";
            this.txbName.TextChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // grpZweitsymbol
            // 
            this.grpZweitsymbol.Controls.Add(this.txbZweitsymbol);
            this.grpZweitsymbol.Location = new System.Drawing.Point(392, 96);
            this.grpZweitsymbol.Name = "grpZweitsymbol";
            this.grpZweitsymbol.Size = new System.Drawing.Size(200, 88);
            this.grpZweitsymbol.TabIndex = 16;
            this.grpZweitsymbol.TabStop = false;
            this.grpZweitsymbol.Text = "Zweitsymbol";
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(527, 339);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(64, 24);
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
            this.Size = new System.Drawing.Size(609, 385);
            this.grpColor.ResumeLayout(false);
            this.grpColor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.satTransparenz)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.satLum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sldSat)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
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

    private System.Windows.Forms.TextBox txbZweitsymbol;
    private System.Windows.Forms.GroupBox grpColor;
    private System.Windows.Forms.TextBox txbChangeGreen;
    private System.Windows.Forms.TextBox txbFaerbung;
    private System.Windows.Forms.Label Label5;
    private System.Windows.Forms.Label Label3;
    private System.Windows.Forms.Label Transpl;
    private System.Windows.Forms.Label Label6;
    private System.Windows.Forms.Label Helll;
    private System.Windows.Forms.TrackBar satTransparenz;
    private System.Windows.Forms.Label Label4;
    private System.Windows.Forms.TrackBar satLum;
    private System.Windows.Forms.Label SATL;
    private System.Windows.Forms.Label Label2;
    private System.Windows.Forms.TrackBar sldSat;
    private System.Windows.Forms.PictureBox picPreview;
    private System.Windows.Forms.GroupBox grpEffects;
    private System.Windows.Forms.CheckBox chkbXPDisabled;
    private System.Windows.Forms.CheckBox chkbMEDisabled;
    private System.Windows.Forms.CheckBox chkbGrauStufen;
    private System.Windows.Forms.CheckBox chkbDurchgestrichen;
    private System.Windows.Forms.TextBox txbHeight;
    private System.Windows.Forms.TextBox txbWidth;
    private System.Windows.Forms.Label Label1;
    private System.Windows.Forms.GroupBox GroupBox2;
    private System.Windows.Forms.GroupBox grpName;
    private BlueControls.Controls.ListBox lstNames;
    private System.Windows.Forms.TextBox txbName;
    private System.Windows.Forms.GroupBox grpZweitsymbol;
    internal System.Windows.Forms.Button btnOk;
}
