// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Editoren;
using BlueControls.EventArgs;
using BlueTable.Interfaces;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueTable.Editoren;

public partial class FilterCollectionEditor : EditorEasy, IHasTable {

    #region Constructors

    public FilterCollectionEditor() {
        InitializeComponent();
        lstFilterItems.AddMethod = AddFilter;
    }

    #endregion

    #region Properties

    public override Type? EditorFor => typeof(FilterCollection);
    public override EditorMode SupportedModes => EditorMode.EditItem;
    public Table? Table { get; private set; }

    #endregion

    #region Methods

    public override void Clear() {
        lstFilterItems.ItemClear();
        filterItemEditor.InputItem = null;
        Table = null;
    }

    public override object? CreateNewItem() {
        WriteCurrentFilterBack();
        return InputItem;
    }

    protected override void InitializeComponentDefaultValues() { }

    protected override void SetEnabledState(bool enabled) {
        base.SetEnabledState(enabled);
        lstFilterItems.AddAllowed = enabled ? BlueControls.Enums.AddType.UserDef : BlueControls.Enums.AddType.None;
        lstFilterItems.RemoveAllowed = enabled;
        filterItemEditor.Mode = enabled ? Mode : EditorMode.OnlyShow;
    }

    protected override bool SetValuesToFormula(object? toEdit) {
        if (toEdit is not FilterCollection fc) { return false; }

        Table = fc.Table;
        filterItemEditor.Mode = Mode;
        UpdateFilterList();
        return true;
    }

    private AbstractListItem? AddFilter() {
        WriteCurrentFilterBack();
        if (InputItem is not FilterCollection fc) { return null; }
        if (Table is not { IsDisposed: false } tb) { return null; }

        var col = tb.Column.FirstOrDefault(c => !c.IsSystemColumn());
        if (col == null) { return null; }

        var newFi = new FilterItem(col, FilterType.Istgleich, string.Empty);
        fc.Add(newFi);

        return ItemOf(newFi, string.Empty);
    }

    private FilterItem? GetSelectedFilter() {
        if (lstFilterItems.CheckedItems.Count != 1) { return null; }
        return (lstFilterItems.CheckedItems[0] as ReadableListItem)?.Item as FilterItem;
    }

    private void lstFilterItems_ItemClicked(object sender, AbstractListItemEventArgs e) {
        WriteCurrentFilterBack();

        filterItemEditor.Mode = Mode;
        filterItemEditor.InputItem = GetSelectedFilter();
    }

    private void lstFilterItems_RemoveClicked(object sender, AbstractListItemEventArgs e) {
        if (InputItem is not FilterCollection fc) { return; }

        if (lstFilterItems[e.Item.KeyName] is not ReadableListItem rli) { return; }
        if (rli.Item is not FilterItem fi) { return; }

        fc.Remove(fi);
    }

    private void SelectFilter(FilterItem? fi) {
        if (fi == null) { return; }

        for (var i = 0; i < lstFilterItems.ItemCount; i++) {
            if (lstFilterItems[i] is ReadableListItem rli && ReferenceEquals(rli.Item, fi)) {
                lstFilterItems.Check(rli.KeyName);
                return;
            }
        }
    }

    private void UpdateFilterList() {
        if (InputItem is not FilterCollection fc) { return; }

        lstFilterItems.ItemClear();

        foreach (var fi in fc) {
            lstFilterItems.ItemAdd(ItemOf(fi, string.Empty));
        }
    }

    private void WriteCurrentFilterBack() {
        if (InputItem is not FilterCollection fc) { return; }
        var selectedFilter = GetSelectedFilter();
        if (selectedFilter == null) { return; }
        if (((IIsEditor)filterItemEditor).OutputItem is not FilterItem editedFi) { return; }

        if (editedFi.Equals(selectedFilter)) { return; }

        fc.Remove(selectedFilter);
        fc.Add(editedFi);

        UpdateFilterList();
        SelectFilter(editedFi);
    }

    #endregion
}