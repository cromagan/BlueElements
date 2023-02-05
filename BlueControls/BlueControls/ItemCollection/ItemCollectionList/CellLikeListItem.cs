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
using BlueControls.Controls;
using BlueControls.Enums;
using BlueDatabase;
using BlueDatabase.Enums;
using System.Drawing;

namespace BlueControls.ItemCollection.ItemCollectionList;

public class CellLikeListItem : BasicListItem {
    // Implements IReadableText
    //http://www.kurztutorial.info/programme/punkt-mm/rechner.html
    // Dim Ausgleich As Double = MmToPixel(1 / 72 * 25.4, 300)
    //   Dim FixZoom As Single = 3.07F

    #region Fields

    private readonly BildTextVerhalten _bildTextverhalten;

    private readonly ShortenStyle _style;

    /// <summary>
    /// Nach welcher Spalte sich der Stil richten muss.
    /// Wichtig, dass es ein Spalten-Item ist, da bei neuen Datenbanken zwar die Spalte vorhanden ist,
    /// aber wenn keine Zeile Vorhanden ist, logischgerweise auch keine Zelle da ist.
    /// </summary>
    private readonly ColumnItem? _styleLikeThis;

    #endregion

    #region Constructors

    public CellLikeListItem(string internalAndReadableText, ColumnItem? columnStyle, ShortenStyle style, bool enabled, BildTextVerhalten bildTextverhalten) : base(internalAndReadableText, enabled) {
        _styleLikeThis = columnStyle;
        _style = style;
        _bildTextverhalten = bildTextverhalten;
    }

    #endregion

    #region Properties

    public override string QuickInfo => Internal.CreateHtmlCodes(true);

    #endregion

    #region Methods

    // unveränderter Text
    public override object Clone() {
        var x = new CellLikeListItem(Internal, _styleLikeThis, _style, Enabled, _bildTextverhalten);
        x.CloneBasicStatesFrom(this);
        return x;
    }

    public override bool FilterMatch(string filterText) {
        if (base.FilterMatch(filterText)) { return true; }
        if (_styleLikeThis == null) { return false; }
        var txt = CellItem.ValueReadable(_styleLikeThis, Internal, ShortenStyle.Both, _styleLikeThis.BehaviorOfImageAndText, true);
        return txt.ToUpper().Contains(filterText.ToUpper());
    }

    public override int HeightForListBox(BlueListBoxAppearance style, int columnWidth, Design itemdesign) => SizeUntouchedForListBox(itemdesign).Height;

    protected override Size ComputeSizeUntouchedForListBox(Design itemdesign) => Table.FormatedText_NeededSize(_styleLikeThis, Internal, Skin.GetBlueFont(itemdesign, States.Standard), _style, 16, _bildTextverhalten);

    protected override void DrawExplicit(Graphics gr, Rectangle positionModified, Design itemdesign, States vState, bool drawBorderAndBack, bool translate) {
        if (drawBorderAndBack) {
            Skin.Draw_Back(gr, itemdesign, vState, positionModified, null, false);
        }
        Table.Draw_FormatedText(gr, Internal, _styleLikeThis, positionModified, itemdesign, vState, _style, _bildTextverhalten);
        if (drawBorderAndBack) {
            Skin.Draw_Border(gr, itemdesign, vState, positionModified);
        }
    }

    protected override string GetCompareKey() {
        // Die hauptklasse frägt nach diesem Kompare-Key
        //var txt = CellItem.ValueReadable(_StyleLikeThis, Internal, ShortenStyle.HTML, true); // Muss Kompakt sein, um Suffixe zu vermeiden
        var txt = CellItem.ValueReadable(_styleLikeThis, Internal, ShortenStyle.HTML, _bildTextverhalten, true);

        return txt.CompareKey(_styleLikeThis.SortType) + "|" + Internal;
        //Internal.CompareKey(_StyleLikeThis.SortType) + "|" + Internal;
    }

    #endregion
}