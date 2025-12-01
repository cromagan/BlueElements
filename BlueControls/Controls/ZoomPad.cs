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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
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

    private Rectangle _availableControlPaintArea;
    private RectangleF? _canvasMaxbounds;
    private Size _lastSize;

    private bool _lastSliderXVisible;

    private bool _lastSliderYVisible;
    private float _offsetX;
    private float _offsetY;
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

    /// <summary>
    /// Die Koordinaten, an der Stelle der Mausknopf gedrückt wurde. Zoom und Slider wurden eingerechnet, dass die Koordinaten Massstabsunabhängis sind.
    /// </summary>
    public PointF MouseDownCanvas { get; protected set; }

    [DefaultValue(0f)]
    public float OffsetX {
        get => _offsetX;
        set {
            if (ScreenshotMode) { return; }
            if (Math.Abs(value - _offsetX) < DefaultTolerance) { return; }
            _offsetX = value;
            OnOffsetXChanged();
        }
    }

    [DefaultValue(0f)]
    public float OffsetY {
        get => _offsetY;
        set {
            if (ScreenshotMode) { return; }
            if (Math.Abs(value - _offsetY) < DefaultTolerance) { return; }
            _offsetY = value;
            OnOffsetYChanged();
        }
    }

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

    public static bool ScaleWarnung() {
        if (Skin.Scale is > 0.98f and < 1.02f) { return false; }
        Forms.MessageBox.Show("Diese Funktion kann mit ihrer aktuellen Schriftgrößeneinstellung<br>leider nicht möglich.", ImageCode.Warnung, "OK");
        return true;
    }

    /// <summary>
    /// Gibt den Zeichenbereich zurück. Entspricht der Control-Größe abzüglich der Slider-Breite/Höhe
    /// </summary>
    /// <returns></returns>
    public Rectangle AvailableControlPaintArea() {
        // Schneller Check ob sich etwas geändert hat
        if (_lastSize == Size &&
            _lastSliderXVisible == SliderX.Visible &&
            _lastSliderYVisible == SliderY.Visible) {
            return _availableControlPaintArea;
        }

        // Nur bei Änderungen neu berechnen

        // Cache aktualisieren
        _availableControlPaintArea = new(
            0,
            0,
            Size.Width - (SliderY.Visible ? SliderY.Width : 0),
            Size.Height - (SliderX.Visible ? SliderX.Height : 0)
        );
        _lastSize = Size;
        _lastSliderXVisible = SliderX.Visible;
        _lastSliderYVisible = SliderY.Visible;

        return _availableControlPaintArea;
    }

    //public RectangleF AvailablePaintAreaScaled() => AvailableControlPaintArea().CanvasToControlX(Zoom, 0, 0, true);

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
        var canvasBounds = CanvasMaxBounds();
        var controlBounds = AvailableControlPaintArea();
        controlBounds.Inflate(-16, -16);
        _zoomFit = ItemCollectionPadItem.ZoomFitValue(canvasBounds, controlBounds.Size);
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

    internal void EnsureVisibleX(float controlX) {
        var pa = AvailableControlPaintArea();

        if (controlX < pa.Left) {
            OffsetX = OffsetX + controlX - pa.Width;
        } else if (controlX > pa.Width) {
            OffsetX = OffsetX + controlX - pa.Width;
        }
    }

    internal void EnsureVisibleY(float controlY) {
        var pa = AvailableControlPaintArea();

        if (controlY < pa.Top) {
            OffsetY = OffsetY + controlY - pa.Height;
        } else if (controlY > pa.Width) {
            OffsetY = OffsetY + controlY - pa.Height;
        }
    }

    protected abstract RectangleF CalculateCanvasMaxBounds();

    protected RectangleF CanvasMaxBounds() {
        if (_canvasMaxbounds is RectangleF r) { return r; }
        var r2 = CalculateCanvasMaxBounds();
        _canvasMaxbounds = r2;
        return r2;
    }

    protected override void DrawControl(Graphics gr, States state) {
        //base.DrawControl(gr, state);

        var maxCanvasBounds = CanvasMaxBounds();

        if (maxCanvasBounds.Width == 0) { return; }

        var a = AvailableControlPaintArea();

        var freiraumControl = ItemCollectionPadItem.FreiraumControl(maxCanvasBounds, a.Size, _zoom);
        PointF sliderv;
        if (AutoCenter) {
            sliderv = ItemCollectionPadItem.SliderValues(maxCanvasBounds, _zoom, freiraumControl);
        } else {
            sliderv = new PointF(0, 0);
        }

        if (freiraumControl.X < 0) {
            SliderX.Enabled = true;
            SliderX.Minimum = (float)(maxCanvasBounds.Left.CanvasToControl(_zoom) - (a.Width * SliderZoomOutAddition));
            SliderX.Maximum = (float)(maxCanvasBounds.Right.CanvasToControl(_zoom) - a.Width + (a.Width * SliderZoomOutAddition)) + 1;
            SliderX.Value = -OffsetX;
        } else {
            SliderX.Enabled = false;
            if (!MousePressing()) {
                SliderX.Minimum = -sliderv.X;
                SliderX.Maximum = -sliderv.X;
                SliderX.Value = -sliderv.X;
            }
        }

        if (freiraumControl.Y < 0) {
            SliderY.Enabled = true;
            SliderY.Minimum = (float)(maxCanvasBounds.Top.CanvasToControl(_zoom) - (a.Height * SliderZoomOutAddition));
            SliderY.Maximum = (float)(maxCanvasBounds.Bottom.CanvasToControl(_zoom) - a.Height + (a.Height * SliderZoomOutAddition)) + 1;
            SliderY.Value = -OffsetY;
        } else {
            SliderY.Enabled = false;
            if (!MousePressing()) {
                SliderY.Minimum = -sliderv.Y;
                SliderY.Maximum = -sliderv.Y;
                SliderY.Value = -sliderv.Y;
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

    protected void Invalidate_MaxBounds() {
        _canvasMaxbounds = null;
        Invalidate();
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);

        if (IsDisposed) { return; }

        ControlPressing = e.Modifiers == Keys.Control;

        switch (e.KeyCode) {
            case Keys.PageDown:
                if (SliderY.Enabled) {
                    OffsetY += SliderY.LargeChange;
                }
                break;

            case Keys.PageUp: //Bildab
                if (SliderY.Enabled) {
                    OffsetY -= SliderY.LargeChange;
                }
                break;

            case Keys.Home:
                if (SliderY.Enabled) {
                    OffsetY = SliderY.Minimum;
                }
                break;

            case Keys.End:
                if (SliderY.Enabled) {
                    OffsetY = SliderY.Maximum;
                }
                break;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e) {
        base.OnKeyUp(e);
        ControlPressing = false;
    }

    protected sealed override void OnMouseDown(MouseEventArgs e) {
        var cme = new CanvasMouseEventArgs(e, _zoom, _offsetX, _offsetY);
        MouseDownCanvas = cme.CanvasPoint;
        base.OnMouseDown(e);
        OnMouseDown(cme);
    }

    protected virtual void OnMouseDown(CanvasMouseEventArgs e) { }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        ControlPressing = false;
    }

    protected sealed override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        OnMouseMove(new CanvasMouseEventArgs(e, _zoom, _offsetX, _offsetY));
    }

    protected virtual void OnMouseMove(CanvasMouseEventArgs e) { }

    protected sealed override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);
        MouseDownCanvas = Point.Empty;
        OnMouseUp(new CanvasMouseEventArgs(e, _zoom, _offsetX, _offsetY));
    }

    protected virtual void OnMouseUp(CanvasMouseEventArgs e) { }

    protected sealed override void OnMouseWheel(MouseEventArgs controle) {
        base.OnMouseWheel(controle);

        if (ControlMustPressed && !ControlPressing) { return; }

        Fitting = false;
        var m = new CanvasMouseEventArgs(controle, _zoom, _offsetX, _offsetY);
        if (controle.Delta > 0) {
            Zoom *= 1.5f;
        } else {
            Zoom *= 1f / 1.5f;
        }

        //var canvasBounds = CanvasMaxBounds();
        //ComputeSliders(canvasBounds);
        // m.Canvas Beeinhaltet den Punkt, wo die Maus hinzeigt Maßstabunabhängig.
        // Der Slider ist abhängig vom Maßsstab - sowie die echten Mauskoordinaten ebenfalls.
        // Deswegen die m.Canvas mit dem neuen Zoom-Faktor berechnen umrechen, um auch Masstababhängig zu sein
        // Die Verschiebung der echten Mauskoordinaten berechnen und den Slider auf den Wert setzen.
        OffsetX = m.CanvasX.CanvasToControl(_zoom) - controle.X;
        OffsetY = m.CanvasY.CanvasToControl(_zoom) - controle.Y;

        // Alte Berechnung für Mittig Setzen
        //SliderX.Value = (m.ControlX * _zoom) - (Width / 2) - SliderY.Width
        //SliderY.Value = (m.Y * _zoom) - (Height / 2) - SliderX.Height
    }

    protected virtual void OnOffsetXChanged() {
        Invalidate();
    }

    protected virtual void OnOffsetYChanged() {
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

    private void SliderX_ValueChanged(object sender, System.EventArgs e) => OffsetX = -SliderX.Value;

    private void SliderY_ValueChanged(object sender, System.EventArgs e) => OffsetY = -SliderY.Value;

    private void UpdateScreenshotLayout() {
        // Im Screenshot-Modus immer Zoom-Fit aktivieren
        _zoomFit = ItemCollectionPadItem.ZoomFitValue(CanvasMaxBounds(), AvailableControlPaintArea().Size);
        _zoom = _zoomFit;

        // Bild zentrieren
        var centerPos = ItemCollectionPadItem.FreiraumControl(CanvasMaxBounds(), AvailableControlPaintArea().Size, _zoom);
        _offsetX = centerPos.X;
        _offsetY = centerPos.Y;

        // Fitting aktivieren damit bei Größenänderung automatisch angepasst wird
        Fitting = true;
    }

    #endregion
}