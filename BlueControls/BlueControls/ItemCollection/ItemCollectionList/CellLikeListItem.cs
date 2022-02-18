// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueBasics;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueDatabase;
using BlueDatabase.Enums;
using System.Drawing;

namespace BlueControls.ItemCollection {

    public class CellLikeListItem : BasicListItem {
        // Implements IReadableText
        //http://www.kurztutorial.info/programme/punkt-mm/rechner.html
        // Dim Ausgleich As Double = mmToPixel(1 / 72 * 25.4, 300)
        //   Dim FixZoom As Single = 3.07F

        #region Fields

        private readonly enBildTextVerhalten _bildTextverhalten;

        private readonly enShortenStyle _style;

        /// <summary>
        /// Nach welche Spalte sich der Stil richten muss.
        /// Wichtig, dass es ein Spalten-item ist, da bei neuen Datenbanken zwar die Spalte vorhnden ist, aber wenn keine Zeile Vorhanden ist, logischgerweise auch keine Zelle da ist.
        /// </summary>
        private readonly ColumnItem _StyleLikeThis;

        #endregion

        #region Constructors

        public CellLikeListItem(string internalAndReadableText, ColumnItem columnStyle, enShortenStyle style, bool enabled, enBildTextVerhalten bildTextverhalten) : base(internalAndReadableText) {
            _StyleLikeThis = columnStyle;
            _style = style;
            _Enabled = enabled;
            _bildTextverhalten = bildTextverhalten;
        }

        #endregion

        #region Properties

        public override string QuickInfo => Internal.CreateHtmlCodes(true);

        #endregion

        #region Methods

        // unveränderter Text
        public override object Clone() {
            var x = new CellLikeListItem(Internal, _StyleLikeThis, _style, _Enabled, _bildTextverhalten);
            x.CloneBasicStatesFrom(this);
            return x;
        }

        public override bool FilterMatch(string FilterText) {
            if (base.FilterMatch(FilterText)) { return true; }
            var txt = CellItem.ValueReadable(_StyleLikeThis, Internal, enShortenStyle.Both, _StyleLikeThis.BildTextVerhalten, true);
            return txt.ToUpper().Contains(FilterText.ToUpper());
        }

        public override int HeightForListBox(enBlueListBoxAppearance style, int columnWidth) => SizeUntouchedForListBox().Height;

        protected override Size ComputeSizeUntouchedForListBox() => Table.FormatedText_NeededSize(_StyleLikeThis, Internal, Skin.GetBlueFont(Parent.ItemDesign, enStates.Standard), _style, 16, _bildTextverhalten);

        protected override void DrawExplicit(Graphics GR, Rectangle PositionModified, enDesign itemdesign, enStates vState, bool DrawBorderAndBack, bool Translate) {
            if (DrawBorderAndBack) {
                Skin.Draw_Back(GR, itemdesign, vState, PositionModified, null, false);
            }
            Table.Draw_FormatedText(GR, Internal, _StyleLikeThis, PositionModified, itemdesign, vState, _style, _bildTextverhalten);
            if (DrawBorderAndBack) {
                Skin.Draw_Border(GR, itemdesign, vState, PositionModified);
            }
        }

        protected override string GetCompareKey() {
            // Die hauptklasse frägt nach diesem Kompare-Key
            //var txt = CellItem.ValueReadable(_StyleLikeThis, Internal, enShortenStyle.HTML, true); // Muss Kompakt sein, um Suffixe zu vermeiden
            var txt = CellItem.ValueReadable(_StyleLikeThis, Internal, enShortenStyle.HTML, _bildTextverhalten, true);

            return txt.CompareKey(_StyleLikeThis.SortType) + "|" + Internal;
            //Internal.CompareKey(_StyleLikeThis.SortType) + "|" + Internal;
        }

        #endregion
    }
}