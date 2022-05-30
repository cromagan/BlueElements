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

#nullable enable

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

namespace BlueBasics;

public sealed class QuickImage : BitmapExt, IReadableText {

    #region Fields

    public readonly string ChangeGreenTo;
    public readonly string Code;
    public readonly int DrehWinkel;
    public readonly ImageCodeEffect Effekt;
    public readonly string Färbung;
    public readonly int Helligkeit;
    public readonly string Name;
    public readonly int Sättigung;
    public readonly int Transparenz;
    public readonly string? Zweitsymbol;
    private static readonly object Locker = new();
    private static readonly Dictionary<string, QuickImage> Pics = new();

    #endregion

    #region Constructors

    /// <summary>
    /// QuickImages werden immer in den Speicher für spätere Zugriffe aufgenommen!
    /// </summary>
    public QuickImage(string imageCode, bool generate) : base() {
        var w = (imageCode + "||||||||||").Split('|');
        Name = w[0];
        var width = -1;
        if (!string.IsNullOrEmpty(w[1])) { IntTryParse(w[1], out width); }
        var height = -1;
        if (!string.IsNullOrEmpty(w[2])) { IntTryParse(w[2], out height); }
        CorrectSize(width, height, null);
        Effekt = ImageCodeEffect.Ohne;
        if (!string.IsNullOrEmpty(w[3])) {
            IntTryParse(w[3], out var tmp);
            Effekt = (ImageCodeEffect)tmp;
        }
        Färbung = w[4];
        ChangeGreenTo = w[5];
        Helligkeit = string.IsNullOrEmpty(w[6]) ? 100 : IntParse(w[6]);
        Sättigung = string.IsNullOrEmpty(w[7]) ? 100 : IntParse(w[7]);
        DrehWinkel = string.IsNullOrEmpty(w[8]) ? 0 : IntParse(w[8]);
        Transparenz = string.IsNullOrEmpty(w[9]) ? 0 : IntParse(w[9]);
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

        if (Exists(imagecode)) { Develop.DebugPrint(FehlerArt.Warnung, "Doppeltes Bild:" + imagecode); }
        if (imagecode.Contains("|")) { Develop.DebugPrint(FehlerArt.Warnung, "Fehlerhafter Name:" + imagecode); }

        Name = imagecode;
        Code = Name;
        lock (Locker) {
            CorrectSize(-1, -1, bmp);
            Pics.Add(Code, this);
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

    public bool IsError { get; private set; }

    public new int Width { get; private set; }

    #endregion

    #region Methods

    public static bool Exists(string imageCode) => !string.IsNullOrEmpty(imageCode) && Pics.ContainsKey(imageCode);

    public static ImageCode FileTypeImage(FileFormat file) {
        switch (file) {
            case FileFormat.WordKind: return ImageCode.Word;
            case FileFormat.ExcelKind: return ImageCode.Excel;
            case FileFormat.PowerPointKind: return ImageCode.PowerPoint;
            case FileFormat.Textdocument: return ImageCode.Textdatei;
            case FileFormat.EMail: return ImageCode.Brief;
            case FileFormat.Pdf: return ImageCode.PDF;
            case FileFormat.HTML: return ImageCode.Globus;
            case FileFormat.Image: return ImageCode.Bild;
            case FileFormat.CompressedArchive: return ImageCode.Karton;
            case FileFormat.Movie: return ImageCode.Filmrolle;
            case FileFormat.Executable: return ImageCode.Anwendung;
            case FileFormat.HelpFile: return ImageCode.Frage;
            case FileFormat.Database: return ImageCode.Datenbank;
            case FileFormat.XMLFile: return ImageCode.XML;
            case FileFormat.Visitenkarte: return ImageCode.Visitenkarte;
            case FileFormat.Sound: return ImageCode.Note;
            case FileFormat.Unknown: return ImageCode.Datei;
            case FileFormat.ProgrammingCode: return ImageCode.Skript;
            case FileFormat.Link: return ImageCode.Undo;
            case FileFormat.BlueCreativeFile: return ImageCode.Smiley;
            case FileFormat.Icon: return ImageCode.Bild;
            default:
                Develop.DebugPrint(file);
                return ImageCode.Datei;
        }
    }

    public static string GenerateCode(string name, int width, int height, ImageCodeEffect effekt, string färbung, string changeGreenTo, int sättigung, int helligkeit, int drehwinkel, int transparenz, string? zweitsymbol) {
        var c = new StringBuilder();

        c.Append(name);
        c.Append("|");
        if (width > 0) { c.Append(width); }
        c.Append("|");
        if (height > 0 && width != height) { c.Append(height); }
        c.Append("|");
        if (effekt != ImageCodeEffect.Ohne) { c.Append((int)effekt); }
        c.Append("|");
        if(färbung != "00ffffff") { c.Append(färbung); }
     
        c.Append("|");
        if (changeGreenTo != "00ffffff") {               c.Append(changeGreenTo);            }
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

    public static QuickImage? Get(QuickImage? imageCode, ImageCodeEffect additionalState) => additionalState == ImageCodeEffect.Ohne ? imageCode
        : Get(GenerateCode(imageCode.Name, imageCode.Width, imageCode.Height, imageCode.Effekt | additionalState, imageCode.Färbung, imageCode.ChangeGreenTo, imageCode.Sättigung, imageCode.Helligkeit, imageCode.DrehWinkel, imageCode.Transparenz, imageCode.Zweitsymbol));

    public static QuickImage? Get(string imageCode) {
        if (string.IsNullOrEmpty(imageCode)) { return null; }

        QuickImage? x;
        lock (Locker) {
            if (Pics.TryGetValue(imageCode, out var p)) { return p; }
            x = new QuickImage(imageCode, false);
            Pics.Add(imageCode, x);
        }

        x.Generate();
        return x;
    }

    public static QuickImage? Get(string image, int squareWidth) {
        if (string.IsNullOrEmpty(image)) { return null; }
        if (image.Contains("|")) {
            var w = (image + "||||||").Split('|');
            w[1] = squareWidth.ToString();
            w[2] = string.Empty;
            return Get(w.JoinWith("|"));
        }
        return Get(GenerateCode(image, squareWidth, 0, ImageCodeEffect.Ohne, string.Empty, string.Empty, 100, 100, 0, 0, string.Empty));
    }

    public static QuickImage? Get(ImageCode image) => Get(image, 16);

    public static QuickImage? Get(ImageCode image, int squareWidth) => image == ImageCode.None ? null
        : Get(GenerateCode(Enum.GetName(image.GetType(), image), squareWidth, 0, ImageCodeEffect.Ohne, string.Empty, string.Empty, 100, 100, 0, 0, string.Empty));

    public static QuickImage? Get(ImageCode image, int squareWidth, Color färbung, Color changeGreenTo) => image == ImageCode.None ? null
        : Get(GenerateCode(Enum.GetName(image.GetType(), image), squareWidth, 0, ImageCodeEffect.Ohne, färbung.ToHtmlCode(), changeGreenTo.ToHtmlCode(), 100, 100, 0, 0, string.Empty));

    public static QuickImage? Get(FileFormat file, int size) => Get(FileTypeImage(file), size);

    public static implicit operator Bitmap?(QuickImage? p) {
        //if (!p._generated && !p._generating) { p.Generate(); }
        Bitmap? bmp;
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
        using var gr = Graphics.FromImage(this);
        gr.Clear(Color.Black);
        gr.DrawLine(new Pen(Color.Red, 3), 0, 0, Width - 1, Height - 1);
        gr.DrawLine(new Pen(Color.Red, 3), Width - 1, 0, 0, Height - 1);
    }

    public void OnNeedImage(NeedImageEventArgs e) => NeedImage?.Invoke(this, e);

    public string ReadableText() => string.Empty;

    public QuickImage SymbolForReadableText() => this;

    public override string ToString() => Code;

    private void CorrectSize(int width, int height, Image? bmp) {
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

        if (bmpOri != null && Effekt == ImageCodeEffect.Ohne && string.IsNullOrEmpty(ChangeGreenTo) && string.IsNullOrEmpty(Färbung) && Sättigung == 100 && Helligkeit == 100 && Transparenz == 100 && string.IsNullOrEmpty(Zweitsymbol)) {
            CloneFromBitmap(bmpOri);
            Resize(Width, Height, SizeModes.EmptySpace, InterpolationMode.High, false);
            return;
        }

        #endregion

        #region Attribute in Variablen umsetzen

        Color? colgreen = null;
        Color? colfärb = null;
        Bitmap? bmpKreuz = null;
        Bitmap? bmpSecond = null;

        if (!string.IsNullOrEmpty(ChangeGreenTo)) { colgreen = ChangeGreenTo.FromHtmlCode(); }
        if (!string.IsNullOrEmpty(Färbung)) { colfärb = Färbung.FromHtmlCode(); }

        if (Effekt.HasFlag(ImageCodeEffect.Durchgestrichen)) {
            var tmpEx = Effekt ^ ImageCodeEffect.Durchgestrichen;
            var n = "Kreuz|" + bmpOri.Width + "|";
            if (bmpOri.Width != bmpOri.Height) { n += bmpOri.Height; }
            n += "|";
            if (tmpEx != ImageCodeEffect.Ohne) { n += (int)tmpEx; }
            bmpKreuz = Get(n.Trim("|"));
        }
        if (!string.IsNullOrEmpty(Zweitsymbol)) {
            var x = bmpOri.Width / 2;
            bmpSecond = Get(Zweitsymbol + "|" + x);
        }

        #endregion

        var bmpTmp = new BitmapExt(bmpOri.Width, bmpOri.Height);

        #region Bild Pixelgerecht berechnen

        for (var x = 0; x < bmpOri.Width; x++) {
            for (var y = 0; y < bmpOri.Height; y++) {
                var c = bmpOri.GetPixel(x, y);
                if (bmpSecond != null && x > bmpOri.Width - bmpSecond.Width && y > bmpOri.Height - bmpSecond.Height) {
                    var c2 = bmpSecond.GetPixel(x - (bmpOri.Width - bmpSecond.Width), y - (bmpOri.Height - bmpSecond.Height));
                    if (!c2.IsMagentaOrTransparent()) { c = c2; }
                }
                if (c.IsMagentaOrTransparent()) {
                    c = Color.FromArgb(0, 0, 0, 0);
                } else {
                    if (colgreen != null && c.ToArgb() == -16711936) { c = (Color)colgreen; }
                    if (colfärb is Color cf && cf.A > 0) {
                        c = cf.GetHue().FromHsb(cf.GetSaturation(), c.GetBrightness(), c.A);
                    }
                    if (Sättigung != 100 || Helligkeit != 100) { c = c.GetHue().FromHsb(c.GetSaturation() * Sättigung / 100, c.GetBrightness() * Helligkeit / 100, c.A); }
                    if (Effekt.HasFlag(ImageCodeEffect.WindowsXPDisabled)) {
                        var w = (int)(c.GetBrightness() * 100);
                        w = (int)(w / 2.8);
                        c = Extensions.FromHsb(0, 0, (float)((w / 100.0) + 0.5), c.A);
                    }
                    if (Effekt.HasFlag(ImageCodeEffect.Graustufen)) { c = c.ToGrey(); }
                }
                if (Effekt.HasFlag(ImageCodeEffect.Durchgestrichen)) {
                    if (c.IsMagentaOrTransparent()) {
                        c = bmpKreuz.GetPixel(x, y);
                    } else {
                        if (bmpKreuz.GetPixel(x, y).A > 0) { c = bmpKreuz.GetPixel(x, y).MixColor(c, 0.5); }
                    }
                }
                if (!c.IsMagentaOrTransparent() && Transparenz > 0 && Transparenz < 100) {
                    c = Color.FromArgb((int)(c.A * (100 - Transparenz) / 100.0), c.R, c.G, c.B);
                }
                if (Effekt.HasFlag(ImageCodeEffect.WindowsMEDisabled)) {
                    var c1 = Color.FromArgb(0, 0, 0, 0);
                    if (!c.IsMagentaOrTransparent()) {
                        var randPixel = x > 0 && bmpOri.GetPixel(x - 1, y).IsMagentaOrTransparent() || y > 0 && bmpOri.GetPixel(x, y - 1).IsMagentaOrTransparent();
                        if (x < bmpOri.Width - 1 && bmpOri.GetPixel(x + 1, y).IsMagentaOrTransparent()) { randPixel = true; }
                        if (y < bmpOri.Height - 1 && bmpOri.GetPixel(x, y + 1).IsMagentaOrTransparent()) { randPixel = true; }
                        if (c.B < 128 || randPixel) {
                            c1 = SystemColors.ControlDark;
                            if (x < bmpOri.Width - 1 && y < bmpOri.Height - 1 && bmpOri.GetPixel(x + 1, y + 1).IsMagentaOrTransparent()) {
                                c1 = SystemColors.ControlLightLight;
                            }
                        }
                    }
                    c = c1;
                }
                bmpTmp.SetPixel(x, y, c);
            }
        }

        #endregion

        bmpTmp.Resize(Width, Height, SizeModes.EmptySpace, InterpolationMode.High, false);
        CloneFromBitmap(bmpTmp);
    }

    private Bitmap? GetBitmap(string tmpname) {
        var vbmp = GetEmmbedBitmap(Assembly.GetAssembly(typeof(QuickImage)), tmpname + ".png");
        if (vbmp != null) { return vbmp; }

        if (Pics.TryGetValue(tmpname, out var p) && p != this) {
            return p.IsError ? null : (Bitmap)p;
        }

        NeedImageEventArgs e = new(tmpname);
        OnNeedImage(e);

        // Evtl. hat die "OnNeedImage" das Bild auch in den Stack hochgeladen
        // Falls nicht, hier noch erledigen
        return Exists(tmpname) && Get(tmpname) != this ? Get(tmpname) : (Bitmap)e.Bmp;
    }

    #endregion
}