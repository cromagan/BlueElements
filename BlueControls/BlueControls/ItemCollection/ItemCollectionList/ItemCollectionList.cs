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
using static BlueBasics.Extensions;

namespace BlueControls.ItemCollection {

    public class ItemCollectionList : ListExt<BasicListItem>, ICloneable {

        #region Fields

        private enBlueListBoxAppearance _Appearance;
        private Size _CellposCorrect = Size.Empty;
        private enCheckBehavior _CheckBehavior;
        private enDesign _ControlDesign;
        private enDesign _ItemDesign;
        private bool _Validating;
        private SizeF LastCheckedMaxSize = Size.Empty;

        #endregion

        #region Constructors

        public ItemCollectionList() : this(enBlueListBoxAppearance.Listbox) { }

        public ItemCollectionList(enBlueListBoxAppearance design) : base() {
            _CellposCorrect = Size.Empty;
            _Appearance = enBlueListBoxAppearance.Listbox;
            _ItemDesign = enDesign.Undefiniert;
            _ControlDesign = enDesign.Undefiniert;
            _CheckBehavior = enCheckBehavior.SingleSelection;
            _Appearance = design;
            GetDesigns();
        }

        #endregion

        #region Events

        public event EventHandler DoInvalidate;

        public event EventHandler ItemCheckedChanged;

        #endregion

        #region Properties

        public enBlueListBoxAppearance Appearance {
            get => _Appearance;
            set {
                if (value == _Appearance && _ItemDesign != enDesign.Undefiniert) { return; }
                _Appearance = value;
                GetDesigns();
                //DesignOrStyleChanged();
                OnDoInvalidate();
            }
        }

        public int BreakAfterItems { get; private set; }

        public enCheckBehavior CheckBehavior {
            get => _CheckBehavior;
            set {
                if (value == _CheckBehavior) { return; }
                _CheckBehavior = value;
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
                if (_ControlDesign == enDesign.Undefiniert) { Develop.DebugPrint(enFehlerArt.Fehler, "ControlDesign undefiniert!"); }
                return _ControlDesign;
            }
        }

        /// <summary>
        /// Itemdesign wird durch Appearance gesetzt
        /// </summary>
        /// <returns></returns>
        public enDesign ItemDesign //Implements IDesignAble.Design
        {
            get {
                if (_ItemDesign == enDesign.Undefiniert) { Develop.DebugPrint(enFehlerArt.Fehler, "ItemDesign undefiniert!"); }
                return _ItemDesign;
            }
        }

        public enOrientation Orientation { get; private set; }

        #endregion

        #region Indexers

        public BasicListItem this[int X, int Y] {
            get {
                foreach (var ThisItem in this) {
                    if (ThisItem != null && ThisItem.Contains(X, Y)) { return ThisItem; }
                }
                return null;
            }
        }

        public BasicListItem this[string Internal] {
            get {
                try {
                    if (string.IsNullOrEmpty(Internal)) { return null; }
                    foreach (var ThisItem in this) {
                        if (ThisItem != null && Internal.ToUpper() == ThisItem.Internal.ToUpper()) { return ThisItem; }
                    }
                    return null;
                } catch {
                    return this[Internal];
                }
            }
        }

        #endregion

        #region Methods

        public static void GetItemCollection(ItemCollectionList e, ColumnItem column, RowItem checkedItemsAtRow, enShortenStyle style, int maxItems) {
            List<string> Marked = new();
            List<string> l = new();
            e.Clear();
            e.CheckBehavior = enCheckBehavior.MultiSelection; // Es kann ja mehr als nur eines angewählt sein, auch wenn nicht erlaubt!
            l.AddRange(column.DropDownItems);
            if (column.DropdownWerteAndererZellenAnzeigen) {
                if (column.DropdownKey >= 0 && checkedItemsAtRow != null) {
                    var cc = column.Database.Column.SearchByKey(column.DropdownKey);
                    FilterCollection F = new(column.Database)
                    {
                        new FilterItem(cc, enFilterType.Istgleich_GroßKleinEgal, checkedItemsAtRow.CellGetString(cc))
                    };
                    l.AddRange(column.Contents(F, null));
                } else {
                    l.AddRange(column.Contents());
                }
            }
            switch (column.Format) {
                //case enDataFormat.Bit:
                //    l.Add(true.ToPlusMinus());
                //    l.Add(false.ToPlusMinus());
                //    break;

                case enDataFormat.Columns_für_LinkedCellDropdown:
                    var DB = column.LinkedDatabase();
                    if (DB != null && !string.IsNullOrEmpty(column.LinkedKeyKennung)) {
                        foreach (var ThisColumn in DB.Column) {
                            if (ThisColumn.Name.ToLower().StartsWith(column.LinkedKeyKennung.ToLower())) {
                                l.Add(ThisColumn.Key.ToString());
                            }
                        }
                    }
                    //l = l.SortedDistinctList(); // Sind nur die Keys....
                    if (l.Count == 0) {
                        Notification.Show("Keine Spalten gefunden, die<br>mit '" + column.LinkedKeyKennung + "' beginnen.", enImageCode.Information);
                    }
                    break;

                case enDataFormat.Values_für_LinkedCellDropdown:
                    var DB2 = column.LinkedDatabase();
                    l.AddRange(DB2.Column[0].Contents());
                    if (l.Count == 0) {
                        Notification.Show("Keine Zeilen in der Quell-Datenbank vorhanden.", enImageCode.Information);
                    }
                    break;
            }
            if (column.Database.Row.Count > 0) {
                if (checkedItemsAtRow != null) {
                    if (!checkedItemsAtRow.CellIsNullOrEmpty(column)) {
                        if (column.MultiLine) {
                            Marked = checkedItemsAtRow.CellGetList(column);
                        } else {
                            Marked.Add(checkedItemsAtRow.CellGetString(column));
                        }
                    }
                    l.AddRange(Marked);
                }
                l = l.SortedDistinctList();
            }
            if (maxItems > 0 && l.Count > maxItems) { return; }
            e.AddRange(l, column, style, column.BildTextVerhalten);
            if (checkedItemsAtRow != null) {
                foreach (var t in Marked) {
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

        public TextListItem Add(string readableText, string internalname, QuickImage symbol, bool enabled) => Add(readableText, internalname, symbol, false, enabled, string.Empty);

        public TextListItem Add(string readableText, string internalname, QuickImage symbol, bool enabled, string userDefCompareKey) => Add(readableText, internalname, symbol, false, enabled, userDefCompareKey);

        public TextListItem Add(string readableText, string internalname) => Add(readableText, internalname, null, false, true, string.Empty);

        public TextListItem Add(string readableText, string internalname, enImageCode symbol) => Add(readableText, internalname, symbol, false, true, string.Empty);

        public TextListItem Add(string readableText, string internalname, QuickImage symbol) => Add(readableText, internalname, symbol, false, true, string.Empty);

        public TextListItem Add(string readableText, string internalname, enImageCode symbol, bool isCaption, bool enabled, string userDefCompareKey) => Add(readableText, internalname, QuickImage.Get(symbol, 16), isCaption, enabled, userDefCompareKey);

        public TextListItem Add(string readableText, string internalname, QuickImage symbol, bool isCaption, bool enabled, string userDefCompareKey) {
            TextListItem x = new(readableText, internalname, symbol, isCaption, enabled, userDefCompareKey);
            Add(x);
            return x;
        }

        public DataListItem Add(byte[] b, string caption) {
            DataListItem i = new(b, string.Empty, caption);
            Add(i);
            return i;
        }

        public BitmapListItem Add(Bitmap bmp, string caption) {
            BitmapListItem i = new(bmp, string.Empty, caption);
            Add(i);
            return i;
        }

        public new void Add(BasicListItem item) {
            if (Contains(item)) { Develop.DebugPrint(enFehlerArt.Fehler, "Bereits vorhanden!"); return; }
            if (this[item.Internal] != null) { Develop.DebugPrint(enFehlerArt.Warnung, "Name bereits vorhanden: " + item.Internal); return; }
            base.Add(item);
        }

        public BitmapListItem Add(string filename, string internalname, string caption, string EncryptionKey) {
            BitmapListItem i = new(filename, internalname, caption, EncryptionKey);
            Add(i);
            return i;
        }

        public CellLikeListItem Add(string internalAndReadableText, ColumnItem columnStyle, enShortenStyle style, enBildTextVerhalten bildTextverhaltent, bool enabled) {
            CellLikeListItem i = new(internalAndReadableText, columnStyle, style, enabled, bildTextverhaltent);
            Add(i);
            return i;
        }

        public RowFormulaListItem Add(RowItem row, string layoutID) => Add(row, layoutID, string.Empty);

        public RowFormulaListItem Add(RowItem row, string layoutID, string userDefCompareKey) {
            RowFormulaListItem i = new(row, layoutID, userDefCompareKey);
            Add(i);
            return i;
        }

        public TextListItem Add(enContextMenuComands comand, bool enabled = true) {
            var _Internal = comand.ToString();
            QuickImage _Symbol;
            string _ReadableText;
            switch (comand) {
                case enContextMenuComands.DateiPfadÖffnen:
                    _ReadableText = "Dateipfad öffnen";
                    _Symbol = QuickImage.Get("Ordner|16");
                    break;

                case enContextMenuComands.Abbruch:
                    _ReadableText = "Abbrechen";
                    _Symbol = QuickImage.Get("TasteESC|16");
                    break;

                case enContextMenuComands.Bearbeiten:
                    _ReadableText = "Bearbeiten";
                    _Symbol = QuickImage.Get(enImageCode.Stift);
                    break;

                case enContextMenuComands.Kopieren:
                    _ReadableText = "Kopieren";
                    _Symbol = QuickImage.Get(enImageCode.Kopieren);
                    break;

                case enContextMenuComands.InhaltLöschen:
                    _ReadableText = "Inhalt löschen";
                    _Symbol = QuickImage.Get(enImageCode.Radiergummi);
                    break;

                case enContextMenuComands.ZeileLöschen:
                    _ReadableText = "Zeile löschen";
                    _Symbol = QuickImage.Get("Zeile|16|||||||||Kreuz");
                    break;

                case enContextMenuComands.DateiÖffnen:
                    _ReadableText = "Öffnen / Ausführen";
                    _Symbol = QuickImage.Get(enImageCode.Blitz);
                    break;

                case enContextMenuComands.SpaltenSortierungAZ:
                    _ReadableText = "Nach dieser Spalte aufsteigend sortieren";
                    _Symbol = QuickImage.Get("AZ|16|8");
                    break;

                case enContextMenuComands.SpaltenSortierungZA:
                    _ReadableText = "Nach dieser Spalte absteigend sortieren";
                    _Symbol = QuickImage.Get("ZA|16|8");
                    break;

                case enContextMenuComands.Information:
                    _ReadableText = "Informationen anzeigen";
                    _Symbol = QuickImage.Get(enImageCode.Frage);
                    break;

                case enContextMenuComands.ZellenInhaltKopieren:
                    _ReadableText = "Zelleninhalt kopieren";
                    _Symbol = QuickImage.Get(enImageCode.Kopieren);
                    break;

                case enContextMenuComands.ZellenInhaltPaste:
                    _ReadableText = "In Zelle einfügen";
                    _Symbol = QuickImage.Get(enImageCode.Clipboard);
                    break;

                case enContextMenuComands.SpaltenEigenschaftenBearbeiten:
                    _ReadableText = "Spalteneigenschaften bearbeiten";
                    _Symbol = QuickImage.Get("Spalte|16|||||||||Stift");
                    break;

                case enContextMenuComands.Speichern:
                    _ReadableText = "Speichern";
                    _Symbol = QuickImage.Get(enImageCode.Diskette);
                    break;

                case enContextMenuComands.Löschen:
                    _ReadableText = "Löschen";
                    _Symbol = QuickImage.Get(enImageCode.Kreuz);
                    break;

                case enContextMenuComands.Umbenennen:
                    _ReadableText = "Umbenennen";
                    _Symbol = QuickImage.Get(enImageCode.Stift);
                    break;

                case enContextMenuComands.SuchenUndErsetzen:
                    _ReadableText = "Suchen und ersetzen";
                    _Symbol = QuickImage.Get(enImageCode.Fernglas);
                    break;

                case enContextMenuComands.Einfügen:
                    _ReadableText = "Einfügen";
                    _Symbol = QuickImage.Get(enImageCode.Clipboard);
                    break;

                case enContextMenuComands.Ausschneiden:
                    _ReadableText = "Ausschneiden";
                    _Symbol = QuickImage.Get(enImageCode.Schere);
                    break;

                case enContextMenuComands.VorherigenInhaltWiederherstellen:
                    _ReadableText = "Vorherigen Inhalt wieder herstellen";
                    _Symbol = QuickImage.Get(enImageCode.Undo);
                    break;

                case enContextMenuComands.WeitereBefehle:
                    _ReadableText = "Weitere Befehle";
                    _Symbol = QuickImage.Get(enImageCode.Hierarchie);
                    break;

                default:
                    Develop.DebugPrint(comand);
                    _ReadableText = _Internal;
                    _Symbol = QuickImage.Get(enImageCode.Fragezeichen);
                    break;
            }
            if (string.IsNullOrEmpty(_Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben:" + comand); }
            return Add(_ReadableText, _Internal, _Symbol, enabled);
        }

        public BasicListItem Add(string Value, ColumnItem ColumnStyle, enShortenStyle Style, enBildTextVerhalten bildTextverhalten) {
            if (this[Value] == null) {
                if (ColumnStyle.Format == enDataFormat.Link_To_Filesystem && Value.FileType() == enFileFormat.Image) {
                    return Add(ColumnStyle.BestFile(Value, false), Value, Value, ColumnStyle.Database.FileEncryptionKey);
                } else {
                    CellLikeListItem i = new(Value, ColumnStyle, Style, true, bildTextverhalten);
                    Add(i);
                    return i;
                }
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

        public void AddRange(string[] Values) {
            if (Values == null) { return; }
            foreach (var thisstring in Values) {
                if (this[thisstring] == null) { Add(thisstring); }
            }
        }

        public void AddRange(ListExt<string> Values) {
            if (Values == null) { return; }
            foreach (var thisstring in Values) {
                if (this[thisstring] == null) { Add(thisstring, thisstring); }
            }
        }

        public void AddRange(List<string> Values, ColumnItem ColumnStyle, enShortenStyle Style, enBildTextVerhalten bildTextverhalten) {
            if (Values == null) { return; }
            if (Values.Count > 10000) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Values > 100000");
                return;
            }
            foreach (var thisstring in Values) {
                Add(thisstring, ColumnStyle, Style, bildTextverhalten); // If Item(thisstring) Is Nothing Then Add(New CellLikeItem(thisstring, ColumnStyle))
            }
        }

        public void AddRange(List<RowItem> list, string layoutID) {
            if (list == null || list.Count == 0) { return; }

            foreach (var thisRow in list) {
                Add(thisRow, layoutID);
            }
        }

        public void AddRange(List<string> list) {
            if (list == null) { return; }
            foreach (var thisstring in list) {
                if (!string.IsNullOrEmpty(thisstring)) {
                    if (this[thisstring] == null) { Add(thisstring, thisstring); }
                }
            }
        }

        /// <summary>
        /// Fügt die Spalte hinzu. Als interner Name wird der Column.Key verwendet.
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="doCaptionSort">Bei True werden auch die Überschriften der Spalte als Text hinzugefügt und auch danach sortiert</param>
        public void AddRange(ColumnCollection columns, bool doCaptionSort) {
            foreach (var ThisColumnItem in columns) {
                if (ThisColumnItem != null) {
                    var co = Add(ThisColumnItem);

                    if (doCaptionSort) {
                        co.UserDefCompareKey = ThisColumnItem.Ueberschriften + Constants.SecondSortChar + ThisColumnItem.Name;

                        var capt = ThisColumnItem.Ueberschriften;
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
            (var BiggestItemX, var _, var HeightAdded, var SenkrechtAllowed) = ItemData();
            if (SenkrechtAllowed == enOrientation.Waagerecht) { return ComputeAllItemPositions(new Size(300, 300), null, BiggestItemX, HeightAdded, SenkrechtAllowed); }
            BreakAfterItems = CalculateColumnCount(BiggestItemX, HeightAdded, SenkrechtAllowed);
            return ComputeAllItemPositions(new Size(1, 30), null, BiggestItemX, HeightAdded, SenkrechtAllowed);
        }

        public void Check(ListExt<string> vItems, bool Checked) => Check(vItems.ToArray(), Checked);

        public void Check(List<string> vItems, bool Checked) => Check(vItems.ToArray(), Checked);

        public void Check(string[] vItems, bool Checked) {
            for (var z = 0; z <= vItems.GetUpperBound(0); z++) {
                if (this[vItems[z]] != null) {
                    this[vItems[z]].Checked = Checked;
                }
            }
        }

        public void CheckAll() {
            foreach (var ThisItem in this) {
                if (ThisItem != null) {
                    ThisItem.Checked = true;
                }
            }
        }

        public List<BasicListItem> Checked() {
            List<BasicListItem> p = new();
            foreach (var ThisItem in this) {
                if (ThisItem != null && ThisItem.Checked) { p.Add(ThisItem); }
            }
            return p;
        }

        public object Clone() {
            ItemCollectionList x = new(_Appearance) {
                CheckBehavior = _CheckBehavior
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
        /// <param name="ZumDropdownHinzuAb">Erster Wert der Enumeration, der Hinzugefügt werden soll. Inklusive deses Wertes</param>
        /// <param name="ZumDropdownHinzuBis">Letzter Wert der Enumeration, der nicht mehr hinzugefügt wird, also exklusives diese Wertes</param>
        public void GetValuesFromEnum(Type t, int ZumDropdownHinzuAb, int ZumDropdownHinzuBis) {
            var items = Enum.GetValues(t);
            Clear();
            foreach (var thisItem in items) {
                var te = Enum.GetName(t, thisItem);
                var th = (int)thisItem;
                if (!string.IsNullOrEmpty(te)) {
                    //NewReplacer.Add(th.ToString() + "|" + te);
                    if (th >= ZumDropdownHinzuAb && th < ZumDropdownHinzuBis) {
                        Add(te, th.ToString());
                    }
                }
            }
        }

        public override void OnChanged() {
            _CellposCorrect = Size.Empty;
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
        public void Remove(string Internal) => Remove(this[Internal]);

        public void RemoveRange(List<string> Internal) {
            foreach (var item in Internal) {
                Remove(item);
            }
        }

        public void UncheckAll() {
            foreach (var ThisItem in this) {
                if (ThisItem != null) {
                    ThisItem.Checked = false;
                }
            }
        }

        internal Size ComputeAllItemPositions(Size controlDrawingArea, Slider sliderY, int biggestItemX, int heightAdded, enOrientation senkrechtAllowed) {
            try {
                if (Math.Abs(LastCheckedMaxSize.Width - controlDrawingArea.Width) > 0.1 || Math.Abs(LastCheckedMaxSize.Height - controlDrawingArea.Height) > 0.1) {
                    LastCheckedMaxSize = controlDrawingArea;
                    _CellposCorrect = Size.Empty;
                }
                if (!_CellposCorrect.IsEmpty) { return _CellposCorrect; }
                if (Count == 0) {
                    _CellposCorrect = Size.Empty;
                    return Size.Empty;
                }
                PreComputeSize();
                if (_ItemDesign == enDesign.Undefiniert) { GetDesigns(); }
                if (BreakAfterItems < 1) { senkrechtAllowed = enOrientation.Waagerecht; }
                var SliderWidth = 0;
                if (sliderY != null) {
                    if (BreakAfterItems < 1 && heightAdded > controlDrawingArea.Height) {
                        SliderWidth = sliderY.Width;
                    }
                }
                int colWidth;
                switch (_Appearance) {
                    case enBlueListBoxAppearance.Gallery:
                        colWidth = 200;
                        break;

                    case enBlueListBoxAppearance.FileSystem:
                        colWidth = 110;
                        break;

                    default:
                        // u.a. Autofilter
                        if (BreakAfterItems < 1) {
                            colWidth = controlDrawingArea.Width - SliderWidth;
                        } else {
                            var colCount = Count / BreakAfterItems;
                            var r = Count % colCount;
                            if (r != 0) { colCount++; }
                            colWidth = controlDrawingArea.Width < 5 ? biggestItemX : (controlDrawingArea.Width - SliderWidth) / colCount;
                        }
                        break;
                }
                var MaxX = int.MinValue;
                var Maxy = int.MinValue;
                var itenc = -1;
                BasicListItem previtem = null;
                foreach (var ThisItem in this) {
                    // PaintmodX kann immer abgezogen werden, da es eh nur bei einspaltigen Listboxen verändert wird!
                    if (ThisItem != null) {
                        var cx = 0;
                        var cy = 0;
                        var wi = colWidth;
                        var he = 0;
                        itenc++;
                        if (senkrechtAllowed == enOrientation.Waagerecht) {
                            if (ThisItem.IsCaption) { wi = controlDrawingArea.Width - SliderWidth; }
                            he = ThisItem.HeightForListBox(_Appearance, wi);
                        } else {
                            he = ThisItem.HeightForListBox(_Appearance, wi);
                        }
                        if (previtem != null) {
                            if (senkrechtAllowed == enOrientation.Waagerecht) {
                                if (previtem.Pos.Right + colWidth > controlDrawingArea.Width || ThisItem.IsCaption) {
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
                        ThisItem.SetCoordinates(new Rectangle(cx, cy, wi, he));
                        MaxX = Math.Max(ThisItem.Pos.Right, MaxX);
                        Maxy = Math.Max(ThisItem.Pos.Bottom, Maxy);
                        previtem = ThisItem;
                    }
                }
                if (sliderY != null) {
                    bool SetTo0;
                    if (SliderWidth > 0) {
                        if (Maxy - controlDrawingArea.Height <= 0) {
                            sliderY.Enabled = false;
                            SetTo0 = true;
                        } else {
                            sliderY.Enabled = true;
                            sliderY.Minimum = 0;
                            sliderY.SmallChange = 16;
                            sliderY.LargeChange = controlDrawingArea.Height;
                            sliderY.Maximum = Maxy - controlDrawingArea.Height;
                            SetTo0 = false;
                        }
                        sliderY.Height = controlDrawingArea.Height;
                        sliderY.Visible = true;
                    } else {
                        SetTo0 = true;
                        sliderY.Visible = false;
                    }
                    if (SetTo0) {
                        sliderY.Minimum = 0;
                        sliderY.Maximum = 0;
                        sliderY.Value = 0;
                    }
                }
                _CellposCorrect = new Size(MaxX, Maxy);
                return _CellposCorrect;
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
                foreach (var ThisItem in this) {
                    if (ThisItem != null) {
                        var s = ThisItem.SizeUntouchedForListBox();
                        w = Math.Max(w, s.Width);
                        h = Math.Max(h, s.Height);
                        hall += s.Height;
                        if (sameh < 0) {
                            sameh = ThisItem.SizeUntouchedForListBox().Height;
                        } else {
                            if (sameh != ThisItem.SizeUntouchedForListBox().Height) { or = enOrientation.Waagerecht; }
                            sameh = ThisItem.SizeUntouchedForListBox().Height;
                        }
                        if (ThisItem is not TextListItem and not CellLikeListItem) { or = enOrientation.Waagerecht; }
                    }
                }
                return (w, h, hall, or);
            } catch {
                return ItemData();
            }
        }

        internal void SetNewCheckState(BasicListItem This, bool value, ref bool CheckVariable) {
            if (!This.IsClickable()) { value = false; }
            if (_CheckBehavior == enCheckBehavior.NoSelection) {
                value = false;
            } else if (CheckVariable && value == false && _CheckBehavior == enCheckBehavior.AlwaysSingleSelection) {
                if (Checked().Count == 1) { value = true; }
            }
            if (value == CheckVariable) { return; }
            CheckVariable = value;
            if (_Validating) { return; }
            ValidateCheckStates(This);
            OnItemCheckedChanged();
            OnDoInvalidate();
        }

        internal void SetValuesTo(List<string> values, string fileEncryptionKey) {
            var _Ist = this.ToListOfString();
            var _zuviel = _Ist.Except(values).ToList();
            var _zuwenig = values.Except(_Ist).ToList();
            // Zu viele im Mains aus der Liste löschen
            foreach (var ThisString in _zuviel) {
                if (!values.Contains(ThisString)) { Remove(ThisString); }
            }
            // und die Mains auffüllen
            foreach (var ThisString in _zuwenig) {
                if (FileOperations.FileExists(ThisString)) {
                    if (ThisString.FileType() == enFileFormat.Image) {
                        Add(ThisString, ThisString, ThisString.FileNameWithoutSuffix(), fileEncryptionKey);
                    } else {
                        Add(ThisString.FileNameWithSuffix(), ThisString, QuickImage.Get(ThisString.FileType(), 48));
                    }
                } else {
                    Add(ThisString);
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

        private int CalculateColumnCount(int BiggestItemWidth, int AllItemsHeight, enOrientation orientation) {
            if (orientation != enOrientation.Senkrecht) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Nur 'senkrecht' erlaubt mehrere Spalten");
            }
            if (Count < 12) { return -1; }  // <10 ergibt dividieb by zere, weil es da 0 einträge währen bei 10 Spalten
            var dithemh = AllItemsHeight / Count;
            for (var TestSP = 10; TestSP >= 1; TestSP--) {
                var colc = Count / TestSP;
                var rest = Count % colc;
                var ok = true;
                if (rest > 0 && rest < colc / 2) { ok = false; }
                if (colc < 5) { ok = false; }
                if (colc > 20) { ok = false; }
                if (colc * dithemh > 600) { ok = false; }
                if (colc * dithemh < 150) { ok = false; }
                if (TestSP * BiggestItemWidth > 600) { ok = false; }
                if (colc * (float)dithemh / (TestSP * (float)BiggestItemWidth) < 0.5) { ok = false; }
                if (ok) {
                    return colc;
                }
            }
            return -1;
        }

        private void GetDesigns() {
            _ControlDesign = (enDesign)_Appearance;
            switch (_Appearance) {
                case enBlueListBoxAppearance.Autofilter:
                    _ItemDesign = enDesign.Item_Autofilter;
                    break;

                case enBlueListBoxAppearance.DropdownSelectbox:
                    _ItemDesign = enDesign.Item_DropdownMenu;
                    break;

                case enBlueListBoxAppearance.Gallery:
                    _ItemDesign = enDesign.Item_Listbox;
                    _ControlDesign = enDesign.ListBox;
                    break;

                case enBlueListBoxAppearance.FileSystem:
                    _ItemDesign = enDesign.Item_Listbox;
                    _ControlDesign = enDesign.ListBox;
                    break;

                case enBlueListBoxAppearance.Listbox:
                    _ItemDesign = enDesign.Item_Listbox;
                    _ControlDesign = enDesign.ListBox;
                    break;

                case enBlueListBoxAppearance.KontextMenu:
                    _ItemDesign = enDesign.Item_KontextMenu;
                    break;

                case enBlueListBoxAppearance.ComboBox_Textbox:
                    _ItemDesign = enDesign.ComboBox_Textbox;
                    break;

                default:
                    Develop.DebugPrint(enFehlerArt.Fehler, "Unbekanntes Design: " + _Appearance);
                    break;
            }
        }

        private void OnItemCheckedChanged() => ItemCheckedChanged?.Invoke(this, System.EventArgs.Empty);

        private void PreComputeSize() {
            try {
                System.Threading.Tasks.Parallel.ForEach(this, ThisItem => {
                    ThisItem.SizeUntouchedForListBox();
                });
            } catch {
                PreComputeSize();
            }
        }

        private void ValidateCheckStates(BasicListItem ThisMustBeChecked) {
            _Validating = true;
            var SomethingDonex = false;
            var Done = false;
            BasicListItem F = null;
            switch (_CheckBehavior) {
                case enCheckBehavior.NoSelection:
                    UncheckAll();
                    break;

                case enCheckBehavior.MultiSelection:
                    break;

                case enCheckBehavior.SingleSelection:
                case enCheckBehavior.AlwaysSingleSelection:
                    foreach (var ThisItem in this) {
                        if (ThisItem != null) {
                            if (ThisMustBeChecked == null) {
                                if (F == null) { F = ThisItem; }
                                if (ThisItem.Checked) {
                                    if (!Done) {
                                        Done = true;
                                    } else {
                                        if (ThisItem.Checked) {
                                            ThisItem.Checked = false;
                                            SomethingDonex = true;
                                        }
                                    }
                                }
                            } else {
                                Done = true;
                                if (ThisItem != ThisMustBeChecked && ThisItem.Checked) {
                                    SomethingDonex = true;
                                    ThisItem.Checked = false;
                                }
                            }
                        }
                    }
                    if (_CheckBehavior == enCheckBehavior.AlwaysSingleSelection && !Done && F != null && !F.Checked) {
                        F.Checked = true;
                        SomethingDonex = true;
                    }
                    break;

                default:
                    Develop.DebugPrint(_CheckBehavior);
                    break;
            }
            _Validating = false;
            if (SomethingDonex) { OnDoInvalidate(); }
        }

        #endregion
    }
}