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
using BlueControls.Designer_Support;
using BlueControls.Enums;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Constants;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ZoomPad : GenericControl {

    #region Fields

    public static readonly Pen PenGray = new(Color.FromArgb(40, 0, 0, 0));

    /// <summary>
    /// Die Koordinaten, an der Stelle der Mausknopf gedrückt wurde. Zoom und Slider wurden eingerechnet, dass die Koordinaten Massstabsunabhängis sind.
    /// </summary>
    public Point MouseDownPos11;

    /// <summary>
    /// Die Koordinaten, an der die der Mauspfeil zuletzt war. Zoom und Slider wurden eingerechnet, dass die Koordinaten Massstabsunabhängis sind.
    /// </summary>
    public Point MousePos11;

    protected bool Fitting = true;

    private float _shiftX = 0;

    private float _shiftY = 0;
    private float _zoom = 1;
    private float _zoomFit = 1;

    #endregion

    #region Constructors

    public ZoomPad() : base(true, true) => InitializeComponent();

    #endregion

    #region Properties

    [DefaultValue(0)]
    public float ShiftX {
        get => _shiftX;
        set {
            if (Math.Abs(value - _shiftX) < DefaultTolerance) { return; }
            _shiftX = value;
            Invalidate();
        }
    }

    [DefaultValue(0)]
    public float ShiftY {
        get => _shiftY;
        set {
            if (Math.Abs(value - _shiftY) < DefaultTolerance) { return; }
            _shiftY = value;
            Invalidate();
        }
    }


    [DefaultValue(1)]
    public float Zoom {
        get => _zoom;
        set {
            value = Math.Max(_zoomFit / 10f, value);
            value = Math.Min(20, value);

            if (Math.Abs(value - _zoom) < DefaultTolerance) { return; }
            _zoom = value;
            Invalidate();
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gibt den Zeichenbereich zurück. Entspricht der Control-Größe abzüglich der Slider-Breite/Höhe
    /// </summary>
    /// <returns></returns>
    public Rectangle AvailablePaintArea() {
        var wi = Size.Width;
        if (SliderY.Visible) { wi -= SliderY.Width; }
        var he = Size.Height;
        if (SliderX.Visible) { he -= SliderX.Height; }
        return new Rectangle(0, 0, wi, he);
    }

    public void ZoomFit() {
        if (IsDisposed) { return; }
        var mb = MaxBounds();
        var x = AvailablePaintArea();
        x.Inflate(-16, -16);
        _zoomFit = ItemCollectionPad.ItemCollectionPad.ZoomFitValue(mb, x.Size);
        Zoom = _zoomFit;
    }

    public void ZoomIn(MouseEventArgs e) {
        MouseEventArgs x = new(e.Button, e.Clicks, e.X, e.Y, 1);
        OnMouseWheel(x);
    }

    public void ZoomOut(MouseEventArgs e) {
        MouseEventArgs x = new(e.Button, e.Clicks, e.X, e.Y, -1);
        OnMouseWheel(x);
    }

    ///// <summary>
    ///// Kümmert sich um Slider und Maximal-Setting.
    ///// Bei einem negativen Wert wird der neue Zoom nicht gesetzt.
    ///// </summary>
    ///// <param name="newzoom"></param>
    //protected void CalculateZoomFitAndSliders(float newzoom) {
    //    var mb = MaxBounds();
    //    _ZoomFit = ItemCollectionPad.ZoomFitValue(mb, SliderY.Width + 32, SliderX.Height + 32, Size);
    //    if (newzoom >= 0) {
    //        Zoom = newzoom;

    //        Fitting = Math.Abs(Zoom - newzoom) < 0.01;
    //    }
    //    ComputeSliders(mb);
    //    if (newzoom >= 0) {
    //        ZoomOrShiftChanged();
    //        Invalidate();
    //    }
    //}

    protected override void DrawControl(Graphics gr, States state) {
        //base.DrawControl(gr, state);

        var maxBounds = MaxBounds();

        if (maxBounds.Width == 0) { return; }
        var p = ItemCollectionPad.ItemCollectionPad.CenterPos(maxBounds, AvailablePaintArea().Size, Zoom);
        var sliderv = ItemCollectionPad.ItemCollectionPad.SliderValues(maxBounds, Zoom, p);
        if (p.X < 0) {
            SliderX.Enabled = true;
            SliderX.Minimum = (float)((maxBounds.Left * Zoom) - (Width * 0.6d));
            SliderX.Maximum = (float)((maxBounds.Right * Zoom) - Width + (Width * 0.6d));
            SliderX.Value = ShiftX;
        } else {
            SliderX.Enabled = false;
            if (MousePressing() == false) {
                SliderX.Minimum = sliderv.X;
                SliderX.Maximum = sliderv.X;
                SliderX.Value = sliderv.X;
            }
        }

        if (p.Y < 0) {
            SliderY.Enabled = true;
            SliderY.Minimum = (float)((maxBounds.Top * Zoom) - (Height * 0.6d));
            SliderY.Maximum = (float)((maxBounds.Bottom * Zoom) - Height + (Height * 0.6d));
            SliderY.Value = ShiftY;
        } else {
            SliderY.Enabled = false;
            if (MousePressing() == false) {
                SliderY.Minimum = sliderv.Y;
                SliderY.Maximum = sliderv.Y;
                SliderY.Value = sliderv.Y;
            }
        }
    }

    /// <summary>
    /// Berechnet Maus Koordinaten des Steuerelements in in Koordinaten um, als ob auf dem unscalierten Inhalt direkt gewählt werden würde.
    /// Falls die Maus-Koordinaten ausserhalb der grenzen sind, wird nichts getrimmt.
    /// </summary>
    /// <remarks>
    /// </remarks>
    protected Point KoordinatesUnscaled(MouseEventArgs e) =>
        new((int)Math.Round(((e.X + _shiftX) / Zoom) - 0.5d, 0, MidpointRounding.AwayFromZero),
            (int)Math.Round(((e.Y + _shiftY) / Zoom) - 0.5d, 0, MidpointRounding.AwayFromZero));

    protected virtual RectangleF MaxBounds() {
        Develop.DebugPrint_RoutineMussUeberschriebenWerden(false);
        return default;
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        MousePos11 = KoordinatesUnscaled(e);
        MouseDownPos11 = KoordinatesUnscaled(e);
        base.OnMouseDown(e);
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        MousePos11 = Point.Empty;
        base.OnMouseLeave(e);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        MousePos11 = KoordinatesUnscaled(e);
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        MousePos11 = KoordinatesUnscaled(e);
        base.OnMouseUp(e);
        MouseDownPos11 = Point.Empty;
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        base.OnMouseWheel(e);
        Fitting = false;
        var m = KoordinatesUnscaled(e);
        if (e.Delta > 0) {
            Zoom *= 1.5f;
        } else {
            Zoom *= 1f / 1.5f;
        }

        //var mb = MaxBounds();
        //ComputeSliders(mb);
        // M Beeinhaltet den Punkt, wo die Maus hinzeigt Maßstabunabhängig.
        // Der Slider ist abhängig vom Maßsstab - sowie die echten Mauskoordinaten ebenfalls.
        // Deswegen die M mit dem neuen Zoom-Faktor berechnen umrechen, um auch Masstababhängig zu sein
        // Die Verschiebung der echten Mauskoordinaten berechnen und den Slider auf den Wert setzen.
        ShiftX = (m.X * Zoom) - e.X;
        ShiftY = (m.Y * Zoom) - e.Y;

        // Alte Berechnung für Mittig Setzen
        //SliderX.Value = (m.X * _Zoom) - (Width / 2) - SliderY.Width
        //SliderY.Value = (m.Y * _Zoom) - (Height / 2) - SliderX.Height
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        if (Fitting) { ZoomFit(); }

        base.OnSizeChanged(e);
    }

    private void SliderX_ValueChanged(object sender, System.EventArgs e) => ShiftX = SliderX.Value;

    private void SliderY_ValueChanged(object sender, System.EventArgs e) => ShiftY = SliderY.Value;

    #endregion
}