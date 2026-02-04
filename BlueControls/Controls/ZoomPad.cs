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
using BlueBasics.Classes;
using BlueBasics.Enums;
using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionPad;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Constants;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public abstract partial class ZoomPad : GenericControl, IBackgroundNone {
    #region Fields

    public static readonly Pen PenGray = new(Color.FromArgb(40, 0, 0, 0));

    private Size _lastSize;

    private bool _lastSliderXVisible;

    private bool _lastSliderYVisible;
    private float _zoomFit = 1;

    #endregion

    #region Constructors

    protected ZoomPad() : base(true, true, false) => InitializeComponent();

    #endregion

    #region Properties

    /// <summary>
    /// Gibt den Zeichenbereich zurück. Entspricht der Control-Größe abzüglich der Slider-Breite/Höhe
    /// </summary>
    /// <returns></returns>
    public Rectangle AvailableControlPaintArea {
        get {
            // Schneller Check ob sich etwas geändert hat
            if (_lastSize == Size &&
                _lastSliderXVisible == SliderX.Visible &&
                _lastSliderYVisible == SliderY.Visible) {
                return field;
            }

            // Nur bei Änderungen neu berechnen

            // Cache aktualisieren
            field = new(
                0,
                0,
                Size.Width - (SliderY.Visible ? SliderY.Width : 0),
                Size.Height - (SliderX.Visible ? SliderX.Height : 0)
            );
            _lastSize = Size;
            _lastSliderXVisible = SliderX.Visible;
            _lastSliderYVisible = SliderY.Visible;

            return field;
        }
    }

    public abstract bool ControlMustPressedForZoomWithWheel { get; }

    public bool Fitting { get; private set; }

    public new bool Focused => base.Focused || SliderX.Focused || SliderY.Focused;

    public bool IsMaxYOffset => SliderY.Maximum < 6 || Math.Abs(OffsetY + SliderY.Maximum) < IntTolerance;

    /// <summary>
    /// Die Koordinaten, an der Stelle der Mausknopf gedrückt wurde.
    /// </summary>
    public CanvasMouseEventArgs? MouseDownData { get; protected set; }

    [DefaultValue(0f)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int OffsetX {
        get => ShowSliderX && SlideAndZoomAllowed ? field : 0;
        set {
            if (!ShowSliderX || !SlideAndZoomAllowed) { value = 0; }

            value = (int)Math.Min(value, -SliderX.Minimum);
            value = (int)Math.Max(value, -SliderX.Maximum);

            if (Math.Abs(value - field) < DefaultTolerance) { return; }
            field = value;
            OnOffsetXChanged();
        }
    }

    [DefaultValue(0f)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int OffsetY {
        get => SlideAndZoomAllowed ? field : 0;
        set {
            if (!SlideAndZoomAllowed) { value = 0; }

            value = (int)Math.Min(value, -SliderY.Minimum);
            value = (int)Math.Max(value, -SliderY.Maximum);

            if (Math.Abs(value - field) < DefaultTolerance) { return; }
            field = value;
            OnOffsetYChanged();
        }
    }

    [DefaultValue(true)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool SlideAndZoomAllowed {
        get;
        set {
            if (field == value) { return; }
            field = value;

            if (!value) {
                ZoomFit();
            }

            Invalidate();
        }
    } = true;

    [DefaultValue(1f)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float Zoom {
        get => SlideAndZoomAllowed ? field : 1f;
        set {
            if (!SlideAndZoomAllowed) { return; }
            value = Math.Max(_zoomFit / 10f, value);
            value = Math.Min(20, value);

            if (Math.Abs(value - field) < DefaultTolerance) { return; }
            field = value;
            OnZoomChanged();
        }
    } = 1;

    [DefaultValue(true)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    protected bool AutoCenter { get; set; } = true;

    protected RectangleF CanvasMaxBounds {
        get {
            if (!field.IsEmpty) { return field; }
            field = CalculateCanvasMaxBounds();
            return field;
        }
        private set;
    }

    protected abstract bool ShowSliderX { get; }

    protected abstract int SmallChangeY { get; }

    /// <summary>
    /// True, wenn aktuell die STRG-Taste gedrückt wird.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    private bool ControlPressing { get; set; }

    #endregion

    #region Methods

    public static bool ScaleWarnung() {
        if (Skin.Scale is > 0.98f and < 1.02f) { return false; }
        Forms.MessageBox.Show("Diese Funktion kann mit ihrer aktuellen Schriftgrößeneinstellung<br>leider nicht möglich.", ImageCode.Warnung, "OK");
        return true;
    }

    //public RectangleF AvailablePaintAreaScaled() => AvailableControlPaintArea.CanvasToControlX(Zoom, 0, 0, true);

    public void DoZoom(bool zoomIn) {
        var nz = Zoom;

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
        result.ParseableAdd("Zoom", Zoom);
        result.ParseableAdd("SliderX", SliderX.Value);
        result.ParseableAdd("SliderY", SliderY.Value);
        return result;
    }

    public void ZoomFit() {
        if (IsDisposed) { return; }
        var controlBounds = AvailableControlPaintArea;
        _zoomFit = ItemCollectionPadItem.ZoomFitValue(CanvasMaxBounds, controlBounds.Size);
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

    protected abstract RectangleF CalculateCanvasMaxBounds();

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }

        var tmpCanvasMaxBounds = CanvasMaxBounds;

        var freiraumNoSliders = ItemCollectionPadItem.FreiraumControl(tmpCanvasMaxBounds, Size, Zoom);
        var freiraumBoth = ItemCollectionPadItem.FreiraumControl(tmpCanvasMaxBounds, new Size(Size.Width - SliderY.Width, Size.Height - SliderX.Height), Zoom);

        // Erst die Slider-Sichtbarkeit bestimmen
        SliderY.Visible = SlideAndZoomAllowed && freiraumNoSliders.Y < 0 && freiraumBoth.Y < 0;
        SliderX.Visible = ShowSliderX && SlideAndZoomAllowed &&
                         (SliderY.Visible ? freiraumBoth.X < 0 : freiraumNoSliders.X < 0);

        // Jetzt mit finaler Größe arbeiten - NACH der Slider-Schaltung
        var controlArea = AvailableControlPaintArea;
        var freiraumControl = ItemCollectionPadItem.FreiraumControl(tmpCanvasMaxBounds, controlArea.Size, Zoom);

        if (SliderX.Visible) {
            SliderX.Minimum = tmpCanvasMaxBounds.Right.CanvasToControl(Zoom) - controlArea.Right - tmpCanvasMaxBounds.Left.CanvasToControl(Zoom);
            SliderX.Maximum = tmpCanvasMaxBounds.Left.CanvasToControl(Zoom) - controlArea.Left;
            SliderX.LargeChange = controlArea.Width;
            SliderX.Value = -OffsetX;
        } else {
            if (!MousePressing()) {
                var val = 0;
                if (AutoCenter) { val = (freiraumControl.X - tmpCanvasMaxBounds.Left.CanvasToControl(Zoom)) / 2; }
                if (!SlideAndZoomAllowed) { val = -freiraumControl.X; }
                SliderX.Minimum = -val;
                SliderX.Maximum = -val;
                SliderX.Value = -val;
            }
        }

        if (SliderY.Visible) {
            SliderY.Maximum = tmpCanvasMaxBounds.Bottom.CanvasToControl(Zoom) - controlArea.Bottom - tmpCanvasMaxBounds.Top.CanvasToControl(Zoom);
            SliderY.Minimum = tmpCanvasMaxBounds.Top.CanvasToControl(Zoom) - controlArea.Top;
            SliderY.LargeChange = controlArea.Height;
            SliderY.Value = -OffsetY;
        } else {
            if (!MousePressing()) {
                var val = 0;
                if (AutoCenter) { val = (freiraumControl.Y - tmpCanvasMaxBounds.Top.CanvasToControl(Zoom)) / 2; }
                if (!SlideAndZoomAllowed) { val = -freiraumControl.Y; }
                SliderY.Minimum = -val;
                SliderY.Maximum = -val;
                SliderY.Value = -val;
            }
        }

        base.DrawControl(gr, state);
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
        CanvasMaxBounds = RectangleF.Empty;

        Invalidate();
    }

    protected override void OnDoubleClick(System.EventArgs e) {
        base.OnDoubleClick(e);
        if (MouseDownData == null) { return; }
        OnDoubleClick(MouseDownData);
    }

    protected virtual void OnDoubleClick(CanvasMouseEventArgs e) { }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);

        if (IsDisposed) { return; }

        ControlPressing = e.Modifiers == Keys.Control;

        switch (e.KeyCode) {
            case Keys.PageDown:
                if (SliderY.Enabled) {
                    OffsetY -= (int)SliderY.LargeChange;
                }
                break;

            case Keys.PageUp: //Bildab
                if (SliderY.Enabled) {
                    OffsetY += (int)SliderY.LargeChange;
                }
                break;

            case Keys.Home:
                if (SliderY.Enabled) {
                    OffsetY = (int)SliderY.Minimum;
                }
                break;

            case Keys.End:
                if (SliderY.Enabled) {
                    OffsetY = (int)SliderY.Maximum;
                }
                break;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e) {
        base.OnKeyUp(e);
        ControlPressing = false;
    }

    protected override sealed void OnMouseDown(MouseEventArgs e) {
        if ((SliderX.Visible && SliderX.Bounds.Contains(e.Location)) ||
            (SliderY.Visible && SliderY.Bounds.Contains(e.Location))) {
            // Windows-Bug, manchmal stimmen die States nicht
            // Diabled/Enabled korrigiert den State
            SliderX.Enabled = false;
            SliderY.Enabled = false;
            SliderX.Enabled = true;
            SliderY.Enabled = true;
            return;
        }

        var cme = new CanvasMouseEventArgs(e, Zoom, OffsetX, OffsetY);
        MouseDownData = cme;
        base.OnMouseDown(e);
        OnMouseDown(cme);
    }

    protected virtual void OnMouseDown(CanvasMouseEventArgs e) { }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        ControlPressing = false;
    }

    protected override sealed void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        OnMouseMove(new CanvasMouseEventArgs(e, Zoom, OffsetX, OffsetY));
    }

    protected virtual void OnMouseMove(CanvasMouseEventArgs e) { }

    protected override sealed void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);
        MouseDownData = null;
        OnMouseUp(new CanvasMouseEventArgs(e, Zoom, OffsetX, OffsetY));
    }

    protected virtual void OnMouseUp(CanvasMouseEventArgs e) { }

    protected override sealed void OnMouseWheel(MouseEventArgs e) {
        base.OnMouseWheel(e);

        if (ControlMustPressedForZoomWithWheel && !ControlPressing) {
            var v = SmallChangeY.CanvasToControl(Zoom);
            if (SliderY.Visible && e.Delta > 0) { OffsetY += v; }
            if (SliderY.Visible && e.Delta < 0) { OffsetY -= v; }
            return;
        }

        Fitting = false;
        var m = new CanvasMouseEventArgs(e, Zoom, OffsetX, OffsetY);
        if (e.Delta > 0) {
            Zoom *= 1.5f;
        } else {
            Zoom *= 1f / 1.5f;
        }
        OffsetX = m.CanvasX.CanvasToControl(Zoom) - e.X;
        OffsetY = m.CanvasY.CanvasToControl(Zoom) - e.Y;
    }

    protected virtual void OnOffsetXChanged() => Invalidate();

    protected virtual void OnOffsetYChanged() => Invalidate();

    protected override void OnSizeChanged(System.EventArgs e) {
        if (!SlideAndZoomAllowed || Fitting) {
            ZoomFit();
        }
        base.OnSizeChanged(e);
    }

    protected virtual void OnZoomChanged() => Invalidate();

    private void SliderX_ValueChanged(object sender, System.EventArgs e) => OffsetX = -(int)SliderX.Value;

    private void SliderY_ValueChanged(object sender, System.EventArgs e) => OffsetY = -(int)SliderY.Value;

    #endregion
}