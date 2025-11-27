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
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Interfaces;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Constants;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent(nameof(ValueChanged))]
public partial class Slider : IBackgroundNone {

    #region Fields

    private const int ButtonSize = 18;

    private readonly object _lock = new();

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

    public Slider() : base(false, false, true) {
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
        get {
            lock (_lock) {
                return Math.Max(_minimum, _maximum);
            }
        }
        set {
            lock (_lock) {
                if (Math.Abs(_maximum - value) < DefaultTolerance) { return; }
                _maximum = value;
            }

            CheckButtonEnabledState();
            Invalidate();

            lock (_lock) {
                Value = CheckMinMax(_value);
            }
        }
    }

    [DefaultValue(0f)]
    public float Minimum {
        get {
            lock (_lock) {
                return Math.Min(_minimum, _maximum);
            }
        }
        set {
            lock (_lock) {
                if (Math.Abs(_minimum - value) < DefaultTolerance) { return; }
                _minimum = value;
            }

            CheckButtonEnabledState();
            Invalidate();

            lock (_lock) {
                Value = CheckMinMax(_value);
            }
        }
    }

    [DefaultValue(1f)]
    public float MouseChange { get; set; } = 1;

    [DefaultValue(Orientation.Waagerecht)]
    public Orientation Orientation {
        get {
            lock (_lock) {
                return _orientation;
            }
        }
        set {
            if (value == _orientation) { return; }

            lock (_lock) {
                if (value == _orientation) { return; }
                _orientation = value;
            }

            GenerateButtons();
        }
    }

    [DefaultValue(1f)]
    public float SmallChange { get; set; } = 1;

    [DefaultValue(0)]
    public new int TabIndex {
        get => 0;
  
        set => base.TabIndex = 0;
    }

    [DefaultValue(false)]
    public new bool TabStop {
        get => false;
  
        set => base.TabStop = false;
    }

    [DefaultValue(0f)]
    public float Value {
        get {
            lock (_lock) {
                return _value;
            }
        }
        set {
            lock (_lock) {
                value = CheckMinMax(value);
            }

            lock (_lock) {
                // Beim ersten Init noch keinen Raise starten
                if (_lastFiredValue == null) {
                    _lastFiredValue = value;
                    return;
                }
                if (Math.Abs(_value - value) < DefaultTolerance) { return; }
                _value = value;
                _lastFiredValue = _value;
            }

            Invalidate();
            CheckButtonEnabledState();
            OnValueChanged();
        }
    }

    #endregion

    #region Methods

    public new bool Focused() => base.Focused || But1.Focused || But2.Focused;

    internal void DoMouseWheel(MouseEventArgs e) => OnMouseWheel(e);

    protected override void DrawControl(Graphics gr, States state) {
        var vStateBack = state;
        var vStateSlider = state;

        Rectangle clickArea;
        Rectangle slider;
        bool sliderContainsMouse;
        float value, minimum, maximum;
        Orientation orientation;

        lock (_lock) {
            clickArea = _clickArea;
            value = _value;
            minimum = Minimum;
            maximum = Maximum;
            orientation = _orientation;
        }

        var mousePos = MousePos();
        var clickAreaContainsMouse = clickArea.Contains(mousePos.X, mousePos.Y);

        var proz = maximum - minimum > 0 ? (value - minimum) / (maximum - minimum) : 0;

        if (maximum - minimum > 0) {
            slider = orientation == Orientation.Waagerecht
                ? new Rectangle((int)(clickArea.Left + (proz * (clickArea.Width - But1.Width))), 0, But1.Width, But1.Height)
                : new Rectangle(0, (int)(clickArea.Top + (proz * (clickArea.Height - But1.Height))), But1.Width, But1.Height);
            sliderContainsMouse = slider.Contains(mousePos);
        } else {
            slider = new Rectangle();
            sliderContainsMouse = false;
        }

        // Cache-Update für nachfolgende Aufrufe
        lock (_lock) {
            _clickAreaContainsMouse = clickAreaContainsMouse;
            _sliderContainsMouse = sliderContainsMouse;
            _slider = slider;
        }

        if (state.HasFlag(States.Standard_MouseOver)) {
            if (sliderContainsMouse) {
                vStateBack ^= States.Standard_MouseOver;
            } else {
                vStateSlider ^= States.Standard_MouseOver;
            }
        }

        if (state.HasFlag(States.Standard_MousePressed)) {
            if (sliderContainsMouse) {
                vStateBack ^= States.Standard_MousePressed;
            } else {
                vStateSlider ^= States.Standard_MousePressed;
            }
        }

        Skin.Draw_Back(gr, _backStyle, vStateBack, clickArea, this, true);
        Skin.Draw_Border(gr, _backStyle, vStateBack, clickArea);

        if (maximum - minimum <= 0) { return; }

        Skin.Draw_Back(gr, _sliderStyle, vStateSlider, slider, this, false);
        Skin.Draw_Border(gr, _sliderStyle, vStateSlider, slider);
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);
        CheckButtonEnabledState();
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);

        if (!Enabled) { return; }

        lock (_lock) {
            _sliderContainsMouse = _slider.Contains(e.X, e.Y);
            _clickAreaContainsMouse = _clickArea.Contains(e.X, e.Y);
        }

        Invalidate();
    }

    protected override void OnMouseEnter(System.EventArgs e) {
        base.OnMouseEnter(e);

        if (!Enabled) { return; }

        var mousePos = MousePos();
        lock (_lock) {
            _sliderContainsMouse = _slider.Contains(mousePos.X, mousePos.Y);
            _clickAreaContainsMouse = _clickArea.Contains(mousePos.X, mousePos.Y);
        }

        Invalidate();
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);

        if (!Enabled) { return; }

        lock (_lock) {
            _sliderContainsMouse = false;
            _clickAreaContainsMouse = false;
        }

        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);

        if (!Enabled) { return; }

        lock (_lock) {
            _sliderContainsMouse = _slider.Contains(e.X, e.Y);
            _clickAreaContainsMouse = _clickArea.Contains(e.X, e.Y);
        }

        if (e.Button != MouseButtons.Left) { return; }

        DoMouseAction(e, true);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);

        if (!Enabled) { return; }

        lock (_lock) {
            _sliderContainsMouse = _slider.Contains(e.X, e.Y);
            _clickAreaContainsMouse = _clickArea.Contains(e.X, e.Y);
        }

        if (e.Button == MouseButtons.Left) {
            DoMouseAction(e, false);
        }

        Invalidate();
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
        if (!Enabled) { return; }

        lock (_lock) {
            _sliderContainsMouse = false;
            _clickAreaContainsMouse = false;
        }

        Value -= SmallChange;
    }

    private void But2_Click(object sender, System.EventArgs e) {
        if (!Enabled) { return; }

        lock (_lock) {
            _sliderContainsMouse = false;
            _clickAreaContainsMouse = false;
        }

        Value += SmallChange;
    }

    private void CheckButtonEnabledState() {
        if (!Enabled) {
            But1.Enabled = false;
            But2.Enabled = false;
            return;
        }

        bool newBut1Enabled, newBut2Enabled;

        lock (_lock) {
            if (_clickArea.Width <= 0 || _clickArea.Height <= 0) {
                newBut1Enabled = _value > Minimum;
                newBut2Enabled = _value < Maximum;
            } else {
                newBut1Enabled = true;
                newBut2Enabled = true;
            }
        }

        But1.Enabled = newBut1Enabled;
        But2.Enabled = newBut2Enabled;
    }

    private float CheckMinMax(float valueToCheck) {
        var min = Math.Min(_minimum, _maximum);
        var max = Math.Max(_minimum, _maximum);
        return valueToCheck < min ? min : valueToCheck > max ? max : valueToCheck;
    }

    private void DoMouseAction(MouseEventArgs e, bool mouseisMoving) {
        Rectangle clickArea;
        Rectangle slider;
        float value, minimum, maximum, mouseChange, largeChange;
        Orientation orientation;

        lock (_lock) {
            clickArea = _clickArea;
            slider = _slider;
            value = _value;
            minimum = Minimum;
            maximum = Maximum;
            mouseChange = MouseChange;
            largeChange = LargeChange;
            orientation = _orientation;
        }

        var clickAreaContainsMouse = clickArea.Contains(e.X, e.Y);
        var sliderContainsMouse = slider.Contains(e.X, e.Y);

        if (!clickAreaContainsMouse && !mouseisMoving) { return; }
        if (sliderContainsMouse && !mouseisMoving) { return; }
        if (clickArea.Width <= 0 || clickArea.Height <= 0) { return; }

        var testVal = orientation == Orientation.Waagerecht
            ? minimum + ((e.X - clickArea.Left - (slider.Width / 2f)) / (clickArea.Width - slider.Width) * (maximum - minimum))
            : minimum + ((e.Y - clickArea.Top - (slider.Height / 2f)) / (clickArea.Height - slider.Height) * (maximum - minimum));

        testVal = CheckMinMax((int)(testVal / mouseChange) * mouseChange);

        if (!mouseisMoving) {
            testVal = testVal > value ? value + largeChange : value - largeChange;
        }

        if (Math.Abs(testVal - value) < 0.00001) { return; }

        Develop.SetUserDidSomething();
        Value = testVal;
    }

    private void GenerateButtons() {
        Orientation orientation;

        lock (_lock) {
            _sliderContainsMouse = false;
            _clickAreaContainsMouse = false;
            orientation = _orientation;
        }

        if (orientation == Orientation.Waagerecht) {
            _backStyle = Design.Slider_Hintergrund_Waagerecht;
            _sliderStyle = Design.Button_Slider_Waagerecht;
            But1.SetBounds(0, 0, ButtonSize, Height);
            But1.ImageCode = "Pfeil_Links_Scrollbar|8|||||0";
            But2.SetBounds(Width - ButtonSize, 0, ButtonSize, Height);
            But2.ImageCode = "Pfeil_Rechts_Scrollbar|8|||||0";

            lock (_lock) {
                _clickArea = new Rectangle(DisplayRectangle.Left + But1.Width, DisplayRectangle.Top, DisplayRectangle.Width - But1.Width - But2.Width, DisplayRectangle.Height);
            }

            Invalidate();
            return;
        }

        _backStyle = Design.Slider_Hintergrund_Senkrecht;
        _sliderStyle = Design.Button_Slider_Senkrecht;
        But1.ImageCode = "Pfeil_Oben_Scrollbar|8|||||0";
        But1.SetBounds(0, 0, Width, ButtonSize);
        But2.ImageCode = "Pfeil_Unten_Scrollbar|8|||||0";
        But2.SetBounds(0, Height - ButtonSize, Width, ButtonSize);

        lock (_lock) {
            _clickArea = new Rectangle(DisplayRectangle.Left, DisplayRectangle.Top + But1.Height, DisplayRectangle.Width, DisplayRectangle.Height - But1.Height - But2.Height);
        }

        Invalidate();
    }

    private void OnValueChanged() => ValueChanged?.Invoke(this, System.EventArgs.Empty);

    #endregion
}