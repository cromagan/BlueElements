// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueControls.Enums;

namespace BlueControls.Forms;

public partial class QuickInfo : FloatingForm {

    #region Fields

    private static string _autoClosedTxt = string.Empty;
    private static string _shownTxt = string.Empty;
    private int _counter;
    private bool _shown;

    #endregion

    #region Constructors

    private QuickInfo() : base(Design.Form_QuickInfo) => InitializeComponent();

    private QuickInfo(string text) : this() {
        //InitializeComponent();
        capTXT.Text = text;
        var he = Math.Min(capTXT.TextRequiredSize().Height, (int)(Screen.PrimaryScreen.Bounds.Size.Height * 0.7));
        var wi = Math.Min(capTXT.TextRequiredSize().Width, (int)(Screen.PrimaryScreen.Bounds.Size.Width * 0.7));
        Size = new Size(wi + (capTXT.Left * 2), he + (capTXT.Top * 2));
        Visible = false;
        timQI.Enabled = true;
    }

    #endregion

    #region Methods

    public new static void Close() => Close(false);

    public static void Show(string text) {
        if (text == _shownTxt) { return; }
        Close(false);
        if (text == _autoClosedTxt) { return; }
        _shownTxt = text;
        if (string.IsNullOrEmpty(text)) { return; }
        _ = new QuickInfo(text);
    }

    private static void Close(bool autoClose) {
        if (autoClose) {
            _autoClosedTxt = _shownTxt;
        } else {
            _shownTxt = string.Empty;
            _autoClosedTxt = string.Empty;
        }
        foreach (var thisForm in AllBoxes) {
            if (thisForm.IsDisposed || thisForm is not QuickInfo qi) {
                continue;
            }

            try {
                qi.timQI.Enabled = false;
                thisForm.Close();
                Close(autoClose);
                return;
            } catch (Exception ex) {
                Develop.DebugPrint("Fehler beim Schließen des QuickInfos", ex);
            }
        }
    }

    private void timQI_Tick(object sender, System.EventArgs e) {
        Position_LocateToMouse();
        if (!_shown) {
            _shown = true;
            Show();
            timQI.Interval = 15;
        }
        _counter++;
        if (_counter * timQI.Interval > 10000) {
            timQI.Enabled = false;
            Close(true);
        }
    }

    #endregion
}