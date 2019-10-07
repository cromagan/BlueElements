using BlueControls.Controls;

namespace BluePaint
{
    partial class Form1 : BlueControls.Forms.Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TabControl1 = new BlueControls.Controls.TabControl();
            this.Tab_Start = new BlueControls.Controls.TabPage();
            this.grpDatei = new BlueControls.Controls.GroupBox();
            this.Tab_Werkzeug = new BlueControls.Controls.TabPage();
            this.btnOK = new BlueControls.Controls.Button();
            this.grpNeu = new BlueControls.Controls.GroupBox();
            this.btnDummy = new BlueControls.Controls.Button();
            this.btnScreenshot = new BlueControls.Controls.Button();
            this.grpSonstiges = new BlueControls.Controls.GroupBox();
            this.btnKontrast = new BlueControls.Controls.Button();
            this.btnSpiegeln = new BlueControls.Controls.Button();
            this.btnBruchlinie = new BlueControls.Controls.Button();
            this.btnClipping = new BlueControls.Controls.Button();
            this.grpZeichnen = new BlueControls.Controls.GroupBox();
            this.btnZeichnen = new BlueControls.Controls.Button();
            this.btnRadiergummi = new BlueControls.Controls.Button();
            this.grpSteuerung = new BlueControls.Controls.GroupBox();
            this.btnZoomFit = new BlueControls.Controls.Button();
            this.btnRückgänig = new BlueControls.Controls.Button();
            this.P = new BlueControls.Controls.ZoomPic();
            this.Split = new System.Windows.Forms.SplitContainer();
            this.BLupe = new BlueControls.Controls.GroupBox();
            this.InfoText = new BlueControls.Controls.Caption();
            this.TabControl1.SuspendLayout();
            this.Tab_Start.SuspendLayout();
            this.Tab_Werkzeug.SuspendLayout();
            this.grpNeu.SuspendLayout();
            this.grpSonstiges.SuspendLayout();
            this.grpZeichnen.SuspendLayout();
            this.grpSteuerung.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Split)).BeginInit();
            this.Split.Panel1.SuspendLayout();
            this.Split.Panel2.SuspendLayout();
            this.Split.SuspendLayout();
            this.BLupe.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabControl1
            // 
            this.TabControl1.Controls.Add(this.Tab_Start);
            this.TabControl1.Controls.Add(this.Tab_Werkzeug);
            this.TabControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.TabControl1.HotTrack = true;
            this.TabControl1.IsRibbonBar = true;
            this.TabControl1.Location = new System.Drawing.Point(0, 0);
            this.TabControl1.Name = "TabControl1";
            this.TabControl1.SelectedIndex = 1;
            this.TabControl1.Size = new System.Drawing.Size(1007, 110);
            this.TabControl1.TabIndex = 0;
            // 
            // Tab_Start
            // 
            this.Tab_Start.Controls.Add(this.grpDatei);
            this.Tab_Start.Location = new System.Drawing.Point(4, 25);
            this.Tab_Start.Name = "Tab_Start";
            this.Tab_Start.Size = new System.Drawing.Size(999, 81);
            this.Tab_Start.TabIndex = 0;
            this.Tab_Start.Text = "Start";
            // 
            // grpDatei
            // 
            this.grpDatei.CausesValidation = false;
            this.grpDatei.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpDatei.Location = new System.Drawing.Point(0, 0);
            this.grpDatei.Name = "grpDatei";
            this.grpDatei.Size = new System.Drawing.Size(288, 81);
            this.grpDatei.Text = "Dateisystem";
            // 
            // Tab_Werkzeug
            // 
            this.Tab_Werkzeug.Controls.Add(this.btnOK);
            this.Tab_Werkzeug.Controls.Add(this.grpNeu);
            this.Tab_Werkzeug.Controls.Add(this.grpSonstiges);
            this.Tab_Werkzeug.Controls.Add(this.grpZeichnen);
            this.Tab_Werkzeug.Controls.Add(this.grpSteuerung);
            this.Tab_Werkzeug.Location = new System.Drawing.Point(4, 25);
            this.Tab_Werkzeug.Name = "Tab_Werkzeug";
            this.Tab_Werkzeug.Size = new System.Drawing.Size(999, 81);
            this.Tab_Werkzeug.TabIndex = 1;
            this.Tab_Werkzeug.Text = "Werkzeug";
            // 
            // btnOK
            // 
            this.btnOK.ImageCode = "Häkchen";
            this.btnOK.Location = new System.Drawing.Point(680, 0);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(56, 72);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.OK_Click);
            // 
            // grpNeu
            // 
            this.grpNeu.CausesValidation = false;
            this.grpNeu.Controls.Add(this.btnDummy);
            this.grpNeu.Controls.Add(this.btnScreenshot);
            this.grpNeu.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpNeu.Location = new System.Drawing.Point(520, 0);
            this.grpNeu.Name = "grpNeu";
            this.grpNeu.Size = new System.Drawing.Size(152, 81);
            this.grpNeu.Text = "Neu";
            // 
            // btnDummy
            // 
            this.btnDummy.ImageCode = "Fragezeichen";
            this.btnDummy.Location = new System.Drawing.Point(64, 2);
            this.btnDummy.Name = "btnDummy";
            this.btnDummy.Size = new System.Drawing.Size(48, 66);
            this.btnDummy.TabIndex = 1;
            this.btnDummy.Text = "Dummy";
            this.btnDummy.Click += new System.EventHandler(this.Dummy_Click);
            // 
            // btnScreenshot
            // 
            this.btnScreenshot.ImageCode = "Screenshot";
            this.btnScreenshot.Location = new System.Drawing.Point(8, 2);
            this.btnScreenshot.Name = "btnScreenshot";
            this.btnScreenshot.Size = new System.Drawing.Size(48, 66);
            this.btnScreenshot.TabIndex = 0;
            this.btnScreenshot.Text = "Screen-shot";
            this.btnScreenshot.Click += new System.EventHandler(this.Screenshot_Click);
            // 
            // grpSonstiges
            // 
            this.grpSonstiges.CausesValidation = false;
            this.grpSonstiges.Controls.Add(this.btnKontrast);
            this.grpSonstiges.Controls.Add(this.btnSpiegeln);
            this.grpSonstiges.Controls.Add(this.btnBruchlinie);
            this.grpSonstiges.Controls.Add(this.btnClipping);
            this.grpSonstiges.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpSonstiges.Location = new System.Drawing.Point(280, 0);
            this.grpSonstiges.Name = "grpSonstiges";
            this.grpSonstiges.Size = new System.Drawing.Size(240, 81);
            this.grpSonstiges.Text = "Sonstiges";
            // 
            // btnKontrast
            // 
            this.btnKontrast.ImageCode = "Kontrast";
            this.btnKontrast.Location = new System.Drawing.Point(176, 2);
            this.btnKontrast.Name = "btnKontrast";
            this.btnKontrast.Size = new System.Drawing.Size(56, 66);
            this.btnKontrast.TabIndex = 5;
            this.btnKontrast.Text = "Kontrast";
            this.btnKontrast.Click += new System.EventHandler(this.Kontrast_Click);
            // 
            // btnSpiegeln
            // 
            this.btnSpiegeln.ImageCode = "SpiegelnVertikal";
            this.btnSpiegeln.Location = new System.Drawing.Point(8, 2);
            this.btnSpiegeln.Name = "btnSpiegeln";
            this.btnSpiegeln.Size = new System.Drawing.Size(56, 66);
            this.btnSpiegeln.TabIndex = 3;
            this.btnSpiegeln.Text = "Spiegeln";
            this.btnSpiegeln.Click += new System.EventHandler(this.Spiegeln_Click);
            // 
            // btnBruchlinie
            // 
            this.btnBruchlinie.ImageCode = "Bruchlinie";
            this.btnBruchlinie.Location = new System.Drawing.Point(120, 2);
            this.btnBruchlinie.Name = "btnBruchlinie";
            this.btnBruchlinie.Size = new System.Drawing.Size(56, 66);
            this.btnBruchlinie.TabIndex = 2;
            this.btnBruchlinie.Text = "Bruch-Linie";
            this.btnBruchlinie.Click += new System.EventHandler(this.Bruchlinie_Click);
            // 
            // btnClipping
            // 
            this.btnClipping.ImageCode = "Zuschneiden";
            this.btnClipping.Location = new System.Drawing.Point(64, 2);
            this.btnClipping.Name = "btnClipping";
            this.btnClipping.Size = new System.Drawing.Size(56, 66);
            this.btnClipping.TabIndex = 1;
            this.btnClipping.Text = "Zuschnei-den";
            this.btnClipping.Click += new System.EventHandler(this.Clipping_Click);
            // 
            // grpZeichnen
            // 
            this.grpZeichnen.CausesValidation = false;
            this.grpZeichnen.Controls.Add(this.btnZeichnen);
            this.grpZeichnen.Controls.Add(this.btnRadiergummi);
            this.grpZeichnen.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpZeichnen.Location = new System.Drawing.Point(144, 0);
            this.grpZeichnen.Name = "grpZeichnen";
            this.grpZeichnen.Size = new System.Drawing.Size(136, 81);
            this.grpZeichnen.Text = "Zeichnen";
            // 
            // btnZeichnen
            // 
            this.btnZeichnen.ImageCode = "Stift";
            this.btnZeichnen.Location = new System.Drawing.Point(8, 2);
            this.btnZeichnen.Name = "btnZeichnen";
            this.btnZeichnen.Size = new System.Drawing.Size(56, 66);
            this.btnZeichnen.TabIndex = 6;
            this.btnZeichnen.Text = "Zeichnen";
            this.btnZeichnen.Click += new System.EventHandler(this.Zeichnen_Click);
            // 
            // btnRadiergummi
            // 
            this.btnRadiergummi.ImageCode = "Radiergummi";
            this.btnRadiergummi.Location = new System.Drawing.Point(64, 2);
            this.btnRadiergummi.Name = "btnRadiergummi";
            this.btnRadiergummi.Size = new System.Drawing.Size(56, 66);
            this.btnRadiergummi.TabIndex = 4;
            this.btnRadiergummi.Text = "Radier-gummi";
            this.btnRadiergummi.Click += new System.EventHandler(this.Radiergummi_Click);
            // 
            // grpSteuerung
            // 
            this.grpSteuerung.CausesValidation = false;
            this.grpSteuerung.Controls.Add(this.btnZoomFit);
            this.grpSteuerung.Controls.Add(this.btnRückgänig);
            this.grpSteuerung.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpSteuerung.Location = new System.Drawing.Point(0, 0);
            this.grpSteuerung.Name = "grpSteuerung";
            this.grpSteuerung.Size = new System.Drawing.Size(144, 81);
            this.grpSteuerung.Text = "Steuerung";
            // 
            // btnZoomFit
            // 
            this.btnZoomFit.ImageCode = "ZoomFit";
            this.btnZoomFit.Location = new System.Drawing.Point(88, 2);
            this.btnZoomFit.Name = "btnZoomFit";
            this.btnZoomFit.Size = new System.Drawing.Size(48, 66);
            this.btnZoomFit.TabIndex = 8;
            this.btnZoomFit.Text = "ein-passen";
            this.btnZoomFit.Click += new System.EventHandler(this.btnZoomFit_Click);
            // 
            // btnRückgänig
            // 
            this.btnRückgänig.Enabled = false;
            this.btnRückgänig.ImageCode = "Undo";
            this.btnRückgänig.Location = new System.Drawing.Point(8, 2);
            this.btnRückgänig.Name = "btnRückgänig";
            this.btnRückgänig.Size = new System.Drawing.Size(72, 66);
            this.btnRückgänig.TabIndex = 7;
            this.btnRückgänig.Text = "Rückgängig";
            this.btnRückgänig.Click += new System.EventHandler(this.Rückg_Click);
            // 
            // P
            // 
            this.P.AlwaysSmooth = false;
            this.P.Dock = System.Windows.Forms.DockStyle.Fill;
            this.P.Location = new System.Drawing.Point(0, 0);
            this.P.Mittellinie = BlueBasics.Enums.enOrientation.Ohne;
            this.P.Name = "P";
            this.P.Size = new System.Drawing.Size(722, 340);
            this.P.TabIndex = 2;
            this.P.TabStop = false;
            this.P.Translate = false;
            this.P.ImageMouseDown += new System.EventHandler<System.Windows.Forms.MouseEventArgs>(this.P_ImageMouseDown);
            this.P.ImageMouseMove += new System.EventHandler<System.Windows.Forms.MouseEventArgs>(this.P_ImageMouseMove);
            this.P.ImageMouseUp += new System.EventHandler<System.Windows.Forms.MouseEventArgs>(this.P_ImageMouseUp);
            this.P.ImageMouseLeave += new System.EventHandler(this.P_ImageMouseLeave);
            // 
            // Split
            // 
            this.Split.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Split.Location = new System.Drawing.Point(0, 110);
            this.Split.Name = "Split";
            // 
            // Split.Panel1
            // 
            this.Split.Panel1.Controls.Add(this.BLupe);
            // 
            // Split.Panel2
            // 
            this.Split.Panel2.Controls.Add(this.P);
            this.Split.Size = new System.Drawing.Size(1007, 340);
            this.Split.SplitterDistance = 275;
            this.Split.SplitterWidth = 10;
            this.Split.TabIndex = 3;
            // 
            // BLupe
            // 
            this.BLupe.CausesValidation = false;
            this.BLupe.Controls.Add(this.InfoText);
            this.BLupe.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BLupe.Location = new System.Drawing.Point(0, 176);
            this.BLupe.Name = "BLupe";
            this.BLupe.Size = new System.Drawing.Size(275, 164);
            this.BLupe.Text = "Information";
            // 
            // InfoText
            // 
            this.InfoText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InfoText.CausesValidation = false;
            this.InfoText.Location = new System.Drawing.Point(8, 16);
            this.InfoText.Name = "InfoText";
            this.InfoText.Size = new System.Drawing.Size(256, 144);
            this.InfoText.Translate = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1007, 450);
            this.Controls.Add(this.Split);
            this.Controls.Add(this.TabControl1);
            this.Name = "Form1";
            this.Text = "BluePaint";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.TabControl1.ResumeLayout(false);
            this.Tab_Start.ResumeLayout(false);
            this.Tab_Werkzeug.ResumeLayout(false);
            this.grpNeu.ResumeLayout(false);
            this.grpSonstiges.ResumeLayout(false);
            this.grpZeichnen.ResumeLayout(false);
            this.grpSteuerung.ResumeLayout(false);
            this.Split.Panel1.ResumeLayout(false);
            this.Split.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Split)).EndInit();
            this.Split.ResumeLayout(false);
            this.BLupe.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        internal TabControl TabControl1;
        internal TabPage Tab_Start;
        internal TabPage Tab_Werkzeug;
        internal ZoomPic P;
        internal GroupBox grpSonstiges;
        internal Button btnClipping;
        internal Button btnScreenshot;
        internal System.Windows.Forms.SplitContainer Split;
        internal Button btnBruchlinie;
        internal Button btnSpiegeln;
        internal Button btnZeichnen;
        internal Button btnKontrast;
        internal Button btnRadiergummi;
        internal Button btnRückgänig;
        internal GroupBox grpNeu;
        internal GroupBox grpZeichnen;
        internal GroupBox grpSteuerung;
        internal Button btnOK;
        internal GroupBox BLupe;
        internal Caption InfoText;
        internal Button btnDummy;
        #endregion

        private GroupBox grpDatei;
        private Button btnZoomFit;
    }
}