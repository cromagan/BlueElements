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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static bool Contains(this IEnumerable<string> collection, string searchKeyword, bool caseSensitive) => caseSensitive
            ? collection.Contains(searchKeyword)
            : collection.Any(item => string.Equals(item, searchKeyword, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Verbindet die Collection mit Trennzeichen und doppelte Trennzeichen am Ende werden nicht abgeschnitten
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="joinChar"></param>
    /// <returns></returns>
    public static string JoinWith(this IEnumerable<string>? collection, string joinChar) => collection == null ? string.Empty : string.Join(joinChar, collection);

    /// <summary>
    /// Verbindet die Collection mit \r und doppelte \r am Ende werden nicht abgeschnitten.
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static string JoinWithCr(this IEnumerable<string>? collection) => JoinWith(collection, "\r");

    /// <summary>
    /// Gibt einen String zurück, der alle Elemet der Collection mittels einem Zeilenumbruch zusammenfügt.
    /// Alle Elemente, die nach erreichen der Maximallänge noch übrig sind, werden verworfen.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="maxlength"></param>
    /// <returns></returns>
    public static string JoinWithCr(this IEnumerable<string> collection, long maxlength) {
        StringBuilder sb = new();

        foreach (var thisitem in collection) {
            if (sb.Length + thisitem.Length <= maxlength) {
                if (sb.Length > 0) { sb.Append("\r"); }
                sb.Append(thisitem);
            } else {
                return sb.ToString();
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Gib TRUE zurück, wenn ein Item erfolgreich entfernt wurde.
    /// Gibt False zurück, wenn kein Item entfernt wurde - weil es auf Fehler lief oder gar nicht vorhanden war.
    /// </summary>
    /// <param name="iC"></param>
    /// <param name="remove"></param>
    /// <returns></returns>
    public static bool RemoveRange(this List<string> iC, List<string> remove) {
        //Obacht! nicht für Obersvalbe Collections benutzen.
        // ist eine Vererbung / Überschreibung von Remove vorhnanden, wird diese hiermit auser kraft gesetzt!

        var did = false;
        foreach (var thisItem in remove) {
            if (iC.Remove(thisItem)) { did = true; }
        }
        return did;
    }

    #endregion
}