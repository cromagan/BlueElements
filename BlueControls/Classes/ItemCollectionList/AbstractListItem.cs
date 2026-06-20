// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;
using BlueControls.Renderer;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BlueControls.Classes.ItemCollectionList;

public static class AbstractListItemExtension {

    #region Methods

    public static List<AbstractListItem> AllAvailableTables() {
        var ld = Table.AllAvailableTables();
        var ld2 = new List<AbstractListItem>();
        foreach (var thisd in ld) {
            ld2.Add(ItemOf(thisd.FileNameWithoutSuffix(), thisd));
        }
        return ld2;
    }

    /// <summary>
    ///  BiggestItemX, BiggestItemY, HeightAdded, SenkrechtAllowed
    /// </summary>
    /// <returns></returns>
    public static (int BiggestItemX, int BiggestItemY, int HeightAdded, Orientation Orientation) CanvasItemData(this List<AbstractListItem> item, Design itemDesign) {
        try {
            var w = 16;
            var h = 0;
            var hall = 0;
            var sameh = -1;
            var or = Orientation.Senkrecht;
            item.PreComputeSize(itemDesign);

            foreach (var thisItem in item) {
                if (thisItem is { Visible: true }) {
                    var s = thisItem.UntrimmedCanvasSize(itemDesign);
                    w = Math.Max(w, s.Width);
                    h = Math.Max(h, s.Height);
                    hall += s.Height;
                    if (sameh < 0) {
                        sameh = s.Height;
                    } else {
                        if (sameh != s.Height) { or = Orientation.Waagerecht; }
                        sameh = s.Height;
                    }
                    if (thisItem is not TextListItem) { or = Orientation.Waagerecht; }
                }
            }

            return (w, h, hall, or);
        } catch {
            Develop.AbortAppIfStackOverflow();
            return item.CanvasItemData(itemDesign);
        }
    }

    public static void DrawItems(this IEnumerable<AbstractListItem>? list, Graphics gr, Rectangle visControlArea, AbstractListItem? _mouseOverItem, int offsetX, int offsetY, string FilterText, States controlState, Design _controlDesign, Design _itemDesign, Design checkboxDesign, List<string>? _checked, float zoom) {
        if (list is null) { return; }

        try {
            foreach (var thisItem in list) {
                if (thisItem.IsVisible(visControlArea, zoom, offsetX, offsetY)) {
                    var itemState = controlState;
                    if (_mouseOverItem == thisItem && !controlState.HasFlag(States.Checked_Disabled)) { itemState |= States.Standard_MouseOver; }

                    if (!thisItem.Enabled || controlState.HasFlag(States.Standard_Disabled)) { itemState = States.Standard_Disabled; }

                    if (_checked?.Contains(thisItem.KeyName) ?? false) { itemState |= States.Checked; }

                    thisItem.Draw(gr, visControlArea, offsetX, offsetY, _controlDesign, _itemDesign, itemState, true, FilterText, false, checkboxDesign, zoom);
                }
            }
        } catch { }
    }

    public static AbstractListItem? ElementAtPosition(this List<AbstractListItem>? list, int controlX, int controlY, float zoom, float offsetX, float offsetY) {
        if (list is not { Count: > 0 }) { return null; }

        for (var i = list.Count - 1; i >= 0; i--) {
            var thisItem = list[i];
            if (thisItem?.Visible == true && thisItem.ControlPosition(zoom, offsetX, offsetY).Contains(controlX, controlY)) {
                return thisItem;
            }
        }
        return null;
    }

    /// <summary>
    /// Gibt das erste sichtbare Element vom Typ <typeparamref name="T"/> in der Liste zurück.
    /// </summary>
    /// <typeparam name="T">Der Typ des gesuchten Elements, muss von <see cref="AbstractListItem"/> erben.</typeparam>
    /// <param name="list">Die Liste, in der gesucht werden soll.</param>
    /// <returns>
    /// Das erste sichtbare Element vom Typ <typeparamref name="T"/>, oder <c>null</c>,
    /// wenn kein passendes Element gefunden wurde oder die Liste ungültig ist.
    /// </returns>
    /// <example>
    /// <code>
    /// var items = new List&lt;AbstractListItem&gt; { item1, item2, item3 };
    /// var firstButton = items.First&lt;ButtonItem&gt;();
    /// </code>
    /// </example>
    public static T? First<T>(this List<AbstractListItem>? list) where T : AbstractListItem? {
        if (list is not { Count: > 0 }) { return null; }

        for (var i = 0; i < list.Count; i++) {
            if (list[i] is T typedItem && list[i].Visible) {
                return typedItem;
            }
        }

        return null;
    }

    public static TextListItem ItemOf(IEditable edit) {
        void OnClick(object? sender, ContextMenuEventArgs e) => edit.Edit();
        return ItemOf(edit.CaptionForEditor + " bearbeiten", ImageCode.Stift, OnClick, true);
    }

    public static TextListItem ItemOf(string keyNameAndReadableText) => ItemOf(keyNameAndReadableText, keyNameAndReadableText, null, false, true, string.Empty);

    public static ReadableListItem ItemOf(ColumnItem column) => ItemOf((IReadableTextWithKey)column);

    public static CellLikeListItem ItemOf(string value, ColumnItem columnStyle, Renderer_Abstract cellRenderer) => new(value, cellRenderer, true, columnStyle.DoOpticalTranslation, (Alignment)columnStyle.Align, columnStyle.SortType);

    /// <summary>
    /// Als Interner Name wird der RowKey als String abgelegt
    /// </summary>
    public static RowFormulaListItem ItemOf(RowItem row, string layoutId, string userDefCompareKey) => new(row, layoutId, userDefCompareKey);

    /// <summary>
    /// Als Interner Name wird der RowKey als String abgelegt
    /// </summary>
    public static RowFormulaListItem ItemOf(RowItem row, string layoutId) => ItemOf(row, layoutId, string.Empty);

    public static BitmapListItem ItemOf(string filename, string keyName, string caption, string quickInfo) => new(filename, keyName, caption, quickInfo);

    public static BitmapListItem ItemOf(Bitmap? bmp, string caption, string quickInfo) => new(bmp, string.Empty, caption, quickInfo);

    public static TextListItem ItemOf(string readableText, string keyName, QuickImage? symbol, bool isCaption, bool enabled, string userDefCompareKey) => new(readableText, keyName, symbol, isCaption, enabled, string.Empty, userDefCompareKey);

    public static TextListItem ItemOf(string readableText, string keyName, ImageCode symbol, bool isCaption, bool enabled, string userDefCompareKey) => ItemOf(readableText, keyName, QuickImage.Get(symbol, 16), isCaption, enabled, userDefCompareKey);

    public static TextListItem ItemOf(string readableText, string keyName, QuickImage? symbol) => ItemOf(readableText, keyName, symbol, false, true, string.Empty);

    public static TextListItem ItemOf(string readableText, string keyName, ImageCode symbol) => ItemOf(readableText, keyName, symbol, false, true, string.Empty);

    public static TextListItem ItemOf(string readableText, string keyName) => ItemOf(readableText, keyName, null, false, true, string.Empty);

    public static TextListItem ItemOf(string readableText, string keyName, QuickImage? symbol, bool enabled, string userDefCompareKey) => ItemOf(readableText, keyName, symbol, false, enabled, userDefCompareKey);

    public static TextListItem ItemOf(string readableText, string keyName, QuickImage? symbol, bool enabled) => ItemOf(readableText, keyName, symbol, false, enabled, string.Empty);

    public static TextListItem ItemOf(string readableText, ImageCode symbol, EventHandler<ContextMenuEventArgs> click, bool enabled) => ItemOf(readableText, QuickImage.Get(symbol, 16), string.Empty, click, enabled, string.Empty);

    public static TextListItem ItemOf(string readableText, QuickImage? symbol, EventHandler<ContextMenuEventArgs> click, bool enabled, string quickInfo) => ItemOf(readableText, symbol, string.Empty, click, enabled, quickInfo);

    public static TextListItem ItemOf(string readableText, QuickImage? symbol, string keyName, EventHandler<ContextMenuEventArgs> click, bool enabled, string quickInfo) {
        var i = ItemOf(readableText, keyName, symbol, false, enabled, string.Empty);
        i.LeftClickExecute = click;
        i.QuickInfo = quickInfo;
        return i;
    }

    public static TextListItem ItemOf(string readableText, string keyName, ImageCode symbol, bool enabled, string userDefCompareKey) => ItemOf(readableText, keyName, symbol, false, enabled, userDefCompareKey);

    public static TextListItem ItemOf(string readableText, string keyName, ImageCode symbol, bool enabled) => ItemOf(readableText, keyName, symbol, false, enabled, string.Empty);

    public static TextListItem ItemOf(string readableText, string keyName, bool enabled) => ItemOf(readableText, keyName, null, false, enabled, string.Empty);

    public static TextListItem ItemOf(string keyNameAndReadableText, ImageCode symbol) => ItemOf(keyNameAndReadableText, keyNameAndReadableText, symbol, false, true, string.Empty);

    public static TextListItem ItemOf(string keyNameAndReadableText, SortierTyp format) => ItemOf(keyNameAndReadableText, keyNameAndReadableText, null, false, true, keyNameAndReadableText.CompareKey(format));

    public static TextListItem ItemOf(string keyNameAndReadableText, bool isCaption) => ItemOf(keyNameAndReadableText, keyNameAndReadableText, null, isCaption, true, string.Empty);

    public static TextListItem ItemOf(string readableText, string keyName, bool isCaption, string userDefCompareKey) => ItemOf(readableText, keyName, null, isCaption, true, userDefCompareKey);

    /// <summary>
    /// Fügt das übergebende Object den Tags hinzu.
    /// </summary>
    /// <param name="readableObject"></param>
    public static ReadableListItem ItemOf(IReadableTextWithKey readableObject) => new(readableObject, true, string.Empty);

    public static ReadableListItem ItemOf(IReadableText readableObject, string keyName) => new(readableObject, keyName, true, string.Empty, string.Empty);

    public static TextListItem ItemOf(string readableText, string keyName, QuickImage? symbol, EventHandler<ContextMenuEventArgs> click, bool enabled, string quickInfo) {
        var i = ItemOf(readableText, keyName, symbol, false, enabled, string.Empty);
        i.LeftClickExecute = click;
        i.QuickInfo = quickInfo;
        return i;
    }

    public static TextListItem ItemOf(string readableText, string keyName, ImageCode symbol, EventHandler<ContextMenuEventArgs> click, bool enabled, string quickInfo) => ItemOf(readableText, keyName, QuickImage.Get(symbol, 16), click, enabled, quickInfo);

    public static TextListItem ItemOf(string readableText, string keyName, ImageCode symbol, EventHandler<ContextMenuEventArgs> click, bool enabled) => ItemOf(readableText, keyName, symbol, click, enabled, string.Empty);

    public static TextListItem ItemOf(string readableText, string keyName, EventHandler<ContextMenuEventArgs> click, bool enabled) => ItemOf(readableText, keyName, null, click, enabled, string.Empty);

    public static List<AbstractListItem> ItemsOf(ColumnItem column, RowItem? checkedItemsAtRow, int maxItems, Renderer_Abstract cellRenderer) {
        List<string> l = [];

        if (column.IsDisposed) { return []; }

        l.AddRange(column.DropDownItems);
        if (column.ShowValuesOfOtherCellsInDropdown) { l.AddRange(column.Contents()); }

        if (column.RelationType == RelationType.DropDownValues) {
            var tbLinked = column.LinkedTable;
            if (tbLinked is null) { QuickNote.Show(NoteSymbols.Warning, "Verknüpfte Tabelle fehlt"); return []; }

            // Spalte aus der Ziel-Tabelle ermitteln
            var targetColumn = tbLinked.Column[column.ColumnKeyOfLinkedTable];
            if (targetColumn is null) { QuickNote.Show(NoteSymbols.Warning, "Verknüpfte Spalte fehlt"); return []; }

            var result = CellCollection.GetFilterFromLinkedCellData(tbLinked, column, checkedItemsAtRow, null);
            if (result.IsFailed) {
                QuickNote.Show(NoteSymbols.Critical, "Fehler");
                return [];
            }

            if (result.Value is not FilterCollection { } fc) {
                QuickNote.Show(NoteSymbols.Warning, "Kein Filter definiert");
                return [];
            }

            l.AddRange(targetColumn.Contents(fc, null));

            if (l.Count == 0) {
                Notification.Show("Keine Zeilen in der Quell-Tabelle vorhanden.", ImageCode.Information);
            }
        }

        if (checkedItemsAtRow?.Table is { IsDisposed: false }) {
            l.AddRange(checkedItemsAtRow.CellGetList(column));
            l = l.SortedDistinctList();
        }

        return maxItems > 0 && l.Count > maxItems ? [] : ItemsOf(l, column, cellRenderer);
    }

    public static List<AbstractListItem> ItemsOf(IEnumerable<ColumnItem> columns, bool doCaptionSort) {
        var l = new List<AbstractListItem>();

        List<string> cl = [string.Empty];

        foreach (var thisColumnItem in columns) {
            if (thisColumnItem is not null) {
                var co = ItemOf(thisColumnItem);

                if (doCaptionSort) {
                    var capt = thisColumnItem.CaptionsCombined;

                    co.UserDefCompareKey = capt + Constants.SecondSortChar + thisColumnItem.KeyName;

                    if (!cl.Contains(capt)) {
                        cl.Add(capt);
                        l.Add(new TextListItem(capt, capt, null, true, true, string.Empty, capt + Constants.FirstSortChar));
                    }
                }

                l.Add(co);
            }
        }

        return l;
    }

    public static List<AbstractListItem> ItemsOf(IEnumerable<string>? list) {
        var l = new List<AbstractListItem>();
        if (list is null) { return l; }

        foreach (var thisitem in list) {
            if (thisitem is { } ti) { l.Add(ItemOf(ti)); }
        }
        return l;
    }

    public static List<AbstractListItem> ItemsOf(ICollection<string>? values, ColumnItem columnStyle, Renderer_Abstract renderer) {
        var l = new List<AbstractListItem>();

        if (values is null) { return l; }
        if (values.Count > 10000) {
            Develop.DebugError("Values > 100000");
            return l;
        }

        foreach (var thisstring in values) {
            l.Add(ItemOf(thisstring, columnStyle, renderer));
        }

        return l;
    }

    /// <summary>
    /// Fügt eine Enumeration hinzu.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<AbstractListItem> ItemsOf(Type type) {
        var l = new List<AbstractListItem>();
        var underlyingType = Enum.GetUnderlyingType(type);

        if (underlyingType == typeof(int)) {
            foreach (int z1 in Enum.GetValues(type)) {
                if (Enum.GetName(type, z1) is { } n) {
                    l.Add(ItemOf(n.Replace('_', ' '), z1.ToString1()));
                }
            }
            return l;
        }

        if (underlyingType == typeof(byte)) {
            foreach (byte z1 in Enum.GetValues(type)) {
                if (Enum.GetName(type, z1) is { } n) {
                    l.Add(ItemOf(n.Replace('_', ' '), z1.ToString1()));
                }
            }
            return l;
        }

        if (underlyingType == typeof(long)) {
            foreach (long z1 in Enum.GetValues(type)) {
                if (Enum.GetName(type, z1) is { } n) {
                    l.Add(ItemOf(n.Replace('_', ' '), z1.ToString1()));
                }
            }
            return l;
        }

        Develop.DebugError("Typ unbekannt");
        return l;
    }

    public static void PreComputeSize(this List<AbstractListItem> item, Design itemDesign) {
        try {
            Parallel.ForEach(item, thisItem => thisItem?.UntrimmedCanvasSize(itemDesign));
        } catch {
            Develop.AbortAppIfStackOverflow();
            item.PreComputeSize(itemDesign);
        }
    }

    public static LineListItem Separator() => SeparatorWith(string.Empty);

    public static LineListItem SeparatorWith(string userDefCompareKey) => new(string.Empty, userDefCompareKey);

    #endregion
}

public abstract class AbstractListItem : IComparable, IHasKeyName, INotifyPropertyChanged, IDisposableExtended {

    #region Fields

    private volatile int _isDisposedFlag;
    private Size _untrimmedCanvasSize = Size.Empty;

    #endregion

    #region Constructors

    protected AbstractListItem(string keyName, bool enabled) {
        KeyName = string.IsNullOrEmpty(keyName) ? Generic.GetUniqueKey() : keyName;
        if (string.IsNullOrEmpty(KeyName)) { Develop.DebugError("Interner Name nicht vergeben."); }
        Enabled = enabled;
        CanvasPosition = Rectangle.Empty;
        UserDefCompareKey = string.Empty;
    }

    #endregion

    #region Events

    public event EventHandler? CompareKeyChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public Rectangle CanvasPosition {
        get;
        set {
            if (field.Equals(value)) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool Enabled {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IgnoreXOffset {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IgnoreYOffset {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public int Indent {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsDisposed => _isDisposedFlag == 1;

    public string KeyName {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public EventHandler<ContextMenuEventArgs>? LeftClickExecute { get; set; }

    // Es wird mit Zeilenschlüsseln gearbeitet
    public string QuickInfo { get; set; } = string.Empty;

    public bool RemoveLocked { get; set; }

    public string UserDefCompareKey {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnCompareKeyChanged();
            OnPropertyChanged();
        }
    }

    public bool Visible {
        get;
        set {
            if (field == value) { return; }

            field = value;
            OnPropertyChanged();
        }
    } = true;

    #endregion

    #region Methods

    public string CompareKey() {
        if (!string.IsNullOrEmpty(UserDefCompareKey)) {
            if (UserDefCompareKey.Length > 0 && UserDefCompareKey[0] < 32) { Develop.DebugPrint("Sortierung inkorrekt: " + UserDefCompareKey); }

            return UserDefCompareKey;// + Constants.FirstSortChar + Parent?.IndexOf(this).ToString(Constants.Format_Integer6);
        }
        return GetCompareKey();
    }

    public int CompareTo(object? obj) {
        if (obj is AbstractListItem tobj) {
            return string.Compare(CompareKey(), tobj.CompareKey(), StringComparison.OrdinalIgnoreCase);
        }

        Develop.DebugError("Falscher Objecttyp!");
        return 0;
    }

    /// <summary>
    /// Spezielle Berechnung, doe die Ignore-Werte berücksichtigt
    /// </summary>
    /// <param name="zoom"></param>
    /// <param name="offsetX"></param>
    /// <param name="offsetY"></param>
    /// <returns></returns>
    public Rectangle ControlPosition(float zoom, float offsetX, float offsetY) {
        if (IgnoreYOffset) { offsetY = 0; }
        if (IgnoreXOffset) { offsetX = 0; }

        return CanvasPosition.CanvasToControl(zoom, offsetX, offsetY, true);
    }

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void Draw(Graphics gr, Rectangle visibleArea, float offsetX, float offsetY, Design controldesign, Design itemdesign, States state, bool drawBorderAndBack, string filterText, bool translate, Design checkboxDesign, float zoom) {
        if (itemdesign == Design.Undefined) { return; }

        var controlPos = ControlPosition(zoom, offsetX, offsetY);
        var p20 = 20.CanvasToControl(zoom) * Indent;
        var controlIndented = new Rectangle(controlPos.X + p20, controlPos.Y, controlPos.Width - p20, controlPos.Height);

        if (checkboxDesign != Design.Undefined) {
            var design = IsClickable()
                ? Skin.DesignOf(checkboxDesign, state)
                : Skin.DesignOf(checkboxDesign, States.Standard_Disabled);
            gr.DrawImageUnscaled(QuickImage.Get(design.Image, 12.CanvasToControl(zoom)), controlIndented.X + 4.CanvasToControl(zoom), controlIndented.Y + 3.CanvasToControl(zoom));
            controlIndented.X += 20.CanvasToControl(zoom);
            controlIndented.Width -= 20.CanvasToControl(zoom);
            if (state.HasFlag(States.Checked)) { state ^= States.Checked; }
        }

        if (state.HasFlag(States.Standard_Disabled)) {
            state &= ~(States.Standard_MouseOver | States.Standard_MousePressed | States.Standard_HasFocus);
        }

        DrawExplicit(gr, visibleArea, controlIndented, itemdesign, state, drawBorderAndBack, translate, offsetX, offsetY, zoom);
        if (drawBorderAndBack) {
            if (!string.IsNullOrEmpty(filterText) && !FilterMatch(filterText)) {
                var c1 = Skin.Color_Back(controldesign, States.Standard);// Standard als Notlösung, um nicht doppelt checken zu müssen
                c1 = c1.SetAlpha(160);
                var fb = BackgroundFill.GetBrush(c1);
                lock (fb) { gr.FillRectangle(fb, controlIndented); }
            }
        }
    }

    public virtual bool FilterMatch(string filterText) => KeyName.Contains(filterText, StringComparison.OrdinalIgnoreCase);

    public abstract int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign);

    public virtual bool IsClickable() => true;

    public void OnCompareKeyChanged() => CompareKeyChanged?.Invoke(this, System.EventArgs.Empty);

    public Size UntrimmedCanvasSize(Design itemdesign) {
        if (_untrimmedCanvasSize.IsEmpty) {
            _untrimmedCanvasSize = ComputeUntrimmedCanvasSize(itemdesign);
        }
        return _untrimmedCanvasSize;
    }

    internal bool IsVisible(Rectangle controlArea, float zoom, float offsetX, float offsetY) => Visible && ControlPosition(zoom, offsetX, offsetY).IntersectsWith(controlArea);

    protected abstract Size ComputeUntrimmedCanvasSize(Design itemdesign);

    protected virtual void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (disposing) {
            PropertyChanged = null;
            CompareKeyChanged = null;
            LeftClickExecute = null;
        }
    }

    protected abstract void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float zoom);

    protected abstract string GetCompareKey();

    protected void Invalidate_UntrimmedCanvasSize() => _untrimmedCanvasSize = Size.Empty;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}