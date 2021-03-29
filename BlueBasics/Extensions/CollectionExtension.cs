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

using BlueBasics.Enums;
using System.Collections.Generic;
using System.Linq;

namespace BlueBasics {
    public static partial class Extensions {
        public static bool Contains(this ICollection<string> iC, string searchKeyword, bool caseSensitive) {
            if (caseSensitive) { Develop.DebugPrint(enFehlerArt.Fehler, "CaseSensitive = True"); }

            return iC.Any(item => string.Equals(item, searchKeyword, System.StringComparison.OrdinalIgnoreCase));
        }

        public static string JoinWithCr(this ICollection<string> iC) {
            if (iC == null || iC.Count == 0) { return string.Empty; }

            return iC.JoinWith("\r");
        }

        public static string JoinWith(this ICollection<string> iC, string joinChar) {
            return string.Join(joinChar, iC.ToArray()); // .TrimEnd(JoinChar);
        }

        public static void RemoveRange<t>(this ICollection<t> iC, List<t> remove) {
            foreach (var thisItem in remove) {
                iC.Remove(thisItem);
            }
        }
    }
}