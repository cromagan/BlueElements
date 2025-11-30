// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Text;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;

namespace BlueBasics;

public sealed class QuickImage : IReadableText, IEditable {

    #region Fields

    /// <summary>
    /// QuickImages werden immer in den Speicher für spätere Zugriffe aufgenommen!
    /// </summary>
    private static readonly object _picsLock = new object();

    private static readonly ConcurrentDictionary<string, QuickImage> Pics = [];
    private readonly Bitmap _bitmap;

    #endregion

    #region Constructors

    /// <summary>
    /// QuickImages werden immer in den Speicher für spätere Zugriffe aufgenommen!
    /// </summary>
    public QuickImage(string imageCode) : base() {
        var w = (imageCode + "||||||||||").Split('|');

        Name = w[0];
        var width = IntParse(w[1]);
        var height = IntParse(w[2]);

        Effekt = (ImageCodeEffect)IntParse(w[3]);
        Färbung = string.IsNullOrEmpty(w[4]) ? null : ColorParse(w[4]);
        ChangeGreenTo = string.IsNullOrEmpty(w[5]) ? null : ColorParse(w[5]);
        Helligkeit = string.IsNullOrEmpty(w[6]) ? 100 : IntParse(w[6]);
        Sättigung = string.IsNullOrEmpty(w[7]) ? 100 : IntParse(w[7]);
        DrehWinkel = string.IsNullOrEmpty(w[8]) ? 0 : IntParse(w[8]);
        Transparenz = string.IsNullOrEmpty(w[9]) ? 0 : IntParse(w[9]);
        Zweitsymbol = string.IsNullOrEmpty(w[10]) ? string.Empty : w[10];

        CorrectSize(width, height, null);

        Code = imageCode;

        (_bitmap, IsError) = Generate();

        Pics.TryAdd(Code, this);
    }

    public QuickImage(string name, Bitmap bmp) {
        if (string.IsNullOrEmpty(name)) {
            _bitmap = GenerateErrorImage(5, 5);
            IsError = true;
            return;
        }

        if (Exists(name)) {
            Develop.DebugPrint(ErrorType.Warning, "Doppeltes Bild:" + name);
        }
        if (name.Contains("|")) {
            Develop.DebugPrint(ErrorType.Warning, "Fehlerhafter Name:" + name);
        }

        Name = name;
        Code = Name;

        CorrectSize(-1, -1, bmp);

        // Thread-sicheres Hinzufügen zur Collection
        lock (_picsLock) {
            Pics.TryAdd(Code, this);
        }

        _bitmap = bmp.CloneFromBitmap();
    }

    #endregion

    #region Events

    public static event EventHandler<NeedImageEventArgs>? NeedImage;

    #endregion

    #region Properties

    public string CaptionForEditor => "Bild";
    public Color? ChangeGreenTo { get; }
    public string Code { get; } = string.Empty;
    public int DrehWinkel { get; }
    public Type? Editor { get; set; }
    public ImageCodeEffect Effekt { get; } = ImageCodeEffect.Ohne;
    public Color? Färbung { get; }
    public int Height { get; private set; }
    public int Helligkeit { get; }
    public bool IsError { get; }
    public string Name { get; } = string.Empty;
    public int Sättigung { get; }

    public int Transparenz { get; }

    public int Width { get; private set; }

    public string Zweitsymbol { get; } = string.Empty;

    #endregion

    #region Methods

    public static bool Exists(string imageCode) => !string.IsNullOrEmpty(imageCode) && Pics.ContainsKey(imageCode);

    public static ImageCode FileTypeImage(FileFormat file) {
        switch (file) {
            case FileFormat.WordKind:
                return ImageCode.Word;

            case FileFormat.CSV:
            case FileFormat.ExcelKind:
                return ImageCode.Excel;

            case FileFormat.PowerPointKind:
                return ImageCode.PowerPoint;

            case FileFormat.Textdocument:
                return ImageCode.Textdatei;

            case FileFormat.EMail:
                return ImageCode.Brief;

            case FileFormat.Pdf:
                return ImageCode.PDF;

            case FileFormat.HTML:
                return ImageCode.Globus;

            case FileFormat.Image:
                return ImageCode.Bild;

            case FileFormat.CompressedArchive:
                return ImageCode.Karton;

            case FileFormat.Movie:
                return ImageCode.Filmrolle;

            case FileFormat.Executable:
                return ImageCode.Anwendung;

            case FileFormat.HelpFile:
                return ImageCode.Frage;

            case FileFormat.Table:
                return ImageCode.Tabelle;

            case FileFormat.XMLFile:
                return ImageCode.XML;

            case FileFormat.Visitenkarte:
                return ImageCode.Visitenkarte;

            case FileFormat.Sound:
                return ImageCode.Note;

            case FileFormat.Unknown:
                return ImageCode.Datei;

            case FileFormat.ProgrammingCode:
                return ImageCode.Skript;

            case FileFormat.Link:
                return ImageCode.Undo;

            case FileFormat.BlueCreativeFile:
                return ImageCode.Smiley;

            case FileFormat.BlueCreativeSymbol:
                return ImageCode.Smiley;

            case FileFormat.Icon:
                return ImageCode.Bild;

            default:
                Develop.DebugPrint(file);
                return ImageCode.Datei;
        }
    }

    public static string GenerateCode(string name, int width, int height, ImageCodeEffect effekt, Color? färbung, Color? changeGreenTo, int sättigung, int helligkeit, int drehwinkel, int transparenz, string? zweitsymbol) {
        var c = new StringBuilder();

        c.Append(name);
        c.Append('|');
        if (width > 0) { c.Append(width); }
        c.Append('|');
        if (height > 0 && width != height) { c.Append(height); }
        c.Append('|');
        if (effekt != ImageCodeEffect.Ohne) { c.Append((int)effekt); }
        c.Append('|');
        if (färbung.HasValue && färbung.Value != Color.Transparent) { c.Append(färbung.Value.ToHtmlCode()); }

        c.Append('|');
        if (changeGreenTo.HasValue && changeGreenTo.Value != Color.Transparent) { c.Append(changeGreenTo.Value.ToHtmlCode()); }
        c.Append('|');
        if (helligkeit != 100) { c.Append(helligkeit); }
        c.Append('|');
        if (sättigung != 100) { c.Append(sättigung); }
        c.Append('|');
        if (drehwinkel > 0) { c.Append(drehwinkel); }
        c.Append('|');
        if (transparenz > 0) { c.Append(transparenz); }
        c.Append('|');
        if (!string.IsNullOrEmpty(zweitsymbol)) { c.Append(zweitsymbol); }
        return c.ToString().TrimEnd('|');
    }

    public static QuickImage Get(QuickImage qi, ImageCodeEffect additionalState) => additionalState == ImageCodeEffect.Ohne ? qi
            : Get(GenerateCode(qi.Name, qi.Width, qi.Height, qi.Effekt | additionalState, qi.Färbung, qi.ChangeGreenTo, qi.Sättigung, qi.Helligkeit, qi.DrehWinkel, qi.Transparenz, qi.Zweitsymbol));

    public static QuickImage Get(string code) => Pics.TryGetValue(code, out var p) ? p : new QuickImage(code);

    public static QuickImage Get(string image, int squareWidth) {
        //if (string.IsNullOrEmpty(image)) { return null; }
        if (image.Contains("|")) {
            var w = (image + "||||||").Split('|');
            w[1] = squareWidth.ToString();
            w[2] = string.Empty;
            return Get(w.JoinWith("|"));
        }
        return Get(GenerateCode(image, squareWidth, 0, ImageCodeEffect.Ohne, null, null, 100, 100, 0, 0, string.Empty));
    }

    public static QuickImage Get(ImageCode image) => Get(image, 16);

    public static QuickImage Get(ImageCode image, int squareWidth) => Get(GenerateCode(Enum.GetName(image.GetType(), image), squareWidth, 0, ImageCodeEffect.Ohne, null, null, 100, 100, 0, 0, string.Empty));

    public static QuickImage Get(ImageCode image, int squareWidth, Color färbung, Color changeGreenTo, int helligkeit) => Get(GenerateCode(Enum.GetName(image.GetType(), image), squareWidth, 0, ImageCodeEffect.Ohne, färbung, changeGreenTo, 100, helligkeit, 0, 0, string.Empty));

    public static QuickImage Get(ImageCode image, int squareWidth, Color färbung, Color changeGreenTo) => Get(GenerateCode(Enum.GetName(image.GetType(), image), squareWidth, 0, ImageCodeEffect.Ohne, färbung, changeGreenTo, 100, 100, 0, 0, string.Empty));

    public static QuickImage Get(FileFormat file, int size) => Get(FileTypeImage(file), size);

    public static ReadOnlyCollection<string> Images() {
        var type = typeof(ImageCode);
        var l = new List<string>();

        foreach (int z1 in Enum.GetValues(type)) {
            var n = Enum.GetName(type, z1);
            if (n != null) {
                l.Add(n);
            }
        }

        return l.AsReadOnly();
    }

    public static implicit operator Bitmap(QuickImage qi) => qi._bitmap;

    public string IsNowEditable() => string.Empty;

    public void OnNeedImage(NeedImageEventArgs e) => NeedImage?.Invoke(this, e);

    public string ReadableText() => string.Empty;

    public QuickImage Scale(float zoom) {
        if (Math.Abs(zoom - 1f) < DefaultTolerance) { return this; }

        zoom = Math.Max(zoom, 0.001f);

        return Get(GenerateCode(Name, Width.CanvasToControl(zoom), Height.CanvasToControl(zoom), Effekt, Färbung, ChangeGreenTo, Sättigung, Helligkeit, DrehWinkel, Transparenz, Zweitsymbol));
    }

    public QuickImage SymbolForReadableText() => this;

    /// <summary>
    /// Gibt den ImageCode zurück
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Code;

    private static Bitmap GenerateErrorImage(int width, int height) {
        var bmp = new Bitmap(width, height);

        using var gr = Graphics.FromImage(bmp);
        gr.Clear(Color.Black);
        gr.DrawLine(new Pen(Color.Red, 3), 0, 0, width - 1, height - 1);
        gr.DrawLine(new Pen(Color.Red, 3), width - 1, 0, 0, height - 1);
        return bmp;
    }

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

    private (Bitmap bmp, bool isError) Generate() {
        var bmpOri = GetEmmbedBitmap(Assembly.GetAssembly(typeof(QuickImage)), Name + ".png");

        if (bmpOri == null && Pics.TryGetValue(Name, out var p) && p != this) {
            if (p.IsError) { return (p._bitmap, true); }
        }

        if (bmpOri == null) {
            NeedImageEventArgs e = new(Name);
            OnNeedImage(e);
            if (e is { Done: true, Bmp: not null }) { bmpOri = e.Bmp; }
        }

        if (bmpOri == null) {
            return (GenerateErrorImage(Width, Height), true);
        }

        bmpOri = bmpOri.Resize(Width, Height, SizeModes.EmptySpace, InterpolationMode.High, false);

        #region Bild ohne besonderen Effekte, schnell abhandeln

        if (Effekt == ImageCodeEffect.Ohne &&
            !ChangeGreenTo.HasValue &&
            !Färbung.HasValue &&
            Sättigung == 100 &&
            Helligkeit == 100 &&
            Transparenz == 0 &&
            string.IsNullOrEmpty(Zweitsymbol)) {
            return (bmpOri, false);
        }

        #endregion

        #region Attribute in Variablen umsetzen

        BitmapExt? bmpKreuz = null;
        BitmapExt? bmpSecond = null;
        var bmpOriE = new BitmapExt(bmpOri);

        if (Effekt.HasFlag(ImageCodeEffect.Durchgestrichen)) {
            var tmpEx = Effekt ^ ImageCodeEffect.Durchgestrichen;
            var n = "Kreuz|" + bmpOriE.Width + "|";
            if (bmpOriE.Width != bmpOriE.Height) { n += bmpOriE.Height; }
            n += "|";
            if (tmpEx != ImageCodeEffect.Ohne) { n += (int)tmpEx; }
            bmpKreuz = new BitmapExt(Get(n.Trim("|")));
        }
        if (!string.IsNullOrEmpty(Zweitsymbol)) {
            var siz = Math.Max(bmpOriE.Width / 3, bmpOriE.Height / 3);
            siz = Math.Max(siz, 10);
            siz = Math.Min(Math.Min(siz, bmpOriE.Width), bmpOriE.Height);

            bmpSecond = new BitmapExt(Get(Zweitsymbol + "|" + siz));
        }

        #endregion

        var bmpTmp = new BitmapExt(bmpOriE.Width, bmpOriE.Height);

        #region Bild Pixelgerecht berechnen

        for (var x = 0; x < bmpOriE.Width; x++) {
            for (var y = 0; y < bmpOriE.Height; y++) {
                var c = bmpOriE.GetPixel(x, y);

                if (bmpSecond != null) {
                    var secx = x - (bmpOriE.Width - bmpSecond.Width);
                    var secy = y - (bmpOriE.Height - bmpSecond.Height);

                    var c2 = bmpSecond.GetPixel(secx, secy);
                    if (!c2.IsMagentaOrTransparent()) {
                        c = c2;
                    } else {
                        if (bmpSecond.GetPixel(secx + 1, secy + 1).A > 128) {
                            c = Color.Transparent;
                        } else if (bmpSecond.GetPixel(secx + 1, secy).A > 128) {
                            c = Color.Transparent;
                        } else if (bmpSecond.GetPixel(secx, secy + 1).A > 128) {
                            c = Color.Transparent;
                        } else if (bmpSecond.GetPixel(secx - 1, secy - 1).A > 128) {
                            c = Color.Transparent;
                        } else if (bmpSecond.GetPixel(secx - 1, secy).A > 128) {
                            c = Color.Transparent;
                        } else if (bmpSecond.GetPixel(secx, secy - 1).A > 128) {
                            c = Color.Transparent;
                        }
                    }
                }

                if (c.IsMagentaOrTransparent()) {
                    c = Color.FromArgb(0, 0, 0, 0);
                } else {
                    if (ChangeGreenTo.HasValue && c.ToArgb() == -16711936) { c = ChangeGreenTo.Value; }
                    if (Färbung is { A: > 0 } cf) {
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
                    if (bmpKreuz != null) {
                        if (c.IsMagentaOrTransparent()) {
                            c = bmpKreuz.GetPixel(x, y);
                        } else {
                            if (bmpKreuz.GetPixel(x, y).A > 0) {
                                c = bmpKreuz.GetPixel(x, y).MixColor(c, 0.5);
                            }
                        }
                    }
                }
                if (!c.IsMagentaOrTransparent() && Transparenz is > 0 and < 100) {
                    c = Color.FromArgb((int)(c.A * (100 - Transparenz) / 100.0), c.R, c.G, c.B);
                }
                if (Effekt.HasFlag(ImageCodeEffect.WindowsMEDisabled)) {
                    var c1 = Color.FromArgb(0, 0, 0, 0);
                    if (!c.IsMagentaOrTransparent()) {
                        var randPixel = (x > 0 && bmpOriE.GetPixel(x - 1, y).IsMagentaOrTransparent()) ||
                                             (y > 0 && bmpOriE.GetPixel(x, y - 1).IsMagentaOrTransparent()) ||
                                             (x < bmpOriE.Width - 1 && bmpOriE.GetPixel(x + 1, y).IsMagentaOrTransparent()) ||
                                             (y < bmpOriE.Height - 1 && bmpOriE.GetPixel(x, y + 1).IsMagentaOrTransparent());

                        if (c.B < 128 || randPixel) {
                            c1 = SystemColors.ControlDark;
                            if (x < bmpOriE.Width - 1 && y < bmpOriE.Height - 1 && bmpOriE.GetPixel(x + 1, y + 1).IsMagentaOrTransparent()) {
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

        var bmp = bmpTmp.CloneOfBitmap()?.Resize(Width, Height, SizeModes.EmptySpace, InterpolationMode.High, false);

        return bmp == null ? (new Bitmap(Width, Height), true) : (bmp, false);
    }

    #endregion
}