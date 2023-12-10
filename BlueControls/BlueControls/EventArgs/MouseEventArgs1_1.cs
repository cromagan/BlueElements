// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using System.Windows.Forms;

namespace BlueControls.EventArgs;

public sealed class MouseEventArgs1_1 : MouseEventArgs {

    #region Constructors

    public MouseEventArgs1_1(MouseButtons button, int clicks, int x, int y, int delta, int trimmedX, int trimmedy, bool isinPic) : base(button, clicks, x, y, delta) {
        IsInPic = isinPic;
        TrimmedX = trimmedX;
        TrimmedY = trimmedy;
    }

    #endregion

    #region Properties

    public bool IsInPic { get; }
    public int TrimmedX { get; }
    public int TrimmedY { get; }

    #endregion
}