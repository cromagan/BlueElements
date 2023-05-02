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
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;

namespace BlueControls.ItemCollection;

/// <summary>
/// Ein einfaches Item, das immer als Rechteck dargestellt wird
/// und einen Text enthalten kann.
/// </summary>
public class GenericPadItem : FixedRectangleBitmapPadItem {

    #region Fields

    private readonly string _text;

    #endregion

    #region Constructors

    public GenericPadItem(string intern, string text, Size s) : base(intern) {
        _text = text;
        Size = s;
    }

    public GenericPadItem(string intern) : this(intern, string.Empty, new Size(10, 10)) { }

    #endregion

    #region Properties

    public static string ClassId => "GenericPadItem";
    public override string Description => string.Empty;
    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public override void ProcessStyleChange() => RemovePic();

    public override string ToString() {
        var result = new List<string>();
        result.ParseableAdd("Text", _text.ToNonCritical());
        result.ParseableAdd("Size", Size);
        return result.Parseable(base.ToString());
    }

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

    #endregion

    //protected override BasicPadItem? TryCreate(string id, string name) {
    //    if (id.Equals(ClassId, StringComparison.OrdinalIgnoreCase)) {
    //        return new GenericPadItem(name);
    //    }
    //    return null;
    //}
}