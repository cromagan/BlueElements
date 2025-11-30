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

    public static int CanvasToControl(this float value, float zoom, float offset) => (int)Math.Round(value * zoom + offset, 0, MidpointRounding.AwayFromZero);

    public static int CanvasToControl(this float value, float zoom) => (int)Math.Round(value * zoom, 0, MidpointRounding.AwayFromZero);

    public static string ToStringFloat(this double value) => value.ToString(CultureInfo.InvariantCulture);

    public static string ToStringFloat1(this float value) => Math.Round(value, 1, MidpointRounding.AwayFromZero).ToString("0.#", CultureInfo.InvariantCulture);

    public static string ToStringFloat1(this double value) => Math.Round(value, 1, MidpointRounding.AwayFromZero).ToString("0.#", CultureInfo.InvariantCulture);

    public static string ToStringFloat10_3(this double value) => Math.Round(value, 3, MidpointRounding.AwayFromZero).ToString("0000000000.###", CultureInfo.InvariantCulture);

    public static string ToStringFloat2(this float value) => Math.Round(value, 2, MidpointRounding.AwayFromZero).ToString("0.##", CultureInfo.InvariantCulture);

    public static string ToStringFloat2(this double value) => Math.Round(value, 2, MidpointRounding.AwayFromZero).ToString("0.##", CultureInfo.InvariantCulture);

    public static string ToStringFloat3(this float value) => Math.Round(value, 3, MidpointRounding.AwayFromZero).ToString("0.###", CultureInfo.InvariantCulture);

    public static string ToStringFloat3(this double value) => Math.Round(value, 3, MidpointRounding.AwayFromZero).ToString("0.###", CultureInfo.InvariantCulture);

    public static string ToStringFloat5(this float value) => Math.Round(value, 5, MidpointRounding.AwayFromZero).ToString("0.#####", CultureInfo.InvariantCulture);

    public static string ToStringFloat5(this double value) => Math.Round(value, 5, MidpointRounding.AwayFromZero).ToString("0.#####", CultureInfo.InvariantCulture);

    #endregion
}