﻿// Authors:
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
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Dieses Element kann Filter empfangen, und gibt dem Nutzer die Möglichkeit, aus dem daraus resultierenden Zeilen EINE zu wählen.
/// Per Tabellenansicht
/// </summary>

public class TableViewPadItem : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptSomething, IItemSendSomething, IAutosizable {

    #region Fields

    private readonly ItemAcceptSomething _itemAccepts;
    private readonly ItemSendSomething _itemSends;

    private Filterausgabe _filterOutputType = Filterausgabe.Gewähle_Zeile;

    #endregion

    #region Constructors

    public TableViewPadItem(string keyname, string toParse) : this(keyname) => this.Parse(toParse);

    public TableViewPadItem(Database? db) : this(string.Empty, db) { }

    public TableViewPadItem(string intern, Database? db) : base(intern) {
        _itemAccepts = new();
        _itemSends = new();

        DatabaseOutput = db;
    }

    public TableViewPadItem(string intern) : this(intern, null as Database) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-TableView";
    public bool AutoSizeableHeight => true;

    public ReadOnlyCollection<string> ChildIds {
        get => _itemSends.ChildIdsGet();
        set => _itemSends.ChildIdsSet(value, this);
    }

    public Database? DatabaseInput => _itemAccepts.DatabaseInput(this);
    public Database? DatabaseInputMustBe => DatabaseOutput;

    public Database? DatabaseOutput {
        get => _itemSends.DatabaseOutputGet();
        set => _itemSends.DatabaseOutputSet(value, this);
    }

    public override string Description => "Darstellung einer Datenbank in Tabellen-Form.";

    public Filterausgabe FilterOutputType {
        get => _filterOutputType;
        set {
            if (IsDisposed) { return; }
            if (_filterOutputType == value) { return; }
            _filterOutputType = value;
            OnChanged();
        }
    }

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

    public bool WaitForDatabase => true;
    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public void AddChild(IHasKeyName add) => _itemSends.AddChild(this, add);

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public override System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent) {
        var con = new Table();
        con.DatabaseSet(DatabaseOutput, string.Empty);
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
        List<GenericControl> l = [];

        var u = new ItemCollectionList.ItemCollectionList(false);
        u.AddRange(typeof(Filterausgabe));
        l.Add(new FlexiControlForProperty<Filterausgabe>(() => FilterOutputType, u));

        l.Add(new FlexiControl());

        l.AddRange(_itemAccepts.GetStyleOptions(this, widthOfControl));
        l.AddRange(_itemSends.GetStyleOptions(this, widthOfControl));
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
        if (_itemSends.ParseThis(tag, value)) { return true; }
        if (_itemAccepts.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "id":
                //Id = IntParse(value);
                return true;
        }
        return false;
    }

    public override string ReadableText() {
        const string txt = "Tabellenansicht: ";

        if (this.IsOk() && DatabaseOutput != null) {
            return txt + DatabaseOutput.Caption;
        }

        return txt + ErrorReason();
    }

    public void RemoveChild(IItemAcceptSomething remove) => _itemSends.RemoveChild(remove, this);

    public override QuickImage SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Datenbank, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [.. _itemAccepts.ParsableTags(), .. _itemSends.ParsableTags()];
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
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, false);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        DrawArrorInput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, InputColorId);
    }

    #endregion
}