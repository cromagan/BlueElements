// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.Abstract;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.ItemCollectionPad;

public sealed class BitmapPadItem : RectanglePadItem, ICanHaveVariables {

    #region Fields

    public readonly List<QuickImage> Overlays;

    public int Padding;

    #endregion

    #region Constructors

    public BitmapPadItem() : this(string.Empty, null, Size.Empty) { }

    public BitmapPadItem(string keyName, Bitmap? bmp, Size size) : base(keyName) {
        Bitmap = bmp;
        SetCoordinates(new RectangleF(0, 0, size.Width, size.Height), true);
        Overlays = [];
        Hintergrund_Weiß_Füllen = true;
        Padding = 0;
        Bild_Modus = SizeModes.EmptySpace;
        Stil = PadStyles.Undefiniert; // Kein Rahmen
    }

    #endregion

    #region Properties

    public static string ClassId => "IMAGE";

    public SizeModes Bild_Modus { get; set; }

    public Bitmap? Bitmap { get; set; }

    public override string Description => string.Empty;

    public bool Hintergrund_Weiß_Füllen { get; set; }

    public override string MyClassId => ClassId;

    [Description("Hier kann ein Variablenname als Platzhalter eingegeben werden. Beispiel: ~Bild~")]
    public string Platzhalter_Für_Layout { get; set; } = string.Empty;

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public void Bildschirmbereich_wählen() {
        if (Bitmap != null) {
            if (Forms.MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        }
        Bitmap = ScreenShot.GrabArea(null).CloneOfBitmap();
    }

    public void Datei_laden() {
        if (Bitmap != null) {
            if (Forms.MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        }
        OpenFileDialog e = new() {
            CheckFileExists = true,
            Multiselect = false,
            Title = "Bild wählen:",
            Filter = "PNG Portable Network Graphics|*.png|JPG Jpeg Interchange|*.jpg|Bmp Windows Bitmap|*.bmp"
        };
        _ = e.ShowDialog();

        if (!FileExists(e.FileName)) { return; }
        Bitmap = (Bitmap?)Image_FromFile(e.FileName);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        var comms = new List<AbstractListItem>();
        comms.Add(ItemOf("Abschneiden", ((int)SizeModes.BildAbschneiden).ToString(), QuickImage.Get("BildmodusAbschneiden|32")));
        comms.Add(ItemOf("Verzerren", ((int)SizeModes.Verzerren).ToString(), QuickImage.Get("BildmodusVerzerren|32")));
        comms.Add(ItemOf("Einpassen", ((int)SizeModes.EmptySpace).ToString(), QuickImage.Get("BildmodusEinpassen|32")));

        List<GenericControl> result =
        [
            new FlexiControlForDelegate(Bildschirmbereich_wählen, "Bildschirmbereich wählen", ImageCode.Bild),
            new FlexiControlForDelegate(Datei_laden, "Bild laden", ImageCode.Ordner),
            new FlexiControl(),
            new FlexiControlForProperty<string>(() => Platzhalter_Für_Layout, 2),
            new FlexiControl()
        ];
        result.Add(new FlexiControlForProperty<SizeModes>(() => Bild_Modus, comms));
        result.Add(new FlexiControl());
        result.Add(new FlexiControlForProperty<PadStyles>(() => Stil, Skin.GetRahmenArt(Parent?.SheetStyle, true)));

        result.Add(new FlexiControlForProperty<bool>(() => Hintergrund_Weiß_Füllen));

        result.Add(new FlexiControl());
        result.AddRange(base.GetProperties(widthOfControl));

        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "stretchallowed": // ALT
                return true;

            case "modus":
                Bild_Modus = (SizeModes)IntParse(value);
                return true;

            case "whiteback":
                Hintergrund_Weiß_Füllen = value.FromPlusMinus();
                return true;

            case "padding":
                Padding = IntParse(value);
                return true;

            case "image":
                Bitmap = Base64ToBitmap(value);
                return true;

            case "placeholder":
                Platzhalter_Für_Layout = value.FromNonCritical();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Bild";

    public bool ReplaceVariable(Variable variable) {
        if (IsDisposed) { return false; }
        if (string.IsNullOrEmpty(Platzhalter_Für_Layout)) { return false; }
        if ("~" + variable.KeyName.ToLowerInvariant() + "~" != Platzhalter_Für_Layout.ToLowerInvariant()) { return false; }

        Bitmap? ot;

        if (variable is VariableBitmap vbmp) {
            ot = vbmp.ValueBitmap;
        } else if (variable is VariableString filn) {
            if (FileExists(filn.ValueString)) {
                ot = Image_FromFile(filn.ValueString) as Bitmap;
            } else {
                return false;
            }
        } else {
            return false;
        }

        if (ot != null) {
            Bitmap = ot;
            OnPropertyChanged();
            return true;
        }

        return false;
    }

    public bool ResetVariables() {
        if (IsDisposed) { return false; }
        if (!string.IsNullOrEmpty(Platzhalter_Für_Layout) && Bitmap != null) {
            Bitmap?.Dispose();
            Bitmap = null;
            OnPropertyChanged();
            return true;
        }
        return false;
    }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Bild, 16);

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [];

        result.ParseableAdd("Modus", Bild_Modus);
        result.ParseableAdd("Placeholder", Platzhalter_Für_Layout);
        result.ParseableAdd("WhiteBack", Hintergrund_Weiß_Füllen);
        result.ParseableAdd("Overlays", "Overlay", Overlays);

        result.ParseableAdd("Padding", Padding);
        result.ParseableAdd("Image", Bitmap);

        return result.Parseable(base.ToParseableString());
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            if (Bitmap != null) {
                Bitmap?.Dispose();
                Bitmap = null;
            }

            //IsDisposed = true;
        }
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        positionModified.Inflate(-Padding, -Padding);
        RectangleF r1 = new(positionModified.Left + Padding, positionModified.Top + Padding,
            positionModified.Width - (Padding * 2), positionModified.Height - (Padding * 2));
        RectangleF r2 = new();
        RectangleF r3 = new();
        if (Bitmap != null) {
            r3 = new RectangleF(0, 0, Bitmap.Width, Bitmap.Height);
            switch (Bild_Modus) {
                case SizeModes.Verzerren: {
                        r2 = r1;
                        break;
                    }

                case SizeModes.BildAbschneiden: {
                        var scale = Math.Max((positionModified.Width - (Padding * 2)) / Bitmap.Width, (positionModified.Height - (Padding * 2)) / Bitmap.Height);
                        var tmpw = (positionModified.Width - (Padding * 2)) / scale;
                        var tmph = (positionModified.Height - (Padding * 2)) / scale;
                        r3 = new RectangleF((Bitmap.Width - tmpw) / 2, (Bitmap.Height - tmph) / 2, tmpw, tmph);
                        r2 = r1;
                        break;
                    }
                default: // Is = enSizeModes.WeißerRand
                {
                        var scale = Math.Min((positionModified.Width - (Padding * 2)) / Bitmap.Width, (positionModified.Height - (Padding * 2)) / Bitmap.Height);
                        r2 = new RectangleF(((positionModified.Width - (Bitmap.Width * scale)) / 2) + positionModified.Left, ((positionModified.Height - (Bitmap.Height * scale)) / 2) + positionModified.Top, Bitmap.Width * scale, Bitmap.Height * scale);
                        break;
                    }
            }
        }
        var trp = positionModified.PointOf(Alignment.Horizontal_Vertical_Center);
        gr.TranslateTransform(trp.X, trp.Y);
        gr.RotateTransform(-Drehwinkel);
        r1 = r1 with { X = r1.Left - trp.X, Y = r1.Top - trp.Y };
        r2 = r2 with { X = r2.Left - trp.X, Y = r2.Top - trp.Y };
        if (Hintergrund_Weiß_Füllen) {
            gr.FillRectangle(Brushes.White, r1);
        }
        try {
            if (Bitmap != null) {
                if (forPrinting) {
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                } else {
                    gr.InterpolationMode = InterpolationMode.Low;
                    gr.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                }
                gr.DrawImage(Bitmap, r2, r3, GraphicsUnit.Pixel);
            }
        } catch {
            Generic.CollectGarbage();
        }
        if (Stil != PadStyles.Undefiniert) {
            if (Parent is { SheetStyle: not null, SheetStyleScale: > 0 }) {
                gr.DrawRectangle(Skin.GetBlueFont(Stil, Parent.SheetStyle).Pen(zoom * Parent.SheetStyleScale), r1);
            }
        }
        foreach (var thisQi in Overlays) {
            gr.DrawImage(thisQi, r2.Left + 8, r2.Top + 8);
        }
        gr.TranslateTransform(-trp.X, -trp.Y);
        gr.ResetTransform();
        if (!forPrinting) {
            if (!string.IsNullOrEmpty(Platzhalter_Für_Layout)) {
                Font f = new("Arial", 8);
                BlueFont.DrawString(gr, Platzhalter_Für_Layout, f, Brushes.Black, positionModified.Left, positionModified.Top);
            }
        }
        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    #endregion

}