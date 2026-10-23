// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.BlueTableDialogs {
    public sealed partial class ColumnOverview {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            tblSpalten = new TableViewWithFilters();
            capSpalten = new Caption();
            pnlStatusBar.SuspendLayout();
            SuspendLayout();
            //
            // capStatusBar
            //
            capStatusBar.Location = new Point(208, 0);
            capStatusBar.Size = new Size(992, 24);
            //
            // pnlStatusBar
            //
            pnlStatusBar.Controls.Add(capSpalten);
            pnlStatusBar.Location = new Point(0, 776);
            pnlStatusBar.Size = new Size(1200, 24);
            pnlStatusBar.Controls.SetChildIndex(capSpalten, 0);
            pnlStatusBar.Controls.SetChildIndex(capStatusBar, 0);
            //
            // capSpalten
            //
            capSpalten.CausesValidation = false;
            capSpalten.Dock = DockStyle.Left;
            capSpalten.Location = new Point(0, 0);
            capSpalten.Name = "capSpalten";
            capSpalten.Size = new Size(208, 24);
            capSpalten.Translate = false;
            //
            // tblSpalten
            //
            tblSpalten.Ansichtbearbeitung = false;
            tblSpalten.ColumnMoveAllowed = false;
            tblSpalten.ContextMenuDefault = false;
            tblSpalten.Dock = DockStyle.Fill;
            tblSpalten.Location = new Point(0, 0);
            tblSpalten.Name = "tblSpalten";
            tblSpalten.PowerEdit = false;
            tblSpalten.Size = new Size(1200, 776);
            tblSpalten.TabIndex = 1;
            //
            // ColumnOverview
            //
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1200, 800);
            Controls.Add(tblSpalten);
            GlobalMenuHeight = 0;
            MinimumSize = new Size(600, 400);
            Name = "ColumnOverview";
            StartPosition = FormStartPosition.CenterScreen;
            Tag = "";
            Text = "Spaltenübersicht";
            Controls.SetChildIndex(pnlStatusBar, 0);
            Controls.SetChildIndex(tblSpalten, 0);
            pnlStatusBar.ResumeLayout(false);
            ResumeLayout(false);

        }

        private TableViewWithFilters tblSpalten;
        private Caption capSpalten;
    }
}
