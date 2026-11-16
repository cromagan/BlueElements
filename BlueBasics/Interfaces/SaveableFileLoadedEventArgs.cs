// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.Interfaces;

/// <summary>
/// Event-Argumente für SaveableFile.Loaded: Meldet den Abschluss eines Ladevorgangs.
/// </summary>
/// <param name="isFirst">True beim initialen Laden.</param>
public class SaveableFileLoadedEventArgs(bool isFirst) : System.EventArgs {

    #region Properties

    /// <summary>
    /// True beim initialen Laden.
    /// </summary>
    public bool IsFirst { get; } = isFirst;

    #endregion
}
