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
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.Abstract;
using BlueDatabase;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using static BlueDatabase.Database;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class TablePadItem : RectanglePadItem, IStyleable, IStyleableChild {

    #region Fields

    private Database? _database;

    private string _defaultArrangement = string.Empty;

    private int _zeile_bis = -1;

    private int _zeile_von = -1;

    #endregion

    #region Constructors

    public TablePadItem() : base(string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "Table";

    public Database? Database {
        get => _database;

        set {
            if (IsDisposed) { return; }

            if (value == _database) { return; }

            _database = value;

            OnPropertyChanged();
            OnDoUpdateSideOptionMenu();
        }
    }

    public override string Description => "Darstellung einer Datenbank Tabelle.";

    public string SheetStyle {
        get {
            if (_parent is IStyleable ist) { return ist.SheetStyle; }
            return string.Empty;
        }
    }

    public float SheetStyleScale {
        get {
            if (_parent is IStyleable ist) { return ist.SheetStyleScale; }
            return 1f;
        }
    }

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

    [DefaultValue(-1)]
    public int Zeile_Bis {
        get => _zeile_bis;
        set {
            if (IsDisposed) { return; }
            if (_zeile_bis == value) { return; }
            _zeile_bis = value;
            OnPropertyChanged();
        }
    }

    [DefaultValue(-1)]
    public int Zeile_Von {
        get => _zeile_von;
        set {
            if (IsDisposed) { return; }
            if (_zeile_von == value) { return; }
            _zeile_von = value;
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            .. base.GetProperties(widthOfControl),
            new FlexiControl("Einstellungen:", widthOfControl, true),
            new FlexiControlForProperty<Database?>(() => Database, AllAvailableTables())
        ];

        if (_database is { IsDisposed: false } db) {
            result.Add(new FlexiControlForProperty<string>(() => Standard_Ansicht, AllAvailableColumArrangemengts(db)));
            result.Add(new FlexiControlForProperty<int>(() => Zeile_Von));
            result.Add(new FlexiControlForProperty<int>(() => Zeile_Bis));
        }

        //if (DatabaseOutput is { IsDisposed: false }) {
        //    var u = new List<AbstractListItem>();
        //    u.AddRange(ItemsOf(typeof(Filterausgabe)));
        //    result.Add(new FlexiControlForProperty<Filterausgabe>(() => FilterOutputType, u));
        //}
        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        if (_database is { IsDisposed: false } db) {
            result.ParseableAdd("Database", db.KeyName);
        }

        result.ParseableAdd("DefaultArrangement", _defaultArrangement);
        result.ParseableAdd("RowStart", _zeile_von);
        result.ParseableAdd("RowEnd", _zeile_bis);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "id":
                return true;

            case "database":

                _database = GetById(new ConnectionInfo(value.FromNonCritical(), null, string.Empty), false, null, true);

                return true;

            case "defaultarrangement":
                _defaultArrangement = value.FromNonCritical();
                return true;

            case "rowstart":
                _zeile_von = IntParse(value);
                return true;

            case "rowend":
                _zeile_bis = IntParse(value);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Tabellenansicht";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Tabelle, 16);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionModified, float scale, float shiftX, float shiftY) {
        //if (!ForPrinting) {
        //    DrawArrowOutput(gr, positionModified, scale, ForPrinting, OutputColorId);
        //    DrawColorScheme(gr, positionModified, scale, InputColorId, true, true, false);
        //}

        //base.DrawExplicit(gr,visibleArea,positionModified,scale,shiftX, shiftY);
        //DrawArrorInput(gr, positionModified, scale, ForPrinting, InputColorId);
    }

    #endregion
}