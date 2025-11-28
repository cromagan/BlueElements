using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.BlueTableDialogs;
using BlueControls.Controls;

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
            this.tableVariablen = new TableViewWithFilters();
            this.SuspendLayout();
            // 
            // tableVariablen
            // 
            this.tableVariablen.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                           | AnchorStyles.Left) 
                                                          | AnchorStyles.Right)));
            this.tableVariablen.Location = new Point(8, 64);
            this.tableVariablen.Name = "tableVariablen";
            this.tableVariablen.Size = new Size(483, 303);
            this.tableVariablen.TabIndex = 2;
            this.tableVariablen.Text = "tabVariablen";
            // 
            // VariableEditor
            // 
            this.Controls.Add(this.tableVariablen);
            this.Name = "VariableEditor";
            this.Size = new Size(502, 375);
            this.ResumeLayout(false);

        }

        #endregion
        private TableViewWithFilters tableVariablen;
    }
}
