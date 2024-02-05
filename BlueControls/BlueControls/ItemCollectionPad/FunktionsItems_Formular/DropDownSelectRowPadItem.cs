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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.ConnectedFormula;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using BlueDatabase.Enums;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class DropDownSelectRowPadItem : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptFilter, IItemSendSomething, IAutosizable {

    #region Fields

    private readonly ItemAcceptSomething _itemAccepts;
    private readonly ItemSendSomething _itemSends;
    private string _anzeige = string.Empty;
    private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld_mit_Auswahlknopf;
    private string _überschrift = string.Empty;
    private CaptionPosition _überschriftanordung = CaptionPosition.Über_dem_Feld;

    #endregion

    #region Constructors

    public DropDownSelectRowPadItem(string keyname, string toParse) : this(keyname, null as Database) => this.Parse(toParse);

    public DropDownSelectRowPadItem(Database? db) : this(string.Empty, db) { }

    public DropDownSelectRowPadItem(string intern, Database? db) : base(intern) {
        _itemAccepts = new();
        _itemSends = new();

        DatabaseOutput = db;
    }

    public DropDownSelectRowPadItem(string intern) : this(intern, null as Database) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-SelectRowWithDropDownMenu";

    [Description("Nach welchem Format die Zeilen angezeigt werden sollen. Es können Variablen im Format ~Variable~ benutzt werden. Achtung, KEINE Skript-Variaben, nur Spaltennamen.")]
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

    public CaptionPosition CaptionPosition {
        get => _überschriftanordung;
        set {
            if (IsDisposed) { return; }
            if (_überschriftanordung == value) { return; }
            _überschriftanordung = value;
            OnChanged();
        }
    }

    public ReadOnlyCollection<string> ChildIds {
        get => _itemSends.ChildIdsGet();
        set => _itemSends.ChildIdsSet(value, this);
    }

    public Database? DatabaseInput => _itemAccepts.DatabaseInput(this);
    public bool DatabaseInputMustMatchOutputDatabase => true;

    public Database? DatabaseOutput {
        get => _itemSends.DatabaseOutputGet();
        set => _itemSends.DatabaseOutputSet(value, this);
    }

    public override string Description => "Ein Auswahlmenü, aus dem der Benutzer eine Zeile wählen kann,<br>die durch die Vor-Filter bestimmt wurden.";
    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);
    public override bool MustBeInDrawingArea => true;
    public bool MustBeOneRow => false;

    public int OutputColorId {
        get => _itemSends.OutputColorIdGet();
        set => _itemSends.OutputColorIdSet(value, this);
    }

    public ReadOnlyCollection<string> Parents {
        get => _itemAccepts.GetFilterFromKeysGet();
        set => _itemAccepts.GetFilterFromKeysSet(value, this);
    }

    public string Überschrift {
        get => _überschrift;
        set {
            if (IsDisposed) { return; }
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

    public override System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent) {
        var con = new FlexiControlRowSelector(DatabaseOutput, _überschrift, _anzeige) {
            EditType = _bearbeitung,
            CaptionPosition = CaptionPosition
        };

        con.DoOutputSettings(this);
        con.DoInputSettings(parent, this);

        return con;
    }

    public override string ErrorReason() {
        var b = base.ErrorReason();
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = _itemAccepts.ErrorReason(this);
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = _itemSends.ErrorReason(this);
        if (!string.IsNullOrEmpty(b)) { return b; }

        return string.Empty;
    }

    public override List<GenericControl> GetStyleOptions(int widthOfControl) {
        List<GenericControl> l =
        [
            .. _itemAccepts.GetStyleOptions(this, widthOfControl),
            new FlexiControl(),
            .. _itemSends.GetStyleOptions(this, widthOfControl),
            new FlexiControl(),
            new FlexiControl("Einstellungen:", -1),
            new FlexiControlForProperty<string>(() => Überschrift),
            new FlexiControlForProperty<string>(() => Anzeige),
        ];

        var u = new ItemCollectionList.ItemCollectionList(false);
        u.AddRange(typeof(CaptionPosition));
        l.Add(new FlexiControlForProperty<CaptionPosition>(() => CaptionPosition, u));

        l.Add(new FlexiControl());
        l.AddRange(base.GetStyleOptions(widthOfControl));
        return l;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        _itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
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
                _überschriftanordung = (CaptionPosition)IntParse(value);
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
        const string txt = "Zeilenauswahl: ";

        if (this.IsOk() && DatabaseOutput != null) {
            return txt + DatabaseOutput.Caption;
        }

        return txt + ErrorReason();
    }

    public void RemoveChild(IItemAcceptFilter remove) => _itemSends.RemoveChild(remove, this);

    public override QuickImage SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [.. _itemAccepts.ParsableTags(), .. _itemSends.ParsableTags()];

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
            DrawArrowOutput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, OutputColorId);
            DrawFakeControl(gr, positionModified, zoom, CaptionPosition, _überschrift, EditTypeFormula.Textfeld_mit_Auswahlknopf);
            DrawColorScheme(gr, positionModified, zoom, null, true, true, true);
        } else {
            DrawFakeControl(gr, positionModified, zoom, CaptionPosition, _überschrift, EditTypeFormula.Textfeld_mit_Auswahlknopf);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, true);
        DrawArrorInput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, InputColorId);
    }

    #endregion
}