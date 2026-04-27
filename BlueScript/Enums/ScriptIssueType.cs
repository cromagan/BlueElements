// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Enums;

public enum ScriptIssueType {
    ohne = 0,
    FalscherDatentyp = 1,
    AttributAnzahl = 2,

    /// <summary>
    /// Es ist eine Rechenformel / BoolscheOperation vorhanden und diese ist Fehgeschlagen
    /// </summary>
    BerechnungFehlgeschlagen = 3,

    VariableNichtGefunden = 4,
    VariableErwartet = 5
}