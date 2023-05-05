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

public class EasyPicPadItem : FakeControlPadItem, IItemToControl, IItemAcceptRow {

    #region Fields

    private readonly ItemAcceptRow _itemAccepts;
    private string _bild_Dateiname = string.Empty;

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

    public override string Description => "Dieses Element erzeugt eine Bild-Steuerelement,\r\nwelches dann auch bearbeitet werden kann.";

    public IItemSendRow? GetRowFrom {
        get => _itemAccepts.GetRowFromGet(this);
        set => _itemAccepts.GetRowFromSet(value, this);
    }

    public override int InputColorId {
        get => _itemAccepts.InputColorIdGet();
        set => _itemAccepts.InputColorIdSet(this, value);
    }

    protected override int SaveOrder => 4;

    #endregion

    #region Methods

    public override Control CreateControl(ConnectedFormulaView parent) {
        var con = new EasyPic {
            OriginalText = Bild_Dateiname,
        };

        con.DoInputSettings(parent, this);
        //con.DoOutputSettings(this);
        return con;
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();
        l.AddRange(_itemAccepts.GetStyleOptions(this));
        l.Add(new FlexiControlForProperty<string>(() => Bild_Dateiname));

        //l.Add(new FlexiControl());
        //l.AddRange(base.GetStyleOptions());
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

    internal override void AddedToCollection() {
        base.AddedToCollection();
        //_itemSends.DoCreativePadAddedToCollection(this);
        _itemAccepts.DoCreativePadAddedToCollection(this);
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        var id = -1;
        if (GetRowFrom != null) { id = GetRowFrom.OutputColorId; }

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, id);
        }

        DrawFakeControl(gr, positionModified, zoom, ÜberschriftAnordnung.Über_dem_Feld, "Bilddatei");

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        RowEntryPadItem.DrawInputArrow(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", InputColorId);
    }

    protected override void ParseFinished() {
        base.ParseFinished();
        //_itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
    }

    #endregion

    //protected override BasicPadItem? TryCreate(string id, string name) {
    //    if (id.Equals(ClassId, StringComparison.OrdinalIgnoreCase)) {
    //        return new EasyPicPadItem(name);
    //    }
    //    return null;
    //}
}