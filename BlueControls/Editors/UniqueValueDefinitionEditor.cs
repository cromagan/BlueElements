// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Editoren;
using BlueTable.Interfaces;

namespace BlueControls.Forms;

/// <summary>
/// Editor für eine einzelne <see cref="UniqueValueDefinition"/> (EditCopy). Die
/// anzuzeigenden Elemente liefert die Backend-Collection
/// <see cref="Table.UniqueValues"/> direkt als
/// <see cref="EditorForIEnumerable.InputItem"/>. Neue Definitionen
/// erzeugt das hostende Form über das AddClicked-Event des
/// <see cref="EditorForIEnumerable"/>.
/// </summary>
public partial class UniqueValueDefinitionEditor : EditorEasy, IHasTable, INotifyPropertyChanged {

    #region Constructors

    public UniqueValueDefinitionEditor() : base() => InitializeComponent();

    #endregion

    #region Events

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public override Type? EditorFor => typeof(UniqueValueDefinition);
    public override EditorMode SupportedModes => EditorMode.EditCopy | EditorMode.EditItem;

    /// <summary>
    /// Die Tabelle, deren Unique-Value-Regeln bearbeitet werden. Unabhängig vom
    /// selektierten Element — wird vom Host gesetzt.
    /// </summary>
    [DefaultValue(null)]
    public Table? Table { get; set; } = null;

    #endregion

    #region Methods

    public override void Clear() => lbxKeyColumns.ItemClear();

    public override object? CreateNewItem() {
        if (Table is not { IsDisposed: false } tb) { return null; }

        var colnam = lbxKeyColumns.CheckedItems.OfType<ReadableListItem>().Select(rli => rli.Item).OfType<ColumnItem>().ToList();
        return new UniqueValueDefinition(tb, colnam);
    }

    protected override void InitializeComponentDefaultValues() { }

    protected override bool SetValuesToFormula(object? toEdit) {
        if (toEdit is not UniqueValueDefinition { } uvd) { return false; }

        lbxKeyColumns.ItemAddRange(ItemsOf(uvd.Table.Column.Where(c => c.RelationType == RelationType.None)));

        foreach (var thisColumn in uvd.KeyColumns) {
            if (thisColumn is { IsDisposed: false }) {
                lbxKeyColumns.Check(thisColumn.KeyName);
            }
        }

        return true;
    }

    // EditorEasy.InputItem setzt field während Clear/SetValuesToFormula auf null
    // (analog zum _item = null Pattern im TableScriptEditor).
    // In dieser Phase keine Change-Events feuern.
    private void lbxKeyColumns_ItemCheckedChanged(object? sender, System.EventArgs e) {
        if (InputItem is null) { return; }

        // EditCopy-Mode: das OutputItem ändert sich, weil es aus den
        // angekreuzten Spalten neu erzeugt wird.
        OnPropertyChanged("Columns");
    }

    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}