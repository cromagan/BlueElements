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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.ItemCollection;

public class RowInputPadItem : RectanglePadItemWithVersion, IReadableText, IContentHolder, IItemToControl {

    #region Fields

    private string _spaltenname;

    #endregion

    #region Constructors

    public RowInputPadItem() : this(UniqueInternal(), string.Empty) { }

    public RowInputPadItem(string intern, string spaltenname) : base(intern) => _spaltenname = spaltenname;

    //Size = new Size(200, 40);
    public RowInputPadItem(string intern) : this(intern, string.Empty) { }

    #endregion

    #region Properties

    [Description("Aus welcher Spalte der Eingangs-Zeile kommen soll.\r\nEs muss der interne Spaltenname der ankommenden Zeile verwendet werden.\r\nAlternativ kann auch #first benutzt werden, wenn die erste Spalte benutzt werden soll.")]
    public string Spaltenname {
        get => _spaltenname;
        set {
            if (_spaltenname == value) { return; }
            _spaltenname = value;
            OnChanged();
        }
    }

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent) {
        var con = new FlexiControlForCell();
        con.Width = 200;
        con.Height = 32;
        con.CaptionPosition = ÜberschriftAnordnung.Über_dem_Feld;
        con.EditType = EditTypeFormula.nur_als_Text_anzeigen;
        con.Name = DefaultItemToControlName();
        return con;
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();
        l.Add(new FlexiControlForProperty<string>(() => Spaltenname));
        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "column":
                Spaltenname = value.FromNonCritical();
                return true;
        }
        return false;
    }

    public string ReadableText() => "Einganszeilen-Spalte: " + _spaltenname;

    public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Zeile, 10, Color.Transparent, Color.Green);

    public override string ToString() {
        var t = base.ToString();
        t = t.Substring(0, t.Length - 1) + ", ";

        t = t + "Column=" + _spaltenname.ToNonCritical() + ", ";
        return t.Trim(", ") + "}";
    }

    protected override string ClassId() => "FI-InputRow";

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            gr.DrawRectangle(new Pen(Color.Black, zoom), positionModified);
            var t = "Eingangs-Zeilen-Spalte\r\n" + _spaltenname;

            Skin.Draw_FormatedText(gr, t, SymbolForReadableText(), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnFont.Scale(zoom), false);
        } else {
            CustomizableShowPadItem.DrawFakeControl(gr, positionModified, zoom, ÜberschriftAnordnung.Links_neben_Dem_Feld, _spaltenname + ":");
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    protected override BasicPadItem? TryCreate(string id, string name) {
        if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
            return new RowInputPadItem(name);
        }
        return null;
    }

    #endregion
}