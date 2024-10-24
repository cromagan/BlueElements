﻿using System.ComponentModel;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.EventArgs;
using Button = BlueControls.Controls.Button;
using Form = BlueControls.Forms.Form;
using GroupBox = BlueControls.Controls.GroupBox;

namespace BluePaint
{
    partial class MainWindow : Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
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
            this.tabRibbonbar = new BlueControls.Controls.RibbonBar();
            this.Tab_Start = new System.Windows.Forms.TabPage();
            this.grpDatei = new BlueControls.Controls.GroupBox();
            this.btnEinfügen = new BlueControls.Controls.Button();
            this.btnCopy = new BlueControls.Controls.Button();
            this.btnSave = new BlueControls.Controls.Button();
            this.btnLetzteDateien = new BlueControls.Controls.LastFilesCombo();
            this.btnOeffnen = new BlueControls.Controls.Button();
            this.btnSaveAs = new BlueControls.Controls.Button();
            this.btnNeu = new BlueControls.Controls.Button();
            this.Tab_Werkzeug = new System.Windows.Forms.TabPage();
            this.btnOK = new BlueControls.Controls.Button();
            this.grpNeu = new BlueControls.Controls.GroupBox();
            this.btnDummy = new BlueControls.Controls.Button();
            this.btnScreenshot = new BlueControls.Controls.Button();
            this.grpSonstiges = new BlueControls.Controls.GroupBox();
            this.btnGrößeÄndern = new BlueControls.Controls.Button();
            this.btnKontrast = new BlueControls.Controls.Button();
            this.btnSpiegeln = new BlueControls.Controls.Button();
            this.btnBruchlinie = new BlueControls.Controls.Button();
            this.btnClipping = new BlueControls.Controls.Button();
            this.grpZeichnen = new BlueControls.Controls.GroupBox();
            this.btnZeichnen = new BlueControls.Controls.Button();
            this.btnRadiergummi = new BlueControls.Controls.Button();
            this.grpSteuerung = new BlueControls.Controls.GroupBox();
            this.btn100 = new BlueControls.Controls.Button();
            this.btnZoomFit = new BlueControls.Controls.Button();
            this.btnRückgänig = new BlueControls.Controls.Button();
            this.P = new BlueControls.Controls.ZoomPic();
            this.Split = new System.Windows.Forms.SplitContainer();
            this.BLupe = new BlueControls.Controls.GroupBox();
            this.InfoText = new BlueControls.Controls.Caption();
            this.LoadTab = new System.Windows.Forms.OpenFileDialog();
            this.SaveTab = new System.Windows.Forms.SaveFileDialog();
            this.tabRibbonbar.SuspendLayout();
            this.Tab_Start.SuspendLayout();
            this.grpDatei.SuspendLayout();
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
            // tabRibbonbar
            // 
            this.tabRibbonbar.Controls.Add(this.Tab_Start);
            this.tabRibbonbar.Controls.Add(this.Tab_Werkzeug);
            this.tabRibbonbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabRibbonbar.HotTrack = true;
            this.tabRibbonbar.Location = new System.Drawing.Point(0, 0);
            this.tabRibbonbar.Name = "tabRibbonbar";
            this.tabRibbonbar.SelectedIndex = 1;
            this.tabRibbonbar.Size = new System.Drawing.Size(1007, 110);
            this.tabRibbonbar.TabDefault = null;
            this.tabRibbonbar.TabDefaultOrder = null;
            this.tabRibbonbar.TabIndex = 0;
            // 
            // Tab_Start
            // 
            this.Tab_Start.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.Tab_Start.Controls.Add(this.grpDatei);
            this.Tab_Start.Location = new System.Drawing.Point(4, 25);
            this.Tab_Start.Name = "Tab_Start";
            this.Tab_Start.Size = new System.Drawing.Size(999, 81);
            this.Tab_Start.TabIndex = 0;
            this.Tab_Start.Text = "Start";
            // 
            // grpDatei
            // 
            this.grpDatei.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpDatei.CausesValidation = false;
            this.grpDatei.Controls.Add(this.btnEinfügen);
            this.grpDatei.Controls.Add(this.btnCopy);
            this.grpDatei.Controls.Add(this.btnSave);
            this.grpDatei.Controls.Add(this.btnLetzteDateien);
            this.grpDatei.Controls.Add(this.btnOeffnen);
            this.grpDatei.Controls.Add(this.btnSaveAs);
            this.grpDatei.Controls.Add(this.btnNeu);
            this.grpDatei.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpDatei.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpDatei.Location = new System.Drawing.Point(0, 0);
            this.grpDatei.Name = "grpDatei";
            this.grpDatei.Size = new System.Drawing.Size(544, 81);
            this.grpDatei.TabIndex = 1;
            this.grpDatei.TabStop = false;
            this.grpDatei.Text = "Datei";
            // 
            // btnEinfügen
            // 
            this.btnEinfügen.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnEinfügen.ImageCode = "Clipboard";
            this.btnEinfügen.Location = new System.Drawing.Point(440, 2);
            this.btnEinfügen.Name = "btnEinfügen";
            this.btnEinfügen.Size = new System.Drawing.Size(64, 66);
            this.btnEinfügen.TabIndex = 7;
            this.btnEinfügen.Text = "Einfügen";
            this.btnEinfügen.Click += new System.EventHandler(this.btnEinfügen_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnCopy.ImageCode = "Kopieren";
            this.btnCopy.Location = new System.Drawing.Point(376, 2);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(64, 66);
            this.btnCopy.TabIndex = 6;
            this.btnCopy.Text = "Kopieren";
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnSave
            // 
            this.btnSave.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnSave.ImageCode = "Diskette";
            this.btnSave.Location = new System.Drawing.Point(240, 2);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(64, 66);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Speichern";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnLetzteDateien
            // 
            this.btnLetzteDateien.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.btnLetzteDateien.DrawStyle = BlueControls.Enums.ComboboxStyle.RibbonBar;
            this.btnLetzteDateien.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.btnLetzteDateien.Enabled = false;
            this.btnLetzteDateien.ImageCode = "Ordner";
            this.btnLetzteDateien.Location = new System.Drawing.Point(128, 2);
            this.btnLetzteDateien.Name = "btnLetzteDateien";
            this.btnLetzteDateien.Size = new System.Drawing.Size(104, 66);
            this.btnLetzteDateien.TabIndex = 1;
            this.btnLetzteDateien.Text = "zuletzt geöffnete Dateien";
            this.btnLetzteDateien.ItemClicked += new System.EventHandler<AbstractListItemEventArgs>(this.btnLetzteDateien_ItemClicked);
            // 
            // btnOeffnen
            // 
            this.btnOeffnen.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnOeffnen.ImageCode = "Ordner";
            this.btnOeffnen.Location = new System.Drawing.Point(72, 2);
            this.btnOeffnen.Name = "btnOeffnen";
            this.btnOeffnen.Size = new System.Drawing.Size(56, 66);
            this.btnOeffnen.TabIndex = 1;
            this.btnOeffnen.Text = "Öffnen";
            this.btnOeffnen.Click += new System.EventHandler(this.btnOeffnen_Click);
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnSaveAs.ImageCode = "Diskette";
            this.btnSaveAs.Location = new System.Drawing.Point(304, 2);
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Size = new System.Drawing.Size(64, 66);
            this.btnSaveAs.TabIndex = 4;
            this.btnSaveAs.Text = "Speichern unter";
            this.btnSaveAs.Click += new System.EventHandler(this.btnSaveAs_Click);
            // 
            // btnNeu
            // 
            this.btnNeu.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnNeu.ImageCode = "Datei";
            this.btnNeu.Location = new System.Drawing.Point(8, 2);
            this.btnNeu.Name = "btnNeu";
            this.btnNeu.Size = new System.Drawing.Size(56, 66);
            this.btnNeu.TabIndex = 0;
            this.btnNeu.Text = "Neu";
            this.btnNeu.Click += new System.EventHandler(this.btnNeu_Click);
            // 
            // Tab_Werkzeug
            // 
            this.Tab_Werkzeug.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
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
            this.btnOK.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnOK.ImageCode = "Häkchen";
            this.btnOK.Location = new System.Drawing.Point(760, 0);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(56, 72);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.OK_Click);
            // 
            // grpNeu
            // 
            this.grpNeu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpNeu.CausesValidation = false;
            this.grpNeu.Controls.Add(this.btnDummy);
            this.grpNeu.Controls.Add(this.btnScreenshot);
            this.grpNeu.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpNeu.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpNeu.Location = new System.Drawing.Point(632, 0);
            this.grpNeu.Name = "grpNeu";
            this.grpNeu.Size = new System.Drawing.Size(120, 81);
            this.grpNeu.TabIndex = 2;
            this.grpNeu.TabStop = false;
            this.grpNeu.Text = "Neu";
            // 
            // btnDummy
            // 
            this.btnDummy.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
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
            this.btnScreenshot.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnScreenshot.ImageCode = "Bild||||||||||Monitor";
            this.btnScreenshot.Location = new System.Drawing.Point(8, 2);
            this.btnScreenshot.Name = "btnScreenshot";
            this.btnScreenshot.Size = new System.Drawing.Size(48, 66);
            this.btnScreenshot.TabIndex = 0;
            this.btnScreenshot.Text = "Screen-shot";
            this.btnScreenshot.Click += new System.EventHandler(this.Screenshot_Click);
            // 
            // grpSonstiges
            // 
            this.grpSonstiges.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpSonstiges.CausesValidation = false;
            this.grpSonstiges.Controls.Add(this.btnGrößeÄndern);
            this.grpSonstiges.Controls.Add(this.btnKontrast);
            this.grpSonstiges.Controls.Add(this.btnSpiegeln);
            this.grpSonstiges.Controls.Add(this.btnBruchlinie);
            this.grpSonstiges.Controls.Add(this.btnClipping);
            this.grpSonstiges.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpSonstiges.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpSonstiges.Location = new System.Drawing.Point(328, 0);
            this.grpSonstiges.Name = "grpSonstiges";
            this.grpSonstiges.Size = new System.Drawing.Size(304, 81);
            this.grpSonstiges.TabIndex = 3;
            this.grpSonstiges.TabStop = false;
            this.grpSonstiges.Text = "Sonstiges";
            // 
            // btnGrößeÄndern
            // 
            this.btnGrößeÄndern.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnGrößeÄndern.ImageCode = "Bild||||||||||ZoomFit";
            this.btnGrößeÄndern.Location = new System.Drawing.Point(8, 2);
            this.btnGrößeÄndern.Name = "btnGrößeÄndern";
            this.btnGrößeÄndern.Size = new System.Drawing.Size(56, 66);
            this.btnGrößeÄndern.TabIndex = 6;
            this.btnGrößeÄndern.Text = "Größe ändern";
            this.btnGrößeÄndern.Click += new System.EventHandler(this.btnGrößeÄndern_Click);
            // 
            // btnKontrast
            // 
            this.btnKontrast.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnKontrast.ImageCode = "Kontrast";
            this.btnKontrast.Location = new System.Drawing.Point(240, 2);
            this.btnKontrast.Name = "btnKontrast";
            this.btnKontrast.Size = new System.Drawing.Size(56, 66);
            this.btnKontrast.TabIndex = 5;
            this.btnKontrast.Text = "Kontrast";
            this.btnKontrast.Click += new System.EventHandler(this.Kontrast_Click);
            // 
            // btnSpiegeln
            // 
            this.btnSpiegeln.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnSpiegeln.ImageCode = "SpiegelnVertikal";
            this.btnSpiegeln.Location = new System.Drawing.Point(64, 2);
            this.btnSpiegeln.Name = "btnSpiegeln";
            this.btnSpiegeln.Size = new System.Drawing.Size(64, 66);
            this.btnSpiegeln.TabIndex = 3;
            this.btnSpiegeln.Text = "Drehen & Spiegeln";
            this.btnSpiegeln.Click += new System.EventHandler(this.Spiegeln_Click);
            // 
            // btnBruchlinie
            // 
            this.btnBruchlinie.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnBruchlinie.ImageCode = "Bruchlinie";
            this.btnBruchlinie.Location = new System.Drawing.Point(184, 2);
            this.btnBruchlinie.Name = "btnBruchlinie";
            this.btnBruchlinie.Size = new System.Drawing.Size(56, 66);
            this.btnBruchlinie.TabIndex = 2;
            this.btnBruchlinie.Text = "Bruch-Linie";
            this.btnBruchlinie.Click += new System.EventHandler(this.Bruchlinie_Click);
            // 
            // btnClipping
            // 
            this.btnClipping.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnClipping.ImageCode = "Zuschneiden";
            this.btnClipping.Location = new System.Drawing.Point(128, 2);
            this.btnClipping.Name = "btnClipping";
            this.btnClipping.Size = new System.Drawing.Size(56, 66);
            this.btnClipping.TabIndex = 1;
            this.btnClipping.Text = "Zuschnei-den";
            this.btnClipping.Click += new System.EventHandler(this.Clipping_Click);
            // 
            // grpZeichnen
            // 
            this.grpZeichnen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpZeichnen.CausesValidation = false;
            this.grpZeichnen.Controls.Add(this.btnZeichnen);
            this.grpZeichnen.Controls.Add(this.btnRadiergummi);
            this.grpZeichnen.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpZeichnen.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpZeichnen.Location = new System.Drawing.Point(192, 0);
            this.grpZeichnen.Name = "grpZeichnen";
            this.grpZeichnen.Size = new System.Drawing.Size(136, 81);
            this.grpZeichnen.TabIndex = 4;
            this.grpZeichnen.TabStop = false;
            this.grpZeichnen.Text = "Zeichnen";
            // 
            // btnZeichnen
            // 
            this.btnZeichnen.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
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
            this.btnRadiergummi.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
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
            this.grpSteuerung.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpSteuerung.CausesValidation = false;
            this.grpSteuerung.Controls.Add(this.btn100);
            this.grpSteuerung.Controls.Add(this.btnZoomFit);
            this.grpSteuerung.Controls.Add(this.btnRückgänig);
            this.grpSteuerung.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpSteuerung.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpSteuerung.Location = new System.Drawing.Point(0, 0);
            this.grpSteuerung.Name = "grpSteuerung";
            this.grpSteuerung.Size = new System.Drawing.Size(192, 81);
            this.grpSteuerung.TabIndex = 5;
            this.grpSteuerung.TabStop = false;
            this.grpSteuerung.Text = "Steuerung";
            // 
            // btn100
            // 
            this.btn100.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btn100.ImageCode = "Pinnadel";
            this.btn100.Location = new System.Drawing.Point(136, 2);
            this.btn100.Name = "btn100";
            this.btn100.Size = new System.Drawing.Size(48, 66);
            this.btn100.TabIndex = 9;
            this.btn100.Text = "100%";
            this.btn100.Click += new System.EventHandler(this.btn100_Click);
            // 
            // btnZoomFit
            // 
            this.btnZoomFit.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
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
            this.btnRückgänig.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
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
            this.P.Dock = System.Windows.Forms.DockStyle.Fill;
            this.P.Location = new System.Drawing.Point(0, 0);
            this.P.Name = "P";
            this.P.Size = new System.Drawing.Size(722, 340);
            this.P.TabIndex = 2;
            this.P.TabStop = false;
            this.P.DoAdditionalDrawing += new System.EventHandler<BlueControls.EventArgs.AdditionalDrawing>(this.P_DoAdditionalDrawing);
            this.P.ImageMouseDown += new System.EventHandler<BlueControls.EventArgs.MouseEventArgs1_1>(this.P_ImageMouseDown);
            this.P.ImageMouseMove += new System.EventHandler<BlueControls.EventArgs.MouseEventArgs1_1DownAndCurrent>(this.P_ImageMouseMove);
            this.P.ImageMouseUp += new System.EventHandler<BlueControls.EventArgs.MouseEventArgs1_1DownAndCurrent>(this.P_ImageMouseUp);
            this.P.MouseLeave += new System.EventHandler(this.P_MouseLeave);
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
            this.BLupe.TabIndex = 0;
            this.BLupe.TabStop = false;
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
            // LoadTab
            // 
            this.LoadTab.Filter = "PNG-Dateien|*.PNG|BMP-Dateien|*.BMP|JPG-Dateien|*.JPG";
            this.LoadTab.Title = "Bild laden";
            this.LoadTab.FileOk += new System.ComponentModel.CancelEventHandler(this.LoadTab_FileOk);
            // 
            // SaveTab
            // 
            this.SaveTab.Filter = "PNG-Dateien|*.PNG|BMP-Dateien|*.BMP|JPG-Dateien|*.JPG";
            this.SaveTab.Title = "Bitte neuen Dateinamen wählen.";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1007, 450);
            this.Controls.Add(this.Split);
            this.Controls.Add(this.tabRibbonbar);
            this.Name = "MainWindow";
            this.Text = "BluePaint";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.tabRibbonbar.ResumeLayout(false);
            this.Tab_Start.ResumeLayout(false);
            this.grpDatei.ResumeLayout(false);
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
        internal RibbonBar tabRibbonbar;
        internal  System.Windows.Forms.TabPage Tab_Start;
        internal  System.Windows.Forms.TabPage Tab_Werkzeug;
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

        private Button btnZoomFit;
        private GroupBox grpDatei;
        internal LastFilesCombo btnLetzteDateien;
        private Button btnOeffnen;
        private Button btnSaveAs;
        private Button btnNeu;
        private OpenFileDialog LoadTab;
        private SaveFileDialog SaveTab;
        private Button btnSave;
        internal Button btnGrößeÄndern;
        private Button btn100;
        private Button btnEinfügen;
        private Button btnCopy;
    }
}