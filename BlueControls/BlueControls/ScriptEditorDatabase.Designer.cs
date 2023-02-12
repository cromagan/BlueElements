
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.EventArgs;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls {
    partial class ScriptEditorDatabase {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.txbTestZeile = new TextBox();
            this.grpMainBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpMainBar
            // 
            this.grpMainBar.Controls.Add(this.txbTestZeile);
            this.grpMainBar.Controls.SetChildIndex(this.btnZusatzDateien, 0);
            this.grpMainBar.Controls.SetChildIndex(this.btnTest, 0);
            this.grpMainBar.Controls.SetChildIndex(this.txbTestZeile, 0);
            this.grpMainBar.Controls.SetChildIndex(this.btnBefehlsUebersicht, 0);
            // 
            // btnBefehlsUebersicht
            // 
            this.btnBefehlsUebersicht.Location = new Point(384, 8);
            // 
            // btnZusatzDateien
            // 
            this.btnZusatzDateien.Location = new Point(528, 8);
            // 
            // txbTestZeile
            // 
            this.txbTestZeile.Cursor = Cursors.IBeam;
            this.txbTestZeile.Enabled = false;
            this.txbTestZeile.Location = new Point(120, 8);
            this.txbTestZeile.Name = "txbTestZeile";
            this.txbTestZeile.Size = new Size(256, 32);
            this.txbTestZeile.TabIndex = 4;
            // 
            // ScriptEditorDatabase
            // 
            this.Name = "ScriptEditorDatabase";
            this.ContextMenuInit += new EventHandler<ContextMenuInitEventArgs>(this.scriptEditor_ContextMenuInit);
            this.ContextMenuItemClicked += new EventHandler<ContextMenuItemClickedEventArgs>(this.scriptEditor_ContextMenuItemClicked);
            this.grpMainBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TextBox txbTestZeile;

    }
}
