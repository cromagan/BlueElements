// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueControls.BlueTableDialogs;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.ItemCollectionList;
using BlueTable;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq.Expressions;
using System.Windows.Forms;
using static BlueBasics.Constants;
using static BlueBasics.Converter;

namespace BlueControls.Controls;

// https://stackoverflow.com/questions/724143/how-do-i-create-a-delegate-for-a-net-property
// http://peisker.net/dotnet/propertydelegates.htm
// http://geekswithblogs.net/akraus1/archive/2006/02/10/69047.aspx
public class FlexiControlForProperty<T> : FlexiControl {

    #region Fields

    private readonly Accessor<T>? _accessor;

    private readonly Timer? _checker;

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
    public FlexiControlForProperty(Expression<Func<T>> expr, List<AbstractListItem>? list, bool texteditAllowed) : this(expr, string.Empty, 1, list, CheckBehavior.MultiSelection, AddType.None, texteditAllowed ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList) { }

    /// <summary>
    /// Je nach Datentyp eine andere Anzeige, mit der angegeben Anzahl an Zeilen.
    /// Listen werden zu Listbox, String zu Textfeldern
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="rowCount"></param>
    ///
    public FlexiControlForProperty(Expression<Func<T>> expr, int rowCount) : this(expr, string.Empty, rowCount, null, CheckBehavior.MultiSelection, AddType.None, ComboBoxStyle.DropDownList) { }

    /// <summary>
    /// Je nach Datentyp eine andere Anzeige
    /// </summary>
    /// <param name="expr"></param>
    ///
    public FlexiControlForProperty(Expression<Func<T>> expr) : this(expr, string.Empty, 1, null, CheckBehavior.MultiSelection, AddType.None, ComboBoxStyle.DropDownList) { }

    /// <summary>
    /// Je nach Datentyp eine andere Anzeige
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="captionText"></param>
    public FlexiControlForProperty(Expression<Func<T>> expr, string captionText) : this(expr, captionText, 1, null, CheckBehavior.MultiSelection, AddType.None, ComboBoxStyle.DropDownList) { }

    public FlexiControlForProperty() : this(null, string.Empty, 1, null, CheckBehavior.MultiSelection, AddType.None, ComboBoxStyle.DropDownList) { }

    /// <summary>
    /// Je nach Datentyp eine andere Anzeige
    /// </summary>
    public FlexiControlForProperty(Expression<Func<T>>? expr, string captionText, int rowCount, List<AbstractListItem>? list, CheckBehavior checkBehavior, AddType addallowed, ComboBoxStyle comboBoxStyle) : base() {
        _accessor = new(expr);

        GenFehlerText();

        CaptionPosition = CaptionPosition.Links_neben_dem_Feld;
        EditType = EditTypeFormula.Textfeld;
        Size = new Size(200, 24);

        #region Caption setzen

        if (string.IsNullOrEmpty(captionText)) {
            var x = _accessor.Name.SplitAndCutBy("__");
            Caption = x[0].Replace("_", " ") + ":";
        } else {
            Caption = captionText.TrimEnd(":") + ":";
        }

        #endregion Caption setzen

        #region Art des Steuerelements bestimmen

        switch (_accessor) {
            case Accessor<bool>: {
                    EditType = EditTypeFormula.Ja_Nein_Knopf;
                    var s1 = BlueControls.Controls.Caption.RequiredTextSize(Caption, SteuerelementVerhalten.Text_Abschneiden, Design.Caption, null, Translate, -1);

                    Size = new Size(s1.Width + 30, 22);
                    break;
                }

            case Accessor<List<string>>:
            case Accessor<ReadOnlyCollection<string>>: {
                    CaptionPosition = CaptionPosition.Über_dem_Feld;
                    EditType = EditTypeFormula.Listbox;
                    Size = new Size(200, 16 + (24 * rowCount));

                    StyleListBox(GetControl<ListBox>(), list, checkBehavior, addallowed);

                    break;
                }

            default: // Alle enums sind ein eigener Typ.... deswegen alles in die Textbox
            {
                    if (list != null) {
                        EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;
                        var s2 = BlueControls.Controls.Caption.RequiredTextSize(Caption, SteuerelementVerhalten.Text_Abschneiden, Design.Caption, null, Translate, -1);

                        var (biggestItemX, biggestItemY, _, _) = ListBox.ItemData(list, Design.ComboBox_Textbox);
                        var x2 = Math.Max(biggestItemX + 20 + s2.Width, 200);
                        var y2 = Math.Max(biggestItemY + (Skin.PaddingSmal * 2), 24);
                        Size = new Size(x2, y2);
                        StyleComboBox(GetControl<ComboBox>(), list, comboBoxStyle, true, 1);
                    } else if (_accessor.Get() is IEditable) {
                        EditType = EditTypeFormula.Button;
                        var s1 = BlueControls.Controls.Caption.RequiredTextSize(Caption, SteuerelementVerhalten.Text_Abschneiden, Design.Caption, null, Translate, -1);
                        Size = new Size(s1.Width + 30, 22);

                        if (GetControl<Button>() is { IsDisposed: false } b) {
                            b.ImageCode = "Stift|16";
                            b.Text = "bearbeiten";
                        }
                    } else if (_accessor.Get() is null) {
                        CaptionPosition = CaptionPosition.Links_neben_dem_Feld;
                        EditType = EditTypeFormula.nur_als_Text_anzeigen;
                    } else {
                        EditType = EditTypeFormula.Textfeld;
                        if (rowCount >= 2) {
                            CaptionPosition = CaptionPosition.Über_dem_Feld;
                            Size = new Size(200, 16 + (24 * rowCount));
                            this.GetStyleFrom(FormatHolder.Text);
                            MultiLine = true;
                        } else {
                            CaptionPosition = CaptionPosition.Links_neben_dem_Feld;
                            Size = new Size(200, 24);
                            MultiLine = false;
                            switch (_accessor) {
                                case Accessor<string>:
                                    this.GetStyleFrom(FormatHolder.Text);
                                    break;

                                case Accessor<long>:
                                case Accessor<int>:
                                    this.GetStyleFrom(FormatHolder.Long);
                                    break;

                                case Accessor<float>:
                                    this.GetStyleFrom(FormatHolder.Float);
                                    break;

                                case Accessor<double>:
                                    this.GetStyleFrom(FormatHolder.Float);
                                    break;

                                case Accessor<Color>:
                                    this.GetStyleFrom(FormatHolder.Text);
                                    break;

                                default:
                                    this.GetStyleFrom(FormatHolder.Text);
                                    break;
                            }
                        }

                        StyleTextBox(GetControl<TextBox>(), 1);
                    }
                    break;
                }
        }

        #endregion Art des Steuerelements bestimmen

        QuickInfo = _accessor.QuickInfo;

        SetValueFromProperty();
        GenFehlerText();

        _ = CheckEnabledState();

        _checker = new Timer {
            Enabled = true,
            Interval = 1000
        };
        _checker.Tick += Checker_Tick;
    }

    #endregion

    #region Destructors

    ~FlexiControlForProperty() {
        Dispose(false);
    }

    #endregion

    #region Methods

    protected override void Dispose(bool disposing) {
        if (disposing && _checker != null) {
            _checker.Enabled = false;
            _checker.Tick -= Checker_Tick;
            _checker.Dispose();
        }
        base.Dispose(disposing);
    }

    protected override void OnButtonClicked() {
        base.OnButtonClicked();
        if (_accessor != null) {
            object? x = _accessor.Get();

            if (x is IEditable iei) {
                iei.Edit();
            }
        }
    }

    protected override void OnControlAdded(ControlEventArgs e) {
        _ = CheckEnabledState();
        base.OnControlAdded(e);
    }

    protected override void OnHandleDestroyed(System.EventArgs e) {
        FillPropertyNow();
        base.OnHandleDestroyed(e);
    }

    protected override void OnValueChanged() {
        FillPropertyNow(); // erst befüllen, bevor das Event ausgelöst wird
        GenFehlerText(); // erst Standard fehler Text, bevor das Event ausgelöst wird
        base.OnValueChanged();
    }

    protected void StyleListBox(ListBox? control, List<AbstractListItem>? list, CheckBehavior checkBehavior, AddType addallowed) {
        if (control == null) { return; }

        //control.Enabled = Enabled;

        //EditType = EditTypeFormula.Listbox;
        //var s1 = BlueControls.Controls.Caption.RequiredTextSize(Caption, SteuerelementVerhalten.Text_Abschneiden, Design.Caption, null, Translate, -1);

        control.CheckBehavior = checkBehavior;
        control.Appearance = ListBoxAppearance.Listbox_Boxes;
        control.ItemAddRange(list);
        control.AddAllowed = addallowed;
        control.RemoveAllowed = false;
        control.ItemEditAllowed = string.Equals(Generic.UserGroup, Administrator, StringComparison.OrdinalIgnoreCase);

        ValueSet(string.Empty, true);
        //control.Check(Value.SplitByCr());

        //control.Item.Clear();
        //control.CheckBehavior = CheckBehavior.MultiSelection;
        //if (column == null || column.IsDisposed) { return; }

        //var item =  new List<AbstractListItem>();;
        //if (column.DropdownBearbeitungErlaubt) {
        //    item.AddRange(GetItemCollection(column, null, ShortenStyle.Replaced, 10000));
        //    if (!column.DropdownWerteAndererZellenAnzeigen) {
        //        bool again;
        //        do {
        //            again = false;
        //            foreach (var thisItem in item) {
        //                if (!column.DropDownItems.Contains(thisItem.KeyName)) {
        //                    again = true;
        //                    item.Remove(thisItem);
        //                    break;
        //                }
        //            }
        //        } while (again);
        //    }
        //}

        //switch (ColumnItem.UserEditDialogTypeInTable(column, false)) {
        //    case EditTypeTable.Textfeld:
        //        control.AddAllowed = AddType.Text;
        //        break;

        //    case EditTypeTable.Listbox:
        //        control.AddAllowed = AddType.OnlySuggests;
        //        break;

        //    default:
        //        control.AddAllowed = AddType.None;
        //        break;
        //}

        //control.FilterAllowed = false;
        //control.MoveAllowed = false;
        //switch (EditType) {
        //    //case EditTypeFormula.Gallery:
        //    //    control.Appearance = BlueListBoxAppearance.Gallery;
        //    //    control.RemoveAllowed = true;
        //    //    break;

        //    case EditTypeFormula.Listbox:
        //        control.RemoveAllowed = true;
        //        control.Appearance = ListBoxAppearance.Listbox;
        //        break;
        //}
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

    private void Checker_Tick(object sender, System.EventArgs e) {
        if (Parent is not { Visible: true } || !Visible || IsDisposed || Parent.IsDisposed) { return; }
        //if (_IsFilling) { return; }
        if (!Allinitialized) { return; }
        SetValueFromProperty();
    }

    /// <summary>
    /// Schreibt den Wert von Value in das Property Objekt zurück
    /// </summary>
    private void FillPropertyNow() {
        if (!Allinitialized) { return; }
        if (!CheckEnabledState()) { return; } // Versuch. Eigentlich darf das Steuerelement dann nur empfangen und nix ändern.

        if (_accessor is not { CanRead: true } || !_accessor.CanWrite) { return; }

        switch (_accessor) {
            case Accessor<string> al:
                if (al.Get() != Value) { al.Set(Value); }
                break;

            case Accessor<ReadOnlyCollection<string>> roc:
                var listnewro = Value.SplitAndCutByCrToList().AsReadOnly();
                if (listnewro.IsDifferentTo(roc.Get())) { roc.Set(listnewro); }
                break;

            case Accessor<List<string>> ls:
                var listnew = Value.SplitAndCutByCrToList();
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
                var db = Table.Get(Value, TableView.Table_NeedPassword, true);
                if (db != null) { db.Editor = typeof(TableHeadEditor); }

                if (adb.Get() != db) {
                    adb.Set(db);
                }
                break;

            //case Accessor<IEditable> _:
            //    //var db = Table.GetById(new ConnectionInfo(Value, null, string.Empty), false, null, true);
            //    //if (adb.Get() != db) { adb.Set(db); }
            //    break;

            //case Accessor <enum> ae:
            //    FloatTryParse(Value, out var f);
            //    if (af.Get() != f) { af.Set(f); }
            //    break;

            default:

                if (_accessor.Get() is null) {
                } else if (_accessor.Get() is IEditable) {
                } else if (_accessor.Get() is Enum) {
                    _ = int.TryParse(Value, out var ef);
                    var nval = (T)Enum.ToObject(typeof(T), ef); // https://stackoverflow.com/questions/29482/how-can-i-cast-int-to-enum
                    if (nval.ToString() != _accessor.Get()?.ToString()) { _accessor.Set(nval); }
                } else {
                    Develop.DebugPrint(ErrorType.Error, "Art unbekannt!");
                }
                break;
        }
    }

    private void GenFehlerText() =>
        //if ( _accessor != null) {
        //    InfoText = string.Empty;
        //    return;
        //}
        ////if (_FehlerWennLeer && string.IsNullOrEmpty(Value)) {
        ////    InfoText = "Dieses Feld darf nicht leer sein.";
        ////    return;
        ////}
        //if (string.IsNullOrEmpty(Value)) {
        //    InfoText = string.Empty;
        //    return;
        //}
        ////if (_FehlerFormatCheck && !Value.IsFormat(Format)) {
        ////    InfoText = "Der Wert entspricht nicht dem erwarteten Format.";
        ////    return;
        ////}
        InfoText = string.Empty;

    private void SetValueFromProperty() {
        if (_accessor is not { CanRead: true }) {
            ValueSet(string.Empty, true);
            InfoText = string.Empty;
            return;
        }
        object? x = _accessor.Get();

        switch (x) {
            case null:
                ValueSet(string.Empty, true);
                break;

            case string s:
                ValueSet(s, true);
                break;

            case ReadOnlyCollection<string> roc:
                ValueSet(roc.JoinWithCr(), true);
                break;

            case List<string> ls:
                ValueSet(ls.JoinWithCr(), true);
                break;

            case bool bo:
                ValueSet(bo.ToPlusMinus(), true);
                break;

            case int iv:
                ValueSet(iv.ToString(), true);
                break;

            case Enum:
                ValueSet(((int)x).ToString(), true);
                break;

            case double db:
                ValueSet(db.ToStringFloat2(), true);
                break;

            case float fl:
                ValueSet(fl.ToStringFloat2(), true);
                break;

            case Color co:
                ValueSet(co.ToHtmlCode(), true);
                break;

            case Table db:
                ValueSet(db.KeyName, true);
                break;

            case IEditable:
                //var db = Table.GetById(new ConnectionInfo(Value, null, string.Empty), false, null, true);
                //if (adb.Get() != db) { adb.Set(db); }
                break;

            default:
                Develop.DebugPrint(ErrorType.Error, "Art unbekannt!");
                break;
        }
    }

    #endregion
}