#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace BlueControls
{
    public sealed class BlueFont : IParseable, IReadableText
    {
        #region  Variablen-Deklarationen 

        private string _Code;

        private Font _Font;
        private Font _FontOL;

        private Color _Color_Main;
        private Pen _Pen;


        private float _WidthOf2Points;

        private SizeF[] _CharSize;


        private static readonly List<BlueFont> _FontsAll = new List<BlueFont>();


        private int _Zeilenabstand = -1;
        private float _Oberl‰ngex = -1;
        private float _Kapit‰lchenPlus = -1;


        private float FehlerSchwelle = float.MaxValue;
        private float TestesOK = float.MinValue;


        private QuickImage SymbolForReadableText_sym;
        private QuickImage NameInStyle_sym;
        private Bitmap SampleText_sym;
        private QuickImage SymbolOfLine_sym;

        #endregion


        #region  Event-Deklarationen + Delegaten 
        public event EventHandler Changed;
        #endregion


        #region  Construktor + Initialize 



        private void Initialize()
        {
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

            _Color_Main = Color.Red;
            Color_Outline = Color.Magenta;
            _Pen = null;
            Brush_Color_Main = null;
            Brush_Color_Outline = null;

            _WidthOf2Points = 0;
            _CharSize = new SizeF[0];

            _CharSize = new SizeF[256];
            for (var z = 0 ; z <= _CharSize.GetUpperBound(0) ; z++)
            {
                _CharSize[z] = new SizeF(-1, -1);
            }
        }

        private BlueFont(string Code)
        {
            Initialize();
            Parse(Code);
        }


        #endregion


        #region  Properties 

        public bool IsParsing { get; private set; }

        public Font Font(decimal Zoom)
        {

            if (Math.Abs(Zoom - 1) < 0.001m && SizeOK(_Font.Size)) { return _Font; }

            var GR = _FontOL.Size * (float)Zoom / Skin.Scale;


            if (SizeOK(GR))
            {
                return new Font(FontName, GR, _Font.Style, _Font.Unit);
            }

            return new Font("Arial", GR, _Font.Style, _Font.Unit);
        }
        public Font Font()
        {
            if (SizeOK(_Font.Size)) { return _Font; }
            return new Font("Arial", _FontOL.Size, _Font.Style, _Font.Unit);
        }


        public Font FontWithoutLines(float Zoom)
        {

            if (Math.Abs(Zoom - 1) < 0.001 && SizeOK(_FontOL.Size)) { return _FontOL; }

            var GR = _FontOL.Size * Zoom / Skin.Scale;


            if (SizeOK(GR))
            {
                return new Font(FontName, GR, _FontOL.Style, _FontOL.Unit);
            }

            return new Font("Arial", GR, _FontOL.Style, _FontOL.Unit);
        }
        public Font FontWithoutLinesForCapitals(float Zoom)
        {
            return new Font(_FontOL.Name, _FontOL.Size * Zoom * 0.8F / Skin.Scale, _FontOL.Style, _FontOL.Unit);
        }


        public bool Italic { get; private set; }

        public bool Bold { get; private set; }

        public bool Underline { get; private set; }

        public bool Outline { get; private set; }

        public bool Kapit‰lchen { get; private set; }

        public bool OnlyUpper { get; private set; }

        public bool OnlyLower { get; private set; }


        public bool StrikeOut { get; private set; }

        public float FontSize { get; private set; }

        public string FontName { get; private set; }


        public Pen Pen(float czoom)
        {
            if (Math.Abs(czoom - 1) < 0.001) { return _Pen; }
            return GeneratePen(czoom);
        }


        public Pen Pen(decimal czoom)
        {
            if (Math.Abs(czoom - 1) < 0.001m) { return _Pen; }
            return GeneratePen((float)czoom);
        }


        public Brush Brush_Color_Main { get; private set; }

        public Brush Brush_Color_Outline { get; private set; }

        public Color Color_Main
        {
            get
            {
                return _Color_Main;
            }
        }

        public Color Color_Outline { get; private set; }


        public SizeF CharSize(float DummyWidth)
        {
            return new SizeF(DummyWidth, _Zeilenabstand);
        }


        internal float Oberl‰nge(float Zoom)
        {
            return _Oberl‰ngex * Zoom;
        }



        internal float Kapit‰lchenPlus(float Zoom)
        {
            return _Kapit‰lchenPlus * Zoom;
        }




        internal SizeF CharSize(char vChar)
        {


            if (vChar <= _CharSize.GetUpperBound(0))
            {

                if (_CharSize[vChar].Height <= 0)
                {
                    _CharSize[vChar] = Compute_Size(vChar);
                }

                if (_CharSize[vChar].Width < 1 && vChar > 30)
                {
                    return _CharSize[vChar];
                }

                return _CharSize[vChar];
            }

            return Compute_Size(vChar);
        }


        #endregion


        private bool SizeOK(float S)
        {
            // Windwos mach seltsamerweiﬂe bei manchen Schriften einen Fehler. Seit dem neuen Firmen-Windows-Update vom 08.06.2015

            if (S <= TestesOK) { return true; }
            if (S >= FehlerSchwelle) { return false; }


            try
            {
                MeasureString("x", new Font(_Font.Name, S / Skin.Scale, _Font.Style, _Font.Unit));
                TestesOK = S;
                return true;

            }
            catch
            {
                FehlerSchwelle = S;
                return false;
            }
        }




        internal SizeF Compute_Size(char vChar)
        {

            if (vChar >= 0 && vChar <= 31) { return new SizeF(0, _Zeilenabstand); }
            if (vChar >= ExtChar.ImagesStart)
            {
                var BNR = QuickImage.Get((int)vChar - (int)ExtChar.ImagesStart);
                if (BNR == null) { return new SizeF(0, 0); }
                return new SizeF(BNR.BMP.Width + 1, BNR.BMP.Height + 1);
            }

            //if (vChar == ExtChar.StoreX || vChar == ExtChar.Top)
            //{
            //    Develop.DebugPrint(vChar);
            //    return new SizeF(0, _Zeilenabstand);
            //}

           // var c = Convert.ToChar(vChar).ToString();

            if (Kapit‰lchen && char.ToUpper(vChar) != vChar)
            {
                return new SizeF((MeasureString("." + char.ToUpper(vChar) + ".", _FontOL).Width - _WidthOf2Points) * 0.8F, _Zeilenabstand);
            }

            if (OnlyUpper)
            {
                return new SizeF(MeasureString("." + char.ToUpper(vChar) + ".", _FontOL).Width - _WidthOf2Points, _Zeilenabstand);
            }

            if (OnlyLower)
            {
                return new SizeF(MeasureString("." + char.ToLower(vChar) + ".", _FontOL).Width - _WidthOf2Points, _Zeilenabstand);
            }

            return new SizeF(MeasureString("." + vChar + ".", _FontOL).Width - _WidthOf2Points, _Zeilenabstand);
        }


        public void Parse(string ToParse)
        {
            IsParsing = true;
            Initialize();

            var ftst = FontStyle.Regular;
            var ftst2 = FontStyle.Regular;


            foreach (var pair in ToParse.GetAllTags())
            {
                switch (pair.Key)
                {
                    case "name":
                    case "fontname":
                        FontName = pair.Value;
                        break;

                    case "size":
                    case "fontsize":
                        FontSize = float.Parse(pair.Value);
                        if (FontSize < 0.1F) { Develop.DebugPrint(enFehlerArt.Fehler, "Fontsize=" + FontSize); }
                        break;

                    case "color":
                        _Color_Main = pair.Value.FromHTMLCode();
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
            IsParsing = false;


            _Font = new Font(FontName, FontSize / Skin.Scale, ftst);
            _FontOL = new Font(FontName, FontSize / Skin.Scale, ftst2);

            // Die Oberl‰nge immer berechnen, Symbole benˆtigen die exacte hˆhe
            var Multi = 50 / _FontOL.Size; // Zu groﬂe Schriften verursachen bei manchen Fonts Fehler!!!

            var tmpfont = new Font(_FontOL.Name, _FontOL.Size * Multi / Skin.Scale, _FontOL.Style);
            var f = BlueFont.MeasureString("Z", tmpfont);
            var bmp = new Bitmap((int)(f.Width + 1), (int)(f.Height + 1));
            var gr = Graphics.FromImage(bmp);

            for (var du = 0 ; du <= 1 ; du++)
            {
                gr.Clear(Color.White);
                if (du == 1)
                {
                    tmpfont = new Font(_FontOL.Name, _FontOL.Size * Multi * 0.8F / Skin.Scale, _FontOL.Style);
                }
                gr.DrawString("Z", tmpfont, Brushes.Black, 0, 0);

                var miny = (int)(f.Height / 2.0);

                //var tempVar = (int)(f.Width - 1);
                for (var x = 1 ; x <= (f.Width - 1) ; x++)
                {
                    for (var y = (int)(f.Height - 1) ; y >= miny ; y--)
                    {
                        if (y > miny && bmp.GetPixel(x, y).R == 0) { miny = y; }

                    }
                }

                if (du == 0)
                {
                    _Oberl‰ngex = miny / Multi;
                    if (!Kapit‰lchen) { break; }
                }
                else
                {
                    _Kapit‰lchenPlus = _Oberl‰ngex - miny / Multi;
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
            Brush_Color_Main = new SolidBrush(_Color_Main);
            Brush_Color_Outline = new SolidBrush(Color_Outline);


            _Code = ToString(FontName, FontSize, Bold, Italic, Underline, StrikeOut, Outline, _Color_Main.ToHTMLCode(), Color_Outline.ToHTMLCode(), Kapit‰lchen, OnlyUpper, OnlyLower);
        }

        internal BlueFont Scale(double fontScale)
        {
            if (Math.Abs(1 - fontScale) < 0.01) { return this; }
            return Get(FontName, (float)(FontSize * fontScale), Bold, Italic, Underline, StrikeOut, Outline, Color_Main, Color_Outline, Kapit‰lchen, OnlyUpper, OnlyLower);
        }

        private Pen GeneratePen(float cZoom)
        {

            var linDi = _Zeilenabstand / 10 * cZoom;

            if (Bold) { linDi = linDi * 1.5F; }

            return new Pen(_Color_Main, linDi);
        }


        public new string ToString()
        {
            return _Code;
        }


        private static string ToString(string FontName, float FontSize, bool Bold, bool Italic, bool Underline, bool Strikeout, bool OutLine, string Color_Main, string Color_Outline, bool vKapit‰lchen, bool vonlyuppe, bool vonlylower)
        {
            var c = "{Name=" + FontName + ", Size=" + FontSize;
            if (Bold) { c = c + ", Bold=True"; }
            if (Italic) { c = c + ", Italic=True"; }
            if (Underline) { c = c + ", Underline=True"; }
            if (Strikeout) { c = c + ", Strikeout=True"; }
            if (vKapit‰lchen) { c = c + ", Capitals=True"; }
            if (vonlyuppe) { c = c + ", OnlyUpper=True"; }
            if (vonlylower) { c = c + ", OnlyLower=True"; }
            if (OutLine) { c = c + ", Outline=True, OutlineColor=" + Color_Outline; }
            c = c + ", Color=" + Color_Main;
            return c + "}";
        }



        public static BlueFont Get(FontFamily vFont, float Size)
        {
            return Get(vFont.Name, Size, false, false, false, false, false, "000000", "FFFFFF", false, false, false);
        }


        public static BlueFont Get(string FontName, float FontSize, bool Bold, bool Italic, bool Underline, bool Strikeout, bool OutLine, string Color_Main, string Color_Outline, bool Kapit‰lchen, bool OnlyUpper, bool OnlyLower)
        {
            return Get(ToString(FontName, FontSize, Bold, Italic, Underline, Strikeout, OutLine, Color_Main, Color_Outline, Kapit‰lchen, OnlyUpper, OnlyLower));
        }

        public static BlueFont Get(string FontName, float FontSize, bool Bold, bool Italic, bool Underline, bool Strikeout, bool OutLine, Color Color_Main, Color Color_Outline, bool Kapit‰lchen, bool OnlyUpper, bool OnlyLower)
        {
            return Get(FontName, FontSize, Bold, Italic, Underline, Strikeout, OutLine, Color_Main.ToHTMLCode(), Color_Outline.ToHTMLCode(), Kapit‰lchen, OnlyUpper, OnlyLower);
        }


        public static BlueFont Get(string Code)
        {
            if (string.IsNullOrEmpty(Code)) { return null; }


            if (!Code.Contains("{")) { Code = "{Name=Arial, Size=10, Color=ff0000}"; }


            foreach (var Thisfont in _FontsAll)
            {
                if (Thisfont.ToString().ToUpper() == Code.ToUpper()) { return Thisfont; }
            }

            var f = new BlueFont(Code);
            _FontsAll.Add(f);

            if (f._Code.ToUpper() != Code.ToUpper())
            {
                Develop.DebugPrint("Schrift-Fehlerhaft: " + Code + " (" + f._Code + ")");
            }


            return f;
        }


        public string ReadableText()
        {
            var t = FontName + ", " + FontSize + " pt, ";


            if (Bold) { t = t + "B"; }
            if (Italic) { t = t + "I"; }
            if (Underline) { t = t + "U"; }
            if (StrikeOut) { t = t + "S"; }
            if (Kapit‰lchen) { t = t + "C"; }
            if (Outline) { t = t + "O"; }
            if (OnlyLower) { t = t + "L"; }
            if (OnlyUpper) { t = t + "U"; }
            return t.TrimEnd(", ");
        }


        private Bitmap Symbol(string Text, bool Transparent)
        {

            var s = BlueFont.MeasureString(Text, Font());
            var bmp = new Bitmap((int)(s.Width + 1), (int)(s.Height + 1));


            using (var gr = Graphics.FromImage(bmp))
            {
                if (Transparent)
                {
                    gr.Clear(Color.FromArgb(180, 180, 180));
                }
                else if (_Color_Main.GetBrightness() > 0.9F)
                {
                    gr.Clear(Color.FromArgb(200, 200, 200));
                }
                else
                {
                    gr.Clear(Color.White);
                }


                //End Using


                //If Transparent Then bmp.MakeTransparent(Color.White)


                //Using gr As Graphics = Graphics.FromImage(bmp)
                gr.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;


                //var etxt = new ExtText(enDesign.TextBox, enStates.Standard);
                //etxt.MaxHeight = 500;
                //etxt.MaxWidth = 500;
                //etxt.PlainText = Text;
                //etxt.Draw(gr, 1);
                Skin.Draw_FormatedText(gr, Text, null, enAlignment.Top_Left, new Rectangle(0, 0, 1000, 1000), null, false, this, false);

                // gr.DrawString("Text", Font(), Brush_Color_Main, 0, 0) ', System.Drawing.StringFormat.GenericTypographic)
            }


            if (Transparent)
            {
                bmp.MakeTransparent(Color.FromArgb(180, 180, 180));
            }

            return bmp;
        }


        public QuickImage SymbolForReadableText()
        {
            if (SymbolForReadableText_sym != null)
            {
                return SymbolForReadableText_sym;
            }
            QuickImage.Add("Font-" + ToString(), Symbol("Abc", false));
            SymbolForReadableText_sym = QuickImage.Get("Font-" + ToString());
            return SymbolForReadableText_sym;
        }


        public QuickImage NameInStyle()
        {
            if (NameInStyle_sym != null) { return NameInStyle_sym; }
            QuickImage.Add("FontName-" + ToString(), Symbol(Font().Name, true));
            NameInStyle_sym = QuickImage.Get("FontName-" + ToString());
            return NameInStyle_sym;
        }

        public Bitmap SampleText()
        {
            if (SampleText_sym != null) { return SampleText_sym; }
            SampleText_sym = Symbol("AaBbCcƒ‰.,?!", false);
            return SampleText_sym;
        }


        public QuickImage SymbolOfLine()
        {
            if (SymbolOfLine_sym != null) { return SymbolOfLine_sym; }

            var bmp = new Bitmap(32, 12);

            using (var gr = Graphics.FromImage(bmp))
            {

                if (_Color_Main.GetBrightness() > 0.9F)
                {
                    gr.Clear(Color.FromArgb(200, 200, 200));
                }
                else
                {
                    gr.Clear(Color.White);
                }

                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.DrawLine(Pen(1f), 3, 4, 29, 8);
            }

            QuickImage.Add("Line-" + ToString(), bmp);
            SymbolOfLine_sym = QuickImage.Get("Line-" + ToString());

            return SymbolOfLine_sym;
        }



        public void OnChanged()
        {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }


        public SizeF MeasureString(string s)
        {
            return MeasureString(s, _FontOL);
        }

        public static SizeF MeasureString(string s, Font f)
        {
            using (var g = Graphics.FromHwnd(IntPtr.Zero))
            {
                return g.MeasureString(s, f);
            }
        }




    }
}
