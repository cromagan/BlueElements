// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollectionList;

public static class AbstractListItemExtension {

    #region Methods

    public static TextListItem Item(string internalAndReadableText) => ItemOf(internalAndReadableText, internalAndReadableText, null, false, true, string.Empty);

    public static TextListItem ItemOf(ColumnItem column) => ItemOf((IReadableTextWithPropertyChangingAndKey)column);

    public static CellLikeListItem ItemOf(string value, ColumnItem? columnStyle, ShortenStyle style, BildTextVerhalten bildTextverhalten) => new(value, columnStyle, style, true, bildTextverhalten);

    public static TextListItem ItemOf(ContextMenuCommands command, bool enabled = true) {
        var internalName = command.ToString();
        QuickImage? symbol;
        string? readableText;
        switch (command) {
            case ContextMenuCommands.DateiPfadÖffnen:
                readableText = "Dateipfad öffnen";
                symbol = QuickImage.Get("Ordner|16");
                break;

            case ContextMenuCommands.Abbruch:
                readableText = "Abbrechen";
                symbol = QuickImage.Get("TasteESC|16");
                break;

            case ContextMenuCommands.Bearbeiten:
                readableText = "Bearbeiten";
                symbol = QuickImage.Get(ImageCode.Stift);
                break;

            case ContextMenuCommands.Kopieren:
                readableText = "Kopieren";
                symbol = QuickImage.Get(ImageCode.Kopieren);
                break;

            case ContextMenuCommands.InhaltLöschen:
                readableText = "Inhalt löschen";
                symbol = QuickImage.Get(ImageCode.Radiergummi);
                break;

            case ContextMenuCommands.ZeileLöschen:
                readableText = "Zeile löschen";
                symbol = QuickImage.Get("Zeile|16|||||||||Kreuz");
                break;

            case ContextMenuCommands.DateiÖffnen:
                readableText = "Öffnen / Ausführen";
                symbol = QuickImage.Get(ImageCode.Blitz);
                break;

            case ContextMenuCommands.SpaltenSortierungAZ:
                readableText = "Nach dieser Spalte aufsteigend sortieren";
                symbol = QuickImage.Get("AZ|16|8");
                break;

            case ContextMenuCommands.SpaltenSortierungZA:
                readableText = "Nach dieser Spalte absteigend sortieren";
                symbol = QuickImage.Get("ZA|16|8");
                break;

            case ContextMenuCommands.Information:
                readableText = "Informationen anzeigen";
                symbol = QuickImage.Get(ImageCode.Frage);
                break;

            case ContextMenuCommands.ZellenInhaltKopieren:
                readableText = "Zelleninhalt kopieren";
                symbol = QuickImage.Get(ImageCode.Kopieren);
                break;

            case ContextMenuCommands.ZellenInhaltPaste:
                readableText = "In Zelle einfügen";
                symbol = QuickImage.Get(ImageCode.Clipboard);
                break;

            case ContextMenuCommands.SpaltenEigenschaftenBearbeiten:
                readableText = "Spalteneigenschaften bearbeiten";
                symbol = QuickImage.Get("Spalte|16|||||||||Stift");
                break;

            case ContextMenuCommands.Speichern:
                readableText = "Speichern";
                symbol = QuickImage.Get(ImageCode.Diskette);
                break;

            case ContextMenuCommands.Löschen:
                readableText = "Löschen";
                symbol = QuickImage.Get(ImageCode.Kreuz);
                break;

            case ContextMenuCommands.Umbenennen:
                readableText = "Umbenennen";
                symbol = QuickImage.Get(ImageCode.Stift);
                break;

            case ContextMenuCommands.SuchenUndErsetzen:
                readableText = "Suchen und ersetzen";
                symbol = QuickImage.Get(ImageCode.Fernglas);
                break;

            case ContextMenuCommands.Einfügen:
                readableText = "Einfügen";
                symbol = QuickImage.Get(ImageCode.Clipboard);
                break;

            case ContextMenuCommands.Ausschneiden:
                readableText = "Ausschneiden";
                symbol = QuickImage.Get(ImageCode.Schere);
                break;

            case ContextMenuCommands.VorherigenInhaltWiederherstellen:
                readableText = "Vorherigen Inhalt wieder herstellen";
                symbol = QuickImage.Get(ImageCode.Undo);
                break;

            case ContextMenuCommands.WeitereBefehle:
                readableText = "Weitere Befehle";
                symbol = QuickImage.Get(ImageCode.Hierarchie);
                break;

            default:
                Develop.DebugPrint(command);
                readableText = internalName;
                symbol = QuickImage.Get(ImageCode.Fragezeichen);
                break;
        }
        if (string.IsNullOrEmpty(internalName)) { Develop.DebugPrint(FehlerArt.Fehler, "Interner Name nicht vergeben:" + command); }
        return ItemOf(readableText, internalName, symbol, enabled);
    }

    /// <summary>
    /// Als Interner Name wird der RowKey als String abgelegt
    /// </summary>
    public static RowFormulaListItem ItemOf(RowItem row, string layoutId, string userDefCompareKey) => new(row, layoutId, userDefCompareKey);

    /// <summary>
    /// Als Interner Name wird der RowKey als String abgelegt
    /// </summary>
    public static RowFormulaListItem ItemOf(RowItem row, string layoutId) => ItemOf(row, layoutId, string.Empty);

    public static BitmapListItem ItemOf(string filename, string internalname, string caption) => new(filename, internalname, caption);

    public static BitmapListItem ItemOf(Bitmap? bmp, string caption) => new(bmp, string.Empty, caption);

    public static TextListItem ItemOf(string readableText, string internalname, QuickImage? symbol, bool isCaption, bool enabled, string userDefCompareKey) => new(readableText, internalname, symbol, isCaption, enabled, userDefCompareKey);

    public static TextListItem ItemOf(string readableText, string internalname, ImageCode symbol, bool isCaption, bool enabled, string userDefCompareKey) => ItemOf(readableText, internalname, QuickImage.Get(symbol, 16), isCaption, enabled, userDefCompareKey);

    public static TextListItem ItemOf(string readableText, string internalname, QuickImage? symbol) => ItemOf(readableText, internalname, symbol, false, true, string.Empty);

    public static TextListItem ItemOf(string readableText, string internalname, ImageCode symbol) => ItemOf(readableText, internalname, symbol, false, true, string.Empty);

    public static TextListItem ItemOf(string readableText, string internalname) => ItemOf(readableText, internalname, null, false, true, string.Empty);

    public static TextListItem ItemOf(string readableText, string internalname, QuickImage? symbol, bool enabled, string userDefCompareKey) => ItemOf(readableText, internalname, symbol, false, enabled, userDefCompareKey);

    public static TextListItem ItemOf(string readableText, string internalname, QuickImage? symbol, bool enabled) => ItemOf(readableText, internalname, symbol, false, enabled, string.Empty);

    public static TextListItem ItemOf(string readableText, string internalname, ImageCode symbol, bool enabled, string userDefCompareKey) => ItemOf(readableText, internalname, symbol, false, enabled, userDefCompareKey);

    public static TextListItem ItemOf(string readableText, string internalname, ImageCode symbol, bool enabled) => ItemOf(readableText, internalname, symbol, false, enabled, string.Empty);

    public static TextListItem ItemOf(string readableText, string internalname, bool enabled) => ItemOf(readableText, internalname, null, false, enabled, string.Empty);

    public static TextListItem ItemOf(string internalAndReadableText, ImageCode symbol) => ItemOf(internalAndReadableText, internalAndReadableText, symbol, false, true, string.Empty);

    public static TextListItem ItemOf(string internalAndReadableText, SortierTyp format) => ItemOf(internalAndReadableText, internalAndReadableText, null, false, true, internalAndReadableText.CompareKey(format));

    public static TextListItem ItemOf(string internalAndReadableText, bool isCaption) => ItemOf(internalAndReadableText, internalAndReadableText, null, isCaption, true, string.Empty);

    public static TextListItem ItemOf(string readableText, string internalname, bool isCaption, string userDefCompareKey) => ItemOf(readableText, internalname, null, isCaption, true, userDefCompareKey);

    /// <summary>
    /// Fügt das übergebende Object den Tags hinzu.
    /// </summary>
    /// <param name="readableObject"></param>
    public static ReadableListItem ItemOf(IReadableTextWithKey readableObject) => new(readableObject, false, true, string.Empty);

    public static List<CellLikeListItem> ItemsOf(ColumnItem column, RowItem? checkedItemsAtRow, ShortenStyle style, int maxItems) {
        List<string> l = [];

        if (column.IsDisposed) { return []; }

        l.AddRange(column.DropDownItems);
        if (column.DropdownWerteAndererZellenAnzeigen) { l.AddRange(column.Contents()); }

        switch (column.Function) {
            case ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems:
                var db2 = column.LinkedDatabase;
                if (db2 == null) { Notification.Show("Verknüpfte Datenbank nicht vorhanden", ImageCode.Information); return []; }

                // Spalte aus der Ziel-Datenbank ermitteln
                var targetColumn = db2.Column[column.LinkedCell_ColumnNameOfLinkedDatabase];
                if (targetColumn == null) { Notification.Show("Die Spalte ist in der Zieldatenbank nicht vorhanden."); return []; }

                var (fc, info) = CellCollection.GetFilterFromLinkedCellData(db2, column, checkedItemsAtRow);
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
                    Notification.Show("Keine Zeilen in der Quell-Datenbank vorhanden.", ImageCode.Information);
                }
                break;
        }

        if (checkedItemsAtRow?.Database is Database db && !db.IsDisposed) {
            l.AddRange(checkedItemsAtRow.CellGetList(column));
            l = l.SortedDistinctList();
        }

        if (maxItems > 0 && l.Count > maxItems) { return []; }

        return ItemsOf(l, column, style, column.BehaviorOfImageAndText);
    }

    public static List<TextListItem> ItemsOf(IEnumerable<ColumnItem> columns, bool doCaptionSort) {
        var l = new List<TextListItem>();

        foreach (var thisColumnItem in columns) {
            if (thisColumnItem != null) {
                var co = ItemOf(thisColumnItem);

                if (doCaptionSort) {
                    co.UserDefCompareKey = thisColumnItem.Ueberschriften + Constants.SecondSortChar + thisColumnItem.KeyName;

                    var capt = thisColumnItem.Ueberschriften;

                    l.Add(new TextListItem(capt, capt, null, true, true, capt + Constants.FirstSortChar));
                }
            }
        }

        return l;
    }

    public static List<RowFormulaListItem> ItemsOf(IEnumerable<RowItem?>? list, string layoutId) {
        var l = new List<RowFormulaListItem>();
        if (list == null) { return l; }

        foreach (var thisRow in list) {
            if (thisRow != null && !thisRow.IsDisposed) {
                l.Add(ItemOf(thisRow, layoutId));
            }
        }

        return l;
    }

    public static List<TextListItem> ItemsOf(IEnumerable<string>? list) {
        var l = new List<TextListItem>();
        if (list == null) { return l; }

        foreach (var thisitem in list) {
            if (thisitem != null) { l.Add(Item(thisitem)); }
        }
        return l;
    }

    public static List<CellLikeListItem> ItemsOf(ICollection<string>? values, ColumnItem? columnStyle, ShortenStyle style, BildTextVerhalten bildTextverhalten) {
        var l = new List<CellLikeListItem>();

        if (values == null) { return l; }
        if (values.Count > 10000) {
            Develop.DebugPrint(FehlerArt.Fehler, "Values > 100000");
            return l;
        }
        foreach (var thisstring in values) {
            l.Add(ItemOf(thisstring, columnStyle, style, bildTextverhalten));
        }

        return l;
    }

    /// <summary>
    /// Fügt eine Enumeration hinzu.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<TextListItem> ItemsOf(Type type) {
        var l = new List<TextListItem>();
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

        Develop.DebugPrint(FehlerArt.Fehler, "Typ unbekannt");
        return l;
    }

    public static LineListItem Separator() => SeparatorWith(string.Empty);

    public static LineListItem SeparatorWith(string userDefCompareKey) => new(string.Empty, userDefCompareKey);

    #endregion
}