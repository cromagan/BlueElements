// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Globalization;

[assembly: DoNotParallelize]

namespace BlueTable.Tests;

/// <summary>
/// Initialisiert die Test-Umgebung einmal pro Assembly: Debug-Service, Benutzer und Abbruch-Regeln.
/// </summary>
[TestClass]
public static class TestSetup {

    #region Properties

    /// <summary>
    /// Eindeutiger Lauf-Kenner; Tabellen-Basisnamen tragen ihn, damit sich Läufe im Temp-Ordner nicht in die Quere kommen.
    /// </summary>
    public static string RunId { get; private set; } = string.Empty;

    #endregion

    #region Methods

    [AssemblyInitialize]
    public static void AssemblyInit() {
        RunId = DateTime.UtcNow.ToString("HHmmssfff", CultureInfo.InvariantCulture);
        Develop.StartService();
        Develop.TraceLogging_End();
        Generic.UserName = "RTTEST";
        RowCollection.AllowProcessingAborts = false;

        // Edit-Sperre aus: Der Test lädt und speichert sofort in derselben Datei
        // weiter; die Multi-User-Sperre würde Row-Chunks als fremd einstufen.
        TableChunk.EditLockSeconds = 0;
    }

    #endregion
}
