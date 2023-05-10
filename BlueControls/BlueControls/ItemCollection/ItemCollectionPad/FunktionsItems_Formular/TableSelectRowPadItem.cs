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
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueDatabase;

namespace BlueControls.ItemCollection;

/// <summary>
/// Dieses Element kann Filter empfangen, und gibt dem Nutzer die Möglichkeit, aus dem daraus resultierenden Zeilen EINE zu wählen.
/// Per Tabellenansicht
/// </summary>

public class TableSelectRowPadItem : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptFilter, IItemSendRow {

    #region Fields

    private readonly ItemAcceptFilter _itemAccepts;

    private readonly ItemSendRow _itemSends;

    #endregion

    #region Constructors

    public TableSelectRowPadItem(string keyname, string toParse) : this(keyname) => Parse(toParse);

    public TableSelectRowPadItem(DatabaseAbstract? db) : this(string.Empty, db) { }

    public TableSelectRowPadItem(string intern, DatabaseAbstract? db) : base(intern) {
        _itemAccepts = new();
        _itemSends = new();

        OutputDatabase = db;
    }

    public TableSelectRowPadItem(string intern) : this(intern, null as DatabaseAbstract) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-SelectRowWithTable";

    public ReadOnlyCollection<string>? ChildIds {
        get => _itemSends.ChildIdsGet();
        set => _itemSends.ChildIdsSet(value, this);
    }

    public override string Description => "Darstellung einer Datenbank.\r\nKann Vorfilter empfangen und eine Zeile weitergeben.";

    public ReadOnlyCollection<string>? GetFilterFrom {
        get => _itemAccepts.GetFilterFromKeysGet();
        set => _itemAccepts.GetFilterFromKeysSet(value, this);
    }

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);

    public DatabaseAbstract? InputDatabase => _itemAccepts.InputDatabase(this);

    public int OutputColorId {
        get => _itemSends.OutputColorIdGet();
        set => _itemSends.OutputColorIdSet(value, this);
    }

    public DatabaseAbstract? OutputDatabase {
        get => _itemSends.OutputDatabaseGet();
        set => _itemSends.OutputDatabaseSet(value, this);
    }

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public void AddChild(IHasKeyName add) => _itemSends.AddChild(this, add);

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public override Control CreateControl(ConnectedFormulaView parent) {
        var con = new Table();
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
        l.AddRange(_itemSends.GetStyleOptions(this));

        l.Add(new FlexiControl());
        l.AddRange(base.GetStyleOptions());
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
        }
        return false;
    }

    public override string ReadableText() {
        var txt = "Tabellenansicht: ";

        if (IsOk() && OutputDatabase != null) {
            return txt + OutputDatabase.Caption;
        }

        return txt + ErrorReason();
    }

    public void RemoveChild(IItemAcceptSomething remove) => _itemSends.RemoveChild(remove, this);

    public override QuickImage? SymbolForReadableText() {
        if (IsOk()) {
            return QuickImage.Get(ImageCode.Datenbank, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        var result = new List<string>();
        result.AddRange(_itemAccepts.ParsableTags());
        result.AddRange(_itemSends.ParsableTags());
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
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        DrawArrorInput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", InputColorId);
    }

    protected override void ParseFinished() {
        base.ParseFinished();
        _itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
    }

    #endregion
}