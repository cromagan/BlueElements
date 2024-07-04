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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Extended_Text;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using BlueScript;
using BlueScript.Methods;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.ItemCollectionList;
using static BlueBasics.Converter;
using Button = BlueControls.Controls.Button;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;

#nullable enable

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class ButtonPadItem : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptFilter, IAutosizable {

    #region Fields

    private readonly ItemAcceptFilter _itemAccepts;
    private string _action = string.Empty;
    private string _anzeige = string.Empty;
    private string _arg1 = string.Empty;
    private string _arg2 = string.Empty;
    private string _arg3 = string.Empty;
    private string _arg4 = string.Empty;
    private string _arg5 = string.Empty;
    private string _arg6 = string.Empty;
    private string _arg7 = string.Empty;
    private string _arg8 = string.Empty;
    private ButtonArgs _enabledwhenrows;
    private ExtText? _eTxt;
    private string _image = string.Empty;
    private string _quickinfo = string.Empty;

    #endregion

    #region Constructors

    public ButtonPadItem(string keyName) : this(keyName, null) { }

    public ButtonPadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) => _itemAccepts = new();

    #endregion

    #region Properties

    public static string ClassId => "FI-FilterButton";

    [Description("Welches Skript ausgeführt werden soll")]
    public string Aktion {
        get => _action;
        set {
            if (IsDisposed) { return; }
            if (_action == value) { return; }
            _action = value;
            OnPropertyChanged();
            UpdateSideOptionMenu();
        }
    }

    public AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None | AllowedInputFilter.More;

    [Description("Muss befüllt werden.\r\nVariablen können benutzt werden.\r\nTexte müssen mit \" beginnen.")]
    public string Arg1 {
        get => _arg1;
        set {
            if (IsDisposed) { return; }
            if (_arg1 == value) { return; }
            _arg1 = value;
            OnPropertyChanged();
        }
    }

    [Description("Muss befüllt werden.\r\nVariablen können benutzt werden.\r\nTexte müssen mit \" beginnen.")]
    public string Arg2 {
        get => _arg2;
        set {
            if (IsDisposed) { return; }
            if (_arg2 == value) { return; }
            _arg2 = value;
            OnPropertyChanged();
        }
    }

    [Description("Muss befüllt werden.\r\nVariablen können benutzt werden.\r\nTexte müssen mit \" beginnen.")]
    public string Arg3 {
        get => _arg3;
        set {
            if (IsDisposed) { return; }
            if (_arg3 == value) { return; }
            _arg3 = value;
            OnPropertyChanged();
        }
    }

    [Description("Muss befüllt werden.\r\nVariablen können benutzt werden.\r\nTexte müssen mit \" beginnen.")]
    public string Arg4 {
        get => _arg4;
        set {
            if (IsDisposed) { return; }
            if (_arg4 == value) { return; }
            _arg4 = value;
            OnPropertyChanged();
        }
    }

    [Description("Muss befüllt werden.\r\nVariablen können benutzt werden.\r\nTexte müssen mit \" beginnen.")]
    public string Arg5 {
        get => _arg5;
        set {
            if (IsDisposed) { return; }
            if (_arg5 == value) { return; }
            _arg5 = value;
            OnPropertyChanged();
        }
    }

    [Description("Muss befüllt werden.\r\nVariablen können benutzt werden.\r\nTexte müssen mit \" beginnen.")]
    public string Arg6 {
        get => _arg6;
        set {
            if (IsDisposed) { return; }
            if (_arg6 == value) { return; }
            _arg6 = value;
            OnPropertyChanged();
        }
    }

    [Description("Muss befüllt werden.\r\nVariablen können benutzt werden.\r\nTexte müssen mit \" beginnen.")]
    public string Arg7 {
        get => _arg7;
        set {
            if (IsDisposed) { return; }
            if (_arg7 == value) { return; }
            _arg7 = value;
            OnPropertyChanged();
        }
    }

    [Description("Muss befüllt werden.\r\nVariablen können benutzt werden.\r\nTexte müssen mit \" beginnen.")]
    public string Arg8 {
        get => _arg8;
        set {
            if (IsDisposed) { return; }
            if (_arg8 == value) { return; }
            _arg8 = value;
            OnPropertyChanged();
        }
    }

    public bool AutoSizeableHeight => false;

    [Description("Die Beschriftung des Knopfes.")]
    public string Beschriftung {
        get => _anzeige;
        set {
            if (IsDisposed) { return; }
            if (_anzeige == value) { return; }
            _anzeige = value;
            OnPropertyChanged();
        }
    }

    [Description("Ein Bild für den Knopf. Beispiel: PlusZeichen|16")]
    public string Bild {
        get => _image;
        set {
            if (IsDisposed) { return; }
            if (_image == value) { return; }
            _image = value;
            OnPropertyChanged();
        }
    }

    [Description("Eine Information, die dem Benutzer angezeigt wird,\r\nwenn er mit der Maus über den Knopf fährt..")]
    public string ButtonQuickInfo {
        get => _quickinfo;
        set {
            if (IsDisposed) { return; }
            if (_quickinfo == value) { return; }
            _quickinfo = value;
            OnPropertyChanged();
        }
    }

    public Database? DatabaseInput => _itemAccepts.DatabaseInputGet(this);
    public bool DatabaseInputMustMatchOutputDatabase => false;
    public override string Description => "Ein Knopf, den der Benutzer drücken kann und eine Aktion startet.";

    [Description("Schaltet den Knopf ein oder aus.<br>Dazu werden die Zeilen berechnet, die mit der Eingangsfilterung möglich sind.<br>Wobei ein Zahlenwert größer 1 als 'mehr als eine' gilt.")]
    public ButtonArgs Drückbar_wenn {
        get => _enabledwhenrows;
        set {
            if (IsDisposed) { return; }
            if (_enabledwhenrows == value) { return; }
            _enabledwhenrows = value;
            if (!PossibleFor(Script.Commands.Get(_action), _enabledwhenrows)) { Aktion = string.Empty; }
            OnPropertyChanged();
            UpdateSideOptionMenu();
        }
    }

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);
    public bool InputMustBeOneRow => false;
    public override bool MustBeInDrawingArea => true;

    public ReadOnlyCollection<string> Parents {
        get => _itemAccepts.GetFilterFromKeysGet();
        set => _itemAccepts.GetFilterFromKeysSet(value, this);
    }

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public static bool PossibleFor(Method? toCheck, ButtonArgs clickableWhen) {
        if (toCheck is not IUseableForButton ufb) { return false; }

        if (!ufb.ClickableWhen.HasFlag(clickableWhen)) { return false; }

        //if (toCheck.GetCodeBlockAfter) { return false; }
        //if (toCheck.Args.Count == 0) { return false; }
        //if (toCheck.MustUseReturnValue) { return false; }

        //if (clickableWhen is not ButtonArgs.Egal and
        //                      not ButtonArgs.Genau_eine_Zeile) {
        //    // Egal MUSS eine Zeile berechnen. Und da müssen am End die Filter rein
        //    // Und DIESE müssen endlos erlaubt sein.
        //    if (toCheck.LastArgMinCount < 1) { return false; }
        //    if (toCheck.Args.Count > 4) { return false; }
        //} else {
        //    if (toCheck.LastArgMinCount != -1) { return false; }
        //    if (toCheck.Args.Count > 5) { return false; }
        //}

        //int? rowno = null;
        //int? fono = null;

        //for (var nr = 0; nr < toCheck.Args.Count; nr++) {
        //    var thisarg = toCheck.Args[nr];

        //    if (thisarg.Count != 1) { return false; }

        //    if (thisarg[0] != VariableFloat.ShortName_Plain &&
        //        thisarg[0] != VariableString.ShortName_Plain &&
        //        thisarg[0] != VariableBool.ShortName_Plain &&
        //        thisarg[0] != VariableFilterItem.ShortName_Variable &&
        //        thisarg[0] != VariableRowItem.ShortName_Variable) { return false; }

        //    if (thisarg[0] == VariableRowItem.ShortName_Variable) {
        //        if (rowno != null) { return false; }
        //        rowno = nr;
        //    }

        //    if (thisarg[0] == VariableFilterItem.ShortName_Variable) {
        //        if (fono != null) { return false; }
        //        fono = nr;
        //    }
        //}

        //if (fono != null && rowno != null) { return false; }

        //if (clickableWhen is ButtonArgs.Keine_Zeile or
        //    ButtonArgs.Eine_oder_mehr_Zeilen) {
        //    if (rowno != null) { return false; }
        //    if (fono == null || fono != toCheck.Args.Count - 1) { return false; }
        //}

        //if (clickableWhen == ButtonArgs.Genau_eine_Zeile) {
        //    //Steuerung über Variablen möglich!
        //    //if (rowno == null) { return false; }
        //}

        //if (clickableWhen == ButtonArgs.Egal) {
        //    if (fono != null || rowno != null) { return false; }
        //}

        return true;
    }

    public override void AddedToCollection() {
        base.AddedToCollection();
        //_itemSends.DoCreativePadAddedToCollection(this);
        _itemAccepts.DoCreativePadAddedToCollection(this);
        //RepairConnections();
    }

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public override System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent) {
        var con = new ConnectedFormulaButton() {
            Text = _anzeige,
            ImageCode = _image + "|16",
            Drückbar_wenn = _enabledwhenrows,
            Arg1 = _arg1,
            Arg2 = _arg2,
            Arg3 = _arg3,
            Arg4 = _arg4,
            Arg5 = _arg5,
            Arg6 = _arg6,
            Arg7 = _arg7,
            Arg8 = _arg8,
            Action = _action,
            QuickInfo = ButtonQuickInfo
        };

        con.DoInputSettings(parent, this);
        //con.DoOutputSettings(this);

        return con;
    }

    public override string ErrorReason() {
        var b = base.ErrorReason();
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = _itemAccepts.ErrorReason(this);
        if (!string.IsNullOrEmpty(b)) { return b; }

        //b = _itemSends.ErrorReason(this);
        //if (!string.IsNullOrEmpty(b)) { return b; }

        return string.Empty;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> l = [.. _itemAccepts.GetProperties(this, widthOfControl)];

        if (DatabaseInput is not Database db || db.IsDisposed) { return l; }

        //l.Add(new FlexiControl());

        l.Add(new FlexiControl("Eigenschaften:", widthOfControl, true));

        l.Add(new FlexiControlForProperty<string>(() => Beschriftung));

        var im = QuickImage.Images();

        var c = new List<AbstractListItem>();
        foreach (var thisIm in im) {
            c.Add(ItemOf(thisIm, thisIm, QuickImage.Get(thisIm, 16)));
        }

        l.Add(new FlexiControlForProperty<string>(() => Bild, c));

        //var sn = new  List<AbstractListItem>(true) {
        //    "#Neue Zeile in der Datenbank anlegen"
        //};

        //foreach (var thisScript in db.EventScript) {
        //    sn.Add(thisScript.KeyName);
        //}

        //l.Add(new FlexiControlForProperty<string>(() => SkriptName, sn));

        var za = new List<AbstractListItem>();
        za.Add(ItemOf("...keine Zeile gefunden wurde", ((int)ButtonArgs.Keine_Zeile).ToString()));
        za.Add(ItemOf("...genau eine Zeile gefunden wurde", ((int)ButtonArgs.Genau_eine_Zeile).ToString()));
        za.Add(ItemOf("...genau eine oder mehr Zeilen gefunden wurden", ((int)ButtonArgs.Eine_oder_mehr_Zeilen).ToString()));
        za.Add(ItemOf("...egal - immer", ((int)ButtonArgs.Egal).ToString()));

        l.Add(new FlexiControlForProperty<ButtonArgs>(() => Drückbar_wenn, za));

        var co = new List<AbstractListItem>();

        if (Script.Commands != null) {
            foreach (var cmd in Script.Commands) {
                if (PossibleFor(cmd, Drückbar_wenn) && cmd is IUseableForButton ufb2) {
                    co.Add(ItemOf(ufb2.NiceTextForUser, ufb2.Command));
                }
            }
        }

        l.Add(new FlexiControl("Aktion bei Drücken:", widthOfControl, true));

        l.Add(new FlexiControlForProperty<string>(() => Aktion, co));

        var m = Script.Commands.Get(_action);

        if (m is IUseableForButton ufb) {
            if (ufb.ArgsForButton.Count > 0) { l.Add(new FlexiControlForProperty<string>(() => Arg1, ufb.ArgsForButtonDescription[0])); }
            if (ufb.ArgsForButton.Count > 1) { l.Add(new FlexiControlForProperty<string>(() => Arg2, ufb.ArgsForButtonDescription[1])); }
            if (ufb.ArgsForButton.Count > 2) { l.Add(new FlexiControlForProperty<string>(() => Arg3, ufb.ArgsForButtonDescription[2])); }
            if (ufb.ArgsForButton.Count > 3) { l.Add(new FlexiControlForProperty<string>(() => Arg4, ufb.ArgsForButtonDescription[3])); }
            if (ufb.ArgsForButton.Count > 4) { l.Add(new FlexiControlForProperty<string>(() => Arg5, ufb.ArgsForButtonDescription[4])); }
            if (ufb.ArgsForButton.Count > 5) { l.Add(new FlexiControlForProperty<string>(() => Arg6, ufb.ArgsForButtonDescription[5])); }
            if (ufb.ArgsForButton.Count > 6) { l.Add(new FlexiControlForProperty<string>(() => Arg7, ufb.ArgsForButtonDescription[6])); }
            if (ufb.ArgsForButton.Count > 7) { l.Add(new FlexiControlForProperty<string>(() => Arg8, ufb.ArgsForButtonDescription[7])); }
        }

        l.Add(new FlexiControlForProperty<string>(() => ButtonQuickInfo, 3));

        l.AddRange(base.GetProperties(widthOfControl));

        return l;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        //_itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
    }

    public override bool ParseThis(string key, string value) {
        if (base.ParseThis(key, value)) { return true; }

        if (_itemAccepts.ParseThis(key, value)) { return true; }

        switch (key) {
            case "caption":
                _anzeige = value.FromNonCritical();
                return true;

            case "image":
                _image = value.FromNonCritical();
                return true;

            case "arg1":
                _arg1 = value.FromNonCritical();
                return true;

            case "arg2":
                _arg2 = value.FromNonCritical();
                return true;

            case "arg3":
                _arg3 = value.FromNonCritical();
                return true;

            case "arg4":
                _arg4 = value.FromNonCritical();
                return true;

            case "arg5":
                _arg5 = value.FromNonCritical();
                return true;

            case "arg6":
                _arg6 = value.FromNonCritical();
                return true;

            case "arg7":
                _arg7 = value.FromNonCritical();
                return true;

            case "arg8":
                _arg8 = value.FromNonCritical();
                return true;

            case "quickinfo":
                _quickinfo = value.FromNonCritical();
                return true;

            case "enablewhenrows":
                _enabledwhenrows = (ButtonArgs)IntParse(value);
                return true;

            case "action":
                _action = value.FromNonCritical();
                return true;
        }

        //result.ParseableAdd("Caption", _anzeige);
        //result.ParseableAdd("Image", _image);

        //result.ParseableAdd("EnableWhenRows", _enabledwhenrows);
        //result.ParseableAdd("ScriptName", _scriptname);

        return false;
    }

    public override string ReadableText() {
        const string txt = "Knopf ";

        if (this.IsOk() && DatabaseInput != null) {
            return txt + DatabaseInput.Caption;
        }

        return txt + ErrorReason();
    }

    public override QuickImage SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Stop, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [.. _itemAccepts.ParsableTags()];

        result.ParseableAdd("Caption", _anzeige);
        result.ParseableAdd("Image", _image);

        result.ParseableAdd("Arg1", _arg1);
        result.ParseableAdd("Arg2", _arg2);
        result.ParseableAdd("Arg3", _arg3);
        result.ParseableAdd("Arg4", _arg4);
        result.ParseableAdd("Arg5", _arg5);
        result.ParseableAdd("Arg6", _arg6);
        result.ParseableAdd("Arg7", _arg7);
        result.ParseableAdd("Arg8", _arg8);

        result.ParseableAdd("QuickInfo", _quickinfo);
        result.ParseableAdd("EnableWhenRows", _enabledwhenrows);
        result.ParseableAdd("Action", _action);
        return result.Parseable(base.ToString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        //if (!forPrinting) {
        //    DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, false);
        //}

        //if (!forPrinting) {
        //    DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, false);
        //}

        //if (Column  ==null || Column .IsDisposed) {
        //    Skin.Draw_FormatedText(gr, "Spalte fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), CaptionFnt.Scale(zoom), true);
        //} else {
        //DrawFakeControl(gr, positionModified, zoom, CaptionPosition, Column?.ReadableText() + ":", EditType);
        //}

        _eTxt ??= new ExtText(Design.Button, States.Standard);
        Button.DrawButton(null, gr, Design.Button, States.Standard, QuickImage.Get(_image), Alignment.Horizontal_Vertical_Center, false, _eTxt, _anzeige, positionModified.ToRect(), false);

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId, false, false, true);
        }

        //base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);

        DrawArrorInput(gr, positionModified, zoom, forPrinting, InputColorId);

        //base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    #endregion
}