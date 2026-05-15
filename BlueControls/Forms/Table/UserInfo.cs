// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Windows.Forms;

namespace BlueControls.BlueTableDialogs;

public sealed partial class UserInfo : FormWithStatusBar {

    #region Constructors

    public UserInfo() : base() =>
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

        foreach (var thisTb in Table.AllFiles) {
            if (thisTb is TableFile) {
                TableHeadEditor.AddUndosToTable(tblUndo, thisTb, 0.5f);
            }
        }

        tblUndo.Table?.Freeze("Nur Ansicht");
    }

    #endregion
}