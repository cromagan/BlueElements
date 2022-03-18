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
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using static BlueBasics.FileOperations;
using static BlueBasics.Converter;

namespace BlueDatabase {

    public sealed class ColumnItem : IReadableTextWithChanging, IDisposable, IInputFormat {

        #region Fields

        public readonly ListExt<string> AfterEditAutoReplace = new();
        public readonly ListExt<string> DropDownItems = new();
        public readonly ListExt<string?> LinkedCellFilter = new();
        public readonly ListExt<string> OpticalReplace = new();
        public readonly ListExt<string> PermissionGroupsChangeCell = new();
        public readonly ListExt<string> Tags = new();
        public bool? TmpAutoFilterSinnvoll = null;
        public QuickImage? TmpCaptionBitmap;
        public SizeF TmpCaptionTextSize = new(-1, -1);
        public int? TmpColumnContentWidth;
        public int? TmpIfFilterRemoved = null;
        internal List<string>? UcaseNamesSortedByLenght;
        private AdditionalCheck _additionalCheck;
        private string _adminInfo;
        private bool _afterEditAutoCorrect;
        private bool _afterEditDoUCase;
        private bool _afterEditQuickSortRemoveDouble;
        private int _afterEditRunden;
        private AlignmentHorizontal _align;
        private string _allowedChars;
        private string _autoFilterJoker;
        private string _autoRemove;
        private Color _backColor;
        private string _bestFileStandardFolder;
        private string _bestFileStandardSuffix;
        private string _bildCodeConstantHeight;
        private BildTextVerhalten _bildTextVerhalten;
        private string _caption;
        private string _captionBitmapTxt;
        private string _cellInitValue;
        private bool _disposedValue;
        private bool _dropdownAllesAbw�hlenErlaubt;
        private bool _dropdownBearbeitungErlaubt;

        //private bool _ZellenZusammenfassen;
        private long _dropDownKey;

        private bool _dropdownWerteAndererZellenAnzeigen;
        private bool _editTrotzSperreErlaubt;
        private EditTypeFormula _editType;
        private FilterOptions _filterOptions;
        private Color _foreColor;
        private BlueBasics.Enums.DataFormat _format;
        private bool _formatierungErlaubt;
        private string _identifier;
        private bool _ignoreAtRowFilter;

        //private enDauerfilter _AutoFilter_Dauerfilter;
        private long _keyColumnKey;

        private ColumnLineStyle _lineLeft;
        private ColumnLineStyle _lineRight;

        /// <summary>
        /// Die Quell-Spalte (aus der verlinkten Datenbank) ist immer
        /// </summary>
        private long _linkedCellColumnKeyOfLinkedDatabase;

        private string _linkedDatabaseFile;

        private bool _multiLine;
        private string _name;
        private string _prefix;
        private string _quickInfo;
        private string _regex = string.Empty;
        private bool _saveContent;
        private ScriptType _scriptType;
        private bool _showMultiLineInOneLine;
        private bool _showUndo;
        private SortierTyp _sortType;
        private bool _spellCheckingEnabled;
        private string _suffix;
        private bool _textBearbeitungErlaubt;
        private Database? _tmpLinkedDatabase;
        private TranslationType _translate;
        private string _ueberschrift1;
        private string _ueberschrift2;

        private string _ueberschrift3;

        private long _vorschlagsColumn;

        #endregion

        #region Constructors

        public ColumnItem(Database database, long columnkey) {
            Database = database;
            Database.Disposing += Database_Disposing;
            if (columnkey < 0) { Develop.DebugPrint(FehlerArt.Fehler, "ColumnKey <0"); }
            var ex = Database.Column.SearchByKey(columnkey);
            if (ex != null) { Develop.DebugPrint(FehlerArt.Fehler, "Key existiert bereits"); }
            Key = columnkey;

            #region Standard-Werte

            _name = Database.Column.Freename(string.Empty);
            _caption = string.Empty;
            //_CaptionBitmap = null;
            _format = BlueBasics.Enums.DataFormat.Text;
            _lineLeft = ColumnLineStyle.D�nn;
            _lineRight = ColumnLineStyle.Ohne;
            _multiLine = false;
            _quickInfo = string.Empty;
            _ueberschrift1 = string.Empty;
            _ueberschrift2 = string.Empty;
            _ueberschrift3 = string.Empty;
            //_Intelligenter_Multifilter = string.Empty;
            _foreColor = Color.Black;
            _backColor = Color.White;
            _cellInitValue = string.Empty;
            //_linkedCellRowKeyIsInColumn = -1;
            _linkedCellColumnKeyOfLinkedDatabase = -1;
            _sortType = SortierTyp.Original_String;
            //_ZellenZusammenfassen = false;
            _dropDownKey = -1;
            _vorschlagsColumn = -1;
            _align = AlignmentHorizontal.Links;
            _keyColumnKey = -1;
            _editType = EditTypeFormula.Textfeld;
            _identifier = string.Empty;
            _allowedChars = string.Empty;
            _adminInfo = string.Empty;
            _captionBitmapTxt = string.Empty;
            _filterOptions = FilterOptions.Enabled | FilterOptions.TextFilterEnabled | FilterOptions.ExtendedFilterEnabled;
            //_AutofilterErlaubt = true;
            //_AutofilterTextFilterErlaubt = true;
            //_AutoFilterErweitertErlaubt = true;
            _ignoreAtRowFilter = false;
            _dropdownBearbeitungErlaubt = false;
            _dropdownAllesAbw�hlenErlaubt = false;
            _textBearbeitungErlaubt = false;
            _dropdownWerteAndererZellenAnzeigen = false;
            _afterEditQuickSortRemoveDouble = false;
            _afterEditRunden = -1;
            _afterEditAutoCorrect = false;
            _afterEditDoUCase = false;
            _formatierungErlaubt = false;
            _additionalCheck = AdditionalCheck.None;
            _scriptType = ScriptType.undefiniert;
            _autoRemove = string.Empty;
            _autoFilterJoker = string.Empty;
            _saveContent = true;
            //_AutoFilter_Dauerfilter = enDauerfilter.ohne;
            _spellCheckingEnabled = false;
            //_CompactView = true;
            _showUndo = true;
            _translate = TranslationType.Original_Anzeigen;
            _showMultiLineInOneLine = false;
            _editTrotzSperreErlaubt = false;
            _suffix = string.Empty;
            _linkedDatabaseFile = string.Empty;
            _bildTextVerhalten = BildTextVerhalten.Nur_Text;
            _bildCodeConstantHeight = string.Empty;
            _prefix = string.Empty;
            _bestFileStandardSuffix = string.Empty;
            _bestFileStandardFolder = string.Empty;
            UcaseNamesSortedByLenght = null;

            #endregion Standard-Werte

            LinkedCellFilter.Changed += LinkedCellFilters_ListOrItemChanged;
            DropDownItems.Changed += DropDownItems_ListOrItemChanged;
            OpticalReplace.Changed += OpticalReplacer_ListOrItemChanged;
            AfterEditAutoReplace.Changed += AfterEdit_AutoReplace_ListOrItemChanged;
            PermissionGroupsChangeCell.Changed += PermissionGroups_ChangeCell_ListOrItemChanged;
            Tags.Changed += Tags_ListOrItemChanged;
            Invalidate_TmpVariables();
        }

        #endregion

        #region Destructors

        // TODO: Finalizer nur �berschreiben, wenn "Dispose(bool disposing)" Code f�r die Freigabe nicht verwalteter Ressourcen enth�lt
        ~ColumnItem() {
            // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(false);
        }

        #endregion

        #region Events

        public event EventHandler Changed;

        #endregion

        #region Properties

        public AdditionalCheck AdditionalCheck {
            get => _additionalCheck;
            set {
                if (_additionalCheck == value) { return; }
                Database?.AddPending(DatabaseDataType.co_AdditionalCheck, this, ((int)_additionalCheck).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public string AdminInfo {
            get => _adminInfo;
            set {
                if (_adminInfo == value) { return; }
                Database?.AddPending(DatabaseDataType.co_AdminInfo, this, _adminInfo, value, true);
                OnChanged();
            }
        }

        public bool AfterEdit_AutoCorrect {
            get => _afterEditAutoCorrect;
            set {
                if (_afterEditAutoCorrect == value) { return; }
                Database?.AddPending(DatabaseDataType.co_AfterEdit_AutoCorrect, this, _afterEditAutoCorrect.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool AfterEdit_DoUCase {
            get => _afterEditDoUCase;
            set {
                if (_afterEditDoUCase == value) { return; }
                Database?.AddPending(DatabaseDataType.co_AfterEdit_DoUcase, this, _afterEditDoUCase.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool AfterEdit_QuickSortRemoveDouble {
            get => _multiLine && _afterEditQuickSortRemoveDouble;
            set {
                if (_afterEditQuickSortRemoveDouble == value) { return; }
                Database?.AddPending(DatabaseDataType.co_AfterEdit_QuickSortAndRemoveDouble, this, _afterEditQuickSortRemoveDouble.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public int AfterEdit_Runden {
            get => _afterEditRunden;
            set {
                if (_afterEditRunden == value) { return; }
                Database?.AddPending(DatabaseDataType.co_AfterEdit_Runden, this, _afterEditRunden.ToString(), value.ToString(), true);
                OnChanged();
            }
        }

        public AlignmentHorizontal Align {
            get => _align;
            set {
                if (_align == value) { return; }
                Database?.AddPending(DatabaseDataType.co_Align, this, ((int)_align).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public string AllowedChars {
            get => _allowedChars;
            set {
                if (_allowedChars == value) { return; }
                Database?.AddPending(DatabaseDataType.co_AllowedChars, this, _allowedChars, value, true);
                OnChanged();
            }
        }

        public string Am_A_Key_For_Other_Column { get; private set; }

        public string AutoFilterJoker {
            get => _autoFilterJoker;
            set {
                if (_autoFilterJoker == value) { return; }
                Database?.AddPending(DatabaseDataType.co_AutoFilterJoker, this, _autoFilterJoker, value, true);
                OnChanged();
            }
        }

        public string AutoRemove {
            get => _autoRemove;
            set {
                if (_autoRemove == value) { return; }
                Database?.AddPending(DatabaseDataType.co_AutoRemove, this, _autoRemove, value, true);
                OnChanged();
            }
        }

        public Color BackColor {
            get => _backColor;
            set {
                if (_backColor.ToArgb() == value.ToArgb()) { return; }
                Database?.AddPending(DatabaseDataType.co_BackColor, this, _backColor.ToArgb().ToString(), value.ToArgb().ToString(), true);
                OnChanged();
            }
        }

        public string BestFile_StandardFolder {
            get => _bestFileStandardFolder;
            set {
                if (_bestFileStandardFolder == value) { return; }
                Database?.AddPending(DatabaseDataType.co_BestFile_StandardFolder, this, _bestFileStandardFolder, value, true);
                OnChanged();
            }
        }

        public string BestFile_StandardSuffix {
            get => _bestFileStandardSuffix;
            set {
                if (_bestFileStandardSuffix == value) { return; }
                Database?.AddPending(DatabaseDataType.co_BestFile_StandardSuffix, this, _bestFileStandardSuffix, value, true);
                OnChanged();
            }
        }

        public string BildCode_ConstantHeight {
            get => _bildCodeConstantHeight;
            set {
                if (_bildCodeConstantHeight == value) { return; }
                Database?.AddPending(DatabaseDataType.co_BildCode_ConstantHeight, this, _bildCodeConstantHeight, value, true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public BildTextVerhalten BildTextVerhalten {
            get => _bildTextVerhalten;
            set {
                if (_bildTextVerhalten == value) { return; }
                Database?.AddPending(DatabaseDataType.co_BildTextVerhalten, this, ((int)_bildTextVerhalten).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public string Caption {
            get => _caption;
            set {
                value = value.Replace("<br>", "\r", RegexOptions.IgnoreCase);
                if (_caption == value) { return; }
                Database?.AddPending(DatabaseDataType.co_Caption, this, _caption, value, true);
                Invalidate_TmpVariables();
                OnChanged();
            }
        }

        public string CaptionBitmap {
            get => _captionBitmapTxt;
            set {
                if (_captionBitmapTxt == value) { return; }
                Database?.AddPending(DatabaseDataType.co_CaptionBitmapTXT, this, _captionBitmapTxt, value, false);
                _captionBitmapTxt = value;
                Invalidate_TmpVariables();
                OnChanged();
            }
        }

        public string CellInitValue {
            get => _cellInitValue;
            set {
                if (_cellInitValue == value) { return; }
                Database?.AddPending(DatabaseDataType.co_CellInitValue, this, _cellInitValue, value, true);
                OnChanged();
            }
        }

        public Database? Database { get; private set; }

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

        public bool DropdownAllesAbw�hlenErlaubt {
            get => _dropdownAllesAbw�hlenErlaubt;
            set {
                if (_dropdownAllesAbw�hlenErlaubt == value) { return; }
                Database?.AddPending(DatabaseDataType.co_DropdownAllesAbw�hlenErlaubt, this, _dropdownAllesAbw�hlenErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool DropdownBearbeitungErlaubt {
            get => _dropdownBearbeitungErlaubt;
            set {
                if (_dropdownBearbeitungErlaubt == value) { return; }
                Database?.AddPending(DatabaseDataType.co_DropdownBearbeitungErlaubt, this, _dropdownBearbeitungErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
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
        public long DropdownKey {
            get => _dropDownKey;
            set {
                if (_dropDownKey == value) { return; }
                Database?.AddPending(DatabaseDataType.co_DropDownKey, this, _dropDownKey.ToString(), value.ToString(), true);
                OnChanged();
            }
        }

        public bool DropdownWerteAndererZellenAnzeigen {
            get => _dropdownWerteAndererZellenAnzeigen;
            set {
                if (_dropdownWerteAndererZellenAnzeigen == value) { return; }
                Database?.AddPending(DatabaseDataType.co_DropdownWerteAndererZellenAnzeigen, this, _dropdownWerteAndererZellenAnzeigen.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool EditTrotzSperreErlaubt {
            get => _editTrotzSperreErlaubt;
            set {
                if (_editTrotzSperreErlaubt == value) { return; }
                Database?.AddPending(DatabaseDataType.co_EditTrotzSperreErlaubt, this, _editTrotzSperreErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public EditTypeFormula EditType {
            get => _editType;
            set {
                if (_editType == value) { return; }
                Database?.AddPending(DatabaseDataType.co_EditType, this, ((int)_editType).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public FilterOptions FilterOptions {
            get => _filterOptions;
            set {
                if (_filterOptions == value) { return; }
                Database?.AddPending(DatabaseDataType.co_FilterOptions, this, ((int)_filterOptions).ToString(), ((int)value).ToString(), true);
                Invalidate_TmpVariables();
                OnChanged();
            }
        }

        public Color ForeColor {
            get => _foreColor;
            set {
                if (_foreColor.ToArgb() == value.ToArgb()) { return; }
                Database?.AddPending(DatabaseDataType.co_ForeColor, this, _foreColor.ToArgb().ToString(), value.ToArgb().ToString(), true);
                OnChanged();
            }
        }

        public BlueBasics.Enums.DataFormat Format {
            get => _format;
            set {
                if (_format == value) { return; }
                Database?.AddPending(DatabaseDataType.co_Format, this, ((int)_format).ToString(), ((int)value).ToString(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public bool FormatierungErlaubt {
            get => _formatierungErlaubt;
            set {
                if (_formatierungErlaubt == value) { return; }
                Database?.AddPending(DatabaseDataType.co_FormatierungErlaubt, this, _formatierungErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public string Identifier {
            get => _identifier;
            set {
                if (_identifier == value) { return; }
                Database?.AddPending(DatabaseDataType.co_Identifier, this, _identifier, value, true);
                OnChanged();
            }
        }

        public bool IgnoreAtRowFilter {
            get => !_format.Autofilter_m�glich() || _ignoreAtRowFilter;
            set {
                if (_ignoreAtRowFilter == value) { return; }
                Database?.AddPending(DatabaseDataType.co_BeiZeilenfilterIgnorieren, this, _ignoreAtRowFilter.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public long Key { get; }

        /// <summary>
        /// H�lt Werte, dieser Spalte gleich, bezugnehmend der KeyColumn(key)
        /// </summary>
        public long KeyColumnKey {
            get => _keyColumnKey;
            set {
                if (_keyColumnKey == value) { return; }
                var c = Database?.Column.SearchByKey(_keyColumnKey);
                c?.CheckIfIAmAKeyColumn();
                Database?.AddPending(DatabaseDataType.co_KeyColumnKey, this, _keyColumnKey.ToString(), value.ToString(), true);
                c = Database?.Column.SearchByKey(_keyColumnKey);
                c?.CheckIfIAmAKeyColumn();
                OnChanged();
            }
        }

        public ColumnLineStyle LineLeft {
            get => _lineLeft;
            set {
                if (_lineLeft == value) { return; }
                Database?.AddPending(DatabaseDataType.co_LineLeft, this, ((int)_lineLeft).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public ColumnLineStyle LineRight {
            get => _lineRight;
            set {
                if (_lineRight == value) { return; }
                Database?.AddPending(DatabaseDataType.co_LinieRight, this, ((int)_lineRight).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        /// <summary>
        /// Die Quell-Spalte (aus der verlinkten Datenbank) ist immer
        /// </summary>
        public long LinkedCell_ColumnKeyOfLinkedDatabase {
            get => _linkedCellColumnKeyOfLinkedDatabase;
            set {
                if (_linkedCellColumnKeyOfLinkedDatabase == value) { return; }
                Database?.AddPending(DatabaseDataType.co_LinkedCell_ColumnKeyOfLinkedDatabase, this, _linkedCellColumnKeyOfLinkedDatabase.ToString(), value.ToString(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public Database? LinkedDatabase {
            get {
                if (_tmpLinkedDatabase != null) { return _tmpLinkedDatabase; }
                if (string.IsNullOrEmpty(_linkedDatabaseFile)) { return null; }

                Tmp_LinkedDatabase = _linkedDatabaseFile.Contains(@"\")
                    ? Database.GetByFilename(_linkedDatabaseFile, true, false)
                    : Database.GetByFilename(Database.Filename.FilePath() + _linkedDatabaseFile, true, false);

                if (_tmpLinkedDatabase != null) { _tmpLinkedDatabase.UserGroup = Database.UserGroup; }
                return _tmpLinkedDatabase;
            }
        }

        public string LinkedDatabaseFile {
            get => _linkedDatabaseFile;
            set {
                if (_linkedDatabaseFile == value) { return; }
                Database?.AddPending(DatabaseDataType.co_LinkedDatabase, this, _linkedDatabaseFile, value, true);
                Invalidate_TmpVariables();
                OnChanged();
            }
        }

        public bool MultiLine {
            get => _multiLine;
            set {
                if (!_format.MultilinePossible()) { value = false; }
                if (_multiLine == value) { return; }
                Database?.AddPending(DatabaseDataType.co_MultiLine, this, _multiLine.ToPlusMinus(), value.ToPlusMinus(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public string Name {
            get => _name.ToUpper();
            set {
                value = value.ToUpper();
                if (value == _name.ToUpper()) { return; }
                if (Database?.Column.Exists(value) != null) { return; }
                if (string.IsNullOrEmpty(value)) { return; }
                var old = _name;
                Database?.AddPending(DatabaseDataType.co_Name, this, _name, value, true);
                Database?.Column_NameChanged(old, this);
                OnChanged();
            }
        }

        public string Prefix {
            get => _prefix;
            set {
                if (_prefix == value) { return; }
                Database?.AddPending(DatabaseDataType.co_Prefix, this, _prefix, value, true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public string Quickinfo {
            get => _quickInfo;
            set {
                if (_quickInfo == value) { return; }
                Database?.AddPending(DatabaseDataType.co_QuickInfo, this, _quickInfo, value, true);
                OnChanged();
            }
        }

        public string Regex {
            get => _regex;
            set {
                if (_regex == value) { return; }
                Database?.AddPending(DatabaseDataType.co_Regex, this, _regex, value, true);
                OnChanged();
            }
        }

        public bool SaveContent {
            get => _saveContent;
            set {
                if (_saveContent == value) { return; }
                Database?.AddPending(DatabaseDataType.co_SaveContent, this, _saveContent.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public ScriptType ScriptType {
            get => _scriptType;
            set {
                if (_scriptType == value) { return; }
                Database?.AddPending(DatabaseDataType.co_ScriptType, this, ((int)_scriptType).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public bool ShowMultiLineInOneLine {
            get => _multiLine && _showMultiLineInOneLine;
            set {
                if (_showMultiLineInOneLine == value) { return; }
                Database?.AddPending(DatabaseDataType.co_ShowMultiLineInOneLine, this, _showMultiLineInOneLine.ToPlusMinus(), value.ToPlusMinus(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public bool ShowUndo {
            get => _showUndo;
            set {
                if (_showUndo == value) { return; }
                Database?.AddPending(DatabaseDataType.co_ShowUndo, this, _showUndo.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public SortierTyp SortType {
            get => _sortType;
            set {
                if (_sortType == value) { return; }
                Database?.AddPending(DatabaseDataType.co_SortType, this, ((int)_sortType).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public bool SpellChecking {
            get => _spellCheckingEnabled;
            set {
                if (_spellCheckingEnabled == value) { return; }
                Database?.AddPending(DatabaseDataType.co_SpellCheckingEnabled, this, _spellCheckingEnabled.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        /// <summary>
        /// Was in Textfeldern oder Datenbankzeilen f�r ein Suffix angezeigt werden soll. Beispiel: mm
        /// </summary>
        public string Suffix {
            get => _suffix;
            set {
                if (_suffix == value) { return; }
                Database?.AddPending(DatabaseDataType.co_Suffix, this, _suffix, value, true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public bool TextBearbeitungErlaubt {
            get => Database?.PowerEdit.Subtract(DateTime.Now).TotalSeconds > 0 || _textBearbeitungErlaubt;
            set {
                if (_textBearbeitungErlaubt == value) { return; }
                Database?.AddPending(DatabaseDataType.co_TextBearbeitungErlaubt, this, _textBearbeitungErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public TranslationType Translate {
            get => _translate;
            set {
                if (_translate == value) { return; }
                Database?.AddPending(DatabaseDataType.co_Translate, this, ((int)_translate).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public string Ueberschrift1 {
            get => _ueberschrift1;
            set {
                if (_ueberschrift1 == value) { return; }
                Database?.AddPending(DatabaseDataType.co_Ueberschrift1, this, _ueberschrift1, value, true);
                OnChanged();
            }
        }

        public string Ueberschrift2 {
            get => _ueberschrift2;
            set {
                if (_ueberschrift2 == value) { return; }
                Database?.AddPending(DatabaseDataType.co_Ueberschrift2, this, _ueberschrift2, value, true);
                OnChanged();
            }
        }

        public string Ueberschrift3 {
            get => _ueberschrift3;
            set {
                if (_ueberschrift3 == value) { return; }
                Database?.AddPending(DatabaseDataType.co_Ueberschrift3, this, _ueberschrift3, value, true);
                OnChanged();
            }
        }

        public string Ueberschriften {
            get {
                var txt = _ueberschrift1 + "/" + _ueberschrift2 + "/" + _ueberschrift3;
                return txt == "//" ? "###" : txt.TrimEnd("/");
            }
        }

        public long VorschlagsColumn {
            get => _vorschlagsColumn;
            set {
                if (_vorschlagsColumn == value) { return; }
                Database?.AddPending(DatabaseDataType.co_VorschlagColumn, this, _vorschlagsColumn.ToString(), value.ToString(), true);
                OnChanged();
            }
        }

        private Database Tmp_LinkedDatabase {
            set {
                if (value == _tmpLinkedDatabase) { return; }
                Invalidate_TmpVariables();
                _tmpLinkedDatabase = value;
                if (_tmpLinkedDatabase != null) {
                    _tmpLinkedDatabase.ConnectedControlsStopAllWorking += _TMP_LinkedDatabase_ConnectedControlsStopAllWorking;
                    _tmpLinkedDatabase.Cell.CellValueChanged += _TMP_LinkedDatabase_Cell_CellValueChanged;
                    _tmpLinkedDatabase.Disposing += _TMP_LinkedDatabase_Disposing;
                }
            }
        }

        #endregion

        #region Methods

        public static EditTypeTable UserEditDialogTypeInTable(BlueBasics.Enums.DataFormat format, bool doDropDown, bool keybordInputAllowed, bool isMultiline) {
            if (!doDropDown && !keybordInputAllowed) { return EditTypeTable.None; }

            switch (format) {
                case BlueBasics.Enums.DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems:
                    return EditTypeTable.Dropdown_Single;

                case BlueBasics.Enums.DataFormat.Link_To_Filesystem:
                    return EditTypeTable.FileHandling_InDateiSystem;

                case BlueBasics.Enums.DataFormat.FarbeInteger:
                    if (doDropDown) { return EditTypeTable.Dropdown_Single; }
                    return EditTypeTable.Farb_Auswahl_Dialog;

                case BlueBasics.Enums.DataFormat.Schrift:
                    if (doDropDown) { return EditTypeTable.Dropdown_Single; }
                    return EditTypeTable.Font_AuswahlDialog;

                case BlueBasics.Enums.DataFormat.Button:
                    return EditTypeTable.None;

                default:
                    if (format.TextboxEditPossible()) {
                        return !doDropDown
                            ? EditTypeTable.Textfeld
                            : isMultiline
                            ? EditTypeTable.Dropdown_Single
                            : keybordInputAllowed ? EditTypeTable.Textfeld_mit_Auswahlknopf : EditTypeTable.Dropdown_Single;
                    }
                    Develop.DebugPrint(format);
                    return EditTypeTable.None;
            }
        }

        public static EditTypeTable UserEditDialogTypeInTable(ColumnItem? column, bool doDropDown) => UserEditDialogTypeInTable(column.Format, doDropDown, column.TextBearbeitungErlaubt, column.MultiLine);

        public string AutoCorrect(string value) {
            if (Format == BlueBasics.Enums.DataFormat.Link_To_Filesystem) {
                List<string> l = new(value.SplitAndCutByCr());
                var l2 = l.Select(thisFile => SimplyFile(thisFile)).ToList();
                value = l2.SortedDistinctList().JoinWithCr();
            }
            if (_afterEditDoUCase) { value = value.ToUpper(); }
            if (!string.IsNullOrEmpty(_autoRemove)) { value = value.RemoveChars(_autoRemove); }
            if (AfterEditAutoReplace.Count > 0) {
                List<string> l = new(value.SplitAndCutByCr());
                foreach (var thisar in AfterEditAutoReplace) {
                    var rep = thisar.SplitAndCutBy("|");
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
                            if (string.Equals(l[z], rep[0], StringComparison.CurrentCultureIgnoreCase)) { l[z] = r; }
                        }
                    }
                }
                value = l.JoinWithCr();
            }
            if (_afterEditAutoCorrect) { value = KleineFehlerCorrect(value); }
            if (_afterEditRunden > -1 && DoubleTryParse(value, out var erg)) {
                erg = Math.Round(erg, _afterEditRunden);
                value = erg.ToString(CultureInfo.InvariantCulture);
            }
            if (_afterEditQuickSortRemoveDouble) {
                var l = new List<string>(value.SplitAndCutByCr()).SortedDistinctList();
                value = l.JoinWithCr();
            }
            return value;
        }

        public List<string> Autofilter_ItemList(FilterCollection? filter, List<RowItem?> pinned) {
            if (filter == null || filter.Count < 0) { return Contents((FilterCollection)null, pinned); }
            FilterCollection tfilter = new(Database);
            foreach (var thisFilter in filter.Where(thisFilter => thisFilter != null && this != thisFilter.Column)) {
                tfilter.Add(thisFilter);
            }
            return Contents(tfilter, pinned);
        }

        public bool AutoFilterSymbolPossible() => FilterOptions.HasFlag(FilterOptions.Enabled) && Format.Autofilter_m�glich();

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
            if (_format != BlueBasics.Enums.DataFormat.Link_To_Filesystem) { Develop.DebugPrint(FehlerArt.Fehler, "Nur bei Link_To_Filesystem erlaubt!"); }

            if (string.IsNullOrEmpty(filename)) {
                if (!mustBeFree) { return string.Empty; }
                filename = (_name.Substring(0, 1) + DateTime.Now.ToString("mm.fff")).RemoveChars(Constants.Char_DateiSonderZeichen + ".");
            }
            if (filename.Contains("\r")) { Develop.DebugPrint_NichtImplementiert(); }

            // Wenn FileNameWithoutPath kein Suffix hat, das Standard Suffix hinzuf�gen
            var suffix = filename.FileSuffix();
            var cleanfilename = filename;
            if (string.IsNullOrEmpty(suffix)) {
                suffix = _bestFileStandardSuffix;
            } else {
                cleanfilename = filename.FileNameWithoutSuffix();
            }
            // Den Standardfolder benutzen. Falls dieser fehlt, 'Files' benutzen.
            var directory = _bestFileStandardFolder.Trim("\\");
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
                    if (columnitem.Format == BlueBasics.Enums.DataFormat.Link_To_Filesystem) {
                        var r = Database.Row[new FilterItem(columnitem, FilterType.Istgleich_Gro�KleinEgal, tmpname)];
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

        /// <summary>
        /// �berschreibt alle Spalteeigenschaften mit dem der Vorlage.
        /// Nur der Name bleibt unver�ndert.
        /// </summary>
        /// <param name="source"></param>
        public void CloneFrom(ColumnItem? source) {
            Caption = source.Caption;
            CaptionBitmap = source.CaptionBitmap;
            Format = source.Format;
            LineLeft = source.LineLeft;
            LineRight = source.LineRight;
            MultiLine = source.MultiLine;
            Quickinfo = source.Quickinfo;
            ForeColor = source.ForeColor;
            BackColor = source.BackColor;
            EditTrotzSperreErlaubt = source.EditTrotzSperreErlaubt;
            EditType = source.EditType;
            Identifier = source.Identifier;
            PermissionGroupsChangeCell.Clear();
            PermissionGroupsChangeCell.AddRange(source.PermissionGroupsChangeCell);
            Tags.Clear();
            Tags.AddRange(source.Tags);
            AdminInfo = source.AdminInfo;
            FilterOptions = source.FilterOptions;
            IgnoreAtRowFilter = source.IgnoreAtRowFilter;
            DropdownBearbeitungErlaubt = source.DropdownBearbeitungErlaubt;
            DropdownAllesAbw�hlenErlaubt = source.DropdownAllesAbw�hlenErlaubt;
            TextBearbeitungErlaubt = source.TextBearbeitungErlaubt;
            SpellChecking = source.SpellChecking;
            DropdownWerteAndererZellenAnzeigen = source.DropdownWerteAndererZellenAnzeigen;
            AfterEdit_QuickSortRemoveDouble = source.AfterEdit_QuickSortRemoveDouble;
            AfterEdit_Runden = source.AfterEdit_Runden;
            AfterEdit_DoUCase = source.AfterEdit_DoUCase;
            AfterEdit_AutoCorrect = source.AfterEdit_AutoCorrect;
            AutoRemove = source.AutoRemove;
            SaveContent = source.SaveContent;
            CellInitValue = source.CellInitValue;
            AutoFilterJoker = source.AutoFilterJoker;
            KeyColumnKey = source.KeyColumnKey;
            //LinkedCell_RowKeyIsInColumn = source.LinkedCell_RowKeyIsInColumn;
            LinkedCell_ColumnKeyOfLinkedDatabase = source.LinkedCell_ColumnKeyOfLinkedDatabase;
            DropdownKey = source.DropdownKey;
            VorschlagsColumn = source.VorschlagsColumn;
            Align = source.Align;
            SortType = source.SortType;
            DropDownItems.Clear();
            DropDownItems.AddRange(source.DropDownItems);
            LinkedCellFilter.Clear();
            LinkedCellFilter.AddRange(source.LinkedCellFilter);
            OpticalReplace.Clear();
            OpticalReplace.AddRange(source.OpticalReplace);
            AfterEditAutoReplace.Clear();
            AfterEditAutoReplace.AddRange(source.OpticalReplace);
            this.GetStyleFrom(source); // regex, Allowed Chars, etc.
            ScriptType = source.ScriptType;
            ShowUndo = source.ShowUndo;
            ShowMultiLineInOneLine = source.ShowMultiLineInOneLine;
            Ueberschrift1 = source.Ueberschrift1;
            Ueberschrift2 = source.Ueberschrift2;
            Ueberschrift3 = source.Ueberschrift3;
            //LinkedKeyKennung = source.LinkedKeyKennung;
            LinkedDatabaseFile = source.LinkedDatabaseFile;
            BildTextVerhalten = source.BildTextVerhalten;
            BildCode_ConstantHeight = source.BildCode_ConstantHeight;
            BestFile_StandardSuffix = source.BestFile_StandardSuffix;
            BestFile_StandardFolder = source.BestFile_StandardFolder;
        }

        public string CompareKey() {
            var tmp = string.IsNullOrEmpty(_caption) ? _name + Constants.FirstSortChar + _name : _caption + Constants.FirstSortChar + _name;
            tmp = tmp.Trim(' ');
            tmp = tmp.TrimStart('-');
            tmp = tmp.Trim(' ');
            return tmp;
        }

        public List<string> Contents() => Contents((FilterCollection)null, null);

        public List<string> Contents(List<FilterItem>? fi, List<RowItem>? pinned) {
            if (fi == null || fi.Count == 0) { return Contents((FilterCollection)null, null); }
            var ficol = new FilterCollection(fi[0].Database);
            ficol.AddRange(fi);
            return Contents(ficol, pinned);
        }

        public List<string> Contents(FilterItem fi, List<RowItem?> pinned) {
            var x = new FilterCollection(fi.Database) { fi };
            return Contents(x, pinned);
        }

        public List<string> Contents(FilterCollection? filter, List<RowItem>? pinned) {
            List<string> list = new();
            foreach (var thisRowItem in Database.Row) {
                if (thisRowItem != null) {
                    var add = thisRowItem.MatchesTo(filter);
                    if (!add && pinned != null) { add = pinned.Contains(thisRowItem); }
                    if (add) {
                        if (_multiLine) {
                            list.AddRange(thisRowItem.CellGetList(this));
                        } else {
                            if (thisRowItem.CellGetString(this).Length > 0) {
                                list.Add(thisRowItem.CellGetString(this));
                            }
                        }
                    }
                }
            }
            return list.SortedDistinctList();
        }

        public void DeleteContents(FilterCollection? filter, List<RowItem?> pinned) {
            foreach (var thisRowItem in Database.Row) {
                if (thisRowItem != null) {
                    if (thisRowItem.MatchesTo(filter) || pinned.Contains(thisRowItem)) { thisRowItem.CellSet(this, ""); }
                }
            }
        }

        public void Dispose() {
            // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string ErrorReason() {
            if (Key < 0) { return "Interner Fehler: ID nicht definiert"; }
            if (string.IsNullOrEmpty(_name)) { return "Der Spaltenname ist nicht definiert."; }
            if (!Name.ContainsOnlyChars(Constants.AllowedCharsVariableName)) { return "Der Spaltenname enth�lt ung�ltige Zeichen. Erlaubt sind A-Z, 0-9 und _"; }

            //if (!Constants.Char_AZ.Contains(Name.Substring(0, 1).ToUpper())) { return "Der Spaltenname muss mit einem Buchstaben beginnen."; }

            if (Database.Column.Any(thisColumn => thisColumn != this && thisColumn != null && string.Equals(_name, thisColumn.Name, StringComparison.CurrentCultureIgnoreCase))) {
                return "Spalten-Name bereits vorhanden.";
            }

            if (string.IsNullOrEmpty(_caption)) { return "Spalten Beschriftung fehlt."; }
            if (!_saveContent && string.IsNullOrEmpty(_identifier)) { return "Inhalt der Spalte muss gespeichert werden."; }
            if (!_saveContent && _showUndo) { return "Wenn der Inhalt der Spalte nicht gespeichert wird, darf auch kein Undo geloggt werden."; }
            if (((int)_format).ToString() == _format.ToString()) { return "Format fehlerhaft."; }
            if (_format.NeedTargetDatabase()) {
                if (LinkedDatabase == null) { return "Verkn�pfte Datenbank fehlt oder existiert nicht."; }
                if (LinkedDatabase == Database) { return "Zirkelbezug mit verkn�pfter Datenbank."; }
            }
            if (!_format.Autofilter_m�glich() && _filterOptions != FilterOptions.None) { return "Bei diesem Format keine Filterung erlaubt."; }
            if (_filterOptions != FilterOptions.None && !_filterOptions.HasFlag(FilterOptions.Enabled)) { return "Filter Kombination nicht m�glich."; }
            if (_filterOptions != FilterOptions.Enabled_OnlyAndAllowed && _filterOptions.HasFlag(FilterOptions.OnlyAndAllowed)) { return "Filter Kombination nicht m�glich."; }
            if (_filterOptions != FilterOptions.Enabled_OnlyOrAllowed && _filterOptions.HasFlag(FilterOptions.OnlyOrAllowed)) { return "Filter Kombination nicht m�glich."; }
            if (_filterOptions.HasFlag(FilterOptions.OnlyAndAllowed) || _filterOptions.HasFlag(FilterOptions.OnlyOrAllowed)) {
                if (!_multiLine) {
                    return "Dieser Filter kann nur bei Mehrzeiligen Spalten benutzt werden.";
                }
            }
            switch (_format) {
                case BlueBasics.Enums.DataFormat.RelationText:
                    if (!_multiLine) { return "Bei diesem Format muss mehrzeilig ausgew�hlt werden."; }
                    if (_keyColumnKey > -1) { return "Diese Format darf keine Verkn�pfung zu einer Schl�sselspalte haben."; }
                    if (IsFirst()) { return "Diese Format ist bei der ersten (intern) erste Spalte nicht erlaubt."; }
                    if (!string.IsNullOrEmpty(_cellInitValue)) { return "Diese Format kann keinen Initial-Text haben."; }
                    if (_vorschlagsColumn > 0) { return "Diese Format kann keine Vorschlags-Spalte haben."; }
                    break;

                //case enDataFormat.Verkn�pfung_zu_anderer_Datenbank_Skriptgesteuert:
                //    //case enDataFormat.Verkn�pfung_zu_anderer_Datenbank:
                //    if (!string.IsNullOrEmpty(_cellInitValue)) { return "Dieses Format kann keinen Initial-Text haben."; }
                //    if (_keyColumnKey > -1) { return "Dieses Format darf keine Verkn�pfung zu einer Schl�sselspalte haben."; }
                //    if (IsFirst()) { return "Dieses Format ist bei der ersten (intern) erste Spalte nicht erlaubt."; }
                //    if (_linkedCellRowKeyIsInColumn is < 0 and not (-9999)) { return "Die Angabe der Spalte, aus der der Schl�sselwert geholt wird, fehlt."; }
                //    if (_vorschlagsColumn > 0) { return "Dieses Format kann keine Vorschlags-Spalte haben."; }
                //    if (_linkedCellRowKeyIsInColumn >= 0) {
                //        var c = LinkedDatabase().Column.SearchByKey(_linkedCellColumnKeyOfLinkedDatabase);
                //        if (c == null) { return "Die verkn�pfte Spalte existiert nicht."; }
                //        //this.GetStyleFrom(c);
                //        //BildTextVerhalten = c.BildTextVerhalten;

                //        //_MultiLine = c.MultiLine;// != ) { return "Multiline stimmt nicht mit der Ziel-Spalte Multiline �berein"; }
                //        //} else {
                //        //    if (!_MultiLine) { return "Dieses Format muss mehrzeilig sein, da es von der Ziel-Spalte gesteuert wird."; }
                //    }
                //    break;

                case BlueBasics.Enums.DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems:
                    //Develop.DebugPrint("Values_f�r_LinkedCellDropdown Verwendung bei:" + Database.Filename); //TODO: 29.07.2021 Values_f�r_LinkedCellDropdown Format entfernen
                    if (!string.IsNullOrEmpty(_cellInitValue)) { return "Dieses Format kann keinen Initial-Text haben."; }
                    if (KeyColumnKey > -1) { return "Dieses Format darf keine Verkn�pfung zu einer Schl�sselspalte haben."; }
                    if (_vorschlagsColumn > 0) { return "Dieses Format kann keine Vorschlags-Spalte haben."; }
                    break;

                case BlueBasics.Enums.DataFormat.Link_To_Filesystem:
                    if (!string.IsNullOrEmpty(_cellInitValue)) { return "Dieses Format kann keinen Initial-Text haben."; }
                    if (_multiLine && !_afterEditQuickSortRemoveDouble) { return "Format muss sortiert werden."; }
                    if (_vorschlagsColumn > 0) { return "Dieses Format kann keine Vorschlags-Spalte haben."; }
                    if (!string.IsNullOrEmpty(_autoRemove)) { return "Dieses Format darf keine Autokorrektur-Ma�nahmen haben haben."; }
                    if (_afterEditAutoCorrect) { return "Dieses Format darf keine Autokorrektur-Ma�nahmen haben haben."; }
                    if (_afterEditDoUCase) { return "Dieses Format darf keine Autokorrektur-Ma�nahmen haben haben."; }
                    if (_afterEditRunden != -1) { return "Dieses Format darf keine Autokorrektur-Ma�nahmen haben haben."; }
                    if (AfterEditAutoReplace.Count > 0) { return "Dieses Format darf keine Autokorrektur-Ma�nahmen haben haben."; }
                    break;
            }

            if (_multiLine) {
                if (!_format.MultilinePossible()) { return "Format unterst�tzt keine mehrzeiligen Texte."; }
                if (_afterEditRunden != -1) { return "Runden nur bei einzeiligen Texten m�glich"; }
            } else {
                if (_showMultiLineInOneLine) { return "Wenn mehrzeilige Texte einzeilig dargestellt werden sollen, muss mehrzeilig angew�hlt sein."; }
                if (_afterEditQuickSortRemoveDouble) { return "Sortierung kann nur bei mehrzeiligen Feldern erfolgen."; }
            }
            //if (_SpellCheckingEnabled && !_Format.SpellCheckingPossible()) { return "Rechtschreibpr�fung bei diesem Format nicht m�glich."; }
            if (_editTrotzSperreErlaubt && !_textBearbeitungErlaubt && !_dropdownBearbeitungErlaubt) { return "Wenn die Zeilensperre ignoriert werden soll, muss eine Bearbeitungsmethode definiert sein."; }
            var tmpEditDialog = UserEditDialogTypeInTable(_format, false, true, _multiLine);
            if (_textBearbeitungErlaubt) {
                if (tmpEditDialog == EditTypeTable.Dropdown_Single) { return "Format unterst�tzt nur Dropdown-Men�."; }
                if (tmpEditDialog == EditTypeTable.None) { return "Format unterst�tzt keine Standard-Bearbeitung."; }
            } else {
                if (_vorschlagsColumn > -1) { return "'Vorschlags-Text-Spalte' nur bei Texteingabe m�glich."; }
                //if (!string.IsNullOrEmpty(_AllowedChars)) { return "'Erlaubte Zeichen' nur bei Texteingabe n�tig."; }
            }
            if (_dropdownBearbeitungErlaubt) {
                //if (_SpellCheckingEnabled) { return "Entweder Dropdownmen� oder Rechtschreibpr�fung."; }
                if (tmpEditDialog == EditTypeTable.None) { return "Format unterst�tzt keine Auswahlmen�-Bearbeitung."; }
            }
            if (!_dropdownBearbeitungErlaubt && !_textBearbeitungErlaubt) {
                if (PermissionGroupsChangeCell.Count > 0) { return "Bearbeitungsberechtigungen entfernen, wenn keine Bearbeitung erlaubt ist."; }
            }
            if (!string.IsNullOrEmpty(_cellInitValue)) {
                if (IsFirst()) { return "Die erste Spalte darf keinen InitialWert haben."; }
                if (_vorschlagsColumn > -1) { return "InitialWert und Vorschlagspalten-Initial-Text gemeinsam nicht m�glich"; }
            }
            foreach (var thisS in PermissionGroupsChangeCell) {
                if (thisS.Contains("|")) { return "Unerlaubtes Zeichen bei den Gruppen, die eine Zelle bearbeiten d�rfen."; }
                if (thisS.ToUpper() == "#ADMINISTRATOR") { return "'#Administrator' bei den Bearbeitern entfernen."; }
            }
            if (_dropdownBearbeitungErlaubt || tmpEditDialog == EditTypeTable.Dropdown_Single) {
                if (_format != BlueBasics.Enums.DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) {
                    if (!_dropdownWerteAndererZellenAnzeigen && DropDownItems.Count == 0) { return "Keine Dropdown-Items vorhanden bzw. Alles hinzuf�gen nicht angew�hlt."; }
                }
            } else {
                if (_dropdownWerteAndererZellenAnzeigen) { return "Dropdownmenu nicht ausgew�hlt, 'alles hinzuf�gen' pr�fen."; }
                if (_dropdownAllesAbw�hlenErlaubt) { return "Dropdownmenu nicht ausgew�hlt, 'alles abw�hlen' pr�fen."; }
                if (DropDownItems.Count > 0) { return "Dropdownmenu nicht ausgew�hlt, Dropdown-Items vorhanden."; }
            }
            if (_dropdownWerteAndererZellenAnzeigen && !_format.DropdownItemsOfOtherCellsAllowed()) { return "'Dropdownmenu alles hinzuf�gen' bei diesem Format nicht erlaubt."; }
            if (_dropdownAllesAbw�hlenErlaubt && !_format.DropdownUnselectAllAllowed()) { return "'Dropdownmenu alles abw�hlen' bei diesem Format nicht erlaubt."; }
            if (DropDownItems.Count > 0 && !_format.DropdownItemsAllowed()) { return "Manuelle 'Dropdow-Items' bei diesem Format nicht erlaubt."; }
            if (_bildTextVerhalten != BildTextVerhalten.Nur_Text) {
                //if (_Format is enDataFormat.Text) {
                //    // Performance-Teschnische Gr�nde
                //    _BildTextVerhalten = BildTextVerhalten.Nur_Text;
                //    //return "Bei diesem Format muss das Bild/Text-Verhalten 'Nur Text' sein.";
                //}
            }
            if (!string.IsNullOrEmpty(_suffix)) {
                if (_multiLine) { return "Einheiten und Mehrzeilig darf nicht kombiniert werden."; }
            }
            if (_afterEditRunden > 6) { return "Beim Runden maximal 6 Nachkommastellen m�glich"; }
            if (_filterOptions == FilterOptions.None) {
                if (!string.IsNullOrEmpty(_autoFilterJoker)) { return "Wenn kein Autofilter erlaubt ist, immer anzuzeigende Werte entfernen"; }
            }
            //if (string.IsNullOrEmpty(_linkedKeyKennung) && _format.NeedLinkedKeyKennung()) { return "Spaltenkennung f�r verlinkte Datenbanken fehlt."; }
            if (OpticalReplace.Count > 0) {
                if (_format is not BlueBasics.Enums.DataFormat.Text and
                    not BlueBasics.Enums.DataFormat.RelationText) { return "Format unterst�tzt keine Ersetzungen."; }
                if (_filterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled)) { return "Entweder 'Ersetzungen' oder 'erweiternden Autofilter'"; }
                if (!string.IsNullOrEmpty(_autoFilterJoker)) { return "Entweder 'Ersetzungen' oder 'Autofilter Joker'"; }
            }
            if (_keyColumnKey > -1) {
                if (!string.IsNullOrEmpty(Am_A_Key_For_Other_Column)) { return "Eine Schl�sselspalte darf selbst keine Verkn�pfung zu einer anderen Spalte haben: " + Am_A_Key_For_Other_Column; }
                var c = Database.Column.SearchByKey(_keyColumnKey);
                if (c == null) { return "Die verkn�pfte Schl�sselspalte existiert nicht."; }
            }
            if (IsFirst()) {
                if (_keyColumnKey > -1) { return "Die (intern) erste Spalte darf keine Verkn�pfung zu einer andern Schl�sselspalte haben."; }
            }
            if (_format is not BlueBasics.Enums.DataFormat.Verkn�pfung_zu_anderer_Datenbank and not BlueBasics.Enums.DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) {
                //if (_LinkedCell_RowKeyIsInColumn > -1) { return "Nur verlinkte Zellen k�nnen Daten �ber verlinkte Zellen enthalten."; }
                if (_linkedCellColumnKeyOfLinkedDatabase > -1) { return "Nur verlinkte Zellen k�nnen Daten �ber verlinkte Zellen enthalten."; }
            }
            return string.Empty;
        }

        public bool ExportableTextformatForLayout() => _format.ExportableForLayout();

        public List<string> GetUcaseNamesSortedByLenght() {
            if (UcaseNamesSortedByLenght != null) { return UcaseNamesSortedByLenght; }
            var tmp = Contents((FilterCollection)null, null);
            for (var z = 0; z < tmp.Count; z++) {
                tmp[z] = tmp[z].Length.ToString(Constants.Format_Integer10) + tmp[z].ToUpper();
            }
            tmp.Sort();
            for (var z = 0; z < tmp.Count; z++) {
                tmp[z] = tmp[z].Substring(10);
            }
            UcaseNamesSortedByLenght = tmp;
            return UcaseNamesSortedByLenght;
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
        public void GetUniques(List<RowItem?> rows, out List<string>? einzigartig, out List<string?>? nichtEinzigartig) {
            einzigartig = new List<string>();
            nichtEinzigartig = new List<string?>();
            foreach (var thisRow in rows) {
                var tmp = MultiLine ? thisRow.CellGetList(this) : new List<string?> { thisRow.CellGetString(this) };
                foreach (var thisString in tmp) {
                    if (einzigartig.Contains(thisString)) {
                        nichtEinzigartig.AddIfNotExists(thisString);
                    } else {
                        einzigartig.AddIfNotExists(thisString);
                    }
                }
            }
            einzigartig.RemoveString(nichtEinzigartig, false);
        }

        ///// <summary>
        ///// F�llt die Ersetzungen mittels eines �bergebenen Enums aus.
        ///// </summary>
        ///// <param name="t">Beispiel: GetType(enDesign)</param>
        ///// <param name="ZumDropdownHinzuAb">Erster Wert der Enumeration, der Hinzugef�gt werden soll. Inklusive deses Wertes</param>
        ///// <param name="ZumDropdownHinzuBis">Letzter Wert der Enumeration, der nicht mehr hinzugef�gt wird, also exklusives diese Wertes</param>
        //public void GetValuesFromEnum(Type t, int ZumDropdownHinzuAb, int ZumDropdownHinzuBis) {
        //    List<string> NewReplacer = new();
        //    List<string> NewAuswahl = new();
        //    var items = Enum.GetValues(t);
        //    foreach (var thisItem in items) {
        //        var te = Enum.GetName(t, thisItem);
        //        var th = (int)thisItem;
        //        if (!string.IsNullOrEmpty(te)) {
        //            NewReplacer.Add(th.ToString() + "|" + te);
        //            if (th >= ZumDropdownHinzuAb && th < ZumDropdownHinzuBis) {
        //                NewAuswahl.Add(th.ToString());
        //            }
        //        }
        //    }
        //    NewReplacer.Reverse();
        //    if (OpticalReplace.IsDifferentTo(NewReplacer)) {
        //        OpticalReplace.Clear();
        //        OpticalReplace.AddRange(NewReplacer);
        //    }
        //    if (DropDownItems.IsDifferentTo(NewAuswahl)) {
        //        DropDownItems.Clear();
        //        DropDownItems.AddRange(NewAuswahl);
        //    }
        //}

        public int Index() => Database.Column.IndexOf(this);

        /// <summary>
        /// Der Invalidate, der am meisten invalidiert: Alle tempor�ren Variablen und auch jede Zell-Gr��e der Spalte.
        /// </summary>
        public void Invalidate_ColumAndContent() {
            TmpCaptionTextSize = new SizeF(-1, -1);
            Invalidate_TmpColumnContentWidth();
            Invalidate_TmpVariables();
            foreach (var thisRow in Database.Row) {
                if (thisRow != null) { CellCollection.Invalidate_CellContentSize(this, thisRow); }
            }
            Database.OnViewChanged();
        }

        public bool IsFirst() => Convert.ToBoolean(Database.Column[0] == this);

        public bool IsOk() => string.IsNullOrEmpty(ErrorReason());

        public ColumnItem? Next() {
            var columnCount = Index();
            do {
                columnCount++;
                if (columnCount >= Database.Column.Count) { return null; }
                if (Database.Column[columnCount] != null) { return Database.Column[columnCount]; }
            } while (true);
        }

        public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

        public ColumnItem? Previous() {
            var columnCount = Index();
            do {
                columnCount--;
                if (columnCount < 0) { return null; }
                if (Database.Column[columnCount] != null) { return Database.Column[columnCount]; }
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
        public string QuickInfoText(string additionalText) {
            var T = string.Empty;
            if (!string.IsNullOrEmpty(_quickInfo)) { T += _quickInfo; }
            if (Database.IsAdministrator() && !string.IsNullOrEmpty(_adminInfo)) { T = T + "<br><br><b><u>Administrator-Info:</b></u><br>" + _adminInfo; }
            if (Database.IsAdministrator() && Tags.Count > 0) { T = T + "<br><br><b><u>Spalten-Tags:</b></u><br>" + Tags.JoinWith("<br>"); }
            T = T.Trim();
            T = T.Trim("<br>");
            T = T.Trim();
            if (!string.IsNullOrEmpty(T) && !string.IsNullOrEmpty(additionalText)) {
                T = "<b><u>" + additionalText + "</b></u><br><br>" + T;
            }
            return T;
        }

        public string ReadableText() {
            var ret = _caption;
            if (Database.Column.Any(thisColumnItem => thisColumnItem != null && thisColumnItem != this && string.Equals(thisColumnItem.Caption, _caption, StringComparison.CurrentCultureIgnoreCase))) {
                var done = false;
                if (!string.IsNullOrEmpty(_ueberschrift3)) {
                    ret = _ueberschrift3 + "/" + ret;
                    done = true;
                }
                if (!string.IsNullOrEmpty(_ueberschrift2)) {
                    ret = _ueberschrift2 + "/" + ret;
                    done = true;
                }
                if (!string.IsNullOrEmpty(_ueberschrift1)) {
                    ret = _ueberschrift1 + "/" + ret;
                    done = true;
                }
                if (!done) {
                    ret = _name; //_Caption + " (" + _Name + ")";
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

        public void Repair() {
            // Unbekannt = -1,
            // Nothing = 0,
            //    Text = 1,

            //Bit = 2,

            //// Bin�rdaten_Bild = 19,
            //// Passwort = 20, // String
            ////  Text_Ohne_Kritische_Zeichen = 21,
            //// Bin�rdaten = 39,
            //// Link_To_BlueDataSystem = 42
            //// Telefonnummer = 43, // Spezielle Formate
            //FarbeInteger = 45, // Color

            //// Email = 46, // Spezielle Formate
            //// InternetAdresse = 47, // Spezielle Formate
            //// Relation = 65,
            //// Event = 66,
            //// Tendenz = 67
            //// Einsch�tzung = 68,
            //Schrift = 69,

            //Text_mit_Formatierung = 70,

            //// TextmitFormatierungUndLinkToAnotherDatabase = 71
            //// Relation_And_Event_Mixed = 72,
            //Link_To_Filesystem = 73,

            //LinkedCell = 74,
            //Columns_f�r_LinkedCellDropdown = 75,

            //Values_f�r_LinkedCellDropdown = 76,

            //RelationText = 77,

            //// KeyForSame = 78
            //Button = 79

            //// bis 999 wird gepr�ft
            ///

            CheckIfIAmAKeyColumn();

            try {
                switch ((int)_format) {
                    case (int)BlueBasics.Enums.DataFormat.Button:
                        ScriptType = ScriptType.Nicht_vorhanden;
                        break;

                    case 21: //Text_Ohne_Kritische_Zeichen
                        SetFormatForText();
                        break;

                    case 15:// Date_GermanFormat = 15
                    case 16://Datum_und_Uhrzeit = 16,
                        SetFormatForDateTime();
                        break;

                    case 2:   //Bit = 3,
                        SetFormatForBit();
                        break;

                    case 3:   //Ganzzahl = 3,
                        SetFormatForInteger();
                        break;

                    case 6:  //Gleitkommazahl = 6,
                        SetFormatForFloat();
                        break;

                    case 13:         //BildCode = 13,
                        SetFormatForBildCode();
                        break;

                    case 70:
                        SetFormatForTextMitFormatierung();
                        break;

                    case 74: //(int)enDataFormat.Verkn�pfung_zu_anderer_Datenbank_Skriptgesteuert:

                        //if (LinkedCell_RowKeyIsInColumn != -9999) {
                        _format = BlueBasics.Enums.DataFormat.Verkn�pfung_zu_anderer_Datenbank;
                        LinkedCellFilter.Clear();
                        //LinkedCellFilter.Add(LinkedCell_RowKeyIsInColumn.ToString());
                        //LinkedCell_RowKeyIsInColumn = -1;
                        //}
                        break;

                    case 75:
                        _format = BlueBasics.Enums.DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems;

                        break;
                }

                if (ScriptType == ScriptType.undefiniert) {
                    if (MultiLine) {
                        ScriptType = ScriptType.List;
                    } else if (Format is BlueBasics.Enums.DataFormat.Text or BlueBasics.Enums.DataFormat.Link_To_Filesystem) {
                        if (SortType is SortierTyp.ZahlenwertFloat or SortierTyp.ZahlenwertInt) {
                            ScriptType = ScriptType.Numeral;
                        }
                        ScriptType = ScriptType.String;
                    } else if (Format == BlueBasics.Enums.DataFormat.Schrift) {
                        ScriptType = ScriptType.Nicht_vorhanden;
                    }
                }

                if (_format is BlueBasics.Enums.DataFormat.Verkn�pfung_zu_anderer_Datenbank) {
                    var c = LinkedDatabase?.Column.SearchByKey(_linkedCellColumnKeyOfLinkedDatabase);
                    if (c != null) {
                        this.GetStyleFrom(c);
                        BildTextVerhalten = c.BildTextVerhalten;
                        //ScriptType = c.ScriptType;
                        Translate = c.Translate;
                    }
                }

                if (ScriptType == ScriptType.undefiniert) {
                    Develop.DebugPrint(FehlerArt.Warnung, "Umsetzung fehlgeschlagen: " + Caption + " " + Database.Filename);
                }

                ResetSystemToDefault(false);
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
            }
        }

        public void ResetSystemToDefault(bool setAll) {
            if (string.IsNullOrEmpty(_identifier)) { return; }
            //if (SetAll && !IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Ausserhalb des parsens!"); }
            // ACHTUNG: Die SetAll Befehle OHNE _, die m�ssen geloggt werden.
            if (setAll) {
                LineLeft = ColumnLineStyle.D�nn;
                LineRight = ColumnLineStyle.Ohne;
                ForeColor = Color.FromArgb(0, 0, 0);
                //CaptionBitmap = null;
            }
            switch (_identifier) {
                case "System: Creator":
                    _name = "SYS_Creator";
                    _format = BlueBasics.Enums.DataFormat.Text;
                    if (setAll) {
                        Caption = "Ersteller";
                        DropdownBearbeitungErlaubt = true;
                        DropdownWerteAndererZellenAnzeigen = true;
                        SpellChecking = false;
                        ForeColor = Color.FromArgb(0, 0, 128);
                        BackColor = Color.FromArgb(185, 186, 255);
                    }
                    break;

                case "System: Changer":
                    _name = "SYS_Changer";
                    _format = BlueBasics.Enums.DataFormat.Text;
                    _spellCheckingEnabled = false;
                    _textBearbeitungErlaubt = false;
                    _dropdownBearbeitungErlaubt = false;
                    _showUndo = false;
                    _scriptType = ScriptType.String_Readonly;
                    PermissionGroupsChangeCell.Clear();
                    if (setAll) {
                        Caption = "�nderer";
                        ForeColor = Color.FromArgb(0, 128, 0);
                        BackColor = Color.FromArgb(185, 255, 185);
                    }
                    break;

                case "System: Chapter":
                    _name = "SYS_Chapter";
                    _format = BlueBasics.Enums.DataFormat.Text;
                    _afterEditAutoCorrect = true; // Verhindert \r am Ende und somit anzeigefehler
                    if (setAll) {
                        Caption = "Kapitel";
                        ForeColor = Color.FromArgb(0, 0, 0);
                        BackColor = Color.FromArgb(255, 255, 150);
                        LineLeft = ColumnLineStyle.Dick;
                        if (_multiLine) { _showMultiLineInOneLine = true; }
                    }
                    break;

                case "System: Date Created":
                    _name = "SYS_CreateDate";
                    _spellCheckingEnabled = false;
                    SetFormatForDateTime();
                    if (setAll) {
                        Caption = "Erstell-Datum";
                        ForeColor = Color.FromArgb(0, 0, 128);
                        BackColor = Color.FromArgb(185, 185, 255);
                        LineLeft = ColumnLineStyle.Dick;
                    }
                    break;

                case "System: Date Changed":
                    _name = "SYS_ChangeDate";
                    _spellCheckingEnabled = false;
                    _showUndo = false;
                    // SetFormatForDateTime(); --Sriptt Type Chaos
                    _textBearbeitungErlaubt = false;
                    _spellCheckingEnabled = false;
                    _dropdownBearbeitungErlaubt = false;
                    _scriptType = ScriptType.String_Readonly;
                    PermissionGroupsChangeCell.Clear();
                    if (setAll) {
                        Caption = "�nder-Datum";
                        ForeColor = Color.FromArgb(0, 128, 0);
                        BackColor = Color.FromArgb(185, 255, 185);
                        LineLeft = ColumnLineStyle.Dick;
                    }
                    break;

                case "System: Correct":
                    _name = "SYS_Correct";
                    _caption = "Fehlerfrei";
                    _spellCheckingEnabled = false;
                    _format = BlueBasics.Enums.DataFormat.Text;
                    //_AutoFilterErweitertErlaubt = false;
                    _autoFilterJoker = string.Empty;
                    //_AutofilterTextFilterErlaubt = false;
                    _ignoreAtRowFilter = true;
                    _filterOptions = FilterOptions.Enabled;
                    _scriptType = ScriptType.Nicht_vorhanden;
                    _align = AlignmentHorizontal.Zentriert;
                    DropDownItems.Clear();
                    LinkedCellFilter.Clear();
                    PermissionGroupsChangeCell.Clear();
                    _textBearbeitungErlaubt = false;
                    _dropdownBearbeitungErlaubt = false;

                    if (setAll) {
                        ForeColor = Color.FromArgb(128, 0, 0);
                        BackColor = Color.FromArgb(255, 185, 185);
                        LineLeft = ColumnLineStyle.Dick;
                    }
                    break;

                case "System: Locked":
                    _name = "SYS_Locked";
                    _spellCheckingEnabled = false;
                    _format = BlueBasics.Enums.DataFormat.Text;
                    _scriptType = ScriptType.Bool;
                    _filterOptions = FilterOptions.Enabled;
                    _autoFilterJoker = string.Empty;
                    _ignoreAtRowFilter = true;
                    _bildTextVerhalten = BildTextVerhalten.Interpretiere_Bool;
                    _align = AlignmentHorizontal.Zentriert;

                    if (_textBearbeitungErlaubt || _dropdownBearbeitungErlaubt) {
                        _quickInfo = "Eine abgeschlossene Zeile kann<br>nicht mehr bearbeitet werden.";
                        _textBearbeitungErlaubt = false;
                        _dropdownBearbeitungErlaubt = true;
                        _editTrotzSperreErlaubt = true;
                        DropDownItems.AddIfNotExists("+");
                        DropDownItems.AddIfNotExists("-");
                    } else {
                        DropDownItems.Clear();
                    }

                    if (setAll) {
                        Caption = "Abgeschlossen";
                        ForeColor = Color.FromArgb(128, 0, 0);
                        BackColor = Color.FromArgb(255, 185, 185);
                    }
                    break;

                case "System: State":
                    _name = "SYS_RowState";
                    _caption = "veraltet und kann gel�scht werden: Zeilenstand";
                    _identifier = "";
                    break;

                case "System: ID":
                    _name = "SYS_ID";
                    _caption = "veraltet und kann gel�scht werden: Zeilen-ID";
                    _identifier = "";
                    break;

                case "System: Last Used Layout":
                    _name = "SYS_Layout";
                    _caption = "veraltet und kann gel�scht werden:  Letztes Layout";
                    _identifier = "";
                    break;

                default:
                    Develop.DebugPrint("Unbekannte Kennung: " + _identifier);
                    break;
            }
        }

        public void SetFormat(VarType format) {
            switch (format) {
                case VarType.Text: SetFormatForText(); break;
                case VarType.Date: SetFormatForDate(); break;
                case VarType.DateTime: SetFormatForDateTime(); break;
                case VarType.Email: SetFormatForEmail(); break;
                case VarType.Float: SetFormatForFloat(); break;
                case VarType.Integer: SetFormatForInteger(); break;
                case VarType.PhoneNumber: SetFormatForPhoneNumber(); break;
                case VarType.TextMitFormatierung: SetFormatForTextMitFormatierung(); break;
                case VarType.Url: SetFormatForUrl(); break;
                case VarType.Bit: SetFormatForBit(); break;
                default:
                    Develop.DebugPrint(FehlerArt.Warnung);
                    break;
            }
        }

        public void SetFormatForBildCode() {
            ((IInputFormat)this).SetFormat(VarType.Text); // Standard Verhalten

            Format = BlueBasics.Enums.DataFormat.Text;
            Align = AlignmentHorizontal.Links;
            SortType = SortierTyp.Original_String;
            Translate = TranslationType.Original_Anzeigen;
            AfterEdit_QuickSortRemoveDouble = false;
            BildTextVerhalten = BildTextVerhalten.Bild_oder_Text;
            ScriptType = ScriptType.String;
        }

        public void SetFormatForBit() {
            ((IInputFormat)this).SetFormat(VarType.Bit); // Standard Verhalten

            Format = BlueBasics.Enums.DataFormat.Text;
            Align = AlignmentHorizontal.Zentriert;
            SortType = SortierTyp.Original_String;
            Translate = TranslationType.Original_Anzeigen;
            AfterEdit_QuickSortRemoveDouble = false;
            BildTextVerhalten = BildTextVerhalten.Interpretiere_Bool;
            ScriptType = ScriptType.Bool;

            DropdownAllesAbw�hlenErlaubt = false;
            DropdownBearbeitungErlaubt = true;
            TextBearbeitungErlaubt = false;
            DropDownItems.AddIfNotExists("+");
            DropDownItems.AddIfNotExists("-");
        }

        public void SetFormatForDate() {
            ((IInputFormat)this).SetFormat(VarType.Date);

            Format = BlueBasics.Enums.DataFormat.Text;
            Align = AlignmentHorizontal.Links;
            SortType = SortierTyp.Datum_Uhrzeit;
            Translate = TranslationType.Datum;
            AfterEdit_QuickSortRemoveDouble = false;
            BildTextVerhalten = BildTextVerhalten.Nur_Text;
            ScriptType = ScriptType.String;
        }

        public void SetFormatForDateTime() {
            ((IInputFormat)this).SetFormat(VarType.DateTime);

            Format = BlueBasics.Enums.DataFormat.Text;
            Align = AlignmentHorizontal.Links;
            SortType = SortierTyp.Datum_Uhrzeit;
            Translate = TranslationType.Datum;
            AfterEdit_QuickSortRemoveDouble = false;
            BildTextVerhalten = BildTextVerhalten.Nur_Text;
            ScriptType = ScriptType.String;
        }

        public void SetFormatForEmail() {
            ((IInputFormat)this).SetFormat(VarType.Email);

            Format = BlueBasics.Enums.DataFormat.Text;
            Align = AlignmentHorizontal.Links;
            SortType = SortierTyp.Original_String;
            Translate = TranslationType.Original_Anzeigen;
            AfterEdit_QuickSortRemoveDouble = true;
            BildTextVerhalten = BildTextVerhalten.Nur_Text;
            ScriptType = ScriptType.String;
        }

        public void SetFormatForFloat() {
            ((IInputFormat)this).SetFormat(VarType.Float);

            Format = BlueBasics.Enums.DataFormat.Text;
            Align = AlignmentHorizontal.Rechts;
            Translate = TranslationType.Zahl;
            SortType = SortierTyp.ZahlenwertFloat;
            AfterEdit_QuickSortRemoveDouble = false;
            BildTextVerhalten = BildTextVerhalten.Nur_Text;
            ScriptType = ScriptType.Numeral;
        }

        public void SetFormatForInteger() {
            ((IInputFormat)this).SetFormat(VarType.Integer);

            Format = BlueBasics.Enums.DataFormat.Text;
            Align = AlignmentHorizontal.Rechts;
            SortType = SortierTyp.ZahlenwertInt;
            Translate = TranslationType.Original_Anzeigen;
            AfterEdit_QuickSortRemoveDouble = false;
            BildTextVerhalten = BildTextVerhalten.Nur_Text;
            ScriptType = ScriptType.Numeral;
        }

        public void SetFormatForPhoneNumber() {
            ((IInputFormat)this).SetFormat(VarType.PhoneNumber);

            Format = BlueBasics.Enums.DataFormat.Text;
            Align = AlignmentHorizontal.Links;
            SortType = SortierTyp.Original_String;
            Translate = TranslationType.Original_Anzeigen;
            AfterEdit_QuickSortRemoveDouble = false;
            BildTextVerhalten = BildTextVerhalten.Nur_Text;
            ScriptType = ScriptType.String;
        }

        public void SetFormatForText() {
            ((IInputFormat)this).SetFormat(VarType.Text);

            Format = BlueBasics.Enums.DataFormat.Text;
            Align = AlignmentHorizontal.Links;
            SortType = SortierTyp.Sprachneutral_String;
            Translate = TranslationType.Original_Anzeigen;
            AfterEdit_QuickSortRemoveDouble = false;
            BildTextVerhalten = BildTextVerhalten.Nur_Text;
            ScriptType = ScriptType.String;
        }

        public void SetFormatForTextMitFormatierung() {
            ((IInputFormat)this).SetFormat(VarType.TextMitFormatierung);

            Format = BlueBasics.Enums.DataFormat.Text;
            Align = AlignmentHorizontal.Links;
            SortType = SortierTyp.Sprachneutral_String;
            Translate = TranslationType.Original_Anzeigen;
            AfterEdit_QuickSortRemoveDouble = false;
            BildTextVerhalten = BildTextVerhalten.Nur_Text;
            ScriptType = ScriptType.String;
        }

        public void SetFormatForTextOptions() {
            ((IInputFormat)this).SetFormat(VarType.Text);

            MultiLine = true; // Verhalten von Setformat �berschreiben

            Format = BlueBasics.Enums.DataFormat.Text;
            Align = AlignmentHorizontal.Links;
            SortType = SortierTyp.Sprachneutral_String;
            Translate = TranslationType.�bersetzen;
            AfterEdit_QuickSortRemoveDouble = true;
            BildTextVerhalten = BildTextVerhalten.Nur_Text;
            ScriptType = ScriptType.List;
        }

        public void SetFormatForUrl() {
            ((IInputFormat)this).SetFormat(VarType.Url);

            Format = BlueBasics.Enums.DataFormat.Text;
            Align = AlignmentHorizontal.Links;
            SortType = SortierTyp.Original_String;
            Translate = TranslationType.Original_Anzeigen;
            AfterEdit_QuickSortRemoveDouble = false;
            BildTextVerhalten = BildTextVerhalten.Nur_Text;
            ScriptType = ScriptType.String;
        }

        public string SimplyFile(string fullFileName) {
            if (_format != BlueBasics.Enums.DataFormat.Link_To_Filesystem) {
                Develop.DebugPrint(FehlerArt.Fehler, "Nur bei Link_To_Filesystem erlaubt!");
            }
            var tmpfile = fullFileName.FileNameWithoutSuffix();
            if (string.Equals(tmpfile, fullFileName, StringComparison.CurrentCultureIgnoreCase)) { return tmpfile; }
            if (string.Equals(BestFile(tmpfile, false), fullFileName, StringComparison.CurrentCultureIgnoreCase)) { return tmpfile; }
            tmpfile = fullFileName.FileNameWithSuffix();
            return string.Equals(tmpfile, fullFileName, StringComparison.CurrentCultureIgnoreCase)
                ? tmpfile
                : string.Equals(BestFile(tmpfile, false), fullFileName, StringComparison.CurrentCultureIgnoreCase) ? tmpfile : fullFileName;
        }

        public void Statisik(List<FilterItem>? filter, List<RowItem>? pinnedRows) {
            var r = Database.Row.CalculateVisibleRows(filter, pinnedRows);

            if (r == null || r.Count < 1) { return; }

            var d = new Dictionary<string, int>();

            foreach (var thisRow in r) {
                var keyValue = thisRow.CellGetString(this);
                if (string.IsNullOrEmpty(keyValue)) { keyValue = "[empty]"; }

                keyValue = keyValue.Replace("\r", ";");

                var count = 0;
                if (d.ContainsKey(keyValue)) {
                    d.TryGetValue(keyValue, out count);
                    d.Remove(keyValue);
                }
                count++;
                d.Add(keyValue, count);
            }

            var l = new List<string> {
                "Statisik der vorkommenden Werte der Spalte: " + ReadableText(),
                " - nur aktuell angezeigte Zeilen",
                " - Zelleninhalte werden als ganzes behandelt",
                " "
            };

            do {
                var maxCount = 0;
                var keyValue = string.Empty;

                foreach (var thisKey in d.Where(thisKey => thisKey.Value > maxCount)) {
                    keyValue = thisKey.Key;
                    maxCount = thisKey.Value;
                }

                d.Remove(keyValue);
                l.Add(maxCount + " - " + keyValue);
            } while (d.Count > 0);

            l.Save(TempFile(string.Empty, string.Empty, "txt"), System.Text.Encoding.UTF8, true);
        }

        public double? Summe(FilterCollection? filter) {
            double summ = 0;
            foreach (var thisrow in Database.Row) {
                if (thisrow != null && thisrow.MatchesTo(filter)) {
                    if (!thisrow.CellIsNullOrEmpty(this)) {
                        if (!thisrow.CellGetString(this).IsDouble()) { return null; }
                        summ += thisrow.CellGetDouble(this);
                    }
                }
            }
            return summ;
        }

        public double? Summe(List<RowData?> sort) {
            double summ = 0;
            foreach (var thisrow in sort) {
                if (thisrow != null && !thisrow.Row.CellIsNullOrEmpty(this)) {
                    if (!thisrow.Row.CellGetString(this).IsDouble()) { return null; }
                    summ += thisrow.Row.CellGetDouble(this);
                }
            }
            return summ;
        }

        public QuickImage? SymbolForReadableText() => this == Database.Column.SysRowChanger
? QuickImage.Get(ImageCode.Person)
: this == Database.Column.SysRowCreator
? QuickImage.Get(ImageCode.Person)
: _format switch {
    BlueBasics.Enums.DataFormat.Link_To_Filesystem => QuickImage.Get(ImageCode.Datei, 16),
    BlueBasics.Enums.DataFormat.RelationText => QuickImage.Get(ImageCode.Herz, 16),
    BlueBasics.Enums.DataFormat.FarbeInteger => QuickImage.Get(ImageCode.Pinsel, 16),
    BlueBasics.Enums.DataFormat.Verkn�pfung_zu_anderer_Datenbank => QuickImage.Get(ImageCode.Fernglas, 16),
    BlueBasics.Enums.DataFormat.Button => QuickImage.Get(ImageCode.Kugel, 16),
    _ => _format.TextboxEditPossible()
    ? _multiLine ? QuickImage.Get(ImageCode.Textfeld, 16, Color.Red, Color.Transparent) : QuickImage.Get(ImageCode.Textfeld)
    : QuickImage.Get("Pfeil_Unten_Scrollbar|14|||||0")
};

        /// <summary>
        ///
        /// </summary>
        /// <param name="nummer">Muss 1, 2 oder 3 sein</param>
        /// <returns></returns>
        public string Ueberschrift(int nummer) {
            switch (nummer) {
                case 0:
                    return _ueberschrift1;

                case 1:
                    return _ueberschrift2;

                case 2:
                    return _ueberschrift3;

                default:
                    Develop.DebugPrint(FehlerArt.Warnung, "Nummer " + nummer + " nicht erlaubt.");
                    return string.Empty;
            }
        }

        public bool UserEditDialogTypeInFormula(EditTypeFormula editTypeToCheck) {
            switch (_format) {
                case BlueBasics.Enums.DataFormat.Text:
                case BlueBasics.Enums.DataFormat.RelationText:
                    if (editTypeToCheck == EditTypeFormula.Textfeld) { return true; } // Textfeld immer erlauben auch wenn beide Bearbeitungen nicht erlaubt sind. Einfach der �bersichtlichktei
                    if (_multiLine && editTypeToCheck == EditTypeFormula.Textfeld_mit_Auswahlknopf) { return false; }
                    if (_dropdownBearbeitungErlaubt && editTypeToCheck == EditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
                    if (_dropdownBearbeitungErlaubt && _dropdownWerteAndererZellenAnzeigen && editTypeToCheck == EditTypeFormula.SwapListBox) { return true; }
                    //if (_MultiLine && _DropdownBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Listbox_3_Zeilen) { return true; }
                    if (_multiLine && _dropdownBearbeitungErlaubt && editTypeToCheck == EditTypeFormula.Listbox) { return true; }
                    if (editTypeToCheck == EditTypeFormula.nur_als_Text_anzeigen) { return true; }
                    if (!_multiLine && editTypeToCheck == EditTypeFormula.Ja_Nein_Knopf) { return true; }
                    return false;

                case BlueBasics.Enums.DataFormat.Verkn�pfung_zu_anderer_Datenbank:
                    if (editTypeToCheck == EditTypeFormula.None) { return true; }
                    //if (EditType_To_Check != enEditTypeFormula.Textfeld &&
                    //    EditType_To_Check != enEditTypeFormula.nur_als_Text_anzeigen) { return false; }
                    if (Database.IsParsing) { return true; }

                    //var skriptgesteuert = LinkedCell_RowKeyIsInColumn == -9999;
                    //if (skriptgesteuert) {
                    //    return editTypeToCheck is enEditTypeFormula.Textfeld or enEditTypeFormula.nur_als_Text_anzeigen;
                    //}

                    if (LinkedDatabase == null) { return false; }
                    if (_linkedCellColumnKeyOfLinkedDatabase < 0) { return false; }
                    var col = LinkedDatabase.Column.SearchByKey(_linkedCellColumnKeyOfLinkedDatabase);
                    if (col == null) { return false; }
                    return col.UserEditDialogTypeInFormula(editTypeToCheck);

                case BlueBasics.Enums.DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems:
                    if (editTypeToCheck == EditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
                    return false;

                //case enDataFormat.Bit:
                //    if (_MultiLine) { return false; }
                //    if (editType_To_Check == enEditTypeFormula.Ja_Nein_Knopf) {
                //        return !_DropdownWerteAndererZellenAnzeigen && DropDownItems.Count <= 0;
                //    }
                //    if (editType_To_Check == enEditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
                //    return false;

                case BlueBasics.Enums.DataFormat.Link_To_Filesystem:
                    if (_multiLine) {
                        //if (EditType_To_Check == enEditType.Listbox) { return true; }
                        if (editTypeToCheck == EditTypeFormula.Gallery) { return true; }
                    } else {
                        if (editTypeToCheck == EditTypeFormula.EasyPic) { return true; }
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
                case BlueBasics.Enums.DataFormat.FarbeInteger:
                    return editTypeToCheck == EditTypeFormula.Farb_Auswahl_Dialog;

                case BlueBasics.Enums.DataFormat.Schrift:
                    return editTypeToCheck == EditTypeFormula.Font_AuswahlDialog;

                case BlueBasics.Enums.DataFormat.Button:
                    //if (EditType_To_Check == enEditTypeFormula.Button) { return true; }
                    return false;

                default:
                    Develop.DebugPrint(_format);
                    return false;
            }
        }

        public string Verwendung() {
            var t = "<b><u>Verwendung von " + ReadableText() + "</b></u><br>";
            if (!string.IsNullOrEmpty(_identifier)) {
                t += " - Systemspalte<br>";
            }
            return t + Database.Column_UsedIn(this);
        }

        internal void CheckFormulaEditType() {
            if (UserEditDialogTypeInFormula(_editType)) { return; }// Alles OK!
            for (var z = 0; z <= 999; z++) {
                var w = (EditTypeFormula)z;
                if (w.ToString() != z.ToString()) {
                    if (UserEditDialogTypeInFormula(w)) {
                        _editType = w;
                        return;
                    }
                }
            }
            _editType = EditTypeFormula.None;
        }

        internal void CheckIfIAmAKeyColumn() {
            Am_A_Key_For_Other_Column = string.Empty;
            foreach (var thisColumn in Database.Column) {
                if (thisColumn.KeyColumnKey == Key) { Am_A_Key_For_Other_Column = "Spalte " + thisColumn.ReadableText() + " verweist auf diese Spalte"; } // Werte Gleichhalten
                                                                                                                                                          //if (thisColumn.LinkedCell_RowKeyIsInColumn == Key) { Am_A_Key_For_Other_Column = "Spalte " + thisColumn.ReadableText() + " verweist auf diese Spalte"; } // LinkdeCells pflegen
                                                                                                                                                          //if (ThisColumn.LinkedCell_ColumnValueFoundIn == Key) { I_Am_A_Key_For_Other_Column = "Spalte " + ThisColumn.ReadableText() + " verweist auf diese Spalte"; } // LinkdeCells pflegen

                if (thisColumn.Format == BlueBasics.Enums.DataFormat.Verkn�pfung_zu_anderer_Datenbank) {
                    foreach (var thisV in thisColumn.LinkedCellFilter) {
                        if (IntTryParse(thisV, out var key)) {
                            if (key == Key) { Am_A_Key_For_Other_Column = "Spalte " + thisColumn.ReadableText() + " verweist auf diese Spalte"; }
                        }
                    }
                }
            }
            //if (_format == enDataFormat.Columns_f�r_LinkedCellDropdown) { Am_A_Key_For_Other_Column = "Die Spalte selbst durch das Format"; }
        }

        /// <summary>
        /// Wenn sich ein Zelleninhalt ver�ndert hat, muss die Spalte neu berechnet werden.
        /// </summary>
        internal void Invalidate_TmpColumnContentWidth() => TmpColumnContentWidth = null;

        internal void Invalidate_TmpVariables() {
            TmpCaptionTextSize = new SizeF(-1, -1);
            TmpCaptionBitmap = null;
            if (_tmpLinkedDatabase != null) {
                //_TMP_LinkedDatabase.RowKeyChanged -= _TMP_LinkedDatabase_RowKeyChanged;
                //_TMP_LinkedDatabase.ColumnKeyChanged -= _TMP_LinkedDatabase_ColumnKeyChanged;
                _tmpLinkedDatabase.ConnectedControlsStopAllWorking -= _TMP_LinkedDatabase_ConnectedControlsStopAllWorking;
                _tmpLinkedDatabase.Cell.CellValueChanged -= _TMP_LinkedDatabase_Cell_CellValueChanged;
                _tmpLinkedDatabase.Disposing -= _TMP_LinkedDatabase_Disposing;
                _tmpLinkedDatabase = null;
            }
            TmpColumnContentWidth = null;
        }

        internal string Load(DatabaseDataType art, string wert) {
            switch (art) {
                case DatabaseDataType.co_Name:
                    _name = wert;
                    Invalidate_TmpVariables();
                    break;

                case DatabaseDataType.co_Caption:
                    _caption = wert;
                    break;

                case DatabaseDataType.co_Format:
                    _format = (BlueBasics.Enums.DataFormat)IntParse(wert);
                    break;

                case DatabaseDataType.co_ForeColor:
                    _foreColor = Color.FromArgb(IntParse(wert));
                    break;

                case DatabaseDataType.co_BackColor:
                    _backColor = Color.FromArgb(IntParse(wert));
                    break;

                case DatabaseDataType.co_LineLeft:
                    _lineLeft = (ColumnLineStyle)IntParse(wert);
                    break;

                case DatabaseDataType.co_LinieRight:
                    _lineRight = (ColumnLineStyle)IntParse(wert);
                    break;

                case DatabaseDataType.co_QuickInfo:
                    _quickInfo = wert;
                    break;
                //case enDatabaseDataType.co_Intelligenter_Multifilter: _Intelligenter_Multifilter = Wert; break;
                case DatabaseDataType.co_DauerFilterPos:
                    //_dauerFilterPos = wert.PointParse();
                    break;

                case DatabaseDataType.co_Ueberschrift1:
                    _ueberschrift1 = wert;
                    break;

                case DatabaseDataType.co_Ueberschrift2:
                    _ueberschrift2 = wert;
                    break;

                case DatabaseDataType.co_Ueberschrift3:
                    _ueberschrift3 = wert;
                    break;

                //case enDatabaseDataType.co_CaptionBitmap:
                //    if (!string.IsNullOrEmpty(Wert)) {
                //        _CaptionBitmapTXT = "co_" + _Name;
                //    }
                //    break;

                case DatabaseDataType.co_Identifier:
                    _identifier = wert;
                    ResetSystemToDefault(false);
                    Database.Column.GetSystems();
                    break;

                case DatabaseDataType.co_EditType:
                    _editType = (EditTypeFormula)IntParse(wert);
                    break;

                case DatabaseDataType.co_MultiLine:
                    _multiLine = wert.FromPlusMinus();
                    break;

                case DatabaseDataType.co_DropDownItems:
                    DropDownItems.SplitAndCutByCr_QuickSortAndRemoveDouble(wert);
                    break;

                case DatabaseDataType.co_LinkedCellFilter:
                    LinkedCellFilter.SplitAndCutByCr(wert);
                    break;

                case DatabaseDataType.co_OpticalReplace:
                    OpticalReplace.SplitAndCutByCr(wert);
                    break;

                case DatabaseDataType.co_AfterEdit_AutoReplace:
                    AfterEditAutoReplace.SplitAndCutByCr(wert);
                    break;

                case DatabaseDataType.co_Regex:
                    _regex = wert;
                    break;

                case DatabaseDataType.co_Tags:
                    Tags.SplitAndCutByCr(wert);
                    break;

                case DatabaseDataType.co_AutoFilterJoker:
                    _autoFilterJoker = wert;
                    break;

                case DatabaseDataType.co_PermissionGroups_ChangeCell:
                    PermissionGroupsChangeCell.SplitAndCutByCr_QuickSortAndRemoveDouble(wert);
                    break;

                case DatabaseDataType.co_AllowedChars:
                    _allowedChars = wert;
                    break;

                case DatabaseDataType.co_FilterOptions:
                    _filterOptions = (FilterOptions)IntParse(wert);
                    break;

                case DatabaseDataType.co_AutoFilterErlaubt_alt:
                    _filterOptions = FilterOptions.None;
                    if (wert.FromPlusMinus()) { _filterOptions |= FilterOptions.Enabled; }
                    break;

                case DatabaseDataType.co_AutoFilterTextFilterErlaubt_alt:
                    if (wert.FromPlusMinus()) { _filterOptions |= FilterOptions.TextFilterEnabled; }
                    break;

                case DatabaseDataType.co_AutoFilterErweitertErlaubt_alt:
                    if (wert.FromPlusMinus()) { _filterOptions |= FilterOptions.ExtendedFilterEnabled; }
                    break;

                case DatabaseDataType.co_BeiZeilenfilterIgnorieren:
                    _ignoreAtRowFilter = wert.FromPlusMinus();
                    break;

                case DatabaseDataType.co_CompactView_alt:
                    break;

                case DatabaseDataType.co_ShowUndo:
                    _showUndo = wert.FromPlusMinus();
                    break;

                case DatabaseDataType.co_ShowMultiLineInOneLine:
                    _showMultiLineInOneLine = wert.FromPlusMinus();
                    break;

                case DatabaseDataType.co_TextBearbeitungErlaubt:
                    _textBearbeitungErlaubt = wert.FromPlusMinus();
                    break;

                case DatabaseDataType.co_DropdownBearbeitungErlaubt:
                    _dropdownBearbeitungErlaubt = wert.FromPlusMinus();
                    break;

                case DatabaseDataType.co_SpellCheckingEnabled:
                    _spellCheckingEnabled = wert.FromPlusMinus();
                    break;

                case DatabaseDataType.co_DropdownAllesAbw�hlenErlaubt:
                    _dropdownAllesAbw�hlenErlaubt = wert.FromPlusMinus();
                    break;

                case DatabaseDataType.co_DropdownWerteAndererZellenAnzeigen:
                    _dropdownWerteAndererZellenAnzeigen = wert.FromPlusMinus();
                    break;

                case DatabaseDataType.co_AfterEdit_QuickSortAndRemoveDouble:
                    _afterEditQuickSortRemoveDouble = wert.FromPlusMinus();
                    break;

                case DatabaseDataType.co_AfterEdit_Runden:
                    _afterEditRunden = IntParse(wert);
                    break;

                case DatabaseDataType.co_AfterEdit_DoUcase:
                    _afterEditDoUCase = wert.FromPlusMinus();
                    break;

                case DatabaseDataType.co_AfterEdit_AutoCorrect:
                    _afterEditAutoCorrect = wert.FromPlusMinus();
                    break;

                case DatabaseDataType.co_SaveContent:
                    _saveContent = wert.FromPlusMinus();
                    break;

                case DatabaseDataType.co_AutoRemove:
                    _autoRemove = wert;
                    break;

                case DatabaseDataType.co_AdminInfo:
                    _adminInfo = wert;
                    break;

                case DatabaseDataType.co_CaptionBitmapTXT:
                    _captionBitmapTxt = wert;
                    break;

                case DatabaseDataType.co_Suffix:
                    _suffix = wert;
                    break;

                case DatabaseDataType.co_LinkedDatabase:
                    _linkedDatabaseFile = wert;
                    break;

                case DatabaseDataType.co_LinkKeyKennung:
                    //_linkedKeyKennung = wert;
                    break;

                case DatabaseDataType.co_BestFile_StandardSuffix:
                    _bestFileStandardSuffix = wert;
                    break;

                case DatabaseDataType.co_BestFile_StandardFolder:
                    _bestFileStandardFolder = wert;
                    break;

                case DatabaseDataType.co_BildCode_ConstantHeight:
                    if (wert == "0") { wert = string.Empty; }
                    _bildCodeConstantHeight = wert;
                    break;

                case DatabaseDataType.co_Prefix:
                    _prefix = wert;
                    break;

                case DatabaseDataType.co_Translate:
                    _translate = (TranslationType)IntParse(wert);
                    break;

                case DatabaseDataType.co_AdditionalCheck:
                    _additionalCheck = (AdditionalCheck)IntParse(wert);
                    break;

                case DatabaseDataType.co_ScriptType:
                    _scriptType = (ScriptType)IntParse(wert);
                    break;

                case DatabaseDataType.co_BildTextVerhalten:
                    _bildTextVerhalten = (BildTextVerhalten)IntParse(wert);
                    break;

                case DatabaseDataType.co_EditTrotzSperreErlaubt:
                    _editTrotzSperreErlaubt = wert.FromPlusMinus();
                    break;

                case DatabaseDataType.co_FormatierungErlaubt:
                    _formatierungErlaubt = wert.FromPlusMinus();
                    break;

                case DatabaseDataType.co_CellInitValue:
                    _cellInitValue = wert;
                    break;

                case DatabaseDataType.co_KeyColumnKey:
                    _keyColumnKey = LongParse(wert);
                    break;

                case DatabaseDataType.co_LinkedCell_RowKeyIsInColumn:
                    //_linkedCellRowKeyIsInColumn = LongParse(wert);
                    break;

                case DatabaseDataType.co_LinkedCell_ColumnKeyOfLinkedDatabase:
                    _linkedCellColumnKeyOfLinkedDatabase = LongParse(wert);
                    break;

                //case enDatabaseDataType.co_LinkedCell_ColumnValueFoundIn:
                //    break;

                //case enDatabaseDataType.co_LinkedCell_ColumnValueAdd:
                //    break;

                case DatabaseDataType.co_SortType:
                    if (string.IsNullOrEmpty(wert)) {
                        _sortType = SortierTyp.Original_String;
                    } else {
                        _sortType = (SortierTyp)LongParse(wert);
                    }
                    break;

                //case enDatabaseDataType.co_ZellenZusammenfassen: _ZellenZusammenfassen = Wert.FromPlusMinus(); break;
                case DatabaseDataType.co_DropDownKey:
                    _dropDownKey = LongParse(wert);
                    break;

                case DatabaseDataType.co_VorschlagColumn:
                    _vorschlagsColumn = LongParse(wert);
                    break;

                case DatabaseDataType.co_Align:
                    _align = (AlignmentHorizontal)IntParse(wert);
                    if (_align == (AlignmentHorizontal)(-1)) { _align = AlignmentHorizontal.Links; }
                    break;
                //case (enDatabaseDataType)189: break;
                //case (enDatabaseDataType)192: break;
                //case (enDatabaseDataType)193: break;
                default:
                    if (art.ToString() == ((int)art).ToString()) {
                        //Develop.DebugPrint(enFehlerArt.Info, "Laden von Datentyp '" + Art + "' nicht definiert.<br>Wert: " + Wert + "<br>Datei: " + Database.Filename);
                    } else {
                        return "Interner Fehler: F�r den Datentyp  '" + art + "'  wurde keine Laderegel definiert.";
                    }
                    break;
            }
            return string.Empty;
        }

        internal string ParsableColumnKey() => ColumnCollection.ParsableColumnKey(this);

        internal void SaveToByteList(ref List<byte> l) {
            Database.SaveToByteList(l, DatabaseDataType.co_Name, _name, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_Caption, _caption, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_Format, ((int)_format).ToString(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_Ueberschrift1, _ueberschrift1, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_Ueberschrift2, _ueberschrift2, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_Ueberschrift3, _ueberschrift3, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_MultiLine, _multiLine.ToPlusMinus(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_CellInitValue, _cellInitValue, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_AfterEdit_QuickSortAndRemoveDouble, _afterEditQuickSortRemoveDouble.ToPlusMinus(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_AfterEdit_DoUcase, _afterEditDoUCase.ToPlusMinus(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_AfterEdit_AutoCorrect, _afterEditAutoCorrect.ToPlusMinus(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_AfterEdit_Runden, _afterEditRunden.ToString(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_AutoRemove, _autoRemove, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_SaveContent, _saveContent.ToPlusMinus(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_FilterOptions, ((int)_filterOptions).ToString(), Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_AutoFilter_Dauerfilter, ((int)_AutoFilter_Dauerfilter).ToString(), Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_AutoFilterErlaubt, _AutofilterErlaubt.ToPlusMinus(), Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_AutoFilterTextFilterErlaubt, _AutofilterTextFilterErlaubt.ToPlusMinus(), Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_AutoFilterErweitertErlaubt, _AutoFilterErweitertErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_AutoFilterJoker, _autoFilterJoker, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_BeiZeilenfilterIgnorieren, _ignoreAtRowFilter.ToPlusMinus(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_TextBearbeitungErlaubt, _textBearbeitungErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_SpellCheckingEnabled, _spellCheckingEnabled.ToPlusMinus(), Key);
            //  Database.SaveToByteList(l, enDatabaseDataType.co_CompactView, _CompactView.ToPlusMinus(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_ShowMultiLineInOneLine, _showMultiLineInOneLine.ToPlusMinus(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_ShowUndo, _showUndo.ToPlusMinus(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_FormatierungErlaubt, _formatierungErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_ForeColor, _foreColor.ToArgb().ToString(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_BackColor, _backColor.ToArgb().ToString(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_LineLeft, ((int)_lineLeft).ToString(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_LinieRight, ((int)_lineRight).ToString(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_DropdownBearbeitungErlaubt, _dropdownBearbeitungErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_DropDownItems, DropDownItems.JoinWithCr(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_LinkedCellFilter, LinkedCellFilter.JoinWithCr(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_OpticalReplace, OpticalReplace.JoinWithCr(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_AfterEdit_AutoReplace, AfterEditAutoReplace.JoinWithCr(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_Regex, _regex, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_DropdownAllesAbw�hlenErlaubt, _dropdownAllesAbw�hlenErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_DropdownWerteAndererZellenAnzeigen, _dropdownWerteAndererZellenAnzeigen.ToPlusMinus(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_QuickInfo, _quickInfo, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_AdminInfo, _adminInfo, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_CaptionBitmapTXT, _captionBitmapTxt, Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_CaptionBitmapUTF8, modConverter.BitmapToStringUnicode(_CaptionBitmap, ImageFormat.Png), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_AllowedChars, _allowedChars, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_PermissionGroups_ChangeCell, PermissionGroupsChangeCell.JoinWithCr(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_EditType, ((int)_editType).ToString(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_Tags, Tags.JoinWithCr(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_EditTrotzSperreErlaubt, _editTrotzSperreErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_Suffix, _suffix, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_LinkedDatabase, _linkedDatabaseFile, Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_LinkKeyKennung, _linkedKeyKennung, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_BestFile_StandardFolder, _bestFileStandardFolder, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_BestFile_StandardSuffix, _bestFileStandardSuffix, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_BildCode_ConstantHeight, _bildCodeConstantHeight, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_BildTextVerhalten, ((int)_bildTextVerhalten).ToString(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_Translate, ((int)_translate).ToString(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_AdditionalCheck, ((int)_additionalCheck).ToString(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_ScriptType, ((int)_scriptType).ToString(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_Prefix, _prefix, Key);
            Database.SaveToByteList(l, DatabaseDataType.co_KeyColumnKey, _keyColumnKey.ToString(), Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_LinkedCell_RowKeyIsInColumn, _linkedCellRowKeyIsInColumn.ToString(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_LinkedCell_ColumnKeyOfLinkedDatabase, _linkedCellColumnKeyOfLinkedDatabase.ToString(), Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_LinkedCell_ColumnValueFoundIn, _LinkedCell_ColumnValueFoundIn.ToString(), Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_LinkedCell_ColumnValueAdd, _LinkedCell_ColumnValueAdd, Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_ZellenZusammenfassen, _ZellenZusammenfassen.ToPlusMinus(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_DropDownKey, _dropDownKey.ToString(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_VorschlagColumn, _vorschlagsColumn.ToString(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_Align, ((int)_align).ToString(), Key);
            Database.SaveToByteList(l, DatabaseDataType.co_SortType, ((int)_sortType).ToString(), Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_Intelligenter_Multifilter, _Intelligenter_Multifilter, Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_DauerFilterPos, _dauerFilterPos.ToString(), Key);
            //Kennung UNBEDINGT zum Schluss, damit die Standard-Werte gesetzt werden k�nnen
            Database.SaveToByteList(l, DatabaseDataType.co_Identifier, _identifier, Key);
        }

        private void _TMP_LinkedDatabase_Cell_CellValueChanged(object sender, CellEventArgs e) {
            var tKey = CellCollection.KeyOfCell(e.Column, e.Row);
            foreach (var thisRow in Database.Row) {
                if (Database.Cell.GetStringBehindLinkedValue(this, thisRow) == tKey) {
                    CellCollection.Invalidate_CellContentSize(this, thisRow);
                    Invalidate_TmpColumnContentWidth();
                    Database.Cell.OnCellValueChanged(new CellEventArgs(this, thisRow));
                    thisRow.DoAutomatic(true, false, 5, "value changed");
                }
            }
        }

        //private void _TMP_LinkedDatabase_ColumnKeyChanged(object sender, KeyChangedEventArgs e) {
        //    Database.BlockReload(false);
        //    if (_Format != enDataFormat.Columns_f�r_LinkedCellDropdown) {
        //        var os = e.KeyOld.ToString();
        //        var ns = e.KeyNew.ToString();
        //        foreach (var ThisRow in Database.Row) {
        //            if (Database.Cell.GetStringBehindLinkedValue(this, ThisRow) == os) {
        //                Database.Cell.SetValueBehindLinkedValue(this, ThisRow, ns);
        //            }
        //        }
        //    }
        //    if (_Format != enDataFormat.LinkedCell) {
        //        var os = e.KeyOld.ToString() + "|";
        //        var ns = e.KeyNew.ToString() + "|";
        //        foreach (var ThisRow in Database.Row) {
        //            var val = Database.Cell.GetStringBehindLinkedValue(this, ThisRow);
        //            if (val.StartsWith(os)) {
        //                Database.Cell.SetValueBehindLinkedValue(this, ThisRow, val.Replace(os, ns));
        //            }
        //        }
        //    }
        //}

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

        //private void _TMP_LinkedDatabase_RowKeyChanged(object sender, KeyChangedEventArgs e) {
        //    if (_Format != enDataFormat.LinkedCell) {
        //        var os = "|" + e.KeyOld.ToString();
        //        var ns = "|" + e.KeyNew.ToString();
        //        foreach (var ThisRow in Database.Row) {
        //            var val = Database.Cell.GetStringBehindLinkedValue(this, ThisRow);
        //            if (val.EndsWith(os)) {
        //                Database.Cell.SetValueBehindLinkedValue(this, ThisRow, val.Replace(os, ns));
        //            }
        //        }
        //    }
        //}

        private void AfterEdit_AutoReplace_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(DatabaseDataType.co_AfterEdit_AutoReplace, Key, AfterEditAutoReplace.JoinWithCr(), false);
            OnChanged();
        }

        private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

        private void Dispose(bool disposing) {
            if (!_disposedValue) {
                if (disposing) {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                }
                Database.Disposing -= Database_Disposing;
                Invalidate_TmpVariables();
                Database = null;

                DropDownItems.Changed -= DropDownItems_ListOrItemChanged;
                DropDownItems.Dispose();

                LinkedCellFilter.Changed -= LinkedCellFilters_ListOrItemChanged;
                LinkedCellFilter.Dispose();

                Tags.Changed -= Tags_ListOrItemChanged;
                Tags.Dispose();

                PermissionGroupsChangeCell.Changed -= PermissionGroups_ChangeCell_ListOrItemChanged;
                PermissionGroupsChangeCell.Dispose();

                OpticalReplace.Changed -= OpticalReplacer_ListOrItemChanged;
                OpticalReplace.Dispose();

                AfterEditAutoReplace.Changed -= AfterEdit_AutoReplace_ListOrItemChanged;
                AfterEditAutoReplace.Dispose();
                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer �berschreiben
                // TODO: Gro�e Felder auf NULL setzen
                _disposedValue = true;
            }
        }

        private void DropDownItems_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(DatabaseDataType.co_DropDownItems, Key, DropDownItems.JoinWithCr(), false);
            OnChanged();
        }

        private string KleineFehlerCorrect(string txt) {
            if (string.IsNullOrEmpty(txt)) { return string.Empty; }
            const char h4 = (char)1004; // H4 = Normaler Text, nach links rutschen
            const char h3 = (char)1003; // �berschrift
            const char h2 = (char)1002; // �berschrift
            const char h1 = (char)1001; // �berschrift
            const char h7 = (char)1007; // bold
            if (FormatierungErlaubt) { txt = txt.HtmlSpecialToNormalChar(false); }
            string oTxt;
            do {
                oTxt = txt;
                if (oTxt.ToLower().Contains(".at")) { break; }
                if (oTxt.ToLower().Contains(".de")) { break; }
                if (oTxt.ToLower().Contains(".com")) { break; }
                if (oTxt.ToLower().Contains("http")) { break; }
                if (oTxt.ToLower().Contains("ftp")) { break; }
                if (oTxt.ToLower().Contains(".xml")) { break; }
                if (oTxt.ToLower().Contains(".doc")) { break; }
                if (oTxt.IsDateTime()) { break; }
                txt = txt.Replace("\r\n", "\r");
                // 1/2 l Milch
                // 3-5 Stunden
                // 180�C
                // Nach Zahlen KEINE leerzeichen einf�gen. Es gibt so viele dinge.... 90er Schichtsalat
                txt = txt.Insert(" ", ",", "1234567890, \r");
                txt = txt.Insert(" ", "!", " !?)\r");
                txt = txt.Insert(" ", "?", " !?)\r");
                txt = txt.Insert(" ", ".", " 1234567890.!?/)\r");
                txt = txt.Insert(" ", ")", " .;!?\r");
                txt = txt.Insert(" ", ";", " 1234567890\r");
                txt = txt.Insert(" ", ":", "1234567890 \\/\r"); // auch 3:50 Uhr
                                                                // H4= Normaler Text
                txt = txt.Replace(" " + h4, h4 + " "); // H4 = Normaler Text, nach links rutschen
                txt = txt.Replace("\r" + h4, h4 + "\r");
                // Dei restlichen Hs'
                txt = txt.Replace(h3 + " ", " " + h3); // �berschrift, nach Rechts
                txt = txt.Replace(h2 + " ", " " + h2); // �berschrift, nach Rechts
                txt = txt.Replace(h1 + " ", " " + h1); // �berschrift, nach Rechts
                txt = txt.Replace(h7 + " ", " " + h7); // Bold, nach Rechts
                txt = txt.Replace(h3 + "\r", "\r" + h3); // �berschrift, nach Rechts
                txt = txt.Replace(h2 + "\r", "\r" + h2); // �berschrift, nach Rechts
                txt = txt.Replace(h1 + "\r", "\r" + h1); // �berschrift, nach Rechts
                txt = txt.Replace(h7 + "\r", "\r" + h7); // Bold, nach Rechts
                txt = txt.Replace(h7 + h4.ToString(), h4.ToString());
                txt = txt.Replace(h3 + h4.ToString(), h4.ToString());
                txt = txt.Replace(h2 + h4.ToString(), h4.ToString());
                txt = txt.Replace(h1 + h4.ToString(), h4.ToString());
                txt = txt.Replace(h4 + h4.ToString(), h4.ToString());
                txt = txt.Replace(" �", "�");
                txt = txt.Replace(" .", ".");
                txt = txt.Replace(" ,", ",");
                txt = txt.Replace(" :", ":");
                txt = txt.Replace(" ?", "?");
                txt = txt.Replace(" !", "!");
                txt = txt.Replace(" )", ")");
                txt = txt.Replace("( ", "(");
                txt = txt.Replace("/ ", "/");
                txt = txt.Replace(" /", "/");
                txt = txt.Replace("\r ", "\r");
                txt = txt.Replace(" \r", "\r");
                txt = txt.Replace("     ", " "); // Wenn das hier nicht da ist, passieren wirklich fehler...
                txt = txt.Replace("    ", " ");
                txt = txt.Replace("   ", " "); // Wenn das hier nicht da ist, passieren wirklich fehler...
                txt = txt.Replace("  ", " ");
                txt = txt.Trim(' ');
                txt = txt.Trim("\r");
                txt = txt.TrimEnd("\t");
            } while (oTxt != txt);
            if (FormatierungErlaubt) {
                txt = txt.CreateHtmlCodes(true);
                txt = txt.Replace("<br>", "\r");
            }
            return txt;
        }

        private void LinkedCellFilters_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(DatabaseDataType.co_LinkedCellFilter, Key, LinkedCellFilter.JoinWithCr(), false);
            OnChanged();
        }

        private void OpticalReplacer_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(DatabaseDataType.co_OpticalReplace, Key, OpticalReplace.JoinWithCr(), false);
            Invalidate_ColumAndContent();
            OnChanged();
        }

        private void PermissionGroups_ChangeCell_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(DatabaseDataType.co_PermissionGroups_ChangeCell, Key, PermissionGroupsChangeCell.JoinWithCr(), false);
            OnChanged();
        }

        private void Tags_ListOrItemChanged(object sender, System.EventArgs e) {
            Database.AddPending(DatabaseDataType.co_Tags, Key, Tags.JoinWithCr(), false);
            OnChanged();
        }

        #endregion
    }
}