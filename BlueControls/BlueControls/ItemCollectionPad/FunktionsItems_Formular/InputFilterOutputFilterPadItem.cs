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
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using BlueDatabase.Enums;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Dieses Element kann einen Vorfilter empfangen und stellt dem Benutzer die Wahl, einen neuen Filter auszuwählen und gibt diesen weiter.
/// </summary>

public class InputFilterOutputFilterPadItem : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptFilter, IItemSendFilter, IAutosizable {

    #region Fields

    private readonly ItemAcceptFilter _itemAccepts;
    private readonly ItemSendFilter _itemSends;
    private string _anzeige = string.Empty;
    private string _überschrift = string.Empty;
    private ÜberschriftAnordnung _überschriftanordung = ÜberschriftAnordnung.Über_dem_Feld;

    #endregion

    #region Constructors

    public InputFilterOutputFilterPadItem(string keyname, string toParse) : this(keyname, null as DatabaseAbstract) => Parse(toParse);

    public InputFilterOutputFilterPadItem(DatabaseAbstract? db) : this(string.Empty, db) { }

    public InputFilterOutputFilterPadItem(string intern, DatabaseAbstract? db) : base(intern) {
        _itemAccepts = new();
        _itemSends = new();

        OutputDatabase = db;
    }

    public InputFilterOutputFilterPadItem(string intern) : this(intern, null as DatabaseAbstract) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-InputOutputElement";

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

    public ÜberschriftAnordnung CaptionPosition {
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

    public override string Description => "Dieses Element kann Filter empfangen - um eine Vorauswahl der verfügbaren Filterelement darzustallen.\r\nAnschließend kann mit den übrig gebliebenen Werten ein neuer Filter erzeugt werden, den der Benutzer auswählen kann.";

    public ReadOnlyCollection<string> GetFilterFrom {
        get => _itemAccepts.GetFilterFromKeysGet();
        set => _itemAccepts.GetFilterFromKeysSet(value, this);
    }

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);
    public DatabaseAbstract? InputDatabase => _itemAccepts.InputDatabase(this);

    public DatabaseAbstract? InputDatabaseMustBe => OutputDatabase;
    public override bool MustBeInDrawingArea => true;

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
            if (IsDisposed) { return; }
            if (_überschrift == value) { return; }
            _überschrift = value;
            OnChanged();
        }
    }

    public bool WaitForDatabase => true;
    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public void AddChild(IHasKeyName add) => _itemSends.AddChild(this, add);

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public override Control CreateControl(ConnectedFormulaView parent) {
        var con = new FlexiControlForFilterNew(OutputDatabase) {
            //EditType = _bearbeitung,
            //CaptionPosition = CaptionPosition,
            //Name = DefaultItemToControlName()
        };
        //return con;
        con.DoOutputSettings(parent, this);
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
        List<GenericControl> l = new();
        l.AddRange(_itemAccepts.GetStyleOptions(this, widthOfControl));
        l.AddRange(_itemSends.GetStyleOptions(this, widthOfControl));

        l.Add(new FlexiControl());

        l.Add(new FlexiControlForProperty<string>(() => Überschrift));
        l.Add(new FlexiControlForProperty<string>(() => Anzeige));

        var u = new ItemCollectionList.ItemCollectionList(false);
        u.AddRange(typeof(ÜberschriftAnordnung));
        l.Add(new FlexiControlForProperty<ÜberschriftAnordnung>(() => CaptionPosition, u));

        l.Add(new FlexiControl());
        l.AddRange(base.GetStyleOptions(widthOfControl));

        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        if (_itemSends.ParseThis(tag, value)) { return true; }
        if (_itemAccepts.ParseThis(tag, value)) { return true; }

        switch (tag) {
            case "id":
                //Id = IntParse(value);
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
        var txt = "Filter: ";

        if (this.IsOk() && OutputDatabase != null) {
            return txt + OutputDatabase.Caption;
        }

        return txt + ErrorReason();
    }

    public void RemoveChild(IItemAcceptSomething remove) => _itemSends.RemoveChild(remove, this);

    public override QuickImage SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Trichter, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        var result = new List<string>();

        result.AddRange(_itemAccepts.ParsableTags());
        result.AddRange(_itemSends.ParsableTags());

        result.ParseableAdd("CaptionText", _überschrift);
        result.ParseableAdd("ShowFormat", _anzeige);
        result.ParseableAdd("Caption", _überschriftanordung);
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
            DrawArrowOutput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Trichter", OutputColorId);
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, false);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        DrawArrorInput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Trichter", InputColorId);
    }

    protected override void ParseFinished() {
        base.ParseFinished();
        _itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
    }

    #endregion
}