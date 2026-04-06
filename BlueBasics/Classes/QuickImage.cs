// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics.Classes.BitmapExt_ImageFilters;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;
using static BlueBasics.ClassesStatic.Constants;
using static BlueBasics.ClassesStatic.Converter;
using static BlueBasics.Extensions;

namespace BlueBasics.Classes;

public sealed class QuickImage : IReadableText, IEditable {

    #region Fields

    /// <summary>
    /// QuickImages werden immer in den Speicher für spätere Zugriffe aufgenommen!
    /// </summary>
    private static readonly object _picsLock = new object();

    private static readonly ConcurrentDictionary<string, QuickImage> Pics = new(StringComparer.OrdinalIgnoreCase);
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

        KeyName = imageCode;

        (_bitmap, IsError) = Generate();

        Pics.TryAdd(KeyName, this);
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
        KeyName = Name;

        CorrectSize(-1, -1, bmp);

        // Thread-sicheres Hinzufügen zur Collection
        lock (_picsLock) {
            Pics.TryAdd(KeyName, this);
        }

        _bitmap = bmp.CloneFromBitmap();
    }

    #endregion

    #region Properties

    public static string CachePfad { get; set; } = string.Empty;

    public string CaptionForEditor => "Bild";
    public Color? ChangeGreenTo { get; }
    public int DrehWinkel { get; }
    public ImageCodeEffect Effekt { get; } = ImageCodeEffect.None;
    public Color? Färbung { get; }
    public int Height { get; private set; }
    public int Helligkeit { get; }
    public string HTMLCode => $"<Imagecode={KeyName}>";
    public bool IsError { get; }
    public string KeyName { get; } = string.Empty;
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
        if (effekt != ImageCodeEffect.None) { c.Append((int)effekt); }
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

    public static QuickImage Get(QuickImage qi, ImageCodeEffect additionalState) => additionalState == ImageCodeEffect.None ? qi
            : Get(GenerateCode(qi.Name, qi.Width, qi.Height, qi.Effekt | additionalState, qi.Färbung, qi.ChangeGreenTo, qi.Sättigung, qi.Helligkeit, qi.DrehWinkel, qi.Transparenz, qi.Zweitsymbol));

    public static QuickImage Get(string code) => Pics.TryGetValue(code, out var p) ? p : new QuickImage(code);

    public static QuickImage Get(string image, int squareWidth) {
        //if (string.IsNullOrEmpty(image)) { return null; }
        if (image.Contains("|")) {
            var w = (image + "||||||").Split('|');
            w[1] = squareWidth.ToString1();
            w[2] = string.Empty;
            return Get(w.JoinWith("|"));
        }
        return Get(GenerateCode(image, squareWidth, 0, ImageCodeEffect.None, null, null, 100, 100, 0, 0, string.Empty));
    }

    public static QuickImage Get(ImageCode image) => Get(image, 16);

    public static QuickImage Get(ImageCode image, int squareWidth) => Get(GenerateCode(Enum.GetName(image.GetType(), image), squareWidth, 0, ImageCodeEffect.None, null, null, 100, 100, 0, 0, string.Empty));

    public static QuickImage Get(ImageCode image, int squareWidth, Color färbung, Color changeGreenTo, int helligkeit) => Get(GenerateCode(Enum.GetName(image.GetType(), image), squareWidth, 0, ImageCodeEffect.None, färbung, changeGreenTo, 100, helligkeit, 0, 0, string.Empty));

    public static QuickImage Get(ImageCode image, int squareWidth, Color färbung, Color changeGreenTo) => Get(GenerateCode(Enum.GetName(image.GetType(), image), squareWidth, 0, ImageCodeEffect.None, färbung, changeGreenTo, 100, 100, 0, 0, string.Empty));

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
    public override string ToString() => KeyName;

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

        if (bmpOri == null && !string.IsNullOrWhiteSpace(CachePfad)) {
            var fullname = CachePfad.NormalizePath() + Name.RemoveChars(Char_DateiSonderZeichen) + ".PNG";
            if (IO.FileExists(fullname)) {
                if (Image_FromFile(fullname) is Bitmap bmpCache) { bmpOri = bmpCache; }
            }
        }

        if (bmpOri == null) {
            return (GenerateErrorImage(Width, Height), true);
        }

        bmpOri = bmpOri.Resize(Width, Height, SizeModes.EmptySpace, InterpolationMode.High, false);

        #region Bild ohne besonderen Effekte, schnell abhandeln

        if (Effekt == ImageCodeEffect.None &&
            !ChangeGreenTo.HasValue &&
            !Färbung.HasValue &&
            Sättigung == 100 &&
            Helligkeit == 100 &&
            Transparenz == 0 &&
            string.IsNullOrEmpty(Zweitsymbol)) {
            return (bmpOri, false);
        }

        #endregion

        var filters = new List<(ImageFilter filter, float factor, object? parameter)>();

        if (ChangeGreenTo.HasValue) {
            filters.Add((ImageFilter.AllFilters.GetByKey("ColorChange")!, 0, (Color.FromArgb(0, 128, 0), ChangeGreenTo.Value)));
        }
        if (Färbung is { } cf && cf.A > 0) {
            filters.Add((ImageFilter.AllFilters.GetByKey("Färbung")!, 0, cf));
        }
        if (Sättigung != 100 || Helligkeit != 100) {
            filters.Add((ImageFilter.AllFilters.GetByKey("SättigungHelligkeit")!, 0, (Sättigung, Helligkeit)));
        }
        if (Effekt.HasFlag(ImageCodeEffect.WindowsXPDisabled)) {
            filters.Add((ImageFilter.AllFilters.GetByKey("WindowsXPDisabled")!, 0, null));
        }
        if (Effekt.HasFlag(ImageCodeEffect.Graustufen)) {
            filters.Add((ImageFilter.AllFilters.GetByKey("Graustufen")!, 0, null));
        }
        if (Transparenz is > 0 and < 100) {
            filters.Add((ImageFilter.AllFilters.GetByKey("Transparenz")!, Transparenz, null));
        }

        var bmp = bmpOri.CloneFromBitmap();

        if (!string.IsNullOrEmpty(Zweitsymbol)) {
            bmp = ApplyZweitsymbol(bmp);
        }

        bmp.ApplyFilter(filters.ToArray());

        if (Effekt.HasFlag(ImageCodeEffect.Durchgestrichen)) {
            bmp = ApplyDurchgestrichen(bmp);
        }

        if (Effekt.HasFlag(ImageCodeEffect.WindowsMEDisabled)) {
            var oriRect = new Rectangle(0, 0, bmpOri.Width, bmpOri.Height);
            var oriData = bmpOri.LockBits(oriRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try {
                var oriBits = new byte[oriData.Stride * bmpOri.Height];
                Marshal.Copy(oriData.Scan0, oriBits, 0, oriBits.Length);
                bmp.ApplyFilter((ImageFilter.AllFilters.GetByKey("WindowsMEDisabled")!, 0, (oriData, oriBits)));
            } finally {
                bmpOri.UnlockBits(oriData);
            }
        }

        bmp = bmp.CloneFromBitmap()?.Resize(Width, Height, SizeModes.EmptySpace, InterpolationMode.High, false);

        return bmp == null ? (new Bitmap(Width, Height), true) : (bmp, false);
    }

    private Bitmap ApplyZweitsymbol(Bitmap bmp) {
        var oriW = bmp.Width;
        var oriH = bmp.Height;
        var siz = Math.Max(oriW / 3, oriH / 3);
        siz = Math.Max(siz, 10);
        siz = Math.Min(Math.Min(siz, oriW), oriH);

        using var bmpSecond = (Bitmap)Get(Zweitsymbol + "|" + siz);
        var secondW = bmpSecond.Width;
        var secondH = bmpSecond.Height;
        var lockArea = new Rectangle(0, 0, secondW, secondH);
        var secondData = bmpSecond.LockBits(lockArea, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var secondBits = new byte[secondData.Stride * secondH];
        Marshal.Copy(secondData.Scan0, secondBits, 0, secondBits.Length);
        try {
            var rect = new Rectangle(0, 0, oriW, oriH);
            var data = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var bits = new byte[data.Stride * oriH];
            Marshal.Copy(data.Scan0, bits, 0, bits.Length);
            try {
                for (var x = 0; x < oriW; x++) {
                    for (var y = 0; y < oriH; y++) {
                        var idx = y * data.Stride + x * 4;
                        var c = Color.FromArgb(bits[idx + 3], bits[idx + 2], bits[idx + 1], bits[idx]);

                        var secx = x - (oriW - secondW);
                        var secy = y - (oriH - secondH);

                        var c2 = GetPixelSafe(secondData, secondBits, secx, secy, secondW, secondH);
                        if (!c2.IsMagentaOrTransparent()) {
                            c = c2;
                        } else {
                            if (GetPixelSafe(secondData, secondBits, secx + 1, secy + 1, secondW, secondH).A > 128) {
                                c = Color.Transparent;
                            } else if (GetPixelSafe(secondData, secondBits, secx + 1, secy, secondW, secondH).A > 128) {
                                c = Color.Transparent;
                            } else if (GetPixelSafe(secondData, secondBits, secx, secy + 1, secondW, secondH).A > 128) {
                                c = Color.Transparent;
                            } else if (GetPixelSafe(secondData, secondBits, secx - 1, secy - 1, secondW, secondH).A > 128) {
                                c = Color.Transparent;
                            } else if (GetPixelSafe(secondData, secondBits, secx - 1, secy, secondW, secondH).A > 128) {
                                c = Color.Transparent;
                            } else if (GetPixelSafe(secondData, secondBits, secx, secy - 1, secondW, secondH).A > 128) {
                                c = Color.Transparent;
                            }
                        }

                        bits[idx] = c.B;
                        bits[idx + 1] = c.G;
                        bits[idx + 2] = c.R;
                        bits[idx + 3] = c.A;
                    }
                }
                Marshal.Copy(bits, 0, data.Scan0, bits.Length);
            } finally {
                bmp.UnlockBits(data);
            }
        } finally {
            bmpSecond.UnlockBits(secondData);
        }

        return bmp;
    }

    private Bitmap ApplyDurchgestrichen(Bitmap bmp) {
        var oriW = bmp.Width;
        var oriH = bmp.Height;
        var tmpEx = Effekt ^ ImageCodeEffect.Durchgestrichen;
        var n = "Kreuz|" + oriW + "|";
        if (oriW != oriH) { n += oriH; }
        n += "|";
        if (tmpEx != ImageCodeEffect.None) { n += (int)tmpEx; }

        using var bmpKreuz = (Bitmap)Get(n.Trim("|"));
        var kreuzW = bmpKreuz.Width;
        var kreuzH = bmpKreuz.Height;
        var lockArea = new Rectangle(0, 0, kreuzW, kreuzH);
        var kreuzData = bmpKreuz.LockBits(lockArea, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var kreuzBits = new byte[kreuzData.Stride * kreuzH];
        Marshal.Copy(kreuzData.Scan0, kreuzBits, 0, kreuzBits.Length);
        try {
            var rect = new Rectangle(0, 0, oriW, oriH);
            var data = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var bits = new byte[data.Stride * oriH];
            Marshal.Copy(data.Scan0, bits, 0, bits.Length);
            try {
                for (var x = 0; x < oriW; x++) {
                    for (var y = 0; y < oriH; y++) {
                        var idx = y * data.Stride + x * 4;
                        var c = Color.FromArgb(bits[idx + 3], bits[idx + 2], bits[idx + 1], bits[idx]);

                        if (c.IsMagentaOrTransparent()) {
                            c = GetPixelSafe(kreuzData, kreuzBits, x, y, kreuzW, kreuzH);
                        } else {
                            if (GetPixelSafe(kreuzData, kreuzBits, x, y, kreuzW, kreuzH).A > 0) {
                                c = GetPixelSafe(kreuzData, kreuzBits, x, y, kreuzW, kreuzH).MixColor(c, 0.5);
                            }
                        }

                        bits[idx] = c.B;
                        bits[idx + 1] = c.G;
                        bits[idx + 2] = c.R;
                        bits[idx + 3] = c.A;
                    }
                }
                Marshal.Copy(bits, 0, data.Scan0, bits.Length);
            } finally {
                bmp.UnlockBits(data);
            }
        } finally {
            bmpKreuz.UnlockBits(kreuzData);
        }

        return bmp;
    }

    private static Color GetPixelSafe(BitmapData? data, byte[]? bits, int x, int y, int w, int h) {
        if (data == null || bits == null || x < 0 || y < 0 || x >= w || y >= h) { return Color.FromArgb(0, 0, 0, 0); }
        return GetPixel(data, bits, x, y);
    }

    #endregion
}