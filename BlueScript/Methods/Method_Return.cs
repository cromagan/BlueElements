// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_Return : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "return";
    public override string Description => "Beendet das Skript oder Unterskript ohne Fehler und setzt den Rückgabewert für Call-Routinen.";
    public override string StartSequence => string.Empty;

    public override string Syntax => "Return \"ReturnValue\";";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) =>
        new(false, false, true, string.Empty, attvar.Attributes[0]);

    #endregion
}