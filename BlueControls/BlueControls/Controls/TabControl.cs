// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using System.Windows.Forms;

namespace BlueControls.Controls;

public class TabControl : AbstractTabControl {

    #region Constructors

    public TabControl() : base() => BackColor = Skin.Color_Back(Design.TabStrip_Body, States.Standard);

    #endregion

    #region Methods

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);
        if (e.Control is not TabPage tp) {
            return;
        }

        tp.BackColor = Skin.Color_Back(Design.TabStrip_Body, States.Standard);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e) => DrawControl(e, Design.TabStrip_Back);

    #endregion
}