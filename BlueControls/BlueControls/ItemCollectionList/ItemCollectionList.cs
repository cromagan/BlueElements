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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.ItemCollectionList;

public class ItemCollectionList : ObservableCollection<AbstractListItem>, ICloneable, IChangedFeedback {

    #region Fields

    private BlueListBoxAppearance _appearance;

    private bool _autoSort;

    private CheckBehavior _checkBehavior;

    private Design _controlDesign;

    private Design _itemDesign;

    private ReadOnlyCollection<AbstractListItem>? _itemOrder;

    private SizeF _lastCheckedMaxSize = Size.Empty;

    private Size _maxNeededItemSize;

    #endregion

    #region Constructors

    public ItemCollectionList(bool autosort) : this(BlueListBoxAppearance.Listbox, autosort) { }

    public ItemCollectionList(BlueListBoxAppearance design, bool autosort) : base() {
        BindingOperations.EnableCollectionSynchronization(this, new object());

        _maxNeededItemSize = Size.Empty;
        _appearance = BlueListBoxAppearance.Listbox;
        _itemDesign = Design.Undefiniert;
        _controlDesign = Design.Undefiniert;
        _checkBehavior = CheckBehavior.SingleSelection;
        _appearance = design;
        _autoSort = autosort;
        GetDesigns();
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    public event EventHandler? ItemCheckedChanged;

    #endregion

    #region Properties

    public BlueListBoxAppearance Appearance {
        get => _appearance;
        set {
            if (value == _appearance && _itemDesign != Design.Undefiniert) { return; }
            _appearance = value;
            GetDesigns();
            //DesignOrStyleChanged();
            OnChanged();
        }
    }

    public bool AutoSort {
        get => _autoSort;
        set {
            if (value == _autoSort) { return; }
            _autoSort = value;
            _maxNeededItemSize = Size.Empty;
            _itemOrder = null;
            OnChanged();
        }
    }

    public int BreakAfterItems { get; private set; }

    public CheckBehavior CheckBehavior {
        get => _checkBehavior;
        set {
            if (value == _checkBehavior) { return; }
            _checkBehavior = value;
            ValidateCheckStates(null);
        }
    }

    /// <summary>
    /// ControlDesign wird durch Appearance gesetzt
    /// </summary>
    /// <returns></returns>
    public Design ControlDesign //Implements IDesignAble.Design
    {
        get {
            if (_controlDesign == Design.Undefiniert) { Develop.DebugPrint(FehlerArt.Fehler, "ControlDesign undefiniert!"); }
            return _controlDesign;
        }
    }

    /// <summary>
    /// Itemdesign wird durch Appearance gesetzt
    /// </summary>
    /// <returns></returns>
    public Design ItemDesign //Implements IDesignAble.Design
    {
        get {
            if (_itemDesign == Design.Undefiniert) { Develop.DebugPrint(FehlerArt.Fehler, "ItemDesign undefiniert!"); }
            return _itemDesign;
        }
    }

    public ReadOnlyCollection<AbstractListItem> ItemOrder {
        get {
            _itemOrder ??= CalculateItemOrder();
            return _itemOrder;
        }
    }

    #endregion

    #region Indexers

    public AbstractListItem? this[int x, int y] => this.FirstOrDefault(thisItem => thisItem != null && thisItem.Contains(x, y));

    public AbstractListItem? this[string @internal] {
        get {
            try {
                if (string.IsNullOrEmpty(@internal)) { return null; }

                return this.FirstOrDefault(thisItem => thisItem != null && string.Equals(@internal, thisItem.Internal, StringComparison.OrdinalIgnoreCase));
            } catch {
                Develop.CheckStackForOverflow();
                return this[@internal];
            }
        }
    }

    #endregion

    #region Methods

    public static void GetItemCollection(ItemCollectionList e, ColumnItem column, RowItem? checkedItemsAtRow, ShortenStyle style, int maxItems) {
        List<string> marked = [];
        List<string> l = [];

        e.Clear();
        e.CheckBehavior = CheckBehavior.MultiSelection; // Es kann ja mehr als nur eines angewählt sein, auch wenn nicht erlaubt!
        if (column.IsDisposed) { return; }

        l.AddRange(column.DropDownItems);
        if (column.DropdownWerteAndererZellenAnzeigen) { l.AddRange(column.Contents()); }

        switch (column.Format) {
            case DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems:
                var db2 = column.LinkedDatabase;
                if (db2 == null) { Notification.Show("Verknüpfte Datenbank nicht vorhanden", ImageCode.Information); return; }

                // Spalte aus der Ziel-Datenbank ermitteln
                var targetColumn = db2.Column.Exists(column.LinkedCell_ColumnNameOfLinkedDatabase);
                if (targetColumn == null) { Notification.Show("Die Spalte ist in der Zieldatenbank nicht vorhanden."); return; }

                var (fc, info) = CellCollection.GetFilterFromLinkedCellData(db2, column, checkedItemsAtRow);
                if (!string.IsNullOrEmpty(info)) {
                    Notification.Show(info, ImageCode.Information);
                    return;
                }

                if (fc == null) {
                    Notification.Show("Keine Filterung definiert.", ImageCode.Information);
                    return;
                }

                l.AddRange(targetColumn.Contents(fc, null));
                if (l.Count == 0) {
                    Notification.Show("Keine Zeilen in der Quell-Datenbank vorhanden.", ImageCode.Information);
                }
                break;
        }

        if (checkedItemsAtRow?.Database is Database db) {
            if (db.Row.Count > 0) {
                if (!checkedItemsAtRow.CellIsNullOrEmpty(column)) {
                    if (column.MultiLine) {
                        marked = checkedItemsAtRow.CellGetList(column);
                    } else {
                        marked.Add(checkedItemsAtRow.CellGetString(column));
                    }
                }
                l.AddRange(marked);
            }
            l = l.SortedDistinctList();
        }

        if (maxItems > 0 && l.Count > maxItems) { return; }

        e.AddRange(l, column, style, column.BehaviorOfImageAndText);
        if (checkedItemsAtRow != null) {
            foreach (var t in marked) {
                if (e[t] is AbstractListItem bli) { bli.Checked = true; }
            }
        }
    }

    public TextListItem Add(string internalAndReadableText) => Add(internalAndReadableText, internalAndReadableText, null, false, true, string.Empty);

    /// <summary>
    /// Fügt das übergebende Object den Tags hinzu.
    /// </summary>
    /// <param name="readableObject"></param>
    public TextListItem Add(IReadableTextWithKey readableObject) {
        var i = new ReadableListItem(readableObject, false, true, string.Empty);
        Add(i);
        return i;
    }

    public TextListItem Add(string readableText, string internalname, bool isCaption, string userDefCompareKey) => Add(readableText, internalname, null, isCaption, true, userDefCompareKey);

    public TextListItem Add(string internalAndReadableText, bool isCaption) => Add(internalAndReadableText, internalAndReadableText, null, isCaption, true, string.Empty);

    public TextListItem Add(string internalAndReadableText, SortierTyp format) => Add(internalAndReadableText, internalAndReadableText, null, false, true, internalAndReadableText.CompareKey(format));

    public TextListItem Add(string internalAndReadableText, ImageCode symbol) => Add(internalAndReadableText, internalAndReadableText, symbol, false, true, string.Empty);

    public TextListItem Add(string readableText, string internalname, bool enabled) => Add(readableText, internalname, null, false, enabled, string.Empty);

    public TextListItem Add(string readableText, string internalname, ImageCode symbol, bool enabled) => Add(readableText, internalname, symbol, false, enabled, string.Empty);

    public TextListItem Add(string readableText, string internalname, ImageCode symbol, bool enabled, string userDefCompareKey) => Add(readableText, internalname, symbol, false, enabled, userDefCompareKey);

    public TextListItem Add(string readableText, string internalname, QuickImage? symbol, bool enabled) => Add(readableText, internalname, symbol, false, enabled, string.Empty);

    public TextListItem Add(string readableText, string internalname, QuickImage? symbol, bool enabled, string userDefCompareKey) => Add(readableText, internalname, symbol, false, enabled, userDefCompareKey);

    public TextListItem Add(string readableText, string internalname) => Add(readableText, internalname, null, false, true, string.Empty);

    public TextListItem Add(string readableText, string internalname, ImageCode symbol) => Add(readableText, internalname, symbol, false, true, string.Empty);

    public TextListItem Add(string readableText, string internalname, QuickImage? symbol) => Add(readableText, internalname, symbol, false, true, string.Empty);

    public TextListItem Add(string readableText, string internalname, ImageCode symbol, bool isCaption, bool enabled, string userDefCompareKey) => Add(readableText, internalname, QuickImage.Get(symbol, 16), isCaption, enabled, userDefCompareKey);

    public TextListItem Add(string readableText, string internalname, QuickImage? symbol, bool isCaption, bool enabled, string userDefCompareKey) {
        TextListItem x = new(readableText, internalname, symbol, isCaption, enabled, userDefCompareKey);
        Add(x);
        return x;
    }

    public BitmapListItem Add(Bitmap? bmp, string caption) {
        BitmapListItem i = new(bmp, string.Empty, caption);
        Add(i);
        return i;
    }

    //public DataListItem Add(byte[] b, string caption) {
    //    DataListItem i = new(b, string.Empty, caption);
    //    Add(i);
    //    return i;
    //}
    public new void Add(AbstractListItem? item) {
        if (item == null) { Develop.DebugPrint(FehlerArt.Fehler, "Item ist null"); return; }
        if (Contains(item)) { Develop.DebugPrint(FehlerArt.Fehler, "Bereits vorhanden!"); return; }
        if (this[item.Internal] != null) { Develop.DebugPrint(FehlerArt.Warnung, "Name bereits vorhanden: " + item.Internal); return; }

        if (string.IsNullOrEmpty(item.Internal)) { Develop.DebugPrint(FehlerArt.Fehler, "Item ohne Namen!"); return; }
        base.Add(item);

        item.Changed += Item_Changed;
        item.CheckedChanged += Item_CheckedChanged;
        item.CompareKeyChanged += Item_CompareKeyChangedChanged;
    }

    public BitmapListItem Add(string filename, string internalname, string caption) {
        BitmapListItem i = new(filename, internalname, caption);
        Add(i);
        return i;
    }

    /// <summary>
    /// Als Interner Name wird der RowKey als String abgelegt
    /// </summary>
    public RowFormulaListItem Add(RowItem row, string layoutId) => Add(row, layoutId, string.Empty);

    /// <summary>
    /// Als Interner Name wird der RowKey als String abgelegt
    /// </summary>
    public RowFormulaListItem Add(RowItem row, string layoutId, string userDefCompareKey) {
        RowFormulaListItem i = new(row, layoutId, userDefCompareKey);
        Add(i);
        return i;
    }

    public TextListItem Add(ContextMenuCommands command, bool enabled = true) {
        var @internal = command.ToString();
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
                readableText = @internal;
                symbol = QuickImage.Get(ImageCode.Fragezeichen);
                break;
        }
        if (string.IsNullOrEmpty(@internal)) { Develop.DebugPrint(FehlerArt.Fehler, "Interner Name nicht vergeben:" + command); }
        return Add(readableText, @internal, symbol, enabled);
    }

    public AbstractListItem? Add(string value, ColumnItem? columnStyle, ShortenStyle style, BildTextVerhalten bildTextverhalten) {
        if (this[value] == null) {
            //if (columnStyle.Format == DataFormat.Link_To_Filesystem && value.FileType() == FileFormat.Image) {
            //    return GenerateAndAdd(columnStyle.BestFile(value, false), value, value, columnStyle.Database.FileEncryptionKey);
            //}

            CellLikeListItem i = new(value, columnStyle, style, true, bildTextverhalten);
            Add(i);
            return i;
        }
        return null;
    }

    public TextListItem Add(ColumnItem column) => Add((IReadableTextWithChangingAndKey)column);

    public void AddClonesFrom(ICollection<AbstractListItem>? itemstoclone) {
        if (itemstoclone == null || itemstoclone.Count == 0) { return; }

        foreach (var thisItem in itemstoclone) {
            if (thisItem.Clone() is AbstractListItem c) {
                Add(c);
            }
        }
    }

    public void AddRange(IEnumerable<AbstractListItem?>? list) {
        if (list == null) { return; }

        foreach (var thisitem in list) {
            if (thisitem != null) { Add(thisitem); }
        }
    }

    /// <summary>
    /// Fügt eine Enumeration hinzu.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public void AddRange(Type type) {
        var underlyingType = Enum.GetUnderlyingType(type);

        if (underlyingType == typeof(int)) {
            foreach (int z1 in Enum.GetValues(type)) {
                if (this[z1.ToString()] == null) {
                    var n = Enum.GetName(type, z1);
                    if (n != null) {
                        _ = Add(n.Replace("_", " "), z1.ToString());
                    }
                }
            }
            return;
        }

        if (underlyingType == typeof(byte)) {
            foreach (byte z1 in Enum.GetValues(type)) {
                if (this[z1.ToString()] == null) {
                    var n = Enum.GetName(type, z1);
                    if (n != null) {
                        _ = Add(n.Replace("_", " "), z1.ToString());
                    }
                }
            }
            return;
        }

        Develop.DebugPrint(FehlerArt.Fehler, "Typ unbekannt");
    }

    public void AddRange(IEnumerable<string>? list) {
        if (list == null) { return; }

        foreach (var thisstring in list) {
            if (!string.IsNullOrEmpty(thisstring) && this[thisstring] == null) {
                _ = Add(thisstring, thisstring);
            }
        }
    }

    public void AddRange(ICollection<string>? values, ColumnItem? columnStyle, ShortenStyle style, BildTextVerhalten bildTextverhalten) {
        if (values == null) { return; }
        if (values.Count > 10000) {
            Develop.DebugPrint(FehlerArt.Fehler, "Values > 100000");
            return;
        }
        foreach (var thisstring in values) {
            _ = Add(thisstring, columnStyle, style, bildTextverhalten); // If Item(thisstring) Is Nothing Then GenerateAndAdd(New CellLikeItem(thisstring, ColumnStyle))
        }
    }

    public void AddRange(IEnumerable<RowItem?>? list, string layoutId) {
        if (list == null) { return; }

        foreach (var thisRow in list) {
            if (thisRow != null && !thisRow.IsDisposed) {
                _ = Add(thisRow, layoutId);
            }
        }
    }

    public void AddRange(IEnumerable<ColumnItem> columns, bool doCaptionSort) {
        foreach (var thisColumnItem in columns) {
            if (thisColumnItem != null) {
                var co = Add(thisColumnItem);

                if (doCaptionSort) {
                    co.UserDefCompareKey = thisColumnItem.Ueberschriften + Constants.SecondSortChar + thisColumnItem.KeyName;

                    var capt = thisColumnItem.Ueberschriften;
                    if (this[capt] == null) {
                        Add(new TextListItem(capt, capt, null, true, true, capt + Constants.FirstSortChar));
                    }
                }
            }
        }
    }

    public LineListItem AddSeparator(string userDefCompareKey) {
        LineListItem i = new(string.Empty, userDefCompareKey);
        Add(i);
        return i;
    }

    public LineListItem AddSeparator() => AddSeparator(string.Empty);

    public Size CalculateColumnAndSize() {
        var (biggestItemX, _, heightAdded, orienation) = ItemData();
        if (orienation == Orientation.Waagerecht) { return ComputeAllItemPositions(new Size(300, 300), null, biggestItemX, heightAdded, orienation); }
        BreakAfterItems = CalculateColumnCount(biggestItemX, heightAdded, orienation);
        return ComputeAllItemPositions(new Size(1, 30), null, biggestItemX, heightAdded, orienation);
    }

    public void Check(IList<string> itemnames, bool checkstate) {
        foreach (var thisItem in itemnames) {
            if (this[thisItem] is AbstractListItem bli) {
                bli.Checked = checkstate;
            }
        }
    }

    public List<AbstractListItem> Checked() => this.Where(thisItem => thisItem != null && thisItem.Checked).ToList();

    public object Clone() {
        ItemCollectionList x = new(_appearance, _autoSort) {
            CheckBehavior = _checkBehavior
        };
        foreach (var thisItem in this) {
            x.Add(thisItem.Clone() as AbstractListItem);   /* ThisItem.CloneToNewCollection(x);*/
        }
        return x;
    }

    public void OnChanged() {
        _maxNeededItemSize = Size.Empty;
        _itemOrder = null;

        Changed?.Invoke(this, System.EventArgs.Empty);
    }

    public void Remove(string internalnam) => Remove(this[internalnam]);

    public new void Remove(AbstractListItem? item) {
        if (item == null || !Contains(item)) { return; }
        item.Changed -= Item_Changed;
        item.CheckedChanged -= Item_CheckedChanged;
        item.CompareKeyChanged -= Item_CompareKeyChangedChanged;
        _ = base.Remove(item);
        OnChanged();
    }

    public void Swap(int index1, int index2) {
        if (index1 == index2) { return; }
        var l = ItemOrder.ToList();
        (l[index1], l[index2]) = (l[index2], l[index1]);
        _maxNeededItemSize = Size.Empty;
        _itemOrder = null;
        OnChanged();
    }

    public void UncheckAll() {
        foreach (var thisItem in this) {
            if (thisItem != null) {
                thisItem.Checked = false;
            }
        }
    }

    internal Size ComputeAllItemPositions(Size controlDrawingArea, Slider? sliderY, int biggestItemX, int heightAdded, Orientation senkrechtAllowed) {
        try {
            if (Math.Abs(_lastCheckedMaxSize.Width - controlDrawingArea.Width) > 0.1 || Math.Abs(_lastCheckedMaxSize.Height - controlDrawingArea.Height) > 0.1) {
                _lastCheckedMaxSize = controlDrawingArea;
                _maxNeededItemSize = Size.Empty;
            }
            if (!_maxNeededItemSize.IsEmpty) { return _maxNeededItemSize; }
            if (Count == 0) {
                _maxNeededItemSize = Size.Empty;
                return Size.Empty;
            }
            PreComputeSize();
            if (_itemDesign == Design.Undefiniert) { GetDesigns(); }
            if (BreakAfterItems < 1) { senkrechtAllowed = Orientation.Waagerecht; }
            var sliderWidth = 0;
            if (sliderY != null) {
                if (BreakAfterItems < 1 && heightAdded > controlDrawingArea.Height) {
                    sliderWidth = sliderY.Width;
                }
            }

            #region colWidth

            int colWidth;
            switch (_appearance) {
                case BlueListBoxAppearance.Gallery:
                    colWidth = 200;
                    break;

                case BlueListBoxAppearance.FileSystem:
                    colWidth = 110;
                    break;

                default:
                    // u.a. Autofilter
                    if (BreakAfterItems < 1) {
                        colWidth = controlDrawingArea.Width - sliderWidth;
                    } else {
                        var colCount = Count / BreakAfterItems;
                        var r = Count % colCount;
                        if (r != 0) { colCount++; }
                        colWidth = controlDrawingArea.Width < 5 ? biggestItemX : (controlDrawingArea.Width - sliderWidth) / colCount;
                    }
                    break;
            }

            #endregion

            var maxX = int.MinValue;
            var maxy = int.MinValue;
            var itenc = -1;
            AbstractListItem? previtem = null;
            foreach (var thisItem in ItemOrder) {
                // PaintmodX kann immer abgezogen werden, da es eh nur bei einspaltigen Listboxen verändert wird!
                if (thisItem != null) {
                    var cx = 0;
                    var cy = 0;
                    var wi = colWidth;
                    int he;
                    itenc++;
                    if (senkrechtAllowed == Orientation.Waagerecht) {
                        if (thisItem.IsCaption) { wi = controlDrawingArea.Width - sliderWidth; }
                        he = thisItem.HeightForListBox(_appearance, wi, ItemDesign);
                    } else {
                        he = thisItem.HeightForListBox(_appearance, wi, ItemDesign);
                    }
                    if (previtem != null) {
                        if (senkrechtAllowed == Orientation.Waagerecht) {
                            if (previtem.Pos.Right + colWidth > controlDrawingArea.Width || thisItem.IsCaption) {
                                cx = 0;
                                cy = previtem.Pos.Bottom;
                            } else {
                                cx = previtem.Pos.Right;
                                cy = previtem.Pos.Top;
                            }
                        } else {
                            if (itenc % BreakAfterItems == 0) {
                                cx = previtem.Pos.Right;
                                cy = 0;
                            } else {
                                cx = previtem.Pos.Left;
                                cy = previtem.Pos.Bottom;
                            }
                        }
                    }
                    thisItem.SetCoordinates(new Rectangle(cx, cy, wi, he));
                    maxX = Math.Max(thisItem.Pos.Right, maxX);
                    maxy = Math.Max(thisItem.Pos.Bottom, maxy);
                    previtem = thisItem;
                }
            }

            #region  sliderY

            if (sliderY != null) {
                bool setTo0;
                if (sliderWidth > 0) {
                    if (maxy - controlDrawingArea.Height <= 0) {
                        sliderY.Enabled = false;
                        setTo0 = true;
                    } else {
                        sliderY.Enabled = true;
                        sliderY.Minimum = 0;
                        sliderY.SmallChange = 16;
                        sliderY.LargeChange = controlDrawingArea.Height;
                        sliderY.Maximum = maxy - controlDrawingArea.Height;
                        setTo0 = false;
                    }
                    sliderY.Height = controlDrawingArea.Height;
                    sliderY.Visible = true;
                } else {
                    setTo0 = true;
                    sliderY.Visible = false;
                }
                if (setTo0) {
                    sliderY.Minimum = 0;
                    sliderY.Maximum = 0;
                    sliderY.Value = 0;
                }
            }

            #endregion

            _maxNeededItemSize = new Size(maxX, maxy);
            return _maxNeededItemSize;
        } catch {
            Develop.CheckStackForOverflow();
            return ComputeAllItemPositions(controlDrawingArea, sliderY, biggestItemX, heightAdded, senkrechtAllowed);
        }
    }

    internal void Item_CheckedChanged(object sender, System.EventArgs e) {
        if (sender is not AbstractListItem item) { return; }

        if (item.Checked) {
            ValidateCheckStates(item);
        } else {
            ValidateCheckStates(null);
        }

        OnItemCheckedChanged();
    }

    internal void Item_CompareKeyChangedChanged(object sender, System.EventArgs e) {
        _maxNeededItemSize = Size.Empty;
        _itemOrder = null;
    }

    /// <summary>
    ///  BiggestItemX, BiggestItemY, HeightAdded, SenkrechtAllowed
    /// </summary>
    /// <returns></returns>
    internal (int BiggestItemX, int BiggestItemY, int HeightAdded, Orientation Orientation) ItemData() {
        try {
            var w = 16;
            var h = 0;
            var hall = 0;
            var sameh = -1;
            var or = Orientation.Senkrecht;
            PreComputeSize();

            foreach (var thisItem in ItemOrder) {
                if (thisItem != null) {
                    var s = thisItem.SizeUntouchedForListBox(ItemDesign);
                    w = Math.Max(w, s.Width);
                    h = Math.Max(h, s.Height);
                    hall += s.Height;
                    if (sameh < 0) {
                        sameh = thisItem.SizeUntouchedForListBox(ItemDesign).Height;
                    } else {
                        if (sameh != thisItem.SizeUntouchedForListBox(ItemDesign).Height) { or = Orientation.Waagerecht; }
                        sameh = thisItem.SizeUntouchedForListBox(ItemDesign).Height;
                    }
                    if (thisItem is not TextListItem) { or = Orientation.Waagerecht; }
                }
            }

            return (w, h, hall, or);
        } catch {
            Develop.CheckStackForOverflow();
            return ItemData();
        }
    }

    internal void SetValuesTo(List<string> values) {
        var ist = this.ToListOfString();
        var zuviel = ist.Except(values).ToList();
        var zuwenig = values.Except(ist).ToList();
        // Zu viele im Mains aus der Liste löschen
        foreach (var thisString in zuviel) {
            if (!values.Contains(thisString)) {
                Remove(thisString);
            }
        }

        // und die Mains auffüllen
        foreach (var thisString in zuwenig) {
            if (IO.FileExists(thisString)) {
                if (thisString.FileType() == FileFormat.Image) {
                    _ = Add(thisString, thisString, thisString.FileNameWithoutSuffix());
                } else {
                    _ = Add(thisString.FileNameWithSuffix(), thisString, QuickImage.Get(thisString.FileType(), 48));
                }
            } else {
                _ = Add(thisString);
            }
        }
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
        _maxNeededItemSize = Size.Empty;
        _itemOrder = null;
        base.OnCollectionChanged(e);
    }

    private int CalculateColumnCount(int biggestItemWidth, int allItemsHeight, Orientation orientation) {
        if (orientation != Orientation.Senkrecht) {
            Develop.DebugPrint(FehlerArt.Fehler, "Nur 'senkrecht' erlaubt mehrere Spalten");
        }
        if (Count < 12) { return -1; }  // <10 ergibt dividieb by zere, weil es da 0 einträge währen bei 10 Spalten
        var dithemh = allItemsHeight / Count;
        for (var testSp = 10; testSp >= 1; testSp--) {
            var colc = Count / testSp;
            var rest = Count % colc;
            var ok = !(rest > 0 && rest < colc / 2);
            if (colc < 5) { ok = false; }
            if (colc > 20) { ok = false; }
            if (colc * dithemh > 600) { ok = false; }
            if (colc * dithemh < 150) { ok = false; }
            if (testSp * biggestItemWidth > 600) { ok = false; }
            if (colc * (float)dithemh / (testSp * (float)biggestItemWidth) < 0.5) { ok = false; }
            if (ok) {
                return colc;
            }
        }
        return -1;
    }

    private ReadOnlyCollection<AbstractListItem> CalculateItemOrder() {
        var l = new List<AbstractListItem>();
        l.AddRange(this);

        if (_autoSort) { l.Sort(); }

        return l.AsReadOnly();
    }

    private void GetDesigns() {
        _controlDesign = (Design)_appearance;
        switch (_appearance) {
            case BlueListBoxAppearance.Autofilter:
                _itemDesign = Design.Item_Autofilter;
                break;

            case BlueListBoxAppearance.DropdownSelectbox:
                _itemDesign = Design.Item_DropdownMenu;
                break;

            case BlueListBoxAppearance.Gallery:
                _itemDesign = Design.Item_Listbox;
                _controlDesign = Design.ListBox;
                break;

            case BlueListBoxAppearance.FileSystem:
                _itemDesign = Design.Item_Listbox;
                _controlDesign = Design.ListBox;
                break;

            case BlueListBoxAppearance.Listbox:
                _itemDesign = Design.Item_Listbox;
                _controlDesign = Design.ListBox;
                break;

            case BlueListBoxAppearance.KontextMenu:
                _itemDesign = Design.Item_KontextMenu;
                break;

            case BlueListBoxAppearance.ComboBox_Textbox:
                _itemDesign = Design.ComboBox_Textbox;
                break;

            default:
                Develop.DebugPrint(FehlerArt.Fehler, "Unbekanntes Design: " + _appearance);
                break;
        }
    }

    private void Item_Changed(object sender, System.EventArgs e) => OnChanged();

    private void OnItemCheckedChanged() => ItemCheckedChanged?.Invoke(this, System.EventArgs.Empty);

    private void PreComputeSize() {
        try {
            _ = Parallel.ForEach(this, thisItem => {
                _ = thisItem?.SizeUntouchedForListBox(ItemDesign);
            });
        } catch {
            Develop.CheckStackForOverflow();
            PreComputeSize();
        }
    }

    private void ValidateCheckStates(AbstractListItem? thisMustBeChecked) {
        switch (_checkBehavior) {
            case CheckBehavior.NoSelection:
                UncheckAll();
                break;

            case CheckBehavior.MultiSelection:
                break;

            case CheckBehavior.SingleSelection:
                if (Checked().Count > 1) {
                    foreach (var thisItem in this) {
                        if (thisItem != null && thisItem != thisMustBeChecked) { thisItem.Checked = false; }
                    }
                }
                break;

            case CheckBehavior.AlwaysSingleSelection:
                if (Checked().Count != 1) {
                    thisMustBeChecked ??= this.FirstOrDefault(thisp => thisp != null && !thisp.IsClickable());

                    foreach (var thisItem in this) {
                        if (thisItem != null && thisItem != thisMustBeChecked) { thisItem.Checked = false; }
                    }

                    if (thisMustBeChecked != null) { thisMustBeChecked.Checked = true; }
                }
                break;

            default:
                Develop.DebugPrint(_checkBehavior);
                break;
        }
    }

    #endregion
}