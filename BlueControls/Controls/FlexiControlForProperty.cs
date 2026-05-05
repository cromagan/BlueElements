// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Constants;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueControls.Controls;

// https://stackoverflow.com/questions/724143/how-do-i-create-a-delegate-for-a-net-property
// http://peisker.net/dotnet/propertydelegates.htm
// http://geekswithblogs.net/akraus1/archive/2006/02/10/69047.aspx
public class FlexiControlForProperty<T> : FlexiControl {

    #region Fields

    private readonly Accessor<T>? _accessor;
    private bool _isUpdating;

    #endregion

    #region Constructors

    /// <summary>
    /// Anzeige als Dropdown-Feld
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="list"></param>
    public FlexiControlForProperty(Expression<Func<T>> expr, List<AbstractListItem>? list) : this(expr, list, false) { }

    /// <summary>
    /// Anzeige als Dropdown-Feld
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="list"></param>
    /// <param name="texteditAllowed"></param>
    public FlexiControlForProperty(Expression<Func<T>> expr, List<AbstractListItem>? list, bool texteditAllowed) : this(expr, string.Empty, 1, list, CheckBehavior.MultiSelection, AddType.None, true) { }

    /// <summary>
    /// Je nach Datentyp eine andere Anzeige, mit der angegeben Anzahl an Zeilen.
    /// Listen werden zu Listbox, String zu Textfeldern
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="rowCount"></param>
    ///
    public FlexiControlForProperty(Expression<Func<T>> expr, int rowCount) : this(expr, string.Empty, rowCount, null, CheckBehavior.MultiSelection, AddType.None, true) { }

    /// <summary>
    /// Je nach Datentyp eine andere Anzeige
    /// </summary>
    /// <param name="expr"></param>
    ///
    public FlexiControlForProperty(Expression<Func<T>> expr) : this(expr, string.Empty, 1, null, CheckBehavior.MultiSelection, AddType.None, true) { }

    /// <summary>
    /// Je nach Datentyp eine andere Anzeige
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="captionText"></param>
    public FlexiControlForProperty(Expression<Func<T>> expr, string captionText) : this(expr, captionText, 1, null, CheckBehavior.MultiSelection, AddType.None, true) { }

    public FlexiControlForProperty() : this(null, string.Empty, 1, null, CheckBehavior.MultiSelection, AddType.None, true) { }

    /// <summary>
    /// Je nach Datentyp eine andere Anzeige
    /// </summary>
    public FlexiControlForProperty(Expression<Func<T>>? expr, string captionText, int rowCount, List<AbstractListItem>? allPossibleItems, CheckBehavior checkBehavior, AddType addallowed, bool autoSort) : base() {
        _accessor = new(expr);

        GenFehlerText();

        CaptionPosition = CaptionPosition.Links_neben_dem_Feld;
        EditType = EditTypeFormula.Textfeld;
        Size = new Size(200, 24);

        CheckBehavior = checkBehavior;
        AddAllowed = addallowed;
        AutoSort = autoSort;
        ListItems = allPossibleItems;

        #region Caption setzen

        if (string.IsNullOrEmpty(captionText)) {
            var x = _accessor.Name.SplitAndCutBy("__");
            Caption = x[0].Replace('_', ' ') + ":";
        } else {
            Caption = captionText.TrimEnd(':') + ":";
        }

        #endregion

        UserEditDialogType = default;

        #region Art des Steuerelements bestimmen

        switch (_accessor) {
            case Accessor<bool>: {
                    EditType = EditTypeFormula.Ja_Nein_Knopf;
                    var s1 = BlueControls.Controls.Caption.RequiredTextSize(Caption, Design.Caption, Translate, -1);
                    Size = new Size(s1.Width + 30, 22);
                    break;
                }

            case Accessor<List<string>>:
            case Accessor<ReadOnlyCollection<string>>: {
                    CaptionPosition = CaptionPosition.Über_dem_Feld;
                    EditType = EditTypeFormula.Listbox;
                    Size = new Size(200, 16 + (24 * rowCount));
                    DropdownAllowed = true;
                    ShowValuesOfOtherCellsInDropdown = false;
                    RaiseChangeDelay = 1;
                    TextInputAllowed = false;
                    CreateSubControls();
                    break;
                }

            default: // Alle enums sind ein eigener Typ.... deswegen alles in die Textbox
            {
                    if (allPossibleItems != null) {
                        EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;
                        var s2 = BlueControls.Controls.Caption.RequiredTextSize(Caption, Design.Caption, Translate, -1);

                        var (biggestItemX, biggestItemY, _, _) = allPossibleItems.CanvasItemData(Design.ComboBox_TextBox);
                        var x2 = Math.Max(biggestItemX + 20 + s2.Width, 200);
                        var y2 = Math.Max(biggestItemY + (Skin.PaddingSmal * 2), 24);
                        Size = new Size(x2, y2);

                        RaiseChangeDelay = 1;
                        DropdownAllowed = false;
                        TextInputAllowed = false;
                        ShowValuesOfOtherCellsInDropdown = false;
                        CreateSubControls();
                    } else if (_accessor.Get() is IEditable) {
                        EditType = EditTypeFormula.Button;
                        ImageCode = "Stift|16";
                        var s1 = BlueControls.Controls.Caption.RequiredTextSize(Caption, Design.Caption, Translate, -1);
                        Size = new Size(s1.Width + 30, 22);
                        Caption = "bearbeiten";
                    } else if (_accessor.Get() is null) {
                        CaptionPosition = CaptionPosition.Links_neben_dem_Feld;
                        EditType = EditTypeFormula.nur_als_Text_anzeigen;
                    } else {
                        EditType = EditTypeFormula.Textfeld;
                        if (rowCount >= 2) {
                            CaptionPosition = CaptionPosition.Über_dem_Feld;
                            Size = new Size(200, 16 + (24 * rowCount));
                            this.GetStyleFrom(FormatHolder_Text.Instance);
                            MultiLine = true;
                        } else {
                            CaptionPosition = CaptionPosition.Links_neben_dem_Feld;
                            Size = new Size(200, 24);
                            MultiLine = false;
                            switch (_accessor) {
                                case Accessor<string>:
                                    this.GetStyleFrom(FormatHolder_Text.Instance);
                                    break;

                                case Accessor<long>:
                                case Accessor<int>:
                                    this.GetStyleFrom(FormatHolder_Long.Instance);
                                    break;

                                case Accessor<float>:
                                    this.GetStyleFrom(FormatHolder_Float.Instance);
                                    break;

                                case Accessor<double>:
                                    this.GetStyleFrom(FormatHolder_Float.Instance);
                                    break;

                                case Accessor<Color>:
                                    this.GetStyleFrom(FormatHolder_Text.Instance);
                                    break;

                                default:
                                    this.GetStyleFrom(FormatHolder_Text.Instance);
                                    break;
                            }
                        }
                        RaiseChangeDelay = 1;
                        DropdownAllowed = false;
                        TextInputAllowed = false;
                        ShowValuesOfOtherCellsInDropdown = false;
                        CreateSubControls();
                    }
                    break;
                }
        }

        #endregion

        QuickInfo = _accessor.QuickInfo;
        _accessor.ValueChanged += _accessor_ValueChanged;

        SetValueFromProperty();
        GenFehlerText();

        CheckEnabledState();
    }

    #endregion

    #region Methods

    public void RefreshFromProperty() {
        if (!Allinitialized || IsDisposed) { return; }
        SetValueFromProperty();
    }

    protected override void Dispose(bool disposing) {
        if (disposing && _accessor != null) {
            _accessor.ValueChanged -= _accessor_ValueChanged;
            _accessor.Dispose();
        }
        base.Dispose(disposing);
    }

    protected override void OnControlAdded(ControlEventArgs e) {
        CheckEnabledState();
        base.OnControlAdded(e);
    }

    protected override void OnExecuteComand() {
        base.OnExecuteComand();
        if (_accessor != null) {
            object? x = _accessor.Get();

            if (x is IEditable iei) {
                iei.Edit();
            }
        }
    }

    protected override void OnHandleDestroyed(System.EventArgs e) {
        FillPropertyNow();
        base.OnHandleDestroyed(e);
    }

    protected override void OnValueChanged() {
        if (IsDisposed) { return; }
        FillPropertyNow();
        if (IsDisposed) { return; }
        GenFehlerText();
        base.OnValueChanged();
    }

    private void _accessor_ValueChanged(object? sender, System.EventArgs e) {
        if (IsHandleCreated) {
            BeginInvoke(new Action(SetValueFromProperty));
        } else {
            SetValueFromProperty();
        }
    }

    private bool CheckEnabledState() {
        if (DesignMode) {
            DisabledReason = string.Empty;
            return true;
        }

        if (_accessor == null) {
            DisabledReason = "Kein zugehöriges Objekt definiert.";
            return false;
        }

        if (!_accessor.CanWrite) {
            DisabledReason = "Feld kann generell nicht beschrieben werden.";
            return false;
        }
        //if (_alwaysDiabled) {
        //    DisabledReason = "Feld ist schreibgeschützt.";
        //    return false;
        //}
        DisabledReason = string.Empty;
        return true;
    }

    /// <summary>
    /// Schreibt den Wert von Value in das Property Objekt zurück
    /// </summary>
    private void FillPropertyNow() {
        if (IsDisposed) { return; }
        if (!Allinitialized) { return; }
        if (_isUpdating) { return; }
        if (!CheckEnabledState()) { return; }

        if (_accessor is not { CanRead: true } || !_accessor.CanWrite) { return; }

        _isUpdating = true;
        try {
            switch (_accessor) {
                case Accessor<string> al:
                    if (al.Get() != Value) { al.Set(Value); }
                    break;

                case Accessor<ReadOnlyCollection<string>> roc:
                    var listnewro = Value.SplitAndCutByCr().ToList().AsReadOnly();
                    if (listnewro.IsDifferentTo(roc.Get())) { roc.Set(listnewro); }
                    break;

                case Accessor<List<string>> ls:
                    var listnew = Value.SplitAndCutByCr().ToList();
                    if (listnew.IsDifferentTo(ls.Get())) { ls.Set(listnew); }
                    break;

                case Accessor<bool> ab:
                    var nb = Value.FromPlusMinus();
                    if (ab.Get() != nb) { ab.Set(nb); }
                    break;

                case Accessor<Color> ac:
                    if (ac.Get().ToHtmlCode() != Value) { ac.Set(ColorParse(Value)); }
                    break;

                case Accessor<int> ai:
                    _ = int.TryParse(Value, out var i);
                    if (ai.Get() != i) { ai.Set(i); }
                    break;

                case Accessor<double> ad:
                    _ = DoubleTryParse(Value, out var d);
                    if (Math.Abs(ad.Get() - d) > DefaultTolerance) { ad.Set(d); }
                    break;

                case Accessor<float> af:
                    _ = FloatTryParse(Value, out var f);
                    if (Math.Abs(af.Get() - f) > DefaultTolerance) { af.Set(f); }
                    break;

                case Accessor<Table?> adb:
                    var tb = Table.Get(Value, TableView.Table_NeedPassword);

                    if (adb.Get() != tb) {
                        adb.Set(tb);
                    }
                    break;

                default:

                    if (_accessor.Get() is null) { } else if (_accessor.Get() is IEditable) { } else if (_accessor.Get() is Enum) {
                        var ef = IntParse(Value);
                        var nval = (T)Enum.ToObject(typeof(T), ef); // https://stackoverflow.com/questions/29482/how-can-i-cast-int-to-enum
                        if (nval.ToString() != _accessor.Get()?.ToString()) { _accessor.Set(nval); }
                    } else {
                        Develop.DebugError("Art unbekannt!");
                    }
                    break;
            }
        } finally {
            _isUpdating = false;
        }
    }

    private void GenFehlerText() => InfoText = string.Empty;

    private void SetValueFromProperty() {
        if (IsDisposed) { return; }
        if (_isUpdating) { return; }
        if (_accessor is not { CanRead: true }) {
            Value = string.Empty;
            InfoText = string.Empty;
            return;
        }
        _isUpdating = true;
        try {
            object? x = _accessor.Get();

            switch (x) {
                case null:
                    Value = string.Empty;
                    break;

                case string s:
                    Value = s;
                    break;

                case ReadOnlyCollection<string> roc:
                    Value = string.Join('\r', roc);
                    break;

                case List<string> ls:
                    Value = string.Join('\r', ls);
                    break;

                case bool bo:
                    Value = bo.ToPlusMinus();
                    break;

                case int iv:
                    Value = iv.ToString1();
                    break;

                case Enum:
                    Value = ((int)x).ToString1();
                    break;

                case double db:
                    Value = db.ToString1_2();
                    break;

                case float fl:
                    Value = fl.ToString1_2();
                    break;

                case Color co:
                    Value = co.ToHtmlCode();
                    break;

                case Table tb:
                    Value = tb.KeyName;
                    break;

                case IEditable:
                    break;

                default:
                    Develop.DebugError("Art unbekannt!");
                    break;
            }
        } finally {
            _isUpdating = false;
        }
    }

    #endregion
}