// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.ClassesStatic;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

internal class Method_Var : Method {

    #region Fields

    public const string CommandText = "var";

    #endregion

    #region Properties

    public override List<List<string>> Args => [[Variable.Any_Plain]];
    public override string Command => "var";
    public override List<string> Constants => [];
    public override string Description => "Erstellt eine neue Variable, der Typ wird automatisch bestimmt.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => string.Empty;
    public override string Syntax => "var VariablenName = Wert;";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        if (string.IsNullOrEmpty(infos.AttributText)) { return new DoItFeedback("Kein Text angekommen.", true, infos.LogData); }

        return VariablenBerechnung(varCol, infos.LogData, scp, infos.AttributText + ";", true);
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        // Dummy überschreibung.
        // Wird niemals aufgerufen, weil die andere DoIt Routine überschrieben wurde.

        Develop.DebugPrint_NichtImplementiert(true);
        return DoItFeedback.Falsch();
    }

    #endregion
}