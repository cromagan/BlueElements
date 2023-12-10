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

using System;
using System.Windows.Forms;

namespace BlueBasics.Enums;

[Flags]
public enum Alignment {

    // Undefiniert = CByte(TextFormatFlags.none)
    Left = TextFormatFlags.Left,

    Right = TextFormatFlags.Right,
    HorizontalCenter = TextFormatFlags.HorizontalCenter,
    Top = TextFormatFlags.Top,
    Bottom = TextFormatFlags.Bottom,
    VerticalCenter = TextFormatFlags.VerticalCenter,

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