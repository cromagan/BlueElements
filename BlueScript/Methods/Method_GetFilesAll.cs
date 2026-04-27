// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueBasics.ClassesStatic.IO;

namespace BlueScript.Methods;


internal class Method_GetFilesAll : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "getfilesall";
    public override string Description => "Gibt alle Dateien im angegebenen Verzeichnis inkl. Unterverzeichnisse zurück. Komplett, mit Pfad und Suffix. Pfad muss mit \\ enden. Suffix im Format *.png";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListString.ShortName_Plain;
    public override string Syntax => "GetFilesAll(Path, Suffix)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var pf = attvar.ValueStringGet(0);

        if (!DirectoryExists(pf)) {
            return new DoItFeedback("Verzeichnis existiert nicht", true, ld);
        }

        try {
            return new DoItFeedback(GetFiles(pf, attvar.ValueStringGet(1), System.IO.SearchOption.AllDirectories));
        } catch {
            return DoItFeedback.InternerFehler(ld);
        }
    }

    #endregion
}