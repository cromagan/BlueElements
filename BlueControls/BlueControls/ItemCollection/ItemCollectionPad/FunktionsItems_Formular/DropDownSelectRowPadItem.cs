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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.ConnectedFormula;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection;

public class DropDownSelectRowPadItem : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptFilter, IItemSendRow {

    #region Fields

    private readonly ItemAcceptFilter _itemAccepts;
    private readonly ItemSendRow _itemSends;
    private string _anzeige = string.Empty;
    private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld_mit_Auswahlknopf;
    private string _überschrift = string.Empty;
    private ÜberschriftAnordnung _überschriftanordung = ÜberschriftAnordnung.Über_dem_Feld;

    #endregion

    #region Constructors

    public DropDownSelectRowPadItem(string keyname, string toParse) : this(keyname, null as DatabaseAbstract) => Parse(toParse);

    public DropDownSelectRowPadItem(DatabaseAbstract? db) : this(string.Empty, db) { }

    public DropDownSelectRowPadItem(string intern, DatabaseAbstract? db) : base(intern) {
        _itemAccepts = new();
        _itemSends = new();

        OutputDatabase = db;
    }

    public DropDownSelectRowPadItem(string intern) : this(intern, null as DatabaseAbstract) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-SelectRowWithDropDownMenu";

    [Description("Nach welchem Format die Zeilen angezeigt werden sollen. Es können Variablen im Format ~Variable~ benutzt werden. Achtung, KEINE Skript-Variaben, nur Spaltennamen.")]
    public string Anzeige {
        get => _anzeige;
        set {
            if (_anzeige == value) { return; }
            _anzeige = value;
            OnChanged();
        }
    }

    public ÜberschriftAnordnung CaptionPosition {
        get => _überschriftanordung;
        set {
            if (_überschriftanordung == value) { return; }
            _überschriftanordung = value;
            OnChanged();
        }
    }

    public ReadOnlyCollection<string>? ChildIds {
        get => _itemSends.ChildIdsGet();
        set => _itemSends.ChildIdsSet(value, this);
    }

    public override string Description => "Dieses Element kann Filter empfangen, und gibt dem Nutzer die Möglichkeit,\r\naus dem daraus resultierenden Zeilen EINE per Dropdownmenu zu wählen.";

    public ReadOnlyCollection<string> GetFilterFrom {
        get => _itemAccepts.GetFilterFromKeysGet();
        set => _itemAccepts.GetFilterFromKeysSet(value, this);
    }

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);
    public DatabaseAbstract? InputDatabase => _itemAccepts.InputDatabase(this);
    public bool OnlyOneInputDatabase => true;

    public int OutputColorId {
        get => _itemSends.OutputColorIdGet();
        set => _itemSends.OutputColorIdSet(value, this);
    }

    public DatabaseAbstract? OutputDatabase {
        get => _itemSends.OutputDatabaseGet();
        set => _itemSends.OutputDatabaseSet(value, this);
    }

    public string Überschrift {
        get => _überschrift;
        set {
            if (_überschrift == value) { return; }
            _überschrift = value;
            OnChanged();
        }
    }

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public void AddChild(IHasKeyName add) => _itemSends.AddChild(this, add);

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public override Control CreateControl(ConnectedFormulaView parent) {
        var con = new FlexiControlRowSelectorNew(OutputDatabase, _überschrift, _anzeige) {
            EditType = _bearbeitung,
            CaptionPosition = CaptionPosition,
        };

        con.DoOutputSettings(parent, this);
        con.DoInputSettings(parent, this);

        return con;
    }

    public override string ErrorReason() {
        if (InputDatabase == null || InputDatabase.IsDisposed) {
            return "Quelle fehlt";
        }
        if (OutputDatabase == null || OutputDatabase.IsDisposed) {
            return "Ziel fehlt";
        }
        return string.Empty;
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();
        l.AddRange(_itemAccepts.GetStyleOptions(this));
        l.Add(new FlexiControl());
        l.AddRange(_itemSends.GetStyleOptions(this));
        l.Add(new FlexiControl());
        l.Add(new FlexiControl("Einstellungen:"));
        l.Add(new FlexiControlForProperty<string>(() => Überschrift));
        l.Add(new FlexiControlForProperty<string>(() => Anzeige));

        var u = new ItemCollectionList.ItemCollectionList(false);
        u.AddRange(typeof(ÜberschriftAnordnung));
        l.Add(new FlexiControlForProperty<ÜberschriftAnordnung>(() => CaptionPosition, u));

        l.Add(new FlexiControl());
        l.AddRange(base.GetStyleOptions());
        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        if (_itemAccepts.ParseThis(tag, value)) { return true; }
        if (_itemSends.ParseThis(tag, value)) { return true; }

        switch (tag) {
            case "id":
                //ColorId = IntParse(value);
                return true;

            case "edittype":
                _bearbeitung = (EditTypeFormula)IntParse(value);
                return true;

            case "caption":
                _überschriftanordung = (ÜberschriftAnordnung)IntParse(value);
                return true;

            case "captiontext":
                _überschrift = value.FromNonCritical();
                return true;

            case "showformat":
                _anzeige = value.FromNonCritical();
                return true;
        }
        return false;
    }

    public override string ReadableText() {
        var txt = "Zeilenauswahl: ";

        if (IsOk() && OutputDatabase != null) {
            return txt + OutputDatabase.Caption;
        }

        return txt + ErrorReason();
    }

    public void RemoveChild(IItemAcceptSomething remove) => _itemSends.RemoveChild(remove, this);

    public override QuickImage SymbolForReadableText() {
        if (IsOk()) {
            return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        var result = new List<string>();
        result.AddRange(_itemAccepts.ParsableTags());
        result.AddRange(_itemSends.ParsableTags());

        result.ParseableAdd("CaptionText", _überschrift);
        result.ParseableAdd("ShowFormat", _anzeige);
        result.ParseableAdd("EditType", _bearbeitung);
        result.ParseableAdd("Caption", _überschriftanordung);
        //result.ParseableAdd("ID", ColorId);

        return result.Parseable(base.ToString());
    }

    internal override void AddedToCollection() {
        base.AddedToCollection();
        _itemSends.DoCreativePadAddedToCollection(this);
        _itemAccepts.DoCreativePadAddedToCollection(this);
        //RepairConnections();
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawArrowOutput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", OutputColorId);
            DrawFakeControl(gr, positionModified, zoom, CaptionPosition, _überschrift);
            DrawColorScheme(gr, positionModified, zoom, null, true, true, true);
        } else {
            DrawFakeControl(gr, positionModified, zoom, CaptionPosition, _überschrift);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, true);
        DrawArrorInput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", InputColorId);
    }

    protected override void ParseFinished() {
        base.ParseFinished();
        _itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
    }

    #endregion
}