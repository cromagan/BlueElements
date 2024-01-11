using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using Button = BlueControls.Controls.Button;
using ComboBox = BlueControls.Controls.ComboBox;
using GroupBox = BlueControls.Controls.GroupBox;
using TabControl = BlueControls.Controls.TabControl;

namespace BlueControls.BlueDatabaseDialogs 
    {
        public partial class LayoutPadEditor 
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
            this.btnCopyID = new BlueControls.Controls.Button();
            this.capLayout = new BlueControls.Controls.Caption();
            this.cbxLayout = new BlueControls.Controls.ComboBox();
            this.tabRight = new BlueControls.Controls.TabControl();
            this.grpExterneLayouts = new BlueControls.Controls.GroupBox();
            this.btnTextEditor = new BlueControls.Controls.Button();
            this.btnLayoutVerzeichnis = new BlueControls.Controls.Button();
            this.btnLayoutOeffnen = new BlueControls.Controls.Button();
            this.tabDatei.SuspendLayout();
            this.grpDateiSystem.SuspendLayout();
            this.grpDesign.SuspendLayout();
            this.tabHintergrund.SuspendLayout();
            this.Ribbon.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAssistent.SuspendLayout();
            this.pnlStatusBar.SuspendLayout();
            this.grpLayoutSelection.SuspendLayout();
            this.grpExterneLayouts.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabDatei
            // 
            this.tabDatei.Controls.Add(this.grpExterneLayouts);
            this.tabDatei.Controls.Add(this.grpLayoutSelection);
            this.tabDatei.Size = new System.Drawing.Size(1332, 81);
            this.tabDatei.Controls.SetChildIndex(this.grpDateiSystem, 0);
            this.tabDatei.Controls.SetChildIndex(this.grpLayoutSelection, 0);
            this.tabDatei.Controls.SetChildIndex(this.grpExterneLayouts, 0);
            // 
            // grpDateiSystem
            // 
            this.grpDateiSystem.Size = new System.Drawing.Size(208, 75);
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
            // tabHintergrund
            // 
            this.tabHintergrund.Size = new System.Drawing.Size(1332, 81);
            // 
            // tabRightSide
            // 
            this.tabRightSide.Location = new System.Drawing.Point(968, 136);
            this.tabRightSide.Size = new System.Drawing.Size(372, 452);
            // 
            // Pad
            // 
            this.Pad.Location = new System.Drawing.Point(0, 136);
            this.Pad.Size = new System.Drawing.Size(968, 452);
            // 
            // Ribbon
            // 
            this.Ribbon.Size = new System.Drawing.Size(1340, 110);
            this.Ribbon.TabDefaultOrder = new string[] {
        "Datei",
        "Start",
        "Hintergrund",
        "Export"};
            // 
            // tabStart
            // 
            this.tabStart.Size = new System.Drawing.Size(1332, 81);
            // 
            // tabExport
            // 
            this.tabExport.Size = new System.Drawing.Size(1332, 81);
            // 
            // tabSeiten
            // 
            this.tabSeiten.Size = new System.Drawing.Size(1340, 26);
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(1340, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 588);
            this.pnlStatusBar.Size = new System.Drawing.Size(1340, 24);
            // 
            // grpLayoutSelection
            // 
            this.grpLayoutSelection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpLayoutSelection.CausesValidation = false;
            this.grpLayoutSelection.Controls.Add(this.btnCopyID);
            this.grpLayoutSelection.Controls.Add(this.capLayout);
            this.grpLayoutSelection.Controls.Add(this.cbxLayout);
            this.grpLayoutSelection.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpLayoutSelection.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpLayoutSelection.Location = new System.Drawing.Point(211, 3);
            this.grpLayoutSelection.Name = "grpLayoutSelection";
            this.grpLayoutSelection.Size = new System.Drawing.Size(232, 75);
            this.grpLayoutSelection.TabIndex = 2;
            this.grpLayoutSelection.TabStop = false;
            this.grpLayoutSelection.Text = "Datenbank";
            // 
            // btnCopyID
            // 
            this.btnCopyID.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnCopyID.ImageCode = "Clipboard|16";
            this.btnCopyID.Location = new System.Drawing.Point(176, 2);
            this.btnCopyID.Name = "btnCopyID";
            this.btnCopyID.Size = new System.Drawing.Size(48, 22);
            this.btnCopyID.TabIndex = 4;
            this.btnCopyID.Text = "ID";
            this.btnCopyID.Click += new System.EventHandler(this.btnCopyID_Click);
            // 
            // capLayout
            // 
            this.capLayout.CausesValidation = false;
            this.capLayout.Location = new System.Drawing.Point(8, 2);
            this.capLayout.Name = "capLayout";
            this.capLayout.Size = new System.Drawing.Size(82, 22);
            this.capLayout.Text = "Layout";
            this.capLayout.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            // 
            // cbxLayout
            // 
            this.cbxLayout.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxLayout.Location = new System.Drawing.Point(8, 24);
            this.cbxLayout.Name = "cbxLayout";
            this.cbxLayout.Size = new System.Drawing.Size(216, 22);
            this.cbxLayout.TabIndex = 0;
            this.cbxLayout.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.cbxLayout_ItemClicked);
            // 
            // tabRight
            // 
            this.tabRight.HotTrack = true;
            this.tabRight.Location = new System.Drawing.Point(8, 67);
            this.tabRight.Name = "tabRight";
            this.tabRight.SelectedIndex = 0;
            this.tabRight.Size = new System.Drawing.Size(472, 285);
            this.tabRight.TabDefault = null;
            this.tabRight.TabDefaultOrder = null;
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
            this.grpExterneLayouts.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpExterneLayouts.Location = new System.Drawing.Point(443, 3);
            this.grpExterneLayouts.Name = "grpExterneLayouts";
            this.grpExterneLayouts.Size = new System.Drawing.Size(336, 75);
            this.grpExterneLayouts.TabIndex = 1;
            this.grpExterneLayouts.TabStop = false;
            this.grpExterneLayouts.Text = "Externe Layouts aus dem Dateisytem";
            // 
            // btnTextEditor
            // 
            this.btnTextEditor.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
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
            this.btnLayoutVerzeichnis.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
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
            this.btnLayoutOeffnen.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnLayoutOeffnen.ImageCode = "Anwendung|16";
            this.btnLayoutOeffnen.Location = new System.Drawing.Point(96, 2);
            this.btnLayoutOeffnen.Name = "btnLayoutOeffnen";
            this.btnLayoutOeffnen.Size = new System.Drawing.Size(232, 22);
            this.btnLayoutOeffnen.TabIndex = 83;
            this.btnLayoutOeffnen.Text = "Layout mit Std.-Anwendung öffnen";
            this.btnLayoutOeffnen.Click += new System.EventHandler(this.btnLayoutOeffnen_Click);
            // 
            // LayoutPadEditor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1340, 612);
            this.Name = "LayoutPadEditor";
            this.Text = "Druck-Layout";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.tabDatei.ResumeLayout(false);
            this.grpDateiSystem.ResumeLayout(false);
            this.grpDesign.ResumeLayout(false);
            this.tabHintergrund.ResumeLayout(false);
            this.Ribbon.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAssistent.ResumeLayout(false);
            this.pnlStatusBar.ResumeLayout(false);
            this.grpLayoutSelection.ResumeLayout(false);
            this.grpExterneLayouts.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        internal GroupBox grpLayoutSelection;
        internal Caption capLayout;
        internal ComboBox cbxLayout;
        private TabControl tabRight;
        internal GroupBox grpExterneLayouts;
        private Button btnTextEditor;
        private Button btnLayoutVerzeichnis;
        private Button btnLayoutOeffnen;
        internal Button btnCopyID;
    }
}