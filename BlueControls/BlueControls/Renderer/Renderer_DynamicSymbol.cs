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

using System.Collections.Generic;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.ItemCollectionPad;
using BlueDatabase.Enums;

namespace BlueControls.CellRenderer;

// ReSharper disable once UnusedMember.Global
public class Renderer_DynamicSymbol : Renderer_Abstract {

    #region Fields

    public static Renderer_DynamicSymbol Method = new();

    #endregion

    #region Constructors

    public Renderer_DynamicSymbol() : base() { }

    #endregion

    #region Properties

    public static string ClassId => "DynamicSymbol";

    public override string Description => "Kann spezielle Skripte als Symbol rendern";

    public override string MyClassId => ClassId;

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, Rectangle drawarea, Design design, States state, TranslationType translate, Alignment align, float scale) {
        if (string.IsNullOrEmpty(content)) { return; }


        if (drawarea.Width > 4 && drawarea.Height > 4) {
            using var bmp = new Bitmap(drawarea.Width, drawarea.Height);

            var ok = DynamicSymbolPadItem.ExecuteScript(content, string.Empty, bmp);

            if (!ok.AllOk) {
                using var gr2 = Graphics.FromImage(bmp);
                gr2.DrawImage(QuickImage.Get(ImageCode.Warnung, 16), 0, 0);
            }

        gr.DrawImage(bmp, drawarea.X, drawarea.Y);
        }

    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [];
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key.ToLower()) {
            //case "showpic":
            //    _bild_anzeigen = value.FromPlusMinus();
            //    return true;

            //case "showtext":
            //    _text_anzeigen = value.FromPlusMinus();
            //    return true;

            //case "showcheckstate":
            //    _checkstatus_anzeigen = value.FromPlusMinus();
            //    return true;
        }
        return true; // Immer true. So kann gefahrlos hin und her geschaltet werden und evtl. Werte aus anderen Renderen benutzt werden.
    }

    public override string ReadableText() => "Skript als Symbol anzeigen";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Textfeld2);

    public override string ToParseableString() {
        List<string> result = [];

        //result.ParseableAdd("ShowPic", _bild_anzeigen);
        //result.ParseableAdd("ShowText", _text_anzeigen);
        //result.ParseableAdd("ShowCheckState", _checkstatus_anzeigen);
        return result.Parseable(base.ToParseableString());
    }

    protected override Size CalculateContentSize(string content, Design design, States state, TranslationType translate) {
        //var font = Skin.DesignOf(design, state).BFont?.Font();

        //if (font == null) { return new Size(16, 32); }
        //var replacedText = ValueReadable(content, ShortenStyle.Replaced, translate);

        //return font.FormatedText_NeededSize(replacedText, QImage(content), 32);
        return new Size(48, 48);
    }

    /// <summary>
    /// Gibt eine einzelne Zeile richtig ersetzt mit Prä- und Suffix zurück.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="style"></param>
    /// <param name="translate"></param>
    /// <returns></returns>
    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType translate) {
        return string.Empty;
    }

    #endregion
}