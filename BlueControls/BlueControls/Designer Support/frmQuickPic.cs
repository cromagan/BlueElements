// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace BlueControls.Designer_Support {

    internal sealed class frmQuickPic : Panel {

        #region Fields

        internal Button ButOK;

        private CheckBox chkbDurchgestrichen;

        private CheckBox chkbGrauStufen;

        private CheckBox chkbMEDisabled;

        private CheckBox chkbXPDisabled;

        private TextBox Färb;

        private GroupBox GroupBox1;

        private GroupBox GroupBox2;

        private GroupBox GroupBox3;

        private GroupBox GroupBox4;

        private TextBox grün;

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

        public frmQuickPic() =>
                                                                                                                                                                                                                                                            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
                                                                                                                                                                                                                                                            InitializeComponent();

        #endregion

        // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen

        #region Methods

        public void GeneratePreview() {
            try {
                Preview.Image = QuickImage.Get(ICode()).BMP;
            } catch {
                Preview.Image = null;
            }
        }

        public string ICode() {
            var e = (enImageCodeEffect)(((chkbGrauStufen.Checked ? -1 : 0) * -(int)enImageCodeEffect.Graustufen) | ((chkbDurchgestrichen.Checked ? -1 : 0) * -(int)enImageCodeEffect.Durchgestrichen) | ((chkbMEDisabled.Checked ? -1 : 0) * -(int)enImageCodeEffect.WindowsMEDisabled) | ((chkbXPDisabled.Checked ? -1 : 0) * -(int)enImageCodeEffect.WindowsXPDisabled));
            return QuickImage.GenerateCode(PicName.Text, int.Parse(GrX.Text), int.Parse(GrY.Text), e, Färb.Text, grün.Text, SAT.Value, Hell.Value, 0, Transp.Value, txbZweitsymbol.Text);
        }

        public void StartAll(string C) {
            LB.Items.Clear();
            const enImageCode tempVar = (enImageCode)9999;
            for (enImageCode z = 0; z <= tempVar; z++) {
                var w = Enum.GetName(z.GetType(), z);
                if (!string.IsNullOrEmpty(w)) { LB.Items.Add(w); }
            }
            QuickImage l = new(C);
            PicName.Text = l.Name;
            Färb.Text = l.Färbung;
            grün.Text = l.ChangeGreenTo;
            chkbGrauStufen.Checked = Convert.ToBoolean(l.Effekt & enImageCodeEffect.Graustufen);
            SAT.Value = l.Sättigung;
            Hell.Value = l.Helligkeit;
            Transp.Value = l.Transparenz;
            if (l.Effekt < 0) { l.Effekt = 0; }
            chkbDurchgestrichen.Checked = Convert.ToBoolean(l.Effekt & enImageCodeEffect.Durchgestrichen);
            chkbMEDisabled.Checked = Convert.ToBoolean(l.Effekt & enImageCodeEffect.WindowsMEDisabled);
            chkbXPDisabled.Checked = Convert.ToBoolean(l.Effekt & enImageCodeEffect.WindowsXPDisabled);
            GrX.Text = l.Width.ToString();
            GrY.Text = l.Height.ToString();
            txbZweitsymbol.Text = l.Zweitsymbol;
        }

        // Die Form überschreibt den Deletevorgang der Basisklasse, um Komponenten zu bereinigen.
        protected override void Dispose(bool NowDisposing) {
            if (NowDisposing) { }
            base.Dispose(NowDisposing);
        }

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
            Färb = new TextBox();
            grün = new TextBox();
            GroupBox3 = new GroupBox();
            ZweitSymbol = new GroupBox();
            txbZweitsymbol = new TextBox();
            GroupBox1.SuspendLayout();
            GroupBox2.SuspendLayout();
            GroupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Preview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)SAT).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Hell).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Transp).BeginInit();
            GroupBox3.SuspendLayout();
            ZweitSymbol.SuspendLayout();
            SuspendLayout();
            //
            // LB
            //
            LB.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
            | AnchorStyles.Left
            | AnchorStyles.Right;
            LB.Location = new System.Drawing.Point(8, 16);
            LB.Name = "LB";
            LB.Size = new System.Drawing.Size(192, 303);
            LB.TabIndex = 0;
            LB.DoubleClick += new EventHandler(LB_DoubleClick);
            //
            // ButOK
            //
            ButOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            ButOK.Location = new System.Drawing.Point(519, 331);
            ButOK.Name = "ButOK";
            ButOK.Size = new System.Drawing.Size(64, 24);
            ButOK.TabIndex = 1;
            ButOK.Text = "OK";
            //
            // PicName
            //
            PicName.Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            | AnchorStyles.Right;
            PicName.BorderStyle = BorderStyle.FixedSingle;
            PicName.Location = new System.Drawing.Point(8, 334);
            PicName.Name = "PicName";
            PicName.Size = new System.Drawing.Size(192, 20);
            PicName.TabIndex = 2;
            PicName.Text = "PicNam";
            PicName.TextChanged += new EventHandler(SomethingChanged);
            //
            // GroupBox1
            //
            GroupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
            | AnchorStyles.Left;
            GroupBox1.Controls.Add(LB);
            GroupBox1.Controls.Add(PicName);
            GroupBox1.Location = new System.Drawing.Point(0, 0);
            GroupBox1.Name = "GroupBox1";
            GroupBox1.Size = new System.Drawing.Size(208, 358);
            GroupBox1.TabIndex = 3;
            GroupBox1.TabStop = false;
            GroupBox1.Text = "Name";
            //
            // GroupBox2
            //
            GroupBox2.Controls.Add(GrY);
            GroupBox2.Controls.Add(GrX);
            GroupBox2.Controls.Add(Label1);
            GroupBox2.Location = new System.Drawing.Point(304, 8);
            GroupBox2.Name = "GroupBox2";
            GroupBox2.Size = new System.Drawing.Size(160, 48);
            GroupBox2.TabIndex = 4;
            GroupBox2.TabStop = false;
            GroupBox2.Text = "Abmessung";
            //
            // GrY
            //
            GrY.BorderStyle = BorderStyle.FixedSingle;
            GrY.Location = new System.Drawing.Point(88, 16);
            GrY.Name = "GrY";
            GrY.Size = new System.Drawing.Size(24, 20);
            GrY.TabIndex = 2;
            GrY.Text = "16";
            GrY.TextChanged += new EventHandler(SomethingChanged);
            //
            // GrX
            //
            GrX.BorderStyle = BorderStyle.FixedSingle;
            GrX.Location = new System.Drawing.Point(48, 16);
            GrX.Name = "GrX";
            GrX.Size = new System.Drawing.Size(24, 20);
            GrX.TabIndex = 0;
            GrX.Text = "16";
            GrX.TextChanged += new EventHandler(SomethingChanged);
            //
            // Label1
            //
            Label1.Location = new System.Drawing.Point(8, 16);
            Label1.Name = "Label1";
            Label1.Size = new System.Drawing.Size(144, 16);
            Label1.TabIndex = 1;
            Label1.Text = "Größe            x            Pixel";
            //
            // chkbDurchgestrichen
            //
            chkbDurchgestrichen.FlatStyle = FlatStyle.Flat;
            chkbDurchgestrichen.Location = new System.Drawing.Point(8, 16);
            chkbDurchgestrichen.Name = "chkbDurchgestrichen";
            chkbDurchgestrichen.Size = new System.Drawing.Size(120, 16);
            chkbDurchgestrichen.TabIndex = 0;
            chkbDurchgestrichen.Text = "Durchgestrichen";
            chkbDurchgestrichen.CheckedChanged += new EventHandler(SomethingCheckedChanged);
            //
            // chkbGrauStufen
            //
            chkbGrauStufen.FlatStyle = FlatStyle.Flat;
            chkbGrauStufen.Location = new System.Drawing.Point(8, 64);
            chkbGrauStufen.Name = "chkbGrauStufen";
            chkbGrauStufen.Size = new System.Drawing.Size(152, 16);
            chkbGrauStufen.TabIndex = 15;
            chkbGrauStufen.Text = "Bild in Graustufen anzeigen";
            chkbGrauStufen.CheckedChanged += new EventHandler(SomethingCheckedChanged);
            //
            // chkbMEDisabled
            //
            chkbMEDisabled.FlatStyle = FlatStyle.Flat;
            chkbMEDisabled.Location = new System.Drawing.Point(8, 32);
            chkbMEDisabled.Name = "chkbMEDisabled";
            chkbMEDisabled.Size = new System.Drawing.Size(136, 16);
            chkbMEDisabled.TabIndex = 1;
            chkbMEDisabled.Text = "Windows ME disabled";
            chkbMEDisabled.CheckedChanged += new EventHandler(SomethingCheckedChanged);
            //
            // chkbXPDisabled
            //
            chkbXPDisabled.FlatStyle = FlatStyle.Flat;
            chkbXPDisabled.Location = new System.Drawing.Point(8, 48);
            chkbXPDisabled.Name = "chkbXPDisabled";
            chkbXPDisabled.Size = new System.Drawing.Size(136, 16);
            chkbXPDisabled.TabIndex = 2;
            chkbXPDisabled.Text = "Windows XP disabled";
            chkbXPDisabled.CheckedChanged += new EventHandler(SomethingCheckedChanged);
            //
            // GroupBox4
            //
            GroupBox4.Controls.Add(chkbXPDisabled);
            GroupBox4.Controls.Add(chkbMEDisabled);
            GroupBox4.Controls.Add(chkbGrauStufen);
            GroupBox4.Controls.Add(chkbDurchgestrichen);
            GroupBox4.Location = new System.Drawing.Point(216, 88);
            GroupBox4.Name = "GroupBox4";
            GroupBox4.Size = new System.Drawing.Size(160, 88);
            GroupBox4.TabIndex = 7;
            GroupBox4.TabStop = false;
            GroupBox4.Text = "Effekt";
            //
            // Preview
            //
            Preview.Location = new System.Drawing.Point(216, 8);
            Preview.Name = "Preview";
            Preview.Size = new System.Drawing.Size(80, 72);
            Preview.TabIndex = 8;
            Preview.TabStop = false;
            //
            // SAT
            //
            SAT.AutoSize = false;
            SAT.LargeChange = 1;
            SAT.Location = new System.Drawing.Point(80, 8);
            SAT.Maximum = 200;
            SAT.Name = "SAT";
            SAT.Size = new System.Drawing.Size(240, 32);
            SAT.TabIndex = 5;
            SAT.TickFrequency = 10;
            SAT.Value = 100;
            SAT.ValueChanged += new EventHandler(SomethingChanged);
            //
            // Label2
            //
            Label2.Location = new System.Drawing.Point(8, 16);
            Label2.Name = "Label2";
            Label2.Size = new System.Drawing.Size(56, 16);
            Label2.TabIndex = 6;
            Label2.Text = "Sättigung:";
            //
            // SATL
            //
            SATL.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            SATL.Location = new System.Drawing.Point(320, 16);
            SATL.Name = "SATL";
            SATL.Size = new System.Drawing.Size(40, 16);
            SATL.TabIndex = 7;
            SATL.Text = "100%";
            //
            // Hell
            //
            Hell.AutoSize = false;
            Hell.LargeChange = 1;
            Hell.Location = new System.Drawing.Point(80, 40);
            Hell.Maximum = 200;
            Hell.Name = "Hell";
            Hell.Size = new System.Drawing.Size(240, 32);
            Hell.TabIndex = 8;
            Hell.TickFrequency = 10;
            Hell.Value = 100;
            Hell.ValueChanged += new EventHandler(SomethingChanged);
            //
            // Label4
            //
            Label4.Location = new System.Drawing.Point(8, 40);
            Label4.Name = "Label4";
            Label4.Size = new System.Drawing.Size(56, 16);
            Label4.TabIndex = 9;
            Label4.Text = "Helligkeit:";
            //
            // Transp
            //
            Transp.AutoSize = false;
            Transp.LargeChange = 1;
            Transp.Location = new System.Drawing.Point(80, 72);
            Transp.Maximum = 99;
            Transp.Name = "Transp";
            Transp.Size = new System.Drawing.Size(240, 32);
            Transp.TabIndex = 8;
            Transp.TickFrequency = 10;
            Transp.ValueChanged += new EventHandler(SomethingChanged);
            //
            // Helll
            //
            Helll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            Helll.Location = new System.Drawing.Point(320, 40);
            Helll.Name = "Helll";
            Helll.Size = new System.Drawing.Size(40, 16);
            Helll.TabIndex = 10;
            Helll.Text = "100%";
            //
            // Label6
            //
            Label6.Location = new System.Drawing.Point(8, 72);
            Label6.Name = "Label6";
            Label6.Size = new System.Drawing.Size(72, 16);
            Label6.TabIndex = 9;
            Label6.Text = "Transparenz:";
            //
            // Transpl
            //
            Transpl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            Transpl.Location = new System.Drawing.Point(320, 72);
            Transpl.Name = "Transpl";
            Transpl.Size = new System.Drawing.Size(40, 16);
            Transpl.TabIndex = 10;
            Transpl.Text = "0%";
            //
            // Label3
            //
            Label3.Location = new System.Drawing.Point(8, 112);
            Label3.Name = "Label3";
            Label3.Size = new System.Drawing.Size(56, 16);
            Label3.TabIndex = 11;
            Label3.Text = "Färbung:";
            //
            // Label5
            //
            Label5.Location = new System.Drawing.Point(128, 112);
            Label5.Name = "Label5";
            Label5.Size = new System.Drawing.Size(72, 16);
            Label5.TabIndex = 13;
            Label5.Text = "Grün wird zu:";
            //
            // Färb
            //
            Färb.BorderStyle = BorderStyle.FixedSingle;
            Färb.Location = new System.Drawing.Point(64, 112);
            Färb.Name = "Färb";
            Färb.Size = new System.Drawing.Size(48, 20);
            Färb.TabIndex = 16;
            Färb.TextChanged += new EventHandler(SomethingChanged);
            //
            // grün
            //
            grün.BorderStyle = BorderStyle.FixedSingle;
            grün.Location = new System.Drawing.Point(200, 112);
            grün.Name = "grün";
            grün.Size = new System.Drawing.Size(48, 20);
            grün.TabIndex = 17;
            grün.TextChanged += new EventHandler(SomethingChanged);
            //
            // GroupBox3
            //
            GroupBox3.Anchor = AnchorStyles.Top | AnchorStyles.Left
            | AnchorStyles.Right;
            GroupBox3.Controls.Add(grün);
            GroupBox3.Controls.Add(Färb);
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
            GroupBox3.Location = new System.Drawing.Point(216, 184);
            GroupBox3.Name = "GroupBox3";
            GroupBox3.Size = new System.Drawing.Size(368, 144);
            GroupBox3.TabIndex = 6;
            GroupBox3.TabStop = false;
            GroupBox3.Text = "Color";
            //
            // ZweitSymbol
            //
            ZweitSymbol.Anchor = AnchorStyles.Top | AnchorStyles.Left
            | AnchorStyles.Right;
            ZweitSymbol.Controls.Add(txbZweitsymbol);
            ZweitSymbol.Location = new System.Drawing.Point(384, 88);
            ZweitSymbol.Name = "ZweitSymbol";
            ZweitSymbol.Size = new System.Drawing.Size(200, 88);
            ZweitSymbol.TabIndex = 9;
            ZweitSymbol.TabStop = false;
            ZweitSymbol.Text = "Zweitsymbol";
            //
            // txbZweitsymbol
            //
            txbZweitsymbol.BorderStyle = BorderStyle.FixedSingle;
            txbZweitsymbol.Location = new System.Drawing.Point(8, 24);
            txbZweitsymbol.Name = "txbZweitsymbol";
            txbZweitsymbol.Size = new System.Drawing.Size(184, 20);
            txbZweitsymbol.TabIndex = 17;
            txbZweitsymbol.TextChanged += new EventHandler(SomethingChanged);
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
            Name = "frmQuickPic";
            Size = new System.Drawing.Size(591, 362);
            GroupBox1.ResumeLayout(false);
            GroupBox1.PerformLayout();
            GroupBox2.ResumeLayout(false);
            GroupBox2.PerformLayout();
            GroupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)Preview).EndInit();
            ((System.ComponentModel.ISupportInitialize)SAT).EndInit();
            ((System.ComponentModel.ISupportInitialize)Hell).EndInit();
            ((System.ComponentModel.ISupportInitialize)Transp).EndInit();
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

        private void SomethingCheckedChanged(object sender, System.EventArgs e) { }

        #endregion
    }
}