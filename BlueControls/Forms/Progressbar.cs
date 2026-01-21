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

using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Forms;

public partial class Progressbar : FloatingForm {

    #region Fields

    private readonly Dictionary<int, DateTime> _eProgressbarTimeDic = [];
    private string _baseText = string.Empty;
    private int _count;
    private int _eProgressbarLastCalulatedSeconds = int.MinValue;
    private int _eProgressbarLastCurrent = int.MaxValue;
    private DateTime _eProgressbarLastTimeUpdate = DateTime.UtcNow;

    #endregion

    #region Constructors

    private Progressbar() : base(Design.Form_BitteWarten) => InitializeComponent();

    private Progressbar(string text) : this() => UpdateInternal(text);

    #endregion

    #region Methods

    public static Progressbar Show(string text) {
        Progressbar p = new(text) {
            _baseText = text
        };
        p.Show();
        return p;
    }

    public static Progressbar Show(string text, int count) {
        Progressbar p = new(text) {
            _baseText = text,
            _count = count
        };
        p.Update(0);
        p.Show();
        p.BringToFront();
        return p;
    }

    public void Update(string text) {
        _baseText = text;
        UpdateInternal(text);
    }

    public void Update(int current) {
        if (InvokeRequired) {
            Invoke(new Action(() => UpdateInternal(CalculateText(_baseText, current, _count))));
            return;
        }
        UpdateInternal(CalculateText(_baseText, current, _count));
    }

    private string CalculateText(string baseText, int current, int count) {
        if (current < _eProgressbarLastCurrent) {
            _eProgressbarTimeDic.Clear();
            _eProgressbarLastTimeUpdate = DateTime.UtcNow;
            _eProgressbarLastCalulatedSeconds = int.MinValue;
        }

        if (count <= 0) { return baseText; }
        var pr = current / (double)count;

        if (pr > 1) { pr = 1; }
        if (pr < 0) { pr = 0; }
        if (double.IsNaN(pr)) { pr = 0; }
        int tmpCalculatedSeconds;
        if (current > 0) {
            if (_eProgressbarTimeDic.ContainsKey(Math.Max(0, current - 100))) {
                var d = _eProgressbarTimeDic[Math.Max(0, current - 100)];
                var ts = DateTime.UtcNow.Subtract(d).TotalSeconds;
                tmpCalculatedSeconds = (int)(ts / Math.Min(current, 100) * (count - current));
            } else {
                tmpCalculatedSeconds = int.MinValue;
            }
        } else {
            tmpCalculatedSeconds = 0;
        }
        _eProgressbarLastCurrent = current;
        if (!_eProgressbarTimeDic.ContainsKey(current)) {
            _eProgressbarTimeDic.Add(current, DateTime.UtcNow);
        }
        if (_eProgressbarLastCalulatedSeconds != tmpCalculatedSeconds && DateTime.UtcNow.Subtract(_eProgressbarLastTimeUpdate).TotalSeconds > 5) {
            _eProgressbarLastTimeUpdate = DateTime.UtcNow;
            if (current < 2 && tmpCalculatedSeconds > 0) {
                _eProgressbarLastCalulatedSeconds = tmpCalculatedSeconds;
            }
            if (tmpCalculatedSeconds < _eProgressbarLastCalulatedSeconds * 0.9) {
                _eProgressbarLastCalulatedSeconds = tmpCalculatedSeconds;
            }
            if (tmpCalculatedSeconds > _eProgressbarLastCalulatedSeconds * 1.5) {
                _eProgressbarLastCalulatedSeconds = tmpCalculatedSeconds;
            }
        }
        var prt = (int)(pr * 100);
        if (prt > 100) { prt = 100; }
        if (prt < 0) { prt = 0; }

        baseText = baseText + "</b></i></u><br>" + prt + " % - ";

        if (current <= 3 || count < 1 || _eProgressbarLastCalulatedSeconds < -10) {
            return baseText + "Restzeit wird ermittelt";
        }

        if (_eProgressbarLastCalulatedSeconds > 94) {
            return baseText + "Geschätzte Restzeit:   " + (_eProgressbarLastCalulatedSeconds / 60) + " Minuten";
        }

        if (_eProgressbarLastCalulatedSeconds > 10) {
            return baseText + "Geschätzte Restzeit: " + (_eProgressbarLastCalulatedSeconds / 5 * 5) + " Sekunden";
        }

        if (_eProgressbarLastCalulatedSeconds > 0) {
            return baseText + "Geschätzte Restzeit: <<> 10 Sekunden";
        }

        return baseText + "...abgeschlossen!";
    }

    private void UpdateInternal(string text) {
        if (text != capText.Text) {
            capText.Text = text;
            capText.FitSize();
            capText.Location = new Point(Skin.Padding, Skin.Padding);
            var wi = Math.Max(Size.Width, capText.Right + Skin.Padding);
            var he = Math.Max(Size.Height, capText.Bottom + Skin.Padding);
            Size = new Size(wi, he);
            Refresh();
        }
    }

    #endregion
}