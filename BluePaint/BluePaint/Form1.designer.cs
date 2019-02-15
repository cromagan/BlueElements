using BlueControls;
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
            this.TabControl1 = new TabControl();
            this.Tab_Start = new TabPage();
            this.Tab_Werkzeug = new TabPage();
            this.OK = new Button();
            this.GroupBox4 = new GroupBox();
            this.Dummy = new Button();
            this.Screenshot = new Button();
            this.GroupBox2 = new GroupBox();
            this.Kontrast = new Button();
            this.Spiegeln = new Button();
            this.Bruchlinie = new Button();
            this.Clipping = new Button();
            this.GroupBox3 = new GroupBox();
            this.Zeichnen = new Button();
            this.Radiergummi = new Button();
            this.Steuerung = new GroupBox();
            this.Rückg = new Button();
            this.P = new System.Windows.Forms.PictureBox();
            this.Split = new System.Windows.Forms.SplitContainer();
            this.BLupe = new GroupBox();
            this.InfoText = new Caption();
            this.Lupe = new System.Windows.Forms.PictureBox();
            this.TabControl1.SuspendLayout();
            this.Tab_Werkzeug.SuspendLayout();
            this.GroupBox4.SuspendLayout();
            this.GroupBox2.SuspendLayout();
            this.GroupBox3.SuspendLayout();
            this.Steuerung.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.P)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Split)).BeginInit();
            this.Split.Panel1.SuspendLayout();
            this.Split.Panel2.SuspendLayout();
            this.Split.SuspendLayout();
            this.BLupe.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Lupe)).BeginInit();
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
            this.Tab_Start.Location = new System.Drawing.Point(4, 25);
            this.Tab_Start.Name = "Tab_Start";
            this.Tab_Start.Size = new System.Drawing.Size(999, 81);
            this.Tab_Start.TabIndex = 0;
            this.Tab_Start.Text = "Start";
            // 
            // Tab_Werkzeug
            // 
            this.Tab_Werkzeug.Controls.Add(this.OK);
            this.Tab_Werkzeug.Controls.Add(this.GroupBox4);
            this.Tab_Werkzeug.Controls.Add(this.GroupBox2);
            this.Tab_Werkzeug.Controls.Add(this.GroupBox3);
            this.Tab_Werkzeug.Controls.Add(this.Steuerung);
            this.Tab_Werkzeug.Location = new System.Drawing.Point(4, 25);
            this.Tab_Werkzeug.Name = "Tab_Werkzeug";
            this.Tab_Werkzeug.Size = new System.Drawing.Size(999, 81);
            this.Tab_Werkzeug.TabIndex = 1;
            this.Tab_Werkzeug.Text = "Werkzeug";
            // 
            // OK
            // 
            this.OK.ImageCode = "Häkchen";
            this.OK.Location = new System.Drawing.Point(624, 0);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(56, 72);
            this.OK.TabIndex = 1;
            this.OK.Text = "OK";
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // GroupBox4
            // 
            this.GroupBox4.CausesValidation = false;
            this.GroupBox4.Controls.Add(this.Dummy);
            this.GroupBox4.Controls.Add(this.Screenshot);
            this.GroupBox4.Dock = System.Windows.Forms.DockStyle.Left;
            this.GroupBox4.Location = new System.Drawing.Point(464, 0);
            this.GroupBox4.Name = "GroupBox4";
            this.GroupBox4.Size = new System.Drawing.Size(152, 81);
            this.GroupBox4.Text = "Neu";
            // 
            // Dummy
            // 
            this.Dummy.ImageCode = "Fragezeichen";
            this.Dummy.Location = new System.Drawing.Point(64, 2);
            this.Dummy.Name = "Dummy";
            this.Dummy.Size = new System.Drawing.Size(48, 66);
            this.Dummy.TabIndex = 1;
            this.Dummy.Text = "Dummy";
            this.Dummy.Click += new System.EventHandler(this.Dummy_Click);
            // 
            // Screenshot
            // 
            this.Screenshot.ImageCode = "Screenshot";
            this.Screenshot.Location = new System.Drawing.Point(8, 2);
            this.Screenshot.Name = "Screenshot";
            this.Screenshot.Size = new System.Drawing.Size(48, 66);
            this.Screenshot.TabIndex = 0;
            this.Screenshot.Text = "Screen-shot";
            this.Screenshot.Click += new System.EventHandler(this.Screenshot_Click);
            // 
            // GroupBox2
            // 
            this.GroupBox2.CausesValidation = false;
            this.GroupBox2.Controls.Add(this.Kontrast);
            this.GroupBox2.Controls.Add(this.Spiegeln);
            this.GroupBox2.Controls.Add(this.Bruchlinie);
            this.GroupBox2.Controls.Add(this.Clipping);
            this.GroupBox2.Dock = System.Windows.Forms.DockStyle.Left;
            this.GroupBox2.Location = new System.Drawing.Point(224, 0);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new System.Drawing.Size(240, 81);
            this.GroupBox2.Text = "Sonstiges";
            this.GroupBox2.Click += new System.EventHandler(this.GroupBox2_Click);
            // 
            // Kontrast
            // 
            this.Kontrast.ImageCode = "Kontrast";
            this.Kontrast.Location = new System.Drawing.Point(176, 2);
            this.Kontrast.Name = "Kontrast";
            this.Kontrast.Size = new System.Drawing.Size(56, 66);
            this.Kontrast.TabIndex = 5;
            this.Kontrast.Text = "Kontrast";
            this.Kontrast.Click += new System.EventHandler(this.Kontrast_Click);
            // 
            // Spiegeln
            // 
            this.Spiegeln.ImageCode = "SpiegelnVertikal";
            this.Spiegeln.Location = new System.Drawing.Point(8, 2);
            this.Spiegeln.Name = "Spiegeln";
            this.Spiegeln.Size = new System.Drawing.Size(56, 66);
            this.Spiegeln.TabIndex = 3;
            this.Spiegeln.Text = "Spiegeln";
            this.Spiegeln.Click += new System.EventHandler(this.Spiegeln_Click);
            // 
            // Bruchlinie
            // 
            this.Bruchlinie.ImageCode = "Bruchlinie";
            this.Bruchlinie.Location = new System.Drawing.Point(120, 2);
            this.Bruchlinie.Name = "Bruchlinie";
            this.Bruchlinie.Size = new System.Drawing.Size(56, 66);
            this.Bruchlinie.TabIndex = 2;
            this.Bruchlinie.Text = "Bruch-Linie";
            this.Bruchlinie.Click += new System.EventHandler(this.Bruchlinie_Click);
            // 
            // Clipping
            // 
            this.Clipping.ImageCode = "Zuschneiden";
            this.Clipping.Location = new System.Drawing.Point(64, 2);
            this.Clipping.Name = "Clipping";
            this.Clipping.Size = new System.Drawing.Size(56, 66);
            this.Clipping.TabIndex = 1;
            this.Clipping.Text = "Zuschnei-den";
            this.Clipping.Click += new System.EventHandler(this.Clipping_Click);
            // 
            // GroupBox3
            // 
            this.GroupBox3.CausesValidation = false;
            this.GroupBox3.Controls.Add(this.Zeichnen);
            this.GroupBox3.Controls.Add(this.Radiergummi);
            this.GroupBox3.Dock = System.Windows.Forms.DockStyle.Left;
            this.GroupBox3.Location = new System.Drawing.Point(88, 0);
            this.GroupBox3.Name = "GroupBox3";
            this.GroupBox3.Size = new System.Drawing.Size(136, 81);
            this.GroupBox3.Text = "Zeichnen";
            // 
            // Zeichnen
            // 
            this.Zeichnen.ImageCode = "Stift";
            this.Zeichnen.Location = new System.Drawing.Point(8, 2);
            this.Zeichnen.Name = "Zeichnen";
            this.Zeichnen.Size = new System.Drawing.Size(56, 66);
            this.Zeichnen.TabIndex = 6;
            this.Zeichnen.Text = "Zeichnen";
            this.Zeichnen.Click += new System.EventHandler(this.Zeichnen_Click);
            // 
            // Radiergummi
            // 
            this.Radiergummi.ImageCode = "Radiergummi";
            this.Radiergummi.Location = new System.Drawing.Point(64, 2);
            this.Radiergummi.Name = "Radiergummi";
            this.Radiergummi.Size = new System.Drawing.Size(56, 66);
            this.Radiergummi.TabIndex = 4;
            this.Radiergummi.Text = "Radier-gummi";
            this.Radiergummi.Click += new System.EventHandler(this.Radiergummi_Click);
            // 
            // Steuerung
            // 
            this.Steuerung.CausesValidation = false;
            this.Steuerung.Controls.Add(this.Rückg);
            this.Steuerung.Dock = System.Windows.Forms.DockStyle.Left;
            this.Steuerung.Location = new System.Drawing.Point(0, 0);
            this.Steuerung.Name = "Steuerung";
            this.Steuerung.Size = new System.Drawing.Size(88, 81);
            this.Steuerung.Text = "Steuerung";
            // 
            // Rückg
            // 
            this.Rückg.Enabled = false;
            this.Rückg.ImageCode = "Undo";
            this.Rückg.Location = new System.Drawing.Point(8, 2);
            this.Rückg.Name = "Rückg";
            this.Rückg.Size = new System.Drawing.Size(72, 66);
            this.Rückg.TabIndex = 7;
            this.Rückg.Text = "Rückgängig";
            this.Rückg.Click += new System.EventHandler(this.Rückg_Click);
            // 
            // P
            // 
            this.P.Dock = System.Windows.Forms.DockStyle.Fill;
            this.P.Location = new System.Drawing.Point(0, 0);
            this.P.Name = "P";
            this.P.Size = new System.Drawing.Size(722, 340);
            this.P.TabIndex = 2;
            this.P.TabStop = false;
            this.P.SizeChanged += new System.EventHandler(this.P_SizeChanged);
            this.P.MouseDown += new System.Windows.Forms.MouseEventHandler(this.P_MouseDown);
            this.P.MouseLeave += new System.EventHandler(this.P_MouseLeave);
            this.P.MouseMove += new System.Windows.Forms.MouseEventHandler(this.P_MouseMove);
            this.P.MouseUp += new System.Windows.Forms.MouseEventHandler(this.P_MouseUp);
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
            this.BLupe.Controls.Add(this.Lupe);
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
            this.InfoText.Location = new System.Drawing.Point(160, 16);
            this.InfoText.Name = "InfoText";
            this.InfoText.Size = new System.Drawing.Size(104, 144);
            // 
            // Lupe
            // 
            this.Lupe.Location = new System.Drawing.Point(8, 16);
            this.Lupe.Name = "Lupe";
            this.Lupe.Size = new System.Drawing.Size(144, 144);
            this.Lupe.TabIndex = 3;
            this.Lupe.TabStop = false;
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
            this.Tab_Werkzeug.ResumeLayout(false);
            this.GroupBox4.ResumeLayout(false);
            this.GroupBox2.ResumeLayout(false);
            this.GroupBox3.ResumeLayout(false);
            this.Steuerung.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.P)).EndInit();
            this.Split.Panel1.ResumeLayout(false);
            this.Split.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Split)).EndInit();
            this.Split.ResumeLayout(false);
            this.BLupe.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Lupe)).EndInit();
            this.ResumeLayout(false);

        }

        internal TabControl TabControl1;
        internal TabPage Tab_Start;
        internal TabPage Tab_Werkzeug;
        internal System.Windows.Forms.PictureBox P;
        internal GroupBox GroupBox2;
        internal Button Clipping;
        internal Button Screenshot;
        internal System.Windows.Forms.SplitContainer Split;
        internal Button Bruchlinie;
        internal Button Spiegeln;
        internal Button Zeichnen;
        internal Button Kontrast;
        internal Button Radiergummi;
        internal Button Rückg;
        internal GroupBox GroupBox4;
        internal GroupBox GroupBox3;
        internal GroupBox Steuerung;
        internal Button OK;
        internal GroupBox BLupe;
        internal System.Windows.Forms.PictureBox Lupe;
        internal Caption InfoText;
        internal Button Dummy;
        #endregion
    }
}