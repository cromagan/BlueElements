// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.BlueTableDialogs {
    public sealed partial class FragmentEditor {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            tblFragments = new TableViewWithFilters();
            SuspendLayout();
            //
            // tblFragments
            //
            tblFragments.Ansichtbearbeitung = false;
            tblFragments.ColumnMoveAllowed = false;
            tblFragments.Dock = DockStyle.Fill;
            tblFragments.Location = new Point(0, 0);
            tblFragments.Name = "tblFragments";
            tblFragments.PowerEdit = false;
            tblFragments.Size = new Size(984, 569);
            tblFragments.TabIndex = 0;
            //
            // FragmentEditor
            //
            AutoScaleMode = AutoScaleMode.None;
            Controls.Add(tblFragments);
            Name = "FragmentEditor";
            Size = new Size(984, 569);
            ResumeLayout(false);
        }

        /// <summary>
        /// Verwendete Ressourcen bereinigen; die temporäre Arbeits-Tabelle wird explizit freigegeben.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                // Die Arbeits-Tabelle aus EnsureTable() gehört allein diesem Editor und wird
                // vom Control nicht mitfreigegeben; TableViewWithFilters reagiert auf das
                // Disposed-Ereignis selbst und entfernt die Referenz.
                if (tblFragments.Table is { IsDisposed: false } tb) {
                    tb.Dispose();
                }

                if (components is not null) {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private System.ComponentModel.IContainer components = null;
        private TableViewWithFilters tblFragments;
    }
}
