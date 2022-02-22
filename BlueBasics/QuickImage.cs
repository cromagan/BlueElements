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
using System.Text;
using static BlueBasics.Converter;

namespace BlueBasics {

    public sealed class QuickImage : BitmapExt, IReadableText, ICompareKey {

        #region Fields

        public readonly string ChangeGreenTo;
        public readonly string Code;
        public readonly int DrehWinkel;
        public readonly enImageCodeEffect Effekt;
        public readonly string Färbung;
        public readonly int Helligkeit;
        public readonly string Name;
        public readonly int Sättigung;
        public readonly int Transparenz;
        public readonly string Zweitsymbol;
        private static readonly Dictionary<string, QuickImage> _pics = new();
        private static object Locker = new();

        #endregion

        #region Constructors

        /// <summary>
        /// QuickImages werden immer in den Speicher für spätere Zugriffe aufgenommen!
        /// </summary>
        public QuickImage(string imageCode, bool generate) : base() {
            var w = (imageCode + "||||||||||").Split('|');
            Name = w[0];
            var width = string.IsNullOrEmpty(w[1]) ? -1 : IntParse(w[1]);
            var height = string.IsNullOrEmpty(w[2]) ? -1 : IntParse(w[2]);
            CorrectSize(width, height, null);
            Effekt = string.IsNullOrEmpty(w[3]) ? enImageCodeEffect.Ohne : (enImageCodeEffect)IntParse(w[3]);
            Färbung = w[4];
            ChangeGreenTo = w[5];
            Helligkeit = string.IsNullOrEmpty(w[6]) ? 100 : int.Parse(w[6]);
            Sättigung = string.IsNullOrEmpty(w[7]) ? 100 : int.Parse(w[7]);
            DrehWinkel = string.IsNullOrEmpty(w[8]) ? 0 : int.Parse(w[8]);
            Transparenz = string.IsNullOrEmpty(w[9]) ? 0 : int.Parse(w[9]);
            Zweitsymbol = string.IsNullOrEmpty(w[10]) ? string.Empty : w[10];

            //Code = GenerateCode(Name, width, height, Effekt, Färbung, ChangeGreenTo, Sättigung, Helligkeit, DrehWinkel, Transparenz, Zweitsymbol);
            Code = imageCode;

            if (generate) { Generate(); }
        }

        /// <summary>
        /// QuickImages werden immer in den Speicher für spätere Zugriffe aufgenommen!
        /// </summary>
        public QuickImage(string imagecode, Bitmap bmp) {
            if (string.IsNullOrEmpty(imagecode)) { return; }

            if (Exists(imagecode)) { Develop.DebugPrint(enFehlerArt.Warnung, "Doppeltes Bild:" + imagecode); }
            if (imagecode.Contains("|")) { Develop.DebugPrint(enFehlerArt.Warnung, "Fehlerhafter Name:" + imagecode); }

            Name = imagecode;
            Code = Name;
            lock (Locker) {
                CorrectSize(-1, -1, bmp);
                _pics.Add(Code, this);
            }

            if (bmp == null) {
                GenerateErrorImage();
            } else {
                CloneFromBitmap(bmp);
            }
        }

        #endregion

        #region Events

        public static event EventHandler<NeedImageEventArgs> NeedImage;

        #endregion

        #region Properties

        public new int Height { get; private set; }

        public bool IsError { get; private set; } = false;

        public new int Width { get; private set; }

        #endregion

        #region Methods

        public static bool Exists(string imageCode) {
            if (string.IsNullOrEmpty(imageCode)) { return false; }
            return _pics.ContainsKey(imageCode);
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
            var c = new StringBuilder();

            c.Append(name);
            c.Append("|");
            if (width > 0) { c.Append(width); }
            c.Append("|");
            if (height > 0 && width != height) { c.Append(height); }
            c.Append("|");
            if (effekt != enImageCodeEffect.Ohne) { c.Append((int)effekt); }
            c.Append("|");
            c.Append(färbung);
            c.Append("|");
            c.Append(changeGreenTo);
            c.Append("|");
            if (helligkeit != 100) { c.Append(helligkeit); }
            c.Append("|");
            if (sättigung != 100) { c.Append(sättigung); }
            c.Append("|");
            if (drehwinkel > 0) { c.Append(drehwinkel); }
            c.Append("|");
            if (transparenz > 0) { c.Append(transparenz); }
            c.Append("|");
            if (!string.IsNullOrEmpty(zweitsymbol)) { c.Append(zweitsymbol); }
            return c.ToString().TrimEnd('|');
        }

        public static QuickImage Get(QuickImage imageCode, enImageCodeEffect additionalState) => additionalState == enImageCodeEffect.Ohne ? imageCode
: Get(GenerateCode(imageCode.Name, imageCode.Width, imageCode.Height, imageCode.Effekt | additionalState, imageCode.Färbung, imageCode.ChangeGreenTo, imageCode.Sättigung, imageCode.Helligkeit, imageCode.DrehWinkel, imageCode.Transparenz, imageCode.Zweitsymbol));

        public static QuickImage Get(string imageCode) {
            if (string.IsNullOrEmpty(imageCode)) { return null; }

            QuickImage x;
            lock (Locker) {
                if (_pics.TryGetValue(imageCode, out var p)) { return p; }
                x = new QuickImage(imageCode, false);
                _pics.Add(imageCode, x);
            }

            x.Generate();
            return x;
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

        public static QuickImage Get(enFileFormat file, int size) => Get(FileTypeImage(file), size);

        public static implicit operator Bitmap(QuickImage p) {
            //if (!p._generated && !p._generating) { p.Generate(); }
            Bitmap bmp;
            var tim = DateTime.Now;

            do {
                if (DateTime.Now.Subtract(tim).TotalSeconds > 5) { return null; }

                try { // .... wir an anderer Stelle verwendet....
                    bmp = (BitmapExt)p;
                    if (bmp == null || bmp.Width < p.Width) { bmp = null; }
                } catch { bmp = null; }
            } while (bmp == null);

            return bmp;
        }

        public string CompareKey() => ToString();

        public void GenerateErrorImage() {
            IsError = true;

            EmptyBitmap(Width, Height);
            using var GR = Graphics.FromImage(this);
            GR.Clear(Color.Black);
            GR.DrawLine(new Pen(Color.Red, 3), 0, 0, Width - 1, Height - 1);
            GR.DrawLine(new Pen(Color.Red, 3), Width - 1, 0, 0, Height - 1);
        }

        public void OnNeedImage(NeedImageEventArgs e) => NeedImage?.Invoke(this, e);

        public string ReadableText() => string.Empty;

        public QuickImage SymbolForReadableText() => this;

        public override string ToString() => Code;

        private void CorrectSize(int width, int height, Bitmap bmp) {
            if (width > 0 && height > 0) {
                Width = width;
                Height = height;
            } else if (width > 0) {
                Width = width;
                Height = width;
            } else if (height > 0) {
                Width = height;
                Height = height;
            } else if (bmp != null) {
                Width = bmp.Width;
                Height = bmp.Height;
            } else {
                Width = 16;
                Height = 16;
            }
        }

        private void Generate() {
            var bmpOri = GetBitmap(Name);

            #region Fehlerhaftes Bild erzeugen

            if (bmpOri == null) {
                GenerateErrorImage();
                return;
            }

            #endregion

            #region Bild ohne besonderen Effekte, schnell abhandeln

            if (bmpOri != null && Effekt == enImageCodeEffect.Ohne && string.IsNullOrEmpty(ChangeGreenTo) && string.IsNullOrEmpty(Färbung) && Sättigung == 100 && Helligkeit == 100 && Transparenz == 100 && string.IsNullOrEmpty(Zweitsymbol)) {
                CloneFromBitmap(bmpOri);
                Resize(Width, Height, enSizeModes.EmptySpace, InterpolationMode.High, false);
                return;
            }

            #endregion

            #region Attribute in Variablen umsetzen

            Color? colgreen = null;
            Color? colfärb = null;
            Bitmap bmpKreuz = null;
            Bitmap bmpSecond = null;

            if (!string.IsNullOrEmpty(ChangeGreenTo)) { colgreen = ChangeGreenTo.FromHTMLCode(); }
            if (!string.IsNullOrEmpty(Färbung)) { colfärb = Färbung.FromHTMLCode(); }

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

            #endregion

            var bmpTMP = new BitmapExt(bmpOri.Width, bmpOri.Height);

            #region Bild Pixelgerecht berechnen

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

            #endregion

            bmpTMP.Resize(Width, Height, enSizeModes.EmptySpace, InterpolationMode.High, false);
            CloneFromBitmap(bmpTMP);
        }

        private Bitmap GetBitmap(string tmpname) {
            var vbmp = GetEmmbedBitmap(Assembly.GetAssembly(typeof(QuickImage)), tmpname + ".png");
            if (vbmp != null) { return vbmp; }

            if (_pics.TryGetValue(tmpname, out var p) && p != this) {
                if (p.IsError) { return null; }
                return p;
            }

            NeedImageEventArgs e = new(tmpname);
            OnNeedImage(e);

            // Evtl. hat die "OnNeedImage" das Bild auch in den Stack hochgeladen
            // Falls nicht, hier noch erledigen
            if (Exists(tmpname) && Get(tmpname) != this) { return Get(tmpname); }

            return e.BMP;
        }

        #endregion
    }
}