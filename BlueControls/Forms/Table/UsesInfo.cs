// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Windows.Forms;

namespace BlueControls.BlueTableDialogs;

public sealed partial class UsesInfo : FormWithStatusBar {

    #region Constructors

    public UsesInfo() : base() =>
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

    #endregion

    #region Methods

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);
        tblUndo.Table?.Dispose();
    }

    protected override void OnShown(System.EventArgs e) {
        base.OnShown(e);
        TableHeadEditor.GenerateUndoTabelle(tblUndo);

        if (tblUndo.Table is { IsDisposed: false } tb) {
            tb.SuppressEvents();
            try {
                foreach (var thisTb in Table.AllFiles) {
                    if (thisTb is TableFile) {
                        TableHeadEditor.AddUndosToTable(tblUndo, thisTb, 0.5f);
                    }
                }
            } finally {
                tb.ResumeEvents();
            }
        }

        tblUndo.Table?.Freeze("Nur Ansicht");
    }

    #endregion
}