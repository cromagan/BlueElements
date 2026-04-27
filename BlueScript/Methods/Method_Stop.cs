// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


public class Method_Stop : Method {

    #region Properties

    public override List<List<string>> Args => [];
    public override string Command => "stop";
    public override List<string> Constants => [];
    public override string Description => "Beendet die Ausführung im Testmodus.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => string.Empty;

    public override string Syntax => "Stop;";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (!scp.ProduktivPhase) { return new DoItFeedback("=== STOP ===", true, ld); }
        return DoItFeedback.Null();
    }

    #endregion
}