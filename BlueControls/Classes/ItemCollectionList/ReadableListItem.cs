// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Classes.ItemCollectionList;

public class ReadableListItem : TextListItem {

    #region Constructors

    public ReadableListItem(IReadableTextWithKey item, bool isCaption, bool enabled, string userDefCompareKey) : this(item, item.KeyName, isCaption, enabled, item.QuickInfo, userDefCompareKey) { }

    public ReadableListItem(IReadableText item, string keyName, bool isCaption, bool enabled, string quickinfo, string userDefCompareKey) : base(item.ReadableText(), keyName, item.SymbolForReadableText(), isCaption, enabled, quickinfo, userDefCompareKey) {
        Item = item;
    }

    #endregion

    #region Properties

    public IReadableText? Item {
        get;
        private set {
            if (field == value) { return; }

            if (field is INotifyPropertyChanged npc) {
                npc.PropertyChanged -= Item_PropertyChanged;
            }

            if (field is IDisposableExtendedWithEvent disposable) {
                disposable.DisposingEvent -= Item_DisposingEvent;
            }

            field = value;
            Update();

            if (field is INotifyPropertyChanged npc2) {
                npc2.PropertyChanged += Item_PropertyChanged;
            }

            if (field is IDisposableExtendedWithEvent disposable2) {
                disposable2.DisposingEvent += Item_DisposingEvent;
            }
        }
    }

    #endregion

    #region Methods

    private void Item_DisposingEvent(object? sender, System.EventArgs e) => Item = null;

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e) => Update();

    private void Update() {
        if (Item is IDisposableExtended di && di.IsDisposed) {
            Item = null;
            return; // Item seter geht nochmal rein
        }

        if (Item == null) {
            Text = "Verworfen";
            Symbol = QuickImage.Get(ImageCode.Kritisch, 16);
            QuickInfo = string.Empty;
            return;
        }

        Text = Item.ReadableText();
        Symbol = Item.SymbolForReadableText();
        if (Item is IHasKeyName hkn) { KeyName = hkn.KeyName; }
        if (Item is IReadableTextWithKey rtk) { QuickInfo = rtk.QuickInfo; }
    }

    #endregion
}