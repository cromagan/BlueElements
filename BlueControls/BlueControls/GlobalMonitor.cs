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

using BlueBasics;
using BlueControls.Forms;
using System;
using static BlueBasics.Extensions;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

#nullable enable

namespace BlueControls;

public partial class GlobalMonitor : Form {

    #region Fields

    private int _n = 0;

    #endregion

    #region Constructors

    public GlobalMonitor() {
        InitializeComponent();

        Develop.MonitorMessage = Message;
    }

    #endregion

    #region Methods

    public void Message(string category, string symbol, string message, int indent) {
        if (Disposing || IsDisposed) { return; }

        if (InvokeRequired) {
            try {
                _ = Invoke(new Action(() => Message(category, symbol, message, indent)));
                return;
            } catch {
                return;
            }
        }

        _n--;
        if (_n < 0) { _n = 99999; }

        var e = $"[{DateTime.Now.ToString7()}] [Ebene {indent+1}] {category}: {new string(' ', indent * 6)} {message}";

        lstLog.ItemAdd(ItemOf(e, _n.ToStringInt7()));

        lstLog.Refresh();
    }

    protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
        Develop.MonitorMessage = null;
        base.OnFormClosing(e);
    }

    protected override void OnShown(System.EventArgs e) {
        base.OnShown(e);
        Develop.MonitorMessage?.Invoke("Global", "Information", "Monitoring gestartet", 0);
    }

    private void btnFilterDel_Click(object sender, System.EventArgs e) => txbFilter.Text = string.Empty;

    private void btnLeeren_Click(object sender, System.EventArgs e) {
        lstLog.ItemClear();
    }

    private void txbFilter_TextChanged(object sender, System.EventArgs e) {
        lstLog.FilterText = txbFilter.Text;
        btnFilterDel.Enabled = Enabled && !string.IsNullOrEmpty(txbFilter.Text);
    }

    #endregion
}