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

using BlueControls.Classes;
using BlueControls.Enums;
using System.Windows.Forms;

namespace BlueControls.Controls;

public sealed class RibbonBar : AbstractTabControl {

    #region Constructors

    public RibbonBar() : base() {
        Height = 110;
        SendToBack();
        Dock = DockStyle.Top;
        var state = Enabled ? States.Standard : States.Standard_Disabled;
        BackColor = Skin.Color_Back(Design.RibbonBar_Body, state);
    }

    #endregion

    #region Methods

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);
        if (e.Control is not TabPage tp) {
            return;
        }

        var state = Enabled ? States.Standard : States.Standard_Disabled;
        tp.BackColor = Skin.Color_Back(Design.RibbonBar_Body, state);
        Invalidate();
    }

    // Korrektur: Hintergrundfarben bei Statusänderung anpassen
    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);
        var state = Enabled ? States.Standard : States.Standard_Disabled;

        BackColor = Skin.Color_Back(Design.RibbonBar_Body, state);

        foreach (Control control in Controls) {
            if (control is TabPage tp) {
                tp.BackColor = Skin.Color_Back(Design.RibbonBar_Body, state);
            }
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e) => DrawControl(e, Design.RibbonBar_Back);

    #endregion
}