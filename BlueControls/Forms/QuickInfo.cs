// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics.ClassesStatic;
using BlueControls.Classes;
using BlueControls.Enums;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Forms;

public partial class QuickInfo : FloatingForm {

    #region Fields

    private static string _autoClosedTxt = string.Empty;

    private static string _shownTxt = string.Empty;

    private int _counter;

    private bool _shown;

    private System.Threading.Timer? _timQI;

    private int _timQIInterval = 500;

    #endregion

    #region Constructors

    private QuickInfo() : base(Design.Form_QuickInfo) => InitializeComponent();

    private QuickInfo(string text) : this() {
        //InitializeComponent();
        capText.Text = text;
        capText.FitSize();
        capText.Location = new Point(Skin.PaddingMedium, Skin.PaddingMedium);
        var wi = Math.Min((int)(Screen.PrimaryScreen.Bounds.Size.Height * 0.7), capText.Right + Skin.PaddingMedium);
        var he = Math.Min((int)(Screen.PrimaryScreen.Bounds.Size.Height * 0.7), capText.Bottom + Skin.PaddingMedium);
        Size = new Size(wi, he);
        Visible = false;
        _timQI = new System.Threading.Timer(_ => {
            if (IsHandleCreated) { BeginInvoke(new Action(TimQI_Tick)); }
        }, null, 500, 500);
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

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) { components?.Dispose(); }
        base.Dispose(disposing);
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
                qi._timQI?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                thisForm.Close();
                Close(autoClose);
                return;
            } catch (Exception ex) {
                Develop.DebugPrint("Fehler beim Schließen des QuickInfos", ex);
            }
        }
    }

    private void TimQI_Tick() {
        Position_LocateToMouse();
        if (!_shown) {
            _shown = true;
            Show();
            _timQIInterval = 15;
            _timQI?.Change(15, 15);
        }
        _counter++;
        if (_counter * _timQIInterval > 10000) {
            _timQI?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            Close(true);
        }
    }

    #endregion
}