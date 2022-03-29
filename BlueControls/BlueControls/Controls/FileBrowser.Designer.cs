
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
            this.SuspendLayout();
            // 
            // ThumbGenerator
            // 
            this.ThumbGenerator.WorkerReportsProgress = true;
            this.ThumbGenerator.WorkerSupportsCancellation = true;
            this.ThumbGenerator.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.ThumbGenerator_ProgressChanged);
            this.ThumbGenerator.DoWork += new System.ComponentModel.DoWorkEventHandler(this.ThumbGenerator_DoWork);
            // 
            // lsbFiles
            // 
            this.lsbFiles.AddAllowed = BlueControls.Enums.AddType.None;
            this.lsbFiles.Appearance = BlueControls.Enums.BlueListBoxAppearance.FileSystem;
            this.lsbFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lsbFiles.Location = new System.Drawing.Point(0, 24);
            this.lsbFiles.Name = "lsbFiles";
            this.lsbFiles.Size = new System.Drawing.Size(414, 315);
            this.lsbFiles.TabIndex = 4;
            this.lsbFiles.ContextMenuInit += new System.EventHandler<BlueControls.EventArgs.ContextMenuInitEventArgs>(this.lsbFiles_ContextMenuInit);
            this.lsbFiles.ContextMenuItemClicked += new System.EventHandler<BlueControls.EventArgs.ContextMenuItemClickedEventArgs>(this.lsbFiles_ContextMenuItemClicked);
            this.lsbFiles.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.lsbFiles_ItemClicked);
            // 
            // txbPfad
            // 
            this.txbPfad.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbPfad.Dock = System.Windows.Forms.DockStyle.Top;
            this.txbPfad.Location = new System.Drawing.Point(0, 0);
            this.txbPfad.Name = "txbPfad";
            this.txbPfad.Size = new System.Drawing.Size(414, 24);
            this.txbPfad.TabIndex = 3;
            this.txbPfad.Enter += new System.EventHandler(this.txbPfad_Enter);
            // 
            // FileBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lsbFiles);
            this.Controls.Add(this.txbPfad);
            this.Name = "FileBrowser";
            this.Size = new System.Drawing.Size(414, 339);
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker ThumbGenerator;
        private ListBox lsbFiles;
        private TextBox txbPfad;
    }
}
