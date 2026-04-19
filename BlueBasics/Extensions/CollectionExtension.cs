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

using System.Collections.Generic;
using System.Text;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    /// <summary>
    /// Gibt einen String zurück, der alle Elemet der Collection mittels einem Zeilenumbruch zusammenfügt.
    /// Alle Elemente, die nach erreichen der Maximallänge noch übrig sind, werden verworfen.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="maxlength"></param>
    /// <returns></returns>
    public static string JoinWithCr(this IEnumerable<string> collection, long maxlength) {
        var sb = new StringBuilder();

        foreach (var thisitem in collection) {
            if (sb.Length + thisitem.Length <= maxlength) {
                if (sb.Length > 0) { sb.Append('\r'); }
                sb.Append(thisitem);
            } else {
                return sb.ToString();
            }
        }
        return sb.ToString();
    }

    #endregion
}