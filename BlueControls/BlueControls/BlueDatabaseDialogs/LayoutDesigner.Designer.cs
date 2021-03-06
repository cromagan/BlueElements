﻿using System.Diagnostics;
using BlueControls.Controls;

namespace BlueControls.BlueDatabaseDialogs
{
    internal partial class LayoutDesigner
    { 
        //Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.grpLayoutSelection = new BlueControls.Controls.GroupBox();
            this.btnLayoutUmbenennen = new BlueControls.Controls.Button();
            this.btnLayoutLöschen = new BlueControls.Controls.Button();
            this.btnLayoutHinzu = new BlueControls.Controls.Button();
            this.capLayout = new BlueControls.Controls.Caption();
            this.cbxLayout = new BlueControls.Controls.ComboBox();
            this.tabRight = new BlueControls.Controls.TabControl();
            this.grpExterneLayouts = new BlueControls.Controls.GroupBox();
            this.btnTextEditor = new BlueControls.Controls.Button();
            this.btnLayoutVerzeichnis = new BlueControls.Controls.Button();
            this.btnLayoutOeffnen = new BlueControls.Controls.Button();
            this.tabRightSide = new BlueControls.Controls.TabControl();
            this.tabElementEigenschaften = new System.Windows.Forms.TabPage();
            this.tabSkript = new System.Windows.Forms.TabPage();
            this.scriptEditor = new BlueControls.ScriptEditor();
            this.Ribbon.SuspendLayout();
            this.tabPageControl.SuspendLayout();
            this.tabPageStart.SuspendLayout();
            this.grpDrucken.SuspendLayout();
            this.grpDesign.SuspendLayout();
            this.grpAssistent.SuspendLayout();
            this.grpDateiSystem.SuspendLayout();
            this.grpLayoutSelection.SuspendLayout();
            this.grpExterneLayouts.SuspendLayout();
            this.tabRightSide.SuspendLayout();
            this.tabSkript.SuspendLayout();
            this.SuspendLayout();
            // 
            // Pad
            // 
            this.Pad.Size = new System.Drawing.Size(816, 502);
            this.Pad.ClickedItemChanged += new System.EventHandler(this.Pad_ClickedItemChanged);
            // 
            // Ribbon
            // 
            this.Ribbon.Size = new System.Drawing.Size(1340, 110);
            // 
            //  System.Windows.Forms.TabPageControl
            // 
            this.tabPageControl.Size = new System.Drawing.Size(1332, 81);
            // 
            //  System.Windows.Forms.TabPageStart
            // 
            this.tabPageStart.Controls.Add(this.grpExterneLayouts);
            this.tabPageStart.Controls.Add(this.grpLayoutSelection);
            this.tabPageStart.Size = new System.Drawing.Size(1332, 81);
            this.tabPageStart.Controls.SetChildIndex(this.grpDateiSystem, 0);
            this.tabPageStart.Controls.SetChildIndex(this.grpDrucken, 0);
            this.tabPageStart.Controls.SetChildIndex(this.grpLayoutSelection, 0);
            this.tabPageStart.Controls.SetChildIndex(this.grpExterneLayouts, 0);
            // 
            // grpDrucken
            // 
            this.grpDrucken.Location = new System.Drawing.Point(208, 0);
            this.grpDrucken.Visible = false;
            // 
            // grpDateiSystem
            // 
            this.grpDateiSystem.Size = new System.Drawing.Size(208, 81);
            // 
            // btnLastFiles
            // 
            this.btnLastFiles.Location = new System.Drawing.Point(200, 2);
            this.btnLastFiles.Visible = false;
            // 
            // btnOeffnen
            // 
            this.btnOeffnen.ImageCode = "Textdatei||||||||||Pfeil_Links";
            this.btnOeffnen.Text = "Import";
            // 
            // btnSpeichern
            // 
            this.btnSpeichern.Location = new System.Drawing.Point(136, 2);
            this.btnSpeichern.Text = "Speichern unter";
            // 
            // grpLayoutSelection
            // 
            this.grpLayoutSelection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpLayoutSelection.CausesValidation = false;
            this.grpLayoutSelection.Controls.Add(this.btnLayoutUmbenennen);
            this.grpLayoutSelection.Controls.Add(this.btnLayoutLöschen);
            this.grpLayoutSelection.Controls.Add(this.btnLayoutHinzu);
            this.grpLayoutSelection.Controls.Add(this.capLayout);
            this.grpLayoutSelection.Controls.Add(this.cbxLayout);
            this.grpLayoutSelection.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpLayoutSelection.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.grpLayoutSelection.Location = new System.Drawing.Point(504, 0);
            this.grpLayoutSelection.Name = "grpLayoutSelection";
            this.grpLayoutSelection.Size = new System.Drawing.Size(232, 81);
            this.grpLayoutSelection.TabIndex = 2;
            this.grpLayoutSelection.TabStop = false;
            this.grpLayoutSelection.Text = "Datenbank";
            // 
            // btnLayoutUmbenennen
            // 
            this.btnLayoutUmbenennen.ImageCode = "Stift|16";
            this.btnLayoutUmbenennen.Location = new System.Drawing.Point(120, 46);
            this.btnLayoutUmbenennen.Name = "btnLayoutUmbenennen";
            this.btnLayoutUmbenennen.Size = new System.Drawing.Size(24, 22);
            this.btnLayoutUmbenennen.TabIndex = 3;
            this.btnLayoutUmbenennen.Click += new System.EventHandler(this.btnLayoutUmbenennen_Click);
            // 
            // btnLayoutLöschen
            // 
            this.btnLayoutLöschen.ImageCode = "MinusZeichen|16";
            this.btnLayoutLöschen.Location = new System.Drawing.Point(72, 46);
            this.btnLayoutLöschen.Name = "btnLayoutLöschen";
            this.btnLayoutLöschen.Size = new System.Drawing.Size(24, 22);
            this.btnLayoutLöschen.TabIndex = 2;
            this.btnLayoutLöschen.Click += new System.EventHandler(this.btnLayoutLöschen_Click);
            // 
            // btnLayoutHinzu
            // 
            this.btnLayoutHinzu.ImageCode = "PlusZeichen|16";
            this.btnLayoutHinzu.Location = new System.Drawing.Point(40, 46);
            this.btnLayoutHinzu.Name = "btnLayoutHinzu";
            this.btnLayoutHinzu.Size = new System.Drawing.Size(24, 22);
            this.btnLayoutHinzu.TabIndex = 1;
            this.btnLayoutHinzu.Click += new System.EventHandler(this.btnLayoutHinzu_Click);
            // 
            // capLayout
            // 
            this.capLayout.CausesValidation = false;
            this.capLayout.Location = new System.Drawing.Point(8, 2);
            this.capLayout.Name = "capLayout";
            this.capLayout.Size = new System.Drawing.Size(82, 22);
            this.capLayout.Text = "Layout";
            this.capLayout.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            // 
            // cbxLayout
            // 
            this.cbxLayout.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxLayout.Location = new System.Drawing.Point(8, 24);
            this.cbxLayout.Name = "cbxLayout";
            this.cbxLayout.Size = new System.Drawing.Size(216, 22);
            this.cbxLayout.TabIndex = 0;
            this.cbxLayout.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.cbxLayout_ItemClicked);
            // 
            // tabRight
            // 
            this.tabRight.HotTrack = true;
            this.tabRight.Location = new System.Drawing.Point(8, 67);
            this.tabRight.Name = "tabRight";
            this.tabRight.SelectedIndex = 0;
            this.tabRight.Size = new System.Drawing.Size(472, 285);
            this.tabRight.TabIndex = 111;
            // 
            // grpExterneLayouts
            // 
            this.grpExterneLayouts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpExterneLayouts.CausesValidation = false;
            this.grpExterneLayouts.Controls.Add(this.btnTextEditor);
            this.grpExterneLayouts.Controls.Add(this.btnLayoutVerzeichnis);
            this.grpExterneLayouts.Controls.Add(this.btnLayoutOeffnen);
            this.grpExterneLayouts.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpExterneLayouts.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.grpExterneLayouts.Location = new System.Drawing.Point(736, 0);
            this.grpExterneLayouts.Name = "grpExterneLayouts";
            this.grpExterneLayouts.Size = new System.Drawing.Size(336, 81);
            this.grpExterneLayouts.TabIndex = 1;
            this.grpExterneLayouts.TabStop = false;
            this.grpExterneLayouts.Text = "Externe Layouts aus dem Dateisytem";
            // 
            // btnTextEditor
            // 
            this.btnTextEditor.ImageCode = "Textdatei";
            this.btnTextEditor.Location = new System.Drawing.Point(8, 2);
            this.btnTextEditor.Name = "btnTextEditor";
            this.btnTextEditor.QuickInfo = "Layout mit Texteditor bearbeiten";
            this.btnTextEditor.Size = new System.Drawing.Size(80, 66);
            this.btnTextEditor.TabIndex = 81;
            this.btnTextEditor.Text = "Texteditor";
            this.btnTextEditor.Click += new System.EventHandler(this.btnTextEditor_Click);
            // 
            // btnLayoutVerzeichnis
            // 
            this.btnLayoutVerzeichnis.ImageCode = "Ordner|16";
            this.btnLayoutVerzeichnis.Location = new System.Drawing.Point(96, 24);
            this.btnLayoutVerzeichnis.Name = "btnLayoutVerzeichnis";
            this.btnLayoutVerzeichnis.Size = new System.Drawing.Size(232, 22);
            this.btnLayoutVerzeichnis.TabIndex = 84;
            this.btnLayoutVerzeichnis.Text = "Layout-Verzeichnis öffnen";
            this.btnLayoutVerzeichnis.Click += new System.EventHandler(this.btnLayoutVerzeichnis_Click);
            // 
            // btnLayoutOeffnen
            // 
            this.btnLayoutOeffnen.ImageCode = "Anwendung|16";
            this.btnLayoutOeffnen.Location = new System.Drawing.Point(96, 2);
            this.btnLayoutOeffnen.Name = "btnLayoutOeffnen";
            this.btnLayoutOeffnen.Size = new System.Drawing.Size(232, 22);
            this.btnLayoutOeffnen.TabIndex = 83;
            this.btnLayoutOeffnen.Text = "Layout mit Std.-Anwendung öffnen";
            this.btnLayoutOeffnen.Click += new System.EventHandler(this.btnLayoutOeffnen_Click);
            // 
            // tabRightSide
            // 
            this.tabRightSide.Controls.Add(this.tabElementEigenschaften);
            this.tabRightSide.Controls.Add(this.tabSkript);
            this.tabRightSide.Dock = System.Windows.Forms.DockStyle.Right;
            this.tabRightSide.HotTrack = true;
            this.tabRightSide.Location = new System.Drawing.Point(816, 110);
            this.tabRightSide.Name = "tabRightSide";
            this.tabRightSide.SelectedIndex = 0;
            this.tabRightSide.Size = new System.Drawing.Size(524, 502);
            this.tabRightSide.TabIndex = 3;
            // 
            // tabElementEigenschaften
            // 
            this.tabElementEigenschaften.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabElementEigenschaften.Location = new System.Drawing.Point(4, 25);
            this.tabElementEigenschaften.Name = "tabElementEigenschaften";
            this.tabElementEigenschaften.Size = new System.Drawing.Size(516, 473);
            this.tabElementEigenschaften.TabIndex = 0;
            this.tabElementEigenschaften.Text = "Element-Eigenschaften";
            // 
            // tabSkript
            // 
            this.tabSkript.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabSkript.Controls.Add(this.scriptEditor);
            this.tabSkript.Location = new System.Drawing.Point(4, 25);
            this.tabSkript.Name = "tabSkript";
            this.tabSkript.Size = new System.Drawing.Size(516, 473);
            this.tabSkript.TabIndex = 1;
            this.tabSkript.Text = "Skript";
            // 
            // scriptEditor
            // 
            this.scriptEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.scriptEditor.Database = null;
            this.scriptEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scriptEditor.Location = new System.Drawing.Point(0, 0);
            this.scriptEditor.Name = "scriptEditor";
            this.scriptEditor.Size = new System.Drawing.Size(516, 473);
            this.scriptEditor.TabIndex = 1;
            this.scriptEditor.TabStop = false;
            // 
            // LayoutDesigner
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1340, 612);
            this.Controls.Add(this.tabRightSide);
            this.Name = "LayoutDesigner";
            this.Text = "Druck-Layout";
            this.TopMost = false;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Controls.SetChildIndex(this.Ribbon, 0);
            this.Controls.SetChildIndex(this.tabRightSide, 0);
            this.Controls.SetChildIndex(this.Pad, 0);
            this.Ribbon.ResumeLayout(false);
            this.tabPageControl.ResumeLayout(false);
            this.tabPageStart.ResumeLayout(false);
            this.grpDrucken.ResumeLayout(false);
            this.grpDesign.ResumeLayout(false);
            this.grpAssistent.ResumeLayout(false);
            this.grpDateiSystem.ResumeLayout(false);
            this.grpLayoutSelection.ResumeLayout(false);
            this.grpExterneLayouts.ResumeLayout(false);
            this.tabRightSide.ResumeLayout(false);
            this.tabSkript.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        internal GroupBox grpLayoutSelection;
        internal Button btnLayoutUmbenennen;
        internal Button btnLayoutLöschen;
        internal Button btnLayoutHinzu;
        internal Caption capLayout;
        internal ComboBox cbxLayout;
        private TabControl tabRight;
        internal GroupBox grpExterneLayouts;
        private Button btnTextEditor;
        private Button btnLayoutVerzeichnis;
        private Button btnLayoutOeffnen;
        private  System.Windows.Forms.TabPage tabElementEigenschaften;
        private  System.Windows.Forms.TabPage tabSkript;
        private TabControl tabRightSide;
        private ScriptEditor scriptEditor;
    }
}
