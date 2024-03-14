using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;

namespace BlueControls.Controls {
    partial class FileBrowser {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private IContainer components = null;



        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.components = new Container();
            this.ThumbGenerator = new BackgroundWorker();
            this.lsbFiles = new ListBox();
            this.txbPfad = new TextBox();
            this.panel1 = new Panel();
            this.btnZurück = new Button();
            this.btnExplorerÖffnen = new Button();
            this.btnAddScreenShot = new Button();
            this.chkFolder = new Timer(this.components);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ThumbGenerator
            // 
            this.ThumbGenerator.WorkerReportsProgress = true;
            this.ThumbGenerator.WorkerSupportsCancellation = true;
            this.ThumbGenerator.DoWork += new DoWorkEventHandler(this.ThumbGenerator_DoWork);
            this.ThumbGenerator.ProgressChanged += new ProgressChangedEventHandler(this.ThumbGenerator_ProgressChanged);
            this.ThumbGenerator.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.ThumbGenerator_RunWorkerCompleted);
            // 
            // lsbFiles
            // 
            this.lsbFiles.AddAllowed = AddType.None;
            this.lsbFiles.AllowDrop = true;
            this.lsbFiles.Appearance = ListBoxAppearance.FileSystem;
            this.lsbFiles.AutoSort = true;
            this.lsbFiles.CheckBehavior = CheckBehavior.NoSelection;
            this.lsbFiles.Dock = DockStyle.Fill;
            this.lsbFiles.Location = new Point(0, 32);
            this.lsbFiles.Name = "lsbFiles";
            this.lsbFiles.Size = new Size(414, 307);
            this.lsbFiles.TabIndex = 4;
            this.lsbFiles.ContextMenuInit += new EventHandler<ContextMenuInitEventArgs>(this.lsbFiles_ContextMenuInit);
            this.lsbFiles.ContextMenuItemClicked += new EventHandler<ContextMenuItemClickedEventArgs>(this.lsbFiles_ContextMenuItemClicked);
            this.lsbFiles.ItemClicked += new EventHandler<AbstractListItemEventArgs>(this.lsbFiles_ItemClicked);
            this.lsbFiles.DragDrop += new DragEventHandler(this.lsbFiles_DragDrop);
            this.lsbFiles.DragEnter += new DragEventHandler(this.lsbFiles_DragEnter);
            // 
            // txbPfad
            // 
            this.txbPfad.Cursor = Cursors.IBeam;
            this.txbPfad.Dock = DockStyle.Fill;
            this.txbPfad.Location = new Point(32, 0);
            this.txbPfad.Name = "txbPfad";
            this.txbPfad.Size = new Size(318, 32);
            this.txbPfad.TabIndex = 3;
            this.txbPfad.Enter += new EventHandler(this.txbPfad_Enter);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.txbPfad);
            this.panel1.Controls.Add(this.btnZurück);
            this.panel1.Controls.Add(this.btnExplorerÖffnen);
            this.panel1.Controls.Add(this.btnAddScreenShot);
            this.panel1.Dock = DockStyle.Top;
            this.panel1.Location = new Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(414, 32);
            this.panel1.TabIndex = 5;
            // 
            // btnZurück
            // 
            this.btnZurück.Dock = DockStyle.Left;
            this.btnZurück.ImageCode = "Pfeil_Oben|24|||0000FF";
            this.btnZurück.Location = new Point(0, 0);
            this.btnZurück.Name = "btnZurück";
            this.btnZurück.QuickInfo = "Eine Ordnerstufe höher";
            this.btnZurück.Size = new Size(32, 32);
            this.btnZurück.TabIndex = 6;
            this.btnZurück.Click += new EventHandler(this.btnZurück_Click);
            // 
            // btnExplorerÖffnen
            // 
            this.btnExplorerÖffnen.Dock = DockStyle.Right;
            this.btnExplorerÖffnen.ImageCode = "Ordner|24";
            this.btnExplorerÖffnen.Location = new Point(350, 0);
            this.btnExplorerÖffnen.Name = "btnExplorerÖffnen";
            this.btnExplorerÖffnen.QuickInfo = "Pfad im Explorer öffnen";
            this.btnExplorerÖffnen.Size = new Size(32, 32);
            this.btnExplorerÖffnen.TabIndex = 5;
            this.btnExplorerÖffnen.Click += new EventHandler(this.btnExplorerÖffnen_Click);
            // 
            // btnAddScreenShot
            // 
            this.btnAddScreenShot.Dock = DockStyle.Right;
            this.btnAddScreenShot.ImageCode = "Bild|24|||||||||PlusZeichen";
            this.btnAddScreenShot.Location = new Point(382, 0);
            this.btnAddScreenShot.Name = "btnAddScreenShot";
            this.btnAddScreenShot.QuickInfo = "Screenshot erstellen und speichern";
            this.btnAddScreenShot.Size = new Size(32, 32);
            this.btnAddScreenShot.TabIndex = 4;
            this.btnAddScreenShot.Click += new EventHandler(this.btnAddScreenShot_Click);
            // 
            // chkFolder
            // 
            this.chkFolder.Enabled = true;
            this.chkFolder.Interval = 1000;
            this.chkFolder.Tick += new EventHandler(this.chkFolder_Tick);
            // 
            // FileBrowser
            // 
            this.Controls.Add(this.lsbFiles);
            this.Controls.Add(this.panel1);
            this.Name = "FileBrowser";
            this.Size = new Size(414, 339);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private BackgroundWorker ThumbGenerator;
        private ListBox lsbFiles;
        private TextBox txbPfad;
        private Panel panel1;
        private Button btnAddScreenShot;
        private Button btnExplorerÖffnen;
        private Button btnZurück;
        private Timer chkFolder;
    }
}
