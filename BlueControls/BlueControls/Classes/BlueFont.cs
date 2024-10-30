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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;

namespace BlueControls;

public sealed class BlueFont : IReadableTextWithPropertyChanging, IHasKeyName, IEditable, IParseable {

    #region Fields

    public static readonly BlueFont DefaultFont = Get("Arial", 8f, false, false, false, false, false, Color.Red, Color.Black, false, false, false, Color.Transparent);
    internal Brush BrushColorMain = Brushes.Red;
    internal Brush BrushColorOutline = Brushes.Red;
    private static readonly List<BlueFont> FontsAll = [];
    private SizeF[] _charSize = new SizeF[256];

    /// <summary>
    /// Die Schriftart, mit allen Attributen, die nativ unterstützt werden.
    /// </summary>
    private Font _font = new("Arial", 9);

    /// <summary>
    /// Die Schriftart, ohne den Stilen Strikeout und Underline
    /// </summary>
    private Font _fontOl = new("Arial", 9);

    private float _kapitälchenPlus = -1;
    private QuickImage? _nameInStyleSym;
    private float _oberlänge = -1;
    private Pen _pen = new(Brushes.Red);
    private Bitmap? _sampleTextSym;
    private float _sizeTestedAndFailed = float.MaxValue;
    private float _sizeTestedAndOk = float.MinValue;
    private QuickImage? _symbolForReadableTextSym;
    private QuickImage? _symbolOfLineSym;
    private float _widthOf2Points;
    private int _zeilenabstand = -1;

    #endregion

    #region Events

    public event EventHandler? PropertyChanged;

    #endregion

    #region Properties

    public string CaptionForEditor => "Schriftart";

    public float CharHeight => _zeilenabstand;
    public Color ColorBack { get; private set; } = Color.Transparent;
    public Color ColorMain { get; private set; } = Color.Black;
    public Color ColorOutline { get; private set; } = Color.Transparent;
    public Type? Editor { get; set; }
    public string FontName { get; private set; } = "Arial";

    public bool Italic { get; private set; }

    public bool Kapitälchen { get; private set; }

    public string KeyName { get; private set; } = string.Empty;

    public bool OnlyLower { get; private set; }

    public bool OnlyUpper { get; private set; }

    public bool Outline { get; private set; }

    public float Size { get; private set; } = 9;

    public bool StrikeOut { get; private set; }

    public bool Underline { get; private set; }

    internal bool Bold { get; private set; }

    #endregion

    #region Methods

    public static void DrawString(Graphics gr, string text, Font font, Brush brush, float x, float y) => DrawString(gr, text, font, brush, x, y, StringFormat.GenericDefault);

    public static void DrawString(Graphics gr, string text, Font font, Brush brush, float x, float y, StringFormat stringFormat) {
        try {
            lock (brush) {
                SetTextRenderingHint(gr, font);
                gr.DrawString(text, font, brush, x, y, stringFormat);
            }
        } catch {
            // Wird bereits an anderer Stelle verwendet... Multitasking, wenn mehrere items auf einmal generiert werden.
            Develop.CheckStackForOverflow();
            DrawString(gr, text, font, brush, x, y, stringFormat);
        }
    }

    public static BlueFont Get(FontFamily font, float fontSize) => Get(font.Name, fontSize, false, false, false, false, false, Color.Black, Color.Transparent, false, false, false, Color.Transparent);

    public static BlueFont Get(string fontName, float fontSize, bool bold, bool italic, bool underline, bool strikeout, bool outLine, string colorMain, string colorOutline, bool kapitälchen, bool onlyUpper, bool onlyLower, string colorBack) => Get(ToParseableString(fontName, fontSize, bold, italic, underline, strikeout, outLine, colorMain, colorOutline, kapitälchen, onlyUpper, onlyLower, colorBack).FinishParseable());

    public static BlueFont Get(string fontName, float fontSize, bool bold, bool italic, bool underline, bool strikeout, bool outLine, Color colorMain, Color colorOutline, bool kapitälchen, bool onlyUpper, bool onlyLower, Color colorBack) => Get(fontName, fontSize, bold, italic, underline, strikeout, outLine, colorMain.ToHtmlCode(), colorOutline.ToHtmlCode(), kapitälchen, onlyUpper, onlyLower, colorBack.ToHtmlCode());

    public static BlueFont Get(string toParse) {
        if (string.IsNullOrEmpty(toParse) || !toParse.Contains("{")) { return DefaultFont; }

        if (FontsAll.Get(toParse.Replace(" ", string.Empty).ToUpperInvariant()) is { } found) { return found; }

        toParse = toParse.Replace(",", ", "); // TODO: vor 01.10.2021 Entferen wenn inv bei den exports repariert wurde
        var f = new BlueFont();

        f.Parse(toParse);

        // Kann beim Initialisieren von anderen Fonts tatsächlih noch null sein
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        FontsAll?.Add(f);

        return f;
    }

    public static implicit operator Font(BlueFont font) => font._font;

    public static List<string> SplitByWidth(Font font, string text, float maxWidth, int maxLines) {
        List<string> broken = [];
        var pos = 0;
        var foundCut = 0;
        var rest = text;
        if (maxLines < 1) { maxLines = 100; }

        do {
            pos++;
            var toTEst = rest.Substring(0, pos);
            var s = font.MeasureString(toTEst);
            if (pos < rest.Length && Convert.ToChar(rest.Substring(pos, 1)).IsPossibleLineBreak()) { foundCut = pos; }
            if (s.Width > maxWidth || pos == rest.Length) {
                if (pos == rest.Length) {
                    broken.Add(rest);
                    return broken;
                } // Alles untergebracht
                if (broken.Count == maxLines - 1) {
                    // Ok, werden zu viele Zeilen. Also diese Kürzen.
                    broken.Add(TrimByWidth(font, rest, maxWidth));
                    return broken;
                }
                if (foundCut > 1) {
                    pos = foundCut + 1;
                    toTEst = rest.Substring(0, pos);
                    foundCut = 0;
                }
                broken.Add(toTEst);
                rest = rest.Substring(pos);
                pos = -1; // wird gleich erhöht
            }
        } while (true);
    }

    // font.SizeOk(font.Size) ? font._font : new Font("Arial", font._fontOl.Size, font._font.Style, font._font.Unit);
    public static string TrimByWidth(Font font, string txt, float maxWidth) {
        var tSize = font.MeasureString(txt);
        if (tSize.Width - 1 > maxWidth && txt.Length > 1) {
            var min = 0;
            var max = txt.Length;
            int middle;
            do {
                middle = (int)(min + ((max - min) / 2.0));
                tSize = font.MeasureString(txt.Substring(0, middle) + "...");
                if (tSize.Width + 3 > maxWidth) {
                    max = middle;
                } else {
                    min = middle;
                }
            } while (max - min > 1);
            if (middle == 1 && tSize.Width - 2 > maxWidth) {
                return string.Empty;  // ACHTUNG: 5 Pixel breiter (Beachte oben +4 und hier +2)
            }
            return txt.Substring(0, middle) + "...";
        }
        return txt;
    }

    public float CalculateLineWidth(float scale) {
        var baseWidth = (_zeilenabstand / 10f);
        return baseWidth * (Bold ? 1.5f : 1f) * scale;
    }

    public SizeF CharSize(float dummyWidth) => new(dummyWidth, _zeilenabstand);

    public void DrawString(Graphics gr, string text, float x, float y) => DrawString(gr, text, x, y, 1f, StringFormat.GenericDefault);

    public void DrawString(Graphics gr, string text, float x, float y, float scale, StringFormat stringFormat) {
        if (string.IsNullOrEmpty(text)) return;

        // Text-Transformationen in einem Schritt
        string transformedText = text;
        bool isCap = false;

        // Schnellere Variante der Text-Transformation mit weniger Vergleichen
        if (OnlyUpper || (Kapitälchen && text != text.ToUpperInvariant())) {
            transformedText = text.ToUpperInvariant();
            if (Kapitälchen) isCap = true;
        } else if (OnlyLower) {
            transformedText = text.ToLowerInvariant();
        }

        // Font nur einmal erstellen
        var f = isCap ? FontWithoutLinesForCapitals(scale) : FontWithoutLines(scale);

        // Größenmessung nur einmal durchführen wenn nötig

        SizeF si = SizeF.Empty;

        if (Underline || StrikeOut || !ColorBack.IsMagentaOrTransparent()) {
            si = (text.Length == 1) ? CharSize(text[0]) : MeasureString(transformedText, stringFormat);
        }

        // Hintergrund zeichnen
        if (!ColorBack.IsMagentaOrTransparent()) {
            using var brush = new SolidBrush(ColorBack);
            gr.FillRectangle(brush, x, y, si.Width, si.Height);
        }

        float actualY = y;
        if (isCap) actualY += KapitälchenPlus(scale);

        // Outline in einem separaten GraphicsPath für bessere Performance
        if (Outline) {
            using var path = new GraphicsPath();
            for (var px = -1; px <= 1; px++) {
                for (var py = -1; py <= 1; py++) {
                    DrawString(gr, transformedText, f, BrushColorOutline,
                        x + (px * scale), actualY + (py * scale), stringFormat);
                }
            }
        }

        // Haupttext zeichnen
        if (isCap) {
            DrawString(gr, transformedText, f, BrushColorMain, x + (0.3F * scale), actualY, stringFormat);
        }
        DrawString(gr, transformedText, f, BrushColorMain, x, actualY, stringFormat);

        // Linien in einem Batch zeichnen
        if (Underline || StrikeOut) {
            var lineWidth = CalculateLineWidth(scale);
            using var scaledPen = new Pen(ColorMain, lineWidth);

            if (Underline) {
                float underlineY = y + Oberlänge(scale) + lineWidth + scale + 0.5f;
                gr.DrawLine(scaledPen, x, (int)underlineY, x + ((1 + si.Width) * scale), (int)underlineY);
            }
            if (StrikeOut) {
                float strikeY = y + (si.Height * 0.55f);
                gr.DrawLine(scaledPen, x - 1, (int)strikeY, (int)(x + 1 + si.Width), (int)strikeY);
            }
        }
    }

    public Font Font(float scale) {
        if (Math.Abs(scale - 1) < DefaultTolerance && SizeOk(_font.Size)) { return _font; }

        var emSize = _fontOl.Size * scale / Skin.Scale;
        return SizeOk(emSize) ? new Font(FontName, emSize, _font.Style, _font.Unit)
            : new Font("Arial", emSize, _font.Style, _font.Unit);
    }

    public Font FontWithoutLines(float scale) {
        if (Math.Abs(scale - 1) < DefaultTolerance && SizeOk(_fontOl.Size)) { return _fontOl; }
        var gr = _fontOl.Size * scale / Skin.Scale;

        return SizeOk(gr) ? new Font(FontName, gr, _fontOl.Style, _fontOl.Unit)
            : new Font("Arial", gr, _fontOl.Style, _fontOl.Unit);
    }

    public Font FontWithoutLinesForCapitals(float scale) =>
        new(_fontOl.Name, _fontOl.Size * scale * 0.8F / Skin.Scale, _fontOl.Style, _fontOl.Unit);

    public Size FormatedText_NeededSize(string text, QuickImage? qi, int minSize) {
        try {
            var pSize = SizeF.Empty;
            var tSize = SizeF.Empty;

            if (qi != null) {
                lock (qi) {
                    pSize = ((Bitmap)qi).Size;
                }
            }

            if (!string.IsNullOrEmpty(text)) { tSize = _font.MeasureString(text); }

            if (!string.IsNullOrEmpty(text)) {
                if (qi == null) {
                    return new Size((int)(tSize.Width + 1), Math.Max((int)tSize.Height, minSize));
                }

                return new Size((int)(tSize.Width + 2 + pSize.Width + 1), Math.Max((int)tSize.Height, (int)pSize.Height));
            }

            if (qi != null) { return new Size((int)pSize.Width, (int)pSize.Height); }

            return new Size(minSize, minSize);
        } catch {
            // tmpImageCode wird an anderer Stelle verwendet
            Develop.CheckStackForOverflow();
            return FormatedText_NeededSize(text, qi, minSize);
        }
    }

    public SizeF MeasureString(string text) => _fontOl.MeasureString(text);

    public SizeF MeasureString(string text, StringFormat stringFormat) => _fontOl.MeasureString(text, stringFormat);

    public QuickImage? NameInStyle() {
        if (_nameInStyleSym != null) { return _nameInStyleSym; }

        var n = "FontName-" + ParseableItems().FinishParseable();
        if (!QuickImage.Exists(n)) { _ = new QuickImage(n, Symbol(FontName, true)); }

        _nameInStyleSym = QuickImage.Get(n);
        return _nameInStyleSym;
    }

    public void OnPropertyChanged() => PropertyChanged?.Invoke(this, System.EventArgs.Empty);

    public List<string> ParseableItems() => ToParseableString(FontName, Size, Bold, Italic, Underline, StrikeOut, Outline, ColorMain.ToHtmlCode(), ColorOutline.ToHtmlCode(), Kapitälchen, OnlyUpper, OnlyLower, ColorBack.ToHtmlCode());

    public void ParseFinished(string parsed) {
        // StringBuilder ist bei vielen Replace-Operationen schneller,
        // bei einer einzelnen Operation aber langsamer
        KeyName = parsed.Replace(" ", string.Empty).ToUpperInvariant();

        // Schnellere Array-Initialisierung
        _charSize = new SizeF[256];
        var defaultSize = new SizeF(-1, -1);
        for (var i = 0; i < _charSize.Length; i++) {
            _charSize[i] = defaultSize;
        }

        // Flags direkt in einer Operation setzen
        var ftst = FontStyle.Regular |
                   (Italic ? FontStyle.Italic : 0) |
                   (Bold ? FontStyle.Bold : 0) |
                   (Underline ? FontStyle.Underline : 0) |
                   (StrikeOut ? FontStyle.Strikeout : 0);

        var ftst2 = FontStyle.Regular |
                    (Italic ? FontStyle.Italic : 0) |
                    (Bold ? FontStyle.Bold : 0);

        var s = Math.Max(Size / Skin.Scale, 0.1f);

        _font = new Font(FontName, s, ftst);
        _fontOl = new Font(FontName, s, ftst2);

        // Oberlängenberechnung
        var multi = 50 / _fontOl.Size;
        using var tmpfont = new Font(_fontOl.Name, _fontOl.Size * multi / Skin.Scale, _fontOl.Style);
        var f = tmpfont.MeasureString("Z");

        using var bmp = new Bitmap((int)(f.Width + 1), (int)(f.Height + 1));
        using var gr = Graphics.FromImage(bmp);

        for (var du = 0; du <= 1; du++) {
            gr.Clear(Color.White);

            using var currentFont = du == 1
                ? new Font(_fontOl.Name, _fontOl.Size * multi * 0.8F / Skin.Scale, _fontOl.Style)
                : tmpfont;

            DrawString(gr, "Z", currentFont, Brushes.Black, 0, 0);
            var miny = (int)(f.Height / 2.0);

            // Schnellere Pixelmanipulation durch LockBits
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);

            unsafe {
                var ptr = (byte*)bmpData.Scan0;
                var stride = bmpData.Stride;

                for (var x = 1; x < f.Width - 1; x++) {
                    for (var y = (int)(f.Height - 1); y >= miny; y--) {
                        // Bei 32bpp ist jedes Pixel 4 Bytes
                        var pixel = ptr[y * stride + x * 4];
                        if (y > miny && pixel == 0) {
                            miny = y;
                            break;
                        }
                    }
                }
            }

            bmp.UnlockBits(bmpData);

            if (du == 0) {
                _oberlänge = miny / multi;
                if (!Kapitälchen) break;
            } else {
                _kapitälchenPlus = _oberlänge - (miny / multi);
            }
        }

        _widthOf2Points = MeasureString("..", StringFormat.GenericTypographic).Width;
        //http://www.vb-helper.com/howto_net_rainbow_text.html
        //https://msdn.microsoft.com/de-de/library/xwf9s90b(v=vs.90).aspx
        //http://www.typo-info.de/schriftgrad.htm
        _zeilenabstand = _fontOl.Height;
        _pen = GeneratePen(1.0F);
        BrushColorMain = new SolidBrush(ColorMain);
        BrushColorOutline = new SolidBrush(ColorOutline);
    }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "name":
            case "fontname":
                FontName = value;
                return true;

            case "size":
            case "fontsize":
                Size = FloatParse(value.FromNonCritical());
                if (Size < 0.1f) { Size = 0.1f; }
                Size = (float)Math.Round((double)Size, 3, MidpointRounding.AwayFromZero);
                return true;

            case "color":
                ColorMain = value.FromHtmlCode();
                return true;

            case "backcolor":
                ColorBack = value.FromHtmlCode();
                return true;

            case "italic":
                Italic = true;
                return true;

            case "bold":
                Bold = true;
                return true;

            case "underline":
                Underline = true;
                return true;

            case "capitals":
                Kapitälchen = true;
                return true;

            case "strikeout":
                StrikeOut = true;
                return true;

            case "outline":
                Outline = true;
                return true;

            case "outlinecolor":
                ColorOutline = value.FromHtmlCode();
                return true;

            case "onlylower":
                OnlyLower = true;
                return true;

            case "onlyupper":
                OnlyUpper = true;
                return true;
        }

        return false;
    }

    public Pen Pen(float addtionalScale) => Math.Abs(addtionalScale - 1) < DefaultTolerance ? _pen : GeneratePen(addtionalScale);

    public string ReadableText() {
        var t = FontName + ", " + Size + " pt, ";
        if (Bold) { t += "B"; }
        if (Italic) { t += "I"; }
        if (Underline) { t += "U"; }
        if (StrikeOut) { t += "S"; }
        if (Kapitälchen) { t += "C"; }
        if (Outline) { t += "O"; }
        if (OnlyLower) { t += "L"; }
        if (OnlyUpper) { t += "U"; }
        return t.TrimEnd(", ");
    }

    public Bitmap? SampleText() {
        if (_sampleTextSym != null) { return _sampleTextSym; }
        _sampleTextSym = Symbol("AaBbCcÄä.,?!", false);
        return _sampleTextSym;
    }

    public BlueFont Scale(float scale) => Math.Abs(1 - scale) < DefaultTolerance ? this : Get(FontName, Size * scale, Bold, Italic, Underline, StrikeOut, Outline, ColorMain, ColorOutline, Kapitälchen, OnlyUpper, OnlyLower, ColorBack);

    public QuickImage? SymbolForReadableText() {
        if (_symbolForReadableTextSym != null) { return _symbolForReadableTextSym; }

        var n = "Font-" + ParseableItems().FinishParseable();
        if (!QuickImage.Exists(n)) { _ = new QuickImage(n, Symbol("Abc", false)); }

        _symbolForReadableTextSym = QuickImage.Get(n);
        return _symbolForReadableTextSym;
    }

    public QuickImage? SymbolOfLine() {
        if (_symbolOfLineSym != null) { return _symbolOfLineSym; }

        Bitmap bmp = new(32, 12);
        using (var gr = Graphics.FromImage(bmp)) {
            gr.Clear(ColorMain.GetBrightness() > 0.9F ? Color.FromArgb(200, 200, 200) : Color.White);
            gr.SmoothingMode = SmoothingMode.HighQuality;
            gr.DrawLine(Pen(1f), 3, 4, 29, 8);
        }

        var n = "Line-" + ParseableItems().FinishParseable();
        if (!QuickImage.Exists(n)) { _ = new QuickImage(n, bmp); }

        _symbolOfLineSym = QuickImage.Get(n);
        return _symbolOfLineSym;
    }

    internal SizeF CharSize(char c) {
        // Direkter Zugriff auf vorberechnete Größe wenn möglich
        if (c <= _charSize.GetUpperBound(0)) {
            ref SizeF size = ref _charSize[c];
            return (size.Height <= 0 || (size.Width < 1 && c > 30))
                ? (size = Compute_Size(c))
                : size;
        }
        return Compute_Size(c);
    }

    internal float KapitälchenPlus(float scale) => _kapitälchenPlus * scale;

    internal float Oberlänge(float scale) => _oberlänge * scale;

    private static List<string> ToParseableString(string fontName, float fontSize, bool bold, bool italic, bool underline, bool strikeout, bool outLine, string colorMain, string colorOutline, bool capitals, bool onlyupper, bool onlylower, string colorBack) {
        List<string> result = [];

        result.ParseableAdd("Name", fontName);
        result.ParseableAdd("Size", Math.Round(fontSize, 3, MidpointRounding.AwayFromZero));
        if (bold) { result.ParseableAdd("Bold", bold); }
        if (italic) { result.ParseableAdd("Italic", italic); }
        if (underline) { result.ParseableAdd("Underline", underline); }
        if (strikeout) { result.ParseableAdd("Strikeout", strikeout); }
        if (capitals) { result.ParseableAdd("Capitals", capitals); }
        if (onlyupper) { result.ParseableAdd("OnlyUpper", onlyupper); }
        if (onlylower) { result.ParseableAdd("OnlyLower", onlylower); }
        if (outLine) {
            result.ParseableAdd("Outline", outLine);
            result.ParseableAdd("OutlineColor", colorOutline);
        }
        if (colorMain != "000000") { result.ParseableAdd("Color", colorMain); }
        if (colorBack.ToLowerInvariant() != "00ffffff") { result.ParseableAdd("BackColor", colorBack); }
        return result;
    }

    private SizeF Compute_Size(char c) {
        if (c <= 31) {
            return new SizeF(0, _zeilenabstand);
        }

        // Zeichentransformation nur einmal durchführen
        char transformedChar = c;
        bool isSmallCaps = false;

        if (OnlyUpper || (Kapitälchen && char.IsLower(c))) {
            transformedChar = char.ToUpper(c);
            isSmallCaps = Kapitälchen && c != transformedChar;
        } else if (OnlyLower) {
            transformedChar = char.ToLower(c);
        }

        var s = _fontOl.MeasureString($".{transformedChar}.", StringFormat.GenericTypographic);

        return new SizeF(
            (isSmallCaps ? (s.Width * 0.8f) : s.Width) - _widthOf2Points,
            _zeilenabstand
        );
    }

    private Pen GeneratePen(float additionalScale) => new Pen(ColorMain, CalculateLineWidth(additionalScale));

    private bool SizeOk(float sizeToCheck) {
        // Windwows macht seltsamerweiße bei manchen Schriften einen Fehler. Seit dem neuen Firmen-Windows-Update vom 08.06.2015
        if (sizeToCheck <= _sizeTestedAndOk) { return true; }
        if (sizeToCheck >= _sizeTestedAndFailed) { return false; }
        try {
            _ = new Font(_font.Name, sizeToCheck / Skin.Scale, _font.Style, _font.Unit).MeasureString("x");
            _sizeTestedAndOk = sizeToCheck;
            return true;
        } catch {
            _sizeTestedAndFailed = sizeToCheck;
            return false;
        }
    }

    private Bitmap Symbol(string text, bool transparent) {
        var s = MeasureString(text);
        Bitmap bmp = new((int)(s.Width + 1), (int)(s.Height + 1));
        using (var gr = Graphics.FromImage(bmp)) {
            if (transparent) {
                gr.Clear(Color.FromArgb(180, 180, 180));
            } else if (ColorMain.GetBrightness() > 0.9F) {
                gr.Clear(Color.FromArgb(200, 200, 200));
            } else {
                gr.Clear(Color.White);
            }
            //End Using
            //If Transparent Then bmp.MakeTransparent(Color.White)
            //Using gr As Graphics = Graphics.FromImage(bmp)
            //gr.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            //var etxt = new ExtText(enDesign.TextBox, enStates.Standard);
            //etxt.MaxHeight = 500;
            //etxt.MaxWidth = 500;
            //etxt.PlainText = Text;
            //etxt.Draw(gr, 1);
            Skin.Draw_FormatedText(gr, text, null, Alignment.Top_Left, new Rectangle(0, 0, 1000, 1000), null, false, this, false);
            // BlueFont.DrawString(GR,"Text",(Font)this, Brush_Color_Main, 0, 0) ', System.Drawing.StringFormat.GenericTypographic)
        }
        if (transparent) {
            bmp.MakeTransparent(Color.FromArgb(180, 180, 180));
        }
        return bmp;
    }

    #endregion
}