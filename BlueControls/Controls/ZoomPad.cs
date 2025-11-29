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
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Constants;
using static BlueBasics.Converter;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public abstract partial class ZoomPad : GenericControl, IBackgroundNone {

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

    private Rectangle _lastPaintArea;
    private Size _lastSize;
    private bool _lastSliderXVisible;
    private bool _lastSliderYVisible;
    private float _shiftX;
    private float _shiftY;
    private float _zoom = 1;
    private float _zoomFit = 1;

    #endregion

    #region Constructors

    protected ZoomPad() : base(true, true, false) => InitializeComponent();

    #endregion

    #region Properties

    public abstract bool ControlMustPressed { get; }

    public bool Fitting { get; private set; } = true;

    public new bool Focused => base.Focused || SliderX.Focused || SliderY.Focused;

    [DefaultValue(false)]
    public bool ScreenshotMode {
        get;
        set {
            if (field == value) { return; }
            field = value;

            // Slider verstecken/anzeigen
            SliderX.Visible = !value;
            SliderY.Visible = !value;

            if (value) {
                UpdateScreenshotLayout();
            }

            Invalidate();
        }
    }

    [DefaultValue(0f)]
    public float ShiftX {
        get => _shiftX;
        set {
            if (ScreenshotMode) { return; }
            if (Math.Abs(value - _shiftX) < DefaultTolerance) { return; }
            _shiftX = value;
            OnShiftChanged();
        }
    }

    [DefaultValue(0f)]
    public float ShiftY {
        get => _shiftY;
        set {
            if (ScreenshotMode) { return; }
            if (Math.Abs(value - _shiftY) < DefaultTolerance) { return; }
            _shiftY = value;
            OnShiftChanged();
        }
    }

    [DefaultValue(1f)]
    public float Zoom {
        get => _zoom;
        set {
            if (ScreenshotMode) { return; }
            value = Math.Max(_zoomFit / 10f, value);
            value = Math.Min(20, value);

            if (Math.Abs(value - _zoom) < DefaultTolerance) { return; }
            _zoom = value;
            OnZoomChanged();
        }
    }

    protected abstract bool AutoCenter { get; }

    /// <summary>
    /// So viel darf zusätzlich hinausgezoomt werden.
    /// </summary>
    protected abstract float SliderZoomOutAddition { get; }

    private bool ControlPressing { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Berechnet Maus Koordinaten des Steuerelements in in Koordinaten um, als ob auf dem unscalierten Inhalt direkt gewählt werden würde.
    /// Falls die Maus-Koordinaten ausserhalb der grenzen sind, wird nichts getrimmt.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public static Point CoordinatesUnscaled(MouseEventArgs e, float scale, float shiftX, float shiftY) => CoordinatesUnscaled(e.Location, scale, shiftX, shiftY);

    public static Point CoordinatesUnscaled(Point e, float scale, float shiftX, float shiftY) =>
    new((int)Math.Round(((e.X + shiftX) / scale) - 0.5d, 0, MidpointRounding.AwayFromZero),
        (int)Math.Round(((e.Y + shiftY) / scale) - 0.5d, 0, MidpointRounding.AwayFromZero));

    public static int GetPix(int pix, float scale) => (int)((pix * scale) + 0.5);

    public static bool ScaleWarnung() {
        if (Skin.Scale is > 0.98f and < 1.02f) { return false; }
        Forms.MessageBox.Show("Diese Funktion kann mit ihrer aktuellen Schriftgrößeneinstellung<br>leider nicht möglich.", ImageCode.Warnung, "OK");
        return true;
    }

    /// <summary>
    /// Gibt den Zeichenbereich zurück. Entspricht der Control-Größe abzüglich der Slider-Breite/Höhe
    /// </summary>
    /// <returns></returns>
    public Rectangle AvailablePaintArea() {
        // Schneller Check ob sich etwas geändert hat
        if (_lastSize == Size &&
            _lastSliderXVisible == SliderX.Visible &&
            _lastSliderYVisible == SliderY.Visible) {
            return _lastPaintArea;
        }

        // Nur bei Änderungen neu berechnen

        // Cache aktualisieren
        _lastPaintArea = new(
            0,
            0,
            Size.Width - (SliderY.Visible ? SliderY.Width : 0),
            Size.Height - (SliderX.Visible ? SliderX.Height : 0)
        );
        _lastSize = Size;
        _lastSliderXVisible = SliderX.Visible;
        _lastSliderYVisible = SliderY.Visible;

        return _lastPaintArea;
    }

    public RectangleF AvailablePaintAreaScaled() => AvailablePaintArea().ZoomAndMoveRect(Zoom, 0, 0, true);

    public void DoZoom(bool zoomIn) {
        var nz = _zoom;

        if (zoomIn) {
            nz *= 1.05f;
        } else {
            nz *= 1f / 1.05f;
        }

        nz = Math.Max(nz, 0.5f);
        nz = Math.Min(nz, 4);
        Zoom = nz;
    }

    public virtual void ParseView(string toParse) {
        if (IsDisposed) { return; }

        if (!string.IsNullOrEmpty(toParse) && toParse.GetAllTags() is { } x) {
            foreach (var pair in x) {
                switch (pair.Key) {
                    case "sliderx":
                        SliderX.Maximum = Math.Max(SliderX.Maximum, IntParse(pair.Value));
                        SliderX.Value = IntParse(pair.Value);
                        break;

                    case "slidery":
                        SliderY.Maximum = Math.Max(SliderY.Maximum, IntParse(pair.Value));
                        SliderY.Value = IntParse(pair.Value);
                        break;

                    case "zoom":
                        Zoom = FloatParse(pair.Value.FromNonCritical());
                        break;
                }
            }
        }

        Fitting = false;
    }

    public virtual List<string> ViewToString() {
        List<string> result = [];
        result.ParseableAdd("Zoom", _zoom);
        result.ParseableAdd("SliderX", SliderX.Value);
        result.ParseableAdd("SliderY", SliderY.Value);
        return result;
    }

    public void ZoomFit() {
        if (IsDisposed) { return; }
        var mb = MaxBounds();
        var x = AvailablePaintArea();
        x.Inflate(-16, -16);
        _zoomFit = ItemCollectionPadItem.ZoomFitValue(mb, x.Size);
        Zoom = _zoomFit;
        Fitting = true;
    }

    public void ZoomIn(MouseEventArgs e) {
        MouseEventArgs x = new(e.Button, e.Clicks, e.X, e.Y, 1);
        OnMouseWheel(x);
    }

    public void ZoomOut(MouseEventArgs e) {
        MouseEventArgs x = new(e.Button, e.Clicks, e.X, e.Y, -1);
        OnMouseWheel(x);
    }

    internal void EnsureVisibleX(int x) {
        var pa = AvailablePaintArea();

        if (x < pa.Left) {
            ShiftX = ShiftX + x - pa.Width;
        } else if (x > pa.Width) {
            ShiftX = ShiftX + x - pa.Width;
        }
    }

    internal void EnsureVisibleY(int y) {
        var pa = AvailablePaintArea();

        if (y < pa.Top) {
            ShiftY = ShiftY + y - pa.Height;
        } else if (y > pa.Width) {
            ShiftY = ShiftY + y - pa.Height;
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        //base.DrawControl(gr, state);

        var maxBounds = MaxBounds();

        if (maxBounds.Width == 0) { return; }
        var p = ItemCollectionPadItem.CenterPos(maxBounds, AvailablePaintArea().Size, _zoom);
        PointF sliderv;
        if (AutoCenter) {
            sliderv = ItemCollectionPadItem.SliderValues(maxBounds, _zoom, p);
        } else {
            sliderv = new PointF(0, 0);
        }

        if (p.X < 0) {
            SliderX.Enabled = true;
            SliderX.Minimum = (float)((maxBounds.Left * _zoom) - (Width * SliderZoomOutAddition));
            SliderX.Maximum = (float)((maxBounds.Right * _zoom) - Width + (Width * SliderZoomOutAddition));
            SliderX.Value = ShiftX;
        } else {
            SliderX.Enabled = false;
            if (!MousePressing()) {
                SliderX.Minimum = sliderv.X;
                SliderX.Maximum = sliderv.X;
                SliderX.Value = sliderv.X;
            }
        }

        if (p.Y < 0) {
            SliderY.Enabled = true;
            SliderY.Minimum = (float)((maxBounds.Top * _zoom) - (Height * SliderZoomOutAddition));
            SliderY.Maximum = (float)((maxBounds.Bottom * _zoom) - Height + (Height * SliderZoomOutAddition));
            SliderY.Value = ShiftY;
        } else {
            SliderY.Enabled = false;
            if (!MousePressing()) {
                SliderY.Minimum = sliderv.Y;
                SliderY.Maximum = sliderv.Y;
                SliderY.Value = sliderv.Y;
            }
        }
    }

    protected void DrawWaitScreen(Graphics gr, string info) {
        Skin.Draw_Back(gr, Design.Table_And_Pad, States.Standard_Disabled, base.DisplayRectangle, this, true);

        var i = QuickImage.Get(ImageCode.Uhr, 64);
        gr.DrawImage(i, (Width - 64) / 2, (Height - 64) / 2);

        var fa = BlueFont.DefaultFont;

        fa.DrawString(gr, info, 12, 50);

        Skin.Draw_Border(gr, Design.Table_And_Pad, States.Standard_Disabled, base.DisplayRectangle);
    }

    protected abstract RectangleF MaxBounds();

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);

        if (IsDisposed) { return; }

        ControlPressing = e.Modifiers == Keys.Control;

        switch (e.KeyCode) {
            case Keys.PageDown:
                if (SliderY.Enabled) {
                    ShiftY += SliderY.LargeChange;
                }
                break;

            case Keys.PageUp: //Bildab
                if (SliderY.Enabled) {
                    ShiftY -= SliderY.LargeChange;
                }
                break;

            case Keys.Home:
                if (SliderY.Enabled) {
                    ShiftY = SliderY.Minimum;
                }
                break;

            case Keys.End:
                if (SliderY.Enabled) {
                    ShiftY = SliderY.Maximum;
                }
                break;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e) {
        base.OnKeyUp(e);
        ControlPressing = false;
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        MousePos11 = CoordinatesUnscaled(e, _zoom, _shiftX, _shiftY);
        MouseDownPos11 = CoordinatesUnscaled(e, _zoom, _shiftX, _shiftY);
        base.OnMouseDown(e);
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        ControlPressing = false;
        MousePos11 = Point.Empty;
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        MousePos11 = CoordinatesUnscaled(e, _zoom, _shiftX, _shiftY);
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        MousePos11 = CoordinatesUnscaled(e, _zoom, _shiftX, _shiftY);
        base.OnMouseUp(e);
        MouseDownPos11 = Point.Empty;
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        base.OnMouseWheel(e);

        if (ControlMustPressed && !ControlPressing) { return; }

        Fitting = false;
        var m = CoordinatesUnscaled(e, _zoom, _shiftX, _shiftY);
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
        ShiftX = (m.X * _zoom) - e.X;
        ShiftY = (m.Y * _zoom) - e.Y;

        // Alte Berechnung für Mittig Setzen
        //SliderX.Value = (m.X * _zoom) - (Width / 2) - SliderY.Width
        //SliderY.Value = (m.Y * _zoom) - (Height / 2) - SliderX.Height
    }

    protected virtual void OnShiftChanged() {
        Invalidate();
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        if (ScreenshotMode) {
            UpdateScreenshotLayout();
        } else {
            base.OnSizeChanged(e);
        }
    }

    protected virtual void OnZoomChanged() {
        Invalidate();
    }

    private void SliderX_ValueChanged(object sender, System.EventArgs e) => ShiftX = SliderX.Value;

    private void SliderY_ValueChanged(object sender, System.EventArgs e) => ShiftY = SliderY.Value;

    private void UpdateScreenshotLayout() {
        // Im Screenshot-Modus immer Zoom-Fit aktivieren
        _zoomFit = ItemCollectionPadItem.ZoomFitValue(MaxBounds(), AvailablePaintArea().Size);
        _zoom = _zoomFit;

        // Bild zentrieren
        var centerPos = ItemCollectionPadItem.CenterPos(MaxBounds(), AvailablePaintArea().Size, _zoom);
        _shiftX = centerPos.X;
        _shiftY = centerPos.Y;

        // Fitting aktivieren damit bei Größenänderung automatisch angepasst wird
        Fitting = true;
    }

    #endregion
}