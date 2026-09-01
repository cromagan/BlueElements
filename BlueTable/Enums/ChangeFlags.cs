// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Enums;

/// <summary>
/// Flags, die steuern, wie eine Tabellen-Änderung angewendet wird (Undo, Events, Nachbereitung, Ausnahmen).
/// </summary>
[Flags]
public enum ChangeFlags : byte {

    /// <summary>
    /// Kein Sonderverhalten: Nur-Speicher-Schreibzugriff ohne Logging, Events oder Nachbereitung.
    /// </summary>
    None = 0,

    /// <summary>
    /// Fügt der Änderung einen Undo-Eintrag hinzu; ob das Logbuch geführt wird, entscheidet
    /// zusätzlich die Property Table.LogUndo. In InvalidateRowState steuert dieser Wert zusätzlich,
    /// ob über die Änderungs-Pipeline (Festplatte/Server) oder nur im Speicher geschrieben wird.
    /// </summary>
    LogUndo = 1,

    /// <summary>
    /// Feuert die Change-Events, damit die zugehörigen Steuerelemente reagieren und sich anpassen.
    /// Weitere Schritte (Logs, Nachbereitung) bleiben bewusst aus, z. B. wenn Daten nachgeladen
    /// werden und die Laderoutine diese selbst übernimmt.
    /// </summary>
    RaiseEvents = 2,

    /// <summary>
    /// Führt die Nachbereitung der Änderung aus: Systemspalten pflegen, Ersteller-Stempel setzen
    /// und Ereignis-Skripte (z. B. row_deleting) ausführen.
    /// </summary>
    PostProcess = 4,

    /// <summary>
    /// Umgeht die Freeze-Prüfung, z. B. beim Laden, Parsen oder bei Rollbacks.
    /// Wird nur von SetValueInternal und ColumnCollection.ExecuteCommand ausgewertet;
    /// die öffentlichen Einstiege ChangeData und CellSet sperren bei Freeze stets.
    /// </summary>
    IgnoreFreeze = 8,

    /// <summary>
    /// Unterdrückt die Systemspalten-Pflege (DoSystemColumns) für diesen Schreibzugriff.
    /// </summary>
    IgnoreSystemColumns = 16,

    /// <summary>
    /// Vollständiger benutzerinitiierter Schreibzugriff: Undo, Events und Nachbereitung.
    /// </summary>
    UserCommand = LogUndo | RaiseEvents | PostProcess,
}
