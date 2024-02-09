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
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using static BlueBasics.Converter;
using static BlueBasics.Constants;
using static BlueBasics.Extensions;

namespace BlueControls;

public sealed class BlueFont : IReadableTextWithChanging, IHasKeyName, IParseable {

    #region Fields

    public static readonly BlueFont DefaultFont = Get("Arial", 8f, false, false, false, false, false, Color.Red, Color.Black, false, false, false);

    internal Brush? BrushColorMain;

    internal Brush? BrushColorOutline;

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
    private Pen _pen = new Pen(Brushes.Red);
    private Bitmap? _sampleTextSym;
    private float _sizeTestedAndFailed = float.MaxValue;
    private float _sizeTestedAndOk = float.MinValue;
    private QuickImage? _symbolForReadableTextSym;
    private QuickImage? _symbolOfLineSym;
    private float _widthOf2Points;
    private int _zeilenabstand = -1;

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Properties

    public string KeyName { get; private set; } = string.Empty;

    internal bool Bold { get; private set; }

    internal Color ColorMain { get; private set; } = Color.Black;

    internal Color ColorOutline { get; private set; } = Color.Magenta;

    internal string FontName { get; private set; } = "Arial";

    internal bool Italic { get; private set; }
    internal bool Kapitälchen { get; private set; }
    internal bool OnlyLower { get; private set; }
    internal bool OnlyUpper { get; private set; }
    internal bool Outline { get; private set; }
    internal float Size { get; private set; } = 9;
    internal bool StrikeOut { get; private set; }
    internal bool Underline { get; private set; }

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

    public static BlueFont Get(FontFamily font, float fontSize) => Get(font.Name, fontSize, false, false, false, false, false, "000000", "FFFFFF", false, false, false);

    public static BlueFont Get(string fontName, float fontSize, bool bold, bool italic, bool underline, bool strikeout, bool outLine, string colorMain, string colorOutline, bool kapitälchen, bool onlyUpper, bool onlyLower) => Get(ToString(fontName, fontSize, bold, italic, underline, strikeout, outLine, colorMain, colorOutline, kapitälchen, onlyUpper, onlyLower));

    public static BlueFont Get(string fontName, float fontSize, bool bold, bool italic, bool underline, bool strikeout, bool outLine, Color colorMain, Color colorOutline, bool kapitälchen, bool onlyUpper, bool onlyLower) => Get(fontName, fontSize, bold, italic, underline, strikeout, outLine, colorMain.ToHtmlCode(), colorOutline.ToHtmlCode(), kapitälchen, onlyUpper, onlyLower);

    public static BlueFont Get(string toParse) {
        if (string.IsNullOrEmpty(toParse) || !toParse.Contains("{")) { return DefaultFont; }

        if (FontsAll.Get(toParse.Replace(" ", string.Empty).ToUpper()) is BlueFont found) { return found; }

        var f = new BlueFont();

        toParse = toParse.Replace(",", ", "); // TODO: vor 01.10.2021 Entferen wenn inv bei den exports repariert wurde

        f.Parse(toParse);

        // Kann beim Initialisieren von anderen Fonts tatsächlih noch null sein
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        FontsAll?.Add(f);

        return f;
    }

    public static implicit operator Font(BlueFont font) => font._font; // font.SizeOk(font.Size) ? font._font : new Font("Arial", font._fontOl.Size, font._font.Style, font._font.Unit);

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

    public SizeF CharSize(float dummyWidth) => new(dummyWidth, _zeilenabstand);

    public void DrawString(Graphics gr, string text, float x, float y) => DrawString(gr, text, x, y, 1f, StringFormat.GenericDefault);

    public void DrawString(Graphics gr, string text, float x, float y, float zoom, StringFormat stringFormat) {
        var f = FontWithoutLines(zoom);

        var isCap = false;

        if (Kapitälchen && text != text.ToUpper()) {
            isCap = true;
            f = FontWithoutLinesForCapitals(zoom);
            text = text.ToUpper();
        } else if (OnlyUpper) {
            text = text.ToUpper();
        } else if (OnlyLower) {
            text = text.ToLower();
        }

        var si = SizeF.Empty;
        if (Underline || StrikeOut) {
            si = MeasureString(text, stringFormat);
        }

        if (Underline) {
            gr.DrawLine(Pen(zoom), x, (int)(y + Oberlänge(zoom) + ((Pen(1f).Width + 1) * zoom) + 0.5), x + ((1 + si.Width) * zoom), (int)(y + Oberlänge(zoom) + ((Pen(1f).Width + 1) * zoom) + 0.5));
        }

        if (isCap) {
            y += KapitälchenPlus(zoom);
        }

        if (Outline) {
            for (var px = -1; px <= 1; px++) {
                for (var py = -1; py <= 1; py++) {
                    DrawString(gr, text, f, BrushColorOutline, x + (px * zoom), y + (py * zoom), stringFormat);
                }
            }
        }

        if (isCap) {
            DrawString(gr, text, f, BrushColorMain, x + (0.3F * zoom), y, stringFormat);
        }

        DrawString(gr, text, f, BrushColorMain, x, y, stringFormat);

        if (StrikeOut) {
            gr.DrawLine(Pen(zoom), x - 1, (int)(y + (si.Height * 0.55)), (int)(x + 1 + si.Width), (int)(y + (si.Height * 0.55)));
        }

        //if (_Size.Width < 1) {
        //    GR.DrawLine(new Pen(Color.Red), DrawX + 1, DrawY - 4, DrawX + 1, DrawY + 16);
        //}
    }

    public Font Font(float zoom) {
        if (Math.Abs(zoom - 1) < DefaultTolerance && SizeOk(_font.Size)) { return _font; }

        var emSize = _fontOl.Size * zoom / Skin.Scale;
        return SizeOk(emSize) ? new Font(FontName, emSize, _font.Style, _font.Unit)
            : new Font("Arial", emSize, _font.Style, _font.Unit);
    }

    public Font FontWithoutLines(float zoom) {
        if (Math.Abs(zoom - 1) < DefaultTolerance && SizeOk(_fontOl.Size)) { return _fontOl; }
        var gr = _fontOl.Size * zoom / Skin.Scale;

        return SizeOk(gr) ? new Font(FontName, gr, _fontOl.Style, _fontOl.Unit)
            : new Font("Arial", gr, _fontOl.Style, _fontOl.Unit);
    }

    public Font FontWithoutLinesForCapitals(float zoom) =>
        new(_fontOl.Name, _fontOl.Size * zoom * 0.8F / Skin.Scale, _fontOl.Style, _fontOl.Unit);

    public SizeF MeasureString(string text) => _fontOl.MeasureString(text);

    public SizeF MeasureString(string text, StringFormat stringFormat) => _fontOl.MeasureString(text, stringFormat);

    public QuickImage? NameInStyle() {
        if (_nameInStyleSym != null) { return _nameInStyleSym; }

        var n = "FontName-" + ToString();
        if (!QuickImage.Exists(n)) { _ = new QuickImage(n, Symbol(FontName, true)); }

        _nameInStyleSym = QuickImage.Get(n);
        return _nameInStyleSym;
    }

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public void ParseFinished(string parsed) {
        KeyName = parsed.Replace(" ", string.Empty).ToUpper();

        _charSize = new SizeF[256];
        for (var z = 0; z <= _charSize.GetUpperBound(0); z++) {
            _charSize[z] = new SizeF(-1, -1);
        }

        var ftst = FontStyle.Regular;
        var ftst2 = FontStyle.Regular;

        if (Italic) {
            ftst |= FontStyle.Italic;
            ftst2 |= FontStyle.Italic;
        }

        if (Bold) {
            ftst |= FontStyle.Bold;
            ftst2 |= FontStyle.Bold;
        }

        if (Underline) {
            ftst |= FontStyle.Underline;
        }

        if (StrikeOut) {
            ftst |= FontStyle.Strikeout;
        }

        var s = Size / Skin.Scale;
        if (s < 0.1f) { s = 0.1f; }

        _font = new Font(FontName, s, ftst);
        _fontOl = new Font(FontName, s, ftst2);

        // Die Oberlänge immer berechnen, Symbole benötigen die exacte höhe
        var multi = 50 / _fontOl.Size; // Zu große Schriften verursachen bei manchen Fonts Fehler!!!
        Font tmpfont = new(_fontOl.Name, _fontOl.Size * multi / Skin.Scale, _fontOl.Style);
        var f = tmpfont.MeasureString("Z");
        Bitmap bmp = new((int)(f.Width + 1), (int)(f.Height + 1));
        var gr = Graphics.FromImage(bmp);
        for (var du = 0; du <= 1; du++) {
            gr.Clear(Color.White);
            if (du == 1) {
                tmpfont = new Font(_fontOl.Name, _fontOl.Size * multi * 0.8F / Skin.Scale, _fontOl.Style);
            }
            DrawString(gr, "Z", tmpfont, Brushes.Black, 0, 0);
            var miny = (int)(f.Height / 2.0);

            for (var x = 1; x <= f.Width - 1; x++) {
                for (var y = (int)(f.Height - 1); y >= miny; y--) {
                    if (y > miny && bmp.GetPixel(x, y).R == 0) { miny = y; }
                }
            }
            if (du == 0) {
                _oberlänge = miny / multi;
                if (!Kapitälchen) { break; }
            } else {
                _kapitälchenPlus = _oberlänge - (miny / multi);
            }
        }

        bmp.Dispose();
        gr.Dispose();
        tmpfont.Dispose();

        _widthOf2Points = MeasureString("..", StringFormat.GenericTypographic).Width;
        //http://www.vb-helper.com/howto_net_rainbow_text.html
        //https://msdn.microsoft.com/de-de/library/xwf9s90b(v=vs.90).aspx
        //http://www.typo-info.de/schriftgrad.htm
        _zeilenabstand = _fontOl.Height;
        _pen = GeneratePen(1.0F);
        BrushColorMain = new SolidBrush(ColorMain);
        BrushColorOutline = new SolidBrush(ColorOutline);
        //_code = ToString(FontName, FontSize, Bold, Italic, Underline, StrikeOut, Outline, ColorMain.ToHtmlCode(), ColorOutline.ToHtmlCode(), Kapitälchen, OnlyUpper, OnlyLower);
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
                return true;

            case "color":
                ColorMain = value.FromHtmlCode();
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

    public Pen Pen(float zoom) => Math.Abs(zoom - 1) < DefaultTolerance ? _pen : GeneratePen(zoom);

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

    public QuickImage? SymbolForReadableText() {
        if (_symbolForReadableTextSym != null) { return _symbolForReadableTextSym; }

        var n = "Font-" + ToString();
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

        var n = "Line-" + ToString();
        if (!QuickImage.Exists(n)) { _ = new QuickImage(n, bmp); }

        _symbolOfLineSym = QuickImage.Get(n);
        return _symbolOfLineSym;
    }

    public new string ToString() => ToString(FontName, Size, Bold, Italic, Underline, StrikeOut, Outline, ColorMain.ToHtmlCode(), ColorOutline.ToHtmlCode(), Kapitälchen, OnlyUpper, OnlyLower);

    internal SizeF CharSize(char c) {
        if (c <= _charSize.GetUpperBound(0)) {
            if (_charSize[c].Height <= 0) {
                _charSize[c] = Compute_Size(c);
            }
            return _charSize[c].Width < 1 && c > 30 ? Compute_Size(c) : _charSize[c];
        }
        return Compute_Size(c);
    }

    internal SizeF Compute_Size(char c) {
        if (c is >= (char)0 and <= (char)31) { return new SizeF(0, _zeilenabstand); }

        SizeF s;

        if (Kapitälchen && char.ToUpper(c) != c) {
            s = MeasureString("." + char.ToUpper(c) + ".", StringFormat.GenericTypographic);
            s = s with { Width = s.Width * 0.8f };
        } else if (OnlyUpper) {
            s = _fontOl.MeasureString("." + char.ToUpper(c) + ".", StringFormat.GenericTypographic);
        } else if (OnlyLower) {
            s = _fontOl.MeasureString("." + char.ToLower(c) + ".", StringFormat.GenericTypographic);
        } else {
            s = _fontOl.MeasureString("." + c + ".", StringFormat.GenericTypographic);
        }

        return new SizeF(s.Width - _widthOf2Points, _zeilenabstand);
    }

    internal float KapitälchenPlus(float zoom) => _kapitälchenPlus * zoom;

    internal float Oberlänge(float zoom) => _oberlänge * zoom;

    internal BlueFont Scale(double zoom) => Math.Abs(1 - zoom) < 0.01 ? this : Get(FontName, (float)(Size * zoom), Bold, Italic, Underline, StrikeOut, Outline, ColorMain, ColorOutline, Kapitälchen, OnlyUpper, OnlyLower);

    private static string ToString(string fontName, float fontSize, bool bold, bool italic, bool underline, bool strikeout, bool outLine, string colorMain, string colorOutline, bool capitals, bool onlyupper, bool onlylower) {
        var result = new List<string>();

        result.ParseableAdd("Name", fontName);
        result.ParseableAdd("Size", fontSize);
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
        return result.Parseable();
    }

    private Pen GeneratePen(float zoom) {
        var linDi = _zeilenabstand / 10 * zoom;
        if (Bold) { linDi *= 1.5F; }
        return new Pen(ColorMain, linDi);
    }

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