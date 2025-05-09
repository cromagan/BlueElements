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

namespace BlueControls;

public class ScreenData {

    #region Fields

    public Bitmap? Area;
    public Point Point1;
    public Point Point2;
    public Bitmap? Screen;

    #endregion

    #region Methods

    public Rectangle AreaRectangle() =>
        new(Math.Min(Point1.X, Point2.X), Math.Min(Point1.Y, Point2.Y),
            Math.Max(Point1.X - Point2.X, Point2.X - Point1.X) + 1,
            Math.Max(Point1.Y - Point2.Y, Point2.Y - Point1.Y) + 1);

    #endregion
}