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
using static BlueDatabase.DatabaseAbstract;
using static BlueBasics.Constants;

namespace BlueDatabase;

public sealed class ColumnItem : IReadableTextWithChangingAndKey, IDisposableExtended, IColumnInputFormat, IErrorCheckable, IHasDatabase, IHasKeyName {

    #region Fields

    public int? Contentwidth;

    public DateTime? IsInCache = null;

    public QuickImage? TmpCaptionBitmapCode;

    public SizeF TmpCaptionTextSize = new(-1, -1);

    public int? TmpIfFilterRemoved = null;

    internal List<string>? UcaseNamesSortedByLenght;

    private const string TmpNewDummy = "TMPNEWDUMMY";

    private readonly List<string> _afterEditAutoReplace = [];

    private readonly List<string> _dropDownItems = [];

    private readonly List<string> _linkedCellFilter = [];

    private readonly List<string> _opticalReplace = [];

    private readonly List<string> _permissionGroupsChangeCell = [];

    private readonly List<string> _tags = [];

    private AdditionalCheck _additionalFormatCheck;

    private string _adminInfo;

    private bool _afterEditAutoCorrect;

    private bool _afterEditDoUCase;

    private bool _afterEditQuickSortRemoveDouble;

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

    private TranslationType _doOpticalTranslation;

    private bool _dropdownAllesAbwählenErlaubt;

    private bool _dropdownBearbeitungErlaubt;

    private bool _dropdownWerteAndererZellenAnzeigen;

    private bool _editAllowedDespiteLock;

    private FilterOptions _filterOptions;

    private int _fixedColumnWidth;

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

    private string _linkedDatabaseTableName;

    private int _maxCellLenght;

    private int _maxTextLenght;

    private bool _multiLine;

    private string _name;

    private string _prefix;

    private string _quickInfo;

    private string _regex = string.Empty;

    private int _roundAfterEdit;

    //private bool _saveContent;
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
        Database.DisposingEvent += Database_Disposing;

        var ex = database.Column.Exists(name);
        if (ex != null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Key existiert bereits");
        }

        //_key = database.Column.NextColumnKey();

        #region Standard-Werte

        _name = name;
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
        _maxCellLenght = 4000;
        Contentwidth = null;
        //ContentWidthIsValid = false;
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
        _roundAfterEdit = -1;
        _fixedColumnWidth = 0;
        _afterEditAutoCorrect = false;
        _afterEditDoUCase = false;
        _formatierungErlaubt = false;
        _additionalFormatCheck = AdditionalCheck.None;
        _scriptType = ScriptType.undefiniert;
        _autoRemove = string.Empty;
        _autoFilterJoker = string.Empty;
        //_saveContent = true;
        //_AutoFilter_Dauerfilter = enDauerfilter.ohne;
        _spellCheckingEnabled = false;
        //_CompactView = true;
        _showUndo = true;
        _doOpticalTranslation = TranslationType.Original_Anzeigen;
        _showMultiLineInOneLine = false;
        _editAllowedDespiteLock = false;
        _suffix = string.Empty;
        _linkedDatabaseTableName = string.Empty;
        _behaviorOfImageAndText = BildTextVerhalten.Nur_Text;
        _constantHeightOfImageCode = string.Empty;
        _prefix = string.Empty;
        UcaseNamesSortedByLenght = null;
        Am_A_Key_For_Other_Column = string.Empty;

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
            if (IsDisposed) { return; }
            if (_additionalFormatCheck == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.AdditionalFormatCheck, this, null, ((int)_additionalFormatCheck).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    //    #endregion Standard-Werte
    public string AdminInfo {
        get => _adminInfo;
        set {
            if (IsDisposed) { return; }
            if (_adminInfo == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnAdminInfo, this, null, _adminInfo, value, Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public bool AfterEditAutoCorrect {
        get => _afterEditAutoCorrect;
        set {
            if (IsDisposed) { return; }
            if (_afterEditAutoCorrect == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.AutoCorrectAfterEdit, this, null, _afterEditAutoCorrect.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    //    #region Standard-Werte
    public ReadOnlyCollection<string> AfterEditAutoReplace {
        get => new(_afterEditAutoReplace);
        set {
            if (IsDisposed) { return; }
            if (!_afterEditAutoReplace.IsDifferentTo(value)) { return; }

            _ = Database?.ChangeData(DatabaseDataType.AutoReplaceAfterEdit, this, null, _afterEditAutoReplace.JoinWithCr(), value.JoinWithCr(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    //    _key = columnkey;
    public bool AfterEditDoUCase {
        get => _afterEditDoUCase;
        set {
            if (IsDisposed) { return; }
            if (_afterEditDoUCase == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.DoUcaseAfterEdit, this, null, _afterEditDoUCase.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    //    var ex = database.Column.SearchByKey(columnkey);
    //    if (ex != null) {
    //        Develop.DebugPrint(FehlerArt.Fehler, "_name existiert bereits");
    //    }
    public bool AfterEditQuickSortRemoveDouble {
        get => _multiLine && _afterEditQuickSortRemoveDouble;
        set {
            if (IsDisposed) { return; }
            if (_afterEditQuickSortRemoveDouble == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.SortAndRemoveDoubleAfterEdit, this, null, _afterEditQuickSortRemoveDouble.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public AlignmentHorizontal Align {
        get => _align;
        set {
            if (IsDisposed) { return; }
            if (_align == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnAlign, this, null, ((int)_align).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public string AllowedChars {
        get => _allowedChars;
        set {
            if (IsDisposed) { return; }
            if (_allowedChars == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.AllowedChars, this, null, _allowedChars, value, Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public string Am_A_Key_For_Other_Column { get; private set; }

    public string AutoFilterJoker {
        get => _autoFilterJoker;
        set {
            if (IsDisposed) { return; }
            if (_autoFilterJoker == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.AutoFilterJoker, this, null, _autoFilterJoker, value, Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public string AutoRemove {
        get => _autoRemove;
        set {
            if (IsDisposed) { return; }
            if (_autoRemove == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.AutoRemoveCharAfterEdit, this, null, _autoRemove, value, Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public Color BackColor {
        get => _backColor;
        set {
            if (IsDisposed) { return; }
            if (_backColor.ToArgb() == value.ToArgb()) { return; }

            _ = Database?.ChangeData(DatabaseDataType.BackColor, this, null, _backColor.ToArgb().ToString(), value.ToArgb().ToString(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public BildTextVerhalten BehaviorOfImageAndText {
        get => _behaviorOfImageAndText;
        set {
            if (IsDisposed) { return; }
            if (_behaviorOfImageAndText == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.BehaviorOfImageAndText, this, null, ((int)_behaviorOfImageAndText).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public string Caption {
        get => _caption;
        set {
            if (IsDisposed) { return; }
            value = value.Replace("<br>", "\r", RegexOptions.IgnoreCase);
            if (_caption == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnCaption, this, null, _caption, value, Generic.UserName, DateTime.UtcNow, string.Empty);
            Invalidate_Head();
            OnChanged();
        }
    }

    public string CaptionBitmapCode {
        get => _captionBitmapCode;
        set {
            if (IsDisposed) { return; }
            if (_captionBitmapCode == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.CaptionBitmapCode, this, null, _captionBitmapCode, value, Generic.UserName, DateTime.UtcNow, string.Empty);
            _captionBitmapCode = value;
            Invalidate_Head();
            OnChanged();
        }
    }

    public string CaptionGroup1 {
        get => _captionGroup1;
        set {
            if (IsDisposed) { return; }
            if (_captionGroup1 == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.CaptionGroup1, this, null, _captionGroup1, value, Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public string CaptionGroup2 {
        get => _captionGroup2;
        set {
            if (IsDisposed) { return; }
            if (_captionGroup2 == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.CaptionGroup2, this, null, _captionGroup2, value, Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public string CaptionGroup3 {
        get => _captionGroup3;
        set {
            if (IsDisposed) { return; }
            if (_captionGroup3 == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.CaptionGroup3, this, null, _captionGroup3, value, Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public string CellInitValue {
        get => _cellInitValue;
        set {
            if (IsDisposed) { return; }
            if (_cellInitValue == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.CellInitValue, this, null, _cellInitValue, value, Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public string ConstantHeightOfImageCode {
        get => _constantHeightOfImageCode;
        set {
            if (IsDisposed) { return; }
            if (_constantHeightOfImageCode == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ConstantHeightOfImageCode, this, null, _constantHeightOfImageCode, value, Generic.UserName, DateTime.UtcNow, string.Empty);
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    public DatabaseAbstract? Database { get; private set; }

    public TranslationType DoOpticalTranslation {
        get => _doOpticalTranslation;
        set {
            if (IsDisposed) { return; }
            if (_doOpticalTranslation == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.DoOpticalTranslation, this, null, ((int)_doOpticalTranslation).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public bool DropdownAllesAbwählenErlaubt {
        get => _dropdownAllesAbwählenErlaubt;
        set {
            if (IsDisposed) { return; }
            if (_dropdownAllesAbwählenErlaubt == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.DropdownDeselectAllAllowed, this, null, _dropdownAllesAbwählenErlaubt.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public bool DropdownBearbeitungErlaubt {
        get => _dropdownBearbeitungErlaubt;
        set {
            if (IsDisposed) { return; }
            if (_dropdownBearbeitungErlaubt == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.EditableWithDropdown, this, null, _dropdownBearbeitungErlaubt.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public ReadOnlyCollection<string> DropDownItems {
        get => new(_dropDownItems);
        set {
            if (IsDisposed) { return; }
            if (!_dropDownItems.IsDifferentTo(value)) { return; }

            _ = Database?.ChangeData(DatabaseDataType.DropDownItems, this, null, _dropDownItems.JoinWithCr(), value.JoinWithCr(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public bool DropdownWerteAndererZellenAnzeigen {
        get => _dropdownWerteAndererZellenAnzeigen;
        set {
            if (IsDisposed) { return; }
            if (_dropdownWerteAndererZellenAnzeigen == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ShowValuesOfOtherCellsInDropdown, this, null, _dropdownWerteAndererZellenAnzeigen.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public bool EditAllowedDespiteLock {
        get => _editAllowedDespiteLock;
        set {
            if (IsDisposed) { return; }
            if (_editAllowedDespiteLock == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.EditAllowedDespiteLock, this, null, _editAllowedDespiteLock.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public FilterOptions FilterOptions {
        get => _filterOptions;
        set {
            if (IsDisposed) { return; }
            if (_filterOptions == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.FilterOptions, this, null, ((int)_filterOptions).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty);
            Invalidate_Head();
            OnChanged();
        }
    }

    public int FixedColumnWidth {
        get => _fixedColumnWidth;
        set {
            if (IsDisposed) { return; }
            if (_fixedColumnWidth == value) { return; }
            _ = Database?.ChangeData(DatabaseDataType.FixedColumnWidth, this, null, _fixedColumnWidth.ToString(), value.ToString(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public Color ForeColor {
        get => _foreColor;
        set {
            if (IsDisposed) { return; }
            if (_foreColor.ToArgb() == value.ToArgb()) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ForeColor, this, null, _foreColor.ToArgb().ToString(), value.ToArgb().ToString(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public DataFormat Format {
        get => _format;
        set {
            if (IsDisposed) { return; }
            if (_format == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnFormat, this, null, ((int)_format).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty);
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    public bool FormatierungErlaubt {
        get => _formatierungErlaubt;
        set {
            if (IsDisposed) { return; }
            if (_formatierungErlaubt == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.TextFormatingAllowed, this, null, _formatierungErlaubt.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public bool IgnoreAtRowFilter {
        get => !_format.Autofilter_möglich() || _ignoreAtRowFilter;
        set {
            if (IsDisposed) { return; }
            if (_ignoreAtRowFilter == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.IgnoreAtRowFilter, this, null, _ignoreAtRowFilter.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public bool IsDisposed { get; private set; }

    public string KeyName {
        get => _name.ToUpper();
        set {
            if (IsDisposed) { return; }
            value = value.ToUpper();
            if (value == _name.ToUpper()) { return; }

            if (!ColumNameAllowed(value)) {
                Develop.DebugPrint(FehlerArt.Warnung, "Spaltenname nicht erlaubt: " + _name);
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

            _ = Database?.ChangeData(DatabaseDataType.ColumnName, this, null, _name, value, Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
            CheckIfIAmAKeyColumn();
        }
    }

    public ColumnLineStyle LineLeft {
        get => _lineLeft;
        set {
            if (IsDisposed) { return; }
            if (_lineLeft == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.LineStyleLeft, this, null, ((int)_lineLeft).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    //        var c = Database?.Column.SearchByKey(_keyColumnKey);
    //        c?.CheckIfIAmAKeyColumn();
    //        Database?.ChangeData(DatabaseDataType.KeyColumnKey, _name, null, _keyColumnKey.ToString(false), value.ToString(false), string.Empty);
    //        c = Database?.Column.SearchByKey(_keyColumnKey);
    //        c?.CheckIfIAmAKeyColumn();
    //        OnChanged();
    //    }
    //}
    public ColumnLineStyle LineRight {
        get => _lineRight;
        set {
            if (IsDisposed) { return; }
            if (_lineRight == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.LineStyleRight, this, null, ((int)_lineRight).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public string LinkedCell_ColumnNameOfLinkedDatabase {
        get => _linkedCell_ColumnNameOfLinkedDatabase;
        set {
            if (IsDisposed) { return; }
            if (value == "-1") { value = string.Empty; }

            if (_linkedCell_ColumnNameOfLinkedDatabase == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnNameOfLinkedDatabase, this, null, _linkedCell_ColumnNameOfLinkedDatabase, value, Generic.UserName, DateTime.UtcNow, string.Empty);
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    //        _ = (Database?.ChangeData(DatabaseDataType.ColumnKey, Name, null, _name, value.ToString(), string.Empty));
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
            if (IsDisposed) { return; }
            if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

            value = value.SortedDistinctList();

            if (!_linkedCellFilter.IsDifferentTo(value)) { return; }

            _ = db.ChangeData(DatabaseDataType.LinkedCellFilter, this, null, _linkedCellFilter.JoinWithCr(), value.JoinWithCr(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();

            foreach (var thisColumn in db.Column) {
                thisColumn.CheckIfIAmAKeyColumn();
            }
        }
    }

    public DatabaseAbstract? LinkedDatabase {
        get {
            if (Database is not DatabaseAbstract db || db.IsDisposed) { return null; }

            if (string.IsNullOrEmpty(_linkedDatabaseTableName)) { return null; }

            if (_linkedDatabase != null && !_linkedDatabase.IsDisposed) {
                return _linkedDatabase;
            }

            GetLinkedDatabase();
            return _linkedDatabase;
        }
    }

    public string LinkedDatabaseTableName {
        get => _linkedDatabaseTableName;
        set {
            if (IsDisposed) { return; }
            if (_linkedDatabaseTableName == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.LinkedDatabase, this, null, _linkedDatabaseTableName, value, Generic.UserName, DateTime.UtcNow, string.Empty);
            Invalidate_LinkedDatabase();
            OnChanged();
        }
    }

    public int MaxCellLenght {
        get => _maxCellLenght;
        set {
            if (IsDisposed) { return; }
            if (_maxCellLenght == value) { return; }
            _ = Database?.ChangeData(DatabaseDataType.MaxCellLenght, this, null, _maxCellLenght.ToString(), value.ToString(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public int MaxTextLenght {
        get => _maxTextLenght;
        set {
            if (IsDisposed) { return; }
            if (_maxTextLenght == value) { return; }
            _ = Database?.ChangeData(DatabaseDataType.MaxTextLenght, this, null, _maxTextLenght.ToString(), value.ToString(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public bool MultiLine {
        get => _multiLine;
        set {
            if (IsDisposed) { return; }
            if (!_format.MultilinePossible()) { value = false; }

            if (_multiLine == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.MultiLine, this, null, _multiLine.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty);
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    public ReadOnlyCollection<string> OpticalReplace {
        get => new(_opticalReplace);
        set {
            if (IsDisposed) { return; }
            if (!_opticalReplace.IsDifferentTo(value)) { return; }

            _ = Database?.ChangeData(DatabaseDataType.OpticalTextReplace, this, null, _opticalReplace.JoinWithCr(), value.JoinWithCr(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public ReadOnlyCollection<string> PermissionGroupsChangeCell {
        get => new(_permissionGroupsChangeCell);
        set {
            if (IsDisposed) { return; }
            if (!_permissionGroupsChangeCell.IsDifferentTo(value)) { return; }

            _ = Database?.ChangeData(DatabaseDataType.PermissionGroupsChangeCell, this, null, _permissionGroupsChangeCell.JoinWithCr(), value.JoinWithCr(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public string Prefix {
        get => _prefix;
        set {
            if (IsDisposed) { return; }
            if (_prefix == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.Prefix, this, null, _prefix, value, Generic.UserName, DateTime.UtcNow, string.Empty);
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    public string Quickinfo {
        get => _quickInfo;
        set {
            if (IsDisposed) { return; }
            if (_quickInfo == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnQuickInfo, this, null, _quickInfo, value, Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public string Regex {
        get => _regex;
        set {
            if (IsDisposed) { return; }
            if (_regex == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.RegexCheck, this, null, _regex, value, Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    //    Database = database;
    //    Database.DisposingEvent += Database_Disposing;
    //    if (columnkey < 0) {
    //        Develop.DebugPrint(FehlerArt.Fehler, "ColumnKey < 0");
    //    }
    public int RoundAfterEdit {
        get => _roundAfterEdit;
        set {
            if (IsDisposed) { return; }
            if (_roundAfterEdit == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.RoundAfterEdit, this, null, _roundAfterEdit.ToString(), value.ToString(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public ScriptType ScriptType {
        get => _scriptType;
        set {
            if (IsDisposed) { return; }
            if (_scriptType == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ScriptType, this, null, ((int)_scriptType).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    //        _ = Database?.ChangeData(DatabaseDataType.SaveContent, this, null, _saveContent.ToPlusMinus(), value.ToPlusMinus(), string.Empty);
    //        OnChanged();
    //    }
    //}
    public bool ShowMultiLineInOneLine {
        get => _multiLine && _showMultiLineInOneLine;
        set {
            if (IsDisposed) { return; }
            if (_showMultiLineInOneLine == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ShowMultiLineInOneLine, this, null, _showMultiLineInOneLine.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty);
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    //public bool SaveContent {
    //    get => _saveContent;
    //    set {
    //        if (_saveContent == value) { return; }
    public bool ShowUndo {
        get => _showUndo;
        set {
            if (IsDisposed) { return; }
            if (_showUndo == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ShowUndo, this, null, _showUndo.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public SortierTyp SortType {
        get => _sortType;
        set {
            if (IsDisposed) { return; }
            if (_sortType == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.SortType, this, null, ((int)_sortType).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public bool SpellCheckingEnabled {
        get => _spellCheckingEnabled;
        set {
            if (IsDisposed) { return; }
            if (_spellCheckingEnabled == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.SpellCheckingEnabled, this, null, _spellCheckingEnabled.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public string Suffix {
        get => _suffix;
        set {
            if (IsDisposed) { return; }
            if (_suffix == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.Suffix, this, null, _suffix, value, Generic.UserName, DateTime.UtcNow, string.Empty);
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
            if (IsDisposed) { return; }
            if (!_tags.IsDifferentTo(value)) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnTags, this, null, _tags.JoinWithCr(), value.JoinWithCr(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public bool TextBearbeitungErlaubt {
        get => Database?.PowerEdit.Subtract(DateTime.UtcNow).TotalSeconds > 0 || _textBearbeitungErlaubt;
        set {
            if (IsDisposed) { return; }
            if (_textBearbeitungErlaubt == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.EditableWithTextInput, this, null, _textBearbeitungErlaubt.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty);
            OnChanged();
        }
    }

    public string Ueberschriften {
        get {
            var txt = _captionGroup1 + "/" + _captionGroup2 + "/" + _captionGroup3;
            return txt == "//" ? "###" : txt.TrimEnd("/");
        }
    }

    #endregion

    #region Methods

    public static bool IsValidColumnName(string name) {
        if (string.IsNullOrWhiteSpace(name)) { return false; }

        if (!name.ContainsOnlyChars(Constants.AllowedCharsVariableName)) { return false; }

        if (!Constants.Char_AZ.Contains(name.Substring(0, 1).ToUpper())) { return false; }
        if (name.Length > 128) { return false; }

        if (name.ToUpper() == "USER") { return false; } // SQL System-Name
        if (name.ToUpper() == "COMMENT") { return false; } // SQL System-Name
        if (name.ToUpper() == "TABLE_NAME") { return false; } // SQL System-Name
        if (name.ToUpper() == "COLUMN_NAME") { return false; } // SQL System-Name
        if (name.ToUpper() == "OWNER") { return false; } // SQL System-Name
        if (name.ToUpper() == "DATA_TYPE") { return false; } // SQL System-Name
        if (name.ToUpper() == "DATA_LENGTH") { return false; } // SQL System-Name
        if (name.ToUpper() == "OFFLINE") { return false; } // SQL System-Name
        if (name.ToUpper() == "ONLINE") { return false; } // SQL System-Name

        if (name.ToUpper() == TmpNewDummy) { return false; } // BlueDatabase name bei neuen Spalten

        return true;
    }

    public static EditTypeTable UserEditDialogTypeInTable(ColumnItem? column, bool preverDropDown) {
        if (column == null || column.IsDisposed) { return EditTypeTable.None; }
        return UserEditDialogTypeInTable(column.Format, preverDropDown && column.DropdownBearbeitungErlaubt, column.TextBearbeitungErlaubt, column.MultiLine);
    }

    public string AutoCorrect(string value, bool exitifLinkedFormat) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return value; }

        if (IsSystemColumn()) { return value; }

        if (exitifLinkedFormat) {
            if (_format is DataFormat.Verknüpfung_zu_anderer_Datenbank or
                DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) { return value; }
        }

        if (_afterEditDoUCase) { value = value.ToUpper(); }
        if (!string.IsNullOrEmpty(_autoRemove)) { value = value.RemoveChars(_autoRemove); }
        if (_afterEditAutoReplace.Count > 0) {
            List<string> l = [.. value.SplitAndCutByCr()];
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
        if (_roundAfterEdit > -1 && DoubleTryParse(value, out var erg)) {
            erg = Math.Round(erg, _roundAfterEdit);
            value = erg.ToString(CultureInfo.InvariantCulture);
        }
        if (_afterEditQuickSortRemoveDouble) {
            var l = new List<string>(value.SplitAndCutByCr()).SortedDistinctList();
            value = l.JoinWithCr();
        }

        return value.CutToUtf8Length(_maxCellLenght);
    }

    public bool AutoFilterSymbolPossible() => FilterOptions.HasFlag(FilterOptions.Enabled) && Format.Autofilter_möglich();

    public int CalculatePreveredMaxCellLenght(double prozentZuschlag) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return 0; }

        //if (Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) { return 35; }
        //if (Format == DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) { return 15; }
        var m = 0;

        Database.RefreshColumnsData(this);

        foreach (var thisRow in Database.Row) {
            var t = thisRow.CellGetString(this);
            m = Math.Max(m, t.StringtoUtf8().Length);
        }

        if (m <= 0) { return 8; }
        if (m == 1) { return 1; }
        var erg = Math.Max((int)(m * prozentZuschlag) + 1, _maxTextLenght);
        return Math.Min(erg, 3999);
    }

    public int CalculatePreveredMaxTextLenght(double prozentZuschlag) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return 0; }

        ////if (Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) { return 35; }
        ////if (Format == DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) { return 15; }
        var m = 0;

        Database.RefreshColumnsData(this);

        var l = Contents();

        foreach (var thiss in l) {
            m = Math.Max(m, thiss.Length);
        }

        if (m <= 0) { return 8; }
        if (m == 1) { return 1; }
        return Math.Min((int)(m * prozentZuschlag) + 1, 1000);
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
        if (!string.IsNullOrEmpty(DatabaseAbstract.EditableErrorReason(Database, EditableErrorReasonType.EditAcut))) { return; }

        if (source.Database != null) { source.Repair(); }

        if (nameAndKeyToo) {
            KeyName = source.KeyName;
            //Database?.ChangeData(DatabaseDataType.ColumnKey, this, null, this._name.ToString(false), source._name.ToString(false));

            //_name = source._name;
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
        Invalidate_ContentWidth();
        FilterOptions = source.FilterOptions;
        IgnoreAtRowFilter = source.IgnoreAtRowFilter;
        DropdownBearbeitungErlaubt = source.DropdownBearbeitungErlaubt;
        DropdownAllesAbwählenErlaubt = source.DropdownAllesAbwählenErlaubt;
        TextBearbeitungErlaubt = source.TextBearbeitungErlaubt;
        SpellCheckingEnabled = source.SpellCheckingEnabled;
        DropdownWerteAndererZellenAnzeigen = source.DropdownWerteAndererZellenAnzeigen;
        AfterEditQuickSortRemoveDouble = source.AfterEditQuickSortRemoveDouble;
        RoundAfterEdit = source.RoundAfterEdit;
        MaxCellLenght = source.MaxCellLenght;
        FixedColumnWidth = source.FixedColumnWidth;
        AfterEditDoUCase = source.AfterEditDoUCase;
        AfterEditAutoCorrect = source.AfterEditAutoCorrect;
        AutoRemove = source.AutoRemove;
        //SaveContent = source.SaveContent;
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
        LinkedDatabaseTableName = source.LinkedDatabaseTableName;
        BehaviorOfImageAndText = source.BehaviorOfImageAndText;
        ConstantHeightOfImageCode = source.ConstantHeightOfImageCode;
        //BestFile_StandardSuffix = source.BestFile_StandardSuffix;
        //BestFile_StandardFolder = source.BestFile_StandardFolder;
    }

    public bool ColumNameAllowed(string nameToTest) {
        if (!IsValidColumnName(nameToTest)) { return false; }

        if (nameToTest.Equals(_name, StringComparison.OrdinalIgnoreCase)) { return true; }
        if (Database?.Column.Exists(nameToTest) != null) { return false; }

        return true;
    }

    public SizeF ColumnCaptionText_Size(Font columnFont) {
        if (IsDisposed) { return new SizeF(16, 16); }

        if (TmpCaptionTextSize.Width > 0) { return TmpCaptionTextSize; }
        //if (_columnFont == null) { return new SizeF(_pix16, _pix16); }
        TmpCaptionTextSize = columnFont.MeasureString(Caption.Replace("\r", "\r\n"));
        return TmpCaptionTextSize;
    }

    public SizeF ColumnHead_Size(Font columnFont) {
        //Bitmap? CaptionBitmapCode = null; // TODO: Caption Bitmap neu erstellen
        //if (CaptionBitmapCode != null && CaptionBitmapCode.Width > 10) {
        //    wi = Math.Max(50, ColumnCaptionText_Size(column).Width + 4);
        //    he = 50 + ColumnCaptionText_Size(column).Height + 3;
        //} else {
        var ccts = ColumnCaptionText_Size(columnFont);

        var wi = ccts.Height + 4;
        var he = ccts.Width + 3;

        if (!IsDisposed) {
            if (!string.IsNullOrEmpty(CaptionGroup3)) {
                he += ColumnCaptionSizeY * 3;
            } else if (!string.IsNullOrEmpty(CaptionGroup2)) {
                he += ColumnCaptionSizeY * 2;
            } else if (!string.IsNullOrEmpty(CaptionGroup1)) {
                he += ColumnCaptionSizeY;
            }
        }

        return new SizeF(wi, he);
    }

    public List<string> Contents() => Contents(Database?.Row.ToList());

    public List<string> Contents(FilterCollection fc, List<RowItem>? pinned) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return []; }
        var r = fc.Rows;

        var r2 = new List<RowItem>();
        r2.AddRange(r);

        if (pinned != null) { r.AddIfNotExists(pinned); }

        return Contents(r2);
    }

    //public List<string> Contents(FilterItem fi, List<RowItem>? additional) => Contents(new FilterCollection(fi.Database) { fi }, additional);
    public List<string> Contents(ICollection<RowItem>? rows) {
        if (rows == null || rows.Count == 0) { return []; }

        RefreshColumnsData();

        //ConcurrentBag<string> list = new();
        //q
        //try {
        //    _ = Parallel.ForEach(rows, thisRowItem => {
        //        if (thisRowItem != null) {
        //            if (_multiLine) {
        //                list.AddIfNotExists(thisRowItem.CellGetList(this));
        //            } else {
        //                if (thisRowItem.CellGetString(this).Length > 0) {
        //                    list.AddIfNotExists(thisRowItem.CellGetString(this));
        //                }
        //            }
        //        }
        //    });
        //} catch {
        //    Develop.CheckStackForOverflow();
        //    return Contents(rows);
        //}

        var list = new List<string>();
        foreach (var thisRowItem in rows) {
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

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    //    foreach (var thisRowItem in Database.Row) {
    //        if (thisRowItem != null) {
    //            if (thisRowItem.MatchesTo(filter) ||
    //                (pinned != null && pinned.Contains(thisRowItem))) {
    //                thisRowItem.CellSet(this, string.Empty);
    //            }
    //        }
    //    }
    //}
    public string ErrorReason() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return "Datenbank verworfen"; }
        //if (_name < 0) { return "Interner Fehler: ID nicht definiert"; }
        if (string.IsNullOrEmpty(_name)) { return "Der Spaltenname ist nicht definiert."; }

        if (!IsValidColumnName(_name)) { return "Der Spaltenname ist ungültig."; }

        //if (!IsSystemColumn()) {
        //if (_maxTextLenght == 4000) {
        //    _maxTextLenght = CalculatePreveredMaxTextLenght(1.2);
        //}

        if (_maxCellLenght < _maxTextLenght) { return "Zellengröße zu klein!"; }
        //}

        if (_maxCellLenght < 1) { return "Zellengröße zu klein!"; }
        if (_maxCellLenght > 4000) { return "Zellengröße zu groß!"; }
        if (_maxTextLenght > 4000) { return "Maximallänge zu groß!"; }

        if (Database.Column.Any(thisColumn => thisColumn != this && thisColumn != null && string.Equals(_name, thisColumn._name, StringComparison.OrdinalIgnoreCase))) {
            return "Spalten-Name bereits vorhanden.";
        }

        if (string.IsNullOrEmpty(_caption)) { return "Spalten Beschriftung fehlt."; }
        //if (!_saveContent && !IsSystemColumn()) { return "Inhalt der Spalte muss gespeichert werden."; }
        //if (!_saveContent && _showUndo) { return "Wenn der Inhalt der Spalte nicht gespeichert wird, darf auch kein Undo geloggt werden."; }
        if (((int)_format).ToString() == _format.ToString()) { return "Format fehlerhaft."; }
        if (_format.NeedTargetDatabase()) {
            if (LinkedDatabase is not DatabaseAbstract db2 || db2.IsDisposed) { return "Verknüpfte Datenbank fehlt oder existiert nicht."; }
            if (db == db2) { return "Zirkelbezug mit verknüpfter Datenbank."; }
            var c = db2.Column.Exists(_linkedCell_ColumnNameOfLinkedDatabase);
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
            if (_roundAfterEdit != -1) { return "Runden nur bei einzeiligen Texten möglich"; }
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
            if (String.Equals(thisS, Constants.Administrator, StringComparison.OrdinalIgnoreCase)) { return "'#Administrator' bei den Bearbeitern entfernen."; }
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
        if (_roundAfterEdit > 6) { return "Beim Runden maximal 6 Nachkommastellen möglich"; }
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

    public List<string> GetUcaseNamesSortedByLenght() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return []; }

        if (UcaseNamesSortedByLenght != null) { return UcaseNamesSortedByLenght; }
        var tmp = Contents(db.Row.ToList());
        tmp.Sort((s1, s2) => s2.Length.CompareTo(s1.Length));
        UcaseNamesSortedByLenght = tmp;
        return UcaseNamesSortedByLenght;
    }

    public void GetUniques(List<RowItem> rows, out List<string> einzigartig, out List<string> nichtEinzigartig) {
        einzigartig = [];
        nichtEinzigartig = [];
        foreach (var thisRow in rows) {
            if (thisRow != null && !thisRow.IsDisposed) {
                var tmp = MultiLine ? thisRow.CellGetList(this) : [thisRow.CellGetString(this)];
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
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        Invalidate_Head();
        Invalidate_ContentWidth();
        Invalidate_LinkedDatabase();
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
    public bool IsFirst() => Database?.Column.First() == this;

    public bool IsSystemColumn() =>
        _name.ToUpper() is "SYS_CORRECT" or
            "SYS_CHANGER" or
            "SYS_CREATOR" or
            "SYS_CHAPTER" or
            "SYS_DATECREATED" or
            "SYS_DATECHANGED" or
            "SYS_LOCKED" or
            "SYS_ROWSTATE";

    public void OnChanged() => Changed?.Invoke(this, new ColumnEventArgs(this));

    public string QuickInfoText(string additionalText) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return string.Empty; }
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
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return string.Empty; }

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
                ret = _name; //_Caption + " (" + _Name + ")";
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
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        if (_name == TmpNewDummy) { Develop.DebugPrint("TMPNEWDUMMY kann nicht geladen werden"); return; }

        var x = new List<ColumnItem> { this };
        Database?.RefreshColumnsData(x);
    }

    public void Repair() {
        if (!string.IsNullOrEmpty(DatabaseAbstract.EditableErrorReason(Database, EditableErrorReasonType.EditAcut))) { return; }

        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        if (IsDisposed) { return; }

        if (_format == DataFormat.Button) { ScriptType = ScriptType.Nicht_vorhanden; }

        if (_format.ToString() == ((int)_format).ToString()) {
            this.GetStyleFrom(ColumnFormatHolder.Text);
        }

        if (ScriptType == ScriptType.undefiniert) {
            if (MultiLine) {
                ScriptType = ScriptType.List;
            } else if (Format is DataFormat.Text) {
                if (SortType is SortierTyp.ZahlenwertFloat or SortierTyp.ZahlenwertInt) {
                    ScriptType = ScriptType.Numeral;
                }
                ScriptType = ScriptType.String;
            } else {
                ScriptType = ScriptType.Nicht_vorhanden;
            }
        }

        if (ScriptType == (ScriptType)6) {
            // vorher Datum
            ScriptType = ScriptType.String;
        }

        if (_format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {

            #region Aus Dateinamen den Tablename extrahieren

            if (!_linkedDatabaseTableName.Contains("|") && _linkedDatabaseTableName.IsFormat(FormatHolder.FilepathAndName)) {
                _linkedDatabaseTableName = _linkedDatabaseTableName.ToUpper().TrimEnd(".MDB").TrimEnd(".BDB").TrimEnd(".MBDB");
                LinkedDatabaseTableName = MakeValidTableName(_linkedDatabaseTableName);
            }

            #endregion

            #region Aus Connection-info den Tablename extrahieren

            if (_linkedDatabaseTableName.Contains("|")) {
                var l = _linkedDatabaseTableName.Split('|');
                if (IsValidTableName(l[0], false)) { LinkedDatabaseTableName = l[0]; }
            }

            #endregion

            var c = LinkedDatabase?.Column.Exists(_linkedCell_ColumnNameOfLinkedDatabase);
            if (c != null && !c.IsDisposed) {
                this.GetStyleFrom((IInputFormat)c);
                BehaviorOfImageAndText = c.BehaviorOfImageAndText;
                ScriptType = c.ScriptType; // 29.06.2022 Wieder aktivert. Grund: Plananalyse waren zwei vershieden Typen bei den Zeitn. So erschien immer automatisch eine 0 bei den Stnden, und es war nicht ersichtlich warum.
                DoOpticalTranslation = c.DoOpticalTranslation;
            }
        }

        if (MaxCellLenght < MaxTextLenght) { MaxCellLenght = MaxTextLenght; }

        ResetSystemToDefault(false);
        CheckIfIAmAKeyColumn();
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
        switch (_name.ToUpper()) {
            case "SYS_CREATOR":
                _format = DataFormat.Text;
                _maxTextLenght = 20;
                _maxCellLenght = 20;
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
                _scriptType = ScriptType.Nicht_vorhanden;  // um Script-Prüfung zu reduzieren
                _permissionGroupsChangeCell.Clear();
                _maxTextLenght = 20;
                _maxCellLenght = 20;
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

                this.GetStyleFrom(FormatHolder.DateTime); // Ja, FormatHolder, da wird der Script-Type nicht verändert
                MaxCellLenght = MaxTextLenght;
                if (setOpticalToo) {
                    Caption = "Erstell-Datum";
                    ForeColor = Color.FromArgb(0, 0, 128);
                    BackColor = Color.FromArgb(185, 185, 255);
                    LineLeft = ColumnLineStyle.Dick;
                }

                break;

            case "SYS_ROWSTATE":
                _spellCheckingEnabled = false;
                _ignoreAtRowFilter = true;

                this.GetStyleFrom(FormatHolder.IntegerPositive); // Ja, FormatHolder, da wird der Script-Type nicht verändert
                MaxCellLenght = MaxTextLenght;
                if (setOpticalToo) {
                    Caption = "Zeilen-Status";
                    ForeColor = Color.FromArgb(128, 0, 0);
                    BackColor = Color.FromArgb(255, 185, 185);
                    //LineLeft = ColumnLineStyle.Dick;
                }
                _scriptType = ScriptType.Nicht_vorhanden;  // um Script-Prüfung zu reduzieren

                break;

            case "SYS_DATECHANGED":
                _spellCheckingEnabled = false;
                _ignoreAtRowFilter = true;
                _showUndo = false;

                this.GetStyleFrom(FormatHolder.DateTimeWithMilliSeconds); // Ja, FormatHolder, da wird der Script-Type nicht verändert
                MaxCellLenght = MaxTextLenght;
                _textBearbeitungErlaubt = false;
                _spellCheckingEnabled = false;
                _dropdownBearbeitungErlaubt = false;
                _scriptType = ScriptType.Nicht_vorhanden; // um Script-Prüfung zu reduzieren
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
                _maxCellLenght = 1;
                _dropdownWerteAndererZellenAnzeigen = false;
                _adminInfo = "Diese Spalte kann nur über ein Skript bearbeitet<br>werden, mit dem Befehl 'SetError'";

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
                _maxCellLenght = 1;

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
                Develop.DebugPrint("Unbekannte Kennung: " + _name);
                break;
        }
    }

    public void Statisik(List<RowItem> rows, bool ignoreMultiLine) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        if (rows.Count < 1) { return; }

        var d = new Dictionary<string, int>();

        var values = new List<string>();

        foreach (var thisRow in rows) {
            if (ignoreMultiLine) {
                var keyValue = thisRow.CellGetString(this);
                if (string.IsNullOrEmpty(keyValue)) { values.Add("[empty]"); } else {
                    values.Add(keyValue.Replace("\r", ";"));
                }
            } else {
                var keyValue = thisRow.CellGetList(this);
                if (keyValue.Count == 0) { values.Add("[empty]"); } else {
                    values.AddRange(keyValue);
                }
            }
        }

        foreach (var keyValue in values) {
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
            ignoreMultiLine ? " - Zelleninhalte werden als ganzes behandelt" : " - Zelleninhalte werden gesplittet",
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

        l.WriteAllText(TempFile(string.Empty, string.Empty, "txt"), Encoding.UTF8, true);
    }

    public double? Summe(FilterCollection? fc) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return null; }

        double summ = 0;
        foreach (var thisrow in db.Row) {
            if (thisrow != null && thisrow.MatchesTo(fc.ToList())) {
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
    public double? Summe(IEnumerable<RowItem>? sort) {
        if (sort == null) { return null; }

        double summ = 0;
        foreach (var thisrow in sort) {
            if (thisrow != null && !thisrow.CellIsNullOrEmpty(this)) {
                if (!thisrow.CellGetString(this).IsDouble()) { return null; }
                summ += thisrow.CellGetDouble(this);
            }
        }
        return summ;
    }

    //        case FormatHolder.Bit:
    //            SetFormatForBit();
    //            break;
    public QuickImage? SymbolForReadableText() {
        if (IsDisposed) { return QuickImage.Get(ImageCode.Warnung); }
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return QuickImage.Get(ImageCode.Warnung); }

        if (this == db.Column.SysRowChanger) { return QuickImage.Get(ImageCode.Person); }

        if (this == db.Column.SysRowChanger) { return QuickImage.Get(ImageCode.Person); }
        if (this == db.Column.SysRowCreator) { return QuickImage.Get(ImageCode.Person); }
        if (this == db.Column.SysRowCreateDate) { return QuickImage.Get(ImageCode.Uhr); }
        if (this == db.Column.SysRowChangeDate) { return QuickImage.Get(ImageCode.Uhr); }

        if (this == db.Column.SysLocked) { return QuickImage.Get(ImageCode.Schloss); }

        if (this == db.Column.SysCorrect) { return QuickImage.Get(ImageCode.Warnung); }

        if (_format == DataFormat.RelationText) { return QuickImage.Get(ImageCode.Herz, 16); }

        if (_format == DataFormat.FarbeInteger) { return QuickImage.Get(ImageCode.Pinsel, 16); }

        if (_format == DataFormat.FarbeInteger) { return QuickImage.Get(ImageCode.Pinsel, 16); }
        if (_format == DataFormat.Button) { return QuickImage.Get(ImageCode.Kugel, 16); }
        if (_format == DataFormat.Verknüpfung_zu_anderer_Datenbank) { return QuickImage.Get(ImageCode.Fernglas, 16); }

        foreach (var thisFormat in FormatHolder.AllFormats) {
            if (thisFormat.IsFormatIdenticalSoft(this)) { return thisFormat.Image; }
        }

        if (_dropdownBearbeitungErlaubt) {
            return QuickImage.Get("Pfeil_Unten_Scrollbar|14|||||0");
        }

        if (_format.TextboxEditPossible()) {
            return _multiLine ? QuickImage.Get(ImageCode.Textfeld, 16, Color.Red, Color.Transparent) :
                           QuickImage.Get(ImageCode.Textfeld);
        }

        return QuickImage.Get(ImageCode.Warnung);
    }

    //        case FormatHolder.Url:
    //            SetFormatForUrl();
    //            break;
    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        return _name + " -> " + Caption;
    }

    public string Useage() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return string.Empty; }

        var t = "<b><u>Verwendung von " + ReadableText() + "</b></u><br>";
        if (IsSystemColumn()) {
            t += " - Systemspalte<br>";
        }

        if (db.SortDefinition?.Columns.Contains(this) ?? false) { t += " - Sortierung<br>"; }
        //var view = false;
        //foreach (var thisView in OldFormulaViews) {
        //    if (thisView[column] != null) { view = true; }
        //}
        //if (view) { t += " - Formular-Ansichten<br>"; }
        var cola = false;
        var first = true;
        foreach (var thisView in db.ColumnArrangements) {
            if (!first && thisView[this] != null) { cola = true; }
            first = false;
        }
        if (cola) { t += " - Benutzerdefinierte Spalten-Anordnungen<br>"; }
        if (UsedInScript()) { t += " - Regeln-Skript<br>"; }
        if (db.ZeilenQuickInfo.ToUpper().Contains(_name.ToUpper())) { t += " - Zeilen-Quick-Info<br>"; }
        if (_tags.JoinWithCr().ToUpper().Contains(_name.ToUpper())) { t += " - Datenbank-Tags<br>"; }

        if (!string.IsNullOrEmpty(Am_A_Key_For_Other_Column)) { t += Am_A_Key_For_Other_Column; }

        var l = Contents();
        if (l.Count > 0) {
            t += "<br><br><b>Zusatz-Info:</b><br>";
            t = t + " - Befüllt mit " + l.Count + " verschiedenen Werten";
        }
        return t;
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

                //if (LinkedDatabase is not DatabaseAbstract db) { return false; }
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

    internal static string MakeValidColumnName(string columnname) => columnname.ToUpper().Replace(" ", "_").ReduceToChars(Constants.AllowedCharsVariableName);

    internal string EditableErrorReason(EditableErrorReasonType mode, bool checkEditmode) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return "Die Datenbank wurde verworfen."; }
        if (IsDisposed) { return "Die Spalte wurde verworfen."; }

        var f = Database.EditableErrorReason(mode);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (mode == EditableErrorReasonType.OnlyRead) { return string.Empty; }

        //if (!SaveContent) { return "Der Spalteninhalt wird nicht gespeichert."; }

        if (checkEditmode) {
            if (!TextBearbeitungErlaubt && !DropdownBearbeitungErlaubt) {
                return "Die Inhalte dieser Spalte können nicht manuell bearbeitet werden, da keine Bearbeitungsmethode erlaubt ist.";
            }

            if (UserEditDialogTypeInTable(Format, false, true, MultiLine && DropdownBearbeitungErlaubt) == EditTypeTable.None) {
                return "Interner Programm-Fehler: Es ist keine Bearbeitungsmethode für den Typ des Spalteninhalts '" + Format + "' definiert.";
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Wenn sich ein Zelleninhalt verändert hat, muss die Spalte neu berechnet werden.
    /// </summary>
    internal void Invalidate_ContentWidth() => Contentwidth = null;

    internal void Optimize() {
        if (!IsSystemColumn()) {
            // Maximale Text-Länge beeinflusst stark die Ladezeit vom Server
            var l = CalculatePreveredMaxCellLenght(1.2f);
            if (l < MaxCellLenght) { MaxCellLenght = l; }

            if (MaxTextLenght > MaxCellLenght) { MaxTextLenght = MaxCellLenght; }

            // ScriptType beeinflusst, ob eine Zeile neu durchgerechnet werden muss nach einer Änderung dieser Zelle
            if (!UsedInScript()) {
                ScriptType = ScriptType.Nicht_vorhanden;
            }

            //// Beeinflussst stark den Áufbau bei großen Zeilen.
            //// Aber erst ab 20, da es ansonsten wenige verschiedene Einträge sind, es und es sich nicht reniert.
            //// Zudem können zu kleine Werte ein Berechnungsfehler sein
            //Invalidate_ContentWidth();
            //if (_contentwidth is int v && v > 20 && FixedColumnWidth == 0) {
            //    FixedColumnWidth = v;
            //}
        }
    }

    /// <summary>
    /// Setzt den Wert in die dazugehörige Variable.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="newvalue"></param>
    /// <returns></returns>
    internal string SetValueInternal(DatabaseDataType type, string newvalue) {
        if (type.IsObsolete()) { return string.Empty; }

        switch (type) {
            case DatabaseDataType.ColumnName:
                var nname = newvalue.ToUpper();

                //if (reason == Reason.LoadReload && nname != _name) {
                //    return "Namen Inkonsistent: " + nname + " " + _name;
                //}

                var ok = Database?.Column.ChangeName(_name, nname) ?? false;

                if (!ok) {
                    Database?.Freeze("Schwerer Spalten Umbenennungsfehler!");
                    return "Schwerer Spalten Umbenennungsfehler!";
                }

                _name = nname;
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
                //_maxCellLenght = _maxTextLenght;
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
                _roundAfterEdit = IntParse(newvalue);
                break;

            case DatabaseDataType.FixedColumnWidth:
                _fixedColumnWidth = IntParse(newvalue);
                break;

            case DatabaseDataType.MaxCellLenght:
                _maxCellLenght = IntParse(newvalue);
                break;

            case DatabaseDataType.DoUcaseAfterEdit:
                _afterEditDoUCase = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.AutoCorrectAfterEdit:
                _afterEditAutoCorrect = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.AutoRemoveCharAfterEdit:
                _autoRemove = newvalue;
                break;

            case DatabaseDataType.ColumnAdminInfo:
                _adminInfo = newvalue;
                break;

            //case DatabaseDataType.ColumnContentWidth:
            //    _contentwidth = IntParse(newvalue);
            //    ContentWidthIsValid = true;
            //    break;

            case DatabaseDataType.CaptionBitmapCode:
                _captionBitmapCode = newvalue;
                break;

            case DatabaseDataType.Suffix:
                _suffix = newvalue;
                break;

            case DatabaseDataType.LinkedDatabase:
                _linkedDatabaseTableName = newvalue;
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

            case DatabaseDataType.ColumnNameOfLinkedDatabase:

                if (newvalue.IsFormat(FormatHolder.Integer)) {
                    _linkedCell_ColumnNameOfLinkedDatabase = string.Empty;
                } else {
                    _linkedCell_ColumnNameOfLinkedDatabase = newvalue;
                }

                break;

            case DatabaseDataType.SortType:
                if (string.IsNullOrEmpty(newvalue)) {
                    _sortType = SortierTyp.Original_String;
                } else {
                    _sortType = (SortierTyp)LongParse(newvalue);
                }
                break;

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

    private static EditTypeTable UserEditDialogTypeInTable(DataFormat format, bool doDropDown, bool keybordInputAllowed, bool isMultiline) {
        if (!doDropDown && !keybordInputAllowed) { return EditTypeTable.None; }

        switch (format) {
            case DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems:
                return EditTypeTable.Dropdown_Single;

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
                    if (!doDropDown) {
                        return EditTypeTable.Textfeld;
                    }

                    if (isMultiline) {
                        return EditTypeTable.Dropdown_Single;
                    }

                    if (keybordInputAllowed) {
                        return EditTypeTable.Textfeld_mit_Auswahlknopf;
                    }

                    return EditTypeTable.Dropdown_Single;
                }

                Develop.DebugPrint(format);
                return EditTypeTable.None;
        }
    }

    private void _TMP_LinkedDatabase_Cell_CellValueChanged(object sender, CellChangedEventArgs e) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        if (e.Column.KeyName != LinkedCell_ColumnNameOfLinkedDatabase) { return; }

        foreach (var thisRow in Database.Row) {
            if (Database.Cell.GetStringCore(this, thisRow) == e.Row.KeyName) {
                //CellCollection.Invalidate_CellContentSize(this, thisRow);
                Invalidate_ContentWidth();
                Database.Cell.OnCellValueChanged(new CellChangedEventArgs(this, thisRow, e.Reason));
                //_ = thisRow.ExecuteScript(EventTypes.value_changedx, string.Empty, true, false, true, 5);
                thisRow.Database?.Row.AddRowWithChangedValue(thisRow);
            }
        }
    }

    private void _TMP_LinkedDatabase_Disposing(object sender, System.EventArgs e) {
        Invalidate_LinkedDatabase();
        Database?.Dispose();
    }

    private void CheckIfIAmAKeyColumn() {
        Am_A_Key_For_Other_Column = string.Empty;

        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        foreach (var c in db.Column) {
            //if (thisColumn.KeyColumnKey == _name) { Am_A_Key_For_Other_Column = "Spalte " + thisColumn.ReadableText() + " verweist auf diese Spalte"; } // Werte Gleichhalten
            //if (thisColumn.LinkedCell_RowKeyIsInColumn == _name) { Am_A_Key_For_Other_Column = "Spalte " + thisColumn.ReadableText() + " verweist auf diese Spalte"; } // LinkdeCells pflegen
            //if (ThisColumn.LinkedCell_ColumnValueFoundIn == _name) { I_Am_A_Key_For_Other_Column = "Spalte " + ThisColumn.ReadableText() + " verweist auf diese Spalte"; } // LinkdeCells pflegen
            if (c.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
                foreach (var thisitem in c.LinkedCellFilter) {
                    var tmp = thisitem.SplitBy("|");

                    if (tmp[2].ToLower().Contains("~" + _name.ToLower() + "~")) {
                        Am_A_Key_For_Other_Column = "Spalte " + c.ReadableText() + " verweist auf diese Spalte";
                    }
                }
            }
        }
        //if (_format == DataFormat.Columns_für_LinkedCellDropdown) { Am_A_Key_For_Other_Column = "Die Spalte selbst durch das Format"; }
    }

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            if (Database != null && !Database.IsDisposed) { Database.DisposingEvent -= Database_Disposing; }
            Invalidate_LinkedDatabase();
            Database = null;

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    private void GetLinkedDatabase() {
        Invalidate_LinkedDatabase(); // Um evtl. Events zu löschen

        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        foreach (var thisFile in AllFiles) {
            if (thisFile.TableName.Equals(_linkedDatabaseTableName, StringComparison.OrdinalIgnoreCase)) {
                _linkedDatabase = thisFile;
                break;
            }
        }

        if (_linkedDatabase == null && IsValidTableName(_linkedDatabaseTableName, false)) {
            var sr = string.Empty;
            if (!string.IsNullOrEmpty(Database.FreezedReason)) { sr = "Vorgänger eingefroren"; }
            _linkedDatabase = Database.GetOtherTable(_linkedDatabaseTableName, false, sr);
        }

        if (_linkedDatabase == null) {
            var ci = new ConnectionInfo(_linkedDatabaseTableName, null, Database.FreezedReason);
            _linkedDatabase = GetById(ci, false, null, true);
        }

        if (_linkedDatabase != null) {
            _linkedDatabase.Cell.CellValueChanged += _TMP_LinkedDatabase_Cell_CellValueChanged;
            _linkedDatabase.DisposingEvent += _TMP_LinkedDatabase_Disposing;
        }
    }

    private void Invalidate_Head() {
        TmpCaptionTextSize = new SizeF(-1, -1);
        TmpCaptionBitmapCode = null;
    }

    private void Invalidate_LinkedDatabase() {
        if (_linkedDatabase != null) {
            _linkedDatabase.Cell.CellValueChanged -= _TMP_LinkedDatabase_Cell_CellValueChanged;
            _linkedDatabase.DisposingEvent -= _TMP_LinkedDatabase_Disposing;
            _linkedDatabase = null;
        }
    }

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

    /// <summary>
    /// CallByFileName Aufrufe werden nicht geprüft
    /// </summary>
    /// <returns></returns>
    private bool UsedInScript() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return false; }

        foreach (var thiss in Database.EventScript) {
            if (thiss.ScriptText.ContainsWord(_name, RegexOptions.IgnoreCase)) { return true; }
        }

        return false;
    }

    #endregion
}