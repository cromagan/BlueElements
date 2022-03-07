// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BlueControls.ItemCollection {

    public class ItemCollectionList : ListExt<BasicListItem>, ICloneable {

        #region Fields

        private enBlueListBoxAppearance _appearance;
        private Size _cellposCorrect;
        private enCheckBehavior _checkBehavior;
        private enDesign _controlDesign;
        private enDesign _itemDesign;
        private SizeF _lastCheckedMaxSize = Size.Empty;
        private bool _validating;

        #endregion

        #region Constructors

        public ItemCollectionList() : this(enBlueListBoxAppearance.Listbox) { }

        public ItemCollectionList(enBlueListBoxAppearance design) : base() {
            _cellposCorrect = Size.Empty;
            _appearance = enBlueListBoxAppearance.Listbox;
            _itemDesign = enDesign.Undefiniert;
            _controlDesign = enDesign.Undefiniert;
            _checkBehavior = enCheckBehavior.SingleSelection;
            _appearance = design;
            GetDesigns();
        }

        #endregion

        #region Events

        public event EventHandler DoInvalidate;

        public event EventHandler ItemCheckedChanged;

        #endregion

        #region Properties

        public enBlueListBoxAppearance Appearance {
            get => _appearance;
            set {
                if (value == _appearance && _itemDesign != enDesign.Undefiniert) { return; }
                _appearance = value;
                GetDesigns();
                //DesignOrStyleChanged();
                OnDoInvalidate();
            }
        }

        public int BreakAfterItems { get; private set; }

        public enCheckBehavior CheckBehavior {
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
        public enDesign ControlDesign //Implements IDesignAble.Design
        {
            get {
                if (_controlDesign == enDesign.Undefiniert) { Develop.DebugPrint(enFehlerArt.Fehler, "ControlDesign undefiniert!"); }
                return _controlDesign;
            }
        }

        /// <summary>
        /// Itemdesign wird durch Appearance gesetzt
        /// </summary>
        /// <returns></returns>
        public enDesign ItemDesign //Implements IDesignAble.Design
        {
            get {
                if (_itemDesign == enDesign.Undefiniert) { Develop.DebugPrint(enFehlerArt.Fehler, "ItemDesign undefiniert!"); }
                return _itemDesign;
            }
        }

        #endregion

        #region Indexers

        public BasicListItem? this[int x, int y] => this.FirstOrDefault(thisItem => thisItem != null && thisItem.Contains(x, y));

        public BasicListItem? this[string @internal] {
            get {
                try {
                    if (string.IsNullOrEmpty(@internal)) { return null; }

                    return this.FirstOrDefault(thisItem => thisItem != null && @internal.ToUpper() == thisItem.Internal.ToUpper());
                } catch {
                    return this[@internal];
                }
            }
        }

        #endregion

        #region Methods

        public static void GetItemCollection(ItemCollectionList? e, ColumnItem? column, RowItem? checkedItemsAtRow, enShortenStyle style, int maxItems) {
            List<string> marked = new();
            List<string> l = new();
            e.Clear();
            e.CheckBehavior = enCheckBehavior.MultiSelection; // Es kann ja mehr als nur eines angewählt sein, auch wenn nicht erlaubt!
            l.AddRange(column.DropDownItems);
            if (column.DropdownWerteAndererZellenAnzeigen) {
                if (column.DropdownKey >= 0 && checkedItemsAtRow != null) {
                    var cc = column.Database.Column.SearchByKey(column.DropdownKey);
                    FilterCollection f = new(column.Database)
                    {
                        new FilterItem(cc, enFilterType.Istgleich_GroßKleinEgal, checkedItemsAtRow.CellGetString(cc))
                    };
                    l.AddRange(column.Contents(f, null));
                } else {
                    l.AddRange(column.Contents());
                }
            }
            switch (column.Format) {
                case enDataFormat.Columns_für_LinkedCellDropdown:
                    var db = column.LinkedDatabase();
                    if (db != null && !string.IsNullOrEmpty(column.LinkedKeyKennung)) {
                        foreach (var thisColumn in db.Column) {
                            if (thisColumn.Name.ToLower().StartsWith(column.LinkedKeyKennung.ToLower())) {
                                l.Add(thisColumn.Key.ToString());
                            }
                        }
                    }
                    //l = l.SortedDistinctList(); // Sind nur die Keys....
                    if (l.Count == 0) {
                        Notification.Show("Keine Spalten gefunden, die<br>mit '" + column.LinkedKeyKennung + "' beginnen.", enImageCode.Information);
                    }
                    break;

                case enDataFormat.Values_für_LinkedCellDropdown:
                    var db2 = column.LinkedDatabase();
                    if (db2 == null) { Notification.Show("Verknüpfte Datenbank nicht vorhanden", enImageCode.Information); return; }

                    /// Spalte aus der Ziel-Datenbank ermitteln
                    var targetColumn = db2.Column.SearchByKey(column.LinkedCell_ColumnKeyOfLinkedDatabase);
                    if (targetColumn == null) { Notification.Show("Die Spalte ist in der Zieldatenbank nicht vorhanden."); return; }

                    var (filter, info) = CellCollection.GetFilterFromLinkedCellData(db2, column, checkedItemsAtRow);
                    if (!string.IsNullOrEmpty(info)) {
                        Notification.Show("Keine Zeilen in der Quell-Datenbank vorhanden.", enImageCode.Information);
                    }

                    //                    var r = LinkedDatabase.Row.CalculateFilteredRows(filter);

                    //q

                    l.AddRange(targetColumn.Contents(filter, null));
                    if (l.Count == 0) {
                        Notification.Show("Keine Zeilen in der Quell-Datenbank vorhanden.", enImageCode.Information);
                    }
                    break;
            }

            if (column.Database.Row.Count > 0) {
                if (checkedItemsAtRow != null) {
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
            if (maxItems > 0 && l.Count > maxItems) {
                return;
            }
            e.AddRange(l, column, style, column.BildTextVerhalten);
            if (checkedItemsAtRow != null) {
                foreach (var t in marked) {
                    if (e[t] is BasicListItem bli) { bli.Checked = true; }
                }
            }
            e.Sort();
        }

        public TextListItem Add(string internalAndReadableText) => Add(internalAndReadableText, internalAndReadableText, null, false, true, string.Empty);

        /// <summary>
        /// Fügt das übergebende Object den Tags hinzu.
        /// </summary>
        /// <param name="readableObject"></param>
        public TextListItem Add(IReadableText readableObject) {
            var i = Add(readableObject, string.Empty, string.Empty);
            i.Tag = readableObject;
            return i;
        }

        /// <summary>
        /// Fügt das übergebende Object den Tags hinzu.
        /// </summary>
        /// <param name="readableObject"></param>
        /// <param name="internalname"></param>
        public TextListItem Add(IReadableText readableObject, string internalname) {
            var i = Add(readableObject, internalname, string.Empty);
            i.Tag = readableObject;
            return i;
        }

        /// <summary>
        /// Fügt das übergebende Object den Tags hinzu.
        /// </summary>
        /// <param name="internalname"></param>
        /// <param name="readableObject"></param>
        public TextListItem Add(IReadableText readableObject, string internalname, string userDefCompareKey) {
            var i = Add(readableObject.ReadableText(), internalname, readableObject.SymbolForReadableText(), true, userDefCompareKey);
            i.Tag = readableObject;
            return i;
        }

        public TextListItem Add(string readableText, string internalname, bool isCaption, string userDefCompareKey) => Add(readableText, internalname, null, isCaption, true, userDefCompareKey);

        public TextListItem Add(string internalAndReadableText, bool isCaption) => Add(internalAndReadableText, internalAndReadableText, null, isCaption, true, string.Empty);

        public TextListItem Add(string internalAndReadableText, enSortierTyp format) => Add(internalAndReadableText, internalAndReadableText, null, false, true, internalAndReadableText.CompareKey(format));

        public TextListItem Add(string internalAndReadableText, enImageCode symbol) => Add(internalAndReadableText, internalAndReadableText, symbol, false, true, string.Empty);

        public TextListItem Add(string readableText, string internalname, bool enabled) => Add(readableText, internalname, null, false, enabled, string.Empty);

        public TextListItem Add(string readableText, string internalname, enImageCode symbol, bool enabled) => Add(readableText, internalname, symbol, false, enabled, string.Empty);

        public TextListItem Add(string readableText, string internalname, enImageCode symbol, bool enabled, string userDefCompareKey) => Add(readableText, internalname, symbol, false, enabled, userDefCompareKey);

        public TextListItem Add(string readableText, string internalname, QuickImage? symbol, bool enabled) => Add(readableText, internalname, symbol, false, enabled, string.Empty);

        public TextListItem Add(string readableText, string internalname, QuickImage? symbol, bool enabled, string userDefCompareKey) => Add(readableText, internalname, symbol, false, enabled, userDefCompareKey);

        public TextListItem Add(string readableText, string internalname) => Add(readableText, internalname, null, false, true, string.Empty);

        public TextListItem Add(string readableText, string internalname, enImageCode symbol) => Add(readableText, internalname, symbol, false, true, string.Empty);

        public TextListItem Add(string readableText, string internalname, QuickImage? symbol) => Add(readableText, internalname, symbol, false, true, string.Empty);

        public TextListItem Add(string readableText, string internalname, enImageCode symbol, bool isCaption, bool enabled, string userDefCompareKey) => Add(readableText, internalname, QuickImage.Get(symbol, 16), isCaption, enabled, userDefCompareKey);

        public TextListItem Add(string readableText, string internalname, QuickImage? symbol, bool isCaption, bool enabled, string userDefCompareKey) {
            TextListItem x = new(readableText, internalname, symbol, isCaption, enabled, userDefCompareKey);
            Add(x);
            return x;
        }

        public DataListItem Add(byte[] b, string caption) {
            DataListItem i = new(b, string.Empty, caption);
            Add(i);
            return i;
        }

        public BitmapListItem Add(Bitmap? bmp, string caption) {
            BitmapListItem i = new(bmp, string.Empty, caption);
            Add(i);
            return i;
        }

        public new void Add(BasicListItem? item) {
            if (Contains(item)) { Develop.DebugPrint(enFehlerArt.Fehler, "Bereits vorhanden!"); return; }
            if (this[item.Internal] != null) { Develop.DebugPrint(enFehlerArt.Warnung, "Name bereits vorhanden: " + item.Internal); return; }
            base.Add(item);
        }

        public BitmapListItem Add(string filename, string internalname, string caption, string encryptionKey) {
            BitmapListItem i = new(filename, internalname, caption, encryptionKey);
            Add(i);
            return i;
        }

        public CellLikeListItem Add(string internalAndReadableText, ColumnItem? columnStyle, enShortenStyle style, enBildTextVerhalten bildTextverhaltent, bool enabled) {
            CellLikeListItem i = new(internalAndReadableText, columnStyle, style, enabled, bildTextverhaltent);
            Add(i);
            return i;
        }

        public RowFormulaListItem Add(RowItem? row, string layoutId) => Add(row, layoutId, string.Empty);

        public RowFormulaListItem Add(RowItem? row, string layoutId, string userDefCompareKey) {
            RowFormulaListItem i = new(row, layoutId, userDefCompareKey);
            Add(i);
            return i;
        }

        public TextListItem Add(enContextMenuComands comand, bool enabled = true) {
            var @internal = comand.ToString();
            QuickImage? symbol;
            string? readableText;
            switch (comand) {
                case enContextMenuComands.DateiPfadÖffnen:
                    readableText = "Dateipfad öffnen";
                    symbol = QuickImage.Get("Ordner|16");
                    break;

                case enContextMenuComands.Abbruch:
                    readableText = "Abbrechen";
                    symbol = QuickImage.Get("TasteESC|16");
                    break;

                case enContextMenuComands.Bearbeiten:
                    readableText = "Bearbeiten";
                    symbol = QuickImage.Get(enImageCode.Stift);
                    break;

                case enContextMenuComands.Kopieren:
                    readableText = "Kopieren";
                    symbol = QuickImage.Get(enImageCode.Kopieren);
                    break;

                case enContextMenuComands.InhaltLöschen:
                    readableText = "Inhalt löschen";
                    symbol = QuickImage.Get(enImageCode.Radiergummi);
                    break;

                case enContextMenuComands.ZeileLöschen:
                    readableText = "Zeile löschen";
                    symbol = QuickImage.Get("Zeile|16|||||||||Kreuz");
                    break;

                case enContextMenuComands.DateiÖffnen:
                    readableText = "Öffnen / Ausführen";
                    symbol = QuickImage.Get(enImageCode.Blitz);
                    break;

                case enContextMenuComands.SpaltenSortierungAZ:
                    readableText = "Nach dieser Spalte aufsteigend sortieren";
                    symbol = QuickImage.Get("AZ|16|8");
                    break;

                case enContextMenuComands.SpaltenSortierungZA:
                    readableText = "Nach dieser Spalte absteigend sortieren";
                    symbol = QuickImage.Get("ZA|16|8");
                    break;

                case enContextMenuComands.Information:
                    readableText = "Informationen anzeigen";
                    symbol = QuickImage.Get(enImageCode.Frage);
                    break;

                case enContextMenuComands.ZellenInhaltKopieren:
                    readableText = "Zelleninhalt kopieren";
                    symbol = QuickImage.Get(enImageCode.Kopieren);
                    break;

                case enContextMenuComands.ZellenInhaltPaste:
                    readableText = "In Zelle einfügen";
                    symbol = QuickImage.Get(enImageCode.Clipboard);
                    break;

                case enContextMenuComands.SpaltenEigenschaftenBearbeiten:
                    readableText = "Spalteneigenschaften bearbeiten";
                    symbol = QuickImage.Get("Spalte|16|||||||||Stift");
                    break;

                case enContextMenuComands.Speichern:
                    readableText = "Speichern";
                    symbol = QuickImage.Get(enImageCode.Diskette);
                    break;

                case enContextMenuComands.Löschen:
                    readableText = "Löschen";
                    symbol = QuickImage.Get(enImageCode.Kreuz);
                    break;

                case enContextMenuComands.Umbenennen:
                    readableText = "Umbenennen";
                    symbol = QuickImage.Get(enImageCode.Stift);
                    break;

                case enContextMenuComands.SuchenUndErsetzen:
                    readableText = "Suchen und ersetzen";
                    symbol = QuickImage.Get(enImageCode.Fernglas);
                    break;

                case enContextMenuComands.Einfügen:
                    readableText = "Einfügen";
                    symbol = QuickImage.Get(enImageCode.Clipboard);
                    break;

                case enContextMenuComands.Ausschneiden:
                    readableText = "Ausschneiden";
                    symbol = QuickImage.Get(enImageCode.Schere);
                    break;

                case enContextMenuComands.VorherigenInhaltWiederherstellen:
                    readableText = "Vorherigen Inhalt wieder herstellen";
                    symbol = QuickImage.Get(enImageCode.Undo);
                    break;

                case enContextMenuComands.WeitereBefehle:
                    readableText = "Weitere Befehle";
                    symbol = QuickImage.Get(enImageCode.Hierarchie);
                    break;

                default:
                    Develop.DebugPrint(comand);
                    readableText = @internal;
                    symbol = QuickImage.Get(enImageCode.Fragezeichen);
                    break;
            }
            if (string.IsNullOrEmpty(@internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben:" + comand); }
            return Add(readableText, @internal, symbol, enabled);
        }

        public BasicListItem? Add(string value, ColumnItem? columnStyle, enShortenStyle style, enBildTextVerhalten bildTextverhalten) {
            if (this[value] == null) {
                if (columnStyle.Format == enDataFormat.Link_To_Filesystem && value.FileType() == enFileFormat.Image) {
                    return Add(columnStyle.BestFile(value, false), value, value, columnStyle.Database.FileEncryptionKey);
                }

                CellLikeListItem i = new(value, columnStyle, style, true, bildTextverhalten);
                Add(i);
                return i;
            }
            return null;
        }

        /// <summary>
        /// Fügt die Spalte hinzu. Als interner Name wird der Column.Key verwendet.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public TextListItem Add(ColumnItem column) => Add(column, column.Key.ToString());

        public void AddRange(Type type) {
            foreach (int z1 in Enum.GetValues(type)) {
                if (this[z1.ToString()] == null) { Add(Enum.GetName(type, z1).Replace("_", " "), z1.ToString()); }
            }
            Sort();
        }

        public void AddRange(string[] values) {
            if (values == null) { return; }
            foreach (var thisstring in values) {
                if (this[thisstring] == null) { Add(thisstring); }
            }
        }

        public void AddRange(ListExt<string> values) {
            if (values == null) { return; }

            foreach (var thisstring in values.Where(thisstring => this[thisstring] == null)) {
                Add(thisstring, thisstring);
            }
        }

        public void AddRange(List<string> values, ColumnItem? columnStyle, enShortenStyle style, enBildTextVerhalten bildTextverhalten) {
            if (values == null) { return; }
            if (values.Count > 10000) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Values > 100000");
                return;
            }
            foreach (var thisstring in values) {
                Add(thisstring, columnStyle, style, bildTextverhalten); // If Item(thisstring) Is Nothing Then Add(New CellLikeItem(thisstring, ColumnStyle))
            }
        }

        public void AddRange(List<RowItem?> list, string layoutId) {
            if (list == null || list.Count == 0) { return; }

            foreach (var thisRow in list) {
                Add(thisRow, layoutId);
            }
        }

        public void AddRange(List<string> list) {
            if (list == null) { return; }

            foreach (var thisstring in list.Where(thisstring => !string.IsNullOrEmpty(thisstring) && this[thisstring] == null)) {
                Add(thisstring, thisstring);
            }
        }

        /// <summary>
        /// Fügt die Spalte hinzu. Als interner Name wird der Column.Key verwendet.
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="doCaptionSort">Bei True werden auch die Überschriften der Spalte als Text hinzugefügt und auch danach sortiert</param>
        public void AddRange(ColumnCollection columns, bool doCaptionSort) {
            foreach (var thisColumnItem in columns) {
                if (thisColumnItem != null) {
                    var co = Add(thisColumnItem);

                    if (doCaptionSort) {
                        co.UserDefCompareKey = thisColumnItem.Ueberschriften + Constants.SecondSortChar + thisColumnItem.Name;

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
            var (biggestItemX, _, heightAdded, senkrechtAllowed) = ItemData();
            if (senkrechtAllowed == enOrientation.Waagerecht) { return ComputeAllItemPositions(new Size(300, 300), null, biggestItemX, heightAdded, senkrechtAllowed); }
            BreakAfterItems = CalculateColumnCount(biggestItemX, heightAdded, senkrechtAllowed);
            return ComputeAllItemPositions(new Size(1, 30), null, biggestItemX, heightAdded, senkrechtAllowed);
        }

        public void Check(ListExt<string> vItems, bool @checked) => Check(vItems.ToArray(), @checked);

        public void Check(List<string> vItems, bool @checked) => Check(vItems.ToArray(), @checked);

        public void Check(string[] vItems, bool @checked) {
            for (var z = 0; z <= vItems.GetUpperBound(0); z++) {
                if (this[vItems[z]] != null) {
                    this[vItems[z]].Checked = @checked;
                }
            }
        }

        public void CheckAll() {
            foreach (var thisItem in this.Where(thisItem => thisItem != null)) {
                thisItem.Checked = true;
            }
        }

        public List<BasicListItem> Checked() => this.Where(thisItem => thisItem != null && thisItem.Checked).ToList();

        public object Clone() {
            ItemCollectionList x = new(_appearance) {
                CheckBehavior = _checkBehavior
            };
            foreach (var thisItem in this) {
                x.Add((BasicListItem)thisItem.Clone());   /* ThisItem.CloneToNewCollection(x);*/
            }
            return x;
        }

        /// <summary>
        /// Füllt die Ersetzungen mittels eines Übergebenen Enums aus.
        /// </summary>
        /// <param name="t">Beispiel: GetType(enDesign)</param>
        /// <param name="zumDropdownHinzuAb">Erster Wert der Enumeration, der Hinzugefügt werden soll. Inklusive deses Wertes</param>
        /// <param name="zumDropdownHinzuBis">Letzter Wert der Enumeration, der nicht mehr hinzugefügt wird, also exklusives diese Wertes</param>
        public void GetValuesFromEnum(Type t, int zumDropdownHinzuAb, int zumDropdownHinzuBis) {
            var items = Enum.GetValues(t);
            Clear();
            foreach (var thisItem in items) {
                var te = Enum.GetName(t, thisItem);
                var th = (int)thisItem;
                if (!string.IsNullOrEmpty(te)) {
                    //NewReplacer.Add(th.ToString() + "|" + te);
                    if (th >= zumDropdownHinzuAb && th < zumDropdownHinzuBis) {
                        Add(te, th.ToString());
                    }
                }
            }
        }

        public override void OnChanged() {
            _cellposCorrect = Size.Empty;
            base.OnChanged();
            OnDoInvalidate();
        }

        public void OnDoInvalidate() => DoInvalidate?.Invoke(this, System.EventArgs.Empty);

        //public ListExt<clsNamedBinary> GetNamedBinaries() {
        //    var l = new ListExt<clsNamedBinary>();
        //    foreach (var thisItem in this) {
        //        switch (thisItem) {
        //            case BitmapListItem BI:
        //                l.Add(new clsNamedBinary(BI.Caption, BI.Bitmap));
        //                break;
        //            case TextListItem TI:
        //                l.Add(new clsNamedBinary(TI.Text, TI.Internal));
        //                break;
        //        }
        //    }
        //    return l;
        //}
        public void Remove(string @internal) => Remove(this[@internal]);

        public void RemoveRange(List<string> @internal) {
            foreach (var item in @internal) {
                Remove(item);
            }
        }

        public void UncheckAll() {
            foreach (var thisItem in this.Where(thisItem => thisItem != null)) {
                thisItem.Checked = false;
            }
        }

        internal Size ComputeAllItemPositions(Size controlDrawingArea, Slider? sliderY, int biggestItemX, int heightAdded, enOrientation senkrechtAllowed) {
            try {
                if (Math.Abs(_lastCheckedMaxSize.Width - controlDrawingArea.Width) > 0.1 || Math.Abs(_lastCheckedMaxSize.Height - controlDrawingArea.Height) > 0.1) {
                    _lastCheckedMaxSize = controlDrawingArea;
                    _cellposCorrect = Size.Empty;
                }
                if (!_cellposCorrect.IsEmpty) { return _cellposCorrect; }
                if (Count == 0) {
                    _cellposCorrect = Size.Empty;
                    return Size.Empty;
                }
                PreComputeSize();
                if (_itemDesign == enDesign.Undefiniert) { GetDesigns(); }
                if (BreakAfterItems < 1) { senkrechtAllowed = enOrientation.Waagerecht; }
                var sliderWidth = 0;
                if (sliderY != null) {
                    if (BreakAfterItems < 1 && heightAdded > controlDrawingArea.Height) {
                        sliderWidth = sliderY.Width;
                    }
                }
                int colWidth;
                switch (_appearance) {
                    case enBlueListBoxAppearance.Gallery:
                        colWidth = 200;
                        break;

                    case enBlueListBoxAppearance.FileSystem:
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
                var maxX = int.MinValue;
                var maxy = int.MinValue;
                var itenc = -1;
                BasicListItem previtem = null;
                foreach (var thisItem in this) {
                    // PaintmodX kann immer abgezogen werden, da es eh nur bei einspaltigen Listboxen verändert wird!
                    if (thisItem != null) {
                        var cx = 0;
                        var cy = 0;
                        var wi = colWidth;
                        int he;
                        itenc++;
                        if (senkrechtAllowed == enOrientation.Waagerecht) {
                            if (thisItem.IsCaption) { wi = controlDrawingArea.Width - sliderWidth; }
                            he = thisItem.HeightForListBox(_appearance, wi);
                        } else {
                            he = thisItem.HeightForListBox(_appearance, wi);
                        }
                        if (previtem != null) {
                            if (senkrechtAllowed == enOrientation.Waagerecht) {
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
                _cellposCorrect = new Size(maxX, maxy);
                return _cellposCorrect;
            } catch {
                return ComputeAllItemPositions(controlDrawingArea, sliderY, biggestItemX, heightAdded, senkrechtAllowed);
            }
        }

        /// <summary>
        ///  BiggestItemX, BiggestItemY, HeightAdded, SenkrechtAllowed
        /// </summary>
        /// <returns></returns>
        internal (int BiggestItemX, int BiggestItemY, int HeightAdded, enOrientation SenkrechtAllowed) ItemData() {
            try {
                var w = 16;
                var h = 0;
                var hall = 0;
                var sameh = -1;
                var or = enOrientation.Senkrecht;
                PreComputeSize();
                foreach (var thisItem in this) {
                    if (thisItem != null) {
                        var s = thisItem.SizeUntouchedForListBox();
                        w = Math.Max(w, s.Width);
                        h = Math.Max(h, s.Height);
                        hall += s.Height;
                        if (sameh < 0) {
                            sameh = thisItem.SizeUntouchedForListBox().Height;
                        } else {
                            if (sameh != thisItem.SizeUntouchedForListBox().Height) { or = enOrientation.Waagerecht; }
                            sameh = thisItem.SizeUntouchedForListBox().Height;
                        }
                        if (thisItem is not TextListItem and not CellLikeListItem) { or = enOrientation.Waagerecht; }
                    }
                }
                return (w, h, hall, or);
            } catch {
                return ItemData();
            }
        }

        internal void SetNewCheckState(BasicListItem @this, bool value, ref bool checkVariable) {
            if (!@this.IsClickable()) { value = false; }
            if (_checkBehavior == enCheckBehavior.NoSelection) {
                value = false;
            } else if (checkVariable && value == false && _checkBehavior == enCheckBehavior.AlwaysSingleSelection) {
                if (Checked().Count == 1) { value = true; }
            }
            if (value == checkVariable) { return; }
            checkVariable = value;
            if (_validating) { return; }
            ValidateCheckStates(@this);
            OnItemCheckedChanged();
            OnDoInvalidate();
        }

        internal void SetValuesTo(List<string> values, string fileEncryptionKey) {
            var ist = this.ToListOfString();
            var zuviel = ist.Except(values).ToList();
            var zuwenig = values.Except(ist).ToList();
            // Zu viele im Mains aus der Liste löschen
            foreach (var thisString in zuviel.Where(thisString => !values.Contains(thisString))) {
                Remove(thisString);
            }
            // und die Mains auffüllen
            foreach (var thisString in zuwenig) {
                if (FileOperations.FileExists(thisString)) {
                    if (thisString.FileType() == enFileFormat.Image) {
                        Add(thisString, thisString, thisString.FileNameWithoutSuffix(), fileEncryptionKey);
                    } else {
                        Add(thisString.FileNameWithSuffix(), thisString, QuickImage.Get(thisString.FileType(), 48));
                    }
                } else {
                    Add(thisString);
                }
            }
        }

        protected override void OnItemAdded(BasicListItem item) {
            if (string.IsNullOrEmpty(item.Internal)) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Der Auflistung soll ein Item hinzugefügt werden, welches keinen Namen hat " + item.Internal);
            }
            item.Parent = this;
            base.OnItemAdded(item);
            OnDoInvalidate();
        }

        private int CalculateColumnCount(int biggestItemWidth, int allItemsHeight, enOrientation orientation) {
            if (orientation != enOrientation.Senkrecht) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Nur 'senkrecht' erlaubt mehrere Spalten");
            }
            if (Count < 12) { return -1; }  // <10 ergibt dividieb by zere, weil es da 0 einträge währen bei 10 Spalten
            var dithemh = allItemsHeight / Count;
            for (var testSp = 10; testSp >= 1; testSp--) {
                var colc = Count / testSp;
                var rest = Count % colc;
                var ok = true;
                if (rest > 0 && rest < colc / 2) { ok = false; }
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

        private void GetDesigns() {
            _controlDesign = (enDesign)_appearance;
            switch (_appearance) {
                case enBlueListBoxAppearance.Autofilter:
                    _itemDesign = enDesign.Item_Autofilter;
                    break;

                case enBlueListBoxAppearance.DropdownSelectbox:
                    _itemDesign = enDesign.Item_DropdownMenu;
                    break;

                case enBlueListBoxAppearance.Gallery:
                    _itemDesign = enDesign.Item_Listbox;
                    _controlDesign = enDesign.ListBox;
                    break;

                case enBlueListBoxAppearance.FileSystem:
                    _itemDesign = enDesign.Item_Listbox;
                    _controlDesign = enDesign.ListBox;
                    break;

                case enBlueListBoxAppearance.Listbox:
                    _itemDesign = enDesign.Item_Listbox;
                    _controlDesign = enDesign.ListBox;
                    break;

                case enBlueListBoxAppearance.KontextMenu:
                    _itemDesign = enDesign.Item_KontextMenu;
                    break;

                case enBlueListBoxAppearance.ComboBox_Textbox:
                    _itemDesign = enDesign.ComboBox_Textbox;
                    break;

                default:
                    Develop.DebugPrint(enFehlerArt.Fehler, "Unbekanntes Design: " + _appearance);
                    break;
            }
        }

        private void OnItemCheckedChanged() => ItemCheckedChanged?.Invoke(this, System.EventArgs.Empty);

        private void PreComputeSize() {
            try {
                System.Threading.Tasks.Parallel.ForEach(this, thisItem => {
                    thisItem.SizeUntouchedForListBox();
                });
            } catch {
                PreComputeSize();
            }
        }

        private void ValidateCheckStates(BasicListItem? thisMustBeChecked) {
            _validating = true;
            var somethingDonex = false;
            var done = false;
            BasicListItem? f = null;
            switch (_checkBehavior) {
                case enCheckBehavior.NoSelection:
                    UncheckAll();
                    break;

                case enCheckBehavior.MultiSelection:
                    break;

                case enCheckBehavior.SingleSelection:
                case enCheckBehavior.AlwaysSingleSelection:
                    foreach (var thisItem in this) {
                        if (thisItem != null) {
                            if (thisMustBeChecked == null) {
                                if (f == null) { f = thisItem; }
                                if (thisItem.Checked) {
                                    if (!done) {
                                        done = true;
                                    } else {
                                        if (thisItem.Checked) {
                                            thisItem.Checked = false;
                                            somethingDonex = true;
                                        }
                                    }
                                }
                            } else {
                                done = true;
                                if (thisItem != thisMustBeChecked && thisItem.Checked) {
                                    somethingDonex = true;
                                    thisItem.Checked = false;
                                }
                            }
                        }
                    }
                    if (_checkBehavior == enCheckBehavior.AlwaysSingleSelection && !done && f != null && !f.Checked) {
                        f.Checked = true;
                        somethingDonex = true;
                    }
                    break;

                default:
                    Develop.DebugPrint(_checkBehavior);
                    break;
            }
            _validating = false;
            if (somethingDonex) { OnDoInvalidate(); }
        }

        #endregion
    }
}