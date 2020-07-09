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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BlueControls
{
    public sealed class SystemInputHook
    {
        [DllImport("user32.dll", EntryPoint = "GetAsyncKeyState", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern short GetAsyncKeyState(System.Windows.Forms.Keys nVirtKey);

        #region  Variablen-Deklarationen 


        [AccessedThroughProperty(nameof(Tim))]
        private System.Windows.Forms.Timer _Tim;
        private System.Windows.Forms.Timer Tim
        {
            [DebuggerNonUserCode]
            get
            {
                return _Tim;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            [DebuggerNonUserCode]
            set
            {
                if (_Tim != null)
                {
                    _Tim.Tick -= Tim_Tick;
                }

                _Tim = value;

                if (value != null)
                {
                    _Tim.Tick += Tim_Tick;
                }
            }
        }

        private bool Mouse_IsPressing;
        private int Mouse_LastX;
        private int Mouse_LastY;
        private System.Windows.Forms.MouseButtons Mouse_LastButton;

        private bool Key_IsPressing;
        private System.Windows.Forms.Keys Key_LastKey;

        #endregion


        #region  Event-Deklarationen + Delegaten 
        public event System.EventHandler<System.Windows.Forms.MouseEventArgs> MouseDown;
        public event System.EventHandler<System.Windows.Forms.MouseEventArgs> MouseUp;
        public event System.EventHandler<System.Windows.Forms.MouseEventArgs> MouseMove;
        public event System.EventHandler<System.Windows.Forms.KeyEventArgs> KeyDown;
        public event System.EventHandler<System.Windows.Forms.KeyEventArgs> KeyUp;
        #endregion


        #region  Construktor + Initialize 

        private void Initialize()
        {
            Tim = new System.Windows.Forms.Timer
            {
                Interval = 1,
                Enabled = false
            };
            Mouse_IsPressing = false;
            Mouse_LastX = 0;
            Mouse_LastY = 0;
            Key_IsPressing = false;
            Key_LastKey = 0;
        }


        public SystemInputHook()
        {
            Initialize();
        }

        #endregion


        #region  Properties 

        #endregion


        public void InstallHook()
        {

            Tim.Enabled = true;

            Mouse_IsPressing = false;
            Mouse_LastX = -1;
            Mouse_LastY = -1;
            Mouse_LastButton = 0;

            Key_IsPressing = false;
            Key_LastKey = 0;
        }

        public void RemoveHook()
        {
            Tim.Enabled = false;
        }


        public void CheckNow()
        {
            Tim_Tick(null, null);
        }


        private void Tim_Tick(object sender, System.EventArgs e)
        {

            Tim.Enabled = false;


            DoMouse();
            DoKeyboard();


            Tim.Enabled = true;
        }


        public void DoMouse()
        {
            var B = System.Windows.Forms.MouseButtons.None;

            if (GetAsyncKeyState(System.Windows.Forms.Keys.LButton) != 0)
            {
                B |= System.Windows.Forms.MouseButtons.Left;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.RButton) != 0)
            {
                B |= System.Windows.Forms.MouseButtons.Right;
            }


            var mev = new System.Windows.Forms.MouseEventArgs(B, 0, System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y, 0);
            var mevold = new System.Windows.Forms.MouseEventArgs(Mouse_LastButton, 0, System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y, 0);


            if (Mouse_LastX != mev.X || Mouse_LastY != mev.Y)
            {
                OnMouseMove(mev);

            }


            if (Mouse_IsPressing)
            {
                if (B == System.Windows.Forms.MouseButtons.None)
                {
                    OnMouseUp(mevold);
                    Mouse_IsPressing = false;
                }
            }
            else
            {
                if (B != System.Windows.Forms.MouseButtons.None)
                {
                    OnMouseDown(mev);
                    Mouse_IsPressing = true;
                }
            }

            Mouse_LastX = mev.X;
            Mouse_LastY = mev.Y;
            Mouse_LastButton = B;
        }

        private void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            MouseDown?.Invoke(null, e);
        }

        private void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            MouseUp?.Invoke(this, e);
        }

        private void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }

        public void DoKeyboard()
        {


            var k = System.Windows.Forms.Keys.None;


            if (GetAsyncKeyState(System.Windows.Forms.Keys.D1) != 0)
            {
                k |= System.Windows.Forms.Keys.D1;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D2) != 0)
            {
                k |= System.Windows.Forms.Keys.D2;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D3) != 0)
            {
                k |= System.Windows.Forms.Keys.D3;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D4) != 0)
            {
                k |= System.Windows.Forms.Keys.D4;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D5) != 0)
            {
                k |= System.Windows.Forms.Keys.D5;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D6) != 0)
            {
                k |= System.Windows.Forms.Keys.D6;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D7) != 0)
            {
                k |= System.Windows.Forms.Keys.D7;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D8) != 0)
            {
                k |= System.Windows.Forms.Keys.D8;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D9) != 0)
            {
                k |= System.Windows.Forms.Keys.D9;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.D0) != 0)
            {
                k |= System.Windows.Forms.Keys.D0;
            }


            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad1) != 0)
            {
                k |= System.Windows.Forms.Keys.NumPad1;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad2) != 0)
            {
                k |= System.Windows.Forms.Keys.NumPad2;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad3) != 0)
            {
                k |= System.Windows.Forms.Keys.NumPad3;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad4) != 0)
            {
                k |= System.Windows.Forms.Keys.NumPad4;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad5) != 0)
            {
                k |= System.Windows.Forms.Keys.NumPad5;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad6) != 0)
            {
                k |= System.Windows.Forms.Keys.NumPad6;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad7) != 0)
            {
                k |= System.Windows.Forms.Keys.NumPad7;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad8) != 0)
            {
                k |= System.Windows.Forms.Keys.NumPad8;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad9) != 0)
            {
                k |= System.Windows.Forms.Keys.NumPad9;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.NumPad0) != 0)
            {
                k |= System.Windows.Forms.Keys.NumPad0;
            }


            if (GetAsyncKeyState(System.Windows.Forms.Keys.X) != 0)
            {
                k |= System.Windows.Forms.Keys.X;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.V) != 0)
            {
                k |= System.Windows.Forms.Keys.V;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.C) != 0)
            {
                k |= System.Windows.Forms.Keys.C;
            }

            if (GetAsyncKeyState(System.Windows.Forms.Keys.Tab) != 0)
            {
                k |= System.Windows.Forms.Keys.Tab;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Enter) != 0)
            {
                k |= System.Windows.Forms.Keys.Enter;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Return) != 0)
            {
                k |= System.Windows.Forms.Keys.Return;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.ControlKey) != 0)
            {
                k |= System.Windows.Forms.Keys.Control;
            }
            //    If GetAsyncKeyState(System.Windows.Forms.Keys.ControlKey) Then k = k Or System.Windows.Forms.Keys.ControlKey
            //   If GetAsyncKeyState(System.Windows.Forms.Keys.Menu) Then k = k Or System.Windows.Forms.Keys.Menu
            if (GetAsyncKeyState(System.Windows.Forms.Keys.ShiftKey) != 0)
            {
                k |= System.Windows.Forms.Keys.Shift;
            }

            if (GetAsyncKeyState(System.Windows.Forms.Keys.Right) != 0)
            {
                k |= System.Windows.Forms.Keys.Right;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Left) != 0)
            {
                k |= System.Windows.Forms.Keys.Left;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Up) != 0)
            {
                k |= System.Windows.Forms.Keys.Up;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Down) != 0)
            {
                k |= System.Windows.Forms.Keys.Down;
            }

            if (GetAsyncKeyState(System.Windows.Forms.Keys.Back) != 0)
            {
                k |= System.Windows.Forms.Keys.Back;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Delete) != 0)
            {
                k |= System.Windows.Forms.Keys.Delete;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Space) != 0)
            {
                k |= System.Windows.Forms.Keys.Space;
            }


            if (GetAsyncKeyState(System.Windows.Forms.Keys.Add) != 0)
            {
                k |= System.Windows.Forms.Keys.Add;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Subtract) != 0)
            {
                k |= System.Windows.Forms.Keys.Subtract;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Multiply) != 0)
            {
                k |= System.Windows.Forms.Keys.Multiply;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Divide) != 0)
            {
                k |= System.Windows.Forms.Keys.Divide;
            }

            if (GetAsyncKeyState(System.Windows.Forms.Keys.OemMinus) != 0)
            {
                k |= System.Windows.Forms.Keys.OemMinus;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.OemPeriod) != 0)
            {
                k |= System.Windows.Forms.Keys.OemPeriod;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Oemcomma) != 0)
            {
                k |= System.Windows.Forms.Keys.Oemcomma;
            }


            if (GetAsyncKeyState(System.Windows.Forms.Keys.F1) != 0)
            {
                k |= System.Windows.Forms.Keys.F1;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F2) != 0)
            {
                k |= System.Windows.Forms.Keys.F2;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F3) != 0)
            {
                k |= System.Windows.Forms.Keys.F3;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F4) != 0)
            {
                k |= System.Windows.Forms.Keys.F4;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F5) != 0)
            {
                k |= System.Windows.Forms.Keys.F5;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F6) != 0)
            {
                k |= System.Windows.Forms.Keys.F6;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F7) != 0)
            {
                k |= System.Windows.Forms.Keys.F7;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F8) != 0)
            {
                k |= System.Windows.Forms.Keys.F8;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F9) != 0)
            {
                k |= System.Windows.Forms.Keys.F9;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F10) != 0)
            {
                k |= System.Windows.Forms.Keys.F10;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F11) != 0)
            {
                k |= System.Windows.Forms.Keys.F11;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.F12) != 0)
            {
                k |= System.Windows.Forms.Keys.F12;
            }


            if (GetAsyncKeyState(System.Windows.Forms.Keys.Pause) != 0)
            {
                k |= System.Windows.Forms.Keys.Pause;
            }
            if (GetAsyncKeyState(System.Windows.Forms.Keys.Oem5) != 0) // Zirkumflex ^
            {
                k |= System.Windows.Forms.Keys.Oem5;
            }


            var kev = new System.Windows.Forms.KeyEventArgs(k);
            var kevold = new System.Windows.Forms.KeyEventArgs(Key_LastKey);

            if (Key_IsPressing)
            {
                if (k == System.Windows.Forms.Keys.None)
                {
                    OnKeyUp(kevold);
                    Key_IsPressing = false;
                }
                else if (k != Key_LastKey)
                {
                    OnKeyDown(kev);
                }
            }
            else
            {
                if (k != System.Windows.Forms.Keys.None)
                {
                    OnKeyDown(kev);
                    Key_IsPressing = true;
                }
            }

            Key_LastKey = k;
        }

        private void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            KeyDown?.Invoke(this, e);
        }

        private void OnKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            KeyUp?.Invoke(null, e);
        }

    }

}