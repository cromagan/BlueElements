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
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueDatabase;
using System;
using System.Drawing;

namespace BlueControls.ItemCollection {

    public class ColumnPadItem : FixedRectangleBitmapPadItem {

        #region Fields

        public static BlueFont? CellFont = Skin.GetBlueFont(enDesign.Table_Cell, enStates.Standard);

        public static BlueFont? ChapterFont = Skin.GetBlueFont(enDesign.Table_Cell_Chapter, enStates.Standard);

        public static BlueFont? ColumnFont = Skin.GetBlueFont(enDesign.Table_Column, enStates.Standard);

        public readonly ColumnItem? Column;

        #endregion

        #region Constructors

        //public static BlueFont Column_Filter_Font = BlueFont.Get(Column_Font.FontName, Column_Font.FontSize, false, false, false, false, true, Color.White, Color.Red, false, false, false);
        public ColumnPadItem(ColumnItem? c) : base(c.Name) => Column = c;

        #endregion

        #region Methods

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            if (Column != null) {
                t = t + "Database=" + Column.Database.Filename.ToNonCritical() + ", ";
                t = t + "ColumnKey=" + Column.Key + ", ";
                t = t + "ColumnName=" + Column.Name.ToNonCritical() + ", ";
            }
            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "Column";

        protected override Bitmap GeneratePic() {
            if (Column == null) {
                return QuickImage.Get(enImageCode.Warnung, 128);
            }

            var wi = Table.TmpColumnContentWidth(null, Column, CellFont, 16);

            var bmp = new Bitmap(Math.Max((int)(wi * 0.7), 30), 300);
            var gr = Graphics.FromImage(bmp);

            gr.Clear(Column.BackColor);
            //gr.DrawString(Column.Caption, CellFont, )
            //Table.Draw_FormatedText(gr,)

            for (var z = 0; z < 3; z++) {
                var n = Column.Ueberschrift(z);
                if (!string.IsNullOrEmpty(n)) {
                    Skin.Draw_FormatedText(gr, n, null, enAlignment.Horizontal_Vertical_Center, new Rectangle(0, z * 16, bmp.Width, 61), null, false, ColumnFont, true);
                }
            }

            gr.TranslateTransform(bmp.Width / 2, 50);
            gr.RotateTransform(-90);
            Skin.Draw_FormatedText(gr, Column.Caption, null, enAlignment.VerticalCenter_Left, new Rectangle(-150, -150, 300, 300), null, false, ColumnFont, true);

            gr.TranslateTransform(-bmp.Width / 2, -50);
            gr.ResetTransform();

            gr.DrawLine(Pens.Black, 0, 210, bmp.Width, 210);

            var r = Column.Database.Row.First();
            if (r != null) {
                Table.Draw_FormatedText(gr, r.CellGetString(Column), Column, new Rectangle(0, 210, bmp.Width, 90), enDesign.Table_Cell, enStates.Standard, BlueDatabase.Enums.enShortenStyle.Replaced, Column.BildTextVerhalten);
            }

            return bmp;
        }

        #endregion
    }
}