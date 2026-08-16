// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

internal class DeleteFileScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[StringScriptVariable.ShortName_Plain, ListOfStringsScriptVariable.ShortName_Plain]];
    public override string Command => "deletefile";
    public override string Description => "Löscht die Datei aus dem Dateisystem. Gibt TRUE zurück, wenn die Datei nicht (mehr) existiert.";

    public override string Returns => BoolScriptVariable.ShortName_Variable;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "DeleteFile(Filename)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var files = new List<string>();

        foreach (var thisAtt in attvar.Attributes) {
            if (thisAtt is StringScriptVariable vs1) { files.Add(vs1.ValueString); }
            if (thisAtt is ListOfStringsScriptVariable vl1) { files.AddRange(vl1.ValueList); }
        }
        files = files.SortedDistinctList();

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }

        foreach (var filn in files) {
            if (!filn.IsValidFilepathAndName()) {
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