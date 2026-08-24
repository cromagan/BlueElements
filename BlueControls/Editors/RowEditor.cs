// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.Interfaces;

namespace BlueControls.Editoren;

public partial class RowEditor : EditorEasy, IHasTable {

    #region Constructors

    public RowEditor() => InitializeComponent();

    #endregion

    #region Properties

    public override Type? EditorFor => typeof(RowItem);
    public override EditorMode SupportedModes => EditorMode.EditItem;
    public Table? Table => InputItem is RowItem { IsDisposed: false } r ? r.Table : null;

    #endregion

    #region Methods

    public override void Clear() => formular.Page = null;

    protected override void InitializeComponentDefaultValues() { }

    protected override void SetEnabledState(bool enabled) {
        base.SetEnabledState(enabled);
        formular.Enabled = enabled;
    }

    protected override bool SetValuesToFormula(object? toEdit) {
        RowItem? row = null;
        if (toEdit is RowItem r) { row = r; }

        formular.GetHeadPageFrom(row?.Table);
        formular.SetToRow(row);

        return true;
    }

    #endregion
}