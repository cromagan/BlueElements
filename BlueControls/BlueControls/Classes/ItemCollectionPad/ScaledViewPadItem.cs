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
using BlueControls.ItemCollectionPad.Abstract;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollectionPad;

public sealed class ScaledViewPadItem : FixedRectanglePadItem {

    #region Fields

    private Bitmap? _bitmap;
    private string _includedjointPoints = string.Empty;

    #endregion

    #region Constructors

    public ScaledViewPadItem() : base(string.Empty) {
    }

    #endregion

    #region Properties

    public static string ClassId => "SCALEDVIEW";

    public string Caption { get; internal set; }

    //public ScaledViewPadItem(string keyName, Bitmap? bmp, Size size) : base(keyName) {
    //    Bitmap = bmp;
    //    SetCoordinates(new RectangleF(0, 0, size.Width, size.Height), true);
    //    Overlays = [];
    //    Hintergrund_Weiß_Füllen = true;
    //    Padding = 0;
    //    Bild_Modus = SizeModes.EmptySpace;
    //    Stil = PadStyles.Undefiniert; // Kein Rahmen
    //}
    public override string Description => string.Empty;

    public string IncludedJointPoints {
        get {
            return _includedjointPoints;
        }

        set {
            if (_includedjointPoints != value) { return; }
            _includedjointPoints = value;
            OnPropertyChanged();
        }
    }

    public double Scale { get; internal set; }

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [

                       new FlexiControlForProperty<string>(() => IncludedJointPoints, 5),
            new FlexiControl()
        ];
        //result.Add(new FlexiControlForProperty<SizeModes>(() => Bild_Modus, comms));
        //result.Add(new FlexiControl());
        //result.Add(new FlexiControlForProperty<PadStyles>(() => Stil, Skin.GetRahmenArt(Parent?.SheetStyle, true)));

        //result.Add(new FlexiControlForProperty<bool>(() => Hintergrund_Weiß_Füllen));

        //result.Add(new FlexiControl());
        result.AddRange(base.GetProperties(widthOfControl));

        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("IncludedJointPoints", _includedjointPoints);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "includedjointpoints":
                _includedjointPoints = value.FromNonCritical();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Skalierte Ansicht";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.LupePlus, 16);

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            if (_bitmap != null) {
                _bitmap?.Dispose();
                _bitmap = null;
            }

            //IsDisposed = true;
        }
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionModified, float scale, float shiftX, float shiftY) {
        if (Parent is not { } icpi) { return; }

        var l = new List<PointM>();

        foreach (var thiss in _includedjointPoints.SplitAndCutByCr()) {
            if (icpi.GetJointPoint(thiss, this) is { } p) {
                l.Add(p);
            }
        }
        var r = RectangleF.Empty;

        if (l.Count > 2) { }

        //if (Stil == PadStyles.Undefiniert) { return new RectangleF(0, 0, 0, 0); }
        //var geszoom = Parent?.SheetStyleScale * Skalierung ?? Skalierung;
        //var f2 = Skin.GetBlueFont(Stil, Parent?.SheetStyle).Font(geszoom);

        //var maxrad = Math.Max(Math.Max(sz1.Width, sz1.Height), Math.Max(sz2.Width, sz2.Height));
        //RectangleF x = new(_point1, new SizeF(0, 0));
        //x = x.ExpandTo(_point2);
        //x = x.ExpandTo(_bezugslinie1);
        //x = x.ExpandTo(_bezugslinie2);
        //x = x.ExpandTo(_textPoint, maxrad);

        //x.Inflate(-2, -2); // die Sicherheits koordinaten damit nicht linien abgeschnitten werden
        //return x;

        //positionModified.Inflate(-Padding, -Padding);
        //RectangleF r1 = new(positionModified.Left + Padding, positionModified.Top + Padding,
        //    positionModified.Width - (Padding * 2), positionModified.Height - (Padding * 2));
        //RectangleF r2 = new();
        //RectangleF r3 = new();
        //if (Bitmap != null) {
        //    r3 = new RectangleF(0, 0, Bitmap.Width, Bitmap.Height);
        //    switch (Bild_Modus) {
        //        case SizeModes.Verzerren: {
        //                r2 = r1;
        //                break;
        //            }

        //        case SizeModes.BildAbschneiden: {
        //                var scale = Math.Max((positionModified.Width - (Padding * 2)) / Bitmap.Width, (positionModified.Height - (Padding * 2)) / Bitmap.Height);
        //                var tmpw = (positionModified.Width - (Padding * 2)) / scale;
        //                var tmph = (positionModified.Height - (Padding * 2)) / scale;
        //                r3 = new RectangleF((Bitmap.Width - tmpw) / 2, (Bitmap.Height - tmph) / 2, tmpw, tmph);
        //                r2 = r1;
        //                break;
        //            }
        //        default: // Is = enSizeModes.WeißerRand
        //        {
        //                var scale = Math.Min((positionModified.Width - (Padding * 2)) / Bitmap.Width, (positionModified.Height - (Padding * 2)) / Bitmap.Height);
        //                r2 = new RectangleF(((positionModified.Width - (Bitmap.Width * scale)) / 2) + positionModified.Left, ((positionModified.Height - (Bitmap.Height * scale)) / 2) + positionModified.Top, Bitmap.Width * scale, Bitmap.Height * scale);
        //                break;
        //            }
        //    }
        //}
        //var trp = positionModified.PointOf(Alignment.Horizontal_Vertical_Center);
        //gr.TranslateTransform(trp.X, trp.Y);
        //gr.RotateTransform(-Drehwinkel);
        //r1 = r1 with { X = r1.Left - trp.X, Y = r1.Top - trp.Y };
        //r2 = r2 with { X = r2.Left - trp.X, Y = r2.Top - trp.Y };
        //if (Hintergrund_Weiß_Füllen) {
        //    gr.FillRectangle(Brushes.White, r1);
        //}
        //try {
        //    if (Bitmap != null) {
        //        if (ForPrinting) {
        //            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
        //        } else {
        //            gr.InterpolationMode = InterpolationMode.Low;
        //            gr.PixelOffsetMode = PixelOffsetMode.HighSpeed;
        //        }
        //        gr.DrawImage(Bitmap, r2, r3, GraphicsUnit.Pixel);
        //    }
        //} catch {
        //    Generic.CollectGarbage();
        //}
        //if (Stil != PadStyles.Undefiniert) {
        //    if (Parent is { SheetStyle: not null, SheetStyleScale: > 0 }) {
        //        gr.DrawRectangle(Skin.GetBlueFont(Stil, Parent.SheetStyle).Pen(zoom * Parent.SheetStyleScale), r1);
        //    }
        //}
        //foreach (var thisQi in Overlays) {
        //    gr.DrawImage(thisQi, r2.Left + 8, r2.Top + 8);
        //}
        //gr.TranslateTransform(-trp.X, -trp.Y);
        //gr.ResetTransform();
        //if (!ForPrinting) {
        //    if (!string.IsNullOrEmpty(Platzhalter_Für_Layout)) {
        //        Font f = new("Arial", 8);
        //        BlueFont.DrawString(gr, Platzhalter_Für_Layout, f, Brushes.Black, positionModified.Left, positionModified.Top);
        //    }
        //}
    }

    #endregion
}