// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Gibt den Befehl an Windows ab.
/// Versucht das Beste daraus zu machen,
/// </summary>
internal class ExecuteScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];

    public override string Command => "execute";

    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.GUI;

    public override string Syntax => "Execute(Command, Attribut);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        IO.ExecuteFile(attvar.ValueStringGet(0), attvar.ValueStringGet(1), false, false);

        return DoItFeedback.Null();
    }

    #endregion
}
