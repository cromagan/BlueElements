// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Tests;

/// <summary>
/// Erwartete Zellwerte einer Zeile für die Inhaltsprüfung nach dem Laden.
/// </summary>
public sealed class ExpectedRow(string name, string menge, string notiz, string chunk) {

    #region Properties

    public string Name { get; } = name;

    public string Menge { get; set; } = menge;

    public string Notiz { get; set; } = notiz;

    public string Chunk { get; } = chunk;

    #endregion
}
