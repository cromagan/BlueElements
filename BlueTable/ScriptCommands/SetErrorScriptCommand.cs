// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Kann nur im Skript "Formular vorbereiten" benutzt werden.
/// Die hier angegebenen Variablen müssen einer Spalte der Tabelle entsprechen.
/// Diese werden dann als 'fehlerhaft' in der Tabellen-Zeile markiert, mit der hier
/// angegebenen Nachricht. Zeilenumbrüche und '|' werden automatisch aus der Nachricht entfernt.
/// Statt einer Variablen ist auch ein Listen-Zugriff möglich (z. B. Test[0] markiert die Spalte Test0).
/// </summary>
public class SetErrorScriptCommand : TableGenericScriptCommand {

    #region Fields

    public static readonly ScriptCommand Method = new SetErrorScriptCommand();

    #endregion

    #region Properties

    public override List<List<string>> Args => [StringVal, [ScriptVariable.Any_Variable]];
    public override string Command => "seterror";

    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.Special;

    public override string Syntax => "SetError(Nachricht, Column1, Column2, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        if (varCol.GetByKey("ErrorColumns") is not ListOfStringsScriptVariable vls) { return DoItFeedback.InternerFehler(); }

        // Der Rohtext wird benötigt, um Spalten auch über Listen-Zugriffe (z. B. Test[0] → Spalte Test0) zu ermitteln.
        var attributes = SplitAttributeToString(infos.AttributText);
        if (attributes is not { Count: > 1 }) { return new DoItFeedback("SetError benötigt eine Nachricht und mindestens eine Spalte.", true); }

        var msg = SplitAttributeToVars(Command, varCol, attributes[0], [StringVal], LastArgMinCountTypeScriptCommand.ExactlyOnce, scp);
        if (msg.Failed) { return DoItFeedback.AttributFehler(msg); }

        var message = msg.ValueStringGet(0).Replace("|", "").Replace("\r", "").Replace("\n", "");

        var l = vls.ValueList;

        for (var z = 1; z < attributes.Count; z++) {
            var column = Column(scp, varCol, attributes[z]);
            if (column is not { IsDisposed: false }) { return new DoItFeedback("Spalte nicht gefunden: " + attributes[z], true); }
            l.Add(column.KeyName.ToUpperInvariant() + "|" + message);
        }

        vls.ValueList = l.SortedDistinctList();

        return DoItFeedback.Null();
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        // Dummy überschreibung.
        // Wird niemals aufgerufen, weil die andere DoIt Routine überschrieben wurde.

        Develop.DebugPrint_NichtImplementiert(true);
        return DoItFeedback.Falsch();
    }

    /// <summary>
    /// Ermittelt die Spalte aus dem Roh-Attribut: eine Skript-Variable oder ein Listen-Zugriff
    /// Name[Index], wobei die Spalte NameIndex gesucht wird (z. B. Test[0] → Spalte Test0).
    /// </summary>
    private static ColumnItem? Column(ScriptProperties scp, VariableCollection varCol, string attribut) {
        attribut = attribut.Trim();

        if (attribut.EndsWith(']')) {
            var posb = attribut.IndexOf('[');
            if (posb > 0) {
                var idx = GetVariableByParsing(attribut[(posb + 1)..^1], varCol, scp);
                if (idx.Failed || idx.ReturnValue is null) { return null; }
                return MyTable(scp)?.Column[attribut[..posb] + idx.ReturnValue.ValueForReplace];
            }
        }

        var v = varCol.GetByKey(attribut);
        return v is null ? null : Column(scp, new SplittedAttributesFeedback([v]), 0);
    }

    #endregion
}