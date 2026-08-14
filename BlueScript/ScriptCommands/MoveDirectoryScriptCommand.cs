// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueBasics.ClassesStatic.IO;

namespace BlueScript.ScriptCommands;

internal class MoveDirectoryScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "movedirectory";
    public override string Description => "Verschiebt einen Ordner.";

    public override string Returns => BoolScriptVariable.ShortName_Plain;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "MoveDirectoryScriptCommand(SourceCompleteName, DestinationCompleteName)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var sop = attvar.ValueStringGet(0);
        if (!DirectoryExists(sop)) { return new DoItFeedback("Quell-Verzeichnis existiert nicht.", true); }
        var dep = attvar.ValueStringGet(1);

        if (DirectoryExists(dep)) { return DoItFeedback.Falsch(); }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }

        if (!DirectoryMove(sop, dep, false)) {
            return DoItFeedback.Falsch();
        }

        return DoItFeedback.Wahr();
    }

    #endregion
}