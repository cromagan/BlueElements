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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueDatabase.Enums;

namespace BlueControls.ItemCollection;

/// <summary>
/// Erzeut ein EasyPic
/// Standard-Bearbeitungs-Feld
/// </summary>
public class EasyPicPadItem : FakeControlPadItem, IItemToControl, IItemAcceptRow {

    #region Fields

    private string _bild_Dateiname = string.Empty;

    private ItemAcceptRow _itemAccepts;

    #endregion

    #region Constructors

    public EasyPicPadItem(string internalname) : base(internalname) {
        _itemAccepts = new();
        SetCoordinates(new RectangleF(0, 0, 50, 30), true);
    }

    #endregion

    #region Properties

    public static string ClassId => "FI-EasyPic";

    [Description("Der Datename des Bildes, das angezeigt werden sollen.\r\nEs können Variablen aus dem Skript benutzt werden.\r\nDiese müssen im Format ~variable~ angegeben werden.")]
    public string Bild_Dateiname {
        get => _bild_Dateiname;

        set {
            if (value == _bild_Dateiname) { return; }
            _bild_Dateiname = value;
            this.RaiseVersion();
            OnChanged();
        }
    }

    [Description("Wählt ein Zeilen-Objekt, aus der die Werte kommen.")]
    public string Datenquelle_wählen {
        get => string.Empty;
        set => _itemAccepts.Datenquelle_wählen(this);
    }

    public IItemSendRow? GetRowFrom {
        get => _itemAccepts.GetRowFromGet(this);
        set => _itemAccepts.GetRowFromSet(value, this);
    }

    public override int InputColorId {
        get => _itemAccepts.InputColorIdGet();
        set => _itemAccepts.InputColorIdSet(value, this);
    }

    protected override int SaveOrder => 4;

    #endregion

    #region Methods

    public override Control CreateControl(ConnectedFormulaView parent) {
        var con = new EasyPic {
            OriginalText = Bild_Dateiname,
            Name = DefaultItemToControlName()
        };

        if (GetRowFrom is ICalculateRowsItemLevel rfw2) {
            var ff = parent.SearchOrGenerate(rfw2);
            if (ff is ICalculateRowsControlLevel cc) { cc.ChildAdd(con); }
        }
        return con;
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();
        l.AddRange(base.GetStyleOptions());
        l.Add(new FlexiControlForProperty<string>(() => Bild_Dateiname));
        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        if (_itemAccepts.ParseThis(tag, value)) { return true; }

        switch (tag) {
            case "imagename":
                _bild_Dateiname = value.FromNonCritical();
                return true;
        }
        return false;
    }

    public override string ToString() {
        var result = new List<string>();

        result.AddRange(_itemAccepts.ParsableTags());

        result.ParseableAdd("ImageName", _bild_Dateiname);
        return result.Parseable(base.ToString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        var id = -1;
        if (GetRowFrom != null) { id = GetRowFrom.OutputColorId; }

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, id);
            RowEntryPadItem.DrawInputArrow(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", InputColorId);
        }

        DrawFakeControl(gr, positionModified, zoom, ÜberschriftAnordnung.Über_dem_Feld, "Bilddatei");

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    #endregion

    //protected override BasicPadItem? TryCreate(string id, string name) {
    //    if (id.Equals(ClassId, StringComparison.OrdinalIgnoreCase)) {
    //        return new EasyPicPadItem(name);
    //    }
    //    return null;
    //}
}