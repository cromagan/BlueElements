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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BlueBasics.Generic;
using static BlueBasics.Develop;
using static BlueBasics.FileOperations;
using BlueBasics;

namespace BlueTools {

    public partial class Form1 : Form {

        #region Fields

        private GlobalKeyboardHook _Hook = new GlobalKeyboardHook();
        private int _MousePixelMoved = 10;
        private string _MousePosition = string.Empty;

        #endregion

        #region Constructors

        public Form1() {
            _MousePosition = MousePosition.ToString();
            InitializeComponent();

            #region auf doppelten Start checken

            if (IsRunning()) {
                tim.Enabled = false;
                lblInfo.Text = "Doppelter Start entdeckt.";
                Pause(60, true);

                if (IsRunning()) {
                    AbortExe();
                    return;
                }

                tim.Enabled = true;
            }

            #endregion

            tim.Enabled = true;

            _Hook.KeyDown += _Hook_KeyDown;
            _Hook.KeyUp += _Hook_KeyUp;
        }

        #endregion

        #region Methods

        private void _Hook_KeyDown(object sender, KeyEventArgs e) {
           if (e.KeyData.HasFlag(Keys.OemClear)) {
                WindowsRemoteControl.KeyDown(BlueBasics.Enums.enTaste.VK_A);
                e.Handled = true;
            }
        }

        private void _Hook_KeyUp(object sender, KeyEventArgs e) {
            if (e.KeyData.HasFlag(Keys.OemClear)) {
                WindowsRemoteControl.KeyUp(BlueBasics.Enums.enTaste.VK_A);
                e.Handled = true;
            }
        }

        private void chkFNMappen_CheckedChanged(object sender, EventArgs e) {
            if (chkFNMappen.Checked) {
                _Hook.Hook();
            } else {
                _Hook.Unhook();
            }
        }

        private void chkMouseMove_CheckedChanged(object sender, EventArgs e) {
        }

        private void Form1_Load(object sender, EventArgs e) {
        }

        private void tim_Tick(object sender, EventArgs e) {

            #region Maus bei Bedarf bewegen

            if (chkMouseMove.Checked) {
                var newpos = MousePosition.ToString();
                if (_MousePosition != newpos) {
                    _MousePosition = newpos;
                    return;
                }
                WindowsRemoteControl.MoveMouse(MousePosition.X + _MousePixelMoved, MousePosition.Y + _MousePixelMoved);
                _MousePixelMoved *= -1;
                _MousePosition = MousePosition.ToString();
                lblInfo.Text = DateTime.Now.ToString(Constants.Format_Date8) + " Maus bewegt";
            }

            #endregion
        }

        #endregion
    }
}