#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion


using System;
using System.Diagnostics;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;

namespace BlueControls.Designer_Support
{
    internal sealed class frmQuickPic : Panel
    {
        #region  Vom Windows Form Designer generierter Code 

        public frmQuickPic()
        {

            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();

            // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen
        }

        // Die Form überschreibt den Deletevorgang der Basisklasse, um Komponenten zu bereinigen.
        protected override void Dispose(bool NowDisposing)
        {
            if (NowDisposing)
            {

            }
            base.Dispose(NowDisposing);
        }



        //HINWEIS: Die folgende Prozedur ist für den Windows Form-Designer erforderlich
        //Sie kann mit dem Windows Form-Designer modifiziert werden.
        //Verwenden Sie nicht den Code-Editor zur Bearbeitung.
        private ListBox LB;
        private GroupBox GroupBox1;
        private GroupBox GroupBox2;
        private Label Label1;
        private TextBox GrY;
        private TextBox GrX;
        internal Button ButOK;
        private CheckBox chkbDurchgestrichen;
        private CheckBox chkbGrauStufen;
        private CheckBox chkbMEDisabled;
        private CheckBox chkbXPDisabled;
        private GroupBox GroupBox4;
        private PictureBox Preview;
        private TrackBar SAT;
        private Label Label2;
        private Label SATL;
        private TrackBar Hell;
        private Label Label4;
        private TrackBar Transp;
        private Label Helll;
        private Label Label6;
        private Label Transpl;
        private Label Label3;
        private Label Label5;
        private TextBox Färb;
        private TextBox grün;
        private GroupBox GroupBox3;
        private GroupBox ZweitSymbol;
        private TextBox txbZweitsymbol;
        private TextBox PicName;

        [DebuggerStepThrough]
        private void InitializeComponent()
        {
            this.LB = new System.Windows.Forms.ListBox();
            this.ButOK = new System.Windows.Forms.Button();
            this.PicName = new System.Windows.Forms.TextBox();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.GroupBox2 = new System.Windows.Forms.GroupBox();
            this.GrY = new System.Windows.Forms.TextBox();
            this.GrX = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.chkbDurchgestrichen = new System.Windows.Forms.CheckBox();
            this.chkbGrauStufen = new System.Windows.Forms.CheckBox();
            this.chkbMEDisabled = new System.Windows.Forms.CheckBox();
            this.chkbXPDisabled = new System.Windows.Forms.CheckBox();
            this.GroupBox4 = new System.Windows.Forms.GroupBox();
            this.Preview = new System.Windows.Forms.PictureBox();
            this.SAT = new System.Windows.Forms.TrackBar();
            this.Label2 = new System.Windows.Forms.Label();
            this.SATL = new System.Windows.Forms.Label();
            this.Hell = new System.Windows.Forms.TrackBar();
            this.Label4 = new System.Windows.Forms.Label();
            this.Transp = new System.Windows.Forms.TrackBar();
            this.Helll = new System.Windows.Forms.Label();
            this.Label6 = new System.Windows.Forms.Label();
            this.Transpl = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.Label5 = new System.Windows.Forms.Label();
            this.Färb = new System.Windows.Forms.TextBox();
            this.grün = new System.Windows.Forms.TextBox();
            this.GroupBox3 = new System.Windows.Forms.GroupBox();
            this.ZweitSymbol = new System.Windows.Forms.GroupBox();
            this.txbZweitsymbol = new System.Windows.Forms.TextBox();
            this.GroupBox1.SuspendLayout();
            this.GroupBox2.SuspendLayout();
            this.GroupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Preview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SAT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Hell)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Transp)).BeginInit();
            this.GroupBox3.SuspendLayout();
            this.ZweitSymbol.SuspendLayout();
            this.SuspendLayout();
            // 
            // LB
            // 
            this.LB.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LB.Location = new System.Drawing.Point(8, 16);
            this.LB.Name = "LB";
            this.LB.Size = new System.Drawing.Size(192, 303);
            this.LB.TabIndex = 0;
            this.LB.DoubleClick += new System.EventHandler(this.LB_DoubleClick);
            // 
            // ButOK
            // 
            this.ButOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButOK.Location = new System.Drawing.Point(519, 331);
            this.ButOK.Name = "ButOK";
            this.ButOK.Size = new System.Drawing.Size(64, 24);
            this.ButOK.TabIndex = 1;
            this.ButOK.Text = "OK";
            // 
            // PicName
            // 
            this.PicName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PicName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PicName.Location = new System.Drawing.Point(8, 334);
            this.PicName.Name = "PicName";
            this.PicName.Size = new System.Drawing.Size(192, 20);
            this.PicName.TabIndex = 2;
            this.PicName.Text = "PicNam";
            this.PicName.TextChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // GroupBox1
            // 
            this.GroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.GroupBox1.Controls.Add(this.LB);
            this.GroupBox1.Controls.Add(this.PicName);
            this.GroupBox1.Location = new System.Drawing.Point(0, 0);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(208, 358);
            this.GroupBox1.TabIndex = 3;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Name";
            // 
            // GroupBox2
            // 
            this.GroupBox2.Controls.Add(this.GrY);
            this.GroupBox2.Controls.Add(this.GrX);
            this.GroupBox2.Controls.Add(this.Label1);
            this.GroupBox2.Location = new System.Drawing.Point(304, 8);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new System.Drawing.Size(160, 48);
            this.GroupBox2.TabIndex = 4;
            this.GroupBox2.TabStop = false;
            this.GroupBox2.Text = "Abmessung";
            // 
            // GrY
            // 
            this.GrY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.GrY.Location = new System.Drawing.Point(88, 16);
            this.GrY.Name = "GrY";
            this.GrY.Size = new System.Drawing.Size(24, 20);
            this.GrY.TabIndex = 2;
            this.GrY.Text = "16";
            this.GrY.TextChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // GrX
            // 
            this.GrX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.GrX.Location = new System.Drawing.Point(48, 16);
            this.GrX.Name = "GrX";
            this.GrX.Size = new System.Drawing.Size(24, 20);
            this.GrX.TabIndex = 0;
            this.GrX.Text = "16";
            this.GrX.TextChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // Label1
            // 
            this.Label1.Location = new System.Drawing.Point(8, 16);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(144, 16);
            this.Label1.TabIndex = 1;
            this.Label1.Text = "Größe            x            Pixel";
            // 
            // chkbDurchgestrichen
            // 
            this.chkbDurchgestrichen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkbDurchgestrichen.Location = new System.Drawing.Point(8, 16);
            this.chkbDurchgestrichen.Name = "chkbDurchgestrichen";
            this.chkbDurchgestrichen.Size = new System.Drawing.Size(120, 16);
            this.chkbDurchgestrichen.TabIndex = 0;
            this.chkbDurchgestrichen.Text = "Durchgestrichen";
            this.chkbDurchgestrichen.CheckedChanged += new System.EventHandler(this.SomethingCheckedChanged);
            // 
            // chkbGrauStufen
            // 
            this.chkbGrauStufen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkbGrauStufen.Location = new System.Drawing.Point(8, 64);
            this.chkbGrauStufen.Name = "chkbGrauStufen";
            this.chkbGrauStufen.Size = new System.Drawing.Size(152, 16);
            this.chkbGrauStufen.TabIndex = 15;
            this.chkbGrauStufen.Text = "Bild in Graustufen anzeigen";
            this.chkbGrauStufen.CheckedChanged += new System.EventHandler(this.SomethingCheckedChanged);
            // 
            // chkbMEDisabled
            // 
            this.chkbMEDisabled.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkbMEDisabled.Location = new System.Drawing.Point(8, 32);
            this.chkbMEDisabled.Name = "chkbMEDisabled";
            this.chkbMEDisabled.Size = new System.Drawing.Size(136, 16);
            this.chkbMEDisabled.TabIndex = 1;
            this.chkbMEDisabled.Text = "Windows ME disabled";
            this.chkbMEDisabled.CheckedChanged += new System.EventHandler(this.SomethingCheckedChanged);
            // 
            // chkbXPDisabled
            // 
            this.chkbXPDisabled.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkbXPDisabled.Location = new System.Drawing.Point(8, 48);
            this.chkbXPDisabled.Name = "chkbXPDisabled";
            this.chkbXPDisabled.Size = new System.Drawing.Size(136, 16);
            this.chkbXPDisabled.TabIndex = 2;
            this.chkbXPDisabled.Text = "Windows XP disabled";
            this.chkbXPDisabled.CheckedChanged += new System.EventHandler(this.SomethingCheckedChanged);
            // 
            // GroupBox4
            // 
            this.GroupBox4.Controls.Add(this.chkbXPDisabled);
            this.GroupBox4.Controls.Add(this.chkbMEDisabled);
            this.GroupBox4.Controls.Add(this.chkbGrauStufen);
            this.GroupBox4.Controls.Add(this.chkbDurchgestrichen);
            this.GroupBox4.Location = new System.Drawing.Point(216, 88);
            this.GroupBox4.Name = "GroupBox4";
            this.GroupBox4.Size = new System.Drawing.Size(160, 88);
            this.GroupBox4.TabIndex = 7;
            this.GroupBox4.TabStop = false;
            this.GroupBox4.Text = "Effekt";
            // 
            // Preview
            // 
            this.Preview.Location = new System.Drawing.Point(216, 8);
            this.Preview.Name = "Preview";
            this.Preview.Size = new System.Drawing.Size(80, 72);
            this.Preview.TabIndex = 8;
            this.Preview.TabStop = false;
            // 
            // SAT
            // 
            this.SAT.AutoSize = false;
            this.SAT.LargeChange = 1;
            this.SAT.Location = new System.Drawing.Point(80, 8);
            this.SAT.Maximum = 200;
            this.SAT.Name = "SAT";
            this.SAT.Size = new System.Drawing.Size(240, 32);
            this.SAT.TabIndex = 5;
            this.SAT.TickFrequency = 10;
            this.SAT.Value = 100;
            this.SAT.ValueChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // Label2
            // 
            this.Label2.Location = new System.Drawing.Point(8, 16);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(56, 16);
            this.Label2.TabIndex = 6;
            this.Label2.Text = "Sättigung:";
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
            // Hell
            // 
            this.Hell.AutoSize = false;
            this.Hell.LargeChange = 1;
            this.Hell.Location = new System.Drawing.Point(80, 40);
            this.Hell.Maximum = 200;
            this.Hell.Name = "Hell";
            this.Hell.Size = new System.Drawing.Size(240, 32);
            this.Hell.TabIndex = 8;
            this.Hell.TickFrequency = 10;
            this.Hell.Value = 100;
            this.Hell.ValueChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // Label4
            // 
            this.Label4.Location = new System.Drawing.Point(8, 40);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(56, 16);
            this.Label4.TabIndex = 9;
            this.Label4.Text = "Helligkeit:";
            // 
            // Transp
            // 
            this.Transp.AutoSize = false;
            this.Transp.LargeChange = 1;
            this.Transp.Location = new System.Drawing.Point(80, 72);
            this.Transp.Maximum = 99;
            this.Transp.Name = "Transp";
            this.Transp.Size = new System.Drawing.Size(240, 32);
            this.Transp.TabIndex = 8;
            this.Transp.TickFrequency = 10;
            this.Transp.ValueChanged += new System.EventHandler(this.SomethingChanged);
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
            // Label6
            // 
            this.Label6.Location = new System.Drawing.Point(8, 72);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(72, 16);
            this.Label6.TabIndex = 9;
            this.Label6.Text = "Transparenz:";
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
            // Label3
            // 
            this.Label3.Location = new System.Drawing.Point(8, 112);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(56, 16);
            this.Label3.TabIndex = 11;
            this.Label3.Text = "Färbung:";
            // 
            // Label5
            // 
            this.Label5.Location = new System.Drawing.Point(128, 112);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(72, 16);
            this.Label5.TabIndex = 13;
            this.Label5.Text = "Grün wird zu:";
            // 
            // Färb
            // 
            this.Färb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Färb.Location = new System.Drawing.Point(64, 112);
            this.Färb.Name = "Färb";
            this.Färb.Size = new System.Drawing.Size(48, 20);
            this.Färb.TabIndex = 16;
            this.Färb.TextChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // grün
            // 
            this.grün.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.grün.Location = new System.Drawing.Point(200, 112);
            this.grün.Name = "grün";
            this.grün.Size = new System.Drawing.Size(48, 20);
            this.grün.TabIndex = 17;
            this.grün.TextChanged += new System.EventHandler(this.SomethingChanged);
            // 
            // GroupBox3
            // 
            this.GroupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GroupBox3.Controls.Add(this.grün);
            this.GroupBox3.Controls.Add(this.Färb);
            this.GroupBox3.Controls.Add(this.Label5);
            this.GroupBox3.Controls.Add(this.Label3);
            this.GroupBox3.Controls.Add(this.Transpl);
            this.GroupBox3.Controls.Add(this.Label6);
            this.GroupBox3.Controls.Add(this.Helll);
            this.GroupBox3.Controls.Add(this.Transp);
            this.GroupBox3.Controls.Add(this.Label4);
            this.GroupBox3.Controls.Add(this.Hell);
            this.GroupBox3.Controls.Add(this.SATL);
            this.GroupBox3.Controls.Add(this.Label2);
            this.GroupBox3.Controls.Add(this.SAT);
            this.GroupBox3.Location = new System.Drawing.Point(216, 184);
            this.GroupBox3.Name = "GroupBox3";
            this.GroupBox3.Size = new System.Drawing.Size(368, 144);
            this.GroupBox3.TabIndex = 6;
            this.GroupBox3.TabStop = false;
            this.GroupBox3.Text = "Color";
            // 
            // ZweitSymbol
            // 
            this.ZweitSymbol.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ZweitSymbol.Controls.Add(this.txbZweitsymbol);
            this.ZweitSymbol.Location = new System.Drawing.Point(384, 88);
            this.ZweitSymbol.Name = "ZweitSymbol";
            this.ZweitSymbol.Size = new System.Drawing.Size(200, 88);
            this.ZweitSymbol.TabIndex = 9;
            this.ZweitSymbol.TabStop = false;
            this.ZweitSymbol.Text = "Zweitsymbol";
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
            // frmQuickPic
            // 
            this.Controls.Add(this.ZweitSymbol);
            this.Controls.Add(this.Preview);
            this.Controls.Add(this.GroupBox4);
            this.Controls.Add(this.GroupBox3);
            this.Controls.Add(this.GroupBox2);
            this.Controls.Add(this.GroupBox1);
            this.Controls.Add(this.ButOK);
            this.Name = "frmQuickPic";
            this.Size = new System.Drawing.Size(591, 362);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox1.PerformLayout();
            this.GroupBox2.ResumeLayout(false);
            this.GroupBox2.PerformLayout();
            this.GroupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Preview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SAT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Hell)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Transp)).EndInit();
            this.GroupBox3.ResumeLayout(false);
            this.GroupBox3.PerformLayout();
            this.ZweitSymbol.ResumeLayout(false);
            this.ZweitSymbol.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public void StartAll(string C)
        {

            LB.Items.Clear();

            const enImageCode tempVar = (enImageCode)9999;
            for (enImageCode z = 0 ; z <= tempVar ; z++)
            {
                var w = Enum.GetName(z.GetType(), z);
                if (!string.IsNullOrEmpty(w)) { LB.Items.Add(w); }
            }


            var l = new QuickImage(C);


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


        private void LB_DoubleClick(object sender, System.EventArgs e)
        {
            PicName.Text = Convert.ToString(LB.SelectedItem);
        }


        public void GeneratePreview()
        {
            try
            {
                Preview.Image = QuickImage.Get(ICode()).BMP;
            }
            catch
            {
                Preview.Image = null;
            }
        }

        public string ICode()
        {
            var e = (enImageCodeEffect)(((chkbGrauStufen.Checked ? -1 : 0) * -(int)enImageCodeEffect.Graustufen) | ((chkbDurchgestrichen.Checked ? -1 : 0) * -(int)enImageCodeEffect.Durchgestrichen) | ((chkbMEDisabled.Checked ? -1 : 0) * -(int)enImageCodeEffect.WindowsMEDisabled) | ((chkbXPDisabled.Checked ? -1 : 0) * -(int)enImageCodeEffect.WindowsXPDisabled));
            return QuickImage.GenerateCode(PicName.Text, int.Parse(GrX.Text), int.Parse(GrY.Text), e, Färb.Text, grün.Text, SAT.Value, Hell.Value, 0, Transp.Value, txbZweitsymbol.Text);
        }

        private void SomethingCheckedChanged(object sender, System.EventArgs e)
        {

        }

        private void SomethingChanged(object sender, System.EventArgs e)
        {
            Helll.Text = Hell.Value + "%";
            SATL.Text = SAT.Value + "%";
            Transpl.Text = Transp.Value + "%";
            GeneratePreview();
        }
    }
}