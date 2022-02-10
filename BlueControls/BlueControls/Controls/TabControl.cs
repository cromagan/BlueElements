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

namespace BlueControls.Controls {

    public class TabControl : AbstractTabControl {

        #region Constructors

        public TabControl() : base() => BackColor = Skin.Color_Back(enDesign.TabStrip_Body, enStates.Standard);

        #endregion

        #region Methods

        protected override void OnControlAdded(ControlEventArgs e) {
            base.OnControlAdded(e);
            if (e.Control is TabPage tp) {
                tp.BackColor = Skin.Color_Back(enDesign.TabStrip_Body, enStates.Standard);
                Invalidate();
            }
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) => DrawControl(e, enDesign.TabStrip_Back);

        #endregion
    }
}