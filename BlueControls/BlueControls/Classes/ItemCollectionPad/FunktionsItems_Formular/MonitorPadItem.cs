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
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class MonitorPadItem : ReciverControlPadItem, IItemToControl, IAutosizable {

    #region Constructors

    public MonitorPadItem() : this(string.Empty, null) { }

    public MonitorPadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) => SetCoordinates(new RectangleF(0, 0, 50, 30), true);

    #endregion

    #region Properties

    public static string ClassId => "FI-Monitor";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One;
    public bool AutoSizeableHeight => true;

    public override bool DatabaseInputMustMatchOutputDatabase => false;
    public override string Description => "Zeigt Änderungen einer Zeile an.";

    public override bool InputMustBeOneRow => true;
    public override bool MustBeInDrawingArea => true;

    protected override int SaveOrder => 5;

    #endregion

    #region Methods

    public System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new Monitor();
        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        //result.ParseableAdd("Pfad", _pfad);
        //result.ParseableAdd("CreateDir", _bei_Bedarf_Erzeugen);
        //result.ParseableAdd("DeleteDir", _leere_Ordner_Löschen);
        return result;
    }

    public override string ReadableText() {
        const string txt = "Monitor: ";

        return txt + DatabaseInput?.Caption;
    }

    public override QuickImage SymbolForReadableText() {
        return QuickImage.Get(ImageCode.Textdatei, 16);
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float scale, float shiftX, float shiftY, bool forPrinting, bool showJointPoints){
        //var id = GetRowFrom?.OutputColorId ?? -1;

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, scale, InputColorId, true, true, false);
        }

        //DrawFakeControl(gr, positionModified, scale, CaptionPosition.Über_dem_Feld, "Monitor", EditTypeFormula.Listbox);

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, scale, InputColorId, true, true, true);
        }

        base.DrawExplicit(gr, positionModified, scale, shiftX, shiftY, forPrinting, showJointPoints);
        DrawArrorInput(gr, positionModified, scale, forPrinting, InputColorId);
    }

    #endregion
}