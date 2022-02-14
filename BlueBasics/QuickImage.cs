// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using static BlueBasics.Converter;

namespace BlueBasics {

    public sealed class QuickImage : BitmapExt, IReadableText, ICompareKey {

        #region Fields

        private static readonly object _locker = new();
        private static readonly Dictionary<string, QuickImage> _pics = new();

        #endregion

        #region Constructors

        /// <summary>
        /// QuickImages werden immer in den Speicher für spätere Zugriffe aufgenommen!
        /// </summary>
        public QuickImage(string imageCode) : base() {
            IsParsing = true;

            lock (_locker) {
                if (Exists(imageCode)) { Develop.DebugPrint_NichtImplementiert(); }

                Name = string.Empty;
                Effekt = enImageCodeEffect.Ohne;
                Färbung = string.Empty;
                ChangeGreenTo = string.Empty;
                Helligkeit = 100;
                Sättigung = 100;
                DrehWinkel = 0;
                Transparenz = 0;
                Zweitsymbol = string.Empty;

                var w = (imageCode + "||||||||||").Split('|');
                Name = w[0];
                var width = string.IsNullOrEmpty(w[1]) ? -1 : IntParse(w[1]);
                var height = string.IsNullOrEmpty(w[2]) ? -1 : IntParse(w[2]);
                if (!string.IsNullOrEmpty(w[3])) {
                    Effekt = (enImageCodeEffect)IntParse(w[3]);
                }
                Färbung = w[4];
                ChangeGreenTo = w[5];
                Helligkeit = string.IsNullOrEmpty(w[6]) ? 100 : int.Parse(w[6]);
                Sättigung = string.IsNullOrEmpty(w[7]) ? 100 : int.Parse(w[7]);
                DrehWinkel = string.IsNullOrEmpty(w[8]) ? 0 : int.Parse(w[8]);
                Transparenz = string.IsNullOrEmpty(w[9]) ? 0 : int.Parse(w[9]);
                Zweitsymbol = string.IsNullOrEmpty(w[10]) ? string.Empty : w[10];
                if (width > 0 && height < 0) { height = width; }

                if (Effekt < 0) { Effekt = enImageCodeEffect.Ohne; }

                IsParsing = false;

                Generate(width, height);

                Code = GenerateCode(Name, Width, Height, Effekt, Färbung, ChangeGreenTo, Sättigung, Helligkeit, DrehWinkel, Transparenz, Zweitsymbol);

                _pics.Add(Code, this);
            }
        }

        /// <summary>
        /// QuickImages werden immer in den Speicher für spätere Zugriffe aufgenommen!
        /// </summary>
        public QuickImage(string name, Bitmap bmp, int width, int height) {
            if (string.IsNullOrEmpty(name)) { return; }

            lock (_locker) {
                if (Exists(name)) { Develop.DebugPrint(enFehlerArt.Warnung, "Doppeltes Bild:" + name); }
                if (name.Contains("|")) { Develop.DebugPrint(enFehlerArt.Warnung, "Fehlerhafter Name:" + name); }

                Name = name;
                Code = Name;
                _pics.Add(Code, this);
            }

            if (bmp == null) {
                EmptyBitmap(width, height);
                IsError = true;
            } else {
                CloneFromBitmap(bmp);
                Resize(width, height, enSizeModes.EmptySpace, InterpolationMode.High, false);
            }
        }

        #endregion

        #region Events

        public static event EventHandler<NeedImageEventArgs> NeedImage;

        #endregion

        #region Properties

        public string ChangeGreenTo { get; private set; }
        public string Code { get; private set; }
        public int DrehWinkel { get; private set; }
        public enImageCodeEffect Effekt { get; private set; }
        public string Färbung { get; private set; }
        public int Helligkeit { get; private set; }
        public bool IsError { get; private set; } = false;
        public bool IsParsing { get; private set; }
        public string Name { get; private set; }
        public int Sättigung { get; private set; }
        public int Transparenz { get; private set; }
        public string Zweitsymbol { get; private set; }

        #endregion

        #region Methods

        public static bool Exists(string imageCode) {
            if (string.IsNullOrEmpty(imageCode)) { return false; }
            return _pics.ContainsKey(imageCode);

            //var z = GetIndex(imageCode);
            //return z >= 0;
        }

        public static enImageCode FileTypeImage(enFileFormat file) {
            switch (file) {
                case enFileFormat.WordKind: return enImageCode.Word;
                case enFileFormat.ExcelKind: return enImageCode.Excel;
                case enFileFormat.PowerPointKind: return enImageCode.PowerPoint;
                case enFileFormat.Textdocument: return enImageCode.Textdatei;
                case enFileFormat.EMail: return enImageCode.Brief;
                case enFileFormat.Pdf: return enImageCode.PDF;
                case enFileFormat.HTML: return enImageCode.Globus;
                case enFileFormat.Image: return enImageCode.Bild;
                case enFileFormat.CompressedArchive: return enImageCode.Karton;
                case enFileFormat.Movie: return enImageCode.Filmrolle;
                case enFileFormat.Executable: return enImageCode.Anwendung;
                case enFileFormat.HelpFile: return enImageCode.Frage;
                case enFileFormat.Database: return enImageCode.Datenbank;
                case enFileFormat.XMLFile: return enImageCode.XML;
                case enFileFormat.Visitenkarte: return enImageCode.Visitenkarte;
                case enFileFormat.Sound: return enImageCode.Note;
                case enFileFormat.Unknown: return enImageCode.Datei;
                case enFileFormat.ProgrammingCode: return enImageCode.Skript;
                case enFileFormat.Link: return enImageCode.Undo;
                case enFileFormat.BlueCreativeFile: return enImageCode.Smiley;
                case enFileFormat.Icon: return enImageCode.Bild;
                default:
                    Develop.DebugPrint(file);
                    return enImageCode.Datei;
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

        //public static QuickImage Get(Bitmap image) {
        //    if (image == null) { return null; }
        //    lock (_locker) {
        //        foreach (var ThisQuickImage in _pics) {
        //            if (ThisQuickImage != null) { return ThisQuickImage; }
        //        }
        //    }
        //    return null;
        //}

        public static QuickImage Get(QuickImage imageCode, enImageCodeEffect additionalState) => additionalState == enImageCodeEffect.Ohne ? imageCode
: Get(GenerateCode(imageCode.Name, imageCode.Width, imageCode.Height, imageCode.Effekt | additionalState, imageCode.Färbung, imageCode.ChangeGreenTo, imageCode.Sättigung, imageCode.Helligkeit, imageCode.DrehWinkel, imageCode.Transparenz, imageCode.Zweitsymbol));

        public static QuickImage Get(string imageCode) {
            if (string.IsNullOrEmpty(imageCode)) { return null; }
            //var z = GetIndex(imageCode);
            //if (z >= 0) { return _pics[z]; }

            if (_pics.TryGetValue(imageCode, out var p)) { return p; }

            //if (l.ToString() != imageCode) {
            //    Develop.DebugPrint("Fehlerhafter Imagecode: " + imageCode + " -> " + l);
            //    z = GetIndex(l.ToString());
            //    if (z >= 0) { return _pics[z]; }
            //}

            return new(imageCode);
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

        //public static QuickImage Get(int index) {
        //    lock (_locker) {
        //        if (index >= 0 && index < _pics.Count) { return _pics[index]; }
        //    }
        //    return null;
        //}

        public static QuickImage Get(enFileFormat file, int size) => Get(FileTypeImage(file), size);

        //public static int GetIndex(QuickImage img) {
        //    lock (_locker) {
        //        return _pics.IndexOf(img);
        //    }
        //}

        public string CompareKey() => ToString();

        public void OnNeedImage(NeedImageEventArgs e) => NeedImage?.Invoke(this, e);

        public string ReadableText() => string.Empty;

        public QuickImage SymbolForReadableText() => this;

        public override string ToString() => Code;

        //private static int GetIndex(string code) {
        //    lock (_locker) {
        //        if (string.IsNullOrEmpty(code)) { return -1; }
        //        if (_SearchedCode == code) { return _FoundInde; }
        //        for (var z = 0; z < _pics.Count; z++) {
        //            if (code == _pics[z].ToString()) {
        //                _SearchedCode = code;
        //                _FoundInde = z;
        //                return z;
        //            }
        //        }
        //        return -1;
        //    }
        //}

        private void Generate(int width, int height) {
            BitmapExt bmpTMP = null;
            Bitmap bmpKreuz = null;
            Bitmap bmpSecond = null;
            Color? colgreen = null;
            Color? colfärb = null;

            var bmpOri = GetBitmap(Name, width, height);

            #region  Fehlerhaftes Bild erzeugen

            if (bmpOri == null) {
                IsError = true;
                EmptyBitmap(width, height);
                using var GR = Graphics.FromImage(this);
                GR.Clear(Color.Black);
                GR.DrawLine(new Pen(Color.Red, 3), 0, 0, width - 1, height - 1);
                GR.DrawLine(new Pen(Color.Red, 3), width - 1, 0, 0, height - 1);
                return;
            }

            #endregion

            #region Bild ohne besonderen Effekte, schnell abhandeln

            if (bmpOri != null && Effekt == enImageCodeEffect.Ohne && string.IsNullOrEmpty(ChangeGreenTo) && string.IsNullOrEmpty(Färbung) && Sättigung == 100 && Helligkeit == 100 && Transparenz == 100 && string.IsNullOrEmpty(Zweitsymbol)) {
                CloneFromBitmap(bmpOri);
                if (width > 0) {
                    if (height > 0) {
                        Resize(width, height, enSizeModes.EmptySpace, InterpolationMode.High, false);
                    } else {
                        Resize(width, width, enSizeModes.EmptySpace, InterpolationMode.High, false);
                    }
                }

                return;
            }

            #endregion

            if (!string.IsNullOrEmpty(ChangeGreenTo)) { colgreen = ChangeGreenTo.FromHTMLCode(); }
            if (!string.IsNullOrEmpty(Färbung)) { colfärb = Färbung.FromHTMLCode(); }
            // Bild Modifizieren ---------------------------------
            if (bmpOri != null) {
                bmpTMP = new BitmapExt(bmpOri.Width, bmpOri.Height);
                if (Effekt.HasFlag(enImageCodeEffect.Durchgestrichen)) {
                    var tmpEx = Effekt ^ enImageCodeEffect.Durchgestrichen;
                    var n = "Kreuz|" + bmpOri.Width + "|";
                    if (bmpOri.Width != bmpOri.Height) { n += bmpOri.Height; }
                    n += "|";
                    if (tmpEx != enImageCodeEffect.Ohne) { n += (int)tmpEx; }
                    bmpKreuz = Get(n.Trim("|"));
                }
                if (!string.IsNullOrEmpty(Zweitsymbol)) {
                    var x = bmpOri.Width / 2;
                    bmpSecond = Get(Zweitsymbol + "|" + x);
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
                            if (Sättigung != 100 || Helligkeit != 100) { c = Extensions.FromHSB(c.GetHue(), c.GetSaturation() * Sättigung / 100, c.GetBrightness() * Helligkeit / 100, c.A); }
                            if (Effekt.HasFlag(enImageCodeEffect.WindowsXPDisabled)) {
                                var w = (int)(c.GetBrightness() * 100);
                                w = (int)(w / 2.8);
                                c = Extensions.FromHSB(0, 0, (float)((w / 100.0) + 0.5), c.A);
                            }
                            if (Effekt.HasFlag(enImageCodeEffect.Graustufen)) { c = c.ToGrey(); }
                        }
                        if (Effekt.HasFlag(enImageCodeEffect.Durchgestrichen)) {
                            if (c.IsMagentaOrTransparent()) {
                                c = bmpKreuz.GetPixel(X, Y);
                            } else {
                                if (bmpKreuz.GetPixel(X, Y).A > 0) { c = Extensions.MixColor(bmpKreuz.GetPixel(X, Y), c, 0.5); }
                            }
                        }
                        if (!c.IsMagentaOrTransparent() && Transparenz > 0 && Transparenz < 100) {
                            c = Color.FromArgb((int)(c.A * (100 - Transparenz) / 100.0), c.R, c.G, c.B);
                        }
                        if (Effekt.HasFlag(enImageCodeEffect.WindowsMEDisabled)) {
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
            if (width > 0) {
                if (height > 0) {
                    bmpTMP.Resize(width, height, enSizeModes.EmptySpace, InterpolationMode.High, false);
                } else {
                    bmpTMP.Resize(width, width, enSizeModes.EmptySpace, InterpolationMode.High, false);
                }
            }
            CloneFromBitmap(bmpTMP);
        }

        private Bitmap GetBitmap(string tmpname, int width, int height) {
            var vbmp = GetEmmbedBitmap(Assembly.GetAssembly(typeof(QuickImage)), tmpname + ".png");
            if (vbmp != null) {
                vbmp.Resize(width, height, enSizeModes.EmptySpace, InterpolationMode.High, false);
                return vbmp;
            }

            if (_pics.TryGetValue(tmpname, out var p) && p != this) {
                if (p.IsError) { return null; }
                return p;
            }

            //lock (_locker) {
            //    var i = GetIndex(tmpname);
            //    if (i >= 0 && _pics[i] != this) {
            //        if (_pics[i].IsError) { return null; }
            //        return _pics[i];
            //    }
            //}

            NeedImageEventArgs e = new(tmpname);
            OnNeedImage(e);

            lock (_locker) {
                // Evtl. hat die "OnNeedImage" das Bild auch in den Stack hochgeladen
                // Falls nicht, hier noch erledigen
                if (!Exists(tmpname)) { return new QuickImage(tmpname, e.BMP, width, height); }
            }
            return e.BMP;
        }

        #endregion
    }
}