#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion

using BlueBasics;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueDatabase;
using BlueDatabase.Enums;
using System.Drawing;

namespace BlueControls.ItemCollection
{
    public class CellLikeListItem : BasicListItem
    {
        // Implements IReadableText

        //http://www.kurztutorial.info/programme/punkt-mm/rechner.html
        // Dim Ausgleich As Double = mmToPixel(1 / 72 * 25.4, 300)
        //   Dim FixZoom As Single = 3.07F

        #region  Variablen-Deklarationen 


        /// <summary>
        /// Nach welche Spalte sich der Stil richten muss.
        /// Wichtig, dass es ein Spalten-item ist, da bei neuen Datenbanken zwar die Spalte vorhnden ist, aber wenn keine Zeile Vorhanden ist, logischgerweise auch keine Zelle da ist.
        /// </summary>
        private readonly ColumnItem _StyleLikeThis;

        private readonly enShortenStyle _style;

        private readonly enBildTextVerhalten _bildTextverhalten;

        public override string QuickInfo
        {
            get
            {
                return Internal.ToHTMLText(); // unveränderter Text
            }
        }

        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 




        public CellLikeListItem(string internalAndReadableText, ColumnItem columnStyle, enShortenStyle style, bool enabled, enBildTextVerhalten bildTextverhalten) : base(internalAndReadableText)
        {
            _StyleLikeThis = columnStyle;
            _style = style;

            _Enabled = enabled;

            _bildTextverhalten = bildTextverhalten;


        }




        #endregion


        #region  Properties 

        #endregion



        public override Size SizeUntouchedForListBox()
        {
            return Table.FormatedText_NeededSize(_StyleLikeThis, Internal, Skin.GetBlueFont(Parent.ItemDesign, enStates.Standard), _style, 16, _bildTextverhalten);
        }


        protected override void DrawExplicit(Graphics GR, Rectangle PositionModified, enDesign itemdesign, enStates vState, bool DrawBorderAndBack, bool Translate)
        {

            if (DrawBorderAndBack)
            {
                Skin.Draw_Back(GR, itemdesign, vState, PositionModified, null, false);
            }

            Table.Draw_FormatedText(_StyleLikeThis, Internal, GR, PositionModified, false, _style, itemdesign, vState, _bildTextverhalten);

            if (DrawBorderAndBack)
            {
                Skin.Draw_Border(GR, itemdesign, vState, PositionModified);
            }
        }


        protected override string GetCompareKey()
        {
            // Die hauptklasse frägt nach diesem Kompare-Key
        //    var txt = CellItem.ValueReadable(_StyleLikeThis, Internal, enShortenStyle.HTML, true); // Muss Kompakt sein, um Suffixe zu vermeiden
            return DataFormat.CompareKey(Internal, _StyleLikeThis.Format) + "|" + Internal;
        }


        public override int HeightForListBox(enBlueListBoxAppearance style, int columnWidth)
        {
            return SizeUntouchedForListBox().Height;
        }


        public override BasicListItem CloneToNewCollection(ItemCollectionList newParent)
        {
            return CloneToNewCollection(newParent, new CellLikeListItem(Internal, _StyleLikeThis, _style, _Enabled, _bildTextverhalten));
        }

        public override bool FilterMatch(string FilterText)
        {
            if (base.FilterMatch(FilterText)) { return true; }
            var txt = CellItem.ValueReadable(_StyleLikeThis, Internal, enShortenStyle.Both, _StyleLikeThis.BildTextVerhalten);
            return txt.ToUpper().Contains(FilterText.ToUpper());
        }

    }
}