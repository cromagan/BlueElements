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

using BlueControls.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static BlueBasics.Converter;

namespace BlueControls.Extended_Text;

public static class ExtTextClipboardHelper {

    #region Fields

    private const string ExtCharFormat = "BlueElements.ExtChar";

    #endregion

    #region Methods

    /// <summary>
    /// Prüft, ob das Clipboard ExtChar-Daten oder Text enthält
    /// </summary>
    public static bool CanPasteExtChars() {
        return Clipboard.ContainsData(ExtCharFormat) || Clipboard.ContainsText();
    }

    /// <summary>
    /// Kopiert ExtChar-Objekte ins Clipboard
    /// </summary>
    public static void CopyExtCharsToClipboard(List<ExtChar> chars) {
        if (chars == null || chars.Count == 0)
            return;

        var dataObject = new DataObject();

        // 1. Als ExtChar-Format (für interne Verwendung)
        var serializedData = SerializeExtChars(chars);
        dataObject.SetData(ExtCharFormat, serializedData);

        // 2. Als Plain Text (für externe Anwendungen)
        var plainText = string.Join("", chars.Select(c => c.PlainText()));
        dataObject.SetText(plainText);

        // 3. Als HTML (für Rich-Text-Anwendungen)
        var htmlText = ConvertExtCharsToHtml(chars);
        dataObject.SetData(DataFormats.Html, htmlText);

        Clipboard.SetDataObject(dataObject, true);
    }

    /// <summary>
    /// Fügt ExtChar-Objekte aus dem Clipboard ein
    /// </summary>
    public static List<ExtChar>? PasteExtCharsFromClipboard(ExtText parent) {
        if (!Clipboard.ContainsData(ExtCharFormat)) {
            // Fallback: Plain Text
            if (Clipboard.ContainsText()) {
                return CreateExtCharsFromText(parent, Clipboard.GetText());
            }
            return null;
        }

        var serializedData = Clipboard.GetData(ExtCharFormat) as string;
        if (string.IsNullOrEmpty(serializedData))
            return null;

        return DeserializeExtChars(parent, serializedData);
    }

    private static string ConvertExtCharsToHtml(List<ExtChar> chars) {
        var sb = new StringBuilder();
        sb.AppendLine("Version:0.9");
        sb.AppendLine("StartHTML:0000000000");
        sb.AppendLine("EndHTML:0000000000");
        sb.AppendLine("StartFragment:0000000000");
        sb.AppendLine("EndFragment:0000000000");
        sb.AppendLine("<html><body><!--StartFragment-->");

        foreach (var extChar in chars) {
            sb.Append(extChar.HtmlText());
        }

        sb.AppendLine("<!--EndFragment--></body></html>");
        return sb.ToString();
    }

    private static ExtChar? CreateExtCharFromData(ExtText parent, Dictionary<string, string> data) {
        if (!data.TryGetValue("Type", out var typeName) ||
            !data.TryGetValue("PlainText", out var plainText)) {
            return null;
        }

        // Style ermitteln
        var style = PadStyles.Standard;
        if (data.TryGetValue("Style", out var styleStr) &&
            int.TryParse(styleStr, out var styleInt)) {
            style = (PadStyles)styleInt;
        }

        // Font rekonstruieren
        BlueFont? font = null;
        if (data.TryGetValue("FontName", out var fontName)) {
            var fontSize = 12f;
            var fontBold = false;
            var fontItalic = false;
            var fontUnderline = false;
            var fontColor = System.Drawing.Color.Black;

            if (data.TryGetValue("FontSize", out var fontSizeStr)) {
                FloatTryParse(fontSizeStr, out fontSize);
            }
            if (data.TryGetValue("FontBold", out var fontBoldStr)) {
                bool.TryParse(fontBoldStr, out fontBold);
            }
            if (data.TryGetValue("FontItalic", out var fontItalicStr)) {
                bool.TryParse(fontItalicStr, out fontItalic);
            }
            if (data.TryGetValue("FontUnderline", out var fontUnderlineStr)) {
                bool.TryParse(fontUnderlineStr, out fontUnderline);
            }
            if (data.TryGetValue("FontColorMain", out var fontColorStr) &&
                int.TryParse(fontColorStr, out var colorArgb)) {
                fontColor = System.Drawing.Color.FromArgb(colorArgb);
            }

            font = BlueFont.Get(fontName, fontSize, fontBold, fontItalic, fontUnderline,
                              false, fontColor, System.Drawing.Color.Transparent,
                              false, false, false, System.Drawing.Color.Transparent);
        }

        // ExtChar basierend auf Typ erstellen
        return typeName switch {
            "ExtCharAscii" when plainText.Length > 0 =>
                new ExtCharAscii(parent, style, font ?? Skin.GetBlueFont(parent.SheetStyle, style), plainText[0]),
            "ExtCharCrlfCode" =>
                new ExtCharCrlfCode(parent, style, font ?? Skin.GetBlueFont(parent.SheetStyle, style)),
            "ExtCharTabCode" =>
                new ExtCharTabCode(parent, style, font ?? Skin.GetBlueFont(parent.SheetStyle, style)),
            "ExtCharImageCode" when data.TryGetValue("HtmlText", out var imageCode) =>
                new ExtCharImageCode(parent, style, font ?? Skin.GetBlueFont(parent.SheetStyle, style), imageCode),
            _ => plainText.Length > 0 ?
                new ExtCharAscii(parent, style, font ?? Skin.GetBlueFont(parent.SheetStyle, style), plainText[0]) : null
        };
    }

    private static List<ExtChar> CreateExtCharsFromText(ExtText parent, string text) {
        var result = new List<ExtChar>();
        var font = Skin.GetBlueFont(parent.SheetStyle, parent.StyleBeginns);

        foreach (var c in text) {
            if (c == '\r')
                continue; // Ignoriere \r

            if (c == '\n') {
                result.Add(new ExtCharCrlfCode(parent, parent.StyleBeginns, font));
            } else {
                result.Add(new ExtCharAscii(parent, parent.StyleBeginns, font, c));
            }
        }

        return result;
    }

    private static List<ExtChar> DeserializeExtChars(ExtText parent, string serializedData) {
        var result = new List<ExtChar>();
        var lines = serializedData.Split('\n');

        var currentCharData = new Dictionary<string, string>();

        foreach (var line in lines) {
            var trimmedLine = line.Trim();

            if (trimmedLine == "---" || string.IsNullOrEmpty(trimmedLine)) {
                // Ende eines Zeichens erreicht
                if (currentCharData.Count > 0) {
                    var extChar = CreateExtCharFromData(parent, currentCharData);
                    if (extChar != null) {
                        result.Add(extChar);
                    }
                    currentCharData.Clear();
                }
                continue;
            }

            var colonIndex = trimmedLine.IndexOf(':');
            if (colonIndex > 0) {
                var key = trimmedLine.Substring(0, colonIndex);
                var value = trimmedLine.Substring(colonIndex + 1);
                currentCharData[key] = value;
            }
        }

        // Letztes Zeichen verarbeiten (falls kein Trenner am Ende)
        if (currentCharData.Count > 0) {
            var extChar = CreateExtCharFromData(parent, currentCharData);
            if (extChar != null) {
                result.Add(extChar);
            }
        }

        return result;
    }

    private static string SerializeExtChars(List<ExtChar> chars) {
        var sb = new StringBuilder();

        foreach (var extChar in chars) {
            sb.AppendLine($"Type:{extChar.GetType().Name}");
            sb.AppendLine($"Style:{(int)extChar.Style}");
            sb.AppendLine($"HtmlText:{extChar.HtmlText()}");
            sb.AppendLine($"PlainText:{extChar.PlainText()}");

            // Font-Informationen
            if (extChar.Font != null) {
                sb.AppendLine($"FontName:{extChar.Font.FontName}");
                sb.AppendLine($"FontSize:{extChar.Font.Size}");
                sb.AppendLine($"FontBold:{extChar.Font.Bold}");
                sb.AppendLine($"FontItalic:{extChar.Font.Italic}");
                sb.AppendLine($"FontUnderline:{extChar.Font.Underline}");
                sb.AppendLine($"FontColorMain:{extChar.Font.ColorMain.ToArgb()}");
            }

            sb.AppendLine("---"); // Trenner zwischen Zeichen
        }

        return sb.ToString();
    }

    #endregion
}