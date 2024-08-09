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
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Dieses Element ist in jedem Formular vorhanden und empfängt die Zeile aus einem anderen Element.
/// Hat NICHT IAcceptRowItem, da es nur von einer einzigen internen Routine befüllt werden darf.
/// Unsichtbares Element, wird nicht angezeigt.
/// </summary>
public class RowEntryPadItem : ReciverSenderControlPadItem, IReadableText {

    #region Constructors

    public RowEntryPadItem(string keyName, string toParse) : this(keyName, null, null) => this.Parse(toParse);

    public RowEntryPadItem(Database? db) : this(string.Empty, db, null) { }

    public RowEntryPadItem(string keyName, Database? db, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula, db) { }

    public RowEntryPadItem(string keyName) : this(keyName, null, null) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-RowEntryElement";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One | AllowedInputFilter.None;

    public override bool DatabaseInputMustMatchOutputDatabase => true;

    public override string Description => "Dieses Element ist in jedem Formular vorhanden und kann\r\ndie Zeile aus einem übergerordneten Element empfangen uns weitergeben.\r\n\r\nUnsichtbares Element, wird nicht angezeigt.";
    public List<int> InputColorId => [OutputColorId];
    public override bool InputMustBeOneRow => false;
    public override bool MustBeInDrawingArea => false;
    public override string MyClassId => ClassId;

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> l =
        [
            //.. _itemSends.GetProperties(this, widthOfControl),
            //new FlexiControl(),
            .. base.GetProperties(widthOfControl),
        ];

        return l;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "id": // TODO: 29.03.2023
                //Id = IntParse(value);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Eingangs-Zeile: ";

        if (this.IsOk() && DatabaseOutput != null) {
            return txt + DatabaseOutput.Caption;
        }

        return txt + ErrorReason();
    }

    public override QuickImage SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [];
        return result.Parseable(base.ToParseableString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        // Die Eigangszeile ist immer vom übergeordenetem Formular und wird einfach weitergegeben.
        // Deswegen ist InputColorID nur Fake

        if (!forPrinting) {
            DrawArrowOutput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, OutputColorId);
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, false);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);

        DrawArrorInput(gr, positionModified, zoom, forPrinting, InputColorId);
    }

    #endregion
}