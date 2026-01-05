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
using BlueControls.Interfaces;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls;

public class TabControl : AbstractTabControl, ISendsFocusedChild {

    #region Constructors

    public TabControl() : base() => BackColor = Skin.Color_Back(Design.TabStrip_Body, States.Standard);

    #endregion

    #region Events

    public event EventHandler<ControlEventArgs>? ChildGotFocus;

    #endregion

    #region Properties

    public sealed override Color BackColor {
        get => base.BackColor;
        set => base.BackColor = value;
    }

    #endregion

    #region Methods

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);
        if (e.Control is TabPage tab) {
            tab.BackColor = Skin.Color_Back(Design.TabStrip_Body, States.Standard);
            Invalidate();
            tab.ControlAdded += AddedControlInTabPage;
            tab.ControlRemoved += RemovedControlInTabPage;
        }
    }

    protected override void OnControlRemoved(ControlEventArgs e) {
        base.OnControlRemoved(e);
        if (e.Control is TabPage tab) {
            tab.ControlAdded -= AddedControlInTabPage;
            tab.ControlRemoved -= RemovedControlInTabPage;
        }
    }

    protected override void OnPaint(PaintEventArgs e) => DrawControl(e, Design.TabStrip_Back);

    private void AddedControlInTabPage(object sender, ControlEventArgs e) {
        if (e.Control is TabPage) { return; }

        if (e.Control is ISendsFocusedChild sfc) {
            sfc.ChildGotFocus += Sfc_ChildGotFocus;
        }
        e.Control.GotFocus += ControlInTabPage_GotFocus;
    }

    private void ControlInTabPage_GotFocus(object sender, System.EventArgs e) {
        if (sender is not Control c) { return; }
        OnChildGotFocus(new ControlEventArgs(c));
    }

    private void OnChildGotFocus(ControlEventArgs e) => ChildGotFocus?.Invoke(this, e);

    private void RemovedControlInTabPage(object sender, ControlEventArgs e) {
        if (e.Control is TabPage) { return; }

        if (e.Control is ISendsFocusedChild sfc) {
            sfc.ChildGotFocus -= Sfc_ChildGotFocus;
        }
        e.Control.GotFocus -= ControlInTabPage_GotFocus;
    }

    private void Sfc_ChildGotFocus(object sender, ControlEventArgs e) => OnChildGotFocus(e);

    #endregion
}