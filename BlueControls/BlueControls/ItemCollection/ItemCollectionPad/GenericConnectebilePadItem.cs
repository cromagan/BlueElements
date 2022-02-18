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
using System.Drawing;

namespace BlueControls.ItemCollection {

    public class GenericConnectebilePadItem : FixedRectangleBitmapPadItem {

        #region Fields

        public readonly Size Size;
        public readonly string Text;

        #endregion

        #region Constructors

        //public static BlueFont Column_Filter_Font = BlueFont.Get(Column_Font.FontName, Column_Font.FontSize, false, false, false, false, true, Color.White, Color.Red, false, false, false);
        public GenericConnectebilePadItem(string intern, string text, Size s) : base(intern) {
            Text = text;
            Size = s;
        }

        #endregion

        #region Methods

        public override void DesignOrStyleChanged() => RemovePic();

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";

            t = t + "Text=" + Text.ToNonCritical() + ", ";
            t = t + "Size=" + Size.ToString() + ", ";

            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "GenericConnectebile";

        protected override Bitmap GeneratePic() {
            var bmp = new Bitmap(Size.Width, Size.Height);
            var gr = Graphics.FromImage(bmp);

            gr.Clear(Color.White);

            var Font = Skin.GetBlueFont(Stil, Parent.SheetStyle);

            if (Font != null) {
                Skin.Draw_FormatedText(gr, Text, null, enAlignment.Horizontal_Vertical_Center, new Rectangle(0, 0, Size.Width, Size.Height), null, false, Font, false);
                gr.DrawRectangle(Font.Pen(1), new Rectangle(0, 0, Size.Width, Size.Height));
            }

            return bmp;
        }

        #endregion
    }
}