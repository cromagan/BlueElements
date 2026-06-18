// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.BlueTableDialogs;
using BlueControls.Controls;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

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
            tableVariablen = new TableViewWithFilters();
            SuspendLayout();
            // 
            // tableVariablen
            // 
            tableVariablen.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableVariablen.Ansichtbearbeitung = false;
            tableVariablen.Location = new Point(8, 24);
            tableVariablen.Name = "tableVariablen";
            tableVariablen.PowerEdit = false;
            tableVariablen.Size = new Size(483, 343);
            tableVariablen.TabIndex = 2;
            tableVariablen.Text = "tabVariablen";
            // 
            // VariableEditor
            // 
            Controls.Add(tableVariablen);
            Name = "VariableEditor";
            Size = new Size(502, 375);
            ResumeLayout(false);

        }

        #endregion
        private TableViewWithFilters tableVariablen;
    }
}
