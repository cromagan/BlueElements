// Licensed under AGPL-3.0; see License.md for disclaimer and details.

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

    /// <summary>
    /// Zeitanzeige, bestens geeignet zum Sortieren.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToString9(this DateTime value) => value.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

    #endregion
}