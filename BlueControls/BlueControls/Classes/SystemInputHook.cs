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

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BlueControls;

public sealed class SystemInputHook {

    #region Fields

    private readonly Timer _tim;
    private bool _keyIsPressing;

    private Keys _keyLastKey;

    private bool _mouseIsPressing;

    private MouseButtons _mouseLastButton;

    private int _mouseLastX;

    private int _mouseLastY;

    #endregion

    #region Constructors

    public SystemInputHook() {
        _tim = new Timer {
            Interval = 1,
            Enabled = false
        };
        _tim.Tick += Tim_Tick;
        _mouseIsPressing = false;
        _mouseLastX = 0;
        _mouseLastY = 0;
        _keyIsPressing = false;
        _keyLastKey = 0;
    }

    #endregion

    #region Events

    public event EventHandler<KeyEventArgs>? KeyDown;

    public event EventHandler<KeyEventArgs>? KeyUp;

    public event EventHandler<MouseEventArgs>? MouseDown;

    public event EventHandler<MouseEventArgs>? MouseMove;

    public event EventHandler<MouseEventArgs>? MouseUp;

    #endregion

    #region Methods

    public void CheckNow() => Tim_Tick(null, null);

    public void DoKeyboard() {
        var k = Keys.None;
        if (GetAsyncKeyState(Keys.D1) != 0) {
            k |= Keys.D1;
        }
        if (GetAsyncKeyState(Keys.D2) != 0) {
            k |= Keys.D2;
        }
        if (GetAsyncKeyState(Keys.D3) != 0) {
            k |= Keys.D3;
        }
        if (GetAsyncKeyState(Keys.D4) != 0) {
            k |= Keys.D4;
        }
        if (GetAsyncKeyState(Keys.D5) != 0) {
            k |= Keys.D5;
        }
        if (GetAsyncKeyState(Keys.D6) != 0) {
            k |= Keys.D6;
        }
        if (GetAsyncKeyState(Keys.D7) != 0) {
            k |= Keys.D7;
        }
        if (GetAsyncKeyState(Keys.D8) != 0) {
            k |= Keys.D8;
        }
        if (GetAsyncKeyState(Keys.D9) != 0) {
            k |= Keys.D9;
        }
        if (GetAsyncKeyState(Keys.D0) != 0) {
            k |= Keys.D0;
        }
        if (GetAsyncKeyState(Keys.NumPad1) != 0) {
            k |= Keys.NumPad1;
        }
        if (GetAsyncKeyState(Keys.NumPad2) != 0) {
            k |= Keys.NumPad2;
        }
        if (GetAsyncKeyState(Keys.NumPad3) != 0) {
            k |= Keys.NumPad3;
        }
        if (GetAsyncKeyState(Keys.NumPad4) != 0) {
            k |= Keys.NumPad4;
        }
        if (GetAsyncKeyState(Keys.NumPad5) != 0) {
            k |= Keys.NumPad5;
        }
        if (GetAsyncKeyState(Keys.NumPad6) != 0) {
            k |= Keys.NumPad6;
        }
        if (GetAsyncKeyState(Keys.NumPad7) != 0) {
            k |= Keys.NumPad7;
        }
        if (GetAsyncKeyState(Keys.NumPad8) != 0) {
            k |= Keys.NumPad8;
        }
        if (GetAsyncKeyState(Keys.NumPad9) != 0) {
            k |= Keys.NumPad9;
        }
        if (GetAsyncKeyState(Keys.NumPad0) != 0) {
            k |= Keys.NumPad0;
        }
        if (GetAsyncKeyState(Keys.X) != 0) {
            k |= Keys.X;
        }
        if (GetAsyncKeyState(Keys.V) != 0) {
            k |= Keys.V;
        }
        if (GetAsyncKeyState(Keys.C) != 0) {
            k |= Keys.C;
        }
        if (GetAsyncKeyState(Keys.Tab) != 0) {
            k |= Keys.Tab;
        }
        if (GetAsyncKeyState(Keys.Enter) != 0) {
            k |= Keys.Enter;
        }
        if (GetAsyncKeyState(Keys.Return) != 0) {
            k |= Keys.Return;
        }
        if (GetAsyncKeyState(Keys.ControlKey) != 0) {
            k |= Keys.Control;
        }
        //    If GetAsyncKeyState(System.Windows.Forms.Keys.ControlKey) Then k = k Or System.Windows.Forms.Keys.ControlKey
        //   If GetAsyncKeyState(System.Windows.Forms.Keys.Menu) Then k = k Or System.Windows.Forms.Keys.Menu
        if (GetAsyncKeyState(Keys.ShiftKey) != 0) {
            k |= Keys.Shift;
        }
        if (GetAsyncKeyState(Keys.Right) != 0) {
            k |= Keys.Right;
        }
        if (GetAsyncKeyState(Keys.Left) != 0) {
            k |= Keys.Left;
        }
        if (GetAsyncKeyState(Keys.Up) != 0) {
            k |= Keys.Up;
        }
        if (GetAsyncKeyState(Keys.Down) != 0) {
            k |= Keys.Down;
        }
        if (GetAsyncKeyState(Keys.Back) != 0) {
            k |= Keys.Back;
        }
        if (GetAsyncKeyState(Keys.Delete) != 0) {
            k |= Keys.Delete;
        }
        if (GetAsyncKeyState(Keys.Space) != 0) {
            k |= Keys.Space;
        }
        if (GetAsyncKeyState(Keys.Add) != 0) {
            k |= Keys.Add;
        }
        if (GetAsyncKeyState(Keys.Subtract) != 0) {
            k |= Keys.Subtract;
        }
        if (GetAsyncKeyState(Keys.Multiply) != 0) {
            k |= Keys.Multiply;
        }
        if (GetAsyncKeyState(Keys.Divide) != 0) {
            k |= Keys.Divide;
        }
        if (GetAsyncKeyState(Keys.OemMinus) != 0) {
            k |= Keys.OemMinus;
        }
        if (GetAsyncKeyState(Keys.OemPeriod) != 0) {
            k |= Keys.OemPeriod;
        }
        if (GetAsyncKeyState(Keys.Oemcomma) != 0) {
            k |= Keys.Oemcomma;
        }
        if (GetAsyncKeyState(Keys.F1) != 0) {
            k |= Keys.F1;
        }
        if (GetAsyncKeyState(Keys.F2) != 0) {
            k |= Keys.F2;
        }
        if (GetAsyncKeyState(Keys.F3) != 0) {
            k |= Keys.F3;
        }
        if (GetAsyncKeyState(Keys.F4) != 0) {
            k |= Keys.F4;
        }
        if (GetAsyncKeyState(Keys.F5) != 0) {
            k |= Keys.F5;
        }
        if (GetAsyncKeyState(Keys.F6) != 0) {
            k |= Keys.F6;
        }
        if (GetAsyncKeyState(Keys.F7) != 0) {
            k |= Keys.F7;
        }
        if (GetAsyncKeyState(Keys.F8) != 0) {
            k |= Keys.F8;
        }
        if (GetAsyncKeyState(Keys.F9) != 0) {
            k |= Keys.F9;
        }
        if (GetAsyncKeyState(Keys.F10) != 0) {
            k |= Keys.F10;
        }
        if (GetAsyncKeyState(Keys.F11) != 0) {
            k |= Keys.F11;
        }
        if (GetAsyncKeyState(Keys.F12) != 0) {
            k |= Keys.F12;
        }
        if (GetAsyncKeyState(Keys.Pause) != 0) {
            k |= Keys.Pause;
        }
        if (GetAsyncKeyState(Keys.Oem5) != 0) // Zirkumflex ^
        {
            k |= Keys.Oem5;
        }
        KeyEventArgs kev = new(k);
        KeyEventArgs kevold = new(_keyLastKey);
        if (_keyIsPressing) {
            if (k == Keys.None) {
                OnKeyUp(kevold);
                _keyIsPressing = false;
            } else if (k != _keyLastKey) {
                OnKeyDown(kev);
            }
        } else {
            if (k != Keys.None) {
                OnKeyDown(kev);
                _keyIsPressing = true;
            }
        }
        _keyLastKey = k;
    }

    public void DoMouse() {
        var b = MouseButtons.None;
        if (GetAsyncKeyState(Keys.LButton) != 0) {
            b |= MouseButtons.Left;
        }
        if (GetAsyncKeyState(Keys.RButton) != 0) {
            b |= MouseButtons.Right;
        }
        MouseEventArgs mev = new(b, 0, Cursor.Position.X,
            Cursor.Position.Y, 0);
        MouseEventArgs mevold = new(_mouseLastButton, 0,
            Cursor.Position.X, Cursor.Position.Y, 0);
        if (_mouseLastX != mev.X || _mouseLastY != mev.Y) {
            OnMouseMove(mev);
        }
        if (_mouseIsPressing) {
            if (b == MouseButtons.None) {
                OnMouseUp(mevold);
                _mouseIsPressing = false;
            }
        } else {
            if (b != MouseButtons.None) {
                OnMouseDown(mev);
                _mouseIsPressing = true;
            }
        }
        _mouseLastX = mev.X;
        _mouseLastY = mev.Y;
        _mouseLastButton = b;
    }

    public void InstallHook() {
        _tim.Enabled = true;
        _mouseIsPressing = false;
        _mouseLastX = -1;
        _mouseLastY = -1;
        _mouseLastButton = 0;
        _keyIsPressing = false;
        _keyLastKey = 0;
    }

    public void RemoveHook() => _tim.Enabled = false;

    [DllImport("user32.dll", EntryPoint = "GetAsyncKeyState", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern short GetAsyncKeyState(Keys nVirtKey);

    private void OnKeyDown(KeyEventArgs e) => KeyDown?.Invoke(this, e);

    private void OnKeyUp(KeyEventArgs e) => KeyUp?.Invoke(null, e);

    private void OnMouseDown(MouseEventArgs e) => MouseDown?.Invoke(null, e);

    private void OnMouseMove(MouseEventArgs e) => MouseMove?.Invoke(this, e);

    private void OnMouseUp(MouseEventArgs e) => MouseUp?.Invoke(this, e);

    private void Tim_Tick(object? sender, System.EventArgs? e) {
        _tim.Enabled = false;
        DoMouse();
        DoKeyboard();
        _tim.Enabled = true;
    }

    #endregion
}