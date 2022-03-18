// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueBasics.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlueBasics {

    public static partial class Extensions {

        #region Methods

        public static bool Contains(this ICollection<string> collection, string searchKeyword, bool caseSensitive) {
            if (caseSensitive) { Develop.DebugPrint(FehlerArt.Fehler, "CaseSensitive = True"); }
            return collection.Any(item => string.Equals(item, searchKeyword, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Verbindet die Collection mit Trennzeichen und doppelte Trennzeichen am Ende werden nicht abgeschnitten
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="joinChar"></param>
        /// <returns></returns>
        public static string JoinWith(this ICollection<string> collection, string joinChar) => string.Join(joinChar, collection.ToArray());

        /// <summary>
        /// Verbindet die Collection mit \r und doppelte \r am Ende werden nicht abgeschnitten.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static string JoinWithCr(this ICollection<string> collection) => collection == null || collection.Count == 0 ? string.Empty : collection.JoinWith("\r");

        /// <summary>
        /// Gibt einen String zurück, der alle Elemet der Collection mittels einem Zeilenumbruch zusammenfügt.
        /// Alle Elemente, die nach erreichen der Maximallänge noch übrig sind, werden verworfen.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="maxlenght"></param>
        /// <returns></returns>
        public static string JoinWithCr(this ICollection<string> collection, long maxlenght) {
            StringBuilder sb = new();

            foreach (var thisitem in collection) {
                if (sb.Length + thisitem.Length <= maxlenght) {
                    if (sb.Length > 0) { sb.Append("\r"); }
                    sb.Append(thisitem);
                } else {
                    return sb.ToString();
                }
            }
            return sb.ToString();
        }

        // .TrimEnd(JoinChar);

        public static void RemoveRange<Tt>(this ICollection<Tt> iC, List<Tt> remove) {
            foreach (var thisItem in remove) {
                iC.Remove(thisItem);
            }
        }

        #endregion
    }
}