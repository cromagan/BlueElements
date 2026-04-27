// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Classes.ItemCollectionList;

public class ReadableListItem : TextListItem {

    #region Fields

    private IReadableTextWithKey _item;

    #endregion

    #region Constructors

    public ReadableListItem(IReadableTextWithKey item, bool isCaption, bool enabled, string userDefCompareKey) : base(item.ReadableText(), item.KeyName, item.SymbolForReadableText(), isCaption, enabled, item.QuickInfo, userDefCompareKey) {
        Item = item;
        _item = item;
    }

    #endregion

    #region Properties

    public IReadableTextWithKey Item {
        get => _item;
        set {
            if (_item == value) { return; }

            if (_item is INotifyPropertyChanged it2) {
                it2.PropertyChanged += Item_PropertyChanged;
            }

            _item = value;
            Update();

            if (_item is INotifyPropertyChanged it3) {
                it3.PropertyChanged += Item_PropertyChanged;
            }

            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e) => Update();

    private void Update() {
        Text = Item.ReadableText();
        Symbol = Item.SymbolForReadableText();
        KeyName = Item.KeyName;
        QuickInfo = Item.QuickInfo;
    }

    #endregion
}