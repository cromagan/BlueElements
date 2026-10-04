// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using static BlueBasics.ClassesStatic.IO;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Gibt alle Dateien im angegebenen Verzeichnis inkl. Unterverzeichnisse zurück. Komplett, mit Pfad und Suffix. Pfad muss mit \ enden. Suffix im Format *.png
/// </summary>
internal class GetFilesAllScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[StringScriptVariable.ClassId], [StringScriptVariable.ClassId]];
    public override string Command => "getfilesall";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => ListOfStringsScriptVariable.ClassId;
    public override string Syntax => "GetFilesAll(Path, Suffix)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var pf = attvar.ValueStringGet(0);

        if (!DirectoryExists(pf)) {
            return new DoItFeedback("Verzeichnis existiert nicht", true);
        }

        try {
            return new DoItFeedback(GetFiles(pf, attvar.ValueStringGet(1), System.IO.SearchOption.AllDirectories));
        } catch {
            return DoItFeedback.InternerFehler();
        }
    }

    #endregion
}
