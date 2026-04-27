// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;

namespace BlueTable.Enums;

[Flags]
public enum Reason : byte {

    /// <summary>
    /// Wichtigster Wert, wenn Änderungen permanent geloggt werden sollen.
    /// LogUndo wird das Logbuch aktualisiert und Werte fest in die Datenbank geschreieben.
    /// </summary>
    LogUndo = 1,

    /// <summary>
    /// Typische Anwendung: Daten werden nachgeladen.
    /// Die Zugehörigen Steuerelemente müssen reagieren und sich anpassen,
    /// aber ansonsten werden keine weiteren Schritte (Reparaturn, Logs) erwünscht,
    /// weil das die Laderoutine macht
    /// </summary>
    RaiseEvents = 2,

    /// <summary>
    /// Skripte ausführen, DateChanged, etc setzen.
    /// </summary>
    DoRepair = 4,

    IgnoreFreeze = 8,

    NoUndo_NoInvalidate = IgnoreFreeze,

    SetCommand = LogUndo | RaiseEvents | DoRepair,
}