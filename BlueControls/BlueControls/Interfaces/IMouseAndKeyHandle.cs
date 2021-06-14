#region BlueElements - a collection of useful tools, database and controls

// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

#endregion BlueElements - a collection of useful tools, database and controls

namespace BlueControls.Interfaces {

    public interface IMouseAndKeyHandle {

        bool MouseDown(object sender, System.Windows.Forms.MouseEventArgs e, decimal zoom, decimal shiftX, decimal shiftY);

        bool MouseMove(object sender, System.Windows.Forms.MouseEventArgs e, decimal zoom, decimal shiftX, decimal shiftY);

        bool MouseUp(object sender, System.Windows.Forms.MouseEventArgs e, decimal zoom, decimal shiftX, decimal shiftY);

        bool KeyUp(object sender, System.Windows.Forms.KeyEventArgs e, decimal zoom, decimal shiftX, decimal shiftY);

        //Function BlueCreativePad_MouseWheel(sender As Object, e As System.Windows.Forms.MouseEventArgs) As Boolean
        //Function MouseClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) As Boolean
        //Function MouseDoubleClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) As Boolean
        //Function MouseEnter(sender As Object, e As System.EventArgs) As Boolean
        //Function MouseHover(sender As Object, e As System.EventArgs) As Boolean
        //Function MouseLeave(sender As Object, e As System.EventArgs) As Boolean
    }
}