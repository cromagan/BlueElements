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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Interfaces;
using Orientation = BlueBasics.Enums.Orientation;
using static BlueBasics.Constants;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("ValueChanged")]
public partial class Slider : IBackgroundNone {

    #region Fields

    private const int ButtonSize = 18;

    private readonly object _lockRaiseEvent = new();

    private readonly object _lockUserAction = new();

    private Design _backStyle;

    private Rectangle _clickArea;

    private bool _clickAreaContainsMouse;

    private float? _lastFiredValue;
    private float _maximum = 100;

    private float _minimum;

    private Orientation _orientation = Orientation.Waagerecht;

    private Rectangle _slider;

    private bool _sliderContainsMouse;

    private Design _sliderStyle;

    private float _value;

    #endregion

    #region Constructors

    public Slider() : base(false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        SetNotFocusable();
        SetStyle(ControlStyles.ContainerControl, true);
    }

    #endregion

    #region Events

    public event EventHandler? ValueChanged;

    #endregion

    #region Properties

    [DefaultValue(10f)]
    public float LargeChange { get; set; } = 10;

    [DefaultValue(100f)]
    public float Maximum {
        get => Math.Max(_minimum, _maximum);
        set {
            if (Math.Abs(_maximum - value) < DefaultTolerance) { return; }
            _maximum = value;
            CheckButtonEnabledState();
            Invalidate();
            Value = CheckMinMax(_value);
        }
    }

    [DefaultValue(0f)]
    public float Minimum {
        get => Math.Min(_minimum, _maximum);
        set {
            if (Math.Abs(_minimum - value) < DefaultTolerance) { return; }
            _minimum = value;
            CheckButtonEnabledState();
            Invalidate();
            Value = CheckMinMax(_value);
        }
    }

    [DefaultValue(1f)]
    public float MouseChange { get; set; } = 1;

    [DefaultValue(Orientation.Waagerecht)]
    public Orientation Orientation {
        get => _orientation;
        set {
            if (value == _orientation) { return; }
            _orientation = value;
            GenerateButtons();
        }
    }

    [DefaultValue(1f)]
    public float SmallChange { get; set; } = 1;

    [DefaultValue(0)]
    public new int TabIndex {
        get => 0;
        // ReSharper disable once ValueParameterNotUsed
        set => base.TabIndex = 0;
    }

    [DefaultValue(false)]
    public new bool TabStop {
        get => false;
        // ReSharper disable once ValueParameterNotUsed
        set => base.TabStop = false;
    }

    [DefaultValue(0f)]
    public float Value {
        get => _value;
        set {
            value = CheckMinMax(value);
            // Beim Ersten Init noch keinen Raise starten, allerdings muss das Value unabhängig geschehen.
            if (_lastFiredValue == null) {
                _lastFiredValue = value;
                return;
            }
            if (Math.Abs(_value - value) < DefaultTolerance) { return; }
            _value = value;
            Invalidate();
            CheckButtonEnabledState();
            lock (_lockRaiseEvent) {
                _lastFiredValue = _value;
                OnValueChanged();
            }
        }
    }

    #endregion

    #region Methods

    public new bool Focused() => base.Focused || But1.Focused || But2.Focused;

    internal void DoMouseWheel(MouseEventArgs e) => OnMouseWheel(e);

    protected override void DrawControl(Graphics gr, States state) {
        var vStateBack = state;
        var vStateSlider = state;
        _clickAreaContainsMouse = _clickArea.Contains(MousePos().X, MousePos().Y);
        var proz = (_value - Minimum) / (Maximum - Minimum);
        if (Maximum - Minimum > 0) {
            _slider = _orientation == Orientation.Waagerecht
                ? new Rectangle((int)(_clickArea.Left + (proz * (_clickArea.Width - But1.Width))), 0, But1.Width, But1.Height)
                : new Rectangle(0, (int)(_clickArea.Top + (proz * (_clickArea.Height - But1.Height))), But1.Width, But1.Height);
            _sliderContainsMouse = _slider.Contains(MousePos());
        } else {
            _slider.Width = 0;
            _slider.Height = 0;
            _sliderContainsMouse = false;
        }
        if (state.HasFlag(States.Standard_MouseOver)) {
            if (_sliderContainsMouse) {
                vStateBack ^= States.Standard_MouseOver;
            } else {
                vStateSlider ^= States.Standard_MouseOver;
            }
        }
        if (state.HasFlag(States.Standard_MousePressed)) {
            if (_sliderContainsMouse) {
                vStateBack ^= States.Standard_MousePressed;
            } else {
                vStateSlider ^= States.Standard_MousePressed;
            }
        }
        Skin.Draw_Back(gr, _backStyle, vStateBack, _clickArea, this, true);
        Skin.Draw_Border(gr, _backStyle, vStateBack, _clickArea);
        if (Maximum - Minimum > 0) {
            Skin.Draw_Back(gr, _sliderStyle, vStateSlider, _slider, this, false);
            Skin.Draw_Border(gr, _sliderStyle, vStateSlider, _slider);
        }
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);
        CheckButtonEnabledState();
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);
        lock (_lockUserAction) {
            _sliderContainsMouse = _slider.Contains(e.X, e.Y);
            _clickAreaContainsMouse = _clickArea.Contains(e.X, e.Y);
            if (!Enabled) { return; }
            Invalidate();
        }
    }

    protected override void OnMouseEnter(System.EventArgs e) {
        base.OnMouseEnter(e);
        _sliderContainsMouse = _slider.Contains(MousePos().X, MousePos().Y);
        _clickAreaContainsMouse = _clickArea.Contains(MousePos().X, MousePos().Y);
        if (!Enabled) { return; }
        Invalidate();
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        _sliderContainsMouse = false;
        _clickAreaContainsMouse = false;
        if (!Enabled) { return; }
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        lock (_lockUserAction) {
            _sliderContainsMouse = _slider.Contains(e.X, e.Y);
            _clickAreaContainsMouse = _clickArea.Contains(e.X, e.Y);
            if (!Enabled) { return; }
            if (e.Button == MouseButtons.Left) { DoMouseAction(e, true); }
        }
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);
        lock (_lockUserAction) {
            _sliderContainsMouse = _slider.Contains(e.X, e.Y);
            _clickAreaContainsMouse = _clickArea.Contains(e.X, e.Y);
            if (!Enabled) { return; }
            if (e.Button == MouseButtons.Left) { DoMouseAction(e, false); }
            Invalidate();
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        base.OnMouseWheel(e);
        if (!Enabled) { return; }
        if (e.Delta > 0) { But1_Click(But1, e); }
        if (e.Delta < 0) { But2_Click(But2, e); }
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        base.OnSizeChanged(e);
        GenerateButtons();
    }

    private void But1_Click(object sender, System.EventArgs e) {
        lock (_lockUserAction) {
            _sliderContainsMouse = false;
            _clickAreaContainsMouse = false;
            if (!Enabled) { return; }
            Value -= SmallChange;
        }
    }

    private void But2_Click(object sender, System.EventArgs e) {
        lock (_lockUserAction) {
            _sliderContainsMouse = false;
            _clickAreaContainsMouse = false;
            if (!Enabled) { return; }
            Value += SmallChange;
        }
    }

    private void CheckButtonEnabledState() {
        var ol1 = But1.Enabled;
        var ol2 = But1.Enabled;
        if (!Enabled) {
            But1.Enabled = false;
            But2.Enabled = false;
        } else {
            if (_clickArea.Width <= 0 || _clickArea.Height <= 0) {
                But1.Enabled = _value > Minimum;
                But2.Enabled = _value < Maximum;
            } else {
                But1.Enabled = true;
                But2.Enabled = true;
            }
        }
        if (ol1 != But1.Enabled) { But1.Invalidate(); }
        if (ol2 != But2.Enabled) { But2.Invalidate(); }
    }

    private float CheckMinMax(float valueToCheck) {
        var min = Math.Min(_minimum, _maximum);
        var max = Math.Max(_minimum, _maximum);
        return valueToCheck < min ? min : valueToCheck > max ? max : valueToCheck;
    }

    private void DoMouseAction(MouseEventArgs e, bool mouseisMoving) {
        _clickAreaContainsMouse = _clickArea.Contains(e.X, e.Y);
        if (!_clickAreaContainsMouse && !mouseisMoving) { return; }
        if (_sliderContainsMouse && !mouseisMoving) { return; }
        if (_clickArea.Width <= 0 || _clickArea.Height <= 0) { return; }
        var testVal = _orientation == Orientation.Waagerecht
            ? Minimum + ((e.X - _clickArea.Left - (_slider.Width / 2f)) / (_clickArea.Width - _slider.Width) * (Maximum - Minimum))
            : Minimum + ((e.Y - _clickArea.Top - (_slider.Height / 2f)) / (_clickArea.Height - _slider.Height) * (Maximum - Minimum));
        testVal = CheckMinMax((int)(testVal / MouseChange) * MouseChange);
        if (!mouseisMoving) {
            testVal = testVal > _value ? _value + LargeChange : _value - LargeChange;
        }
        if (Math.Abs(testVal - _value) < 0.00001) { return; }
        Value = testVal;
    }

    private void GenerateButtons() {
        _sliderContainsMouse = false;
        _clickAreaContainsMouse = false;
        if (_orientation == Orientation.Waagerecht) {
            _backStyle = Design.Slider_Hintergrund_Waagerecht;
            _sliderStyle = Design.Button_Slider_Waagerecht;
            But1.SetBounds(0, 0, ButtonSize, Height);
            But1.ImageCode = "Pfeil_Links_Scrollbar|8|||||0";
            But2.SetBounds(Width - ButtonSize, 0, ButtonSize, Height);
            But2.ImageCode = "Pfeil_Rechts_Scrollbar|8|||||0";
            _clickArea = new Rectangle(DisplayRectangle.Left + But1.Width, DisplayRectangle.Top, DisplayRectangle.Width - But1.Width - But2.Width, DisplayRectangle.Height);
        } else {
            _backStyle = Design.Slider_Hintergrund_Senkrecht;
            _sliderStyle = Design.Button_Slider_Senkrecht;
            But1.ImageCode = "Pfeil_Oben_Scrollbar|8|||||0";
            But1.SetBounds(0, 0, Width, ButtonSize);
            But2.ImageCode = "Pfeil_Unten_Scrollbar|8|||||0";
            But2.SetBounds(0, Height - ButtonSize, Width, ButtonSize);
            _clickArea = new Rectangle(DisplayRectangle.Top, DisplayRectangle.Top + But1.Height, DisplayRectangle.Width, DisplayRectangle.Height - But1.Height - But2.Height);
        }
        Invalidate();
    }

    private void OnValueChanged() => ValueChanged?.Invoke(this, System.EventArgs.Empty);

    #endregion
}