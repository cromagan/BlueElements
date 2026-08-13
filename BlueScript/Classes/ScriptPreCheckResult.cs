// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Classes;

/// <summary>
/// Ergebnis einer Skript-Vorabprüfung. Enthält die erkannten
/// Variablennamen, Protokollnachrichten (nicht blockierend) und
/// Syntax-Fehler (blockierend).
/// </summary>
public class ScriptPreCheckResult {

    #region Properties

    public bool HasSyntaxErrors => SyntaxErrors.Count > 0;

    /// <summary>
    /// Nicht-blockierende Hinweise, z.B. Var-Deklarationen, deren Wert
    /// nicht aufgelöst werden konnte. Variablen dürfen keinen Fehler
    /// auslösen, da sie zur Laufzeit erzeugt werden können.
    /// </summary>
    public List<string> Protocol { get; } = [];

    /// <summary>
    /// Blockierende Syntax-Fehler. Methoden, die syntaktisch falsch
    /// aufgerufen wurden oder Befehle, die überhaupt nicht geparsed
    /// werden können.
    /// </summary>
    public List<string> SyntaxErrors { get; } = [];

    public List<string> VariableNames { get; } = [];

    #endregion
}