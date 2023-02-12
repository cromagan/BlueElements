using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueDatabase.Enums;
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
            this.grpLayoutSelection = new GroupBox();
            this.btnCopyID = new Button();
            this.btnLayoutUmbenennen = new Button();
            this.btnLayoutLöschen = new Button();
            this.btnLayoutHinzu = new Button();
            this.capLayout = new Caption();
            this.cbxLayout = new ComboBox();
            this.tabRight = new TabControl();
            this.grpExterneLayouts = new GroupBox();
            this.btnTextEditor = new Button();
            this.btnLayoutVerzeichnis = new Button();
            this.btnLayoutOeffnen = new Button();
            this.tabDatei.SuspendLayout();
            this.grpDateiSystem.SuspendLayout();
            this.grpDesign.SuspendLayout();
            this.tabHintergrund.SuspendLayout();
            this.tabRightSide.SuspendLayout();
            this.Ribbon.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAssistent.SuspendLayout();
            this.grpLayoutSelection.SuspendLayout();
            this.grpExterneLayouts.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabDatei
            // 
            this.tabDatei.Controls.Add(this.grpExterneLayouts);
            this.tabDatei.Controls.Add(this.grpLayoutSelection);
            this.tabDatei.Controls.SetChildIndex(this.grpDateiSystem, 0);
            this.tabDatei.Controls.SetChildIndex(this.grpLayoutSelection, 0);
            this.tabDatei.Controls.SetChildIndex(this.grpExterneLayouts, 0);
            // 
            // grpDateiSystem
            // 
            this.grpDateiSystem.Size = new Size(208, 75);
            // 
            // btnLastFiles
            // 
            this.btnLastFiles.AdditionalFormatCheck = AdditionalCheck.None;
            this.btnLastFiles.Location = new Point(200, 2);
            this.btnLastFiles.Verhalten = SteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            this.btnLastFiles.Visible = false;
            // 
            // btnOeffnen
            // 
            this.btnOeffnen.ImageCode = "Textdatei||||||||||Pfeil_Links";
            this.btnOeffnen.Text = "Import";
            // 
            // tabRightSide
            // 
            this.tabRightSide.Location = new Point(968, 136);
            this.tabRightSide.Size = new Size(372, 476);
            // 
            // Pad
            // 
            this.Pad.Size = new Size(968, 476);
            // 
            // Ribbon
            // 
            this.Ribbon.Size = new Size(1340, 110);
            this.Ribbon.TabDefaultOrder = new string[] {
        "Datei",
        "Start",
        "Hintergrund",
        "Export"};
            // 
            // tabStart
            // 
            this.tabStart.Size = new Size(1332, 81);
            // 
            // tabSeiten
            // 
            this.tabSeiten.Size = new Size(1340, 26);
            // 
            // grpLayoutSelection
            // 
            this.grpLayoutSelection.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpLayoutSelection.CausesValidation = false;
            this.grpLayoutSelection.Controls.Add(this.btnCopyID);
            this.grpLayoutSelection.Controls.Add(this.btnLayoutUmbenennen);
            this.grpLayoutSelection.Controls.Add(this.btnLayoutLöschen);
            this.grpLayoutSelection.Controls.Add(this.btnLayoutHinzu);
            this.grpLayoutSelection.Controls.Add(this.capLayout);
            this.grpLayoutSelection.Controls.Add(this.cbxLayout);
            this.grpLayoutSelection.Dock = DockStyle.Left;
            this.grpLayoutSelection.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            this.grpLayoutSelection.Location = new Point(211, 3);
            this.grpLayoutSelection.Name = "grpLayoutSelection";
            this.grpLayoutSelection.Size = new Size(232, 75);
            this.grpLayoutSelection.TabIndex = 2;
            this.grpLayoutSelection.TabStop = false;
            this.grpLayoutSelection.Text = "Datenbank";
            // 
            // btnCopyID
            // 
            this.btnCopyID.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnCopyID.ImageCode = "Clipboard|16";
            this.btnCopyID.Location = new Point(176, 2);
            this.btnCopyID.Name = "btnCopyID";
            this.btnCopyID.Size = new Size(48, 22);
            this.btnCopyID.TabIndex = 4;
            this.btnCopyID.Text = "ID";
            this.btnCopyID.Click += new EventHandler(this.btnCopyID_Click);
            // 
            // btnLayoutUmbenennen
            // 
            this.btnLayoutUmbenennen.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnLayoutUmbenennen.ImageCode = "Stift|16";
            this.btnLayoutUmbenennen.Location = new Point(120, 46);
            this.btnLayoutUmbenennen.Name = "btnLayoutUmbenennen";
            this.btnLayoutUmbenennen.Size = new Size(24, 22);
            this.btnLayoutUmbenennen.TabIndex = 3;
            this.btnLayoutUmbenennen.Click += new EventHandler(this.btnLayoutUmbenennen_Click);
            // 
            // btnLayoutLöschen
            // 
            this.btnLayoutLöschen.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnLayoutLöschen.ImageCode = "MinusZeichen|16";
            this.btnLayoutLöschen.Location = new Point(72, 46);
            this.btnLayoutLöschen.Name = "btnLayoutLöschen";
            this.btnLayoutLöschen.Size = new Size(24, 22);
            this.btnLayoutLöschen.TabIndex = 2;
            this.btnLayoutLöschen.Click += new EventHandler(this.btnLayoutLöschen_Click);
            // 
            // btnLayoutHinzu
            // 
            this.btnLayoutHinzu.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnLayoutHinzu.ImageCode = "PlusZeichen|16";
            this.btnLayoutHinzu.Location = new Point(40, 46);
            this.btnLayoutHinzu.Name = "btnLayoutHinzu";
            this.btnLayoutHinzu.Size = new Size(24, 22);
            this.btnLayoutHinzu.TabIndex = 1;
            this.btnLayoutHinzu.Click += new EventHandler(this.btnLayoutHinzu_Click);
            // 
            // capLayout
            // 
            this.capLayout.CausesValidation = false;
            this.capLayout.Location = new Point(8, 2);
            this.capLayout.Name = "capLayout";
            this.capLayout.Size = new Size(82, 22);
            this.capLayout.Text = "Layout";
            this.capLayout.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            // 
            // cbxLayout
            // 
            this.cbxLayout.Cursor = Cursors.IBeam;
            this.cbxLayout.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxLayout.Location = new Point(8, 24);
            this.cbxLayout.Name = "cbxLayout";
            this.cbxLayout.Size = new Size(216, 22);
            this.cbxLayout.TabIndex = 0;
            this.cbxLayout.ItemClicked += new EventHandler<BasicListItemEventArgs>(this.cbxLayout_ItemClicked);
            // 
            // tabRight
            // 
            this.tabRight.Database = null;
            this.tabRight.HotTrack = true;
            this.tabRight.Location = new Point(8, 67);
            this.tabRight.Name = "tabRight";
            this.tabRight.RowKey = ((long)(-1));
            this.tabRight.SelectedIndex = 0;
            this.tabRight.Size = new Size(472, 285);
            this.tabRight.TabDefault = null;
            this.tabRight.TabDefaultOrder = null;
            this.tabRight.TabIndex = 111;
            // 
            // grpExterneLayouts
            // 
            this.grpExterneLayouts.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpExterneLayouts.CausesValidation = false;
            this.grpExterneLayouts.Controls.Add(this.btnTextEditor);
            this.grpExterneLayouts.Controls.Add(this.btnLayoutVerzeichnis);
            this.grpExterneLayouts.Controls.Add(this.btnLayoutOeffnen);
            this.grpExterneLayouts.Dock = DockStyle.Left;
            this.grpExterneLayouts.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            this.grpExterneLayouts.Location = new Point(443, 3);
            this.grpExterneLayouts.Name = "grpExterneLayouts";
            this.grpExterneLayouts.Size = new Size(336, 75);
            this.grpExterneLayouts.TabIndex = 1;
            this.grpExterneLayouts.TabStop = false;
            this.grpExterneLayouts.Text = "Externe Layouts aus dem Dateisytem";
            // 
            // btnTextEditor
            // 
            this.btnTextEditor.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnTextEditor.ImageCode = "Textdatei";
            this.btnTextEditor.Location = new Point(8, 2);
            this.btnTextEditor.Name = "btnTextEditor";
            this.btnTextEditor.QuickInfo = "Layout mit Texteditor bearbeiten";
            this.btnTextEditor.Size = new Size(80, 66);
            this.btnTextEditor.TabIndex = 81;
            this.btnTextEditor.Text = "Texteditor";
            this.btnTextEditor.Click += new EventHandler(this.btnTextEditor_Click);
            // 
            // btnLayoutVerzeichnis
            // 
            this.btnLayoutVerzeichnis.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnLayoutVerzeichnis.ImageCode = "Ordner|16";
            this.btnLayoutVerzeichnis.Location = new Point(96, 24);
            this.btnLayoutVerzeichnis.Name = "btnLayoutVerzeichnis";
            this.btnLayoutVerzeichnis.Size = new Size(232, 22);
            this.btnLayoutVerzeichnis.TabIndex = 84;
            this.btnLayoutVerzeichnis.Text = "Layout-Verzeichnis öffnen";
            this.btnLayoutVerzeichnis.Click += new EventHandler(this.btnLayoutVerzeichnis_Click);
            // 
            // btnLayoutOeffnen
            // 
            this.btnLayoutOeffnen.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnLayoutOeffnen.ImageCode = "Anwendung|16";
            this.btnLayoutOeffnen.Location = new Point(96, 2);
            this.btnLayoutOeffnen.Name = "btnLayoutOeffnen";
            this.btnLayoutOeffnen.Size = new Size(232, 22);
            this.btnLayoutOeffnen.TabIndex = 83;
            this.btnLayoutOeffnen.Text = "Layout mit Std.-Anwendung öffnen";
            this.btnLayoutOeffnen.Click += new EventHandler(this.btnLayoutOeffnen_Click);
            // 
            // LayoutPadEditor
            // 
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(1340, 612);
            this.Name = "LayoutPadEditor";
            this.Text = "Druck-Layout";
            this.WindowState = FormWindowState.Maximized;
            this.tabDatei.ResumeLayout(false);
            this.grpDateiSystem.ResumeLayout(false);
            this.grpDesign.ResumeLayout(false);
            this.tabHintergrund.ResumeLayout(false);
            this.tabRightSide.ResumeLayout(false);
            this.Ribbon.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAssistent.ResumeLayout(false);
            this.grpLayoutSelection.ResumeLayout(false);
            this.grpExterneLayouts.ResumeLayout(false);
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
        internal Button btnCopyID;
    }
}