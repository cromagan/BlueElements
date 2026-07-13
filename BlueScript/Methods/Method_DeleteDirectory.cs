// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;

internal class Method_DeleteDirectory : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "deletedirectory";
    public override string Description => "Löscht die Verzeichnis und dessn Inhalt aus dem Dateisystem. Gibt TRUE zurück, wenn das Verzeichnis nicht (mehr) existiert.";

    public override MethodType MethodLevel => MethodType.LongTime;

    public override string Returns => VariableBool.ShortName_Variable;

    public override string Syntax => "DeleteDirectory(Dir)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var filn = attvar.ValueStringGet(0);

        if (!filn.IsFormat(FormatHolder_Filepath.Instance)) { return new DoItFeedback("Dateinamen-Fehler!", true); }

        if (!IO.DirectoryExists(filn)) {
            return DoItFeedback.Wahr();
        }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }

        try {
            return new DoItFeedback(IO.DeleteDir(filn, false));
        } catch {
            return new DoItFeedback("Fehler beim Löschen: " + filn, true);
        }
    }

    #endregion
}