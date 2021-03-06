// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueBasics.MultiUserFile;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using static BlueBasics.Extensions;
using static BlueBasics.FileOperations;

namespace BlueDatabase {

    public sealed class ColumnItem : IReadableTextWithChanging, ICompareKey, ICheckable, IDisposable {

        #region Fields

        public readonly ListExt<string> AfterEdit_AutoReplace = new();
        public readonly ListExt<string> DropDownItems = new();
        public readonly ListExt<string> OpticalReplace = new();
        public readonly ListExt<string> PermissionGroups_ChangeCell = new();
        public readonly ListExt<string> Regex = new();
        public readonly ListExt<string> Tags = new();
        public bool? TMP_AutoFilterSinnvoll = null;
        public Bitmap TMP_CaptionBitmap;
        public SizeF TMP_CaptionText_Size = new(-1, -1);
        public int? TMP_ColumnContentWidth = null;
        public int? TMP_IfFilterRemoved = null;
        internal Database _TMP_LinkedDatabase;
        internal List<string> _UcaseNamesSortedByLenght = null;
        private string _AdminInfo;
        private bool _AfterEdit_AutoCorrect;
        private bool _AfterEdit_DoUCase;
        private bool _AfterEdit_QuickSortRemoveDouble;
        private int _AfterEdit_Runden;
        private enAlignmentHorizontal _Align;
        private string _AllowedChars;
        private string _AutoFilterJoker;
        private string _AutoRemove;
        private Color _BackColor;
        private string _BestFile_StandardFolder;
        private string _BestFile_StandardSuffix;
        private int _BildCode_ConstantHeight;
        private enBildTextVerhalten _BildTextVerhalten;
        private string _Caption;
        private string _CaptionBitmapTXT;
        private string _CellInitValue;
        private Point _DauerFilterPos;
        private bool _DropdownAllesAbw�hlenErlaubt;
        private bool _DropdownBearbeitungErlaubt;

        //private bool _ZellenZusammenfassen;
        private int _DropDownKey;

        private bool _DropdownWerteAndererZellenAnzeigen;
        private bool _EditTrotzSperreErlaubt;
        private enEditTypeFormula _EditType;
        private enFilterOptions _FilterOptions;
        private Color _ForeColor;
        private enDataFormat _Format;
        private string _Identifier;
        private bool _IgnoreAtRowFilter;

        //private enDauerfilter _AutoFilter_Dauerfilter;
        private int _KeyColumnKey;

        private enColumnLineStyle _LineLeft;
        private enColumnLineStyle _LineRight;

        /// <summary>
        /// Die Quell-Spalte (aus der verlinkten Datenbank) ist immer
        /// </summary>
        private int _LinkedCell_ColumnKey;

        /// <summary>
        /// ...zus�tzlich folgende Zeichenkette hinzuf�gen
        /// </summary>
        private string _LinkedCell_ColumnValueAdd;

        /// <summary>
        /// Die zu suchende Spalte ist in dieser Spalte zu finden
        /// </summary>
        private int _LinkedCell_ColumnValueFoundIn;

        /// <summary>
        /// Die zu Suchende ZEile ist in dieser Spalte zu finden
        /// </summary>
        private int _LinkedCell_RowKey;

        private string _LinkedDatabaseFile;
        private string _LinkedKeyKennung;
        private bool _MultiLine;
        private string _Name;
        private string _Prefix;
        private string _QuickInfo;
        private bool _SaveContent;
        private bool _ShowMultiLineInOneLine;
        private bool _ShowUndo;
        private string _SortMask;
        private bool _SpellCheckingEnabled;
        private string _Suffix;
        private bool _TextBearbeitungErlaubt;
        private string _Ueberschrift1;
        private string _Ueberschrift2;
        private string _Ueberschrift3;
        private int _VorschlagsColumn;
        private bool disposedValue;

        #endregion

        #region Constructors

        public ColumnItem(Database database, int columnkey) {
            Database = database;
            Database.Disposing += Database_Disposing;
            if (columnkey < 0) { Develop.DebugPrint(enFehlerArt.Fehler, "ColumnKey <0"); }
            var ex = Database.Column.SearchByKey(columnkey);
            if (ex != null) { Develop.DebugPrint(enFehlerArt.Fehler, "Key existiert bereits"); }
            Key = columnkey;

            #region Standard-Werte

            _Name = Database.Column.Freename(string.Empty);
            _Caption = string.Empty;
            //_CaptionBitmap = null;
            _Format = enDataFormat.Bit;
            _LineLeft = enColumnLineStyle.D�nn;
            _LineRight = enColumnLineStyle.Ohne;
            _MultiLine = false;
            _QuickInfo = string.Empty;
            _Ueberschrift1 = string.Empty;
            _Ueberschrift2 = string.Empty;
            _Ueberschrift3 = string.Empty;
            //_Intelligenter_Multifilter = string.Empty;
            _ForeColor = Color.Black;
            _BackColor = Color.White;
            _CellInitValue = string.Empty;
            _LinkedCell_RowKey = -1;
            _LinkedCell_ColumnKey = -1;
            _LinkedCell_ColumnValueFoundIn = -1;
            _LinkedCell_ColumnValueAdd = string.Empty;
            _SortMask = string.Empty;
            //_ZellenZusammenfassen = false;
            _DropDownKey = -1;
            _VorschlagsColumn = -1;
            _Align = enAlignmentHorizontal.Keine_Pr�ferenz;
            _KeyColumnKey = -1;
            _EditType = enEditTypeFormula.Textfeld;
            _Identifier = string.Empty;
            _AllowedChars = string.Empty;
            _AdminInfo = string.Empty;
            _CaptionBitmapTXT = string.Empty;
            _FilterOptions = enFilterOptions.Enabled | enFilterOptions.TextFilterEnabled | enFilterOptions.ExtendedFilterEnabled;
            //_AutofilterErlaubt = true;
            //_AutofilterTextFilterErlaubt = true;
            //_AutoFilterErweitertErlaubt = true;
            _IgnoreAtRowFilter = false;
            _DropdownBearbeitungErlaubt = false;
            _DropdownAllesAbw�hlenErlaubt = false;
            _TextBearbeitungErlaubt = false;
            _DropdownWerteAndererZellenAnzeigen = false;
            _AfterEdit_QuickSortRemoveDouble = false;
            _AfterEdit_Runden = -1;
            _AfterEdit_AutoCorrect = false;
            _AfterEdit_DoUCase = false;
            _AutoRemove = string.Empty;
            _AutoFilterJoker = string.Empty;
            _SaveContent = true;
            //_AutoFilter_Dauerfilter = enDauerfilter.ohne;
            _SpellCheckingEnabled = false;
            //_CompactView = true;
            _ShowUndo = true;
            _ShowMultiLineInOneLine = false;
            _EditTrotzSperreErlaubt = false;
            _Suffix = string.Empty;
            _LinkedKeyKennung = string.Empty;
            _LinkedDatabaseFile = string.Empty;
            _BildTextVerhalten = enBildTextVerhalten.Nur_Text;
            _BildCode_ConstantHeight = 0;
            _Prefix = string.Empty;
            _BestFile_StandardSuffix = string.Empty;
            _BestFile_StandardFolder = string.Empty;
            _UcaseNamesSortedByLenght = null;
            //_Intelligenter_Multifilter = string.Empty;
            _DauerFilterPos = Point.Empty;

            #endregion Standard-Werte

            DropDownItems.Changed += DropDownItems_ListOrItemChanged;
            OpticalReplace.Changed += OpticalReplacer_ListOrItemChanged;
            AfterEdit_AutoReplace.Changed += AfterEdit_AutoReplace_ListOrItemChanged;
            Regex.Changed += Regex_ListOrItemChanged;
            PermissionGroups_ChangeCell.Changed += PermissionGroups_ChangeCell_ListOrItemChanged;
            Tags.Changed += Tags_ListOrItemChanged;
            Invalidate_TmpVariables();
        }

        #endregion

        #region Destructors

        // TODO: Finalizer nur �berschreiben, wenn "Dispose(bool disposing)" Code f�r die Freigabe nicht verwalteter Ressourcen enth�lt
        ~ColumnItem() {
            // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: false);
        }

        #endregion

        #region Events

        public event EventHandler Changed;

        #endregion

        #region Properties

        public string AdminInfo {
            get => _AdminInfo;
            set {
                if (_AdminInfo == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AdminInfo, this, _AdminInfo, value, true);
                OnChanged();
            }
        }

        public bool AfterEdit_AutoCorrect {
            get => _AfterEdit_AutoCorrect;
            set {
                if (_AfterEdit_AutoCorrect == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AfterEdit_AutoCorrect, this, _AfterEdit_AutoCorrect.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool AfterEdit_DoUCase {
            get => _AfterEdit_DoUCase;
            set {
                if (_AfterEdit_DoUCase == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AfterEdit_DoUcase, this, _AfterEdit_DoUCase.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool AfterEdit_QuickSortRemoveDouble {
            get => _MultiLine && _AfterEdit_QuickSortRemoveDouble;
            set {
                if (_AfterEdit_QuickSortRemoveDouble == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AfterEdit_QuickSortAndRemoveDouble, this, _AfterEdit_QuickSortRemoveDouble.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public int AfterEdit_Runden {
            get => _AfterEdit_Runden;
            set {
                if (_AfterEdit_Runden == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AfterEdit_Runden, this, _AfterEdit_Runden.ToString(), value.ToString(), true);
                OnChanged();
            }
        }

        public enAlignmentHorizontal Align {
            get => _Align;
            set {
                if (_Align == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Align, this, ((int)_Align).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public string AllowedChars {
            get => _AllowedChars;
            set {
                if (_AllowedChars == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AllowedChars, this, _AllowedChars, value, true);
                OnChanged();
            }
        }

        public string AutoFilterJoker {
            get => _AutoFilterJoker;
            set {
                if (_AutoFilterJoker == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AutoFilterJoker, this, _AutoFilterJoker, value, true);
                OnChanged();
            }
        }

        public string AutoRemove {
            get => _AutoRemove;
            set {
                if (_AutoRemove == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AutoRemove, this, _AutoRemove, value, true);
                OnChanged();
            }
        }

        public Color BackColor {
            get => _BackColor;
            set {
                if (_BackColor.ToArgb() == value.ToArgb()) { return; }
                Database.AddPending(enDatabaseDataType.co_BackColor, this, _BackColor.ToArgb().ToString(), value.ToArgb().ToString(), true);
                OnChanged();
            }
        }

        public string BestFile_StandardFolder {
            get => _BestFile_StandardFolder;
            set {
                if (_BestFile_StandardFolder == value) { return; }
                Database.AddPending(enDatabaseDataType.co_BestFile_StandardFolder, this, _BestFile_StandardFolder, value, true);
                OnChanged();
            }
        }

        public string BestFile_StandardSuffix {
            get => _BestFile_StandardSuffix;
            set {
                if (_BestFile_StandardSuffix == value) { return; }
                Database.AddPending(enDatabaseDataType.co_BestFile_StandardSuffix, this, _BestFile_StandardSuffix, value, true);
                OnChanged();
            }
        }

        public int BildCode_ConstantHeight {
            get => _BildCode_ConstantHeight;
            set {
                if (_BildCode_ConstantHeight == value) { return; }
                Database.AddPending(enDatabaseDataType.co_BildCode_ConstantHeight, this, _BildCode_ConstantHeight.ToString(), value.ToString(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public enBildTextVerhalten BildTextVerhalten {
            get => _BildTextVerhalten;
            set {
                if (_BildTextVerhalten == value) { return; }
                Database.AddPending(enDatabaseDataType.co_BildTextVerhalten, this, ((int)_BildTextVerhalten).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public string Caption {
            get => _Caption;
            set {
                value = value.Replace("<br>", "\r", RegexOptions.IgnoreCase);
                if (_Caption == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Caption, this, _Caption, value, true);
                Invalidate_TmpVariables();
                OnChanged();
            }
        }

        public string CaptionBitmap {
            get => _CaptionBitmapTXT;
            set {
                if (_CaptionBitmapTXT == value) { return; }
                Database.AddPending(enDatabaseDataType.co_CaptionBitmapTXT, this, _CaptionBitmapTXT, value, false);
                _CaptionBitmapTXT = value;
                Invalidate_TmpVariables();
                OnChanged();
            }
        }

        public string CellInitValue {
            get => _CellInitValue;
            set {
                if (_CellInitValue == value) { return; }
                Database.AddPending(enDatabaseDataType.co_CellInitValue, this, _CellInitValue, value, true);
                OnChanged();
            }
        }

        public Database Database { get; private set; }

        //public string Intelligenter_Multifilter
        //{
        //    get
        //    {
        //        return _Intelligenter_Multifilter;
        //    }
        //    set
        //    {
        //        if (_Intelligenter_Multifilter == value) { return; }
        //        Database.AddPending(enDatabaseDataType.co_Intelligenter_Multifilter, this, _Intelligenter_Multifilter, value, true);
        //        OnChanged();
        //    }
        //}
        public Point DauerFilterPos {
            get => _DauerFilterPos;
            set {
                if (_DauerFilterPos.ToString() == value.ToString()) { return; }
                Database.AddPending(enDatabaseDataType.co_DauerFilterPos, this, _DauerFilterPos.ToString(), value.ToString(), true);
                OnChanged();
            }
        }

        public bool DropdownAllesAbw�hlenErlaubt {
            get => _DropdownAllesAbw�hlenErlaubt;
            set {
                if (_DropdownAllesAbw�hlenErlaubt == value) { return; }
                Database.AddPending(enDatabaseDataType.co_DropdownAllesAbw�hlenErlaubt, this, _DropdownAllesAbw�hlenErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool DropdownBearbeitungErlaubt {
            get => _DropdownBearbeitungErlaubt;
            set {
                if (_DropdownBearbeitungErlaubt == value) { return; }
                Database.AddPending(enDatabaseDataType.co_DropdownBearbeitungErlaubt, this, _DropdownBearbeitungErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        //public enDauerfilter AutoFilter_Dauerfilter
        //{
        //    get
        //    {
        //        return _AutoFilter_Dauerfilter;
        //    }
        //    set
        //    {
        //        if (_AutoFilter_Dauerfilter == value) { return; }
        //        Database.AddPending(enDatabaseDataType.co_AutoFilter_Dauerfilter, this, ((int)_AutoFilter_Dauerfilter).ToString(), ((int)value).ToString(), true);
        //        OnChanged();
        //    }
        //}
        public int DropdownKey {
            get => _DropDownKey;
            set {
                if (_DropDownKey == value) { return; }
                Database.AddPending(enDatabaseDataType.co_DropDownKey, this, _DropDownKey.ToString(), value.ToString(), true);
                OnChanged();
            }
        }

        public bool DropdownWerteAndererZellenAnzeigen {
            get => _DropdownWerteAndererZellenAnzeigen;
            set {
                if (_DropdownWerteAndererZellenAnzeigen == value) { return; }
                Database.AddPending(enDatabaseDataType.co_DropdownWerteAndererZellenAnzeigen, this, _DropdownWerteAndererZellenAnzeigen.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool EditTrotzSperreErlaubt {
            get => _EditTrotzSperreErlaubt;
            set {
                if (_EditTrotzSperreErlaubt == value) { return; }
                Database.AddPending(enDatabaseDataType.co_EditTrotzSperreErlaubt, this, _EditTrotzSperreErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public enEditTypeFormula EditType {
            get => _EditType;
            set {
                if (_EditType == value) { return; }
                Database.AddPending(enDatabaseDataType.co_EditType, this, ((int)_EditType).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public enFilterOptions FilterOptions {
            get => _FilterOptions;
            set {
                if (_FilterOptions == value) { return; }
                Database.AddPending(enDatabaseDataType.co_FilterOptions, this, ((int)_FilterOptions).ToString(), ((int)value).ToString(), true);
                Invalidate_TmpVariables();
                OnChanged();
            }
        }

        public Color ForeColor {
            get => _ForeColor;
            set {
                if (_ForeColor.ToArgb() == value.ToArgb()) { return; }
                Database.AddPending(enDatabaseDataType.co_ForeColor, this, _ForeColor.ToArgb().ToString(), value.ToArgb().ToString(), true);
                OnChanged();
            }
        }

        public enDataFormat Format {
            get => _Format;
            set {
                if (_Format == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Format, this, ((int)_Format).ToString(), ((int)value).ToString(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public string I_Am_A_Key_For_Other_Column { get; private set; }

        public string Identifier {
            get => _Identifier;
            set {
                if (_Identifier == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Identifier, this, _Identifier, value, true);
                OnChanged();
            }
        }

        //public bool AutoFilterErlaubt
        //{
        //    get
        //    {
        //        if (!_Format.Autofilter_m�glich()) { return false; }
        //        return _AutofilterErlaubt;
        //    }
        //    set
        //    {
        //        if (_AutofilterErlaubt == value) { return; }
        //        Database.AddPending(enDatabaseDataType.co_AutoFilterErlaubt, this, _AutofilterErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
        //        Invalidate_TmpVariables();
        //        OnChanged();
        //    }
        //}
        //public bool AutoFilterErweitertErlaubt
        //{
        //    get
        //    {
        //        if (!AutoFilterErlaubt) { return false; }
        //        return _AutoFilterErweitertErlaubt;
        //    }
        //    set
        //    {
        //        if (_AutoFilterErweitertErlaubt == value) { return; }
        //        Database.AddPending(enDatabaseDataType.co_AutoFilterErweitertErlaubt, this, _AutoFilterErweitertErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
        //        OnChanged();
        //    }
        //}
        //public bool AutofilterTextFilterErlaubt
        //{
        //    get
        //    {
        //        if (!AutoFilterErlaubt) { return false; }
        //        if (!_Format.TextboxEditPossible()) { return false; }
        //        return _AutofilterTextFilterErlaubt;
        //    }
        //    set
        //    {
        //        if (_AutofilterTextFilterErlaubt == value) { return; }
        //        Database.AddPending(enDatabaseDataType.co_AutoFilterTextFilterErlaubt, this, _AutofilterTextFilterErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
        //        OnChanged();
        //    }
        //}
        public bool IgnoreAtRowFilter {
            get => !_Format.Autofilter_m�glich() || _IgnoreAtRowFilter;
            set {
                if (_IgnoreAtRowFilter == value) { return; }
                Database.AddPending(enDatabaseDataType.co_BeiZeilenfilterIgnorieren, this, _IgnoreAtRowFilter.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public int Key { get; }

        /// <summary>
        /// H�lt Werte, dieser Spalte gleich, bezugnehmend der KeyColumn(key)
        /// </summary>
        public int KeyColumnKey {
            get => _KeyColumnKey;
            set {
                if (_KeyColumnKey == value) { return; }
                var c = Database.Column.SearchByKey(_KeyColumnKey);
                if (c != null) { c.CheckIfIAmAKeyColumn(); }
                Database.AddPending(enDatabaseDataType.co_KeyColumnKey, this, _KeyColumnKey.ToString(), value.ToString(), true);
                c = Database.Column.SearchByKey(_KeyColumnKey);
                if (c != null) { c.CheckIfIAmAKeyColumn(); }
                OnChanged();
            }
        }

        public enColumnLineStyle LineLeft {
            get => _LineLeft;
            set {
                if (_LineLeft == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LineLeft, this, ((int)_LineLeft).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public enColumnLineStyle LineRight {
            get => _LineRight;
            set {
                if (_LineRight == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LinieRight, this, ((int)_LineRight).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public int LinkedCell_ColumnKey {
            get => _LinkedCell_ColumnKey;
            set {
                if (_LinkedCell_ColumnKey == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LinkedCell_ColumnKey, this, _LinkedCell_ColumnKey.ToString(), value.ToString(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public string LinkedCell_ColumnValueAdd {
            get => _LinkedCell_ColumnValueAdd;
            set {
                if (_LinkedCell_ColumnValueAdd == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LinkedCell_ColumnValueAdd, this, _LinkedCell_ColumnValueAdd, value, true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public int LinkedCell_ColumnValueFoundIn {
            get => _LinkedCell_ColumnValueFoundIn;
            set {
                if (_LinkedCell_ColumnValueFoundIn == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LinkedCell_ColumnValueFoundIn, this, _LinkedCell_ColumnValueFoundIn.ToString(), value.ToString(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public int LinkedCell_RowKey {
            get => _LinkedCell_RowKey;
            set {
                if (_LinkedCell_RowKey == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LinkedCell_RowKey, this, _LinkedCell_RowKey.ToString(), value.ToString(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public string LinkedDatabaseFile {
            get => _LinkedDatabaseFile;
            set {
                if (_LinkedDatabaseFile == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LinkedDatabase, this, _LinkedDatabaseFile, value, true);
                Invalidate_TmpVariables();
                OnChanged();
            }
        }

        public string LinkedKeyKennung {
            get => _LinkedKeyKennung;
            set {
                if (_LinkedKeyKennung == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LinkKeyKennung, this, _LinkedKeyKennung, value, true);
                OnChanged();
            }
        }

        public bool MultiLine {
            get => _MultiLine;
            set {
                if (!_Format.MultilinePossible()) { value = false; }
                if (_MultiLine == value) { return; }
                Database.AddPending(enDatabaseDataType.co_MultiLine, this, _MultiLine.ToPlusMinus(), value.ToPlusMinus(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public string Name {
            get => _Name.ToUpper();
            set {
                value = value.ToUpper();
                if (value == _Name.ToUpper()) { return; }
                if (Database.Column.Exists(value) != null) { return; }
                if (string.IsNullOrEmpty(value)) { return; }
                var old = _Name;
                Database.AddPending(enDatabaseDataType.co_Name, this, _Name, value, true);
                //_Name = value;
                Database.Column_NameChanged(old, this);
                OnChanged();
            }
        }

        public string Prefix {
            get => _Prefix;
            set {
                if (_Prefix == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Prefix, this, _Prefix, value, true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public string Quickinfo {
            get => _QuickInfo;
            set {
                if (_QuickInfo == value) { return; }
                Database.AddPending(enDatabaseDataType.co_QuickInfo, this, _QuickInfo, value, true);
                OnChanged();
            }
        }

        public bool SaveContent {
            get => _SaveContent;
            set {
                if (_SaveContent == value) { return; }
                Database.AddPending(enDatabaseDataType.co_SaveContent, this, _SaveContent.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool ShowMultiLineInOneLine {
            get => _MultiLine && _ShowMultiLineInOneLine;
            set {
                if (_ShowMultiLineInOneLine == value) { return; }
                Database.AddPending(enDatabaseDataType.co_ShowMultiLineInOneLine, this, _ShowMultiLineInOneLine.ToPlusMinus(), value.ToPlusMinus(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public bool ShowUndo {
            get => _ShowUndo;
            set {
                if (_ShowUndo == value) { return; }
                Database.AddPending(enDatabaseDataType.co_ShowUndo, this, _ShowUndo.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public string SortMask {
            get => _SortMask;
            set {
                if (_SortMask == value) { return; }
                Database.AddPending(enDatabaseDataType.co_SortMask, this, _SortMask, value, true);
                OnChanged();
            }
        }

        public bool SpellCheckingEnabled {
            get => _SpellCheckingEnabled;
            set {
                if (_SpellCheckingEnabled == value) { return; }
                Database.AddPending(enDatabaseDataType.co_SpellCheckingEnabled, this, _SpellCheckingEnabled.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        /// <summary>
        /// Was in Textfeldern oder Datenbankzeilen f�r ein Suffix angezeigt werden soll. Beispiel: mm
        /// </summary>
        public string Suffix {
            get => _Suffix;
            set {
                if (_Suffix == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Suffix, this, _Suffix, value, true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public bool TextBearbeitungErlaubt {
            get => _TextBearbeitungErlaubt;
            set {
                if (_TextBearbeitungErlaubt == value) { return; }
                Database.AddPending(enDatabaseDataType.co_TextBearbeitungErlaubt, this, _TextBearbeitungErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public string Ueberschrift1 {
            get => _Ueberschrift1;
            set {
                if (_Ueberschrift1 == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Ueberschrift1, this, _Ueberschrift1, value, true);
                OnChanged();
            }
        }

        public string Ueberschrift2 {
            get => _Ueberschrift2;
            set {
                if (_Ueberschrift2 == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Ueberschrift2, this, _Ueberschrift2, value, true);
                OnChanged();
            }
        }

        public string Ueberschrift3 {
            get => _Ueberschrift3;
            set {
                if (_Ueberschrift3 == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Ueberschrift3, this, _Ueberschrift3, value, true);
                OnChanged();
            }
        }

        public string Ueberschriften {
            get {
                var txt = _Ueberschrift1 + "/" + _Ueberschrift2 + "/" + _Ueberschrift3;
                return txt == "//" ? "###" : txt.TrimEnd("/");
            }
        }

        public int VorschlagsColumn {
            get => _VorschlagsColumn;
            set {
                if (_VorschlagsColumn == value) { return; }
                Database.AddPending(enDatabaseDataType.co_VorschlagColumn, this, _VorschlagsColumn.ToString(), value.ToString(), true);
                OnChanged();
            }
        }

        private Database TMP_LinkedDatabase {
            set {
                if (value == _TMP_LinkedDatabase) { return; }
                Invalidate_TmpVariables();
                _TMP_LinkedDatabase = value;
                if (_TMP_LinkedDatabase != null) {
                    _TMP_LinkedDatabase.RowKeyChanged += _TMP_LinkedDatabase_RowKeyChanged;
                    _TMP_LinkedDatabase.ColumnKeyChanged += _TMP_LinkedDatabase_ColumnKeyChanged;
                    _TMP_LinkedDatabase.ConnectedControlsStopAllWorking += _TMP_LinkedDatabase_ConnectedControlsStopAllWorking;
                    _TMP_LinkedDatabase.Cell.CellValueChanged += _TMP_LinkedDatabase_Cell_CellValueChanged;
                    _TMP_LinkedDatabase.Disposing += _TMP_LinkedDatabase_Disposing;
                }
            }
        }

        #endregion

        #region Methods

        public static enEditTypeTable UserEditDialogTypeInTable(ColumnItem vColumn, bool DoDropDown) => UserEditDialogTypeInTable(vColumn.Format, DoDropDown, vColumn.TextBearbeitungErlaubt, vColumn.MultiLine);

        public static enEditTypeTable UserEditDialogTypeInTable(enDataFormat Format, bool DoDropDown, bool KeybordInputAllowed, bool isMultiline) {
            if (!DoDropDown && !KeybordInputAllowed) { return enEditTypeTable.None; }
            switch (Format) {
                case enDataFormat.Bit:
                case enDataFormat.Columns_f�r_LinkedCellDropdown:
                case enDataFormat.Values_f�r_LinkedCellDropdown:
                    return enEditTypeTable.Dropdown_Single;

                case enDataFormat.Link_To_Filesystem:
                    return enEditTypeTable.FileHandling_InDateiSystem;

                case enDataFormat.FarbeInteger:
                    if (DoDropDown) { return enEditTypeTable.Dropdown_Single; }
                    return enEditTypeTable.Farb_Auswahl_Dialog;

                case enDataFormat.Schrift:
                    if (DoDropDown) { return enEditTypeTable.Dropdown_Single; }
                    return enEditTypeTable.Font_AuswahlDialog;

                case enDataFormat.Button:
                    return enEditTypeTable.None;

                case enDataFormat.Text_mit_Formatierung:
                    return enEditTypeTable.WarnungNurFormular; // Wegen dem Sonderzeichen
                default:
                    if (Format.TextboxEditPossible()) {
                        return !DoDropDown
                            ? enEditTypeTable.Textfeld
                            : isMultiline
                            ? enEditTypeTable.Dropdown_Single
                            : KeybordInputAllowed ? enEditTypeTable.Textfeld_mit_Auswahlknopf : enEditTypeTable.Dropdown_Single;
                    }
                    Develop.DebugPrint(Format);
                    return enEditTypeTable.None;
            }
        }

        public string AutoCorrect(string Value) {
            if (Format == enDataFormat.Link_To_Filesystem) {
                List<string> l = new(Value.SplitByCR());
                List<string> l2 = new();
                foreach (var thisFile in l) {
                    l2.Add(SimplyFile(thisFile));
                }
                Value = l2.SortedDistinctList().JoinWithCr();
            }
            if (_AfterEdit_DoUCase) { Value = Value.ToUpper(); }
            if (!string.IsNullOrEmpty(_AutoRemove)) { Value = Value.RemoveChars(_AutoRemove); }
            if (AfterEdit_AutoReplace.Count > 0) {
                List<string> l = new(Value.SplitByCR());
                foreach (var thisar in AfterEdit_AutoReplace) {
                    var rep = thisar.SplitBy("|");
                    for (var z = 0; z < l.Count; z++) {
                        var r = string.Empty;
                        if (rep.Length > 1) { r = rep[1].Replace(";cr;", "\r"); }
                        var op = string.Empty;
                        if (rep.Length > 2) { op = rep[2].ToLower(); }
                        if (op == "casesensitive") {
                            if (l[z] == rep[0]) { l[z] = r; }
                        } else if (op == "instr") {
                            l[z] = l[z].Replace(rep[0], r, RegexOptions.IgnoreCase);
                            //if (l[z].ToLower() == rep[0].ToLower()) { l[z] = r; }
                        } else {
                            if (l[z].ToLower() == rep[0].ToLower()) { l[z] = r; }
                        }
                    }
                }
                Value = l.JoinWithCr();
            }
            if (_AfterEdit_AutoCorrect) { Value = KleineFehlerCorrect(Value); }
            if (_AfterEdit_Runden > -1 && double.TryParse(Value, out var erg)) {
                erg = Math.Round(erg, _AfterEdit_Runden);
                Value = erg.ToString();
            }
            if (_AfterEdit_QuickSortRemoveDouble) {
                var l = new List<string>(Value.SplitByCR()).SortedDistinctList();
                Value = l.JoinWithCr();
            }
            return Value;
        }

        public List<string> Autofilter_ItemList(FilterCollection filter, List<RowItem> pinned) {
            if (filter == null || filter.Count < 0) { return Contents(null, pinned); }
            FilterCollection tfilter = new(Database);
            foreach (var ThisFilter in filter) {
                if (ThisFilter != null && this != ThisFilter.Column) { tfilter.Add(ThisFilter); }
            }
            return Contents(tfilter, pinned);
        }

        public bool AutoFilterSymbolPossible() => FilterOptions.HasFlag(enFilterOptions.Enabled) && Format.Autofilter_m�glich();

        /// <summary>
        /// Gibt einen Dateinamen mit Pfad und Suffix zur�ck, der sich aus dem Standard-Angaben der Spalte zusammensetzt.
        /// Existiert in keiner Spalte und auch nicht auf der Festplatte.
        /// </summary>
        public string BestFile() => BestFile(string.Empty, true);

        /// <summary>
        /// Gibt den Dateinamen mit Pfad und Suffix zur�ck, der sich aus dem Standard-Angaben der Zelle und dem hier �bergebebenen Dateinamen zusammensetzt.
        /// </summary>
        /// <param name="filename">Der Dateiname. Ein evtl. fehlender Pfad und ein evtl. fehlendes Suffix werden erg�nzt. </param>
        /// <param name="mustBeFree">Wenn True wird ein Dateiname zur�ckgegeben, der noch nicht im Verzeichnis vorhanden ist.</param>
        /// <returns> Gibt den Dateinamen mit Pfad und Suffix zur�ck</returns>
        public string BestFile(string filename, bool mustBeFree) {
            if (_Format != enDataFormat.Link_To_Filesystem) { Develop.DebugPrint(enFehlerArt.Fehler, "Nur bei Link_To_Filesystem erlaubt!"); }
            //FileNameWithoutPath = FileNameWithoutPath.RemoveChars(Constants.Char_DateiSonderZeichen); // Falls ein Korrekter Pfad �bergeben wurde, w�rde er hier verst�mmelt werden
            if (string.IsNullOrEmpty(filename)) {
                if (!mustBeFree) { return string.Empty; }
                filename = (_Name.Substring(0, 1) + DateTime.Now.ToString("mm.fff")).RemoveChars(Constants.Char_DateiSonderZeichen + ".");
            }
            if (filename.Contains("\r")) { Develop.DebugPrint_NichtImplementiert(); }
            // Wenn FileNameWithoutPath kein Suffix hat, das Standard Suffix hinzuf�gen
            var suffix = filename.FileSuffix();
            var cleanfilename = filename;
            if (string.IsNullOrEmpty(suffix)) {
                suffix = _BestFile_StandardSuffix;
            } else {
                cleanfilename = filename.FileNameWithoutSuffix();
            }
            // Den Standardfolder benutzen. Falls dieser fehlt, 'Files' benutzen.
            var directory = _BestFile_StandardFolder.Trim("\\");
            if (string.IsNullOrEmpty(directory)) { directory = "Files"; }
            // Ist nur ein Unterferzeichniss angegeben, den Datenbankpfad benutzen und das Unterverzeichniss anh�ngen
            if (directory.Substring(1, 1) != ":" && directory.Substring(0, 1) != "\\") { directory = Database.Filename.FilePath() + directory; }
            if (!mustBeFree) {
                return (directory.TrimEnd("\\") + "\\" + cleanfilename + "." + suffix.ToLower()).TrimEnd(".");
            }
            var nr = -1;
            do {
                nr++;
                var tmpname = cleanfilename;
                if (nr > 0) { tmpname += nr.ToString(Constants.Format_Integer2); }
                var ok = true;
                foreach (var columnitem in Database.Column) {
                    if (columnitem.Format == enDataFormat.Link_To_Filesystem) {
                        var r = Database.Row[new FilterItem(columnitem, enFilterType.Istgleich_Gro�KleinEgal, tmpname)];
                        if (r != null) {
                            ok = false;
                            break;
                        }
                    }
                }
                if (ok) {
                    var tmp = (directory.TrimEnd("\\") + "\\" + tmpname + "." + suffix.ToLower()).TrimEnd(".");
                    if (!FileExists(tmp)) { return tmp; }
                }
            } while (true);
        }

        public string CompareKey() {
            var tmp = string.IsNullOrEmpty(_Caption) ? _Name + Constants.FirstSortChar + _Name : _Caption + Constants.FirstSortChar + _Name;
            tmp = tmp.Trim(' ');
            tmp = tmp.TrimStart('-');
            tmp = tmp.Trim(' ');
            return tmp;
        }

        public List<string> Contents() => Contents(null, null);

        public List<string> Contents(FilterCollection filter, List<RowItem> pinned) {
            List<string> list = new();
            foreach (var ThisRowItem in Database.Row) {
                if (ThisRowItem != null) {
                    var add = ThisRowItem.MatchesTo(filter);
                    if (!add && pinned != null) { add = pinned.Contains(ThisRowItem); }
                    if (add) {
                        if (_MultiLine) {
                            list.AddRange(ThisRowItem.CellGetList(this));
                        } else {
                            if (ThisRowItem.CellGetString(this).Length > 0) {
                                list.Add(ThisRowItem.CellGetString(this));
                            }
                        }
                    }
                }
            }
            return list.SortedDistinctList();
        }

        public void DeleteContents(FilterCollection filter, List<RowItem> pinned) {
            foreach (var ThisRowItem in Database.Row) {
                if (ThisRowItem != null) {
                    if (ThisRowItem.MatchesTo(filter) || pinned.Contains(ThisRowItem)) { ThisRowItem.CellSet(this, ""); }
                }
            }
        }

        public void Dispose() {
            // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public string ErrorReason() {
            if (Key < 0) { return "Interner Fehler: ID nicht definiert"; }
            if (string.IsNullOrEmpty(_Name)) { return "Spaltenname nicht definiert."; }
            if (!Name.ContainsOnlyChars(Constants.AllowedCharsVariableName)) { return "Spaltenname enth�lt ung�ltige Zeichen. Erlaubt sind A-Z, 0-9 und _"; }
            foreach (var ThisColumn in Database.Column) {
                if (ThisColumn != this && ThisColumn != null) {
                    if (_Name.ToUpper() == ThisColumn.Name.ToUpper()) { return "Spalten-Name bereits vorhanden."; }
                }
            }
            if (string.IsNullOrEmpty(_Caption)) { return "Spalten Beschriftung fehlt."; }
            if (!_SaveContent && string.IsNullOrEmpty(_Identifier)) { return "Inhalt der Spalte muss gespeichert werden."; }
            if (!_SaveContent && _ShowUndo) { return "Wenn der Inhalt der Spalte nicht gespeichert wird, darf auch kein Undo geloggt werden."; }
            if (((int)_Format).ToString() == _Format.ToString()) { return "Format fehlerhaft."; }
            if (_Format.NeedTargetDatabase()) {
                if (LinkedDatabase() == null) { return "Verkn�pfte Datenbank fehlt oder existiert nicht."; }
                if (LinkedDatabase() == Database) { return "Zirkelbezug mit verkn�pfter Datenbank."; }
            }
            if (!_Format.Autofilter_m�glich() && _FilterOptions != enFilterOptions.None) { return "Bei diesem Format keine Filterung erlaubt."; }
            if (_FilterOptions != enFilterOptions.None && !_FilterOptions.HasFlag(enFilterOptions.Enabled)) { return "Filter Kombination nicht m�glich."; }
            if (_FilterOptions != enFilterOptions.Enabled_OnlyAndAllowed && _FilterOptions.HasFlag(enFilterOptions.OnlyAndAllowed)) { return "Filter Kombination nicht m�glich."; }
            if (_FilterOptions != enFilterOptions.Enabled_OnlyOrAllowed && _FilterOptions.HasFlag(enFilterOptions.OnlyOrAllowed)) { return "Filter Kombination nicht m�glich."; }
            if (_FilterOptions.HasFlag(enFilterOptions.OnlyAndAllowed) || _FilterOptions.HasFlag(enFilterOptions.OnlyOrAllowed)) {
                if (!_MultiLine) {
                    return "Dieser Filter kann nur bei Mehrzeiligen Spalten benutzt werden.";
                }
            }
            switch (_Format) {
                case enDataFormat.Bit:
                    if (_FilterOptions.HasFlag(enFilterOptions.ExtendedFilterEnabled)) { return "Format unterst�tzt keinen 'erweiternden Autofilter'"; }
                    if (_FilterOptions.HasFlag(enFilterOptions.TextFilterEnabled)) { return "Format unterst�tzt keine 'Texteingabe bei Autofilter'"; }
                    if (!string.IsNullOrEmpty(_AutoFilterJoker)) { return "Format unterst�tzt keinen 'Autofilter Joker'"; }
                    if (!_IgnoreAtRowFilter) { return "Format muss bei Zeilenfilter ignoriert werden.'"; }
                    break;

                case enDataFormat.RelationText:
                    if (!_MultiLine) { return "Bei diesem Format muss mehrzeilig ausgew�hlt werden."; }
                    if (_KeyColumnKey > -1) { return "Diese Format darf keine Verkn�pfung zu einer Schl�sselspalte haben."; }
                    if (IsFirst()) { return "Diese Format ist bei der ersten (intern) erste Spalte nicht erlaubt."; }
                    if (!string.IsNullOrEmpty(_CellInitValue)) { return "Diese Format kann keinen Initial-Text haben."; }
                    if (_VorschlagsColumn > 0) { return "Diese Format kann keine Vorschlags-Spalte haben."; }
                    break;

                case enDataFormat.LinkedCell:
                    if (!string.IsNullOrEmpty(_CellInitValue)) { return "Dieses Format kann keinen Initial-Text haben."; }
                    if (_KeyColumnKey > -1) { return "Dieses Format darf keine Verkn�pfung zu einer Schl�sselspalte haben."; }
                    if (IsFirst()) { return "Dieses Format ist bei der ersten (intern) erste Spalte nicht erlaubt."; }
                    if (_LinkedCell_RowKey < 0) { return "Die Angabe der Spalte, aus der der Schl�sselwert geholt wird, fehlt."; }
                    if (_LinkedCell_ColumnValueFoundIn < 0 && _LinkedCell_ColumnKey < 0) { return "Information fehlt, welche Spalte der Zieldatenbank verwendet werden soll."; }
                    if (_LinkedCell_ColumnValueFoundIn > -1 && _LinkedCell_ColumnKey > -1) { return "Doppelte Informationen, welche Spalte der Zieldatenbank verwendet werden soll."; }
                    if (_LinkedCell_ColumnValueFoundIn < 0 && !string.IsNullOrEmpty(LinkedCell_ColumnValueAdd)) { return "Falsche Ziel-Spalte ODER Spalten-Vortext flasch."; }
                    if (_VorschlagsColumn > 0) { return "Dieses Format kann keine Vorschlags-Spalte haben."; }
                    if (_LinkedCell_ColumnKey > 0) {
                        var c = LinkedDatabase().Column.SearchByKey(_LinkedCell_ColumnKey);
                        if (c == null) { return "Die verkn�pfte Spalte existiert nicht."; }
                        if (c.MultiLine != _MultiLine) { return "Multiline stimmt nicht mit der Ziel-Spalte Multiline �berein"; }
                    } else {
                        if (!_MultiLine) { return "Dieses Format muss mehrzeilig sein, da es von der Ziel-Spalte gesteuert wird."; }
                    }
                    break;

                case enDataFormat.Values_f�r_LinkedCellDropdown:
                    if (!string.IsNullOrEmpty(_CellInitValue)) { return "Dieses Format kann keinen Initial-Text haben."; }
                    if (KeyColumnKey > -1) { return "Dieses Format darf keine Verkn�pfung zu einer Schl�sselspalte haben."; }
                    if (_VorschlagsColumn > 0) { return "Dieses Format kann keine Vorschlags-Spalte haben."; }
                    break;

                case enDataFormat.Link_To_Filesystem:
                    if (!string.IsNullOrEmpty(_CellInitValue)) { return "Dieses Format kann keinen Initial-Text haben."; }
                    if (_MultiLine && !_AfterEdit_QuickSortRemoveDouble) { return "Format muss sortiert werden."; }
                    if (_VorschlagsColumn > 0) { return "Dieses Format kann keine Vorschlags-Spalte haben."; }
                    if (!string.IsNullOrEmpty(_AutoRemove)) { return "Dieses Format darf keine Autokorrektur-Ma�nahmen haben haben."; }
                    if (_AfterEdit_AutoCorrect) { return "Dieses Format darf keine Autokorrektur-Ma�nahmen haben haben."; }
                    if (_AfterEdit_DoUCase) { return "Dieses Format darf keine Autokorrektur-Ma�nahmen haben haben."; }
                    if (_AfterEdit_Runden != -1) { return "Dieses Format darf keine Autokorrektur-Ma�nahmen haben haben."; }
                    if (AfterEdit_AutoReplace.Count > 0) { return "Dieses Format darf keine Autokorrektur-Ma�nahmen haben haben."; }
                    break;

                case enDataFormat.Text_mit_Formatierung:
                    if (_AfterEdit_QuickSortRemoveDouble) { return "Format darf nicht sortiert werden."; }
                    break;
            }
            if (_MultiLine) {
                if (!_Format.MultilinePossible()) { return "Format unterst�tzt keine mehrzeiligen Texte."; }
                if (_AfterEdit_Runden != -1) { return "Runden nur bei einzeiligen Texten m�glich"; }
            } else {
                if (_ShowMultiLineInOneLine) { return "Wenn mehrzeilige Texte einzeilig dargestellt werden sollen, muss mehrzeilig angew�hlt sein."; }
                if (_AfterEdit_QuickSortRemoveDouble) { return "Sortierung kann nur bei mehrzeiligen Feldern erfolgen."; }
            }
            if (_SpellCheckingEnabled && !_Format.SpellCheckingPossible()) { return "Rechtschreibpr�fung bei diesem Format nicht m�glich."; }
            if (_EditTrotzSperreErlaubt && !_TextBearbeitungErlaubt && !_DropdownBearbeitungErlaubt) { return "Wenn die Zeilensperre ignoriert werden soll, muss eine Bearbeitungsmethode definiert sein."; }
            var TMP_EditDialog = UserEditDialogTypeInTable(_Format, false, true, _MultiLine);
            if (_TextBearbeitungErlaubt) {
                if (TMP_EditDialog == enEditTypeTable.Dropdown_Single) { return "Format unterst�tzt nur Dropdown-Men�."; }
                if (TMP_EditDialog == enEditTypeTable.None) { return "Format unterst�tzt keine Standard-Bearbeitung."; }
            } else {
                if (_VorschlagsColumn > -1) { return "'Vorschlags-Text-Spalte' nur bei Texteingabe m�glich."; }
                if (!string.IsNullOrEmpty(_AllowedChars)) { return "'Erlaubte Zeichen' nur bei Texteingabe n�tig."; }
            }
            if (_DropdownBearbeitungErlaubt) {
                if (_SpellCheckingEnabled) { return "Entweder Dropdownmen� oder Rechtschreibpr�fung."; }
                if (TMP_EditDialog == enEditTypeTable.None) { return "Format unterst�tzt keine Auswahlmen�-Bearbeitung."; }
            }
            if (!_DropdownBearbeitungErlaubt && !_TextBearbeitungErlaubt) {
                if (PermissionGroups_ChangeCell.Count > 0) { return "Bearbeitungsberechtigungen entfernen, wenn keine Bearbeitung erlaubt ist."; }
            }
            if (!string.IsNullOrEmpty(_CellInitValue)) {
                if (IsFirst()) { return "Die erste Spalte darf keinen InitialWert haben."; }
                if (_VorschlagsColumn > -1) { return "InitialWert und Vorschlagspalten-Initial-Text gemeinsam nicht m�glich"; }
            }
            foreach (var thisS in PermissionGroups_ChangeCell) {
                if (thisS.Contains("|")) { return "Unerlaubtes Zeichen bei den Gruppen, die eine Zelle bearbeiten d�rfen."; }
                if (thisS.ToUpper() == "#ADMINISTRATOR") { return "'#Administrator' bei den Bearbeitern entfernen."; }
            }
            if (_DropdownBearbeitungErlaubt || TMP_EditDialog == enEditTypeTable.Dropdown_Single) {
                if (_Format is not enDataFormat.Bit and not enDataFormat.Columns_f�r_LinkedCellDropdown and not enDataFormat.Values_f�r_LinkedCellDropdown) {
                    if (!_DropdownWerteAndererZellenAnzeigen && DropDownItems.Count == 0) { return "Keine Dropdown-Items vorhanden bzw. Alles hinzuf�gen nicht angew�hlt."; }
                }
            } else {
                if (_DropdownWerteAndererZellenAnzeigen) { return "Dropdownmenu nicht ausgew�hlt, 'alles hinzuf�gen' pr�fen."; }
                if (_DropdownAllesAbw�hlenErlaubt) { return "Dropdownmenu nicht ausgew�hlt, 'alles abw�hlen' pr�fen."; }
                if (DropDownItems.Count > 0) { return "Dropdownmenu nicht ausgew�hlt, Dropdown-Items vorhanden."; }
            }
            if (_DropdownWerteAndererZellenAnzeigen && !_Format.DropdownItemsOfOtherCellsAllowed()) { return "'Dropdownmenu alles hinzuf�gen' bei diesem Format nicht erlaubt."; }
            if (_DropdownAllesAbw�hlenErlaubt && !_Format.DropdownUnselectAllAllowed()) { return "'Dropdownmenu alles abw�hlen' bei diesem Format nicht erlaubt."; }
            if (DropDownItems.Count > 0 && !_Format.DropdownItemsAllowed()) { return "Manuelle 'Dropdow-Items' bei diesem Format nicht erlaubt."; }
            if (_BildTextVerhalten != enBildTextVerhalten.Nur_Text) {
                if (_Format is enDataFormat.Datum_und_Uhrzeit or
                               enDataFormat.Ganzzahl or
                               enDataFormat.Gleitkommazahl or
                               enDataFormat.Text or
                               enDataFormat.Text_mit_Formatierung) {
                    // Performance-Teschnische Gr�nde
                    _BildTextVerhalten = enBildTextVerhalten.Nur_Text;
                    //return "Bei diesem Format muss das Bild/Text-Verhalten 'Nur Text' sein.";
                }
            } else {
                if (_Format is enDataFormat.BildCode or enDataFormat.FarbeInteger) {
                    return "Bei diesem Format darf das Bild/Text-Verhalten nicht 'Nur Text' sein.";
                }
            }
            if (!string.IsNullOrEmpty(_Suffix)) {
                if (_MultiLine) { return "Einheiten und Mehrzeilig darf nicht kombiniert werden."; }
            }
            if (_AfterEdit_Runden > 6) { return "Beim Runden maximal 6 Nachkommastellen m�glich"; }
            if (_FilterOptions == enFilterOptions.None) {
                if (!string.IsNullOrEmpty(_AutoFilterJoker)) { return "Wenn kein Autofilter erlaubt ist, immer anzuzeigende Werte entfernen"; }
            }
            if (string.IsNullOrEmpty(_LinkedKeyKennung) && _Format.NeedLinkedKeyKennung()) { return "Spaltenkennung f�r verlinkte Datenbanken fehlt."; }
            if (OpticalReplace.Count > 0) {
                if (_Format is not enDataFormat.Text and
                    not enDataFormat.Columns_f�r_LinkedCellDropdown and
                    not enDataFormat.BildCode and
                    not enDataFormat.Ganzzahl and
                    not enDataFormat.Gleitkommazahl and
                    not enDataFormat.RelationText) { return "Format unterst�tzt keine Ersetzungen."; }
                if (_FilterOptions.HasFlag(enFilterOptions.ExtendedFilterEnabled)) { return "Entweder 'Ersetzungen' oder 'erweiternden Autofilter'"; }
                if (!string.IsNullOrEmpty(_AutoFilterJoker)) { return "Entweder 'Ersetzungen' oder 'Autofilter Joker'"; }
            }
            if (_KeyColumnKey > -1) {
                if (!string.IsNullOrEmpty(I_Am_A_Key_For_Other_Column)) { return "Eine Schl�sselspalte darf selbst keine Verkn�pfung zu einer anderen Spalte haben: " + I_Am_A_Key_For_Other_Column; }
                var c = Database.Column.SearchByKey(_KeyColumnKey);
                if (c == null) { return "Die verkn�pfte Schl�sselspalte existiert nicht."; }
            }
            if (IsFirst()) {
                if (_KeyColumnKey > -1) { return "Die (intern) erste Spalte darf keine Verkn�pfung zu einer andern Schl�sselspalte haben."; }
            }
            if (_Format != enDataFormat.LinkedCell) {
                if (_LinkedCell_RowKey > -1) { return "Nur verlinkte Zellen k�nnen Daten �ber verlinkte Zellen enthalten."; }
                if (_LinkedCell_ColumnKey > -1) { return "Nur verlinkte Zellen k�nnen Daten �ber verlinkte Zellen enthalten."; }
                if (_LinkedCell_ColumnValueFoundIn > -1) { return "Nur verlinkte Zellen k�nnen Daten �ber verlinkte Zellen enthalten."; }
            }
            return string.Empty;
        }

        public bool ExportableTextformatForLayout() => _Format.ExportableForLayout();

        public List<string> GetUcaseNamesSortedByLenght() {
            if (_UcaseNamesSortedByLenght != null) { return _UcaseNamesSortedByLenght; }
            var tmp = Contents(null, null);
            for (var Z = 0; Z < tmp.Count; Z++) {
                tmp[Z] = tmp[Z].Length.ToString(Constants.Format_Integer10) + tmp[Z].ToUpper();
            }
            tmp.Sort();
            for (var Z = 0; Z < tmp.Count; Z++) {
                tmp[Z] = tmp[Z].Substring(10);
            }
            _UcaseNamesSortedByLenght = tmp;
            return _UcaseNamesSortedByLenght;
        }

        //public void Parse(string ToParse) {
        //    Develop.DebugPrint(enFehlerArt.Fehler, "Kann nur �ber die Datenbank geparsed werden.");
        //}
        //public object Clone()
        //{
        //    if (!IsOk())
        //    {
        //        Develop.DebugPrint(enFehlerArt.Fehler, "Quell-Spalte fehlerhaft:\r\nQuelle: " + Name + "\r\nFehler: " + ErrorReason());
        //    }
        //    return new ColumnItem(this, false);
        //}
        public void GetUniques(List<RowItem> rows, out List<string> Einzigartig, out List<string> NichtEinzigartig) {
            Einzigartig = new List<string>();
            NichtEinzigartig = new List<string>();
            foreach (var ThisRow in rows) {
                var TMP = MultiLine ? ThisRow.CellGetList(this) : new List<string> { ThisRow.CellGetString(this) };
                foreach (var ThisString in TMP) {
                    if (Einzigartig.Contains(ThisString)) {
                        NichtEinzigartig.AddIfNotExists(ThisString);
                    } else {
                        Einzigartig.AddIfNotExists(ThisString);
                    }
                }
            }
            Einzigartig.RemoveString(NichtEinzigartig, false);
        }

        /// <summary>
        /// F�llt die Ersetzungen mittels eines �bergebenen Enums aus.
        /// </summary>
        /// <param name="t">Beispiel: GetType(enDesign)</param>
        /// <param name="ZumDropdownHinzuAb">Erster Wert der Enumeration, der Hinzugef�gt werden soll. Inklusive deses Wertes</param>
        /// <param name="ZumDropdownHinzuBis">Letzter Wert der Enumeration, der nicht mehr hinzugef�gt wird, also exklusives diese Wertes</param>
        public void GetValuesFromEnum(Type t, int ZumDropdownHinzuAb, int ZumDropdownHinzuBis) {
            List<string> NewReplacer = new();
            List<string> NewAuswahl = new();
            var items = Enum.GetValues(t);
            foreach (var thisItem in items) {
                var te = Enum.GetName(t, thisItem);
                var th = (int)thisItem;
                if (!string.IsNullOrEmpty(te)) {
                    NewReplacer.Add(th.ToString() + "|" + te);
                    if (th >= ZumDropdownHinzuAb && th < ZumDropdownHinzuBis) {
                        NewAuswahl.Add(th.ToString());
                    }
                }
            }
            NewReplacer.Reverse();
            if (OpticalReplace.IsDifferentTo(NewReplacer)) {
                OpticalReplace.Clear();
                OpticalReplace.AddRange(NewReplacer);
            }
            if (DropDownItems.IsDifferentTo(NewAuswahl)) {
                DropDownItems.Clear();
                DropDownItems.AddRange(NewAuswahl);
            }
        }

        public int Index() => Database.Column.IndexOf(this);

        /// <summary>
        /// Der Invalidate, der am meisten invalidiert: Alle tempor�ren Variablen und auch jede Zell-Gr��e der Spalte.
        /// </summary>
        public void Invalidate_ColumAndContent() {
            TMP_CaptionText_Size = new SizeF(-1, -1);
            Invalidate_TmpColumnContentWidth();
            Invalidate_TmpVariables();
            foreach (var ThisRow in Database.Row) {
                if (ThisRow != null) { CellCollection.Invalidate_CellContentSize(this, ThisRow); }
            }
            Database.OnViewChanged();
        }

        public bool IsFirst() => Convert.ToBoolean(Database.Column[0] == this);

        public bool IsOk() => string.IsNullOrEmpty(ErrorReason());

        public Database LinkedDatabase() {
            if (_TMP_LinkedDatabase != null) { return _TMP_LinkedDatabase; }
            if (string.IsNullOrEmpty(_LinkedDatabaseFile)) { return null; }

            TMP_LinkedDatabase = _LinkedDatabaseFile.Contains(@"\")
                ? Database.GetByFilename(_LinkedDatabaseFile, true)
                : Database.GetByFilename(Database.Filename.FilePath() + _LinkedDatabaseFile, true);

            if (_TMP_LinkedDatabase != null) { _TMP_LinkedDatabase.UserGroup = Database.UserGroup; }
            return _TMP_LinkedDatabase;
        }

        public ColumnItem Next() {
            var ColumnCount = Index();
            do {
                ColumnCount++;
                if (ColumnCount >= Database.Column.Count) { return null; }
                if (Database.Column[ColumnCount] != null) { return Database.Column[ColumnCount]; }
            } while (true);
        }

        public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

        public ColumnItem Previous() {
            var ColumnCount = Index();
            do {
                ColumnCount--;
                if (ColumnCount < 0) { return null; }
                if (Database.Column[ColumnCount] != null) { return Database.Column[ColumnCount]; }
            } while (true);
        }

        //public bool ZellenZusammenfassen
        //{
        //    get
        //    {
        //        return _ZellenZusammenfassen;
        //    }
        //    set
        //    {
        //        if (_ZellenZusammenfassen == value) { return; }
        //        Database.AddPending(enDatabaseDataType.co_ZellenZusammenfassen, this, _ZellenZusammenfassen.ToPlusMinus(), value.ToPlusMinus(), true);
        //        OnChanged();
        //    }
        //}
        public string QuickInfoText(string AdditionalText) {
            var T = string.Empty;
            if (!string.IsNullOrEmpty(_QuickInfo)) { T += _QuickInfo; }
            if (Database.IsAdministrator() && !string.IsNullOrEmpty(_AdminInfo)) { T = T + "<br><br><b><u>Administrator-Info:</b></u><br>" + _AdminInfo; }
            if (Database.IsAdministrator() && Tags.Count > 0) { T = T + "<br><br><b><u>Spalten-Tags:</b></u><br>" + Tags.JoinWith("<br>"); }
            T = T.Trim();
            T = T.Trim("<br>");
            T = T.Trim();
            if (!string.IsNullOrEmpty(T) && !string.IsNullOrEmpty(AdditionalText)) {
                T = "<b><u>" + AdditionalText + "</b></u><br><br>" + T;
            }
            return T;
        }

        public string ReadableText() {
            var ret = _Caption;
            foreach (var ThisColumnItem in Database.Column) {
                if (ThisColumnItem != null) {
                    if (ThisColumnItem != this && ThisColumnItem.Caption.ToUpper() == _Caption.ToUpper()) {
                        var done = false;
                        if (!string.IsNullOrEmpty(_Ueberschrift3)) {
                            ret = _Ueberschrift3 + "/" + ret;
                            done = true;
                        }
                        if (!string.IsNullOrEmpty(_Ueberschrift2)) {
                            ret = _Ueberschrift2 + "/" + ret;
                            done = true;
                        }
                        if (!string.IsNullOrEmpty(_Ueberschrift1)) {
                            ret = _Ueberschrift1 + "/" + ret;
                            done = true;
                        }
                        if (!done) {
                            ret = _Name; //_Caption + " (" + _Name + ")";
                        }
                        break;
                    }
                }
            }
            ret = ret.Replace("\n", "\r").Replace("\r\r", "\r");
            var i = ret.IndexOf("-\r");
            if (i > 0 && i < ret.Length - 3) {
                var tzei = ret.Substring(i + 2, 1);
                if (tzei.ToLower() == tzei) {
                    ret = ret.Substring(0, i) + ret.Substring(i + 2);
                }
            }
            return ret.Replace("\r", " ").Replace("  ", " ").TrimEnd(":");
        }

        public void ResetSystemToDefault(bool SetAll) {
            if (string.IsNullOrEmpty(_Identifier)) { return; }
            //if (SetAll && !IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Ausserhalb des parsens!"); }
            // ACHTUNG: Die SetAll Befehle OHNE _, die m�ssen geloggt werden.
            if (SetAll) {
                LineLeft = enColumnLineStyle.D�nn;
                LineRight = enColumnLineStyle.Ohne;
                ForeColor = Color.FromArgb(0, 0, 0);
                //CaptionBitmap = null;
            }
            switch (_Identifier) {
                case "System: Creator":
                    _Name = "SYS_Creator";
                    _Format = enDataFormat.Text;
                    if (SetAll) {
                        Caption = "Ersteller";
                        DropdownBearbeitungErlaubt = true;
                        DropdownWerteAndererZellenAnzeigen = true;
                        SpellCheckingEnabled = false;
                        ForeColor = Color.FromArgb(0, 0, 128);
                        BackColor = Color.FromArgb(185, 186, 255);
                    }
                    break;

                case "System: Changer":
                    _Name = "SYS_Changer";
                    _Format = enDataFormat.Text;
                    _SpellCheckingEnabled = false;
                    _TextBearbeitungErlaubt = false;
                    _DropdownBearbeitungErlaubt = false;
                    _ShowUndo = false;
                    PermissionGroups_ChangeCell.Clear();
                    if (SetAll) {
                        Caption = "�nderer";
                        ForeColor = Color.FromArgb(0, 128, 0);
                        BackColor = Color.FromArgb(185, 255, 185);
                    }
                    break;

                case "System: Chapter":
                    _Name = "SYS_Chapter";
                    _Format = enDataFormat.Text;
                    if (SetAll) {
                        Caption = "Kapitel";
                        ForeColor = Color.FromArgb(0, 0, 0);
                        BackColor = Color.FromArgb(255, 255, 150);
                        LineLeft = enColumnLineStyle.Dick;
                        if (_MultiLine) { _ShowMultiLineInOneLine = true; }
                    }
                    break;

                case "System: Date Created":
                    _Name = "SYS_CreateDate";
                    _SpellCheckingEnabled = false;
                    _Format = enDataFormat.Datum_und_Uhrzeit;
                    if (SetAll) {
                        Caption = "Erstell-Datum";
                        ForeColor = Color.FromArgb(0, 0, 128);
                        BackColor = Color.FromArgb(185, 185, 255);
                        LineLeft = enColumnLineStyle.Dick;
                    }
                    break;

                case "System: Date Changed":
                    _Name = "SYS_ChangeDate";
                    _SpellCheckingEnabled = false;
                    _ShowUndo = false;
                    _Format = enDataFormat.Datum_und_Uhrzeit;
                    _TextBearbeitungErlaubt = false;
                    _SpellCheckingEnabled = false;
                    _DropdownBearbeitungErlaubt = false;
                    PermissionGroups_ChangeCell.Clear();
                    if (SetAll) {
                        Caption = "�nder-Datum";
                        ForeColor = Color.FromArgb(0, 128, 0);
                        BackColor = Color.FromArgb(185, 255, 185);
                        LineLeft = enColumnLineStyle.Dick;
                    }
                    break;

                case "System: Correct":
                    _Name = "SYS_Correct";
                    _Caption = "Fehlerfrei";
                    _SpellCheckingEnabled = false;
                    _Format = enDataFormat.Bit;
                    //_AutoFilterErweitertErlaubt = false;
                    _AutoFilterJoker = string.Empty;
                    //_AutofilterTextFilterErlaubt = false;
                    _IgnoreAtRowFilter = true;
                    _FilterOptions = enFilterOptions.Enabled;
                    if (SetAll) {
                        ForeColor = Color.FromArgb(128, 0, 0);
                        BackColor = Color.FromArgb(255, 185, 185);
                        LineLeft = enColumnLineStyle.Dick;
                    }
                    break;

                case "System: Locked":
                    _Name = "SYS_Locked";
                    _SpellCheckingEnabled = false;
                    _Format = enDataFormat.Bit;
                    _FilterOptions = enFilterOptions.Enabled;
                    //_AutoFilterErweitertErlaubt = false;
                    _AutoFilterJoker = string.Empty;
                    //_AutofilterTextFilterErlaubt = false;
                    _IgnoreAtRowFilter = true;
                    if (_TextBearbeitungErlaubt || _DropdownBearbeitungErlaubt) {
                        _QuickInfo = "Eine abgeschlossene Zeile kann<br>nicht mehr bearbeitet werden.";
                        _TextBearbeitungErlaubt = false;
                        _DropdownBearbeitungErlaubt = true;
                        _EditTrotzSperreErlaubt = true;
                    }
                    if (SetAll) {
                        Caption = "Abgeschlossen";
                        ForeColor = Color.FromArgb(128, 0, 0);
                        BackColor = Color.FromArgb(255, 185, 185);
                    }
                    break;

                case "System: State":
                    _Name = "SYS_RowState";
                    _Caption = "veraltet und kann gel�scht werden: Zeilenstand";
                    _Identifier = "";
                    break;

                case "System: ID":
                    _Name = "SYS_ID";
                    _Caption = "veraltet und kann gel�scht werden: Zeilen-ID";
                    _Identifier = "";
                    break;

                case "System: Last Used Layout":
                    _Name = "SYS_Layout";
                    _Caption = "veraltet und kann gel�scht werden:  Letztes Layout";
                    _Identifier = "";
                    break;

                default:
                    Develop.DebugPrint("Unbekannte Kennung: " + _Identifier);
                    break;
            }
        }

        public string SimplyFile(string fullFileName) {
            if (_Format != enDataFormat.Link_To_Filesystem) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Nur bei Link_To_Filesystem erlaubt!");
            }
            var tmpfile = fullFileName.FileNameWithoutSuffix();
            if (tmpfile.ToLower() == fullFileName.ToLower()) { return tmpfile; }
            if (BestFile(tmpfile, false).ToLower() == fullFileName.ToLower()) { return tmpfile; }
            tmpfile = fullFileName.FileNameWithSuffix();
            return tmpfile.ToLower() == fullFileName.ToLower()
                ? tmpfile
                : BestFile(tmpfile, false).ToLower() == fullFileName.ToLower() ? tmpfile : fullFileName;
        }

        public double? Summe(FilterCollection Filter) {
            double summ = 0;
            foreach (var thisrow in Database.Row) {
                if (thisrow != null && thisrow.MatchesTo(Filter)) {
                    if (!thisrow.CellIsNullOrEmpty(this)) {
                        if (!thisrow.CellGetString(this).IsDouble()) { return null; }
                        summ += thisrow.CellGetDouble(this);
                    }
                }
            }
            return summ;
        }

        public double? Summe(List<RowItem> sort) {
            double summ = 0;
            foreach (var thisrow in sort) {
                if (thisrow != null) {
                    if (!thisrow.CellIsNullOrEmpty(this)) {
                        if (!thisrow.CellGetString(this).IsDouble()) { return null; }
                        summ += thisrow.CellGetDouble(this);
                    }
                }
            }
            return summ;
        }

        public QuickImage SymbolForReadableText() => this == Database.Column.SysRowChanger
? QuickImage.Get(enImageCode.Person)
: this == Database.Column.SysRowCreator
? QuickImage.Get(enImageCode.Person)
: _Format switch {
    enDataFormat.Link_To_Filesystem => QuickImage.Get(enImageCode.Datei, 16),
    enDataFormat.RelationText => QuickImage.Get(enImageCode.Herz, 16),
    enDataFormat.Datum_und_Uhrzeit => QuickImage.Get(enImageCode.Uhr, 16),
    enDataFormat.Bit => QuickImage.Get(enImageCode.H�kchen, 16),
    enDataFormat.FarbeInteger => QuickImage.Get(enImageCode.Pinsel, 16),
    enDataFormat.Ganzzahl => QuickImage.Get(enImageCode.Ganzzahl, 16),
    enDataFormat.Gleitkommazahl => QuickImage.Get(enImageCode.Gleitkommazahl, 16),
    enDataFormat.BildCode => QuickImage.Get(enImageCode.Smiley, 16),
    enDataFormat.LinkedCell => QuickImage.Get(enImageCode.Fernglas, 16),
    enDataFormat.Columns_f�r_LinkedCellDropdown => QuickImage.Get(enImageCode.Fernglas, 16, "FF0000", ""),
    enDataFormat.Values_f�r_LinkedCellDropdown => QuickImage.Get(enImageCode.Fernglas, 16, "00FF00", ""),
    enDataFormat.Button => QuickImage.Get(enImageCode.Kugel, 16),
    _ => _Format.TextboxEditPossible()
    ? _MultiLine ? QuickImage.Get(enImageCode.Textfeld, 16, "FF0000", "") : QuickImage.Get(enImageCode.Textfeld)
    : QuickImage.Get("Pfeil_Unten_Scrollbar|14|||||0"),
};

        /// <summary>
        ///
        /// </summary>
        /// <param name="Nummer">Muss 1, 2 oder 3 sein</param>
        /// <returns></returns>
        public string Ueberschrift(int Nummer) {
            switch (Nummer) {
                case 0:
                    return _Ueberschrift1;

                case 1:
                    return _Ueberschrift2;

                case 2:
                    return _Ueberschrift3;

                default:
                    Develop.DebugPrint(enFehlerArt.Warnung, "Nummer " + Nummer + " nicht erlaubt.");
                    return string.Empty;
            }
        }

        public bool UserEditDialogTypeInFormula(enEditTypeFormula EditType_To_Check) {
            switch (_Format) {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.BildCode:
                case enDataFormat.RelationText:
                    if (_TextBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Textfeld) { return true; } // Textfeld immer erlauben auch wenn beide Bearbeitungen nicht erlaubt sind, um die Anzeieg zu gew�hrleisten.
                    if (_MultiLine && EditType_To_Check == enEditTypeFormula.Textfeld_mit_Auswahlknopf) { return false; }
                    if (_DropdownBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
                    if (_DropdownBearbeitungErlaubt && _DropdownWerteAndererZellenAnzeigen && EditType_To_Check == enEditTypeFormula.SwapListBox) { return true; }
                    //if (_MultiLine && _DropdownBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Listbox_3_Zeilen) { return true; }
                    if (_MultiLine && _DropdownBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Listbox) { return true; }
                    if (EditType_To_Check == enEditTypeFormula.nur_als_Text_anzeigen) { return true; }
                    return false;

                case enDataFormat.LinkedCell:
                    if (EditType_To_Check == enEditTypeFormula.None) { return true; }
                    //if (EditType_To_Check != enEditTypeFormula.Textfeld &&
                    //    EditType_To_Check != enEditTypeFormula.nur_als_Text_anzeigen) { return false; }
                    if (Database.IsParsing) { return true; }
                    if (LinkedDatabase() == null) { return false; }
                    if (_LinkedCell_ColumnKey < 0) { return false; }
                    var col = LinkedDatabase().Column.SearchByKey(_LinkedCell_ColumnKey);
                    if (col == null) { return false; }
                    return col.UserEditDialogTypeInFormula(EditType_To_Check);

                case enDataFormat.Columns_f�r_LinkedCellDropdown:
                case enDataFormat.Values_f�r_LinkedCellDropdown:
                    if (EditType_To_Check == enEditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
                    return false;

                case enDataFormat.Bit:
                    if (_MultiLine) { return false; }
                    if (EditType_To_Check == enEditTypeFormula.Ja_Nein_Knopf) {
                        return !_DropdownWerteAndererZellenAnzeigen && DropDownItems.Count <= 0;
                    }
                    if (EditType_To_Check == enEditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
                    return false;

                case enDataFormat.Link_To_Filesystem:
                    if (_MultiLine) {
                        //if (EditType_To_Check == enEditType.Listbox) { return true; }
                        if (EditType_To_Check == enEditTypeFormula.Gallery) { return true; }
                    } else {
                        if (EditType_To_Check == enEditTypeFormula.EasyPic) { return true; }
                    }
                    return false;
                //case enDataFormat.Relation:
                //    switch (EditType_To_Check)
                //    {
                //        case enEditTypeFormula.Listbox_1_Zeile:
                //        case enEditTypeFormula.Listbox_3_Zeilen:
                //        case enEditTypeFormula.Listbox_6_Zeilen:
                //            return true;
                //        default:
                //            return false;
                //    }
                case enDataFormat.FarbeInteger:
                    if (EditType_To_Check == enEditTypeFormula.Farb_Auswahl_Dialog) { return true; }
                    return false;

                case enDataFormat.Schrift:
                    if (EditType_To_Check == enEditTypeFormula.Font_AuswahlDialog) { return true; }
                    return false;

                case enDataFormat.Button:
                    //if (EditType_To_Check == enEditTypeFormula.Button) { return true; }
                    return false;

                default:
                    Develop.DebugPrint(_Format);
                    return false;
            }
        }

        public string Verwendung() {
            var t = "<b><u>Verwendung von " + ReadableText() + "</b></u><br>";
            if (!string.IsNullOrEmpty(_Identifier)) {
                t += " - Systemspalte<br>";
            }
            return t + Database.Column_UsedIn(this);
        }

        internal void CheckFormulaEditType() {
            if (UserEditDialogTypeInFormula(_EditType)) { return; }// Alles OK!
            for (var z = 0; z <= 999; z++) {
                var w = (enEditTypeFormula)z;
                if (w.ToString() != z.ToString()) {
                    if (UserEditDialogTypeInFormula(w)) {
                        _EditType = w;
                        return;
                    }
                }
            }
            _EditType = enEditTypeFormula.None;
        }

        internal void CheckIfIAmAKeyColumn() {
            I_Am_A_Key_For_Other_Column = string.Empty;
            foreach (var ThisColumn in Database.Column) {
                if (ThisColumn.KeyColumnKey == Key) { I_Am_A_Key_For_Other_Column = "Spalte " + ThisColumn.ReadableText() + " verweist auf diese Spalte"; } // Werte Gleichhalten
                if (ThisColumn.LinkedCell_RowKey == Key) { I_Am_A_Key_For_Other_Column = "Spalte " + ThisColumn.ReadableText() + " verweist auf diese Spalte"; } // LinkdeCells pflegen
                if (ThisColumn.LinkedCell_ColumnValueFoundIn == Key) { I_Am_A_Key_For_Other_Column = "Spalte " + ThisColumn.ReadableText() + " verweist auf diese Spalte"; } // LinkdeCells pflegen
            }
            if (_Format == enDataFormat.Columns_f�r_LinkedCellDropdown) { I_Am_A_Key_For_Other_Column = "Die Spalte selbst durch das Format"; }
        }

        /// <summary>
        /// Wenn sich ein Zelleninhalt ver�ndert hat, muss die Spalte neu berechnet werden.
        /// </summary>
        internal void Invalidate_TmpColumnContentWidth() => TMP_ColumnContentWidth = null;

        internal void Invalidate_TmpVariables() {
            TMP_CaptionText_Size = new SizeF(-1, -1);
            TMP_CaptionBitmap = null;
            if (_TMP_LinkedDatabase != null) {
                _TMP_LinkedDatabase.RowKeyChanged -= _TMP_LinkedDatabase_RowKeyChanged;
                _TMP_LinkedDatabase.ColumnKeyChanged -= _TMP_LinkedDatabase_ColumnKeyChanged;
                _TMP_LinkedDatabase.ConnectedControlsStopAllWorking -= _TMP_LinkedDatabase_ConnectedControlsStopAllWorking;
                _TMP_LinkedDatabase.Cell.CellValueChanged -= _TMP_LinkedDatabase_Cell_CellValueChanged;
                _TMP_LinkedDatabase.Disposing -= _TMP_LinkedDatabase_Disposing;
                _TMP_LinkedDatabase = null;
            }
            TMP_ColumnContentWidth = null;
        }

        internal string Load(enDatabaseDataType Art, string Wert) {
            switch (Art) {
                case enDatabaseDataType.co_Name:
                    _Name = Wert;
                    Invalidate_TmpVariables();
                    break;

                case enDatabaseDataType.co_Caption:
                    _Caption = Wert;
                    break;

                case enDatabaseDataType.co_Format:
                    _Format = (enDataFormat)int.Parse(Wert);
                    if (Wert == "21") { _Format = enDataFormat.Text; }
                    break;

                case enDatabaseDataType.co_ForeColor:
                    _ForeColor = Color.FromArgb(int.Parse(Wert));
                    break;

                case enDatabaseDataType.co_BackColor:
                    _BackColor = Color.FromArgb(int.Parse(Wert));
                    break;

                case enDatabaseDataType.co_LineLeft:
                    _LineLeft = (enColumnLineStyle)int.Parse(Wert);
                    break;

                case enDatabaseDataType.co_LinieRight:
                    _LineRight = (enColumnLineStyle)int.Parse(Wert);
                    break;

                case enDatabaseDataType.co_QuickInfo:
                    _QuickInfo = Wert;
                    break;
                //case enDatabaseDataType.co_Intelligenter_Multifilter: _Intelligenter_Multifilter = Wert; break;
                case enDatabaseDataType.co_DauerFilterPos:
                    _DauerFilterPos = Extensions.PointParse(Wert);
                    break;

                case enDatabaseDataType.co_Ueberschrift1:
                    _Ueberschrift1 = Wert;
                    break;

                case enDatabaseDataType.co_Ueberschrift2:
                    _Ueberschrift2 = Wert;
                    break;

                case enDatabaseDataType.co_Ueberschrift3:
                    _Ueberschrift3 = Wert;
                    break;

                case enDatabaseDataType.co_CaptionBitmap:
                    if (!string.IsNullOrEmpty(Wert)) {
                        _CaptionBitmapTXT = "co_" + _Name;
                    }
                    break;

                case enDatabaseDataType.co_Identifier:
                    _Identifier = Wert;
                    ResetSystemToDefault(false);
                    Database.Column.GetSystems();
                    break;

                case enDatabaseDataType.co_EditType:
                    _EditType = (enEditTypeFormula)int.Parse(Wert);
                    break;

                case enDatabaseDataType.co_MultiLine:
                    _MultiLine = Wert.FromPlusMinus();
                    break;

                case enDatabaseDataType.co_DropDownItems:
                    DropDownItems.SplitByCR_QuickSortAndRemoveDouble(Wert);
                    break;

                case enDatabaseDataType.co_OpticalReplace:
                    OpticalReplace.SplitByCR(Wert);
                    break;

                case enDatabaseDataType.co_AfterEdit_AutoReplace:
                    AfterEdit_AutoReplace.SplitByCR(Wert);
                    break;

                case enDatabaseDataType.co_Regex:
                    Regex.SplitByCR(Wert);
                    break;

                case enDatabaseDataType.co_Tags:
                    Tags.SplitByCR(Wert);
                    break;

                case enDatabaseDataType.co_AutoFilterJoker:
                    _AutoFilterJoker = Wert;
                    break;

                case enDatabaseDataType.co_PermissionGroups_ChangeCell:
                    PermissionGroups_ChangeCell.SplitByCR_QuickSortAndRemoveDouble(Wert);
                    break;

                case enDatabaseDataType.co_AllowedChars:
                    _AllowedChars = Wert;
                    break;

                case enDatabaseDataType.co_FilterOptions:
                    _FilterOptions = (enFilterOptions)int.Parse(Wert);
                    break;

                case enDatabaseDataType.co_AutoFilterErlaubt_alt:
                    _FilterOptions = enFilterOptions.None;
                    if (Wert.FromPlusMinus()) { _FilterOptions |= enFilterOptions.Enabled; }
                    break;

                case enDatabaseDataType.co_AutoFilterTextFilterErlaubt_alt:
                    if (Wert.FromPlusMinus()) { _FilterOptions |= enFilterOptions.TextFilterEnabled; }
                    break;

                case enDatabaseDataType.co_AutoFilterErweitertErlaubt_alt:
                    if (Wert.FromPlusMinus()) { _FilterOptions |= enFilterOptions.ExtendedFilterEnabled; }
                    break;

                case enDatabaseDataType.co_BeiZeilenfilterIgnorieren:
                    _IgnoreAtRowFilter = Wert.FromPlusMinus();
                    break;

                case enDatabaseDataType.co_CompactView_alt:
                    break;

                case enDatabaseDataType.co_ShowUndo:
                    _ShowUndo = Wert.FromPlusMinus();
                    break;

                case enDatabaseDataType.co_ShowMultiLineInOneLine:
                    _ShowMultiLineInOneLine = Wert.FromPlusMinus();
                    break;

                case enDatabaseDataType.co_TextBearbeitungErlaubt:
                    _TextBearbeitungErlaubt = Wert.FromPlusMinus();
                    break;

                case enDatabaseDataType.co_DropdownBearbeitungErlaubt:
                    _DropdownBearbeitungErlaubt = Wert.FromPlusMinus();
                    break;

                case enDatabaseDataType.co_SpellCheckingEnabled:
                    _SpellCheckingEnabled = Wert.FromPlusMinus();
                    break;

                case enDatabaseDataType.co_DropdownAllesAbw�hlenErlaubt:
                    _DropdownAllesAbw�hlenErlaubt = Wert.FromPlusMinus();
                    break;

                case enDatabaseDataType.co_DropdownWerteAndererZellenAnzeigen:
                    _DropdownWerteAndererZellenAnzeigen = Wert.FromPlusMinus();
                    break;

                case enDatabaseDataType.co_AfterEdit_QuickSortAndRemoveDouble:
                    _AfterEdit_QuickSortRemoveDouble = Wert.FromPlusMinus();
                    break;

                case enDatabaseDataType.co_AfterEdit_Runden:
                    _AfterEdit_Runden = int.Parse(Wert);
                    break;

                case enDatabaseDataType.co_AfterEdit_DoUcase:
                    _AfterEdit_DoUCase = Wert.FromPlusMinus();
                    break;

                case enDatabaseDataType.co_AfterEdit_AutoCorrect:
                    _AfterEdit_AutoCorrect = Wert.FromPlusMinus();
                    break;

                case enDatabaseDataType.co_SaveContent:
                    _SaveContent = Wert.FromPlusMinus();
                    break;

                case enDatabaseDataType.co_AutoRemove:
                    _AutoRemove = Wert;
                    break;

                case enDatabaseDataType.co_AdminInfo:
                    _AdminInfo = Wert;
                    break;

                case enDatabaseDataType.co_CaptionBitmapTXT:
                    _CaptionBitmapTXT = Wert;
                    break;

                case enDatabaseDataType.co_Suffix:
                    _Suffix = Wert;
                    break;

                case enDatabaseDataType.co_LinkedDatabase:
                    _LinkedDatabaseFile = Wert;
                    break;

                case enDatabaseDataType.co_LinkKeyKennung:
                    _LinkedKeyKennung = Wert;
                    break;

                case enDatabaseDataType.co_BestFile_StandardSuffix:
                    _BestFile_StandardSuffix = Wert;
                    break;

                case enDatabaseDataType.co_BestFile_StandardFolder:
                    _BestFile_StandardFolder = Wert;
                    break;

                case enDatabaseDataType.co_BildCode_ConstantHeight:
                    _BildCode_ConstantHeight = int.Parse(Wert);
                    break;

                case enDatabaseDataType.co_Prefix:
                    _Prefix = Wert;
                    break;

                case enDatabaseDataType.co_BildTextVerhalten:
                    _BildTextVerhalten = (enBildTextVerhalten)int.Parse(Wert);
                    break;

                case enDatabaseDataType.co_EditTrotzSperreErlaubt:
                    _EditTrotzSperreErlaubt = Wert.FromPlusMinus();
                    break;

                case enDatabaseDataType.co_CellInitValue:
                    _CellInitValue = Wert;
                    break;

                case enDatabaseDataType.co_KeyColumnKey:
                    _KeyColumnKey = int.Parse(Wert);
                    break;

                case enDatabaseDataType.co_LinkedCell_RowKey:
                    _LinkedCell_RowKey = int.Parse(Wert);
                    break;

                case enDatabaseDataType.co_LinkedCell_ColumnKey:
                    _LinkedCell_ColumnKey = int.Parse(Wert);
                    break;

                case enDatabaseDataType.co_LinkedCell_ColumnValueFoundIn:
                    _LinkedCell_ColumnValueFoundIn = int.Parse(Wert);
                    break;

                case enDatabaseDataType.co_LinkedCell_ColumnValueAdd:
                    _LinkedCell_ColumnValueAdd = Wert;
                    break;

                case enDatabaseDataType.co_SortMask:
                    _SortMask = Wert;
                    break;
                //case enDatabaseDataType.co_ZellenZusammenfassen: _ZellenZusammenfassen = Wert.FromPlusMinus(); break;
                case enDatabaseDataType.co_DropDownKey:
                    _DropDownKey = int.Parse(Wert);
                    break;

                case enDatabaseDataType.co_VorschlagColumn:
                    _VorschlagsColumn = int.Parse(Wert);
                    break;

                case enDatabaseDataType.co_Align:
                    _Align = (enAlignmentHorizontal)int.Parse(Wert);
                    break;
                //case (enDatabaseDataType)189: break;
                //case (enDatabaseDataType)192: break;
                //case (enDatabaseDataType)193: break;
                default:
                    if (Art.ToString() == ((int)Art).ToString()) {
                        Develop.DebugPrint(enFehlerArt.Info, "Laden von Datentyp '" + Art + "' nicht definiert.<br>Wert: " + Wert + "<br>Datei: " + Database.Filename);
                    } else {
                        return "Interner Fehler: F�r den Datentyp  '" + Art + "'  wurde keine Laderegel definiert.";
                    }
                    break;
            }
            return string.Empty;
        }

        internal string ParsableColumnKey() => ColumnCollection.ParsableColumnKey(this);

        internal void SaveToByteList(ref List<byte> l) {
            Database.SaveToByteList(l, enDatabaseDataType.co_Name, _Name, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Caption, _Caption, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Format, ((int)_Format).ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Ueberschrift1, _Ueberschrift1, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Ueberschrift2, _Ueberschrift2, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Ueberschrift3, _Ueberschrift3, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_MultiLine, _MultiLine.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_CellInitValue, _CellInitValue, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AfterEdit_QuickSortAndRemoveDouble, _AfterEdit_QuickSortRemoveDouble.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AfterEdit_DoUcase, _AfterEdit_DoUCase.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AfterEdit_AutoCorrect, _AfterEdit_AutoCorrect.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AfterEdit_Runden, _AfterEdit_Runden.ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AutoRemove, _AutoRemove, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_SaveContent, _SaveContent.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_FilterOptions, ((int)_FilterOptions).ToString(), Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_AutoFilter_Dauerfilter, ((int)_AutoFilter_Dauerfilter).ToString(), Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_AutoFilterErlaubt, _AutofilterErlaubt.ToPlusMinus(), Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_AutoFilterTextFilterErlaubt, _AutofilterTextFilterErlaubt.ToPlusMinus(), Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_AutoFilterErweitertErlaubt, _AutoFilterErweitertErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AutoFilterJoker, _AutoFilterJoker, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_BeiZeilenfilterIgnorieren, _IgnoreAtRowFilter.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_TextBearbeitungErlaubt, _TextBearbeitungErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_SpellCheckingEnabled, _SpellCheckingEnabled.ToPlusMinus(), Key);
            //  Database.SaveToByteList(l, enDatabaseDataType.co_CompactView, _CompactView.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_ShowMultiLineInOneLine, _ShowMultiLineInOneLine.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_ShowUndo, _ShowUndo.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_ForeColor, _ForeColor.ToArgb().ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_BackColor, _BackColor.ToArgb().ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_LineLeft, ((int)_LineLeft).ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_LinieRight, ((int)_LineRight).ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_DropdownBearbeitungErlaubt, _DropdownBearbeitungErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_DropDownItems, DropDownItems.JoinWithCr(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_OpticalReplace, OpticalReplace.JoinWithCr(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AfterEdit_AutoReplace, AfterEdit_AutoReplace.JoinWithCr(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Regex, Regex.JoinWithCr(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_DropdownAllesAbw�hlenErlaubt, _DropdownAllesAbw�hlenErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_DropdownWerteAndererZellenAnzeigen, _DropdownWerteAndererZellenAnzeigen.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_QuickInfo, _QuickInfo, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AdminInfo, _AdminInfo, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_CaptionBitmapTXT, _CaptionBitmapTXT, Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_CaptionBitmapUTF8, modConverter.BitmapToStringUnicode(_CaptionBitmap, ImageFormat.Png), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AllowedChars, _AllowedChars, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_PermissionGroups_ChangeCell, PermissionGroups_ChangeCell.JoinWithCr(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_EditType, ((int)_EditType).ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Tags, Tags.JoinWithCr(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_EditTrotzSperreErlaubt, _EditTrotzSperreErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Suffix, _Suffix, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_LinkedDatabase, _LinkedDatabaseFile, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_LinkKeyKennung, _LinkedKeyKennung, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_BestFile_StandardFolder, _BestFile_StandardFolder, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_BestFile_StandardSuffix, _BestFile_StandardSuffix, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_BildCode_ConstantHeight, _BildCode_ConstantHeight.ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_BildTextVerhalten, ((int)_BildTextVerhalten).ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Prefix, _Prefix, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_KeyColumnKey, _KeyColumnKey.ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_LinkedCell_RowKey, _LinkedCell_RowKey.ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_LinkedCell_ColumnKey, _LinkedCell_ColumnKey.ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_LinkedCell_ColumnValueFoundIn, _LinkedCell_ColumnValueFoundIn.ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_LinkedCell_ColumnValueAdd, _LinkedCell_ColumnValueAdd, Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_ZellenZusammenfassen, _ZellenZusammenfassen.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_DropDownKey, _DropDownKey.ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_VorschlagColumn, _VorschlagsColumn.ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Align, ((int)_Align).ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_SortMask, _SortMask, Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_Intelligenter_Multifilter, _Intelligenter_Multifilter, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_DauerFilterPos, _DauerFilterPos.ToString(), Key);
            //Kennung UNBEDINGT zum Schluss, damit die Standard-Werte gesetzt werden k�nnen
            Database.SaveToByteList(l, enDatabaseDataType.co_Identifier, _Identifier, Key);
        }

        private void _TMP_LinkedDatabase_Cell_CellValueChanged(object sender, CellEventArgs e) {
            var tKey = CellCollection.KeyOfCell(e.Column, e.Row);
            foreach (var ThisRow in Database.Row) {
                if (Database.Cell.GetStringBehindLinkedValue(this, ThisRow) == tKey) {
                    CellCollection.Invalidate_CellContentSize(this, ThisRow);
                    Invalidate_TmpColumnContentWidth();
                    Database.Cell.OnCellValueChanged(new CellEventArgs(this, ThisRow));
                    ThisRow.DoAutomatic(true, false, 5, "value changed");
                }
            }
        }

        private void _TMP_LinkedDatabase_ColumnKeyChanged(object sender, KeyChangedEventArgs e) {
            Database.BlockReload(false);
            if (_Format != enDataFormat.Columns_f�r_LinkedCellDropdown) {
                var os = e.KeyOld.ToString();
                var ns = e.KeyNew.ToString();
                foreach (var ThisRow in Database.Row) {
                    if (Database.Cell.GetStringBehindLinkedValue(this, ThisRow) == os) {
                        Database.Cell.SetValueBehindLinkedValue(this, ThisRow, ns);
                    }
                }
            }
            if (_Format != enDataFormat.LinkedCell) {
                var os = e.KeyOld.ToString() + "|";
                var ns = e.KeyNew.ToString() + "|";
                foreach (var ThisRow in Database.Row) {
                    var val = Database.Cell.GetStringBehindLinkedValue(this, ThisRow);
                    if (val.StartsWith(os)) {
                        Database.Cell.SetValueBehindLinkedValue(this, ThisRow, val.Replace(os, ns));
                    }
                }
            }
        }

        private void _TMP_LinkedDatabase_ConnectedControlsStopAllWorking(object sender, MultiUserFileStopWorkingEventArgs e) => Database.OnConnectedControlsStopAllWorking(e);

        //public bool IsParsing {
        //    get {
        //        Develop.DebugPrint(enFehlerArt.Fehler, "Kann nur �ber die Datenbank geparsed werden.");
        //        return false;
        //    }
        //}
        private void _TMP_LinkedDatabase_Disposing(object sender, System.EventArgs e) {
            Invalidate_TmpVariables();
            Database.Dispose();
        }

        private void _TMP_LinkedDatabase_RowKeyChanged(object sender, KeyChangedEventArgs e) {
            if (_Format != enDataFormat.LinkedCell) {
                var os = "|" + e.KeyOld.ToString();
                var ns = "|" + e.KeyNew.ToString();
                foreach (var ThisRow in Database.Row) {
                    var val = Database.Cell.GetStringBehindLinkedValue(this, ThisRow);
                    if (val.EndsWith(os)) {
                        Database.Cell.SetValueBehindLinkedValue(this, ThisRow, val.Replace(os, ns));
                    }
                }
            }
        }

        private void AfterEdit_AutoReplace_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(enDatabaseDataType.co_AfterEdit_AutoReplace, Key, AfterEdit_AutoReplace.JoinWithCr(), false);
            OnChanged();
        }

        private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

        private void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                }
                Database.Disposing += Database_Disposing;
                Invalidate_TmpVariables();
                Database = null;
                DropDownItems.Dispose();
                Tags.Dispose();
                PermissionGroups_ChangeCell.Dispose();
                OpticalReplace.Dispose();
                AfterEdit_AutoReplace.Dispose();
                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer �berschreiben
                // TODO: Gro�e Felder auf NULL setzen
                disposedValue = true;
            }
        }

        private void DropDownItems_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(enDatabaseDataType.co_DropDownItems, Key, DropDownItems.JoinWithCr(), false);
            OnChanged();
        }

        private string KleineFehlerCorrect(string TXT) {
            if (string.IsNullOrEmpty(TXT)) { return string.Empty; }
            const char h4 = (char)1004; // H4 = Normaler Text, nach links rutschen
            const char h3 = (char)1003; // �berschrift
            const char h2 = (char)1002; // �berschrift
            const char h1 = (char)1001; // �berschrift
            const char h7 = (char)1007; // bold
            if (_Format == enDataFormat.Text_mit_Formatierung) { TXT = TXT.HTMLSpecialToNormalChar(false); }
            string oTXT;
            do {
                oTXT = TXT;
                if (oTXT.ToLower().Contains(".at")) { break; }
                if (oTXT.ToLower().Contains(".de")) { break; }
                if (oTXT.ToLower().Contains(".com")) { break; }
                if (oTXT.ToLower().Contains("http")) { break; }
                if (oTXT.ToLower().Contains("ftp")) { break; }
                if (oTXT.ToLower().Contains(".xml")) { break; }
                if (oTXT.ToLower().Contains(".doc")) { break; }
                if (oTXT.IsFormat(enDataFormat.Datum_und_Uhrzeit)) { break; }
                TXT = TXT.Replace("\r\n", "\r");
                // 1/2 l Milch
                // 3-5 Stunden
                // 180�C
                // Nach Zahlen KEINE leerzeichen einf�gen. Es gibgt so viele dinge.... 90er Schichtsalat
                TXT = TXT.Insert(" ", ",", "1234567890, \r");
                TXT = TXT.Insert(" ", "!", " !?)\r");
                TXT = TXT.Insert(" ", "?", " !?)\r");
                TXT = TXT.Insert(" ", ".", " 1234567890.!?/)\r");
                TXT = TXT.Insert(" ", ")", " .;!?\r");
                TXT = TXT.Insert(" ", ";", " 1234567890\r");
                TXT = TXT.Insert(" ", ":", "1234567890 \\/\r"); // auch 3:50 Uhr
                // H4= Normaler Text
                TXT = TXT.Replace(" " + h4, h4 + " "); // H4 = Normaler Text, nach links rutschen
                TXT = TXT.Replace("\r" + h4, h4 + "\r");
                // Dei restlichen Hs'
                TXT = TXT.Replace(h3 + " ", " " + h3); // �berschrift, nach Rechts
                TXT = TXT.Replace(h2 + " ", " " + h2); // �berschrift, nach Rechts
                TXT = TXT.Replace(h1 + " ", " " + h1); // �berschrift, nach Rechts
                TXT = TXT.Replace(h7 + " ", " " + h7); // Bold, nach Rechts
                TXT = TXT.Replace(h3 + "\r", "\r" + h3); // �berschrift, nach Rechts
                TXT = TXT.Replace(h2 + "\r", "\r" + h2); // �berschrift, nach Rechts
                TXT = TXT.Replace(h1 + "\r", "\r" + h1); // �berschrift, nach Rechts
                TXT = TXT.Replace(h7 + "\r", "\r" + h7); // Bold, nach Rechts
                TXT = TXT.Replace(h7 + h4.ToString(), h4.ToString());
                TXT = TXT.Replace(h3 + h4.ToString(), h4.ToString());
                TXT = TXT.Replace(h2 + h4.ToString(), h4.ToString());
                TXT = TXT.Replace(h1 + h4.ToString(), h4.ToString());
                TXT = TXT.Replace(h4 + h4.ToString(), h4.ToString());
                TXT = TXT.Replace(" �", "�");
                TXT = TXT.Replace(" .", ".");
                TXT = TXT.Replace(" ,", ",");
                TXT = TXT.Replace(" :", ":");
                TXT = TXT.Replace(" ?", "?");
                TXT = TXT.Replace(" !", "!");
                TXT = TXT.Replace(" )", ")");
                TXT = TXT.Replace("( ", "(");
                TXT = TXT.Replace("/ ", "/");
                TXT = TXT.Replace(" /", "/");
                TXT = TXT.Replace("\r ", "\r");
                TXT = TXT.Replace(" \r", "\r");
                TXT = TXT.Replace("     ", " "); // Wenn das hier nicht da ist, passieren wirklich fehler...
                TXT = TXT.Replace("    ", " ");
                TXT = TXT.Replace("   ", " "); // Wenn das hier nicht da ist, passieren wirklich fehler...
                TXT = TXT.Replace("  ", " ");
                TXT = TXT.Trim(' ');
                TXT = TXT.Trim("\r");
                TXT = TXT.TrimEnd("\t");
            } while (oTXT != TXT);
            if (Format == enDataFormat.Text_mit_Formatierung) {
                TXT = TXT.CreateHtmlCodes(true);
                TXT = TXT.Replace("<br>", "\r");
            }
            return TXT;
        }

        private void OpticalReplacer_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(enDatabaseDataType.co_OpticalReplace, Key, OpticalReplace.JoinWithCr(), false);
            Invalidate_ColumAndContent();
            OnChanged();
        }

        private void PermissionGroups_ChangeCell_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(enDatabaseDataType.co_PermissionGroups_ChangeCell, Key, PermissionGroups_ChangeCell.JoinWithCr(), false);
            OnChanged();
        }

        private void Regex_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(enDatabaseDataType.co_Regex, Key, Regex.JoinWithCr(), false);
            OnChanged();
        }

        private void Tags_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(enDatabaseDataType.co_Tags, Key, Tags.JoinWithCr(), false);
            OnChanged();
        }

        #endregion
    }
}