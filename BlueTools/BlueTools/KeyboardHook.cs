// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BlueTools {

    internal class GlobalKeyboardHook {

        #region Fields

        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x100;

        private const int WM_KEYUP = 0x101;

        private const int WM_SYSKEYDOWN = 0x104;

        private const int WM_SYSKEYUP = 0x105;

        private IntPtr _HookHandle = IntPtr.Zero;

        #endregion

        #region Constructors

        /// <summary>
        /// erstellt die Keyboard-hook-Klasse. Der Hook wird nicht gesetzt und muss manuell mit Hook gemacht werden
        /// </summary>
        public GlobalKeyboardHook() {
        }

        #endregion

        #region Destructors

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="globalKeyboardHook"/> is reclaimed by garbage collection and uninstalls the keyboard hook.
        /// </summary>
        ~GlobalKeyboardHook() {
            Unhook();
        }

        #endregion

        #region Delegates

        /// <summary>
        /// defines the callback type for the hook
        /// </summary>
        public delegate int keyboardHookProc(int code, int wParam, ref KeyboardHookStruct lParam);

        #endregion

        #region Events

        public event KeyEventHandler KeyDown;

        public event KeyEventHandler KeyUp;

        #endregion

        #region Methods

        public void Hook() {
            var hInstance = LoadLibrary("User32");
            _HookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, hInstance, 0);
        }

        public void Unhook() => UnhookWindowsHookEx(_HookHandle);

        protected void OnKeyDown(KeyEventArgs e) => KeyDown?.Invoke(this, e);

        protected void OnKeyUp(KeyEventArgs e) => KeyUp?.Invoke(this, e);

        /// <summary>
        /// Calls the next hook.
        /// </summary>
        /// <param name="idHook">The hook id</param>
        /// <param name="nCode">The hook code</param>
        /// <param name="wParam">The wparam.</param>
        /// <param name="lParam">The lparam.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref KeyboardHookStruct lParam);

        /// <summary>
        /// Loads the library.
        /// </summary>
        /// <param name="lpFileName">Name of the library</param>
        /// <returns>A handle to the library</returns>
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        /// <summary>
        /// Sets the windows hook, do the desired event, one of hInstance or threadId must be non-null
        /// </summary>
        /// <param name="idHook">The id of the event you want to hook</param>
        /// <param name="callback">The callback.</param>
        /// <param name="hInstance">The handle you want to attach the event to, can be null</param>
        /// <param name="threadId">The thread you want to attach the event to, can be null</param>
        /// <returns>a handle to the desired hook</returns>
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);

        /// <summary>
        /// Unhooks the windows hook.
        /// </summary>
        /// <param name="hInstance">The hook handle that was returned from SetWindowsHookEx</param>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        /// <summary>
        /// The callback for the keyboard hook
        /// </summary>
        /// <param name="code">The hook code, if it isn't >= 0, the function shouldn't do anyting</param>
        /// <param name="wParam">The event type</param>
        /// <param name="lParam">The keyhook event information</param>
        /// <returns></returns>
        private int hookProc(int code, int wParam, ref KeyboardHookStruct lParam) {
            if (code >= 0) {
                var key = (Keys)lParam.VirtualKeyCode;
                var kea = new KeyEventArgs(key);
                if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyDown != null)) {
                    OnKeyDown(kea);
                } else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP) && (KeyUp != null)) {
                    OnKeyUp(kea);
                }
                if (kea.Handled) { return 1; }
            }
            return CallNextHookEx(_HookHandle, code, wParam, ref lParam);
        }

        #endregion

        #region Structs

        /// <summary>
        /// The KeyboardHookStruct structure contains information about a low-level keyboard input event.
        /// https://github.com/reliak/moonpdf/tree/master/ext/MouseKeyboardActivityMonitor/WinApi
        /// </summary>
        /// <remarks>
        /// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookstructures/cwpstruct.asp
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        internal struct KeyboardHookStruct {

            /// <summary>
            /// Specifies a virtual-key code. The code must be a value in the range 1 to 254.
            /// </summary>
            public int VirtualKeyCode;

            /// <summary>
            /// Specifies a hardware scan code for the key.
            /// </summary>
            public int ScanCode;

            /// <summary>
            /// Specifies the extended-key flag, event-injected flag, context code, and transition-state flag.
            /// </summary>
            public int Flags;

            /// <summary>
            /// Specifies the Time stamp for this message.
            /// </summary>
            public int Time;

            /// <summary>
            /// Specifies extra information associated with the message.
            /// </summary>
            public int ExtraInfo;
        }

        #endregion
    }
}