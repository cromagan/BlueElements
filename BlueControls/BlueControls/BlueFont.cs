// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace BlueControls {

    public sealed class BlueFont : IReadableTextWithChanging {

        #region Fields

        private static readonly List<BlueFont> _FontsAll = new();
        private readonly SizeF[] _CharSize;
        private string _Code;
        private Font _Font;
        private Font _FontOL;
        private float _Kapit‰lchenPlus = -1;
        private float _Oberl‰nge = -1;
        private Pen _Pen;
        private float _WidthOf2Points;
        private int _Zeilenabstand = -1;
        private float FehlerSchwelle = float.MaxValue;
        private QuickImage NameInStyle_sym;
        private BitmapExt SampleText_sym;
        private QuickImage SymbolForReadableText_sym;
        private QuickImage SymbolOfLine_sym;
        private float TestesOK = float.MinValue;

        #endregion

        #region Constructors

        private BlueFont() {
            _Code = "";
            _Font = null;
            _FontOL = null;
            FontName = "Arial";
            FontSize = 9;
            Underline = false;
            StrikeOut = false;
            Italic = false;
            Bold = false;
            Outline = false;
            Kapit‰lchen = false;
            OnlyLower = false;
            OnlyUpper = false;
            Color_Main = Color.Black;
            Color_Outline = Color.Magenta;
            _Pen = null;
            Brush_Color_Main = null;
            Brush_Color_Outline = null;
            _WidthOf2Points = 0;
            _CharSize = new SizeF[0];
            _CharSize = new SizeF[256];
            for (var z = 0; z <= _CharSize.GetUpperBound(0); z++) {
                _CharSize[z] = new SizeF(-1, -1);
            }
        }

        private BlueFont(string codeToParse) : this() => Parse(codeToParse);

        #endregion

        #region Events

        public event EventHandler Changed;

        #endregion

        #region Properties

        public bool Bold { get; private set; }

        public Brush Brush_Color_Main { get; private set; }

        public Brush Brush_Color_Outline { get; private set; }

        public Color Color_Main { get; private set; }

        public Color Color_Outline { get; private set; }

        public string FontName { get; private set; }

        public float FontSize { get; private set; }

        public bool Italic { get; private set; }

        public bool Kapit‰lchen { get; private set; }

        public bool OnlyLower { get; private set; }

        public bool OnlyUpper { get; private set; }

        public bool Outline { get; private set; }

        public bool StrikeOut { get; private set; }

        public bool Underline { get; private set; }

        #endregion

        #region Methods

        public static void DrawString(Graphics gr, string text, Font font, Brush brush, float x, float y) {
            //http://csharphelper.com/blog/2014/09/understand-font-aliasing-issues-in-c/
            if (font.Size < 8) {
                gr.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            } else if (font.Size < 14) {
                gr.TextRenderingHint = TextRenderingHint.AntiAlias;
            } else {
                gr.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            }

            gr.DrawString(text, font, brush, x, y, StringFormat.GenericTypographic);
        }

        public static BlueFont Get(FontFamily font, float fontSize) => Get(font.Name, fontSize, false, false, false, false, false, "000000", "FFFFFF", false, false, false);

        public static BlueFont Get(string fontName, float fontSize, bool bold, bool Italic, bool Underline, bool Strikeout, bool OutLine, string Color_Main, string Color_Outline, bool Kapit‰lchen, bool OnlyUpper, bool OnlyLower) => Get(ToString(fontName, fontSize, bold, Italic, Underline, Strikeout, OutLine, Color_Main, Color_Outline, Kapit‰lchen, OnlyUpper, OnlyLower));

        public static BlueFont Get(string fontName, float fontSize, bool bold, bool Italic, bool Underline, bool Strikeout, bool OutLine, Color Color_Main, Color Color_Outline, bool Kapit‰lchen, bool OnlyUpper, bool OnlyLower) => Get(fontName, fontSize, bold, Italic, Underline, Strikeout, OutLine, Color_Main.ToHTMLCode(), Color_Outline.ToHTMLCode(), Kapit‰lchen, OnlyUpper, OnlyLower);

        public static BlueFont Get(string Code) {
            if (string.IsNullOrEmpty(Code)) { return null; }
            if (!Code.Contains("{")) { Code = "{Name=Arial, Size=10, Color=ff0000}"; }
            var searchcode = Code.ToUpper().Replace(" ", "");
            try {
                foreach (var Thisfont in _FontsAll) {
                    if (Thisfont.ToString().Replace(" ", "").ToUpper() == searchcode) { return Thisfont; }
                }
            } catch {
                return Get(Code);
            }
            BlueFont f = new(Code);
            _FontsAll.Add(f);
            if (f._Code.Replace(" ", "").ToUpper() != Code.Replace(" ", "").ToUpper()) {
                Develop.DebugPrint("Schrift-Fehlerhaft: " + Code + " (" + f._Code + ")");
            }
            return f;
        }

        public static SizeF MeasureString(string text, Font font) {
            using var g = Graphics.FromHwnd(IntPtr.Zero);
            return g.MeasureString(text, font);
        }

        public static SizeF MeasureStringOfCaption(string text) => MeasureString(text, Skin.GetBlueFont(enDesign.Caption, enStates.Standard).Font());

        public SizeF CharSize(float dummyWidth) => new(dummyWidth, _Zeilenabstand);

        public void DrawString(Graphics gr, string text, float x, float y) => DrawString(gr, text, x, y, 1f);

        public void DrawString(Graphics gr, string text, float x, float y, float zoom) {
            var f = FontWithoutLines(zoom);

            var IsCap = false;

            if (Kapit‰lchen && text != text.ToUpper()) {
                IsCap = true;
                f = FontWithoutLinesForCapitals(zoom);
                text = text.ToUpper();
            } else if (OnlyUpper) {
                text = text.ToUpper();
            } else if (OnlyLower) {
                text = text.ToLower();
            }

            var si = SizeF.Empty;
            if (Underline || StrikeOut) {
                si = MeasureString(text);
            }

            if (Underline) {
                gr.DrawLine(Pen(zoom), x, (int)(y + Oberl‰nge(zoom) + ((Pen(1f).Width + 1) * zoom) + 0.5), x + ((1 + si.Width) * zoom), (int)(y + Oberl‰nge(zoom) + ((Pen(1f).Width + 1) * zoom) + 0.5));
            }

            if (IsCap) {
                y += Kapit‰lchenPlus(zoom);
            }

            if (Outline) {
                for (var PX = -1; PX <= 1; PX++) {
                    for (var PY = -1; PY <= 1; PY++) {
                        BlueFont.DrawString(gr, text, f, Brush_Color_Outline, x + PX, y + PY);
                    }
                }
            }

            if (IsCap) {
                BlueFont.DrawString(gr, text, f, Brush_Color_Main, x + (0.3F * zoom), y);
            }

            BlueFont.DrawString(gr, text, f, Brush_Color_Main, x, y);

            if (StrikeOut) {
                gr.DrawLine(Pen(zoom), x - 1, (int)(y + (si.Height * 0.55)), (int)(x + 1 + si.Width), (int)(y + (si.Height * 0.55)));
            }

            //if (_Size.Width < 1) {
            //    GR.DrawLine(new Pen(Color.Red), DrawX + 1, DrawY - 4, DrawX + 1, DrawY + 16);
            //}
        }

        public Font Font(double zoom) {
            if (Math.Abs(zoom - 1) < 0.001d && SizeOK(_Font.Size)) { return _Font; }
            var GR = _FontOL.Size * (float)zoom / Skin.Scale;
            return SizeOK(GR) ? new Font(FontName, GR, _Font.Style, _Font.Unit) : new Font("Arial", GR, _Font.Style, _Font.Unit);
        }

        public Font Font() => SizeOK(_Font.Size) ? _Font : new Font("Arial", _FontOL.Size, _Font.Style, _Font.Unit);

        public Font FontWithoutLines(float zoom) {
            if (Math.Abs(zoom - 1) < 0.001 && SizeOK(_FontOL.Size)) { return _FontOL; }
            var GR = _FontOL.Size * zoom / Skin.Scale;
            return SizeOK(GR) ? new Font(FontName, GR, _FontOL.Style, _FontOL.Unit) : new Font("Arial", GR, _FontOL.Style, _FontOL.Unit);
        }

        public Font FontWithoutLinesForCapitals(float zoom) => new(_FontOL.Name, _FontOL.Size * zoom * 0.8F / Skin.Scale, _FontOL.Style, _FontOL.Unit);

        public SizeF MeasureString(string text) => MeasureString(text, _FontOL);

        public QuickImage NameInStyle() {
            if (NameInStyle_sym != null) { return NameInStyle_sym; }
            QuickImage.Add("FontName-" + ToString(), Symbol(Font().Name, true));
            NameInStyle_sym = QuickImage.Get("FontName-" + ToString());
            return NameInStyle_sym;
        }

        public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

        public Pen Pen(float zoom) => Math.Abs(zoom - 1) < 0.001 ? _Pen : GeneratePen(zoom);

        public Pen Pen(double zoom) => Math.Abs(zoom - 1) < 0.001d ? _Pen : GeneratePen((float)zoom);

        public string ReadableText() {
            var t = FontName + ", " + FontSize + " pt, ";
            if (Bold) { t += "B"; }
            if (Italic) { t += "I"; }
            if (Underline) { t += "U"; }
            if (StrikeOut) { t += "S"; }
            if (Kapit‰lchen) { t += "C"; }
            if (Outline) { t += "O"; }
            if (OnlyLower) { t += "L"; }
            if (OnlyUpper) { t += "U"; }
            return t.TrimEnd(", ");
        }

        public BitmapExt SampleText() {
            if (SampleText_sym != null) { return SampleText_sym; }
            SampleText_sym = Symbol("AaBbCcƒ‰.,?!", false);
            return SampleText_sym;
        }

        public QuickImage SymbolForReadableText() {
            if (SymbolForReadableText_sym != null) {
                return SymbolForReadableText_sym;
            }
            QuickImage.Add("Font-" + ToString(), Symbol("Abc", false));
            SymbolForReadableText_sym = QuickImage.Get("Font-" + ToString());
            return SymbolForReadableText_sym;
        }

        public QuickImage SymbolOfLine() {
            if (SymbolOfLine_sym != null) { return SymbolOfLine_sym; }
            BitmapExt bmp = new(32, 12);
            using (var gr = Graphics.FromImage(bmp.Bitmap)) {
                if (Color_Main.GetBrightness() > 0.9F) {
                    gr.Clear(Color.FromArgb(200, 200, 200));
                } else {
                    gr.Clear(Color.White);
                }
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.DrawLine(Pen(1f), 3, 4, 29, 8);
            }
            QuickImage.Add("Line-" + ToString(), bmp);
            SymbolOfLine_sym = QuickImage.Get("Line-" + ToString());
            return SymbolOfLine_sym;
        }

        public new string ToString() => _Code;

        internal SizeF CharSize(char vChar) {
            if (vChar <= _CharSize.GetUpperBound(0)) {
                if (_CharSize[vChar].Height <= 0) {
                    _CharSize[vChar] = Compute_Size(vChar);
                }
                return _CharSize[vChar].Width < 1 && vChar > 30 ? _CharSize[vChar] : _CharSize[vChar];
            }
            return Compute_Size(vChar);
        }

        internal SizeF Compute_Size(char vChar) {
            if (vChar is >= (char)0 and <= (char)31) { return new SizeF(0, _Zeilenabstand); }
            if (vChar >= (int)enASCIIKey.ImageStart) {
                var BNR = QuickImage.Get(vChar - (int)enASCIIKey.ImageStart);
                return BNR == null ? SizeF.Empty : new SizeF(BNR.BMP.Width + 1, BNR.BMP.Height + 1);
            }
            //if (vChar == ExtChar.StoreX || vChar == ExtChar.Top)
            //{
            //    Develop.DebugPrint(vChar);
            //    return new SizeF(0, _Zeilenabstand);
            //}
            // var c = Convert.ToChar(vChar).ToString();
            return Kapit‰lchen && char.ToUpper(vChar) != vChar ? new SizeF((MeasureString("." + char.ToUpper(vChar) + ".", _FontOL).Width - _WidthOf2Points) * 0.8F, _Zeilenabstand)
                                                               : OnlyUpper ? new SizeF(MeasureString("." + char.ToUpper(vChar) + ".", _FontOL).Width - _WidthOf2Points, _Zeilenabstand)
                                                               : OnlyLower ? new SizeF(MeasureString("." + char.ToLower(vChar) + ".", _FontOL).Width - _WidthOf2Points, _Zeilenabstand)
                                                               : new SizeF(MeasureString("." + vChar + ".", _FontOL).Width - _WidthOf2Points, _Zeilenabstand);
        }

        internal float Kapit‰lchenPlus(float zoom) => _Kapit‰lchenPlus * zoom;

        internal float Oberl‰nge(float zoom) => _Oberl‰nge * zoom;

        internal BlueFont Scale(double fontScale) => Math.Abs(1 - fontScale) < 0.01 ? this : Get(FontName, (float)(FontSize * fontScale), Bold, Italic, Underline, StrikeOut, Outline, Color_Main, Color_Outline, Kapit‰lchen, OnlyUpper, OnlyLower);

        internal List<string> SplitByWidth(string text, float maxWidth, int maxLines) {
            List<string> _broken = new();
            var pos = 0;
            var FoundCut = 0;
            var Rest = text;
            if (maxLines < 1) { maxLines = 100; }

            do {
                pos++;
                var ToTEst = Rest.Substring(0, pos);
                var s = MeasureString(ToTEst, Font());
                if (pos < Rest.Length && Convert.ToChar(Rest.Substring(pos, 1)).isPossibleLineBreak()) { FoundCut = pos; }
                if (s.Width > maxWidth || pos == Rest.Length) {
                    if (pos == Rest.Length) {
                        _broken.Add(Rest);
                        return _broken;
                    } // Alles untergebracht
                    if (_broken.Count == maxLines - 1) {
                        // Ok, werden zu viele Zeilen. Also diese K¸rzen.
                        _broken.Add(TrimByWidth(Rest, maxWidth));
                        return _broken;
                    }
                    if (FoundCut > 1) {
                        pos = FoundCut + 1;
                        ToTEst = Rest.Substring(0, pos);
                        FoundCut = 0;
                    }
                    _broken.Add(ToTEst);
                    Rest = Rest.Substring(pos);
                    pos = -1; // wird gleich erhˆht
                }
            } while (true);
        }

        internal string TrimByWidth(string txt, float maxWidth) {
            var tSize = MeasureString(txt, Font());
            if (tSize.Width - 1 > maxWidth && txt.Length > 1) {
                var Min = 0;
                var Max = txt.Length;
                int Middle;
                do {
                    Middle = (int)(Min + ((Max - Min) / 2.0));
                    tSize = MeasureString(txt.Substring(0, Middle) + "...", Font());
                    if (tSize.Width + 3 > maxWidth) {
                        Max = Middle;
                    } else {
                        Min = Middle;
                    }
                } while (Max - Min > 1);
                if (Middle == 1 && tSize.Width - 2 > maxWidth) {
                    return string.Empty;  // ACHTUNG: 5 Pixel breiter (Beachte oben +4 und hier +2)
                }
                return txt.Substring(0, Middle) + "...";
            }
            return txt;
        }

        private static string ToString(string FontName, float FontSize, bool Bold, bool Italic, bool Underline, bool Strikeout, bool OutLine, string Color_Main, string Color_Outline, bool vKapit‰lchen, bool vonlyuppe, bool vonlylower) {
            var c = "{Name=" + FontName + ", Size=" + FontSize.ToString().ToNonCritical();
            if (Bold) { c += ", Bold=True"; }
            if (Italic) { c += ", Italic=True"; }
            if (Underline) { c += ", Underline=True"; }
            if (Strikeout) { c += ", Strikeout=True"; }
            if (vKapit‰lchen) { c += ", Capitals=True"; }
            if (vonlyuppe) { c += ", OnlyUpper=True"; }
            if (vonlylower) { c += ", OnlyLower=True"; }
            if (OutLine) { c = c + ", Outline=True, OutlineColor=" + Color_Outline; }
            if (Color_Main != "000000") { c = c + ", Color=" + Color_Main; }
            return c + "}";
        }

        private Pen GeneratePen(float cZoom) {
            var linDi = _Zeilenabstand / 10 * cZoom;
            if (Bold) { linDi *= 1.5F; }
            return new Pen(Color_Main, linDi);
        }

        private void Parse(string ToParse) {
            var ftst = FontStyle.Regular;
            var ftst2 = FontStyle.Regular;
            ToParse = ToParse.Replace(",", ", "); // TODO: vor 01.10.2021 Entferen wenn inv bei den exports repariert wurde
            foreach (var pair in ToParse.GetAllTags()) {
                switch (pair.Key) {
                    case "name":

                    case "fontname":
                        FontName = pair.Value;
                        break;

                    case "size":

                    case "fontsize":
                        FontSize = float.Parse(pair.Value.FromNonCritical());
                        if (FontSize < 0.1F) { Develop.DebugPrint(enFehlerArt.Fehler, "Fontsize=" + FontSize); }
                        break;

                    case "color":
                        Color_Main = pair.Value.FromHTMLCode();
                        break;

                    case "italic":
                        ftst |= FontStyle.Italic;
                        ftst2 |= FontStyle.Italic;
                        Italic = true;
                        break;

                    case "bold":
                        ftst |= FontStyle.Bold;
                        ftst2 |= FontStyle.Bold;
                        Bold = true;
                        break;

                    case "underline":
                        ftst |= FontStyle.Underline;
                        Underline = true;
                        break;

                    case "capitals":
                        Kapit‰lchen = true;
                        break;

                    case "strikeout":
                        ftst |= FontStyle.Strikeout;
                        StrikeOut = true;
                        break;

                    case "outline":
                        Outline = true;
                        break;

                    case "outlinecolor":
                        Color_Outline = pair.Value.FromHTMLCode();
                        break;

                    case "onlylower":
                        OnlyLower = true;
                        break;

                    case "onlyupper":
                        OnlyUpper = true;
                        break;

                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
            _Font = new Font(FontName, FontSize / Skin.Scale, ftst);
            _FontOL = new Font(FontName, FontSize / Skin.Scale, ftst2);
            // Die Oberl‰nge immer berechnen, Symbole benˆtigen die exacte hˆhe
            var Multi = 50 / _FontOL.Size; // Zu groﬂe Schriften verursachen bei manchen Fonts Fehler!!!
            Font tmpfont = new(_FontOL.Name, _FontOL.Size * Multi / Skin.Scale, _FontOL.Style);
            var f = MeasureString("Z", tmpfont);
            Bitmap bmp = new((int)(f.Width + 1), (int)(f.Height + 1));
            var gr = Graphics.FromImage(bmp);
            for (var du = 0; du <= 1; du++) {
                gr.Clear(Color.White);
                if (du == 1) {
                    tmpfont = new Font(_FontOL.Name, _FontOL.Size * Multi * 0.8F / Skin.Scale, _FontOL.Style);
                }
                BlueFont.DrawString(gr, "Z", tmpfont, Brushes.Black, 0, 0);
                var miny = (int)(f.Height / 2.0);

                for (var x = 1; x <= (f.Width - 1); x++) {
                    for (var y = (int)(f.Height - 1); y >= miny; y--) {
                        if (y > miny && bmp.GetPixel(x, y).R == 0) { miny = y; }
                    }
                }
                if (du == 0) {
                    _Oberl‰nge = miny / Multi;
                    if (!Kapit‰lchen) { break; }
                } else {
                    _Kapit‰lchenPlus = _Oberl‰nge - (miny / Multi);
                }
            }

            bmp.Dispose();
            gr.Dispose();
            tmpfont.Dispose();
            _WidthOf2Points = MeasureString("..").Width;
            ///http://www.vb-helper.com/howto_net_rainbow_text.html
            ///https://msdn.microsoft.com/de-de/library/xwf9s90b(v=vs.90).aspx
            ///http://www.typo-info.de/schriftgrad.htm
            _Zeilenabstand = _FontOL.Height;
            _Pen = GeneratePen(1.0F);
            Brush_Color_Main = new SolidBrush(Color_Main);
            Brush_Color_Outline = new SolidBrush(Color_Outline);
            _Code = ToString(FontName, FontSize, Bold, Italic, Underline, StrikeOut, Outline, Color_Main.ToHTMLCode(), Color_Outline.ToHTMLCode(), Kapit‰lchen, OnlyUpper, OnlyLower);
        }

        private bool SizeOK(float S) {
            // Windwos mach seltsamerweiﬂe bei manchen Schriften einen Fehler. Seit dem neuen Firmen-Windows-Update vom 08.06.2015
            if (S <= TestesOK) { return true; }
            if (S >= FehlerSchwelle) { return false; }
            try {
                MeasureString("x", new Font(_Font.Name, S / Skin.Scale, _Font.Style, _Font.Unit));
                TestesOK = S;
                return true;
            } catch {
                FehlerSchwelle = S;
                return false;
            }
        }

        private BitmapExt Symbol(string Text, bool Transparent) {
            var s = MeasureString(Text, Font());
            BitmapExt bmp = new((int)(s.Width + 1), (int)(s.Height + 1));
            using (var gr = Graphics.FromImage(bmp.Bitmap)) {
                if (Transparent) {
                    gr.Clear(Color.FromArgb(180, 180, 180));
                } else if (Color_Main.GetBrightness() > 0.9F) {
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
                Skin.Draw_FormatedText(gr, Text, null, enAlignment.Top_Left, new Rectangle(0, 0, 1000, 1000), null, false, this, false);
                // BlueFont.DrawString(GR,"Text", Font(), Brush_Color_Main, 0, 0) ', System.Drawing.StringFormat.GenericTypographic)
            }
            if (Transparent) {
                bmp.Bitmap.MakeTransparent(Color.FromArgb(180, 180, 180));
            }
            return bmp;
        }

        #endregion
    }
}