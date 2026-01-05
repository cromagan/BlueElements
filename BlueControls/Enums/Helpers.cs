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

using System;

namespace BlueControls.Enums;

[Flags]
public enum Helpers {
    // Sollten die Rountnen benötigt werden, nach
    // ZoomPicWithPoints.DrawHelpers
    // verschieben

    None = 0,
    SmallCircle_unused = 1 << 0,   // 1
    SymetricalHorizontal = 1 << 1,   // 2
    SymetricalVertical_unused = 1 << 2,   // 4
    MouseDownPoint = 1 << 3,   // 8
    HorizontalLine = 1 << 4,   // 16
    VerticalLine = 1 << 5,   // 32
    DrawToPoint_unused = 1 << 6,   // 64
    FilledRectancle = 1 << 7,   // 128
    PointNames = 1 << 8,   // 256
    Magnifier = 1 << 9,   // 512
    DrawRectangle = 1 << 10,  // 1024
    Draw20x10 = 1 << 11,  // 2048
}