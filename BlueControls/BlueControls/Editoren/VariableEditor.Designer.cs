using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Controls;
using BlueControls.Enums;
using GroupBox = BlueControls.Controls.GroupBox;

namespace BlueControls {
    partial class VariableEditor {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing) {
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
            this.tableVariablen = new BlueControls.Controls.Table();
            this.filterVariablen = new BlueControls.BlueDatabaseDialogs.Filterleiste();
            this.SuspendLayout();
            // 
            // tableVariablen
            // 
            this.tableVariablen.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableVariablen.Location = new System.Drawing.Point(8, 64);
            this.tableVariablen.Name = "tableVariablen";
            this.tableVariablen.Size = new System.Drawing.Size(483, 303);
            this.tableVariablen.TabIndex = 2;
            this.tableVariablen.Text = "tabVariablen";
            // 
            // filterVariablen
            // 
            this.filterVariablen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filterVariablen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.filterVariablen.Location = new System.Drawing.Point(8, 16);
            this.filterVariablen.Name = "filterVariablen";
            this.filterVariablen.Size = new System.Drawing.Size(483, 40);
            this.filterVariablen.TabIndex = 1;
            this.filterVariablen.TabStop = false;
            // 
            // VariableEditor
            // 
            this.Controls.Add(this.tableVariablen);
            this.Controls.Add(this.filterVariablen);
            this.Name = "VariableEditor";
            this.Size = new System.Drawing.Size(502, 375);
            this.ResumeLayout(false);

        }

        #endregion
        private Table tableVariablen;
        private Filterleiste filterVariablen;
    }
}
