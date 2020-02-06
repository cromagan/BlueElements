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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using BlueBasics.Enums;
using BlueBasics.Interfaces;

namespace BlueBasics
{
    public sealed class QuickImage : IParseable, IReadableText, ICompareKey
    {
        private static string _SearchedCode = "";
        private static int _FoundInde = -1;

        #region  Variablen-Deklarationen 

        private string _name;

        private enImageCodeEffect _effekt;

        private int _width;
        private int _height;

        private string _färbung;
        private string _changeGreenTo;

        private int _helligkeit;
        private int _sättigung;
        private int _transparenz;

        private string _Zweitsymbol;

        private int _drehWinkel;


        private Bitmap _Bitmap;


        private string TMPCode = "";


        private bool _IsError;

        #endregion


        #region  Event-Deklarationen + Delegaten 
        public event EventHandler Changed;
        #endregion


        #region  Construktor + Initialize 





        public QuickImage(string ImageCode)
        {
            Parse(ImageCode);
        }

        private void Initialize()
        {
            _name = string.Empty;
            _effekt = enImageCodeEffect.Ohne;
            _width = -1;
            _height = -1;
            _färbung = string.Empty;
            _changeGreenTo = string.Empty;
            _helligkeit = 100;
            _sättigung = 100;
            _drehWinkel = 0;
            _transparenz = 0;
            _Zweitsymbol = string.Empty;
        }

        #endregion


        #region  Properties 

        public Bitmap BMP
        {
            get
            {
                if (_Bitmap == null) { Generate(); }
                return _Bitmap;
            }
        }


        public bool IsError
        {
            get
            {
                if (_Bitmap == null) { Generate(); }
                return _IsError;
            }
        }

        public bool IsParsing { get; private set; }

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
                OnChanged();
            }
        }

        public enImageCodeEffect Effekt
        {
            get
            {
                return _effekt;
            }

            set
            {
                _effekt = value;
                OnChanged();
            }
        }

        public int Width
        {
            get
            {
                return _width;
            }

            set
            {
                _width = value;
                OnChanged();
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }

            set
            {
                _height = value;
                OnChanged();
            }
        }

        public string Färbung
        {
            get
            {
                return _färbung;
            }

            set
            {
                _färbung = value;
                OnChanged();
            }
        }

        public string ChangeGreenTo
        {
            get
            {
                return _changeGreenTo;
            }

            set
            {
                _changeGreenTo = value;
                OnChanged();
            }
        }

        public int Helligkeit
        {
            get
            {
                return _helligkeit;
            }

            set
            {
                _helligkeit = value;
                OnChanged();
            }
        }

        public int Sättigung
        {
            get
            {
                return _sättigung;
            }

            set
            {
                _sättigung = value;
                OnChanged();
            }
        }

        public int Transparenz
        {
            get
            {
                return _transparenz;
            }

            set
            {
                _transparenz = value;
                OnChanged();
            }
        }

        public string Zweitsymbol
        {
            get
            {
                return _Zweitsymbol;
            }

            set
            {
                _Zweitsymbol = value;
                OnChanged();
            }
        }

        public int DrehWinkel
        {
            get
            {
                return _drehWinkel;
            }

            set
            {
                _drehWinkel = value;
                OnChanged();
            }
        }


        #endregion


        #region  Shares 

        private static readonly List<QuickImage> PC = new List<QuickImage>();




        public static QuickImage Get(Bitmap Image)
        {
            if (Image == null) { return null; }

            foreach (var ThisQuickImage in PC)
            {
                if (ThisQuickImage != null) { return ThisQuickImage; }
            }

            return null;
        }


        public static QuickImage Get(QuickImage ImageCode, enImageCodeEffect AdditionalState)
        {
            if (AdditionalState == enImageCodeEffect.Ohne) { return ImageCode; }
            return Get(GenerateCode(ImageCode.Name, ImageCode.Width, ImageCode.Height, ImageCode.Effekt | AdditionalState, ImageCode.Färbung, ImageCode.ChangeGreenTo, ImageCode.Sättigung, ImageCode.Helligkeit, ImageCode.DrehWinkel, ImageCode.Transparenz, ImageCode.Zweitsymbol));
        }

        public static QuickImage Get(string ImageCode)
        {
            if (string.IsNullOrEmpty(ImageCode)) { return null; }

            var z = GetIndex(ImageCode);
            if (z >= 0) { return PC[z]; }



            var l = new QuickImage(ImageCode);
            if (l.ToString() != ImageCode)
            {
                Develop.DebugPrint("Fehlerhafter Imagecode: " + ImageCode + " -> " + l);
                z = GetIndex(l.ToString());
                if (z >= 0) { return PC[z]; }
            }


            PC.Add(l);

            return l;
        }

        public static QuickImage Get(string Image, int SquareWidth)
        {
            if (string.IsNullOrEmpty(Image)) { return null; }


            if (Image.Contains("|"))
            {
                var w = (Image + "||||||").Split('|');
                w[1] = SquareWidth.ToString();
                w[2] = string.Empty;
                return Get(w.JoinWith("|"));
            }

            return Get(GenerateCode(Image, SquareWidth, 0, enImageCodeEffect.Ohne, "", "", 100, 100, 0, 0, string.Empty));
        }




        public static QuickImage Get(enImageCode Image)
        {
            return Get(Image, 16);
        }

        public static QuickImage Get(enImageCode Image, int SquareWidth)
        {
            if (Image == enImageCode.None) { return null; }
            return Get(GenerateCode(Enum.GetName(Image.GetType(), Image), SquareWidth, 0, enImageCodeEffect.Ohne, "", "", 100, 100, 0, 0, string.Empty));
        }

        public static QuickImage Get(enImageCode Image, int SquareWidth, string Färbung, string ChangeGreenTo)
        {
            if (Image == enImageCode.None) { return null; }
            return Get(GenerateCode(Enum.GetName(Image.GetType(), Image), SquareWidth, 0, enImageCodeEffect.Ohne, Färbung, ChangeGreenTo, 100, 100, 0, 0, string.Empty));
        }

        public static QuickImage Get(int Index)
        {
            if (Index >= 0 && Index < PC.Count) { return PC[Index]; }
            return null;
        }

        public static QuickImage Get(enFileFormat File, int Size)
        {
            switch (File)
            {
                case enFileFormat.WordKind:
                    return Get(enImageCode.Word, Size);
                case enFileFormat.ExcelKind:
                    return Get(enImageCode.Excel, Size);
                case enFileFormat.PowerPointKind:
                    return Get(enImageCode.PowerPoint, Size);
                case enFileFormat.Textdocument:
                    return Get(enImageCode.Textdatei, Size);
                case enFileFormat.EMail:
                    return Get(enImageCode.Brief, Size);
                case enFileFormat.Pdf:
                    return Get(enImageCode.PDF, Size);
                case enFileFormat.HTML:
                    return Get(enImageCode.Globus, Size);
                case enFileFormat.Image:
                    return Get(enImageCode.Bild, Size);
                case enFileFormat.CompressedArchive:
                    return Get(enImageCode.Karton, Size);
                case enFileFormat.Movie:
                    return Get(enImageCode.Filmrolle, Size);
                case enFileFormat.Executable:
                    return Get(enImageCode.Anwendung, Size);
                case enFileFormat.HelpFile:
                    return Get(enImageCode.Frage, Size);
                case enFileFormat.Database:
                    return Get(enImageCode.Skript, Size);
                case enFileFormat.XMLFile:
                    return Get(enImageCode.XML, Size);
                case enFileFormat.Visitenkarte:
                    return Get(enImageCode.Visitenkarte, Size);
                case enFileFormat.Sound:
                    return Get(enImageCode.Note, Size);
                case enFileFormat.Unknown:
                    return Get(enImageCode.Datei, Size);
                case enFileFormat.ProgrammingCode:
                    return Get(enImageCode.Summe, Size);
                case enFileFormat.Link:
                    return Get(enImageCode.Undo, Size);
                default:
                    Develop.DebugPrint(File);
                    return Get(enImageCode.Datei, Size);
            }
        }


        public static int GetIndex(QuickImage Img)
        {
            return PC.IndexOf(Img);
        }


        public static int GetIndex(string Code)
        {
            if (string.IsNullOrEmpty(Code)) { return -1; }

            if (_SearchedCode == Code) { return _FoundInde; }

            for (var z = 0 ; z < PC.Count ; z++)
            {
                if (Code == PC[z].ToString())
                {
                    _SearchedCode = Code;
                    _FoundInde = z;
                    return z;
                }

            }
            return -1;
        }

        public static string GenerateCode(string Name, int Width, int Height, enImageCodeEffect Effekt, string Färbung, string ChangeGreenTo, int Sättigung, int Helligkeit, int Drehwinkel, int Transparenz, string Zweitsymbol)
        {

            var C = Name + "|";
            if (Width > 0) { C = C + Width; }
            C = C + "|";
            if (Height > 0 && Width != Height) { C = C + Height; }
            C = C + "|";
            if (Effekt != enImageCodeEffect.Ohne) { C = C + (int)Effekt; }
            C = C + "|";
            C = C + Färbung;
            C = C + "|";
            C = C + ChangeGreenTo;
            C = C + "|";
            if (Helligkeit != 100) { C = C + Helligkeit; }
            C = C + "|";
            if (Sättigung != 100) { C = C + Sättigung; }
            C = C + "|";
            if (Drehwinkel > 0) { C = C + Drehwinkel; }
            C = C + "|";
            if (Transparenz > 0) { C = C + Transparenz; }
            C = C + "|";
            if (!string.IsNullOrEmpty(Zweitsymbol)) { C = C + Zweitsymbol; }
            return C.TrimEnd('|');
        }


        public static void Add(string Name, Bitmap BMP)
        {

            if (string.IsNullOrEmpty(Name)) { return; }
            if (BMP == null) { return; }


            var z = GetIndex(Name);
            if (z < 0)
            {
                var l = new QuickImage(Name);
                PC.Add(l);
                z = GetIndex(Name);
            }


            PC[z]._Bitmap = BMP;
        }


        #endregion



        public override string ToString()
        {

            if (string.IsNullOrEmpty(TMPCode))
            {
                TMPCode = GenerateCode(_name, _width, _height, _effekt, _färbung, _changeGreenTo, _sättigung, _helligkeit, _drehWinkel, _transparenz, _Zweitsymbol);
            }

            return TMPCode;
        }


        public Bitmap GetBitmap(string TMPName)
        {
            Bitmap vbmp = null;
            //vbmp = modAllgemein.GetEmmbedBitmap(Assembly.GetAssembly(typeof(QuickImage)), TMPName + ".ico");
            //if (vbmp != null) { return vbmp; }
            vbmp = modAllgemein.GetEmmbedBitmap(Assembly.GetAssembly(typeof(QuickImage)), TMPName + ".png");
            if (vbmp != null) { return vbmp; }
            //vbmp = modAllgemein.GetEmmbedBitmap(Assembly.GetAssembly(typeof(QuickImage)), TMPName + ".bmp");
            //if (vbmp != null) { return vbmp; }


            var i = GetIndex(TMPName);
            if (i >= 0 && PC[i] != this) { return PC[i].BMP; }

            return null;
        }


        private void Generate()
        {

            var bmpOri = GetBitmap(_name);
            Bitmap bmpTMP = null;
            Bitmap bmpKreuz = null;
            Bitmap bmpSecond = null;
            Color? colgreen = null;
            Color? colfärb = null;

            // Fehlerhaftes Bild
            if (bmpOri == null)
            {
                _IsError = true;
                _Bitmap = new Bitmap(16, 16, PixelFormat.Format32bppPArgb);
                using (var GR = Graphics.FromImage(_Bitmap))
                {
                    GR.Clear(Color.Black);
                    GR.DrawLine(new Pen(Color.Red, 3), 0, 0, _Bitmap.Width - 1, _Bitmap.Height - 1);
                    GR.DrawLine(new Pen(Color.Red, 3), _Bitmap.Width - 1, 0, 0, _Bitmap.Height - 1);
                }
                return;
            }



            // Bild ohne besonderen Effekte, schnell abhandeln
            if (bmpOri != null && _effekt == enImageCodeEffect.Ohne && string.IsNullOrEmpty(_changeGreenTo) && string.IsNullOrEmpty(_färbung) && _sättigung == 100 && _helligkeit == 100 && _transparenz == 100 && string.IsNullOrEmpty(_Zweitsymbol))
            {
                if (_width > 0)
                {
                    if (_height > 0)
                    {
                        _Bitmap = bmpOri.Resize(_width, _height, enSizeModes.EmptySpace, InterpolationMode.High, false);
                    }
                    else
                    {
                        _Bitmap = bmpOri.Resize(_width, _width, enSizeModes.EmptySpace, InterpolationMode.High, false);
                    }
                }
                else
                {
                    _Bitmap = bmpOri;
                }
                return;
            }


            if (!string.IsNullOrEmpty(_changeGreenTo)) { colgreen = _changeGreenTo.FromHTMLCode(); }
            if (!string.IsNullOrEmpty(_färbung)) { colfärb = _färbung.FromHTMLCode(); }


            // Bild Modifizieren ---------------------------------
            if (bmpOri != null)
            {

                bmpTMP = new Bitmap(bmpOri.Width, bmpOri.Height, PixelFormat.Format32bppPArgb);
                if (_effekt.HasFlag(enImageCodeEffect.Durchgestrichen))
                {
                    var tmpEx = _effekt ^ enImageCodeEffect.Durchgestrichen;
                    var n = "Kreuz|" + bmpOri.Width + "|";
                    if (bmpOri.Width != bmpOri.Height) { n += bmpOri.Height; }
                    n += "|";
                    if (tmpEx != enImageCodeEffect.Ohne) { n += (int)tmpEx; }
                    bmpKreuz = Get(n.Trim("|")).BMP;
                }

                if (!string.IsNullOrEmpty(_Zweitsymbol))
                {
                    var x = (int)(bmpOri.Width / 2);
                    bmpSecond = Get(_Zweitsymbol + "|" + x).BMP;
                }



                var c = new Color();
                var c1 = new Color();
                for (var X = 0 ; X < bmpOri.Width ; X++)
                {

                    for (var Y = 0 ; Y < bmpOri.Height ; Y++)
                    {
                        c = bmpOri.GetPixel(X, Y);

                        if (bmpSecond != null && X > bmpOri.Width - bmpSecond.Width && Y > bmpOri.Height - bmpSecond.Height)
                        {
                            var c2 = bmpSecond.GetPixel(X - (bmpOri.Width - bmpSecond.Width), Y - (bmpOri.Height - bmpSecond.Height));
                            if (!c2.IsMagentaOrTransparent()) { c = c2; }
                            //c = Color.Black;
                        }


                        if (c.IsMagentaOrTransparent())
                        {
                            c = Color.FromArgb(0, 0, 0, 0);
                        }
                        else
                        {
                            if (colgreen != null && c.ToArgb() == -16711936) { c = (Color)colgreen; }
                            if (colfärb != null) { c = Extensions.FromHSB(((Color)colfärb).GetHue(), ((Color)colfärb).GetSaturation(), c.GetBrightness(), c.A); }
                            if (_sättigung != 100 || _helligkeit != 100) { c = Extensions.FromHSB(c.GetHue(), c.GetSaturation() * _sättigung / 100, c.GetBrightness() * _helligkeit / 100, c.A); }

                            if (_effekt.HasFlag(enImageCodeEffect.WindowsXPDisabled))
                            {
                                var w = (int)(c.GetBrightness() * 100);
                                w = (int)(w / 2.8);
                                c = Extensions.FromHSB(0, 0, (float)(w / 100.0 + 0.5), c.A);
                            }

                            if (_effekt.HasFlag(enImageCodeEffect.Graustufen)) { c = c.ToGrey(); }
                        }


                        if (_effekt.HasFlag(enImageCodeEffect.Durchgestrichen))
                        {
                            if (c.IsMagentaOrTransparent())
                            {
                                c = bmpKreuz.GetPixel(X, Y);
                            }
                            else
                            {
                                if (bmpKreuz.GetPixel(X, Y).A > 0) { c = Extensions.MixColor(bmpKreuz.GetPixel(X, Y), c, 0.5); }
                            }
                        }

                        if (!c.IsMagentaOrTransparent())
                        {
                            if (_transparenz > 0 && _transparenz < 100) { c = Color.FromArgb((int)(c.A * (100 - _transparenz) / 100.0), c.R, c.G, c.B); }
                        }

                        if (_effekt.HasFlag(enImageCodeEffect.WindowsMEDisabled))
                        {
                            c1 = Color.FromArgb(0, 0, 0, 0);
                            if (!c.IsMagentaOrTransparent())
                            {
                                var RandPixel = false;
                                if (X > 0 && bmpOri.GetPixel(X - 1, Y).IsMagentaOrTransparent()) { RandPixel = true; }
                                if (Y > 0 && bmpOri.GetPixel(X, Y - 1).IsMagentaOrTransparent()) { RandPixel = true; }
                                if (X < bmpOri.Width - 1 && bmpOri.GetPixel(X + 1, Y).IsMagentaOrTransparent()) { RandPixel = true; }
                                if (Y < bmpOri.Height - 1 && bmpOri.GetPixel(X, Y + 1).IsMagentaOrTransparent()) { RandPixel = true; }

                                if (c.B < 128 || RandPixel)
                                {
                                    c1 = SystemColors.ControlDark;
                                    if (X < bmpOri.Width - 1 && Y < bmpOri.Height - 1)
                                    {
                                        if (bmpOri.GetPixel(X + 1, Y + 1).IsMagentaOrTransparent()) { c1 = SystemColors.ControlLightLight; }
                                    }
                                }
                            }
                            c = c1;
                        }

                        bmpTMP.SetPixel(X, Y, c);
                    }
                }

            }

            if (_width > 0)
            {
                if (_height > 0)
                {
                    _Bitmap = new Bitmap(_width, _height, PixelFormat.Format32bppPArgb);
                }
                else
                {
                    _Bitmap = new Bitmap(_width, _width, PixelFormat.Format32bppPArgb);
                }
            }
            else
            {
                _Bitmap = new Bitmap(bmpTMP.Width, bmpTMP.Height, PixelFormat.Format32bppPArgb);
            }


            using (var GR = Graphics.FromImage(_Bitmap))
            {
                GR.Clear(Color.FromArgb(0, 0, 0, 0));
                GR.InterpolationMode = InterpolationMode.High;
                bmpTMP = bmpTMP.Resize(_Bitmap.Width, _Bitmap.Height, enSizeModes.EmptySpace, InterpolationMode.High, false);
                GR.DrawImage(bmpTMP, 0, 0, _Bitmap.Width, _Bitmap.Height);
            }
        }




        public void Parse(string ToParse)
        {
            IsParsing = true;

            Initialize();

            var w = (ToParse + "||||||||||").Split('|');
            _name = w[0];




            if (string.IsNullOrEmpty(w[1]))
            {
                _width = -1;
            }
            else
            {
                _width = int.Parse(w[1]);
            }

            if (string.IsNullOrEmpty(w[2]))
            {
                _height = -1;
            }
            else
            {
                _height = int.Parse(w[2]);
            }

            if (!string.IsNullOrEmpty(w[3]))
            {
                _effekt = (enImageCodeEffect)int.Parse(w[3]);
            }


            _färbung = w[4];
            _changeGreenTo = w[5];


            if (string.IsNullOrEmpty(w[6]))
            {
                _helligkeit = 100;
            }
            else
            {
                _helligkeit = int.Parse(w[6]);
            }

            if (string.IsNullOrEmpty(w[7]))
            {
                _sättigung = 100;
            }
            else
            {
                _sättigung = int.Parse(w[7]);
            }

            if (string.IsNullOrEmpty(w[8]))
            {
                _drehWinkel = 0;
            }
            else
            {
                _drehWinkel = int.Parse(w[8]);
            }

            if (string.IsNullOrEmpty(w[9]))
            {
                _transparenz = 0;
            }
            else
            {
                _transparenz = int.Parse(w[9]);
            }

            if (string.IsNullOrEmpty(w[10]))
            {
                _Zweitsymbol = string.Empty;
            }
            else
            {
                _Zweitsymbol = w[10];
            }

            if (_width > 0 && _height < 0)
            {
                _height = _width;
            }
            IsParsing = false;
        }



        public QuickImage SymbolForReadableText()
        {
            return this;
        }

        public string ReadableText()
        {
            return string.Empty;
        }

        public string CompareKey()
        {
            return ToString();
        }

        public void OnChanged()
        {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }
    }
}

