// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using BlueControls.BlueTableDialogs;
using BlueControls.CellRenderer;
using BlueControls.Forms;
using BlueTable;
using BlueTable.Enums;
using System.Threading.Tasks;
using System.Linq;

namespace BlueControls.ItemCollectionList;

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

    public static void Conextmenu_OpenEditor(object sender, ObjectEventArgs e) {
        if (e.Data is not IEditable edit) { return; }
        edit.Edit();
    }

    public static void DrawItems(this List<AbstractListItem>? list, Graphics gr, Rectangle visArea, AbstractListItem? _mouseOverItem, int shiftX, int shiftY, string FilterText, States controlState, Design _controlDesign, Design _itemDesign, Design checkboxDesign, List<string>? _checked, float scale) {
        try {
            object locker = new();
            Parallel.ForEach(list, thisItem => {
                if (thisItem.IsVisible(visArea)) {
                    var itemState = controlState;
                    if (_mouseOverItem == thisItem && !controlState.HasFlag(States.Checked_Disabled)) { itemState |= States.Standard_MouseOver; }

                    if (!thisItem.Enabled) { itemState = States.Standard_Disabled; }

                    if (_checked?.Contains(thisItem.KeyName) ?? false) { itemState |= States.Checked; }

                    lock (locker) {
                        thisItem.Draw(gr, visArea, shiftX, shiftY, _controlDesign, _itemDesign, itemState, true, FilterText, false, checkboxDesign, scale);
                    }
                }
            });
        } catch { }
    }

    public static AbstractListItem? ElementAtPosition(this List<AbstractListItem>? list, int x, int y, float shiftX, float shiftY) => list.FirstOrDefault(thisItem => thisItem?.Visible == true && thisItem?.Contains((int)(x + shiftX), (int)(y + shiftY)) == true);

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
        if (list == null) { return null; }

        for (int i = 0; i < list.Count; i++) {
            if (list[i] is T typedItem && list[i].Visible) {
                return typedItem;
            }
        }

        return null;
    }

    /// <summary>
    ///  BiggestItemX, BiggestItemY, HeightAdded, SenkrechtAllowed
    /// </summary>
    /// <returns></returns>
    public static (int BiggestItemX, int BiggestItemY, int HeightAdded, Orientation Orientation) ItemData(this List<AbstractListItem> item, Design itemDesign) {
        try {
            var w = 16;
            var h = 0;
            var hall = 0;
            var sameh = -1;
            var or = Orientation.Senkrecht;
            PreComputeSize(item, itemDesign);

            foreach (var thisItem in item) {
                if (thisItem != null && thisItem.Visible) {
                    var s = thisItem.SizeUntouchedForListBox(itemDesign);
                    w = Math.Max(w, s.Width);
                    h = Math.Max(h, s.Height);
                    hall += s.Height;
                    if (sameh < 0) {
                        sameh = thisItem.SizeUntouchedForListBox(itemDesign).Height;
                    } else {
                        if (sameh != thisItem.SizeUntouchedForListBox(itemDesign).Height) { or = Orientation.Waagerecht; }
                        sameh = thisItem.SizeUntouchedForListBox(itemDesign).Height;
                    }
                    if (thisItem is not TextListItem) { or = Orientation.Waagerecht; }
                }
            }

            return (w, h, hall, or);
        } catch {
            Develop.AbortAppIfStackOverflow();
            return ItemData(item, itemDesign);
        }
    }

    public static TextListItem ItemOf(IEditable edit) => ItemOf(edit.CaptionForEditor + " bearbeiten", ImageCode.Stift, Conextmenu_OpenEditor, edit, true);

    public static TextListItem ItemOf(string keyNameAndReadableText) => ItemOf(keyNameAndReadableText, keyNameAndReadableText, null, false, true, string.Empty);

    public static TextListItem ItemOf(ColumnItem column) => ItemOf((IReadableTextWithKey)column);

    public static CellLikeListItem ItemOf(string value, ColumnItem columnStyle, Renderer_Abstract cellRenderer, object? tag) => new(value, cellRenderer, true, columnStyle.DoOpticalTranslation, (Alignment)columnStyle.Align, columnStyle.SortType, tag);

    /// <summary>
    /// Als Interner Name wird der RowKey als String abgelegt
    /// </summary>
    public static RowFormulaListItem ItemOf(RowItem row, string layoutId, string userDefCompareKey, object? tag) => new(row, layoutId, userDefCompareKey);

    /// <summary>
    /// Als Interner Name wird der RowKey als String abgelegt
    /// </summary>
    public static RowFormulaListItem ItemOf(RowItem row, string layoutId, object? tag) => ItemOf(row, layoutId, string.Empty, tag);

    public static BitmapListItem ItemOf(string filename, string keyName, string caption) => new(filename, keyName, caption);

    public static BitmapListItem ItemOf(Bitmap? bmp, string caption) => new(bmp, string.Empty, caption);

    public static TextListItem ItemOf(string readableText, string keyName, QuickImage? symbol, bool isCaption, bool enabled, string userDefCompareKey) => new(readableText, keyName, symbol, isCaption, enabled, userDefCompareKey);

    public static TextListItem ItemOf(string readableText, string keyName, ImageCode symbol, bool isCaption, bool enabled, string userDefCompareKey) => ItemOf(readableText, keyName, QuickImage.Get(symbol, 16), isCaption, enabled, userDefCompareKey);

    public static TextListItem ItemOf(string readableText, string keyName, QuickImage? symbol) => ItemOf(readableText, keyName, symbol, false, true, string.Empty);

    public static TextListItem ItemOf(string readableText, string keyName, ImageCode symbol) => ItemOf(readableText, keyName, symbol, false, true, string.Empty);

    public static TextListItem ItemOf(string readableText, string keyName) => ItemOf(readableText, keyName, null, false, true, string.Empty);

    public static TextListItem ItemOf(string readableText, string keyName, QuickImage? symbol, bool enabled, string userDefCompareKey) => ItemOf(readableText, keyName, symbol, false, enabled, userDefCompareKey);

    public static TextListItem ItemOf(string readableText, string keyName, QuickImage? symbol, bool enabled) => ItemOf(readableText, keyName, symbol, false, enabled, string.Empty);

    public static TextListItem ItemOf(string readableText, ImageCode symbol, EventHandler<ObjectEventArgs> click, object? tag, bool enabled) => ItemOf(readableText, QuickImage.Get(symbol, 16), click, tag, enabled);

    public static TextListItem ItemOf(string readableText, QuickImage? symbol, EventHandler<ObjectEventArgs> click, object? tag, bool enabled) {
        var i = ItemOf(readableText, string.Empty, symbol, false, enabled, string.Empty);
        i.Tag = tag;
        i.LeftClickExecute += click;
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
    public static ReadableListItem ItemOf(IReadableTextWithKey readableObject) => new(readableObject, false, true, string.Empty);

    public static ReadableListItem ItemOf(IReadableTextWithKey readableObject, string userDefCompareKey) => new(readableObject, false, true, userDefCompareKey);

    public static List<AbstractListItem> ItemsOf(ColumnItem column, RowItem? checkedItemsAtRow, int maxItems, Renderer_Abstract cellRenderer, object? tag) {
        List<string> l = [];

        if (column.IsDisposed) { return []; }

        l.AddRange(column.DropDownItems);
        if (column.ShowValuesOfOtherCellsInDropdown) { l.AddRange(column.Contents()); }

        if (column.RelationType == RelationType.DropDownValues) {
            var db2 = column.LinkedTable;
            if (db2 == null) { Notification.Show("Verknüpfte Tabelle nicht vorhanden", ImageCode.Information); return []; }

            // Spalte aus der Ziel-Tabelle ermitteln
            var targetColumn = db2.Column[column.ColumnNameOfLinkedTable];
            if (targetColumn == null) { Notification.Show("Die Spalte ist in der Zieltabelle nicht vorhanden."); return []; }

            var (fc, info) = CellCollection.GetFilterFromLinkedCellData(db2, column, checkedItemsAtRow, null);
            if (!string.IsNullOrEmpty(info)) {
                Notification.Show(info, ImageCode.Information);
                return [];
            }

            if (fc == null) {
                Notification.Show("Keine Filterung definiert.", ImageCode.Information);
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

        return maxItems > 0 && l.Count > maxItems ? [] : ItemsOf(l, column, cellRenderer, tag);
    }

    public static List<AbstractListItem> ItemsOf(IEnumerable<ColumnItem> columns, bool doCaptionSort) {
        var l = new List<AbstractListItem>();

        List<string> cl = [""];

        foreach (var thisColumnItem in columns) {
            if (thisColumnItem != null) {
                var co = ItemOf(thisColumnItem);

                thisColumnItem.Editor = typeof(ColumnEditor);

                if (doCaptionSort) {
                    var capt = thisColumnItem.CaptionsCombined;

                    co.UserDefCompareKey = capt + Constants.SecondSortChar + thisColumnItem.KeyName;

                    if (!cl.Contains(capt)) {
                        cl.Add(capt);
                        l.Add(new TextListItem(capt, capt, null, true, true, capt + Constants.FirstSortChar));
                    }
                }

                l.Add(co);
            }
        }

        return l;
    }

    public static List<AbstractListItem> ItemsOf(IEnumerable<string>? list) {
        var l = new List<AbstractListItem>();
        if (list == null) { return l; }

        foreach (var thisitem in list) {
            if (thisitem != null) { l.Add(ItemOf(thisitem)); }
        }
        return l;
    }

    public static List<AbstractListItem> ItemsOf(ICollection<string>? values, ColumnItem columnStyle, Renderer_Abstract renderer, object? tag) {
        var l = new List<AbstractListItem>();

        if (values == null) { return l; }
        if (values.Count > 10000) {
            Develop.DebugPrint(ErrorType.Error, "Values > 100000");
            return l;
        }

        foreach (var thisstring in values) {
            l.Add(ItemOf(thisstring, columnStyle, renderer, tag));
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
                var n = Enum.GetName(type, z1);
                if (n != null) {
                    l.Add(ItemOf(n.Replace("_", " "), z1.ToStringInt1()));
                }
            }
            return l;
        }

        if (underlyingType == typeof(byte)) {
            foreach (byte z1 in Enum.GetValues(type)) {
                var n = Enum.GetName(type, z1);
                if (n != null) {
                    l.Add(ItemOf(n.Replace("_", " "), z1.ToString()));
                }
            }
            return l;
        }

        Develop.DebugPrint(ErrorType.Error, "Typ unbekannt");
        return l;
    }

    /// <summary>
    /// Gibt das letzte sichtbare Element vom Typ <typeparamref name="T"/> in der Liste zurück.
    /// </summary>
    /// <typeparam name="T">Der Typ des gesuchten Elements, muss von <see cref="AbstractListItem"/> erben.</typeparam>
    /// <param name="list">Die Liste, in der gesucht werden soll.</param>
    /// <returns>
    /// Das letzte sichtbare Element vom Typ <typeparamref name="T"/>, oder <c>null</c>,
    /// wenn kein passendes Element gefunden wurde oder die Liste ungültig ist.
    /// </returns>
    /// <example>
    /// <code>
    /// var items = new List&lt;AbstractListItem&gt; { item1, item2, item3 };
    /// var lastButton = items.Last&lt;ButtonItem&gt;();
    /// </code>
    /// </example>
    public static T? Last<T>(this List<AbstractListItem>? list) where T : AbstractListItem? {
        if (list == null) { return null; }

        for (int i = list.Count - 1; i >= 0; i--) {
            if (list[i] is T typedItem && list[i].Visible) {
                return typedItem;
            }
        }

        return null;
    }

    /// <summary>
    /// Gibt das nächste sichtbare Element vom Typ <typeparamref name="T"/> in der Liste zurück.
    /// </summary>
    /// <typeparam name="T">Der Typ des gesuchten Elements, muss von <see cref="AbstractListItem"/> erben.</typeparam>
    /// <param name="list">Die Liste, in der gesucht werden soll.</param>
    /// <param name="currentItem">Das aktuelle Element, ab dem gesucht wird.</param>
    /// <returns>
    /// Das nächste sichtbare Element vom Typ <typeparamref name="T"/>, oder <c>null</c>,
    /// wenn kein passendes Element gefunden wurde oder die Eingabeparameter ungültig sind.
    /// </returns>
    /// <example>
    /// <code>
    /// var items = new List&lt;AbstractListItem&gt; { item1, item2, item3 };
    /// var nextButton = items.Next&lt;ButtonItem&gt;(currentItem);
    /// </code>
    /// </example>
    public static T? Next<T>(this List<AbstractListItem>? list, AbstractListItem? currentItem) where T : AbstractListItem? {
        if (list == null || currentItem == null) { return null; }

        int currentIndex = list.IndexOf(currentItem);
        if (currentIndex < 0) { return null; }

        for (int i = currentIndex + 1; i < list.Count; i++) {
            if (list[i] is T typedItem && list[i].Visible) {
                return typedItem;
            }
        }

        return null;
    }

    public static void PreComputeSize(this List<AbstractListItem> item, Design itemDesign) {
        try {
            Parallel.ForEach(item, thisItem => thisItem?.SizeUntouchedForListBox(itemDesign));
        } catch {
            Develop.AbortAppIfStackOverflow();
            PreComputeSize(item, itemDesign);
        }
    }

    /// <summary>
    /// Gibt das vorherige sichtbare Element vom Typ <typeparamref name="T"/> in der Liste zurück.
    /// </summary>
    /// <typeparam name="T">Der Typ des gesuchten Elements, muss von <see cref="AbstractListItem"/> erben.</typeparam>
    /// <param name="list">Die Liste, in der gesucht werden soll.</param>
    /// <param name="currentItem">Das aktuelle Element, ab dem rückwärts gesucht wird.</param>
    /// <returns>
    /// Das vorherige sichtbare Element vom Typ <typeparamref name="T"/>, oder <c>null</c>,
    /// wenn kein passendes Element gefunden wurde oder die Eingabeparameter ungültig sind.
    /// </returns>
    /// <example>
    /// <code>
    /// var items = new List&lt;AbstractListItem&gt; { item1, item2, item3 };
    /// var previousButton = items.Previous&lt;ButtonItem&gt;(currentItem);
    /// </code>
    /// </example>
    public static T? Previous<T>(this List<AbstractListItem>? list, AbstractListItem? currentItem) where T : AbstractListItem? {
        if (list == null || currentItem == null) { return null; }

        int currentIndex = list.IndexOf(currentItem);
        if (currentIndex < 0) { return null; }

        for (int i = currentIndex - 1; i >= 0; i--) {
            if (list[i] is T typedItem && list[i].Visible) {
                return typedItem;
            }
        }

        return null;
    }

    public static LineListItem Separator() => SeparatorWith(string.Empty);

    public static LineListItem SeparatorWith(string userDefCompareKey) => new(string.Empty, userDefCompareKey);

    #endregion
}

public abstract class AbstractListItem : IComparable, IHasKeyName, INotifyPropertyChanged {

    #region Fields

    private Size _sizeUntouchedForListBox = Size.Empty;

    #endregion

    #region Constructors

    protected AbstractListItem(string keyName, bool enabled) {
        KeyName = string.IsNullOrEmpty(keyName) ? Generic.GetUniqueKey() : keyName;
        if (string.IsNullOrEmpty(KeyName)) { Develop.DebugPrint(ErrorType.Error, "Interner Name nicht vergeben."); }
        Enabled = enabled;
        Position = Rectangle.Empty;
        UserDefCompareKey = string.Empty;
    }

    #endregion

    #region Events

    public event EventHandler? CompareKeyChanged;

    public event EventHandler<ObjectEventArgs>? LeftClickExecute;

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public bool Enabled {
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

    public bool KeyIsCaseSensitive => false;

    public string KeyName {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public Rectangle Position {
        get;
        set {
            if (field.Equals(value)) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public abstract string QuickInfo { get; }

    /// <summary>
    /// Falls eine spezielle Information gespeichert und zurückgegeben werden soll
    /// </summary>
    /// <remarks></remarks>
    public object? Tag {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

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

    public int CompareTo(object obj) {
        if (obj is AbstractListItem tobj) {
            return string.Compare(CompareKey(), tobj.CompareKey(), StringComparison.OrdinalIgnoreCase);
        }

        Develop.DebugPrint(ErrorType.Error, "Falscher Objecttyp!");
        return 0;
    }

    public bool Contains(int x, int y) => Position.Contains(x, y);

    public void Draw(Graphics gr, Rectangle visibleArea, float shiftX, float shiftY, Design controldesign, Design itemdesign, States state, bool drawBorderAndBack, string filterText, bool translate, Design checkboxDesign, float scale) {
        if (itemdesign == Design.Undefiniert) { return; }
        var positionModifiedi = Position with { X = (int)(Position.X + (Indent * 20)), Y = (int)(Position.Y), Width = (int)(Position.Width - (Indent * 20)) };
        var positionModifiedf = positionModifiedi.ZoomAndMoveRect(scale, shiftX, shiftY, false);

        if (checkboxDesign != Design.Undefiniert) {
            var design = Skin.DesignOf(checkboxDesign, state);
            gr.DrawImage(QuickImage.Get(design.Image, Controls.ZoomPad.GetPix(12, scale)), positionModifiedf.X + Controls.ZoomPad.GetPix(4, scale), positionModifiedf.Y + Controls.ZoomPad.GetPix(3, scale));
            positionModifiedf.X += Controls.ZoomPad.GetPix(20, scale);
            positionModifiedf.Width -= Controls.ZoomPad.GetPix(20, scale);
            if (state.HasFlag(States.Checked)) { state ^= States.Checked; }
        }

        DrawExplicit(gr, visibleArea, positionModifiedf, itemdesign, state, drawBorderAndBack, translate, shiftX, shiftY, scale);
        if (drawBorderAndBack) {
            if (!string.IsNullOrEmpty(filterText) && !FilterMatch(filterText)) {
                var c1 = Skin.Color_Back(controldesign, States.Standard); // Standard als Notlösung, um nicht doppelt checken zu müssen
                c1 = c1.SetAlpha(160);
                gr.FillRectangle(new SolidBrush(c1), positionModifiedf);
            }
        }
    }

    public virtual bool FilterMatch(string filterText) => KeyName.ToUpperInvariant().Contains(filterText.ToUpperInvariant());

    public abstract int HeightForListBox(ListBoxAppearance style, int columnWidth, Design itemdesign);

    public virtual bool IsClickable() => true;

    public void OnCompareKeyChanged() => CompareKeyChanged?.Invoke(this, System.EventArgs.Empty);

    public Size SizeUntouchedForListBox(Design itemdesign) {
        if (_sizeUntouchedForListBox.IsEmpty) {
            _sizeUntouchedForListBox = ComputeSizeUntouchedForListBox(itemdesign);
        }
        return _sizeUntouchedForListBox;
    }

    internal bool IsVisible(Rectangle visArea) => Visible && Position.IntersectsWith(visArea);

    internal void OnLeftClickExecute() => LeftClickExecute?.Invoke(this, new ObjectEventArgs(Tag));

    protected abstract Size ComputeSizeUntouchedForListBox(Design itemdesign);

    protected abstract void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionModified, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float shiftX, float shiftY, float scale);

    protected abstract string GetCompareKey();

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}