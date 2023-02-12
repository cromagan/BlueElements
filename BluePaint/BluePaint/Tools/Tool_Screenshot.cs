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

using BlueBasics;
using BlueControls;

namespace BluePaint;

public partial class Tool_Screenshot {

    #region Constructors

    public Tool_Screenshot() : base() => InitializeComponent();

    #endregion

    #region Methods

    internal override void ToolFirstShown() {
        DoScreenShot();
        OnZoomFit();
    }

    private void DoScreenShot() {
        OnHideMainWindow();
        Generic.Pause(1, true);
        var pic = ScreenShot.GrabArea(null);
        if (pic != null) { OnOverridePic(pic); }
        OnShowMainWindow();
    }

    private void NeuerScreenshot_Click(object sender, System.EventArgs e) {
        DoScreenShot();
        OnZoomFit();
    }

    #endregion
}