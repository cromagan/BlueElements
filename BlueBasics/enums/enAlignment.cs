#region BlueElements - a collection of useful tools, database and controls
// Authors:
// Christian Peter
//
// Copyright (c) 2020 Christian Peter
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
#endregion

using System;

namespace BlueBasics.Enums {
    [Flags]
    public enum enAlignment {
        // Undefiniert = CByte(TextFormatFlags.none)
        Left = System.Windows.Forms.TextFormatFlags.Left,
        Right = System.Windows.Forms.TextFormatFlags.Right,

        HorizontalCenter = System.Windows.Forms.TextFormatFlags.HorizontalCenter,

        Top = System.Windows.Forms.TextFormatFlags.Top,
        Bottom = System.Windows.Forms.TextFormatFlags.Bottom,

        VerticalCenter = System.Windows.Forms.TextFormatFlags.VerticalCenter,

        // DehnenLR = 64
        // DehnenOU = 128

        // DehnenLROU = DehnenLR Or DehnenOU

        Top_Left = Left | Top,
        Top_HorizontalCenter = HorizontalCenter | Top,
        Top_Right = Right | Top,

        Bottom_Left = Left | Bottom,
        Bottom_HorizontalCenter = HorizontalCenter | Bottom,
        Bottom_Right = Right | Bottom,

        VerticalCenter_Left = Left | VerticalCenter,
        Horizontal_Vertical_Center = HorizontalCenter | VerticalCenter,
        VerticalCenter_Right = Right | VerticalCenter
    }
}
