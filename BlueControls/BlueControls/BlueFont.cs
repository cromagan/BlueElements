using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Enums;
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
        private float _Oberlängex = -1;
        private float _KapitälchenPlus = -1;


        private float FehlerSchwelle = float.MaxValue;
        private float TestesOK = float.MinValue;


        private Bitmap DummyBMP;
        private Graphics DummyGR;

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
            Kapitälchen = false;
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

            var GR = _FontOL.Size * (float)Zoom / clsSkin.Scale;


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

            var GR = _FontOL.Size * Zoom / clsSkin.Scale;


            if (SizeOK(GR))
            {
                return new Font(FontName, GR, _FontOL.Style, _FontOL.Unit);
            }

            return new Font("Arial", GR, _FontOL.Style, _FontOL.Unit);
        }
        public Font FontWithoutLinesForCapitals(float Zoom)
        {
            return new Font(_FontOL.Name, _FontOL.Size * Zoom * 0.8F / clsSkin.Scale, _FontOL.Style, _FontOL.Unit);
        }


        public bool Italic { get; private set; }

        public bool Bold { get; private set; }

        public bool Underline { get; private set; }

        public bool Outline { get; private set; }

        public bool Kapitälchen { get; private set; }

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


        internal float Oberlänge(float Zoom)
        {
            return _Oberlängex * Zoom;
        }



        internal float KapitälchenPlus(float Zoom)
        {
            return _KapitälchenPlus * Zoom;
        }




        internal SizeF CharSize(char vChar)
        {


            if ((int)vChar <= _CharSize.GetUpperBound(0))
            {

                if (_CharSize[(int)vChar].Height <= 0)
                {
                    _CharSize[(int)vChar] = Compute_Size(vChar);
                }
                return _CharSize[(int)vChar];
            }

            return Compute_Size(vChar);
        }


        #endregion


        private bool SizeOK(float S)
        {
            // Windwos mach seltsamerweiße bei manchen Schriften einen Fehler. Seit dem neuen Firmen-Windows-Update vom 08.06.2015

            if (S <= TestesOK) { return true; }
            if (S >= FehlerSchwelle) { return false; }


            try
            {
                DummyGraphics().DrawString("x", new Font(_Font.Name, S / clsSkin.Scale, _Font.Style, _Font.Unit), Brushes.Black, 0, 0);
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
                if (BNR==null) { return new SizeF(0, 0); }
                return new SizeF(BNR.BMP.Width + 1, BNR.BMP.Height + 1);
            }

            //if (vChar == ExtChar.StoreX || vChar == ExtChar.Top)
            //{
            //    Develop.DebugPrint(vChar);
            //    return new SizeF(0, _Zeilenabstand);
            //}

            var c = Convert.ToChar(vChar);

            if (Kapitälchen && char.ToUpper(c) != c)
            {
                return new SizeF((DummyGraphics().MeasureString("." + char.ToUpper(c) + ".", _FontOL).Width - _WidthOf2Points) * 0.8F, _Zeilenabstand);
            }

            if (OnlyUpper)
            {
                return new SizeF(DummyGraphics().MeasureString("." + char.ToUpper(c) + ".", _FontOL).Width - _WidthOf2Points, _Zeilenabstand);
            }

            if (OnlyLower)
            {
                return new SizeF(DummyGraphics().MeasureString("." + char.ToLower(c) + ".", _FontOL).Width - _WidthOf2Points, _Zeilenabstand);
            }

            return new SizeF(DummyGraphics().MeasureString("." + c + ".", _FontOL).Width - _WidthOf2Points, _Zeilenabstand);
        }


        public void Parse(string StringToParse)
        {
            IsParsing = true;
            Initialize();

            var ftst = FontStyle.Regular;
            var ftst2 = FontStyle.Regular;


            foreach (var pair in StringToParse.GetAllTags())
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
                        Kapitälchen = true;
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


            _Font = new Font(FontName, FontSize / clsSkin.Scale, ftst);
            _FontOL = new Font(FontName, FontSize / clsSkin.Scale, ftst2);

            // Die Oberlänge immer berechnen, Symbole benötigen die exacte höhe
            var Multi = 50 / _FontOL.Size; // Zu große Schriften verursachen bei manchen Fonts Fehler!!!

            var tmpfont = new Font(_FontOL.Name, _FontOL.Size * Multi / clsSkin.Scale, _FontOL.Style);
            var f = DummyGraphics().MeasureString("Z", tmpfont);
            var bmp = new Bitmap((int)(f.Width + 1), (int)(f.Height + 1));
            var gr = Graphics.FromImage(bmp);


            for (var du = 0 ; du <= 1 ; du++)
            {
                gr.Clear(Color.White);
                if (du == 1)
                {
                    tmpfont = new Font(_FontOL.Name, _FontOL.Size * Multi * 0.8F / clsSkin.Scale, _FontOL.Style);
                }
                gr.DrawString("Z", tmpfont, Brushes.Black, 0, 0);

                var miny = Convert.ToInt32(f.Height / 2.0);

                //var tempVar = (int)(f.Width - 1);
                for (var x = 1 ; x <= (int)(f.Width - 1) ; x++)
                {
                    for (var y = (int)(f.Height - 1) ; y >= miny ; y--)
                    {
                        if (y > miny && bmp.GetPixel(x, y).R == 0) { miny = y; }

                    }
                }

                if (du == 0)
                {
                    _Oberlängex = miny / Multi;
                    if (!Kapitälchen) { break; }
                }
                else
                {
                    _KapitälchenPlus = _Oberlängex - miny / Multi;
                }
            }

            bmp.Dispose();
            gr.Dispose();
            tmpfont.Dispose();

            _WidthOf2Points = DummyGraphics().MeasureString("..", _FontOL).Width;

            ///http://www.vb-helper.com/howto_net_rainbow_text.html
            ///https://msdn.microsoft.com/de-de/library/xwf9s90b(v=vs.90).aspx
            ///http://www.typo-info.de/schriftgrad.htm
            _Zeilenabstand = _FontOL.Height;

            _Pen = GeneratePen(1.0F);
            Brush_Color_Main = new SolidBrush(_Color_Main);
            Brush_Color_Outline = new SolidBrush(Color_Outline);


            _Code = ToString(FontName, FontSize, Bold, Italic, Underline, StrikeOut, Outline, _Color_Main.ToHTMLCode(), Color_Outline.ToHTMLCode(), Kapitälchen, OnlyUpper, OnlyLower);
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


        private static string ToString(string FontName, float FontSize, bool Bold, bool Italic, bool Underline, bool Strikeout, bool OutLine, string Color_Main, string Color_Outline, bool vKapitälchen, bool vonlyuppe, bool vonlylower)
        {
            var c = "{Name=" + FontName + ", Size=" + FontSize;
            if (Bold) { c = c + ", Bold=True"; }
            if (Italic) { c = c + ", Italic=True"; }
            if (Underline) { c = c + ", Underline=True"; }
            if (Strikeout) { c = c + ", Strikeout=True"; }
            if (vKapitälchen) { c = c + ", Capitals=True"; }
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


        public static BlueFont Get(string FontName, float FontSize, bool Bold, bool Italic, bool Underline, bool Strikeout, bool OutLine, string Color_Main, string Color_Outline, bool Kapitälchen, bool OnlyUpper, bool OnlyLower)
        {
            return Get(ToString(FontName, FontSize, Bold, Italic, Underline, Strikeout, OutLine, Color_Main, Color_Outline, Kapitälchen, OnlyUpper, OnlyLower));
        }

        public static BlueFont Get(string FontName, float FontSize, bool Bold, bool Italic, bool Underline, bool Strikeout, bool OutLine, Color Color_Main, Color Color_Outline, bool Kapitälchen, bool OnlyUpper, bool OnlyLower)
        {
            return Get(FontName, FontSize, Bold, Italic, Underline, Strikeout, OutLine, Color_Main.ToHTMLCode(), Color_Outline.ToHTMLCode(), Kapitälchen, OnlyUpper, OnlyLower);
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
            if (Kapitälchen) { t = t + "C"; }
            if (Outline) { t = t + "O"; }
            if (OnlyLower) { t = t + "L"; }
            if (OnlyUpper) { t = t + "U"; }
            return t.TrimEnd(", ");
        }


        private Bitmap Symbol(string Text, bool Transparent)
        {

            var s = DummyGraphics().MeasureString(Text, Font());
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


                var etxt = new ExtText(enDesign.TextBox, enStates.Standard);
                etxt.MaxHeight = 500;
                etxt.MaxWidth = 500;
                etxt.PlainText = Text;
                etxt.Draw(gr, 1);


                //  gr.DrawString("Text", Font(), Brush_Color_Main, 0, 0) ', System.Drawing.StringFormat.GenericTypographic)
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
            SampleText_sym = Symbol("AaBbCcÄä.,?!", false);
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



        internal Graphics DummyGraphics()
        {

            if (DummyGR == null)
            {
                DummyBMP = new Bitmap(1, 1);
                DummyGR = Graphics.FromImage(DummyBMP);
            }
            return DummyGR;

        }


        public void OnChanged()
        {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }


    }
}
