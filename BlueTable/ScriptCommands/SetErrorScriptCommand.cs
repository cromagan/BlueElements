// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

public class SetErrorScriptCommand : TableGenericScriptCommand {

    #region Fields

    public static readonly ScriptCommand Method = new SetErrorScriptCommand();

    #endregion

    #region Properties

    public override List<List<string>> Args => [StringVal, [ScriptVariable.Any_Variable]];
    public override string Command => "seterror";

    public override string Description => "Kann nur im Skript \"Formular vorbereiten\" benutzt werden.\r\n" +
                                          "Die hier angegebenen Variablen müssen einer Spalte der Tabelle entsprechen.\r\n" +
                                          "Diese werden dann als 'fehlerhaft' in der Tabellen-Zeile markiert, mit der hier\r\n" +
                                          "angegebenen Nachricht. Die Nachricht darf keine Zeilenumbrüche und kein '|' enthalten.";

    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.Special;

    public override string Syntax => "SetErrorScriptCommand(Nachricht, Column1, Colum2, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (varCol.GetByKey("ErrorColumns") is not ListOfStringsScriptVariable vls) { return DoItFeedback.InternerFehler(); }

        var message = attvar.ValueStringGet(0);

        if (message.Contains('|') || message.Contains('\r') || message.Contains('\n')) {
            return new DoItFeedback("Die Nachricht enthält verbotene Zeichen (Zeilenumbruch oder '|').", true);
        }

        var l = vls.ValueList;

        for (var z = 1; z < attvar.Attributes.Count; z++) {
            var column = Column(scp, attvar, z);
            if (column is not { IsDisposed: false }) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.Name(z), true); }
            l.Add(column.KeyName.ToUpperInvariant() + "|" + message);
        }

        vls.ValueList = l.SortedDistinctList();

        return DoItFeedback.Null();
    }

    #endregion
}