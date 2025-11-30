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
using System.Globalization;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static int CanvasToControl(this int value, float zoom, float offset) => (int)Math.Round(value * zoom + offset, 0, MidpointRounding.AwayFromZero);

    public static int CanvasToControl(this int value, float zoom) => (int)Math.Round(value * zoom, 0, MidpointRounding.AwayFromZero);

    public static float ControlToCanvas(this int value, float zoom, float offset) => (value - offset) / zoom;

    public static float ControlToCanvas(this int value, float zoom) => value / zoom;

    public static string ToStringInt1(this int value) => value.ToString("0", CultureInfo.InvariantCulture);

    public static string ToStringInt10(this int value) => value.ToString("0000000000", CultureInfo.InvariantCulture);

    public static string ToStringInt2(this int value) => value.ToString("00", CultureInfo.InvariantCulture);

    public static string ToStringInt3(this int value) => value.ToString("000", CultureInfo.InvariantCulture);

    public static string ToStringInt4(this int value) => value.ToString("0000", CultureInfo.InvariantCulture);

    public static string ToStringInt5(this int value) => value.ToString("00000", CultureInfo.InvariantCulture);

    public static string ToStringInt7(this int value) => value.ToString("0000000", CultureInfo.InvariantCulture);

    #endregion
}