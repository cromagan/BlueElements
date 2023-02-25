// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using static BlueBasics.Converter;

namespace BlueControls.Designer_Support;

internal sealed class QuickPic : Panel {

    #region Fields

    internal Button ButOK;

    private CheckBox chkbDurchgestrichen;

    private CheckBox chkbGrauStufen;

    private CheckBox chkbMEDisabled;

    private CheckBox chkbXPDisabled;

    private TextBox F�rb;

    private GroupBox GroupBox1;

    private GroupBox GroupBox2;

    private GroupBox GroupBox3;

    private GroupBox GroupBox4;

    private TextBox gr�n;

    private TextBox GrX;

    private TextBox GrY;

    private TrackBar Hell;

    private Label Helll;

    private Label Label1;

    private Label Label2;

    private Label Label3;

    private Label Label4;

    private Label Label5;

    private Label Label6;

    private ListBox LB;

    private TextBox PicName;

    private PictureBox Preview;

    private TrackBar SAT;

    private Label SATL;

    private TrackBar Transp;

    private Label Transpl;

    private TextBox txbZweitsymbol;

    private GroupBox ZweitSymbol;

    #endregion

    #region Constructors

    public QuickPic() =>
        // Dieser Aufruf ist f�r den Windows Form-Designer erforderlich.
        InitializeComponent();

    #endregion

    // Initialisierungen nach dem Aufruf InitializeComponent() hinzuf�gen

    #region Methods

    public void GeneratePreview() {
        try {
            Preview.Image = QuickImage.Get(ICode());
        } catch {
            Preview.Image = null;
        }
    }

    public string ICode() {
        var e = (ImageCodeEffect)(((chkbGrauStufen.Checked ? -1 : 0) * -(int)ImageCodeEffect.Graustufen) | ((chkbDurchgestrichen.Checked ? -1 : 0) * -(int)ImageCodeEffect.Durchgestrichen) | ((chkbMEDisabled.Checked ? -1 : 0) * -(int)ImageCodeEffect.WindowsMEDisabled) | ((chkbXPDisabled.Checked ? -1 : 0) * -(int)ImageCodeEffect.WindowsXPDisabled));
        return QuickImage.GenerateCode(PicName.Text, IntParse(GrX.Text), IntParse(GrY.Text), e, F�rb.Text, gr�n.Text, SAT.Value, Hell.Value, 0, Transp.Value, txbZweitsymbol.Text);
    }

    public void StartAll(string qicode) {
        LB.Items.Clear();
        const ImageCode tempVar = (ImageCode)9999;
        for (ImageCode z = 0; z <= tempVar; z++) {
            var w = Enum.GetName(z.GetType(), z);
            if (!string.IsNullOrEmpty(w)) { _ = LB.Items.Add(w); }
        }
        QuickImage l = new(qicode, true);
        PicName.Text = l.Name;
        F�rb.Text = l.F�rbung;
        gr�n.Text = l.ChangeGreenTo;
        chkbGrauStufen.Checked = Convert.ToBoolean(l.Effekt & ImageCodeEffect.Graustufen);
        SAT.Value = l.S�ttigung;
        Hell.Value = l.Helligkeit;
        Transp.Value = l.Transparenz;
        //if (l.Effekt < 0) { l.Effekt =  ImageCodeEffect.Ohne; }
        chkbDurchgestrichen.Checked = Convert.ToBoolean(l.Effekt & ImageCodeEffect.Durchgestrichen);
        chkbMEDisabled.Checked = Convert.ToBoolean(l.Effekt & ImageCodeEffect.WindowsMEDisabled);
        chkbXPDisabled.Checked = Convert.ToBoolean(l.Effekt & ImageCodeEffect.WindowsXPDisabled);
        GrX.Text = l.Width.ToString();
        GrY.Text = l.Height.ToString();
        txbZweitsymbol.Text = l.Zweitsymbol;
    }

    // Die Form �berschreibt den Deletevorgang der Basisklasse, um Komponenten zu bereinigen.
    protected override void Dispose(bool NowDisposing) {
        if (NowDisposing) { }
        base.Dispose(NowDisposing);
    }

    private static void SomethingCheckedChanged(object sender, System.EventArgs e) { }

    [DebuggerStepThrough]
    private void InitializeComponent() {
        LB = new ListBox();
        ButOK = new Button();
        PicName = new TextBox();
        GroupBox1 = new GroupBox();
        GroupBox2 = new GroupBox();
        GrY = new TextBox();
        GrX = new TextBox();
        Label1 = new Label();
        chkbDurchgestrichen = new CheckBox();
        chkbGrauStufen = new CheckBox();
        chkbMEDisabled = new CheckBox();
        chkbXPDisabled = new CheckBox();
        GroupBox4 = new GroupBox();
        Preview = new PictureBox();
        SAT = new TrackBar();
        Label2 = new Label();
        SATL = new Label();
        Hell = new TrackBar();
        Label4 = new Label();
        Transp = new TrackBar();
        Helll = new Label();
        Label6 = new Label();
        Transpl = new Label();
        Label3 = new Label();
        Label5 = new Label();
        F�rb = new TextBox();
        gr�n = new TextBox();
        GroupBox3 = new GroupBox();
        ZweitSymbol = new GroupBox();
        txbZweitsymbol = new TextBox();
        GroupBox1.SuspendLayout();
        GroupBox2.SuspendLayout();
        GroupBox4.SuspendLayout();
        ((ISupportInitialize)Preview).BeginInit();
        ((ISupportInitialize)SAT).BeginInit();
        ((ISupportInitialize)Hell).BeginInit();
        ((ISupportInitialize)Transp).BeginInit();
        GroupBox3.SuspendLayout();
        ZweitSymbol.SuspendLayout();
        SuspendLayout();
        //
        // LB
        //
        LB.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                                     | AnchorStyles.Left
                                     | AnchorStyles.Right;
        LB.Location = new Point(8, 16);
        LB.Name = "LB";
        LB.Size = new Size(192, 303);
        LB.TabIndex = 0;
        LB.DoubleClick += LB_DoubleClick;
        //
        // ButOK
        //
        ButOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        ButOK.Location = new Point(519, 331);
        ButOK.Name = "ButOK";
        ButOK.Size = new Size(64, 24);
        ButOK.TabIndex = 1;
        ButOK.Text = "OK";
        //
        // PicName
        //
        PicName.Anchor = AnchorStyles.Bottom | AnchorStyles.Left
                                             | AnchorStyles.Right;
        PicName.BorderStyle = BorderStyle.FixedSingle;
        PicName.Location = new Point(8, 334);
        PicName.Name = "PicName";
        PicName.Size = new Size(192, 20);
        PicName.TabIndex = 2;
        PicName.Text = "PicNam";
        PicName.TextChanged += SomethingChanged;
        //
        // GroupBox1
        //
        GroupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                                            | AnchorStyles.Left;
        GroupBox1.Controls.Add(LB);
        GroupBox1.Controls.Add(PicName);
        GroupBox1.Location = new Point(0, 0);
        GroupBox1.Name = "GroupBox1";
        GroupBox1.Size = new Size(208, 358);
        GroupBox1.TabIndex = 3;
        GroupBox1.TabStop = false;
        GroupBox1.Text = "Name";
        //
        // GroupBox2
        //
        GroupBox2.Controls.Add(GrY);
        GroupBox2.Controls.Add(GrX);
        GroupBox2.Controls.Add(Label1);
        GroupBox2.Location = new Point(304, 8);
        GroupBox2.Name = "GroupBox2";
        GroupBox2.Size = new Size(160, 48);
        GroupBox2.TabIndex = 4;
        GroupBox2.TabStop = false;
        GroupBox2.Text = "Abmessung";
        //
        // GrY
        //
        GrY.BorderStyle = BorderStyle.FixedSingle;
        GrY.Location = new Point(88, 16);
        GrY.Name = "GrY";
        GrY.Size = new Size(24, 20);
        GrY.TabIndex = 2;
        GrY.Text = "16";
        GrY.TextChanged += SomethingChanged;
        //
        // GrX
        //
        GrX.BorderStyle = BorderStyle.FixedSingle;
        GrX.Location = new Point(48, 16);
        GrX.Name = "GrX";
        GrX.Size = new Size(24, 20);
        GrX.TabIndex = 0;
        GrX.Text = "16";
        GrX.TextChanged += SomethingChanged;
        //
        // Label1
        //
        Label1.Location = new Point(8, 16);
        Label1.Name = "Label1";
        Label1.Size = new Size(144, 16);
        Label1.TabIndex = 1;
        Label1.Text = "Gr��e            x            Pixel";
        //
        // chkbDurchgestrichen
        //
        chkbDurchgestrichen.FlatStyle = FlatStyle.Flat;
        chkbDurchgestrichen.Location = new Point(8, 16);
        chkbDurchgestrichen.Name = "chkbDurchgestrichen";
        chkbDurchgestrichen.Size = new Size(120, 16);
        chkbDurchgestrichen.TabIndex = 0;
        chkbDurchgestrichen.Text = "Durchgestrichen";
        chkbDurchgestrichen.CheckedChanged += SomethingCheckedChanged;
        //
        // chkbGrauStufen
        //
        chkbGrauStufen.FlatStyle = FlatStyle.Flat;
        chkbGrauStufen.Location = new Point(8, 64);
        chkbGrauStufen.Name = "chkbGrauStufen";
        chkbGrauStufen.Size = new Size(152, 16);
        chkbGrauStufen.TabIndex = 15;
        chkbGrauStufen.Text = "Bild in Graustufen anzeigen";
        chkbGrauStufen.CheckedChanged += SomethingCheckedChanged;
        //
        // chkbMEDisabled
        //
        chkbMEDisabled.FlatStyle = FlatStyle.Flat;
        chkbMEDisabled.Location = new Point(8, 32);
        chkbMEDisabled.Name = "chkbMEDisabled";
        chkbMEDisabled.Size = new Size(136, 16);
        chkbMEDisabled.TabIndex = 1;
        chkbMEDisabled.Text = "Windows ME disabled";
        chkbMEDisabled.CheckedChanged += SomethingCheckedChanged;
        //
        // chkbXPDisabled
        //
        chkbXPDisabled.FlatStyle = FlatStyle.Flat;
        chkbXPDisabled.Location = new Point(8, 48);
        chkbXPDisabled.Name = "chkbXPDisabled";
        chkbXPDisabled.Size = new Size(136, 16);
        chkbXPDisabled.TabIndex = 2;
        chkbXPDisabled.Text = "Windows XP disabled";
        chkbXPDisabled.CheckedChanged += SomethingCheckedChanged;
        //
        // GroupBox4
        //
        GroupBox4.Controls.Add(chkbXPDisabled);
        GroupBox4.Controls.Add(chkbMEDisabled);
        GroupBox4.Controls.Add(chkbGrauStufen);
        GroupBox4.Controls.Add(chkbDurchgestrichen);
        GroupBox4.Location = new Point(216, 88);
        GroupBox4.Name = "GroupBox4";
        GroupBox4.Size = new Size(160, 88);
        GroupBox4.TabIndex = 7;
        GroupBox4.TabStop = false;
        GroupBox4.Text = "Effekt";
        //
        // Preview
        //
        Preview.Location = new Point(216, 8);
        Preview.Name = "Preview";
        Preview.Size = new Size(80, 72);
        Preview.TabIndex = 8;
        Preview.TabStop = false;
        //
        // SAT
        //
        SAT.AutoSize = false;
        SAT.LargeChange = 1;
        SAT.Location = new Point(80, 8);
        SAT.Maximum = 200;
        SAT.Name = "SAT";
        SAT.Size = new Size(240, 32);
        SAT.TabIndex = 5;
        SAT.TickFrequency = 10;
        SAT.Value = 100;
        SAT.ValueChanged += SomethingChanged;
        //
        // Label2
        //
        Label2.Location = new Point(8, 16);
        Label2.Name = "Label2";
        Label2.Size = new Size(56, 16);
        Label2.TabIndex = 6;
        Label2.Text = "S�ttigung:";
        //
        // SATL
        //
        SATL.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
        SATL.Location = new Point(320, 16);
        SATL.Name = "SATL";
        SATL.Size = new Size(40, 16);
        SATL.TabIndex = 7;
        SATL.Text = "100%";
        //
        // Hell
        //
        Hell.AutoSize = false;
        Hell.LargeChange = 1;
        Hell.Location = new Point(80, 40);
        Hell.Maximum = 200;
        Hell.Name = "Hell";
        Hell.Size = new Size(240, 32);
        Hell.TabIndex = 8;
        Hell.TickFrequency = 10;
        Hell.Value = 100;
        Hell.ValueChanged += SomethingChanged;
        //
        // Label4
        //
        Label4.Location = new Point(8, 40);
        Label4.Name = "Label4";
        Label4.Size = new Size(56, 16);
        Label4.TabIndex = 9;
        Label4.Text = "Helligkeit:";
        //
        // Transp
        //
        Transp.AutoSize = false;
        Transp.LargeChange = 1;
        Transp.Location = new Point(80, 72);
        Transp.Maximum = 99;
        Transp.Name = "Transp";
        Transp.Size = new Size(240, 32);
        Transp.TabIndex = 8;
        Transp.TickFrequency = 10;
        Transp.ValueChanged += SomethingChanged;
        //
        // Helll
        //
        Helll.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
        Helll.Location = new Point(320, 40);
        Helll.Name = "Helll";
        Helll.Size = new Size(40, 16);
        Helll.TabIndex = 10;
        Helll.Text = "100%";
        //
        // Label6
        //
        Label6.Location = new Point(8, 72);
        Label6.Name = "Label6";
        Label6.Size = new Size(72, 16);
        Label6.TabIndex = 9;
        Label6.Text = "Transparenz:";
        //
        // Transpl
        //
        Transpl.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
        Transpl.Location = new Point(320, 72);
        Transpl.Name = "Transpl";
        Transpl.Size = new Size(40, 16);
        Transpl.TabIndex = 10;
        Transpl.Text = "0%";
        //
        // Label3
        //
        Label3.Location = new Point(8, 112);
        Label3.Name = "Label3";
        Label3.Size = new Size(56, 16);
        Label3.TabIndex = 11;
        Label3.Text = "F�rbung:";
        //
        // Label5
        //
        Label5.Location = new Point(128, 112);
        Label5.Name = "Label5";
        Label5.Size = new Size(72, 16);
        Label5.TabIndex = 13;
        Label5.Text = "Gr�n wird zu:";
        //
        // F�rb
        //
        F�rb.BorderStyle = BorderStyle.FixedSingle;
        F�rb.Location = new Point(64, 112);
        F�rb.Name = "F�rb";
        F�rb.Size = new Size(48, 20);
        F�rb.TabIndex = 16;
        F�rb.TextChanged += SomethingChanged;
        //
        // gr�n
        //
        gr�n.BorderStyle = BorderStyle.FixedSingle;
        gr�n.Location = new Point(200, 112);
        gr�n.Name = "gr�n";
        gr�n.Size = new Size(48, 20);
        gr�n.TabIndex = 17;
        gr�n.TextChanged += SomethingChanged;
        //
        // GroupBox3
        //
        GroupBox3.Anchor = AnchorStyles.Top | AnchorStyles.Left
                                            | AnchorStyles.Right;
        GroupBox3.Controls.Add(gr�n);
        GroupBox3.Controls.Add(F�rb);
        GroupBox3.Controls.Add(Label5);
        GroupBox3.Controls.Add(Label3);
        GroupBox3.Controls.Add(Transpl);
        GroupBox3.Controls.Add(Label6);
        GroupBox3.Controls.Add(Helll);
        GroupBox3.Controls.Add(Transp);
        GroupBox3.Controls.Add(Label4);
        GroupBox3.Controls.Add(Hell);
        GroupBox3.Controls.Add(SATL);
        GroupBox3.Controls.Add(Label2);
        GroupBox3.Controls.Add(SAT);
        GroupBox3.Location = new Point(216, 184);
        GroupBox3.Name = "GroupBox3";
        GroupBox3.Size = new Size(368, 144);
        GroupBox3.TabIndex = 6;
        GroupBox3.TabStop = false;
        GroupBox3.Text = "Color";
        //
        // ZweitSymbol
        //
        ZweitSymbol.Anchor = AnchorStyles.Top | AnchorStyles.Left
                                              | AnchorStyles.Right;
        ZweitSymbol.Controls.Add(txbZweitsymbol);
        ZweitSymbol.Location = new Point(384, 88);
        ZweitSymbol.Name = "ZweitSymbol";
        ZweitSymbol.Size = new Size(200, 88);
        ZweitSymbol.TabIndex = 9;
        ZweitSymbol.TabStop = false;
        ZweitSymbol.Text = "Zweitsymbol";
        //
        // txbZweitsymbol
        //
        txbZweitsymbol.BorderStyle = BorderStyle.FixedSingle;
        txbZweitsymbol.Location = new Point(8, 24);
        txbZweitsymbol.Name = "txbZweitsymbol";
        txbZweitsymbol.Size = new Size(184, 20);
        txbZweitsymbol.TabIndex = 17;
        txbZweitsymbol.TextChanged += SomethingChanged;
        //
        // frmQuickPic
        //
        Controls.Add(ZweitSymbol);
        Controls.Add(Preview);
        Controls.Add(GroupBox4);
        Controls.Add(GroupBox3);
        Controls.Add(GroupBox2);
        Controls.Add(GroupBox1);
        Controls.Add(ButOK);
        Name = "QuickPic";
        Size = new Size(591, 362);
        GroupBox1.ResumeLayout(false);
        GroupBox1.PerformLayout();
        GroupBox2.ResumeLayout(false);
        GroupBox2.PerformLayout();
        GroupBox4.ResumeLayout(false);
        ((ISupportInitialize)Preview).EndInit();
        ((ISupportInitialize)SAT).EndInit();
        ((ISupportInitialize)Hell).EndInit();
        ((ISupportInitialize)Transp).EndInit();
        GroupBox3.ResumeLayout(false);
        GroupBox3.PerformLayout();
        ZweitSymbol.ResumeLayout(false);
        ZweitSymbol.PerformLayout();
        ResumeLayout(false);
    }

    private void LB_DoubleClick(object sender, System.EventArgs e) => PicName.Text = Convert.ToString(LB.SelectedItem);

    private void SomethingChanged(object sender, System.EventArgs e) {
        Helll.Text = Hell.Value + "%";
        SATL.Text = SAT.Value + "%";
        Transpl.Text = Transp.Value + "%";
        GeneratePreview();
    }

    #endregion
}