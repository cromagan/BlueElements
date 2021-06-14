#region BlueElements - a collection of useful tools, database and controls

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

#endregion BlueElements - a collection of useful tools, database and controls

using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using static BlueBasics.modConverter;

namespace BlueBasics {

    public sealed class QuickImage : IParseable, IReadableTextWithChanging, ICompareKey {
        private static string _SearchedCode = string.Empty;
        private static int _FoundInde = -1;
        private static readonly object _locker = new();

        #region Variablen-Deklarationen

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
        private BitmapExt _Bitmap;
        private string TMPCode = string.Empty;
        private bool _IsError;

        #endregion Variablen-Deklarationen

        #region Event-Deklarationen + Delegaten

        public event EventHandler Changed;

        public static event EventHandler<NeedImageEventArgs> NeedImage;

        #endregion Event-Deklarationen + Delegaten

        #region Construktor + Initialize

        public QuickImage(string imageCode) => Parse(imageCode);

        private void Initialize() {
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

        #endregion Construktor + Initialize

        #region Properties

        public Bitmap BMP {
            get {
                if (_Bitmap == null) { Generate(); }
                return _Bitmap.Bitmap;
            }
        }

        public BitmapExt BMPExt {
            get {
                if (_Bitmap == null) { Generate(); }
                return _Bitmap;
            }
        }

        public bool IsError {
            get {
                if (_Bitmap == null) { Generate(); }
                return _IsError;
            }
        }

        public bool IsParsing { get; private set; }

        public string Name {
            get => _name;
            set {
                _name = value;
                OnChanged();
            }
        }

        public enImageCodeEffect Effekt {
            get => _effekt;
            set {
                _effekt = value;
                OnChanged();
            }
        }

        public int Width {
            get => _width;
            set {
                _width = value;
                OnChanged();
            }
        }

        public int Height {
            get => _height;
            set {
                _height = value;
                OnChanged();
            }
        }

        public string Färbung {
            get => _färbung;
            set {
                _färbung = value;
                OnChanged();
            }
        }

        public string ChangeGreenTo {
            get => _changeGreenTo;
            set {
                _changeGreenTo = value;
                OnChanged();
            }
        }

        public int Helligkeit {
            get => _helligkeit;
            set {
                _helligkeit = value;
                OnChanged();
            }
        }

        public int Sättigung {
            get => _sättigung;
            set {
                _sättigung = value;
                OnChanged();
            }
        }

        public int Transparenz {
            get => _transparenz;
            set {
                _transparenz = value;
                OnChanged();
            }
        }

        public string Zweitsymbol {
            get => _Zweitsymbol;
            set {
                _Zweitsymbol = value;
                OnChanged();
            }
        }

        public int DrehWinkel {
            get => _drehWinkel;
            set {
                _drehWinkel = value;
                OnChanged();
            }
        }

        #endregion Properties

        #region Shares

        private static readonly List<QuickImage> _pics = new();

        public static QuickImage Get(Bitmap image) {
            if (image == null) { return null; }
            lock (_locker) {
                foreach (var ThisQuickImage in _pics) {
                    if (ThisQuickImage != null) { return ThisQuickImage; }
                }
            }
            return null;
        }

        public static QuickImage Get(QuickImage imageCode, enImageCodeEffect additionalState) => additionalState == enImageCodeEffect.Ohne ? imageCode
: Get(GenerateCode(imageCode.Name, imageCode.Width, imageCode.Height, imageCode.Effekt | additionalState, imageCode.Färbung, imageCode.ChangeGreenTo, imageCode.Sättigung, imageCode.Helligkeit, imageCode.DrehWinkel, imageCode.Transparenz, imageCode.Zweitsymbol));

        public static QuickImage Get(string imageCode) {
            if (string.IsNullOrEmpty(imageCode)) { return null; }
            var z = GetIndex(imageCode);
            if (z >= 0) { return _pics[z]; }
            QuickImage l = new(imageCode);
            if (l.ToString() != imageCode) {
                Develop.DebugPrint("Fehlerhafter Imagecode: " + imageCode + " -> " + l);
                z = GetIndex(l.ToString());
                if (z >= 0) { return _pics[z]; }
            }
            lock (_locker) {
                _pics.Add(l);
            }
            return l;
        }

        public static QuickImage Get(string image, int squareWidth) {
            if (string.IsNullOrEmpty(image)) { return null; }
            if (image.Contains("|")) {
                var w = (image + "||||||").Split('|');
                w[1] = squareWidth.ToString();
                w[2] = string.Empty;
                return Get(w.JoinWith("|"));
            }
            return Get(GenerateCode(image, squareWidth, 0, enImageCodeEffect.Ohne, string.Empty, string.Empty, 100, 100, 0, 0, string.Empty));
        }

        public static QuickImage Get(enImageCode image) => Get(image, 16);

        public static QuickImage Get(enImageCode image, int squareWidth) => image == enImageCode.None ? null
: Get(GenerateCode(Enum.GetName(image.GetType(), image), squareWidth, 0, enImageCodeEffect.Ohne, string.Empty, string.Empty, 100, 100, 0, 0, string.Empty));

        public static QuickImage Get(enImageCode image, int squareWidth, string färbung, string changeGreenTo) => image == enImageCode.None ? null
: Get(GenerateCode(Enum.GetName(image.GetType(), image), squareWidth, 0, enImageCodeEffect.Ohne, färbung, changeGreenTo, 100, 100, 0, 0, string.Empty));

        public static QuickImage Get(int index) {
            lock (_locker) {
                if (index >= 0 && index < _pics.Count) { return _pics[index]; }
            }
            return null;
        }

        public static enImageCode FileTypeImage(enFileFormat file) {
            switch (file) {
                case enFileFormat.WordKind:
                    return enImageCode.Word;

                case enFileFormat.ExcelKind:
                    return enImageCode.Excel;

                case enFileFormat.PowerPointKind:
                    return enImageCode.PowerPoint;

                case enFileFormat.Textdocument:
                    return enImageCode.Textdatei;

                case enFileFormat.EMail:
                    return enImageCode.Brief;

                case enFileFormat.Pdf:
                    return enImageCode.PDF;

                case enFileFormat.HTML:
                    return enImageCode.Globus;

                case enFileFormat.Image:
                    return enImageCode.Bild;

                case enFileFormat.CompressedArchive:
                    return enImageCode.Karton;

                case enFileFormat.Movie:
                    return enImageCode.Filmrolle;

                case enFileFormat.Executable:
                    return enImageCode.Anwendung;

                case enFileFormat.HelpFile:
                    return enImageCode.Frage;

                case enFileFormat.Database:
                    return enImageCode.Datenbank;

                case enFileFormat.XMLFile:
                    return enImageCode.XML;

                case enFileFormat.Visitenkarte:
                    return enImageCode.Visitenkarte;

                case enFileFormat.Sound:
                    return enImageCode.Note;

                case enFileFormat.Unknown:
                    return enImageCode.Datei;

                case enFileFormat.ProgrammingCode:
                    return enImageCode.Skript;

                case enFileFormat.Link:
                    return enImageCode.Undo;

                case enFileFormat.BlueCreativeFile:
                    return enImageCode.Smiley;

                case enFileFormat.Icon:
                    return enImageCode.Bild;

                default:
                    Develop.DebugPrint(file);
                    return enImageCode.Datei;
            }
        }

        public static QuickImage Get(enFileFormat file, int size) => Get(FileTypeImage(file), size);

        public static int GetIndex(QuickImage img) {
            lock (_locker) {
                return _pics.IndexOf(img);
            }
        }

        public static int GetIndex(string code) {
            lock (_locker) {
                if (string.IsNullOrEmpty(code)) { return -1; }
                if (_SearchedCode == code) { return _FoundInde; }
                for (var z = 0; z < _pics.Count; z++) {
                    if (code == _pics[z].ToString()) {
                        _SearchedCode = code;
                        _FoundInde = z;
                        return z;
                    }
                }
                return -1;
            }
        }

        public static string GenerateCode(string name, int width, int height, enImageCodeEffect effekt, string färbung, string changeGreenTo, int sättigung, int helligkeit, int drehwinkel, int transparenz, string zweitsymbol) {
            var C = name + "|";
            if (width > 0) { C += width; }
            C += "|";
            if (height > 0 && width != height) { C += height; }
            C += "|";
            if (effekt != enImageCodeEffect.Ohne) { C += (int)effekt; }
            C += "|";
            C += färbung;
            C += "|";
            C += changeGreenTo;
            C += "|";
            if (helligkeit != 100) { C += helligkeit; }
            C += "|";
            if (sättigung != 100) { C += sättigung; }
            C += "|";
            if (drehwinkel > 0) { C += drehwinkel; }
            C += "|";
            if (transparenz > 0) { C += transparenz; }
            C += "|";
            if (!string.IsNullOrEmpty(zweitsymbol)) { C += zweitsymbol; }
            return C.TrimEnd('|');
        }

        public static void Add(string name, BitmapExt bMP) {
            if (string.IsNullOrEmpty(name)) { return; }
            if (bMP == null) { return; }
            lock (_locker) {
                var z = GetIndex(name);
                if (z < 0) {
                    QuickImage l = new(name);
                    _pics.Add(l);
                    z = GetIndex(name);
                }
                _pics[z]._Bitmap = bMP;
            }
        }

        #endregion Shares

        public override string ToString() {
            if (string.IsNullOrEmpty(TMPCode)) {
                TMPCode = GenerateCode(_name, _width, _height, _effekt, _färbung, _changeGreenTo, _sättigung, _helligkeit, _drehWinkel, _transparenz, _Zweitsymbol);
            }
            return TMPCode;
        }

        public BitmapExt GetBitmap(string tmpname) {
            var vbmp = modAllgemein.GetEmmbedBitmap(Assembly.GetAssembly(typeof(QuickImage)), tmpname + ".png");
            if (vbmp != null) { return vbmp; }
            lock (_locker) {
                var i = GetIndex(tmpname);
                if (i >= 0 && _pics[i] != this) { return _pics[i].BMPExt; }
            }
            NeedImageEventArgs e = new(tmpname);
            OnNeedImage(e);
            lock (_locker) {
                // Evtl. hat die "OnNeedImage" das Bild auch in den Stack hochgeladen
                var i2 = GetIndex(tmpname);
                if (i2 < 0) { Add(tmpname, e.BMP); }
            }
            return e.BMP;
        }

        private void Generate() {
            var bmpOri = GetBitmap(_name);
            BitmapExt bmpTMP = null;
            Bitmap bmpKreuz = null;
            Bitmap bmpSecond = null;
            Color? colgreen = null;
            Color? colfärb = null;
            // Fehlerhaftes Bild
            if (bmpOri == null) {
                _IsError = true;
                _Bitmap = new BitmapExt(16, 16);
                using var GR = Graphics.FromImage(_Bitmap.Bitmap);
                GR.Clear(Color.Black);
                GR.DrawLine(new Pen(Color.Red, 3), 0, 0, _Bitmap.Width - 1, _Bitmap.Height - 1);
                GR.DrawLine(new Pen(Color.Red, 3), _Bitmap.Width - 1, 0, 0, _Bitmap.Height - 1);
                return;
            }
            // Bild ohne besonderen Effekte, schnell abhandeln
            if (bmpOri != null && _effekt == enImageCodeEffect.Ohne && string.IsNullOrEmpty(_changeGreenTo) && string.IsNullOrEmpty(_färbung) && _sättigung == 100 && _helligkeit == 100 && _transparenz == 100 && string.IsNullOrEmpty(_Zweitsymbol)) {
                if (_width > 0) {
                    if (_height > 0) {
                        bmpOri.Resize(_width, _height, enSizeModes.EmptySpace, InterpolationMode.High, false);
                    } else {
                        bmpOri.Resize(_width, _width, enSizeModes.EmptySpace, InterpolationMode.High, false);
                    }
                }
                _Bitmap = bmpOri;
                return;
            }
            if (!string.IsNullOrEmpty(_changeGreenTo)) { colgreen = _changeGreenTo.FromHTMLCode(); }
            if (!string.IsNullOrEmpty(_färbung)) { colfärb = _färbung.FromHTMLCode(); }
            // Bild Modifizieren ---------------------------------
            if (bmpOri != null) {
                bmpTMP = new BitmapExt(bmpOri.Width, bmpOri.Height);
                if (_effekt.HasFlag(enImageCodeEffect.Durchgestrichen)) {
                    var tmpEx = _effekt ^ enImageCodeEffect.Durchgestrichen;
                    var n = "Kreuz|" + bmpOri.Width + "|";
                    if (bmpOri.Width != bmpOri.Height) { n += bmpOri.Height; }
                    n += "|";
                    if (tmpEx != enImageCodeEffect.Ohne) { n += (int)tmpEx; }
                    bmpKreuz = Get(n.Trim("|")).BMP;
                }
                if (!string.IsNullOrEmpty(_Zweitsymbol)) {
                    var x = bmpOri.Width / 2;
                    bmpSecond = Get(_Zweitsymbol + "|" + x).BMP;
                }
                for (var X = 0; X < bmpOri.Width; X++) {
                    for (var Y = 0; Y < bmpOri.Height; Y++) {
                        var c = bmpOri.GetPixel(X, Y);
                        if (bmpSecond != null && X > bmpOri.Width - bmpSecond.Width && Y > bmpOri.Height - bmpSecond.Height) {
                            var c2 = bmpSecond.GetPixel(X - (bmpOri.Width - bmpSecond.Width), Y - (bmpOri.Height - bmpSecond.Height));
                            if (!c2.IsMagentaOrTransparent()) { c = c2; }
                        }
                        if (c.IsMagentaOrTransparent()) {
                            c = Color.FromArgb(0, 0, 0, 0);
                        } else {
                            if (colgreen != null && c.ToArgb() == -16711936) { c = (Color)colgreen; }
                            if (colfärb != null) { c = Extensions.FromHSB(((Color)colfärb).GetHue(), ((Color)colfärb).GetSaturation(), c.GetBrightness(), c.A); }
                            if (_sättigung != 100 || _helligkeit != 100) { c = Extensions.FromHSB(c.GetHue(), c.GetSaturation() * _sättigung / 100, c.GetBrightness() * _helligkeit / 100, c.A); }
                            if (_effekt.HasFlag(enImageCodeEffect.WindowsXPDisabled)) {
                                var w = (int)(c.GetBrightness() * 100);
                                w = (int)(w / 2.8);
                                c = Extensions.FromHSB(0, 0, (float)((w / 100.0) + 0.5), c.A);
                            }
                            if (_effekt.HasFlag(enImageCodeEffect.Graustufen)) { c = c.ToGrey(); }
                        }
                        if (_effekt.HasFlag(enImageCodeEffect.Durchgestrichen)) {
                            if (c.IsMagentaOrTransparent()) {
                                c = bmpKreuz.GetPixel(X, Y);
                            } else {
                                if (bmpKreuz.GetPixel(X, Y).A > 0) { c = Extensions.MixColor(bmpKreuz.GetPixel(X, Y), c, 0.5); }
                            }
                        }
                        if (!c.IsMagentaOrTransparent() && _transparenz > 0 && _transparenz < 100) {
                            c = Color.FromArgb((int)(c.A * (100 - _transparenz) / 100.0), c.R, c.G, c.B);
                        }
                        if (_effekt.HasFlag(enImageCodeEffect.WindowsMEDisabled)) {
                            var c1 = Color.FromArgb(0, 0, 0, 0);
                            if (!c.IsMagentaOrTransparent()) {
                                var RandPixel = false;
                                if (X > 0 && bmpOri.GetPixel(X - 1, Y).IsMagentaOrTransparent()) { RandPixel = true; }
                                if (Y > 0 && bmpOri.GetPixel(X, Y - 1).IsMagentaOrTransparent()) { RandPixel = true; }
                                if (X < bmpOri.Width - 1 && bmpOri.GetPixel(X + 1, Y).IsMagentaOrTransparent()) { RandPixel = true; }
                                if (Y < bmpOri.Height - 1 && bmpOri.GetPixel(X, Y + 1).IsMagentaOrTransparent()) { RandPixel = true; }
                                if (c.B < 128 || RandPixel) {
                                    c1 = SystemColors.ControlDark;
                                    if (X < bmpOri.Width - 1 && Y < bmpOri.Height - 1 && bmpOri.GetPixel(X + 1, Y + 1).IsMagentaOrTransparent()) {
                                        c1 = SystemColors.ControlLightLight;
                                    }
                                }
                            }
                            c = c1;
                        }
                        bmpTMP.SetPixel(X, Y, c);
                    }
                }
            }
            if (_width > 0) {
                if (_height > 0) {
                    bmpTMP.Resize(_width, _height, enSizeModes.EmptySpace, InterpolationMode.High, false);
                } else {
                    bmpTMP.Resize(_width, _width, enSizeModes.EmptySpace, InterpolationMode.High, false);
                }
                _Bitmap = bmpTMP;
            } else {
                _Bitmap = bmpTMP;
            }
        }

        public void Parse(string toParse) {
            IsParsing = true;
            Initialize();
            var w = (toParse + "||||||||||").Split('|');
            _name = w[0];
            _width = string.IsNullOrEmpty(w[1]) ? -1 : IntParse(w[1]);
            _height = string.IsNullOrEmpty(w[2]) ? -1 : IntParse(w[2]);
            if (!string.IsNullOrEmpty(w[3])) {
                _effekt = (enImageCodeEffect)IntParse(w[3]);
            }
            _färbung = w[4];
            _changeGreenTo = w[5];
            _helligkeit = string.IsNullOrEmpty(w[6]) ? 100 : int.Parse(w[6]);
            _sättigung = string.IsNullOrEmpty(w[7]) ? 100 : int.Parse(w[7]);
            _drehWinkel = string.IsNullOrEmpty(w[8]) ? 0 : int.Parse(w[8]);
            _transparenz = string.IsNullOrEmpty(w[9]) ? 0 : int.Parse(w[9]);
            _Zweitsymbol = string.IsNullOrEmpty(w[10]) ? string.Empty : w[10];
            if (_width > 0 && _height < 0) {
                _height = _width;
            }
            IsParsing = false;
        }

        public QuickImage SymbolForReadableText() => this;

        public string ReadableText() => string.Empty;

        public string CompareKey() => ToString();

        public void OnChanged() {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }

        public void OnNeedImage(NeedImageEventArgs e) => NeedImage?.Invoke(this, e);
    }
}