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

namespace BlueControls.Interfaces;

public interface IMouseAndKeyHandle {

    #region Methods

    bool KeyUp(object sender, System.Windows.Forms.KeyEventArgs e, float zoom, float shiftX, float shiftY);

    bool MouseDown(object sender, System.Windows.Forms.MouseEventArgs e, float zoom, float shiftX, float shiftY);

    bool MouseMove(object sender, System.Windows.Forms.MouseEventArgs e, float zoom, float shiftX, float shiftY);

    bool MouseUp(object sender, System.Windows.Forms.MouseEventArgs e, float zoom, float shiftX, float shiftY);

    #endregion

    //Function BlueCreativePad_MouseWheel(sender As Object, e As System.Windows.Forms.MouseEventArgs) As Boolean
    //Function MouseClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) As Boolean
    //Function MousefloatClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) As Boolean
    //Function MouseEnter(sender As Object, e As System.EventArgs) As Boolean
    //Function MouseHover(sender As Object, e As System.EventArgs) As Boolean
    //Function MouseLeave(sender As Object, e As System.EventArgs) As Boolean
}