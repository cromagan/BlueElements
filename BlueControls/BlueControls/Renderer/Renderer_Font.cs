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
using BlueTable.Enums;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.CellRenderer;

// ReSharper disable once UnusedMember.Global
public class Renderer_Font : Renderer_Abstract {

    #region Fields

    private const string txt = "ABCabc123ÄÖÜäöü@ß?";

    #endregion

    #region Properties

    public static string ClassId => "Font";

    public override string Description => "Stellt eine Schriftart dar.";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, Rectangle scaleddrawarea, TranslationType translate, Alignment align, float scale) {
        if (string.IsNullOrEmpty(content)) { return; }
        Skin.Draw_FormatedText(gr, txt, null, align, scaleddrawarea, BlueFont.Get(content).Scale(scale), false);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) => [];

    public override List<string> ParseableItems() => [];

    public override bool ParseThis(string key, string value) => base.ParseThis(key, value);

    public override string ReadableText() => "Schriftart darstellen";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Schriftart);

    protected override Size CalculateContentSize(string content, TranslationType translate) => BlueFont.Get(content).FormatedText_NeededSize(txt, null, 16);

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