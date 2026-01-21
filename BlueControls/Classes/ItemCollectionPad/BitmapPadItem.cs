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

public sealed class BitmapPadItem : RectanglePadItem, ICanHaveVariables, IStyleableOne {

    #region Fields

    private SizeModes _bild_modus;
    private Bitmap? _bitmap;

    [Description("Hier kann ein Variablenname als Platzhalter eingegeben werden. Beispiel: ~Bild~")]
    private string _platzhalter_für_layout = string.Empty;

    private PadStyles _style;

    #endregion

    #region Constructors

    public BitmapPadItem() : this(string.Empty, null, Size.Empty) { }

    public BitmapPadItem(string keyName, Bitmap? bmp, Size size) : base(keyName) {
        _bitmap = bmp;
        SetCoordinates(new RectangleF(0, 0, size.Width, size.Height));
        Hintergrund_Weiß_Füllen = true;
        _bild_modus = SizeModes.EmptySpace;
        _style = PadStyles.Undefiniert; // Kein Rahmen
    }

    #endregion

    #region Properties

    public static string ClassId => "IMAGE";

    public SizeModes Bild_Modus {
        get => _bild_modus; set {
            if (_bild_modus == value) { return; }
            _bild_modus = value;
            OnPropertyChanged();
        }
    }

    public Bitmap? Bitmap {
        get => _bitmap; set {
            if (_bitmap == value) { return; }
            _bitmap = value;
            OnPropertyChanged();
        }
    }

    public override string Description => string.Empty;

    public BlueFont? Font { get; set; }

    public bool Hintergrund_Weiß_Füllen { get; set; }

    public string Platzhalter_Für_Layout {
        get => _platzhalter_für_layout; set {
            if (_platzhalter_für_layout == value) { return; }
            _platzhalter_für_layout = value;
            OnPropertyChanged();
        }
    }

    public string SheetStyle => Parent is IStyleable ist ? ist.SheetStyle : string.Empty;

    public PadStyles Style {
        get => _style;
        set {
            if (_style == value) { return; }
            _style = value;
            this.InvalidateFont();
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public void Bildschirmbereich_wählen() {
        if (Bitmap != null) {
            if (Forms.MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        }
        Bitmap = ScreenShot.GrabArea(null).Area;
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
        e.ShowDialog();

        if (!FileExists(e.FileName)) { return; }
        Bitmap = (Bitmap?)Image_FromFile(e.FileName);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<AbstractListItem> comms =
        [
            ItemOf("Abschneiden", ((int)SizeModes.BildAbschneiden).ToString1(),
                QuickImage.Get("BildmodusAbschneiden|32")),
            ItemOf("Verzerren", ((int)SizeModes.Verzerren).ToString1(), QuickImage.Get("BildmodusVerzerren|32")),
            ItemOf("Einpassen", ((int)SizeModes.EmptySpace).ToString1(), QuickImage.Get("BildmodusEinpassen|32"))
        ];

        List<GenericControl> result =
        [
            new FlexiControlForDelegate(Bildschirmbereich_wählen, "Bildschirmbereich wählen", ImageCode.Bild),

            new FlexiControlForDelegate(Datei_laden, "Bild laden", ImageCode.Ordner),

            new FlexiControl(),

            new FlexiControlForProperty<string>(() => Platzhalter_Für_Layout, 2),

            new FlexiControl(),
            new FlexiControlForProperty<SizeModes>(() => Bild_Modus, comms),
            new FlexiControl(),
            new FlexiControlForProperty<PadStyles>(() => Style, Skin.GetRahmenArt(SheetStyle, true)),
            new FlexiControlForProperty<bool>(() => Hintergrund_Weiß_Füllen),
            new FlexiControl(),
            ..base.GetProperties(widthOfControl)
        ];

        return result;
    }

    public override void Mirror(PointM? p, bool vertical, bool horizontal) {
        p ??= new PointM(JointMiddle);

        base.Mirror(p, vertical, horizontal);

        if (horizontal == vertical || _bitmap == null) { return; }

        if (vertical) {
            _bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
        }

        if (horizontal) {
            _bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }

        OnPropertyChanged();
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Modus", Bild_Modus);
        result.ParseableAdd("Placeholder", Platzhalter_Für_Layout);
        result.ParseableAdd("WhiteBack", Hintergrund_Weiß_Füllen);
        result.ParseableAdd("Image", Bitmap);
        result.ParseableAdd("Style", _style);

        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "modus":
                _bild_modus = (SizeModes)IntParse(value);
                return true;

            case "whiteback":
                Hintergrund_Weiß_Füllen = value.FromPlusMinus();
                return true;

            case "padding":
                //_padding = IntParse(value);
                return true;

            case "image":
                _bitmap = Base64ToBitmap(value);
                return true;

            case "placeholder":
                _platzhalter_für_layout = value.FromNonCritical();
                return true;

            case "style":
                _style = (PadStyles)IntParse(value);
                _style = Skin.RepairStyle(_style);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Bild";

    public bool ReplaceVariable(Variable variable) {
        if (IsDisposed) { return false; }
        if (string.IsNullOrEmpty(Platzhalter_Für_Layout)) { return false; }
        if (!("~" + variable.KeyName + "~").Equals(Platzhalter_Für_Layout, StringComparison.OrdinalIgnoreCase)) { return false; }

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
            return true;
        }

        return false;
    }

    public bool ResetVariables() {
        if (IsDisposed) { return false; }
        if (!string.IsNullOrEmpty(Platzhalter_Für_Layout) && Bitmap != null) {
            Bitmap = null;
            return true;
        }
        return false;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Bild, 16);

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        UnRegisterEvents();
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

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        //positionControl.Inflate(-_padding, -_padding);
        //RectangleF r1 = new(positionControl.Left , positionControl.Top , positionControl.Width , positionControl.Height );
        RectangleF r2 = new();
        RectangleF r3 = new();
        if (Bitmap != null) {
            r3 = new RectangleF(0, 0, Bitmap.Width, Bitmap.Height);
            switch (Bild_Modus) {
                case SizeModes.Verzerren: {
                        r2 = positionControl;
                        break;
                    }

                case SizeModes.BildAbschneiden: {
                        var scale2 = Math.Max(positionControl.Width / Bitmap.Width, positionControl.Height / Bitmap.Height);
                        var tmpw = positionControl.Width / scale2;
                        var tmph = positionControl.Height / scale2;
                        r3 = new RectangleF((Bitmap.Width - tmpw) / 2, (Bitmap.Height - tmph) / 2, tmpw, tmph);
                        r2 = positionControl;
                        break;
                    }
                default: // Is = enSizeModes.WeißerRand
                {
                        var scale2 = Math.Min(positionControl.Width / Bitmap.Width, positionControl.Height / Bitmap.Height);
                        r2 = new RectangleF(((positionControl.Width - Bitmap.Width.CanvasToControl(scale2)) / 2) + positionControl.Left, ((positionControl.Height - Bitmap.Height.CanvasToControl(scale2)) / 2) + positionControl.Top, Bitmap.Width.CanvasToControl(scale2), Bitmap.Height.CanvasToControl(scale2));
                        break;
                    }
            }
        }
        var trp = positionControl.PointOf(Alignment.Horizontal_Vertical_Center);
        gr.TranslateTransform(trp.X, trp.Y);
        gr.RotateTransform(-Drehwinkel);
        var r1 = positionControl with { X = positionControl.Left - trp.X, Y = positionControl.Top - trp.Y };
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
        if (_style != PadStyles.Undefiniert) {
            gr.DrawRectangle(this.GetFont().Pen(zoom), r1);
        }

        gr.TranslateTransform(-trp.X, -trp.Y);
        gr.ResetTransform();
        if (!forPrinting) {
            if (!string.IsNullOrEmpty(Platzhalter_Für_Layout)) {
                var f = new Font("Arial", 8);
                BlueFont.DrawString(gr, Platzhalter_Für_Layout, f, Brushes.Black, positionControl.Left, positionControl.Top);
            }
        }
    }

    protected override void OnParentChanged() {
        base.OnParentChanged();
        this.InvalidateFont();
        if (Parent is ItemCollectionPadItem icpi) {
            icpi.StyleChanged += Icpi_StyleChanged;
        }
    }

    protected override void OnParentChanging() {
        base.OnParentChanging();
        UnRegisterEvents();
    }

    private void Icpi_StyleChanged(object sender, System.EventArgs e) => this.InvalidateFont();

    private void UnRegisterEvents() {
        if (Parent is ItemCollectionPadItem icpi) {
            icpi.StyleChanged -= Icpi_StyleChanged;
        }
    }

    #endregion
}