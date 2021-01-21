#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion


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
using System.Drawing.Imaging;
using System.Linq;
using System.Text.RegularExpressions;
using static BlueBasics.Extensions;
using static BlueBasics.FileOperations;

namespace BlueDatabase {
    public sealed class ColumnItem : IReadableTextWithChanging, IParseable, ICompareKey, ICheckable {
        #region  Variablen-Deklarationen 

        public readonly Database Database;


        private string _Name;
        private bool _MultiLine;
        private string _Caption;
        private Bitmap _CaptionBitmap;
        private string _QuickInfo;
        //private string _Intelligenter_Multifilter;
        private Point _DauerFilterPos;
        private string _Ueberschrift1;
        private string _Ueberschrift2;
        private string _Ueberschrift3;
        private enDataFormat _Format;
        private enEditTypeFormula _EditType;
        private Color _ForeColor;
        private Color _BackColor;
        private enColumnLineStyle _LineLeft;
        private enColumnLineStyle _LineRight;

        private string _AutoFilterJoker;
        private string _Identifier;

        public readonly ListExt<string> DropDownItems = new ListExt<string>();
        public readonly ListExt<string> Tags = new ListExt<string>();
        public readonly ListExt<string> PermissionGroups_ChangeCell = new ListExt<string>();
        public readonly ListExt<string> OpticalReplace = new ListExt<string>();
        public readonly ListExt<string> AfterEdit_AutoReplace = new ListExt<string>();

        private string _AllowedChars;
        private string _AdminInfo;
        private enFilterOptions _FilterOptions;
        //private bool _AutofilterErlaubt;
        //private bool _AutofilterTextFilterErlaubt;
        //private bool _AutoFilterErweitertErlaubt;
        private bool _IgnoreAtRowFilter;
        //private bool _CompactView;
        private bool _ShowMultiLineInOneLine;
        private bool _DropdownBearbeitungErlaubt;
        private bool _DropdownAllesAbwählenErlaubt;
        private bool _TextBearbeitungErlaubt;
        private bool _DropdownWerteAndererZellenAnzeigen;

        private bool _EditTrotzSperreErlaubt;
        private bool _SpellCheckingEnabled;
        private string _Suffix;
        private bool _ShowUndo;


        private string _LinkedKeyKennung;
        private string _LinkedDatabaseFile;
        private enBildTextVerhalten _BildTextVerhalten;
        private int _BildCode_ConstantHeight;

        private string _Prefix;

        private string _BestFile_StandardSuffix;
        private string _BestFile_StandardFolder;

        private bool _AfterEdit_QuickSortRemoveDouble;
        private int _AfterEdit_Runden;
        private bool _AfterEdit_DoUCase;
        private bool _AfterEdit_AutoCorrect;

        private string _CellInitValue;

        /// <summary>
        /// Die zu Suchende ZEile ist in dieser Spalte zu finden
        /// </summary>
        private int _LinkedCell_RowKey;
        /// <summary>
        /// Die Quell-Spalte (aus der verlinkten Datenbank) ist immer
        /// </summary>
        private int _LinkedCell_ColumnKey;
        /// <summary>
        /// Die zu suchende Spalte ist in dieser Spalte zu finden
        /// </summary>
        private int _LinkedCell_ColumnValueFoundIn;
        /// <summary>
        /// ...zusätzlich folgende Zeichenkette hinzufügen
        /// </summary>
        private string _LinkedCell_ColumnValueAdd;

        //private bool _ZellenZusammenfassen;

        private int _DropDownKey;

        private int _VorschlagsColumn;

        private enAlignmentHorizontal _Align;

        public readonly ListExt<string> Regex = new ListExt<string>();

        private string _SortMask;

        private string _AutoRemove;
        private bool _SaveContent;
        //private enDauerfilter _AutoFilter_Dauerfilter;

        private int _KeyColumnKey;

        public SizeF TMP_CaptionText_Size = new SizeF(-1, -1);
        internal Database _TMP_LinkedDatabase;
        public int? TMP_ColumnContentWidth = null;
        public bool? TMP_AutoFilterSinnvoll = null;
        public int? TMP_IfFilterRemoved = null;

        internal List<string> _UcaseNamesSortedByLenght = null;

        public const string AllowedCharsInternalName = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_"; // Klammern, Semikolon etc. verboten, da es im Regeln bei Substring verwendet wird

        #endregion


        #region  Event-Deklarationen + Delegaten 
        public event EventHandler Changed;
        #endregion


        #region  Construktor + Initialize 


        public ColumnItem(Database database, int columnkey) {
            Database = database;
            if (columnkey < 0) { Develop.DebugPrint(enFehlerArt.Fehler, "ColumnKey <0"); }

            var ex = Database.Column.SearchByKey(columnkey);
            if (ex != null) { Develop.DebugPrint(enFehlerArt.Fehler, "Key existiert bereits"); }

            Key = columnkey;


            #region Standard-Werte

            _Name = Database.Column.Freename(string.Empty);
            _Caption = string.Empty;
            _CaptionBitmap = null;

            _Format = enDataFormat.Bit;
            _LineLeft = enColumnLineStyle.Dünn;
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
            _Align = enAlignmentHorizontal.Keine_Präferenz;



            _KeyColumnKey = -1;



            _EditType = enEditTypeFormula.Textfeld;
            _Identifier = string.Empty;



            _AllowedChars = string.Empty;
            _AdminInfo = string.Empty;
            _FilterOptions = enFilterOptions.Enabled | enFilterOptions.TextFilterEnabled | enFilterOptions.ExtendedFilterEnabled;
            //_AutofilterErlaubt = true;
            //_AutofilterTextFilterErlaubt = true;
            //_AutoFilterErweitertErlaubt = true;
            _IgnoreAtRowFilter = false;
            _DropdownBearbeitungErlaubt = false;
            _DropdownAllesAbwählenErlaubt = false;
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

            #endregion


            DropDownItems.Changed += DropDownItems_ListOrItemChanged;
            OpticalReplace.Changed += OpticalReplacer_ListOrItemChanged;
            AfterEdit_AutoReplace.Changed += AfterEdit_AutoReplace_ListOrItemChanged;
            Regex.Changed += Regex_ListOrItemChanged;
            PermissionGroups_ChangeCell.Changed += PermissionGroups_ChangeCell_ListOrItemChanged;
            Tags.Changed += Tags_ListOrItemChanged;




            Invalidate_TmpVariables();
        }
        #endregion




        #region  Properties 



        public int Key { get; }

        public string I_Am_A_Key_For_Other_Column { get; private set; }


        internal void CheckIfIAmAKeyColumn() {
            I_Am_A_Key_For_Other_Column = string.Empty;

            foreach (var ThisColumn in Database.Column) {

                if (ThisColumn.KeyColumnKey == Key) { I_Am_A_Key_For_Other_Column = "Spalte " + ThisColumn.ReadableText() + " verweist auf diese Spalte"; } // Werte Gleichhalten
                if (ThisColumn.LinkedCell_RowKey == Key) { I_Am_A_Key_For_Other_Column = "Spalte " + ThisColumn.ReadableText() + " verweist auf diese Spalte"; } // LinkdeCells pflegen
                if (ThisColumn.LinkedCell_ColumnValueFoundIn == Key) { I_Am_A_Key_For_Other_Column = "Spalte " + ThisColumn.ReadableText() + " verweist auf diese Spalte"; } // LinkdeCells pflegen

            }

            if (_Format == enDataFormat.Columns_für_LinkedCellDropdown) { I_Am_A_Key_For_Other_Column = "Die Spalte selbst durch das Format"; }
        }



        public string Caption {
            get {
                return _Caption;
            }
            set {
                value = value.Replace("<br>", "\r", RegexOptions.IgnoreCase);
                if (_Caption == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Caption, this, _Caption, value, true);
                Invalidate_TmpVariables();
                OnChanged();
            }
        }



        public string AutoFilterJoker {
            get {
                return _AutoFilterJoker;
            }
            set {
                if (_AutoFilterJoker == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AutoFilterJoker, this, _AutoFilterJoker, value, true);
                OnChanged();
            }
        }


        public string Name {
            get {
                return _Name.ToUpper();
            }
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

        public string Identifier {
            get {
                return _Identifier;
            }
            set {
                if (_Identifier == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Identifier, this, _Identifier, value, true);
                OnChanged();
            }
        }

        public enEditTypeFormula EditType {
            get {
                return _EditType;
            }
            set {
                if (_EditType == value) { return; }
                Database.AddPending(enDatabaseDataType.co_EditType, this, ((int)_EditType).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public bool MultiLine {
            get {
                return _MultiLine;
            }
            set {
                if (!_Format.MultilinePossible()) { value = false; }

                if (_MultiLine == value) { return; }
                Database.AddPending(enDatabaseDataType.co_MultiLine, this, _MultiLine.ToPlusMinus(), value.ToPlusMinus(), true);
                Invalidate_ColumAndContent();
                OnChanged();

            }
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

        public string Quickinfo {
            get {
                return _QuickInfo;
            }
            set {
                if (_QuickInfo == value) { return; }
                Database.AddPending(enDatabaseDataType.co_QuickInfo, this, _QuickInfo, value, true);
                OnChanged();
            }
        }
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
            get {
                return _DauerFilterPos;
            }
            set {
                if (_DauerFilterPos.ToString() == value.ToString()) { return; }
                Database.AddPending(enDatabaseDataType.co_DauerFilterPos, this, _DauerFilterPos.ToString(), value.ToString(), true);
                OnChanged();
            }
        }
        public string Ueberschrift1 {
            get {
                return _Ueberschrift1;
            }
            set {
                if (_Ueberschrift1 == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Ueberschrift1, this, _Ueberschrift1, value, true);
                OnChanged();
            }
        }


        public string Ueberschrift2 {
            get {
                return _Ueberschrift2;
            }
            set {
                if (_Ueberschrift2 == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Ueberschrift2, this, _Ueberschrift2, value, true);
                OnChanged();
            }
        }

        public string Ueberschrift3 {
            get {
                return _Ueberschrift3;
            }
            set {
                if (_Ueberschrift3 == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Ueberschrift3, this, _Ueberschrift3, value, true);
                OnChanged();
            }
        }



        public Bitmap CaptionBitmap {
            get {
                return _CaptionBitmap;
            }
            set {
                if (modConverter.BitmapToString(_CaptionBitmap, ImageFormat.Png) == modConverter.BitmapToString(value, ImageFormat.Png)) { return; }
                Database.AddPending(enDatabaseDataType.co_CaptionBitmap, this, modConverter.BitmapToString(_CaptionBitmap, ImageFormat.Png), modConverter.BitmapToString(value, ImageFormat.Png), false);


                if (value == null) {
                    _CaptionBitmap = null;
                } else {
                    _CaptionBitmap = (Bitmap)value.Clone();
                }

                Invalidate_TmpVariables();
                OnChanged();
            }
        }


        public string AdminInfo {
            get {
                return _AdminInfo;
            }
            set {
                if (_AdminInfo == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AdminInfo, this, _AdminInfo, value, true);
                OnChanged();
            }
        }

        public List<string> GetUcaseNamesSortedByLenght() {

            if (_UcaseNamesSortedByLenght != null) { return _UcaseNamesSortedByLenght; }
            var tmp = Contents(null);


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

        /// <summary>
        /// Was in Textfeldern oder Datenbankzeilen für ein Suffix angezeigt werden soll. Beispiel: mm
        /// </summary>
        public string Suffix {
            get {
                return _Suffix;
            }
            set {
                if (_Suffix == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Suffix, this, _Suffix, value, true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }



        public string Prefix {
            get {
                return _Prefix;
            }
            set {
                if (_Prefix == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Prefix, this, _Prefix, value, true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }



        public string LinkedDatabaseFile {
            get {
                return _LinkedDatabaseFile;
            }
            set {
                if (_LinkedDatabaseFile == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LinkedDatabase, this, _LinkedDatabaseFile, value, true);
                Invalidate_TmpVariables();
                OnChanged();
            }
        }


        public string LinkedKeyKennung {
            get {
                return _LinkedKeyKennung;
            }
            set {
                if (_LinkedKeyKennung == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LinkKeyKennung, this, _LinkedKeyKennung, value, true);
                OnChanged();
            }
        }

        public string BestFile_StandardSuffix {
            get {
                return _BestFile_StandardSuffix;
            }
            set {
                if (_BestFile_StandardSuffix == value) { return; }
                Database.AddPending(enDatabaseDataType.co_BestFile_StandardSuffix, this, _BestFile_StandardSuffix, value, true);
                OnChanged();
            }
        }

        public string BestFile_StandardFolder {
            get {
                return _BestFile_StandardFolder;
            }
            set {
                if (_BestFile_StandardFolder == value) { return; }
                Database.AddPending(enDatabaseDataType.co_BestFile_StandardFolder, this, _BestFile_StandardFolder, value, true);
                OnChanged();
            }
        }

        public int BildCode_ConstantHeight {
            get {
                return _BildCode_ConstantHeight;
            }
            set {
                if (_BildCode_ConstantHeight == value) { return; }
                Database.AddPending(enDatabaseDataType.co_BildCode_ConstantHeight, this, _BildCode_ConstantHeight.ToString(), value.ToString(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }


        public string Ueberschriften {
            get {
                var txt = _Ueberschrift1 + "/" + _Ueberschrift2 + "/" + _Ueberschrift3;
                if (txt == "//") { return "###"; }
                return txt.TrimEnd("/");

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nummer">Muss 1, 2 oder 3 sein</param>
        /// <returns></returns>
        public string Ueberschrift(int Nummer) {
            switch (Nummer) {
                case 0: return _Ueberschrift1;
                case 1: return _Ueberschrift2;
                case 2: return _Ueberschrift3;
                default:
                    Develop.DebugPrint(enFehlerArt.Warnung, "Nummer " + Nummer + " nicht erlaubt.");
                    return string.Empty;
            }
        }

        public enBildTextVerhalten BildTextVerhalten {
            get {
                return _BildTextVerhalten;
            }
            set {
                if (_BildTextVerhalten == value) { return; }
                Database.AddPending(enDatabaseDataType.co_BildTextVerhalten, this, ((int)_BildTextVerhalten).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }



        public enAlignmentHorizontal Align {
            get {
                return _Align;
            }
            set {
                if (_Align == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Align, this, ((int)_Align).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public enFilterOptions FilterOptions {
            get {
                return _FilterOptions;
            }
            set {
                if (_FilterOptions == value) { return; }
                Database.AddPending(enDatabaseDataType.co_FilterOptions, this, ((int)_FilterOptions).ToString(), ((int)value).ToString(), true);
                Invalidate_TmpVariables();
                OnChanged();
            }
        }
        //public bool AutoFilterErlaubt
        //{
        //    get
        //    {
        //        if (!_Format.Autofilter_möglich()) { return false; }
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
            get {
                if (!_Format.Autofilter_möglich()) { return true; }
                return _IgnoreAtRowFilter;
            }
            set {
                if (_IgnoreAtRowFilter == value) { return; }
                Database.AddPending(enDatabaseDataType.co_BeiZeilenfilterIgnorieren, this, _IgnoreAtRowFilter.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool TextBearbeitungErlaubt {
            get {
                return _TextBearbeitungErlaubt;
            }
            set {
                if (_TextBearbeitungErlaubt == value) { return; }
                Database.AddPending(enDatabaseDataType.co_TextBearbeitungErlaubt, this, _TextBearbeitungErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool SpellCheckingEnabled {
            get {
                return _SpellCheckingEnabled;
            }
            set {
                if (_SpellCheckingEnabled == value) { return; }
                Database.AddPending(enDatabaseDataType.co_SpellCheckingEnabled, this, _SpellCheckingEnabled.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }


        public bool DropdownBearbeitungErlaubt {
            get {
                return _DropdownBearbeitungErlaubt;
            }
            set {
                if (_DropdownBearbeitungErlaubt == value) { return; }
                Database.AddPending(enDatabaseDataType.co_DropdownBearbeitungErlaubt, this, _DropdownBearbeitungErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool DropdownAllesAbwählenErlaubt {
            get {
                return _DropdownAllesAbwählenErlaubt;
            }
            set {
                if (_DropdownAllesAbwählenErlaubt == value) { return; }
                Database.AddPending(enDatabaseDataType.co_DropdownAllesAbwählenErlaubt, this, _DropdownAllesAbwählenErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool DropdownWerteAndererZellenAnzeigen {
            get {
                return _DropdownWerteAndererZellenAnzeigen;
            }
            set {
                if (_DropdownWerteAndererZellenAnzeigen == value) { return; }
                Database.AddPending(enDatabaseDataType.co_DropdownWerteAndererZellenAnzeigen, this, _DropdownWerteAndererZellenAnzeigen.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool AfterEdit_QuickSortRemoveDouble {
            get {
                if (!_MultiLine) { return false; }
                return _AfterEdit_QuickSortRemoveDouble;
            }
            set {
                if (_AfterEdit_QuickSortRemoveDouble == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AfterEdit_QuickSortAndRemoveDouble, this, _AfterEdit_QuickSortRemoveDouble.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool AfterEdit_DoUCase {
            get {
                return _AfterEdit_DoUCase;
            }
            set {
                if (_AfterEdit_DoUCase == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AfterEdit_DoUcase, this, _AfterEdit_DoUCase.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }


        public bool AfterEdit_AutoCorrect {
            get {
                return _AfterEdit_AutoCorrect;
            }
            set {
                if (_AfterEdit_AutoCorrect == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AfterEdit_AutoCorrect, this, _AfterEdit_AutoCorrect.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public string CellInitValue {
            get {
                return _CellInitValue;
            }
            set {
                if (_CellInitValue == value) { return; }
                Database.AddPending(enDatabaseDataType.co_CellInitValue, this, _CellInitValue, value, true);
                OnChanged();
            }
        }


        public string SortMask {
            get {
                return _SortMask;
            }
            set {
                if (_SortMask == value) { return; }
                Database.AddPending(enDatabaseDataType.co_SortMask, this, _SortMask, value, true);
                OnChanged();
            }
        }

        public string AutoRemove {
            get {
                return _AutoRemove;
            }
            set {
                if (_AutoRemove == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AutoRemove, this, _AutoRemove, value, true);
                OnChanged();
            }
        }

        public bool SaveContent {
            get {
                return _SaveContent;
            }
            set {
                if (_SaveContent == value) { return; }
                Database.AddPending(enDatabaseDataType.co_SaveContent, this, _SaveContent.ToPlusMinus(), value.ToPlusMinus(), true);
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
            get {
                return _DropDownKey;
            }
            set {
                if (_DropDownKey == value) { return; }
                Database.AddPending(enDatabaseDataType.co_DropDownKey, this, _DropDownKey.ToString(), value.ToString(), true);
                OnChanged();
            }
        }

        public int VorschlagsColumn {
            get {
                return _VorschlagsColumn;
            }
            set {
                if (_VorschlagsColumn == value) { return; }
                Database.AddPending(enDatabaseDataType.co_VorschlagColumn, this, _VorschlagsColumn.ToString(), value.ToString(), true);
                OnChanged();
            }
        }


        public int LinkedCell_RowKey {
            get {
                return _LinkedCell_RowKey;
            }
            set {
                if (_LinkedCell_RowKey == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LinkedCell_RowKey, this, _LinkedCell_RowKey.ToString(), value.ToString(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }


        public int LinkedCell_ColumnKey {
            get {
                return _LinkedCell_ColumnKey;
            }
            set {
                if (_LinkedCell_ColumnKey == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LinkedCell_ColumnKey, this, _LinkedCell_ColumnKey.ToString(), value.ToString(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public int LinkedCell_ColumnValueFoundIn {
            get {
                return _LinkedCell_ColumnValueFoundIn;
            }
            set {
                if (_LinkedCell_ColumnValueFoundIn == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LinkedCell_ColumnValueFoundIn, this, _LinkedCell_ColumnValueFoundIn.ToString(), value.ToString(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public string LinkedCell_ColumnValueAdd {
            get {
                return _LinkedCell_ColumnValueAdd;
            }
            set {
                if (_LinkedCell_ColumnValueAdd == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LinkedCell_ColumnValueAdd, this, _LinkedCell_ColumnValueAdd, value, true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }



        /// <summary>
        /// Hält Werte, dieser Spalte gleich, bezugnehmend der KeyColumn(key)
        /// </summary>
        public int KeyColumnKey {
            get {
                return _KeyColumnKey;
            }
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


        public int AfterEdit_Runden {
            get {
                return _AfterEdit_Runden;
            }
            set {
                if (_AfterEdit_Runden == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AfterEdit_Runden, this, _AfterEdit_Runden.ToString(), value.ToString(), true);
                OnChanged();
            }
        }


        public bool ShowUndo {
            get {
                return _ShowUndo;
            }
            set {
                if (_ShowUndo == value) { return; }
                Database.AddPending(enDatabaseDataType.co_ShowUndo, this, _ShowUndo.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }


        public bool ShowMultiLineInOneLine {
            get {
                if (!_MultiLine) { return false; }
                return _ShowMultiLineInOneLine;
            }
            set {
                if (_ShowMultiLineInOneLine == value) { return; }
                Database.AddPending(enDatabaseDataType.co_ShowMultiLineInOneLine, this, _ShowMultiLineInOneLine.ToPlusMinus(), value.ToPlusMinus(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }


        public string AllowedChars {
            get {
                return _AllowedChars;
            }
            set {
                if (_AllowedChars == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AllowedChars, this, _AllowedChars, value, true);
                OnChanged();
            }
        }


        public enDataFormat Format {
            get {
                return _Format;
            }
            set {
                if (_Format == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Format, this, ((int)_Format).ToString(), ((int)value).ToString(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public Color ForeColor {
            get {
                return _ForeColor;
            }
            set {
                if (_ForeColor.ToArgb() == value.ToArgb()) { return; }
                Database.AddPending(enDatabaseDataType.co_ForeColor, this, _ForeColor.ToArgb().ToString(), value.ToArgb().ToString(), true);
                OnChanged();
            }
        }


        public Color BackColor {
            get {
                return _BackColor;
            }
            set {
                if (_BackColor.ToArgb() == value.ToArgb()) { return; }
                Database.AddPending(enDatabaseDataType.co_BackColor, this, _BackColor.ToArgb().ToString(), value.ToArgb().ToString(), true);
                OnChanged();
            }
        }


        public bool EditTrotzSperreErlaubt {
            get {
                return _EditTrotzSperreErlaubt;
            }
            set {
                if (_EditTrotzSperreErlaubt == value) { return; }
                Database.AddPending(enDatabaseDataType.co_EditTrotzSperreErlaubt, this, _EditTrotzSperreErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }


        public enColumnLineStyle LineLeft {
            get {
                return _LineLeft;
            }
            set {
                if (_LineLeft == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LineLeft, this, ((int)_LineLeft).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public enColumnLineStyle LineRight {
            get {
                return _LineRight;
            }
            set {
                if (_LineRight == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LinieRight, this, ((int)_LineRight).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public bool IsParsing {
            get {
                Develop.DebugPrint(enFehlerArt.Fehler, "Kann nur über die Datenbank geparsed werden.");
                return false;
            }
        }




        #endregion


        private Database TMP_LinkedDatabase {
            set {
                if (value == _TMP_LinkedDatabase) { return; }

                Invalidate_TmpVariables();

                _TMP_LinkedDatabase = value;

                if (_TMP_LinkedDatabase != null) {
                    _TMP_LinkedDatabase.RowKeyChanged += _TMP_LinkedDatabase_RowKeyChanged;
                    _TMP_LinkedDatabase.ColumnKeyChanged += _TMP_LinkedDatabase_ColumnKeyChanged;
                    _TMP_LinkedDatabase.ConnectedControlsStopAllWorking += _TMP_LinkedDatabase_ConnectedControlsStopAllWorking;
                    //_TMP_LinkedDatabase.Disposed += _TMP_LinkedDatabase_Disposed;
                    _TMP_LinkedDatabase.Cell.CellValueChanged += _TMP_LinkedDatabase_Cell_CellValueChanged;
                }

            }
        }

        private void _TMP_LinkedDatabase_ColumnKeyChanged(object sender, KeyChangedEventArgs e) {



            if (_Format != enDataFormat.Columns_für_LinkedCellDropdown) {
                var os = e.KeyOld.ToString();
                var ns = e.KeyNew.ToString();
                foreach (var ThisRow in Database.Row) {
                    if (Database.Cell.GetStringBehindLinkedValue(this, ThisRow) == os) {
                        Database.Cell.SetValueBehindLinkedValue(this, ThisRow, ns, false);
                    }
                }
            }

            if (_Format != enDataFormat.LinkedCell) {
                var os = e.KeyOld.ToString() + "|";
                var ns = e.KeyNew.ToString() + "|";
                foreach (var ThisRow in Database.Row) {
                    var val = Database.Cell.GetStringBehindLinkedValue(this, ThisRow);
                    if (val.StartsWith(os)) {
                        Database.Cell.SetValueBehindLinkedValue(this, ThisRow, val.Replace(os, ns), false);
                    }
                }
            }
        }

        private void _TMP_LinkedDatabase_RowKeyChanged(object sender, KeyChangedEventArgs e) {
            if (_Format != enDataFormat.LinkedCell) {
                var os = "|" + e.KeyOld.ToString();
                var ns = "|" + e.KeyNew.ToString();
                foreach (var ThisRow in Database.Row) {
                    var val = Database.Cell.GetStringBehindLinkedValue(this, ThisRow);
                    if (val.EndsWith(os)) {
                        Database.Cell.SetValueBehindLinkedValue(this, ThisRow, val.Replace(os, ns), false);
                    }
                }
            }
        }

        private void _TMP_LinkedDatabase_Cell_CellValueChanged(object sender, CellEventArgs e) {

            var tKey = CellCollection.KeyOfCell(e.Column, e.Row);

            foreach (var ThisRow in Database.Row) {
                if (Database.Cell.GetStringBehindLinkedValue(this, ThisRow) == tKey) {
                    CellCollection.Invalidate_CellContentSize(this, ThisRow);
                    Invalidate_TmpColumnContentWidth();
                    Database.Cell.OnCellValueChanged(new CellEventArgs(this, ThisRow));
                    ThisRow.DoAutomatic(true, false, 5);
                }
            }
        }

        private void _TMP_LinkedDatabase_ConnectedControlsStopAllWorking(object sender, MultiUserFileStopWorkingEventArgs e) {
            Database.OnConnectedControlsStopAllWorking(e);
        }

        public Database LinkedDatabase() {
            if (_TMP_LinkedDatabase != null) { return _TMP_LinkedDatabase; }
            if (string.IsNullOrEmpty(_LinkedDatabaseFile)) { return null; }

            DatabaseSettingsEventHandler el = null;


            if (FileExists(_LinkedDatabaseFile)) {
                el = new DatabaseSettingsEventHandler(this, _LinkedDatabaseFile, Database.ReadOnly);

            } else {
                el = new DatabaseSettingsEventHandler(this, Database.Filename.FilePath() + _LinkedDatabaseFile, Database.ReadOnly);

            }

            TMP_LinkedDatabase = (Database)clsMultiUserFile.GetByFilename(el.Filenname, true);
            if (_TMP_LinkedDatabase == null) {
                Database.OnLoadingLinkedDatabase(el);
            }



            TMP_LinkedDatabase = (Database)clsMultiUserFile.GetByFilename(el.Filenname, true); // Event wird ausgelöst, Multitasking pfuscht rein, nochmal prüfen!!!!


            if (_TMP_LinkedDatabase == null) {
                if (FileExists(el.Filenname)) {

                    TMP_LinkedDatabase = new Database(el.Filenname, el.ReadOnly, false); // Wichtig, NICHT _TMP_LinkedDatabase
                }
            }

            if (_TMP_LinkedDatabase != null) { _TMP_LinkedDatabase.UserGroup = Database.UserGroup; }
            return _TMP_LinkedDatabase;
        }


        public void OnChanged() {
            Changed?.Invoke(this, System.EventArgs.Empty);
        }
        internal string ParsableColumnKey() {
            return ColumnCollection.ParsableColumnKey(this);
        }




        public List<string> Contents(FilterCollection Filter) {
            var list = new List<string>();

            foreach (var ThisRowItem in Database.Row) {
                if (ThisRowItem != null) {
                    if (ThisRowItem.MatchesTo(Filter)) {
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


        public void DeleteContents(FilterCollection Filter) {

            foreach (var ThisRowItem in Database.Row) {
                if (ThisRowItem != null && ThisRowItem.MatchesTo(Filter)) { ThisRowItem.CellSet(this, ""); }
            }
        }




        public bool IsFirst() {
            return Convert.ToBoolean(Database.Column[0] == this);
        }


        public ColumnItem Previous() {

            var ColumnCount = Index();

            do {
                ColumnCount--;
                if (ColumnCount < 0) { return null; }
                if (Database.Column[ColumnCount] != null) { return Database.Column[ColumnCount]; }
            } while (true);
        }

        public ColumnItem Next() {


            var ColumnCount = Index();

            do {
                ColumnCount++;
                if (ColumnCount >= Database.Column.Count) { return null; }
                if (Database.Column[ColumnCount] != null) { return Database.Column[ColumnCount]; }

            } while (true);
        }


        internal string Load(enDatabaseDataType Art, string Wert) {
            switch (Art) {

                case enDatabaseDataType.co_Name: _Name = Wert; Invalidate_TmpVariables(); break;
                case enDatabaseDataType.co_Caption: _Caption = Wert; break;
                case enDatabaseDataType.co_Format:
                    _Format = (enDataFormat)int.Parse(Wert);
                    if (Wert == "21") { _Format = enDataFormat.Text; }
                    break;

                case enDatabaseDataType.co_ForeColor: _ForeColor = Color.FromArgb(int.Parse(Wert)); break;
                case enDatabaseDataType.co_BackColor: _BackColor = Color.FromArgb(int.Parse(Wert)); break;
                case enDatabaseDataType.co_LineLeft: _LineLeft = (enColumnLineStyle)int.Parse(Wert); break;
                case enDatabaseDataType.co_LinieRight: _LineRight = (enColumnLineStyle)int.Parse(Wert); break;
                case enDatabaseDataType.co_QuickInfo: _QuickInfo = Wert; break;
                //case enDatabaseDataType.co_Intelligenter_Multifilter: _Intelligenter_Multifilter = Wert; break;
                case enDatabaseDataType.co_DauerFilterPos: _DauerFilterPos = PointParse(Wert); break;
                case enDatabaseDataType.co_Ueberschrift1: _Ueberschrift1 = Wert; break;
                case enDatabaseDataType.co_Ueberschrift2: _Ueberschrift2 = Wert; break;
                case enDatabaseDataType.co_Ueberschrift3: _Ueberschrift3 = Wert; break;
                case enDatabaseDataType.co_CaptionBitmap: _CaptionBitmap = modConverter.StringToBitmap(Wert); break;
                case enDatabaseDataType.co_Identifier:
                    _Identifier = Wert;
                    StandardWerteNachKennung(false);
                    Database.Column.GetSystems();
                    break;
                case enDatabaseDataType.co_EditType: _EditType = (enEditTypeFormula)int.Parse(Wert); break;
                case enDatabaseDataType.co_MultiLine: _MultiLine = Wert.FromPlusMinus(); break;
                case enDatabaseDataType.co_DropDownItems: DropDownItems.SplitByCR_QuickSortAndRemoveDouble(Wert); break;
                case enDatabaseDataType.co_OpticalReplace: OpticalReplace.SplitByCR(Wert); break;
                case enDatabaseDataType.co_AfterEdit_AutoReplace: AfterEdit_AutoReplace.SplitByCR(Wert); break;
                case enDatabaseDataType.co_Regex: Regex.SplitByCR(Wert); break;
                case enDatabaseDataType.co_Tags: Tags.SplitByCR(Wert); break;
                case enDatabaseDataType.co_AutoFilterJoker: _AutoFilterJoker = Wert; break;
                case enDatabaseDataType.co_PermissionGroups_ChangeCell: PermissionGroups_ChangeCell.SplitByCR_QuickSortAndRemoveDouble(Wert); break;
                case enDatabaseDataType.co_AllowedChars: _AllowedChars = Wert; break;
                case enDatabaseDataType.co_FilterOptions: _FilterOptions = (enFilterOptions)int.Parse(Wert); break;
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
                case enDatabaseDataType.co_BeiZeilenfilterIgnorieren: _IgnoreAtRowFilter = Wert.FromPlusMinus(); break;

                case enDatabaseDataType.co_CompactView_alt:
                    break;
                case enDatabaseDataType.co_ShowUndo: _ShowUndo = Wert.FromPlusMinus(); break;
                case enDatabaseDataType.co_ShowMultiLineInOneLine: _ShowMultiLineInOneLine = Wert.FromPlusMinus(); break;
                case enDatabaseDataType.co_TextBearbeitungErlaubt: _TextBearbeitungErlaubt = Wert.FromPlusMinus(); break;
                case enDatabaseDataType.co_DropdownBearbeitungErlaubt: _DropdownBearbeitungErlaubt = Wert.FromPlusMinus(); break;
                case enDatabaseDataType.co_SpellCheckingEnabled: _SpellCheckingEnabled = Wert.FromPlusMinus(); break;
                case enDatabaseDataType.co_DropdownAllesAbwählenErlaubt: _DropdownAllesAbwählenErlaubt = Wert.FromPlusMinus(); break;
                case enDatabaseDataType.co_DropdownWerteAndererZellenAnzeigen: _DropdownWerteAndererZellenAnzeigen = Wert.FromPlusMinus(); break;
                case enDatabaseDataType.co_AfterEdit_QuickSortAndRemoveDouble: _AfterEdit_QuickSortRemoveDouble = Wert.FromPlusMinus(); break;
                case enDatabaseDataType.co_AfterEdit_Runden: _AfterEdit_Runden = int.Parse(Wert); break;
                case enDatabaseDataType.co_AfterEdit_DoUcase: _AfterEdit_DoUCase = Wert.FromPlusMinus(); break;
                case enDatabaseDataType.co_AfterEdit_AutoCorrect: _AfterEdit_AutoCorrect = Wert.FromPlusMinus(); break;
                case enDatabaseDataType.co_SaveContent: _SaveContent = Wert.FromPlusMinus(); break;
                case enDatabaseDataType.co_AutoRemove: _AutoRemove = Wert; break;
                case enDatabaseDataType.co_AdminInfo: _AdminInfo = Wert; break;
                case enDatabaseDataType.co_Suffix: _Suffix = Wert; break;
                case enDatabaseDataType.co_LinkedDatabase: _LinkedDatabaseFile = Wert; break;
                case enDatabaseDataType.co_LinkKeyKennung: _LinkedKeyKennung = Wert; break;
                case enDatabaseDataType.co_BestFile_StandardSuffix: _BestFile_StandardSuffix = Wert; break;
                case enDatabaseDataType.co_BestFile_StandardFolder: _BestFile_StandardFolder = Wert; break;
                case enDatabaseDataType.co_BildCode_ConstantHeight: _BildCode_ConstantHeight = int.Parse(Wert); break;
                case enDatabaseDataType.co_Prefix: _Prefix = Wert; break;
                case enDatabaseDataType.co_BildTextVerhalten: _BildTextVerhalten = (enBildTextVerhalten)int.Parse(Wert); break;
                case enDatabaseDataType.co_EditTrotzSperreErlaubt: _EditTrotzSperreErlaubt = Wert.FromPlusMinus(); break;
                case enDatabaseDataType.co_CellInitValue: _CellInitValue = Wert; break;
                case enDatabaseDataType.co_KeyColumnKey: _KeyColumnKey = int.Parse(Wert); break;
                case enDatabaseDataType.co_LinkedCell_RowKey: _LinkedCell_RowKey = int.Parse(Wert); break;
                case enDatabaseDataType.co_LinkedCell_ColumnKey: _LinkedCell_ColumnKey = int.Parse(Wert); break;
                case enDatabaseDataType.co_LinkedCell_ColumnValueFoundIn: _LinkedCell_ColumnValueFoundIn = int.Parse(Wert); break;
                case enDatabaseDataType.co_LinkedCell_ColumnValueAdd: _LinkedCell_ColumnValueAdd = Wert; break;
                case enDatabaseDataType.co_SortMask: _SortMask = Wert; break;
                //case enDatabaseDataType.co_ZellenZusammenfassen: _ZellenZusammenfassen = Wert.FromPlusMinus(); break;
                case enDatabaseDataType.co_DropDownKey: _DropDownKey = int.Parse(Wert); break;
                case enDatabaseDataType.co_VorschlagColumn: _VorschlagsColumn = int.Parse(Wert); break;
                case enDatabaseDataType.co_Align: _Align = (enAlignmentHorizontal)int.Parse(Wert); break;
                //case (enDatabaseDataType)189: break;
                //case (enDatabaseDataType)192: break;
                //case (enDatabaseDataType)193: break;

                default:
                    if (Art.ToString() == ((int)Art).ToString()) {
                        Develop.DebugPrint(enFehlerArt.Info, "Laden von Datentyp '" + Art + "' nicht definiert.<br>Wert: " + Wert + "<br>Datei: " + Database.Filename);
                    } else {
                        return "Interner Fehler: Für den Datentyp  '" + Art + "'  wurde keine Laderegel definiert.";
                    }

                    break;
            }


            return string.Empty;
        }

        public void StandardWerteNachKennung(bool SetAll) {
            if (string.IsNullOrEmpty(_Identifier)) { return; }

            //if (SetAll && !IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Ausserhalb des parsens!"); }

            // ACHTUNG: Die SetAll Befehle OHNE _, die müssen geloggt werden.

            if (SetAll) {
                LineLeft = enColumnLineStyle.Dünn;
                LineRight = enColumnLineStyle.Ohne;
                ForeColor = Color.FromArgb(0, 0, 0);
                CaptionBitmap = null;
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
                        Caption = "Änderer";
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
                        Caption = "Änder-Datum";
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
                    _Caption = "veraltet und kann gelöscht werden: Zeilenstand";
                    _Identifier = "";
                    break;

                case "System: ID":
                    _Name = "SYS_ID";
                    _Caption = "veraltet und kann gelöscht werden: Zeilen-ID";
                    _Identifier = "";
                    break;

                case "System: Last Used Layout":
                    _Name = "SYS_Layout";
                    _Caption = "veraltet und kann gelöscht werden:  Letztes Layout";
                    _Identifier = "";
                    break;

                default:
                    Develop.DebugPrint("Unbekannte Kennung: " + _Identifier);
                    break;
            }
        }

        public string Verwendung() {

            var t = "<b><u>Verwendung von " + ReadableText() + "</b></u><br>";


            if (!string.IsNullOrEmpty(_Identifier)) {
                t += " - Systemspalte<br>";
            }


            return t + Database.Column_UsedIn(this);



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


        public bool ExportableTextformatForLayout() {
            return _Format.ExportableForLayout();
        }

        /// <summary>
        /// Der Invalidate, der am meisten invalidiert: Alle temporären Variablen und auch jede Zell-Größe der Spalte.
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

        /// <summary>
        /// Wenn sich ein Zelleninhalt verändert hat, muss die Spalte neu berechnet werden.
        /// </summary>
        internal void Invalidate_TmpColumnContentWidth() {
            TMP_ColumnContentWidth = null;
        }

        internal void Invalidate_TmpVariables() {
            TMP_CaptionText_Size = new SizeF(-1, -1);


            if (_TMP_LinkedDatabase != null) {
                _TMP_LinkedDatabase.RowKeyChanged -= _TMP_LinkedDatabase_RowKeyChanged;
                _TMP_LinkedDatabase.ColumnKeyChanged -= _TMP_LinkedDatabase_ColumnKeyChanged;
                _TMP_LinkedDatabase.ConnectedControlsStopAllWorking -= _TMP_LinkedDatabase_ConnectedControlsStopAllWorking;
                //_TMP_LinkedDatabase.Disposed -= _TMP_LinkedDatabase_Disposed;
                _TMP_LinkedDatabase.Cell.CellValueChanged -= _TMP_LinkedDatabase_Cell_CellValueChanged;
                _TMP_LinkedDatabase = null;
            }

            TMP_ColumnContentWidth = null;
        }

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
            Database.SaveToByteList(l, enDatabaseDataType.co_DropdownAllesAbwählenErlaubt, _DropdownAllesAbwählenErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_DropdownWerteAndererZellenAnzeigen, _DropdownWerteAndererZellenAnzeigen.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_QuickInfo, _QuickInfo, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AdminInfo, _AdminInfo, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_CaptionBitmap, modConverter.BitmapToString(_CaptionBitmap, ImageFormat.Png), Key);
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


            //Kennung UNBEDINGT zum Schluss, damit die Standard-Werte gesetzt werden können
            Database.SaveToByteList(l, enDatabaseDataType.co_Identifier, _Identifier, Key);
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


        public QuickImage SymbolForReadableText() {
            if (this == Database.Column.SysRowChanger) { return QuickImage.Get(enImageCode.Person); }
            if (this == Database.Column.SysRowCreator) { return QuickImage.Get(enImageCode.Person); }


            switch (_Format) {
                case enDataFormat.Link_To_Filesystem: return QuickImage.Get(enImageCode.Datei, 16);
                case enDataFormat.RelationText: return QuickImage.Get(enImageCode.Herz, 16);
                case enDataFormat.Datum_und_Uhrzeit: return QuickImage.Get(enImageCode.Uhr, 16);
                case enDataFormat.Bit: return QuickImage.Get(enImageCode.Häkchen, 16);
                case enDataFormat.FarbeInteger: return QuickImage.Get(enImageCode.Pinsel, 16);
                case enDataFormat.Ganzzahl: return QuickImage.Get(enImageCode.Ganzzahl, 16);
                case enDataFormat.Gleitkommazahl: return QuickImage.Get(enImageCode.Gleitkommazahl, 16);
                case enDataFormat.BildCode: return QuickImage.Get(enImageCode.Smiley, 16);
                case enDataFormat.LinkedCell: return QuickImage.Get(enImageCode.Fernglas, 16);
                case enDataFormat.Columns_für_LinkedCellDropdown: return QuickImage.Get(enImageCode.Fernglas, 16, "FF0000", "");
                case enDataFormat.Values_für_LinkedCellDropdown: return QuickImage.Get(enImageCode.Fernglas, 16, "00FF00", "");
                case enDataFormat.Button: return QuickImage.Get(enImageCode.Kugel, 16);
            }


            if (_Format.TextboxEditPossible()) {
                if (_MultiLine) { return QuickImage.Get(enImageCode.Textfeld, 16, "FF0000", ""); }
                return QuickImage.Get(enImageCode.Textfeld);
            }

            return QuickImage.Get("Pfeil_Unten_Scrollbar|14");

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

        public string CompareKey() {
            string tmp;

            if (string.IsNullOrEmpty(_Caption)) {
                tmp = _Name + Constants.FirstSortChar + _Name;
            } else {
                tmp = _Caption + Constants.FirstSortChar + _Name;
            }


            tmp = tmp.Trim(' ');
            tmp = tmp.TrimStart('-');
            tmp = tmp.Trim(' ');

            return tmp;
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
                    if (_TextBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Textfeld) { return true; } // Textfeld immer erlauben auch wenn beide Bearbeitungen nicht erlaubt sind, um die Anzeieg zu gewährleisten.
                    if (_MultiLine && EditType_To_Check == enEditTypeFormula.Textfeld_mit_Auswahlknopf) { return false; }
                    if (_DropdownBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
                    if (_DropdownBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Listbox_1_Zeile) { return true; }
                    if (_MultiLine && _DropdownBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Listbox_3_Zeilen) { return true; }
                    if (_MultiLine && _DropdownBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Listbox_6_Zeilen) { return true; }

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


                case enDataFormat.Columns_für_LinkedCellDropdown:
                case enDataFormat.Values_für_LinkedCellDropdown:
                    if (EditType_To_Check == enEditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
                    return false;

                case enDataFormat.Bit:
                    if (_MultiLine) { return false; }
                    if (EditType_To_Check == enEditTypeFormula.Ja_Nein_Knopf) {
                        if (_DropdownWerteAndererZellenAnzeigen) { return false; }
                        if (DropDownItems.Count > 0) { return false; }
                        return true;
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

        public int Index() {
            return Database.Column.IndexOf(this);
        }

        public string ErrorReason() {

            if (Key < 0) { return "Interner Fehler: ID nicht definiert"; }

            if (string.IsNullOrEmpty(_Name)) { return "Spaltenname nicht definiert."; }


            // Diese Routine ist nicht ganz so streng und erlaubgt auch Ä' und so.
            // Beim Editor eingeben wird das allerdings unterbunden.
            if (!Name.ContainsOnlyChars(AllowedCharsInternalName)) { return "Spaltenname enthält ungültige Zeichen. Erlaubt sind A-Z, 0-9 und _"; }


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
                if (LinkedDatabase() == null) { return "Verknüpfte Datenbank fehlt oder existiert nicht."; }
                if (LinkedDatabase() == Database) { return "Zirkelbezug mit verknüpfter Datenbank."; }
            }


            if (!_Format.Autofilter_möglich() && _FilterOptions != enFilterOptions.None) { return "Bei diesem Format keine Filterung erlaubt."; }

            if (_FilterOptions != enFilterOptions.None && !_FilterOptions.HasFlag(enFilterOptions.Enabled)) { return "Filter Kombination nicht möglich."; }

            if (_FilterOptions != enFilterOptions.Enabled_OnlyAndAllowed && _FilterOptions.HasFlag(enFilterOptions.OnlyAndAllowed)) { return "Filter Kombination nicht möglich."; }
            if (_FilterOptions != enFilterOptions.Enabled_OnlyOrAllowed && _FilterOptions.HasFlag(enFilterOptions.OnlyOrAllowed)) { return "Filter Kombination nicht möglich."; }

            if (_FilterOptions.HasFlag(enFilterOptions.OnlyAndAllowed) || _FilterOptions.HasFlag(enFilterOptions.OnlyOrAllowed)) {
                if (!_MultiLine) {
                    return "Dieser Filter kann nur bei Mehrzeiligen Spalten benutzt werden.";
                }
            }


            switch (_Format) {

                case enDataFormat.Bit:
                    if (_FilterOptions.HasFlag(enFilterOptions.ExtendedFilterEnabled)) { return "Format unterstützt keinen 'erweiternden Autofilter'"; }
                    if (_FilterOptions.HasFlag(enFilterOptions.TextFilterEnabled)) { return "Format unterstützt keine 'Texteingabe bei Autofilter'"; }
                    if (!string.IsNullOrEmpty(_AutoFilterJoker)) { return "Format unterstützt keinen 'Autofilter Joker'"; }
                    if (!_IgnoreAtRowFilter) { return "Format muss bei Zeilenfilter ignoriert werden.'"; }
                    break;

                case enDataFormat.RelationText:
                    if (!_MultiLine) { return "Bei diesem Format muss mehrzeilig ausgewählt werden."; }
                    if (_KeyColumnKey > -1) { return "Diese Format darf keine Verknüpfung zu einer Schlüsselspalte haben."; }
                    if (IsFirst()) { return "Diese Format ist bei der ersten (intern) erste Spalte nicht erlaubt."; }
                    if (!string.IsNullOrEmpty(_CellInitValue)) { return "Diese Format kann keinen Initial-Text haben."; }
                    if (_VorschlagsColumn > 0) { return "Diese Format kann keine Vorschlags-Spalte haben."; }

                    break;

                case enDataFormat.LinkedCell:
                    if (!string.IsNullOrEmpty(_CellInitValue)) { return "Dieses Format kann keinen Initial-Text haben."; }
                    if (_KeyColumnKey > -1) { return "Dieses Format darf keine Verknüpfung zu einer Schlüsselspalte haben."; }
                    if (IsFirst()) { return "Dieses Format ist bei der ersten (intern) erste Spalte nicht erlaubt."; }
                    if (_LinkedCell_RowKey < 0) { return "Die Angabe der Spalte, aus der der Schlüsselwert geholt wird, fehlt."; }
                    if (_LinkedCell_ColumnValueFoundIn < 0 && _LinkedCell_ColumnKey < 0) { return "Information fehlt, welche Spalte der Zieldatenbank verwendet werden soll."; }
                    if (_LinkedCell_ColumnValueFoundIn > -1 && _LinkedCell_ColumnKey > -1) { return "Doppelte Informationen, welche Spalte der Zieldatenbank verwendet werden soll."; }
                    if (_LinkedCell_ColumnValueFoundIn < 0 && !string.IsNullOrEmpty(LinkedCell_ColumnValueAdd)) { return "Falsche Ziel-Spalte ODER Spalten-Vortext flasch."; }
                    if (_VorschlagsColumn > 0) { return "Dieses Format kann keine Vorschlags-Spalte haben."; }

                    if (_LinkedCell_ColumnKey > 0) {
                        var c = LinkedDatabase().Column.SearchByKey(_LinkedCell_ColumnKey);
                        if (c == null) { return "Die verknüpfte Spalte existiert nicht."; }
                        if (c.MultiLine != _MultiLine) { return "Multiline stimmt nicht mit der Ziel-Spalte Multiline überein"; }
                    } else {
                        if (!_MultiLine) { return "Dieses Format muss mehrzeilig sein, da es von der Ziel-Spalte gesteuert wird."; }
                    }



                    break;

                case enDataFormat.Values_für_LinkedCellDropdown:
                    if (!string.IsNullOrEmpty(_CellInitValue)) { return "Dieses Format kann keinen Initial-Text haben."; }
                    if (KeyColumnKey > -1) { return "Dieses Format darf keine Verknüpfung zu einer Schlüsselspalte haben."; }
                    if (_VorschlagsColumn > 0) { return "Dieses Format kann keine Vorschlags-Spalte haben."; }
                    break;


                case enDataFormat.Link_To_Filesystem:
                    if (!string.IsNullOrEmpty(_CellInitValue)) { return "Dieses Format kann keinen Initial-Text haben."; }
                    if (_MultiLine && !_AfterEdit_QuickSortRemoveDouble) { return "Format muss sortiert werden."; }
                    if (_VorschlagsColumn > 0) { return "Dieses Format kann keine Vorschlags-Spalte haben."; }
                    if (!string.IsNullOrEmpty(_AutoRemove)) { return "Dieses Format darf keine Autokorrektur-Maßnahmen haben haben."; }
                    if (_AfterEdit_AutoCorrect) { return "Dieses Format darf keine Autokorrektur-Maßnahmen haben haben."; }
                    if (_AfterEdit_DoUCase) { return "Dieses Format darf keine Autokorrektur-Maßnahmen haben haben."; }
                    if (_AfterEdit_Runden != -1) { return "Dieses Format darf keine Autokorrektur-Maßnahmen haben haben."; }
                    if (AfterEdit_AutoReplace.Count > 0) { return "Dieses Format darf keine Autokorrektur-Maßnahmen haben haben."; }
                    break;


                case enDataFormat.Text_mit_Formatierung:
                    if (_AfterEdit_QuickSortRemoveDouble) { return "Format darf nicht sortiert werden."; }
                    break;

            }



            if (_MultiLine) {
                if (!_Format.MultilinePossible()) { return "Format unterstützt keine mehrzeiligen Texte."; }
                if (_AfterEdit_Runden != -1) { return "Runden nur bei einzeiligen Texten möglich"; }
            } else {
                if (_ShowMultiLineInOneLine) { return "Wenn mehrzeilige Texte einzeilig dargestellt werden sollen, muss mehrzeilig angewählt sein."; }
                if (_AfterEdit_QuickSortRemoveDouble) { return "Sortierung kann nur bei mehrzeiligen Feldern erfolgen."; }
            }

            if (_SpellCheckingEnabled && !_Format.SpellCheckingPossible()) { return "Rechtschreibprüfung bei diesem Format nicht möglich."; }



            if (_EditTrotzSperreErlaubt && !_TextBearbeitungErlaubt && !_DropdownBearbeitungErlaubt) { return "Wenn die Zeilensperre ignoriert werden soll, muss eine Bearbeitungsmethode definiert sein."; }




            var TMP_EditDialog = UserEditDialogTypeInTable(_Format, false, true, _MultiLine);
            if (_TextBearbeitungErlaubt) {
                if (TMP_EditDialog == enEditTypeTable.Dropdown_Single) { return "Format unterstützt nur Dropdown-Menü."; }
                if (TMP_EditDialog == enEditTypeTable.None) { return "Format unterstützt keine Standard-Bearbeitung."; }
            } else {
                if (_VorschlagsColumn > -1) { return "'Vorschlags-Text-Spalte' nur bei Texteingabe möglich."; }
                if (!string.IsNullOrEmpty(_AllowedChars)) { return "'Erlaubte Zeichen' nur bei Texteingabe nötig."; }
            }

            if (_DropdownBearbeitungErlaubt) {
                if (_SpellCheckingEnabled) { return "Entweder Dropdownmenü oder Rechtschreibprüfung."; }
                if (TMP_EditDialog == enEditTypeTable.None) { return "Format unterstützt keine Auswahlmenü-Bearbeitung."; }
            }


            if (!_DropdownBearbeitungErlaubt && !_TextBearbeitungErlaubt) {
                if (PermissionGroups_ChangeCell.Count > 0) { return "Bearbeitungsberechtigungen entfernen, wenn keine Bearbeitung erlaubt ist."; }
            }

            if (!string.IsNullOrEmpty(_CellInitValue)) {
                if (IsFirst()) { return "Die erste Spalte darf keinen InitialWert haben."; }
                if (_VorschlagsColumn > -1) { return "InitialWert und Vorschlagspalten-Initial-Text gemeinsam nicht möglich"; }
            }

            foreach (var thisS in PermissionGroups_ChangeCell) {
                if (thisS.Contains("|")) { return "Unerlaubtes Zeichen bei den Gruppen, die eine Zelle bearbeiten dürfen."; }
                if (thisS.ToUpper() == "#ADMINISTRATOR") { return "'#Administrator' bei den Bearbeitern entfernen."; }
            }

            if (_DropdownBearbeitungErlaubt || TMP_EditDialog == enEditTypeTable.Dropdown_Single) {

                if (_Format != enDataFormat.Bit && _Format != enDataFormat.Columns_für_LinkedCellDropdown && _Format != enDataFormat.Values_für_LinkedCellDropdown) {
                    if (!_DropdownWerteAndererZellenAnzeigen && DropDownItems.Count == 0) { return "Keine Dropdown-Items vorhanden bzw. Alles hinzufügen nicht angewählt."; }
                }

            } else {

                if (_DropdownWerteAndererZellenAnzeigen) { return "Dropdownmenu nicht ausgewählt, 'alles hinzufügen' prüfen."; }
                if (_DropdownAllesAbwählenErlaubt) { return "Dropdownmenu nicht ausgewählt, 'alles abwählen' prüfen."; }
                if (DropDownItems.Count > 0) { return "Dropdownmenu nicht ausgewählt, Dropdown-Items vorhanden."; }

            }


            if (_DropdownWerteAndererZellenAnzeigen && !_Format.DropdownItemsOfOtherCellsAllowed()) { return "'Dropdownmenu alles hinzufügen' bei diesem Format nicht erlaubt."; }
            if (_DropdownAllesAbwählenErlaubt && !_Format.DropdownUnselectAllAllowed()) { return "'Dropdownmenu alles abwählen' bei diesem Format nicht erlaubt."; }
            if (DropDownItems.Count > 0 && !_Format.DropdownItemsAllowed()) { return "Manuelle 'Dropdow-Items' bei diesem Format nicht erlaubt."; }



            if (_BildTextVerhalten != enBildTextVerhalten.Nur_Text) {

                if (_Format == enDataFormat.Datum_und_Uhrzeit ||
                    _Format == enDataFormat.Ganzzahl ||
                    _Format == enDataFormat.Gleitkommazahl ||
                    _Format == enDataFormat.Text ||
                    _Format == enDataFormat.Text_mit_Formatierung) {
                    // Performance-Teschnische Gründe
                    _BildTextVerhalten = enBildTextVerhalten.Nur_Text;
                    //return "Bei diesem Format muss das Bild/Text-Verhalten 'Nur Text' sein.";
                }
            } else {
                if (_Format == enDataFormat.BildCode || _Format == enDataFormat.FarbeInteger) {
                    return "Bei diesem Format darf das Bild/Text-Verhalten nicht 'Nur Text' sein.";
                }
            }




            if (!string.IsNullOrEmpty(_Suffix)) {
                if (_MultiLine) { return "Einheiten und Mehrzeilig darf nicht kombiniert werden."; }
            }


            if (_AfterEdit_Runden > 6) { return "Beim Runden maximal 6 Nachkommastellen möglich"; }


            if (_FilterOptions == enFilterOptions.None) {
                if (!string.IsNullOrEmpty(_AutoFilterJoker)) { return "Wenn kein Autofilter erlaubt ist, immer anzuzeigende Werte entfernen"; }
            }





            if (string.IsNullOrEmpty(_LinkedKeyKennung) && _Format.NeedLinkedKeyKennung()) { return "Spaltenkennung für verlinkte Datenbanken fehlt."; }


            if (OpticalReplace.Count > 0) {
                if (_Format != enDataFormat.Text &&
                    _Format != enDataFormat.Columns_für_LinkedCellDropdown &&
                    _Format != enDataFormat.BildCode &&
                    _Format != enDataFormat.Ganzzahl &&
                    _Format != enDataFormat.Gleitkommazahl &&
                    _Format != enDataFormat.RelationText) { return "Format unterstützt keine Ersetzungen."; }

                if (_FilterOptions.HasFlag(enFilterOptions.ExtendedFilterEnabled)) { return "Entweder 'Ersetzungen' oder 'erweiternden Autofilter'"; }
                if (!string.IsNullOrEmpty(_AutoFilterJoker)) { return "Entweder 'Ersetzungen' oder 'Autofilter Joker'"; }
            }

            if (_KeyColumnKey > -1) {
                if (!string.IsNullOrEmpty(I_Am_A_Key_For_Other_Column)) { return "Eine Schlüsselspalte darf selbst keine Verknüpfung zu einer anderen Spalte haben: " + I_Am_A_Key_For_Other_Column; }
                var c = Database.Column.SearchByKey(_KeyColumnKey);
                if (c == null) { return "Die verknüpfte Schlüsselspalte existiert nicht."; }

            }


            if (IsFirst()) {
                if (_KeyColumnKey > -1) { return "Die (intern) erste Spalte darf keine Verknüpfung zu einer andern Schlüsselspalte haben."; }

            }

            if (_Format != enDataFormat.LinkedCell) {
                if (_LinkedCell_RowKey > -1) { return "Nur verlinkte Zellen können Daten über verlinkte Zellen enthalten."; }
                if (_LinkedCell_ColumnKey > -1) { return "Nur verlinkte Zellen können Daten über verlinkte Zellen enthalten."; }
                if (_LinkedCell_ColumnValueFoundIn > -1) { return "Nur verlinkte Zellen können Daten über verlinkte Zellen enthalten."; }
            }


            return string.Empty;
        }


        public bool IsOk() {
            return string.IsNullOrEmpty(ErrorReason());
        }

        private void Tags_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(enDatabaseDataType.co_Tags, Key, Tags.JoinWithCr(), false);
            OnChanged();
        }


        private void DropDownItems_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(enDatabaseDataType.co_DropDownItems, Key, DropDownItems.JoinWithCr(), false);
            OnChanged();
        }

        private void OpticalReplacer_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(enDatabaseDataType.co_OpticalReplace, Key, OpticalReplace.JoinWithCr(), false);
            Invalidate_ColumAndContent();
            OnChanged();
        }

        private void AfterEdit_AutoReplace_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(enDatabaseDataType.co_AfterEdit_AutoReplace, Key, AfterEdit_AutoReplace.JoinWithCr(), false);
            OnChanged();
        }

        private void Regex_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(enDatabaseDataType.co_Regex, Key, Regex.JoinWithCr(), false);
            OnChanged();
        }

        private void PermissionGroups_ChangeCell_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(enDatabaseDataType.co_PermissionGroups_ChangeCell, Key, PermissionGroups_ChangeCell.JoinWithCr(), false);
            OnChanged();
        }



        public string AutoCorrect(string Value) {

            if (Format == enDataFormat.Link_To_Filesystem) {
                var l = new List<string>(Value.SplitByCR());
                var l2 = new List<string>();

                foreach (var thisFile in l) {
                    l2.Add(SimplyFile(thisFile));
                }

                Value = l2.SortedDistinctList().JoinWithCr();
            }


            if (_AfterEdit_DoUCase) { Value = Value.ToUpper(); }

            if (!string.IsNullOrEmpty(_AutoRemove)) { Value = Value.RemoveChars(_AutoRemove); }



            if (AfterEdit_AutoReplace.Count > 0) {
                var l = new List<string>(Value.SplitByCR());

                foreach (var thisar in AfterEdit_AutoReplace) {
                    var rep = thisar.SplitBy("|");

                    for (var z = 0; z < l.Count; z++) {

                        var r = string.Empty;
                        if (rep.Count() > 1) { r = rep[1].Replace(";cr;", "\r"); }
                        var op = string.Empty;
                        if (rep.Count() > 2) { op = rep[2].ToLower(); }


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


        private string KleineFehlerCorrect(string TXT) {
            if (string.IsNullOrEmpty(TXT)) { return string.Empty; }

            const char h4 = (char)1004; // H4 = Normaler Text, nach links rutschen
            const char h3 = (char)1003; // überschrift
            const char h2 = (char)1002; // überschrift
            const char h1 = (char)1001; // überschrift
            const char h7 = (char)1007; // bold

            if (_Format == enDataFormat.Text_mit_Formatierung) { TXT = TXT.HTMLSpecialToNormalChar(); }

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
                // 180°C


                // Nach Zahlen KEINE leerzeichen einfügen. Es gibgt so viele dinge.... 90er Schichtsalat



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
                TXT = TXT.Replace(h3 + " ", " " + h3); // Überschrift, nach Rechts
                TXT = TXT.Replace(h2 + " ", " " + h2); // Überschrift, nach Rechts
                TXT = TXT.Replace(h1 + " ", " " + h1); // Überschrift, nach Rechts
                TXT = TXT.Replace(h7 + " ", " " + h7); // Bold, nach Rechts


                TXT = TXT.Replace(h3 + "\r", "\r" + h3); // Überschrift, nach Rechts
                TXT = TXT.Replace(h2 + "\r", "\r" + h2); // Überschrift, nach Rechts
                TXT = TXT.Replace(h1 + "\r", "\r" + h1); // Überschrift, nach Rechts
                TXT = TXT.Replace(h7 + "\r", "\r" + h7); // Bold, nach Rechts

                TXT = TXT.Replace(h7 + h4.ToString(), h4.ToString());
                TXT = TXT.Replace(h3 + h4.ToString(), h4.ToString());
                TXT = TXT.Replace(h2 + h4.ToString(), h4.ToString());
                TXT = TXT.Replace(h1 + h4.ToString(), h4.ToString());
                TXT = TXT.Replace(h4 + h4.ToString(), h4.ToString());


                TXT = TXT.Replace(" °", "°");
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



        public string SimplyFile(string fullFileName) {
            if (_Format != enDataFormat.Link_To_Filesystem) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Nur bei Link_To_Filesystem erlaubt!");
            }

            var tmpfile = fullFileName.FileNameWithoutSuffix();
            if (tmpfile.ToLower() == fullFileName.ToLower()) { return tmpfile; }
            if (BestFile(tmpfile, false).ToLower() == fullFileName.ToLower()) { return tmpfile; }

            tmpfile = fullFileName.FileNameWithSuffix();
            if (tmpfile.ToLower() == fullFileName.ToLower()) { return tmpfile; }
            if (BestFile(tmpfile, false).ToLower() == fullFileName.ToLower()) { return tmpfile; }

            return fullFileName;
        }


        /// <summary>
        /// Gibt den Dateinamen mit Pfad und Suffix zurück, der sich aus dem Standard-Angaben der Zelle und dem hier übergebebenen Dateinamen zusammensetzt.
        /// Existiert in keiner Spalte und auch nicht auf der Festplatte.
        /// </summary>
        public string BestFile() {
            return BestFile(string.Empty, true);
        }

        /// <summary>
        /// Gibt den Dateinamen mit Pfad und Suffix zurück, der sich aus dem Standard-Angaben der Zelle und dem hier übergebebenen Dateinamen zusammensetzt.
        /// </summary>
        /// <param name="filename">Der Dateiname. Ein evtl. fehlender Pfad und ein evtl. fehlendes Suffix werden ergänzt. </param>
        /// <param name="mustBeFree">Wenn True wird ein Dateiname zurückgegeben, der noch nicht im Verzeichnis vorhanden ist.</param>
        /// <returns> Gibt den Dateinamen mit Pfad und Suffix zurück</returns>
        public string BestFile(string filename, bool mustBeFree) {

            if (_Format != enDataFormat.Link_To_Filesystem) { Develop.DebugPrint(enFehlerArt.Fehler, "Nur bei Link_To_Filesystem erlaubt!"); }

            //FileNameWithoutPath = FileNameWithoutPath.RemoveChars(Constants.Char_DateiSonderZeichen); // Falls ein Korrekter Pfad übergeben wurde, würde er hier verstümmelt werden
            if (string.IsNullOrEmpty(filename)) {
                if (!mustBeFree) { return string.Empty; }
                filename = (_Name.Substring(0, 1) + DateTime.Now.ToString("mm.fff")).RemoveChars(Constants.Char_DateiSonderZeichen + ".");
            }

            if (filename.Contains("\r")) { Develop.DebugPrint_NichtImplementiert(); }



            // Wenn FileNameWithoutPath kein Suffix hat, das Standard Suffix hinzufügen
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


            // Ist nur ein Unterferzeichniss angegeben, den Datenbankpfad benutzen und das Unterverzeichniss anhängen
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

                        var r = Database.Row[new FilterItem(columnitem, enFilterType.Istgleich_GroßKleinEgal, tmpname)];
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


        public bool AutoFilterSymbolPossible() {
            if (!FilterOptions.HasFlag(enFilterOptions.Enabled)) { return false; }
            return Format.Autofilter_möglich();
        }


        public List<string> Autofilter_ItemList(FilterCollection vFilter) {
            if (vFilter == null || vFilter.Count < 0) { return Contents(null); }

            var tfilter = new FilterCollection(Database);

            foreach (var ThisFilter in vFilter) {
                if (ThisFilter != null && this != ThisFilter.Column) { tfilter.Add(ThisFilter); }
            }

            return Contents(tfilter);
        }

        public static enEditTypeTable UserEditDialogTypeInTable(ColumnItem vColumn, bool DoDropDown) {
            return UserEditDialogTypeInTable(vColumn.Format, DoDropDown, vColumn.TextBearbeitungErlaubt, vColumn.MultiLine);
        }

        public static enEditTypeTable UserEditDialogTypeInTable(enDataFormat Format, bool DoDropDown, bool KeybordInputAllowed, bool isMultiline) {
            if (!DoDropDown && !KeybordInputAllowed) { return enEditTypeTable.None; }

            switch (Format) {

                case enDataFormat.Bit:
                case enDataFormat.Columns_für_LinkedCellDropdown:
                case enDataFormat.Values_für_LinkedCellDropdown:
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
                        if (!DoDropDown) { return enEditTypeTable.Textfeld; }

                        if (isMultiline) { return enEditTypeTable.Dropdown_Single; }
                        if (KeybordInputAllowed) { return enEditTypeTable.Textfeld_mit_Auswahlknopf; }
                        return enEditTypeTable.Dropdown_Single;
                    }

                    Develop.DebugPrint(Format);
                    return enEditTypeTable.None;

            }

        }

        public void Parse(string ToParse) {
            Develop.DebugPrint(enFehlerArt.Fehler, "Kann nur über die Datenbank geparsed werden.");
        }

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
                List<string> TMP = null;
                if (MultiLine) {
                    TMP = ThisRow.CellGetList(this);
                } else {
                    TMP = new List<string> { ThisRow.CellGetString(this) };
                }

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
        /// Füllt die Ersetzungen mittels eines übergebenen Enums aus.
        /// </summary>
        /// <param name="t">Beispiel: GetType(enDesign)</param>
        /// <param name="ZumDropdownHinzuAb">Erster Wert der Enumeration, der Hinzugefügt werden soll. Inklusive deses Wertes</param>
        /// <param name="ZumDropdownHinzuBis">Letzter Wert der Enumeration, der nicht mehr hinzugefügt wird, also exklusives diese Wertes</param>
        public void GetValuesFromEnum(System.Type t, int ZumDropdownHinzuAb, int ZumDropdownHinzuBis) {
            var NewReplacer = new List<string>();
            var NewAuswahl = new List<string>();
            var items = System.Enum.GetValues(t);

            foreach (var thisItem in items) {

                var te = System.Enum.GetName(t, thisItem);
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
    }
}