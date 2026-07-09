// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;

public class Method_If : Method {

    #region Fields

    public static readonly List<string> OderOder = ["||"];
    public static readonly List<string> UndUnd = ["&&"];

    /// <summary>
    /// Vergleichsopeatoren in der richtigen Rang-Reihenfolge
    /// https://de.wikipedia.org/wiki/Operatorrangfolge
    /// </summary>
    public static readonly List<string> VergleichsOperatoren = ["==", "!=", ">=", "<=", "<", ">", "!", "&&", "||"];

    #endregion

    #region Properties

    public override List<List<string>> Args => [BoolVal];
    public override string Command => "if";
    public override string Description => "Nur wenn der Wert in der Klammer TRUE ist, wird der nachfolgende Codeblock ausgeführt. Es werden IMMER alle Vergleichsoperatoren aufgelöst. Deswegen sind Verschachtelungen mit Vorsicht zu verwenden - z.B. mit einem Exists-Befehl.";
    public override bool GetCodeBlockAfter => true;
    public override string Syntax => "if (true) { Code zum Ausführen }";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        //var m = new List<Method>();
        //foreach (var thism in scp.AllowedMethods) {
        //    if (!thism.MethodType.HasFlag(MethodType.SpecialVariables)) {
        //        m.Add(thism);
        //    }
        //}

        var scpt = new ScriptProperties(scp, scp.AllowedMethods, scp.Stufe + 1, scp.Chain);

        var attvar = SplitAttributeToVars(Command, varCol, infos.AttributText, Args, LastArgMinCount, infos.LogData, scpt);
        if (attvar.Failed) { return new DoItFeedback("Fehler innerhalb der runden Klammern des If-Befehls: " + attvar.FailedReason, true, infos.LogData); }

        if (attvar.ValueBoolGet(0)) {
            var scx = Method_CallByFilename.CallSub(varCol, scp, infos.CodeBlockAfterText, infos.LogData.Line - 1, infos.LogData.Subname, null, null, "If", infos.LogData);
            return scx; // If muss die Breaks und Endsripts erhalten!
        }

        return DoItFeedback.Null();
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        // Dummy überschreibung.
        // Wird niemals aufgerufen, weil die andere DoIt Routine überschrieben wurde.

        Develop.DebugPrint_NichtImplementiert(true);
        return DoItFeedback.Falsch();
    }

    #endregion
}