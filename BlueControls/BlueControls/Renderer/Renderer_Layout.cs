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
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad;
using BlueTable;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BlueControls.CellRenderer;

// ReSharper disable once UnusedMember.Global
public class Renderer_Layout : Renderer_Abstract {

    #region Fields

    private string _file = string.Empty;

    #endregion

    #region Properties

    public static string ClassId => "Layout";

    public override string Description => "Langsame, aber schöne Anzeige eines Layoutes";

    public string File {
        get => _file;
        set {
            if (_file == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _file = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, RowItem? affectingRow, Rectangle scaleddrawarea, TranslationType translate, Alignment align, float scale) {
        if (affectingRow == null) { return; }

        using var l = new ItemCollectionPadItem(_file);
        l.ForPrinting = true;
        l.GridShow = 0;

        if (!l.Any()) {
            var replacedText = ValueReadable("Layout nicht gefunden oder fehlerhaft.", ShortenStyle.Replaced, translate);

            Skin.Draw_FormatedText(gr, replacedText, null, align, scaleddrawarea, this.GetFont(scale), false);
            return;
        }

        _ = l.ResetVariables();
        var scx = l.ReplaceVariables(affectingRow);

        if (scx.Failed) {
            var replacedText = ValueReadable("Layout Generierung fehlgeschlagen.", ShortenStyle.Replaced, translate);
            Skin.Draw_FormatedText(gr, replacedText, null, align, scaleddrawarea, this.GetFont(scale), false);
            return;
        }

        var bmp = l.ToBitmap(scale);

        if (bmp == null) {
            var replacedText = ValueReadable("Bild Erstellung fehlgeschlagen.", ShortenStyle.Replaced, translate);
            Skin.Draw_FormatedText(gr, replacedText, null, align, scaleddrawarea, this.GetFont(scale), false);
            return;
        }

        try {
            float scale2 = Math.Min((float)scaleddrawarea.Width / bmp.Width, (float)scaleddrawarea.Height / bmp.Height);
            gr.DrawImage(bmp, new Rectangle(scaleddrawarea.X + (scaleddrawarea.Width - (int)(bmp.Width * scale2)) / 2, scaleddrawarea.Y + (scaleddrawarea.Height - (int)(bmp.Height * scale2)) / 2, (int)(bmp.Width * scale2), (int)(bmp.Height * scale2)));
        } catch {
            var replacedText = ValueReadable("Anzeige fehlgeschlagen.", ShortenStyle.Replaced, translate);
        }
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [   new FlexiControlForProperty<string>(() => File)
        ];
        return result;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("LayoutFile", _file);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key.ToLower()) {
            case "layoutfile":
                _file = value.FromNonCritical();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Layout-Anzeige";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Layout);

    protected override Size CalculateContentSize(string content, TranslationType translate) {
        using var l = new ItemCollectionPadItem(_file);
        l.ForPrinting = true;

        if (!l.Any()) {
            return new Size(16, 16);
        }

        return new Size((int)l.UsedArea.Width, (int)l.UsedArea.Height);
    }

    /// <summary>
    /// Gibt eine einzelne Zeile richtig ersetzt mit Prä- und Suffix zurück.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="style"></param>
    /// <param name="translate"></param>
    /// <returns></returns>
    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType translate) => LanguageTool.PrepaireText(content, style, string.Empty, string.Empty, translate, null);

    #endregion
}