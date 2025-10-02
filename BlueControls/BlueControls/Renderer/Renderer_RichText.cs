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
using BlueControls.Extended_Text;
using BlueTable.Enums;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.CellRenderer;

// ReSharper disable once UnusedMember.Global
public class Renderer_RichText : Renderer_Abstract {

    #region Properties

    public static string ClassId => "RichText";

    public override string Description => "Langsame aber korrekte Anzeige mit Formatierungen.";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, Rectangle scaleddrawarea, TranslationType translate, Alignment align, float scale) {
        if (string.IsNullOrEmpty(content)) { return; }

        //gr.SetClip(positionModified);
        //var trp = positionModified.PointOf(Alignment.Horizontal_Vertical_Center);
        //gr.TranslateTransform(trp.X, trp.Y);
        //gr.RotateTransform(-Drehwinkel);

        using var _txt = new ExtText(SheetStyle, Style) {
            HtmlText = content,
            //// da die Font 1:1 berechnet wird, aber bei der Ausgabe evtl. skaliert,
            //// muss etxt vorgegaukelt werden, daß der Drawberehich xxx% größer ist
            //etxt.DrawingArea = new Rectangle((int)UsedArea().Left, (int)UsedArea().Top, (int)(UsedArea().Width / AdditionalScale / SheetStyleScale), -1);
            //etxt.LineBreakWidth = etxt.DrawingArea.Width;
            TextDimensions = new Size((int)(scaleddrawarea.Width), -1),
            Ausrichtung = align,
            DrawingArea = scaleddrawarea,
            DrawingPos = scaleddrawarea.PointOf(Alignment.Top_Left)
        };

        _txt.Draw(gr, scale);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        return new List<GenericControl>();
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];

        return result;
    }

    public override bool ParseThis(string key, string value) {
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Formatierter Text";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Sonne);

    protected override Size CalculateContentSize(string content, TranslationType translate) {
        using var _etxt = new ExtText(SheetStyle, Style) {
            HtmlText = content,
            Multiline = true
        };

        _etxt.TextDimensions = new Size(400, -1);

        return _etxt.LastSize();
    }

    /// <summary>
    /// Gibt eine einzelne Zeile richtig ersetzt mit Prä- und Suffix zurück.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="style"></param>
    /// <param name="translate"></param>
    /// <returns></returns>
    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType translate) {
        return content;
    }

    #endregion
}