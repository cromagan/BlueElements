
namespace BlueControls {
    partial class ScriptEditorDatabase {
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
            this.txbTestZeile = new BlueControls.Controls.TextBox();
            this.capTestZeile = new BlueControls.Controls.Caption();
            this.SuspendLayout();
            // 
            // txbTestZeile
            // 
            this.txbTestZeile.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbTestZeile.Location = new System.Drawing.Point(229, 247);
            this.txbTestZeile.Name = "txbTestZeile";
            this.txbTestZeile.Size = new System.Drawing.Size(144, 24);
            this.txbTestZeile.TabIndex = 4;
            // 
            // capTestZeile
            // 
            this.capTestZeile.CausesValidation = false;
            this.capTestZeile.Location = new System.Drawing.Point(229, 231);
            this.capTestZeile.Name = "capTestZeile";
            this.capTestZeile.Size = new System.Drawing.Size(72, 16);
            this.capTestZeile.Text = "Test-Zeile:";
            // 
            // ScriptEditorDatabase
            // 
            this.Controls.Add(this.txbTestZeile);
            this.Controls.Add(this.capTestZeile);
            this.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.Normal;
            this.Name = "ScriptEditorDatabase";
            this.ContextMenuInit += new System.EventHandler<BlueControls.EventArgs.ContextMenuInitEventArgs>(this.scriptEditor_ContextMenuInit);
            this.ContextMenuItemClicked += new System.EventHandler<BlueControls.EventArgs.ContextMenuItemClickedEventArgs>(this.scriptEditor_ContextMenuItemClicked); 
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.TextBox txbTestZeile;
        private Controls.Caption capTestZeile;
    }
}
