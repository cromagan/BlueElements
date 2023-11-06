﻿// Authors:
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Extended_Text;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class FilterButtonPadItem : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptFilter, IAutosizable {

    #region Fields

    private readonly ItemAcceptFilter _itemAccepts;
    private string _anzeige = string.Empty;
    private int _enabledwhenrows = 0;
    private ExtText? _eTxt;
    private string _scriptname = string.Empty;

    #endregion

    #region Constructors

    public FilterButtonPadItem(string keyname, string toParse) : this(keyname) => this.Parse(toParse);

    public FilterButtonPadItem(string intern) : base(intern) => _itemAccepts = new();

    #endregion

    #region Properties

    public static string ClassId => "FI-FilterButton";

    [Description("Die Beschriftung des Knopfes.")]
    public string Anzeige {
        get => _anzeige;
        set {
            if (IsDisposed) { return; }
            if (_anzeige == value) { return; }
            _anzeige = value;
            OnChanged();
        }
    }

    public bool AutoSizeableHeight => false;

    public override string Description => "Dieses Element wird als Knopf zum drücken dargstellt.<br>Das Element kann Filter empfangen.<br>Diese Filter werden an ein Skript weitergegeben.";

    [Description("Schaltet den Knopf ein oder aus.<br>Dazu werden die Zeilen berechnet, die mit der Eingangsfilterung möglich sind.<br>Wobei ein Zahlenwert  größer 1 als 'mehr als eine' gilt.")]
    public int Drückbar_wenn {
        get => _enabledwhenrows;
        set {
            if (IsDisposed) { return; }
            if (_enabledwhenrows == value) { return; }
            _enabledwhenrows = value;
            OnChanged();
        }
    }

    public ReadOnlyCollection<string> GetFilterFrom {
        get => _itemAccepts.GetFilterFromKeysGet();
        set => _itemAccepts.GetFilterFromKeysSet(value, this);
    }

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);

    public DatabaseAbstract? InputDatabase => _itemAccepts.InputDatabase(this);

    public DatabaseAbstract? InputDatabaseMustBe => InputDatabase;

    public override bool MustBeInDrawingArea => true;

    [Description("Welches Skript ausgeführt werden soll")]
    public string SkriptName {
        get => _scriptname;
        set {
            if (IsDisposed) { return; }
            if (_scriptname == value) { return; }
            _scriptname = value;
            OnChanged();
        }
    }

    public bool WaitForDatabase => false;
    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public override Control CreateControl(ConnectedFormulaView parent) {
        var con = new ConnectedFormulaFilterButton();

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

    public override List<GenericControl> GetStyleOptions(int widthOfControl) {
        List<GenericControl> l = new();
        l.AddRange(_itemAccepts.GetStyleOptions(this, widthOfControl));

        if (InputDatabase is not DatabaseAbstract db || db.IsDisposed) { return l; }

        l.Add(new FlexiControl());
        l.AddRange(base.GetStyleOptions(widthOfControl));

        var sn = new ItemCollectionList.ItemCollectionList(true);
        sn.Add("#Neue Zeile in der Datenbank anlegen");

        foreach (var thisScript in db.EventScript) {
            sn.Add(thisScript.KeyName);
        }

        l.Add(new FlexiControlForProperty<string>(() => SkriptName, sn));

        var za = new ItemCollectionList.ItemCollectionList(true);
        za.Add("-1", "...egal - immer");
        za.Add("0", "...keine Zeile gefunden wurde");
        za.Add("1", "...genau eine Zeile gefunden wurde");

        l.Add(new FlexiControlForProperty<int>(() => Drückbar_wenn, za));

        return l;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        //_itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }

        if (_itemAccepts.ParseThis(tag, value)) { return true; }

        switch (tag) {
            case "showformat":
                _anzeige = value.FromNonCritical();
                return true;
        }
        return false;
    }

    public override string ReadableText() {
        var txt = "Knopf ";

        if (this.IsOk() && InputDatabase != null) {
            return txt + InputDatabase.Caption;
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
        List<string> result = new();

        result.AddRange(_itemAccepts.ParsableTags());

        result.ParseableAdd("ShowFormat", _anzeige);
        return result.Parseable(base.ToString());
    }

    internal override void AddedToCollection() {
        base.AddedToCollection();
        //_itemSends.DoCreativePadAddedToCollection(this);
        _itemAccepts.DoCreativePadAddedToCollection(this);
        //RepairConnections();
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, false);
        }

        _eTxt ??= new ExtText(Design.Button, States.Standard);
        Controls.Button.DrawButton(null, gr, Design.Button, States.Standard, QuickImage.Get(ImageCode.PlusZeichen), Alignment.Top_HorizontalCenter, false, _eTxt, "xxx", positionModified.ToRect(), false);

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    #endregion
}