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

using System;
using System.Drawing;

namespace BlueControls.EventArgs;

public class MouseEventArgs1_1DownAndCurrent : System.EventArgs {

    #region Constructors

    public MouseEventArgs1_1DownAndCurrent(MouseEventArgs1_1 mouseDown, MouseEventArgs1_1 current) : base() {
        MouseDown = mouseDown;
        Current = current;
    }

    #endregion

    #region Properties

    public MouseEventArgs1_1 Current { get; }
    public MouseEventArgs1_1 MouseDown { get; }

    #endregion

    #region Methods

    public Rectangle TrimmedRectangle() => new(Math.Min(MouseDown.TrimmedX, Current.TrimmedX), Math.Min(MouseDown.TrimmedY, Current.TrimmedY), Math.Abs(MouseDown.TrimmedX - Current.TrimmedX) + 1, Math.Abs(MouseDown.TrimmedY - Current.TrimmedY) + 1);

    #endregion
}