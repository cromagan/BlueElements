// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;

namespace BlueBasics.Enums;

[Flags]
public enum ScriptEventTypes {
    Ohne_Auslöser = 0,

    InitialValues = 1,
    value_changed = 2,

    /// <summary>
    /// Berechnet die Fehler, Variablen für das Formular und Nichtspeicherbare Spalten
    /// </summary>
    prepare_formula = 4,

    value_changed_extra_thread = 8,

    //loaded = 16,
    export = 32,

    //clipboard_changed = 64,

    //value_changed_large = 128,
    row_deleting = 256

    //correct_changed = 512
}