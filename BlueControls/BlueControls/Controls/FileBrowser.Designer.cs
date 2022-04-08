﻿
namespace BlueControls.Controls {
    partial class FileBrowser {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.ThumbGenerator = new System.ComponentModel.BackgroundWorker();
            this.lsbFiles = new BlueControls.Controls.ListBox();
            this.txbPfad = new BlueControls.Controls.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnExplorerÖffnen = new BlueControls.Controls.Button();
            this.btnAddScreenShot = new BlueControls.Controls.Button();
            this.btnZurück = new BlueControls.Controls.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ThumbGenerator
            // 
            this.ThumbGenerator.WorkerReportsProgress = true;
            this.ThumbGenerator.WorkerSupportsCancellation = true;
            this.ThumbGenerator.DoWork += new System.ComponentModel.DoWorkEventHandler(this.ThumbGenerator_DoWork);
            this.ThumbGenerator.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.ThumbGenerator_ProgressChanged);
            // 
            // lsbFiles
            // 
            this.lsbFiles.AddAllowed = BlueControls.Enums.AddType.None;
            this.lsbFiles.Appearance = BlueControls.Enums.BlueListBoxAppearance.FileSystem;
            this.lsbFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lsbFiles.Location = new System.Drawing.Point(0, 32);
            this.lsbFiles.Name = "lsbFiles";
            this.lsbFiles.Size = new System.Drawing.Size(414, 307);
            this.lsbFiles.TabIndex = 4;
            this.lsbFiles.ContextMenuInit += new System.EventHandler<BlueControls.EventArgs.ContextMenuInitEventArgs>(this.lsbFiles_ContextMenuInit);
            this.lsbFiles.ContextMenuItemClicked += new System.EventHandler<BlueControls.EventArgs.ContextMenuItemClickedEventArgs>(this.lsbFiles_ContextMenuItemClicked);
            this.lsbFiles.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.lsbFiles_ItemClicked);
            // 
            // txbPfad
            // 
            this.txbPfad.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbPfad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txbPfad.Location = new System.Drawing.Point(0, 0);
            this.txbPfad.Name = "txbPfad";
            this.txbPfad.Size = new System.Drawing.Size(382, 32);
            this.txbPfad.TabIndex = 3;
            this.txbPfad.Enter += new System.EventHandler(this.txbPfad_Enter);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnZurück);
            this.panel1.Controls.Add(this.btnExplorerÖffnen);
            this.panel1.Controls.Add(this.txbPfad);
            this.panel1.Controls.Add(this.btnAddScreenShot);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(414, 32);
            this.panel1.TabIndex = 5;
            // 
            // btnExplorerÖffnen
            // 
            this.btnExplorerÖffnen.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnExplorerÖffnen.ImageCode = "Ordner|24";
            this.btnExplorerÖffnen.Location = new System.Drawing.Point(350, 0);
            this.btnExplorerÖffnen.Name = "btnExplorerÖffnen";
            this.btnExplorerÖffnen.QuickInfo = "Pfad im Explorer öffnen";
            this.btnExplorerÖffnen.Size = new System.Drawing.Size(32, 32);
            this.btnExplorerÖffnen.TabIndex = 5;
            this.btnExplorerÖffnen.Click += new System.EventHandler(this.btnExplorerÖffnen_Click);
            // 
            // btnAddScreenShot
            // 
            this.btnAddScreenShot.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnAddScreenShot.ImageCode = "Bild|24|||||||||PlusZeichen";
            this.btnAddScreenShot.Location = new System.Drawing.Point(382, 0);
            this.btnAddScreenShot.Name = "btnAddScreenShot";
            this.btnAddScreenShot.QuickInfo = "Screenshot erstellen und speichern";
            this.btnAddScreenShot.Size = new System.Drawing.Size(32, 32);
            this.btnAddScreenShot.TabIndex = 4;
            this.btnAddScreenShot.Click += new System.EventHandler(this.btnAddScreenShot_Click);
            // 
            // btnZurück
            // 
            this.btnZurück.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnZurück.ImageCode = "Pfeil_Oben|24|||0000FF";
            this.btnZurück.Location = new System.Drawing.Point(0, 0);
            this.btnZurück.Name = "btnZurück";
            this.btnZurück.QuickInfo = "Eine Ordnerstufe höher";
            this.btnZurück.Size = new System.Drawing.Size(32, 32);
            this.btnZurück.TabIndex = 6;
            this.btnZurück.Click += new System.EventHandler(this.btnZurück_Click);
            // 
            // FileBrowser
            // 
            this.Controls.Add(this.lsbFiles);
            this.Controls.Add(this.panel1);
            this.Name = "FileBrowser";
            this.Size = new System.Drawing.Size(414, 339);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker ThumbGenerator;
        private ListBox lsbFiles;
        private TextBox txbPfad;
        private System.Windows.Forms.Panel panel1;
        private Button btnAddScreenShot;
        private Button btnExplorerÖffnen;
        private Button btnZurück;
    }
}
