// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueBasics.ClassesStatic.IO;

namespace BlueScript.Methods;

internal class Method_GetFiles : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "getfiles";
    public override string Description => "Gibt alle Dateien im angegebenen Verzeichnis zurück - ohne die Unterverzeichnisse. Komplett, mit Pfad und Suffix. Pfad muss mit \\ enden. Suffix im Format *.png";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListString.ShortName_Plain;
    public override string Syntax => "GetFiles(Path, Suffix)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var pf = attvar.ValueStringGet(0);

        if (!DirectoryExists(pf)) {
            return new DoItFeedback("Verzeichnis existiert nicht", true);
        }

        try {
            return new DoItFeedback(GetFiles(pf, attvar.ValueStringGet(1), System.IO.SearchOption.TopDirectoryOnly));
        } catch {
            return DoItFeedback.InternerFehler();
        }
    }

    #endregion
}