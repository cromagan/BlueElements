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
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Dieses Element kann Filter empfangen, und gibt dem Nutzer die Möglichkeit, aus dem daraus resultierenden Zeilen EINE zu wählen.
/// Per Tabellenansicht
/// </summary>

public class TableViewPadItem : ReciverSenderControlPadItem, IItemToControl, IAutosizable {

    #region Fields

    private string _defaultArrangement = string.Empty;
    private Filterausgabe _filterOutputType = Filterausgabe.Gewähle_Zeile;

    #endregion

    #region Constructors

    public TableViewPadItem() : this(string.Empty, null, null) { }

    public TableViewPadItem(string keyName, Database? db, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula, db) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-TableView";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None | AllowedInputFilter.More;
    public bool AutoSizeableHeight => true;

    public override bool DatabaseInputMustMatchOutputDatabase => true;

    public override string Description => "Darstellung einer Datenbank in Tabellen-Form.";

    public Filterausgabe FilterOutputType {
        get => _filterOutputType;
        set {
            if (IsDisposed) { return; }
            if (_filterOutputType == value) { return; }
            _filterOutputType = value;
            OnPropertyChanged();
        }
    }

    public override bool InputMustBeOneRow => false;
    public override bool MustBeInDrawingArea => true;

    [DefaultValue("")]
    public string Standard_Ansicht {
        get => _defaultArrangement;
        set {
            if (IsDisposed) { return; }
            if (_defaultArrangement == value) { return; }
            _defaultArrangement = value;
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new Table();
        con.DatabaseSet(DatabaseOutput, string.Empty);
        con.DoDefaultSettings(parent, this, mode);
        con.Arrangement = _defaultArrangement;
        con.EditButton = string.Equals(Generic.UserGroup, Constants.Administrator, StringComparison.OrdinalIgnoreCase);
        return con;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            .. base.GetProperties(widthOfControl),
            new FlexiControl("Einstellungen:", widthOfControl, true)
        ];

        if (DatabaseOutput is { IsDisposed: false } db) {
            var tcvc = ColumnViewCollection.ParseAll(db);
            var u2 = new List<AbstractListItem>();
            foreach (var thisC in tcvc) {
                u2.Add(ItemOf(thisC));
            }
            result.Add(new FlexiControlForProperty<string>(() => Standard_Ansicht, u2));
        }

        if (DatabaseOutput is { IsDisposed: false }) {
            var u = new List<AbstractListItem>();
            u.AddRange(ItemsOf(typeof(Filterausgabe)));
            result.Add(new FlexiControlForProperty<Filterausgabe>(() => FilterOutputType, u));
        }
        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("DefaultArrangement", _defaultArrangement);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "id":
                return true;

            case "defaultarrangement":
                _defaultArrangement = value.FromNonCritical();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Tabellenansicht: ";

        return txt + DatabaseOutput?.Caption;
    }

    public override QuickImage SymbolForReadableText() {
        return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionModified, float scale, float shiftX, float shiftY) {
        if (!ForPrinting) {
            DrawArrowOutput(gr, positionModified, scale, ForPrinting, OutputColorId);
            DrawColorScheme(gr, positionModified, scale, InputColorId, true, true, false);
        }

        base.DrawExplicit(gr, visibleArea, positionModified, scale, shiftX, shiftY);
        DrawArrorInput(gr, positionModified, scale, ForPrinting, InputColorId);
    }

    #endregion
}