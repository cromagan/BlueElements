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
using MessageBox = BlueControls.Forms.MessageBox;

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

    // ReSharper disable once UnusedMember.Global
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
            if (MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        }
        Bitmap = ScreenShot.GrabArea(null).Area;
    }

    public void Datei_laden() {
        if (Bitmap != null) {
            if (MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
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
            ItemOf("Abschneiden", ((int)SizeModes.BildAbschneiden).ToString(),
                QuickImage.Get("BildmodusAbschneiden|32")),
            ItemOf("Verzerren", ((int)SizeModes.Verzerren).ToString(), QuickImage.Get("BildmodusVerzerren|32")),
            ItemOf("Einpassen", ((int)SizeModes.EmptySpace).ToString(), QuickImage.Get("BildmodusEinpassen|32"))
        ];

        List<GenericControl> result =
        [
            new FlexiDelegateControl(Bildschirmbereich_wählen, "Bildschirmbereich wählen", ImageCode.Bild),

            new FlexiDelegateControl(Datei_laden, "Bild laden", ImageCode.Ordner),

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

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionModified, float scale, float shiftX, float shiftY) {
        //positionModified.Inflate(-_padding, -_padding);
        //RectangleF r1 = new(positionModified.Left , positionModified.Top , positionModified.Width , positionModified.Height );
        RectangleF r2 = new();
        RectangleF r3 = new();
        if (Bitmap != null) {
            r3 = new RectangleF(0, 0, Bitmap.Width, Bitmap.Height);
            switch (Bild_Modus) {
                case SizeModes.Verzerren: {
                        r2 = positionModified;
                        break;
                    }

                case SizeModes.BildAbschneiden: {
                        var scale2 = Math.Max(positionModified.Width / Bitmap.Width, positionModified.Height / Bitmap.Height);
                        var tmpw = positionModified.Width / scale2;
                        var tmph = positionModified.Height / scale2;
                        r3 = new RectangleF((Bitmap.Width - tmpw) / 2, (Bitmap.Height - tmph) / 2, tmpw, tmph);
                        r2 = positionModified;
                        break;
                    }
                default: // Is = enSizeModes.WeißerRand
                {
                        var scale2 = Math.Min(positionModified.Width / Bitmap.Width, positionModified.Height / Bitmap.Height);
                        r2 = new RectangleF(((positionModified.Width - (Bitmap.Width * scale2)) / 2) + positionModified.Left, ((positionModified.Height - (Bitmap.Height * scale2)) / 2) + positionModified.Top, Bitmap.Width * scale2, Bitmap.Height * scale2);
                        break;
                    }
            }
        }
        var trp = positionModified.PointOf(Alignment.Horizontal_Vertical_Center);
        gr.TranslateTransform(trp.X, trp.Y);
        gr.RotateTransform(-Drehwinkel);
        var r1 = positionModified with { X = positionModified.Left - trp.X, Y = positionModified.Top - trp.Y };
        r2 = r2 with { X = r2.Left - trp.X, Y = r2.Top - trp.Y };
        if (Hintergrund_Weiß_Füllen) {
            gr.FillRectangle(Brushes.White, r1);
        }
        try {
            if (Bitmap != null) {
                if (ForPrinting) {
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
            gr.DrawRectangle(this.GetFont().Pen(scale), r1);
        }

        gr.TranslateTransform(-trp.X, -trp.Y);
        gr.ResetTransform();
        if (!ForPrinting) {
            if (!string.IsNullOrEmpty(Platzhalter_Für_Layout)) {
                Font f = new("Arial", 8);
                BlueFont.DrawString(gr, Platzhalter_Für_Layout, f, Brushes.Black, positionModified.Left, positionModified.Top);
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