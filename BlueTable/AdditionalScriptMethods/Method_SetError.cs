// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.AdditionalScriptMethods;

public class Method_SetError : Method_TableGeneric {

    #region Fields

    public static readonly Method Method = new Method_SetError();

    #endregion

    #region Properties

    public override List<List<string>> Args => [StringVal, [Variable.Any_Variable]];
    public override string Command => "seterror";

    public override string Description => "Kann nur im Skript \"Formular vorbereiten\" benutzt werden.\r\n" +
                                          "Die hier angegebenen Variablen müssen einer Spalte der Tabelle entsprechen.\r\n" +
                                          "Diese werden dann als 'fehlerhaft' in der Tabellen-Zeile markiert, mit der hier\r\n" +
                                          "angegebenen Nachricht.";

    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinOnce;
    public override MethodType MethodLevel => MethodType.Special;


    public override string Syntax => "SetError(Nachricht, Column1, Colum2, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (varCol.GetByKey("ErrorColumns") is not VariableListString vls) { return DoItFeedback.InternerFehler(ld); }
        var l = vls.ValueList;

        for (var z = 1; z < attvar.Attributes.Count; z++) {
            var column = Column(scp, attvar, z);
            if (column is not { IsDisposed: false }) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.Name(z), true, ld); }
            l.Add(column.KeyName.ToUpperInvariant() + "|" + attvar.ValueStringGet(0));
        }

        vls.ValueList = l.SortedDistinctList();

        return DoItFeedback.Null();
    }

    #endregion
}