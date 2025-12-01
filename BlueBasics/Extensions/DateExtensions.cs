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

using System;
using System.Globalization;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static string ToString1(this DateTime value) => value.ToString("yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

    //Used: Only BZL
    public static string ToString3(this DateTime value) => value.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);

    public static string ToString4(this DateTime value) => value.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);

    /// <summary>
    /// Deutsche normale Zeitanzeige mit Sekunden
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToString5(this DateTime value) => value.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);

    public static string ToString6(this DateTime value) => value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

    /// <summary>
    /// Deutsche normale Zeitanzeige mit Millisekunden
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToString7(this DateTime value) => value.ToString("dd.MM.yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture);

    public static string ToString9(this DateTime value) => value.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

    #endregion
}