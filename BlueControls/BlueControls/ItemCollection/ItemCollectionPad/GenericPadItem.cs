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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollection {
    /// <summary>
    /// Ein einfaches Item, das immer als Rechteck dargestellt wird
    /// und einen Text enthalten kann.
    /// </summary>

    public class GenericPadItem : FixedRectangleBitmapPadItem {

        #region Fields

        //public readonly Size Size;
        private readonly string _text;

        #endregion

        #region Constructors

        public GenericPadItem(string intern, string text, Size s) : base(intern) {
            _text = text;
            Size = s;
        }

        public GenericPadItem(string intern) : this(intern, string.Empty, new Size(10, 10)) { }

        #endregion

        #region Methods

        public override void ProcessStyleChange() => RemovePic();

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";

            t = t + "Text=" + _text.ToNonCritical() + ", ";
            t = t + "Size=" + Size + ", ";

            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "GenericPadItem";

        protected override void GeneratePic() {
            var bmp = new Bitmap(Size.Width, Size.Height);
            var gr = Graphics.FromImage(bmp);

            gr.Clear(Color.White);

            var font = Skin.GetBlueFont(Stil, Parent.SheetStyle);

            if (font == null) {
                GeneratedBitmap = bmp;
                return;
            }

            Skin.Draw_FormatedText(gr, _text, null, Alignment.Horizontal_Vertical_Center, new Rectangle(0, 0, Size.Width, Size.Height), null, false, font, false);
            gr.DrawRectangle(font.Pen(1), new Rectangle(0, 0, Size.Width, Size.Height));

            GeneratedBitmap = bmp;
        }

        protected override BasicPadItem? TryParse(string id, string name, List<KeyValuePair<string, string>> toParse) {
            if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
                var x = new GenericPadItem(name);
                x.Parse(toParse);
                return x;
            }
            return null;
        }

        #endregion
    }
}