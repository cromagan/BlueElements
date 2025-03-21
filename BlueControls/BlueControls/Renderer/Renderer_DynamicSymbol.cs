// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueControls.ItemCollectionPad;
using BlueDatabase.Enums;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.CellRenderer;

// ReSharper disable once UnusedMember.Global
public class Renderer_DynamicSymbol : Renderer_Abstract {

    #region Fields

    public static readonly Renderer_DynamicSymbol Method = new();

    #endregion

    #region Properties

    public static string ClassId => "DynamicSymbol";

    public override string Description => "Kann spezielle Skripte als Symbol rendern";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, Rectangle scaleddrawarea, TranslationType translate, Alignment align, float scale) {
        if (string.IsNullOrEmpty(content)) { return; }

        if (scaleddrawarea is { Width: > 4, Height: > 4 }) {
            using var bmp = new Bitmap(scaleddrawarea.Width, scaleddrawarea.Height);

            var ok = DynamicSymbolPadItem.ExecuteScript(content, string.Empty, bmp);

            if (!ok.AllOk) {
                using var gr2 = Graphics.FromImage(bmp);
                gr2.DrawImage(QuickImage.Get(ImageCode.Warnung, 16), 0, 0);
            }

            gr.DrawImage(bmp, scaleddrawarea.X, scaleddrawarea.Y);
        }
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [];
        return result;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];

        //result.ParseableAdd("ShowPic", _bild_anzeigen);
        //result.ParseableAdd("ShowText", _text_anzeigen);
        //result.ParseableAdd("ShowCheckState", _checkstatus_anzeigen);
        return result;
    }

    public override bool ParseThis(string key, string value) => base.ParseThis(key, value);

    public override string ReadableText() => "Skript als Symbol anzeigen";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Stern);

    protected override Size CalculateContentSize(string content, TranslationType translate) =>
        //var font = Skin.DesignOf(design, state).BFont?.Font();

        //if (font == null) { return new Size(16, 32); }
        //var replacedText = ValueReadable(content, ShortenStyle.Replaced, translate);

        //return font.FormatedText_NeededSize(replacedText, QImage(content), 32);
        new Size(48, 48);

    /// <summary>
    /// Gibt eine einzelne Zeile richtig ersetzt mit Prä- und Suffix zurück.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="style"></param>
    /// <param name="translate"></param>
    /// <returns></returns>
    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType translate) => string.Empty;

    #endregion
}