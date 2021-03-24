#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion
using BlueBasics.Enums;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Interfaces;
using System;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.Controls {
    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("ValueChanged")]
    public partial class Slider : IBackgroundNone {

        #region Constructor

        public Slider() : base(false, false) {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            SetNotFocusable();
            SetStyle(System.Windows.Forms.ControlStyles.ContainerControl, true);
        }

        #endregion

        #region  Variablen 

        private double? LastFiredValue;
        private readonly object Lock_UserAction = new();
        private readonly object Lock_RaiseEvent = new();

        private enOrientation _Orientation = enOrientation.Waagerecht;
        private double _Minimum;
        private double _Maximum = 100;
        private double _Value;

        private enDesign _BackStyle;
        private enDesign _SliderStyle;


        private Rectangle _ClickArea;
        private bool _ClickAreaContainsMouse;

        private Rectangle _Slider;
        private bool _SliderContainsMouse;


        private const int _ButtonSize = 18;


        #endregion


        #region  Properties 


        [DefaultValue(0)]
        public new int TabIndex {
            get {
                return 0;
            }

            set {
                base.TabIndex = 0;
            }
        }

        [DefaultValue(false)]
        public new bool TabStop {
            get {
                return false;
            }

            set {
                base.TabStop = false;
            }
        }



        [DefaultValue(enOrientation.Waagerecht)]
        public enOrientation Orientation {
            get {
                return _Orientation;
            }
            set {
                if (value == _Orientation) { return; }
                _Orientation = value;
                GenerateButtons();

            }
        }

        [DefaultValue(0D)]
        public double Minimum {
            get {
                return Math.Min(_Minimum, _Maximum);
            }
            set {
                if (_Minimum == value) { return; }
                _Minimum = value;
                CheckButtonEnabledState();
                Invalidate();
                Value = CheckMinMax(_Value);
            }
        }

        [DefaultValue(100.0D)]
        public double Maximum {
            get {
                return Math.Max(_Minimum, _Maximum);
            }
            set {
                if (_Maximum == value) { return; }
                _Maximum = value;
                CheckButtonEnabledState();
                Invalidate();
                Value = CheckMinMax(_Value);
            }
        }

        [DefaultValue(10.0D)]
        public double LargeChange { get; set; } = 10;


        [DefaultValue(1.0D)]
        public double SmallChange { get; set; } = 1;

        [DefaultValue(1.0D)]
        public double MouseChange { get; set; } = 1;

        [DefaultValue(0D)]
        public double Value {
            get {
                return _Value;
            }
            set {
                value = CheckMinMax(value);

                // Beim Ersten Init noch keinen Raise starten, allerdings muss das Value unabhängig geschehen.
                if (LastFiredValue == null) {
                    LastFiredValue = value;
                    return;
                }

                if (_Value == value) { return; }
                _Value = value;
                Invalidate();

                CheckButtonEnabledState();
                lock (Lock_RaiseEvent) {

                    if (_Value == LastFiredValue) { return; }
                    LastFiredValue = _Value;
                    OnValueChanged();
                }

            }
        }

        #endregion


        #region  Event-Deklarationen 

        public event EventHandler ValueChanged;
        #endregion


        private void OnValueChanged() {
            ValueChanged?.Invoke(this, System.EventArgs.Empty);
        }


        private void But1_Click(object sender, System.EventArgs e) {
            lock (Lock_UserAction) {
                _SliderContainsMouse = false;
                _ClickAreaContainsMouse = false;

                if (!Enabled) { return; }
                Value -= SmallChange;
            }
        }
        private void But2_Click(object sender, System.EventArgs e) {
            lock (Lock_UserAction) {
                _SliderContainsMouse = false;
                _ClickAreaContainsMouse = false;

                if (!Enabled) { return; }
                Value += SmallChange;
            }
        }


        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseDown(e);

            lock (Lock_UserAction) {
                _SliderContainsMouse = _Slider.Contains(e.X, e.Y);
                _ClickAreaContainsMouse = _ClickArea.Contains(e.X, e.Y);

                if (!Enabled) { return; }
                Invalidate();
            }
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseMove(e);

            lock (Lock_UserAction) {
                _SliderContainsMouse = _Slider.Contains(e.X, e.Y);
                _ClickAreaContainsMouse = _ClickArea.Contains(e.X, e.Y);


                if (!Enabled) { return; }
                if (e.Button == System.Windows.Forms.MouseButtons.Left) { DoMouseAction(e, true); }
            }
        }


        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseUp(e);

            lock (Lock_UserAction) {
                _SliderContainsMouse = _Slider.Contains(e.X, e.Y);
                _ClickAreaContainsMouse = _ClickArea.Contains(e.X, e.Y);

                if (!Enabled) { return; }
                if (e.Button == System.Windows.Forms.MouseButtons.Left) { DoMouseAction(e, false); }

                Invalidate();
            }
        }


        public new bool Focused() {
            if (base.Focused) { return true; }
            if (But1.Focused) { return true; }
            if (But2.Focused) { return true; }
            return false;
        }


        private void GenerateButtons() {

            _SliderContainsMouse = false;
            _ClickAreaContainsMouse = false;

            if (_Orientation == enOrientation.Waagerecht) {
                _BackStyle = enDesign.Slider_Hintergrund_Waagerecht;
                _SliderStyle = enDesign.Button_Slider_Waagerecht;
                But1.SetBounds(0, 0, _ButtonSize, Height);
                But1.ImageCode = "Pfeil_Links_Scrollbar|8|||||0";
                But2.SetBounds(Width - _ButtonSize, 0, _ButtonSize, Height);
                But2.ImageCode = "Pfeil_Rechts_Scrollbar|8|||||0";

                _ClickArea = new Rectangle(DisplayRectangle.Left + But1.Width, DisplayRectangle.Top, DisplayRectangle.Width - But1.Width - But2.Width, DisplayRectangle.Height);

            } else {
                _BackStyle = enDesign.Slider_Hintergrund_Senkrecht;
                _SliderStyle = enDesign.Button_Slider_Senkrecht;
                But1.ImageCode = "Pfeil_Oben_Scrollbar|8|||||0";
                But1.SetBounds(0, 0, Width, _ButtonSize);
                But2.ImageCode = "Pfeil_Unten_Scrollbar|8|||||0";
                But2.SetBounds(0, Height - _ButtonSize, Width, _ButtonSize);

                _ClickArea = new Rectangle(DisplayRectangle.Top, DisplayRectangle.Top + But1.Height, DisplayRectangle.Width, DisplayRectangle.Height - But1.Height - But2.Height);
            }

            Invalidate();

        }





        private double CheckMinMax(double ValueToCheck) {
            var _Min = Math.Min(_Minimum, _Maximum);
            var _Max = Math.Max(_Minimum, _Maximum);

            if (ValueToCheck < _Min) { return _Min; }
            if (ValueToCheck > _Max) { return _Max; }
            return ValueToCheck;
        }

        private void DoMouseAction(System.Windows.Forms.MouseEventArgs e, bool MouseisMoving) {
            double TestVal = 0;

            _ClickAreaContainsMouse = _ClickArea.Contains(e.X, e.Y);
            if (!_ClickAreaContainsMouse && !MouseisMoving) { return; }
            if (_SliderContainsMouse && !MouseisMoving) { return; }
            if (_ClickArea.Width <= 0 || _ClickArea.Height <= 0) { return; }


            if (_Orientation == enOrientation.Waagerecht) {
                TestVal = Minimum + (e.X - _ClickArea.Left - _Slider.Width / 2.0) / (_ClickArea.Width - _Slider.Width) * (Maximum - Minimum);
            } else {
                TestVal = Minimum + (e.Y - _ClickArea.Top - _Slider.Height / 2.0) / (_ClickArea.Height - _Slider.Height) * (Maximum - Minimum);
            }


            TestVal = CheckMinMax((int)(TestVal / MouseChange) * MouseChange);


            if (!MouseisMoving) {
                if (TestVal > _Value) {
                    TestVal = _Value + LargeChange;
                } else {
                    TestVal = _Value - LargeChange;
                }
            }

            if (Math.Abs(TestVal - _Value) < 0.00001) { return; }

            Value = TestVal;
        }



        protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseWheel(e);
            if (!Enabled) { return; }
            if (e.Delta > 0) { But1_Click(But1, e); }
            if (e.Delta < 0) { But2_Click(But2, e); }
        }

        protected override void DrawControl(Graphics gr, enStates state) {
            var vState_Back = state;
            var vState_Slider = state;

            _ClickAreaContainsMouse = _ClickArea.Contains(MousePos().X, MousePos().Y);

            var Proz = (_Value - Minimum) / (Maximum - Minimum);

            if (Maximum - Minimum > 0) {

                if (_Orientation == enOrientation.Waagerecht) {
                    _Slider = new Rectangle((int)(_ClickArea.Left + Proz * (_ClickArea.Width - But1.Width)), 0, But1.Width, But1.Height);
                } else {
                    _Slider = new Rectangle(0, (int)(_ClickArea.Top + Proz * (_ClickArea.Height - But1.Height)), But1.Width, But1.Height);
                }

                _SliderContainsMouse = _Slider.Contains(MousePos());

            } else {
                _Slider.Width = 0;
                _Slider.Height = 0;
                _SliderContainsMouse = false;
            }


            if (Convert.ToBoolean(state & enStates.Standard_MouseOver)) {
                if (_SliderContainsMouse) {
                    vState_Back ^= enStates.Standard_MouseOver;
                } else {
                    vState_Slider ^= enStates.Standard_MouseOver;
                }
            }

            if (Convert.ToBoolean(state & enStates.Standard_MousePressed)) {
                if (_SliderContainsMouse) {
                    vState_Back ^= enStates.Standard_MousePressed;
                } else {
                    vState_Slider ^= enStates.Standard_MousePressed;
                }
            }

            Skin.Draw_Back(gr, _BackStyle, vState_Back, _ClickArea, this, true);
            Skin.Draw_Border(gr, _BackStyle, vState_Back, _ClickArea);


            if (Maximum - Minimum > 0) {
                Skin.Draw_Back(gr, _SliderStyle, vState_Slider, _Slider, this, false);
                Skin.Draw_Border(gr, _SliderStyle, vState_Slider, _Slider);
            }


        }

        protected override void OnMouseEnter(System.EventArgs e) {
            base.OnMouseEnter(e);

            _SliderContainsMouse = _Slider.Contains(MousePos().X, MousePos().Y);
            _ClickAreaContainsMouse = _ClickArea.Contains(MousePos().X, MousePos().Y);

            if (!Enabled) { return; }
            Invalidate();
        }


        protected override void OnMouseLeave(System.EventArgs e) {
            base.OnMouseLeave(e);

            _SliderContainsMouse = false;
            _ClickAreaContainsMouse = false;

            if (!Enabled) { return; }
            Invalidate();
        }


        protected override void OnSizeChanged(System.EventArgs e) {
            base.OnSizeChanged(e);

            GenerateButtons();
        }

        protected override void OnEnabledChanged(System.EventArgs e) {
            base.OnEnabledChanged(e);
            CheckButtonEnabledState();
        }


        private void CheckButtonEnabledState() {

            var ol1 = But1.Enabled;
            var ol2 = But1.Enabled;

            if (!Enabled) {
                But1.Enabled = false;
                But2.Enabled = false;
            } else {
                if (_ClickArea.Width <= 0 || _ClickArea.Height <= 0) {
                    But1.Enabled = _Value > Minimum;
                    But2.Enabled = _Value < Maximum;

                } else {
                    But1.Enabled = true;
                    But2.Enabled = true;
                }
            }

            if (ol1 != But1.Enabled) { But1.Invalidate(); }
            if (ol2 != But2.Enabled) { But2.Invalidate(); }

        }

        internal void DoMouseWheel(System.Windows.Forms.MouseEventArgs e) {
            OnMouseWheel(e);
        }
    }
}
