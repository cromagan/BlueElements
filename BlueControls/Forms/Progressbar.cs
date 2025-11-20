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

    private Progressbar(string text) : this() {
        // InitializeComponent();
        capTXT.Text = text;
        var he = Math.Min(capTXT.RequiredTextSize().Height, (int)(Screen.PrimaryScreen.Bounds.Size.Height * 0.7));
        var wi = Math.Min(capTXT.RequiredTextSize().Width, (int)(Screen.PrimaryScreen.Bounds.Size.Width * 0.7));
        Size = new Size(wi + (capTXT.Left * 2), he + (capTXT.Top * 2));
    }

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
            // Es kommt zwar die ganze Berechnung durcheinander, aber besser als ein Fehler
            Invoke(new Action(() => Update(current)));
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
            if (current < 2) {
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
        return baseText + "</b></i></u>" +
               (count < 1 ? string.Empty
                   : current <= 3 ? "<br>Restzeit wird ermittelt<tab>"
                   : _eProgressbarLastCalulatedSeconds < -10 ? "<br>Restzeit wird ermittelt<tab>"
                   : _eProgressbarLastCalulatedSeconds > 94 ? "<br>" + prt + " % - Geschätzte Restzeit:   " + (_eProgressbarLastCalulatedSeconds / 60) + " Minuten<tab>"
                   : _eProgressbarLastCalulatedSeconds > 10 ? "<br>" + prt + " % - Geschätzte Restzeit: " + (_eProgressbarLastCalulatedSeconds / 5 * 5) + " Sekunden<tab>"
                   : _eProgressbarLastCalulatedSeconds > 0 ? "<br>" + prt + " % - Geschätzte Restzeit: <<> 10 Sekunden<tab>"
                   : "<br>100 % - ...abgeschlossen!<tab>");
    }

    private void UpdateInternal(string text) {
        if (text != capTXT.Text) {
            capTXT.Text = text;
            var wi = Math.Max(Size.Width, capTXT.Width + (Skin.Padding * 2));
            var he = Math.Max(Size.Height, capTXT.Height + (Skin.Padding * 2));
            Size = new Size(wi, he);
            Refresh();
        }
    }

    #endregion
}