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

#endregion BlueElements - a collection of useful tools, database and controls

using BlueBasics;
using System;
using System.Drawing;

namespace BlueControls.Forms {

    public partial class QuickInfo : FloatingForm {
        public static string _shownTXT = string.Empty;
        public static string _AutoClosedTXT = string.Empty;
        private bool _Shown = false;
        private int Counter = 0;

        private QuickInfo() : base(Enums.enDesign.Form_QuickInfo) => InitializeComponent();

        private QuickInfo(string Text) : this() {
            //InitializeComponent();
            capTXT.Text = Text;
            var He = Math.Min(capTXT.TextRequiredSize().Height, (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Height * 0.7));
            var Wi = Math.Min(capTXT.TextRequiredSize().Width, (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Width * 0.7));
            Size = new Size(Wi + (capTXT.Left * 2), He + (capTXT.Top * 2));
            Visible = false;
            timQI.Enabled = true;
        }

        public static void Show(string Text) {
            if (Text == _shownTXT) { return; }
            Close(false);
            if (Text == _AutoClosedTXT) { return; }
            _shownTXT = Text;
            if (string.IsNullOrEmpty(Text)) { return; }
            new QuickInfo(Text);
        }

        public static new void Close() => Close(false);

        private static void Close(bool AutoClose) {
            if (AutoClose) {
                _AutoClosedTXT = _shownTXT;
            } else {
                _shownTXT = string.Empty;
                _AutoClosedTXT = string.Empty;
            }
            foreach (var ThisForm in AllBoxes) {
                if (!ThisForm.IsDisposed && ThisForm is QuickInfo QI) {
                    try {
                        QI.timQI.Enabled = false;
                        ThisForm.Close();
                        Close(AutoClose);
                        return;
                    } catch (Exception ex) {
                        Develop.DebugPrint(ex);
                    }
                }
            }
        }

        private void timQI_Tick(object sender, System.EventArgs e) {
            Position_LocateToMouse();
            if (!_Shown) {
                _Shown = true;
                Show();
                timQI.Interval = 15;
            }
            Counter++;
            if (Counter * timQI.Interval > 10000) {
                timQI.Enabled = false;
                Close(true);
            }
        }
    }
}