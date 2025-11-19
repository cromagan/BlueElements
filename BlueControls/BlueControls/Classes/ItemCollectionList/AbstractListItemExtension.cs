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
using BlueControls.BlueTableDialogs;
using BlueControls.CellRenderer;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueTable;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;

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

    public static TextListItem ItemOf(IEditable edit) => ItemOf(edit.CaptionForEditor + " bearbeiten", ImageCode.Stift, Conextmenu_OpenEditor, edit, true);

    public static TextListItem ItemOf(string keyNameAndReadableText) => ItemOf(keyNameAndReadableText, keyNameAndReadableText, null, false, true, string.Empty);

    public static TextListItem ItemOf(ColumnItem column) => ItemOf((IReadableTextWithKey)column);

    public static CellLikeListItem ItemOf(string value, ColumnItem columnStyle, Renderer_Abstract cellRenderer) => new(value, cellRenderer, true, columnStyle.DoOpticalTranslation, (Alignment)columnStyle.Align, columnStyle.SortType);

    /// <summary>
    /// Als Interner Name wird der RowKey als String abgelegt
    /// </summary>
    public static RowFormulaListItem ItemOf(RowItem row, string layoutId, string userDefCompareKey) => new(row, layoutId, userDefCompareKey);

    /// <summary>
    /// Als Interner Name wird der RowKey als String abgelegt
    /// </summary>
    public static RowFormulaListItem ItemOf(RowItem row, string layoutId) => ItemOf(row, layoutId, string.Empty);

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

    public static List<AbstractListItem> ItemsOf(ColumnItem column, RowItem? checkedItemsAtRow, int maxItems, Renderer_Abstract cellRenderer) {
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

        return maxItems > 0 && l.Count > maxItems ? [] : ItemsOf(l, column, cellRenderer);
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

    public static List<AbstractListItem> ItemsOf(ICollection<string>? values, ColumnItem columnStyle, Renderer_Abstract renderer) {
        var l = new List<AbstractListItem>();

        if (values == null) { return l; }
        if (values.Count > 10000) {
            Develop.DebugPrint(ErrorType.Error, "Values > 100000");
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
                var n = Enum.GetName(type, z1);
                if (n != null) {
                    l.Add(ItemOf(n.Replace("_", " "), z1.ToString()));
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

    public static LineListItem Separator() => SeparatorWith(string.Empty);

    public static LineListItem SeparatorWith(string userDefCompareKey) => new(string.Empty, userDefCompareKey);

    public static void Conextmenu_OpenEditor(object sender, ObjectEventArgs e) {
        if (e.Data is not IEditable edit) { return; }
        edit.Edit();
    }

    #endregion
}