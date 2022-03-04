// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BlueControls {

    public sealed class SystemInputHook {

        #region Fields

        private bool _keyIsPressing;

        private System.Windows.Forms.Keys _keyLastKey;

        private bool _mouseIsPressing;

        private System.Windows.Forms.MouseButtons _mouseLastButton;

        private int _mouseLastX;

        private int _mouseLastY;

        [AccessedThroughProperty(nameof(Tim))]
        private System.Windows.Forms.Timer _tim;

        #endregion

        #region Constructors

        public SystemInputHook() => Initialize();

        #endregion

        #region Events

        public event System.EventHandler<System.Windows.Forms.KeyEventArgs> KeyDown;

        public event System.EventHandler<System.Windows.Forms.KeyEventArgs> KeyUp;

        public event System.EventHandler<System.Windows.Forms.MouseEventArgs> MouseDown;

        public event System.EventHandler<System.Windows.Forms.MouseEventArgs> MouseMove;

        public event System.EventHandler<System.Windows.Forms.MouseEventArgs> MouseUp;

        #endregion

        #region Properties

        private System.Windows.Forms.Timer Tim {
            [DebuggerNonUserCode]
            get => _tim;
            [MethodImpl(MethodImplOptions.Synchronized)]
            [DebuggerNonUserCode]
            set {
                if (_tim != null) {
                    _tim.Tick -= Tim_Tick;
                }
                _tim = value;
                if (value != null) {
                    _tim.Tick += Tim_Tick;
                }
            }
        }

        #endregion

        #region Methods

        public void CheckNow() => Tim_Tick(null, null);

        public void DoKeyboard() {
            var k = System.Windows.Forms.Keys.None;
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D1) != 0) {
                k |= System.Windows.Forms.Keys.D1;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D2) != 0) {
                k |= System.Windows.Forms.Keys.D2;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D3) != 0) {
                k |= System.Windows.Forms.Keys.D3;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D4) != 0) {
                k |= System.Windows.Forms.Keys.D4;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D5) != 0) {
                k |= System.Windows.Forms.Keys.D5;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D6) != 0) {
                k |= System.Windows.Forms.Keys.D6;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D7) != 0) {
                k |= System.Windows.Forms.Keys.D7;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D8) != 0) {
                k |= System.Windows.Forms.Keys.D8;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D9) != 0) {
                k |= System.Windows.Forms.Keys.D9;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D0) != 0) {
                k |= System.Windows.Forms.Keys.D0;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad1) != 0) {
                k |= System.Windows.Forms.Keys.NumPad1;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad2) != 0) {
                k |= System.Windows.Forms.Keys.NumPad2;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad3) != 0) {
                k |= System.Windows.Forms.Keys.NumPad3;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad4) != 0) {
                k |= System.Windows.Forms.Keys.NumPad4;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad5) != 0) {
                k |= System.Windows.Forms.Keys.NumPad5;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad6) != 0) {
                k |= System.Windows.Forms.Keys.NumPad6;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad7) != 0) {
                k |= System.Windows.Forms.Keys.NumPad7;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad8) != 0) {
                k |= System.Windows.Forms.Keys.NumPad8;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad9) != 0) {
                k |= System.Windows.Forms.Keys.NumPad9;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad0) != 0) {
                k |= System.Windows.Forms.Keys.NumPad0;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.X) != 0) {
                k |= System.Windows.Forms.Keys.X;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.V) != 0) {
                k |= System.Windows.Forms.Keys.V;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.C) != 0) {
                k |= System.Windows.Forms.Keys.C;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Tab) != 0) {
                k |= System.Windows.Forms.Keys.Tab;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Enter) != 0) {
                k |= System.Windows.Forms.Keys.Enter;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Return) != 0) {
                k |= System.Windows.Forms.Keys.Return;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.ControlKey) != 0) {
                k |= System.Windows.Forms.Keys.Control;
            }
            //    If GetAsyncKeyState(System.Windows.Forms.Keys.ControlKey) Then k = k Or System.Windows.Forms.Keys.ControlKey
            //   If GetAsyncKeyState(System.Windows.Forms.Keys.Menu) Then k = k Or System.Windows.Forms.Keys.Menu
            if (GetAsyncKeyState(System.Windows.Forms.Keys.ShiftKey) != 0) {
                k |= System.Windows.Forms.Keys.Shift;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Right) != 0) {
                k |= System.Windows.Forms.Keys.Right;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Left) != 0) {
                k |= System.Windows.Forms.Keys.Left;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Up) != 0) {
                k |= System.Windows.Forms.Keys.Up;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Down) != 0) {
                k |= System.Windows.Forms.Keys.Down;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Back) != 0) {
                k |= System.Windows.Forms.Keys.Back;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Delete) != 0) {
                k |= System.Windows.Forms.Keys.Delete;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Space) != 0) {
                k |= System.Windows.Forms.Keys.Space;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Add) != 0) {
                k |= System.Windows.Forms.Keys.Add;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Subtract) != 0) {
                k |= System.Windows.Forms.Keys.Subtract;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Multiply) != 0) {
                k |= System.Windows.Forms.Keys.Multiply;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Divide) != 0) {
                k |= System.Windows.Forms.Keys.Divide;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.OemMinus) != 0) {
                k |= System.Windows.Forms.Keys.OemMinus;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.OemPeriod) != 0) {
                k |= System.Windows.Forms.Keys.OemPeriod;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Oemcomma) != 0) {
                k |= System.Windows.Forms.Keys.Oemcomma;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F1) != 0) {
                k |= System.Windows.Forms.Keys.F1;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F2) != 0) {
                k |= System.Windows.Forms.Keys.F2;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F3) != 0) {
                k |= System.Windows.Forms.Keys.F3;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F4) != 0) {
                k |= System.Windows.Forms.Keys.F4;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F5) != 0) {
                k |= System.Windows.Forms.Keys.F5;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F6) != 0) {
                k |= System.Windows.Forms.Keys.F6;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F7) != 0) {
                k |= System.Windows.Forms.Keys.F7;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F8) != 0) {
                k |= System.Windows.Forms.Keys.F8;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F9) != 0) {
                k |= System.Windows.Forms.Keys.F9;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F10) != 0) {
                k |= System.Windows.Forms.Keys.F10;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F11) != 0) {
                k |= System.Windows.Forms.Keys.F11;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F12) != 0) {
                k |= System.Windows.Forms.Keys.F12;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Pause) != 0) {
                k |= System.Windows.Forms.Keys.Pause;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Oem5) != 0) // Zirkumflex ^
            {
                k |= System.Windows.Forms.Keys.Oem5;
            }
            System.Windows.Forms.KeyEventArgs kev = new(k);
            System.Windows.Forms.KeyEventArgs kevold = new(_keyLastKey);
            if (_keyIsPressing) {
                if (k == System.Windows.Forms.Keys.None) {
                    OnKeyUp(kevold);
                    _keyIsPressing = false;
                } else if (k != _keyLastKey) {
                    OnKeyDown(kev);
                }
            } else {
                if (k != System.Windows.Forms.Keys.None) {
                    OnKeyDown(kev);
                    _keyIsPressing = true;
                }
            }
            _keyLastKey = k;
        }

        public void DoMouse() {
            var b = System.Windows.Forms.MouseButtons.None;
            if (GetAsyncKeyState(System.Windows.Forms.Keys.LButton) != 0) {
                b |= System.Windows.Forms.MouseButtons.Left;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.RButton) != 0) {
                b |= System.Windows.Forms.MouseButtons.Right;
            }
            System.Windows.Forms.MouseEventArgs mev = new(b, 0, System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y, 0);
            System.Windows.Forms.MouseEventArgs mevold = new(_mouseLastButton, 0, System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y, 0);
            if (_mouseLastX != mev.X || _mouseLastY != mev.Y) {
                OnMouseMove(mev);
            }
            if (_mouseIsPressing) {
                if (b == System.Windows.Forms.MouseButtons.None) {
                    OnMouseUp(mevold);
                    _mouseIsPressing = false;
                }
            } else {
                if (b != System.Windows.Forms.MouseButtons.None) {
                    OnMouseDown(mev);
                    _mouseIsPressing = true;
                }
            }
            _mouseLastX = mev.X;
            _mouseLastY = mev.Y;
            _mouseLastButton = b;
        }

        public void InstallHook() {
            Tim.Enabled = true;
            _mouseIsPressing = false;
            _mouseLastX = -1;
            _mouseLastY = -1;
            _mouseLastButton = 0;
            _keyIsPressing = false;
            _keyLastKey = 0;
        }

        public void RemoveHook() => Tim.Enabled = false;

        [DllImport("user32.dll", EntryPoint = "GetAsyncKeyState", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern short GetAsyncKeyState(System.Windows.Forms.Keys nVirtKey);

        private void Initialize() {
            Tim = new System.Windows.Forms.Timer {
                Interval = 1,
                Enabled = false
            };
            _mouseIsPressing = false;
            _mouseLastX = 0;
            _mouseLastY = 0;
            _keyIsPressing = false;
            _keyLastKey = 0;
        }

        private void OnKeyDown(System.Windows.Forms.KeyEventArgs e) => KeyDown?.Invoke(this, e);

        private void OnKeyUp(System.Windows.Forms.KeyEventArgs e) => KeyUp?.Invoke(null, e);

        private void OnMouseDown(System.Windows.Forms.MouseEventArgs e) => MouseDown?.Invoke(null, e);

        private void OnMouseMove(System.Windows.Forms.MouseEventArgs e) => MouseMove?.Invoke(this, e);

        private void OnMouseUp(System.Windows.Forms.MouseEventArgs e) => MouseUp?.Invoke(this, e);

        private void Tim_Tick(object sender, System.EventArgs e) {
            Tim.Enabled = false;
            DoMouse();
            DoKeyboard();
            Tim.Enabled = true;
        }

        #endregion
    }
}