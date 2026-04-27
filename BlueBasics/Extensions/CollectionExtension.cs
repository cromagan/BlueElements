// Licensed under AGPL-3.0; see License.md for disclaimer and details.

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