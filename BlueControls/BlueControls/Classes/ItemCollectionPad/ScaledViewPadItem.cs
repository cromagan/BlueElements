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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.Abstract;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollectionPad;

public sealed class ScaledViewPadItem : FixedRectanglePadItem, IStyleableOne, ISupportsTextScale {

    #region Fields

    private Alignment _ausrichtung = Alignment.Top_Left;

    private string _caption = string.Empty;

    private ReadOnlyCollection<string> _includedjointPoints = new([]);

    private float _scale = 1f;

    private PadStyles _style = PadStyles.Standard;

    private float _textScale = 3.07f;

    #endregion

    #region Constructors

    public ScaledViewPadItem() : base(string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "SCALEDVIEW";

    public Alignment Ausrichtung {
        get => _ausrichtung;
        set {
            if (IsDisposed) { return; }
            if (value == _ausrichtung) { return; }
            _ausrichtung = value;
            OnPropertyChanged();
        }
    }

    public string Caption {
        get => _caption;
        internal set {
            if (value == _caption) { return; }
            _caption = value;
            OnPropertyChanged();
        }
    }

    public override string Description => string.Empty;

    public BlueFont? Font { get; set; }

    public ReadOnlyCollection<string> IncludedJointPoints {
        get => _includedjointPoints;
        set {
            if (!_includedjointPoints.IsDifferentTo(value)) { return; }
            _includedjointPoints = value;
            CalculateSize();
            OnPropertyChanged();
        }
    }

    public float Scale {
        get => _scale;
        internal set {
            value = Math.Max(value, 0.01f);
            value = Math.Min(value, 100f);
            if (Math.Abs(value - _scale) < Constants.DefaultTolerance) { return; }
            _scale = value;
            CalculateSize();
            OnPropertyChanged();
        }
    }

    public string SheetStyle {
        get {
            if (_parent is IStyleable ist) { return ist.SheetStyle; }
            return string.Empty;
        }
    }


    public PadStyles Stil {
        get => _style;
        set {
            if (_style == value) { return; }
            _style = value;
            this.InvalidateFont();
            OnPropertyChanged();
        }
    }

    public float TextScale {
        get => _textScale;
        set {
            value = Math.Max(value, 0.01f);
            value = Math.Min(value, 20);
            if (Math.Abs(value - _textScale) < Constants.DefaultTolerance) { return; }
            _textScale = value;
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public override void AddedToCollection(ItemCollectionPadItem parent) {
        base.AddedToCollection(parent);
        CalculateSize();
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
          new FlexiControlForProperty<string>(() => Caption),
             new FlexiControlForProperty<float>(() => TextScale),
                      new FlexiControlForProperty<float>(() => Scale),
          //new FlexiControlForProperty<PadStyles>(() => Stil, Skin.GetFonts(SheetStyle)),
          new FlexiControlForProperty<ReadOnlyCollection<string>>(() => IncludedJointPoints, 5),

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
        result.ParseableAdd("Caption", _caption);
        result.ParseableAdd("Style", _style);
        result.ParseableAdd("Scale", _scale);
        result.ParseableAdd("IncludedJointPoints", _includedjointPoints.JoinWithCr());
        result.ParseableAdd("AdditionalScale", _textScale);
        result.ParseableAdd("Alignment", _ausrichtung);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "includedjointpoints":
                //_includedjointPoints = value.FromNonCritical().SplitAndCutByCrToList();
                return true;

            case "additionalscale":
                _textScale = FloatParse(value.FromNonCritical());
                return true;

            case "alignment":
                _ausrichtung = (Alignment)byte.Parse(value);
                return true;

            case "style":
                _style = (PadStyles)IntParse(value);
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
            //if (_bitmap != null) {
            //    _bitmap?.Dispose();
            //    _bitmap = null;
            //}

            //IsDisposed = true;
        }
    }

    //}
    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionModified, float scale, float shiftX, float shiftY) {
        if (Parent is not ItemCollectionPadItem {IsDisposed: false} icpi) { return; }

    
            var newarea = positionModified.ToRect();
            var (childScale, childShiftX, childShiftY) = ItemCollectionPadItem.AlterView(positionModified, scale, shiftX, shiftY, true, CalculateShowingArea());

            foreach (var thisItem in icpi) {
                if (thisItem is not ScaledViewPadItem) {
                    gr.PixelOffsetMode = PixelOffsetMode.None;
                    var childpos = thisItem.UsedArea.ZoomAndMoveRect(childScale, childShiftX, childShiftY, false);
                    if (IsInDrawingArea(childpos, newarea)) {
                        // IsInDrawingArea extra abfragen, weil ShowAlways dazwischenfunkt
                        gr.SetClip(newarea);
                        thisItem.Draw(gr, newarea, childScale, childShiftX, childShiftY);
                        gr.ResetClip();
                    }
                }
            }
        


        var bFont = this.GetFont(_textScale * scale);
        //var font = bFont.Font(allScale);

        Pen colorPen = new(bFont.ColorMain, (float)(8.7d * scale)) {
            DashPattern = [5, 1, 1, 1]
        };
        Pen whitePen = new(Color.White, (float)(8.7d * scale) + 2f);

        var textSize = bFont.MeasureString(_caption);

        // Umrandung der Detailansicht
        gr.DrawRectangle(whitePen, positionModified);
        gr.DrawRectangle(colorPen, positionModified);
        if (_ausrichtung != (Alignment)(-1)) {
            gr.FillRectangle(Brushes.White, new RectangleF(positionModified.Left, positionModified.Top - textSize.Height - (9f * scale), textSize.Width, textSize.Height));
            bFont.DrawString(gr, _caption, positionModified.Left, positionModified.Top - textSize.Height - (9f * scale));
        }

        //Markierung in der Zeichnung
        var f = CalculateShowingArea().ZoomAndMoveRect(scale, shiftX, shiftY, false);
        gr.DrawRectangle(whitePen, f);
        gr.DrawRectangle(colorPen, f);
        if (_ausrichtung != (Alignment)(-1)) {
            gr.FillRectangle(Brushes.White, new RectangleF(f.Left, f.Top - textSize.Height - (9f * scale), textSize.Width, textSize.Height));
            bFont.DrawString(gr, _caption, f.Left, f.Top - textSize.Height - (9f * scale));
        }

        //base.DrawExplicit(gr,visibleArea,positionModified,scale, shiftX, shiftY);
    }

    private RectangleF CalculateShowingArea() {
        var points = new List<PointM>();

        if (Parent is ItemCollectionPadItem {IsDisposed: false} icpi) {
            foreach (var thiss in _includedjointPoints) {
                if (icpi.GetJointPoints(thiss, this) is { } p) {
                    points.AddRange(p);
                }
            }
        }

        points = points.Distinct().ToList();

        if (points.Count < 1) { return new RectangleF(0, 0, 10, 10); }

        if (points.Count == 1) { return new RectangleF(points[0].X - 5, points[0].Y - 5, 10, 10); }

        RectangleF area = new(points[0].X, points[0].Y, 0, 0);

        foreach (var thisPoint in points) {
            area = area.ExpandTo(thisPoint);
        }

        area.Inflate(-2, -2); // die Sicherheits koordinaten damit nicht linien abgeschnitten werden

        return area;
    }

    private void CalculateSize() {
        var r = CalculateShowingArea();
        Size = new SizeF(r.Width * _scale, r.Height * _scale);
    }

    #endregion
}