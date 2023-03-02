// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using static BlueBasics.Converter;
using static BlueBasics.IO;

namespace BlueDatabase;

public sealed class ColumnItem : IReadableTextWithChangingAndKey, IDisposableExtended, IColumnInputFormat, IErrorCheckable, IHasDatabase, IHasKeyName {

    #region Fields

    public static readonly string TmpNewDummy = "TMPNEWDUMMY";
    public DateTime? IsInCache = null;

    //public string _timecode;
    public bool? TmpAutoFilterSinnvoll = null;

    public QuickImage? TmpCaptionBitmapCode;
    public SizeF TmpCaptionTextSize = new(-1, -1);
    public int? TmpIfFilterRemoved = null;
    internal List<string>? UcaseNamesSortedByLenght;
    private readonly List<string> _afterEditAutoReplace = new();
    private readonly List<string> _dropDownItems = new();
    private readonly List<string> _linkedCellFilter = new();
    private readonly List<string> _opticalReplace = new();
    private readonly List<string> _permissionGroupsChangeCell = new();
    private readonly List<string> _tags = new();
    private AdditionalCheck _additionalFormatCheck;
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
    private BildTextVerhalten _behaviorOfImageAndText;
    private string _caption;
    private string _captionBitmapCode;
    private string _captionGroup1;
    private string _captionGroup2;
    private string _captionGroup3;
    private string _cellInitValue;
    private string _constantHeightOfImageCode;
    private int _contentwidth;
    private TranslationType _doOpticalTranslation;
    private bool _dropdownAllesAbwählenErlaubt;
    private bool _dropdownBearbeitungErlaubt;
    private bool _dropdownWerteAndererZellenAnzeigen;
    private bool _editAllowedDespiteLock;
    private FilterOptions _filterOptions;
    private Color _foreColor;
    private DataFormat _format;
    private bool _formatierungErlaubt;
    private bool _ignoreAtRowFilter;

    //private long _key;
    private ColumnLineStyle _lineLeft;

    private ColumnLineStyle _lineRight;

    /// <summary>
    /// Die Quell-Spalte (aus der verlinkten Datenbank) ist immer
    /// </summary>
    private string _linkedCell_ColumnNameOfLinkedDatabase;

    /// <summary>
    /// Diese Variable ist der temporäre Wert und wird von _linkedDatabaseFile abgeleitet.
    /// </summary>
    private DatabaseAbstract? _linkedDatabase;

    private string _linkedDatabaseFile;
    private int _maxTextLenght;
    private bool _multiLine;
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

    #endregion

    #region Constructors

    public ColumnItem(DatabaseAbstract database, string name) {
        if (!IsValidColumnName(name)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname nicht erlaubt!");
        }

        Database = database;
        Database.Disposing += Database_Disposing;

        var ex = database.Column.Exists(name);
        if (ex != null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Key existiert bereits");
        }

        //_key = database.Column.NextColumnKey();

        #region Standard-Werte

        KeyName = name;
        _caption = string.Empty;
        //_CaptionBitmapCode = null;
        _format = DataFormat.Text;
        _lineLeft = ColumnLineStyle.Dünn;
        _lineRight = ColumnLineStyle.Ohne;
        _multiLine = false;
        _quickInfo = string.Empty;
        _captionGroup1 = string.Empty;
        _captionGroup2 = string.Empty;
        _captionGroup3 = string.Empty;
        //_Intelligenter_Multifilter = string.Empty;
        _foreColor = Color.Black;
        _backColor = Color.White;
        _cellInitValue = string.Empty;
        //_linkedCellRowKeyIsInColumn = -1;
        _linkedCell_ColumnNameOfLinkedDatabase = string.Empty;
        _sortType = SortierTyp.Original_String;
        //_ZellenZusammenfassen = false;
        //_dropDownKey = -1;
        //_vorschlagsColumn = -1;
        _align = AlignmentHorizontal.Links;
        //_keyColumnKey = -1;
        _allowedChars = string.Empty;
        _adminInfo = string.Empty;
        _maxTextLenght = 4000;
        _contentwidth = -1;
        UnsavedContentWidth = -1;
        _captionBitmapCode = string.Empty;
        _filterOptions = FilterOptions.Enabled | FilterOptions.TextFilterEnabled | FilterOptions.ExtendedFilterEnabled;
        //_AutofilterErlaubt = true;
        //_AutofilterTextFilterErlaubt = true;
        //_AutoFilterErweitertErlaubt = true;
        _ignoreAtRowFilter = false;
        _dropdownBearbeitungErlaubt = false;
        _dropdownAllesAbwählenErlaubt = false;
        _textBearbeitungErlaubt = false;
        _dropdownWerteAndererZellenAnzeigen = false;
        _afterEditQuickSortRemoveDouble = false;
        _afterEditRunden = -1;
        _afterEditAutoCorrect = false;
        _afterEditDoUCase = false;
        _formatierungErlaubt = false;
        _additionalFormatCheck = AdditionalCheck.None;
        _scriptType = ScriptType.undefiniert;
        _autoRemove = string.Empty;
        _autoFilterJoker = string.Empty;
        _saveContent = true;
        //_AutoFilter_Dauerfilter = enDauerfilter.ohne;
        _spellCheckingEnabled = false;
        //_CompactView = true;
        _showUndo = true;
        _doOpticalTranslation = TranslationType.Original_Anzeigen;
        _showMultiLineInOneLine = false;
        _editAllowedDespiteLock = false;
        _suffix = string.Empty;
        _linkedDatabaseFile = string.Empty;
        _behaviorOfImageAndText = BildTextVerhalten.Nur_Text;
        _constantHeightOfImageCode = string.Empty;
        _prefix = string.Empty;
        UcaseNamesSortedByLenght = null;
        //Am_A_Key_For_Other_Column = string.Empty;

        #endregion Standard-Werte

        Invalidate_Head();
        Invalidate_LinkedDatabase();
    }

    #endregion

    #region Destructors

    ~ColumnItem() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    //private string _vorschlagsColumn;
    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    public event EventHandler? Changed;

    #endregion

    #region Properties

    public AdditionalCheck AdditionalFormatCheck {
        get => _additionalFormatCheck;
        set {
            if (_additionalFormatCheck == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.AdditionalFormatCheck, Name, null,
                ((int)_additionalFormatCheck).ToString(), ((int)value).ToString(), string.Empty);
            OnChanged();
        }
    }

    //    #endregion Standard-Werte
    public string AdminInfo {
        get => _adminInfo;
        set {
            if (_adminInfo == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.ColumnAdminInfo, Name, null, _adminInfo, value, string.Empty);
            OnChanged();
        }
    }

    public bool AfterEditAutoCorrect {
        get => _afterEditAutoCorrect;
        set {
            if (_afterEditAutoCorrect == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.AutoCorrectAfterEdit, Name, null, _afterEditAutoCorrect.ToPlusMinus(),
                value.ToPlusMinus(), string.Empty);
            OnChanged();
        }
    }

    //    #region Standard-Werte
    public ReadOnlyCollection<string> AfterEditAutoReplace {
        get => new(_afterEditAutoReplace);
        set {
            if (!_afterEditAutoReplace.IsDifferentTo(value)) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.AutoReplaceAfterEdit, Name, null, _afterEditAutoReplace.JoinWithCr(),
                value.JoinWithCr(), string.Empty);
            OnChanged();
        }
    }

    //    _key = columnkey;
    public bool AfterEditDoUCase {
        get => _afterEditDoUCase;
        set {
            if (_afterEditDoUCase == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.DoUcaseAfterEdit, Name, null, _afterEditDoUCase.ToPlusMinus(),
                value.ToPlusMinus(), string.Empty);
            OnChanged();
        }
    }

    //    var ex = database.Column.SearchByKey(columnkey);
    //    if (ex != null) {
    //        Develop.DebugPrint(FehlerArt.Fehler, "Key existiert bereits");
    //    }
    public bool AfterEditQuickSortRemoveDouble {
        get => _multiLine && _afterEditQuickSortRemoveDouble;
        set {
            if (_afterEditQuickSortRemoveDouble == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.SortAndRemoveDoubleAfterEdit, Name, null,
                _afterEditQuickSortRemoveDouble.ToPlusMinus(), value.ToPlusMinus(), string.Empty);
            OnChanged();
        }
    }

    //    Database = database;
    //    Database.Disposing += Database_Disposing;
    //    if (columnkey < 0) {
    //        Develop.DebugPrint(FehlerArt.Fehler, "ColumnKey < 0");
    //    }
    public int AfterEditRunden {
        get => _afterEditRunden;
        set {
            if (_afterEditRunden == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.RoundAfterEdit, Name, null, _afterEditRunden.ToString(),
                value.ToString(), string.Empty);
            OnChanged();
        }
    }

    //public ColumnItem(DatabaseAbstract database, long columnkey) {
    //    //if (!IsValidColumnName(columnname)) {
    //    //    Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname nicht erlaubt!");
    //    //}
    public AlignmentHorizontal Align {
        get => _align;
        set {
            if (_align == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnAlign, Name, null, ((int)_align).ToString(), ((int)value).ToString(), string.Empty);
            OnChanged();
        }
    }

    public string AllowedChars {
        get => _allowedChars;
        set {
            if (_allowedChars == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.AllowedChars, Name, null, _allowedChars, value, string.Empty);
            OnChanged();
        }
    }

    public string AutoFilterJoker {
        get => _autoFilterJoker;
        set {
            if (_autoFilterJoker == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.AutoFilterJoker, Name, null, _autoFilterJoker, value, string.Empty);
            OnChanged();
        }
    }

    //public string Am_A_Key_For_Other_Column { get; private set; }
    public string AutoRemove {
        get => _autoRemove;
        set {
            if (_autoRemove == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.AutoRemoveCharAfterEdit, Name, null, _autoRemove, value, string.Empty);
            OnChanged();
        }
    }

    public Color BackColor {
        get => _backColor;
        set {
            if (_backColor.ToArgb() == value.ToArgb()) { return; }

            _ = Database?.ChangeData(DatabaseDataType.BackColor, Name, null, _backColor.ToArgb().ToString(), value.ToArgb().ToString(), string.Empty);
            OnChanged();
        }
    }

    public BildTextVerhalten BehaviorOfImageAndText {
        get => _behaviorOfImageAndText;
        set {
            if (_behaviorOfImageAndText == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.BehaviorOfImageAndText, Name, null, ((int)_behaviorOfImageAndText).ToString(), ((int)value).ToString(), string.Empty);
            OnChanged();
        }
    }

    public string Caption {
        get => _caption;
        set {
            value = value.Replace("<br>", "\r", RegexOptions.IgnoreCase);
            if (_caption == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnCaption, Name, null, _caption, value, string.Empty);
            Invalidate_Head();
            OnChanged();
        }
    }

    public string CaptionBitmapCode {
        get => _captionBitmapCode;
        set {
            if (_captionBitmapCode == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.CaptionBitmapCode, Name, null, _captionBitmapCode, value, string.Empty);
            _captionBitmapCode = value;
            Invalidate_Head();
            OnChanged();
        }
    }

    public string CaptionGroup1 {
        get => _captionGroup1;
        set {
            if (_captionGroup1 == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.CaptionGroup1, Name, null, _captionGroup1, value, string.Empty);
            OnChanged();
        }
    }

    public string CaptionGroup2 {
        get => _captionGroup2;
        set {
            if (_captionGroup2 == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.CaptionGroup2, Name, null, _captionGroup2, value, string.Empty);
            OnChanged();
        }
    }

    public string CaptionGroup3 {
        get => _captionGroup3;
        set {
            if (_captionGroup3 == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.CaptionGroup3, Name, null, _captionGroup3, value, string.Empty);
            OnChanged();
        }
    }

    public string CellInitValue {
        get => _cellInitValue;
        set {
            if (_cellInitValue == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.CellInitValue, Name, null, _cellInitValue, value, string.Empty);
            OnChanged();
        }
    }

    public string ConstantHeightOfImageCode {
        get => _constantHeightOfImageCode;
        set {
            if (_constantHeightOfImageCode == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ConstantHeightOfImageCode, Name, null, _constantHeightOfImageCode, value, string.Empty);
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    public int ContentWidth {
        get => _contentwidth;
        set {
            UnsavedContentWidth = value;
            if (_contentwidth == value) { return; }

            //if (value < 10) {
            //    Develop.DebugPrint("Width < 10!");
            //}

            _ = Database?.ChangeData(DatabaseDataType.ColumnContentWidth, Name, null, _contentwidth.ToString(), value.ToString(), string.Empty);
            OnChanged();
        }
    }

    public DatabaseAbstract? Database { get; private set; }

    public TranslationType DoOpticalTranslation {
        get => _doOpticalTranslation;
        set {
            if (_doOpticalTranslation == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.DoOpticalTranslation, Name, null, ((int)_doOpticalTranslation).ToString(), ((int)value).ToString(), string.Empty);
            OnChanged();
        }
    }

    public bool DropdownAllesAbwählenErlaubt {
        get => _dropdownAllesAbwählenErlaubt;
        set {
            if (_dropdownAllesAbwählenErlaubt == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.DropdownDeselectAllAllowed, Name, null, _dropdownAllesAbwählenErlaubt.ToPlusMinus(), value.ToPlusMinus(), string.Empty);
            OnChanged();
        }
    }

    public bool DropdownBearbeitungErlaubt {
        get => _dropdownBearbeitungErlaubt;
        set {
            if (_dropdownBearbeitungErlaubt == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.EditableWithDropdown, Name, null, _dropdownBearbeitungErlaubt.ToPlusMinus(), value.ToPlusMinus(), string.Empty);
            OnChanged();
        }
    }

    public ReadOnlyCollection<string> DropDownItems {
        get => new(_dropDownItems);
        set {
            if (!_dropDownItems.IsDifferentTo(value)) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.DropDownItems, Name, null, _dropDownItems.JoinWithCr(), value.JoinWithCr(), string.Empty);
            OnChanged();
        }
    }

    public bool DropdownWerteAndererZellenAnzeigen {
        get => _dropdownWerteAndererZellenAnzeigen;
        set {
            if (_dropdownWerteAndererZellenAnzeigen == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.ShowValuesOfOtherCellsInDropdown, Name, null, _dropdownWerteAndererZellenAnzeigen.ToPlusMinus(), value.ToPlusMinus(), string.Empty);
            OnChanged();
        }
    }

    public bool EditAllowedDespiteLock {
        get => _editAllowedDespiteLock;
        set {
            if (_editAllowedDespiteLock == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.EditAllowedDespiteLock, Name, null, _editAllowedDespiteLock.ToPlusMinus(), value.ToPlusMinus(), string.Empty);
            OnChanged();
        }
    }

    public FilterOptions FilterOptions {
        get => _filterOptions;
        set {
            if (_filterOptions == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.FilterOptions, Name, null, ((int)_filterOptions).ToString(), ((int)value).ToString(), string.Empty);
            Invalidate_Head();
            OnChanged();
        }
    }

    public Color ForeColor {
        get => _foreColor;
        set {
            if (_foreColor.ToArgb() == value.ToArgb()) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.ForeColor, Name, null, _foreColor.ToArgb().ToString(), value.ToArgb().ToString(), string.Empty);
            OnChanged();
        }
    }

    public DataFormat Format {
        get => _format;
        set {
            if (_format == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.ColumnFormat, Name, null, ((int)_format).ToString(), ((int)value).ToString(), string.Empty);
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    public bool FormatierungErlaubt {
        get => _formatierungErlaubt;
        set {
            if (_formatierungErlaubt == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.TextFormatingAllowed, Name, null, _formatierungErlaubt.ToPlusMinus(), value.ToPlusMinus(), string.Empty);
            OnChanged();
        }
    }

    public bool IgnoreAtRowFilter {
        get => !_format.Autofilter_möglich() || _ignoreAtRowFilter;
        set {
            if (_ignoreAtRowFilter == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.IgnoreAtRowFilter, Name, null, _ignoreAtRowFilter.ToPlusMinus(), value.ToPlusMinus(), string.Empty);
            OnChanged();
        }
    }

    public bool IsDisposed { get; private set; }
    public string KeyName { get; private set; }

    public ColumnLineStyle LineLeft {
        get => _lineLeft;
        set {
            if (_lineLeft == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.LineStyleLeft, Name, null, ((int)_lineLeft).ToString(), ((int)value).ToString(), string.Empty);
            OnChanged();
        }
    }

    //        var c = Database?.Column.SearchByKey(_keyColumnKey);
    //        c?.CheckIfIAmAKeyColumn();
    //        Database?.ChangeData(DatabaseDataType.KeyColumnKey, Key, null, _keyColumnKey.ToString(false), value.ToString(false), string.Empty);
    //        c = Database?.Column.SearchByKey(_keyColumnKey);
    //        c?.CheckIfIAmAKeyColumn();
    //        OnChanged();
    //    }
    //}
    public ColumnLineStyle LineRight {
        get => _lineRight;
        set {
            if (_lineRight == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.LineStyleRight, Name, null, ((int)_lineRight).ToString(), ((int)value).ToString(), string.Empty);
            OnChanged();
        }
    }

    public string LinkedCell_ColumnNameOfLinkedDatabase {
        get => _linkedCell_ColumnNameOfLinkedDatabase;
        set {
            if (value == "-1") { value = string.Empty; }

            if (_linkedCell_ColumnNameOfLinkedDatabase == value) {
                return;
            }

            _ = Database?.ChangeData(DatabaseDataType.ColumnNameOfLinkedDatabase, Name, null, _linkedCell_ColumnNameOfLinkedDatabase, value, string.Empty);
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    //        _ = (Database?.ChangeData(DatabaseDataType.ColumnKey, Name, null, Key.ToString(), value.ToString(), string.Empty));
    //        OnChanged();
    //    }
    //}
    ///// <summary>
    ///// Hält Werte, dieser Spalte gleich, bezugnehmend der KeyColumn(key)
    ///// </summary>
    //public long KeyColumnKey {
    //    get => _keyColumnKey;
    //    set {
    //        if (_keyColumnKey == value) {
    //            return;
    //        }
    /// <summary>
    /// Die Quell-Spalte (aus der verlinkten Datenbank) ist immer
    /// </summary>
    public List<string> LinkedCellFilter {
        get => _linkedCellFilter;
        set {
            //#region Altes Format konvertieren

            //var newVal = new List<string>();

            //foreach (var item in value) {
            //    if (!string.IsNullOrEmpty(item)) {
            //        if (item.Contains("|")) {
            //            newVal.Add(item);
            //        } else {
            //            Develop.DebugPrint("Altes Format!");
            //            newVal.Add(item);
            //        }
            //    }
            //}

            //#endregion
            value = value.SortedDistinctList();

            if (!_linkedCellFilter.IsDifferentTo(value)) { return; }

            _ = Database?.ChangeData(DatabaseDataType.LinkedCellFilter, Name, null, _linkedCellFilter.JoinWithCr(), value.JoinWithCr(), string.Empty);
            OnChanged();
        }
    }

    public DatabaseAbstract? LinkedDatabase {
        get {
            if (Database == null || Database.IsDisposed) { return null; }

            if (string.IsNullOrEmpty(_linkedDatabaseFile)) { return null; }

            if (_linkedDatabase != null && !_linkedDatabase.IsDisposed) {
                return _linkedDatabase;
            }

            GetLinkedDatabase();
            return _linkedDatabase;
        }
    }

    public string LinkedDatabaseFile {
        get => _linkedDatabaseFile;
        set {
            if (_linkedDatabaseFile == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.LinkedDatabase, Name, null, _linkedDatabaseFile, value, string.Empty);
            Invalidate_LinkedDatabase();
            OnChanged();
        }
    }

    public int MaxTextLenght {
        get => _maxTextLenght;
        set {
            if (_maxTextLenght == value) { return; }
            _ = Database?.ChangeData(DatabaseDataType.MaxTextLenght, Name, null, _maxTextLenght.ToString(), value.ToString(), string.Empty);
            OnChanged();
        }
    }

    public bool MultiLine {
        get => _multiLine;
        set {
            if (!_format.MultilinePossible()) { value = false; }

            if (_multiLine == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.MultiLine, Name, null, _multiLine.ToPlusMinus(), value.ToPlusMinus(), string.Empty);
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    public string Name {
        get => KeyName.ToUpper();
        set {
            value = value.ToUpper();
            if (value == KeyName.ToUpper()) { return; }

            if (!ColumNameAllowed(value)) {
                Develop.DebugPrint(FehlerArt.Warnung, "Spaltenname nicht erlaubt: " + KeyName);
                return;
            }

            //if (Database?.Column.Exists(value) != null) {
            //    Develop.DebugPrint(FehlerArt.Warnung, "Name existiert bereits!");
            //    return;
            //}

            //if (!IsValidColumnName(value)) {
            //    Develop.DebugPrint(FehlerArt.Warnung, "Spaltenname nicht erlaubt!");
            //    return;
            //}

            _ = Database?.ChangeData(DatabaseDataType.ColumnName, Name, null, KeyName, value, string.Empty);
            OnChanged();
        }
    }

    public ReadOnlyCollection<string> OpticalReplace {
        get => new(_opticalReplace);
        set {
            if (!_opticalReplace.IsDifferentTo(value)) { return; }

            _ = Database?.ChangeData(DatabaseDataType.OpticalTextReplace, Name, null, _opticalReplace.JoinWithCr(), value.JoinWithCr(), string.Empty);
            OnChanged();
        }
    }

    public ReadOnlyCollection<string> PermissionGroupsChangeCell {
        get => new(_permissionGroupsChangeCell);
        set {
            if (!_permissionGroupsChangeCell.IsDifferentTo(value)) { return; }

            _ = Database?.ChangeData(DatabaseDataType.PermissionGroupsChangeCell, Name, null, _permissionGroupsChangeCell.JoinWithCr(), value.JoinWithCr(), string.Empty);
            OnChanged();
        }
    }

    public string Prefix {
        get => _prefix;
        set {
            if (_prefix == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.Prefix, Name, null, _prefix, value, string.Empty);
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    public string Quickinfo {
        get => _quickInfo;
        set {
            if (_quickInfo == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnQuickInfo, Name, null, _quickInfo, value, string.Empty);
            OnChanged();
        }
    }

    public string Regex {
        get => _regex;
        set {
            if (_regex == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.RegexCheck, Name, null, _regex, value, string.Empty);
            OnChanged();
        }
    }

    public bool SaveContent {
        get => _saveContent;
        set {
            if (_saveContent == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.SaveContent, Name, null, _saveContent.ToPlusMinus(), value.ToPlusMinus(), string.Empty);
            OnChanged();
        }
    }

    public ScriptType ScriptType {
        get => _scriptType;
        set {
            if (_scriptType == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ScriptType, Name, null, ((int)_scriptType).ToString(), ((int)value).ToString(), string.Empty);
            OnChanged();
        }
    }

    public bool ShowMultiLineInOneLine {
        get => _multiLine && _showMultiLineInOneLine;
        set {
            if (_showMultiLineInOneLine == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ShowMultiLineInOneLine, Name, null, _showMultiLineInOneLine.ToPlusMinus(), value.ToPlusMinus(), string.Empty);
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    public bool ShowUndo {
        get => _showUndo;
        set {
            if (_showUndo == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ShowUndo, Name, null, _showUndo.ToPlusMinus(), value.ToPlusMinus(), string.Empty);
            OnChanged();
        }
    }

    public SortierTyp SortType {
        get => _sortType;
        set {
            if (_sortType == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.SortType, Name, null, ((int)_sortType).ToString(), ((int)value).ToString(), string.Empty);
            OnChanged();
        }
    }

    public bool SpellCheckingEnabled {
        get => _spellCheckingEnabled;
        set {
            if (_spellCheckingEnabled == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.SpellCheckingEnabled, Name, null, _spellCheckingEnabled.ToPlusMinus(), value.ToPlusMinus(), string.Empty);
            OnChanged();
        }
    }

    public string Suffix {
        get => _suffix;
        set {
            if (_suffix == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.Suffix, Name, null, _suffix, value, string.Empty);
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    /// <summary>
    /// Was in Textfeldern oder Datenbankzeilen für ein Suffix angezeigt werden soll. Beispiel: mm
    /// </summary>
    public List<string> Tags {
        get => _tags;
        set {
            if (!_tags.IsDifferentTo(value)) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnTags, Name, null, _tags.JoinWithCr(), value.JoinWithCr(), string.Empty);
            OnChanged();
        }
    }

    public bool TextBearbeitungErlaubt {
        get => Database?.PowerEdit.Subtract(DateTime.Now).TotalSeconds > 0 || _textBearbeitungErlaubt;
        set {
            if (_textBearbeitungErlaubt == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.EditableWithTextInput, Name, null, _textBearbeitungErlaubt.ToPlusMinus(), value.ToPlusMinus(), string.Empty);
            OnChanged();
        }
    }

    public string Ueberschriften {
        get {
            var txt = _captionGroup1 + "/" + _captionGroup2 + "/" + _captionGroup3;
            return txt == "//" ? "###" : txt.TrimEnd("/");
        }
    }

    public int UnsavedContentWidth { get; set; }

    #endregion

    #region Methods

    public static bool IsValidColumnName(string name) {
        if (string.IsNullOrWhiteSpace(name)) { return false; }

        if (!name.ContainsOnlyChars(Constants.AllowedCharsVariableName)) { return false; }

        if (!Constants.Char_AZ.Contains(name.Substring(0, 1).ToUpper())) { return false; }
        if (name.Length > 128) { return false; }

        if (name.ToUpper() == "USER") { return false; } // SQL System-Name
        if (name.ToUpper() == "COMMENT") { return false; } // SQL System-Name

        if (name.ToUpper() == TmpNewDummy) { return false; } // BlueDatabase name bei neuen Spalten

        return true;
    }

    //        Database?.ChangeData(DatabaseDataType.MakeSuggestionFromSameKeyColumn, Key, null, _vorschlagsColumn.ToString(false), value.ToString(false), string.Empty);
    //        OnChanged();
    //    }
    //}
    public static EditTypeTable UserEditDialogTypeInTable(DataFormat format, bool doDropDown, bool keybordInputAllowed,
        bool isMultiline) {
        if (!doDropDown && !keybordInputAllowed) { return EditTypeTable.None; }

        switch (format) {
            case DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems:
                return EditTypeTable.Dropdown_Single;

            //case DataFormat.Link_To_Filesystem:
            //    return EditTypeTable.FileHandling_InDateiSystem;

            case DataFormat.FarbeInteger:
                if (doDropDown) { return EditTypeTable.Dropdown_Single; }

                return EditTypeTable.Farb_Auswahl_Dialog;

            case DataFormat.Schrift:
                if (doDropDown) { return EditTypeTable.Dropdown_Single; }

                return EditTypeTable.Font_AuswahlDialog;

            case DataFormat.Button:
                return EditTypeTable.None;

            default:
                if (format.TextboxEditPossible()) {
                    return !doDropDown
                        ? EditTypeTable.Textfeld
                        : isMultiline
                            ? EditTypeTable.Dropdown_Single
                            : keybordInputAllowed
                                ? EditTypeTable.Textfeld_mit_Auswahlknopf
                                : EditTypeTable.Dropdown_Single;
                }

                Develop.DebugPrint(format);
                return EditTypeTable.None;
        }
    }

    //public string VorschlagsColumn {
    //    get => _vorschlagsColumn;
    //    set {
    //        if (_vorschlagsColumn == value) { return; }
    public static EditTypeTable UserEditDialogTypeInTable(ColumnItem? column, bool doDropDown) {
        if (column == null) { return EditTypeTable.None; }
        return UserEditDialogTypeInTable(column.Format, doDropDown, column.TextBearbeitungErlaubt, column.MultiLine);
    }

    public string AutoCorrect(string value, bool exitifLinkedFormat) {
        //if (Format == DataFormat.Link_To_Filesystem) {
        //    List<string> l = new(value.SplitAndCutByCr());
        //    var l2 = l.Select(thisFile => SimplyFile(thisFile)).ToList();
        //    value = l2.SortedDistinctList().JoinWithCr();
        //}
        if (exitifLinkedFormat) {
            if (_format is DataFormat.Verknüpfung_zu_anderer_Datenbank or
                DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) { return value; }
        }

        if (_afterEditDoUCase) { value = value.ToUpper(); }
        if (!string.IsNullOrEmpty(_autoRemove)) { value = value.RemoveChars(_autoRemove); }
        if (_afterEditAutoReplace.Count > 0) {
            List<string> l = new(value.SplitAndCutByCr());
            foreach (var thisar in _afterEditAutoReplace) {
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
                        if (string.Equals(l[z], rep[0], StringComparison.OrdinalIgnoreCase)) { l[z] = r; }
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

        value = value.CutToUtf8Length(_maxTextLenght);

        return value;
    }

    public bool AutoFilterSymbolPossible() => FilterOptions.HasFlag(FilterOptions.Enabled) && Format.Autofilter_möglich();

    public int CalculatePreveredMaxTextLenght(double prozentZuschlag) {
        if (Database == null || Database.IsDisposed) { return 0; }

        //if (Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) { return 35; }
        //if (Format == DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) { return 15; }
        var m = 0;

        foreach (var thisRow in Database.Row) {
            var t = thisRow.CellGetString(this);
            m = Math.Max(m, t.StringtoUtf8().Length);
        }

        if (m <= 0) {
            return 8;
        }

        if (m == 1) {
            return 1;
        }

        return Math.Min((int)(m * prozentZuschlag) + 1, 4000);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="number">Muss 1, 2 oder 3 sein</param>
    /// <returns></returns>
    public string CaptionGroup(int number) {
        switch (number) {
            case 0:
                return _captionGroup1;

            case 1:
                return _captionGroup2;

            case 2:
                return _captionGroup3;

            default:
                Develop.DebugPrint(FehlerArt.Warnung, "Nummer " + number + " nicht erlaubt.");
                return string.Empty;
        }
    }

    public void CloneFrom(ColumnItem source, bool nameAndKeyToo) {
        if (nameAndKeyToo) {
            Name = source.Name;
            //Database?.ChangeData(DatabaseDataType.ColumnKey, this, null, this.Key.ToString(false), source.Key.ToString(false));

            //Key = source.Key;
        }

        Caption = source.Caption;
        CaptionBitmapCode = source.CaptionBitmapCode;
        Format = source.Format;
        LineLeft = source.LineLeft;
        LineRight = source.LineRight;
        MultiLine = source.MultiLine;
        Quickinfo = source.Quickinfo;
        ForeColor = source.ForeColor;
        BackColor = source.BackColor;
        EditAllowedDespiteLock = source.EditAllowedDespiteLock;
        PermissionGroupsChangeCell = source.PermissionGroupsChangeCell;
        Tags = source.Tags;
        AdminInfo = source.AdminInfo;
        //TimeCode = source.TimeCode;
        UnsavedContentWidth = source.UnsavedContentWidth;
        ContentWidth = source.ContentWidth;
        FilterOptions = source.FilterOptions;
        IgnoreAtRowFilter = source.IgnoreAtRowFilter;
        DropdownBearbeitungErlaubt = source.DropdownBearbeitungErlaubt;
        DropdownAllesAbwählenErlaubt = source.DropdownAllesAbwählenErlaubt;
        TextBearbeitungErlaubt = source.TextBearbeitungErlaubt;
        SpellCheckingEnabled = source.SpellCheckingEnabled;
        DropdownWerteAndererZellenAnzeigen = source.DropdownWerteAndererZellenAnzeigen;
        AfterEditQuickSortRemoveDouble = source.AfterEditQuickSortRemoveDouble;
        AfterEditRunden = source.AfterEditRunden;
        AfterEditDoUCase = source.AfterEditDoUCase;
        AfterEditAutoCorrect = source.AfterEditAutoCorrect;
        AutoRemove = source.AutoRemove;
        SaveContent = source.SaveContent;
        CellInitValue = source.CellInitValue;
        AutoFilterJoker = source.AutoFilterJoker;
        //KeyColumnKey = source.KeyColumnKey;
        //LinkedCell_RowKeyIsInColumn = source.LinkedCell_RowKeyIsInColumn;
        LinkedCell_ColumnNameOfLinkedDatabase = source.LinkedCell_ColumnNameOfLinkedDatabase;
        //DropdownKey = source.DropdownKey;
        //VorschlagsColumn = source.VorschlagsColumn;
        Align = source.Align;
        SortType = source.SortType;
        DropDownItems = source.DropDownItems;
        LinkedCellFilter = source.LinkedCellFilter;
        OpticalReplace = source.OpticalReplace;
        AfterEditAutoReplace = source.AfterEditAutoReplace;
        this.GetStyleFrom(source); // regex, Allowed Chars, etc.
        ScriptType = source.ScriptType;
        ShowUndo = source.ShowUndo;
        ShowMultiLineInOneLine = source.ShowMultiLineInOneLine;
        CaptionGroup1 = source.CaptionGroup1;
        CaptionGroup2 = source.CaptionGroup2;
        CaptionGroup3 = source.CaptionGroup3;
        //LinkedKeyKennung = source.LinkedKeyKennung;
        LinkedDatabaseFile = source.LinkedDatabaseFile;
        BehaviorOfImageAndText = source.BehaviorOfImageAndText;
        ConstantHeightOfImageCode = source.ConstantHeightOfImageCode;
        //BestFile_StandardSuffix = source.BestFile_StandardSuffix;
        //BestFile_StandardFolder = source.BestFile_StandardFolder;
    }

    /// <summary>
    /// Überschreibt alle Spalteeigenschaften mit der der Vorlage.
    /// </summary>
    /// <param name="nameAndKeyToo"></param>
    public bool ColumNameAllowed(string nameToTest) {
        if (!IsValidColumnName(nameToTest)) { return false; }

        if (nameToTest.Equals(KeyName, StringComparison.OrdinalIgnoreCase)) { return true; }
        if (Database?.Column.Exists(nameToTest) != null) { return false; }

        return true;
    }

    public string CompareKey() {
        var tmp = string.IsNullOrEmpty(_caption) ? KeyName + Constants.FirstSortChar + KeyName : _caption + Constants.FirstSortChar + KeyName;
        tmp = tmp.Trim(' ');
        tmp = tmp.TrimStart('-');
        tmp = tmp.Trim(' ');
        return tmp;
    }

    public List<string> Contents() => Contents(null as FilterCollection, null);

    //public List<string> Contents(List<FilterItem>? filter, List<RowItem?>? pinned) {
    //    if (filter == null || filter.Count == 0) { return Contents(); }
    //    //var ficol = new FilterCollection(filter[0].Database);
    //    //ficol.AddRange(filter);
    //    return Contents(filter, pinned);
    //}

    public List<string> Contents(FilterItem filter, List<RowItem?>? pinned) {
        var x = new FilterCollection(filter.Database) { filter };
        return Contents(x, pinned);
    }

    public List<string> Contents(List<RowItem?>? pinned) {
        List<string> list = new();

        if (pinned == null || pinned.Count == 0) { return list; }

        foreach (var thisRowItem in pinned) {
            if (thisRowItem != null) {
                if (_multiLine) {
                    list.AddRange(thisRowItem.CellGetList(this));
                } else {
                    if (thisRowItem.CellGetString(this).Length > 0) {
                        list.Add(thisRowItem.CellGetString(this));
                    }
                }
            }
        }
        return list.SortedDistinctList();
    }

    public List<string> Contents(ICollection<FilterItem>? filter, List<RowItem?>? pinned) {
        List<string> list = new();
        if (Database == null || Database.IsDisposed) { return list; }

        RefreshColumnsData();

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

    public void DeleteContents(FilterCollection filter, List<RowItem?>? pinned) {
        if (Database == null || Database.IsDisposed) { return; }

        foreach (var thisRowItem in Database.Row) {
            if (thisRowItem != null) {
                if (thisRowItem.MatchesTo(filter) ||
                    (pinned != null && pinned.Contains(thisRowItem))) {
                    thisRowItem.CellSet(this, string.Empty);
                }
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="filter">Wird kein Filter übergeben, werden alle Inhalte zurückgegeben!</param>
    /// <param name="pinned"></param>
    /// <returns></returns>
    /// <summary>
    ///
    /// </summary>
    /// <param name="filter">Wird kein Filter übergeben, werden alle Inhalte gelöscht!</param>
    /// <param name="pinned"></param>
    /// <returns></returns>
    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string ErrorReason() {
        if (Database == null || Database.IsDisposed) { return "Datenbank verworfen"; }
        //if (Key < 0) { return "Interner Fehler: ID nicht definiert"; }
        if (string.IsNullOrEmpty(KeyName)) { return "Der Spaltenname ist nicht definiert."; }

        if (!IsValidColumnName(KeyName)) { return "Der Spaltenname ist ungültig."; }

        if (MaxTextLenght < 1) { return "Maximallänge zu klein!"; }
        if (MaxTextLenght > 4000) { return "Maximallänge zu groß!"; }

        if (Database.Column.Any(thisColumn => thisColumn != this && thisColumn != null && string.Equals(KeyName, thisColumn.Name, StringComparison.OrdinalIgnoreCase))) {
            return "Spalten-Name bereits vorhanden.";
        }

        if (string.IsNullOrEmpty(_caption)) { return "Spalten Beschriftung fehlt."; }
        if (!_saveContent && !IsSystemColumn()) { return "Inhalt der Spalte muss gespeichert werden."; }
        if (!_saveContent && _showUndo) { return "Wenn der Inhalt der Spalte nicht gespeichert wird, darf auch kein Undo geloggt werden."; }
        if (((int)_format).ToString() == _format.ToString()) { return "Format fehlerhaft."; }
        if (_format.NeedTargetDatabase()) {
            if (LinkedDatabase == null) { return "Verknüpfte Datenbank fehlt oder existiert nicht."; }
            if (LinkedDatabase == Database) { return "Zirkelbezug mit verknüpfter Datenbank."; }
            var c = LinkedDatabase.Column.Exists(_linkedCell_ColumnNameOfLinkedDatabase);
            if (c == null) { return "Die verknüpfte Schlüsselspalte existiert nicht."; }
            //var (filter, info) = CellCollection.GetFilterFromLinkedCellData(LinkedDatabase, column, row);
            if (_linkedCellFilter == null || _linkedCellFilter.Count == 0) {
                if (Format != DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) {
                    return "Keine Filter für verknüpfte Datenbank definiert.";
                }
            }
        } else {
            if (!string.IsNullOrEmpty(_linkedCell_ColumnNameOfLinkedDatabase)) { return "Nur verlinkte Zellen können Daten über verlinkte Zellen enthalten."; }
        }

        if (!_format.Autofilter_möglich() && _filterOptions != FilterOptions.None) { return "Bei diesem Format keine Filterung erlaubt."; }
        if (_filterOptions != FilterOptions.None && !_filterOptions.HasFlag(FilterOptions.Enabled)) { return "Filter Kombination nicht möglich."; }
        if (_filterOptions != FilterOptions.Enabled_OnlyAndAllowed && _filterOptions.HasFlag(FilterOptions.OnlyAndAllowed)) { return "Filter Kombination nicht möglich."; }
        if (_filterOptions != FilterOptions.Enabled_OnlyOrAllowed && _filterOptions.HasFlag(FilterOptions.OnlyOrAllowed)) { return "Filter Kombination nicht möglich."; }
        if (_filterOptions.HasFlag(FilterOptions.OnlyAndAllowed) || _filterOptions.HasFlag(FilterOptions.OnlyOrAllowed)) {
            if (!_multiLine) {
                return "Dieser Filter kann nur bei mehrzeiligen Spalten benutzt werden.";
            }
        }
        switch (_format) {
            case DataFormat.RelationText:
                if (!_multiLine) { return "Bei diesem Format muss mehrzeilig ausgewählt werden."; }
                //if (_keyColumnKey > -1) { return "Diese Format darf keine Verknüpfung zu einer Schlüsselspalte haben."; }
                if (IsFirst()) { return "Diese Format ist bei der ersten (intern) erste Spalte nicht erlaubt."; }
                if (!string.IsNullOrEmpty(_cellInitValue)) { return "Diese Format kann keinen Initial-Text haben."; }
                //if (!string.IsNullOrEmpty(_vorschlagsColumn)) { return "Diese Format kann keine Vorschlags-Spalte haben."; }
                break;

            //case DataFormat.Verknüpfung_zu_anderer_Datenbank:
            //    if (MaxTextLenght < 35) { return "Maximallänge bei diesem Format mindestens 35!"; }
            //    break;

            case DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems:
                //Develop.DebugPrint("Values_für_LinkedCellDropdown Verwendung bei:" + Database.Filename); //TODO: 29.07.2021 Values_für_LinkedCellDropdown Format entfernen
                if (!string.IsNullOrEmpty(_cellInitValue)) { return "Dieses Format kann keinen Initial-Text haben."; }
                //if (KeyColumnKey > -1) { return "Dieses Format darf keine Verknüpfung zu einer Schlüsselspalte haben."; }
                //if (_vorschlagsColumn > 0) { return "Dieses Format kann keine Vorschlags-Spalte haben."; }
                //if (MaxTextLenght < 15) { return "Maximallänge bei diesem Format mindestens 15!"; }

                break;
        }

        if (_multiLine) {
            if (!_format.MultilinePossible()) { return "Format unterstützt keine mehrzeiligen Texte."; }
            if (_afterEditRunden != -1) { return "Runden nur bei einzeiligen Texten möglich"; }
        } else {
            if (_showMultiLineInOneLine) { return "Wenn mehrzeilige Texte einzeilig dargestellt werden sollen, muss mehrzeilig angewählt sein."; }
            if (_afterEditQuickSortRemoveDouble) { return "Sortierung kann nur bei mehrzeiligen Feldern erfolgen."; }
        }
        if (_spellCheckingEnabled && !_format.SpellCheckingPossible()) { return "Rechtschreibprüfung bei diesem Format nicht möglich."; }
        if (_editAllowedDespiteLock && !_textBearbeitungErlaubt && !_dropdownBearbeitungErlaubt) { return "Wenn die Zeilensperre ignoriert werden soll, muss eine Bearbeitungsmethode definiert sein."; }
        var tmpEditDialog = UserEditDialogTypeInTable(_format, false, true, _multiLine);
        if (_textBearbeitungErlaubt) {
            if (tmpEditDialog == EditTypeTable.Dropdown_Single) { return "Format unterstützt nur Dropdown-Menü."; }
            if (tmpEditDialog == EditTypeTable.None) { return "Format unterstützt keine Standard-Bearbeitung."; }
        }

        if (_dropdownBearbeitungErlaubt) {
            //if (_SpellCheckingEnabled) { return "Entweder Dropdownmenü oder Rechtschreibprüfung."; }
            if (tmpEditDialog == EditTypeTable.None) { return "Format unterstützt keine Auswahlmenü-Bearbeitung."; }
        }
        if (!_dropdownBearbeitungErlaubt && !_textBearbeitungErlaubt) {
            if (_permissionGroupsChangeCell.Count > 0) { return "Bearbeitungsberechtigungen entfernen, wenn keine Bearbeitung erlaubt ist."; }
        }

        foreach (var thisS in _permissionGroupsChangeCell) {
            if (thisS.Contains("|")) { return "Unerlaubtes Zeichen bei den Gruppen, die eine Zelle bearbeiten dürfen."; }
            if (thisS.ToUpper() == "#ADMINISTRATOR") { return "'#Administrator' bei den Bearbeitern entfernen."; }
        }
        if (_dropdownBearbeitungErlaubt || tmpEditDialog == EditTypeTable.Dropdown_Single) {
            if (_format != DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) {
                if (!_dropdownWerteAndererZellenAnzeigen && _dropDownItems.Count == 0) { return "Keine Dropdown-Items vorhanden bzw. Alles hinzufügen nicht angewählt."; }
            }
        } else {
            if (_dropdownWerteAndererZellenAnzeigen) { return "Dropdownmenu nicht ausgewählt, 'alles hinzufügen' prüfen."; }
            if (_dropdownAllesAbwählenErlaubt) { return "Dropdownmenu nicht ausgewählt, 'alles abwählen' prüfen."; }
            if (_dropDownItems.Count > 0) { return "Dropdownmenu nicht ausgewählt, Dropdown-Items vorhanden."; }
        }
        if (_dropdownWerteAndererZellenAnzeigen && !_format.DropdownItemsOfOtherCellsAllowed()) { return "'Dropdownmenu alles hinzufügen' bei diesem Format nicht erlaubt."; }
        if (_dropdownAllesAbwählenErlaubt && !_format.DropdownUnselectAllAllowed()) { return "'Dropdownmenu alles abwählen' bei diesem Format nicht erlaubt."; }
        if (_dropDownItems.Count > 0 && !_format.DropdownItemsAllowed()) { return "Manuelle 'Dropdow-Items' bei diesem Format nicht erlaubt."; }

        if (!string.IsNullOrEmpty(_suffix)) {
            if (_multiLine) { return "Einheiten und Mehrzeilig darf nicht kombiniert werden."; }
        }
        if (_afterEditRunden > 6) { return "Beim Runden maximal 6 Nachkommastellen möglich"; }
        if (_filterOptions == FilterOptions.None) {
            if (!string.IsNullOrEmpty(_autoFilterJoker)) { return "Wenn kein Autofilter erlaubt ist, immer anzuzeigende Werte entfernen"; }
        }
        if (_opticalReplace.Count > 0) {
            if (_format is not DataFormat.Text and
                not DataFormat.RelationText) { return "Format unterstützt keine Ersetzungen."; }
            if (_filterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled)) { return "Entweder 'Ersetzungen' oder 'erweiternden Autofilter'"; }
            if (!string.IsNullOrEmpty(_autoFilterJoker)) { return "Entweder 'Ersetzungen' oder 'Autofilter Joker'"; }
        }

        return string.Empty;
    }

    public bool ExportableTextformatForLayout() => _format.ExportableForLayout();

    public List<string> GetUcaseNamesSortedByLenght() {
        if (UcaseNamesSortedByLenght != null) { return UcaseNamesSortedByLenght; }
        var tmp = Contents(null as FilterCollection, null);
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

    public void GetUniques(List<RowItem> rows, out List<string> einzigartig, out List<string> nichtEinzigartig) {
        einzigartig = new List<string>();
        nichtEinzigartig = new List<string>();
        foreach (var thisRow in rows) {
            if (thisRow != null) {
                var tmp = MultiLine ? thisRow.CellGetList(this) : new List<string> { thisRow.CellGetString(this) };
                foreach (var thisString in tmp) {
                    if (einzigartig.Contains(thisString)) {
                        _ = nichtEinzigartig.AddIfNotExists(thisString);
                    } else {
                        _ = einzigartig.AddIfNotExists(thisString);
                    }
                }
            }
        }
        einzigartig.RemoveString(nichtEinzigartig, false);
    }

    public void Invalidate_ColumAndContent() {
        Invalidate_Head();
        Invalidate_ContentWidth();
        Invalidate_LinkedDatabase();
        //foreach (var thisRow in Database.Row) {
        //    if (thisRow != null) { CellCollection.Invalidate_CellContentSize(this, thisRow); }
        //}
        Database.OnViewChanged();
    }

    //}
    ///// <summary>
    ///// Füllt die Ersetzungen mittels eines übergebenen Enums aus.
    ///// </summary>
    ///// <param name="t">Beispiel: GetType(enDesign)</param>
    ///// <param name="ZumDropdownHinzuAb">Erster Wert der Enumeration, der Hinzugefügt werden soll. Inklusive deses Wertes</param>
    ///// <param name="ZumDropdownHinzuBis">Letzter Wert der Enumeration, der nicht mehr hinzugefügt wird, also exklusives diese Wertes</param>
    //public void GetValuesFromEnum(Type t, int ZumDropdownHinzuAb, int ZumDropdownHinzuBis) {
    //    List<string> NewReplacer = new();
    //    List<string> NewAuswahl = new();
    //    var items = Enum.GetValues(t);
    //    foreach (var thisItem in items) {
    //        var te = Enum.GetName(t, thisItem);
    //        var th = (int)thisItem;
    //        if (!string.IsNullOrEmpty(te)) {
    //            NewReplacer.GenerateAndAdd(th + "|" + te);
    //            if (th >= ZumDropdownHinzuAb && th < ZumDropdownHinzuBis) {
    //                NewAuswahl.GenerateAndAdd(th.ToString(false));
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
    /// <summary>
    /// Der Invalidate, der am meisten invalidiert: Alle temporären Variablen und auch jede Zell-Größe der Spalte.
    /// </summary>
    public bool IsFirst() => Database?.Column.First == this;

    public bool IsOk() => string.IsNullOrEmpty(ErrorReason());

    public bool IsSystemColumn() =>
        KeyName.ToUpper() is "SYS_CORRECT" or
            "SYS_CHANGER" or
            "SYS_CREATOR" or
            "SYS_CHAPTER" or
            "SYS_DATECREATED" or
            "SYS_DATECHANGED" or
            "SYS_LOCKED";

    public void OnChanged() => Changed?.Invoke(this, new ColumnEventArgs(this));

    public string QuickInfoText(string additionalText) {
        var T = string.Empty;
        if (!string.IsNullOrEmpty(_quickInfo)) { T += _quickInfo; }
        if (Database.IsAdministrator() && !string.IsNullOrEmpty(_adminInfo)) { T = T + "<br><br><b><u>Administrator-Info:</b></u><br>" + _adminInfo; }
        if (Database.IsAdministrator() && _tags.Count > 0) { T = T + "<br><br><b><u>Spalten-Tags:</b></u><br>" + Tags.JoinWith("<br>"); }
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
        if (Database.Column.Any(thisColumnItem => thisColumnItem != null && thisColumnItem != this && string.Equals(thisColumnItem.Caption, _caption, StringComparison.OrdinalIgnoreCase))) {
            var done = false;
            if (!string.IsNullOrEmpty(_captionGroup3)) {
                ret = _captionGroup3 + "/" + ret;
                done = true;
            }
            if (!string.IsNullOrEmpty(_captionGroup2)) {
                ret = _captionGroup2 + "/" + ret;
                done = true;
            }
            if (!string.IsNullOrEmpty(_captionGroup1)) {
                ret = _captionGroup1 + "/" + ret;
                done = true;
            }
            if (!done) {
                ret = KeyName; //_Caption + " (" + _Name + ")";
            }
        }
        ret = ret.Replace("\n", "\r").Replace("\r\r", "\r");
        var i = ret.IndexOf("-\r", StringComparison.Ordinal);
        if (i > 0 && i < ret.Length - 3) {
            var tzei = ret.Substring(i + 2, 1);
            if (tzei.ToLower() == tzei) {
                ret = ret.Substring(0, i) + ret.Substring(i + 2);
            }
        }
        return ret.Replace("\r", " ").Replace("  ", " ").TrimEnd(":");
    }

    public void RefreshColumnsData() {
        if (IsInCache != null) { return; }
        if (Database == null || Database.IsDisposed) { return; }
        if (Name == TmpNewDummy) { Develop.DebugPrint("TMPNEWDUMMY kann nicht geladen werden"); return; }

        var x = new List<ColumnItem> { this };
        Database?.RefreshColumnsData(x);
    }

    public void Repair() {
        // Unbekannt = -1,
        // Nothing = 0,
        //    Text = 1,

        //Bit = 2,

        //// Binärdaten_Bild = 19,
        //// Passwort = 20, // String
        ////  Text_Ohne_Kritische_Zeichen = 21,
        //// Binärdaten = 39,
        //// Link_To_BlueDataSystem = 42
        //// Telefonnummer = 43, // Spezielle Formate
        //FarbeInteger = 45, // Color

        //// Email = 46, // Spezielle Formate
        //// InternetAdresse = 47, // Spezielle Formate
        //// Relation = 65,
        //// Event = 66,
        //// Tendenz = 67
        //// Einschätzung = 68,
        //Schrift = 69,

        //Text_mit_Formatierung = 70,

        //// TextmitFormatierungUndLinkToAnotherDatabase = 71
        //// Relation_And_Event_Mixed = 72,
        //Link_To_Filesystem = 73,

        //LinkedCell = 74,
        //Columns_für_LinkedCellDropdown = 75,

        //Values_für_LinkedCellDropdown = 76,

        //RelationText = 77,

        //// KeyForSame = 78
        //Button = 79

        //// bis 999 wird geprüft
        ///

        //CheckIfIAmAKeyColumn();

        try {
            switch ((int)_format) {
                case (int)DataFormat.Button:
                    ScriptType = ScriptType.Nicht_vorhanden;
                    break;

                case 21: //Text_Ohne_Kritische_Zeichen
                    this.GetStyleFrom(ColumnFormatHolder.Text);
                    break;

                case 15:// Date_GermanFormat = 15
                case 16://Datum_und_Uhrzeit = 16,
                    this.GetStyleFrom(ColumnFormatHolder.DateTime);
                    break;

                case 2:   //Bit = 3,
                    this.GetStyleFrom(ColumnFormatHolder.Bit);
                    break;

                case 3:   //Ganzzahl = 3,
                    this.GetStyleFrom(ColumnFormatHolder.Integer);
                    break;

                case 6:  //Gleitkommazahl = 6,
                    this.GetStyleFrom(ColumnFormatHolder.Float);
                    break;

                case 13:         //BildCode = 13,
                    this.GetStyleFrom(ColumnFormatHolder.BildCode);
                    break;

                case 70:
                    this.GetStyleFrom(ColumnFormatHolder.TextMitFormatierung);
                    break;

                case 73: // Link To Filesystem
                    this.GetStyleFrom(ColumnFormatHolder.Text);
                    break;

                case 74: //(int)DataFormat.Verknüpfung_zu_anderer_Datenbank_Skriptgesteuert:

                    //if (LinkedCell_RowKeyIsInColumn != -9999) {
                    _format = DataFormat.Verknüpfung_zu_anderer_Datenbank;
                    _linkedCellFilter.Clear();
                    //LinkedCellFilter.GenerateAndAdd(LinkedCell_RowKeyIsInColumn.ToString(false));
                    //LinkedCell_RowKeyIsInColumn = -1;
                    //}
                    break;

                case 75:
                    _format = DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems;

                    break;
            }

            if (ScriptType == ScriptType.undefiniert) {
                if (MultiLine) {
                    ScriptType = ScriptType.List;
                } else if (Format is DataFormat.Text) {
                    if (SortType is SortierTyp.ZahlenwertFloat or SortierTyp.ZahlenwertInt) {
                        ScriptType = ScriptType.Numeral;
                    }
                    ScriptType = ScriptType.String;
                } else if (Format == DataFormat.Schrift) {
                    ScriptType = ScriptType.Nicht_vorhanden;
                }
            }

            if (_format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {
                var c = LinkedDatabase?.Column.Exists(_linkedCell_ColumnNameOfLinkedDatabase);
                if (c != null) {
                    this.GetStyleFrom((IInputFormat)c);
                    BehaviorOfImageAndText = c.BehaviorOfImageAndText;
                    ScriptType = c.ScriptType; // 29.06.2022 Wieder aktivert. Grund: Plananalyse waren zwei vershieden Typen bei den Zeitn. So erschien immer automatisch eine 0 bei den Stnden, und es war nicht ersichtlich warum.
                    DoOpticalTranslation = c.DoOpticalTranslation;
                }
            }

            if (ScriptType == ScriptType.undefiniert) {
                Develop.DebugPrint(FehlerArt.Warnung, "Umsetzung fehlgeschlagen: " + Caption + " " + Database.ConnectionData.TableName);
            }

            if (_scriptType is ScriptType.Bool or ScriptType.Numeral or ScriptType.DateTime) {
                _ignoreAtRowFilter = true;
            }

            // Nicht möglich, weil die ganze Spalte geladen sein muss
            //if (!Name.StartsWith("SYS_") &&  CalculatePreveredMaxTextLenght(1f) > MaxTextLenght) {
            //    MaxTextLenght = CalculatePreveredMaxTextLenght(1f);
            //}

            ResetSystemToDefault(false);
        } catch (Exception ex) {
            Develop.DebugPrint(ex);
        }
    }

    public void ResetSystemToDefault(bool setOpticalToo) {
        if (!IsSystemColumn()) { return; }
        // ACHTUNG: Die setOpticalToo Befehle OHNE _, die müssen geloggt werden.
        if (setOpticalToo) {
            LineLeft = ColumnLineStyle.Dünn;
            LineRight = ColumnLineStyle.Ohne;
            ForeColor = Color.FromArgb(0, 0, 0);
            //CaptionBitmapCode = null;
        }
        switch (Name.ToUpper()) {
            case "SYS_CREATOR":
                _format = DataFormat.Text;
                _maxTextLenght = 20;
                if (setOpticalToo) {
                    Caption = "Ersteller";
                    DropdownBearbeitungErlaubt = true;
                    DropdownWerteAndererZellenAnzeigen = true;
                    SpellCheckingEnabled = false;
                    ForeColor = Color.FromArgb(0, 0, 128);
                    BackColor = Color.FromArgb(185, 186, 255);
                }
                break;

            case "SYS_CHANGER":
                _format = DataFormat.Text;
                _ignoreAtRowFilter = true;
                _spellCheckingEnabled = false;
                _textBearbeitungErlaubt = false;
                _dropdownBearbeitungErlaubt = false;
                _showUndo = false;
                _scriptType = ScriptType.String_Readonly;
                _permissionGroupsChangeCell.Clear();
                _maxTextLenght = 20;
                if (setOpticalToo) {
                    Caption = "Änderer";
                    ForeColor = Color.FromArgb(0, 128, 0);
                    BackColor = Color.FromArgb(185, 255, 185);
                }
                break;

            case "SYS_CHAPTER":
                _format = DataFormat.Text;
                _afterEditAutoCorrect = true; // Verhindert \r am Ende und somit anzeigefehler
                _multiLine = true;
                if (setOpticalToo) {
                    Caption = "Kapitel";
                    ForeColor = Color.FromArgb(0, 0, 0);
                    BackColor = Color.FromArgb(255, 255, 150);
                    LineLeft = ColumnLineStyle.Dick;
                    ShowMultiLineInOneLine = true;
                }
                break;

            case "SYS_DATECREATED":
                _spellCheckingEnabled = false;
                _ignoreAtRowFilter = true;
                if (Database != null && !Database.ReadOnly) {
                    this.GetStyleFrom(FormatHolder.DateTime); // Ja, FormatHolder, da wird der Script-Type nicht verändert
                }
                if (setOpticalToo) {
                    Caption = "Erstell-Datum";
                    ForeColor = Color.FromArgb(0, 0, 128);
                    BackColor = Color.FromArgb(185, 185, 255);
                    LineLeft = ColumnLineStyle.Dick;
                }

                break;

            case "SYS_DATECHANGED":
                _spellCheckingEnabled = false;
                _ignoreAtRowFilter = true;
                _showUndo = false;
                if (Database != null && !Database.ReadOnly) {
                    this.GetStyleFrom(FormatHolder.DateTime); // Ja, FormatHolder, da wird der Script-Type nicht verändert
                }
                _textBearbeitungErlaubt = false;
                _spellCheckingEnabled = false;
                _dropdownBearbeitungErlaubt = false;
                _scriptType = ScriptType.String_Readonly;
                _permissionGroupsChangeCell.Clear();

                if (setOpticalToo) {
                    Caption = "Änder-Datum";
                    ForeColor = Color.FromArgb(0, 128, 0);
                    BackColor = Color.FromArgb(185, 255, 185);
                    LineLeft = ColumnLineStyle.Dick;
                }
                break;

            case "SYS_CORRECT":
                _caption = "Fehlerfrei";
                _spellCheckingEnabled = false;
                _format = DataFormat.Text;
                //_AutoFilterErweitertErlaubt = false;
                _autoFilterJoker = string.Empty;
                //_AutofilterTextFilterErlaubt = false;
                _ignoreAtRowFilter = true;
                _filterOptions = FilterOptions.Enabled;
                _scriptType = ScriptType.Bool_Readonly;
                _align = AlignmentHorizontal.Zentriert;
                _dropDownItems.Clear();
                _linkedCellFilter.Clear();
                _permissionGroupsChangeCell.Clear();
                _textBearbeitungErlaubt = false;
                _dropdownBearbeitungErlaubt = false;
                _behaviorOfImageAndText = BildTextVerhalten.Interpretiere_Bool;
                _maxTextLenght = 1;
                _dropdownWerteAndererZellenAnzeigen = false;
                _adminInfo = "Diese Spalte kann nur über ein Skript bearbeitet werden,\r\nmit dem Befehl SetError";

                if (setOpticalToo) {
                    ForeColor = Color.FromArgb(128, 0, 0);
                    BackColor = Color.FromArgb(255, 185, 185);
                    LineLeft = ColumnLineStyle.Dick;
                }
                break;

            case "SYS_LOCKED":
                _spellCheckingEnabled = false;
                _format = DataFormat.Text;
                _scriptType = ScriptType.Bool;
                _filterOptions = FilterOptions.Enabled;
                _autoFilterJoker = string.Empty;
                _ignoreAtRowFilter = true;
                _behaviorOfImageAndText = BildTextVerhalten.Interpretiere_Bool;
                _align = AlignmentHorizontal.Zentriert;
                _maxTextLenght = 1;

                if (_textBearbeitungErlaubt || _dropdownBearbeitungErlaubt) {
                    _quickInfo = "Eine abgeschlossene Zeile kann<br>nicht mehr bearbeitet werden.";
                    _textBearbeitungErlaubt = false;
                    _dropdownBearbeitungErlaubt = true;
                    _editAllowedDespiteLock = true;
                    _ = _dropDownItems.AddIfNotExists("+");
                    _ = _dropDownItems.AddIfNotExists("-");
                } else {
                    _dropDownItems.Clear();
                }

                if (setOpticalToo) {
                    Caption = "Abgeschlossen";
                    ForeColor = Color.FromArgb(128, 0, 0);
                    BackColor = Color.FromArgb(255, 185, 185);
                }
                break;

            //case "System: State":
            //    _name = "SYS_RowState";
            //    _caption = "veraltet und kann gelöscht werden: Zeilenstand";
            //    _identifierx = string.Empty;
            //    break;

            //case "System: ID":
            //    _name = "SYS_ID";
            //    _caption = "veraltet und kann gelöscht werden: Zeilen-ID";
            //    _identifierx = string.Empty;
            //    break;

            //case "System: Last Used Layout":
            //    _name = "SYS_Layout";
            //    _caption = "veraltet und kann gelöscht werden:  Letztes Layout";
            //    _identifierx = string.Empty;
            //    break;

            default:
                Develop.DebugPrint("Unbekannte Kennung: " + KeyName);
                break;
        }
    }

    public void Statisik(ICollection<FilterItem>? filter, List<RowItem>? pinnedRows) {
        if (Database == null || Database.IsDisposed) { return; }
        var r = Database.Row.CalculateVisibleRows(filter, pinnedRows);

        if (r.Count < 1) { return; }

        var d = new Dictionary<string, int>();

        foreach (var thisRow in r) {
            var keyValue = thisRow.CellGetString(this);
            if (string.IsNullOrEmpty(keyValue)) { keyValue = "[empty]"; }

            keyValue = keyValue.Replace("\r", ";");

            var count = 0;
            if (d.ContainsKey(keyValue)) {
                _ = d.TryGetValue(keyValue, out count);
                _ = d.Remove(keyValue);
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

            foreach (var thisKey in d) {
                if (thisKey.Value > maxCount) {
                    keyValue = thisKey.Key;
                    maxCount = thisKey.Value;
                }
            }

            _ = d.Remove(keyValue);
            l.Add(maxCount + " - " + keyValue);
        } while (d.Count > 0);

        l.Save(TempFile(string.Empty, string.Empty, "txt"), Encoding.UTF8, true);
    }

    public double? Summe(FilterCollection? filter) {
        if (Database == null || Database.IsDisposed) { return null; }

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

    //        default:
    //            Develop.DebugPrint(FehlerArt.Warnung);
    //            break;
    //    }
    //}
    //public string SimplyFile(string fullFileName) {
    //    if (_format != DataFormat.Link_To_Filesystem) {
    //        Develop.DebugPrint(FehlerArt.Fehler, "Nur bei Link_To_Filesystem erlaubt!");
    //    }
    //    var tmpfile = fullFileName.FileNameWithoutSuffix();
    //    if (string.Equals(tmpfile, fullFileName, StringComparison.OrdinalIgnoreCase)) { return tmpfile; }
    //    if (string.Equals(BestFile(tmpfile, false), fullFileName, StringComparison.OrdinalIgnoreCase)) { return tmpfile; }
    //    tmpfile = fullFileName.FileNameWithSuffix();
    //    return string.Equals(tmpfile, fullFileName, StringComparison.OrdinalIgnoreCase)
    //        ? tmpfile
    //        : string.Equals(BestFile(tmpfile, false), fullFileName, StringComparison.OrdinalIgnoreCase) ? tmpfile : fullFileName;
    //}
    public double? Summe(List<RowData>? sort) {
        if (sort == null) { return null; }

        double summ = 0;
        foreach (var thisrow in sort) {
            if (thisrow?.Row != null && !thisrow.Row.CellIsNullOrEmpty(this)) {
                if (!thisrow.Row.CellGetString(this).IsDouble()) { return null; }
                summ += thisrow.Row.CellGetDouble(this);
            }
        }
        return summ;
    }

    //        case FormatHolder.Bit:
    //            SetFormatForBit();
    //            break;
    public QuickImage? SymbolForReadableText() {
        if (IsDisposed) { return null; }
        if (Database == null || Database.IsDisposed) { return null; }

        if (this == Database.Column.SysRowChanger) { return QuickImage.Get(ImageCode.Person); }

        if (this == Database.Column.SysRowChanger) { return QuickImage.Get(ImageCode.Person); }
        if (this == Database.Column.SysRowCreator) { return QuickImage.Get(ImageCode.Person); }
        if (this == Database.Column.SysRowCreateDate) { return QuickImage.Get(ImageCode.Uhr); }
        if (this == Database.Column.SysRowChangeDate) { return QuickImage.Get(ImageCode.Uhr); }

        if (this == Database.Column.SysLocked) { return QuickImage.Get(ImageCode.Schloss); }

        if (this == Database.Column.SysCorrect) { return QuickImage.Get(ImageCode.Warnung); }

        if (_format == DataFormat.RelationText) { return QuickImage.Get(ImageCode.Herz, 16); }

        if (_format == DataFormat.FarbeInteger) { return QuickImage.Get(ImageCode.Pinsel, 16); }

        if (_format == DataFormat.FarbeInteger) { return QuickImage.Get(ImageCode.Pinsel, 16); }
        if (_format == DataFormat.Button) { return QuickImage.Get(ImageCode.Kugel, 16); }
        if (_format == DataFormat.Verknüpfung_zu_anderer_Datenbank) { return QuickImage.Get(ImageCode.Fernglas, 16); }

        foreach (var thisFormat in FormatHolder.AllFormats) {
            if (thisFormat.IsFormatIdentical(this)) { return thisFormat.Image; }
        }

        if (_dropdownBearbeitungErlaubt) {
            return QuickImage.Get("Pfeil_Unten_Scrollbar|14|||||0");
        }

        if (_format.TextboxEditPossible()) {
            return _multiLine ? QuickImage.Get(ImageCode.Textfeld, 16, Color.Red, Color.Transparent) :
                           QuickImage.Get(ImageCode.Textfeld);
        }

        return null;
    }

    //        case FormatHolder.Url:
    //            SetFormatForUrl();
    //            break;
    public override string ToString() {
        if (IsDisposed) { return "Disposed"; }
        return Name + " -> " + Caption;
    }

    //        case FormatHolder.TextMitFormatierung:
    //            SetFormatForTextMitFormatierung();
    //            break;
    public bool UserEditDialogTypeInFormula(EditTypeFormula editTypeToCheck) {
        switch (_format) {
            case DataFormat.Text:
            case DataFormat.RelationText:
                if (editTypeToCheck == EditTypeFormula.Textfeld) { return true; } // Textfeld immer erlauben auch wenn beide Bearbeitungen nicht erlaubt sind. Einfach der Übersichtlichktei
                if (_multiLine && editTypeToCheck == EditTypeFormula.Textfeld_mit_Auswahlknopf) { return false; }
                if (_dropdownBearbeitungErlaubt && editTypeToCheck == EditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
                if (_dropdownBearbeitungErlaubt && _dropdownWerteAndererZellenAnzeigen && editTypeToCheck == EditTypeFormula.SwapListBox) { return true; }
                //if (_MultiLine && _DropdownBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Listbox_3_Zeilen) { return true; }
                if (_multiLine && _dropdownBearbeitungErlaubt && editTypeToCheck == EditTypeFormula.Listbox) { return true; }
                if (editTypeToCheck == EditTypeFormula.nur_als_Text_anzeigen) { return true; }
                if (!_multiLine && editTypeToCheck == EditTypeFormula.Ja_Nein_Knopf && _behaviorOfImageAndText == BildTextVerhalten.Interpretiere_Bool) { return true; }
                return false;

            case DataFormat.Verknüpfung_zu_anderer_Datenbank:
                if (editTypeToCheck == EditTypeFormula.None) { return true; }
                //if (EditType_To_Check != enEditTypeFormula.Textfeld &&
                //    EditType_To_Check != enEditTypeFormula.nur_als_Text_anzeigen) { return false; }
                //if (Database.IsLoading) { return true; }

                //var skriptgesteuert = LinkedCell_RowKeyIsInColumn == -9999;
                //if (skriptgesteuert) {
                //    return editTypeToCheck is enEditTypeFormula.Textfeld or enEditTypeFormula.nur_als_Text_anzeigen;
                //}

                //if (LinkedDatabase == null) { return false; }
                //if (string.IsNullOrEmpty_linkedCell_ColumnKeyOfLinkedDatabase < 0) { return false; }
                var col = LinkedDatabase?.Column.Exists(_linkedCell_ColumnNameOfLinkedDatabase);
                if (col == null) { return false; }
                return col.UserEditDialogTypeInFormula(editTypeToCheck);

            case DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems:
                if (editTypeToCheck == EditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
                return false;

            //case DataFormat.Bit:
            //    if (_MultiLine) { return false; }
            //    if (editType_To_Check == enEditTypeFormula.Ja_Nein_Knopf) {
            //        return !_DropdownWerteAndererZellenAnzeigen && DropDownItems.Count <= 0;
            //    }
            //    if (editType_To_Check == enEditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
            //    return false;

            //case DataFormat.Link_To_Filesystem:
            //    if (_multiLine) {
            //        //if (EditType_To_Check == enEditType.Listbox) { return true; }
            //        if (editTypeToCheck == EditTypeFormula.Gallery) { return true; }
            //    } else {
            //        if (editTypeToCheck == EditTypeFormula.EasyPic) { return true; }
            //    }
            //    return false;
            //case DataFormat.Relation:
            //    switch (EditType_To_Check)
            //    {
            //        case enEditTypeFormula.Listbox_1_Zeile:
            //        case enEditTypeFormula.Listbox_3_Zeilen:
            //        case enEditTypeFormula.Listbox_6_Zeilen:
            //            return true;
            //        default:
            //            return false;
            //    }
            case DataFormat.FarbeInteger:
                return editTypeToCheck == EditTypeFormula.Farb_Auswahl_Dialog;

            case DataFormat.Schrift:
                return editTypeToCheck == EditTypeFormula.Font_AuswahlDialog;

            case DataFormat.Button:
                //if (EditType_To_Check == enEditTypeFormula.Button) { return true; }
                return false;

            default:
                Develop.DebugPrint(_format);
                return false;
        }
    }

    /// <summary>
    /// Wenn sich ein Zelleninhalt verändert hat, muss die Spalte neu berechnet werden.
    /// </summary>
    internal void Invalidate_ContentWidth() => UnsavedContentWidth = -1;

    //        if (thisColumn.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
    //            foreach (var thisV in thisColumn._linkedCellFilter) {
    //                if (IntTryParse(thisV, out var key)) {
    //                    if (key == Key) { Am_A_Key_For_Other_Column = "Spalte " + thisColumn.ReadableText() + " verweist auf diese Spalte"; }
    //                }
    //            }
    //        }
    //    }
    //    //if (_format == DataFormat.Columns_für_LinkedCellDropdown) { Am_A_Key_For_Other_Column = "Die Spalte selbst durch das Format"; }
    //}
    internal void Invalidate_Head() {
        TmpCaptionTextSize = new SizeF(-1, -1);
        TmpCaptionBitmapCode = null;
    }

    //internal void CheckIfIAmAKeyColumn() {
    //    Am_A_Key_For_Other_Column = string.Empty;
    //    foreach (var thisColumn in Database.Column) {
    //        //if (thisColumn.KeyColumnKey == Key) { Am_A_Key_For_Other_Column = "Spalte " + thisColumn.ReadableText() + " verweist auf diese Spalte"; } // Werte Gleichhalten
    //        //if (thisColumn.LinkedCell_RowKeyIsInColumn == Key) { Am_A_Key_For_Other_Column = "Spalte " + thisColumn.ReadableText() + " verweist auf diese Spalte"; } // LinkdeCells pflegen
    //        //if (ThisColumn.LinkedCell_ColumnValueFoundIn == Key) { I_Am_A_Key_For_Other_Column = "Spalte " + ThisColumn.ReadableText() + " verweist auf diese Spalte"; } // LinkdeCells pflegen
    internal void Invalidate_LinkedDatabase() {
        if (_linkedDatabase != null) {
            _linkedDatabase.Cell.CellValueChanged -= _TMP_LinkedDatabase_Cell_CellValueChanged;
            _linkedDatabase.Disposing -= _TMP_LinkedDatabase_Disposing;
            _linkedDatabase = null;
        }
    }

    //    for (var z = 0; z <= 999; z++) {
    //        var w = (EditTypeFormula)z;
    //        if (w.ToString() != z.ToString()) {
    //            if (UserEditDialogTypeInFormula(w)) {
    //                return w;
    //            }
    //        }
    //    }
    //    return EditTypeFormula.None;
    //}
    /// <summary>
    /// Setzt den Wert in die dazugehörige Variable.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="newvalue"></param>
    /// <returns></returns>
    internal string SetValueInternal(DatabaseDataType type, string newvalue, bool isLoading) {
        if (type.IsObsolete()) { return string.Empty; }

        switch (type) {
            //case DatabaseDataType.ColumnKey:
            //    var nkey = LongParse(newvalue);

            //    if (nkey == 0 && newvalue.Contains(",")) {
            //        // Wiederverwendeter wert:  co_DauerFilterPos = 194 {x=0, y=0}
            //        return string.Empty;
            //    }

            //    _key = nkey;
            //    //Invalidate_TmpVariablesx();
            //    break;

            case DatabaseDataType.ColumnName:
                var nname = newvalue.ToUpper();

                var ok = Database?.Column.ChangeName(KeyName, nname) ?? false;

                if (!ok) {
                    Database?.SetReadOnly();
                    return "Schwerer Spalten Umbenennungsfehler!";
                }

                KeyName = nname;
                //Invalidate_TmpVariablesx();
                break;

            case DatabaseDataType.ColumnCaption:
                _caption = newvalue;
                break;

            case DatabaseDataType.ColumnFormat:
                _format = (DataFormat)IntParse(newvalue);
                break;

            case DatabaseDataType.ForeColor:
                _foreColor = Color.FromArgb(IntParse(newvalue));
                break;

            case DatabaseDataType.BackColor:
                _backColor = Color.FromArgb(IntParse(newvalue));
                break;

            case DatabaseDataType.LineStyleLeft:
                _lineLeft = (ColumnLineStyle)IntParse(newvalue);
                break;

            case DatabaseDataType.LineStyleRight:
                _lineRight = (ColumnLineStyle)IntParse(newvalue);
                break;

            case DatabaseDataType.ColumnQuickInfo:
                _quickInfo = newvalue;
                break;

            case DatabaseDataType.CaptionGroup1:
                _captionGroup1 = newvalue;
                break;

            case DatabaseDataType.CaptionGroup2:
                _captionGroup2 = newvalue;
                break;

            case DatabaseDataType.CaptionGroup3:
                _captionGroup3 = newvalue;
                break;

            case DatabaseDataType.MultiLine:
                _multiLine = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.DropDownItems:
                _dropDownItems.SplitAndCutByCr_QuickSortAndRemoveDouble(newvalue);
                break;

            case DatabaseDataType.LinkedCellFilter:
                _linkedCellFilter.SplitAndCutByCr(newvalue);
                break;

            case DatabaseDataType.OpticalTextReplace:
                _opticalReplace.SplitAndCutByCr(newvalue);
                break;

            case DatabaseDataType.AutoReplaceAfterEdit:
                _afterEditAutoReplace.SplitAndCutByCr(newvalue);
                break;

            case DatabaseDataType.RegexCheck:
                _regex = newvalue;
                break;

            case DatabaseDataType.ColumnTags:
                _tags.SplitAndCutByCr(newvalue);
                break;

            case DatabaseDataType.AutoFilterJoker:
                _autoFilterJoker = newvalue;
                break;

            case DatabaseDataType.PermissionGroupsChangeCell:
                _permissionGroupsChangeCell.SplitAndCutByCr_QuickSortAndRemoveDouble(newvalue);
                break;

            case DatabaseDataType.AllowedChars:
                _allowedChars = newvalue;
                break;

            case DatabaseDataType.MaxTextLenght:
                _maxTextLenght = IntParse(newvalue);
                break;

            case DatabaseDataType.FilterOptions:
                _filterOptions = (FilterOptions)IntParse(newvalue);
                break;

            case DatabaseDataType.IgnoreAtRowFilter:
                _ignoreAtRowFilter = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.ShowUndo:
                _showUndo = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.ShowMultiLineInOneLine:
                _showMultiLineInOneLine = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.EditableWithTextInput:
                _textBearbeitungErlaubt = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.EditableWithDropdown:
                _dropdownBearbeitungErlaubt = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.SpellCheckingEnabled:
                _spellCheckingEnabled = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.DropdownDeselectAllAllowed:
                _dropdownAllesAbwählenErlaubt = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.ShowValuesOfOtherCellsInDropdown:
                _dropdownWerteAndererZellenAnzeigen = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.SortAndRemoveDoubleAfterEdit:
                _afterEditQuickSortRemoveDouble = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.RoundAfterEdit:
                _afterEditRunden = IntParse(newvalue);
                break;

            case DatabaseDataType.DoUcaseAfterEdit:
                _afterEditDoUCase = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.AutoCorrectAfterEdit:
                _afterEditAutoCorrect = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.SaveContent:
                _saveContent = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.AutoRemoveCharAfterEdit:
                _autoRemove = newvalue;
                break;

            case DatabaseDataType.ColumnAdminInfo:
                _adminInfo = newvalue;
                break;

            //case DatabaseDataType.ColumnTimeCode:
            //    _timecode = newvalue;
            //    break;

            case DatabaseDataType.ColumnContentWidth:
                _contentwidth = IntParse(newvalue);
                UnsavedContentWidth = _contentwidth;
                break;

            case DatabaseDataType.CaptionBitmapCode:
                _captionBitmapCode = newvalue;
                break;

            case DatabaseDataType.Suffix:
                _suffix = newvalue;
                break;

            case DatabaseDataType.LinkedDatabase:
                _linkedDatabaseFile = newvalue;
                Invalidate_LinkedDatabase();
                break;

            case DatabaseDataType.ConstantHeightOfImageCode:
                if (newvalue == "0") { newvalue = string.Empty; }
                _constantHeightOfImageCode = newvalue;
                break;

            case DatabaseDataType.Prefix:
                _prefix = newvalue;
                break;

            case DatabaseDataType.DoOpticalTranslation:
                _doOpticalTranslation = (TranslationType)IntParse(newvalue);
                break;

            case DatabaseDataType.AdditionalFormatCheck:
                _additionalFormatCheck = (AdditionalCheck)IntParse(newvalue);
                break;

            case DatabaseDataType.ScriptType:
                _scriptType = (ScriptType)IntParse(newvalue);
                Database?.Row.InvalidateAllCheckData();
                break;

            case DatabaseDataType.BehaviorOfImageAndText:
                _behaviorOfImageAndText = (BildTextVerhalten)IntParse(newvalue);
                break;

            case DatabaseDataType.EditAllowedDespiteLock:
                _editAllowedDespiteLock = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.TextFormatingAllowed:
                _formatierungErlaubt = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.CellInitValue:
                _cellInitValue = newvalue;
                break;

            //case DatabaseDataType.KeyColumnKey:
            //    _keyColumnKey = LongParse(newvalue);
            //    break;

            case DatabaseDataType.ColumnNameOfLinkedDatabase:

                if (newvalue.IsFormat(FormatHolder.Integer)) {
                    _linkedCell_ColumnNameOfLinkedDatabase = string.Empty;
                } else {
                    _linkedCell_ColumnNameOfLinkedDatabase = newvalue;
                }

                //var l = LongParse(newvalue);
                //if (l < 0) {
                //    _linkedCell_ColumnNameOfLinkedDatabase = string.Empty;
                //} else
                //if (l > 0) {
                //    _linkedCell_ColumnNameOfLinkedDatabase = Database?.Column.SearchByKey(l)?.Name ?? string.Empty;
                //} else {
                //    _linkedCell_ColumnNameOfLinkedDatabase = newvalue;
                //}
                break;

            case DatabaseDataType.SortType:
                if (string.IsNullOrEmpty(newvalue)) {
                    _sortType = SortierTyp.Original_String;
                } else {
                    _sortType = (SortierTyp)LongParse(newvalue);
                }
                break;

            //case DatabaseDataType.MakeSuggestionFromSameKeyColumn:
            //    _vorschlagsColumn = LongParse(newvalue);
            //    break;

            case DatabaseDataType.ColumnAlign:
                var tmpalign = (AlignmentHorizontal)IntParse(newvalue);
                if (tmpalign == (AlignmentHorizontal)(-1)) { tmpalign = AlignmentHorizontal.Links; }
                _align = tmpalign;
                break;

            default:
                if (!string.Equals(type.ToString(), ((int)type).ToString(), StringComparison.Ordinal)) {
                    return "Interner Fehler: Für den Datentyp '" + type + "' wurde keine Laderegel definiert.";
                }
                break;
        }
        return string.Empty;
    }

    //    foreach (int z1 in Enum.GetValues(t)) {
    //        if (column.UserEditDialogTypeInFormula((EditTypeFormula)z1)) {
    //            l.Add(new TextListItem(Enum.GetName(t, z1).Replace("_", " "), z1.ToString(), null, false, true, string.Empty));
    //        }
    //    }
    private void _TMP_LinkedDatabase_Cell_CellValueChanged(object sender, CellEventArgs e) {
        if (Database == null || Database.IsDisposed) { return; }

        var tKey = CellCollection.KeyOfCell(e.Column, e.Row);
        foreach (var thisRow in Database.Row) {
            if (Database.Cell.GetStringBehindLinkedValue(this, thisRow) == tKey) {
                //CellCollection.Invalidate_CellContentSize(this, thisRow);
                Invalidate_ContentWidth();
                Database.Cell.OnCellValueChanged(new CellEventArgs(this, thisRow));
                //_ = thisRow.ExecuteScript(EventTypes.value_changedx, string.Empty, true, false, true, 5);
                thisRow.Database?.AddRowWithChangedValue(thisRow.Key);
            }
        }
    }

    //    var t = typeof(EditTypeFormula);
    private void _TMP_LinkedDatabase_Disposing(object sender, System.EventArgs e) {
        Invalidate_LinkedDatabase();
        Database?.Dispose();
    }

    //        case FormatHolder.PhoneNumber:
    //            SetFormatForPhoneNumber();
    //            break;
    //internal EditTypeFormula CheckFormulaEditType(EditTypeFormula toCheck) {
    //    if (UserEditDialogTypeInFormula(toCheck)) { return toCheck; }// Alles OK!
    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    //        case FormatHolder.IntegerPositive:
    //            SetFormatForIntegerPositive();
    //            break;
    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            if (Database != null && !Database.IsDisposed) { Database.Disposing -= Database_Disposing; }
            Invalidate_LinkedDatabase();
            Database = null;

            //DropDownItems.Changed -= DropDownItems_ListOrItemChanged;
            //DropDownItems?.Dispose();

            //LinkedCellFilter.Changed -= LinkedCellFilters_ListOrItemChanged;
            //LinkedCellFilter?.Dispose();

            //Tags.Changed -= Tags_ListOrItemChanged;
            //Tags?.Dispose();

            //PermissionGroupsChangeCell.Changed -= PermissionGroups_ChangeCell_ListOrItemChanged;
            //PermissionGroupsChangeCell?.Dispose();

            //OpticalReplace.Changed -= OpticalReplacer_ListOrItemChanged;
            //OpticalReplace?.Dispose();

            //AfterEditAutoReplace.Changed -= AfterEdit_AutoReplace_ListOrItemChanged;
            //AfterEditAutoReplace?.Dispose();
            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    private void GetLinkedDatabase() {
        Invalidate_LinkedDatabase(); // Um evtl. Events zu löschen

        if (Database == null || Database.IsDisposed) { return; }

        if (!_linkedDatabaseFile.Contains("|") && !_linkedDatabaseFile.IsFormat(FormatHolder.FilepathAndName)) {
            _linkedDatabaseFile = _linkedDatabaseFile.ToUpper().TrimEnd(".MDB");
            _linkedDatabaseFile = SQLBackAbstract.MakeValidTableName(_linkedDatabaseFile);
        }

        if (SQLBackAbstract.IsValidTableName(_linkedDatabaseFile)) {
            _linkedDatabase = Database.GetOtherTable(_linkedDatabaseFile);
        }

        if (_linkedDatabase == null) {
            var ci = new ConnectionInfo(_linkedDatabaseFile, null);

            _linkedDatabase = DatabaseAbstract.GetById(ci, null);
            if (_linkedDatabase != null) {
                _linkedDatabase.Cell.CellValueChanged += _TMP_LinkedDatabase_Cell_CellValueChanged;
                _linkedDatabase.Disposing += _TMP_LinkedDatabase_Disposing;
            }
        }

        if (_linkedDatabase != null) {
            _linkedDatabase.UserGroup = Database.UserGroup;
        }
    }

    //public void SetFormat(VarType format) {
    //    switch (format) {
    //        case FormatHolder.Text:
    //            SetFormatForText();
    //            break;

    //        case FormatHolder.Date:
    //            SetFormatForDate();
    //            break;

    //        case FormatHolder.DateTime:
    //            SetFormatForDateTime(true);
    //            break;

    //        case FormatHolder.Email:
    //            SetFormatForEmail();
    //            break;

    //        case FormatHolder.Float:
    //            SetFormatForFloat();
    //            break;

    //        case FormatHolder.FloatPositive:
    //            SetFormatForFloatPositive();
    //            break;

    //        case FormatHolder.Integer:
    //            SetFormatForInteger();
    //            break;
    private string KleineFehlerCorrect(string txt) {
        if (string.IsNullOrEmpty(txt)) { return string.Empty; }
        const char h4 = (char)1004; // H4 = Normaler Text, nach links rutschen
        const char h3 = (char)1003; // überschrift
        const char h2 = (char)1002; // überschrift
        const char h1 = (char)1001; // überschrift
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
            // 180°C
            // Nach Zahlen KEINE leerzeichen einfügen. Es gibt so viele dinge.... 90er Schichtsalat
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
            txt = txt.Replace(h3 + " ", " " + h3); // Überschrift, nach Rechts
            txt = txt.Replace(h2 + " ", " " + h2); // Überschrift, nach Rechts
            txt = txt.Replace(h1 + " ", " " + h1); // Überschrift, nach Rechts
            txt = txt.Replace(h7 + " ", " " + h7); // Bold, nach Rechts
            txt = txt.Replace(h3 + "\r", "\r" + h3); // Überschrift, nach Rechts
            txt = txt.Replace(h2 + "\r", "\r" + h2); // Überschrift, nach Rechts
            txt = txt.Replace(h1 + "\r", "\r" + h1); // Überschrift, nach Rechts
            txt = txt.Replace(h7 + "\r", "\r" + h7); // Bold, nach Rechts
            txt = txt.Replace(h7 + h4.ToString(), h4.ToString());
            txt = txt.Replace(h3 + h4.ToString(), h4.ToString());
            txt = txt.Replace(h2 + h4.ToString(), h4.ToString());
            txt = txt.Replace(h1 + h4.ToString(), h4.ToString());
            txt = txt.Replace(h4 + h4.ToString(), h4.ToString());
            txt = txt.Replace(" °", "°");
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

    #endregion
}