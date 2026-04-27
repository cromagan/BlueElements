// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_Clear : Method {

    #region Properties

    public override List<List<string>> Args => [ListStringVar];
    public override string Command => "clear";
    public override string Description => "Entfernt alle Einträge einer Liste";
    public override string Syntax => "Clear(VariableListe);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        if (attvar.ValueListStringSet(0, [], ld) is { } dif) { return dif; }

        return DoItFeedback.Null();
    }

    #endregion
}