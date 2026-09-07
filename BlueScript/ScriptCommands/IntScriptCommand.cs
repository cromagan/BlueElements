// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;


/// <summary>
/// Schneidet Nachkommastellen ab. Um einen Text in einen Zahlenwert zu verwandeln, ist der Befehl Number() zu benutzen.
/// </summary>
internal class IntScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [FloatVal];

    public override string Command => "int";

    public override bool MustUseReturnValue => true;
    public override string Returns => DoubleScriptVariable.ShortName_Plain;
    public override string Syntax => "Int(Number)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(attvar.ValueIntGet(0));

    #endregion
}
