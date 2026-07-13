// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;

internal class Method_DeleteFile : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Plain, VariableListString.ShortName_Plain]];
    public override string Command => "deletefile";
    public override string Description => "Löscht die Datei aus dem Dateisystem. Gibt TRUE zurück, wenn die Datei nicht (mehr) existiert.";

    public override MethodType MethodLevel => MethodType.LongTime;

    public override string Returns => VariableBool.ShortName_Variable;

    public override string Syntax => "DeleteFile(Filename)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var files = new List<string>();

        foreach (var thisAtt in attvar.Attributes) {
            if (thisAtt is VariableString vs1) { files.Add(vs1.ValueString); }
            if (thisAtt is VariableListString vl1) { files.AddRange(vl1.ValueList); }
        }
        files = files.SortedDistinctList();

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }

        foreach (var filn in files) {
            if (!filn.IsFormat(FormatHolder_FilepathAndName.Instance)) {
                return new DoItFeedback("Dateinamen-Fehler!", true);
            }

            if (IO.FileExists(filn)) {
                try {
                    if (!IO.DeleteFile(filn, 120)) { return new DoItFeedback("Fehler beim Löschen: " + filn, true); }
                } catch {
                    return new DoItFeedback("Fehler beim Löschen: " + filn, true);
                }
            }
        }

        return DoItFeedback.Wahr();
    }

    #endregion
}