#region BlueElements - a collection of useful tools, database and controls
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
#endregion

using BlueBasics.Enums;
using System.Runtime.InteropServices;

namespace BlueBasics

// TODO: Enums erzeugen
{
    internal static class modTastaturSimulation {
        private const int KEYEVENTF_KEYUP = 0x2; // Release key
        private const int KEYEVENTF_EXTENDEDKEY = 0x1;
        private const int KEYEVENTF_KEYDOWN = 0x0;
        // VK_CANCEL = &H3 'Used for control-break processing.
        // '****************
        //    VK_CRSEL = &HF7
        //    VK_EREOF = &HF9
        //    VK_EXECUTE = &H2B
        //    VK_EXSEL = &HF8
        //    VK_NONAME = &HFC
        //    VK_OEM_CLEAR = &HFE
        //    VK_PA1 = &HFD
        //    VK_PROCESSKEY = &HE5
        //    CAPSLOCK_ON = &H80    '  the capslock light is on.
        [DllImport("user32.dll", EntryPoint = "keybd_event", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        public static void KeyDown(enTaste k) {
            keybd_event((byte)k, 0, KEYEVENTF_KEYDOWN, 0);
        }

        public static void KeyUp(enTaste k) {
            keybd_event((byte)k, 0, KEYEVENTF_KEYUP, 0);
        }
        #region shift ,altgr and alt release sub
        public static void shiftrelease() {
            keybd_event((byte)enTaste.VK_SHIFT, 0, 2, 0);
        }

        public static void altrelease() {
            keybd_event((byte)enTaste.VK_MENU, 0, KEYEVENTF_EXTENDEDKEY | 2, 0);
        }

        public static void leftaltrelease() {
            keybd_event((byte)enTaste.VK_MENU, 0, KEYEVENTF_KEYUP, 0);
        }
        #endregion

    }
}